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
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.DependencyInjection
{
    public interface IJ4JCompositionRootBase : IJ4JProtection
    {
        IHost? Host { get; }
        bool Initialized { get; }
        string ApplicationName { get; }
        string ApplicationConfigurationFolder { get; }
        string UserConfigurationFolder { get; }
        IJ4JProtection Protection { get; }
        J4JLogger GetJ4JLogger( Action<J4JLogger>? configureLogger = null );
        void Initialize();
    }

    public interface IJ4JCompositionRoot : IJ4JCompositionRootBase
    {
        bool UseConsoleLifetime { get; set; }
    }

    public interface IJ4JViewModelLocator : IJ4JCompositionRootBase
    {
        bool InDesignMode { get; }
    }
}