using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using JsonToCsharpPoco.Benchmarks;

[MemoryDiagnoser]
public class RunBenchmarks
{
    static void Main(string[] args)
    {
        var results = BenchmarkRunner.Run<StringUtilityBenchmark>();

        //dotnet commands
        //dotnet run --framework net8.0 net9.0 --configuration Release --no-debug
        //dotnet run --configuration Release --no-debug
    }
}