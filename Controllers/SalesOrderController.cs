using QuickSoft.Web;
using System.Linq.Dynamic.Core;
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
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Drawing;


namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class SalesOrderController : BaseController
    {
        ApplicationDbContext db;
        Models.Common com;
        public SalesOrderController()
        {
            db = new ApplicationDbContext();
            com = new Models.Common();
        }

        // GET: SalesOrder 
        [QkAuthorize(Roles = "Dev,SalesOrder List")]
        public ActionResult Index()
        {
            ViewBag.Customer = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);
            ViewBag.CreatedBy = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);

            ViewBag.SalesExecutive = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
            }, "Value", "Text", 1);
            ViewBag.ItStatus = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Active", Value="0"},
                new SelectListItem() {Text = "Inactive", Value="1"},
            }, "Value", "Text");
            var created = db.Users.Select(s => new
            {
                ID = s.Id,
                Name = s.UserName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");
            _FinancialYear();
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
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

            var MlaSOrder = db.EnableSettings.Where(a => a.EnableType == "MLASOrder").FirstOrDefault();
            var MlaSOrders = MlaSOrder != null ? MlaSOrder.Status : Status.inactive;
            ViewBag.MLASOrder = MlaSOrders;

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindSOrder").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var proj = db.Projects
               .Select(s => new
               {
                   ID = s.ProjectId,
                   Name = s.ProCode + " " + s.ProjectName
               })
               .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");
            var tsk = db.ProTasks
             .Select(s => new
             {
                 ID = s.ProTaskId,
                 Name = s.TaskName
             })
             .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            return View();
        }
        [QkAuthorize(Roles = "Dev,SalesOrder Entry")]
        public ActionResult Create(long? id, string type)
        {
            var SOentry = new SalesOrderViewModel
            {
                BillNo = InvoiceNo(),
                SODate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                TermsCondition = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "sorder").Select(a => a.TermsCondit).FirstOrDefault(),
                SalesTypes = db.SalesTypes.ToList(),
            };


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

            companySet();

            var userpermission = User.IsInRole("All SalesOrder Entry");
            var UserId = User.Identity.GetUserId();

            ViewBag.LastEntry = db.SalesOrders.Where(a => a.CreatedUserId == UserId || userpermission == true).Select(p => p.SalesOrderId).AsEnumerable().DefaultIfEmpty(0).Max();

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;
            _FinancialYear();

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var proj = db.Projects
                .Select(s => new
                {
                    ID = s.ProjectId,
                    Name = s.ProCode + " " + s.ProjectName
                })
                .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");
            var tsk = db.ProTasks
             .Select(s => new
             {
                 ID = s.ProTaskId,
                 Name = s.TaskName
             })
             .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");
            ViewBag.PopUpAddCust = false;

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
            .Select(s => new
            {
                ID = s.EmployeeId,
                Name = s.FirstName + " " + s.LastName
            })
            .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var MlaSOrder = db.EnableSettings.Where(a => a.EnableType == "MLASOrder").FirstOrDefault();
            var MlaSOrders = MlaSOrder != null ? MlaSOrder.Status : Status.inactive;
            ViewBag.MLASOrder = MlaSOrders;
                        
            if (type == "Quote")
            {
                Quotation quote = db.Quotations.Find(id);
                if (quote == null)
                {
                    return NotFound();
                }
                SOentry.ConTypeId = quote.QuotationId;
                SOentry.ConType = type;
                SOentry.SODate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                SOentry.Customer = quote.Customer;
                SOentry.SOGrandTotal = quote.QuotGrandTotal;
                var custmr = db.Customers.Find(quote.Customer);
                SOentry.CustomerEmail = db.Contacts.Where(a => a.ContactID == custmr.Contact).Select(a => a.EmailId).FirstOrDefault();
                SOentry.Remarks = quote.Remarks;
                SOentry.Branch = quote.Branch;
                SOentry.SalesType = quote.SalesType;
                SOentry.SaleType = quote.SaleType;
                SOentry.SOCashier = quote.QuotCashier;

                if(ViewBag.BusinessType== "ProjectBasedBusiness")
                {
                    SOentry.Project = quote.Project;
                    SOentry.ProTask = quote.ProTask;
                }
                if (quote.SaleType == SaleType.Hire)
                {
                    var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Sales order").FirstOrDefault();
                    SOentry.FromDate = Hdet.StartDate;
                    SOentry.ToDate = Hdet.EndDate;
                    SOentry.HireType = Hdet.HireType;
                    SOentry.BillNo = InvoiceNo(0, null, "Hire");
                }
                SOentry.convertFrom = type + " No";//label
                SOentry.convertBill = quote.BillNo;
                SOentry.TermsCondition = quote.TermsCondition;
            }

            var cust = db.Customers.Where(s => s.Type == CRMCustomerType.Customer).Select(s => new
            {
                CustomerID = s.CustomerID,
                CustomerDetails = s.CustomerCode + " - " + s.CustomerName
            }).ToList();
            ViewBag.Customr = QkSelect.List(cust, "CustomerID", "CustomerDetails");
            var use = db.Employees
                             .Select(s => new
                             {
                                 ID = s.EmployeeId,
                                 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                             })
                             .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            SOentry.FieldMap = db.FieldMappings.Where(a => a.Section == "SOrder" && a.Status == Status.active).ToList();
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

            return View(SOentry);
        }
        [QkAuthorize(Roles = "Dev,SalesOrder Entry")]
        public JsonResult CreateSalesOrder(string[][] array, string[] sodata, string action)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                if (!BillExist(Convert.ToString(sodata[15])))
                {
                    var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
                    var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

                    var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
                    var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

                    long Branch = 0;
                    var UserId = User.Identity.GetUserId();

                    var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                    var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                    if (BranchCheck == Status.active)
                    {
                        Branch = Convert.ToInt64(sodata[17]);
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    }


                    var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();

                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

                    var ItemCount = 0;
                    decimal ItemQty = 0;
                    decimal TotTax = 0;
                    decimal ItemDisc = 0;
                    decimal ItemSubTotal = 0;
                    decimal ItemGrandTotal = 0;

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
                    dtItem.Columns.Add("ItemNote");
                    dtItem.Columns.Add("SalesOrder");
                    dtItem.Columns.Add("Item");

                    foreach (var arr in array)
                    {
                        decimal qty = Convert.ToDecimal(arr[2]);
                        decimal unitprice = Convert.ToDecimal(arr[3]);
                        decimal discount =  arr[6] != "" ? Convert.ToDecimal(arr[6]) : 0;
                        decimal tax = Convert.ToDecimal(arr[10]);

                        decimal subtotal = qty * unitprice;
                        decimal newsub = subtotal - discount;

                        decimal taxamt = newsub * (tax / 100);
                        decimal total = newsub + taxamt;


                        DataRow dr = dtItem.NewRow();
                        dr["ItemUnit"] = arr[1];
                        dr["ItemUnitPrice"] = unitprice;
                        dr["ItemQuantity"] = qty;
                        dr["ItemSubTotal"] = subtotal;
                        dr["ItemDiscount"] = discount;
                        dr["ItemTax"] = tax;
                        dr["ItemTaxAmount"] = taxamt;
                        dr["ItemTotalAmount"] = total;

                        dr["itemNote"] = Convert.ToString(arr[32].Replace("\n", "<br />"));
                        dr["SalesOrder"] = null;
                        dr["Item"] = Convert.ToInt32(arr[0]);

                        dtItem.Rows.Add(dr);


                        ItemCount++;
                        ItemQty += qty;
                        ItemDisc += discount;
                        TotTax += taxamt;
                        ItemSubTotal += subtotal;
                        ItemGrandTotal += total;
                        SaleType Satype = SaleType.Sale;
                        if (sodata[18] != null)
                        {
                            string str = sodata[18];
                            Satype = (SaleType)Enum.Parse(typeof(SaleType), str);
                        }
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
                            long typ = Convert.ToInt64(sodata[21]);
                            var bundle = (from a in db.BundleItems
                                          join b in db.Items on a.ItemId equals b.ItemID
                                          join c in db.ItemUnits on a.ItemUnit equals c.ItemUnitID into primary
                                          from c in primary.DefaultIfEmpty()
                                          let hir = db.HireRates.Where(x => x.ItemId == b.ItemID && x.type == typ).Select(y => y.Rate).FirstOrDefault()
                                          where a.ItemBundle == itemBundle.ItemBundleId
                                          select new
                                          {
                                              b.ItemCode,
                                              b.ItemName,
                                              c.ItemUnitName,
                                              ItemUnitPrice = (Satype == SaleType.Sale) ? a.ItemSubTotal : hir,
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
                                var ItemSubTotals = qua * bu.ItemUnitPrice;
                                var buTaxAmount = (ItemSubTotals * bu.ItemTax) / 100;

                                decimal itemtax = 0;
                                decimal taxamts = 0;
                                decimal totamt = 0;

                                itemtax = bu.ItemTax;
                                taxamts = buTaxAmount;
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
                                dbu["SalesOrder"] = null;
                                dbu["Item"] = bu.Item;
                                dtItem.Rows.Add(dbu);
                            }
                        }

                    }

                    //sales entry
                    SalesOrder SOrder = new SalesOrder();
                    if (sodata[18] != null)
                    {
                        string str = sodata[18];
                        SaleType Stype = (SaleType)Enum.Parse(typeof(SaleType), str);
                        SOrder.SaleType = Stype;
                    }
                    else
                    {
                        SOrder.SaleType = SaleType.Sale;
                    }
                    SOrder.SONo = GetQeNo(SOrder.SaleType);
                    SOrder.BillNo = Convert.ToString(sodata[15]);
                    SOrder.SODate = DateTime.Parse(sodata[2], new CultureInfo("en-GB"));
                    SOrder.SOCashier = sodata[1] != "" ? Convert.ToInt64(sodata[1]) : 0;
                    SOrder.Customer = Convert.ToInt64(sodata[0]);

                    SOrder.SOItems = ItemCount;
                    SOrder.SOItemQuantity = ItemQty;
                    SOrder.SOSubTotal = ItemSubTotal;
                    SOrder.SOTax = TotTax;
                    SOrder.SOTaxAmount = TotTax;
                    SOrder.SODiscount = ItemDisc;
                    SOrder.SOGrandTotal = ItemGrandTotal;
                    SOrder.SONote = "";
                    SOrder.Mail = 0;
                    SOrder.SOCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                    SOrder.CreatedUserId = UserId;
                    SOrder.Status = Status.active;
                    SOrder.TermsCondition = Convert.ToString(sodata[11]);
                    SOrder.EmailTemplateID = db.EmailTemplates.Where(a => a.Head == "SalesOrder").Select(a => a.EmailTemplateID).FirstOrDefault();
                    SOrder.CompanyHeaderID = 0;
                    SOrder.Branch = Branch;
                    SOrder.SOValidity = Convert.ToInt32(sodata[10]);
                    SOrder.Remarks = sodata[16];
                    SOrder.SalesType = Convert.ToInt32(sodata[22]);
                    SOrder.Project = sodata[23] != "" ? Convert.ToInt64(sodata[23]) : 0;
                    SOrder.ProTask = sodata[24] != "" ? Convert.ToInt64(sodata[24]) : 0;

                    SOrder.Ref1 = Convert.ToString(sodata[28]);
                    SOrder.Ref2 = Convert.ToString(sodata[29]);
                    SOrder.Ref3 = Convert.ToString(sodata[30]);
                    SOrder.Ref4 = Convert.ToString(sodata[31]);
                    SOrder.Ref5 = Convert.ToString(sodata[32]);

                    db.SalesOrders.Add(SOrder);
                    db.SaveChanges();
                    Int64 SOrderId = 0;
                    SOrderId = SOrder.SalesOrderId;

                    if (SOrder.SaleType == SaleType.Hire)
                    {
                        HireDetail HDetils = new HireDetail();
                        HDetils.StartDate = DateTime.Parse(sodata[19], new CultureInfo("en-GB"));
                        HDetils.EndDate = DateTime.Parse(sodata[20], new CultureInfo("en-GB"));
                        HDetils.Section = "Sales order";
                        HDetils.HireType = Convert.ToInt64(sodata[21]);
                        HDetils.Reference = SOrderId;
                        db.HireDetails.Add(HDetils);
                        db.SaveChanges();
                    }


                    //add SalesOrder Id in Item
                    foreach (DataRow row in dtItem.Rows)
                    {
                        row["SalesOrder"] = SOrderId;
                    }


                    SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "TableTypeSOItems";
                    //// execute sp sql 
                    string sql = String.Format("EXEC {0} {1};", "SP_InsertSalesOrderItems", "@TableType");
                    //// execute sql 
                    db.Database.ExecuteSqlRaw(sql, parameter);

                    //Approved By
                    var Appby = Convert.ToString(sodata[25]);
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = SOrderId;
                            approval.Type = "SalesOrder";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }

                    if (sodata[27] != null && sodata[27] != "0" && sodata[27] != "" && sodata[26] != null && sodata[26] != "" && sodata[26] != "0")
                    {
                        string[] List = sodata[27].Split(',');

                        foreach (var arr in List)
                        {
                            ConvertTransactions ConTran = new ConvertTransactions();

                            ConTran.ConvertFrom = sodata[26];
                            ConTran.ConvertTo = "SOrder";
                            ConTran.From = Convert.ToInt64(arr);
                            ConTran.To = SOrderId;
                            ConTran.Status = 0;
                            ConTran.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            ConTran.CreatedBy = UserId;
                            ConTran.Branch = Convert.ToInt32(BranchID);

                            db.ConvertTransactionss.Add(ConTran);
                            db.SaveChanges();
                            com.addlog(LogTypes.Created, UserId, "ConvertTransactions", "ConvertTransactionss", findip(), ConTran.Id, "Successfully Submitted Conversion");

                        }
                    }


                    com.addlog(LogTypes.Created, UserId, "SalesOrder", "SalesOrders", findip(), SOrderId, "Successfully Submitted SalesOrders");

                    Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                    TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;




                    if (action == "print")
                    {
                        var fmapp = db.FieldMappings.Where(a => a.Section == "SOrder" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                        string qedate = SOrder.SODate.ToString("dd-MM-yyyy");
                        var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                        var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;


                        var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                        var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                        var saleData = com.SalesOrderData(SOrderId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, ComHeadCheck);
                        var item = saleData.pdfItem.ToList();
                        var summary = saleData;

                        var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                        var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                        var def = (PriLay == Status.active) ? Convert.ToInt64(sodata[33]) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                        def = def == 0 ? 1 : def;
                        var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, layout , fmapp, salesorderid = SOrderId } };
                    }
                    else if (action == "sendmail")
                    {

                        SendMail sm = new SendMail();
                        MailMessage message = new MailMessage();
                        string ToMail = sodata[12];
                        string CcMail = sodata[13];
                        string InvoiceNo = "_SalesOrder_" + SOrder.SONo;

                        var em = db.EmailTemplates.Where(a => a.Head == "SalesOrder").FirstOrDefault();
                        if (em != null)
                        {
                            message.Subject = em.Subject;
                            message.Body = em.EmailBody;
                        }
                        else
                        {
                            message.Subject = "Sales Order";
                            message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                                " <p>we are enclosing our sales order for the items / services as requested by you during our discussions.<br/></p> " +
                                " <p>Looking forward to hear from you.</p>";
                        }
                        sm.SendPdfMail(generatePdf(SOrderId), ToMail, CcMail, InvoiceNo, message);

                        msg = "Successfully submitted Sales Order.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    else
                    {
                        msg = "Successfully submitted Sales Order.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg,salesorderid= SOrderId } };
                    }
                    //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {
                    msg = "Sales Order No. Already Exists.";
                    stat = false;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

                }

            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

        }
        public ActionResult UploadFiles()
        {
            // Checking no of files injected in Request object


            long id = Convert.ToInt64(Request.Form.GetValues("id").First());



            if (Request.Form.Files.Count > 0)
            {
                try
                {




                    IFormFileCollection files = Request.Form.Files;
                    if (files.Count > 0)
                    {
                        string path = LegacyWeb.MapPath("~/uploads/salesorderdocument/");
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        for (int i = 0; i < files.Count; i++)
                        {
                            IFormFile file = files[i];
                            if (file.Length > 0)
                            {


                                var fileCount = db.salesorderdocuments.Select(a => a.sid).AsEnumerable().DefaultIfEmpty(0).Max();

                                var fileName = Path.GetFileName(file.FileName);

                                String extension = Path.GetExtension(fileName);

                                var FStatus = Status.active;
                                String newName = fileCount + extension;
                                string newFName = fileCount + extension;

                                var thumbName = "";
                                var resizeName = "";
                                if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                                {
                                    thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/salesorderdocument/"), thumbName);

                                    resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/salesorderdocument/"), resizeName);
                                    newFName = "resize_" + newFName;
                                    FStatus = Status.inactive;
                                }
                                else
                                {
                                    var commonfilename = "Docs-Thump.png";

                                }
                                newName = Path.Combine(LegacyWeb.MapPath("~/uploads/salesorderdocument/"), newName);
                                file.SaveAs(newName);

                                var qtndoc = new salesorderdocument
                                {
                                    salesorderidID = id,
                                    FileName = newFName,//Path.GetFileName(file.FileName),
                                    Status = FStatus,
                                    CreatedDate = Convert.ToDateTime(System.DateTime.Now)
                                };
                                db.salesorderdocuments.Add(qtndoc);
                                db.SaveChanges();

                                if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                                {
                                    Image img = Image.FromFile(newName);
                                    int imgHeight = 100;
                                    int imgWidth = 100;
                                    if (img.Width < img.Height)
                                    {
                                        //portrait image  
                                        imgHeight = 100;
                                        var imgRatio = (float)imgHeight / (float)img.Height;
                                        imgWidth = Convert.ToInt32(img.Height * imgRatio);
                                    }
                                    else if (img.Height < img.Width)
                                    {
                                        //landscape image  
                                        imgWidth = 100;
                                        var imgRatio = (float)imgWidth / (float)img.Width;
                                        imgHeight = Convert.ToInt32(img.Height * imgRatio);
                                    }
                                    Image thumb = img.GetThumbnailImage(imgWidth, imgHeight, () => false, IntPtr.Zero);
                                    thumb.Save(thumbName);

                                    Image lgimg = Image.FromFile(newName);
                                    if (lgimg.Width > 1800 || lgimg.Height > 1800)
                                    {
                                        Image imgs = Image.FromFile(newName);
                                        System.Drawing.Image thumbs = Common.resizeImage(imgs, new Size(1800, 1800));
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/salesorderdocument/"), resizeName);
                                        thumbs.Save(resizeName);
                                    }
                                    else
                                    {
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/salesorderdocument/"), resizeName);
                                        lgimg.Save(resizeName);
                                    }

                                }
                            }
                        }
                    }









                    return Json("File Uploaded Successfully!");
                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }
        [QkAuthorize(Roles = "Dev,Edit SalesOrder")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.image = (from b in db.salesorderdocuments
                             join c in db.SalesOrders on b.salesorderidID equals c.SalesOrderId
                             where c.SalesOrderId == id
                             select new salesorderdocumentviewmodel
                             {
                                 soid= b.sid,
                                 salesorderID = b.salesorderidID,
                                 FileName = b.FileName,
                             }).ToList();
            var userpermission = User.IsInRole("All SalesOrder Entry");
            var UserId = User.Identity.GetUserId();
            SalesOrder sorder = db.SalesOrders.Where(x => (x.CreatedUserId == UserId || userpermission == true) && x.SalesOrderId == id).FirstOrDefault();

            if (sorder == null)
            {
                return NotFound();
            }
            SalesOrderViewModel vmodel = new SalesOrderViewModel();
            var cust = db.Customers
                .Select(s => new
                {
                    CustomerID = s.CustomerID,
                    CustomerDetails = s.CustomerCode + " - " + s.CustomerName
                }).ToList();
            ViewBag.Custer = QkSelect.List(cust, "CustomerID", "CustomerDetails");


            var use = db.Employees
                             .Select(s => new
                             {
                                 ID = s.EmployeeId,
                                 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                             })
                             .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

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

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var proj = db.Projects
              .Select(s => new
              {
                  ID = s.ProjectId,
                  Name = s.ProCode + " " + s.ProjectName
              })
              .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");

            var tsk = db.ProTasks
             .Select(s => new
             {
                 ID = s.ProTaskId,
                 Name = s.TaskName
             })
             .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "SalesOrder").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaSOrder = db.EnableSettings.Where(a => a.EnableType == "MLASOrder").FirstOrDefault();
            var MlaSOrders = MlaSOrder != null ? MlaSOrder.Status : Status.inactive;
            ViewBag.MLASOrder = MlaSOrders;

            var CBill = "";
            var CType = "";
            var ConvertTran = db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "SOrder").FirstOrDefault();
            if (ConvertTran != null)
            {
                CType = ConvertTran.ConvertFrom + " No";
                if (ConvertTran.ConvertFrom == "Quote")
                {
                    CBill = db.Quotations.Where(a => a.QuotationId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
            }

            vmodel = (from b in db.SalesOrders
                      join c in db.HireDetails on new { c1 = b.SalesOrderId, c2 = "Sales order" }
                      equals new { c1 = c.Reference, c2 = c.Section } into hir
                      from c in hir.DefaultIfEmpty()
                      where b.SalesOrderId == id
                      select new SalesOrderViewModel
                      {
                          SONo = b.SONo,
                          Customer = b.Customer,
                          SODate = b.SODate,
                          BillNo = b.BillNo,
                          SOValidity = b.SOValidity,
                          SOCashier = b.SOCashier,
                          SODiscount = b.SODiscount,
                          SOGrandTotal = b.SOGrandTotal,
                          TermsCondition = b.TermsCondition,
                          Remarks = b.Remarks,
                          Branch = b.Branch,
                          SaleType = b.SaleType,
                          FromDate = c.StartDate,
                          ToDate = c.EndDate,
                          HireType = c.HireType,
                          SalesType=b.SalesType,
                          SalesTypes = db.SalesTypes.ToList(),
                          Project = b.Project,
                          ProTask = b.ProTask,
                          convertBill = CBill,
                          convertFrom = CType,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                      }).FirstOrDefault();
            companySet();
            ViewBag.preEntry = db.SalesOrders.Where(a => (a.SalesOrderId < id) && (userpermission == true || a.CreatedUserId == UserId)).Select(a => a.SalesOrderId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.SalesOrders.Where(a => (a.SalesOrderId < id) && (userpermission == true || a.CreatedUserId == UserId)).Select(a => a.SalesOrderId).DefaultIfEmpty().Min();

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;
            _FinancialYear();

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.PopUpAddCust = false;

            var EditPermission = User.IsInRole("Disable SOrder Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "SalesOrder", UserId);

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "SOrder" && a.Status == Status.active).ToList();

            //dummy table operations
            var DItem = db.DummySOrderItems.Where(a => a.SalesOrder == id).FirstOrDefault();
            var SItem = db.SalesOrderItems.Where(a => a.SalesOrder == id).FirstOrDefault();
            if (SItem == null && DItem != null)
            {
                var DItems = db.DummySOrderItems.Where(a => a.SalesOrder == id).ToList();
                foreach (var arr in DItems)
                {
                    //add to se-item table
                    SalesOrderItem sItem = new SalesOrderItem();
                    sItem.ItemUnit = arr.ItemUnit;
                    sItem.ItemUnitPrice = arr.ItemUnitPrice;
                    sItem.ItemQuantity = arr.ItemQuantity;
                    sItem.ItemSubTotal = arr.ItemSubTotal;
                    sItem.ItemDiscount = arr.ItemDiscount;
                    sItem.ItemTax = arr.ItemTax;
                    sItem.ItemTaxAmount = arr.ItemTaxAmount;
                    sItem.ItemTotalAmount = arr.ItemTotalAmount;
                    sItem.ItemNote = arr.ItemNote;
                    sItem.SalesOrder = arr.SalesOrder;
                    sItem.Item = arr.Item;
                    db.SalesOrderItems.Add(sItem);
                    db.SaveChanges();
                }

                db.DummySOrderItems.RemoveRange(db.DummySOrderItems.Where(a => a.SalesOrder == id));
                db.SaveChanges();
            }
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
            return View(vmodel);
        }
        [HttpGet]
        public ActionResult GetSOItems2(long SOEntryID, string ConvertTo)
        {
            var temp = db.ConvertTransactionss.Where(a => a.From == SOEntryID && a.ConvertFrom == "SOrder" && a.ConvertTo == ConvertTo).Select(a => a.To);
            List<ItemList2> DVItems = new List<ItemList2>();
            List<ItemList2> temp6 = new List<ItemList2>();
            List<ItemList2> DVitemsGroupBy = new List<ItemList2>();
            List<ItemList2> RemainingItems = new List<ItemList2>();
            foreach (var tem in temp)
            {
                var temp2 = (from a in db.DvItems
                             join b in db.Items on a.Item equals b.ItemID into t1
                             from b in t1.DefaultIfEmpty()
                             join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                             from c in primary.DefaultIfEmpty()
                             join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                             from d in second.DefaultIfEmpty()
                             where a.Dv == tem
                             select new ItemList2
                             {
                                 Item = a.Item,
                                 ItemQuantity = a.ItemQuantity,
                                 ItemUnit = a.ItemUnit,
                                 ItemUnitPrice = a.ItemUnitPrice,
                                 ItemTax = a.ItemTax,
                                 ItemSubTotal = a.ItemSubTotal,
                                 ItemTaxAmount = a.ItemTaxAmount,
                                 ItemDiscount = a.ItemDiscount,
                                 note = a.ItemNote.Replace("<br />", "\n"),
                                 ItemNote = a.ItemNote != null ? a.ItemNote : "",
                                 ItemTotalAmount = a.ItemTotalAmount,
                                 ItemCode = b.ItemCode,
                                 ItemName = b.ItemName,
                                 ItemWithCode = b.ItemCode + " - " + b.ItemName,
                                 ItemUnitID = b.ItemUnitID,
                                 SubUnitId = b.SubUnitId,
                                 PriUnit = c.ItemUnitName,
                                 SubUnit = d.ItemUnitName,
                                 BasePrice = b.BasePrice,
                                 SellingPrice = b.SellingPrice,
                                 PurchasePrice = b.PurchasePrice,
                                 MRP = b.MRP
                             });
                DVItems.AddRange(temp2);
            }
            DVitemsGroupBy = (from a in DVItems
                              group new { a.Item, a.ItemQuantity, a.ItemUnit, a.ItemUnitPrice, a.ItemTax, a.ItemSubTotal, a.ItemTaxAmount, a.ItemDiscount, a.note, a.ItemNote, a.ItemTotalAmount, a.ItemCode, a.ItemName, a.ItemWithCode, a.ItemUnitID, a.SubUnitId, a.PriUnit, a.SubUnit, a.BasePrice, a.SellingPrice, a.PurchasePrice, a.MRP } by new { a.Item } into g
                              select new ItemList2
                              {
                                  Item = g.FirstOrDefault().Item,
                                  ItemQuantity = g.Sum(k => -k.ItemQuantity),
                                  ItemUnit = g.FirstOrDefault().ItemUnit,
                                  ItemUnitPrice = g.FirstOrDefault().ItemUnitPrice,
                                  ItemTax = g.FirstOrDefault().ItemTax,
                                  ItemSubTotal = g.Sum(k => -k.ItemSubTotal),
                                  ItemTaxAmount = g.Sum(k => -k.ItemTaxAmount),
                                  ItemDiscount = g.Sum(k => -k.ItemDiscount),
                                  note = g.FirstOrDefault().note,
                                  ItemNote = g.FirstOrDefault().ItemNote,
                                  ItemTotalAmount = g.Sum(k => -k.ItemTotalAmount),
                                  ItemCode = g.FirstOrDefault().ItemCode,
                                  ItemName = g.FirstOrDefault().ItemName,
                                  ItemWithCode = g.FirstOrDefault().ItemWithCode,
                                  ItemUnitID = g.FirstOrDefault().ItemUnitID,
                                  SubUnitId = g.FirstOrDefault().SubUnitId,
                                  PriUnit = g.FirstOrDefault().PriUnit,
                                  SubUnit = g.FirstOrDefault().SubUnit,
                                  BasePrice = g.FirstOrDefault().BasePrice,
                                  SellingPrice = g.FirstOrDefault().SellingPrice,
                                  PurchasePrice = g.FirstOrDefault().PurchasePrice,
                                  MRP = g.FirstOrDefault().MRP
                              }).ToList();

            var ConD = (from a in db.SalesOrderItems
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.SalesOrder == SOEntryID && a.ItemNote != "-:{Bundle_Item}"
                        select new ItemList2
                        {
                            Item = a.Item,
                            ItemQuantity = a.ItemQuantity,
                            ItemUnit = a.ItemUnit,
                            ItemUnitPrice = a.ItemUnitPrice,
                            ItemTax = a.ItemTax,
                            ItemSubTotal = a.ItemSubTotal,
                            ItemTaxAmount = a.ItemTaxAmount,
                            ItemDiscount = a.ItemDiscount,
                            note = a.ItemNote.Replace("<br />", "\n"),
                            ItemNote = a.ItemNote != null ? a.ItemNote : "",
                            ItemTotalAmount = a.ItemTotalAmount,
                            ItemCode = b.ItemCode,
                            ItemName = b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            ItemUnitID = b.ItemUnitID,
                            SubUnitId = b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            BasePrice = b.BasePrice,
                            SellingPrice = b.SellingPrice,
                            PurchasePrice = b.PurchasePrice,
                            MRP = b.MRP
                        });
            DVitemsGroupBy.AddRange(ConD);
            RemainingItems = (from a in DVitemsGroupBy
                              group new { a.Item, a.ItemQuantity, a.ItemUnit, a.ItemUnitPrice, a.ItemTax, a.ItemSubTotal, a.ItemTaxAmount, a.ItemDiscount, a.note, a.ItemNote, a.ItemTotalAmount, a.ItemCode, a.ItemName, a.ItemWithCode, a.ItemUnitID, a.SubUnitId, a.PriUnit, a.SubUnit, a.BasePrice, a.SellingPrice, a.PurchasePrice, a.MRP } by new { a.Item } into g
                              select new ItemList2
                              {
                                  Item = g.FirstOrDefault().Item,
                                  ItemQuantity = g.Sum(k => k.ItemQuantity),
                                  ItemUnit = g.FirstOrDefault().ItemUnit,
                                  ItemUnitPrice = g.FirstOrDefault().ItemUnitPrice,
                                  ItemTax = g.FirstOrDefault().ItemTax,
                                  ItemSubTotal = g.Sum(k => k.ItemSubTotal),
                                  ItemTaxAmount = g.Sum(k => k.ItemTaxAmount),
                                  ItemDiscount = g.Sum(k => k.ItemDiscount),
                                  note = g.FirstOrDefault().note,
                                  ItemNote = g.FirstOrDefault().ItemNote,
                                  ItemTotalAmount = g.Sum(k => k.ItemTotalAmount),
                                  ItemCode = g.FirstOrDefault().ItemCode,
                                  ItemName = g.FirstOrDefault().ItemName,
                                  ItemWithCode = g.FirstOrDefault().ItemWithCode,
                                  ItemUnitID = g.FirstOrDefault().ItemUnitID,
                                  SubUnitId = g.FirstOrDefault().SubUnitId,
                                  PriUnit = g.FirstOrDefault().PriUnit,
                                  SubUnit = g.FirstOrDefault().SubUnit,
                                  BasePrice = g.FirstOrDefault().BasePrice,
                                  SellingPrice = g.FirstOrDefault().SellingPrice,
                                  PurchasePrice = g.FirstOrDefault().PurchasePrice,
                                  MRP = g.FirstOrDefault().MRP
                              }).ToList();
            RemainingItems = RemainingItems.Where(a => a.ItemQuantity != 0).ToList();
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(RemainingItems);
            return Json(result);
        }

        [HttpGet]
        public ActionResult GetSOItems(long SOEntryID)
        {
            var ConD = (from a in db.SalesOrderItems
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.SalesOrder == SOEntryID && a.ItemNote != "-:{Bundle_Item}"
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
                            note = a.ItemNote.Replace("<br />", "\n"),
                            ItemNote = a.ItemNote != null ? a.ItemNote : "",
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
                            b.MRP
                        });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(ConD);
            return Json(result);
        }
        
        [QkAuthorize(Roles = "Dev,Edit SalesOrder")]
        public JsonResult UpdateSalesOrder(string[][] array, string[] sodata, string action)
        {
            bool stat = false;
            string msg;
            Int64 soEntryId = Convert.ToInt64(sodata[16]);
            SalesOrder SOrder = db.SalesOrders.Find(soEntryId);
            if (BillExist(Convert.ToString(sodata[15])) && Convert.ToString(sodata[15]) != SOrder.BillNo)
            {
                msg = "Invoice No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            if (ModelState.IsValid)
            {
                var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
                var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

                var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
                var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;


                var UserId = User.Identity.GetUserId();
                var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();

                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

                long Branch = 0;

                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                if (BranchCheck == Status.active)
                {
                    Branch = Convert.ToInt64(sodata[18]);
                }
                else
                {
                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                }

                var EditPermission = User.IsInRole("Disable SOrder Edit After Approval");
                if (com.chkApproved(soEntryId, EditPermission, "SalesOrder", UserId) == true)
                {

                    var ItemCount = 0;
                    decimal ItemQty = 0;
                    decimal TotTax = 0;
                    decimal ItemDisc = 0;
                    decimal ItemSubTotal = 0;
                    decimal ItemGrandTotal = 0;
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
                    dtItem.Columns.Add("ItemNote");
                    dtItem.Columns.Add("SalesOrder");
                    dtItem.Columns.Add("Item");
                    foreach (var arr in array)
                    {
                        decimal qty = Convert.ToDecimal(arr[2]);
                        decimal unitprice = Convert.ToDecimal(arr[3]);
                        decimal discount = Convert.ToDecimal(arr[6]);
                        decimal tax = Convert.ToDecimal(arr[10]);

                        decimal subtotal = qty * unitprice;
                        decimal newsub = subtotal - discount;

                        decimal taxamt = newsub * (tax / 100);
                        decimal total = newsub + taxamt;

                        DataRow dr = dtItem.NewRow();
                        dr["ItemUnit"] = arr[1];
                        dr["ItemUnitPrice"] = unitprice;
                        dr["ItemQuantity"] = qty;
                        dr["ItemSubTotal"] = subtotal;
                        dr["ItemDiscount"] = discount;
                        dr["ItemTax"] = tax;
                        dr["ItemTaxAmount"] = taxamt;
                        dr["ItemTotalAmount"] = total;
                        dr["itemNote"] = Convert.ToString(arr[32].Replace("\n", "<br />"));
                        dr["SalesOrder"] = soEntryId;
                        dr["Item"] = Convert.ToInt32(arr[0]);

                        dtItem.Rows.Add(dr);


                        ItemCount++;
                        ItemQty += qty;
                        ItemDisc += discount;
                        TotTax += taxamt;
                        ItemSubTotal += subtotal;
                        ItemGrandTotal += total;
                        SaleType Satype = SaleType.Sale;
                        if (sodata[19] != null)
                        {
                            string str = sodata[19];
                            Satype = (SaleType)Enum.Parse(typeof(SaleType), str);
                        }
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
                            long typ = Convert.ToInt64(sodata[22]);
                            var bundle = (from a in db.BundleItems
                                          join b in db.Items on a.ItemId equals b.ItemID
                                          join c in db.ItemUnits on a.ItemUnit equals c.ItemUnitID into primary
                                          from c in primary.DefaultIfEmpty()
                                          let hir = db.HireRates.Where(x => x.ItemId == b.ItemID && x.type == typ).Select(y => y.Rate).FirstOrDefault()
                                          where a.ItemBundle == itemBundle.ItemBundleId
                                          select new
                                          {
                                              b.ItemCode,
                                              b.ItemName,
                                              c.ItemUnitName,
                                              ItemUnitPrice = (Satype == SaleType.Sale) ? a.ItemSubTotal : hir,
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
                                var ItemSubTotals = qua * bu.ItemUnitPrice;
                                var buTaxAmount = (ItemSubTotals * bu.ItemTax) / 100;

                                decimal itemtax = 0;
                                decimal taxamts = 0;
                                decimal totamt = 0;

                                itemtax = bu.ItemTax;
                                taxamts = buTaxAmount;
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
                                dbu["SalesOrder"] = soEntryId;
                                dbu["Item"] = bu.Item;
                                dtItem.Rows.Add(dbu);
                            }
                        }

                    }



                    //sales entry


                    SOrder.BillNo = (sodata[15]);
                    SOrder.SODate = DateTime.Parse(sodata[2], new CultureInfo("en-GB"));
                    SOrder.SOCashier = sodata[1] != "" ? Convert.ToInt64(sodata[1]) : 0;
                    SOrder.Customer = Convert.ToInt64(sodata[0]);

                    SOrder.SOItems = ItemCount;
                    SOrder.SOItemQuantity = ItemQty;
                    SOrder.SOSubTotal = ItemSubTotal;
                    SOrder.SOTax = TotTax;
                    SOrder.SOTaxAmount = TotTax;
                    SOrder.SODiscount = ItemDisc;
                    SOrder.SOGrandTotal = ItemGrandTotal;
                    SOrder.SONote = "";
                    SOrder.Mail = 0;
                    SOrder.Status = Status.active;
                    SOrder.TermsCondition = Convert.ToString(sodata[11]);
                    SOrder.EmailTemplateID = db.EmailTemplates.Where(a => a.Head == "SalesOrder").Select(a => a.EmailTemplateID).FirstOrDefault();
                    SOrder.CompanyHeaderID = 0;
                    SOrder.Branch = Branch;
                    SOrder.SOValidity = Convert.ToInt32(sodata[10]);
                    SOrder.Remarks = sodata[17];
                    SOrder.SalesType = Convert.ToInt32(sodata[23]);
                    SOrder.Project = sodata[24] != "" ? Convert.ToInt64(sodata[24]) : 0;
                    SOrder.ProTask = sodata[25] != "" ? Convert.ToInt64(sodata[25]) : 0;

                    if (sodata[19] != null)
                    {
                        string str = sodata[19];
                        SaleType Stype = (SaleType)Enum.Parse(typeof(SaleType), str);
                        SOrder.SaleType = Stype;
                    }
                    else
                    {
                        SOrder.SaleType = SaleType.Sale;
                    }

                    SOrder.Ref1 = Convert.ToString(sodata[27]);
                    SOrder.Ref2 = Convert.ToString(sodata[28]);
                    SOrder.Ref3 = Convert.ToString(sodata[29]);
                    SOrder.Ref4 = Convert.ToString(sodata[30]);
                    SOrder.Ref5 = Convert.ToString(sodata[31]);

                    db.Entry(SOrder).State = EntityState.Modified;
                    db.SaveChanges();

                    var SOItem = db.SalesOrderItems.Where(a => a.SalesOrder == soEntryId).FirstOrDefault();
                    if (SOItem != null)
                    {

                        var SItems = db.SalesOrderItems.Where(a => a.SalesOrder == soEntryId).ToList();
                        foreach (var arr in SItems)
                        {
                            //add to dummy table
                            DummySOrderItem dItem = new DummySOrderItem();
                            dItem.ItemUnit = arr.ItemUnit;
                            dItem.ItemUnitPrice = arr.ItemUnitPrice;
                            dItem.ItemQuantity = arr.ItemQuantity;
                            dItem.ItemSubTotal = arr.ItemSubTotal;
                            dItem.ItemDiscount = arr.ItemDiscount;
                            dItem.ItemTax = arr.ItemTax;
                            dItem.ItemTaxAmount = arr.ItemTaxAmount;
                            dItem.ItemTotalAmount = arr.ItemTotalAmount;
                            dItem.ItemNote = arr.ItemNote;
                            dItem.SalesOrder = arr.SalesOrder;
                            dItem.Item = arr.Item;
                            db.DummySOrderItems.Add(dItem);
                            db.SaveChanges();
                        }

                        db.SalesOrderItems.RemoveRange(db.SalesOrderItems.Where(a => a.SalesOrder == soEntryId));
                        db.SaveChanges();
                    }

                    var HireItem = db.HireDetails.Where(a => a.Reference == soEntryId && a.Section == "Sales order").FirstOrDefault();
                    if (HireItem != null)
                    {
                        db.HireDetails.RemoveRange(db.HireDetails.Where(a => a.Reference == soEntryId && a.Section == "Sales order"));
                        db.SaveChanges();
                    }
                    if (SOrder.SaleType == SaleType.Hire)
                    {
                        HireDetail HDetils = new HireDetail();
                        HDetils.StartDate = DateTime.Parse(sodata[20], new CultureInfo("en-GB"));
                        HDetils.EndDate = DateTime.Parse(sodata[21], new CultureInfo("en-GB"));
                        HDetils.Section = "Sales order";
                        HDetils.Reference = soEntryId;
                        HDetils.HireType = Convert.ToInt64(sodata[22]);
                        db.HireDetails.Add(HDetils);
                        db.SaveChanges();
                    }

                    SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "TableTypeSOItems";
                    //// execute sp sql 
                    string sql = String.Format("EXEC {0} {1};", "SP_InsertSalesOrderItems", "@TableType");
                    //// execute sql 
                    var ret = db.Database.ExecuteSqlRaw(sql, parameter);
                    if (ret > 0)
                    {
                        db.DummySOrderItems.RemoveRange(db.DummySOrderItems.Where(a => a.SalesOrder == soEntryId));
                        db.SaveChanges();
                    }

                    var empuser = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                    var chkapp = db.Approvals.Where(a => a.EmployeeId == empuser && a.TransEntry == soEntryId && a.Type == "SalesOrder").FirstOrDefault();
                    var SoPo = db.Approvals.Where(a => a.TransEntry == soEntryId && a.Type == "SalesOrder").FirstOrDefault();
                    if (SoPo != null)
                    {
                        if (chkapp != null)
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.EmployeeId != empuser && a.TransEntry == soEntryId && a.Type == "SalesOrder"));
                            db.SaveChanges();
                        }
                        else
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == soEntryId && a.Type == "SalesOrder"));
                            db.SaveChanges();
                        }
                    }

                    var Appby = Convert.ToString(sodata[26]);
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = soEntryId;
                            approval.Type = "SalesOrder";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }


                    com.addlog(LogTypes.Updated, UserId, "SalesOrder", "SalesOrderS", findip(), soEntryId, "Successfully Updated SalesOrders");
                }

                Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

                if (action == "print")
                {
                    string sodate = SOrder.SODate.ToString("dd-MM-yyyy");
                    var fmapp = db.FieldMappings.Where(a => a.Section == "SOrder" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();


                    var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                    var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                    var saleData = com.SalesOrderData(soEntryId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, ComHeadCheck);
                    var item = saleData.pdfItem.ToList();
                    var summary = saleData;

                    var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                    var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                    var def = (PriLay == Status.active) ? Convert.ToInt64(sodata[32]) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                    def = def == 0 ? 1 : def;
                    var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, layout, fmapp, salesorderid = soEntryId } };

                }
                else if (action == "sendmail")
                {

                    SendMail sm = new SendMail();
                    MailMessage message = new MailMessage();
                    string ToMail = sodata[12];
                    string CcMail = sodata[13];
                    string InvoiceNo = "_SalesOrder_" + SOrder.SONo;

                    var em = db.EmailTemplates.Where(a => a.Head == "SalesOrder").FirstOrDefault();
                    if (em != null)
                    {
                        message.Subject = em.Subject;
                        message.Body = em.EmailBody;
                    }
                    else
                    {
                        message.Subject = "Sales Order";
                        message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                            " <p>we are enclosing our sales order for the items / services as requested by you during our discussions.<br/></p> " +
                            " <p>Looking forward to hear from you.</p>";
                    }
                    sm.SendPdfMail(generatePdf(soEntryId), ToMail, CcMail, InvoiceNo, message);

                    msg = "Successfully Updated Sales Order.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {
                    msg = "Successfully Updated Sales Order.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, salesorderid = soEntryId } };
                }
                //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Download SalesOrder")]
        public ActionResult Download(long id)
        {
            var Data = db.SalesOrders.Where(s => s.SalesOrderId == id).FirstOrDefault();
            var custname = db.Customers.Where(s => s.CustomerID == Data.Customer).Select(a => a.CustomerName).FirstOrDefault();

            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = Data.BillNo;

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), "inactive");
            return File(ms, "application/pdf", "Sales Order" + "-" + custname + "-" + billno + ".pdf");

        }
        public StringBuilder generatePdf(long sorderId)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

            var saleData = com.SalesOrderData(sorderId, InPrintItemCode, PartNoCheck, TimeOut);
            var item = saleData.pdfItem.ToList();
            var summary = saleData;

            return com.generatepdf(sorderId, summary, item, null, "Sale Order");
        }


        //                   where b.SalesOrderId == sorderId // b.Customer == customer
        //                       BillNo = b.BillNo,
        //                       SONo = b.SONo,
        //                       Date = b.SODate,
        //                       SOValidity = b.SOValidity,
        //                       SOGrandTotal = b.SOGrandTotal,
        //                       PartyName = c.CustomerName,
        //                       CustomerEmail = d.EmailId,
        //                       Address = d.Address,
        //                       City = d.City,
        //                       SubTotal = b.SOSubTotal,
        //                       Discount = b.SODiscount,
        //                       tc = b.SONote,
        //                       TaxAmount = b.SOTaxAmount,
        //                       State = d.State,
        //                       Country = d.Country,
        //                       Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
        //                       GrandTotal = b.SOGrandTotal,
        //                       TRN = c.TaxRegNo,
        //                       Email = d.EmailId,
        //                       Zip = d.Zip,
        //                       Phone = d.Phone,
        //                       Mobile = d.Mobile,
        //                       b.TermsCondition,
        //                       b.Remarks

        //                  where b.SalesOrder == sorderId && b.ItemNote != "-:{Bundle_Item}"
        //                      ItemUnitPrice = b.ItemUnitPrice,
        //                      ItemQuantity = b.ItemQuantity,
        //                      ItemTax = b.ItemTax,
        //                      ItemNote = b.ItemNote,
        //                      ItemTaxAmount = b.ItemTaxAmount,
        //                      ItemTotalAmount = b.ItemTotalAmount,
        //                      ItemSubTotal = b.ItemSubTotal,
        //                      ItemID = b.Item,
        //                      bundleitem = (from ab in db.SalesOrderItems
        //                                    where ab.SalesOrder == sorderId && ab.ItemNote == "-:{Bundle_Item}"
        //                                    && b.Item == ab.ItemDiscount

        //                                        bb.ItemCode,
        //                                        bb.ItemName,
        //                                        cb.ItemUnitName,
        //                                        ItemUnitPrice = ab.ItemUnitPrice,
        //                                        quantity = ab.ItemQuantity,
        //                                        ItemSubTotal = ab.ItemSubTotal,
        //                                        ItemTax = ab.ItemTax,
        //                                        ItemTaxAmount = ab.ItemTaxAmount,
        //                                        ItemTotalAmount = ab.ItemTotalAmount,

        //                                        ab.Item,
        //                                        ab.ItemQuantity,
        //                                        ab.ItemUnit,

        //                                        ItemDiscount = 0,

        //                                        ItemNote = ab.ItemNote,
        //                                        ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
        //                                        bb.ItemUnitID,
        //                                        bb.SubUnitId,
        //                                        PriUnit = cb.ItemUnitName,
        //                                        SubUnit = bd.ItemUnitName,
        //                                        bb.ItemArabic

        //    .Select(s => new
        //        CName = s.CPName,
        //        CAddress = s.CPAddress,
        //        CEmail = s.CPEmail,
        //        CTaxRegNo = s.TRN,
        //        CPhone = s.CPPhone,
        //        s.CPMobile,
        //        CLogo = s.CPLogo,



        //                "<td width='50%'> " +
        //                "<table  style='border: 0px; width: 100 %;'><tr><th><i><b>Customer زبون</b></i></th></tr><tr><td>" + details.PartyName + "</td></tr><tr><td style='font-size:14px;font-weight:normal;'>" + address + "</td></tr></table></td><td width='50%' style='border-left: 1px solid #000000;'>" +


        //                    sb.Append("<img width='40px' height='70px' src='" + LegacyWeb.MapPath("/uploads/itemimages/" + item.ItemID + "/" + item.FileName) + "'/>");





        [HttpPost]
        [QkAuthorize(Roles = "Dev,SalesOrder List")]
        public ActionResult GetSalesOrder(string InvoiceNo, string FromDate, string ToDate, long? customer, long? salesperson, string Stats, string user, int? Validity, string Saletype, long? HireType, string appstat , long? ProjectName, long? Task)
        {

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;

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



            string search = Request.Form.GetValues("search[value]")[0];

            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            Status st = new Status();
            if (Stats != "")
            {
                st = (Stats == "0") ? Status.active : Status.inactive;
            };
            SaleType St = new SaleType();
            if (Saletype != "")
            {
                St = (Saletype == "2") ? SaleType.Hire : SaleType.Sale;
            };
            var userpermission = User.IsInRole("All SalesOrder Entry");
            var UserId = User.Identity.GetUserId();

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


            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
            var fromv = "SOrder";
            var Tosales = "Sale";
            var ToPFA = "ProForma";
            var ToDVN = "DVNote";
            var uDev = User.IsInRole("Dev");
            var uSalesOrderView = User.IsInRole("View SalesOrder");
            var uEdit = User.IsInRole("Edit SalesOrder");
            var uDownload = User.IsInRole("Download SalesOrder");
            var uDelete = User.IsInRole("Delete SalesOrder");
            var SOrderToSale = db.EnableSettings.Where(a => a.EnableType == "SOrderToSale").FirstOrDefault();
            var SOrderToSales = SOrderToSale != null ? SOrderToSale.Status : Status.inactive;

            var SOrderToPF = db.EnableSettings.Where(a => a.EnableType == "SOrderToPF").FirstOrDefault();
            var SOrderToPFs = SOrderToPF != null ? SOrderToPF.Status : Status.inactive;

            var SOrderToDvNote = db.EnableSettings.Where(a => a.EnableType == "SOrderToDvNote").FirstOrDefault();
            var SOrderToDvNotes = SOrderToDvNote != null ? SOrderToDvNote.Status : Status.inactive;

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            // EF Core 10 cannot translate the nested-collection / GroupBy-latest projections (the `app`,
            // `AppStatus`, `chkAppStatus` lets and the `qs/pfa/dvn` ConvertTransactionss subqueries).
            // Split SERVER from CLIENT: materialize only entity columns + simple scalars (left-joined entity
            // access like Customer/EmpName/user and the PaymentTrans .Any() stay server-side) into serverRows,
            // then build client lookups keyed by SalesOrderId and re-project with the SAME member names + order.
            var serverQuery = (from a in db.SalesOrders
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join d in db.Employees on a.SOCashier equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                     join g in db.Users on a.CreatedUserId equals g.Id
                     join h in db.HireDetails on new { h1 = a.SalesOrderId, h2 = "Sales order" }
                    equals new { h1 = h.Reference, h2 = h.Section } into hir
                     from h in hir.DefaultIfEmpty()
                     join j in db.Projects on a.Project equals j.ProjectId into prj
                     from j in prj.DefaultIfEmpty()
                     join k in db.ProTasks on a.ProTask equals k.ProTaskId into task
                     from k in task.DefaultIfEmpty()
                         // qs/pfa/dvn (ConvertTransactionss .FirstOrDefault subqueries) and
                         // app/AppStatus/chkAppStatus (nested collections + GroupBy-latest) are all computed
                         // client-side after materialization — EF Core 10 can't translate them inside this query.
                     where (InvoiceNo == "" || a.BillNo == InvoiceNo) &&
                     (FromDate == "" || EF.Functions.DateDiffDay(a.SODate, fdate) <= 0) &&
                     (ToDate == "" || EF.Functions.DateDiffDay(a.SODate, tdate) >= 0) &&
                     (customer == 0 || b.CustomerID == customer) &&
                     (salesperson == 0 || salesperson == null || d.EmployeeId == salesperson)
                     && (Stats == null || a.Status == st)
                     && (user == null || user == "" || g.Id == user)
                     && (Validity == null || a.SOValidity == Validity)
                     && (userpermission == true || a.CreatedUserId == UserId)
                     && (Saletype == "" || Saletype == null || St == a.SaleType) && (HireType == null || HireType == h.HireType)
                     && (ProjectName == 0 || ProjectName == null || j.ProjectId == ProjectName)
                     && (Task == 0 || Task == null || k.ProTaskId == Task)
                     select new
                     {
                         a.SalesOrderId,
                         a.SONo,
                         a.BillNo,
                         a.SODate,
                         a.SOItems,
                         a.SOItemQuantity,
                         a.SODiscount,
                         a.SOGrandTotal,
                         a.SOTax,
                         a.SOTaxAmount,
                         Customer = b.CustomerCode + " - " + b.CustomerName,
                         EmpName = d.FirstName + " " + d.LastName,
                         user = g.UserName,
                         //PaymentStatus = c.Status,
                         PaymentTrans = db.SETransactions.Any(k => k.SalesEntry == a.SalesOrderId),
                         //c.SEPaidAmount,
                         a.Remarks,
                         validity = a.SOValidity,
                         SaleType = a.SaleType,
                         Dev = uDev,
                         Details = uSalesOrderView,
                         Edit = uEdit,
                         Download = uDownload,
                         Delete = uDelete,
                         ProjectName = (j.ProjectName != null && j.ProjectName != "") ? j.ProCode + "-" + j.ProjectName : "",
                         Task = (k.TaskName != null && k.TaskName != "") ? k.TaskCode + "-" + k.TaskName : "",
                         CreatedDate=a.SOCreatedDate
                     });

            // Performance (audit P2, hybrid): server paging when no search, plain-column sort, AND no client-side
            // filter is active (the conditions below mirror the filters' own guards); else original path.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase){ "BillNo","CreatedDate","Customer","Delete","Details","Dev","Download","Edit","EmpName","PaymentTrans","ProjectName","Remarks","SalesOrderId","SaleType","SODate","SODiscount","SOGrandTotal","SOItemQuantity","SOItems","SONo","SOTax","SOTaxAmount","Task","user","validity" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0 && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn)) && !(appstat != "");
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn) ? serverQuery.OrderBy("SalesOrderId asc") : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

            // CLIENT-side lookups keyed by SalesOrderId (missing key -> empty/absent, no KeyNotFound).
            var soIds = serverRows.Select(o => o.SalesOrderId).ToList();
            // The three convert markers: latest-or-any ConvertTransactionss row per (SalesOrderId, ConvertTo).
            var convLookup = db.ConvertTransactionss
                .Where(ap => ap.ConvertFrom == fromv && soIds.Contains(ap.From)
                       && (ap.ConvertTo == Tosales || ap.ConvertTo == ToPFA || ap.ConvertTo == ToDVN))
                .Select(ap => new { ap.From, ap.ConvertTo })
                .ToList()
                .ToLookup(ap => ap.From);
            // app = approver EmployeeIds for the sales order (nested collection, keyed by TransEntry == SalesOrderId).
            var appLookup = db.Approvals
                .Where(a => a.Type == "SalesOrder" && soIds.Contains(a.TransEntry))
                .Select(a => new { a.TransEntry, a.EmployeeId })
                .ToList()
                .ToLookup(a => a.TransEntry);
            // AppStatus = all ApprovalUpdate statuses; raw rows materialized once and reused for chkAppStatus.
            var appUpdRows = db.ApprovalUpdates
                .Where(a => a.Type == "SalesOrder" && soIds.Contains(a.TransEntry))
                .Select(a => new { a.TransEntry, a.ApprovalStatus, a.ApprovedBy, a.CreatedDate })
                .ToList();
            var appStatusLookup = appUpdRows.ToLookup(a => a.TransEntry);
            // chkAppStatus = latest ApprovalUpdate status per ApprovedBy (GroupBy-latest-per-group), per sales order.
            var chkAppStatusLookup = appUpdRows
                .GroupBy(a => a.TransEntry)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(l => l.ApprovedBy)
                    .Select(grp => grp.OrderByDescending(c => c.CreatedDate).First())
                    .Select(a => a.ApprovalStatus).ToList());

            var v = serverRows.Select(o =>
                     {
                         var conv = convLookup[o.SalesOrderId];
                         var SaleConvert = conv.Where(x => x.ConvertTo == Tosales).Select(x => x.ConvertTo).FirstOrDefault();
                         var PFAConvert = conv.Where(x => x.ConvertTo == ToPFA).Select(x => x.ConvertTo).FirstOrDefault();
                         var DVNConvert = conv.Where(x => x.ConvertTo == ToDVN).Select(x => x.ConvertTo).FirstOrDefault();
                         var app = appLookup[o.SalesOrderId].Select(x => x.EmployeeId).ToList();
                         var AppStatus = appStatusLookup[o.SalesOrderId].Select(x => x.ApprovalStatus).ToList();
                         var chkAppStatus = chkAppStatusLookup.TryGetValue(o.SalesOrderId, out var ck) ? ck : new List<ApprovalStatus>();
                         return new
                     {
                         SaleConvert = SaleConvert,
                         PFAConvert = PFAConvert,
                         DVNConvert = DVNConvert,

                         o.SalesOrderId,
                         o.SONo,
                         o.BillNo,
                         o.SODate,
                         o.SOItems,
                         o.SOItemQuantity,
                         o.SODiscount,
                         o.SOGrandTotal,
                         o.SOTax,
                         o.SOTaxAmount,
                         o.Customer,
                         o.EmpName ,
                         o.user ,
                         //PaymentStatus = c.Status,
                         o.PaymentTrans ,
                         //c.SEPaidAmount,
                         o.Remarks,
                         o.validity,
                         o.SaleType ,
                         o.Dev,
                         o.Details,
                         o.Edit,
                         o.Download,
                         o.Delete,
                         app = app,
                         Approval = (app != null && empl.EmployeeId != null) ? (app.Contains(empl.EmployeeId) ? true : false) : false,
                         ApprovalStatus = (app.Count > 0 && AppStatus.Count > 0 && chkAppStatus.Count > 0) ? (chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (app != null && chkAppStatus != null && app.Count != 0 && chkAppStatus.Count != 0 && chkAppStatus.Count() == app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                         o.ProjectName,
                         o.Task,
                         o.CreatedDate,
                         SOrderToSale = (SaleConvert != null && SOrderToSales == Status.active) ? false : true,
                         SOrderToPF = (PFAConvert != null && SOrderToPFs == Status.active) ? false : true,
                         SOrderToDvNote = (DVNConvert != null && SOrderToDvNotes == Status.active) ? false : true,
                     };
                     });            //var v = db.SalesOrders.Select(b => new
            //    b.SalesOrderId,
            //    b.SONo,
            //    b.BillNo,
            //    b.SODate,
            //    b.SOItems,
            //    b.SOItemQuantity,
            //    b.SODiscount,
            //    b.SOGrandTotal,
            //    b.SOTax,
            //    b.SOTaxAmount,
            //    validity = (DateTime.Now <= DbFunctionsCompat.AddDays(b.SODate, (b.SOValidity == null) ? 0 : b.SOValidity + 1)) ? "Active" : "Expired"
            if (appstat != "")
            {
                v = v.Where(a => a.ApprovalStatus == AppSt);
            }
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
             {
                // Apply search — invoice no, customer, project, executive, created-by
                var s = search.ToLower();
                v = v.Where(p => (p.BillNo != null && p.BillNo.ToString().ToLower().Contains(s)) ||
                                 (p.Customer != null && p.Customer.ToString().ToLower().Contains(s)) ||
                                 (p.ProjectName != null && p.ProjectName.ToString().ToLower().Contains(s)) ||
                                 (p.EmpName != null && p.EmpName.ToString().ToLower().Contains(s)) ||
                                 (p.user != null && p.user.ToString().ToLower().Contains(s)));
            }

            //SORT
            if (!fastPage && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }

            if (!fastPage) { recordsTotal = v.Count(); }
            var data = (fastPage ? v.ToList() : v.Skip(skip).Take(pageSize).ToList());
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        public JsonResult SearchSalesOrder(string q, string x, string page,long? customer)
        {

            var UserId = User.Identity.GetUserId();

            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.SalesOrders
                                  join c in db.Customers on b.Customer equals c.CustomerID into cnts
                                  from c in cnts.DefaultIfEmpty()

                                  where (
                                 (customer == null || customer == b.Customer) && b.BillNo.ToLower().Contains(q.ToLower()) || c.CustomerName.ToLower().Contains(q.ToLower())
                                   || c.CustomerName.StartsWith(q) || c.CustomerName.EndsWith(q))

                                  select new SelectFormat
                                  {
                                      text = b.BillNo + "-" + c.CustomerName,
                                      id = b.SalesOrderId
                                  }).OrderByDescending(a => a.id).Take(pageSize).ToList();

            }
            else
            {
                serialisedJson = (from b in db.SalesOrders
                                  join c in db.Customers on b.Customer equals c.CustomerID into cnts
                                  from c in cnts.DefaultIfEmpty()
                                  where (customer == null || customer == b.Customer)
                                  select new SelectFormat
                                  {
                                      text = b.BillNo + "-" + c.CustomerName,
                                      id = b.SalesOrderId
                                  }).OrderByDescending(a => a.id).Take(pageSize).ToList();
                //serialisedJson = db.Customers.Select(b => new SelectFormat
                //    text = b.CustomerCode + "-" + b.CustomerName,
                //    id = b.CustomerID

            }//
            if (x == "All" || x == "Both" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (string.IsNullOrEmpty(q) && (x == "No" || (x == "Both" && start == 0)))
            {
                var initial = new SelectFormat() { id = -2, text = "--No Sales Order--" };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);

        }

        [HttpGet]
        public ActionResult GetCustomer(int CustID)
        {
            var email = (from b in db.SalesOrders
                         join c in db.Customers on b.Customer equals c.CustomerID into cust
                         from c in cust.DefaultIfEmpty()
                         join d in db.Contacts on c.Contact equals d.ContactID into cnt
                         from d in cnt.DefaultIfEmpty()
                         where b.Customer == CustID
                         select new
                         {
                             CustomerEmail = d.EmailId,
                         }).FirstOrDefault();
            return Json(email);

        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,View SalesOrder")]
        public ActionResult Details(long? id)
        {

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            SalesOrderViewModel vmodel = new SalesOrderViewModel();
            vmodel = (from b in db.SalesOrders
                      join c in db.Customers on b.Customer equals c.CustomerID into cust
                      from c in cust.DefaultIfEmpty()
                      join d in db.Contacts on c.Contact equals d.ContactID into cnt
                      from d in cnt.DefaultIfEmpty()
                      join e in db.Employees on b.SOCashier equals e.EmployeeId into user
                      from e in user.DefaultIfEmpty()
                      join u in db.HireDetails on new { h1 = b.SalesOrderId, h2 = "Sales order" }
                      equals new { h1 = u.Reference, h2 = u.Section } into hir
                      from u in hir.DefaultIfEmpty()
                      join v in db.HireTypes on u.HireType equals v.HireTypeId into htyp
                      from v in htyp.DefaultIfEmpty()
                      join t in db.SalesTypes on b.SalesType equals t.Id into stype
                      from t in stype.DefaultIfEmpty()
                      where b.SalesOrderId == id
                      select new SalesOrderViewModel
                      {
                          CustomerName = c.CustomerCode + " - " + c.CustomerName,
                          SONo = b.SONo,
                          BillNo = b.BillNo,
                          SODate = b.SODate,
                          TermsCondition = b.TermsCondition.Replace("\n", "<br />"),
                          EmployeeName = e.FirstName + " " + e.LastName,
                          SODiscount = b.SODiscount,
                          SOGrandTotal = b.SOGrandTotal,
                          SOValidity = b.SOValidity,
                          Remarks = b.Remarks.Replace("\n", "<br />"),

                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,

                          SaleTypeName = (b.SaleType == SaleType.Sale) ? "Sale" : ((b.SaleType == SaleType.Hire) ? "Hire" : "POS"),
                          SalesTypeName = t.Name,
                          EmailId = d.EmailId,

                          HType = (u != null) ? v.Name : "",
                          StartDate = (u != null) ? u.StartDate : null,
                          EndDate = (u != null) ? u.EndDate : null,
                          Emp = (from ab in db.Approvals
                                 join bb in db.Employees on ab.EmployeeId equals bb.EmployeeId
                                 where ab.TransEntry == id && ab.Type == "SalesOrder"
                                 select new ApprovalViewModel
                                 {
                                     EmpName = bb.FirstName + " " + bb.LastName
                                 }).ToList()
                      }).FirstOrDefault();
            vmodel.SOItem = db.SalesOrderItems.Where(a => a.SalesOrder == id && a.ItemNote != "-:{Bundle_Item}")
            .Select(b => new SOItemViewModel
            {
                ItemUnitPrice = b.ItemUnitPrice,
                ItemQuantity = b.ItemQuantity,
                ItemSubTotal = b.ItemSubTotal,
                ItemTax = b.ItemTax,
                ItemNote = b.ItemNote != null ? b.ItemNote : "",
                ItemTaxAmount = b.ItemTaxAmount,
                ItemTotalAmount = b.ItemTotalAmount,
                ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault(),
                PartNumber = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.PartNumber).FirstOrDefault(),
                bundleitem = (from ab in db.SalesOrderItems
                              join bb in db.Items on ab.Item equals bb.ItemID
                              join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                              from cb in primary.DefaultIfEmpty()
                              join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                              from bd in second.DefaultIfEmpty()
                              where ab.SalesOrder == id && ab.ItemNote == "-:{Bundle_Item}"
                              && b.Item == ab.ItemDiscount
                              select new ItemDetailViewModel
                              {
                                  ItemCode = bb.ItemCode,
                                  ItemName = bb.ItemName,
                                  ItemUnit = cb.ItemUnitName,
                                  ItemQuantity = ab.ItemQuantity,
                              }).ToList()
            }).ToList();
            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "SOrder" && a.Status == Status.active).ToList();
            return View(vmodel);
        }
        private string InvoiceNo(Int64 QENo = 0, string billNo = null,string section = null)
        {
            string prefix = (section == "Hire") ? "HireSalesOrder" : "SalesOrder";
            var companyPrefix = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.number).FirstOrDefault();
            SaleType type = (section != "Hire") ? SaleType.Sale : SaleType.Hire;
            if (billNo == null)
            {
                if ((db.SalesOrders.Where(q => q.SaleType == type).Select(p => p.SONo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                 }
                else
                {
                    QENo = db.SalesOrders.Where(q => q.SaleType == type).Max(p => p.SONo + 1);
                    billNo = companyPrefix + QENo;
                    if (BillExist(billNo))
                    {
                        billNo = InvoiceNo(QENo, billNo,section);
                    }
                }
            }
            else
            {
                QENo = QENo + 1;
                billNo = companyPrefix + QENo;
                if (BillExist(billNo))
                {
                    billNo = InvoiceNo(QENo, billNo,section);
                }

            }
            return billNo;
        }
        private bool BillExist(string QENo)
        {
            var Exists = db.SalesOrders.Any(c => c.BillNo == QENo);
            bool res = (Exists) ? true : false;
            return res;
        }
        //[QkAuthorize(Roles = "Dev,Delete SalesOrder")]
        public ActionResult Delete(long? id)
        {
            //Delete
            var userpermission = User.IsInRole("All SalesOrder Entry");
            var UserId = User.Identity.GetUserId();
            SalesOrder SO = db.SalesOrders.Where(x => (x.CreatedUserId == UserId || userpermission == true) && x.SalesOrderId == id).FirstOrDefault();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (SO == null)
            {
                return NotFound();
            }
            return PartialView(SO);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Delete SalesOrder")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            #region Old Code

            #endregion
            var Msg = chkDeleteWithMsg(id);
            if (Msg != null)
            {
                msg = Msg;
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Successfully deleted Sales Order.";
            }


            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Delete SalesOrder")]
        public ActionResult DeleteAllSalesOrder(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteSOrder(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + " SalesOrder", true);
            return RedirectToAction("Index", "SalesOrder");
        }

        private Boolean DeleteSOrder(long saleId)
        {
            var Msg = chkDeleteWithMsg(saleId);
            if (Msg != null)
            {
                return false;
            }
            else
            {
                return DeleteFn(saleId);
            }
        }

        private Boolean DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            SalesOrder SOrder = db.SalesOrders.Find(id);

            var Sorder = db.SalesOrderItems.Where(a => a.SalesOrder == id).FirstOrDefault();
            if (Sorder != null)
            {
                db.SalesOrderItems.RemoveRange(db.SalesOrderItems.Where(a => a.SalesOrder == id));
            }
            var HireItem = db.HireDetails.Where(a => a.Reference == id && a.Section == "Sales order").FirstOrDefault();
            if (HireItem != null)
            {
                db.HireDetails.RemoveRange(db.HireDetails.Where(a => a.Reference == id && a.Section == "Sales order"));

            }

            var appr = db.Approvals.Where(a => a.TransEntry == id && a.Type == "SalesOrder").FirstOrDefault();
            if (appr != null)
            {
                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == id && a.Type == "SalesOrder"));
            }
            var app = db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "SalesOrder").FirstOrDefault();
            if (app != null)
            {
                db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "SalesOrder"));
            }
            db.SalesOrders.Remove(SOrder);
            db.SaveChanges();
            com.addlog(LogTypes.Deleted, UserId, "SalesOrder", "SalesOrders", findip(), SOrder.SalesOrderId, "Successfully Deleted SalesOrder");

            return true;
        }

        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            var Ext1 = db.ConvertTransactionss.Where(x => x.From == id && x.ConvertFrom == "SOrder" && x.ConvertTo== "ProForma").FirstOrDefault();
            var Ext2 = db.ConvertTransactionss.Where(x => x.From == id && x.ConvertFrom == "SOrder" && x.ConvertTo == "Sale").FirstOrDefault();
            if (Ext1 != null)
            {
                var inv = db.ProFormas.Where(x => x.ProFormaId == Ext1.To).Select(z => z.BillNo).FirstOrDefault();
                msg = "This Invoice was Converted to ProForma : " + inv + "";
            }
            else if (Ext2 != null)
            {
                var inv = db.SalesEntrys.Where(x => x.SalesEntryId == Ext2.To).Select(z => z.BillNo).FirstOrDefault();
                msg = "This Invoice was Converted to Sale : " + inv + "";
            }
            else
            {
                msg = null;
            }
            return msg;
        }

        private long GetQeNo(SaleType type)
        {
            Int64 QENo = 0;
            string prefix = (type == SaleType.Hire) ? "HireSalesOrder" : "SalesOrder";
            Int32 number = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.number).FirstOrDefault();
            if ((db.SalesOrders.Where(a => a.SaleType == type).Select(p => p.SONo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                if (number == 0)
                {
                    QENo = 1;
                }
                else
                {
                    QENo = number;
                }
            }
            else
            {
                QENo = db.SalesOrders.Where(a => a.SaleType == type).Max(p => p.SONo + 1);
            }

            return QENo;
        }
        [HttpPost]
        public ActionResult GetHireInvoiceNum(string hiretype)
        {
            string hirerate = InvoiceNo(0, null, hiretype);
            return Json(hirerate);
        }

        [HttpGet]
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "SalesOrder" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
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

            var MR = db.SalesOrders.Where(a => a.SalesOrderId == id).FirstOrDefault();

            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "SalesOrder").OrderByDescending(a => a.CreatedDate).FirstOrDefault();
       
            if ((chkappby == null) || (chkappby.ApprovalStatus != App.ApprovalStatus))
            {
                ApprovalUpdate AppUp = new ApprovalUpdate();
                AppUp.ApprovalStatus = App.ApprovalStatus;
                AppUp.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                AppUp.ApprovedBy = UserId;
                AppUp.Note = App.Note;
                AppUp.RequestBy = MR.CreatedUserId;
                AppUp.Status = Status.active;
                AppUp.TransEntry = id;
                AppUp.Type = "SalesOrder";

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
                            join d in db.SalesOrders on b.TransEntry equals d.SalesOrderId into team
                            from d in team.DefaultIfEmpty()
                            join e in db.Employees on b.RequestBy equals e.UserId into emp
                            from e in emp.DefaultIfEmpty()
                            join u in db.Users on d.CreatedUserId equals u.Id into req
                            from u in req.DefaultIfEmpty()
                            where b.TransEntry == MCId && b.Type == "SalesOrder"
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
