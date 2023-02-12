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

    public static PlanePosition RelationshipToEdge(this Edge3 edge, Vector3 point)
    {
        var temp = (edge.Point2.X - edge.Point1.X) * (point.Y - edge.Point1.Y)
          - (edge.Point2.Y - edge.Point1.Y) * (point.X - edge.Point1.X);

        return temp switch
        {
            < 0 => PlanePosition.Right,
            > 0 => PlanePosition.Left,
            _ => PlanePosition.OnTheLine
        };
    }
}
