// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of VisualUtilities.
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

using System.ComponentModel;
using System.Numerics;

namespace J4JSoftware.VisualUtilities;

public class CoordinateSystem
{
    public static CoordinateSystem DefaultCartesian { get; } = GetCartesian();

    public static CoordinateSystem GetCartesian( float xOriginCommonSystem = 0f, float yOriginCommonSystem = 0f ) =>
        new(XAxisDirection.RightIsIncrease, YAxisDirection.UpIsIncrease)
        {
            XOriginCartesian = xOriginCommonSystem,
            YOriginCartesian = yOriginCommonSystem
        };

    public static CoordinateSystem GetWindows(float xOriginCommonSystem, float yOriginCommonSystem) =>
        new(XAxisDirection.RightIsIncrease, YAxisDirection.DownIsIncrease)
        {
            XOriginCartesian = xOriginCommonSystem,
            YOriginCartesian = yOriginCommonSystem
        };

    private float _xOrigin;
    private float _yOrigin;
    private Matrix4x4? _transformMatrix;

    public CoordinateSystem(
        XAxisDirection xAxisDirection,
        YAxisDirection yAxisDirection
    )
    {
        XAxisDirection = xAxisDirection;
        YAxisDirection = yAxisDirection;
    }

    public XAxisDirection XAxisDirection { get; }
    public YAxisDirection YAxisDirection { get; }

    public float XOriginCartesian
    {
        get => _xOrigin;

        set
        {
            _xOrigin = value;
            _transformMatrix = null;
        }
    }

    public float YOriginCartesian
    {
        get => _yOrigin;

        set
        {
            _yOrigin = value;
            _transformMatrix = null;
        }
    }

    public void OffsetCartesian( float xOffset, float yOffset )
    {
        _xOrigin += XAxisDirection switch
        {
            XAxisDirection.RightIsIncrease => xOffset,
            XAxisDirection.LeftIsIncrease => -xOffset,
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {nameof( XAxisDirection )} value '{XAxisDirection}'" )
        };

        _yOrigin += YAxisDirection switch
        {
            YAxisDirection.UpIsIncrease => -yOffset,
            YAxisDirection.DownIsIncrease => yOffset,
            _ => throw new InvalidEnumArgumentException(
                $"Unsupported {nameof( YAxisDirection )} value '{YAxisDirection}'")
        };
    }

    public Matrix4x4 TransformMatrix
    {
        get
        {
            if( _transformMatrix != null )
                return _transformMatrix.Value;

            var retVal = Matrix4x4.Identity;

            retVal.M11 = XAxisDirection == XAxisDirection.RightIsIncrease ? 1f : -1f;
            retVal.M13 = _xOrigin;

            retVal.M22 = YAxisDirection == YAxisDirection.UpIsIncrease ? 1f : -1f;
            retVal.M23 = _yOrigin;

            _transformMatrix = retVal;

            return retVal;
        }
    }
}
