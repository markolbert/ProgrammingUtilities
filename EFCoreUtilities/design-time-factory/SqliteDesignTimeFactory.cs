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
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.EFCoreUtilities
{
    public class SqliteDesignTimeFactory<TDbContext> : DesignTimeFactory<TDbContext>
        where TDbContext : DbContext
    {
        public SqliteDesignTimeFactory( string? srcCodeFilePath )
            : base( srcCodeFilePath )
        {
        }

        protected override IDatabaseConfig GetDatabaseConfig( string dbPath ) =>
            new SqliteConfig()
            {
                CollationType = SqliteCollationType.CaseInsensitiveAtoZ,
                CreateNew = true,
                Path = dbPath
            };

        protected override DbContextOptionsBuilder<TDbContext> GetOptionsBuilder( string dbPath )
        {
            var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
            optionsBuilder.UseSqlite( $"Data Source={dbPath}" );

            return optionsBuilder;
        }
    }
}