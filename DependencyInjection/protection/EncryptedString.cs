using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using J4JSoftware.Logging;

namespace J4JSoftware.DependencyInjection
{
    public class EncryptedString
    {
        private string? _clearText;
        private string? _encryptedText;

        [JsonIgnore]
        public IJ4JLogger? Logger { get; set; }

        [JsonIgnore]
        public IJ4JProtection? Protector { get; set; }

        [JsonIgnore]
        public bool IsDefined => !string.IsNullOrEmpty( ClearText ) && !string.IsNullOrEmpty( EncryptedText );

        [JsonIgnore]
        public string? ClearText
        {
            get
            {
                if( !string.IsNullOrEmpty( _clearText ) )
                    return _clearText;

                if( string.IsNullOrEmpty( _encryptedText ) )
                    return null;

                if( Protector == null )
                {
                    Logger?.Error("IJ4JProtector not defined");
                    return null;
                }

                if( Protector.Unprotect(_encryptedText, out var temp))
                    Logger?.Error("Could not decrypt text");

                _clearText = temp;

                return _clearText;
            }

            set
            {
                _clearText = value;

                if( !string.IsNullOrEmpty( _encryptedText ) )
                    _encryptedText = null;
            }
        }

        public string? EncryptedText
        {
            get
            {
                if( !string.IsNullOrEmpty( _encryptedText ) )
                    return _encryptedText;

                if( string.IsNullOrEmpty( _clearText ) )
                    return null;

                if (Protector == null)
                {
                    Logger?.Error("IJ4JProtector not defined");
                    return null;
                }

                if ( !Protector.Protect(_clearText, out var temp  ))
                    Logger?.Error("Could not encrypt text");

                _encryptedText = temp;

                return _encryptedText;
            }

            set
            {
                _encryptedText = value;

                if( !string.IsNullOrEmpty( _clearText ) )
                    _clearText = null;
            }
        }
    }
}
