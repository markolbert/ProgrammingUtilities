#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'WPFViewModel' is free software: you can redistribute it
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

namespace J4JSoftware.DependencyInjection.Deprecated
{
    public class ViewModelDependency
    {
        private readonly IJ4JLogger? _logger;

        public ViewModelDependency( Type vmInterface, IJ4JLogger? logger )
        {
            _logger = logger;
            _logger?.SetLoggedType( GetType() );

            if( vmInterface.IsInterface )
                ViewModelInterface = vmInterface;
            else _logger?.Error( "Type '{0}' is not an interface", vmInterface );
        }

        public bool IsValid => ViewModelInterface != null
                               && TypeImplementsInterface( RunTimeType )
                               && ( DesignTimeType == null || TypeImplementsInterface( DesignTimeType ) );

        internal Type? ViewModelInterface { get; }

        internal Type? RunTimeType { get; private set; }

        internal Type? DesignTimeType { get; private set; }

        internal bool MultipleInstances { get; private set; } = true;

        public ViewModelDependency RunTime<T>()
        {
            RunTimeType = typeof(T);
            return this;
        }

        public ViewModelDependency DesignTime<T>()
        {
            DesignTimeType = typeof(T);
            return this;
        }

        public ViewModelDependency WithMultipleInstances()
        {
            MultipleInstances = true;
            return this;
        }

        public ViewModelDependency SingleInstance()
        {
            MultipleInstances = false;
            return this;
        }

        private bool TypeImplementsInterface( Type? toCheck )
        {
            if( toCheck == null )
            {
                _logger?.Information( "Type to check is undefined, cannot check validity" );
                return false;
            }

            if( ViewModelInterface != null )
                return ViewModelInterface.IsAssignableFrom( toCheck );

            _logger?.Information( "ViewModelInterface is not defined, cannot check validity of Type '{0}'",
                toCheck );

            return false;
        }
    }
}