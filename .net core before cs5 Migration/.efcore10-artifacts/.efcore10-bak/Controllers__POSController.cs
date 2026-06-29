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
using QuickSoft.ViewModel;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class POSController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public POSController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        [QkAuthorize(Roles = "Dev,POS List")]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Index2()
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
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.SalesEntrys
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                     join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                     join g in db.Users on a.CreatedBy equals g.Id
                     where a.SaleType == SaleType.POS
                     select new
                     {
                         a.SalesEntryId,
                         a.SENo,
                         a.BillNo,
                         a.PONo,
                         a.SEDate,
                         a.SEGrandTotal,
                         Customer = b.CustomerCode + " - " + b.CustomerName,
                         EmpName = d.FirstName + " " + d.LastName,
                         user = g.UserName,
                         a.CustomerType,
                         a.PayType,
                         a.SETaxAmount,
                         a.SESubTotal,

                         SEPaidAmount = (decimal?)c.SEPaidAmount??0,
                         BalanceAmt = a.SEGrandTotal - ((decimal?)c.SEPaidAmount ?? 0)
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.BillNo.ToString().ToLower().Equals(search.ToLower()));
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
        [QkAuthorize(Roles = "Dev,POS Entry")]
        public ActionResult Create()
        {
            var seNo = InvoiceNo();
            var model = new POSEntryViewModel
            {
                BillNo = seNo,
                SENo = GetSeNo(),
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

            var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
            var brcheck = enable != null ? enable.Status : Status.inactive;
            ViewBag.BCEnable = brcheck;

            var kbenable = db.EnableSettings.Where(a => a.EnableType == "TouchKeyboard").FirstOrDefault();
            var kbcheck = kbenable != null ? kbenable.Status : Status.inactive;
            ViewBag.KBEnable = Status.active;

            ViewBag.CFDURL = db.EnableSettings.Where(a => a.EnableType == "CFDURL").Select(a => a.TypeValue).FirstOrDefault();
            ViewBag.CFDURL = ViewBag.CFDURL == null ? "" : ViewBag.CFDURL;
            ViewBag.Portno = db.EnableSettings.Where(a => a.EnableType == "Portno").Select(a => a.TypeValue).FirstOrDefault();
            ViewBag.Portno = ViewBag.Portno == null ? "" : ViewBag.Portno;
            companySet();
            return View(model);
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,POS Entry")]
        public ActionResult Create(POSViewModel vmodel)
        {
            bool stat = false;
            string msg;
            if (!BillExist(Convert.ToString(vmodel.saleData.BillNo)))
            {
                // for print or save option
                string action = vmodel.fnval;
                //add to saleEntries
                var UserId = User.Identity.GetUserId();
                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                var today = Convert.ToDateTime(System.DateTime.Now);
                // payment method
                var PayMethod = vmodel.posData.PayMethod;
                //

                //  DateTime.Parse(DateTime.Now.ToString(), new CultureInfo("en-GB")); //
                var TaxAmount = vmodel.saleData.SETaxAmount;

                var PosDate = DateTime.Parse(vmodel.SEDate, new CultureInfo("en-GB"));

                var SEGrandTotal = vmodel.saleData.SEGrandTotal;
                var saleamount = SEGrandTotal - TaxAmount;

                //sales entry
               
                    SalesEntry SEentry = new SalesEntry
                    {
                        SENo = GetSeNo(),
                        BillNo = Convert.ToString(vmodel.saleData.SENo),
                        SEDate = PosDate,
                        SECashier = vmodel.saleData.SECashier,
                        SaleType = SaleType.POS,
                        PayType = vmodel.saleData.PayType,
                        SEItems = vmodel.saleData.SEItems,
                        SEItemQuantity = vmodel.saleData.SEItemQuantity,
                        SESubTotal = vmodel.saleData.SESubTotal,
                        SETax = vmodel.saleData.SETax,
                        SETaxAmount = TaxAmount,
                        SEGrandTotal = SEGrandTotal,
                        SENote = vmodel.saleData.SENote,
                        Print = 1,
                        SECreatedDate = today,
                        CreatedBy = UserId,
                        Status = 1,
                        Branch = BranchID,

                        OrderRefer = vmodel.saleData.OrderRefer,

                        //added 
                        SEDiscount = vmodel.saleData.SEDiscount,


                    };
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
                try
                { 
                db.SalesEntrys.Add(SEentry);
                    db.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }

                Int64 salesEntryId = SEentry.SalesEntryId;
                if (vmodel.saleData.OrderRefer != null && vmodel.saleData.OrderRefer != 0)
                {
                    POSOrder POS = db.POSOrders.Find(vmodel.saleData.OrderRefer);
                    POS.OrderStatus = OrderStatus.Payment;
                    db.Entry(POS).State = EntityState.Modified;
                    db.SaveChanges();
                }
            
                //walkin customer
                if (vmodel.saleData.CustomerType == CustomerType.Walking )
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
                    dr["ItemDiscount"] = arr.ItemDiscount;
                    dr["ItemTax"] = arr.ItemTax;
                    dr["ItemTaxAmount"] = arr.ItemTaxAmount;
                    dr["ItemTotalAmount"] = arr.ItemTotalAmount;
                    dr["itemNote"] = (arr.itemNote == null || arr.itemNote == "undefined") ? "" : arr.itemNote;
                
                    dr["SaleEntry"] = salesEntryId;
                    dr["Item"] = arr.Item;
                    dr["Type"] = 0;
                    /// dr["editable"] = 0;
                    dtItem.Rows.Add(dr);
                    if (arr.Note != null && arr.Note != "" && arr.Note != "undefined")
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
                            var buTaxAmount = (ItemSubTotal * bu.ItemTax)/ 100;
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
                            dbu["itemNote"] = "Bundle Item";
                            dbu["Note"] = "";
                            dbu["SaleEntry"] = salesEntryId;
                            dbu["Item"] = bu.Item;
                            dtItem.Rows.Add(dbu);
                        }
                    }
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


                //billsundry only for round off
                if (vmodel.roundoff != 0)
                {
                    string bsResult = string.Empty;
                    DataTable BsEntry = new DataTable();
                    BsEntry.Columns.Add("SalesEntry");
                    BsEntry.Columns.Add("BillSundry");
                    BsEntry.Columns.Add("BsValue");
                    BsEntry.Columns.Add("AmountType");
                    BsEntry.Columns.Add("BsType");
                    BsEntry.Columns.Add("BsAmount");
                    
                    DataRow drw = BsEntry.NewRow();
                    drw["SalesEntry"] = salesEntryId;
                    drw["BsValue"] = null;
                    drw["AmountType"] = 0;
                    drw["BsType"] = 1;
                    if (vmodel.roundoff < 0)
                    {
                        // round off (+)
                        drw["BillSundry"] = 1;
                        drw["BsAmount"] = 0 - vmodel.roundoff;
                    }
                    else
                    {
                        // round off (-)
                        drw["BillSundry"] = 2;
                        drw["BsAmount"] = vmodel.roundoff;
                    }
                    BsEntry.Rows.Add(drw);
                    ////// create parameter 
                    SqlParameter parameter1 = new SqlParameter("@TableType", BsEntry);
                    parameter1.SqlDbType = SqlDbType.Structured;
                    parameter1.TypeName = "TableTypeSEBillSundry";
                    //// execute sp sql 
                    string sql1 = String.Format("EXEC {0} {1};", "SP_InsertSEBillSundry", "@TableType");
                    //// execute sql 
                    db.Database.ExecuteSqlRaw(sql1, parameter1);
                }
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
                if (PayMethod == "Credit")
                {
                    SEpay.SEPaidAmount = 0;
                }
                else
                {
                    SEpay.SEPaidAmount = vmodel.saleData.SEGrandTotal;
                }
                db.SEPayments.Add(SEpay);
                db.SaveChanges();
                long? AccId = null;
                if (PayMethod == "Card")
                {
                    var card = db.PaymentMethods.Where(c => c.PaymentMethodId==2).SingleOrDefault();
                    AccId = card.AccountId;
                }
                PosData SEpos = new PosData
                {
                    SalesEntry = salesEntryId,
                    PayMethod = PayMethod,
                    PayMode= PayMethod == "Card" ? vmodel.posData.PayMode : null,
                    TotTender = vmodel.posData.TotTender,
                    ChangeDue = vmodel.posData.ChangeDue,
                    Account = AccId
                };
                db.PPosDatas.Add(SEpos);
                db.SaveChanges();
                
                decimal amount = Convert.ToDecimal(vmodel.salePayment.SEPaidAmount);
                Int64 custAccID = db.Customers.Where(a => a.CustomerID == SEentry.Customer).Select(a => a.Accounts).FirstOrDefault();
                Int64 saleAccId = db.Accountss.Where(a => a.Group == 15).Select(a => a.AccountsID).SingleOrDefault();
                Int64 cashAccId = db.Accountss.Where(a => a.Group == 9).Select(a => a.AccountsID).FirstOrDefault();
                Int64 VATOutput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Output").Select(a => a.AccountsID).SingleOrDefault();
                if (PayMethod == "Card")
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
                if (vmodel.salePayment.SEPaidAmount > 0 && PayMethod != "Credit")
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
                
                //add trasaction to sale account with sale entry credit amount
                com.addAccountTrasaction(0, saleamount, saleAccId, "Sale", salesEntryId, DC.Credit, PosDate);
                //add sale trasaction with customer debt amount
                com.addAccountTrasaction(vmodel.saleData.SEGrandTotal, 0, custAccID, "Sale", salesEntryId, DC.Debit, PosDate);
                // add vat output in account transaction
                if (TaxAmount > 0)
                    com.addAccountTrasaction(0,TaxAmount, VATOutput, "Sale", salesEntryId, DC.Credit, date);

                if (vmodel.salePayment.SEPaidAmount > 0 && PayMethod != "Credit")
                {
                    //if payment
                    com.addAccountTrasaction(0, amount, custAccID, "Sale Payment", salesEntryId, DC.Credit, PosDate);
                    com.addAccountTrasaction(amount, 0, cashAccId, "Sale Payment", salesEntryId, DC.Debit, PosDate);
                }

                com.addlog(LogTypes.Created, UserId, "SalesEntry", "SalesEntrys", findip(), salesEntryId, "Successfully Submitted POS Entry");
                var update = com.CusPayment(custAccID, date, BranchID, UserId);

                //add
                if (action == "print" || action == "print_order")
                {
                    string sedate = SEentry.SEDate.ToString("dd-MM-yyyy");
                    if (vmodel.saleData.CustomerType == CustomerType.Walking)
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
                                         Date = a.SEDate,
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
                                         TermsAndCondition = db.TermsAndConditionss.Where(i => i.ConditionTypeID == "sales").Select(i => i.TermsCondit).FirstOrDefault(),

                                     }).FirstOrDefault();
                        var item = (from a in db.SEItemss
                                    join b in db.Items on a.Item equals b.ItemID
                                    join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                                    from c in primary.DefaultIfEmpty()
                                    join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                                    from d in second.DefaultIfEmpty()
                                    join f in db.ItemUnits on a.ItemUnit equals f.ItemUnitID into unit
                                    from f in unit.DefaultIfEmpty()
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
                                        ItemNote = a.itemNote,
                                        ItemWithCode = b.ItemCode + " - " + b.ItemName,
                                        b.ItemUnitID,
                                        b.SubUnitId,
                                        PriUnit = c.ItemUnitName,
                                        SubUnit = d.ItemUnitName,
                                        b.ItemArabic,
                                        b.ItemType,
                                         a.itemNote,
                                        ItemUnitName=f.ItemUnitName,
                                        bundleitem =  (from ab in db.SEItemss
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
                                                            bb.ItemType
                                                        }).ToList()
                                    }).ToList();

                        var billsundry = db.SEBillSundrys.Where(n => n.SalesEntry == salesEntryId).Select(b => new
                        {
                            AmountType = b.AmountType,
                            BsAmount = b.BsAmount,
                            BsType = b.BsType,
                            BsValue = b.BsValue != null ? b.BsValue : 0,
                        }).FirstOrDefault();

                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item = item, sales = sales, PosDate = vmodel.posData , billsundry= billsundry } };
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
                                         Date = a.SEDate,
                                         Note = a.SENote,
                                         CustomerType = a.CustomerType,
                                         Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                                         SEDiscount = a.SEDiscount,
                                         SETotal = a.SEDiscount + a.SEGrandTotal,
                                         SEGrandTotal = a.SEGrandTotal,
                                         SEPaidAmount = d.SEPaidAmount,
                                         SEDueAmount = a.SEGrandTotal - d.SEPaidAmount,
                                         Address = c.Address + " " + c.City + " " + c.State + " " + c.Country + " " + c.Zip,
                                         Email = c.EmailId,
                                         Phone = c.Phone,
                                         Mobile = c.Mobile,
                                         TRN = b.TaxRegNo,
                                         SETaxAmount = a.SETaxAmount,
                                         TermsAndCondition = db.TermsAndConditionss.Where(i => i.ConditionTypeID == "sales").Select(i => i.TermsCondit).FirstOrDefault(),


                                     }).FirstOrDefault();

                        var item = (from a in db.SEItemss
                                    join b in db.Items on a.Item equals b.ItemID
                                    join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                                    from c in primary.DefaultIfEmpty()
                                    join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                                    from d in second.DefaultIfEmpty()
                                    join f in db.ItemUnits on a.ItemUnit equals f.ItemUnitID into unit
                                    from f in unit.DefaultIfEmpty()
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
                                        ItemNote = a.itemNote,
                                        ItemWithCode = b.ItemCode + " - " + b.ItemName,
                                        b.ItemUnitID,
                                        b.SubUnitId,
                                        PriUnit = c.ItemUnitName,
                                        SubUnit = d.ItemUnitName,
                                        b.ItemArabic,
                                        b.ItemType,
                                         a.itemNote,
                                        ItemUnitName = f.ItemUnitName,
                                        bundleitem = (from ab in db.SEItemss
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
                                                          bb.ItemType
                                                      }).ToList()
                                    }).ToList();
                        var billsundry = db.SEBillSundrys.Where(n => n.SalesEntry == salesEntryId).Select(b => new
                        {
                            AmountType = b.AmountType,
                            BsAmount = b.BsAmount,
                            BsType = b.BsType,
                            BsValue = b.BsValue != null ? b.BsValue : 0,
                        }).FirstOrDefault();

                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, sales, PosDate = vmodel.posData, billsundry= billsundry } };
                    }
                }
                else
                {
                    msg = "Successfully Completed POS Entry.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
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
            if (Saleentry == null)
            {
                return NotFound();
            }
            Int64 cashier = Convert.ToInt64(Saleentry.SECashier);
            Int64 customer = Saleentry.Customer;

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

            vmodel = (from b in db.SalesEntrys
                      join c in db.SEPayments on b.SalesEntryId equals c.SalesEntry
                      join d in db.WalkinCustomers on b.SalesEntryId equals d.SalesEntryId into wlk
                      from d in wlk.DefaultIfEmpty()
                      join e in db.PPosDatas on b.SalesEntryId equals e.SalesEntry into pos
                      from e in pos.DefaultIfEmpty()
                      join g in db.SEBillSundrys on b.SalesEntryId equals  g.SalesEntry  into bills
                      from g in bills.DefaultIfEmpty()
                      where b.SalesEntryId == id && (g.BsAmount==null||g.BillSundry==1||g.BillSundry==2 )
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
                         // taxAFdisc =(bool)b.taxAFdisc,
                          SEPaidAmount = c.SEPaidAmount,
                          PayType = e.PayMethod,
                          PayMode=e.PayMode,
                          CustomerType = b.CustomerType,
                          CustomerName = d.CustomerName,
                          MobileNo = d.MobileNo,
                          
                          RoundOff = (decimal)((g.BsAmount == null)?0:(g.BillSundry == 1)?0-g.BsAmount: g.BsAmount)
                      }).FirstOrDefault();

            var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
            var brcheck = enable != null ? enable.Status : Status.inactive;
            ViewBag.BCEnable = brcheck;

            var kbenable = db.EnableSettings.Where(a => a.EnableType == "TouchKeyboard").FirstOrDefault();
            var kbcheck = kbenable != null ? kbenable.Status : Status.inactive;
            ViewBag.KBEnable = kbcheck;

            companySet();
            ViewBag.posdata = db.PPosDatas.Where(a => a.SalesEntry == id).FirstOrDefault();
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
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            var today = Convert.ToDateTime(System.DateTime.Now);
            //

            //  DateTime.Parse(DateTime.Now.ToString(), new CultureInfo("en-GB")); //
            var TaxAmount = vmodel.saleData.SETaxAmount;
            var PosDate = DateTime.Parse(vmodel.SEDate, new CultureInfo("en-GB"));

            var SEGrandTotal = vmodel.saleData.SEGrandTotal;
            var saleamount = SEGrandTotal - TaxAmount;
            //sales entry
            SalesEntry entry = db.SalesEntrys.Find(id);


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
            entry.Print = 1;
            entry.Status = 1;
            entry.Branch = BranchID;
            
            //added
            entry.SEDiscount = vmodel.saleData.SEDiscount;


            //walkin customer
            if (vmodel.saleData.CustomerType == CustomerType.Walking)
            {
                entry.Customer = 0;
                entry.CustomerType = CustomerType.Walking;
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
            if (vmodel.saleData.CustomerType == CustomerType.Walking && (vmodel.wCustomer.CustomerName != null || vmodel.wCustomer.MobileNo != null))
            {
                var wlkCust = db.WalkinCustomers.Where(a => a.SalesEntryId == salesEntryId).FirstOrDefault();
                if (wlkCust != null)
                {
                    db.WalkinCustomers.RemoveRange(db.WalkinCustomers.Where(a => a.SalesEntryId == salesEntryId));
                }

                WalkinCustomer wc = new WalkinCustomer
                {
                    SalesEntryId = salesEntryId,
                    CustomerName = vmodel.wCustomer.CustomerName,
                    MobileNo = vmodel.wCustomer.MobileNo
                };
                db.WalkinCustomers.Add(wc);
                db.SaveChanges();
            }


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
            dtItem.Columns.Add("editable");

            foreach (var arr in vmodel.seItems)
            {
                DataRow dr = dtItem.NewRow();
                dr["ItemUnit"] = arr.ItemUnit;
                dr["ItemUnitPrice"] = arr.ItemUnitPrice;
                dr["ItemQuantity"] = arr.ItemQuantity;
                dr["ItemSubTotal"] = arr.ItemSubTotal;
                dr["ItemDiscount"] = arr.ItemDiscount;
                dr["ItemTax"] = arr.ItemTax;
                dr["ItemTaxAmount"] = arr.ItemTaxAmount;
                dr["ItemTotalAmount"] = arr.ItemTotalAmount;
                dr["itemNote"] = (arr.itemNote == null || arr.itemNote == "undefined") ? "" : arr.itemNote;
                dr["SaleEntry"] = salesEntryId;
                dr["Item"] = arr.Item;
                dr["editable"] = 0;
                dtItem.Rows.Add(dr);
                if (arr.Note != null && arr.Note != "" && arr.Note != "undefined")
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
                        dbu["itemNote"] = "Bundle Item";
                        dbu["SaleEntry"] = salesEntryId;
                        dbu["Item"] = bu.Item;
                        dbu["type"] = 0;

                  



                        dtItem.Rows.Add(dbu);
                    }
                }
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

            //billsundry only for round off
            if (vmodel.roundoff != 0)
            {
                var billsundry = db.SEBillSundrys.Where(a => a.SalesEntry == salesEntryId).FirstOrDefault();
                if (billsundry != null)
                {
                    db.SEBillSundrys.RemoveRange(db.SEBillSundrys.Where(a => a.SalesEntry == salesEntryId));
                }

                string bsResult = string.Empty;
                DataTable BsEntry = new DataTable();
                BsEntry.Columns.Add("SalesEntry");
                BsEntry.Columns.Add("BillSundry");
                BsEntry.Columns.Add("BsValue");
                BsEntry.Columns.Add("AmountType");
                BsEntry.Columns.Add("BsType");
                BsEntry.Columns.Add("BsAmount");

                DataRow drw = BsEntry.NewRow();
                drw["SalesEntry"] = salesEntryId;
                drw["BsValue"] = null;
                drw["AmountType"] = 0;
                drw["BsType"] = 1;
                if (vmodel.roundoff < 0)
                {
                    // round off (+)
                    drw["BillSundry"] = 1;
                    drw["BsAmount"] = 0 - vmodel.roundoff;
                }
                else
                {
                    // round off (-)
                    drw["BillSundry"] = 2;
                    drw["BsAmount"] = vmodel.roundoff;
                }
                BsEntry.Rows.Add(drw);
                ////// create parameter 
                SqlParameter parameter1 = new SqlParameter("@TableType", BsEntry);
                parameter1.SqlDbType = SqlDbType.Structured;
                parameter1.TypeName = "TableTypeSEBillSundry";
                //// execute sp sql 
                string sql1 = String.Format("EXEC {0} {1};", "SP_InsertSEBillSundry", "@TableType");
                //// execute sql 
                db.Database.ExecuteSqlRaw(sql1, parameter1);
            }





            PosData SEpos = db.PPosDatas.Where(a => a.SalesEntry == salesEntryId).FirstOrDefault();
            // payment method
            var PayMethod = vmodel.posData.PayMethod;
            long? AccId = null;
            if (PayMethod == "Card")
            {
                var card = db.PaymentMethods.Where(c => c.PaymentMethodId == 2).SingleOrDefault();
                    AccId = card.AccountId;
            }
            if (SEpos != null)
            {
                SEpos.PayMethod = vmodel.posData.PayMethod;
                SEpos.Account = AccId;
                SEpos.PayMethod = vmodel.posData.PayMethod;
                SEpos.TotTender = vmodel.posData.TotTender;
                SEpos.ChangeDue = vmodel.posData.ChangeDue;
                SEpos.PayMode = PayMethod == "Card" ? vmodel.posData.PayMode : null;
                db.Entry(SEpos).State = EntityState.Modified;
                db.SaveChanges();
            }

            //SEPayment
            SEPayment SEpay = db.SEPayments.Where(a => a.SalesEntry == salesEntryId).FirstOrDefault();

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
            SEpay.SEPaidAmount = payamount;
            db.Entry(SEpay).State = EntityState.Modified;
            db.SaveChanges();

            decimal amount = Convert.ToDecimal(vmodel.salePayment.SEPaidAmount);
            Int64 custAccID = custAccID = db.Customers.Where(a => a.CustomerID == entry.Customer).Select(a => a.Accounts).FirstOrDefault();
            Int64 saleAccId = db.Accountss.Where(a => a.Group == 15).Select(a => a.AccountsID).SingleOrDefault();
            Int64 cashAccId = db.Accountss.Where(a => a.Group == 9).Select(a => a.AccountsID).SingleOrDefault();
            Int64 VATOutput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Output").Select(a => a.AccountsID).SingleOrDefault();
            if (PayMethod == "Card")
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
            if (payamount > 0 || vmodel.saleData.CustomerType == CustomerType.Walking)
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
            com.addAccountTrasaction(0, saleamount, saleAccId, "Sale", salesEntryId, DC.Credit, PosDate);
            //add sale trasaction with customer debt amount
            com.addAccountTrasaction(vmodel.saleData.SEGrandTotal, 0, custAccID, "Sale", salesEntryId, DC.Debit, PosDate);
            // add vat output in account transaction
            if (TaxAmount > 0)
                com.addAccountTrasaction(0,TaxAmount, VATOutput, "Sale", salesEntryId, DC.Credit, date);

            if (payamount > 0 || vmodel.saleData.CustomerType == CustomerType.Walking)
            {
                //if payment
                com.addAccountTrasaction(0, amount, custAccID, "Sale Payment", salesEntryId, DC.Credit, PosDate);
                com.addAccountTrasaction(amount, 0, cashAccId, "Sale Payment", salesEntryId, DC.Debit, PosDate);
            }
            com.addlog(LogTypes.Created, UserId, "SalesEntry", "SalesEntrys", findip(), salesEntryId, "Successfully Updated POS Entry");

                var update = com.CusPayment(custAccID, date, BranchID, UserId);
            if (action == "print" || action == "print_order")
            {
                string sedate = entry.SEDate.ToString("dd-MM-yyyy");
                if (vmodel.saleData.CustomerType == CustomerType.Walking)
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
                                     Date = a.SEDate,
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
                                     TermsAndCondition = db.TermsAndConditionss.Where(i => i.ConditionTypeID == "sales").Select(i => i.TermsCondit).FirstOrDefault(),


                                 }).FirstOrDefault();
                    var item = (from a in db.SEItemss
                                join b in db.Items on a.Item equals b.ItemID
                                join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                                from c in primary.DefaultIfEmpty()
                                join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                                from d in second.DefaultIfEmpty()
                                join f in db.ItemUnits on a.ItemUnit equals f.ItemUnitID into unit
                                from f in unit.DefaultIfEmpty()
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
                                    ItemNote = a.itemNote,
                                    ItemWithCode = b.ItemCode + " - " + b.ItemName,
                                    b.ItemUnitID,
                                    b.SubUnitId,
                                    PriUnit = c.ItemUnitName,
                                    SubUnit = d.ItemUnitName,
                                    b.ItemArabic,
                                    b.ItemType,
                                    ItemUnitName = f.ItemUnitName,
                                    bundleitem = (from ab in db.SEItemss
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
                    var billsundry = db.SEBillSundrys.Where(n => n.SalesEntry == salesEntryId).Select(b => new
                    {
                        AmountType = b.AmountType,
                        BsAmount = b.BsAmount,
                        BsType = b.BsType,
                        BsValue = b.BsValue != null ? b.BsValue : 0,
                    }).FirstOrDefault();
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item = item, sales = sales, PosDate = vmodel.posData, billsundry = billsundry } };
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
                                     Date = a.SEDate,
                                     Note = a.SENote,
                                     CustomerType = a.CustomerType,
                                     Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                                     SEDiscount = a.SEDiscount,
                                     SETotal = a.SEDiscount + a.SEGrandTotal,
                                     SEGrandTotal = a.SEGrandTotal,
                                     SEPaidAmount = d.SEPaidAmount,
                                     SEDueAmount = a.SEGrandTotal - d.SEPaidAmount,
                                     Address = c.Address + " " + c.City + " " + c.State + " " + c.Country + " " + c.Zip,
                                     Email = c.EmailId,
                                     Phone = c.Phone,
                                     Mobile = c.Mobile,
                                     TRN = b.TaxRegNo,
                                     SETaxAmount = a.SETaxAmount,
                                     TermsAndCondition = db.TermsAndConditionss.Where(i => i.ConditionTypeID == "sales").Select(i => i.TermsCondit).FirstOrDefault(),


                                 }).FirstOrDefault();

                    var item = (from a in db.SEItemss
                                join b in db.Items on a.Item equals b.ItemID
                                join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                                from c in primary.DefaultIfEmpty()
                                join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                                from d in second.DefaultIfEmpty()
                                join f in db.ItemUnits on a.ItemUnit equals f.ItemUnitID into unit
                                from f in unit.DefaultIfEmpty()
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
                                    ItemNote = a.itemNote,
                                    ItemWithCode = b.ItemCode + " - " + b.ItemName,
                                    b.ItemUnitID,
                                    b.SubUnitId,
                                    PriUnit = c.ItemUnitName,
                                    SubUnit = d.ItemUnitName,
                                    b.ItemArabic,
                                    b.ItemType,
                                    a.itemNote,
                                    ItemUnitName = f.ItemUnitName,
                                    bundleitem = (from ab in db.SEItemss
                                                  join bb in db.Items on ab.Item equals bb.ItemID
                                                  join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                                                  from cb in primary.DefaultIfEmpty()
                                                  join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                                                  from bd in second.DefaultIfEmpty()
                                                  where ab.SalesEntry == salesEntryId 
                                                  && a.Item == ab.ItemDiscount
                                                  select new
                                                  {
                                                      ab.Item,
                                                      ab.ItemQuantity,
                                                      ab.ItemUnit,
                                                      ab.ItemUnitPrice,
                                                      ab.ItemTax,
                                                      ab.ItemSubTotal,
                                                      ab.ItemTaxAmount,
                                                      ItemDiscount = 0,
                                                      ab.ItemTotalAmount,
                                                      ItemCode = bb.ItemCode,
                                                      ItemName = bb.ItemName,
                                                      ItemNote = ab.itemNote,
                                                      ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                      bb.ItemUnitID,
                                                      bb.SubUnitId,
                                                      PriUnit = cb.ItemUnitName,
                                                      SubUnit = bd.ItemUnitName,
                                                      bb.ItemArabic,
                                                  }).ToList()
                                }).ToList();
                    var billsundry = db.SEBillSundrys.Where(n => n.SalesEntry == salesEntryId).Select(b => new
                    {
                        AmountType = b.AmountType,
                        BsAmount = b.BsAmount,
                        BsType = b.BsType,
                        BsValue = b.BsValue != null ? b.BsValue : 0,
                    }).FirstOrDefault();
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, sales, PosDate = vmodel.posData, billsundry = billsundry } };
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

            var customerId = db.SalesEntrys.Where(a => a.SalesEntryId == id).Select(a => a.Customer).First();


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

            var customerId = db.SalesEntrys.Where(a => a.SalesEntryId == id).Select(a => a.Customer).First();


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
            //             where a.SalesEntryId == SalesEntryID
            //                 CustomerName = f.CustomerName,
            //                 SENo = a.SENo,
            //                 PONo = a.PONo,
            //                 BillNo = a.BillNo,
            //                 Date = a.SEDate,
            //                 Note = a.SENote,
            //                 a.SENote,
            //                 CustomerType = a.CustomerType,
            //                 SEDiscount = a.SEDiscount,
            //                 SETotal = a.SEDiscount + a.SEGrandTotal,
            //                 SEGrandTotal = a.SEGrandTotal,
            //                 SEPaidAmount = a.SEGrandTotal,
            //                 SEDueAmount = 0,
            //                 SETaxAmount = a.SETaxAmount,
            //                 Address = "",
            //                 Email = "",
            //                 Phone = "",
            //                 Mobile = f.MobileNo,
            //                 TRN = "",
            //                 SubTotal = a.SESubTotal,
            //                 WaiterName=e.FirstName+" "+e.LastName,
            //                 a.SECashier,

            //                 RoundOff=g.BsAmount


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
                           

                            PriPurchase = (decimal?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPurchase = (decimal?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriSale = (decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSale = (decimal?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,


                            PriPReturn = (decimal?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPReturn = (decimal?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriSReturn = (decimal?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSReturn = (decimal?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                            //stock adjustment---
                            PriAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Add && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            subLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Less && b.SubUnitId != b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,

                            // production ----
                            bundle = (from ab in db.SEItemss
                                          join bb in db.Items on ab.Item equals bb.ItemID
                                          join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                                          from cb in primary.DefaultIfEmpty()
                                          join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                                          from bd in second.DefaultIfEmpty()
                                          where ab.SalesEntry == SalesEntryID 
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
                        }).AsEnumerable().Select(o => new
                        {
                            o.Item,
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
                            o.note,
                            o.itemNote,
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

                            pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj) - (o.PriSale + o.PriPReturn + o.PriLessAdj)),
                            subtotal = ((o.SubPurchase + o.SubSReturn + o.SubAddAdj) - (o.SubSale + o.SubPReturn + o.subLessAdj)),
                            total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj) - (o.PriSale + o.PriPReturn + o.PriLessAdj)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubAddAdj) - (o.SubSale + o.SubPReturn + o.subLessAdj)),
                            type = (o.bundle.Any()) ? "Bundle" : "",
                            o.bundle
                        }).ToList();
            var data = new { item = item };
            return Json(data);
        }


        [HttpGet]
        [QkAuthorize(Roles = "Dev,View POS")]
        public ActionResult Details(long? id)
        {
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

        #region hold orders 

        [HttpPost]
        [QkAuthorize(Roles = "Dev,POS Entry")]
        public ActionResult Holdit(POSViewModel vmodel)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            var today = Convert.ToDateTime(System.DateTime.Now);
            var TaxAmount = vmodel.saleData.SETaxAmount;
            var PosDate = DateTime.Parse(vmodel.SEDate, new CultureInfo("en-GB"));
            var PayMethod = vmodel.posData.PayMethod;
            Int64 orderId;
            if (vmodel.saleData.OrderRefer == null || vmodel.saleData.OrderRefer == 0)
            {
                //sales orders
                POSOrder POS = new POSOrder
                {
                    BillNo = vmodel.saleData.BillNo,
                    OrderDate = PosDate,
                    WaiterId = vmodel.saleData.SECashier,
                    ItemCount = vmodel.saleData.SEItems,
                    Quantity = vmodel.saleData.SEItemQuantity,
                    SubTotal = vmodel.saleData.SESubTotal,
                    Tax = vmodel.saleData.SETax,
                    TaxAmount = TaxAmount,
                    NetPayable = vmodel.saleData.SEGrandTotal,
                    OrderNote = vmodel.saleData.BillNo,
                    CreatedDate = today,
                    CreatedBy = UserId,
                    Branch = BranchID,
                    //roundoff = vmodel.roundoff,
                    OrderStatus = OrderStatus.Hold,
                    
                    Discount = vmodel.saleData.SEDiscount,
                };
                if (vmodel.saleData.CustomerType == CustomerType.Walking)
                {
                    POS.Customer = 0;
                    POS.CustomerType = CustomerType.Walking;
                    POS.custName = vmodel.wCustomer.CustomerName;
                    POS.custMob = vmodel.wCustomer.MobileNo;
                }
                else
                {
                    POS.Customer = vmodel.saleData.Customer;
                    POS.CustomerType = CustomerType.Customer;
                }
                db.POSOrders.Add(POS);
                db.SaveChanges();
                orderId = POS.POSOrderId;
            }
            else
            {
                POSOrder POS = db.POSOrders.Find(vmodel.saleData.OrderRefer);
                POS.BillNo = vmodel.saleData.BillNo;
                POS.OrderDate = PosDate;
                POS.WaiterId = vmodel.saleData.SECashier;
                POS.ItemCount = vmodel.saleData.SEItems;
                POS.Quantity = vmodel.saleData.SEItemQuantity;
                POS.SubTotal = vmodel.saleData.SESubTotal;
                POS.Tax = vmodel.saleData.SETax;
                POS.TaxAmount = TaxAmount;
                POS.NetPayable = vmodel.saleData.SEGrandTotal;
                POS.OrderNote = vmodel.saleData.SENote;
                POS.CreatedDate = today;
                POS.CreatedBy = UserId;
                POS.Branch = BranchID;
                POS.OrderStatus = OrderStatus.Hold;
                POS.Discount = vmodel.saleData.SEDiscount;
                if (vmodel.saleData.CustomerType == CustomerType.Walking)
                {
                    POS.Customer = 0;
                    POS.CustomerType = CustomerType.Walking;
                    POS.custName = vmodel.wCustomer.CustomerName;
                    POS.custMob = vmodel.wCustomer.MobileNo;
                }
                else
                {
                    POS.Customer = vmodel.saleData.Customer;
                    POS.CustomerType = CustomerType.Customer;
                }
                db.Entry(POS).State = EntityState.Modified;
                db.SaveChanges();
                orderId = POS.POSOrderId;
                var OrderItem = db.POSOrderItems.Where(a => a.OrderId == orderId).FirstOrDefault();
                if (OrderItem != null)
                {
                    db.POSOrderItems.RemoveRange(db.POSOrderItems.Where(a => a.OrderId == orderId));
                    db.SaveChanges();
                }
            }
            // add to order items
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
            dtItem.Columns.Add("ItemNote");
            dtItem.Columns.Add("Note");
            dtItem.Columns.Add("PrintCount");
            dtItem.Columns.Add("Prints");
            dtItem.Columns.Add("OrderId");
            dtItem.Columns.Add("ItemId");
            dtItem.Columns.Add("editable");

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
                dr["ItemNote"] = arr.itemNote == null ? "" : arr.itemNote;
                dr["Note"] = arr.Note;
                dr["PrintCount"] = 0;
                dr["Prints"] = 0;
                dr["OrderId"] = orderId;
                dr["ItemId"] = arr.Item;
                dr["editable"] = 0;
                dtItem.Rows.Add(dr);
                if (arr.Note != null && arr.Note != "" && arr.Note != "undefined")
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
                        dbu["ItemNote"] = "Bundle Item";
                        dbu["Note"] = "";
                        dbu["PrintCount"] = 0;
                        dbu["Prints"] = 0;
                        dbu["OrderId"] = orderId;
                        dbu["ItemId"] = bu.Item;
                        dbu["editable"] = 1;
                        dtItem.Rows.Add(dbu);
                    }
                }
            }
            
            ////// create parameter 
            SqlParameter parameter = new SqlParameter("@TableType", dtItem);
            parameter.SqlDbType = SqlDbType.Structured;
            parameter.TypeName = "TableTypePOSOrderItems";
            //// execute sp sql 
            string sql = String.Format("EXEC {0} {1};", "SP_InsertPOSOrderItems", "@TableType");
            //// execute sql 
            db.Database.ExecuteSqlRaw(sql, parameter);
            com.addlog(LogTypes.Created, UserId, "SalesEntry", "POSOrder", findip(), orderId, "Successfully Hold POS Entry");
            msg = "Order Hold Successfully.";
            stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        public JsonResult holdOrders()
        {
            bool checkdel = User.IsInRole("Dev") || (User.IsInRole("POS Delete"));// Delete POS Order
            var tables = (from a in db.POSOrders
                          join d in db.Employees on a.WaiterId equals d.EmployeeId into waiter
                          from d in waiter.DefaultIfEmpty()
                          where a.OrderStatus == OrderStatus.Hold
                          select new
                          {
                              a.POSOrderId,
                              a.OrderStatus,
                              a.OrderNo,
                              a.NetPayable,
                              a.ItemCount,
                              a.OrderDate,
                              waiter = d.FirstName,
                              DelPer = checkdel
                          }).ToList();

            return Json(tables);
        }
        [HttpPost]
        public JsonResult getOrderById(int orderId)
        {

            var seNo = InvoiceNo();
            var SENo = GetSeNo();
            var tables = (from a in db.POSOrders
                          join b in db.Customers on a.Customer equals b.CustomerID into cust
                          from b in cust.DefaultIfEmpty()
                          join d in db.Employees on a.WaiterId equals d.EmployeeId into emp
                          from d in emp.DefaultIfEmpty()
                          where a.POSOrderId == orderId
                          select new
                          {
                              Id = a.POSOrderId,
                              a.OrderStatus,
                              a.OrderNo,
                              a.NetPayable,
                              a.OrderDate,
                              a.CustomerType,
                              a.Customer,
                              a.WaiterId,
                              a.OrderType,
                              a.dcharge,
                              WaiterName = (d.FirstName + " " + d.LastName),
                              CustName =  b.CustomerName,
                              Mobile = (a.CustomerType == CustomerType.Walking ? a.custMob : ""),
                              a.ItemCount,
                             // a.taxAFdisc,
                              a.Discount,
                              //a.roundoff,
                              BillNo = seNo,
                              SENo,
                          }).FirstOrDefault();
            return Json(tables);
        }

        [HttpPost]
        public ActionResult GetOrderItems(long orderId)
        {
            var ConD = (from a in db.POSOrderItems
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()
                        where a.OrderId == orderId 
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
                            a.ItemNote,
                            a.Note,
                            b.ItemCode,
                            b.ItemName,
                            b.ItemType,
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
                            
                            bundle = (from ab in db.BundleItems
                                      join bb in db.Items on ab.ItemId equals bb.ItemID
                                      join cb in db.ItemUnits on ab.ItemUnit equals cb.ItemUnitID into primarys
                                      from cb in primarys.DefaultIfEmpty()
                                      join gb in db.ItemBundles on ab.ItemBundle equals gb.ItemBundleId
                                      join hb in db.Items on gb.mainItem equals hb.ItemID
                                      where hb.ItemID == a.Item
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
                                          ItemUnit = ab.ItemUnit,
                                          Item = ab.ItemId
                                      }).ToList()
                        }).AsEnumerable().Select(o => new
                        {
                            o.Item,
                            o.ItemQuantity,
                            o.ItemUnit,
                            o.ItemUnitPrice,
                            o.ItemTax,
                            o.ItemSubTotal,
                            o.ItemTaxAmount,
                            o.ItemDiscount,
                            o.ItemTotalAmount,
                            o.ItemNote,
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            //o.note,
                            Note=o.Note+"|"+o.ItemUnitPrice.ToString("G29"),
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
                           // price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                          price=o.ItemUnitPrice,
                            o.KeepStock,

                            PriPurchase = (o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)),
                            SubPurchase = (o.SubPurchase % o.ConFactor),

                            PriSale = (o.PriSale + (int)(o.SubSale / o.ConFactor)),
                            SubSale = (o.SubSale % o.ConFactor),

                            PriPReturn = (o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)),
                            SubPReturn = (o.SubPReturn % o.ConFactor),

                            PriSReturn = (o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)),
                            SubSReturn = (o.SubSReturn % o.ConFactor),

                            pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj) - (o.PriSale + o.PriPReturn + o.PriLessAdj)),
                            subtotal = ((o.SubPurchase + o.SubSReturn + o.SubAddAdj) - (o.SubSale + o.SubPReturn + o.subLessAdj)),
                            total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj) - (o.PriSale + o.PriPReturn + o.PriLessAdj)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubAddAdj) - (o.SubSale + o.SubPReturn + o.subLessAdj)),
                            o.ItemType,
                            o.bundle,
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

            return new QuickSoft.Models.LegacyJsonResult { Data = ConD };
        }

        [HttpPost]
        public JsonResult DeleteOrderById(int orderId)
        {
            bool stat = false;
            string msg;
            POSOrder order = db.POSOrders.Find(orderId);

            var Item = db.POSOrderItems.Where(a => a.OrderId == orderId);
            if (Item != null)
            {
                db.POSOrderItems.RemoveRange(db.POSOrderItems.Where(a => a.OrderId == orderId));
                db.SaveChanges();
            }

            var oldOrder = db.SalesEntrys.Where(c => c.OrderRefer == orderId).FirstOrDefault();
            if (oldOrder != null)
            {
                var id = oldOrder.SalesEntryId;
                SalesEntry SEen = db.SalesEntrys.Find(id);
                var SEItem = db.SEItemss.Where(a => a.SEItemsId == id);
                if (SEItem != null)
                {
                    db.SEItemss.RemoveRange(db.SEItemss.Where(a => a.SalesEntry == id));
                }
                var rec = db.WalkinCustomers.Where(a => a.SalesEntryId == id).FirstOrDefault();
                if (rec != null)
                {
                    db.WalkinCustomers.RemoveRange(db.WalkinCustomers.Where(a => a.SalesEntryId == id));
                }
                db.SalesEntrys.Remove(SEen);
            }
            if (order != null)
            {
                db.POSOrders.Remove(order);
                db.SaveChanges();
            }
            stat = true;
            msg = "Successfully deleted Order.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, orderId } };
        }


        [HttpGet]
        public ActionResult DuplicateBills(string billno, string date)
        {

            DateTime? fdate = null;
            if (date != "")
            {
                fdate = DateTime.Parse(date, new CultureInfo("en-GB"));
            }
            var data = (from a in db.SalesEntrys
                        join f in db.WalkinCustomers on a.SalesEntryId equals f.SalesEntryId into walk
                        from f in walk.DefaultIfEmpty()
                        join d in db.SEPayments on a.SalesEntryId equals d.SalesEntry into pay
                        from d in pay.DefaultIfEmpty()
                        join e in db.Employees on a.SECashier equals e.EmployeeId into user
                        from e in user.DefaultIfEmpty()
                        where (billno == "" || a.BillNo == billno) &&
                        (date == "" || a.SEDate == fdate)
                        select new
                        {
                            a.SalesEntryId,
                            CustomerName = f.CustomerName,
                            SENo = a.SENo,
                            PONo = a.PONo,
                            BillNo = a.BillNo,
                            Date = a.SEDate,
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
                            a.OrderRefer
                        }).ToList();
            return Json(data);
        }
        [HttpGet]
        public ActionResult SalesEntryAndItemById(long entryId)
        {

            var sales = (from a in db.SalesEntrys
                         join f in db.WalkinCustomers on a.SalesEntryId equals f.SalesEntryId into walk
                         from f in walk.DefaultIfEmpty()
                         join d in db.SEPayments on a.SalesEntryId equals d.SalesEntry into pay
                         from d in pay.DefaultIfEmpty()
                         join e in db.Employees on a.SECashier equals e.EmployeeId into user
                         from e in user.DefaultIfEmpty()
                         where a.SalesEntryId == entryId
                         select new
                         {
                             CustomerName = f.CustomerName,
                             SENo = a.SENo,
                             PONo = a.PONo,
                             BillNo = a.BillNo,
                             Date = a.SEDate,
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
                             SubTotal = a.SESubTotal,
                             a.PayType,
                             TermsAndCondition = db.TermsAndConditionss.Where(i => i.ConditionTypeID == "sales").Select(i => i.TermsCondit).FirstOrDefault(),


                         }).FirstOrDefault();
            var item = (from a in db.SEItemss
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join f in db.ItemUnits on a.ItemUnit equals f.ItemUnitID into unit
                        from f in unit.DefaultIfEmpty()
                        where a.SalesEntry == entryId
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
                            ItemNote=a.itemNote,
                            b.ItemType,
                             a.itemNote,
                            ItemUnitName=f.ItemUnitName,
                            bundleitem = (from ab in db.SEItemss
                                          join bb in db.Items on ab.Item equals bb.ItemID
                                          join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                                          from cb in primary.DefaultIfEmpty()
                                          join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                                          from bd in second.DefaultIfEmpty()
                                          where ab.SalesEntry == entryId 
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
            var billsundry = db.SEBillSundrys.Where(n => n.SalesEntry == entryId).Select(b => new
            {
                AmountType = b.AmountType,
                BsAmount = b.BsAmount,
                BsType = b.BsType,
                BsValue = b.BsValue != null ? b.BsValue : 0,
            }).FirstOrDefault();
            var data = new { item = item, sales = sales , billsundry = billsundry };
            return Json(data);
        }

        #endregion
        //bill no


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
                               TermsCondition = a.SENote!=null? a.SENote:"",
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
            if (details.TRN != "")
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
                        "<table  style='border: 0px; width: 100 %;'><tr><th><i><b>Customer زبون</b></i></th></tr><tr><td>" + details.PartyName + "</td></tr><tr><td style='font-size:14px;font-weight:normal;'>" + address + "</td></tr></table></td><td width='50%' style='border-left: 1px solid #ccc;'>" +
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

                        if (item.itemNote == "Bundle")
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

        //order no
        private long GetOrderId()
        {
            Int64 SENo = 0;
            DateTime today = DateTime.Now.Date;
            if ((db.POSOrders.Where(p => p.OrderDate == today).Select(p => p.EntryNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                SENo = 1;
            }
            else
            {
                SENo = db.POSOrders.Where(p => p.OrderDate == today).Max(p => p.EntryNo + 1);
            }

            return SENo;
        }
        ////Total Amount Only
        //[HttpGet]
        //[AllowCrossSiteJson]
        //        default:




        //        //Clear the Display
        //        //Goto Bottem Line - Most Left



        //        //Clear the Display
        //        //Goto Bottem Line - Most Left



        ////Items With Total Amount
        [HttpGet]
        [AllowCrossSiteJson]
        public JsonResult GetDisplayBoard(string Total, long? Count, int? Portno, string item)
        {
            var result = "success";
            new Thread(() =>
            {
                result = printcontents(Total, Count, Portno, item);

            }).Start();

            //    default:
            //    #region prev
            //        mystring = mystring.Substring(1);// remove first char
            //        mystring = mystring + s;// add first char to end
            //   // sport.Write(ogstring.Substring(0, 20));//take first 20
            //    #endregion prev
            //    #region new
            //    // string emptyspace = "                   "; //19 
            //    // for (int j = 0; j< ogstring.Length; j++)
            //    //     if (processed==0)
            //    //     if (mystring.Length > 20)
            //    //     else
            //    //     mystring = mystring.Substring(1);// remove first char
            //    //// sport.Write(ogstring.Substring(0, 20));//take first 20
            //    #endregion new
            //    //previous
            //    //Clear the Display
            //    //Goto Bottem Line - Most Left

            return Json(result);
        }


        // GET: POS

        public ActionResult test1()
        {
            SerialPort sport = new SerialPort("COM2", 9600, Parity.None, 8, StopBits.One);

            sport.Write(new byte[] { 0x0C }, 0, 1);
            sport.Write("TEST1");
            byte[] byteScrollMSGHorizontal = new byte[2] { 0x1F, 0x03 };

            sport.Write(byteScrollMSGHorizontal, 0, byteScrollMSGHorizontal.Length);



            char[] Msg = "*1234567890987654321* ".ToCharArray();

            for (int i = 0; i < Msg.Length; i++)
            {
                sport.Write(Msg[i].ToString());
                System.Threading.Thread.Sleep(110);
            }
            return View();
        }

        public ActionResult test2()
        {
            SerialPort sport = new SerialPort("COM2", 9600, Parity.None, 8, StopBits.One);

            sport.Write(new byte[] { 0x0C }, 0, 1);
            sport.Write("TEST2");
            byte[] byteScrollMSGHorizontal = new byte[4] { 0x1F, 0x03, 0x0A, 0x0D };

            sport.Write(byteScrollMSGHorizontal, 0, byteScrollMSGHorizontal.Length);



            char[] Msg = "*1234567890987654321* ".ToCharArray();

            for (int i = 0; i < Msg.Length; i++)

            {
                sport.Write(Msg[i].ToString());

                System.Threading.Thread.Sleep(110);
            }
            return View();
        }

        public string printcontents(string Total, long? Count, int? Portno, string item)
        {

            var result = "success";
            var myport = "COM1";
            switch (Portno)
            {
                case 1:
                    myport = "COM1";
                    break;
                case 2:
                    myport = "COM2";
                    break;
                case 3:
                    myport = "COM3";
                    break;
                case 4:
                    myport = "COM4";
                    break;
                case 5:
                    myport = "COM5";
                    break;
                case 6:
                    myport = "COM6";
                    break;
                default:
                    myport = "COM1";
                    break;
            }

            try
            {
                SerialPort sport = new SerialPort(myport, 9600, Parity.None, 8, StopBits.One);
                if (sport.IsOpen != true)
                {
                    sport.Open();
                }
                if (Count != null && Count != 0)
                {
                    sport.Write(new byte[] { 0x0C }, 0, 1);
                    sport.Write("Total :" + Total);
                    string ogstring = item + " ";
                    string mystring = "";
                    string displaystring = "";
                    int i = 0;
                    for (int count = 0; count <= ogstring.Length; count++)
                    {

                        if (mystring == "")
                        {
                            mystring = ogstring;
                        }
                        if (mystring.Length > 20)
                        {
                            displaystring = mystring.Substring(0, 20);
                        }
                        else
                        {
                            displaystring = mystring;
                        }

                        sport.Write(new byte[] { 0x0A, 0x0D }, 0, 2);
                        sport.Write(displaystring);
                        System.Threading.Thread.Sleep(500);
                        string s = mystring[0].ToString();//select first char
                        mystring = mystring.Substring(1);// remove first char
                        mystring = mystring + s;// add first char to end
                        i++;
                    }

                    sport.Write(new byte[] { 0x0C }, 0, 1);
                    sport.Write("Total :" + Total);
                    sport.Write(new byte[] { 0x0A, 0x0D }, 0, 2);
                }
                else
                {
                    sport.Write(new byte[] { 0x0C }, 0, 1);
                    sport.WriteLine("Next Customer");
                }
                sport.Close();
            }
            catch (Exception ex)
            {
                if (ex.InnerException is IOException)
                {

                }
                else
                {

                }
                result = ex.InnerException.Message;
            }
            return result;
        }

        [HttpGet]
        [AllowCrossSiteJson]
        public JsonResult GetDisplayNormal(string Total, long? Count, int? Portno, string item)
        {
            var result = "success";
            var myport = "COM1";
            switch (Portno)
            {
                case 1:
                    myport = "COM1";
                    break;
                case 2:
                    myport = "COM2";
                    break;
                case 3:
                    myport = "COM3";
                    break;
                case 4:
                    myport = "COM4";
                    break;
                case 5:
                    myport = "COM5";
                    break;
                case 6:
                    myport = "COM6";
                    break;
                default:
                    myport = "COM1";
                    break;

            }

            SerialPort sport = new SerialPort(myport, 9600, Parity.None, 8, StopBits.One);
            sport.Open();
            if (Count != null && Count != 0)
            {
                //Clear the Display
                sport.Write(new byte[] { 0x0C }, 0, 1);
                sport.Write("Total Amount ");
                //Goto Bottem Line - Most Left
                sport.Write(new byte[] { 0x0A, 0x0D }, 0, 2);

                sport.Write(Total);
            }
            else
            {
                //Clear the Display
                sport.Write(new byte[] { 0x0C }, 0, 1);
                sport.Write("Next Customer");
                //Goto Bottem Line - Most Left
                sport.Write(new byte[] { 0x0A, 0x0D }, 0, 2);
            }

            sport.Close();
            return Json(result);
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
