using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace QuickSoft.Web
{
    // MVC5 had @Html.EnumDropDownListFor(m => m.SomeEnum, htmlAttributes); ASP.NET Core removed it.
    // These extensions re-add it (used in ~270 legacy views) on top of Core's GetEnumSelectList +
    // DropDownListFor, so the views render unchanged. Handles enum and Nullable<enum> properties.
    public static class LegacyEnumHelper
    {
        public static IHtmlContent EnumDropDownListFor<TModel, TEnum>(
            this IHtmlHelper<TModel> html, Expression<Func<TModel, TEnum>> expression)
            => html.DropDownListFor(expression, EnumItems<TEnum>(html));

        public static IHtmlContent EnumDropDownListFor<TModel, TEnum>(
            this IHtmlHelper<TModel> html, Expression<Func<TModel, TEnum>> expression, object htmlAttributes)
            => html.DropDownListFor(expression, EnumItems<TEnum>(html), htmlAttributes);

        public static IHtmlContent EnumDropDownListFor<TModel, TEnum>(
            this IHtmlHelper<TModel> html, Expression<Func<TModel, TEnum>> expression, string optionLabel, object htmlAttributes)
            => html.DropDownListFor(expression, EnumItems<TEnum>(html), optionLabel, htmlAttributes);

        private static IEnumerable<SelectListItem> EnumItems<TEnum>(IHtmlHelper html)
        {
            var t = typeof(TEnum);
            var u = Nullable.GetUnderlyingType(t);
            if (u != null) t = u;
            return t.IsEnum ? html.GetEnumSelectList(t) : new List<SelectListItem>();
        }
    }
}
