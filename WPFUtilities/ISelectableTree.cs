using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        List<ISelectableNode<TKey, TEntity>> RootNodes { get; }
        Dictionary<TKey, ISelectableNode<TKey, TEntity>> AllNodes { get; }

        ISelectableNode<TKey, TEntity> AddOrGetNode( TEntity entity );
        void AddOrGetNodes( IEnumerable<TEntity> entities );
    }
}
