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

      var classDeclarations = new List<ClassDeclarationSyntax>();
      var rootClass = GenerateClass(rootElement, className, classDeclarations);
      classDeclarations.Insert(0, rootClass);

      return FormatCode(classDeclarations);
    }
    catch (Exception ex)
    {
      return $"Error converting JSON: {ex.Message}";
    }
  }

  private ClassDeclarationSyntax GenerateClass(JsonElement jsonObject, string className, List<ClassDeclarationSyntax> classDeclarations)
  {
    var classDeclaration = SyntaxFactory.ClassDeclaration(className)
        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

    var properties = new List<MemberDeclarationSyntax>();

    foreach (var property in jsonObject.EnumerateObject())
    {
      var propertyType = DeterminePropertyType(property.Value, property.Name, classDeclarations);
      var propertyDeclaration = GenerateProperty(property.Name, propertyType);
      properties.Add(propertyDeclaration);
    }

    classDeclaration = classDeclaration.AddMembers(properties.ToArray());
    return classDeclaration;
  }

  private string DeterminePropertyType(JsonElement value, string propertyName, List<ClassDeclarationSyntax> classDeclarations)
  {
    switch (value.ValueKind)
    {
      case JsonValueKind.Number:
        return value.TryGetInt32(out _) ? "int" : "double";

      case JsonValueKind.String:
        if (DateTime.TryParse(value.GetString(), out _))
          return "DateTime";
        return "string";

      case JsonValueKind.True:
        return "bool";

      case JsonValueKind.False:
        return "bool";

      case JsonValueKind.Array:
        var arrayType = DetermineArrayType(value, propertyName, classDeclarations);
        return $"IReadOnlyList<{arrayType}>";

      case JsonValueKind.Object:
        var className = ToPascalCase(propertyName);
        var nestedClass = GenerateClass(value, className, classDeclarations);
        classDeclarations.Add(nestedClass);
        return className;

      default:
        return "object";
    }
  }


  private string DetermineArrayType(JsonElement array, string propertyName, List<ClassDeclarationSyntax> classDeclarations)
  {
    if (!array.EnumerateArray().Any())
      return "object";

    var elementTypes = array.EnumerateArray()
                            .Select(element => DeterminePropertyType(element, propertyName, classDeclarations))
                            .Distinct()
                            .ToList();

    if (elementTypes.Count == 1)
      return elementTypes.First();

    // If the array contains mixed types, return object
    return "object";
  }

  private PropertyDeclarationSyntax GenerateProperty(string propertyName, string propertyType)
  {

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


    var jsonPropertyNameAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("property:JsonPropertyName"))
        .AddArgumentListArguments(SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression($"\"{propertyName}\"")));
    var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(jsonPropertyNameAttribute));

    return propertyDeclaration.AddAttributeLists(attributeList);
  }

  private string FormatCode(List<ClassDeclarationSyntax> classDeclarations)
  {
    var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("JsonConverter"))
        .AddMembers(classDeclarations.ToArray());

    var compilationUnit = SyntaxFactory.CompilationUnit()
        .AddUsings(
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")),
            SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json.Serialization"))
        )
        .AddMembers(namespaceDeclaration);

    return compilationUnit.NormalizeWhitespace().ToFullString();
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
