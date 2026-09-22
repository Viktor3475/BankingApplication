using BankingApplication.Dtos;
using BankingApplication.Models;
using BankingApplication.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingApplication.Controllers;

/// <summary>Server-rendered pages using a separate cookie session from the bearer-token API.</summary>
[Route("web")]
[Authorize(AuthenticationSchemes = "WebCookie")]
public sealed class WebController(
    IAuthService auth, IBankUserService users, IAccountService accounts,
    IMoneyMovementService movements, ICurrentUser currentUser) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var user = await users.GetAsync(currentUser.Id, cancellationToken);
        if (user is null) return NotFound();
        return View(new DashboardViewModel(user,
            await accounts.GetAllAsync(currentUser.Id, cancellationToken),
            await movements.GetHistoryAsync(currentUser.Id, cancellationToken)));
    }

    [AllowAnonymous, HttpGet("register")]
    public IActionResult Register() => View(new RegisterForm());

    [AllowAnonymous, HttpPost("register"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterForm form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(form);
        var result = await users.CreateAsync(new RegisterUserDto(
            form.Username, form.Email, form.Password, form.Country!.Value), cancellationToken);
        if (result.Error is not null)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            return View(form);
        }
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous, HttpGet("login")]
    public IActionResult Login() => View(new LoginForm());

    [AllowAnonymous, HttpPost("login"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginForm form)
    {
        if (!ModelState.IsValid) return View(form);
        var principal = await auth.AuthenticateAsync(form.Email, form.Password);
        if (principal is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(form);
        }
        await HttpContext.SignInAsync("WebCookie", principal);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("logout"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("WebCookie");
        return RedirectToAction(nameof(Login));
    }

    [HttpPost("accounts"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAccount(AccountType accountType, CancellationToken cancellationToken)
    {
        var result = await accounts.CreateAsync(new CreateAccountDto(accountType), currentUser.Id, cancellationToken);
        if (result.Error is not null) TempData["Error"] = result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("transfers"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SendMoney(SendMoneyForm form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["TransferError"] = "Enter a valid source account, recipient IBAN, and positive amount.";
            return RedirectToAction(nameof(Index));
        }

        var result = await movements.SendAsync(form.SourceAccountId!.Value, currentUser.Id,
            new SendMoneyDto(form.RecipientIban, form.Amount, form.Description), cancellationToken);
        if (result.Error is not null) TempData["TransferError"] = result.Error;
        else TempData["TransferSuccess"] = "Money sent successfully.";
        return RedirectToAction(nameof(Index));
    }
}
