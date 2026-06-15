using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace QuickSoft.Web
{
    // MVC5's SelectList resolved the dataValueField/dataTextField by CASE-INSENSITIVE property lookup;
    // ASP.NET Core's SelectList is case-sensitive, so legacy calls like new SelectList(items, "Id", "Name")
    // against an anonymous type with property "ID" fail lazily at render -> NullReferenceException in the
    // view. QkSelect.List re-creates the items with case-insensitive field resolution and returns a real
    // SelectList over plain Value/Text SelectListItems (which always match), so every legacy call works.
    // The controllers' `new SelectList(...)` calls are rewritten to `QkSelect.List(...)`.
    public static class QkSelect
    {
        // new SelectList(items)  — items are already SelectListItem or simple values; no field resolution.
        public static SelectList List(IEnumerable items) => new SelectList(items ?? new List<object>());

        // new SelectList(items, selectedValue)  — no field names, no case issue; delegate.
        public static SelectList List(IEnumerable items, object selectedValue) => new SelectList(items ?? new List<object>(), selectedValue);

        // new SelectList(items, dataValueField, dataTextField)
        public static SelectList List(IEnumerable items, string dataValueField, string dataTextField)
            => Build(items, dataValueField, dataTextField, null);

        // new SelectList(items, dataValueField, dataTextField, selectedValue)
        public static SelectList List(IEnumerable items, string dataValueField, string dataTextField, object selectedValue)
            => Build(items, dataValueField, dataTextField, selectedValue);

        private static SelectList Build(IEnumerable items, string valueField, string textField, object selectedValue)
        {
            var list = new List<SelectListItem>();
            string sel = selectedValue?.ToString();
            if (items != null)
            {
                foreach (var o in items)
                {
                    var v = GetCI(o, valueField);
                    var t = GetCI(o, textField);
                    var vs = v?.ToString();
                    list.Add(new SelectListItem
                    {
                        Value = vs,
                        Text = t?.ToString(),
                        Selected = sel != null && string.Equals(vs, sel, StringComparison.Ordinal)
                    });
                }
            }
            return new SelectList(list, "Value", "Text");
        }

        private static object GetCI(object o, string name)
        {
            if (o == null || string.IsNullOrEmpty(name)) return null;
            var p = o.GetType().GetProperty(name,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return p?.GetValue(o);
        }
    }
}
