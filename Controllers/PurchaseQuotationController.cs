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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class PurchaseQuotationController : BaseController
    {

        ApplicationDbContext db;
        Common com;

        public PurchaseQuotationController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: PurchaseQuotation 
        [QkAuthorize(Roles = "Dev,PurchaseQuotation List")]
        public ActionResult Index()
        {
            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjCheck = ProjectCheck;

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


            ViewBag.Prjct = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = true, Text = "All", Value = null},
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
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var MlaPQuot = db.EnableSettings.Where(a => a.EnableType == "MLAPQuot").FirstOrDefault();
            var MlaPQuots = MlaPQuot != null ? MlaPQuot.Status : Status.inactive;
            ViewBag.MLAPQuot = MlaPQuots;

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindPQuot").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

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

        [QkAuthorize(Roles = "Dev,PurchaseQuotation Entry")]
        public ActionResult Create(long? id, string type)
        {
            var PQuotentry = new PurchaseQuotationViewModel

            {
                BillNo = InvoiceNo(),
                PQuotDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                TermsCondition = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "quote").Select(a => a.TermsCondit).FirstOrDefault(),
                PurchaseTypes = db.PurchaseTypes.ToList()
            };

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            if (id != null)
            {
                if (type == "MOrder")
                {
                    MaterialRequisition morder = db.MaterialRequisitions.Find(id);
                    if (morder == null)
                    {
                        return NotFound();
                    }
                    PQuotentry.ConTypeId = morder.MaterialRequisitionId;
                    PQuotentry.ConType = type;
                    PQuotentry.CMReqNo = morder.BillNo;
                    PQuotentry.PQuotDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    PQuotentry.PQuotCashier = morder.MRCashier;
                    PQuotentry.Supplier = 0;
                    PQuotentry.PQuotDiscount = 0;
                    PQuotentry.PQuotGrandTotal = 0;
                    PQuotentry.Remarks = morder.Remarks;
                    PQuotentry.SupplierEmail = "";
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        PQuotentry.Project = morder.Project;
                        var ProjectName = db.Projects.Where(a => a.ProjectId == morder.Project).Select(a => a.ProjectName).FirstOrDefault();
                        PQuotentry.ProjectName = (ProjectName != null && ProjectName != "") ? morder.Project + "-" + ProjectName : "";
                        PQuotentry.ProTask = morder.ProTask;
                    }
                    PQuotentry.convertFrom = type + " No";//label
                    PQuotentry.convertBill = morder.BillNo;
                    PQuotentry.TermsCondition = morder.TermsCondition;
                }
            }

            var supp = db.Suppliers
            .Select(s => new
            {
                SupplierID = s.SupplierID,
                SupplierDetails = s.SupplierCode + " - " + s.SupplierName
            }).ToList();
            ViewBag.Suppl = QkSelect.List(supp, "SupplierID", "SupplierDetails");

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

            ViewBag.Proj = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = true, Text = "--No Project--"},
                                }, "Value", "Text");

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            companySet();

            var userpermission = User.IsInRole("All PurchaseQuotation Entry");
            var UserId = User.Identity.GetUserId();

            ViewBag.LastEntry = db.PurchaseQuotations.Where(a => a.CreatedUserId == UserId || userpermission == true).Select(p => p.PQuotationId).AsEnumerable().DefaultIfEmpty(0).Max();

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            ViewBag.CheckMail = mail != null ? mail.Status : Status.inactive;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            
            ViewBag.Contype = type;

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                 .Select(s => new
                 {
                     ID = s.EmployeeId,
                     Name = s.FirstName + " " + s.LastName
                 })
                 .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var MlaPQuot = db.EnableSettings.Where(a => a.EnableType == "MLAPQuot").FirstOrDefault();
            var MlaPQuots = MlaPQuot != null ? MlaPQuot.Status : Status.inactive;
            ViewBag.MLAPQuot = MlaPQuots;

            var MakeIn = db.EnableSettings.Where(a => a.EnableType == "MakeInTrans").FirstOrDefault();
            var Make = MakeIn != null ? MakeIn.Status : Status.inactive;
            ViewBag.Make = Make;

            //field mapping
            PQuotentry.FieldMap = db.FieldMappings.Where(a => a.Section == "PQuot" && a.Status == Status.active).ToList();

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
            return View(PQuotentry);

        }

        [QkAuthorize(Roles = "Dev,PurchaseQuotation Entry")]
        public JsonResult CreatePurchaseQuotation(string[][] array, string[] pquotdata, string action, ICollection<QtBillSundry> bsmodel)
        {

            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                bool Exist = !(BillExist(Convert.ToString(pquotdata[15])));
                if (Exist)
                {
                    var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
                    var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

                    var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
                    var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

                    var UserId = User.Identity.GetUserId();
                    var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();

                    long Branch = 0;

                    var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                    var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                    if (BranchCheck == Status.active)
                    {
                        Branch = Convert.ToInt64(pquotdata[17]);
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    }
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                    long? Prj = null;

                    if (pquotdata[18] != null && pquotdata[18] != "")
                    {
                        Prj = Convert.ToInt64(pquotdata[18]);
                    }
                    //sales entry
                    PurchaseQuotation PQuoentry = new PurchaseQuotation();

                    PQuoentry.PQuotNo = GetQNo();
                    PQuoentry.BillNo = Convert.ToString(pquotdata[15]);
                    PQuoentry.PQuotDate = DateTime.Parse(pquotdata[2], new CultureInfo("en-GB"));
                    PQuoentry.PQuotCashier = pquotdata[1] != "" ? Convert.ToInt64(pquotdata[1]) : 0;
                    PQuoentry.Supplier = Convert.ToInt64(pquotdata[0]);

                    PQuoentry.PQuotItems = Convert.ToInt32(pquotdata[3]);
                    PQuoentry.PQuotItemQuantity = Convert.ToDecimal(pquotdata[4]);
                    PQuoentry.PQuotSubTotal = Convert.ToDecimal(pquotdata[8]);
                    PQuoentry.PQuotTax = Convert.ToDecimal(pquotdata[9]);
                    PQuoentry.PQuotTaxAmount = Convert.ToDecimal(pquotdata[5]);
                    PQuoentry.PQuotDiscount = Convert.ToDecimal(pquotdata[6]);
                    PQuoentry.PQuotGrandTotal = Convert.ToDecimal(pquotdata[7]);
                    PQuoentry.PQuotNote = "";
                    PQuoentry.Mail = 0;
                    PQuoentry.PQuotCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                    PQuoentry.CreatedUserId = UserId;
                    PQuoentry.Status = Status.active;
                    PQuoentry.TermsCondition = Convert.ToString(pquotdata[11]);
                    PQuoentry.EmailTemplateID = db.EmailTemplates.Where(a => a.Head == "PurchaseQuotation").Select(a => a.EmailTemplateID).FirstOrDefault();
                    PQuoentry.CompanyHeaderID = 0;
                    PQuoentry.Branch = Branch;
                    PQuoentry.PQuotValidity = pquotdata[10] == "" ? 0 : Convert.ToInt32(pquotdata[10]);
                    PQuoentry.Remarks = pquotdata[16];
                    PQuoentry.Project = Prj;

                    PQuoentry.PaymentTerms = (pquotdata[19]);
                    PQuoentry.PurchaseType = Convert.ToInt64(pquotdata[20]);
                    PQuoentry.ProTask = pquotdata[22] != "" ? Convert.ToInt64(pquotdata[22]) : 0;

                    PQuoentry.Ref1 = Convert.ToString(pquotdata[26]);
                    PQuoentry.Ref2 = Convert.ToString(pquotdata[27]);
                    PQuoentry.Ref3 = Convert.ToString(pquotdata[28]);
                    PQuoentry.Ref4 = Convert.ToString(pquotdata[29]);
                    PQuoentry.Ref5 = Convert.ToString(pquotdata[30]);

                    db.PurchaseQuotations.Add(PQuoentry);
                    db.SaveChanges();
                    Int64 pquotationId = 0;
                    pquotationId = PQuoentry.PQuotationId;

                    var SupplierName = db.Suppliers.Where(a => a.SupplierID == PQuoentry.Supplier).Select(a => a.SupplierCode + " - " + a.SupplierName).FirstOrDefault();
                    var MakeChk = db.EnableSettings.Where(a => a.EnableType == "MakeInTrans").FirstOrDefault();
                    var MakeChks = MakeChk != null ? MakeChk.Status : Status.inactive;


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
                    dtItem.Columns.Add("Quotation");
                    dtItem.Columns.Add("Item");
                    dtItem.Columns.Add("Make");
                    dtItem.Columns.Add("ItemNote");

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
                        dr["Quotation"] = pquotationId;
                        dr["Item"] = Convert.ToInt32(arr[0]);
                        if (MakeChks == Status.active)
                        {
                            dr["Make"] = arr[29] != null ? Convert.ToUInt64(arr[29]) : 0;
                            dr["itemNote"] = Convert.ToString(arr[30].Replace("\n", "<br />"));
                        }
                        else
                        {
                            dr["Make"] = 0;
                            dr["itemNote"] = Convert.ToString(arr[29].Replace("\n", "<br />"));
                        }
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
                                              ItemUnitPrice = a.ItemSubTotal,
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


                                decimal itemtax = 0;
                                decimal taxamt = 0;
                                decimal totamt = 0;

                                itemtax = bu.ItemTax;

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
                                dbu["Quotation"] = pquotationId;
                                dbu["Item"] = bu.Item;
                                dbu["Make"] = arr[29] != null ? Convert.ToUInt64(arr[29]) : 0;
                                dbu["itemNote"] = "-:{Bundle_Item}";
                                dtItem.Rows.Add(dbu);
                            }
                        }
                    }

                    SqlParameter parameter = new SqlParameter("@TableType", dtItem);

                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "TableTypePQuotItems";
                    //// execute sp sql 
                    string sql = String.Format("EXEC {0} {1};", "SP_InsertPurchaseQuotItems", "@TableType");
                    //// execute sql 
                    db.Database.ExecuteSqlRaw(sql, parameter);



                    if (bsmodel != null)
                    {
                        foreach (var bs in bsmodel)
                        {
                            var qtB = new PQtBillSundry
                            {
                                PQuotation = pquotationId,
                                BillSundry = bs.BillSundry,
                                BsValue = bs.BsValue,
                                AmountType = bs.AmountType,
                                BsType = bs.BsType,
                                BsAmount = bs.BsAmount,
                            };
                            db.PQtBillSundrys.Add(qtB);
                            db.SaveChanges();

                        }
                    }

                    //Approved By
                    var Appby = Convert.ToString(pquotdata[21]);
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = pquotationId;
                            approval.Type = "PurchaseQuotation";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }
                    if (pquotdata[23] != null && pquotdata[23] != "0" && pquotdata[23] != "" && pquotdata[24] != null && pquotdata[24] != "" && pquotdata[24] != "0")
                    {

                        ConvertTransactions ConTran = new ConvertTransactions();

                        ConTran.ConvertFrom = pquotdata[24];
                        ConTran.ConvertTo = "PQuote";
                        ConTran.From = Convert.ToInt64(pquotdata[23]);
                        ConTran.To = PQuoentry.PQuotationId;
                        ConTran.Status = 0;
                        ConTran.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                        ConTran.CreatedBy = UserId;
                        ConTran.Branch = Convert.ToInt32(BranchID);

                        db.ConvertTransactionss.Add(ConTran);
                        db.SaveChanges();
                        com.addlog(LogTypes.Created, UserId, "ConvertTransactions", "ConvertTransactionss", findip(), ConTran.Id, "Successfully Submitted Convertion");
                    }
                    com.addlog(LogTypes.Created, UserId, "PurchaseQuotation", "PurchaseQuotations", findip(), pquotationId, "Successfully Submitted PurchaseQuotations");

                    Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                    TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
                    //hide MR Number in print
                    var CMReqNo = "";//pquotdata[25] != "" ? Convert.ToString(pquotdata[25]) :"";

                    if (action == "print")
                    {
                        var fmapp = db.FieldMappings.Where(a => a.Section == "PQuot" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();


                        var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                        var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;


                        var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                        var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                        string qedate = PQuoentry.PQuotDate.ToString("dd-MM-yyyy");

                        var QuotesData = com.PurchaseQuotationData(pquotationId, PartNoCheck, InPrintItemCode, TimeOut, ProjectCheck, ComHeadCheck, CMReqNo);

                        var item = QuotesData.pdfItem.ToList();
                        var summary = QuotesData;
                        var billsundry = QuotesData.billsundry.ToList();

                        var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                        var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                        var def = (PriLay == Status.active) ? Convert.ToInt64(pquotdata[31]) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                        def = def == 0 ? 1 : def;
                        var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, layout, fmapp } };
                    }
                    else if (action == "SaveMail")
                    {

                        SendMail sm = new SendMail();
                        MailMessage message = new MailMessage();
                        string ToMail = pquotdata[12];
                        string CcMail = pquotdata[13];
                        string InvoiceNo = "_PurchaseQuote_" + PQuoentry.PQuotNo;

                        var em = db.EmailTemplates.Where(a => a.Head == "PurchaseQuotation").FirstOrDefault();
                        if (em != null)
                        {
                            message.Subject = em.Subject;
                            message.Body = em.EmailBody;
                        }
                        else
                        {
                            message.Subject = "Purchase Quotation";
                            message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                                " <p>we are enclosing our quotation for the items / services as requested by you during our discussions.<br/></p> " +
                                " <p>Looking forward to hear from you.</p>";
                        }
                        sm.SendPdfMail(generatePdf(pquotationId), ToMail, CcMail, InvoiceNo, message);

                        msg = "Successfully submitted PurchaseQuotation.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    else
                    {
                        msg = "Successfully submitted Purchase Quotation.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }

                }
                else
                {
                    msg = "Purchase Quotation No. Already Exists.";
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

        [QkAuthorize(Roles = "Dev,Edit PurchaseQuotation")]
        public ActionResult Edit(long? id)
        {
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

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userpermission = User.IsInRole("All PurchaseQuotation Entry");
            var UserId = User.Identity.GetUserId();

            PurchaseQuotation pquentry = db.PurchaseQuotations.Where(x => (x.CreatedUserId == UserId || userpermission == true) && x.PQuotationId == id).FirstOrDefault();

            if (pquentry == null)
            {
                return NotFound();
            }
            PurchaseQuotationViewModel vmodel = new PurchaseQuotationViewModel();

            var supp = db.Suppliers
            .Select(s => new
            {
                SupplierID = s.SupplierID,
                SupplierDetails = s.SupplierCode + " - " + s.SupplierName
            }).ToList();

            ViewBag.Suppl = QkSelect.List(supp, "SupplierID", "SupplierDetails");


            var use = db.Employees
                             .Select(s => new
                             {
                                 ID = s.EmployeeId,
                                 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                             })
                             .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjCheck = ProjectCheck;

            var pr = db.Projects
                            .Select(s => new
                            {
                                ID = s.ProjectId,
                                Name = s.ProCode + "-" + s.ProjectName
                            })
                            .ToList();
            ViewBag.getProj = QkSelect.List(pr, "ID", "Name");
            var tsk = db.ProTasks
                .Select(s => new
                {
                    ID = s.ProTaskId,
                    Name = s.TaskName
                })
                .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "PurchaseQuotation").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaPQuot = db.EnableSettings.Where(a => a.EnableType == "MLAPQuot").FirstOrDefault();
            var MlaPQuots = MlaPQuot != null ? MlaPQuot.Status : Status.inactive;
            ViewBag.MLAPQuot = MlaPQuots;

            var MRNo = "";
            var CFrom = db.ConvertTransactionss.Where(a => a.To == id && a.ConvertFrom == "MOrder" && a.ConvertTo == "PQuote").Select(a => a.From).FirstOrDefault();
            if (CFrom != null)
            {
                MRNo = db.MaterialRequisitions.Where(a => a.MaterialRequisitionId == CFrom).Select(a => a.BillNo).FirstOrDefault();
            }

            var CBill = "";
            var CType = "";
            var ConvertTran = db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "PQuote").FirstOrDefault();
            if (ConvertTran != null)
            {
                CType = ConvertTran.ConvertFrom + " No";
                if (ConvertTran.ConvertFrom == "MOrder")
                {
                    CBill = db.MaterialRequisitions.Where(a => a.MaterialRequisitionId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
            }

            vmodel = (from b in db.PurchaseQuotations
                      where b.PQuotationId == id
                      select new PurchaseQuotationViewModel
                      {
                          PQuotNo = b.PQuotNo,
                          Supplier = b.Supplier,
                          PQuotDate = b.PQuotDate,
                          BillNo = b.BillNo,
                          PQuotCashier = b.PQuotCashier,
                          PQuotDiscount = b.PQuotDiscount,
                          PQuotGrandTotal = b.PQuotGrandTotal,
                          TermsCondition = b.TermsCondition,
                          PQuotValidity = b.PQuotValidity,
                          Remarks = b.Remarks,
                          Branch = b.Branch,
                          Project = b.Project != null ? b.Project : null,
                          PurchaseType = b.PurchaseType,
                          PurchaseTypes = db.PurchaseTypes.ToList(),
                          PaymentTerms = b.PaymentTerms,
                          ProTask = b.ProTask,
                          CMReqNo= MRNo,
                          convertBill = CBill,
                          convertFrom = CType,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          //MRNo = b.MRNo
                      }).FirstOrDefault();


            companySet();

            ViewBag.preEntry = db.PurchaseQuotations.Where(a => a.PQuotationId < id && (a.CreatedUserId == UserId)).Select(a => a.PQuotationId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.PurchaseQuotations.Where(a => a.PQuotationId > id && (a.CreatedUserId == UserId)).Select(a => a.PQuotationId).DefaultIfEmpty().Min();

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var MakeIn = db.EnableSettings.Where(a => a.EnableType == "MakeInTrans").FirstOrDefault();
            var Make = MakeIn != null ? MakeIn.Status : Status.inactive;
            ViewBag.Make = Make;

            var EditPermission = User.IsInRole("Disable PQuot Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "PurchaseQuotation", UserId);

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "PQuot" && a.Status == Status.active).ToList();

            //dummy table operations
            var DItem = db.DummyPQuotationItems.Where(a => a.PQuotation == id).FirstOrDefault();
            var SItem = db.PurchaseQuotationItems.Where(a => a.PQuotation == id).FirstOrDefault();
            if (SItem == null && DItem != null)
            {
                var DItems = db.DummyPQuotationItems.Where(a => a.PQuotation == id).ToList();
                foreach (var arr in DItems)
                {
                    //add to se-item table
                    PurchaseQuotationItem sItem = new PurchaseQuotationItem();
                    sItem.ItemUnit = arr.ItemUnit;
                    sItem.ItemUnitPrice = arr.ItemUnitPrice;
                    sItem.ItemQuantity = arr.ItemQuantity;
                    sItem.ItemSubTotal = arr.ItemSubTotal;
                    sItem.ItemDiscount = arr.ItemDiscount;
                    sItem.ItemTax = arr.ItemTax;
                    sItem.ItemTaxAmount = arr.ItemTaxAmount;
                    sItem.ItemTotalAmount = arr.ItemTotalAmount;
                    sItem.ItemNote = arr.ItemNote;
                    sItem.PQuotation = arr.PQuotation;
                    sItem.Item = arr.Item;
                    sItem.Make = arr.Make;
                    db.PurchaseQuotationItems.Add(sItem);
                    db.SaveChanges();
                }

                db.DummyPQuotationItems.RemoveRange(db.DummyPQuotationItems.Where(a => a.PQuotation == id));
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
        public ActionResult GetSupplier(int SupID)
        {
            var email = (from b in db.PurchaseQuotations
                         join c in db.Suppliers on b.Supplier equals c.SupplierID into Sup
                         from c in Sup.DefaultIfEmpty()
                         join d in db.Contacts on c.Contact equals d.ContactID into cnt
                         from d in cnt.DefaultIfEmpty()
                         where b.Supplier == SupID
                         select new
                         {
                             SupplierEmail = d.EmailId,
                         }).FirstOrDefault();
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(email);
            return Json(email);

        }

        [HttpGet]
        public ActionResult GetPQEItems(long PQuoteEntryID)
        {
            var ConD = (from a in db.PurchaseQuotationItems
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join g in db.PurchaseQuotations on a.PQuotation equals g.PQuotationId into PQ
                        from g in PQ.DefaultIfEmpty()
                        join h in db.ItemBrands on a.Make equals h.ItemBrandID into brn
                        from h in brn.DefaultIfEmpty()
                        join p in db.Projects on g.Project equals p.ProjectId into proj
                        from p in proj.DefaultIfEmpty()
                        join t in db.ProTasks on g.ProTask equals t.ProTaskId into protask
                        from t in protask.DefaultIfEmpty()

                        where a.PQuotation == PQuoteEntryID && a.ItemNote != "-:{Bundle_Item}"
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
                            b.MRP,

                            ProjectId = g.Project,
                            TaskId = g.ProTask,
                            p.ProjectName,
                            t.TaskName,
                            ItemMake=h!=null? h.ItemBrandID:0,
                            ItemMakeName= h != null ? h.ItemBrandName:""
                        }).ToList();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(ConD);
            return Json(result);
        }

        [HttpGet]
        public JsonResult GetPQtBillSundry(long quoteID)
        {
            var QtBs = (from a in db.PQtBillSundrys
                        join c in db.BillSundrys on a.BillSundry equals c.BillSundryId
                        where a.PQuotation == quoteID
                        select new
                        {
                            a.AmountType,
                            a.BillSundry,
                            a.BsAmount,
                            a.BsType,
                            a.BsValue,
                            c.BSName,
                            //a.PEBillSundryId,
                            //a.PurchaseEntry,
                            //c.BillSundryId
                        }).ToList();
            return Json(QtBs);
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev, PurchaseQuotation List")]
        public ActionResult GetPurchaseQuotation(string BillNo, string FromDate, string ToDate, long? supplier, long? salesperson, long? project, string Stats, string user, int? Validity, string appstat, long? ProjectName, long? Task)
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

            var userpermission = User.IsInRole("All PurchaseQuotation Entry");
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

            var fromv = "PQuote";
            var ToPO = "POrder";
            var PQuotToPOrder = db.EnableSettings.Where(a => a.EnableType == "PQuotToPOrder").FirstOrDefault();
            var PQuotToPOrders = PQuotToPOrder != null ? PQuotToPOrder.Status : Status.inactive;

            var serverQuery = (from b in db.PurchaseQuotations
                     join c in db.Suppliers on b.Supplier equals c.SupplierID into supp
                     from c in supp.DefaultIfEmpty()
                     join e in db.Employees on b.PQuotCashier equals e.EmployeeId into emp
                     from e in emp.DefaultIfEmpty()
                     join f in db.Projects on b.Project equals f.ProjectId into prj
                     from f in prj.DefaultIfEmpty()
                     join h in db.Users on b.CreatedUserId equals h.Id
                     join k in db.ProTasks on b.ProTask equals k.ProTaskId into task
                     from k in task.DefaultIfEmpty()

                     let po = db.ConvertTransactionss.Where(ap => ap.From == b.PQuotationId && ap.ConvertFrom == fromv && ap.ConvertTo == ToPO).FirstOrDefault()
                         // app/AppStatus/chkAppStatus (nested collections + GroupBy-latest) are computed
                         // client-side after materialization — EF Core 10 can't translate them inside this query.

                     let CFrom = db.ConvertTransactionss.Where(a => a.To == b.PQuotationId && a.ConvertFrom == "MOrder" && a.ConvertTo == "PQuote").Select(a => a.From).FirstOrDefault()

                     where(BillNo == null || BillNo == "" || b.BillNo == BillNo) &&
                     (supplier == 0 || supplier == null || b.Supplier == supplier) && (project == 0 || project == null || b.Project == project) &&
                     (salesperson == 0 || salesperson == null || e.EmployeeId == salesperson) &&
                     (FromDate == "" || FromDate == null || EF.Functions.DateDiffDay(b.PQuotDate, fdate) <= 0) &&
                     (ToDate == "" || FromDate == null || EF.Functions.DateDiffDay(b.PQuotDate, tdate) >= 0)
                     && (Stats == null || b.Status == st) && (user == null || user == "" || h.Id == user)
                     && (Validity == null || Validity == 0 || b.PQuotValidity == Validity)
                     && (userpermission == true || b.CreatedUserId == UserId)
                     && (ProjectName == 0 || ProjectName == null || f.ProjectId == ProjectName)
                     && (Task == 0 || Task == null || k.ProTaskId == Task)

                     select new
                     {
                         POConvert = (po != null) ? po.ConvertTo : "",

                         b.PQuotationId,
                         b.PQuotNo,
                         b.BillNo,
                         b.PQuotDate,
                         b.PQuotItems,
                         b.PQuotDiscount,
                         b.PQuotGrandTotal,
                         b.PQuotTax,
                         b.PQuotTaxAmount,
                         b.PQuotItemQuantity,
                         b.PQuotValidity,
                         b.Remarks,
                         MRNo = CFrom!=null? db.MaterialRequisitions.Where(a => a.MaterialRequisitionId == CFrom).Select(a => a.BillNo).FirstOrDefault():"",
                         b.Project,
                         ProjectName = (f.ProjectName != null && f.ProjectName != "") ? f.ProCode + "-" + f.ProjectName : "",
                         EmpName = e.FirstName + " " + e.LastName,
                         Supplier = c.SupplierCode + " - " + c.SupplierName,
                         user = h.UserName,
                         validity = (DateTime.Now <= b.PQuotDate.AddDays((b.PQuotValidity == null) ? 0 : b.PQuotValidity.Value + 1)) ? "Active" : "Expired",
                         Task = (k.TaskName != null && k.TaskName != "") ? k.TaskCode + "-" + k.TaskName : "",
                         CreatedDate=b.PQuotCreatedDate
                     });

            // Performance (audit P2, hybrid): server paging when no search, plain-column sort, AND no client-side
            // filter is active (the conditions below mirror the filters' own guards); else original path.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase){ "BillNo","CreatedDate","EmpName","MRNo","POConvert","PQuotationId","PQuotDate","PQuotDiscount","PQuotGrandTotal","PQuotItemQuantity","PQuotItems","PQuotNo","PQuotTax","PQuotTaxAmount","PQuotValidity","Project","ProjectName","Remarks","Supplier","Task","user","validity" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0 && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn)) && !(appstat != "");
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn) ? serverQuery.OrderBy("PQuotationId asc") : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

            // CLIENT-side lookups keyed by PQuotationId (missing key -> empty/absent, no KeyNotFound).
            var pqIds = serverRows.Select(o => o.PQuotationId).ToList();
            // app = approver EmployeeIds for the purchase quotation (nested collection, keyed by TransEntry == PQuotationId).
            var appLookup = db.Approvals
                .Where(a => a.Type == "PurchaseQuotation" && pqIds.Contains(a.TransEntry))
                .Select(a => new { a.TransEntry, a.EmployeeId })
                .ToList()
                .ToLookup(a => a.TransEntry);
            // AppStatus = all ApprovalUpdate statuses; raw rows materialized once and reused for chkAppStatus.
            var appUpdRows = db.ApprovalUpdates
                .Where(a => a.Type == "PurchaseQuotation" && pqIds.Contains(a.TransEntry))
                .Select(a => new { a.TransEntry, a.ApprovalStatus, a.ApprovedBy, a.CreatedDate })
                .ToList();
            var appStatusLookup = appUpdRows.ToLookup(a => a.TransEntry);
            // chkAppStatus = latest ApprovalUpdate status per ApprovedBy (GroupBy-latest-per-group), per purchase quotation.
            var chkAppStatusLookup = appUpdRows
                .GroupBy(a => a.TransEntry)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(l => l.ApprovedBy)
                    .Select(grp => grp.OrderByDescending(c => c.CreatedDate).First())
                    .Select(a => a.ApprovalStatus).ToList());

            var v = serverRows.Select(o =>
                     {
                         var app = appLookup[o.PQuotationId].Select(x => x.EmployeeId).ToList();
                         var AppStatus = appStatusLookup[o.PQuotationId].Select(x => x.ApprovalStatus).ToList();
                         var chkAppStatus = chkAppStatusLookup.TryGetValue(o.PQuotationId, out var ck) ? ck : new List<ApprovalStatus>();
                         return new
                     {
                         o.POConvert,
                         o.PQuotationId,
                         o.PQuotNo,
                         o.BillNo,
                         o.PQuotDate,
                         o.PQuotItems,
                         o.PQuotDiscount,
                         o.PQuotGrandTotal,
                         o.PQuotTax,
                         o.PQuotTaxAmount,
                         o.PQuotItemQuantity,
                         o.PQuotValidity,
                         o.Remarks,
                         o.MRNo,
                         o.Project,
                         o.ProjectName,
                         o.Task,
                         o.EmpName,
                         o.Supplier,
                         o.user ,
                         o.validity,
                         app = app,
                         Approval = (app != null && empl.EmployeeId != null) ? (app.Contains(empl.EmployeeId) ? true : false) : false,
                         ApprovalStatus = (app.Count > 0 && AppStatus.Count > 0 && chkAppStatus.Count > 0) ? (chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (app != null && chkAppStatus != null && app.Count != 0 && chkAppStatus.Count != 0 && chkAppStatus.Count() == app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                         o.CreatedDate,
                         PQuotToPOrder = (o.POConvert != "" && PQuotToPOrders == Status.active) ? false : true,

                         };
                     });

            if (appstat != "")
            {
                v = v.Where(a => a.ApprovalStatus == AppSt);
            }

            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.BillNo.ToString().ToLower().Contains(search.ToLower())

               );
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

        [QkAuthorize(Roles = "Dev,Edit PurchaseQuotation")]
        public JsonResult UpdatePurchaseQuotation(string[][] array, string[] pquotdata, string action, ICollection<QtBillSundry> bsmodel)
        {

            bool stat = false;
            string msg;
            Int64 pquotEntryId = Convert.ToInt64(pquotdata[16]);
            PurchaseQuotation PQuoentry = db.PurchaseQuotations.Find(pquotEntryId);

            if (BillExist(Convert.ToString(pquotdata[15])) && Convert.ToString(pquotdata[15]) != PQuoentry.BillNo)
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

                long Branch = 0;

                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                if (BranchCheck == Status.active)
                {
                    Branch = Convert.ToInt64(pquotdata[18]);
                }
                else
                {
                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                }
                long? Prj = null;

                if (pquotdata[19] != null && pquotdata[19] != "")
                {
                    Prj = Convert.ToInt64(pquotdata[19]);
                }


                var EditPermission = User.IsInRole("Disable PQuot Edit After Approval");
                if (com.chkApproved(pquotEntryId, EditPermission, "PurchaseQuotation", UserId) == true)
                {

                    PQuoentry.BillNo = Convert.ToString(pquotdata[15]);
                    PQuoentry.PQuotDate = DateTime.Parse(pquotdata[2], new CultureInfo("en-GB"));
                    PQuoentry.PQuotCashier = pquotdata[1] != "" ? Convert.ToInt64(pquotdata[1]) : 0;
                    PQuoentry.Supplier = Convert.ToInt64(pquotdata[0]);

                    PQuoentry.PQuotItems = Convert.ToInt32(pquotdata[3]);
                    PQuoentry.PQuotItemQuantity = Convert.ToDecimal(pquotdata[4]);
                    PQuoentry.PQuotSubTotal = Convert.ToDecimal(pquotdata[8]);
                    PQuoentry.PQuotTax = Convert.ToDecimal(pquotdata[9]);
                    PQuoentry.PQuotTaxAmount = Convert.ToDecimal(pquotdata[5]);
                    PQuoentry.PQuotDiscount = Convert.ToDecimal(pquotdata[6]);
                    PQuoentry.PQuotGrandTotal = Convert.ToDecimal(pquotdata[7]);
                    PQuoentry.PQuotNote = "";
                    PQuoentry.Mail = 0;
                    PQuoentry.Status = Status.active;
                    PQuoentry.TermsCondition = Convert.ToString(pquotdata[11]);
                    PQuoentry.EmailTemplateID = db.EmailTemplates.Where(a => a.Head == "PurchaseQuotation").Select(a => a.EmailTemplateID).FirstOrDefault();
                    PQuoentry.CompanyHeaderID = 0;
                    PQuoentry.Branch = Branch;
                    PQuoentry.PQuotValidity = pquotdata[10] == "" ? 0 : Convert.ToInt32(pquotdata[10]);
                    PQuoentry.Remarks = pquotdata[17];
                    PQuoentry.Project = Prj;

                    PQuoentry.PaymentTerms = (pquotdata[20]);
                    PQuoentry.PurchaseType = Convert.ToInt64(pquotdata[21]);
                    db.Entry(PQuoentry).State = EntityState.Modified;
                    PQuoentry.ProTask = pquotdata[23] != "" ? Convert.ToInt64(pquotdata[23]) : 0;

                    PQuoentry.Ref1 = Convert.ToString(pquotdata[25]);
                    PQuoentry.Ref2 = Convert.ToString(pquotdata[26]);
                    PQuoentry.Ref3 = Convert.ToString(pquotdata[27]);
                    PQuoentry.Ref4 = Convert.ToString(pquotdata[28]);
                    PQuoentry.Ref5 = Convert.ToString(pquotdata[29]);

                    db.Entry(PQuoentry).State = EntityState.Modified;
                    db.SaveChanges();

                    Int64 pquotationId = 0;
                    pquotationId = PQuoentry.PQuotationId;

                    var SupplierName = db.Suppliers.Where(a => a.SupplierID == PQuoentry.Supplier).Select(a => a.SupplierCode + " - " + a.SupplierName).FirstOrDefault();
                    var MakeChk = db.EnableSettings.Where(a => a.EnableType == "MakeInTrans").FirstOrDefault();
                    var MakeChks = MakeChk != null ? MakeChk.Status : Status.inactive;

                    var PQEItem = db.PurchaseQuotationItems.Where(a => a.PQuotation == pquotEntryId).FirstOrDefault();
                    if (PQEItem != null)
                    {
                        var SItems = db.PurchaseQuotationItems.Where(a => a.PQuotation == pquotEntryId).ToList();
                        foreach (var arr in SItems)
                        {
                            //add to dummy table
                            DummyPQuotationItem dItem = new DummyPQuotationItem();
                            dItem.ItemUnit = arr.ItemUnit;
                            dItem.ItemUnitPrice = arr.ItemUnitPrice;
                            dItem.ItemQuantity = arr.ItemQuantity;
                            dItem.ItemSubTotal = arr.ItemSubTotal;
                            dItem.ItemDiscount = arr.ItemDiscount;
                            dItem.ItemTax = arr.ItemTax;
                            dItem.ItemTaxAmount = arr.ItemTaxAmount;
                            dItem.ItemTotalAmount = arr.ItemTotalAmount;
                            dItem.ItemNote = arr.ItemNote;
                            dItem.PQuotation = arr.PQuotation;
                            dItem.Item = arr.Item;
                            dItem.Make = arr.Make;
                            db.DummyPQuotationItems.Add(dItem);
                            db.SaveChanges();
                        }

                        db.PurchaseQuotationItems.RemoveRange(db.PurchaseQuotationItems.Where(a => a.PQuotation == pquotEntryId));
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
                    dtItem.Columns.Add("Quotation");
                    dtItem.Columns.Add("Item");
                    dtItem.Columns.Add("Make");
                    dtItem.Columns.Add("ItemNote");


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
                        dr["Quotation"] = pquotationId;
                        dr["Item"] = Convert.ToInt32(arr[0]);
                        if (MakeChks == Status.active)
                        {
                            dr["Make"] = arr[29] != null ? Convert.ToUInt64(arr[29]) : 0;
                            dr["itemNote"] = Convert.ToString(arr[30].Replace("\n", "<br />"));
                        }
                        else
                        {
                            dr["Make"] = 0;
                            dr["itemNote"] = Convert.ToString(arr[29].Replace("\n", "<br />"));
                        }
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
                                              ItemUnitPrice = a.ItemSubTotal,
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


                                decimal itemtax = 0;
                                decimal taxamt = 0;
                                decimal totamt = 0;

                                itemtax = bu.ItemTax;



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
                                dbu["Quotation"] = pquotationId;
                                dbu["Item"] = bu.Item;
                                dbu["Make"] = arr[29] != null ? Convert.ToUInt64(arr[29]) : 0;
                                dbu["itemNote"] = "-:{Bundle_Item}";
                                dtItem.Rows.Add(dbu);
                            }
                        }
                    }

                    SqlParameter parameter = new SqlParameter("@TableType", dtItem);

                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "TableTypePQuotItems";
                    //// execute sp sql 
                    string sql = String.Format("EXEC {0} {1};", "SP_InsertPurchaseQuotItems", "@TableType");
                    //// execute sql 
                    var ret = db.Database.ExecuteSqlRaw(sql, parameter);
                    if (ret > 0)
                    {
                        db.DummyPQuotationItems.RemoveRange(db.DummyPQuotationItems.Where(a => a.PQuotation == pquotEntryId));
                        db.SaveChanges();
                    }

                    var PQtBs = db.PQtBillSundrys.Where(a => a.PQuotation == pquotEntryId).FirstOrDefault();
                    if (PQtBs != null)
                    {
                        db.PQtBillSundrys.RemoveRange(db.PQtBillSundrys.Where(a => a.PQuotation == pquotEntryId));
                        db.SaveChanges();
                    }


                    if (bsmodel != null)
                    {
                        foreach (var bs in bsmodel)
                        {
                            var qtB = new PQtBillSundry
                            {
                                PQuotation = pquotationId,
                                BillSundry = bs.BillSundry,
                                BsValue = bs.BsValue,
                                AmountType = bs.AmountType,
                                BsType = bs.BsType,
                                BsAmount = bs.BsAmount,
                            };
                            db.PQtBillSundrys.Add(qtB);
                            db.SaveChanges();

                        }
                    }

                    //Approved By
                    var empuser = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                    var chkapp = db.Approvals.Where(a => a.EmployeeId == empuser && a.TransEntry == pquotationId && a.Type == "PurchaseQuotation").FirstOrDefault();
                    var MrnPO = db.Approvals.Where(a => a.TransEntry == pquotationId && a.Type == "PurchaseQuotation").FirstOrDefault();
                    if (MrnPO != null)
                    {

                        if (chkapp != null)
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.EmployeeId != empuser && a.TransEntry == pquotationId && a.Type == "PurchaseQuotation"));
                            db.SaveChanges();
                        }
                        else
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == pquotationId && a.Type == "PurchaseQuotation"));
                            db.SaveChanges();
                        }
                    }
                    var Appby = Convert.ToString(pquotdata[22]);
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = pquotationId;
                            approval.Type = "PurchaseQuotation";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }

                    com.addlog(LogTypes.Created, UserId, "PurchaseQuotation", "PurchaseQuotations", findip(), pquotationId, "Successfully Submitted PurchaseQuotations");
                }
                Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

                if (action == "print")
                {
                    var fmapp = db.FieldMappings.Where(a => a.Section == "PQuot" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                    var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                    var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                    string qedate = PQuoentry.PQuotDate.ToString("dd-MM-yyyy");


                    var QuotesData = com.PurchaseQuotationData(pquotEntryId, PartNoCheck, InPrintItemCode, TimeOut, ProjectCheck, ComHeadCheck);
                    var item = QuotesData.pdfItem.ToList();
                    var summary = QuotesData;
                    var billsundry = QuotesData.billsundry.ToList();

                    var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                    var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                    var def = (PriLay == Status.active) ? Convert.ToInt64(pquotdata[30]) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                    def = def == 0 ? 1 : def;
                    var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, layout, fmapp } };
                }
                else if (action == "SaveMail")
                {

                    SendMail sm = new SendMail();
                    MailMessage message = new MailMessage();
                    string ToMail = pquotdata[12];
                    string CcMail = pquotdata[13];
                    string InvoiceNo = "_PQuote_" + PQuoentry.PQuotNo;

                    var em = db.EmailTemplates.Where(a => a.Head == "PurchaseQuotation").FirstOrDefault();
                    if (em != null)
                    {
                        message.Subject = em.Subject;
                        message.Body = em.EmailBody;
                    }
                    else
                    {
                        message.Subject = "Purchase Quotation";
                        message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                            " <p>we are enclosing our quotation for the items / services as requested by you during our discussions.<br/></p> " +
                            " <p>Looking forward to hear from you.</p>";
                    }

                    sm.SendPdfMail(generatePdf(pquotEntryId), ToMail, CcMail, InvoiceNo, message);

                    msg = "Successfully Updated PurchaseQuotation.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {
                    msg = "Successfully Updated PurchaseQuotation.";
                    stat = true;
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

        [QkAuthorize(Roles = "Dev,Delete PurchaseQuotation")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Delete
            var userpermission = User.IsInRole("All Quotation Entry");
            var UserId = User.Identity.GetUserId();
            PurchaseQuotation PQuot = db.PurchaseQuotations.Where(x => (x.CreatedUserId == UserId || userpermission == true) && x.PQuotationId == id).FirstOrDefault();


            if (PQuot == null)
            {
                return NotFound();
            }
            return PartialView(PQuot);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete PurchaseQuotation")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            var Msg = chkDeleteWithMsg(id);
            if (Msg != null)
            {
                msg = Msg;
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Successfully deleted purchase quotation details.";
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete PurchaseQuotation")]
        public ActionResult DeleteAllPurchaseQuotation(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeletePQuot(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + "PurchaseQuotation", true);
            return RedirectToAction("Index", "PurchaseQuotation");
        }

        private Boolean DeletePQuot(long id)
        {
            var Msg = chkDeleteWithMsg(id);
            bool res = (Msg != null) ? false : DeleteFn(id);
            return res;
        }
        public bool DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            PurchaseQuotation QSum = db.PurchaseQuotations.Find(id);

            var PQPro = db.PurchaseQuotationItems.Where(a => a.PQuotation == id).FirstOrDefault();
            if (PQPro != null)
            {
                db.PurchaseQuotationItems.RemoveRange(db.PurchaseQuotationItems.Where(a => a.PQuotation == id));
            }
            var qtBs = db.PQtBillSundrys.Where(a => a.PQuotation == id).FirstOrDefault();
            if (qtBs != null)
            {
                db.PQtBillSundrys.RemoveRange(db.PQtBillSundrys.Where(a => a.PQuotation == id));

            }

            var appr = db.Approvals.Where(a => a.TransEntry == id && a.Type == "PurchaseQuotation").FirstOrDefault();
            if (appr != null)
            {
                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == id && a.Type == "PurchaseQuotation"));
            }
            var app = db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "PurchaseQuotation").FirstOrDefault();
            if (app != null)
            {
                db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "PurchaseQuotation"));
            }
            var CPQuote = db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "PQuote").FirstOrDefault();
            if (CPQuote != null)
            {
                db.ConvertTransactionss.RemoveRange(db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "PQuote"));
            }

            db.PurchaseQuotations.Remove(QSum);
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "PurchaseQuotation", "PurchaseQuotations", findip(), QSum.PQuotationId, "Successfully Deleted Purchase Quotations");
            return true;
        }

        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            var Ext1 = db.ConvertTransactionss.Where(x => x.From == id && x.ConvertFrom == "PQuote" && x.ConvertTo == "Purchase").FirstOrDefault();
            var Ext2 = db.ConvertTransactionss.Where(x => x.From == id && x.ConvertFrom == "PQuote" && x.ConvertTo == "POrder").FirstOrDefault();
            if (Ext1 != null)
            {
                var inv = db.PurchaseEntrys.Where(x => x.PurchaseEntryId == Ext1.To).Select(z => z.BillNo).FirstOrDefault();
                msg = "This Invoice was Converted to Purchase Entry : " + inv + ".";
            }
            else if (Ext2 != null)
            {
                var inv = db.PurchaseOrders.Where(x => x.PurchaseOrderId == Ext2.To).Select(z => z.BillNo).FirstOrDefault();
                msg = "This Invoice was Converted to Purchase Order : " + inv + ".";
            }

            else
            {
                msg = null;
            }

            return msg;
        }


        [HttpGet]
        [QkAuthorize(Roles = "Dev,View PurchaseQuotation")]
        public ActionResult Details(long? id)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjCheck = ProjectCheck;

            var MakeIn = db.EnableSettings.Where(a => a.EnableType == "MakeInTrans").FirstOrDefault();
            var Make = MakeIn != null ? MakeIn.Status : Status.inactive;
            ViewBag.Make = Make;

            PurchaseQuotationViewModel vmodel = new PurchaseQuotationViewModel();
            vmodel = (from b in db.PurchaseQuotations
                      join c in db.Suppliers on b.Supplier equals c.SupplierID into supp
                      from c in supp.DefaultIfEmpty()
                      join d in db.Contacts on c.Contact equals d.ContactID into cnt
                      from d in cnt.DefaultIfEmpty()
                      join e in db.Employees on b.PQuotCashier equals e.EmployeeId into user
                      from e in user.DefaultIfEmpty()
                      join f in db.Projects on b.Project equals f.ProjectId into prjct
                      from f in prjct.DefaultIfEmpty()
                      where b.PQuotationId == id
                      select new PurchaseQuotationViewModel
                      {
                          SupplierName = c.SupplierCode + " - " + c.SupplierName,
                          PQuotNo = b.PQuotNo,
                          BillNo = b.BillNo,
                          PQuotDate = b.PQuotDate,
                          TermsCondition = b.TermsCondition.Replace("\n", "<br />"),
                          EmployeeName = e.FirstName + " " + e.LastName,
                          PQuotDiscount = b.PQuotDiscount,
                          PQuotGrandTotal = b.PQuotGrandTotal,
                          PQuotValidity = b.PQuotValidity,
                          PQuotSubTotal = b.PQuotSubTotal,
                          Remarks = b.Remarks.Replace("\n", "<br />"),
                          ProjectName = (f.ProjectName != null && f.ProjectName != "") ? f.ProCode + "-" + f.ProjectName : "",
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          PayTerms=b.PaymentTerms,

                          Emp = (from ab in db.Approvals
                                 join bb in db.Employees on ab.EmployeeId equals bb.EmployeeId
                                 where ab.TransEntry == id && ab.Type == "PurchaseQuotation"
                                 select new ApprovalViewModel
                                 {
                                     EmpName = bb.FirstName + " " + bb.LastName
                                 }).ToList()

                      }).FirstOrDefault();
            vmodel.PQuotItem = db.PurchaseQuotationItems.Where(a => a.PQuotation == id && a.ItemNote != "-:{Bundle_Item}")
            .Select(b => new PQuotItemViewModel
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
                MakeName = db.ItemBrands.Where(a=>a.ItemBrandID==b.Make).Select(a=>a.ItemBrandName).FirstOrDefault(),
                bundleitem = (from ab in db.QuotationItems
                              join bb in db.Items on ab.Item equals bb.ItemID
                              join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                              from cb in primary.DefaultIfEmpty()
                              join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                              from bd in second.DefaultIfEmpty()
                              where ab.Quotation == id && ab.ItemNote == "-:{Bundle_Item}"
                              && b.Item == ab.ItemDiscount
                              select new ItemDetailViewModel
                              {
                                  ItemCode = bb.ItemCode,
                                  ItemName = bb.ItemName,
                                  ItemUnit = cb.ItemUnitName,
                                  ItemQuantity = ab.ItemQuantity,
                              }).ToList()
            }).ToList();
            vmodel.PQtBillSundry = db.QtBillSundrys.Where(a => a.Quotation == id)
                .Select(b => new PQtBillSundryViewModel
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
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "PQuot" && a.Status == Status.active).ToList();

            return PartialView(vmodel);
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Download PurchaseQuotation")]
        public ActionResult Download(long id)
        {
            var Data = db.PurchaseQuotations.Where(s => s.PQuotationId == id).FirstOrDefault();
            var suppname = db.Suppliers.Where(s => s.SupplierID == Data.Supplier).Select(a => a.SupplierName).FirstOrDefault();

            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = Data.BillNo;

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), "inactive");
            return File(ms, "application/pdf", "PurchaseQuotation" + "-" + suppname + "-" + billno + ".pdf");
        }
        public StringBuilder generatePdf(long pquotationId)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

            var QuotesData = com.PurchaseQuotationData(pquotationId, PartNoCheck, InPrintItemCode, TimeOut, ProjectCheck);
            var item = QuotesData.pdfItem.ToList();
            var summary = QuotesData;
            var billsundry = QuotesData.billsundry.ToList();


            return com.generatepdf(pquotationId, summary, item, billsundry, "Purchase");
        }



        //                   where b.PQuotationId == pquotationId // b.Customer == customer
        //                       BillNo = b.BillNo,
        //                       PQuotNo = b.PQuotNo,
        //                       Date = b.PQuotDate,
        //                       PQuotValidity = b.PQuotValidity,
        //                       PQuotGrandTotal = b.PQuotGrandTotal,
        //                       PartyName = c.SupplierName,
        //                       CustomerEmail = d.EmailId,
        //                       Address = d.Address,
        //                       City = d.City,
        //                       SubTotal = b.PQuotSubTotal,
        //                       Discount = b.PQuotDiscount,
        //                       tc = b.PQuotNote,
        //                       TaxAmount = b.PQuotTaxAmount,
        //                       State = d.State,
        //                       Country = d.Country,
        //                       Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
        //                       GrandTotal = b.PQuotGrandTotal,
        //                       TRN = c.TaxRegNo,
        //                       Email = d.EmailId,
        //                       Zip = d.Zip,
        //                       Phone = d.Phone,
        //                       Mobile = d.Mobile,
        //                       b.TermsCondition,
        //                       b.Remarks,
        //                       PrjNameCode = f.ProCode + "-" + f.ProjectName,
        //                       f.ProjectName,
        //                       f.ProCode,


        //                      where b.PQuotation == pquotationId && b.ItemNote != "-:{Bundle_Item}"
        //                          ItemUnitPrice = b.ItemUnitPrice,
        //                          ItemQuantity = b.ItemQuantity,
        //                          ItemSubTotal = b.ItemSubTotal,
        //                          ItemNote = b.ItemNote,
        //                          ItemTax = b.ItemTax,
        //                          ItemTaxAmount = b.ItemTaxAmount,
        //                          ItemTotalAmount = b.ItemTotalAmount,
        //                          ItemID = b.Item,
        //                          bundleitem = (from ab in db.PurchaseQuotationItems
        //                                        where ab.PQuotation == pquotationId && ab.ItemNote == "-:{Bundle_Item}"
        //                                        && b.Item == ab.ItemDiscount

        //                                            bb.ItemCode,
        //                                            bb.ItemName,
        //                                            cb.ItemUnitName,
        //                                            ItemUnitPrice = ab.ItemUnitPrice,
        //                                            quantity = ab.ItemQuantity,
        //                                            ItemSubTotal = ab.ItemSubTotal,
        //                                            ItemTax = ab.ItemTax,
        //                                            ItemTaxAmount = ab.ItemTaxAmount,
        //                                            ItemTotalAmount = ab.ItemTotalAmount,

        //                                            ab.Item,
        //                                            ab.ItemQuantity,
        //                                            ab.ItemUnit,

        //                                            ItemDiscount = 0,

        //                                            ItemNote = ab.ItemNote,
        //                                            ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
        //                                            bb.ItemUnitID,
        //                                            bb.SubUnitId,
        //                                            PriUnit = cb.ItemUnitName,
        //                                            SubUnit = bd.ItemUnitName,
        //                                            bb.ItemArabic
        //                                        }).ToList()


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







        private long GetQNo()
        {
            Int64 QNo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "PurchaseQuotation").Select(a => a.number).FirstOrDefault();
            if ((db.PurchaseQuotations.Select(p => p.PQuotNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                QNo = (number == 0) ? 1 : number;
            }
            else
            {
                QNo = db.PurchaseQuotations.Max(p => p.PQuotNo + 1);
            }
            return QNo;
        }
        private string InvoiceNo(Int64 PQENo = 0, string billNo = null, string section = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "PurchaseQuotation").Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == "PurchaseQuotation").Select(a => a.number).FirstOrDefault();
            if (billNo == null)
            {
                if ((db.PurchaseQuotations.Select(p => p.PQuotNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                    }
                else
                {
                    PQENo = db.PurchaseQuotations.Max(p => p.PQuotNo + 1);
                    billNo = companyPrefix + PQENo;
                    if (BillExist(billNo))
                    {
                        billNo = InvoiceNo(PQENo, billNo, section);
                    }
                }
            }
            else
            {
                PQENo = PQENo + 1;
                billNo = companyPrefix + PQENo;
                if (BillExist(billNo))
                {
                    billNo = InvoiceNo(PQENo, billNo, section);
                }

            }
            return billNo;
        }
        private bool BillExist(string PQENo)
        {
            var Exists = db.PurchaseQuotations.Any(c => c.BillNo == PQENo);
            bool res = (Exists) ? true : false;
            return res;
        }
        [HttpGet]
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "PurchaseQuotation" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
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

            var MR = db.PurchaseQuotations.Where(a => a.PQuotationId== id).FirstOrDefault();

            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "PurchaseQuotation").OrderByDescending(a => a.CreatedDate).FirstOrDefault();
        
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
                AppUp.Type = "PurchaseQuotation";

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
                            join d in db.PurchaseQuotations on b.TransEntry equals d.PQuotationId into team
                            from d in team.DefaultIfEmpty()
                            join e in db.Employees on b.RequestBy equals e.UserId into emp
                            from e in emp.DefaultIfEmpty()
                            join u in db.Users on d.CreatedUserId equals u.Id into req
                            from u in req.DefaultIfEmpty()
                            where b.TransEntry == MCId && b.Type == "PurchaseQuotation"
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
