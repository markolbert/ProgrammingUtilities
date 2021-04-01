using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.EFCoreUtilities
{
    public static class OsUtilities
    {
        public static bool IsFileSystemCaseSensitive() => Environment.OSVersion.Platform switch
        {
            PlatformID.Win32NT => false,
            PlatformID.MacOSX => true,
            PlatformID.Unix => true,
            PlatformID.Win32S => false,
            PlatformID.Win32Windows => false,
            PlatformID.WinCE => false,
            PlatformID.Xbox => false,
            _ => false
        };

        public static string SqliteCollation( this SqliteCollationType type ) =>
            type switch
            {
                SqliteCollationType.IgnoreTrailingWhitespace => "RTRIM",
                SqliteCollationType.CaseInsensitiveAtoZ => "NOCASE",
                SqliteCollationType.CaseSensitive => "BINARY",
                _ => throw new InvalidEnumArgumentException( $"Unhandled {typeof(SqliteCollationType)} value '{type}'" )
            };

        public static StringComparison FileSystemComparison =>
            IsFileSystemCaseSensitive() ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
    }
}
