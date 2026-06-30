using System.IO;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Models
{
    // Compatibility shims so the legacy code ports with minimal edits.
    public static class LegacyExtensions
    {
        // Security (audit S14): block executable/script/markup uploads from ever landing on disk. This is the
        // single choke point every `file.SaveAs(...)` upload site routes through, so the guard covers all ~67
        // handlers at once (defense-in-depth alongside the /uploads serving block in Program.cs). Blocklist (not
        // allowlist) so legitimate business types — images, pdf, office, csv — keep working.
        private static readonly System.Collections.Generic.HashSet<string> BlockedUploadExtensions =
            new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
            { ".html", ".htm", ".xhtml", ".shtml", ".svg", ".js", ".mjs", ".xml", ".cshtml", ".razor", ".aspx",
              ".asp", ".ascx", ".php", ".phtml", ".jsp", ".htaccess", ".swf", ".exe", ".dll", ".bat", ".cmd",
              ".com", ".msi", ".ps1", ".vbs", ".vbe", ".wsf", ".hta", ".jar", ".sh" };

        // Legacy HttpPostedFileBase.SaveAs(path) -> IFormFile.SaveAs(path)
        public static void SaveAs(this IFormFile file, string path)
        {
            if (file == null) return;
            var ext = Path.GetExtension(path);
            if (!string.IsNullOrEmpty(ext) && BlockedUploadExtensions.Contains(ext))
                throw new System.InvalidOperationException($"File type '{ext}' is not allowed.");
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
