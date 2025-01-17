# JSON to C# POCO Converter

A simple tool for converting JSON data into C# POCO (Plain Old CLR Object) classes with support for records, customizable property access, nullable types, and various collection types.

## Features

- Convert JSON to C# classes or records
- Support for nested objects and arrays
- Customizable property access patterns
- Optional JSON property name attributes
- Nullable type support
- Required property markers
- Default value initialization
- Multiple array type options
- Primary constructor support for records

## Settings

### Basic Settings

- `Namespace`: Set the namespace for generated classes (default: "JsonToCsharp")
- `RootTypeName`: Set the name for the root class/record (default: "Root")
- `UseRecords`: Generate C# records instead of classes
- `UsePrimaryConstructor`: Use primary constructor syntax for records (C# 9.0+)

### Property Settings

- `PropertyAccess`: Control property accessor patterns
  - `Mutable`: Generate `{ get; set; }` properties
  - `Immutable`: Generate `{ get; init; }` properties (C# 9.0+)

- `ArrayType`: Choose collection type for arrays
  - `IReadOnlyList<T>`: Immutable list interface
  - `List<T>`: Standard mutable list
  - `T[]`: Array type

- `AddAttribute`: Add `[JsonPropertyName]` attributes for JSON serialization
- `IsNullable`: Generate nullable reference types
- `IsRequired`: Add `required` keyword to properties (C# 11.0+)
- `IsDefaultInitialized`: Initialize properties with default values

## Examples

### Basic Class Generation

Input JSON:
```json
{
    "name": "John",
    "age": 30,
    "isEmployee": true
}
```

Settings:
```csharp
var options = new ConversionSettings
{
    Namespace = "MyNamespace",
    UseRecords = false,
    PropertyAccess = PropertyAccess.Mutable
};
```

Output:
```csharp
namespace MyNamespace;

public class Root
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("age")]
    public int Age { get; set; }
    
    [JsonPropertyName("isEmployee")]
    public bool IsEmployee { get; set; }
}
```

### Record with Primary Constructor

Settings:
```csharp
var options = new ConversionSettings
{
    UseRecords = true,
    UsePrimaryConstructor = true,
    AddAttribute = true
};
```

Output:
```csharp
public record Root(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("age")] int Age,
    [property: JsonPropertyName("isEmployee")] bool IsEmployee
);
```

### Nullable and Required Properties

Settings:
```csharp
var options = new ConversionSettings
{
    IsNullable = true,
    IsRequired = true,
    PropertyAccess = PropertyAccess.Immutable
};
```

Output:
```csharp
public class Root
{
    [JsonPropertyName("name")]
    public required string? Name { get; init; }
    
    [JsonPropertyName("age")]
    public required int? Age { get; init; }
}
```

### Array Type Options

Input JSON:
```json
{
    "items": ["A", "B", "C"]
}
```

Settings for different array types:
```csharp
// IReadOnlyList
options.ArrayType = ArrayType.IReadOnlyList;
// Output: public IReadOnlyList<string> Items { get; init; }

// List
options.ArrayType = ArrayType.List;
// Output: public List<string> Items { get; init; }

// Array
options.ArrayType = ArrayType.Array;
// Output: public string[] Items { get; init; }
```

### Default Initialization

Settings:
```csharp
var options = new ConversionSettings
{
    IsDefaultInitialized = true,
    PropertyAccess = PropertyAccess.Immutable
};
```

Output:
```csharp
public class Root
{
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<string> Tags { get; init; } = [];
    public Address Address { get; init; } = new();
}
```

## Type Mapping

The converter automatically maps JSON types to C# types:
- JSON strings → `string`
- JSON numbers → `int` or `double`
- JSON booleans → `bool`
- JSON arrays → `IReadOnlyList<T>`, `List<T>`, or `T[]`
- JSON objects → Nested classes/records
- JSON dates → `DateTime`