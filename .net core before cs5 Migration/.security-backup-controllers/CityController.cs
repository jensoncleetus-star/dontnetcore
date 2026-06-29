using System.Linq.Dynamic.Core;
using ApplicationUserManager = Microsoft.AspNetCore.Identity.UserManager<QuickSoft.Models.ApplicationUser>;
using ApplicationSignInManager = Microsoft.AspNetCore.Identity.SignInManager<QuickSoft.Models.ApplicationUser>;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System.Collections.Generic;
using System.Data;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class CityController : BaseController
    {

        ApplicationDbContext db;
        Common com;
        public CityController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: City
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetCity()
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var UserId = User.Identity.GetUserId();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            var c = (from a in db.Cities
                     join b in db.Users on a.CreatedBy equals b.Id
                     //where a.CreatedBy == UserId
                     select new
                     {
                         a.Id,
                         a.CityName,
                         CreatedBy = b.UserName,
                         a.CreatedDate,
                         a.Status
                     });
            db.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                c = c.Where(p => p.CityName.ToString().ToLower().Contains(search.ToLower()));
                c = c.Where(p => p.Id.ToString().ToLower().Contains(search.ToLower()));
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                c = c.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = c.Count();
            var data = c.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        public JsonResult SearchCity(string q)
        {
            List<SelectUserFormat> serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Cities
                                  where (a.CityName.ToLower().Contains(q.ToLower()) || a.CityName.Contains(q)) && a.Status == 0
                                  select new SelectUserFormat
                                  {
                                      id = a.CityName,
                                      text = a.CityName, //each json object will have
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Cities.Where(a => a.Status == 0)
                .Select(a => new SelectUserFormat
                {
                    text = a.CityName, //each json object will have 
                        id = a.CityName
                }).OrderBy(a => a.text).ToList();

            }//
            if (string.IsNullOrEmpty(q))
            {
                var initial = new SelectUserFormat() { id = "" };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);


        }

        public ActionResult Create()
        {

            return PartialView();

        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Create New")]
        [ValidateAntiForgeryToken]
        public JsonResult Create(City cCity)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            var UserId = User.Identity.GetUserId();
            var Exists = db.Cities.Any(c => c.CityName == cCity.CityName && c.CreatedBy == UserId);
            if (Exists)
            {
                msg = "City Name already Exist. Use different City Name !";
                stat = false;
            }
            else
            {
                var today = Convert.ToDateTime(System.DateTime.Now);
                var city = new City
                {
                    CityName = cCity.CityName,
                    CreatedBy = UserId,
                    CreatedDate = today,
                    Branch = cCity.Branch,
                    Status = cCity.Status
                };
                db.Cities.Add(city);
                db.SaveChanges();
                Id = cCity.Id;

                com.addlog(LogTypes.Created, UserId, "City", "Cities", findip(), city.Id, "City Created Successfully");
                msg = "Successfully added City details.";
                stat = true;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id } };
        }

        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            City info = db.Cities.Find(id);

            if (info == null)
            {
                return NotFound();
            }
            return PartialView(info);

        }

        [HttpPost]
        // [QkAuthorize(Roles = "Dev,Edit StickyLabels")]
        public JsonResult Edit(long? id, City cityinfo)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                City info = db.Cities.Find(id);
                info.CityName = cityinfo.CityName;
                info.Status = cityinfo.Status;
                db.SaveChanges();
                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Updated, UserId, "City", "Cities", findip(), info.Id, "City Details Updated Successfully");
                msg = "Successfully Updated City Details.";
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
            City info = db.Cities.Find(id);
            if (info == null)
            {
                return NotFound();
            }
            return PartialView(info);

        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Delete Sticky Label")]
        public JsonResult Delete(long id, IFormCollection collection)
        {
            bool stat = false;
            string msg;
            City Info = db.Cities.Find(id);
            var Msg = chkDeleteWithMsg(id);
            if (Msg != null)
            {
                msg = Msg;
                stat = false;
            } else {
                db.Cities.Remove(Info);
                db.SaveChanges();
                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "City", "Cities", findip(), (long)Info.Id, "City Deleted Successfully");
                stat = true;
                msg = "Successfully Deleted City.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            City Info = db.Cities.Find(id);
            var Exists = (db.Contacts.Any(c => c.City == Info.CityName));
            if (Exists)
            {
                msg = "City Already used in Contacts !!";
            }
            else
            {
                msg = null;
            }

            return msg;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        //[HttpGet]
        ////[QkAuthorize(Roles = "Dev,BillSundry Status")]
        //// POST: master/ChangeStatus/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        ////[QkAuthorize(Roles = "Dev,BillSundry Status")]

        //    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

    }


}
