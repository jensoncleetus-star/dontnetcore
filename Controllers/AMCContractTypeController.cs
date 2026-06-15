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

    public class AMCContractTypeController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public AMCContractTypeController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: LeadRejection
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        //   [QkAuthorize(Roles = "Dev,Lead Type")]
        public ActionResult GetContractType(string ColName)
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
            var uEdit = User.IsInRole("Edit AMCContract Type");
            var uDelete = User.IsInRole("Delete AMCContract Type");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.AmcContractTypes
                     where (ColName == "" || a.Type == ColName)
                     select new
                     {

                         a.TypeId,
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
        public JsonResult getcontracttype(string q, string x, string page)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            var start = Convert.ToInt32(page);
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.AmcContractTypes.Where(p => p.Type.ToLower().Contains(q.ToLower()))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.Type,
                                      id = b.TypeId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.AmcContractTypes
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.Type,
                                      id = b.TypeId
                                  }).OrderBy(b => b.text).ToList();

            }
            if (x == "all" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Contract Type" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public ActionResult Create()
        {
            return PartialView();
        }

        // POST: LeadType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Create LeadType")]
        public JsonResult Create([Bind("TypeId,Type")] AmcContractType Lead)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.AmcContractTypes.Any(c => c.Type == Lead.Type);
                if (Exists)
                {
                    msg = "Contract Type already exists.";
                    stat = false;
                }
                else
                {
                    db.AmcContractTypes.Add(Lead);
                    db.SaveChanges();
                    Id = Lead.TypeId;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "AMCContractType", "AMCContractTypes", findip(), Lead.TypeId, " ContractType  Added Successfully");

                    msg = "Contract Type added successfully.";
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
            AmcContractType Lead = db.AmcContractTypes.Find(id);
            if (Lead == null)
            {
                return NotFound();
            }
            return PartialView(Lead);
        }



        // POST: LeadType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public JsonResult Edit([Bind("TypeId,Type")] AmcContractType Lead)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.AmcContractTypes.Any(c => c.Type == Lead.Type && c.TypeId != Lead.TypeId);
                if (Exists)
                {
                    msg = "Contract Type already exists.";
                    stat = false;
                }
                else
                {
                    db.Entry(Lead).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "AMCContractType", "AMCContractTypes", findip(), Lead.TypeId, " Contract Type Updated Successfully");


                    msg = "Successfully updated Contract Type.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        // [QkAuthorize(Roles = "Dev,Delete Lead Rejection")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AmcContractType Lead = db.AmcContractTypes.Find(id);
            if (Lead == null)
            {
                return NotFound();
            }
            return PartialView(Lead);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //  [QkAuthorize(Roles = "Dev,Delete Lead Type")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var Exists = db.Amcs.Any(c => c.ContractTypeId == id);
            if (Exists)
            {
                msg = "Unable to delete Type, AMC with this Type exists.";
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Successfully deleted ContractType.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        // [QkAuthorize(Roles = "Dev,Delete LeadType")]


        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;

            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " ContractType, Unable to Delete " + notdel + " ContractType. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + "ContractType.", true);
            }
            else
            {
                Success("Deleted " + count + " ContractType.", true);
            }
            return RedirectToAction("Index", "AMCContractType");
        }


        public bool DeleteFn(long id)
        {
            AmcContractType Lead = db.AmcContractTypes.Find(id);
            if (Lead != null)
            {
                db.AmcContractTypes.RemoveRange(db.AmcContractTypes.Where(a => a.TypeId == id));


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "ContractType", "ContractTypes", findip(), Lead.TypeId, "Contract Type Deleted Successfully");
                db.SaveChanges();
            }
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