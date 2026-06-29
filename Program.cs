using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuickSoft.Models;
using QuickSoft.Web;

var builder = WebApplication.CreateBuilder(args);

// Run cleanly under the Windows Service Control Manager when hosted via sc.exe (auto-start on boot,
// proper stop signals). No-op when launched as a plain console app, so dev/run is unaffected.
builder.Host.UseWindowsService();

// Connection string: ConnectionStrings:DefaultConnection from config (appsettings / env / secrets).
// In Production it is REQUIRED — never silently fall back to the dev copy. Only Development falls back
// to the local emirtechlatest working copy for convenience.
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(conn))
{
    if (!builder.Environment.IsDevelopment())
        throw new System.InvalidOperationException(
            "ConnectionStrings:DefaultConnection is not configured. Set it (per branch database) before running in Production.");
    conn = @"Server=.\SQLEXPRESS;Database=emirtechlatest;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False";
}

// EF Core 10 translates `list.Contains(column)` via OPENJSON, which fails on SQL Server databases at an
// older compatibility level ("Incorrect syntax near '$'"). Inline such collections as constants instead
// (the pre-EF8 behavior) so every Contains-on-collection query works regardless of the DB's compat level.
builder.Services.AddDbContext<ApplicationDbContext>(o =>
    o.UseSqlServer(conn, sql => sql.TranslateParameterizedCollectionsToConstants()));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(o =>
{
    // Security hardening (audit S1): minimum 8 chars + a digit for NEW/changed passwords
    // (existing passwords keep working — policy applies only when a password is set).
    o.Password.RequireNonAlphanumeric = false;
    o.Password.RequireUppercase = false;
    o.Password.RequireLowercase = false;
    o.Password.RequireDigit = true;
    o.Password.RequiredLength = 8;
    o.User.RequireUniqueEmail = false;
    o.SignIn.RequireConfirmedAccount = false;
    // Security hardening (audit S7): brute-force lockout — 5 failed attempts locks the
    // account for 5 minutes (login passes lockoutOnFailure: true).
    o.Lockout.AllowedForNewUsers = true;
    o.Lockout.MaxFailedAccessAttempts = 5;
    o.Lockout.DefaultLockoutTimeSpan = System.TimeSpan.FromMinutes(5);
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Users/Login";
    o.LogoutPath = "/Users/LogOff";
    o.AccessDeniedPath = "/Home/Unauthorize";
});

// Keep the auth cookie tiny: store the (large, many-role) AuthenticationTicket server-side and put
// only its key in the cookie. Fixes the 40 KB cookie / HTTP 431 for users with hundreds of roles.
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ITicketStore, MemoryTicketStore>();
builder.Services.AddOptions<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme)
    .PostConfigure<ITicketStore>((options, store) => options.SessionStore = store);

