using QuickSoft.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using QuickSoft.Controllers;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Areas.Property.Controllers
{
    [Microsoft.AspNetCore.Mvc.Area("Property")]
    public class PropertyReportsController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PropertyReportsController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Property/MyReports
        public ActionResult Index()
        {
            return View();
        }
        #region Property Registration
        public ActionResult PropertyRegistration()
        {
            var OpAll = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);
            ViewBag.Alldata = OpAll;
            return View();
        }

        [HttpPost]
        public ActionResult GetPropertyRegistrations(string Voucher, long? Developer, long? Owner, long? Broker, long? Property, string fromdate, string todate)
        {

            var UserId = User.Identity.GetUserId();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
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

            var v = (from a in db.PropertyRegistrations
                     join b in db.Developers on a.Developer equals b.DeveloperID into cust
                     from b in cust.DefaultIfEmpty()
                     join c in db.Accountss on a.Owner equals c.AccountsID into pay
                     from c in pay.DefaultIfEmpty()
                     join d in db.Brokers on a.Broker equals d.BrokerID into emp
                     from d in emp.DefaultIfEmpty()
                     join e in db.PropertyMains on a.Property equals e.Id into mcs
                     from e in mcs.DefaultIfEmpty()
                     where (Voucher == "" || a.VoucherNo == Voucher)
                     && (Developer == 0 || Developer == null || a.Developer == Developer)
                     && (Owner == 0 || Owner == null || a.Owner == Owner)
                     && (Broker == 0 || Broker == null || a.Broker == Broker)
                     && (Property == 0 || Property == null || a.Property == Property)
                         && (fromdate == "" || EF.Functions.DateDiffDay(a.RDate, fdate) <= 0)
                       && (todate == "" || EF.Functions.DateDiffDay(a.RDate, tdate) >= 0)
                     select new
                     {
                         Id = a.RegistrationID,
                         Voucher = a.VoucherNo,
                         Date = a.RDate,
                         Developer = b.DeveloperName,
                         Owner = c.Name,
                         Broker = d.BrokerName,
                         Property = e.Name,
                     }).OrderBy(a => a.Date);

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

            //return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        #endregion
        public ActionResult PropertyConsolidated()
        {
            ViewBag.Alldata = QkSelect.List(
                         new List<SelectListItem>
                         {
                             new SelectListItem { Selected = false, Text = "Select Property", Value = ""},
                         }, "Value", "Text", 1);
            propertyreportviewmodel vmodel = new propertyreportviewmodel();
            vmodel.properyname = "no";
            return View(vmodel);

        }
        [HttpPost]
        public ActionResult PropertyConsolidated(long ddlProperty)
        {
            ViewBag.Alldata = QkSelect.List(
                    new List<SelectListItem>
                    {
                             new SelectListItem { Selected = false, Text = "Select Property", Value = ""},
                    }, "Value", "Text", 1);
            var ppt = db.PropertyMains.Find(ddlProperty);
            
            propertyreportviewmodel vmodel = new propertyreportviewmodel();
            vmodel.properyname = ppt.Name;
            vmodel.address = ppt.Address;
            long owner=db.PropertyRegistrations.Where(o => o.Property == ddlProperty).Select(o => o.Owner).FirstOrDefault();
            vmodel.owner = db.Accountss.Find(owner).Name;
            if (ppt.PropertyType != null)
            {
                long propertytype = (long)ppt.PropertyType;
                vmodel.propertytype = db.PropertyTypes.Find(propertytype).Name;
            }
            vmodel.address = ppt.Address;
            vmodel.docmodel = (from a in db.PropertyDocumentTypes
                        join b in db.DocumentFiles on a.ID equals b.Document
                        join c in db.DocumentTypes on a.DocumentType equals c.ID
                        where a.Reference == ddlProperty && a.Purpose == "Property"
                               select new DocumentTypeViewModel
                        {
                            Attachments = b.attachments,
                            Type = c.Name,
                           
                            ID = a.ID,
                            ExpDate = a.ExpDate,
                          
                        }).ToList();

            TenancyContract tc = db.TenancyContracts.Find(db.TenancyContracts.Where(o => o.Property == ddlProperty).Select(o => o.Id).FirstOrDefault());

            if (tc != null)
            {

                long? tenant = db.TenancyContracts.Where(o => o.Property == ddlProperty).Select(o => o.Tenant).FirstOrDefault();
                if (tenant != null)
                    vmodel.tenantname = db.Tenants.Where(o => o.TenantID == tenant).Select(o => o.TenantName).FirstOrDefault();
                vmodel.startdate = tc.StartDate.ToString("dd/MM/yyyy"); 
                vmodel.enddate = tc.EndDate.ToString("dd/MM/yyyy");
                vmodel.rent = tc.Rent.ToString();
                vmodel.tenancydoc = (from a in db.PropertyDocumentTypes
                                     join b in db.DocumentFiles on a.ID equals b.Document
                                     join c in db.DocumentTypes on a.DocumentType equals c.ID
                                     where a.Reference == tc.Id && a.Purpose == "Tenancy"
                                     select new DocumentTypeViewModel
                                     {
                                         Attachments = b.attachments,
                                         Type = c.Name,

                                         ID = a.ID,
                                         ExpDate = a.ExpDate,

                                     }).ToList();
            }
            Maintenance mc = db.Maintenances.Find(db.Maintenances.Where(o => o.Property == ddlProperty).Select(o => o.ID).FirstOrDefault());
            if (mc != null)
            {
                vmodel.amcfees = mc.Amount.ToString();
                vmodel.amcstartdate = mc.StartDate.ToString();
                vmodel.amcenddate = mc.EndDate.ToString();
                long contractor = mc.Contractor;
                vmodel.contractorname = db.Contractors.Find(contractor).ContractorName;
            }


            return View(vmodel);
        }
        //PropertySummery
        public ActionResult PropertySummery()
        {
            ViewBag.Alldata = QkSelect.List(
                         new List<SelectListItem>
                         {
                             new SelectListItem { Selected = false, Text = "Select Property", Value = ""},
                         }, "Value", "Text", 1);
            propertysummery vmodel = new propertysummery();
            vmodel.properyname = "no";
            return View(vmodel);

        }
        [HttpPost]
        public ActionResult PropertySummery(propertysummery vmodel)
        {
            ViewBag.Alldata = QkSelect.List(
                    new List<SelectListItem>
                    {
                             new SelectListItem { Selected = false, Text = "Select Property", Value = ""},
                    }, "Value", "Text", 1);
            var ppt = db.PropertyMains.Find(vmodel.propertyid);

            DateTime fromdate = DateTime.Parse(vmodel.startdate, new CultureInfo("En-GB"));
            DateTime enddate = DateTime.Parse(vmodel.enddate, new CultureInfo("En-GB"));



            vmodel.properyname = ppt.Name;

            List<chequeslist> Reciept = (from c in db.Receipts 
                           join d in db.PDCs on c.ReceiptId equals d.Reference into ps
                           from p in ps.DefaultIfEmpty()
                           where (vmodel.startdate == null || EF.Functions.DateDiffDay(c.Date, fromdate) <= 0) &&
                           (vmodel.enddate == null || EF.Functions.DateDiffDay(c.Date, enddate) >= 0) &&
                           (c.Project == ppt.Id )
                          
                           select  new chequeslist
                           {
                               Amount =c.Paying,
                               description =c.Remark,
                               pdcdate=c.PDCDate,
                               Bank=p.Bank,
                               ChequeNo=p.CheckNo
                           }).ToList();
            vmodel.RentalIncome = Reciept.Sum(o => o.Amount);
            vmodel.receiptchequedetails = Reciept;

            List<chequeslist> expense = (from c in db.Payments
                                         join e in db.Accountss on c.PayTo equals e.AccountsID
                                         join d in db.PDCs on c.PaymentId equals d.Reference into ps
                                         from d in ps.DefaultIfEmpty()
                                         where (vmodel.startdate == null || EF.Functions.DateDiffDay(c.Date, fromdate) <= 0) &&
                                         (vmodel.enddate == null || EF.Functions.DateDiffDay(c.Date, enddate) >= 0) &&
                                         (c.Project == ppt.Id )

                                         select new chequeslist
                                         {
                                             Amount = c.Paying,
                                             description =e.Name+" "+ c.Remark,
                                             pdcdate = (c.PDCDate==null)?c.Date:c.PDCDate,
                                             Bank = (d.Bank==null)?"Cash":d.Bank,
                                             ChequeNo = d.CheckNo
                                         }).ToList();

            vmodel.paymentchequedetails = expense;
            vmodel.TotalExpenses = expense.Sum(o => o.Amount);
            vmodel.NetIncome = vmodel.RentalIncome - vmodel.TotalExpenses;
            TenancyContract tc = db.TenancyContracts.Find(db.TenancyContracts.Where(o => o.Property == vmodel.propertyid).Select(o => o.Id).FirstOrDefault());

            if (tc != null)
            {

                long? tenant = db.TenancyContracts.Where(o => o.Property == vmodel.propertyid).Select(o => o.Tenant).FirstOrDefault();
                if (tenant != null)
                    vmodel.tenantname = db.Tenants.Where(o => o.TenantID == tenant).Select(o => o.TenantName).FirstOrDefault();
                vmodel.tenstartdate = tc.StartDate.ToString("dd/MM/yyyy"); 
                vmodel.tenenddate = tc.EndDate.ToString("dd/MM/yyyy");
                var tenadress = (from a in db.Tenants
                                        join b in db.Contacts on a.Contact equals b.ContactID
                                        select new
                                        {
                                                tenentaddress= b.Address

                                        }

                                       ).FirstOrDefault();
                vmodel.tenentaddress = tenadress.tenentaddress;
            }
            Maintenance mc = db.Maintenances.Find(db.Maintenances.Where(o => o.Property == vmodel.propertyid).Select(o => o.ID).FirstOrDefault());
            if (mc != null)
            {
                vmodel.amcamount = mc.Amount;
                vmodel.amcstartdate = mc.StartDate.ToString();
                vmodel.amcenddate = mc.EndDate.ToString();
             
            }


            return View(vmodel);
        }
        //PropertySummery

        public ActionResult PropertyConsolidated2(long ddlProperty)
        {
            ViewBag.Alldata = QkSelect.List(
                    new List<SelectListItem>
                    {
                             new SelectListItem { Selected = false, Text = "Select Property", Value = ""},
                    }, "Value", "Text", 1);
            var ppt = db.PropertyMains.Find(ddlProperty);

            propertyreportviewmodel vmodel = new propertyreportviewmodel();
            vmodel.properyname = ppt.Name;
            vmodel.address = ppt.Address;
            long owner = db.PropertyRegistrations.Where(o => o.Property == ddlProperty).Select(o => o.Owner).FirstOrDefault();
            vmodel.owner = db.Accountss.Find(owner).Name;
            if (ppt.PropertyType != null)
            {
                long propertytype = (long)ppt.PropertyType;
                vmodel.propertytype = db.PropertyTypes.Find(propertytype).Name;
            }
            vmodel.address = ppt.Address;
            vmodel.docmodel = (from a in db.PropertyDocumentTypes
                               join b in db.DocumentFiles on a.ID equals b.Document
                               join c in db.DocumentTypes on a.DocumentType equals c.ID
                               where a.Reference == ddlProperty && a.Purpose == "Property"
                               select new DocumentTypeViewModel
                               {
                                   Attachments = b.attachments,
                                   Type = c.Name,

                                   ID = a.ID,
                                   ExpDate = a.ExpDate,

                               }).ToList();

            TenancyContract tc = db.TenancyContracts.Find(db.TenancyContracts.Where(o => o.Property == ddlProperty).Select(o => o.Id).FirstOrDefault());

            if (tc != null)
            {

                long? tenant = db.TenancyContracts.Where(o => o.Property == ddlProperty).Select(o => o.Tenant).FirstOrDefault();
                if (tenant != null)
                    vmodel.tenantname = db.Tenants.Where(o => o.TenantID == tenant).Select(o => o.TenantName).FirstOrDefault();
                vmodel.startdate = tc.StartDate.ToString("dd/MM/yyyy");
                vmodel.enddate = tc.EndDate.ToString("dd/MM/yyyy");
                vmodel.rent = tc.Rent.ToString();
                vmodel.tenancydoc = (from a in db.PropertyDocumentTypes
                                     join b in db.DocumentFiles on a.ID equals b.Document
                                     join c in db.DocumentTypes on a.DocumentType equals c.ID
                                     where a.Reference == tc.Id && a.Purpose == "Tenancy"
                                     select new DocumentTypeViewModel
                                     {
                                         Attachments = b.attachments,
                                         Type = c.Name,

                                         ID = a.ID,
                                         ExpDate = a.ExpDate,

                                     }).ToList();
            }
            Maintenance mc = db.Maintenances.Find(db.Maintenances.Where(o => o.Property == ddlProperty).Select(o => o.ID).FirstOrDefault());
            if (mc != null)
            {
                vmodel.amcfees = mc.Amount.ToString();
                vmodel.amcstartdate = mc.StartDate.ToString();
                vmodel.amcenddate = mc.EndDate.ToString();
                long contractor = mc.Contractor;
                vmodel.contractorname = db.Contractors.Find(contractor).ContractorName;
            }


            return View(vmodel);
        }

        #region Tenancy Contract
        public ActionResult TenancyContract()
        {
            var OpAll = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);
            ViewBag.Alldata = OpAll;

            ViewBag.DueDates = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value="0"},
                new SelectListItem() {Text = "1st", Value="1"},
                new SelectListItem() {Text = "2nd", Value="2"},
                new SelectListItem() {Text = "3rd", Value="3"},
                new SelectListItem() {Text = "4th", Value="4"},
                new SelectListItem() {Text = "5th", Value="5"},
                new SelectListItem() {Text = "6th", Value="6"},
                new SelectListItem() {Text = "7th", Value="7"},
                new SelectListItem() {Text = "8th", Value="8"},
                new SelectListItem() {Text = "9th", Value="9"},
                new SelectListItem() {Text = "10th", Value="10"},
                new SelectListItem() {Text = "11th", Value="11"},
                new SelectListItem() {Text = "12th", Value="12"},
                new SelectListItem() {Text = "13th", Value="13"},
                new SelectListItem() {Text = "14th", Value="14"},
                new SelectListItem() {Text = "15th", Value="15"},
                new SelectListItem() {Text = "16th", Value="16"},
                new SelectListItem() {Text = "17th", Value="17"},
                new SelectListItem() {Text = "18th", Value="18"},
                new SelectListItem() {Text = "19th", Value="19"},
                new SelectListItem() {Text = "20th", Value="20"},
                new SelectListItem() {Text = "21st", Value="21"},
                new SelectListItem() {Text = "22nd", Value="22"},
                new SelectListItem() {Text = "23rd", Value="23"},
                new SelectListItem() {Text = "24th", Value="24"},
                new SelectListItem() {Text = "25th", Value="25"},
                new SelectListItem() {Text = "26th", Value="26"},
                new SelectListItem() {Text = "27th", Value="27"},
                new SelectListItem() {Text = "28th", Value="28"},
                new SelectListItem() {Text = "29th", Value="29"},
                new SelectListItem() {Text = "30th", Value="30"},
                new SelectListItem() {Text = "31st", Value="31"},
            }, "Value", "Text");
            return View();
        }

        [HttpPost]
        public ActionResult GetTenancyContract(long? Unit, long? DocumentType, long? Tenant, long? Property, long? PayType, long? Schedule, long? Duedate, string fromdate, string todate)
        {

            var UserId = User.Identity.GetUserId();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
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

            var v = (from a in db.TenancyContracts
                     join b in db.Customers on a.Tenant equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join c in db.PropertyMains on a.Property equals c.Id into mcs
                     from c in mcs.DefaultIfEmpty()
                     join d in db.PropertyUnits on a.Unit equals d.Id into un
                     from d in un.DefaultIfEmpty()
                     join e in db.Durations on a.Duration equals e.Id into dur
                     from e in dur.DefaultIfEmpty()
                     where (Tenant == 0 || Tenant == null || a.Tenant == Tenant)
                     && (Property == 0 || Property == null || a.Property == Property)
                     && (fromdate == "" || EF.Functions.DateDiffDay(a.CreatedDate, fdate) <= 0)
                     && (todate == "" || EF.Functions.DateDiffDay(a.CreatedDate, tdate) >= 0)
                     select new
                     {
                         Id = a.Id,
                         //Tenant=b.TenantName,
                         //Property = e.Name,
                         Unit = d.Name,
                         Rent = a.Rent,
                         Deposit = a.Deposit,
                         Date = a.CreatedDate,
                         a.Schedule,
                         a.DueDate,
                         Tenant = b.CustomerName,
                         Property = c.Name,
                         Duration = e.Name,
                         a.PaymentType,
                         //DueDate = (a.DueDate == 1 || a.DueDate == 21) ? (a.DueDate + "st") : ((a.DueDate == 2 || a.DueDate == 22) ? (a.DueDate + "nd") : ((a.DueDate == 3 || a.DueDate == 23) ? (a.DueDate + "rd") : (a.DueDate + "th"))),


                     }).OrderBy(a => a.Date);

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

            //return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        #endregion

        #region Rental Invoice
        public ActionResult RentalInvoice()
        {
            var OpAll = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);
            ViewBag.Alldata = OpAll;
            return View();
        }

        [HttpPost]
        public ActionResult GetRentalInvoice(string Voucher, long? Tenant, long? Unit, long? Property, string fromdate, string todate)
        {

            var UserId = User.Identity.GetUserId();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
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

            var v = (from a in db.Rentals
                     join b in db.Tenants on a.Tenant equals b.TenantID into cust
                     from b in cust.DefaultIfEmpty()
                     join d in db.PropertyUnits on a.Unit equals d.Id into emp
                     from d in emp.DefaultIfEmpty()
                     join e in db.PropertyMains on a.Property equals e.Id into mcs
                     from e in mcs.DefaultIfEmpty()
                     where (Voucher == "" || a.VoucherNo == Voucher)
                     && (Tenant == 0 || Tenant == null || a.Tenant == Tenant)
                     && (Unit == 0 || Unit == null || a.Unit == Unit)
                     && (Property == 0 || Property == null || a.Property == Property)
                         && (fromdate == "" || EF.Functions.DateDiffDay(a.RDate, fdate) <= 0)
                       && (todate == "" || EF.Functions.DateDiffDay(a.RDate, tdate) >= 0)
                     select new
                     {
                         Id = a.RentalID,
                         Voucher = a.VoucherNo,
                         Date = a.RDate,
                         Tenant = b.TenantName,
                         Unit = d.Name,
                         Property = e.Name,
                         Amount = a.Amount
                     }).OrderBy(a => a.Date);

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

            //return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        #endregion

        #region Maintanance
        public ActionResult Maintance()
        {
            var OpAll = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);
            ViewBag.Alldata = OpAll;
            return View();
        }

        [HttpPost]
        public ActionResult GetMaintance(string Voucher, long? Contractor, long? Property, string fromdate, string todate)
        {

            var UserId = User.Identity.GetUserId();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
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

            var v = (from a in db.Maintenances
                         //join b in db.Developers on a.Developer equals b.DeveloperID into cust
                         //from b in cust.DefaultIfEmpty()
                         //join c in db.Accountss on a.Owner equals c.AccountsID into pay
                         //from c in pay.DefaultIfEmpty()
                     join d in db.Contractors on a.Contractor equals d.ContractorID into emp
                     from d in emp.DefaultIfEmpty()
                     join e in db.PropertyMains on a.Property equals e.Id into mcs
                     from e in mcs.DefaultIfEmpty()
                     join f in db.ContractTypes on a.ContractType equals f.ID into ct
                     from f in ct.DefaultIfEmpty()
                     where (Voucher == "" || a.VoucherNo == Voucher)
                     //&& (Developer == 0 || a.Developer == Developer)
                     //&& (Owner == 0 || a.Owner == Owner)
                     && (Contractor == 0 || Contractor == null || a.Contractor == Contractor)
                     && (Property == 0 || Property == null || a.Property == Property)
                     && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
                     && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
                     select new
                     {
                         Id = a.ID,
                         Voucher = a.VoucherNo,
                         Date = a.Date,
                         Contractor = d.ContractorName,
                         // Owner = c.Name,
                         // Broker = d.BrokerName,
                         Property = e.Name,
                         a.EndDate,
                         a.StartDate,
                         ContractType=f.Name,
                         a.Amount
                     }).OrderBy(a => a.Date);

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

            //return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        #endregion

        #region payment
        [QkAuthorize(Roles = "Dev,Report Payment")]
        public ActionResult Payment()
        {
            var Paidfrom = db.Accountss.Where(p => p.Group == 9 || p.Group == 8).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.Paidfrom = QkSelect.List(Paidfrom, "ID", "Name");

            ViewBag.PaidTo = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                           }, "Value", "Text", 1);


            companySet();
            return View();
        }

        [QkAuthorize(Roles = "Dev,Report Payment")]
        public ActionResult GetPayment(string vno, long? payfrom, long? payto, string fromdate, string todate, int[] MOPay, long? Property, long? Unit)
        {
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
            List<ModeOfPayment> Mop = new List<ModeOfPayment>();
            if (MOPay != null && MOPay.Contains(1))
            {
                Mop.Add(ModeOfPayment.Cash);
            }
            if (MOPay != null && MOPay.Contains(2))
            {
                Mop.Add(ModeOfPayment.PDC);
            }
            if (MOPay != null && MOPay.Contains(3))
            {
                Mop.Add(ModeOfPayment.CDC);
            }
            var count = Mop.Count();
            var v = (from a in db.Payments
                     join b in db.Accountss on a.PayFrom equals b.AccountsID into payf
                     from b in payf.DefaultIfEmpty()
                     join c in db.Accountss on a.PayTo equals c.AccountsID into payt
                     from c in payt.DefaultIfEmpty()
                     join d in db.PropertyMains on a.Project equals d.Id into prop
                     from d in prop.DefaultIfEmpty()
                     join e in db.PropertyUnits on a.ProTask equals e.Id into uni
                     from e in uni.DefaultIfEmpty()
                     where (vno == "" || a.VoucherNo == vno) &&
                    (payfrom == 0 || payfrom == null || a.PayFrom == payfrom) &&
                    (payto == 0 || a.PayTo == payto) &&
                    (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                    (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                    a.editable == choice.Yes
                    && ((count == 0) || (Mop.Contains(a.MOPayment)))
                    && ((Property == 0) || (Property == null) || (Property == a.Project))
                    && ((Unit == 0) || (Unit == null) || (Unit == a.ProTask))
                     select new
                     {
                         VoucherNo = a.VoucherNo,
                         Payer = b.Name,
                         Reciever = c.Name,
                         a.PaymentId,
                         a.Date,
                         a.MOPayment,
                         a.PDCDate,
                         a.PayFrom,
                         a.PayTo,
                         a.TaxAmount,
                         a.SubTotal,
                         a.GrandTotal,
                         a.Paying,
                         a.CreatedDate,
                         a.Discount,
                         a.Remark,
                         Property = d.Name,
                         Unit = e.Name
                     }).AsEnumerable().Select(o => new
                     {
                         o.VoucherNo,
                         o.Payer,
                         o.Reciever,
                         o.PaymentId,
                         o.Date,
                         modeofpay = Enum.GetName(typeof(ModeOfPayment), o.MOPayment),
                         o.MOPayment,
                         o.PDCDate,
                         o.PayFrom,
                         o.PayTo,
                         o.TaxAmount,
                         o.SubTotal,
                         o.GrandTotal,
                         o.Paying,
                         o.CreatedDate,
                         o.Remark,
                         o.Discount,
                         o.Property,
                         o.Unit
                     }).OrderBy(a => a.Date);

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

            //return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }


        #endregion

        #region receipt
        [QkAuthorize(Roles = "Dev,Report Receipt")]
        public ActionResult Receipt()
        {

            var PaidTo = db.Accountss.Where(p => p.Group == 9 || p.Group == 8).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.PaidTo = QkSelect.List(PaidTo, "ID", "Name");

            ViewBag.Paidfrom = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);
            companySet();
            return View();
        }
        [QkAuthorize(Roles = "Dev,Report Receipt")]
        public ActionResult GetReceipt(string vno, long? payfrom, long? payto, string fromdate, string todate, int[] MOPay, long? Property, long? Unit)
        {
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
            List<ModeOfPayment> Mop = new List<ModeOfPayment>();
            if (MOPay != null && MOPay.Contains(1))
            {
                Mop.Add(ModeOfPayment.Cash);
            }
            if (MOPay != null && MOPay.Contains(2))
            {
                Mop.Add(ModeOfPayment.PDC);
            }
            if (MOPay != null && MOPay.Contains(3))
            {
                Mop.Add(ModeOfPayment.CDC);
            }
            var count = Mop.Count();
            var v = (from a in db.Receipts
                     join b in db.Accountss on a.PayFrom equals b.AccountsID into payf
                     from b in payf.DefaultIfEmpty()
                     join c in db.Accountss on a.PayTo equals c.AccountsID into payt
                     from c in payt.DefaultIfEmpty()
                     join d in db.PropertyMains on a.Project equals d.Id into prop
                     from d in prop.DefaultIfEmpty()
                     join e in db.PropertyUnits on a.ProTask equals e.Id into uni
                     from e in uni.DefaultIfEmpty()
                     where (vno == "" || a.VoucherNo == vno) &&
                      (payfrom == 0 || a.PayFrom == payfrom) &&
                      (payto == 0 || payto == null || a.PayTo == payto) &&
                     (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                     (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                     a.editable == choice.Yes
                     && ((count == 0) || (Mop.Contains(a.MOPayment)))
                      && ((Property == 0) || (Property == null) || (Property == a.Project))
                    && ((Unit == 0) || (Unit == null) || (Unit == a.ProTask))
                     select new
                     {
                         VoucherNo = a.VoucherNo,
                         Payer = b.Name,
                         Reciever = c.Name,
                         a.ReceiptId,
                         a.Date,
                         a.MOPayment,
                         a.PDCDate,
                         a.PayFrom,
                         a.PayTo,
                         a.SubTotal,
                         a.GrandTotal,
                         a.Paying,
                         a.CreatedDate,
                         a.Remark,
                         a.Discount,
                         Property = d.Name,
                         Unit = e.Name
                     }).AsEnumerable().Select(o => new
                     {
                         o.VoucherNo,
                         o.Payer,
                         o.Reciever,
                         o.ReceiptId,
                         o.Date,
                         modeofpay = Enum.GetName(typeof(ModeOfPayment), o.MOPayment),
                         o.MOPayment,
                         o.PDCDate,
                         o.PayFrom,
                         o.PayTo,
                         o.SubTotal,
                         o.GrandTotal,
                         o.Paying,
                         o.CreatedDate,
                         o.Remark,
                         o.Discount,
                         o.Property,
                         o.Unit
                     }).OrderBy(a => a.Date);

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
            //return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        #endregion

        #region Journal
        [QkAuthorize(Roles = "Dev,Report Journal")]
        public ActionResult Journal()
        {

            var list = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            ViewBag.list = list;
            companySet();
            return View();
        }

        [QkAuthorize(Roles = "Dev,Report Journal")]
        public ActionResult GetJournal(string vno, long? payfrom, long? payto, string fromdate, string todate)
        {
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
            var v = (from a in db.Journals
                     join b in db.Accountss on a.PayFrom equals b.AccountsID into payf
                     from b in payf.DefaultIfEmpty()
                     join c in db.Accountss on a.PayTo equals c.AccountsID into payt
                     from c in payt.DefaultIfEmpty()
                     where (vno == "" || a.VoucherNo == vno) &&
                    (payfrom == 0 || payfrom == null || a.PayFrom == payfrom) &&
                    (payto == 0 || payto == null || a.PayTo == payto) &&
                    (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                    (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                    a.editable == choice.Yes
                     select new
                     {
                         VoucherNo = a.VoucherNo,
                         Payer = b.Name,
                         Reciever = c.Name,
                         a.JournalId,
                         a.Date,
                         a.PayFrom,
                         a.PayTo,
                         a.SubTotal,
                         a.GrandTotal,
                         a.Paying,
                         a.CreatedDate,
                     }).AsEnumerable().Select(o => new
                     {
                         o.VoucherNo,
                         o.Payer,
                         o.Reciever,
                         o.JournalId,
                         o.Date,
                         o.PayFrom,
                         o.PayTo,
                         o.SubTotal,
                         o.GrandTotal,
                         o.Paying,
                         o.CreatedDate,
                     }).OrderBy(a => a.Date);

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
            //return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }


        #endregion

        #region Empty Units
        public ActionResult EmptyUnits()
        {
            var OpAll = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);
            ViewBag.Alldata = OpAll;
            return View();
        }

        [HttpPost]
        public ActionResult GetEmptyUnits(long? landlords, long? Property)
        {
            var UserId = User.Identity.GetUserId();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var units = db.PropertyUnits.Select(x => x.Id).ToList();
            var allocated = db.TenancyContracts.Select(x => x.Unit).ToList();
            var emptyunits = units.Except(allocated).ToList();
            var v = (from a in db.PropertyUnits
                     join d in db.PropertyMains on a.Property equals d.Id into emp
                     from d in emp.DefaultIfEmpty()
                     join e in db.PropertyUnitTypes on a.UnitType equals e.ID into mcs
                     from e in mcs.DefaultIfEmpty()
                     where (emptyunits.Contains(a.Id)) &&
                     (Property == 0 || Property == null || a.Property == Property)
                     //&& (landlords == 0 || landlords == null || a.landlords == Property)
                     select new
                     {
                         Id = a.Id,
                         Date = a.CreatedDate,
                         //Property = d.Name,
                         Name = a.Name,
                         UnitType = e.Name,
                         a.Rent,
                         a.Deposit,
                     }).OrderBy(a => a.Date);

            recordsTotal = v.Count();
            // EF Core 10 cannot translate the nested Features collection-projection; attach it in memory after paging.
            var page = v.Skip(skip).Take(pageSize).ToList();
            var unitIds = page.Select(r => r.Id).ToList();
            var feats = db.SelectedUnitFeatures.Where(f => unitIds.Contains(f.Unit)).Select(f => new { f.Unit, f.Feature }).ToList();
            var data = page.Select(r => new {
                r.Id, r.Date, r.Name, r.UnitType, r.Rent, r.Deposit,
                Features = feats.Where(f => f.Unit == r.Id).Select(f => new { f.Feature }).ToList()
            }).ToList();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

            //return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        #endregion

        #region Expense Report
        public ActionResult Expense()
        {
            var OpAll = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);
            ViewBag.Alldata = OpAll;
            return View();
        }
        [HttpPost]
        public ActionResult GetExpense(long? landlord, long? Property, string fromdate, string todate)
        {
            var UserId = User.Identity.GetUserId();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
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
            var v = (from b in db.PropertyMains
                     join c in db.Payments on b.Id equals c.Project
                     where (Property == 0 || Property == null || b.Id == Property)// && (landlord == null || a.landlord == landlord)
                     select new
                     {
                         ID = b.Id,
                         Name = b.Name,
                         c.Paying,
                         c.Remark,
                         //expense1 = (decimal?)(from ac in db.Maintenances
                         //                      where
                         //                      (ac.Property == b.Id) && (todate == "" || EF.Functions.DateDiffDay(ac.Date, tdate) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.Date, fdate) <= 0)
                         //                      select new
                         //                      {
                         //                          ac.Amount,
                         //                      }).ToList().Sum(x => x.Amount),
                         b.Code,
                     }).Select(o => new
                     {
                         Id = o.ID,
                         Name = o.Name,
                         Expense = o.Paying,
                         code = o.Code,
                         Remar=o.Remark
                     }).Distinct().ToList();


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

            //return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        #endregion

        #region Income Report
        public ActionResult Income()
        {
            var OpAll = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);
            ViewBag.Alldata = OpAll;
            return View();
        }
        [HttpPost]
        public ActionResult GetIncome(long? landlord, long? Property, string fromdate, string todate)
        {
            var UserId = User.Identity.GetUserId();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
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
            var v = (from a in db.PropertyMains
                     join b in db.Receipts on a.Id equals b.Project
                     where (Property == 0 || Property == null || a.Id == Property)// && (landlord == null || a.landlord == landlord)
                     select new
                     {
                         ID = a.Id,
                         Name = a.Name,
                         //income1 = (decimal?)(from ac in db.Rentals
                         //                     where
                         //                     (ac.Property == a.Id) && (todate == "" || EF.Functions.DateDiffDay(ac.RDate, tdate) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.RDate, fdate) <= 0)
                         //                     select new
                         //                     {
                         //                         ac.Amount,
                         //                     }).ToList().Sum(x => x.Amount)??0,
                         //income2 = (decimal?)(from ac in db.TenancyContracts
                         //                     where
                         //                     (ac.Property == a.Id) && (todate == "" || EF.Functions.DateDiffDay(ac.CreatedDate, tdate) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.CreatedDate, fdate) <= 0)
                         //                     select new
                         //                     {
                         //                         ac.Rent,
                         //                     }).ToList().Sum(x => x.Rent)??0,
                         //income3 = (decimal?)(from ac in db.TenancyContracts
                         //                     where
                         //                     (ac.Property == a.Id) && (todate == "" || EF.Functions.DateDiffDay(ac.CreatedDate, tdate) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.CreatedDate, fdate) <= 0)
                         //                     select new
                         //                     {
                         //                         ac.Deposit,
                         //                     }).ToList().Sum(x => x.Deposit)??0,
                         //income = (decimal?)(from ac in db.Receipts
                         //                    where
                         //                    (ac.Project == a.Id) && (todate == "" || EF.Functions.DateDiffDay(ac.CreatedDate, tdate) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.CreatedDate, fdate) <= 0)
                         //                    select new
                         //                    {
                         //                        ac.Paying,
                         //                    }).ToList().Sum(x => x.Paying) ?? 0,
                         //remark = (from ac in db.Receipts
                         //          where
                         //          (ac.Project == a.Id) && (todate == "" || EF.Functions.DateDiffDay(ac.CreatedDate, tdate) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.CreatedDate, fdate) <= 0)
                         //          select new
                         //          {
                         //              ac.Remark
                         //          }).FirstOrDefault(),
                         a.Code,
                         b.Paying,
                         b.Remark
                     }
                        ).Select(o=> new
                     {
                         Id = o.ID,
                         Name = o.Name,
                         Income = o.Paying,
                         purpose=o.Remark,
                         code = o.Code,
                     }).Distinct().ToList();


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

            //return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        #endregion

        #region rate of return
        public ActionResult rateofreturn()
        {
            var OpAll = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);
            ViewBag.Alldata = OpAll;
            return View();
        }
        [HttpPost]
        public ActionResult Getrateofreturn(long? landlord, long? Property, string fromdate, string todate)
        {
            var UserId = User.Identity.GetUserId();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            // dc.Configuration.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
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
            var v = (from a in db.Maintenances
                     join b in db.PropertyMains on a.Property equals b.Id into cust
                     from b in cust.DefaultIfEmpty()
                     where (Property == 0 || Property == null || a.Property == Property)// && (landlord == null || a.landlord == landlord)
                     select new
                     {
                         ID = a.ID,
                         Name = b.Name,
                         income1 = (decimal?)(from ac in db.Rentals
                                              where
                                              (ac.Property == a.Property) && (todate == "" || EF.Functions.DateDiffDay(ac.RDate, tdate) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.RDate, fdate) <= 0)
                                              select new
                                              {
                                                  ac.Amount,
                                              }).Sum(x => (decimal?)x.Amount) ?? 0,
                         income2 = (decimal?)(from ac in db.TenancyContracts
                                              where
                                              (ac.Property == a.Property) && (todate == "" || EF.Functions.DateDiffDay(ac.CreatedDate, tdate) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.CreatedDate, fdate) <= 0)
                                              select new
                                              {
                                                  ac.Rent,
                                              }).Sum(x => (decimal?)x.Rent) ?? 0,
                         income3 = (decimal?)(from ac in db.TenancyContracts
                                              where
                                              (ac.Property == a.Property) && (todate == "" || EF.Functions.DateDiffDay(ac.CreatedDate, tdate) >= 0) && (fromdate == "" || EF.Functions.DateDiffDay(ac.CreatedDate, fdate) <= 0)
                                              select new
                                              {
                                                  ac.Deposit,
                                              }).Sum(x => (decimal?)x.Deposit) ?? 0,
                         b.Code,
                     }).Select(o => new
                     {
                         Id = o.ID,
                         Name = o.Name,
                         Income = o.income1 + o.income2 + o.income3,
                         code = o.Code,
                     }).Distinct().ToList();


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

            //return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        #endregion

        #region document expiry
        public ActionResult documentexpiry()
        {
            ViewBag.Sect = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value=""},
                new SelectListItem() {Text = "Broker", Value="Broker"},
                new SelectListItem() {Text = "Contractor", Value="Contractor"},
                new SelectListItem() {Text = "Developer", Value="Developer"},
                new SelectListItem() {Text = "Landlords", Value="Landlord"},
                new SelectListItem() {Text = "Property", Value="Property"},
                new SelectListItem() {Text = "Tenant", Value="Tenant"},
                new SelectListItem() {Text = "Tenancy Contract", Value="TenancyContract"},
                new SelectListItem() {Text = "Unit", Value="PropertyUnit"},
            }, "Value", "Text");
            return View();
        }
        [HttpPost]
        public ActionResult Getdocumentexpiry(string section, string date)
        {
            var UserId = User.Identity.GetUserId();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            if (date != "")
            {
                fdate = DateTime.Parse(date, new CultureInfo("en-GB"));
            }
            CultureInfo culture = new CultureInfo("en-US");
            var v = (from a in db.PropertyDocumentTypes
                     join b in db.DocumentTypes on a.DocumentType equals b.ID into cust
                     from b in cust.DefaultIfEmpty()
                     join c in db.Landlords on new { c1 = a.Reference, c2 = a.Purpose }
                     equals new { c1 = c.LandlordID, c2 = "Landlord" } into land
                     from c in land.DefaultIfEmpty()
                     join d in db.PropertyMains on new { d1 = a.Reference, d2 = a.Purpose }
                     equals new { d1 = d.Id, d2 = "Property" } into pro
                     from d in pro.DefaultIfEmpty()
                     join e in db.Brokers on new { e1 = a.Reference, e2 = a.Purpose }
                     equals new { e1 = e.BrokerID, e2 = "Broker" } into bro
                     from e in bro.DefaultIfEmpty()
                     join f in db.Developers on new { f1 = a.Reference, f2 = a.Purpose }
                     equals new { f1 = f.DeveloperID, f2 = "Developer" } into dev
                     from f in dev.DefaultIfEmpty()
                     join g in db.Contractors on new { g1 = a.Reference, g2 = a.Purpose }
                     equals new { g1 = g.ContractorID, g2 = "Contractor" } into con
                     from g in con.DefaultIfEmpty()
                     join h in db.Tenants on new { h1 = a.Reference, h2 = a.Purpose }
                     equals new { h1 = h.TenantID, h2 = "Tenant" } into tenan
                     from h in tenan.DefaultIfEmpty()
                     join i in db.PropertyUnits on new { i1 = a.Reference, i2 = a.Purpose }
                     equals new { i1 = i.Id, i2 = "PropertyUnit" } into unit
                     from i in unit.DefaultIfEmpty()
                     join j in db.TenancyContracts on new { j1 = a.Reference, j2 = a.Purpose }
                     equals new { j1 = j.Id, j2 = "TenancyContract" } into Tencntrct
                     from j in Tencntrct.DefaultIfEmpty()
                     where (section == "" || a.Purpose == section)
                     && (date == "" || EF.Functions.DateDiffDay(a.ExpDate, fdate) >= 0)
                     select new
                     {
                         Id = a.ID,
                         Date = a.ExpDate,
                         Name = (section == "Landlord") ? c.LandlordName :
                                ((section == "Property") ? d.Name :
                                ((section == "Broker") ? e.BrokerName :
                                ((section == "Developer") ? f.DeveloperName :
                                ((section == "Contractor") ? g.ContractorName :
                                ((section == "Tenant") ? h.TenantName :
                                ((section == "PropertyUnit") ? i.Name :
                                ((section == "TenancyContract") ? "Tenancy Contract" :
                                ((section == "") ?
                                ((a.Purpose == "Landlord") ? c.LandlordName :
                                ((a.Purpose == "Property") ? d.Name :
                                ((a.Purpose == "Broker") ? e.BrokerName :
                                ((a.Purpose == "Developer") ? f.DeveloperName :
                                ((a.Purpose == "Contractor") ? g.ContractorName :
                                ((a.Purpose == "Tenant") ? h.TenantName :
                                ((a.Purpose == "PropertyUnit") ? i.Name :
                                ((a.Purpose == "TenancyContract") ? "Tenancy Contract" :
                                (""))))))))) : "")))))))),
                         DocumentType = b.Name,
                         Code = ((section == "TenancyContract") ? j.Code : ((section == "PropertyUnit") ? i.Code : ((section == "Property") ? d.Code : ""))),
                     }).OrderBy(a => a.Date);

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

            //return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        #endregion
    }
}