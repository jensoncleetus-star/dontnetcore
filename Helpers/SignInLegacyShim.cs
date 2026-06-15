using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace QuickSoft.Models
{
    // OWIN ASP.NET Identity 2 returned a SignInStatus enum from PasswordSignInAsync/TwoFactorSignInAsync;
    // ASP.NET Core returns Microsoft.AspNetCore.Identity.SignInResult. This shim re-creates the enum and
    // a mapper so the legacy switch (result) { case SignInStatus.Success: ... } code ports unchanged.
    public enum SignInStatus { Success, LockedOut, RequiresVerification, Failure }

    public static class SignInLegacyExtensions
    {
        public static SignInStatus ToSignInStatus(this SignInResult r)
        {
            if (r == null) return SignInStatus.Failure;
            if (r.Succeeded) return SignInStatus.Success;
            if (r.IsLockedOut) return SignInStatus.LockedOut;
            if (r.RequiresTwoFactor) return SignInStatus.RequiresVerification;
            return SignInStatus.Failure;
        }

        // OWIN SignInManager.HasBeenVerifiedAsync() -> Core has GetTwoFactorAuthenticationUserAsync().
        public static async Task<bool> HasBeenVerifiedAsync(this SignInManager<ApplicationUser> sm)
            => (await sm.GetTwoFactorAuthenticationUserAsync()) != null;
    }
}
