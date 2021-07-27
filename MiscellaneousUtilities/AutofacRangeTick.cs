using System.Linq;
using Autofac;

namespace J4JSoftware.Utilities
{
    public class AutofacRangeTick : Module
    {
        protected override void Load( ContainerBuilder builder )
        {
            base.Load( builder );

            builder.RegisterType<RangeCalculator>()
                .As<IRangeCalculator>();

            builder.RegisterType<TickManagers>()
                .As<ITickManagers>();

            builder.RegisterAssemblyTypes( typeof(ITickManager).Assembly )
                .Where( t => !t.IsAbstract
                             && typeof(ITickManager).IsAssignableFrom( t )
                             && t.GetConstructors().Where( x => !x.GetParameters().Any() ).Any() )
                .AsImplementedInterfaces();
        }
    }
}
