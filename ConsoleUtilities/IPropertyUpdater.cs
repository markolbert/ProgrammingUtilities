using System;
using Serilog;

namespace J4JSoftware.ConsoleUtilities
{
    public interface IPropertyUpdater
    {
        Type ValidatorType { get; }
        UpdaterResult Update( object? origValue, out object? newValue );
    }

    public interface IPropertyUpdater<TProp> : IPropertyUpdater
    {
        UpdaterResult Update( TProp? origValue, out TProp? newValue );
    }
}