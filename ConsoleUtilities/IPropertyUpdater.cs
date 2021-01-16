using System;
using Serilog;

namespace J4JSoftware.ConsoleUtilities
{
    public interface IPropertyUpdater
    {
        Type ValidatorType { get; }
        UpdaterResult Validate( object? origValue, out object? newValue );
    }

    public interface IPropertyUpdater<TProp> : IPropertyUpdater
    {
        UpdaterResult Validate( TProp? origValue, out TProp? newValue );
    }
}