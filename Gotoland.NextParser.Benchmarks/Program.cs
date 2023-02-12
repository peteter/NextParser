using NextParser.Benchmarks;
using BenchmarkDotNet.Running;

public class Program
{
    private static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<NextBenchmarks>();
    }
}