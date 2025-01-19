using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using JsonToCsharpPoco.Benchmarks;

[InProcessAttribute]
public class RunBenchmarks
{
    static void Main(string[] args)
    {
        //var results = BenchmarkRunner.Run<StringUtilityBenchmark>();
         var results = BenchmarkRunner.Run<PascalCaseBenchmark>();

        //dotnet commands
        //dotnet run --framework net8.0 net9.0 --configuration Release --no-debug
        //dotnet run --configuration Release --no-debug
    }
}