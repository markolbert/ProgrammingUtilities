using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Module = Autofac.Module;

namespace J4JSoftware.ConsoleUtilities
{
    public class AutofacConsoleUtilitiesModule : Module
    {
        private readonly Assembly[] _assemblies;

        public AutofacConsoleUtilitiesModule( params Assembly[] assemblies )
        {
            if( assemblies.Length == 0 )
            {
                var entryAssembly = Assembly.GetEntryAssembly();

                _assemblies = entryAssembly != null ? new[] { entryAssembly } : assemblies;
            }
            else _assemblies = assemblies;
        }

        public AutofacConsoleUtilitiesModule( params Type[] types )
        {
            if( types.Length == 0 )
            {
                var entryAssembly = Assembly.GetEntryAssembly();

                _assemblies = entryAssembly != null ? new[] { entryAssembly } : new Assembly[0];
            }
            else _assemblies = types.Select( t => t.Assembly ).Distinct().ToArray();
        }

        protected override void Load( ContainerBuilder builder )
        {
            base.Load( builder );

            builder.RegisterAssemblyTypes( _assemblies )
                .Where( t => !t.IsAbstract
                             && typeof(IPropertyUpdater).IsAssignableFrom( t )
                             && t.GetConstructors().Any( c => c.GetParameters().Length == 0 ) )
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}
