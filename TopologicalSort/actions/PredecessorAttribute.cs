using System;

namespace J4JSoftware.Utilities
{
    [ AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = false ) ]
    public class PredecessorAttribute : Attribute
    {
        public PredecessorAttribute(
            Type? predecessor
        )
        {
            Predecessor = predecessor;
        }

        public Type? Predecessor { get; }
    }
}
