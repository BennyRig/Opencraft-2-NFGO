using UnityEngine;
using Unity.Netcode;
using System.Collections;


    public class FlatMapGenerator_NFGO : NetworkBehaviour
{
    public enum benchmark_type
    {
        default_world,
        terainloading,
        terainmodification
    }

    public enum worldSizes
    {
        small,
        medium,
        large,
        extra_large
    }

    public benchmark_type benchmark_selection = benchmark_type.default_world;


    public GameObject Grass_blockPrefab;
    public GameObject Stone_blockPrefab;

    [Header("Default_world Config")]
    public int mapWidth;
    public int mapLength;
    public int mapHeight;



    [Header("Benchmark Config")]
    public worldSizes benchmark_worldSize = worldSizes.small;
    public int endTime;

    //peramiters for benchmark
    public float tick_interval_time = 1f; 
    private float next_Tick = 0; 
    public int blocks_per_tick = 10;
   

    private int cur_y = 4;
    private int cur_x = 0;
    private int cur_z = -1;

    private float cur_xOffset = 0f;
    private float cur_zOffset = 0f;
    

    public override void OnNetworkSpawn()
    {
        Debug.LogError("STARTING");
        if (IsServer) 
        {
            if (!Application.isEditor)
            {
                string[] args = System.Environment.GetCommandLineArgs();
                for(int i = 0; i < args.Length-1;i++)
                {
                    if (args[i] == "-setWorldSize"){
                        switch(args[i+1]){
                            case "small":
    	                        benchmark_worldSize = worldSizes.small;
                                benchmark_selection = benchmark_type.terainloading;
                                break;
                            case "medium":
                                benchmark_worldSize = worldSizes.medium;
                                benchmark_selection = benchmark_type.terainloading;
                                break;
                            case "large":
                                benchmark_worldSize = worldSizes.large;
                                benchmark_selection = benchmark_type.terainloading;
                                break;
                            case "extra_large":
                                benchmark_worldSize = worldSizes.extra_large;
                                benchmark_selection = benchmark_type.terainloading;
                                break;
                            default:
                                Debug.LogError("Missing world size argument");
                                break;
                        } 
                    }
                }
            }
            Debug.Log("entering switch");
            switch (benchmark_selection)
            {
                case benchmark_type.default_world:
                    GenerateFlatMap();
                    break;
                case benchmark_type.terainloading:
                    terainloading_benchmark();
                    break;
                case benchmark_type.terainmodification:
                    terainmodification_benchmark();
                    break; 

            }
        }
    }

    void Update(){
        if (IsServer && benchmark_selection == benchmark_type.terainmodification){
            if ( next_Tick <= Time.time)
            {
                Vector3 blockDimensions = Grass_blockPrefab.GetComponent<Renderer>().bounds.size;
                for (int i = 0; i < blocks_per_tick; i++){
                    if (cur_z  >= mapLength ){
                        cur_z =  0;
                        if(cur_x >= mapWidth){
                            cur_x = 0;
                            cur_y += 1;
                        }
                        else
                        {
                        cur_x += 1;  
                        }
                    }
                    else{
                        cur_z += 1; 
                    }
                    Vector3 position = new Vector3(cur_x * blockDimensions.x - cur_xOffset, cur_y * blockDimensions.y, cur_z * blockDimensions.z - cur_zOffset);
                    GameObject block = Instantiate(Grass_blockPrefab, position, Quaternion.identity);
                    NetworkObject blockNetworkObject = block.GetComponent<NetworkObject>();
                    blockNetworkObject.Spawn();
                }
                next_Tick = Time.time + tick_interval_time;
            }
          
        }
    }
    
    void GenerateFlatMap()
    {
        if (Grass_blockPrefab == null)
        {
            Debug.LogError("Block prefab is not assigned.");
            return;
        }

        Vector3 blockDimensions = Grass_blockPrefab.GetComponent<Renderer>().bounds.size;

        float xOffset = mapWidth * blockDimensions.x / 2f;
        float zOffset = mapLength * blockDimensions.z / 2f;
        mapHeight = mapHeight *(-1);
        Debug.Log(mapHeight);

        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapLength; z++)
            {
                for (int y = 0; y >= mapHeight ; y--)
                {

                    Vector3 position = new Vector3(x * blockDimensions.x - xOffset, y * blockDimensions.y, z * blockDimensions.z - zOffset);
                    GameObject block = Instantiate(Grass_blockPrefab, position, Quaternion.identity);
                    NetworkObject blockNetworkObject = block.GetComponent<NetworkObject>();
                    blockNetworkObject.Spawn();
                }
            }
        }
        
    }
    void set_world_size(){
        Debug.Log("seting size");
        switch(benchmark_worldSize){
            case worldSizes.small:
                //12,500 objects
                mapWidth  = 50;
                mapHeight = 5;
                mapLength = 50;
                Debug.Log("world size set to small");
                return;
            case worldSizes.medium :
                //50,000 objects 	
                mapWidth  = 100;
                mapHeight = 5;
                mapLength = 100;	
                Debug.Log("world size set to medium");
                return;
            case worldSizes.large :
                //100,000 objects 	
                mapWidth  = 100;
                mapHeight = 10;
                mapLength = 100;
                Debug.Log("world size set to large");
                return;
            case worldSizes.extra_large :
                //400,000 objects 
                mapWidth  = 200;
                mapHeight = 10;
                mapLength = 200;	
                Debug.Log("world size set to extra_large");
                return;
        }
    }



    void terainloading_benchmark(){
        set_world_size();
        GenerateFlatMap();
        // ADD check to see when client disconects 
    }

     void terainmodification_benchmark(){

        Debug.Log("Starting Benchmark: terainmodification_benchmark");
        Vector3 blockDimensions = Grass_blockPrefab.GetComponent<Renderer>().bounds.size;
        cur_xOffset = mapWidth * blockDimensions.x / 2f;
        cur_zOffset = mapLength * blockDimensions.z / 2f;
        set_world_size();
        GenerateFlatMap();
        next_Tick = Time.time + tick_interval_time;
      
    }



    

  
 
}