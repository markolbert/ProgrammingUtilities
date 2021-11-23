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
using System.ComponentModel;
using System.Linq;
using J4JSoftware.Logging;
using Serilog;

namespace J4JSoftware.Utilities
{
    public abstract class SelectableTree<TEntity, TKey> : ISelectableTree<TEntity, TKey>
        where TKey : notnull
        where TEntity : class, ISelectableEntity<TEntity, TKey>
    {
        public event EventHandler? SelectionChanged;

        private readonly Dictionary<TKey, TEntity> _masterDict;
        private readonly IEqualityComparer<TKey> _keyComparer;

        protected SelectableTree(
            IJ4JLogger? logger,
            IEqualityComparer<TKey>? keyComparer = null
        )
        {
            _keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;

            _masterDict = new Dictionary<TKey, TEntity>( keyComparer );

            Logger = logger;
            Logger?.SetLoggedType( GetType() );
        }

        protected IJ4JLogger? Logger { get; }

        public bool Load( List<TEntity> entities )
        {
            var temp = new Dictionary<TKey, TEntity>();

            foreach( var entity in entities )
            {
                if( !temp.ContainsKey( entity.Key ) )
                {
                    temp.Add( entity.Key, entity );
                    continue;
                }

                Logger?.Error( "Entity ({0}) with duplicate key ({1}) encountered", entity.DisplayName, entity.Key );
                return false;
            }

            return Load( temp );
        }

        public bool Load( Dictionary<TKey, TEntity> entities )
        {
            _masterDict.Clear();
            RootEntities.Clear();

            foreach( var kvp in entities )
            {
                if( !_masterDict.ContainsKey( kvp.Key ) )
                    _masterDict.Add( kvp.Key, kvp.Value );

                if( kvp.Value.Parent == null )
                    RootEntities.Add( kvp.Value );
            }

            // check for loops
            var curPath = new List<TKey>();

            foreach( var kvp in _masterDict )
            {
                curPath.Clear();
                var curEntity = kvp.Value;

                while( curEntity.Parent != null )
                {
                    if( !curPath.Any( x => _keyComparer.Equals( x, curEntity.Key ) ) )
                    {
                        curPath.Add( curEntity.Key );
                        curEntity = curEntity.Parent;

                        continue;
                    }

                    Logger?.Error( "Loop detected for entity '{0}' (key: {1}) at '{2}' (key: {3})",
                                  new object[]
                                  {
                                      kvp.Value.DisplayName, kvp.Key, curEntity.DisplayName, curEntity.Key
                                  } );

                    return false;
                }
            }

            return true;
        }

        public void SetAll( bool isSelected )
        {
            foreach( var rootEntity in RootEntities )
            {
                foreach( var entity in rootEntity.DescendantEntitiesAndSelf<TEntity, TKey>() )
                {
                    entity.IsSelected = true;
                }
            }

            OnSelectionChanged();
        }

        protected internal virtual void OnSelectionChanged() => SelectionChanged?.Invoke( this, EventArgs.Empty );

        public ObservableCollection<TEntity> RootEntities { get; } = new();

        public bool FindEntity( TKey key, out TEntity? result )
        {
            result = null;

            if( _masterDict.ContainsKey( key ) )
                result = _masterDict[ key ];

            return result != null;
        }

        public IEnumerable<TEntity> SelectedEntities()
        {
            foreach( var rootEntity in RootEntities )
            {
                foreach( var child in rootEntity.DescendantEntitiesAndSelf<TEntity, TKey>())
                {
                    if( child.IsSelected  )
                        yield return child;
                }
            }
        }

        public IEnumerable<TEntity> UnselectedEntities()
        {
            foreach (var rootEntity in RootEntities)
            {
                foreach (var child in rootEntity.DescendantEntitiesAndSelf<TEntity, TKey>())
                {
                    if (!child.IsSelected)
                        yield return child;
                }
            }
        }

        /////TODO tracing and reversing the path through the selected nodes may not be necessary
        //public TEntity AddOrGetEntity( TEntity entity )
        //{
        //    if( FindEntity( GetKey( entity ), out var retVal ) )
        //        return retVal!;

        //    // trace the entities back up to a root entity, and then
        //    // create, if necessary, nodes in reverse order from that path
        //    var pathEntities = GetEntitiesPath( entity );
        //    pathEntities.Reverse();

        //    TEntity? parent = null;

        //    foreach( var pathEntity in pathEntities )
        //    {
        //        bool nodeExists = FindEntity( GetKey( pathEntity ), out retVal );
        //        retVal = entity;

        //        if( !nodeExists )
        //        {
        //            if( parent == null )
        //                RootEntities.Add( retVal );

        //            if( !_masterDict.ContainsKey(retVal.Key))
        //                _masterDict.Add( retVal.Key, retVal );
        //        }

        //        parent = retVal;
        //    }

        //    return retVal!;
        //}

        //public void AddOrGetEntities( IEnumerable<TEntity> entities )
        //{
        //    foreach ( var entity in entities )
        //    {
        //        AddOrGetEntity( entity );
        //    }
        //}

        //protected abstract bool GetParentEntity( TEntity entity, out TEntity? parentEntity );

        //// don't reverse the entity path (i.e., the root entity should be the last entity in what's returned)
        //protected abstract List<TEntity> GetEntitiesPath( TEntity entity );

        //protected abstract TKey GetKey( TEntity entity );

        //object? ISelectableTree.AddOrGetEntity( object entity )
        //{
        //    if( entity is TEntity castEntity )
        //        return AddOrGetEntity( castEntity );

        //    Logger?.Error( "Expected a {0} but got a {1}", typeof( TEntity ), entity.GetType() );

        //    return null;
        //}

        //void ISelectableTree.AddOrGetEntities( IEnumerable<object> entities )
        //{
        //    var temp = entities.ToList();

        //    if( temp.Any( x => x is not TEntity ) )
        //    {
        //        Logger?.Error( "Expected a collection of {0} but did not get it", typeof( TEntity ) );
        //        return;
        //    }

        //    foreach( var entity in temp.Cast<TEntity>() )
        //    {
        //        AddOrGetEntity( entity );
        //    }
        //}
    }
}
