#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// EntityConfigurationExtensions.cs
//
// This file is part of JumpForJoy Software's EFCoreUtilities.
// 
// EFCoreUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// EFCoreUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with EFCoreUtilities. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.EFCoreUtilities;

public static class EntityConfigurationExtensions
{
    public static void ConfigureEntities( this ModelBuilder modelBuilder, Type? contextType = null, Assembly? assemblyToScan = null )
    {
        assemblyToScan ??= Assembly.GetCallingAssembly();

        // scan assembly for types decorated with EntityConfigurationAttribute and configure them
        foreach ( var entityType in assemblyToScan
                                  .DefinedTypes
                                  .Where( t =>
                                              t.GetCustomAttribute<EntityConfigurationAttribute>() != null ) )
        {
            var attr = entityType.GetCustomAttribute<EntityConfigurationAttribute>();

            if( contextType == null || attr!.ContextType == contextType)
                attr?.GetConfigurator().Configure( modelBuilder );
        }
    }

    public static void ConfigureEntities<TDbContext>(
        this ModelBuilder modelBuilder,
        Assembly? assemblyToScan = null
    )
    where TDbContext : DbContext
    {
        // have to do this here so the right assembly gets picked...
        assemblyToScan ??= Assembly.GetCallingAssembly();

        ConfigureEntities(modelBuilder, typeof(TDbContext), assemblyToScan);
    }
}