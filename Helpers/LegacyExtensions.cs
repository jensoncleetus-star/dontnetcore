using System.IO;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    // Compatibility shims so the legacy code ports with minimal edits.
    public static class LegacyExtensions
    {
        // Legacy HttpPostedFileBase.SaveAs(path) -> IFormFile.SaveAs(path)
        public static void SaveAs(this IFormFile file, string path)
        {
            if (file == null) return;
            using var stream = new FileStream(path, FileMode.Create);
            file.CopyTo(stream);
        }

        // Legacy ASP.NET Identity IIdentity.GetUserId() -> NameIdentifier claim.
        public static string GetUserId(this IIdentity identity)
        {
            var ci = identity as ClaimsIdentity;
            return ci?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        // Legacy Request.IsAjaxRequest()
        public static bool IsAjaxRequest(this HttpRequest request)
            => request != null && request.Headers["X-Requested-With"].ToString() == "XMLHttpRequest";

        // Legacy Response.SetCookie(HttpCookie)
        public static void SetCookie(this HttpResponse response, HttpCookie cookie)
        {
            if (response == null || cookie == null) return;
            var opts = new CookieOptions { Path = cookie.Path ?? "/", Secure = cookie.Secure, HttpOnly = cookie.HttpOnly };
            if (cookie.Expires != default) opts.Expires = cookie.Expires;
            if (!string.IsNullOrEmpty(cookie.Domain)) opts.Domain = cookie.Domain;
            response.Cookies.Append(cookie.Name ?? string.Empty, cookie.Value ?? string.Empty, opts);
        }

        // Legacy MVC5 FormCollection.GetValues(key) -> string[] (Core IFormCollection has only an indexer).
        // Used by every server-side DataTables endpoint (Request.Form.GetValues("draw") etc.).
        public static string[] GetValues(this IFormCollection form, string key)
        {
            if (form == null) return null;
            var v = form[key];
            return v.Count == 0 ? null : (string[])v;
        }

        // Legacy MVC5 Request.UrlReferrer (Uri) -> Core has only the Referer header (string).
        public static System.Uri GetUrlReferrer(this HttpRequest request)
        {
            var v = request?.Headers["Referer"].ToString();
            return string.IsNullOrEmpty(v) ? null : new System.Uri(v);
        }
    }
}
