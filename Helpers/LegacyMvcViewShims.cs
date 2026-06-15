using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace QuickSoft.Web
{
    // MVC5 had Html.Label(expression, object htmlAttributes); ASP.NET Core only has Label(expr, string
    // labelText, object htmlAttributes), so a 2-arg (string, anonymous-object) Label call fails (CS7036).
    // This re-adds it. NOTE: do NOT add LabelFor(expr, object) — Core already has it natively; a duplicate
    // makes every LabelFor(expr, new{}) call ambiguous (CS0121).
    public static class LegacyLabelHelper
    {
        public static IHtmlContent Label(this IHtmlHelper html, string expression, object htmlAttributes)
            => html.Label(expression, null, htmlAttributes);
    }

    // MVC5 @EnumHelper.GetSelectList(typeof(SomeEnum)) -> dropdown items. (Core's EnumHelper was removed.)
    public static class EnumHelper
    {
        public static List<SelectListItem> GetSelectList(Type enumType)
        {
            var t = Nullable.GetUnderlyingType(enumType) ?? enumType;
            var list = new List<SelectListItem>();
            foreach (var v in Enum.GetValues(t))
                list.Add(new SelectListItem { Text = v.ToString(), Value = Convert.ToInt32(v).ToString() });
            return list;
        }
    }

    // MVC5 @Json.Encode(obj) -> JSON string. Core's @Json (IJsonHelper) has Serialize (IHtmlContent) but
    // no Encode; legacy code does @Html.Raw(Json.Encode(x)) expecting a string, so return a string.
    public static class JsonHelperLegacyExtensions
    {
        public static string Encode(this IJsonHelper json, object value)
            => Newtonsoft.Json.JsonConvert.SerializeObject(value);
    }

    // MVC5 Html.BeginForm(action, controller, routeValues, FormMethod, object htmlAttributes) — Core's
    // nearest overload inserts a `bool? antiforgery` before htmlAttributes, so the legacy 5-arg call fails
    // (CS7036). This re-adds the legacy 5-arg shape.
    public static class LegacyFormHelper
    {
        public static MvcForm BeginForm(this IHtmlHelper html, string actionName, string controllerName,
            object routeValues, FormMethod method, object htmlAttributes)
            => html.BeginForm(actionName, controllerName, routeValues, method, null, htmlAttributes);
    }
}
