using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace QuickSoft.Models
{
    // OWIN built UserManager/RoleManager via HttpContext.GetOwinContext().GetUserManager<T>() or
    // `new UserManager(new UserStore(db))`. ASP.NET Core's managers have many ctor dependencies and
    // are normally DI-injected. These builders construct a fully-working manager from just the
    // DbContext so the legacy controllers' lazy `_userManager ?? ...` pattern ports unchanged and
    // also works inside constructors (where HttpContext/RequestServices is not yet available).
    public static class LegacyIdentity
    {
        public static UserManager<ApplicationUser> UserManager(ApplicationDbContext db)
        {
            var store = new UserStore<ApplicationUser>(db);
            return new UserManager<ApplicationUser>(
                store,
                Options.Create(new IdentityOptions()),
                new PasswordHasher<ApplicationUser>(),
                new IUserValidator<ApplicationUser>[] { new UserValidator<ApplicationUser>() },
                new IPasswordValidator<ApplicationUser>[] { new PasswordValidator<ApplicationUser>() },
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                null,
                NullLogger<UserManager<ApplicationUser>>.Instance);
        }

        public static RoleManager<IdentityRole> RoleManager(ApplicationDbContext db)
        {
            var store = new RoleStore<IdentityRole>(db);
            return new RoleManager<IdentityRole>(
                store,
                new IRoleValidator<IdentityRole>[] { new RoleValidator<IdentityRole>() },
                new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(),
                NullLogger<RoleManager<IdentityRole>>.Instance);
        }
    }
}
