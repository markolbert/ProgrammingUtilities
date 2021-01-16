using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.DependencyInjection
{
    public partial class J4JCompositionRootBuilder
    {
        private readonly HashSet<FileInfo> _appFiles;
        private readonly HashSet<FileInfo> _userFiles;

        private ChannelInformation? _channelInfo;
        private bool _inclLastEvent;
        private CachedLoggerScope _cachedLoggerScope = CachedLoggerScope.None;
        private string _loggingSectionKey = "Logger";
        private IHostBuilder? _hostBuilder;

        public J4JCompositionRootBuilder(
            string appName,
            string publisher,
            bool isConsoleApp
            )
        {
            AppName = appName;
            Publisher = publisher;
            IsConsoleApp = isConsoleApp;

            AppFolder = Environment.CurrentDirectory;
            UserFolder = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ),
                Publisher,
                AppName );

            OnWindows = Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => true,
                PlatformID.Win32S => true,
                PlatformID.Win32Windows => true,
                PlatformID.WinCE => true,
                PlatformID.Xbox => true,
                _ => false
            };

            var fiCompare = OnWindows
                ? new FileInfoEqualityComparer( StringComparison.OrdinalIgnoreCase )
                : new FileInfoEqualityComparer( StringComparison.Ordinal );

            _appFiles = new HashSet<FileInfo>( fiCompare );
            _userFiles = new HashSet<FileInfo>( fiCompare );

            _hostBuilder = new HostBuilder()
                .UseServiceProviderFactory( new AutofacServiceProviderFactory() );
        }

        public string AppName { get; }
        public string Publisher { get; }
        public string AppFolder { get; }
        public string UserFolder { get; }

        public bool OnWindows { get; }

        public bool IsConsoleApp { get; }

#pragma warning disable 8603
        public IHostBuilder HostBuilder => _hostBuilder;
#pragma warning restore 8603
        public bool HasBeenBuilt => _hostBuilder == null;

        public J4JCompositionRootBuilder AddAppJsonFile( string fileName, bool required = false, bool reload = false  )
        {
            var newItem = new FileInfo { FileName = fileName, Required = required, Reload = reload };

            if( !_appFiles.Contains( newItem ) )
                _appFiles.Add( newItem );

            return this;
        }

        public J4JCompositionRootBuilder AddUserJsonFile(string fileName, bool required = false, bool reload = false)
        {
            var newItem = new FileInfo { FileName = fileName, Required = required, Reload = reload };

            if (!_userFiles.Contains(newItem))
                _userFiles.Add(newItem);

            return this;
        }

        public J4JCompositionRootBuilder UseJ4JLogger( 
            ChannelInformation channelInfo,
            string loggingSectionKey = "Logging", 
            CachedLoggerScope cachedLoggerScope = CachedLoggerScope.None,
            bool inclLastEvent = false )
        {
            _channelInfo = channelInfo;
            _loggingSectionKey = loggingSectionKey;
            _cachedLoggerScope = cachedLoggerScope;
            _inclLastEvent = inclLastEvent;

            return this;
        }

        //public J4JCompositionRoot? Build()
        //{
        //    if( _hostBuilder == null )
        //        return null;

        //    if( _channelInfo != null )
        //        _hostBuilder.ConfigureContainer<ContainerBuilder>( ( hbc, builder ) =>
        //        {
        //            var factory = new ChannelFactory(
        //                hbc.Configuration,
        //                _channelInfo,
        //                _loggingSectionKey,
        //                _inclLastEvent );

        //            builder.RegisterJ4JLogging<J4JLoggerConfiguration>( factory );
        //        } );
        //}
    }
}