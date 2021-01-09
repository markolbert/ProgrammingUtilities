using System;
using System.Collections.Generic;

namespace J4JSoftware.Utilities
{
    public class TopologicalNode<T>
        where T : class, IEquatable<T>
    {
        private readonly IEqualityComparer<T>? _comparer;

        public TopologicalNode(
            T value,
            TopologicalCollection<T> collection,
            IEqualityComparer<T>? comparer = null
        )
        {
            Value = value;
            Collection = collection;
            _comparer = comparer;
        }

        public T Value { get; }
        public TopologicalCollection<T> Collection { get; }
        public List<TopologicalNode<T>> Dependents => Collection.GetDependents( this );
        public List<TopologicalNode<T>> Ancestors => Collection.GetAncestors(this);

        public bool Equals( TopologicalNode<T>? other )
        {
            if( other == null )
                return false;

            if (_comparer == null)
                return other.Value.Equals(Value);

            return _comparer.Equals( Value, other.Value );
        }
    }
}