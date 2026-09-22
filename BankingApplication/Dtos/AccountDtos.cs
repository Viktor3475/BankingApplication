using BankingApplication.Models;

namespace BankingApplication.Dtos;

/// <summary>Fields a client may supply when opening another account; IBAN and balance are server-owned.</summary>
public sealed record CreateAccountDto(
    AccountType AccountType);

/// <summary>Account fields returned to clients, without the EF navigation property.</summary>
public sealed record AccountDto(Guid Id, string Iban, decimal Balance, AccountType AccountType, Guid BankUserId);
