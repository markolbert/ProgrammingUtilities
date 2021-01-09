using System;
using System.Collections.Generic;

namespace J4JSoftware.Utilities
{
    public interface IEnumerableProcessor<TItem> : ITopologicalAction<TItem>, IEquatable<IEnumerableProcessor<TItem>>
    {
    }

    public interface IProcessorCollection<in TItem>
    {
        bool Process( IEnumerable<TItem> items );
    }


}