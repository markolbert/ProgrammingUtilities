using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace J4JSoftware.WPFUtilities
{
    public class AutoFacWPFUtilities : Module
    {
        protected override void Load( ContainerBuilder builder )
        {
            base.Load( builder );

            // register IRangeCalculators
            builder.RegisterAssemblyTypes(typeof(RangeCalculator<>).Assembly)
                .Where(t =>
                {
                    if (t.IsAbstract)
                        return false;

                    if (!t.BaseType?.IsGenericType ?? false)
                        return false;

                    return t.BaseType!.GetGenericTypeDefinition() == typeof(RangeCalculator<>)
                           && t.GetConstructors().Any();
                })
                .AsImplementedInterfaces();
        }
    }
}
