using System;
using System.Reflection;

namespace J4JSoftware.DependencyInjection;

public record ConfigurationFileKeys(
    bool Required,
    bool ReloadOnChange,
    params string[] CommandLineKeys
);
