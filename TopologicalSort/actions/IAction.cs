using System;
using System.Collections.Generic;

namespace J4JSoftware.Utilities
{
    public interface IAction
    {
        bool Process( object src );
    }

    public interface IAction<TSource> : IAction, IEquatable<IAction<TSource>>
    {
        bool Process( TSource src );
    }
}