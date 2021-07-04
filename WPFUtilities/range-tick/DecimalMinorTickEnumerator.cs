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
    public class DecimalMinorTickEnumerator : IMinorTickEnumerator
    {
        private class ExponentRange
        {
            public static int GetNormalizedValue(int powerOfTen)
            {
                var retVal = 1;

                for (var curExp = 0; curExp < (powerOfTen < 0 ? -powerOfTen : powerOfTen); curExp++)
                {
                    retVal *= 10;
                }

                return retVal;
            }

            public ExponentRange(double minValue, double maxValue)
            {
                var minExp = minValue == 0 ? 0 : (int)Math.Floor(Math.Log10(Math.Abs(minValue)));
                var maxExp = maxValue == 0 ? 0 : (int)Math.Floor(Math.Log10(Math.Abs(maxValue)));

                if (minExp > maxExp)
                {
                    MinimumExponent = maxExp;
                    MaximumExponent = minExp;
                }
                else
                {
                    MinimumExponent = minExp;
                    MaximumExponent = maxExp;
                }
            }

            public int MinimumExponent { get; }
            public int MaximumExponent { get; }
        }

        private readonly List<MinorTick> _minorTicks;

        public DecimalMinorTickEnumerator(
            IEnumerable<MinorTick>? baseMultiples = null
        )
        {
            _minorTicks = baseMultiples?.ToList() ?? new List<MinorTick>
            {
                new MinorTick(1,10),
                new MinorTick(2,5),
                new MinorTick(5,2),
                new MinorTick(25,4),
            };

            Default = _minorTicks[ 0 ];
        }

        public MinorTick Default { get; }

        public IEnumerable<ScaledMinorTick> GetEnumerator( double minValue, double maxValue )
        {
            var expRange = new ExponentRange( minValue, maxValue );

            maxValue = Math.Abs( minValue > maxValue ? minValue : maxValue );
            
            for( var exponent = expRange.MinimumExponent - 1; exponent < expRange.MaximumExponent; exponent++ )
            {
                foreach( var minorTick in _minorTicks )
                {
                    var minorTickSize = exponent < 0
                        ? minorTick.NormalizedSize / (double) ExponentRange.GetNormalizedValue( exponent )
                        : minorTick.NormalizedSize * (double) ExponentRange.GetNormalizedValue( exponent );

                    if( minorTickSize < maxValue )
                        yield return new ScaledMinorTick( minorTick, exponent );
                }
            }
        }
    }
}