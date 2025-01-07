using System.Diagnostics.CodeAnalysis;
using BlazorMonaco;
using BlazorMonaco.Editor;
using BlazorMonaco.Languages;
using JsonToCsharpPoco.Converter;
using Microsoft.JSInterop;
namespace JsonToCsharpPoco.Components.Pages;

public partial class Index
{

  private readonly JsonToCSharp _jsonToCSharp;

  [AllowNull]
  private StandaloneCodeEditor _jsonEditor;

  [AllowNull]
  private StandaloneCodeEditor _csharpEditor;

  public Index(JsonToCSharp jsonToCSharp) => _jsonToCSharp = jsonToCSharp;

  private static StandaloneEditorConstructionOptions JsonEditorConstructionOptions(StandaloneCodeEditor editor)
  {
    return new StandaloneEditorConstructionOptions
    {
      Language = "json",
      AutomaticLayout = true, 
      FontSize = 12

    };
  }

  private static StandaloneEditorConstructionOptions CsharpEditorConstructionOptions(StandaloneCodeEditor editor)
  {
    return new StandaloneEditorConstructionOptions
    {
      Language = "csharp",
      AutomaticLayout = true,
      FontSize = 12
    };
  }

  public async Task Convert()
  {
    var json = await _jsonEditor.GetValue();
    Console.WriteLine(json);
    if (json == null)
      return;

    var csharp = _jsonToCSharp.ConvertJsonToClass(json, "Root");
    Console.WriteLine(csharp);
    await _csharpEditor.SetValue(csharp);
  }
}