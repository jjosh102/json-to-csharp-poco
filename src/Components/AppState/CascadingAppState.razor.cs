
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using JsonToCsharpPoco.Components.Toast;
using JsonToCsharpPoco.Shared;
using Blazored.LocalStorage;
using JsonToCsharpPoco.Models;

namespace JsonToCsharpPoco.Components.AppState;
public partial class CascadingAppState : ComponentBase
{
  private readonly IJSRuntime _jsRuntime;
  private readonly ILocalStorageService _localStorageService;

  public CascadingAppState(IJSRuntime jsRuntime, ILocalStorageService localStorageService)
  {
    _jsRuntime = jsRuntime;
    _localStorageService = localStorageService;
  }


  [Parameter]
  public RenderFragment? ChildContent { get; set; }
  public ToastComponent? ToastService { get; set; }
  public bool IsSettingsSaved { get; set; }
  public bool IsEditorContentSaved { get; set; }
  private string _currentTheme = "light";

  protected override async Task OnInitializedAsync()
  {
    if (await _localStorageService.GetItemAsync<bool>(Constants.SaveSettings) is { } isSettingsSaved)
    {
      IsSettingsSaved = isSettingsSaved;
    }

    if (await _localStorageService.GetItemAsync<bool>(Constants.SaveEditorContents) is { } isEditorContentSaved)
    {
      IsEditorContentSaved = isEditorContentSaved;
    }

    if (await _localStorageService.GetItemAsync<string>(Constants.ThemeKey) is { } theme)
    {
      _currentTheme = theme;
    }
    else
    {
      _currentTheme = await _jsRuntime.InvokeAsync<string>("getSystemTheme");
      await _localStorageService.SetItemAsync(Constants.ThemeKey, _currentTheme);
    }

    await BlazorMonaco.Editor.Global.SetTheme(_jsRuntime, IsDarkTheme ? "vs-dark" : "vs-light");
  }

  public void ToggleTheme()
  {
    _currentTheme = _currentTheme == "light" ? "dark" : "light";
    StateHasChanged();
  }

  public bool IsDarkTheme => _currentTheme == "dark";

}