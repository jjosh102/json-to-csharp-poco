using JsonToCsharpPoco.Models;
using JsonToCsharpPoco.Models.Enums;

namespace JsonToCsharpPocoTests;

using JsonToCsharpPoco.Converter;
using Xunit;

public class JsonToCSharpRecordTests
{
    private readonly JsonToCSharp _converter;
    private readonly ConversionOptions _defaultOptions;

    public JsonToCSharpRecordTests()
    {
        _converter = new JsonToCSharp(new CSharpPocoBuilder());
        _defaultOptions = new ConversionOptions
        {
            Namespace = "TestNamespace",
            GenerateRecords = true,
            UsePrimaryConstructor = true,
            RootTypeName = "RootRecord"
        };
    }

    [Fact]
    public void ConvertJsonToSanitizePropertyName_SimpleRecord_ReturnsExpectedRecord()
    {
        string json = @"{
            ""name"": ""John"",
            ""age"": 30,
            ""isEmployee"": true
        }";


        var result = _converter.ConvertJsonToPoco(json, _defaultOptions);

        Assert.Contains("public record RootRecord", result);
        Assert.Contains("string Name", result);
        Assert.Contains("int Age", result);
        Assert.Contains("bool IsEmployee", result);
        Assert.DoesNotContain("{ get; init; }", result);
    }

    [Fact]
    public void ConvertJsonToPoco_NestedRecord_ReturnsNestedRecordStructure()
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

        var result = _converter.ConvertJsonToPoco(json, _defaultOptions);

        Assert.Contains("public record RootRecord", result);
        Assert.Contains("public record Person", result);
        Assert.Contains("public record Address", result);
        Assert.Contains("Person Person", result);
        Assert.Contains("string Street", result);
        Assert.Contains("string City", result);
    }

    [Fact]
    public void ConvertJsonToPoco_ArrayProperty_ReturnsListTypeInRecord()
    {
        string json = @"{
            ""items"": [
                { ""id"": 1, ""value"": ""A"" },
                { ""id"": 2, ""value"": ""B"" }
            ]
        }";

        var result = _converter.ConvertJsonToPoco(json, _defaultOptions);

        Assert.Contains("public record RootRecord", result);
        Assert.Contains("IReadOnlyList<Items> Items", result);
        Assert.Contains("public record Items", result);
        Assert.Contains("int Id", result);
        Assert.Contains("string Value", result);
    }

    [Fact]
    public void ConvertJsonToPoco_DateTimeProperty_ReturnsDateTimeTypeInRecord()
    {
        string json = @"{
            ""createdAt"": ""2024-01-11T10:00:00Z"",
            ""updatedAt"": ""2024-01-11""
        }";

        var result = _converter.ConvertJsonToPoco(json, _defaultOptions);

        Assert.Contains("public record RootRecord", result);
        Assert.Contains("DateTime CreatedAt", result);
        Assert.Contains("DateTime UpdatedAt", result);
    }

    [Fact]
    public void ConvertJsonToPoco_ComplexNestedStructure_GeneratesCorrectRecordHierarchy()
    {
        string json = @"{
            ""company"": {
                ""departments"": [
                    {
                        ""name"": ""IT"",
                        ""employees"": [
                            {
                                ""id"": 1,
                                ""details"": {
                                    ""position"": ""Developer"",
                                    ""skills"": [""C#"", ""JavaScript""]
                                }
                            }
                        ]
                    }
                ]
            }
        }";

        var result = _converter.ConvertJsonToPoco(json, _defaultOptions);

        Assert.Contains("public record RootRecord", result);
        Assert.Contains("public record Company", result);
        Assert.Contains("public record Departments", result);
        Assert.Contains("public record Employees", result);
        Assert.Contains("public record Details", result);
        Assert.Contains("IReadOnlyList<string> Skills", result);
    }

    [Fact]
    public void ConvertJsonToPoco_EmptyObject_ReturnsEmptyRecord()
    {
        string json = "{}";

        var result = _converter.ConvertJsonToPoco(json, _defaultOptions);

        Assert.Contains("public record RootRecord()", result);
        Assert.DoesNotContain("{ get; init; }", result);
    }

    [Fact]
    public void ConvertJsonToPoco_NumericPropertyNames_ConvertsToValidParameterNames()
    {
        string json = @"{
            ""123"": ""value"",
            ""456"": 100
        }";

        var options = new ConversionOptions
        {
            Namespace = "TestNamespace",
            GenerateRecords = true,
            UsePrimaryConstructor = true,
            RootTypeName = "123"
        };
        var result = _converter.ConvertJsonToPoco(json, options);
        Console.WriteLine(result);
        Assert.Contains("public record _123", result);
        Assert.Contains("[property:JsonPropertyName(\"123\")]", result);
        Assert.Contains("[property:JsonPropertyName(\"456\")]", result);
        Assert.Contains("string _123", result);
        Assert.Contains("int _456", result);
    }

    [Fact]
    public void ConvertJsonToPoco_MixedArray_ReturnsObjectListTypeInRecord()
    {
        string json = @"{
            ""data"": [""text"", true, 1]
        }";

        var result = _converter.ConvertJsonToPoco(json, _defaultOptions);

        Assert.Contains("public record RootRecord", result);
        Assert.Contains("IReadOnlyList<object> Data", result);
    }

    [Fact]
    public void ConvertJsonToPoco_SpecialCharacters_HandlesCorrectlyInRecord()
    {
        string json = @"{
            ""@type"": ""person"",
            ""#id"": 123,
            ""$price"": 99.99
        }";

        var result = _converter.ConvertJsonToPoco(json, _defaultOptions);

        Assert.Contains("public record RootRecord", result);
        Assert.Contains("[property:JsonPropertyName(\"type\")]", result);
        Assert.Contains("[property:JsonPropertyName(\"id\")]", result);
        Assert.Contains("[property:JsonPropertyName(\"price\")]", result);
        Assert.Contains("string Type", result);
        Assert.Contains("int Id", result);
        Assert.Contains("double Price", result);
    }
    
    [Fact]
    public void ConvertJsonToRecord_PropertyAccessMutable_GeneratesGettersAndSetters()
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
    public void ConvertJsonToRecord_PropertyAccessImmutable_GeneratesGettersAndInitOnlySetters()
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
}