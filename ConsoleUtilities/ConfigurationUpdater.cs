#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// 
// This file is part of J4JLogger.
//
// J4JLogger is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JLogger is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JLogger. If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.ConsoleUtilities;

public partial class ConfigurationUpdater<TConfig> : IConfigurationUpdater<TConfig>
    where TConfig : class
{
    private readonly Dictionary<string, PropertyValidation> _updaters = new();

    public ConfigurationUpdater( ILoggerFactory? loggerFactory )
    {
        Logger = loggerFactory?.CreateLogger( GetType() );
    }

    protected ILogger? Logger { get; }

    public virtual bool Update( TConfig config )
    {
        var retVal = true;

        foreach( var propVal in _updaters
                   .Select( kvp => kvp.Value ) )
        {
            var result =
                propVal.Updater.Update( propVal.PropertyInfo.GetValue( config ), out var newValue );

            switch( result )
            {
                case UpdaterResult.Changed:
                    propVal.PropertyInfo.SetValue( config, newValue );
                    break;

                case UpdaterResult.InvalidUserInput:
                    Logger?.LogError( "Validation of {0} was cancelled", propVal.PropertyInfo.Name );
                    retVal = false;

                    break;
            }
        }

        return retVal;
    }

    bool IConfigurationUpdater.Update( object config )
    {
        if( config is TConfig castConfig )
            return Update( castConfig );

        Logger?.LogError( "Got a {0} but required a {1}", config.GetType(), typeof( TConfig ) );

        return false;
    }

    public IConfigurationUpdater<TConfig> Property<TProp>( Expression<Func<TConfig, TProp>> propertySelector,
        IPropertyUpdater<TProp> updater )
    {
        var curExpr = propertySelector.Body;
        PropertyInfo? propInfo = null;

        switch( curExpr )
        {
            case MemberExpression memExpr:
                propInfo = (PropertyInfo) memExpr.Member;
                break;

            case UnaryExpression unaryExpr:
                if( unaryExpr.Operand is MemberExpression unaryMemExpr )
                    propInfo = (PropertyInfo) unaryMemExpr.Member;

                break;
        }

        if( propInfo == null )
        {
            Logger?.LogError( "Could not bind to property" );
            return this;
        }

        if( _updaters.ContainsKey( propInfo.Name ) )
            _updaters[ propInfo.Name ] = new PropertyValidation( propInfo, updater );
        else _updaters.Add( propInfo.Name, new PropertyValidation( propInfo, updater ) );

        return this;
    }
}