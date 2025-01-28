// using Bunit;
// using JsonToCsharpPoco.Components.Toast;
// using Microsoft.Extensions.DependencyInjection;
// using System.Threading.Tasks;
// using Xunit;

// public class ToastComponentTests : TestContext
// {
//     [Fact]
//     public async Task ToastComponent_DisplaysToastMessages()
//     {
    
//         var toastService = new ToastService();
//         Services.AddSingleton(toastService);

//         var component = RenderComponent<ToastProvider>();

//         Assert.Empty(component.FindAll("toast"));

//         await component.InvokeAsync(() => toastService.ShowToastAsync("Test Message"));

//         component.Render();

//         Assert.Single(component.FindAll("toast"));
//     }

//     [Fact]
//     public async Task ToastComponent_RemovesToastMessage()
//     {
      
//         var toastService = new ToastService();
//         Services.AddSingleton(toastService);

       
//         var component = RenderComponent<ToastProvider>();

       
//         await component.InvokeAsync(() => toastService.ShowToastAsync("Test Message"));

//         component.Render();

    
//         var closeButton = component.Find("button");
//         closeButton.Click();

//         component.Render();

//         Assert.Empty(component.FindAll("toast"));
//     }
// }