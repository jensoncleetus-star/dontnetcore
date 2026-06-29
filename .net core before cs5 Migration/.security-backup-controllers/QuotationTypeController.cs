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
    public class QuotationTypeController : BaseController
    {


        ApplicationDbContext db;
        Common com;
        public QuotationTypeController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        public ActionResult Create()
        {
            return PartialView();
        }
        public ActionResult Index()
        {
            return View();
        }
        // POST: QuotationType/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Create QuotationType")]

        // [QkAuthorize(Roles = "Dev,Create QuotationType")]
        public ActionResult Create([Bind("QuotId,QuotType")] QuotationType  ptype)
        {
            bool stat = false;
            Int64 QuotId = ptype.QuotId;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.QuotationTypes.Any(c => c.QuotType == ptype.QuotType);
                if (Exists)
                {
                    msg = "Quotation Type already exists.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    QuotationType c = new QuotationType();
                    c.QuotId = ptype.QuotId;
                    c.QuotType = ptype.QuotType;

                    db.QuotationTypes.Add(ptype);
                    db.SaveChanges();
                    
                    com.addlog(LogTypes.Created, UserId, "QuotationType", "QuotationTypes", findip(), QuotId, "Quotation Type Added Successfully");
                    Success("Quotation Types added successfully...", true);
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = true, message = "Success", Id = QuotId } };
        }
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QuotationType qttype = db.QuotationTypes.Find(id);
            if (qttype == null)
            {
                return NotFound();
            }
            return PartialView(qttype);
        }



        // POST: LeadType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public JsonResult Edit( QuotationType qttype)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.QuotationTypes.Any(c => c.QuotType == qttype.QuotType && c.QuotId != qttype.QuotId);
                if (Exists)
                {
                    msg = "Quotation Type already exists.";
                    stat = false;
                }
                else
                {
                    db.Entry(qttype).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "qutoationType", "qutoationTypes", findip(), qttype.QuotId, "Quotation  Type Details Updated Successfully");


                    msg = "Successfully updated Quotation Type details.";
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
            QuotationType qttype = db.QuotationTypes.Find(id);
            if (qttype == null)
            {
                return NotFound();
            }
            return PartialView(qttype);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //  [QkAuthorize(Roles = "Dev,Delete Lead Type")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var Exists = db.Quotations.Any(c => c.quotationtype == id);
            if (Exists)
            {
                msg = "Unable to delete Type, Quotation with this Type exists.";
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Successfully deleted Quoation Type.";
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
                Success("Deleted " + count + " QuotationType, Unable to Delete " + notdel + " QuotationType. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + "LaedType.", true);
            }
            else
            {
                Success("Deleted " + count + " QuotationType.", true);
            }
            return RedirectToAction("Index", "QuotationType");
        }


        public bool DeleteFn(long id)
        {
            QuotationType qttype = db.QuotationTypes.Find(id);
            if (qttype != null)
            {
                db.QuotationTypes.RemoveRange(db.QuotationTypes.Where(a => a.QuotId == id));


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "QuotationType", "QutoationTypes", findip(), qttype.QuotId, "Quotation Type Deleted Successfully");
                db.SaveChanges();
            }
            return true;
        }
        public ActionResult GetQuotationType(string ColName)
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
            var v = (from a in db.QuotationTypes
                     where (ColName == "" || a.QuotType == ColName)
                     select new
                     {

                         a.QuotId,
                         a.QuotType,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.QuotType.ToString().ToLower().Contains(search.ToLower()));
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
        //Used  for search the MCs
        public JsonResult SearchType(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.QuotationTypes.Where(p => p.QuotType.ToLower().Contains(q.ToLower()) || p.QuotType.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.QuotType, //each json object will have 
                                      id = b.QuotId
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.QuotationTypes.Select(b => new SelectFormat
                {
                    text = b.QuotType, //each json object will have 
                    id = b.QuotId
                }).OrderBy(b => b.text).ToList();

            }//          

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }


    }
}

           