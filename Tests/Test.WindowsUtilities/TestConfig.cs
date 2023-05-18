using J4JSoftware.WindowsUtilities;
#pragma warning disable CS8618

namespace Test.WindowsUtilities;

internal class TestConfig : AppConfigBase
{
    public int NotEncryptable { get; set; }

    [EncryptedProperty]
    public string ProtectedText { get; set; }

    [EncryptedProperty]
    public string? ProtectedNullableText { get; set; }

    [EncryptedProperty]
    public string[] ProtectedTextArray { get; set; }
    
    [EncryptedProperty]
    public string?[]? ProtectedNullableTextArray { get; set;}

    [EncryptedProperty]
    public List<string> ProtectedTextList { get; set; }
    
    [EncryptedProperty]
    public List<string?>? ProtectedNullableTextList { get; set; }
}
