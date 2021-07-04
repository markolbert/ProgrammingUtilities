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
    public class MonthNumberMinorTickEnumerator : IMinorTickEnumerator
    {
        private readonly List<MinorTick> _smallMinorTicks = new List<MinorTick>
        {
            new MinorTick(1,12),
            new MinorTick(3,4),
            new MinorTick(6,2),
            new MinorTick(12,1),
        };

        private readonly List<MinorTick> _largeMinorTicks;

        public MonthNumberMinorTickEnumerator(
            IEnumerable<MinorTick>? largeMultiples = null
        )
        {
            _largeMinorTicks = largeMultiples?.ToList() ?? new List<MinorTick>
            {
                new MinorTick(12, 10),
                new MinorTick(24, 5),
                new MinorTick(60, 2)
            };

            Default = _largeMinorTicks[ 0 ];
        }

        public MinorTick Default { get; }

        public IEnumerable<ScaledMinorTick> GetEnumerator( double minValue, double maxValue)
        {
            maxValue = maxValue <= 12 ? 12 : maxValue;
            minValue = minValue <= 12 ? 12 : minValue;

            var years = (int) Math.Ceiling( (maxValue - minValue) / 12 );
            var maxExponent = (int) Math.Ceiling( Math.Log10( years ) );
            var multiplier = 1;

            for (var exponent = 0; exponent <= maxExponent; exponent++)
            {
                switch( exponent )
                {
                    case 0:
                        foreach( var baseMultiple in _smallMinorTicks )
                        {
                            yield return new ScaledMinorTick( baseMultiple.NormalizedSize, 
                                0,
                                baseMultiple.NumberPerMajor );
                        }

                        break;

                    default:
                        foreach( var largeMultiple in _largeMinorTicks )
                        {
                            var curValue = new ScaledMinorTick( 
                                largeMultiple.NormalizedSize,
                                exponent,
                                largeMultiple.NumberPerMajor );

                            yield return curValue;

                            if( curValue.NormalizedSize * multiplier > years * 12 )
                                yield break;
                        }

                        break;
                }

                multiplier *= 10;
            }
        }
    }
}