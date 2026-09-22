using BankingApplication.Dtos;
using BankingApplication.Models;

namespace BankingApplication.Services;

/// <summary>Queries accounts and creates zero-balance accounts with server-generated demo IBANs.</summary>
public sealed class AccountService(IAccountStore store, DemoIbanGenerator ibanGenerator) : IAccountService
{
    /// <inheritdoc />
    public async Task<AccountDto?> GetAsync(Guid id, Guid ownerId, CancellationToken cancellationToken)
    {
        var account = await store.GetAsync(id, ownerId, cancellationToken);
        return account is null ? null : ToDto(account);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AccountDto>> GetAllAsync(
        Guid ownerId, CancellationToken cancellationToken)
    {
        var accounts = await store.GetAllAsync(ownerId, cancellationToken);
        return accounts.Select(ToDto).ToList();
    }

    /// <inheritdoc />
    public async Task<ServiceResult<AccountDto>> CreateAsync(
        CreateAccountDto request, Guid ownerId, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.AccountType))
            return ServiceResult<AccountDto>.Invalid("Invalid account type.");
        var country = await store.GetOwnerCountryAsync(ownerId, cancellationToken);
        if (country is null)
            return ServiceResult<AccountDto>.Invalid("Bank user does not exist.");
        if (!ibanGenerator.Supports(country.Value))
            return ServiceResult<AccountDto>.Invalid("Automatic IBAN creation is unavailable for this country.");

        var account = new Account
        {
            Iban = ibanGenerator.Generate(country.Value),
            AccountType = request.AccountType,
            BankUserId = ownerId,
            Balance = 0m
        };
        if (!await store.TryCreateAsync(account, cancellationToken))
            return ServiceResult<AccountDto>.Conflict("IBAN already exists.");

        return ServiceResult<AccountDto>.Success(ToDto(account));
    }

    private static AccountDto ToDto(Account account) => new(
        account.Id, account.Iban, account.Balance, account.AccountType, account.BankUserId);
}
