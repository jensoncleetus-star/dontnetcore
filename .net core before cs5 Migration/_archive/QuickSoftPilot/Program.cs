using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuickSoftPilot.Data;
using QuickSoftPilot.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PilotDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Read-only context over the REAL QuickNet database (emirtechlatest) on SQL Server Express.
builder.Services.AddDbContext<QuickSoftPilot.Legacy.LegacyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LegacyConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Relaxed for the pilot so the seeded admin/Admin@123 works without ceremony.
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.User.RequireUniqueEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<PilotDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
});

// Allow AJAX (DataTables delete) to send the antiforgery token via header.
builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Create the database + seed on startup. Pass `--reset-db` once to rebuild the
// schema after adding new entities (pilot uses EnsureCreated, which doesn't migrate).
using (var scope = app.Services.CreateScope())
{
    if (args.Contains("--reset-db"))
    {
        var pilotDb = scope.ServiceProvider.GetRequiredService<PilotDbContext>();
        var cs = (pilotDb.Database.GetConnectionString() ?? "").ToLowerInvariant();
        // SAFETY: only ever drop a LocalDB database — never a real SQL Server DB.
        if (cs.Contains("localdb"))
            await pilotDb.Database.EnsureDeletedAsync();
        else
            Console.WriteLine("[SAFETY] --reset-db ignored: target is not LocalDB.");
    }
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
