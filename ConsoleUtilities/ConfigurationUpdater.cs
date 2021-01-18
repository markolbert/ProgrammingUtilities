using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Alba.CsConsoleFormat;
using Alba.CsConsoleFormat.Fluent;
using J4JSoftware.Logging;

namespace J4JSoftware.ConsoleUtilities
{
    public partial class ConfigurationUpdater<TConfig> : IConfigurationUpdater<TConfig>
        where TConfig: class
    {
        private readonly Dictionary<string, PropertyValidation> _updaters = new();

        public ConfigurationUpdater(
            Func<IJ4JLogger>? loggerFactory
        )
        {
            Logger = loggerFactory?.Invoke();
            Logger?.SetLoggedType( GetType() );
        }

        protected IJ4JLogger? Logger { get; }

        public IConfigurationUpdater<TConfig> Property<TProp>( 
            Expression<Func<TConfig, TProp>> propertySelector,
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
                Logger?.Error("Could not bind to property");
                return this;
            }

            if( _updaters.ContainsKey( propInfo.Name ) )
                _updaters[ propInfo.Name ] = new PropertyValidation( propInfo, updater );
            else _updaters.Add( propInfo.Name, new PropertyValidation( propInfo, updater ) );

            return this;
        }

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
                            Logger?.Error<string>( "Validation of {0} was cancelled", propVal.PropertyInfo.Name );
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

            Logger?.Error( "Got a {0} but required a {1}", config.GetType(), typeof(TConfig) );

            return false;
        }
    }
}
