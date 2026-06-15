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
    public class taskdashboardorderController: BaseController
    {
        ApplicationDbContext db;
        Common com;
        public taskdashboardorderController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Location
        public ActionResult Index()
        {
          
            return View();
        }
        [HttpPost]
        public ActionResult getleaddashboard(string SizeName)
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


            var v = (from a in db.protaskdashbordorder
                     join b  in db.TaskStatus on a.task equals b.TaskStatusId 
                   
                     select new
                     {
                         b.TaskStatusId,
                         b.StatusName,
                     a.protaskdashboardid,
                        a.dashboardposition,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete


                     }); 

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
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

          
                var status = db.TaskStatus
                             .Select(s => new
                             {
                                 FieldID = s.TaskStatusId,
                                 FieldName = s.StatusName
                             }).OrderBy(o=>o.FieldName)
                              .ToList();
                ViewBag.status = QkSelect.List(status,  "FieldID", "FieldName");
            List<SelectListItem> sss = new List<SelectListItem>();
            for(long i=1;i<=30;i++)
            {
                var ss = new SelectListItem { Text = i.ToString(), Value = i.ToString() };
                sss.Add(ss);
            }
            ViewBag.position = QkSelect.List(sss, "Text", "Value");
                return PartialView();



        }

        // POST:Location/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Create Item Size")]
        public JsonResult Create(protaskdashbordorder dash)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;

            if (ModelState.IsValid)
            {
                


                db.protaskdashbordorder.Add(dash);
                db.SaveChanges();
                Id = dash.protaskdashboardid;

                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Created, UserId, "Task Dashboard", "Task Dashboard order", findip(), dash.protaskdashboardid , "Task Dashboard positon Added Successfully");

                msg = "Lead Position added successfully.";
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
            protaskdashbordorder dash = db.protaskdashbordorder.Find(id);
            if (dash == null)
            {
                return NotFound();
            }

            


            var status = db.TaskStatus
                       .Select(s => new
                       {
                           FieldID = s.TaskStatusId,
                           FieldName = s.StatusName
                       }).OrderBy(o => o.FieldName)
                        .ToList();
            ViewBag.statuses = QkSelect.List(status, "FieldID", "FieldName");
            List<SelectListItem> sss = new List<SelectListItem>();
            for (long i = 1; i <= 30; i++)
            {
                var ss = new SelectListItem { Text = i.ToString(), Value = i.ToString() };
                sss.Add(ss);
            }
            ViewBag.position = QkSelect.List(sss, "Text", "Value");

            return PartialView(dash);
        }



        // POST: ItemCategory/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Edit Item Size")]
        public JsonResult Edit(protaskdashbordorder dash)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                
                
                    db.Entry(dash).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "taskdashboard", "taskdashboard", findip(), dash.protaskdashboardid , "taskdashboard Updated Successfully");


                    msg = "Successfully updated Task Dashboard details.";
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
            protaskdashbordorder leaddash = db.protaskdashbordorder.Find(id);
            if (leaddash == null)
            {
                return NotFound();
            }
            return PartialView(leaddash);
        }

        // POST: ItemCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //  [QkAuthorize(Roles = "Dev,Delete Item Color")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            stat = DeleteFn(id);
            msg = "Successfully deleted Location details.";

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
            protaskdashbordorder leaddash = db.protaskdashbordorder.Find(id);
            if (leaddash != null)
            {
                db.protaskdashbordorder.RemoveRange(db.protaskdashbordorder.Where(a => a.protaskdashboardid  == id));
                db.SaveChanges();
            }

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "leaddashboard", "leaddashboard", findip(), leaddash.protaskdashboardid , "leaddashboard Deleted Successfully");
           
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
