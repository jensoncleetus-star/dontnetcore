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
    public class geowallController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public geowallController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: geowall
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Getgeowall()
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


            var v = (from a in db.geowalls
                    
                     join c in db.Employees on a.EmployeeId equals c.EmployeeId

                     select new
                     {
                        a.lat,
                        a.log,
                        a.distance,
                        a.geowallId,
                         Employees = (from z in db.geowalls
                                      join y in db.Employees on z.EmployeeId equals y.EmployeeId
                                      where z.lat == a.lat && z.log == a.log
                                      select new
                                      {
                                          id = y.EmployeeId,
                                          LastName = (y.LastName != null) ? y.LastName : "",
                                          FirstName = (y.FirstName != null) ? y.FirstName : ""
                                      }).ToList(),

                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete,
                        


                     }).ToList().OrderBy(o => o.lat).Select(o => new {
                         o.lat,
                         o.log,
                         o.geowallId,
                         o.Employees,
                         o.distance,
                         o.Dev,
                         o.Edit,
                         o.Delete,
                      
                     }).GroupBy(x => new { x.lat, x.log }, (key, g) => g.OrderBy(m => m.log).FirstOrDefault());


            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.lat.ToString().ToLower().Contains(search.ToLower()) ||
                p.log.ToString().ToLower().Contains(search.ToLower())
                );
            }

            //SORT


            var data = v.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
       

        public ActionResult Create()
        {


            ViewBag.Employee = QkSelect.List(
                                       new List<SelectListItem>
                                       {
                                            new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                       }, "Value", "Text", 1);
            return PartialView();



        }

        // POST:Location/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Create Item Size")]
        public JsonResult Create(geowallviewmodal accmap)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;

            if (ModelState.IsValid)
            {
                foreach (var arr in accmap.EmployeeId)
                {

                    geowall obj = new geowall
                    {
                        lat = accmap.lat,
                        log = accmap.log,
                        EmployeeId = arr,
                        distance = accmap.distance,
                        


                    };

                    db.geowalls.Add(obj);
                }
                db.SaveChanges();

                msg = "geo wall added successfully.";
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
            geowall accmap = db.geowalls.Find(id);
            if (accmap == null)
            {
                return NotFound();
            }

            
            //      .Select(s => new
            //          FieldID = s.EmployeeId,
            //          FieldName = s.FirstName
            //      })

            var emp = (from a in db.Employees
                       join b in db.geowalls on a.EmployeeId equals b.EmployeeId
                       where b.lat == accmap.lat && b.log==accmap.log 
                       select new
                       {


                           FieldID = a.EmployeeId,
                           FieldName = a.FirstName
                       })
                            .ToList();
            ViewBag.employee = QkSelect.List(emp, "FieldID", "FieldName");

            //Getting the Material Centre Name
            geowallviewmodal acm = new geowallviewmodal
            {
                lat = accmap.lat,
                EmployeeId = emp.Select(o => o.FieldID).ToArray(),
                log = accmap.log,
                distance = accmap.distance,
                
            };
            return PartialView(acm);
        }



        // POST: ItemCategory/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Edit Item Size")]
        public JsonResult Edit(geowallviewmodal accmap)
        {
            bool stat = false;
            string msg;
            Int64 PrevAccountId;
            var PrevAccntName = "";

            db.geowalls.RemoveRange(db.geowalls.Where(o => o.lat == accmap.lat&& o.log==accmap.log));
            db.SaveChanges();
            if (ModelState.IsValid)
            {


                foreach (var arr in accmap.EmployeeId)
                {

                    geowall obj = new geowall
                    {
                        lat = accmap.lat,
                        log = accmap.log,
                        EmployeeId = arr,
                        distance = accmap.distance,
                        

                    };


                    db.geowalls.Add(obj);
                    db.SaveChanges();
                }


                msg = "Successfully updated Account  details.";
                stat = true;


            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            geowall accmap = db.geowalls.Find(id);
            if (accmap == null)
            {
                return NotFound();
            }
            return PartialView(accmap);
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

            if (stat == true)
                msg = "Successfully deleted geo wall details...";
            else
                msg = "Unable to Delete..Account is used in Account Transaction..";

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
                Success("Deleted " + count + " Details...", true);
            }
            return RedirectToAction("Index", "geowall");
        }

        public bool DeleteFn(long id)
        {
            bool ReturnValue = true;

            geowall accmap = db.geowalls.Find(id);
            if (accmap != null)
            {

                db.geowalls.RemoveRange(db.geowalls.Where(a => a.geowallId == id));
                db.SaveChanges();
                ReturnValue = true;

            }

            return ReturnValue;
        }
    }
}
