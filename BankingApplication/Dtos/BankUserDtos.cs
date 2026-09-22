using System.ComponentModel.DataAnnotations;
using BankingApplication.Models;

namespace BankingApplication.Dtos;

/// <summary>Fields accepted when registering a bank user.</summary>
public sealed record RegisterUserDto(
    [Required, StringLength(100, MinimumLength = 1)] string Username,
    [Required, EmailAddress, StringLength(320)] string Email,
    [Required] string Password,
    Country Country);

/// <summary>Public profile fields selected from the bank user entity.</summary>
public sealed record BankUserDto(Guid Id, string Username, string Email, Country Country);

/// <summary>The new profile and its automatically created Current account.</summary>
public sealed record CreatedBankUserDto(BankUserDto User, AccountDto Account);
