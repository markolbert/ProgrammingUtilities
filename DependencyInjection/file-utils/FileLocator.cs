#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// 
// This file is part of J4JLogger.
//
// J4JLogger is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JLogger is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JLogger. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using J4JSoftware.DeusEx;
using Serilog;

namespace J4JSoftware.DependencyInjection;

public class FileLocator : IEnumerable<PathInfo>
{
    public FileLocator(
    )
    {
        Logger = J4JDeusEx.GetLogger();
        Logger?.ForContext<FileLocator>();

        FileSystemIsCaseSensitive = Environment.OSVersion.Platform switch
        {
            PlatformID.MacOSX => true,
            PlatformID.Unix => true,
            PlatformID.Win32NT => false,
            PlatformID.Win32S => false,
            PlatformID.Win32Windows => false,
            PlatformID.WinCE => false,
            PlatformID.Xbox => false,
            _ => DefaultSensitivity()
        };
    }

    private bool DefaultSensitivity()
    {
        Logger?.Warning("Unsupported operating system, case sensitivity set to false");
        return false;
    }
    
    internal ILogger? Logger { get; }

    public bool FileSystemIsCaseSensitive { get; internal set; }
    internal StringComparison FileSystemComparison =>
        FileSystemIsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

    public int MaximumMatches { get; set; }
    public int Matches => Results.Count;
    internal bool MatchesMet => MaximumMatches > 0 && Matches>= MaximumMatches;

    public string SearchPath { get; internal set; } = string.Empty;
    public PathState RequiredPathState { get; internal set; } = PathState.None;

    public PathState PathState { get; internal set; } = PathState.None;
    internal List<PathInfo> Results { get; } = new();

    public PathInfo? FirstMatch => Results.FirstOrDefault();

    public IEnumerator<PathInfo> GetEnumerator()
    {
        foreach( var retVal in Results )
        {
            yield return retVal;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}