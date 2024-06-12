using UnityEngine;
using System;
using System.IO;
using System.Linq;
using FishNet.Object;
using FishNet;
using Unity.Profiling;
public class ServerMetricsLogger : NetworkBehaviour
{
    private string logFileName;
    private StreamWriter writer;

    private bool isLoggingInitialized = false;

    private DateTime serverStartTime;

    private ProfilerRecorder usedMemoryRecorder;
    private ProfilerRecorder mainThreadRecorder;

    public override void OnStartServer()
    {
        base.OnStartServer();

        serverStartTime = DateTime.Now;

        if (!isLoggingInitialized)
        {
            InitializeLogging();
            isLoggingInitialized = true;
        }
    }

    void InitializeLogging()
    {
        logFileName = "server_log.csv";
        string logDirectory = Path.Combine(Application.dataPath, "mirror_logs");

        usedMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");

        mainThreadRecorder = ProfilerRecorder.StartNew( ProfilerCategory.Internal, "Main Thread", 15);


        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
        string path = Path.Combine(logDirectory, logFileName);
        writer = new StreamWriter(path, true);






        //write the current command 
        string[] args = System.Environment.GetCommandLineArgs();
        string arg_string ="";
        for(int i = 0; i < args.Length;i++)
        {
            arg_string += args[i]+" ";
        }
        writer.WriteLine($"{arg_string}");
        // Write CSV header
        writer.WriteLine("Timestamp,Uptime(seconds),PlayerCount,ObjectCount,Memory:MB,Frame Time:ms");

        LogServerMetrics();
    }

    void LogServerMetrics()
    {
        
        InvokeRepeating(nameof(LogMetrics), 0f, 0.5f); 
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

    void LogMetrics()
    {
        TimeSpan uptime = DateTime.Now - serverStartTime;
        int playerCount = InstanceFinder.ServerManager.Clients.Count; 
        int objectCount = FindObjectsOfType<NetworkObject>().Count() - playerCount;

        // Log server metrics to file in CSV format
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

         string usedMemory = $"{usedMemoryRecorder.LastValue / (1024 * 1024)}";
        string mainThread = $"{GetRecorderFrameAverage(mainThreadRecorder) * (1e-6f):F1}";


        string csvLine = $"{timestamp},{uptime.TotalSeconds},{playerCount},{objectCount},{usedMemory},{mainThread}";
        writer.WriteLine(csvLine);

        writer.Flush();
    }

    void OnDestroy()
    {
        if (writer != null)
        {
            writer.Close();
            usedMemoryRecorder.Dispose();
            mainThreadRecorder.Dispose();
        }
    }
}
