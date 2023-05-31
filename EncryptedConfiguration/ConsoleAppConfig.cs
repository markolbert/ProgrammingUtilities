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

using System.Reflection;
using Microsoft.AspNetCore.DataProtection;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.EncryptedConfiguration;

public class ConsoleAppConfig : ObservableObject
{
    //private enum EncryptablePropertyType
    //{
    //    Object,
    //    Unencrypted,
    //    SimpleString,
    //    StringArray,
    //    StringEnumerable
    //}

    private static Type[] EncryptableTypes { get; } =
        new[] { typeof( string ), typeof( string[] ), typeof( IEnumerable<string> ) };

    //private record EncryptedProperty(
    //    PropertyInfo Property,
    //    EncryptablePropertyType Type,
    //    List<PropertyInfo>? PropertyPath
    //);

    public static string UserFolder { get; } = Environment.CurrentDirectory;

    private readonly List<List<PropertyInfo>> _propPaths;
    private readonly ILogger? _logger;

    protected ConsoleAppConfig(
        ILoggerFactory? loggerFactory = null
        )
    {
        _logger = loggerFactory?.CreateLogger( GetType() );
        _propPaths = GetEncryptableProperties( GetType() ).ToList();

        //FindEncryptableProperties( GetType().GetProperties().Where( x => x.CanWrite ), null );
    }

    //private void FindEncryptableProperties(
    //    IEnumerable<PropertyInfo> properties,
    //    List<PropertyInfo>? propertyPath
    //)
    //{
    //    foreach( var curProp in properties )
    //    {
    //        var propType = GetSupportedType( curProp );

    //        if( propType != EncryptablePropertyType.Object )
    //        {
    //            _propPaths.Add( new EncryptedProperty( curProp, propType, propertyPath ) );
    //            continue;
    //        }

    //        // we can't handle any types which don't have a parameterless public ctor
    //        if( curProp.PropertyType.GetConstructors().All( x => x.GetParameters().Length != 0 ) )
    //            continue;

    //        // recurse over curProp's public properties
    //        propertyPath = propertyPath == null ? new List<PropertyInfo>() : new List<PropertyInfo>( propertyPath );
    //        propertyPath.Add( curProp );

    //        FindEncryptableProperties( curProp.PropertyType.GetProperties().Where( x => x.CanWrite ),
    //                                   propertyPath );
    //    }
    //}

    //private EncryptablePropertyType GetSupportedType( PropertyInfo propInfo )
    //{
    //    var taggedForEncryption = propInfo.GetCustomAttribute<EncryptedPropertyAttribute>() != null;
    //    var propType = propInfo.PropertyType;

    //    if( propType == typeof( string ) )
    //        return taggedForEncryption ? EncryptablePropertyType.SimpleString : EncryptablePropertyType.Unencrypted;

    //    if( propType == typeof( string[] ) )
    //        return taggedForEncryption ? EncryptablePropertyType.StringArray : EncryptablePropertyType.Unencrypted;

    //    if( propType.IsAssignableTo( typeof( IEnumerable<string> ) ) )
    //        return taggedForEncryption? EncryptablePropertyType.StringEnumerable : EncryptablePropertyType.Unencrypted;

    //    return propType.IsValueType ? EncryptablePropertyType.Unencrypted : EncryptablePropertyType.Object;
    //}

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

    //private object? GetLeafValue(EncryptedProperty encProp, object rootTarget)
    //{
    //    // descend through the property path
    //    var curTarget = rootTarget;

    //    foreach (var propInfo in encProp.PropertyPath ?? Enumerable.Empty<PropertyInfo>())
    //    {
    //        if (propInfo.GetValue(curTarget) == null)
    //            return null;

    //        curTarget = propInfo.GetValue(curTarget);
    //    }

    //    return encProp.Property.GetValue(curTarget);
    //}

    //private void SetLeafValue( EncryptedProperty encProp, object rootTarget, object? value )
    //{
    //    // create the properties along the property path, if one is defined
    //    var curTarget = rootTarget;

    //    foreach( var propInfo in encProp.PropertyPath ?? Enumerable.Empty<PropertyInfo>() )
    //    {
    //        if( propInfo.GetValue( curTarget ) == null )
    //            propInfo.SetValue( curTarget, Activator.CreateInstance( propInfo.PropertyType ) );

    //        curTarget = propInfo.GetValue( curTarget );
    //    }

    //    encProp.Property.SetValue( curTarget, value );
    //}

    //private void EncryptList( EncryptedProperty encProp, IDataProtector protector )
    //{
    //    var plainTextValues = (IEnumerable<string?>?) GetLeafValue( encProp, this );
    //    if( plainTextValues == null )
    //        return;

    //    var encryptedValues = plainTextValues.Select( x => x == null ? null : protector.Protect( x ) ).ToList();
    //    SetLeafValue( encProp, this, encryptedValues );
    //}

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

            switch (valueToDecrypt)
            {
                case string stringValue:
                    decryptedValue = protector.Unprotect(stringValue);
                    break;

                case string?[] stringArray:
                    decryptedValue = stringArray.Select(x => x == null ? null : protector.Unprotect(x)).ToArray();
                    break;

                case IEnumerable<string?> stringEnumerable:
                    decryptedValue = stringEnumerable.Select(x => x == null ? null : protector.Unprotect(x)).ToList();
                    break;
            }

            if (decryptedValue == null)
                _logger?.LogError("Could not decrypt property '{name}'", propPath.Last().Name);
            else propPath.Last().SetValue(parentProp, decryptedValue);
        }
    }

    //private void DecryptProperty( EncryptedProperty encProp, IDataProtector protector )
    //{
    //    var encryptedText = (string?) GetLeafValue( encProp, this );
    //    if( string.IsNullOrEmpty( encryptedText ) )
    //        return;

    //    SetLeafValue( encProp, this, protector.Unprotect( encryptedText ) );
    //}

    //private void DecryptArray( EncryptedProperty encProp, IDataProtector protector )
    //{
    //    var encryptedValues = (string?[]?) GetLeafValue( encProp, this );
    //    if( encryptedValues == null )
    //        return;

    //    var decryptedValues = encryptedValues.Select( x => x == null ? null : protector.Unprotect( x ) ).ToArray();
    //    SetLeafValue( encProp, this, decryptedValues );
    //}

    //private void DecryptList( EncryptedProperty encProp, IDataProtector protector )
    //{
    //    var encryptedValues = (IEnumerable<string?>?) GetLeafValue( encProp, this );
    //    if( encryptedValues == null )
    //        return;

    //    var decryptedValues = encryptedValues.Select( x => x == null ? null : protector.Unprotect( x ) ).ToList();
    //    SetLeafValue( encProp, this, decryptedValues );
    //}
}
