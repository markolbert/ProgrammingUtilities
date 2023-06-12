#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// Rectangle2D.cs
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace J4JSoftware.VisualUtilities;

public record Rectangle2D : IEnumerable<Vector3>
{
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

    public static readonly Rectangle2D Empty = new Rectangle2D( 0, 0, 0, 0, CoordinateSystem2D.Display );

    public const float DefaultComparisonTolerance = 1E-4F;

    private readonly Matrix4x4 _inverseUnitTransform;

    private Rectangle2D(
        float minX,
        float maxX,
        float minY,
        float maxY,
        CoordinateSystem2D coordinateSystem
    )
    {
        LowerLeft = new Vector3( minX, maxY, 0 );
        UpperLeft = new Vector3( minX, minY, 0 );
        UpperRight = new Vector3( maxX, minY, 0 );
        LowerRight = new Vector3( maxX, maxY, 0 );

        Height = Vector3.Distance(LowerLeft, UpperLeft);
        Width = Vector3.Distance(UpperLeft, UpperRight);

        Center = new Vector3( ( minX + maxX ) / 2, ( minY + maxY ) / 2, 0 );
        BoundingBox = this;
    }

    public Rectangle2D(
        float height,
        float width,
        float rotation = 0,
        Vector3? center = null,
        CoordinateSystem2D coordinateSystem = CoordinateSystem2D.Cartesian,
        float comparisonTolerance = DefaultComparisonTolerance
    )
        : this(CreateCorners(height, width, rotation, center), coordinateSystem, comparisonTolerance)
    {
    }

    public Rectangle2D Copy()
    {
        var retVal = (Rectangle2D) this.MemberwiseClone();
        retVal.BoundingBox = (Rectangle2D) retVal.BoundingBox.MemberwiseClone();

        return retVal;
    }

