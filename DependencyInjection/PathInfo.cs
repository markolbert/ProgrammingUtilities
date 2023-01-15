using System;
using System.Runtime.Versioning;

namespace J4JSoftware.DependencyInjection;

[RequiresPreviewFeatures("Under development as of January 2023")]
public class PathInfo
{
    public string Path { get; internal set; } = string.Empty;
    public PathState State { get; internal set; }  = PathState.None;

    internal bool MeetsRequirements( FileLocator fileLoc )
    {
        foreach( var required in Enum.GetValues<PathState>() )
        {
            if( !fileLoc.RequiredPathState.HasFlag( required ) )
                continue;

            if( !State.HasFlag( required ) )
                return false;
        }

        return true;
    }
}
