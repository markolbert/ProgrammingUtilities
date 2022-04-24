using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace J4JSoftware.WindowsAppUtilities
{
    public class J4JWinAppDeusEx : J4JDeusEx
    {
        public static bool Initialize(
            J4JWinAppHostConfiguration? hostConfig,
            string crashFileName = "crashFile.txt"
        )
        {
            var fileName = Path.GetFileName(crashFileName);
            if( string.IsNullOrEmpty( fileName ) )
                fileName = "crashFile.txt";

            return Initialize( hostConfig as J4JHostConfiguration,
                        Path.Combine( Windows.Storage.ApplicationData.Current.LocalFolder.Path, fileName ) );
        }
    }
}
