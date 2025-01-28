using JsonToCsharpPoco.Components.Toast;
using System.Threading.Tasks;
using Xunit;

public class ToastServiceTests
{
    [Fact]
    public async Task ShowToastAsync_AddsToastToList()
    {
        // Arrange
        var toastService = new ToastService();

        // Act
        await toastService.ShowToastAsync("Test Message");

        // Assert
        var toasts = toastService.GetToasts();
        Assert.Single(toasts);
        Assert.Equal("Test Message", toasts[0].Message);
    }

    [Fact]
    public async Task RemoveToast_RemovesToastFromList()
    {
        // Arrange
        var toastService = new ToastService();
        await toastService.ShowToastAsync("Test Message");
        var toast = toastService.GetToasts()[0];

        // Act
        await toastService.RemoveToast(toast);

        // Assert
        Assert.Empty(toastService.GetToasts());
    }
}