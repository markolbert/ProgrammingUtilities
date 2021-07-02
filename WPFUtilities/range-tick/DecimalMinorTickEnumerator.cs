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
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.WPFUtilities
{
    public record MinorTickInfo( double Size, int NumberPerMajor );

    public class DecimalMinorTickEnumerator : IMinorTickEnumerator
    {
        private readonly List<MinorTickInfo> _minorTicks;

        public DecimalMinorTickEnumerator(
            IEnumerable<MinorTickInfo>? baseMultiples = null
        )
        {
            _minorTicks = baseMultiples?.ToList() ?? new List<MinorTickInfo>
            {
                new MinorTickInfo(1,10),
                new MinorTickInfo(2,5),
                new MinorTickInfo(2.5,4),
                new MinorTickInfo(5,2),
            };

            Default = _minorTicks[ 0 ];
        }

        public MinorTickInfo Default { get; }

        public IEnumerable<MinorTickInfo> GetEnumerator( double minValue, double maxValue )
        {
            var maxExponent = maxValue != minValue
                ? (int) Math.Ceiling( Math.Log10( Math.Abs( maxValue - minValue ) ) )
                : 0;

            var sign = Math.Sign( maxExponent );

            for( var exponent = 0; exponent <= Math.Abs( maxExponent ); exponent++ )
            {
                var multiplier = Math.Pow( 10, sign * exponent );

                foreach( var minorTick in _minorTicks )
                {
                    yield return new MinorTickInfo( minorTick.Size * multiplier, minorTick.NumberPerMajor );
                }
            }
        }
    }
}