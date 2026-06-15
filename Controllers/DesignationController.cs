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
    [RedirectingAction]
    public class DesignationController : BaseController
    {
        // GET: Designation
        [QkAuthorize(Roles = "Dev,Designation List")]
        public ActionResult Index()
        {
            return View();
        }
         ApplicationDbContext db;
        Common com;
        public DesignationController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

   
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Designation List")]
        public JsonResult GetDesignation()
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
            var uEdit = User.IsInRole("Edit Designation");
            var uDelete = User.IsInRole("Delete Designation");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.Designations
                     join b in db.Departments on a.department equals b.DepartmentID into dep
                     from b in dep.DefaultIfEmpty()

select new
            {
                a.DesignationID,
                a.DesignationName,
                b.DepartmentName,
                Dev = uDev,
                Edit = uEdit,
                Delete = uDelete
            });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.DesignationName.ToString().ToLower().Contains(search.ToLower()));
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
        // GET: Field/Create
        [QkAuthorize(Roles = "Dev,Create Designation")]
        public ActionResult Create()
        {

            var use = db.Departments
                  .Select(s => new
                  {
                      ID = s.DepartmentID,
                      
                   
                      Name = s.DepartmentName
                  })
                  .ToList();

            ViewBag.departments = QkSelect.List(use, "ID", "Name");
            return PartialView();
        }

        // POST: Field/Create
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create Designation")]
        [ValidateAntiForgeryToken]
        public JsonResult Create(Designation Design)
        { 
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.Designations.Any(c => c.DesignationName == Design.DesignationName);
                if (Exists)
                {
                    msg = "Designation Name already exists.";
                    stat = false;
                }
                else
                { var desg = new Designation
                    {
                        DesignationName = Design.DesignationName ,
                        department=Design.department,
                     
                    };
                    db.Designations.Add(desg);
                    db.SaveChanges();
                    Id = desg.DesignationID;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "Designation", "Designations", findip(), desg.DesignationID, "Designation Added Successfully");

                    msg = "Successfully added Designation details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg , Id } };
        }


        // GET: desg/Edit/5
        [QkAuthorize(Roles = "Dev,Edit Designation")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var use = db.Departments
              .Select(s => new
              {
                  ID = s.DepartmentID,


                  Name = s.DepartmentName
              })
              .ToList();

            ViewBag.departments = QkSelect.List(use, "ID", "Name");
            Designation Designationinformation = db.Designations.Find(id);

            if (Designationinformation == null)
            {
                return NotFound();
            }
            return PartialView(Designationinformation);
        }

        // POST: Designationinform/Edit/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Designation")]
        public JsonResult Edit(int id, Designation Desgnat)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.Designations.Any(c => c.DesignationName == Desgnat.DesignationName && c.DesignationID != id);
                if (Exists)
                {
                    msg = "Designation already exists.";
                    stat = false;
                }
                else
                {
                    db.Entry(Desgnat).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "Designation", "Designations", findip(), Desgnat.DesignationID, "Designation Updated Successfully");


                    msg = "Successfully Updated Designation Details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        // GET: Desg/Delete/5
        [QkAuthorize(Roles = "Dev,Delete Designation")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Designation Desginfo = db.Designations.Find(id);
            if (Desginfo == null)
            {
                return NotFound();
            }

            return PartialView(Desginfo);
        }

        // POST: Field/Delete/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Designation")]
        public JsonResult Delete(long id, IFormCollection collection)
        {
            bool stat = false;
            string msg;
            var Msg = chkDeleteWithMsg(id);
            if (Msg != null)
            {
                msg = Msg;
                stat = false;
            }
            else
            {
                Designation Desginfo = db.Designations.Find(id);
                db.Designations.Remove(Desginfo);
                db.SaveChanges();

                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "Designation", "Designations", findip(), Desginfo.DesignationID, "Designation Deleted Successfully");


                stat = true;
                msg = "Successfully Deleted Designation  details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            var Exists = db.Employees.Any(c => c.DepartmentID == id);
            if (Exists)
            {
                msg = "Unable to delete Designation, user with this Designation exists.";
            }
            else
            {
                msg = null;
            }
            return msg;
        }

        public JsonResult SearchDesignation(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Designations.Where(p => p.DesignationName.ToLower().Contains(q.ToLower()) || p.DesignationName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.DesignationName, //each json object will have 
                                      id = b.DesignationID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Designations
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.DesignationName, //each json object will have 
                                      id = b.DesignationID
                                  }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Designation" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
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
