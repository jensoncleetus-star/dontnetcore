using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CustomHtml
{
    public static class CustHtml
    {
        public static string IsActive(this IHtmlHelper html, string control, string action)
        {
            var routeData = html.ViewContext.RouteData;

            var routeAction = (string)routeData.Values["action"];
            var routeControl = (string)routeData.Values["controller"];

            // both must match
            var returnActive = control == routeControl && action == routeAction;

            return returnActive ? "active" : "";
        }

        public static string CheckActive(string first, string second, string type=null)
        {
            string result;
            var returnActive = first == second;
            result = returnActive ? "active" : "";
            result = (type == "class") ? "class=" + result : result;
            return result;
        }


        // List Country to list
        public static List<string> Country()
        {
            List<string> CountryList = new List<string>();
            CultureInfo[] CInfoList = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            foreach (CultureInfo CInfo in CInfoList)
            {
                RegionInfo R = new RegionInfo(CInfo.LCID);
                if (!(CountryList.Contains(R.EnglishName)))
                {
                    CountryList.Add(R.EnglishName);
                }
            }
            CountryList.Sort();
            return CountryList;
        }

        // month name 

        public static string MonthName(string month)
        {
            int monthno = Convert.ToInt32(month);
            return month = DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(monthno);
        }
    }
}
//Verify the namespace is added to Web.Config namespace section as we did before for Extension Method approach.
//<add namespace=”CustomHtml” />
//we can simply use the CustomTextBox in our View as follows:
//CustHtml.metord(“string1”, “string2”)