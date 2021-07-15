using System;
using System.Collections.Generic;

namespace J4JSoftware.WPFUtilities
{
    public interface ISelectableTree
    {
        void Clear();

        object? AddOrGetNode( object entity );
        void AddOrGetNodes( IEnumerable<object> entities );
        
        void UpdateDisplayNames(bool inclInvisible = true, bool inclUnselected = true);
    }

    public interface ISelectableTree<TKey, TEntity> : ISelectableTree, 
        IEnumerable<ISelectableNode<TKey, TEntity>>
        where TKey : IComparable<TKey>
    {
        Dictionary<TKey, ISelectableNode<TKey, TEntity>> Nodes { get; }

        IEnumerable<TEntity> GetSelectedNodes( bool getUnselected = false );

        ISelectableNode<TKey, TEntity> AddOrGetNode( TEntity entity );
        void AddOrGetNodes( IEnumerable<TEntity> entities );

        void SortNodes( IComparer<ISelectableNode<TKey, TEntity>>? sortComparer = null );
    }
}
