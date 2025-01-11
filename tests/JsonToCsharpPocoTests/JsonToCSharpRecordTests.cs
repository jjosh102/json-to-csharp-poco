namespace JsonToCsharpPocoTests;

using JsonToCsharpPoco.Converter;
using Xunit;

public class JsonToCSharpRecordTests
{
    private readonly JsonToCSharp _converter;

    public JsonToCSharpRecordTests()
    {
        _converter = new JsonToCSharp();
    }

    [Fact]
    public void ConvertJsonToRecord_SimpleRecord_ReturnsExpectedRecord()
    {
        // Arrange
        string json = @"{
            ""name"": ""John"",
            ""age"": 30,
            ""isEmployee"": true
        }";

        // Act
        var result = _converter.ConvertJsonToRecord(json, "PersonRecord");

        // Assert
        Assert.Contains("public record PersonRecord", result);
        Assert.Contains("string Name", result);
        Assert.Contains("int Age", result);
        Assert.Contains("bool IsEmployee", result);
        Assert.DoesNotContain("{ get; set; }", result); 
    }

    [Fact]
    public void ConvertJsonToRecord_NestedRecord_ReturnsNestedRecordStructure()
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
        var result = _converter.ConvertJsonToRecord(json, "RootRecord");

        // Assert
        Assert.Contains("public record RootRecord", result);
        Assert.Contains("public record Person", result);
        Assert.Contains("public record Address", result);
        Assert.Contains("Person Person", result);
        Assert.Contains("string Street", result);
        Assert.Contains("string City", result);
    }

    [Fact]
    public void ConvertJsonToRecord_ArrayProperty_ReturnsListTypeInRecord()
    {
        // Arrange
        string json = @"{
            ""items"": [
                { ""id"": 1, ""value"": ""A"" },
                { ""id"": 2, ""value"": ""B"" }
            ]
        }";

        // Act
        var result = _converter.ConvertJsonToRecord(json, "ContainerRecord");

        // Assert
        Assert.Contains("public record ContainerRecord", result);
        Assert.Contains("IReadOnlyList<Items> Items", result);
        Assert.Contains("public record Items", result);
        Assert.Contains("int Id", result);
        Assert.Contains("string Value", result);
    }

    [Fact]
    public void ConvertJsonToRecord_DateTimeProperty_ReturnsDateTimeTypeInRecord()
    {
        // Arrange
        string json = @"{
            ""createdAt"": ""2024-01-11T10:00:00Z"",
            ""updatedAt"": ""2024-01-11""
        }";

        // Act
        var result = _converter.ConvertJsonToRecord(json, "TimeStampsRecord");

        // Assert
        Assert.Contains("public record TimeStampsRecord", result);
        Assert.Contains("DateTime CreatedAt", result);
        Assert.Contains("DateTime UpdatedAt", result);
    }

    [Fact]
    public void ConvertJsonToRecord_ComplexNestedStructure_GeneratesCorrectRecordHierarchy()
    {
        // Arrange
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

        // Act
        var result = _converter.ConvertJsonToRecord(json, "OrganizationRecord");

        // Assert
        Assert.Contains("public record OrganizationRecord", result);
        Assert.Contains("public record Company", result);
        Assert.Contains("public record Departments", result);
        Assert.Contains("public record Employees", result);
        Assert.Contains("public record Details", result);
        Assert.Contains("IReadOnlyList<string> Skills", result);
    }

    [Fact]
    public void ConvertJsonToRecord_EmptyObject_ReturnsEmptyRecord()
    {
        // Arrange
        string json = "{}";

        // Act
        var result = _converter.ConvertJsonToRecord(json, "EmptyRecord");

        // Assert
        Assert.Contains("public record EmptyRecord()", result);
        Assert.DoesNotContain("{ get; set; }", result);
    }

    [Fact]
    public void ConvertJsonToRecord_NumericPropertyNames_ConvertsToValidParameterNames()
    {
        // Arrange
        string json = @"{
            ""123"": ""value"",
            ""456"": 100
        }";

        // Act
        var result = _converter.ConvertJsonToRecord(json, "NumericPropsRecord");
   
        // Assert
        Assert.Contains("public record NumericPropsRecord", result);
        Assert.Contains("[property :  JsonPropertyName(\"_123\")]", result);
        Assert.Contains("[property :  JsonPropertyName(\"_456\")]", result);
        Assert.Contains("string _123", result);
        Assert.Contains("int _456", result);
    }

    [Fact]
    public void ConvertJsonToRecord_MixedArray_ReturnsObjectListTypeInRecord()
    {
        // Arrange
        string json = @"{
            ""data"": [""text"", true, 1]
        }";

        // Act
        var result = _converter.ConvertJsonToRecord(json, "MixedDataRecord");

        // Assert
        Assert.Contains("public record MixedDataRecord", result);
        Assert.Contains("IReadOnlyList<object> Data", result);
    }

    [Fact]
    public void ConvertJsonToRecord_SpecialCharacters_HandlesCorrectlyInRecord()
    {
        // Arrange
        string json = @"{
            ""@type"": ""person"",
            ""#id"": 123,
            ""$price"": 99.99
        }";

        // Act
        var result = _converter.ConvertJsonToRecord(json, "SpecialPropsRecord");
       
        // Assert
        //todo: remove spaces in the attribute
        Assert.Contains("public record SpecialPropsRecord", result);
        Assert.Contains("[property :  JsonPropertyName(\"type\")]", result);
        Assert.Contains("[property :  JsonPropertyName(\"id\")]", result);
        Assert.Contains("[property :  JsonPropertyName(\"price\")]", result);
        Assert.Contains("string Type", result);
        Assert.Contains("int Id", result);
        Assert.Contains("double Price", result);
    }
}