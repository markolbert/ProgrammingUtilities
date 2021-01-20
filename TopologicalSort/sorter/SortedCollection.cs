using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Utilities
{
    public abstract class SortedCollection<T> : ISortedCollection<T>
        where T : class, ISortable<T>
    {
        private readonly List<T> _items = new List<T>();
        private readonly List<int> _activatedIndices = new List<int>();
        
        private List<T>? _sorted;

        protected SortedCollection(
            IJ4JLogger? logger = null
        )
        {
            Logger = logger;
            Logger?.SetLoggedType(this.GetType());
        }

        protected IJ4JLogger? Logger { get; }

        protected abstract bool SetPredecessors();
        protected List<T> Available { get; } = new List<T>();

        public void Add( T item )
        {
            _items.Add( item );
            _sorted = null;
        }

        public void AddRange( IEnumerable<T> items )
        {
            _items.AddRange( items );
            _sorted = null;
        }

        public List<T> SortedSequence
        {
            get
            {
                if( _sorted != null )
                    return _sorted;

                _sorted = SortItems();

                return _sorted;
            }
        }

        private List<T> SortItems()
        {
            var retVal = new List<T>();

            Available.Clear();
            Available.AddRange( _items );

            if( !SetPredecessors() ) 
                return retVal;

            // remove the items that were activated. there should
            // be only one item left, the root item, afterwards
            foreach( var idx in _activatedIndices.OrderByDescending(x=>x) )
            {
                Available.RemoveAt( idx );
            }

            switch( Available.Count )
            {
                case 0:
                    Logger?.Error<Type>( "No root {0} defined", typeof(T) );
                    break;

                case 1:
                    // should already be null, but just in case...
                    Available[ 0 ].Predecessor = null;

                    _items.Add( Available[ 0 ] );

                    if (TopologicalSorter.Sort(_items, out var result))
                        SortedSequence.AddRange(result!);
                    else
                        Logger?.Error<Type>("Couldn't create execution sequence for {0}", typeof(T));

                    break;

                default:
                    Logger?.Error<Type>( "Multiple root {0} objects defined", typeof(T) );
                    break;
            }
            
            return retVal;
        }

        protected bool SetPredecessor<TNode, TPredecessorNode>()
            where TNode : T
            where TPredecessorNode : T
        {
            var nodeType = typeof(TNode);
            var predecessorType = typeof(TPredecessorNode);

            var selectedIdx = Available.FindIndex( x => x.GetType() == nodeType );

            if( selectedIdx < 0 )
            {
                Logger?.Error<Type>( "Couldn't find '{nodeType}'", nodeType );
                return false;
            }

            var predecessor = Available.FirstOrDefault( x => x.GetType() == predecessorType );

            if( predecessor == null )
            {
                Logger?.Error<Type>( $"Couldn't find '{predecessorType}'", predecessorType );
                return false;
            }

            Available[ selectedIdx ].Predecessor = predecessor;

            _items.Add( Available[ selectedIdx ] );
            _activatedIndices.Add(selectedIdx  );

            return true;
        }
    }
}