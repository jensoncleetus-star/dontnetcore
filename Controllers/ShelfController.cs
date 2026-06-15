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
    public class ShelfController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ShelfController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Rack
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            return PartialView();
        }
        [HttpPost]
        public ActionResult GetShelf(string ColName)
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
            var v = (from a in db.Shelves
                     where (ColName == "" || a.shelfName == ColName)
                     select new
                     {

                         a.ShelfId,
                         a.shelfName,
                         //Dev = uDev,
                         //Edit = uEdit,
                         //Delete = uDelete
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.shelfName.ToString().ToLower().Contains(search.ToLower()));
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Create([Bind("ShelfId,shelfName")] Shelf Lead)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.Shelves.Any(c => c.shelfName == Lead.shelfName);
                if (Exists)
                {
                    msg = "Details already exists.";
                    stat = false;
                }
                else
                {
                    db.Shelves.Add(Lead);
                    db.SaveChanges();
                    Id = Lead.ShelfId;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "Shelf", "Shelfs", findip(), Lead.ShelfId, "Shelf Name Added Successfully");

                    msg = "Shelf Name added successfully.";
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
            Shelf Lead = db.Shelves.Find(id);
            if (Lead == null)
            {
                return NotFound();
            }
            return PartialView(Lead);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public JsonResult Edit([Bind("ShelfId,shelfName")] Shelf Lead)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.Shelves.Any(c => c.shelfName == Lead.shelfName && c.ShelfId != Lead.ShelfId);
                if (Exists)
                {
                    msg = "Shelf Name already exists.";
                    stat = false;
                }
                else
                {
                    db.Entry(Lead).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "Shelf", "Shelf", findip(), Lead.ShelfId, "Shelf Details Updated Successfully");


                    msg = "Successfully updated Shelf details.";
                    stat = true;
                }
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
            Shelf Lead = db.Shelves.Find(id);
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
            var Exists = db.Shelves.Any(c => c.ShelfId == id);
            if (Exists)
            {

                stat = DeleteFn(id);
                msg = "Successfully deleted Shelf.";
            }
            else
            {
                msg = "Unable to delete Shelf.";
                stat = false;
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
            Shelf Lead = db.Shelves.Find(id);
            if (Lead != null)
            {
                db.Shelves.RemoveRange(db.Shelves.Where(a => a.ShelfId == id));


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "Shelf", "Shelfs", findip(), Lead.ShelfId, "Shelf Deleted Successfully");
                db.SaveChanges();
            }
            return true;
        }
        public JsonResult SearchShelf(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (
                                  from c in db.Shelves

                                  where (c.shelfName.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = c.shelfName,
                                      id = c.ShelfId
                                  })
                                  .OrderBy(b => b.text).Distinct().ToList();

            }
            else
            {
                //serialisedJson = (from b in db.Customers
                //                  //where (b.Type ==  CRMCustomerType.Customer)
                //                      text = j.MobileNum, //each json object will have 
                //                      id =c.ContactID
                //                  })

                serialisedJson = (from c in db.Shelves

                                  where (c.shelfName.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = c.shelfName, //each json object will have 
                                      id = c.ShelfId
                                  })
                                 .OrderBy(b => b.text).Distinct().ToList();


            }//

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
            }

            return Json(serialisedJson);
        }
    }
}
