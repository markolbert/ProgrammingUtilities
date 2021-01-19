using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Utilities
{
    public abstract class TopologicallySortedCollection<T> : ITopologicallySorted<T>
        where T : class, ISortable<T>
    {
        protected readonly List<T> _available;
        private readonly List<T> _items = new List<T>();
        private readonly List<int> _activatedIndices = new List<int>();

        protected TopologicallySortedCollection(
            IEnumerable<T> items,
            IJ4JLogger? logger = null
        )
        {
            Logger = logger;
            Logger?.SetLoggedType(this.GetType());

            ExecutionSequence = new List<T>();

            _available = items.ToList();

            if( !SetPredecessors() ) 
                return;

            // remove the items that were activated. there should
            // be only one item left, the root item, afterwards
            foreach( var idx in _activatedIndices.OrderByDescending(x=>x) )
            {
                _available.RemoveAt( idx );
            }

            switch( _available.Count )
            {
                case 0:
                    Logger?.Error<Type>( "No root {0} defined", typeof(T) );
                    break;

                case 1:
                    // should already be null, but just in case...
                    _available[ 0 ].Predecessor = null;

                    _items.Add( _available[ 0 ] );

                    if (TopologicalSorter.Sort(_items, out var result))
                        ExecutionSequence.AddRange(result!);
                    else
                        Logger?.Error<Type>("Couldn't create execution sequence for {0}", typeof(T));

                    break;

                default:
                    Logger?.Error<Type>( "Multiple root {0} objects defined", typeof(T) );
                    break;
            }
        }

        protected IJ4JLogger? Logger { get; }

        protected abstract bool SetPredecessors();

        public List<T> ExecutionSequence { get; }

        protected bool SetPredecessor<TNode, TPredecessorNode>()
            where TNode : T
            where TPredecessorNode : T
        {
            var nodeType = typeof(TNode);
            var predecessorType = typeof(TPredecessorNode);

            var selectedIdx = _available.FindIndex( x => x.GetType() == nodeType );

            if( selectedIdx < 0 )
            {
                Logger?.Error<Type>( "Couldn't find '{nodeType}'", nodeType );
                return false;
            }

            var predecessor = _available.FirstOrDefault( x => x.GetType() == predecessorType );

            if( predecessor == null )
            {
                Logger?.Error<Type>( $"Couldn't find '{predecessorType}'", predecessorType );
                return false;
            }

            _available[ selectedIdx ].Predecessor = predecessor;

            _items.Add( _available[ selectedIdx ] );
            _activatedIndices.Add(selectedIdx  );

            return true;
        }
    }
}