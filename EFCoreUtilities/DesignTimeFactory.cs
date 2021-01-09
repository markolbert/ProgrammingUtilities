using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace J4JSoftware.EFCoreUtilities
{
    public abstract class DesignTimeFactory<TDbContext> : IDesignTimeDbContextFactory<TDbContext>
        where TDbContext : DbContext

    {
        private readonly ConstructorInfo _ctor;

        public DesignTimeFactory()
        {
            // ensure TDbContext can be created from a IDbContextFactoryConfiguration
            _ctor = typeof(TDbContext).GetConstructor( new Type[] { typeof(IDbContextFactoryConfiguration) } );

            if( _ctor == null )
                throw new ArgumentException(
                    $"{typeof(TDbContext).Name} cannot be constructed from a {nameof(IDbContextFactoryConfiguration)}" );
        }

        public virtual TDbContext CreateDbContext( string[] args )
        {
            return (TDbContext) _ctor.Invoke( new object[] { GetDatabaseConfiguration() } );
        }

        protected abstract IDbContextFactoryConfiguration GetDatabaseConfiguration();
    }
}
