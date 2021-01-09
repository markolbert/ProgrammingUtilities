using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.EFCoreUtilities
{
    public interface IEntityConfiguration
    {
        void Configure( ModelBuilder builder );
    }

    public interface IEntityConfiguration<TEntity> : IEntityConfiguration
        where TEntity : class
    {
    }
}