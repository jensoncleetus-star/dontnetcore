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
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Mvc;
using CustomHtml;
using Microsoft.AspNetCore.Identity;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class PurchaseReportController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PurchaseReportController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: pursReport

     
        [QkAuthorize(Roles = "Dev,All Purchase")]
        public ActionResult Indexrebate()
        {
            ViewBag.Item = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);
            ViewBag.Category = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);
            ViewBag.Brand = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);
            ViewBag.Supplier = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            ViewBag.SalesExecutive = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);


            return View();
        }

        [QkAuthorize(Roles = "Dev,All Purchase")]
        public ActionResult PurchaseRebateSupplierWise()
        {
            ViewBag.Item = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);
            ViewBag.Category = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);
            ViewBag.Brand = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);
            ViewBag.Supplier = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            ViewBag.SalesExecutive = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);


            return View();
        }
        #region All Purchase
        [QkAuthorize(Roles = "Dev,All Purchase")]
        public ActionResult Index()
        {
            ViewBag.Prjct = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = null},
                                 }, "Value", "Text", 1);
            ViewBag.Item = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);
            ViewBag.Category = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);
            ViewBag.Brand = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);
            ViewBag.Supplier = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            ViewBag.SalesExecutive = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);


            return View();
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,All Purchase")]
        public ActionResult ViewIndex(string InvoiceNo, long? ddlSupplier, long? ddlSalesExecutive, string From, string To, long? ddlType, long? ddlTaxType, long? ddlMC)
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            if (InvoiceNo != "")
            {
                ViewBag.InvoiceNo = InvoiceNo;
            }
            else
            {
                ViewBag.InvoiceNo = "All";
            }
            ViewBag.fromdate = From;
            ViewBag.todate = To;
            companySet();
            return View();
        }

        [QkAuthorize(Roles = "Dev,Purchasereturn Reports")]
        public ActionResult AllPurchaseReturn()
        {
            ViewBag.Supplier = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            var pay = db.PaymentMethods.Select(s => new
            {
                ID = s.PaymentMethodId,
                Name = s.MethodName
            }).ToList();

            ViewBag.PayMethod = QkSelect.List(pay, "ID", "Name");
            ViewBag.SalesExecutive = QkSelect.List(
                                    new List<SelectListItem>
                                    {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                    }, "Value", "Text", 0);

            ViewBag.SalesMan = QkSelect.List(
                                    new List<SelectListItem>
                                    {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                    }, "Value", "Text", 0);
            var emp = db.Taxs.Select(s => new
            {
                Id = s.Percentage,
                Name = s.TaxName,
            }).OrderByDescending(s => s.Id).ToList();
            ViewBag.TaxType = QkSelect.List(emp, "Id", "Name");


            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();

            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            //    .Select(s => new
            //        ID = s.ProjectId,
            //        Name = s.ProjectName
            //    })

            ViewBag.getProj = QkSelect.List(
                                   new List<SelectListItem>
                                   {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                   }, "Value", "Text", 0);

            ViewBag.getProTask = QkSelect.List(
                                  new List<SelectListItem>
                                  {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                  }, "Value", "Text", 0);

            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Purchasereturn Reports")]
        public ActionResult GetAllPurchaseReturn(string srchtxt, string peno, long? supplier, string fromdate, long? SalesExecutive, string todate, long? type, long? ddlMC, long? Taxtype)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddlMC == 0 || ddlMC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }

            PurchaseReturn PReturn = new PurchaseReturn();
            if (type == 1)
            {
                PReturn.SupplierType = SupplierType.CashSale;
            }
            else if (type == 0)
            {
                PReturn.SupplierType = SupplierType.CreditSale;
            }
            else
            {

            }
            if (1==1)
            {
                Int64 temp = 501;
                var v = (from a in db.PurchaseReturns
                         join b in db.Suppliers on a.Supplier equals b.SupplierID into cust
                         from b in cust.DefaultIfEmpty()
                         join j in db.AccountsTransactions on new { j1 = a.PurchaseReturnId, j2 = "Purchase Return", j3 = temp }
                         equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                         from j in hir1.DefaultIfEmpty()
                         join d in db.Employees on a.PRCashier equals d.EmployeeId into emp
                         from d in emp.DefaultIfEmpty()
                         join e in db.MCs on a.MaterialCenter equals e.MCId into mcs
                         from e in mcs.DefaultIfEmpty()
                         join f in db.PRItemss on a.PurchaseReturnId equals f.PurchaseReturnId
                         join g in db.Items on f.Item equals g.ItemID

                         let grandtotalitmsearch = (decimal)(from m in db.PRItemss
                                                             join nn in db.Items on m.Item equals nn.ItemID
                                                             join oo in db.PurchaseReturns on m.PurchaseReturnId equals oo.PurchaseReturnId
                                                             where nn.ItemName.Contains(srchtxt)
                                                             && oo.PurchaseReturnId == a.PurchaseReturnId

                                                             select new
                                                             {
                                                                 totalprice = m.ItemUnitPrice * m.ItemQuantity
                                                             }).Sum(o => o.totalprice)


                         where (peno == "" || a.BillNo == peno) && a.Status == 1 &&
                                     (supplier == 0 || a.Supplier == supplier) &&
                                     (SalesExecutive == 0 || SalesExecutive == null || a.PRCashier == SalesExecutive) &&
                                     (type == null || a.SupplierType == PReturn.SupplierType) &&
                                     (fromdate == "" || EF.Functions.DateDiffDay(a.PRDate, fdate) <= 0) &&
                                     (todate == "" || EF.Functions.DateDiffDay(a.PRDate, tdate) >= 0)&&
                                     (srchtxt==""||g.ItemName.Contains(srchtxt)) &&
                                     ((!MCList.Any() && ddlMC == null || ddlMC == 0) || MCArray.Contains(a.MaterialCenter) || ddlMC == a.MaterialCenter) //&&
                                   //  (a.PRTax != 0)
                         select new
                         {
                             a.PurchaseReturnId,
                             a.BillNo,
                             a.PRDate,
                             PRGrandTotal = (srchtxt == "") ? a.PRGrandTotal : grandtotalitmsearch,
                             Supplier = b.SupplierName,
                             taxableAmt = (srchtxt == "") ? (a.PRSubTotal - a.PRDiscount):0,
                             EmpName = d.FirstName + " " + d.LastName,
                             MCName = (ddlMC != 0 || ddlMC != null) ? e.MCName : "All",
                             Credit = (srchtxt == "") ? (j.Credit == null ? 0 : j.Credit):0,
                             a.PRCreatedDate,
                             //a.PENo,
                             //a.PESubTotal,
                             //TaxRegNo = f.TRN,
                             //a.PayType,
                             //PaymentStatus = a.Status,
                             //a.SupplierType,
                         }).Distinct().OrderBy(a => a.PRDate);
                var data = v.Skip(skip).Take(pageSize).ToList();
                recordsTotal = v.Count();
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                var results = new ContentResult
                {
                    Content = result,
                    ContentType = "application/json"
                };
                return results;
            }
            else if (Taxtype == 1)
            {
                Int64 temp = 501;
                var v = (from a in db.PurchaseReturns
                         join b in db.Suppliers on a.Supplier equals b.SupplierID into cust
                         from b in cust.DefaultIfEmpty()
                         join j in db.AccountsTransactions on new { j1 = a.PurchaseReturnId, j2 = "Purchase Return", j3 = temp }
                         equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                         from j in hir1.DefaultIfEmpty()
                         join d in db.Employees on a.PRCashier equals d.EmployeeId into emp
                         from d in emp.DefaultIfEmpty()
                         join e in db.MCs on a.MaterialCenter equals e.MCId into mcs
                         from e in mcs.DefaultIfEmpty()

                         where (peno == "" || a.BillNo == peno) && a.Status == 1 &&
                                      (supplier == 0 || a.Supplier == supplier) &&
                                       (SalesExecutive == 0 || SalesExecutive == null || a.PRCashier == SalesExecutive) &&
                                      (type == null || a.SupplierType == PReturn.SupplierType) &&
                                      (fromdate == "" || EF.Functions.DateDiffDay(a.PRDate, fdate) <= 0) &&
                                      (todate == "" || EF.Functions.DateDiffDay(a.PRDate, tdate) >= 0) &&
                                      (ddlMC == null || ddlMC == 0 || a.MaterialCenter == ddlMC) &&
                                      (a.PRTax == 0)
                         select new
                         {
                             a.PurchaseReturnId,
                             a.BillNo,
                             a.PRDate,
                             a.PRGrandTotal,
                             Supplier = b.SupplierName,
                             taxableAmt = a.PRSubTotal - a.PRDiscount,
                             EmpName = d.FirstName + " " + d.LastName,
                             MCName = (ddlMC != 0 || ddlMC != null) ? e.MCName : "All",
                             Credit = (j.Credit == null ? 0 : j.Credit),
                             a.PRCreatedDate,
                             //a.PENo,
                             //a.PESubTotal,
                             //TaxRegNo = f.TRN,
                             //a.PayType,
                             //PaymentStatus = a.Status,
                             //a.SupplierType,
                         }).OrderBy(a => a.PRDate);
                var data = v.Skip(skip).Take(pageSize).ToList();
                recordsTotal = v.Count();
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                var results = new ContentResult
                {
                    Content = result,
                    ContentType = "application/json"
                };
                return results;
            }
            else
            {
                Int64 temp = 501;
                var v = (from a in db.PurchaseReturns
                         join b in db.Suppliers on a.Supplier equals b.SupplierID into cust
                         from b in cust.DefaultIfEmpty()
                         join j in db.AccountsTransactions on new { j1 = a.PurchaseReturnId, j2 = "Purchase Return", j3 = temp }
                         equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                         from j in hir1.DefaultIfEmpty()
                         join d in db.Employees on a.PRCashier equals d.EmployeeId into emp
                         from d in emp.DefaultIfEmpty()
                         join e in db.MCs on a.MaterialCenter equals e.MCId into mcs
                         from e in mcs.DefaultIfEmpty()

                         where (peno == "" || a.BillNo == peno) && a.Status == 1 &&
                                     (supplier == 0 || a.Supplier == supplier) &&
                                     (SalesExecutive == 0 || SalesExecutive == null || a.PRCashier == SalesExecutive) &&
                                     (type == null || a.SupplierType == PReturn.SupplierType) &&
                                     (fromdate == "" || EF.Functions.DateDiffDay(a.PRDate, fdate) <= 0) &&
                                     (todate == "" || EF.Functions.DateDiffDay(a.PRDate, tdate) >= 0)&&
                                     (ddlMC == null || ddlMC == 0 || a.MaterialCenter == ddlMC)
                         select new
                         {
                             a.PurchaseReturnId,
                             a.BillNo,
                             a.PRDate,
                             a.PRGrandTotal,
                             Supplier = b.SupplierName,
                             taxableAmt = a.PRSubTotal - a.PRDiscount,
                             EmpName = d.FirstName + " " + d.LastName,
                             MCName = (ddlMC != 0 || ddlMC != null) ? e.MCName : "All",
                             Credit = (j.Credit == null ? 0 : j.Credit),
                             a.PRCreatedDate,
                             //a.PENo,
                             //a.PESubTotal,
                             //TaxRegNo = f.TRN,
                             //a.PayType,
                             //PaymentStatus = a.Status,
                             //a.SupplierType,
                         }).OrderBy(a => a.PRDate);
                var data = v.Skip(skip).Take(pageSize).ToList();
                recordsTotal = v.Count();
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                var results = new ContentResult
                {
                    Content = result,
                    ContentType = "application/json"
                };
                return results;
            }
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,All Purchase")]
        public ActionResult GetAllpurRebate(string peno, long? supplier, string fromdate, long? SalesExecutive, string todate, long? type, long? ddlMC, long? Taxtype, string srchtxt, long? brand, long? category, long? item)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddlMC == 0 || ddlMC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            PurchaseEntry sEntry = new PurchaseEntry();
            if (type == 1)
            {
                sEntry.SupplierType = SupplierType.CashSale;
            }
            else if (type == 0)
            {
                sEntry.SupplierType = SupplierType.CreditSale;
            }
            else
            {

            }
          
                Int64 temp = 501;
                var v = (from a in db.PurchaseEntrys
                         join b in db.Suppliers on a.Supplier equals b.SupplierID into cust
                         from b in cust.DefaultIfEmpty()
                         join c in db.PEItemss on a.PurchaseEntryId equals c.PurchaseEntry
                         join d in db.Items on c.Item equals d.ItemID
                         join uu in db.ItemUnits on d.ItemUnitID equals uu.ItemUnitID
                         //let itemrebate = (from aa in db.PEItemss
                         //                  (srchtxt == "" || cc.ItemName.Contains(srchtxt))
                         //                  group new { aa.ItemQuantity, aa.ItemUnit } by new { aa.Item, aa.ItemUnitPrice } into grp
                         //                      itemid = grp.Key.Item,
                         //                      unitprice = grp.Key.ItemUnitPrice,
                         //                      itemunitid = grp.FirstOrDefault().ItemUnit
                         //                      //Total =(grp.FirstOrDefault().ItemUnit== grp.FirstOrDefault().ItemUnitID)? grp.Sum(o => o.ItemQuantity) * grp.FirstOrDefault().PurchasePrice: grp.Sum(o => o.ItemQuantity) * grp.FirstOrDefault().PurchasePrice/grp.FirstOrDefault().ConFactor
                         //                  }).ToList().Select(o => new
                         //                      itemqty = o.itemqty,
                         //                      price = o.unitprice * o.itemqty
                         //                  })



                         //group new { aa.ItemQuantity, uu.ItemUnitName, cc.PurchasePrice, cc.ItemCode, cc.ItemName, cc.ConFactor, aa.ItemUnit, cc.ItemUnitID } by aa.Item into grp
                         //    itemname = grp.FirstOrDefault().ItemCode + " - " + grp.FirstOrDefault().ItemName,
                         //    itemunit = grp.FirstOrDefault().ItemUnitName,
                         //    price = grp.Sum(o => o.ItemQuantity) * grp.FirstOrDefault().PurchasePrice


                         //}).ToList()

                         where
                         
                         (peno == "" || a.BillNo == peno) && a.Status == 1 &&
                               (supplier == 0 || a.Supplier == supplier) &&
                               (SalesExecutive == 0 || SalesExecutive == null || a.PECashier == SalesExecutive) &&
                               (type == null || a.SupplierType == sEntry.SupplierType) &&
                               (fromdate == "" || EF.Functions.DateDiffDay(a.PEDate, fdate) <= 0) &&
                               (todate == "" || EF.Functions.DateDiffDay(a.PEDate, tdate) >= 0) &&
                               (ddlMC == null || ddlMC == 0 || a.MaterialCenter == ddlMC)
                         select new
                         {
                             a.PurchaseEntryId,
                             a.PENo,
                             a.BillNo,
                             a.PEDate,
                             a.PESubTotal,
                             a.PEGrandTotal,
                             Supplier = b.SupplierName,
                             User = db.Users.Where(s => s.Id == a.CreatedBy).Select(s => s.Name).FirstOrDefault(),
                             a.PayType,
                             PaymentStatus = a.Status,
                             PaymentTrans = db.PETransactions.Any(k => k.PurchaseEntry == a.PurchaseEntryId),
                             a.SupplierType,
                             a.PECreatedDate,
                             sprice = c.ItemUnitPrice,
                             pprice = d.PurchasePrice,
                             invdate = a.PEDate,
                             qty = c.ItemQuantity,
                             unit = uu.ItemUnitName,
                             itemname = d.ItemCode + "-" + d.ItemName,
                         }).OrderBy(a => a.PEDate);
                var data = v.Skip(skip).Take(pageSize).ToList();
                recordsTotal = v.Count();
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                var results = new ContentResult
                {
                    Content = result,
                    ContentType = "application/json"
                };
                return results;
            
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,All Purchase")]
        public ActionResult GetAllpurRebateSupplierWise(string peno, long? supplier, string fromdate, long? SalesExecutive, string todate, long? type, long? ddlMC, long? Taxtype, string srchtxt, long? brand, long? category, long? item)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddlMC == 0 || ddlMC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            PurchaseEntry sEntry = new PurchaseEntry();
            if (type == 1)
            {
                sEntry.SupplierType = SupplierType.CashSale;
            }
            else if (type == 0)
            {
                sEntry.SupplierType = SupplierType.CreditSale;
            }
            else
            {

            }

            Int64 temp = 501;
            // EF Core 10 cannot translate the per-supplier `let itemrebate = (...GroupBy...).ToList().Select().Sum()`
            // nested grouped-collection projection. Split SERVER from CLIENT: materialize the qualifying PE item
            // lines flat (no grouping in the projection), group/aggregate client-side into a rebate-per-supplier
            // map, then re-project the supplier rows client-side with the SAME member names + order.
            var rebateLines = (from aa in db.PEItemss
                               join bb in db.PurchaseEntrys on aa.PurchaseEntry equals bb.PurchaseEntryId
                               join cc in db.Items on aa.Item equals cc.ItemID
                               where bb.Status == 1 &&
                               (peno == "" || bb.BillNo == peno) &&
                               (fromdate == "" || EF.Functions.DateDiffDay(bb.PEDate, fdate) <= 0) &&
                               (todate == "" || EF.Functions.DateDiffDay(bb.PEDate, tdate) >= 0) &&
                               (ddlMC == null || ddlMC == 0 || bb.MaterialCenter == ddlMC) &&
                               (SalesExecutive == 0 || SalesExecutive == null || bb.PECashier == SalesExecutive) &&
                               (type == null || bb.SupplierType == sEntry.SupplierType) &&
                               (srchtxt == "" || cc.ItemName.Contains(srchtxt))
                               select new
                               {
                                   bb.Supplier,
                                   aa.Item,
                                   aa.ItemUnitPrice,
                                   aa.ItemQuantity
                               }).ToList();

            // CLIENT-side: rebate per supplier = SUM over (Item, ItemUnitPrice) groups of (unitprice * SUM(qty)).
            var rebateBySupplier = rebateLines
                .GroupBy(l => l.Supplier)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(l => new { l.Item, l.ItemUnitPrice })
                    .Sum(grp => grp.Key.ItemUnitPrice * grp.Sum(o => o.ItemQuantity)));

            var suppliers = db.Suppliers
                .Where(a => (supplier == 0 || a.SupplierID == supplier))
                .Select(a => new { a.SupplierID, a.SupplierName })
                .ToList();

            var v2 = suppliers.Select(a => new
                     {
                         a.SupplierID,
                         Supplier = a.SupplierName,
                         itemrebate = rebateBySupplier.TryGetValue(a.SupplierID, out var rb) ? (decimal?)rb : 0,


                     }).OrderByDescending(a => a.itemrebate);

            var data = v2.Skip(skip).Take(pageSize).ToList();
            recordsTotal = v2.Count();
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

        }


        [HttpPost]
        [QkAuthorize(Roles = "Dev,All Purchase")]
        public ActionResult GetAllpur(long? Task,long? project,string peno, long? supplier, string fromdate, long? SalesExecutive, string todate, long? type, long? ddlMC,long? Taxtype, string srchtxt, long? brand, long? category, long? item)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddlMC == 0 || ddlMC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            PurchaseEntry sEntry = new PurchaseEntry();
            if (type == 1)
            {
                sEntry.SupplierType = SupplierType.CashSale;
            }
            else if (type == 0)
            {
                sEntry.SupplierType = SupplierType.CreditSale;
            }
            else
            {

            }
            if (Taxtype == 2)
            {
              Int64 temp = 501;
                var v = (from a in db.PurchaseEntrys
                         join b in db.Suppliers on a.Supplier equals b.SupplierID into cust
                         from b in cust.DefaultIfEmpty()
                         join c in db.PEPayments on a.PurchaseEntryId equals c.PurchaseEntry into pay
                         from c in pay.DefaultIfEmpty()
                         join d in db.Employees on a.PECashier equals d.EmployeeId into emp
                         from d in emp.DefaultIfEmpty()
                         join e in db.MCs on a.MaterialCenter equals e.MCId into mcs
                         from e in mcs.DefaultIfEmpty()
                         join f in db.Accountss on b.Accounts equals f.AccountsID into acc
                         from f in acc.DefaultIfEmpty()
                         join j in db.AccountsTransactions on new { j1 = a.PurchaseEntryId, j2 = "Purchase", j3 = temp }
                         equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                         from j in hir1.DefaultIfEmpty()
                         join h in db.ConvertTransactionss on new { h1 = a.PurchaseEntryId, h2 = "Purchase" }
                         equals new { h1 = h.To, h2 = h.ConvertTo } into hir2
                         from h in hir2.DefaultIfEmpty()
                         join i in db.PurchaseOrders on new { i1 = h.From, i2 = h.ConvertFrom }
                         equals new { i1 = i.PurchaseOrderId, i2 = "POrder" } into hir3
                         from i in hir3.DefaultIfEmpty()
                         join k in db.PEItemss on a.PurchaseEntryId equals k.PurchaseEntry into temp2
                         from k in temp2.DefaultIfEmpty()
                         join l in db.Items on k.Item equals l.ItemID into temp3
                         from l in temp3.DefaultIfEmpty()
                     

                         let grandtotalitmsearch = (decimal)(from m in db.PEItemss
                                                             join nn in db.Items on m.Item equals nn.ItemID
                                                             join oo in db.PurchaseEntrys on m.PurchaseEntry equals oo.PurchaseEntryId
                                                             where nn.ItemName.Contains(srchtxt)
                                                             && oo.PurchaseEntryId == a.PurchaseEntryId

                                                             select new
                                                             {
                                                                 totalprice = m.ItemUnitPrice * m.ItemQuantity
                                                             }).Sum(o => o.totalprice)
                         let discount = (from k in db.PEBillSundrys 
                                                   where k.PurchaseEntry == a.PurchaseEntryId &&
                                                   k.BillSundry==4
                                                   select new
                                                   {
                                                    k.BsAmount
                                                   }).FirstOrDefault().BsAmount
                         let roundoffmin = (from k in db.PEBillSundrys 
                                         where k.PurchaseEntry == a.PurchaseEntryId &&
                                         k.BillSundry == 2
                                         select new
                                         {
                                             k.BsAmount
                                         }).FirstOrDefault().BsAmount
                         let roundoffplus = (from k in db.PEBillSundrys 
                                         where k.PurchaseEntry == a.PurchaseEntryId &&
                                         k.BillSundry == 1
                                         select new
                                         {
                                             k.BsAmount
                                         }).FirstOrDefault().BsAmount
                         where (peno == "" || a.BillNo == peno) && a.Status == 1 &&
                               (supplier == 0 || a.Supplier == supplier) &&
                               (SalesExecutive == 0 || SalesExecutive == null || a.PECashier == SalesExecutive) &&
                               (item == 0 || item == null || k.Item == item) &&
                               (brand == 0 || brand == null || l.ItemBrandID == brand) &&
                               (category == 0 || category == null || l.ItemCategoryID == category) &&
                               (srchtxt == "" || l.ItemName.Contains(srchtxt)) &&
                               (type == null || a.SupplierType == sEntry.SupplierType) &&
                               (fromdate == "" || EF.Functions.DateDiffDay(a.PEDate, fdate) <= 0) &&
                               (todate == "" || EF.Functions.DateDiffDay(a.PEDate, tdate) >= 0) &&
                               (ddlMC == null || ddlMC == 0 || a.MaterialCenter == ddlMC) &&
                               (a.PETax != 0) &&
                               (project==null ||k.ProjectId==project) &&
                               (Task==null||k.TaskId==Task)
                         select new
                         {
                             a.PurchaseEntryId,
                             PONo = (i.PONo == null ? 0 : i.PONo),
                             a.PENo,
                             a.BillNo,
                             a.PEDate,
                             a.PESubTotal,
                             //a.PEGrandTotal,
                             PEGrandTotal = (srchtxt == "") ? a.PEGrandTotal : grandtotalitmsearch,
                             Supplier = b.SupplierName,
                             TaxRegNo = f.TRN,
                             EmpName = d.FirstName + " " + d.LastName,
                             User = db.Users.Where(s => s.Id == a.CreatedBy).Select(s => s.Name).FirstOrDefault(),
                             a.PayType,
                             PaymentStatus = a.Status,
                             PaymentTrans = db.PETransactions.Any(k => k.PurchaseEntry == a.PurchaseEntryId),
                             PEPaidAmount = ((c.PEPaidAmount == null) ? 0 : c.PEPaidAmount),
                             a.SupplierType,
                             PEBalanceAmount = a.PEGrandTotal - ((c.PEPaidAmount == null) ? 0 : c.PEPaidAmount),
                             a.PECreatedDate,
                             MCName = (ddlMC != 0 || ddlMC != null) ? e.MCName : "All",
                             Debit = (srchtxt == "")?(j.Debit == null ? 0 : j.Debit):0,
                             taxableAmt = (srchtxt == "")?(a.PESubTotal - a.PEDiscount): grandtotalitmsearch,
                             discount,
                             roundoffmin,
                             roundoffplus
                         }).Distinct().OrderBy(a => a.PEDate);
                var data = v.Skip(skip).Take(pageSize).ToList();
                recordsTotal = v.Count();
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                var results = new ContentResult
                {
                    Content = result,
                    ContentType = "application/json"
                };
                return results;
            }
            else if (Taxtype == 1)
            {
                 Int64 temp = 501;
                 var v = (from a in db.PurchaseEntrys
                          join b in db.Suppliers on a.Supplier equals b.SupplierID into cust
                          from b in cust.DefaultIfEmpty()
                          join c in db.PEPayments on a.PurchaseEntryId equals c.PurchaseEntry into pay
                          from c in pay.DefaultIfEmpty()
                          join d in db.Employees on a.PECashier equals d.EmployeeId into emp
                          from d in emp.DefaultIfEmpty()
                          join e in db.MCs on a.MaterialCenter equals e.MCId into mcs
                          from e in mcs.DefaultIfEmpty()
                          join f in db.Accountss on b.Accounts equals f.AccountsID into acc
                          from f in acc.DefaultIfEmpty()
                          join j in db.AccountsTransactions on new { j1 = a.PurchaseEntryId, j2 = "Purchase", j3 = temp }
                          equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                          from j in hir1.DefaultIfEmpty()
                          join h in db.ConvertTransactionss on new { h1 = a.PurchaseEntryId, h2 = "Purchase" }
                          equals new { h1 = h.To, h2 = h.ConvertTo } into hir2
                          from h in hir2.DefaultIfEmpty()
                          join i in db.PurchaseOrders on new { i1 = h.From, i2 = h.ConvertFrom }
                          equals new { i1 = i.PurchaseOrderId, i2 = "POrder" } into hir3
                          from i in hir3.DefaultIfEmpty()
                          join k in db.PEItemss on a.PurchaseEntryId equals k.PurchaseEntry into temp2
                          from k in temp2.DefaultIfEmpty()
                          where (peno == "" || a.BillNo == peno) && a.Status == 1 &&
                                (supplier == 0 || a.Supplier == supplier) &&
                                (SalesExecutive == 0 || SalesExecutive == null || a.PECashier == SalesExecutive) &&
                                (type == null || a.SupplierType == sEntry.SupplierType) &&
                                (fromdate == "" || EF.Functions.DateDiffDay(a.PEDate, fdate) <= 0) &&
                                (todate == "" || EF.Functions.DateDiffDay(a.PEDate, tdate) >= 0)&&
                                (ddlMC == null || ddlMC == 0 || a.MaterialCenter == ddlMC) &&
                                (project == null || k.ProjectId == project) &&
                                (a.PETax == 0)
                                &&
                               (Task == null || k.TaskId == Task)
                          select new
                          {
                                a.PurchaseEntryId,
                                a.PENo,
                                PONo = (i.PONo == null ? 0 : i.PONo),
                                a.BillNo,
                                a.PEDate,
                                a.PESubTotal,
                                a.PEGrandTotal,
                                Supplier = b.SupplierName,
                                TaxRegNo = f.TRN,
                                EmpName = d.FirstName + " " + d.LastName,
                                User = db.Users.Where(s => s.Id == a.CreatedBy).Select(s => s.Name).FirstOrDefault(),
                                a.PayType,
                                PaymentStatus = a.Status,
                                PaymentTrans = db.PETransactions.Any(k => k.PurchaseEntry == a.PurchaseEntryId),
                                PEPaidAmount = ((c.PEPaidAmount == null) ? 0 : c.PEPaidAmount),
                                a.SupplierType,
                                PEBalanceAmount = a.PEGrandTotal - ((c.PEPaidAmount == null) ? 0 : c.PEPaidAmount),
                                a.PECreatedDate,
                                MCName = (ddlMC != 0 || ddlMC != null) ? e.MCName : "All",
                                Debit = (j.Debit == null ? 0 : j.Debit),
                                taxableAmt = a.PESubTotal - a.PEDiscount,
                          }).Distinct().OrderBy(a => a.PEDate);
                var data = v.Skip(skip).Take(pageSize).ToList();
                recordsTotal = v.Count();
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                var results = new ContentResult
                {
                    Content = result,
                    ContentType = "application/json"
                };
                return results;
            }
            else
            {
                 Int64 temp = 501;
                 var v = (from a in db.PurchaseEntrys
                          join b in db.Suppliers on a.Supplier equals b.SupplierID into cust
                          from b in cust.DefaultIfEmpty()
                          join c in db.PEPayments on a.PurchaseEntryId equals c.PurchaseEntry into pay
                          from c in pay.DefaultIfEmpty()
                          join d in db.Employees on a.PECashier equals d.EmployeeId into emp
                          from d in emp.DefaultIfEmpty()
                          join e in db.MCs on a.MaterialCenter equals e.MCId into mcs
                          from e in mcs.DefaultIfEmpty()
                          join f in db.Accountss on b.Accounts equals f.AccountsID into acc
                          from f in acc.DefaultIfEmpty()
                          join j in db.AccountsTransactions on new { j1 = a.PurchaseEntryId, j2 = "Purchase", j3 = temp }
                          equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                          from j in hir1.DefaultIfEmpty()
                          join h in db.ConvertTransactionss on new { h1 = a.PurchaseEntryId, h2 = "Purchase" }
                          equals new { h1 = h.To, h2 = h.ConvertTo } into hir2
                          from h in hir2.DefaultIfEmpty()
                          join i in db.PurchaseOrders on new { i1 = h.From, i2 = h.ConvertFrom }
                          equals new { i1 = i.PurchaseOrderId, i2 = "POrder" } into hir3
                          from i in hir3.DefaultIfEmpty()
                          join k in db.PEItemss on a.PurchaseEntryId equals k.PurchaseEntry into temp2
                          from k in temp2.DefaultIfEmpty()
                          join l in db.Items on k.Item equals l.ItemID into temp3
                          from l in temp3.DefaultIfEmpty()
                          where (peno == "" || a.BillNo == peno) && a.Status == 1 &&
                                (supplier == 0 || a.Supplier == supplier) &&
                                (SalesExecutive == 0 || SalesExecutive == null || a.PECashier == SalesExecutive) &&
                                (type == null || a.SupplierType == sEntry.SupplierType) &&
                                (fromdate == "" || EF.Functions.DateDiffDay(a.PEDate, fdate) <= 0) &&
                                (todate == "" || EF.Functions.DateDiffDay(a.PEDate, tdate) >= 0)&&
                                      (srchtxt == "" || l.ItemName.Contains(srchtxt)) &&
                                (ddlMC == null || ddlMC == 0 || a.MaterialCenter == ddlMC) &&
                                (project == null || k.ProjectId == project) &&
                               (Task == null || k.TaskId == Task)
                          select new
                          {
                                a.PurchaseEntryId,
                                a.PENo,
                                PONo = (i.PONo == null ? 0 : i.PONo),
                                a.BillNo,
                                a.PEDate,
                                a.PESubTotal,
                                a.PEGrandTotal,
                                Supplier = b.SupplierName,
                                TaxRegNo = f.TRN,
                                EmpName = d.FirstName + " " + d.LastName,
                                User = db.Users.Where(s => s.Id == a.CreatedBy).Select(s => s.Name).FirstOrDefault(),
                                a.PayType,
                                PaymentStatus = a.Status,
                                PaymentTrans = db.PETransactions.Any(k => k.PurchaseEntry == a.PurchaseEntryId),
                                PEPaidAmount = ((c.PEPaidAmount == null) ? 0 : c.PEPaidAmount),
                                a.SupplierType,
                                PEBalanceAmount = a.PEGrandTotal - ((c.PEPaidAmount == null) ? 0 : c.PEPaidAmount),
                                a.PECreatedDate,
                                MCName = (ddlMC != 0 || ddlMC != null) ? e.MCName : "All",
                                Debit = (j.Debit == null ? 0 : j.Debit),
                                taxableAmt = a.PESubTotal - a.PEDiscount,
                         }).Distinct().OrderBy(a => a.PEDate);
                var data = v.Skip(skip).Take(pageSize).ToList();
                recordsTotal = v.Count();
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                var results = new ContentResult
                {
                    Content = result,
                    ContentType = "application/json"
                };
                return results;
            }
        }
       public class latestpurchase
        {
            public long purchaseid { get; set; }
            public decimal price { get; set; }
            public DateTime pedate { get; set; }
            public string invoiceno { get; set; }
            public decimal qty { get; set; }
        }
        public latestpurchase getlastpurchase(long itemid,int pos)
        {
            var data=(from a in db.PurchaseEntrys
                      join b in db.PEItemss on a.PurchaseEntryId equals b.PurchaseEntry
                      join c in db.Items on new {g1=b.Item,g2=b.ItemUnit} equals new
                      {
                          g1=c.ItemID,g2=c.ItemUnitID
                      }
                      where b.Item==itemid
                      select  new latestpurchase
                      {
                         purchaseid=a.PurchaseEntryId,
                         price=b.ItemUnitPrice,
                         pedate=a.PEDate,
                         invoiceno=a.BillNo,
                         qty=b.ItemQuantity

                        }).OrderByDescending(o=>o.pedate).Skip(pos-1).Take(1).FirstOrDefault();
            return data;
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,All Purchase")]
        public ActionResult GetAllPurchase(string srchtxt,  long? supplier, string fromdate, string todate, long comparprices, long? brand, long? category)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            PurchaseEntry sEntry = new PurchaseEntry();
                Int64 temp = 501;
                var v = (from a in db.Items
                         join b in db.PEItemss on new {g1=a.ItemID,g2=a.ItemUnitID} equals new
                         {g1=b.Item,g2=b.ItemUnit}
                         join c in db.PurchaseEntrys on b.PurchaseEntry equals c.PurchaseEntryId

                         where
                               a.ItemName.Contains(srchtxt) &&
                              
                                     (supplier == 0 || c.Supplier == supplier) &&
                                     (fromdate == "" || EF.Functions.DateDiffDay(c.PEDate, fdate) <= 0) &&
                                     (todate == "" || EF.Functions.DateDiffDay(c.PEDate, tdate) >= 0)&& 
                                     (category==0||a.ItemCategoryID==category) &&
                                     (brand==0||a.ItemBrandID==brand)


                         select new
                         {
                             a.ItemID,
                             ItemName=a.ItemCode+" "+ a.ItemName
                         }).Distinct().ToList().Select(o => new
                         {
                             o.ItemName,
                             o.ItemID,
                             purchase1 = getlastpurchase(o.ItemID, 10),
                             purchase2= getlastpurchase(o.ItemID, 9),
                             purchase3= getlastpurchase(o.ItemID, 8),
                             purchase4= getlastpurchase(o.ItemID, 7),
                             purchase5= getlastpurchase(o.ItemID, 6),
                             purchase6= getlastpurchase(o.ItemID, 5),
                             purchase7= getlastpurchase(o.ItemID, 4),
                             purchase8= getlastpurchase(o.ItemID, 3),
                             purchase9= getlastpurchase(o.ItemID, 2),
                             purchase10= getlastpurchase(o.ItemID, 1),
                             

                         }).Select(o=>new
                         {
                             o.ItemID,
                             o.ItemName,
                             o.purchase1,
                             o.purchase2,
                             o.purchase3,
                             o.purchase4,
                             o.purchase5,
                             o.purchase6,
                             o.purchase7,
                             o.purchase8,
                             o.purchase9,
                             o.purchase10,
                             compairstatus=getcompairstatus(comparprices,o.purchase1, o.purchase2, o.purchase3, o.purchase4, o.purchase5, o.purchase6, o.purchase7, o.purchase8, o.purchase9, o.purchase10)
                         }).OrderBy(a => a.compairstatus);
                var data = v.ToList();
                recordsTotal = v.Count();
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                var results = new ContentResult
                {
                    Content = result,
                    ContentType = "application/json"
                };
                return results;
            
        }
        public string getcompairstatus(long comparprices,latestpurchase purchase1, latestpurchase purchase2, latestpurchase purchase3, latestpurchase purchase4, latestpurchase purchase5, latestpurchase purchase6, latestpurchase purchase7, latestpurchase purchase8, latestpurchase purchase9, latestpurchase purchase10)
        {
            string result = "Normal";
            decimal[] prices = new decimal[comparprices];
                prices[0] = purchase10== null ? 0 : purchase10.price;
                prices[1] = purchase9== null ? 0 : purchase9.price;
                if(comparprices>2)
                prices[2] = purchase8== null ? 0 : purchase8.price;
                 if (comparprices >3)
                prices[3] = purchase7== null ? 0 : purchase7.price;
            if (comparprices > 4)
                prices[4] = purchase6== null ? 0 : purchase6.price;
            if (comparprices > 5)
                prices[5] = purchase5== null ? 0 : purchase5.price;
            if (comparprices > 6)
                prices[6] = purchase4== null ? 0 : purchase4.price;
            if (comparprices > 7)
                prices[7] = purchase3== null ? 0 : purchase3.price;
            if (comparprices > 8)
                prices[8] = purchase2== null ? 0 : purchase2.price;
            if (comparprices > 9)
                prices[9] = purchase1== null ? 0 : purchase1.price;
            decimal[] partprices = prices.Where(o => o != 0).ToArray();
            if(partprices.Distinct().Count()>1)
            {
                result = "Modified";
            }
            
            return result;
        }
        #endregion

        #region Supplier Wise
        [QkAuthorize(Roles = "Dev,Purchase Supplier Wise")]
        public ActionResult SupplierWise()
        {
            ViewBag.Supplier = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                }, "Value", "Text", 0);

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            return View();
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Purchase Supplier Wise")]
        public ActionResult ViewSupplierWise(long? ddlSupplier, string from, string to, long? ddlMC)
        {
            if (ddlSupplier != 0)
            {
                ViewBag.custName = (from a in db.Suppliers
                                    join f in db.Accountss on a.Accounts equals f.AccountsID
                                    where a.SupplierID == ddlSupplier
                                    select new
                                    {
                                        SupplierName = a.SupplierName + (f.TRN != null ? " ; TRN :" + f.TRN : "")
                                    }).FirstOrDefault().SupplierName;
            }
            else
            {
                ViewBag.custName = "All";
            }
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            ViewBag.ddlmc = ddlMC;
            companySet();
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Purchase Supplier Wise")]
        public ActionResult GetSupplierWise(long? Supplier, string fromdate, string todate, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddmc == 0 || ddmc == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }

            var v = (from a in db.Suppliers
                     join f in db.Accountss on a.Accounts equals f.AccountsID into acc
                     from f in acc.DefaultIfEmpty()
                     where (Supplier == 0 || a.SupplierID == Supplier)
                     select new
                     {
                         a.SupplierID,
                         Supplier = a.SupplierCode + "-" + a.SupplierName,
                         TRN = f.TRN,
                         PurAmt = (decimal?)(from i in db.PurchaseEntrys
                                             where (fromdate == "" || EF.Functions.DateDiffDay(i.PEDate, fdate) <= 0) && i.Status == 1 &&
                                             (todate == "" || EF.Functions.DateDiffDay(i.PEDate, tdate) >= 0) &&
                                             (i.Supplier == a.SupplierID)
                                             && ((!MCList.Any() && ddmc == null) || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                             group i by i.Supplier into g
                                             select new
                                             {
                                                 Total = g.Sum(x => x.PESubTotal - x.PEDiscount)
                                             }).FirstOrDefault().Total ?? 0,
                         PurtaxAmt = (decimal?)(from i in db.PurchaseEntrys
                                                where (fromdate == "" || EF.Functions.DateDiffDay(i.PEDate, fdate) <= 0) && i.Status == 1 &&
                                                 (todate == "" || EF.Functions.DateDiffDay(i.PEDate, tdate) >= 0) &&
                                                 (i.Supplier == a.SupplierID)
                                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                                group i by i.Supplier into g
                                                select new
                                                {
                                                    Total = g.Sum(x => x.PETaxAmount)
                                                }).FirstOrDefault().Total ?? 0,
                         PurtotAmt = (decimal?)(from i in db.PurchaseEntrys
                                                where (fromdate == "" || EF.Functions.DateDiffDay(i.PEDate, fdate) <= 0) && i.Status == 1 &&
                                                (todate == "" || EF.Functions.DateDiffDay(i.PEDate, tdate) >= 0) &&
                                                (i.Supplier == a.SupplierID)
                                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                                group i by i.Supplier into g
                                                select new
                                                {
                                                    Total = g.Sum(x => x.PEGrandTotal)
                                                }).FirstOrDefault().Total ?? 0,
                         RetunAmt = (decimal?)(from i in db.PurchaseReturns
                                               where (fromdate == "" || EF.Functions.DateDiffDay(i.PRDate, fdate) <= 0) &&
                                               (todate == "" || EF.Functions.DateDiffDay(i.PRDate, tdate) >= 0) &&
                                               (i.Supplier == a.SupplierID)
                                               && ((!MCList.Any() && ddmc == null) || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                               group i by i.Supplier into g
                                               select new
                                               {
                                                   Total = g.Sum(x => x.PRSubTotal - x.PRDiscount)
                                               }).FirstOrDefault().Total ?? 0,
                         RetuntaxAmt = (decimal?)(from i in db.PurchaseReturns
                                                  where (fromdate == "" || EF.Functions.DateDiffDay(i.PRDate, fdate) <= 0) &&
                                                  (todate == "" || EF.Functions.DateDiffDay(i.PRDate, tdate) >= 0) &&
                                                  (i.Supplier == a.SupplierID)
                                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                                  group i by i.Supplier into g
                                                  select new
                                                  {
                                                      Total = g.Sum(x => x.PRTaxAmount)
                                                  }).FirstOrDefault().Total ?? 0,
                         RetuntotAmt = (decimal?)(from i in db.PurchaseReturns
                                                  where (fromdate == "" || EF.Functions.DateDiffDay(i.PRDate, fdate) <= 0) &&
                                                  (todate == "" || EF.Functions.DateDiffDay(i.PRDate, tdate) >= 0) &&
                                                  (i.Supplier == a.SupplierID)
                                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                                  group i by i.Supplier into g
                                                  select new
                                                  {
                                                      Total = g.Sum(x => x.PRGrandTotal)
                                                  }).FirstOrDefault().Total ?? 0,

                         NoOfVchpur = (int?)(from i in db.PurchaseEntrys
                                             where (fromdate == "" || EF.Functions.DateDiffDay(i.PEDate, fdate) <= 0) && i.Status == 1 &&
                                             (todate == "" || EF.Functions.DateDiffDay(i.PEDate, tdate) >= 0) &&
                                             (i.Supplier == a.SupplierID)

                                             && ((!MCList.Any() && ddmc == null) || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                             select new
                                             {
                                                 purid = i.PurchaseEntryId
                                             }).Count() ?? 0,
                         NoOfVchReturn = (int?)(from j in db.PurchaseReturns
                                                where (fromdate == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                                (todate == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                                (j.Supplier == a.SupplierID)
                                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                select new
                                                {
                                                    purid = j.PurchaseReturnId
                                                }).Count() ?? 0,
                     }).Where(o => o.PurAmt > 0 || o.RetunAmt > 0).OrderBy(b => b.SupplierID);

            var data = v.Skip(skip).Take(pageSize).ToList();
            recordsTotal = v.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }

        #endregion

        #region month wise

        [QkAuthorize(Roles = "Dev,Purchase Month Wise")]
        public ActionResult MonthWiseSelect()
        {
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Purchase Month Wise")]
        public ActionResult MonthWise(int Year)
        {
            companySet();
            var vmodel = new MonthWiseSaleReportViewModel();
            //purs
            //monthly pur count
            // audit batch 11: EF Core SqlQueryRaw<MonthWise> requires every mapped column to be present;
            // legacy EF6 silently left the absent [total] null. Reproduce that exactly with a typed NULL
            // (the view sums columns itself), so the report renders identically instead of 500-ing.
            var Qry1 = "SELECT *, CAST(NULL AS INT) AS [total] FROM(SELECT YEAR(PEDate)[Year],DATENAME(MONTH, PEDate)[Month], " +
                       " COUNT(1)[purs Count] FROM PurchaseEntries Where Status = 1 AND YEAR(PEDate)=" + Year + "" +
                       " GROUP BY YEAR(PEDate), DATENAME(MONTH, PEDate)) AS MontlypursData " +
                       " PIVOT(SUM([purs Count]) " +
                       " FOR Month IN([January], [February], [March], [April], [May], " +
                       " [June], [July], [August], [September], [October], [November], " +
                       " [December])) AS MNamePivot ";

            vmodel.saleCount = db.Database.SqlQueryRaw<MonthWise>(Qry1).AsEnumerable().ToList();

            //monthly taxable amt 
            var Qry2 = "SELECT *, CAST(NULL AS DECIMAL(18,2)) AS [total] FROM(SELECT YEAR(PEDate)[Year],DATENAME(MONTH, PEDate)[Month], " +
                   " sum(PESubTotal - PEDiscount) [puramount] FROM PurchaseEntries Where Status = 1 AND YEAR(PEDate)=" + Year + "" +
                   " GROUP BY YEAR(PEDate), DATENAME(MONTH, PEDate)) AS MontlypursData " +
                   " PIVOT(SUM([puramount]) " +
                   " FOR Month IN([January], [February], [March], [April], [May], " +
                   " [June], [July], [August], [September], [October], [November], " +
                   " [December])) AS MNamePivot ";

            vmodel.taxableAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(Qry2).AsEnumerable().ToList();


            //monthly total purs amount
            var Qry3 = "SELECT *, CAST(NULL AS DECIMAL(18,2)) AS [total] FROM(SELECT YEAR(PEDate)[Year], DATENAME(MONTH, PEDate)[Month]," +
                       " sum(PEGrandTotal)[puramount] FROM PurchaseEntries Where Status = 1 AND YEAR(PEDate)=" + Year + " GROUP BY YEAR(PEDate), " +
                       " DATENAME(MONTH, PEDate)) AS MontlypursData " +
                       " PIVOT(SUM([puramount]) " +
                       " FOR Month IN([January], [February], [March], [April], [May]," +
                       " [June], [July], [August], [September], [October], [November],[December])) AS MNamePivot";

            vmodel.saleAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(Qry3).AsEnumerable().ToList();

            //total tax amount
            var Qry4 = "SELECT *, CAST(NULL AS DECIMAL(18,2)) AS [total] FROM(SELECT YEAR(PEDate)[Year],DATENAME(MONTH, PEDate)[Month], " +
                       " sum(PETaxAmount)[puramount] FROM PurchaseEntries Where Status = 1 AND YEAR(PEDate)=" + Year + "" +
                       " GROUP BY YEAR(PEDate), DATENAME(MONTH, PEDate)) AS MontlypursData " +
                       " PIVOT(SUM([puramount]) " +
                       " FOR Month IN([January], [February], [March], [April], [May], " +
                       " [June], [July], [August], [September], [October], [November], [December])) AS MNamePivot";

            vmodel.taxAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(Qry4).AsEnumerable().ToList();

            //purs return

            //monthly pur count
            var QryR1 = "SELECT *, CAST(NULL AS INT) AS [total] FROM(SELECT YEAR(PRDate)[Year],DATENAME(MONTH, PRDate)[Month], " +
                       " COUNT(1)[purs Count] FROM PurchaseReturns Where YEAR(PRDate)=" + Year + "" +
                       " GROUP BY YEAR(PRDate), DATENAME(MONTH, PRDate)) AS MontlypursData " +
                       " PIVOT(SUM([purs Count]) " +
                       " FOR Month IN([January], [February], [March], [April], [May], " +
                       " [June], [July], [August], [September], [October], [November], " +
                       " [December])) AS MNamePivot ";

            vmodel.saleRetCount = db.Database.SqlQueryRaw<MonthWise>(QryR1).AsEnumerable().ToList();

            //monthly taxable amt 
            var QryR2 = "SELECT *, CAST(NULL AS DECIMAL(18,2)) AS [total] FROM(SELECT YEAR(PRDate)[Year],DATENAME(MONTH, PRDate)[Month], " +
                   " sum(PRSubTotal - PRDiscount) [puramount] FROM PurchaseReturns Where YEAR(PRDate)=" + Year + "" +
                   " GROUP BY YEAR(PRDate), DATENAME(MONTH, PRDate)) AS MontlypursData " +
                   " PIVOT(SUM([puramount]) " +
                   " FOR Month IN([January], [February], [March], [April], [May], " +
                   " [June], [July], [August], [September], [October], [November], " +
                   " [December])) AS MNamePivot ";

            vmodel.taxableRetAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(QryR2).AsEnumerable().ToList();


            //monthly total purs amount
            var QryR3 = "SELECT *, CAST(NULL AS DECIMAL(18,2)) AS [total] FROM(SELECT YEAR(PRDate)[Year], DATENAME(MONTH, PRDate)[Month]," +
                       " sum(PRGrandTotal)[puramount] FROM PurchaseReturns Where YEAR(PRDate)=" + Year + " GROUP BY YEAR(PRDate), " +
                       " DATENAME(MONTH, PRDate)) AS MontlypursData " +
                       " PIVOT(SUM([puramount]) " +
                       " FOR Month IN([January], [February], [March], [April], [May]," +
                       " [June], [July], [August], [September], [October], [November],[December])) AS MNamePivot";

            vmodel.saleRetAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(QryR3).AsEnumerable().ToList();

            //total tax amount
            var QryR4 = "SELECT *, CAST(NULL AS DECIMAL(18,2)) AS [total] FROM(SELECT YEAR(PRDate)[Year],DATENAME(MONTH, PRDate)[Month], " +
                       " sum(PRTaxAmount)[puramount] FROM PurchaseReturns Where YEAR(PRDate)=" + Year + " " +
                       " GROUP BY YEAR(PRDate), DATENAME(MONTH, PRDate)) AS MontlypursData " +
                       " PIVOT(SUM([puramount]) " +
                       " FOR Month IN([January], [February], [March], [April], [May], " +
                       " [June], [July], [August], [September], [October], [November], [December])) AS MNamePivot";
            vmodel.taxRetAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(QryR4).AsEnumerable().ToList();
            // calculate net pur amount
            if (vmodel.saleAmount.Any() && vmodel.saleRetAmount.Any())
            {
                vmodel.netAmount = (from a in vmodel.saleAmount
                                    join b in vmodel.saleRetAmount on a.Year equals b.Year into ret
                                    from b in ret.DefaultIfEmpty()
                                    select new MonthWiseDecimal
                                    {
                                        Year = a.Year,
                                        January = (a.January ?? 0) - (b.January ?? 0),
                                        February = (a.February ?? 0) - (b.February ?? 0),
                                        March = (a.March ?? 0) - (b.March ?? 0),
                                        April = (a.April ?? 0) - (b.April ?? 0),
                                        May = (a.May ?? 0) - (b.May ?? 0),
                                        June = (a.June ?? 0) - (b.June ?? 0),
                                        July = (a.July ?? 0) - (b.July ?? 0),
                                        August = (a.August ?? 0) - (b.August ?? 0),
                                        September = (a.September ?? 0) - (b.September ?? 0),
                                        October = (a.October ?? 0) - (b.October ?? 0),
                                        November = (a.November ?? 0) - (b.November ?? 0),
                                        December = (a.December ?? 0) - (b.December ?? 0),
                                        total = (a.January + a.February + a.March + a.April + a.May + a.June + a.July + a.August + a.September + a.November + a.December) -
                                         (b.January + b.February + b.March + b.April + b.May + b.June + b.July + b.August + b.September + b.November + b.December)
                                    }).ToList();
            }
            else if (vmodel.saleAmount.Any())
            {
                vmodel.netAmount = (from a in vmodel.saleAmount
                                    select new MonthWiseDecimal
                                    {
                                        Year = a.Year,
                                        January = (a.January ?? 0),
                                        February = (a.February ?? 0),
                                        March = (a.March ?? 0),
                                        April = (a.April ?? 0),
                                        May = (a.May ?? 0),
                                        June = (a.June ?? 0),
                                        July = (a.July ?? 0),
                                        August = (a.August ?? 0),
                                        September = (a.September ?? 0),
                                        October = (a.October ?? 0),
                                        November = (a.November ?? 0),
                                        December = (a.December ?? 0),
                                        total = (a.January + a.February + a.March + a.April + a.May + a.June + a.July + a.August + a.September + a.November + a.December)
                                    }).ToList();
            }
            else
            {
                vmodel.netAmount = (from a in vmodel.saleRetAmount
                                    select new MonthWiseDecimal
                                    {
                                        Year = a.Year,
                                        January = (a.January ?? 0),
                                        February = (a.February ?? 0),
                                        March = (a.March ?? 0),
                                        April = (a.April ?? 0),
                                        May = (a.May ?? 0),
                                        June = (a.June ?? 0),
                                        July = (a.July ?? 0),
                                        August = (a.August ?? 0),
                                        September = (a.September ?? 0),
                                        October = (a.October ?? 0),
                                        November = (a.November ?? 0),
                                        December = (a.December ?? 0),
                                        total = 0 - (a.January + a.February + a.March + a.April + a.May + a.June + a.July + a.August + a.September + a.November + a.December)
                                    }).ToList();
            }
            ViewBag.SelYear = Year;
            return View(vmodel);
        }


        [QkAuthorize(Roles = "Dev,Purchase Month Wise")]
        public ActionResult MonthWise()
        {
            var chkpr = db.PurchaseEntrys.Where(a => a.Status == 1).Select(s => s.PEDate.Year).Distinct().Count();
            var chkret = db.PurchaseReturns.Select(s => s.PRDate.Year).Distinct().Count();

            if (chkpr > 1 || chkret > 1)
            {
                return Redirect("/PurchaseReport/MonthWiseSelect");
            }
            else
            {
                companySet();
                var vmodel = new MonthWiseSaleReportViewModel();
                //purs
                //monthly pur count
                var Qry1 = "SELECT * FROM(SELECT YEAR(PEDate)[Year],DATENAME(MONTH, PEDate)[Month], " +
                           " COUNT(1)[purs Count] FROM PurchaseEntries Where Status = 1 " +
                           " GROUP BY YEAR(PEDate), DATENAME(MONTH, PEDate)) AS MontlypursData " +
                           " PIVOT(SUM([purs Count]) " +
                           " FOR Month IN([January], [February], [March], [April], [May], " +
                           " [June], [July], [August], [September], [October], [November], " +
                           " [December])) AS MNamePivot ";

                vmodel.saleCount = db.Database.SqlQueryRaw<MonthWise>(Qry1).AsEnumerable().ToList();

                //monthly taxable amt 
                var Qry2 = "SELECT * FROM(SELECT YEAR(PEDate)[Year],DATENAME(MONTH, PEDate)[Month], " +
                       " sum(PESubTotal - PEDiscount) [puramount] FROM PurchaseEntries Where Status = 1" +
                       " GROUP BY YEAR(PEDate), DATENAME(MONTH, PEDate)) AS MontlypursData " +
                       " PIVOT(SUM([puramount]) " +
                       " FOR Month IN([January], [February], [March], [April], [May], " +
                       " [June], [July], [August], [September], [October], [November], " +
                       " [December])) AS MNamePivot ";

                vmodel.taxableAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(Qry2).AsEnumerable().ToList();


                //monthly total purs amount
                var Qry3 = "SELECT * FROM(SELECT YEAR(PEDate)[Year], DATENAME(MONTH, PEDate)[Month]," +
                           " sum(PEGrandTotal)[puramount] FROM PurchaseEntries Where Status = 1 GROUP BY YEAR(PEDate), " +
                           " DATENAME(MONTH, PEDate)) AS MontlypursData " +
                           " PIVOT(SUM([puramount]) " +
                           " FOR Month IN([January], [February], [March], [April], [May]," +
                           " [June], [July], [August], [September], [October], [November],[December])) AS MNamePivot";

                vmodel.saleAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(Qry3).AsEnumerable().ToList();

                //total tax amount
                var Qry4 = "SELECT * FROM(SELECT YEAR(PEDate)[Year],DATENAME(MONTH, PEDate)[Month], " +
                           " sum(PETaxAmount)[puramount] FROM PurchaseEntries Where Status = 1 " +
                           " GROUP BY YEAR(PEDate), DATENAME(MONTH, PEDate)) AS MontlypursData " +
                           " PIVOT(SUM([puramount]) " +
                           " FOR Month IN([January], [February], [March], [April], [May], " +
                           " [June], [July], [August], [September], [October], [November], [December])) AS MNamePivot";

                vmodel.taxAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(Qry4).AsEnumerable().ToList();

                //purs return

                //monthly pur count
                var QryR1 = "SELECT * FROM(SELECT YEAR(PRDate)[Year],DATENAME(MONTH, PRDate)[Month], " +
                           " COUNT(1)[purs Count] FROM PurchaseReturns " +
                           " GROUP BY YEAR(PRDate), DATENAME(MONTH, PRDate)) AS MontlypursData " +
                           " PIVOT(SUM([purs Count]) " +
                           " FOR Month IN([January], [February], [March], [April], [May], " +
                           " [June], [July], [August], [September], [October], [November], " +
                           " [December])) AS MNamePivot ";

                vmodel.saleRetCount = db.Database.SqlQueryRaw<MonthWise>(QryR1).AsEnumerable().ToList();

                //monthly taxable amt 
                var QryR2 = "SELECT * FROM(SELECT YEAR(PRDate)[Year],DATENAME(MONTH, PRDate)[Month], " +
                       " sum(PRSubTotal - PRDiscount) [puramount] FROM PurchaseReturns " +
                       " GROUP BY YEAR(PRDate), DATENAME(MONTH, PRDate)) AS MontlypursData " +
                       " PIVOT(SUM([puramount]) " +
                       " FOR Month IN([January], [February], [March], [April], [May], " +
                       " [June], [July], [August], [September], [October], [November], " +
                       " [December])) AS MNamePivot ";

                vmodel.taxableRetAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(QryR2).AsEnumerable().ToList();


                //monthly total purs amount
                var QryR3 = "SELECT * FROM(SELECT YEAR(PRDate)[Year], DATENAME(MONTH, PRDate)[Month]," +
                           " sum(PRGrandTotal)[puramount] FROM PurchaseReturns GROUP BY YEAR(PRDate), " +
                           " DATENAME(MONTH, PRDate)) AS MontlypursData " +
                           " PIVOT(SUM([puramount]) " +
                           " FOR Month IN([January], [February], [March], [April], [May]," +
                           " [June], [July], [August], [September], [October], [November],[December])) AS MNamePivot";

                vmodel.saleRetAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(QryR3).AsEnumerable().ToList();

                //total tax amount
                var QryR4 = "SELECT * FROM(SELECT YEAR(PRDate)[Year],DATENAME(MONTH, PRDate)[Month], " +
                           " sum(PRTaxAmount)[puramount] FROM PurchaseReturns " +
                           " GROUP BY YEAR(PRDate), DATENAME(MONTH, PRDate)) AS MontlypursData " +
                           " PIVOT(SUM([puramount]) " +
                           " FOR Month IN([January], [February], [March], [April], [May], " +
                           " [June], [July], [August], [September], [October], [November], [December])) AS MNamePivot";
                vmodel.taxRetAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(QryR4).AsEnumerable().ToList();
                // calculate net pur amount
                if (vmodel.saleAmount.Any() && vmodel.saleRetAmount.Any())
                {
                    vmodel.netAmount = (from a in vmodel.saleAmount
                                        join b in vmodel.saleRetAmount on a.Year equals b.Year into ret
                                        from b in ret.DefaultIfEmpty()
                                        select new MonthWiseDecimal
                                        {
                                            Year = a.Year,
                                            January = (a.January ?? 0) - (b.January ?? 0),
                                            February = (a.February ?? 0) - (b.February ?? 0),
                                            March = (a.March ?? 0) - (b.March ?? 0),
                                            April = (a.April ?? 0) - (b.April ?? 0),
                                            May = (a.May ?? 0) - (b.May ?? 0),
                                            June = (a.June ?? 0) - (b.June ?? 0),
                                            July = (a.July ?? 0) - (b.July ?? 0),
                                            August = (a.August ?? 0) - (b.August ?? 0),
                                            September = (a.September ?? 0) - (b.September ?? 0),
                                            October = (a.October ?? 0) - (b.October ?? 0),
                                            November = (a.November ?? 0) - (b.November ?? 0),
                                            December = (a.December ?? 0) - (b.December ?? 0),
                                            total = (a.January + a.February + a.March + a.April + a.May + a.June + a.July + a.August + a.September + a.November + a.December) -
                                             (b.January + b.February + b.March + b.April + b.May + b.June + b.July + b.August + b.September + b.November + b.December)
                                        }).ToList();
                }
                else if (vmodel.saleAmount.Any())
                {
                    vmodel.netAmount = (from a in vmodel.saleAmount
                                        select new MonthWiseDecimal
                                        {
                                            Year = a.Year,
                                            January = (a.January ?? 0),
                                            February = (a.February ?? 0),
                                            March = (a.March ?? 0),
                                            April = (a.April ?? 0),
                                            May = (a.May ?? 0),
                                            June = (a.June ?? 0),
                                            July = (a.July ?? 0),
                                            August = (a.August ?? 0),
                                            September = (a.September ?? 0),
                                            October = (a.October ?? 0),
                                            November = (a.November ?? 0),
                                            December = (a.December ?? 0),
                                            total = (a.January + a.February + a.March + a.April + a.May + a.June + a.July + a.August + a.September + a.November + a.December)
                                        }).ToList();
                }
                else
                {
                    vmodel.netAmount = (from a in vmodel.saleRetAmount
                                        select new MonthWiseDecimal
                                        {
                                            Year = a.Year,
                                            January = (a.January ?? 0),
                                            February = (a.February ?? 0),
                                            March = (a.March ?? 0),
                                            April = (a.April ?? 0),
                                            May = (a.May ?? 0),
                                            June = (a.June ?? 0),
                                            July = (a.July ?? 0),
                                            August = (a.August ?? 0),
                                            September = (a.September ?? 0),
                                            October = (a.October ?? 0),
                                            November = (a.November ?? 0),
                                            December = (a.December ?? 0),
                                            total = 0 - (a.January + a.February + a.March + a.April + a.May + a.June + a.July + a.August + a.September + a.November + a.December)
                                        }).ToList();
                }
                ViewBag.SelYear = DateTime.Now.Year;
                return View(vmodel);
            }
        }

        #endregion

        #region itemwise
        [QkAuthorize(Roles = "Dev,Purchase Item Wise")]
        public ActionResult ItemWise()
        {
            ViewBag.SalesMan = QkSelect.List(
           new List<SelectListItem>
           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
           }, "Value", "Text", 0);
            ViewBag.Category = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            ViewBag.SalesExec = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);
            ViewBag.brand = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);
            var OptAll = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false  },
                            }, "Value", "Text", 0);

            ViewBag.Item = OptAll;


            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();

            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            return View();
        }
        [HttpPost]
        
        public ActionResult ItemWise(long?[] ddlItem, string From, string To, long? ddlMC, string SaleType, long? ddlItemBrand, long? ddlEmployee, long? ddlItemCategory, long? ddlSalesman)
        {
            var items = "";
            if (ddlItem != null)
            {
                items = String.Join(",", ddlItem);
            }




            return RedirectToAction("ViewItemWise", new { item = items, from = From, to = To, ddMC = ddlMC, saletype = SaleType, brand = ddlItemBrand, salesExc = ddlEmployee, category = ddlItemCategory, salesman = ddlSalesman });
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Purchase Item Wise")]
        public ActionResult ViewItemWise(string item, string from, string to, long? ddMC, string saletype, long? brand, long? salesExc, long? category, long? salesman)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;
            var items = String.Join(",", item);
            //                    where a.ItemID == ddlItem
            //                        ItemName = a.ItemCode + "-" + a.ItemName
            ViewBag.item = item;
            ViewBag.ddlmc = ddMC;
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            ViewBag.SaleTy = saletype;
            companySet();
            return View();
        }
        public ActionResult PurchasePricecomparison()
        {
            ViewBag.Item = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);
            ViewBag.Category = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);
            ViewBag.Brand = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);
            ViewBag.Supplier = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            
            var pay = db.PaymentMethods.Select(s => new
            {
                ID = s.PaymentMethodId,
                Name = s.MethodName
            }).ToList();
            
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            //    .Select(s => new
            //        ID = s.ProjectId,
            //        Name = s.ProjectName
            //    })

            ViewBag.getProj = QkSelect.List(
                                   new List<SelectListItem>
                                   {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                   }, "Value", "Text", 0);

            ViewBag.getProTask = QkSelect.List(
                                  new List<SelectListItem>
                                  {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                  }, "Value", "Text", 0);
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Purchase Item Wise")]
        public ActionResult GetItemWise(string item, string fromdate, string todate, long? ddlMC, string Salety, long? Brand, long? SalesExecutive, long? Category, long? Salesman)
        {
            if (item != "")
            {
                var UserId = User.Identity.GetUserId();
                var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
                var MCList = MCList1;
                if (!MCList.Any() && (ddlMC == 0 || ddlMC == 1))
                {
                    MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
                }
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
                var MCArray = MCList.ToArray();
                int[] items = item.Split(',').Select(x => int.Parse(x)).ToArray();
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                int recordsTotal = 0;
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                DateTime? fdate = null;
                DateTime? tdate = null;
                if (fromdate != "")
                {
                    fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
                }
                if (todate != "")
                {
                    tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
                }
                List<ItemList> v1 = new List<ItemList>();
                foreach (long itemid in items)
                {
                    var v = (from a in db.Items
                             join e in db.ItemUnits on a.ItemUnitID equals e.ItemUnitID into primary
                             from e in primary.DefaultIfEmpty()
                             join f in db.ItemUnits on a.SubUnitId equals f.ItemUnitID into second
                             from f in second.DefaultIfEmpty()
                             where (itemid == 0 || a.ItemID == itemid)
                             select new ItemList
                             {
                                 ItemID = a.ItemID,
                                 Itemname = a.ItemName,
                                 Item = a.ItemCode + "-" + a.ItemName,
                                 PriUnit = e.ItemUnitName,
                                 SubUnit = f.ItemUnitName,
                                 ConFactor = a.ConFactor != 0 ? a.ConFactor : 1,
                                 ItemUnitID = a.ItemUnitID,
                                 SubUnitId = a.SubUnitId,
                                 SellingPrice = a.SellingPrice,
                                 PartNumber = a.PartNumber,
                                 SaleType = a.Branch,
                                 PurchasePrice = a.PurchasePrice,
                                 PripurQty = (decimal?)(from i in db.PEItemss
                                                        join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                                        where (fromdate == "" || EF.Functions.DateDiffDay(j.PEDate, fdate) <= 0) &&
                                                        (todate == "" || EF.Functions.DateDiffDay(j.PEDate, tdate) >= 0) &&
                                                        (i.Item == a.ItemID && i.ItemUnit == a.ItemUnitID)
                                                        && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)
                                                        group i by i.ItemId into g
                                                        select new
                                                        {
                                                            Total = g.Sum(x => x.ItemQuantity)
                                                        }).FirstOrDefault().Total ?? 0,
                                 SubpurQty = (decimal?)(from i in db.PEItemss
                                                        join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                                        where (fromdate == "" || EF.Functions.DateDiffDay(j.PEDate, fdate) <= 0) &&
                                                         (todate == "" || EF.Functions.DateDiffDay(j.PEDate, tdate) >= 0) &&
                                                         (i.Item == a.ItemID && i.ItemUnit == a.SubUnitId) &&
                                                         e.ItemUnitName != f.ItemUnitName
                                                         && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)
                                                        group i by i.ItemId into g
                                                        select new
                                                        {
                                                            Total = g.Sum(x => x.ItemQuantity)
                                                        }).FirstOrDefault().Total ?? 0,
                                 purAmt = (decimal?)(from i in db.PEItemss
                                                     join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                                     where (fromdate == "" || EF.Functions.DateDiffDay(j.PEDate, fdate) <= 0) &&
                                                      (todate == "" || EF.Functions.DateDiffDay(j.PEDate, tdate) >= 0) &&
                                                      (i.Item == a.ItemID)
                                                      && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)
                                                     group i by i.ItemId into g
                                                     select new
                                                     {
                                                         Total = g.Sum(x => x.ItemTotalAmount)
                                                     }).FirstOrDefault().Total ?? 0,
                                 Pripur = (decimal?)(from i in db.PEItemss
                                                     join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                                     where (fromdate == "" || EF.Functions.DateDiffDay(j.PEDate, fdate) <= 0) &&
                                                      (todate == "" || EF.Functions.DateDiffDay(j.PEDate, tdate) >= 0) &&
                                                      (i.Item == a.ItemID && i.ItemUnit == e.ItemUnitID)
                                                      && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)

                                                     group i by i.ItemId into g
                                                     select new
                                                     {
                                                         Total = g.Sum(x => x.ItemSubTotal - x.ItemDiscount)
                                                     }).FirstOrDefault().Total ?? 0,
                                 Subpur = (decimal?)(from i in db.PEItemss
                                                     join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                                     where (fromdate == "" || EF.Functions.DateDiffDay(j.PEDate, fdate) <= 0) &&
                                                      (todate == "" || EF.Functions.DateDiffDay(j.PEDate, tdate) >= 0) &&
                                                      (i.Item == a.ItemID && i.ItemUnit == f.ItemUnitID) &&
                                                      e.ItemUnitName != f.ItemUnitName
                                                      && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)
                                                     group i by i.ItemId into g
                                                     select new
                                                     {
                                                         Total = g.Sum(x => x.ItemSubTotal - x.ItemDiscount)
                                                     }).FirstOrDefault().Total ?? 0,
                             }).Distinct();
                    v = v.Where(a => a.purAmt != 0).OrderBy(a => a.Itemname); //.Where(a => a.SubpurQty != 0 && a.PripurQty != 0)
                    v1.AddRange(v);                                                //v2 = v2.OrderBy(b => b.Itemname&&);
                }
                var data = v1.Skip(skip).Take(pageSize).ToList();
                recordsTotal = v1.Count();
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                var results = new ContentResult
                {
                    Content = result,
                    ContentType = "application/json"
                };
                return results;
            }
            else
            {
                if (1 == 1)
                {
                    var UserId = User.Identity.GetUserId();
                    var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
                    var MCList = MCList1;
                    if (!MCList.Any() && (ddlMC == 0 || ddlMC == 1))
                    {
                        MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
                    }
                    var MCArray = MCList.ToArray();
                    var draw = Request.Form.GetValues("draw").FirstOrDefault();
                    var start = Request.Form.GetValues("start").FirstOrDefault();
                    var length = Request.Form.GetValues("length").FirstOrDefault();
                    int recordsTotal = 0;
                    int pageSize = length != null ? Convert.ToInt32(length) : 0;
                    int skip = start != null ? Convert.ToInt32(start) : 0;
                    DateTime? fdate = null;
                    DateTime? tdate = null;
                    if (fromdate != "")
                    {
                        fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
                    }
                    if (todate != "")
                    {
                        tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
                    }
                    List<ItemList> v1 = new List<ItemList>();
                    if (1 == 1)
                    {
                        var v = (from a in db.Items
                                 join e in db.ItemUnits on a.ItemUnitID equals e.ItemUnitID into primary
                                 from e in primary.DefaultIfEmpty()
                                 join f in db.ItemUnits on a.SubUnitId equals f.ItemUnitID into second
                                 from f in second.DefaultIfEmpty()
                                 join k in db.PEItemss on a.ItemID equals k.Item into forth
                                 from k in forth.DefaultIfEmpty()
                                 join l in db.PurchaseEntrys on k.PurchaseEntry equals l.PurchaseEntryId into fifth
                                 from l in fifth.DefaultIfEmpty()
                                 join m in db.Suppliers on l.Supplier equals m.SupplierID into sixth
                                 from m in sixth.DefaultIfEmpty()
                                 where (Brand == 0 || a.ItemBrandID == Brand) &&
                                 (Category == 0 || a.ItemCategoryID == Category) &&
                                 (fromdate == "" || EF.Functions.DateDiffDay(l.PEDate, fdate) <= 0) &&
                                 (todate == "" || EF.Functions.DateDiffDay(l.PEDate, tdate) >= 0)

                                 select new ItemList
                                 {
                                     ItemID = a.ItemID,
                                     Itemname = a.ItemName,
                                     Item = a.ItemCode + "-" + a.ItemName,
                                     PriUnit = e.ItemUnitName,
                                     SubUnit = f.ItemUnitName,
                                     ConFactor = a.ConFactor != 0 ? a.ConFactor : 1,
                                     ItemUnitID = a.ItemUnitID,
                                     SubUnitId = a.SubUnitId,
                                     SellingPrice = a.SellingPrice,
                                     PartNumber = a.PartNumber,
                                     SaleType = a.Branch,
                                     PurchasePrice = a.PurchasePrice,
                                     PripurQty = (decimal?)(from i in db.PEItemss
                                                            join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                                            where (fromdate == "" || EF.Functions.DateDiffDay(j.PEDate, fdate) <= 0) &&
                                                            (todate == "" || EF.Functions.DateDiffDay(j.PEDate, tdate) >= 0) &&
                                                            (i.Item == a.ItemID && i.ItemUnit == a.ItemUnitID)
                                                            && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)
                                                            group i by i.ItemId into g
                                                            select new
                                                            {
                                                                Total = g.Sum(x => x.ItemQuantity)
                                                            }).FirstOrDefault().Total ?? 0,
                                     SubpurQty = (decimal?)(from i in db.PEItemss
                                                            join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                                            where (fromdate == "" || EF.Functions.DateDiffDay(j.PEDate, fdate) <= 0) &&
                                                             (todate == "" || EF.Functions.DateDiffDay(j.PEDate, tdate) >= 0) &&
                                                             (i.Item == a.ItemID && i.ItemUnit == a.SubUnitId) &&
                                                             e.ItemUnitName != f.ItemUnitName
                                                             && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)
                                                            group i by i.ItemId into g
                                                            select new
                                                            {
                                                                Total = g.Sum(x => x.ItemQuantity)
                                                            }).FirstOrDefault().Total ?? 0,
                                     purAmt = (decimal?)(from i in db.PEItemss
                                                         join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                                         where (fromdate == "" || EF.Functions.DateDiffDay(j.PEDate, fdate) <= 0) &&
                                                          (todate == "" || EF.Functions.DateDiffDay(j.PEDate, tdate) >= 0) &&
                                                          (i.Item == a.ItemID)
                                                          && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)
                                                         group i by i.ItemId into g
                                                         select new
                                                         {
                                                             Total = g.Sum(x => x.ItemTotalAmount)
                                                         }).FirstOrDefault().Total ?? 0,
                                     Pripur = (decimal?)(from i in db.PEItemss
                                                         join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                                         where (fromdate == "" || EF.Functions.DateDiffDay(j.PEDate, fdate) <= 0) &&
                                                          (todate == "" || EF.Functions.DateDiffDay(j.PEDate, tdate) >= 0) &&
                                                          (i.Item == a.ItemID && i.ItemUnit == e.ItemUnitID)
                                                          && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)

                                                         group i by i.ItemId into g
                                                         select new
                                                         {
                                                             Total = g.Sum(x => x.ItemSubTotal - x.ItemDiscount)
                                                         }).FirstOrDefault().Total ?? 0,
                                     Subpur = (decimal?)(from i in db.PEItemss
                                                         join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                                         where (fromdate == "" || EF.Functions.DateDiffDay(j.PEDate, fdate) <= 0) &&
                                                          (todate == "" || EF.Functions.DateDiffDay(j.PEDate, tdate) >= 0) &&
                                                          (i.Item == a.ItemID && i.ItemUnit == f.ItemUnitID) &&
                                                          e.ItemUnitName != f.ItemUnitName
                                                          && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)
                                                         group i by i.ItemId into g
                                                         select new
                                                         {
                                                             Total = g.Sum(x => x.ItemSubTotal - x.ItemDiscount)
                                                         }).FirstOrDefault().Total ?? 0,
                                 }).Distinct();
                        v = v.Where(a => a.purAmt != 0).OrderBy(a => a.Itemname); //.Where(a => a.SubpurQty != 0 && a.PripurQty != 0)
                        v1.AddRange(v);                                                //v2 = v2.OrderBy(b => b.Itemname&&);
                    }
                    var data = v1.Skip(skip).Take(pageSize).ToList();
                    recordsTotal = v1.Count();
                    JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                    javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                    string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                    var results = new ContentResult
                    {
                        Content = result,
                        ContentType = "application/json"
                    };
                    return results;
                }
            }
        }

                #endregion

                #region category wise
                [QkAuthorize(Roles = "Dev,Purchase Category Wise")]
            public ActionResult CategoryWise()
        {
            var OptAll = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Category = OptAll;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);


            return View();
        }
       
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Purchase Category Wise")]
        public ActionResult ViewCategoryWise(long? ddlItemCategory, string From, string To, long? ddlMC)
        {
            if (ddlItemCategory != 0)
            {
                ViewBag.category = (from a in db.ItemCategorys
                                    where a.ItemCategoryID == ddlItemCategory
                                    select new
                                    {
                                        ItemCategory = a.ItemCategoryName
                                    }).FirstOrDefault().ItemCategory;
            }
            else
            {
                ViewBag.category = "All";
            }
            ViewBag.fromdate = From;
            ViewBag.todate = To;
            companySet();
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Purchase Category Wise")]
        public ActionResult GetCategoryWise(long? category, string fromdate, string todate, long? ddlMC)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddlMC == 0 || ddlMC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }

            var v = (from a in db.ItemCategorys

                     where (category == 0 || a.ItemCategoryID == category)
                     select new
                     {
                         Category = a.ItemCategoryName,
                         a.ItemCategoryID,
                         purAmt = (decimal?)(from i in db.PEItemss
                                             join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                             join k in db.Items on i.Item equals k.ItemID into itm
                                             from k in itm.DefaultIfEmpty()
                                             where (fromdate == "" || EF.Functions.DateDiffDay(j.PEDate, fdate) <= 0) &&
                                             (todate == "" || EF.Functions.DateDiffDay(j.PEDate, tdate) >= 0) &&
                                             (k.ItemCategoryID == a.ItemCategoryID)
                                             && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)
                                             select new
                                             {
                                                 Total = i.ItemTotalAmount// i.ItemSubTotal - i.ItemDiscount
                                             }).Sum(x => x.Total) ?? 0,


                         RetunAmt = (decimal?)(from i in db.PRItemss
                                               join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                               join k in db.Items on i.Item equals k.ItemID into itm
                                               from k in itm.DefaultIfEmpty()
                                               where (fromdate == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                               (todate == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                               (k.ItemCategoryID == a.ItemCategoryID)
                                                && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)
                                               select new
                                               {
                                                   Total = i.ItemTotalAmount//i.ItemSubTotal - i.ItemDiscount
                                               }).Sum(x => x.Total) ?? 0,


                         NoOfVchpur = (int?)(from i in db.PEItemss
                                             join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                             join k in db.Items on i.Item equals k.ItemID into itm
                                             from k in itm.DefaultIfEmpty()
                                             where (fromdate == "" || EF.Functions.DateDiffDay(j.PEDate, fdate) <= 0) &&
                                             (todate == "" || EF.Functions.DateDiffDay(j.PEDate, tdate) >= 0) &&
                                             (k.ItemCategoryID == a.ItemCategoryID)
                                              && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)
                                             select new
                                             {
                                                 purid = i.PurchaseEntry
                                             }).GroupBy(x => x.purid).Count() ?? 0,

                         NoOfVchReturn = (int?)(from i in db.PRItemss
                                                join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                                join k in db.Items on i.Item equals k.ItemID into itm
                                                from k in itm.DefaultIfEmpty()
                                                where (fromdate == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                                (todate == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                                (k.ItemCategoryID == a.ItemCategoryID)
                                                && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)
                                                select new
                                                {
                                                    purid = i.PurchaseReturnId
                                                }).GroupBy(x => x.purid).Count() ?? 0,

                     }).Distinct().AsEnumerable().Select(o => new
                     {
                         o.Category,
                         o.ItemCategoryID,
                         o.purAmt,
                         o.RetunAmt,
                         o.NoOfVchpur,
                         o.NoOfVchReturn
                     }).Where(o => o.purAmt > 0 || o.RetunAmt > 0).OrderBy(x => x.Category);

            var data = v.Skip(skip).Take(pageSize).ToList();
            recordsTotal = v.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }





        #endregion

        #region brand wise
        [QkAuthorize(Roles = "Dev,Purchase Brand Wise")]
        public ActionResult BrandWise()
        {
            var OptAll = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Brand = OptAll;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);


            return View();
        }


        [HttpGet]
        [QkAuthorize(Roles = "Dev,Purchase Brand Wise")]
        public ActionResult ViewBrandWise(long? ddlItemBrand, string From, string To, long? ddlMC)
        {
            if (ddlItemBrand != 0)
            {
                ViewBag.brand = (from a in db.ItemBrands
                                 where a.ItemBrandID == ddlItemBrand
                                 select new
                                 {
                                     BName = a.ItemBrandName
                                 }).FirstOrDefault().BName;
            }
            else
            {
                ViewBag.brand = "All";
            }
            ViewBag.fromdate = From;
            ViewBag.todate = To;
            ViewBag.ddlmc = ddlMC;
            companySet();
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Purchase Brand Wise")]
        public ActionResult GetBrandWise(long? brand, string fromdate, string todate, long? ddlMC)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddlMC == 0 || ddlMC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }

            var v = (from a in db.ItemBrands
                     where (brand == 0 || a.ItemBrandID == brand)
                     select new
                     {
                         Brand = a.ItemBrandName,
                         a.ItemBrandID,
                         purAmt = (decimal?)(from i in db.PEItemss
                                             join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                             join k in db.Items on i.Item equals k.ItemID into itm
                                             from k in itm.DefaultIfEmpty()
                                             where (fromdate == "" || EF.Functions.DateDiffDay(j.PEDate, fdate) <= 0) &&
                                             (todate == "" || EF.Functions.DateDiffDay(j.PEDate, tdate) >= 0) &&
                                             (k.ItemBrandID == a.ItemBrandID)
                                             && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)
                                             select new
                                             {
                                                 Total = i.ItemTotalAmount//i.ItemSubTotal - i.ItemDiscount
                                             }).Sum(x => x.Total) ?? 0,


                         RetunAmt = (decimal?)(from i in db.PRItemss
                                               join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                               join k in db.Items on i.Item equals k.ItemID into itm
                                               from k in itm.DefaultIfEmpty()
                                               where (fromdate == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                               (todate == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                                (k.ItemBrandID == a.ItemBrandID)
                                                 && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)

                                               select new
                                               {
                                                   Total = i.ItemTotalAmount//ItemSubTotal - i.ItemDiscount
                                               }).Sum(x => x.Total) ?? 0,



                         NoOfVchpur = (int?)(from i in db.PEItemss
                                             join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                             join k in db.Items on i.Item equals k.ItemID into itm
                                             from k in itm.DefaultIfEmpty()
                                             where (fromdate == "" || EF.Functions.DateDiffDay(j.PEDate, fdate) <= 0) &&
                                             (todate == "" || EF.Functions.DateDiffDay(j.PEDate, tdate) >= 0) &&
                                            (k.ItemBrandID == a.ItemBrandID)
                                             && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)

                                             //group i by i.PurchaseEntry into g
                                             select new
                                             {
                                                 purid = i.PurchaseEntry
                                             }).GroupBy(x => x.purid).Count() ?? 0,


                         NoOfVchReturn = (int?)(from i in db.PRItemss
                                                join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                                join k in db.Items on i.Item equals k.ItemID into itm
                                                from k in itm.DefaultIfEmpty()
                                                where (fromdate == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                                (todate == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                                (k.ItemBrandID == a.ItemBrandID)
                                                 && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)

                                                //  group i by i.ItemId into g
                                                select new
                                                {
                                                    purid = i.PurchaseReturnId
                                                }).GroupBy(x => x.purid).Count() ?? 0,

                     }).Distinct().AsEnumerable().Select(o => new
                     {
                         o.Brand,
                         o.ItemBrandID,
                         o.purAmt,
                         o.RetunAmt,
                         o.NoOfVchpur,
                         o.NoOfVchReturn
                     }).Where(o => o.purAmt > 0 || o.RetunAmt > 0).OrderBy(x => x.Brand);

            var data = v.Skip(skip).Take(pageSize).ToList();
            recordsTotal = v.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }


        #endregion

        #region purs executive wise       

        [QkAuthorize(Roles = "Dev,Purchase Executive Wise")]
        public ActionResult PursExecutiveWise()
        {
            ViewBag.pursExec = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            return View();
        }
       
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Purchase Executive Wise")]
        public ActionResult ViewPursExecutiveWise(long? ddlEmployee, string From, string To, long? ddlMC)
        {
            if (ddlEmployee != 0)
            {
                ViewBag.pursExec = (from a in db.Employees
                                    where a.EmployeeId == ddlEmployee
                                    select new
                                    {
                                        EmpName = a.FirstName + " " + a.MiddleName + " " + a.LastName
                                    }).FirstOrDefault().EmpName;
            }
            else
            {
                ViewBag.pursExec = "All";
            }
            ViewBag.fromdate = From;
            ViewBag.todate = To;
            companySet();
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Purchase Executive Wise")]
        public ActionResult GetpursExeWise(long? pursexec, string fromdate, string todate, long? ddmc)
        {

            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddmc == 0 || ddmc == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }

            var v = (from a in db.Employees
                     where (pursexec == 0 || a.EmployeeId == pursexec)
                     select new
                     {
                         a.EmployeeId,
                         a.FirstName,
                         employee = a.FirstName + " " + a.MiddleName + " " + a.LastName,

                         purAmt = (decimal?)(from i in db.PurchaseEntrys
                                             where (fromdate == "" || EF.Functions.DateDiffDay(i.PEDate, fdate) <= 0) && i.Status == 1 &&
                                             (todate == "" || EF.Functions.DateDiffDay(i.PEDate, tdate) >= 0) &&
                                             (i.PECashier == a.EmployeeId)
                                             && (!MCList.Any() || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                             group i by i.Supplier into g
                                             select new
                                             {
                                                 Total = g.Sum(x => x.PESubTotal - x.PEDiscount)
                                             }).FirstOrDefault().Total ?? 0,
                         purtaxAmt = (decimal?)(from i in db.PurchaseEntrys
                                                where (fromdate == "" || EF.Functions.DateDiffDay(i.PEDate, fdate) <= 0) && i.Status == 1 &&
                                                (todate == "" || EF.Functions.DateDiffDay(i.PEDate, tdate) >= 0) &&
                                                (i.PECashier == a.EmployeeId)
                                                && (!MCList.Any() || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                                group i by i.Supplier into g
                                                select new
                                                {
                                                    Total = g.Sum(x => x.PETaxAmount)
                                                }).FirstOrDefault().Total ?? 0,
                         purtotAmt = (decimal?)(from i in db.PurchaseEntrys
                                                where (fromdate == "" || EF.Functions.DateDiffDay(i.PEDate, fdate) <= 0) && i.Status == 1 &&
                                                (todate == "" || EF.Functions.DateDiffDay(i.PEDate, tdate) >= 0) &&
                                                (i.PECashier == a.EmployeeId)
                                                && (!MCList.Any() || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                                group i by i.Supplier into g
                                                select new
                                                {
                                                    Total = g.Sum(x => x.PEGrandTotal)
                                                }).FirstOrDefault().Total ?? 0,
                         RetunAmt = (decimal?)(from i in db.PurchaseReturns
                                               where (fromdate == "" || EF.Functions.DateDiffDay(i.PRDate, fdate) <= 0) &&
                                               (todate == "" || EF.Functions.DateDiffDay(i.PRDate, tdate) >= 0) &&
                                               (i.PRCashier == a.EmployeeId)
                                               && (!MCList.Any() || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                               group i by i.Supplier into g
                                               select new
                                               {
                                                   Total = g.Sum(x => x.PRSubTotal - x.PRDiscount)
                                               }).FirstOrDefault().Total ?? 0,
                         RetuntaxAmt = (decimal?)(from i in db.PurchaseReturns
                                                  where (fromdate == "" || EF.Functions.DateDiffDay(i.PRDate, fdate) <= 0) &&
                                                  (todate == "" || EF.Functions.DateDiffDay(i.PRDate, tdate) >= 0) &&
                                                  (i.PRCashier == a.EmployeeId)
                                                  && (!MCList.Any() || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                                  group i by i.Supplier into g
                                                  select new
                                                  {
                                                      Total = g.Sum(x => x.PRTaxAmount)
                                                  }).FirstOrDefault().Total ?? 0,
                         RetuntotAmt = (decimal?)(from i in db.PurchaseReturns
                                                  where (fromdate == "" || EF.Functions.DateDiffDay(i.PRDate, fdate) <= 0) &&
                                                  (todate == "" || EF.Functions.DateDiffDay(i.PRDate, tdate) >= 0) &&
                                                  (i.PRCashier == a.EmployeeId)
                                                  && (!MCList.Any() || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                                  group i by i.Supplier into g
                                                  select new
                                                  {
                                                      Total = g.Sum(x => x.PRGrandTotal)
                                                  }).FirstOrDefault().Total ?? 0,

                         NoOfVchpur = (int?)(from i in db.PurchaseEntrys
                                             where (fromdate == "" || EF.Functions.DateDiffDay(i.PEDate, fdate) <= 0) && i.Status == 1 &&
                                             (todate == "" || EF.Functions.DateDiffDay(i.PEDate, tdate) >= 0) &&
                                            (i.PECashier == a.EmployeeId)
                                            && (!MCList.Any() || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                             select new
                                             {
                                                 purid = i.PurchaseEntryId
                                             }).Count() ?? 0,
                         NoOfVchReturn = (int?)(from j in db.PurchaseReturns
                                                where (fromdate == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                                (todate == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                                (j.PRCashier == a.EmployeeId)
                                                && (!MCList.Any() || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                select new
                                                {
                                                    purid = j.PurchaseReturnId
                                                }).Count() ?? 0,
                     }).OrderBy(a => a.FirstName);

            var data = v.Skip(skip).Take(pageSize).ToList();
            recordsTotal = v.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

        }


        #endregion

        #region itemDetails
        public ActionResult itemDetails(long? iditem, long? supplier, string from, string to, long? ddmc)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            if (iditem != 0)
            {
                ViewBag.item = (from a in db.Items
                                where a.ItemID == iditem
                                select new
                                {
                                    ItemName = a.ItemCode + "-" + a.ItemName
                                }).FirstOrDefault().ItemName;
            }
            else
            {
                ViewBag.item = "All";
            }
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            ViewBag.supplier = supplier;
            companySet();
            return View();
        }
        
        public ActionResult itembrand(long? brand, string from, string to, long? ddmc)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            if (brand != 0)
            {
                ViewBag.brand = (from a in db.ItemBrands
                                 where a.ItemBrandID == brand
                                 select new
                                 {
                                     brandName = a.ItemBrandName
                                 }).FirstOrDefault().brandName;
            }
            else
            {
                ViewBag.item = "All";
            }
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            ViewBag.ddlmc = ddmc;
            companySet();
            return View();
        }
        public ActionResult getitembrand(long? brand, string fromdate, string to, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddmc == 0 || ddmc == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (to != "")
            {
                tdate = DateTime.Parse(to, new CultureInfo("en-GB"));
            }
            var v = (from a in db.PEItemss
                     join b in db.Items on a.Item equals b.ItemID
                     join g in db.PurchaseEntrys on a.PurchaseEntry equals g.PurchaseEntryId
                     join e in db.ItemUnits on b.ItemUnitID equals e.ItemUnitID into primary
                     from e in primary.DefaultIfEmpty()
                     join f in db.ItemUnits on b.SubUnitId equals f.ItemUnitID into second
                     from f in second.DefaultIfEmpty()
                     where (fromdate == "" || EF.Functions.DateDiffDay(g.PEDate, fdate) <= 0)
                     && (to == "" || EF.Functions.DateDiffDay(g.PEDate, tdate) >= 0)
                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(g.MaterialCenter) || ddmc == g.MaterialCenter)
                     && (b.ItemBrandID == brand)
                     select new
                     {
                         b.ItemID,
                         b.ItemName,
                         b.ItemCode,
                         Item = b.ItemCode + "-" + b.ItemName,
                         Unit = (a.ItemUnit == b.ItemUnitID) ? e.ItemUnitName : f.ItemUnitName,
                         g.PurchaseEntryId,
                         g.BillNo,
                         a.ItemQuantity,
                         a.ItemSubTotal,
                         a.ItemTaxAmount,
                         a.ItemTotalAmount,
                         a.ItemDiscount,
                         a.itemNote,
                         a.ItemUnitPrice,
                         PartNumber = (b.PartNumber != null && b.PartNumber != "") ? b.PartNumber : "",
                         PriUnit = e.ItemUnitName,
                         SubUnit = f.ItemUnitName,
                         ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                         b.ItemUnitID,
                         b.SubUnitId,
                         b.SellingPrice,
                     }).AsEnumerable().OrderBy(a => a.ItemName);

            var data = v.Skip(skip).Take(pageSize).ToList();
            recordsTotal = v.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

        }

        public ActionResult getitemDetails(long? item, long? supplier, string fromdate, string to, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddmc == 0 || ddmc == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (to != "")
            {
                tdate = DateTime.Parse(to, new CultureInfo("en-GB"));
            }

            var v = (from a in db.PEItemss
                     join b in db.Items on a.Item equals b.ItemID
                     join g in db.PurchaseEntrys on a.PurchaseEntry equals g.PurchaseEntryId

                     join e in db.ItemUnits on b.ItemUnitID equals e.ItemUnitID into primary
                     from e in primary.DefaultIfEmpty()
                     join f in db.ItemUnits on b.SubUnitId equals f.ItemUnitID into second
                     from f in second.DefaultIfEmpty()
                     join s in db.Suppliers on g.Supplier equals s.SupplierID
                     where (item == 0 || a.Item == item) && (supplier == 0 || g.Supplier == supplier)
                     && (fromdate == "" || EF.Functions.DateDiffDay(g.PEDate, fdate) <= 0)
                     && (to == "" || EF.Functions.DateDiffDay(g.PEDate, tdate) >= 0)
                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(g.MaterialCenter) || ddmc == g.MaterialCenter)
                     select new
                     {
                         b.ItemID,
                         b.ItemName,
                         b.ItemCode,
                         Item = b.ItemCode + "-" + b.ItemName,
                         Unit = (a.ItemUnit == b.ItemUnitID) ? e.ItemUnitName : f.ItemUnitName,
                         g.PurchaseEntryId,
                         g.BillNo,
                         a.ItemQuantity,
                         a.ItemSubTotal,
                         a.ItemTaxAmount,
                         a.ItemTotalAmount,
                         a.ItemDiscount,
                         a.itemNote,
                         a.ItemUnitPrice,
                         PartNumber = (b.PartNumber != null && b.PartNumber != "") ? b.PartNumber : "",
                         PriUnit = e.ItemUnitName,
                         SubUnit = f.ItemUnitName,
                         ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                         b.ItemUnitID,
                         b.SubUnitId,
                         b.SellingPrice,
                         s.SupplierName
                     }).AsEnumerable().OrderBy(a => a.ItemName);

            var data = v.Skip(skip).Take(pageSize).ToList();
            recordsTotal = v.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

        }
        #endregion

        #region supDetails
        public ActionResult supDetails(long? sup, string from, string to, long? ddmc)
        {
            if (sup != 0)
            {
                ViewBag.cust = (from a in db.Suppliers
                                where a.SupplierID == sup
                                select new
                                {
                                    Name = a.SupplierName
                                }).FirstOrDefault().Name;
            }
            else
            {
                ViewBag.item = "All";
            }
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            companySet();
            return View();
        }
        public ActionResult getsupDetails(long? sups, string fromdate, string to, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddmc == 0 || ddmc == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (to != "")
            {
                tdate = DateTime.Parse(to, new CultureInfo("en-GB"));
            }

            var v = (from a in db.PurchaseEntrys
                     join b in db.Suppliers on a.Supplier equals b.SupplierID
                     where (sups == 0 || a.Supplier == sups) && a.Status == 1
                     && (fromdate == "" || EF.Functions.DateDiffDay(a.PEDate, fdate) <= 0)
                     && (to == "" || EF.Functions.DateDiffDay(a.PEDate, tdate) >= 0)
                     && (!MCList.Any() || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                     select new
                     {
                         b.SupplierName,
                         a.BillNo,
                         a.PEDiscount,
                         a.PEGrandTotal,
                         a.PESubTotal,
                         a.PETaxAmount,
                         a.PENote,
                         a.PEDate,
                         a.PurchaseEntryId,
                         fromdate,
                         to
                     }).AsEnumerable().OrderBy(a => a.PurchaseEntryId);

            var data = v.Skip(skip).Take(pageSize).ToList();
            recordsTotal = v.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }
        #endregion

        #region supplier itemwise
        [QkAuthorize(Roles = "Dev,Purchase Supplier ItemWise")]
        public ActionResult SupplierItemWise()
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            ViewBag.Item = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);

            //    SupplierID = s.SupplierID,
            //    SupplierDetails = s.SupplierCode + " - " + s.SupplierName

            ViewBag.Supplier = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            companySet();
            return View();
        }

        [QkAuthorize(Roles = "Dev,Purchase Supplier ItemWise")]
        public ActionResult GetSupplierItemWise(long ddlSupplier, long? ddlItem, string From, string To, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddmc == 0 || ddmc == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (From != "")
            {
                fdate = DateTime.Parse(From, new CultureInfo("en-GB"));
            }
            if (To != "")
            {
                tdate = DateTime.Parse(To, new CultureInfo("en-GB"));
            }

            var v = (from a in db.Items                         
                     join e in db.ItemUnits on a.ItemUnitID equals e.ItemUnitID into primary
                     from e in primary.DefaultIfEmpty()
                     join f in db.ItemUnits on a.SubUnitId equals f.ItemUnitID into second
                     from f in second.DefaultIfEmpty()

                     where (ddlItem == 0 || a.ItemID == ddlItem)
                     //(todate == "" || EF.Functions.DateDiffDay(a.PEDate, tdate) >= 0))
                     select new
                     {
                         a.ItemID,
                         a.ItemName,
                         Item = a.ItemCode + "-" + a.ItemName,
                         PriUnit = e.ItemUnitName,
                         SubUnit = f.ItemUnitName,
                         ConFactor = a.ConFactor != 0 ? a.ConFactor : 1,
                         a.ItemUnitID,
                         a.PartNumber,
                         a.SubUnitId,
                         a.SellingPrice,


                         PripurQty = (decimal?)(from i in db.PEItemss
                                                join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                                where (From == "" || EF.Functions.DateDiffDay(j.PEDate, fdate) <= 0) &&
                                                (To == "" || EF.Functions.DateDiffDay(j.PEDate, tdate) >= 0) &&
                                                (i.Item == a.ItemID && i.ItemUnit == a.ItemUnitID) && (j.Supplier == ddlSupplier)
                                                && (!MCList.Any() || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                group i by i.Item into g
                                                select new
                                                {
                                                    Total = g.Sum(x => x.ItemQuantity)
                                                }).FirstOrDefault().Total ?? 0,

                         SubpurQty = (decimal?)(from i in db.PEItemss
                                                join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                                where (From == "" || EF.Functions.DateDiffDay(j.PEDate, fdate) <= 0) &&
                                                (To == "" || EF.Functions.DateDiffDay(j.PEDate, tdate) >= 0) &&
                                                (i.Item == a.ItemID && i.ItemUnit == a.SubUnitId) &&
                                                e.ItemUnitName != f.ItemUnitName && (j.Supplier == ddlSupplier)
                                                && (!MCList.Any() || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                group i by i.Item into g
                                                select new
                                                {
                                                    Total = g.Sum(x => x.ItemQuantity)
                                                }).FirstOrDefault().Total ?? 0,

                         PriRetQty = (decimal?)(from i in db.PRItemss
                                                join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                                where (From == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                                (To == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                                (i.Item == a.ItemID && i.ItemUnit == a.ItemUnitID) && (j.Supplier == ddlSupplier)
                                                && (!MCList.Any() || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                group i by i.Item into g
                                                select new
                                                {
                                                    Total = g.Sum(x => x.ItemQuantity)
                                                }).FirstOrDefault().Total ?? 0,

                         SubRetQty = (decimal?)(from i in db.PRItemss
                                                join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                                where (From == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                                (To == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                                (i.Item == a.ItemID && i.ItemUnit == a.SubUnitId) &&
                                                 e.ItemUnitName != f.ItemUnitName && (j.Supplier == ddlSupplier)
                                                 && (!MCList.Any() || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                group i by i.Item into g
                                                select new
                                                {
                                                    Total = g.Sum(x => x.ItemQuantity)
                                                }).FirstOrDefault().Total ?? 0,

                         
                         purAmt = (decimal?)(from i in db.PEItemss
                                             join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                             where (From == "" || EF.Functions.DateDiffDay(j.PEDate, fdate) <= 0) &&
                                             (To == "" || EF.Functions.DateDiffDay(j.PEDate, tdate) >= 0) &&
                                             (i.Item == a.ItemID) && (j.Supplier == ddlSupplier)
                                             && (!MCList.Any() || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                             group i by i.Item into g
                                             select new
                                             {
                                                 Total = g.Sum(x => x.ItemSubTotal - x.ItemDiscount)
                                             }).FirstOrDefault().Total ?? 0,
                         Pripur = (decimal?)(from i in db.PEItemss
                                             join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                             where (From == "" || EF.Functions.DateDiffDay(j.PEDate, fdate) <= 0) &&
                                             (To == "" || EF.Functions.DateDiffDay(j.PEDate, tdate) >= 0) &&
                                             (i.Item == a.ItemID && i.ItemUnit == e.ItemUnitID) && (j.Supplier == ddlSupplier)
                                             && (!MCList.Any() || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                             group i by i.Item into g
                                             select new
                                             {
                                                 Total = g.Sum(x => x.ItemSubTotal - x.ItemDiscount)
                                             }).FirstOrDefault().Total ?? 0,
                         Subpur = (decimal?)(from i in db.PEItemss
                                             join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                             where (From == "" || EF.Functions.DateDiffDay(j.PEDate, fdate) <= 0) &&
                                             (To == "" || EF.Functions.DateDiffDay(j.PEDate, tdate) >= 0) &&
                                             (i.Item == a.ItemID && i.ItemUnit == f.ItemUnitID) &&
                                             e.ItemUnitName != f.ItemUnitName && (j.Supplier == ddlSupplier)
                                             && (!MCList.Any() || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                             group i by i.Item into g
                                             select new
                                             {
                                                 Total = g.Sum(x => x.ItemSubTotal - x.ItemDiscount)
                                             }).FirstOrDefault().Total ?? 0,


                         RetunAmt = (decimal?)(from i in db.PRItemss
                                               join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                               where (From == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                               (To == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                               (i.Item == a.ItemID) && (j.Supplier == ddlSupplier)
                                               && (!MCList.Any() || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                               group i by i.Item into g
                                               select new
                                               {
                                                   Total = g.Sum(x => x.ItemSubTotal - x.ItemDiscount)
                                               }).FirstOrDefault().Total ?? 0,


                         NoOfVchpur = (int?)(from i in db.PEItemss
                                             join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                             where (From == "" || EF.Functions.DateDiffDay(j.PEDate, fdate) <= 0) &&
                                             (To == "" || EF.Functions.DateDiffDay(j.PEDate, tdate) >= 0) &&
                                             (i.Item == a.ItemID) && (j.Supplier == ddlSupplier)
                                             && (!MCList.Any() || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                             //group i by i.PurchaseEntry into g
                                             select new
                                             {
                                                 purid = i.PurchaseEntry
                                             }).GroupBy(x => x.purid).Count() ?? 0,
                         
                         NoOfVchReturn = (int?)(from i in db.PRItemss
                                                join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                                where (From == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                                (To == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                                (i.Item == a.ItemID) && (j.Supplier == ddlSupplier)
                                                && (!MCList.Any() || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                //  group i by i.ItemId into g
                                                select new
                                                {
                                                    purid = i.PurchaseReturnId
                                                }).GroupBy(x => x.purid).Count() ?? 0,

                     }).Distinct().AsEnumerable().Select(o => new
                     {
                         o.ItemID,
                         o.Item,
                         o.ItemUnitID,
                         o.SubUnitId,
                         PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                         SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                         o.ConFactor,

                         AvgPrice = ((o.Pripur * o.ConFactor) + o.Subpur) / (o.NoOfVchpur != 0 ? o.NoOfVchpur : 1),
                         oldPripurQty = o.PripurQty,
                         oldSubpurQty = o.SubpurQty,
                         PripurQty = (o.PriUnit != o.SubUnit) ? (o.PripurQty + (int)(o.SubpurQty / o.ConFactor)) : o.PripurQty,
                         SubpurQty = (o.SubpurQty % o.ConFactor),

                         PriRetQty = (o.PriRetQty + (int)(o.SubRetQty / o.ConFactor)),
                         SubRetQty = (o.SubRetQty % o.ConFactor),
                         PartNumber = (o.PartNumber != null && o.PartNumber != "") ? o.PartNumber : "",




                         NetQty = (((o.PripurQty - o.PriRetQty) * o.ConFactor) + (o.SubpurQty - o.SubRetQty)),

                         o.purAmt,
                         o.RetunAmt,
                         o.NoOfVchpur,
                         o.NoOfVchReturn,
                         o.ItemName,
                     }).OrderBy(a => a.ItemName);

            var data = v.Skip(skip).Take(pageSize).ToList();
            recordsTotal = v.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

        }

        #endregion

        #region MonthlyPurchase

        [QkAuthorize(Roles = "Dev,MonthWise Purchase Horizontal")]
        public ActionResult MonthlyPurchase()
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,MonthWise Purchase Horizontal")]
        public ActionResult MonthlyPurchase(string From, string To, long? ddlMC)
        {
            return RedirectToAction("ViewMonthlyPurchase", new { from = From, to = To, ddmc = ddlMC });
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,MonthWise Purchase Horizontal")]
        public ActionResult ViewMonthlyPurchase(string from, string to, long? ddlMC)
        {
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            ViewBag.ddlmc = ddlMC;
            companySet();
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,MonthWise Purchase Horizontal")]
        public ActionResult GetMonthlyPurchase(string fromdate, string todate, long? ddmc)
        {

            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddmc == 0 || ddmc == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            DateTime? td = DateTime.Parse(todate, new CultureInfo("en-GB"));
            DateTime tdate = td.Value.AddMonths(1).AddDays(-1);

            var count = 0;
            var months = new List<DateTime>();
            List<MonthlySalesReportVM> monthwise = new List<MonthlySalesReportVM>();
            for (var dt = fdate; dt <= tdate; dt = dt.AddDays(1))
            {
                count++;
                months.Add(dt.AddDays(1));
                monthwise.Add(new MonthlySalesReportVM() { MonthYear = dt });
            }
            var pentry = (from a in db.PurchaseEntrys
                          where (fromdate == "" || EF.Functions.DateDiffDay(a.PEDate, fdate) <= 0) && a.Status == 1 &&
                                (todate == "" || EF.Functions.DateDiffDay(a.PEDate, tdate) >= 0)
                                && (!MCList.Any() || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                          select new
                          {
                              a.PurchaseEntryId,
                              a.BillNo,
                              a.PEDate,
                              a.PETaxAmount,
                              PESubTotal = a.PESubTotal - a.PEDiscount,
                              a.PETax,
                              a.PEGrandTotal,

                          }).GroupBy(x => new { x.PEDate }, (key, group) => new
                          {
                              MonthYear = (DateTime)key.PEDate,

                              PETax = group.Sum(k => k.PETax),
                              PETaxAmount = group.Sum(k => k.PETaxAmount),
                              PESubTotal = group.Sum(k => k.PESubTotal),
                              PEGrandTotal = group.Sum(k => k.PEGrandTotal),

                              PRTax = (decimal)0,
                              PRTaxAmount = (decimal)0,
                              PRSubTotal = (decimal)0,
                              PRGrandTotal = (decimal)0
                          }).ToList();

            var preturn = (from a in db.PurchaseReturns
                           where (fromdate == "" || EF.Functions.DateDiffDay(a.PRDate, fdate) <= 0) &&
                                 (todate == "" || EF.Functions.DateDiffDay(a.PRDate, tdate) >= 0)
                                 && (!MCList.Any() || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                           select new
                           {
                               a.PurchaseReturnId,
                               a.BillNo,
                               a.PRDate,
                               a.PRTaxAmount,
                               PRSubTotal = a.PRSubTotal - a.PRDiscount,
                               a.PRTax,
                               a.PRGrandTotal,
                           }).GroupBy(x => new { x.PRDate }, (key, group) => new
                           {
                               MonthYear = (DateTime)key.PRDate,
                               PRTax = group.Sum(k => k.PRTax),
                               PRTaxAmount = group.Sum(k => k.PRTaxAmount),
                               PRSubTotal = group.Sum(k => k.PRSubTotal),
                               PRGrandTotal = group.Sum(k => k.PRGrandTotal),

                               PETax = (decimal)0,
                               PETaxAmount = (decimal)0,
                               PESubTotal = (decimal)0,
                               PEGrandTotal = (decimal)0
                           }).ToList();


            var sjoin = (from a in monthwise
                         join b in pentry on a.MonthYear equals b.MonthYear into txn
                         from b in txn.DefaultIfEmpty()
                         select new
                         {
                             MonthYear = (DateTime)a.MonthYear,
                             PETax = b != null ? b.PETax : 0,
                             PETaxAmount = b != null ? b.PETaxAmount : 0,
                             PESubTotal = b != null ? b.PESubTotal : 0,
                             PEGrandTotal = b != null ? b.PEGrandTotal : 0,
                         }).ToList();


            var rjoin = (from a in monthwise
                         join b in preturn on a.MonthYear equals b.MonthYear into txn
                         from b in txn.DefaultIfEmpty()
                         select new
                         {
                             MonthYear = (DateTime)a.MonthYear,
                             PRTax = b != null ? b.PRTax : 0,
                             PRTaxAmount = b != null ? b.PRTaxAmount : 0,
                             PRSubTotal = b != null ? b.PRSubTotal : 0,
                             PRGrandTotal = b != null ? b.PRGrandTotal : 0,
                         }).ToList();


            var result = (from a in monthwise
                          join b in sjoin on a.MonthYear equals b.MonthYear into se
                          from b in se.DefaultIfEmpty()
                          join c in rjoin on a.MonthYear equals c.MonthYear into sr
                          from c in sr.DefaultIfEmpty()
                          select new
                          {
                              MonthYear = a.MonthYear,
                              PETax = b != null ? b.PETax : 0,
                              PETaxAmount = b != null ? b.PETaxAmount : 0,
                              PESubTotal = b != null ? b.PESubTotal : 0,
                              PEGrandTotal = b != null ? b.PEGrandTotal : 0,

                              PRTax = c != null ? c.PRTax : 0,
                              PRTaxAmount = c != null ? c.PRTaxAmount : 0,
                              PRSubTotal = c != null ? c.PRSubTotal : 0,
                              PRGrandTotal = c != null ? c.PRGrandTotal : 0,

                              NetAmount = b.PEGrandTotal - c.PRGrandTotal

                          }).GroupBy(x => new { Years = x.MonthYear.Value.Year, Months = x.MonthYear.Value.Month }, (key, group) => new
                          {
                              MonthYear = CustHtml.MonthName(key.Months.ToString()) + " - " + key.Years.ToString(),
                              PETax = group.Sum(k => k.PETax),
                              PETaxAmount = group.Sum(k => k.PETaxAmount),
                              PESubTotal = group.Sum(k => k.PESubTotal),
                              PEGrandTotal = group.Sum(k => k.PEGrandTotal),

                              PRTax = group.Sum(k => k.PRTax),
                              PRTaxAmount = group.Sum(k => k.PRTaxAmount),
                              PRSubTotal = group.Sum(k => k.PRSubTotal),
                              PRGrandTotal = group.Sum(k => k.PRGrandTotal),
                              NetAmount = group.Sum(k => k.NetAmount),
                          }).ToList();

            var data = result.Skip(skip).Take(pageSize).ToList();
            recordsTotal = result.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string resultss = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = resultss,
                ContentType = "application/json"
            };
            return results;
        }

        #endregion MonthlyPurchase

        #region DailyPurchase

        [QkAuthorize(Roles = "Dev,Purchase Day Wise")]
        public ActionResult DayWise()
        {
            ViewBag.Supplier = QkSelect.List(
                                            new List<SelectListItem>
                                            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                            }, "Value", "Text", 0);

            ViewBag.SalesExecutive = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Purchase", Value="1"},
                new SelectListItem() {Text = "CrossHire", Value="2"},
            }, "Value", "Text");

            var hiretype = db.HireTypes
                             .Select(s => new
                             {
                                 ID = s.HireTypeId,
                                 Name = s.Name
                             })
                             .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Purchase Day Wise")]
        public ActionResult DayWise(string From, string To, long? ddlMC, long? ddlEmployee, long? ddlSupplier, string FromDate, string ToDate, long? HireType, string PurType)
        {
            return RedirectToAction("ViewDayWise", new { from = From, to = To, ddmc = ddlMC, emp = ddlEmployee, supplier = ddlSupplier, hfrom = FromDate, hto = ToDate, htype = HireType, ptype = PurType });
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Purchase Day Wise")]
        public ActionResult GetDayWise(string From, string To, long? ddmc, long? emp, long? supplier, string hfrom, string hto, long? htype, string ptype)
        {

            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddmc == 0 || ddmc == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (From != "")
            {
                fdate = DateTime.Parse(From, new CultureInfo("en-GB"));
            }
            if (To != "")
            {
                tdate = DateTime.Parse(To, new CultureInfo("en-GB"));
            }

            DateTime? hfrmdate = null;
            DateTime? htodate = null;
            if (hfrom != "")
            {
                hfrmdate = DateTime.Parse(hfrom, new CultureInfo("en-GB"));
            }
            if (hto != "")
            {
                htodate = DateTime.Parse(hto, new CultureInfo("en-GB"));
            }
            PurchaseHireType Pt = new PurchaseHireType();
            if (ptype != "")
            {
                Pt = (ptype == "1") ? PurchaseHireType.Purchase : PurchaseHireType.CrossHire;
            };

            DateTime fdates = DateTime.Parse(From, new CultureInfo("en-GB"));
            DateTime tdates = DateTime.Parse(To, new CultureInfo("en-GB"));

            var count = 0;
            var dates = new List<DateTime>();
            for (var dt = fdates; dt <= tdates; dt = dt.AddDays(1))
            {
                count++;
                dates.Add(dt.Date);
            }


            var sale = (from a in db.PurchaseEntrys
                        join h in db.HireDetails on new { h1 = a.PurchaseEntryId, h2 = "Purchase" }
                        equals new { h1 = h.Reference, h2 = h.Section } into hir
                        from h in hir.DefaultIfEmpty()
                        where (supplier == 0 || a.Supplier == supplier)
                        && (emp == 0 || emp == null || a.PECashier == emp)
                        && (From == "" || EF.Functions.DateDiffDay(a.PEDate, fdate) <= 0) && a.Status == 1
                        && (To == "" || EF.Functions.DateDiffDay(a.PEDate, tdate) >= 0)
                        && ((!MCList.Any() && ddmc == null)||ddmc==0 || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                        && (ptype == "" || a.PurType == Pt)
                        && (htype == 0 || htype == null || h.HireType == htype)
                        && (hfrom == "" || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0)
                        && (hto == "" || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)
                        select new
                        {

                            Date = a.PEDate,
                            a.PurchaseEntryId,
                            a.PETaxAmount,
                            a.PEGrandTotal,
                        }).GroupBy(x => new { x.Date }, (y, group) => new
                        {
                            SaleCount = group.Select(k => k.PurchaseEntryId).Count(),
                            Date = y.Date,
                            SaleTax = group.Sum(k => k.PETaxAmount),
                            TotalSale = group.Sum(k => k.PEGrandTotal),
                            RetSaleTax = (decimal)0,
                            RetTotalSale = (decimal)0,
                            ReturnCount = 0
                        }).ToList();


            var sreturn = (from a in db.PurchaseReturns
                           where (supplier == 0 || a.Supplier == supplier)
                           && (emp == 0 || emp == null || a.PRCashier == emp)
                           && (From == "" || EF.Functions.DateDiffDay(a.PRDate, fdate) <= 0) && a.Status == 1
                           && (To == "" || EF.Functions.DateDiffDay(a.PRDate, tdate) >= 0)
                           && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                           && (ptype == "" || a.PurType == Pt)
                           select new
                           {
                               Date = a.PRDate,
                               a.PurchaseReturnId,
                               a.PRTaxAmount,
                               a.PRGrandTotal
                           }).GroupBy(x => new { x.Date }, (y, group) => new
                           {
                               ReturnCount = group.Select(k => k.PurchaseReturnId).Count(),
                               Date = y.Date,
                               RetSaleTax = group.Sum(k => k.PRTaxAmount),
                               RetTotalSale = group.Sum(k => k.PRGrandTotal),
                               SaleTax = (decimal)0,
                               TotalSale = (decimal)0,
                               SaleCount = 0
                           }).ToList();


            var sjoin = (from a in dates
                         join b in sale on a.Date equals b.Date into trans
                         from b in trans.DefaultIfEmpty()
                         select new
                         {
                             a.Date,
                             SaleCount = b != null ? b.SaleCount : 0,
                             SaleTax = b != null ? b.SaleTax : 0,
                             TotalSale = b != null ? b.TotalSale : 0,
                         }).ToList();


            var rjoin = (from a in dates
                         join b in sreturn on a.Date equals b.Date into trans
                         from b in trans.DefaultIfEmpty()
                         select new
                         {
                             a.Date,
                             ReturnCount = b != null ? b.ReturnCount : 0,
                             RetSaleTax = b != null ? b.RetSaleTax : 0,
                             RetTotalSale = b != null ? b.RetTotalSale : 0,
                         }).ToList();


            var result = (from a in dates
                          join b in sjoin on a.Date equals b.Date into se
                          from b in se.DefaultIfEmpty()
                          join c in rjoin on a.Date equals c.Date into sr
                          from c in sr.DefaultIfEmpty()
                          select new
                          {
                              a.Date,
                              SaleCount = b != null ? b.SaleCount : 0,
                              SaleTax = b != null ? b.SaleTax : 0,
                              TotalSale = b != null ? b.TotalSale : 0,
                              ReturnCount = c != null ? c.ReturnCount : 0,
                              RetSaleTax = c != null ? c.RetSaleTax : 0,
                              RetTotalSale = c != null ? c.RetTotalSale : 0,
                          }).ToList();

            var data = result.Skip(skip).Take(pageSize).ToList();
            recordsTotal = result.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string resultss = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = resultss,
                ContentType = "application/json"
            };
            return results;

        }


        [HttpGet]
        [QkAuthorize(Roles = "Dev,Purchase Day Wise")]
        public ActionResult ViewDayWise(string from, string to, long? ddmc, long? emp, long? supplier, string hfrom, string hto, long? htype, long? ptype)
        {
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            companySet();
            return View();
        }

        #endregion

        #region Cash or Credit Summary
        [QkAuthorize(Roles = "Dev,CashOrCredit purchaseSummary")]
        public ActionResult CashOrCredit()
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,CashOrCredit purchaseSummary")]
        public ActionResult CashOrCredit(string From, string To, long? ddlMC)
        {
            return RedirectToAction("ViewCashOrCredit", new { from = From, to = To, ddmc = ddlMC });
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,CashOrCredit purchaseSummary")]
        public ActionResult ViewCashOrCredit(string from, string to, long? ddmc)
        {
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            ViewBag.ddlmc = ddmc;
            companySet();
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,CashOrCredit purchaseSummary")]
        public ActionResult GetCashOrCredit(string From, string To, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddmc == 0 || ddmc == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime fdate = DateTime.Parse(From, new CultureInfo("en-GB"));
            DateTime tdate = DateTime.Parse(To, new CultureInfo("en-GB"));

            var count = 0;
            var dates = new List<DateTime>();
            for (var dt = fdate; dt <= tdate; dt = dt.AddDays(1))
            {
                count++;
                dates.Add(dt.Date);
            }

            var Purchase = (from a in db.PurchaseEntrys
                            where (From == "" || EF.Functions.DateDiffDay(a.PEDate, fdate) <= 0) && a.Status == 1 &&
                                  (To == "" || EF.Functions.DateDiffDay(a.PEDate, tdate) >= 0)
                                  && (!MCList.Any() || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                            select new
                            {
                                Date = a.PEDate,
                                id = a.PurchaseEntryId,
                                tax = (decimal?)a.PETaxAmount,
                                total = (decimal?)a.PEGrandTotal,
                                type = (SupplierType?)a.SupplierType
                            }).GroupBy(x => new { x.Date }, (y, group) => new
                            {
                                SaleCount = (decimal?)group.Select(k => k.id).Count(),
                                Date = y.Date,
                                total = (decimal?)group.Sum(k => k.total),
                                cashtotal = (decimal?)group.Where(a => a.type == SupplierType.CashSale).Sum(k => k.total),
                                credittotal = (decimal?)group.Where(a => a.type == SupplierType.CreditSale).Sum(k => k.total)
                            }).ToList();

            var data = Purchase.Skip(skip).Take(pageSize).ToList();
            recordsTotal = Purchase.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string resultss = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = resultss,
                ContentType = "application/json"
            };
            return results;
        }
        #endregion

        #region Invoice itemwise
        [QkAuthorize(Roles = "Dev,Invoice Supplier ItemWise")]
        public ActionResult InvoiceItemWise()
        {
            var select = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);
            ViewBag.Item = select;
            ViewBag.Supplier = select;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            ViewBag.getProj = QkSelect.List(
                new List<SelectListItem>
                {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                }, "Value", "Text", 0);

            ViewBag.getProTask = QkSelect.List(
                                  new List<SelectListItem>
                                  {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                  }, "Value", "Text", 0);

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            return View();
        }
        [QkAuthorize(Roles = "Dev,Invoice Supplier ItemWise")]
        public ActionResult InvoiceWise(long? ddlItem, long? ddlSupplier, string from, string to, long? ddlMC, long? ddlProject, long? ddlProTask)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            if (ddlItem != 0)
            {
                ViewBag.item = (from a in db.Items
                                where a.ItemID == ddlItem
                                select new
                                {
                                    ItemName = a.ItemCode + "-" + a.ItemName
                                }).FirstOrDefault().ItemName;
            }
            else
            {
                ViewBag.item = "All";
            }
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            ViewBag.supplier = ddlSupplier;
            ViewBag.ddlmc = ddlMC;
            companySet();
            return View();
        }

        [QkAuthorize(Roles = "Dev,Invoice Supplier ItemWise")]
        public ActionResult getInvoiceWise(long? item, long? supplier, string fromdate, string to, long? ddmc, long? project, long? task)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddmc == 0 || ddmc == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (to != "")
            {
                tdate = DateTime.Parse(to, new CultureInfo("en-GB"));
            }

            var v = (from a in db.PEItemss
                     join b in db.Items on a.Item equals b.ItemID
                     join g in db.PurchaseEntrys on a.PurchaseEntry equals g.PurchaseEntryId
                     join c in db.Suppliers on g.Supplier equals c.SupplierID
                     join e in db.ItemUnits on b.ItemUnitID equals e.ItemUnitID into primary
                     from e in primary.DefaultIfEmpty()
                     join f in db.ItemUnits on b.SubUnitId equals f.ItemUnitID into second
                     from f in second.DefaultIfEmpty()
                     where (item == 0 || a.Item == item) && (supplier == 0 || g.Supplier == supplier)
                     && (fromdate == "" || EF.Functions.DateDiffDay(g.PEDate, fdate) <= 0)
                     && (to == "" || EF.Functions.DateDiffDay(g.PEDate, tdate) >= 0)
                     && (!MCList.Any() || MCArray.Contains(g.MaterialCenter) || ddmc == g.MaterialCenter)
                     && (project == 0 || project == null || a.ProjectId == project)
                     && (task == 0 || task == null || a.TaskId == task)
                     select new
                     {
                         b.ItemID,
                         b.ItemName,
                         b.ItemCode,
                         Item = b.ItemCode + "-" + b.ItemName,
                         Unit = (a.ItemUnit == b.ItemUnitID) ? e.ItemUnitName : f.ItemUnitName,
                         g.BillNo,
                         a.ItemQuantity,
                         a.ItemSubTotal,
                         TaxAmount = a.ItemTaxAmount,
                         TotalAmount = a.ItemTotalAmount,
                         Discount = a.ItemDiscount,
                         a.itemNote,
                         a.ItemUnitPrice,
                         PartNumber = (b.PartNumber != null && b.PartNumber != "") ? b.PartNumber : "",
                         PriUnit = e.ItemUnitName,
                         SubUnit = f.ItemUnitName,
                         ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                         b.ItemUnitID,
                         b.SubUnitId,
                         b.SellingPrice,
                         Supplier = c.SupplierName,
                         Date = g.PEDate
                     }).AsEnumerable().OrderBy(a => a.ItemName);

            var data = v.Skip(skip).Take(pageSize).ToList();
            recordsTotal = v.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }
        #endregion


        #region SuppllierItemWiseSummary
        [HttpGet]
        [QkAuthorize(Roles = "Dev,SuppllierItemWiseSummary")]
        public ActionResult SuppllierItemWiseSummary()
        {

            ViewBag.Suppllier = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);

            var OptAll = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                            }, "Value", "Text", 0);
            ViewBag.Item = OptAll;

            ViewBag.MC = QkSelect.List(
                             new List<SelectListItem>
                             {
                                                        new SelectListItem { Selected = true, Text = "All", Value = "0"},
                             }, "Value", "Text", 0);

            companySet();
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,SuppllierItemWiseSummary")]
        public ActionResult GetSuppllierItemWise(long? Supp, long? Item, string fromdate, string todate, long? MC)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (MC == 0 || MC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }


            DateTime fdates = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            DateTime tdates = DateTime.Parse(todate, new CultureInfo("en-GB"));

            var count = 0;
            var dates = new List<DateTime>();
            for (var dt = fdates; dt <= tdates; dt = dt.AddDays(1))
            {
                count++;
                dates.Add(dt.Date);
            }

            var purchase = (from a in db.PEItemss
                            join b in db.PurchaseEntrys on a.PurchaseEntry equals b.PurchaseEntryId
                            join c in db.Suppliers on b.Supplier equals c.SupplierID into supp
                            from c in supp.DefaultIfEmpty()
                            join d in db.Items on a.Item equals d.ItemID
                            join e in db.ItemUnits on d.ItemUnitID equals e.ItemUnitID into primary
                            from e in primary.DefaultIfEmpty()
                            join f in db.ItemUnits on d.SubUnitId equals f.ItemUnitID into second
                            from f in second.DefaultIfEmpty()
                            where (Item == 0 || Item == null || a.Item == Item) && (Supp == 0 || Supp == null || b.Supplier == Supp)
                            && (fromdate == "" || EF.Functions.DateDiffDay(b.PEDate, fdate) <= 0) && b.Status == 1
                            && (todate == "" || EF.Functions.DateDiffDay(b.PEDate, tdate) >= 0)
                            && ((!MCList.Any() && MC == null) || MCArray.Contains(b.MaterialCenter) || MC == b.MaterialCenter)
                            && b.PurType == PurchaseHireType.Purchase
                            select new
                            {
                                Date = b.PEDate,
                                c.SupplierCode,
                                c.SupplierName,
                                b.PurchaseEntryId,
                                d.ItemCode,
                                ConFactor = d.ConFactor != 0 ? d.ConFactor : 1,

                                d.ItemUnitID,
                                d.SubUnitId,
                                PriUnit = e.ItemUnitName,
                                SubUnit = f.ItemUnitName,

                                PQty = (decimal?)(from i in db.PEItemss
                                                  join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                                  where (fromdate == "" || EF.Functions.DateDiffDay(j.PEDate, fdate) <= 0) &&
                                                  (todate == "" || EF.Functions.DateDiffDay(j.PEDate, tdate) >= 0) &&
                                                  (i.Item == d.ItemID && i.ItemUnit == d.ItemUnitID) && (Supp == 0 || Supp == null || j.Supplier == Supp)
                                                   && ((!MCList.Any() && MC == null) || MCArray.Contains(j.MaterialCenter) || MC == j.MaterialCenter)
                                                   && j.PurType == PurchaseHireType.Purchase && j.PurchaseEntryId == b.PurchaseEntryId
                                                  group i by i.ItemId into g
                                                  select new
                                                  {
                                                      Total = g.Sum(x => x.ItemQuantity)
                                                  }).FirstOrDefault().Total ?? 0,

                                SQty = (decimal?)(from i in db.PEItemss
                                                  join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                                  where (fromdate == "" || EF.Functions.DateDiffDay(j.PEDate, fdate) <= 0) &&
                                                  (todate == "" || EF.Functions.DateDiffDay(j.PEDate, tdate) >= 0) &&
                                                  (i.Item == d.ItemID && i.ItemUnit == d.SubUnitId) &&
                                                  e.ItemUnitName != f.ItemUnitName && (Supp == 0 || Supp == null || j.Supplier == Supp)
                                                  && ((!MCList.Any() && MC == null) || MCArray.Contains(j.MaterialCenter) || MC == j.MaterialCenter)
                                                  && j.PurType == PurchaseHireType.Purchase && j.PurchaseEntryId == b.PurchaseEntryId
                                                  group i by i.Item into g
                                                  select new
                                                  {
                                                      Total = g.Sum(x => x.ItemQuantity)
                                                  }).FirstOrDefault().Total ?? 0,


                                SValue = (decimal?)(from i in db.PEItemss
                                                    join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                                    where (i.Item == d.ItemID) && j.PurchaseEntryId == b.PurchaseEntryId
                                                    group i by i.ItemId into g
                                                    select new
                                                    {
                                                        Total = g.Sum(x => x.ItemSubTotal - x.ItemDiscount)
                                                    }).FirstOrDefault().Total ?? 0,

                            }).AsEnumerable().Select(o => new
                            {
                                ItemCode = (o.ItemCode != null) ? o.ItemCode : "",
                                Date = o.Date,
                                SupplierCode = o.SupplierCode,
                                SupplierName = o.SupplierName,
                                o.ConFactor,
                                o.ItemUnitID,
                                o.SubUnitId,
                                PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                                SubUnit = (o.SubUnit != null) ? o.SubUnit : "",

                                SValue = o.SValue,
                                PQty = (o.PQty + (int)(o.SQty / o.ConFactor)),
                                SQty = (o.SQty % o.ConFactor),
                                PRQty = (decimal)0,
                                SRQty = (decimal)0,
                                SRValue = (decimal)0,

                            }).ToList();


            var preturn = (from a in db.PRItemss
                           join b in db.PurchaseReturns on a.PurchaseReturnId equals b.PurchaseReturnId
                           join c in db.Suppliers on b.Supplier equals c.SupplierID into supp
                           from c in supp.DefaultIfEmpty()
                           join d in db.Items on a.Item equals d.ItemID
                           join e in db.ItemUnits on d.ItemUnitID equals e.ItemUnitID into primary
                           from e in primary.DefaultIfEmpty()
                           join f in db.ItemUnits on d.SubUnitId equals f.ItemUnitID into second
                           from f in second.DefaultIfEmpty()
                           where (Item == 0 || Item == null || a.Item == Item) && (Supp == 0 || Supp == null || b.Supplier == Supp)
                            && (fromdate == "" || EF.Functions.DateDiffDay(b.PRDate, fdate) <= 0) && b.Status == 1
                            && (todate == "" || EF.Functions.DateDiffDay(b.PRDate, tdate) >= 0)
                            && ((!MCList.Any() && MC == null) || MCArray.Contains(b.MaterialCenter) || MC == b.MaterialCenter)
                            && b.PurType == PurchaseHireType.Purchase
                           select new
                           {
                               Date = b.PRDate,
                               SupplierCode = c.SupplierCode,
                               SupplierName = c.SupplierName,
                               a.PurchaseReturnId,
                               d.ItemCode,
                               ConFactor = d.ConFactor != 0 ? d.ConFactor : 1,
                               d.ItemUnitID,
                               d.SubUnitId,
                               PriUnit = e.ItemUnitName,
                               SubUnit = f.ItemUnitName,

                               PQty = (decimal?)(from i in db.PRItemss
                                                 join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                                 where (fromdate == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                                 (todate == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                                 (i.Item == d.ItemID && i.ItemUnit == d.ItemUnitID) && (Supp == 0 || Supp == null || b.Supplier == Supp)
                                                 && ((!MCList.Any() && MC == null) || MCArray.Contains(j.MaterialCenter) || MC == j.MaterialCenter)
                                                 && b.PurType == PurchaseHireType.Purchase && j.PurchaseReturnId == b.PurchaseReturnId
                                                 group i by i.Item into g
                                                 select new
                                                 {
                                                     Total = g.Sum(x => x.ItemQuantity)
                                                 }).FirstOrDefault().Total ?? 0,

                               SQty = (decimal?)(from i in db.PRItemss
                                                 join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                                 where (fromdate == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                                 (todate == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                                 (i.Item == d.ItemID && i.ItemUnit == d.SubUnitId) &&
                                                 e.ItemUnitName != f.ItemUnitName && (Supp == 0 || Supp == null || j.Supplier == Supp)
                                                 && ((!MCList.Any() && MC == null) || MCArray.Contains(j.MaterialCenter) || MC == j.MaterialCenter)
                                                 && j.PurType == PurchaseHireType.Purchase && j.PurchaseReturnId == b.PurchaseReturnId
                                                 group i by i.Item into g
                                                 select new
                                                 {
                                                     Total = g.Sum(x => x.ItemQuantity)
                                                 }).FirstOrDefault().Total ?? 0,

                               SValue = (decimal?)(from i in db.PRItemss
                                                   join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                                   join k in db.Items on i.Item equals k.ItemID
                                                   where (i.Item == d.ItemID) && i.PurchaseReturnId == a.PurchaseReturnId
                                                   group i by i.Item into g
                                                   select new
                                                   {
                                                       Total = g.Sum(x => x.ItemSubTotal - x.ItemDiscount)
                                                   }).FirstOrDefault().Total ?? 0,


                           }).AsEnumerable().Select(o => new
                           {
                               ItemCode = (o.ItemCode != null) ? o.ItemCode : "",
                               Date = o.Date,
                               SupplierCode = o.SupplierCode,
                               SupplierName = o.SupplierName,
                               o.ConFactor,
                               o.ItemUnitID,
                               o.SubUnitId,
                               PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                               SubUnit = (o.SubUnit != null) ? o.SubUnit : "",

                               SValue = (decimal)0,
                               PQty = (decimal)0,
                               SQty = (decimal)0,
                               PRQty = (o.PQty + (int)(o.SQty / o.ConFactor)),
                               SRQty = (o.SQty % o.ConFactor),
                               SRValue = o.SValue,
                           }).ToList();



            var sjoin = (from a in dates
                         join b in purchase on a.Date equals b.Date into trans
                         from b in trans.DefaultIfEmpty()
                         select new
                         {
                             Date = a.Date,
                             ItemCode = (b != null) ? b.ItemCode : "",
                             SupplierCode = (b != null) ? b.SupplierCode : "",
                             SupplierName = (b != null) ? b.SupplierName : "",
                             ItemUnitID = (b != null) ? b.ItemUnitID : null,
                             SubUnitId = (b != null) ? b.SubUnitId : null,
                             PriUnit = (b != null) ? b.PriUnit : "",
                             SubUnit = (b != null) ? b.SubUnit : "",
                             ConFactor = (b != null) ? b.ConFactor : 1,

                             SValue = (b != null) ? b.SValue : 0,
                             PQty = (b != null) ? (b.PQty + (int)(b.SQty / b.ConFactor)) : 0,
                             SQty = (b != null) ? (b.SQty % b.ConFactor) : 0,
                             PRQty = (decimal)0,
                             SRQty = (decimal)0,
                             SRValue = (decimal)0,
                         }).ToList().GroupBy(x => new { Date = x.Date, ItemCode = x.ItemCode, SupplierCode = x.SupplierCode }, (key, group) => new
                         {
                             ItemCode = key.ItemCode,
                             Date = key.Date,
                             SupplierCode = key.SupplierCode,
                             SupplierName = group.Select(y => y.SupplierName).FirstOrDefault(),

                             ConFactor = group.Select(y => y.ConFactor).FirstOrDefault(),

                             ItemUnitID = (long?)group.Select(y => y.ItemUnitID).FirstOrDefault(),
                             SubUnitId = (long?)group.Select(y => y.SubUnitId).FirstOrDefault(),
                             PriUnit = group.Select(y => y.PriUnit).FirstOrDefault(),
                             SubUnit = group.Select(y => y.SubUnit).FirstOrDefault(),

                             SValue = group.Sum(k => k.SValue),
                             PQty = group.Sum(k => k.PQty),
                             SQty = group.Sum(k => k.SQty),
                             PRQty = (decimal)0,
                             SRQty = (decimal)0,
                             SRValue = (decimal)0,
                         }).ToList();


            var rjoin = (from a in dates
                         join b in preturn on a.Date equals b.Date into trans
                         from b in trans.DefaultIfEmpty()
                         select new
                         {
                             ItemCode = (b != null) ? b.ItemCode : "",
                             Date = a.Date,
                             SupplierCode = (b != null) ? b.SupplierCode : "",
                             SupplierName = (b != null) ? b.SupplierName : "",
                             ItemUnitID = (b != null) ? b.ItemUnitID : null,
                             SubUnitId = (b != null) ? b.SubUnitId : null,
                             PriUnit = (b != null) ? b.PriUnit : "",
                             SubUnit = (b != null) ? b.SubUnit : "",
                             ConFactor = (b != null) ? b.ConFactor : 1,

                             SValue = (decimal)0,
                             PQty = (decimal)0,
                             SQty = (decimal)0,
                             PRQty = (b != null) ? (b.PRQty + (int)(b.SRQty / b.ConFactor)) : 0,
                             SRQty = (b != null) ? (b.SRQty % b.ConFactor) : 0,
                             SRValue = (b != null) ? b.SRValue : 0,
                         }).ToList().GroupBy(x => new { Date = x.Date, ItemCode = x.ItemCode, SupplierCode = x.SupplierCode }, (key, group) => new
                         {
                             ItemCode = key.ItemCode,
                             Date = key.Date,
                             SupplierCode = key.SupplierCode,
                             SupplierName = group.Select(y => y.SupplierName).FirstOrDefault(),

                             ConFactor = group.Select(y => y.ConFactor).FirstOrDefault(),

                             ItemUnitID = (long?)group.Select(y => y.ItemUnitID).FirstOrDefault(),
                             SubUnitId = (long?)group.Select(y => y.SubUnitId).FirstOrDefault(),
                             PriUnit = group.Select(y => y.PriUnit).FirstOrDefault(),
                             SubUnit = group.Select(y => y.SubUnit).FirstOrDefault(),

                             SValue = (decimal)0,
                             PQty = (decimal)0,
                             SQty = (decimal)0,
                             PRQty = group.Sum(k => k.PRQty),
                             SRQty = group.Sum(k => k.SRQty),
                             SRValue = group.Sum(k => k.SRValue),

                         }).ToList();

            var full = sjoin.Union(rjoin);

            var common = (from a in full
                     select new
                     {
                         ItemCode = a.ItemCode,
                         Date = a.Date,
                         SupplierCode = a.SupplierCode,
                         SupplierName = a.SupplierName,

                         ConFactor = a.ConFactor,

                         ItemUnitID = a.ItemUnitID,
                         SubUnitId = a.SubUnitId,
                         PriUnit = a.PriUnit,
                         SubUnit = a.SubUnit,

                         SValue = a.SValue,
                         PQty = a.PQty,
                         SQty = a.SQty,
                         PRQty = a.PRQty,
                         SRQty = a.SRQty,
                         SRValue = a.SRValue,

                     }).ToList()
                          .GroupBy(x => new { Date = x.Date, ItemCode = x.ItemCode, SupplierCode = x.SupplierCode }, (key, group) => new
                          {
                              ItemCode = key.ItemCode,
                              Date = key.Date,
                              SupplierCode = key.SupplierCode,
                              SupplierName = group.Select(y => y.SupplierName).FirstOrDefault(),

                              ConFactor = group.Select(y => y.ConFactor).FirstOrDefault(),

                              ItemUnitID = (long?)group.Select(y => y.ItemUnitID).FirstOrDefault(),
                              SubUnitId = (long?)group.Select(y => y.SubUnitId).FirstOrDefault(),
                              PriUnit = group.Select(y => y.PriUnit).FirstOrDefault(),
                              SubUnit = group.Select(y => y.SubUnit).FirstOrDefault(),

                              SValue = group.Sum(k => k.SValue),
                              PQty = group.Sum(k => k.PQty),
                              SQty = group.Sum(k => k.SQty),
                              PRQty = group.Sum(k => k.PRQty),
                              SRQty = group.Sum(k => k.SRQty),
                              SRValue = group.Sum(k => k.SRValue),

                          }).ToList();

            var v = (from a in common
                     join b in sjoin on new { h1 = a.Date, h2 = a.ItemCode, h3 = a.SupplierCode }
                     equals new { h1 = b.Date, h2 = b.ItemCode, h3 = b.SupplierCode } into sa
                     from b in sa.DefaultIfEmpty()
                     join c in rjoin on new { h1 = a.Date, h2 = a.ItemCode, h3 = a.SupplierCode }
                     equals new { h1 = c.Date, h2 = c.ItemCode, h3 = c.SupplierCode } into ret
                     from c in ret.DefaultIfEmpty()
                     select new
                     {
                         a.ItemCode,
                         a.Date,
                         a.SupplierCode,
                         a.SupplierName,

                         a.ItemUnitID,
                         a.SubUnitId,
                         a.PriUnit,
                         a.SubUnit,
                         a.ConFactor,
                         SValue = b != null ? b.SValue : 0,
                         PQty = b != null ? (b.PQty + (int)(b.SQty / b.ConFactor)) : 0,
                         SQty = b != null ? (b.SQty % b.ConFactor) : 0,
                         PRQty = c != null ? (c.PRQty + (int)(c.SRQty / c.ConFactor)) : 0,
                         SRQty = c != null ? (c.SRQty % c.ConFactor) : 0,
                         SRValue = c != null ? c.SRValue : 0,
                     }).Where(x => x.SupplierName != "").ToList();

            var data = v.Skip(skip).Take(pageSize).ToList();
            recordsTotal = v.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

        }

        #endregion
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
