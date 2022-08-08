using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using J4JSoftware.Logging;

namespace J4JSoftware.DependencyInjection;

public class EncryptedString : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

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

            if( !Protector.Unprotect(_encryptedText, out var temp))
                Logger?.Error("Could not decrypt text");

            SetProperty( ref _clearText, temp );

            return _clearText;
        }

        set
        {
            SetProperty( ref _clearText, value );

            if( !string.IsNullOrEmpty( _encryptedText ) )
                SetProperty( ref _encryptedText, null );
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

            SetProperty( ref _encryptedText, temp );

            return _encryptedText;
        }

        set
        {
            SetProperty( ref _encryptedText, value );

            if( !string.IsNullOrEmpty( _clearText ) )
                SetProperty( ref _clearText, null );
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        field = value;
        OnPropertyChanged(propertyName);
    }
}