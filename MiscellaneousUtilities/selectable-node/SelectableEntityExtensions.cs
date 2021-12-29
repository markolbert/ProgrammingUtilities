using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace J4JSoftware.Utilities
{
    public static class SelectableEntityExtensions
    {
        public static void SetTree<TEntity, TKey>( this ISelectableTree<TEntity, TKey> tree )
            where TEntity : class, ISelectableEntity<TEntity, TKey>
            where TKey : notnull
        {
            foreach (var rootEntity in tree.RootEntities)
            {
                rootEntity.SetBranch<TEntity, TKey>();
            }
        }

        public static void ClearTree<TEntity, TKey>(this ISelectableTree<TEntity, TKey> tree)
            where TEntity : class, ISelectableEntity<TEntity, TKey>
            where TKey : notnull
        {
            foreach (var rootEntity in tree.RootEntities)
            {
                rootEntity.ClearBranch<TEntity, TKey>();
            }
        }

        public static void SetBranch<TEntity, TKey>( this TEntity node )
            where TEntity : class, ISelectableEntity<TEntity, TKey>
            where TKey : notnull
        {
            foreach( var entity in node.DescendantEntitiesAndSelf<TEntity, TKey>() )
            {
                entity.IsSelected = true;
            }
        }

        public static void ClearBranch<TEntity, TKey>( this TEntity node )
            where TEntity : class, ISelectableEntity<TEntity, TKey>
            where TKey : notnull
        {
            foreach( var entity in node.DescendantEntitiesAndSelf<TEntity, TKey>() )
            {
                entity.IsSelected = false;
            }
        }

        public static IEnumerable<TEntity> DescendantEntitiesAndSelf<TEntity, TKey>(
            this TEntity rootEntity )
            where TEntity : class, ISelectableEntity<TEntity, TKey>
        {
            yield return rootEntity;

            foreach( var retVal in rootEntity.DescendantEntities<TEntity, TKey>() )
            {
                yield return retVal;
            }
        }

        public static IEnumerable<TEntity> DescendantEntities<TEntity, TKey>(
            this TEntity rootEntity )
            where TEntity : class, ISelectableEntity<TEntity, TKey>
        {
            foreach( var child in rootEntity.Children )
            {
                foreach ( var retVal in child.DescendantEntitiesAndSelf<TEntity, TKey>() )
                {
                    yield return retVal;
                }
            }
        }

        public static bool EntityOrDescendantSelected<TEntity, TKey>( this TEntity rootEntity )
            where TEntity : class, ISelectableEntity<TEntity, TKey>
        {
            foreach ( var child in rootEntity.DescendantEntitiesAndSelf<TEntity, TKey>() )
            {
                if( child.IsSelected )
                    return true;
            }

            return false;
        }

        public static bool EntityOrDescendantNotSelected<TEntity, TKey>( this TEntity rootEntity )
            where TEntity : class, ISelectableEntity<TEntity, TKey>
        {
            foreach( var child in rootEntity.DescendantEntitiesAndSelf<TEntity, TKey>() )
            {
                if( !child.IsSelected )
                    return false;
            }

            return true;
        }
    }
}
