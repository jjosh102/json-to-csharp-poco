
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;

namespace JsonToCsharpPoco.Benchmarks;

public class StringUtilityBenchmark
{

    [Benchmark]
    public string RemoveSpecialCharactersWithHashSet()
    {
        string input = "123!@#abcABC_@#$defGHI1234*()xyz!@#";
        var validChars = new HashSet<char>("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_");
        var result = new List<char>();
        foreach (var c in input)
        {
            if (validChars.Contains(c))
                result.Add(c);
        }
        return new string(result.ToArray());
    }

    [Benchmark]
    public string RemoveSpecialCharactersWithRegex()
    {
        string input = "123!@#abcABC_@#$defGHI1234*()xyz!@#";
        return Regex.Replace(input, "[^a-zA-Z0-9_]", string.Empty);
    }
}

