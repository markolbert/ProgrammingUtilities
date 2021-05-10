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
using SQLitePCL;

namespace J4JSoftware.EFCoreUtilities
{
    public abstract class DesignTimeFactoryNG<TDbContext> : IDesignTimeDbContextFactory<TDbContext>
        where TDbContext : DbContext
    {
        private readonly string? _srcCodeFilePath;

        public DesignTimeFactoryNG( string? srcCodeFilePath )
        {
            _srcCodeFilePath = srcCodeFilePath;

            var genOptionsType = typeof(DbContextOptions<>);
            var optionsType = genOptionsType.MakeGenericType( typeof(TDbContext) );

            var ctors = typeof(TDbContext).GetConstructors().ToList();
            if( ctors.Count == 0 )
                throw new ArgumentException( $"No constructors" );

            ctors = ctors.Where( x => x.GetParameters().Length == 2 ).ToList();
            if( ctors.Count == 0 )
                throw new ArgumentException( $"No constructors with 2 parameters" );

            ctors = ctors.Where( x => x.GetParameters()[ 0 ].ParameterType.IsAssignableFrom( optionsType ) ).ToList();
            if( ctors.Count == 0 )
                throw new ArgumentException( $"No constructors with 1st parameter assignable from {optionsType}" );

            ctors = ctors.Where( x => x.GetParameters()[ 1 ].ParameterType.IsAssignableTo( typeof(IDatabaseConfig) ) )
                .ToList();
            if( ctors.Count == 0 )
                throw new ArgumentException(
                    $"No constructors with 2nd parameter assignable to {typeof(IDatabaseConfig)}" );
        }

        public TDbContext CreateDbContext( string[] args )
        {
            var dbPath = string.IsNullOrEmpty( _srcCodeFilePath )
                ? args[ 0 ]
                : Path.Combine( Path.GetDirectoryName( _srcCodeFilePath )!, args[ 0 ] );

            var optionsBuilder = GetOptionsBuilder( dbPath );

            return (TDbContext) Activator.CreateInstance( typeof(TDbContext), new object?[]
            {
                optionsBuilder.Options,
                GetDatabaseConfig( dbPath )
            } )!;
        }

        protected abstract IDatabaseConfig GetDatabaseConfig( string dbPath );
        protected abstract DbContextOptionsBuilder<TDbContext> GetOptionsBuilder( string dbPath );
    }
}