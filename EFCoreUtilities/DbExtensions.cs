#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// DbExtensions.cs
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
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable ExplicitCallerInfoArgument

namespace J4JSoftware.EFCoreUtilities;

public static class DbExtensions
{
    public static string FormatDbException(
        this DbUpdateException e,
        [ CallerMemberName ] string callerName = "",
        [ CallerFilePath ] string callerFilePath = "",
        [ CallerLineNumber ] int callerLineNum = 0
    )
    {
        var retVal = new StringBuilder();

        retVal.Append($"\n\tException type:\t{e.GetType().Name}");
        retVal.Append( $"\nDetails:\t{e.Message}" );

        if (e.InnerException != null)
            retVal.Append($"\n\tInner Details:\t{e.InnerException.Message}");

        retVal.Append( "\n\nInvolved entities are:" );

        foreach( var entity in e.Entries )
        {
            retVal.Append( $"\n{entity.Metadata.Name}\n" );

            foreach( var property in entity.Properties )
            {
                retVal.Append( $"\t{property.Metadata.Name}\t{property.CurrentValue}\n" );
            }
        }

        retVal.Append($"\n\n\tCalled by:\t{callerName}");
        retVal.Append($"\n\tSource file:\t{callerFilePath}");
        retVal.Append($"\n\tLine number:\t{callerLineNum}");

        return retVal.ToString();
    }

    public static List<Type> GetEntityTypes( this DbContext dbContext ) =>
        dbContext.Model.GetEntityTypes().Select( et => et.ClrType ).ToList();

    public static List<Type> GetEntityTypes<TContext>()
        where TContext : DbContext =>
        typeof( TContext ).GetProperties()
                          .Select( pi =>
                           {
                               if( !pi.PropertyType.IsGenericType )
                                   return null;

                               var genTypeDef = pi.PropertyType.GetGenericTypeDefinition();
                               if( genTypeDef != typeof( DbSet<> ) )
                                   return null;

                               return pi.PropertyType.GetGenericArguments()[ 0 ];
                           } )
                          .Where( et => et != null )
                          .Select( et => et! )
                          .ToList();

    public static bool TryGetEntityType(this DbContext dbContext, string entityName, out Type? entityType)
    {
        entityType = null;

        foreach (var curType in dbContext.Model.GetEntityTypes())
        {
            if (!curType.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase))
                continue;

            entityType = curType.ClrType;
            return true;
        }

        return false;
    }

    public static bool TryGetEntityName(this DbContext dbContext, Type entityType, out string? entityName)
    {
        entityName = null;

        foreach (var curType in dbContext.Model.GetEntityTypes())
        {
            if (curType.ClrType != entityType)
                continue;

            entityName = curType.Name;
            return true;
        }

        return false;
    }

    public static bool TryGetEntitySingleFieldPrimaryKey(this DbContext dbContext, Type entityType, out string? keyField)
    {
        keyField = null;

        foreach (var curType in dbContext.Model.GetEntityTypes())
        {
            if (curType.ClrType != entityType)
                continue;

            foreach (var key in curType.GetKeys())
            {
                if (!key.IsPrimaryKey() || key.Properties.Count != 1)
                    continue;

                keyField = key.Properties[0].Name;
                return true;
            }

            return false;
        }

        return false;
    }
}