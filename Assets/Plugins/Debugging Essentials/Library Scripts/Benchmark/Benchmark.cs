using System.Diagnostics;

namespace DebuggingEssentials
{
    public class Benchmark
    {
        public static FastCacheList<Benchmark> benchmarks = new FastCacheList<Benchmark>(128);
        public Stopwatch stopwatch = new Stopwatch();

        public string text;
        float minMiliSeconds;

        public static Benchmark Start(string text, float minMiliSeconds = -1)
        {
            Benchmark benchmark = benchmarks.GetItem();
            benchmark.minMiliSeconds = minMiliSeconds;
            benchmark.text = text;
            benchmark.stopwatch.Reset();
            benchmark.stopwatch.Start();
            return benchmark;
        }

        public void Stop()
        {
            stopwatch.Stop();
            float ms = (((float)stopwatch.Elapsed.Ticks / Stopwatch.Frequency) * 1000);

            if (ms > minMiliSeconds)
            {
                UnityEngine.Debug.Log(text + " " + ms + " ms");
                benchmarks.Add(this);
            }
        }
    }
}