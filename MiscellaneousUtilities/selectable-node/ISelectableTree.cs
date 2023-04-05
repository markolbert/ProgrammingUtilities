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
using System.Collections.ObjectModel;
using System.Runtime.Versioning;

namespace J4JSoftware.Utilities;

[RequiresPreviewFeatures("Experimental, subject to change or removal")]
public interface ISelectableTree<TEntity, TKey>
    where TEntity : class, ISelectableEntity<TEntity, TKey>
    where TKey : notnull
{
    ObservableCollection<TEntity> RootEntities { get; }

    bool Load( List<TEntity> entities );
    bool Load( Dictionary<TKey, TEntity> entities );

    IEnumerable<TEntity> SelectedEntities();
    IEnumerable<TEntity> UnselectedEntities();

    bool FindEntity( TKey key, out TEntity? result );
}