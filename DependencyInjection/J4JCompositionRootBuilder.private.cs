using System;
using System.Collections.Generic;

namespace J4JSoftware.DependencyInjection
{
    public partial class J4JCompositionRootBuilder
    {
        private record FileInfo
        {
            public string FileName { get; init;}
            public bool Required { get; init; }
            public bool Reload { get; init; }
        }

        private sealed class FileInfoEqualityComparer : IEqualityComparer<FileInfo>
        {
            private readonly StringComparison _comparison;

            public FileInfoEqualityComparer(StringComparison comparison)
            {
                _comparison = comparison;
            }

            public bool Equals(FileInfo? x, FileInfo? y)
            {
                if ( ReferenceEquals(x, y)) return true;
                if ( ReferenceEquals(x, null)) return false;
                if ( ReferenceEquals(y, null)) return false;

                return x.GetType() == y.GetType()
                       && string.Equals(x.FileName, y.FileName, _comparison);
            }

            public int GetHashCode(FileInfo obj)
            {
                return obj.FileName.GetHashCode();
            }
        }
    }
}