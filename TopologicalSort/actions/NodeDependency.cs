using System;

namespace J4JSoftware.Utilities
{
    public class NodeDependency<T>
        where T : class, IEquatable<T>
    {
        public NodeDependency(
            Node<T> ancestor,
            Node<T> dependent,
            Nodes<T> collection
        )
        {
            AncestorNode = ancestor;
            DependentNode = dependent;
            Collection = collection;
        }

        public Node<T> AncestorNode { get; }
        public Node<T> DependentNode { get; }
        public Nodes<T> Collection { get; }
    }
}