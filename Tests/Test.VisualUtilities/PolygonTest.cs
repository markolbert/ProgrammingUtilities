using System;
using System.Numerics;
using FluentAssertions;
using J4JSoftware.VisualUtilities;
using Xunit;

namespace Test.VisualUtilities;

public class PolygonTest
{
    private Random _random = new Random( DateTime.Now.Millisecond );

    [Fact]
    public void BasicRectangles()
    {
        var rect1 = VectorPolygon.Create(new Vector2[]
        {
            new( 0, 0 ), new( 0, 100 ), new( 100, 100 ), new( 100, 0 )
        });

        var rect2 = VectorPolygon.Create(new Vector2[]
        {
            new( 50, 50 ), new( 50, 150 ), new( 150, 150 ), new( 150, 50 )
        });


        rect1.Should().NotBeNull();
        rect1!.Vertices.Count.Should().Be( 4 );

        rect2.Should().NotBeNull();
        rect2!.Vertices.Count.Should().Be(4);

        rect1.Intersects( rect2 ).Should().Be( true );
    }

    [ Theory ]
    [ InlineData( 0, 0, 100, 100, 0.0, 200, 200, 100, 100, 0.0, false ) ]
    [ InlineData( 0, 0, 100, 100, 30.0, 200, 200, 100, 100, 30.0, false ) ]
    [ InlineData( 0, 0, 100, 100, 0.0, 50, 50, 100, 100, 0.0, true ) ]
    [InlineData(0, 0, 100, 100, 0.0, 101, 101, 100, 100, 0.0, false)]
    [InlineData(0, 0, 100, 100, 0.0, 101, 101, 100, 100, 30.0, false)]
    [InlineData(0, 0, 100, 100, 0.0, 80, 80, 100, 121, 30.0, true)]
    [InlineData(0, 0, 100, 100, 30.0, 80, 80, 100, 121, 30.0, false)]
    public void Rectangles(
        float xOrigin1,
        float yOrigin1,
        float width1,
        float height1,
        double rotation1,
        float xOrigin2,
        float yOrigin2,
        float width2,
        float height2,
        double rotation2,
        bool intersects
    )
    {
        var rect1 = VectorPolygon.Create( new Vector2( xOrigin1, yOrigin1 ),
                                          new Vector2( xOrigin1, yOrigin1 + height1 ),
                                          new Vector2( xOrigin1 + width1, yOrigin1 + height1 ),
                                          new Vector2( xOrigin1 + width1, yOrigin1 ) );
        rect1.Should().NotBeNull();
        rect1 = rect1!.Rotate( rotation1 );

        var rect2 = VectorPolygon.Create( new Vector2( xOrigin2, yOrigin2 ),
                                          new Vector2( xOrigin2, yOrigin2 + height2 ),
                                          new Vector2( xOrigin2 + width2, yOrigin2 + height2 ),
                                          new Vector2( xOrigin2 + width2, yOrigin2 ) );
        rect2.Should().NotBeNull();
        rect2 = rect2!.Rotate( rotation2 );

        rect1.Intersects( rect2 ).Should().Be( intersects );
    }

    [Theory]
    [InlineData(80,80,100,121,50,50,30,60.98076,90.98076,0.001)]
    [InlineData(80, 80, 100, 121, 300, 300, 30, 219.4744, -0.52559, 0.001)]
    public void OffCenterRotations(
        float xOriginal,
        float yOriginal,
        float width,
        float height,
        float xCenter,
        float yCenter,
        double rotation,
        float xTransformed,
        float yTransformed,
        float tolerance
    )
    {
        var original = VectorPolygon.Create(new Vector2(xOriginal, yOriginal),
                                            new Vector2(xOriginal, yOriginal + height),
                                            new Vector2(xOriginal + width, yOriginal + height),
                                            new Vector2(xOriginal + width, yOriginal));
        original.Should().NotBeNull();

        var center = new Vector2( xCenter, yCenter );
        var transformed = original!.Rotate(rotation, center);

        var heightError = GetDistanceError( height, transformed );
        heightError.Should().BeLessThan( tolerance );

        var widthError = GetDistanceError( width, transformed );
        widthError.Should().BeLessThan( tolerance );

        var xError = Math.Abs( transformed.Vertices[ 0 ].X - xTransformed );
        xError.Should().BeLessThan( tolerance );
            
        var yError = Math.Abs( transformed.Vertices[ 0 ].Y - yTransformed );
        yError.Should().BeLessThan(tolerance);
    }

    private float GetDistanceError( float compareTo, VectorPolygon polygon )
    {
        var distance1 = Vector2.Distance( polygon.Vertices[ 0 ], polygon.Vertices[ 1 ] );
        var distance2 = Vector2.Distance( polygon.Vertices[ 0 ], polygon.Vertices[ 3 ] );

        var retVal1 = Math.Abs( compareTo - distance1 );
        var retVal2 = Math.Abs( compareTo - distance2 );

        return retVal2 < retVal1 ? retVal2 : retVal1;
    }

    [Theory]
    [InlineData(0, 0, 100, 100, 30.0, 20, 20, 60, 60, 30.0, true)]
    [InlineData(0, 0, 100, 100, 30.0, 80, 80, 100, 100, 30.0, false)]
    public void InsideRectangles(
        float xOrigin1,
        float yOrigin1,
        float width1,
        float height1,
        double rotation1,
        float xOrigin2,
        float yOrigin2,
        float width2,
        float height2,
        double rotation2,
        bool inside
    )
    {
        var rect1 = VectorPolygon.Create(new Vector2(xOrigin1, yOrigin1),
                                         new Vector2(xOrigin1, yOrigin1 + height1),
                                         new Vector2(xOrigin1 + width1, yOrigin1 + height1),
                                         new Vector2(xOrigin1 + width1, yOrigin1));
        rect1.Should().NotBeNull();
        rect1 = rect1!.Rotate(rotation1);

        var rect2 = VectorPolygon.Create(new Vector2(xOrigin2, yOrigin2),
                                         new Vector2(xOrigin2, yOrigin2 + height2),
                                         new Vector2(xOrigin2 + width2, yOrigin2 + height2),
                                         new Vector2(xOrigin2 + width2, yOrigin2));
        rect2.Should().NotBeNull();
        rect2 = rect2!.Rotate(rotation2);

        rect1.Inside(rect2).Should().Be(inside);
    }

}