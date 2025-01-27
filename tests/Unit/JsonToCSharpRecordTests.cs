using JsonToCsharpPoco.Models;
using JsonToCsharpPoco.Models.Enums;

namespace JsonToCsharpPocoTests;

using JsonToCsharpPoco.Converter;
using Xunit;

public class JsonToCSharpRecordTests
{
    private readonly JsonToCSharp _converter;
    private readonly ConversionSettings _defaultOptions;

    public JsonToCSharpRecordTests()
    {
        _converter = new JsonToCSharp(new CSharpPocoBuilder());
        _defaultOptions = new ConversionSettings
        {
            Namespace = "TestNamespace",
            UseRecords = true,
            UsePrimaryConstructor = true,
            RootTypeName = "RootRecord"
        };
    }

    [Fact]
    public void ConvertJsonToEnsureValidPropertyName_SimpleRecord_ReturnsExpectedRecord()
    {
        string json = @"{
            ""name"": ""John"",
            ""age"": 30,
            ""isEmployee"": true
        }";


        var result = _converter.ConvertJsonToCsharp(json, _defaultOptions);

        Assert.Contains("public record RootRecord", result);
        Assert.Contains("string Name", result);
        Assert.Contains("int Age", result);
        Assert.Contains("bool IsEmployee", result);
        Assert.DoesNotContain("{ get; init; }", result);
    }

    [Fact]
    public void ConvertJsonToRecord_NestedRecord_ReturnsNestedRecordStructure()
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
        string json = @"{
            ""items"": [
                { ""id"": 1, ""value"": ""A"" },
                { ""id"": 2, ""value"": ""B"" }
            ]
        }";

        var result = _converter.ConvertJsonToCsharp(json, _defaultOptions);

        Assert.Contains("public record RootRecord", result);
        Assert.Contains("IReadOnlyList<Items> Items", result);
        Assert.Contains("public record Items", result);
        Assert.Contains("int Id", result);
        Assert.Contains("string Value", result);
    }

    [Fact]
    public void ConvertJsonToRecord_DateTimeProperty_ReturnsDateTimeTypeInRecord()
    {
        string json = @"{
            ""createdAt"": ""2024-01-11T10:00:00Z"",
            ""updatedAt"": ""2024-01-11""
        }";

        var result = _converter.ConvertJsonToCsharp(json, _defaultOptions);

        Assert.Contains("public record RootRecord", result);
        Assert.Contains("DateTime CreatedAt", result);
        Assert.Contains("DateTime UpdatedAt", result);
    }

    [Fact]
    public void ConvertJsonToRecord_ComplexNestedStructure_GeneratesCorrectRecordHierarchy()
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

        var result = _converter.ConvertJsonToCsharp(json, _defaultOptions);

        Assert.Contains("public record RootRecord", result);
        Assert.Contains("public record Company", result);
        Assert.Contains("public record Departments", result);
        Assert.Contains("public record Employees", result);
        Assert.Contains("public record Details", result);
        Assert.Contains("IReadOnlyList<string> Skills", result);
    }

    [Fact]
    public void ConvertJsonToRecord_EmptyObject_ReturnsEmptyRecord()
    {
        string json = "{}";

        var result = _converter.ConvertJsonToCsharp(json, _defaultOptions);

        Assert.Contains("public record RootRecord()", result);
        Assert.DoesNotContain("{ get; init; }", result);
    }

    [Fact]
    public void ConvertJsonToRecord_NumericPropertyNames_ConvertsToValidParameterNames()
    {
        string json = @"{
            ""123"": ""value"",
            ""456"": 100
        }";

        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            UseRecords = true,
            UsePrimaryConstructor = true,
            RootTypeName = "123"
        };

        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("public record _123", result);
        Assert.Contains("[property: JsonPropertyName(\"123\")]", result);
        Assert.Contains("[property: JsonPropertyName(\"456\")]", result);
        Assert.Contains("string _123", result);
        Assert.Contains("int _456", result);
    }

    [Fact]
    public void ConvertJsonToRecord_MixedArray_ReturnsObjectListTypeInRecord()
    {
        string json = @"{
            ""data"": [""text"", true, 1]
        }";

        var result = _converter.ConvertJsonToCsharp(json, _defaultOptions);

        Assert.Contains("public record RootRecord", result);
        Assert.Contains("IReadOnlyList<object> Data", result);
    }

    [Fact]
    public void ConvertJsonToRecord_SpecialCharacters_HandlesCorrectlyInRecord()
    {
        string json = @"{
            ""@type"": ""person"",
            ""#id"": 123,
            ""$price"": 99.99
        }";

        var result = _converter.ConvertJsonToCsharp(json, _defaultOptions);

        Assert.Contains("public record RootRecord", result);
        Assert.Contains("[property: JsonPropertyName(\"type\")]", result);
        Assert.Contains("[property: JsonPropertyName(\"id\")]", result);
        Assert.Contains("[property: JsonPropertyName(\"price\")]", result);
        Assert.Contains("string Type", result);
        Assert.Contains("int Id", result);
        Assert.Contains("double Price", result);
    }

    [Fact]
    public void ConvertJsonToRecord_PropertyAccessMutable_GeneratesGettersAndSetters()
    {
        string json = @"{
            ""name"": ""John"",
            ""age"": 30
        }";

        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            UseRecords = false,
            PropertyAccess = PropertyAccess.Mutable
        };


        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("public string Name { get; set; }", result);
        Assert.Contains("public int Age { get; set; }", result);
    }

    [Fact]
    public void ConvertJsonToRecord_AddAttribute_GeneratesGettersAndInitOnlySetters()
    {
        string json = @"{
            ""name"": ""John"",
            ""age"": 30
        }";
        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            UseRecords = false,
            PropertyAccess = PropertyAccess.Immutable
        };

        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("public string Name { get; init; }", result);
        Assert.Contains("public int Age { get; init; }", result);
    }

    [Fact]
    public void ConvertJsonToRecord_AddAttributeUsingPrimaryConstructor_AttributesShouldBeAdded()
    {
        string json = @"{
            ""123"": ""value"",
            ""456"": 100
        }";

        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            UseRecords = true,
            UsePrimaryConstructor = true,
            AddAttribute = true


        };
        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("[property: JsonPropertyName(\"123\")]", result);
        Assert.Contains("[property: JsonPropertyName(\"456\")]", result);

    }

    [Fact]
    public void ConvertJsonToRecord_RemoveAttributesUsingPrimaryConstructor_AttributesShouldNotBeAdded()
    {
        string json = @"{
            ""123"": ""value"",
            ""456"": 100
        }";

        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            UseRecords = true,
            UsePrimaryConstructor = true,
            AddAttribute = false


        };
        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.DoesNotContain("[property:JsonPropertyName(\"123\")]", result);
        Assert.DoesNotContain("[property:JsonPropertyName(\"456\")]", result);
    }

    [Fact]
    public void ConvertJsonToRecord_AddAttribute_AttributesShouldBeAdded()
    {
        string json = @"{
            ""123"": ""value"",
            ""456"": 100
        }";

        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            UseRecords = true,
            UsePrimaryConstructor = false,
            AddAttribute = true


        };
        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("[JsonPropertyName(\"123\")]", result);
        Assert.Contains("[JsonPropertyName(\"456\")]", result);

    }

    [Fact]
    public void ConvertJsonToRecord_RemoveAttributes_AttributesShouldNotBeAdded()
    {
        string json = @"{
            ""123"": ""value"",
            ""456"": 100
        }";

        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            UseRecords = true,
            UsePrimaryConstructor = false,
            AddAttribute = false


        };
        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.DoesNotContain("[JsonPropertyName(\"123\")]", result);
        Assert.DoesNotContain("[JsonPropertyName(\"456\")]", result);
    }

    [Fact]
    public void ConvertJsonToRecord_NullableAndRequiredProperties_GeneratesCorrectSyntax()
    {
        string json = @"{
        ""name"": ""John"",
        ""age"": 30,
        ""email"": """"
    }";

        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            UseRecords = false,
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

    [Fact]
    public void ConvertJsonToRecord_PrimaryConstructor_NullableProperties_GeneratesCorrectSyntax()
    {
        string json = @"{
        ""name"": ""John"",
        ""age"": 30,
        ""email"": """"
    }";

        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            RootTypeName = "RootRecord",
            UseRecords = true,
            UsePrimaryConstructor = true,
            AddAttribute = true,
            IsNullable = true,
            IsRequired = true,
        };

        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("[property: JsonPropertyName(\"name\")]", result);
        Assert.Contains("[property: JsonPropertyName(\"age\")]", result);
        Assert.Contains("[property: JsonPropertyName(\"email\")]", result);
        Assert.Contains("string? Name", result);
        Assert.Contains("int? Age", result);
        Assert.Contains("string? Email", result);


        options.IsNullable = false;
        result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("[property: JsonPropertyName(\"name\")]", result);
        Assert.Contains("[property: JsonPropertyName(\"age\")]", result);
        Assert.Contains("[property: JsonPropertyName(\"email\")]", result);
        Assert.Contains("string Name", result);
        Assert.Contains("int Age", result);
        Assert.Contains("string Email", result);
    }

    [Fact]
    public void ConvertJsonToRecord_DefaultInitializationWithArraysAndObjects_GeneratesDefaultValues()
    {
        string json = @"{
        ""name"": ""John"",
        ""age"": 30,
        ""isActive"": true,
        ""tags"": [""tag1"", ""tag2""],
        ""address"": {
            ""street"": ""Main St"",
            ""city"": ""New York""
        }
    }";

        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            RootTypeName = "RootRecord",
            UseRecords = true,
            UsePrimaryConstructor = false,
            AddAttribute = false,
            IsDefaultInitialized = true
        };

        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("public record RootRecord", result);
        Assert.Contains("public string Name { get; init; } = string.Empty;", result);
        Assert.Contains("public IReadOnlyList<string> Tags { get; init; } = [];", result);
        Assert.Contains("public Address Address { get; init; } = new();", result);
        Assert.Contains("public record Address", result);
        Assert.Contains("public string Street { get; init; } = string.Empty;", result);
        Assert.Contains("public string City { get; init; } = string.Empty;", result);

        options.IsDefaultInitialized = false;
        result = _converter.ConvertJsonToCsharp(json, options);

        Assert.DoesNotContain("= string.Empty", result);
        Assert.DoesNotContain("= [];", result);
        Assert.DoesNotContain("= new();", result);
    }
    [Fact]
    public void ConvertJsonToRecord_ArrayType_RespectsSelectedArrayType()
    {
        string json = @"{
        ""items"": [
            { ""id"": 1, ""value"": ""A"" },
            { ""id"": 2, ""value"": ""B"" }
        ]
    }";

        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            UseRecords = true,
            UsePrimaryConstructor = false,
            ArrayType = ArrayType.List
        };

        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("public List<Items> Items { get; init; }", result);
        Assert.Contains("public record Items", result);
        Assert.Contains("public int Id { get; init; }", result);
        Assert.Contains("public string Value { get; init; }", result);

        options.ArrayType = ArrayType.IReadOnlyList;
        result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("public IReadOnlyList<Items> Items { get; init; }", result);

        options.ArrayType = ArrayType.Array;
        result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("public Items[] Items { get; init; }", result);
    }

    [Fact]
    public void ConvertJsonToRecord_CollectionRoot_ReturnsRecordForFirstObject()
    {
        string json = @"[
        { ""name"": ""John"", ""age"": 30 },
        { ""name"": ""Jane"", ""age"": 25 }
    ]";

        var result = _converter.ConvertJsonToCsharp(json, _defaultOptions);

        Assert.Contains("public record RootRecord", result);
        Assert.Contains("string Name", result);
        Assert.Contains("int Age", result);
    }

    [Fact]
    public void ConvertJsonToRecord_EmptyCollection_ThrowsInvalidOperationException()
    {
        string json = "[]";

        var result = _converter.ConvertJsonToCsharp(json, _defaultOptions);

        Assert.Equal("Error converting JSON: JSON array is empty. Cannot convert to C# POCO.", result);
    }

    [Fact]
    public void ConvertJsonToRecord_CollectionWithNonObjectFirstElement_ThrowsInvalidOperationException()
    {
        string json = @"[42, ""invalid""]";

        var result = _converter.ConvertJsonToCsharp(json, _defaultOptions);

        Assert.Equal("Error converting JSON: First element in JSON array must be an object.", result);
    }

    [Fact]
    public void ConvertJsonToRecord_CollectionWithNestedObjects_ReturnsNestedRecordStructure()
    {
        string json = @"[
        {
            ""person"": {
                ""name"": ""John"",
                ""address"": {
                    ""street"": ""Main St"",
                    ""city"": ""New York""
                }
            }
        }
    ]";

        var result = _converter.ConvertJsonToCsharp(json, _defaultOptions);

        Assert.Contains("public record RootRecord", result);
        Assert.Contains("public record Person", result);
        Assert.Contains("public record Address", result);
        Assert.Contains("Person Person", result);
        Assert.Contains("string Street", result);
        Assert.Contains("string City", result);
    }

    [Fact]
    public void ConvertJsonToRecord_CollectionWithArrayProperty_ReturnsListTypeInRecord()
    {
        string json = @"[
        {
            ""items"": [
                { ""id"": 1, ""value"": ""A"" },
                { ""id"": 2, ""value"": ""B"" }
            ]
        }
    ]";

        var result = _converter.ConvertJsonToCsharp(json, _defaultOptions);

        Assert.Contains("public record RootRecord", result);
        Assert.Contains("IReadOnlyList<Items> Items", result);
        Assert.Contains("public record Items", result);
        Assert.Contains("int Id", result);
        Assert.Contains("string Value", result);
    }

    [Fact]
    public void ConvertJsonToRecord_CollectionWithDateTimeProperty_ReturnsDateTimeTypeInRecord()
    {
        string json = @"[
        {
            ""createdAt"": ""2024-01-11T10:00:00Z"",
            ""updatedAt"": ""2024-01-11""
        }
    ]";

        var result = _converter.ConvertJsonToCsharp(json, _defaultOptions);

        Assert.Contains("public record RootRecord", result);
        Assert.Contains("DateTime CreatedAt", result);
        Assert.Contains("DateTime UpdatedAt", result);
    }

    [Fact]
    public void ConvertJsonToRecord_CollectionWithComplexNestedStructure_GeneratesCorrectRecordHierarchy()
    {
        string json = @"[
        {
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
        }
    ]";

        var result = _converter.ConvertJsonToCsharp(json, _defaultOptions);

        Assert.Contains("public record RootRecord", result);
        Assert.Contains("public record Company", result);
        Assert.Contains("public record Departments", result);
        Assert.Contains("public record Employees", result);
        Assert.Contains("public record Details", result);
        Assert.Contains("IReadOnlyList<string> Skills", result);
    }

    [Fact]
    public void ConvertJsonToRecord_CollectionWithSpecialCharacters_HandlesCorrectlyInRecord()
    {
        string json = @"[
        {
            ""@type"": ""person"",
            ""#id"": 123,
            ""$price"": 99.99
        }
    ]";

        var result = _converter.ConvertJsonToCsharp(json, _defaultOptions);

        Assert.Contains("public record RootRecord", result);
        Assert.Contains("[property: JsonPropertyName(\"type\")]", result);
        Assert.Contains("[property: JsonPropertyName(\"id\")]", result);
        Assert.Contains("[property: JsonPropertyName(\"price\")]", result);
        Assert.Contains("string Type", result);
        Assert.Contains("int Id", result);
        Assert.Contains("double Price", result);
    }

    [Fact]
    public void ConvertJsonToRecord_CollectionWithNullableAndRequiredProperties_GeneratesCorrectSyntax()
    {
        string json = @"[
        {
            ""name"": ""John"",
            ""age"": 30,
            ""email"": """"
        }
    ]";

        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            UseRecords = false,
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

    [Fact]
    public void ConvertJsonToRecord_CollectionWithPrimaryConstructor_NullableProperties_GeneratesCorrectSyntax()
    {
        string json = @"[
        {
            ""name"": ""John"",
            ""age"": 30,
            ""email"": """"
        }
    ]";

        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            RootTypeName = "RootRecord",
            UseRecords = true,
            UsePrimaryConstructor = true,
            AddAttribute = true,
            IsNullable = true,
            IsRequired = true,
        };

        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("[property: JsonPropertyName(\"name\")]", result);
        Assert.Contains("[property: JsonPropertyName(\"age\")]", result);
        Assert.Contains("[property: JsonPropertyName(\"email\")]", result);
        Assert.Contains("string? Name", result);
        Assert.Contains("int? Age", result);
        Assert.Contains("string? Email", result);

        options.IsNullable = false;
        result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("[property: JsonPropertyName(\"name\")]", result);
        Assert.Contains("[property: JsonPropertyName(\"age\")]", result);
        Assert.Contains("[property: JsonPropertyName(\"email\")]", result);
        Assert.Contains("string Name", result);
        Assert.Contains("int Age", result);
        Assert.Contains("string Email", result);
    }

    [Fact]
    public void ConvertJsonToRecord_CollectionWithDefaultInitialization_GeneratesDefaultValues()
    {
        string json = @"[
        {
            ""name"": ""John"",
            ""age"": 30,
            ""isActive"": true,
            ""tags"": [""tag1"", ""tag2""],
            ""address"": {
                ""street"": ""Main St"",
                ""city"": ""New York""
            }
        }
    ]";

        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            RootTypeName = "RootRecord",
            UseRecords = true,
            UsePrimaryConstructor = false,
            AddAttribute = false,
            IsDefaultInitialized = true
        };

        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("public record RootRecord", result);
        Assert.Contains("public string Name { get; init; } = string.Empty;", result);
        Assert.Contains("public IReadOnlyList<string> Tags { get; init; } = [];", result);
        Assert.Contains("public Address Address { get; init; } = new();", result);
        Assert.Contains("public record Address", result);
        Assert.Contains("public string Street { get; init; } = string.Empty;", result);
        Assert.Contains("public string City { get; init; } = string.Empty;", result);

        options.IsDefaultInitialized = false;
        result = _converter.ConvertJsonToCsharp(json, options);

        Assert.DoesNotContain("= string.Empty", result);
        Assert.DoesNotContain("= [];", result);
        Assert.DoesNotContain("= new();", result);
    }

    [Fact]
    public void ConvertJsonToRecord_CollectionWithArrayType_RespectsSelectedArrayType()
    {
        string json = @"[
        {
            ""items"": [
                { ""id"": 1, ""value"": ""A"" },
                { ""id"": 2, ""value"": ""B"" }
            ]
        }
    ]";

        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            UseRecords = true,
            UsePrimaryConstructor = false,
            ArrayType = ArrayType.List
        };

        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("public List<Items> Items { get; init; }", result);
        Assert.Contains("public record Items", result);
        Assert.Contains("public int Id { get; init; }", result);
        Assert.Contains("public string Value { get; init; }", result);

        options.ArrayType = ArrayType.IReadOnlyList;
        result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("public IReadOnlyList<Items> Items { get; init; }", result);

        options.ArrayType = ArrayType.Array;
        result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("public Items[] Items { get; init; }", result);
    }

    [Fact]
    public void ConvertJsonToRecord_PreservesAttributeNames_WithPrimaryConstructor()
    {
        string json = @"{
        ""@type"": ""user"",
        ""#id"": 1001,
        ""$value"": 99.99
    }";

        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            UseRecords = true,
            UsePrimaryConstructor = true,
            AddAttribute = true
        };

        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("[property: JsonPropertyName(\"type\")]", result);
        Assert.Contains("[property: JsonPropertyName(\"id\")]", result);
        Assert.Contains("[property: JsonPropertyName(\"value\")]", result);
        Assert.Contains("string Type", result);
        Assert.Contains("int Id", result);
        Assert.Contains("double Value", result);
    }

    [Fact]
    public void ConvertJsonToRecord_PreservesAttributeNames_WithoutPrimaryConstructor()
    {
        string json = @"{
        ""first-name"": ""John"",
        ""last.name"": ""Doe""
    }";

        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            UseRecords = true,
            UsePrimaryConstructor = false,
            AddAttribute = true
        };

        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("[JsonPropertyName(\"first-name\")]", result);
        Assert.Contains("[JsonPropertyName(\"last.name\")]", result);
        Assert.Contains("public string FirstName { get; init; }", result);
        Assert.Contains("public string LastName { get; init; }", result);
    }

    [Fact]
    public void ConvertJsonToRecord_PreservesNumericPropertyNames()
    {
        string json = @"{
        ""123"": ""value"",
        ""456-item"": true
    }";

        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            UseRecords = true,
            UsePrimaryConstructor = true,
            AddAttribute = true
        };

        var result = _converter.ConvertJsonToCsharp(json, options);
    
        Assert.Contains("[property: JsonPropertyName(\"123\")]", result);
        Assert.Contains("[property: JsonPropertyName(\"456-item\")]", result);
        Assert.Contains("string _123", result);
        Assert.Contains("bool 456Item", result);
    }

    [Fact]
    public void ConvertJsonToRecord_PreservesAttributesInNestedRecords()
    {
        string json = @"{
        ""user"": {
            ""@email"": ""test@example.com"",
            ""!roles"": [""admin"", ""user""]
        }
    }";

        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            UseRecords = true,
            UsePrimaryConstructor = true,
            AddAttribute = true
        };

        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("[property: JsonPropertyName(\"email\")]", result);
        Assert.Contains("[property: JsonPropertyName(\"roles\")]", result);
        Assert.Contains("public record User", result);
        Assert.Contains("string Email", result);
        Assert.Contains("IReadOnlyList<string> Roles", result);
    }

    [Fact]
    public void ConvertJsonToRecord_PreservesExactCasingWithAttributes()
    {
        string json = @"{
        ""FirstName"": ""John"",
        ""lastName"": ""Doe""
    }";

        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            UseRecords = true,
            UsePrimaryConstructor = true,
            AddAttribute = true
        };

        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("[property: JsonPropertyName(\"FirstName\")]", result);
        Assert.Contains("[property: JsonPropertyName(\"lastName\")]", result);
        Assert.Contains("string FirstName", result);
        Assert.Contains("string LastName", result);
    }

    [Fact]
    public void ConvertJsonToRecord_OmitsAttributesWhenDisabled()
    {
        string json = @"{
        ""@type"": ""user"",
        ""special-property"": ""value""
    }";

        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            UseRecords = true,
            UsePrimaryConstructor = true,
            AddAttribute = false
        };

        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.DoesNotContain("JsonPropertyName", result);
        Assert.Contains("string Type", result);
        Assert.Contains("string SpecialProperty", result);
    }

    [Fact]
    public void ConvertJsonToRecord_PreservesAttributesForArrayItems()
    {
        string json = @"{
        ""items"": [
            { ""item-id"": 1, ""item.value"": ""A"" }
        ]
    }";

        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            UseRecords = true,
            UsePrimaryConstructor = true,
            AddAttribute = true
        };

        var result = _converter.ConvertJsonToCsharp(json, options);
     
        Assert.Contains("[property: JsonPropertyName(\"item-id\")]", result);
        Assert.Contains("[property: JsonPropertyName(\"item.value\")]", result);
        Assert.Contains("public record Items", result);
        Assert.Contains("int ItemId", result);
        Assert.Contains("string ItemValue", result);
    }

    [Fact]
    public void ConvertJsonToRecord_PreservesAttributesForReservedKeywords()
    {
        string json = @"{
        ""class"": ""advanced"",
        ""namespace"": ""system""
    }";

        var options = new ConversionSettings
        {
            Namespace = "TestNamespace",
            UseRecords = true,
            UsePrimaryConstructor = true,
            AddAttribute = true
        };

        var result = _converter.ConvertJsonToCsharp(json, options);

        Assert.Contains("[property: JsonPropertyName(\"class\")]", result);
        Assert.Contains("[property: JsonPropertyName(\"namespace\")]", result);
        Assert.Contains("string Class", result);
        Assert.Contains("string Namespace", result);
    }

}