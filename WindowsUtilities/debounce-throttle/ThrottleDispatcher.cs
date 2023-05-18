#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// ThrottleDispatcher.cs
//
// This file is part of JumpForJoy Software's WindowsUtilities.
// 
// WindowsUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// WindowsUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with WindowsUtilities. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using Microsoft.UI.Xaml;

namespace J4JSoftware.WindowsUtilities.debounce;

// thanx to Rick Strahl for this one.
// https://weblog.west-wind.com/posts/2017/jul/02/debouncing-and-throttling-dispatcher-events
public class ThrottleDispatcher
{
    private DispatcherTimer? _timer;
    private DateTime _timerStarted = DateTime.UtcNow.AddYears(-1);
    private Action<object?>? _action;
    private object? _optParam;

    public void Throttle(int interval, Action<object?> action, object? optParam = null)
    {
        _action = action;
        _optParam = optParam;

        _timer?.Stop();
        _timer = null;

        var curTime = DateTime.UtcNow;

        // if timeout is not up yet - adjust timeout to fire 
        // with potentially new Action parameters           
        if (curTime.Subtract(_timerStarted).TotalMilliseconds < interval)
            interval -= (int)curTime.Subtract(_timerStarted).TotalMilliseconds;

        _timer = new DispatcherTimer();
        _timer.Tick += OnTimerTick;
        _timer.Interval = TimeSpan.FromMilliseconds(interval);

        _timer.Start();
        _timerStarted = curTime;
    }

    private void OnTimerTick(object? sender, object e)
    {
        _timer?.Stop();
        _timer = null;

        _action!.Invoke(_optParam);
    }
}
