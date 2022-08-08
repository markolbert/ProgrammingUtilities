#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'DependencyInjection' is free software: you can redistribute it
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
using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace J4JSoftware.DependencyInjection;

public class J4JProtection : IJ4JProtection
{
    private readonly IDataProtector _protector;

    public J4JProtection( IDataProtectionProvider provider,
        string purpose )
    {
        _protector = provider.CreateProtector( purpose );
    }

    public bool Protect( string plainText, out string? encrypted )
    {
        encrypted = null;

        var utf8 = new UTF8Encoding();
        var bytesToEncrypt = utf8.GetBytes( plainText );

        try
        {
            var encryptedBytes = _protector.Protect( bytesToEncrypt );
            encrypted = Convert.ToBase64String( encryptedBytes );
        }
        catch
        {
            return false;
        }

        return true;
    }

    public bool Unprotect( string encryptedText, out string? decrypted )
    {
        decrypted = null;

        byte[] decryptedBytes;

        try
        {
            var encryptedBytes = Convert.FromBase64String( encryptedText );
            decryptedBytes = _protector.Unprotect( encryptedBytes );
        }
        catch
        {
            return false;
        }

        var utf8 = new UTF8Encoding();
        decrypted = utf8.GetString( decryptedBytes );

        return true;
    }
}