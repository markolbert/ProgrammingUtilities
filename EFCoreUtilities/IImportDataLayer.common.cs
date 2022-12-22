using System;

namespace J4JSoftware.QB2LGL;

public partial interface IImportDataLayer
{
    void LogPendingChanges();

    bool SaveChanges();

    TEntity? GetPendingEntity<TEntity>(
        Func<TEntity, bool> matchFunc
    )
        where TEntity : class;
}