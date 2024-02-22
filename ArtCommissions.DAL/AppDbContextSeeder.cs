using ArtCommissions.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ArtCommissions.DAL
{
    internal static class AppDbContextSeeder
    {
        public static async Task Seed(IConfiguration configuration, AppDbContext dbContext, UserManager<AppUser> userManager)
        {
            dbContext.Database.EnsureCreated();

            try
            {
                if (dbContext.Database.GetPendingMigrations().Count() > 0)
                    dbContext.Database.Migrate();
            }
            catch (Exception)
            {
                throw;
            }

            await SeedAdmin(configuration, userManager);
        }

        private static async Task SeedAdmin(IConfiguration configuration, UserManager<AppUser> userManager)
        {
            var admin = new AppUser 
            { 
                UserName = configuration["AdminSettings:Username"] ?? throw new Exception("\"AdminSettings:Username\" key missing"), 
                Email = configuration["AdminSettings:Email"] ?? throw new Exception("\"AdminSettings:Email\" key missing"),
                EmailConfirmed = true, 
                StripeAccountId = "0",
                StripeCustomerId = "0"
            };

            if ((await userManager.FindByNameAsync(admin.UserName)) == null)
            {
                await userManager.CreateAsync(admin, configuration["AdminSettings:Password"] ?? throw new Exception("\"AdminSettings:Password\" key missing"));
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
