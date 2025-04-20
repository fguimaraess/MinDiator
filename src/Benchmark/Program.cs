using BenchmarkDotNet.Running;

namespace Benchmark;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<Benchmarks>();
        //var b = new Benchmarks();
        //b.Setup();
        //Console.WriteLine(b.MinDiator_IMediator_Send().GetAwaiter().GetResult());
        //b.MinDiator_IPublisher_Publish().GetAwaiter().GetResult();
    }
}

