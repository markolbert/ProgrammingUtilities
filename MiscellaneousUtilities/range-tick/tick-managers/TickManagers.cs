#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'WPFUtilities' is free software: you can redistribute it
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
using J4JSoftware.Logging;

namespace J4JSoftware.Utilities
{
    public class TickManagers : ITickManagers
    {
        private readonly Dictionary<Type, ITickManager> _extractors = new();
        private readonly IJ4JLogger? _logger;

        public TickManagers( IEnumerable<ITickManager> extractors,
                             IJ4JLogger? logger )
        {
            _logger = logger;
            _logger?.SetLoggedType( GetType() );

            foreach( var extractor in extractors )
            {
                if( _extractors.ContainsKey( extractor.SourceType ) )
                {
                    var existing = _extractors[ extractor.SourceType ];

                    if( existing.IsSimpleExtractor == extractor.IsSimpleExtractor )
                        _logger?.Error( "Ignoring duplicate extractor for type '{0}'", extractor.SourceType );
                    else
                    {
                        // replace simple extractors with custom ones
                        if( existing.IsSimpleExtractor )
                        {
                            _logger?.Information( "Replacing simple extractor '{0}' with custom extractor '{1}'",
                                                 existing.GetType(),
                                                 extractor.GetType() );

                            _extractors[ extractor.SourceType ] = extractor;
                        }
                    }
                }
                else _extractors.Add( extractor.SourceType, extractor );
            }
        }

        public ITickManager? this[ Type sourceType ]
        {
            get
            {
                if( _extractors.ContainsKey( sourceType ) )
                    return _extractors[ sourceType ];

                _logger?.Warning( "No ITickManager defined for '{0}'", sourceType );

                return null;
            }
        }
    }
}
