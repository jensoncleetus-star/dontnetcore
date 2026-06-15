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
using System.Net;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Globalization;

namespace QuickSoft.Controllers
{
    public class VehicleServiceController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public VehicleServiceController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        public ActionResult Create()
        {


            return PartialView();



        }
   
        [HttpPost]
        public ActionResult Create(vehiclemasterviewmodel vmodel)
        {
            var uid = User.Identity.GetUserId();
            var crtime = System.DateTime.Now;
            DateTime? opdate = null;
            if (vmodel.openingkelometerdate !=""&& vmodel.openingkelometerdate !=null)
            {
                opdate = DateTime.Parse(vmodel.openingkelometerdate.ToString(), new CultureInfo("en-GB"));
            }
            vehiclemaster vh = new vehiclemaster
            {
                 currentkelometer =vmodel.currentkelometer,
                  openingkelometer=vmodel.openingkelometer,
                  openingkelometerdate=opdate ,
                  VechicleName =vmodel.VechicleName,
                  RegistrationNumber =vmodel.RegistrationNumber,
                  remarks=vmodel.remarks,
                  createdby=uid,
                  createddate=crtime,
                 
            };
            db.vehiclemasters.Add(vh);
            db.SaveChanges();
           string msg = "Vehicle Added";
    bool  stat = true;


            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };



        }
        public ActionResult Edit(long? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            vehiclemasterviewmodel vmodel = new vehiclemasterviewmodel();
            var v = db.vehiclemasters.Find(id);
            vmodel.currentkelometer = v.currentkelometer;
            vmodel.VechicleName = v.VechicleName;
            vmodel.openingkelometer = v.openingkelometer;
            vmodel.RegistrationNumber = v.RegistrationNumber;
            vmodel.remarks = v.remarks;
            vmodel.vehicleid = v.vehicleid;
                return PartialView(vmodel);



        }
        [RedirectingAction]
        //[Authorize(Roles = "Dev,Delete PropertyType")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            vehiclemaster ptype = db.vehiclemasters.Find(id);
            if (ptype == null)
            {
                return NotFound();
            }
            return PartialView(ptype);
        }
        [RedirectingAction]
        //[Authorize(Roles = "Dev,Delete PropertyType")]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
          
                stat = DeleteFn(id);
                msg = "Successfully Deleted ";
           
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public bool DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            var ve = db.vehiclemasters.Where(o=>o.vehicleid==id);
            db.vehiclemasters.RemoveRange(ve);
            db.SaveChanges();
           return true;
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;

            return msg;
        }
        [HttpPost]
        public ActionResult Edit(vehiclemasterviewmodel vmodel)
        {
            var uid = User.Identity.GetUserId();
            var crtime = System.DateTime.Now;
            DateTime? opdate = null;
            if (vmodel.openingkelometerdate != "" && vmodel.openingkelometerdate != null)
            {
                opdate = DateTime.Parse(vmodel.openingkelometerdate.ToString(), new CultureInfo("en-GB"));
            }
            vehiclemaster v = db.vehiclemasters.Find((long)vmodel.vehicleid);

            v.currentkelometer = vmodel.currentkelometer;
            v.openingkelometer = vmodel.openingkelometer;
            v.openingkelometerdate = opdate;
            v.VechicleName = vmodel.VechicleName;
            v.RegistrationNumber = vmodel.RegistrationNumber;
            v.remarks = vmodel.remarks;
            v.createdby = uid;
            v.createddate = crtime;

            
            db.Entry(v).State = EntityState.Modified;
            db.SaveChanges();
            string msg = "Vehicle Updated";
            bool stat = true;


            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };



        }

        // GET: VehicleMaster
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GetVehicle()
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
            var uEdit = User.IsInRole("Edit Vehicle");
            var uDelete = User.IsInRole("Delete Vehicle");


            var v = (from a in db.vehiclemasters

                     select new
                     {
                         a.vehicleid,
                         a.VechicleName,
                         a.RegistrationNumber,
                         a.openingkelometerdate,
                         a.openingkelometer,
                         a.currentkelometer,
                         a.remarks,
                         currentreading = db.vehicleupdations.Where(o => o.vehicleid == a.vehicleid).OrderByDescending(o => o.vehicleupdateid).Select(o=>o.readings).FirstOrDefault(),
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete


                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.VechicleName.ToString().ToLower().Contains(search.ToLower())||
                p.RegistrationNumber.ToString().ToLower().Contains(search.ToLower())
                );
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);

            }

            recordsTotal = v.Count();
            var data = v.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
    }
}
