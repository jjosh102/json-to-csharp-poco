using JsonToCsharpPoco.Models;
using JsonToCsharpPoco.Models.Enums;

namespace JsonToCsharpPocoTests;

using JsonToCsharpPoco.Converter;
using Xunit;

public class JsonToCSsharpClassTests
{
    private readonly JsonToCSharp _converter;
    private readonly ConversionOptions _defaultOptions;

    public JsonToCSsharpClassTests()
    {
        _converter = new JsonToCSharp(new CSharpPocoBuilder());
        _defaultOptions = new ConversionOptions
        {
            Namespace = "TestNamespace",
            GenerateRecords = false,
            RootTypeName = "RootClass"
        };
    }

    [Fact]
    public void ConvertJsonToClass_ValidJson_ReturnsExpectedClass()
    {
        string json = @"{
            ""name"": ""John"",
            ""age"": 30,
            ""isEmployee"": true
        }";

        var result = _converter.ConvertJsonToCsharp(json, _defaultOptions);

        Assert.Contains("public class RootClass", result);
        Assert.Contains("public string Name", result);
        Assert.Contains("public int Age", result);
        Assert.Contains("public bool IsEmployee", result);
    }

    [Fact]
    public void ConvertJsonToClass_InvalidJson_ThrowsError()
    {

        string invalidJson = @"{ name: John }";

        var result = _converter.ConvertJsonToCsharp(invalidJson, _defaultOptions);

        Assert.StartsWith("Error converting JSON", result);
    }

    [Fact]
    public void ConvertJsonToClass_EmptyObjectJson_ReturnsEmptyClass()
    {
        string emptyJson = "{}";

        var result = _converter.ConvertJsonToCsharp(emptyJson, _defaultOptions);

        Assert.Contains("public class RootClass", result);
        Assert.DoesNotContain("public", result.Replace("public class RootClass", ""));
    }

    [Fact]
    public void ConvertJsonToClass_NestedObject_ReturnsNestedClasses()
    {

        string json = @"{
            ""person"": {
                ""name"": ""John"",
                ""address"": {
                    ""street"": ""Main St"",
                    ""city"": ""New York""
                }
            }
        }";

        var result = _converter.ConvertJsonToCsharp(json, _defaultOptions);

        Assert.Contains("public class RootClass", result);
        Assert.Contains("public Person Person", result);
        Assert.Contains("public class Person", result);
        Assert.Contains("public Address Address", result);
        Assert.Contains("public class Address", result);
    }

    [Fact]
    public void ConvertJsonToClass_ArrayProperty_ReturnsListType()
    {

        string json = @"{
            ""items"": [
                { ""id"": 1, ""value"": ""A"" },
                { ""id"": 2, ""value"": ""B"" }
            ]
        }";

        var result = _converter.ConvertJsonToCsharp(json, _defaultOptions);

        Assert.Contains("public IReadOnlyList<Items> Items", result);
        Assert.Contains("public class Items", result);
        Assert.Contains("public int Id", result);
        Assert.Contains("public string Value", result);
    }

    [Fact]
    public void ConvertJsonToClass_MixedArray_ReturnsObjectType()
    {

        string json = @"{
            ""data"": [""text"", true, 1]
        }";

        var result = _converter.ConvertJsonToCsharp(json, _defaultOptions);

        Assert.Contains("public IReadOnlyList<object> Data", result);
    }

    [Fact]
    public void ConvertJsonToClass_NumericPropertyName_ConvertsToValidStringPropertyName()
    {

        string json = @"{
            ""123"": ""value"",
            ""456"": 100
        }";

        var options = new ConversionOptions
        {
            Namespace = "TestNamespace",
            GenerateRecords = false,
            RootTypeName = "123"
        };

        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("public class _123", result);
        Assert.Contains("public string _123", result);
        Assert.Contains("public int _456", result);
    }
    [Fact]
    public void ConvertJsonToClass_PropertyAccessMutable_GeneratesGettersAndSetters()
    {

        string json = @"{
            ""name"": ""John"",
            ""age"": 30
        }";

        var options = new ConversionOptions
        {
            Namespace = "TestNamespace",
            GenerateRecords = false,
            PropertyAccess = PropertyAccess.Mutable
        };

        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("public string Name { get; set; }", result);
        Assert.Contains("public int Age { get; set; }", result);
    }

    [Fact]
    public void ConvertJsonToClass_PropertyAccessImmutable_GeneratesGettersAndInitOnlySetters()
    {

        string json = @"{
            ""name"": ""John"",
            ""age"": 30
        }";
        var options = new ConversionOptions
        {
            Namespace = "TestNamespace",
            GenerateRecords = false,
            PropertyAccess = PropertyAccess.Immutable
        };

        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("public string Name { get; init; }", result);
        Assert.Contains("public int Age { get; init; }", result);
    }

    [Fact]
    public void ConvertJsonToClass_AddAttribute_AttributesShouldBeAdded()
    {
        string json = @"{
            ""123"": ""value"",
            ""456"": 100
        }";

        var options = new ConversionOptions
        {
            Namespace = "TestNamespace",
            GenerateRecords = false,
            AddAttribute = true


        };
        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("[JsonPropertyName(\"123\")]", result);
        Assert.Contains("[JsonPropertyName(\"456\")]", result);

    }

    [Fact]
    public void ConvertJsonToClass_RemoveAttributes_AttributesShouldNotBeAdded()
    {
        string json = @"{
            ""123"": ""value"",
            ""456"": 100
        }";

        var options = new ConversionOptions
        {
            Namespace = "TestNamespace",
            GenerateRecords = false,
            AddAttribute = false


        };
        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.DoesNotContain("[JsonPropertyName(\"123\")]", result);
        Assert.DoesNotContain("[JsonPropertyName(\"456\")]", result);

    }

    [Fact]
    public void ConvertJsonToClass_NullableAndRequiredProperties_GeneratesCorrectSyntax()
    {
        string json = @"{
        ""name"": ""John"",
        ""age"": 30,
        ""email"": """"
    }";

        var options = new ConversionOptions
        {
            Namespace = "TestNamespace",
            GenerateRecords = false,
            UsePrimaryConstructor = false,
            AddAttribute = false,
            IsNullable = true,
            IsRequired = true,
            PropertyAccess = PropertyAccess.Immutable
        };

        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("public required string? Name { get; init; }", result);
        Assert.Contains("public required int? Age { get; init; }", result);
        Assert.Contains("public required string? Email { get; init; }", result);


        options.IsNullable = false;
        result = _converter.ConvertJsonToCsharp(json, options);
        Assert.Contains("public required string Name { get; init; }", result);
        Assert.Contains("public required int Age { get; init; }", result);
    }
}