    public Rectangle2D(
        Vector3[] points,
        CoordinateSystem2D coordinateSystem = CoordinateSystem2D.Cartesian,
        float comparisonTolerance = DefaultComparisonTolerance
    )
    {
        if( points.Length != 4 )
            throw new ArgumentException( "points array must have length == 4" );

        CoordinateSystem = coordinateSystem;
        ComparisonTolerance = comparisonTolerance;

        var pointList = points.ToList();

        var minX = points.Min( p => p.X );
        var minY = CoordinateSystem switch
        {
            CoordinateSystem2D.Cartesian => points.Min( p => p.Y ),
            CoordinateSystem2D.Display => points.Max( p => p.Y ),
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {nameof( CoordinateSystem )} value '{CoordinateSystem}'" )
        };

        var maxX = points.Max( p => p.X );
        var maxY = CoordinateSystem switch
        {
            CoordinateSystem2D.Cartesian => points.Max( p => p.Y ),
            CoordinateSystem2D.Display => points.Min( p => p.Y ),
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {nameof( CoordinateSystem )} value '{CoordinateSystem}'" )
        };

        if ( maxX < minX )
            ( minX, maxX ) = ( maxX, minX );

        if (maxY < minY)
            (minY, maxY) = (maxY, minY);

        BoundingBox = new Rectangle2D( minX, maxX, minY, maxY, CoordinateSystem );

        LowerLeft = GetCornerPoint( p => Math.Abs( p.X - minX ) < ComparisonTolerance,
                                    p => p.Y < maxY,
                                    ref pointList );

        UpperLeft = GetCornerPoint( p => Math.Abs( p.Y - maxY ) < ComparisonTolerance,
                                    p => p.X < maxX,
                                    ref pointList );

        ( LowerLeft, UpperLeft ) = CoordinateSystem switch
        {
            CoordinateSystem2D.Cartesian => ( LowerLeft, UpperLeft ),
            CoordinateSystem2D.Display => ( UpperLeft, LowerLeft ),
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {nameof( CoordinateSystem )} value '{CoordinateSystem}'" )
        };

        UpperRight = GetCornerPoint( p => Math.Abs( p.X - maxX ) < ComparisonTolerance,
                                     p => p.Y > minY,
                                     ref pointList );

        LowerRight = pointList[ 0 ];

        (LowerRight, UpperRight) = CoordinateSystem switch
        {
            CoordinateSystem2D.Cartesian => (LowerRight, UpperRight),
            CoordinateSystem2D.Display => (UpperRight, LowerRight),
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {nameof(CoordinateSystem)} value '{CoordinateSystem}'")
        };

        Height = Vector3.Distance( LowerLeft, UpperLeft );
        Width = Vector3.Distance( UpperLeft, UpperRight );

        var corners = new Vector3[] { UpperLeft, UpperRight, LowerLeft, LowerRight };
        Center = new Vector3( ( corners.Max( c => c.X ) + corners.Min( c => c.X ) ) / 2,
                              ( corners.Max( c => c.Y ) + corners.Min( c => c.Y ) ) / 2,
                              0 );

        var scaleTransform = Matrix4x4.CreateScale(Width, Height, 1 );
        var translationTransform = Matrix4x4.CreateTranslation( LowerLeft.X, LowerRight.Y, 0 );
        var unitTransform = scaleTransform * translationTransform;

        if( Matrix4x4.Invert( unitTransform, out _inverseUnitTransform ) )
            return;

        throw new InvalidOperationException( "Could not create inverse unit transform" );
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
    public CoordinateSystem2D CoordinateSystem { get; }

    public Vector3 LowerLeft { get; }
    public Vector3 UpperLeft { get; }
    public Vector3 UpperRight { get; }
    public Vector3 LowerRight { get; }

    public Vector3 Center { get; }
    public float Height { get; }
    public float Width { get; }
    public Rectangle2D BoundingBox { get; private set; }

    public Vector3 this[ int idx ] =>
        idx switch
        {
            0 => LowerLeft,
            1 => UpperLeft,
            2 => UpperRight,
            3 => LowerRight,
            _ => throw new IndexOutOfRangeException("Index must be >=0 and <= 3")
        };

    // thanx to Nick Alger for this!
    // https://math.stackexchange.com/questions/190111/how-to-check-if-a-point-is-inside-a-rectangle/1685315#1685315?newreg=dea69604c07d4329b2944256e006a34f
    public RelativePosition2D Contains( Rectangle2D toCheck )
    {
        // test for identity
        var sameCorners = true;

        for( var idx = 0; idx < 4; idx++ )
        {
            if( this[ idx ] == toCheck[ idx ] )
                continue;

            sameCorners = false;
            break;
        }

        if( sameCorners )
            return RelativePosition2D.Edge;

        var retVal = RelativePosition2D.Inside;

        foreach( var corner in toCheck )
        {
            retVal = Contains( corner );

            if( retVal == RelativePosition2D.Outside )
                break;
        }

        return retVal;
    }

    public RelativePosition2D Contains( Vector3 point )
    {
        var transformed = Vector3.Transform( point, _inverseUnitTransform );

        // range tests are on the unit square
        if( InRange( transformed.Y, 0, 1 ) )
        {
            if( OnEdge( transformed.X, 0 ) || OnEdge( transformed.X, 1 ) )
                return RelativePosition2D.Edge;

            return InRange( transformed.X, 0, 1 ) 
                ? RelativePosition2D.Inside 
                : RelativePosition2D.Outside;
        }

        return RelativePosition2D.Outside;
    }

    public IEnumerable<Vector3> GetExternalCorners( Rectangle2D toCheck ) =>
        toCheck.Where( corner => Contains( corner ) == RelativePosition2D.Outside );

    private bool OnEdge( float toCheck, float edgeValue ) => Math.Abs( toCheck - edgeValue ) < ComparisonTolerance;

    private bool InRange( float toCheck, float min, float max ) => toCheck >= min && toCheck <= max;

    public IEnumerable<Edge2D> GetEdges()
    {
        yield return new Edge2D( LowerLeft, UpperLeft );
        yield return new Edge2D( UpperLeft, UpperRight );
        yield return new Edge2D( UpperRight, LowerRight );
        yield return new Edge2D( LowerRight, LowerLeft );
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
