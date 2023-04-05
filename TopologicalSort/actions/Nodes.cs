#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// 
// This file is part of J4JLogger.
//
// J4JLogger is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JLogger is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JLogger. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.Utilities;

public class Nodes<T>
    where T : class, IEquatable<T>
{
    private readonly IEqualityComparer<T>? _comparer;
    private readonly HashSet<NodeDependency<T>> _dependencies = new();
    private readonly HashSet<Node<T>> _nodes = new();

    public Nodes( IEqualityComparer<T>? comparer = null )
    {
        _comparer = comparer;
    }

    public void Clear()
    {
        _nodes.Clear();
        _dependencies.Clear();
    }

    public bool ValuesAreEqual( T x, T y ) => _comparer?.Equals( x, y ) ?? x.Equals( y );

    public bool NodesAreEqual( Node<T> x, Node<T> y ) =>
        _comparer?.Equals( x.Value, y.Value ) ?? x.Value.Equals( y.Value );

    public bool DependenciesAreEqual( NodeDependency<T> x, NodeDependency<T> y )
    {
        if( _comparer == null )
            return x.AncestorNode.Value.Equals( y.AncestorNode.Value )
             && x.DependentNode.Value.Equals( y.DependentNode.Value );

        return _comparer.Equals( x.AncestorNode.Value, y.AncestorNode.Value )
         && _comparer.Equals( x.DependentNode.Value, y.DependentNode.Value );
    }

    public List<Node<T>> GetDependents( Node<T> ancestor )
    {
        return _dependencies.Where( x => ValuesAreEqual( x.AncestorNode.Value, ancestor.Value ) )
                            .Select( x => x.DependentNode )
                            .Distinct()
                            .ToList();
    }

    public List<Node<T>> GetAncestors( Node<T> dependent )
    {
        return _dependencies.Where( x => ValuesAreEqual( x.DependentNode.Value, dependent.Value ) )
                            .Select( x => x.AncestorNode )
                            .Distinct()
                            .ToList();
    }

    public List<Node<T>> GetRoots()
    {
        return _nodes.Where( x => !_dependencies.Any( d => NodesAreEqual( d.AncestorNode, x ) ) )
                     .Select( x => x )
                     .Distinct()
                     .ToList();
    }

    public Node<T> AddIndependentNode( T value )
    {
        var retVal = _nodes.FirstOrDefault( n => ValuesAreEqual( n.Value, value ) );

        if( retVal != null )
            return retVal;

        retVal = new Node<T>( value, this, _comparer );
        _nodes.Add( retVal );

        return retVal;
    }

    public Node<T> AddDependentNode( T value, T ancestorValue )
    {
        var retVal = AddIndependentNode( value );
        var ancestor = AddIndependentNode( ancestorValue );

        if( ValuesAreEqual( value, ancestorValue ) )
            return ancestor;

        var dependency = new NodeDependency<T>( retVal, ancestor );

        if( !_dependencies.Any( x => DependenciesAreEqual( x, dependency ) ) )
            _dependencies.Add( dependency );

        return retVal;
    }

    public bool Remove( T toRemove )
    {
        var node = _nodes.FirstOrDefault( x => ValuesAreEqual( x.Value, toRemove ) );

        if( node == null )
            return false;

        var edgesToRemove = new List<NodeDependency<T>>();

        foreach( var dependency in _dependencies )
            if( ValuesAreEqual( dependency.AncestorNode.Value, toRemove )
            || ValuesAreEqual( dependency.DependentNode.Value, toRemove ) )
                edgesToRemove.Add( dependency );

        foreach( var edgeToRemove in edgesToRemove ) _dependencies.Remove( edgeToRemove );

        return _nodes.Remove( node );
    }

    public bool Sort( out List<T>? sorted, out List<NodeDependency<T>>? remainingEdges )
    {
        sorted = null;
        remainingEdges = null;

        switch( _nodes.Count )
        {
            case 0:
                return false;

            case 1:
                if( _dependencies.Count > 0 )
                    return false;

                break;
        }

        // Empty list that will contain the sorted elements
        var retVal = new List<Node<T>>();

        // work with a copy of edges so we can keep re-sorting
        var dependencies = new HashSet<NodeDependency<T>>( _dependencies.ToArray() );

        // Set of all nodes with no incoming edges
        var noIncomingEdges = new HashSet<Node<T>>( _nodes.Where( n =>
                                                                      dependencies
                                                                         .All( e =>
                                                                                   !NodesAreEqual( e
                                                                                          .DependentNode,
                                                                                       n ) ) ) );

        // while noIncomingEdges is non-empty do
        while( noIncomingEdges.Any() )
        {
            //  remove a node from noIncomingEdges
            var nodeToRemove = noIncomingEdges.First();
            noIncomingEdges.Remove( nodeToRemove );

            // add removed node to return value
            retVal.Add( nodeToRemove );

            foreach( var edge in dependencies
                                .Where( e => NodesAreEqual( e.AncestorNode, nodeToRemove ) )
                                .ToList() )
            {
                var targetNode = edge.DependentNode;

                // remove edge from the graph
                dependencies.Remove( edge );

                // if targetNode has no other incoming edges then
                if( dependencies.All( x => !NodesAreEqual( x.DependentNode, targetNode ) ) )

                    // insert targetNode into noIncomingEdges
                    noIncomingEdges.Add( targetNode );
            }
        }

        remainingEdges = dependencies.ToList();

        if( dependencies.Any() )
            return false;

        sorted = retVal.Select( x => x.Value )
                       .ToList();

        return true;
    }
}