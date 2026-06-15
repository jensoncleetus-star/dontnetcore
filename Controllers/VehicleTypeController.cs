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
    public class VehicleTypeController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public VehicleTypeController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: VehicleType
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Create([Bind("VTypeId,Type")] VehicleType Vehicle)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.VehicleTypes.Any(c => c.Type == Vehicle.Type);
                if (Exists)
                {
                    msg = "Details already exists.";
                    stat = false;
                }
                else
                {
                    db.VehicleTypes.Add(Vehicle);
                    db.SaveChanges();
                    Id = Vehicle.VTypeId;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "VehicleType", "VehicleTypes", findip(), Vehicle.VTypeId, "Vehicle Types Added Successfully");

                    msg = "Vehicle Types added successfully.";
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
        [HttpPost]
        public ActionResult GetVehicleType(string ColName)
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
            var uEdit = User.IsInRole("Edit Vehicle Rejection");
            var uDelete = User.IsInRole("Delete Vehicle Rejection");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.VehicleTypes
                     where (ColName == "" || a.Type == ColName)
                     select new
                     {

                         a.VTypeId,
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
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VehicleType Vehicle = db.VehicleTypes.Find(id);
            if (Vehicle == null)
            {
                return NotFound();
            }
            return PartialView(Vehicle);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public JsonResult Edit([Bind("VTypeId,Type")] VehicleType Vehicle)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.VehicleTypes.Any(c => c.Type == Vehicle.Type && c.VTypeId != Vehicle.VTypeId);
                if (Exists)
                {
                    msg = "Vehicle Type already exists.";
                    stat = false;
                }
                else
                {
                    db.Entry(Vehicle).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "VehicleType", "VehicleTypes", findip(), Vehicle.VTypeId, "Vehicle Type Details Updated Successfully");


                    msg = "Successfully updated Vehicle Type details.";
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
            VehicleType Vehicle = db.VehicleTypes.Find(id);
            if (Vehicle == null)
            {
                return NotFound();
            }
            return PartialView(Vehicle);
        }
        [HttpPost]
        // [QkAuthorize(Roles = "Dev,Delete VehicleType")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;

            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " VehicleType, Unable to Delete " + notdel + " VehicleType. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + "VehicleType.", true);
            }
            else
            {
                Success("Deleted " + count + " VehicleType.", true);
            }
            return RedirectToAction("Index", "VehicleType");
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //  [QkAuthorize(Roles = "Dev,Delete Vehicle Type")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var Exists = db.VehicleTypes.Any(c => c.VTypeId == id);
            if (Exists)
            {
               
                stat = DeleteFn(id);
                msg = "Successfully deleted VehicleType.";
            }
            else
            {
                msg = "Unable to delete Type.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public bool DeleteFn(long id)
        {
            VehicleType Vehicle = db.VehicleTypes.Find(id);
            if (Vehicle != null)
            {
                db.VehicleTypes.RemoveRange(db.VehicleTypes.Where(a => a.VTypeId == id));


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "VehicleType", "VehicleTypes", findip(), Vehicle.VTypeId, "Vehicle Type Deleted Successfully");
                db.SaveChanges();
            }
            return true;
        }
    }
}