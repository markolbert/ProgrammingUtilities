﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.EFCoreUtilities
{
    public abstract class EntityConfigurator<TEntity> : IEntityConfiguration<TEntity>
        where TEntity : class
    {
        protected abstract void Configure( EntityTypeBuilder<TEntity> builder );

        void IEntityConfiguration.Configure( ModelBuilder builder )
        {
            Configure( builder.Entity<TEntity>() );
        }
    }
}