#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'WPFUtilities' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.WPFUtilities
{
    public abstract class SelectableNode<TKey, TEntity> : ISelectableNode<TKey, TEntity>, INotifyPropertyChanged
        where TKey : IComparable<TKey>
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly Action<ISelectableNode<TKey, TEntity>, bool>? _selectionChangedHandler;

        private bool _isSelected;
        private bool _subtreeIsSelected;
        private bool _suppressSelectionNotifications;

        protected SelectableNode(
            TEntity entity,
            ISelectableNode<TKey, TEntity>? parentNode,
            Action<ISelectableNode<TKey, TEntity>, bool>? selectionChangedHandler
        )
        {
            Entity = entity;
            ParentNode = parentNode;
            _selectionChangedHandler = selectionChangedHandler;
        }

        protected virtual void Initialize()
        {
            UpdateDisplayName();
        }

        public TEntity Entity { get; }
        public abstract TKey Key { get; }
        public ISelectableNode<TKey, TEntity>? ParentNode { get; }
        public List<ISelectableNode<TKey, TEntity>> ChildNodes { get; private set; } = new();
        public bool IsLeafNode => !ChildNodes.Any();

        public void SortChildNodes( IComparer<ISelectableNode<TKey, TEntity>>? sortComparer = null )
        {
            sortComparer ??= new DefaultSelectableNodeComparer<TKey, TEntity>();

            ChildNodes = ChildNodes.OrderBy( x => x, sortComparer )
                .ToList();

            foreach( var childNode in ChildNodes )
            {
                childNode.SortChildNodes( sortComparer );
            }
        }

        public string DisplayName { get; set; } = "** undefined **";

        public bool IsSelected
        {
            get => _isSelected;

            set
            {
                _isSelected = value;

                OnSelectionChanged( value );
            }
        }

        public bool SubtreeIsSelected
        {
            get => _subtreeIsSelected;

            set
            {
                var changed = _subtreeIsSelected != value;

                _subtreeIsSelected = value;

                if( changed )
                    OnPropertyChanged();
            }
        }

        protected virtual void OnSelectionChanged( bool isSelected )
        {
            if( !_suppressSelectionNotifications && _selectionChangedHandler != null )
                _selectionChangedHandler( this, isSelected );

            UpdateSubtreeSelectionState();
        }

        private void UpdateSubtreeSelectionState()
        {
            SubtreeIsSelected = IsSelected || ChildNodes.Any(x => x.SubtreeIsSelected);

            if ( ParentNode != null )
                ((SelectableNode<TKey, TEntity>)ParentNode).UpdateSubtreeSelectionState();
        }

        public virtual void ChangeSelectedOnSelfAndDescendants( bool isSelected )
        {
            _suppressSelectionNotifications = true;

            IsSelected = isSelected;

            foreach( var childNode in ChildNodes )
            {
                childNode.ChangeSelectedOnSelfAndDescendants( isSelected );
            }

            _suppressSelectionNotifications = false;
        }

        public virtual void UpdateDisplayName()
        {
            DisplayName = GetDisplayName();
        }

        protected abstract string GetDisplayName();

        protected virtual void OnPropertyChanged( [ CallerMemberName ] string propertyName = "" )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}