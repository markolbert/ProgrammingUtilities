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
using System.Reflection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.DependencyInjection
{
    public abstract class J4JCompositionRoot : J4JCompositionRootBase, IJ4JCompositionRoot
    {
        protected J4JCompositionRoot(
            string publisher,
            string appName,
            string? dataProtectionPurpose = null,
            Type? loggerConfigType = null,
            params Assembly[] loggerChannelAssemblies
            )
            : base( publisher, appName, dataProtectionPurpose, loggerConfigType, loggerChannelAssemblies )
        {
        }

        public bool UseConsoleLifetime { get; set; }
        public override string ApplicationConfigurationFolder => Environment.CurrentDirectory;

        protected override void InitializeInternal()
        {
            base.InitializeInternal();

            if( UseConsoleLifetime )
                HostBuilder.UseConsoleLifetime();
        }
    }
}