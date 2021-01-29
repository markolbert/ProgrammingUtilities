using System;
using System.Collections.Generic;
using J4JSoftware.Logging;

namespace J4JSoftware.WPFViewModel
{
    public class ViewModelDependencyBuilder
    {
        private readonly IJ4JLogger _logger;

        public ViewModelDependencyBuilder( IJ4JLogger logger )
        {
            _logger = logger;
        }

        public List<ViewModelDependency> ViewModelDependencies { get; } = new();

        public ViewModelDependency RegisterViewModelInterface<T>() => RegisterViewModelInterface( typeof(T) );

        public ViewModelDependency RegisterViewModelInterface( Type vmInterface )
        {
            var retVal = new ViewModelDependency( vmInterface, _logger );
            ViewModelDependencies.Add( retVal );

            return retVal;
        }
    }
}