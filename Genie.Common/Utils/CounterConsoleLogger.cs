
using Microsoft.AspNetCore.Mvc.TagHelpers;
using System.Diagnostics;
using System.Timers;

namespace Genie.Common.Utils;


public class CounterConsoleLogger
{
    public static DateTime? Started { get; set; }
    public static int Counter { get; set; }
    public static DateTime? BatchStarted { get; set; }
    public static int BatchCounter { get; set; }
    private static int ErrorCounter { get; set; }

    private static PerformanceCounter cpuCounter = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total", true);
    private static PerformanceCounter memoryAvailable = new PerformanceCounter("Memory", "Available MBytes");
    private static PerformanceCounter diskRead = new PerformanceCounter("PhysicalDisk", "Avg. Disk Bytes/Read", "_Total");
    private static PerformanceCounter diskWrite = new PerformanceCounter("PhysicalDisk", "Avg. Disk Bytes/Write", "_Total");

    private static bool countersInitialized = false;

    public List<long> Latency = new(100000);
    private int BatchSize = 25000;

    public CounterConsoleLogger()
    {

    }

    public CounterConsoleLogger(int batchSize) {
        BatchSize = batchSize;
    }

    public static void Reset()
    {
        Started = null;
        Counter = 0;
        BatchStarted = null;
        BatchCounter = 0;
        ErrorCounter = 0;
        countersInitialized = false;
    }

    public void Process()
    {
        Process(null);
    }

    public void Process(long? latency)
    {
        if (latency != null)
            Latency.Add(latency.Value);

        var counter = CounterConsoleLogger.Counter;
        if (counter == 0)
            Console.WriteLine("First Operation Observed");

        Interlocked.Increment(ref counter);
        CounterConsoleLogger.Counter = counter;

        var batchCounter = CounterConsoleLogger.BatchCounter;
        Interlocked.Increment(ref batchCounter);
        CounterConsoleLogger.BatchCounter = batchCounter;

        if (CounterConsoleLogger.Started == null)
        {
            CounterConsoleLogger.Started = DateTime.UtcNow;
        }
            

        if (CounterConsoleLogger.BatchStarted == null)
            CounterConsoleLogger.BatchStarted = DateTime.UtcNow;

        if (counter % BatchSize == 0)
        {
            Print(counter, batchCounter);
        }
    }

    public void ProcessError()
    {
        var counter = CounterConsoleLogger.ErrorCounter;
        Interlocked.Increment(ref counter);
        CounterConsoleLogger.ErrorCounter = counter;
    }

    public void Print()
    {
        Print(CounterConsoleLogger.Counter, CounterConsoleLogger.BatchCounter);

        // Filter out 0;
        Latency = Latency.Where(t => t > 0).Order().ToList();

        var avg = Math.Round(Latency.Average(), 2);
        var min = Latency.Min();
        var max = Latency.Max();
        var median = Latency[Latency.Count / 2];
        var p95 = Latency[(int)Math.Floor(Latency.Count * .95)];
        var p99 = Latency[(int)Math.Floor(Latency.Count * .99)];

        Console.WriteLine($@"Completed {DateTime.Now} Requests: {CounterConsoleLogger.Counter} Errors: {CounterConsoleLogger.ErrorCounter}");
        Console.WriteLine($@"Latency (Median, Mean, Min, Max, 95, 99): [{median}, {avg}, {min}, {max}, {p95}, {p99}]");
    }

    private static void Print(int counter, int batchCounter)
    {
        var elapsed = (DateTime.UtcNow - CounterConsoleLogger.Started!.Value).TotalSeconds;
        var batchElapsed = (DateTime.UtcNow - CounterConsoleLogger.BatchStarted!.Value).TotalSeconds;
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write($@"[{DateTime.Now.ToLongTimeString()}] Overall {Convert.ToInt32(counter / elapsed)}");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($@" Period: {Convert.ToInt32(batchCounter / batchElapsed)}");
        if (CounterConsoleLogger.ErrorCounter > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($@" Errors: {CounterConsoleLogger.ErrorCounter}");
        }

        Console.ForegroundColor = ConsoleColor.Gray;

        if (!countersInitialized)
        {
            countersInitialized = true;

            cpuCounter.NextValue();
            memoryAvailable.NextValue();
            diskRead.NextValue();
            diskWrite.NextValue();
        }


        var cpu_sample = new List<float>();
        var read_sample = new List<float>();
        var memory_sample = new List<float>();
        var write_sample = new List<float>();

        for (int i = 0; i < 5; i++)
        {
            cpu_sample.Add(cpuCounter.NextValue());
            memory_sample.Add(memoryAvailable.NextValue());
            read_sample.Add(diskRead.NextValue());
            write_sample.Add(diskWrite.NextValue());
            Thread.Sleep(5);
        }


        Console.WriteLine($" CPU: {Math.Round(cpu_sample.Average())} Memory: {Math.Round((32768 - memory_sample.Average()) * 100 / 32768)} Disk: [{Math.Round(read_sample.Average())}/{Math.Round(write_sample.Average())}]");

        CounterConsoleLogger.ResetBatch();
    }

    public static void ResetBatch()
    {
        BatchStarted = DateTime.UtcNow;
        BatchCounter = 0;
    }
}