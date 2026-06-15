using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace QuickSoft.Web
{
    // MVC5 bound empty/missing form fields to "" for string params/properties; ASP.NET Core converts empty
    // strings to null by default (ConvertEmptyStringToNull = true). The legacy code is full of `if (x != "")`
    // guards (1500+), which under Core pass for a null x and then x.Parse()/x.ToUpper()/etc. throw — so every
    // date-filtered list/report 500s (and degrades to an empty table) when no filter is set. Restoring the
    // MVC5 behavior globally — empty form values bind to "" instead of null — fixes the whole class at once.
    public class KeepEmptyStringMetadataProvider : IDisplayMetadataProvider
    {
        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            if (context.Key.ModelType == typeof(string))
                context.DisplayMetadata.ConvertEmptyStringToNull = false;
        }
    }
}
