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
    public class VehicleManufacturerController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public VehicleManufacturerController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: VehicleManufacturer
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            var stands = db.VehicleTypes
                       .Select(s => new
                       {
                           VTypeId = s.VTypeId,
                           Type = s.Type
                       })
                        .ToList();
            ViewBag.VehicleType = QkSelect.List(stands, "VTypeId", "Type");

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Create State")]
        public JsonResult Create([Bind("MId,Manufacturer,VTyId")] VehicleManufacturer Vehicle)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;

            if (ModelState.IsValid)
            {

               
                var Exists = db.VehicleManufacturers.Any(c => c.Manufacturer == Vehicle.Manufacturer && c.VTyId==Vehicle.VTyId);
               
                if (Exists)
                {
                        msg = "Manufacturer already exists.";
                        stat = false;
                   

                }
                
                else
                {
                    db.VehicleManufacturers.Add(Vehicle);
                    db.SaveChanges();
                    Id = Vehicle.MId;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "VehicleManufacturer", "VehicleManufacturers", findip(), Vehicle.MId, "Manufacturer Added Successfully");

                    msg = "Manufacturer added successfully.";
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
        public ActionResult GetManufacturer(string ColName)
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


            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.VehicleManufacturers
                     join b in db.VehicleTypes on a.VTyId equals b.VTypeId into cont1
                     from b in cont1.DefaultIfEmpty()
                     where(ColName == null || ColName == "" || a.Manufacturer == ColName)
                     select new
                     {

                         a.MId,
                         a.Manufacturer,
                         b.Type,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.Manufacturer.ToString().ToLower().Contains(search.ToLower()));
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
            VehicleManufacturer Vehicle = db.VehicleManufacturers.Find(id);
            if (Vehicle == null)
            {
                return NotFound();
            }

            var stands = db.VehicleTypes
                       .Select(s => new
                       {
                           VTypeId = s.VTypeId,
                           Type = s.Type
                       })
                        .ToList();
            ViewBag.VehicleType = QkSelect.List(stands, "VTypeId", "Type");

            return PartialView(Vehicle);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Edit([Bind("MId,Manufacturer,VTyId")] VehicleManufacturer Vehicle)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.VehicleManufacturers.Any(c => c.Manufacturer == Vehicle.Manufacturer && c.VTyId == Vehicle.VTyId);
                if (Exists)
                {
                    msg = "Manufacturer already exists.";
                    stat = false;
                }
                else
                {
                    db.Entry(Vehicle).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "VehicleManufacturer", "VehicleManufacturers", findip(), Vehicle.MId, "Manufacturer Updated Successfully");
                    msg = "Successfully updated Manufacturer.";
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
            VehicleManufacturer Vehicle = db.VehicleManufacturers.Find(id);
            if (Vehicle == null)
            {
                return NotFound();
            }
            return PartialView(Vehicle);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var Exists = db.VehicleManufacturers.Any(c => c.MId == id);
            if (Exists)
            {

                stat = DeleteFn(id);
                msg = "Successfully deleted Manufacturer.";
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
            VehicleManufacturer Vehicle = db.VehicleManufacturers.Find(id);
            if (Vehicle != null)
            {
                db.VehicleManufacturers.RemoveRange(db.VehicleManufacturers.Where(a => a.MId == id));


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "VehicleManufacturer", "VehicleManufacturers", findip(), Vehicle.MId, "Vehicle Manufacturer Deleted Successfully");
                db.SaveChanges();
            }
            return true;
        }
        public JsonResult SearchVehicleType(string q, string x)
        {
            List<SelectFormat3> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.VehicleTypes.Where(p => p.Type.ToLower().Contains(q.ToLower()) || p.Type.Contains(q))
                                  .Select(b => new SelectFormat3
                                  {
                                      text = b.Type, //each json object will have 
                                      id = b.VTypeId
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.VehicleTypes.Select(b => new SelectFormat3
                {
                    text = b.Type, //each json object will have 
                    id = b.VTypeId
                }).OrderBy(b => b.text).ToList();

            }//          

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat3() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }

    }
}