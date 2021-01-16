using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Alba.CsConsoleFormat;
using Alba.CsConsoleFormat.Fluent;
using J4JSoftware.Logging;

namespace J4JSoftware.ConsoleUtilities
{
    public partial class ConfigurationUpdater<TConfig> : IConfigurationUpdater<TConfig>
        where TConfig: class
    {
        private readonly Dictionary<string, PropertyValidation> _properties;

        public ConfigurationUpdater(
            IEnumerable<IPropertyUpdater> propValidators,
            Func<IJ4JLogger>? loggerFactory
        )
        {
            _properties = typeof(TConfig)
                .GetProperties()
                .Where( pi => pi.GetCustomAttribute<UpdaterAttribute>() != null )
                .ToDictionary(
                    x => x.Name,
                    x => new PropertyValidation( x, loggerFactory ) );

            Logger = loggerFactory?.Invoke();
            Logger?.SetLoggedType( GetType() );
        }

        protected IJ4JLogger? Logger { get; }

        public virtual bool Validate( TConfig config )
        {
            var retVal = true;

            foreach( var propVal in _properties
                .Select(kvp=>kvp.Value))
            {
                if( propVal.Validator == null )
                    Logger?.Error<string>( "No IPropertyUpdater defined for {0}", propVal.PropertyInfo.Name );
                else
                {
                    var result = propVal.Validator.Validate( propVal.PropertyInfo.GetValue( config ), out var newValue );

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
            }

            return retVal;
        }

        bool IConfigurationUpdater.Validate( object config )
        {
            if( config is TConfig castConfig )
                return Validate( castConfig );

            Logger?.Error( "Got a {0} but required a {1}", config.GetType(), typeof(TConfig) );

            return false;
        }
    }
}
