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
            Debug.LogError("Started Server");
        }
        else
        {
            NetworkManager.Singleton.StartClient();
            Debug.LogError("Started Client");
        
        }
#else
        string[] args = System.Environment.GetCommandLineArgs();

        foreach (string arg in args)
        {
            if (arg == "-server")
            {
               NetworkManager.Singleton.StartServer();
                return;
            }
            else if (arg == "-client")
            {
                NetworkManager.Singleton.StartClient();
                return;
            }
        }

        Debug.LogError("No valid command-line arguments provided.");
#endif
    }
}
