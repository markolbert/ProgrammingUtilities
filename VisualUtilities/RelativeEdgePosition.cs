#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// RelativeEdgePosition.cs
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

public record RelativeEdgePosition
{
    public static RelativeEdgePosition Create( Rectangle2D rect, Vector3 point )
    {
        var retVal = new RelativeEdgePosition() { BaseRectangle = rect, Point = point };

        if( rect.Contains( point ) != RelativePosition.Outside )
            return retVal;

        if( point.X < rect.Min( c => c.X ) )
            retVal.NearestEdge |= NearestEdge.Left;

        if( point.X > rect.Max( c => c.X ) )
            retVal.NearestEdge |= NearestEdge.Right;

        if( rect.CoordinateSystem == CoordinateSystem2D.Cartesian )
        {
            if( point.Y < rect.Min( c => c.Y ) )
                retVal.NearestEdge |= NearestEdge.Top;

            if( point.Y > rect.Max( c => c.Y ) )
                retVal.NearestEdge |= NearestEdge.Bottom;
        }
        else
        {
            if( point.Y > rect.Max( c => c.Y ) )
                retVal.NearestEdge |= NearestEdge.Top;

            if( point.Y < rect.Min( c => c.Y ) )
                retVal.NearestEdge |= NearestEdge.Bottom;
        }

        return retVal;
    }

    public Rectangle2D BaseRectangle { get; private set; } = Rectangle2D.Empty;
    public Vector3 Point { get; private set; }
    public NearestEdge NearestEdge { get; private set; }
}
