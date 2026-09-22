using System.Security.Cryptography;
using BankingApplication.Models;

namespace BankingApplication.Services;

/// <summary>
/// Generates demonstration identifiers with country-specific lengths and valid MOD-97 check digits.
/// Placeholder bank identifiers mean the output is not a bank-issued payment account.
/// </summary>
public sealed class DemoIbanGenerator
{
    /// <summary>Whether this prototype can create a demo IBAN for the selected country.</summary>
    public bool Supports(Country country) => country is Country.UK or Country.BG or Country.RO or Country.RU or Country.TR;

    /// <summary>Creates a demo IBAN. UK is represented by the IBAN country code GB.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The country has no supported IBAN format.</exception>
    public string Generate(Country country)
    {
        var (code, bban) = country switch
        {
            Country.UK => ("GB", "DEMO000000" + Digits(8)),
            Country.BG => ("BG", "DEMO000000" + Digits(8)),
            Country.RO => ("RO", "DEMO" + Digits(16)),
            Country.RU => ("RU", "00000000000000" + Digits(15)),
            Country.TR => ("TR", "000000" + Digits(16)),
            _ => throw new ArgumentOutOfRangeException(nameof(country), "No IBAN format is available for this country.")
        };

        // ISO 13616: move the country and temporary check digits after the BBAN, then take MOD 97.
        var remainder = 0;
        foreach (var character in bban + code + "00")
        {
            if (char.IsAsciiDigit(character)) remainder = (remainder * 10 + character - '0') % 97;
            else remainder = (remainder * 100 + character - 'A' + 10) % 97;
        }
        return code + (98 - remainder).ToString("D2") + bban;
    }

    private static string Digits(int count)
    {
        var result = new char[count];
        for (var i = 0; i < count; i++)
            result[i] = (char)('0' + RandomNumberGenerator.GetInt32(10));
        return new string(result);
    }
}
