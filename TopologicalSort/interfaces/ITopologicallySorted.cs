using System;
using System.Collections.Generic;

namespace J4JSoftware.Utilities
{
    public interface ITopologicallySorted<TNode>
        where TNode : IEquatable<TNode>
    {
        public List<TNode> ExecutionSequence { get; }
    }
}