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

    public interface ISelectableTree<TKey, TEntity> : ISelectableTree
    {
        ObservableCollection<ISelectableNode<TKey, TEntity>> Nodes { get; }
        IEnumerable<TEntity> GetSelectedNodes(bool getUnselected = false);

        void UpdateDisplayNames( IEnumerable<TKey> nodeKeys, bool inclUnselected = true );

        ISelectableNode<TKey, TEntity> AddOrGetNode( TEntity entity );
        void AddOrGetNodes( IEnumerable<TEntity> entities );

        bool FindNode(TKey key, out ISelectableNode<TKey, TEntity>? result);
        void SortNodes( IComparer<ISelectableNode<TKey, TEntity>>? sortComparer = null );
    }
}
