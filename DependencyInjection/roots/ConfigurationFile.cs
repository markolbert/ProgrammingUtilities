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
    public record ConfigurationFile( string FilePath, bool Optional, bool ReloadOnChange )
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
            new FilePathEqualityComparer(StringComparison.OrdinalIgnoreCase);

        public bool FilePathIsRooted => Path.IsPathRooted( FilePath );
    }
}