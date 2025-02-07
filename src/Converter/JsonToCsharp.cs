using System.Text.Json;
using JsonToCsharpPoco.Models;

namespace JsonToCsharpPoco.Converter;

public class JsonToCSharp
{
    private readonly CSharpPocoBuilder _cSharpPocoBuilder;

    public JsonToCSharp(CSharpPocoBuilder cSharpPocoBuilder) => _cSharpPocoBuilder = cSharpPocoBuilder;

    public bool TryConvertJsonToCsharp(string json, ConversionSettings options, out string syntax)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            var rootElement = document.RootElement;

            if (rootElement.ValueKind == JsonValueKind.Array)
            {
                if (rootElement.GetArrayLength() == 0)
                    throw new InvalidOperationException("JSON array is empty. Cannot convert to C# POCO.");

                rootElement = rootElement[0];

                if (rootElement.ValueKind != JsonValueKind.Object)
                    throw new InvalidOperationException("First element in JSON array must be an object.");
            }
            else if (rootElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException("JSON root must be an object or an array of objects.");
            }

            syntax = _cSharpPocoBuilder.Build(rootElement, options);
            return true;
        }
        catch (Exception ex)
        {
            syntax = $"Error converting JSON: {ex.Message}";
            return false;
        }
    }

    public string ConvertJsonToCsharp(string json, ConversionSettings options)
    {
        TryConvertJsonToCsharp(json, options, out var syntax);
        return syntax;
    }
}


