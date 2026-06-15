using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace QuickSoft.Web
{
    // MVC5 had @Html.Action("action","controller", routeValues) to render a child action inline.
    // ASP.NET Core removed child actions (ViewComponents are the replacement). To keep the 100+ legacy
    // @Html.Action call sites unchanged, this shim invokes the target action in-process and captures
    // its rendered output by temporarily swapping the response body (the outer response is still
    // buffered during view render, so this is safe for the simple partial-returning child actions used).
    public static class LegacyHtmlActionExtensions
    {
        public static IHtmlContent Action(this IHtmlHelper html, string actionName, string controllerName, object routeValues = null)
        {
            var ctx = html.ViewContext.HttpContext;
            var provider = ctx.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();
            var descriptor = provider.ActionDescriptors.Items
                .OfType<ControllerActionDescriptor>()
                .FirstOrDefault(d => string.Equals(d.ControllerName, controllerName, System.StringComparison.OrdinalIgnoreCase)
                                  && string.Equals(d.ActionName, actionName, System.StringComparison.OrdinalIgnoreCase));
            if (descriptor == null) return HtmlString.Empty;

            var routeData = new RouteData();
            foreach (var kv in ctx.GetRouteData().Values) routeData.Values[kv.Key] = kv.Value;
            if (routeValues != null)
                foreach (var kv in new RouteValueDictionary(routeValues)) routeData.Values[kv.Key] = kv.Value;
            routeData.Values["controller"] = controllerName;
            routeData.Values["action"] = actionName;

            var invokerFactory = ctx.RequestServices.GetRequiredService<IActionInvokerFactory>();
            var actionContext = new ActionContext(ctx, routeData, descriptor);

            var originalBody = ctx.Response.Body;
            var originalStatus = ctx.Response.StatusCode;
            using var mem = new MemoryStream();
            ctx.Response.Body = mem;
            try
            {
                var invoker = invokerFactory.CreateInvoker(actionContext);
                invoker.InvokeAsync().GetAwaiter().GetResult();
                mem.Position = 0;
                using var reader = new StreamReader(mem);
                return new HtmlString(reader.ReadToEnd());
            }
            catch
            {
                return HtmlString.Empty;
            }
            finally
            {
                // The child action wrote its own status code + Content-Length onto the shared response;
                // restore them so the OUTER page response is not corrupted (Content-Length mismatch).
                ctx.Response.Body = originalBody;
                ctx.Response.StatusCode = originalStatus;
                ctx.Response.Headers.ContentLength = null;
            }
        }

        // @Html.Action(...) is also used in statement position via Html.RenderAction(...) in some views.
        public static void RenderAction(this IHtmlHelper html, string actionName, string controllerName, object routeValues = null)
            => html.ViewContext.Writer.Write(Action(html, actionName, controllerName, routeValues).ToString());
    }
}
