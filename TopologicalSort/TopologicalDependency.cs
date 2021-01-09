using System;

namespace J4JSoftware.Utilities
{
    public class TopologicalDependency<T>
        where T : class, IEquatable<T>
    {
        public TopologicalDependency(
            TopologicalNode<T> ancestor,
            TopologicalNode<T> dependent,
            TopologicalCollection<T> collection
        )
        {
            AncestorNode = ancestor;
            DependentNode = dependent;
            Collection = collection;
        }

        public TopologicalNode<T> AncestorNode { get; }
        public TopologicalNode<T> DependentNode { get; }
        public TopologicalCollection<T> Collection { get; }
    }
}