using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.DataProtection;
using System.Text.Json.Serialization;

namespace J4JSoftware.WindowsUtilities;

public class AppConfigBase
{
    private enum EncryptablePropertyType
    {
        Unsupported,
        Simple,
        Array,
        List
    }

    private record EncryptedProperty( PropertyInfo PropertyInfo, EncryptablePropertyType Type );

    public static string UserFolder { get; } = Windows.Storage.ApplicationData.Current.LocalFolder.Path;

    private readonly Type _configType;
    private readonly List<EncryptedProperty> _encryptedProps;

    protected AppConfigBase()
    {
        _configType = GetType();

        _encryptedProps = _configType
                         .GetProperties()
                         .Where( x => x.CanWrite )
                         .Select( x => new EncryptedProperty( x, GetSupportedType( x ) ) )
                         .ToList();
    }

    private EncryptablePropertyType GetSupportedType( PropertyInfo propInfo )
    {
        if( propInfo.GetCustomAttribute<EncryptedPropertyAttribute>() == null)
            return EncryptablePropertyType.Unsupported;

        var propType = propInfo.PropertyType;

        if( propType == typeof( string ) )
            return EncryptablePropertyType.Simple;

        if( propType == typeof( string[] ) )
            return EncryptablePropertyType.Array;

        return propType.IsAssignableTo( typeof( IEnumerable<string> ) )
            ? EncryptablePropertyType.List
            : EncryptablePropertyType.Unsupported;
    }

    [ JsonIgnore ]
    public string? UserConfigurationFilePath { get; set; }

    [ JsonIgnore ]
    public bool UserConfigurationFileExists =>
        !string.IsNullOrEmpty( UserConfigurationFilePath )
     && File.Exists( Path.Combine( UserFolder, UserConfigurationFilePath ) );

    public AppConfigBase Encrypt( IDataProtector protector )
    {
        var retVal = CreateInstance();

        foreach( var encProp in _encryptedProps )
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch( encProp.Type )
            {
                case EncryptablePropertyType.Simple:
                    EncryptProperty( retVal, encProp, protector );
                    break;

                case EncryptablePropertyType.Array:
                    EncryptArray( retVal, encProp, protector );
                    break;

                case EncryptablePropertyType.List:
                    EncryptList( retVal, encProp, protector );
                    break;

                default:
                    encProp.PropertyInfo.SetValue( retVal, encProp.PropertyInfo.GetValue( this ) );
                    break;
            }
        }

        return retVal;
    }

    private AppConfigBase CreateInstance()
    {
        AppConfigBase retVal;

        try
        {
            retVal = (AppConfigBase) Activator.CreateInstance( _configType )!;
        }
        catch( Exception ex )
        {
            throw new TypeInitializationException( _configType.FullName,
                                                   new ApplicationException(
                                                       $"Could not create an instance of {_configType}. Are you sure it has a public parameterless constructor?",
                                                       ex ) );
        }

        return retVal;
    }

    private void EncryptProperty( AppConfigBase encrypted, EncryptedProperty propInfo, IDataProtector protector )
    {
        var plainText = (string?) propInfo.PropertyInfo.GetValue( this );
        if( string.IsNullOrEmpty( plainText ) )
            return;

        propInfo.PropertyInfo.SetValue( encrypted, protector.Protect( plainText ) );
    }

    private void EncryptArray( AppConfigBase encrypted, EncryptedProperty propInfo, IDataProtector protector )
    {
        var plainTextValues = (string?[]?) propInfo.PropertyInfo.GetValue( this );
        if( plainTextValues == null )
            return;

        var encryptedValues = plainTextValues.Select( x=> x == null ? null : protector.Protect(x) ).ToArray();
        propInfo.PropertyInfo.SetValue( encrypted, encryptedValues );
    }

    private void EncryptList( AppConfigBase encrypted, EncryptedProperty propInfo, IDataProtector protector )
    {
        var plainTextValues = (IEnumerable<string?>?) propInfo.PropertyInfo.GetValue( this );
        if( plainTextValues == null )
            return;

        var encryptedValues = plainTextValues.Select(x => x == null ? null : protector.Protect(x)).ToList();
        propInfo.PropertyInfo.SetValue( encrypted, encryptedValues );
    }

    public AppConfigBase Decrypt( IDataProtector protector )
    {
        var retVal = CreateInstance();

        foreach( var encProp in _encryptedProps )
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch( encProp.Type )
            {
                case EncryptablePropertyType.Simple:
                    DecryptProperty( retVal, encProp, protector );
                    break;

                case EncryptablePropertyType.Array:
                    DecryptArray( retVal, encProp, protector );
                    break;

                case EncryptablePropertyType.List:
                    DecryptList( retVal, encProp, protector );
                    break;

                default:
                    encProp.PropertyInfo.SetValue(retVal, encProp.PropertyInfo.GetValue(this));
                    break;
            }
        }

        return retVal;
    }

    private void DecryptProperty( AppConfigBase encrypted, EncryptedProperty propInfo, IDataProtector protector )
    {
        var encryptedText = (string?) propInfo.PropertyInfo.GetValue( this );
        if( string.IsNullOrEmpty( encryptedText ) )
            return;

        propInfo.PropertyInfo.SetValue( encrypted, protector.Unprotect( encryptedText ) );
    }

    private void DecryptArray( AppConfigBase decrypted, EncryptedProperty propInfo, IDataProtector protector )
    {
        var encryptedValues = (string?[]?) propInfo.PropertyInfo.GetValue( this );
        if( encryptedValues == null )
            return;

        var decryptedValues = encryptedValues.Select( x=> x == null ? null :protector.Unprotect(x) ).ToArray();
        propInfo.PropertyInfo.SetValue( decrypted, decryptedValues );
    }

    private void DecryptList( AppConfigBase decrypted, EncryptedProperty propInfo, IDataProtector protector )
    {
        var encryptedValues = (IEnumerable<string?>?) propInfo.PropertyInfo.GetValue( this );
        if( encryptedValues == null )
            return;

        var decryptedValues = encryptedValues.Select(x => x == null ? null : protector.Unprotect(x)).ToList();
        propInfo.PropertyInfo.SetValue( decrypted, decryptedValues );
    }
}
