using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;

namespace J4JSoftwarargs.Utilities
{
    public static class CollectionChangedExtensions
    {
        public static void HandleEvent<T>( 
            this NotifyCollectionChangedEventArgs args, 
            ObservableCollection<T> toUpdate,
            IJ4JLogger? logger = null, 
            string collectionName = "" )
        {
            collectionName = string.IsNullOrWhiteSpace( collectionName ) ? "target collection" : collectionName;
            var nodeName = $"{typeof(T).Name}(s)";

            switch( args.Action )
            {
                case NotifyCollectionChangedAction.Add:
                    add();
                    return;

                case NotifyCollectionChangedAction.Remove:
                    remove();
                    return;

                case NotifyCollectionChangedAction.Replace:
                    if( args.OldItems == null )
                    {
                        logger?.Error<string, string>(
                            "Notified {0} were replaced in {1} but no nodes were supplied to be removed",
                            nodeName,
                            collectionName );
                        return;
                    }

                    if( args.NewItems == null )
                    {
                        logger?.Error<string, string>(
                            "Notified {0} were replaced in {1} but no nodes were supplied to be added",
                            nodeName,
                            collectionName );
                        return;
                    }

                    if( args.OldStartingIndex != args.NewStartingIndex )
                    {
                        logger?.Error(
                            "Notified {0} were replaced in {1} but the old starting index ({2}) and the new starting index ({3}) are different",
                            new object[]
                            {
                                nodeName,
                                collectionName,
                                args.OldStartingIndex,
                                args.NewStartingIndex
                            } );

                        return;
                    }

                    if( args.OldStartingIndex < 0 )
                    {
                        toUpdate.Clear();

                        foreach( var node in args.NewItems.Cast<T>() )
                        {
                            toUpdate.Add( node );
                        }

                        return;
                    }

                    for( var idx = args.OldStartingIndex; idx < args.OldStartingIndex + args.OldItems.Count; idx++ )
                    {
                        toUpdate[ idx ] = (T) args.NewItems[ idx - args.OldStartingIndex ]!;
                    }

                    return;

                case NotifyCollectionChangedAction.Move:
                    if( remove() )
                        add();

                    return;

                case NotifyCollectionChangedAction.Reset:
                    toUpdate.Clear();
                    return;
            }

            bool add()
            {
                if( args.NewItems == null )
                {
                    logger?.Error<string, string>( "Notified {0} were added to {1} but no nodes were supplied",
                        nodeName,
                        collectionName );

                    return false;
                }

                var insertAt = args.NewStartingIndex >= 0 ? args.NewStartingIndex : args.NewItems.Count - 1;

                foreach( var item in args.NewItems )
                {
                    toUpdate.Insert( insertAt, (T) item );
                }

                return true;
            }

            bool remove()
            {
                if( args.OldItems == null )
                {
                    logger?.Error<string, string>( "Notified {0} were removed from {1} but no nodes were supplied",
                        nodeName,
                        collectionName );
                    return false;
                }

                if( args.OldStartingIndex < 0 )
                {
                    toUpdate.Clear();
                    return true;
                }

                for( var idx = args.OldItems.Count; idx > 0; idx-- )
                {
                    toUpdate.RemoveAt( toUpdate.Count - 1 );
                }

                return true;
            }
        }
    }
}
