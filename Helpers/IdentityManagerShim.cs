using Microsoft.AspNetCore.Identity;

namespace QuickSoft.Models
{
    // Minimal port of the legacy Models/IdentityManager.cs (excluded from build). Only the members the
    // controllers actually call are implemented, on top of the Core RoleManager (built via LegacyIdentity).
    public class IdentityManager
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public bool RoleExists(string name)
            => LegacyIdentity.RoleManager(db).RoleExistsAsync(name).Result;

        public bool CreateRole(AppModules role)
            => LegacyIdentity.RoleManager(db).CreateAsync(role).Result.Succeeded;
    }
}
