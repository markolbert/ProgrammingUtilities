#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// DebounceDispatcher.cs
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

namespace J4JSoftware.WindowsUtilities;

// thanx to Rick Strahl for this one.
// https://weblog.west-wind.com/posts/2017/jul/02/debouncing-and-throttling-dispatcher-events
public class DebounceDispatcher
{
    private DispatcherTimer? _timer;

    public void Debounce(int interval,
        Action<object?> action,
        object? optParam = null)
    {
        // kill pending timer and pending ticks
        _timer?.Stop();
        _timer = null;

        // timer is recreated for each event and effectively resets the timeout.
        // Action only fires after timeout has fully elapsed without other events firing in between
        _timer = new DispatcherTimer();
        _timer.Tick += (_, _) => action.Invoke(optParam);

        _timer.Start();
    }
}
