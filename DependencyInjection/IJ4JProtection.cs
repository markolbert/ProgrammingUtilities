namespace J4JSoftware.DependencyInjection
{
    public interface IJ4JProtection
    {
        bool Protect( string plainText, out string? encrypted );
        bool Unprotect( string encryptedText, out string? decrypted );
    }
}