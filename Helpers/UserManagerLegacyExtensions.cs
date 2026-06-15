using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace QuickSoft.Models
{
    // OWIN ASP.NET Identity 2 exposed string-userId overloads on UserManager (AddToRole(userId, role),
    // GetRoles(userId), IsInRole(userId, role), ...). ASP.NET Core Identity only has TUser-object
    // overloads. These extension shims re-add the string-userId API so the legacy controllers port
    // unchanged. They only bind when the first argument is a string (the user-object instance methods
    // win otherwise), so there is no ambiguity.
    public static class UserManagerLegacyExtensions
    {
        private static ApplicationUser U(UserManager<ApplicationUser> um, string userId)
            => um.FindByIdAsync(userId).Result;

        // async (Task-returning) string overloads
        public static Task<IList<string>> GetRolesAsync(this UserManager<ApplicationUser> um, string userId)
            => um.GetRolesAsync(U(um, userId));
        public static Task<IdentityResult> AddToRoleAsync(this UserManager<ApplicationUser> um, string userId, string role)
            => um.AddToRoleAsync(U(um, userId), role);
        public static Task<IdentityResult> AddToRolesAsync(this UserManager<ApplicationUser> um, string userId, IEnumerable<string> roles)
            => um.AddToRolesAsync(U(um, userId), roles);
        public static Task<IdentityResult> RemoveFromRoleAsync(this UserManager<ApplicationUser> um, string userId, string role)
            => um.RemoveFromRoleAsync(U(um, userId), role);
        public static Task<IdentityResult> RemoveFromRolesAsync(this UserManager<ApplicationUser> um, string userId, IEnumerable<string> roles)
            => um.RemoveFromRolesAsync(U(um, userId), roles);
        public static Task<bool> IsInRoleAsync(this UserManager<ApplicationUser> um, string userId, string role)
            => um.IsInRoleAsync(U(um, userId), role);

        // sync OWIN-style string overloads
        public static IList<string> GetRoles(this UserManager<ApplicationUser> um, string userId)
            => um.GetRolesAsync(U(um, userId)).Result;
        public static IdentityResult AddToRole(this UserManager<ApplicationUser> um, string userId, string role)
            => um.AddToRoleAsync(U(um, userId), role).Result;
        public static IdentityResult AddToRoles(this UserManager<ApplicationUser> um, string userId, IEnumerable<string> roles)
            => um.AddToRolesAsync(U(um, userId), roles).Result;
        public static IdentityResult RemoveFromRole(this UserManager<ApplicationUser> um, string userId, string role)
            => um.RemoveFromRoleAsync(U(um, userId), role).Result;
        public static IdentityResult RemoveFromRoles(this UserManager<ApplicationUser> um, string userId, IEnumerable<string> roles)
            => um.RemoveFromRolesAsync(U(um, userId), roles).Result;
        public static bool IsInRole(this UserManager<ApplicationUser> um, string userId, string role)
            => um.IsInRoleAsync(U(um, userId), role).Result;

        // OWIN string-userId overloads for the password/security-stamp APIs.
        public static Task<IdentityResult> ChangePasswordAsync(this UserManager<ApplicationUser> um, string userId, string currentPassword, string newPassword)
            => um.ChangePasswordAsync(U(um, userId), currentPassword, newPassword);
        public static Task<IdentityResult> UpdateSecurityStampAsync(this UserManager<ApplicationUser> um, string userId)
            => um.UpdateSecurityStampAsync(U(um, userId));
        public static Task<string> GeneratePasswordResetTokenAsync(this UserManager<ApplicationUser> um, string userId)
            => um.GeneratePasswordResetTokenAsync(U(um, userId));
        public static Task<bool> IsEmailConfirmedAsync(this UserManager<ApplicationUser> um, string userId)
            => um.IsEmailConfirmedAsync(U(um, userId));
        public static Task<IdentityResult> ResetPasswordAsync(this UserManager<ApplicationUser> um, string userId, string token, string newPassword)
            => um.ResetPasswordAsync(U(um, userId), token, newPassword);

        // OWIN UserManager.SendEmailAsync(userId, subject, body) had email wired into the manager;
        // Core sends mail separately. Best-effort send via the legacy SendMail path.
        public static Task SendEmailAsync(this UserManager<ApplicationUser> um, string userId, string subject, string body)
        {
            try
            {
                var user = U(um, userId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    var msg = new System.Net.Mail.MailMessage { Subject = subject, Body = body, IsBodyHtml = true };
                    new SendMail().sendMail(user.Email, null, msg);
                }
            }
            catch { /* best-effort */ }
            return Task.CompletedTask;
        }
    }
}
