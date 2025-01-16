using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using BlazorMonaco;
using BlazorMonaco.Editor;
using BlazorMonaco.Languages;
using JsonToCsharpPoco.Converter;
using Microsoft.JSInterop;
using JsonToCsharpPoco.Components.AppState;
using JsonToCsharpPoco.Components.Toast;
using JsonToCsharpPoco.Models;
using System.ComponentModel;
using Blazored.LocalStorage;
using JsonToCsharpPoco.Shared;

namespace JsonToCsharpPoco.Components.Pages;

public partial class Index : ComponentBase, IDisposable
{
  private readonly JsonToCSharp _jsonToCSharp;
  private readonly ISyncLocalStorageService _localStorageService;
  private readonly ILocalStorageService _localStorageServiceAsync;
  private ConversionOptions _conversionOptions = new();

  private readonly IJSRuntime _jsRuntime;

  [AllowNull]
  private StandaloneCodeEditor _jsonEditor;

  [AllowNull]
  private StandaloneCodeEditor _csharpEditor;

  [CascadingParameter]
  public required CascadingAppState AppState { get; set; }

  private bool _isConverting = false;

  public Index(JsonToCSharp jsonToCSharp, IJSRuntime jsRuntime, ISyncLocalStorageService localStorageService, ILocalStorageService localStorageServiceAsync)
  {
    _jsonToCSharp = jsonToCSharp;
    _jsRuntime = jsRuntime;
    _localStorageService = localStorageService;
    _localStorageServiceAsync = localStorageServiceAsync;
  }

  protected override async Task OnInitializedAsync()
  {
    if (await _localStorageServiceAsync.GetItemAsync<ConversionOptions>(Constants.OptionsValue) is { } savedOptions
        && AppState.IsOptionsSave)
    {
      _conversionOptions = savedOptions;
    }
    _conversionOptions.PropertyChanged += OnConversionOptionsChanged;

  }

  private void OnConversionOptionsChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (AppState.IsOptionsSave)
    {
      _localStorageService.SetItem(Constants.OptionsValue, _conversionOptions);
    }
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

  public async Task Convert()
  {
    _isConverting = true;
    await Task.Delay(1000);
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

    if (_jsonToCSharp.TryConvertJsonToCsharp(jsonToConvert, _conversionOptions, out var syntax))
    {
      await _csharpEditor.SetValue(syntax);
      await AppState.ToastService!.ShowToastAsync(
        message: "JSON converted to C# POCO",
        type: ToastType.Success,
        title: "Conversion Successful",
        durationMs: 3000
      );
    }
    else
    {
      await AppState.ToastService!.ShowToastAsync(
        message: "Error converting JSON to C# POCO",
        type: ToastType.Error,
        title: "Conversion Failed",
        durationMs: 3000
      );
    }

    _isConverting = false;
  }

  public async Task CopyToClipboard()
  {
    var csharp = await _csharpEditor.GetValue();
    if (string.IsNullOrWhiteSpace(csharp))
      return;

    await _jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", csharp);
    await AppState.ToastService!.ShowToastAsync(
          message: "C# code copied to clipboard",
          type: ToastType.Success,
          title: "",
          durationMs: 2000
      );
  }

  public void Dispose()
  {
    _conversionOptions.PropertyChanged -= OnConversionOptionsChanged;
  }
}