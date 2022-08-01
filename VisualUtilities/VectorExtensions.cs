using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace J4JSoftware.VisualUtilities;

public static class VectorExtensions
{
    private struct Span
    {
        public Span()
        {
        }

        public float Minimum = float.PositiveInfinity;
        public float Maximum = float.NegativeInfinity;
    }

    public static Vector2 Perpendicular( this Vector2 vector, bool normalize = false )
    {
        var retVal = new Vector2(-vector.Y,vector.X);

        return normalize ? Vector2.Normalize(retVal) : retVal;
    }

    public static VectorPolygon Translate( this VectorPolygon polygon, Vector2 translate )
    {
        var transformation = Matrix3x2.CreateTranslation(translate);

        return VectorPolygon.Create( polygon.Vertices.Select( v => Vector2.Transform( v, transformation ) ).ToArray() )!;
    }

    public static VectorPolygon Translate( this VectorPolygon polygon, double xDelta, double yDelta ) =>
        polygon.Translate( new Vector2( Convert.ToSingle( xDelta ), Convert.ToSingle( yDelta ) ) );

    public static VectorPolygon Rotate(this VectorPolygon polygon, double degrees, Vector2? centerPoint = null )
    {
        centerPoint ??= polygon.Center;
        var radians = Convert.ToSingle( degrees * Math.PI / 180 );

        var transformation = Matrix3x2.CreateRotation( radians, centerPoint.Value );

        return VectorPolygon.Create(polygon.Vertices.Select(v => Vector2.Transform(v, transformation)).ToArray())!;
    }

    public static IEnumerable<Vector2> GetPerpendiculars( params VectorPolygon[] polygons )
    {
        foreach( var polygon in polygons )
        {
            foreach( var edge in polygon.Edges )
            {
                yield return edge.Perpendicular();
            }
        }
    }

    public static IEnumerable<Vector2> GetNormalizedPerpendiculars(params VectorPolygon[] polygons)
    {
        foreach (var polygon in polygons)
        {
            foreach (var edge in polygon.Edges)
            {
                yield return edge.Perpendicular(true);
            }
        }
    }

    public static bool Intersects( this VectorPolygon polygon1, VectorPolygon polygon2, bool throwIfNotConvex = true )
    {
        if( !polygon1.IsConvex || !polygon2.IsConvex )
        {
            if( throwIfNotConvex )
                throw new ArgumentException(
                    $"{nameof( Intersects )}(): one or both of the supplied polygons are not convex, cannot determine intersection" );

            return false;
        }

        foreach( var perpendicular in GetPerpendiculars( polygon1, polygon2 ) )
        {
            var span1 = perpendicular.GetProjectionSpan( polygon1 );
            var span2 = perpendicular.GetProjectionSpan( polygon2 );

            // this test passes if there is no "space" between the two polygons
            // along the current perpendicular direction.
            if( span1.Minimum < span2.Maximum && span1.Minimum > span2.Minimum
            || span2.Minimum < span1.Maximum && span2.Minimum > span1.Minimum )
                continue;

            // there is space between the two polygons along the current perpendicular,
            // so the polygons do not intersect
            return false;
        }

        // if none of the perpendiculars have space between the polygons
        // then they intersect somewhere
        return true;
    }

    public static bool Inside(this VectorPolygon polygon1, VectorPolygon polygon2, bool throwIfNotConvex = true)
    {
        if (!polygon1.IsConvex || !polygon2.IsConvex)
        {
            if (throwIfNotConvex)
                throw new ArgumentException(
                    $"{nameof(Intersects)}(): one or both of the supplied polygons are not convex, cannot determine intersection");

            return false;
        }

        var allIntersect = true;

        foreach (var perpendicular in GetPerpendiculars(polygon1, polygon2))
        {
            var span1 = perpendicular.GetProjectionSpan(polygon1);
            var span2 = perpendicular.GetProjectionSpan(polygon2);

            // the logical comparison here tests to see if there is no "space" between the two polygons
            // along the current perpendicular direction. If none of the perpendiculars show such space
            // -- if all the tests are true -- then polygon2 is inside polygon1
            allIntersect &= span1.Minimum < span2.Maximum && span1.Minimum > span2.Minimum
             || span2.Minimum < span1.Maximum && span2.Minimum > span1.Minimum;
        }

        return allIntersect;
    }

    private static Span GetProjectionSpan( this Vector2 perpendicular, VectorPolygon polygon )
    {
        var retVal = new Span();

        foreach (var vertex in polygon.Vertices)
        {
            var dp = Vector2.Dot(perpendicular, vertex);

            if( dp > retVal.Maximum)
                retVal.Maximum = dp;

            if (dp < retVal.Minimum)
                retVal.Minimum = dp;
        }

        return retVal;
    }

    public static Vector2 ChangeCoordinateSystem(this CoordinateSystem fromSystem, CoordinateSystem toSystem, Vector2 toConvert )
    {
        if (!Matrix4x4.Invert(toSystem.TransformMatrix, out var invertedSource))
            throw new ArgumentException($"Could not invert the source matrix");

        var matrixProduct = Matrix4x4.Multiply( invertedSource, fromSystem.TransformMatrix );

        var matrixArray = new float[2, 4];
        matrixArray[0, 0] = matrixProduct.M11;
        matrixArray[0, 1] = matrixProduct.M12;
        matrixArray[0, 2] = matrixProduct.M13;
        matrixArray[0, 3] = matrixProduct.M14;
        matrixArray[1, 0] = matrixProduct.M21;
        matrixArray[1, 1] = matrixProduct.M22;
        matrixArray[1, 2] = matrixProduct.M23;
        matrixArray[1, 3] = matrixProduct.M24;

        return ChangeCoordinateSystem(matrixArray, toConvert);
    }

    public static VectorPolygon? ChangeCoordinateSystem(
        this CoordinateSystem fromSystem,
        CoordinateSystem toSystem,
        VectorPolygon toConvert
    )
    {
        if( !Matrix4x4.Invert( toSystem.TransformMatrix, out var invertedSource ) )
            throw new ArgumentException( $"Could not invert the source matrix" );

        var matrixProduct = Matrix4x4.Multiply( invertedSource, fromSystem.TransformMatrix );

        var matrixArray = new float[ 2, 4 ];
        matrixArray[ 0, 0 ] = matrixProduct.M11;
        matrixArray[ 0, 1 ] = matrixProduct.M12;
        matrixArray[ 0, 2 ] = matrixProduct.M13;
        matrixArray[ 0, 3 ] = matrixProduct.M14;
        matrixArray[ 1, 0 ] = matrixProduct.M21;
        matrixArray[ 1, 1 ] = matrixProduct.M22;
        matrixArray[ 1, 2 ] = matrixProduct.M23;
        matrixArray[ 1, 3 ] = matrixProduct.M24;

        var newVertices = toConvert.Vertices.Select( v => ChangeCoordinateSystem( matrixArray, v ) );

        return VectorPolygon.Create( newVertices.ToArray() );
    }

    private static Vector2 ChangeCoordinateSystem( float[,] matrixArray, Vector2 toConvert )
    {
        var winV4 = new[] { toConvert.X, toConvert.Y, 1f, 0f };
        var retVal = new float[4];

        for (var row = 0; row < 2; row++)
        {
            for (var col = 0; col < 4; col++)
            {
                retVal[row] += matrixArray[row, col] * winV4[col];
            }
        }

        return new Vector2(retVal[0], retVal[1]);
    }
}