using System;
using System.Collections.Generic;

namespace J4JSoftware.Utilities
{
    public interface ISortedCollection<TNode>
        where TNode : IEquatable<TNode>
    {
        public List<TNode> SortedSequence { get; }
    }
}