using System;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Test.DependencyInjection
{
    public class CommandLineParsing
    {
        private class SimpleObject
        {
            public bool Switch { get; set; }
            public string? Text { get; set; }
        }

        private readonly SimpleObject _simple = new SimpleObject();

        [Theory]
        [InlineData(OSNames.Linux, "-x -y abc")]
        public void Simple(string osName, string cmdLine)
        {
            var consoleRoot = new CompositionRoot( ConfigureSimple, osName );

            var result = consoleRoot.Configuration.Get<SimpleObject>();
            result.Should().NotBeNull();
        }

        private void ConfigureSimple( IOptionCollection options )
        {
            options.Bind<SimpleObject, bool>( x => x.Switch, "x" );
            options.Bind<SimpleObject, string?>( x => x.Text, "y" );
        }
    }
}