// Admin users have hundreds of roles; ASP.NET Core Identity writes every role as a claim into the
// auth cookie, which gets chunked into a request header far larger than Kestrel's 32 KB default ->
// HTTP 431. Raise the header limits so the large (chunked) auth cookie is accepted.
builder.WebHost.ConfigureKestrel(o =>
{
    o.Limits.MaxRequestHeadersTotalSize = 1048576; // 1 MB
    o.Limits.MaxRequestHeaderCount = 500;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();   // legacy Session["..."] usage
// Runtime Razor compilation so the 1,779 legacy views compile on-demand during the port.
// Keep controller JSON in PascalCase (PropertyNamingPolicy = null) — the legacy DataTables/jQuery code
// references PascalCase keys (e.g. { data: "BillNo" }); Core's System.Text.Json default camelCases them
// ("billNo"), breaking every server-side DataTable ("Requested unknown parameter 'BillNo'"). Also keep
// Newtonsoft available for the controllers that call JsonConvert directly.
builder.Services.AddControllersWithViews(o =>
    {
        o.Filters.Add<AjaxErrorFilter>();
        // Restore MVC5 validation parity: a blank OPTIONAL field ([Phone]/[EmailAddress]/[Url]/...) is valid
        // (.NET Core's DataAnnotations reject "" but MVC5 only rejected blank [Required] fields).
        o.Filters.Add<QuickSoft.Helpers.BlankOptionalFieldValidationFilter>();
        // Security hardening (audit S2, stage 2): every unsafe-verb request must carry the antiforgery
        // token. Coverage: BeginForm auto-injects it; _QuickLayout's ajaxSend hook adds the header to all
        // jQuery AJAX; the 13 raw-form views + 2 mobile App pages were given tokens explicitly (2026-06-12).
        o.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
        // Restore MVC5 binding: empty form fields -> "" (not null), so the legacy `if (x != "")` guards work.
        o.ModelMetadataDetailsProviders.Add(new KeepEmptyStringMetadataProvider());
        // Restore MVC5's JsonValueProviderFactory: legacy save/print actions POST a single application/json
        // body carrying MANY named parameters (UpdateSale(string[][] array, string[] saledata, ...)). Core
        // only feeds a json body to a single [FromBody] param, so without this they bound to EMPTY arrays
        // and threw IndexOutOfRange on the first `saledata[i]` ("Error !" toast on Save / Save+Print).
        o.ValueProviderFactories.Insert(0, new QuickSoft.Helpers.JsonBodyValueProviderFactory());
    })
    .AddRazorRuntimeCompilation()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = null;                       // PascalCase (DataTables keys)
        o.JsonSerializerOptions.Converters.Add(new MicrosoftDateTimeConverter());  // "/Date(ms)/" (legacy convertToDate)
        o.JsonSerializerOptions.Converters.Add(new MicrosoftNullableDateTimeConverter());
    });

// Performance (audit P3): pre-compile the hottest runtime-compiled views right after startup,
// so the first user after a restart doesn't pay the per-view Razor compile.
builder.Services.AddHostedService<QuickSoft.Helpers.ViewWarmupService>();

// Real-Estate: daily tenancy-contract expiry reminder emails (gated OFF by default via
// EnableSettings('ReminderAutoSend'); see PropertyReminderService).
builder.Services.AddHostedService<QuickSoft.Helpers.PropertyReminderService>();

// Security hardening (audit S2, stage 1): the antiforgery token is also accepted via this header, and
// _QuickLayout attaches it to every same-origin jQuery AJAX request. Stage 2 (separate pass) flips on
// global AutoValidateAntiforgeryToken once the token-less legacy forms (mobile App views) are inventoried.
builder.Services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");

// Response compression (Brotli + Gzip) compresses plain .js/.css/json on the fly — replacing the stale
// pre-compressed .gz/.br files the old IIS setup relied on. (.js serves as text/javascript, which isn't in
// the default compressible set, so extend MimeTypes.)
builder.Services.AddResponseCompression(o =>
{
    o.EnableForHttps = true;
    o.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes
        .Concat(new[] { "text/javascript", "image/svg+xml" });
});

var app = builder.Build();

// Wire the System.Web compatibility shim + ad-hoc DbContext connection string.
LegacyWeb.Configure(app.Services.GetRequiredService<IHttpContextAccessor>(), app.Environment);
LegacyWeb.ConnectionString = conn;
LegacyWeb.Config = app.Configuration;

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Security hardening (audit S3): baseline security headers on every response.
// (CSP is deliberately omitted — the legacy views are inline-script heavy; revisit in the UI phase.)
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    // Clickjacking defense (modern replacement for the deprecated X-Frame-Options) — frame-ancestors only,
    // so it does NOT block the legacy inline scripts (a full script-src CSP is deferred to the UI phase).
    context.Response.Headers["Content-Security-Policy"] = "frame-ancestors 'self'";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    await next();
});
if (!app.Environment.IsDevelopment())
{
    app.UseHsts(); // takes effect once TLS is enabled at deployment (runbook §5)
    // Go-live HTTPS enforcement: set "Security": { "RequireHttps": true } in appsettings.Production.json
    // once EVERY access path is HTTPS (TLS on Kestrel :8443 or a reverse proxy). Left OFF by default so the
    // current plain-HTTP pilots keep working — cutover needs only this config flag, no code change.
    if (app.Configuration.GetValue<bool>("Security:RequireHttps"))
    {
        app.UseHttpsRedirection();
    }
}

