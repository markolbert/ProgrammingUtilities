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
using System.Collections.Generic;
using System.IO;

namespace J4JSoftware.DependencyInjection
{
    public class ConfigurationFile
    {
        private sealed class FilePathEqualityComparer : IEqualityComparer<ConfigurationFile>
        {
            private readonly StringComparison _textComparison;

            public FilePathEqualityComparer( StringComparison textComparison )
            {
                _textComparison = textComparison;
            }

            public bool Equals( ConfigurationFile? x, ConfigurationFile? y )
            {
                if( ReferenceEquals( x, y ) ) return true;
                if( ReferenceEquals( x, null ) ) return false;
                if( ReferenceEquals( y, null ) ) return false;
                if( x.GetType() != y.GetType() ) return false;

                return string.Equals( x.FilePath, y.FilePath, _textComparison );
            }

            public int GetHashCode( ConfigurationFile obj )
            {
                return obj.FilePath.GetHashCode();
            }
        }

        public static IEqualityComparer<ConfigurationFile> CaseSensitiveComparer { get; } =
            new FilePathEqualityComparer( StringComparison.Ordinal );

        public static IEqualityComparer<ConfigurationFile> CaseInsensitiveComparer { get; } =
            new FilePathEqualityComparer( StringComparison.OrdinalIgnoreCase );

        private readonly J4JHostConfiguration _hostConfig;
        private readonly ConfigurationFileType _configType;
        private readonly string _filePath;

        public ConfigurationFile(
            J4JHostConfiguration hostConfig,
            ConfigurationFileType configType,
            string filePath,
            bool optional = true,
            bool reloadOnChange = false
        )
        {
            _hostConfig = hostConfig;
            _configType = configType;

            _filePath = filePath;
            Optional = optional;
            ReloadOnChange = reloadOnChange;
        }

        public string FilePath
        {
            get
            {
                if( string.IsNullOrEmpty( _filePath ) )
                    return string.Empty;

                if( Path.IsPathRooted(_filePath))
                    return _filePath;

                var folder = _configType == ConfigurationFileType.Application
                    ? _hostConfig.ApplicationConfigurationFolder
                    : _hostConfig.UserConfigurationFolder;

                return Path.Combine( folder, _filePath );
            }
        }

        public bool Optional { get; }
        public bool ReloadOnChange { get; }

        public bool FilePathIsRooted => Path.IsPathRooted( FilePath );
    }
}
