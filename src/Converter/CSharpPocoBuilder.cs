using System.Text.Json;
using JsonToCsharpPoco.Models;
using JsonToCsharpPoco.Extensions;
using JsonToCsharpPoco.Models.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Humanizer;
namespace JsonToCsharpPoco.Converter;

public partial class CSharpPocoBuilder
{
    public string Build(JsonElement rootElement, ConversionSettings options)
    {
        var declarations = new List<MemberDeclarationSyntax>();
        AddTypeFromJson(rootElement, options.RootTypeName, declarations, options);

        BaseNamespaceDeclarationSyntax namespaceDeclaration = options.UseFileScoped
            ? SyntaxFactory.FileScopedNamespaceDeclaration(SyntaxFactory.ParseName(options.Namespace))
            : SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(options.Namespace)).WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
              .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken));

        namespaceDeclaration = namespaceDeclaration.AddMembers([.. declarations]);

        var compilationUnit = SyntaxFactory.CompilationUnit()
        .AddMembers(namespaceDeclaration);

        if (options.AddAttribute)
        {
            compilationUnit = compilationUnit.AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json.Serialization")));
        }

        return compilationUnit.NormalizeWhitespace().ToFullString();

    }

    private void AddTypeFromJson(JsonElement jsonObject,
       string typeName,
       List<MemberDeclarationSyntax> declarations,
       ConversionSettings options)
    {
        typeName = typeName.EnsureValidPropertyName();

        MemberDeclarationSyntax typeDeclaration;

        if (!options.UseRecords)
        {
            var classDeclaration = SyntaxFactory.ClassDeclaration(typeName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            var members = CreatePropertiesFromJson(jsonObject, declarations, options);
            classDeclaration = classDeclaration.AddMembers([.. members]);

            typeDeclaration = classDeclaration;
        }
        else
        {
            var recordDeclaration = SyntaxFactory.RecordDeclaration(SyntaxFactory.Token(SyntaxKind.RecordKeyword), typeName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            if (options.UsePrimaryConstructor)
            {
                var parameters = CreateParametersFromJson(jsonObject, declarations, options);
                recordDeclaration = recordDeclaration
                    .AddParameterListParameters([.. parameters])
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            }
            else
            {
                var members = CreatePropertiesFromJson(jsonObject, declarations, options);
                recordDeclaration = recordDeclaration
                    .WithOpenBraceToken(SyntaxFactory.Token(SyntaxKind.OpenBraceToken))
                    .AddMembers([.. members])
                    .WithCloseBraceToken(SyntaxFactory.Token(SyntaxKind.CloseBraceToken));
            }

            typeDeclaration = recordDeclaration;
        }

        declarations.Add(typeDeclaration);
    }

    private string HandlePropertyType(JsonElement propertyValue,
        string propertyName,
        List<MemberDeclarationSyntax> declarations,
        Action<JsonElement, string, List<MemberDeclarationSyntax>, ConversionSettings> addNestedTypeAction,
        ConversionSettings options)
    {
        string propertyType = DeterminePropertyType(propertyValue, propertyName, options.ArrayType);

        if (propertyValue.ValueKind == JsonValueKind.Object)
        {
            var nestedTypeName = propertyName.ToPascalCase();
            addNestedTypeAction(propertyValue, nestedTypeName, declarations, options);
            return nestedTypeName;
        }

        if (propertyValue.ValueKind == JsonValueKind.Array &&
            propertyValue.EnumerateArray().Any() &&
            propertyValue[0].ValueKind == JsonValueKind.Object)
        {
            var nestedTypeName = propertyName.ToPascalCase();
            addNestedTypeAction(propertyValue[0], nestedTypeName, declarations, options);
            return FormatArrayType(nestedTypeName, options.ArrayType);
        }

        return propertyType;
    }

    private string DeterminePropertyType(JsonElement value, string propertyName, ArrayType arrayType)
    {
        return value.ValueKind switch
        {
            JsonValueKind.Number => value.TryGetInt32(out _) ? "int" : "double",
            JsonValueKind.String => DateTime.TryParse(value.GetString(), out _) ? "DateTime" : "string",
            JsonValueKind.True => "bool",
            JsonValueKind.False => "bool",
            JsonValueKind.Array => DetermineArrayType(value, propertyName, arrayType),
            JsonValueKind.Object => propertyName.ToPascalCase(),
            _ => "object"
        };
    }

    private string GetDefaultValue(string propertyType)
    {
        return propertyType switch
        {
            "string" => "string.Empty",
            "object" => "new()",
            "DateTime" => string.Empty,
            "int" or "double" or "bool" => string.Empty,
            var type when type.StartsWith("IReadOnlyList") || type.StartsWith("List") || type.EndsWith("[]") => "[]",
            var type when !type.Contains("?") => "new()",
            _ => string.Empty
        };
    }

    private string DetermineArrayType(JsonElement array, string propertyName, ArrayType arrayType)
    {
        string elementType;

        if (!array.EnumerateArray().Any())
        {

            elementType = "object";
        }
        else
        {
            var elementTypes = array.EnumerateArray()
                .Select(element => DeterminePropertyType(element, propertyName, arrayType))
                .Distinct()
                .ToList();

            elementType = elementTypes.Count == 1 ? elementTypes.First() : "object";
        }

        return FormatArrayType(elementType, arrayType);
    }

    private string FormatArrayType(string elementType, ArrayType arrayType)
    {
        return arrayType switch
        {
            ArrayType.IReadOnlyList => $"IReadOnlyList<{elementType}>",
            ArrayType.List => $"List<{elementType}>",
            ArrayType.Array => $"{elementType}[]",
            _ => throw new ArgumentOutOfRangeException(nameof(arrayType), arrayType, null)
        };
    }


    private PropertyDeclarationSyntax GenerateClassProperty(string propertyName, string propertyType, ConversionSettings options)
    {
        if (options.IsNullable && !options.IsDefaultInitialized)
        {
            propertyType += "?";
        }

        var accessors = GenerateAccessors(options);

        var propertyDeclaration = SyntaxFactory.PropertyDeclaration(
                SyntaxFactory.ParseTypeName(propertyType),
                SyntaxFactory.Identifier(propertyName.ToPascalCase()))
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
            var attribute = CreateJsonPropertyNameAttribute(propertyName);
            return propertyDeclaration.AddAttributeLists(
                SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(attribute)));
        }

        return propertyDeclaration;
    }


    private AccessorDeclarationSyntax[] GenerateAccessors(ConversionSettings options)
    {
        return options.PropertyAccess switch
        {
            PropertyAccess.Mutable =>
            [
            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
        ],
            PropertyAccess.Immutable =>
            [
            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
            SyntaxFactory.AccessorDeclaration(SyntaxKind.InitAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
        ],
            _ => []
        };
    }


    private AttributeSyntax CreateJsonPropertyNameAttribute(string propertyName)
    {
        var argument = SyntaxFactory.AttributeArgument(
            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(propertyName.RemoveSpecialCharacters())));

        return SyntaxFactory.Attribute(SyntaxFactory.ParseName("JsonPropertyName")).AddArgumentListArguments(argument);
    }


    private ParameterSyntax GenerateRecordParameter(string propertyName, string propertyType, ConversionSettings options)
    {
        if (options.IsNullable)
        {
            propertyType += "?";
        }

        var parameterDeclaration = SyntaxFactory.Parameter(
                SyntaxFactory.Identifier(propertyName.ToPascalCase()))
            .WithType(SyntaxFactory.ParseTypeName(propertyType));

        if (!options.AddAttribute)
            return parameterDeclaration;

        var attribute = SyntaxFactory.Attribute(
                   SyntaxFactory.IdentifierName("property: JsonPropertyName"))
               .AddArgumentListArguments(
                   SyntaxFactory.AttributeArgument(
                       SyntaxFactory.LiteralExpression(
                           SyntaxKind.StringLiteralExpression,
                           SyntaxFactory.Literal(propertyName.RemoveSpecialCharacters()))));

        var attributeList = SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(attribute));

        return parameterDeclaration.AddAttributeLists(attributeList);
    }

    private List<ParameterSyntax> CreateParametersFromJson(JsonElement jsonObject,
        List<MemberDeclarationSyntax> declarations, ConversionSettings options)
    {
        var parameters = new List<ParameterSyntax>();

        foreach (var property in jsonObject.EnumerateObject())
        {
            var propertyType = HandlePropertyType(property.Value, property.Name, declarations, AddTypeFromJson, options);
            var parameter = GenerateRecordParameter(property.Name, propertyType, options);
            parameters.Add(parameter);
        }

        return parameters;
    }

    private List<MemberDeclarationSyntax> CreatePropertiesFromJson(JsonElement jsonObject,
        List<MemberDeclarationSyntax> declarations, ConversionSettings options)
    {
        var properties = new List<MemberDeclarationSyntax>();

        foreach (var property in jsonObject.EnumerateObject())
        {
            var propertyType = HandlePropertyType(property.Value, property.Name, declarations, AddTypeFromJson, options);
            var propertyDeclaration = GenerateClassProperty(property.Name, propertyType, options);
            properties.Add(propertyDeclaration);
        }

        return properties;
    }


}