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
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace J4JSoftware.EFCoreUtilities.Deprecated
{
    public abstract class DesignTimeFactory<TDbContext> : IDesignTimeDbContextFactory<TDbContext>
        where TDbContext : DbContext

    {
        private readonly ConstructorInfo _ctor;

        public DesignTimeFactory()
        {
            // ensure TDbContext can be created from a IDbContextFactoryConfiguration
            var ctor = typeof(TDbContext).GetConstructor( new[] { typeof(IDbContextFactoryConfiguration) } );

            if( ctor == null )
                throw new ArgumentException(
                    $"{typeof(TDbContext).Name} cannot be constructed from a {nameof(IDbContextFactoryConfiguration)}" );

            _ctor = ctor;
        }

        public virtual TDbContext CreateDbContext( string[] args )
        {
            return (TDbContext) _ctor.Invoke( new object[] { GetDatabaseConfiguration() } );
        }

        protected abstract IDbContextFactoryConfiguration GetDatabaseConfiguration();
    }
}