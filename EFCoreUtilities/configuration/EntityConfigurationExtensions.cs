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

    public static void ConfigureEntities(this ModelBuilder modelBuilder, params Type[] entityTypes )
    {
        foreach (var entityType in entityTypes
                     .Where(t =>
                         t.GetCustomAttribute<EntityConfigurationAttribute>() != null))
        {
            var attr = entityType.GetCustomAttribute<EntityConfigurationAttribute>();
            attr?.GetConfigurator().Configure(modelBuilder);
        }
    }

    public static void ConfigureEntitiesFromAttributes<TContext>(this ModelBuilder modelBuilder)
        where TContext: DbContext
    {
        foreach (var configAttr in typeof(TContext).GetProperties()
                     .Where(pi => pi.PropertyType == typeof(DbSet<>))
                     .Select(pi =>
                     {
                         if (!pi.PropertyType.IsGenericType)
                             return null;

                         var typeParams = pi.PropertyType.GetGenericArguments();
                         if (typeParams.Length != 1)
                             return null;

                         return typeParams[0].GetCustomAttribute<EntityConfigurationAttribute>();
                     }))
        {
            configAttr?.GetConfigurator().Configure(modelBuilder);
        }
    }

    public static bool ConfigureEntitiesFromDbContext<TContext>(
        this ModelBuilder modelBuilder,
        out List<Type> unconfigured,
        params Assembly[] assemblies
    )
        where TContext : DbContext
    {
        // we should be configuring every entity type in TContext, so get a list of all the
        // TContext DbSet<>s and extract their entity types. Note that we have to do this by
        // reflection because GetEntityTypes() depends on a fully-configured DbContext Model,
        // and that hasn't been built yet at this point.
        unconfigured = DbExtensions.GetEntityTypes<TContext>();

        // if no assemblies were specified, assume we should only look in the calling assembly
        if( assemblies.Length == 0 )
            assemblies = new[] { Assembly.GetCallingAssembly() };

        // find all the IEntityConfigurator types in the assemblies
        foreach( var configType in assemblies.SelectMany( a => a.GetTypes() )
                                             .Where( t =>
                                              {
                                                  // only interested in types that support IEntityConfiguration
                                                  if( t.GetInterface( nameof( IEntityConfiguration ) ) == null )
                                                      return false;

                                                  // the IEntityConfiguration type must have a public 
                                                  // parameterless constructor
                                                  return t.GetConstructors()
                                                          .Any( ctor => ctor.GetParameters().Length == 0 );
                                              } ) )
        {
            var configurator = Activator.CreateInstance( configType ) as IEntityConfiguration
             ?? throw new NullReferenceException(
                    $"Could not create an instance of entity configurator type {configType.Name}" );

            configurator.Configure( modelBuilder );

            // get the entity type being configured
            if( (!configType.BaseType?.IsGenericType ?? true ) || !configType.BaseType.IsGenericType)
                throw new ArgumentException(
                    $"Type {configType.Name} does not have a generic base type" );

            var entityConfigType = configType.BaseType.GetGenericTypeDefinition();
            if( entityConfigType != typeof( EntityConfigurator<> ) )
                throw new ArgumentException(
                    $"Type {configType.Name} is not derived from {typeof(EntityConfigurator<> )}");

            var genArgs = configType.BaseType.GetGenericArguments();
            if( genArgs.Length != 1 )
                throw new ArgumentException(
                    $"Type {configType.Name} does not have a single generic argument");

            unconfigured.Remove( genArgs[ 0 ] );
        }

        return unconfigured.Count == 0;
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