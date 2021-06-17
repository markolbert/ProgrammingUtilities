using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace J4JSoftware.WPFUtilities
{
    public interface ISelectableNode
    {
        string DisplayName { get; set; }
        bool IsSelected { get; set; }
        Visibility Visibility { get; set; }

        void UpdateDisplayName();
    }

    public interface ISelectableNode<TKey, TEntity> : ISelectableNode
        where TKey: IComparable<TKey>
    {
        TEntity Entity { get; }
        TKey Key { get; }

        ISelectableTree<TKey, TEntity> Tree { get; }

        ISelectableNode<TKey, TEntity>? ParentNode { get; }
        List<ISelectableNode<TKey, TEntity>> ChildNodes { get; }
    }
}
