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
using Microsoft.AspNetCore.Identity;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class POSRESController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public POSRESController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: POS
        [QkAuthorize(Roles = "Dev,POS List")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,POS List")]
        public ActionResult GetSalesEntry()
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

           
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            pageSize = 500;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            int stype = 1;
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.SalesEntrys
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()

                     join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                     join g in db.Users on a.CreatedBy equals g.Id
                     join f in db.WalkinCustomers on a.SalesEntryId equals f.SalesEntryId into wlk
                     from f in wlk.DefaultIfEmpty()


                     select new
                     {
                         a.SalesEntryId,
                         a.SENo,
                         a.BillNo,
                         a.PONo,
                         a.SEDate,
                         paymenttype = (a.CustomerType == CustomerType.Customer) ? "Credit" : (a.CustomerType == CustomerType.Walking) ? "Cash" : "Card",
                         // 'ordertype' removed: System.Text.Json collides "ordertype" with "OrderType"
                         // (case-insensitive) and 500s the action. The Index grid's "ordertype" column render
                         // returns "" (data.ordertype.ot access is commented out) so this nested subquery is unused.
                         a.SEGrandTotal,
                         Customer = b.CustomerCode + " - " + b.CustomerName,
                         CustomerName = f.CustomerName,
                         EmpName = d.FirstName + " " + d.LastName,
                         user = g.UserName,
                         a.CustomerType,
                         a.PayType,
                         OrderType = "Sale",
                         a.SEDiscount,
                         a.SESubTotal,
                         a.SETaxAmount,
                         SEPaidAmount = 0,
                         BalanceAmt = 0,
                     }) ;

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.BillNo.ToString().ToLower().Contains(search.ToLower()));
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
            }
            else
            {
            }
            v = v.OrderByDescending(o => o.SEDate);

            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        public ActionResult QuickCreate()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");//for correct date
            string seNo = "";
            var OrderNoInPOS = db.EnableSettings.Where(a => a.EnableType == "OrderNoInPOS").FirstOrDefault();
            var OrderNoInPOSC = OrderNoInPOS != null ? OrderNoInPOS.Status : Status.inactive;
            var ServiceExpenseInPOS = db.EnableSettings.Where(a => a.EnableType == "ServiceExpenseInPOS").FirstOrDefault();
            ViewBag.ServiceExpenseInPOS = ServiceExpenseInPOS != null ? ServiceExpenseInPOS.Status : Status.inactive;

            var MultiCategory = db.EnableSettings.Where(a => a.EnableType == "MultiCategory").FirstOrDefault();
            ViewBag.MultiCategory = MultiCategory != null ? MultiCategory.Status : Status.inactive;
            var POSCustomItem = db.EnableSettings.Where(a => a.EnableType == "POSCustomItem").FirstOrDefault();
            ViewBag.POSCustomItem = POSCustomItem != null ? POSCustomItem.Status : Status.inactive;
            if (OrderNoInPOSC == Status.active)
            {
                seNo = DOrderNo();
            }
            else
            {
                seNo = OrderNo();
            }
            var orno = (
                from i in db.SalesEntrys
                select i.SENo).AsEnumerable().DefaultIfEmpty(0).Max() + 1;

            var model = new POSEntryViewModel
            {
                OrderNo = orno.ToString(),
                SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                CustomerType = CustomerType.Walking
            };
            ViewBag.custlist = QkSelect.List(db.Customers.Select
                (o => new 
                {
                    Text = o.CustomerName,
                    Value = o.CustomerID
                }), "Value", "Text");
            var pay = db.PaymentCardTypes
                    .Select(s => new
                    {
                        ID = s.CardType,
                        Name = s.CardType
                    })
                    .ToList();
            ViewBag.PaymentMode = QkSelect.List(pay, "ID", "Name");


            var enable = db.EnableSettings.Where(a => a.EnableType == "TouchKeyboard").FirstOrDefault();
            var kbcheck = enable != null ? enable.Status : Status.inactive;
            ViewBag.KBEnable = kbcheck;

            companySet();
            return View(model);
        }



        public ActionResult QuickEdit(long id)
        {
            POSEntryViewModel vmodel = new POSEntryViewModel();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");//for correct date
            string seNo = "";
            var OrderNoInPOS = db.EnableSettings.Where(a => a.EnableType == "OrderNoInPOS").FirstOrDefault();
            var OrderNoInPOSC = OrderNoInPOS != null ? OrderNoInPOS.Status : Status.inactive;
            var ServiceExpenseInPOS = db.EnableSettings.Where(a => a.EnableType == "ServiceExpenseInPOS").FirstOrDefault();
            ViewBag.ServiceExpenseInPOS = ServiceExpenseInPOS != null ? ServiceExpenseInPOS.Status : Status.inactive;

            var MultiCategory = db.EnableSettings.Where(a => a.EnableType == "MultiCategory").FirstOrDefault();
            ViewBag.MultiCategory = MultiCategory != null ? MultiCategory.Status : Status.inactive;
            var POSCustomItem = db.EnableSettings.Where(a => a.EnableType == "POSCustomItem").FirstOrDefault();
            ViewBag.POSCustomItem = POSCustomItem != null ? POSCustomItem.Status : Status.inactive;
            if (OrderNoInPOSC == Status.active)
            {
                seNo = DOrderNo();
            }
            else
            {
                seNo = OrderNo();
            }
            var orno = (
                from i in db.SalesEntrys
                select i.SENo).AsEnumerable().DefaultIfEmpty(0).Max() + 1;

            var model = new POSEntryViewModel
            {
                OrderNo = orno.ToString(),
                SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                CustomerType = CustomerType.Walking
            };
            ViewBag.custlist = QkSelect.List(db.Customers.Select
                (o => new
                {
                    Text = o.CustomerName,
                    Value = o.CustomerID
                }), "Value", "Text");
            var pay = db.PaymentCardTypes
                    .Select(s => new
                    {
                        ID = s.CardType,
                        Name = s.CardType
                    })
                    .ToList();
            ViewBag.PaymentMode = QkSelect.List(pay, "ID", "Name");


            var enable = db.EnableSettings.Where(a => a.EnableType == "TouchKeyboard").FirstOrDefault();
            var kbcheck = enable != null ? enable.Status : Status.inactive;
            ViewBag.KBEnable = kbcheck;
            vmodel = (from b in db.SalesEntrys
                      join c in db.SEPayments on b.SalesEntryId equals c.SalesEntry
                      join d in db.WalkinCustomers on b.SalesEntryId equals d.SalesEntryId into wlk
                      from d in wlk.DefaultIfEmpty()
                      join e in db.PPosDatas on b.SalesEntryId equals e.SalesEntry into pos
                      from e in pos.DefaultIfEmpty()
                      where b.SalesEntryId == id
                      select new POSEntryViewModel
                      {
                          SENo = b.SENo,
                          SENote = b.SENote,
                          SEDate = b.SEDate,
                          BillNo = b.BillNo,
                          SECashier = b.SECashier,
                          Customer = b.Customer,

                          SEDiscount = b.SEDiscount,
                          SEGrandTotal = b.SEGrandTotal,
                          SEPaidAmount = c.SEPaidAmount,
                          PayType = e.PayMethod,
                          CustomerType = b.CustomerType,
                          CustomerName = d.CustomerName,
                          MobileNo = d.MobileNo,

                          // SEDueAmount = b.SEGrandTotal - c.SEPaidAmount,
                      }).FirstOrDefault();
            companySet();
            ViewBag.posdata = db.PPosDatas.Where(a => a.SalesEntry == id).FirstOrDefault();

            //keyboard enable
           
            ViewBag.KBEnable = Status.active;
            companySet();
            return View(model);
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,POS Entry")]
        public ActionResult Createbycustomer()
        {

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");//for correct date
            string seNo = "";
            var cust = db.Customers.Select(s => new
            {
                CustomerID = s.CustomerID,
                CustomerDetails = s.CustomerCode + " - " + s.CustomerName
            }).ToList();
            ViewBag.Customr = QkSelect.List(cust, "CustomerID", "CustomerDetails");
            var OrderNoInPOS = db.EnableSettings.Where(a => a.EnableType == "OrderNoInPOS").FirstOrDefault();
            var OrderNoInPOSC = OrderNoInPOS != null ? OrderNoInPOS.Status : Status.inactive;
            var ServiceExpenseInPOS = db.EnableSettings.Where(a => a.EnableType == "ServiceExpenseInPOS").FirstOrDefault();
            ViewBag.ServiceExpenseInPOS = ServiceExpenseInPOS != null ? ServiceExpenseInPOS.Status : Status.inactive;

            var MultiCategory = db.EnableSettings.Where(a => a.EnableType == "MultiCategory").FirstOrDefault();
            ViewBag.MultiCategory = MultiCategory != null ? MultiCategory.Status : Status.inactive;
            var POSCustomItem = db.EnableSettings.Where(a => a.EnableType == "POSCustomItem").FirstOrDefault();
            ViewBag.POSCustomItem = POSCustomItem != null ? POSCustomItem.Status : Status.inactive;
            if (OrderNoInPOSC == Status.active)
            {
                seNo = DOrderNo();
            }
            else
            {
                seNo = InvoiceNo();
            }
            var model = new POSEntryViewModel
            {
                OrderNo = seNo,
                SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                CustomerType = CustomerType.Walking
            };

            var pay = db.PaymentCardTypes
                    .Select(s => new
                    {
                        ID = s.CardType,
                        Name = s.CardType
                    })
                    .ToList();
            ViewBag.PaymentMode = QkSelect.List(pay, "ID", "Name");


            var enable = db.EnableSettings.Where(a => a.EnableType == "TouchKeyboard").FirstOrDefault();
            var kbcheck = enable != null ? enable.Status : Status.inactive;
            ViewBag.KBEnable = kbcheck;

            companySet();
            return View(model);
        }
        [HttpGet]
       
        public ActionResult Create()
        {

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");//for correct date
            string seNo = "";
            var cust = db.Customers.Select(s => new
            {
                CustomerID = s.CustomerID,
                CustomerDetails = s.CustomerCode + " - " + s.CustomerName
            }).ToList();
            ViewBag.Customr = QkSelect.List(cust, "CustomerID", "CustomerDetails");
            var OrderNoInPOS = db.EnableSettings.Where(a => a.EnableType == "OrderNoInPOS").FirstOrDefault();
            var OrderNoInPOSC = OrderNoInPOS != null ? OrderNoInPOS.Status : Status.inactive;
            var ServiceExpenseInPOS = db.EnableSettings.Where(a => a.EnableType == "ServiceExpenseInPOS").FirstOrDefault();
            ViewBag.ServiceExpenseInPOS = ServiceExpenseInPOS != null ? ServiceExpenseInPOS.Status : Status.inactive;

            var MultiCategory = db.EnableSettings.Where(a => a.EnableType == "MultiCategory").FirstOrDefault();
            ViewBag.MultiCategory = MultiCategory != null ? MultiCategory.Status : Status.inactive;
            var POSCustomItem = db.EnableSettings.Where(a => a.EnableType == "POSCustomItem").FirstOrDefault();
            ViewBag.POSCustomItem = POSCustomItem != null ? POSCustomItem.Status : Status.inactive;
            if (OrderNoInPOSC == Status.active)
            {
                seNo = DOrderNo();
            }
            else
            {
                seNo = InvoiceNo();
            }
            var model = new POSEntryViewModel
            {
                OrderNo = seNo,
                SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                CustomerType = CustomerType.Walking
            };

            var pay = db.PaymentCardTypes
                    .Select(s => new
                    {
                        ID = s.CardType,
                        Name = s.CardType
                    })
                    .ToList();
            ViewBag.PaymentMode = QkSelect.List(pay, "ID", "Name");


            var enable = db.EnableSettings.Where(a => a.EnableType == "TouchKeyboard").FirstOrDefault();
            var kbcheck = enable != null ? enable.Status : Status.inactive;
            ViewBag.KBEnable = kbcheck;

            companySet();
            return View(model);
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,POS Entry")]
        public ActionResult Createmobile()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");//for correct date
            string seNo = "";
            var OrderNoInPOS = db.EnableSettings.Where(a => a.EnableType == "OrderNoInPOS").FirstOrDefault();
            var OrderNoInPOSC = OrderNoInPOS != null ? OrderNoInPOS.Status : Status.inactive;
            var ServiceExpenseInPOS = db.EnableSettings.Where(a => a.EnableType == "ServiceExpenseInPOS").FirstOrDefault();
            ViewBag.ServiceExpenseInPOS = ServiceExpenseInPOS != null ? ServiceExpenseInPOS.Status : Status.inactive;

            var MultiCategory = db.EnableSettings.Where(a => a.EnableType == "MultiCategory").FirstOrDefault();
            ViewBag.MultiCategory = MultiCategory != null ? MultiCategory.Status : Status.inactive;
            var POSCustomItem = db.EnableSettings.Where(a => a.EnableType == "POSCustomItem").FirstOrDefault();
            ViewBag.POSCustomItem = POSCustomItem != null ? POSCustomItem.Status : Status.inactive;
            if (OrderNoInPOSC == Status.active)
            {
                seNo = DOrderNo();
            }
            else
            {
                seNo = OrderNo();
            }
            var model = new POSEntryViewModel
            {
                OrderNo = seNo,
                SEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                CustomerType = CustomerType.Walking
            };

            var pay = db.PaymentCardTypes
                    .Select(s => new
                    {
                        ID = s.CardType,
                        Name = s.CardType
                    })
                    .ToList();
            ViewBag.PaymentMode = QkSelect.List(pay, "ID", "Name");


            var enable = db.EnableSettings.Where(a => a.EnableType == "TouchKeyboard").FirstOrDefault();
            var kbcheck = enable != null ? enable.Status : Status.inactive;
            ViewBag.KBEnable = kbcheck;

            companySet();
            return View(model);
        }
        [HttpPost]
        public ActionResult handoverapprovals(long transid)
        {
            var transactions = db.handover.Where(o => o.reqid == transid).FirstOrDefault();
            var payfrom = db.accountmaps.Where(o => o.PaymentTypeId == EmployeePaymentType.Cash && o.EmployeeId == transactions.reqby).Select(o => o.AccountId).FirstOrDefault();
            var payto = db.accountmaps.Where(o => o.PaymentTypeId == EmployeePaymentType.Cash && o.EmployeeId == transactions.reqto ).Select(o => o.AccountId).FirstOrDefault();
            var amount = transactions.amount;
            var VoucherNo = com.PayVoucherNo();
            long payid=com.addPayment(DateTime.Now, payfrom, payto, amount, amount, amount, "", User.Identity.GetUserId(), 1,0, "Direct Payment");
            com.addAccountTrasaction(amount, 0, payto, "Payment", payid, DC.Debit, DateTime.Now, null, null,null,null);
            com.addAccountTrasaction(0,amount, payfrom, "Payment", payid, DC.Credit, DateTime.Now, null, null, null, null);

            db.handover.RemoveRange(db.handover.Where(o => o.reqid == transid));
            db.SaveChanges();

            Success("transaction success",true);
            return RedirectToAction("Create", "POSRES");
        }
        public ActionResult handoverapproval()
        {
            var userid = User.Identity.GetUserId();
            long empid = db.Employees.Where(o => o.UserId == userid).Select(o => o.EmployeeId).FirstOrDefault();
            var approvallist = (from a in db.handover
                                join b in db.Employees on a.reqby equals b.EmployeeId
                              select new
                              {
                                  amount=a.amount,
                                  name=b.FirstName+" "+b.MiddleName+" "+b.LastName,
                                  trasid=a.reqid,

                                  
                              }).FirstOrDefault();
            if (approvallist != null)
            {
                ViewBag.transferfrom = approvallist.name.ToString();
                ViewBag.accountamount = approvallist.amount.ToString();
                ViewBag.reqid = approvallist.trasid.ToString();
            }
            else
            {
                ViewBag.transferfrom = "";
                ViewBag.accountamount = "";
                ViewBag.reqid = "";
            }
            return View();
        }



        public ActionResult handoverend()
        {
            var userid = User.Identity.GetUserId();
            cashnotes vmodel = new cashnotes();
            var userlist = db.Employees.Where(o => o.UserId != userid).Select(o => new
            {
                Id = o.EmployeeId,
                UserName = o.FirstName + " " + o.MiddleName + " " + o.LastName
            }).ToList();
            ViewBag.ulist = QkSelect.List(userlist, "Id", "UserName");
            var empid = db.Employees.Where(o => o.UserId == userid).Select(o => o.EmployeeId).FirstOrDefault();
            var cashacid = db.accountmaps.Where(o => o.EmployeeId == empid && o.PaymentTypeId == EmployeePaymentType.Cash).Select(o => o.AccountId).FirstOrDefault();
            var debitamout = db.AccountsTransactions.Where(o => o.Account == cashacid).Select(o => o.Debit).AsEnumerable().DefaultIfEmpty(0).Sum();
            var creditamount = db.AccountsTransactions.Where(o => o.Account == cashacid).Select(o => o.Credit).AsEnumerable().DefaultIfEmpty(0).Sum();
            ViewBag.accountamount = (debitamout - creditamount).ToString();
            return View(vmodel);
        }
        [HttpPost]
        public ActionResult handoverend(decimal? txtamount, long transeferto,cashnotes cn)
        {
            var userid = User.Identity.GetUserId();
            long empid = transeferto;// db.Employees.Where(o => o.UserId == transeferto).Select(o => o.EmployeeId).FirstOrDefault();
            DateTime today = DateTime.Now;
           

            handover ht = new handover
            {
                amount = (decimal)txtamount,
                reqby = 0,
                reqdate = today,
                reqto = empid,

            };
       
            Int64 cashAccId = db.Accountss.Where(a => a.Group == 9).Select(a => a.AccountsID).FirstOrDefault();
            var custAccID = 504;
            db.Accountss.Where(a => a.Name == "owner").Select(a => a.AccountsID).FirstOrDefault();
            if (custAccID == null || custAccID == 0)
            {
                Danger("failed");
            }
            else
            {
                var uid = User.Identity.GetUserId();
                var payid = com.addPayment(System.DateTime.Now.Date, cashAccId, custAccID, ht.amount,ht.amount, ht.amount, "End Session", uid, 1);
                com.addAccountTrasaction(0, ht.amount, cashAccId, "Payment", payid, DC.Credit, System.DateTime.Now.Date,true);
                cn.purpuse = "Payment";
                cn.trasdate = today;
                cn.CreatedBy = userid;
                db.cashnotes.Add(cn);
                db.SaveChanges();
                Success("success", true);
            }
            return RedirectToAction("DailySummaryoldautosubmit", "SalesReport");
        }
        public ActionResult handover()
        {
            cashnotes vmodel = new cashnotes();
            var userid = User.Identity.GetUserId();
            var userlist = db.Employees.Where(o => o.UserId != userid).Select(o => new
            {
                Id=o.EmployeeId,
                UserName=o.FirstName+ " "+o.MiddleName + " "+o.LastName
            }).ToList();
            ViewBag.ulist = QkSelect.List(userlist, "Id", "UserName");
            var empid = db.Employees.Where(o => o.UserId == userid).Select(o => o.EmployeeId).FirstOrDefault();
            var cashacid = db.accountmaps.Where(o => o.EmployeeId == empid && o.PaymentTypeId == EmployeePaymentType.Cash).Select(o => o.AccountId).FirstOrDefault();
            var debitamout = db.AccountsTransactions.Where(o => o.Account == cashacid).Select(o => o.Debit).AsEnumerable().DefaultIfEmpty(0).Sum();
            var creditamount = db.AccountsTransactions.Where(o => o.Account == cashacid).Select(o => o.Credit).AsEnumerable().DefaultIfEmpty(0).Sum();
            ViewBag.accountamount = (debitamout - creditamount).ToString();
            return View(vmodel);
        }
        [HttpPost]
        public ActionResult handover(decimal? txtamount,long transeferto, cashnotes cn)
        {
            var userid = User.Identity.GetUserId();
            long empid = transeferto;// db.Employees.Where(o => o.UserId == transeferto).Select(o => o.EmployeeId).FirstOrDefault();
            DateTime today = DateTime.Now;
            handover ht = new handover
            {
                amount = (decimal)txtamount,
                reqby = 0,
                reqdate = today,
                reqto = empid,

            };


            Int64 cashAccId = db.Accountss.Where(a => a.Group == 9).Select(a => a.AccountsID).FirstOrDefault();
            var custAccID = 504;// db.Accountss.Where(a => a.Name=="owner").Select(a => a.AccountsID).FirstOrDefault();
            if (custAccID == null || custAccID == 0)
            {
                Danger("failed");
            }
            else
            {
                var uid = User.Identity.GetUserId();
                var payid = com.addReceipt(System.DateTime.Now.Date, custAccID, cashAccId, ht.amount, ht.amount, "Start Session", uid, 1);
                com.addAccountTrasaction(ht.amount, 0, cashAccId, "Receipt", payid, DC.Debit, System.DateTime.Now.Date,true);
                cn.purpuse = "Receipt";
                cn.trasdate = today;
                cn.CreatedBy = userid;
                db.cashnotes.Add(cn);
                db.SaveChanges();
                Success("success", true);
            }
            return RedirectToAction("Create", "POSRES");
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,POS Entry")]
        public ActionResult Create(POSViewModel vmodel)
        {
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
            bool stat = false;
            string msg;
            while (1==1)
            { 
                if (BillExist(Convert.ToString(vmodel.saleData.BillNo)))
                     {
                    vmodel.saleData.BillNo = (Convert.ToInt64(vmodel.saleData.BillNo) + 1).ToString();
                    }
                else
                {
                    break;
                }
              }
                if (!BillExist(Convert.ToString(vmodel.saleData.BillNo)))
            {
                // for print or save option
                string action = vmodel.fnval;
                //add to saleEntries
                var UserId = User.Identity.GetUserId();
                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                var today = Convert.ToDateTime(System.DateTime.Now);
                //

                //  DateTime.Parse(DateTime.Now.ToString(), new CultureInfo("en-GB")); //
                var TaxAmount = vmodel.saleData.SETaxAmount;

                var PosDate = DateTime.Parse(vmodel.SEDate, new CultureInfo("en-GB"));
                
                //sales entry
                SalesEntry SEentry = new SalesEntry
                {
                    SENo = GetSeNo(),
                    BillNo = vmodel.saleData.BillNo,
                    SEDate = PosDate,
                    SECashier = vmodel.saleData.SECashier,
                    SaleType = SaleType.POS,
                    PayType = vmodel.saleData.PayType,
                    SEItems = vmodel.saleData.SEItems,
                    SEItemQuantity = vmodel.saleData.SEItemQuantity,
                    SESubTotal = vmodel.saleData.SESubTotal,
                    SETax = vmodel.saleData.SETax,
                    SETaxAmount = TaxAmount,
                    SEGrandTotal = vmodel.saleData.SEGrandTotal,
                    SENote = vmodel.saleData.SENote,
                    SEDiscount=vmodel.saleData.SEDiscount,
                    Print = 1,
                    SECreatedDate = today,
                    CreatedBy = UserId,
                    Status = 1,
                    Branch = BranchID,
                    MaterialCenter=1
                };
                //walkin customer
                if (vmodel.saleData.CustomerType == CustomerType.Walking)
                {
                    SEentry.Customer = 0;
                    SEentry.CustomerType = CustomerType.Walking;
                }
                else
                {
                    SEentry.Customer = vmodel.saleData.Customer;
                    SEentry.CustomerType = CustomerType.Customer;
                }
                if(vmodel.saleData.Customer!=null)
                 {
                    SEentry.Customer = vmodel.saleData.Customer;
                    SEentry.CustomerType = CustomerType.Customer;
                }
                db.SalesEntrys.Add(SEentry);
                db.SaveChanges();
                Int64 salesEntryId = SEentry.SalesEntryId;

                //walkin customer
                if (vmodel.saleData.CustomerType == CustomerType.Walking && (vmodel.wCustomer.CustomerName != null || vmodel.wCustomer.MobileNo != null))
                {
                    WalkinCustomer wc = new WalkinCustomer
                    {
                        SalesEntryId = salesEntryId,
                        CustomerName = vmodel.wCustomer.CustomerName,
                        MobileNo = vmodel.wCustomer.MobileNo
                    };
                    db.WalkinCustomers.Add(wc);
                    db.SaveChanges();
                }

                ////// add to SEItem
                string result = string.Empty;
                DataTable dtItem = new DataTable();
                dtItem.Columns.Add("ItemUnit");
                dtItem.Columns.Add("ItemUnitPrice");
                dtItem.Columns.Add("ItemQuantity");
                dtItem.Columns.Add("ItemSubTotal");
                dtItem.Columns.Add("ItemDiscount");
                dtItem.Columns.Add("ItemTax");
                dtItem.Columns.Add("ItemTaxAmount");
                dtItem.Columns.Add("ItemTotalAmount");
                dtItem.Columns.Add("itemNote");
                dtItem.Columns.Add("SaleEntry");
                dtItem.Columns.Add("Item");
                dtItem.Columns.Add("Type");
                foreach (var arr in vmodel.seItems)
                {
                    DataRow dr = dtItem.NewRow();
                    dr["ItemUnit"] = arr.ItemUnit;
                    dr["ItemUnitPrice"] = arr.ItemUnitPrice;
                    dr["ItemQuantity"] = arr.ItemQuantity;
                    dr["ItemSubTotal"] = arr.ItemSubTotal;
                    dr["ItemDiscount"] = 0;
                    dr["ItemTax"] = arr.ItemTax;
                    dr["ItemTaxAmount"] = arr.ItemTaxAmount;
                    dr["ItemTotalAmount"] = arr.ItemTotalAmount;
                    dr["itemNote"] = "";
                    dr["SaleEntry"] = salesEntryId;
                    dr["Item"] = arr.Item;
                    dr["Type"] = 0;
                    dtItem.Rows.Add(dr);
                }

                ////// create parameter 
                SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = "TableTypeSEItems";
                //// execute sp sql 
                string sql = String.Format("EXEC {0} {1};", "SP_InsertSEItems", "@TableType");
                //// execute sql 
                db.Database.ExecuteSqlRaw(sql, parameter);
                QuickSoft.Helpers.DocumentTotals.RecomputeSalesEntry(db, salesEntryId); // forward-correctness: header = SUM(lines)


                //SEPayment
                SEPayment SEpay = new SEPayment
                {
                    CustomerId = vmodel.saleData.Customer,
                    SEDate = PosDate,
                    SEEntryDate = today,
                    SEBillAmount = vmodel.saleData.SEGrandTotal,
                    CreatedBranch = BranchID,
                    CreatedUserId = UserId,
                    SECreatedDate = today,
                    Status = 1,
                    SalesEntry = salesEntryId
                };
                //walkin customer
                if (vmodel.saleData.CustomerType == CustomerType.Walking)
                {
                    SEpay.SEPaidAmount = vmodel.saleData.SEGrandTotal;
                }
                else
                {
                    SEpay.SEPaidAmount = vmodel.salePayment.SEPaidAmount;
                }
                db.SEPayments.Add(SEpay);
                db.SaveChanges();

                decimal amount = Convert.ToDecimal(vmodel.salePayment.SEPaidAmount);
                Int64 custAccID = custAccID = db.Customers.Where(a => a.CustomerID == SEentry.Customer).Select(a => a.Accounts).FirstOrDefault();
                Int64 saleAccId = db.Accountss.Where(a => a.Group == 15).Select(a => a.AccountsID).SingleOrDefault();
                Int64 cashAccId = db.Accountss.Where(a => a.Group == 9).Select(a => a.AccountsID).SingleOrDefault();
                Int64 VATOutput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Output").Select(a => a.AccountsID).SingleOrDefault();


                var date = PosDate;
                //walkin customer
                if (vmodel.saleData.CustomerType == CustomerType.Walking)
                {
                    //AccountsTransaction
                    custAccID = 4;
                    amount = vmodel.saleData.SEGrandTotal;
                }
                if (vmodel.salePayment.SEPaidAmount > 0 || vmodel.saleData.CustomerType == CustomerType.Walking)
                {

                    var Remark = "Direct Reciept From POS";
                    long payid;
                    //SETransaction
                    SETransaction SEtran = new SETransaction
                    {
                        CustomerId = vmodel.saleData.Customer,
                        SEPayDate = date,
                        SEPayAmount = amount,
                        SECreatedDate = today,
                        CreatedBranch = BranchID,
                        CreatedUserId = UserId,
                        SalesEntry = salesEntryId
                    };
                    payid = com.addReceipt(date, custAccID, cashAccId, amount, amount, Remark, UserId, BranchID, salesEntryId);
                    SEtran.Recieptid = payid;
                    db.SETransactions.Add(SEtran);
                    db.SaveChanges();
                }
                if (vmodel.saleData.CustomerType != CustomerType.Walking)
                {
                    var update = com.CusPayment(custAccID, date, BranchID, UserId);
                }
                //add trasaction to sale account with sale entry credit amount
                com.addAccountTrasaction(0, vmodel.saleData.SEGrandTotal, saleAccId, "Sale", salesEntryId, DC.Credit, PosDate);
                //add sale trasaction with customer debt amount
                com.addAccountTrasaction(vmodel.saleData.SEGrandTotal, 0, custAccID, "Sale", salesEntryId, DC.Debit, PosDate);
                // add vat output in account transaction
                if (TaxAmount > 0)
                    com.addAccountTrasaction(TaxAmount, 0, VATOutput, "Sale", salesEntryId, DC.Debit, date);
                //541
                if (vmodel.salePayment.SEPaidAmount > 0 || vmodel.saleData.CustomerType == CustomerType.Walking)
                {
                    //if payment
                    com.addAccountTrasaction(0, amount, custAccID, "Sale Payment", salesEntryId, DC.Credit, PosDate);
                    com.addAccountTrasaction(amount, 0, cashAccId, "Sale Payment", salesEntryId, DC.Debit, PosDate);
                }
                if (vmodel.salePayment.SEPaidAmount > 0 || vmodel.saleData.CustomerType == CustomerType.Card)
                {
                    //if payment
                    com.addAccountTrasaction(0, amount, custAccID, "Sale Payment", salesEntryId, DC.Credit, PosDate);
                    com.addAccountTrasaction(amount, 0, 541, "Sale Payment", salesEntryId, DC.Debit, PosDate);
                }
                com.addlog(LogTypes.Created, UserId, "SalesEntry", "SalesEntrys", findip(), salesEntryId, "Successfully Submitted POS Entry");
                if (action == "print" || action == "print_order")
                {
                    string sedate = SEentry.SEDate.ToString("dd-MM-yyyy");
                    if (1==2)
                    {
                        var sales = (from a in db.SalesEntrys
                                     join f in db.WalkinCustomers on a.SalesEntryId equals f.SalesEntryId into walk
                                     from f in walk.DefaultIfEmpty()
                                     join d in db.SEPayments on a.SalesEntryId equals d.SalesEntry into pay
                                     from d in pay.DefaultIfEmpty()
                                     join e in db.Employees on a.SECashier equals e.EmployeeId into user
                                     from e in user.DefaultIfEmpty()
                                     where a.SalesEntryId == salesEntryId
                                     select new
                                     {
                                         CustomerName = f.CustomerName,
                                         SENo = a.SENo,
                                         PONo = a.PONo,
                                         BillNo = a.BillNo,
                                         Date = sedate,
                                         Note = a.SENote,
                                         CustomerType = a.CustomerType,
                                         Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                                         SEDiscount = a.SEDiscount,
                                         SETotal = a.SEDiscount + a.SEGrandTotal,
                                         SEGrandTotal = a.SEGrandTotal,
                                         SEPaidAmount = a.SEGrandTotal,
                                         SEDueAmount = 0,
                                         SETaxAmount = a.SETaxAmount,
                                         Address = "",
                                         Email = "",
                                         Phone = "",
                                         Mobile = f.MobileNo,
                                         TRN = "",


                                     }).FirstOrDefault();
                        var item = (from a in db.SEItemss
                                    join b in db.Items on a.Item equals b.ItemID
                                    join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                                    from c in primary.DefaultIfEmpty()
                                    join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                                    from d in second.DefaultIfEmpty()
                                    where a.SalesEntry == salesEntryId
                                    select new
                                    {
                                        a.Item,
                                        a.ItemQuantity,
                                        a.ItemUnit,
                                        a.ItemUnitPrice,
                                        a.ItemTax,
                                        a.ItemSubTotal,
                                        a.ItemTaxAmount,
                                        a.ItemDiscount,
                                        a.ItemTotalAmount,
                                        ItemCode = b.ItemCode,
                                        ItemName = b.ItemName,
                                        ItemWithCode = b.ItemCode + " - " + b.ItemName,
                                        b.ItemUnitID,
                                        b.SubUnitId,
                                        PriUnit = c.ItemUnitName,
                                        SubUnit = d.ItemUnitName,
                                        b.ItemArabic
                                    }).ToList();
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { billno = vmodel.saleData.BillNo, status = stat, item = item, sales = sales, PosDate = vmodel.posData } };
                    }
                    else
                    {

                        var sales = (from a in db.SalesEntrys
                                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                                     from b in cust.DefaultIfEmpty()
                                     join c in db.Contacts on b.Contact equals c.ContactID into cnt
                                     from c in cnt.DefaultIfEmpty()
                                     join d in db.SEPayments on a.SalesEntryId equals d.SalesEntry into pay
                                     from d in pay.DefaultIfEmpty()
                                     join e in db.Employees on a.SECashier equals e.EmployeeId into user
                                     from e in user.DefaultIfEmpty()
                                     where a.SalesEntryId == salesEntryId
                                     select new
                                     {
                                         CustomerName = b.CustomerName,
                                         SENo = a.SENo,
                                         PONo = a.PONo,
                                         BillNo = a.BillNo,
                                         Date = sedate,
                                         Note = a.SENote,
                                         CustomerType = a.CustomerType,
                                         Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                                         SEDiscount = a.SEDiscount,
                                         SETotal = a.SEDiscount + a.SEGrandTotal,
                                         SEGrandTotal = a.SEGrandTotal,
                                         SEPaidAmount = d != null ? d.SEPaidAmount : 0,
                                         SEDueAmount = d != null ? a.SEGrandTotal - d.SEPaidAmount : 0,
                                         Address = c.Address + " " + c.City + " " + c.State + " " + c.Country + " " + c.Zip,
                                         Email = c.EmailId,
                                         Phone = c.Phone,
                                         Mobile = c.Mobile,
                                         TRN = b.TaxRegNo,
                                         SETaxAmount = a.SETaxAmount,


                                     }).FirstOrDefault();

                        var item = db.SEItemss.Where(n => n.SalesEntry == salesEntryId).Select(b => new
                        {
                            ItemUnitPrice = b.ItemUnitPrice,
                            ItemQuantity = b.ItemQuantity,
                            ItemSubTotal = b.ItemSubTotal,
                            ItemTax = b.ItemTax,
                            ItemTaxAmount = b.ItemTaxAmount,
                            ItemTotalAmount = b.ItemTotalAmount,
                            ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                            ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                            ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == (db.Items.Where(c => c.ItemID == b.Item).Select(c => c.ItemUnitID).FirstOrDefault())).Select(a => a.ItemUnitName).FirstOrDefault()

                        }).ToList();
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { billno = vmodel.saleData.BillNo, status = stat, item, sales, PosDate = vmodel.posData } };
                    }
                }
                else
                {
                    msg = "Successfully Completed POS Entry.";
                    stat = true;
                    
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { billno= vmodel.saleData.BillNo, status = stat, message = msg } };
                }
            }
            else
            {
                msg = "Invoice No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

            }

        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,POS Edit")]
        public ActionResult Edit(long id)
        {
            SalesEntry Saleentry = db.SalesEntrys.Find(id);
            ViewBag.sepayedamouts = db.SEPayments.Where(o => o.SalesEntry == id).Select(o => o.SEPaidAmount).FirstOrDefault();
            if (Saleentry == null)
            {
                return NotFound();
            }
            Int64 cashier = Convert.ToInt64(Saleentry.SECashier);
            Int64 customer = Saleentry.Customer;

            var ServiceExpenseInPOS = db.EnableSettings.Where(a => a.EnableType == "ServiceExpenseInPOS").FirstOrDefault();
            ViewBag.ServiceExpenseInPOS = ServiceExpenseInPOS != null ? ServiceExpenseInPOS.Status : Status.inactive;
            POSEntryViewModel vmodel = new POSEntryViewModel();



            var cust = db.Customers.Select(s => new
            {
                CustomerID = s.CustomerID,
                CustomerDetails = s.CustomerCode + " - " + s.CustomerName
            }).ToList();
            ViewBag.Customr = QkSelect.List(cust, "CustomerID", "CustomerDetails");


            var use = db.Employees.Select(s => new
            {
                ID = s.EmployeeId,
                Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
            }).ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            var pay = db.PaymentCardTypes
                    .Select(s => new
                    {
                        ID = s.CardType,
                        Name = s.CardType
                    })
                    .ToList();
            ViewBag.PaymentMode = QkSelect.List(pay, "ID", "Name");

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");//for correct date
            vmodel = (from b in db.SalesEntrys
                      where b.SalesEntryId == id
                      select new POSEntryViewModel
                      {
                          SENo = b.SENo,
                          SENote = b.SENote,
                          SEDate = b.SEDate,
                          BillNo = b.BillNo,
                          SECashier = b.SECashier,
                          Customer = b.Customer,
                          
                          SEDiscount = b.SEDiscount,
                          SEGrandTotal = b.SEGrandTotal,
                          //SEPaidAmount = c.SEPaidAmount,
                          //PayType = e.PayMethod,
                          CustomerType = b.CustomerType,
                          //CustomerName = d.CustomerName,
                         // MobileNo = d.MobileNo,

                          // SEDueAmount = b.SEGrandTotal - c.SEPaidAmount,
                      }).FirstOrDefault();
            companySet();
            ViewBag.posdata = db.PPosDatas.Where(a => a.SalesEntry == id).FirstOrDefault();

            //keyboard enable
            var enable = db.EnableSettings.Where(a => a.EnableType == "TouchKeyboard").FirstOrDefault();
            var kbcheck = enable != null ? enable.Status : Status.inactive;
            ViewBag.KBEnable = Status.active;
            return View(vmodel);
        }


        [HttpPost]
        [QkAuthorize(Roles = "Dev,POS Edit")]
        public ActionResult Edit(POSViewModel vmodel, long id)
        {
            bool stat = false;
            string msg;

            // for print or save option
            string action = vmodel.fnval;
            //add to saleEntries
            var UserId = User.Identity.GetUserId();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
            var today = Convert.ToDateTime(System.DateTime.Now);
            //

            //  DateTime.Parse(DateTime.Now.ToString(), new CultureInfo("en-GB")); //
            var TaxAmount = vmodel.saleData.SETaxAmount;
            var PosDate = DateTime.Parse(vmodel.SEDate, new CultureInfo("en-GB"));

            //sales entry
           
            SalesEntry entry = db.SalesEntrys.Find(id);

            var ord = db.POSOrders.Where(o => o.POSOrderId == entry.OrderRefer).FirstOrDefault();
            ord.dcharge = vmodel.dcharge;
            db.Entry(ord).State = EntityState.Modified;
            db.SaveChanges();
            entry.SEDate = PosDate;
            entry.SECashier = vmodel.saleData.SECashier;
            entry.SaleType = SaleType.POS;
            entry.PayType = vmodel.saleData.PayType;
            entry.SEItems = vmodel.saleData.SEItems;
            entry.SEItemQuantity = vmodel.saleData.SEItemQuantity;
            entry.SESubTotal = vmodel.saleData.SESubTotal;
            entry.SETax = vmodel.saleData.SETax;
            entry.SETaxAmount = TaxAmount;
            entry.SEGrandTotal = vmodel.saleData.SEGrandTotal;
            entry.SENote = vmodel.saleData.SENote;
            entry.SEDiscount = vmodel.saleData.SEDiscount;
        
            entry.Print = 1;
            entry.Status = 1;
            entry.Branch = BranchID;
            entry.SETax = vmodel.saleData.SETax;

            //walkin customer
            if (vmodel.saleData.CustomerType == CustomerType.Walking)
            {
                entry.Customer = vmodel.saleData.Customer;
                entry.CustomerType = CustomerType.Walking;
            }
            else if(vmodel.saleData.CustomerType == CustomerType.Card)
            {
                entry.Customer = vmodel.saleData.Customer;
                entry.CustomerType = CustomerType.Card;
            }
            else
            {
                entry.Customer = vmodel.saleData.Customer;
                entry.CustomerType = CustomerType.Customer;
            }
            db.Entry(entry).State = EntityState.Modified;
            db.SaveChanges();


            Int64 salesEntryId = entry.SalesEntryId;

            //walkin customer

            //    WalkinCustomer wc = new WalkinCustomer
            //        SalesEntryId = salesEntryId,
            //        CustomerName = vmodel.wCustomer.CustomerName,
            //        MobileNo = vmodel.wCustomer.MobileNo


            var SEItem = db.SEItemss.Where(a => a.SalesEntry == salesEntryId).FirstOrDefault();
            if (SEItem != null)
            {
                db.SEItemss.RemoveRange(db.SEItemss.Where(a => a.SalesEntry == salesEntryId));
            }


            ////// add to SEItem
            string result = string.Empty;
            DataTable dtItem = new DataTable();
            dtItem.Columns.Add("ItemUnit");
            dtItem.Columns.Add("ItemUnitPrice");
            dtItem.Columns.Add("ItemQuantity");
            dtItem.Columns.Add("ItemSubTotal");
            dtItem.Columns.Add("ItemDiscount");
            dtItem.Columns.Add("ItemTax");
            dtItem.Columns.Add("ItemTaxAmount");
            dtItem.Columns.Add("ItemTotalAmount");
            dtItem.Columns.Add("itemNote");

            dtItem.Columns.Add("SaleEntry");
            dtItem.Columns.Add("Item");
            dtItem.Columns.Add("Type");

            foreach (var arr in vmodel.seItems)
            {
                DataRow dr = dtItem.NewRow();
                dr["ItemUnit"] = arr.ItemUnit;
                dr["ItemUnitPrice"] = arr.ItemUnitPrice;
                dr["ItemQuantity"] = arr.ItemQuantity;
                dr["ItemSubTotal"] = arr.ItemSubTotal;
                dr["ItemDiscount"] = 0;
                dr["ItemTax"] = arr.ItemTax;
                dr["ItemTaxAmount"] = arr.ItemTaxAmount;
                dr["ItemTotalAmount"] = arr.ItemTotalAmount;
                dr["itemNote"] = arr.itemNote == null ? "" : arr.itemNote;

                dr["SaleEntry"] = salesEntryId;
                dr["Item"] = arr.Item;
                dr["Type"] = 0;
                dtItem.Rows.Add(dr);
                if (1==2)
                {
                    var bunQuan = arr.ItemQuantity;
                    var itemBundle = (from g in db.ItemBundles
                                      join b in db.Items on g.mainItem equals b.ItemID
                                      where b.ItemID == arr.Item
                                      select new
                                      {
                                          g.ItemBundleId
                                      }).FirstOrDefault();
                    var bundle = (from a in db.BundleItems
                                  join b in db.Items on a.ItemId equals b.ItemID
                                  join c in db.ItemUnits on a.ItemUnit equals c.ItemUnitID into primary
                                  from c in primary.DefaultIfEmpty()
                                  where a.ItemBundle == itemBundle.ItemBundleId
                                  select new
                                  {
                                      b.ItemCode,
                                      b.ItemName,
                                      c.ItemUnitName,
                                      ItemUnitPrice = a.ItemUnitPrice,
                                      quantity = a.ItemQuantity,
                                      ItemSubTotal = a.ItemSubTotal,
                                      ItemTax = a.ItemTax,
                                      ItemTaxAmount = a.ItemTaxAmount,
                                      ItemTotalAmount = a.ItemTotalAmount,
                                      ItemUnit = a.ItemUnit,
                                      Item = a.ItemId
                                  }).ToList();
                    foreach (var bu in bundle)
                    {
                        var qua = (bunQuan * bu.quantity);
                        var ItemSubTotal = qua * bu.ItemUnitPrice;
                        var buTaxAmount = (ItemSubTotal * bu.ItemTax) / 100;
                        DataRow dbu = dtItem.NewRow();
                        dbu["ItemUnit"] = bu.ItemUnit;
                        dbu["ItemUnitPrice"] = bu.ItemUnitPrice;
                        dbu["ItemQuantity"] = (bunQuan * bu.quantity);
                        dbu["ItemSubTotal"] = ItemSubTotal;
                        // add parent itemid in discount for reference
                        dbu["ItemDiscount"] = arr.Item;
                        dbu["ItemTax"] = bu.ItemTax;
                        dbu["ItemTaxAmount"] = buTaxAmount;
                        dbu["ItemTotalAmount"] = (buTaxAmount + ItemSubTotal);
                        dbu["itemNote"] = "Item Bundle";

                        dbu["SaleEntry"] = salesEntryId;
                        dbu["Item"] = bu.Item;
                        dbu["Type"] = 0;
                        dtItem.Rows.Add(dbu);
                    }
                }

                //                  where arr.AddOnNote.Contains(a.ItemAddOnID)
                //                      b.ItemCode,
                //                      a.Unit,
                //                      b.ItemName,
                //                      c.ItemUnitName,
                //                      ItemUnitPrice = a.UnitPrice,
                //                      quantity = a.Quantity,
                //                      Item = b.ItemID,
                //                      a.ItemAddOnID

                //        // add parent itemid in discount for reference
                //        // passing addon id for identifying
            }

            ////// create parameter 
            SqlParameter parameter = new SqlParameter("@TableType", dtItem);
            parameter.SqlDbType = SqlDbType.Structured;
            parameter.TypeName = "TableTypeSEItems";

            //// execute sp sql 
            string sql = String.Format("EXEC {0} {1};", "SP_InsertSEItems", "@TableType");
            //// execute sql 
            db.Database.ExecuteSqlRaw(sql, parameter);
                QuickSoft.Helpers.DocumentTotals.RecomputeSalesEntry(db, salesEntryId); // forward-correctness: header = SUM(lines)



            //////// add to SEItem


            //////// create parameter 
            ////// execute sp sql 
            ////// execute sql 



            // payment method
            var PayMethod = (vmodel.saleData.CustomerType == CustomerType.Walking) ? "Cash" : (vmodel.saleData.CustomerType == CustomerType.Customer) ? "Credit" : "Card";
            long? AccId = null;
            PosData SEpos = db.PPosDatas.Where(a => a.SalesEntry == salesEntryId).FirstOrDefault();
            if (SEpos != null)
            {
                SEpos.TotTender = vmodel.posData.TotTender;
                SEpos.ChangeDue = vmodel.posData.ChangeDue;

                if (PayMethod == "Card"|| PayMethod == "card")
                {
                    var card = db.PaymentMethods.Where(c => c.MethodName=="card").SingleOrDefault();
                    AccId = card.AccountId;
                    //added
                    SEpos.PayMode = vmodel.posData.PayMode;
                }
                SEpos.PayMethod = vmodel.posData.PayMethod;
                SEpos.Account = AccId;
                db.Entry(SEpos).State = EntityState.Modified;
                db.SaveChanges();
            }
            //SEPayment
            SEPayment SEpay = db.SEPayments.Where(a => a.SalesEntry == salesEntryId).FirstOrDefault();
            if (SEpay != null)
            {
                SEpay.CustomerId = vmodel.saleData.Customer;
                SEpay.SEDate = PosDate;
                SEpay.SEEntryDate = today;
                SEpay.SEBillAmount = vmodel.saleData.SEGrandTotal;
                SEpay.CreatedBranch = BranchID;
                SEpay.CreatedUserId = UserId;
                SEpay.SECreatedDate = today;
                SEpay.Status = 1;
                SEpay.SalesEntry = salesEntryId;

                var payamount = (vmodel.saleData.CustomerType == CustomerType.Walking || vmodel.salePayment.SEPaidAmount > vmodel.saleData.SEGrandTotal) ? vmodel.saleData.SEGrandTotal : vmodel.salePayment.SEPaidAmount;
                // payment method

                if (PayMethod == "Credit")
                {
                    payamount = 0;
                }
                //walkin customer
                SEpay.SEPaidAmount = payamount;
                db.Entry(SEpay).State = EntityState.Modified;
                db.SaveChanges();
            }
            var payamount1 = (vmodel.saleData.CustomerType == CustomerType.Walking || vmodel.salePayment.SEPaidAmount > vmodel.saleData.SEGrandTotal) ? vmodel.saleData.SEGrandTotal : vmodel.salePayment.SEPaidAmount;

            decimal amount = Convert.ToDecimal(payamount1);
            Int64 custAccID = custAccID = db.Customers.Where(a => a.CustomerID == entry.Customer).Select(a => a.Accounts).FirstOrDefault();
            Int64 saleAccId = db.Accountss.Where(a => a.Group == 15).Select(a => a.AccountsID).SingleOrDefault();
            Int64 cashAccId = db.Accountss.Where(a => a.Group == 9).Select(a => a.AccountsID).FirstOrDefault();
            Int64 VATOutput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Output").Select(a => a.AccountsID).SingleOrDefault();
            if (PayMethod == "Card"|| PayMethod == "card")
            {
                cashAccId = (long)AccId;
            }

            var date = PosDate;
            //walkin customer
            if (vmodel.saleData.CustomerType == CustomerType.Walking)
            {
                //AccountsTransaction
                custAccID = 4;
                amount = vmodel.saleData.SEGrandTotal;
            }
            if (payamount1 > 0 || vmodel.saleData.CustomerType == CustomerType.Walking)
            {

                var Remark = "Direct Reciept From POS";
                long payid;
                //SETransaction
                db.SETransactions.RemoveRange(db.SETransactions.Where(a => a.SalesEntry == salesEntryId));
                db.Receipts.RemoveRange(db.Receipts.Where(a => a.Reference == salesEntryId && a.RefType == "Sales"));
                db.SaveChanges();

                SETransaction SEtran = new SETransaction
                {
                    CustomerId = vmodel.saleData.Customer,
                    SEPayDate = date,
                    SEPayAmount = amount,
                    SECreatedDate = today,
                    CreatedBranch = BranchID,
                    CreatedUserId = UserId,
                    SalesEntry = salesEntryId
                };
                payid = com.addReceipt(date, custAccID, cashAccId, amount, amount, Remark, UserId, BranchID, salesEntryId);
                SEtran.Recieptid = payid;
                db.SETransactions.Add(SEtran);
                db.SaveChanges();
            }
            

            bool delete = com.DeleteAllAccountTransaction("Sale", salesEntryId);
            bool deletepay = com.DeleteAllAccountTransaction("Sale Payment", salesEntryId);


            //add trasaction to sale account with sale entry credit amount
            com.addAccountTrasaction(0, vmodel.saleData.SEGrandTotal, saleAccId, "Sale", salesEntryId, DC.Credit, PosDate);
            //add sale trasaction with customer debt amount
            com.addAccountTrasaction(vmodel.saleData.SEGrandTotal, 0, custAccID, "Sale", salesEntryId, DC.Debit, PosDate);
            // add vat output in account transaction
            if (TaxAmount > 0)
                com.addAccountTrasaction(TaxAmount, 0, VATOutput, "Sale", salesEntryId, DC.Debit, date);

            if (payamount1 > 0 || vmodel.saleData.CustomerType == CustomerType.Walking)
            {
                //if payment
                long? EmpId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                long? pettyaccid = db.accountmaps.Where(a => a.EmployeeId == EmpId).Select(a => a.AccountId).FirstOrDefault();

                com.addAccountTrasaction(0, amount, custAccID, "Sale Payment", salesEntryId, DC.Credit, PosDate);

                if (pettyaccid != 0 && PayMethod == "Cash")
                 
               com.addAccountTrasaction(amount, 0, (long)pettyaccid, "Sale Payment", salesEntryId, DC.Debit, PosDate);
                else
                    com.addAccountTrasaction(amount, 0, cashAccId, "Sale Payment", salesEntryId, DC.Debit, PosDate);



            }

            //    // add service expence to payable account delivery Charge Payable : 498
            com.addlog(LogTypes.Created, UserId, "SalesEntry", "SalesEntrys", findip(), salesEntryId, "Successfully Updated POS Entry");
                var update = com.CusPayment(custAccID, date, BranchID, UserId);

            if (action == "print" || action == "print_order")
            {
                string sedate = entry.SEDate.ToString("dd-MM-yyyy");
                if (1==2)
                {
                    var sales = (from a in db.SalesEntrys
                                 join f in db.Customers on a.Customer equals f.CustomerID into walk
                                 from f in walk.DefaultIfEmpty()
                                 join d in db.SEPayments on a.SalesEntryId equals d.SalesEntry into pay
                                 from d in pay.DefaultIfEmpty()
                                 join e in db.Employees on a.SECashier equals e.EmployeeId into user
                                 from e in user.DefaultIfEmpty()
                                 where a.SalesEntryId == salesEntryId
                                 select new
                                 {
                                     CustomerName = f.CustomerName,
                                     SENo = a.SENo,
                                     PONo = a.PONo,
                                     BillNo = a.BillNo,
                                     Date = sedate,
                                     Note = a.SENote,
                                     CustomerType = a.CustomerType,
                                     Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                                     SEDiscount = a.SEDiscount,
                                     SETotal = a.SEDiscount + a.SEGrandTotal,
                                     SEGrandTotal = a.SEGrandTotal,
                                     SEPaidAmount = a.SEGrandTotal,
                                     SEDueAmount = 0,
                                     SETaxAmount = a.SETaxAmount,
                                     Address = "",
                                     Email = "",
                                     Phone = "",
                                     //Mobile = f.MobileNo,
                                     TRN = "",
                                     a.SETax,
                                     TermsAndCondition = db.TermsAndConditionss.Where(i => i.ConditionTypeID == "sales").Select(i => i.TermsCondit).FirstOrDefault(),


                                 }).FirstOrDefault();
                    var item = (from a in db.SEItemss
                                join b in db.Items on a.Item equals b.ItemID
                                join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                                from c in primary.DefaultIfEmpty()
                                join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                                from d in second.DefaultIfEmpty()

                                join g in db.ItemBundles on b.ItemID equals g.mainItem into bundle
                                from g in bundle.DefaultIfEmpty()
                                where a.SalesEntry == salesEntryId 
                                select new
                                {
                                    a.Item,
                                    a.ItemQuantity,
                                    a.ItemUnit,
                                    a.ItemUnitPrice,
                                    a.ItemTax,
                                    a.ItemSubTotal,
                                    a.ItemTaxAmount,
                                    a.ItemDiscount,
                                    a.ItemTotalAmount,
                                    ItemCode = b.ItemCode,
                                    ItemName = b.ItemName,
                                    ItemWithCode = b.ItemCode + " - " + b.ItemName,
                                    b.ItemUnitID,
                                    b.SubUnitId,
                                    PriUnit = c.ItemUnitName,
                                    SubUnit = d.ItemUnitName,
                                    b.ItemArabic,
                                    a.itemNote,
                                    ItemNote = a.itemNote,
                                    g.BundleType,
                                    b.ItemType,
                                    bundle = (from ab in db.SEItemss
                                              join bb in db.Items on ab.Item equals bb.ItemID
                                              join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                                              from cb in primary.DefaultIfEmpty()
                                              join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                                              from bd in second.DefaultIfEmpty()
                                              where ab.SalesEntry == salesEntryId 
                                              && a.Item == ab.ItemDiscount
                                              select new
                                              {
                                                  bb.ItemCode,
                                                  bb.ItemName,
                                                  cb.ItemUnitName,
                                                  ItemUnitPrice = ab.ItemUnitPrice,
                                                  quantity = ab.ItemQuantity,
                                                  ItemSubTotal = ab.ItemSubTotal,
                                                  ItemTax = ab.ItemTax,
                                                  ItemTaxAmount = ab.ItemTaxAmount,
                                                  ItemTotalAmount = ab.ItemTotalAmount,

                                                  ab.Item,
                                                  ab.ItemQuantity,
                                                  ab.ItemUnit,

                                                  ItemDiscount = 0,

                                                  ItemNote = ab.itemNote,
                                                  ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                  bb.ItemUnitID,
                                                  bb.SubUnitId,
                                                  PriUnit = cb.ItemUnitName,
                                                  SubUnit = bd.ItemUnitName,
                                                  bb.ItemArabic,
                                              }).ToList()
                                }).ToList();
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item = item, sales = sales, PosDate = vmodel.posData } };
                }
                else
                {


                    var sales = (from a in db.SalesEntrys
                                 join b in db.Customers on a.Customer equals b.CustomerID into cust
                                 from b in cust.DefaultIfEmpty()
                                 join c in db.Contacts on b.Contact equals c.ContactID into cnt
                                 from c in cnt.DefaultIfEmpty()
                                 join d in db.SEPayments on a.SalesEntryId equals d.SalesEntry into pay
                                 from d in pay.DefaultIfEmpty()
                                 join e in db.Employees on a.SECashier equals e.EmployeeId into user
                                 from e in user.DefaultIfEmpty()
                                 where a.SalesEntryId == salesEntryId
                                 select new
                                 {
                                     CustomerName = b.CustomerName,
                                     SENo = a.SENo,
                                     PONo = a.PONo,
                                     BillNo = a.BillNo,
                                     Date = sedate,
                                     Note = a.SENote,
                                     CustomerType = a.CustomerType,
                                     Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                                     SEDiscount = a.SEDiscount,
                                     SETotal = a.SEDiscount + a.SEGrandTotal,
                                     SEGrandTotal = a.SEGrandTotal,
                                     SEPaidAmount = (d.SEPaidAmount==null)?0: d.SEPaidAmount,
                                     SEDueAmount = a.SEGrandTotal - (d.SEPaidAmount==null?0: d.SEPaidAmount),
                                     Address = c.Address + " " + c.City + " " + c.State + " " + c.Country + " " + c.Zip,
                                     Email = c.EmailId,
                                     Phone = c.Phone,
                                     Mobile = c.Mobile,
                                     TRN = b.TaxRegNo,
                                     SETaxAmount = a.SETaxAmount,
                                     customer = db.Customers.Where(c => c.CustomerID == a.Customer).FirstOrDefault(),
                                     totalpayed = (from ba in db.POSOrders
                                                   where ba.POSOrderId == a.OrderRefer
                                                   select new
                                                   {
                                                       ba.tendering
                                                   }).FirstOrDefault(),
                                     createtime = (from ba in db.POSOrders
                                                   where ba.POSOrderId == a.OrderRefer
                                                   select new
                                                   {
                                                       ba.CreatedDate
                                                   }).FirstOrDefault(),
                                     oc = (from ba in db.POSOrders
                                           where ba.POSOrderId == a.OrderRefer
                                           select new
                                           {
                                               ba.dcharge
                                           }).FirstOrDefault(),
                                     ordertype = (from ba in db.POSOrders
                                                  where ba.POSOrderId == a.OrderRefer
                                                  select new
                                                  {
                                                      ba.OrderType,
                                                      Table = db.Tables.Where(c => c.TableId == ba.TableId).Select(c => c.TableName).FirstOrDefault(),

                                                  }).FirstOrDefault(),

                                 }).FirstOrDefault();

                    var item = db.SEItemss.Where(n => n.SalesEntry == salesEntryId).Select(b => new
                    {
                        ItemUnitPrice = b.ItemUnitPrice,
                        ItemQuantity = b.ItemQuantity,
                        ItemSubTotal = b.ItemSubTotal,
                        ItemTax = b.ItemTax,
                        ItemTaxAmount = b.ItemTaxAmount,
                        ItemTotalAmount = b.ItemTotalAmount,
                        ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                        ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                        ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == (db.Items.Where(c => c.ItemID == b.Item).Select(c => c.ItemUnitID).FirstOrDefault())).Select(a => a.ItemUnitName).FirstOrDefault(),
                      // realname =db.ItemSizes.Where(o=>o.ItemSizeID==db.itemsizeprice.Where(a=>a.sizepriceid.ToString()==b.itemNote).Select(a=>a.sizeid).FirstOrDefault()).Select(a=>a.ItemSizeName).FirstOrDefault()
                    }).ToList();

                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, sales, PosDate = vmodel.posData } };
                }
            }
            else
            {
                msg = "Successfully Updated POS Entry.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }
        [QkAuthorize(Roles = "Dev,POS Delete")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SalesEntry SEen = db.SalesEntrys.Find(id);
            if (SEen == null)
            {
                return NotFound();
            }


            return PartialView(SEen);
        }
        [HttpPost, ActionName("Delete")]
        [QkAuthorize(Roles = "Dev,POS Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            SalesEntry SEen = db.SalesEntrys.Find(id);
            var SEItem = db.SEItemss.Where(a => a.SEItemsId == id);
            var SEP = db.SEPayments.Where(a => a.SalesEntry == id).FirstOrDefault();
            var SEPT = db.SETransactions.Where(a => a.SalesEntry == id).ToList();
            var SEBs = db.SEBillSundrys.Where(a => a.SalesEntry == id).FirstOrDefault();
            var SEposdata = db.PPosDatas.Where(a => a.SalesEntry == id).FirstOrDefault();
            var order = db.POSOrders.Where(a => a.POSOrderId == SEen.OrderRefer).FirstOrDefault();
            if (order != null)
            {
                POSOrder oe = db.POSOrders.Find(SEen.OrderRefer);
                var orderitems = db.POSOrderItems.Where(a => a.OrderId == oe.POSOrderId).FirstOrDefault();
                if (orderitems != null)
                {
                    db.POSOrderItems.RemoveRange(db.POSOrderItems.Where(a => a.OrderId == oe.POSOrderId));
                }
                db.POSOrders.RemoveRange(db.POSOrders.Where(a => a.POSOrderId == SEen.OrderRefer));
            }

            var customerId = db.SalesEntrys.Where(a => a.SalesEntryId == id).Select(a => a.Customer).FirstOrDefault();


            if (SEItem != null)
            {
                db.SEItemss.RemoveRange(db.SEItemss.Where(a => a.SalesEntry == id));

            }
            if (SEBs != null)
            {
                db.SEBillSundrys.RemoveRange(db.SEBillSundrys.Where(a => a.SalesEntry == id));

            }
            if (SEP != null)
            {
                db.SEPayments.RemoveRange(db.SEPayments.Where(a => a.SalesEntry == id));
            }
            if (SEPT != null)
            {
                db.SETransactions.RemoveRange(db.SETransactions.Where(a => a.SalesEntry == id));
            }
            if (SEposdata != null)
            {
                db.PPosDatas.RemoveRange(db.PPosDatas.Where(a => a.SalesEntry == id));
            }

            var rec = db.Receipts.Where(a => a.Reference == id && a.RefType == "Sales").FirstOrDefault();
            if (rec != null)
            {
                db.Receipts.RemoveRange(db.Receipts.Where(a => a.Reference == id && a.RefType == "Sales"));
            }

            db.SalesEntrys.Remove(SEen);


            bool delete = com.DeleteAllAccountTransaction("Sale", id);
            bool deletepay = com.DeleteAllAccountTransaction("Sale Payment", id);


            com.addlog(LogTypes.Deleted, UserId, "SalesEntry", "SalesEntrys", findip(), SEen.SalesEntryId, "Successfully Deleted POS Entry");


            db.SaveChanges();
            stat = true;
            msg = "Successfully deleted POS details.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,POS Delete")]
        public ActionResult DeleteAllSales(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = DeleteSale(arr);
                if (chk == true)
                {
                    count++;
                }
            }
            Success("Deleted " + count + " Sales Entry.", true);
            return RedirectToAction("Index", "POS");
        }

        private Boolean DeleteSale(long id)
        {
            var UserId = User.Identity.GetUserId();
            SalesEntry SEen = db.SalesEntrys.Find(id);
            var SEItem = db.SEItemss.Where(a => a.SEItemsId == id);
            var SEP = db.SEPayments.Where(a => a.SalesEntry == id).FirstOrDefault();
            var SEPT = db.SETransactions.Where(a => a.SalesEntry == id).ToList();
            var SEBs = db.SEBillSundrys.Where(a => a.SalesEntry == id).FirstOrDefault();
            var SEposdata = db.PPosDatas.Where(a => a.SalesEntry == id).FirstOrDefault();
            var order = db.POSOrders.Where(a => a.POSOrderId == SEen.OrderRefer).FirstOrDefault();
            if (order != null)
            {
                POSOrder oe = db.POSOrders.Find(SEen.OrderRefer);
                var orderitems = db.POSOrderItems.Where(a => a.OrderId == oe.POSOrderId).FirstOrDefault();
                if (orderitems != null)
                {
                    db.POSOrderItems.RemoveRange(db.POSOrderItems.Where(a => a.OrderId == oe.POSOrderId));
                }
                db.POSOrders.RemoveRange(db.POSOrders.Where(a => a.POSOrderId == SEen.OrderRefer));
            }

            var customerId = db.SalesEntrys.Where(a => a.SalesEntryId == id).Select(a => a.Customer).FirstOrDefault();


            if (SEItem != null)
            {
                db.SEItemss.RemoveRange(db.SEItemss.Where(a => a.SalesEntry == id));

            }
            if (SEBs != null)
            {
                db.SEBillSundrys.RemoveRange(db.SEBillSundrys.Where(a => a.SalesEntry == id));

            }
            if (SEP != null)
            {
                db.SEPayments.RemoveRange(db.SEPayments.Where(a => a.SalesEntry == id));
            }
            if (SEPT != null)
            {
                db.SETransactions.RemoveRange(db.SETransactions.Where(a => a.SalesEntry == id));
            }
            if (SEposdata != null)
            {
                db.PPosDatas.RemoveRange(db.PPosDatas.Where(a => a.SalesEntry == id));
            }

            var rec = db.Receipts.Where(a => a.Reference == id && a.RefType == "Sales").FirstOrDefault();
            if (rec != null)
            {
                db.Receipts.RemoveRange(db.Receipts.Where(a => a.Reference == id && a.RefType == "Sales"));
            }

            db.SalesEntrys.Remove(SEen);


            bool delete = com.DeleteAllAccountTransaction("Sale", id);
            bool deletepay = com.DeleteAllAccountTransaction("Sale Payment", id);

            com.addlog(LogTypes.Deleted, UserId, "SalesEntry", "SalesEntrys", findip(), SEen.SalesEntryId, "Successfully Deleted POS Entry");

            db.SaveChanges();

            return true;
        }

        [HttpGet]
        public ActionResult GetSEItems(long SalesEntryID)
        {
            var sales = (from a in db.SalesEntrys
                        
                         
                         join e in db.Employees on a.SECashier equals e.EmployeeId into emp
                         from e in emp.DefaultIfEmpty()
                         join f in db.WalkinCustomers on a.SalesEntryId equals f.SalesEntryId into walk
                         from f in walk.DefaultIfEmpty()
                         join g in db.PPosDatas on a.SalesEntryId equals g.SalesEntry into pos
                         from g in pos.DefaultIfEmpty()

                         where a.SalesEntryId == SalesEntryID
                         select new
                         {
                             CustomerName = f.CustomerName,
                             SENo = a.SENo,
                             PONo = a.PONo,
                             BillNo = a.BillNo,
                             Date = a.SEDate,
                             Note = a.SENote,
                             CustomerType = a.CustomerType,
                             TableName ="table",
                             SEDiscount = a.SEDiscount,
                             SETotal = a.SEDiscount + a.SEGrandTotal,
                             SEGrandTotal = a.SEGrandTotal,
                             SEPaidAmount = a.SEGrandTotal,
                             SEDueAmount = 0,
                             SETaxAmount = a.SETaxAmount,
                             Address = "",
                             Email = "",
                             Phone = "",
                             Mobile = f.MobileNo,
                             TRN = "",
                             SubTotal = a.SESubTotal,
                             WaiterName = e.FirstName + " " + e.LastName,
                             OrderType="Sales",
                             a.SECashier,
                             g.PayMethod,
                             g.PayMode,
                             oc = (from ba in db.POSOrders
                                   where ba.POSOrderId == a.OrderRefer
                                   select new
                                   {
                                       ba.dcharge
                                   }).FirstOrDefault(),
                             a.SETax

                         }).FirstOrDefault();


            var item = (from a in db.SEItemss
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()
                        join g in db.ItemBundles on b.ItemID equals g.mainItem into bundle
                        from g in bundle.DefaultIfEmpty()

                        join u in db.ItemUnits on a.ItemUnit equals u.ItemUnitID into unit
                        from u in unit.DefaultIfEmpty()

                        where a.SalesEntry == SalesEntryID 
                        select new
                        {
                            a.Item,
                            a.ItemQuantity,
                            a.ItemUnit,
                            a.ItemUnitPrice,
                            a.ItemTax,
                            a.ItemSubTotal,
                            a.ItemTaxAmount,
                            a.ItemDiscount,
                            a.ItemTotalAmount,
                            note = a.itemNote.Replace("<br />", "\n"),
                            Note =a.itemNote+"|"+ a.ItemUnitPrice.ToString(),                      
                            a.itemNote,
                            b.ItemCode,
                            b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                            b.ItemID,
                            b.OpeningStock,
                            b.MinStock,
                            categoryname = e.ItemCategoryName,
                            Tax = f.Percentage,
                            b.SellingPrice,
                            b.PurchasePrice,
                            b.BasePrice,
                            b.MRP,
                            b.KeepStock,
                            g.BundleType,
                            u.ItemUnitName,

                            PriPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,


                            PriSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,

                            //stock adjustment---
                            PriAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            subLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            //-------

                            // main item
                            //PriProdItem = (decimal?)db.Productions.Where(a => a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
                            //SubProdItem = (decimal?)db.Productions.Where(a => a.Item == b.ItemID && a.Unit == b.SubUnitId).Select(a => a.Qty).Sum() ?? 0,
                            //// compined item
                            PriProdCItem = (decimal?)db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,
                            SubProdCItem = (decimal?)db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId).Select(a => a.Quantity).Sum() ?? 0,
                            // unassemble -----
                            // main item
                            //PriUnItem = (decimal?)db.Unassembles.Where(a => a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
                            //SubUnItem = (decimal?)db.Unassembles.Where(a => a.Item == b.ItemID && a.Unit == b.SubUnitId).Select(a => a.Qty).Sum() ?? 0,
                            //// compined item
                            PriUnCItem = (decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,
                            SubUnCItem = (decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId).Select(a => a.Quantity).Sum() ?? 0,


                            // production ----
                            b.ItemType,
                            bundle = (from ab in db.SEItemss
                                      join bb in db.Items on ab.Item equals bb.ItemID
                                      join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                                      from cb in primary.DefaultIfEmpty()
                                      join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                                      from bd in second.DefaultIfEmpty()

                                      join un in db.ItemUnits on ab.ItemUnit equals un.ItemUnitID into unit
                                      from un in unit.DefaultIfEmpty()
                                      where ab.SalesEntry == SalesEntryID 
                                      && a.Item == ab.ItemDiscount && ab.itemNote != "AddOn"
                                      select new
                                      {
                                          bb.ItemCode,
                                          bb.ItemName,
                                          UnitName = cb.ItemUnitName,
                                          ItemUnitPrice = ab.ItemUnitPrice,
                                          quantity = ab.ItemQuantity,
                                          ItemSubTotal = ab.ItemSubTotal,
                                          ItemTax = ab.ItemTax,
                                          ItemTaxAmount = ab.ItemTaxAmount,
                                          ItemTotalAmount = ab.ItemTotalAmount,

                                          ab.Item,
                                          ab.ItemQuantity,
                                          ab.ItemUnit,

                                          ItemDiscount = 0,

                                          ItemNote = ab.itemNote,
                                          ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                          bb.ItemUnitID,
                                          bb.SubUnitId,
                                          PriUnit = cb.ItemUnitName,
                                          SubUnit = bd.ItemUnitName,
                                          bb.ItemArabic,
                                          un.ItemUnitName,
                                      }).ToList(),
                            Addon = (from ab in db.SEItemss
                                     join bb in db.Items on ab.Item equals bb.ItemID
                                     join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                                     from cb in primary.DefaultIfEmpty()
                                     join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                                     from bd in second.DefaultIfEmpty()
                                     join be in db.ItemAddOns on ab.ItemTax equals be.ItemAddOnID into addon
                                     from be in addon.DefaultIfEmpty()
                                     where ab.SalesEntry == SalesEntryID 
                                     && a.Item == ab.ItemDiscount && ab.itemNote == "AddOn"
                                     select new
                                     {
                                         bb.ItemCode,
                                         bb.ItemName,
                                         cb.ItemUnitName,
                                         ItemUnitPrice = ab.ItemUnitPrice,
                                         quantity = ab.ItemQuantity,
                                         ItemSubTotal = 0,
                                         ItemTax = 0,
                                         ItemTaxAmount = 0,
                                         ItemTotalAmount = 0,
                                         ab.ItemId,
                                         ab.ItemQuantity,
                                         ab.ItemUnit,
                                         ItemDiscount = 0,
                                         ItemNote = ab.itemNote,
                                         ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                         bb.ItemUnitID,
                                         bb.SubUnitId,
                                         PriUnit = cb.ItemUnitName,
                                         SubUnit = bd.ItemUnitName,
                                         bb.ItemArabic,
                                         Name = be.Name
                                     }).ToList(),
                        }).AsEnumerable().Select(o => new
                        {
                            itemid = o.Item,
                            o.ItemQuantity,
                            o.ItemUnit,
                            o.ItemUnitPrice,
                            o.ItemTax,
                            o.ItemSubTotal,
                            o.ItemTaxAmount,
                            o.ItemDiscount,
                            o.ItemTotalAmount,
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            o.itemNote,
                            o.Note,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            o.categoryname,
                            o.Tax,
                            OpeningStock = o.OpeningStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ConFactor,
                            o.SellingPrice,
                            o.PurchasePrice,
                            o.BasePrice,
                            o.MRP,
                            //price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            price = o.ItemUnitPrice,
                            o.KeepStock,
                            ItemNote = o.itemNote,
                            PriPurchase = (o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)),
                            SubPurchase = (o.SubPurchase % o.ConFactor),

                            PriSale = (o.PriSale + (int)(o.SubSale / o.ConFactor)),
                            SubSale = (o.SubSale % o.ConFactor),

                            PriPReturn = (o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)),
                            SubPReturn = (o.SubPReturn % o.ConFactor),

                            PriSReturn = (o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)),
                            SubSReturn = (o.SubSReturn % o.ConFactor),

                            o.ItemType,
                            o.BundleType,
                            o.bundle,
                            o.Addon,
                            itemsize = (from a in db.itemsizeprice
                                        join b in db.ItemSizes on a.sizeid equals b.ItemSizeID
                                        where a.itemid == o.ItemID
                                        select new
                                        {
                                            a.itemid,
                                            b.ItemSizeName,
                                            a.price,
                                            a.sizepriceid
                                        }).ToList()
                        }).ToList();

            var data = new { item = item, sales = sales };
            return Json(data);
        }


        [HttpGet]
        [QkAuthorize(Roles = "Dev,View POS")]
        public ActionResult Details(long? id)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");//for correct date
            SalesEntryViewModel vmodel = new SalesEntryViewModel();
            vmodel = (from b in db.SalesEntrys
                      join c in db.SEPayments on b.SalesEntryId equals c.SalesEntry into pay
                      from c in pay.DefaultIfEmpty()
                      join d in db.Employees on b.SECashier equals d.EmployeeId into emp
                      from d in emp.DefaultIfEmpty()
                      join f in db.Customers on b.Customer equals f.CustomerID into cust
                      from f in cust.DefaultIfEmpty()
                      join g in db.WalkinCustomers on b.SalesEntryId equals g.SalesEntryId into wlk
                      from g in wlk.DefaultIfEmpty()
                      where b.SalesEntryId == id
                      select new SalesEntryViewModel
                      {
                          CustomerName = (b.CustomerType == CustomerType.Walking ? g.CustomerName : f.CustomerCode + " - " + f.CustomerName),
                          SENo = b.SENo,
                          BillNo = b.BillNo,
                          PONo = b.PONo,
                          SEDate = b.SEDate,
                          SENote = b.SENote.Replace("\n", "<br />"),
                          EmployeeName = d.FirstName + " " + d.LastName,
                          CustomerType = b.CustomerType,
                          SEDiscount = b.SEDiscount,
                          SETotal = b.SEDiscount + b.SEGrandTotal,
                          SEGrandTotal = b.SEGrandTotal,
                          SEPaidAmount = b.CustomerType == 0 ? c.SEPaidAmount : b.SEGrandTotal,
                          SEDueAmount = b.CustomerType == 0 ? b.SEGrandTotal - c.SEPaidAmount : 0,
                          PayType = (b.CustomerType == CustomerType.Walking ? "Cash" : "Credit")
                      }).FirstOrDefault();

            vmodel.SEItem = db.SEItemss.Where(a => a.SalesEntry == id)
            .Select(b => new SEItemViewModel
            {
                ItemUnitPrice = b.ItemUnitPrice,
                ItemQuantity = b.ItemQuantity,
                ItemSubTotal = b.ItemSubTotal,
                ItemTax = b.ItemTax,
                itemNote = b.itemNote,
                ItemTaxAmount = b.ItemTaxAmount,
                ItemTotalAmount = b.ItemTotalAmount,
                ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault()

            }).ToList();
            vmodel.SEbs = db.SEBillSundrys.Where(a => a.SalesEntry == id)
         .Select(b => new SEBillSundryViewModel
         {
             AmountType = b.AmountType,
             BsAmount = b.BsAmount,
             BsType = b.BsType,
             BsValue = b.BsValue,
             Type = b.BsType == 0 ? "Add" : "Less",
             AmtType = b.AmountType == 0 ? "" : "%",
             BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
         }).ToList();

            return View(vmodel);
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,POS Download")]
        public ActionResult Download(long id)
        {
            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = db.SalesEntrys.Where(s => s.SalesEntryId == id).Select(s => s.BillNo).FirstOrDefault();

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id));
            return File(ms, "application/pdf", cname + "_TaxInvoice_" + billno + "_" + System.DateTime.Now.ToShortDateString() + ".pdf");

        }
 
        public StringBuilder generatePdf(long id)
        {
            var details = (from a in db.SalesEntrys
                           join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry into pay
                           from c in pay.DefaultIfEmpty()
                           join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                           from d in emp.DefaultIfEmpty()
                           join f in db.Customers on a.Customer equals f.CustomerID into cust
                           from f in cust.DefaultIfEmpty()
                           join g in db.WalkinCustomers on a.SalesEntryId equals g.SalesEntryId into wlk
                           from g in wlk.DefaultIfEmpty()
                           join e in db.Employees on a.SECashier equals e.EmployeeId into user
                           from e in user.DefaultIfEmpty()
                           join h in db.Contacts on f.Contact equals h.ContactID into cnt
                           from h in cnt.DefaultIfEmpty()
                           where a.SalesEntryId == id
                           select new
                           {

                               PartyName = (a.CustomerType == CustomerType.Walking ? f.CustomerName : g.CustomerName),
                               BillId = a.SalesEntryId,

                               BillNo = a.BillNo,
                               Date = a.SEDate,
                               Note = a.SENote,
                               CustomerType = a.CustomerType,
                               Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                               Discount = a.SEDiscount,
                               Total = a.SEDiscount + a.SEGrandTotal,
                               GrandTotal = a.SEGrandTotal,
                               Paid = a.SEGrandTotal,
                               SubTotal = a.SESubTotal,
                               TaxAmount = a.SETaxAmount,
                               Balance = 0,
                               Address = (a.CustomerType == CustomerType.Walking ? "" : h.Address),
                               Email = (a.CustomerType == CustomerType.Walking ? "" : h.EmailId),
                               Phone = (a.CustomerType == CustomerType.Walking ? "" : h.Phone),
                               Mobile = (a.CustomerType == CustomerType.Walking ? g.MobileNo : h.Mobile),
                               TRN = (a.CustomerType == CustomerType.Walking ? "" : f.TaxRegNo),
                               City = (a.CustomerType == CustomerType.Walking ? "" : h.City),
                               State = (a.CustomerType == CustomerType.Walking ? "" : h.State),
                               Country = (a.CustomerType == CustomerType.Walking ? "" : h.Country),
                               Zip = (a.CustomerType == CustomerType.Walking ? "" : h.Zip),
                               paytype = (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit"),
                               TermsCondition = a.SENote != null ? a.SENote : "",
                           }).FirstOrDefault();
            var saleitem = (from a in db.SEItemss
                            join b in db.Items on a.Item equals b.ItemID
                            join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                            from c in primary.DefaultIfEmpty()
                            join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                            from d in second.DefaultIfEmpty()
                            join e in db.ItemImages on b.ItemID equals e.ItemID into img
                            from e in img.DefaultIfEmpty()
                            where a.SalesEntry == id 
                            select new
                            {
                                a.Item,
                                a.ItemQuantity,
                                a.ItemUnit,
                                a.ItemUnitPrice,
                                a.ItemTax,
                                a.ItemSubTotal,
                                a.ItemTaxAmount,
                                a.ItemDiscount,
                                a.ItemTotalAmount,
                                ItemCode = b.ItemCode,
                                ItemName = b.ItemName,
                                ItemNote = a.itemNote,
                                ItemWithCode = b.ItemCode + " - " + b.ItemName,
                                b.ItemUnitID,
                                b.SubUnitId,
                                PriUnit = c.ItemUnitName,
                                SubUnit = d.ItemUnitName,
                                b.ItemArabic,
                                b.ItemType,
                                a.itemNote,
                                b.ItemDescription,
                                e.FileName,
                                bundleitem = (from ab in db.SEItemss
                                              join bb in db.Items on ab.Item equals bb.ItemID
                                              join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                                              from cb in primary.DefaultIfEmpty()
                                              join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                                              from bd in second.DefaultIfEmpty()
                                              where ab.SalesEntry == id 
                                              && a.Item == ab.ItemDiscount
                                              select new
                                              {

                                                  bb.ItemCode,
                                                  bb.ItemName,
                                                  cb.ItemUnitName,
                                                  ItemUnitPrice = ab.ItemUnitPrice,
                                                  quantity = ab.ItemQuantity,
                                                  ItemSubTotal = ab.ItemSubTotal,
                                                  ItemTax = ab.ItemTax,
                                                  ItemTaxAmount = ab.ItemTaxAmount,
                                                  ItemTotalAmount = ab.ItemTotalAmount,

                                                  ab.Item,
                                                  ab.ItemQuantity,
                                                  ab.ItemUnit,

                                                  ItemDiscount = 0,

                                                  ItemNote = ab.itemNote,
                                                  ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                  bb.ItemUnitID,
                                                  bb.SubUnitId,
                                                  PriUnit = cb.ItemUnitName,
                                                  SubUnit = bd.ItemUnitName,
                                                  bb.ItemArabic,
                                                  bb.ItemType
                                              }).ToList()
                            }).ToList();
            var billsundry = db.SEBillSundrys.Where(n => n.SalesEntry == id).Select(b => new
            {
                AmountType = b.AmountType,
                BsAmount = b.BsAmount,
                BsType = b.BsType,
                BsValue = b.BsValue != null ? b.BsValue : 0,
                BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
            }).ToList();

            var cdetails = db.companys
            .Select(s => new
            {
                CName = s.CPName,
                CAddress = s.CPAddress,
                CEmail = s.CPEmail,
                CTaxRegNo = s.TRN,
                CPhone = s.CPPhone,
                s.CPMobile,
                CLogo = s.CPLogo,
            }).FirstOrDefault();

            int SI = 1;
            string address = "";
            if (details.Address != null)
            {
                address += details.Address;
            }
            if (details.City != null)
            {
                address += "<br />" + details.City;
            }
            else if (details.State != null)
            {
                address += "<br />" + details.State;
            }
            else if (details.Country != null)
            {
                address += "<br />" + details.Country;
            }
            else if (details.Zip != null)
            {
                address += "<br />" + details.Zip;
            }
            address += " <br/> Phone : ";
            if (details.Mobile != null)
            {
                address += details.Mobile;
                if (details.Phone != null)
                {
                    address += ", " + details.Phone;
                }
            }
            else
            {
                if (details.Phone != null)
                {
                    address += details.Phone;
                }
            }
            if (details.Email != null)
            {
                address += "<br/> Email : " + details.Email;
            }
            if (details.TRN != null)
            {
                address += "<br/> TRN : " + details.TRN;
            }

            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {

                    sb.Append("<table width='100%' style='border: 0px;text-align:center;'><tr><td><b>Tax Invoice</b></td></tr></table>");
                    string partyDetails = "<table style='border:.1px #ccc;' width='100%'><tr style='border-top:.1px #ccc; '> " +
                        "<td width='50%'> " +
                        "<table  style='border: 0px; width: 100 %;'><tr><th><i><b>Customer زبون</b></i></th></tr><tr><td>" + details.PartyName + "</td></tr><tr><td style='font-size:14px;font-weight:normal;'>" + address + "</td></tr></table></td><td width='50%' style='border-left: 1px solid #000000;'>" +
                        "<table  style='border: 0px; width: 100 %;'><tr><th>Invoice No</th><td style='font-size:14px;font-weight:normal;'>: " + details.BillNo + "</td></tr><tr><th>Date تاريخ</th><td style='font-size:14px;font-weight:normal;'>: " + details.Date.ToString("dd-MM-yyyy") + "</td></tr><tr><th>Sales Executive منفذ مبيعات</th><td style='font-size:14px;font-weight:normal;'>: " + details.Cashier + "</td></tr></table></td></tr></table>";

                    sb.Append(partyDetails);
                    sb.Append("<table width='100%' style='border-collapse:collapse;font-size:12px;border: .1px solid #ccc;'>");
                    sb.Append("<thead>");
                    sb.Append("<tr style='font-size:13px;'>");
                    sb.Append("<th width='5%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>S.N.</th>");
                    sb.Append("<th style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Item</th>");
                    sb.Append("<th width='5%' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>Unit</th>");
                    sb.Append("<th width='8%' style='border: .5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Quantity</th>");
                    sb.Append("<th width='8%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Unit Price</th>");
                    sb.Append("<th width='8%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Amount</th>");
                    sb.Append("<th width='10%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Tax(5.00 %)</th>");
                    sb.Append("<th width='8%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Total</th>");
                    sb.Append("</tr>");
                    sb.Append("</thead>");
                    sb.Append("<tbody>");
                    var itemcount = 0;
                    foreach (var item in saleitem)
                    {
                        var itemname = "";
                        if (item.ItemArabic != null)
                        {
                            itemname = item.ItemName + " " + item.ItemArabic;
                        }
                        else
                        {
                            itemname = item.ItemName;
                        }

                        if (item.ItemType ==1)
                        {
                            var desc = "<br/>[<span class='descr' data-name='Note'>";
                            foreach (var itemss in item.bundleitem)
                            {
                                desc += itemss.ItemCode + " - " + itemss.ItemName;
                                desc += " - " + (itemss.quantity) + " ";
                                desc += (itemss.ItemUnitName != null) ? itemss.ItemUnitName : "";
                                desc += "<br/>";
                            }
                            desc += "</span>]";
                            itemname = itemname + desc;
                        }
                        if (item.ItemNote != "" && item.ItemNote != null && item.ItemNote != "undefined")
                        {
                            itemname = itemname + "<br />" + item.ItemNote;
                        }

                        sb.Append("<tr style='font-size:10px;'>");
                        {
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;border: .1px solid #ccc;'>" + SI++ + "</td>");
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;border: .1px solid #ccc;'>");
                            sb.Append(itemname + "<br/>");
                            sb.Append("<img width='40px' height='70px' src='" + LegacyWeb.MapPath("/uploads/itemimages/" + item.Item + "/" + item.FileName) + "'/>");
                            sb.Append("<br/>");
                            sb.Append("</td>");
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + item.ItemUnit + "</td>");
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + item.ItemQuantity + "</td>");
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + item.ItemUnitPrice + "</td>");
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + item.ItemSubTotal + "</td>");
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + item.ItemTaxAmount + "</td>");
                            sb.Append("<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + item.ItemTotalAmount + "</td>");
                        }
                        sb.Append("</tr>");
                        itemcount++;
                    }
                    sb.Append("</tbody>");
                    sb.Append("</table>");

                    string words = com.ConvertToWords(details.GrandTotal.ToString());
                    sb.Append("<table width='100%' style='border-collapse: collapse;border: .1px solid #ccc;font-size: 14px;'>");
                    string discount = "";
                    int count = 2;
                    if (details.Discount > 0)
                    {
                        discount = "<td style='border: .1px solid #ccc;padding: 10px;'>Discount خصم</td><td style='border: .1px solid #ccc;padding: 10px;' class='text-right'>" + details.Discount + "</td></tr> ";
                        discount += "<tr class='border-top'><td style='border: .1px solid #ccc;padding: 10px;'>VAT<span style='direction:ltr'>(5.00%)</span> برميل</td><td style='border: .1px solid #ccc;padding: 10px;' class='text-right'>" + details.TaxAmount + "</td></tr>";
                        count++;
                    }
                    else
                    {

                        discount += "<td style='border: .1px solid #ccc;padding: 10px;'>VAT<span style='direction:ltr'>(5.00%)</span> برميل </td><td style='border: .1px solid #ccc;padding: 10px;' class='text-right'>" + details.TaxAmount + "</td></tr>";
                    }

                    string bsundry = "";
                    if (billsundry != null)
                    {
                        foreach (var bilsun in billsundry)
                        {

                            bsundry += "<tr class='border-top'>";
                            bsundry += "<td style='border: .1px solid #ccc;padding: 10px;'>" + bilsun.BillSundry + "</td>";
                            bsundry += "<td style='border: .1px solid #ccc;padding: 10px;' class='text-right'>" + Convert.ToDecimal(bilsun.BsAmount).ToString() + "</td>";
                            bsundry += "</tr>";

                            count++;
                        }
                    }
                    discount += bsundry;

                    discount += "<tr class='border-top'><td style='border: .1px solid #ccc;padding: 10px;'><b>Total المبلغ الإجمالي(AED)</b></td><td style='border: .1px solid #ccc;padding: 10px;' class='text-right'><b>" + details.GrandTotal + "</b></td></tr>";

                    string word = "<tr class='border-top'><td style='border: .1px solid #ccc;padding: 10px;font-size: 15px;' colspan='6'><strong>" + words + " </strong></td><td style='border: .1px solid #ccc;padding: 10px;'>Amount كمية</td><td style='border: .1px solid #ccc;padding: 10px;' class='text-right'>" + details.SubTotal + "</td></tr>";
                    string remarks = "<tr class='border-top'><td style='border: .1px solid #ccc;padding: 10px;' colspan='6' rowspan='" + count + "'><strong><u> Terms And Conditions :</u></strong><br/>" + details.TermsCondition.Replace("\n", "<br />") + " </td>";

                    sb.Append(word + remarks + discount);
                    sb.Append("</table>");

                    sb.Append("<table width='100%' style='border: .1px solid #ccc;border-collapse:collapse;'>");
                    sb.Append("<tr>");
                    sb.Append("<td align='left' width='50%' style='border: .1px solid #ccc;padding: 10px;vertical-align: top;'>");
                    sb.Append("<div style='font-size: 14px;text-align: left;'>Receiver's Signature:<br />توقيع المتلقي</div>");
                    sb.Append("</td>");
                    sb.Append("<td style='border: .1px solid #ccc;padding: 10px;vertical-align: top;'>");
                    sb.Append("<div style='font-size: 14px;text-align: left;'>");
                    sb.Append("For " + cdetails.CName + "");
                    sb.Append("</div>");
                    sb.Append("</td>");
                    sb.Append("</tr>");
                    sb.Append("</table>");
                }
            }
            return sb;
        }

        //bill no
        public long GetSeNo()
        {
            Int64 SENo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "Invoice").Select(a => a.number).FirstOrDefault();
            if ((db.SalesEntrys.Select(p => p.SENo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                if (number == 0)
                {
                    SENo = 1;
                }
                else
                {
                    SENo = number;
                }
            }
            else
            {
                SENo = db.SalesEntrys.Max(p => p.SENo + 1);
            }

            return SENo;
        }
        public string InvoiceNo(Int64 SENo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "Invoice").Select(a => a.prefix).FirstOrDefault();
            if (billNo == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "Invoice").Select(a => a.number).FirstOrDefault();
                if ((db.SalesEntrys.Select(p => p.SENo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    if (number == 0)
                    {
                        billNo = companyPrefix + 1;
                    }
                    else
                    {
                        billNo = companyPrefix + number;
                    }
                }
                else
                {
                    SENo = db.SalesEntrys.Max(p => p.SENo + 1);
                    billNo = companyPrefix + SENo;
                    if (BillExist(billNo))
                    {
                        billNo = InvoiceNo(SENo, billNo);
                    }
                }
            }
            else
            {
                SENo = SENo + 1;
                billNo = companyPrefix + SENo;
                if (BillExist(billNo))
                {
                    billNo = InvoiceNo(SENo, billNo);
                }

            }
            return billNo;
        }
        protected bool BillExist(string SENo)
        {
            var Exists = db.SalesEntrys.Any(c => c.BillNo == SENo);
            if (Exists)
            {
                return true;
            }
            else
            {
                return false;
            }

        }



        private string OrderNo(Int64 SENo = 0, string billNo = null)
        {
            DateTime today = DateTime.Now.Date;
            if (billNo == null)
            {
                if ((db.POSOrders.Where(p => p.OrderDate == today).Select(p => p.EntryNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = "1";
                }
                else
                {
                    SENo = db.POSOrders.Where(p => p.OrderDate == today).Max(p => p.EntryNo + 1);
                    billNo = Convert.ToString(SENo);
                    if (OrderExist(billNo))
                    {
                        billNo = OrderNo(SENo, billNo);
                    }
                }
            }
            else
            {
                SENo = SENo + 1;
                billNo = Convert.ToString(SENo);
                if (OrderExist(billNo))
                {
                    billNo = OrderNo(SENo, billNo);
                }

            }
            return billNo;
        }
        private bool OrderExist(string SENo)
        {
            DateTime today = DateTime.Now.Date;
            var Exists = db.POSOrders.Any(c => c.OrderNo == SENo && c.OrderDate == today);
            if (Exists)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        // order no without limiting days
        private string DOrderNo(Int64 SENo = 0, string billNo = null)
        {
            if (billNo == null)
            {
                if ((db.POSOrders.Select(p => p.EntryNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = "1";
                }
                else
                {
                    SENo = db.POSOrders.Max(p => p.EntryNo + 1);
                    billNo = Convert.ToString(SENo);
                    if (DOrderExist(billNo))
                    {
                        billNo = OrderNo(SENo, billNo);
                    }
                }
            }
            else
            {
                SENo = SENo + 1;
                billNo = Convert.ToString(SENo);
                if (DOrderExist(billNo))
                {
                    billNo = OrderNo(SENo, billNo);
                }

            }
            return billNo;
        }
        private bool DOrderExist(string SENo)
        {
            var Exists = db.POSOrders.Any(c => c.OrderNo == SENo);
            if (Exists)
            {
                return true;
            }
            else
            {
                return false;
            }

        }


        public JsonResult GetOrderNoByDate(string date)
        {
            DateTime? fdate = null;
            if (date != "")
            {
                fdate = DateTime.Parse(date, new CultureInfo("en-GB").DateTimeFormat);
            }

            var OrderNoInPOS = db.EnableSettings.Where(a => a.EnableType == "OrderNoInPOS").FirstOrDefault();
            var OrderNoInPOSC = OrderNoInPOS != null ? OrderNoInPOS.Status : Status.inactive;


            long voucherno = 1;
            if (OrderNoInPOSC == Status.active) {
                var porder = db.POSOrders.Select(p => p.EntryNo).AsEnumerable().DefaultIfEmpty(0).Max();
                if (porder != 0)
                {
                    voucherno = db.POSOrders.Max(p => p.EntryNo + 1);
                }
            }
            else
            {
                var porder = db.POSOrders.Where(p => p.OrderDate == fdate).Select(p => p.EntryNo).AsEnumerable().DefaultIfEmpty(0).Max();
                if (porder != 0)
                {
                    voucherno = db.POSOrders.Where(p => p.OrderDate == fdate).Max(p => p.EntryNo + 1);
                }
            }

            return Json(voucherno);

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
