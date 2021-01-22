using System;
using J4JSoftware.Logging;

namespace J4JSoftware.ConsoleUtilities
{
    public abstract class PropertyUpdater<TProp> : IPropertyUpdater<TProp>
    {
        protected PropertyUpdater( IJ4JLogger? logger )
        {
            Logger = logger;
            Logger?.SetLoggedType( GetType() );
        }

        protected IJ4JLogger? Logger { get; }

        public abstract UpdaterResult Update( TProp? origValue, out TProp? newValue );

        public Type ValidatorType => typeof(TProp);

        UpdaterResult IPropertyUpdater.Update( object? origValue, out object? newValue )
        {
            newValue = origValue;

            if( origValue is TProp castValue )
            {
                var result = Update( castValue, out var innerNew );

                if( result == UpdaterResult.Changed)
                    newValue = innerNew;

                return result;
            }

            Logger?.Error( "Expected a {0} but got a {1}", typeof(TProp), origValue?.GetType() );

            return UpdaterResult.InvalidValidator;
        }
    }
}