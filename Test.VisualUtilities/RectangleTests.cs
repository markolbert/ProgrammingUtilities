using System;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using J4JSoftware.VisualUtilities;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
}
