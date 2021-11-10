#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'Test.MiscellaneousUtilities' is free software: you can redistribute it
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using FluentAssertions;

namespace Test.MiscellaneousUtilities
{
    public class TestDataBase<T> : IEnumerable<object[]>
    {
        private readonly string _filePath;

        protected TestDataBase( string fileName )
        {
            _filePath = Path.Combine( Environment.CurrentDirectory, "test-data", fileName );
            File.Exists( _filePath ).Should().BeTrue();
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            var parsed = JsonSerializer.Deserialize<List<T>>( File.ReadAllText( _filePath ) );
            parsed.Should().NotBeNull();

            foreach( var unitData in parsed! )
            {
                yield return new object[] { unitData! };
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
