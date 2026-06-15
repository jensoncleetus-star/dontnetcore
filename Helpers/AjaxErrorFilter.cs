using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuickSoft.Web
{
    // Graceful degradation for AJAX/DataTables endpoints during the port: if a server-side DataTables
    // action throws (e.g. a complex legacy report query not yet ported to EF Core), return an empty
    // DataTables payload instead of a 500 HTML page. Without this, jQuery DataTables shows a blocking
    // "Ajax error" alert on the dashboard for a single un-ported widget. The real exception is written
    // to stderr so these can be found and the underlying query fixed (the per-screen report tail).
    // Non-AJAX requests are untouched (they still get the normal/developer error page).
    public class AjaxErrorFilter : IAsyncExceptionFilter
    {
        public Task OnExceptionAsync(ExceptionContext context)
        {
            var req = context.HttpContext.Request;
            bool isAjax = string.Equals(req.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
            if (isAjax)
            {
                Console.Error.WriteLine("[AjaxError] " + req.Method + " " + req.Path + req.QueryString +
                                        " -> " + context.Exception.GetType().Name + ": " + context.Exception.Message +
                                        "\n" + context.Exception.StackTrace);
                // DataTables reads { data: [...] } and renders an empty table for an empty array. Do NOT
                // include an "error" property — DataTables shows any "error" field as a blocking warning.
                // (The real exception is logged above for fixing the underlying query.)
                context.Result = new JsonResult(new { data = Array.Empty<object>(), recordsTotal = 0, recordsFiltered = 0 });
                context.ExceptionHandled = true;
            }
            return Task.CompletedTask;
        }
    }
}
