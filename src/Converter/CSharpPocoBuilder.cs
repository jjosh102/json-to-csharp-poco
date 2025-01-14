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
        return options.UseRecords
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
        .AddMembers(namespaceDeclaration);

        if (options.AddAttribute)
        {
            compilationUnit = compilationUnit.AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json.Serialization")));
        }

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
            GeneratePrimaryConstructorRecordFromJson(jsonObject, recordName, declarations, options);
        }
        else
        {
            GenerateStandardRecordFromJson(jsonObject, recordName, declarations, options);
        }
    }


    private void GeneratePrimaryConstructorRecordFromJson(JsonElement jsonObject,
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


    private void GenerateStandardRecordFromJson(JsonElement jsonObject,
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

    private string GetDefaultValue(string propertyType)
    {
        return propertyType switch
        {
            "string" => "string.Empty",
            "object" => "new()",
            "int" or "double" or "bool" => string.Empty,
            var type when type.StartsWith("IReadOnlyList<") => "[]",
            var type when !type.Contains("?") => "new()",
            _ => string.Empty
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

    private PropertyDeclarationSyntax GenerateClassProperty(string propertyName, string propertyType, ConversionOptions options)
    {
        var accessors = options.PropertyAccess switch
        {
            PropertyAccess.Mutable =>
            [
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            ],
            PropertyAccess.Immutable =>
            [
                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
            SyntaxFactory.AccessorDeclaration(SyntaxKind.InitAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            ],
            _ => Array.Empty<AccessorDeclarationSyntax>()
        };

        if (options.IsNullable && !options.IsDefaultInitialized)
        {
            propertyType += "?";
        }

        var propertyDeclaration = SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.ParseTypeName(propertyType),
                SyntaxFactory.Identifier(ToPascalCase(propertyName)))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        if (options.IsRequired)
        {
            propertyDeclaration = propertyDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.RequiredKeyword));
        }

        propertyDeclaration = propertyDeclaration.AddAccessorListAccessors(accessors);

        if (options.IsDefaultInitialized && !options.IsNullable)
        {
            var defaultValue = GetDefaultValue(propertyType);
            if (!string.IsNullOrEmpty(defaultValue))
            {
                propertyDeclaration = propertyDeclaration.WithInitializer(
                    SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression(defaultValue)))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            }
        }

        if (options.AddAttribute)
        {
            var attribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("JsonPropertyName"))
                .AddArgumentListArguments(
                    SyntaxFactory.AttributeArgument(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            SyntaxFactory.Literal(RemoveSpecialCharacters(propertyName)))));

            return propertyDeclaration.AddAttributeLists(
                SyntaxFactory.AttributeList(
                    SyntaxFactory.SingletonSeparatedList(attribute)));
        }

        return propertyDeclaration;
    }

    private ParameterSyntax GenerateRecordParameter(string propertyName, string propertyType, ConversionOptions options)
    {
        if (options.IsNullable)
        {
            propertyType += "?";
        }

        var parameterDeclaration = SyntaxFactory.Parameter(
                SyntaxFactory.Identifier(ToPascalCase(propertyName)))
            .WithType(SyntaxFactory.ParseTypeName(propertyType));

        if (options.IsDefaultInitialized)
        {
            var defaultValue = GetDefaultValue(propertyType);
            if (!string.IsNullOrEmpty(defaultValue))
            {
                parameterDeclaration = parameterDeclaration.WithDefault(
                    SyntaxFactory.EqualsValueClause(SyntaxFactory.ParseExpression(defaultValue)));
            }
        }

        if (options.AddAttribute)
        {
            var attribute = SyntaxFactory.Attribute(
                    SyntaxFactory.IdentifierName("property:JsonPropertyName"))
                .AddArgumentListArguments(
                    SyntaxFactory.AttributeArgument(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            SyntaxFactory.Literal(RemoveSpecialCharacters(propertyName)))));

            var attributeList = SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList(attribute));

            return parameterDeclaration.AddAttributeLists(attributeList);
        }

        return parameterDeclaration;
    }

    private List<ParameterSyntax> CreateParametersFromJson(JsonElement jsonObject,
        List<MemberDeclarationSyntax> declarations, ConversionOptions options)
    {
        var parameters = new List<ParameterSyntax>();

        foreach (var property in jsonObject.EnumerateObject())
        {
            var propertyType = HandlePropertyType(property.Value, property.Name, declarations, AddRecordFromJson, options);
            var parameter = GenerateRecordParameter(property.Name, propertyType, options);
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
            var propertyType = HandlePropertyType(property.Value, property.Name, declarations,
                    options.UseRecords ? AddRecordFromJson : AddClassFromJson, options);
            var propertyDeclaration = GenerateClassProperty(property.Name, propertyType, options);
            properties.Add(propertyDeclaration);
        }

        return properties;
    }

    private string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var sanitizedPropertyName = SanitizePropertyName(input);

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