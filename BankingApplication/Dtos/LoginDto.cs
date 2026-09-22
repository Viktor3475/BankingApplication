using System.ComponentModel.DataAnnotations;

namespace BankingApplication.Dtos;

/// <summary>Credentials exchanged for an ASP.NET Core Identity bearer token.</summary>
public sealed record LoginDto([Required, EmailAddress] string Email, [Required] string Password);
