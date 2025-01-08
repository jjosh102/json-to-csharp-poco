using System.Text.Json;
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

      var builder = new CSharpClassBuilder("JsonConverter");
      builder.AddClassFromJson(rootElement, className);

      return builder.Build();
    }
    catch (Exception ex)
    {
      return $"Error converting JSON: {ex.Message}";
    }
  }
}

internal class CSharpClassBuilder
{
  private readonly string _namespaceName;
  private readonly List<ClassDeclarationSyntax> _classDeclarations = new();

  internal CSharpClassBuilder(string namespaceName)
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
      var propertyType = DeterminePropertyType(property.Value, property.Name);

      // Handle nested objects and arrays
      if (property.Value.ValueKind == JsonValueKind.Object)
      {
        var nestedClassName = ToPascalCase(property.Name);
        AddClassFromJson(property.Value, nestedClassName);
        propertyType = nestedClassName;
      }
      else if (property.Value.ValueKind == JsonValueKind.Array && property.Value.EnumerateArray().Any() && property.Value[0].ValueKind == JsonValueKind.Object)
      {
        var nestedClassName = ToPascalCase(property.Name);
        AddClassFromJson(property.Value[0], nestedClassName);
        propertyType = $"IReadOnlyList<{nestedClassName}>";
      }

      var propertyDeclaration = GenerateProperty(property.Name, propertyType);
      properties.Add(propertyDeclaration);
    }

    classDeclaration = classDeclaration.AddMembers(properties.ToArray());
    _classDeclarations.Add(classDeclaration);

  }

  internal string Build()
  {
    var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(_namespaceName))
        .AddMembers(_classDeclarations.ToArray());

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

  public PropertyDeclarationSyntax GenerateProperty(string propertyName, string propertyType)
  {
    if (int.TryParse(propertyName, out _))
    {
      propertyName = $"_{propertyName}";
    }

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

    var jsonPropertyNameAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("JsonPropertyName"))
        .AddArgumentListArguments(SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression($"\"{propertyName}\"")));

    var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(jsonPropertyNameAttribute));
    return propertyDeclaration.AddAttributeLists(attributeList);
  }

  private string ToPascalCase(string input)
  {
    if (string.IsNullOrEmpty(input)) return input;
    Span<char> buffer = stackalloc char[input.Length];
    input.AsSpan().CopyTo(buffer);
    buffer[0] = char.ToUpperInvariant(buffer[0]);
    return new string(buffer);
  }

  private string RemoveTrailingS(string input)
  {
    //todo: handle cases like "status" -> "Status" and "items" -> "Item"
    if (string.IsNullOrEmpty(input)) return input;
    return input.EndsWith("s", StringComparison.InvariantCultureIgnoreCase) ? input[..^1] : input;
  }
}
