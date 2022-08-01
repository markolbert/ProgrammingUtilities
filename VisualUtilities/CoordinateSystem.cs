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
