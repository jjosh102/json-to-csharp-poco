
using Microsoft.AspNetCore.Components;
using JsonToCsharpPoco.Components.Toast;

namespace JsonToCsharpPoco.Components.AppState;
public partial class CascadingAppState : ComponentBase
{
  [Parameter]
  public RenderFragment? ChildContent { get; set; }
  public ToastComponent? ToastComponentService;

  //todo:support local storage
  public string CurrentTheme { get; private set; } = "light";

  public void ToggleTheme()
  {
    CurrentTheme = CurrentTheme == "light" ? "dark" : "light";
    StateHasChanged();
  }

  public bool IsDarkTheme => CurrentTheme == "dark";

}