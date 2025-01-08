
using Microsoft.AspNetCore.Components;
using JsonToCsharpPoco.Components.Toast;

namespace JsonToCsharpPoco.Components.AppState;
public partial class CascadingAppState : ComponentBase
{
  [Parameter]
  public RenderFragment? ChildContent { get; set; }
  public ToastComponent? ToastComponentService;

  
}