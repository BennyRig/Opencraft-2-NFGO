using System;
using UnityEngine;
using Mirror;
using System.IO;
using Unity.Profiling;


public class PlayerLogging : NetworkBehaviour
{
    private string logFileName;
    private StreamWriter writer;

    private bool isLoggingInitialized = false;

    private ProfilerRecorder usedMemoryRecorder;
    private ProfilerRecorder mainThreadRecorder;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        if (isLocalPlayer && !isLoggingInitialized)
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
       
        double player_rtt = NetworkTime.rtt * 1000;
        
        string formattedLatency =$"{player_rtt}";
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string usedMemory = $"{usedMemoryRecorder.LastValue / (1024 * 1024)}";
        string mainThread = $"{GetRecorderFrameAverage(mainThreadRecorder) * (1e-6f):F1}";

        // Log player data to file in CSV format
        string csvLine = $"{timestamp},{playerPosition.x},{playerPosition.y},{playerPosition.z},{formattedLatency},{usedMemory},{mainThread}";
        writer.WriteLine(csvLine);

        writer.Flush();
    }

    void OnDestroy()
    {
        if(isLocalPlayer){
            if (writer != null)
            {
                writer.Close();
            }
            usedMemoryRecorder.Dispose();
            mainThreadRecorder.Dispose();
        }
    }
}