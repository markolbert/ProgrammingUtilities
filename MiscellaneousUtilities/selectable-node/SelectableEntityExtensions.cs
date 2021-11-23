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
