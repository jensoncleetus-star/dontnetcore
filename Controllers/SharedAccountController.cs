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
using System.Text;
using System.Data;
using System.Globalization;
using QuickSoft.ViewModel;

namespace QuickSoft.Controllers
{
    public class SharedAccountController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public SharedAccountController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: SharedAccount
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create()
        {
            ViewBag.Account = QkSelect.List(
                          new List<SelectListItem>
                          {
                             new SelectListItem { Selected = false, Text = "Select Account", Value = ""},
                          }, "Value", "Text", 1);
            ViewBag.MC = QkSelect.List(
                           new List<SelectListItem>
                           {
                             new SelectListItem { Selected = false, Text = "Select Account", Value = ""},
                           }, "Value", "Text", 1);
            return PartialView();
        }
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            sharedaccount Lead = db.sharedaccounts.Find(id);
            var use = db.Accountss.Select(s => new { ID = s.AccountsID, Name = s.Name }).ToList();
            ViewBag.Account = QkSelect.List(use, "ID", "Name");
            var use2 = db.MCs.Select(s => new { ID = s.MCId, Name = s.MCName }).ToList();
            ViewBag.MC = QkSelect.List(use2, "ID", "Name");
            if (Lead == null)
            {
                return NotFound();
            }
            return PartialView(Lead);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public JsonResult Edit([Bind("accsharedid,accountid,mcid,percentage")] sharedaccount Lead)
        {
            bool stat = false;
            string msg;
            sharedaccount shared = new sharedaccount();
            if (ModelState.IsValid)
            {
                var Exists = db.sharedaccounts.Any(c => c.accountid == Lead.accountid && c.mcid==Lead.mcid && c.accsharedid != Lead.accsharedid);
                if (Exists)
                {
                    msg = " already exists.";
                    stat = false;
                }
                else
                {
                    var sharedacc = db.sharedaccounts.Where(a => a.accsharedid == Lead.accsharedid).Select(a => a.accountid).FirstOrDefault();
                    if(sharedacc!=Lead.accountid)
                    {
                        var checkexistingacc = db.sharedaccounts.Where(a => a.accountid == sharedacc && a.accsharedid != Lead.accsharedid).FirstOrDefault();
                        if(checkexistingacc == null)
                        {
                            Accounts acc1 = db.Accountss.Find(sharedacc);
                            acc1.shared = null;
                            db.Entry(acc1).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                

                    db.Entry(Lead).State = EntityState.Modified;
                    db.SaveChanges();
                    Accounts acc = db.Accountss.Find(Lead.accountid);
                    acc.shared = 1;
                    db.Entry(acc).State = EntityState.Modified;
                    db.SaveChanges();


                    msg = "Successfully updated Shared Account details.";
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
            sharedaccount Lead = db.sharedaccounts.Find(id);
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
            var Exists = db.sharedaccounts.Any(c => c.accsharedid == id);
            if (Exists)
            {

                stat = DeleteFn(id);
                msg = "Successfully deleted Shared Account.";
            }
            else
            {
                msg = "Unable to delete the Ahared Account.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public bool DeleteFn(long id)
        {
            sharedaccount Lead = db.sharedaccounts.Find(id);
            if (Lead != null)
            {
                var checkexistingacc = db.sharedaccounts.Where(a => a.accountid == Lead.accountid && a.accsharedid != Lead.accsharedid).FirstOrDefault();
                if (checkexistingacc == null)
                {
                    Accounts acc1 = db.Accountss.Find(Lead.accountid);
                    acc1.shared = null;
                    db.Entry(acc1).State = EntityState.Modified;
                    db.SaveChanges();
                }
                db.sharedaccounts.RemoveRange(db.sharedaccounts.Where(a => a.accsharedid == id));


                db.SaveChanges();
            }
            return true;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Create LeadType")]
        public JsonResult Create([Bind("accsharedid,accountid,mcid,percentage")] sharedaccount Lead)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            var accID = Lead.accountid;
            if (ModelState.IsValid)
            {
                var Exists = db.sharedaccounts.Any(c => c.accountid == Lead.accountid && c.mcid==Lead.mcid);
                if (Exists)
                {
                    msg = "Details already exists.";
                    stat = false;
                }
                else
                {
                    db.sharedaccounts.Add(Lead);
                    db.SaveChanges();
                    Id = Lead.accsharedid;
                    Accounts acc = db.Accountss.Find(Lead.accountid);
                    acc.shared = 1;
                    db.Entry(acc).State = EntityState.Modified;
                    db.SaveChanges();

                    msg = "Shared Account Created successfully.";
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
        [HttpPost]
        //   [QkAuthorize(Roles = "Dev,Lead Type")]
        public ActionResult GetSharedAccount(string ColName)
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
            var v = (from a in db.sharedaccounts
                     join b in db.Accountss on a.accountid equals b.AccountsID into temp1
                     from b in temp1.DefaultIfEmpty()
                     join c in db.MCs on a.mcid equals c.MCId into temp2
                     from c in temp2.DefaultIfEmpty()
                     select new
                     {

                         a.accsharedid,
                         a.accountid,
                         a.mcid,
                         a.percentage,
                         b.Name,
                         c.MCName,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.Name.ToString().ToLower().Contains(search.ToLower()) || p.MCName.ToString().ToLower().Contains(search.ToLower()));
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
    }
}
