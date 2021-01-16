using System;
using System.Reflection;
using J4JSoftware.Logging;

namespace J4JSoftware.ConsoleUtilities
{
    public partial class ConfigurationUpdater<TConfig>
    {
        private class PropertyValidation
        {
            public PropertyValidation( PropertyInfo propInfo, Func<IJ4JLogger>? loggerFactory )
            {
                PropertyInfo = propInfo;

                var logger = loggerFactory?.Invoke();

                var valAttr = propInfo.GetCustomAttribute<UpdaterAttribute>();
                if( valAttr == null )
                    return;

                if( logger == null )
                    Validator = (IPropertyUpdater?) Activator.CreateInstance( valAttr.ValidatorType,
                        new object?[] { null } );
                else
                    Validator = (IPropertyUpdater?) Activator.CreateInstance( valAttr.ValidatorType,
                        new object[] { logger } );
            }

            public PropertyInfo PropertyInfo { get; set; }
            public IPropertyUpdater? Validator { get; set; }
        }
    }
}