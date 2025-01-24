using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Humanizer;
namespace JsonToCsharpPoco.Benchmarks;
[MemoryDiagnoser]

public class PascalCaseBenchmark
{

    private readonly string[] testCases;

    public PascalCaseBenchmark()
    {

        testCases = new[]
        {
            "hello_world",
            "this_is_a_long_variable_name_with_many_parts",
            "short",
            "12345",
            "x_1_y_2",
            "ALREADY_UPPER_CASE",
            "mixed_CASE_string",
            "a_b_c_d_e_f_g_h_i_j",
            "very_very_very_very_very_very_long_string_with_many_underscores",
            string.Empty,
            "   ",
            "no_underscore",
            "___multiple___underscores___"
        };
    }

    private string EnsureValidPropertyName(string input)
    {
        return input.Trim();
    }

    [Benchmark(Description = "Regex Implementation")]
    public void RegexImplementation()
    {
        foreach (var test in testCases)
        {
            ToPascalCaseRegex(test);
        }
    }

    // [Benchmark(Description = "Span Implementation")]
    // public void SpanImplementation()
    // {
    //     foreach (var test in testCases)
    //     {
    //         ToPascalCaseSpan(test);
    //     }
    // }

    [Benchmark(Description = "Hunanizer's Pascalize")]
    public void PascalizeImplementation()
    {
        foreach (var test in testCases)
        {
            test.Pascalize();
        }
    }

    [Benchmark(Description = "Nested Humanizer")]
    public void NestedHumanizerImplementation()
    {
        foreach (var test in testCases)
        {
            test.Humanize().Pascalize();
        }
    }
    
    [Benchmark(Description = "Nested Humanizer With LowerCase")]
    public void NestedHumanizerWithLowerCaseImplementation()
    {
        foreach (var test in testCases)
        {
            test.Humanize(LetterCasing.LowerCase).Pascalize();
        }
    }

    private string ToPascalCaseRegex(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        return Regex.Replace(EnsureValidPropertyName(input), @"(^|_)([a-z])", match => match.Groups[2].Value.ToUpper());
    }

    private string ToPascalCaseSpan(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var sanitizedPropertyName = EnsureValidPropertyName(input);

        if (int.TryParse(input, out _)) return sanitizedPropertyName;

        string[] parts = sanitizedPropertyName.Split('_', StringSplitOptions.RemoveEmptyEntries);
        int maxLength = parts.Max(p => p.Length);
        Span<char> buffer = stackalloc char[maxLength];

        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].Length > 0)
            {
                parts[i].AsSpan().CopyTo(buffer);
                buffer[0] = char.ToUpperInvariant(buffer[0]);
                parts[i] = new string(buffer.Slice(0, parts[i].Length));
            }
        }

        return string.Concat(parts);
    }
}