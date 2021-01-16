using System;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.ConsoleUtilities
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class UpdaterAttribute : Attribute
    {
        public UpdaterAttribute( Type validatorType )
        {
            if( !typeof(IPropertyUpdater).IsAssignableFrom( validatorType ) )
                throw new ArgumentException( $"{validatorType} is not a {nameof(IPropertyUpdater)}" );

            if( validatorType.GetConstructors().All( x =>
            {
                var ctorParams = x.GetParameters();

                return ctorParams.Length != 1 || !typeof(IJ4JLogger).IsAssignableFrom( ctorParams[ 0 ].ParameterType );
            } ) )
                throw new ArgumentException(
                    $"{validatorType} does not have a public constructor taking an {nameof(IJ4JLogger)}" );

            ValidatorType = validatorType;
        }

        public Type ValidatorType { get; }
    }
}