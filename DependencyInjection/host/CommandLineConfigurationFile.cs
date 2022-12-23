using System;
using System.Reflection;

namespace J4JSoftware.DependencyInjection;

public record CommandLineConfigurationFile(
    PropertyInfo Property,
    Type ConfigurationObjectType,
    bool IsRequired,
    bool ReloadOnChange
);
