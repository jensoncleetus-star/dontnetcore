using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace QuickSoft.Helpers
{
    // MVC5 -> ASP.NET Core migration compensation.
    //
    // .NET Core's DataAnnotations ([Phone], [EmailAddress], [Url], [CreditCard], [RegularExpression], ...) report an
    // EMPTY string as INVALID, whereas .NET Framework / MVC5 treated a blank OPTIONAL field as valid (only [Required]
    // rejected blank). This app also uses KeepEmptyStringMetadataProvider so blank form fields bind to "" (not null)
    // — needed for the 1500+ legacy `if (x != "")` guards. The combination means every blank optional contact field
    // (phone/email/website) fails server-side validation and blocks the save, e.g. Supplier/Create with no contact
    // details (and 500s if an action's invalid-ModelState re-render path is fragile).
    //
    // This global filter restores MVC5 behaviour: on each POST, for any field left BLANK, drop its format/type
    // validation errors but KEEP a genuine [Required] error (so a blank required field still fails). Runs after model
    // binding, before the action, so the action sees the corrected ModelState.IsValid.
    public class BlankOptionalFieldValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var modelState = context.ModelState;
            if (modelState.IsValid) return;

            var toRevalidate = new List<string>();
            foreach (var entry in modelState)
            {
                var state = entry.Value;
                if (state.ValidationState != ModelValidationState.Invalid) continue;
                // only fields that were submitted blank/whitespace
                if (!string.IsNullOrWhiteSpace(state.AttemptedValue)) continue;
                // a genuinely required field that is blank must still fail -> leave it invalid
                bool hasRequiredError = state.Errors.Any(e =>
                    e.ErrorMessage != null &&
                    e.ErrorMessage.IndexOf("required", StringComparison.OrdinalIgnoreCase) >= 0);
                if (hasRequiredError) continue;
                toRevalidate.Add(entry.Key);
            }

            // blank + only format/type errors -> treat as valid (MVC5 parity). ClearValidationState resets the entry
            // (Invalid -> Unvalidated) so MarkFieldValid won't throw "A field previously marked invalid should not be
            // marked valid". (Doing this after the loop avoids mutating the dictionary while enumerating it.)
            foreach (var key in toRevalidate)
            {
                modelState.ClearValidationState(key);
                modelState.MarkFieldValid(key);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
