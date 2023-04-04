// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of TopologicalSort.
//
// TopologicalSort is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// TopologicalSort is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with TopologicalSort. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.Utilities;

public static class TopologicalSortExtensions
{
    public static Nodes<T> ToNodeList<T>( 
        this IEnumerable<T> source, 
        IEqualityComparer<T>? comparer = null,
        ILogger? logger = null )
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
                logger?.LogError("Node type '{0}' (item {1}) is not decorated with a PredecessorAttribute", nodeType, idx );
                continue;
            }

            if( predAttr.Predecessor == null )
                retVal.AddIndependentNode( node );
            else
            {
                var predecessor = nodeList.FirstOrDefault(x => x.GetType() == predAttr.Predecessor);
                if (predecessor == null)
                {
                    logger?.LogCritical( "Couldn't find predecessor extractor {0}", predAttr.Predecessor.Name );
                    continue;
                }

                retVal.AddDependentNode(node, predecessor);
            }

            idx++;
        }

        return retVal;
    }
}