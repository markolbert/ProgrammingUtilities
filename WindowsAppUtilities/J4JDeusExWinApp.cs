using System.IO;
using J4JSoftware.DependencyInjection;

namespace J4JSoftware.WindowsAppUtilities
{
    public class J4JDeusExWinApp : J4JDeusExHosted
    {
        protected override string GetCrashFilePath( J4JHostConfiguration hostConfig, string crashFileName = "crashFile.txt" )
        {
            var fileName = Path.GetFileName(crashFileName);

            if (string.IsNullOrEmpty(fileName))
                fileName = "crashFile.txt";

            return Path.Combine( Windows.Storage.ApplicationData.Current.LocalFolder.Path, fileName );
        }
    }
}
