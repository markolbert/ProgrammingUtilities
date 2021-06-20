#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'WPFUtilities' is free software: you can redistribute it
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

namespace J4JSoftware.WPFUtilities
{
    public class MinorTickInfo
    {
        public static MinorTickInfo[] Default = new[]
        {
            new MinorTickInfo( 1, 10 ),
            new MinorTickInfo( 2, 5 ),
            new MinorTickInfo( 5, 2 ),
            new MinorTickInfo( 25, 4 )
        };

        public MinorTickInfo( int normalizedWidth, int minorPerMajor )
        {
            NormalizedTickWidth = Validate( normalizedWidth, nameof(normalizedWidth) );
            MinorTicksPerMajorTick = Validate( minorPerMajor, nameof(minorPerMajor) );
        }

        private int Validate( int toCheck, string paramName )
        {
            switch (toCheck)
            {
                case 0:
                    throw new ArgumentException($"{paramName} cannot be zero");

                case < 0:
                    return -toCheck;

                default:
                    return toCheck;
            }
        }

        public int NormalizedTickWidth { get; }
        public int MinorTicksPerMajorTick { get; }
    }
}