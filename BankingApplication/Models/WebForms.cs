using System.ComponentModel.DataAnnotations;

namespace BankingApplication.Models;

/// <summary>Fields accepted by the server-rendered registration form.</summary>
public sealed class RegisterForm
{
    [Required, StringLength(100)] public string Username { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    [Required] public Country? Country { get; set; }
}

/// <summary>Fields accepted by the server-rendered login form.</summary>
public sealed class LoginForm
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}

/// <summary>Data displayed on the signed-in user's home page.</summary>
public sealed record DashboardViewModel(
    BankingApplication.Dtos.BankUserDto User,
    IReadOnlyList<BankingApplication.Dtos.AccountDto> Accounts,
    IReadOnlyList<BankingApplication.Dtos.MoneyMovementDto> Movements);

/// <summary>Fields accepted by the server-rendered transfer form.</summary>
public sealed class SendMoneyForm
{
    [Required] public Guid? SourceAccountId { get; set; }
    [Required, StringLength(34, MinimumLength = 15)] public string RecipientIban { get; set; } = "";
    [Range(typeof(decimal), "0.01", "9999999999999999.99")] public decimal Amount { get; set; }
    [StringLength(140)] public string? Description { get; set; }
}
