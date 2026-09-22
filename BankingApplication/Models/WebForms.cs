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
    IReadOnlyList<BankingApplication.Dtos.AccountDto> Accounts);
