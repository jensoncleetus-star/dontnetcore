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
using System.Collections.Generic;

namespace QuickSoft.Controllers
{
    public class routemapController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public routemapController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: routemap
        public ActionResult Index()
        {
          
            return View();
        }
        [HttpPost]
        public ActionResult GetLocation(string SizeName)
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
            var uEdit = User.IsInRole("Edit Location");
            var uDelete = User.IsInRole("Delete Location");


            var v = (from a in db.routemap
                     join b in db.LocationNames on a.locationid equals b.LocationId into cont1
                     from b in cont1.DefaultIfEmpty()
                     join c in db.LocationNames on a.nearestlocationid equals c.LocationId into cont12
                     from c in cont12.DefaultIfEmpty()
                    // where (SizeName == null || SizeName == "" || a.Location == SizeName)
                     select new
                     {
                         route=b.Location,
                         locationnext=c.Location,
                         a.locationorder,
                         a.routeid,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete


                     }); 

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.locationnext.ToString().ToLower().Contains(search.ToLower()));
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

          
                var stands = db.LocationNames
                             .Select(s => new
                             {
                                 FieldID = s.LocationId,
                                 FieldName = s.Location
                             })
                              .ToList();
                ViewBag.States = QkSelect.List(stands,  "FieldID", "FieldName");

                return PartialView();



        }

        // POST:Location/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Create Item Size")]
        public JsonResult Create(routemap rts)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;

            if (ModelState.IsValid)
            {


                db.routemap.Add(rts);
                db.SaveChanges();
                Id = rts.routeid;

                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Created, UserId, "Routemap", "RouteMap", findip(), rts.routeid, "Route Added Successfully");

                msg = "Route added successfully.";
                stat = true;

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
            LocationName Locations = db.LocationNames.Find(id);
            if (Locations == null)
            {
                return NotFound();
            }

            var stands = db.States
                             .Select(s => new
                             {
                                 FieldID = s.StateID,
                                 FieldName = s.StateName
                             })
                              .ToList();
            ViewBag.States = QkSelect.List(stands, "FieldID", "FieldName");

            return PartialView(Locations);
        }



        // POST: ItemCategory/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Edit Item Size")]
        public JsonResult Edit([Bind("LocationId,Location,StateID")] LocationName Locations)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                
                
                    db.Entry(Locations).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "LocationName", "LocationNames", findip(), Locations.LocationId, "Location Updated Successfully");


                    msg = "Successfully updated Location details.";
                    stat = true;
               
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
            LocationName Locations = db.LocationNames.Find(id);
            if (Locations == null)
            {
                return NotFound();
            }
            return PartialView(Locations);
        }

        // POST: ItemCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //  [QkAuthorize(Roles = "Dev,Delete Item Color")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var Exists = db.Customers.Any(c => c.LocationID == id);
            if (Exists)
            {
                msg = "Unable to delete Type, Customer with this Location exists.";
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Successfully deleted Location.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        // [QkAuthorize(Roles = "Dev,Delete Item Color")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Details, Unable to Delete " + notdel + " Details. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Details.", true);
            }
            else
            {
                Success("Deleted " + count + " Details.", true);
            }
            return RedirectToAction("Index", "Location");
        }

        public bool DeleteFn(long id)
        {
            LocationName Locations = db.LocationNames.Find(id);
            if (Locations != null)
            {
                db.LocationNames.RemoveRange(db.LocationNames.Where(a => a.LocationId == id));


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "LocationName", "LocationNames", findip(), Locations.LocationId, "Location Deleted Successfully");
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
