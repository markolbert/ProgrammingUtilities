using System.Collections.Generic;
using System.Numerics;
using J4JSoftware.VisualUtilities;

namespace Test.VisualUtilities;

public class Rectangle2DTestData
{
    public record RectParams( float Height, float Width, float Rotation, Vector3 Center );

    public record RectTestParams(
            float Height,
            float Width,
            float Rotation,
            Vector3 Center,
            Vector3[] RectangleCorners
        )
        : RectParams( Height,
                      Width,
                      Rotation,
                      Center );

    public record InsideOutsideRectParams( RectParams Outer, RectParams Inner, RelativePosition Relationship );

    public static IEnumerable<object?[]> GetTestRectangles()
    {
        yield return new object?[]
        {
            new RectTestParams( 512,
                            512,
                            0,
                            new Vector3( 0, 0, 0 ),
                            new[]
                            {
                                new Vector3( -256, -256, 0 ),
                                new Vector3( -256, 256, 0 ),
                                new Vector3( 256, 256, 0 ),
                                new Vector3( 256, -256, 0 ),
                            } )
        };

        yield return new object?[]
        {
            new RectTestParams( 512,
                            512,
                            315,
                            new Vector3( 0, 0, 0 ),
                            new[]
                            {
                                new Vector3( -362.04F, 0, 0 ),
                                new Vector3( 0, 362.04F, 0 ),
                                new Vector3( 362.04F, 0, 0 ),
                                new Vector3( 0, -362.04F, 0 ),
                            } )
        };

        yield return new object?[]
        {
            new RectTestParams( 300,
                            750,
                            151,
                            new Vector3( 73, -129, 0 ),
                            new[]
                            {
                                new Vector3( -327.704F, -78.389F, 0 ),
                                new Vector3( -182.261F, 183.997F, 0 ),
                                new Vector3( 473.704F, -179.611F, 0 ),
                                new Vector3( 328.261F, -441.997F, 0 ),
                            } )
        };
    }

    public static IEnumerable<object?[]> GetInsideOutside()
    {
        yield return new object?[]
        {
            new InsideOutsideRectParams( new RectTestParams( 512,
                                                         512,
                                                         0,
                                                         new Vector3( 0, 0, 0 ),
                                                         new[]
                                                         {
                                                             new Vector3( -256, 256, 0 ),
                                                             new Vector3( 256, 256, 0 ),
                                                             new Vector3( 256, -256, 0 ),
                                                             new Vector3( -256, -256, 0 )
                                                         } ),
                                     new RectTestParams( 512,
                                                         512,
                                                         0,
                                                         new Vector3( 0, 0, 0 ),
                                                         new[]
                                                         {
                                                             new Vector3( -128, 128, 0 ),
                                                             new Vector3( 128, 128, 0 ),
                                                             new Vector3( 128, -128, 0 ),
                                                             new Vector3( -128, -128, 0 )
                                                         } ),
                                     RelativePosition.Edge )
        };

        yield return new object?[]
        {
            new InsideOutsideRectParams( new RectTestParams( 512,
                                                         512,
                                                         0,
                                                         new Vector3( 0, 0, 0 ),
                                                         new[]
                                                         {
                                                             new Vector3( -256, 256, 0 ),
                                                             new Vector3( 256, 256, 0 ),
                                                             new Vector3( 256, -256, 0 ),
                                                             new Vector3( -256, -256, 0 )
                                                         } ),
                                     new RectTestParams( 256,
                                                         256,
                                                         0,
                                                         new Vector3( 0, 0, 0 ),
                                                         new[]
                                                         {
                                                             new Vector3( -256, 256, 0 ),
                                                             new Vector3( 256, 256, 0 ),
                                                             new Vector3( 256, -256, 0 ),
                                                             new Vector3( -256, -256, 0 )
                                                         } ),
                                     RelativePosition.Inside )
        };

        yield return new object?[]
        {
            new InsideOutsideRectParams( new RectTestParams( 512,
                                                         512,
                                                         0,
                                                         new Vector3( 0, 0, 0 ),
                                                         new[]
                                                         {
                                                             new Vector3( -256, 256, 0 ),
                                                             new Vector3( 256, 256, 0 ),
                                                             new Vector3( 256, -256, 0 ),
                                                             new Vector3( -256, -256, 0 )
                                                         } ),
                                     new RectTestParams( 256,
                                                         256,
                                                         0,
                                                         new Vector3( 512, 512, 0 ),
                                                         new[]
                                                         {
                                                             new Vector3( -128, 128, 0 ),
                                                             new Vector3( 128, 128, 0 ),
                                                             new Vector3( 128, -128, 0 ),
                                                             new Vector3( -128, -128, 0 )
                                                         } ),
                                     RelativePosition.Outside )
        };

    }
}
