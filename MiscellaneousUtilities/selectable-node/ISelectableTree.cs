using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace J4JSoftware.Utilities
{
    public interface ISelectableTree
    {
        void Clear();
        void SetAll( bool isSelected );
        object? AddOrGetNode( object entity );
        void AddOrGetNodes( IEnumerable<object> entities );
    }

    public interface ISelectableTree<TEntity, TKey> : ISelectableTree
        where TEntity : ISelectableEntity<TEntity, TKey>
    {
        ObservableCollection<ISelectableNode<TEntity, TKey>> Nodes { get; }
        IEnumerable<TEntity> GetSelectedNodes( bool getUnselected = false );

        ISelectableNode<TEntity, TKey> AddOrGetNode( TEntity entity );
        void AddOrGetNodes( IEnumerable<TEntity> entities );

        bool FindNode( TKey key, out ISelectableNode<TEntity, TKey>? result );
        void SortNodes( IComparer<ISelectableNode<TEntity, TKey>>? sortComparer = null );
    }
}
