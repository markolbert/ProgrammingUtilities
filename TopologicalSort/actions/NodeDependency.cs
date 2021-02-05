using System;
using System.Runtime.Intrinsics.Arm;

namespace J4JSoftware.Utilities
{
    public class NodeDependency<T>
        where T : class, IEquatable<T>
    {
        public NodeDependency( Node<T> dependent, Node<T> ancestor )
        {
            DependentNode = dependent;
            AncestorNode = ancestor;
        }

        public Node<T> AncestorNode { get; }
        public Node<T> DependentNode { get; }
    }
}