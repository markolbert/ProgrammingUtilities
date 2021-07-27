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
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Utilities
{
    public abstract class SelectableTree<TKey, TEntity> : ISelectableTree<TKey, TEntity>
        where TKey: notnull
    {
        private static readonly TKey _intKey = (TKey) Convert.ChangeType( 5, typeof(TKey) );

        private readonly ISelectableNodeFactory<TKey, TEntity> _nodeFactory;
        private readonly Dictionary<TKey, ISelectableNode<TKey, TEntity>> _masterNodeDict;

        protected SelectableTree(
            ISelectableNodeFactory<TKey, TEntity> nodeFactory,
            IJ4JLogger? logger,
            IEqualityComparer<TKey>? keyComparer = null
        )
        {
            _nodeFactory = nodeFactory;

            keyComparer ??= EqualityComparer<TKey>.Default;
            _masterNodeDict = new Dictionary<TKey, ISelectableNode<TKey, TEntity>>( keyComparer );

            Logger = logger;
            Logger?.SetLoggedType(GetType());
        }

        protected IJ4JLogger? Logger { get; }

        public void Clear()
        {
            Nodes.Clear();
            _masterNodeDict.Clear();
        }

        public void SetAll( bool isSelected )
        {
            foreach( var node in Nodes.SelectMany( x => x.DescendantsAndSelf ) )
            {
                node.IsSelected = isSelected;
            }
        }

        public ObservableCollection<ISelectableNode<TKey, TEntity>> Nodes { get; } = new();

        public bool FindNode( TKey key, out ISelectableNode<TKey, TEntity>? result )
        {
            result = null;

            if( _masterNodeDict.ContainsKey( key ) )
                result = _masterNodeDict[ key ];

            return result != null;
        }

        public IEnumerable<TEntity> GetSelectedNodes( bool getUnselected = false ) => getUnselected
                ? _masterNodeDict
                    .Where( x => !x.Value.IsSelected )
                    .Select( x => x.Value.Entity )
                : _masterNodeDict
                    .Where( x => x.Value.IsSelected )
                    .Select( x => x.Value.Entity );

        ///TODO tracing and reversing the path through the selected nodes may not be necessary
        public ISelectableNode<TKey, TEntity> AddOrGetNode( TEntity entity )
        {
            if( FindNode( GetKey( entity ), out var node ) )
                return node!;

            // trace the entities back up to a root entity, and then
            // create, if necessary, nodes in reverse order from that path
            var pathEntities = GetEntitiesPath( entity );
            pathEntities.Reverse();

            ISelectableNode<TKey, TEntity>? parentNode = null;
            ISelectableNode<TKey, TEntity>? retVal = null;

            foreach( var pathEntity in pathEntities )
            {
                var pathEntityKey = GetKey( pathEntity );
                var match = EqualityComparer<TKey>.Default.Equals( pathEntityKey, _intKey );

                bool nodeExists = FindNode( GetKey( pathEntity ), out retVal );
                retVal ??= _nodeFactory.Create( pathEntity, parentNode );

                if( !nodeExists )
                {
                    if( parentNode == null )
                        Nodes.Add( retVal );
                    else parentNode.ChildNodes.Add( retVal );

                    _masterNodeDict.Add( retVal.Key, retVal );
                }

                parentNode = retVal;
            }

            return retVal!;
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

            var tempRoot = Nodes
                .OrderBy( x => x, sortComparer )
                .ToList();

            Nodes.Clear();

            foreach( var node in tempRoot )
            {
                Nodes.Add( node );

                node.SortChildNodes( sortComparer );
            }
        }

        protected abstract bool GetParentEntity( TEntity entity, out TEntity? parentEntity );

        // don't reverse the entity path (i.e., the root entity should be the last entity in what's returned)
        protected abstract List<TEntity> GetEntitiesPath( TEntity entity );
        
        protected abstract TKey GetKey( TEntity entity );

        public void UpdateDisplayNames( IEnumerable<TKey> nodeKeys, bool inclUnselected = true )
        {
            foreach( var node in Nodes
                .Where( x => nodeKeys.Any( y => EqualityComparer<TKey>.Default.Equals(x.Key, y) )
                             && ( x.IsSelected || inclUnselected ) ) )
            {
                node.UpdateDisplayName();
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
    }
}