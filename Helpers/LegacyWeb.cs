using System.IO;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace QuickSoft.Models
{
    // Minimal System.Web compatibility shim for the faithful port.
    // Wired at startup via Configure(...). Lets the legacy code keep its shape.
    public static class LegacyWeb
    {
        private static IHttpContextAccessor _accessor;
        private static IWebHostEnvironment _env;

        // Connection string for ad-hoc `new ApplicationDbContext()` (the legacy creates contexts everywhere).
        public static string ConnectionString { get; set; }

        // App configuration — used to resolve a connection-string NAME the EF6 way (see ResolveConnection).
        public static IConfiguration Config { get; set; }

        public static void Configure(IHttpContextAccessor accessor, IWebHostEnvironment env)
        {
            _accessor = accessor;
            _env = env;
        }

        // Reproduces EF6 `new DbContext(nameOrConnectionString)` resolution for the Core port.
        // EF Core's UseSqlServer() only accepts a full connection string, so the legacy multi-branch
        // callers — multi-company BalanceSheet, branch ShowRoomItemForecast/Users — that pass a bare
        // connection-string NAME (e.g. "abudhabi") would otherwise throw "Format of the initialization
        // string does not conform to specification starting at index 0". Faithful semantics:
        //   • a value containing '=' is already a connection string -> used as-is
        //   • a bare token is a connection-string NAME -> resolved from config (Web.config
        //     <connectionStrings> became appsettings ConnectionStrings)
        //   • else the EF6 convention: a database of that name on the app's default server/auth
        //     (the branch databases all live on one SQL Server, distinguished by catalog).
        public static string ResolveConnection(string nameOrConnectionString)
        {
            if (string.IsNullOrWhiteSpace(nameOrConnectionString))
                return ConnectionString;
            if (nameOrConnectionString.IndexOf('=') >= 0)
                return nameOrConnectionString;
            var named = Config?.GetConnectionString(nameOrConnectionString);
            if (!string.IsNullOrWhiteSpace(named))
                return named;
            try
            {
                return new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(ConnectionString)
                {
                    InitialCatalog = nameOrConnectionString
                }.ConnectionString;
            }
            catch
            {
                return nameOrConnectionString; // last resort: let SqlClient surface the original error
            }
        }

        // Replaces System.Web HttpContext.Current (Request, User, etc. are Core-compatible).
        public static HttpContext Current => _accessor?.HttpContext;

        // Replaces Server.MapPath("~/x") -> physical path under the web/content root.
        public static string MapPath(string virtualPath)
        {
            var root = _env?.WebRootPath ?? _env?.ContentRootPath ?? Directory.GetCurrentDirectory();
            if (string.IsNullOrEmpty(virtualPath)) return root;
            var rel = virtualPath.Replace("~/", string.Empty).Replace("~", string.Empty)
                                 .Replace('/', Path.DirectorySeparatorChar)
                                 .TrimStart(Path.DirectorySeparatorChar);
            return Path.Combine(root, rel);
        }
    }

    // Minimal System.Web HttpCookie shim (reads: implicit from the string Request.Cookies[..] returns in Core).
    public class HttpCookie
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public System.DateTime Expires { get; set; }
        public bool Secure { get; set; }
        public bool HttpOnly { get; set; }
        public string Path { get; set; } = "/";
        public string Domain { get; set; }
        public HttpCookie() { }
        public HttpCookie(string name) { Name = name; }
        public HttpCookie(string name, string value) { Name = name; Value = value; }
        public static implicit operator HttpCookie(string v) => v == null ? null : new HttpCookie { Value = v };
    }

    // Minimal OWIN IAuthenticationManager shim — legacy code only calls SignOut(...).
    public class LegacyAuthManager
    {
        private readonly HttpContext _ctx;
        public LegacyAuthManager(HttpContext ctx) { _ctx = ctx; }
        public void SignOut(params object[] authenticationTypes)
        {
            _ctx?.SignOutAsync().GetAwaiter().GetResult();
        }
    }

    // Minimal System.Web.UI.HtmlTextWriter shim (wraps a TextWriter).
    public class HtmlTextWriter : TextWriter
    {
        private readonly TextWriter _inner;
        public HtmlTextWriter(TextWriter inner) { _inner = inner; }
        public override Encoding Encoding => _inner.Encoding;
        public override void Write(char value) => _inner.Write(value);
        public override void Write(string value) => _inner.Write(value);
        public override string ToString() => _inner.ToString();
    }
}
