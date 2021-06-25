using System;
using System.Windows.Threading;

namespace J4JSoftware.WPFUtilities
{
    // thanx to Rick Strahl for this one.
    // https://weblog.west-wind.com/posts/2017/jul/02/debouncing-and-throttling-dispatcher-events
    public class DebounceDispatcher
    {
        private DispatcherTimer? _timer;
        private DateTime _timerStarted = DateTime.UtcNow.AddYears(-1);

        /// <param name="interval">Timeout in Milliseconds</param>
        /// <param name="action">Action<object> to fire when debounced event fires</object></param>
        /// <param name="optParam">optional parameter</param>
        /// <param name="priority">optional priorty for the dispatcher</param>
        /// <param name="dispatcher">optional dispatcher. If not passed or null CurrentDispatcher is used.</param>        
        public void Debounce(
            int interval, 
            Action<object?> action,
            object? optParam = null,
            DispatcherPriority priority = DispatcherPriority.ApplicationIdle,
            Dispatcher? dispatcher = null)
        {
            // kill pending timer and pending ticks
            _timer?.Stop();
            _timer = null;

            dispatcher ??= Dispatcher.CurrentDispatcher;

            // timer is recreated for each event and effectively resets the timeout.
            // Action only fires after timeout has fully elapsed without other events firing in between
            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), priority, (s, e) =>
            {
                if (_timer == null)
                    return;

                _timer?.Stop();
                _timer = null;
                action.Invoke(optParam);
            }, dispatcher);

            _timer.Start();
        }

        /// <param name="interval">Timeout in Milliseconds</param>
        /// <param name="action">Action<object> to fire when debounced event fires</object></param>
        /// <param name="optParam">optional parameter</param>
        /// <param name="priority">optional priorty for the dispatcher</param>
        /// <param name="dispatcher">optional dispatcher. If not passed or null CurrentDispatcher is used.</param>
        public void Throttle(
            int interval, 
            Action<object?> action,
            object? optParam = null,
            DispatcherPriority priority = DispatcherPriority.ApplicationIdle,
            Dispatcher? dispatcher = null)
        {
            // kill pending timer and pending ticks
            _timer?.Stop();
            _timer = null;

            dispatcher ??= Dispatcher.CurrentDispatcher;

            var curTime = DateTime.UtcNow;

            // if timeout is not up yet - adjust timeout to fire 
            // with potentially new Action parameters           
            if (curTime.Subtract(_timerStarted).TotalMilliseconds < interval)
                interval -= (int)curTime.Subtract(_timerStarted).TotalMilliseconds;

            _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(interval), priority, (s, e) =>
            {
                if (_timer == null)
                    return;

                _timer?.Stop();
                _timer = null;
                action.Invoke(optParam);
            }, dispatcher);

            _timer.Start();
            _timerStarted = curTime;
        }
    }
}
