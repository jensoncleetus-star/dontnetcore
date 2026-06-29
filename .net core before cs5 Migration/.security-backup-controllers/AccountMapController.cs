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

    public class AccountMapController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public AccountMapController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
   
        [QkAuthorize(Roles = "Dev,Delete Sales Entry")]
        public ActionResult keygenerate()
        {
            keytableview mview = new keytableview();
            Random r = new Random();
            var x = r.Next(0, 1000000);
            string s = x.ToString("000000");
            mview.keyvalue = s;
            ViewBag.Empl = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = false, Text = null, Value = null},
                          }, "Value", "Text", 1);
            return View(mview);

        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Sales Entry")]
        public ActionResult keygenerate(keytableview mv)
        {
            mv.entrytime = DateTime.Now;
            db.keytableviews.Add(mv);
            db.SaveChanges();
            ViewBag.Empl = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = false, Text = null, Value = null},
                          }, "Value", "Text", 1);
            Success("Saved success fully key:" + mv.keyvalue);
            return View(mv);

        }

        public ActionResult Index()
        {
          
            return View();
        }
        [HttpPost]
        public ActionResult GetAccountMap()
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
            var uEdit = User.IsInRole("Edit Location");
            var uDelete = User.IsInRole("Delete Location");


            var v = (from a in db.accountmaps
                     join b in db.Accountss on a.AccountId equals b.AccountsID 
                     join c in db.Employees on a.EmployeeId equals c.EmployeeId
                   
                     select new
                     {
                        b.Name,
                        c.FirstName,
                        a.AccountMapId,
                        a.description,
                         Employees = (from z in db.accountmaps
                                      join y in db.Employees on z.EmployeeId equals y.EmployeeId
                                      where z.AccountId == a.AccountId && z.description==a.description
                                      select new
                                      {
                                          id = y.EmployeeId,
                                          LastName = (y.LastName != null) ? y.LastName : "",
                                          FirstName = (y.FirstName != null) ? y.FirstName : ""
                                      }).ToList(),
                        
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete,
                         a.level


                     }).ToList().OrderBy(o=>o.level).Select(o => new {
                         o.Name,
                         o.FirstName,
                         o.AccountMapId,
                         o.Employees,
                         o.description,
                         o.Dev,
                         o.Edit,
                         o.Delete,
                         o.level
                     }).GroupBy(x => new { x.Name, x.description }, (key, g) => g.OrderBy(m => m.level).FirstOrDefault());
            

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.FirstName.ToString().ToLower().Contains(search.ToLower())||
                p.Name.ToString().ToLower().Contains(search.ToLower())
                );
            }

            //SORT


            var data = v.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        public ActionResult Create()
        {

          
                var accounts = db.Accountss.Where(o=>o.Group==9||o.Group==13|| o.Group == 8)
                             .Select(s => new
                             {
                                 FieldID = s.AccountsID,
                                 FieldName = s.Name
                             })
                              .ToList();
                ViewBag.accounts = QkSelect.List(accounts,  "FieldID", "FieldName");
            //      .Select(s => new
            //          FieldID = s.EmployeeId,
            //          FieldName = s.FirstName
            //      })
            ViewBag.Employee = QkSelect.List(
                                       new List<SelectListItem>
                                       {
                                            new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                       }, "Value", "Text", 1);
            return PartialView();



        }

        // POST:Location/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Create Item Size")]
        public JsonResult Create(accountmapviewmodal accmap)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;

            if (ModelState.IsValid)
            {
                foreach (var arr in accmap.EmployeeId)
                {

                    accountmap obj = new accountmap
                    {
                        AccountId = accmap.AccountId,
                        PaymentTypeId = accmap.PaymentTypeId,
                        EmployeeId = arr,
                        description=accmap.description,
                        level=accmap.level,
                        notintaxinvoice=accmap.notintaxinvoice


                    };

                    db.accountmaps.Add(obj);
                }
                db.SaveChanges();

                msg = "Account Map added successfully.";
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
            accountmap accmap = db.accountmaps.Find(id);
            if (accmap == null)
            {
                return NotFound();
            }

            var accounts = db.Accountss.Where(o => o.Group == 9 || o.Group == 13|| o.Group ==8)
                            .Select(s => new
                            {
                                FieldID = s.AccountsID,
                                FieldName = s.Name
                            })
                             .ToList();
            ViewBag.accounts = QkSelect.List(accounts, "FieldID", "FieldName");
            //      .Select(s => new
            //          FieldID = s.EmployeeId,
            //          FieldName = s.FirstName
            //      })
            
            var emp=(from a in db.Employees
                     join b in db.accountmaps on a.EmployeeId equals b.EmployeeId
                     where b.AccountId==accmap.AccountId && b.description==accmap.description
                          select new{
                             
                           
                               FieldID = a.EmployeeId,
                               FieldName = a.FirstName
                           })
                            .ToList();
            ViewBag.employee = QkSelect.List(emp, "FieldID", "FieldName");

            //Getting the Material Centre Name
            ViewBag.AccountName =   db.Accountss.Where(o => o.AccountsID == accmap.AccountId).Select(o => o.Name).FirstOrDefault();
            accountmapviewmodal acm = new accountmapviewmodal
            {
                AccountId = accmap.AccountId,
                EmployeeId = emp.Select(o => o.FieldID).ToArray(),
                PaymentTypeId = accmap.PaymentTypeId,
                description=accmap.description,
                level=accmap.level,
                notintaxinvoice =accmap.notintaxinvoice
                
            };
            return PartialView(acm);
        }



        // POST: ItemCategory/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Edit Item Size")]
        public JsonResult Edit(accountmapviewmodal accmap)
        {
            bool stat = false;
            string msg;
            Int64 PrevAccountId;
            var PrevAccntName = "";

            db.accountmaps.RemoveRange(db.accountmaps.Where(o => o.AccountId == accmap.AccountId));
            db.SaveChanges();
            if (ModelState.IsValid)
            {

                PrevAccountId = db.accountmaps.Where(a => a.AccountMapId == accmap.AccountMapId).Select(a => a.AccountId).FirstOrDefault();

                foreach (var arr in accmap.EmployeeId)
                {

                    accountmap obj = new accountmap
                    {
                        AccountId = accmap.AccountId,
                        PaymentTypeId = accmap.PaymentTypeId,
                        EmployeeId = arr,
                        description=accmap.description,
                        level=accmap.level,
                        notintaxinvoice=accmap.notintaxinvoice

                    };


                    db.accountmaps.Add(obj);
                    db.SaveChanges();
                }


                    msg = "Successfully updated Account  details.";
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
            accountmap accmap = db.accountmaps.Find(id);
            if (accmap == null)
            {
                return NotFound();
            }
            return PartialView(accmap);
        }

        // POST: ItemCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //  [QkAuthorize(Roles = "Dev,Delete Item Color")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            stat = DeleteFn(id);

            if(stat == true)
                msg = "Successfully deleted Account Map details...";
            else
                msg = "Unable to Delete..Account is used in Account Transaction..";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        // [QkAuthorize(Roles = "Dev,Delete Item Color")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Details, Unable to Delete " + notdel + " Details. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Details.", true);
            }
            else
            {
                Success("Deleted " + count + " Details...", true);
            }
            return RedirectToAction("Index", "AccountMap");
        }

        public bool DeleteFn(long id)
        {
            bool ReturnValue = true;

            accountmap accmap = db.accountmaps.Find(id);
            if (accmap != null)
            {
                
                    db.accountmaps.RemoveRange(db.accountmaps.Where(a => a.AccountMapId == id));
                    db.SaveChanges();
                    ReturnValue = true;
               
            }     
           
            return ReturnValue;
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
