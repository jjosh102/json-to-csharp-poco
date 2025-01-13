using System.Text.Json;
using System.Text.RegularExpressions;
using JsonToCsharpPoco.Models;
using JsonToCsharpPoco.Models.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace JsonToCsharpPoco.Converter;

public partial class CSharpPocoBuilder
{
    public string Build(JsonElement rootElement, ConversionOptions options)
    {
        return options.GenerateRecords
            ? Build(rootElement, options, AddRecordFromJson)
            : Build(rootElement, options, AddClassFromJson);
    }

    private string Build(JsonElement rootElement, ConversionOptions options,
        Action<JsonElement, string, List<MemberDeclarationSyntax>, ConversionOptions> buildSyntax)
    {
        var declarations = new List<MemberDeclarationSyntax>();
        buildSyntax(rootElement, options.RootTypeName, declarations, options);

        var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(options.Namespace))
            .AddMembers(declarations.ToArray());

        var compilationUnit = SyntaxFactory.CompilationUnit()
            .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json.Serialization")))
            .AddMembers(namespaceDeclaration);

        return compilationUnit.NormalizeWhitespace().ToFullString();
    }

    private void AddClassFromJson(JsonElement jsonObject,
        string className,
        List<MemberDeclarationSyntax> declarations,
        ConversionOptions options)
    {
        className = SanitizePropertyName(className);
        var classDeclaration = SyntaxFactory.ClassDeclaration(className)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        var properties = CreatePropertiesFromJson(jsonObject, declarations, options);

        classDeclaration = classDeclaration.AddMembers(properties.ToArray());
        declarations.Add(classDeclaration);
    }

    //todo:refactor
    private void AddRecordFromJson(JsonElement jsonObject,
        string recordName,
        List<MemberDeclarationSyntax> declarations,
        ConversionOptions options)
    {
        if (options.UsePrimaryConstructor)
        {
            GenerateRecordFromJsonUsingPrimaryConstructor(jsonObject, recordName, declarations, options);
        }
        else
        {
            GenerateRecordFromJson(jsonObject, recordName, declarations, options);
        }
    }

    //todo:refactor
    private void GenerateRecordFromJsonUsingPrimaryConstructor(JsonElement jsonObject,
        string recordName,
        List<MemberDeclarationSyntax> declarations,
        ConversionOptions options)
    {
        recordName = SanitizePropertyName(recordName);
        var recordDeclaration = SyntaxFactory
            .RecordDeclaration(SyntaxFactory.Token(SyntaxKind.RecordKeyword), recordName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        var parameters = CreateParametersFromJson(jsonObject, declarations, options);

        recordDeclaration = recordDeclaration
            .AddParameterListParameters(parameters.ToArray())
            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

        declarations.Add(recordDeclaration);
    }

    //todo:refactor
    private void GenerateRecordFromJson(JsonElement jsonObject,
        string recordName,
        List<MemberDeclarationSyntax> declarations,
        ConversionOptions options)
    {
        recordName = SanitizePropertyName(recordName);
        var recordDeclaration = SyntaxFactory
            .RecordDeclaration(SyntaxFactory.Token(SyntaxKind.RecordKeyword), recordName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
            .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken));

        var properties = CreatePropertiesFromJson(jsonObject, declarations, options);

        recordDeclaration = recordDeclaration.AddMembers(properties.ToArray());
        declarations.Add(recordDeclaration);
    }

    private string HandlePropertyType(JsonElement propertyValue,
        string propertyName,
        List<MemberDeclarationSyntax> declarations,
        Action<JsonElement, string, List<MemberDeclarationSyntax>, ConversionOptions> addNestedTypeAction,
        ConversionOptions options)
    {
        string propertyType = DeterminePropertyType(propertyValue, propertyName);

        if (propertyValue.ValueKind == JsonValueKind.Object)
        {
            var nestedTypeName = ToPascalCase(propertyName);
            addNestedTypeAction(propertyValue, nestedTypeName, declarations, options);
            return nestedTypeName;
        }

        if (propertyValue.ValueKind == JsonValueKind.Array &&
            propertyValue.EnumerateArray().Any() &&
            propertyValue[0].ValueKind == JsonValueKind.Object)
        {
            var nestedTypeName = ToPascalCase(propertyName);
            addNestedTypeAction(propertyValue[0], nestedTypeName, declarations, options);
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

    private PropertyDeclarationSyntax GenerateClassProperty(string propertyName, string propertyType,
        ConversionOptions options)
    {
        string sanitizedPropertyName = SanitizePropertyName(propertyName);

        var accessors = options.PropertyAccess switch
        {
            PropertyAccess.Mutable => new[]
            {
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            },
            PropertyAccess.Immutable => new[]
            {
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                SyntaxFactory.AccessorDeclaration(SyntaxKind.InitAccessorDeclaration)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            },
            _ => Array.Empty<AccessorDeclarationSyntax>()
        };

        var propertyDeclaration = SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.ParseTypeName(propertyType),
                SyntaxFactory.Identifier(ToPascalCase(sanitizedPropertyName)))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(accessors);

        var attribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("JsonPropertyName"))
            .AddArgumentListArguments(
                SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression($"\"{propertyName}\"")));

        return propertyDeclaration.AddAttributeLists(
            SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute)));
    }

    private ParameterSyntax GenerateRecordParameter(string propertyName, string propertyType)
    {
        string sanitizedPropertyName = SanitizePropertyName(propertyName);

        var propertyDeclaration = SyntaxFactory.Parameter(
                SyntaxFactory.Identifier(ToPascalCase(sanitizedPropertyName)))
            .WithType(SyntaxFactory.ParseTypeName(propertyType));

        var attribute = SyntaxFactory.Attribute(
                SyntaxFactory.IdentifierName("property:JsonPropertyName"))
            .AddArgumentListArguments(
                SyntaxFactory.AttributeArgument(
                    SyntaxFactory.LiteralExpression(
                        SyntaxKind.StringLiteralExpression,
                        SyntaxFactory.Literal(RemoveSpecialCharacters(propertyName)))));

        var attributeList = SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(attribute));

        return propertyDeclaration.AddAttributeLists(attributeList);
    }

    private List<ParameterSyntax> CreateParametersFromJson(JsonElement jsonObject,
        List<MemberDeclarationSyntax> declarations, ConversionOptions options)
    {
        var parameters = new List<ParameterSyntax>();

        foreach (var property in jsonObject.EnumerateObject())
        {
            var propertyType =
                HandlePropertyType(property.Value, property.Name, declarations, AddRecordFromJson, options);

            var parameter = GenerateRecordParameter(property.Name, propertyType);
            parameters.Add(parameter);
        }

        return parameters;
    }

    private List<MemberDeclarationSyntax> CreatePropertiesFromJson(JsonElement jsonObject,
        List<MemberDeclarationSyntax> declarations, ConversionOptions options)
    {
        var properties = new List<MemberDeclarationSyntax>();

        foreach (var property in jsonObject.EnumerateObject())
        {
            var propertyType =
                HandlePropertyType(property.Value, property.Name, declarations,
                    options.GenerateRecords ? AddRecordFromJson : AddClassFromJson, options);
            var propertyDeclaration = GenerateClassProperty(property.Name, propertyType, options);
            properties.Add(propertyDeclaration);
        }

        return properties;
    }

    private TypeDeclarationSyntax CreateClassDeclaration(string className, ConversionOptions options)
    {
        if (options.GenerateRecords)
        {
            return SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
        }
        else
        {
            return SyntaxFactory.RecordDeclaration(SyntaxFactory.Token(SyntaxKind.RecordKeyword), className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
        }
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
    private static partial Regex SpecialCharactersRegex();

    private static string RemoveSpecialCharacters(string input) =>
        SpecialCharactersRegex().Replace(input, string.Empty);
}