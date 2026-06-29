using Microsoft.AspNetCore.Identity;

namespace QuickSoftPilot.Models
{
    // Mirrors the legacy QuickSoft.Models.ApplicationUser: extra profile fields on the Identity user.
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public long BranchID { get; set; }
    }
}
