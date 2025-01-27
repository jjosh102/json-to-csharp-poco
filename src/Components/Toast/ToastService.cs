
using Timer = System.Timers.Timer;
namespace JsonToCsharpPoco.Components.Toast;
public class ToastService : IDisposable
{
    private readonly List<ToastMessage> _toasts = [];
    private const int DefaultDurationMs = 5000;
    private readonly Timer? _timer;

    public event Action? OnToastsChanged;

    public ToastService()
    {
        _timer = new Timer(1000);
        _timer.Elapsed += async (sender, e) => await CheckToasts();
        _timer.Start();
    }

    public async Task ShowToastAsync(string message, ToastType type = ToastType.Info, string title = "", int durationMs = DefaultDurationMs)
    {
        var toast = new ToastMessage
        {
            Message = message,
            Title = title,
            Type = type,
            DurationMs = durationMs,
            IsVisible = false,
            CreatedAt = DateTime.Now
        };

        _toasts.Add(toast);
        OnToastsChanged?.Invoke();

        await Task.Delay(50);
        toast.IsVisible = true;
        OnToastsChanged?.Invoke();
    }

    private async Task CheckToasts()
    {
        var expiredToasts = _toasts
            .Where(t => (DateTime.Now - t.CreatedAt).TotalMilliseconds >= t.DurationMs)
            .ToList();

        foreach (var toast in expiredToasts)
        {
            await RemoveToast(toast);
        }
    }

    public async Task RemoveToast(ToastMessage toast)
    {
        toast.IsVisible = false;
        OnToastsChanged?.Invoke();

        await Task.Delay(500);

        _toasts.Remove(toast);
        OnToastsChanged?.Invoke();
    }

    public List<ToastMessage> GetToasts()
    {
        return _toasts;
    }

    public void Dispose()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Dispose();
        }
        _toasts.Clear();
    }
}