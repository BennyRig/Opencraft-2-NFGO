using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkHelper_NFGO : MonoBehaviour
{
    // Start is called before the first frame update

     public bool startServerInEditor = false;

    void Start()
    {
         if (NetworkManager.Singleton == null)
        {
            Debug.LogError("Network Manager is not assigned!");
            return;
        }
#if UNITY_EDITOR
        if (startServerInEditor)
        {
            NetworkManager.Singleton.StartServer();
            Debug.Log("Started Server");
        }
        else
        {
            NetworkManager.Singleton.StartClient();
            Debug.Log("Started Client");
        
        }
#else
        string[] args = System.Environment.GetCommandLineArgs();

        foreach (string arg in args)
        {
            if (arg == "-server")
            {
                NetworkManager.Singleton.StartServer();
                Debug.Log("Started Server");
                return;
            }
            else if (arg == "-client")
            {
                NetworkManager.Singleton.StartClient();
                Debug.Log("Started Client");
                return;
            }
        }

        Debug.LogError("No valid command-line arguments provided.");
#endif
    }
}
