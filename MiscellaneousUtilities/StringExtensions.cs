#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// StringExtensions.cs
//
// This file is part of JumpForJoy Software's MiscellaneousUtilities.
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
#endregion

using System.Linq;
using System.Security.Cryptography;
using System.Text;

// ReSharper disable ExplicitCallerInfoArgument

namespace J4JSoftware.Utilities;

public static class StringExtensions
{
    public record HashContext( bool CaseInsensitive = true, bool IgnoreWhiteSpace = true );

    public static string CalculateHash( this HashContext context, params string?[] toHash ) =>
        CalculateHash( context.CaseInsensitive, context.IgnoreWhiteSpace, toHash );

    // thanx to alexd for this! (https://stackoverflow.com/users/3246555/alexd)
    // https://stackoverflow.com/questions/38043954/generate-unique-hash-code-based-on-string
    public static string CalculateHash(
        bool caseInsensitive,
        bool ignoreWhiteSpace,
        params string?[] toHash
    )
    {
        var sb = new StringBuilder();

        foreach( var item in toHash.Where(x=>!string.IsNullOrEmpty(x)  ) )
        {
            sb.Append( ignoreWhiteSpace 
                           ? caseInsensitive 
                               ? item!.RemoveWhiteSpace().ToLower() 
                               : item!.RemoveWhiteSpace()
                           : caseInsensitive
                               ? item!.ToLower()
                               : item! );
        }

        var shaHash = SHA256.Create();

        var retVal = new StringBuilder();

        // Convert the input string to a byte array and compute the hash.
        byte[] data = shaHash.ComputeHash( Encoding.UTF8.GetBytes( sb.ToString() ) );

        // Loop through each byte of the hashed data and format each one as a hexadecimal string.
        foreach( var curChar in data )
        {
            retVal.Append( curChar.ToString( "x2" ) );
        }

        return retVal.ToString();
    }

    // thanx to Henk Meulekamp for this
    // https://stackoverflow.com/questions/6219454/efficient-way-to-remove-all-whitespace-from-string
    public static string RemoveWhiteSpace( this string text ) =>
        new string( text.Where( c => !char.IsWhiteSpace( c ) )
                        .ToArray() );
}