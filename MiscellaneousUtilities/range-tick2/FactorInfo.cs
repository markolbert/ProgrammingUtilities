#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'MiscellaneousUtilities' is free software: you can redistribute it
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
using System.Runtime.Versioning;

namespace J4JSoftware.Utilities;

[RequiresPreviewFeatures("Experimental, subject to change or removal")]
public class FactorInfo
{
    public static List<FactorInfo> GetFactors( int number )
    {
        if( number < 0 )
            number = -number;

        return number switch
        {
            0 => new List<FactorInfo>(),
            1 => new List<FactorInfo>() { new FactorInfo( 1 ) },
            _ => GetFactorsInternal( number )
        };
    }

    private static List<FactorInfo> GetFactorsInternal( int number )
    {
        var retVal = new List<FactorInfo>() { new FactorInfo( 1 ) };

        while( number > 1 )
        {
            var factor = GetLargestFactor( number );

            var factorInfo = retVal.FirstOrDefault( x => x.Factor == factor );
            if( factorInfo == null )
                retVal.Add( new FactorInfo( factor ) );
            else factorInfo.Frequency++;

            if( factor == 1 )
                break;

            number /= factor;
        }

        return retVal.OrderBy( x => x.Factor ).ToList();
    }

    private static int GetLargestFactor( int value )
    {
        var maxFactor = Convert.ToInt32( Math.Ceiling( Math.Sqrt( Convert.ToDouble( value ) ) ) );

        for ( var factor = maxFactor; factor > 1; factor-- )
        {
            if ( value % factor == 0 )
                return factor;
        }

        return value;
    }

    private int _frequency;

    public FactorInfo( int factor,
        int frequency = 1 )
    {
        if( factor <= 0 )
            throw new ArgumentException( $"{nameof( factor )} cannot be <= 0" );

        if ( frequency <= 0 )
            throw new ArgumentException( $"{nameof( frequency )} cannot be <= 0" );

        Factor = factor;
        _frequency = frequency;
    }

    public int Factor { get; }

    public int Frequency
    {
        get => _frequency;

        set
        {
            if( value == 0 )
                throw new ArgumentException( $"{nameof( value )} cannot be <= 0" );

            _frequency = value;
        }
    }
}