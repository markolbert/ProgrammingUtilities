using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.DependencyInjection;

namespace Test.DependencyInjection
{
    public sealed class CompositionRoot : ConsoleRoot
    {
        private readonly Action<IOptionCollection> _configureOptions;

        public CompositionRoot(
            Action<IOptionCollection> configureOptions,
            string osName
            ) 
            : base( "J4JSoftware", "DITest", true, osName: osName)
        {
            _configureOptions = configureOptions;

            Build();
        }

        protected override void ConfigureCommandLineParsing()
        {
            base.ConfigureCommandLineParsing();

            _configureOptions( CommandLineOptions! );
        }
    }
}
