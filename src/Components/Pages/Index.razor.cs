using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using BlazorMonaco;
using BlazorMonaco.Editor;
using BlazorMonaco.Languages;
using JsonToCsharpPoco.Converter;
using Microsoft.JSInterop;
using JsonToCsharpPoco.Components.AppState;
using JsonToCsharpPoco.Components.Toast;
namespace JsonToCsharpPoco.Components.Pages;

public partial class Index : ComponentBase
{

  private readonly JsonToCSharp _jsonToCSharp;

  private readonly IJSRuntime _jsRuntime;

  [AllowNull]
  private StandaloneCodeEditor _jsonEditor;

  [AllowNull]
  private StandaloneCodeEditor _csharpEditor;

  [CascadingParameter]
  public required CascadingAppState AppState { get; set; }

  public Index(JsonToCSharp jsonToCSharp, IJSRuntime jsRuntime)
  {
    _jsonToCSharp = jsonToCSharp;
    _jsRuntime = jsRuntime;
  }

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

  private async Task CsharpEditorOnDidInit()
  {
    await _csharpEditor.AddCommand((int)KeyMod.CtrlCmd | (int)KeyCode.KeyC, async (args) =>
    {
      var csharp = await _csharpEditor.GetValue();
      if (string.IsNullOrWhiteSpace(csharp))
        return;

      await _jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", csharp);
      await AppState.ToastService!.ShowToastAsync(
            message: "C# code copied to clipboard",
            type: ToastType.Success,
            title: "",
            durationMs: 3000
        );
    });
  }

  public async Task Convert()
  {
    var jsonToConvert = await _jsonEditor.GetValue();

    if (string.IsNullOrWhiteSpace(jsonToConvert))
    {
      await AppState.ToastService!.ShowToastAsync(
            message: "Please enter JSON to convert",
            type: ToastType.Error,
            title: "Error",
            durationMs: 3000
        );
      return;
    }

    var csharp = _jsonToCSharp.ConvertJsonToRecord(jsonToConvert, "Root");
    await _csharpEditor.SetValue(csharp);
    await AppState.ToastService!.ShowToastAsync(
            message: "JSON converted to C# POCO",
            type: ToastType.Success,
            title: "Conversion Successful",
            durationMs: 3000
        );
  }
}