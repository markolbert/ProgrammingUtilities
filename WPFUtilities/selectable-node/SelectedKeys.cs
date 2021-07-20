using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.WPFUtilities
{
    public class SelectedKeys<TKey, TEntity>
        where TKey : IComparable<TKey>
    {
        public static SelectedKeys<TKey, TEntity> Create( ISelectableTree<TKey, TEntity> tree )
        {
            var retVal = new SelectedKeys<TKey, TEntity>( default(TKey), false, null );

            foreach (var selectedNode in tree.Nodes
                .Where(x => x.Value.IsSelected)
                .Select(x => x.Value))
            {
                // find the path of nodes back to the root
                var pathNodes = new List<ISelectableNode<TKey, TEntity>>();
                var curNode = selectedNode;

                while (curNode.ParentNode != null)
                {
                    pathNodes.Add(curNode);
                    curNode = curNode.ParentNode;
                }

                pathNodes.Reverse();
                var curParent = retVal;

                foreach (var pathNode in pathNodes)
                {
                    var nextParent = curParent.Children
                        .FirstOrDefault( x => x.Key!.CompareTo( pathNode.Key ) == 0 );

                    if( nextParent == null )
                    {
                        nextParent = new SelectedKeys<TKey, TEntity>( pathNode.Key, pathNode.IsSelected, curParent );
                        curParent.Children.Add( nextParent );
                    }

                    curParent = nextParent;
                }
            }

            return retVal;
        }

        private SelectedKeys(
            TKey? key,
            bool isSelected,
            SelectedKeys<TKey, TEntity>? parent 
            )
        {
            Key = key;
            IsSelected = isSelected;
            Parent = parent;
        }

        public TKey? Key { get; }
        public bool IsSelected { get; }
        public SelectedKeys<TKey, TEntity>? Parent { get; }
        public List<SelectedKeys<TKey, TEntity>> Children { get; } = new();
    }
}
