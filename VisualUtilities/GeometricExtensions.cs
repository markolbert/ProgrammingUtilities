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

    public static Vector3 Perpendicular( this Vector3 vector, float magnitude = 1 ) =>
        new Vector3( magnitude, -vector.X * magnitude / vector.Y, 0 );
}
