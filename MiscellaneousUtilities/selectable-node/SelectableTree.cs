﻿#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// SelectableTree.cs
//
// This file is part of JumpForJoy Software's MiscellaneousUtilities.
// 
// MiscellaneousUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// MiscellaneousUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with MiscellaneousUtilities. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.Utilities;

[RequiresPreviewFeatures("Experimental, subject to change or removal")]
public abstract class SelectableTree<TEntity, TKey> : ISelectableTree<TEntity, TKey>
    where TKey : notnull
    where TEntity : class, ISelectableEntity<TEntity, TKey>
{
    private readonly Dictionary<TKey, TEntity> _masterDict;
    private readonly IEqualityComparer<TKey> _keyComparer;

    protected SelectableTree(
        ILoggerFactory? loggerFactory = null,
        IEqualityComparer<TKey>? keyComparer = null
    )
    {
        _keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;

        _masterDict = new Dictionary<TKey, TEntity>( keyComparer );

        Logger = loggerFactory?.CreateLogger(GetType());
    }

    protected ILogger? Logger { get; }

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

            Logger?.LogError( "Entity ({0}) with duplicate key ({1}) encountered", entity.DisplayName, entity.Key );
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

                Logger?.LogError( "Loop detected for entity '{0}' (key: {1}) at '{2}' (key: {3})",
                               new object[]
                               {
                                   kvp.Value.DisplayName, kvp.Key, curEntity.DisplayName, curEntity.Key
                               } );

                return false;
            }
        }

        return true;
    }

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
}