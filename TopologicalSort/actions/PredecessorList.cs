using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using J4JSoftware.Logging;

namespace J4JSoftware.Utilities
{
    public static class TopologicalSortExtensions
    {
        public static Nodes<T> ToNodeList<T>( 
            this IEnumerable<T> source, 
            IEqualityComparer<T>? comparer = null,
            IJ4JLogger? logger = null )
            where T: class, IEquatable<T>
        {
            var retVal = new Nodes<T>( comparer );

            var idx = 0;
            var nodeList = source.ToList();

            foreach( var node in nodeList )
            {
                var nodeType = node.GetType();

                var predAttr = nodeType.GetCustomAttribute<PredecessorAttribute>(false);
                if( predAttr == null )
                {
                    logger?.Error("Node type '{0}' (item {1}) is not decorated with a PredecessorAttribute", nodeType, idx );
                    continue;
                }

                if( predAttr.Predecessor == null )
                    retVal.AddIndependentNode( node );
                else
                {
                    var predecessor = nodeList.FirstOrDefault(x => x.GetType() == predAttr.Predecessor);
                    if (predecessor == null)
                    {
                        logger?.Fatal<string>( "Couldn't find predecessor extractor {0}", predAttr.Predecessor.Name );
                        continue;
                    }

                    retVal.AddDependentNode(node, predecessor);
                }

                idx++;
            }

            return retVal;
        }
    }
}
