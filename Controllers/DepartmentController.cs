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
    public class DepartmentController : BaseController
    {
        // GET: Department
        [QkAuthorize(Roles = "Dev,Department List")]
        public ActionResult Index()
        {
            return View();
        }
         ApplicationDbContext db;
        Common com;
         public DepartmentController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

   
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Department List")]
        public JsonResult GetDepartment()
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
            var uEdit = User.IsInRole("Edit Department");
            var uDelete = User.IsInRole("Delete Department");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from b in db.Departments
                     join c in db.CalendarTemplates on b.CalendarTemplateId equals c.CalendarTemplateID into ctemp
                     from c in ctemp.DefaultIfEmpty()
                     select new
                     {
                         b.DepartmentID,
                         b.DepartmentName,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete,
                         c.TemplateName
                     });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.DepartmentName.ToString().ToLower().Contains(search.ToLower()));
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
        [QkAuthorize(Roles = "Dev,Create Department")]
        public ActionResult Create()
        {
            ViewBag.CTemplates = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = true, Text = "Select Calendar Template", Value = ""},
                               }, "Value", "Text", 0);

            var EnablePayroll = db.EnableSettings.Where(a => a.EnableType == "EnablePayroll").FirstOrDefault();
            var EnablePayrolls = EnablePayroll != null ? EnablePayroll.Status : Status.inactive;
            ViewBag.EnablePayroll = EnablePayrolls;

            return PartialView();
        }

        // POST: Dep/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Create Department")]
        public JsonResult Create(Department Dept)
        { 
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.Departments.Any(c => c.DepartmentName == Dept.DepartmentName);
                if (Exists)
                {
                    msg = "Department Name already exists.";
                    stat = false;
                }
                else
                {
                    var calendertemp = db.CalendarTemplates.Where(a => a.DefaultValue == true).Select(a=>a.CalendarTemplateID).FirstOrDefault();
                    var dep = new Department
                    {
                        DepartmentName = Dept.DepartmentName,
                        CalendarTemplateId = Dept.CalendarTemplateId!=null ? Dept.CalendarTemplateId : calendertemp,
                    };
                    db.Departments.Add(dep);
                    db.SaveChanges();
                    Id = dep.DepartmentID;
                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "Department", "Departments", findip(), dep.DepartmentID, "Department Added Successfully");

                    msg = "Successfully added Department details.";
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


        // GET: dep/Edit/5
        [QkAuthorize(Roles = "Dev,Edit Department")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Department Departmentsinformation = db.Departments.Find(id);

            if (Departmentsinformation == null)
            {
                return NotFound();
            }

            var Temp = db.CalendarTemplates
               .Select(s => new
               {
                   Id = s.CalendarTemplateID,
                   Name = s.TemplateName
               }).ToList();
            ViewBag.CTemplates = QkSelect.List(Temp, "Id", "Name");

            var EnablePayroll = db.EnableSettings.Where(a => a.EnableType == "EnablePayroll").FirstOrDefault();
            var EnablePayrolls = EnablePayroll != null ? EnablePayroll.Status : Status.inactive;
            ViewBag.EnablePayroll = EnablePayrolls;

            return PartialView(Departmentsinformation);
        }

        // POST: department/Edit/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Department")]
        public JsonResult Edit(int id, Department dept)
        {
            
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.Departments.Any(c => c.DepartmentName == dept.DepartmentName && c.DepartmentID != id);
                if (Exists)
                {
                    msg = "Department already exists.";
                    stat = false;
                }
                else
                {
                    var calendertemp = db.CalendarTemplates.Where(a => a.DefaultValue == true).Select(a => a.CalendarTemplateID).FirstOrDefault();
                    dept.CalendarTemplateId = dept.CalendarTemplateId != null ? dept.CalendarTemplateId : calendertemp;
                    db.Entry(dept).State = EntityState.Modified;
                    db.SaveChanges();
                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "Department", "Departments", findip(), dept.DepartmentID, "Department Updated Successfully");


                    msg = "Successfully updated department details.";
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
        [QkAuthorize(Roles = "Dev,Delete Department")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Department Depinfo = db.Departments.Find(id);
            if (Depinfo == null)
            {
                return NotFound();
            }

            return PartialView(Depinfo);
        }

        // POST: Field/Delete/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Department")]
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
                Department Depinfo = db.Departments.Find(id);
                db.Departments.Remove(Depinfo);
                db.SaveChanges();

                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "Department", "Departments", findip(), Depinfo.DepartmentID, "Department Deleted Successfully");


                stat = true;
                msg = "Successfully Deleted Department  details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            var Exists = db.Employees.Any(c => c.DepartmentID == id);
            if (Exists)
            {
                msg = "Unable to delete Department, user with this Department exists.";
            }
            else
            {
                msg = null;
            }

            return msg;
        }

        public JsonResult SearchDepartment(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Departments.Where(p => p.DepartmentName.ToLower().Contains(q.ToLower()) || p.DepartmentName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.DepartmentName, //each json object will have 
                                      id = b.DepartmentID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Departments
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.DepartmentName, //each json object will have 
                                      id = b.DepartmentID
                                  }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Department" };
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
