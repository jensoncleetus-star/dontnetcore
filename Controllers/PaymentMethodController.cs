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
using Microsoft.AspNetCore.Identity;
using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class PaymentMethodController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PaymentMethodController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }


        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,PaymentMethod")]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,PaymentMethod")]
        public ActionResult action()
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
            var uEdit = User.IsInRole("Edit PaymentMethod");
            var uDelete = User.IsInRole("Delete PaymentMethod");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.PaymentMethods
                     join b in db.Users on a.CreatedBy equals b.Id
                     join c in db.Accountss on a.AccountId equals c.AccountsID
                     select new
                     {
                         id = a.PaymentMethodId,
                         a.MethodName,
                         AccName=c.Name,
                         a.AccountId,
                         a.CreatedBy,
                         a.CreatedDate,
                         a.editable,
                         a.Status,
                         User = b.UserName,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete

                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.MethodName.ToString().ToLower().Contains(search.ToLower()));
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
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Create PaymentMethod")]
        public ActionResult Create()
        {
            //    ID = r.AccountsID,
            //    Name = r.Name

            return PartialView();
        }
        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Create PaymentMethod")]
        public JsonResult Create(PaymentMethod Vmodel)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            var payMethod = db.PaymentMethods.Any(u =>u.MethodName == Vmodel.MethodName);
            if (payMethod)
            {
                msg = "A Pay Method with same Account exists.";
                stat = false;
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var UserId = User.Identity.GetUserId();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                    PaymentMethod pay = new PaymentMethod();
                    pay.AccountId = Vmodel.AccountId;
                    pay.MethodName = Vmodel.MethodName;
                    pay.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                    pay.CreatedBy = User.Identity.GetUserId();
                    pay.Branch = BranchID;
                    pay.editable = choice.Yes;
                    pay.Status = Status.active;

                    db.PaymentMethods.Add(pay);
                    db.SaveChanges();
                    Id = pay.PaymentMethodId;

                    msg = "Pay Method added successfully.";
                    stat = true;
                    com.addlog(LogTypes.Created, UserId, "PaymentMethod", "PaymentMethods", findip(), Id, "Pay Method Added Successfully");

                }
                else
                {
                    msg = "Looks like something went wrong. Please check your form.";
                    stat = false;
                }
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit PaymentMethod")]
        public ActionResult Edit(long id)
        {
            var acc = db.Accountss.Where(p => p.Group == 8).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.Account = QkSelect.List(acc, "ID", "Name");

            PaymentMethod Vmodel = db.PaymentMethods.Find(id);
            if (Vmodel == null)
            {
                return NotFound();
            }
            return PartialView(Vmodel);
        }
        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit PaymentMethod")]
        public JsonResult Edit(long Id, PaymentMethod Vmodel)
        {
            bool stat = false;
            string msg;

            var paymethod = db.PaymentMethods.Where(a => a.PaymentMethodId == Id).SingleOrDefault();
            var paysale = db.SalesEntrys.Any(u => u.PaymentMethod == Id);

            var payMethod = db.PaymentMethods.Any(u =>u.MethodName == Vmodel.MethodName && u.PaymentMethodId != Id);
            if (payMethod)
            {
                msg = "A Payment Method with same Account exists.";
                stat = false;
            }
            else if (paysale && (Vmodel.AccountId != paymethod.AccountId))
            {
                msg = "Unable to Change Bank Account in Payment Method, Sales with this Payment Method exists.";
                stat = false;
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var UserId = User.Identity.GetUserId();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                    PaymentMethod pay =db.PaymentMethods.Find(Id);
                    pay.AccountId = Vmodel.AccountId;
                    pay.MethodName = Vmodel.MethodName;

                    db.Entry(pay).State = EntityState.Modified;
                    db.SaveChanges();

                    msg = "Pay Method Updated successfully.";
                    stat = true;
                    com.addlog(LogTypes.Updated, UserId, "PaymentMethod", "PaymentMethods", findip(), Id, "Pay Method Updated Successfully");

                }
                else
                {
                    msg = "Looks like something went wrong. Please check your form.";
                    stat = false;
                }
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };

        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete PaymentMethod")]
        public ActionResult Delete(long id)
        {
            PaymentMethod Vmodel = db.PaymentMethods.Find(id);
            if (Vmodel == null)
            {
                return NotFound();
            }
            return PartialView();
        }
        [HttpPost, ActionName("Delete")]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete PaymentMethod")]
        public JsonResult DeleteConfirm(long id)
        {
            bool stat = false;
            string msg;
            PaymentMethod Vmodel = db.PaymentMethods.Find(id);
            if (Vmodel == null)
            {
                msg = "Unable to delete Payment Method.";
                stat = false;
            }
            var Exists = db.SalesEntrys.Any(c => c.PaymentMethod == Vmodel.PaymentMethodId);
            if (Exists)
            {
                msg = "Unable to delete Payment Method, Sales with this Payment Method exists.";
                stat = false;
            }
            else
            {
                var UserId = User.Identity.GetUserId();

                db.PaymentMethods.Remove(Vmodel);
                db.SaveChanges();
                stat = true;
                msg = "Successfully deleted Payment Method details.";

                com.addlog(LogTypes.Deleted, UserId, "PaymentMethod", "PaymentMethods", findip(), Vmodel.PaymentMethodId, "Payment Method Deleted Successfully");

            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        public JsonResult SearchPayMethod(string q, string x)
        {

            List<SelectFormat> serialisedJson;
            string stt = "Cash";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.PaymentMethods.Where(p => (p.MethodName.ToLower().Contains(q.ToLower()) || p.MethodName.Contains(q)))
                              .Select(b => new SelectFormat
                              {
                                  text = b.MethodName, //each json object will have 
                                  id = b.PaymentMethodId
                              })
                              .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.PaymentMethods
                              .Select(b => new SelectFormat
                              {
                                  text = b.MethodName, //each json object will have 
                                  id = b.PaymentMethodId
                              })
                              .OrderBy(b => b.text).ToList();

            }
            if (x == "cash" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }



        private void addPayMethod(string type, Status stat, long? accId)
        {
            var payid = db.PaymentMethods.Where(c => c.MethodName == type).Select(c => c.PaymentMethodId).FirstOrDefault();
            if (payid > 0)
            {
                PaymentMethod paymethod = db.PaymentMethods.Find(payid);
                paymethod.AccountId = accId;
                paymethod.MethodName = type;
                paymethod.Status = stat;

                db.Entry(paymethod).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                PaymentMethod paymt = new PaymentMethod();
                paymt.AccountId = accId;
                paymt.MethodName = type;
                paymt.Status = stat;

                db.PaymentMethods.Add(paymt);
                db.SaveChanges();
            }

        }

        [HttpGet]
        public ActionResult GetPayMethod()
        {
            var paytype = db.PaymentMethods.Where(a => a.Status == Status.active).Select(a => a.MethodName).ToList();
            return Json(paytype);

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
