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
        // Arrange
        string json = @"{
            ""name"": ""John"",
            ""age"": 30,
            ""isEmployee"": true
        }";

        // Act
        var result = _converter.ConvertJsonToPoco(json, _defaultOptions);

        // Assert
        Assert.Contains("public class RootClass", result);
        Assert.Contains("public string Name", result);
        Assert.Contains("public int Age", result);
        Assert.Contains("public bool IsEmployee", result);
    }

    [Fact]
    public void ConvertJsonToClass_InvalidJson_ThrowsError()
    {
        // Arrange
        string invalidJson = @"{ name: John }";

        // Act
        var result = _converter.ConvertJsonToPoco(invalidJson, _defaultOptions);

        // Assert
        Assert.StartsWith("Error converting JSON", result);
    }

    [Fact]
    public void ConvertJsonToClass_EmptyObjectJson_ReturnsEmptyClass()
    {
        // Arrange
        string emptyJson = "{}";

        // Act
        var result = _converter.ConvertJsonToPoco(emptyJson, _defaultOptions);

        // Assert
        Assert.Contains("public class RootClass", result);
        Assert.DoesNotContain("public", result.Replace("public class RootClass", ""));
    }

    [Fact]
    public void ConvertJsonToClass_NestedObject_ReturnsNestedClasses()
    {
        // Arrange
        string json = @"{
            ""person"": {
                ""name"": ""John"",
                ""address"": {
                    ""street"": ""Main St"",
                    ""city"": ""New York""
                }
            }
        }";

        // Act
        var result = _converter.ConvertJsonToPoco(json, _defaultOptions);

        // Assert
        Assert.Contains("public class RootClass", result);
        Assert.Contains("public Person Person", result);
        Assert.Contains("public class Person", result);
        Assert.Contains("public Address Address", result);
        Assert.Contains("public class Address", result);
    }

    [Fact]
    public void ConvertJsonToClass_ArrayProperty_ReturnsListType()
    {
        // Arrange
        string json = @"{
            ""items"": [
                { ""id"": 1, ""value"": ""A"" },
                { ""id"": 2, ""value"": ""B"" }
            ]
        }";

        // Act
        var result = _converter.ConvertJsonToPoco(json, _defaultOptions);

        // Assert
        Assert.Contains("public IReadOnlyList<Items> Items", result);
        Assert.Contains("public class Items", result);
        Assert.Contains("public int Id", result);
        Assert.Contains("public string Value", result);
    }

    [Fact]
    public void ConvertJsonToClass_MixedArray_ReturnsObjectType()
    {
        // Arrange
        string json = @"{
            ""data"": [""text"", true, 1]
        }";

        // Act
        var result = _converter.ConvertJsonToPoco(json, _defaultOptions);

        // Assert
        Assert.Contains("public IReadOnlyList<object> Data", result);
    }

    [Fact]
    public void ConvertJsonToClass_NumericPropertyName_ConvertsToValidStringPropertyName()
    {
        // Arrange
        string json = @"{
            ""123"": ""value"",
            ""456"": 100
        }";

        var options  = new ConversionOptions
        {
            Namespace = "TestNamespace",
            GenerateRecords = false,
            RootTypeName = "123"
        };

        // Act
        var result = _converter.ConvertJsonToPoco(json, options);

        // Assert
        Assert.Contains("public class _123", result);
        Assert.Contains("public string _123", result);
        Assert.Contains("public int _456", result);
    }
    [Fact]
    public void ConvertJsonToClass_PropertyAccessMutable_GeneratesGettersAndSetters()
    {
        // Arrange
        string json = @"{
            ""name"": ""John"",
            ""age"": 30
        }";
        
        var options  = new ConversionOptions
        {
            Namespace = "TestNamespace",
            GenerateRecords = false,
            PropertyAccess = PropertyAccess.Mutable
        };
        
        // Act
        var result = _converter.ConvertJsonToPoco(json, options);

        // Assert
        Assert.Contains("public string Name { get; set; }", result);
        Assert.Contains("public int Age { get; set; }", result);
    }

    [Fact]
    public void ConvertJsonToClass_PropertyAccessImmutable_GeneratesGettersAndInitOnlySetters()
    {
        // Arrange
        string json = @"{
            ""name"": ""John"",
            ""age"": 30
        }";
        var options  = new ConversionOptions
        {
            Namespace = "TestNamespace",
            GenerateRecords = false,
            PropertyAccess = PropertyAccess.Immutable
        };

        // Act
        var result = _converter.ConvertJsonToPoco(json, options);

        // Assert
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
        var result = _converter.ConvertJsonToPoco(json, options);

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
        var result = _converter.ConvertJsonToPoco(json, options);

        Assert.DoesNotContain("[JsonPropertyName(\"123\")]", result);
        Assert.DoesNotContain("[JsonPropertyName(\"456\")]", result);
    
    }
}