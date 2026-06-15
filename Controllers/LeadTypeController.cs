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

    public class LeadTypeController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public LeadTypeController()
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
        public ActionResult GetLeadType(string ColName)
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
            var uEdit = User.IsInRole("Edit Lead Rejection");
            var uDelete = User.IsInRole("Delete Lead Rejection");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.LeadTypes
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
        public JsonResult getleadtype(string q, string x, string page)
        {
            var isemirtech = db.companys.Any(o => o.CPName.Contains("EMIRTECH TECHNOLOGY"));
            List<SelectFormat> serialisedJson;
            string stt = "All";
            var start = Convert.ToInt32(page);
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                if (!isemirtech)
                {
                    serialisedJson = db.LeadTypes.Where(p => p.Type.ToLower().Contains(q.ToLower()))
                                      .Select(b => new SelectFormat
                                      {
                                          text = b.Type,
                                          id = b.TypeId
                                      })
                                      .OrderBy(b => b.text).ToList();
                }
                else
                {
                    serialisedJson = db.LeadTypes.Where(p =>p.TypeId> 10013&& p.Type.ToLower().Contains(q.ToLower()))
                                                          .Select(b => new SelectFormat
                                                          {
                                                              text = b.Type,
                                                              id = b.TypeId
                                                          })
                                                          .OrderBy(b => b.text).ToList();
                }
            }
            else
            {
                if (!isemirtech)
                {
                    serialisedJson = db.LeadTypes
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.Type,
                                      id = b.TypeId
                                  }).OrderBy(b => b.text).ToList();
                }
                else
                {
                    serialisedJson = db.LeadTypes.Where(p => p.TypeId > 10013)
                                                    .Select(b => new SelectFormat
                                                    {
                                                        text = b.Type,
                                                        id = b.TypeId
                                                    }).OrderBy(b => b.text).ToList();
                }

            }
            if (x == "all" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
              var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Lead Type" };
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
        public JsonResult Create([Bind("TypeId,Type")] LeadType Lead)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.LeadTypes.Any(c => c.Type == Lead.Type);
                if (Exists)
                {
                    msg = "Detalis already exists.";
                    stat = false;
                }
                else
                {
                    db.LeadTypes.Add(Lead);
                    db.SaveChanges();
                    Id = Lead.TypeId;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "LeadType", "LeadTypes", findip(), Lead.TypeId, "Lead Types Added Successfully");

                    msg = "Lead Types added successfully.";
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
           LeadType Lead = db.LeadTypes.Find(id);
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

        public JsonResult Edit([Bind("TypeId,Type")] LeadType Lead)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.LeadTypes.Any(c => c.Type == Lead.Type && c.TypeId != Lead.TypeId);
                if (Exists)
                {
                    msg = "Lead Type already exists.";
                    stat = false;
                }
                else
                {
                    db.Entry(Lead).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "LeadType", "LeadTypes", findip(), Lead.TypeId, "Lead  Type Details Updated Successfully");


                    msg = "Successfully updated Lead Type details.";
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
            LeadType Lead = db.LeadTypes.Find(id);
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
            var Exists = db.Customers.Any(c => c.LeadType == id);
            if (Exists)
            {
                msg = "Unable to delete Type, Customer with this Type exists.";
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Successfully deleted LeadType.";
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
                Success("Deleted " + count + " LeadType, Unable to Delete " + notdel + " LeadType. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + "LaedType.", true);
            }
            else
            {
                Success("Deleted " + count + " LeadType.", true);
            }
            return RedirectToAction("Index", "LeadType");
        }
      

        public bool DeleteFn(long id)
        {
           LeadType Lead = db.LeadTypes.Find(id);
            if (Lead != null)
            {
                db.LeadTypes.RemoveRange(db.LeadTypes.Where(a => a.TypeId == id));


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "LeadType", "LeadTypes", findip(), Lead.TypeId, "Lead Type Deleted Successfully");
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