using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ArtCommissions.Common.DTOs;

public class AppUser
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
}

public class AppUserStripeProfile
{
    public string StripeCustomerId { get; set; } = null!;
    public string StripeAccountId { get; set; } = null!;
}

public class AppUserAddRequestModel
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(60, ErrorMessage = "Username cannot be longer than 60 characters")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [ValidateNever]
    public string StripeAccountId { get; set; } = null!;

    [ValidateNever]
    public string StripeCustomerId { get; set; } = null!;

    [ValidateNever]
    public string Role { get; set; } = null!;
}
