using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace J4JSoftware.Utilities
{
    public interface ISelectableNode : INotifyPropertyChanged
    {
        string DisplayName { get; }
        bool IsSelected { get; set; }
        bool SubtreeHasSelectedItems { get; }
        bool IsLeafNode { get; }
    }

    public interface ISelectableNode<TEntity, TKey> : ISelectableNode
        where TEntity : ISelectableEntity<TEntity, TKey>
    {
        TEntity Entity { get; }
        TKey Key { get; }
        List<ISelectableNode<TEntity, TKey>> DescendantsAndSelf { get; }

        ISelectableNode<TEntity, TKey>? ParentNode { get; }
        ObservableCollection<ISelectableNode<TEntity, TKey>> ChildNodes { get; }

        void SortChildNodes( IComparer<ISelectableNode<TEntity, TKey>>? sortComparer = null );
    }
}
