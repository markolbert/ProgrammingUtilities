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
#pragma warning disable 8618

namespace J4JSoftware.WPFUtilities
{
    public abstract class RangeTicks<T> : IRangeTicks<T>
        where T : ScaledTick, new()
    {
        protected RangeTicks()
        {
        }

        protected List<Tick> NormalizedRangeTicks { get; init; }

        public Tick Default { get; init; }

        public virtual IEnumerable<T> GetEnumerator( double minValue, double maxValue )
        {
            var expRange = new ExponentRange( minValue, maxValue );

            maxValue = Math.Abs( minValue > maxValue ? minValue : maxValue );
            
            for( var exponent = expRange.MinimumExponent; exponent <= expRange.MaximumExponent; exponent++ )
            {
                foreach( var minorTick in NormalizedRangeTicks )
                {
                    var minorTickSize = exponent < 0
                        ? minorTick.NormalizedSize / (double) ExponentRange.GetNormalizedValue( exponent )
                        : minorTick.NormalizedSize * (double) ExponentRange.GetNormalizedValue( exponent );

                    if( minorTickSize < maxValue )
                        yield return new T()
                        {
                            NormalizedSize = minorTick.NormalizedSize,
                            NumberPerMajor = minorTick.NumberPerMajor,
                            PowerOfTen = exponent
                        };
                }
            }
        }

        public abstract RangeParameters<T> GetDefaultRange( double minValue, double maxValue );
    }
}