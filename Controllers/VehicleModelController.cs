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
    public class VehicleModelController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public VehicleModelController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: VehicleModel
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GetModel(string ColName)
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
            var v = (from a in db.VehicleModels
                     join b in db.VehicleManufacturers on a.MaId equals b.MId into cont1
                     from b in cont1.DefaultIfEmpty()
                     join c in db.VehicleTypes on a.VTId equals c.VTypeId into cont2
                     from c in cont2.DefaultIfEmpty()
                     where (ColName == null || ColName == "" || a.Model == ColName)
                     select new
                     {

                         a.ModelId,
                         a.Model,
                         b.Manufacturer,
                         c.Type,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.Model.ToString().ToLower().Contains(search.ToLower()));
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
            var stand1 = db.VehicleTypes
                       .Select(s => new
                       {
                           VTId = s.VTypeId,
                           Type = s.Type
                       })
                        .ToList();
            ViewBag.VehicleType = QkSelect.List(stand1, "VTId", "Type");

            var stand2 = db.VehicleManufacturers
                       .Select(m => new
                       {
                           MId = m.MId,
                           Manufacturer = m.Manufacturer
                       })
                        .ToList();
            ViewBag.VehicleManufacturer = QkSelect.List(stand2, "MId", "Manufacturer");


            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Create([Bind("ModelId,Model,MaId,VTId")] VehicleModel Vehicle)
        {
            bool stat = false;
            string msg;
            long? Id = 0;

            if (ModelState.IsValid)
            {


                var Exists = db.VehicleModels.Any(c => c.Model == Vehicle.Model && c.MaId == Vehicle.MaId && c.VTId==Vehicle.VTId);

                if (Exists)
                {
                    msg = "Model already exists.";
                    stat = false;


                }

                else
                {
                    db.VehicleModels.Add(Vehicle);
                    db.SaveChanges();
                    Id = Vehicle.ModelId;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "VehicleModel", "VehicleModels", findip(), Vehicle.ModelId, "Vehicle Model Added Successfully");

                    msg = "Vehicle Model added successfully.";
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
            VehicleModel Vehicle = db.VehicleModels.Find(id);
            if (Vehicle == null)
            {
                return NotFound();
            }

            var stand1 = db.VehicleTypes
                       .Select(s => new
                       {
                           VTId = s.VTypeId,
                           Type = s.Type
                       })
                        .ToList();
            ViewBag.VehicleType = QkSelect.List(stand1, "VTId", "Type");

            var stand2 = db.VehicleManufacturers
                       .Select(m => new
                       {
                           MId = m.MId,
                           Manufacturer = m.Manufacturer
                       })
                        .ToList();
            ViewBag.VehicleManufacturer = QkSelect.List(stand2, "MId", "Manufacturer");

            return PartialView(Vehicle);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Edit([Bind("ModelId,Model,MaId,VTId")] VehicleModel Vehicle)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.VehicleModels.Any(c => c.Model == Vehicle.Model && c.MaId == Vehicle.MaId && c.VTId == Vehicle.VTId);
                if (Exists)
                {
                    msg = "Model already exists.";
                    stat = false;
                }
                else
                {
                    db.Entry(Vehicle).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "VehicleModel", "VehicleModels", findip(), Vehicle.ModelId, "Model Updated Successfully");
                    msg = "Successfully updated Model.";
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
            VehicleModel Vehicle = db.VehicleModels.Find(id);
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
            var Exists = db.VehicleModels.Any(c => c.ModelId == id);
            if (Exists)
            {

                stat = DeleteFn(id);
                msg = "Successfully deleted Model.";
            }
            else
            {
                msg = "Unable to delete Model.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public bool DeleteFn(long id)
        {
            VehicleModel Vehicle = db.VehicleModels.Find(id);
            if (Vehicle != null)
            {
                db.VehicleModels.RemoveRange(db.VehicleModels.Where(a => a.ModelId == id));


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "VehicleModel", "VehicleModels", findip(), Vehicle.ModelId, "Vehicle Model Deleted Successfully");
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

            }

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat3() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }
        public JsonResult SearchManufacturer(string q, string x,long VTid)
        {
            List<SelectFormat3> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.VehicleManufacturers
                                  where (VTid != 0 && a.VTyId == VTid) &&
                                        (q == null || a.Manufacturer.ToLower().Contains(q.ToLower()) || a.Manufacturer.Contains(q))
                                  select new SelectFormat3
                                  {
                                      text = a.Manufacturer,
                                      id = a.MId
                                  }).OrderBy(b => b.text).ToList();
                
            }
            else
            {
                serialisedJson = (from a in db.VehicleManufacturers
                                  where (a.VTyId == VTid)
                                  select new SelectFormat3
                                  {
                                      text = a.Manufacturer,
                                      id = a.MId
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