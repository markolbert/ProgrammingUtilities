using System;
using System.Collections.Generic;

namespace J4JSoftware.Utilities
{
    public interface IAction<TArg> : IEquatable<IAction<TArg>>
    {
        bool Process( IEnumerable<TArg> items );
    }
}