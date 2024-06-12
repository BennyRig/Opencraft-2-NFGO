using UnityEngine;

using System;
using System.IO;
using System.Linq;
using Unity.Netcode;

public class ServerMetricsLogger : NetworkBehaviour
{
    private string logFileName;
    private StreamWriter writer;

    private bool isLoggingInitialized = false;

    private DateTime serverStartTime;

   // idk if OnNetworkSpawn is correct
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(IsServer){
            serverStartTime = DateTime.Now;
            if (!isLoggingInitialized)
            {
                InitializeLogging();
                isLoggingInitialized = true;
            }
        }
    }

    void InitializeLogging()
    {
        logFileName = "server_log.csv";
        string logDirectory = Path.Combine(Application.dataPath, "mirror_logs");
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
        string path = Path.Combine(logDirectory, logFileName);
        writer = new StreamWriter(path, true);
    	
        // Write CSV header
        writer.WriteLine("Timestamp,Uptime(seconds),PlayerCount,ObjectCount");

        LogServerMetrics();
    }

    void LogServerMetrics()
    {
        InvokeRepeating(nameof(LogMetrics), 0f, 60f); // Log metrics every minute
    }

    void LogMetrics()
    {
        
        TimeSpan uptime = DateTime.Now - serverStartTime;
        int playerCount = NetworkManager.ConnectedClients.Count;
        int objectCount = FindObjectsOfType<NetworkObject>().Count() - playerCount;
        string[] args = System.Environment.GetCommandLineArgs();
        string arg_string ="";
        for(int i = 0; i < args.Length;i++)
        {
            arg_string += args[i] +" ";
        }

        // Log server metrics to file in CSV format
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string csvLine = $"{timestamp},{uptime.TotalSeconds},{playerCount},{objectCount},{arg_string}";
        writer.WriteLine(csvLine);

        writer.Flush();
    }

    
    public override void OnDestroy()
    {
        if(!IsServer){
            if (writer != null)
            {
                writer.Close();
            }
         }
    }
}
