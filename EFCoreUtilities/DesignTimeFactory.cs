// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of EFCoreUtilities.
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
    // this function returns the directory of the source code file from which it is called
    protected static string GetSourceCodeDirectoryOfClass( [ CallerFilePath ] string? callerFilePath = null ) =>
        Path.GetDirectoryName( callerFilePath )!;

    private readonly string _srcCodeDir;

    // the parameter srcCodeDir defines the directory where the derived DesignTimeFactory class is defined
    // this is typically within a separate assembly devoted to defining a DbContext and its entities
    // the value cannot be calculated in this class because this source code file is in its own library,
    // which isn't likely to be located anywhere near where the database is being defined.
    protected DesignTimeFactory(
        string srcCodeDir
        )
    {
        _srcCodeDir = srcCodeDir;

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
        var dbPath = args.Length == 0
            ? _srcCodeDir
            : Path.IsPathRooted( args[ 0 ] )
                ? args[ 0 ]
                : Path.Combine( _srcCodeDir, args[ 0 ] );

        dbPath = Path.GetFullPath( dbPath );

        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
        ConfigureOptionsBuilder( optionsBuilder, dbPath );

        var retVal = (TDbContext?) Activator.CreateInstance( typeof( TDbContext ), optionsBuilder.Options );

        if( retVal != null )
            return retVal;

        throw new ArgumentException(
            $"{GetType().Name}::CreateDbContext() -- failed to create instance of {typeof( TDbContext )}" );
    }

    protected abstract void ConfigureOptionsBuilder( DbContextOptionsBuilder<TDbContext> builder, string dbPath );
}