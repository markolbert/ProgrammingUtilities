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

using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace J4JSoftware.EFCoreUtilities;

public abstract class DesignTimeFactory<TDbContext> : IDesignTimeDbContextFactory<TDbContext>
    where TDbContext : DbContext
{
    protected static string? GetDatabaseProjectDirectory( [ CallerFilePath ] string? callerFilePath = null ) =>
        Path.GetDirectoryName( callerFilePath );

    protected DesignTimeFactory()
    {
        var genOptionsType = typeof( DbContextOptions<> );
        var optionsType = genOptionsType.MakeGenericType( typeof( TDbContext ) );

        // check to see if there's a public constructor taking a DbContextOptions parameter
        var constructors = typeof( TDbContext ).GetConstructors().ToList();

        if( !constructors.Any( x =>
           {
               var args = x.GetParameters();
               return args.Length == 1 && args[ 0 ].ParameterType.IsAssignableFrom( optionsType );
           } ) )
            throw new ArgumentException(
                $"{GetType().Name}::ctor() -- {typeof( TDbContext )} does not have a public constructor taking a single {typeof( DbContextOptions )} argument" );
    }

    public virtual DbContextOptions<TDbContext>? GetOptions() => null;

    public TDbContext CreateDbContext( string[] args )
    {
        var dbDir = args.Length == 0 ? GetDatabaseProjectDirectory() : args[ 0 ];

        if( string.IsNullOrEmpty( dbDir ) )
            throw new ArgumentException(
                $"{GetType().Name}::CreateDbContext() -- Database directory is undefined" );

        if( !Directory.Exists( dbDir ) )
            throw new ArgumentException(
                $"{GetType().Name}::CreateDbContext() -- directory '{dbDir}' does not exist or is not accessible" );

        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
        ConfigureOptionsBuilder( optionsBuilder, dbDir );

        var retVal = (TDbContext?) Activator.CreateInstance( typeof( TDbContext ), optionsBuilder.Options );

        if( retVal != null )
            return retVal;

        throw new ArgumentException(
            $"{GetType().Name}::CreateDbContext() -- failed to create instance of {typeof( TDbContext )}" );
    }

    protected abstract void ConfigureOptionsBuilder( DbContextOptionsBuilder<TDbContext> builder, string dbDirectory );
}