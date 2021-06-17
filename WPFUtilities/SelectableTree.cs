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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.TextFormatting;
using J4JSoftware.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace J4JSoftware.WPFUtilities
{
    public abstract class SelectableTree<TKey, TEntity> : ISelectableTree<TKey, TEntity>
        where TKey: IComparable<TKey>
    {
        public const int DefaultMaxNames = 2;

        private readonly Func<
            TEntity,
            ISelectableNode<TKey, TEntity>?,
            ISelectableTree<TKey, TEntity>,
            Action<ISelectableNode<TKey, TEntity>, bool>?,
            ISelectableNode<TKey, TEntity>
        > _nodeCreator;

        private readonly int _maxNames;
        private readonly Action<ISelectableNode<TKey, TEntity>, bool>? _selectionChangedHandler;

        protected SelectableTree(
            Func<
                TEntity,
                ISelectableNode<TKey, TEntity>?,
                ISelectableTree<TKey, TEntity>,
                Action<ISelectableNode<TKey, TEntity>, bool>?,
                ISelectableNode<TKey, TEntity>
            > nodeCreator,
            Action<
                ISelectableNode<TKey, TEntity>, 
                bool
            >? selectionChangedHandler,
            int maxNames,
            IJ4JLogger? logger
        )
        {
            _nodeCreator = nodeCreator;
            _selectionChangedHandler = selectionChangedHandler;

            _maxNames = maxNames > 0 ? maxNames : DefaultMaxNames;

            Logger = logger;
            Logger?.SetLoggedType( GetType() );
        }

        protected IJ4JLogger? Logger { get; }

        public void Clear()
        {
            RootNodes.Clear();
            AllNodes.Clear();
        }

        public List<ISelectableNode<TKey, TEntity>> RootNodes { get; } = new();
        public Dictionary<TKey, ISelectableNode<TKey, TEntity>> AllNodes { get; } = new();

        public ISelectableNode<TKey, TEntity> AddOrGetNode( TEntity entity )
        {
            var curKey = GetKey( entity );

            if( AllNodes.ContainsKey( curKey ) )
                return AllNodes[ curKey ];

            TEntity? curEntity = entity;
            ISelectableNode<TKey, TEntity>? retVal = null;

            do
            {
                var parentEntity = GetParentEntity( curEntity );

                retVal = _nodeCreator(
                    curEntity,
                    parentEntity == null ? null : AddOrGetNode( parentEntity ),
                    this,
                    _selectionChangedHandler );

                AllNodes.Add( retVal.Key, retVal );

                if( parentEntity == null )
                    RootNodes.Add( retVal );

                curEntity = parentEntity;

            } while( curEntity != null );

            return retVal;
        }

        public void AddOrGetNodes(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                AddOrGetNode(entity);
            }
        }

        protected abstract TEntity? GetParentEntity( TEntity entity );
        protected abstract TKey GetKey( TEntity entity );

        public virtual void UpdateDisplayNames( bool inclInvisible = true, bool inclUnselected = true )
        {
            foreach( var node in AllNodes
                .Where( x => ( x.Value.Visibility == Visibility.Visible || inclInvisible )
                             && ( x.Value.IsSelected || inclUnselected ) ) )
            {
                node.Value.UpdateDisplayName();
            }
        }

        object? ISelectableTree.AddOrGetNode( object entity )
        {
            if( entity is TEntity castEntity )
                return AddOrGetNode( castEntity );

            Logger?.Error( "Expected a {0} but got a {1}", typeof(TEntity), entity.GetType() );

            return null;
        }

        void ISelectableTree.AddOrGetNodes( IEnumerable<object> entities )
        {
            var temp = entities.ToList();

            if( temp.Any( x => x is not TEntity ) )
            {
                Logger?.Error( "Expected a collection of {0} but did not get it", typeof(TEntity) );
                return;
            }

            foreach( var entity in temp.Cast<TEntity>() )
            {
                AddOrGetNode( entity );
            }
        }

        public IEnumerator<ISelectableNode<TKey, TEntity>> GetEnumerator()
        {
            foreach( var node in RootNodes )
            {
                foreach( var temp in EnumerateNodes( node ) )
                {
                    yield return temp;
                }
            }
        }

        private IEnumerable<ISelectableNode<TKey, TEntity>> EnumerateNodes(ISelectableNode<TKey, TEntity> node )
        {
            yield return node;

            foreach( var childNode in node.ChildNodes )
            {
                foreach( var temp in EnumerateNodes( childNode ) )
                {
                    yield return temp;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}