using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuickSoftPilot.Models;

namespace QuickSoftPilot.Data
{
    // LocalDB is used ONLY for authentication (Core Identity) in this pilot.
    // All business data (categories, quotations, customers...) comes from the real emirtechlatest DB.
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider sp)
        {
            var db = sp.GetRequiredService<PilotDbContext>();
            await db.Database.EnsureCreatedAsync();

            var users = sp.GetRequiredService<UserManager<ApplicationUser>>();
            if (await users.FindByNameAsync("admin") == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin",
                    Name = "Administrator",
                    Email = "admin@quicknet.local",
                    EmailConfirmed = true,
                    BranchID = 1
                };
                await users.CreateAsync(admin, "Admin@123");
            }
        }
    }
}
