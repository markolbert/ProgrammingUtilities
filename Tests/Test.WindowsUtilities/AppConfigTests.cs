using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;

namespace Test.WindowsUtilities;

public class AppConfigTests
{
    [ Fact ]
    public void EncryptDecrypt()
    {
        var plainCore = new TestConfigCore
        {
            NotEncryptable = 37,
            ProtectedText = "protected text",
            ProtectedNullableText = "protected nullable text",
            ProtectedTextArray = new[] { "protected text1", "protected text2", string.Empty },
            ProtectedNullableTextArray = new[] { "protected text1", "protected text2", null },
            ProtectedTextList = new List<string> { "protected text1", "protected text2", string.Empty },
            ProtectedNullableTextList = new List<string?> { "protected text1", "protected text2", null }
        };

        var plainText = new TestConfig
        {
            NotEncryptable = 37,
            ProtectedText = "protected text",
            ProtectedNullableText = "protected nullable text",
            ProtectedTextArray = new[] { "protected text1", "protected text2", string.Empty },
            ProtectedNullableTextArray = new[] { "protected text1", "protected text2", null },
            ProtectedTextList = new List<string> { "protected text1", "protected text2", string.Empty },
            ProtectedNullableTextList = new List<string?> { "protected text1", "protected text2", null },

            SubConfig = plainCore,
        };

        var localAppFolder = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );

        var dpProvider = DataProtectionProvider.Create(
            new DirectoryInfo( Path.Combine( localAppFolder, "ASP.NET", "DataProtection-Keys" ) ) );

        var protector = dpProvider.CreateProtector( "Test.WindowsUtilities" );

        plainText.Encrypt( protector );
        plainText.Decrypt( protector );

        CheckValues( plainText, plainText ).Should().BeTrue();
        CheckValues( plainText, plainText.SubConfig ).Should().BeTrue();
    }

    private bool CheckValues( TestConfigCore correct, TestConfigCore toCheck )
    {
        toCheck.NotEncryptable.Should().Be(correct.NotEncryptable);

        toCheck.ProtectedText.Should().Be(correct.ProtectedText);
        toCheck.ProtectedNullableText.Should().Be(correct.ProtectedNullableText);

        toCheck.ProtectedTextArray.Length.Should().Be(correct.ProtectedTextArray.Length);
        for (var idx = 0; idx < toCheck.ProtectedTextArray.Length; idx++)
        {
            toCheck.ProtectedTextArray[idx].Should().Be(correct.ProtectedTextArray[idx]);
        }

        toCheck.ProtectedNullableTextArray.Should().NotBeNull();
        toCheck.ProtectedNullableTextArray!.Length.Should().Be(correct.ProtectedNullableTextArray!.Length);
        for (var idx = 0; idx < toCheck.ProtectedNullableTextArray.Length; idx++)
        {
            toCheck.ProtectedNullableTextArray[idx].Should().Be(correct.ProtectedNullableTextArray[idx]);
        }

        toCheck.ProtectedTextList.Count.Should().Be(correct.ProtectedTextList.Count);
        for (var idx = 0; idx < toCheck.ProtectedTextList.Count; idx++)
        {
            toCheck.ProtectedTextList[idx].Should().Be(correct.ProtectedTextList[idx]);
        }

        toCheck.ProtectedNullableTextList.Should().NotBeNull();
        toCheck.ProtectedNullableTextList!.Count.Should().Be(correct.ProtectedNullableTextList!.Count);
        for (var idx = 0; idx < toCheck.ProtectedNullableTextList.Count; idx++)
        {
            toCheck.ProtectedNullableTextList[idx].Should().Be(correct.ProtectedNullableTextList[idx]);
        }

        return true;
    }
}