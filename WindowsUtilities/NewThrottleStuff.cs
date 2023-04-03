using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;

namespace J4JSoftware.WindowsUtilities;

// thanx to Rick Strahl for this one.
// https://weblog.west-wind.com/posts/2017/jul/02/debouncing-and-throttling-dispatcher-events
public abstract class ThrottleBase
{
    private DispatcherTimer? _timer;
    private DateTime _timerStarted = DateTime.UtcNow.AddYears(-1);

    protected ThrottleBase()
    {
    }

    protected void Throttle(int milliseconds) => Throttle(TimeSpan.FromMilliseconds(milliseconds));

    protected void Throttle(TimeSpan interval)
    {
        _timer?.Stop();
        _timer = null;

        var curTime = DateTime.UtcNow;

        // if timeout is not up yet - adjust timeout to fire 
        // with potentially new Action parameters           
        if (curTime.Subtract(_timerStarted) < interval)
            interval -= curTime.Subtract(_timerStarted);

        _timer = new DispatcherTimer();
        _timer.Tick += ProcessTimerTick;
        _timer.Interval = interval;

        _timer.Start();
        _timerStarted = curTime;
    }

    private void ProcessTimerTick(object? sender, object e)
    {
        _timer?.Stop();
        _timer = null;

        OnTimerTick();
    }

    protected abstract void OnTimerTick();
}

public class ThrottleAction : ThrottleBase
{
    public event EventHandler? CallbackExecuted;

    private Action? _callback;

    public void Throttle(int milliseconds, Action callback) =>
        Throttle(TimeSpan.FromMilliseconds(milliseconds), callback);

    public void Throttle(TimeSpan interval, Action callback)
    {
        _callback = callback;
        Throttle(interval);
    }

    protected override void OnTimerTick()
    {
        _callback!.Invoke();
        CallbackExecuted?.Invoke(this, EventArgs.Empty);
    }
}

public class ThrottleAction<TParam> : ThrottleBase
{
    public event EventHandler? CallbackExecuted;

    private Action<TParam?>? _callback;
    private bool _isAsync;
    private TParam? _arg;

    public void Throttle(int milliseconds, Action<TParam?> callback, TParam? arg) =>
        Throttle(TimeSpan.FromMilliseconds(milliseconds), callback, arg);

    public void Throttle(TimeSpan interval, Action<TParam?> callback, TParam? arg)
    {
        _callback = callback;
        _isAsync = false;
        _arg = arg;

        Throttle(interval);
    }

    public void ThrottleAsync(int milliseconds, Action<TParam?> callback, TParam? arg) =>
        ThrottleAsync(TimeSpan.FromMilliseconds(milliseconds), callback, arg);

    public async Task ThrottleAsync(TimeSpan interval, Action<TParam?> callback, TParam? arg)
    {
        _callback = callback;
        _isAsync = true;
        _arg = arg;

        Throttle(interval);
    }

    protected override void OnTimerTick()
    {
        _callback!.Invoke(_arg);
        CallbackExecuted?.Invoke(this, EventArgs.Empty);
    }
}

public class ThrottleFunc<TResult> : ThrottleBase
{
    public event EventHandler<TResult>? CallbackExecuted;

    private Func<TResult>? _callback;

    public void Throttle(int milliseconds, Func<TResult> callback) =>
        Throttle(TimeSpan.FromMilliseconds(milliseconds), callback);

    public void Throttle(TimeSpan interval, Func<TResult> callback)
    {
        _callback = callback;
        Throttle(interval);
    }

    protected override void OnTimerTick()
    {
        var result = _callback!.Invoke();
        CallbackExecuted?.Invoke(this, result);
    }
}

public class ThrottleFunc<TParam, TResult> : ThrottleBase
{
    public event EventHandler<TResult>? CallbackExecuted;

    private Func<TParam?, TResult>? _callback;
    private TParam? _arg;

    public void Throttle(int milliseconds, Func<TParam?, TResult> callback, TParam? arg) =>
        Throttle(TimeSpan.FromMilliseconds(milliseconds), callback, arg);

    public void Throttle(TimeSpan interval, Func<TParam?, TResult> callback, TParam? arg)
    {
        _callback = callback;
        _arg = arg;

        Throttle(interval);
    }

    protected override void OnTimerTick()
    {
        var result = _callback!.Invoke(_arg);
        CallbackExecuted?.Invoke(this, result);
    }
}

