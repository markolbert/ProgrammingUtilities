using System;

namespace J4JSoftware.DependencyInjection;

[Flags]
public enum PathState
{
    Exists = 1 << 0,
    Readable = 1 << 1,
    // ReSharper disable once IdentifierTypo
    Writeable = 1 << 2,

    None = 0,
    All = Exists | Readable | Writeable
}
