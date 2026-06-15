using QuickSoft.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
using QuickSoft.ViewModel;
using System.Collections.Generic;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class CurrencyMasterController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public CurrencyMasterController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        [HttpGet]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Sales Entry")]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Sales Entry")]
        public JsonResult GetCurrency()
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
            var uEdit = User.IsInRole("Edit Currency");
            var uDelete = User.IsInRole("Delete Currency");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = db.CurrencyMasters.Select(b => new
            {
                Id = b.Id,
                CurrencyCode = b.CurrencyCode,
                Description = b.Description,
                ConvertionRate = b.ConvertionRate,
                b.editable,
                Dev = uDev,
                Edit = uEdit,
                Delete = uDelete
            });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.Id.ToString().ToLower().Contains(search.ToLower()));
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

        [HttpGet]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Sales Entry")]
        public ActionResult Create()
        {
            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            return PartialView();
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Sales Entry")]
        //[ValidateAntiForgeryToken]
        public JsonResult Create(CurrencyMasterViewModel Design)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (!ModelState.IsValid)
            {
                var modelErrors = new List<string>();
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var modelError in modelState.Errors)
                    {
                        modelErrors.Add(modelError.ErrorMessage);
                    }
                }
            }
            if (ModelState.IsValid)
            {
                var Exists = db.CurrencyMasters.Any(c => c.CurrencyCode == Design.CurrencyCode);
                if (Exists)
                {
                    msg = "Currency Name already exists.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    long Branch = 0;

                    var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                    var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                    if (BranchCheck == Status.active)
                    {
                        Branch = Design.Branch;
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();                        
                    }
                    var today = Convert.ToDateTime(System.DateTime.Now);
                    var desg = new CurrencyMaster
                    {
                        CurrencyCode = Design.CurrencyCode,
                        Description = Design.Description,
                        ConvertionRate = Design.ConvertionRate.ToString(),
                        Fraction = Design.Fraction,
                        Symbol = Design.Symbol,
                        MinConvertionRate = Design.MinConvertionRate.ToString(),
                        MaxConvertionRate = Design.MaxConvertionRate.ToString(),
                        Branch = Branch,
                        Status = Status.active,
                        CreatedBy = UserId,
                        CreatedDate = today,
                        editable = choice.Yes

                    };
                    db.CurrencyMasters.Add(desg);
                    db.SaveChanges();
                    Id = desg.Id;

                    com.addlog(LogTypes.Created, UserId, "Currencymaster", "CurrencyMasters", findip(), Id, "Currency Added Successfully");

                    msg = "Successfully Added Currency Details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id } };
        }

        [HttpGet]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Sales Entry")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            CurrencyMaster Cm = db.CurrencyMasters.Find(id);

            if (Cm == null)
            {
                return NotFound();
            }
            return PartialView(Cm);
        }


        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Sales Entry")]
        public JsonResult Edit(long id, CurrencyMaster Cm)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.CurrencyMasters.Any(c => c.CurrencyCode == Cm.CurrencyCode && c.Id != id);
                if (Exists)
                {
                    msg = "Currency Already exists.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    long Branch = 0;

                    var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                    var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                    if (BranchCheck == Status.active)
                    {
                        Branch = Cm.Branch;
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    }
                    CurrencyMaster ncm = db.CurrencyMasters.Find(id);
                    ncm.CurrencyCode = Cm.CurrencyCode;
                    ncm.Description = Cm.Description;
                    ncm.ConvertionRate = Cm.ConvertionRate.ToString();
                    ncm.Fraction = Cm.Fraction;
                    ncm.Symbol = Cm.Symbol;
                    ncm.MinConvertionRate = Cm.MinConvertionRate!=null? Cm.MinConvertionRate.ToString():"";
                    ncm.MaxConvertionRate = Cm.MaxConvertionRate != null ? Cm.MaxConvertionRate.ToString() : "";
                    ncm.Branch = Branch;
                    db.SaveChanges();

                    com.addlog(LogTypes.Updated, UserId, "CurrencyMaster", "CurrencyMasters", findip(), id, "Currency Master Updated Successfully");

                    msg = "Successfully Updated Currency Master Details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpGet]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Sales Entry")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CurrencyMaster Desginfo = db.CurrencyMasters.Find(id);
            if (Desginfo == null)
            {
                return NotFound();
            }

            return PartialView(Desginfo);
        }


        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Sales Entry")]
        public JsonResult Delete(long id, IFormCollection collection)
        {
            bool stat = false;
            string msg;

            CurrencyMaster Desginfo = db.CurrencyMasters.Find(id);
            if (Desginfo.CurrencyCode == "AED")
            {
                stat = false;
                msg = "Sorry.. Default Currency Cannot Delete";
            }
            else
            {
                var Msg = chkDeleteWithMsg(id);
                if (Msg != null)
                {
                    msg = Msg;
                    stat = false;
                }
                else
                {
                    if (Desginfo != null)
                    {
                        db.CurrencyMasters.RemoveRange(db.CurrencyMasters.Where(a => a.Id == id));
                    }                    
                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Deleted, UserId, "CurrencyMaster", "CurrencyMasters", findip(), Desginfo.Id, "Currency Deleted Successfully");
                    db.SaveChanges();
                    stat = true;
                    msg = "Successfully Deleted Currency masters  details.";
                }
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            if (db.Items.Any(c => c.Currency == id))
            {
                msg = "Currency Already used in Item !!";
            }
            else if (db.SalesEntrys.Any(c => c.Currency == id))
            {
                msg = "Currency Already used in Sales Entry !!";
            }
            else if (db.PurchaseEntrys.Any(c => c.Currency == id))
            {
                msg = "Currency Already used in Purchase Entry !!";
            }
            else
            {
                msg = null;
            }

            return msg;
        }


        public JsonResult SearchCurrency(string q, string x)
        {
            List<SelectMultiFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.CurrencyMasters.Where(p => p.CurrencyCode.ToLower().Contains(q.ToLower()) || p.CurrencyCode.Contains(q))
                              .Select(b => new SelectMultiFormat
                              {
                                  text = b.CurrencyCode,
                                  id = b.Id,
                                  Name = b.ConvertionRate.ToString()
                              })
                              .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.CurrencyMasters.Select(b => new SelectMultiFormat
                {
                    text = b.CurrencyCode,
                    id = b.Id,
                    Name = b.ConvertionRate.ToString()
                }).OrderBy(b => b.text).ToList();

            }
            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectMultiFormat() { id = -1, text = stt, Name = x };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }

        [HttpGet]
        public JsonResult GetCurrencyById(int CId)
        {
            var v = (from a in db.CurrencyMasters
                     where a.Id == CId
                     select new
                     {
                         a.Id,
                         a.CurrencyCode,
                         a.ConvertionRate,
                     }).FirstOrDefault();
            return Json(v);
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