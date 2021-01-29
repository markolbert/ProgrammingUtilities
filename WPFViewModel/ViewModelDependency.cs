using System;
using J4JSoftware.Logging;

namespace J4JSoftware.WPFViewModel
{
    public class ViewModelDependency
    {
        private readonly IJ4JLogger? _logger;

        public ViewModelDependency( IJ4JLogger? logger )
        {
            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public bool IsValid => ViewModelInterface != null
                               && TypeImplementsInterface( RunTimeType )
                               && ( DesignTimeType == null || TypeImplementsInterface( DesignTimeType ) );

        public Type? ViewModelInterface { get; private set; }

        public ViewModelDependency UsingInterface<T>()
        {
            var iType = typeof(T);

            if( iType.IsInterface )
                ViewModelInterface = iType;
            else _logger?.Error("Type '{0}' is not an interface", iType);

            return this;
        }

        public Type? RunTimeType { get; private set; }

        public ViewModelDependency RegisterRunTime<T>()
        {
            RunTimeType = typeof(T);
            return this;
        }

        public Type? DesignTimeType { get; private set; }

        public ViewModelDependency RegisterDesignTime<T>()
        {
            DesignTimeType = typeof(T);
            return this;
        }

        public bool MultipleInstances { get; private set; } = false;

        public ViewModelDependency WithMultipleInstances()
        {
            MultipleInstances = true;
            return this;
        }

        private bool TypeImplementsInterface( Type? toCheck )
        {
            if( toCheck == null )
            {
                _logger?.Information( "Type to check is undefined, cannot check validity" );
                return false;
            }

            if( ViewModelInterface != null ) 
                return ViewModelInterface.IsAssignableFrom( toCheck );

            _logger?.Information( "ViewModelInterface is not defined, cannot check validity of Type '{0}'",
                toCheck );

            return false;
        }
    }
}