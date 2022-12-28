// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of VisualUtilities.
//
// VisualUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// VisualUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with VisualUtilities. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Globalization;

namespace J4JSoftware.VisualUtilities;

public static class WebColors
{
    public static int ToAbgr( this Color color, byte transparency = 0xFF )
    {
        var bytes = new byte[ 4 ];
        bytes[ 0 ] = color.R;
        bytes[ 1 ] = color.G;
        bytes[ 2 ] = color.B;
        bytes[ 3 ] = transparency;

        return BitConverter.ToInt32( bytes );
    }

    public static string ToAbgrHex( this Color color, byte transparency = 0xFF )
    {
        return color.ToAbgr( transparency ).ToString( "x" );
    }

    public static int HexTextToAbgr( string text )
    {
        if( int.TryParse( text, NumberStyles.HexNumber, null, out var temp ) )
            return temp;

        return -1;
    }

    public static Color ToColor( int code )
    {
        var bytes = BitConverter.GetBytes( code );

        return bytes.Length != 4
            ? Color.White
            : Color.FromArgb( bytes[ 3 ], bytes[ 0 ], bytes[ 1 ], bytes[ 2 ] );
    }

    public static Color FromAbgrHex( string text )
    {
        var bytes = BitConverter.GetBytes( HexTextToAbgr( text ) );

        return bytes.Length != 4
            ? Color.White
            : Color.FromArgb( bytes[ 3 ], bytes[ 0 ], bytes[ 1 ], bytes[ 2 ] );
    }
}