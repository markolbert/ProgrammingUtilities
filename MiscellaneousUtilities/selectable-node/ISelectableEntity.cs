#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'MiscellaneousUtilities' is free software: you can redistribute it
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
using System.ComponentModel;

namespace J4JSoftware.Utilities;

public interface ISelectableEntity<TEntity, TKey> : INotifyPropertyChanged
    where TEntity : class, ISelectableEntity<TEntity, TKey>
{
    TEntity Entity { get; }
    TEntity? Parent { get; }
    IEnumerable<TEntity> Children { get; }
    TKey Key { get; }

    string DisplayName { get; }
    bool IsSelected { get; set; }
}