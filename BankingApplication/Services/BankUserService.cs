using BankingApplication.Dtos;
using BankingApplication.Models;

namespace BankingApplication.Services;

/// <summary>Validates profiles and saves each new user with a demo-funded initial account atomically.</summary>
public sealed class BankUserService(IBankUserStore store, DemoIbanGenerator ibanGenerator) : IBankUserService
{
    private const decimal DemoOpeningBalance = 1_000m;
    /// <inheritdoc />
    public async Task<BankUserDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await store.GetAsync(id, cancellationToken);
        return user is null ? null : ToDto(user);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<CreatedBankUserDto>> CreateAsync(
        RegisterUserDto request, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.Country))
            return ServiceResult<CreatedBankUserDto>.Invalid("Invalid country.");
        if (string.IsNullOrWhiteSpace(request.Username))
            return ServiceResult<CreatedBankUserDto>.Invalid("Username is required.");
        if (!ibanGenerator.Supports(request.Country))
            return ServiceResult<CreatedBankUserDto>.Invalid("Automatic IBAN creation is unavailable for this country.");

        var user = new BankUser
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Country = request.Country
        };
        var account = new Account
        {
            Iban = ibanGenerator.Generate(request.Country),
            AccountType = AccountType.Current,
            BankUser = user,
            BankUserId = user.Id,
            Balance = DemoOpeningBalance
        };
        var write = await store.RegisterAsync(user, account, request.Password, cancellationToken);
        if (!write.Succeeded)
            return write.IsConflict
                ? ServiceResult<CreatedBankUserDto>.Conflict(write.Error!)
                : ServiceResult<CreatedBankUserDto>.Invalid(write.Error!);

        return ServiceResult<CreatedBankUserDto>.Success(new CreatedBankUserDto(
            ToDto(user),
            new AccountDto(account.Id, account.Iban, account.Balance, account.AccountType, account.BankUserId)));
    }

    private static BankUserDto ToDto(BankUser user) => new(user.Id, user.Username, user.Email, user.Country);
}
