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
  
    public class AreaController : BaseController
    {
        // GET: Department
        [RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Area List")]
        public ActionResult Index()
        {
            return View();
        }
        ApplicationDbContext db;
        Common com;
        public AreaController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        [RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Area List")]
        [HttpPost]
        public JsonResult GetArea()
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
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = db.Areas.Select(b => new
            {
                b.AreaId,
                b.AreaName
            });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.AreaName.ToString().ToLower().Contains(search.ToLower()));
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
        [RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Create Area")]
        public ActionResult Create()
        {
            return PartialView();
        }

        // POST: Dep/Create
       
        [RedirectingAction]
        [ValidateAntiForgeryToken]
        //[QkAuthorize(Roles = "Dev,Create Area")]
        [HttpPost]
        public JsonResult Create(Area are)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.Areas.Any(c => c.AreaName == are.AreaName);
                if (Exists)
                {
                    msg = "Area Name already exists.";
                    stat = false;
                }
                else
                {
                    var area = new Area
                    {
                        AreaName = are.AreaName

                    };
                    db.Areas.Add(area);
                    db.SaveChanges();
                    Id = area.AreaId;
                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "Area", "Areas", findip(), area.AreaId, "Area Added Successfully");

                    msg = "Successfully added Area details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id } };
        }


        // GET: dep/Edit/5
        [RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Edit Area")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Area area = db.Areas.Find(id);

            if (area == null)
            {
                return NotFound();
            }
            return PartialView(area);
        }

        // POST: department/Edit/5
        [HttpPost]
        [RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Edit Area")]
        public JsonResult Edit(int? id, Area area)
        {

            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.Areas.Any(c => c.AreaName == area.AreaName && c.AreaId != id);
                if (Exists)
                {
                    msg = "Area already exists.";
                    stat = false;
                }
                else
                {
                    db.Entry(area).State = EntityState.Modified;
                    db.SaveChanges();
                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "Area", "Areas", findip(), area.AreaId, "Area Updated Successfully");


                    msg = "Successfully updated Area details.";
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
        [RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Delete Area")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Area area = db.Areas.Find(id);
            if (area == null)
            {
                return NotFound();
            }

            return PartialView(area);
        }

        // POST: Field/Delete/5
        [RedirectingAction]
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Delete Area")]
        public JsonResult Delete(long id, IFormCollection collection)
        {
            bool stat = false;
            string msg;
            var Exists = db.Tables.Any(c => c.AreaId == id);
            if (Exists)
            {
                msg = "Unable to delete Area, Table with this Area exists.";
                stat = false;
            }
            else
            {
                Area area = db.Areas.Find(id);
                db.Areas.Remove(area);
                db.SaveChanges();

                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "Area", "Areas", findip(), area.AreaId, "Area Deleted Successfully");


                stat = true;
                msg = "Successfully Deleted Area details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        public JsonResult Search(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            serialisedJson = db.Areas
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.AreaName, //each json object will have 
                                      id = b.AreaId
                                  })
                                  .OrderBy(b => b.text).ToList();

            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
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
