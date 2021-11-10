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
    public class SelectableNode<TEntity, TKey> : ISelectableNode<TEntity, TKey>
        where TEntity : ISelectableEntity<TEntity, TKey>
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isSelected;

        protected SelectableNode( ISelectableEntity<TEntity, TKey> value )
        {
            Entity = value.Entity;
            ParentNode = (ISelectableNode<TEntity, TKey>?) value.Parent;
        }

        public TEntity Entity { get; }
        public TKey Key => Entity.Key;
        public ISelectableNode<TEntity, TKey>? ParentNode { get; }
        public ObservableCollection<ISelectableNode<TEntity, TKey>> ChildNodes { get; } = new();
        public bool IsLeafNode => !ChildNodes.Any();

        public void SortChildNodes( IComparer<ISelectableNode<TEntity, TKey>>? sortComparer = null )
        {
            sortComparer ??= new DefaultSelectableNodeComparer<TEntity, TKey>();

            var tempRoot = ChildNodes
                           .OrderBy( x => x, sortComparer )
                           .ToList();

            ChildNodes.Clear();

            foreach ( var node in tempRoot )
            {
                ChildNodes.Add( node );

                node.SortChildNodes( sortComparer );
            }
        }

        public virtual string DisplayName { get; } = "** undefined **";

        public bool IsSelected
        {
            get => _isSelected;

            set
            {
                SetProperty( ref _isSelected, value );

                SelectableNode<TEntity, TKey>? curNode = (SelectableNode<TEntity, TKey>) this;

                do
                {
                    curNode.OnPropertyChanged( nameof( SubtreeHasSelectedItems ) );
                    curNode = (SelectableNode<TEntity, TKey>?) curNode.ParentNode;
                } while ( curNode != null );
            }
        }

        public bool SubtreeHasSelectedItems
        {
            get => DescendantsAndSelf.Any( x => x.IsSelected );
        }

        public List<ISelectableNode<TEntity, TKey>> Descendants
        {
            get
            {
                var retVal = new List<ISelectableNode<TEntity, TKey>>();

                foreach ( var childNode in ChildNodes )
                {
                    AddDescendantsAndSelf( childNode, retVal );
                }

                return retVal;
            }
        }

        public List<ISelectableNode<TEntity, TKey>> DescendantsAndSelf
        {
            get
            {
                var retVal = Descendants;
                retVal.Insert( 0, this );

                return retVal;
            }
        }

        private void AddDescendantsAndSelf( ISelectableNode<TEntity, TKey> curNode,
                                            List<ISelectableNode<TEntity, TKey>> descAndSelf )
        {
            descAndSelf.Add( curNode );

            foreach( var childNode in curNode.ChildNodes )
            {
                AddDescendantsAndSelf( childNode, descAndSelf );
            }
        }

        private bool SetProperty<T>( ref T field, T value, [ CallerMemberName ] string callerName = "" )
        {
            if( EqualityComparer<T>.Default.Equals( field, value ) )
                return false;

            field = value;
            OnPropertyChanged( callerName );

            return true;
        }

        private void OnPropertyChanged( [ CallerMemberName ] string callerName = "" ) =>
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( callerName ) );
    }
}
