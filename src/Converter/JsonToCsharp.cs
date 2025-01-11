using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace JsonToCsharpPoco.Converter;

public class JsonToCSharp
{
  public string ConvertJsonToClass(string json, string className)
  {
    try
    {
      using var document = JsonDocument.Parse(json);
      var rootElement = document.RootElement;

      if (rootElement.ValueKind != JsonValueKind.Object)
        throw new InvalidOperationException("JSON root must be an object.");

      var builder = new CSharpPocoBuilder("JsonConverter");
      builder.AddClassFromJson(rootElement, className);

      return builder.Build();
    }
    catch (Exception ex)
    {
      return $"Error converting JSON: {ex.Message}";
    }
  }
  public string ConvertJsonToRecord(string json, string recordName)
  {
    try
    {
      using var document = JsonDocument.Parse(json);
      var rootElement = document.RootElement;

      if (rootElement.ValueKind != JsonValueKind.Object)
        throw new InvalidOperationException("JSON root must be an object.");

      var builder = new CSharpPocoBuilder("JsonConverter");
      builder.AddRecordFromJson(rootElement, recordName);

      return builder.Build();
    }
    catch (Exception ex)
    {
      return $"Error converting JSON: {ex.Message}";
    }
  }
}

internal class CSharpPocoBuilder
{
  private readonly string _namespaceName;
  private readonly List<MemberDeclarationSyntax> _declarations = [];

  internal CSharpPocoBuilder(string namespaceName)
  {
    _namespaceName = namespaceName;
  }

  internal void AddClassFromJson(JsonElement jsonObject, string className)
  {
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

  internal void AddRecordFromJson(JsonElement jsonObject, string recordName)
  {
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

  internal string Build()
  {
    var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(_namespaceName))
        .AddMembers(_declarations.ToArray());

    var compilationUnit = SyntaxFactory.CompilationUnit()
        .AddUsings(
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json.Serialization"))
        )
        .AddMembers(namespaceDeclaration);

    return compilationUnit.NormalizeWhitespace().ToFullString();
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
    propertyName = ProcessPropertyName(propertyName);

    var propertyDeclaration = SyntaxFactory.PropertyDeclaration(
            SyntaxFactory.ParseTypeName(propertyType),
            SyntaxFactory.Identifier(ToPascalCase(propertyName)))
        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
        .AddAccessorListAccessors(
            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
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
    propertyName = ProcessPropertyName(propertyName);

    var propertyDeclaration = SyntaxFactory.Parameter(
              SyntaxFactory.Identifier(ToPascalCase(propertyName)))
             .WithType(SyntaxFactory.ParseTypeName(propertyType));

    var jsonPropertyNameAttribute = SyntaxFactory.Attribute(
              //todo: remove spaces in the attribute
              SyntaxFactory.ParseName("property:JsonPropertyName"))
            .AddArgumentListArguments(SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression($"\"{propertyName}\"")));

    var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(jsonPropertyNameAttribute));

    return propertyDeclaration.AddAttributeLists(attributeList.NormalizeWhitespace(string.Empty));
  }

  private string ToPascalCase(string input)
  {
    if (string.IsNullOrEmpty(input)) return input;
    Span<char> buffer = stackalloc char[input.Length];
    input.AsSpan().CopyTo(buffer);
    buffer[0] = char.ToUpperInvariant(buffer[0]);
    return new string(buffer);
  }

  private string ProcessPropertyName(string propertyName)
  {

    if (int.TryParse(propertyName, out _))
    {
      propertyName = $"_{propertyName}";
    }

    return RemoveSpecialCharacters(propertyName);
  }

  public static string RemoveSpecialCharacters(string input) =>
    Regex.Replace(input, "[^a-zA-Z0-9_]", string.Empty);

}