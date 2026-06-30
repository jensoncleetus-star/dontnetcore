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
using System.Data;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Mvc;
using CustomHtml;
using Microsoft.AspNetCore.Identity;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class SalesReportController : BaseController
    {

        ApplicationDbContext db;
        Common com;
        public SalesReportController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: SalesReport


        public ActionResult SalesProfitdepartment()
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
        public ActionResult SalesProfitdepartment(long?[] ddlEmployee, string InvoiceNo, long? ddlCustomer, string From, string To, long? ddlType, long? ddlMC, string SaleType, long? HireType, string FromDate, string ToDate, long? ddlProject, long? ddlProTask)
        {
            var stringemployee = String.Join(",", ddlEmployee);
            return RedirectToAction("ViewIndexprofitdepartment", new { seno = InvoiceNo, customer = ddlCustomer, SalesExecutive = stringemployee, from = From, to = To, type = ddlType, ddMC = ddlMC, satype = SaleType, htype = HireType, hfdate = FromDate, htdate = ToDate, project = ddlProject, task = ddlProTask });
        }

        public ActionResult ViewIndexprofitdepartment(string seno, long? paymethod, long? customer, string SalesExecutive, string from, string to, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate)
        {

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

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
            companySet();
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,All Sales")]


        public ActionResult GetAllSaleprofitdepartment(string seno, long? paymethod, long? customer, string SalesExecutive, string fromdate, string todate, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate, long? project, long? task)
        {
            long[] SalesExecutivear = new long[] { };

            if (!string.IsNullOrEmpty(SalesExecutive))
            {
                SalesExecutivear = SalesExecutive.Split(',').Select(x => long.Parse(x)).ToArray();
            }

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
            if (satype != "")
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
            if (!string.IsNullOrEmpty(hfdate))
            {
                hfrmdate = DateTime.Parse(hfdate, new CultureInfo("en-GB"));
            }
            if (!string.IsNullOrEmpty(htdate))
            {
                htodate = DateTime.Parse(htdate, new CultureInfo("en-GB"));
            }

            SalesEntry sEntry = new SalesEntry();
            if (type == 1)
            {
                sEntry.CustomerType = (type == 1) ? CustomerType.Walking : (type == 0) ? CustomerType.Customer : CustomerType.Card;
            }
            else if (type == 0)
            {
                sEntry.CustomerType = CustomerType.Customer;
            }

            paymethod = paymethod == 0 ? null : paymethod;
            var v = (from a in db.SalesEntrys
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                     join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                     join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                     from f in paymeth.DefaultIfEmpty()
                     join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                     from g in mcs.DefaultIfEmpty()
                     join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                     from h in hir.DefaultIfEmpty()
                     join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()
                     where
                    (SalesExecutive == null || SalesExecutivear.Contains((long)a.SECashier)) &&

                    (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                    (todate == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0)
                     select new
                     {
                         a.SalesEntryId,
                         a.SENo,
                         a.BillNo,
                         a.SEDate,
                         a.SEGrandTotal,
                         a.SETaxAmount,
                         Customer = b.CustomerName,
                         TaxRegNo = i.TRN,
                         EmpName = d.FirstName + " " + d.LastName,
                         MCName = g.MCName,
                         c.SEPaidAmount,
                         a.CustomerType,
                         SEBalanceAmount = a.SEGrandTotal - c.SEPaidAmount,
                         PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == a.BillNo).Select(x => x.Paying).Sum()),
                         salesreturns = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId).Select(x => x.SRSubTotal).Sum()),



                         JournalExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == a.BillNo).Select(y => y.Paying).Sum()),




                         itemprice = (decimal?)(from se in db.SEItemss
                                                join seen in db.SalesEntrys on se.SalesEntry equals seen.SalesEntryId
                                                join seit in db.Items on se.Item equals seit.ItemID
                                                where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
                                                select new
                                                {
                                                    salesprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.SellingPrice * se.ItemQuantity) : ((seit.SellingPrice * se.ItemQuantity) / seit.ConFactor)
                                                    //salesprice = (se.ItemUnit == seit.ItemUnitID) ? ((NewSalesPrice == 0) ? (seit.SellingPrice * se.ItemQuantity) : (NewSalesPrice * se.ItemQuantity)) : (((NewSalesPrice == 0) ? (seit.SellingPrice * se.ItemQuantity) : (NewSalesPrice * se.ItemQuantity)) / seit.ConFactor)
                                                }
                                    ).Sum(x => x.salesprice) ?? 0,


                         a.SECreatedDate,
                         PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                         SaleType = a.SaleType,
                         FromDate = h.StartDate,
                         ToDate = h.EndDate,
                         HireType = h.HireType,
                         a.Ref1
                     }).Select(o => new
                     {
                         o.SalesEntryId,
                         o.SENo,
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
                         NewExpense = (o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
                         ((o.PaymentExpense != null) ? o.PaymentExpense : o.JournalExpense),
                         salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
                         o.itemprice,
                         o.SECreatedDate,
                         o.PayMethod,
                         o.SaleType,
                         o.FromDate,
                         o.ToDate,
                         o.HireType,
                         o.Ref1
                     }).OrderBy(a => a.SEDate).ThenBy(a => a.SECreatedDate);

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



        [QkAuthorize(Roles = "Dev,Sales Profit")]
        public ActionResult SalesProfitnew(string seno, long? paymethod, long? customer, long? SalesExecutive, string fromdate, string todate, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate)
        {
            var lead = db.SourceOfLeads.Select(r => new
            {
                ID = r.SourceOfLeadId,
                Name = r.SrcName
            }).ToList();
            ViewBag.Source = QkSelect.List(lead, "ID", "Name");
            var loc = db.ProTasks
              .Select(s => new
              {
                  ID = s.Location,
                  Name = s.Location
              }).Distinct().OrderBy(o => o.Name).ToList();
            ViewBag.Local = QkSelect.List(loc, "ID", "Name");
            ViewBag.SalesMan = QkSelect.List(
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



        public ActionResult SalesProfitbytarget(string seno, long? paymethod, long? customer, long? SalesExecutive, string fromdate, string todate, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate)
        {
            var lead = db.SourceOfLeads.Select(r => new
            {
                ID = r.SourceOfLeadId,
                Name = r.SrcName
            }).ToList();
            ViewBag.Source = QkSelect.List(lead, "ID", "Name");
            var loc = db.ProTasks
              .Select(s => new
              {
                  ID = s.Location,
                  Name = s.Location
              }).Distinct().OrderBy(o => o.Name).ToList();
            ViewBag.Local = QkSelect.List(loc, "ID", "Name");

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
                                      new SelectListItem {Selected=true, Text = "All", Value = "0"},
                                  }, "Value", "Text", 0);
            var useid = User.Identity.GetUserId();
            SelectListItem curremployee = db.Employees.Where(o => o.UserId == useid).Select(o => new SelectListItem { Selected = false, Value = o.EmployeeId.ToString(), Text = o.FirstName + " " + o.LastName }).FirstOrDefault();
            ViewBag.SalesMan = QkSelect.List(
                                       new List<SelectListItem>
                                       { new SelectListItem {Selected=true,  Text = "All", Value = "0"},


                                       }, "Value", "Text", curremployee.Value);
            return View();
        }


        [QkAuthorize(Roles = "Dev,Sales Profit")]
        public ActionResult SalesProfitgroup(string seno, long? paymethod, long? customer, long? SalesExecutive, string fromdate, string todate, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate)
        {
            var lead = db.SourceOfLeads.Select(r => new
            {
                ID = r.SourceOfLeadId,
                Name = r.SrcName
            }).ToList();
            ViewBag.Source = QkSelect.List(lead, "ID", "Name");
            var loc = db.ProTasks
              .Select(s => new
              {
                  ID = s.Location,
                  Name = s.Location
              }).Distinct().OrderBy(o => o.Name).ToList();
            ViewBag.Local = QkSelect.List(loc, "ID", "Name");
            ViewBag.SalesMan = QkSelect.List(
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


        //[HttpPost]

        //#region All Sales
        [QkAuthorize(Roles = "Dev,Sales Profit")]
        public ActionResult SalesProfit(string seno, long? paymethod, long? customer, long? SalesExecutive, string fromdate, string todate, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate)
        {
            var lead = db.SourceOfLeads.Select(r => new
            {
                ID = r.SourceOfLeadId,
                Name = r.SrcName
            }).ToList();
            ViewBag.Source = QkSelect.List(lead, "ID", "Name");
            var loc = db.ProTasks
              .Select(s => new
              {
                  ID = s.Location,
                  Name = s.Location
              }).Distinct().OrderBy(o => o.Name).ToList();
            ViewBag.Local = QkSelect.List(loc, "ID", "Name");
            ViewBag.SalesMan = QkSelect.List(
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
        //[HttpPost]
        //[QkAuthorize(Roles = "Dev,All Sales")]


        public ActionResult employeesales()
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
                                      new SelectListItem { Selected = true, Text = "", Value = ""},
                                    }, "Value", "Text", 0);

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCNa vme
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
        public ActionResult employeesales(string InvoiceNo, long? ddlCustomer, long? ddlEmployee, string From, string To, long? ddlType, long? ddlMC, string SaleType, long? HireType, string FromDate, string ToDate, long? ddlProject, long? ddlProTask, bool wise)
        {
            return RedirectToAction("viewemployeesales", new { seno = InvoiceNo, customer = ddlCustomer, SalesExecutive = ddlEmployee, from = From, to = To, type = ddlType, ddMC = ddlMC, satype = SaleType, htype = HireType, hfdate = FromDate, htdate = ToDate, project = ddlProject, task = ddlProTask, perwise = wise });
        }





        public ActionResult GetAllSaleemployee(string seno, long? paymethod, long? customer, long? SalesExecutive, string fromdate, string todate, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate, long? project, long? task, Boolean perwise)
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

            SaleType St = new SaleType();
            if (satype != "")
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
            if (!string.IsNullOrEmpty(hfdate))
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
            var v = (from a in db.SalesEntrys
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                     join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                     join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                     from f in paymeth.DefaultIfEmpty()
                     join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                     from g in mcs.DefaultIfEmpty()
                     join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                     from h in hir.DefaultIfEmpty()
                     join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()
                     where
                    (perwise == true || SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                    (perwise == false || SalesExecutive == 0 || SalesExecutive == null || b.SalesPerson == SalesExecutive) &&

                    (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                    (todate == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0)

                     select new
                     {
                         a.SalesEntryId,
                         a.SENo,
                         a.BillNo,
                         a.SEDate,
                         a.SEGrandTotal,
                         Customer = b.CustomerName,
                         TaxRegNo = i.TRN,
                         EmpName = d.FirstName + " " + d.LastName,
                         MCName = g.MCName,
                         c.SEPaidAmount,
                         a.CustomerType,
                         SEBalanceAmount = a.SEGrandTotal - c.SEPaidAmount,
                         a.SECreatedDate,
                         PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                         SaleType = a.SaleType,
                         FromDate = h.StartDate,
                         ToDate = h.EndDate,
                         HireType = h.HireType
                     }).OrderBy(a => a.SEDate).ThenBy(a => a.SECreatedDate);

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
        [QkAuthorize(Roles = "Dev,SalesReturn Reports")]
        public ActionResult AllSalesReturn(string seno, long? paymethod, long? customer, long? SalesExecutive, string from, string to, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate)
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

        [QkAuthorize(Roles = "Dev,All Sales")]
        public ActionResult Indexrebate(string seno, long? paymethod, long? customer, long? SalesExecutive, string from, string to, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate)
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

        public ActionResult Comparison(string seno, long? paymethod, long? customer, long? SalesExecutive, string from, string to, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate)
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
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
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


        //#region All Sales
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
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
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
        //[HttpPost]
        //[QkAuthorize(Roles = "Dev,All Sales")]
        //#region All Sales





        public class lastsales
        {
            public long salesid { get; set; }
            public decimal price { get; set; }
            public DateTime sedat { get; set; }
            public string invoiceno { get; set; }
            public decimal qty { get; set; }
        }
        public lastsales getlastsales(long itemid, int pos)
        {
            var data = (from a in db.SalesEntrys
                        join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                        join c in db.Items on new { g1 = b.Item, g2 = b.ItemUnit } equals new
                        {
                            g1 = c.ItemID,
                            g2 = c.ItemUnitID
                        }
                        where b.Item == itemid
                        select new lastsales
                        {
                            salesid = a.SalesEntryId,
                            price = b.ItemUnitPrice,
                            sedat = a.SEDate,
                            invoiceno = a.BillNo,
                            qty = b.ItemQuantity

                        }).OrderByDescending(o => o.sedat).Skip(pos - 1).Take(1).FirstOrDefault();
            return data;
        }
        public ActionResult SalesPriceComparison()
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
        [QkAuthorize(Roles = "Dev,All Purchase")]
        public ActionResult GetAllSal(string srchtxt, long? customer, string fromdate, string todate, long comparprices, long? brand, long? category)
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
                     join b in db.SEItemss on new { g1 = a.ItemID, g2 = a.ItemUnitID } equals new
                     { g1 = b.Item, g2 = b.ItemUnit }
                     join c in db.SalesEntrys on b.SalesEntry equals c.SalesEntryId

                     where
                           a.ItemName.Contains(srchtxt) &&

                                 (customer == 0 || c.Customer == customer) &&
                                 (fromdate == "" || EF.Functions.DateDiffDay(c.SEDate, fdate) <= 0) &&
                                 (todate == "" || EF.Functions.DateDiffDay(c.SEDate, tdate) >= 0) &&
                                 (category == 0 || a.ItemCategoryID == category) &&
                                 (brand == 0 || a.ItemBrandID == brand)


                     select new
                     {
                         a.ItemID,
                         ItemName = a.ItemCode + " " + a.ItemName
                     }).Distinct().ToList().Select(o => new
                     {
                         o.ItemName,
                         o.ItemID,
                         purchase1 = getlastsales(o.ItemID, 10),
                         purchase2 = getlastsales(o.ItemID, 9),
                         purchase3 = getlastsales(o.ItemID, 8),
                         purchase4 = getlastsales(o.ItemID, 7),
                         purchase5 = getlastsales(o.ItemID, 6),
                         purchase6 = getlastsales(o.ItemID, 5),
                         purchase7 = getlastsales(o.ItemID, 4),
                         purchase8 = getlastsales(o.ItemID, 3),
                         purchase9 = getlastsales(o.ItemID, 2),
                         purchase10 = getlastsales(o.ItemID, 1),


                     }).Select(o => new
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
                         compairstatus = getcompairstatus(comparprices, o.purchase1, o.purchase2, o.purchase3, o.purchase4, o.purchase5, o.purchase6, o.purchase7, o.purchase8, o.purchase9, o.purchase10)
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
        public string getcompairstatus(long comparprices, lastsales purchase1, lastsales purchase2, lastsales purchase3, lastsales purchase4, lastsales purchase5, lastsales purchase6, lastsales purchase7, lastsales purchase8, lastsales purchase9, lastsales purchase10)
        {
            string result = "Normal";
            decimal[] prices = new decimal[comparprices];
            prices[0] = purchase10 == null ? 0 : purchase10.price;
            prices[1] = purchase9 == null ? 0 : purchase9.price;
            if (comparprices > 2)
                prices[2] = purchase8 == null ? 0 : purchase8.price;
            if (comparprices > 3)
                prices[3] = purchase7 == null ? 0 : purchase7.price;
            if (comparprices > 4)
                prices[4] = purchase6 == null ? 0 : purchase6.price;
            if (comparprices > 5)
                prices[5] = purchase5 == null ? 0 : purchase5.price;
            if (comparprices > 6)
                prices[6] = purchase4 == null ? 0 : purchase4.price;
            if (comparprices > 7)
                prices[7] = purchase3 == null ? 0 : purchase3.price;
            if (comparprices > 8)
                prices[8] = purchase2 == null ? 0 : purchase2.price;
            if (comparprices > 9)
                prices[9] = purchase1 == null ? 0 : purchase1.price;
            decimal[] partprices = prices.Where(o => o != 0).ToArray();
            if (partprices.Distinct().Count() > 1)
            {
                result = "Modified";
            }

            return result;
        }












        [HttpPost]
        [QkAuthorize(Roles = "Dev,All Sales")]
        public ActionResult GetAllSaleRebate(string srchtxt, long? brand, long? category, long? item, string seno, long? paymethod, long? customer, long? SalesExecutive, long? SalesMan, string fromdate, string todate, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate, long? project, long? task)
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
            if (satype != "")
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
            if (!string.IsNullOrEmpty(hfdate))
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
            var v = (from a in db.SalesEntrys
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join c in db.SEItemss on a.SalesEntryId equals c.SalesEntry
                     join d in db.Items on c.Item equals d.ItemID
                     join uu in db.ItemUnits on d.ItemUnitID equals uu.ItemUnitID
                     join sr in db.SalesReturns on a.SalesEntryId equals sr.SalesEntryId into srr
                     from sr in srr.DefaultIfEmpty()
                     join srit in db.SRItemss on sr.SalesReturnId equals srit.SalesReturnId into srrit
                     from srit in srrit.DefaultIfEmpty()
                         //equals new { h1 = h.Reference, h2 = h.Section } into hir
                         //equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                         //let itemrebate = (from aa in db.SEItemss
                         //                  (srchtxt == "" || cc.ItemName.Contains(srchtxt))

                         //                      itemid = aa.Item,
                         //                      unitprice = aa.ItemUnitPrice,
                         //                      itemqty =aa.ItemQuantity,
                         //                      itemunitid=aa.ItemUnit
                         //                      //Total =(grp.FirstOrDefault().ItemUnit== grp.FirstOrDefault().ItemUnitID)? grp.Sum(o => o.ItemQuantity) * grp.FirstOrDefault().PurchasePrice: grp.Sum(o => o.ItemQuantity) * grp.FirstOrDefault().PurchasePrice/grp.FirstOrDefault().ConFactor
                         //                  }).ToList().Select(o => new
                         //                      itemqty=o.itemqty,
                         //                      price = o.unitprice * o.itemqty
                         //                  })



                         //group new { aa.ItemQuantity, uu.ItemUnitName, cc.PurchasePrice, cc.ItemCode, cc.ItemName, cc.ConFactor, aa.ItemUnit, cc.ItemUnitID } by aa.Item into grp
                         //                      itemname = grp.FirstOrDefault().ItemCode + " - " + grp.FirstOrDefault().ItemName,
                         //                      itemunit = grp.FirstOrDefault().ItemUnitName,
                         //                      price = grp.Sum(o => o.ItemQuantity) * grp.FirstOrDefault().PurchasePrice


                         //                  }).ToList()
                     where

                     (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                   (customer == 0 || a.Customer == customer) &&

                    (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                    (SalesMan == 0 || SalesMan == null || b.SalesPerson == SalesMan) &&
                    (type == null || a.CustomerType == sEntry.CustomerType) &&
                    (paymethod == null || a.PaymentMethod == paymethod) &&
                    (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                    (todate == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                    (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
              (ddMC == null || ddMC == 0 || ddMC == a.MaterialCenter) &&
                    (project == 0 || project == null || a.Project == project) &&
                    (task == 0 || task == null || a.ProTask == task)
                     select new
                     {
                         a.SalesEntryId,
                         a.SENo,
                         a.BillNo,
                         a.SEDate,
                         a.SESubTotal,

                         // itemrebate,
                         Customer = b.CustomerName,
                         b.CustomerID,
                         a.CustomerType,
                         a.SECreatedDate,
                         SaleType = a.SaleType,
                         sprice = c.ItemUnitPrice,
                         pprice = d.PurchasePrice,
                         qty = c.ItemQuantity - ((srit == null) ? 0 : srit.ItemQuantity),
                         unit = uu.ItemUnitName,
                         invdate = a.SEDate,
                         itemname = d.ItemCode + "-" + d.ItemName,
                         //Credit = (srchtxt == "") ? (j.Credit == null ? 0 : j.Credit) : 0,
                         //taxableAmt = (srchtxt == "") ? (Decimal?)taxamountt.Credit : null,
                         //discount,
                         //roundoffplus,
                         //roundoffmin
                     }).OrderBy(a => a.CustomerID).ThenBy(a => a.SECreatedDate);
            if (srchtxt != "")
            {
            }
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

        public ActionResult GetComparison(string srchtxt, long? brand, long? category, long? item, long? ddMCfrom, long? ddMCto)
        {

            Int64 temp = 502;
            Int64 sac = 1;







            var v = (from a in db.Items


                     where (item == 0 || item == null || a.ItemID == item) &&
                    (brand == 0 || brand == null || a.ItemBrandID == brand) &&
                    (category == 0 || category == null || a.ItemCategoryID == category) &&
                    (srchtxt == "" || a.ItemName.Contains(srchtxt))
                     select new
                     {
                         a.ItemID,
                         a.ItemCode,
                         a.ItemName,
                     }).ToList().Select(o => new
                     {
                         o.ItemID,
                         o.ItemName,
                         o.ItemCode,
                         stockfromstock = com.GetItemWisestock(o.ItemID, ddMCfrom),
                         stocktostock = com.GetItemWisestock(o.ItemID, ddMCto),
                     }).Where(o => o.stockfromstock > 0);
            var data = v.OrderBy(o => o.stocktostock).ToList();
            var recordsTotal = v.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }


        [HttpPost]
        [QkAuthorize(Roles = "Dev,All Sales")]
        public ActionResult GetAllSale(string srchtxt, long? brand, long? category, long? item, string seno, long? paymethod, long? customer, long? SalesExecutive, long? SalesMan, string fromdate, string todate, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate, long? project, long? task)
        {

            Int64 temp = 502;
            Int64 sac = 1;
            var saleaccounts = db.Projects.Select(o => o.IncomeAccount).Distinct().ToArray();
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddMC == 0 || ddMC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            long discountid = db.BillSundrys.Where(o => o.BSName.Contains("DISCOUNT")).Select(o => o.BillSundryId).FirstOrDefault();
            SaleType St = new SaleType();
            if (satype != "")
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
            if (hfdate != null && hfdate != "")
            {
                hfrmdate = DateTime.Parse(hfdate, new CultureInfo("en-GB"));
            }
            if (htdate != null && htdate != "")
            {
                htodate = DateTime.Parse(htdate, new CultureInfo("en-GB"));
            }

            SalesEntry sEntry = new SalesEntry();
            sEntry.CustomerType = (type == 1) ? CustomerType.Walking : (type == 2) ? CustomerType.Card : (type == 3) ? CustomerType.Online : (type == 4) ? CustomerType.OnlineAccount : CustomerType.Customer; ;

            paymethod = paymethod == 0 ? null : paymethod;
            var v = (from a in db.SalesEntrys
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join si in db.SEItemss on a.SalesEntryId equals si.SalesEntry
                     join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry into sepa
                     from c in sepa.DefaultIfEmpty()
                     join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                     join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                     from f in paymeth.DefaultIfEmpty()
                     join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                     from g in mcs.DefaultIfEmpty()
                     join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                     from h in hir.DefaultIfEmpty()
                     join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()
                     join j in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = temp }
                     equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                     from j in hir1.DefaultIfEmpty()
                     join k in db.SEItemss on a.SalesEntryId equals k.SalesEntry into temp2
                     from k in temp2.DefaultIfEmpty()
                     join l in db.Items on k.Item equals l.ItemID into temp3
                     from l in temp3.DefaultIfEmpty()
                     let grandtotalitmsearch = (decimal)(from m in db.SEItemss
                                                         join nn in db.Items on m.Item equals nn.ItemID
                                                         join oo in db.SalesEntrys on m.SalesEntry equals oo.SalesEntryId
                                                         where nn.ItemName.Contains(srchtxt)
                                                         && oo.SalesEntryId == a.SalesEntryId

                                                         select new
                                                         {
                                                             totalprice = m.ItemUnitPrice * m.ItemQuantity
                                                         }).Sum(o => o.totalprice)
                     let taxamountt = (from ii in db.AccountsTransactions

                                       where (
                          ii.reference == a.SalesEntryId && ii.Purpose == "Sale"
                          && saleaccounts.Contains(ii.Account))
                                       select new
                                       {
                                           ii.Credit
                                       }).FirstOrDefault()
                     let discount = (from k in db.SEBillSundrys
                                     where k.SalesEntry == a.SalesEntryId &&
                                     k.BillSundry == discountid
                                     select new
                                     {
                                         k.BsAmount
                                     }).FirstOrDefault().BsAmount
                     let roundoffmin = (
                                        from k in db.SEBillSundrys
                                        where k.SalesEntry == a.SalesEntryId &&
                                       k.BillSundry == 2
                                        select new
                                        {
                                            k.BsAmount
                                        }).FirstOrDefault().BsAmount
                     let roundoffplus = (from k in db.SEBillSundrys
                                         where k.SalesEntry == a.SalesEntryId &&
                                        k.BillSundry == 1
                                         select new
                                         {
                                             k.BsAmount
                                         }).FirstOrDefault().BsAmount


                     where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                   (customer == 0 || a.Customer == customer) &&

                    (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                    (SalesMan == 0 || SalesMan == null || b.SalesPerson == SalesMan) &&
                    (item == 0 || item == null || k.Item == item) &&
                    (brand == 0 || brand == null || l.ItemBrandID == brand) &&
                    (category == 0 || category == null || l.ItemCategoryID == category) &&
                    (srchtxt == "" || l.ItemName.Contains(srchtxt)) &&
                    (type == null || a.CustomerType == sEntry.CustomerType) &&
                    (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                    (todate == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                    (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                    (htype == null || htype == null || h.HireType == htype) &&
                    (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                    (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0) &&
              (ddMC == null || ddMC == 0 || ddMC == a.MaterialCenter) &&
                    (project == 0 || project == null || a.Project == project) &&
                    (task == 0 || task == null || a.ProTask == task)
                     select new
                     {
                         i.AccountsID,
                         a.SalesEntryId,
                         a.SENo,
                         a.BillNo,
                         a.SEDate,
                         a.SESubTotal,
                         SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : grandtotalitmsearch,
                         Customer = b.CustomerName,
                         b.CustomerID,
                         TaxRegNo = i.TRN,
                         EmpName = d.FirstName + " " + d.LastName,
                         MCName = g.MCName,
                         SEPaidAmount = (c.SEPaidAmount == null ? 0 : c.SEPaidAmount),
                         a.CustomerType,
                         SEBalanceAmount = (a.SEGrandTotal == null ? 0 : a.SEGrandTotal) - (c.SEPaidAmount == null ? 0 : c.SEPaidAmount),
                         a.SECreatedDate,
                         PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                         SaleType = a.SaleType,
                         FromDate = h.StartDate,
                         ToDate = h.EndDate,
                         HireType = h.HireType,
                         Credit = (srchtxt == "") ? (j.Credit == null ? 0 : j.Credit) : 0,
                         taxableAmt = (srchtxt == "") ? (Decimal?)taxamountt.Credit : null,
                         discount,
                         roundoffplus,
                         roundoffmin
                     }).Distinct().OrderBy(a => a.SEDate).ThenBy(a => a.SECreatedDate);

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
        [QkAuthorize(Roles = "Dev,All Sales")]
        public ActionResult SalesRebateCustomerWise(string seno, long? paymethod, long? customer, long? SalesExecutive, string from, string to, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate)
        {
            ViewBag.CustName = QkSelect.List(
                 new List<SelectListItem>
                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                 }, "Value", "Text", 0);
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

        [HttpPost]
        [QkAuthorize(Roles = "Dev,All Sales")]
        public ActionResult GetAllSaleRebateCustomerWise(string srchtxt, long? brand, long? category, long? item, string seno, long? paymethod, long? customer, long? SalesExecutive, long? SalesMan, string fromdate, string todate, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate, long? project, long? task, long? custyp)
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
            if (satype != "")
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
            if (!string.IsNullOrEmpty(hfdate))
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
            var trascustomer = (from a in db.Customers
                                join bb in db.SalesEntrys on a.CustomerID equals bb.Customer
                                where
                                   (fromdate == "" || EF.Functions.DateDiffDay(bb.SEDate, fdate) <= 0) &&
                                   (todate == "" || EF.Functions.DateDiffDay(bb.SEDate, tdate) >= 0)
                                select new
                                {
                                    a.CustomerID,
                                    a.SalesPerson,
                                    a.CustomerType,
                                    a.CustomerName,
                                    a.CustomerCode
                                }



                              ).Distinct();
            // EF Core 10 cannot translate the per-customer rebate `let` subqueries: each grouped subquery has an
            // intermediate `.ToList().Select(...).Sum(...)` which surfaces an untranslatable `Expression of type
            // 'List<anonymous>'`. Split SERVER from CLIENT: materialize the filtered customer base rows, compute
            // both rebate aggregates as grouped server queries keyed by CustomerID (sum of unitprice*qty per
            // Item/UnitPrice group, summed per customer Ã¢â‚¬â€ no intermediate ToList), then re-project client-side.
            var custRows = (from a in trascustomer
                            where
                            (SalesMan == 0 || SalesMan == null || a.SalesPerson == SalesMan) &&
                            (customer == 0 || a.CustomerID == customer) &&
                            (custyp == 0 || a.CustomerType == custyp)
                            select new
                            {
                                a.CustomerID,
                                a.CustomerName,
                            }).Distinct().ToList();

            // itemrebate per customer: group SEItems by (Customer, Item, UnitPrice) -> unitprice*Sum(qty), summed per customer.
            var itemrebateLookup = (from aa in db.SEItemss
                                    join bb in db.SalesEntrys on aa.SalesEntry equals bb.SalesEntryId
                                    join cc in db.Items on aa.Item equals cc.ItemID
                                    join uu in db.ItemUnits on cc.ItemUnitID equals uu.ItemUnitID
                                    where
                                    (fromdate == "" || EF.Functions.DateDiffDay(bb.SEDate, fdate) <= 0) &&
                                    (todate == "" || EF.Functions.DateDiffDay(bb.SEDate, tdate) >= 0) &&
                                    (seno == "" || seno == null || bb.BillNo == seno) && bb.Status == 1 &&
                                    (SalesExecutive == 0 || SalesExecutive == null || bb.SECashier == SalesExecutive) &&
                                    (ddMC == null || ddMC == 0 || ddMC == bb.MaterialCenter) &&
                                    (project == 0 || project == null || bb.Project == project) &&
                                    (task == 0 || task == null || bb.ProTask == task) &&
                                    (string.IsNullOrEmpty(satype) || bb.SaleType == St) &&
                                    (paymethod == null || bb.PaymentMethod == paymethod) &&
                                    (srchtxt == "" || srchtxt == null || cc.ItemName.Contains(srchtxt))
                                    group new { aa.ItemQuantity } by new { bb.Customer, aa.Item, aa.ItemUnitPrice } into grp
                                    select new
                                    {
                                        grp.Key.Customer,
                                        total = grp.Key.ItemUnitPrice * grp.Sum(o => o.ItemQuantity)
                                    })
                                    .ToList()
                                    .GroupBy(o => o.Customer)
                                    .ToDictionary(g => g.Key, g => g.Sum(o => o.total));

            // itemrebatesr per customer: same shape over SRItems (sales returns).
            var itemrebatesrLookup = (from aa in db.SRItemss
                                      join bb in db.SalesReturns on aa.SalesReturnId equals bb.SalesReturnId
                                      join cc in db.Items on aa.Item equals cc.ItemID
                                      join uu in db.ItemUnits on cc.ItemUnitID equals uu.ItemUnitID
                                      join vv in db.SalesEntrys on bb.SalesEntryId equals vv.SalesEntryId
                                      where
                                      (fromdate == "" || EF.Functions.DateDiffDay(vv.SEDate, fdate) <= 0) &&
                                      (todate == "" || EF.Functions.DateDiffDay(vv.SEDate, tdate) >= 0) &&
                                      (seno == "" || seno == null || bb.BillNo == seno) && bb.Status == 1 &&
                                      (SalesExecutive == 0 || SalesExecutive == null || bb.SRCashier == SalesExecutive) &&
                                      (ddMC == null || ddMC == 0 || ddMC == bb.MaterialCenter) &&
                                      (project == 0 || project == null || bb.Project == project) &&
                                      (task == 0 || task == null || bb.ProTask == task) &&
                                      (string.IsNullOrEmpty(satype) || bb.SaleType == St) &&
                                      (srchtxt == "" || srchtxt == null || cc.ItemName.Contains(srchtxt))
                                      group new { aa.ItemQuantity } by new { bb.Customer, aa.Item, aa.ItemUnitPrice } into grp
                                      select new
                                      {
                                          grp.Key.Customer,
                                          total = grp.Key.ItemUnitPrice * grp.Sum(o => o.ItemQuantity)
                                      })
                                      .ToList()
                                      .GroupBy(o => o.Customer)
                                      .ToDictionary(g => g.Key, g => g.Sum(o => o.total));

            var v2 = custRows.Select(a =>
                      {
                          decimal? itemrebate = itemrebateLookup.TryGetValue(a.CustomerID, out var ir) ? ir : (decimal?)null;
                          decimal? itemrebatesr = itemrebatesrLookup.TryGetValue(a.CustomerID, out var irs) ? irs : (decimal?)null;
                          return new
                          {
                              a.CustomerID,
                              itemrebate = ((itemrebate != null) ? itemrebate : 0) - ((itemrebatesr != null) ? itemrebatesr : 0),
                              Customer = a.CustomerName,
                          };
                      }).Distinct().OrderByDescending(a => a.itemrebate).ToList();//.ThenBy(a => a.SECreatedDate);


            if (srchtxt != "")
            {
            }
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
        [QkAuthorize(Roles = "Dev,All Sales")]
        public ActionResult GetAllSaleRebateCustomerWisebonus(string srchtxt, long? brand, long? category, long? item, string seno, long? paymethod, long? customer, long? SalesExecutive, long? SalesMan, string fromdate, string todate, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate, long? project, long? task, long? custyp)
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
            if (satype != "")
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
            if (!string.IsNullOrEmpty(hfdate))
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
            var trascustomer = (from a in db.Customers
                                join bb in db.SalesEntrys on a.CustomerID equals bb.Customer
                                where
                                   (fromdate == "" || EF.Functions.DateDiffDay(bb.SEDate, fdate) <= 0) &&
                                   (todate == "" || EF.Functions.DateDiffDay(bb.SEDate, tdate) >= 0) &&
                                   bb.SEDate >= a.startbonusdate

                                select new
                                {
                                    a.CustomerID,
                                    a.SalesPerson,
                                    a.CustomerType,
                                    a.CustomerName,
                                    a.CustomerCode
                                }



                              ).Distinct();
            // EF Core 10 cannot translate the per-customer rebate `let` subqueries: each grouped subquery has an
            // intermediate `.ToList().Select(...).Sum(...)` which surfaces an untranslatable `Expression of type
            // 'List<anonymous>'`. Split SERVER from CLIENT: materialize the customer base rows, compute both
            // rebate aggregates as grouped server queries keyed by CustomerID (sum of unitprice*qty per
            // Item/UnitPrice group, summed per customer Ã¢â‚¬â€ no intermediate ToList), then re-project client-side.
            var custRows = (from a in trascustomer
                            select new
                            {
                                a.CustomerID,
                                a.CustomerName,
                            }).Distinct().ToList();

            // itemrebate per customer: group SEItems by (Customer, Item, UnitPrice) -> unitprice*Sum(qty), summed per customer.
            var itemrebateLookup = (from aa in db.SEItemss
                                    join bb in db.SalesEntrys on aa.SalesEntry equals bb.SalesEntryId
                                    join cc in db.Items on aa.Item equals cc.ItemID
                                    join uu in db.ItemUnits on cc.ItemUnitID equals uu.ItemUnitID
                                    where
                                    (fromdate == "" || EF.Functions.DateDiffDay(bb.SEDate, fdate) <= 0) &&
                                    (todate == "" || EF.Functions.DateDiffDay(bb.SEDate, tdate) >= 0)
                                    group new { aa.ItemQuantity } by new { bb.Customer, aa.Item, aa.ItemUnitPrice } into grp
                                    select new
                                    {
                                        grp.Key.Customer,
                                        total = grp.Key.ItemUnitPrice * grp.Sum(o => o.ItemQuantity)
                                    })
                                    .ToList()
                                    .GroupBy(o => o.Customer)
                                    .ToDictionary(g => g.Key, g => g.Sum(o => o.total));

            // itemrebatesr per customer: same shape over SRItems (sales returns).
            var itemrebatesrLookup = (from aa in db.SRItemss
                                      join bb in db.SalesReturns on aa.SalesReturnId equals bb.SalesReturnId
                                      join cc in db.Items on aa.Item equals cc.ItemID
                                      join uu in db.ItemUnits on cc.ItemUnitID equals uu.ItemUnitID
                                      join vv in db.SalesEntrys on bb.SalesEntryId equals vv.SalesEntryId
                                      where
                                      (fromdate == "" || EF.Functions.DateDiffDay(vv.SEDate, fdate) <= 0) &&
                                      (todate == "" || EF.Functions.DateDiffDay(vv.SEDate, tdate) >= 0)
                                      group new { aa.ItemQuantity } by new { bb.Customer, aa.Item, aa.ItemUnitPrice } into grp
                                      select new
                                      {
                                          grp.Key.Customer,
                                          total = grp.Key.ItemUnitPrice * grp.Sum(o => o.ItemQuantity)
                                      })
                                      .ToList()
                                      .GroupBy(o => o.Customer)
                                      .ToDictionary(g => g.Key, g => g.Sum(o => o.total));

            var v2 = custRows.Select(a =>
                      {
                          decimal? itemrebate = itemrebateLookup.TryGetValue(a.CustomerID, out var ir) ? ir : (decimal?)null;
                          decimal? itemrebatesr = itemrebatesrLookup.TryGetValue(a.CustomerID, out var irs) ? irs : (decimal?)null;
                          return new
                          {
                              a.CustomerID,
                              itemrebate = ((itemrebate != null) ? itemrebate : 0) - ((itemrebatesr != null) ? itemrebatesr : 0),
                              Customer = a.CustomerName,
                          };
                      }).Distinct().OrderByDescending(a => a.itemrebate).ToList();//.ThenBy(a => a.SECreatedDate);


            if (srchtxt != "")
            {
            }
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
        [QkAuthorize(Roles = "Dev,SalesReturn Reports")]
        public ActionResult GetAllSaleReturn(string seno, long? paymethod, long? customer, long? SalesExecutive, long? SalesMan, string fromdate, string todate, long? type, long? ddMC, long? project, long? task)
        {

            Int64 temp = 502;
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

            SalesReturn sReturn = new SalesReturn();
            sReturn.CustomerType = (type == 1) ? CustomerType.Walking : (type == 0) ? CustomerType.Customer : CustomerType.Card;

            paymethod = paymethod == 0 ? null : paymethod;
            var v = (from a in db.SalesReturns
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join d in db.Employees on a.SRCashier equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                     join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                     from g in mcs.DefaultIfEmpty()
                     join j in db.AccountsTransactions on new { j1 = a.SalesReturnId, j2 = "Sale Return", j3 = temp }
                     equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                     from j in hir1.DefaultIfEmpty()
                         //equals new { h1 = h.Reference, h2 = h.Section } into hir
                     where
                    (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                    (customer == 0 || a.Customer == customer) &&
                    (SalesExecutive == 0 || SalesExecutive == null || a.SRCashier == SalesExecutive) &&
                    (SalesMan == 0 || SalesMan == null || b.SalesPerson == SalesMan) &&
                    (type == null || a.CustomerType == sReturn.CustomerType) &&
                    (fromdate == "" || EF.Functions.DateDiffDay(a.SRDate, fdate) <= 0) &&
                    (todate == "" || EF.Functions.DateDiffDay(a.SRDate, tdate) >= 0) &&
                    ((!MCList.Any() && ddMC == null || ddMC == 0) || MCArray.Contains(a.MaterialCenter) || ddMC == a.MaterialCenter) &&
                    (project == 0 || project == null || a.Project == project) &&
                    (task == 0 || task == null || a.ProTask == task)//&&
                     select new
                     {
                         a.SalesReturnId,
                         a.BillNo,
                         a.SRDate,
                         a.SRGrandTotal,
                         Customer = b.CustomerName,
                         taxableAmt = a.SRSubTotal - a.SRDiscount,
                         EmpName = d.FirstName + " " + d.LastName,
                         MCName = g.MCName,
                         Debit = (j.Debit == null ? 0 : j.Debit),
                         a.SRCreatedDate,
                         //a.SRSubTotal,
                         //TaxRegNo = i.TRN,
                         // c.SRPaidAmount,
                         //a.CustomerType,
                         //SEBalanceAmount = a.SRGrandTotal - c.SRPaidAmount,
                         //a.SRNo,
                         //SaleType = a.SaleType,
                         //FromDate = h.StartDate,
                         //ToDate = h.EndDate,
                         //HireType = h.HireType,
                     }).OrderBy(a => a.SRDate).ThenBy(a => a.SRCreatedDate);

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
        //[HttpPost]
        //[QkAuthorize(Roles = "Dev,All Sales")]







        //    //if (type == 1)
        //    //else if (type == 0)

        //                 equals new { h1 = h.Reference, h2 = h.Section } into hir
        //                 equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
        //                (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)

        //                     (source==0|| source==null||b.SourceOfLead == source)

        //                     a.SalesEntryId,
        //                     a.SENo,
        //                     a.BillNo,
        //                     a.SEDate,
        //                     SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : 0,
        //                     SETaxAmount = (srchtxt == "") ? a.SETaxAmount : 0,
        //                     Credit = (srchtxt == "") ? j.Credit : 0,
        //                     Customer = b.CustomerName,
        //                     TaxRegNo = i.TRN,
        //                     EmpName = d.FirstName + " " + d.LastName,
        //                     MCName = g.MCName,
        //                     SEPaidAmount = (srchtxt == "") ? c.SEPaidAmount : 0,
        //                     a.CustomerType,
        //                     SEBalanceAmount = (srchtxt == "") ? (a.SEGrandTotal - c.SEPaidAmount) : 0,
        //                     //for expense

        //                     discountt = (decimal?)(from ayy in db.BillSundrys

        //                                            where ayy.BSName == "DISCOUNT" && azz.SalesEntry == a.SalesEntryId
        //                                                BsAmount = (azz.BsAmount == null) ? 0 : azz.BsAmount
        //                                      ).Sum(x => x.BsAmount) ?? 0,
        //                     //PaymentExpense = (decimal?)(from x in db.Payments

        //                     //                            where (x.InvoiceNo == a.BillNo)
        //                     //                            select new
        //                     //                                Expense = x.GrandTotal
        //                     //JournalExpense = (decimal?)(from x in db.Journals

        //                     //                            where (x.InvoiceNo == a.BillNo)
        //                     //                            select new
        //                     //                                Expense = x.GrandTotal


        //                     //itemprice = (decimal?)(from se in db.SEItemss
        //                     //                       join seen in db.SalesEntrys on se.SalesEntry equals seen.SalesEntryId
        //                     //                       join seit in db.Items on se.Item equals seit.ItemID
        //                     //                       where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
        //                     //                       select new
        //                     //                           purprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * se.ItemQuantity) : ((seit.PurchasePrice * se.ItemQuantity) / seit.ConFactor)
        //                     //           ).Sum(x => x.purprice) ?? 0,

        //                     a.SECreatedDate,
        //                     SaleType = a.SaleType,
        //                     FromDate = h.StartDate,
        //                     ToDate = h.EndDate,
        //                     HireType = h.HireType,
        //                     a.SalesStatus

        //                 }).Distinct().AsEnumerable().Select(o => new
        //                     o.SalesEntryId,
        //                     o.SENo,
        //                     o.Credit,
        //                     o.BillNo,
        //                     o.SEDate,

        //                     o.SEGrandTotal,
        //                     o.SETaxAmount,
        //                     o.Customer,
        //                     o.TaxRegNo,
        //                     o.discountt,
        //                     o.EmpName,
        //                     o.MCName,
        //                     o.SEPaidAmount,
        //                     o.CustomerType,
        //                     o.SEBalanceAmount,
        //                     NewExpense = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
        //                     ((o.PaymentExpense != null && o.JournalExpense != null) ? o.PaymentExpense + o.JournalExpense : ((o.PaymentExpense == null && o.JournalExpense != null) ? o.JournalExpense : o.PaymentExpense))) : 0,
        //                     salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
        //                     salesrtnnote = (o.salesreturnsnote == null) ? 0 : o.salesreturnsnote,

        //                     //Calling Function To Get Total Item Price for each Sales Entry
        //                     empcoste = GetTotalEmpCost(o.SalesEntryId) * perhourcost,
        //                     o.SECreatedDate,
        //                     o.PayMethod,
        //                     o.SaleType,
        //                     o.FromDate,
        //                     o.ToDate,
        //                     o.HireType,
        //                     o.SalesStatus,


        //            Content = result,
        //            ContentType = "application/json"
        //                 equals new { h1 = h.Reference, h2 = h.Section } into hir
        //                 equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
        //                 let grandtotalitmsearch = (decimal)(from m in db.SEItemss
        //                                                     where nn.ItemName.Contains(srchtxt)
        //                                                     && oo.SalesEntryId == a.SalesEntryId

        //                                                         totalprice = m.ItemUnitPrice * m.ItemQuantity
        //                                                     }).Sum(o => o.totalprice)

        //                (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)

        //                   (srchtxt == "" || l.ItemName.Contains(srchtxt))

        //                     a.SalesEntryId,
        //                     a.SENo,
        //                     a.BillNo,
        //                     a.SEDate,
        //                     SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : grandtotalitmsearch,
        //                     SETaxAmount = (srchtxt == "") ? a.SETaxAmount : 0,
        //                     Credit = (srchtxt == "") ? j.Credit : grandtotalitmsearch,
        //                     Customer = b.CustomerName,
        //                     TaxRegNo = i.TRN,
        //                     EmpName = d.FirstName + " " + d.LastName,
        //                     MCName = g.MCName,
        //                     SEPaidAmount = (srchtxt == "") ? c.SEPaidAmount : 0,
        //                     a.CustomerType,
        //                     SEBalanceAmount = (srchtxt == "") ? (a.SEGrandTotal - c.SEPaidAmount) : 0,
        //                     //for expense

        //                     discountt = (from ay in db.BillSundrys

        //                                  where ay.BSName == "DISCOUNT" && az.SalesEntry == a.SalesEntryId
        //                                      az.BsAmount
        //                                      ).Sum(x => x.BsAmount) ?? 0,
        //                     //PaymentExpense = (decimal?)(from x in db.Payments

        //                     //                            where (x.InvoiceNo == a.BillNo)
        //                     //                            select new
        //                     //                                Expense = x.GrandTotal
        //                     //JournalExpense = (decimal?)(from x in db.Journals

        //                     //                            where (x.InvoiceNo == a.BillNo)
        //                     //                            select new
        //                     //                                Expense = x.GrandTotal


        //                     //itemprice = (decimal?)(from se in db.SEItemss
        //                     //                       join seen in db.SalesEntrys on se.SalesEntry equals seen.SalesEntryId
        //                     //                       join seit in db.Items on se.Item equals seit.ItemID
        //                     //                       where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
        //                     //                       select new
        //                     //                           purprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * se.ItemQuantity) : ((seit.PurchasePrice * se.ItemQuantity) / seit.ConFactor)
        //                     //           ).Sum(x => x.purprice) ?? 0,

        //                     a.SECreatedDate,
        //                     SaleType = a.SaleType,
        //                     FromDate = h.StartDate,
        //                     ToDate = h.EndDate,
        //                     HireType = h.HireType,
        //                     a.SalesStatus

        //                 }).AsEnumerable().Select(o => new
        //                     o.SalesEntryId,
        //                     o.SENo,
        //                     o.Credit,
        //                     o.BillNo,
        //                     o.SEDate,
        //                     o.SEGrandTotal,
        //                     o.SETaxAmount,
        //                     o.Customer,
        //                     o.TaxRegNo,
        //                     o.discountt,
        //                     o.EmpName,
        //                     o.MCName,
        //                     o.SEPaidAmount,
        //                     o.CustomerType,
        //                     o.SEBalanceAmount,

        //                     NewExpense = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
        //                     ((o.PaymentExpense != null) ? o.PaymentExpense : o.JournalExpense)) : 0,
        //                     salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
        //                     salesrtnnote = (o.salesreturnsnote == null) ? 0 : o.salesreturnsnote,

        //                     //Calling Function To Get Total Item Price for each Sales Entry
        //                     empcoste = GetTotalEmpCost(o.SalesEntryId) * perhourcost,
        //                     o.SECreatedDate,
        //                     o.PayMethod,
        //                     o.SaleType,
        //                     o.FromDate,
        //                     o.ToDate,
        //                     o.HireType,
        //                     o.SalesStatus,


        //            Content = result,
        //            ContentType = "application/json"


        public ActionResult updateworkstatus(long salesid, long empid)
        {
            ViewBag.salesids = salesid;
            ViewBag.impids = empid;
            var Stat = QkSelect.List(
                new List<SelectListItem> {

                new SelectListItem { Value="0",Text="Completed"}, new SelectListItem { Value="1",Text="Not Completed"},}, "Value", "Text");
            ViewBag.workstatuss = Stat;
            return PartialView();
        }
        [HttpPost]
        public ActionResult updateworkstatus(int WorkStatus, long salesid, long empid)
        {
            bool stat = true;
            string msg = "Success";
            var exist = db.salesmanprofittargets.Any(o => o.salesentryid == salesid);
            if (exist)
            {
                var v = db.salesmanprofittargets.Where(o => o.salesentryid == salesid).FirstOrDefault();
                v.completed = WorkStatus;
                v.employeeid = empid;
                db.Entry(v).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                salesmanprofittarget v = new salesmanprofittarget
                {

                    salesentryid = salesid,
                    employeeid = empid,
                    completed = WorkStatus,


                };
                db.salesmanprofittargets.Add(v);
                db.SaveChanges();
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }
        public ActionResult updatecontribution(long salesid, long empid)
        {
            ViewBag.salesids = salesid;
            ViewBag.impids = empid;
            return PartialView();
        }
        [HttpPost]
        public ActionResult updatecontribution(long? percentage, long salesid, long empid)
        {
            bool stat = true;
            string msg = "Success";
            var exist = db.salesmanprofittargets.Any(o => o.salesentryid == salesid && o.employeeid == empid);
            if (exist)
            {
                var v = db.salesmanprofittargets.Where(o => o.salesentryid == salesid && o.employeeid == empid).FirstOrDefault();
                v.contributionpercentage = percentage;
                db.Entry(v).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                salesmanprofittarget v = new salesmanprofittarget
                {

                    salesentryid = salesid,
                    employeeid = empid,
                    contributionpercentage = percentage,


                };
                db.salesmanprofittargets.Add(v);
                db.SaveChanges();
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }
        [HttpPost]



        public ActionResult GetAllSaleprofitnew(string seno, double perhourcost, long? paymethod, long? SalesMan, long? customer, long? SalesExecutive, string fromdate, string todates, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate, long? project, long? task, string srchtxt, bool employeehourrate, long? source, string location, long? technician)
        {
            db.SetCommandTimeOut(60 * 60);
            decimal? target = null;
            if (SalesMan != null || SalesMan != 0)
            {
                string tr = db.Employees.Where(o => o.EmployeeId == SalesMan).Select(o => o.OtherIdNo).FirstOrDefault();
                if (tr != "" && tr != null)
                {
                    target = Convert.ToDecimal(tr);
                }
            }
            var isemirtech = db.companys.Any(o => o.CPName.Contains("EMIRTECH TECHNOLOGY"));
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
            if (satype != "")
            {
                St = (satype == "1") ? SaleType.Sale : SaleType.Hire;
            };

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todates != "")
            {
                tdate = DateTime.Parse(todates, new CultureInfo("en-GB"));
            }

            DateTime? hfrmdate = null;
            DateTime? htodate = null;
            if (!string.IsNullOrEmpty(hfdate))
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

            var allshworoom = db.MCs.Where(o => o.MCId == ddMC).Select(o => o.MCName).FirstOrDefault();

            if (allshworoom != "ALL SHOWROOMS")
            {
                if ((location == "All" || location == "") && (technician == null || technician == 0))
                {



                    if (srchtxt == "")
                    {

                        var v = (from a in db.SalesEntrys
                                 join b in db.Customers on a.Customer equals b.CustomerID into cust
                                 from b in cust.DefaultIfEmpty()
                                 join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry into sepay
                                 from c in sepay.DefaultIfEmpty()
                                 join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                                 from d in emp.DefaultIfEmpty()
                                 join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                                 from f in paymeth.DefaultIfEmpty()
                                 join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                                 from g in mcs.DefaultIfEmpty()
                                 join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                                 equals new { h1 = h.Reference, h2 = h.Section } into hir
                                 from h in hir.DefaultIfEmpty()
                                 join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                                 from i in acc.DefaultIfEmpty()
                                 join j in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = sac }
                                 equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                                 from j in hir1.DefaultIfEmpty()
                                 join m in db.Employees on b.SalesPerson equals m.EmployeeId into empp
                                 from m in empp.DefaultIfEmpty()
                                 where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                                (customer == 0 || a.Customer == customer) &&
                                (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                                (type == null || a.CustomerType == sEntry.CustomerType) &&
                                 (paymethod == null || a.PaymentMethod == paymethod) &&
                                 (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                                 (todates == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                                 (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                                (htype == null || htype == null || h.HireType == htype) &&
                                (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                                (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)
                               && (ddMC == 0 || ddMC == null || a.MaterialCenter == ddMC) &&

                                (project == 0 || project == null || a.Project == project) &&
                                (task == 0 || task == null || a.ProTask == task) &&
                                     (SalesMan == 0 || b.SalesPerson == SalesMan) && j.Credit > 0 &&

                                     (source == 0 || source == null || b.SourceOfLead == source)

                                 select new
                                 {

                                     a.SalesEntryId,
                                     a.SENo,
                                     a.BillNo,
                                     a.SEDate,
                                     SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : 0,
                                     SETaxAmount = (srchtxt == "") ? a.SETaxAmount : 0,
                                     Credit = (srchtxt == "") ? j.Credit : 0,
                                     Customer = b.CustomerName,
                                     b.CustomerID,
                                     TaxRegNo = i.TRN,
                                     EmpName = d.FirstName + " " + d.LastName,
                                     SalesMan = m.FirstName + " " + m.LastName,
                                     MCName = g.MCName,
                                     SEPaidAmount = (srchtxt == "") ? c.SEPaidAmount : 0,
                                     a.CustomerType,
                                     SEBalanceAmount = (srchtxt == "") ? (a.SEGrandTotal - c.SEPaidAmount) : 0,
                                     //for expense
                                     PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == a.BillNo).Select(x => x.Paying).Sum()),
                                     salesreturns = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 != "Credit Note").Select(x => x.SRSubTotal).Sum()),
                                     salesreturnsnote = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 == "Credit Note").Select(x => x.SRGrandTotal).Sum()),
                                     completed = (from comp in db.salesmanprofittargets
                                                  where comp.employeeid == b.SalesPerson &&
                                                  comp.salesentryid == a.SalesEntryId
                                                  select new
                                                  {
                                                      comp.completed,
                                                      comp.contributionpercentage
                                                  }
                                                ).FirstOrDefault(),
                                     discountt = (decimal?)(from ayy in db.BillSundrys
                                                            join azz in db.SEBillSundrys on ayy.BillSundryId equals azz.BillSundry

                                                            where ayy.BSName == "DISCOUNT" && azz.SalesEntry == a.SalesEntryId
                                                            select new
                                                            {
                                                                BsAmount = (azz.BsAmount == null) ? 0 : azz.BsAmount
                                                            }
                                                      ).Sum(x => x.BsAmount) ?? 0,
                                     //PaymentExpense = (decimal?)(from x in db.Payments

                                     //                            where (x.InvoiceNo == a.BillNo)
                                     //                                Expense = x.GrandTotal
                                     //JournalExpense = (decimal?)(from x in db.Journals

                                     //                            where (x.InvoiceNo == a.BillNo)
                                     //                                Expense = x.GrandTotal

                                     JournalExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == a.BillNo).Select(y => y.Paying).Sum()),

                                     //itemprice = (decimal?)(from se in db.SEItemss
                                     //                       where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
                                     //                           purprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * se.ItemQuantity) : ((seit.PurchasePrice * se.ItemQuantity) / seit.ConFactor)
                                     //           ).Sum(x => x.purprice) ?? 0,

                                     a.SECreatedDate,
                                     PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                                     SaleType = a.SaleType,
                                     FromDate = h.StartDate,
                                     ToDate = h.EndDate,
                                     HireType = h.HireType,
                                     a.SalesStatus

                                 });

                        var vv = v.AsEnumerable().Select(o => new
                        {
                            o.SalesEntryId,
                            o.SENo,
                            o.completed,
                            o.Credit,
                            o.BillNo,
                            o.SEDate,
                            target,
                            o.CustomerID,
                            o.SEGrandTotal,
                            o.SETaxAmount,
                            o.Customer,
                            o.TaxRegNo,
                            o.discountt,
                            o.EmpName,
                            o.MCName,
                            o.SEPaidAmount,
                            o.CustomerType,
                            o.SEBalanceAmount,
                            NewExpense = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
                                     ((o.PaymentExpense != null && o.JournalExpense != null) ? o.PaymentExpense + o.JournalExpense : ((o.PaymentExpense == null && o.JournalExpense != null) ? o.JournalExpense : o.PaymentExpense))) : 0,
                            NewExpenselink = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? "" :
                                     ((o.PaymentExpense != null && o.JournalExpense != null) ? String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>")) + " " + String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : ((o.PaymentExpense == null && o.JournalExpense != null) ? String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>"))))) : "",

                            salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
                            salesrtnnote = (o.salesreturnsnote == null) ? 0 : o.salesreturnsnote,

                            //Calling Function To Get Total Item Price for each Sales Entry
                            itemprice = (isemirtech == true) ? getusedprice(o.SalesEntryId) : null,
                            empcoste = (employeehourrate == true) ? (GetTotalEmpCost(o.SalesEntryId) * perhourcost) : GetTotalEmpCostwithrate(o.SalesEntryId),
                            o.SECreatedDate,
                            o.PayMethod,
                            o.SaleType,
                            o.FromDate,
                            o.ToDate,
                            o.HireType,
                            o.SalesStatus,
                            o.SalesMan
                        }).Distinct().OrderBy(a => a.SEDate).ToList();
                        var data = vv.ToList();
                        recordsTotal = 100;


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
                    else
                    {
                        var v = (from a in db.SalesEntrys
                                 join b in db.Customers on a.Customer equals b.CustomerID into cust
                                 from b in cust.DefaultIfEmpty()
                                 join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                                 join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                                 from d in emp.DefaultIfEmpty()
                                 join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                                 from f in paymeth.DefaultIfEmpty()
                                 join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                                 from g in mcs.DefaultIfEmpty()
                                 join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                                 equals new { h1 = h.Reference, h2 = h.Section } into hir
                                 from h in hir.DefaultIfEmpty()
                                 join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                                 from i in acc.DefaultIfEmpty()
                                 join j in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = sac }
                                 equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                                 from j in hir1.DefaultIfEmpty()
                                 join k in db.SEItemss on a.SalesEntryId equals k.SalesEntry into seit
                                 from k in seit.DefaultIfEmpty()
                                 join l in db.Items on k.Item equals l.ItemID into itemm
                                 from l in itemm.DefaultIfEmpty()
                                 join m in db.Employees on b.SalesPerson equals m.EmployeeId into empp
                                 from m in empp.DefaultIfEmpty()
                                 let grandtotalitmsearch = (decimal)(from m in db.SEItemss
                                                                     join nn in db.Items on m.Item equals nn.ItemID
                                                                     join oo in db.SalesEntrys on m.SalesEntry equals oo.SalesEntryId
                                                                     where nn.ItemName.Contains(srchtxt)
                                                                     && oo.SalesEntryId == a.SalesEntryId

                                                                     select new
                                                                     {
                                                                         totalprice = m.ItemUnitPrice * m.ItemQuantity
                                                                     }).Sum(o => o.totalprice)

                                 where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                                (customer == 0 || a.Customer == customer) &&
                                (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                                (type == null || a.CustomerType == sEntry.CustomerType) &&
                                 (paymethod == null || a.PaymentMethod == paymethod) &&
                                 (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                                 (todates == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                                 (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                                (htype == null || htype == null || h.HireType == htype) &&
                                (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                                (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)
                                && (ddMC == 0 || ddMC == null || a.MaterialCenter == ddMC) &&

                                (project == 0 || project == null || a.Project == project) &&
                                (task == 0 || task == null || a.ProTask == task) &&
                                   (srchtxt == "" || l.ItemName.Contains(srchtxt))
                                    &&
                                     (source == 0 || source == null || b.SourceOfLead == source)

                                 select new
                                 {

                                     a.SalesEntryId,
                                     a.SENo,
                                     a.BillNo,
                                     a.SEDate,
                                     SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : grandtotalitmsearch,
                                     SETaxAmount = (srchtxt == "") ? a.SETaxAmount : 0,
                                     Credit = (srchtxt == "") ? j.Credit : grandtotalitmsearch,
                                     Customer = b.CustomerName,
                                     b.CustomerID,
                                     TaxRegNo = i.TRN,
                                     EmpName = d.FirstName + " " + d.LastName,
                                     MCName = g.MCName,
                                     SalesMan = m.FirstName + " " + m.LastName,
                                     SEPaidAmount = (srchtxt == "") ? c.SEPaidAmount : 0,
                                     a.CustomerType,
                                     SEBalanceAmount = (srchtxt == "") ? (a.SEGrandTotal - c.SEPaidAmount) : 0,
                                     //for expense
                                     PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == a.BillNo).Select(x => x.Paying).Sum()),
                                     salesreturns = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 != "Credit Note").Select(x => x.SRGrandTotal).Sum()),
                                     salesreturnsnote = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 == "Credit Note").Select(x => x.SRGrandTotal).Sum()),
                                     completed = (from comp in db.salesmanprofittargets
                                                  where comp.employeeid == b.SalesPerson &&
                                                  comp.salesentryid == a.SalesEntryId
                                                  select new
                                                  {
                                                      comp.completed,
                                                      comp.contributionpercentage
                                                  }
                                                ).FirstOrDefault(),
                                     discountt = (from ay in db.BillSundrys
                                                  join az in db.SEBillSundrys on ay.BillSundryId equals az.BillSundry

                                                  where ay.BSName == "DISCOUNT" && az.SalesEntry == a.SalesEntryId
                                                  select new
                                                  {
                                                      az.BsAmount
                                                  }
                                                      ).Sum(x => x.BsAmount) ?? 0,
                                     //PaymentExpense = (decimal?)(from x in db.Payments

                                     //                            where (x.InvoiceNo == a.BillNo)
                                     //                                Expense = x.GrandTotal
                                     //JournalExpense = (decimal?)(from x in db.Journals

                                     //                            where (x.InvoiceNo == a.BillNo)
                                     //                                Expense = x.GrandTotal

                                     JournalExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == a.BillNo).Select(y => y.Paying).Sum()),
                                     //itemprice = (decimal?)(from se in db.SEItemss
                                     //                       where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
                                     //                           purprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * se.ItemQuantity) : ((seit.PurchasePrice * se.ItemQuantity) / seit.ConFactor)
                                     //           ).Sum(x => x.purprice) ?? 0,

                                     a.SECreatedDate,
                                     PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                                     SaleType = a.SaleType,
                                     FromDate = h.StartDate,
                                     ToDate = h.EndDate,
                                     HireType = h.HireType,
                                     a.SalesStatus

                                 }).AsEnumerable().Select(o => new
                                 {
                                     o.SalesEntryId,
                                     o.SENo,
                                     o.Credit,
                                     o.BillNo,
                                     o.completed,
                                     o.SEDate,
                                     o.SEGrandTotal,
                                     o.SETaxAmount,
                                     o.Customer,
                                     o.TaxRegNo,
                                     o.discountt,
                                     o.EmpName,
                                     o.MCName,
                                     o.SEPaidAmount,
                                     o.CustomerType,
                                     o.SEBalanceAmount,
                                     o.SalesMan,
                                     target,
                                     NewExpense = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
                                     ((o.PaymentExpense != null) ? o.PaymentExpense : o.JournalExpense)) : 0,
                                     NewExpenselink = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? "" :
                                     ((o.PaymentExpense != null && o.JournalExpense != null) ? String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>")) + " " + String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : ((o.PaymentExpense == null && o.JournalExpense != null) ? String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>"))))) : "",

                                     salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
                                     salesrtnnote = (o.salesreturnsnote == null) ? 0 : o.salesreturnsnote,

                                     //Calling Function To Get Total Item Price for each Sales Entry
                                     itemprice = (isemirtech == true) ? getusedprice(o.SalesEntryId) : GetTotalItemPrice(o.SalesEntryId, o.SEDate, srchtxt),
                                     empcoste = GetTotalEmpCost(o.SalesEntryId) * perhourcost,
                                     o.SECreatedDate,
                                     o.PayMethod,
                                     o.SaleType,
                                     o.FromDate,
                                     o.ToDate,
                                     o.CustomerID,
                                     o.HireType,
                                     o.SalesStatus,
                                 }).OrderBy(a => a.SEDate).ThenBy(a => a.SECreatedDate);
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
                }
                else
                {

                    var pid = (from x in db.ProTasks
                               where (location == "" || x.Location == location && (

                                 (fromdate == "" || EF.Functions.DateDiffDay(x.CreatedDate, fdate) <= 0) &&
                             (todates == "" || EF.Functions.DateDiffDay(x.CreatedDate, tdate) >= 0)



                               )
                               ) &&
                               (technician == null || technician == 0 || (from a in db.servicereports
                                                                          join
                                               b in db.servicereportmembers on a.servicereportid equals b.servicereportid
                                                                          where a.protaskid == x.ProTaskId && b.employeeid == technician
                                                                          select new { a.protaskid }).Any())






                               select new
                               {
                                   pids = x.ProTaskId
                               }).Select(o => o.pids).ToList().ToArray();

                    var salesids = (from x in db.SalesEntrys
                                    join y in db.additionaltasks on x.SalesEntryId equals y.salesentryid into yy
                                    from y in yy.DefaultIfEmpty()
                                    where (pid.Contains(y.taskid) || pid.Contains((long)x.ProTask))

                                    && (technician == null || technician == 0 || (
                                 (fromdate == "" || EF.Functions.DateDiffDay(x.SEDate, fdate) <= 0) &&
                             (todates == "" || EF.Functions.DateDiffDay(x.SEDate, tdate) >= 0)
                             ))
                                    select new
                                    {
                                        x.SalesEntryId
                                    }).Distinct().Select(o => o.SalesEntryId).ToList().ToArray();
                    var v = (from a in db.SalesEntrys

                             join b in db.Customers on a.Customer equals b.CustomerID into cust
                             from b in cust.DefaultIfEmpty()
                             join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                             join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                             from d in emp.DefaultIfEmpty()
                             join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                             from f in paymeth.DefaultIfEmpty()
                             join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                             from g in mcs.DefaultIfEmpty()
                             join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                             equals new { h1 = h.Reference, h2 = h.Section } into hir
                             from h in hir.DefaultIfEmpty()
                             join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                             from i in acc.DefaultIfEmpty()
                             join j in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = sac }
                             equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                             from j in hir1.DefaultIfEmpty()
                             join m in db.Employees on b.SalesPerson equals m.EmployeeId into empp
                             from m in empp.DefaultIfEmpty()
                             where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                            (customer == 0 || a.Customer == customer) &&
                            (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                            (type == null || a.CustomerType == sEntry.CustomerType) &&
                             (paymethod == null || a.PaymentMethod == paymethod) &&
                             salesids.Contains(a.SalesEntryId) &&
                             (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                            (htype == null || htype == null || h.HireType == htype) &&
                            (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                            (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)
                           && (ddMC == 0 || ddMC == null || a.MaterialCenter == ddMC) &&

                            (project == 0 || project == null || a.Project == project) &&
                            (task == 0 || task == null || a.ProTask == task) &&
                                 (SalesMan == 0 || b.SalesPerson == SalesMan) && j.Credit > 0 &&

                                 (source == 0 || source == null || b.SourceOfLead == source)

                             select new
                             {

                                 a.SalesEntryId,
                                 a.SENo,
                                 a.BillNo,
                                 a.SEDate,
                                 SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : 0,
                                 SETaxAmount = (srchtxt == "") ? a.SETaxAmount : 0,
                                 Credit = (srchtxt == "") ? j.Credit : 0,
                                 Customer = b.CustomerName,
                                 b.CustomerID,
                                 TaxRegNo = i.TRN,
                                 EmpName = d.FirstName + " " + d.LastName,
                                 SalesMan = m.FirstName + " " + m.LastName,
                                 MCName = g.MCName,
                                 SEPaidAmount = (srchtxt == "") ? c.SEPaidAmount : 0,
                                 a.CustomerType,
                                 SEBalanceAmount = (srchtxt == "") ? (a.SEGrandTotal - c.SEPaidAmount) : 0,
                                 //for expense
                                 PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == a.BillNo).Select(x => x.Paying).Sum()),
                                 salesreturns = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 != "Credit Note").Select(x => x.SRSubTotal).Sum()),
                                 salesreturnsnote = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 == "Credit Note").Select(x => x.SRGrandTotal).Sum()),

                                 discountt = (decimal?)(from ayy in db.BillSundrys
                                                        join azz in db.SEBillSundrys on ayy.BillSundryId equals azz.BillSundry

                                                        where ayy.BSName == "DISCOUNT" && azz.SalesEntry == a.SalesEntryId
                                                        select new
                                                        {
                                                            BsAmount = (azz.BsAmount == null) ? 0 : azz.BsAmount
                                                        }
                                                  ).Sum(x => x.BsAmount) ?? 0,
                                 //PaymentExpense = (decimal?)(from x in db.Payments

                                 //                            where (x.InvoiceNo == a.BillNo)
                                 //                                Expense = x.GrandTotal
                                 //JournalExpense = (decimal?)(from x in db.Journals

                                 //                            where (x.InvoiceNo == a.BillNo)
                                 //                                Expense = x.GrandTotal

                                 JournalExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == a.BillNo).Select(y => y.Paying).Sum()),

                                 //itemprice = (decimal?)(from se in db.SEItemss
                                 //                       where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
                                 //                           purprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * se.ItemQuantity) : ((seit.PurchasePrice * se.ItemQuantity) / seit.ConFactor)
                                 //           ).Sum(x => x.purprice) ?? 0,

                                 a.SECreatedDate,
                                 PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                                 SaleType = a.SaleType,
                                 FromDate = h.StartDate,
                                 ToDate = h.EndDate,
                                 HireType = h.HireType,
                                 a.SalesStatus

                             });

                    var vv = v.AsEnumerable().Select(o => new
                    {
                        o.SalesEntryId,
                        o.SENo,
                        o.Credit,
                        o.BillNo,
                        o.SEDate,
                        o.CustomerID,
                        o.SEGrandTotal,
                        o.SETaxAmount,
                        o.Customer,
                        o.TaxRegNo,
                        o.discountt,
                        o.EmpName,
                        o.MCName,
                        o.SEPaidAmount,
                        o.CustomerType,
                        o.SEBalanceAmount,
                        NewExpense = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
                                 ((o.PaymentExpense != null && o.JournalExpense != null) ? o.PaymentExpense + o.JournalExpense : ((o.PaymentExpense == null && o.JournalExpense != null) ? o.JournalExpense : o.PaymentExpense))) : 0,
                        NewExpenselink = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? "" :
                                 ((o.PaymentExpense != null && o.JournalExpense != null) ? String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>")) + " " + String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : ((o.PaymentExpense == null && o.JournalExpense != null) ? String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>"))))) : "",

                        salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
                        salesrtnnote = (o.salesreturnsnote == null) ? 0 : o.salesreturnsnote,

                        //Calling Function To Get Total Item Price for each Sales Entry
                        itemprice = (isemirtech == true) ? getusedprice(o.SalesEntryId) : GetTotalItemPrice(o.SalesEntryId, o.SEDate, srchtxt),
                        empcoste = (employeehourrate == true) ? (GetTotalEmpCost(o.SalesEntryId) * perhourcost) : GetTotalEmpCostwithrate(o.SalesEntryId),
                        mycoste = GetTotalEmpCost(o.SalesEntryId, technician) * perhourcost,
                        o.SECreatedDate,
                        o.PayMethod,
                        o.SaleType,
                        o.FromDate,
                        o.ToDate,
                        o.HireType,
                        o.SalesStatus,
                        o.SalesMan
                    }).Distinct().OrderBy(a => a.SEDate).ToList();
                    var data = vv.ToList();
                    recordsTotal = 100;


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
            }
            else
            {
                long[] MCArrays = { 20085, 20086, 20087 };
                string returnstr = "";
                foreach (var mcc in MCArrays)

                {

                    ddMC = mcc;
                    if ((location == "All" || location == "") && (technician == null || technician == 0))
                    {


                        if (srchtxt == "")
                        {
                            var v = (from a in db.SalesEntrys
                                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                                     from b in cust.DefaultIfEmpty()
                                     join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                                     join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                                     from d in emp.DefaultIfEmpty()
                                     join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                                     from f in paymeth.DefaultIfEmpty()
                                     join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                                     from g in mcs.DefaultIfEmpty()
                                     join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                                     from h in hir.DefaultIfEmpty()
                                     join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                                     from i in acc.DefaultIfEmpty()
                                     join j in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = sac }
                                     equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                                     from j in hir1.DefaultIfEmpty()
                                     join m in db.Employees on b.SalesPerson equals m.EmployeeId into empp
                                     from m in empp.DefaultIfEmpty()
                                     where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                                    (customer == 0 || a.Customer == customer) &&
                                    (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                                    (type == null || a.CustomerType == sEntry.CustomerType) &&
                                     (paymethod == null || a.PaymentMethod == paymethod) &&
                                     (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                                     (todates == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                                     (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                                    (htype == null || htype == null || h.HireType == htype) &&
                                    (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                                    (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)
                                   && (ddMC == 0 || ddMC == null || a.MaterialCenter == ddMC) &&

                                    (project == 0 || project == null || a.Project == project) &&
                                    (task == 0 || task == null || a.ProTask == task) &&
                                         (SalesMan == 0 || b.SalesPerson == SalesMan) && j.Credit > 0 &&

                                         (source == 0 || source == null || b.SourceOfLead == source)

                                     select new
                                     {

                                         a.SalesEntryId,
                                         a.SENo,
                                         a.BillNo,
                                         a.SEDate,
                                         SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : 0,
                                         SETaxAmount = (srchtxt == "") ? a.SETaxAmount : 0,
                                         Credit = (srchtxt == "") ? j.Credit : 0,
                                         Customer = b.CustomerName,
                                         b.CustomerID,
                                         TaxRegNo = i.TRN,
                                         EmpName = d.FirstName + " " + d.LastName,
                                         SalesMan = m.FirstName + " " + m.LastName,
                                         MCName = g.MCName,
                                         SEPaidAmount = (srchtxt == "") ? c.SEPaidAmount : 0,
                                         a.CustomerType,
                                         SEBalanceAmount = (srchtxt == "") ? (a.SEGrandTotal - c.SEPaidAmount) : 0,
                                         //for expense
                                         PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == a.BillNo).Select(x => x.Paying).Sum()),
                                         salesreturns = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 != "Credit Note").Select(x => x.SRSubTotal).Sum()),
                                         salesreturnsnote = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 == "Credit Note").Select(x => x.SRGrandTotal).Sum()),

                                         discountt = (decimal?)(from ayy in db.BillSundrys
                                                                join azz in db.SEBillSundrys on ayy.BillSundryId equals azz.BillSundry

                                                                where ayy.BSName == "DISCOUNT" && azz.SalesEntry == a.SalesEntryId
                                                                select new
                                                                {
                                                                    BsAmount = (azz.BsAmount == null) ? 0 : azz.BsAmount
                                                                }
                                                          ).Sum(x => x.BsAmount) ?? 0,
                                         //PaymentExpense = (decimal?)(from x in db.Payments

                                         //                            where (x.InvoiceNo == a.BillNo)
                                         //                                Expense = x.GrandTotal
                                         //JournalExpense = (decimal?)(from x in db.Journals

                                         //                            where (x.InvoiceNo == a.BillNo)
                                         //                                Expense = x.GrandTotal

                                         JournalExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == a.BillNo).Select(y => y.Paying).Sum()),

                                         //itemprice = (decimal?)(from se in db.SEItemss
                                         //                       where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
                                         //                           purprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * se.ItemQuantity) : ((seit.PurchasePrice * se.ItemQuantity) / seit.ConFactor)
                                         //           ).Sum(x => x.purprice) ?? 0,

                                         a.SECreatedDate,
                                         PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                                         SaleType = a.SaleType,
                                         FromDate = h.StartDate,
                                         ToDate = h.EndDate,
                                         HireType = h.HireType,
                                         a.SalesStatus

                                     });

                            var vv = v.AsEnumerable().Select(o => new
                            {
                                o.SalesEntryId,
                                o.SENo,
                                o.Credit,
                                o.BillNo,
                                o.SEDate,
                                o.CustomerID,
                                o.SEGrandTotal,
                                o.SETaxAmount,
                                o.Customer,
                                o.TaxRegNo,
                                o.discountt,
                                o.EmpName,
                                o.MCName,
                                o.SEPaidAmount,
                                o.CustomerType,
                                o.SEBalanceAmount,
                                NewExpense = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
                                         ((o.PaymentExpense != null && o.JournalExpense != null) ? o.PaymentExpense + o.JournalExpense : ((o.PaymentExpense == null && o.JournalExpense != null) ? o.JournalExpense : o.PaymentExpense))) : 0,
                                NewExpenselink = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? "" :
                                         ((o.PaymentExpense != null && o.JournalExpense != null) ? String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>")) + " " + String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : ((o.PaymentExpense == null && o.JournalExpense != null) ? String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>"))))) : "",

                                salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
                                salesrtnnote = (o.salesreturnsnote == null) ? 0 : o.salesreturnsnote,

                                //Calling Function To Get Total Item Price for each Sales Entry
                                itemprice = (isemirtech == true) ? getusedprice(o.SalesEntryId) : GetTotalItemPrice(o.SalesEntryId, o.SEDate, srchtxt),
                                empcoste = (employeehourrate == true) ? (GetTotalEmpCost(o.SalesEntryId) * perhourcost) : GetTotalEmpCostwithrate(o.SalesEntryId),
                                o.SECreatedDate,
                                o.PayMethod,
                                o.SaleType,
                                o.FromDate,
                                o.ToDate,
                                o.HireType,
                                o.SalesStatus,
                                o.SalesMan
                            }).Distinct().OrderBy(a => a.SEDate).ToList();
                            var data = vv.ToList();
                            recordsTotal = 100;


                            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                            string result = javaScriptSerializer.Serialize(data);
                            returnstr = returnstr + result;
                        }
                        else
                        {
                            var v = (from a in db.SalesEntrys
                                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                                     from b in cust.DefaultIfEmpty()
                                     join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                                     join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                                     from d in emp.DefaultIfEmpty()
                                     join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                                     from f in paymeth.DefaultIfEmpty()
                                     join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                                     from g in mcs.DefaultIfEmpty()
                                     join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                                     from h in hir.DefaultIfEmpty()
                                     join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                                     from i in acc.DefaultIfEmpty()
                                     join j in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = sac }
                                     equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                                     from j in hir1.DefaultIfEmpty()
                                     join k in db.SEItemss on a.SalesEntryId equals k.SalesEntry into seit
                                     from k in seit.DefaultIfEmpty()
                                     join l in db.Items on k.Item equals l.ItemID into itemm
                                     from l in itemm.DefaultIfEmpty()
                                     join m in db.Employees on b.SalesPerson equals m.EmployeeId into empp
                                     from m in empp.DefaultIfEmpty()
                                     let grandtotalitmsearch = (decimal)(from m in db.SEItemss
                                                                         join nn in db.Items on m.Item equals nn.ItemID
                                                                         join oo in db.SalesEntrys on m.SalesEntry equals oo.SalesEntryId
                                                                         where nn.ItemName.Contains(srchtxt)
                                                                         && oo.SalesEntryId == a.SalesEntryId

                                                                         select new
                                                                         {
                                                                             totalprice = m.ItemUnitPrice * m.ItemQuantity
                                                                         }).Sum(o => o.totalprice)

                                     where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                                    (customer == 0 || a.Customer == customer) &&
                                    (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                                    (type == null || a.CustomerType == sEntry.CustomerType) &&
                                     (paymethod == null || a.PaymentMethod == paymethod) &&
                                     (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                                     (todates == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                                     (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                                    (htype == null || htype == null || h.HireType == htype) &&
                                    (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                                    (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)
                                    && (ddMC == 0 || ddMC == null || a.MaterialCenter == ddMC) &&

                                    (project == 0 || project == null || a.Project == project) &&
                                    (task == 0 || task == null || a.ProTask == task) &&
                                       (srchtxt == "" || l.ItemName.Contains(srchtxt))
                                        &&
                                         (source == 0 || source == null || b.SourceOfLead == source)

                                     select new
                                     {

                                         a.SalesEntryId,
                                         a.SENo,
                                         a.BillNo,
                                         a.SEDate,
                                         SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : grandtotalitmsearch,
                                         SETaxAmount = (srchtxt == "") ? a.SETaxAmount : 0,
                                         Credit = (srchtxt == "") ? j.Credit : grandtotalitmsearch,
                                         Customer = b.CustomerName,
                                         b.CustomerID,
                                         TaxRegNo = i.TRN,
                                         EmpName = d.FirstName + " " + d.LastName,
                                         MCName = g.MCName,
                                         SalesMan = m.FirstName + " " + m.LastName,
                                         SEPaidAmount = (srchtxt == "") ? c.SEPaidAmount : 0,
                                         a.CustomerType,
                                         SEBalanceAmount = (srchtxt == "") ? (a.SEGrandTotal - c.SEPaidAmount) : 0,
                                         //for expense
                                         PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == a.BillNo).Select(x => x.Paying).Sum()),
                                         salesreturns = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 != "Credit Note").Select(x => x.SRGrandTotal).Sum()),
                                         salesreturnsnote = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 == "Credit Note").Select(x => x.SRGrandTotal).Sum()),

                                         discountt = (from ay in db.BillSundrys
                                                      join az in db.SEBillSundrys on ay.BillSundryId equals az.BillSundry

                                                      where ay.BSName == "DISCOUNT" && az.SalesEntry == a.SalesEntryId
                                                      select new
                                                      {
                                                          az.BsAmount
                                                      }
                                                          ).Sum(x => x.BsAmount) ?? 0,
                                         //PaymentExpense = (decimal?)(from x in db.Payments

                                         //                            where (x.InvoiceNo == a.BillNo)
                                         //                                Expense = x.GrandTotal
                                         //JournalExpense = (decimal?)(from x in db.Journals

                                         //                            where (x.InvoiceNo == a.BillNo)
                                         //                                Expense = x.GrandTotal

                                         JournalExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == a.BillNo).Select(y => y.Paying).Sum()),
                                         //itemprice = (decimal?)(from se in db.SEItemss
                                         //                       where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
                                         //                           purprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * se.ItemQuantity) : ((seit.PurchasePrice * se.ItemQuantity) / seit.ConFactor)
                                         //           ).Sum(x => x.purprice) ?? 0,

                                         a.SECreatedDate,
                                         PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                                         SaleType = a.SaleType,
                                         FromDate = h.StartDate,
                                         ToDate = h.EndDate,
                                         HireType = h.HireType,
                                         a.SalesStatus

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
                                         o.discountt,
                                         o.EmpName,
                                         o.MCName,
                                         o.SEPaidAmount,
                                         o.CustomerType,
                                         o.SEBalanceAmount,
                                         o.SalesMan,
                                         NewExpense = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
                                         ((o.PaymentExpense != null) ? o.PaymentExpense : o.JournalExpense)) : 0,
                                         NewExpenselink = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? "" :
                                         ((o.PaymentExpense != null && o.JournalExpense != null) ? String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>")) + " " + String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : ((o.PaymentExpense == null && o.JournalExpense != null) ? String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>"))))) : "",

                                         salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
                                         salesrtnnote = (o.salesreturnsnote == null) ? 0 : o.salesreturnsnote,

                                         //Calling Function To Get Total Item Price for each Sales Entry
                                         itemprice = (isemirtech == true) ? getusedprice(o.SalesEntryId) : GetTotalItemPrice(o.SalesEntryId, o.SEDate, srchtxt),
                                         empcoste = GetTotalEmpCost(o.SalesEntryId) * perhourcost,
                                         o.SECreatedDate,
                                         o.PayMethod,
                                         o.SaleType,
                                         o.FromDate,
                                         o.ToDate,
                                         o.CustomerID,
                                         o.HireType,
                                         o.SalesStatus,
                                     }).OrderBy(a => a.SEDate).ThenBy(a => a.SECreatedDate);
                            var data = v.ToList();
                            recordsTotal = v.Count();


                            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                            string result = javaScriptSerializer.Serialize(new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                            returnstr = returnstr + result;
                        }
                    }
                    else
                    {

                        var pid = (from x in db.ProTasks
                                   where (location == "" || x.Location == location && (

                                     (fromdate == "" || EF.Functions.DateDiffDay(x.CreatedDate, fdate) <= 0) &&
                                 (todates == "" || EF.Functions.DateDiffDay(x.CreatedDate, tdate) >= 0)



                                   )
                                   ) &&
                                   (technician == null || technician == 0 || (from a in db.servicereports
                                                                              join
                                                   b in db.servicereportmembers on a.servicereportid equals b.servicereportid
                                                                              where a.protaskid == x.ProTaskId && b.employeeid == technician
                                                                              select new { a.protaskid }).Any())






                                   select new
                                   {
                                       pids = x.ProTaskId
                                   }).Select(o => o.pids).ToList().ToArray();

                        var salesids = (from x in db.SalesEntrys
                                        join y in db.additionaltasks on x.SalesEntryId equals y.salesentryid into yy
                                        from y in yy.DefaultIfEmpty()
                                        where (pid.Contains(y.taskid) || pid.Contains((long)x.ProTask))

                                        && (technician == null || technician == 0 || (
                                     (fromdate == "" || EF.Functions.DateDiffDay(x.SEDate, fdate) <= 0) &&
                                 (todates == "" || EF.Functions.DateDiffDay(x.SEDate, tdate) >= 0)
                                 ))
                                        select new
                                        {
                                            x.SalesEntryId
                                        }).Distinct().Select(o => o.SalesEntryId).ToList().ToArray();
                        var v = (from a in db.SalesEntrys

                                 join b in db.Customers on a.Customer equals b.CustomerID into cust
                                 from b in cust.DefaultIfEmpty()
                                 join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                                 join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                                 from d in emp.DefaultIfEmpty()
                                 join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                                 from f in paymeth.DefaultIfEmpty()
                                 join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                                 from g in mcs.DefaultIfEmpty()
                                 join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                                 equals new { h1 = h.Reference, h2 = h.Section } into hir
                                 from h in hir.DefaultIfEmpty()
                                 join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                                 from i in acc.DefaultIfEmpty()
                                 join j in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = sac }
                                 equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                                 from j in hir1.DefaultIfEmpty()
                                 join m in db.Employees on b.SalesPerson equals m.EmployeeId into empp
                                 from m in empp.DefaultIfEmpty()
                                 where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                                (customer == 0 || a.Customer == customer) &&
                                (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                                (type == null || a.CustomerType == sEntry.CustomerType) &&
                                 (paymethod == null || a.PaymentMethod == paymethod) &&
                                 salesids.Contains(a.SalesEntryId) &&
                                 (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                                (htype == null || htype == null || h.HireType == htype) &&
                                (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                                (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)
                               && (ddMC == 0 || ddMC == null || a.MaterialCenter == ddMC) &&

                                (project == 0 || project == null || a.Project == project) &&
                                (task == 0 || task == null || a.ProTask == task) &&
                                     (SalesMan == 0 || b.SalesPerson == SalesMan) && j.Credit > 0 &&

                                     (source == 0 || source == null || b.SourceOfLead == source)

                                 select new
                                 {

                                     a.SalesEntryId,
                                     a.SENo,
                                     a.BillNo,
                                     a.SEDate,
                                     SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : 0,
                                     SETaxAmount = (srchtxt == "") ? a.SETaxAmount : 0,
                                     Credit = (srchtxt == "") ? j.Credit : 0,
                                     Customer = b.CustomerName,
                                     b.CustomerID,
                                     TaxRegNo = i.TRN,
                                     EmpName = d.FirstName + " " + d.LastName,
                                     SalesMan = m.FirstName + " " + m.LastName,
                                     MCName = g.MCName,
                                     SEPaidAmount = (srchtxt == "") ? c.SEPaidAmount : 0,
                                     a.CustomerType,
                                     SEBalanceAmount = (srchtxt == "") ? (a.SEGrandTotal - c.SEPaidAmount) : 0,
                                     //for expense
                                     PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == a.BillNo).Select(x => x.Paying).Sum()),
                                     salesreturns = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 != "Credit Note").Select(x => x.SRSubTotal).Sum()),
                                     salesreturnsnote = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 == "Credit Note").Select(x => x.SRGrandTotal).Sum()),

                                     discountt = (decimal?)(from ayy in db.BillSundrys
                                                            join azz in db.SEBillSundrys on ayy.BillSundryId equals azz.BillSundry

                                                            where ayy.BSName == "DISCOUNT" && azz.SalesEntry == a.SalesEntryId
                                                            select new
                                                            {
                                                                BsAmount = (azz.BsAmount == null) ? 0 : azz.BsAmount
                                                            }
                                                      ).Sum(x => x.BsAmount) ?? 0,
                                     //PaymentExpense = (decimal?)(from x in db.Payments

                                     //                            where (x.InvoiceNo == a.BillNo)
                                     //                                Expense = x.GrandTotal
                                     //JournalExpense = (decimal?)(from x in db.Journals

                                     //                            where (x.InvoiceNo == a.BillNo)
                                     //                                Expense = x.GrandTotal

                                     JournalExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == a.BillNo).Select(y => y.Paying).Sum()),

                                     //itemprice = (decimal?)(from se in db.SEItemss
                                     //                       where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
                                     //                           purprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * se.ItemQuantity) : ((seit.PurchasePrice * se.ItemQuantity) / seit.ConFactor)
                                     //           ).Sum(x => x.purprice) ?? 0,

                                     a.SECreatedDate,
                                     PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                                     SaleType = a.SaleType,
                                     FromDate = h.StartDate,
                                     ToDate = h.EndDate,
                                     HireType = h.HireType,
                                     a.SalesStatus

                                 });

                        var vv = v.AsEnumerable().Select(o => new
                        {
                            o.SalesEntryId,
                            o.SENo,
                            o.Credit,
                            o.BillNo,
                            o.SEDate,
                            o.CustomerID,
                            o.SEGrandTotal,
                            o.SETaxAmount,
                            o.Customer,
                            o.TaxRegNo,
                            o.discountt,
                            o.EmpName,
                            o.MCName,
                            o.SEPaidAmount,
                            o.CustomerType,
                            o.SEBalanceAmount,
                            NewExpense = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
                                     ((o.PaymentExpense != null && o.JournalExpense != null) ? o.PaymentExpense + o.JournalExpense : ((o.PaymentExpense == null && o.JournalExpense != null) ? o.JournalExpense : o.PaymentExpense))) : 0,
                            NewExpenselink = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? "" :
                                     ((o.PaymentExpense != null && o.JournalExpense != null) ? String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>")) + " " + String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : ((o.PaymentExpense == null && o.JournalExpense != null) ? String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>"))))) : "",

                            salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
                            salesrtnnote = (o.salesreturnsnote == null) ? 0 : o.salesreturnsnote,

                            //Calling Function To Get Total Item Price for each Sales Entry
                            itemprice = (isemirtech == true) ? getusedprice(o.SalesEntryId) : GetTotalItemPrice(o.SalesEntryId, o.SEDate, srchtxt),
                            empcoste = (employeehourrate == true) ? (GetTotalEmpCost(o.SalesEntryId) * perhourcost) : GetTotalEmpCostwithrate(o.SalesEntryId),
                            mycoste = GetTotalEmpCost(o.SalesEntryId, technician) * perhourcost,
                            o.SECreatedDate,
                            o.PayMethod,
                            o.SaleType,
                            o.FromDate,
                            o.ToDate,
                            o.HireType,
                            o.SalesStatus,
                            o.SalesMan
                        }).Distinct().OrderBy(a => a.SEDate).ToList();
                        var data = vv.ToList();
                        recordsTotal = 100;


                        JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                        javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                        string result = javaScriptSerializer.Serialize(new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                        returnstr = returnstr + result;




                    }

                }
                var results = new ContentResult
                {
                    Content = returnstr,
                    ContentType = "application/json"
                };
                return results;
            }

        }
        public class salesreturnmodel
        {
            public long itid { get; set; }
            public long saleid { get; set; }
            public decimal qty { get; set; }
            public string billno { get; set; }

        }
        public class salesmanarr
            {
            public long salesman { get; set; }
            public decimal target { get; set; }
    }

        [HttpPost]



        public ActionResult GetAllSaleprofitgroup(string completed, string seno, double perhourcost, long? paymethod, long? SalesMan, long? customer, long? SalesExecutive, string fromdate, string todates, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate, long? project, long? task, string srchtxt, bool employeehourrate, long? source, string location, long? technician, long? sourcelead, bool cached, long? cusage, bool nostocktransfer)
        {
            db.SetCommandTimeOut(60 * 60);
            //QUICK NET COMPUTERS
            var isemirtech = db.companys.Any(o => o.CPName.Contains("EMIRTECH TECHNOLOGY"));
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
            if (satype != "")
            {
                St = (satype == "1") ? SaleType.Sale : SaleType.Hire;
            };

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todates != "")
            {
                tdate = DateTime.Parse(todates, new CultureInfo("en-GB"));
            }

            DateTime? hfrmdate = null;
            DateTime? htodate = null;
            if (!string.IsNullOrEmpty(hfdate))
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

            var allshworoom = db.MCs.Where(o => o.MCId == ddMC).Select(o => o.MCName).FirstOrDefault();

            if (allshworoom != "ALL SHOWROOMS")
            {
                if ((location == "All" || location == "") && (technician == null || technician == 0))
                {




                    if (srchtxt == "")
                    {
                        IEnumerable<SalesEntry> salesentrys = (from a in db.SalesEntrys

                                                               where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                                                    (customer == 0 || a.Customer == customer) &&
                                                    (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                                                    (type == null || a.CustomerType == sEntry.CustomerType) &&
                                                     (paymethod == null || a.PaymentMethod == paymethod) &&
                                                     (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                                                     (todates == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                                                     (string.IsNullOrEmpty(satype) || a.SaleType == St)

                                                   && (ddMC == 0 || ddMC == null || a.MaterialCenter == ddMC) &&

                                                    (project == 0 || project == null || a.Project == project) &&
                                                    (task == 0 || task == null || a.ProTask == task)

                                                               select a


                                           ).AsEnumerable();
                        var salereturnids = (from a in db.SalesReturns
                                             join b in db.SRItemss on a.SalesReturnId equals b.SalesReturnId
                                             join c in salesentrys on a.SalesEntryId equals c.SalesEntryId
                                             group new { b.Item, b.ItemQuantity, c.SalesEntryId, c.BillNo } by new { c.SalesEntryId, b.Item } into grp
                                             select new salesreturnmodel
                                             {
                                                 itid = grp.FirstOrDefault().Item,
                                                 saleid = grp.FirstOrDefault().SalesEntryId,
                                                 qty = (grp.Count() == 1) ? grp.Sum(o => o.ItemQuantity) : grp.Average(o => o.ItemQuantity),
                                                 billno = grp.FirstOrDefault().BillNo,
                                             }
                           ).ToList();

                        long comm = 0;

                        var v = (from a in db.SalesEntrys
                                 join b in db.Customers on a.Customer equals b.CustomerID into cust
                                 from b in cust.DefaultIfEmpty()

                                 join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry

                                 join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                                 from d in emp.DefaultIfEmpty()
                                 join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                                 from f in paymeth.DefaultIfEmpty()
                                 join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                                 from g in mcs.DefaultIfEmpty()
                                 join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                                 equals new { h1 = h.Reference, h2 = h.Section } into hir
                                 from h in hir.DefaultIfEmpty()
                                 join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                                 from i in acc.DefaultIfEmpty()
                                 join j in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = sac }
                                 equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                                 from j in hir1.DefaultIfEmpty()
                                 join m in db.Employees on b.SalesPerson equals m.EmployeeId into empp
                                 from m in empp.DefaultIfEmpty()
                                 let leads = (
                                 from xx in db.Customers
                                 join cc in db.leadcustomerrelation on xx.CustomerID equals cc.customerid
                                 where cc.customerid == b.CustomerID
                                 select new
                                 {
                                     xx.SourceOfLead
                                 }
                                 ).FirstOrDefault()
                                 where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                                (customer == 0 || a.Customer == customer) &&
                                (cusage == 0 || cusage == null || (cusage == 1 && b.CreatedDate >= fdate && b.CreatedDate <= tdate) || (cusage == 2 && b.CreatedDate < fdate)) &&
                                (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                                (type == null || a.CustomerType == sEntry.CustomerType) &&
                                 (paymethod == null || a.PaymentMethod == paymethod) &&
                                 (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                                 (todates == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                                 (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                                (htype == null || htype == null || h.HireType == htype) &&
                                (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                                (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)
                               && (ddMC == 0 || ddMC == null || a.MaterialCenter == ddMC) &&

                                (project == 0 || project == null || a.Project == project) &&
                                (task == 0 || task == null || a.ProTask == task) &&
                                     (SalesMan == 0 || b.SalesPerson == SalesMan) && j.Credit > 0 &&

                                     (source == 0 || source == null || b.SourceOfLead == source) &&
(sourcelead == 0 || sourcelead == null | (leads != null && leads.SourceOfLead == sourcelead))

                                 select new
                                 {

                                     a.SalesEntryId,
                                     a.SENo,
                                     a.BillNo,
                                     mtcost = (a.materialcost == null) ? 0 : a.materialcost,
                                     a.SEDate,
                                     SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : 0,
                                     SETaxAmount = (srchtxt == "") ? a.SETaxAmount : 0,
                                     Credit = (srchtxt == "") ? j.Credit : 0,
                                     Customer = b.CustomerName,
                                     b.CustomerID,
                                     TaxRegNo = i.TRN,
                                     EmpName = d.FirstName + " " + d.LastName,
                                     a.SECashier,
                                     SalesMan = m.FirstName + " " + m.LastName,
                                     MCName = g.MCName,
                                     SEPaidAmount = (srchtxt == "") ? c.SEPaidAmount : 0,
                                     a.CustomerType,
                                     SEBalanceAmount = (srchtxt == "") ? (a.SEGrandTotal - c.SEPaidAmount) : 0,
                                     //for expense
                                     PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == a.BillNo).Select(x => x.Paying).Sum()),
                                     salesreturns = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 != "Credit Note").Select(x => x.SRSubTotal).Sum()),
                                     salesreturnsnote = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 == "Credit Note").Select(x => x.SRGrandTotal).Sum()),
                                     completed = (from comp in db.salesmanprofittargets
                                                  where
                                                 comp.salesentryid == a.SalesEntryId &&
                                                 comp.completed == comm
                                                  select comp
                                                 ).OrderByDescending(o => o.targetprofitid).FirstOrDefault(),

                                     contribute = (from comp in db.salesmanprofittargets
                                                   where
                                                  comp.salesentryid == a.SalesEntryId &&
                                                  comp.employeeid == a.SECashier
                                                   select comp

                                                ).OrderByDescending(o => o.targetprofitid).FirstOrDefault(),

                                     target = db.Employees.Where(o => o.EmployeeId == a.SECashier).Select(o => o.OtherIdNo).FirstOrDefault(),


                                     discountt = (decimal?)(from ayy in db.BillSundrys
                                                            join azz in db.SEBillSundrys on ayy.BillSundryId equals azz.BillSundry

                                                            where ayy.BSName == "DISCOUNT" && azz.SalesEntry == a.SalesEntryId
                                                            select new
                                                            {
                                                                BsAmount = (azz.BsAmount == null) ? 0 : azz.BsAmount
                                                            }
                                                      ).Sum(x => x.BsAmount) ?? 0,
                                     roundoffmin = (decimal?)(
                                        from k in db.SEBillSundrys
                                        where k.SalesEntry == a.SalesEntryId &&
                                       k.BillSundry == 2
                                        select new
                                        {
                                            k.BsAmount
                                        }).Select(o => o.BsAmount).FirstOrDefault() ?? 0,
                                     roundoffplus = (decimal?)(from k in db.SEBillSundrys
                                                               where k.SalesEntry == a.SalesEntryId &&
                                                              k.BillSundry == 1
                                                               select new
                                                               {
                                                                   k.BsAmount
                                                               }).Select(o => o.BsAmount).FirstOrDefault() ?? 0,

                                     //PaymentExpense = (decimal?)(from x in db.Payments

                                     //                            where (x.InvoiceNo == a.BillNo)
                                     //                                Expense = x.GrandTotal
                                     //JournalExpense = (decimal?)(from x in db.Journals

                                     //                            where (x.InvoiceNo == a.BillNo)
                                     //                                Expense = x.GrandTotal

                                     JournalExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == a.BillNo).Select(y => y.Paying).Sum()),

                                     //itemprice = (decimal?)(from se in db.SEItemss
                                     //                       where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
                                     //                           purprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * se.ItemQuantity) : ((seit.PurchasePrice * se.ItemQuantity) / seit.ConFactor)
                                     //           ).Sum(x => x.purprice) ?? 0,

                                     a.SECreatedDate,
                                     PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                                     SaleType = a.SaleType,
                                     FromDate = h.StartDate,
                                     ToDate = h.EndDate,
                                     HireType = h.HireType,
                                     a.SalesStatus,


                                 });


                        var vv = v.ToList().Where(o =>

                            (completed == "" || completed == null || (completed == "0" && ((o.completed == null) ? o.BillNo == "-1" : o.completed.completed == 0)) || (completed == "1" && ((o.completed == null) ? 1 == 1 : o.completed.completed == 1)))


                        ).Select(o => new
                        {
                            o.SalesEntryId,
                            o.SENo,
                            o.mtcost,
                            o.Credit,
                            o.BillNo,
                            target = (o.target == null || o.target == "") ? 0 : Convert.ToDecimal(o.target),
                            o.SEDate,
                            o.CustomerID,
                            o.SEGrandTotal,
                            o.SETaxAmount,
                            o.completed,
                            o.contribute,
                            o.Customer,
                            o.TaxRegNo,
                            o.discountt,
                            o.roundoffmin,
                            o.roundoffplus,
                            SalesPerson = o.SECashier,
                            o.EmpName,
                            o.MCName,
                            o.SEPaidAmount,
                            o.CustomerType,
                            o.SEBalanceAmount,
                            NewExpense = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
                                     ((o.PaymentExpense != null && o.JournalExpense != null) ? o.PaymentExpense + o.JournalExpense : ((o.PaymentExpense == null && o.JournalExpense != null) ? o.JournalExpense : o.PaymentExpense))) : 0,
                            NewExpenselink = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? "" :
                                     ((o.PaymentExpense != null && o.JournalExpense != null) ? String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>")) + " " + String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : ((o.PaymentExpense == null && o.JournalExpense != null) ? String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>"))))) : "",

                            salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
                            salesrtnnote = (o.salesreturnsnote == null) ? 0 : o.salesreturnsnote,

                            //Calling Function To Get Total Item Price for each Sales Entry
                            itemprice = (isemirtech == true) ? getusedprice(o.SalesEntryId) : (cached != true) ? GetTotalItemPricenewnew(o.SalesEntryId, o.SEDate, srchtxt, salesentrys, salereturnids, nostocktransfer) : getcachedprice(o.mtcost),
                            empcoste = (isemirtech == true) ? ((employeehourrate == true) ? (GetTotalEmpCost(o.SalesEntryId) * perhourcost) : GetTotalEmpCostwithrate(o.SalesEntryId)) : 0,

                            o.SECreatedDate,
                            o.PayMethod,
                            o.SaleType,
                            o.FromDate,
                            o.ToDate,
                            o.HireType,
                            o.SalesStatus,
                            o.SalesMan
                        }).Distinct().OrderBy(a => a.SEDate).ToList();
                        var data = vv.ToList();
                        foreach (var d in data)
                        {
                            var se = db.SalesEntrys.Find(d.SalesEntryId);
                            se.materialcost = (decimal)d.itemprice["itemprice"];
                            db.Entry(se).State = EntityState.Modified;
                            db.SaveChanges();

                        }

                        recordsTotal = 100;


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
                    else
                    {
                        var v = (from a in db.SalesEntrys
                                 join b in db.Customers on a.Customer equals b.CustomerID into cust
                                 from b in cust.DefaultIfEmpty()
                                 join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                                 join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                                 from d in emp.DefaultIfEmpty()
                                 join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                                 from f in paymeth.DefaultIfEmpty()
                                 join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                                 from g in mcs.DefaultIfEmpty()
                                 join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                                 equals new { h1 = h.Reference, h2 = h.Section } into hir
                                 from h in hir.DefaultIfEmpty()
                                 join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                                 from i in acc.DefaultIfEmpty()
                                 join j in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = sac }
                                 equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                                 from j in hir1.DefaultIfEmpty()
                                 join k in db.SEItemss on a.SalesEntryId equals k.SalesEntry into seit
                                 from k in seit.DefaultIfEmpty()
                                 join l in db.Items on k.Item equals l.ItemID into itemm
                                 from l in itemm.DefaultIfEmpty()
                                 join m in db.Employees on b.SalesPerson equals m.EmployeeId into empp
                                 from m in empp.DefaultIfEmpty()
                                 let grandtotalitmsearch = (decimal)(from m in db.SEItemss
                                                                     join nn in db.Items on m.Item equals nn.ItemID
                                                                     join oo in db.SalesEntrys on m.SalesEntry equals oo.SalesEntryId
                                                                     where nn.ItemName.Contains(srchtxt)
                                                                     && oo.SalesEntryId == a.SalesEntryId

                                                                     select new
                                                                     {
                                                                         totalprice = m.ItemUnitPrice * m.ItemQuantity
                                                                     }).Sum(o => o.totalprice)

                                 where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                                (customer == 0 || a.Customer == customer) &&
                                (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                                (type == null || a.CustomerType == sEntry.CustomerType) &&
                                 (paymethod == null || a.PaymentMethod == paymethod) &&
                                 (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                                 (todates == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                                 (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                                (htype == null || htype == null || h.HireType == htype) &&
                                (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                                (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)
                                && (ddMC == 0 || ddMC == null || a.MaterialCenter == ddMC) &&

                                (project == 0 || project == null || a.Project == project) &&
                                (task == 0 || task == null || a.ProTask == task) &&
                                   (srchtxt == "" || l.ItemName.Contains(srchtxt))
                                    &&
                                     (source == 0 || source == null || b.SourceOfLead == source)

                                 select new
                                 {

                                     a.SalesEntryId,
                                     a.SENo,
                                     a.BillNo,
                                     a.SEDate,
                                     a.SECashier,
                                     SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : grandtotalitmsearch,
                                     SETaxAmount = (srchtxt == "") ? a.SETaxAmount : 0,
                                     Credit = (srchtxt == "") ? j.Credit : grandtotalitmsearch,
                                     Customer = b.CustomerName,
                                     b.CustomerID,
                                     TaxRegNo = i.TRN,
                                     EmpName = d.FirstName + " " + d.LastName,
                                     MCName = g.MCName,
                                     SalesMan = m.FirstName + " " + m.LastName,
                                     SEPaidAmount = (srchtxt == "") ? c.SEPaidAmount : 0,
                                     a.CustomerType,
                                     SEBalanceAmount = (srchtxt == "") ? (a.SEGrandTotal - c.SEPaidAmount) : 0,
                                     //for expense
                                     PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == a.BillNo).Select(x => x.Paying).Sum()),
                                     salesreturns = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 != "Credit Note").Select(x => x.SRGrandTotal).Sum()),
                                     salesreturnsnote = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 == "Credit Note").Select(x => x.SRGrandTotal).Sum()),
                                     completed = (from comp in db.salesmanprofittargets
                                                  where
                                                 comp.salesentryid == a.SalesEntryId &&
                                                 comp.completed == 0
                                                  select new
                                                  {
                                                      comp.completed,
                                                      comp.contributionpercentage
                                                  }
                                                ).FirstOrDefault(),
                                     contribute = (from comp in db.salesmanprofittargets
                                                   where
                                                  comp.salesentryid == a.SalesEntryId &&
                                                  comp.employeeid == a.SECashier
                                                   select new
                                                   {

                                                       comp.contributionpercentage,
                                                       comp.completed,
                                                   }
                                                ).FirstOrDefault(),
                                     target = db.Employees.Where(o => o.EmployeeId == a.SECashier).Select(o => o.OtherIdNo).FirstOrDefault(),

                                     discountt = (from ay in db.BillSundrys
                                                  join az in db.SEBillSundrys on ay.BillSundryId equals az.BillSundry

                                                  where ay.BSName == "DISCOUNT" && az.SalesEntry == a.SalesEntryId
                                                  select new
                                                  {
                                                      az.BsAmount
                                                  }
                                                      ).Sum(x => x.BsAmount) ?? 0,
                                     //PaymentExpense = (decimal?)(from x in db.Payments

                                     //                            where (x.InvoiceNo == a.BillNo)
                                     //                                Expense = x.GrandTotal
                                     //JournalExpense = (decimal?)(from x in db.Journals

                                     //                            where (x.InvoiceNo == a.BillNo)
                                     //                                Expense = x.GrandTotal

                                     JournalExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == a.BillNo).Select(y => y.Paying).Sum()),
                                     //itemprice = (decimal?)(from se in db.SEItemss
                                     //                       where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
                                     //                           purprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * se.ItemQuantity) : ((seit.PurchasePrice * se.ItemQuantity) / seit.ConFactor)
                                     //           ).Sum(x => x.purprice) ?? 0,

                                     a.SECreatedDate,
                                     PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                                     SaleType = a.SaleType,
                                     FromDate = h.StartDate,
                                     ToDate = h.EndDate,
                                     HireType = h.HireType,
                                     a.SalesStatus

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
                                     o.discountt,
                                     o.EmpName,
                                     o.MCName,
                                     o.SEPaidAmount,
                                     o.CustomerType,
                                     o.SEBalanceAmount,
                                     o.SalesMan,
                                     NewExpense = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
                                     ((o.PaymentExpense != null) ? o.PaymentExpense : o.JournalExpense)) : 0,
                                     NewExpenselink = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? "" :
                                     ((o.PaymentExpense != null && o.JournalExpense != null) ? String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>")) + " " + String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : ((o.PaymentExpense == null && o.JournalExpense != null) ? String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>"))))) : "",

                                     salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
                                     salesrtnnote = (o.salesreturnsnote == null) ? 0 : o.salesreturnsnote,
                                     target = (o.target == null || o.target == "") ? 0 : Convert.ToDecimal(o.target),

                                     //Calling Function To Get Total Item Price for each Sales Entry
                                     itemprice = (isemirtech == true) ? getusedprice(o.SalesEntryId) : GetTotalItemPrice(o.SalesEntryId, o.SEDate, srchtxt),
                                     empcoste = GetTotalEmpCost(o.SalesEntryId) * perhourcost,
                                     o.SECreatedDate,
                                     o.PayMethod,
                                     o.completed,
                                     o.contribute,
                                     o.SaleType,
                                     o.FromDate,
                                     o.ToDate,
                                     o.CustomerID,
                                     o.HireType,
                                     o.SalesStatus,
                                     SalesPerson = o.SECashier,
                                 }).OrderBy(a => a.SEDate).ThenBy(a => a.SECreatedDate);
                        var data = v.ToList();
                        var salesmans = data.ToList().OrderBy(o => o.SalesPerson);

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
                }
                else
                {

                    var pid = (from x in db.ProTasks
                               where (location == "" || x.Location == location && (

                                 (fromdate == "" || EF.Functions.DateDiffDay(x.CreatedDate, fdate) <= 0) &&
                             (todates == "" || EF.Functions.DateDiffDay(x.CreatedDate, tdate) >= 0)



                               )
                               ) &&
                               (technician == null || technician == 0 || (from a in db.servicereports
                                                                          join
                                               b in db.servicereportmembers on a.servicereportid equals b.servicereportid
                                                                          where a.protaskid == x.ProTaskId && b.employeeid == technician
                                                                          select new { a.protaskid }).Any())






                               select new
                               {
                                   pids = x.ProTaskId
                               }).Select(o => o.pids).ToList().ToArray();

                    var salesids = (from x in db.SalesEntrys
                                    join y in db.additionaltasks on x.SalesEntryId equals y.salesentryid into yy
                                    from y in yy.DefaultIfEmpty()
                                    where (pid.Contains(y.taskid) || pid.Contains((long)x.ProTask))

                                    && (technician == null || technician == 0 || (
                                 (fromdate == "" || EF.Functions.DateDiffDay(x.SEDate, fdate) <= 0) &&
                             (todates == "" || EF.Functions.DateDiffDay(x.SEDate, tdate) >= 0)
                             ))
                                    select new
                                    {
                                        x.SalesEntryId
                                    }).Distinct().Select(o => o.SalesEntryId).ToList().ToArray();
                    var v = (from a in db.SalesEntrys

                             join b in db.Customers on a.Customer equals b.CustomerID into cust
                             from b in cust.DefaultIfEmpty()
                             join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                             join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                             from d in emp.DefaultIfEmpty()
                             join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                             from f in paymeth.DefaultIfEmpty()
                             join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                             from g in mcs.DefaultIfEmpty()
                             join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                             equals new { h1 = h.Reference, h2 = h.Section } into hir
                             from h in hir.DefaultIfEmpty()
                             join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                             from i in acc.DefaultIfEmpty()
                             join j in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = sac }
                             equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                             from j in hir1.DefaultIfEmpty()
                             join m in db.Employees on b.SalesPerson equals m.EmployeeId into empp
                             from m in empp.DefaultIfEmpty()
                             where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                            (customer == 0 || a.Customer == customer) &&
                            (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                            (type == null || a.CustomerType == sEntry.CustomerType) &&
                             (paymethod == null || a.PaymentMethod == paymethod) &&
                             salesids.Contains(a.SalesEntryId) &&
                             (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                            (htype == null || htype == null || h.HireType == htype) &&
                            (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                            (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)
                           && (ddMC == 0 || ddMC == null || a.MaterialCenter == ddMC) &&

                            (project == 0 || project == null || a.Project == project) &&
                            (task == 0 || task == null || a.ProTask == task) &&
                                 (SalesMan == 0 || b.SalesPerson == SalesMan) && j.Credit > 0 &&

                                 (source == 0 || source == null || b.SourceOfLead == source)

                             select new
                             {

                                 a.SalesEntryId,
                                 a.SENo,
                                 a.BillNo,
                                 a.SEDate,
                                 SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : 0,
                                 SETaxAmount = (srchtxt == "") ? a.SETaxAmount : 0,
                                 Credit = (srchtxt == "") ? j.Credit : 0,
                                 Customer = b.CustomerName,
                                 b.CustomerID,
                                 TaxRegNo = i.TRN,
                                 EmpName = d.FirstName + " " + d.LastName,
                                 SalesMan = m.FirstName + " " + m.LastName,
                                 MCName = g.MCName,
                                 SEPaidAmount = (srchtxt == "") ? c.SEPaidAmount : 0,
                                 a.CustomerType,
                                 SEBalanceAmount = (srchtxt == "") ? (a.SEGrandTotal - c.SEPaidAmount) : 0,
                                 //for expense
                                 PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == a.BillNo).Select(x => x.Paying).Sum()),
                                 salesreturns = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 != "Credit Note").Select(x => x.SRSubTotal).Sum()),
                                 salesreturnsnote = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 == "Credit Note").Select(x => x.SRGrandTotal).Sum()),

                                 discountt = (decimal?)(from ayy in db.BillSundrys
                                                        join azz in db.SEBillSundrys on ayy.BillSundryId equals azz.BillSundry

                                                        where ayy.BSName == "DISCOUNT" && azz.SalesEntry == a.SalesEntryId
                                                        select new
                                                        {
                                                            BsAmount = (azz.BsAmount == null) ? 0 : azz.BsAmount
                                                        }
                                                  ).Sum(x => x.BsAmount) ?? 0,
                                 //PaymentExpense = (decimal?)(from x in db.Payments

                                 //                            where (x.InvoiceNo == a.BillNo)
                                 //                                Expense = x.GrandTotal
                                 //JournalExpense = (decimal?)(from x in db.Journals

                                 //                            where (x.InvoiceNo == a.BillNo)
                                 //                                Expense = x.GrandTotal

                                 JournalExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == a.BillNo).Select(y => y.Paying).Sum()),

                                 //itemprice = (decimal?)(from se in db.SEItemss
                                 //                       where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
                                 //                           purprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * se.ItemQuantity) : ((seit.PurchasePrice * se.ItemQuantity) / seit.ConFactor)
                                 //           ).Sum(x => x.purprice) ?? 0,

                                 a.SECreatedDate,
                                 PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                                 SaleType = a.SaleType,
                                 FromDate = h.StartDate,
                                 ToDate = h.EndDate,
                                 HireType = h.HireType,
                                 a.SalesStatus

                             });

                    var vv = v.AsEnumerable().Select(o => new
                    {
                        o.SalesEntryId,
                        o.SENo,
                        o.Credit,
                        o.BillNo,
                        o.SEDate,
                        o.CustomerID,
                        o.SEGrandTotal,
                        o.SETaxAmount,
                        o.Customer,
                        o.TaxRegNo,
                        o.discountt,
                        o.EmpName,
                        o.MCName,
                        o.SEPaidAmount,
                        o.CustomerType,
                        o.SEBalanceAmount,
                        NewExpense = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
                                 ((o.PaymentExpense != null && o.JournalExpense != null) ? o.PaymentExpense + o.JournalExpense : ((o.PaymentExpense == null && o.JournalExpense != null) ? o.JournalExpense : o.PaymentExpense))) : 0,
                        NewExpenselink = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? "" :
                                 ((o.PaymentExpense != null && o.JournalExpense != null) ? String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>")) + " " + String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : ((o.PaymentExpense == null && o.JournalExpense != null) ? String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>"))))) : "",

                        salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
                        salesrtnnote = (o.salesreturnsnote == null) ? 0 : o.salesreturnsnote,

                        //Calling Function To Get Total Item Price for each Sales Entry
                        itemprice = (isemirtech == true) ? getusedprice(o.SalesEntryId) : GetTotalItemPrice(o.SalesEntryId, o.SEDate, srchtxt),
                        empcoste = (employeehourrate == true) ? (GetTotalEmpCost(o.SalesEntryId) * perhourcost) : GetTotalEmpCostwithrate(o.SalesEntryId),
                        mycoste = GetTotalEmpCost(o.SalesEntryId, technician) * perhourcost,
                        o.SECreatedDate,
                        o.PayMethod,
                        o.SaleType,
                        o.FromDate,
                        o.ToDate,
                        o.HireType,
                        o.SalesStatus,
                        o.SalesMan
                    }).Distinct().OrderBy(a => a.SEDate).ToList();
                    var data = vv.ToList();
                    recordsTotal = 100;


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
            }
            else
            {
                long[] MCArrays = { 20085, 20086, 20087 };
                string returnstr = "";
                foreach (var mcc in MCArrays)

                {

                    ddMC = mcc;
                    if ((location == "All" || location == "") && (technician == null || technician == 0))
                    {


                        if (srchtxt == "")
                        {
                            var v = (from a in db.SalesEntrys
                                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                                     from b in cust.DefaultIfEmpty()
                                     join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                                     join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                                     from d in emp.DefaultIfEmpty()
                                     join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                                     from f in paymeth.DefaultIfEmpty()
                                     join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                                     from g in mcs.DefaultIfEmpty()
                                     join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                                     from h in hir.DefaultIfEmpty()
                                     join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                                     from i in acc.DefaultIfEmpty()
                                     join j in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = sac }
                                     equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                                     from j in hir1.DefaultIfEmpty()
                                     join m in db.Employees on b.SalesPerson equals m.EmployeeId into empp
                                     from m in empp.DefaultIfEmpty()
                                     where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                                    (customer == 0 || a.Customer == customer) &&
                                    (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                                    (type == null || a.CustomerType == sEntry.CustomerType) &&
                                     (paymethod == null || a.PaymentMethod == paymethod) &&
                                     (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                                     (todates == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                                     (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                                    (htype == null || htype == null || h.HireType == htype) &&
                                    (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                                    (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)
                                   && (ddMC == 0 || ddMC == null || a.MaterialCenter == ddMC) &&

                                    (project == 0 || project == null || a.Project == project) &&
                                    (task == 0 || task == null || a.ProTask == task) &&
                                         (SalesMan == 0 || b.SalesPerson == SalesMan) && j.Credit > 0 &&

                                         (source == 0 || source == null || b.SourceOfLead == source)

                                     select new
                                     {

                                         a.SalesEntryId,
                                         a.SENo,
                                         a.BillNo,
                                         a.SEDate,
                                         SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : 0,
                                         SETaxAmount = (srchtxt == "") ? a.SETaxAmount : 0,
                                         Credit = (srchtxt == "") ? j.Credit : 0,
                                         Customer = b.CustomerName,
                                         b.CustomerID,
                                         TaxRegNo = i.TRN,
                                         EmpName = d.FirstName + " " + d.LastName,
                                         SalesMan = m.FirstName + " " + m.LastName,
                                         MCName = g.MCName,
                                         SEPaidAmount = (srchtxt == "") ? c.SEPaidAmount : 0,
                                         a.CustomerType,
                                         SEBalanceAmount = (srchtxt == "") ? (a.SEGrandTotal - c.SEPaidAmount) : 0,
                                         //for expense
                                         PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == a.BillNo).Select(x => x.Paying).Sum()),
                                         salesreturns = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 != "Credit Note").Select(x => x.SRSubTotal).Sum()),
                                         salesreturnsnote = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 == "Credit Note").Select(x => x.SRGrandTotal).Sum()),

                                         discountt = (decimal?)(from ayy in db.BillSundrys
                                                                join azz in db.SEBillSundrys on ayy.BillSundryId equals azz.BillSundry

                                                                where ayy.BSName == "DISCOUNT" && azz.SalesEntry == a.SalesEntryId
                                                                select new
                                                                {
                                                                    BsAmount = (azz.BsAmount == null) ? 0 : azz.BsAmount
                                                                }
                                                          ).Sum(x => x.BsAmount) ?? 0,
                                         //PaymentExpense = (decimal?)(from x in db.Payments

                                         //                            where (x.InvoiceNo == a.BillNo)
                                         //                                Expense = x.GrandTotal
                                         //JournalExpense = (decimal?)(from x in db.Journals

                                         //                            where (x.InvoiceNo == a.BillNo)
                                         //                                Expense = x.GrandTotal

                                         JournalExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == a.BillNo).Select(y => y.Paying).Sum()),

                                         //itemprice = (decimal?)(from se in db.SEItemss
                                         //                       where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
                                         //                           purprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * se.ItemQuantity) : ((seit.PurchasePrice * se.ItemQuantity) / seit.ConFactor)
                                         //           ).Sum(x => x.purprice) ?? 0,

                                         a.SECreatedDate,
                                         PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                                         SaleType = a.SaleType,
                                         FromDate = h.StartDate,
                                         ToDate = h.EndDate,
                                         HireType = h.HireType,
                                         a.SalesStatus

                                     });

                            var vv = v.AsEnumerable().Select(o => new
                            {
                                o.SalesEntryId,
                                o.SENo,
                                o.Credit,
                                o.BillNo,
                                o.SEDate,
                                o.CustomerID,
                                o.SEGrandTotal,
                                o.SETaxAmount,
                                o.Customer,
                                o.TaxRegNo,
                                o.discountt,
                                o.EmpName,
                                o.MCName,
                                o.SEPaidAmount,
                                o.CustomerType,
                                o.SEBalanceAmount,
                                NewExpense = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
                                         ((o.PaymentExpense != null && o.JournalExpense != null) ? o.PaymentExpense + o.JournalExpense : ((o.PaymentExpense == null && o.JournalExpense != null) ? o.JournalExpense : o.PaymentExpense))) : 0,
                                NewExpenselink = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? "" :
                                         ((o.PaymentExpense != null && o.JournalExpense != null) ? String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>")) + " " + String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : ((o.PaymentExpense == null && o.JournalExpense != null) ? String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>"))))) : "",

                                salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
                                salesrtnnote = (o.salesreturnsnote == null) ? 0 : o.salesreturnsnote,

                                //Calling Function To Get Total Item Price for each Sales Entry
                                itemprice = (isemirtech == true) ? getusedprice(o.SalesEntryId) : GetTotalItemPrice(o.SalesEntryId, o.SEDate, srchtxt),
                                empcoste = (employeehourrate == true) ? (GetTotalEmpCost(o.SalesEntryId) * perhourcost) : GetTotalEmpCostwithrate(o.SalesEntryId),
                                o.SECreatedDate,
                                o.PayMethod,
                                o.SaleType,
                                o.FromDate,
                                o.ToDate,
                                o.HireType,
                                o.SalesStatus,
                                o.SalesMan
                            }).Distinct().OrderBy(a => a.SEDate).ToList();
                            var data = vv.ToList();
                            recordsTotal = 100;


                            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                            string result = javaScriptSerializer.Serialize(data);
                            returnstr = returnstr + result;
                        }
                        else
                        {
                            var v = (from a in db.SalesEntrys
                                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                                     from b in cust.DefaultIfEmpty()
                                     join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                                     join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                                     from d in emp.DefaultIfEmpty()
                                     join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                                     from f in paymeth.DefaultIfEmpty()
                                     join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                                     from g in mcs.DefaultIfEmpty()
                                     join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                                     from h in hir.DefaultIfEmpty()
                                     join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                                     from i in acc.DefaultIfEmpty()
                                     join j in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = sac }
                                     equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                                     from j in hir1.DefaultIfEmpty()
                                     join k in db.SEItemss on a.SalesEntryId equals k.SalesEntry into seit
                                     from k in seit.DefaultIfEmpty()
                                     join l in db.Items on k.Item equals l.ItemID into itemm
                                     from l in itemm.DefaultIfEmpty()
                                     join m in db.Employees on b.SalesPerson equals m.EmployeeId into empp
                                     from m in empp.DefaultIfEmpty()
                                     let grandtotalitmsearch = (decimal)(from m in db.SEItemss
                                                                         join nn in db.Items on m.Item equals nn.ItemID
                                                                         join oo in db.SalesEntrys on m.SalesEntry equals oo.SalesEntryId
                                                                         where nn.ItemName.Contains(srchtxt)
                                                                         && oo.SalesEntryId == a.SalesEntryId

                                                                         select new
                                                                         {
                                                                             totalprice = m.ItemUnitPrice * m.ItemQuantity
                                                                         }).Sum(o => o.totalprice)

                                     where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                                    (customer == 0 || a.Customer == customer) &&
                                    (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                                    (type == null || a.CustomerType == sEntry.CustomerType) &&
                                     (paymethod == null || a.PaymentMethod == paymethod) &&
                                     (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                                     (todates == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                                     (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                                    (htype == null || htype == null || h.HireType == htype) &&
                                    (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                                    (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)
                                    && (ddMC == 0 || ddMC == null || a.MaterialCenter == ddMC) &&

                                    (project == 0 || project == null || a.Project == project) &&
                                    (task == 0 || task == null || a.ProTask == task) &&
                                       (srchtxt == "" || l.ItemName.Contains(srchtxt))
                                        &&
                                         (source == 0 || source == null || b.SourceOfLead == source)

                                     select new
                                     {

                                         a.SalesEntryId,
                                         a.SENo,
                                         a.BillNo,
                                         a.SEDate,
                                         SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : grandtotalitmsearch,
                                         SETaxAmount = (srchtxt == "") ? a.SETaxAmount : 0,
                                         Credit = (srchtxt == "") ? j.Credit : grandtotalitmsearch,
                                         Customer = b.CustomerName,
                                         b.CustomerID,
                                         TaxRegNo = i.TRN,
                                         EmpName = d.FirstName + " " + d.LastName,
                                         MCName = g.MCName,
                                         SalesMan = m.FirstName + " " + m.LastName,
                                         SEPaidAmount = (srchtxt == "") ? c.SEPaidAmount : 0,
                                         a.CustomerType,
                                         SEBalanceAmount = (srchtxt == "") ? (a.SEGrandTotal - c.SEPaidAmount) : 0,
                                         //for expense
                                         PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == a.BillNo).Select(x => x.Paying).Sum()),
                                         salesreturns = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 != "Credit Note").Select(x => x.SRGrandTotal).Sum()),
                                         salesreturnsnote = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 == "Credit Note").Select(x => x.SRGrandTotal).Sum()),

                                         discountt = (from ay in db.BillSundrys
                                                      join az in db.SEBillSundrys on ay.BillSundryId equals az.BillSundry

                                                      where ay.BSName == "DISCOUNT" && az.SalesEntry == a.SalesEntryId
                                                      select new
                                                      {
                                                          az.BsAmount
                                                      }
                                                          ).Sum(x => x.BsAmount) ?? 0,
                                         //PaymentExpense = (decimal?)(from x in db.Payments

                                         //                            where (x.InvoiceNo == a.BillNo)
                                         //                                Expense = x.GrandTotal
                                         //JournalExpense = (decimal?)(from x in db.Journals

                                         //                            where (x.InvoiceNo == a.BillNo)
                                         //                                Expense = x.GrandTotal

                                         JournalExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == a.BillNo).Select(y => y.Paying).Sum()),
                                         //itemprice = (decimal?)(from se in db.SEItemss
                                         //                       where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
                                         //                           purprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * se.ItemQuantity) : ((seit.PurchasePrice * se.ItemQuantity) / seit.ConFactor)
                                         //           ).Sum(x => x.purprice) ?? 0,

                                         a.SECreatedDate,
                                         PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                                         SaleType = a.SaleType,
                                         FromDate = h.StartDate,
                                         ToDate = h.EndDate,
                                         HireType = h.HireType,
                                         a.SalesStatus

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
                                         o.discountt,
                                         o.EmpName,
                                         o.MCName,
                                         o.SEPaidAmount,
                                         o.CustomerType,
                                         o.SEBalanceAmount,
                                         o.SalesMan,
                                         NewExpense = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
                                         ((o.PaymentExpense != null) ? o.PaymentExpense : o.JournalExpense)) : 0,
                                         NewExpenselink = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? "" :
                                         ((o.PaymentExpense != null && o.JournalExpense != null) ? String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>")) + " " + String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : ((o.PaymentExpense == null && o.JournalExpense != null) ? String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>"))))) : "",

                                         salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
                                         salesrtnnote = (o.salesreturnsnote == null) ? 0 : o.salesreturnsnote,

                                         //Calling Function To Get Total Item Price for each Sales Entry
                                         itemprice = (isemirtech == true) ? getusedprice(o.SalesEntryId) : GetTotalItemPrice(o.SalesEntryId, o.SEDate, srchtxt),
                                         empcoste = GetTotalEmpCost(o.SalesEntryId) * perhourcost,
                                         o.SECreatedDate,
                                         o.PayMethod,
                                         o.SaleType,
                                         o.FromDate,
                                         o.ToDate,
                                         o.CustomerID,
                                         o.HireType,
                                         o.SalesStatus,
                                     }).OrderBy(a => a.SEDate).ThenBy(a => a.SECreatedDate);
                            var data = v.ToList();
                            recordsTotal = v.Count();


                            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                            string result = javaScriptSerializer.Serialize(new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                            returnstr = returnstr + result;
                        }
                    }
                    else
                    {

                        var pid = (from x in db.ProTasks
                                   where (location == "" || x.Location == location && (

                                     (fromdate == "" || EF.Functions.DateDiffDay(x.CreatedDate, fdate) <= 0) &&
                                 (todates == "" || EF.Functions.DateDiffDay(x.CreatedDate, tdate) >= 0)



                                   )
                                   ) &&
                                   (technician == null || technician == 0 || (from a in db.servicereports
                                                                              join
                                                   b in db.servicereportmembers on a.servicereportid equals b.servicereportid
                                                                              where a.protaskid == x.ProTaskId && b.employeeid == technician
                                                                              select new { a.protaskid }).Any())






                                   select new
                                   {
                                       pids = x.ProTaskId
                                   }).Select(o => o.pids).ToList().ToArray();

                        var salesids = (from x in db.SalesEntrys
                                        join y in db.additionaltasks on x.SalesEntryId equals y.salesentryid into yy
                                        from y in yy.DefaultIfEmpty()
                                        where (pid.Contains(y.taskid) || pid.Contains((long)x.ProTask))

                                        && (technician == null || technician == 0 || (
                                     (fromdate == "" || EF.Functions.DateDiffDay(x.SEDate, fdate) <= 0) &&
                                 (todates == "" || EF.Functions.DateDiffDay(x.SEDate, tdate) >= 0)
                                 ))
                                        select new
                                        {
                                            x.SalesEntryId
                                        }).Distinct().Select(o => o.SalesEntryId).ToList().ToArray();
                        var v = (from a in db.SalesEntrys

                                 join b in db.Customers on a.Customer equals b.CustomerID into cust
                                 from b in cust.DefaultIfEmpty()
                                 join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                                 join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                                 from d in emp.DefaultIfEmpty()
                                 join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                                 from f in paymeth.DefaultIfEmpty()
                                 join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                                 from g in mcs.DefaultIfEmpty()
                                 join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                                 equals new { h1 = h.Reference, h2 = h.Section } into hir
                                 from h in hir.DefaultIfEmpty()
                                 join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                                 from i in acc.DefaultIfEmpty()
                                 join j in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = sac }
                                 equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                                 from j in hir1.DefaultIfEmpty()
                                 join m in db.Employees on b.SalesPerson equals m.EmployeeId into empp
                                 from m in empp.DefaultIfEmpty()
                                 where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                                (customer == 0 || a.Customer == customer) &&
                                (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                                (type == null || a.CustomerType == sEntry.CustomerType) &&
                                 (paymethod == null || a.PaymentMethod == paymethod) &&
                                 salesids.Contains(a.SalesEntryId) &&
                                 (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                                (htype == null || htype == null || h.HireType == htype) &&
                                (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                                (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)
                               && (ddMC == 0 || ddMC == null || a.MaterialCenter == ddMC) &&

                                (project == 0 || project == null || a.Project == project) &&
                                (task == 0 || task == null || a.ProTask == task) &&
                                     (SalesMan == 0 || b.SalesPerson == SalesMan) && j.Credit > 0 &&

                                     (source == 0 || source == null || b.SourceOfLead == source)

                                 select new
                                 {

                                     a.SalesEntryId,
                                     a.SENo,
                                     a.BillNo,
                                     a.SEDate,
                                     SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : 0,
                                     SETaxAmount = (srchtxt == "") ? a.SETaxAmount : 0,
                                     Credit = (srchtxt == "") ? j.Credit : 0,
                                     Customer = b.CustomerName,
                                     b.CustomerID,
                                     TaxRegNo = i.TRN,
                                     EmpName = d.FirstName + " " + d.LastName,
                                     SalesMan = m.FirstName + " " + m.LastName,
                                     MCName = g.MCName,
                                     SEPaidAmount = (srchtxt == "") ? c.SEPaidAmount : 0,
                                     a.CustomerType,
                                     SEBalanceAmount = (srchtxt == "") ? (a.SEGrandTotal - c.SEPaidAmount) : 0,
                                     //for expense
                                     PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == a.BillNo).Select(x => x.Paying).Sum()),
                                     salesreturns = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 != "Credit Note").Select(x => x.SRSubTotal).Sum()),
                                     salesreturnsnote = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 == "Credit Note").Select(x => x.SRGrandTotal).Sum()),

                                     discountt = (decimal?)(from ayy in db.BillSundrys
                                                            join azz in db.SEBillSundrys on ayy.BillSundryId equals azz.BillSundry

                                                            where ayy.BSName == "DISCOUNT" && azz.SalesEntry == a.SalesEntryId
                                                            select new
                                                            {
                                                                BsAmount = (azz.BsAmount == null) ? 0 : azz.BsAmount
                                                            }
                                                      ).Sum(x => x.BsAmount) ?? 0,
                                     //PaymentExpense = (decimal?)(from x in db.Payments

                                     //                            where (x.InvoiceNo == a.BillNo)
                                     //                                Expense = x.GrandTotal
                                     //JournalExpense = (decimal?)(from x in db.Journals

                                     //                            where (x.InvoiceNo == a.BillNo)
                                     //                                Expense = x.GrandTotal

                                     JournalExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == a.BillNo).Select(y => y.Paying).Sum()),

                                     //itemprice = (decimal?)(from se in db.SEItemss
                                     //                       where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
                                     //                           purprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * se.ItemQuantity) : ((seit.PurchasePrice * se.ItemQuantity) / seit.ConFactor)
                                     //           ).Sum(x => x.purprice) ?? 0,

                                     a.SECreatedDate,
                                     PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                                     SaleType = a.SaleType,
                                     FromDate = h.StartDate,
                                     ToDate = h.EndDate,
                                     HireType = h.HireType,
                                     a.SalesStatus

                                 });

                        var vv = v.AsEnumerable().Select(o => new
                        {
                            o.SalesEntryId,
                            o.SENo,
                            o.Credit,
                            o.BillNo,
                            o.SEDate,
                            o.CustomerID,
                            o.SEGrandTotal,
                            o.SETaxAmount,
                            o.Customer,
                            o.TaxRegNo,
                            o.discountt,
                            o.EmpName,
                            o.MCName,
                            o.SEPaidAmount,
                            o.CustomerType,
                            o.SEBalanceAmount,
                            NewExpense = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
                                     ((o.PaymentExpense != null && o.JournalExpense != null) ? o.PaymentExpense + o.JournalExpense : ((o.PaymentExpense == null && o.JournalExpense != null) ? o.JournalExpense : o.PaymentExpense))) : 0,
                            NewExpenselink = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? "" :
                                     ((o.PaymentExpense != null && o.JournalExpense != null) ? String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>")) + " " + String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : ((o.PaymentExpense == null && o.JournalExpense != null) ? String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>"))))) : "",

                            salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
                            salesrtnnote = (o.salesreturnsnote == null) ? 0 : o.salesreturnsnote,

                            //Calling Function To Get Total Item Price for each Sales Entry
                            itemprice = (isemirtech == true) ? getusedprice(o.SalesEntryId) : GetTotalItemPrice(o.SalesEntryId, o.SEDate, srchtxt),
                            empcoste = (employeehourrate == true) ? (GetTotalEmpCost(o.SalesEntryId) * perhourcost) : GetTotalEmpCostwithrate(o.SalesEntryId),
                            mycoste = GetTotalEmpCost(o.SalesEntryId, technician) * perhourcost,
                            o.SECreatedDate,
                            o.PayMethod,
                            o.SaleType,
                            o.FromDate,
                            o.ToDate,
                            o.HireType,
                            o.SalesStatus,
                            o.SalesMan
                        }).Distinct().OrderBy(a => a.SEDate).ToList();
                        var data = vv.ToList();
                        recordsTotal = 100;


                        JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                        javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                        string result = javaScriptSerializer.Serialize(new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                        returnstr = returnstr + result;




                    }

                }
                var results = new ContentResult
                {
                    Content = returnstr,
                    ContentType = "application/json"
                };
                return results;
            }

        }

        [HttpPost]

      
 public ActionResult GetAllSaleprofitsummery(string completed, string seno, double perhourcost, long? paymethod, long? SalesMan, long? customer, long? SalesExecutive, string fromdate, string todates, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate, long? project, long? task, string srchtxt, bool employeehourrate, bool amccus, long? source, string location, long? technician, long? sourcelead, bool cached, long? cusage, bool nostocktransfer)
        {
            db.SetCommandTimeOut(60 * 60);
            //QUICK NET COMPUTERS
            var isemirtech = db.companys.Any(o => o.CPName.Contains("EMIRTECH TECHNOLOGY"));
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
            if (satype != "")
            {
                St = (satype == "1") ? SaleType.Sale : SaleType.Hire;
            };

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todates != "")
            {
                tdate = DateTime.Parse(todates, new CultureInfo("en-GB"));
            }

            DateTime? hfrmdate = null;
            DateTime? htodate = null;
            if (!string.IsNullOrEmpty(hfdate))
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

            var allshworoom = db.MCs.Where(o => o.MCId == ddMC).Select(o => o.MCName).FirstOrDefault();

     


                  
                        IEnumerable<SalesEntry> salesentrys = (from a in db.SalesEntrys

                                                               where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                                                    (customer == 0 || a.Customer == customer) &&
                                                    (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                                                    (type == null || a.CustomerType == sEntry.CustomerType) &&
                                                     (paymethod == null || a.PaymentMethod == paymethod) &&
                                                     (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                                                     (todates == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                                                     (string.IsNullOrEmpty(satype) || a.SaleType == St)

                                                   && (ddMC == 0 || ddMC == null || a.MaterialCenter == ddMC) &&

                                                    (project == 0 || project == null || a.Project == project) &&
                                                    (task == 0 || task == null || a.ProTask == task)

                                                               select a


                                           ).AsEnumerable();
                        var salereturnids = (from a in db.SalesReturns
                                             join b in db.SRItemss on a.SalesReturnId equals b.SalesReturnId
                                             join c in salesentrys on a.SalesEntryId equals c.SalesEntryId
                                             group new { b.Item, b.ItemQuantity, c.SalesEntryId, c.BillNo } by new { c.SalesEntryId, b.Item } into grp
                                             select new salesreturnmodel
                                             {
                                                 itid = grp.FirstOrDefault().Item,
                                                 saleid = grp.FirstOrDefault().SalesEntryId,
                                                 qty = (grp.Count() == 1) ? grp.Sum(o => o.ItemQuantity) : grp.Average(o => o.ItemQuantity),
                                                 billno = grp.FirstOrDefault().BillNo,
                                             }
                           ).ToList();

                        long comm = 0;
            var amccusts = (from aaa in db.Amcs
                           where aaa.OpenClose == 0
                           select new
                           {
                               custid = aaa.CustomerId
                           }).Select(o => o.custid).Distinct().ToList().ToArray();
                        var v = (from a in db.SalesEntrys
                                 join b in db.Customers on a.Customer equals b.CustomerID into cust
                                 from b in cust.DefaultIfEmpty()
                                 

                                 join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry

                                 join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                                 from d in emp.DefaultIfEmpty()
                                 join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                                 from f in paymeth.DefaultIfEmpty()
                                 join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                                 from g in mcs.DefaultIfEmpty()
                                 join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                                 equals new { h1 = h.Reference, h2 = h.Section } into hir
                                 from h in hir.DefaultIfEmpty()
                                 join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                                 from i in acc.DefaultIfEmpty()
                                 join j in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = sac }
                                 equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                                 from j in hir1.DefaultIfEmpty()
                                 join m in db.Employees on b.SalesPerson equals m.EmployeeId into empp
                                 from m in empp.DefaultIfEmpty()
                                 let leads = (
                                 from xx in db.Customers
                                 join cc in db.leadcustomerrelation on xx.CustomerID equals cc.customerid
                                 where cc.customerid == b.CustomerID
                                 select new
                                 {
                                     xx.SourceOfLead
                                 }
                                 ).FirstOrDefault()
                                 where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                                (customer == 0 || a.Customer == customer) &&
                                (cusage == 0 || cusage == null || (cusage == 1 && b.CreatedDate >= fdate && b.CreatedDate <= tdate) || (cusage == 2 && b.CreatedDate < fdate)) &&
                                (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                                (type == null || a.CustomerType == sEntry.CustomerType) &&
                                 (paymethod == null || a.PaymentMethod == paymethod) &&
                                 (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                                 (todates == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                                 (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                                (htype == null || htype == null || h.HireType == htype) &&
                                (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                                (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)
                               && (ddMC == 0 || ddMC == null || a.MaterialCenter == ddMC) &&

                                (project == 0 || project == null || a.Project == project) &&
                                (task == 0 || task == null || a.ProTask == task) &&
                                     (SalesMan == 0 || b.SalesPerson == SalesMan) && j.Credit > 0 &&

                                     (source == 0 || source == null || b.SourceOfLead == source) &&
(sourcelead == 0 || sourcelead == null | (leads != null && leads.SourceOfLead == sourcelead))&&
(amccus==false||amccusts.Contains(a.Customer))

                                 select new
                                 {

                                     a.SalesEntryId,
                                     a.SENo,
                                     a.BillNo,
                                     mtcost = (a.materialcost == null) ? 0 : a.materialcost,
                                     a.SEDate,
                                     SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : 0,
                                     SETaxAmount = (srchtxt == "") ? a.SETaxAmount : 0,
                                     Credit = (srchtxt == "") ? j.Credit : 0,
                                     Customer = b.CustomerName,
                                     b.CustomerID,
                                     TaxRegNo = i.TRN,
                                     EmpName = d.FirstName + " " + d.LastName,
                                     a.SECashier,
                                     SalesMan = m.FirstName + " " + m.LastName,
                                     MCName = g.MCName,
                                     SEPaidAmount = (srchtxt == "") ? c.SEPaidAmount : 0,
                                     a.CustomerType,
                                     SEBalanceAmount = (srchtxt == "") ? (a.SEGrandTotal - c.SEPaidAmount) : 0,
                                     //for expense
                                     PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == a.BillNo).Select(x => x.Paying).Sum()),
                                     salesreturns = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 != "Credit Note").Select(x => x.SRSubTotal).Sum()),
                                     salesreturnsnote = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 == "Credit Note").Select(x => x.SRGrandTotal).Sum()),
                                     completed = (from comp in db.salesmanprofittargets
                                                  where
                                                 comp.salesentryid == a.SalesEntryId &&
                                                 comp.completed == comm
                                                  select comp
                                                 ).OrderByDescending(o => o.targetprofitid).FirstOrDefault(),

                                     contribute = (from comp in db.salesmanprofittargets
                                                   where
                                                  comp.salesentryid == a.SalesEntryId &&
                                                  comp.employeeid == a.SECashier
                                                   select comp

                                                ).OrderByDescending(o => o.targetprofitid).FirstOrDefault(),

                                     target = db.Employees.Where(o => o.EmployeeId == a.SECashier).Select(o => o.OtherIdNo).FirstOrDefault(),


                                     discountt = (decimal?)(from ayy in db.BillSundrys
                                                            join azz in db.SEBillSundrys on ayy.BillSundryId equals azz.BillSundry

                                                            where ayy.BSName == "DISCOUNT" && azz.SalesEntry == a.SalesEntryId
                                                            select new
                                                            {
                                                                BsAmount = (azz.BsAmount == null) ? 0 : azz.BsAmount
                                                            }
                                                      ).Sum(x => x.BsAmount) ?? 0,
                                     roundoffmin = (decimal?)(
                                        from k in db.SEBillSundrys
                                        where k.SalesEntry == a.SalesEntryId &&
                                       k.BillSundry == 2
                                        select new
                                        {
                                            k.BsAmount
                                        }).Select(o => o.BsAmount).FirstOrDefault() ?? 0,
                                     roundoffplus = (decimal?)(from k in db.SEBillSundrys
                                                               where k.SalesEntry == a.SalesEntryId &&
                                                              k.BillSundry == 1
                                                               select new
                                                               {
                                                                   k.BsAmount
                                                               }).Select(o => o.BsAmount).FirstOrDefault() ?? 0,

                                     //PaymentExpense = (decimal?)(from x in db.Payments

                                     //                            where (x.InvoiceNo == a.BillNo)
                                     //                                Expense = x.GrandTotal
                                     //JournalExpense = (decimal?)(from x in db.Journals

                                     //                            where (x.InvoiceNo == a.BillNo)
                                     //                                Expense = x.GrandTotal

                                     JournalExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == a.BillNo).Select(y => y.Paying).Sum()),

                                     //itemprice = (decimal?)(from se in db.SEItemss
                                     //                       where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
                                     //                           purprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * se.ItemQuantity) : ((seit.PurchasePrice * se.ItemQuantity) / seit.ConFactor)
                                     //           ).Sum(x => x.purprice) ?? 0,

                                     a.SECreatedDate,
                                     PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                                     SaleType = a.SaleType,
                                     FromDate = h.StartDate,
                                     ToDate = h.EndDate,
                                     HireType = h.HireType,
                                     a.SalesStatus,


                                 });


                        var vv = v.ToList().Where(o =>

                            (completed == "" || completed == null || (completed == "0" && ((o.completed == null) ? o.BillNo == "-1" : o.completed.completed == 0)) || (completed == "1" && ((o.completed == null) ? 1 == 1 : o.completed.completed == 1)))


                        ).Select(o => new
                        {
                            o.SalesEntryId,
                            o.SENo,
                            o.mtcost,
                            o.Credit,
                            o.BillNo,
                            target = (o.target == null || o.target == "") ? 0 : Convert.ToDecimal(o.target),
                            o.SEDate,
                            o.CustomerID,
                            o.SEGrandTotal,
                            o.SETaxAmount,
                            o.completed,
                            o.contribute,
                            o.Customer,
                            o.TaxRegNo,
                            o.discountt,
                            o.roundoffmin,
                            o.roundoffplus,
                            SalesPerson = o.SECashier,
                            o.EmpName,
                            o.MCName,
                            o.SEPaidAmount,
                            o.CustomerType,
                            o.SEBalanceAmount,
                            NewExpense = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
                                     ((o.PaymentExpense != null && o.JournalExpense != null) ? o.PaymentExpense + o.JournalExpense : ((o.PaymentExpense == null && o.JournalExpense != null) ? o.JournalExpense : o.PaymentExpense))) : 0,
                            NewExpenselink = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? "" :
                                     ((o.PaymentExpense != null && o.JournalExpense != null) ? String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>")) + " " + String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : ((o.PaymentExpense == null && o.JournalExpense != null) ? String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>"))))) : "",
    
                            salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
                            salesrtnnote = (o.salesreturnsnote == null) ? 0 : o.salesreturnsnote,

                            //Calling Function To Get Total Item Price for each Sales Entry
                            itemprice = (isemirtech == true) ? Convert.ToDecimal(getusedprice(o.SalesEntryId)["itemprice"]) : (cached != true) ? Convert.ToDecimal( GetTotalItemPricenewnew(o.SalesEntryId, o.SEDate, srchtxt, salesentrys, salereturnids, nostocktransfer)["itemprice"]): Convert.ToDecimal(getcachedprice(o.mtcost)["itemprice"]),
                            empcoste = (isemirtech == true) ? ((employeehourrate == true) ? (GetTotalEmpCost(o.SalesEntryId) * perhourcost) : GetTotalEmpCostwithrate(o.SalesEntryId)) : 0,

                            o.SECreatedDate,
                            o.PayMethod,
                            o.SaleType,
                            o.FromDate,
                            o.ToDate,
                            o.HireType,
                            o.SalesStatus,
                            o.SalesMan
                        }).ToList().GroupBy(e=>e.CustomerID).Select(g=>new
                        {
                           SalesEntryId=g.Select(o=>o.SalesEntryId).FirstOrDefault(),
                           SENo = g.Select(o => o.SENo).FirstOrDefault(),
                           mtcost = g.Select(o => o.mtcost).Sum(),
                           Credit = g.Select(o => o.Credit).Sum(),
                           BillNo = g.Select(o => o.BillNo).FirstOrDefault(),
                            target =  g.Select(o => o.target).FirstOrDefault(),
                            SEDate = g.Select(o => o.SEDate).FirstOrDefault(),
                           CustomerID = g.Key,
                           SEGrandTotal = g.Select(o => o.SEGrandTotal).Sum(),
                           SETaxAmount = g.Select(o => o.SETaxAmount).Sum(),
                            completed = g.Select(o => o.completed).FirstOrDefault(),
                            contribute = g.Select(o => o.contribute).FirstOrDefault(),
                            Customer = g.Select(o => o.Customer).FirstOrDefault(),


                            TaxRegNo = g.Select(o => o.TaxRegNo).FirstOrDefault(),
                            discountt = g.Select(o => o.discountt).Sum(),
                            roundoffmin = g.Select(o => o.roundoffmin).Sum(),
                            roundoffplus = g.Select(o => o.roundoffplus).Sum(),
                            SalesPerson = g.Select(o => o.SalesPerson).FirstOrDefault(),
                            EmpName = g.Select(o => o.EmpName).FirstOrDefault(),
                            MCName = g.Select(o => o.MCName).FirstOrDefault(),
                            SEPaidAmount = g.Select(o => o.SEPaidAmount).Sum(),
                            CustomerType = g.Select(o => o.CustomerType).FirstOrDefault(),
                            SEBalanceAmount = g.Select(o => o.SEBalanceAmount).Sum(),
                            NewExpense  = g.Select(o => o.NewExpense).Sum(),
                            NewExpenselink = g.Select(o => o.NewExpenselink).FirstOrDefault(),

                            salesrtn = g.Select(o => o.salesrtn).Sum(),
                            salesrtnnote = g.Select(o => o.salesrtnnote).Sum(),
                            //Calling Function To Get Total Item Price for each Sales Entry
                            itemprice = g.Select(o=>o.itemprice).Sum(),
                            empcoste = g.Select(o => o.empcoste).Sum(),
                            SECreatedDate= g.Select(o => o.SECreatedDate).FirstOrDefault(),
                            PayMethod =g.Select(o => o.TaxRegNo).FirstOrDefault(),
                            SaleType =g.Select(o => o.SaleType).FirstOrDefault(),
                            FromDate =g.Select(o => o.FromDate).FirstOrDefault(),
                            ToDate= g.Select(o => o.ToDate).FirstOrDefault(),
                            HireType= g.Select(o => o.HireType).FirstOrDefault(),
                            SalesStatus =g.Select(o => o.SalesStatus).FirstOrDefault(),
                            SalesMan = g.Select(o => o.SalesMan).FirstOrDefault(),
                        }).ToList();
                        

                        recordsTotal = 100;


                        JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                        javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                        string result = javaScriptSerializer.Serialize(new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = vv });
                        var results = new ContentResult
                        {
                            Content = result,
                            ContentType = "application/json"
                        };
                        return results;
                    
               
             
      

        }

        public ActionResult GetAllSaleprofit(string completed, string seno, double perhourcost, long? paymethod, long? SalesMan, long? customer, long? SalesExecutive, string fromdate, string todates, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate, long? project, long? task, string srchtxt, bool employeehourrate, long? source, string location, long? technician, long? sourcelead, bool cached, long? cusage, bool nostocktransfer)
        {
            db.SetCommandTimeOut(60 * 60);
            //QUICK NET COMPUTERS
            var isemirtech = db.companys.Any(o => o.CPName.Contains("EMIRTECH TECHNOLOGY"));
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
            if (satype != "")
            {
                St = (satype == "1") ? SaleType.Sale : SaleType.Hire;
            };

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todates != "")
            {
                tdate = DateTime.Parse(todates, new CultureInfo("en-GB"));
            }

            DateTime? hfrmdate = null;
            DateTime? htodate = null;
            if (!string.IsNullOrEmpty(hfdate))
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

            var allshworoom = db.MCs.Where(o => o.MCId == ddMC).Select(o => o.MCName).FirstOrDefault();

            if (allshworoom != "ALL SHOWROOMS")
            {
                if ((location == "All" || location == "") && (technician == null || technician == 0))
                {




                    if (srchtxt == "")
                    {
                        IEnumerable<SalesEntry> salesentrys = (from a in db.SalesEntrys

                                                               where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                                                    (customer == 0 || a.Customer == customer) &&
                                                    (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                                                    (type == null || a.CustomerType == sEntry.CustomerType) &&
                                                     (paymethod == null || a.PaymentMethod == paymethod) &&
                                                     (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                                                     (todates == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                                                     (string.IsNullOrEmpty(satype) || a.SaleType == St)

                                                   && (ddMC == 0 || ddMC == null || a.MaterialCenter == ddMC) &&

                                                    (project == 0 || project == null || a.Project == project) &&
                                                    (task == 0 || task == null || a.ProTask == task)

                                                               select a


                                           ).AsEnumerable();
                        var salereturnids = (from a in db.SalesReturns
                                             join b in db.SRItemss on a.SalesReturnId equals b.SalesReturnId
                                             join c in salesentrys on a.SalesEntryId equals c.SalesEntryId
                                             group new { b.Item, b.ItemQuantity, c.SalesEntryId, c.BillNo } by new { c.SalesEntryId, b.Item } into grp
                                             select new salesreturnmodel
                                             {
                                                 itid = grp.FirstOrDefault().Item,
                                                 saleid = grp.FirstOrDefault().SalesEntryId,
                                                 qty = (grp.Count() == 1) ? grp.Sum(o => o.ItemQuantity) : grp.Average(o => o.ItemQuantity),
                                                 billno = grp.FirstOrDefault().BillNo,
                                             }
                           ).ToList();

                        long comm = 0;
            
                        var v = (from a in db.SalesEntrys
                                 join b in db.Customers on a.Customer equals b.CustomerID into cust
                                 from b in cust.DefaultIfEmpty()

                                 join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry

                                 join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                                 from d in emp.DefaultIfEmpty()
                                 join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                                 from f in paymeth.DefaultIfEmpty()
                                 join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                                 from g in mcs.DefaultIfEmpty()
                                 join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                                 equals new { h1 = h.Reference, h2 = h.Section } into hir
                                 from h in hir.DefaultIfEmpty()
                                 join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                                 from i in acc.DefaultIfEmpty()
                                 join j in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = sac }
                                 equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                                 from j in hir1.DefaultIfEmpty()
                                 join m in db.Employees on b.SalesPerson equals m.EmployeeId into empp
                                 from m in empp.DefaultIfEmpty()
                                 let leads = (
                                 from xx in db.Customers
                                 join cc in db.leadcustomerrelation on xx.CustomerID equals cc.customerid
                                 where cc.customerid == b.CustomerID
                                 select new
                                 {
                                     xx.SourceOfLead
                                 }
                                 ).FirstOrDefault()
                                 where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                                (customer == 0 || a.Customer == customer) &&
                                (cusage == 0||cusage==null||(cusage==1&&b.CreatedDate>=fdate && b.CreatedDate<=tdate)||(cusage==2&&b.CreatedDate<fdate))&&
                                (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                                (type == null || a.CustomerType == sEntry.CustomerType) &&
                                 (paymethod == null || a.PaymentMethod == paymethod) &&
                                 (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                                 (todates == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                                 (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                                (htype == null || htype == null || h.HireType == htype) &&
                                (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                                (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)
                               && (ddMC == 0 || ddMC == null || a.MaterialCenter == ddMC) &&

                                (project == 0 || project == null || a.Project == project) &&
                                (task == 0 || task == null || a.ProTask == task) &&
                                     (SalesMan == 0 || b.SalesPerson == SalesMan) && j.Credit > 0 &&

                                     (source == 0 || source == null || b.SourceOfLead == source) &&
(sourcelead == 0 || sourcelead == null | (leads != null && leads.SourceOfLead == sourcelead))

                                 select new
                                 {

                                     a.SalesEntryId,
                                     a.SENo,
                                     a.BillNo,
                                     mtcost = (a.materialcost == null) ? 0 : a.materialcost,
                                     a.SEDate,
                                     SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : 0,
                                     SETaxAmount = (srchtxt == "") ? a.SETaxAmount : 0,
                                     Credit = (srchtxt == "") ? j.Credit : 0,
                                     Customer = b.CustomerName,
                                     b.CustomerID,
                                     TaxRegNo = i.TRN,
                                     EmpName = d.FirstName + " " + d.LastName,
                                     a.SECashier,
                                     SalesMan = m.FirstName + " " + m.LastName,
                                     MCName = g.MCName,
                                     SEPaidAmount = (srchtxt == "") ? c.SEPaidAmount : 0,
                                     a.CustomerType,
                                     SEBalanceAmount = (srchtxt == "") ? (a.SEGrandTotal - c.SEPaidAmount) : 0,
                                     //for expense
                                     PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == a.BillNo).Select(x => x.Paying).Sum()),
                                     salesreturns = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 != "Credit Note").Select(x => x.SRSubTotal).Sum()),
                                     salesreturnsnote = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 == "Credit Note").Select(x => x.SRGrandTotal).Sum()),
                                     completed = (from comp in db.salesmanprofittargets
                                                  where
                                                 comp.salesentryid == a.SalesEntryId &&
                                                 comp.completed == comm
                                                 select  comp
                                                 ).OrderByDescending(o=>o.targetprofitid).FirstOrDefault(),
                                                
                                     contribute = (from comp in db.salesmanprofittargets
                                                   where
                                                  comp.salesentryid == a.SalesEntryId &&
                                                  comp.employeeid ==a.SECashier
                                                   select comp
                                                  
                                                ).OrderByDescending(o=>o.targetprofitid).FirstOrDefault(),

                                    target = db.Employees.Where(o => o.EmployeeId == a.SECashier).Select(o => o.OtherIdNo).FirstOrDefault(),
                       

                        discountt = (decimal?)(from ayy in db.BillSundrys
                                                            join azz in db.SEBillSundrys on ayy.BillSundryId equals azz.BillSundry

                                                            where ayy.BSName == "DISCOUNT" && azz.SalesEntry == a.SalesEntryId
                                                            select new
                                                            {
                                                                BsAmount = (azz.BsAmount == null) ? 0 : azz.BsAmount
                                                            }
                                                      ).Sum(x => x.BsAmount) ?? 0,
                  roundoffmin = (decimal?) (
                                        from k in db.SEBillSundrys
                                        where k.SalesEntry == a.SalesEntryId &&
                                       k.BillSundry == 2
                                        select new
                                        {
                                            k.BsAmount
                                        }).Select(o=>o.BsAmount).FirstOrDefault()??0,
                      roundoffplus = (decimal?)(from k in db.SEBillSundrys
                                         where k.SalesEntry == a.SalesEntryId &&
                                        k.BillSundry == 1
                                         select new
                                         {
                                             k.BsAmount
                                         }).Select(o=>o.BsAmount).FirstOrDefault()??0,

                                     //PaymentExpense = (decimal?)(from x in db.Payments

                        //                            where (x.InvoiceNo == a.BillNo)
                        //                                Expense = x.GrandTotal
                        //JournalExpense = (decimal?)(from x in db.Journals

                        //                            where (x.InvoiceNo == a.BillNo)
                        //                                Expense = x.GrandTotal

                        JournalExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == a.BillNo).Select(y => y.Paying).Sum()),

                                     //itemprice = (decimal?)(from se in db.SEItemss
                                     //                       where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
                                     //                           purprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * se.ItemQuantity) : ((seit.PurchasePrice * se.ItemQuantity) / seit.ConFactor)
                                     //           ).Sum(x => x.purprice) ?? 0,

                                     a.SECreatedDate,
                                     PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                                     SaleType = a.SaleType,
                                     FromDate = h.StartDate,
                                     ToDate = h.EndDate,
                                     HireType = h.HireType,
                                     a.SalesStatus,


                                 });
                 
                     
                        var vv = v.ToList().Where(o=>
                        
                            (completed == "" || completed == null|| (completed=="0"&& ((o.completed==null)?o.BillNo=="-1":o.completed.completed ==0)) || (completed == "1" && ((o.completed == null) ? 1==1: o.completed.completed == 1             )))


                        ).Select(o => new
                        {
                            o.SalesEntryId,
                            o.SENo,
                            o.mtcost,
                            o.Credit,
                            o.BillNo,
                            target=(o.target==null||o.target=="")?0:Convert.ToDecimal(o.target),
                            o.SEDate,
                            o.CustomerID,
                            o.SEGrandTotal,
                            o.SETaxAmount,
                            o.completed,
                           o.contribute,
                            o.Customer,
                            o.TaxRegNo,
                            o.discountt,
                            o.roundoffmin,
                            o.roundoffplus,
                            SalesPerson=o.SECashier,
                            o.EmpName,
                            o.MCName,
                            o.SEPaidAmount,
                            o.CustomerType,
                            o.SEBalanceAmount,
                            NewExpense = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
                                     ((o.PaymentExpense != null && o.JournalExpense != null) ? o.PaymentExpense + o.JournalExpense : ((o.PaymentExpense == null && o.JournalExpense != null) ? o.JournalExpense : o.PaymentExpense))) : 0,
                            NewExpenselink = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? "" :
                                     ((o.PaymentExpense != null && o.JournalExpense != null) ? String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>")) + " " + String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : ((o.PaymentExpense == null && o.JournalExpense != null) ? String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>"))))) : "",

                            salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
                            salesrtnnote = (o.salesreturnsnote == null) ? 0 : o.salesreturnsnote,

                            //Calling Function To Get Total Item Price for each Sales Entry
                            itemprice = (isemirtech == true) ? getusedprice(o.SalesEntryId) : (cached != true) ? GetTotalItemPricenewnew(o.SalesEntryId, o.SEDate,  srchtxt,salesentrys, salereturnids,nostocktransfer) : getcachedprice(o.mtcost),
                            empcoste = (isemirtech == true) ?( (employeehourrate == true) ? (GetTotalEmpCost(o.SalesEntryId) * perhourcost) : GetTotalEmpCostwithrate(o.SalesEntryId)):0,

                            o.SECreatedDate,
                            o.PayMethod,
                            o.SaleType,
                            o.FromDate,
                            o.ToDate,
                            o.HireType,
                            o.SalesStatus,
                            o.SalesMan
                        }).Distinct().OrderBy(a => a.SEDate).ToList();
                        var data = vv.ToList();
                        foreach (var d in data)
                        {
                            var se = db.SalesEntrys.Find(d.SalesEntryId);
                            se.materialcost = (decimal)d.itemprice["itemprice"];
                            db.Entry(se).State = EntityState.Modified;
                            db.SaveChanges();

                        }
                      
                        recordsTotal = 100;


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
                    else
                    {
                        var v = (from a in db.SalesEntrys
                                 join b in db.Customers on a.Customer equals b.CustomerID into cust
                                 from b in cust.DefaultIfEmpty()
                                 join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                                 join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                                 from d in emp.DefaultIfEmpty()
                                 join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                                 from f in paymeth.DefaultIfEmpty()
                                 join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                                 from g in mcs.DefaultIfEmpty()
                                 join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                                 equals new { h1 = h.Reference, h2 = h.Section } into hir
                                 from h in hir.DefaultIfEmpty()
                                 join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                                 from i in acc.DefaultIfEmpty()
                                 join j in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = sac }
                                 equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                                 from j in hir1.DefaultIfEmpty()
                                 join k in db.SEItemss on a.SalesEntryId equals k.SalesEntry into seit
                                 from k in seit.DefaultIfEmpty()
                                 join l in db.Items on k.Item equals l.ItemID into itemm
                                 from l in itemm.DefaultIfEmpty()
                                 join m in db.Employees on b.SalesPerson equals m.EmployeeId into empp
                                 from m in empp.DefaultIfEmpty()
                                 let grandtotalitmsearch = (decimal)(from m in db.SEItemss
                                                                     join nn in db.Items on m.Item equals nn.ItemID
                                                                     join oo in db.SalesEntrys on m.SalesEntry equals oo.SalesEntryId
                                                                     where nn.ItemName.Contains(srchtxt)
                                                                     && oo.SalesEntryId == a.SalesEntryId

                                                                     select new
                                                                     {
                                                                         totalprice = m.ItemUnitPrice * m.ItemQuantity
                                                                     }).Sum(o => o.totalprice)

                                 where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                                (customer == 0 || a.Customer == customer) &&
                                (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                                (type == null || a.CustomerType == sEntry.CustomerType) &&
                                 (paymethod == null || a.PaymentMethod == paymethod) &&
                                 (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                                 (todates == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                                 (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                                (htype == null || htype == null || h.HireType == htype) &&
                                (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                                (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)
                                && (ddMC == 0 || ddMC == null || a.MaterialCenter == ddMC) &&

                                (project == 0 || project == null || a.Project == project) &&
                                (task == 0 || task == null || a.ProTask == task) &&
                                   (srchtxt == "" || l.ItemName.Contains(srchtxt))
                                    &&
                                     (source == 0 || source == null || b.SourceOfLead == source)

                                 select new
                                 {

                                     a.SalesEntryId,
                                     a.SENo,
                                     a.BillNo,
                                     a.SEDate,
                                     a.SECashier,
                                     SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : grandtotalitmsearch,
                                     SETaxAmount = (srchtxt == "") ? a.SETaxAmount : 0,
                                     Credit = (srchtxt == "") ? j.Credit : grandtotalitmsearch,
                                     Customer = b.CustomerName,
                                     b.CustomerID,
                                     TaxRegNo = i.TRN,
                                     EmpName = d.FirstName + " " + d.LastName,
                                     MCName = g.MCName,
                                     SalesMan = m.FirstName + " " + m.LastName,
                                     SEPaidAmount = (srchtxt == "") ? c.SEPaidAmount : 0,
                                     a.CustomerType,
                                     SEBalanceAmount = (srchtxt == "") ? (a.SEGrandTotal - c.SEPaidAmount) : 0,
                                     //for expense
                                     PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == a.BillNo).Select(x => x.Paying).Sum()),
                                     salesreturns = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 != "Credit Note").Select(x => x.SRGrandTotal).Sum()),
                                     salesreturnsnote = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 == "Credit Note").Select(x => x.SRGrandTotal).Sum()),
                                     completed = (from comp in db.salesmanprofittargets
                                                  where
                                                 comp.salesentryid == a.SalesEntryId &&
                                                 comp.completed == 0
                                                  select new
                                                  {
                                                      comp.completed,
                                                      comp.contributionpercentage
                                                  }
                                                ).FirstOrDefault(),
                                     contribute = (from comp in db.salesmanprofittargets
                                                   where
                                                  comp.salesentryid == a.SalesEntryId &&
                                                  comp.employeeid == a.SECashier
                                                   select new
                                                   {

                                                       comp.contributionpercentage,
                                                       comp.completed,
                                                   }
                                                ).FirstOrDefault(),
                                     target = db.Employees.Where(o => o.EmployeeId == a.SECashier).Select(o => o.OtherIdNo).FirstOrDefault(),

                                     discountt = (from ay in db.BillSundrys
                                                  join az in db.SEBillSundrys on ay.BillSundryId equals az.BillSundry

                                                  where ay.BSName == "DISCOUNT" && az.SalesEntry == a.SalesEntryId
                                                  select new
                                                  {
                                                      az.BsAmount
                                                  }
                                                      ).Sum(x => x.BsAmount) ?? 0,
                                     //PaymentExpense = (decimal?)(from x in db.Payments

                                     //                            where (x.InvoiceNo == a.BillNo)
                                     //                                Expense = x.GrandTotal
                                     //JournalExpense = (decimal?)(from x in db.Journals

                                     //                            where (x.InvoiceNo == a.BillNo)
                                     //                                Expense = x.GrandTotal

                                     JournalExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == a.BillNo).Select(y => y.Paying).Sum()),
                                     //itemprice = (decimal?)(from se in db.SEItemss
                                     //                       where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
                                     //                           purprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * se.ItemQuantity) : ((seit.PurchasePrice * se.ItemQuantity) / seit.ConFactor)
                                     //           ).Sum(x => x.purprice) ?? 0,

                                     a.SECreatedDate,
                                     PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                                     SaleType = a.SaleType,
                                     FromDate = h.StartDate,
                                     ToDate = h.EndDate,
                                     HireType = h.HireType,
                                     a.SalesStatus

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
                                     o.discountt,
                                     o.EmpName,
                                     o.MCName,
                                     o.SEPaidAmount,
                                     o.CustomerType,
                                     o.SEBalanceAmount,
                                     o.SalesMan,
                                     NewExpense = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
                                     ((o.PaymentExpense != null) ? o.PaymentExpense : o.JournalExpense)) : 0,
                                     NewExpenselink = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? "" :
                                     ((o.PaymentExpense != null && o.JournalExpense != null) ? String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>")) + " " + String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : ((o.PaymentExpense == null && o.JournalExpense != null) ? String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>"))))) : "",

                                     salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
                                     salesrtnnote = (o.salesreturnsnote == null) ? 0 : o.salesreturnsnote,
                                     target = (o.target == null || o.target == "") ? 0 : Convert.ToDecimal(o.target),

                                     //Calling Function To Get Total Item Price for each Sales Entry
                                     itemprice = (isemirtech == true) ? getusedprice(o.SalesEntryId) : GetTotalItemPrice(o.SalesEntryId, o.SEDate, srchtxt),
                                     empcoste = GetTotalEmpCost(o.SalesEntryId) * perhourcost,
                                     o.SECreatedDate,
                                     o.PayMethod,
                                     o.completed,
                                     o.contribute,
                                     o.SaleType,
                                     o.FromDate,
                                     o.ToDate,
                                     o.CustomerID,
                                     o.HireType,
                                     o.SalesStatus,
                              SalesPerson = o.SECashier,
                                 }).OrderBy(a => a.SEDate).ThenBy(a => a.SECreatedDate);
                        var data = v.ToList();
                        var salesmans = data.ToList().OrderBy(o => o.SalesPerson);
                        
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
                }
                else
                {

                    var pid = (from x in db.ProTasks
                               where (location == "" || x.Location == location && (

                                 (fromdate == "" || EF.Functions.DateDiffDay(x.CreatedDate, fdate) <= 0) &&
                             (todates == "" || EF.Functions.DateDiffDay(x.CreatedDate, tdate) >= 0)



                               )
                               ) &&
                               (technician == null || technician == 0 || (from a in db.servicereports
                                                                          join
                                               b in db.servicereportmembers on a.servicereportid equals b.servicereportid
                                                                          where a.protaskid == x.ProTaskId && b.employeeid == technician
                                                                          select new { a.protaskid }).Any())






                               select new
                               {
                                   pids = x.ProTaskId
                               }).Select(o => o.pids).ToList().ToArray();

                    var salesids = (from x in db.SalesEntrys
                                    join y in db.additionaltasks on x.SalesEntryId equals y.salesentryid into yy
                                    from y in yy.DefaultIfEmpty()
                                    where (pid.Contains(y.taskid) || pid.Contains((long)x.ProTask))

                                    && (technician == null || technician == 0 || (
                                 (fromdate == "" || EF.Functions.DateDiffDay(x.SEDate, fdate) <= 0) &&
                             (todates == "" || EF.Functions.DateDiffDay(x.SEDate, tdate) >= 0)
                             ))
                                    select new
                                    {
                                        x.SalesEntryId
                                    }).Distinct().Select(o => o.SalesEntryId).ToList().ToArray();
                    var v = (from a in db.SalesEntrys

                             join b in db.Customers on a.Customer equals b.CustomerID into cust
                             from b in cust.DefaultIfEmpty()
                             join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                             join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                             from d in emp.DefaultIfEmpty()
                             join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                             from f in paymeth.DefaultIfEmpty()
                             join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                             from g in mcs.DefaultIfEmpty()
                             join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                             equals new { h1 = h.Reference, h2 = h.Section } into hir
                             from h in hir.DefaultIfEmpty()
                             join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                             from i in acc.DefaultIfEmpty()
                             join j in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = sac }
                             equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                             from j in hir1.DefaultIfEmpty()
                             join m in db.Employees on b.SalesPerson equals m.EmployeeId into empp
                             from m in empp.DefaultIfEmpty()
                             where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                            (customer == 0 || a.Customer == customer) &&
                            (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                            (type == null || a.CustomerType == sEntry.CustomerType) &&
                             (paymethod == null || a.PaymentMethod == paymethod) &&
                             salesids.Contains(a.SalesEntryId) &&
                             (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                            (htype == null || htype == null || h.HireType == htype) &&
                            (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                            (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)
                           && (ddMC == 0 || ddMC == null || a.MaterialCenter == ddMC) &&

                            (project == 0 || project == null || a.Project == project) &&
                            (task == 0 || task == null || a.ProTask == task) &&
                                 (SalesMan == 0 || b.SalesPerson == SalesMan) && j.Credit > 0 &&

                                 (source == 0 || source == null || b.SourceOfLead == source)

                             select new
                             {

                                 a.SalesEntryId,
                                 a.SENo,
                                 a.BillNo,
                                 a.SEDate,
                                 SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : 0,
                                 SETaxAmount = (srchtxt == "") ? a.SETaxAmount : 0,
                                 Credit = (srchtxt == "") ? j.Credit : 0,
                                 Customer = b.CustomerName,
                                 b.CustomerID,
                                 TaxRegNo = i.TRN,
                                 EmpName = d.FirstName + " " + d.LastName,
                                 SalesMan = m.FirstName + " " + m.LastName,
                                 MCName = g.MCName,
                                 SEPaidAmount = (srchtxt == "") ? c.SEPaidAmount : 0,
                                 a.CustomerType,
                                 SEBalanceAmount = (srchtxt == "") ? (a.SEGrandTotal - c.SEPaidAmount) : 0,
                                 //for expense
                                 PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == a.BillNo).Select(x => x.Paying).Sum()),
                                 salesreturns = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 != "Credit Note").Select(x => x.SRSubTotal).Sum()),
                                 salesreturnsnote = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 == "Credit Note").Select(x => x.SRGrandTotal).Sum()),

                                 discountt = (decimal?)(from ayy in db.BillSundrys
                                                        join azz in db.SEBillSundrys on ayy.BillSundryId equals azz.BillSundry

                                                        where ayy.BSName == "DISCOUNT" && azz.SalesEntry == a.SalesEntryId
                                                        select new
                                                        {
                                                            BsAmount = (azz.BsAmount == null) ? 0 : azz.BsAmount
                                                        }
                                                  ).Sum(x => x.BsAmount) ?? 0,
                                 //PaymentExpense = (decimal?)(from x in db.Payments

                                 //                            where (x.InvoiceNo == a.BillNo)
                                 //                                Expense = x.GrandTotal
                                 //JournalExpense = (decimal?)(from x in db.Journals

                                 //                            where (x.InvoiceNo == a.BillNo)
                                 //                                Expense = x.GrandTotal

                                 JournalExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == a.BillNo).Select(y => y.Paying).Sum()),

                                 //itemprice = (decimal?)(from se in db.SEItemss
                                 //                       where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
                                 //                           purprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * se.ItemQuantity) : ((seit.PurchasePrice * se.ItemQuantity) / seit.ConFactor)
                                 //           ).Sum(x => x.purprice) ?? 0,

                                 a.SECreatedDate,
                                 PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                                 SaleType = a.SaleType,
                                 FromDate = h.StartDate,
                                 ToDate = h.EndDate,
                                 HireType = h.HireType,
                                 a.SalesStatus

                             });

                    var vv = v.AsEnumerable().Select(o => new
                    {
                        o.SalesEntryId,
                        o.SENo,
                        o.Credit,
                        o.BillNo,
                        o.SEDate,
                        o.CustomerID,
                        o.SEGrandTotal,
                        o.SETaxAmount,
                        o.Customer,
                        o.TaxRegNo,
                        o.discountt,
                        o.EmpName,
                        o.MCName,
                        o.SEPaidAmount,
                        o.CustomerType,
                        o.SEBalanceAmount,
                        NewExpense = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
                                 ((o.PaymentExpense != null && o.JournalExpense != null) ? o.PaymentExpense + o.JournalExpense : ((o.PaymentExpense == null && o.JournalExpense != null) ? o.JournalExpense : o.PaymentExpense))) : 0,
                        NewExpenselink = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? "" :
                                 ((o.PaymentExpense != null && o.JournalExpense != null) ? String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>")) + " " + String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : ((o.PaymentExpense == null && o.JournalExpense != null) ? String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>"))))) : "",

                        salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
                        salesrtnnote = (o.salesreturnsnote == null) ? 0 : o.salesreturnsnote,

                        //Calling Function To Get Total Item Price for each Sales Entry
                        itemprice = (isemirtech == true) ? getusedprice(o.SalesEntryId) : GetTotalItemPrice(o.SalesEntryId, o.SEDate, srchtxt),
                        empcoste = (employeehourrate == true) ? (GetTotalEmpCost(o.SalesEntryId) * perhourcost) : GetTotalEmpCostwithrate(o.SalesEntryId),
                        mycoste = GetTotalEmpCost(o.SalesEntryId, technician) * perhourcost,
                        o.SECreatedDate,
                        o.PayMethod,
                        o.SaleType,
                        o.FromDate,
                        o.ToDate,
                        o.HireType,
                        o.SalesStatus,
                        o.SalesMan
                    }).Distinct().OrderBy(a => a.SEDate).ToList();
                    var data = vv.ToList();
                    recordsTotal = 100;


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
            }
            else
            {
                long[] MCArrays = { 20085, 20086, 20087 };
                string returnstr = "";
                foreach (var mcc in MCArrays)

                        {

                    ddMC = mcc;
                    if ((location == "All" || location == "") && (technician == null || technician == 0))
                    {


                        if (srchtxt == "")
                        {
                            var v = (from a in db.SalesEntrys
                                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                                     from b in cust.DefaultIfEmpty()
                                     join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                                     join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                                     from d in emp.DefaultIfEmpty()
                                     join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                                     from f in paymeth.DefaultIfEmpty()
                                     join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                                     from g in mcs.DefaultIfEmpty()
                                     join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                                     from h in hir.DefaultIfEmpty()
                                     join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                                     from i in acc.DefaultIfEmpty()
                                     join j in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = sac }
                                     equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                                     from j in hir1.DefaultIfEmpty()
                                     join m in db.Employees on b.SalesPerson equals m.EmployeeId into empp
                                     from m in empp.DefaultIfEmpty()
                                     where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                                    (customer == 0 || a.Customer == customer) &&
                                    (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                                    (type == null || a.CustomerType == sEntry.CustomerType) &&
                                     (paymethod == null || a.PaymentMethod == paymethod) &&
                                     (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                                     (todates == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                                     (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                                    (htype == null || htype == null || h.HireType == htype) &&
                                    (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                                    (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)
                                   && (ddMC == 0 || ddMC == null || a.MaterialCenter == ddMC) &&

                                    (project == 0 || project == null || a.Project == project) &&
                                    (task == 0 || task == null || a.ProTask == task) &&
                                         (SalesMan == 0 || b.SalesPerson == SalesMan) && j.Credit > 0 &&

                                         (source == 0 || source == null || b.SourceOfLead == source)

                                     select new
                                     {

                                         a.SalesEntryId,
                                         a.SENo,
                                         a.BillNo,
                                         a.SEDate,
                                         SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : 0,
                                         SETaxAmount = (srchtxt == "") ? a.SETaxAmount : 0,
                                         Credit = (srchtxt == "") ? j.Credit : 0,
                                         Customer = b.CustomerName,
                                         b.CustomerID,
                                         TaxRegNo = i.TRN,
                                         EmpName = d.FirstName + " " + d.LastName,
                                         SalesMan = m.FirstName + " " + m.LastName,
                                         MCName = g.MCName,
                                         SEPaidAmount = (srchtxt == "") ? c.SEPaidAmount : 0,
                                         a.CustomerType,
                                         SEBalanceAmount = (srchtxt == "") ? (a.SEGrandTotal - c.SEPaidAmount) : 0,
                                         //for expense
                                         PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == a.BillNo).Select(x => x.Paying).Sum()),
                                         salesreturns = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 != "Credit Note").Select(x => x.SRSubTotal).Sum()),
                                         salesreturnsnote = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 == "Credit Note").Select(x => x.SRGrandTotal).Sum()),

                                         discountt = (decimal?)(from ayy in db.BillSundrys
                                                                join azz in db.SEBillSundrys on ayy.BillSundryId equals azz.BillSundry

                                                                where ayy.BSName == "DISCOUNT" && azz.SalesEntry == a.SalesEntryId
                                                                select new
                                                                {
                                                                    BsAmount = (azz.BsAmount == null) ? 0 : azz.BsAmount
                                                                }
                                                          ).Sum(x => x.BsAmount) ?? 0,
                                         //PaymentExpense = (decimal?)(from x in db.Payments

                                         //                            where (x.InvoiceNo == a.BillNo)
                                         //                                Expense = x.GrandTotal
                                         //JournalExpense = (decimal?)(from x in db.Journals

                                         //                            where (x.InvoiceNo == a.BillNo)
                                         //                                Expense = x.GrandTotal

                                         JournalExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == a.BillNo).Select(y => y.Paying).Sum()),

                                         //itemprice = (decimal?)(from se in db.SEItemss
                                         //                       where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
                                         //                           purprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * se.ItemQuantity) : ((seit.PurchasePrice * se.ItemQuantity) / seit.ConFactor)
                                         //           ).Sum(x => x.purprice) ?? 0,

                                         a.SECreatedDate,
                                         PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                                         SaleType = a.SaleType,
                                         FromDate = h.StartDate,
                                         ToDate = h.EndDate,
                                         HireType = h.HireType,
                                         a.SalesStatus

                                     });

                            var vv = v.AsEnumerable().Select(o => new
                            {
                                o.SalesEntryId,
                                o.SENo,
                                o.Credit,
                                o.BillNo,
                                o.SEDate,
                                o.CustomerID,
                                o.SEGrandTotal,
                                o.SETaxAmount,
                                o.Customer,
                                o.TaxRegNo,
                                o.discountt,
                                o.EmpName,
                                o.MCName,
                                o.SEPaidAmount,
                                o.CustomerType,
                                o.SEBalanceAmount,
                                NewExpense = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
                                         ((o.PaymentExpense != null && o.JournalExpense != null) ? o.PaymentExpense + o.JournalExpense : ((o.PaymentExpense == null && o.JournalExpense != null) ? o.JournalExpense : o.PaymentExpense))) : 0,
                                NewExpenselink = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? "" :
                                         ((o.PaymentExpense != null && o.JournalExpense != null) ? String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>")) + " " + String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : ((o.PaymentExpense == null && o.JournalExpense != null) ? String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>"))))) : "",

                                salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
                                salesrtnnote = (o.salesreturnsnote == null) ? 0 : o.salesreturnsnote,

                                //Calling Function To Get Total Item Price for each Sales Entry
                                itemprice = (isemirtech == true) ? getusedprice(o.SalesEntryId) : GetTotalItemPrice(o.SalesEntryId, o.SEDate, srchtxt),
                                empcoste = (employeehourrate == true) ? (GetTotalEmpCost(o.SalesEntryId) * perhourcost) : GetTotalEmpCostwithrate(o.SalesEntryId),
                                o.SECreatedDate,
                                o.PayMethod,
                                o.SaleType,
                                o.FromDate,
                                o.ToDate,
                                o.HireType,
                                o.SalesStatus,
                                o.SalesMan
                            }).Distinct().OrderBy(a => a.SEDate).ToList();
                            var data = vv.ToList();
                            recordsTotal = 100;


                            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                            string result = javaScriptSerializer.Serialize(data);
                            returnstr = returnstr + result;
                        }
                        else
                        {
                            var v = (from a in db.SalesEntrys
                                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                                     from b in cust.DefaultIfEmpty()
                                     join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                                     join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                                     from d in emp.DefaultIfEmpty()
                                     join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                                     from f in paymeth.DefaultIfEmpty()
                                     join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                                     from g in mcs.DefaultIfEmpty()
                                     join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                                     from h in hir.DefaultIfEmpty()
                                     join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                                     from i in acc.DefaultIfEmpty()
                                     join j in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = sac }
                                     equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                                     from j in hir1.DefaultIfEmpty()
                                     join k in db.SEItemss on a.SalesEntryId equals k.SalesEntry into seit
                                     from k in seit.DefaultIfEmpty()
                                     join l in db.Items on k.Item equals l.ItemID into itemm
                                     from l in itemm.DefaultIfEmpty()
                                     join m in db.Employees on b.SalesPerson equals m.EmployeeId into empp
                                     from m in empp.DefaultIfEmpty()
                                     let grandtotalitmsearch = (decimal)(from m in db.SEItemss
                                                                         join nn in db.Items on m.Item equals nn.ItemID
                                                                         join oo in db.SalesEntrys on m.SalesEntry equals oo.SalesEntryId
                                                                         where nn.ItemName.Contains(srchtxt)
                                                                         && oo.SalesEntryId == a.SalesEntryId

                                                                         select new
                                                                         {
                                                                             totalprice = m.ItemUnitPrice * m.ItemQuantity
                                                                         }).Sum(o => o.totalprice)

                                     where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                                    (customer == 0 || a.Customer == customer) &&
                                    (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                                    (type == null || a.CustomerType == sEntry.CustomerType) &&
                                     (paymethod == null || a.PaymentMethod == paymethod) &&
                                     (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                                     (todates == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                                     (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                                    (htype == null || htype == null || h.HireType == htype) &&
                                    (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                                    (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)
                                    && (ddMC == 0 || ddMC == null || a.MaterialCenter == ddMC) &&

                                    (project == 0 || project == null || a.Project == project) &&
                                    (task == 0 || task == null || a.ProTask == task) &&
                                       (srchtxt == "" || l.ItemName.Contains(srchtxt))
                                        &&
                                         (source == 0 || source == null || b.SourceOfLead == source)

                                     select new
                                     {

                                         a.SalesEntryId,
                                         a.SENo,
                                         a.BillNo,
                                         a.SEDate,
                                         SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : grandtotalitmsearch,
                                         SETaxAmount = (srchtxt == "") ? a.SETaxAmount : 0,
                                         Credit = (srchtxt == "") ? j.Credit : grandtotalitmsearch,
                                         Customer = b.CustomerName,
                                         b.CustomerID,
                                         TaxRegNo = i.TRN,
                                         EmpName = d.FirstName + " " + d.LastName,
                                         MCName = g.MCName,
                                         SalesMan = m.FirstName + " " + m.LastName,
                                         SEPaidAmount = (srchtxt == "") ? c.SEPaidAmount : 0,
                                         a.CustomerType,
                                         SEBalanceAmount = (srchtxt == "") ? (a.SEGrandTotal - c.SEPaidAmount) : 0,
                                         //for expense
                                         PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == a.BillNo).Select(x => x.Paying).Sum()),
                                         salesreturns = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 != "Credit Note").Select(x => x.SRGrandTotal).Sum()),
                                         salesreturnsnote = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 == "Credit Note").Select(x => x.SRGrandTotal).Sum()),

                                         discountt = (from ay in db.BillSundrys
                                                      join az in db.SEBillSundrys on ay.BillSundryId equals az.BillSundry

                                                      where ay.BSName == "DISCOUNT" && az.SalesEntry == a.SalesEntryId
                                                      select new
                                                      {
                                                          az.BsAmount
                                                      }
                                                          ).Sum(x => x.BsAmount) ?? 0,
                                         //PaymentExpense = (decimal?)(from x in db.Payments

                                         //                            where (x.InvoiceNo == a.BillNo)
                                         //                                Expense = x.GrandTotal
                                         //JournalExpense = (decimal?)(from x in db.Journals

                                         //                            where (x.InvoiceNo == a.BillNo)
                                         //                                Expense = x.GrandTotal

                                         JournalExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == a.BillNo).Select(y => y.Paying).Sum()),
                                         //itemprice = (decimal?)(from se in db.SEItemss
                                         //                       where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
                                         //                           purprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * se.ItemQuantity) : ((seit.PurchasePrice * se.ItemQuantity) / seit.ConFactor)
                                         //           ).Sum(x => x.purprice) ?? 0,

                                         a.SECreatedDate,
                                         PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                                         SaleType = a.SaleType,
                                         FromDate = h.StartDate,
                                         ToDate = h.EndDate,
                                         HireType = h.HireType,
                                         a.SalesStatus

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
                                         o.discountt,
                                         o.EmpName,
                                         o.MCName,
                                         o.SEPaidAmount,
                                         o.CustomerType,
                                         o.SEBalanceAmount,
                                         o.SalesMan,
                                         NewExpense = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
                                         ((o.PaymentExpense != null) ? o.PaymentExpense : o.JournalExpense)) : 0,
                                         NewExpenselink = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? "" :
                                         ((o.PaymentExpense != null && o.JournalExpense != null) ? String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>")) + " " + String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : ((o.PaymentExpense == null && o.JournalExpense != null) ? String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>"))))) : "",

                                         salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
                                         salesrtnnote = (o.salesreturnsnote == null) ? 0 : o.salesreturnsnote,

                                         //Calling Function To Get Total Item Price for each Sales Entry
                                         itemprice = (isemirtech == true) ? getusedprice(o.SalesEntryId) : GetTotalItemPrice(o.SalesEntryId, o.SEDate, srchtxt),
                                         empcoste = GetTotalEmpCost(o.SalesEntryId) * perhourcost,
                                         o.SECreatedDate,
                                         o.PayMethod,
                                         o.SaleType,
                                         o.FromDate,
                                         o.ToDate,
                                         o.CustomerID,
                                         o.HireType,
                                         o.SalesStatus,
                                     }).OrderBy(a => a.SEDate).ThenBy(a => a.SECreatedDate);
                            var data = v.ToList();
                            recordsTotal = v.Count();


                            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                            string result = javaScriptSerializer.Serialize(new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                            returnstr = returnstr + result;
                        }
                    }
                    else
                    {

                        var pid = (from x in db.ProTasks
                                   where (location == "" || x.Location == location && (

                                     (fromdate == "" || EF.Functions.DateDiffDay(x.CreatedDate, fdate) <= 0) &&
                                 (todates == "" || EF.Functions.DateDiffDay(x.CreatedDate, tdate) >= 0)



                                   )
                                   ) &&
                                   (technician == null || technician == 0 || (from a in db.servicereports
                                                                              join
                                                   b in db.servicereportmembers on a.servicereportid equals b.servicereportid
                                                                              where a.protaskid == x.ProTaskId && b.employeeid == technician
                                                                              select new { a.protaskid }).Any())






                                   select new
                                   {
                                       pids = x.ProTaskId
                                   }).Select(o => o.pids).ToList().ToArray();

                        var salesids = (from x in db.SalesEntrys
                                        join y in db.additionaltasks on x.SalesEntryId equals y.salesentryid into yy
                                        from y in yy.DefaultIfEmpty()
                                        where (pid.Contains(y.taskid) || pid.Contains((long)x.ProTask))

                                        && (technician == null || technician == 0 || (
                                     (fromdate == "" || EF.Functions.DateDiffDay(x.SEDate, fdate) <= 0) &&
                                 (todates == "" || EF.Functions.DateDiffDay(x.SEDate, tdate) >= 0)
                                 ))
                                        select new
                                        {
                                            x.SalesEntryId
                                        }).Distinct().Select(o => o.SalesEntryId).ToList().ToArray();
                        var v = (from a in db.SalesEntrys

                                 join b in db.Customers on a.Customer equals b.CustomerID into cust
                                 from b in cust.DefaultIfEmpty()
                                 join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                                 join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                                 from d in emp.DefaultIfEmpty()
                                 join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                                 from f in paymeth.DefaultIfEmpty()
                                 join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                                 from g in mcs.DefaultIfEmpty()
                                 join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                                 equals new { h1 = h.Reference, h2 = h.Section } into hir
                                 from h in hir.DefaultIfEmpty()
                                 join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                                 from i in acc.DefaultIfEmpty()
                                 join j in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = sac }
                                 equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                                 from j in hir1.DefaultIfEmpty()
                                 join m in db.Employees on b.SalesPerson equals m.EmployeeId into empp
                                 from m in empp.DefaultIfEmpty()
                                 where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                                (customer == 0 || a.Customer == customer) &&
                                (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                                (type == null || a.CustomerType == sEntry.CustomerType) &&
                                 (paymethod == null || a.PaymentMethod == paymethod) &&
                                 salesids.Contains(a.SalesEntryId) &&
                                 (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                                (htype == null || htype == null || h.HireType == htype) &&
                                (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                                (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)
                               && (ddMC == 0 || ddMC == null || a.MaterialCenter == ddMC) &&

                                (project == 0 || project == null || a.Project == project) &&
                                (task == 0 || task == null || a.ProTask == task) &&
                                     (SalesMan == 0 || b.SalesPerson == SalesMan) && j.Credit > 0 &&

                                     (source == 0 || source == null || b.SourceOfLead == source)

                                 select new
                                 {

                                     a.SalesEntryId,
                                     a.SENo,
                                     a.BillNo,
                                     a.SEDate,
                                     SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : 0,
                                     SETaxAmount = (srchtxt == "") ? a.SETaxAmount : 0,
                                     Credit = (srchtxt == "") ? j.Credit : 0,
                                     Customer = b.CustomerName,
                                     b.CustomerID,
                                     TaxRegNo = i.TRN,
                                     EmpName = d.FirstName + " " + d.LastName,
                                     SalesMan = m.FirstName + " " + m.LastName,
                                     MCName = g.MCName,
                                     SEPaidAmount = (srchtxt == "") ? c.SEPaidAmount : 0,
                                     a.CustomerType,
                                     SEBalanceAmount = (srchtxt == "") ? (a.SEGrandTotal - c.SEPaidAmount) : 0,
                                     //for expense
                                     PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == a.BillNo).Select(x => x.Paying).Sum()),
                                     salesreturns = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 != "Credit Note").Select(x => x.SRSubTotal).Sum()),
                                     salesreturnsnote = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 == "Credit Note").Select(x => x.SRGrandTotal).Sum()),

                                     discountt = (decimal?)(from ayy in db.BillSundrys
                                                            join azz in db.SEBillSundrys on ayy.BillSundryId equals azz.BillSundry

                                                            where ayy.BSName == "DISCOUNT" && azz.SalesEntry == a.SalesEntryId
                                                            select new
                                                            {
                                                                BsAmount = (azz.BsAmount == null) ? 0 : azz.BsAmount
                                                            }
                                                      ).Sum(x => x.BsAmount) ?? 0,
                                     //PaymentExpense = (decimal?)(from x in db.Payments

                                     //                            where (x.InvoiceNo == a.BillNo)
                                     //                                Expense = x.GrandTotal
                                     //JournalExpense = (decimal?)(from x in db.Journals

                                     //                            where (x.InvoiceNo == a.BillNo)
                                     //                                Expense = x.GrandTotal

                                     JournalExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == a.BillNo).Select(y => y.Paying).Sum()),

                                     //itemprice = (decimal?)(from se in db.SEItemss
                                     //                       where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
                                     //                           purprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * se.ItemQuantity) : ((seit.PurchasePrice * se.ItemQuantity) / seit.ConFactor)
                                     //           ).Sum(x => x.purprice) ?? 0,

                                     a.SECreatedDate,
                                     PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                                     SaleType = a.SaleType,
                                     FromDate = h.StartDate,
                                     ToDate = h.EndDate,
                                     HireType = h.HireType,
                                     a.SalesStatus

                                 });

                        var vv = v.AsEnumerable().Select(o => new
                        {
                            o.SalesEntryId,
                            o.SENo,
                            o.Credit,
                            o.BillNo,
                            o.SEDate,
                            o.CustomerID,
                            o.SEGrandTotal,
                            o.SETaxAmount,
                            o.Customer,
                            o.TaxRegNo,
                            o.discountt,
                            o.EmpName,
                            o.MCName,
                            o.SEPaidAmount,
                            o.CustomerType,
                            o.SEBalanceAmount,
                            NewExpense = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
                                     ((o.PaymentExpense != null && o.JournalExpense != null) ? o.PaymentExpense + o.JournalExpense : ((o.PaymentExpense == null && o.JournalExpense != null) ? o.JournalExpense : o.PaymentExpense))) : 0,
                            NewExpenselink = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? "" :
                                     ((o.PaymentExpense != null && o.JournalExpense != null) ? String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>")) + " " + String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : ((o.PaymentExpense == null && o.JournalExpense != null) ? String.Join(",", db.Journals.Where(y => y.InvoiceNo == o.BillNo).Select(j => j.JournalId).ToList().Select(jid => "<a href=\"/JournalV/details/" + jid + "\" class=\"detailsclass\">Link</a>")) : String.Join(",", db.Payments.Where(p => p.InvoiceNo == o.BillNo).Select(p => p.PaymentId).ToList().Select(pid => "<a href=\"/Payment/details/" + pid + "\" class=\"detailsclass\">Link</a>"))))) : "",

                            salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
                            salesrtnnote = (o.salesreturnsnote == null) ? 0 : o.salesreturnsnote,

                            //Calling Function To Get Total Item Price for each Sales Entry
                            itemprice = (isemirtech == true) ? getusedprice(o.SalesEntryId) : GetTotalItemPrice(o.SalesEntryId, o.SEDate, srchtxt),
                            empcoste = (employeehourrate == true) ? (GetTotalEmpCost(o.SalesEntryId) * perhourcost) : GetTotalEmpCostwithrate(o.SalesEntryId),
                            mycoste = GetTotalEmpCost(o.SalesEntryId, technician) * perhourcost,
                            o.SECreatedDate,
                            o.PayMethod,
                            o.SaleType,
                            o.FromDate,
                            o.ToDate,
                            o.HireType,
                            o.SalesStatus,
                            o.SalesMan
                        }).Distinct().OrderBy(a => a.SEDate).ToList();
                        var data = vv.ToList();
                        recordsTotal = 100;


                        JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                        javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                        string result = javaScriptSerializer.Serialize(new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                        returnstr = returnstr + result ;

                        
                       

                    }

                }
                var results = new ContentResult
                {
                    Content = returnstr,
                    ContentType = "application/json"
                };
                return results;
            }
        
        }
        public Dictionary<string,object> getusedprice(long salesid)
        {

            var v = (from a in db.SalesEntrys
                     join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                     join c in db.Customers on a.Customer equals c.CustomerID
                     join d in db.ProTasks on a.ProTask equals d.ProTaskId into task
                     from d in task.DefaultIfEmpty()
                     join e in db.Items on b.Item equals e.ItemID
                     join f in db.ItemUnits on b.ItemUnit equals f.ItemUnitID into unit
                     from f in unit.DefaultIfEmpty()
                     where
                     a.SalesEntryId ==salesid
                      && b.Type == true
                     select new
                     {
                         a.SalesEntryId,
                         e.ItemName,
                         f.ItemUnitName,
                         b.ItemQuantity,

                         b.ItemSubTotal,
                         b.ItemTaxAmount,
                         e.ConFactor,
                         e.SubUnitId,
                         e.ItemUnitID,
                         b.ItemUnit,
                         ItemUnitPrice = b.ItemUnitPrice,// (e.SubUnitId == b.ItemUnit) ? e.SellingPrice / e.ConFactor : e.SellingPrice,
                     }).ToList().Select(o => new
                     {
                         o.SalesEntryId,
                         o.ItemName,
                         o.ItemUnitName,
                         o.ItemQuantity,
                         //ItemUnitPrice=(o.SubUnitId==o.ItemUnit)?o.ItemUnitPrice/o.ConFactor:o.ItemUnitPrice,
                         o.ItemUnitPrice,
                         o.ItemSubTotal,
                         o.ItemTaxAmount,
                         Total = o.ItemUnitPrice * o.ItemQuantity
                     }).Sum(o=>o.Total);
            Dictionary<string, Object> ret = new Dictionary<string, object>();
            ret.Add("itemprice", v);
            ret.Add("loss", false);
            return ret;
        }
        public ActionResult GetAllSaleprofitCASA(string seno, long? paymethod, long? customer, long? SalesExecutive, string fromdate, string todate, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate, long? project, long? task)
        {
            db.SetCommandTimeOut(60 * 60);
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (ddMC == null)
                ddMC = 0;
            if (!MCList.Any() && (ddMC == 0 || ddMC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            int recordsTotal = 0;

            SaleType St = new SaleType();
            if (satype != "")
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
            if (!string.IsNullOrEmpty(hfdate))
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
            var v = (from a in db.SalesEntrys
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                     join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                     join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                     from f in paymeth.DefaultIfEmpty()
                     join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                     from g in mcs.DefaultIfEmpty()
                     join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                     from h in hir.DefaultIfEmpty()
                     join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()
                     join j in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = sac }
                     equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                     from j in hir1.DefaultIfEmpty()

                     where (seno == "" || a.BillNo == seno) && a.Status == 1 &&
                    (customer == 0 || a.Customer == customer) &&
                    (SalesExecutive == 0 || SalesExecutive == null || a.SECashier == SalesExecutive) &&
                    (type == null || a.CustomerType == sEntry.CustomerType) &&
                     (paymethod == null || a.PaymentMethod == paymethod) &&
                     (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                     (todate == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0) &&
                     (string.IsNullOrEmpty(satype) || a.SaleType == St) &&
                    (htype == null || htype == null || h.HireType == htype) &&
                    (string.IsNullOrEmpty(hfdate) || EF.Functions.DateDiffDay(h.StartDate, hfrmdate) <= 0) &&
                    (string.IsNullOrEmpty(htdate) || EF.Functions.DateDiffDay(h.EndDate, htodate) >= 0)
                    && ((!MCList.Any() && ddMC == 0) || MCArray.Contains(a.MaterialCenter) || ddMC == a.MaterialCenter) &&
                    (project == 0 || project == null || a.Project == project) &&
                    (task == 0 || task == null || a.ProTask == task)
                     select new
                     {
                         a.SalesEntryId,
                         a.SENo,
                         a.BillNo,
                         a.SEDate,
                         a.SEGrandTotal,
                         a.SETaxAmount,
                         j.Credit,
                         Customer = b.CustomerName,
                         TaxRegNo = i.TRN,
                         EmpName = d.FirstName + " " + d.LastName,
                         MCName = g.MCName,
                         c.SEPaidAmount,
                         a.CustomerType,
                         SEBalanceAmount = a.SEGrandTotal - c.SEPaidAmount,
                         //for expense
                         PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == a.BillNo).Select(x => x.Paying).Sum()),
                         salesreturns = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId).Select(x => x.SRSubTotal).Sum()),

                         //PaymentExpense = (decimal?)(from x in db.Payments

                         //                            where (x.InvoiceNo == a.BillNo)
                         //                                Expense = x.GrandTotal
                         //JournalExpense = (decimal?)(from x in db.Journals

                         //                            where (x.InvoiceNo == a.BillNo)
                         //                                Expense = x.GrandTotal

                         JournalExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == a.BillNo).Select(y => y.Paying).Sum()),

                         itemprice = (decimal?)(from se in db.SEItemss
                                                join seen in db.SalesEntrys on se.SalesEntry equals seen.SalesEntryId
                                                join seit in db.Items on se.Item equals seit.ItemID
                                                where se.SalesEntry == a.SalesEntryId && seit.KeepStock == true
                                                select new
                                                {
                                                    purprice = (se.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * se.ItemQuantity) : ((seit.PurchasePrice * se.ItemQuantity) / seit.ConFactor)
                                                }
                                    ).Sum(x => x.purprice) ?? 0,

                         a.SECreatedDate,
                         PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                         SaleType = a.SaleType,
                         FromDate = h.StartDate,
                         ToDate = h.EndDate,
                         HireType = h.HireType,
                         a.Ref1

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
                         NewExpense = (o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
                         ((o.PaymentExpense != null) ? o.PaymentExpense : o.JournalExpense),
                         salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
                         //Calling Function To Get Total Item Price for each Sales Entry
                         o.itemprice,

                         o.SECreatedDate,
                         o.PayMethod,
                         o.SaleType,
                         o.FromDate,
                         o.ToDate,
                         o.HireType,
                         o.Ref1
                     }).OrderBy(a => a.SEDate).ThenBy(a => a.SECreatedDate);

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
        public double GetTotalEmpCostwithrate(long salesid)
        {
            var tsk = db.SalesEntrys.Where(o => o.SalesEntryId == salesid).Select(o => o.ProTask).FirstOrDefault();
            var task = (tsk == null) ? 0 : tsk;
            var addtas = db.additionaltasks.Where(o => o.salesentryid == salesid).Select(o => o.taskid).ToList();
            addtas.Add((long)task);
            if (task == null || task == 0)
                return 0;
            else
            {
                var data = (from a in db.servicereports
                            join b in db.servicereportmembers on a.servicereportid equals b.servicereportid
                            join c in db.Employees on b.employeeid equals c.EmployeeId
                            where addtas.Contains(a.protaskid)
                            select new
                            {
                                a.starttime,
                                endtime = (a.endtime < a.starttime) ? a.endtime.Value.AddDays(1) : a.endtime,
                                hourlyrate = (c.perhour == null) ? 0 : c.perhour,
                            }).ToList();
                double totalminute = 0;
                for (var i = 0; i < data.Count(); i++)
                {
                    totalminute = totalminute + (((DateTime)data[i].endtime - (DateTime)data[i].starttime).TotalHours * Convert.ToDouble(data[i].hourlyrate));

                }
                return totalminute;


            }
        }
        public double GetTotalEmpCost(long salesid,long? technician=0)
        {
            var tsk = db.SalesEntrys.Where(o => o.SalesEntryId == salesid).Select(o => o.ProTask).FirstOrDefault();
            var task = (tsk == null) ? 0 : tsk;
            var addtas = db.additionaltasks.Where(o => o.salesentryid == salesid).Select(o => o.taskid).ToList();
            addtas.Add((long)task);
            if (task == null || task == 0)
                return 0;
            else
            {
                var data = (from a in db.servicereports
                            join b in db.servicereportmembers on a.servicereportid equals b.servicereportid
                            where addtas.Contains(a.protaskid) &&
                            (technician==0||technician==null||b.employeeid==technician)
                            select new
                            {
                                a.starttime,
                                endtime = (a.endtime < a.starttime) ? a.endtime.Value.AddDays(1) : a.endtime
                            }).ToList();
                TimeSpan totalminute = TimeSpan.Zero;
                for (var i = 0; i < data.Count(); i++)
                {
                    totalminute = totalminute + ((DateTime)data[i].endtime - (DateTime)data[i].starttime);

                }
                return totalminute.TotalHours;

            }

        }
        public Dictionary<string, Object> GetTotalItemPricenewnew(long? SalesEntryId, DateTime? SEDate, string srchtxt, IEnumerable<SalesEntry> salesentrys = null, List<salesreturnmodel> salereturnids = null,bool nostocktransfer=false)
        {
   
         
            

            var ItemList = (from se in db.SEItemss
                            join seen in salesentrys on se.SalesEntry equals seen.SalesEntryId
                            join seit in db.Items on se.Item equals seit.ItemID



                            where se.SalesEntry == SalesEntryId && (seit.KeepStock == true || seit.accmap == true) &&
                            (srchtxt == "" || seit.ItemName.Contains(srchtxt))
                            select new
                            {
                                SEDate = seen.SECreatedDate,
                                DetailId = se.SEItemsId,
                                ItemId = se.Item,
                                itemcode = seit.ItemCode,
                                seItemUnit = se.ItemUnit,
                                seen.SalesEntryId,
                                seItemQuantity = se.ItemQuantity,
                                seitItemUnitID = seit.ItemUnitID,
                                seitPurchasePrice = seit.PurchasePrice,
                                seitConFactor = seit.ConFactor,
                                seen.MaterialCenter,
                                orgsellingprice = se.ItemUnitPrice,
                                se.Type,
                                ItemUnitPrice = (seit.SubUnitId == se.ItemUnit) ? seit.SellingPrice / seit.ConFactor : seit.SellingPrice,

                            }).Distinct();
            var ItemList2 = ItemList.ToList().Select(o => new
            {
                o.SEDate,
                o.DetailId,
                o.ItemId,
                o.seItemUnit,
                o.seItemQuantity,
                o.seitItemUnitID,
                o.seitPurchasePrice,
                o.seitConFactor,
                o.orgsellingprice,
                o.SalesEntryId,
                o.Type,
                o.itemcode,
                retnqty =salereturnids.Where(k=>k.saleid==o.SalesEntryId && k.itid==o.ItemId).Select(k=>k.qty).FirstOrDefault(), //getsalesreturn(o.SalesEntryId, o.ItemId),
                ItemUnitPrice = o.Type == true ? o.ItemUnitPrice : 0,
                //Calling Function To Get Item Purchase Price (If Exists Any With in SEDate)for each Item
                NewPurchPrice = GetItemPurchasePricenewnew(o.ItemId, o.SEDate, o.MaterialCenter, o.DetailId,false,0,salereturnids, nostocktransfer)

            }).Select(s => new
            {
                s.SEDate,
                s.DetailId,
                s.ItemId,
                s.seItemUnit,
                s.seItemQuantity,
                s.seitItemUnitID,
                s.orgsellingprice,
                s.seitPurchasePrice,
                s.seitConFactor,
                s.ItemUnitPrice,
                s.Type,
                s.retnqty,
                s.itemcode,
                //Calculating ItemPrice * Quantity(If Secondary Unit Exists ==> Considering Conversion Factor)
                ItemPrice = (1 == 2) ? Math.Round(Math.Round(s.ItemUnitPrice, 2) * (s.seItemQuantity - s.retnqty), 2) : ((s.seItemUnit == s.seitItemUnitID) ? ((s.NewPurchPrice< 0) ? (s.seitPurchasePrice * (s.seItemQuantity - s.retnqty)) : (s.NewPurchPrice * (s.seItemQuantity - s.retnqty))) : (((s.NewPurchPrice == 0) ? (s.seitPurchasePrice * (s.seItemQuantity - s.retnqty)) : (s.NewPurchPrice * (s.seItemQuantity - s.retnqty))) / s.seitConFactor))
                //ItemPrice = (s.NewPurchPrice * (s.seItemQuantity - s.retnqty)) 

            }).ToList();

            var j = 0;
            decimal ItemPrice = 0;
            bool loss = false;
            string itemcodeandprice = "";
            //Taking Sum of Item Price ==>  Item Price Of Each Item
            for (j = 0; j < ItemList2.Count; j++)
            {
                //#if DEBUG
                //#endif

                ItemPrice = Convert.ToDecimal(ItemPrice + ItemList2[j].ItemPrice);
                if (ItemList2[j].seItemQuantity > 0)
                {
                    if ((ItemList2[j].ItemPrice / ItemList2[j].seItemQuantity) > ItemList2[j].orgsellingprice)
                    {
                        itemcodeandprice = itemcodeandprice + " , Item Code : " + ItemList2[j].itemcode + " : Entered Price :  " + Math.Round(ItemList2[j].orgsellingprice, 2).ToString() + " : Orginal Price " + Math.Round((ItemList2[j].ItemPrice / ItemList2[j].seItemQuantity), 2).ToString();
                        loss = true;

                    }
                }
            }

            Dictionary<string, Object> ret = new Dictionary<string, object>();
            ret.Add("itemprice", ItemPrice);
            ret.Add("loss", itemcodeandprice);
            return ret;
        }

        //Function To Return Total Item Price for each Sales Entry
        public Dictionary<string, Object> GetTotalItemPrice(long? SalesEntryId, DateTime? SEDate, string srchtxt)
        {

            //Getting All Items In Sales Entry
         
            var ItemList = (from se in db.SEItemss
                            join seen in db.SalesEntrys on se.SalesEntry equals seen.SalesEntryId
                            join seit in db.Items on se.Item equals seit.ItemID



                            where se.SalesEntry == SalesEntryId && (seit.KeepStock == true || seit.accmap == true) &&
                            (srchtxt == "" || seit.ItemName.Contains(srchtxt))
                            select new
                            {
                                SEDate = seen.SECreatedDate,
                                DetailId = se.SEItemsId,
                                ItemId = se.Item,
                                itemcode=seit.ItemCode,
                                seItemUnit = se.ItemUnit,
                                seen.SalesEntryId,
                                seItemQuantity = se.ItemQuantity,
                                seitItemUnitID = seit.ItemUnitID,
                                seitPurchasePrice = seit.PurchasePrice,
                                seitConFactor = seit.ConFactor,
                                seen.MaterialCenter,
                                orgsellingprice=se.ItemUnitPrice,
                                se.Type,
                                ItemUnitPrice = (seit.SubUnitId == se.ItemUnit) ? seit.SellingPrice / seit.ConFactor : seit.SellingPrice,

                            }).Distinct();
            var ItemList2 = ItemList.ToList().Select(o => new
            {
                o.SEDate,
                o.DetailId,
                o.ItemId,
                o.seItemUnit,
                o.seItemQuantity,
                o.seitItemUnitID,
                o.seitPurchasePrice,
                o.seitConFactor,
                o.orgsellingprice,
                o.SalesEntryId,
                o.Type,
                o.itemcode,
                retnqty = getsalesreturn(o.SalesEntryId, o.ItemId),
                ItemUnitPrice = o.Type == true ? o.ItemUnitPrice : 0,
                //Calling Function To Get Item Purchase Price (If Exists Any With in SEDate)for each Item
                NewPurchPrice = GetItemPurchasePrice(o.ItemId, o.SEDate, o.MaterialCenter, o.DetailId)

            }).Select(s => new
            {
                s.SEDate,
                s.DetailId,
                s.ItemId,
                s.seItemUnit,
                s.seItemQuantity,
                s.seitItemUnitID,
                s.orgsellingprice,
                s.seitPurchasePrice,
                s.seitConFactor,
                s.ItemUnitPrice,
                s.Type,
                s.retnqty,
                s.itemcode,
                //Calculating ItemPrice * Quantity(If Secondary Unit Exists ==> Considering Conversion Factor)
                ItemPrice = (1 == 2) ? Math.Round(Math.Round(s.ItemUnitPrice, 2) * (s.seItemQuantity - s.retnqty), 2) : ((s.seItemUnit == s.seitItemUnitID) ? ((s.NewPurchPrice == 0) ? (s.seitPurchasePrice * (s.seItemQuantity - s.retnqty)) : (s.NewPurchPrice * (s.seItemQuantity - s.retnqty))) : (((s.NewPurchPrice == 0) ? (s.seitPurchasePrice * (s.seItemQuantity - s.retnqty)) : (s.NewPurchPrice * (s.seItemQuantity - s.retnqty))) / s.seitConFactor))
                //ItemPrice = (s.NewPurchPrice * (s.seItemQuantity - s.retnqty)) 
                
            }).ToList();

            var j = 0;
            decimal ItemPrice = 0;
            bool loss = false;
            string itemcodeandprice = "";
            //Taking Sum of Item Price ==>  Item Price Of Each Item
            for (j = 0; j < ItemList2.Count; j++)
            {
                //#if DEBUG
                //#endif
              
                ItemPrice = Convert.ToDecimal(ItemPrice + ItemList2[j].ItemPrice);
                if ((ItemList2[j].ItemPrice / ItemList2[j].seItemQuantity) > ItemList2[j].orgsellingprice)
                {
                    itemcodeandprice = itemcodeandprice +" , Item Code : " + ItemList2[j].itemcode + " : Entered Price :  " + Math.Round(ItemList2[j].orgsellingprice,2).ToString()+ " : Orginal Price "+ Math.Round((ItemList2[j].ItemPrice / ItemList2[j].seItemQuantity),2).ToString();
                    loss = true;

                }
            }

           Dictionary<string,Object> ret= new Dictionary<string, object>();
            ret.Add("itemprice", ItemPrice);
            ret.Add("loss", itemcodeandprice);
            return ret;
        }
        public Dictionary<string,Object> getcachedprice(decimal? mccost)
        {
            Dictionary<string, Object> ret = new Dictionary<string, object>();
            ret.Add("itemprice", mccost);
            ret.Add("loss", "");
            return ret;
        }
        public bool salesentrycontaintemp(long salesentryid)
        {
            var tempexist = db.SEItemss.Any(o => o.Item == 75021 && o.SalesEntry == salesentryid);
                return tempexist;
        }
        public bool salesentrycontainret(long salesentryid)
        {
            var tempexist = db.SalesReturns.Any(o=>o.SalesEntryId == salesentryid);
            return tempexist;
        }
        
        public Dictionary<string, Object> GetTotalItemPricenew(long? SalesEntryId, DateTime? SEDate, long? ddMC ,string srchtxt)
        {
            ddMC = db.SalesEntrys.Find(SalesEntryId).MaterialCenter;
              var selitem = new SqlParameter("@ItemId", 1);
            var selmc = new SqlParameter("@MCId", (object)ddMC ?? DBNull.Value);
            var brand = new SqlParameter("@BrandId", "0");
            var stkble = new SqlParameter("@Stockble", "");
            var catgry = new SqlParameter("@CategoryId", "0");
            var fromdatee = new SqlParameter("@fromdate", SEDate.Value.AddMonths(-3));
            var todate = new SqlParameter("@todate", SEDate.Value.AddDays(30));
            var stype = new SqlParameter("@Stype", "0");
            var saleid = new SqlParameter("@salesentryid", (object)SalesEntryId ?? DBNull.Value);
            var datamovement = db.Database.SqlQueryDedup<StockDataDetails>("SP_AVCOMethodprofit @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype,@salesentryid", selitem, selmc, brand, stkble, catgry, fromdatee, todate, stype, saleid).AsEnumerable().OrderBy(a => a.TDate).ToList();
            
            //Getting All Items In Sales Entry
            var ItemList = (from se in db.SEItemss
                            join seen in db.SalesEntrys on se.SalesEntry equals seen.SalesEntryId
                            join seit in db.Items on se.Item equals seit.ItemID

                          

            where se.SalesEntry == SalesEntryId && (seit.KeepStock == true || seit.accmap == true) &&
                            (srchtxt == "" || seit.ItemName.Contains(srchtxt))
                            select new
                            {
                                SEDate = seen.SEDate,
                                DetailId = se.SEItemsId,
                                ItemId = se.Item,
                                itemcode = seit.ItemCode,
                                seItemUnit = se.ItemUnit,
                                seen.SalesEntryId,
                                seItemQuantity = se.ItemQuantity,
                                seitItemUnitID = seit.ItemUnitID,
                                seitPurchasePrice = seit.PurchasePrice,
                                seitConFactor = seit.ConFactor,
                                seen.MaterialCenter,
                                orgsellingprice = se.ItemUnitPrice,
                                se.Type,
                               
                                ItemUnitPrice = (seit.SubUnitId == se.ItemUnit) ? seit.SellingPrice / seit.ConFactor : seit.SellingPrice,

                            }).Distinct();
            var ItemList2 = ItemList.ToList().Select(o => new
            {
                o.SEDate,
                o.DetailId,
                o.ItemId,
                o.seItemUnit,
                o.seItemQuantity,
                o.seitItemUnitID,
                o.seitPurchasePrice,
                o.seitConFactor,
                o.orgsellingprice,
                o.SalesEntryId,
                o.Type,
                o.itemcode,
               
                retnqty = getsalesreturn(o.SalesEntryId, o.ItemId),
                ItemUnitPrice = o.Type == true ? o.ItemUnitPrice : 0,
                //Calling Function To Get Item Purchase Price (If Exists Any With in SEDate)for each Item
                NewPurchPrice = GetItemPurchasePricenew(o.ItemId, o.SEDate, o.MaterialCenter, o.DetailId, datamovement)

            }).Select(s => new
            {
                s.SEDate,
                s.DetailId,
                s.ItemId,
                s.seItemUnit,
                s.seItemQuantity,
                s.seitItemUnitID,
                s.orgsellingprice,
                s.seitPurchasePrice,
                s.seitConFactor,
                s.ItemUnitPrice,
                s.Type,
                s.retnqty,
                s.itemcode,
                //Calculating ItemPrice * Quantity(If Secondary Unit Exists ==> Considering Conversion Factor)
                ItemPrice = (1 == 2) ? Math.Round(Math.Round(s.ItemUnitPrice, 2) * (s.seItemQuantity - s.retnqty), 2) : ((s.seItemUnit == s.seitItemUnitID) ? ((s.NewPurchPrice == 0) ? (s.seitPurchasePrice * (s.seItemQuantity - s.retnqty)) : (s.NewPurchPrice * (s.seItemQuantity - s.retnqty))) : (((s.NewPurchPrice == 0) ? (s.seitPurchasePrice * (s.seItemQuantity - s.retnqty)) : (s.NewPurchPrice * (s.seItemQuantity - s.retnqty))) / s.seitConFactor))
                //ItemPrice = (s.NewPurchPrice * (s.seItemQuantity - s.retnqty)) 

            }).ToList();

            var j = 0;
            decimal ItemPrice = 0;
            bool loss = false;
            string itemcodeandprice = "";
            //Taking Sum of Item Price ==>  Item Price Of Each Item
            for (j = 0; j < ItemList2.Count; j++)
            {
                //#if DEBUG
                //#endif

                ItemPrice = Convert.ToDecimal(ItemPrice + ItemList2[j].ItemPrice);
                if ((ItemList2[j].ItemPrice / ItemList2[j].seItemQuantity) > ItemList2[j].orgsellingprice)
                {
                    itemcodeandprice = itemcodeandprice + " , Item Code : " + ItemList2[j].itemcode + " : Entered Price :  " + Math.Round(ItemList2[j].orgsellingprice, 2).ToString() + " : Orginal Price " + Math.Round((ItemList2[j].ItemPrice / ItemList2[j].seItemQuantity), 2).ToString();
                    loss = true;

                }
            }
      
          
          

            Dictionary<string, Object> ret = new Dictionary<string, object>();
            ret.Add("itemprice", ItemPrice);
            ret.Add("loss", itemcodeandprice);
            return ret;
        }
        
        public decimal getsalesreturn(long salesentryid, long itemid)
        {
            var v = (from a in db.SalesReturns
                     join b in db.SRItemss on a.SalesReturnId equals b.SalesReturnId
                     where a.SalesEntryId == salesentryid && b.Item == itemid
                     select new
                     {
                         b.ItemQuantity
                     }
                  ).ToList();
            if (v.Count() <= 0)
                return 0;
            else if (v.Count() == 1)
                return v.Sum(o => o.ItemQuantity);
            else
            {
                return v.Average(o => o.ItemQuantity);
            }

        }
        public decimal getsalesreturndetailid(string billno, long itemid)
        {
            var exist = (from a in db.SalesEntrys
                         join b in db.SalesReturns on a.SalesEntryId equals b.SalesEntryId
                         join c in db.SRItemss on b.SalesReturnId equals c.SalesReturnId
                         where a.BillNo == billno && c.Item == itemid
                         select new
                         { c.ItemQuantity })
                         .Any();
            if (!exist)
            {
                return 0;
            }
            var salesentryid = db.SalesEntrys.Where(o => o.BillNo == billno).Select(o => o.SalesEntryId).FirstOrDefault();
            var v = (from a in db.SalesReturns
                     join b in db.SRItemss on a.SalesReturnId equals b.SalesReturnId
                     join c in db.Items on b.Item equals c.ItemID
                     where a.SalesEntryId == salesentryid && b.Item == itemid
                     select new
                     {
                         ItemQuantity = (c.ItemUnitID == b.ItemUnit) ? b.ItemQuantity * c.ConFactor : b.ItemQuantity
                     }
                  ).ToList();
            if (v.Count() <= 0)
                return 0;
            else if (v.Count() == 1)
                return v.Sum(o => o.ItemQuantity);
            else
            {
                return v.Average(o => o.ItemQuantity);
            }

        }
        //Function To Get Item Purchase Price (If Any Exists With in SEDate)

        public decimal salesreturnseid(long salesreturnsrid, long itemid)
        {
            var v = (from a in db.SalesReturns
                     join b in db.SRItemss on a.SalesReturnId equals b.SalesReturnId
                     join c in db.SalesEntrys on a.SalesEntryId equals c.SalesEntryId
                     join d in db.SEItemss on c.SalesEntryId equals d.SalesEntry
                     where b.SRItemsId == salesreturnsrid && b.Item == itemid
                     select new
                     {
                         d.SEItemsId
                     }
                 ).ToList();
            if (v.Count() <= 0)
                return 0;
            else if (v.Count() == 1)
                return v.Select(o => o.SEItemsId).FirstOrDefault();
            else
            {
                return v.Select(o => o.SEItemsId).FirstOrDefault();
            }

        }
        public string getallprifitnew(long salesentryid)
        {
            var v = (from a in db.SEItemss
                     join b in db.SalesEntrys on a.SalesEntry equals b.SalesEntryId
                     join c in db.Items on a.Item equals c.ItemID
                     where b.SalesEntryId == salesentryid
                     && c.KeepStock == true
                     select new
                     {
                         a.Item,
                         a.ItemUnit,
                         a.ItemQuantity,
                         a.ItemUnitPrice,
                         b.MaterialCenter,
                         SEDate=b.SECreatedDate,
                         a.SEItemsId,
                         c.ConFactor,
                     }).ToList();
            var itemsids = v.Select(o => o.Item).ToList().ToArray();
            var reqsqtys = v.Select(o => o.ItemQuantity).ToList().ToArray();

            string retvalues = "";
            int i = 0;

            decimal totalprofit = 0;
            var unitss = v.Select(o => o.ItemUnit).ToList().ToArray();

            var enteredpirces = v.Select(o => o.ItemUnitPrice).ToList().ToArray();
            var salesdetailid = v.Select(o => o.SEItemsId).ToList().ToArray();
            var SEDate = v.Select(o => o.SEDate).FirstOrDefault();
            var mc = v.Select(o => o.MaterialCenter).FirstOrDefault();
            var confactor = v.Select(o => o.ConFactor).FirstOrDefault();
            confactor = (confactor == null) ? 1 : confactor;
            foreach (var it in itemsids)
            {
                decimal qty = reqsqtys[i];
              var details = db.Items.Where(o => o.ItemID == it).Select(o => new { o.ItemCode, o.ConFactor ,o.ItemUnitID,o.SubUnitId}).FirstOrDefault();
                var profit = GetItemPurchasePricenewnew(it, SEDate, mc, salesdetailid[i]);



                decimal reqprice =(unitss[i]==details.SubUnitId)? ((profit / details.ConFactor) * qty):(profit*qty);
                decimal enteredprice = enteredpirces[i] * qty;
                decimal finalprofit = enteredprice - reqprice;
                  retvalues = retvalues + "Item Code : " + details.ItemCode + " Profit " + Math.Round(finalprofit, 2).ToString() + "\n";
                totalprofit = totalprofit + finalprofit;
                i++;
            }
            retvalues = retvalues + "Total Profit " + Math.Round(totalprofit, 2).ToString();
            return retvalues;
        }

        public string getallprifit(string itid, DateTime? SEDate, long? mc, long? salesentrydetailid, bool moment = false, string reqqtys = "",string enteredpirce="",string units="")
        {
           var  itemsids = itid.Split(',').Select(x => long.Parse(x)).ToArray();
            var reqsqtys = reqqtys.Split(',').Select(x => long.Parse(x)).ToArray();
            string retvalues = "";
            int i = 0;
          
            decimal totalprofit = 0;
            var unitss= units.Split(',').Select(x => long.Parse(x)).ToArray();
            var enteredpirces = enteredpirce.Split(',').Select(x => decimal.Parse(x)).ToArray();
            foreach (var it in itemsids)
            {
                decimal qty = reqsqtys[i];
                var details = db.Items.Where(o => o.ItemID == it).Select(o => new { o.ItemCode, o.ConFactor }).FirstOrDefault();
                 var profit = GetItemPurchasePricenewnew(it, SEDate, mc, null,false, qty);
                if (unitss[i] == 3 || unitss[i] == 13 || unitss[i] == 5)
                {
                    profit = profit / ((details.ConFactor == null) ? 1 : details.ConFactor);
                    }


                var reqprice = profit * qty;
                var enteredprice = enteredpirces[i] * qty;
                var finalprofit =enteredprice - reqprice;

                retvalues = retvalues+"Item Code : " + details.ItemCode + " Profit " + Math.Round(finalprofit,2).ToString() + "\n";
                totalprofit = totalprofit + finalprofit;
                i++;
            }
            retvalues = retvalues + "Total Profit " + Math.Round(totalprofit,2).ToString();
            return retvalues;
        }
        public decimal bonusclamable(long customerid, DateTime? fromdateinvoice)
        {
            decimal zero = 0;
            var cus = db.Customers.Find(customerid);
            DateTime curdate = System.DateTime.Now.Date;
  
            if (cus.bonuscheck == false)
            {
                return zero;
                    }
            else if (cus.bonusbaseamount == BonusBase.InvoiceTotalAmount)
            {
                fromdateinvoice = (DateTime)cus.startbonusdate;
                var salesentris = (from a in db.SalesEntrys

                                   where
                                     a.SEDate >= fromdateinvoice
                                     && a.SEDate<=curdate
                                     && a.Customer == customerid
                                   select new
                                   {

                                       a.MaterialCenter,
                                       a.SEDate,
                                       total=a.SESubTotal-a.SEDiscount


                                   }).ToList().Select(o => new
                                   {
                                       profit = (o.total)
                                   }

                               ).Sum(o => o.profit);

                decimal clamedbonus = (decimal?)db.customerbonus.Where(o => o.customerid == customerid).Sum(o => o.claimamount) ?? zero;
                Decimal totalprofit = (salesentris);
                var bonuspercentage = db.Customers.Where(o => o.bonuscheck == true && o.CustomerID == customerid).Select(o => o.bonuspercentage).FirstOrDefault();
                var bonusclimpercentage = db.Customers.Where(o => o.bonuscheck == true && o.CustomerID == customerid).Select(o => o.bonusclimembility).FirstOrDefault();

                if (bonuspercentage != null && bonusclimpercentage != null)
                {
                    return Math.Round((decimal)(((totalprofit * bonusclimpercentage / 100)) * bonuspercentage / 100)- clamedbonus, 2);
                }
                else
                {
                    return zero;
                }
            }
            else
            {
                fromdateinvoice =cus.startbonusdate;
                var salesentris = (from a in db.SalesEntrys

                                   join c in db.SEItemss on a.SalesEntryId equals c.SalesEntry
                                   join d in db.Items on c.Item equals d.ItemID
                                   where d.KeepStock == true
                                    && a.SEDate >= fromdateinvoice
                                      && a.SEDate <= curdate
                                    && a.Customer == customerid
                                   select new
                                   {
                                       c.Item,
                                       c.ItemQuantity,
                                       a.SalesEntryId,
                                       c.ItemUnitPrice,
                                       a.MaterialCenter,
                                       a.SEDate,
                                       c.SEItemsId,

                                   }).ToList().Select(o => new
                                   {
                                       profit = (o.ItemQuantity * o.ItemUnitPrice) - GetItemPurchasePrice(o.Item, o.SEDate, o.MaterialCenter, o.SEItemsId),
                                   }

                                 ).Sum(o => o.profit);
                decimal clamedbonus = (decimal?)db.customerbonus.Where(o => o.customerid == customerid).Sum(o => o.claimamount) ?? zero;
                Decimal totalprofit = (salesentris ); 

                var bonuspercentage = db.Customers.Where(o => o.bonuscheck == true && o.CustomerID == customerid).Select(o => o.bonuspercentage).FirstOrDefault();
                var bonusclimpercentage = db.Customers.Where(o => o.bonuscheck == true && o.CustomerID == customerid).Select(o => o.bonusclimembility).FirstOrDefault();

                if (bonuspercentage != null && bonusclimpercentage != null)
                {
                    return Math.Round((decimal)(((totalprofit * bonusclimpercentage / 100)) * bonuspercentage / 100)- clamedbonus, 2);
                }
                else
                {
                    return zero;
                }
            }
        }
        public decimal GetItemPurchasePrice(long? ItemId, DateTime? SEDate, long? mc, long? salesentrydetailid, bool moment = false, decimal? reqqty = 0)
        {
            decimal confactor = 1;
            decimal confactoract = 1;
            decimal sellqty = 0;
            var sellingunit = db.SEItemss.Where(o => o.SEItemsId == salesentrydetailid).Select(o => o.ItemUnit).FirstOrDefault();
            var items = db.Items.Where(o => o.ItemID == ItemId).FirstOrDefault();
            if (items.ConFactor != null)
            {
                confactor = items.ConFactor;
            }
            if (sellingunit == items.SubUnitId)
            {
                confactoract = confactor;
            }
            if (salesentrydetailid != null)
                sellqty = db.SEItemss.Where(o => o.SEItemsId == salesentrydetailid).Select(o => o.ItemQuantity).FirstOrDefault();
            else
                sellqty = 0;
            if (reqqty != 0)
                sellqty = (decimal)reqqty;

            DateTime fromdate = SEDate.Value.AddMonths(-3);
            List<DateTime> stockin = new List<DateTime>();


            moment = false;





            var selitem = new SqlParameter("@ItemId", (object)ItemId ?? DBNull.Value);
            var selmc = new SqlParameter("@MCId", (object)mc ?? DBNull.Value);
            var brand = new SqlParameter("@BrandId", "0");
            var stkble = new SqlParameter("@Stockble", "");
            var catgry = new SqlParameter("@CategoryId", "0");
            var fromdatee = new SqlParameter("@fromdate", (object)fromdate ?? DBNull.Value);
            var todate = new SqlParameter("@todate", SEDate.Value.AddDays(30));
            var stype = new SqlParameter("@Stype", "0");


            var data = db.Database.SqlQueryDedup<StockDataDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdatee, todate, stype).AsEnumerable().OrderBy(a => a.TDate).ToList();
            var data2 = data.Where(o => (o.TItemId != salesentrydetailid && (o.TItemType == "Purchase" || o.TItemType == "Stock Received" || o.TItemType == "Stock Receivedadj"))).ToList().Select
                (o => new batchstock
                {
                    TDate = o.TDate,
                    OQty = o.Qty,
                    BQty = o.Qty,
                    UnitPrice = o.UnitPrice,
                    TItemType = o.TItemType,
                    currstock = o.Qty,
                    confactor = confactor,
                    transactiondid = o.TItemId,
                    itemid = o.ItemId
                }
                ).OrderBy(o => o.TDate).ToList();


            var data3 = data.Where(o => (o.TItemId != salesentrydetailid && o.TDate < SEDate && (o.TItemType == "Sales" || o.TItemType == "Stock Transfered" || o.TItemType == "Stock Transferedadj" || o.TItemType == "Purchase Return")))
                .GroupBy(x => new { x.TDate, x.TItemType, x.UnitPrice, x.ItemId, x.Invoice }, (key, group) => new batchstock

                {

                    TDate = key.TDate,
                    OQty = group.Sum(o => o.Qty),
                    BQty = group.Sum(o => o.Qty),
                    UnitPrice = key.UnitPrice,
                    TItemType = key.TItemType,
                    currstock = group.Sum(o => o.Qty),
                    confactor = confactor,
                    transactiondid = group.Max(o => o.TItemId),
                    itemid = key.ItemId,
                    invoice = key.Invoice

                }).ToList()


                .Select
                (o => new batchstock
                {
                    TDate = o.TDate,
                    OQty = o.OQty - getsalesreturndetailid(o.invoice, o.itemid),
                    BQty = o.OQty,
                    UnitPrice = o.UnitPrice,
                    TItemType = o.TItemType,
                    currstock = o.OQty,
                    confactor = confactor,
                    transactiondid = o.itemid,
                    itemid = o.itemid
                }
                ).OrderBy(o => o.TDate).ToList();







            decimal? sumseqty = 0;

            int i = 0;
            foreach (var dt3 in data3)
            {
                if (data2.Count() > i)
                {

                    sumseqty = sumseqty + dt3.OQty;

                    if (sumseqty >= data2[i].OQty)
                    {
                        sumseqty = sumseqty - data2[i].OQty;/// data2[i].confactor);
                        data2[i].currstock = 0;
                        if ((i + 1) < data2.Count())
                        {
                            data2[i + 1].OQty = data2[i + 1].OQty - sumseqty;
                            sumseqty = 0;
                            data2[i + 1].BQty = data2[i + 1].OQty;
                            data2[i + 1].currstock = data2[i + 1].OQty;

                        }

                        i = i + 1;
                    }
                    else
                    {

                        data2[i].currstock = data2[i].currstock - dt3.OQty;
                    }
                }
            }
            var finaldata = data2.Where(o => o.currstock != 0).ToList();
            decimal? mcvalue = 0;
            var sellqtyorg = sellqty / confactoract;
            int flag = 0;
            int flagfirst = 0;
            foreach (var fidt in finaldata)
            {
                if (sellqty <= 0)
                    break;
                if ((sellqty / confactoract) >= (fidt.currstock / confactor))
                {

                    flagfirst = 1;
                    mcvalue = mcvalue + (decimal)(((decimal)fidt.currstock / confactor) * (decimal)(fidt.UnitPrice));
                    sellqty = (sellqty / confactoract) - (decimal)fidt.currstock / confactor;
                }
                else
                {
                    if (flagfirst == 0)
                    {
                        mcvalue = mcvalue + (decimal)(sellqty / confactoract * (decimal)(fidt.UnitPrice));
                        sellqty = (sellqty / confactoract) - (decimal)fidt.currstock / confactor;
                    }
                    else
                    {

                        mcvalue = mcvalue + (decimal)(sellqty / confactoract * (decimal)(fidt.UnitPrice));
                        sellqty = (sellqty / confactoract) - (decimal)fidt.currstock / confactor;

                    }
                }



            }


            if (mcvalue > 0 && sellqty <= 0)
            {

                //#if DEBUG
                //#endif
                return Convert.ToDecimal(mcvalue / sellqtyorg);
            }
            else
            {
                var NewPurPrice = (from aa in db.PEItemss
                                   join bb in db.PurchaseEntrys on aa.PurchaseEntry equals bb.PurchaseEntryId
                                   where (aa.Item == ItemId &&
                                   bb.PEDate >= fromdate &&

                                   bb.PEDate <= SEDate) &&
                                   bb.MaterialCenter == mc
                                   orderby bb.PEDate descending
                                   select new
                                   {
                                       unitprice = aa.ItemUnitPrice,
                                       date = bb.PEDate


                                   }).FirstOrDefault();
                var newstocktransfer = (from aa in db.StockTransferItems
                                        join bb in db.StockTransfers on aa.StockTransferId equals bb.Id
                                        where (aa.Item == ItemId &&
                                           bb.Date >= fromdate &&
                                        bb.Date <= SEDate) &&
                                        bb.MCTo == mc
                                        orderby bb.Date descending
                                        select new
                                        {
                                            unitprice = aa.Price,
                                            date = bb.Date,


                                        }).FirstOrDefault();


                if (newstocktransfer == null && NewPurPrice == null)
                {
                    decimal a = 0;
                    return items.PurchasePrice / confactoract;
                }
                if (newstocktransfer != null && NewPurPrice != null)
                {
                    if (newstocktransfer.date > NewPurPrice.date)
                    {
                        return newstocktransfer.unitprice / confactoract;
                    }
                    else
                    {
                        return NewPurPrice.unitprice / confactoract;
                    }
                }
                else if (newstocktransfer != null)
                {
                    return newstocktransfer.unitprice / confactoract;
                }
                else if (NewPurPrice != null)
                {
                    return NewPurPrice.unitprice / confactoract;
                }
                else
                {
                    decimal a = 0;
                    return items.PurchasePrice / confactoract;
                }

            }


        }

        //Function To Get Item Purchase Price (If Any Exists With in SEDate)
        public decimal GetItemPurchasePricenewnew(long? ItemId, DateTime? SEDate, long? mc, long? salesentrydetailid, bool moment = false,decimal? reqqty=0,List<salesreturnmodel> salereturnids=null, bool nostocktransfer=false)
        {
            decimal confactor = 1;
            decimal confactoract = 1;
            decimal sellqty = 0;
            var sellingunit = db.SEItemss.Where(o => o.SEItemsId == salesentrydetailid).Select(o => o.ItemUnit).FirstOrDefault();
            var items = db.Items.Where(o => o.ItemID == ItemId).FirstOrDefault();
            if (items.ConFactor != null)
            {
                confactor = items.ConFactor;
            }
            if (sellingunit == items.SubUnitId)
            {
                confactoract = confactor;
            }
            if (salesentrydetailid != null)
                sellqty = db.SEItemss.Where(o => o.SEItemsId == salesentrydetailid).Select(o => o.ItemQuantity).FirstOrDefault();
            else
                sellqty = 0;
            if (reqqty != 0 )
                sellqty = (decimal)reqqty;

            DateTime fromdate = SEDate.Value.AddMonths(-3);
            List<DateTime> stockin = new List<DateTime>();


            moment = false;





            var selitem = new SqlParameter("@ItemId", (object)ItemId ?? DBNull.Value);
            if (nostocktransfer == true)
                mc = 0;
            if (mc == null)
                mc = 0;
            var selmc = new SqlParameter("@MCId", (object)mc ?? DBNull.Value);
            var brand = new SqlParameter("@BrandId", "0");
            var stkble = new SqlParameter("@Stockble", "");
            var catgry = new SqlParameter("@CategoryId", "0");
            var fromdatee = new SqlParameter("@fromdate", (object)fromdate ?? DBNull.Value);
            var todate = new SqlParameter("@todate", SEDate.Value.AddDays(30));
            var stype = new SqlParameter("@Stype", "0");


            var data = db.Database.SqlQueryDedup<StockDataDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdatee, todate, stype).AsEnumerable().OrderBy(a => a.TDate).ToList();
            var data2 = data.Where(o => (o.TItemId != salesentrydetailid && (o.TItemType == "Purchase" ||(nostocktransfer==true|| o.TItemType == "Stock Received" )||(nostocktransfer==true|| o.TItemType == "Stock Receivedadj") )) ).ToList().Select
                (o => new batchstock
                {
                    TDate = o.TDate,
                    OQty = o.Qty,
                    BQty = o.Qty,
                    UnitPrice = o.UnitPrice,
                    TItemType = o.TItemType,
                    currstock = o.Qty,
                    confactor = confactor,
                    transactiondid = o.TItemId,
                    itemid = o.ItemId
                }
                ).OrderBy(o => o.TDate).ToList();
            var data4 = data.Where(o => (o.TItemId != salesentrydetailid && (o.TItemType == "Sales Return"))).ToList().Select
                (o => new batchstock
                {
                    TDate = o.TDate,
                    OQty = o.Qty,
                    BQty = o.Qty,
                    UnitPrice = o.UnitPrice,
                    TItemType = o.TItemType,
                    currstock = o.Qty,
                    confactor = confactor,
                    //transactiondid = (from a in db.SEItemss
                    //                  where d.Item==a.Item && d.SRItemsId==o.TItemId
                    //                      a.SEItemsId
                    transactiondid=o.TItemId,
                    itemid = o.ItemId
                }
                ).OrderBy(o => o.TDate).ToList();
            

            //                             b.SalesEntryId

            //                        where salesentrieswitid.Contains(c.SalesEntryId)
            //                        //join c in db.Items on b.Item equals c.ItemID
            //                        group new { b.Item, b.ItemQuantity, c.SalesEntryId, c.BillNo } by new { c.SalesEntryId, b.Item } into grp
            //                            itid = grp.FirstOrDefault().Item,
            //                            saleid = grp.FirstOrDefault().SalesEntryId,
            //                            billno = grp.FirstOrDefault().BillNo,

            var data3 = data.Where(o => (o.TItemId != salesentrydetailid && o.TDate < SEDate && (o.TItemType == "Sales" ||(nostocktransfer==true|| o.TItemType == "Stock Transfered" )|| (nostocktransfer==true||o.TItemType == "Stock Transferedadj") || o.TItemType == "Purchase Return")))
                .GroupBy(x => new { x.TDate, x.TItemType, x.UnitPrice, x.ItemId, x.Invoice }, (key, group) => new batchstock

                {

                    TDate = key.TDate,
                    OQty = group.Sum(o => o.Qty),
                    BQty = group.Sum(o => o.Qty),
                    UnitPrice = key.UnitPrice,
                    TItemType = key.TItemType,
                    currstock = group.Sum(o => o.Qty),
                    confactor = confactor,
                    transactiondid = group.Max(o => o.TItemId),
                    itemid = key.ItemId,
                    invoice = key.Invoice

                }).ToList()


                .Select
                (o => new batchstock
                {
                    TDate = o.TDate,
                    OQty = o.OQty -((decimal?)data4.Where(k=>k.transactiondid==o.transactiondid).Select(k=>k.OQty).FirstOrDefault()??0),//- getsalesreturndetailid(o.invoice, o.itemid),//- salereturnidsnew.Where(x => x.itid == o.itemid && x.billno == o.invoice).Select(x => x.qty).FirstOrDefault(),//getsalesreturndetailid(o.invoice, o.itemid),
                    BQty = o.OQty,
                    UnitPrice = o.UnitPrice,
                    TItemType = o.TItemType,
                    currstock = o.OQty,
                    confactor = confactor,
                    transactiondid = o.transactiondid,
                    itemid = o.itemid
                }
                ).OrderBy(o => o.TDate).ToList();







            decimal? sumseqty = 0;

            int i = 0;
            foreach (var dt3 in data3)
            {
                if (data2.Count() > i)
                {

                    sumseqty = sumseqty + dt3.OQty;

                    if (sumseqty >= data2[i].OQty      )
                    {
                        sumseqty = sumseqty - data2[i].OQty;/// data2[i].confactor);
                        data2[i].currstock = 0;
                        if ((i + 1) < data2.Count())
                        {
                            data2[i + 1].OQty = data2[i + 1].OQty - sumseqty;
                            sumseqty = 0;
                            data2[i + 1].BQty = data2[i + 1].OQty;
                            data2[i + 1].currstock = data2[i + 1].OQty;

                        }

                        i = i + 1;
                    }
                    else
                    {

                        data2[i].currstock = data2[i].currstock - dt3.OQty;
                    }
                }
            }
            var fine = data2.Where(o => o.currstock!=  0).ToList();
            i = 0;
       
            //finaldata = fine;`
            decimal? mcvalue = 0;
            var sellqtyorg = sellqty / confactoract;
            int flag = 0;
            int flagfirst = 0;
            decimal vv = 0;
            decimal firstobjvalue = 0;
            int j = 0;
            foreach( var minus in fine)
            {
             
                if(minus.currstock<0)
                {
                     vv =vv+ (decimal)minus.currstock;
                    minus.currstock = 0;
                    fine[j].currstock = 0;

                }
                else
                {



                    if (minus.currstock < Convert.ToDecimal(Math.Abs(vv)))
                    {
                        vv = vv + (decimal)minus.currstock;
                        fine[j].currstock = 0;
                    }
                    else
                    {
                        minus.currstock = minus.currstock + vv;
                        fine[j].currstock = minus.currstock;
                        break;
                    }


                }
                j++;
            }
            fine = fine.Where(o => o.currstock > 0).ToList();
          
            foreach (var fidt in fine)
            {
                if (sellqty <= 0)
                    break;
                if ((sellqty / confactoract) >= (fidt.currstock / confactor))
                {

                    flagfirst = 1;
                  
                    mcvalue = mcvalue + (decimal)(((decimal)fidt.currstock / confactor) * (decimal)(fidt.UnitPrice));
                    sellqty = (sellqty / confactoract) - (decimal)fidt.currstock / confactor;
                }
                else
                {
                    if (flagfirst == 0)
                    {
                        mcvalue = mcvalue + (decimal)(sellqty / confactoract * (decimal)(fidt.UnitPrice));
                        sellqty = (sellqty / confactoract) - (decimal)fidt.currstock / confactor;
                    }
                    else
                    {

                        mcvalue = mcvalue + (decimal)(sellqty / confactoract * (decimal)(fidt.UnitPrice));
                        sellqty = (sellqty / confactoract) - (decimal)fidt.currstock / confactor;

                    }
                }



            }


            if (mcvalue >= 0 && sellqty<=0&& sellqtyorg>0)
            {

                //#if DEBUG
                //#endif
                return Convert.ToDecimal(mcvalue / sellqtyorg);
            }
            else
            {
                var NewPurPrice = (from aa in db.PEItemss
                                   join bb in db.PurchaseEntrys on aa.PurchaseEntry equals bb.PurchaseEntryId
                                   where (aa.Item == ItemId &&
                                   bb.PEDate >= fromdate &&

                                   bb.PEDate <= SEDate) &&
                                   bb.MaterialCenter == mc
                                   orderby bb.PEDate descending
                                   select new
                                   {
                                       unitprice = aa.ItemUnitPrice,
                                       date = bb.PEDate


                                   }).FirstOrDefault();
                var newstocktransfer = (from aa in db.StockTransferItems
                                        join bb in db.StockTransfers on aa.StockTransferId equals bb.Id
                                        where (aa.Item == ItemId &&
                                           bb.Date >= fromdate &&
                                        bb.Date <= SEDate) &&
                                        bb.MCTo == mc
                                        orderby bb.Date descending
                                        select new
                                        {
                                            unitprice = aa.Price,
                                            date = bb.Date,


                                        }).FirstOrDefault();


                if (newstocktransfer == null && NewPurPrice == null)
                {
                    decimal a = 0;
                    return items.PurchasePrice / confactoract;
                }
                if (newstocktransfer != null && NewPurPrice != null)
                {
                    if (newstocktransfer.date > NewPurPrice.date)
                    {
                        return newstocktransfer.unitprice / confactoract;
                    }
                    else
                    {
                        return NewPurPrice.unitprice / confactoract;
                    }
                }
                else if (newstocktransfer != null)
                {
                    return newstocktransfer.unitprice / confactoract;
                }
                else if (NewPurPrice != null)
                {
                    return NewPurPrice.unitprice / confactoract;
                }
                else
                {
                    decimal a = 0;
                    return items.PurchasePrice / confactoract;
                }

            }


        }
        public decimal GetItemPurchasePricenew(long? ItemId, DateTime? SEDate, long? mc, long? salesentrydetailid,List<StockDataDetails> data, bool moment = false, decimal? reqqty = 0)
        {
            decimal confactor = 1;
            decimal confactoract = 1;
            decimal sellqty = 0;
            var sellingunit = db.SEItemss.Where(o => o.SEItemsId == salesentrydetailid).Select(o => o.ItemUnit).FirstOrDefault();
            var items = db.Items.Where(o => o.ItemID == ItemId).FirstOrDefault();
            if (items.ConFactor != null)
            {
                confactor = items.ConFactor;
            }
            if (sellingunit == items.SubUnitId)
            {
                confactoract = confactor;
            }
            if (salesentrydetailid != null)
            {
                sellqty = db.SEItemss.Where(o => o.SEItemsId == salesentrydetailid).Select(o => o.ItemQuantity).FirstOrDefault();

            }
            else
                sellqty = 0;
            if (reqqty != 0)
                sellqty = (decimal)reqqty;

            DateTime fromdate = SEDate.Value.AddMonths(-3);
            List<DateTime> stockin = new List<DateTime>();


            moment = false;





            var selitem = new SqlParameter("@ItemId", (object)ItemId ?? DBNull.Value);
            var selmc = new SqlParameter("@MCId", (object)mc ?? DBNull.Value);
            var brand = new SqlParameter("@BrandId", "0");
            var stkble = new SqlParameter("@Stockble", "");
            var catgry = new SqlParameter("@CategoryId", "0");
            var fromdatee = new SqlParameter("@fromdate", (object)fromdate ?? DBNull.Value);
            var todate = new SqlParameter("@todate", SEDate.Value.AddDays(30));
            var stype = new SqlParameter("@Stype", "0");


            var data2 = data.Where(o => (o.ItemId==ItemId && o.TItemId != salesentrydetailid && (o.TItemType == "Purchase" || o.TItemType == "Stock Received" || o.TItemType == "Stock Receivedadj" || o.TItemType == "Sales Return"))).ToList().Select
                (o => new batchstock
                {
                    TDate = o.TDate,
                    OQty = o.Qty,
                    BQty = o.Qty,
                    UnitPrice = o.UnitPrice,
                    TItemType = o.TItemType,
                    currstock = o.Qty,
                    confactor = confactor,
                    transactiondid = o.TItemId,
                    itemid = o.ItemId
                }
                ).OrderBy(o => o.TDate).ToList();


            var data3 = data.Where(o => (o.ItemId == ItemId && o.TItemId != salesentrydetailid && o.TDate <= SEDate.Value.AddDays(-1) && (o.TItemType == "Sales" || o.TItemType == "Stock Transfered" || o.TItemType == "Stock Transferedadj" || o.TItemType == "Purchase Return")))
                .GroupBy(x => new { x.TDate, x.TItemType, x.UnitPrice, x.ItemId, x.Invoice }, (key, group) => new batchstock

                {

                    TDate = key.TDate,
                    OQty = group.Sum(o => o.Qty),
                    BQty = group.Sum(o => o.Qty),
                    UnitPrice = key.UnitPrice,
                    TItemType = key.TItemType,
                    currstock = group.Sum(o => o.Qty),
                    confactor = confactor,
                    transactiondid = group.Max(o => o.TItemId),
                    itemid = key.ItemId,
                    invoice = key.Invoice

                }).ToList()


                .Select
                (o => new batchstock
                {
                    TDate = o.TDate,
                    OQty = o.OQty - getsalesreturndetailid(o.invoice, o.itemid),
                    BQty = o.OQty,
                    UnitPrice = o.UnitPrice,
                    TItemType = o.TItemType,
                    currstock = o.OQty,
                    confactor = confactor,
                    transactiondid = o.itemid,
                    itemid = o.itemid
                }
                ).OrderBy(o => o.TDate).ToList();





            decimal? sumseqty = 0;

            int i = 0;
            foreach (var dt3 in data3)
            {
                if (data2.Count() > i)
                {

                    sumseqty = sumseqty + dt3.OQty;

                    if (sumseqty >= data2[i].OQty)
                    {
                        sumseqty = sumseqty - data2[i].OQty;/// data2[i].confactor);
                        data2[i].currstock = 0;
                        if ((i + 1) < data2.Count())
                        {
                            data2[i + 1].OQty = data2[i + 1].OQty - sumseqty;
                            sumseqty = 0;
                            data2[i + 1].BQty = data2[i + 1].OQty;
                            data2[i + 1].currstock = data2[i + 1].OQty;

                        }

                        i = i + 1;
                    }
                    else
                    {

                        data2[i].currstock = data2[i].currstock - dt3.OQty;
                    }
                }
            }
            var finaldata = data2.Where(o => o.currstock != 0).ToList();
            decimal? mcvalue = 0;
            var sellqtyorg = sellqty / confactoract;
            int flag = 0;
            int flagfirst = 0;
            foreach (var fidt in finaldata)
            {
                if (sellqty <= 0)
                    break;
                if ((sellqty / confactoract) >= (fidt.currstock / confactor))
                {

                    flagfirst = 1;
                    mcvalue = mcvalue + (decimal)(((decimal)fidt.currstock / confactor) * (decimal)(fidt.UnitPrice));
                    sellqty = (sellqty / confactoract) - (decimal)fidt.currstock / confactor;
                }
                else
                {
                    if (flagfirst == 0)
                    {
                        mcvalue = mcvalue + (decimal)(sellqty / confactoract * (decimal)(fidt.UnitPrice));
                        sellqty = (sellqty / confactoract) - (decimal)fidt.currstock / confactor;
                    }
                    else
                    {

                        mcvalue = mcvalue + (decimal)(sellqty / confactoract * (decimal)(fidt.UnitPrice));
                        sellqty = (sellqty / confactoract) - (decimal)fidt.currstock / confactor;

                    }
                }



            }
            if (mcvalue > 0 && sellqty <= 0)
            {

                //#if DEBUG
                //#endif
                return Convert.ToDecimal(mcvalue / sellqtyorg);
            }
            else
            {
                var NewPurPrice = (from aa in db.PEItemss
                                   join bb in db.PurchaseEntrys on aa.PurchaseEntry equals bb.PurchaseEntryId
                                   where (aa.Item == ItemId &&
                                   bb.PEDate >= fromdate &&

                                   bb.PEDate <= SEDate) &&
                                   bb.MaterialCenter == mc
                                   orderby bb.PEDate descending
                                   select new
                                   {
                                       unitprice = aa.ItemUnitPrice,
                                       date = bb.PEDate


                                   }).FirstOrDefault();
                var newstocktransfer = (from aa in db.StockTransferItems
                                        join bb in db.StockTransfers on aa.StockTransferId equals bb.Id
                                        where (aa.Item == ItemId &&
                                           bb.Date >= fromdate &&
                                        bb.Date <= SEDate) &&
                                        bb.MCTo == mc
                                        orderby bb.Date descending
                                        select new
                                        {
                                            unitprice = aa.Price,
                                            date = bb.Date,


                                        }).FirstOrDefault();


                if (newstocktransfer == null && NewPurPrice == null)
                {
                    decimal a = 0;
                    return items.PurchasePrice / confactoract;
                }
                if (newstocktransfer != null && NewPurPrice != null)
                {
                    if (newstocktransfer.date > NewPurPrice.date)
                    {
                        return newstocktransfer.unitprice / confactoract;
                    }
                    else
                    {
                        return NewPurPrice.unitprice / confactoract;
                    }
                }
                else if (newstocktransfer != null)
                {
                    return newstocktransfer.unitprice / confactoract;
                }
                else if (NewPurPrice != null)
                {
                    return NewPurPrice.unitprice / confactoract;
                }
                else
                {
                    decimal a = 0;
                    return items.PurchasePrice / confactoract;
                }

            }


        }

        public DateTime? getsaledetailid(long itemid, long transid)
        {
            var salesentryid = (from a in db.SalesEntrys
                                join b in db.SalesReturns on a.SalesEntryId equals b.SalesEntryId
                                join c in db.SRItemss on b.SalesReturnId equals c.SalesReturnId
                                where c.SalesReturnId == transid && c.Item == itemid
                                select new
                                {
                                    a.SEDate
                                }).FirstOrDefault();

            return Convert.ToDateTime(salesentryid);
        }
        [QkAuthorize(Roles = "Dev,All Sales")]
        public ActionResult ViewIndexprofit(string seno, long? paymethod, long? customer, long? SalesExecutive, string from, string to, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate)
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

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
            companySet();
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View();
        }

        [QkAuthorize(Roles = "Dev,All Sales")]
        public ActionResult ViewIndex(string seno, long? paymethod, long? customer, long? SalesExecutive, string from, string to, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate)
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

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
            companySet();
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View();
        }

        public ActionResult viewemployeesales(string seno, long? paymethod, long? customer, long? SalesExecutive, string from, string to, long? type, long? ddMC, string satype, long? htype, string hfdate, string htdate, Boolean perwise)
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

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
            companySet();
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View();
        }


        //Customer region

        [QkAuthorize(Roles = "Dev,Sales Customer Wise")]
        public ActionResult CustomerWise(long? cust, string from, string to, string saletype, long? ddMC)
        {

            SaleType St = new SaleType();
            if (saletype != "" && saletype != null)
            {
                St = (saletype == "1") ? SaleType.Sale : SaleType.Hire;
            };
            if (cust != null)
            {
                ViewBag.custName = (from a in db.Customers
                                    join b in db.SalesEntrys on a.CustomerID equals b.Customer into cat
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
            ViewBag.Customer = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 0);
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");

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
        //[HttpPost]
        //[QkAuthorize(Roles = "Dev,Sales Customer Wise")]
        //Customer region
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Sales Customer Wise")]
        public ActionResult GetCustomerWise(long? customer, string fromdate, string todate, string Salety, long? ddMC, long? project, long? task)
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
            SaleType St = new SaleType();
            if (Salety == "1" || Salety == "2")
            {
                St = (Salety == "1") ? SaleType.Sale : SaleType.Hire;
            }
            var v = (from a in db.Customers
                     join f in db.Accountss on a.Accounts equals f.AccountsID into acc
                     from f in acc.DefaultIfEmpty()
                     join b in db.SalesEntrys on a.CustomerID equals b.Customer into cust
                     from b in cust.DefaultIfEmpty()

                     where (customer == 0 || a.CustomerID == customer)
                      && (fromdate == "" || EF.Functions.DateDiffDay(b.SEDate, fdate) <= 0) &&
                      (todate == "" || EF.Functions.DateDiffDay(b.SEDate, tdate) >= 0)
                     && ((!MCList.Any() && ddMC == null) || MCArray.Contains(b.MaterialCenter) || ddMC == b.MaterialCenter)
                      && (project == 0 || project == null || b.Project == project)
                     && (task == 0 || task == null || b.ProTask == task)
                     select new
                     {
                         a.CustomerID,
                         customer = a.CustomerCode + "-" + a.CustomerName,
                         TRN = f.TRN,
                         SaleAmt = (decimal?)(from i in db.SalesEntrys
                                              where (fromdate == "" || EF.Functions.DateDiffDay(i.SEDate, fdate) <= 0) && i.Status == 1 &&
                                              (todate == "" || EF.Functions.DateDiffDay(i.SEDate, tdate) >= 0) &&
                                              (i.Customer == a.CustomerID) && (Salety == "" || Salety == null || i.SaleType == St)
                                              && ((!MCList.Any() && ddMC == null) || MCArray.Contains(i.MaterialCenter) || ddMC == i.MaterialCenter)
                                              && (project == 0 || project == null || i.Project == project)
                                              && (task == 0 || task == null || i.ProTask == task)
                                              group i by i.Customer into g
                                              select new
                                              {
                                                  Total = g.Sum(x => x.SESubTotal - x.SEDiscount)
                                              }).FirstOrDefault().Total ?? 0,
                         SaletaxAmt = (decimal?)(from i in db.SalesEntrys
                                                 where (fromdate == "" || EF.Functions.DateDiffDay(i.SEDate, fdate) <= 0) && i.Status == 1 &&
                                                 (todate == "" || EF.Functions.DateDiffDay(i.SEDate, tdate) >= 0) &&
                                                 (i.Customer == a.CustomerID) && (Salety == "" || Salety == null || i.SaleType == St)
                                                  && ((!MCList.Any() && ddMC == null) || MCArray.Contains(i.MaterialCenter) || ddMC == i.MaterialCenter)
                                                  && (project == 0 || project == null || i.Project == project)
                                                  && (task == 0 || task == null || i.ProTask == task)
                                                 group i by i.Customer into g
                                                 select new
                                                 {
                                                     Total = g.Sum(x => x.SETaxAmount)
                                                 }).FirstOrDefault().Total ?? 0,
                         SaletotAmt = (decimal?)(from i in db.SalesEntrys
                                                 where (fromdate == "" || EF.Functions.DateDiffDay(i.SEDate, fdate) <= 0) && i.Status == 1 &&
                                                 (todate == "" || EF.Functions.DateDiffDay(i.SEDate, tdate) >= 0) &&
                                                 (i.Customer == a.CustomerID) && (Salety == "" || Salety == null || i.SaleType == St)
                                                  && ((!MCList.Any() && ddMC == null) || MCArray.Contains(i.MaterialCenter) || ddMC == i.MaterialCenter)
                                                  && (project == 0 || project == null || i.Project == project)
                                                  && (task == 0 || task == null || i.ProTask == task)
                                                 group i by i.Customer into g
                                                 select new
                                                 {
                                                     Total = g.Sum(x => x.SEGrandTotal)
                                                 }).FirstOrDefault().Total ?? 0,
                         RetunAmt = (decimal?)(from i in db.SalesReturns
                                               where (fromdate == "" || EF.Functions.DateDiffDay(i.SRDate, fdate) <= 0) &&
                                               (todate == "" || EF.Functions.DateDiffDay(i.SRDate, tdate) >= 0) &&
                                               (i.Customer == a.CustomerID) && (Salety == "" || Salety == null || i.SaleType == St)
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
                                                  (i.Customer == a.CustomerID) && (Salety == "" || Salety == null || i.SaleType == St)
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
                                                  (i.Customer == a.CustomerID) && (Salety == "" || Salety == null || i.SaleType == St)
                                                  && ((!MCList.Any() && ddMC == null) || MCArray.Contains(i.MaterialCenter) || ddMC == i.MaterialCenter)
                                                  && (project == 0 || project == null || i.Project == project)
                                                  && (task == 0 || task == null || i.ProTask == task)
                                                  group i by i.Customer into g
                                                  select new
                                                  {
                                                      Total = g.Sum(x => x.SRGrandTotal)
                                                  }).FirstOrDefault().Total ?? 0,

                         NoOfVchSale = (int?)(from i in db.SalesEntrys
                                              where (fromdate == "" || EF.Functions.DateDiffDay(i.SEDate, fdate) <= 0) && i.Status == 1 &&
                                              (todate == "" || EF.Functions.DateDiffDay(i.SEDate, tdate) >= 0) &&
                                              (i.Customer == a.CustomerID) && (Salety == "" || Salety == null || i.SaleType == St)
                                              && ((!MCList.Any() && ddMC == null) || MCArray.Contains(i.MaterialCenter) || ddMC == i.MaterialCenter)
                                              && (project == 0 || project == null || i.Project == project)
                                              && (task == 0 || task == null || i.ProTask == task)
                                              select new
                                              {
                                                  saleid = i.SalesEntryId
                                              }).Count() ?? 0,
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
                     }).Distinct().OrderBy(b => b.customer);

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
        [QkAuthorize(Roles = "Dev,Sales Customer Wise")]
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
                                    join b in db.SalesEntrys on a.CustomerID equals b.Customer into cat
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



        //#region month wise

        [QkAuthorize(Roles = "Dev,Sales Month Wise")]
        public ActionResult MonthWiseSelect()
        {
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Sales Month Wise")]
        public ActionResult MonthWise(int Year)
        {
            companySet();
            var vmodel = new MonthWiseSaleReportViewModel();



            //sales
            //monthly sale count
            var Qry1 = "SELECT *, CAST(NULL AS INT) AS [total] FROM(SELECT YEAR(SEDate)[Year] ,DATENAME(MONTH, SEDate)[Month], " +
                       " COUNT(1)[Sales Count] FROM SalesEntries Where Status=1 AND YEAR(SEDate)=" + Year + "" +
                       " GROUP BY YEAR(SEDate), DATENAME(MONTH, SEDate)) AS MontlySalesData " +
                       " PIVOT(SUM([Sales Count]) " +
                       " FOR Month IN([January], [February], [March], [April], [May], " +
                       " [June], [July], [August], [September], [October], [November], " +
                       " [December])) AS MNamePivot ";

            vmodel.saleCount = db.Database.SqlQueryRaw<MonthWise>(Qry1).AsEnumerable().ToList();

            //monthly taxable amt 
            var Qry2 = "SELECT *, CAST(NULL AS DECIMAL(18,2)) AS [total] FROM(SELECT YEAR(SEDate)[Year],DATENAME(MONTH, SEDate)[Month], " +
                   " sum(SESubTotal - SEDiscount) [saleamount] FROM SalesEntries Where Status=1 AND YEAR(SEDate)=" + Year + "" +
                   " GROUP BY YEAR(SEDate), DATENAME(MONTH, SEDate)) AS MontlySalesData " +
                   " PIVOT(SUM([saleamount]) " +
                   " FOR Month IN([January], [February], [March], [April], [May], " +
                   " [June], [July], [August], [September], [October], [November], " +
                   " [December])) AS MNamePivot ";

            vmodel.taxableAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(Qry2).AsEnumerable().ToList();


            //monthly total sales amount
            var Qry3 = "SELECT *, CAST(NULL AS DECIMAL(18,2)) AS [total] FROM(SELECT YEAR(SEDate)[Year], DATENAME(MONTH, SEDate)[Month]," +
                       " sum(SEGrandTotal)[saleamount] FROM SalesEntries Where Status=1 AND YEAR(SEDate)=" + Year + " GROUP BY YEAR(SEDate), " +
                       " DATENAME(MONTH, SEDate)) AS MontlySalesData " +
                       " PIVOT(SUM([saleamount]) " +
                       " FOR Month IN([January], [February], [March], [April], [May]," +
                       " [June], [July], [August], [September], [October], [November],[December])) AS MNamePivot";

            vmodel.saleAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(Qry3).AsEnumerable().ToList();

            //total tax amount
            var Qry4 = "SELECT *, CAST(NULL AS DECIMAL(18,2)) AS [total] FROM(SELECT YEAR(SEDate)[Year],DATENAME(MONTH, SEDate)[Month], " +
                       " sum(SETaxAmount)[saleamount] FROM SalesEntries Where Status=1 AND YEAR(SEDate)=" + Year + "" +
                       " GROUP BY YEAR(SEDate), DATENAME(MONTH, SEDate)) AS MontlySalesData " +
                       " PIVOT(SUM([saleamount]) " +
                       " FOR Month IN([January], [February], [March], [April], [May], " +
                       " [June], [July], [August], [September], [October], [November], [December])) AS MNamePivot";

            vmodel.taxAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(Qry4).AsEnumerable().ToList();

            //sales return

            //monthly sale count
            var QryR1 = "SELECT *, CAST(NULL AS INT) AS [total] FROM(SELECT YEAR(SRDate)[Year],DATENAME(MONTH, SRDate)[Month], " +
                       " COUNT(1)[Sales Count] FROM SalesReturns Where YEAR(SRDate)=" + Year + "" +
                       " GROUP BY YEAR(SRDate), DATENAME(MONTH, SRDate)) AS MontlySalesData " +
                       " PIVOT(SUM([Sales Count]) " +
                       " FOR Month IN([January], [February], [March], [April], [May], " +
                       " [June], [July], [August], [September], [October], [November], " +
                       " [December])) AS MNamePivot ";

            vmodel.saleRetCount = db.Database.SqlQueryRaw<MonthWise>(QryR1).AsEnumerable().ToList();


            //monthly taxable amt 
            var QryR2 = "SELECT *, CAST(NULL AS DECIMAL(18,2)) AS [total] FROM(SELECT YEAR(SRDate)[Year],DATENAME(MONTH, SRDate)[Month], " +
                   " sum(SRSubTotal - SRDiscount) [saleamount] FROM SalesReturns Where YEAR(SRDate)=" + Year + "" +
                   " GROUP BY YEAR(SRDate), DATENAME(MONTH, SRDate)) AS MontlySalesData " +
                   " PIVOT(SUM([saleamount]) " +
                   " FOR Month IN([January], [February], [March], [April], [May], " +
                   " [June], [July], [August], [September], [October], [November], " +
                   " [December])) AS MNamePivot ";

            vmodel.taxableRetAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(QryR2).AsEnumerable().ToList();


            //monthly total sales amount
            var QryR3 = "SELECT *, CAST(NULL AS DECIMAL(18,2)) AS [total] FROM(SELECT YEAR(SRDate)[Year], DATENAME(MONTH, SRDate)[Month]," +
                       " sum(SRGrandTotal)[saleamount] FROM SalesReturns Where YEAR(SRDate)=" + Year + " GROUP BY YEAR(SRDate), " +
                       " DATENAME(MONTH, SRDate)) AS MontlySalesData " +
                       " PIVOT(SUM([saleamount]) " +
                       " FOR Month IN([January], [February], [March], [April], [May]," +
                       " [June], [July], [August], [September], [October], [November],[December])) AS MNamePivot";

            vmodel.saleRetAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(QryR3).AsEnumerable().ToList();

            //total tax amount
            var QryR4 = "SELECT *, CAST(NULL AS DECIMAL(18,2)) AS [total] FROM(SELECT YEAR(SRDate)[Year],DATENAME(MONTH, SRDate)[Month], " +
                       " sum(SRTaxAmount)[saleamount] FROM SalesReturns Where YEAR(SRDate)=" + Year + "" +
                       " GROUP BY YEAR(SRDate), DATENAME(MONTH, SRDate)) AS MontlySalesData " +
                       " PIVOT(SUM([saleamount]) " +
                       " FOR Month IN([January], [February], [March], [April], [May], " +
                       " [June], [July], [August], [September], [October], [November], [December])) AS MNamePivot";
            vmodel.taxRetAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(QryR4).AsEnumerable().ToList();
            // calculate net sale amount
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

        [QkAuthorize(Roles = "Dev,Sales Month Wise")]
        public ActionResult MonthWise()
        {

            var chksale = db.SalesEntrys.Where(s => s.Status == 1).Select(s => s.SEDate.Year).Distinct().Count();
            var chkret = db.SalesReturns.Select(s => s.SRDate.Year).Distinct().Count();

            if (chksale > 1 || chkret > 1)
            {
                return Redirect("/SalesReport/MonthWiseSelect");
            }
            else
            {
                companySet();
                var vmodel = new MonthWiseSaleReportViewModel();
                //sales
                //monthly sale count
                var Qry1 = "SELECT *, CAST(NULL AS INT) AS [total] FROM(SELECT YEAR(SEDate)[Year],DATENAME(MONTH, SEDate)[Month], " +
                           " COUNT(1)[Sales Count] FROM SalesEntries Where Status=1  " +
                           " GROUP BY YEAR(SEDate), DATENAME(MONTH, SEDate)) AS MontlySalesData " +
                           " PIVOT(SUM([Sales Count]) " +
                           " FOR Month IN([January], [February], [March], [April], [May], " +
                           " [June], [July], [August], [September], [October], [November], " +
                           " [December])) AS MNamePivot ";

                vmodel.saleCount = db.Database.SqlQueryRaw<MonthWise>(Qry1).AsEnumerable().ToList();

                //monthly taxable amt 
                var Qry2 = "SELECT *, CAST(NULL AS DECIMAL(18,2)) AS [total] FROM(SELECT YEAR(SEDate)[Year],DATENAME(MONTH, SEDate)[Month], " +
                       " sum(SESubTotal - SEDiscount) [saleamount] FROM SalesEntries Where Status=1  " +
                       " GROUP BY YEAR(SEDate), DATENAME(MONTH, SEDate)) AS MontlySalesData " +
                       " PIVOT(SUM([saleamount]) " +
                       " FOR Month IN([January], [February], [March], [April], [May], " +
                       " [June], [July], [August], [September], [October], [November], " +
                       " [December])) AS MNamePivot ";

                vmodel.taxableAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(Qry2).AsEnumerable().ToList();


                //monthly total sales amount
                var Qry3 = "SELECT *, CAST(NULL AS DECIMAL(18,2)) AS [total] FROM(SELECT YEAR(SEDate)[Year], DATENAME(MONTH, SEDate)[Month]," +
                           " sum(SEGrandTotal)[saleamount] FROM SalesEntries Where Status=1  GROUP BY YEAR(SEDate), " +
                           " DATENAME(MONTH, SEDate)) AS MontlySalesData " +
                           " PIVOT(SUM([saleamount]) " +
                           " FOR Month IN([January], [February], [March], [April], [May]," +
                           " [June], [July], [August], [September], [October], [November],[December])) AS MNamePivot";

                vmodel.saleAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(Qry3).AsEnumerable().ToList();

                //total tax amount
                var Qry4 = "SELECT *, CAST(NULL AS DECIMAL(18,2)) AS [total] FROM(SELECT YEAR(SEDate)[Year],DATENAME(MONTH, SEDate)[Month], " +
                           " sum(SETaxAmount)[saleamount] FROM SalesEntries Where Status=1  " +
                           " GROUP BY YEAR(SEDate), DATENAME(MONTH, SEDate)) AS MontlySalesData " +
                           " PIVOT(SUM([saleamount]) " +
                           " FOR Month IN([January], [February], [March], [April], [May], " +
                           " [June], [July], [August], [September], [October], [November], [December])) AS MNamePivot";

                vmodel.taxAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(Qry4).AsEnumerable().ToList();

                //sales return

                //monthly sale count
                var QryR1 = "SELECT *, CAST(NULL AS INT) AS [total] FROM(SELECT YEAR(SRDate)[Year],DATENAME(MONTH, SRDate)[Month], " +
                           " COUNT(1)[Sales Count] FROM SalesReturns " +
                           " GROUP BY YEAR(SRDate), DATENAME(MONTH, SRDate)) AS MontlySalesData " +
                           " PIVOT(SUM([Sales Count]) " +
                           " FOR Month IN([January], [February], [March], [April], [May], " +
                           " [June], [July], [August], [September], [October], [November], " +
                           " [December])) AS MNamePivot ";

                vmodel.saleRetCount = db.Database.SqlQueryRaw<MonthWise>(QryR1).AsEnumerable().ToList();

                //monthly taxable amt 
                var QryR2 = "SELECT *, CAST(NULL AS DECIMAL(18,2)) AS [total] FROM(SELECT YEAR(SRDate)[Year],DATENAME(MONTH, SRDate)[Month], " +
                       " sum(SRSubTotal - SRDiscount) [saleamount] FROM SalesReturns " +
                       " GROUP BY YEAR(SRDate), DATENAME(MONTH, SRDate)) AS MontlySalesData " +
                       " PIVOT(SUM([saleamount]) " +
                       " FOR Month IN([January], [February], [March], [April], [May], " +
                       " [June], [July], [August], [September], [October], [November], " +
                       " [December])) AS MNamePivot ";

                vmodel.taxableRetAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(QryR2).AsEnumerable().ToList();


                //monthly total sales amount
                var QryR3 = "SELECT *, CAST(NULL AS DECIMAL(18,2)) AS [total] FROM(SELECT YEAR(SRDate)[Year], DATENAME(MONTH, SRDate)[Month]," +
                           " sum(SRGrandTotal)[saleamount] FROM SalesReturns GROUP BY YEAR(SRDate), " +
                           " DATENAME(MONTH, SRDate)) AS MontlySalesData " +
                           " PIVOT(SUM([saleamount]) " +
                           " FOR Month IN([January], [February], [March], [April], [May]," +
                           " [June], [July], [August], [September], [October], [November],[December])) AS MNamePivot";

                vmodel.saleRetAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(QryR3).AsEnumerable().ToList();

                //total tax amount
                var QryR4 = "SELECT *, CAST(NULL AS DECIMAL(18,2)) AS [total] FROM(SELECT YEAR(SRDate)[Year],DATENAME(MONTH, SRDate)[Month], " +
                           " sum(SRTaxAmount)[saleamount] FROM SalesReturns " +
                           " GROUP BY YEAR(SRDate), DATENAME(MONTH, SRDate)) AS MontlySalesData " +
                           " PIVOT(SUM([saleamount]) " +
                           " FOR Month IN([January], [February], [March], [April], [May], " +
                           " [June], [July], [August], [September], [October], [November], [December])) AS MNamePivot";
                vmodel.taxRetAmount = db.Database.SqlQueryRaw<MonthWiseDecimal>(QryR4).AsEnumerable().ToList();
                // calculate net sale amount



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

        //    //Find Order Column
        //                (todate == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0))
        //                 a.SalesEntryId,
        //                 a.SENo,
        //                 a.BillNo,
        //                 a.SEDate,
        //                 a.SEGrandTotal,
        //                 Customer = b.CustomerName,
        //                 b.TaxRegNo,
        //                 EmpName = d.FirstName + " " + d.LastName,
        //                 a.PayType,
        //                 PaymentStatus = a.Status,
        //                 c.SEPaidAmount,
        //                 a.CustomerType,
        //                 SEBalanceAmount = a.SEGrandTotal - c.SEPaidAmount
        //    //search
        //        // Apply search   
        //                          p.SEPaidAmount.ToString().ToLower().Contains(search.ToLower())
        //                          //p.SEBalanceAmount.ToString().ToLower().Contains(search.ToLower())
        //    //SORT

        //itemswise region list


        public ActionResult ItemWisenew()
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


            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View();
        }
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


            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Sales Item Wise")]
        public ActionResult ItemWise(long?[] ddlItem, string From, string To, long? ddlMC, string SaleType, long? ddlItemBrand, long? ddlEmployee, long? ddlItemCategory, long? ddlSalesman,long? ddlSupplier)
        {
            var items = "";
            if (ddlItem != null)
            {
                items = String.Join(",", ddlItem);
            }




            return RedirectToAction("ViewItemWise", new { item = items, from = From, to = To, ddMC = ddlMC, saletype = SaleType, brand = ddlItemBrand, salesExc = ddlEmployee, category = ddlItemCategory, salesman = ddlSalesman,supplier= ddlSupplier });
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Sales Item Wise")]
        public ActionResult ItemWisenew(long?[] ddlItem, string From, string To, long? ddlMC, string SaleType, long? ddlItemBrand, long? ddlEmployee, long? ddlItemCategory, long? ddlSalesman)
        {
            var items = "";
            if (ddlItem != null)
            {
                items = String.Join(",", ddlItem);
            }




            return RedirectToAction("ViewItemWisenew", new { item = items, from = From, to = To, ddMC = ddlMC, saletype = SaleType, brand = ddlItemBrand, salesExc = ddlEmployee, category = ddlItemCategory, salesman = ddlSalesman });
        }
        




        [HttpPost]
        [QkAuthorize(Roles = "Dev,Sales Item Wise")]
        public ActionResult GetItemWise(string item, string fromdate, string todate, long? ddMC, string Salety, long? Brand, long? SalesExecutive, long? Category, long? Salesman,long? supplier)
        {
            var allshworoom = db.MCs.Where(o => o.MCId == ddMC).Select(o=>o.MCName).FirstOrDefault();
            if (item != "" && allshworoom!= "ALL SHOWROOMS")
            {
                var UserId = User.Identity.GetUserId();
                var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
                var MCList = MCList1;
                if (!MCList.Any() && (ddMC == 0 || ddMC == 1))
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
                SaleType St = new SaleType();
                if (Salety != "")
                {
                    St = (Salety == "1") ? SaleType.Sale : SaleType.Hire;
                };
                List<ItemList> v1 = new List<ItemList>();
                foreach (long itemid in items)
                {


                    var v = (from a in db.Items
                             join e in db.ItemUnits on a.ItemUnitID equals e.ItemUnitID into primary
                             from e in primary.DefaultIfEmpty()
                             join f in db.ItemUnits on a.SubUnitId equals f.ItemUnitID into second
                             from f in second.DefaultIfEmpty()
                             join h in db.SEItemss on a.ItemID equals h.Item into third
                             from h in third.DefaultIfEmpty()
                             join k in db.SalesEntrys on h.SalesEntry equals k.SalesEntryId into forth
                             from k in forth.DefaultIfEmpty()
                             join l in db.Customers on k.Customer equals l.CustomerID into fifth
                             from l in fifth.DefaultIfEmpty()

                             where (itemid == 0 || a.ItemID == itemid) &&
                             (Brand == 0 || a.ItemBrandID == Brand) &&
                             (Category == 0 || a.ItemCategoryID == Category) &&
                             (SalesExecutive == 0 || k.SECashier == SalesExecutive) &&
                             (Salesman == 0 || l.SalesPerson == Salesman) &&
                             (fromdate == "" || EF.Functions.DateDiffDay(k.SEDate, fdate) <= 0) &&
                             (todate == "" || EF.Functions.DateDiffDay(k.SEDate, tdate) >= 0)

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
                                 PriSaleQty = (decimal?)(from i in db.SEItemss
                                                         join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                         where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                                         (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                                         (SalesExecutive == 0 || j.SECashier == SalesExecutive) &&
                                                         (i.Item == a.ItemID && (i.ItemUnit == a.ItemUnitID || i.ItemUnit == null))
                                                         && (ddMC == 0 || ddMC == j.MaterialCenter)
                                                         && (Salety == "" || Salety == null || j.SaleType == St)
                                                         group i by i.ItemId into g
                                                         select new
                                                         {
                                                             Total = g.Sum(x => x.ItemQuantity)
                                                         }).FirstOrDefault().Total ?? 0,


                                 SubSaleQty = (decimal?)(from i in db.SEItemss
                                                         join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                         where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                                         (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                                         (SalesExecutive == 0 || j.SECashier == SalesExecutive) &&
                                                         (i.Item == a.ItemID && i.ItemUnit == a.SubUnitId) &&
                                                         e.ItemUnitName != f.ItemUnitName
                                                         && (ddMC == 0 || ddMC == j.MaterialCenter)
                                                         && (Salety == "" || Salety == null || j.SaleType == St)
                                                         group i by i.ItemId into g
                                                         select new
                                                         {
                                                             Total = g.Sum(x => x.ItemQuantity)
                                                         }).FirstOrDefault().Total ?? 0,

                                 ////PriRetQty = (decimal?)(from i in db.SRItemss
                                 ////                       join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                 ////                       where (fromdate == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                 ////                       (todate == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0) &&
                                 ////                       (i.Item == a.ItemID && i.ItemUnit == a.ItemUnitID)
                                 ////                       && ((!MCList.Any() && ddMC == null) || MCArray.Contains(j.MaterialCenter) || ddMC == j.MaterialCenter)

                                 ////                       group i by i.ItemId into g
                                 ////                       select new
                                 ////                       {
                                 ////                           Total = g.Sum(x => x.ItemQuantity)
                                 ////                       }).FirstOrDefault().Total ?? 0,

                                 //SubRetQty = (decimal?)(from i in db.SRItemss
                                 //                        e.ItemUnitName != f.ItemUnitName
                                 //                       && ((!MCList.Any() && ddMC == null) || MCArray.Contains(j.MaterialCenter) || ddMC == j.MaterialCenter)
                                 //                       group i by i.ItemId into g
                                 //                           Total = g.Sum(x => x.ItemQuantity)
                                 //                       }).FirstOrDefault().Total ?? 0,

                                 SaleAmt = (decimal?)(from i in db.SEItemss
                                                      join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                      where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                                      (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                                      (SalesExecutive == 0 || j.SECashier == SalesExecutive) &&
                                                      (i.Item == a.ItemID)
                                                      && (ddMC == 0 || ddMC == j.MaterialCenter)
                                                      && (Salety == "" || Salety == null || j.SaleType == St)
                                                      group i by i.ItemId into g
                                                      select new
                                                      {
                                                          Total = g.Sum(x => (x.ItemUnitPrice*x.ItemQuantity) - x.ItemDiscount)
                                                      }).FirstOrDefault().Total ?? 0,
                                 PriSale = (decimal?)(from i in db.SEItemss
                                                      join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                      where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                                      (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                                      (SalesExecutive == 0 || j.SECashier == SalesExecutive) &&
                                                      (i.Item == a.ItemID && i.ItemUnit == e.ItemUnitID)
                                                      && (ddMC == 0 || ddMC == j.MaterialCenter)
                                                      && (Salety == "" || Salety == null || j.SaleType == St)
                                                      group i by i.ItemId into g
                                                      select new
                                                      {
                                                          Total = g.Sum(x => (x.ItemUnitPrice * x.ItemQuantity) - x.ItemDiscount)
                                                      }).FirstOrDefault().Total ?? 0,
                                 SubSale = (decimal?)(from i in db.SEItemss
                                                      join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                      where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                                      (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                                      (SalesExecutive == 0 || j.SECashier == SalesExecutive) &&
                                                      (i.Item == a.ItemID && i.ItemUnit == f.ItemUnitID) &&
                                                      e.ItemUnitName != f.ItemUnitName
                                                      && (ddMC == 0 || ddMC == j.MaterialCenter)
                                                      && (Salety == "" || Salety == null || j.SaleType == St)
                                                      group i by i.ItemId into g
                                                      select new
                                                      {
                                                          Total = g.Sum(x => (x.ItemUnitPrice * x.ItemQuantity) - x.ItemDiscount)
                                                      }).FirstOrDefault().Total ?? 0,

                                 //RetunAmt = (decimal?)(from i in db.SRItemss
                                 //                      (i.Item == a.ItemID)
                                 //                      && ((!MCList.Any() && ddMC == null) || MCArray.Contains(j.MaterialCenter) || ddMC == j.MaterialCenter)
                                 //                      group i by i.ItemId into g
                                 //                          Total = g.Sum(x => x.ItemSubTotal - x.ItemDiscount)
                                 //                      }).FirstOrDefault().Total ?? 0,

                                 //NoOfVchSale = (int?)(from i in db.SEItemss
                                 //                     (i.Item == a.ItemID)
                                 //                     && ((!MCList.Any() && ddMC == null) || MCArray.Contains(j.MaterialCenter) || ddMC == j.MaterialCenter)
                                 //                     && (Salety == "" || Salety == null || j.SaleType == St)
                                 //                         saleid = i.SalesEntry
                                 //                     }).GroupBy(x => x.saleid).Count() ?? 0,

                                 //NoOfVchReturn = (int?)(from i in db.SRItemss
                                 //                       (i.Item == a.ItemID)
                                 //                       && ((!MCList.Any() && ddMC == null) || MCArray.Contains(j.MaterialCenter) || ddMC == j.MaterialCenter)
                                 //                           saleid = i.SalesReturnId
                                 //                       }).GroupBy(x => x.saleid).Count() ?? 0,

                             }).Distinct();
                    v = v.OrderBy(b => b.Itemname);
                    recordsTotal = v.Count();
                    var datas = v.ToList();
                
                    if (supplier != null && supplier != 0)
                    {
                        datas = (from a in datas
                                join b in db.PEItemss on a.ItemID equals b.Item
                                join c in db.PurchaseEntrys on b.PurchaseEntry equals c.PurchaseEntryId
                                join d in db.Suppliers on c.Supplier equals d.SupplierID
                                where d.SupplierID == supplier
                                select a).Distinct().ToList();
                    }
                    var mydata =
                        (from b in datas
                         select new ItemList
                         {
                             ItemID = b.ItemID,
                             Item = b.Item,
                             ItemUnitID = b.ItemUnitID,
                             SubUnitId = b.SubUnitId,
                             PriUnit = (b.PriUnit != null) ? b.PriUnit : "",
                             SubUnit = (b.SubUnit != null) ? b.SubUnit : "",
                             ConFactor = b.ConFactor,
                             PartNumber = (b.PartNumber != null && b.PartNumber != "") ? b.PartNumber : "",


                             PriSaleQty = (b.PriSaleQty + (int)(b.SubSaleQty / b.ConFactor)),
                             SubSaleQty = (b.SubSaleQty % b.ConFactor),



                             SaleAmt = b.SaleAmt,
                             //RetunAmt = b.RetunAmt,
                             //NoOfVchSale = b.NoOfVchSale,
                             //NoOfVchReturn = b.NoOfVchReturn,
                             Itemname = b.Itemname,
                         }).OrderBy(a => a.Itemname).ToList();

                    v1.AddRange(mydata);


                }

                var  data = v1.Skip(skip).Take(pageSize).ToList();
                recordsTotal = v1.Count();

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                string result = javaScriptSerializer.Serialize(new {/* draw = draw, */recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                var results = new ContentResult
                {
                    Content = result,
                    ContentType = "application/json"
                };

                return results;
            }
            if (item != "" && allshworoom == "ALL SHOWROOMS")
            {
                var UserId = User.Identity.GetUserId();
                var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
                var MCList = MCList1;
                if (!MCList.Any() && (ddMC == 0 || ddMC == 1))
                {
                    MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
                }
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
                long[] MCArray =  { 20085, 20086, 20087 };
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
                SaleType St = new SaleType();
                if (Salety != "")
                {
                    St = (Salety == "1") ? SaleType.Sale : SaleType.Hire;
                };
                List<ItemList> v1 = new List<ItemList>();
                foreach (long itemid in items)
                {


                    var v = (from a in db.Items
                             join e in db.ItemUnits on a.ItemUnitID equals e.ItemUnitID into primary
                             from e in primary.DefaultIfEmpty()
                             join f in db.ItemUnits on a.SubUnitId equals f.ItemUnitID into second
                             from f in second.DefaultIfEmpty()
                             join h in db.SEItemss on a.ItemID equals h.Item into third
                             from h in third.DefaultIfEmpty()
                             join k in db.SalesEntrys on h.SalesEntry equals k.SalesEntryId into forth
                             from k in forth.DefaultIfEmpty()
                             join l in db.Customers on k.Customer equals l.CustomerID into fifth
                             from l in fifth.DefaultIfEmpty()

                             where (itemid == 0 || a.ItemID == itemid) &&
                             (Brand == 0 || a.ItemBrandID == Brand) &&
                             (Category == 0 || a.ItemCategoryID == Category) &&
                             (SalesExecutive == 0 || k.SECashier == SalesExecutive) &&
                             (Salesman == 0 || l.SalesPerson == Salesman) &&
                             (fromdate == "" || EF.Functions.DateDiffDay(k.SEDate, fdate) <= 0) &&
                             (todate == "" || EF.Functions.DateDiffDay(k.SEDate, tdate) >= 0)

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
                                 PriSaleQty = (decimal?)(from i in db.SEItemss
                                                         join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                         where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                                         (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                                         (SalesExecutive == 0 || j.SECashier == SalesExecutive) &&
                                                         (i.Item == a.ItemID && (i.ItemUnit == a.ItemUnitID || i.ItemUnit == null))
                                                       //  && (ddMC == 0 || ddMC == j.MaterialCenter)
                                                       && (MCArray.Contains((long)j.MaterialCenter)) 
                                                         && (Salety == "" || Salety == null || j.SaleType == St)
                                                         group i by i.ItemId into g
                                                         select new
                                                         {
                                                             Total = g.Sum(x => x.ItemQuantity)
                                                         }).FirstOrDefault().Total ?? 0,


                                 SubSaleQty = (decimal?)(from i in db.SEItemss
                                                         join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                         where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                                         (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                                         (SalesExecutive == 0 || j.SECashier == SalesExecutive) &&
                                                         (i.Item == a.ItemID && i.ItemUnit == a.SubUnitId) &&
                                                         e.ItemUnitName != f.ItemUnitName
                                  //                        && (ddMC == 0 || ddMC == j.MaterialCenter)
                                  && (MCArray.Contains((long)j.MaterialCenter))
                                                         && (Salety == "" || Salety == null || j.SaleType == St)
                                                         group i by i.ItemId into g
                                                         select new
                                                         {
                                                             Total = g.Sum(x => x.ItemQuantity)
                                                         }).FirstOrDefault().Total ?? 0,

                                 ////PriRetQty = (decimal?)(from i in db.SRItemss
                                 ////                       join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                 ////                       where (fromdate == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                 ////                       (todate == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0) &&
                                 ////                       (i.Item == a.ItemID && i.ItemUnit == a.ItemUnitID)
                                 ////                       && ((!MCList.Any() && ddMC == null) || MCArray.Contains(j.MaterialCenter) || ddMC == j.MaterialCenter)

                                 ////                       group i by i.ItemId into g
                                 ////                       select new
                                 ////                       {
                                 ////                           Total = g.Sum(x => x.ItemQuantity)
                                 ////                       }).FirstOrDefault().Total ?? 0,

                                 //SubRetQty = (decimal?)(from i in db.SRItemss
                                 //                        e.ItemUnitName != f.ItemUnitName
                                 //                       && ((!MCList.Any() && ddMC == null) || MCArray.Contains(j.MaterialCenter) || ddMC == j.MaterialCenter)
                                 //                       group i by i.ItemId into g
                                 //                           Total = g.Sum(x => x.ItemQuantity)
                                 //                       }).FirstOrDefault().Total ?? 0,

                                 SaleAmt = (decimal?)(from i in db.SEItemss
                                                      join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                      where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                                      (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                                      (SalesExecutive == 0 || j.SECashier == SalesExecutive) &&
                                                      (i.Item == a.ItemID)
                                                      //&& (ddMC == 0 || ddMC == j.MaterialCenter)
                                                && (MCArray.Contains((long)j.MaterialCenter))
                                                      && (Salety == "" || Salety == null || j.SaleType == St)
                                                      group i by i.ItemId into g
                                                      select new
                                                      {
                                                          Total = g.Sum(x => (x.ItemUnitPrice * x.ItemQuantity) - x.ItemDiscount)
                                                      }).FirstOrDefault().Total ?? 0,
                                 PriSale = (decimal?)(from i in db.SEItemss
                                                      join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                      where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                                      (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                                      (SalesExecutive == 0 || j.SECashier == SalesExecutive) &&
                                                      (i.Item == a.ItemID && i.ItemUnit == e.ItemUnitID)
                                                      //&& (ddMC == 0 || ddMC == j.MaterialCenter)
                                           && (MCArray.Contains((long)j.MaterialCenter))
                                                      && (Salety == "" || Salety == null || j.SaleType == St)
                                                      group i by i.ItemId into g
                                                      select new
                                                      {
                                                          Total = g.Sum(x => (x.ItemUnitPrice * x.ItemQuantity) - x.ItemDiscount)
                                                      }).FirstOrDefault().Total ?? 0,
                                 SubSale = (decimal?)(from i in db.SEItemss
                                                      join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                      where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                                      (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                                      (SalesExecutive == 0 || j.SECashier == SalesExecutive) &&
                                                      (i.Item == a.ItemID && i.ItemUnit == f.ItemUnitID) &&
                                                      e.ItemUnitName != f.ItemUnitName
                                            //  && (ddMC == 0 || ddMC == j.MaterialCenter)
                                            && (MCArray.Contains((long)j.MaterialCenter))
                                                      && (Salety == "" || Salety == null || j.SaleType == St)
                                                      group i by i.ItemId into g
                                                      select new
                                                      {
                                                          Total = g.Sum(x => (x.ItemUnitPrice * x.ItemQuantity) - x.ItemDiscount)
                                                      }).FirstOrDefault().Total ?? 0,

                                 //RetunAmt = (decimal?)(from i in db.SRItemss
                                 //                      (i.Item == a.ItemID)
                                 //                      && ((!MCList.Any() && ddMC == null) || MCArray.Contains(j.MaterialCenter) || ddMC == j.MaterialCenter)
                                 //                      group i by i.ItemId into g
                                 //                          Total = g.Sum(x => x.ItemSubTotal - x.ItemDiscount)
                                 //                      }).FirstOrDefault().Total ?? 0,

                                 //NoOfVchSale = (int?)(from i in db.SEItemss
                                 //                     (i.Item == a.ItemID)
                                 //                     && ((!MCList.Any() && ddMC == null) || MCArray.Contains(j.MaterialCenter) || ddMC == j.MaterialCenter)
                                 //                     && (Salety == "" || Salety == null || j.SaleType == St)
                                 //                         saleid = i.SalesEntry
                                 //                     }).GroupBy(x => x.saleid).Count() ?? 0,

                                 //NoOfVchReturn = (int?)(from i in db.SRItemss
                                 //                       (i.Item == a.ItemID)
                                 //                       && ((!MCList.Any() && ddMC == null) || MCArray.Contains(j.MaterialCenter) || ddMC == j.MaterialCenter)
                                 //                           saleid = i.SalesReturnId
                                 //                       }).GroupBy(x => x.saleid).Count() ?? 0,

                             }).Distinct();
                    v = v.OrderBy(b => b.Itemname);
                    recordsTotal = v.Count();
                    var datas = v.ToList();
                    
                    if (supplier != null && supplier != 0)
                    {
                        datas = (from a in datas
                                join b in db.PEItemss on a.ItemID equals b.Item
                                join c in db.PurchaseEntrys on b.PurchaseEntry equals c.PurchaseEntryId
                                join d in db.Suppliers on c.Supplier equals d.SupplierID
                                where d.SupplierID == supplier
                                select a).Distinct().ToList();
                    }
                    var mydata =
                        (from b in datas
                         select new ItemList
                         {
                             ItemID = b.ItemID,
                             Item = b.Item,
                             ItemUnitID = b.ItemUnitID,
                             SubUnitId = b.SubUnitId,
                             PriUnit = (b.PriUnit != null) ? b.PriUnit : "",
                             SubUnit = (b.SubUnit != null) ? b.SubUnit : "",
                             ConFactor = b.ConFactor,
                             PartNumber = (b.PartNumber != null && b.PartNumber != "") ? b.PartNumber : "",


                             PriSaleQty = (b.PriSaleQty + (int)(b.SubSaleQty / b.ConFactor)),
                             SubSaleQty = (b.SubSaleQty % b.ConFactor),



                             SaleAmt = b.SaleAmt,
                             //RetunAmt = b.RetunAmt,
                             //NoOfVchSale = b.NoOfVchSale,
                             //NoOfVchReturn = b.NoOfVchReturn,
                             Itemname = b.Itemname,
                         }).OrderBy(a => a.Itemname).ToList();

                    v1.AddRange(mydata);


                }

                 var data = v1.ToList();
                recordsTotal = v1.Count();

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                string result = javaScriptSerializer.Serialize(new {/* draw = draw, */recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                var results = new ContentResult
                {
                    Content = result,
                    ContentType = "application/json"
                };

                return results;
            }
            else
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
                SaleType St = new SaleType();
                if (Salety != "")
                {
                    St = (Salety == "1") ? SaleType.Sale : SaleType.Hire;
                };
                var v = (from a in db.Items
                         join e in db.ItemUnits on a.ItemUnitID equals e.ItemUnitID into primary
                         from e in primary.DefaultIfEmpty()
                         join f in db.ItemUnits on a.SubUnitId equals f.ItemUnitID into second
                         from f in second.DefaultIfEmpty()
                         join h in db.ItemBrands on a.ItemBrandID equals h.ItemBrandID into third
                         from h in third.DefaultIfEmpty()
                         join k in db.SEItemss on a.ItemID equals k.Item into forth
                         from k in forth.DefaultIfEmpty()
                         join l in db.SalesEntrys on k.SalesEntry equals l.SalesEntryId into fifth
                         from l in fifth.DefaultIfEmpty()
                         join m in db.Customers on l.Customer equals m.CustomerID into sixth
                         from m in sixth.DefaultIfEmpty()
                         where (Brand == 0 || a.ItemBrandID == Brand) &&
                         (Category == 0 || a.ItemCategoryID == Category) &&
                         (SalesExecutive == 0 || l.SECashier == SalesExecutive) &&
                         (Salesman == 0 || m.SalesPerson == Salesman) &&
                         (fromdate == "" || EF.Functions.DateDiffDay(l.SEDate, fdate) <= 0) &&
                         (todate == "" || EF.Functions.DateDiffDay(l.SEDate, tdate) >= 0)

                         select new
                         {
                             ItemID = a.ItemID,
                             Itemname = a.ItemName,
                             Item = a.ItemCode + "-" + a.ItemName,
                             a.Barcode,
                             PriUnit = e.ItemUnitName,
                             SubUnit = f.ItemUnitName,
                             ConFactor = a.ConFactor != 0 ? a.ConFactor : 1,
                             ItemUnitID = a.ItemUnitID,
                             SubUnitId = a.SubUnitId,
                             SellingPrice = a.SellingPrice,
                             PartNumber = a.PartNumber,
                             SaleType = a.Branch,
                             PriSaleQty = (decimal?)(from i in db.SEItemss
                                                     join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                     where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                                     (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                                     (SalesExecutive == 0 || j.SECashier == SalesExecutive) &&
                                                     (i.Item == a.ItemID && (i.ItemUnit == a.ItemUnitID || i.ItemUnit == null))
                                                     && (ddMC == 0 || ddMC == j.MaterialCenter)
                                                     && (Salety == "" || Salety == null || j.SaleType == St)
                                                     group i by i.ItemId into g
                                                     select new
                                                     {
                                                         Total = g.Sum(x => x.ItemQuantity)
                                                     }).FirstOrDefault().Total ?? 0,

                             SubSaleQty = (decimal?)(from i in db.SEItemss
                                                     join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                     where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                                     (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                                     (SalesExecutive == 0 || j.SECashier == SalesExecutive) &&
                                                     (i.Item == a.ItemID && i.ItemUnit == a.SubUnitId) &&
                                                     e.ItemUnitName != f.ItemUnitName
                                                     && (ddMC == 0 || ddMC == j.MaterialCenter)
                                                     && (Salety == "" || Salety == null || j.SaleType == St)
                                                     group i by i.ItemId into g
                                                     select new
                                                     {
                                                         Total = g.Sum(x => x.ItemQuantity)
                                                     }).FirstOrDefault().Total ?? 0,
                             SaleAmt = (decimal?)(from i in db.SEItemss
                                                  join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                  where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                                  (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                                  (SalesExecutive == 0 || j.SECashier == SalesExecutive) &&
                                                  (i.Item == a.ItemID)
                                                  && (ddMC == 0 || ddMC == j.MaterialCenter)
                                                  && (Salety == "" || Salety == null || j.SaleType == St)
                                                  group i by i.ItemId into g
                                                  select new
                                                  {
                                                      Total = g.Sum(x => (x.ItemUnitPrice*x.ItemQuantity) - x.ItemDiscount)
                                                  }).FirstOrDefault().Total ?? 0,
                             purchaseamount =a.PurchasePrice,
                             SaleAmtpf = (decimal?)(from i in db.SEItemss
                                                  join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                  join p in db.Items on i.Item equals p.ItemID
                                                  where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                                  (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                                  (SalesExecutive == 0 || j.SECashier == SalesExecutive) &&
                                                  (i.Item == a.ItemID)
                                                  && (ddMC == 0 || ddMC == j.MaterialCenter)
                                                  && (Salety == "" || Salety == null || j.SaleType == St)
                                                  group new { p.PurchasePrice,i.ItemUnitPrice, i.ItemQuantity } by p.ItemID into g
                                                  select new
                                                  {
                                                      Total = g.Sum(x => (x.ItemUnitPrice * x.ItemQuantity)-(x.PurchasePrice * x.ItemQuantity))
                                                  }).FirstOrDefault().Total ?? 0,
                             PriSale = (decimal?)(from i in db.SEItemss
                                                  join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                  where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                                  (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                                  (SalesExecutive == 0 || j.SECashier == SalesExecutive) &&
                                                  (i.Item == a.ItemID && i.ItemUnit == e.ItemUnitID)
                                                  && (ddMC == 0 || ddMC == j.MaterialCenter)
                                                  && (Salety == "" || Salety == null || j.SaleType == St)
                                                  group i by i.ItemId into g
                                                  select new
                                                  {
                                                      Total = g.Sum(x => (x.ItemUnitPrice * x.ItemQuantity) - x.ItemDiscount)
                                                  }).FirstOrDefault().Total ?? 0,
                             SubSale = (decimal?)(from i in db.SEItemss
                                                  join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                  where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                                  (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                                  (SalesExecutive == 0 || j.SECashier == SalesExecutive) &&
                                                  (i.Item == a.ItemID && i.ItemUnit == f.ItemUnitID) &&
                                                  e.ItemUnitName != f.ItemUnitName
                                                  && (ddMC == 0 || ddMC == j.MaterialCenter)
                                                  && (Salety == "" || Salety == null || j.SaleType == St)
                                                  group i by i.ItemId into g
                                                  select new
                                                  {
                                                      Total = g.Sum(x => (x.ItemUnitPrice * x.ItemQuantity) - x.ItemDiscount)
                                                  }).FirstOrDefault().Total ?? 0,

                         }).Distinct();
                v = v.OrderBy(b => b.Itemname);
                recordsTotal = v.Count();
                var datas = v.Skip(skip).Take(pageSize).ToList();
                var mydata =
                        (from b in datas
                         select new
                         {
                             ItemID = b.ItemID,
                             Item = b.Item,
                            b.purchaseamount,
                             ItemUnitID = b.ItemUnitID,
                             SubUnitId = b.SubUnitId,
                             PriUnit = (b.PriUnit != null) ? b.PriUnit : "",
                             SubUnit = (b.SubUnit != null) ? b.SubUnit : "",
                             ConFactor = b.ConFactor,
                             PartNumber = (b.PartNumber != null && b.PartNumber != "") ? b.PartNumber : "",
                            Barcode= b.Barcode,

                             PriSaleQty = (b.PriSaleQty + (int)(b.SubSaleQty / b.ConFactor)),
                             SubSaleQty = (b.SubSaleQty % b.ConFactor),


                             SaleAmtpf=b.SaleAmtpf,
                             SaleAmt = b.SaleAmt,
                             //RetunAmt = b.RetunAmt,
                             //NoOfVchSale = b.NoOfVchSale,
                             //NoOfVchReturn = b.NoOfVchReturn,
                             Itemname = b.Itemname,
                         }).OrderBy(a => a.Itemname).ToList();
                
                var data = mydata.ToList();
                if(supplier!=null && supplier!=0)
                {
                    data = (from a in data
                                        join b in db.PEItemss on a.ItemID equals b.Item
                                        join c in db.PurchaseEntrys on b.PurchaseEntry equals c.PurchaseEntryId
                                        join d in db.Suppliers on c.Supplier equals d.SupplierID
                                        where d.SupplierID == supplier
                                        select a).Distinct().ToList();
                }
                recordsTotal = data.Count();

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                string result = javaScriptSerializer.Serialize(new {/* draw = draw, */recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                var results = new ContentResult
                {
                    Content = result,
                    ContentType = "application/json"
                };

                return results;
            }

        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Sales Item Wise")]
        public ActionResult GetItemWisenew(string item, string fromdate, string todate, long? ddMC, string Salety, long? Brand, long? SalesExecutive, long? Category, long? Salesman)
        {
            
                var UserId = User.Identity.GetUserId();
                var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
                var MCList = MCList1;
                if (!MCList.Any() && (ddMC == 0 || ddMC == 1))
                {
                    MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
                }
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
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
                SaleType St = new SaleType();
                if (Salety != "")
                {
                    St = (Salety == "1") ? SaleType.Sale : SaleType.Hire;
                };
                List<ItemList> v1 = new List<ItemList>();


            var v = (from a in db.Items
                     join e in db.ItemUnits on a.ItemUnitID equals e.ItemUnitID into primary
                     from e in primary.DefaultIfEmpty()
                     join f in db.ItemUnits on a.SubUnitId equals f.ItemUnitID into second
                     from f in second.DefaultIfEmpty()
                     join h in db.SEItemss on a.ItemID equals h.Item into third
                     from h in third.DefaultIfEmpty()
                     join k in db.SalesEntrys on h.SalesEntry equals k.SalesEntryId into forth
                     from k in forth.DefaultIfEmpty()
                     join l in db.Customers on k.Customer equals l.CustomerID into fifth
                     from l in fifth.DefaultIfEmpty()

                     where
                     (Brand == 0 || a.ItemBrandID == Brand) &&
                     (Category == 0 || a.ItemCategoryID == Category) &&
                     (SalesExecutive == 0 || k.SECashier == SalesExecutive) &&
                     (Salesman == 0 || l.SalesPerson == Salesman) &&
                     (fromdate == "" || EF.Functions.DateDiffDay(k.SEDate, fdate) <= 0) &&
                     (todate == "" || EF.Functions.DateDiffDay(k.SEDate, tdate) >= 0)

                     select new
                     {
                         ItemID = a.ItemID,
                         Itemname = a.ItemName,
                         Item = a.ItemCode + "-" + a.ItemName,
                         CategoryName = a.ItemCategorys.ItemCategoryName,
                                 PriUnit = e.ItemUnitName,
                         SubUnit = f.ItemUnitName,
                         ConFactor = a.ConFactor != 0 ? a.ConFactor : 1,
                         ItemUnitID = a.ItemUnitID,
                         SubUnitId = a.SubUnitId,
                         SellingPrice = h != null ? h.ItemUnitPrice : 0m,
                         ItemQuantity = h != null ? h.ItemQuantity : 0m,
                         sellingcost = h != null ? h.ItemQuantity * h.ItemUnitPrice : 0m,
                         PartNumber = a.PartNumber,
                         SaleType = a.Branch,
                         a.Barcode,
                         k.SEDate,
                         k.BillNo,
                         a.PurchasePrice,
                         purchasecost = h != null ? h.ItemQuantity * a.PurchasePrice : 0m,
                         profit = h != null ? (h.ItemQuantity * h.ItemUnitPrice) - (h.ItemQuantity * a.PurchasePrice) : 0m
                     }).Distinct();
                    v = v.OrderBy(b => b.SEDate);
                   
                


                

                var data = v.ToList();
                recordsTotal = v.Count();

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                string result = javaScriptSerializer.Serialize(new {/* draw = draw, */recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                var results = new ContentResult
                {
                    Content = result,
                    ContentType = "application/json"
                };

                return results;
            
           
        }


        [HttpGet]
        [QkAuthorize(Roles = "Dev,Sales Item Wise")]
        public ActionResult ViewItemWise(long? supplier,string item, string from, string to, long? ddMC, string saletype, long? brand, long? salesExc, long? category, long? salesman)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            //                    where a.ItemID == item
            //                        ItemName = a.ItemCode + "-" + a.ItemName

            ViewBag.item = item;
            ViewBag.ddlmc = ddMC;
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            ViewBag.SaleTy = saletype;
            ViewBag.supplierid = supplier;
            companySet();
            return View();
        }



        [HttpGet]
        [QkAuthorize(Roles = "Dev,Sales Item Wise")]
        public ActionResult ViewItemWisenew(string item, string from, string to, long? ddMC, string saletype, long? brand, long? salesExc, long? category, long? salesman)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            //                    where a.ItemID == item
            //                        ItemName = a.ItemCode + "-" + a.ItemName

            ViewBag.item = item;
            ViewBag.ddlmc = ddMC;
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            ViewBag.SaleTy = saletype;
            companySet();
            return View();
        }


        //CategoryWise Region list
        [QkAuthorize(Roles = "Dev,Sales Category Wise")]
        public ActionResult CategoryWise(long? category, string from, string to, long? ddMC, string saletype)
        {

            if (category != null)
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
            ViewBag.SaleTy = saletype;

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

            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View();
        }
        // [QkAuthorize(Roles = "Dev,Sales Category Wise")]

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Sales Category Wise")]
        public ActionResult GetCategoryWise(long? category, string fromdate, string todate, long? ddMC, string Salety)
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
            SaleType St = new SaleType();
            if (Salety != null)
            {
                St = (Salety == "1") ? SaleType.Sale : SaleType.Hire;
            };
            var v = (from a in db.ItemCategorys

                     where (category == 0 || a.ItemCategoryID == category)
                     select new
                     {
                         Category = a.ItemCategoryName,
                         a.ItemCategoryID,
                         SaleAmt = (decimal?)(from i in db.SEItemss
                                              join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                              join k in db.Items on i.Item equals k.ItemID into itm
                                              from k in itm.DefaultIfEmpty()
                                              where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                              (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                              (k.ItemCategoryID == a.ItemCategoryID)
                                              && (Salety == "" || Salety == null || j.SaleType == St)
                                              && ((!MCList.Any() && ddMC == null) || MCArray.Contains(j.MaterialCenter) || ddMC == j.MaterialCenter)
                                              select new
                                              {
                                                  Total = i.ItemSubTotal - i.ItemDiscount
                                              }).Sum(x => x.Total) ?? 0,


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




                         NoOfVchSale = (int?)(from i in db.SEItemss
                                              join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                              join k in db.Items on i.Item equals k.ItemID into itm
                                              from k in itm.DefaultIfEmpty()
                                              where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                              (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                              (k.ItemCategoryID == a.ItemCategoryID)
                                              && (Salety == "" || Salety == null || j.SaleType == St)
                                              && ((!MCList.Any() && ddMC == null) || MCArray.Contains(j.MaterialCenter) || ddMC == j.MaterialCenter)
                                              select new
                                              {
                                                  saleid = i.SalesEntry
                                              }).GroupBy(x => x.saleid).Count() ?? 0,


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
                         o.SaleAmt,
                         o.RetunAmt,
                         o.NoOfVchSale,
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
        // [QkAuthorize(Roles = "Dev,Sales Category Wise")]
        public ActionResult ViewCategoryWise(long? category, string from, string to, long? ddMC, string saletype)
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
            ViewBag.SaleTy = saletype;
            return View();
        }



        ////Brandwise region
        [QkAuthorize(Roles = "Dev,Sales Brand Wise")]
        public ActionResult BrandWise(long? brand, string from, string to, string saletype, long? ddmc)
        {
            if (brand != null)
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
            ViewBag.SaleTy = saletype;
            ViewBag.Brand = QkSelect.List(
             new List<SelectListItem>
             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
             }, "Value", "Text", 1);
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

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
        //[HttpPost]
        //[QkAuthorize(Roles = "Dev,Sales Brand Wise")]

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Sales Brand Wise")]
        public ActionResult GetBrandWise(long? brand, string fromdate, string todate, string Salety, long? ddmc)
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
            SaleType St = new SaleType();
            if (Salety == "1" || Salety == "2")
            {
                St = (Salety == "1") ? SaleType.Sale : SaleType.Hire;
            }
            var v = (from a in db.ItemBrands
                     where (brand == 0 || a.ItemBrandID == brand)
                     select new
                     {
                         Brand = a.ItemBrandName,
                         a.ItemBrandID,
                         SaleAmt = (decimal?)(from i in db.SEItemss
                                              join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                              join k in db.Items on i.Item equals k.ItemID into itm
                                              from k in itm.DefaultIfEmpty()
                                              where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                              (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                              (k.ItemBrandID == a.ItemBrandID) && (Salety == "" || Salety == null || j.SaleType == St)
                                               && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                              select new
                                              {
                                                  Total = i.ItemSubTotal - i.ItemDiscount
                                              }).Sum(x => x.Total) ?? 0,


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

                         NoOfVchSale = (int?)(from i in db.SEItemss
                                              join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                              join k in db.Items on i.Item equals k.ItemID into itm
                                              from k in itm.DefaultIfEmpty()
                                              where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                              (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                             (k.ItemBrandID == a.ItemBrandID) && (Salety == "" || Salety == null || j.SaleType == St)
                                              && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                              //group i by i.SalesEntry into g
                                              select new
                                              {
                                                  saleid = i.SalesEntry
                                              }).GroupBy(x => x.saleid).Count() ?? 0,


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
                         o.SaleAmt,
                         o.RetunAmt,
                         o.NoOfVchSale,
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
        [QkAuthorize(Roles = "Dev,Sales Brand Wise")]
        public ActionResult ViewBrandWise(long? brand, string from, string to, string saletype, long? ddmc)
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
            ViewBag.SaleTy = saletype;
            return View();
        }




        [QkAuthorize(Roles = "Dev,Sales Executive Wise")]
        public ActionResult SalesExecWise()
        {
            return View();
        }

        //SalesExecutiveWise list region
        [QkAuthorize(Roles = "Dev,Sales Executive Wise")]
        public ActionResult SalesExecutiveWise(long? salesexec, string from, string to, long? ddmc)
        {
            if (salesexec != 0 && salesexec != null)
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

            ViewBag.SalesExec = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
            }, "Value", "Text", 0);
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

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
        //[HttpPost]
        //[QkAuthorize(Roles = "Dev,Sales Executive Wise")]
        [QkAuthorize(Roles = "Dev,Sales Executive Wise")]
        public ActionResult GetSalesExeWise(long? salesexec, string fromdate, string todate, string Salety, long? ddmc)
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
            SaleType St = new SaleType();
            if (Salety == "1" || Salety == "2")
            {
                St = (Salety == "1") ? SaleType.Sale : SaleType.Hire;
            }
            var v = (from a in db.Employees
                     where (salesexec == 0 || a.EmployeeId == salesexec)
                     select new
                     {
                         a.EmployeeId,
                         a.FirstName,
                         employee = a.FirstName + " " + a.MiddleName + " " + a.LastName,

                         SaleAmt = (decimal?)(from i in db.SalesEntrys
                                              where (fromdate == "" || EF.Functions.DateDiffDay(i.SEDate, fdate) <= 0) && i.Status == 1 &&
                                              (todate == "" || EF.Functions.DateDiffDay(i.SEDate, tdate) >= 0) &&
                                              (i.SECashier == a.EmployeeId) && (Salety == "" || Salety == null || i.SaleType == St)
                                              && ((!MCList.Any() && ddmc == null) || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                              // group i by i.Customer into g
                                              select new
                                              {
                                                  Total = i.SESubTotal - i.SEDiscount  // Calc fix (N4): net of discount (was gross SESubTotal; legacy intent shown in the line below)
                                              }).Sum(x => x.Total) ?? 0,
                         //.FirstOrDefault().Total ?? 0,
                         SaletaxAmt = (decimal?)(from i in db.SalesEntrys
                                                 where (fromdate == "" || EF.Functions.DateDiffDay(i.SEDate, fdate) <= 0) && i.Status == 1 &&
                                                 (todate == "" || EF.Functions.DateDiffDay(i.SEDate, tdate) >= 0) &&
                                                 (i.SECashier == a.EmployeeId) && (Salety == "" || Salety == null || i.SaleType == St)
                                                 && ((!MCList.Any() && ddmc == null) || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                                 group i by i.Customer into g
                                                 select new
                                                 {
                                                     Total = g.Sum(x => x.SETaxAmount)
                                                 }).Sum(z => (decimal?)z.Total) ?? 0,   // Calc fix (N4): sum across ALL the employee's customers (was FirstOrDefault — kept only the first)
                         SaletotAmt = (decimal?)(from i in db.SalesEntrys
                                                 where (fromdate == "" || EF.Functions.DateDiffDay(i.SEDate, fdate) <= 0) && i.Status == 1 &&
                                                 (todate == "" || EF.Functions.DateDiffDay(i.SEDate, tdate) >= 0) &&
                                                 (i.SECashier == a.EmployeeId) && (Salety == "" || Salety == null || i.SaleType == St)
                                                 && ((!MCList.Any() && ddmc == null) || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                                 // group i by i.Customer into g
                                                 select new
                                                 {
                                                     //Total = g.Sum(x => x.SEGrandTotal)
                                                     total = i.SEGrandTotal
                                                 }).Sum(x => x.total) ?? 0,
                         RetunAmt = (decimal?)(from i in db.SalesReturns
                                               where (fromdate == "" || EF.Functions.DateDiffDay(i.SRDate, fdate) <= 0) &&
                                               (todate == "" || EF.Functions.DateDiffDay(i.SRDate, tdate) >= 0) &&
                                               (i.SRCashier == a.EmployeeId)
                                               && ((!MCList.Any() && ddmc == null) || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                               group i by i.Customer into g
                                               select new
                                               {
                                                   Total = g.Sum(x => x.SRSubTotal - x.SRDiscount)
                                               }).Sum(z => (decimal?)z.Total) ?? 0,   // Calc fix (N4): sum across ALL the employee's customers (was FirstOrDefault — kept only the first)
                         RetuntaxAmt = (decimal?)(from i in db.SalesReturns
                                                  where (fromdate == "" || EF.Functions.DateDiffDay(i.SRDate, fdate) <= 0) &&
                                                  (todate == "" || EF.Functions.DateDiffDay(i.SRDate, tdate) >= 0) &&
                                                  (i.SRCashier == a.EmployeeId)
                                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                                  group i by i.Customer into g
                                                  select new
                                                  {
                                                      Total = g.Sum(x => x.SRTaxAmount)
                                                  }).Sum(z => (decimal?)z.Total) ?? 0,   // Calc fix (N4): sum across ALL the employee's customers (was FirstOrDefault — kept only the first)
                         RetuntotAmt = (decimal?)(from i in db.SalesReturns
                                                  where (fromdate == "" || EF.Functions.DateDiffDay(i.SRDate, fdate) <= 0) &&
                                                  (todate == "" || EF.Functions.DateDiffDay(i.SRDate, tdate) >= 0) &&
                                                  (i.SRCashier == a.EmployeeId)
                                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                                  group i by i.Customer into g
                                                  select new
                                                  {
                                                      Total = g.Sum(x => x.SRGrandTotal)
                                                  }).Sum(z => (decimal?)z.Total) ?? 0,   // Calc fix (N4): sum across ALL the employee's customers (was FirstOrDefault — kept only the first)

                         NoOfVchSale = (int?)(from i in db.SalesEntrys
                                              where (fromdate == "" || EF.Functions.DateDiffDay(i.SEDate, fdate) <= 0) && i.Status == 1 &&
                                              (todate == "" || EF.Functions.DateDiffDay(i.SEDate, tdate) >= 0) &&
                                             (i.SECashier == a.EmployeeId)
                                             && (Salety == "" || Salety == null || i.SaleType == St)
                                             && ((!MCList.Any() && ddmc == null) || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                              select new
                                              {
                                                  saleid = i.SalesEntryId
                                              }).Count() ?? 0,
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
        [QkAuthorize(Roles = "Dev,Sales Executive Wise")]
        public ActionResult GetSalessummary(long? salesexec, string fromdate, string todate, string Salety, long? ddmc)
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
            SaleType St = new SaleType();
            if (Salety == "1" || Salety == "2")
            {
                St = (Salety == "1") ? SaleType.Sale : SaleType.Hire;
            }
            var v = (from a in db.Employees
                   
                     select new salessummary
                     {
                         
                         empname = a.FirstName + " " + a.MiddleName + " " + a.LastName,

                         salesamount = (decimal?)(from i in db.SalesEntrys
                                              where (fromdate == "" || EF.Functions.DateDiffDay(i.SEDate, fdate) <= 0) && i.Status == 1 &&
                                              (todate == "" || EF.Functions.DateDiffDay(i.SEDate, tdate) >= 0) &&
                                              (i.SECashier == a.EmployeeId) && (Salety == "" || Salety == null || i.SaleType == St)
                                              && ((!MCList.Any() && ddmc == null) || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                              // group i by i.Customer into g
                                              select new
                                              {
                                                  Total = i.SEGrandTotal
                                                  //  Total = g.Sum(x => x.SESubTotal - x.SEDiscount)
                                              }).Sum(x => x.Total) ?? 0,
                         }).OrderByDescending(o=>o.salesamount).ToList();

            var v2 = (from a in db.Employees
                  

                     select new salessummary
                     {
                         
                         empname = a.FirstName + " " + a.MiddleName + " " + a.LastName,

                         salesamount = (decimal?)(from i in db.SalesEntrys
                                                  join j in db.Customers on i.Customer equals j.CustomerID
                                              where (fromdate == "" || EF.Functions.DateDiffDay(i.SEDate, fdate) <= 0) && i.Status == 1 &&
                                              (todate == "" || EF.Functions.DateDiffDay(i.SEDate, tdate) >= 0) &&
                                              (j.SalesPerson == a.EmployeeId) && (Salety == "" || Salety == null || i.SaleType == St)
                                              select new
                                              {
                                                  Total = i.SEGrandTotal
                                                  //  Total = g.Sum(x => x.SESubTotal - x.SEDiscount)
                                              }).Sum(x => x.Total) ?? 0,
                     }).OrderByDescending(a => a.salesamount).ToList();

            var v3 = (from a in db.MCs
                     
                     join c in db.SalesEntrys on a.MCId equals c.MC
                    
                     select new salessummary
                     {

                         empname=a.MCName,
                         salesamount = (decimal?)(from i in db.SalesEntrys
                                              where (fromdate == "" || EF.Functions.DateDiffDay(i.SEDate, fdate) <= 0) && i.Status == 1 &&
                                              (todate == "" || EF.Functions.DateDiffDay(i.SEDate, tdate) >= 0) 
                                              && MCArray.Contains(a.MCId) 
                                              // group i by i.Customer into g
                                              select new
                                              {
                                                  Total = i.SEGrandTotal
                                                  //  Total = g.Sum(x => x.SESubTotal - x.SEDiscount)
                                              }).Sum(x => x.Total) ?? 0,
                     }).Where(o=>o.salesamount>0).OrderByDescending(a => a.salesamount).ToList();
            var cid = db.ItemCategorys.Select(o => o.ItemCategoryID).FirstOrDefault();
            var salesmanwise = (from x in db.ItemCategorys
                                where x.ItemCategoryID == cid
                                select new salessummary
                                {
                                    empname = "<strong>Sales Man Wise</strong>",
                                    salesamount = 0
                                }
                    ).ToList();
            var salesexecutive = (from x in db.ItemCategorys
                                  where x.ItemCategoryID == cid
                                  select new salessummary
                                  {
                                      empname = "<strong>Sales Executive Wise</strong>",
                                      salesamount = 0
                                  }
                  ).ToList();
            var MaterialCenter = (from x in db.ItemCategorys
                                  where x.ItemCategoryID == cid
                                  select new salessummary
                                  {
                                      empname = "<strong>Materila Center Wise</strong>",
                                      salesamount = 0
                                  }
               ).ToList();
            List<salessummary> data2=new List<salessummary>();
            data2.AddRange(salesmanwise);
            data2.AddRange(v2);
            data2.AddRange(salesexecutive);
            data2.AddRange(v);
            data2.AddRange(MaterialCenter);
            data2.AddRange(v3);



            var data = data2.ToList(); ;
            recordsTotal = salesmanwise.Count();

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
        public class salessummary
        {
            public string empname;
            public decimal? salesamount;

        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Sales Executive Wise")]
        public ActionResult ViewSalesExecutiveWise(long? salesexec, string from, string to, long? ddmc)
        {
            if (salesexec != 0 && salesexec != null)
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
        //DailySummary region
        [QkAuthorize(Roles = "Dev,Sales DailySummary")]
        public ActionResult DailySummaryold()
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
        //DailySummary region


        [QkAuthorize(Roles = "Dev,Sales DailySummary")]
        public ActionResult DailySummaryoldautosubmit()
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
        //DailySummary region
        [QkAuthorize(Roles = "Dev,Sales DailySummary")]
        public ActionResult DailySummary()
        {
            bool isquicknet = db.companys.Any(o => o.CPName.Contains("QUICK NET COMPUTERS"));
            if (!isquicknet)
            {
                return RedirectToAction("DailySummaryold", "SalesReport");
            }
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
        [QkAuthorize(Roles = "Dev,Sales DailySummary")]
        public ActionResult DailySummaryold(string Cashier, string From, long? ddlMC)
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
           
            if (!MCList.Any() && (ddlMC == 0 || ddlMC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            DailySummary vmodel = new DailySummary();
          
            String format = "dd-MM-yyyy";
            DateTime? fdate = null;
            if (From != "")
            {
                fdate = DateTime.ParseExact(From, format, new CultureInfo("en-GB"));
            }
            else
            {
                string today = Convert.ToString(System.DateTime.Now);
                fdate = DateTime.ParseExact(today, format, new CultureInfo("en-GB"));
            }
            DateTime fdate2 = fdate.Value.AddHours(27);
            fdate = fdate.Value.AddHours(6);

            vmodel.startsection = (from a in db.Payments
                                   join b in db.AccountsTransactions on a.PaymentId equals b.reference
                                   where b.Purpose == "Payment" && b.Account == 3
                                   && a.Remark == "End Session"
                                  && b.CreatedDate >= fdate && b.CreatedDate <= fdate2
                                  && (Cashier == "All" || a.CreatedBy == Cashier)
                                   select new
                                   {
                                       b.Debit,
                                       b.Credit
                                   }
                                ).ToList().Select( o=>o.Credit).Sum();
            vmodel.endsection = (from a in db.Receipts
                                 join b in db.AccountsTransactions on a.ReceiptId equals b.reference
                                 where b.Purpose== "Receipt"  && b.Account==3

                                && b.CreatedDate >= fdate && b.CreatedDate <= fdate2 
                                && (Cashier == "All" || a.CreatedBy == Cashier)
                                select new
                                {
                                    b.Debit,
                                    b.Credit
                                }
                                ).ToList().Select(o => o.Debit).Sum();
          // Item wise
            var tomorrow =fdate.Value.AddDays(1);
            DateTime srdate = fdate.Value.Date;
            vmodel.cn = db.cashnotes.Where(o => o.trasdate >= fdate && o.trasdate < fdate2 && o.purpuse== "Receipt" && (Cashier == "All" || o.CreatedBy == Cashier)).OrderBy(o=>o.cashid).Take(1).FirstOrDefault();
            vmodel.cno = db.cashnotes.Where(o => o.trasdate >= fdate && o.trasdate < fdate2 && o.purpuse == "Payment" && (Cashier == "All" || o.CreatedBy == Cashier)).OrderByDescending(o => o.cashid).Take(1).FirstOrDefault();
            var sr = db.SalesReturns.Where(o => o.SRDate ==srdate).Select(o => o.SRGrandTotal).AsEnumerable().DefaultIfEmpty(0).Sum();
            vmodel.salesreturn = sr;
            cashnotes cv = new cashnotes();
            if (vmodel.cn==null)
            {
                
                vmodel.cn = cv;
            }
                   
           
                var data = (from a in db.SalesEntrys
                        join b in db.Customers on a.Customer equals b.CustomerID into cust
                        from b in cust.DefaultIfEmpty()
                        join e in db.PaymentMethods on a.PaymentMethod equals e.PaymentMethodId into paymeth
                        from e in paymeth.DefaultIfEmpty()
                        join f in db.MCs on a.MaterialCenter equals f.MCId into mcs
                        from f in mcs.DefaultIfEmpty()
                        join g in db.Users on a.CreatedBy equals g.Id
                        where (Cashier == "All" || a.CreatedBy == Cashier) &&
                        a.SECreatedDate >= fdate && a.SECreatedDate <= fdate2

                        && a.Status == 1
                          select new
                        {
                            BillNo = a.BillNo,
                            Date = a.SEDate,
                            a.CreatedBy,
                            CreatedDate = a.SECreatedDate,
                            a.SENo,
                            Tax = a.SETax,
                            TaxAmount = a.SETaxAmount,
                            STotal = a.SESubTotal,
                            Note = a.SENote,
                            GTotal = a.SEGrandTotal,
                            Discount = a.SEDiscount,
                            MCName = f.MCName,
                            a.PayType,
                            a.SaleType,
                            a.Status,
                            a.SalesEntryId,
                            a.Customer,
                            a.CustomerType,
                            MethodName = (a.CustomerType == CustomerType.Card ? e.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit"))
                        }).ToList();
            if (data.Any())
            {
                var first = data.OrderBy(a => a.CreatedDate).ThenBy(a => a.SalesEntryId).Select(a => new { a.CreatedDate, a.BillNo, a.SENo }).FirstOrDefault();
                vmodel.first = first.CreatedDate;
                vmodel.firstInvoice = first.BillNo;
                var last = data.OrderByDescending(a => a.CreatedDate).ThenBy(a => a.SalesEntryId).Select(a => new { a.CreatedDate, a.BillNo, a.SENo }).FirstOrDefault();
               if(last==null)
                {
                    Danger("no End Session");
                    return RedirectToAction("Create", "POSRES");
                }
                
                vmodel.last = last.CreatedDate;
                vmodel.lastInvoice = last.BillNo;
                vmodel.Average = data.OrderBy(x => Guid.NewGuid()).Select(a => a.GTotal).FirstOrDefault();
                vmodel.customers = data.Select(a => a.Customer).Count();
                vmodel.Sales = data.Select(a => a.SENo).Count();
                vmodel.GTotal = data.Sum(a => a.GTotal);
                vmodel.Tax = data.Sum(a => a.TaxAmount);
                vmodel.Disc = data.Sum(a => a.Discount);
                vmodel.STotal = data.Sum(a => a.STotal);
                vmodel.MCName = data.Select(a => a.MCName).FirstOrDefault();

                var cs = (from a in db.SalesEntrys
                                 where (Cashier == "All" || a.CreatedBy == Cashier)
                                  && a.SECreatedDate >= fdate && a.SECreatedDate <= fdate2
                                  && a.Status == 1
                                  
                                  group new {a.CustomerType,a.SEGrandTotal} by new { a.CustomerType, a.SEDate }
                                 into  gpr
                          select new ItemSum
                          {
                              name=gpr.Select(o=>o.CustomerType).FirstOrDefault().ToString(),
                              Amount=gpr.Sum(o=>o.SEGrandTotal),
                          }).OrderBy(a => a.name).ToList();
                vmodel.Payment = cs;
               vmodel.pysicalcash= db.AccountsTransactions.Where(o => (o.Account == 504 || o.Account ==3) &&  o.CreatedDate<=fdate2).Select(o => o.Debit - o.Credit).Sum(); // Calc fix: parenthesized account test — && bound tighter than ||, so Account==504 rows had ignored the date bound (pulled future-dated cash in).
                // Item wise
                var itemWise = (from a in db.Items
                                join b in db.SEItemss on a.ItemID equals b.Item
                                join c in db.SalesEntrys on b.SalesEntry equals c.SalesEntryId
                                join e in db.ItemUnits on a.ItemUnitID equals e.ItemUnitID into primary
                                from e in primary.DefaultIfEmpty()
                                join f in db.ItemUnits on a.SubUnitId equals f.ItemUnitID into second
                                from f in second.DefaultIfEmpty()
                                where (Cashier == "All" || c.CreatedBy == Cashier)


                               &&  c.SECreatedDate >= fdate && c.SECreatedDate <= fdate2
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
                                    PriSaleQty = (int?)(from i in db.SEItemss
                                                        join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                        where (Cashier == "All" || j.CreatedBy == Cashier) && j.SEDate == fdate
                                                         && (i.Item == a.ItemID)
                                                                 group i by i.ItemId into g
                                                        select new
                                                        {
                                                            Total = g.Sum(x => x.ItemQuantity)
                                                        }).FirstOrDefault().Total ?? 0,
                                    SubSaleQty = (int?)(from i in db.SEItemss
                                                        join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                        where (Cashier == "All" || j.CreatedBy == Cashier) && j.SEDate == fdate &&
                                                        (i.Item == a.ItemID && i.ItemUnit == a.SubUnitId)
                                                         group i by i.ItemId into g
                                                        select new
                                                        {
                                                            Total = g.Sum(x => x.ItemQuantity)
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
                                    o.PriSaleQty,

                                    o.ItemName
                                }).OrderBy(a => a.ItemName).ToList();

                if (itemWise.Any())
                {
                    vmodel.item = itemWise.Select(b => new ItemSum
                    {
                        name = b.ItemName,
                        quantity = b.PriSaleQty
                    }).ToList();
                }
                // Category Wise
                var CategoryWise = (from i in db.SEItemss
                                    join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                    join k in db.Items on i.Item equals k.ItemID into itm
                                    from k in itm.DefaultIfEmpty()
                                    join l in db.ItemCategorys on k.ItemCategoryID equals l.ItemCategoryID
                                    where (Cashier == "All" || j.CreatedBy == Cashier)
                                    && j.SECreatedDate >= fdate &&j.SECreatedDate <= fdate2
                                    group new { i.ItemQuantity, i.ItemSubTotal, l.ItemCategoryName } by k.ItemCategoryID into grp
                                    select new
                                    {

                                        qty = grp.Sum(o => o.ItemQuantity),
                                        amount = grp.Sum(o => o.ItemSubTotal),
                                        categoryname = grp.Select(o => o.ItemCategoryName).FirstOrDefault(),


                                    }).ToList();

                if (CategoryWise.Any())
                {
                    vmodel.category = CategoryWise.Select(b => new ItemSum
                  {
                        name = b.categoryname,
                        quantity = b.qty,
                        Amount= b.amount,
                    }).ToList();
                }
                vmodel.Date = data.Select(a => a.Date).FirstOrDefault();
            }
            companySet();
            vmodel.time = System.DateTime.Now;
            vmodel.by = User.Identity.Name;
            if (vmodel.cno ==null)
            {
                vmodel.cno =cv;
            }
            return View(vmodel);

        }


        [HttpPost]
        [QkAuthorize(Roles = "Dev,Sales DailySummary")]
        public ActionResult DailySummary(string Cashier, string From, long? ddlMC,CustomerType? payment)
        {
           
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddlMC == 0 || ddlMC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            DailySummary vmodel = new DailySummary();
            String format = "dd-MM-yyyy";
            DateTime fdate;
            if (From != "")
            {
                fdate = DateTime.ParseExact(From, format, new CultureInfo("en-GB"));
            }
            else
            {
                string today = Convert.ToString(System.DateTime.Now);
                fdate = DateTime.ParseExact(today, format, new CultureInfo("en-GB"));
            }
            long empid = db.Employees.Where(o => o.UserId == Cashier).Select(o => o.EmployeeId).FirstOrDefault();
            var acmaps = db.accountmaps.Where(o => o.EmployeeId == empid).Select(o => o.AccountId).ToArray();
            DateTime opdate = fdate.AddDays(-1);
            var account = (from a in db.Accountss
                           where acmaps.Contains(a.AccountsID)
                           select new
                           {
                               a.AccountsID,
                               a.Name,
                               OpCredit = (db.AccountsTransactions.Where(d => d.Account == a.AccountsID && d.Status == null && d.Date <= opdate).Sum(d => (decimal?)d.Credit) ?? 0),
                               OpDebit = (db.AccountsTransactions.Where(b => b.Account == a.AccountsID && b.Status == null && b.Date <= opdate).Sum(b => (decimal?)b.Debit) ?? 0),
                               clCredit = (db.AccountsTransactions.Where(d => d.Account == a.AccountsID && d.Status == null && d.Date <= fdate).Sum(d => (decimal?)d.Credit) ?? 0),
                               clDebit = (db.AccountsTransactions.Where(b => b.Account == a.AccountsID && b.Status == null && b.Date <= fdate).Sum(b => (decimal?)b.Debit) ?? 0),

                           }).Select(o => new accbalance
                           {

                               acname = o.Name,
                               opbalance = o.OpDebit - o.OpCredit,
                               clbalance = o.clDebit - o.clCredit
                           }).ToList();
            ViewBag.accountinfo = account;
            // Basic Details
            var data = (from a in db.SalesEntrys
                        join b in db.Customers on a.Customer equals b.CustomerID into cust
                        from b in cust.DefaultIfEmpty()
                        join e in db.PaymentMethods on a.PaymentMethod equals e.PaymentMethodId into paymeth
                        from e in paymeth.DefaultIfEmpty()
                        join f in db.MCs on a.MaterialCenter equals f.MCId into mcs
                        from f in mcs.DefaultIfEmpty()
                        join g in db.Users on a.CreatedBy equals g.Id
                        where (Cashier == "All" || a.CreatedBy == Cashier) && a.SEDate == fdate && a.Status == 1
                        && (payment==null||a.CustomerType ==payment)
                        && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(a.MaterialCenter) || ddlMC == a.MaterialCenter)
                        select new
                        {
                            BillNo = a.BillNo,
                            Date = a.SEDate,
                            a.CreatedBy,
                            CreatedDate = a.SECreatedDate,
                            a.SENo,
                            Tax = a.SETax,
                            TaxAmount = a.SETaxAmount,
                            STotal = a.SESubTotal,
                            Note = a.SENote,
                            GTotal = a.SEGrandTotal,
                            Discount = a.SEDiscount,
                            MCName = f.MCName,
                            a.PayType,
                            a.SaleType,
                            a.Status,
                            a.SalesEntryId,
                            a.Customer,
                            a.CustomerType,
                            MethodName = (a.CustomerType == CustomerType.Card) ? "Card" : (a.CustomerType == CustomerType.Walking) ? "Cash" : (a.CustomerType == CustomerType.Online)?"Online":"Credit",
                        }).ToList();
            if (data.Any())
            {
                var first = data.OrderBy(a => a.CreatedDate).ThenBy(a => a.SalesEntryId).Select(a => new { a.CreatedDate, a.BillNo, a.SENo }).FirstOrDefault();
                vmodel.first = first.CreatedDate;
                vmodel.firstInvoice = first.BillNo;
                var last = data.OrderByDescending(a => a.CreatedDate).ThenBy(a => a.SalesEntryId).Select(a => new { a.CreatedDate, a.BillNo, a.SENo }).FirstOrDefault();
                vmodel.last = last.CreatedDate;
                vmodel.lastInvoice = last.BillNo;
                vmodel.Average = data.OrderBy(x => Guid.NewGuid()).Select(a => a.GTotal).FirstOrDefault();
                vmodel.customers = data.Select(a => a.Customer).Distinct().Count();
                vmodel.Sales = data.Select(a => a.SENo).Count();
                vmodel.GTotal = data.Sum(a => a.GTotal);
                vmodel.Tax = data.Sum(a => a.TaxAmount);
                vmodel.Disc = data.Sum(a => a.Discount);
                vmodel.STotal = data.Sum(a => a.STotal);
                vmodel.MCName = data.Select(a => a.MCName).FirstOrDefault();

                vmodel.Payment = data
                 .Select(b => new
                 {
                     b.CustomerType,
                     b.MethodName,
                 }).Distinct().Select(o => new ItemSum
                 {
                     name = (o.CustomerType == CustomerType.Walking ? o.MethodName : "Credit"),
                     quantity = data.Where(a => a.CustomerType == o.CustomerType && a.MethodName == o.MethodName).Sum(a => a.GTotal),

                 }).OrderBy(a => a.name).ToList();

                // Item wise
                var itemWise = (from a in db.Items
                                join b in db.SEItemss on a.ItemID equals b.Item
                                join c in db.SalesEntrys on b.SalesEntry equals c.SalesEntryId
                                join e in db.ItemUnits on a.ItemUnitID equals e.ItemUnitID into primary
                                from e in primary.DefaultIfEmpty()
                                join f in db.ItemUnits on a.SubUnitId equals f.ItemUnitID into second
                                from f in second.DefaultIfEmpty()
                                where (Cashier == "All" || c.CreatedBy == Cashier) && c.SEDate == fdate
                                && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(c.MaterialCenter) || ddlMC == c.MaterialCenter)
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
                                    PriSaleQty = (int?)(from i in db.SEItemss
                                                        join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                        where (Cashier == "All" || j.CreatedBy == Cashier) && j.SEDate == fdate
                                                         && (i.Item == a.ItemID)
                                                         && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)
                                                        group i by i.ItemId into g
                                                        select new
                                                        {
                                                            Total = g.Sum(x => x.ItemQuantity)
                                                        }).FirstOrDefault().Total ?? 0,
                                    SubSaleQty = (int?)(from i in db.SEItemss
                                                        join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                        where (Cashier == "All" || j.CreatedBy == Cashier) && j.SEDate == fdate &&
                                                        (i.Item == a.ItemID && i.ItemUnit == a.SubUnitId)
                                                        && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)
                                                        group i by i.ItemId into g
                                                        select new
                                                        {
                                                            Total = g.Sum(x => x.ItemQuantity)
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
                                    o.PriSaleQty,

                                    o.ItemName
                                }).OrderBy(a => a.ItemName).ToList();

                if (itemWise.Any())
                {
                    vmodel.item = itemWise.Select(b => new ItemSum
                    {
                        name = b.ItemName,
                        quantity = b.PriSaleQty
                    }).ToList();
                }
                // Category Wise
                var CategoryWise = (from a in db.ItemCategorys
                                    select new
                                    {
                                        Category = a.ItemCategoryName,
                                        TotQty = (int?)(from i in db.SEItemss
                                                        join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                        join k in db.Items on i.Item equals k.ItemID into itm
                                                        from k in itm.DefaultIfEmpty()
                                                        where (Cashier == "All" || j.CreatedBy == Cashier) && j.SEDate == fdate &&
                                                        (k.ItemCategoryID == a.ItemCategoryID)
                                                        && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)
                                                        select new
                                                        {
                                                            qty = i.ItemQuantity
                                                        }).Sum(x => x.qty) ?? 0,
                                    }).Distinct().AsEnumerable().Select(o => new
                                    {
                                        o.Category,
                                        o.TotQty,
                                    }).OrderBy(x => x.Category).ToList();

                if (CategoryWise.Any())
                {
                    vmodel.category = CategoryWise.Select(b => new ItemSum
                    {
                        name = b.Category,
                        quantity = b.TotQty
                    }).ToList();
                }
                vmodel.Date = data.Select(a => a.Date).FirstOrDefault();
            }
            companySet();
            vmodel.time = System.DateTime.Now;
            vmodel.by = User.Identity.Name;
            return View(vmodel);

        }

        public ActionResult SalesCommission()
        {
            List<SelectFormat> empall = new List<SelectFormat>
            {
                new SelectFormat{id=0,text="All"}
            };

            var emp = (from a in db.Employees
                       select new SelectFormat
                       {
                           id = a.EmployeeId,
                           text = a.FirstName
                       }).ToList().Union(empall).ToList().OrderBy(o => o.text);
            ViewBag.emp = QkSelect.List(emp, "id", "text", 0);





            return View();
        }
        [HttpPost]
        public ActionResult SalesCommission(long ddlemp, string From, string To)
        {
            return RedirectToAction("ViewSalesCommission", new { ddlemp = ddlemp, from = From, to = To });
        }
        public ActionResult ViewSalesCommission(long ddlemp, string From, string To)
        {
            String seltext;
            if (ddlemp == 0)
                seltext = "All Employees";
            else
            {
                seltext = db.Employees.Where(o => o.EmployeeId == ddlemp).Select(o => o.FirstName).FirstOrDefault();

            }
            ViewBag.seltext = seltext;

            return View();
        }

        [QkAuthorize(Roles = "Dev,CommissionReport")]
        public ActionResult Commission()
        {
            ViewBag.Brand = QkSelect.List(
             new List<SelectListItem>
             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
             }, "Value", "Text", 1);
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

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
        [QkAuthorize(Roles = "Dev,CommissionReport")]
        public ActionResult Commission(long? ddlItemBrand, string From, string To, string SaleType, long? ddlMC)
        {
            return RedirectToAction("ViewCommission", new { brand = ddlItemBrand, from = From, to = To, saletype = SaleType, ddmc = ddlMC });
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,CommissionReport")]
        public ActionResult GetCommission(long? brand, string fromdate, string todate, string Salety, long? ddmc)
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
            SaleType St = new SaleType();
            if (Salety != "")
            {
                St = (Salety == "1") ? SaleType.Sale : SaleType.Hire;
            };
            var v = (from a in db.SEItemss
                     join b in db.SalesEntrys on a.SalesEntry equals b.SalesEntryId
                     join c in db.Items on a.Item equals c.ItemID into item
                     from c in item.DefaultIfEmpty()
                     join d in db.ItemBrands on c.ItemBrandID equals d.ItemBrandID into brnd
                     from d in brnd.DefaultIfEmpty()
                     where (brand == 0 || c.ItemBrandID == brand) && (Salety == "" || Salety == null || b.SaleType == St)
                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(b.MaterialCenter) || ddmc == b.MaterialCenter)
                     select new
                     {
                         InvoiceNo = b.BillNo,
                         SEDate = b.SEDate,
                         ItemName = c.ItemName,
                         PartNumber = (c.PartNumber != null && c.PartNumber != "") ? c.PartNumber : "",
                         Quantity = a.ItemQuantity,
                         SubTotal = a.ItemSubTotal,
                         Commission = c.Commission,
                         CommissionAmount = (a.ItemSubTotal * (c.Commission / 100)),
                         Difference = a.ItemSubTotal - (a.ItemSubTotal * (c.Commission / 100)),
                     }).OrderByDescending(x => x.SEDate);

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

        public ActionResult GetSalesCommission(long ddlemp, string fromdate, string todate)
        {
            var UserId = User.Identity.GetUserId();

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

            var v = (from b in db.SalesEntrys
                     join c in db.commissions on b.SalesEntryId equals c.salesid
                     join d in db.Employees on c.agent equals d.EmployeeId

                     //let seitemtotal = (decimal?)(from f in db.SEItemss

                     //                   where (f.SalesEntry == b.SalesEntryId) && (f.Type == true) && (f.itemNote != "-:{Bundle_Item}")
                     //                       total = (f.ItemQuantity * f.ItemUnitPrice)
                     //                   }).ToList().Sum(o => o.total) ?? 0

                     let seitemtotal = (decimal?)(from f in db.SEItemss
                                                  join seen in db.SalesEntrys on f.SalesEntry equals seen.SalesEntryId
                                                  join seit in db.Items on f.Item equals seit.ItemID
                                                  where (f.SalesEntry == b.SalesEntryId)
                                                  select new
                                                  {
                                                      total = (f.ItemUnit == seit.ItemUnitID) ? (seit.PurchasePrice * f.ItemQuantity) : ((seit.PurchasePrice * f.ItemQuantity) / seit.ConFactor)
                                                  }).Sum(o => o.total) ?? 0

                     where (ddlemp == 0 || d.EmployeeId == ddlemp) && (fromdate == "" || EF.Functions.DateDiffDay(b.SEDate, fdate) <= 0) &&
                      (todate == "" || EF.Functions.DateDiffDay(b.SEDate, tdate) >= 0)
                     select new
                     {
                         InvoiceNo = b.BillNo,
                         SEDate = b.SEDate,
                         SalesId = b.SalesEntryId,
                         employeename = d.FirstName,
                         b.SEGrandTotal,
                         b.SETaxAmount,
                         seitemtotal,
                         CommissionVal = c.comvalue,
                         //commision = (seitemtotal != 0) ? ((c.commisiontype == 1) ? ((((c.commisionmode == 1) ? b.SEGrandTotal : b.SESubTotal) - seitemtotal) * c.comvalue / 100) : c.comvalue) : 0,
                         commision = (seitemtotal != 0) ? ((c.commisiontype == 1) ? ((((c.commisionmode == 1) ? b.SESubTotal : b.SEGrandTotal) - seitemtotal) * c.comvalue / 100) : c.comvalue) : 0,
                     }).OrderByDescending(x => x.SEDate);

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
        [QkAuthorize(Roles = "Dev,CommissionReport")]
        public ActionResult ViewCommission(long? brand, string from, string to, string saletype, long? ddmc)
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
            ViewBag.SaleTy = saletype;
            return View();
        }


        //#region itemDetails
        public ActionResult itemDetails(long? iditem, long? customer, string from, string to, long? ddmc)
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
            ViewBag.customer = customer;
            companySet();
            return View();
        }
        public ActionResult getitemDetails(long? item, long? customer, string fromdate, string to, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Select(a => (long?)a.MCId).ToList();
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

            var v = (from a in db.SEItemss
                     join b in db.Items on a.Item equals b.ItemID
                     join g in db.SalesEntrys on a.SalesEntry equals g.SalesEntryId
                     join cu in db.Customers on g.Customer equals cu.CustomerID
                     join e in db.ItemUnits on b.ItemUnitID equals e.ItemUnitID into primary
                     from e in primary.DefaultIfEmpty()
                     join f in db.ItemUnits on b.SubUnitId equals f.ItemUnitID into second
                     from f in second.DefaultIfEmpty()
                     where (item == 0 || a.Item == item) && (customer == 0 || g.Customer == customer)
                     && (fromdate == "" || EF.Functions.DateDiffDay(g.SEDate, fdate) <= 0)
                     && (to == "" || EF.Functions.DateDiffDay(g.SEDate, tdate) >= 0)
                     && (ddmc == 0 || ddmc == g.MaterialCenter)

                     select new
                     {
                         b.ItemID,
                         b.ItemName,
                         b.ItemCode,
                         Item = b.ItemCode + "-" + b.ItemName,
                         Unit = (a.ItemUnit == b.ItemUnitID) ? e.ItemUnitName : f.ItemUnitName,
                         g.BillNo,
                         g.SalesEntryId,
                         cu.CustomerName,
                         a.ItemQuantity,
                         a.ItemSubTotal,
                         a.ItemTaxAmount,
                         a.ItemTotalAmount,
                         a.ItemDiscount,
                         PartNumber = (b.PartNumber != null && b.PartNumber != "") ? b.PartNumber : "",
                         a.itemNote,
                         a.ItemUnitPrice,
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


        //#region custDetails
        public ActionResult custDetails(long? idcust, string from, string to, long? ddmc)
        {
            if (idcust != 0)
            {
                ViewBag.cust = (from a in db.Customers
                                where a.CustomerID == idcust
                                select new
                                {
                                    Name = a.CustomerName
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
        public ActionResult getcustDetails(long? cust, string fromdate, string to, long? ddmc)
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

            var v = (from a in db.SalesEntrys
                     join b in db.Customers on a.Customer equals b.CustomerID
                     where (cust == 0 || a.Customer == cust) && a.Status == 1
                     && (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0)
                     && (to == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0)
                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                     select new
                     {
                         b.CustomerName,
                         a.BillNo,
                         a.SEDiscount,
                         a.SEGrandTotal,
                         a.SESubTotal,
                         a.SETaxAmount,
                         a.SENote,
                         a.SEDate,
                         a.SalesEntryId,
                         fromdate,
                         to
                     }).AsEnumerable().OrderBy(a => a.SalesEntryId);

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
        public class custmodal
        {
            public long id { get; set; }
            public string CustomerCode { get; set; }
            public long accounts { get; set; }
            public string CustomerName { get; set; }
            public string TaxRegNo { get; set; }
            public string Location { get; set; }
            public string Address { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public decimal CreditLimit { get; set; }
            public decimal CreditPeriod { get; set; }
            public string OpnBalance { get; set; }
            public decimal Credit { get; set; }
            public decimal Debit { get; set; }
            public bool Dev { get; set; }
            public bool Details { get; set; }
            public bool Edit { get; set; }
            public bool Delete { get; set; }
            public string Alias { get; set; }
            public DateTime? ldate { get; set; }
            public List<MobileViewModel> mobmodel { get; set; } //getmobilesno(id){get;set;}
            public string currentbalance { get; set; }
            public decimal? decimalcurrbalance { get; set; }
            public decimal? totalSales { get; set; }
            public decimal? nofsales { get; set; }
            public decimal? noofqutation { get; set; }
            public long? salesman { get; set; }
            public long? source { get; set; }
            public string sourceoflead { get; set; }
            public long? customertype { get; set; }

            public string customertypestr { get; set; }
        }

        [QkAuthorize(Roles = "Dev,Customer")]
        public ActionResult CustomerSummary()
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
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

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
        [QkAuthorize(Roles = "Dev,Customer")]
        public ActionResult GetCustomer(string FromDate, string ToDate)
        {
           
          
            int recordsTotal = 0;
            DateTime? fdate = null;
            DateTime? tdate = null;
            if (FromDate != "")
            {
                fdate = DateTime.Parse(FromDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (ToDate != "")
            {
                tdate = DateTime.Parse(ToDate, new CultureInfo("en-GB").DateTimeFormat);
            }
           

            var userpermission = User.IsInRole("All Customers");
            var UserId = User.Identity.GetUserId();

            var uDev = User.IsInRole("Dev");
            var uCustView = User.IsInRole("View Customer");
            var uEdit = User.IsInRole("Edit Customer");
            var uDelete = User.IsInRole("Delete Customer");

            
            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
            var cus = (from z in db.SalesEntrys
                       where
                       (FromDate == "" || EF.Functions.DateDiffDay(z.SEDate, fdate) <= 0) &&
                       (ToDate == "" || EF.Functions.DateDiffDay(z.SEDate, tdate) >= 0)
                       select new
                       {
                           z.Customer
                       }).Select(o => o.Customer).Distinct().ToList().ToArray();
                      
                      
             var v = (from a in db.Customers
                     join x in db.Accountss on a.Accounts equals x.AccountsID
                     join y in db.Employees on a.SalesPerson equals y.EmployeeId into def
                     from y in def.DefaultIfEmpty()
                     


                     where

                     a.Type == CRMCustomerType.Customer &&
                     cus.Contains(a.CustomerID)
                     select new
                     {
                         id = a.CustomerID,
                         a.CustomerCode,
                         a.CustomerName,
                         TaxRegNo = x.TRN,
                         a.Location,
                         Address = a.Addres,
                         Phone = a.Addres,
                         source = a.SourceOfLead,
                         salesman = y.FirstName + " " + y.MiddleName + " " + y.LastName,
                         //Mobile = b.Mobile,
                         Email = a.Addres,
                         CreditLimit = a.CreditLimit,
                         CreditPeriod = a.CreditPeriod,
                         OpnBalance = (x.OpnBalanceCr > 0) ? (x.OpnBalanceCr != 0 ? x.OpnBalanceCr + " Cr." : "0.00") : (x.OpnBalance != 0 ? x.OpnBalance + " Dr." : "0.00"),
                         a.Accounts,
                         Credit = 0,// (db.AccountsTransactions.Where(d => d.Account == a.Accounts && d.Status == null).Sum(d => (decimal?)d.Credit) ?? 0),
                         Debit = 0,// (db.AccountsTransactions.Where(b => b.Account == a.Accounts && b.Status == null).Sum(b => (decimal?)b.Debit) ?? 0),
                         Dev = uDev,
                         Details = uCustView,
                         Edit = uEdit,
                         Delete = uDelete,
                         Alias = x.Alias,
                         ldate = ((a != null) && (a.CreatedDate > a.logtime)) ? a.CreatedDate : a.logtime,
                         //mobmodel = (from ac in db.Mobiles
                         //            where (ac.Contact == a.Contact)
                         //                /* Num = (ac.Name == "" || ac.Name == null) ? ac.MobileNum : ac.MobileNum */
                         //                Num = (ac.Name == null || ac.Name == "") ? ac.MobileNum : ac.MobileNum + b.Mobile + "-" + ac.Name,
                         //                Name = ac.Name,
                         //                emails = b.EmailId,

                     }).Select(o => new custmodal
                     {
                         id = o.id,
                         CustomerCode = o.CustomerCode,
                         CustomerName = o.CustomerName,
                         TaxRegNo = o.TaxRegNo,
                         Location = o.Location,
                         Address = o.Address,
                         Phone = o.Phone,
                         Email = o.salesman,
                         CreditLimit = o.CreditLimit,
                         CreditPeriod = o.CreditPeriod,
                         OpnBalance = o.OpnBalance,
                         Credit = o.Credit,
                         Debit = o.Debit,
                         Dev = o.Dev,
                         Details = o.Details,
                         Edit = o.Edit,
                         Delete = o.Delete,
                         Alias = o.Alias,
                         ldate = o.ldate,
                         accounts = o.Accounts,
                         totalSales = 0,
                         nofsales =0,
                         noofqutation=0
                     }); 
            //search
            

            //SORT
           
            recordsTotal = v.Count();
            var data = v.ToList();//.GroupBy(x => x.CustomerName, (key, g) => g.OrderByDescending(m => m.id).FirstOrDefault()).ToList(); 
            long accts = 0;
            if (1==2)
            {

               
            }
            else
            {

                var contactslist = (from a in db.Customers
                                    join b in db.ContactRelation on a.CustomerID equals b.RelationID
                                    join c in db.Contacts on b.ContactID equals c.ContactID
                                    where (b.RelationType == (long)CRMCustomerType.Customer)

                                    select new MobileViewModel
                                    {
                                        emails = c.EmailId,
                                        Name = c.FirstName + " " + c.LastName,
                                        Num = c.Mobile,
                                        ID = a.CustomerID
                                    }).ToList();

                var distinctcustomer = db.SalesEntrys.Select(o => o.Customer).Distinct().ToArray();
                var distinctcustomertask = db.ProTasks.Select(o => (long)o.CustomerID).Distinct().ToArray();
                var distinctcustomerqut = db.Quotations.Select(o => o.Customer).Distinct().ToArray();
                var allunition = distinctcustomer.Union(distinctcustomertask);
                allunition = allunition.Union(distinctcustomerqut);
                data = (from o in data
                        join p in db.Employees on o.source equals p.EmployeeId into emps
                        from p in emps.DefaultIfEmpty()
                        join q in db.CustomerTyps on o.customertype equals q.TypeId into ctyp
                        from q in ctyp.DefaultIfEmpty()
                        join e in allunition on o.id equals e
                        join x in db.Accountss on o.accounts equals x.AccountsID
                        select new custmodal
                        {
                            id = o.id,
                           sourceoflead=(p==null)?"":(p.FirstName+" "+p.LastName),
                           customertypestr =(q==null)?"":q.Type,
                            CustomerCode = o.CustomerCode,
                            CustomerName = o.CustomerName,
                            TaxRegNo = o.TaxRegNo,
                            Location = o.Location,
                            Address = o.Address,
                            Phone = o.Phone,
                            Email = o.Email,
                            CreditLimit = o.CreditLimit,
                            CreditPeriod = o.CreditPeriod,
                            OpnBalance = o.OpnBalance,
                            Credit = o.Credit,
                            Debit = o.Debit,
                            Dev = o.Dev,
                            Details = o.Details,
                            Edit = o.Edit,
                            Delete = o.Delete,
                            Alias = o.Alias,
                            decimalcurrbalance = 0,
                            accounts = o.accounts,
                            mobmodel = contactslist.Where(xx => xx.ID == o.id).ToList(),
                            totalSales=gettotalsales(o.id,FromDate,ToDate),
                            nofsales= getnumberofsales(o.id, FromDate, ToDate),
                            noofqutation = getnumberofqut(o.id, FromDate, ToDate),

                        }).ToList();

                data = data.OrderByDescending(o => o.decimalcurrbalance).ToList();
                recordsTotal = data.Count();
            }
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new {  recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;


        }
        public long? getnumberofqut(long custimerid, string From, string To)
        {
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
            var v = (from a in db.Quotations
                     where a.Customer == custimerid &&
                      (From == "" || EF.Functions.DateDiffDay(a.QuotDate, fdate) <= 0) &&
                      (To == "" || EF.Functions.DateDiffDay(a.QuotDate, tdate) >= 0)
                     select new
                     {
                         a.QuotationId

                     }).Select(o => o.QuotationId).Count();

            return v;





        }

        public long? getnumberofsales(long custimerid, string From, string To)
        {
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
            var v = (from a in db.SalesEntrys
                     where a.Customer == custimerid &&
                      (From == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                      (To == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0)
                     select new
                     {
                         a.SalesEntryId

                     }).Select(o => o.SalesEntryId).Count();

            return v;




                    
        }

        public decimal? gettotalsales(long custimerid,string From,string To)
        {
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
            var v = (from a in db.SalesEntrys
                     where a.Customer == custimerid &&
                      (From == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                      (To == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0)
                     select new
                     {
                        total=(a==null)?0: a.SEGrandTotal
                     }).ToList().Select(o => o.total).Sum();
            return  v;




                    
        }
        //#region customer item wise
        [QkAuthorize(Roles = "Dev,Sales Customer ItemWise")]
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
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

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
        [QkAuthorize(Roles = "Dev,Sales Customer ItemWise")]
        public ActionResult GetCustomerItemWise(long ddlCustomer, long? ddlItem, string From, string To, string SalType, long? ddmc)
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
            SaleType St = new SaleType();
            if (SalType == "1" || SalType == "2")
            {
                St = (SalType == "1") ? SaleType.Sale : SaleType.Hire;
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


                         PriSaleQty = (decimal?)(from i in db.SEItemss
                                                 join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                 where (From == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                                 (To == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                                 (i.Item == a.ItemID && (i.ItemUnit == a.ItemUnitID || i.ItemUnit == null)) && (j.Customer == ddlCustomer)
                                                  && (SalType == "" || SalType == null || j.SaleType == St)
                                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                 group i by i.ItemId into g
                                                 select new
                                                 {
                                                     Total = g.Sum(x => x.ItemQuantity)
                                                 }).FirstOrDefault().Total ?? 0,

                         SubSaleQty = (decimal?)(from i in db.SEItemss
                                                 join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                 where (From == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                                 (To == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                                 (i.Item == a.ItemID && i.ItemUnit == a.SubUnitId) &&
                                                 e.ItemUnitName != f.ItemUnitName && (j.Customer == ddlCustomer)
                                                 && (SalType == "" || SalType == null || j.SaleType == St)
                                                 && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                 group i by i.ItemId into g
                                                 select new
                                                 {
                                                     Total = g.Sum(x => x.ItemQuantity)
                                                 }).FirstOrDefault().Total ?? 0,

                         PriRetQty = (decimal?)(from i in db.SRItemss
                                                join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                                where (From == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                                (To == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0) &&
                                                (i.Item == a.ItemID && (i.ItemUnit == a.ItemUnitID || i.ItemUnit == null)) && (j.Customer == ddlCustomer)
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

                         SaleAmt = (decimal?)(from i in db.SEItemss
                                              join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                              where (From == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                              (To == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                              (i.Item == a.ItemID) && (j.Customer == ddlCustomer)
                                              && (SalType == "" || SalType == null || j.SaleType == St)
                                              && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                              group i by i.ItemId into g
                                              select new
                                              {
                                                  Total = g.Sum(x => x.ItemSubTotal - x.ItemDiscount)
                                              }).FirstOrDefault().Total ?? 0,
                         PriSale = (decimal?)(from i in db.SEItemss
                                              join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                              where (From == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                              (To == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                              (i.Item == a.ItemID && (i.ItemUnit == e.ItemUnitID || i.ItemUnit == null)) && (j.Customer == ddlCustomer)
                                              && (SalType == "" || SalType == null || j.SaleType == St)
                                              && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                              group i by i.ItemId into g
                                              select new
                                              {
                                                  Total = g.Sum(x => x.ItemSubTotal - x.ItemDiscount)
                                              }).FirstOrDefault().Total ?? 0,
                         SubSale = (decimal?)(from i in db.SEItemss
                                              join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                              where (From == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                              (To == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                              (i.Item == a.ItemID && i.ItemUnit == f.ItemUnitID) &&
                                              e.ItemUnitName != f.ItemUnitName && (j.Customer == ddlCustomer)
                                              && (SalType == "" || SalType == null || j.SaleType == St)
                                              && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                              group i by i.ItemId into g
                                              select new
                                              {
                                                  Total = g.Sum(x => x.ItemSubTotal - x.ItemDiscount)
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



                         NoOfVchSale = (int?)(from i in db.SEItemss
                                              join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                              where (From == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                              (To == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                              (i.Item == a.ItemID) && (j.Customer == ddlCustomer)
                                              && (SalType == "" || SalType == null || j.SaleType == St)
                                              && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                              //group i by i.SalesEntry into g
                                              select new
                                              {
                                                  saleid = i.SalesEntry
                                              }).GroupBy(x => x.saleid).Count() ?? 0,

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

                         AvgPrice = ((o.PriSale * o.ConFactor) + o.SubSale) / ((o.PriSaleQty + (o.SubSaleQty / o.ConFactor)) != 0 ? (o.PriSaleQty + (o.SubSaleQty / o.ConFactor)) : 1),
                         PriSaleQty = (o.PriSaleQty + (int)(o.SubSaleQty / o.ConFactor)),
                         SubSaleQty = (o.SubSaleQty % o.ConFactor),

                         PriRetQty = (o.PriRetQty + (int)(o.SubRetQty / o.ConFactor)),
                         SubRetQty = (o.SubRetQty % o.ConFactor),

                         NetQty = (((o.PriSaleQty - o.PriRetQty) * o.ConFactor) + (o.SubSaleQty - o.SubRetQty)),

                         o.SaleAmt,
                         o.RetunAmt,
                         o.NoOfVchSale,
                         o.NoOfVchReturn,
                         o.ItemName,
                     }).Where(a => a.NoOfVchReturn != 0 || a.NoOfVchSale != 0).OrderBy(a => a.ItemName);

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




        //#region monthly
        [QkAuthorize(Roles = "Dev,MonthWise Sale Horizontal")]
        public ActionResult MonthlySale(string From, string To, long? ddlMC)
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
        //[HttpPost]
        //[QkAuthorize(Roles = "Dev,MonthWise Sale Horizontal")]

        [HttpPost]
        [QkAuthorize(Roles = "Dev,MonthWise Sale Horizontal")]
        public ActionResult ViewMonthlySale(string from, string to, long? ddlMC)
        {
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            ViewBag.ddlmc = ddlMC;
            companySet();
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,MonthWise Sale Horizontal")]
        public ActionResult GetMonthlySale(string fromdate, string todate, long? ddmc)
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
            var sentry = (from a in db.SalesEntrys
                          where (fromdate == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) && a.Status == 1 &&
                                (todate == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0)
                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                          select new
                          {
                              a.SalesEntryId,
                              a.BillNo,
                              SEDate = (DateTime)a.SEDate,
                              a.SETaxAmount,
                              a.SEDiscount,
                              //SSub = a.SESubTotal,
                              SESubTotal = a.SESubTotal - a.SEDiscount,
                              a.SETax,
                              a.SEGrandTotal,
                          }).GroupBy(x => new { x.SEDate }, (key, group) => new
                          {

                              SaleCount = group.Select(k => k.SalesEntryId),
                              MonthYear = (DateTime)key.SEDate,
                              SESaleTax = group.Sum(k => k.SETax),
                              SESaleTaxAmount = group.Sum(k => k.SETaxAmount),
                              SESubTotal = group.Sum(k => k.SESubTotal),
                              SEGrandTotal = group.Sum(k => k.SEGrandTotal),

                              SRSaleTax = (decimal)0,
                              SRSaleTaxAmount = (decimal)0,
                              SRSubTotal = (decimal)0,
                              SRGrandTotal = (decimal)0
                          }).ToList();

            var sreturn = (from a in db.SalesReturns
                           where (fromdate == "" || EF.Functions.DateDiffDay(a.SRDate, fdate) <= 0) &&
                                 (todate == "" || EF.Functions.DateDiffDay(a.SRDate, tdate) >= 0)
                                 && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                           select new
                           {
                               a.SalesReturnId,
                               SRDate = (DateTime)a.SRDate,
                               a.SRTax,
                               a.SRTaxAmount,
                               a.SRDiscount,
                               SRSubTotal = a.SRSubTotal - a.SRDiscount,
                               a.SRGrandTotal
                           }).GroupBy(x => new { x.SRDate }, (key, group) => new
                           {
                               SaleRetCount = group.Select(k => k.SalesReturnId),
                               MonthYear = (DateTime)key.SRDate,
                               SRSaleTax = group.Sum(k => k.SRTax),
                               SRSaleTaxAmount = group.Sum(k => k.SRTaxAmount),
                               SRSubTotal = group.Sum(k => k.SRSubTotal),
                               SRGrandTotal = group.Sum(k => k.SRGrandTotal),

                               SESaleTax = (decimal)0,
                               SESaleTaxAmount = (decimal)0,
                               SESubTotal = (decimal)0,
                               SEGrandTotal = (decimal)0
                           }).ToList();


            var sjoin = (from a in monthwise
                         join b in sentry on a.MonthYear equals b.MonthYear into txn
                         from b in txn.DefaultIfEmpty()
                         select new
                         {
                             MonthYear = (DateTime)a.MonthYear,
                             SESaleTax = b != null ? b.SESaleTax : 0,
                             SESaleTaxAmount = b != null ? b.SESaleTaxAmount : 0,
                             SESubTotal = b != null ? b.SESubTotal : 0,
                             SEGrandTotal = b != null ? b.SEGrandTotal : 0,
                         }).ToList();


            var rjoin = (from a in monthwise
                         join b in sreturn on a.MonthYear equals b.MonthYear into txn
                         from b in txn.DefaultIfEmpty()
                         select new
                         {

                             MonthYear = (DateTime)a.MonthYear,
                             SRSaleTax = b != null ? b.SRSaleTax : 0,
                             SRSaleTaxAmount = b != null ? b.SRSaleTaxAmount : 0,
                             SRSubTotal = b != null ? b.SRSubTotal : 0,
                             SRGrandTotal = b != null ? b.SRGrandTotal : 0,
                         }).ToList();


            var result = (from a in monthwise
                          join b in sjoin on a.MonthYear equals b.MonthYear into se
                          from b in se.DefaultIfEmpty()
                          join c in rjoin on a.MonthYear equals c.MonthYear into sr
                          from c in sr.DefaultIfEmpty()
                          select new
                          {

                              MonthYear = a.MonthYear,
                              SESaleTax = b != null ? b.SESaleTax : 0,
                              SESaleTaxAmount = b != null ? b.SESaleTaxAmount : 0,
                              SESubTotal = b != null ? b.SESubTotal : 0,
                              SEGrandTotal = b != null ? b.SEGrandTotal : 0,

                              SRSaleTax = c != null ? c.SRSaleTax : 0,
                              SRSaleTaxAmount = c != null ? c.SRSaleTaxAmount : 0,
                              SRSubTotal = c != null ? c.SRSubTotal : 0,
                              SRGrandTotal = c != null ? c.SRGrandTotal : 0,

                              NetAmount = b.SEGrandTotal - c.SRGrandTotal

                          }).GroupBy(x => new { Years = x.MonthYear.Value.Year, Months = x.MonthYear.Value.Month }, (key, group) => new
                          {
                              MonthYear = CustHtml.MonthName(key.Months.ToString()) + " - " + key.Years.ToString(),
                              SESaleTax = group.Sum(k => k.SESaleTax),
                              SESaleTaxAmount = group.Sum(k => k.SESaleTaxAmount),
                              SESubTotal = group.Sum(k => k.SESubTotal),
                              SEGrandTotal = group.Sum(k => k.SEGrandTotal),

                              SRSaleTax = group.Sum(k => k.SRSaleTax),
                              SRSaleTaxAmount = group.Sum(k => k.SRSaleTaxAmount),
                              SRSubTotal = group.Sum(k => k.SRSubTotal),
                              SRGrandTotal = group.Sum(k => k.SRGrandTotal),

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



        //#region daywise
        [QkAuthorize(Roles = "Dev,Sales Day Wise")]
        public ActionResult DayWise(string from, string to, long? ddmc, long? emp, long? task, long? project, long? customer, string hfrom, string hto, long? htype, long? stype)
        {

            ViewBag.fromdate = from;
            ViewBag.todate = to;
            companySet();

            ViewBag.Customer = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                               }, "Value", "Text", 0);
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
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

            ViewBag.SalesExec = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 0);

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

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

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View();
        }
        //[HttpPost]
        //[QkAuthorize(Roles = "Dev,Sales Day Wise")]

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Sales Day Wise")]
        public ActionResult GetDayWise(string From, string To, long? ddmc, long? emp, long? task, long? project, long? customer, string hfrom, string hto, long? htype, string stype)
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
            if (!string.IsNullOrEmpty(hfrom))
            {
                hfrmdate = DateTime.Parse(hfrom, new CultureInfo("en-GB"));
            }
            if (!string.IsNullOrEmpty(hto))
            {
                htodate = DateTime.Parse(hto, new CultureInfo("en-GB"));
            }
            SaleType St = new SaleType();
            if (stype != null)
            {
                St = (stype == "1") ? SaleType.Sale : SaleType.Hire;
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


            // Calc fix (N3): removed the unused HireDetails LEFT join (it fanned out every header total ×N for hire
            // invoices; `h` was never projected) AND restored the filters — was aggregating ALL sales (cancelled,
            // every MC, every date). Filters now mirror the `sreturn` sibling below exactly (SE fields vs SR).
            var sale = (from a in db.SalesEntrys
                        where (customer == 0 || a.Customer == customer)
                         && (emp == 0 || emp == null || a.SECashier == emp)
                         && (From == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) && a.Status == 1
                         && (To == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0)
                         && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                         && (project == 0 || project == null || a.Project == project)
                         && (task == 0 || task == null || a.ProTask == task)
                         && (stype == "" || a.SaleType == St)
                        select new
                        {
                            Date = a.SEDate,
                            a.SalesEntryId,
                            a.SETaxAmount,
                            a.SEGrandTotal,
                            a.CustomerType,
                            
                        }).GroupBy(x => new { x.Date }, (y, group) => new
                        {
                            SaleCount = group.Select(k => k.SalesEntryId).Count(),
                            cashsales=(decimal?)group.Where(k=>k.CustomerType==CustomerType.Walking).Sum(o=>o.SEGrandTotal)??0,
                            cardsahles = (decimal?)group.Where(k => k.CustomerType == CustomerType.Card).Sum(o => o.SEGrandTotal)??0,

                            Date = y.Date,
                            SaleTax = group.Sum(k => k.SETaxAmount),
                            TotalSale = group.Sum(k => k.SEGrandTotal),
                            RetSaleTax = (decimal)0,
                            RetTotalSale = (decimal)0,
                            ReturnCount = 0
                        }).ToList();


            var sreturn = (from a in db.SalesReturns
                           where (customer == 0 || a.Customer == customer)
                            && (emp == 0 || emp == null || a.SRCashier == emp)
                            && (From == "" || EF.Functions.DateDiffDay(a.SRDate, fdate) <= 0) && a.Status == 1
                            && (To == "" || EF.Functions.DateDiffDay(a.SRDate, tdate) >= 0)
                            && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                            && (project == 0 || project == null || a.Project == project)
                            && (task == 0 || task == null || a.ProTask == task)
                            && (stype == "" || a.SaleType == St)
                           select new
                           {
                               Date = a.SRDate,
                               a.SalesReturnId,
                               a.SRTaxAmount,
                               a.SRGrandTotal
                           }).GroupBy(x => new { x.Date }, (y, group) => new
                           {
                               ReturnCount = group.Select(k => k.SalesReturnId).Count(),
                               Date = y.Date,
                               RetSaleTax = group.Sum(k => k.SRTaxAmount),
                               RetTotalSale = group.Sum(k => k.SRGrandTotal),
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
                             cashsales=(b==null)?0:b.cashsales,
                             cardsahles=(b==null)?0:b.cardsahles,
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
                             cashsales=0,
                             cardsahles=0,
                      
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
                              Date = a.Date,
                              cashsales = b.cashsales,
                              cardsahles = b.cardsahles,
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
        [QkAuthorize(Roles = "Dev,Sales Day Wise")]
        public ActionResult ViewDayWise(string from, string to, long? ddmc, long? emp, long? task, long? project, long? customer, string hfrom, string hto, long? htype, long? stype)
        {
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            companySet();
            return View();
        }


        //#region Cash or Credit Summary
        [QkAuthorize(Roles = "Dev,CashOrCredit SaleSummary")]
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
        [QkAuthorize(Roles = "Dev,CashOrCredit SaleSummary")]
        public ActionResult CashOrCredit(string From, string To, long? ddlMC)
        {
            return RedirectToAction("ViewCashOrCredit", new { from = From, to = To, ddmc = ddlMC });
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,CashOrCredit SaleSummary")]
        public ActionResult ViewCashOrCredit(string from, string to, long? ddmc)
        {
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            companySet();
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,CashOrCredit SaleSummary")]
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

            var sale = (from a in db.SalesEntrys
                        where (From == "" || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) && a.Status == 1 &&
                              (To == "" || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0)
                              && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                        select new
                        {
                            Date = a.SEDate,
                            id = a.SalesEntryId,
                            tax = (decimal?)a.SETaxAmount,
                            total = (decimal?)a.SEGrandTotal,
                            type = (CustomerType?)a.CustomerType
                        }).GroupBy(x => new { x.Date }, (y, group) => new
                        {
                            SaleCount = (decimal?)group.Select(k => k.id).Count(),
                            Date = y.Date,
                            total = (decimal?)group.Sum(k => k.total),
                            cashtotal = (decimal?)group.Where(a => a.type == CustomerType.Walking).Sum(k => k.total),
                            credittotal = (decimal?)group.Where(a => a.type == CustomerType.Customer).Sum(k => k.total)
                        }).ToList();

            var data = sale.Skip(skip).Take(pageSize).ToList();
            recordsTotal = sale.Count();

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


        //#region Invoice item wise
        [QkAuthorize(Roles = "Dev,Sales Invoice ItemWise")]
        public ActionResult InvoiceItemWise(long? ddlItem, long? ddlCustomer, string from, string to, long? ddlMC)
        {


            if (ddlItem != null)
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
        [QkAuthorize(Roles = "Dev,Sales Invoice ItemWise")]
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
        [QkAuthorize(Roles = "Dev,Sales Invoice ItemWise")]
        public ActionResult getInvoiceWise(long? item, long? customer, string fromdate, string todate, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Select(a => (long?)a.MCId).ToList();
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

            var v = (from a in db.SEItemss
                     join b in db.Items on a.Item equals b.ItemID
                     join g in db.SalesEntrys on a.SalesEntry equals g.SalesEntryId
                     join c in db.Customers on g.Customer equals c.CustomerID
                     join e in db.ItemUnits on b.ItemUnitID equals e.ItemUnitID into primary
                     from e in primary.DefaultIfEmpty()
                     join f in db.ItemUnits on b.SubUnitId equals f.ItemUnitID into second
                     from f in second.DefaultIfEmpty()
                     where (item == 0 || a.Item == item) && (customer == 0 || g.Customer == customer)
                     && (fromdate == "" || EF.Functions.DateDiffDay(g.SEDate, fdate) <= 0)
                     && (todate == "" || EF.Functions.DateDiffDay(g.SEDate, tdate) >= 0)
                     && (ddmc==null||ddmc==0 || ddmc == g.MaterialCenter)
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
                         TotalAmount = a.ItemUnitPrice* a.ItemQuantity,
                         Discount = a.ItemDiscount,
                         a.itemNote,
                         a.ItemUnitPrice,
                         PriUnit = e.ItemUnitName,
                         SubUnit = f.ItemUnitName,
                         ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                         b.ItemUnitID,
                         b.SubUnitId,
                         b.SellingPrice,
                         Customer = c.CustomerName,
                         Date = g.SEDate
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


        //#region supplier wise
        //[QkAuthorize(Roles = "Dev,Sales Brand Wise")]
        public ActionResult SupplierWise()
        {
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
            return View();
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Sales Brand Wise")]
        public ActionResult SupplierWise(long? ddlSupplier, string From, string To, long? ddlMC)
        {
            return RedirectToAction("ViewSupplierWise", new { supplier = ddlSupplier, from = From, to = To, ddmc = ddlMC });
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Sales Supplier Wise")]
        public ActionResult GetSupplierWise(long? supplier, string fromdate, string todate, long? ddmc)
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
            int recordsTotal = 0;

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
                     where (supplier == 0 || a.SupplierID == supplier)
                     select new
                     {
                         Supplier = a.SupplierName,
                         a.SupplierID,
                         SaleAmt = (decimal?)(from i in db.SEItemss
                                              join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                              join k in db.Items on i.Item equals k.ItemID into itm
                                              from k in itm.DefaultIfEmpty()
                                              where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                              (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                              (k.Supplier == a.SupplierID)
                                              && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                              select new
                                              {
                                                  Total = i.ItemSubTotal - i.ItemDiscount
                                              }).Sum(x => x.Total) ?? 0,


                         RetunAmt = (decimal?)(from i in db.SRItemss
                                               join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                               join k in db.Items on i.Item equals k.ItemID into itm
                                               from k in itm.DefaultIfEmpty()
                                               where (fromdate == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                               (todate == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0) &&
                                                (k.Supplier == a.SupplierID)
                                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                               select new
                                               {
                                                   Total = i.ItemSubTotal - i.ItemDiscount
                                               }).Sum(x => x.Total) ?? 0,



                         // NoOfVchSale = (int?)db.SEItemss.Where(d => d.Item == a.ItemID).Select(d => d.SaleEntryId).Distinct().Count() ?? 0,
                         NoOfVchSale = (int?)(from i in db.SEItemss
                                              join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                              join k in db.Items on i.Item equals k.ItemID into itm
                                              from k in itm.DefaultIfEmpty()
                                              where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                              (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                             (k.Supplier == a.SupplierID)
                                             && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                              //group i by i.SalesEntry into g
                                              select new
                                              {
                                                  saleid = i.SalesEntry
                                              }).GroupBy(x => x.saleid).Count() ?? 0,

                         //NoOfVchReturn = (int?)db.SRItemss.Where(d => d.Item == a.ItemID).Select(d => d.SalesReturnId).Distinct().Count() ?? 0
                         NoOfVchReturn = (int?)(from i in db.SRItemss
                                                join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                                join k in db.Items on i.Item equals k.ItemID into itm
                                                from k in itm.DefaultIfEmpty()
                                                where (fromdate == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                                (todate == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0) &&
                                                (k.Supplier == a.SupplierID)
                                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                //  group i by i.ItemId into g
                                                select new
                                                {
                                                    saleid = i.SalesReturnId
                                                }).GroupBy(x => x.saleid).Count() ?? 0,

                     }).Distinct().AsEnumerable().Select(o => new
                     {
                         o.Supplier,
                         o.SupplierID,
                         o.SaleAmt,
                         o.RetunAmt,
                         o.NoOfVchSale,
                         o.NoOfVchReturn
                     }).OrderBy(x => x.SupplierID);

            recordsTotal = v.Count();
            var data = v.ToList();

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
        //[QkAuthorize(Roles = "Dev,Sales Supplier Wise")]
        public ActionResult ViewSupplierWise(long? supplier, string from, string to)
        {
            if (supplier != 0)
            {
                ViewBag.supplier = (from a in db.Suppliers
                                    where a.SupplierID == supplier
                                    select new
                                    {
                                        BName = a.SupplierName
                                    }).FirstOrDefault().BName;
            }
            else
            {
                ViewBag.supplier = "All";
            }
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            companySet();
            return View();
        }



        //#region Customer Area Wise Sales
        public ActionResult CustomerAreaWise()
        {

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            return View();
        }
        [HttpPost]
        public ActionResult CustomerAreaWise(string State, long? ddlMC)
        {
            return RedirectToAction("ViewCustomerAreaWise", new { state = State, ddmc = ddlMC });
        }

        [HttpPost]
        public ActionResult GetCustomerAreaWise(string state, long? ddmc)
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

            var v = (from a in db.SalesEntrys
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry//into pay
                     join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                     join e in db.Contacts on b.Contact equals e.ContactID into cont
                     from e in cont.DefaultIfEmpty()

                     join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                     from f in paymeth.DefaultIfEmpty()
                     join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                     from g in mcs.DefaultIfEmpty()
                     join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                     from h in hir.DefaultIfEmpty()
                     join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()
                     where (state == "0" || e.State == state) && a.Status == 1
                      && ((!MCList.Any() && ddmc == null) || MCArray.Contains(a.MaterialCenter) || ddmc == a.MaterialCenter)
                     select new
                     {
                         a.SalesEntryId,
                         a.SENo,
                         a.BillNo,
                         a.SEDate,
                         a.SEGrandTotal,
                         Customer = b.CustomerName,
                         TaxRegNo = i.TRN,
                         EmpName = d.FirstName + " " + d.LastName,
                         MCName = g.MCName,
                         c.SEPaidAmount,
                         a.CustomerType,
                         SEBalanceAmount = a.SEGrandTotal - c.SEPaidAmount,
                         a.SECreatedDate,
                         PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                         SaleType = a.SaleType,
                         FromDate = h.StartDate,
                         ToDate = h.EndDate,
                         HireType = h.HireType
                     }).OrderBy(a => a.SEDate).ThenBy(a => a.SECreatedDate);

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
        //[QkAuthorize(Roles = "Dev,Sales Supplier Wise")]
        public ActionResult ViewCustomerAreaWise(string state, long? ddmc)
        {
            ViewBag.State = state;
            companySet();
            return View();
        }

        public ActionResult StockValue()
        {
            ViewBag.Item = QkSelect.List(
                                       new List<SelectListItem>
                                       {
                                    new SelectListItem { Selected = false,Text = "Select Item", Value = "0"},
                                       }, "Value", "Text", 1);

            ViewBag.Supplier = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);


            companySet();
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var use = db.MCs.Select(s => new SelectFormat { id = s.MCId, text = s.MCName }).ToList();
            var initial = new SelectFormat() { id = 0, text = "All" };
            use.Insert(0, initial);
            long[] selmc = { 0 };


            ViewBag.mc = new MultiSelectList(use, "id", "text", selmc);

            ViewBag.Brand = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                }, "Value", "Text", 0);

            ViewBag.Category = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                }, "Value", "Text", 0);
            List<SelectListItem> SelectPeriod = new List<SelectListItem>() {
                    new SelectListItem {
                          Text = "3 Month", Value = "3"
                                        },
                    new SelectListItem {
                        Text = "2 Month", Value = "2"
                                   },
                    new SelectListItem {

                         Text = "1 Month", Value = "1"
                          },

            };
            ViewBag.Period = SelectPeriod;

            return View();
        }

        //#region Sale Receipt Summary List
        [HttpGet]
        [QkAuthorize(Roles = "Dev,SaleReceipt Summary")]
        public ActionResult SaleReceiptSummary()
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var OpAll = QkSelect.List(
                         new List<SelectListItem>
                         {
                          new SelectListItem { Selected = true, Text = "All", Value = "0"},
                         }, "Value", "Text", 1);

            ViewBag.Customer = OpAll;
            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();

            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            companySet();

            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,SaleReceipt Summary")]
        public ActionResult GetSaleReceiptSummary(string vno, long? customer, string fromdate, string todate)
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

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
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

            var v = (from a in db.SalesEntrys
                     join f in db.SEPayments on a.SalesEntryId equals f.SalesEntry into salesP
                     from f in salesP.DefaultIfEmpty()
                     join c in db.Customers on a.Customer equals c.CustomerID into cust
                     from c in cust.DefaultIfEmpty()
                     let Rc = (from d in db.Receipts
                               join e in db.Accountss on d.PayTo equals e.AccountsID into Acc
                               from e in Acc.DefaultIfEmpty()
                               join p in db.PDCs on new { e1 = d.ReceiptId, e2 = "Receipt" }
                               equals new { e1 = p.Reference, e2 = p.PDCType } into pdc
                               from p in pdc.DefaultIfEmpty()
                               join b in db.SETransactions on d.ReceiptId equals b.Recieptid
                               where b.SalesEntry == a.SalesEntryId && a.Status == 1
                               select new
                               {
                                   RecieptNo = d.VoucherNo,
                                   Date = (DateTime?)d.Date,
                                   Account = e.Name,
                                   CheckNo = p.CheckNo,
                                   PDCDate = (DateTime?)d.PDCDate,
                                   amt = b.SEPayAmount,
                                   RecieptAmount = d.Paying + d.Discount,
                                   CBD = d.Remark == "Direct Reciept From Sale Entry" ? "" : d.Remark,
                                   Type = (ModeOfPayment?)d.MOPayment

                               }).ToList()
                     where (vno == "" || vno == null || a.BillNo == vno) &&
                     (customer == 0 || a.Customer == customer) &&
                     ((fromdate == null || EF.Functions.DateDiffDay(a.SEDate, fdate) <= 0) &&
                     (todate == null || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0))
                     select new
                     {
                         id = a.SalesEntryId,
                         BillNo = a.BillNo,
                         SEDate = (DateTime?)a.SEDate,
                         Customer = c.CustomerName,
                         Amount = a.SEGrandTotal,
                         Paid = a.SEGrandTotal,
                         Balance = (f.SEBillAmount - f.SEPaidAmount),
                         rc = Rc
                     }).OrderBy(a => a.SEDate);
            //         equals new { e1 = p.Reference, e2 = p.PDCType } into pdc
            //         (todate == null || EF.Functions.DateDiffDay(a.SEDate, tdate) >= 0))
            //             id = a.SalesEntryId,
            //             BillNo = a.BillNo,
            //             SEDate = (DateTime?)a.SEDate,
            //             Customer = c.CustomerName,
            //             Amount = a.SEGrandTotal,
            //             Paid = a.SEGrandTotal,
            //             RecieptNo = d.VoucherNo,
            //             Date = (DateTime?)d.Date,
            //             Account = e.Name,
            //             CheckNo = p.CheckNo,
            //             PDCDate = (DateTime?)d.PDCDate,
            //             amt = b.SEPayAmount,
            //             RecieptAmount = d.Paying+d.Discount,
            //             CBD = d.Remark == "Direct Reciept From Sale Entry"?"":d.Remark,
            //             Type =  (ModeOfPayment?) d.MOPayment

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



        //#region CustomerItemWiseSummary
        [HttpGet]
        [QkAuthorize(Roles = "Dev,CustomerItemWiseSummary")]
        public ActionResult CustomerItemWiseSummary()
        {

            ViewBag.Customer = QkSelect.List(
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
        [QkAuthorize(Roles = "Dev,CustomerItemWiseSummary")]
        public ActionResult GetCustomerItemWiseSum(long? Cust, long? Item, string fromdate, string todate, long? MC)
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
            var sale = (from a in db.SEItemss
                        join b in db.SalesEntrys on a.SalesEntry equals b.SalesEntryId
                        join c in db.Customers on b.Customer equals c.CustomerID into cust
                        from c in cust.DefaultIfEmpty()
                        join d in db.Items on a.Item equals d.ItemID
                        join e in db.ItemUnits on d.ItemUnitID equals e.ItemUnitID into primary
                        from e in primary.DefaultIfEmpty()
                        join f in db.ItemUnits on d.SubUnitId equals f.ItemUnitID into second
                        from f in second.DefaultIfEmpty()
                        where (Item == 0 || Item == null || a.Item == Item) && (Cust == 0 || Cust == null || b.Customer == Cust)
                        && (fromdate == "" || EF.Functions.DateDiffDay(b.SEDate, fdate) <= 0) && b.Status == 1
                        && (todate == "" || EF.Functions.DateDiffDay(b.SEDate, tdate) >= 0)
                        && ((!MCList.Any() && MC == null) || MCArray.Contains(b.MaterialCenter) || MC == b.MaterialCenter)
                        && b.SaleType == SaleType.Sale
                        select new
                        {
                            Date = b.SEDate,
                            c.CustomerCode,
                            c.CustomerName,
                            b.SalesEntryId,
                            d.ItemCode,
                            ConFactor = d.ConFactor != 0 ? d.ConFactor : 1,

                            d.ItemUnitID,
                            d.SubUnitId,
                            PriUnit = e.ItemUnitName,
                            SubUnit = f.ItemUnitName,

                            PQty = (decimal?)(from i in db.SEItemss
                                              join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                              where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                              (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                              (i.Item == d.ItemID && i.ItemUnit == d.ItemUnitID) && (Cust == 0 || Cust == null || j.Customer == Cust)
                                               && ((!MCList.Any() && MC == null) || MCArray.Contains(j.MaterialCenter) || MC == j.MaterialCenter)
                                               && j.SaleType == SaleType.Sale && j.SalesEntryId == b.SalesEntryId
                                              group i by i.ItemId into g
                                              select new
                                              {
                                                  Total = g.Sum(x => x.ItemQuantity)
                                              }).FirstOrDefault().Total ?? 0,

                            SQty = (decimal?)(from i in db.SEItemss
                                              join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                              where (fromdate == "" || EF.Functions.DateDiffDay(j.SEDate, fdate) <= 0) &&
                                              (todate == "" || EF.Functions.DateDiffDay(j.SEDate, tdate) >= 0) &&
                                              (i.Item == d.ItemID && i.ItemUnit == d.SubUnitId) &&
                                              e.ItemUnitName != f.ItemUnitName && (Cust == 0 || Cust == null || j.Customer == Cust)
                                               && ((!MCList.Any() && MC == null) || MCArray.Contains(j.MaterialCenter) || MC == j.MaterialCenter)
                                               && j.SaleType == SaleType.Sale && j.SalesEntryId == b.SalesEntryId
                                              group i by i.ItemId into g
                                              select new
                                              {
                                                  Total = g.Sum(x => x.ItemQuantity)
                                              }).FirstOrDefault().Total ?? 0,


                            SValue = (decimal?)(from i in db.SEItemss
                                                join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                                where (i.Item == d.ItemID) && j.SalesEntryId == b.SalesEntryId
                                                group i by i.ItemId into g
                                                select new
                                                {
                                                    Total = g.Sum(x => x.ItemSubTotal - x.ItemDiscount)
                                                }).FirstOrDefault().Total ?? 0,

                        }).AsEnumerable().Select(o => new
                        {
                            ItemCode = (o.ItemCode != null) ? o.ItemCode : "",
                            Date = o.Date,
                            CustomerCode = o.CustomerCode,
                            CustomerName = o.CustomerName,

                            o.ItemUnitID,
                            o.SubUnitId,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            o.ConFactor,

                            SValue = o.SValue,
                            PQty = (o.PQty + (int)(o.SQty / o.ConFactor)),
                            SQty = (o.SQty % o.ConFactor),
                            PRQty = (decimal)0,
                            SRQty = (decimal)0,
                            SRValue = (decimal)0,
                        }).ToList();


            var sreturn = (from a in db.SRItemss
                           join b in db.SalesReturns on a.SalesReturnId equals b.SalesReturnId
                           join c in db.Customers on b.Customer equals c.CustomerID into cust
                           from c in cust.DefaultIfEmpty()
                           join d in db.Items on a.Item equals d.ItemID
                           join e in db.ItemUnits on d.ItemUnitID equals e.ItemUnitID into primary
                           from e in primary.DefaultIfEmpty()
                           join f in db.ItemUnits on d.SubUnitId equals f.ItemUnitID into second
                           from f in second.DefaultIfEmpty()
                           where (Item == 0 || Item == null || a.Item == Item) && (Cust == 0 || Cust == null || b.Customer == Cust)
                            && (fromdate == "" || EF.Functions.DateDiffDay(b.SRDate, fdate) <= 0) && b.Status == 1
                            && (todate == "" || EF.Functions.DateDiffDay(b.SRDate, tdate) >= 0)
                            && ((!MCList.Any() && MC == null) || MCArray.Contains(b.MaterialCenter) || MC == b.MaterialCenter)
                            && b.SaleType == SaleType.Sale
                           select new
                           {
                               Date = b.SRDate,
                               CustomerCode = c.CustomerCode,
                               CustomerName = c.CustomerName,
                               a.SalesReturnId,
                               d.ItemCode,
                               ConFactor = d.ConFactor != 0 ? d.ConFactor : 1,
                               d.ItemUnitID,
                               d.SubUnitId,
                               PriUnit = e.ItemUnitName,
                               SubUnit = f.ItemUnitName,

                               PQty = (decimal?)(from i in db.SRItemss
                                                 join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                                 where (fromdate == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                                 (todate == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0) &&
                                                 (i.Item == d.ItemID && i.ItemUnit == d.ItemUnitID) && (Cust == 0 || Cust == null || j.Customer == Cust)
                                                  && ((!MCList.Any() && MC == null) || MCArray.Contains(j.MaterialCenter) || MC == j.MaterialCenter)
                                                  && j.SaleType == SaleType.Sale && j.SalesReturnId == b.SalesReturnId
                                                 group i by i.ItemId into g
                                                 select new
                                                 {
                                                     Total = g.Sum(x => x.ItemQuantity)
                                                 }).FirstOrDefault().Total ?? 0,

                               SQty = (decimal?)(from i in db.SRItemss
                                                 join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                                 where (fromdate == "" || EF.Functions.DateDiffDay(j.SRDate, fdate) <= 0) &&
                                                 (todate == "" || EF.Functions.DateDiffDay(j.SRDate, tdate) >= 0) &&
                                                 (i.Item == d.ItemID && i.ItemUnit == d.SubUnitId) &&
                                                 e.ItemUnitName != f.ItemUnitName && (Cust == 0 || Cust == null || j.Customer == Cust)
                                                  && ((!MCList.Any() && MC == null) || MCArray.Contains(j.MaterialCenter) || MC == j.MaterialCenter)
                                                  && j.SaleType == SaleType.Sale && j.SalesReturnId == b.SalesReturnId
                                                 group i by i.ItemId into g
                                                 select new
                                                 {
                                                     Total = g.Sum(x => x.ItemQuantity)
                                                 }).FirstOrDefault().Total ?? 0,

                               SValue = (decimal?)(from i in db.SRItemss
                                                   join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                                   join k in db.Items on i.Item equals k.ItemID
                                                   where (i.Item == d.ItemID) && i.SalesReturnId == a.SalesReturnId
                                                   group i by i.ItemId into g
                                                   select new
                                                   {
                                                       Total = g.Sum(x => x.ItemSubTotal - x.ItemDiscount)
                                                   }).FirstOrDefault().Total ?? 0,


                           }).AsEnumerable().Select(o => new
                           {
                               ItemCode = (o.ItemCode != null) ? o.ItemCode : "",
                               Date = o.Date,
                               CustomerCode = o.CustomerCode,
                               CustomerName = o.CustomerName,

                               o.ItemUnitID,
                               o.SubUnitId,
                               PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                               SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                               o.ConFactor,

                               SValue = (decimal)0,
                               PQty = (decimal)0,
                               SQty = (decimal)0,
                               PRQty = (o.PQty + (int)(o.SQty / o.ConFactor)),
                               SRQty = (o.SQty % o.ConFactor),
                               SRValue = o.SValue,
                           }).ToList();


            var sjoin = (from a in dates
                         join b in sale on a.Date equals b.Date into trans
                         from b in trans.DefaultIfEmpty()
                         select new
                         {
                             Date = a.Date,
                             ItemCode = (b != null) ? b.ItemCode : "",
                             CustomerCode = (b != null) ? b.CustomerCode : "",
                             CustomerName = (b != null) ? b.CustomerName : "",
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
                         }).ToList().GroupBy(x => new { Date = x.Date, ItemCode = x.ItemCode, CustomerCode = x.CustomerCode }, (key, group) => new
                         {
                             ItemCode = key.ItemCode,
                             Date = key.Date,
                             CustomerCode = key.CustomerCode,
                             CustomerName = group.Select(y => y.CustomerName).FirstOrDefault(),

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
                         join b in sreturn on a.Date equals b.Date into trans
                         from b in trans.DefaultIfEmpty()
                         select new
                         {
                             ItemCode = (b != null) ? b.ItemCode : "",
                             Date = a.Date,
                             CustomerCode = (b != null) ? b.CustomerCode : "",
                             CustomerName = (b != null) ? b.CustomerName : "",
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
                         }).ToList().GroupBy(x => new { Date = x.Date, ItemCode = x.ItemCode, CustomerCode = x.CustomerCode }, (key, group) => new
                         {
                             ItemCode = key.ItemCode,
                             Date = key.Date,
                             CustomerCode = key.CustomerCode,
                             CustomerName = group.Select(y => y.CustomerName).FirstOrDefault(),

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
                              CustomerCode = a.CustomerCode,
                              CustomerName = a.CustomerName,

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
                          .GroupBy(x => new { Date = x.Date, ItemCode = x.ItemCode, CustomerCode = x.CustomerCode }, (key, group) => new
                          {
                              ItemCode = key.ItemCode,
                              Date = key.Date,
                              CustomerCode = key.CustomerCode,
                              CustomerName = group.Select(y => y.CustomerName).FirstOrDefault(),

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
                     join b in sjoin on new { h1 = a.Date, h2 = a.ItemCode, h3 = a.CustomerCode }
                     equals new { h1 = b.Date, h2 = b.ItemCode, h3 = b.CustomerCode } into sa
                     from b in sa.DefaultIfEmpty()
                     join c in rjoin on new { h1 = a.Date, h2 = a.ItemCode, h3 = a.CustomerCode }
                     equals new { h1 = c.Date, h2 = c.ItemCode, h3 = c.CustomerCode } into ret
                     from c in ret.DefaultIfEmpty()
                     select new
                     {
                         a.ItemCode,
                         a.Date,
                         a.CustomerCode,
                         a.CustomerName,

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
                     }).Where(x => x.CustomerName != "").ToList();

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
