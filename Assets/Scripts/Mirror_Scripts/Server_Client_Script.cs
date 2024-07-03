using UnityEngine;
using Mirror;
using System.Collections;
using kcp2k;

public class NetworkHelper : MonoBehaviour
{
    public NetworkManager networkManager;
    
    public bool startServerInEditor = false;
    public bool startClientInEditor = false;
    public string serverIP = "127.0.0.1";
    public int serverPort = 7777;  // Add server port variable


    void Start()
    {
        if (networkManager == null)
        {
            Debug.LogError("Network Manager is not assigned!");
            return;
        }

        if (Application.isEditor)
        {
            if (startServerInEditor)
            {
                networkManager.StartServer();
                return;
            }

            if(startClientInEditor){
                networkManager.StartClient();
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
            networkManager.networkAddress = serverIP;

            if (networkManager.transport is KcpTransport kcpTransport)
            {
                kcpTransport.Port =  (ushort)serverPort;
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
            }

        }

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
// {
//     public NetworkManager networkManager;
//     public bool startServerInEditor = false;

//     // Start is called before the first frame update
//     void Start()
//     {
//         if (networkManager == null)
//         {
//             Debug.LogError("Network Manager is not assigned!");
//             return;
//         }

// #if UNITY_EDITOR
//         if (startServerInEditor)
//         {
//             networkManager.StartServer();
//         }
//         else
//         {
//             networkManager.StartClient();
//         }
// #else
//         string[] args = System.Environment.GetCommandLineArgs();

//         foreach (string arg in args)
//         {
//             if (arg == "-server")
//             {
//                 networkManager.StartServer();
//                 return;
//             }
//             else if (arg == "-client")
//             {
//                 networkManager.StartClient();
//                 return;
//             }
//         }

//         Debug.LogError("No valid command-line arguments provided.");
// #endif
//     }
// }