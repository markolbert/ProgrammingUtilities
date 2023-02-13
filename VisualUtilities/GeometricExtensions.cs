using System.Linq;
using System.Numerics;

namespace J4JSoftware.VisualUtilities;

public static class GeometricExtensions
{
    public static Vector3[] ApplyTransform(this Vector3[] points, Matrix4x4 transform)
    {
        var retVal = new Vector3[points.Length];

        for (var idx = 0; idx < points.Length; idx++)
        {
            retVal[idx] = Vector3.Transform(points[idx], transform);
        }

        return retVal;
    }

    public static Rectangle2D ApplyTransform(
        this Rectangle2D rectangle,
        Matrix4x4 transform,
        CoordinateSystem2D? coordinateSystem = null
    )
    {
        coordinateSystem ??= rectangle.CoordinateSystem;

        var corners = rectangle.ToArray();
        var transformed = corners.ApplyTransform( transform );

        return new Rectangle2D( transformed, coordinateSystem.Value, rectangle.ComparisonTolerance );
    }

    public static Rectangle2D ToDisplaySpace(
        this Rectangle2D rectangle,
        Vector3 viewport
    )
    {
        var translateTransform = Matrix4x4.CreateTranslation( viewport.X / 2,
                                                              -viewport.Y / 2,
                                                              0 );
        
        var mirrorYTransform = Matrix4x4.CreateScale(1, -1, 1);

        var combinedTransform = translateTransform * mirrorYTransform;

        var transformedRect = rectangle.ApplyTransform( combinedTransform, CoordinateSystem2D.Display );

        return transformedRect;
    }

    public static Rectangle2D FromDisplaySpace(
        this Rectangle2D rectangle,
        Vector3 viewport
    )
    {
        var translateTransform = Matrix4x4.CreateTranslation(
            -viewport.X/2,
            viewport.Y/2,
            0);

        var mirrorYTransform = Matrix4x4.CreateScale( 1, -1, 1 );
        
        var combinedTransform = mirrorYTransform * translateTransform;

        var transformedRect = rectangle.ApplyTransform( combinedTransform, CoordinateSystem2D.Cartesian );

        return transformedRect;
    }

    public static Vector3 Perpendicular( this Vector3 vector, float magnitude = 1 ) =>
        new( magnitude, -vector.X * magnitude / vector.Y, 0 );
}
