#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'TopologicalSort' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.Utilities
{
    // thanx to https://gist.github.com/Sup3rc4l1fr4g1l1571c3xp14l1d0c10u5/3341dba6a53d7171fe3397d13d00ee3f for the Kahn's
    // topological sorting algorithm!!
    public static class TopologicalSorter
    {
        public static bool Sort<TNode>(
            IEnumerable<TNode> items,
            out List<TNode>? result
        )
            where TNode : class, ISortable<TNode>
        {
            var available = items.ToList();

            result = null;

            switch( available.Count( x => x.Predecessor == null ) )
            {
                case 0:
                    // no root item
                    return false;

                case 1:
                    // no op; expected case
                    break;

                default:
                    // multiple root items
                    return false;
            }

            var nodes = new HashSet<TNode>();
            var edges = new HashSet<(TNode start, TNode end)>();

            foreach( var item in available )
            {
                nodes.Add( item );

                var predecessor = item.Predecessor == null
                    ? null
                    : available.FirstOrDefault( x => item.Predecessor.Equals( x ) );

                if( predecessor != null )
                    edges.Add( ( item, predecessor ) );
            }

            // Empty list that will contain the sorted elements
            var retVal = new Stack<TNode>();

            // Set of all nodes with no incoming edges
            var noIncomingEdges =
                new HashSet<TNode>( nodes.Where( n => edges.All( e => e.end.Equals( n ) == false ) ) );

            // while noIncomingEdges is non-empty do
            while( noIncomingEdges.Any() )
            {
                //  remove a node from noIncomingEdges
                var nodeToRemove = noIncomingEdges.First();
                noIncomingEdges.Remove( nodeToRemove );

                // add removed node to stack
                retVal.Push( nodeToRemove );

                // for each targetNode with an edge from nodeToRemove to targetNode do
                foreach( var edge in edges.Where( e => e.start.Equals( nodeToRemove ) ).ToList() )
                {
                    var targetNode = edge.end;

                    // remove edge from the graph
                    edges.Remove( edge );

                    // if targetNode has no other incoming edges then
                    if( edges.All( me => me.end.Equals( targetNode ) == false ) )
                        // insert targetNode into noIncomingEdges
                        noIncomingEdges.Add( targetNode );
                }
            }

            if( edges.Any() )
                return false;

            result = retVal.ToList();

            return true;
        }
    }
}