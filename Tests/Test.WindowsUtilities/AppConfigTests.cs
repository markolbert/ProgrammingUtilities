using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;

namespace Test.WindowsUtilities;

public class AppConfigTests
{
    [Fact]
    public void EncryptDecrypt()
    {
        var plainText = new TestConfig
        {
            NotEncryptable = 37,
            ProtectedText = "protected text",
            ProtectedNullableText = "protected nullable text",
            ProtectedTextArray = new[] { "protected text1", "protected text2", string.Empty },
            ProtectedNullableTextArray = new[] { "protected text1", "protected text2", null },
            ProtectedTextList = new List<string> { "protected text1", "protected text2", string.Empty },
            ProtectedNullableTextList = new List<string?> { "protected text1", "protected text2", null }
        };

        var localAppFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        var dpProvider = DataProtectionProvider.Create(
            new DirectoryInfo(Path.Combine(localAppFolder, "ASP.NET", "DataProtection-Keys")));

        var protector = dpProvider.CreateProtector("Test.WindowsUtilities");

        var encrypted = plainText.Encrypt( protector );
        var temp = encrypted.Decrypt( protector );
        temp.Should().BeAssignableTo<TestConfig>();

        var decrypted = (TestConfig) temp;

        decrypted.NotEncryptable.Should().Be( plainText.NotEncryptable );

        decrypted.ProtectedText.Should().Be(plainText.ProtectedText);
        decrypted.ProtectedNullableText.Should().Be( plainText.ProtectedNullableText );
        
        decrypted.ProtectedTextArray.Length.Should().Be( plainText.ProtectedTextArray.Length );
        for( var idx = 0; idx < decrypted.ProtectedTextArray.Length; idx++ )
        {
            decrypted.ProtectedTextArray[ idx ].Should().Be( plainText.ProtectedTextArray[ idx ] );
        }

        decrypted.ProtectedNullableTextArray.Should().NotBeNull();
        decrypted.ProtectedNullableTextArray!.Length.Should().Be(plainText.ProtectedNullableTextArray.Length);
        for (var idx = 0; idx < decrypted.ProtectedNullableTextArray.Length; idx++)
        {
            decrypted.ProtectedNullableTextArray[idx].Should().Be(plainText.ProtectedNullableTextArray[idx]);
        }

        decrypted.ProtectedTextList.Count.Should().Be(plainText.ProtectedTextList.Count);
        for (var idx = 0; idx < decrypted.ProtectedTextList.Count; idx++)
        {
            decrypted.ProtectedTextList[idx].Should().Be(plainText.ProtectedTextList[idx]);
        }

        decrypted.ProtectedNullableTextList.Should().NotBeNull();
        decrypted.ProtectedNullableTextList!.Count.Should().Be(plainText.ProtectedNullableTextList.Count);
        for (var idx = 0; idx < decrypted.ProtectedNullableTextList.Count; idx++)
        {
            decrypted.ProtectedNullableTextList[idx].Should().Be(plainText.ProtectedNullableTextList[idx]);
        }
    }
}