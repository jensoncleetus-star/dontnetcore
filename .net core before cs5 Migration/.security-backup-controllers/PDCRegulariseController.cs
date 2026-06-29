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
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.ViewModel;
using Microsoft.AspNetCore.Identity;
using System.Net;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class PDCRegulariseController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PDCRegulariseController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Item Status")]
        public ActionResult ChangeStatus(string type, long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PDC itm = db.PDCs.Find(id);
            if (itm == null)
            {
                return NotFound();
            }
            if (type == "UNHOLD")
            {
                ViewBag.type = "UNHOLD";
                ViewBag.link = "UNHOLD";
                ViewBag.status = Status.active;
            }
            else
            {
                ViewBag.type = "HOLD";
                ViewBag.link = "HOLD";
                ViewBag.status = Status.inactive;
            }
            return PartialView();
        }
        [HttpPost]
        // [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Item Status")]
        public JsonResult ChangeStatus(string type, long? id, Item item)
        {
            bool stat = false;
            string msg;
            string types = "";
            PDC itm = db.PDCs.Find(id);
            if (type == "HOLD")
            {
                types = " Inactive";
                itm.withhold = null;
            }
            else
            {
                types = " Active";
                itm.withhold = 1;
            }
          


            db.Entry(itm).State = EntityState.Modified;
            db.SaveChanges();

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Changed, UserId, "status", "PDCRegularise", findip(), item.ItemID, "Successfully Changed the Item to" + types);


            stat = true;
            msg = " Successfully Changed the Item to" + types;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        // GET: PDCRegularise
        [QkAuthorize(Roles = "Dev,PDC Regularise")]
        public ActionResult Index()
        {
            var supparentid = new SqlParameter("@parentid", 8);
            var supgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", supparentid).AsEnumerable().ToList();
            var supgpid = supgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            var arr = supgpid.ToArray();
            /*  var bank = db.Accountss.Where(p => p.Group == 8 && arr.Contains(p.Group)).Select(r => new
              {
                  ID = r.AccountsID,
                  Name = r.Name
              }).Distinct().ToList();
            */
            // bank filter = the DISTINCT non-empty bank names actually stored on the cheques (the old Bank-name-vs-BankId join never matched)
            var bank = db.PDCs.Where(b => b.Bank != null && b.Bank != "").Select(b => b.Bank).Distinct().OrderBy(x => x)
                        .Select(x => new { ID = x, Name = x }).ToList();


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
        //Get: View of Adding Remarks

        [HttpPost]
        public ActionResult GetAllRemarks(long? CustomerId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();

            var v = (from a in db.RemarkCheque
                     join b in db.Users on a.createdby equals b.Id into emp
                     from b in emp.DefaultIfEmpty()
                     where a.pdcid == CustomerId 
                     orderby a.CreatedDate descending
                     select new
                     {
                         id = a.RemarkId,
                         a.CreatedDate,
                         a.status,
                         EmpName = b.UserName,
                         a.Remark,
                     });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            recordsTotal = v.Count();
            var data = v.ToList();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        public JsonResult SearchStatus(string q, string x, string page)
        {

            var UserId = User.Identity.GetUserId();

            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectFormat2> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.chequeStatuses
                                  

                                  where 
                                  
                                  b.ChequeStatusName.ToLower().Contains(q.ToLower())

                                  select new SelectFormat2
                                  {
                                      text = b.ChequeStatusName,
                                      id = b.ChequeStatusName
                                  }).Distinct().OrderByDescending(a => a.id).Take(pageSize).ToList();

            }
            else
            {
                serialisedJson = (from b in db.chequeStatuses
                                  
                              
                                  select new SelectFormat2
                                  {
                                      text = b.ChequeStatusName,
                                      id = b.ChequeStatusName
                                  }).Distinct().OrderByDescending(a => a.id).Take(pageSize).ToList();
                //serialisedJson = db.Customers.Select(b => new SelectFormat
                //    text = b.CustomerCode + "-" + b.CustomerName,
                //    id = b.CustomerID

            }//
            if (x == "All" || x == "Both" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat2() { id = "0", text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (string.IsNullOrEmpty(q) && (x == "No" || (x == "Both" && start == 0)))
            {
                var initial = new SelectFormat2() { id = "", text = "--No Data--" };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);

        }

        public ActionResult addpdcremark(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PDC cus = db.PDCs.Find(id);
            var UserId = User.Identity.GetUserId();
            var loc = db.chequeStatuses
                .Select(s => new
                {
                    ID = s.ChequeStatusName,
                    Name = s.ChequeStatusName
                }).Distinct().OrderBy(o => o.Name).ToList();
            ViewBag.statusname = QkSelect.List(loc, "ID", "Name");
            if (cus == null)
            {
                return NotFound();
            }
            var pdcremark = new RemarkCheque
            {
                pdcid = cus.PDCid,
                createdby=UserId,
                 
             
         

            };

            return PartialView(pdcremark);
        }

        //Saving of Remarks
        [HttpPost]
        public JsonResult addpdcremark(RemarkCheque CusRemark)
        {
            Int64 CustomerId = CusRemark.pdcid;

            if (ModelState.IsValid)
            {
             
                if (CusRemark.status != null )
                {
                    Common com = new Common();

                    RemarkCheque Obj = new RemarkCheque
                    {
                        pdcid = CusRemark.pdcid,
                        Remark = CusRemark.Remark,
                        createdby = CusRemark.createdby,
                        status=CusRemark.status,
                        CreatedDate=System.DateTime.Now,
                    };
                    db.RemarkCheque.Add(Obj);
                    db.SaveChanges();

                    com.addlog(LogTypes.Created, CusRemark.createdby, "chequeprint", "remarkaddedchequeprint", findip(), CustomerId, "Remarks Added Successfully..");
                    Success("Remark added successfully...", true);
                }
                else
                {
                    Danger("Failed to add Remarks...,please login again", true);
                }
            }
            else
            {
                Danger("Failed to add Remarks...,please login again", true);
            }

            return Json(new { msg = "Success", status = true });

        }

        [HttpPost]
        // datatable fields listing
        public JsonResult GetData2(bool withhold,string fromdate, string todate, long? AccId, string Type, long? customer, long? supplier, string CheckNumber, long? tenant, long? contractor, string bankid)
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            DateTime? fdate = null;
            DateTime? tdate = null;
            var BusinessType = db.EnableSettings.Where(c => c.EnableType == "BusinessType").Select(a => a.TypeValue).SingleOrDefault();

            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }

            long paybankid = db.Accountss.Where(o => o.Name.Contains(bankid.Trim())).Select(o => o.AccountsID).FirstOrDefault();
            var v = (from a in db.PDCs
                     join b in db.Payments on new { f1 = a.Reference, f2 = a.PDCType } equals new { f1 = b.PaymentId, f2 = "Payment" } into pay
                     from b in pay.DefaultIfEmpty()
                     join c in db.Receipts on new { f1 = a.Reference, f2 = a.PDCType } equals new { f1 = c.ReceiptId, f2 = "Receipt" } into rec
                     from c in rec.DefaultIfEmpty()
                     join d in db.Accountss on b.PayTo equals d.AccountsID into payto
                     from d in payto.DefaultIfEmpty()
                     join e in db.Accountss on c.PayFrom equals e.AccountsID into payfrom
                     from e in payfrom.DefaultIfEmpty()
                     join f in db.Users on a.CreatedBy equals f.Id into user
                     from f in user.DefaultIfEmpty()
                     join g in db.Branchs on a.Branch equals g.BranchID into branch
                     from g in branch.DefaultIfEmpty()
                     join h in db.Journals on new { f1 = a.Reference, f2 = a.PDCType } equals new { f1 = h.JournalId, f2 = "Journal" } into jnl
                     from h in jnl.DefaultIfEmpty()
                     join i in db.Accountss on b.PayFrom equals i.AccountsID into payfromnew
                     from i in payfromnew.DefaultIfEmpty()
                     let bank = (from x in db.AccountsTransactions
                                 join y in db.Banks on x.Account equals y.AccountId
                                 join z in db.Accountss on y.AccountId equals z.AccountsID
                                 where x.reference == a.Reference
                                 select new
                                 {
                                     z.Name,
                                     y.BankId,
                                     x.reference

                                 }).FirstOrDefault()



                     where (((Type == "all") && (a.PDCType != "TenancyContract") && (a.PDCType != "Maintenance")) ||
                           (Type == "Payments" && a.PDCType == "Payment") || (Type == "Reciepts" && a.PDCType == "Receipt") || (Type == "Journals" && a.PDCType == "Journal")) &&
                           (AccId == null || b.PayFrom == AccId || c.PayTo == AccId) &&
                           (a.RegStatus == choice.No) &&
                           (fromdate == "" || EF.Functions.DateDiffDay(a.PDCDate, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(a.PDCDate, tdate) >= 0) &&
                           (CheckNumber == "" || a.CheckNo == CheckNumber)
                           && (bankid == "" || b.PayFrom == paybankid)
                           && (a.Type != 1) &&
                          (withhold==true||a.withhold==null)
                     select new PDCViewModel
                     {
                         id = a.PDCid,
                         Date = a.PDCDate,
                         bankname = (bank == null) ? "" : i.Name,
                         Voucher = (a.PDCType == "Payment") ? b.VoucherNo : ((a.PDCType == "Receipt") ? c.VoucherNo : h.VoucherNo), //(b.VoucherNo != null) ? b.VoucherNo : c.VoucherNo,
                         Account = (d.Name != null) ? d.Name : e.Name,
                         Issued = (decimal?)b.GrandTotal,
                         Receipt = (decimal?)c.Paying,
                         Journal = (decimal?)h.Paying,
                         check = a.CheckNo,
                         remark = a.Note,
                         CreatedBy = f.UserName,
                         CreatedDate = a.CreatedDate,
                         Branch = g.BranchName,
                         pdctype = a.PDCType,
                       
                         withhold=a.withhold,
                         voucherid = (a.PDCType == "Payment") ? b.PaymentId : ((a.PDCType == "Receipt") ? c.ReceiptId : h.JournalId),
                     }); 


            if (BusinessType == "Propertyy")
            {
                var prov = (from a in db.PDCs
                            join z in db.PropertyRegistrations on new { f1 = a.Reference, f2 = a.PDCType, f3 = a.CreatedDate } equals new { f1 = z.RegistrationID, f2 = "PropertyRegistrations", f3 = z.CreatedDate } into payy
                            from z in payy.DefaultIfEmpty()
                            join z2 in db.PropertyMains on z.Property equals z2.Id into z2z2
                            from z2 in z2z2.DefaultIfEmpty()
                            join b in db.TenancyContracts on new { f1 = a.Reference } equals new { f1 = b.Id } into pay
                            from b in pay.DefaultIfEmpty()
                            join c in db.Maintenances on new { f1 = a.Reference, f2 = a.PDCType } equals new { f1 = c.ID, f2 = "Maintenance" } into rec
                            from c in rec.DefaultIfEmpty()
                            join d in db.Tenants on b.Tenant equals d.TenantID into ten
                            from d in ten.DefaultIfEmpty()
                            join e in db.Contractors on c.Contractor equals e.ContractorID into cont
                            from e in cont.DefaultIfEmpty()
                            join f in db.Users on a.CreatedBy equals f.Id into user
                            from f in user.DefaultIfEmpty()
                            join g in db.Branchs on a.Branch equals g.BranchID into branch
                            from g in branch.DefaultIfEmpty()
                            join h in db.Cheques on new { f1 = a.Reference, f3 = a.PDCDate, f2 = a.CheckNo } equals new { f1 = h.Reference, f3 = h.Date, f2 = h.ChequeNo } into cheqTC
                            from h in cheqTC.DefaultIfEmpty()


                            where (((Type == "all") && (a.PDCType != "Payments") && (a.PDCType != "Payment") && (a.PDCType != "Reciepts") && (a.PDCType != "Reciept")) && (a.PDCType != "Journal") || (Type == "TenancyContract" && a.PDCType == "TenancyContract") || (Type == "Maintenance" && a.PDCType == "Maintenance") || (Type == "PropertyRegistrations" && a.PDCType == "PropertyRegistrations")
                            ) &&
                                  (a.RegStatus == choice.No) &&
                                  (fromdate == "" || EF.Functions.DateDiffDay(a.PDCDate, fdate) <= 0) &&
                                  (todate == "" || EF.Functions.DateDiffDay(a.PDCDate, tdate) >= 0) &&
                                  (CheckNumber == "" || a.CheckNo == CheckNumber)
                                  && (bankid == null || a.Bank == bankid.ToString())
                                  && (a.Type != 1) && (tenant == null || tenant == 0 || tenant == b.Tenant) && (contractor == null || contractor == 0 || contractor == c.Contractor)
                            select new PDCViewModel
                            {
                                id = a.PDCid,
                                Date = a.PDCDate,
                                Voucher = (a.PDCType == "TenancyContract") ? b.Code : ((a.PDCType == "Maintenance") ? c.VoucherNo : (a.PDCType == "PropertyRegistrations") ? z.VoucherNo : ""), //(b.VoucherNo != null) ? b.VoucherNo : c.VoucherNo,
                                Account = (a.PDCType.Contains("Tenancy")) ? d.TenantName : ((a.PDCType == "PropertyRegistrations") ? "PropertyRegistrations" : "Maintenance"),
                                Issued = h.Amount, //(a.PDCType == "TenancyContract") ? h.Amount : 0,
                                //((a.PDCType == "PropertyRegistrations") ? hh.Amount :i.Amount
                                // (a.PDCType == "TenancyContractDeposit")?j.Amount:
                                Receipt = 0,
                                Journal = 0,
                                check = a.CheckNo,
                                remark = a.Note,
                                CreatedBy = f.UserName,
                                CreatedDate = a.CreatedDate,
                                Branch = g.BranchName
                            });
                if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    prov = prov.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
                }
                recordsTotal = v.Count();

                var data2 = prov.Skip(skip).ToList();
                v = v.Union(prov);
            }

            //SORT

            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(
                    p => p.Account.ToString().ToLower().Contains(search.ToLower()) ||
                    p.bankname.ToString().ToLower().StartsWith(search.ToLower()) ||
                    p.Voucher.ToString().ToLower().EndsWith(search.ToLower()));
            }
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = v.Count();
            var data = v.Skip(skip).ToList();
            return Json(new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        [HttpPost]
        // datatable fields listing
        public JsonResult GetData3(string fromdate, string todate, long? AccId, string Type, long? customer, long? supplier, string CheckNumber, long? tenant, long? contractor, string bankid, string status)
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            DateTime? fdate = null;
            DateTime? tdate = null;
            var BusinessType = db.EnableSettings.Where(c => c.EnableType == "BusinessType").Select(a => a.TypeValue).SingleOrDefault();

            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }

            long accustomer = 0;
            if(customer!=0)
            accustomer = db.Customers.Find((long)customer).Accounts;
            long accsupplier = 0;
            if(supplier!=0)
            accsupplier = db.Suppliers.Find((long)supplier).Accounts;

            var v = (from a in db.PDCs
                   
                     join b in db.Payments on new { f1 = a.Reference, f2 = a.PDCType } equals new { f1 = b.PaymentId, f2 = "Payment" } into pay
                     from b in pay.DefaultIfEmpty()
                     join c in db.Receipts on new { f1 = a.Reference, f2 = a.PDCType } equals new { f1 = c.ReceiptId, f2 = "Receipt" } into rec
                     from c in rec.DefaultIfEmpty()
                     join d in db.Accountss on b.PayTo equals d.AccountsID into payto
                     from d in payto.DefaultIfEmpty()
                     join e in db.Accountss on c.PayFrom equals e.AccountsID into payfrom
                     from e in payfrom.DefaultIfEmpty()
                     join f in db.Users on a.CreatedBy equals f.Id into user
                     from f in user.DefaultIfEmpty()
                     join g in db.Branchs on a.Branch equals g.BranchID into branch
                     from g in branch.DefaultIfEmpty()
                     join h in db.Journals on new { f1 = a.Reference, f2 = a.PDCType } equals new { f1 = h.JournalId, f2 = "Journal" } into jnl
                     from h in jnl.DefaultIfEmpty()
                    let latstatus =  db.RemarkCheque.Where(o=>o.pdcid==a.PDCid).OrderByDescending(o=>o.CreatedDate).Select(o=>o.status).FirstOrDefault()
                     where 
                                  (a.RegStatus == choice.No) &&
                           (customer == 0 || customer == null || e.AccountsID == accustomer) &&
                           (supplier == 0 || supplier == null || d.AccountsID == accsupplier) &&
                                  (fromdate == "" || EF.Functions.DateDiffDay(a.PDCDate, fdate) <= 0) &&
                                  (todate == "" || EF.Functions.DateDiffDay(a.PDCDate, tdate) >= 0) &&
                                  (CheckNumber == "" || a.CheckNo == CheckNumber)
                                  && (bankid == "" || a.Bank == bankid.ToString())
                           && (a.Type != 1)

                     select new PDCViewModel
                     {
                         id = a.PDCid,
                         Date = a.PDCDate,
                         Voucher = (a.PDCType == "Payment") ? b.VoucherNo : ((a.PDCType == "Receipt") ? c.VoucherNo : h.VoucherNo), //(b.VoucherNo != null) ? b.VoucherNo : c.VoucherNo,
                         Account = (d.Name != null) ? d.Name : e.Name,
                         Issued = (decimal?)b.GrandTotal,
                         Receipt = (decimal?)c.Paying,
                         Journal = (decimal?)h.Paying,
                         check = a.CheckNo,
                         remark = latstatus,
                         CreatedBy = f.UserName,
                         CreatedDate = a.CreatedDate,
                         Branch = g.BranchName,
                         pdctype = a.PDCType,
                         voucherid = (a.PDCType == "Payment") ? b.PaymentId : ((a.PDCType == "Receipt") ? c.ReceiptId : h.JournalId),
                     });

            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.Account.ToString().ToLower().Contains(search.ToLower()) ||
                                p.remark.ToString().ToLower().Contains(search.ToLower())
                                 );

            }
            if(status!=""&& status != "0")
            {
                v = v.Where(p => p.remark == status);
            }
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = v.Count();
            var data = v.Skip(skip).ToList();
            return Json(new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        [QkAuthorize(Roles = "Dev,PDC Regularise")]
        public JsonResult GetData(string fromdate, string todate, long? AccId, string Type, long? customer, long? supplier, string CheckNumber,long? tenant, long? contractor,long? bankid)
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            long accustomer = db.Customers.Find((long)customer).Accounts;
            long accsupplier = db.Customers.Find((long)supplier).Accounts;

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            DateTime? fdate = null;
            DateTime? tdate = null;
            var BusinessType = db.EnableSettings.Where(c => c.EnableType == "BusinessType").Select(a => a.TypeValue).SingleOrDefault();

            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }

            var v = (from a in db.PDCs
                     join b in db.Payments on new { f1 = a.Reference, f2 = a.PDCType } equals new { f1 = b.PaymentId, f2 = "Payment" } into pay
                     from b in pay.DefaultIfEmpty()
                     join c in db.Receipts on new { f1 = a.Reference, f2 = a.PDCType } equals new { f1 = c.ReceiptId, f2 = "Receipt" } into rec
                     from c in rec.DefaultIfEmpty()
                     join d in db.Accountss on b.PayTo equals d.AccountsID into payto
                     from d in payto.DefaultIfEmpty()
                     join e in db.Accountss on c.PayFrom equals e.AccountsID into payfrom
                     from e in payfrom.DefaultIfEmpty()
                     join f in db.Users on a.CreatedBy equals f.Id into user
                     from f in user.DefaultIfEmpty()
                     join g in db.Branchs on a.Branch equals g.BranchID into branch
                     from g in branch.DefaultIfEmpty()
                     join h in db.Journals on new { f1 = a.Reference, f2 = a.PDCType } equals new { f1 = h.JournalId, f2 = "Journal" } into jnl
                     from h in jnl.DefaultIfEmpty()
                     where (((Type == "all") && (a.PDCType != "TenancyContract") && (a.PDCType != "Maintenance")) ||
                           (Type == "Payments" && a.PDCType == "Payment") || (Type == "Reciepts" && a.PDCType == "Receipt") || (Type == "Journals" && a.PDCType == "Journal")) &&
                           (AccId == null || b.PayFrom == AccId || c.PayTo == AccId) &&
                           (a.RegStatus == choice.No) &&
                           (customer==0||customer==null||e.AccountsID ==customer) &&
                           (supplier==0||supplier==null||d.AccountsID==supplier)&&
                           (fromdate == "" || EF.Functions.DateDiffDay(a.PDCDate, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(a.PDCDate, tdate) >= 0) &&
                           (CheckNumber == "" || a.CheckNo == CheckNumber)
                           && (bankid == null || a.Bank == bankid.ToString())
                           && (a.Type != 1)
                     select new PDCViewModel
                     {
                         id = a.PDCid,
                         Date = a.PDCDate,
                         Voucher = (a.PDCType == "Payment") ? b.VoucherNo : ((a.PDCType == "Receipt") ? c.VoucherNo : h.VoucherNo), //(b.VoucherNo != null) ? b.VoucherNo : c.VoucherNo,
                         Account = (d.Name != null) ? d.Name : e.Name,
                         Issued = (decimal?)b.GrandTotal,
                         Receipt = (decimal?)c.Paying,
                         Journal = (decimal?)h.Paying,
                         check = a.CheckNo,
                         remark = a.Note,
                         CreatedBy = f.UserName,
                         CreatedDate=a.CreatedDate,
                         Branch = g.BranchName
                     });


            if (BusinessType == "Property")
            {
                var prov = (from a in db.PDCs
                            join z in db.PropertyRegistrations on new { f1 = a.Reference, f2 = a.PDCType,f3=a.CreatedDate } equals new { f1 = z.RegistrationID, f2 = "PropertyRegistrations",f3=z.CreatedDate } into payy
                            from z in payy.DefaultIfEmpty()
                            join z2 in db.PropertyMains on z.Property equals z2.Id into z2z2
                            from z2 in z2z2.DefaultIfEmpty()
                            join b in db.TenancyContracts on new { f1 = a.Reference} equals new { f1 = b.Id} into pay
                            from b in pay.DefaultIfEmpty()
                            join c in db.Maintenances on new { f1 = a.Reference, f2 = a.PDCType } equals new { f1 = c.ID, f2 = "Maintenance" } into rec
                            from c in rec.DefaultIfEmpty()
                            join d in db.Tenants on b.Tenant equals d.TenantID into ten
                            from d in ten.DefaultIfEmpty()
                            join e in db.Contractors on c.Contractor equals e.ContractorID into cont
                            from e in cont.DefaultIfEmpty()
                            join f in db.Users on a.CreatedBy equals f.Id into user
                            from f in user.DefaultIfEmpty()
                            join g in db.Branchs on a.Branch equals g.BranchID into branch
                            from g in branch.DefaultIfEmpty()
                            join h in db.Cheques on new { f1 = a.Reference ,f3=a.PDCDate,f2=a.CheckNo} equals new { f1 = h.Reference, f3=h.Date,f2=h.ChequeNo } into cheqTC
                            from h in cheqTC.DefaultIfEmpty()


                            where (((Type == "all") && (a.PDCType != "Payments") && (a.PDCType != "Payment") && (a.PDCType != "Reciepts") && (a.PDCType != "Reciept"))&&(a.PDCType!= "Journal") || (Type == "TenancyContract" && a.PDCType == "TenancyContract") || (Type == "Maintenance" && a.PDCType == "Maintenance")||(Type== "PropertyRegistrations" && a.PDCType== "PropertyRegistrations")
                            ) &&
                                  (a.RegStatus == choice.No) &&
                                  (fromdate == "" || EF.Functions.DateDiffDay(a.PDCDate, fdate) <= 0) &&
                                  (todate == "" || EF.Functions.DateDiffDay(a.PDCDate, tdate) >= 0) &&
                                  (CheckNumber == "" || a.CheckNo == CheckNumber) 
                                  && (bankid == null || a.Bank == bankid.ToString())
                                  && (a.Type != 1) && (tenant == null || tenant == 0 || tenant == b.Tenant) && (contractor == null || contractor == 0 || contractor == c.Contractor)
                            select new PDCViewModel
                            {
                                id = a.PDCid,
                                Date = a.PDCDate,
                                Voucher = (a.PDCType == "TenancyContract") ? b.Code : ((a.PDCType == "Maintenance") ? c.VoucherNo : (a.PDCType == "PropertyRegistrations") ? z.VoucherNo : ""), //(b.VoucherNo != null) ? b.VoucherNo : c.VoucherNo,
                                Account = (a.PDCType.Contains("Tenancy") ) ? d.TenantName : ((a.PDCType == "PropertyRegistrations") ? "PropertyRegistrations" : "Maintenance"),
                                Issued = h.Amount, //(a.PDCType == "TenancyContract") ? h.Amount : 0,
                                //((a.PDCType == "PropertyRegistrations") ? hh.Amount :i.Amount
                                // (a.PDCType == "TenancyContractDeposit")?j.Amount:
                                Receipt = 0,
                                Journal = 0,
                                check = a.CheckNo,
                                remark = a.Note,
                                CreatedBy = f.UserName,
                                CreatedDate = a.CreatedDate,
                                Branch = g.BranchName
                            });
                if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    prov = prov.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
                }
                recordsTotal = v.Count();
             
                var data2 = prov.Skip(skip).ToList();
                v = v.Union(prov);
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = v.Count();
            var data = v.Skip(skip).ToList();
            return Json(new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,PDC Regularise")]
        public JsonResult update(long[] bill)
        {
            int n = 0;
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var arr in (bill ?? new long[0]))
                    {
                        if (Regularise(arr)) n++;
                    }
                    tx.Commit();
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = false, message = "Regularise failed and was rolled back: " + ex.Message } };
                }
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = true, message = n + " cheque(s) regularised successfully." } };
        }

        // Mark pending cheques as BOUNCED. No GL moves: the held legs were never live, the cheque simply did not clear.
        [HttpPost]
        [QkAuthorize(Roles = "Dev,PDC Regularise")]
        public JsonResult BounceCheques(long[] bill, string reason)
        {
            int n = 0;
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var id in (bill ?? new long[0]))
                    {
                        var pdc = db.PDCs.Find(id);
                        if (pdc == null || pdc.RegStatus == choice.Yes) continue;
                        pdc.BounceDate = System.DateTime.Now;
                        pdc.BounceReason = string.IsNullOrWhiteSpace(reason) ? "Bounced" : reason.Trim();
                        db.Entry(pdc).State = EntityState.Modified;
                        n++;
                    }
                    db.SaveChanges();
                    tx.Commit();
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = false, message = "Bounce failed: " + ex.Message } };
                }
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = true, message = n + " cheque(s) marked as bounced." } };
        }

        // Bulk toggle the hold flag on selected cheques (no GL).
        [HttpPost]
        [QkAuthorize(Roles = "Dev,PDC Regularise")]
        public JsonResult HoldMany(long[] bill)
        {
            int n = 0;
            foreach (var id in (bill ?? new long[0]))
            {
                var pdc = db.PDCs.Find(id);
                if (pdc == null) continue;
                pdc.withhold = (pdc.withhold == null) ? (int?)1 : null;
                db.Entry(pdc).State = EntityState.Modified;
                n++;
            }
            db.SaveChanges();
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = true, message = n + " cheque(s) hold toggled." } };
        }

        // KPI summary for the console header.
        [HttpPost]
        [QkAuthorize(Roles = "Dev,PDC Regularise")]
        public JsonResult GetPdcKpi()
        {
            var today = DateTime.Today;
            var q = (from a in db.PDCs
                     join b in db.Payments on new { f1 = a.Reference, f2 = a.PDCType } equals new { f1 = b.PaymentId, f2 = "Payment" } into pay
                     from b in pay.DefaultIfEmpty()
                     join c in db.Receipts on new { f1 = a.Reference, f2 = a.PDCType } equals new { f1 = c.ReceiptId, f2 = "Receipt" } into rec
                     from c in rec.DefaultIfEmpty()
                     join h in db.Journals on new { f1 = a.Reference, f2 = a.PDCType } equals new { f1 = h.JournalId, f2 = "Journal" } into jnl
                     from h in jnl.DefaultIfEmpty()
                     where a.RegStatus == choice.No && a.BounceDate == null && (a.Type == null || a.Type != 1)
                           && (a.PDCType == "Payment" || a.PDCType == "Receipt" || a.PDCType == "Journal")
                           && ((a.PDCType == "Payment" && b.VoucherNo != null) || (a.PDCType == "Receipt" && c.VoucherNo != null) || (a.PDCType == "Journal" && h.VoucherNo != null))
                     select new { a.PDCDate, a.withhold, amt = (a.PDCType == "Payment") ? (decimal?)b.GrandTotal : ((a.PDCType == "Receipt") ? (decimal?)c.Paying : (decimal?)h.Paying) }).ToList();
            var overdue = q.Where(x => x.PDCDate.Date < today).ToList();
            var dueToday = q.Where(x => x.PDCDate.Date == today).ToList();
            var week = q.Where(x => x.PDCDate.Date >= today && x.PDCDate.Date <= today.AddDays(7)).ToList();
            return Json(new
            {
                pendingCount = q.Count,
                pendingAmt = q.Sum(x => x.amt ?? 0),
                overdueCount = overdue.Count,
                overdueAmt = overdue.Sum(x => x.amt ?? 0),
                todayCount = dueToday.Count,
                todayAmt = dueToday.Sum(x => x.amt ?? 0),
                weekCount = week.Count,
                weekAmt = week.Sum(x => x.amt ?? 0),
                heldCount = q.Count(x => x.withhold != null)
            });
        }

        // Clean, paginated, role-gated grid feed for the reworked console.
        [HttpPost]
        [QkAuthorize(Roles = "Dev,PDC Regularise")]
        public JsonResult GetPdc(bool withhold, string fromdate, string todate, long? AccId, string Type, string CheckNumber, string bankid, string status, string due)
        {
            string search = Request.Form["search[value]"];
            int pageSize = int.TryParse(Request.Form["length"].ToString(), out var l) ? l : 50;
            int skip = int.TryParse(Request.Form["start"].ToString(), out var s) ? s : 0;
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"] + "][name]"].ToString();
            var sortDir = Request.Form["order[0][dir]"].ToString();
            bankid = (bankid ?? "").Trim();
            CheckNumber = (CheckNumber ?? "").Trim();
            Type = string.IsNullOrEmpty(Type) ? "all" : Type;
            status = string.IsNullOrEmpty(status) ? "pending" : status;
            due = string.IsNullOrEmpty(due) ? "all" : due;
            DateTime? fdate = (!string.IsNullOrWhiteSpace(fromdate)) ? DateTime.Parse(fromdate, new CultureInfo("en-GB")) : (DateTime?)null;
            DateTime? tdate = (!string.IsNullOrWhiteSpace(todate)) ? DateTime.Parse(todate, new CultureInfo("en-GB")) : (DateTime?)null;
            var today = DateTime.Today;
            long paybankid = bankid != "" ? db.Accountss.Where(o => o.Name.Contains(bankid)).Select(o => o.AccountsID).FirstOrDefault() : 0;

            var v = (from a in db.PDCs
                     join b in db.Payments on new { f1 = a.Reference, f2 = a.PDCType } equals new { f1 = b.PaymentId, f2 = "Payment" } into pay
                     from b in pay.DefaultIfEmpty()
                     join c in db.Receipts on new { f1 = a.Reference, f2 = a.PDCType } equals new { f1 = c.ReceiptId, f2 = "Receipt" } into rec
                     from c in rec.DefaultIfEmpty()
                     join d in db.Accountss on b.PayTo equals d.AccountsID into payto
                     from d in payto.DefaultIfEmpty()
                     join e in db.Accountss on c.PayFrom equals e.AccountsID into payfrom
                     from e in payfrom.DefaultIfEmpty()
                     join h in db.Journals on new { f1 = a.Reference, f2 = a.PDCType } equals new { f1 = h.JournalId, f2 = "Journal" } into jnl
                     from h in jnl.DefaultIfEmpty()
                     join i in db.Accountss on b.PayFrom equals i.AccountsID into payfromnew
                     from i in payfromnew.DefaultIfEmpty()
                     where ((Type == "all") || (Type == "Payments" && a.PDCType == "Payment") || (Type == "Reciepts" && a.PDCType == "Receipt") || (Type == "Journals" && a.PDCType == "Journal"))
                           && (a.PDCType == "Payment" || a.PDCType == "Receipt" || a.PDCType == "Journal")
                           // exclude orphaned PDCs whose source voucher was deleted (can't be regularised, showed as blank rows)
                           && ((a.PDCType == "Payment" && b.VoucherNo != null) || (a.PDCType == "Receipt" && c.VoucherNo != null) || (a.PDCType == "Journal" && h.VoucherNo != null))
                           && (status == "all"
                               || (status == "bounced" && a.BounceDate != null)
                               || (status == "cleared" && a.RegStatus == choice.Yes)
                               || (status == "pending" && a.RegStatus == choice.No && a.BounceDate == null))
                           && (AccId == null || AccId == 0 || b.PayFrom == AccId || c.PayTo == AccId)
                           && (fdate == null || EF.Functions.DateDiffDay(a.PDCDate, fdate) <= 0)
                           && (tdate == null || EF.Functions.DateDiffDay(a.PDCDate, tdate) >= 0)
                           && (CheckNumber == "" || a.CheckNo == CheckNumber)
                           && (bankid == "" || a.Bank == bankid || b.PayFrom == paybankid)
                           && (a.Type == null || a.Type != 1)
                           && (withhold || a.withhold == null)
                           && (due == "all"
                               || (due == "overdue" && EF.Functions.DateDiffDay(a.PDCDate, today) > 0)
                               || (due == "today" && EF.Functions.DateDiffDay(a.PDCDate, today) == 0)
                               || (due == "week" && EF.Functions.DateDiffDay(today, a.PDCDate) >= 0 && EF.Functions.DateDiffDay(today, a.PDCDate) <= 7))
                     select new PDCViewModel
                     {
                         id = a.PDCid,
                         Date = a.PDCDate,
                         bankname = (a.Bank != null && a.Bank != "") ? a.Bank : i.Name,
                         Voucher = (a.PDCType == "Payment") ? b.VoucherNo : ((a.PDCType == "Receipt") ? c.VoucherNo : h.VoucherNo),
                         Account = (d.Name != null) ? d.Name : (e.Name != null ? e.Name : i.Name),
                         Amount = (a.PDCType == "Payment") ? (decimal?)b.GrandTotal : ((a.PDCType == "Receipt") ? (decimal?)c.Paying : (decimal?)h.Paying),
                         Issued = (decimal?)b.GrandTotal,
                         Receipt = (decimal?)c.Paying,
                         Journal = (decimal?)h.Paying,
                         check = a.CheckNo,
                         remark = a.BounceDate != null ? ("BOUNCED: " + a.BounceReason) : a.Note,
                         CreatedDate = a.CreatedDate,
                         pdctype = a.PDCType,
                         withhold = a.withhold,
                         voucherid = (a.PDCType == "Payment") ? b.PaymentId : ((a.PDCType == "Receipt") ? c.ReceiptId : h.JournalId),
                     });

            if (!string.IsNullOrWhiteSpace(search))
            {
                var sl = search.ToLower();
                v = v.Where(p => (p.Account != null && p.Account.ToLower().Contains(sl)) || (p.Voucher != null && p.Voucher.ToLower().Contains(sl)) || (p.check != null && p.check.Contains(search)));
            }
            int recordsTotal = v.Count();
            if (!string.IsNullOrEmpty(sortColumn))
            {
                try { v = v.AsQueryable().OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortDir) ? "asc" : sortDir)); }
                catch { v = v.OrderBy(p => p.Date); }
            }
            else { v = v.OrderBy(p => p.Date); }
            var data = v.Skip(skip).Take(pageSize > 0 ? pageSize : 50).ToList();
            return Json(new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        private bool Regularise(long pdcid)
        {
            PDC pdc = db.PDCs.Find(pdcid);
            if (pdc == null || pdc.RegStatus == choice.Yes || pdc.BounceDate != null) return false; // skip missing / already-regularised / bounced
            var date = pdc.PDCDate;
            var refId = pdc.Reference;
            if (pdc.PDCType == "Payment")
            {
                var payment = db.Payments.Where(a => a.PaymentId == refId).SingleOrDefault();
                if (payment == null) return false;
                var Balance = com.Accbalance(payment.PayTo);
                var BILL = (pdc.Bills != "" && pdc.Bills != null && pdc.Bills != ";") ? Array.ConvertAll(pdc.Bills.Split(';'), long.Parse) : null;
                string acctype = Convert.ToString(Balance["acctype"]);
                if (BILL != null)
                {
                    var billCheck = com.BillClearPayment(payment.PayTo, payment.Paying, payment.PaymentId, date, payment.Branch, payment.CreatedBy, acctype, BILL);
                }
                Int64 VATInput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Input").Select(a => a.AccountsID).SingleOrDefault();
                //payment

                var pdcaccvat = db.AccountsTransactions.Where(a => (a.Account == VATInput) && (a.Purpose == "Expense Payment") && a.reference == payment.PaymentId).FirstOrDefault();
                if (pdcaccvat != null)
                {
                    var aid = pdcaccvat.Id;
                    com.UpdateAccountTrasaction(aid, payment.TaxAmount, 0, VATInput, "Expense Payment", payment.PaymentId, DC.Debit, date);
                }

                var pdcaccdet = db.AccountsTransactions.Where(a => (a.Account == payment.PayFrom) && (a.Purpose == "Payment") && a.reference == payment.PaymentId).FirstOrDefault();
                if (pdcaccdet != null)
                {
                    var aid = pdcaccdet.Id;
                    com.UpdateAccountTrasaction(aid, 0, payment.Paying, payment.PayFrom, "Payment", payment.PaymentId, DC.Credit, date);
                }
                var pdcaccdet1 = db.AccountsTransactions.Where(a => (a.Account == payment.PayTo) && (a.Purpose == "Payment") && a.reference == payment.PaymentId).FirstOrDefault();
                if (pdcaccdet1 != null)
                {
                    var aid = pdcaccdet1.Id;
                    com.UpdateAccountTrasaction(aid, payment.Paying, 0, payment.PayTo, "Payment", payment.PaymentId, DC.Debit, date);
                }
                //discount
                var Discacc = db.AccountsTransactions.Where(a => (a.Account == 497 || a.Account == 498) && (a.Purpose == "Discount Received") && a.reference == payment.PaymentId).FirstOrDefault();
                if (Discacc != null)
                {
                    var aid = Discacc.Id;
                    com.UpdateAccountTrasaction(aid, 0, Convert.ToDecimal(payment.Discount), 498, "Discount Received", payment.PaymentId, DC.Credit, date);
                }

            }
            if (pdc.PDCType == "Receipt")
            {

                var receipt = db.Receipts.Where(a => a.ReceiptId == refId).SingleOrDefault();
                if (receipt == null) return false;
                var BILL = (pdc.Bills != "" && pdc.Bills != ";" && pdc.Bills != null) ? Array.ConvertAll(pdc.Bills.Split(';'), long.Parse) : null;
                //bill check
                var billCheck = com.BillClearReciept(receipt.PayFrom, receipt.Paying, receipt.ReceiptId, date, receipt.Branch, receipt.CreatedBy, BILL);
                //receipt
                var pdcaccdet = db.AccountsTransactions.Where(a => (a.Account == receipt.PayFrom) && (a.Purpose == "Receipt") && a.reference == receipt.ReceiptId).FirstOrDefault();
                if (pdcaccdet != null)
                {
                    var aid = pdcaccdet.Id;
                    com.UpdateAccountTrasaction(aid, 0, Convert.ToDecimal(receipt.Paying), receipt.PayFrom, "Receipt", receipt.ReceiptId, DC.Credit, date);
                }
                var pdcaccdet1 = db.AccountsTransactions.Where(a => (a.Account == receipt.PayTo) && (a.Purpose == "Receipt") && a.reference == receipt.ReceiptId).FirstOrDefault();
                if (pdcaccdet1 != null)
                {
                    var aid = pdcaccdet1.Id;
                    com.UpdateAccountTrasaction(aid, Convert.ToDecimal(receipt.Paying), 0, receipt.PayTo, "Receipt", receipt.ReceiptId, DC.Debit, date);
                }

                //discount
                var Discacc = db.AccountsTransactions.Where(a => (a.Account == 497 || a.Account == 498) && (a.Purpose == "Discount Allowed") && a.reference == receipt.ReceiptId).FirstOrDefault();
                if (Discacc != null)
                {
                    var aid = Discacc.Id;
                    com.UpdateAccountTrasaction(aid, Convert.ToDecimal(receipt.Discount), 0, 497, "Discount Allowed", receipt.ReceiptId, DC.Debit, date); // keep the receipt-discount row on its original DEBIT side (the old credit-side update was dormant: its lookup never matched)
                }

            }
            if (pdc.PDCType == "Journal")
            {



                var journal = db.Journals.Where(a => a.JournalId == refId).SingleOrDefault();
                if (journal == null) return false;
                var jnlacc = db.AccountsTransactions.Where(a => a.Purpose == "Journal" && a.reference == journal.JournalId).FirstOrDefault();


                //          where (a.Jornal == journal.JournalId)
                //              a.payfrom


                ////bill check
                ////receipt

                
                if (jnlacc != null)
                {
                    (from a in db.AccountsTransactions
                     where a.Purpose == "Journal" && a.reference == journal.JournalId
                     select a).ToList().ForEach(x => { x.Status = null; x.Date = journal.PDCDate; } ); 
                    db.SaveChanges();
                }
            }
            //        (from a in db.AccountsTransactions
            //         where a.Purpose == "TenancyContract" && a.reference == TC.Id
            //                 where a.Purpose == "Maintenance" && a.reference == Maint.ID
            //        //main.
            pdc.RegStatus = choice.Yes;
            pdc.PDCRegDate = System.DateTime.Now;
            db.Entry(pdc).State = EntityState.Modified;
            db.SaveChanges();
            return true;
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
