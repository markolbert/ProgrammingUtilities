using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.DependencyInjection
{
    public class J4JHostBuilder : HostBuilder
    {
        public static string[] ExpandUnvaluedCommandLineSwitches()
        {
            var args = Environment.GetCommandLineArgs();

            var retVal = new List<string> { args[ 0 ] };

            foreach ( var arg in Environment.GetCommandLineArgs().Skip(1) )
            {
                var isSwitch = true;

                foreach( var curChar in arg.Trim() )
                {
                    var charAsText = curChar.ToString();

                    if( charAsText == "/" 
                        || charAsText == "-" 
                        || (curChar != ' ' && curChar != '=') )
                        continue;

                    isSwitch = false;
                    break;
                }

                retVal.Add( isSwitch ? $"{arg} true" : arg );
            }

            return retVal.ToArray();
        }
    }
}
