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
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using J4JSoftware.Logging;

namespace J4JSoftware.WPFUtilities
{
    public abstract class SelectableTree<TKey, TEntity, TContainer> : ISelectableTree<TKey, TEntity>
        where TKey: IComparable<TKey>
        where TContainer : class
    {
        private readonly ISelectableNodeFactory<TKey, TEntity> _nodeFactory;
        private readonly ObservableCollection<ISelectableNode<TKey, TEntity>> _nodeCollection;

        protected SelectableTree(
            ISelectableNodeFactory<TKey, TEntity> nodeFactory,
            TContainer nodeCollectionSource,
            Expression<Func<TContainer, ObservableCollection<ISelectableNode<TKey, TEntity>>>> nodeCollection,
            IJ4JLogger? logger
        )
        {
            _nodeFactory = nodeFactory;

            Logger = logger;
            Logger?.SetLoggedType(GetType());

            var memExpr = (MemberExpression) nodeCollection.Body;
            var nodeCollInfo = (PropertyInfo) memExpr.Member;

            if(nodeCollInfo.GetValue(nodeCollectionSource) is not ObservableCollection<ISelectableNode<TKey, TEntity>> castColl )
            {
                Logger?.Fatal( "Node collection could not be found or is not an '{0}'",
                    typeof(ObservableCollection<ISelectableNode<TKey, TEntity>>) );;

                throw new ArgumentException(
                    $"Node collection could not be found or is not an '{typeof(ObservableCollection<ISelectableNode<TKey, TEntity>>)}'" );
            }

            _nodeCollection = castColl;
        }

        protected IJ4JLogger? Logger { get; }

        public void Clear()
        {
            _nodeCollection.Clear();
            Nodes.Clear();
        }

        public Dictionary<TKey, ISelectableNode<TKey, TEntity>> Nodes { get; } = new();

        public ISelectableNode<TKey, TEntity> AddOrGetNode( TEntity entity )
        {
            var curKey = GetKey( entity );

            if( Nodes.ContainsKey( curKey ) )
                return Nodes[ curKey ];

            TEntity? curEntity = entity;
            ISelectableNode<TKey, TEntity>? retVal = null;

            do
            {
                var parentEntity = GetParentEntity( curEntity );
                var parentNode = parentEntity == null ? null : AddOrGetNode( parentEntity );

                retVal = _nodeFactory.Create( curEntity, parentNode );

                Nodes.Add( retVal.Key, retVal );

                parentNode?.ChildNodes.Add( retVal );

                if( parentEntity == null )
                    _nodeCollection.Add( retVal );

                curEntity = parentEntity;

            } while( curEntity != null && !Nodes.ContainsKey( GetKey( curEntity ) ) );

            return retVal;
        }

        public void AddOrGetNodes(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                AddOrGetNode(entity);
            }
        }

        public void SortNodes( IComparer<ISelectableNode<TKey, TEntity>>? sortComparer = null )
        {
            sortComparer ??= new DefaultSelectableNodeComparer<TKey, TEntity>();

            var tempRoot = _nodeCollection
                .OrderBy( x => x, sortComparer )
                .ToList();

            _nodeCollection.Clear();

            foreach( var node in tempRoot )
            {
                _nodeCollection.Add( node );

                node.SortChildNodes( sortComparer );
            }
        }

        protected abstract TEntity? GetParentEntity( TEntity entity );
        protected abstract TKey GetKey( TEntity entity );

        public virtual void UpdateDisplayNames( bool inclInvisible = true, bool inclUnselected = true )
        {
            foreach( var node in Nodes
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
            foreach( var kvp in Nodes )
            {
                yield return kvp.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}