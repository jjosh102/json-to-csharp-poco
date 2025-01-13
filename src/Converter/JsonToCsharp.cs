using System.Text.Json;
using JsonToCsharp.Models;

namespace JsonToCsharpPoco.Converter;

public class JsonToCSharp
{
  private readonly CSharpPocoBuilder _cSharpPocoBuilder;

  public JsonToCSharp(CSharpPocoBuilder cSharpPocoBuilder) => _cSharpPocoBuilder = cSharpPocoBuilder;

  public bool TryConvertJsonToPoco(string json, ConversionOptions options, out string syntax)
  {
    try
    {
      using var document = JsonDocument.Parse(json);
      var rootElement = document.RootElement;

      if (rootElement.ValueKind != JsonValueKind.Object)
        throw new InvalidOperationException("JSON root must be an object.");

      syntax = _cSharpPocoBuilder.Build(rootElement, options);
      return true;
    }
    catch (Exception ex)
    {
      syntax = $"Error converting JSON: {ex.Message}";
      return false;
    }
  }

  public string ConvertJsonToPoco(string json, ConversionOptions options)
  {
    TryConvertJsonToPoco(json, options, out var syntax);
    return syntax;
  }
}


