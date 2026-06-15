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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    public class UpgradeController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public UpgradeController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Upgrade
        public ActionResult Index()
        {
            return View();
        } 
        [HttpPost]
        public JsonResult action()
        {
           
            string msg = "";
            bool stat = true;

            var lastVersion = db.AppVersions.OrderByDescending(x => x.AppVersionId).FirstOrDefault();
            var currentVersion = lastVersion.Versions;
            var upgradeversion = currentVersion;
            var Date = Convert.ToDateTime(System.DateTime.Now);
            if (currentVersion == "1.1.1")
            {
                //upgrade code
                Int64 saleAccId = db.Accountss.Where(a => a.Group == 15).Select(a => a.AccountsID).FirstOrDefault();
                Int64 purAccId = db.Accountss.Where(a => a.Group == 16).Select(a => a.AccountsID).FirstOrDefault();
                Int64 VATOutput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Output").Select(a => a.AccountsID).SingleOrDefault();
                // sale update
                var v = db.SalesEntrys.ToList();
                foreach (var item in v)
                {
                    var acc = db.AccountsTransactions.Where(a => (a.Account == saleAccId) && (a.Purpose == "Sale") && a.reference == item.SalesEntryId).FirstOrDefault();
                    if (acc != null)
                    {
                        var aid = acc.Id;
                        var amount = item.SEGrandTotal - item.SETaxAmount;
                        com.UpdateAccountTrasaction(aid, 0, amount, saleAccId, "Sale", item.SalesEntryId, DC.Credit, acc.Date);
                    }
                    var acc1 = db.AccountsTransactions.Where(a => (a.Account == VATOutput) && (a.Purpose == "Sale") && a.reference == item.SalesEntryId).FirstOrDefault();
                    if (acc1 != null)
                    {
                        var aid = acc1.Id;
                        var amount = item.SETaxAmount;
                        com.UpdateAccountTrasaction(aid, 0, amount, VATOutput, "Sale", item.SalesEntryId, DC.Credit, acc.Date);
                    }
                }
                // sale update
                var sr = db.SalesReturns.ToList();
                foreach (var item in sr)
                {
                    var acc = db.AccountsTransactions.Where(a => (a.Account == saleAccId) && (a.Purpose == "Sale Return") && a.reference == item.SalesReturnId).FirstOrDefault();
                    if (acc != null)
                    {
                        var aid = acc.Id;
                        var amount = item.SRGrandTotal - item.SRTaxAmount;
                        com.UpdateAccountTrasaction(aid, amount, 0, saleAccId, "Sale Return", item.SalesReturnId, DC.Debit, acc.Date);
                    }
                    var acc1 = db.AccountsTransactions.Where(a => (a.Account == VATOutput) && (a.Purpose == "Sale Return") && a.reference == item.SalesEntryId).FirstOrDefault();
                    if (acc1 != null)
                    {
                        var aid = acc1.Id;
                        var amount = item.SRTaxAmount;
                        com.UpdateAccountTrasaction(aid, amount, 0, VATOutput, "Sale Return", item.SalesReturnId, DC.Debit, acc.Date);
                    }
                }
                // purchase update
                var u = db.PurchaseEntrys.ToList();
                foreach (var item1 in u)
                {
                    var acc = db.AccountsTransactions.Where(a => (a.Account == purAccId) && (a.Purpose == "Purchase") && a.reference == item1.PurchaseEntryId).FirstOrDefault();
                    if (acc != null)
                    {
                        var aid = acc.Id;
                        var amount = item1.PEGrandTotal - item1.PETaxAmount;
                        com.UpdateAccountTrasaction(aid, amount, 0, purAccId, "Purchase", item1.PurchaseEntryId, DC.Debit, acc.Date);
                    }
                }
                // purchase return update
                var pr = db.PurchaseReturns.ToList();
                foreach (var item1 in pr)
                {
                    var acc = db.AccountsTransactions.Where(a => (a.Account == purAccId) && (a.Purpose == "Purchase Return") && a.reference == item1.PurchaseReturnId).FirstOrDefault();
                    if (acc != null)
                    {
                        var aid = acc.Id;
                        var amount = item1.PRGrandTotal - item1.PRTaxAmount;
                        com.UpdateAccountTrasaction(aid, 0, amount, purAccId, "Purchase", item1.PurchaseReturnId, DC.Credit, acc.Date);
                    }
                }
                // payment update
                var t = db.Payments.ToList();
                foreach (var item2 in t)
                {
                    var acc = db.AccountsTransactions.Where(a => (a.Account == item2.PayTo) && (a.Purpose == "Payment") && a.reference == item2.PaymentId).FirstOrDefault();
                    if (acc != null)
                    {
                        var aid = acc.Id;
                        var amount = item2.Paying - item2.TaxAmount;
                        com.UpdateAccountTrasaction(aid, amount, 0, item2.PayTo, "Payment", item2.PaymentId, DC.Debit, acc.Date);
                    }
                }

                //expense payment check and delete
                var acctrans = db.AccountsTransactions.Where(a => a.Purpose == "Expense Payment").ToList();
                foreach (var item in acctrans)
                {
                    var paymt = db.Payments.Where(a => a.PaymentId == item.reference).FirstOrDefault();
                    if (paymt == null)
                    {
                        db.AccountsTransactions.RemoveRange(db.AccountsTransactions.Where(a => a.Id == item.Id));
                        db.SaveChanges();
                    }
                }



                //payment voucherno issue
                var payment = db.Payments.Where(a => a.Voucher != 0).ToList();
                Int32 num = 1;
                foreach (var pays in payment)
                {
                    Payment paymnt = db.Payments.Find(pays.PaymentId);
                    paymnt.Voucher = num;
                    paymnt.VoucherNo = Convert.ToString(num);
                    db.Entry(paymnt).State = EntityState.Modified;
                    db.SaveChanges();
                    num++;
                }


                upgradeversion = "1.2";
                msg = "Application Upgraded to Version " + currentVersion + " to Version 1.2";
                AppVersion ap = new AppVersion
                {
                    Versions = upgradeversion,
                    InstallDate = Date
                };
                db.AppVersions.Add(ap);
                db.SaveChanges();
                currentVersion = upgradeversion;
            }
            if (currentVersion == "1.2")
            {

                var sreturn = db.SalesReturns.Where(a => a.CustomerType == CustomerType.Walking).ToList();
                foreach (var entrys in sreturn)
                {
                    var acctrans = db.AccountsTransactions.Where(a => a.reference == entrys.SalesReturnId && a.Purpose == "Sale Return Payment").FirstOrDefault();
                    if (acctrans == null)
                    {
                        Int64 cashAccId = db.Accountss.Where(a => a.Group == 9).Select(a => a.AccountsID).SingleOrDefault();
                        Int64 custAccID = custAccID = db.Customers.Where(a => a.CustomerID == entrys.Customer).Select(a => a.Accounts).FirstOrDefault();

                        com.addAccountTrasaction(entrys.SRGrandTotal, 0, custAccID, "Sale Return Payment", entrys.SalesReturnId, DC.Debit, entrys.SRDate);
                        com.addAccountTrasaction(0, entrys.SRGrandTotal, cashAccId, "Sale Return Payment", entrys.SalesReturnId, DC.Credit, entrys.SRDate);
                    }
                }


                upgradeversion = "1.2.1";
                msg = "Application Upgraded to Version " + currentVersion + " to Version 1.2.1";
                AppVersion ap = new AppVersion
                {
                    Versions = upgradeversion,
                    InstallDate = Date
                };
                db.AppVersions.Add(ap);
                db.SaveChanges();
                currentVersion = upgradeversion;

            }
            if (currentVersion == "1.2.1")
            {
                //issue in sales return/purchase return - type direct & payment type credit
                var UserId = User.Identity.GetUserId();
                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

                var sreturn = db.SalesReturns.Where(a => a.ReturnType == ReturnType.Direct && a.CustomerType == CustomerType.Customer).ToList();
                foreach (var entrys in sreturn)
                {

                    var GrandTotal = entrys.SRGrandTotal;
                    var saleramount = GrandTotal - entrys.SRTaxAmount;
                    Int64 custAccID = db.Customers.Where(a => a.CustomerID == entrys.Customer).Select(a => a.Accounts).FirstOrDefault();
                    Int64 cashAccId = db.Accountss.Where(a => a.Group == 9).Select(a => a.AccountsID).FirstOrDefault();
                    Int64 saleAccId = db.Accountss.Where(a => a.Group == 15).Select(a => a.AccountsID).FirstOrDefault();
                    Int64 VATOutput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Output").Select(a => a.AccountsID).SingleOrDefault();

                    if (entrys.Project != null && entrys.Project != 0)
                    {
                        saleAccId = db.Projects.Where(a => a.ProjectId == entrys.Project).Select(a => a.IncomeAccount).FirstOrDefault();
                    }



                    SRPayment SRpay = db.SRPayments.Where(a => a.SalesReturnId == entrys.SalesReturnId).FirstOrDefault();


                    SRpay.CustomerId = entrys.Customer;
                    SRpay.SRDate = entrys.SRDate;
                    SRpay.SREntryDate = Convert.ToDateTime(System.DateTime.Now);
                    SRpay.SRBillAmount = GrandTotal;

                    
                    SRpay.SReturnAmount = GrandTotal;

                    db.Entry(SRpay).State = EntityState.Modified;
                    db.SRTransactions.RemoveRange(db.SRTransactions.Where(a => a.SalesReturnId == entrys.SalesReturnId));
                    db.Payments.RemoveRange(db.Payments.Where(a => a.Reference == entrys.SalesReturnId && a.RefType == "SalesReturn"));
                    db.SaveChanges();


                    if (entrys.SRGrandTotal > 0)
                    {
                        var Remark = "Direct Payment From SalesReturn";
                        long payid;
                        //SETransaction
                        SRTransaction SRtran = new SRTransaction();

                        SRtran.CustomerId = entrys.Customer;
                        SRtran.SRPayDate = entrys.SRDate;
                        
                        SRtran.SRPayAmount = GrandTotal;
                        payid = com.addPayment(entrys.SRDate, cashAccId, custAccID, GrandTotal, GrandTotal, GrandTotal, Remark, UserId, BranchID, entrys.SalesReturnId, "SalesReturn");
                        
                        SRtran.PaymentId = payid;
                        SRtran.SRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                        SRtran.CreatedBranch = Convert.ToInt32(BranchID);
                        SRtran.CreatedUserId = UserId;
                        SRtran.Status = 1;
                        SRtran.SalesReturnId = entrys.SalesReturnId;
                        db.SRTransactions.Add(SRtran);
                        db.SaveChanges();
                    }


                    //change bill
                    var chkrec = db.PaymentBills.Where(a => a.InvoiceNo == entrys.SalesReturnId && a.BillType == "Sales Return").ToList();
                    if (chkrec != null)
                    {
                        db.PaymentBills.Where(a => a.InvoiceNo == entrys.SalesReturnId && a.BillType == "Sales Return").ToList().ForEach(a => a.Type = "New Reference");
                        db.SaveChanges();
                    }



                    bool delete = com.DeleteAllAccountTransaction("Sale Return", entrys.SalesReturnId);
                    bool deletepay = com.DeleteAllAccountTransaction("Sale Return Payment", entrys.SalesReturnId);


                    //bill sundry account
                    var Gtotal = GrandTotal;
                    var billsundry = db.SRBillSundrys.Where(a => a.SalesReturnId == entrys.SalesReturnId).ToList();
                    if (billsundry.Count() > 0)
                    {
                        foreach (var bs in billsundry)
                        {
                            var ChkAcc = db.BillSundrys.Where(a => a.BillSundryId == bs.BillSundry).FirstOrDefault();
                            if (ChkAcc.SAccount != null && ChkAcc.SAccount != 0)
                            {
                                var bsamount = (decimal)bs.BsAmount;
                                if (ChkAcc.BSType == 0)//additive
                                {
                                    saleramount = saleramount - bsamount;
                                    com.addAccountTrasaction((decimal)bs.BsAmount, 0, (long)ChkAcc.SAccount, "Sale Return", entrys.SalesReturnId, DC.Debit, entrys.SRDate, null, null, entrys.Project, entrys.ProTask);
                                }
                                else //substract
                                {
                                    saleramount = saleramount + bsamount;
                                    com.addAccountTrasaction(0, (decimal)bs.BsAmount, (long)ChkAcc.SAccount, "Sale Return", entrys.SalesReturnId, DC.Credit, entrys.SRDate, null, null, entrys.Project, entrys.ProTask);
                                }
                            }
                        }
                    }

                    com.addAccountTrasaction(saleramount, 0, saleAccId, "Sale Return", entrys.SalesReturnId, DC.Debit, entrys.SRDate, null, null, entrys.Project, entrys.ProTask);
                    com.addAccountTrasaction(0, GrandTotal, custAccID, "Sale Return", entrys.SalesReturnId, DC.Credit, entrys.SRDate, null, null, entrys.Project, entrys.ProTask);


                    if (entrys.SRTaxAmount > 0)
                        com.addAccountTrasaction(entrys.SRTaxAmount, 0, VATOutput, "Sale Return", entrys.SalesReturnId, DC.Debit, entrys.SRDate, null, null, entrys.Project, entrys.ProTask);
                    if (Convert.ToDecimal(GrandTotal) > 0 )
                    {
                        //if payment
                        com.addAccountTrasaction(GrandTotal, 0, custAccID, "Sale Return Payment", entrys.SalesReturnId, DC.Debit, entrys.SRDate, null, null, entrys.Project, entrys.ProTask);
                        com.addAccountTrasaction(0, GrandTotal, cashAccId, "Sale Return Payment", entrys.SalesReturnId, DC.Credit, entrys.SRDate, null, null, entrys.Project, entrys.ProTask);
                    }
                    com.addlog(LogTypes.Updated, UserId, "SaleReturn", "SaleReturns", findip(), entrys.SalesReturnId, "Successfully Updated Sales Return");
                    var update = com.CusPayment(custAccID, entrys.SRDate, BranchID, UserId);

                    SalesReturn SRentry = db.SalesReturns.Find(entrys.SalesReturnId);
                    SRentry.CustomerType = CustomerType.Walking;
                    db.Entry(SRentry).State = EntityState.Modified;
                    db.SaveChanges();
                }


                var preturn = db.PurchaseReturns.Where(a => a.ReturnType == ReturnType.Direct && a.SupplierType == SupplierType.CreditSale).ToList();
                foreach (var pentrys in preturn)
                {
                    var pGrandTotal = pentrys.PRGrandTotal;
                    var purchaseramount = pGrandTotal - pentrys.PRTaxAmount;

                    Int64 suppAccID = db.Suppliers.Where(a => a.SupplierID == pentrys.Supplier).Select(a => a.Accounts).FirstOrDefault();
                    Int64 purAccId = db.Accountss.Where(a => a.Group == 16).Select(a => a.AccountsID).FirstOrDefault();
                    Int64 cashAccId = db.Accountss.Where(a => a.Group == 9).Select(a => a.AccountsID).FirstOrDefault();
                    Int64 VATInput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Input").Select(a => a.AccountsID).SingleOrDefault();

                    PRPayment PRpay = db.PRPayments.Where(a => a.PurchaseReturnId == pentrys.PurchaseReturnId).FirstOrDefault();

                    PRpay.SupplierId = pentrys.Supplier;
                    PRpay.PRDate = pentrys.PRDate;
                    PRpay.PREntryDate = Convert.ToDateTime(System.DateTime.Now);
                    PRpay.PRBillAmount = pGrandTotal;

                    PRpay.PReturnAmount = pGrandTotal;
                    
                    PRpay.CreatedBranch = Convert.ToInt32(BranchID);
                    PRpay.CreatedUserId = UserId;
                    PRpay.PRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                    PRpay.Status = 1;
                    PRpay.PurchaseReturnId = pentrys.PurchaseReturnId;

                    db.Entry(PRpay).State = EntityState.Modified;
                    db.SaveChanges();


                    db.PRTransactions.RemoveRange(db.PRTransactions.Where(a => a.PurchaseReturnId == pentrys.PurchaseReturnId));
                    db.Receipts.RemoveRange(db.Receipts.Where(a => a.Reference == pentrys.PurchaseReturnId && a.RefType == "Purchase Return"));
                    db.SaveChanges();

                    if (pentrys.PRGrandTotal > 0)
                    {

                        var Remark = "Receipt From Purchase Return";
                        var reftype = "Purchase Return";
                        long recid;
                        //PRTransaction
                        PRTransaction PRtran = new PRTransaction();
                        PRtran.SupplierId = pentrys.Supplier;
                        PRtran.PRPayDate = pentrys.PRDate;
                        
                            PRtran.PRPayAmount = pGrandTotal;
                            recid = com.addReceipt(pentrys.PRDate, suppAccID, cashAccId, pGrandTotal, pGrandTotal, Remark, UserId, BranchID, pentrys.PurchaseReturnId, reftype);
                       
                        PRtran.Recieptid = recid;
                        PRtran.PRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                        PRtran.CreatedBranch = Convert.ToInt32(BranchID);
                        PRtran.CreatedUserId = UserId;
                        PRtran.Status = 1;
                        PRtran.PurchaseReturnId = pentrys.PurchaseReturnId;

                        db.PRTransactions.Add(PRtran);
                        db.SaveChanges();

                    }

                    //receipt bill changes
                    var chkrec = db.ReceiptBills.Where(a => a.InvoiceNo == pentrys.PurchaseReturnId && a.BillType == "Purchase Return").ToList();
                    if (chkrec != null)
                    {
                        db.ReceiptBills.Where(a => a.InvoiceNo == pentrys.PurchaseReturnId && a.BillType == "Purchase Return").ToList().ForEach(a => a.Type = "New Reference");
                        db.SaveChanges();
                    }

                    bool delete = com.DeleteAllAccountTransaction("Purchase Return", pentrys.PurchaseReturnId);
                    bool deletepay = com.DeleteAllAccountTransaction("Purchase Return Payment", pentrys.PurchaseReturnId);

                    var Gtotal = pGrandTotal;
                    var pbillsundry = db.PRBillSundrys.Where(a => a.PurchaseReturnId == pentrys.PurchaseReturnId).ToList();
                    if (pbillsundry != null)
                    {
                        foreach (var bs in pbillsundry)
                        {
                            var ChkAcc = db.BillSundrys.Where(a => a.BillSundryId == bs.BillSundry).FirstOrDefault();
                            if (ChkAcc.PAccount != null && ChkAcc.PAccount != 0)
                            {
                                var bsamount = (decimal)bs.BsAmount;
                                if (ChkAcc.BSType == 0)//additive
                                {
                                    purchaseramount = purchaseramount - bsamount;
                                    com.addAccountTrasaction(0, (decimal)bs.BsAmount, (long)ChkAcc.PAccount, "Purchase Return", pentrys.PurchaseReturnId, DC.Credit, pentrys.PRDate);
                                }
                                else //substract
                                {
                                    purchaseramount = purchaseramount + bsamount;
                                    com.addAccountTrasaction((decimal)bs.BsAmount, 0, (long)ChkAcc.PAccount, "Purchase Return", pentrys.PurchaseReturnId, DC.Debit, pentrys.PRDate);
                                }
                            }
                        }
                    }

                    //add trasaction to purchase account
                    com.addAccountTrasaction(0, purchaseramount, purAccId, "Purchase Return", pentrys.PurchaseReturnId, DC.Credit, pentrys.PRDate);
                    //add purchase trasaction 
                    com.addAccountTrasaction(pGrandTotal, 0, suppAccID, "Purchase Return", pentrys.PurchaseReturnId, DC.Debit, pentrys.PRDate);
                    // add vat input in account transaction
                    if (pentrys.PRTaxAmount > 0)
                        com.addAccountTrasaction(0, pentrys.PRTaxAmount, VATInput, "Purchase Return", pentrys.PurchaseReturnId, DC.Credit, pentrys.PRDate);
                    if (Convert.ToDecimal(pGrandTotal) > 0)
                    {
                        //if payment
                        com.addAccountTrasaction(0, pGrandTotal, suppAccID, "Purchase Return Payment", pentrys.PurchaseReturnId, DC.Credit, pentrys.PRDate);
                        com.addAccountTrasaction(pGrandTotal, 0, cashAccId, "Purchase Return Payment", pentrys.PurchaseReturnId, DC.Debit, pentrys.PRDate);
                    }
                    var update = com.SuplPayment(suppAccID, pentrys.PRDate, BranchID, UserId);

                    PurchaseReturn PRentry = db.PurchaseReturns.Find(pentrys.PurchaseReturnId);
                    PRentry.SupplierType = SupplierType.CashSale;
                    db.Entry(PRentry).State = EntityState.Modified;
                    db.SaveChanges();
                }



                upgradeversion = "1.2.2";
                msg = "Application Upgraded to Version " + currentVersion + " to Version 1.2.2";
                AppVersion ap = new AppVersion
                {
                    Versions = upgradeversion,
                    InstallDate = Date
                };
                db.AppVersions.Add(ap);
                db.SaveChanges();
                currentVersion = upgradeversion;
            }


                // v1.0.7


                //// add vat output in account transaction
                //// avoid repeatedly adding
                ////var dlt1 = "DELETE FROM [dbo].[AccountsTransactions] WHERE [dbo].[AccountsTransactions].[Account]=502";
                ////var dlts1 = db.Database.ExecuteSqlRaw(dlt1);

                //// add vat input in account transaction
                //// avoid repeatedly adding
                ////var dlt2 = "DELETE FROM [dbo].[AccountsTransactions] WHERE [dbo].[AccountsTransactions].[Account]=501";
                ////var dlts2 = db.Database.ExecuteSqlRaw(dlt2);
                //// end v1.0.7

                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }

        // GET: Upgrade
        public ActionResult EditIssue()
        {
            LedgerViewModel vmodel = new LedgerViewModel();
            var Reciept = (from a in db.Receipts
                           join b in db.Accountss on a.PayFrom equals b.AccountsID
                           join c in db.Accountss on a.PayTo equals c.AccountsID
                           join d in db.SalesEntrys on a.Reference equals d.SalesEntryId into sale
                           from d in sale.DefaultIfEmpty()
                           where a.RefType != "Direct Receipt"
                           select new
                           {
                               id = a.ReceiptId,
                               particulars = c.Name,
                               Date = (DateTime?)a.Date,
                               Invoice = (a.editable == choice.Yes) ? a.VoucherNo : (d.BillNo != null) ? d.BillNo : "",
                               Type = a.RefType,
                               RAccount = (b.Name),
                               RAccountID = (long?)b.AccountsID,
                               Debit = (decimal?)null,
                               Credit = (decimal?)a.Paying,
                               entry = (DateTime?)a.CreatedDate
                           }).OrderBy(a=>a.Invoice);
            
            vmodel.Ledger = (from a in Reciept
                             select new Ledger
                             {
                                 Date = a.Date,
                                 Invoice = a.Invoice,
                                 Type = a.Type,
                                 RAccount = a.RAccount,
                                 RAccountID = a.RAccountID,
                                 Debit = a.Debit,
                                 Credit = a.Credit,
                                 particulars = a.particulars
                             }).ToList();
            var data = (from a in db.Receipts
                        join d in db.SalesEntrys on a.Reference equals d.SalesEntryId into sale
                        from d in sale.DefaultIfEmpty()
                        where a.RefType == "Sales"
                        select new
                        {
                            id = a.ReceiptId,
                            Date = (DateTime?)a.Date,
                            Invoice = d.BillNo,
                            Type = a.RefType,
                            Credit = (decimal?)a.Paying,
                            entry = (DateTime?)a.CreatedDate,
                            refer = a.Reference
                        }).OrderBy(a => a.Invoice).ThenBy(a => a.id);
            long? refer = 0;
            long previd = 0;
            var count = 1;
            foreach (var d in data)
            {
                if (refer != d.refer && count == 0)
                {
                    vmodel.Ledger.Remove(vmodel.Ledger.Where(a => a.RAccountID == previd).FirstOrDefault());
                    
                }
                if(refer != d.refer)
                {
                    count = 0;
                }
                else
                {
                    count++;
                }
                vmodel.Ledger.Add(new Ledger
                {
                    Date = d.Date,
                    Invoice = d.Invoice,
                    Type = d.Type,
                    RAccount = " current "+d.refer + " previous "+ refer + " count " + count,
                    RAccountID = d.id,
                    Debit = d.id,
                    Credit = d.Credit,
                    particulars = "testtttttt"
                });
                if (refer == 0)
                {
                    foreach (var item in vmodel.Ledger.ToList())
                    {
                        if (item.particulars != "testtttttt")
                        {
                            vmodel.Ledger.Remove(item);    //Will work!
                        }
                    }
                }
                refer = d.refer;
                previd = d.id;
            }
            if (count == 0)
            {
                vmodel.Ledger.Remove(vmodel.Ledger.Where(a => a.RAccountID == previd).FirstOrDefault());

            }

            vmodel.OpeningBalance = 0;
            vmodel.blnceType = "Credit";
            vmodel.MainAccount = "All";
            vmodel.MainAccountID = 1;
            vmodel.from = null;
            vmodel.to = null;
            companySet();
            return View(vmodel);
        }

        
        [HttpPost]
        public JsonResult DeleteData()
        {
            bool stat = false;
            string msg;
                var data = (from a in db.Receipts
                            join d in db.SalesEntrys on a.Reference equals d.SalesEntryId into sale
                            from d in sale.DefaultIfEmpty()
                            where a.RefType == "Sales"
                            select new
                            {
                                id = a.ReceiptId,
                                Date = (DateTime?)a.Date,
                                Invoice = d.BillNo,
                                Type = a.RefType,
                                Credit = (decimal?)a.Paying,
                                entry = (DateTime?)a.CreatedDate,
                                refer = a.Reference
                            }).OrderBy(a => a.Invoice).ThenBy(a => a.id).ToList();
                long? refer = 0;
                long previd = 0;
                var count = 1;
                var dlt = new List<long>();
                var i = 0;

                foreach (var d in data)
                {
                    if (refer != d.refer)
                    {
                        count = 0;
                    }
                    else
                    {
                        count++;
                    }
                    if (refer == d.refer && count != 0)
                    {
                        dlt.Add(previd);
                        i++;
                    }
                    refer = d.refer;
                    previd = d.id;
                }
                if (dlt != null)
                {
                    db.Receipts.RemoveRange(db.Receipts.Where(a => dlt.Contains(a.ReceiptId)));
                    db.SaveChanges();
                }
                
                stat = true;
                msg = "Successfully deleted Duplicate entry.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg} };
            //    return new QuickSoft.Models.LegacyJsonResult
            //        Data = new
            //            status = stat,
            //            message = msg,
            //            ex.Message
        }

    }
}
