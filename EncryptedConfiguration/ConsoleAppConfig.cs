#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// ConsoleAppConfig.cs
//
// This file is part of JumpForJoy Software's EncryptedConfiguration.
// 
// EncryptedConfiguration is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// EncryptedConfiguration is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with EncryptedConfiguration. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.EncryptedConfiguration;

public class ConsoleAppConfig : ObservableObject
{
    private static Type[] EncryptableTypes { get; } =
        new[] { typeof( string ), typeof( string[] ), typeof( IEnumerable<string> ) };

    public static string UserFolder { get; } = Environment.CurrentDirectory;

    private readonly List<List<PropertyInfo>> _propPaths;
    private readonly ILogger? _logger;

    protected ConsoleAppConfig(
        ILoggerFactory? loggerFactory = null
        )
    {
        _logger = loggerFactory?.CreateLogger( GetType() );
        _propPaths = GetEncryptableProperties( GetType() ).ToList();
    }

    private IEnumerable<List<PropertyInfo>> GetEncryptableProperties( Type type, List<PropertyInfo>? pathToType = null )
    {
        pathToType ??= new List<PropertyInfo>();

        foreach( var propInfo in type.GetProperties().Where( x => x.CanWrite ) )
        {
            // strings are classes so we have to use a two part filter
            // plus we only deal with property types that have public parameterless 
            // constructors so we can create them as needed
            if( propInfo.PropertyType.IsClass
            && EncryptableTypes.All( x => x != propInfo.PropertyType )
            && propInfo.PropertyType.GetConstructors().Any( x => x.GetParameters().Length == 0 ) )
            {
                var pathToChildType = new List<PropertyInfo>( pathToType ) { propInfo };

                foreach( var child in GetEncryptableProperties( propInfo.PropertyType, pathToChildType ) )
                {
                    yield return child;
                }
            }

            if( !propInfo.IsDefined( typeof( EncryptedPropertyAttribute ) )
            || EncryptableTypes.All( x => x != propInfo.PropertyType ) )
                continue;

            var retVal = new List<PropertyInfo>( pathToType ) { propInfo };
            yield return retVal;
        }
    }

    [ JsonIgnore ]
    public string? UserConfigurationFilePath { get; set; }

    [ JsonIgnore ]
    public bool UserConfigurationFileExists =>
        !string.IsNullOrEmpty( UserConfigurationFilePath )
     && File.Exists( Path.Combine( UserFolder, UserConfigurationFilePath ) );

    public void Encrypt( IDataProtector protector )
    {
        foreach( var propPath in _propPaths )
        {
            // create the parent properties as needed
            object parentProp = this;

            foreach( var childPropInfo in propPath.Take( propPath.Count - 1 ) )
            {
                var childPropValue = childPropInfo.GetValue( parentProp );
                childPropValue ??= Activator.CreateInstance( childPropInfo.PropertyType );

                parentProp = childPropValue!;
            }

            var valueToEncrypt = propPath.Last().GetValue( parentProp );
            valueToEncrypt ??= Activator.CreateInstance( propPath.Last().PropertyType )!;

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            object? encryptedValue = null;

            switch( valueToEncrypt )
            {
                case string stringValue:
                    encryptedValue = protector.Protect( stringValue );
                    break;

                case string?[] stringArray:
                    encryptedValue = stringArray.Select( x => x == null ? null : protector.Protect( x ) ).ToArray();
                    break;

                case IEnumerable<string?> stringEnumerable:
                    encryptedValue = stringEnumerable.Select( x => x == null ? null : protector.Protect( x ) ).ToList();
                    break;
            }

            if( encryptedValue == null )
                _logger?.LogError( "Could not encrypt property '{name}'", propPath.Last().Name );
            else propPath.Last().SetValue( parentProp, encryptedValue );
        }
    }

    public void Decrypt( IDataProtector protector )
    {
        foreach (var propPath in _propPaths)
        {
            // create the parent properties as needed
            object parentProp = this;

            foreach (var childPropInfo in propPath.Take(propPath.Count - 1))
            {
                var childPropValue = childPropInfo.GetValue(parentProp);
                childPropValue ??= Activator.CreateInstance(childPropInfo.PropertyType);

                parentProp = childPropValue!;
            }

            var valueToDecrypt = propPath.Last().GetValue(parentProp);
            valueToDecrypt ??= Activator.CreateInstance(propPath.Last().PropertyType)!;

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            object? decryptedValue = null;

            try
            {
                switch( valueToDecrypt )
                {
                    case string stringValue:
                        decryptedValue = protector.Unprotect( stringValue );
                        break;

                    case string?[] stringArray:
                        decryptedValue = stringArray.Select( x => x == null ? null : protector.Unprotect( x ) )
                                                    .ToArray();
                        break;

                    case IEnumerable<string?> stringEnumerable:
                        decryptedValue = stringEnumerable.Select( x => x == null ? null : protector.Unprotect( x ) )
                                                         .ToList();
                        break;
                }
            }
            catch( CryptographicException exCrypto )
            {
                _logger?.LogError( "Cryptographic exception occurred while decrypting {prop}, message is '{mesg}'",
                                   propPath.Last().Name,
                                   exCrypto.Message );
            }
            catch( Exception ex )
            {
                _logger?.LogCritical( "Exception occurred while decrypting {prop}, message is '{mesg}'",
                                      propPath.Last().Name,
                                      ex.Message );
            }

            if( decryptedValue != null )
                propPath.Last().SetValue( parentProp, decryptedValue );
        }
    }
}
