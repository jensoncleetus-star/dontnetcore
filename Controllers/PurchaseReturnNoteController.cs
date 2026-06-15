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
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class PurchaseReturnNoteController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PurchaseReturnNoteController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: PurchaseReturn 
        [QkAuthorize(Roles = "Dev,Purchase Return List")]
        public ActionResult Index()
        {
            ViewBag.Supplier = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);

            ViewBag.SalesExecutive = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
            }, "Value", "Text", 1);
            ViewBag.Balance = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value=null},
                new SelectListItem() {Text = "Fully Paid", Value="0"},
                new SelectListItem() {Text = "Pending", Value="1"},
            }, "Value", "Text");
            var created = db.Users.Select(s => new
            {
                ID = s.Id,
                Name = s.UserName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");
            _FinancialYear();
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            if (mcchk != null)
            {
                var mcs = db.MCs.Where(s => s.AssignedUser == UserId).Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
                ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();
            }
            else
            {
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
                ViewBag.LastMc = mcs.Select(a => a.Id).FirstOrDefault();
            }

            var MlaPReturn = db.EnableSettings.Where(a => a.EnableType == "MLAPReturn").FirstOrDefault();
            var MlaPReturns = MlaPReturn != null ? MlaPReturn.Status : Status.inactive;
            ViewBag.MLAPReturn = MlaPReturns;

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindPReturn").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

            return View();
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Purchase Return Entry")]
        public ActionResult Create()
        {
            var preturn = new PurchaseReturnViewModel
            {
                BillNo = InvoiceNo(),
                PRDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                PRNote = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "preturn").Select(a => a.TermsCondit).FirstOrDefault(),
                PurchaseTypes = db.PurchaseTypes.ToList(),
            };
            companySet();
            var userpermission = User.IsInRole("All Purchase Return Entry");
            var UserId = User.Identity.GetUserId();

            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            ViewBag.LastEntry = db.PurchaseReturns.Where(p => (!MCList.Any() || MCArray.Contains(p.MaterialCenter)) && (userpermission == true || p.CreatedBy == UserId)).Select(p => p.PurchaseReturnId).AsEnumerable().DefaultIfEmpty(0).Max();

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            if (mcchk != null)
            {
                var mcs = db.MCs.Where(s => s.AssignedUser == UserId).Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
            }
            else
            {
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
            }

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;
            _FinancialYear();

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var pos = db.EnableSettings.Where(a => a.EnableType == "POSInvoice").FirstOrDefault();
            var poscheck = pos != null ? pos.Status : Status.inactive;
            ViewBag.POS = poscheck;

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
               .Select(s => new
               {
                   ID = s.EmployeeId,
                   Name = s.FirstName + " " + s.LastName
               })
               .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var MlaPReturn = db.EnableSettings.Where(a => a.EnableType == "MLAPReturn").FirstOrDefault();
            var MlaPReturns = MlaPReturn != null ? MlaPReturn.Status : Status.inactive;
            ViewBag.MLAPReturn = MlaPReturns;

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            ViewBag.DefMc = db.MCs.Where(a => a.MCName == "Hire").Select(a => a.MCId).FirstOrDefault();

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;
            //field mapping
            preturn.FieldMap = db.FieldMappings.Where(a => a.Section == "PReturn" && a.Status == Status.active).ToList();
            var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
            var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
            ViewBag.PrintLayout = PriLay;
            var invoice = db.InvoiceLayouts
                             .Select(s => new
                             {
                                 ID = s.Id,
                                 Name = s.Name,
                             })
                             .ToList();
            ViewBag.printlay = QkSelect.List(invoice, "ID", "Name");
            var rtype = Request.Query["rtype"];
            if (rtype == "APP")
            {
                return View("App/Create", preturn);
            }
            else
            {
                return View(preturn);
            }
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Purchase Return Entry")]
        public JsonResult CreatePurchaseReturn(string[][] array, string[] purchasedata, PRBillSundryViewModel bsmodel, ICollection<BatchStockPViewModel> bstmodel)
        {
            bool stat = false;
            string msg;
            if (!BillExist(Convert.ToString(purchasedata[16])))
            {
                //add to saleEntries


                var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
                var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

                var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
                var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

                var UserId  = User.Identity.GetUserId();
                var Today   = Convert.ToDateTime(System.DateTime.Now);

                long Branch = 0;
                Int64 purRAcc = (long)db.companys.Select(a => a.PReturnAccount).FirstOrDefault();

                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                if (BranchCheck == Status.active)
                {
                    Branch = Convert.ToInt64(purchasedata[20]);
                }
                else
                {
                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                }

                var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
                var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;

                long MC = 0;
                if (MCcheck == Status.active)
                {
                    MC = Convert.ToInt64(purchasedata[19]);
                }
                else
                {
                    MC = 1;
                }

                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                var TaxAmount = Convert.ToDecimal(purchasedata[5]);
                var GrandTotal = Convert.ToDecimal(purchasedata[7]);
                var purRamount = GrandTotal - TaxAmount;
                var subtotal = Convert.ToDecimal(purchasedata[8]);

                //sales entry
                PurchaseReturn PRentry = new PurchaseReturn();

                PRentry.PurType = (purchasedata[28] == "1") ? PurchaseHireType.CrossHire : PurchaseHireType.Purchase;
                PRentry.PRNo = GetPrNo();
                PRentry.BillNo = purchasedata[16];
                PRentry.purchaseEntryId = Convert.ToInt64(purchasedata[14]);
                PRentry.PRDate = DateTime.Parse(purchasedata[2], new CultureInfo("en-GB"));
                PRentry.PRCashier = purchasedata[1] != "" ? Convert.ToInt64(purchasedata[1]) : 0;
                PRentry.Supplier = Convert.ToInt64(purchasedata[0]);
                PRentry.PayType = "";//need change
                PRentry.PRItems = Convert.ToInt32(purchasedata[3]);
                PRentry.PRItemQuantity = Convert.ToDecimal(purchasedata[4]);
                PRentry.PRSubTotal = Convert.ToDecimal(purchasedata[8]);
                PRentry.PRTax = Convert.ToDecimal(purchasedata[9]);
                PRentry.PRTaxAmount = TaxAmount;
                PRentry.PRDiscount = Convert.ToDecimal(purchasedata[6]);
                PRentry.PRGrandTotal = GrandTotal;
                PRentry.PRNote = purchasedata[11];
                PRentry.Print = 1;
                PRentry.PRCreatedDate = Today;
                PRentry.CreatedBy = UserId;
                PRentry.Status = 1;
                PRentry.Branch = Branch;
                PRentry.Remarks = purchasedata[18];
                PRentry.MaterialCenter = MC;
                PRentry.PurchaseType = Convert.ToInt64(purchasedata[21]);
                if (purchasedata[12] == "1")
                {
                    PRentry.SupplierType = SupplierType.CashSale;
                }
                else
                {
                    PRentry.SupplierType = SupplierType.CreditSale;
                }
                if (purchasedata[15] == "1")
                {
                    PRentry.ReturnType = ReturnType.Direct;
                }
                else
                {
                    PRentry.ReturnType = ReturnType.AgainstBill;
                }

                PRentry.Ref1 = Convert.ToString(purchasedata[23]);
                PRentry.Ref2 = Convert.ToString(purchasedata[24]);
                PRentry.Ref3 = Convert.ToString(purchasedata[25]);
                PRentry.Ref4 = Convert.ToString(purchasedata[26]);
                PRentry.Ref5 = "Debit Note";
                PRentry.PReturnAccount = purRAcc;
                db.PurchaseReturns.Add(PRentry);
                db.SaveChanges();

                //To Update the quantity in Create Mode(ItemTransaction Table)
                com.ItemTransInCreateMode("PurchaseReturn", MC, 0, 0, array, UserId, Today);

                Int64 PurchaseReturnId = PRentry.PurchaseReturnId;
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
                dtItem.Columns.Add("PurchaseReturnId");
                dtItem.Columns.Add("Item");

                foreach (var arr in array)
                {
                    DataRow dr = dtItem.NewRow();
                    dr["ItemUnit"] = arr[1];
                    dr["ItemUnitPrice"] = Convert.ToDecimal(arr[3]);
                    dr["ItemQuantity"] = Convert.ToDecimal(arr[2]);
                    dr["ItemSubTotal"] = Convert.ToDecimal(arr[5]);
                    dr["ItemDiscount"] = Convert.ToDecimal(arr[6]);
                    dr["ItemTax"] = Convert.ToDecimal(arr[10]);
                    dr["ItemTaxAmount"] = Convert.ToDecimal(arr[9]);
                    dr["ItemTotalAmount"] = Convert.ToDecimal(arr[11]);
                    dr["itemNote"] = Convert.ToString(arr[29].Replace("\n", "<br />"));
                    dr["PurchaseReturnId"] = PurchaseReturnId;
                    dr["Item"] = Convert.ToInt32(arr[0]);
                    dtItem.Rows.Add(dr);

                    var item = Convert.ToInt32(arr[0]);
                    var chkbundle = db.ItemBundles.Where(a => a.mainItem == item).Select(a => a.ItemBundleId).FirstOrDefault();
                    if (chkbundle > 0)
                    {
                        var bunQuan = Convert.ToDecimal(arr[2]);
                        var itemBundle = (from g in db.ItemBundles
                                          join b in db.Items on g.mainItem equals b.ItemID
                                          where b.ItemID == item
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

                            decimal itemtax = 0;
                            decimal taxamt = 0;
                            decimal totamt = 0;

                            itemtax = bu.ItemTax;
                            taxamt = buTaxAmount;
                            totamt = (buTaxAmount + ItemSubTotal);

                            DataRow dbu = dtItem.NewRow();
                            dbu["ItemUnit"] = bu.ItemUnit;
                            dbu["ItemUnitPrice"] = bu.ItemUnitPrice;
                            dbu["ItemQuantity"] = (bunQuan * bu.quantity);
                            dbu["ItemSubTotal"] = ItemSubTotal;
                            // add parent itemid in discount for reference
                            dbu["ItemDiscount"] = item;
                            dbu["ItemTax"] = itemtax;
                            dbu["ItemTaxAmount"] = taxamt;
                            dbu["ItemTotalAmount"] = totamt;
                            dbu["itemNote"] = "-:{Bundle_Item}";
                            dbu["PurchaseReturnId"] = PurchaseReturnId;
                            dbu["Item"] = bu.Item;
                            dtItem.Rows.Add(dbu);
                        }
                    }

                }

                SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = "TableTypePRItems";
                //// execute sp sql 
                string sql = String.Format("EXEC {0} {1};", "SP_InsertPRItemNotes", "@TableType");
                //// execute sql 
                db.Database.ExecuteSqlRaw(sql, parameter);

                // batch stock
                if (bstmodel != null)
                {
                    foreach (var bst in bstmodel)
                    {
                        if (bst.BatchNo != "" && bst.BatchNo != null)
                        {
                            DateTime? exp = null;
                            DateTime? mfg = null;
                            if (bst.EXP != null && bst.EXP != "")
                            {
                                exp = DateTime.Parse(bst.EXP, new CultureInfo("en-GB"));
                            }
                            if (bst.MFG != null && bst.MFG != "")
                            {
                                mfg = DateTime.Parse(bst.MFG, new CultureInfo("en-GB"));
                            }
                            decimal bStock = 0;
                            if (bst.Unit == bst.Priunit)
                            {
                                bStock = bst.StockOut * bst.cfactor;
                            }
                            BatchStock Btst = new BatchStock();
                            Btst.BatchNo = bst.BatchNo;
                            Btst.Item = bst.Item;
                            Btst.Unit = bst.Unit;
                            Btst.Cost = bst.Cost;
                            Btst.StockOut = bStock;
                            Btst.StockIn = 0;
                            Btst.Order = bst.Order;
                            Btst.EXP = exp;
                            Btst.MFG = mfg;
                            Btst.Reference = PurchaseReturnId;
                            Btst.Type = "PReturn";

                            Btst.CreatedDate = Today;
                            Btst.Date = DateTime.Parse(purchasedata[2], new CultureInfo("en-GB"));


                            db.BatchStocks.Add(Btst);
                        }
                    }
                    db.SaveChanges();
                }
                decimal tem = 0;
                if (bsmodel.prbsundrys != null)
                {
                    foreach (var bs in bsmodel.prbsundrys)
                    {
                        if (bs.BsAmount == null)
                        {
                            bs.BsAmount = tem;
                        }
                    }
                }
                //billsundry
                if (bsmodel.prbsundrys != null)
                {
                    string bsResult = string.Empty;

                    DataTable BsEntry = new DataTable();
                    BsEntry.Columns.Add("PurchaseReturnId");
                    BsEntry.Columns.Add("BillSundry");
                    BsEntry.Columns.Add("BsValue");
                    BsEntry.Columns.Add("AmountType");
                    BsEntry.Columns.Add("BsType");
                    BsEntry.Columns.Add("BsAmount");

                    foreach (var bs in bsmodel.prbsundrys)
                    {
                        DataRow drw = BsEntry.NewRow();
                        drw["PurchaseReturnId"] = PurchaseReturnId;
                        drw["BillSundry"] = bs.BillSundry;
                        drw["BsValue"] = bs.BsValue;
                        drw["AmountType"] = bs.AmountType;
                        drw["BsType"] = bs.BsType;
                        drw["BsAmount"] = bs.BsAmount;

                        BsEntry.Rows.Add(drw);
                    }

                    ////// create parameter 
                    SqlParameter parameter1 = new SqlParameter("@TableType", BsEntry);
                    parameter1.SqlDbType = SqlDbType.Structured;
                    parameter1.TypeName = "TableTypePRBillSundry";
                    //// execute sp sql 
                    string sql1 = String.Format("EXEC {0} {1};", "SP_InsertPRBillSundry", "@TableType");
                    //// execute sql 
                    db.Database.ExecuteSqlRaw(sql1, parameter1);
                    //-------------------------------------
                }

                //SEPayment
                PRPayment PRpay = new PRPayment();

                PRpay.SupplierId = Convert.ToInt64(purchasedata[0]);
                PRpay.PRDate = DateTime.Parse(purchasedata[2], new CultureInfo("en-GB"));
                PRpay.PREntryDate = Today;
                PRpay.PRBillAmount = GrandTotal;

                if (purchasedata[12] == "1")
                {
                    PRpay.PReturnAmount = Convert.ToDecimal(purchasedata[7]);
                }
                else
                {
                    PRpay.PReturnAmount = Convert.ToDecimal(purchasedata[10]);
                }

                PRpay.CreatedBranch = Convert.ToInt32(BranchID);
                PRpay.CreatedUserId = UserId;
                PRpay.PRCreatedDate = Today;
                PRpay.Status = 1;
                PRpay.PurchaseReturnId = PurchaseReturnId;

                db.PRPayments.Add(PRpay);
                db.SaveChanges();

                Int64 suppAccID = db.Suppliers.Where(a => a.SupplierID == PRentry.Supplier).Select(a => a.Accounts).FirstOrDefault();
                Int64 purAccId = purRAcc;// db.Accountss.Where(a => a.Group == 16).Select(a => a.AccountsID).FirstOrDefault();
                Int64 cashAccId = db.Accountss.Where(a => a.Group == 9).Select(a => a.AccountsID).FirstOrDefault();
                Int64 VATInput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Input").Select(a => a.AccountsID).SingleOrDefault();

                decimal amount = Convert.ToDecimal(purchasedata[10]);
                if (purchasedata[12] == "1")
                {
                    amount = GrandTotal;
                }


                var date = DateTime.Parse(purchasedata[2], new CultureInfo("en-GB"));
                if (Convert.ToDecimal(purchasedata[10]) > 0 || purchasedata[12] == "1")
                {

                    var Remark = "Receipt From Purchase Return";
                    var reftype = "Purchase Return";
                    long recid;
                    //PRTransaction
                    PRTransaction PRtran = new PRTransaction();
                    PRtran.SupplierId = Convert.ToInt64(purchasedata[0]);
                    PRtran.PRPayDate = DateTime.Parse(purchasedata[2], new CultureInfo("en-GB"));
                    PRtran.PRPayAmount = amount;
                    recid = com.addReceipt(date, suppAccID, cashAccId, amount, amount, Remark, UserId, BranchID, PurchaseReturnId, reftype);
                    PRtran.Recieptid = recid;
                    PRtran.PRCreatedDate = Today;
                    PRtran.CreatedBranch = Convert.ToInt32(BranchID);
                    PRtran.CreatedUserId = UserId;
                    PRtran.Status = 1;
                    PRtran.PurchaseReturnId = PurchaseReturnId;

                    db.PRTransactions.Add(PRtran);
                    db.SaveChanges();

                }
  
                              
                //bill sundry account
                var Gtotal = GrandTotal;
                if (bsmodel.prbsundrys != null)
                {
                    foreach (var bs in bsmodel.prbsundrys)
                    {
                        var ChkAcc = db.BillSundrys.Where(a => a.BillSundryId == bs.BillSundry).FirstOrDefault();
                        if (ChkAcc.PAccount != null && ChkAcc.PAccount != 0)
                        {
                            var bsamount = (decimal)bs.BsAmount;
                            if (ChkAcc.BSType == 0)//additive
                            {
                                purRamount = purRamount - bsamount;
                                com.addAccountTrasaction(0, (decimal)bs.BsAmount, (long)ChkAcc.PAccount, "Purchase Return", PurchaseReturnId, DC.Credit, date);
                            }
                            else //substract
                            {
                                purRamount = purRamount + bsamount;
                                com.addAccountTrasaction((decimal)bs.BsAmount, 0, (long)ChkAcc.PAccount, "Purchase Return", PurchaseReturnId, DC.Debit, date);
                            }
                        }
                    }
                }


                //add trasaction to purchase account
                com.addAccountTrasaction(0, purRamount, purAccId, "Purchase Return", PurchaseReturnId, DC.Credit, date);
                //add purchase trasaction 
                com.addAccountTrasaction(GrandTotal, 0, suppAccID, "Purchase Return", PurchaseReturnId, DC.Debit, date);
                // add vat input in account transaction
                if (TaxAmount > 0 && PRentry.PurchaseType != 3)
                    com.addAccountTrasaction(0, TaxAmount, VATInput, "Purchase Return", PurchaseReturnId, DC.Credit, date);
                if (Convert.ToDecimal(purchasedata[10]) > 0 || purchasedata[12] == "1")
                {
                    //if payment
                    com.addAccountTrasaction(0, amount, suppAccID, "Purchase Return Payment", PurchaseReturnId, DC.Credit, date);
                    com.addAccountTrasaction(amount, 0, cashAccId, "Purchase Return Payment", PurchaseReturnId, DC.Debit, date);
                }

                //----------------------
                com.addlog(LogTypes.Created, UserId, "PurchaseReturn", "PurchaseReturns", findip(), PurchaseReturnId, "Successfully Submitted Purchase Return");
                //-----------

                string action = purchasedata[13];

                var Appby = Convert.ToString(purchasedata[22]);
                if (Appby != null && Appby != "")
                {
                    long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                    Approval approval = new Approval();
                    foreach (var emp in Approve)
                    {
                        approval.TransEntry = PurchaseReturnId;
                        approval.Type = "PurchaseReturn";
                        approval.EmployeeId = emp;
                        db.Approvals.Add(approval);
                        db.SaveChanges();
                    }
                }


                Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

                if (action == "print")
                {
                    var fmapp = db.FieldMappings.Where(a => a.Section == "PReturn" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                    var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();
                    string pedate = PRentry.PRDate.ToString("dd-MM-yyyy");

                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                    var PurchaseReturnData = com.PurchaseReturnData(PurchaseReturnId, InPrintItemCode, PartNoCheck, TimeOut, ComHeadCheck);
                    var item = PurchaseReturnData.pdfItem.ToList();
                    var summary = PurchaseReturnData;
                    var billsundry = PurchaseReturnData.billsundry.ToList();

                    var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                    var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                    var def = (PriLay == Status.active) ? Convert.ToInt64(purchasedata[29]) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                    def = def == 0 ? 1 : def;
                    var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, layout, fmapp } };
                }
                else if (action == "sendmail")
                {

                    SendMail sm = new SendMail();
                    MailMessage message = new MailMessage();
                    string ToMail = purchasedata[17];
                    string CcMail = "";
                    string InvoiceNo = "_PurchaseReturn_" + PRentry.BillNo;

                    var em = db.EmailTemplates.Where(a => a.Head == "PurchaseReturn").FirstOrDefault();
                    if (em != null)
                    {
                        message.Subject = em.Subject;
                        message.Body = em.EmailBody;
                    }
                    else
                    {
                        message.Subject = "Purchase Return";
                        message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                            " <p>we are enclosing our purchase return for the items / services as requested by you during our discussions.<br/></p> " +
                            " <p>Looking forward to hear from you.</p>";
                    }
                    sm.SendPdfMail(generatePdf(PurchaseReturnId), ToMail, CcMail, InvoiceNo, message);

                    msg = "Successfully submitted Purchase Return.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

                }
                else
                {
                    msg = "Successfully Submitted Purchase Return.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

            }
            else
            {
                msg = "Voucher No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

            }

        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Purchase Return List")]
        public ActionResult GetPurchaseReturn(string BillNo, string FromDate, string ToDate, long? supplier, long? salesperson, long? type, string user, int? Balance, long? MC, string appstat, string PurchaseType)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (FromDate != "")
            {
                fdate = DateTime.Parse(FromDate, new CultureInfo("en-GB"));
            }
            if (ToDate != "")
            {
                tdate = DateTime.Parse(ToDate, new CultureInfo("en-GB"));
            }

            var SupType = SupplierType.CashSale;
            if (type == 1)
            {
                SupType = SupplierType.CashSale;
            }
            else if (type == 0)
            {
                SupType = SupplierType.CreditSale;
            }
            else
            {

            }
            string search = Request.Form.GetValues("search[value]")[0];
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var userpermission = User.IsInRole("All Purchase Return Entry");
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            ApprovalStatus AppSt = new ApprovalStatus();
            if (appstat != "")
            {
                if (appstat == "0")
                {
                    AppSt = ApprovalStatus.Approved;
                }
                else if (appstat == "1")
                {
                    AppSt = ApprovalStatus.Rejected;
                }
                else
                {
                    AppSt = ApprovalStatus.PendingApproval;
                }
            };

            PurchaseHireType St = new PurchaseHireType();
            if (PurchaseType != "")
            {
                St = (PurchaseType == "2") ? PurchaseHireType.CrossHire : PurchaseHireType.Purchase;
            };

            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var uDev = User.IsInRole("Dev");
            var uPurchaseReturnView = User.IsInRole("View Purchase Return");
            var uEdit = User.IsInRole("Edit Sales Entry");
            var uDownload = User.IsInRole("Download Purchase Return");
            var uDelete = User.IsInRole("Delete Purchase Return");

            var serverQuery = (from a in db.PurchaseReturns
                     join b in db.Suppliers on a.Supplier equals b.SupplierID into supp
                     from b in supp.DefaultIfEmpty()
                     join c in db.PRPayments on a.PurchaseReturnId equals c.PurchaseReturnId into pay
                     from c in pay.DefaultIfEmpty()
                     join d in db.Employees on a.PRCashier equals d.EmployeeId into useremp
                     from d in useremp.DefaultIfEmpty()
                     join e in db.Users on a.CreatedBy equals e.Id
                     join i in db.MCs on a.MaterialCenter equals i.MCId into mcs
                     from i in mcs.DefaultIfEmpty()
                         //let mc = db.MCs.Where(x => x.AssignedUser == a.CreatedBy).Select(x => x.MCId).FirstOrDefault()
                         // app/AppStatus/chkAppStatus (nested collections + GroupBy-latest) are computed
                         // client-side after materialization — EF Core 10 can't translate them inside this query.
                     where (BillNo == null || BillNo == "" || a.BillNo == BillNo) &&
                     (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(a.PRDate, fdate) <= 0) &&
                     (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(a.PRDate, tdate) >= 0) &&
                     (supplier == 0 || supplier == null || a.Supplier == supplier)
                     && a.Ref5 == "Debit Note"
                     && (type == null || a.SupplierType == SupType)
                     && (salesperson == 0 || salesperson == null || d.EmployeeId == salesperson)
                     && (user == null || user == "" || e.Id == user)
                     && ((Balance == null) || (Balance == 1 ? (((decimal?)a.PRGrandTotal ?? 0) > ((decimal?)c.PReturnAmount)) : (((decimal?)a.PRGrandTotal ?? 0) == ((decimal?)c.PReturnAmount))))
                     //&& (mc == 0 || mc == a.MaterialCenter)
                     && ((MCArray.Contains(a.MaterialCenter) && MC == a.MaterialCenter) || ((MC == null) && MCArray.Contains(a.MaterialCenter))) //&& (!MCList.Any() || MCArray.Contains(a.MaterialCenter))
                     && (userpermission == true || a.CreatedBy == UserId)
                     && (PurchaseType == "" || PurchaseType == null || St == a.PurType)
                     select new
                     {
                         a.PurchaseReturnId,
                         BillNo = a.BillNo,
                         a.purchaseEntryId,
                         a.PRDate,
                         a.PRGrandTotal,
                         SupplierName = b.SupplierCode + " - " + b.SupplierName,
                         EmpName = d.FirstName + " " + d.LastName,
                         User = e.UserName,
                         a.SupplierType,
                         a.PayType,
                         PaymentStatus = c != null ? c.Status : 0,
                         PaymentTrans = db.PRTransactions.Any(k => k.PurchaseReturnId == a.PurchaseReturnId),
                         PReturnAmount = c != null ? c.PReturnAmount : 0m,
                         a.Remarks,
                         BalanceAmt = c != null ? a.PRGrandTotal - c.PReturnAmount : 0m,
                         Dev = uDev,
                         Details = uPurchaseReturnView,
                         Edit = uEdit,
                         Download = uDownload,
                         Delete = uDelete,
                         MC = i.MCName,
                         CreatedDate = a.PRCreatedDate,
                         a.PurType
                     });

            // Performance (audit P2, hybrid): server paging when no search, plain-column sort, AND no client-side
            // filter is active (the conditions below mirror the filters' own guards); else original path.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase){ "BalanceAmt","BillNo","CreatedDate","Delete","Details","Dev","Download","Edit","EmpName","MC","PaymentStatus","PaymentTrans","PayType","PRDate","PReturnAmount","PRGrandTotal","purchaseEntryId","PurchaseReturnId","PurType","Remarks","SupplierName","SupplierType","User" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0 && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn)) && !(appstat != "");
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn) ? serverQuery.OrderBy("PurchaseReturnId asc") : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

            // CLIENT-side lookups keyed by PurchaseReturnId (missing key -> empty/absent, no KeyNotFound).
            var prIds = serverRows.Select(o => o.PurchaseReturnId).ToList();
            // app = approver EmployeeIds for the purchase return (nested collection, keyed by TransEntry == PurchaseReturnId).
            var appLookup = db.Approvals
                .Where(x => x.Type == "PurchaseReturn" && prIds.Contains(x.TransEntry))
                .Select(x => new { x.TransEntry, x.EmployeeId })
                .ToList()
                .ToLookup(x => x.TransEntry);
            // AppStatus = all ApprovalUpdate statuses; raw rows materialized once and reused for chkAppStatus.
            var appUpdRows = db.ApprovalUpdates
                .Where(x => x.Type == "PurchaseReturn" && prIds.Contains(x.TransEntry))
                .Select(x => new { x.TransEntry, x.ApprovalStatus, x.ApprovedBy, x.CreatedDate })
                .ToList();
            var appStatusLookup = appUpdRows.ToLookup(x => x.TransEntry);
            // chkAppStatus = latest ApprovalUpdate status per ApprovedBy (GroupBy-latest-per-group), per purchase return.
            var chkAppStatusLookup = appUpdRows
                .GroupBy(x => x.TransEntry)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(l => l.ApprovedBy)
                    .Select(grp => grp.OrderByDescending(b => b.CreatedDate).First())
                    .Select(x => x.ApprovalStatus).ToList());

            var v = serverRows.Select(o =>
                     {
                         var app = appLookup[o.PurchaseReturnId].Select(x => x.EmployeeId).ToList();
                         var AppStatus = appStatusLookup[o.PurchaseReturnId].Select(x => x.ApprovalStatus).ToList();
                         var chkAppStatus = chkAppStatusLookup.TryGetValue(o.PurchaseReturnId, out var ck) ? ck : new List<ApprovalStatus>();
                         return new
                     {
                         o.PurchaseReturnId,
                         o.BillNo,
                         o.purchaseEntryId,
                         o.PRDate,
                         o.PRGrandTotal,
                         o.SupplierName,
                         o.EmpName,
                         o.User,
                         o.SupplierType,
                         o.PayType,
                         o.PaymentStatus,
                         o.PaymentTrans,
                         o.PReturnAmount,
                         o.Remarks,
                         o.BalanceAmt,
                         o.Dev,
                         o.Details,
                         o.Edit,
                         o.Download,
                         o.Delete,
                         o.MC,
                         app = app,
                         Approval = (app != null && empl.EmployeeId != null) ? (app.Contains(empl.EmployeeId) ? true : false) : false,
                         ApprovalStatus = (app.Count > 0 && AppStatus.Count > 0 && chkAppStatus.Count > 0) ? (chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (app != null && chkAppStatus != null && app.Count != 0 && chkAppStatus.Count != 0 && chkAppStatus.Count() == app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                         o.CreatedDate,
                         PurType = o.PurType,
                         };
                     });
            if (appstat != "")
            {
                v = v.Where(a => a.ApprovalStatus == AppSt);
            }
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.BillNo.ToString().ToLower().Equals(search.ToLower())
                                 //// p.CreditPeriod.ToString().ToLower().Contains(search.ToLower()) ||
                                 //p.PReturnAmount.ToString().ToLower().Contains(search.ToLower())
                                 ////p.SEBalanceAmount.ToString().ToLower().Contains(search.ToLower())
                                 );
            }

            //SORT
            if (!fastPage && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            else
            {
                v = v.OrderByDescending(b => Convert.ToInt64(b.PurchaseReturnId));
            }

            if (!fastPage) { recordsTotal = v.Count(); }
            var data = (fastPage ? v.ToList() : v.Skip(skip).Take(pageSize).ToList());
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,View Purchase Return")]
        public ActionResult Details(long? id)
        {

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            PurchaseReturnViewModel vmodel = new PurchaseReturnViewModel();

            vmodel = (from b in db.PurchaseReturns
                      join c in db.PRPayments on b.PurchaseReturnId equals c.PurchaseReturnId into pay
                      from c in pay.DefaultIfEmpty()
                      join d in db.Employees on b.PRCashier equals d.EmployeeId into emp
                      from d in emp.DefaultIfEmpty()
                      join f in db.Suppliers on b.Supplier equals f.SupplierID into supp
                      from f in supp.DefaultIfEmpty()
                      join h in db.MCs on b.MaterialCenter equals h.MCId into mcs
                      from h in mcs.DefaultIfEmpty()
                      join t in db.PurchaseTypes on b.PurchaseType equals t.Id into ptype
                      from t in ptype.DefaultIfEmpty()
                      join x in db.Contacts on f.Contact equals x.ContactID into cnt
                      from x in cnt.DefaultIfEmpty()
                      where b.PurchaseReturnId == id
                      select new PurchaseReturnViewModel
                      {
                          SupplierName = f.SupplierCode + " - " + f.SupplierName,
                          BillNo = b.BillNo,
                          purchaseEntryId = b.purchaseEntryId,
                          PRDate = b.PRDate,
                          PRNote = b.PRNote.Replace("\n", "<br />"),
                          EmployeeName = d.FirstName + " " + d.LastName,
                          SupplierType = b.SupplierType,
                          PRDiscount = b.PRDiscount,
                          PRTotal = b.PRDiscount + b.PRGrandTotal,
                          PRGrandTotal = b.PRGrandTotal,
                          PReturnAmount = c.PReturnAmount,//b.SupplierType == 0 ? c.PReturnAmount : b.PRGrandTotal,
                          PRDueAmount = b.PRGrandTotal - c.PReturnAmount, //b.SupplierType == 0 ? b.PRGrandTotal - c.PReturnAmount : 0,
                          PayType = (b.SupplierType == SupplierType.CashSale ? "Cash" : "Credit"),
                          Remarks = b.Remarks.Replace("\n", "<br />"),
                          MCName = h.MCName,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          PurTypeName = (b.PurType == PurchaseHireType.Purchase) ? "Purchase" : "CrossHire",
                          PursTypeName = t.Name,
                          EmailId = x.EmailId,
                          ReturnTypeName = (b.ReturnType == ReturnType.AgainstBill) ? "AgainstBill" : "Direct",
                          Emp = (from ab in db.Approvals
                                 join bb in db.Employees on ab.EmployeeId equals bb.EmployeeId
                                 where ab.TransEntry == id && ab.Type == "PurchaseReturn"
                                 select new ApprovalViewModel
                                 {
                                     EmpName = bb.FirstName + " " + bb.LastName
                                 }).ToList()
                      }).FirstOrDefault();

            vmodel.PRItemNote = db.PRItemNotes.Where(a => a.PurchaseReturnId == id && a.itemNote != "-:{Bundle_Item}")
            .Select(b => new PRItemViewModel
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
                ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault(),
                PartNumber = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.PartNumber).FirstOrDefault(),
                bundleitem = (from ab in db.PRItemNotes
                              join bb in db.Items on ab.Item equals bb.ItemID
                              join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                              from cb in primary.DefaultIfEmpty()
                              join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                              from bd in second.DefaultIfEmpty()
                              where ab.PurchaseReturnId == id && ab.itemNote == "-:{Bundle_Item}"
                              && b.Item == ab.ItemDiscount
                              select new ItemDetailViewModel
                              {
                                  ItemCode = bb.ItemCode,
                                  ItemName = bb.ItemName,
                                  ItemUnit = cb.ItemUnitName,
                                  ItemQuantity = ab.ItemQuantity,
                              }).ToList()
            }).ToList();

            vmodel.PRbs = db.PRBillSundrys.Where(a => a.PurchaseReturnId == id)
           .Select(b => new PRBillSundryViewModel
           {
               AmountType = b.AmountType,
               BsAmount = b.BsAmount,
               BsType = b.BsType,
               BsValue = b.BsValue,
               Type = b.BsType == 0 ? "Add" : "Less",
               AmtType = b.AmountType == 0 ? "" : "%",
               BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
           }).ToList();

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "PReturn" && a.Status == Status.active).ToList();

            return PartialView(vmodel);
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Edit Purchase Return")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userpermission = User.IsInRole("All Purchase Return Entry");
            var UserId = User.Identity.GetUserId();
            PurchaseReturn Prtn = db.PurchaseReturns.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.PurchaseReturnId == id).FirstOrDefault();

            if (Prtn == null)
            {
                return NotFound();
            }
            Int64 cashier = Convert.ToInt64(Prtn.PRCashier);
            Int64 supplier = Prtn.Supplier;

            PurchaseReturnViewModel vmodel = new PurchaseReturnViewModel();


            var supp = db.Suppliers
                .Select(s => new
                {
                    SupplierID = s.SupplierID,
                    SupplierDetails = s.SupplierCode + " - " + s.SupplierName
                }).ToList();
            ViewBag.Supp = QkSelect.List(supp, "SupplierID", "SupplierDetails");

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            if (mcchk != null)
            {
                var mcs = db.MCs.Where(s => s.AssignedUser == UserId).Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
            }
            else
            {
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
            }

            var use = db.Employees
                             .Select(s => new
                             {
                                 ID = s.EmployeeId,
                                 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                             })
                             .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            var pentry = db.PurchaseEntrys
                           .Select(s => new
                           {
                               ID = s.PurchaseEntryId,
                               Name = s.BillNo
                           })
                           .ToList();
            ViewBag.purchaseEntry = QkSelect.List(pentry, "ID", "Name");

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "PurchaseReturn").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaPReturn = db.EnableSettings.Where(a => a.EnableType == "MLAPReturn").FirstOrDefault();
            var MlaPReturns = MlaPReturn != null ? MlaPReturn.Status : Status.inactive;
            ViewBag.MLAPReturn = MlaPReturns;

            vmodel = (from b in db.PurchaseReturns
                      join c in db.PRPayments on b.PurchaseReturnId equals c.PurchaseReturnId into prp
                      from c in prp.DefaultIfEmpty()
                      join d in db.Suppliers on b.Supplier equals d.SupplierID into sup
                      from d in sup.DefaultIfEmpty()
                      join e in db.Contacts on d.Contact equals e.ContactID into cont
                      from e in cont.DefaultIfEmpty()
                      where b.PurchaseReturnId == id
                      select new PurchaseReturnViewModel
                      {
                          BillNo = b.BillNo,
                          PRDate = b.PRDate,
                          purchaseEntryId = b.purchaseEntryId,
                          PRCashier = b.PRCashier,
                          Supplier = b.Supplier,
                          PRDiscount = b.PRDiscount,
                          PRGrandTotal = b.PRGrandTotal,
                          //  PReturnAmount = c.PReturnAmount,
                          SupplierType = b.SupplierType,
                          ReturnType = b.ReturnType,
                          // PRDueAmount = b.PRGrandTotal - c.PReturnAmount,
                          SupplierName = db.Suppliers.Where(a => a.SupplierID == b.Supplier).Select(a => a.SupplierCode + " - " + a.SupplierName).FirstOrDefault(),
                          PRNote = b.PRNote,
                          suppEmailId = e.EmailId,
                          Remarks = b.Remarks,
                          MaterialCenter = b.MaterialCenter,
                          Branch = b.Branch,
                          PurchaseType = b.PurchaseType,
                          PurchaseTypes = db.PurchaseTypes.ToList(),
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          PurType = b.PurType,
                      }).FirstOrDefault();


            companySet();

            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            ViewBag.preEntry = db.PurchaseReturns.Where(a => a.PurchaseReturnId < id && (!MCList.Any() || MCArray.Contains(a.MaterialCenter)) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.PurchaseReturnId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.PurchaseReturns.Where(a => a.PurchaseReturnId > id && (!MCList.Any() || MCArray.Contains(a.MaterialCenter)) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.PurchaseReturnId).DefaultIfEmpty().Min();

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;
            _FinancialYear();

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var pos = db.EnableSettings.Where(a => a.EnableType == "POSInvoice").FirstOrDefault();
            var poscheck = pos != null ? pos.Status : Status.inactive;
            ViewBag.POS = poscheck;

            var EditPermission = User.IsInRole("Disable PReturn Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "PurchaseReturn", UserId);

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "PReturn" && a.Status == Status.active).ToList();

            //dummy table operations
            //        //add to se-item table


            ViewBag.DefMc = db.MCs.Where(a => a.MCName == "Hire").Select(a => a.MCId).FirstOrDefault();

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;
            var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
            var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
            ViewBag.PrintLayout = PriLay;
            var invoice = db.InvoiceLayouts
                             .Select(s => new
                             {
                                 ID = s.Id,
                                 Name = s.Name,
                             })
                             .ToList();
            ViewBag.printlay = QkSelect.List(invoice, "ID", "Name");
            var rtype = Request.Query["rtype"];
            if (rtype == "APP")
            {
                return View("App/Edit", vmodel);
            }
            else
            {
                return View(vmodel);
            }
        }
        [HttpGet]
        public ActionResult GetPurchaseTypes(int PrId)
        {
            var types = db.PurchaseReturns.Where(a => a.PurchaseReturnId == PrId)
                .Select(b => new
                {
                    b.ReturnType,
                    b.SupplierType
                }).FirstOrDefault();
            return Json(types);

        }
        [HttpGet]
        public ActionResult GetPRItems(long PurchaseReturnID)
        {
            var ConD = (from a in db.PRItemNotes
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.PurchaseReturnId == PurchaseReturnID && a.itemNote != "-:{Bundle_Item}"
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
                            note = a.itemNote.Replace("<br />", "\n"),
                            a.ItemTotalAmount,
                            ItemCode = b.ItemCode,
                            ItemName = b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            b.BasePrice,
                            b.SellingPrice,
                            b.PurchasePrice,
                            b.MRP,
                            b.ConFactor,
                            b.KeepStock,
                            b.slreq,
                            batch = (from ay in db.BatchStocks
                                     join az in db.PRItemNotes on new { f1 = ay.Item, f2 = ay.Unit, f3 = ay.Reference, f4 = ay.Type }
                                          equals new { f1 = az.Item, f2 = az.ItemUnit, f3 = az.PurchaseReturnId, f4 = "PReturn" }
                                     where az.PurchaseReturnId == PurchaseReturnID && ay.Item == a.Item
                                     select new BatchStockPViewModel
                                     {
                                         BatchNo = ay.BatchNo,
                                         MFGd = ay.MFG,
                                         EXPd = ay.EXP,
                                         StockIn = ay.StockIn,
                                         StockOut = ay.StockOut,
                                         Item = ay.Item,
                                         cfactor = b.ConFactor,
                                         Priunit = b.ItemUnitID,
                                         Secunit = b.SubUnitId,
                                         Unit = ay.Unit,
                                         Cost = ay.Cost,
                                         origin = "PReturn",
                                         Order = ay.Order
                                     }).ToList()
                        });
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(ConD);
            return Json(result);
        }
        [HttpGet]
        public ActionResult GetPRBillSundry(long PurchaseReturnID)
        {
            var PRBs = (from a in db.PRBillSundrys
                        join c in db.BillSundrys on a.BillSundry equals c.BillSundryId
                        where a.PurchaseReturnId == PurchaseReturnID
                        select new
                        {
                            a.AmountType,
                            a.BillSundry,
                            a.BsAmount,
                            a.BsType,
                            a.BsValue,
                            //a.PEBillSundryId,
                            //a.PurchaseEntry,
                            c.BSName
                            //c.BillSundryId
                        }).ToList();

            return Json(PRBs);
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Purchase Return")]
        public JsonResult UpdatePurchaseReturn(string[][] array, string[] purchasedata, PRBillSundryViewModel bsmodel, ICollection<BatchStockPViewModel> bstmodel)
        {
            bool stat = false;
            string msg;
            Int64 purchaseReturnId = Convert.ToInt64(purchasedata[15]);
            PurchaseReturn PRentry = db.PurchaseReturns.Find(purchaseReturnId);
            if (BillExist(Convert.ToString(purchasedata[17])) && Convert.ToString(purchasedata[17]) != PRentry.BillNo)
            {
                msg = "Invoice No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

            var UserId = User.Identity.GetUserId();
            var CurrentDate = Convert.ToDateTime(System.DateTime.Now);

            long Branch = 0;
            Int64 purRAcc = (long)db.companys.Select(a => a.PReturnAccount).FirstOrDefault();

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

            if (BranchCheck == Status.active)
            {
                Branch = Convert.ToInt64(purchasedata[21]);
            }
            else
            {
                Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
            }

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;

            long MC = 0;
            if (MCcheck == Status.active)
            {
                MC = Convert.ToInt64(purchasedata[20]);
            }
            else
            {
                MC = 1;
            }



            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            var TaxAmount = Convert.ToDecimal(purchasedata[5]);
            var GrandTotal = Convert.ToDecimal(purchasedata[7]);
            var purchaseramount = GrandTotal - TaxAmount;
            var subtotal = Convert.ToDecimal(purchasedata[8]);

            var EditPermission = User.IsInRole("Disable PReturn Edit After Approval");
            if (com.chkApproved(purchaseReturnId, EditPermission, "PurchaseReturn", UserId) == true)
            {
                PRentry.PurType = (purchasedata[29] == "1") ? PurchaseHireType.CrossHire : PurchaseHireType.Purchase;
                PRentry.BillNo = purchasedata[17];
                PRentry.PRDate = DateTime.Parse(purchasedata[2], new CultureInfo("en-GB"));
                PRentry.PRCashier = purchasedata[1] != "" ? Convert.ToInt64(purchasedata[1]) : 0;
                PRentry.Supplier = Convert.ToInt64(purchasedata[0]);
                PRentry.PayType = "";//need change
                PRentry.PRItems = Convert.ToInt32(purchasedata[3]);
                PRentry.PRItemQuantity = Convert.ToDecimal(purchasedata[4]);
                PRentry.PRSubTotal = Convert.ToDecimal(purchasedata[8]);
                PRentry.PRTax = Convert.ToDecimal(purchasedata[9]);
                PRentry.PRTaxAmount = TaxAmount;
                PRentry.PRDiscount = Convert.ToDecimal(purchasedata[6]);
                PRentry.PRGrandTotal = GrandTotal;
                PRentry.PRNote = purchasedata[11];
                PRentry.Print = 1;
                PRentry.Status = 1;
                PRentry.Branch = Branch;
                PRentry.Remarks = purchasedata[19];
                PRentry.MaterialCenter = MC;
                PRentry.PurchaseType = Convert.ToInt64(purchasedata[22]);


                var SuppType = PRentry.SupplierType;
                if (purchasedata[12] == "1")
                {
                    PRentry.SupplierType = SupplierType.CashSale;
                }
                else
                {
                    PRentry.SupplierType = SupplierType.CreditSale;
                }
                if (purchasedata[16] == "1")
                {
                    PRentry.ReturnType = ReturnType.Direct;
                    PRentry.purchaseEntryId = 0;
                }
                else
                {
                    PRentry.ReturnType = ReturnType.AgainstBill;
                    PRentry.purchaseEntryId = Convert.ToInt64(purchasedata[14]);
                }

                PRentry.Ref1 = Convert.ToString(purchasedata[24]);
                PRentry.Ref2 = Convert.ToString(purchasedata[25]);
                PRentry.Ref3 = Convert.ToString(purchasedata[26]);
                PRentry.Ref4 = Convert.ToString(purchasedata[27]);
                PRentry.Ref5 = "Debit Note";
                PRentry.PReturnAccount = purRAcc;
                db.Entry(PRentry).State = EntityState.Modified;

                //To Update the quantity in Edit Mode(ItemTransaction Table)               
                com.ItemTransInEditMode("PurchaseReturn", MC, 0, 0, array, purchaseReturnId, UserId, CurrentDate);

                var PRItem = db.PRItemNotes.Where(a => a.PurchaseReturnId == purchaseReturnId).FirstOrDefault();
                if (PRItem != null)
                {
                    var PItems = db.PRItemNotes.Where(a => a.PurchaseReturnId == purchaseReturnId).ToList();
                    foreach (var arr in PItems)
                    {
                        //add to dummy table
                        ////DummyPRItem dItem = new DummyPRItem();
                        ////dItem.ItemUnit = arr.ItemUnit;
                        ////dItem.ItemUnitPrice = arr.ItemUnitPrice;
                        ////dItem.ItemQuantity = arr.ItemQuantity;
                        ////dItem.ItemSubTotal = arr.ItemSubTotal;
                        ////dItem.ItemDiscount = arr.ItemDiscount;
                        ////dItem.ItemTax = arr.ItemTax;
                        ////dItem.ItemTaxAmount = arr.ItemTaxAmount;
                        ////dItem.ItemTotalAmount = arr.ItemTotalAmount;
                        ////dItem.itemNote = arr.itemNote;
                        ////dItem.PurchaseReturnId = arr.PurchaseReturnId;
                        ////dItem.Item = arr.Item;
                        ////db.DummyPRItems.Add(dItem);
                        ////db.SaveChanges();
                    }

                    db.PRItemNotes.RemoveRange(db.PRItemNotes.Where(a => a.PurchaseReturnId == purchaseReturnId));
                    db.SaveChanges();
                }

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
                dtItem.Columns.Add("PurchaseReturnId");
                dtItem.Columns.Add("Item");


                foreach (var arr in array)
                {
                    DataRow dr = dtItem.NewRow();

                    dr["ItemUnit"] = arr[1];
                    dr["ItemUnitPrice"] = Convert.ToDecimal(arr[3]);
                    dr["ItemQuantity"] = Convert.ToDecimal(arr[2]);
                    dr["ItemSubTotal"] = Convert.ToDecimal(arr[5]);
                    dr["ItemDiscount"] = Convert.ToDecimal(arr[6]);
                    dr["ItemTax"] = Convert.ToDecimal(arr[10]);
                    dr["ItemTaxAmount"] = Convert.ToDecimal(arr[9]);
                    dr["ItemTotalAmount"] = Convert.ToDecimal(arr[11]);
                    dr["itemNote"] = Convert.ToString(arr[29].Replace("\n", "<br />"));
                    dr["PurchaseReturnId"] = purchaseReturnId;
                    dr["Item"] = Convert.ToInt32(arr[0]);

                    dtItem.Rows.Add(dr);

                    var item = Convert.ToInt32(arr[0]);
                    var chkbundle = db.ItemBundles.Where(a => a.mainItem == item).Select(a => a.ItemBundleId).FirstOrDefault();
                    if (chkbundle > 0)
                    {
                        var bunQuan = Convert.ToDecimal(arr[2]);
                        var itemBundle = (from g in db.ItemBundles
                                          join b in db.Items on g.mainItem equals b.ItemID
                                          where b.ItemID == item
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

                            decimal itemtax = 0;
                            decimal taxamt = 0;
                            decimal totamt = 0;

                            itemtax = bu.ItemTax;
                            taxamt = buTaxAmount;
                            totamt = (buTaxAmount + ItemSubTotal);

                            DataRow dbu = dtItem.NewRow();
                            dbu["ItemUnit"] = bu.ItemUnit;
                            dbu["ItemUnitPrice"] = bu.ItemUnitPrice;
                            dbu["ItemQuantity"] = (bunQuan * bu.quantity);
                            dbu["ItemSubTotal"] = ItemSubTotal;
                            // add parent itemid in discount for reference
                            dbu["ItemDiscount"] = item;
                            dbu["ItemTax"] = itemtax;
                            dbu["ItemTaxAmount"] = taxamt;
                            dbu["ItemTotalAmount"] = totamt;
                            dbu["itemNote"] = "-:{Bundle_Item}";
                            dbu["PurchaseReturnId"] = purchaseReturnId;
                            dbu["Item"] = bu.Item;
                            dtItem.Rows.Add(dbu);
                        }
                    }

                }

                SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = "TableTypePRItems";
                //// execute sp sql 
                string sql = String.Format("EXEC {0} {1};", "SP_InsertPRItemNotes", "@TableType");
                //// execute sql 
                var ret = db.Database.ExecuteSqlRaw(sql, parameter);
                if (ret > 0)
                {
                }

                // batch stock





                // bill sundry
                var PRBs = db.PRBillSundrys.Where(a => a.PurchaseReturnId == purchaseReturnId).FirstOrDefault();
                if (PRBs != null)
                {
                    db.PRBillSundrys.RemoveRange(db.PRBillSundrys.Where(a => a.PurchaseReturnId == purchaseReturnId));
                    db.SaveChanges();
                }
                decimal tem = 0;
                if (bsmodel.prbsundrys != null)
                {
                    foreach (var bs in bsmodel.prbsundrys)
                    {
                        if (bs.BsAmount == null)
                        {
                            bs.BsAmount = tem;
                        }
                    }
                }
                if (bsmodel.prbsundrys != null)
                {
                    string bsResult = string.Empty;

                    DataTable BsEntry = new DataTable();
                    BsEntry.Columns.Add("PurchaseReturnId");
                    BsEntry.Columns.Add("BillSundry");
                    BsEntry.Columns.Add("BsValue");
                    BsEntry.Columns.Add("AmountType");
                    BsEntry.Columns.Add("BsType");
                    BsEntry.Columns.Add("BsAmount");

                    foreach (var bs in bsmodel.prbsundrys)
                    {
                        DataRow drw = BsEntry.NewRow();
                        drw["PurchaseReturnId"] = purchaseReturnId;
                        drw["BillSundry"] = bs.BillSundry;
                        drw["BsValue"] = bs.BsValue;
                        drw["AmountType"] = bs.AmountType;
                        drw["BsType"] = bs.BsType;
                        drw["BsAmount"] = bs.BsAmount;

                        BsEntry.Rows.Add(drw);
                    }

                    ////// create parameter 
                    SqlParameter parameter1 = new SqlParameter("@TableType", BsEntry);
                    parameter1.SqlDbType = SqlDbType.Structured;
                    parameter1.TypeName = "TableTypePRBillSundry";
                    //// execute sp sql 
                    string sql1 = String.Format("EXEC {0} {1};", "SP_InsertPRBillSundry", "@TableType");
                    //// execute sql 
                    db.Database.ExecuteSqlRaw(sql1, parameter1);
                    //-------------------------------------
                }

                decimal amount = Convert.ToDecimal(purchasedata[10]);
                var date = DateTime.Parse(purchasedata[2], new CultureInfo("en-GB"));
                Int64 suppAccID = db.Suppliers.Where(a => a.SupplierID == PRentry.Supplier).Select(a => a.Accounts).FirstOrDefault();
                Int64 purAccId = purRAcc;// db.Accountss.Where(a => a.Group == 16).Select(a => a.AccountsID).FirstOrDefault();
                Int64 cashAccId = db.Accountss.Where(a => a.Group == 9).Select(a => a.AccountsID).FirstOrDefault();
                Int64 VATInput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Input").Select(a => a.AccountsID).SingleOrDefault();


                //----------new added----------------
                if (purchasedata[12] == "1")//cash
                {
                    deleteAndUpdateTrans(purchaseReturnId, purchasedata, amount, suppAccID, cashAccId, BranchID, UserId);
                    changeRecBill(purchaseReturnId);
                    amount = GrandTotal;
                }
                if (purchasedata[12] == "0")//credit
                {
                    if (SuppType == SupplierType.CashSale)//previous cash
                    {
                        deleteAndUpdateTrans(purchaseReturnId, purchasedata, amount, suppAccID, cashAccId, BranchID, UserId);
                    }
                    if (SuppType == SupplierType.CreditSale)//previous credit
                    {

                        decimal sumTran = db.PRTransactions.Where(a => a.PurchaseReturnId == purchaseReturnId).FirstOrDefault() != null ? (decimal?)db.PRTransactions.Where(a => a.PurchaseReturnId == purchaseReturnId).Select(a => a.PRPayAmount).Sum() ?? 0 : 0;
                        if (sumTran > GrandTotal)
                        {
                            var chkrec = db.ReceiptBills.Where(a => a.InvoiceNo == purchaseReturnId && a.BillType == "Purchase Return" && a.Type == "Against Reference").ToList();
                            if (chkrec != null)
                            {
                                var recamount = sumTran - GrandTotal;
                                amount = GrandTotal;
                                decimal TotAmt = GrandTotal;
                                foreach (var rbill in chkrec)
                                {
                                    TotAmt = TotAmt - rbill.Amount;
                                    if (TotAmt < 0 && rbill.Amount > recamount)
                                    {
                                        var reamt = rbill.Amount - recamount;

                                        ReceiptBill recbillz = db.ReceiptBills.Find(rbill.ReceiptBillId);
                                        recbillz.Amount = reamt;
                                        db.Entry(recbillz).State = EntityState.Modified;
                                        db.SaveChanges();

                                        ReceiptBill recbill = new ReceiptBill();
                                        recbill.InvoiceNo = null;
                                        recbill.NewRefName = "";
                                        recbill.Receipt = Convert.ToInt64(rbill.Receipt);
                                        recbill.BillType = null;
                                        recbill.Amount = recamount;
                                        recbill.Type = "New Reference";
                                        recbill.Status = Status.active;

                                        db.ReceiptBills.Add(recbill);
                                        db.SaveChanges();

                                    }
                                }
                                updateSepayment(purchaseReturnId, purchasedata, amount, BranchID, 0);
                            }
                            else
                            {
                                deleteAndUpdateTrans(purchaseReturnId, purchasedata, amount, suppAccID, cashAccId, BranchID, UserId);
                            }
                        }
                        else
                        {
                            updateSepayment(purchaseReturnId, purchasedata, amount, BranchID, 1);
                        }

                    }
                }


                bool delete = com.DeleteAllAccountTransaction("Purchase Return", purchaseReturnId);
                bool deletepay = com.DeleteAllAccountTransaction("Purchase Return Payment", purchaseReturnId);


                //bill sundry account
                var Gtotal = GrandTotal;
                if (bsmodel.prbsundrys != null)
                {
                    foreach (var bs in bsmodel.prbsundrys)
                    {
                        var ChkAcc = db.BillSundrys.Where(a => a.BillSundryId == bs.BillSundry).FirstOrDefault();
                        if (ChkAcc.PAccount != null && ChkAcc.PAccount != 0)
                        {
                            var bsamount = (decimal)bs.BsAmount;
                            if (ChkAcc.BSType == 0)//additive
                            {
                                purchaseramount = purchaseramount - bsamount;
                                com.addAccountTrasaction(0, (decimal)bs.BsAmount, (long)ChkAcc.PAccount, "Purchase Return", purchaseReturnId, DC.Credit, date);
                            }
                            else //substract
                            {
                                purchaseramount = purchaseramount + bsamount;
                                com.addAccountTrasaction((decimal)bs.BsAmount, 0, (long)ChkAcc.PAccount, "Purchase Return", purchaseReturnId, DC.Debit, date);
                            }
                        }
                    }
                }


                //add trasaction to purchase account
                com.addAccountTrasaction(0, purchaseramount, purAccId, "Purchase Return", purchaseReturnId, DC.Credit, date);
                //add purchase trasaction 
                com.addAccountTrasaction(GrandTotal, 0, suppAccID, "Purchase Return", purchaseReturnId, DC.Debit, date);
                // add vat input in account transaction
                if (TaxAmount > 0 && PRentry.PurchaseType != 3)
                    com.addAccountTrasaction(0, TaxAmount, VATInput, "Purchase Return", purchaseReturnId, DC.Credit, date);
                if (Convert.ToDecimal(purchasedata[10]) > 0 || purchasedata[12] == "1")
                {
                    //if payment
                    com.addAccountTrasaction(0, amount, suppAccID, "Purchase Return Payment", purchaseReturnId, DC.Credit, date);
                    com.addAccountTrasaction(amount, 0, cashAccId, "Purchase Return Payment", purchaseReturnId, DC.Debit, date);
                }

                com.addlog(LogTypes.Updated, UserId, "PurchaseReturn", "PurchaseReturns", findip(), purchaseReturnId, "Successfully Updated Purchase Return");



                //Approved By
                var empuser = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                var chkapp = db.Approvals.Where(a => a.EmployeeId == empuser && a.TransEntry == purchaseReturnId && a.Type == "PurchaseReturn").FirstOrDefault();
                var MrnPO = db.Approvals.Where(a => a.TransEntry == purchaseReturnId && a.Type == "PurchaseReturn").FirstOrDefault();
                if (MrnPO != null)
                {
                    if (chkapp != null)
                    {
                        db.Approvals.RemoveRange(db.Approvals.Where(a => a.EmployeeId != empuser && a.TransEntry == purchaseReturnId && a.Type == "PurchaseReturn"));
                        db.SaveChanges();
                    }
                    else
                    {
                        db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == purchaseReturnId && a.Type == "PurchaseReturn"));
                        db.SaveChanges();
                    }
                }
                var Appby = Convert.ToString(purchasedata[23]);
                if (Appby != null && Appby != "")
                {
                    long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                    Approval approval = new Approval();
                    foreach (var emp in Approve)
                    {
                        approval.TransEntry = purchaseReturnId;
                        approval.Type = "PurchaseReturn";
                        approval.EmployeeId = emp;
                        db.Approvals.Add(approval);
                        db.SaveChanges();
                    }
                }
            }
            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            string action = purchasedata[13];

            if (action == "print")
            {
                var fmapp = db.FieldMappings.Where(a => a.Section == "PReturn" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();


                var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();
                string pedate = PRentry.PRDate.ToString("dd-MM-yyyy");

                var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                var PurchaseReturnData = com.PurchaseReturnData(purchaseReturnId, InPrintItemCode, PartNoCheck, TimeOut, ComHeadCheck);
                var item = PurchaseReturnData.pdfItem.ToList();
                var summary = PurchaseReturnData;
                var billsundry = PurchaseReturnData.billsundry.ToList();

                var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                var def = (PriLay == Status.active) ? Convert.ToInt64(purchasedata[30]) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                def = def == 0 ? 1 : def;
                var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, layout, fmapp } };
            }
            else if (action == "sendmail")
            {

                SendMail sm = new SendMail();
                MailMessage message = new MailMessage();
                string ToMail = purchasedata[17];
                string CcMail = "";
                string InvoiceNo = "_PurchaseReturn_" + PRentry.BillNo;

                var em = db.EmailTemplates.Where(a => a.Head == "PurchaseReturn").FirstOrDefault();
                if (em != null)
                {
                    message.Subject = em.Subject;
                    message.Body = em.EmailBody;
                }
                else
                {
                    message.Subject = "Purchase Return";
                    message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                        " <p>we are enclosing our purchase return for the items / services as requested by you during our discussions.<br/></p> " +
                        " <p>Looking forward to hear from you.</p>";
                }
                sm.SendPdfMail(generatePdf(purchaseReturnId), ToMail, CcMail, InvoiceNo, message);

                msg = "Successfully Updated Purchase Return.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

            }
            else
            {
                msg = "Successfully Updated Purchase Return.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }
        public void updateSepayment(long pEntryId, string[]prdata, decimal amount, long BranchID, int chk)
        {
            var GrandTotal = Convert.ToDecimal(prdata[7]);
            PRPayment PRpay = db.PRPayments.Where(a => a.PurchaseReturnId == pEntryId).FirstOrDefault();
            PRpay.PRDate = DateTime.Parse(prdata[2], new CultureInfo("en-GB"));
            PRpay.PRBillAmount = GrandTotal;

            if (chk == 0)
            {
                 //walkin customer
                if (prdata[12] == "1")
                {
                    PRpay.PReturnAmount = GrandTotal;
                }
                else
                {
                    PRpay.PReturnAmount = amount != 0 ? amount : Convert.ToDecimal(prdata[10]);
                }
            }

            PRpay.SupplierId = Convert.ToInt64(prdata[0]);
            PRpay.CreatedBranch = Convert.ToInt32(BranchID);
            PRpay.Status = 1;
            db.Entry(PRpay).State = EntityState.Modified;
            db.SaveChanges();
        }
        public void deleteAndUpdateTrans(long purchaseReturnId, string[] purchasedata, decimal amount, long suppAccID, long cashAccId, long BranchID, string UserId)
        {

            var GrandTotal = Convert.ToDecimal(purchasedata[7]);
            var date = DateTime.Parse(purchasedata[2], new CultureInfo("en-GB"));
            PRPayment PRpay = db.PRPayments.Where(a => a.PurchaseReturnId == purchaseReturnId).FirstOrDefault();

            PRpay.SupplierId = Convert.ToInt64(purchasedata[0]);
            PRpay.PRDate = DateTime.Parse(purchasedata[2], new CultureInfo("en-GB"));
            PRpay.PREntryDate = Convert.ToDateTime(System.DateTime.Now);
            PRpay.PRBillAmount = GrandTotal;

            if (purchasedata[12] == "1")
            {
                PRpay.PReturnAmount = GrandTotal;
            }
            else
            {
                PRpay.PReturnAmount = amount != 0 ? amount : Convert.ToDecimal(purchasedata[10]);
            }

            PRpay.CreatedBranch = Convert.ToInt32(BranchID);
            PRpay.CreatedUserId = UserId;
            PRpay.PRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
            PRpay.Status = 1;
            PRpay.PurchaseReturnId = purchaseReturnId;

            db.Entry(PRpay).State = EntityState.Modified;
            db.SaveChanges();


            db.PRTransactions.RemoveRange(db.PRTransactions.Where(a => a.PurchaseReturnId == purchaseReturnId));
            db.Receipts.RemoveRange(db.Receipts.Where(a => a.Reference == purchaseReturnId && a.RefType == "Purchase Return"));
            db.SaveChanges();

            if (amount > 0 || Convert.ToDecimal(purchasedata[10]) > 0 || purchasedata[12] == "1")
            {

                var Remark = "Receipt From Purchase Return";
                var reftype = "Purchase Return";
                long recid;
                //PRTransaction
                PRTransaction PRtran = new PRTransaction();
                PRtran.SupplierId = Convert.ToInt64(purchasedata[0]);
                PRtran.PRPayDate = DateTime.Parse(purchasedata[2], new CultureInfo("en-GB"));
                if (purchasedata[12] == "1")
                {
                    PRtran.PRPayAmount = amount;
                    recid = com.addReceipt(date, suppAccID, cashAccId, amount, amount, Remark, UserId, BranchID, purchaseReturnId, reftype);
                }
                else
                {
                    PRtran.PRPayAmount = amount;
                    recid = com.addReceipt(date, suppAccID, cashAccId, amount, amount, Remark, UserId, BranchID, purchaseReturnId, reftype);
                }
                PRtran.Recieptid = recid;
                PRtran.PRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                PRtran.CreatedBranch = Convert.ToInt32(BranchID);
                PRtran.CreatedUserId = UserId;
                PRtran.Status = 1;
                PRtran.PurchaseReturnId = purchaseReturnId;

                db.PRTransactions.Add(PRtran);
                db.SaveChanges();

            }
        }
        public void changeRecBill(long EntryId)
        {
            //receipt bill changes
            var chkrec = db.ReceiptBills.Where(a => a.InvoiceNo == EntryId && a.BillType == "Purchase Return").ToList();
            if (chkrec != null)
            {
                db.ReceiptBills.Where(a => a.InvoiceNo == EntryId && a.BillType == "Purchase Return").ToList().ForEach(a => a.Type = "New Reference");
                db.SaveChanges();
            }
        }


        [HttpGet]
        [QkAuthorize(Roles = "Dev,Download Purchase Return")]
        public ActionResult Download(long id)
        {
            var Data = db.PurchaseReturns.Where(s => s.PurchaseReturnId == id).FirstOrDefault();
            var supname = db.Suppliers.Where(s => s.SupplierID == Data.Supplier).Select(a => a.SupplierName).FirstOrDefault();

            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = Data.BillNo;

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), "inactive");
            return File(ms, "application/pdf", "Purchase Return" + "-" + supname + "-" + billno + ".pdf");

        }
        public StringBuilder generatePdf(long PurchaseReturnId)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

            var PurchaseReturnData = com.PurchaseReturnData(PurchaseReturnId, InPrintItemCode, PartNoCheck, TimeOut);
            var item = PurchaseReturnData.pdfItem.ToList();
            var summary = PurchaseReturnData;
            var billsundry = PurchaseReturnData.billsundry.ToList();


            return com.generatepdf(PurchaseReturnId, summary, item, billsundry, "Purchase");
        }

        //                   where a.PurchaseReturnId == PurchaseReturnId

        //                       PartyName = b.SupplierName,
        //                       BillNo = a.BillNo,
        //                       Date = a.PRDate,
        //                       Note = a.PRNote,
        //                       Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
        //                       Discount = a.PRDiscount,
        //                       Total = a.PRDiscount + a.PRGrandTotal,
        //                       GrandTotal = a.PRGrandTotal,
        //                       Paid = d.PReturnAmount,
        //                       Balance = a.PRGrandTotal - d.PReturnAmount,
        //                       SubTotal = a.PRSubTotal,
        //                       TaxAmount = a.PRTaxAmount,
        //                       c.Address,
        //                       c.City,
        //                       c.State,
        //                       c.Country,
        //                       c.Zip,
        //                       Email = c.EmailId,
        //                       Phone = c.Phone,
        //                       Mobile = c.Mobile,
        //                       TRN = b.TaxRegNo,
        //                       a.Remarks,
        //                       BillId = a.PurchaseReturnId,
        //                       TermsCondition = a.PRNote,



        //                 where b.PurchaseReturnId == PurchaseReturnId && b.itemNote != "-:{Bundle_Item}"
        //                     ItemUnitPrice = b.ItemUnitPrice,
        //                     ItemQuantity = b.ItemQuantity,
        //                     ItemSubTotal = b.ItemSubTotal,
        //                     ItemTax = b.ItemTax,
        //                     ItemNote = b.itemNote,
        //                     ItemTaxAmount = b.ItemTaxAmount,
        //                     ItemTotalAmount = b.ItemTotalAmount,
        //                     ItemID = b.Item,
        //                     bundleitem = (from ab in db.PRItemNotes
        //                                   where ab.PurchaseReturnId == PurchaseReturnId && ab.itemNote == "-:{Bundle_Item}"
        //                                   && b.Item == ab.ItemDiscount

        //                                       bb.ItemCode,
        //                                       bb.ItemName,
        //                                       cb.ItemUnitName,
        //                                       ItemUnitPrice = ab.ItemUnitPrice,
        //                                       quantity = ab.ItemQuantity,
        //                                       ItemSubTotal = ab.ItemSubTotal,
        //                                       ItemTax = ab.ItemTax,
        //                                       ItemTaxAmount = ab.ItemTaxAmount,
        //                                       ItemTotalAmount = ab.ItemTotalAmount,

        //                                       ab.Item,
        //                                       ab.ItemQuantity,
        //                                       ab.ItemUnit,

        //                                       ItemDiscount = 0,

        //                                       ItemNote = ab.itemNote,
        //                                       ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
        //                                       bb.ItemUnitID,
        //                                       bb.SubUnitId,
        //                                       PriUnit = cb.ItemUnitName,
        //                                       SubUnit = bd.ItemUnitName,
        //                                       bb.ItemArabic
        //                                   }).ToList()
        //        AmountType = b.AmountType,
        //        BsAmount = b.BsAmount,
        //        BsType = b.BsType,
        //        BsValue = b.BsValue != null ? b.BsValue : 0,
        //        BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
        //    .Select(s => new
        //        CName = s.CPName,
        //        CAddress = s.CPAddress,
        //        CEmail = s.CPEmail,
        //        CTaxRegNo = s.TRN,
        //        CPhone = s.CPPhone,
        //        s.CPMobile,
        //        CLogo = s.CPLogo,



        //                "<td width='50%'> " +
        //                "<table  style='border: 0px; width: 100 %;'><tr><th><i><b>Customer زبون</b></i></th></tr><tr><td>" + details.PartyName + "</td></tr><tr><td style='font-size:14px;font-weight:normal;'>" + address + "</td></tr></table></td><td width='50%' style='border-left: 1px solid #ccc;'>" +

        //                    sb.Append("<img width='40px' height='70px' src='" + LegacyWeb.MapPath("/uploads/itemimages/" + item.ItemID + "/" + item.FileName) + "'/>");










        [QkAuthorize(Roles = "Dev,Delete Purchase Return")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Delete
            var userpermission = User.IsInRole("All Purchase Return Entry");
            var UserId = User.Identity.GetUserId();
            PurchaseReturn PRrt = db.PurchaseReturns.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.PurchaseReturnId == id).FirstOrDefault();

            if (PRrt == null)
            {
                return NotFound();
            }
            return PartialView(PRrt);
        }
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete Purchase Return")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            #region Old Code
            ////var SET = db.AccountsTransactions.Where(a => a.Id == id).FirstOrDefault();

            #endregion

            var chk = DeletePReturn(id);
            if (chk == true)
            {
                stat = true;
                msg = "Successfully deleted Purchase Return details.";
            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Purchase Return")]
        public ActionResult DeleteAllPReturn(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeletePReturn(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + " Purchase Return.", true);
            return RedirectToAction("Index", "PurchaseReturn");
        }

        private Boolean DeletePReturn(long id)
        {
            var UserId = User.Identity.GetUserId();
            PurchaseReturn PErt = db.PurchaseReturns.Find(id);
            var PRItem = db.PRItemNotes.Where(a => a.PurchaseReturnId == id);
            var PRP = db.PRPayments.Where(a => a.PurchaseReturnId == id).FirstOrDefault();
            var PRPT = db.PRTransactions.Where(a => a.PurchaseReturnId == id).ToList();
            var PRBs = db.PRBillSundrys.Where(a => a.PurchaseReturnId == id).FirstOrDefault();
            var suppId = db.PurchaseReturns.Where(a => a.PurchaseReturnId == id).Select(a => a.Supplier).FirstOrDefault();
            var CurrentDate = Convert.ToDateTime(System.DateTime.Now);

            if (PErt.MaterialCenter != null)
                /***************** Item Transaction ******************/
                com.ItemTransInDeleteMode("PurchaseReturn", PErt.MaterialCenter, 0, 0, id, UserId, CurrentDate);

            db.PurchaseReturns.Remove(PErt);

            if (PRItem != null)
            {
                db.PRItemNotes.RemoveRange(db.PRItemNotes.Where(a => a.PurchaseReturnId == id));

            }
            if (PRBs != null)
            {
                db.PRBillSundrys.RemoveRange(db.PRBillSundrys.Where(a => a.PurchaseReturnId == id));
            }
            if (PRP != null)
            {
                db.PRPayments.RemoveRange(db.PRPayments.Where(a => a.PurchaseReturnId == id));
            }
            if (PRPT != null)
            {
                db.PRTransactions.RemoveRange(db.PRTransactions.Where(a => a.PurchaseReturnId == id));
            }

            var rec = db.Receipts.Where(a => a.Reference == id && a.RefType == "Purchase Return").FirstOrDefault();
            if (rec != null)
            {
                db.Receipts.RemoveRange(db.Receipts.Where(a => a.Reference == id && a.RefType == "Purchase Return"));
            }
            //receipt bill remove
            var recbill = db.ReceiptBills.Where(a => a.InvoiceNo == id && a.BillType == "Purchase Return" && a.Type == "Against Reference").ToList();
            if (recbill != null)
            {
                var recbillz = db.ReceiptBills.Where(a => a.InvoiceNo == id && a.BillType == "Purchase Return" && a.Type == "Against Reference").ToList();
                recbillz.ForEach(a =>
                {
                    a.Type = "New Reference";
                    a.BillType = null;
                    a.InvoiceNo = null;
                });
                db.SaveChanges();
            }


            if (PErt.purchaseEntryId != 0)
            {
                PEPayment PEP = db.PEPayments.Where(a => a.PurchaseEntry == PErt.purchaseEntryId).FirstOrDefault();
                PEP.PEPaidAmount = PEP.PEPaidAmount - PErt.PRGrandTotal;
                db.Entry(PEP).State = EntityState.Modified;
                db.SaveChanges();
            }
            var appr = db.Approvals.Where(a => a.TransEntry == id && a.Type == "PurchaseReturn").FirstOrDefault();
            if (appr != null)
            {
                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == id && a.Type == "PurchaseReturn"));
            }
            var app = db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "PurchaseReturn").FirstOrDefault();
            if (app != null)
            {
                db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "PurchaseReturn"));
            }
            // batch stock

            var SEBst = db.BatchStocks.Where(a => a.Reference == id && a.Type == "PReturn").FirstOrDefault();
            if (SEBst != null)
            {
                db.BatchStocks.RemoveRange(db.BatchStocks.Where(a => a.Reference == id && a.Type == "PReturn"));
                db.SaveChanges();
            }

            bool delete = com.DeleteAllAccountTransaction("Purchase Return", id);
            bool deletepay = com.DeleteAllAccountTransaction("Purchase Return Payment", id);
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "PurchaseReturn", "PurchaseReturns", findip(), PErt.PurchaseReturnId, "Successfully Deleted Purchase Return");

            return true;
        }
        private string InvoiceNo(Int64 PRNo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "PurchaseReturn").Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == "PurchaseReturn").Select(a => a.number).FirstOrDefault();
            if (billNo == null)
            {
                if ((db.PurchaseReturns.Where(o => o.Ref5 == "Debit Note").Select(p => p.PRNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    PRNo = db.PurchaseReturns.Where(o => o.Ref5 == "Debit Note").Max(p => p.PRNo + 1);
                    billNo = companyPrefix + PRNo;
                    if (BillExist(billNo))
                    {
                        billNo = InvoiceNo(PRNo, billNo);
                    }
                }
            }
            else
            {
                PRNo = PRNo + 1;
                billNo = companyPrefix + PRNo;
                if (BillExist(billNo))
                {
                    billNo = InvoiceNo(PRNo, billNo);
                }

            }
            return billNo;
        }
        private bool BillExist(string PRNo)
        {
            var Exists = db.PurchaseReturns.Any(c => c.BillNo == PRNo);
            bool res = (Exists) ? true : false;
            return res;
        }
        private long GetPrNo()
        {
            Int64 PENo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "PurchaseReturn").Select(a => a.number).FirstOrDefault();
            if ((db.PurchaseReturns.Select(p => p.PRNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                if (number == 0)
                {
                    PENo = 1;
                }
                else
                {
                    PENo = number;
                }
            }
            else
            {
                PENo = db.PurchaseReturns.Max(p => p.PRNo + 1);
            }

            return PENo;
        }

        [HttpGet]
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "PurchaseReturn" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
                                       .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                       .ToList();

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       where e != ApprovalStatus.PendingApproval && (appstat.Count == 0 || e != appstat.Select(a => a.ApprovalStatus).FirstOrDefault())
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            return PartialView();
        }

        [HttpPost]
        public ActionResult EditStatus(ApprovalUpdate App, long id)
        {
            bool stat = false;
            string msg = "";
            var UserId = User.Identity.GetUserId();

            var MR = db.PurchaseReturns.Where(a => a.PurchaseReturnId == id).FirstOrDefault();

            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "PurchaseReturn").OrderByDescending(a => a.CreatedDate).FirstOrDefault();
            if ((chkappby == null) || (chkappby.ApprovalStatus != App.ApprovalStatus))
            {
                ApprovalUpdate AppUp = new ApprovalUpdate();
                AppUp.ApprovalStatus = App.ApprovalStatus;
                AppUp.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                AppUp.ApprovedBy = UserId;
                AppUp.Note = App.Note;
                AppUp.RequestBy = MR.CreatedBy;
                AppUp.Status = Status.active;
                AppUp.TransEntry = id;
                AppUp.Type = "PurchaseReturn";

                db.ApprovalUpdates.Add(AppUp);
                db.SaveChanges();

                stat = true;
                msg = "Successfully Updated Status.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                stat = false;
                msg = "Updating Same Status.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

        }

        [HttpPost]
        public ActionResult GetAllStatusUpdation(long MCId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            var UserView = (from b in db.ApprovalUpdates
                            join c in db.Users on b.ApprovedBy equals c.Id
                            join d in db.PurchaseReturns on b.TransEntry equals d.PurchaseReturnId into team
                            from d in team.DefaultIfEmpty()
                            join e in db.Employees on b.RequestBy equals e.UserId into emp
                            from e in emp.DefaultIfEmpty()
                            join u in db.Users on d.CreatedBy equals u.Id into req
                            from u in req.DefaultIfEmpty()
                            where b.TransEntry == MCId && b.Type == "PurchaseReturn"
                            select new
                            {
                                b.ApprovalUpdateID,
                                b.TransEntry,
                                b.Status,
                                b.ApprovalStatus,
                                b.CreatedDate,
                                b.Note,
                                RequestBy = u.UserName,
                                c.UserName,
                                ApprovedBy = "" //e.FirstName + " " + e.LastName,
                            }).Distinct().ToList().Select(o => new
                            {
                                o.ApprovalUpdateID,
                                o.TransEntry,
                                o.Status,
                                ApprovalStatus = Enum.GetName(typeof(ApprovalStatus), o.ApprovalStatus),

                                o.ApprovedBy,
                                o.RequestBy,
                                User = o.UserName, //db.Users.Where(a => a.Id == o.CreatedUser).Select(a => a.UserName).FirstOrDefault(),
                                o.CreatedDate,
                                Remarks = o.Note
                            });
            recordsTotal = UserView.Count();
            var data = UserView.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
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