app.UseResponseCompression();
// PWA: serve the web manifest with its correct MIME type (the default provider doesn't know
// .webmanifest, so without this it would be sent as application/octet-stream and ignored by browsers).
var pwaContentTypes = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
pwaContentTypes.Mappings[".webmanifest"] = "application/manifest+json";
// Performance (audit P4): browser-cache static assets so navigations stop re-fetching them.
// Fonts/images rarely change -> 7 days; css/js -> 1 day (theme iterations during UAT only need
// a hard refresh within that window; bump at go-live if desired).
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = pwaContentTypes,
    OnPrepareResponse = ctx =>
    {
        var name = ctx.File.Name.ToLowerInvariant();
        // PWA control files must NOT be long-cached, or service-worker/manifest updates won't reach
        // installed clients. Browsers revalidate these on every load when sent no-cache.
        if (name == "sw.js" || name == "manifest.webmanifest")
        {
            ctx.Context.Response.Headers["Cache-Control"] = "no-cache";
            return;
        }
        var ext = System.IO.Path.GetExtension(ctx.File.Name).ToLowerInvariant();
        var seconds = ext switch
        {
            ".woff" or ".woff2" or ".ttf" or ".eot" or ".png" or ".jpg" or ".jpeg" or ".gif" or ".svg" or ".ico" => 604800, // 7 days
            ".css" or ".js" => 86400,                                                                                      // 1 day
            _ => 3600
        };
        ctx.Context.Response.Headers["Cache-Control"] = $"public,max-age={seconds}";
    }
});
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Bug 6: the FileManager screen (Syncfusion, licensed) is excluded from this build, so the legacy
// "/FileManager/Index" menu link (stored in the DB module list) 404s. Redirect it to the working FileDocument screen.
app.MapGet("/FileManager/Index", context =>
{
    context.Response.Redirect("/FileDocument/Index");
    return System.Threading.Tasks.Task.CompletedTask;
});

// Legacy routes (ported from App_Start/RouteConfig.cs).
// Area route first ({area:exists} only matches registered areas Property/Hr, so it won't shadow main controllers).
app.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(name: "Login", pattern: "login",
    defaults: new { controller = "Users", action = "Login" });
// Legacy "calendar" area alias (calendarAreaRegistration.cs): the area had no controllers of its own —
// "/calendar/taskcalendar/..." resolved to the MAIN taskcalendarController (and is what the time-sheet
// manual view's iframes link to). Constrain it to ONLY the taskcalendar controller: a generic
// "calendar/{controller}/{action}" pattern matched any controller, so any page reached under /calendar/
// became the ambient route and leaked the "calendar/" prefix into every generated link (Url.Action /
// BeginForm / ActionLink) — e.g. POST /calendar/Users/copypermissionAsync -> 404. Pinning the controller
// keeps the iframe URL working while letting all other links fall back to the default (no-prefix) route.
app.MapControllerRoute(name: "calendar_default", pattern: "calendar/taskcalendar/{action=Index}/{id?}",
    defaults: new { controller = "taskcalendar" });
app.MapControllerRoute(name: "DoubleR", pattern: "{controller}/{action}/{id}/{type}",
    defaults: new { controller = "Home", action = "Index", type = "", id = "" });
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

