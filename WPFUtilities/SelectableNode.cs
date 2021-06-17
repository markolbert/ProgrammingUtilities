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
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.WPFUtilities
{
    public abstract class SelectableNode<TKey, TEntity> : ObservableRecipient, ISelectableNode<TKey, TEntity>
        where TKey : IComparable<TKey>
    {
        private readonly Action<ISelectableNode<TKey, TEntity>, bool>? _selectionChangedHandler;

        private string _displayName = string.Empty;
        private bool _isSelected;
        private Visibility _visibility = Visibility.Hidden;

        protected SelectableNode( 
            TEntity entity,
            ISelectableNode<TKey, TEntity>? parentNode,
            ISelectableTree<TKey, TEntity> tree,
            Action<ISelectableNode<TKey, TEntity>, bool>? selectionChangedHandler
        )
        {
            Entity = entity;
            ParentNode = parentNode;
            Tree = tree;
            _selectionChangedHandler = selectionChangedHandler;
        }

        protected virtual void Initialize()
        {
            UpdateDisplayName();
        }

        public TEntity Entity { get; }
        public abstract TKey Key { get; }
        public ISelectableTree<TKey, TEntity> Tree { get; }
        public ISelectableNode<TKey, TEntity>? ParentNode { get; }
        public List<ISelectableNode<TKey, TEntity>> ChildNodes { get; } = new();


        public string DisplayName
        {
            get => _displayName;
            set => SetProperty(ref _displayName, value);
        }

        public bool IsSelected
        {
            get => _isSelected;

            set
            {
                SetProperty(ref _isSelected, value);

                OnSelectionChanged(value);
            }
        }

        public Visibility Visibility
        {
            get => _visibility;

            set
            {
                SetProperty(ref _visibility, value);

                OnVisibilityChanged();
            }
        }

        protected virtual void OnVisibilityChanged()
        {
        }

        protected virtual void OnSelectionChanged( bool isSelected )
        {
            if( _selectionChangedHandler != null )
                _selectionChangedHandler( this, isSelected );
        }

        public virtual void UpdateDisplayName()
        {
            DisplayName = GetDisplayName();
        }

        protected abstract string GetDisplayName();
    }
}