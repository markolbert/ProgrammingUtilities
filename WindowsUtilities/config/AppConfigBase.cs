#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// AppConfigBase.cs
//
// This file is part of JumpForJoy Software's WindowsUtilities.
// 
// WindowsUtilities is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// WindowsUtilities is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with WindowsUtilities. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.DataProtection;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace J4JSoftware.WindowsUtilities;

public class AppConfigBase : ObservableObject
{
    private enum EncryptablePropertyType
    {
        Object,
        Unencrypted,
        SimpleString,
        StringArray,
        StringEnumerable
    }

    private record EncryptedProperty(
        PropertyInfo Property,
        EncryptablePropertyType Type,
        List<PropertyInfo>? PropertyPath
    );

    public static string UserFolder { get; } = Windows.Storage.ApplicationData.Current.LocalFolder.Path;

    private readonly List<EncryptedProperty> _encryptedProps = new();

    protected AppConfigBase()
    {
        FindEncryptableProperties( GetType().GetProperties().Where( x => x.CanWrite ), null );
    }

    private void FindEncryptableProperties(
        IEnumerable<PropertyInfo> properties,
        List<PropertyInfo>? propertyPath
    )
    {
        foreach( var curProp in properties )
        {
            var propType = GetSupportedType( curProp );

            if( propType != EncryptablePropertyType.Object )
            {
                _encryptedProps.Add( new EncryptedProperty( curProp, propType, propertyPath ) );
                continue;
            }

            // we can't handle any types which don't have a parameterless public ctor
            if( curProp.PropertyType.GetConstructors().All( x => x.GetParameters().Length != 0 ) )
                continue;

            // recurse over curProp's public properties
            propertyPath = propertyPath == null ? new List<PropertyInfo>() : new List<PropertyInfo>( propertyPath );
            propertyPath.Add( curProp );

            FindEncryptableProperties( curProp.PropertyType.GetProperties().Where( x => x.CanWrite ),
                                       propertyPath );
        }
    }

    private EncryptablePropertyType GetSupportedType( PropertyInfo propInfo )
    {
        var taggedForEncryption = propInfo.GetCustomAttribute<EncryptedPropertyAttribute>() != null;
        var propType = propInfo.PropertyType;

        if( propType == typeof( string ) )
            return taggedForEncryption ? EncryptablePropertyType.SimpleString : EncryptablePropertyType.Unencrypted;

        if( propType == typeof( string[] ) )
            return taggedForEncryption ? EncryptablePropertyType.StringArray : EncryptablePropertyType.Unencrypted;

        if( propType.IsAssignableTo( typeof( IEnumerable<string> ) ) )
            return taggedForEncryption? EncryptablePropertyType.StringEnumerable : EncryptablePropertyType.Unencrypted;

        return propType.IsValueType ? EncryptablePropertyType.Unencrypted : EncryptablePropertyType.Object;
    }

    [ JsonIgnore ]
    public string? UserConfigurationFilePath { get; set; }

    [ JsonIgnore ]
    public bool UserConfigurationFileExists =>
        !string.IsNullOrEmpty( UserConfigurationFilePath )
     && File.Exists( Path.Combine( UserFolder, UserConfigurationFilePath ) );

    public PositionSize MainWindowRectangle { get; set; } = PositionSize.Empty;

    public void Encrypt( IDataProtector protector )
    {
        foreach( var encProp in _encryptedProps )
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch( encProp.Type )
            {
                case EncryptablePropertyType.SimpleString:
                    EncryptProperty( encProp, protector );
                    break;

                case EncryptablePropertyType.StringArray:
                    EncryptArray( encProp, protector );
                    break;

                case EncryptablePropertyType.StringEnumerable:
                    EncryptList( encProp, protector );
                    break;
            }
        }
    }

    private object? GetLeafValue(EncryptedProperty encProp, object rootTarget)
    {
        // descend through the property path
        var curTarget = rootTarget;

        foreach (var propInfo in encProp.PropertyPath ?? Enumerable.Empty<PropertyInfo>())
        {
            if (propInfo.GetValue(curTarget) == null)
                return null;

            curTarget = propInfo.GetValue(curTarget);
        }

        return encProp.Property.GetValue(curTarget);
    }

    private void SetLeafValue( EncryptedProperty encProp, object rootTarget, object? value )
    {
        // create the properties along the property path, if one is defined
        var curTarget = rootTarget;

        foreach( var propInfo in encProp.PropertyPath ?? Enumerable.Empty<PropertyInfo>() )
        {
            if( propInfo.GetValue( curTarget ) == null )
                propInfo.SetValue( curTarget, Activator.CreateInstance( propInfo.PropertyType ) );

            curTarget = propInfo.GetValue( curTarget );
        }

        encProp.Property.SetValue( curTarget, value );
    }

    private void EncryptProperty( EncryptedProperty encProp, IDataProtector protector )
    {
        var plainText = (string?) GetLeafValue( encProp, this );
        if( string.IsNullOrEmpty( plainText ) )
            return;

        SetLeafValue( encProp, this, protector.Protect( plainText ) );
    }

    private void EncryptArray( EncryptedProperty encProp, IDataProtector protector )
    {
        var plainTextValues = (string?[]?) GetLeafValue( encProp, this );
        if( plainTextValues == null )
            return;

        var encryptedValues = plainTextValues.Select( x => x == null ? null : protector.Protect( x ) ).ToArray();
        SetLeafValue( encProp, this, encryptedValues );
    }

    private void EncryptList( EncryptedProperty encProp, IDataProtector protector )
    {
        var plainTextValues = (IEnumerable<string?>?) GetLeafValue( encProp, this );
        if( plainTextValues == null )
            return;

        var encryptedValues = plainTextValues.Select( x => x == null ? null : protector.Protect( x ) ).ToList();
        SetLeafValue( encProp, this, encryptedValues );
    }

    public void Decrypt( IDataProtector protector )
    {
        foreach ( var encProp in _encryptedProps )
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch( encProp.Type )
            {
                case EncryptablePropertyType.SimpleString:
                    DecryptProperty( encProp, protector );
                    break;

                case EncryptablePropertyType.StringArray:
                    DecryptArray(encProp, protector );
                    break;

                case EncryptablePropertyType.StringEnumerable:
                    DecryptList( encProp, protector );
                    break;
            }
        }
    }

    private void DecryptProperty( EncryptedProperty encProp, IDataProtector protector )
    {
        var encryptedText = (string?) GetLeafValue( encProp, this );
        if( string.IsNullOrEmpty( encryptedText ) )
            return;

        SetLeafValue( encProp, this, protector.Unprotect( encryptedText ) );
    }

    private void DecryptArray( EncryptedProperty encProp, IDataProtector protector )
    {
        var encryptedValues = (string?[]?) GetLeafValue( encProp, this );
        if( encryptedValues == null )
            return;

        var decryptedValues = encryptedValues.Select( x => x == null ? null : protector.Unprotect( x ) ).ToArray();
        SetLeafValue( encProp, this, decryptedValues );
    }

    private void DecryptList( EncryptedProperty encProp, IDataProtector protector )
    {
        var encryptedValues = (IEnumerable<string?>?) GetLeafValue( encProp, this );
        if( encryptedValues == null )
            return;

        var decryptedValues = encryptedValues.Select( x => x == null ? null : protector.Unprotect( x ) ).ToList();
        SetLeafValue( encProp, this, decryptedValues );
    }
}
