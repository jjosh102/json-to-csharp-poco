namespace JsonToCsharp.Models;

public class ConversionOptions
{
  public bool GenerateRecords { get; set; } = false;
  public bool MakePropertiesImmutable { get; set; } = false;
  public string Namespace { get; set; } = "JsonToCsharp";
  public string RootTypeName { get; set; } = "Root";
  //todo: styles-> record with accessors or a default style
}