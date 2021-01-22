using System;
using System.Drawing;
using System.Linq;
using FluentAssertions;
using J4JSoftware.VisualUtilities;
using Xunit;

namespace Test.VisualUtilities
{
    public class ColorConversionTest
    {
        private readonly ILookup<int, Color> _colorTable;

        public ColorConversionTest()
        {
            _colorTable = Enum.GetValues<KnownColor>()
                .Cast<KnownColor>()
                .Select(Color.FromKnownColor)
                .ToLookup(x => x.ToArgb());
        }

        [Theory]
        [ClassData(typeof(ColorSet))]
        public void RoundTrip(Color color)
        {
            var abgrHex = color.ToAbgrHex();
            var reconverted = WebColors.FromAbgrHex( abgrHex );

            var lookup = _colorTable[ reconverted.ToArgb() ].FirstOrDefault( x => x.A == reconverted.A
                && x.R == reconverted.R
                && x.G == reconverted.G
                && x.B == reconverted.B );

            lookup.Should().NotBeNull();
            lookup.Should().Be( color );
        }
    }
}
