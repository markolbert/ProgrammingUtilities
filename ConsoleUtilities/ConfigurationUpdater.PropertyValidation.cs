using System;
using System.Collections.Generic;
using System.Reflection;
using Alba.CsConsoleFormat;
using J4JSoftware.Logging;

namespace J4JSoftware.ConsoleUtilities
{
    public partial class ConfigurationUpdater<TConfig>
    {
        private class PropertyValidation
        {
            public PropertyValidation( PropertyInfo propInfo, IPropertyUpdater updater )
            {
                PropertyInfo = propInfo;
                Updater = updater;
            }

            public PropertyInfo PropertyInfo { get; }
            public IPropertyUpdater Updater { get; }
        }
    }
}