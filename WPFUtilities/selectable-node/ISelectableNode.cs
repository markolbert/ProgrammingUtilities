using System;
using System.Collections.Generic;
using System.Windows;

namespace J4JSoftware.WPFUtilities
{
    public interface ISelectableNode
    {
        string DisplayName { get; set; }
        bool IsSelected { get; set; }

        void ChangeSelectedOnSelfAndDescendants( bool isSelected );

        void UpdateDisplayName();
    }

    public interface ISelectableNode<TKey, TEntity> : ISelectableNode
        where TKey: IComparable<TKey>
    {
        TEntity Entity { get; }
        TKey Key { get; }

        ISelectableNode<TKey, TEntity>? ParentNode { get; }
        List<ISelectableNode<TKey, TEntity>> ChildNodes { get; }

        void SortChildNodes( IComparer<ISelectableNode<TKey, TEntity>>? sortComparer = null );
    }
}
