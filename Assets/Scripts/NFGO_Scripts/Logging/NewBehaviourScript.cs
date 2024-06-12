// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// using System.Diagnostics;
// public class Measure : MonoBehaviour {


// private PerformanceCounter cpuCounter;
// private PerformanceCounter ramCounter;
//     void Start()
//     {
        
//         InitialiseCPUCounter();
//         InitializeRAMCounter();

//     }

//     void Update()
//     {
        
//         print( "CPU Usage: " +System.Convert.ToInt32(cpuCounter.NextValue()).ToString() +"%");

//         print( System.Convert.ToInt32(ramCounter.NextValue()).ToString()+"Mb");
//     }

//     private void InitialiseCPUCounter()
//     {
//         cpuCounter = new PerformanceCounter(
//             "Processor",
//             "% Processor Time",
//             "_Total",
//             true
//         );
//     }

//     private void InitializeRAMCounter()
//     {
//         ramCounter = new PerformanceCounter("Memory", "Available MBytes", true);

//     }
// }