using UnityEngine;
using FishNet;
using PolkaDOTS.Emulation;
using FishNet.Managing;
using FishNet.Transporting;
using System.Collections;

public class NetworkHelper : MonoBehaviour
 {
    private NetworkManager networkManager;
    public bool startServerInEditor = false;
    public bool startClientInEditor = false;
    public string serverIP = "localhost";
    public int serverPort = 7777;  // Add server port variable

    void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();

        if (networkManager == null)
        {
            Debug.LogError("Network Manager is not assigned!");
            return;
        }

        if (Application.isEditor)
        {
            if (startServerInEditor)
            {
                networkManager.ServerManager.StartConnection();
                return;
            }

            if(startClientInEditor){
                networkManager.ClientManager.StartConnection();
                return;
            }
        }
        else{
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
                if (arg == "-ip")
                {
                    int index = System.Array.IndexOf(args, arg);
                    if (index < args.Length - 1)
                    {
                        serverIP = args[index + 1];
                    }
                }
                else if(arg == "-port")  // Handle server port argument
                {
                    int index = System.Array.IndexOf(args, arg);
                    if (index < args.Length - 1)
                    {
                        serverPort = int.Parse(args[index + 1]);
                    }
                }
            }

            // // Set the server IP
            networkManager.TransportManager.Transport.SetClientAddress(serverIP);
            networkManager.TransportManager.Transport.SetPort((ushort)serverPort);

            args = System.Environment.GetCommandLineArgs();
            foreach (string arg in args)
            {
                if (arg == "-server")
                {
                    networkManager.ServerManager.StartConnection();
                    return;
                }
                else if (arg == "-client")
                {
                    networkManager.ClientManager.StartConnection();
                    return;
                }
            }

        }

    }

    // void Start()
    // {
    //     _networkManager = FindObjectOfType<NetworkManager>();

    //     if (_networkManager == null)
    //     {
    //         Debug.LogError("Network Manager is not assigned!");
    //         return;
    //     }

    //     #if UNITY_EDITOR  
    //     if (startServerInEditor)
    //     {
    //         _networkManager.ServerManager.StartConnection();
    //     }

    //     if(startClientInEditor){
    //         _networkManager.ClientManager.StartConnection();
    //     }

    //     #else
    //     string[] args = System.Environment.GetCommandLineArgs();
    //     for(int i = 0; i < args.Length-1;i++)
    //     {
    //         if (args[i] == "-closeafter"){
    //             int time;
    //             if (!int.TryParse(args[i+1],  out time)){
    //                 Debug.LogError("Unable to parse string.");    
    //             }
    //             StartCoroutine(waiter(time));
    //             break;
    //         }
    //     }

    //     args = System.Environment.GetCommandLineArgs();

    //     foreach (string arg in args)
    //     {
    //         if (arg == "-server")
    //         {
    //             _networkManager.ServerManager.StartConnection();
    //             return;
    //         }
    //         else if (arg == "-client")
    //         {
    //             _networkManager.ClientManager.StartConnection();
    //             return;
    //         }
    //         else if (arg == "-server_ip")
    //         {
    //             int index = System.Array.IndexOf(args, arg);
    //             if (index < args.Length - 1)
    //             {
    //                 serverIP = args[index + 1];
    //             }
    //             break;
    //         }
    //         else if(arg == "-server_port")  // Handle server port argument
    //         {
    //             int index = System.Array.IndexOf(args, arg);
    //             if (index < args.Length - 1)
    //             {
    //                 serverPort = int.Parse(args[index + 1]);
    //             }
    //             break;
    //         }
    //     }

    //     // // Set the server IP
    //     // networkManager.networkAddress = serverIP;

    //     // // Set the server port if a port transport is available
    //     // if (Transport.active is PortTransport portTransport)
    //     // {
    //     //     portTransport.Port = (ushort)serverPort;
    //     // }
        
    //     // networkManager.StartClient();
    //     #endif

    // }
    IEnumerator waiter(int time)
    {
        yield return new WaitForSeconds(time);
        Debug.Log("quiting client");
        Application.Quit();
        Debug.Log("this should not print");
    }

    // private void StartPlayerReplayEmulation(string inputTraceFile){
    //     // Spawn player and remember the instance
        
    //     // Get the player GameObject and load the input recording into it

    //     InputRecorder inputRecorder = player.GetComponent<InputRecorder>();
    //     inputRecorder.LoadCaptureFromFile(inputTraceFile);
    //     inputRecorder.StartReplay();
    // }

}

