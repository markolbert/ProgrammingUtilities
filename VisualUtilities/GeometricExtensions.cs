#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// GeometricExtensions.cs
//
// This file is part of JumpForJoy Software's VisualUtilities.
// 
// VisualUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// VisualUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with VisualUtilities. If not, see <https://www.gnu.org/licenses/>.
#endregion

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
