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

        [Theory]
        [InlineData("-x -y abc", true, true, "abc")]
        [InlineData("-y def", false, false, "def")]
        public void SimpleLinuxParsing(string cmdLine, bool hasFlag, bool flag, string text )
        {
            var parser = Parser.GetLinuxDefault( null );
            parser.Should().NotBeNull();

            var flagOption = parser!.Options.Bind<SimpleObject, bool>(x => x.Switch, "x");
            var textOption = parser.Options.Bind<SimpleObject, string?>(x => x.Text, "y");
            parser.Options.FinishConfiguration();

            parser!.Parse( cmdLine ).Should().BeTrue();

            if( hasFlag )
            {
                flagOption!.ValuesSatisfied.Should().BeTrue();
                flagOption.GetValue( out var flagResult ).Should().BeTrue();
                flagResult.Should().Be( flag );
            }

            textOption!.ValuesSatisfied.Should().BeTrue();
            textOption.GetValue( out var textResult ).Should().BeTrue();
            textResult.Should().Be( text );
        }

        [Theory]
        [InlineData("-x -y abc", true, "abc")]
        [InlineData("-y def", false, "def")]
        public void LinuxCompositionRootParsing( string cmdLine, bool flag, string text )
        {
            var root = new CompositionRoot( ConfigureOptions, "Linux" );
            root.CommandLineSource.Should().NotBeNull();
            root.CommandLineSource!.SetCommandLine( cmdLine );

            var result = root.Configuration.Get<SimpleObject>();
            result.Should().NotBeNull();

            result.Switch.Should().Be( flag );
            result.Text.Should().Be( text );
        }

        private void ConfigureOptions( IOptionCollection options )
        {
            options.Bind<SimpleObject, bool>(x => x.Switch, "x");
            options.Bind<SimpleObject, string?>(x => x.Text, "y");
            options.FinishConfiguration();
        }
    }
}
