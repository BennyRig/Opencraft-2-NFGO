﻿using System.Collections.Generic;
using Opencraft.Terrain;
using Opencraft.Terrain.Authoring;
using Opencraft.Terrain.Blocks;
using Opencraft.Terrain.Utilities;
using Opencraft.ThirdParty;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Physics;
using Unity.Profiling;
using Unity.Rendering;
using UnityEngine;

// Annoyingly this assembly directive must be outside the namespace. So we have to import Opencraft.Terrain namespace to itself...
[assembly: RegisterGenericJobType(typeof(SortJob<int3, Int3DistanceComparer>))]
namespace Opencraft.Terrain
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TerrainNeighborSystem))]
    [BurstCompile]
    // System that generates new terrain areas based on basic perlin noise
    public partial struct TerrainGenerationSystem : ISystem
    {
        private EntityQuery _newSpawnQuery;
        private ProfilerMarker markerTerrainGen;
        //private double lastUpdate;

        public void OnCreate(ref SystemState state)
        {
            // Wait for scene load/baking to occur before updates. 
            state.RequireForUpdate<TerrainSpawner>();
            _newSpawnQuery = SystemAPI.QueryBuilder().WithAll<NewSpawn>().Build();
            markerTerrainGen = new ProfilerMarker("TerrainGeneration");
            //lastUpdate = -1.0;
        }

        public void OnUpdate(ref SystemState state)
        {
            /*if (state.World.Time.ElapsedTime - lastUpdate < 1.0)
            {
                return;
            }
            lastUpdate = state.World.Time.ElapsedTime;*/
            
            // Disable the NewSpawn tag component from the areas we populated in the previous tick
            state.EntityManager.SetComponentEnabled<NewSpawn>(_newSpawnQuery, false);

            // Fetch the terrain spawner entity and component
            var terrainSpawner = SystemAPI.GetSingleton<TerrainSpawner>();
            Entity terrainSpawnerEntity = SystemAPI.GetSingletonEntity<TerrainSpawner>();

            // Fetch what chunks to spawn this tick
            var toSpawnbuffer = SystemAPI.GetBuffer<TerrainAreasToSpawn>(terrainSpawnerEntity);
            DynamicBuffer<int3> chunksToSpawnBuffer = toSpawnbuffer.Reinterpret<int3>();
            // If there is nothing to spawn, don't :)
            if (chunksToSpawnBuffer.Length == 0)
            {
                return;
            }
            markerTerrainGen.Begin();
            NativeArray<int3> chunksToSpawn = chunksToSpawnBuffer.AsNativeArray();
            // Sort the chunks to spawn so ones closer to 0,0 are first
            SortJob<int3, Int3DistanceComparer> sortJob = chunksToSpawn.SortJob<int3, Int3DistanceComparer>(new Int3DistanceComparer { });
            JobHandle sortHandle = sortJob.Schedule();

            // Spawn the terrain area entities
            state.EntityManager.Instantiate(terrainSpawner.TerrainArea,
                chunksToSpawn.Length > terrainSpawner.maxChunkSpawnsPerTick
                    ? terrainSpawner.maxChunkSpawnsPerTick
                    : chunksToSpawn.Length,
                Allocator.Temp);
            // Then populate them on worker threads
            JobHandle populateHandle = new PopulateTerrainAreas
            {
                chunksToSpawn = chunksToSpawn,
                noiseSeed = terrainSpawner.seed,
                blocksPerSide = terrainSpawner.blocksPerSide,
                YBounds = terrainSpawner.YBounds,
            }.ScheduleParallel(sortHandle);
            populateHandle.Complete();

            // Remove spawned areas from the toSpawn buffer
            if (chunksToSpawnBuffer.Length > terrainSpawner.maxChunkSpawnsPerTick)
                chunksToSpawnBuffer.RemoveRange(0, terrainSpawner.maxChunkSpawnsPerTick);
            else
                chunksToSpawnBuffer.Clear();
            markerTerrainGen.End();
        }
    }


    // Comparer for sorting locations by distance from zero
    public struct Int3DistanceComparer : IComparer<int3>
    {
        public int Compare(int3 a, int3 b)
        {
            int lSum = math.abs(a.x) + math.abs(a.y) + math.abs(a.z);
            int rSum = math.abs(b.x) + math.abs(b.y) + math.abs(b.z);
            if (lSum > rSum)
                return 1;
            if (lSum < rSum)
                return -1;
            return 0;
        }
    }

    [WithAll(typeof(NewSpawn))]
    [BurstCompile]
    // Job that fills the terrain area block buffer
    partial struct PopulateTerrainAreas : IJobEntity
    {
        public NativeArray<int3> chunksToSpawn;

        public int blocksPerSide;

        public int noiseSeed;
        public int2 YBounds; // x is sea level, y is sky level

        public void Execute([EntityIndexInQuery] int index, ref DynamicBuffer<TerrainBlocks> terrainBlocksBuffer,
            ref LocalTransform localTransform, ref TerrainArea terrainArea)
        {
            int3 chunk = chunksToSpawn[index];
            terrainArea.location = chunk; // terrain area grid position
            int areaX = chunk.x * blocksPerSide;
            int areaY = chunk.y * blocksPerSide;
            int areaZ = chunk.z * blocksPerSide;
            localTransform.Position = new float3(areaX, areaY, areaZ); // world space position

            var perLayer = blocksPerSide * blocksPerSide;
            var perArea = perLayer * blocksPerSide;
            terrainBlocksBuffer.Resize(perArea, NativeArrayOptions.UninitializedMemory);
            DynamicBuffer<BlockType> terrainBlocks = terrainBlocksBuffer.Reinterpret<BlockType>();
            for (int block = 0; block < perArea; block++)
            {
                var blockX = (block % blocksPerSide);
                var blockY = (block % perLayer) / blocksPerSide;
                var blockZ = (block / perLayer);
                var globalX = areaX + blockX;
                var globalY = areaY + blockY;
                var globalZ = areaZ + blockZ;
                if (globalY > YBounds.x) // Everything beneath sea level exists, so only check if above that
                {
                    float noise = FastPerlin.PerlinGetNoise(globalX, globalZ, noiseSeed);
                    var cutoff = YBounds.x + noise * (YBounds.y - YBounds.x);
                    if (globalY > cutoff)
                    {
                        terrainBlocks[block] = BlockType.Air;
                        continue;
                    }
                }

                if (blockX <= 8)
                {
                    terrainBlocks[block] = BlockType.Stone;
                }
                else
                {
                    terrainBlocks[block] = BlockType.Dirt;
                }
            }
        }
    }
}