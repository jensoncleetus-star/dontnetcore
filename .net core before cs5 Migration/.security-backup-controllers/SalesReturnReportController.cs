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
    public class SalesReturnReportController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public SalesReturnReportController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: SalesReturn
        [QkAuthorize(Roles = "Dev,All Sales")]
        public ActionResult Index(string seno, long? paymethod, long? customer, long? SalesExecutive, string from, string to, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate)
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
            ViewBag.Customer = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);

            var pay = db.PaymentMethods.Select(s => new
            {
                ID = s.PaymentMethodId,
                Name = s.MethodName
            }).ToList();
            if (seno != null)
            {
                ViewBag.InvoiceNo = seno;
            }
            else
            {
                ViewBag.InvoiceNo = "All";
            }
            ViewBag.fromdate = from;
            ViewBag.todate = to;
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
       
        
        
        
         public ActionResult GetAllSaleprofit(string seno, long? paymethod, long? customer, long? SalesExecutive, string fromdate, string todate, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate, long? project, long? task,string srchtxt)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (ddMC == null)
                ddMC = 0;
            if (!MCList.Any() && (ddMC == 0 || ddMC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            var MCArray = MCList.ToArray();

            int recordsTotal = 0;

            SaleType St = new SaleType();
            // Port/forward fix (audit batch 11): the "All" sale-type option posts satype="" (empty
            // string, NOT null). The old `satype != ""` guard left St at its enum default POS(0), and the
            // WHERE's `satype == null` guard never caught the empty string -> the report filtered to POS
            // only and returned 0 rows. Treat empty-string as "no filter" (== the All intent).
            if (!string.IsNullOrEmpty(satype))
            {
                St = (satype == "1") ? SaleType.Sale : SaleType.Hire;
            };

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

            DateTime? hfrmdate = null;
            DateTime? htodate = null;
            if (!string.IsNullOrEmpty(hfdate))   // audit batch 11: view posts "" (not null) for an empty hire-date picker; '' != null was true -> DateTime.Parse("") threw
            {
                hfrmdate = DateTime.Parse(hfdate, new CultureInfo("en-GB"));
            }
            if (!string.IsNullOrEmpty(htdate))
            {
                htodate = DateTime.Parse(htdate, new CultureInfo("en-GB"));
            }

            SalesEntry sEntry = new SalesEntry();
            sEntry.CustomerType = (type == 1) ? CustomerType.Walking : (type == 0) ? CustomerType.Customer : CustomerType.Card;
            Int64 sac = 1;
            paymethod = paymethod == 0 ? null : paymethod;
                var v = (from a in db.SalesReturns
                         join b in db.Customers on a.Customer equals b.CustomerID into cust
                         from b in cust.DefaultIfEmpty()
                         join c in db.SRPayments on a.SalesReturnId equals c.SalesReturnId into srp
                         from c in srp.DefaultIfEmpty()
                         join d in db.Employees on a.SRCashier equals d.EmployeeId into emp
                         from d in emp.DefaultIfEmpty()
                         join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                         from g in mcs.DefaultIfEmpty()
                         join h in db.HireDetails on new { h1 = a.SalesReturnId, h2 = "Sales" }
                         equals new { h1 = h.Reference, h2 = h.Section } into hir
                         from h in hir.DefaultIfEmpty()
                         join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                         from i in acc.DefaultIfEmpty()
                         join j in db.AccountsTransactions on new { j1 = a.SalesReturnId, j2 = "Sale Return", j3 = sac }
                         equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                         from j in hir1.DefaultIfEmpty()
                         join k in db.SRItemss on a.SalesReturnId equals k.SalesReturnId
                         join l in db.Items on k.Item equals l.ItemID
                         let grandtotalitmsearch = (decimal)(from m in db.SRItemss
                                                             join nn in db.Items on m.Item equals nn.ItemID
                                                             join oo in db.SalesReturns on m.SalesReturnId equals oo.SalesReturnId
                                                             where nn.ItemName.Contains(srchtxt)
                                                             && oo.SalesReturnId == a.SalesReturnId

                                                             select new
                                                             {
                                                                 totalprice = m.ItemUnitPrice * m.ItemQuantity
                                                             }).Sum(o => o.totalprice)

                         where (srchtxt==""|| l.ItemName.Contains(srchtxt))  &&
                        (customer == 0 || a.Customer == customer) &&
                        (SalesExecutive == 0 || SalesExecutive == null || a.SRCashier == SalesExecutive) &&
                        (type == null || a.CustomerType == sEntry.CustomerType) &&

                         (fromdate == "" || EF.Functions.DateDiffDay(a.SRDate, fdate) <= 0) &&
                         (todate == "" || EF.Functions.DateDiffDay(a.SRDate, tdate) >= 0) &&
                         (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                        (htype == null || htype == null || h.HireType == htype) &&
                        (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                        (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)
                        && (ddMC==null||ddMC==0 || ddMC == a.MaterialCenter) &&

                        (project == 0 || project == null || a.Project == project) &&
                        (task == 0 || task == null || a.ProTask == task)
                         select new
                         {

                             SalesEntryId = a.SalesReturnId,
                             SENo = a.SRNo,
                             a.BillNo,
                             SEDate = a.SRDate,
                            // SEGrandTotal = (srchtxt == "") ? a.SRGrandTotal : 0,
                            //SETaxAmount = (srchtxt == "") ? a.SRTaxAmount : 0,
                             SEGrandTotal = (srchtxt == "") ? a.SRGrandTotal : grandtotalitmsearch,
                             SETaxAmount = (srchtxt == "") ? a.SRTaxAmount : 0,
                             //Credit = (srchtxt == "") ? j.Credit : grandtotalitmsearch,
                             Credit = 0,//(srchtxt == "") ? j.Credit : 0,
                             Customer = b.CustomerName,
                             TaxRegNo = i.TRN,
                             EmpName = d.FirstName + " " + d.LastName,
                             MCName = g.MCName,
                             SEPaidAmount = (srchtxt == "") ? 0 : 0,
                             a.CustomerType,
                             SEBalanceAmount = (srchtxt == "") ? (a.SRGrandTotal - 0) : 0,
                             //for expense
                           
                             SECreatedDate = a.SRCreatedDate,
                             SaleType = a.SaleType,
                             FromDate = h.StartDate,
                             ToDate = h.EndDate,
                             HireType = h.HireType,
                             SalesStatus = 0

                         }).AsEnumerable().Select(o => new
                         {
                             o.SalesEntryId,
                             o.SENo,
                             o.Credit,
                             o.BillNo,
                             o.SEDate,
                             o.SEGrandTotal,
                             o.SETaxAmount,
                             o.Customer,
                             o.TaxRegNo,
                             o.EmpName,
                             o.MCName,
                             o.SEPaidAmount,
                             o.CustomerType,
                             o.SEBalanceAmount,
                             //Calling Function To Get Total Item Price for each Sales Entry
                             itemprice = GetTotalItemPriceSR(o.SalesEntryId, o.SEDate, srchtxt),

                             o.SECreatedDate,
                             // o.PayMethod,
                             o.SaleType,
                             o.FromDate,
                             o.ToDate,
                             o.HireType,
                             o.SalesStatus,
                         }).Distinct().OrderBy(a => a.SEDate).ThenBy(a => a.SECreatedDate);
            
            var data = v.ToList();
            recordsTotal = v.Count();


            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }
        [QkAuthorize(Roles = "Dev,All Sales")]
        public ActionResult SalesProfit(string seno, long? paymethod, long? customer, long? SalesExecutive, string fromdate, string todate, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate)
        {
            ViewBag.Customer = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);

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
            if (seno != null)
            {
                ViewBag.InvoiceNo = seno;
            }
            else
            {
                ViewBag.InvoiceNo = "All";
            }
            ViewBag.fromdate = fromdate;
            ViewBag.todate = todate;
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
        [QkAuthorize(Roles = "Dev,All Sales")]
        public ActionResult GetAllSale(string srchtxt, long? brand, long? category, long? item, string seno, long? paymethod, long? customer, long? SalesExecutive, long? SalesMan, string fromdate, string todate, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate, long? project, long? task)
        {

            Int64 temp = 502;
            Int64 sac = 1;
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddMC == 0 || ddMC == 1))
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

            SaleType St = new SaleType();
            // Port/forward fix (audit batch 11): the "All" sale-type option posts satype="" (empty
            // string, NOT null). The old `satype != ""` guard left St at its enum default POS(0), and the
            // WHERE's `satype == null` guard never caught the empty string -> the report filtered to POS
            // only and returned 0 rows. Treat empty-string as "no filter" (== the All intent).
            if (!string.IsNullOrEmpty(satype))
            {
                St = (satype == "1") ? SaleType.Sale : SaleType.Hire;
            };

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

            DateTime? hfrmdate = null;
            DateTime? htodate = null;
            if (!string.IsNullOrEmpty(hfdate))   // audit batch 11: view posts "" (not null) for an empty hire-date picker; '' != null was true -> DateTime.Parse("") threw
            {
                hfrmdate = DateTime.Parse(hfdate, new CultureInfo("en-GB"));
            }
            if (!string.IsNullOrEmpty(htdate))
            {
                htodate = DateTime.Parse(htdate, new CultureInfo("en-GB"));
            }

            SalesEntry sEntry = new SalesEntry();
            sEntry.CustomerType = (type == 1) ? CustomerType.Walking : (type == 0) ? CustomerType.Customer : CustomerType.Card;

            paymethod = paymethod == 0 ? null : paymethod;
            var v = (from a in db.SalesReturns
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join c in db.SRPayments on a.SalesReturnId equals c.SalesReturnId into sepa
                     from c in sepa.DefaultIfEmpty()
                     join d in db.Employees on a.SRCashier equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                    
                     join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                     from g in mcs.DefaultIfEmpty()
                     join h in db.HireDetails on new { h1 = a.SalesReturnId, h2 = "Sale Return" }
                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                     from h in hir.DefaultIfEmpty()
                     join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()
                     join j in db.AccountsTransactions on new { j1 = a.SalesReturnId, j2 = "Sale Return", j3 = temp }
                     equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                     from j in hir1.DefaultIfEmpty()
                     join k in db.SRItemss on a.SalesReturnId equals k.SalesReturnId into temp2
                     from k in temp2.DefaultIfEmpty()
                     join l in db.Items on k.Item equals l.ItemID into temp3
                     from l in temp3.DefaultIfEmpty()
                     let grandtotalitmsearch = (decimal)(from m in db.SRItemss
                                                         join nn in db.Items on m.Item equals nn.ItemID
                                                         join oo in db.SalesReturns on m.SalesReturnId equals oo.SalesReturnId
                                                         where nn.ItemName.Contains(srchtxt)
                                                         && oo.SalesReturnId == a.SalesReturnId

                                                         select new
                                                         {
                                                             totalprice = m.ItemUnitPrice * m.ItemQuantity
                                                         }).Sum(o => o.totalprice)
                     let taxamountt = (from ii in db.AccountsTransactions

                                       where (
                          ii.reference == a.SalesReturnId && ii.Purpose == "Sale Return"
                          && ii.Account == sac)
                                       select new
                                       {
                                           ii.Debit
                                       }).FirstOrDefault()
                     let tax = (from ii in db.AccountsTransactions

                                       where (
                          ii.reference == a.SalesReturnId && ii.Purpose == "Sale Return"
                          && ii.Account == temp)
                                       select new
                                       {
                                           ii.Debit
                                       }).FirstOrDefault()
                     let discount = (from k in db.SRBillSundrys
                                     where k.SalesReturnId == a.SalesReturnId &&
                                     k.BillSundry == 4
                                     select new
                                     {
                                         k.BsAmount
                                     }).FirstOrDefault().BsAmount
                     let roundoffmin = (
                                        from k in db.SRBillSundrys
                                        where k.SalesReturnId == a.SalesReturnId &&
                                       k.BillSundry == 2
                                        select new
                                        {
                                            k.BsAmount
                                        }).FirstOrDefault().BsAmount
                     let roundoffplus = (from k in db.SRBillSundrys
                                         where k.SalesReturnId == a.SalesReturnId &&
                                        k.BillSundry == 1
                                         select new
                                         {
                                             k.BsAmount
                                         }).FirstOrDefault().BsAmount


                     where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                   (customer == 0 || a.Customer == customer) &&

                    (SalesExecutive == 0 || SalesExecutive == null || a.SRCashier == SalesExecutive) &&
                    (SalesMan == 0 || SalesMan == null || b.SalesPerson == SalesMan) &&
                    (item == 0 || item == null || k.Item == item) &&
                    (brand == 0 || brand == null || l.ItemBrandID == brand) &&
                    (category == 0 || category == null || l.ItemCategoryID == category) &&
                    (srchtxt == "" || l.ItemName.Contains(srchtxt)) &&
                    (type == null || a.CustomerType == sEntry.CustomerType) &&
                    
                    (fromdate == "" || EF.Functions.DateDiffDay(a.SRDate, fdate) <= 0) &&
                    (todate == "" || EF.Functions.DateDiffDay(a.SRDate, tdate) >= 0) &&
                    (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                    (htype == null || htype == null || h.HireType == htype) &&
                    (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                    (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0) &&
                    ((!MCList.Any() && ddMC == null || ddMC == 0) || MCArray.Contains(a.MaterialCenter) || ddMC == a.MaterialCenter) &&
                    (project == 0 || project == null || a.Project == project) &&
                    (task == 0 || task == null || a.ProTask == task)
                     select new
                     {
                         i.AccountsID,
                         a.SalesReturnId,
                         a.SRNo,
                         a.BillNo,
                         a.SRDate,
                         a.SRSubTotal,
                         SEGrandTotal = (srchtxt == "") ? a.SRGrandTotal : (grandtotalitmsearch==null)?0: grandtotalitmsearch,
                         Customer = b.CustomerName,
                         b.CustomerID,
                         TaxRegNo = i.TRN,
                         EmpName = d.FirstName + " " + d.LastName,
                         MCName = g.MCName,
                         SEPaidAmount =0,
                         a.CustomerType,
                         SEBalanceAmount = 0,
                         a.SRCreatedDate,
                         SaleType = a.SaleType,
                         FromDate = h.StartDate,
                         ToDate = h.EndDate,
                         HireType = h.HireType,
                         Credit = (srchtxt == "") ? (j.Credit == null ? 0 : j.Credit) : 0,
                         ////taxableAmt = (a.SalesType != 3 && j.Account != 502) ? a.SESubTotal - a.SEDiscount : ((j.Credit / (decimal)5) * 100),
                         taxableAmt = (srchtxt == "") ? (Decimal?)taxamountt.Debit : null,
                         tax = (srchtxt == "") ? (Decimal?)tax.Debit : null,
                         discount,
                         roundoffplus,
                         roundoffmin
                     }).Distinct().OrderBy(a => a.SRDate).ThenBy(a => a.SRCreatedDate);

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

        //Function To Return Total Item Price for each Sales Entry
        public decimal GetTotalItemPrice(long? SalesEntryId, DateTime? SEDate, string srchtxt)
        {

            //Getting All Items In Sales Entry
            var ItemList = (from se in db.SEItemss
                            join seen in db.SalesEntrys on se.SalesEntry equals seen.SalesEntryId
                            join seit in db.Items on se.Item equals seit.ItemID
                            where se.SalesEntry == SalesEntryId && seit.KeepStock == true &&
                            (srchtxt == "" || seit.ItemName.Contains(srchtxt))
                            select new
                            {
                                SEDate = seen.SEDate,
                                DetailId = se.SEItemsId,
                                ItemId = se.Item,
                                seItemUnit = se.ItemUnit,
                                seItemQuantity = se.ItemQuantity,
                                seitItemUnitID = seit.ItemUnitID,
                                seitPurchasePrice = seit.PurchasePrice,
                                seitConFactor = seit.ConFactor
                            }).AsEnumerable().Select(o => new
                            {
                                o.SEDate,
                                o.DetailId,
                                o.ItemId,
                                o.seItemUnit,
                                o.seItemQuantity,
                                o.seitItemUnitID,
                                o.seitPurchasePrice,
                                o.seitConFactor,

                                //Calling Function To Get Item Purchase Price (If Exists Any With in SEDate)for each Item
                                NewPurchPrice = GetItemPurchasePrice(o.ItemId, o.SEDate)

                            }).Select(s => new
                            {
                                s.SEDate,
                                s.DetailId,
                                s.ItemId,
                                s.seItemUnit,
                                s.seItemQuantity,
                                s.seitItemUnitID,
                                s.seitPurchasePrice,
                                s.seitConFactor,

                                //Calculating ItemPrice * Quantity(If Secondary Unit Exists ==> Considering Conversion Factor)
                                ItemPrice = (s.seItemUnit == s.seitItemUnitID) ? ((s.NewPurchPrice == 0) ? (s.seitPurchasePrice * s.seItemQuantity) : (s.NewPurchPrice * s.seItemQuantity)) : (((s.NewPurchPrice == 0) ? (s.seitPurchasePrice * s.seItemQuantity) : (s.NewPurchPrice * s.seItemQuantity)) / s.seitConFactor)

                            }).ToList();

            var j = 0;
            decimal ItemPrice = 0;

            //Taking Sum of Item Price ==> Item Price Of Each Item
            for (j = 0; j < ItemList.Count; j++)
            {
                ItemPrice = Convert.ToDecimal(ItemPrice + ItemList[j].ItemPrice);
            }

            return ItemPrice;
        }
        public decimal GetTotalItemPriceSR(long? SalesEntryId, DateTime? SEDate, string srchtxt)
        {

            //Getting All Items In Sales Entry
            var ItemList = (from se in db.SRItemss
                            join seen in db.SalesReturns on se.SalesReturnId equals seen.SalesReturnId
                            join seit in db.Items on se.Item equals seit.ItemID
                            where se.SalesReturnId == SalesEntryId && seit.KeepStock == true &&
                            (srchtxt == "" || seit.ItemName.Contains(srchtxt))
                            select new
                            {
                                SEDate = seen.SRDate,
                                DetailId = se.SRItemsId,
                                ItemId = se.Item,
                                seItemUnit = se.ItemUnit,
                                seItemQuantity = se.ItemQuantity,
                                seitItemUnitID = seit.ItemUnitID,
                                seitPurchasePrice = seit.PurchasePrice,
                                seitConFactor = seit.ConFactor
                            }).AsEnumerable().Select(o => new
                            {
                                o.SEDate,
                                o.DetailId,
                                o.ItemId,
                                o.seItemUnit,
                                o.seItemQuantity,
                                o.seitItemUnitID,
                                o.seitPurchasePrice,
                                o.seitConFactor,

                                //Calling Function To Get Item Purchase Price (If Exists Any With in SEDate)for each Item
                                NewPurchPrice = GetItemPurchasePrice(o.ItemId, o.SEDate)

                            }).Select(s => new
                            {
                                s.SEDate,
                                s.DetailId,
                                s.ItemId,
                                s.seItemUnit,
                                s.seItemQuantity,
                                s.seitItemUnitID,
                                s.seitPurchasePrice,
                                s.seitConFactor,

                                //Calculating ItemPrice * Quantity(If Secondary Unit Exists ==> Considering Conversion Factor)
                                ItemPrice = (s.seItemUnit == s.seitItemUnitID) ? ((s.NewPurchPrice == 0) ? (s.seitPurchasePrice * s.seItemQuantity) : (s.NewPurchPrice * s.seItemQuantity)) : (((s.NewPurchPrice == 0) ? (s.seitPurchasePrice * s.seItemQuantity) : (s.NewPurchPrice * s.seItemQuantity)) / s.seitConFactor)

                            }).ToList();

            var j = 0;
            decimal ItemPrice = 0;

            //Taking Sum of Item Price ==> Item Price Of Each Item
            for (j = 0; j < ItemList.Count; j++)
            {
                ItemPrice = Convert.ToDecimal(ItemPrice + ItemList[j].ItemPrice);
            }

            return ItemPrice;
        }

        //Function To Get Item Purchase Price (If Any Exists With in SEDate)
        public decimal? GetItemPurchasePrice(long? ItemId, DateTime? SEDate)
        {
            var NewPurPrice = (from aa in db.PEItemss
                               join bb in db.PurchaseEntrys on aa.PurchaseEntry equals bb.PurchaseEntryId
                               where (aa.Item == ItemId && bb.PEDate <= SEDate)
                               orderby bb.PEDate descending
                               select aa.ItemUnitPrice).FirstOrDefault();

            if (NewPurPrice == null)
                return 0;
            else
                return NewPurPrice;
        }

        #region itemwise
        public ActionResult ItemWise()
        {
            var OptAll = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Item = OptAll;

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

            return View();
        }

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,SalesReturn Item Wise")]
        public ActionResult ViewItemWise(long? ddlItem, string From, string To, long? ddlMC)
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
                                    ItemName = a.ItemCode
                                }).FirstOrDefault().ItemName;
            }
            else
            {
                ViewBag.item = "All";                
            }
            ViewBag.itemid = ddlItem;
            ViewBag.fromdate = From;
            ViewBag.todate = To;
            ViewBag.ddlmc = ddlMC;
            companySet();
            return View();
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,SalesReturn Item Wise")]
        public ActionResult GetItemWise(long? item, string fromdate, string todate, long? ddlMC)
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
            var v = (from a in db.Items
                     join e in db.ItemUnits on a.ItemUnitID equals e.ItemUnitID into primary
                     from e in primary.DefaultIfEmpty()
                     join f in db.ItemUnits on a.SubUnitId equals f.ItemUnitID into second
                     from f in second.DefaultIfEmpty()
                     where (item == 0 || item == null || a.ItemID == item)

                     select new
                     {
                         a.ItemID,
                         a.ItemName,
                         Item = a.ItemCode + "-" + a.ItemName,
                         PriUnit = e.ItemUnitName,
                         SubUnit = f.ItemUnitName,
                         ConFactor = a.ConFactor != 0 ? a.ConFactor : 1,
                         a.ItemUnitID,
                         a.SubUnitId,
                         a.SellingPrice,
                         a.PartNumber,
                         SaleType = a.Branch,
                         PriRetQty = (decimal?)(from i in db.SRItemss
                                                join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                                where (fromdate == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                                (todate == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0) &&
                                                (i.Item == a.ItemID && i.ItemUnit == a.ItemUnitID)
                                                && ((!MCList.Any() && ddlMC == 0) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)

                                                group i by i.ItemId into g
                                                select new
                                                {
                                                    Total = g.Sum(x => x.ItemQuantity)
                                                }).FirstOrDefault().Total ?? 0,

                         SubRetQty = (decimal?)(from i in db.SRItemss
                                                join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                                where (fromdate == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                                (todate == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0) &&
                                                (i.Item == a.ItemID && i.ItemUnit == a.SubUnitId) &&
                                                 e.ItemUnitName != f.ItemUnitName
                                                && ((!MCList.Any() && ddlMC == 0) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)
                                                group i by i.ItemId into g
                                                select new
                                                {
                                                    Total = g.Sum(x => x.ItemQuantity)
                                                }).FirstOrDefault().Total ?? 0,                                                

                         RetunAmt = (decimal?)(from i in db.SRItemss
                                               join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                               where (fromdate == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                               (todate == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0) &&
                                               (i.Item == a.ItemID)
                                               && ((!MCList.Any() && ddlMC == 0) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)
                                               group i by i.ItemId into g
                                               select new
                                               {
                                                   Total = g.Sum(x => x.ItemSubTotal - x.ItemDiscount)
                                               }).FirstOrDefault().Total ?? 0,
                                               

                     }).Distinct().AsEnumerable().Select(o => new
                     {
                         o.ItemID,
                         o.Item,
                         o.ItemUnitID,
                         o.SubUnitId,
                         PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                         SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                         o.ConFactor,
                         PartNumber = (o.PartNumber != null && o.PartNumber != "") ? o.PartNumber : "",
                         

                         PriRetQty = (o.PriRetQty + (int)(o.SubRetQty / o.ConFactor)),
                         SubRetQty = (o.SubRetQty % o.ConFactor),

                         NetQty = ((o.PriRetQty * o.ConFactor) + (o.SubRetQty)),
                         
                         o.RetunAmt,
                         o.ItemName
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

        #region customer itemwise
        //[QkAuthorize(Roles = "Dev,SalesReturn Customer ItemWise")]
        public ActionResult CustomerItemWise()
        {
            ViewBag.Item = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                            }, "Value", "Text", 0);


            var cust = db.Customers.Where(s => s.Type == CRMCustomerType.Customer).Select(s => new
            {
                CustomerID = s.CustomerID,
                CustomerDetails = s.CustomerCode + " - " + s.CustomerName
            }).ToList();
            ViewBag.Customer = QkSelect.List(cust, "CustomerID", "CustomerDetails");
            companySet();
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

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

            return View();
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,SalesReturn Customer ItemWise")]
        public ActionResult GetCustomerItemWise(long? ddlCustomer, long? ddlItem, string From, string To, long? ddmc)
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
                     select new
                     {
                         a.ItemID,
                         a.ItemName,
                         Item = a.ItemCode + "-" + a.ItemName,
                         PriUnit = e.ItemUnitName,
                         SubUnit = f.ItemUnitName,
                         ConFactor = a.ConFactor != 0 ? a.ConFactor : 1,
                         a.ItemUnitID,
                         a.SubUnitId,
                         a.SellingPrice,                         

                         PriRetQty = (decimal?)(from i in db.SRItemss
                                                join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                                where (From == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                                (To == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0) &&
                                                (i.Item == a.ItemID && i.ItemUnit == a.ItemUnitID) && (j.Customer == ddlCustomer)
                                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                group i by i.ItemId into g
                                                select new
                                                {
                                                    Total = g.Sum(x => x.ItemQuantity)
                                                }).FirstOrDefault().Total ?? 0,

                         SubRetQty = (decimal?)(from i in db.SRItemss
                                                join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                                where (From == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                                (To == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0) &&
                                                (i.Item == a.ItemID && i.ItemUnit == a.SubUnitId) &&
                                                 e.ItemUnitName != f.ItemUnitName && (j.Customer == ddlCustomer)
                                                 && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                group i by i.ItemId into g
                                                select new
                                                {
                                                    Total = g.Sum(x => x.ItemQuantity)
                                                }).FirstOrDefault().Total ?? 0,
                                      

                         RetunAmt = (decimal?)(from i in db.SRItemss
                                               join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                               where (From == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                               (To == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0) &&
                                               (i.Item == a.ItemID) && (j.Customer == ddlCustomer)
                                               && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                               group i by i.ItemId into g
                                               select new
                                               {
                                                   Total = g.Sum(x => x.ItemSubTotal - x.ItemDiscount)
                                               }).FirstOrDefault().Total ?? 0,                                               

                         NoOfVchReturn = (int?)(from i in db.SRItemss
                                                join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                                where (From == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                                (To == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0) &&
                                                (i.Item == a.ItemID) && (j.Customer == ddlCustomer)
                                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                //  group i by i.ItemId into g
                                                select new
                                                {
                                                    saleid = i.SalesReturnId
                                                }).GroupBy(x => x.saleid).Count() ?? 0,

                     }).Distinct().AsEnumerable().Select(o => new
                     {
                         o.ItemID,
                         o.Item,
                         o.ItemUnitID,
                         o.SubUnitId,
                         PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                         SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                         o.ConFactor,

                         PriRetQty = (o.PriRetQty + (int)(o.SubRetQty / o.ConFactor)),
                         SubRetQty = (o.SubRetQty % o.ConFactor),

                         NetQty = ((o.PriRetQty * o.ConFactor) +  o.SubRetQty),
                         
                         o.RetunAmt,
                         o.NoOfVchReturn,
                         o.ItemName,
                     }).Where(a => a.NoOfVchReturn != 0).OrderBy(a => a.ItemName);

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

        #region Customer Wise
        //[QkAuthorize(Roles = "Dev,SalesReturn Customer Wise")]
        public ActionResult CustomerWise()
        {
            ViewBag.Customer = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 0);

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

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

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View();
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,SalesReturn Customer Wise")]
        public ActionResult CustomerWise(long? ddlCustomer, string From, string To, long? ddlMC, long? ddlProject, long? ddlProTask)
        {
            return RedirectToAction("ViewCustomerWise", new { cust = ddlCustomer, from = From, to = To, ddMC = ddlMC, project = ddlProject, task = ddlProTask });
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,SalesReturn Customer Wise")]
        public ActionResult GetCustomerWise(long? customer, string fromdate, string todate, long? ddMC, long? project, long? task)
        {

            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddMC == 0 || ddMC == 1))
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
            var v = (from a in db.Customers
                     join f in db.Accountss on a.Accounts equals f.AccountsID into acc
                     from f in acc.DefaultIfEmpty()
                     where (customer == 0 || a.CustomerID == customer)
                     select new
                     {
                         a.CustomerID,
                         customer = a.CustomerCode + "-" + a.CustomerName,
                         TRN = f.TRN,
                         RetunAmt = (decimal?)(from i in db.SalesReturns
                                               where (fromdate == "" || EF.Functions.DateDiffDay(i.SRDate, fdate) <= 0) &&
                                               (todate == "" || EF.Functions.DateDiffDay(i.SRDate, tdate) >= 0) &&
                                               (i.Customer == a.CustomerID)
                                                && ((!MCList.Any() && ddMC == null) || MCArray.Contains(i.MaterialCenter) || ddMC == i.MaterialCenter)
                                                && (project == 0 || project == null || i.Project == project)
                                                && (task == 0 || task == null || i.ProTask == task)
                                               group i by i.Customer into g
                                               select new
                                               {
                                                   Total = g.Sum(x => x.SRSubTotal - x.SRDiscount)
                                               }).FirstOrDefault().Total ?? 0,
                         RetuntaxAmt = (decimal?)(from i in db.SalesReturns
                                                  where (fromdate == "" || EF.Functions.DateDiffDay(i.SRDate, fdate) <= 0) &&
                                                  (todate == "" || EF.Functions.DateDiffDay(i.SRDate, tdate) >= 0) &&
                                                  (i.Customer == a.CustomerID)
                                                  && ((!MCList.Any() && ddMC == null) || MCArray.Contains(i.MaterialCenter) || ddMC == i.MaterialCenter)
                                                  && (project == 0 || project == null || i.Project == project)
                                                  && (task == 0 || task == null || i.ProTask == task)
                                                  group i by i.Customer into g
                                                  select new
                                                  {
                                                      Total = g.Sum(x => x.SRTaxAmount)
                                                  }).FirstOrDefault().Total ?? 0,
                         RetuntotAmt = (decimal?)(from i in db.SalesReturns
                                                  where (fromdate == "" || EF.Functions.DateDiffDay(i.SRDate, fdate) <= 0) &&
                                                  (todate == "" || EF.Functions.DateDiffDay(i.SRDate, tdate) >= 0) &&
                                                  (i.Customer == a.CustomerID)
                                                  && ((!MCList.Any() && ddMC == null) || MCArray.Contains(i.MaterialCenter) || ddMC == i.MaterialCenter)
                                                  && (project == 0 || project == null || i.Project == project)
                                                  && (task == 0 || task == null || i.ProTask == task)
                                                  group i by i.Customer into g
                                                  select new
                                                  {
                                                      Total = g.Sum(x => x.SRGrandTotal)
                                                  }).FirstOrDefault().Total ?? 0,
                                                  
                         NoOfVchReturn = (int?)(from j in db.SalesReturns
                                                where (fromdate == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                                (todate == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0) &&
                                                (j.Customer == a.CustomerID)
                                                 && ((!MCList.Any() && ddMC == null) || MCArray.Contains(j.MaterialCenter) || ddMC == j.MaterialCenter)
                                                 && (project == 0 || project == null || j.Project == project)
                                                 && (task == 0 || task == null || j.ProTask == task)
                                                select new
                                                {
                                                    saleid = j.SalesReturnId
                                                }).Count() ?? 0,
                     }).Where(b=>b.NoOfVchReturn!=0).OrderBy(b => b.customer);

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
        [HttpGet]
        //[QkAuthorize(Roles = "Dev,SalesReturn Customer Wise")]
        public ActionResult ViewCustomerWise(long? cust, string from, string to, string saletype, long? ddMC)
        {

            SaleType St = new SaleType();
            if (saletype != "" && saletype != null)
            {
                St = (saletype == "1") ? SaleType.Sale : SaleType.Hire;
            };
            if (cust != 0)
            {
                ViewBag.custName = (from a in db.Customers
                                    join b in db.SalesReturns on a.CustomerID equals b.Customer into cat
                                    from b in cat.DefaultIfEmpty()
                                    join f in db.Accountss on a.Accounts equals f.AccountsID into acc
                                    from f in acc.DefaultIfEmpty()
                                    where a.CustomerID == cust && (saletype == null || b.SaleType == St)
                                    select new
                                    {
                                        CustomerName = a.CustomerName + (f.TRN != null ? " ; TRN :" + f.TRN : "")
                                    }).FirstOrDefault().CustomerName;
            }
            else
            {
                ViewBag.custName = "All";
            }
            ViewBag.ddlmc = ddMC;
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            ViewBag.SaleTy = saletype;
            companySet();
            return View();
        }

        #endregion

        #region category wise
        //[QkAuthorize(Roles = "Dev,SalesReturn Category Wise")]
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
            
            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View();
        }
        //[QkAuthorize(Roles = "Dev,SalesReturn Category Wise")]
        public ActionResult CategoryWises(long? ddlItemCategory, string From, string To, long? ddlMC)
        {
            return RedirectToAction("ViewCategoryWise", new { category = ddlItemCategory, from = From, to = To, ddMC = ddlMC });
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,SalesReturn Category Wise")]
        public ActionResult GetCategoryWise(long? category, string fromdate, string todate, long? ddMC)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddMC == 0 || ddMC == 1))
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

                         RetunAmt = (decimal?)(from i in db.SRItemss
                                               join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                               join k in db.Items on i.Item equals k.ItemID into itm
                                               from k in itm.DefaultIfEmpty()
                                               where (fromdate == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                               (todate == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0) &&
                                               (k.ItemCategoryID == a.ItemCategoryID)
                                               && ((!MCList.Any() && ddMC == null) || MCArray.Contains(j.MaterialCenter) || ddMC == j.MaterialCenter)
                                               select new
                                               {
                                                   Total = i.ItemSubTotal - i.ItemDiscount
                                               }).Sum(x => x.Total) ?? 0,




                         NoOfVchReturn = (int?)(from i in db.SRItemss
                                                join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                                join k in db.Items on i.Item equals k.ItemID into itm
                                                from k in itm.DefaultIfEmpty()
                                                where (fromdate == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                                (todate == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0) &&
                                                (k.ItemCategoryID == a.ItemCategoryID)
                                                && ((!MCList.Any() && ddMC == null) || MCArray.Contains(j.MaterialCenter) || ddMC == j.MaterialCenter)
                                                select new
                                                {
                                                    saleid = i.SalesReturnId
                                                }).GroupBy(x => x.saleid).Count() ?? 0,

                     }).Distinct().AsEnumerable().Select(o => new
                     {
                         o.Category,
                         o.ItemCategoryID,
                         o.RetunAmt,
                         o.NoOfVchReturn
                     }).OrderBy(x => x.Category);

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

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,SalesReturn Category Wise")]
        public ActionResult ViewCategoryWise(long? category, string from, string to, long? ddMC)
        {
            if (category != 0)
            {
                ViewBag.category = (from a in db.ItemCategorys
                                    where a.ItemCategoryID == category
                                    select new
                                    {
                                        ItemCategory = a.ItemCategoryName
                                    }).FirstOrDefault().ItemCategory;
            }
            else
            {
                ViewBag.category = "All";
            }
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            companySet();
            return View();
        }

        #endregion

        #region brand wise
        //[QkAuthorize(Roles = "Dev,SalesReturn Brand Wise")]
        public ActionResult BrandWise()
        {
            ViewBag.Brand = QkSelect.List(
             new List<SelectListItem>
             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
             }, "Value", "Text", 1);
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

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

            return View();
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,SalesReturn Brand Wise")]
        public ActionResult BrandWise(long? ddlItemBrand, string From, string To, long? ddlMC)
        {
            return RedirectToAction("ViewBrandWise", new { brand = ddlItemBrand, from = From, to = To, ddmc = ddlMC });
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,SalesReturn Brand Wise")]
        public ActionResult GetBrandWise(long? brand, string fromdate, string todate, long? ddmc)
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
            var v = (from a in db.ItemBrands
                     where (brand == 0 || a.ItemBrandID == brand)
                     select new
                     {
                         Brand = a.ItemBrandName,
                         a.ItemBrandID,

                         RetunAmt = (decimal?)(from i in db.SRItemss
                                               join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                               join k in db.Items on i.Item equals k.ItemID into itm
                                               from k in itm.DefaultIfEmpty()
                                               where (fromdate == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                               (todate == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0) &&
                                                (k.ItemBrandID == a.ItemBrandID)
                                                 && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                               select new
                                               {
                                                   Total = i.ItemSubTotal - i.ItemDiscount
                                               }).Sum(x => x.Total) ?? 0,

                         NoOfVchReturn = (int?)(from i in db.SRItemss
                                                join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                                join k in db.Items on i.Item equals k.ItemID into itm
                                                from k in itm.DefaultIfEmpty()
                                                where (fromdate == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                                (todate == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0) &&
                                                (k.ItemBrandID == a.ItemBrandID)
                                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                //  group i by i.ItemId into g
                                                select new
                                                {
                                                    saleid = i.SalesReturnId
                                                }).GroupBy(x => x.saleid).Count() ?? 0,

                     }).Distinct().AsEnumerable().Select(o => new
                     {
                         o.Brand,
                         o.ItemBrandID,
                         o.RetunAmt,
                         o.NoOfVchReturn
                     }).OrderBy(x => x.Brand);

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
        [HttpGet]
        //[QkAuthorize(Roles = "Dev,SalesReturn Brand Wise")]
        public ActionResult ViewBrandWise(long? brand, string from, string to, long? ddmc)
        {
            if (brand != 0)
            {
                ViewBag.brand = (from a in db.ItemBrands
                                 where a.ItemBrandID == brand
                                 select new
                                 {
                                     BName = a.ItemBrandName
                                 }).FirstOrDefault().BName;
            }
            else
            {
                ViewBag.brand = "All";
            }
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            companySet();
            return View();
        }

        #endregion

        #region executive wise
        //[QkAuthorize(Roles = "Dev,SalesReturn Executive Wise")]
        public ActionResult ExecutiveWise()
        {
            ViewBag.SalesExec = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
            }, "Value", "Text", 0);
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

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

            return View();
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,SalesReturn Executive Wise")]
        public ActionResult SalesExecutiveWise(long? ddlEmployee, string From, string To, long? ddlMC)
        {
            return RedirectToAction("ViewSalesExecutiveWise", new { salesexec = ddlEmployee, from = From, to = To, ddmc = ddlMC });
        }
        //[QkAuthorize(Roles = "Dev,SalesReturn Executive Wise")]
        public ActionResult GetExecutiveWise(long? salesexec, string fromdate, string todate, long? ddmc)
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
                     where (salesexec == 0 || a.EmployeeId == salesexec)
                     select new
                     {
                         a.EmployeeId,
                         a.FirstName,
                         employee = a.FirstName + " " + a.MiddleName + " " + a.LastName,
                         
                         RetunAmt = (decimal?)(from i in db.SalesReturns
                                               where (fromdate == "" || EF.Functions.DateDiffDay(i.SRDate, fdate) <= 0) &&
                                               (todate == "" || EF.Functions.DateDiffDay(i.SRDate, tdate) >= 0) &&
                                               (i.SRCashier == a.EmployeeId)
                                               && ((!MCList.Any() && ddmc == null) || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                               group i by i.Customer into g
                                               select new
                                               {
                                                   Total = g.Sum(x => x.SRSubTotal - x.SRDiscount)
                                               }).FirstOrDefault().Total ?? 0,
                         RetuntaxAmt = (decimal?)(from i in db.SalesReturns
                                                  where (fromdate == "" || EF.Functions.DateDiffDay(i.SRDate, fdate) <= 0) &&
                                                  (todate == "" || EF.Functions.DateDiffDay(i.SRDate, tdate) >= 0) &&
                                                  (i.SRCashier == a.EmployeeId)
                                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                                  group i by i.Customer into g
                                                  select new
                                                  {
                                                      Total = g.Sum(x => x.SRTaxAmount)
                                                  }).FirstOrDefault().Total ?? 0,
                         RetuntotAmt = (decimal?)(from i in db.SalesReturns
                                                  where (fromdate == "" || EF.Functions.DateDiffDay(i.SRDate, fdate) <= 0) &&
                                                  (todate == "" || EF.Functions.DateDiffDay(i.SRDate, tdate) >= 0) &&
                                                  (i.SRCashier == a.EmployeeId)
                                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                                  group i by i.Customer into g
                                                  select new
                                                  {
                                                      Total = g.Sum(x => x.SRGrandTotal)
                                                  }).FirstOrDefault().Total ?? 0,

                         NoOfVchReturn = (int?)(from j in db.SalesReturns
                                                where (fromdate == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                                (todate == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0) &&
                                                (j.SRCashier == a.EmployeeId)
                                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                select new
                                                {
                                                    saleid = j.SalesReturnId
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

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,SalesReturn Executive Wise")]
        public ActionResult ViewSalesExecutiveWise(long? salesexec, string from, string to, long? ddmc)
        {
            if (salesexec != 0)
            {
                ViewBag.salesExec = (from a in db.Employees
                                     where a.EmployeeId == salesexec
                                     select new
                                     {
                                         EmpName = a.FirstName + " " + a.MiddleName + " " + a.LastName
                                     }).FirstOrDefault().EmpName;
            }
            else
            {
                ViewBag.salesExec = "All";
            }
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            companySet();
            return View();
        }
        #endregion

        #region Invoice item wise
        //[QkAuthorize(Roles = "Dev,SalesReturn Invoice ItemWise")]
        public ActionResult InvoiceItemWise()
        {
            var select = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                            }, "Value", "Text", 0);
            ViewBag.Customer = select;
            ViewBag.Item = select;

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
            return View();
        }
        //[QkAuthorize(Roles = "Dev,SalesReturn Invoice ItemWise")]
        public ActionResult InvoiceWise(long? ddlItem, long? ddlCustomer, string from, string to, long? ddlMC)
        {
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
            ViewBag.customer = ddlCustomer;
            companySet();
            return View();
        }
        //[QkAuthorize(Roles = "Dev,SalesReturn Invoice ItemWise")]
        public ActionResult getInvoiceWise(long? item, long? customer, string fromdate, string to, long? ddmc)
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

            var v = (from a in db.SRItemss
                     join b in db.Items on a.Item equals b.ItemID
                     join g in db.SalesReturns on a.SalesReturnId equals g.SalesReturnId
                     join c in db.Customers on g.Customer equals c.CustomerID
                     join e in db.ItemUnits on b.ItemUnitID equals e.ItemUnitID into primary
                     from e in primary.DefaultIfEmpty()
                     join f in db.ItemUnits on b.SubUnitId equals f.ItemUnitID into second
                     from f in second.DefaultIfEmpty()
                     where (item == 0 || a.Item == item) && (customer == 0 || g.Customer == customer)
                     && (fromdate == "" || EF.Functions.DateDiffDay(g.SRDate, fdate) <= 0)
                     && (to == "" || EF.Functions.DateDiffDay(g.SRDate, tdate) >= 0)
                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(g.MaterialCenter) || ddmc == g.MaterialCenter)
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
                         a.ItemTaxAmount,
                         a.ItemTotalAmount,
                         a.ItemDiscount,
                         a.itemNote,
                         a.ItemUnitPrice,
                         PriUnit = e.ItemUnitName,
                         SubUnit = f.ItemUnitName,
                         ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                         b.ItemUnitID,
                         b.SubUnitId,
                         b.SellingPrice,
                         Customer = c.CustomerName,
                         Date = g.SRDate
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
    }
}
