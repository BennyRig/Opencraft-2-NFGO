using UnityEngine;
using FishNet.Object;
using FishNet;

public class FlatMapGenerator : NetworkBehaviour
{
    public GameObject blockPrefab;
    public int mapWidth;
    public int mapLength;
    public int mapHeight;

    void Start()
    {
        Debug.Log("I AM SERVER"+!base.Owner.IsLocalClient);
        if (!base.Owner.IsLocalClient)
        {
            Debug.Log("I AM SERVER");
            GenerateFlatMap();
        }
    }

    void GenerateFlatMap()
    {
        Debug.Log("generating terain");
        if (blockPrefab == null)
        {
            Debug.LogError("Block prefab is not assigned.");
            return;
        }

        Vector3 blockDimensions = blockPrefab.GetComponent<Renderer>().bounds.size;

        float xOffset = mapWidth * blockDimensions.x / 2f;
        float zOffset = mapLength * blockDimensions.z / 2f;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapLength; z++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    Vector3 position = new Vector3(x * blockDimensions.x - xOffset, y * blockDimensions.y, z * blockDimensions.z - zOffset);
                    GameObject block = Instantiate(blockPrefab, position, Quaternion.identity);
                    InstanceFinder.ServerManager.Spawn(block);
                }
            }
        }
    }
}
