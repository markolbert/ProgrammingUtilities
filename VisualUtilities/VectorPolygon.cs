using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;

namespace J4JSoftware.VisualUtilities;

public class VectorPolygon
{
    public static VectorPolygon? Create( params Vector2[] vertices )
    {
        if( vertices.Length < 3 )
            return null;

        var retVal = new VectorPolygon();
        
        foreach( var v in vertices )
        {
            retVal.AddVertex( v );
        }

        return retVal;
    }

    private List<Vector2> _vertices = new();
    private bool _isConvex;
    private bool _convexityDetermined;

    private VectorPolygon()
    {
    }

    public ReadOnlyCollection<Vector2> Vertices => _vertices.AsReadOnly();

    private void AddVertex( Vector2 vertex )
    {
        _vertices.Add( vertex );
        _vertices = _vertices.OrderBy( v => Math.Atan2( v.X - Center.X, v.Y - Center.Y ) )
                             .ToList();

        _convexityDetermined = false;
    }

    public Vector2 Center =>
        _vertices.Count == 0
            ? new Vector2( 0f, 0f )
            : new Vector2( Vertices.Sum( v => v.X ) / _vertices.Count,
                           Vertices.Sum( v => v.Y ) / _vertices.Count );

    public bool IsConvex
    {
        get
        {
            if( _convexityDetermined )
                return _isConvex;

            var prevCrossProduct = 0f;
            _isConvex = true;

            for( var vNum = 0; vNum < _vertices.Count; vNum++ )
            {
                var vNumPlus1 = vNum + 1;
                var vNumPlus2 = vNum + 2;

                vNumPlus1 = vNumPlus1 % _vertices.Count;
                vNumPlus2 = vNumPlus2 % _vertices.Count;

                var edge1 = _vertices[ vNumPlus1 ] - _vertices[ vNum ];
                var edge2 = _vertices[ vNumPlus2 ] - _vertices[ vNumPlus1 ];

                var crossProduct = edge1.X * edge2.Y - edge2.X * edge1.Y;

                if( crossProduct == 0 )
                    continue;

                if( crossProduct * prevCrossProduct < 0 )
                {
                    _isConvex = false;
                    break;
                }
                    
                prevCrossProduct = crossProduct;
            }

            _convexityDetermined = true;

            return _isConvex;
        }
    }

    public IEnumerable<Vector2> Edges
    {
        get
        {
            for( var vNum = 0; vNum < _vertices.Count; vNum++ )
            {
                var vNext = vNum == _vertices.Count - 1 ? 0 : vNum + 1;
                yield return _vertices[ vNext ] - _vertices[vNum];
            }
        }
    }

    public (float X, float Y) Minimum
    {
        get
        {
            (float X, float Y) retVal = (float.MaxValue, float.MaxValue);

            foreach (var vertex in _vertices)
            {
                if (vertex.X < retVal.X)
                    retVal.X = vertex.X;

                if (vertex.Y < retVal.Y)
                    retVal.Y = vertex.Y;
            }

            return retVal;
        }
    }

    public (float X, float Y) Maximum
    {
        get
        {
            (float X, float Y) retVal = (float.MinValue, float.MinValue);

            foreach (var vertex in _vertices)
            {
                if (vertex.X > retVal.X)
                    retVal.X = vertex.X;

                if (vertex.Y > retVal.Y)
                    retVal.Y = vertex.Y;
            }

            return retVal;
        }
    }

    public (float Width, float Height) BoundingRectangle
    {
        get
        {
            var min = Minimum;
            var max = Maximum;

            return (max.X - min.X, max.Y - min.Y);
        }
    }
}
