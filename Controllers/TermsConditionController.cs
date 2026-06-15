using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Http;
using ApplicationUserManager = Microsoft.AspNetCore.Identity.UserManager<QuickSoft.Models.ApplicationUser>;
using ApplicationSignInManager = Microsoft.AspNetCore.Identity.SignInManager<QuickSoft.Models.ApplicationUser>;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.Models;


namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class TermsConditionController : BaseController
    {
        ApplicationDbContext db;

        //Get EmailTemplate From Db
        public TermsConditionController()
        {
            db = new ApplicationDbContext();
        }
        // GET: TermsCondition

        [HttpGet]
        [QkAuthorize(Roles = "Dev,TC Sales Return,TC Purchase,TC Purchase Return,TC Pro Forma,TC DeliveryNote,TC Sales,TC Quotation,TC PurchaseOrder,TC CrNote,TC MR,TC MRNote")]
        public ActionResult Index(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            var Exists = db.TermsAndConditionss.Any(c => c.ConditionTypeID == id);
            if (Exists)
            {
                TermsAndConditions terms = db.TermsAndConditionss.Where(a => a.ConditionTypeID == id).First();
                return View(terms);
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,TC Sales Return,TC Purchase,TC Purchase Return,TC Pro Forma,TC DeliveryNote,TC Sales,TC Quotation,TC PurchaseOrder,TC CrNote,TC MR,TC MRNote")]
        public ActionResult Index(string id,TermsAndConditions term)
        {
            if (ModelState.IsValid)
            {
                var Exists = db.TermsAndConditionss.Any(c => c.ConditionTypeID == id);
                if (Exists)
                {
                    TermsAndConditions terms = db.TermsAndConditionss.Where(a => a.ConditionTypeID == id).First();
                    terms.TermsCondit = term.TermsCondit;
                    db.Entry(terms).State = EntityState.Modified;
                    db.SaveChanges();
                    Success("Successfully Updated Terms and Condition Details .", true);
                    return RedirectToAction("Index", "TermsCondition");
                }
                else
                {
                    TermsAndConditions terms = new TermsAndConditions();
                    terms.ConditionTypeID = id;
                    terms.TermsCondit = term.TermsCondit;
                    db.TermsAndConditionss.Add(terms);
                    db.SaveChanges();
                    Success("Successfully Updated Terms and Condition Details .", true);
                    return RedirectToAction("Index", "TermsCondition");
                }
            }
            else
            {
                Warning("Looks like something went wrong. Please check your form.", true);
                return View();
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}