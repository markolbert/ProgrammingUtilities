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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace J4JSoftware.Utilities
{
    public abstract class SelectableNode<TKey, TEntity> : ISelectableNode<TKey, TEntity>
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isSelected;

        protected SelectableNode(
            TEntity entity,
            ISelectableNode<TKey, TEntity>? parentNode
        )
        {
            Entity = entity;
            ParentNode = parentNode;
        }

        protected virtual void Initialize()
        {
            UpdateDisplayName();
        }

        public TEntity Entity { get; }
        public abstract TKey Key { get; }
        public ISelectableNode<TKey, TEntity>? ParentNode { get; }
        public ObservableCollection<ISelectableNode<TKey, TEntity>> ChildNodes { get; } = new();
        public bool IsLeafNode => !ChildNodes.Any();

        public void SortChildNodes( IComparer<ISelectableNode<TKey, TEntity>>? sortComparer = null )
        {
            sortComparer ??= new DefaultSelectableNodeComparer<TKey, TEntity>();

            var tempRoot = ChildNodes
                .OrderBy(x => x, sortComparer)
                .ToList();

            ChildNodes.Clear();

            foreach (var node in tempRoot)
            {
                ChildNodes.Add(node);

                node.SortChildNodes(sortComparer);
            }
        }

        public string DisplayName { get; set; } = "** undefined **";

        public bool IsSelected
        {
            get => _isSelected;

            set
            {
                SetProperty( ref _isSelected, value );

                SelectableNode<TKey, TEntity>? curNode = (SelectableNode<TKey, TEntity>)this;

                do
                {
                    curNode.OnPropertyChanged(nameof(SubtreeHasSelectedItems));
                    curNode = (SelectableNode<TKey, TEntity>?)curNode.ParentNode;
                } while (curNode != null);
            }
        }

        public bool SubtreeHasSelectedItems
        {
            get => DescendantsAndSelf.Any( x => x.IsSelected );
        }

        public List<ISelectableNode<TKey, TEntity>> Descendants
        {
            get
            {
                var retVal = new List<ISelectableNode<TKey, TEntity>>();

                foreach (var childNode in ChildNodes)
                {
                    AddDescendantsAndSelf(childNode, retVal);
                }

                return retVal;
            }
        }

        public List<ISelectableNode<TKey, TEntity>> DescendantsAndSelf
        {
            get
            {
                var retVal = Descendants;
                retVal.Insert( 0, this );

                return retVal;
            }
        }

        private void AddDescendantsAndSelf( ISelectableNode<TKey, TEntity> curNode, List<ISelectableNode<TKey, TEntity>> descAndSelf )
        {
            descAndSelf.Add( curNode );

            foreach( var childNode in curNode.ChildNodes )
            {
                AddDescendantsAndSelf( childNode, descAndSelf );
            }
        }

        public virtual void UpdateDisplayName()
        {
            DisplayName = GetDisplayName();
        }

        protected abstract string GetDisplayName();

        private bool SetProperty<T>( ref T field, T value, [ CallerMemberName ] string callerName = "" )
        {
            if( EqualityComparer<T>.Default.Equals(field, value) ) 
                return false;
            
            field = value;
            OnPropertyChanged( callerName );

            return true;
        }

        private void OnPropertyChanged( [ CallerMemberName ] string callerName = "" ) =>
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( callerName ) );
    }
}