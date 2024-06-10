using UnityEngine;
using Unity.Netcode;
using PolkaDOTS.Emulation;
using Unity.Netcode.Transports.UTP;
using System.Collections;

public class NetworkHelper_NFGO : MonoBehaviour
{
    public NetworkManager networkManager;
    
    public bool startServerInEditor = false;
    public bool startClientInEditor = false;
    public string serverIP = "localhost";
    public int serverPort = 7777;  // Add server port variable


    void Start()
    {
        if (networkManager == null)
        {
            Debug.LogError("Network Manager is not assigned!");
            return;
        }

        #if UNITY_EDITOR
        if (startServerInEditor)
        {
            networkManager.StartServer();

            
        }

        if(startClientInEditor){
            networkManager.StartClient();
        }

        #else
        string[] args = System.Environment.GetCommandLineArgs();
        for(int i = 0; i < args.Length-1;i++)
        {
            if (args[i] == "-closeafter"){
                int time;
                if (!int.TryParse(args[i+1],  out time)){
                    Debug.LogError("Unable to parse string.");    
                }
                StartCoroutine(waiter(time));
                break;
            }
        }
        args = System.Environment.GetCommandLineArgs();
        foreach (string arg in args)
        {
            if (arg == "-server")
            {
                networkManager.StartServer();
                return;
            }
            else if (arg == "-client")
            {
                networkManager.StartClient();
                return;
            }
            
            // else if (arg == "-server_ip")
            // {
            //     int index = System.Array.IndexOf(args, arg);
            //     if (index < args.Length - 1)
            //     {
            //         serverIP = args[index + 1];
            //     }
            //     break;
            // }
            // else if(arg == "-server_port")  // Handle server port argument
            // {
            //     int index = System.Array.IndexOf(args, arg);
            //     if (index < args.Length - 1)
            //     {
            //         serverPort = int.Parse(args[index + 1]);
            //     }
            //     break;
            // }
        }

        // // Set the server IP
        // UnityTransport unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        
        // unityTransport.SetConnectionData(serverIP, serverPort);

        // // Set the server port if a port transport is available
        // if (Transport.active is PortTransport portTransport)
        // {
        //     portTransport.Port = (ushort)serverPort;
        // }
        
        // networkManager.StartClient();
        #endif

    }

    // private void StartPlayerReplayEmulation(string inputTraceFile){
    //     // Spawn player and remember the instance
        
    //     // Get the player GameObject and load the input recording into it

        // InputRecorder inputRecorder = player.GetComponent<InputRecorder>();
        // inputRecorder.LoadCaptureFromFile(inputTraceFile);
        // inputRecorder.StartReplay();
    // }

    
    IEnumerator waiter(int time)
    {
        //Wait for 4 seconds
        yield return new WaitForSeconds(time);
        Debug.Log("quiting client");
        Application.Quit();
        Debug.Log("this should not print");
    }


}
// using System.Collections;
// using System.Collections.Generic;
// using Unity.Netcode;
// using UnityEngine;

// public class NetworkHelper_NFGO : MonoBehaviour
// {
//     // Start is called before the first frame update

//      public bool startServerInEditor = false;

//     void Start()
//     {
//          if (NetworkManager.Singleton == null)
//         {
//             Debug.LogError("Network Manager is not assigned!");
//             return;
//         }
// #if UNITY_EDITOR
//         if (startServerInEditor)
//         {
//             NetworkManager.Singleton.StartServer();
//             Debug.Log("Started Server");
//         }
//         else
//         {
//             NetworkManager.Singleton.StartClient();
//             Debug.Log("Started Client");
        
//         }
// #else
//         string[] args = System.Environment.GetCommandLineArgs();

//         foreach (string arg in args)
//         {
//             if (arg == "-server")
//             {
//                 NetworkManager.Singleton.StartServer();
//                 Debug.Log("Started Server");
//                 return;
//             }
//             else if (arg == "-client")
//             {
//                 NetworkManager.Singleton.StartClient();
//                 Debug.Log("Started Client");
//                 return;
//             }
//         }

//         Debug.LogError("No valid command-line arguments provided.");
// #endif
//     }
// }
