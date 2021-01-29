using System;
using System.Text;
using Microsoft.AspNetCore.DataProtection;

namespace J4JSoftware.DependencyInjection
{
    public class J4JProtection : IJ4JProtection
    {
        private readonly IDataProtector _protector;

        public J4JProtection(
            IDataProtectionProvider provider,
            string purpose )
        {
            _protector = provider.CreateProtector( purpose );
        }

        public bool Protect( string plainText, out string? encrypted )
        {
            encrypted = null;

            var utf8 = new UTF8Encoding();
            var bytesToEncrypt = utf8.GetBytes(plainText);

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
}