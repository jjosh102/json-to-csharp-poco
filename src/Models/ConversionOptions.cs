using JsonToCsharpPoco.Models.Enums;

namespace JsonToCsharpPoco.Models;

public class ConversionOptions
{
    public bool GenerateRecords { get; set; } 
    public PropertyAccess PropertyAccess { get; set; }
    public string Namespace { get; set; } = "JsonToCsharp";
    public string RootTypeName { get; set; } = "Root";
    
    public bool UsePrimaryConstructor { get; set; } 
    //todo: Add option to add attributes or not
}