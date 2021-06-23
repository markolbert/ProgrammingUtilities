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

namespace J4JSoftware.WPFUtilities.rangetick
{
    public class IntegerRangeRounder : IRangeRounder<int>
    {
        private readonly decimal _minorTickWidth;

        public IntegerRangeRounder(
            RangeParameters<int> rangeParameters
        )
        {
            _minorTickWidth = rangeParameters.MinorTickWidth;
        }

        public int RoundUp(int toRound)
        {
            var modulo = toRound % _minorTickWidth;
            if (modulo == 0)
                return toRound;

            var intModulo = 0;
            var intMinor = 0;

            try
            {
                intModulo = Convert.ToInt32( modulo );
                intMinor = Convert.ToInt32( _minorTickWidth );
            }
            catch
            {
                return toRound;
            }

            return toRound < 0 ? toRound - intModulo : toRound + intMinor - intModulo;
        }

        public int RoundDown(int toRound)
        {
            var modulo = toRound % _minorTickWidth;
            if (modulo == 0)
                return toRound;

            var intModulo = 0;
            var intMinor = 0;

            try
            {
                intModulo = Convert.ToInt32(modulo);
                intMinor = Convert.ToInt32(_minorTickWidth);
            }
            catch
            {
                return toRound;
            }

            return toRound < 0 ? toRound - intMinor - intModulo : toRound - intModulo;
        }
    }
}