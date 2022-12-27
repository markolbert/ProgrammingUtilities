# Basic Configuration Extension Methods

The API needs certain basic information to work, and offers a number of options for controlling how the `IHostBuilder` gets built.

```csharp
public static J4JHostConfiguration Publisher( this J4JHostConfiguration config, string publisher )
```

Specifies the publisher. **Required**, as it's needed to define the user configuration folder on Windows.

```csharp
public static J4JHostConfiguration ApplicationName( this J4JHostConfiguration config, string name )
```

Specifies the application name. **Required**, as it's needed to define the user configuration folder on Windows.

```csharp
public static J4JHostConfiguration DataProtectionPurpose( this J4JHostConfiguration config, string purpose )
```

Specifies the string used by the data protection subsystem to identify itself (and probably to seed the system, I suspect). *Optional* ( ApplicationName will be used if DataProtectionPurpose is not specified).

```csharp
public static J4JHostConfiguration OperatingSystem( this J4JHostConfiguration config, string osName )
```

Specifies the operating system under which the IHost is being created. **Required** for the command line processing subsystem. The value should be one defined in `OSNames` so the subsystem can recognize it).

```csharp
public static J4JHostConfiguration CaseSensitiveFileSystem( this J4JHostConfiguration config )
```

Specifies that the operating system uses case-**sensitive** file names. *Optional*. The builder assumes case-insensitive file names, as per Windows, if not configured otherwise.

```csharp
public static J4JHostConfiguration CaseInsensitiveFileSystem( this J4JHostConfiguration config )
```

Specifies that the operating system uses case-**insensitive** file names. *Optional*. The builder assumes case-insensitive file names, as per Windows, if not configured otherwise.
