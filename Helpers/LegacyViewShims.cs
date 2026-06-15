using System.Security.Principal;

// Shims for legacy namespaces/types referenced by .cshtml views (surfaced by build-time Razor
// compilation). Views are runtime-compiled, so these only appear when a view is actually rendered;
// providing the legacy namespaces lets the views compile unchanged (faithful port).

namespace System.Web.Mvc
{
    // Legacy MVC5 error views use `@model System.Web.Mvc.HandleErrorInfo` (Views/Shared/Error.cshtml,
    // Shared/Lockout.cshtml, Error/Index.cshtml). ASP.NET Core has no System.Web.Mvc — provide the POCO.
    public class HandleErrorInfo
    {
        public HandleErrorInfo() { }
        public HandleErrorInfo(System.Exception exception, string controllerName, string actionName)
        {
            Exception = exception;
            ControllerName = controllerName;
            ActionName = actionName;
        }
        public System.Exception Exception { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
    }
}

namespace System.Web.Script.Serialization
{
    // Marker so legacy `@using System.Web.Script.Serialization;` resolves (JournalV/Create, JournalV/Edit,
    // Areas/Property/PJournalV/Edit). The actual JavaScriptSerializer shim lives in QuickSoft.Models
    // (Helpers/LegacyShims.cs) and is reached via _ViewImports — so `new JavaScriptSerializer()` is
    // unambiguous and this namespace only needs to exist.
    internal static class ShimMarker { }
}

namespace Microsoft.AspNet.Identity
{
    // Legacy `@using Microsoft.AspNet.Identity` + `User.Identity.GetUserName()` (Views/Shared/_LoginPartial).
    // NOTE: GetUserId already exists in QuickSoft.Models.LegacyExtensions (always imported via _ViewImports);
    // do NOT redefine it here or every GetUserId() call site becomes ambiguous (CS0121).
    public static class IdentityExtensions
    {
        public static string GetUserName(this IIdentity identity) => identity?.Name;
    }
}
