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
    public class ContactTypeController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ContactTypeController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: ContactType
        public ActionResult Index()
        {
            return View();
        }
       
        [HttpPost]
        //   [QkAuthorize(Roles = "Dev,ContactType")]
        public ActionResult GetContactType(string ColName)
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
            var uEdit = User.IsInRole("Edit Contat Type");
            var uDelete = User.IsInRole("Delete Contact Type");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.ContactTypes
                     where (ColName == "" || a.Type == ColName)
                     select new
                     {

                         a.ContactId,
                         a.Type,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.Type.ToString().ToLower().Contains(search.ToLower()));
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

        public ActionResult Create()
        {
            return PartialView();
        }

        // POST: LeadType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Create LeadType")]
        public JsonResult Create([Bind("ContactId,Type")] ContactType Contact)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.ContactTypes.Any(c => c.Type == Contact.Type);
                if (Exists)
                {
                    msg = "Contact Type exists.";
                    stat = false;
                }
                else
                {
                    db.ContactTypes.Add(Contact);
                    db.SaveChanges();
                    Id = Contact.ContactId;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "ContactType", "ContactTypes", findip(), Contact.ContactId, "Contact Types Added Successfully");

                    msg = "Contact Types added successfully.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }

        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ContactType Contact = db.ContactTypes.Find(id);
            if (Contact == null)
            {
                return NotFound();
            }
            return PartialView(Contact);
        }



        // POST: ContactType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public JsonResult Edit([Bind("ContactId,Type")]ContactType Contact)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.ContactTypes.Any(c => c.Type == Contact.Type && c.ContactId != Contact.ContactId);
                if (Exists)
                {
                    msg = "ContactType already exists.";
                    stat = false;
                }
                else
                {
                    db.Entry(Contact).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "ContactType", "ContactTypes", findip(), Contact.ContactId, "ContactType Updated Successfully");


                    msg = "Successfully updated Contact Type .";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ContactType Contact = db.ContactTypes.Find(id);
            if (Contact == null)
            {
                return NotFound();
            }
            return PartialView(Contact);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //  [QkAuthorize(Roles = "Dev,Delete ContactType")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            stat = DeleteFn(id);
            msg = "Successfully deleted ContactType.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        // [QkAuthorize(Roles = "Dev,Delete LeadType")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;

            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " ContactType, Unable to Delete " + notdel + " ContactType. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + "ContactType.", true);
            }
            else
            {
                Success("Deleted " + count + " ContactType.", true);
            }
            return RedirectToAction("Index", "ContactType");
        }


        public bool DeleteFn(long id)
        {
            ContactType Contact = db.ContactTypes.Find(id);
            if (Contact != null)
            {
                db.ContactTypes.RemoveRange(db.ContactTypes.Where(a => a.ContactId == id));

                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "ContactType", "ContactTypes", findip(), Contact.ContactId, "Contact Type Deleted Successfully");
                db.SaveChanges();
            }
            return true;
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





