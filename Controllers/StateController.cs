using QuickSoft.Web;
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
    public class StateController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public StateController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GetEmirates(string SizeName)
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
            var uEdit = User.IsInRole("Edit State");
            var uDelete = User.IsInRole("Delete State");


            var v = (from a in db.States
                     join b in db.Country on a.CountryID equals b.CountryID into cont1
                     from b in cont1.DefaultIfEmpty()
                     where (SizeName == null || SizeName == "" || a.StateName == SizeName)
                     select new
                     {
                         a.StateName,
                         b.CountryName,
                         a.StateID,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete


                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.StateName.ToString().ToLower().Contains(search.ToLower()));
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


            var stands = db.Country
                         .Select(s => new
                         {
                             FieldID = s.CountryID,
                             FieldName = s.CountryName
                         })
                          .ToList();
            ViewBag.States = QkSelect.List(stands, "FieldID", "FieldName");

            return PartialView();



        }

        // POST:State/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Create State")]
        public JsonResult Create([Bind("StateID ,StateCode,StateName,CountryID  ")] States Locations)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;

            if (ModelState.IsValid)
            {
                var Exists = db.States.Any(c => c.StateName == Locations.StateName);
                if (Exists)
                {
                    msg = "StateName already exists.";
                    stat = false;
                }
                else
                {
                    db.States.Add(Locations);
                    db.SaveChanges();
                    Id = Locations.StateID;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "States", "States", findip(), Locations.StateID, "States Added Successfully");

                    msg = "States added successfully.";
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
            States Locations = db.States.Find(id);
            if (Locations == null)
            {
                return NotFound();
            }

            var stands = db.Country
                             .Select(s => new
                             {
                                 FieldID = s.CountryID,
                                 FieldName = s.CountryName,
                             })
                              .ToList();
            ViewBag.States = QkSelect.List(stands, "FieldID", "FieldName");

            return PartialView(Locations);
        }



        // POST: State/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Edit States")]
        public JsonResult Edit([Bind("StateID ,StateCode,StateName,CountryID")] States Locations)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.States.Any(c => c.StateName == Locations.StateName);
                if (Exists)
                {
                    msg = "StateName already exists.";
                    stat = false;
                }
                else
                {
                    db.Entry(Locations).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "States", "States", findip(), Locations.StateID, "States/Emirate Updated Successfully");

                    msg = "Successfully updated State details.";
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
            States Locations = db.States.Find(id);
            if (Locations == null)
            {
                return NotFound();
            }
            return PartialView(Locations);
        }

        // POST: ItemCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //  [QkAuthorize(Roles = "Dev,Delete Emirate")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            stat = DeleteFn(id);
            msg = "Successfully deleted Emirate/State.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        // [QkAuthorize(Roles = "Dev,Delete Emirate/State")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Details, Unable to Delete " + notdel + " Emirate/State. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + "  Emirate/State.", true);
            }
            else
            {
                Success("Deleted " + count + " Emirate/State.", true);
            }
            return RedirectToAction("Index", "State");
        }

        public bool DeleteFn(long id)
        {
            States Locations = db.States.Find(id);
            if (Locations != null)
            {
                db.States.RemoveRange(db.States.Where(a => a.StateID == id));


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "States", "States", findip(), Locations.StateID, "Emirate/State Deleted Successfully");
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
