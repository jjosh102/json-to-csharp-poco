namespace JsonToCsharpPocoTests;

using global::JsonToCsharpPoco.Converter;
using Xunit;

public class JsonToCSharpTests
{
    private readonly JsonToCSharp _converter;

    public JsonToCSharpTests()
    {
        _converter = new JsonToCSharp();
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
        string expectedClassName = "Person";

        // Act
        var result = _converter.ConvertJsonToClass(json, expectedClassName);

        // Assert
        Assert.Contains("public class Person", result);
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
        var result = _converter.ConvertJsonToClass(invalidJson, "Person");

        // Assert
        Assert.StartsWith("Error converting JSON", result);
    }

    [Fact]
    public void ConvertJsonToClass_EmptyObjectJson_ReturnsEmptyClass()
    {
        // Arrange
        string emptyJson = "{}";
        string expectedClassName = "Empty";

        // Act
        var result = _converter.ConvertJsonToClass(emptyJson, expectedClassName);

        // Assert
        Assert.Contains("public class Empty", result);
        Assert.DoesNotContain("public", result.Replace("public class Empty", ""));
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
        string expectedClassName = "Root";

        // Act
        var result = _converter.ConvertJsonToClass(json, expectedClassName);

        // Assert
        Assert.Contains("public class Root", result);
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
        string expectedClassName = "Root";

        // Act
        var result = _converter.ConvertJsonToClass(json, expectedClassName);

        // Assert
        Assert.Contains("public IReadOnlyList<Items> Items", result);
        Assert.Contains("public class Item", result);
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
        string expectedClassName = "Root";

        // Act
        var result = _converter.ConvertJsonToClass(json, expectedClassName);
            Console.WriteLine(result);

        // Assert
        Assert.Contains("public IReadOnlyList<object> Data", result);
    }
}