if (app.Environment.IsDevelopment())
{
    // DEV-ONLY: set a known password on a user to exercise authenticated screens during the port
    // (the emirtechlatest DB is a safe non-production copy). Remove before any real deployment.
    app.MapGet("/dev/setpw", async (string user, string pw, UserManager<ApplicationUser> um) =>
    {
        var u = await um.FindByNameAsync(user);
        if (u == null) return Results.NotFound($"no user '{user}'");
        var token = await um.GeneratePasswordResetTokenAsync(u);
        var r = await um.ResetPasswordAsync(u, token, pw);
        return r.Succeeded
            ? Results.Ok($"password set for {user} (id={u.Id})")
            : Results.BadRequest(string.Join("; ", r.Errors.Select(e => e.Description)));
    });

    // DEV-ONLY: model-vs-schema diff — finds mapped entity properties whose column is ABSENT from the
    // emirtechlatest schema (would crash on INSERT/UPDATE) + entities whose table is missing. JSON. Remove before deploy.
    app.MapGet("/dev/schemadiff", (ApplicationDbContext db) =>
    {
        var issues = new System.Collections.Generic.List<object>();
        var conn = db.Database.GetDbConnection();
        var wasClosed = conn.State != System.Data.ConnectionState.Open;
        if (wasClosed) conn.Open();
        try
        {
            foreach (var et in db.Model.GetEntityTypes())
            {
                var table = et.GetTableName();
                if (string.IsNullOrEmpty(table)) continue;
                var actual = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('" + table + "')";
                    try { using var r = cmd.ExecuteReader(); while (r.Read()) actual.Add(r.GetString(0)); } catch { }
                }
                if (actual.Count == 0) { issues.Add(new { table, issue = "MISSING_TABLE" }); continue; }
                foreach (var prop in et.GetProperties())
                {
                    var col = prop.GetColumnName();
                    if (!string.IsNullOrEmpty(col) && !actual.Contains(col))
                        issues.Add(new { table, column = col, prop = prop.Name, issue = "PHANTOM_COLUMN" });
                }
            }
        }
        finally { if (wasClosed) conn.Close(); }
        return Results.Json(issues);
    });

    // DEV-ONLY: invoke the forward-correctness header-recompute directly (golden-tests the helper
    // without driving the full multi-request save flow). Remove before deploy.
    app.MapGet("/dev/recompute", (string type, long id, ApplicationDbContext db) =>
    {
        if (type == "sale") QuickSoft.Helpers.DocumentTotals.RecomputeSalesEntry(db, id);
        else if (type == "quot") QuickSoft.Helpers.DocumentTotals.RecomputeQuotation(db, id);
        else return Results.BadRequest("type must be sale|quot");
        return Results.Ok($"recomputed {type} {id}");
    });

    // DEV-ONLY: compile every .cshtml through the RUNTIME Razor compiler (the production code path) and
    // report any view that fails — proves complete view-migration without single-assembly precompilation
    // (which dies on CS8103: the 2,000+ large legacy views exceed the per-assembly user-string limit).
    // Optional ?prefix=/Views/A filters the set so the scan can be sharded. Remove before deploy.
    app.MapGet("/dev/compileviews", (string prefix, Microsoft.AspNetCore.Mvc.Razor.IRazorViewEngine engine, IWebHostEnvironment env) =>
    {
        var root = env.ContentRootPath;
        var fails = new System.Collections.Generic.List<object>(); int ok = 0, total = 0;
        foreach (var dir in new[] { "Views", "Areas" })
        {
            var basePath = System.IO.Path.Combine(root, dir);
            if (!System.IO.Directory.Exists(basePath)) continue;
            foreach (var f in System.IO.Directory.EnumerateFiles(basePath, "*.cshtml", System.IO.SearchOption.AllDirectories))
            {
                var rel = "/" + System.IO.Path.GetRelativePath(root, f).Replace('\\', '/');
                if (rel.StartsWith("/Views/FileManager/")) continue; // Syncfusion screen excluded by design (redirected)
                if (!string.IsNullOrEmpty(prefix) && !rel.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase)) continue;
                total++;
                try
                {
                    var page = engine.GetPage("/", rel); // forces runtime compilation of this file
                    if (page.Page != null) ok++;
                    else fails.Add(new { view = rel, error = "view engine did not resolve the page" });
                }
                catch (System.Exception ex)
                {
                    var msg = ex.Message;
                    fails.Add(new { view = rel, error = msg.Length > 400 ? msg.Substring(0, 400) : msg });
                }
            }
        }
        return Results.Json(new { total, ok, failed = fails.Count, fails });
    });
}

app.Run();
