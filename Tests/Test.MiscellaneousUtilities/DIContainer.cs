using Autofac;
using J4JSoftware.Utilities;

namespace Test.MiscellaneousUtilities;

public class DIContainer
{
    public static IContainer Default { get; }

    static DIContainer()
    {
        var builder = new ContainerBuilder();

        builder.RegisterAssemblyTypes( typeof( TickRanges ).Assembly )
               .Where( t => !t.IsAbstract
                        && typeof( ITickRange ).IsAssignableFrom( t ) )
               .SingleInstance()
               .AsImplementedInterfaces();

        builder.RegisterType<TickRanges>()
               .SingleInstance()
               .AsSelf();

        Default = builder.Build();
    }
}