using System.Text.Json;
using System.Text.RegularExpressions;
using JsonToCsharp.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace JsonToCsharpPoco.Converter;

public class JsonToCSharp
{
  public bool TryConvertJsonToPoco(string json, ConversionOptions options, out string syntax)
  {
    try
    {
      using var document = JsonDocument.Parse(json);
      var rootElement = document.RootElement;

      if (rootElement.ValueKind != JsonValueKind.Object)
        throw new InvalidOperationException("JSON root must be an object.");

      var builder = new CSharpPocoBuilder(rootElement, options);
      syntax = builder.Build();
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

internal partial class CSharpPocoBuilder
{
  private readonly ConversionOptions _conversionOptions;
  private readonly JsonElement _rootElement;
  private readonly List<MemberDeclarationSyntax> _declarations = [];
  internal CSharpPocoBuilder(JsonElement rootElement, ConversionOptions conversionOptions)
  {
    _conversionOptions = conversionOptions;
    _rootElement = rootElement;
  }

  internal string Build()
  {
    return _conversionOptions.GenerateRecords
        ? Build(AddRecordFromJson)
        : Build(AddClassFromJson);
  }

  private string Build(Action<JsonElement, string> buildSyntax)
  {
    buildSyntax(_rootElement, _conversionOptions.RootTypeName);

    var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(_conversionOptions.Namespace))
        .AddMembers(_declarations.ToArray());

    var compilationUnit = SyntaxFactory.CompilationUnit()
        .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json.Serialization")))
        .AddMembers(namespaceDeclaration);

    return compilationUnit.NormalizeWhitespace().ToFullString();
  }

  private void AddClassFromJson(JsonElement jsonObject, string className)
  {
    className = SanitizePropertyName(className);
    var classDeclaration = SyntaxFactory.ClassDeclaration(className)
        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

    var properties = new List<MemberDeclarationSyntax>();

    foreach (var property in jsonObject.EnumerateObject())
    {
      var propertyType = HandlePropertyType(property.Value, property.Name, AddClassFromJson);
      var propertyDeclaration = GenerateClassProperty(property.Name, propertyType);
      properties.Add(propertyDeclaration);
    }

    classDeclaration = classDeclaration.AddMembers(properties.ToArray());
    _declarations.Add(classDeclaration);
  }

  private void AddRecordFromJson(JsonElement jsonObject, string recordName)
  {
    recordName = SanitizePropertyName(recordName);
    var properties = new List<ParameterSyntax>();

    foreach (var property in jsonObject.EnumerateObject())
    {
      var propertyType = HandlePropertyType(property.Value, property.Name, AddRecordFromJson);

      var parameter = GenerateRecordProperty(property.Name, propertyType);
      properties.Add(parameter);
    }

    var recordDeclaration = SyntaxFactory.RecordDeclaration(SyntaxFactory.Token(SyntaxKind.RecordKeyword), recordName)
        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
        .AddParameterListParameters(properties.ToArray())
        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

    _declarations.Add(recordDeclaration);
  }

  private string HandlePropertyType(JsonElement propertyValue, string propertyName, Action<JsonElement, string> addNestedTypeAction)
  {
    string propertyType = DeterminePropertyType(propertyValue, propertyName);

    if (propertyValue.ValueKind == JsonValueKind.Object)
    {
      var nestedTypeName = ToPascalCase(propertyName);
      addNestedTypeAction(propertyValue, nestedTypeName);
      return nestedTypeName;
    }

    if (propertyValue.ValueKind == JsonValueKind.Array &&
        propertyValue.EnumerateArray().Any() &&
        propertyValue[0].ValueKind == JsonValueKind.Object)
    {
      var nestedTypeName = ToPascalCase(propertyName);
      addNestedTypeAction(propertyValue[0], nestedTypeName);
      return $"IReadOnlyList<{nestedTypeName}>";
    }

    return propertyType;
  }

  private string DeterminePropertyType(JsonElement value, string propertyName)
  {
    return value.ValueKind switch
    {
      JsonValueKind.Number => value.TryGetInt32(out _) ? "int" : "double",
      JsonValueKind.String => DateTime.TryParse(value.GetString(), out _) ? "DateTime" : "string",
      JsonValueKind.True => "bool",
      JsonValueKind.False => "bool",
      JsonValueKind.Array => $"IReadOnlyList<{DetermineArrayType(value, propertyName)}>",
      JsonValueKind.Object => ToPascalCase(propertyName),
      _ => "object"
    };
  }

  private string DetermineArrayType(JsonElement array, string propertyName)
  {
    if (!array.EnumerateArray().Any())
      return "object";

    var elementTypes = array.EnumerateArray()
        .Select(element => DeterminePropertyType(element, propertyName))
        .Distinct()
        .ToList();

    return elementTypes.Count == 1 ? elementTypes.First() : "object";
  }

  private PropertyDeclarationSyntax GenerateClassProperty(string propertyName, string propertyType)
  {
    propertyName = SanitizePropertyName(propertyName);

    var propertyDeclaration = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.ParseTypeName(propertyType),
            SyntaxFactory.Identifier(ToPascalCase(propertyName)))
        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
        .AddAccessorListAccessors(
            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
            // todo: add option mutabale or not 
            SyntaxFactory.AccessorDeclaration(SyntaxKind.InitAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
        );

    var jsonPropertyNameAttribute = SyntaxFactory.Attribute(
        SyntaxFactory.ParseName("JsonPropertyName"))
        .AddArgumentListArguments(SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression($"\"{propertyName}\"")));

    var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(jsonPropertyNameAttribute));

    return propertyDeclaration.AddAttributeLists(attributeList);
  }

  private ParameterSyntax GenerateRecordProperty(string propertyName, string propertyType)
  {
    string sanitizePropertyName = SanitizePropertyName(propertyName);

    var propertyDeclaration = SyntaxFactory.Parameter(
            SyntaxFactory.Identifier(ToPascalCase(sanitizePropertyName)))
        .WithType(SyntaxFactory.ParseTypeName(propertyType));

    var jsonPropertyNameAttribute = SyntaxFactory.Attribute(
            SyntaxFactory.IdentifierName("property:JsonPropertyName"))
        .AddArgumentListArguments(
            SyntaxFactory.AttributeArgument(
                SyntaxFactory.LiteralExpression(
                    SyntaxKind.StringLiteralExpression,
                    SyntaxFactory.Literal(RemoveSpecialCharacters(propertyName)))));

    var attributeList = SyntaxFactory.AttributeList(
        SyntaxFactory.SingletonSeparatedList(jsonPropertyNameAttribute));

    return propertyDeclaration.AddAttributeLists(attributeList);
  }

  private string ToPascalCase(string input)
  {
    if (string.IsNullOrEmpty(input)) return input;
    input = SanitizePropertyName(input);
    Span<char> buffer = stackalloc char[input.Length];
    input.AsSpan().CopyTo(buffer);
    buffer[0] = char.ToUpperInvariant(buffer[0]);
    return new string(buffer);
  }

  private string SanitizePropertyName(string propertyName)
  {
    if (int.TryParse(propertyName, out _))
    {
      propertyName = $"_{propertyName}";
    }

    return RemoveSpecialCharacters(propertyName);
  }

  [GeneratedRegex("[^a-zA-Z0-9_]", RegexOptions.Compiled)]
  private static partial Regex MyRegex();
  private static string RemoveSpecialCharacters(string input) =>
      MyRegex().Replace(input, string.Empty);
}
