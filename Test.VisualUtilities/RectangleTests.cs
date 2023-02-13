using System.Numerics;
using FluentAssertions;
using J4JSoftware.VisualUtilities;
using Xunit;

namespace Test.VisualUtilities;

public class RectangleTests
{
    [Theory]
    [MemberData(nameof(Rectangle2DTestData.GetTestRectangles), MemberType = typeof(Rectangle2DTestData))]
    public void Corners( Rectangle2DTestData.RectTestParams data )
    {
        var rectangle = new Rectangle2D( data.Height, data.Width, data.Rotation, data.Center );

        for( var idx = 0; idx < 4; idx++ )
        {
            CheckPointEquivalence(rectangle[idx], data.RectangleCorners[idx], 1E-2F);
        }
    }

    [Theory]
    [MemberData(nameof(Rectangle2DTestData.GetInsideOutside), MemberType = typeof(Rectangle2DTestData))]
    public void InsideOutsideRectangle( Rectangle2DTestData.InsideOutsideRectParams data )
    {
        var outer = new Rectangle2D(data.Outer.Height, data.Outer.Width, data.Outer.Rotation, data.Outer.Center);
        var inner = new Rectangle2D(data.Inner.Height, data.Inner.Width, data.Inner.Rotation, data.Inner.Center);

        outer.Contains( inner ).Should().Be( data.Relationship );
    }

    private void CheckPointEquivalence( Vector3 point1, Vector3 point2, float precision )
    {
        point1.X.Should().BeInRange(point2.X - precision, point2.X + precision);
        point1.Y.Should().BeInRange(point2.Y - precision, point2.Y + precision);
    }

    [ Theory ]
    [ InlineData( 100, 200, 0, 0, 0, 5000, 5000 ) ]
    [ InlineData( 100, 200, 45, 0, 0, 5000, 5000 ) ]
    [InlineData(100, 200, 0, 50, 50, 5000, 5000)]
    [InlineData(100, 200, 45, 50, 50, 5000, 5000)]
    [InlineData(100, 200, 0, -50, -50, 5000, 5000)]
    [InlineData(100, 200, 45, -50, -50, 5000, 5000)]
    [InlineData(100, 200, 0, -50, 50, 5000, 5000)]
    [InlineData(100, 200, 45, -50, 50, 5000, 5000)]
    [InlineData(100, 200, 0, 50, -50, 5000, 5000)]
    [InlineData(100, 200, 45, 50, -50, 5000, 5000)]
    public void DisplaySpace(
        float height,
        float width,
        float rotation,
        float centerX,
        float centerY,
        float displayHeight,
        float displayWidth
    )
    {
        var initialRect = new Rectangle2D( height, width, rotation, new Vector3( centerX, centerY, 0 ) );
        var viewport = new Vector3( displayHeight, displayWidth, 0 );
        var displayRect = initialRect.ToDisplaySpace( viewport );

        var reversedRect = displayRect.FromDisplaySpace( viewport );

        for( var idx = 0; idx < 4; idx++ )
        {
            reversedRect[ idx ]
               .X.Should()
               .BeApproximately( initialRect[ idx ].X, 0.001F );

            reversedRect[idx]
               .Y.Should()
               .BeApproximately(initialRect[idx].Y, 0.001F);
        }
    }
}
