
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using JsonToCsharpPoco.Components.Toast;

namespace JsonToCsharpPoco.Components.AppState;
public partial class CascadingAppState : ComponentBase
{
  private readonly IJSRuntime _jsRuntime;

  public CascadingAppState(IJSRuntime jsRuntime) => _jsRuntime = jsRuntime;

  [Parameter]
  public RenderFragment? ChildContent { get; set; }
  public ToastComponent? ToastComponentService;
  private string _currentTheme = "light";
  protected override async Task OnInitializedAsync()
  {
    //todo:support local storage
    _currentTheme = await _jsRuntime.InvokeAsync<string>("getSystemTheme");
    await BlazorMonaco.Editor.Global.SetTheme(_jsRuntime, IsDarkTheme ? "vs-dark" : "vs-light");
  }

  public void ToggleTheme()
  {
    _currentTheme = _currentTheme == "light" ? "dark" : "light";
    StateHasChanged();
  }

  public bool IsDarkTheme => _currentTheme == "dark";

}