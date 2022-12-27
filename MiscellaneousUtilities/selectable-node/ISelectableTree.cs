using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Versioning;

namespace J4JSoftware.Utilities;

[RequiresPreviewFeatures("Experimental, subject to change or removal")]
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