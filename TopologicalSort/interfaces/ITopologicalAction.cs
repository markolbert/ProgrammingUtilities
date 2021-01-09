using System.Collections.Generic;

namespace J4JSoftware.Utilities
{
    public interface ITopologicalAction<in TArg>
    {
        bool Process( IEnumerable<TArg> items );
    }
}