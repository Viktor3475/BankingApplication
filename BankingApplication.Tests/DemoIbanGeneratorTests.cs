using BankingApplication.Models;
using BankingApplication.Services;

namespace BankingApplication.Tests;

public sealed class DemoIbanGeneratorTests
{
    [Theory]
    [InlineData(Country.UK, "GB", 22)]
    [InlineData(Country.BG, "BG", 22)]
    [InlineData(Country.RO, "RO", 24)]
    [InlineData(Country.RU, "RU", 33)]
    [InlineData(Country.TR, "TR", 26)]
    public void Generate_UsesCountryFormatAndValidCheckDigits(Country country, string prefix, int length)
    {
        var iban = new DemoIbanGenerator().Generate(country);

        Assert.StartsWith(prefix, iban);
        Assert.Equal(length, iban.Length);
        Assert.All(iban, character => Assert.True(char.IsAsciiLetterOrDigit(character)));
        Assert.Equal(1, Mod97(iban[4..] + iban[..4]));
    }

    [Fact]
    public void UnitedStatesCannotReceiveAnIban()
    {
        var generator = new DemoIbanGenerator();

        Assert.False(generator.Supports(Country.US));
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Generate(Country.US));
    }

    private static int Mod97(string reordered)
    {
        var remainder = 0;
        foreach (var character in reordered)
        {
            var digits = char.IsAsciiDigit(character)
                ? character.ToString()
                : (character - 'A' + 10).ToString();
            foreach (var digit in digits)
                remainder = (remainder * 10 + digit - '0') % 97;
        }
        return remainder;
    }
}
