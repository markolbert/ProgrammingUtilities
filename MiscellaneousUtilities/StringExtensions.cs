using System;
using System.Linq;
using System.Runtime.CompilerServices;
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