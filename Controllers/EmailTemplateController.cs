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
using System;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.Models;
using Microsoft.AspNetCore.Identity;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class EmailTemplateController : BaseController
    {
        // GET: EmailTemplate
        ApplicationDbContext db;
        Common com;
        //Get EmailTemplate From Db
        public EmailTemplateController()
        {
            com = new Common();
            db = new ApplicationDbContext();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Email Template")]
        public ActionResult GetEmailTemplate()
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit Email Template");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = db.EmailTemplates.Select(b => new
            {
                b.EmailTemplateID,
                b.Subject,
                b.Head,
                Dev = uDev,
                Edit = uEdit,
            });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.Head.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Subject.ToString().ToLower().Contains(search.ToLower()) 

                );
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }

            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        [QkAuthorize(Roles = "Dev,Email Template")]
        public ActionResult Index()
        {
            return View();
        }

        //[HttpGet]
        //[QkAuthorize(Roles = "Dev,Create Email Template")]

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[QkAuthorize(Roles = "Dev,Create Email Template")]


        //                Head = Email.Head,
        //                Subject = Email.Subject,
        //                EmailBody = Email.EmailBody



        //        Warning("Looks like something went wrong. Please check your form.", true);


        // GET: EmailTemplate/Edit

        [QkAuthorize(Roles = "Dev,Edit Email Template")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            EmailTemplate Emailinformation = db.EmailTemplates.Find(id);

            if (Emailinformation == null)
            {
                return NotFound();
            }
            return View(Emailinformation);
        }

        // POST: EmailTemplate/Edit
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Email Template")]
        public ActionResult Edit(long id, EmailTemplate EmailT)
        {
            if (ModelState.IsValid)
            {
                var Exists = db.EmailTemplates.Any(c => c.Head == EmailT.Head && c.EmailTemplateID != id);
                if (Exists)
                {
                    Danger("EmailTemplates Head already exists.", true);
                }
                else
                {
                    EmailTemplate Emailtemp = db.EmailTemplates.Find(id);
                    Emailtemp.Head = EmailT.Head;
                    Emailtemp.Subject = EmailT.Subject;
                    Emailtemp.EmailBody = EmailT.EmailBody;
                    db.Entry(Emailtemp).State = EntityState.Modified;
                    db.SaveChanges();

                    var userid = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, userid, "EmailTemplate", "EmailTemplates", findip(), EmailT.EmailTemplateID, "Email Template Updated Successfully");


                    Success("Successfully Updated Email Template Details .", true);
                    return RedirectToAction("Index", "EmailTemplate");
                }
            }
            else
            {
                Warning("Looks like something went wrong. Please check your form.", true);
            }
            return View();
        }

        // GET: EmailTemplate/Delete
        //[QkAuthorize(Roles = "Dev,Delete Email Template")]


        // POST: EmailTemplate/Delete
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Delete Email Template")]
        public ActionResult Delete(long id, IFormCollection collection)
        {

            bool stat = false;
            string msg;
            EmailTemplate Emailinfom = db.EmailTemplates.Find(id);
            db.EmailTemplates.Remove(Emailinfom);
            db.SaveChanges();
            stat = true;

            var userid = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, userid, "EmailTemplate", "EmailTemplates", findip(), Emailinfom.EmailTemplateID, "Email Template Deleted Successfully");


            msg = "Successfully Deleted Email Template details.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

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
