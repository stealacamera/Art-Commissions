using Microsoft.AspNetCore.Identity;

namespace ArtCommissions.DAL.Entities;

public class AppUser : IdentityUser<int>
{
    public int NrSuspensionStrikes { get; set; }
    public string StripeCustomerId { get; set; } = null!;
    public string StripeAccountId { get; set; } = null!;
}

public class AppRole : IdentityRole<int>
{
}