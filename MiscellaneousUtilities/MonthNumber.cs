// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of MiscellaneousUtilities.
//
// MiscellaneousUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// MiscellaneousUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with MiscellaneousUtilities. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace J4JSoftware.Utilities;

public static class MonthNumber
{
    public static int GetMonthNumber( DateTime dt ) => dt.Year * 12 + dt.Month - 1;

    public static int GetMonthNumber( string dateText )
    {
        if ( DateTime.TryParse( dateText, out var result ) )
            return GetMonthNumber( result );

        return GetMonthNumber( DateTime.MinValue );
    }

    public static DateTime DateFromMonthNumber( int monthNumber )
    {
        var year = monthNumber / 12;
        var month = monthNumber - 12 * year + 1;

        return new DateTime( year, month, 1 );
    }

    public static DateTime DateFromMonthNumber( double monthNumber )
    {
        var year = (int) monthNumber / 12;
        var month = (int) monthNumber - 12 * year + 1;
        var day = (int) Math.Round( monthNumber - 12 * year - month + 1 );

        day = day <= 0 ? 1 : day;
        var daysInMonth = DateTime.DaysInMonth( year, month );
        day = day > daysInMonth ? daysInMonth : day;

        return new DateTime( year, month, (int) day );
    }
}