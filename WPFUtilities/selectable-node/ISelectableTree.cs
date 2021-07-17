using System;
using System.Collections.Generic;

namespace J4JSoftware.WPFUtilities
{
    public interface ISelectableTree
    {
        void Clear();
        void SetAll( bool isSelected );
        object? AddOrGetNode( object entity );
        void AddOrGetNodes( IEnumerable<object> entities );
    }

    public interface ISelectableTree<TKey, TEntity> : ISelectableTree, 
        IEnumerable<ISelectableNode<TKey, TEntity>>
        where TKey : IComparable<TKey>
    {
        Dictionary<TKey, ISelectableNode<TKey, TEntity>> Nodes { get; }
        IEnumerable<TEntity> GetSelectedNodes(bool getUnselected = false);

        bool SetNode( TKey key, bool isSelected );
        void UpdateDisplayNames( IEnumerable<TKey> nodeKeys, bool inclUnselected = true );

        ISelectableNode<TKey, TEntity> AddOrGetNode( TEntity entity );
        void AddOrGetNodes( IEnumerable<TEntity> entities );

        void SortNodes( IComparer<ISelectableNode<TKey, TEntity>>? sortComparer = null );
    }
}
