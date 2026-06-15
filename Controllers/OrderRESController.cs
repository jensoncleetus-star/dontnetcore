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
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    public class OrderRESController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public OrderRESController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,POS Entry")]
        public ActionResult Create(OrderViewModel vmodel)
         {
         if(vmodel.orderdata.Customer==null)
            {
              return new QuickSoft.Models.LegacyJsonResult { Data = new { status = false } };
            }
            
            var UserId = User.Identity.GetUserId();
            var emp = db.Employees.Where(o => o.UserId == UserId).Select(o => o.EmployeeId).FirstOrDefault();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
            var today = System.DateTime.Now;
            var TaxAmount = vmodel.orderdata.TaxAmount;
            var orderDate = DateTime.Parse(vmodel.OrderDate, new CultureInfo("en-GB"));
            long orderid = 0;

            var OrderNoInPOS = db.EnableSettings.Where(a => a.EnableType == "OrderNoInPOS").FirstOrDefault();
            if (1==1)
            {
                orderid = GetOrderId(orderDate);
            }
            else
            {
                orderid = GetOrderId(orderDate);
            }

            //check print bill or hold
            var directBill = db.EnableSettings.Where(a => a.EnableType == "DirectPrintBill").FirstOrDefault();
            var billdirect = directBill != null ? (directBill.Status == Status.active ? 0 : 1) : 1;

           
                // order details
                POSOrder order = new POSOrder
            {
                EntryNo = orderid,
                OrderNo = (vmodel.orderdata.OrderNo== "NaN")?orderid.ToString(): vmodel.orderdata.OrderNo,
                OrderDate = orderDate,
                TableId = vmodel.orderdata.TableId,
                OrderType = vmodel.OrderType,
                PeopleCount = vmodel.orderdata.PeopleCount,
                WaiterId =(vmodel.orderdata.WaiterId==null)? emp: vmodel.orderdata.WaiterId,
                tendering=(vmodel.OrderStatus == OrderStatus.Payment) ? vmodel.salePayment.SEPaidAmount:0,
                    dcharge = vmodel.orderdata.dcharge,
                    ItemCount = vmodel.orderdata.ItemCount,
                Quantity = vmodel.orderdata.Quantity,
                SubTotal = vmodel.orderdata.SubTotal,
                Tax = vmodel.orderdata.Tax,
                TaxAmount = TaxAmount,
              
            
                NetPayable = vmodel.orderdata.NetPayable,
                OrderNote = vmodel.orderdata.OrderNote,
                Discount = vmodel.orderdata.Discount,

                OrderStatus = (billdirect==0 && vmodel.OrderStatus==OrderStatus.PrintBill)? OrderStatus.Payment : vmodel.OrderStatus,
                CreatedDate = today,
                CreatedBy = UserId,
                Status = Status.active,
                Branch = BranchID,
                taxAFdisc = vmodel.orderdata.taxAFdisc,
            };
            //walkin customer
            if (vmodel.orderdata.CustomerType == CustomerType.Walking)
            {
                order.Customer = vmodel.orderdata.Customer;
                order.CustomerType = CustomerType.Walking;
                order.Customer = vmodel.orderdata.Customer;
            }
            else if (vmodel.orderdata.CustomerType == CustomerType.Card)
            {
                order.Customer = vmodel.orderdata.Customer;
                order.CustomerType = CustomerType.Card;
                order.Customer = vmodel.orderdata.Customer;
            }
            else
            {
                order.Customer = vmodel.orderdata.Customer;
                order.CustomerType = CustomerType.Customer;
            }
            
            db.POSOrders.Add(order);
            db.SaveChanges();
            Int64 POSOrderId = order.POSOrderId;


            insertItem(vmodel.orderitem, POSOrderId);

            InsertSalesEntry(vmodel, UserId, BranchID, today, POSOrderId);

            if (vmodel.OrderStatus == OrderStatus.PrintBill || vmodel.OrderStatus == OrderStatus.Hold || vmodel.OrderStatus == OrderStatus.Payment)
            {


                //        //table update




            }

            //print bill based on configuration
            var entype = Enum.GetName(typeof(OrderStatus), vmodel.OrderStatus);
            com.addlog(LogTypes.Created, UserId, "POSOrder", "POSOrders", findip(), POSOrderId, "Successfully Submitted POS Order by " + entype);
            if (vmodel.OrderStatus == OrderStatus.Payment)
            {
                return salesData(POSOrderId, vmodel.fnval, vmodel.orderdata.CustomerType, vmodel.posData);
            }
            else
            {
                return OrderData(POSOrderId, null);
            }
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,POS Entry")]
        public ActionResult Edit(POSViewModel vmodel)
        {

            bool stat = false;
            string msg;
            long id = vmodel.saleData.SalesEntryId;
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


            SalesEntry entry = db.SalesEntrys.Find(id);

            var ord = db.POSOrders.Where(o => o.POSOrderId == entry.OrderRefer).FirstOrDefault();
            if (ord != null)
            {
                ord.dcharge = vmodel.dcharge;
                db.Entry(ord).State = EntityState.Modified;
                db.SaveChanges();
            }
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
            else if (vmodel.saleData.CustomerType == CustomerType.Card)
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
                if (1 == 2)
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

            var sepayment = db.SEPayments.Where(o => o.SalesEntry == id).ToList();
            var setransaction = db.SETransactions.Where(o => o.SalesEntry == id).ToList();
            db.SEPayments.RemoveRange(sepayment);
            db.SaveChanges();
            db.SETransactions.RemoveRange(setransaction);
            bool delete = com.DeleteAllAccountTransaction("Sale", id);
            bool deletepay = com.DeleteAllAccountTransaction("Sale Payment", id);











            PosDate = DateTime.Parse(vmodel.SEDate, new CultureInfo("en-GB"));
            var custid = vmodel.saleData.Customer;
             TaxAmount = vmodel.saleData.SETaxAmount;



            var payamount = vmodel.salePayment.SEPaidAmount;
            // payment method
            var PayMethod = (vmodel.saleData.CustomerType == CustomerType.Walking) ? "Cash" : (vmodel.saleData.CustomerType == CustomerType.Customer) ? "Credit" : "Card";
            long? AccId = null;

            if (PayMethod == "Credit")
            {
                payamount = 0;
            }
            if (PayMethod == "Card" || PayMethod == "card")
            {

                var emid = db.Employees.Where(o => o.UserId == UserId).Select(o => o.EmployeeId).FirstOrDefault();
                var cusacid = db.accountmaps.Where(o => o.EmployeeId == emid && o.PaymentTypeId == EmployeePaymentType.Card).Select(o => o.AccountId).FirstOrDefault();
                if (cusacid != null)
                    AccId = cusacid;
                else
                {
                    var card = db.PaymentMethods.Where(c => c.MethodName == "card").SingleOrDefault();
                    AccId = card.AccountId;
                }




            }
            //SEPayment
            SEPayment SEpay = new SEPayment
            {
                CustomerId = custid,
                SEDate = PosDate,
                SEEntryDate = today,
                SEBillAmount = vmodel.saleData.SEGrandTotal,
                CreatedBranch = BranchID,
                CreatedUserId = UserId,
                SECreatedDate = today,
                Status = 1,
                SalesEntry = salesEntryId,
                SEPaidAmount = payamount
            };
            db.SEPayments.Add(SEpay);
            db.SaveChanges();

            PosData SEpos = new PosData
            {
                SalesEntry = salesEntryId,
                PayMethod = vmodel.posData.PayMethod,
                PayMode = PayMethod == "Card" ? vmodel.posData.PayMode : null,
                TotTender = vmodel.posData.TotTender,
                ChangeDue = vmodel.posData.ChangeDue,
                Account = AccId
            };
            db.PPosDatas.Add(SEpos);
            db.SaveChanges();

            Int64 custAccID = custAccID = db.Customers.Where(a => a.CustomerID == custid).Select(a => a.Accounts).FirstOrDefault();
            Int64 saleAccId = db.Accountss.Where(a => a.Group == 15).Select(a => a.AccountsID).SingleOrDefault();
            Int64 cashAccId = db.Accountss.Where(a => a.Group == 9).Select(a => a.AccountsID).FirstOrDefault();
            Int64 VATOutput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Output").Select(a => a.AccountsID).SingleOrDefault();
            if (PayMethod == "Card" || PayMethod == "card")
            {
                cashAccId = (long)AccId;
            }
            else
            {
                cashAccId = 3;
            }
            var date = PosDate;
            //walkin customer
            if (vmodel.saleData.CustomerType == CustomerType.Walking)
            {
                var emid = db.Employees.Where(o => o.UserId == UserId).Select(o => o.EmployeeId).FirstOrDefault();
                var cusacid = db.accountmaps.Where(o => o.EmployeeId == emid && o.PaymentTypeId == EmployeePaymentType.Cash).Select(o => o.AccountId).FirstOrDefault();
                if (cusacid != 0)
                    cashAccId = cusacid;
                else
                {
                    cashAccId = 3;
                }
            }
            if (vmodel.saleData.CustomerType == CustomerType.Card)
            {
                var emid = db.Employees.Where(o => o.UserId == UserId).Select(o => o.EmployeeId).FirstOrDefault();
                var cusacid = db.accountmaps.Where(o => o.EmployeeId == emid && o.PaymentTypeId == EmployeePaymentType.Card).Select(o => o.AccountId).FirstOrDefault();
                if (cusacid != 0)
                    cashAccId = cusacid;
                else
                {
                    cashAccId = 3;
                }
            }
            if (payamount > 0 || vmodel.saleData.CustomerType == CustomerType.Walking)
            {

                var Remark = "Direct Reciept From POS";
                long payid;
                //SETransaction
                SETransaction SEtran = new SETransaction
                {
                    CustomerId = custid,
                    SEPayDate = date,
                    SEPayAmount = payamount,
                    SECreatedDate = today,
                    CreatedBranch = BranchID,
                    CreatedUserId = UserId,
                    SalesEntry = salesEntryId
                };

                long? EmpId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                long? pettyaccid = db.accountmaps.Where(a => a.EmployeeId == EmpId && a.PaymentTypeId == EmployeePaymentType.Cash).Select(a => a.AccountId).FirstOrDefault();


                if (pettyaccid != 0)
                    payid = com.addReceipt(date, custAccID, cashAccId, payamount, payamount, Remark, UserId, BranchID, salesEntryId);
                else
                    payid = com.addReceipt(date, custAccID, (long)pettyaccid, payamount, payamount, Remark, UserId, BranchID, salesEntryId);

                SEtran.Recieptid = payid;
                db.SETransactions.Add(SEtran);
                db.SaveChanges();
            }
            if (vmodel.saleData.CustomerType == CustomerType.Walking || vmodel.saleData.CustomerType == CustomerType.Card)
            {
                com.addAccountTrasaction(vmodel.saleData.SEGrandTotal, 0, cashAccId, "Sale", salesEntryId, DC.Debit, PosDate);

            }

            else if (vmodel.saleData.CustomerType == CustomerType.Customer)
            {
                Int64 custAccIDs = db.Customers.Where(a => a.CustomerID == custid).Select(a => a.Accounts).FirstOrDefault();
                com.addAccountTrasaction(vmodel.saleData.SEGrandTotal, 0, custAccIDs, "Sale", salesEntryId, DC.Debit, PosDate);

            }
            //add trasaction to sale account with sale entry credit amount
            com.addAccountTrasaction(0, vmodel.saleData.SESubTotal , saleAccId, "Sale", salesEntryId, DC.Credit, PosDate);
            //add sale trasaction with customer debt amount

            // add vat output in account transaction
            if (TaxAmount > 0)
                com.addAccountTrasaction(TaxAmount, 0, VATOutput, "Sale", salesEntryId, DC.Debit, date);

            if (payamount > 0 || vmodel.saleData.CustomerType == CustomerType.Walking)
            {
                //if payment
                long? EmpId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                long? pettyaccid = db.accountmaps.Where(a => a.EmployeeId == EmpId && a.PaymentTypeId == EmployeePaymentType.Cash).Select(a => a.AccountId).FirstOrDefault();

                Int64 custAccIDs = db.Customers.Where(a => a.CustomerID == custid).Select(a => a.Accounts).FirstOrDefault();



            }
            com.addlog(LogTypes.Created, UserId, "SalesEntry", "SalesEntrys", findip(), salesEntryId, "Successfully Submitted POS Entry");
            if (action == "print" || action == "print_order")
            {
                string sedate = entry.SEDate.ToString("dd-MM-yyyy");
                if (1 == 2)
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
                                     SEPaidAmount = (d.SEPaidAmount == null) ? 0 : d.SEPaidAmount,
                                     SEDueAmount = a.SEGrandTotal - (d.SEPaidAmount == null ? 0 : d.SEPaidAmount),
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

        [HttpPost]
        [QkAuthorize(Roles = "Dev,POS Entry")]
        public ActionResult Update(OrderViewModel vmodel)
        {
            //check print bill or hold
            var directBill = db.EnableSettings.Where(a => a.EnableType == "DirectPrintBill").FirstOrDefault();
            var billdirect = directBill != null ? (directBill.Status == Status.active ? 0 : 1) : 1;


            var exist = db.POSOrders.Find(vmodel.orderdata.POSOrderId);
            if (exist != null)
            {

                var UserId = User.Identity.GetUserId();
                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                var today = Convert.ToDateTime(System.DateTime.Now);
                var TaxAmount = vmodel.orderdata.TaxAmount;
                var orderDate = DateTime.Parse(vmodel.OrderDate, new CultureInfo("en-GB"));
                var emp = db.Employees.Where(o => o.UserId == UserId).Select(o => o.EmployeeId).FirstOrDefault();

                //sales entry
                POSOrder order = db.POSOrders.Find(vmodel.orderdata.POSOrderId);
                var orderitems = db.POSOrderItems.Where(a => a.OrderId == vmodel.orderdata.POSOrderId).ToList();
                order.OrderDate = orderDate;
                order.WaiterId = (vmodel.orderdata.WaiterId==null)?emp: vmodel.orderdata.WaiterId;
                order.ItemCount = vmodel.orderdata.ItemCount;
                order.Quantity = vmodel.orderdata.Quantity;
                order.SubTotal = vmodel.orderdata.SubTotal;
                order.Tax = vmodel.orderdata.Tax;
                order.TaxAmount = TaxAmount;
                order.NetPayable = vmodel.orderdata.NetPayable;
                order.OrderNote = vmodel.orderdata.OrderNote;
                order.Status = Status.active;
                order.Branch = BranchID;
                if (vmodel.OrderStatus == OrderStatus.Payment)
                    order.tendering = vmodel.salePayment.SEPaidAmount;
                else
                    order.tendering = 0;

                order.dcharge = vmodel.orderdata.dcharge;



                order.OrderType = vmodel.OrderType;
                order.OrderStatus = vmodel.OrderStatus;//(vmodel.OrderStatus== "PrintKOT") ? OrderStatus.PrintKOT : OrderStatus.SaveOrder;//(billdirect == 0 && vmodel.OrderStatus == OrderStatus.PrintBill) ? OrderStatus.Payment : vmodel.OrderStatus;
                order.OrderNote = vmodel.orderdata.OrderNote;

                order.Discount = vmodel.orderdata.Discount;
                order.taxAFdisc = vmodel.orderdata.taxAFdisc;

                order.TableId = vmodel.orderdata.TableId;
                order.PeopleCount = vmodel.orderdata.PeopleCount;

                //walkin customer
                if (vmodel.orderdata.CustomerType == CustomerType.Walking)
                {
                    order.Customer = 0;
                    order.CustomerType = CustomerType.Walking;
                    order.Customer = vmodel.orderdata.Customer;
                }
                else
                {
                    order.Customer = vmodel.orderdata.Customer;
                    order.CustomerType = CustomerType.Customer;
                }
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();

                Int64 POSOrderId = order.POSOrderId;

                var OrderItem = db.POSOrderItems.Where(a => a.OrderId == POSOrderId).FirstOrDefault();
                if (OrderItem != null)
                {
                    db.POSOrderItems.RemoveRange(db.POSOrderItems.Where(a => a.OrderId == POSOrderId));
                    db.SaveChanges();
                }
                insertItem(vmodel.orderitem, POSOrderId);

                ////update table
                //        //table update



                //check print bill or hold
                if (vmodel.OrderStatus == OrderStatus.PrintBill || vmodel.OrderStatus == OrderStatus.Hold || vmodel.OrderStatus == OrderStatus.Payment)
                {
                    var oldOrder = db.SalesEntrys.Where(c => c.OrderRefer == POSOrderId).FirstOrDefault();
                    if (oldOrder != null)
                    {
                        UpdateSalesEntry(vmodel, POSOrderId, UserId, BranchID, today);
                    }
                    else
                    {
                        InsertSalesEntry(vmodel, UserId, BranchID, today, POSOrderId);
                    }
                }
                var entype = Enum.GetName(typeof(OrderStatus), vmodel.OrderStatus);
                com.addlog(LogTypes.Updated, UserId, "POSOrder", "POSOrders", findip(), POSOrderId, "Successfully Updated POS Order by " + entype);
                if (vmodel.OrderStatus == OrderStatus.Payment)
                {
                  
                    return salesData(POSOrderId, vmodel.fnval, vmodel.orderdata.CustomerType, vmodel.posData);
                }
                else
                {
                    return OrderData(POSOrderId, orderitems);
                }
            }
            else
            {
                return Create(vmodel);
            }
        }

        private bool insertItem(ICollection<POSOrderItemViewModel> orderitem, long POSOrderId)
        {
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

            foreach (var arr in orderitem)
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
                dr["ItemNote"] = arr.ItemNote == null ? "" : arr.ItemNote;
                dr["Note"] = (arr.itemsize!=null)?arr.itemsize.Split('|')[0]:null;
                dr["PrintCount"] = 0;
                dr["Prints"] = arr.Prints;
                dr["OrderId"] = POSOrderId;
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
                        dbu["Prints"] = arr.Prints;
                        dbu["OrderId"] = POSOrderId;
                        dbu["ItemId"] = bu.Item;
                        dbu["editable"] = 1;
                        dtItem.Rows.Add(dbu);
                    }
                }
                if (arr.AddOnNote != null)
                {
                    if (arr.AddOnNote.Any())
                    {
                        var addons = (from a in db.ItemAddOns
                                      join b in db.Items on a.MainItem equals b.ItemID
                                      join c in db.ItemUnits on a.Unit equals c.ItemUnitID into primary
                                      from c in primary.DefaultIfEmpty()
                                      where arr.AddOnNote.Contains(a.ItemAddOnID)
                                      select new
                                      {
                                          b.ItemCode,
                                          a.Unit,
                                          b.ItemName,
                                          c.ItemUnitName,
                                          ItemUnitPrice = a.UnitPrice,
                                          quantity = a.Quantity,
                                          Item = b.ItemID,
                                          a.ItemAddOnID

                                      }).ToList();
                        foreach (var bu in addons)
                        {
                            var qua = bu.quantity;

                            DataRow dbu = dtItem.NewRow();
                            dbu["ItemUnit"] = bu.Unit;
                            dbu["ItemUnitPrice"] = bu.ItemUnitPrice;
                            dbu["ItemQuantity"] = bu.quantity;
                            dbu["ItemSubTotal"] = bu.ItemUnitPrice;
                            // add parent itemid in discount for reference
                            dbu["ItemDiscount"] = arr.Item;
                            // passing addon id for identifying
                            dbu["ItemTax"] = bu.ItemAddOnID;
                            dbu["ItemTaxAmount"] = 0;
                            dbu["ItemTotalAmount"] = bu.ItemUnitPrice;
                            dbu["ItemNote"] = "AddOn";
                            dbu["Note"] = "";
                            dbu["PrintCount"] = 0;
                            dbu["Prints"] = arr.Prints;
                            dbu["OrderId"] = POSOrderId;
                            dbu["ItemId"] = bu.Item;
                            dbu["editable"] = 1;
                            dtItem.Rows.Add(dbu);
                        }
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
            return true;
        }
        private JsonResult OrderData(long POSOrderId, ICollection<POSOrderItem> order1)
        {
            var fnval = "";
            var sales = db.POSOrders.Where(c => c.POSOrderId == POSOrderId).Select(a => new
            {
                BillNo = "",//need change
                OrderNo = a.OrderNo,
                Date = a.OrderDate,
                Note = a.OrderNote,
                CustomerType = a.CustomerType,
                Cashier = db.Employees.Where(d => d.EmployeeId == a.WaiterId).Select(d => d.FirstName + " " + d.LastName).FirstOrDefault(),
                SEDiscount = a.Discount,
                SEGrandTotal = a.NetPayable,
                SEPaidAmount = a.NetPayable,
                SEDueAmount = 0,
                SETaxAmount = a.TaxAmount,
                a.dcharge,
                a.OrderStatus,
                a.SubTotal,
                a.OrderType,
                Table = db.Tables.Where(c => c.TableId == a.TableId).Select(c => c.TableName).FirstOrDefault(),
                customer = db.Customers.Where(c => c.CustomerID == a.Customer).FirstOrDefault(),//.Join(db.Contacts, post => post.Contact, meta => meta.ContactID, (post, meta) => new { Post = post, Meta = meta }).FirstOrDefault(),
                wcustomer = a.custName,
                TermsAndCondition = db.TermsAndConditionss.Where(i => i.ConditionTypeID == "sales").Select(i => i.TermsCondit).FirstOrDefault(),
                a.custMob
            }).ToList().Select(o => new
            {
                o.OrderNo,
                o.Date,
                o.Note,
                o.CustomerType,
                o.dcharge,
                o.Cashier,
                o.SEDiscount,
                o.SubTotal,
                o.SEGrandTotal,
                o.SEPaidAmount,
                o.SEDueAmount,
                o.SETaxAmount,
                o.Table,
                o.OrderStatus,
                o.OrderType,
                o.customer,
                o.wcustomer,
                o.custMob,
                o.TermsAndCondition,
                CustomerName=o.customer.CustomerName,
                // Mobile = (o.CustomerType == CustomerType.Walking ? "" : o.customer.Meta.Mobile)
            }).FirstOrDefault();
            var item = (from a in db.POSOrderItems
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()

                        join f in db.ItemUnits on a.ItemUnit equals f.ItemUnitID into unit
                        from f in unit.DefaultIfEmpty()

                        join g in db.ItemBundles on b.ItemID equals g.mainItem into bundle
                        from g in bundle.DefaultIfEmpty()

                        where a.OrderId == POSOrderId
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
                            ItemNote = a.ItemNote,
                            g.BundleType,
                            b.ItemType,
                            //itemsizename = (from ab in db.itemsizeprice
                                            //where ab.itemid == a.Item && ab.price.ToString().Contains(a.Note)
                                            //    bb.ItemSizeName
                            ItemUnitName = f.ItemUnitName,

                            realname = (
                                 from aa in db.POSOrderItems
                                 join ee in db.itemsizeprice on aa.Item equals ee.itemid into ss
                                 from ee in ss.DefaultIfEmpty()
                                 join ff in db.ItemSizes on ee.sizeid equals ff.ItemSizeID
                                 where aa.Note == ee.sizepriceid.ToString() &&
                               aa.Item == a.Item && ee.price == a.ItemUnitPrice
                               && aa.OrderId == POSOrderId
                                 select new
                                 {
                                     ff.ItemSizeName
                                 }).FirstOrDefault(),
                            bundle = (from ab in db.POSOrderItems
                                      join bb in db.Items on ab.Item equals bb.ItemID
                                      join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                                      from cb in primary.DefaultIfEmpty()
                                      join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                                      from bd in second.DefaultIfEmpty()

                                      join un in db.ItemUnits on ab.ItemUnit equals un.ItemUnitID into unit
                                      from un in unit.DefaultIfEmpty()

                                      where ab.OrderId == POSOrderId
                                          && b.ItemID == ab.ItemDiscount && ab.ItemNote != "AddOn"
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


                                          ab.Note,
                                          ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                          bb.ItemUnitID,
                                          bb.SubUnitId,
                                          PriUnit = cb.ItemUnitName,
                                          SubUnit = bd.ItemUnitName,
                                          bb.ItemArabic,
                                          ItemUnitName = un.ItemUnitName,
                                      }).ToList(),

                            addon = (from ab in db.POSOrderItems
                                     join bb in db.Items on ab.Item equals bb.ItemID
                                     join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                                     from cb in primary.DefaultIfEmpty()
                                     join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                                     from bd in second.DefaultIfEmpty()

                                     join un in db.ItemUnits on ab.ItemUnit equals un.ItemUnitID into unit
                                     from un in unit.DefaultIfEmpty()
                                     join be in db.ItemAddOns on ab.ItemTax equals be.ItemAddOnID into addon
                                     from be in addon.DefaultIfEmpty()

                                     where ab.OrderId == POSOrderId
                                         && b.ItemID == ab.ItemDiscount && ab.ItemNote == "AddOn"
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

                                         ab.Item,
                                         ab.ItemQuantity,
                                         ab.ItemUnit,

                                         ItemDiscount = 0,


                                         ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                         bb.ItemUnitID,
                                         bb.SubUnitId,
                                         PriUnit = cb.ItemUnitName,
                                         SubUnit = bd.ItemUnitName,
                                         bb.ItemArabic,
                                         Name = be.Name
                                     }).ToList(),
                        }).ToList();


                

            ////var memoitem = db.Items.ToList();
            if (order1 != null&& sales.OrderType !=OrderType.Delivery)
            {
                var compare = (from a in item
                               join b in order1 on a.Item equals b.Item into old
                               from c in old.DefaultIfEmpty()


                               select new
                               {
                                   a.Item,
                                   a.ItemName,
                                   a.ItemArabic,
                                   
                                   a.ItemSubTotal,
                                   a.ItemUnitPrice,
                                   olditem = (c?.ItemQuantity == null) ? 0 : c.ItemQuantity,
                                   a.ItemQuantity,
                                   a.ItemNote,
                                   a.BundleType,
                                   a.ItemType,
                                   a.ItemUnitName,
                                   bundle = (from ab in db.POSOrderItems
                                             join bb in db.Items on ab.Item equals bb.ItemID
                                             join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                                             from cb in primary.DefaultIfEmpty()
                                             join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                                             from bd in second.DefaultIfEmpty()

                                             join un in db.ItemUnits on ab.ItemUnit equals un.ItemUnitID into unit
                                             from un in unit.DefaultIfEmpty()

                                             where ab.OrderId == POSOrderId
                                             && a.Item == ab.ItemDiscount
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


                                                 ab.Note,
                                                 ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                 bb.ItemUnitID,
                                                 bb.SubUnitId,
                                                 PriUnit = cb.ItemUnitName,
                                                 SubUnit = bd.ItemUnitName,
                                                 bb.ItemArabic,
                                                 ItemUnitName = un.ItemUnitName,
                                             }).ToList(),
                                   addon = (from ab in db.POSOrderItems
                                            join bb in db.Items on ab.Item equals bb.ItemID
                                            join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                                            from cb in primary.DefaultIfEmpty()
                                            join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                                            from bd in second.DefaultIfEmpty()

                                            join un in db.ItemUnits on ab.ItemUnit equals un.ItemUnitID into unit
                                            from un in unit.DefaultIfEmpty()
                                            join be in db.ItemAddOns on ab.ItemTax equals be.ItemAddOnID into addon
                                            from be in addon.DefaultIfEmpty()

                                            where ab.OrderId == POSOrderId
                                                && a.Item == ab.ItemDiscount && ab.ItemNote == "AddOn"
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

                                                ab.Item,
                                                ab.ItemQuantity,
                                                ab.ItemUnit,

                                                ItemDiscount = 0,


                                                ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                bb.ItemUnitID,
                                                bb.SubUnitId,
                                                PriUnit = cb.ItemUnitName,
                                                SubUnit = bd.ItemUnitName,
                                                bb.ItemArabic,
                                                Name = be.Name
                                            }).ToList(),
                               }).ToList();
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = true, item = item, printitem = compare, sales = sales, fnval = fnval } };
            }
            else
            {

                var compare = (from a in item



                               select new
                               {
                                   a.Item,
                                   a.ItemName,
                                   a.ItemArabic,
                                   a.ItemSubTotal,
                                   a.ItemUnitPrice,
                                   a.ItemQuantity,
                                   a.ItemNote,
                                   olditem = 0,
                                   a.BundleType,
                                   a.ItemType,
                                   a.ItemUnitName,
                                   bundle = (from ab in db.POSOrderItems
                                             join bb in db.Items on ab.Item equals bb.ItemID
                                             join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                                             from cb in primary.DefaultIfEmpty()
                                             join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                                             from bd in second.DefaultIfEmpty()

                                             join un in db.ItemUnits on ab.ItemUnit equals un.ItemUnitID into unit
                                             from un in unit.DefaultIfEmpty()

                                             where ab.OrderId == POSOrderId
                                             && a.Item == ab.ItemDiscount
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

                                               
                                                 ab.ItemNote,
                                                 ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                 bb.ItemUnitID,
                                                 bb.SubUnitId,
                                                 PriUnit = cb.ItemUnitName,
                                                 SubUnit = bd.ItemUnitName,
                                                 bb.ItemArabic,
                                                 un.ItemUnitName,

                                             }).ToList()
                               }).ToList();
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = true, item = item, printitem = compare, sales = sales } };

            }

        }

        private JsonResult salesData(long POSOrderId, string action, CustomerType type, PosData posData)
        {

            var salesEntryId = db.SalesEntrys.Where(c => c.OrderRefer == POSOrderId).Select(a => a.SalesEntryId).FirstOrDefault();
            bool stat = false;
            string msg;
            if (action == "print" || action == "print_order")
            {
                if (type == CustomerType.Walking)
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
                                     Mobile = f.Addres,
                       
                                     totalpayed = (from ba in db.POSOrders
                                                   where ba.POSOrderId==a.OrderRefer
                                                   select new
                                                   {
                                                       ba.tendering
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
                                     customer = db.Customers.Where(c => c.CustomerID == a.Customer).FirstOrDefault(),

                                     TRN = "",
                                     TermsAndCondition = db.TermsAndConditionss.Where(i => i.ConditionTypeID == "sales").Select(i => i.TermsCondit).FirstOrDefault(),
                                     taxAFdisc = a.SETax
                                 }).FirstOrDefault();
                    var item = (from a in db.SEItemss
                                join b in db.Items on a.Item equals b.ItemID
                                join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                                from c in primary.DefaultIfEmpty()
                                join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                                from d in second.DefaultIfEmpty()
                                join f in db.ItemUnits on a.ItemUnit equals f.ItemUnitID into unit
                                from f in unit.DefaultIfEmpty()
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
                                    ItemNote = a.itemNote,
                                    b.ItemArabic,
                                    g.BundleType,
                                    b.ItemType,


                                 realname = (
                                 from aa in db.POSOrderItems
                                 join ee in db.itemsizeprice on aa.Item equals ee.itemid into ss
                                 from ee in ss.DefaultIfEmpty()
                                 join ff in db.ItemSizes on ee.sizeid equals ff.ItemSizeID
                                 where aa.Note==ee.sizepriceid.ToString() &&
                               aa.Item==a.Item && ee.price==a.ItemUnitPrice
                               && aa.OrderId==POSOrderId
                                                                    select new
                                                                    {
                                                                        ff.ItemSizeName
                                                                    }).FirstOrDefault(),
                                                 

                                    ItemUnitName = f.ItemUnitName,
                                    bundle = (from ab in db.SEItemss
                                              join bb in db.Items on ab.Item equals bb.ItemID
                                              join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                                              from cb in primary.DefaultIfEmpty()
                                              join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                                              from bd in second.DefaultIfEmpty()
                                              where ab.SalesEntry == salesEntryId
                                              && a.Item == ab.ItemDiscount && ab.itemNote != "AddOn"
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
                                              }).ToList(),

                                    addon = (from ab in db.SEItemss
                                             join bb in db.Items on ab.Item equals bb.ItemID
                                             join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                                             from cb in primary.DefaultIfEmpty()
                                             join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                                             from bd in second.DefaultIfEmpty()
                                             join be in db.ItemAddOns on ab.ItemTax equals be.ItemAddOnID into addon
                                             from be in addon.DefaultIfEmpty()
                                             where ab.SalesEntry == salesEntryId
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

                                }).ToList();
                    stat = true;

                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item = item, sales = sales, PosDate = posData, fnval = action } };
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
                                     SEPaidAmount = d != null ? d.SEPaidAmount : 0,
                                     SEDueAmount = d != null ? a.SEGrandTotal - d.SEPaidAmount : 0,
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
                                     TermsAndCondition = db.TermsAndConditionss.Where(i => i.ConditionTypeID == "sales").Select(i => i.TermsCondit).FirstOrDefault(),
                                     taxAFdisc = a.SETax
                                 }).FirstOrDefault();

                    var item = (from a in db.SEItemss
                                join b in db.Items on a.Item equals b.ItemID
                                join f in db.ItemUnits on a.ItemUnit equals f.ItemUnitID into unit
                                from f in unit.DefaultIfEmpty()
                                join g in db.ItemBundles on b.ItemID equals g.mainItem into bundle
                                from g in bundle.DefaultIfEmpty()
                                where a.SalesEntry == salesEntryId 
                                select new
                                {

                                    ItemUnitPrice = a.ItemUnitPrice,
                                    ItemQuantity = a.ItemQuantity,
                                    ItemSubTotal = a.ItemSubTotal,
                                    ItemTax = a.ItemTax,
                                    ItemTaxAmount = a.ItemTaxAmount,
                                    ItemTotalAmount = a.ItemTotalAmount,
                                    ItemCode = b.ItemCode,
                                    ItemName = b.ItemName,
                                    ItemUnit = b.ItemUnit,
                                    ItemNote = a.itemNote,
                                    ItemUnitName = f.ItemUnitName,
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
                                              }).ToList(),
                                    addon = (from ab in db.SEItemss
                                             join bb in db.Items on ab.Item equals bb.ItemID
                                             join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                                             from cb in primary.DefaultIfEmpty()
                                             join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                                             from bd in second.DefaultIfEmpty()
                                             join be in db.ItemAddOns on ab.ItemTax equals be.ItemAddOnID into addon
                                             from be in addon.DefaultIfEmpty()
                                             where ab.SalesEntry == salesEntryId
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

                                }).ToList();
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, sales, PosDate = posData, fnval = action } };
                }
            }
            else
            {
                msg = "Successfully Completed POS Entry.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, fnval = action } };
            }
        }


        private bool InsertSalesEntry(OrderViewModel vmodel, string UserId, long BranchID, DateTime today, long POSOrderId)
        {
            POSController pos = new POSController();
            var PosDate = DateTime.Parse(vmodel.OrderDate, new CultureInfo("en-GB"));
            SalesEntry SEentry = new SalesEntry
            {
            
                SENo = pos.GetSeNo(),
                BillNo = pos.InvoiceNo(),
                SEDate = PosDate,
                SECashier = vmodel.orderdata.WaiterId,
                SaleType = SaleType.POS,
                PayType = "",
                SEItems = vmodel.orderdata.ItemCount,
                SEItemQuantity = vmodel.orderdata.Quantity,
                SESubTotal = vmodel.orderdata.SubTotal,
                SETax = vmodel.orderdata.Tax,
                SETaxAmount = vmodel.orderdata.TaxAmount,
                SEGrandTotal = vmodel.orderdata.NetPayable,
                SENote = vmodel.orderdata.OrderNote,
                SEDiscount = (decimal)vmodel.orderdata.Discount,
                Print = 1,
                SECreatedDate = today,
                CreatedBy = UserId,
                Status = 1,
                OrderRefer = POSOrderId,
                Branch =1,
         
            };
            //walkin customer

            if (vmodel.orderdata.CustomerType == CustomerType.Walking)
            {
                SEentry.Customer = (long)vmodel.orderdata.Customer;
                SEentry.CustomerType = CustomerType.Walking;
            }
            else if(vmodel.orderdata.CustomerType == CustomerType.Card)
            {
                SEentry.Customer = (long)vmodel.orderdata.Customer;
                SEentry.CustomerType = CustomerType.Card;
            }
            else
            {
                SEentry.Customer = (long)vmodel.orderdata.Customer;
                SEentry.CustomerType = CustomerType.Customer;
            }
            db.SalesEntrys.Add(SEentry);
            db.SaveChanges();
            Int64 salesEntryId = SEentry.SalesEntryId;


            //walkin customer
            //    WalkinCustomer wc = new WalkinCustomer
            //        SalesEntryId = salesEntryId,
            //        CustomerName = vmodel.wCustomer.CustomerName,
            //        MobileNo = vmodel.wCustomer.MobileNo
            insertSEItem(vmodel.orderitem, salesEntryId);

            //check print bill or hold
            var directBill = db.EnableSettings.Where(a => a.EnableType == "DirectPrintBill").FirstOrDefault();
            var billdirect = directBill != null ? (directBill.Status == Status.active ? 0 : 1) : 1;
            ////bill direct
            if (billdirect == 0 && vmodel.OrderStatus == OrderStatus.PrintBill)
            {

                PosData pdata = new PosData();

                pdata.PayMethod = "Cash";
                pdata.TotTender = vmodel.orderdata.NetPayable.ToString();
                pdata.ChangeDue = "0.00";

                vmodel.posData = pdata;
                vmodel.fnval = "print";
                vmodel.OrderStatus = OrderStatus.Payment;
            }


            if (vmodel.OrderStatus == OrderStatus.Payment)
            {
                sePayment(vmodel, salesEntryId, UserId, BranchID, today);
            }
            if(vmodel.ServiceExpense != 0 && vmodel.ServiceExpense != null)
            {
                // add service expence to payable account delivery Charge Payable : 498
                Int64 deliveryChargePayable = 498;
                com.addAccountTrasaction(0, (decimal)vmodel.ServiceExpense, deliveryChargePayable, "Sale", salesEntryId, DC.Credit, PosDate);
            }
            return true;
        }
        private bool UpdateSalesEntry(OrderViewModel vmodel, long POSOrderId, string UserId, long BranchID, DateTime today)
        {
            var PosDate = DateTime.Parse(vmodel.OrderDate, new CultureInfo("en-GB"));
            SalesEntry SEentry = db.SalesEntrys.Where(c => c.OrderRefer == POSOrderId).FirstOrDefault();
            SEentry.SEDate = PosDate;
            SEentry.SECashier = vmodel.orderdata.WaiterId;
            SEentry.SaleType = SaleType.POS;
            SEentry.PayType = "";
            SEentry.SEItems = vmodel.orderdata.ItemCount;
            SEentry.SEItemQuantity = vmodel.orderdata.Quantity;
            SEentry.SESubTotal = vmodel.orderdata.SubTotal;
            SEentry.SETax = vmodel.orderdata.Tax;
            SEentry.SETaxAmount = vmodel.orderdata.TaxAmount;
            SEentry.SEGrandTotal = vmodel.orderdata.NetPayable;
            SEentry.SENote = vmodel.orderdata.OrderNote;
            SEentry.SEDiscount = (decimal)vmodel.orderdata.Discount;
            SEentry.SETax = vmodel.orderdata.Tax;
            SEentry.Print = 1;
            //walkin customer
            if (vmodel.orderdata.CustomerType == CustomerType.Walking)
            {
                SEentry.Customer = (long)vmodel.orderdata.Customer;
                SEentry.CustomerType = CustomerType.Walking;
            }
            else
            {
                SEentry.Customer = (long)vmodel.orderdata.Customer;
                SEentry.CustomerType = CustomerType.Customer;
            }
            db.Entry(SEentry).State = EntityState.Modified;
            db.SaveChanges();
            var SalesEntryId = SEentry.SalesEntryId;

            //walkin customer

            //    WalkinCustomer wc = new WalkinCustomer
            //        SalesEntryId = SalesEntryId,
            //        CustomerName = vmodel.wCustomer.CustomerName,
            //        MobileNo = vmodel.wCustomer.MobileNo

            var SEItem = db.SEItemss.Where(a => a.SalesEntry == SalesEntryId).FirstOrDefault();
            if (SEItem != null)
            {
                db.SEItemss.RemoveRange(db.SEItemss.Where(a => a.SalesEntry == SalesEntryId));
            }
            insertSEItem(vmodel.orderitem, SalesEntryId);


            //check print bill or hold
            var directBill = db.EnableSettings.Where(a => a.EnableType == "DirectPrintBill").FirstOrDefault();
            var billdirect = directBill != null ? (directBill.Status == Status.active ? 0 : 1) : 1;
            ////bill direct
            if (billdirect == 0 && vmodel.OrderStatus == OrderStatus.PrintBill)
            {
                PosData pdata = new PosData();

                pdata.PayMethod = "Cash";
                pdata.TotTender = vmodel.orderdata.NetPayable.ToString();
                pdata.ChangeDue = "0.00";

                vmodel.posData = pdata;
                vmodel.fnval = "print";

                vmodel.OrderStatus = OrderStatus.Payment;
            }
            if (vmodel.OrderStatus == OrderStatus.Payment)
            {
                sePayment(vmodel, SalesEntryId, UserId, BranchID, today);
            }
            return true;
        }

        private bool insertSEItem(ICollection<POSOrderItemViewModel> orderitem, long salesEntryId)
        {
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

            foreach (var arr in orderitem)
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
                dr["itemNote"]=(arr.itemsize != null) ? arr.itemsize.Split('|')[0] : "";
                dr["SaleEntry"] = salesEntryId;
                dr["Item"] = arr.Item;
           
                dr["Type"] = 0;
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
                        dbu["Type"] = 0;
        
                        dtItem.Rows.Add(dbu);
                    }
                }
                if (arr.AddOnNote != null)
                {
                    if (arr.AddOnNote.Any())
                    {
                        var addons = (from a in db.ItemAddOns
                                      join b in db.Items on a.MainItem equals b.ItemID
                                      join c in db.ItemUnits on a.Unit equals c.ItemUnitID into primary
                                      from c in primary.DefaultIfEmpty()
                                      where arr.AddOnNote.Contains(a.ItemAddOnID)
                                      select new
                                      {
                                          b.ItemCode,
                                          a.Unit,
                                          b.ItemName,
                                          c.ItemUnitName,
                                          ItemUnitPrice = a.UnitPrice,
                                          quantity = a.Quantity,
                                          Item = b.ItemID,
                                          a.ItemAddOnID
                                      }).ToList();
                        foreach (var bu in addons)
                        {
                            var qua = bu.quantity;

                            DataRow dbu = dtItem.NewRow();
                            dbu["ItemUnit"] = bu.Unit;
                            dbu["ItemUnitPrice"] = bu.ItemUnitPrice;
                            dbu["ItemQuantity"] = bu.quantity;
                            dbu["ItemSubTotal"] = 0;
                            // add parent itemid in discount for reference
                            dbu["ItemDiscount"] = arr.Item;
                            // passing addon id for identifying
                            dbu["ItemTax"] = bu.ItemAddOnID;
                            dbu["ItemTaxAmount"] = 0;
                            dbu["ItemTotalAmount"] = bu.ItemUnitPrice;
                            dbu["itemNote"] = "AddOn";
                            dbu["Note"] = "";
                            dbu["SaleEntry"] = salesEntryId;
                            dbu["Item"] = bu.Item;
                            dbu["editable"] = 1;
                            dtItem.Rows.Add(dbu);
                        }
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
            return true;
        }

        private bool sePayment(OrderViewModel vmodel, long salesEntryId, string UserId, long BranchID, DateTime today)
        {
            var PosDate = DateTime.Parse(vmodel.OrderDate, new CultureInfo("en-GB"));
            var custid = vmodel.orderdata.Customer ?? 0;
            var TaxAmount = vmodel.orderdata.TaxAmount;



            var payamount = vmodel.salePayment.SEPaidAmount;// (vmodel.orderdata.CustomerType == CustomerType.Walking || (vmodel.salePayment != null? vmodel.salePayment.SEPaidAmount : vmodel.orderdata.NetPayable) > vmodel.orderdata.NetPayable) ? vmodel.orderdata.NetPayable : vmodel.salePayment.SEPaidAmount;
            // payment meth-od
            var PayMethod = (vmodel.orderdata.CustomerType == CustomerType.Walking) ? "Cash" : (vmodel.orderdata.CustomerType == CustomerType.Customer) ? "Credit" : "Card";
            long? AccId = null;

            if (PayMethod == "Credit")
            {
                payamount = 0;
            }
            if (PayMethod == "Card"|| PayMethod == "card")
            {

                var emid = db.Employees.Where(o => o.UserId == UserId).Select(o => o.EmployeeId).FirstOrDefault();
                var cusacid = db.accountmaps.Where(o => o.EmployeeId == emid && o.PaymentTypeId == EmployeePaymentType.Card).Select(o => o.AccountId).FirstOrDefault();
                if (cusacid != null)
                    AccId = cusacid;
                else
                {
                    var card = db.PaymentMethods.Where(c => c.MethodName == "card").SingleOrDefault();
                    AccId = card.AccountId;
                }




                }
            //SEPayment
            SEPayment SEpay = new SEPayment
            {
                CustomerId = custid,
                SEDate = PosDate,
                SEEntryDate = today,
                SEBillAmount = vmodel.orderdata.NetPayable,
                CreatedBranch = BranchID,
                CreatedUserId = UserId,
                SECreatedDate = today,
                Status = 1,
                SalesEntry = salesEntryId,
                SEPaidAmount = payamount
            };
            db.SEPayments.Add(SEpay);
            db.SaveChanges();

            PosData SEpos = new PosData
            {
                SalesEntry = salesEntryId,
                PayMethod = vmodel.posData.PayMethod,
                PayMode = PayMethod == "Card" ? vmodel.posData.PayMode : null,
                TotTender = vmodel.posData.TotTender,
                ChangeDue = vmodel.posData.ChangeDue,
                Account = AccId
            };
            db.PPosDatas.Add(SEpos);
            db.SaveChanges();

            Int64 custAccID = custAccID = db.Customers.Where(a => a.CustomerID == custid).Select(a => a.Accounts).FirstOrDefault();
            Int64 saleAccId = db.Accountss.Where(a => a.Group == 15).Select(a => a.AccountsID).SingleOrDefault();
            Int64 cashAccId = db.Accountss.Where(a => a.Group == 9).Select(a => a.AccountsID).FirstOrDefault();
            Int64 VATOutput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Output").Select(a => a.AccountsID).SingleOrDefault();
            if (PayMethod == "Card"|| PayMethod == "card")
            {
                cashAccId = (long)AccId;
            }
            else
            {
                cashAccId = 3;
            }
            var date = PosDate;
            //walkin customer
            if (vmodel.orderdata.CustomerType == CustomerType.Walking)
            {
                var emid = db.Employees.Where(o => o.UserId == UserId).Select(o => o.EmployeeId).FirstOrDefault();
                var cusacid = db.accountmaps.Where(o => o.EmployeeId == emid && o.PaymentTypeId == EmployeePaymentType.Cash).Select(o => o.AccountId).FirstOrDefault();
                if (cusacid != 0)
                    cashAccId = cusacid;
                else
                {
                    cashAccId = 3;
                }
            }
            long cashaccountid = 3;
            long cardaccountid = (long)db.PaymentMethods.Select(o => o.AccountId).FirstOrDefault();
            
            if (vmodel.orderdata.CustomerType == CustomerType.Card)
            {
                var emid = db.Employees.Where(o => o.UserId == UserId).Select(o => o.EmployeeId).FirstOrDefault();
                var cusacid = db.accountmaps.Where(o => o.EmployeeId == emid && o.PaymentTypeId == EmployeePaymentType.Card).Select(o => o.AccountId).FirstOrDefault();
                if (cusacid != 0)
                    cashAccId = cusacid;
                else
                {
                    cashAccId = 3;
                }
                var cards = db.PaymentMethods.Where(c => c.MethodName == "card").SingleOrDefault();
                cashAccId =(long) cards.AccountId;
            }
            if (payamount > 0 || vmodel.orderdata.CustomerType == CustomerType.Walking)
            {

                var Remark = "Direct Reciept From POS";
                long payid;
                //SETransaction
                SETransaction SEtran = new SETransaction
                {
                    CustomerId = custid,
                    SEPayDate = date,
                    SEPayAmount = payamount,
                    SECreatedDate = today,
                    CreatedBranch = BranchID,
                    CreatedUserId = UserId,
                    SalesEntry = salesEntryId
                };

                long? EmpId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                long? pettyaccid = db.accountmaps.Where(a => a.EmployeeId == EmpId&&a.PaymentTypeId==EmployeePaymentType.Cash).Select(a => a.AccountId).FirstOrDefault();


                if(pettyaccid!=0)
                payid = com.addReceipt(date, custAccID, cashAccId, payamount, payamount, Remark, UserId, BranchID, salesEntryId);
                else
                payid = com.addReceipt(date, custAccID, (long)pettyaccid, payamount, payamount, Remark, UserId, BranchID, salesEntryId);

                SEtran.Recieptid = payid;
                db.SETransactions.Add(SEtran);
                db.SaveChanges();
            }


            if ((payamount == vmodel.orderdata.NetPayable) && (vmodel.orderdata.CustomerType == CustomerType.Walking ))
            {
                com.addAccountTrasaction(vmodel.orderdata.NetPayable, 0, cashAccId, "Sale", salesEntryId, DC.Debit, PosDate);

            }
            else if ((payamount ==0) && (vmodel.orderdata.CustomerType == CustomerType.Walking ))
            {
                com.addAccountTrasaction(vmodel.orderdata.NetPayable, 0, cashAccId, "Sale", salesEntryId, DC.Debit, PosDate);

            }
            else if ((payamount < vmodel.orderdata.NetPayable) && (vmodel.orderdata.CustomerType == CustomerType.Walking ))
            {
                com.addAccountTrasaction(payamount, 0, cashaccountid, "Sale", salesEntryId, DC.Debit, PosDate);
                com.addAccountTrasaction(vmodel.orderdata.NetPayable , 0, (long)cardaccountid, "Sale", salesEntryId, DC.Debit, PosDate);

            }
            else if (vmodel.orderdata.CustomerType == CustomerType.Card)
            { 
                com.addAccountTrasaction(vmodel.orderdata.NetPayable , 0, (long)541, "Sale", salesEntryId, DC.Debit, PosDate);

        }
            else if (vmodel.orderdata.CustomerType == CustomerType.Customer)
            {
                Int64 custAccIDs = db.Customers.Where(a => a.CustomerID == custid).Select(a => a.Accounts).FirstOrDefault();
                com.addAccountTrasaction(vmodel.orderdata.NetPayable, 0, custAccIDs, "Sale", salesEntryId, DC.Debit, PosDate);

            }
            else { }
            //add trasaction to sale account with sale entry credit amount
            com.addAccountTrasaction(0, vmodel.orderdata.NetPayable-vmodel.orderdata.TaxAmount, saleAccId, "Sale", salesEntryId, DC.Credit, PosDate);
            //add sale trasaction with customer debt amount
          
            // add vat output in account transaction
            if (TaxAmount > 0)
                com.addAccountTrasaction(TaxAmount, 0, VATOutput, "Sale", salesEntryId, DC.Debit, date);

            if (payamount > 0 || vmodel.orderdata.CustomerType == CustomerType.Walking)
            {
                //if payment
                long? EmpId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                long? pettyaccid = db.accountmaps.Where(a => a.EmployeeId == EmpId && a.PaymentTypeId==EmployeePaymentType.Cash).Select(a => a.AccountId).FirstOrDefault();

                Int64 custAccIDs = db.Customers.Where(a => a.CustomerID == custid).Select(a => a.Accounts).FirstOrDefault();

                
               
              }
            com.addlog(LogTypes.Created, UserId, "SalesEntry", "SalesEntrys", findip(), salesEntryId, "Successfully Submitted POS Entry");

            return true;
        }

        public JsonResult KotOrders()
        {
            //permission for order delete
            bool checkdel = User.IsInRole("Dev") || (User.IsInRole("POS Delete"));// Delete POS Order

            var tables = (from a in db.POSOrders
                          join b in db.Tables on a.TableId equals b.TableId into tble
                          from b in tble.DefaultIfEmpty()
                          join c in db.Areas on b.AreaId equals c.AreaId into area
                          from c in area.DefaultIfEmpty()
                          join f in db.Customers on a.Customer equals f.CustomerID into cust
                          from f in cust.DefaultIfEmpty()
                          where a.OrderStatus == OrderStatus.PrintKOT || a.OrderStatus == OrderStatus.SaveOrder || a.OrderStatus == OrderStatus.PrintBill
                          orderby a.OrderType, b.AreaId, b.TableId
                          select new
                          {
                              a.TableId,
                              a.POSOrderId,
                              a.OrderStatus,
                              a.OrderType,
                              a.OrderNo,
                              a.NetPayable,
                              a.ItemCount,
                              b.AreaId,
                              a.Discount,
                              c.AreaName,
                              b.TableName,
                              a.taxAFdisc,
                              CustName = f.CustomerName,
                              DelPer = checkdel,
                              a.OrderDate,
                          }).ToList();

            return Json(tables);
        }
        [HttpPost]
        public JsonResult getOrderById(int orderId)
        {

            var tables = (from a in db.POSOrders
                          join b in db.Customers on a.Customer equals b.CustomerID into cust
                          from b in cust.DefaultIfEmpty()
                          join d in db.Employees on a.WaiterId equals d.EmployeeId into emp
                          from d in emp.DefaultIfEmpty()
                          join e in db.Tables on a.TableId equals e.TableId into tbl
                          from e in tbl.DefaultIfEmpty()
                          where a.POSOrderId == orderId
                          select new
                          {
                              a.TableId,
                              Id = a.POSOrderId,
                              a.OrderStatus,
                              a.OrderType,
                              a.PeopleCount,
                              a.OrderNo,
                              a.NetPayable,
                              a.OrderDate,
                              a.CustomerType,
                              a.Customer,
                              a.WaiterId,
                              e.TableName,
                              WaiterName = (d.FirstName + " " + d.LastName),
                              CustName = (a.CustomerType != CustomerType.Walking ? b.CustomerName : (a.custName == null) ? "" : a.custName),
                              Mobile = (a.CustomerType == CustomerType.Walking ? a.custMob : ""),
                              a.ItemCount,
                              a.Discount,
                              a.taxAFdisc
                          }).FirstOrDefault();
            return Json(tables);
        }





        [HttpPost]
        public ActionResult GetOrderItems(long orderId)
        {
            var item = (from a in db.POSOrderItems
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

                        where a.OrderId == orderId 
                        select new
                        {
                            // a.Item,
                            a.ItemQuantity,
                            a.ItemUnit,
                            a.ItemUnitPrice,
                            a.ItemTax,
                            a.ItemSubTotal,
                            a.ItemTaxAmount,
                            a.ItemDiscount,
                            a.ItemTotalAmount,
                            note = a.ItemNote.Replace("<br />", "\n"),
                            a.ItemNote,
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
                            //PriUnCItem = (decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,
                            //SubUnCItem = (decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId).Select(a => a.Quantity).Sum() ?? 0,

                            u.ItemUnitName,
                            // production ----
                            bundle = (from ab in db.POSOrderItems
                                      join bb in db.Items on ab.Item equals bb.ItemID
                                      join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                                      from cb in primary.DefaultIfEmpty()
                                      join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                                      from bd in second.DefaultIfEmpty()

                                      join un in db.ItemUnits on ab.ItemUnit equals un.ItemUnitID into unit
                                      from un in unit.DefaultIfEmpty()
                                      where ab.OrderId == orderId
                                      && a.Item == ab.ItemDiscount
                                      select new
                                      {

                                          bb.ItemCode,
                                          bb.ItemName,
                                          // cb.ItemUnitName,
                                          un.ItemUnitName,
                                          ItemUnitPrice = ab.ItemUnitPrice,
                                          quantity = ab.ItemQuantity,
                                          ItemSubTotal = ab.ItemSubTotal,
                                          ItemTax = ab.ItemTax,
                                          ItemTaxAmount = ab.ItemTaxAmount,
                                          ItemTotalAmount = ab.ItemTotalAmount,

                                          // ab.Item,
                                          ab.ItemQuantity,
                                          ab.ItemUnit,

                                          ItemDiscount = 0,

                                          ItemNote = ab.ItemNote,
                                          ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                          bb.ItemUnitID,
                                          bb.SubUnitId,
                                          PriUnit = cb.ItemUnitName,
                                          SubUnit = bd.ItemUnitName,
                                          bb.ItemArabic,

                                      }).ToList()
                        }).AsEnumerable().Select(o => new
                        {
                            // o.Item,
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
                            o.ItemNote,
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
                            price = o.ItemUnitPrice,
                            o.KeepStock,
                            
                            PriPurchase = (o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)),
                            SubPurchase = (o.SubPurchase % o.ConFactor),

                            PriSale = (o.PriSale + (int)(o.SubSale / o.ConFactor)),
                            SubSale = (o.SubSale % o.ConFactor),

                            PriPReturn = (o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)),
                            SubPReturn = (o.SubPReturn % o.ConFactor),

                            PriSReturn = (o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)),
                            SubSReturn = (o.SubSReturn % o.ConFactor),



                            type = (o.bundle.Any()) ? "Bundle" : "",
                            o.BundleType,
                            o.bundle,
                            o.ItemUnitName,
                        }).ToList();
            var data = new { item = item };
            return Json(data);
        }


        #region order list
        public JsonResult holdOrders()
        {
            bool checkdel = User.IsInRole("Dev") || (User.IsInRole("POS Delete"));// Delete POS Order
            var tables = (from a in db.POSOrders
                          join b in db.Tables on a.TableId equals b.TableId into tble
                          from b in tble.DefaultIfEmpty()
                          join c in db.Areas on b.AreaId equals c.AreaId into area
                          from c in area.DefaultIfEmpty()
                          join d in db.Employees on a.WaiterId equals d.EmployeeId into waiter
                          from d in waiter.DefaultIfEmpty()
                          where a.OrderStatus == OrderStatus.Hold
                          select new
                          {
                              a.TableId,
                              a.POSOrderId,
                              a.OrderStatus,
                              a.OrderType,
                              a.OrderNo,
                              a.NetPayable,
                              a.ItemCount,
                              b.AreaId,
                              c.AreaName,
                              a.OrderDate,
                              a.Discount,
                              waiter = d.FirstName,
                              DelPer = checkdel,
                              a.taxAFdisc
                          }).ToList();

            return Json(tables);
        }
        public JsonResult ListOrders(OrderStatus ostatus)
        {

            bool checkdel = User.IsInRole("Dev") || (User.IsInRole("POS Delete"));// Delete POS Order
            var tables = (from a in db.POSOrders
                          join b in db.Tables on a.TableId equals b.TableId into tble
                          from b in tble.DefaultIfEmpty()
                          join c in db.Areas on b.AreaId equals c.AreaId into area
                          from c in area.DefaultIfEmpty()
                          join d in db.Employees on a.WaiterId equals d.EmployeeId into waiter
                          from d in waiter.DefaultIfEmpty()
                          where a.OrderStatus == ostatus
                          select new
                          {
                              a.TableId,
                              a.POSOrderId,
                              a.OrderStatus,
                              a.OrderType,
                              a.OrderNo,
                              a.NetPayable,
                              a.ItemCount,
                              b.AreaId,
                              c.AreaName,
                              a.OrderDate,
                              a.Discount,
                              waiter = d.FirstName,
                              a.taxAFdisc,
                              DelPer = checkdel
                          }).ToList();

            return Json(tables);
        }
        #endregion

        #region delivery order list
        public JsonResult DeliveryList()
        {

            bool checkdel = User.IsInRole("Dev") || (User.IsInRole("POS Delete"));// Delete POS Order
            var tables = (from a in db.POSOrders
                          join b in db.Tables on a.TableId equals b.TableId into tble
                          from b in tble.DefaultIfEmpty()
                          join c in db.Areas on b.AreaId equals c.AreaId into area
                          from c in area.DefaultIfEmpty()
                          join d in db.Employees on a.WaiterId equals d.EmployeeId
                          join e in db.Users on a.CreatedBy equals e.Id into cashier
                          from e in cashier.DefaultIfEmpty()
                          join f in db.Customers on a.Customer equals f.CustomerID into cust
                          from f in cust.DefaultIfEmpty()
                          where a.OrderStatus != OrderStatus.Payment && a.OrderStatus != OrderStatus.VoidBill && a.OrderType == OrderType.Delivery
                          select new
                          {
                              a.TableId,
                              a.POSOrderId,
                              a.OrderStatus,
                              a.OrderType,
                              a.OrderNo,
                              a.NetPayable,
                              a.ItemCount,
                              b.AreaId,
                              c.AreaName,
                              a.OrderDate,
                              a.Discount,
                              waiter = d.FirstName,
                              waiterid = d.EmployeeId,
                              Cashier = e.UserName,
                              DelPer = checkdel,
                              CustName = (a.CustomerType != CustomerType.Walking ? f.CustomerName : (a.custName == null) ? "" : a.custName),
                              a.taxAFdisc,
                          }).ToList().OrderBy(a => a.waiterid);
            return Json(tables);
        }
        #endregion


        #region duplicate kot
        public JsonResult DuplicateKotOrders()
        {
            //permission for order delete

            var tables = (from a in db.POSOrders
                          join b in db.Tables on a.TableId equals b.TableId into tble
                          from b in tble.DefaultIfEmpty()
                          join c in db.Areas on b.AreaId equals c.AreaId into area
                          from c in area.DefaultIfEmpty()
                          join f in db.Customers on a.Customer equals f.CustomerID into cust
                          from f in cust.DefaultIfEmpty()
                          where a.OrderStatus == OrderStatus.PrintKOT
                          orderby a.OrderType, b.AreaId, b.TableId
                          select new
                          {
                              a.TableId,
                              a.POSOrderId,
                              a.OrderStatus,
                              a.OrderType,
                              a.OrderNo,
                              a.NetPayable,
                              a.ItemCount,
                              b.AreaId,
                              c.AreaName,
                              b.TableName,
                              a.Discount,
                              CustName = (a.CustomerType != CustomerType.Walking ? f.CustomerName : (a.custName == null) ? "" : a.custName),
                              a.taxAFdisc,
                              a.OrderDate,
                              // DelPer = checkdel
                          }).ToList();

            return Json(tables);
        }
        #endregion
        [HttpGet]
        public ActionResult OrdersAndItemById(long orderId)
        {
            var sales = db.POSOrders.Where(c => c.POSOrderId == orderId).Select(a => new
            {
                BillNo = "",//need change
                OrderNo = a.OrderNo,
                Date = a.OrderDate,
                Note = a.OrderNote,
                CustomerType = a.CustomerType,
                Cashier = db.Employees.Where(d => d.EmployeeId == a.WaiterId).Select(d => d.FirstName + " " + d.LastName).FirstOrDefault(),
                SEDiscount = a.Discount,
                SEGrandTotal = a.NetPayable,
                SEPaidAmount = a.NetPayable,
                SEDueAmount = 0,
                SETaxAmount = a.TaxAmount,
                a.OrderStatus,
                a.SubTotal,
                a.OrderType,
                Table = db.Tables.Where(c => c.TableId == a.TableId).Select(c => c.TableName).FirstOrDefault(),
                customer = db.Customers.Where(c => c.CustomerID == a.Customer).Select(c => c.CustomerName).FirstOrDefault(),
                wcustomer = a.custName,
                a.custMob,
                a.taxAFdisc
            }).ToList().Select(o => new
            {
                o.OrderNo,
                o.Date,
                o.Note,
                o.CustomerType,
                o.Cashier,
                o.SEDiscount,
                o.SubTotal,
                o.SEGrandTotal,
                o.SEPaidAmount,
                o.SEDueAmount,
                o.SETaxAmount,
                o.Table,
                o.OrderStatus,
                o.OrderType,
                orderStatus = Enum.GetName(typeof(OrderStatus), o.OrderStatus),
                orderType = Enum.GetName(typeof(OrderType), o.OrderType),
                o.customer,
                o.wcustomer,
                CustName = (o.CustomerType == CustomerType.Walking ? o.wcustomer : o.customer),
                o.custMob,
                o.taxAFdisc
            }).FirstOrDefault();
            var printitem = (from a in db.POSOrderItems
                             join b in db.Items on a.Item equals b.ItemID
                             join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                             from c in primary.DefaultIfEmpty()
                             join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                             from d in second.DefaultIfEmpty()

                             join f in db.ItemUnits on a.ItemUnit equals f.ItemUnitID into unit
                             from f in unit.DefaultIfEmpty()

                             join g in db.ItemBundles on b.ItemID equals g.mainItem into bundle
                             from g in bundle.DefaultIfEmpty()
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
                                 ItemCode = b.ItemCode,
                                 ItemName = b.ItemName,
                                 ItemWithCode = b.ItemCode + " - " + b.ItemName,
                                 b.ItemUnitID,
                                 b.SubUnitId,
                                 PriUnit = c.ItemUnitName,
                                 SubUnit = d.ItemUnitName,
                                 b.ItemArabic,
                                 g.BundleType,
                                 b.ItemType,
                                 a.ItemNote,
                                 f.ItemUnitName,
                                 bundle = (from ab in db.POSOrderItems
                                           join bb in db.Items on ab.Item equals bb.ItemID
                                           join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                                           from cb in primary.DefaultIfEmpty()
                                           join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                                           from bd in second.DefaultIfEmpty()

                                           join un in db.ItemUnits on ab.ItemUnit equals un.ItemUnitID into unit
                                           from un in unit.DefaultIfEmpty()
                                           where ab.OrderId == orderId
                                           && b.ItemID == ab.ItemDiscount
                                           select new
                                           {
                                               bb.ItemCode,
                                               bb.ItemName,
                                               //cb.ItemUnitName,
                                               un.ItemUnitName,
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

                                               ItemNote = ab.ItemNote,
                                               ab.Note,
                                               ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                               bb.ItemUnitID,
                                               bb.SubUnitId,
                                               PriUnit = cb.ItemUnitName,
                                               SubUnit = bd.ItemUnitName,
                                               bb.ItemArabic,
                                               //un.ItemUnitName
                                           }).ToList()
                             }).ToList();

            // return new QuickSoft.Models.LegacyJsonResult { Data = new { printitem = printitem, sales = sales } };
            var data = new { printitem = printitem, sales = sales };
            return Json(data);
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
                            a.OrderRefer,
                            a.SETax
                        }).ToList();
            return Json(data);
        }

        [HttpGet]
        public ActionResult SalesEntryAndItemById(long entryId)
        {

            var sales = (from a in db.SalesEntrys
                         join b in db.Customers on a.Customer equals b.CustomerID into cust
                         from b in cust.DefaultIfEmpty()
                         join c in db.Contacts on b.Contact equals c.ContactID into cnt
                         from c in cnt.DefaultIfEmpty()
                         join f in db.WalkinCustomers on a.SalesEntryId equals f.SalesEntryId into walk
                         from f in walk.DefaultIfEmpty()
                         join d in db.SEPayments on a.SalesEntryId equals d.SalesEntry into pay
                         from d in pay.DefaultIfEmpty()
                         join e in db.Employees on a.SECashier equals e.EmployeeId into user
                         from e in user.DefaultIfEmpty()
                         where a.SalesEntryId == entryId
                         select new
                         {
                             CustomerName = a.CustomerType == CustomerType.Customer ? b.CustomerName : f.CustomerName,
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
                             Address = a.CustomerType == CustomerType.Customer ? (c.Address + " " + c.City + " " + c.State + " " + c.Country + " " + c.Zip) : "",
                             Email = a.CustomerType == CustomerType.Customer ? c.EmailId : "",
                             Phone = a.CustomerType == CustomerType.Customer ? c.Phone : "",
                             Mobile = a.CustomerType == CustomerType.Customer ? c.Mobile : f.MobileNo,
                             TRN = a.CustomerType == CustomerType.Customer ? b.TaxRegNo : "",
                             SubTotal = a.SESubTotal,
                             TermsAndCondition = db.TermsAndConditionss.Where(i => i.ConditionTypeID == "sales").Select(i => i.TermsCondit).FirstOrDefault(),
                             a.SETax

                         }).FirstOrDefault();
            var item = (from a in db.SEItemss
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()

                        join f in db.ItemUnits on a.ItemUnit equals f.ItemUnitID into unit
                        from f in unit.DefaultIfEmpty()

                        join g in db.ItemBundles on b.ItemID equals g.mainItem into bundle
                        from g in bundle.DefaultIfEmpty()
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
                            a.itemNote,
                            ItemNote = a.itemNote,
                            g.BundleType,
                            b.ItemType,
                            f.ItemUnitName,
                            bundle = (from ab in db.SEItemss
                                      join bb in db.Items on ab.Item equals bb.ItemID
                                      join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                                      from cb in primary.DefaultIfEmpty()
                                      join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                                      from bd in second.DefaultIfEmpty()

                                      join un in db.ItemUnits on ab.ItemUnit equals un.ItemUnitID into unit
                                      from un in unit.DefaultIfEmpty()

                                      where ab.SalesEntry == entryId
                                      && b.ItemID == ab.ItemDiscount
                                      select new
                                      {
                                          bb.ItemCode,
                                          bb.ItemName,
                                          // cb.ItemUnitName,
                                          un.ItemUnitName,
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

            var data = new { item = item, sales = sales };
            return Json(data);
        }

        //Delete--> move the order to void bills - from pos entry
        [HttpPost]
        public JsonResult DeleteOrderById(long orderId)
        {
            bool stat = false;
            string msg;
            POSOrder order = db.POSOrders.Find(orderId);
            order.OrderStatus = OrderStatus.VoidBill;
            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();

            stat = true;
            msg = "";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, orderId } };
        }


        [QkAuthorize(Roles = "Dev,VoidBill List")]
        public ActionResult VoidBills()
        {
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,VoidBill List")]
        public ActionResult GetVoidBills()
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
            var v = (from a in db.POSOrders
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join d in db.Users on a.CreatedBy equals d.Id
                     where a.OrderStatus == OrderStatus.VoidBill
                     select new
                     {
                         a.POSOrderId,
                         a.OrderNo,
                         a.BillNo,
                         a.OrderDate,
                         a.NetPayable,
                         Customer = b.CustomerCode + " - " + b.CustomerName,
                         CustomerName = a.custName,
                         a.CustomerType,
                         user = d.UserName,
                         a.OrderType,
                         a.TaxAmount,
                         a.Discount
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.OrderNo.ToString().ToLower().Equals(search.ToLower()));
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


        [QkAuthorize(Roles = "Dev,VoidBill Delete")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            POSOrder porder = db.POSOrders.Find(id);
            if (porder == null)
            {
                return NotFound();
            }
            return PartialView(porder);
        }
        [HttpPost, ActionName("Delete")]
        [QkAuthorize(Roles = "Dev,VoidBill Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            POSOrder order = db.POSOrders.Find(id);
            var Item = db.POSOrderItems.Where(a => a.OrderId == id);
            if (Item != null)
            {
                db.POSOrderItems.RemoveRange(db.POSOrderItems.Where(a => a.OrderId == id));
                db.SaveChanges();
            }

            var oldOrder = db.SalesEntrys.Where(c => c.OrderRefer == id).FirstOrDefault();
            if (oldOrder != null)
            {
                var oldid = oldOrder.SalesEntryId;
                SalesEntry SEen = db.SalesEntrys.Find(oldid);
                var SEItem = db.SEItemss.Where(a => a.SEItemsId == oldid);
                if (SEItem != null)
                {
                    db.SEItemss.RemoveRange(db.SEItemss.Where(a => a.SalesEntry == oldid));
                    db.SaveChanges();
                }
                var rec = db.WalkinCustomers.Where(a => a.SalesEntryId == oldid).FirstOrDefault();
                if (rec != null)
                {
                    db.WalkinCustomers.RemoveRange(db.WalkinCustomers.Where(a => a.SalesEntryId == oldid));
                    db.SaveChanges();
                }
                db.SalesEntrys.Remove(SEen);
                db.SaveChanges();
            }
            db.POSOrders.Remove(order);
            db.SaveChanges();


            com.addlog(LogTypes.Deleted, UserId, "POSOrder", "POSOrders", findip(), order.POSOrderId, "Successfully Deleted POS Orders");
            stat = true;
            msg = "Successfully deleted POS Orders details.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        [HttpPost]
        [QkAuthorize(Roles = "Dev,VoidBill Delete")]
        public ActionResult DeleteAllVoidBill(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = DeleteVBill(arr);
                if (chk == true)
                {
                    count++;
                }
            }
            Success("Deleted " + count + " POS Orders", true);
            return RedirectToAction("VoidBills", "Order");
        }

        private Boolean DeleteVBill(long id)
        {
            var UserId = User.Identity.GetUserId();
            POSOrder order = db.POSOrders.Find(id);
            var Item = db.POSOrderItems.Where(a => a.OrderId == id);
            if (Item != null)
            {
                db.POSOrderItems.RemoveRange(db.POSOrderItems.Where(a => a.OrderId == id));
                db.SaveChanges();
            }

            var oldOrder = db.SalesEntrys.Where(c => c.OrderRefer == id).FirstOrDefault();
            if (oldOrder != null)
            {
                var oldid = oldOrder.SalesEntryId;
                SalesEntry SEen = db.SalesEntrys.Find(oldid);
                var SEItem = db.SEItemss.Where(a => a.SEItemsId == oldid);
                if (SEItem != null)
                {
                    db.SEItemss.RemoveRange(db.SEItemss.Where(a => a.SalesEntry == oldid));
                    db.SaveChanges();
                }
                var rec = db.WalkinCustomers.Where(a => a.SalesEntryId == oldid).FirstOrDefault();
                if (rec != null)
                {
                    db.WalkinCustomers.RemoveRange(db.WalkinCustomers.Where(a => a.SalesEntryId == oldid));
                    db.SaveChanges();
                }
                db.SalesEntrys.Remove(SEen);
                db.SaveChanges();
            }
            db.POSOrders.Remove(order);
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "POSOrder", "POSOrders", findip(), order.POSOrderId, "Successfully Deleted POS Orders");
            return true;
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,View VoidBill")]
        public ActionResult Details(long? id)
        {
            OrderDetailViewModel vmodel = new OrderDetailViewModel();
            vmodel = (from a in db.POSOrders
                      join b in db.Customers on a.Customer equals b.CustomerID into cust
                      from b in cust.DefaultIfEmpty()
                      join c in db.Tables on a.TableId equals c.TableId into table
                      from c in table.DefaultIfEmpty()
                      join d in db.Employees on a.WaiterId equals d.EmployeeId into emp
                      from d in emp.DefaultIfEmpty()
                      join e in db.Users on a.CreatedBy equals e.Id
                      where a.POSOrderId == id
                      select new
                      {
                          a.CustomerType,
                          a.custName,
                          b.CustomerCode,
                          b.CustomerName,
                          a.OrderNo,
                          a.OrderDate,
                          a.Discount,
                          a.TaxAmount,
                          a.NetPayable,
                          c.TableName,
                          a.OrderNote,
                          a.OrderType,
                          a.OrderStatus,
                          a.PeopleCount,
                          a.ItemCount,
                          a.Quantity,
                          a.SubTotal,
                          a.Tax,
                          e.UserName,
                          d.FirstName,
                          d.LastName
                      }).ToList().Select(o => new OrderDetailViewModel
                      {
                          CustomerName = (o.CustomerType == CustomerType.Walking ? o.custName : o.CustomerCode + " - " + o.CustomerName),
                          OrderNo = o.OrderNo,
                          OrderDate = o.OrderDate,
                          Discount = o.Discount,
                          TaxAmount = o.TaxAmount,
                          NetPayable = o.NetPayable,
                          CustomerType = (o.CustomerType == CustomerType.Walking ? "Cash" : "Credit"),
                          TableName = o.TableName,
                          OrderNote = o.OrderNote,
                          OrderType = Enum.GetName(typeof(OrderType), o.OrderType),
                          PeopleCount = o.PeopleCount,
                          OrderStatus = Enum.GetName(typeof(OrderStatus), o.OrderStatus),
                          ItemCount = o.ItemCount,
                          Quantity = o.Quantity,
                          SubTotal = o.SubTotal,
                          Tax = o.Tax,
                          WaiterName = o.FirstName + " " + o.LastName,
                          CreatedBy = o.UserName
                      }).FirstOrDefault();

            vmodel.OrderItems = db.POSOrderItems.Where(a => a.OrderId == id)
            .Select(b => new SEItemViewModel
            {
                ItemUnitPrice = b.ItemUnitPrice,
                ItemQuantity = b.ItemQuantity,
                ItemSubTotal = b.ItemSubTotal,
                ItemTax = b.ItemTax,
                itemNote = b.ItemNote,
                ItemTaxAmount = b.ItemTaxAmount,
                ItemTotalAmount = b.ItemTotalAmount,
                ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault()

            }).ToList();

            return View(vmodel);
        }


        //[HttpPost]



        //    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, orderId } };



        private long GetOrderId(DateTime today)
        {
            Int64 SENo = 0;
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
        private long DGetOrderId()
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
