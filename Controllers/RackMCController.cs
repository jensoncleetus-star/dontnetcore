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
    public class RackMCController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public RackMCController()
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
            ViewBag.rk = QkSelect.List(
            new List<SelectListItem>
            {
             new SelectListItem { Selected = false, Text = null, Value = null},
            }, "Value", "Text", 1);
            ViewBag.slf = QkSelect.List(
           new List<SelectListItem>
           {
             new SelectListItem { Selected = false, Text = null, Value = null},
           }, "Value", "Text", 1);
            ViewBag.mc = QkSelect.List(
            new List<SelectListItem>
            {
             new SelectListItem { Selected = false, Text = null, Value = null},
            }, "Value", "Text", 1);
            return PartialView();
        }
        [HttpPost]
        public ActionResult GetRackMC()
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
            var v = (from a in db.rackmaterialcentres
                     join b in db.Racks on a.rackid equals b.RackId into temp1
                     from b in temp1.DefaultIfEmpty()
                     join c in db.Shelves on a.shelfid equals c.ShelfId into temp2
                     from c in temp2.DefaultIfEmpty()
                     join d in db.MCs on a.mcid equals d.MCId into temp3
                     from d in temp3.DefaultIfEmpty()
                     select new
                     {

                         a.rackmcid,
                         b.RackName,
                         c.shelfName,
                         d.MCName,
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
        public JsonResult Create([Bind("rackmcid,rackid,shelfid,mcid")] rackmaterialcentre Lead)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {

                db.rackmaterialcentres.Add(Lead);
                db.SaveChanges();
                Id = Lead.rackmcid;

                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Created, UserId, "rackmaterialcentre", "rackmaterialcentres", findip(), Lead.rackmcid, "Rack MC Added Successfully");

                msg = "Rack MC added successfully.";
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
            rackmaterialcentre Lead = db.rackmaterialcentres.Find(id);
            if (Lead == null)
            {
                return NotFound();
            }
            
            var stands = db.Racks
                           .Select(s => new
                           {
                               FieldID = s.RackId,
                               FieldName = s.RackName
                           })
                            .ToList();
            ViewBag.rk = QkSelect.List(stands, "FieldID", "FieldName");


            var stands2 = db.Shelves
               .Select(s => new
               {
                   FieldID = s.ShelfId,
                   FieldName = s.shelfName
               })
                .ToList();
            ViewBag.slf = QkSelect.List(stands2, "FieldID", "FieldName");
          
            var stands3 = db.MCs
          .Select(s => new
          {
              FieldID = s.MCId,
              FieldName = s.MCName
          })
           .ToList();
            ViewBag.mc = QkSelect.List(stands3, "FieldID", "FieldName");
            return PartialView(Lead);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public JsonResult Edit([Bind("rackmcid,rackid,shelfid,mcid")] rackmaterialcentre Lead)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {


                db.Entry(Lead).State = EntityState.Modified;
                db.SaveChanges();

                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Updated, UserId, "rackmaterialcentre", "rackmaterialcentres", findip(), Lead.rackmcid, "Rack MC Updated Successfully");


                msg = "Successfully updated Rack MC details.";
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
            rackmaterialcentre Lead = db.rackmaterialcentres.Find(id);
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
            var Exists = db.rackmaterialcentres.Any(c => c.rackmcid == id);
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
            rackmaterialcentre Lead = db.rackmaterialcentres.Find(id);
            if (Lead != null)
            {
                db.rackmaterialcentres.RemoveRange(db.rackmaterialcentres.Where(a => a.rackmcid == id));


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "rackmaterialcentre", "rackmaterialcentres", findip(), Lead.rackmcid, "Rack MC Deleted Successfully");
                db.SaveChanges();
            }
            return true;
        }
        public JsonResult SearchMC(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (
                                  from c in db.MCs

                                  where (c.MCName.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = c.MCName,
                                      id = c.MCId
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

                serialisedJson = (from c in db.MCs

                                  where (c.MCName.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = c.MCName, //each json object will have 
                                      id = c.MCId
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
