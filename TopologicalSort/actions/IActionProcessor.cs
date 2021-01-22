using System.Collections.Generic;

namespace J4JSoftware.Utilities
{
    //public interface IEnumerableProcessor<TItem> : IAction<TItem>, IEquatable<IAction<TItem>>
    //{
    //}

    public interface IActionProcessor<in TItem>
    {
        bool Process( IEnumerable<TItem> items );
    }


}