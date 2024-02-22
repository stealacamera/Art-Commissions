using ArtCommissions.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArtCommissions.DAL;

public static class Finalize
{
    public static async Task SeedData(this IServiceProvider services, IConfiguration configuration)
    {
        using (var scope = services.CreateScope())
        {
            await AppDbContextSeeder.Seed(
                configuration,
                scope.ServiceProvider.GetRequiredService<AppDbContext>(), 
                scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>());
        }
    }
}
