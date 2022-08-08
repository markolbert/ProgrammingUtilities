#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'Test.VisualUtilities' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Drawing;
using System.Linq;
using FluentAssertions;
using J4JSoftware.VisualUtilities;
using Xunit;

namespace Test.VisualUtilities;

public class ColorConversionTest
{
    private readonly ILookup<int, Color> _colorTable;

    public ColorConversionTest()
    {
        _colorTable = Enum.GetValues<KnownColor>()
                          .Select( Color.FromKnownColor )
                          .ToLookup( x => x.ToArgb() );
    }

    [ Theory ]
    [ ClassData( typeof( ColorSet ) ) ]
    public void RoundTrip( Color color )
    {
        var abgrHex = color.ToAbgrHex();
        var reconverted = WebColors.FromAbgrHex( abgrHex );

        var lookup = _colorTable[ reconverted.ToArgb() ]
           .FirstOrDefault( x => x.A == reconverted.A
                             && x.R == reconverted.R
                             && x.G == reconverted.G
                             && x.B == reconverted.B );

        lookup.Should().NotBeNull();
        lookup.Should().Be( color );
    }
}