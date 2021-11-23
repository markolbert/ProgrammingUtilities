using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace J4JSoftware.Utilities
{
    public interface ISelectableTree
    {
        void SetAll( bool isSelected );
        //object? AddOrGetEntity( object entity );
        //void AddOrGetEntities( IEnumerable<object> entities );
    }

    public interface ISelectableTree<TEntity, TKey> : ISelectableTree
        where TEntity : class, ISelectableEntity<TEntity, TKey>
        where TKey : notnull
    {
        ObservableCollection<TEntity> RootEntities { get; }

        bool Load( List<TEntity> entities );
        bool Load( Dictionary<TKey, TEntity> entities );

        IEnumerable<TEntity> SelectedEntities();
        IEnumerable<TEntity> UnselectedEntities();

        //TEntity AddOrGetEntity( TEntity entity );
        //void AddOrGetEntities( IEnumerable<TEntity> entities );

        bool FindEntity( TKey key, out TEntity? result );
        //void Sort( IComparer<TEntity>? sortComparer = null );
    }
}
