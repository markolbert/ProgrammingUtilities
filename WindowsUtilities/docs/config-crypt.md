# AppConfigBase

## Usage

`AppConfigBase` provides a base class you can use to encrypt and decrypt application configuration files automatically. It uses the `IDataProtector` interface, which is available in Windows.

You use it by calling the following methods on an instance of a derived class:

|Method|Argument(s)|Description|
|------|-----------|-----------|
|`Encrypt`|`IDataProtector` protector|returns a new instance of the derived class, with the fields marked for encryption having *encrypted* values|
|`Decrypt`|`IDataProtector` protector|returns a new instance of the derived class, with the fields marked for encryption having *decrypted* values|

The derived class must have a public, parameterless constructor. This shouldn't be an issue, since the `IConfiguration` system requires that as well.

Only certain types of configuration properties can be encrypted/decrypted:

- simple string properties
- string array properties
- `IEnumerable<string>` properties

Nullable types of each of these are also supported. The supported collections allow `null` values, too, but they are not encrypted.

If the property type is not supported no encryption/decryption takes place and the value is simply copied over.

You flag the properties to be encrypted/decrypted by decorating them with an `EncryptedPropertyAttribute`.

## Example

*This is from the `Test.WindowsUtilities` project. XUnit and FluentAssertion code has been removed for clarity*

```csharp
// as this is for WinUI 3 apps, the configuration file will be located
// in Environment.SpecialFolder.LocalApplicationData
var localAppFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

// one way of creating an IDataProtectionProvider
var dpProvider = DataProtectionProvider.Create(
    new DirectoryInfo(Path.Combine(localAppFolder, "ASP.NET", "DataProtection-Keys")));

var protector = dpProvider.CreateProtector("Test.WindowsUtilities");

// configToEncrypt is an instance of your AppConfigBase-derived
// configuration class
var encrypted = configToEncrypt.Encrypt( protector );

// DerivedConfigClass is your app's config class
var decrypted = (DerivedConfigClass) encrypted.Decrypt( protector );
```
