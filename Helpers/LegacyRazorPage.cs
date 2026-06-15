using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Razor;
using QuickSoft.Models;

namespace QuickSoft.Web
{
    // MVC5 Razor views could access Session["x"], Server.MapPath(...), Request.*, IsPost directly. ASP.NET
    // Core's RazorPage does not expose those. These custom base pages re-add them so the legacy .cshtml
    // views port unchanged. Wired globally via Views/_ViewImports.cshtml (@inherits).
    public abstract class LegacyRazorPage<TModel> : RazorPage<TModel>
    {
        public LegacySession Session => new LegacySession(Context.Session);
        public LegacyServer Server => new LegacyServer();
        public LegacyRequest Request => new LegacyRequest(Context);
        public bool IsPost => HttpMethods.IsPost(Context.Request.Method);
    }

    public abstract class LegacyRazorPage : RazorPage
    {
        public LegacySession Session => new LegacySession(Context.Session);
        public LegacyServer Server => new LegacyServer();
        public LegacyRequest Request => new LegacyRequest(Context);
        public bool IsPost => HttpMethods.IsPost(Context.Request.Method);
    }

    // Minimal MVC5 HttpRequestBase shim for the handful of view usages (Request.Url, .IsAuthenticated, ...).
    public class LegacyRequest
    {
        private readonly HttpContext _ctx;
        public LegacyRequest(HttpContext ctx) { _ctx = ctx; }
        public bool IsAuthenticated => _ctx.User?.Identity?.IsAuthenticated ?? false;
        public Uri Url => new Uri(_ctx.Request.GetDisplayUrl());
        public string RawUrl => _ctx.Request.GetEncodedPathAndQuery();
        public bool IsAjaxRequest() => _ctx.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        public string this[string key]
            => _ctx.Request.Query[key].FirstOrDefault()
               ?? (_ctx.Request.HasFormContentType ? _ctx.Request.Form[key].FirstOrDefault() : null);
    }
}
