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
    public class SourceOfLeadController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public SourceOfLeadController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: ProductBrand
        
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        
        public ActionResult GetSourceOfLead()
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
            var v = (from a in db.SourceOfLeads select a);

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.SrcName.ToString().ToLower().Contains(search.ToLower()));
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
            return PartialView();
        }

        // POST: ProductCategory/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        
        public ActionResult Create(SourceOfLead srcLead)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.SourceOfLeads.Any(c => c.SrcName == srcLead.SrcName);
                if (Exists)
                {
                    msg = "Source Of Lead already exists.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    var src = new SourceOfLead
                    {
                        SrcName = srcLead.SrcName

                    };
                    db.SourceOfLeads.Add(srcLead);
                    db.SaveChanges();
                    Id = srcLead.SourceOfLeadId;
                    msg = "Source Of Lead added successfully.";
                    stat = true;
                    com.addlog(LogTypes.Created, UserId, "SourceOfLead", "SourceOfLeads", findip(), Id, "Source Of Lead Added Successfully");


                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            // return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id } };
            return RedirectToAction("Index", "SourceOfLead");
        }

        [QkAuthorize(Roles = "Dev,Edit SourceOfLead")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SourceOfLead srclead = db.SourceOfLeads.Find(id);
            if (srclead == null)
            {
                return NotFound();
            }
            return PartialView(srclead);
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit SourceOfLead")]
        public JsonResult Edit(int id, SourceOfLead srclead)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.SourceOfLeads.Any(c => c.SrcName == srclead.SrcName && c.SourceOfLeadId != srclead.SourceOfLeadId);
                if (Exists)
                {
                    msg = "Source Of Lead already exists.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    SourceOfLead src = db.SourceOfLeads.Find(id);
                    src.SrcName = srclead.SrcName;
                    db.Entry(src).State = EntityState.Modified;
                    db.SaveChanges();
                    msg = "Successfully updated Source Of Leads details.";
                    stat = true;

                    com.addlog(LogTypes.Updated, UserId, "SourceOfLead", "SourceOfLeads", findip(), src.SourceOfLeadId, "Source Of Lead Updated Successfully");

                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        // GET: ProductCategory/Delete/5
        [QkAuthorize(Roles = "Dev,Delete SourceOfLead")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SourceOfLead srclead = db.SourceOfLeads.Find(id);
            if (srclead == null)
            {
                return NotFound();
            }
            return PartialView(srclead);
        }

        // POST: ProductCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [QkAuthorize(Roles = "Dev,Delete SourceOfLead")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var Exists = db.Customers.Any(c => c.SourceOfLead == id);
            if (Exists)
            {
                msg = "Unable to delete Source Of Lead, Customer with this Source Of Lead exists.";
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Successfully deleted Source of Lead.";

            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public bool DeleteFn(long id)
        {
           SourceOfLead Lead = db.SourceOfLeads.Find(id);
            if (Lead != null)
            {
                db.SourceOfLeads.RemoveRange(db.SourceOfLeads.Where(a => a.SourceOfLeadId == id));


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "SourceofLead", "SourceofLeads", findip(), Lead.SourceOfLeadId, "Lead Source Deleted Successfully");
                db.SaveChanges();
            }
            return true;
        }

        public JsonResult SearchSrcLead(string q, string x)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.SourceOfLeads.Where(p => p.SrcName.ToLower().Contains(q.ToLower()) || p.SrcName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.SrcName, //each json object will have 
                                      id = b.SourceOfLeadId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.SourceOfLeads.Select(b => new SelectFormat
                {
                    text = b.SrcName, //each json object will have 
                    id = b.SourceOfLeadId
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Source Of Lead" };
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
