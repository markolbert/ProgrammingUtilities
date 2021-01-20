using System;
using System.Collections.Generic;

namespace J4JSoftware.Utilities
{
    public class Node<T>
        where T : class, IEquatable<T>
    {
        private readonly IEqualityComparer<T>? _comparer;

        public Node(
            T value,
            Nodes<T> collection,
            IEqualityComparer<T>? comparer = null
        )
        {
            Value = value;
            Collection = collection;
            _comparer = comparer;
        }

        public T Value { get; }
        public Nodes<T> Collection { get; }
        public List<Node<T>> Dependents => Collection.GetDependents( this );
        public List<Node<T>> Ancestors => Collection.GetAncestors(this);

        public bool Equals( Node<T>? other )
        {
            if( other == null )
                return false;

            if (_comparer == null)
                return other.Value.Equals(Value);

            return _comparer.Equals( Value, other.Value );
        }
    }
}