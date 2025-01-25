using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using BlazorMonaco;
using BlazorMonaco.Editor;
using JsonToCsharpPoco.Converter;
using Microsoft.JSInterop;
using JsonToCsharpPoco.Components.AppState;
using JsonToCsharpPoco.Components.Toast;
using JsonToCsharpPoco.Models;
using System.ComponentModel;
using Blazored.LocalStorage;
using JsonToCsharpPoco.Shared;
using JsonToCsharpPoco.Resources;

namespace JsonToCsharpPoco.Components.Pages;

public partial class Index : ComponentBase, IDisposable
{
  private readonly JsonToCSharp _jsonToCSharp;
  private readonly ISyncLocalStorageService _localStorageService;
  private readonly ILocalStorageService _localStorageServiceAsync;
  private readonly IJSRuntime _jsRuntime;

  private ConversionSettings _conversionSettings = new();

  [AllowNull] private StandaloneCodeEditor _jsonEditor;
  [AllowNull] private StandaloneCodeEditor _csharpEditor;

  [CascadingParameter] public required CascadingAppState AppState { get; set; }

  private bool _isConverting;

  public Index(
      JsonToCSharp jsonToCSharp,
      IJSRuntime jsRuntime,
      ISyncLocalStorageService localStorageService,
      ILocalStorageService localStorageServiceAsync)
  {
    _jsonToCSharp = jsonToCSharp;
    _jsRuntime = jsRuntime;
    _localStorageService = localStorageService;
    _localStorageServiceAsync = localStorageServiceAsync;
  }

  protected override async Task OnInitializedAsync()
  {
    await LoadEditorSettings();
    await LoadEditorContent(Constants.JsonEditorContents, _jsonEditor);
    await LoadEditorContent(Constants.CsharpEditorContents, _csharpEditor);

    _conversionSettings.PropertyChanged += OnConversionSettingsChanged;
  }

  private async Task LoadEditorSettings()
  {
    var savedSettings = await _localStorageServiceAsync.GetItemAsync<ConversionSettings>(Constants.SettingsContents);
    if (savedSettings != null && AppState.Preferences.IsSettingsSaved)
    {
      _conversionSettings = savedSettings;
    }
  }

  private async Task LoadEditorContent(string storageKey, StandaloneCodeEditor? editor)
  {
    if (editor is null || !AppState.Preferences.IsEditorContentSaved)
      return;

    var content = await _localStorageServiceAsync.GetItemAsync<string>(storageKey);
    if (!string.IsNullOrWhiteSpace(content))
    {
      await editor!.SetValue(content);
    }
  }

  private void OnConversionSettingsChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (AppState.Preferences.IsSettingsSaved)
    {
      _localStorageService.SetItem(Constants.SettingsContents, _conversionSettings);
    }
  }

  private static StandaloneEditorConstructionOptions CreateEditorOptions(string language)
  {
    return new StandaloneEditorConstructionOptions
    {
      Language = language,
      AutomaticLayout = true,
      FontSize = 12
    };
  }

  private async Task SaveEditorContent(string storageKey, StandaloneCodeEditor editor)
  {
    if (editor is null || !AppState.Preferences.IsEditorContentSaved)
      return;

    var content = await editor.GetValue();
    await _localStorageServiceAsync.SetItemAsStringAsync(storageKey, content);
  }

  public async Task Convert()
  {
    _isConverting = true;
    await Task.Delay(1000);

    var jsonToConvert = await _jsonEditor.GetValue();
    if (string.IsNullOrWhiteSpace(jsonToConvert))
    {
      await ShowToastAsync(Localizer.EnterJson, ToastType.Error, "Error");
      _isConverting = false;
      return;
    }

    if (_jsonToCSharp.TryConvertJsonToCsharp(jsonToConvert, _conversionSettings, out var result))
    {
      await _csharpEditor.SetValue(result);
      await ShowToastAsync(Localizer.JsonConversionSuccess, ToastType.Success, Localizer.ConversionSuccess);
    }
    else
    {
      await ShowToastAsync(Localizer.JsonConversionError, ToastType.Error, $"{Localizer.ConversionFailed} - {result}");
    }

    _isConverting = false;
  }

  public async Task CopyToClipboard()
  {
    var csharpCode = await _csharpEditor.GetValue();
    if (string.IsNullOrWhiteSpace(csharpCode))
      return;

    await _jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", csharpCode);
    await ShowToastAsync(Localizer.ClipboardCopySuccess, ToastType.Success);
  }

  private async Task ShowToastAsync(string message, ToastType type, string title = "", int durationMs = 3000)
  {
    if (AppState.ToastService != null)
    {
      await AppState.ToastService.ShowToastAsync(message, type, title, durationMs);
    }
  }

  private async Task OnEditorContentChanged(ModelContentChangedEvent eventArgs, string storageKey, StandaloneCodeEditor editor) =>
    await SaveEditorContent(storageKey, editor);

  private async Task OnJsonDidChangeModelContent(ModelContentChangedEvent eventArgs) =>
    await OnEditorContentChanged(eventArgs, Constants.JsonEditorContents, _jsonEditor);


  private async Task OnCsharpDidChangeModelContent(ModelContentChangedEvent eventArgs) =>
    await OnEditorContentChanged(eventArgs, Constants.CsharpEditorContents, _csharpEditor);


  public void Dispose() =>
    _conversionSettings.PropertyChanged -= OnConversionSettingsChanged;
}
