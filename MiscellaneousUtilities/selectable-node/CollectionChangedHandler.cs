using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Utilities
{
    ///TODO may not be needed
    public class CollectionChangedHandler<TEntity, TKey>
        where TEntity : ISelectableEntity<TEntity, TKey>
        where TKey: notnull
    {
        private readonly ObservableCollection<ISelectableNode<TKey, TEntity>> _obsColl;
        private readonly Dictionary<TKey, ISelectableNode<TKey, TEntity>> _allNodes;
        private readonly string _collName;
        private readonly string _nodeName;

        private readonly IJ4JLogger? _logger;

        public CollectionChangedHandler(
            ObservableCollection<ISelectableNode<TKey, TEntity>> obsColl,
            IJ4JLogger? logger,
            string collName = ""
        )
        {
            _obsColl = obsColl;

            _collName = string.IsNullOrWhiteSpace( collName ) ? "target collection" : collName;
            _nodeName = $"{typeof(TEntity).Name}(s)";

            _allNodes = obsColl.SelectMany( x => x.DescendantsAndSelf )
                .ToDictionary( x => x.Key, x => x );

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public void HandleHierarchicalEvent( NotifyCollectionChangedEventArgs args )        
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add( args );
                    return;

                case NotifyCollectionChangedAction.Remove:
                    Remove( args );
                    return;

                case NotifyCollectionChangedAction.Replace:
                    if (args.OldItems == null)
                    {
                        _logger?.Error<string, string>(
                            "Notified {0} were replaced in {1} but no nodes were supplied to be removed",
                            _nodeName,
                            _collName);
                        return;
                    }

                    if (args.NewItems == null)
                    {
                        _logger?.Error<string, string>(
                            "Notified {0} were replaced in {1} but no nodes were supplied to be added",
                            _nodeName,
                            _collName);
                        return;
                    }

                    if (args.OldStartingIndex != args.NewStartingIndex)
                    {
                        _logger?.Error(
                            "Notified {0} were replaced in {1} but the old starting index ({2}) and the new starting index ({3}) are different",
                            new object[]
                            {
                                _nodeName,
                                _collName,
                                args.OldStartingIndex,
                                args.NewStartingIndex
                            });

                        return;
                    }

                    if (Remove(args))
                        Add(args);

                    return;

                case NotifyCollectionChangedAction.Move:
                    if( Remove( args ) )
                        Add( args );

                    return;

                case NotifyCollectionChangedAction.Reset:
                    _obsColl.Clear();
                    return;
            }
        }

        private bool FindCollectionInTarget( 
            ISelectableNode<TKey, TEntity> childNode,
            out ObservableCollection<ISelectableNode<TKey, TEntity>>? result )
        {
            result = null;

            if( childNode.ParentNode == null )
                result = _obsColl;
            else
            {
                if( !_allNodes.ContainsKey( childNode.ParentNode.Key ) )
                    _logger?.Error<TKey, string>( "Could not find containing node with key '{0}' in {1}",
                        childNode.ParentNode.Key,
                        _collName );
                else result = _allNodes[ childNode.ParentNode.Key ].ChildNodes;
            }

            return result != null;
        }

        private bool Add( NotifyCollectionChangedEventArgs args )
        {
            if( args.NewItems == null )
            {
                _logger?.Error<string, string>( "Notified {0} were added to {1} but no nodes were supplied",
                    _nodeName,
                    _collName );

                return false;
            }

            foreach( var item in args.NewItems.Cast<ISelectableNode<TKey, TEntity>>() )
            {
                if( !FindCollectionInTarget( item, out var container ) )
                    return false;

                container!.Add( item );
            }

            return true;
        }

        private bool Remove( NotifyCollectionChangedEventArgs args ) 
        {
            if( args.OldItems == null )
            {
                _logger?.Error<string, string>( "Notified {0} were removed from {1} but no nodes were supplied",
                    _nodeName, 
                    _collName );
                return false;
            }

            if( args.OldStartingIndex < 0 )
            {
                _obsColl.Clear();
                return true;
            }

            foreach( var item in args.OldItems.Cast<ISelectableNode<TKey, TEntity>>() )
            {
                if( !FindCollectionInTarget( item, out var container ) )
                    return false;

                if( !RemoveAt( container!, item ) )
                    return false;
            }

            return true;
        }

        private bool RemoveAt( 
            ObservableCollection<ISelectableNode<TKey, TEntity>> container, 
            ISelectableNode<TKey, TEntity> nodetoRemove ) 
        {
            var itemIndex = container.ToList()
                .FindIndex( x => EqualityComparer<TKey>.Default.Equals( x.Key, nodetoRemove.Key ) );

            if( itemIndex < 0 )
            {
                _logger?.Error<TKey, string>( "Could not find node with key '{0}' in the root of {1}", nodetoRemove.Key, _collName );

                return false;
            }

            container.RemoveAt( itemIndex );

            return true;
        }
    }
}
