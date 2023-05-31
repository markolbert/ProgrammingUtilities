using J4JSoftware.EncryptedConfiguration;

namespace Test.WindowsUtilities;

#pragma warning disable CS8618
internal class TestConfigCore : ConsoleAppConfig
{
    public int NotEncryptable { get; set; }

    [EncryptedProperty]
    public string ProtectedText { get; set; }

    [EncryptedProperty]
    public string? ProtectedNullableText { get; set; }

    [EncryptedProperty]
    public string[] ProtectedTextArray { get; set; }

    [EncryptedProperty]
    public string?[]? ProtectedNullableTextArray { get; set; }

    [EncryptedProperty]
    public List<string> ProtectedTextList { get; set; }

    [EncryptedProperty]
    public List<string?>? ProtectedNullableTextList { get; set; }
}
