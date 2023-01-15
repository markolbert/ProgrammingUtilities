﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using J4JSoftware.Logging;

namespace J4JSoftware.DependencyInjection;

[RequiresPreviewFeatures("Under development as of January 2023")]
public class FileLocator : IEnumerable<PathInfo>
{
    public FileLocator(
        IJ4JLogger? logger = null
    )
    {
        Logger = logger;
        Logger?.SetLoggedType( GetType() );

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
    
    internal IJ4JLogger? Logger { get; }

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