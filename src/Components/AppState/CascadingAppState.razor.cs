
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
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
  public Preferences Preferences { get; set; } = new();

  protected override async Task OnInitializedAsync()
  {
    if (await _localStorageService.GetItemAsync<Preferences>(Constants.SavedPreferences) is { } preferences)
    {
      Preferences = preferences;
    }

    await BlazorMonaco.Editor.Global.SetTheme(_jsRuntime, IsDarkTheme ? "vs-dark" : "vs-light");
  }

  public async Task ToggleTheme()
  {
    var currentTheme = IsDarkTheme ? "light" : "dark";
    await UpdatePreferenceAsync(t => t.CurrentTheme = currentTheme, Constants.SavedPreferences);
    await BlazorMonaco.Editor.Global.SetTheme(_jsRuntime, IsDarkTheme ? "vs-dark" : "vs-light");
    StateHasChanged();
  }

  public bool IsDarkTheme => Preferences.CurrentTheme == "dark";

  public async Task UpdatePreferenceAsync(Action<Preferences> updateAction, string storageKey)
  {
    updateAction(Preferences);
    await _localStorageService.SetItemAsync(storageKey, Preferences);
  }

}