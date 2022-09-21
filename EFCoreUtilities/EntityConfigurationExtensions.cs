#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'EFCoreUtilities' is free software: you can redistribute it
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

using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.EFCoreUtilities;

public static class EntityConfigurationExtensions
{
    public static void ConfigureEntities( this ModelBuilder modelBuilder, Assembly? assemblyToScan = null )
    {
        assemblyToScan ??= Assembly.GetCallingAssembly();

        // scan assembly for types decorated with EntityConfigurationAttribute and configure them
        foreach( var entityType in assemblyToScan.DefinedTypes
                                                 .Where( t => t.GetCustomAttribute<EntityConfigurationAttribute>()
                                                          != null ) )
        {
            var attr = entityType.GetCustomAttribute<EntityConfigurationAttribute>();

            attr?.GetConfigurator().Configure( modelBuilder );
        }
    }
}