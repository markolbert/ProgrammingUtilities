using System;

namespace J4JSoftware.Utilities
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class TopologicalPredecessorAttribute : Attribute
    {
        public TopologicalPredecessorAttribute( Type predecessorType )
        {
            PredecessorType = predecessorType;
        }

        public Type PredecessorType { get; }
    }
}
