using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace J4JSoftware.VisualUtilities;

public record Rectangle2D : IEnumerable<Vector3>
{
    public const float DefaultComparisonTolerance = 1E-4F;

    private record IndexedPoint( Vector3 Point, int Index );

    #region IEquatable...

    public virtual bool Equals( Rectangle2D? other )
    {
        if( ReferenceEquals( null, other ) )
            return false;
        if( ReferenceEquals( this, other ) )
            return true;

        for( var idx = 0; idx < 4; idx++ )
        {
            if( Math.Abs( this[ idx ].X - other[ idx ].X ) > ComparisonTolerance )
                return false;

            if (Math.Abs(this[idx].Y - other[idx].Y) > ComparisonTolerance)
                return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        var temp = this.ToList();
        return temp.GetHashCode();
    }

    #endregion

    private Rectangle2D(
        float minX,
        float maxX,
        float minY,
        float maxY
    )
    {
        LowerLeft = new Vector3( minX, maxY, 0 );
        UpperLeft = new Vector3( minX, minY, 0 );
        UpperRight = new Vector3( maxX, minY, 0 );
        LowerRight = new Vector3( maxX, maxY, 0 );

        BoundingBox = this;
    }

    public Rectangle2D(
        Vector3[] points,
        float comparisonTolerance = DefaultComparisonTolerance
    )
    {
        if( points.Length != 4 )
            throw new ArgumentException( "points array must have length == 4" );

        ComparisonTolerance = comparisonTolerance;

        var pointList = points.ToList();

        var minX = points.Min( p => p.X );
        var minY = points.Min( p => p.Y );
        var maxX = points.Max( p => p.X );
        var maxY = points.Max( p => p.Y );

        if( maxX < minX )
            ( minX, maxX ) = ( maxX, minX );

        if( maxY < minY )
            ( minY, maxY ) = ( maxY, minY );

        BoundingBox = new Rectangle2D( minX, maxX, minY, maxY );

        LowerLeft = GetCornerPoint( p => Math.Abs( p.X - minX ) < ComparisonTolerance,
                                    p => p.Y < maxY,
                                    ref pointList );

        UpperLeft = GetCornerPoint( p => Math.Abs( p.Y - maxY ) < ComparisonTolerance,
                                    p => p.X < maxX,
                                    ref pointList );

        UpperRight = GetCornerPoint( p => Math.Abs( p.X - maxX ) < ComparisonTolerance,
                                     p => p.Y > minY,
                                     ref pointList );

        LowerRight = pointList[ 0 ];
    }

    public Rectangle2D(
        float height,
        float width,
        float rotation = 0,
        Vector3? center = null,
        float comparisonTolerance = DefaultComparisonTolerance
    )
        : this( CreateCorners( height, width, rotation, center ), comparisonTolerance )
    {
    }

    private static Vector3[] CreateCorners(float height, float width, float rotation, Vector3? center)
    {
        center ??= new Vector3(width / 2F, height / 2F, 0);

        var retVal = new Vector3[]
        {
            new( center.Value.X - width / 2F, center.Value.Y + height / 2F, 0 ),
            new( center.Value.X + width / 2F, center.Value.Y + height / 2F, 0 ),
            new( center.Value.X + width / 2F, center.Value.Y - height / 2F, 0 ),
            new( center.Value.X - width / 2F, center.Value.Y - height / 2F, 0 )
        };

        retVal = retVal.ApplyTransform(
            Matrix4x4.CreateRotationZ(rotation * GeometricConstants.RadiansPerDegree, center.Value));

        return retVal;
    }

    private Vector3 GetCornerPoint( Func<Vector3, bool> comparer, Func<Vector3, bool> filter, ref List<Vector3> points )
    {
        var leftmost = points.Where( comparer )
                             .ToList();

        var retVal = leftmost.Count == 1
            ? leftmost[ 0 ]
            : leftmost.First( filter );

        var toRemove = points.FindIndex( p => p.Equals( retVal ) );
        points.RemoveAt( toRemove );

        return retVal;
    }

    public float ComparisonTolerance { get; }

    public Vector3 LowerLeft { get; }
    public Vector3 UpperLeft { get; }
    public Vector3 UpperRight { get; }
    public Vector3 LowerRight { get; }

    public Rectangle2D BoundingBox { get; }

    public Vector3 this[ int idx ] =>
        idx switch
        {
            0 => LowerLeft,
            1 => UpperLeft,
            2 => UpperRight,
            3 => LowerRight,
            _ => throw new IndexOutOfRangeException("Index must be >=0 and <= 3")
        };

    public ConvexPolygonPosition Contains( Rectangle2D inner )
    {
        // test for identity
        var sameCorners = true;

        for( var idx = 0; idx < 4; idx++ )
        {
            if( this[ idx ] == inner[ idx ] )
                continue;

            sameCorners = false;
            break;
        }

        if( sameCorners )
            return ConvexPolygonPosition.Same;

        foreach( var edge in GetEdges() )
        {
            foreach( var point in inner )
            {
                if( edge.RelationshipToEdge( point ) != PlanePosition.Left )
                    return ConvexPolygonPosition.Outside;
            }
        }

        return ConvexPolygonPosition.Inside;
    }

    public bool Contains( Vector3 point )
    {
        foreach( var edge in GetEdges() )
        {
            if( edge.RelationshipToEdge( point ) != PlanePosition.Left )
                return false;
        }

        return true;
    }

    public IEnumerable<Edge3> GetEdges()
    {
        yield return new Edge3( LowerLeft, UpperLeft );
        yield return new Edge3( UpperLeft, UpperRight );
        yield return new Edge3( UpperRight, LowerRight );
        yield return new Edge3( LowerRight, LowerLeft );
    }

    public IEnumerator<Vector3> GetEnumerator()
    {
        yield return LowerLeft;
        yield return UpperLeft;
        yield return UpperRight;
        yield return LowerRight;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
