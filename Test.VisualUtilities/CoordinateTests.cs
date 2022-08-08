using System.Numerics;
using FluentAssertions;
using J4JSoftware.VisualUtilities;
using Xunit;

namespace Test.VisualUtilities;

public class CoordinateTests
{
    [ Theory ]
    [ InlineData( 0, 0, 100, 100, 10, 10, 60, 40 ) ]
    [ InlineData( 0, 0, 100, 100, 60, 70, 110, -20 ) ]
    [ InlineData( 10, -10, 100, 100, 10, 10, 60, 40 ) ]
    [ InlineData( -10, 10, 100, 100, 60, 70, 110, -20 ) ]
    public void ToCartesian(
        float xCartesianCenter,
        float yCartesianCenter,
        float width,
        float height,
        float xConvert,
        float yConvert,
        float xConverted,
        float yConverted
    )
    {
        var cartesianVector = new Vector2( xConvert, yConvert );
        var cartesianSystem = CoordinateSystem.GetCartesian( xCartesianCenter, yCartesianCenter );

        var windowsSystem = CoordinateSystem.GetWindows(xCartesianCenter, yCartesianCenter);
        windowsSystem.OffsetCartesian( -width / 2, height / 2 );

        var windowsVector = cartesianSystem.ChangeCoordinateSystem( windowsSystem, cartesianVector );

        windowsVector.X.Should().Be( xConverted );
        windowsVector.Y.Should().Be( yConverted );

        var reconverted = windowsSystem.ChangeCoordinateSystem( cartesianSystem, windowsVector );
        reconverted.X.Should().Be( cartesianVector.X );
        reconverted.Y.Should().Be( cartesianVector.Y );
    }
}