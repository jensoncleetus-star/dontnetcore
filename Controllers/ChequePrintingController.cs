using QuickSoft.Web;
using System.Linq.Dynamic.Core;
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
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic;
using System.Net;
using System.Globalization;

namespace QuickSoft.Controllers
{
    public class ChequePrintingController : BaseController
    {
        // GET: ChequePrinting
        ApplicationDbContext db;
        Common com;
        public ChequePrintingController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        #region Cheque Printing 
        
        public ActionResult Create()
        {

            var Entry = new ChequePrintingViewModel();

            Entry.ScaleMode = "Inches";
            Entry.PrintingMode = "LandScape";
            Entry.PrinterConfiguration = "N";
            Entry.TopMargin = 2;
            Entry.LeftMargin = 3;
            Entry.ChequeHeight = 400;
            Entry.ChequeWidth = 100;

            var TemplateName = db.ChequePrintings
                .Select(s => new
                {
                    Id = s.ChequePrintingId,
                    FormateName = s.FormateName
                }).ToList();

            ViewBag.Template = QkSelect.List(TemplateName, "Id", "FormateName");

            return View(Entry);



        }

        #endregion
        public JsonResult loadvoucher(string voucherno)
        { 
            Payment pay = db.Payments.Where(x => x.VoucherNo == voucherno).FirstOrDefault();
            var PaidTo = db.Accountss.Where(a => a.AccountsID == pay.PayTo).
                              Select(r => new
                              {
                                  
                                  Name = r.Name,
                                 r.Alias
                              }).FirstOrDefault();
            string printname = PaidTo.Alias == null ? PaidTo.Name : PaidTo.Alias;
            return Json(new {  payment = pay,payto= printname });

        }
        public ActionResult checquelist()
        {
            var loc = db.chequeStatuses
                .Select(s => new
                {
                    ID = s.ChequeStatusName,
                    Name = s.ChequeStatusName
                }).Distinct().OrderBy(o => o.Name).ToList();
            ViewBag.statusname = QkSelect.List(loc, "ID", "Name");
            var bank = (from b in db.PDCs
                        join c in db.Banks on b.Bank equals c.BankId.ToString()
                        select new
                        {
                            ID = b.Bank,
                            Name = b.Bank
                        }
                     ).ToList();


            var BusinessType = db.EnableSettings.Where(c => c.EnableType == "BusinessType").Select(a => a.TypeValue).SingleOrDefault();
            ViewBag.BusinessType = BusinessType;
            ViewBag.Bank = QkSelect.List(bank, "ID", "Name");

            ViewBag.Customer = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                              }, "Value", "Text", 1);
            ViewBag.Supplier = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);
            ViewBag.Alldata = QkSelect.List(
              new List<SelectListItem>
              {
                   new SelectListItem { Selected = true, Text = "All", Value = "0"},
              }, "Value", "Text", 0);
            if (BusinessType == "Property")
            {
                ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value="all"},
                        new SelectListItem() {Text="PropertyRegistrations",Value="PropertyRegistrations"},
                        new SelectListItem() { Text = "TenancyContract", Value = "TenancyContract" },
                        new SelectListItem() { Text = "Maintenance", Value = "Maintenance" },

                        new SelectListItem() {Text = "Payments", Value="Payments"},
                        new SelectListItem() {Text = "Reciepts", Value="Reciepts"},
                        new SelectListItem() {Text = "Journals", Value="Journals"}
                        }, "Value", "Text");
            }
            else
            {
                ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value="all"},
                        new SelectListItem() {Text = "Payments", Value="Payments"},
                        new SelectListItem() {Text = "Reciepts", Value="Reciepts"},
                        new SelectListItem() {Text = "Journals", Value="Journals"},
                        }, "Value", "Text");
            }

            return View();
        }
    
        public JsonResult CreateChequePrinting(string[] entry, string action)
        {
            string msg;
            bool stat = false;

            if (ModelState.IsValid)
            {
                ChequePrinting ChequePrintingEntry = new ChequePrinting();

                var FormateName = Convert.ToString(entry[0]);

                var FormateNameExists = db.ChequePrintings.Any(u => u.FormateName == FormateName);

                if (!FormateNameExists)
                {
                    ChequePrintingEntry.FormateName = Convert.ToString(entry[0]);
                    ChequePrintingEntry.ScaleMode = Convert.ToString(entry[1]);
                    ChequePrintingEntry.PrintingMode = Convert.ToString(entry[2]);
                    ChequePrintingEntry.TopMargin = Convert.ToDouble(entry[3]);
                    ChequePrintingEntry.LeftMargin = Convert.ToDouble(entry[4]);
                    ChequePrintingEntry.ChequeWidth = Convert.ToDouble(entry[5]);
                    ChequePrintingEntry.ChequeHeight = Convert.ToDouble(entry[6]);


                    db.ChequePrintings.Add(ChequePrintingEntry);
                    db.SaveChanges();

                    var id = db.ChequePrintings.Where(a => a.FormateName == FormateName).FirstOrDefault();

                    ChequeDesign Design = new ChequeDesign();
                    Design.ChequePrintingId = ChequePrintingEntry.ChequePrintingId;
                    Design.BankName = "Bank Name";
                    Design.BankTop = 7;
                    Design.BankLeft = 150;
                    Design.PayToTo = "PayTo";
                    Design.PayToLeft = 9;
                    Design.PayToTop = 65;
                    Design.Date = "Date";
                    Design.DateLeft = 265;
                    Design.DateTop = 9;
                    Design.Amount = "Amount";
                    Design.AmountLeft = 150;
                    Design.AmountTop = 65;
                    Design.ChequeNo = "ChequeNo";
                    Design.ChequeNoLeft = 9;
                    Design.ChequeNoTop = 7;

                    db.ChequeDesigns.Add(Design);
                    db.SaveChanges();

                    stat = true;
                    msg = "  Saved  in default design ";
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }

                else
                {
                    stat = false;
                    msg = "Same Formate Name exists please enter new Formate Name.";

                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form..";

                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }



        }

   
        public ActionResult Index()
        {
            var TemplateName = db.ChequePrintings
                .Select(s => new
                {
                    Id = s.ChequePrintingId,
                    FormateName = s.FormateName
                }).ToList();

            ViewBag.Template = QkSelect.List(TemplateName, "Id", "FormateName");

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjCheck = ProjectCheck;


            ViewBag.Prjct = QkSelect.List(
                              new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = true, Text = "All", Value = null},
                               }, "Value", "Text", 1);



            var created = db.Users.Select(s => new
            {
                ID = s.Id,
                Name = s.UserName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");

            return View();


        }


        [HttpPost]
        public ActionResult GetChequePrinting(string FormateName)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;


            string search = Request.Form.GetValues("search[value]")[0];
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();

            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;


            var UserId = User.Identity.GetUserId();

            var v = (from b in db.ChequePrintings

                         //where (FormateName == null || FormateName == "" || b.FormateName == FormateName)
            select new
                     {
                         b.ChequePrintingId,
                         b.FormateName,
                         b.ChequeHeight,
                         b.ChequeWidth,
                         b.ScaleMode,
                         b.PrintingMode,
                         b.TopMargin,
                         b.LeftMargin,
                         b.PrinterConfiguration


                     });


            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.FormateName.ToString().ToLower().Contains(search.ToLower())

               );
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

        public ActionResult DeleteAllPrintTemplate(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = DeleteChequeTemplate(arr);
                if (chk == true)
                {
                    count++;
                }
            }

            Success("Deleted " + count + "Cheque Formate", true);
            return RedirectToAction("Index", "ChequePrinting");
        }

        private Boolean DeleteChequeTemplate(long id)
        {
            var UserId = User.Identity.GetUserId();

            ChequePrinting QSum = db.ChequePrintings.Find(id);

            var ChequePrinting = db.ChequeDesigns.Where(a => a.ChequePrintingId == id).FirstOrDefault();
            if (ChequePrinting != null)
            {
                db.ChequeDesigns.RemoveRange(db.ChequeDesigns.Where(a => a.ChequePrintingId == id));
            }

            db.ChequePrintings.Remove(QSum);

            db.SaveChanges();
            com.addlog(LogTypes.Deleted, UserId, "ChequePrinting", "ChequePriniting", findip(), QSum.ChequePrintingId, "Successfully Deleted cheque Formate");
            return true;


        }
      


        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            var chk = DeleteChequeTemplate(id);
            if (chk == true)
            {
                stat = true;
                msg = "Successfully deleted Template.";
            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }

        public ActionResult ChequeDesign()
        {

            return View();
        }

       
        public JsonResult CreateChequeDesign(string[] entry, string action)
        {
            bool stat = false;
            string msg;

            Int64 ChequePrintingId = Convert.ToInt64(entry[0]);
            var Entryitem = db.ChequeDesigns.Where(u => u.ChequePrintingId == ChequePrintingId).Select(u => u.ChequeDesignid).FirstOrDefault();

            ChequeDesign Design = db.ChequeDesigns.Find(Entryitem);

            if (ModelState.IsValid)
            {
                Design.ChequePrintingId = Convert.ToInt64(entry[0]);

                Design.BankName = Convert.ToString(entry[1]);
                Design.BankLeft = Convert.ToDouble(entry[2]);
                Design.BankTop = Convert.ToDouble(entry[3]);

                Design.PayToTo = Convert.ToString(entry[4]);
                Design.PayToLeft = Convert.ToDouble(entry[5]);
                Design.PayToTop = Convert.ToDouble(entry[6]);

                Design.Amount = Convert.ToString(entry[7]);
                Design.AmountLeft = Convert.ToDouble(entry[8]);
                Design.AmountTop = Convert.ToDouble(entry[9]);

                Design.Date = Convert.ToString(entry[10]);
                Design.DateLeft = Convert.ToDouble(entry[11]);
                Design.DateTop = Convert.ToDouble(entry[12]);

                Design.ChequeNo = Convert.ToString(entry[13]);
                Design.ChequeNoLeft = Convert.ToDouble(entry[14]);
                Design.ChequeNoTop = Convert.ToDouble(entry[15]);

                db.Entry(Design).State = EntityState.Modified;
                db.SaveChanges();

                stat = true;
                msg = "Successfully updated Cheque printing Design.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };


            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }


        }

        [HttpPost]
        public JsonResult GetFormatePosition(long id)
        {

            var Formate = (from a in db.ChequePrintings

                           where a.ChequePrintingId == id
                           select new
                           {
                               a.FormateName,
                               a.ScaleMode,
                               a.PrintingMode,
                               a.TopMargin,
                               a.LeftMargin,
                               a.ChequeHeight,
                               a.ChequeWidth

                           }).FirstOrDefault();

            return Json(Formate);

        }

        [HttpPost]
        public JsonResult AddDesignPosition(long id)
        {

            var Formate = (from a in db.ChequeDesigns

                           where a.ChequePrintingId == id
                           select new
                           {
                               a.BankName,
                               a.BankLeft,
                               a.BankTop,
                               a.PayToTo,
                               a.PayToLeft,
                               a.PayToTop,
                               a.Amount,
                               a.AmountLeft,
                               a.AmountTop,
                               a.Date,
                               a.DateLeft,
                               a.DateTop,
                               a.ChequeNo,
                               a.ChequeNoLeft,
                               a.ChequeNoTop,
                           }).FirstOrDefault();

            return Json(Formate);

        }

        public ActionResult _chequeLayout()
        {
            return View();
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
