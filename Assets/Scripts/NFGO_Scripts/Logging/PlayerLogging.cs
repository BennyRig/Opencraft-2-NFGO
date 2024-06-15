using System.Diagnostics;
using System;
using UnityEngine;
using System.IO;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine.Profiling;


public class PlayerLogging : NetworkBehaviour
{
    private string logFileName;
    private StreamWriter writer;

    private bool isLoggingInitialized = false;


    private ProfilerRecorder usedMemoryRecorder;
    private ProfilerRecorder mainThreadRecorder;
    
    public override void OnNetworkSpawn()
    {
        
        base.OnNetworkSpawn();
        if (!IsServer&& IsOwner && !isLoggingInitialized)
        {
            InitializeLogging();
            isLoggingInitialized = true;
        }
    }

    void InitializeLogging()
    {
        string connectionId = Guid.NewGuid().ToString();
        logFileName = "player_log_" + connectionId + ".csv";
        
        string logDirectory = Path.Combine(Application.dataPath, "mirror_logs");


        usedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");

        mainThreadRecorder = ProfilerRecorder.StartNew( ProfilerCategory.Internal, "Main Thread", 15);

        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        string path = Path.Combine(logDirectory, logFileName);
        writer = new StreamWriter(path, true);
        string[] args = System.Environment.GetCommandLineArgs();
        string arg_string ="";
        for(int i = 0; i < args.Length;i++)
        {
            arg_string += args[i]+" ";
        }
        //writer.WriteLine($"{arg_string}");

        // Write CSV header
        writer.WriteLine("Timestamp,PlayerPosition_X,PlayerPosition_Y,PlayerPosition_Z,RoundTripDelay_ms,Memory:MB,Frame Time:ms");

        LogPlayerData();
    }

    void LogPlayerData()
    {
        InvokeRepeating(nameof(LogPlayerState), 0f, 0.5f);
    }

    
    static double GetRecorderFrameAverage(ProfilerRecorder recorder)
    {
        var samplesCount = recorder.Capacity;
        if (samplesCount == 0)
            return 0;

        double r = 0;
        unsafe
        {
            var samples = stackalloc ProfilerRecorderSample[samplesCount];
            recorder.CopyTo(samples, samplesCount);
            for (var i = 0; i < samplesCount; ++i)
                r += samples[i].Value;
            r /= samplesCount;
        }

        return r;
    }
    void LogPlayerState()
    {
        if (transform == null)
            return;

        Vector3 playerPosition = transform.position;
        
        //place holder since cant find a way to measure rtt in NFGO
        double player_rtt = NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.Singleton.NetworkConfig.NetworkTransport.ServerClientId);
        string formattedLatency =$"{player_rtt}";
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string usedMemory = $"{usedMemoryRecorder.LastValue / (1024 * 1024)}";
        string mainThread = $"{GetRecorderFrameAverage(mainThreadRecorder) * (1e-6f):F1}";

        // Log player data to file in CSV format
        string csvLine = $"{timestamp},{playerPosition.x},{playerPosition.y},{playerPosition.z},{formattedLatency},{usedMemory},{mainThread}";
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
            usedMemoryRecorder.Dispose();
            mainThreadRecorder.Dispose();
        }
    }
}
