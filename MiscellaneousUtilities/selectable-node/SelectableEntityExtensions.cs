#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// 
// This file is part of J4JLogger.
//
// J4JLogger is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JLogger is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JLogger. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Collections.Generic;
using System.Runtime.Versioning;

namespace J4JSoftware.Utilities;

[RequiresPreviewFeatures("Experimental, subject to change or removal")]
public static class SelectableEntityExtensions
{
    public static void SetTree<TEntity, TKey>( this ISelectableTree<TEntity, TKey> tree )
        where TEntity : class, ISelectableEntity<TEntity, TKey>
        where TKey : notnull
    {
        foreach (var rootEntity in tree.RootEntities)
        {
            rootEntity.SetBranch<TEntity, TKey>();
        }
    }

    public static void ClearTree<TEntity, TKey>(this ISelectableTree<TEntity, TKey> tree)
        where TEntity : class, ISelectableEntity<TEntity, TKey>
        where TKey : notnull
    {
        foreach (var rootEntity in tree.RootEntities)
        {
            rootEntity.ClearBranch<TEntity, TKey>();
        }
    }

    public static void SetBranch<TEntity, TKey>( this TEntity node )
        where TEntity : class, ISelectableEntity<TEntity, TKey>
        where TKey : notnull
    {
        foreach( var entity in node.DescendantEntitiesAndSelf<TEntity, TKey>() )
        {
            entity.IsSelected = true;
        }
    }

    public static void ClearBranch<TEntity, TKey>( this TEntity node )
        where TEntity : class, ISelectableEntity<TEntity, TKey>
        where TKey : notnull
    {
        foreach( var entity in node.DescendantEntitiesAndSelf<TEntity, TKey>() )
        {
            entity.IsSelected = false;
        }
    }

    public static IEnumerable<TEntity> DescendantEntitiesAndSelf<TEntity, TKey>(
        this TEntity rootEntity )
        where TEntity : class, ISelectableEntity<TEntity, TKey>
    {
        yield return rootEntity;

        foreach( var retVal in rootEntity.DescendantEntities<TEntity, TKey>() )
        {
            yield return retVal;
        }
    }

    public static IEnumerable<TEntity> DescendantEntities<TEntity, TKey>(
        this TEntity rootEntity )
        where TEntity : class, ISelectableEntity<TEntity, TKey>
    {
        foreach( var child in rootEntity.Children )
        {
            foreach ( var retVal in child.DescendantEntitiesAndSelf<TEntity, TKey>() )
            {
                yield return retVal;
            }
        }
    }

    public static bool EntityOrDescendantSelected<TEntity, TKey>( this TEntity rootEntity )
        where TEntity : class, ISelectableEntity<TEntity, TKey>
    {
        foreach ( var child in rootEntity.DescendantEntitiesAndSelf<TEntity, TKey>() )
        {
            if( child.IsSelected )
                return true;
        }

        return false;
    }

    public static bool EntityOrDescendantNotSelected<TEntity, TKey>( this TEntity rootEntity )
        where TEntity : class, ISelectableEntity<TEntity, TKey>
    {
        foreach( var child in rootEntity.DescendantEntitiesAndSelf<TEntity, TKey>() )
        {
            if( !child.IsSelected )
                return false;
        }

        return true;
    }
}