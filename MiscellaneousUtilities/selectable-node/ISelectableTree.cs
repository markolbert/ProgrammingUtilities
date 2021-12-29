using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace J4JSoftware.Utilities
{
    public interface ISelectableTree<TEntity, TKey>
        where TEntity : class, ISelectableEntity<TEntity, TKey>
        where TKey : notnull
    {
        ObservableCollection<TEntity> RootEntities { get; }

        bool Load( List<TEntity> entities );
        bool Load( Dictionary<TKey, TEntity> entities );

        IEnumerable<TEntity> SelectedEntities();
        IEnumerable<TEntity> UnselectedEntities();

        bool FindEntity( TKey key, out TEntity? result );
    }
}
