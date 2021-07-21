using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace J4JSoftware.WPFUtilities
{
    public interface ISelectableNode : INotifyPropertyChanged
    {
        string DisplayName { get; set; }
        bool IsSelected { get; set; }
        bool SubtreeHasSelectedItems { get; }
        bool IsLeafNode { get; }

        void UpdateDisplayName();
    }

    public interface ISelectableNode<TKey, TEntity> : ISelectableNode
    {
        TEntity Entity { get; }
        TKey Key { get; }
        List<ISelectableNode<TKey, TEntity>> DescendantsAndSelf { get; }

        ISelectableNode<TKey, TEntity>? ParentNode { get; }
        ObservableCollection<ISelectableNode<TKey, TEntity>> ChildNodes { get; }

        void SortChildNodes( IComparer<ISelectableNode<TKey, TEntity>>? sortComparer = null );
    }
}
