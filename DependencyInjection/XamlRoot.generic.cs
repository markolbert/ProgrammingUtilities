#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'DependencyInjection' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using J4JSoftware.Logging;

namespace J4JSoftware.DependencyInjection
{
    public abstract class XamlRoot<TLoggerConfig, TLoggerConfigurator> 
        : CompositionRoot<TLoggerConfig, TLoggerConfigurator>
        where TLoggerConfig : class
        where TLoggerConfigurator : class, ILoggerConfigurator
    {
        private readonly Func<bool> _inDesignMode;

        protected XamlRoot(
            string publisher,
            string appName,
            Func<bool> inDesignMode,
            string? dataProtectionPurpose = null
        )
            : base(publisher, appName, dataProtectionPurpose)
        {
            _inDesignMode = inDesignMode;
        }

        public bool InDesignMode => _inDesignMode();

        public override string ApplicationConfigurationFolder =>
            InDesignMode ? AppContext.BaseDirectory : Environment.CurrentDirectory;
    }
}