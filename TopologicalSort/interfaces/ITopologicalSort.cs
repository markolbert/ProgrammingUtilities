using System;

namespace J4JSoftware.Utilities
{
    public interface ITopologicalSort<TNode> : IEquatable<TNode>
        where TNode : class
    {
        TNode? Predecessor { get; set; }
    }
}
