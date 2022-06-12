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

    public static Polygon Translate( this Polygon polygon, Vector2 translate )
    {
        var transformation = Matrix3x2.CreateTranslation(translate);

        return Polygon.Create( polygon.Vertices.Select( v => Vector2.Transform( v, transformation ) ).ToArray() )!;
    }

    public static Polygon Translate( this Polygon polygon, double xDelta, double yDelta ) =>
        polygon.Translate( new Vector2( Convert.ToSingle( xDelta ), Convert.ToSingle( yDelta ) ) );

    public static Polygon Rotate(this Polygon polygon, double degrees, Vector2? centerPoint = null )
    {
        centerPoint ??= polygon.Center;
        var radians = Convert.ToSingle( degrees * Math.PI / 180 );

        var transformation = Matrix3x2.CreateRotation( radians, centerPoint.Value );

        return Polygon.Create(polygon.Vertices.Select(v => Vector2.Transform(v, transformation)).ToArray())!;
    }

    public static IEnumerable<Vector2> GetPerpendiculars( params Polygon[] polygons )
    {
        foreach( var polygon in polygons )
        {
            foreach( var edge in polygon.Edges )
            {
                yield return edge.Perpendicular();
            }
        }
    }

    public static IEnumerable<Vector2> GetNormalizedPerpendiculars(params Polygon[] polygons)
    {
        foreach (var polygon in polygons)
        {
            foreach (var edge in polygon.Edges)
            {
                yield return edge.Perpendicular(true);
            }
        }
    }

    public static bool Intersects( this Polygon polygon1, Polygon polygon2, bool throwIfNotConvex = true )
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

            if( span1.Minimum < span2.Maximum && span1.Minimum > span2.Minimum
            || span2.Minimum < span1.Maximum && span2.Minimum > span1.Minimum )
                continue;

            return false;
        }

        return true;
    }

    private static Span GetProjectionSpan( this Vector2 perpendicular, Polygon polygon )
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
}