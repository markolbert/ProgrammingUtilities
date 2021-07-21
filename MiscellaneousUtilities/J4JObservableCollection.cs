using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace J4JSoftware.Utilities
{
    public class J4JObservableCollection<T> : IList<T>, IReadOnlyList<T>, INotifyCollectionChanged,
        INotifyPropertyChanged
    {
        private record Snapshot( bool IsReadOnly, int Count );

        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly List<T> _items = new();
        private Snapshot? _snapshot;
        private readonly IEqualityComparer<T> _equalityComparer;

        private bool _isReadOnly;
        private bool _notificationsAllowed = true;

        public J4JObservableCollection(
            IEqualityComparer<T>? equalityComparer = null
        )
        {
            _equalityComparer = equalityComparer ?? EqualityComparer<T>.Default;
        }

        #region ReadOnly

        public bool IsReadOnly => _isReadOnly;

        public void AsReadOnly()
        {
            _isReadOnly = true;

            if( _notificationsAllowed)
                OnPropertyChanged( nameof(IsReadOnly) );
        }

        #endregion

        #region Notifications

        protected bool NotificationsAllowed => _notificationsAllowed;

        protected void SuppressNotifications()
        {
            _notificationsAllowed = false;

            _snapshot = new Snapshot( IsReadOnly, Count );
        }

        protected void AllowNotifications()
        {
            _notificationsAllowed = true;

            CollectionChanged?.Invoke( this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Reset
                )
            );

            CollectionChanged?.Invoke( this,
                new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    _items,
                    0
                )
            );

            if( _snapshot!.IsReadOnly != IsReadOnly )
                OnPropertyChanged( nameof(IsReadOnly) );

            if( _snapshot.Count != Count )
                OnPropertyChanged( nameof(Count) );
        }

        #endregion

        public int Count => _items.Count;

        #region Enumerators

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Add/Clear/Insert/Change/Remove

        public void Clear()
        {
            if( _isReadOnly )
                throw new ReadOnlyException( $"Collection is read-only" );

            _items.Clear();

            if( _notificationsAllowed )
            {
                CollectionChanged?.Invoke( this,
                    new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );

                OnPropertyChanged(nameof(Count));
            }
        }

        public void Add( T item )
        {
            if( item != null )
                return;

            _items.Add( item );

            if( _notificationsAllowed )
            {
                CollectionChanged?.Invoke( this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add,
                        item
                    )
                );

                OnPropertyChanged( nameof(Count) );
            }
        }

        public void AddRange( IEnumerable<T> items )
        {
            var newItems = new List<T>();

            foreach( var item in items )
            {
                if( item == null )
                    continue;

                _items.Add( item );
                newItems.Add( item );
            }

            if( _notificationsAllowed )
            {
                CollectionChanged?.Invoke( this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add,
                        newItems
                    )
                );

                OnPropertyChanged(nameof(Count));
            }
        }

        public bool Remove( T item )
        {
            if( _isReadOnly )
                throw new ReadOnlyException( $"Collection is read-only" );

            if( item == null )
                return false;

            var idx = _items.FindIndex( x => _equalityComparer.Equals( x, item ) );
            if( idx < 0 )
                return false;

            _items.RemoveAt( idx );

            if( _notificationsAllowed )
            {
                CollectionChanged?.Invoke( this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove,
                        item
                    )
                );

                OnPropertyChanged(nameof(Count));
            }

            return true;
        }

        public void RemoveAt( int index )
        {
            if( _isReadOnly )
                throw new ReadOnlyException( $"Collection is read-only" );

            if( index < 0 || index > _items.Count - 1 )
                return;

            var removed = _items[ index ];

            _items.RemoveAt( index );

            if( _notificationsAllowed )
            {
                CollectionChanged?.Invoke( this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Remove,
                        removed
                    )
                );

                OnPropertyChanged(nameof(Count));
            }
        }

        public void Insert( int index, T item )
        {
            if( _isReadOnly )
                throw new ReadOnlyException( $"Collection is read-only" );

            if( item == null || index < 0 || index > _items.Count )
                return;

            _items.Insert( index, item );

            if( _notificationsAllowed )
            {
                CollectionChanged?.Invoke( this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add,
                        item,
                        index
                    )
                );

                OnPropertyChanged(nameof(Count));
            }
        }

        public T this[ int index ]
        {
            get
            {
                if( index < 0 || index > _items.Count - 1 )
                    throw new ArgumentOutOfRangeException( $"Index ({index}) < zero or >= collection size " );

                return _items[ index ];
            }

            set
            {
                if( _isReadOnly )
                    throw new ReadOnlyException( $"Collection is read-only" );

                if( index < 0 || index > _items.Count - 1 )
                    throw new ArgumentOutOfRangeException( $"Index ({index}) < zero or >= collection size " );

                _items[ index ] = value;

                if( _notificationsAllowed )
                    CollectionChanged?.Invoke( this,
                    new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace,
                        value,
                        index
                    )
                );
            }
        }

        #endregion

        #region Find/Contains

        public bool Contains( T item ) =>
            item is not null
            && _items.Contains( item, _equalityComparer );

        public int IndexOf( T item )
        {
            if( item == null )
                return -1;

            return _items.FindIndex( x => _equalityComparer.Equals( x, item ) );
        }

        #endregion

        public void CopyTo( T[] array, int arrayIndex )
        {
            if( arrayIndex + _items.Count >= array.Length )
                throw new ArgumentOutOfRangeException(
                    $"Starting index ({arrayIndex}) and collection size ({_items.Count}) exceeds target array length ({array.Length})" );

            for( var idx = 0; idx < _items.Count; idx++ )
            {
                array[ arrayIndex + idx ] = _items[ idx ];
            }
        }

        protected virtual void OnPropertyChanged( [ CallerMemberName ] string propertyName = "" )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        int IReadOnlyCollection<T>.Count => _items.Count;
    }
}
