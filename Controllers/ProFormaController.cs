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
using System;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Drawing;
using System.Globalization;
using System.Drawing.Drawing2D;
using System.Text;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Net.Mail;



namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class ProFormaController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ProFormaController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        [QkAuthorize(Roles = "Dev,Pro Forma List")]
        public ActionResult Index()
        {
            ViewBag.Customer = QkSelect.List(
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
                new SelectListItem() {Text = "Active", Value="1"},
                new SelectListItem() {Text = "Inactive", Value="0"},
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

            var MlaPForma = db.EnableSettings.Where(a => a.EnableType == "MLAPForma ").FirstOrDefault();
            var MlaPFormas = MlaPForma != null ? MlaPForma.Status : Status.inactive;
            ViewBag.MLAPForma = MlaPFormas;

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindProForma").FirstOrDefault();
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
               }).Take(1)
               .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");
            var tsk = db.ProTasks
             .Select(s => new
             {
                 ID = s.ProTaskId,
                 Name = s.TaskName
             }).Take(1)
             .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            return View();
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Pro Forma Entry")]
        public ActionResult Create(long? id, string type)
        {
            var pfentry = new ProFormaViewModel
            {
                BillNo = InvoiceNo(),
                PFDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                PFNote = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "proforma").Select(a => a.TermsCondit).FirstOrDefault(),
                SalesTypes = db.SalesTypes.ToList(),
            };
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            if (id != null)
            {
                if (type == "Quote")
                {
                    Quotation quentry = db.Quotations.Find(id);

                    if (quentry == null)
                    {
                        return NotFound();
                    }
                    pfentry.ConTypeId = quentry.QuotationId;
                    pfentry.ConType = type;
                    pfentry.PFDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    pfentry.PFCashier = quentry.QuotCashier;
                    pfentry.Customer = quentry.Customer;
                    pfentry.PFDiscount = quentry.QuotDiscount;
                    pfentry.PFGrandTotal = quentry.QuotGrandTotal;
                    pfentry.Remarks = quentry.Remarks;
                    var custmr = db.Customers.Find(quentry.Customer);
                    pfentry.custEmailId = db.Contacts.Where(a => a.ContactID == custmr.Contact).Select(a => a.EmailId).FirstOrDefault();
                    pfentry.Branch = quentry.Branch;
                    pfentry.SaleType = quentry.SaleType;
                    pfentry.PaymentTerms = quentry.PaymentTerms;
                    pfentry.SalesType = quentry.SalesType;

                    pfentry.convertFrom = type + " No";//label
                    pfentry.convertBill = quentry.BillNo;

                    pfentry.PFNote = quentry.TermsCondition;
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        pfentry.Project = quentry.Project;
                        pfentry.ProTask = quentry.ProTask;
                    }
                    if (quentry.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Quotation").FirstOrDefault();
                        pfentry.FromDate = Hdet.StartDate;
                        pfentry.ToDate = Hdet.EndDate;
                        pfentry.HireType = Hdet.HireType;
                    }
                }
                if (type == "SOrder")
                {
                    SalesOrder sorder = db.SalesOrders.Find(id);

                    if (sorder == null)
                    {
                        return NotFound();
                    }
                    pfentry.ConTypeId = sorder.SalesOrderId;
                    pfentry.ConType = type;
                    pfentry.PFDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    pfentry.PFCashier = sorder.SOCashier;
                    pfentry.Customer = sorder.Customer;
                    pfentry.PFDiscount = sorder.SODiscount;
                    pfentry.PFGrandTotal = sorder.SOGrandTotal;
                    pfentry.Remarks = sorder.Remarks;
                    var custmr = db.Customers.Find(sorder.Customer);
                    pfentry.custEmailId = db.Contacts.Where(a => a.ContactID == custmr.Contact).Select(a => a.EmailId).FirstOrDefault();
                    pfentry.Branch = sorder.Branch;
                    pfentry.SaleType = sorder.SaleType;

                    pfentry.convertFrom = type + " No";//label
                    pfentry.convertBill = sorder.BillNo;

                    pfentry.PFNote = sorder.TermsCondition;
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        pfentry.Project = sorder.Project;
                        pfentry.ProTask = sorder.ProTask;
                    }
                    if (sorder.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Sales order").FirstOrDefault();
                        pfentry.FromDate = Hdet.StartDate;
                        pfentry.ToDate = Hdet.EndDate;
                        pfentry.HireType = Hdet.HireType;
                    }
                }
                if (type == "DVNote")
                {
                    Deliverynote dvnote = db.Deliverynotes.Find(id);

                    if (dvnote == null)
                    {
                        return NotFound();
                    }
                    pfentry.ConTypeId = dvnote.DeliverynoteId;
                    pfentry.ConType = type;
                    pfentry.PFDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    pfentry.PFCashier = dvnote.DvCashier;
                    pfentry.Customer = dvnote.Customer;
                    pfentry.PFDiscount = dvnote.DvDiscount;
                    pfentry.PFGrandTotal = dvnote.DvGrandTotal;
                    pfentry.Remarks = dvnote.Remarks;
                    var custmr = db.Customers.Find(dvnote.Customer);
                    pfentry.custEmailId = db.Contacts.Where(a => a.ContactID == custmr.Contact).Select(a => a.EmailId).FirstOrDefault();
                    pfentry.Branch = dvnote.Branch;
                    pfentry.SaleType = dvnote.SaleType;

                    pfentry.convertFrom = type + " No";//label
                    pfentry.convertBill = dvnote.BillNo;

                    pfentry.PFNote = dvnote.TermsCondition;
                    if (ViewBag.BusinessType == "ProjectBasedBusiness")
                    {
                        pfentry.Project = dvnote.Project;
                        pfentry.ProTask = dvnote.ProTask;
                    }
                    if (dvnote.SaleType == SaleType.Hire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Sales order").FirstOrDefault();
                        pfentry.FromDate = Hdet.StartDate;
                        pfentry.ToDate = Hdet.EndDate;
                        pfentry.HireType = Hdet.HireType;
                    }
                }
            }
            var cust = db.Customers.Where(s => s.Type == CRMCustomerType.Customer).Select(s => new
            {
                CustomerID = s.CustomerID,
                CustomerDetails = s.CustomerCode + " - " + s.CustomerName
            }).Take(1).ToList();
            ViewBag.Customr = QkSelect.List(cust, "CustomerID", "CustomerDetails");
            var use = db.Employees
                             .Select(s => new
                             {
                                 ID = s.EmployeeId,
                                 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                             })
                             .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");
            companySet();
            var UserId = User.Identity.GetUserId();

            var userpermission = User.IsInRole("All Pro Forma Entry");


            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            ViewBag.LastEntry = db.ProFormas.Where(p => (!MCList.Any() || MCArray.Contains(p.MaterialCenter)) && (userpermission == true || p.CreatedBy == UserId)).Select(p => p.ProFormaId).AsEnumerable().DefaultIfEmpty(0).Max();

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
            var hiretype = db.HireTypes
                .Select(s => new
                {
                    ID = s.HireTypeId,
                    Name = s.Name
                })
                .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.Contype = type;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var proj = db.Projects
                .Select(s => new
                {
                    ID = s.ProjectId,
                    Name = s.ProCode + " " + s.ProjectName
                }).Take(1)
                .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");
            var tsk = db.ProTasks
            .Select(s => new
            {
                ID = s.ProTaskId,
                Name = s.TaskName
            }).Take(1)
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

            var MlaPForma = db.EnableSettings.Where(a => a.EnableType == "MLAPForma ").FirstOrDefault();
            var MlaPFormas = MlaPForma != null ? MlaPForma.Status : Status.inactive;
            ViewBag.MLAPForma = MlaPFormas;

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            pfentry.FieldMap = db.FieldMappings.Where(a => a.Section == "ProForma" && a.Status == Status.active).ToList();
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

            return View(pfentry);
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Pro Forma Entry")]
        public JsonResult CreateProForma(string[][] array, string[] proforma, PFBillSundryViewModel bsmodel)
        {
            bool stat = false;
            string msg;
            if (!BillExist(Convert.ToString(proforma[17])))
            {
                var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
                var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

                var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
                var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

                var UserId = User.Identity.GetUserId();
                long Branch = 0;

                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                if (BranchCheck == Status.active)
                {
                    Branch = Convert.ToInt64(proforma[26]);
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
                    MC = Convert.ToInt64(proforma[23]);
                }
                else
                {
                    MC = 1;
                }

                string action = proforma[15];
                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                var Type = proforma[27] == null ? "Sale" : proforma[27];
                ProForma PFentry = new ProForma();
                if (proforma[27] != null)
                {
                    string str = proforma[27];
                    SaleType Stype = (SaleType)Enum.Parse(typeof(SaleType), str);
                    PFentry.SaleType = Stype;
                }
                else
                {
                    PFentry.SaleType = SaleType.Sale;
                }
                PFentry.PFNo = GetPFNo(PFentry.SaleType);
                PFentry.BillNo = Convert.ToString(proforma[17]);
                PFentry.PFDate = DateTime.Parse(proforma[2], new CultureInfo("en-GB"));
                PFentry.PFCashier = proforma[1] != "" ? Convert.ToInt64(proforma[1]) : 0;


                PFentry.Customer = Convert.ToInt64(proforma[0]);


                PFentry.PFItems = Convert.ToInt32(proforma[3]);
                PFentry.PFItemQuantity = Convert.ToDecimal(proforma[4]);
                PFentry.PFSubTotal = Convert.ToDecimal(proforma[8]);
                PFentry.PFTax = Convert.ToDecimal(proforma[9]);
                PFentry.PFTaxAmount = Convert.ToDecimal(proforma[5]);
                PFentry.PFDiscount = Convert.ToDecimal(proforma[6]);
                PFentry.PFGrandTotal = Convert.ToDecimal(proforma[7]);
                PFentry.PFNote = proforma[11];
                PFentry.Print = 1;
                PFentry.PFCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                PFentry.CreatedBy = UserId;
                PFentry.Status = 1;
                PFentry.Branch = Branch;
                PFentry.Location = proforma[20];
                PFentry.Remarks = proforma[22];
                PFentry.MaterialCenter = MC;
                PFentry.HSCode = proforma[31];
                PFentry.SalesType = Convert.ToInt32(proforma[32]);
                PFentry.PaymentTerms = (proforma[33]);
                PFentry.Project = proforma[34] != "" ? Convert.ToInt64(proforma[34]) : 0;
                PFentry.ProTask = proforma[35] != "" ? Convert.ToInt64(proforma[35]) : 0;
                //pay type
                PFentry.CustomerType = (proforma[21] == "1") ? CustomerType.Walking : CustomerType.Customer;

                PFentry.Ref1 = Convert.ToString(proforma[37]);
                PFentry.Ref2 = Convert.ToString(proforma[38]);
                PFentry.Ref3 = Convert.ToString(proforma[39]);
                PFentry.Ref4 = Convert.ToString(proforma[40]);
                PFentry.Ref5 = Convert.ToString(proforma[41]);

                db.ProFormas.Add(PFentry);
                db.SaveChanges();
                Int64 ProFormaEntryId = PFentry.ProFormaId;
                if (PFentry.SaleType == SaleType.Hire)
                {
                    HireDetail HDetils = new HireDetail();
                    HDetils.StartDate = DateTime.Parse(proforma[28], new CultureInfo("en-GB"));
                    HDetils.EndDate = DateTime.Parse(proforma[29], new CultureInfo("en-GB"));
                    HDetils.Section = "Proforma";
                    HDetils.Reference = ProFormaEntryId;
                    HDetils.HireType = Convert.ToInt64(proforma[30]);
                    db.HireDetails.Add(HDetils);
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
                dtItem.Columns.Add("ProForma");
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
                    if (arr.Length > 30)
                        dr["itemNote"] = Convert.ToString(arr[29]);
                    else
                        dr["itemNote"] = "";
                    dr["ProForma"] = ProFormaEntryId;
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
                        long typ = Convert.ToInt64(proforma[30]);
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
                                          ItemUnitPrice = (PFentry.SaleType == SaleType.Sale) ? a.ItemSubTotal : hir,
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
                            dbu["ProForma"] = ProFormaEntryId;
                            dbu["Item"] = bu.Item;
                            dtItem.Rows.Add(dbu);
                        }
                    }

                }

                ////// create parameter 
                SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = "TableTypePFItems";
                //// execute sp sql 
                string sql = String.Format("EXEC {0} {1};", "SP_InsertPFItems", "@TableType");
                //// execute sql 
                db.Database.ExecuteSqlRaw(sql, parameter);




                //billsundry
                if (bsmodel.pfbsundrys != null)
                {
                    string bsResult = string.Empty;

                    DataTable BsEntry = new DataTable();
                    BsEntry.Columns.Add("ProForma");
                    BsEntry.Columns.Add("BillSundry");
                    BsEntry.Columns.Add("BsValue");
                    BsEntry.Columns.Add("AmountType");
                    BsEntry.Columns.Add("BsType");
                    BsEntry.Columns.Add("BsAmount");

                    foreach (var bs in bsmodel.pfbsundrys)
                    {
                        DataRow drw = BsEntry.NewRow();
                        drw["ProForma"] = ProFormaEntryId;
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
                    parameter1.TypeName = "TableTypePFBillSundry";
                    //// execute sp sql 
                    string sql1 = String.Format("EXEC {0} {1};", "SP_InsertPFBillSundry", "@TableType");
                    //// execute sql 
                    db.Database.ExecuteSqlRaw(sql1, parameter1);

                }


                if (proforma[24] != null && proforma[24] != "0" && proforma[24] != "" && proforma[25] != null && proforma[25] != "" && proforma[25] != "0")
                {

                    ConvertTransactions ConTran = new ConvertTransactions();

                    ConTran.ConvertFrom = proforma[25];
                    ConTran.ConvertTo = "ProForma";
                    ConTran.From = Convert.ToInt64(proforma[24]);
                    ConTran.To = PFentry.ProFormaId;
                    ConTran.Status = 0;
                    ConTran.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                    ConTran.CreatedBy = UserId;
                    ConTran.Branch = Convert.ToInt32(BranchID);
                    db.ConvertTransactionss.Add(ConTran);
                    db.SaveChanges();
                    com.addlog(LogTypes.Created, UserId, "ConvertTransactions", "ConvertTransactionss", findip(), ConTran.Id, "Successfully Submitted Convertion");
                }

                //Approved By
                var Appby = Convert.ToString(proforma[36]);
                if (Appby != null && Appby != "")
                {
                    long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                    Approval approval = new Approval();
                    foreach (var emp in Approve)
                    {
                        approval.TransEntry = ProFormaEntryId;
                        approval.Type = "ProForma";
                        approval.EmployeeId = emp;
                        db.Approvals.Add(approval);
                        db.SaveChanges();
                    }
                }

                com.addlog(LogTypes.Created, UserId, "ProForma", "ProForma", findip(), ProFormaEntryId, "Successfully Submitted Pro Forma Entry");

                Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

                if (action == "print")
                {
                    var fmapp = db.FieldMappings.Where(a => a.Section == "ProForma" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                    var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                    var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                    var ProFormaData = com.ProFormaData(ProFormaEntryId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, ComHeadCheck);
                    var item = ProFormaData.pdfItem.ToList();
                    var summary = ProFormaData;
                    var billsundry = ProFormaData.billsundry.ToList();

                    var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                    var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                    var def = (PriLay == Status.active) ? Convert.ToInt64(proforma[42]) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                    def = def == 0 ? 1 : def;
                    var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, layout, fmapp, ProFormaEntryId = ProFormaEntryId } };
                }
                else if (action == "sendmail")
                {

                    SendMail sm = new SendMail();
                    MailMessage message = new MailMessage();
                    string ToMail = proforma[19];
                    string CcMail = "";
                    string InvoiceNo = "_ProForma_" + PFentry.BillNo;

                    var em = db.EmailTemplates.Where(a => a.Head == "ProForma").FirstOrDefault();
                    if (em != null)
                    {
                        message.Subject = em.Subject;
                        message.Body = em.EmailBody;
                    }
                    else
                    {
                        message.Subject = "Pro Forma";
                        message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                            " <p>we are enclosing our pro forma for the items / services as requested by you during our discussions.<br/></p> " +
                            " <p>Looking forward to hear from you.</p>";
                    }
                    sm.SendPdfMail(generatePdf(ProFormaEntryId), ToMail, CcMail, InvoiceNo, message);

                    msg = "Successfully submitted Pro Forma Entry.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, ProFormaEntryId = ProFormaEntryId } };
                }
                else
                {
                    msg = "Successfully submitted Pro Forma Entry.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, ProFormaEntryId = ProFormaEntryId } };
                }
                //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                msg = "Invoice No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

            }
        }


        [HttpPost]
        [QkAuthorize(Roles = "Dev,Pro Forma List")]
        public ActionResult GetProFormaEntry(string BillNo, string FromDate, string ToDate, long? customer, long? salesperson, int? Stats, string user, string Saletype, long? HireType, long? MC, string appstat, long? ProjectName, long? Task)
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

            var fromv = "ProForma";
            var Tosales = "Sale";
            var ToDVN = "DVNote";
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
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

            var userpermission = User.IsInRole("All Pro Forma Entry");
            var UserId = User.Identity.GetUserId();
            SaleType St = new SaleType();
            if (Saletype != "")
            {
                St = (Saletype == "2") ? SaleType.Hire : SaleType.Sale;
            };
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var uDev = User.IsInRole("Dev");
            var uProFormaView = User.IsInRole("View Pro Forma");
            var uEdit = User.IsInRole("Edit Pro Forma");
            var uDownload = User.IsInRole("Download Pro Forma");
            var uDelete = User.IsInRole("Delete Pro Forma");

            var PFToSale = db.EnableSettings.Where(a => a.EnableType == "PFToSale").FirstOrDefault();
            var PFToSales = PFToSale != null ? PFToSale.Status : Status.inactive;

            var PFToDvNote = db.EnableSettings.Where(a => a.EnableType == "PFToDvNote").FirstOrDefault();
            var PFToDvNotes = PFToDvNote != null ? PFToDvNote.Status : Status.inactive;

            // EF Core 10 cannot translate the nested-collection / GroupBy-latest projections (the `app`,
            // `AppStatus`, `chkAppStatus` lets and the `qs/ps` ConvertTransactionss subqueries).
            // Split SERVER from CLIENT: materialize only entity columns + simple scalars (left-joined entity
            // access like Customer/EmpName/user stays server-side) into serverRows, then build client lookups
            // keyed by ProFormaId and re-project client-side with the SAME member names + order.
            var serverQuery = (from a in db.ProFormas
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join d in db.Employees on a.PFCashier equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                     join g in db.Users on a.CreatedBy equals g.Id
                     join h in db.HireDetails on new { h1 = a.ProFormaId, h2 = "Proforma" }
                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                     from h in hir.DefaultIfEmpty()
                     join i in db.MCs on a.MaterialCenter equals i.MCId into mcs
                     from i in mcs.DefaultIfEmpty()
                     join j in db.Projects on a.Project equals j.ProjectId into prj
                     from j in prj.DefaultIfEmpty()
                     join k in db.ProTasks on a.ProTask equals k.ProTaskId into task
                     from k in task.DefaultIfEmpty()
                     //let mc = db.MCs.Where(x => x.AssignedUser == a.CreatedBy).Select(x => x.MCId).FirstOrDefault()
                         // qs/ps (ConvertTransactionss .FirstOrDefault subqueries) and
                         // app/AppStatus/chkAppStatus (nested collections + GroupBy-latest) are all computed
                         // client-side after materialization — EF Core 10 can't translate them inside this query.

                     where ((BillNo == null || BillNo == "" || a.BillNo == BillNo) &&
                     (customer == 0 || a.Customer == customer) &&
                     (salesperson == 0 || d.EmployeeId == salesperson) &&
                     (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(a.PFDate, fdate) <= 0) &&
                     (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(a.PFDate, tdate) >= 0)
                     && (user == null || user == "" || g.Id == user)) //&& (Validity == null || Validity == 0 || b.val == Validity)
                     && (Stats == null || a.Status == Stats)
                     //&& (mc == 0 || mc == a.MaterialCenter)
                     && ((MCArray.Contains(a.MaterialCenter) && MC == a.MaterialCenter) || ((MC == null) && MCArray.Contains(a.MaterialCenter)))
                     && (userpermission == true || a.CreatedBy == UserId)
                     && (Saletype == "" || Saletype == null || St == a.SaleType) && (HireType == 0 || HireType == null || HireType == h.HireType)
                     && (ProjectName == 0 || ProjectName == null || j.ProjectId == ProjectName)
                     && (Task == 0 || Task == null || k.ProTaskId == Task)
                     select new
                     {
                         a.ProFormaId,
                         a.PFNo,
                         a.BillNo,
                         a.PFDate,
                         a.PFGrandTotal,
                         Customer = b.CustomerCode + " - " + b.CustomerName,
                         EmpName = d.FirstName + " " + d.LastName,
                         user = g.UserName,
                         a.Location,
                         a.Remarks,
                         SaleType = a.SaleType,
                         Dev = uDev,
                         Details = uProFormaView,
                         Edit = uEdit,
                         Download = uDownload,
                         Delete = uDelete,
                         MC = i.MCName,
                         ProjectName = (j.ProjectName != null && j.ProjectName != "") ? j.ProCode + "-" + j.ProjectName : "",
                         Task = (k.TaskName != null && k.TaskName != "") ? k.TaskCode + "-" + k.TaskName : "",
                         CreatedDate = a.PFCreatedDate
                     });

            // Performance (audit P2, hybrid): server paging when no search, plain-column sort, AND no client-side
            // filter is active (the conditions below mirror the filters' own guards); else original path.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase){ "BillNo","CreatedDate","Customer","Delete","Details","Dev","Download","Edit","EmpName","Location","MC","PFDate","PFGrandTotal","PFNo","ProFormaId","ProjectName","Remarks","SaleType","Task","user" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0 && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn)) && !(appstat != "");
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn) ? serverQuery.OrderBy("ProFormaId asc") : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

            // CLIENT-side lookups keyed by ProFormaId (missing key -> empty/absent, no KeyNotFound).
            var pfIds = serverRows.Select(o => o.ProFormaId).ToList();
            // The two convert markers: latest-or-any ConvertTransactionss row per (ProFormaId, ConvertTo).
            var convLookup = db.ConvertTransactionss
                .Where(ap => ap.ConvertFrom == fromv && pfIds.Contains(ap.From)
                       && (ap.ConvertTo == Tosales || ap.ConvertTo == ToDVN))
                .Select(ap => new { ap.From, ap.ConvertTo })
                .ToList()
                .ToLookup(ap => ap.From);
            // app = approver EmployeeIds for the proforma (nested collection, keyed by TransEntry == ProFormaId).
            var appLookup = db.Approvals
                .Where(a => a.Type == "ProForma" && pfIds.Contains(a.TransEntry))
                .Select(a => new { a.TransEntry, a.EmployeeId })
                .ToList()
                .ToLookup(a => a.TransEntry);
            // AppStatus = all ApprovalUpdate statuses; raw rows materialized once and reused for chkAppStatus.
            var appUpdRows = db.ApprovalUpdates
                .Where(a => a.Type == "ProForma" && pfIds.Contains(a.TransEntry))
                .Select(a => new { a.TransEntry, a.ApprovalStatus, a.ApprovedBy, a.CreatedDate })
                .ToList();
            var appStatusLookup = appUpdRows.ToLookup(a => a.TransEntry);
            // chkAppStatus = latest ApprovalUpdate status per ApprovedBy (GroupBy-latest-per-group), per proforma.
            var chkAppStatusLookup = appUpdRows
                .GroupBy(a => a.TransEntry)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(l => l.ApprovedBy)
                    .Select(grp => grp.OrderByDescending(c => c.CreatedDate).First())
                    .Select(a => a.ApprovalStatus).ToList());

            var v = serverRows.Select(o =>
                     {
                         var conv = convLookup[o.ProFormaId];
                         var SaleConvert = conv.Where(x => x.ConvertTo == Tosales).Select(x => x.ConvertTo).FirstOrDefault();
                         var DVNConvert = conv.Where(x => x.ConvertTo == ToDVN).Select(x => x.ConvertTo).FirstOrDefault();
                         var app = appLookup[o.ProFormaId].Select(x => x.EmployeeId).ToList();
                         var AppStatus = appStatusLookup[o.ProFormaId].Select(x => x.ApprovalStatus).ToList();
                         var chkAppStatus = chkAppStatusLookup.TryGetValue(o.ProFormaId, out var ck) ? ck : new List<ApprovalStatus>();
                         return new
                     {

                         SaleConvert = SaleConvert,
                         DVNConvert = DVNConvert,

                         o.ProFormaId,
                         o.PFNo,
                         o.BillNo,
                         o.PFDate,
                         o.PFGrandTotal,
                         o.Customer,
                         o.EmpName,
                         o.user,
                         o.Location,
                         o.Remarks,
                         o.SaleType,
                         o.Dev,
                         o.Details,
                         o.Edit,
                         o.Download,
                         o.Delete,
                         o.MC,
                         app = app,
                         Approval = (app != null && empl.EmployeeId != null) ? (app.Contains(empl.EmployeeId) ? true : false) : false,
                         ApprovalStatus = (app.Count > 0 && AppStatus.Count > 0 && chkAppStatus.Count > 0) ? (chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (app != null && chkAppStatus != null && app.Count != 0 && chkAppStatus.Count != 0 && chkAppStatus.Count() == app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                         o.ProjectName,
                         o.Task,
                         o.CreatedDate,
                         PFToSale = (SaleConvert != null && PFToSales == Status.active) ? false : true,
                         PFToDvNote = (DVNConvert != null && PFToDvNotes == Status.active) ? false : true,
                     };
                     });
            if (appstat != "")
            {
                v = v.Where(a => a.ApprovalStatus == AppSt);
            }
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
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
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Edit Pro Forma")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userpermission = User.IsInRole("All Pro Forma Entry");
            var UserId = User.Identity.GetUserId();
            ProForma Pfentry = db.ProFormas.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.ProFormaId == id).FirstOrDefault();

            //Fetching the images from AttachmentDocuments
            ViewBag.Image = (from a in db.AttachmentDocuments
                             join b in db.ProFormas
                             on a.TransactionID equals b.ProFormaId
                             where a.TransactionID == id && a.TransactionType == "proforma"
                             select new ProformaDocumentViewModel
                             {
                                 DocumentID = a.DocumentID,
                                 ProformaId = a.TransactionID,
                                 FileName = a.FileName,
                                 CreatedDate = a.CreatedDate
                             }).ToList();
            //------------------------------------------------------


            ViewBag.JournalID = id;
            if (Pfentry == null)
            {
                return NotFound();
            }
            Int64 cashier = Convert.ToInt64(Pfentry.PFCashier);
            Int64 customer = Pfentry.Customer;

            string custname = "";
            ProFormaViewModel vmodel = new ProFormaViewModel();

            custname = db.Customers.Where(a => a.CustomerID == customer).Select(a => a.CustomerCode + " - " + a.CustomerName).FirstOrDefault();

            var cust = db.Customers.Where(s =>s.CustomerID== customer).Select(s => new
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

            var CBill = "";
            var CType = "";
            var ConvertTran = db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "ProForma").FirstOrDefault();
            if (ConvertTran != null)
            {
                CType = ConvertTran.ConvertFrom + " No";
                if (ConvertTran.ConvertFrom == "Quote")
                {
                    CBill = db.Quotations.Where(a => a.QuotationId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "SOrder")
                {
                    CBill = db.SalesOrders.Where(a => a.SalesOrderId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
            }

            vmodel = (from b in db.ProFormas
                      join d in db.Customers on b.Customer equals d.CustomerID into cst
                      from d in cst.DefaultIfEmpty()
                      join e in db.Contacts on d.Contact equals e.ContactID into cont
                      from e in cont.DefaultIfEmpty()
                      join f in db.HireDetails on new { f1 = b.ProFormaId, f2 = "Proforma" }
                      equals new { f1 = f.Reference, f2 = f.Section } into hir
                      from f in hir.DefaultIfEmpty()
                      where b.ProFormaId == id
                      select new ProFormaViewModel
                      {
                          PFNo = b.PFNo,
                          PFNote = b.PFNote,
                          PFDate = b.PFDate,
                          BillNo = b.BillNo,
                          PFCashier = b.PFCashier,
                          Customer = b.Customer,
                          PFDiscount = b.PFDiscount,
                          PFGrandTotal = b.PFGrandTotal,
                          // PFPaidAmount = c.PFPaidAmount,
                          // PFDueAmount = b.PFGrandTotal - c.PFPaidAmount,
                          CustomerName = custname,
                          custEmailId = e.EmailId,
                          Location = b.Location,
                          Remarks = b.Remarks,
                          MaterialCenter = b.MaterialCenter,
                          Branch = b.Branch,
                          SaleType = b.SaleType,
                          FromDate = f.StartDate,
                          ToDate = f.EndDate,
                          HireType = f.HireType,
                          HSCode = b.HSCode,
                          SalesType = b.SalesType,
                          SalesTypes = db.SalesTypes.ToList(),
                          PaymentTerms = b.PaymentTerms,
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

            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            ViewBag.preEntry = db.ProFormas.Where(a => a.ProFormaId < id && (!MCList.Any() || MCArray.Contains(a.MaterialCenter)) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.ProFormaId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.ProFormas.Where(a => a.ProFormaId > id && (!MCList.Any() || MCArray.Contains(a.MaterialCenter)) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.ProFormaId).DefaultIfEmpty().Min();

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
            var proj = db.Projects.Where(o=>o.ProjectId==vmodel.Project)
              .Select(s => new
              {
                  ID = s.ProjectId,
                  Name = s.ProCode + " " + s.ProjectName
              })
              .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");
            var tsk = db.ProTasks.Where(o=>o.ProTaskId==vmodel.ProTask)
            .Select(s => new
            {
                ID = s.ProTaskId,
                Name = s.TaskName
            })
            .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            ViewBag.PopUpAddCust = false;

            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "ProForma").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaPForma = db.EnableSettings.Where(a => a.EnableType == "MLAPForma ").FirstOrDefault();
            var MlaPFormas = MlaPForma != null ? MlaPForma.Status : Status.inactive;
            ViewBag.MLAPForma = MlaPFormas;

            var EditPermission = User.IsInRole("Disable ProForma Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "ProForma", UserId);

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "ProForma" && a.Status == Status.active).ToList();

            //dummy table operations
            var DItem = db.DummyPFItems.Where(a => a.ProForma == id).FirstOrDefault();
            var PItem = db.PFItemss.Where(a => a.ProForma == id).FirstOrDefault();
            if (PItem == null && DItem != null)
            {
                var DItems = db.DummyPFItems.Where(a => a.ProForma == id).ToList();
                foreach (var arr in DItems)
                {
                    //add to se-item table
                    PFItems pItem = new PFItems();
                    pItem.ItemUnit = arr.ItemUnit;
                    pItem.ItemUnitPrice = arr.ItemUnitPrice;
                    pItem.ItemQuantity = arr.ItemQuantity;
                    pItem.ItemSubTotal = arr.ItemSubTotal;
                    pItem.ItemDiscount = arr.ItemDiscount;
                    pItem.ItemTax = arr.ItemTax;
                    pItem.ItemTaxAmount = arr.ItemTaxAmount;
                    pItem.ItemTotalAmount = arr.ItemTotalAmount;
                    pItem.itemNote = arr.itemNote;
                    pItem.ProForma = arr.ProForma;
                    pItem.Item = arr.Item;
                    db.PFItemss.Add(pItem);
                    db.SaveChanges();
                }

                db.DummyPFItems.RemoveRange(db.DummyPFItems.Where(a => a.ProForma == id));
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

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Pro Forma")]
        public ActionResult UpdateProForma(string[][] array, string[] proforma, PFBillSundryViewModel bsmodel)
        {
            bool stat = false;
            string msg;
            Int64 proFormaEntryId = Convert.ToInt64(proforma[15]);
            ProForma PFentry = db.ProFormas.Find(proFormaEntryId);
            if (BillExist(Convert.ToString(proforma[16])) && Convert.ToString(proforma[16]) != PFentry.BillNo)
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
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

            long Branch = 0;

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

            if (BranchCheck == Status.active)
            {
                Branch = Convert.ToInt64(proforma[24]);
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
                MC = Convert.ToInt64(proforma[23]);
            }
            else
            {
                MC = 1;
            }

            var EditPermission = User.IsInRole("Disable ProForma Edit After Approval");
            if (com.chkApproved(proFormaEntryId, EditPermission, "ProForma", UserId) == true)
            {

                PFentry.PFDate = DateTime.Parse(proforma[2], new CultureInfo("en-GB"));
                PFentry.PFCashier = proforma[1] != "" ? Convert.ToInt64(proforma[1]) : 0;
                if (proforma[25] != null)
                {
                    string str = proforma[25];
                    SaleType Stype = (SaleType)Enum.Parse(typeof(SaleType), str);
                    PFentry.SaleType = Stype;
                }
                else
                {
                    PFentry.SaleType = SaleType.Sale;
                }
                PFentry.BillNo = proforma[16];
                PFentry.Customer = Convert.ToInt64(proforma[0]);
                PFentry.PFItems = Convert.ToInt32(proforma[3]);
                PFentry.PFItemQuantity = Convert.ToDecimal(proforma[4]);
                PFentry.PFSubTotal = Convert.ToDecimal(proforma[8]);
                PFentry.PFTax = Convert.ToDecimal(proforma[9]);
                PFentry.PFTaxAmount = Convert.ToDecimal(proforma[5]);
                PFentry.PFDiscount = Convert.ToDecimal(proforma[6]);
                PFentry.PFGrandTotal = Convert.ToDecimal(proforma[7]);
                PFentry.PFNote = proforma[11];
                PFentry.Print = 1;
                PFentry.Status = 1;
                PFentry.Branch = Branch;
                PFentry.Location = proforma[20];
                PFentry.Remarks = proforma[22];
                PFentry.MaterialCenter = MC;
                PFentry.HSCode = proforma[29];
                PFentry.SalesType = Convert.ToInt64(proforma[30]);
                PFentry.PaymentTerms = proforma[31];
                PFentry.Project = proforma[32] != "" ? Convert.ToInt64(proforma[32]) : 0;
                PFentry.ProTask = proforma[33] != "" ? Convert.ToInt64(proforma[33]) : 0;
                //pay type
                PFentry.CustomerType = (proforma[21] == "1") ? CustomerType.Walking : CustomerType.Customer;

                PFentry.Ref1 = Convert.ToString(proforma[35]);
                PFentry.Ref2 = Convert.ToString(proforma[36]);
                PFentry.Ref3 = Convert.ToString(proforma[37]);
                PFentry.Ref4 = Convert.ToString(proforma[38]);
                PFentry.Ref5 = Convert.ToString(proforma[39]);

                db.Entry(PFentry).State = EntityState.Modified;
                db.SaveChanges();
                var HireItem = db.HireDetails.Where(a => a.Reference == proFormaEntryId && a.Section == "Proforma").FirstOrDefault();
                if (HireItem != null)
                {
                    db.HireDetails.RemoveRange(db.HireDetails.Where(a => a.Reference == proFormaEntryId && a.Section == "Proforma"));
                    db.SaveChanges();
                }
                if (PFentry.SaleType == SaleType.Hire)
                {
                    HireDetail HDetils = new HireDetail();
                    HDetils.StartDate = DateTime.Parse(proforma[26], new CultureInfo("en-GB"));
                    HDetils.EndDate = DateTime.Parse(proforma[27], new CultureInfo("en-GB"));
                    HDetils.Section = "Proforma";
                    HDetils.Reference = proFormaEntryId;
                    HDetils.HireType = Convert.ToInt64(proforma[28]);
                    db.HireDetails.Add(HDetils);
                    db.SaveChanges();
                }

                var PFItem = db.PFItemss.Where(a => a.ProForma == proFormaEntryId).FirstOrDefault();
                if (PFItem != null)
                {
                    var PItems = db.PFItemss.Where(a => a.ProForma == proFormaEntryId).ToList();
                    foreach (var arr in PItems)
                    {
                        //add to dummy table
                        DummyPFItem dItem = new DummyPFItem();
                        dItem.ItemUnit = arr.ItemUnit;
                        dItem.ItemUnitPrice = arr.ItemUnitPrice;
                        dItem.ItemQuantity = arr.ItemQuantity;
                        dItem.ItemSubTotal = arr.ItemSubTotal;
                        dItem.ItemDiscount = arr.ItemDiscount;
                        dItem.ItemTax = arr.ItemTax;
                        dItem.ItemTaxAmount = arr.ItemTaxAmount;
                        dItem.ItemTotalAmount = arr.ItemTotalAmount;
                        dItem.itemNote = arr.itemNote;
                        dItem.ProForma = arr.ProForma;
                        dItem.Item = arr.Item;
                        db.DummyPFItems.Add(dItem);
                        db.SaveChanges();
                    }

                    db.PFItemss.RemoveRange(db.PFItemss.Where(a => a.ProForma == proFormaEntryId));
                    db.SaveChanges();
                }

                ////// add to PFItem
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

                dtItem.Columns.Add("ProForma");
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
                    if (arr.Length >= 30)
                        dr["itemNote"] = Convert.ToString(arr[29]);
                    else
                        dr["itemNote"] = "";
                    dr["ProForma"] = proFormaEntryId;
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
                        long typ = Convert.ToInt64(proforma[28]);
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
                                          ItemUnitPrice = (PFentry.SaleType == SaleType.Sale) ? a.ItemSubTotal : hir,
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
                            dbu["ProForma"] = proFormaEntryId;
                            dbu["Item"] = bu.Item;
                            dtItem.Rows.Add(dbu);
                        }
                    }

                }


                ////// create parameter 
                SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = "TableTypePFItems";
                //// execute sp sql 
                string sql = String.Format("EXEC {0} {1};", "SP_InsertPFItems", "@TableType");
                //// execute sql 
                var ret = db.Database.ExecuteSqlRaw(sql, parameter);
                if (ret > 0)
                {
                    db.DummyPFItems.RemoveRange(db.DummyPFItems.Where(a => a.ProForma == proFormaEntryId));
                    db.SaveChanges();
                }

                var PFBs = db.PFBillSundrys.Where(a => a.ProForma == proFormaEntryId).FirstOrDefault();
                if (PFBs != null)
                {
                    db.PFBillSundrys.RemoveRange(db.PFBillSundrys.Where(a => a.ProForma == proFormaEntryId));
                    db.SaveChanges();
                }
                if (bsmodel.pfbsundrys != null)
                {
                    string bsResult = string.Empty;
                    DataTable BsEntry = new DataTable();
                    BsEntry.Columns.Add("ProForma");
                    BsEntry.Columns.Add("BillSundry");
                    BsEntry.Columns.Add("BsValue");
                    BsEntry.Columns.Add("AmountType");
                    BsEntry.Columns.Add("BsType");
                    BsEntry.Columns.Add("BsAmount");

                    foreach (var bs in bsmodel.pfbsundrys)
                    {
                        DataRow drw = BsEntry.NewRow();
                        drw["ProForma"] = proFormaEntryId;
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
                    parameter1.TypeName = "TableTypePFBillSundry";
                    //// execute sp sql 
                    string sql1 = String.Format("EXEC {0} {1};", "SP_InsertPFBillSundry", "@TableType");
                    //// execute sql 
                    db.Database.ExecuteSqlRaw(sql1, parameter1);

                }
                //Approved By
                var empuser = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                var chkapp = db.Approvals.Where(a => a.EmployeeId == empuser && a.TransEntry == proFormaEntryId && a.Type == "ProForma").FirstOrDefault();
                var MrnPO = db.Approvals.Where(a => a.TransEntry == proFormaEntryId && a.Type == "ProForma").FirstOrDefault();
                if (MrnPO != null)
                {
                    if (chkapp != null)
                    {
                        db.Approvals.RemoveRange(db.Approvals.Where(a => a.EmployeeId != empuser && a.TransEntry == proFormaEntryId && a.Type == "ProForma"));
                        db.SaveChanges();
                    }
                    else
                    {
                        db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == proFormaEntryId && a.Type == "ProForma"));
                        db.SaveChanges();
                    }
                }
                var Appby = Convert.ToString(proforma[34]);
                if (Appby != null && Appby != "")
                {
                    long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                    Approval approval = new Approval();
                    foreach (var emp in Approve)
                    {
                        approval.TransEntry = proFormaEntryId;
                        approval.Type = "ProForma";
                        approval.EmployeeId = emp;
                        db.Approvals.Add(approval);
                        db.SaveChanges();
                    }
                }
                com.addlog(LogTypes.Updated, UserId, "ProForma", "ProForma", findip(), proFormaEntryId, "Successfully Updated Pro Forma Entry");
            }
            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

            string action = proforma[18];
            if (action == "print")
            {
                var fmapp = db.FieldMappings.Where(a => a.Section == "ProForma" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();


                var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

                var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                var ProFormaData = com.ProFormaData(proFormaEntryId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, ComHeadCheck);
                var item = ProFormaData.pdfItem.ToList();
                var summary = ProFormaData;
                var billsundry = ProFormaData.billsundry.ToList();

                var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                var def = (PriLay == Status.active) ? Convert.ToInt64(proforma[40]) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                def = def == 0 ? 1 : def;
                var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, layout, fmapp } };
            }
            else if (action == "sendmail")
            {

                SendMail sm = new SendMail();
                MailMessage message = new MailMessage();
                string ToMail = proforma[19];
                string CcMail = "";
                string InvoiceNo = "_ProForma_" + PFentry.BillNo;

                var em = db.EmailTemplates.Where(a => a.Head == "ProForma").FirstOrDefault();
                if (em != null)
                {
                    message.Subject = em.Subject;
                    message.Body = em.EmailBody;
                }
                else
                {
                    message.Subject = "Pro Forma";
                    message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                        " <p>we are enclosing our pro forma for the items / services as requested by you during our discussions.<br/></p> " +
                        " <p>Looking forward to hear from you.</p>";
                }
                sm.SendPdfMail(generatePdf(proFormaEntryId), ToMail, CcMail, InvoiceNo, message);

                msg = "Successfully Updated Pro Forma Entry.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                msg = "Successfully Updated Pro Forma Entry.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Download Pro Forma")]
        public ActionResult Download(long id)
        {
            var Data = db.ProFormas.Where(s => s.ProFormaId == id).FirstOrDefault();
            var custname = db.Customers.Where(s => s.CustomerID == Data.Customer).Select(a => a.CustomerName).FirstOrDefault();

            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = Data.BillNo;

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), "inactive");
            return File(ms, "application/pdf", "Pro Forma" + "-" + custname + "-" + billno + ".pdf");

        }
        public StringBuilder generatePdf(long Id)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

            var ProFormaData = com.ProFormaData(Id, InPrintItemCode, PartNoCheck, TimeOut);
            var item = ProFormaData.pdfItem.ToList();
            var summary = ProFormaData;
            var billsundry = ProFormaData.billsundry.ToList();


            return com.generatepdf(Id, summary, item, billsundry, "Proforma");
        }

        //                   where a.ProFormaId == ProFormaEntryId
        //                       PartyName = b.CustomerName,
        //                       BillNo = a.BillNo,
        //                       Date = a.PFDate,
        //                       Note = a.PFNote,
        //                       Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
        //                       Discount = a.PFDiscount,

        //                       GrandTotal = a.PFGrandTotal,
        //                       SubTotal = a.PFSubTotal,
        //                       TaxAmount = a.PFTaxAmount,
        //                       c.Address,
        //                       c.City,
        //                       c.State,
        //                       c.Country,
        //                       c.Zip,
        //                       Email = c.EmailId,
        //                       Phone = c.Phone,
        //                       Mobile = c.Mobile,
        //                       TRN = b.TaxRegNo,
        //                       BillId = a.PFNo,
        //                       TermsCondition = a.PFNote,
        //                       a.Location,
        //                       b.CreditPeriod,
        //                       a.Remarks

        //                   where b.ProForma == ProFormaEntryId && b.itemNote != "-:{Bundle_Item}"
        //                       ItemUnitPrice = b.ItemUnitPrice,
        //                       ItemQuantity = b.ItemQuantity,
        //                       ItemSubTotal = b.ItemSubTotal,
        //                       ItemNote = b.itemNote,
        //                       ItemTax = b.ItemTax,
        //                       ItemTaxAmount = b.ItemTaxAmount,
        //                       ItemTotalAmount = b.ItemTotalAmount,
        //                       ItemID = b.Item,

        //                       bundleitem = (from ab in db.PFItemss
        //                                     where ab.ProForma == ProFormaEntryId && ab.itemNote == "-:{Bundle_Item}"
        //                                     && b.Item == ab.ItemDiscount

        //                                         bb.ItemCode,
        //                                         bb.ItemName,
        //                                         cb.ItemUnitName,
        //                                         ItemUnitPrice = ab.ItemUnitPrice,
        //                                         quantity = ab.ItemQuantity,
        //                                         ItemSubTotal = ab.ItemSubTotal,
        //                                         ItemTax = ab.ItemTax,
        //                                         ItemTaxAmount = ab.ItemTaxAmount,
        //                                         ItemTotalAmount = ab.ItemTotalAmount,

        //                                         ab.Item,
        //                                         ab.ItemQuantity,
        //                                         ab.ItemUnit,

        //                                         ItemDiscount = 0,

        //                                         ItemNote = ab.itemNote,
        //                                         ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
        //                                         bb.ItemUnitID,
        //                                         bb.SubUnitId,
        //                                         PriUnit = cb.ItemUnitName,
        //                                         SubUnit = bd.ItemUnitName,
        //                                         bb.ItemArabic

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









        [HttpGet]
        public ActionResult GetPFItems2(long ProFormaID, string ConvertTo)
        {
            var temp = db.ConvertTransactionss.Where(a => a.From == ProFormaID && a.ConvertFrom == "ProForma" && a.ConvertTo == ConvertTo).Select(a => a.To);
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

            var ConD = (from a in db.PFItemss
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.ProForma == ProFormaID && a.itemNote != "-:{Bundle_Item}"
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
                            note = a.itemNote.Replace("<br />", "\n"),
                            ItemNote = a.itemNote != null ? a.itemNote : "",
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
            return Json(RemainingItems);
        }

        [HttpGet]
        public ActionResult GetPFItems(long ProFormaID)
        {
            var ConD = (from a in db.PFItemss
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()
                        where a.ProForma == ProFormaID && a.itemNote != "-:{Bundle_Item}"
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

                            PriPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,


                            PriSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0

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
                            price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            o.KeepStock,

                            PriPurchase = (o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)),
                            SubPurchase = (o.SubPurchase % o.ConFactor),

                            PriSale = (o.PriSale + (int)(o.SubSale / o.ConFactor)),
                            SubSale = (o.SubSale % o.ConFactor),

                            PriPReturn = (o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)),
                            SubPReturn = (o.SubPReturn % o.ConFactor),

                            PriSReturn = (o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)),
                            SubSReturn = (o.SubSReturn % o.ConFactor),

                            pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn) - (o.PriSale + o.PriPReturn)),
                            subtotal = ((o.SubPurchase + o.SubSReturn) - (o.SubSale + o.SubPReturn)),
                            total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn) - (o.PriSale + o.PriPReturn)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn) - (o.SubSale + o.SubPReturn)),
                        }).ToList();
            return Json(ConD);
        }


        //[HttpGet]
        //                where a.ProForma == ProFormaID
        //                    a.Item,
        //                    a.ItemQuantity,
        //                    a.ItemUnit,
        //                    a.ItemUnitPrice,
        //                    a.ItemTax,
        //                    a.ItemSubTotal,
        //                    a.ItemTaxAmount,
        //                    a.ItemDiscount,
        //                    ItemNote = a.itemNote != null ? a.itemNote : "",
        //                    a.ItemTotalAmount,
        //                    ItemCode = b.ItemCode,
        //                    ItemName = b.ItemName,
        //                    ItemWithCode = b.ItemCode + " - " + b.ItemName,
        //                    b.ItemUnitID,
        //                    b.SubUnitId,
        //                    PriUnit = c.ItemUnitName,
        //                    SubUnit = d.ItemUnitName,
        //                    b.BasePrice,
        //                    b.SellingPrice,
        //                    b.PurchasePrice,
        //                    b.MRP



        [HttpGet]
        public ActionResult GetPFBillSundry(long ProFormaID)
        {
            var SEBs = (from a in db.PFBillSundrys
                        join c in db.BillSundrys on a.BillSundry equals c.BillSundryId
                        where a.ProForma == ProFormaID
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
            return Json(SEBs);
        }




        [HttpGet]
        [QkAuthorize(Roles = "Dev,View Pro Forma")]
        public ActionResult Details(long? id)
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            ProFormaViewModel vmodel = new ProFormaViewModel();
            vmodel = (from b in db.ProFormas
                      join d in db.Employees on b.PFCashier equals d.EmployeeId into emp
                      from d in emp.DefaultIfEmpty()
                      join f in db.Customers on b.Customer equals f.CustomerID into cust
                      from f in cust.DefaultIfEmpty()
                      join g in db.MCs on b.MaterialCenter equals g.MCId into mcs
                      from g in mcs.DefaultIfEmpty()
                      join t in db.SalesTypes on b.SalesType equals t.Id into stype
                      from t in stype.DefaultIfEmpty()
                      join x in db.Contacts on f.Contact equals x.ContactID into cnt
                      from x in cnt.DefaultIfEmpty()
                      join u in db.HireDetails on new { h1 = b.ProFormaId, h2 = "Proforma" }
                      equals new { h1 = u.Reference, h2 = u.Section } into hir
                      from u in hir.DefaultIfEmpty()
                      join v in db.HireTypes on u.HireType equals v.HireTypeId into htyp
                      from v in htyp.DefaultIfEmpty()
                      where b.ProFormaId == id
                      select new ProFormaViewModel
                      {
                          CustomerName = f.CustomerCode + " - " + f.CustomerName,
                          PFNo = b.PFNo,
                          BillNo = b.BillNo,
                          PFDate = b.PFDate,
                          PFNote = b.PFNote.Replace("\n", "<br />"),
                          EmployeeName = d.FirstName + " " + d.LastName,
                          PFDiscount = b.PFDiscount,
                          PFTotal = b.PFDiscount + b.PFGrandTotal,
                          PFGrandTotal = b.PFGrandTotal,
                          Location = b.Location,
                          PayType = (b.CustomerType == CustomerType.Walking ? "Cash" : "Credit"),
                          CreditPeriod = f.CreditPeriod,
                          Remarks = b.Remarks.Replace("\n", "<br />"),
                          MCName = g.MCName,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,

                          SaleTypeName = (b.SaleType == SaleType.Sale) ? "Sale" : ((b.SaleType == SaleType.Hire) ? "Hire" : "POS"),
                          SalesTypeName = t.Name,
                          EmailId = x.EmailId,
                          PaymentTerms = b.PaymentTerms,
                          HSCode = b.HSCode,

                          HType = (u != null) ? v.Name : "",
                          StartDate = (u != null) ? u.StartDate : null,
                          EndDate = (u != null) ? u.EndDate : null,
                          Emp = (from ab in db.Approvals
                                 join bb in db.Employees on ab.EmployeeId equals bb.EmployeeId
                                 where ab.TransEntry == id && ab.Type == "ProForma"
                                 select new ApprovalViewModel
                                 {
                                     EmpName = bb.FirstName + " " + bb.LastName
                                 }).ToList()
                      }).FirstOrDefault();

            vmodel.PFItem = db.PFItemss.Where(a => a.ProForma == id && a.itemNote != "-:{Bundle_Item}")
            .Select(b => new PFItemViewModel
            {
                ItemUnitPrice = b.ItemUnitPrice,
                ItemQuantity = b.ItemQuantity,
                ItemSubTotal = b.ItemSubTotal,
                ItemDiscount = b.ItemDiscount,
                ItemTax = b.ItemTax,
                itemNote = b.itemNote,
                ItemTaxAmount = b.ItemTaxAmount,
                ItemTotalAmount = b.ItemTotalAmount,
                ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault(),
                PartNumber = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.PartNumber).FirstOrDefault(),
                bundleitem = (from ab in db.PFItemss
                              join bb in db.Items on ab.Item equals bb.ItemID
                              join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                              from cb in primary.DefaultIfEmpty()
                              join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                              from bd in second.DefaultIfEmpty()
                              where ab.ProForma == id && ab.itemNote == "-:{Bundle_Item}"
                              && b.Item == ab.ItemDiscount
                              select new ItemDetailViewModel
                              {
                                  ItemCode = bb.ItemCode,
                                  ItemName = bb.ItemName,
                                  ItemUnit = cb.ItemUnitName,
                                  ItemQuantity = ab.ItemQuantity,
                              }).ToList()
            }).ToList();
            vmodel.PFbs = db.PFBillSundrys.Where(a => a.ProForma == id)
         .Select(b => new PFBillSundryViewModel
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
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "ProForma" && a.Status == Status.active).ToList();


            return View(vmodel);
        }

        [QkAuthorize(Roles = "Dev,Delete Pro Forma")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Delete
            var userpermission = User.IsInRole("All Pro Forma Entry");
            var UserId = User.Identity.GetUserId();
            ProForma PFen = db.ProFormas.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.ProFormaId == id).FirstOrDefault();

            if (PFen == null)
            {
                return NotFound();
            }
            return PartialView(PFen);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete Pro Forma")]
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
                stat = DeletePF(id);
                msg = "Successfully deleted ProForma.";
            }
            #region Old Code
            ////var SET = db.AccountsTransactions.Where(a => a.Id == id).FirstOrDefault();
            #endregion

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Pro Forma")]
        public ActionResult DeleteAllProForma(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteProForma(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + " ProForma.", true);
            return RedirectToAction("Index", "ProForma");
        }

        private Boolean DeleteProForma(long saleId)
        {
            var Msg = chkDeleteWithMsg(saleId);
            if (Msg != null)
            {
                return false;
            }
            else
            {
                return DeletePF(saleId);
            }
        }

        private Boolean DeletePF(long pId)
        {
            var UserId = User.Identity.GetUserId();
            ProForma PFen = db.ProFormas.Find(pId);
            var PFItem = db.PFItemss.Where(a => a.PFItemsId == pId);
            var PFBs = db.PFBillSundrys.Where(a => a.ProForma == pId).FirstOrDefault();
            var customerId = db.ProFormas.Where(a => a.ProFormaId == pId).Select(a => a.Customer).FirstOrDefault();


            if (PFItem != null)
            {
                db.PFItemss.RemoveRange(db.PFItemss.Where(a => a.ProForma == pId));

            }
            if (PFBs != null)
            {
                db.PFBillSundrys.RemoveRange(db.PFBillSundrys.Where(a => a.ProForma == pId));
            }
            var rec = db.Receipts.Where(a => a.Reference == pId && a.RefType == "ProForma").FirstOrDefault();
            if (rec != null)
            {
                db.Receipts.RemoveRange(db.Receipts.Where(a => a.Reference == pId && a.RefType == "ProForma"));
            }

            var ConPro = db.ConvertTransactionss.Where(a => a.To == pId && a.ConvertTo == "ProForma").FirstOrDefault();
            if (ConPro != null)
            {
                db.ConvertTransactionss.RemoveRange(db.ConvertTransactionss.Where(a => a.To == pId && a.ConvertTo == "ProForma"));
            }
            var HireItem = db.HireDetails.Where(a => a.Reference == pId && a.Section == "Proforma").FirstOrDefault();
            if (HireItem != null)
            {
                db.HireDetails.RemoveRange(db.HireDetails.Where(a => a.Reference == pId && a.Section == "Proforma"));

            }

            var appr = db.Approvals.Where(a => a.TransEntry == pId && a.Type == "ProForma").FirstOrDefault();
            if (appr != null)
            {
                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == pId && a.Type == "ProForma"));
            }
            var app = db.ApprovalUpdates.Where(a => a.TransEntry == pId && a.Type == "ProForma").FirstOrDefault();
            if (app != null)
            {
                db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == pId && a.Type == "ProForma"));
            }
            db.ProFormas.Remove(PFen);
            ///*********** Delete from AttachmentDocuments Table *********************/

            ////List all the documents attached corresponding to the JournalId
            //        //To remove the attached file from folder
            //        string FullPath = LegacyWeb.MapPath("~/uploads/ProformaDocuments/" + DocumentLists.ElementAt(i).FileName);


            //        //To remove the attached file from server
            ///***********************************************************************/


            bool delete = com.DeleteAllAccountTransaction("ProForma", pId);
            bool deletepay = com.DeleteAllAccountTransaction("ProForma Payment", pId);
            db.SaveChanges();
            com.addlog(LogTypes.Deleted, UserId, "ProForma", "ProForma", findip(), PFen.ProFormaId, "Successfully Deleted Pro Forma Entry");

            return true;
        }

        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            var Ext1 = db.ConvertTransactionss.Where(x => x.From == id && x.ConvertFrom == "ProForma" && x.ConvertTo == "Sale").FirstOrDefault();
            var Ext2 = db.ConvertTransactionss.Where(x => x.From == id && x.ConvertFrom == "ProForma" && x.ConvertTo == "DVNote").FirstOrDefault();
            if (Ext1 != null)
            {
                var inv = db.SalesEntrys.Where(x => x.SalesEntryId == Ext1.To).Select(z => z.BillNo).FirstOrDefault();
                msg = "This Invoice was Converted to Invoice: " + inv + ".";
            }
            else if (Ext2 != null)
            {
                var inv = db.Deliverynotes.Where(x => x.DeliverynoteId == Ext2.To).Select(z => z.BillNo).FirstOrDefault();
                msg = "This Invoice was Converted to Delivery Note: " + inv + ".";
            }
            else
            {
                msg = null;
            }

            return msg;
        }



        private long GetPFNo(SaleType type)
        {
            Int64 PFNo = 0;
            string prefix = (type == SaleType.Hire) ? "HireProforma" : "ProForma";
            Int32 number = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.number).FirstOrDefault();
            if ((db.ProFormas.Where(a => a.SaleType == type).Select(p => p.PFNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                if (number == 0)
                {
                    PFNo = 1;
                }
                else
                {
                    PFNo = number;
                }
            }
            else
            {
                PFNo = db.ProFormas.Max(p => p.PFNo + 1);
            }

            return PFNo;
        }
        private string InvoiceNo(Int64 PFNo = 0, string billNo = null, string section = null)
        {
            string prefix = (section == "Hire") ? "HireProforma" : "ProForma";
            var companyPrefix = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.number).FirstOrDefault();
            SaleType type = (section != "Hire") ? SaleType.Sale : SaleType.Hire;
            if (billNo == null)
            {
                if ((db.ProFormas.Where(q => q.SaleType == type).Select(p => p.PFNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    PFNo = db.ProFormas.Where(q => q.SaleType == type).Max(p => p.PFNo + 1);
                    billNo = companyPrefix + PFNo;
                    if (BillExist(billNo))
                    {
                        billNo = InvoiceNo(PFNo, billNo, section);
                    }
                }
            }
            else
            {
                PFNo = PFNo + 1;
                billNo = companyPrefix + PFNo;
                if (BillExist(billNo))
                {
                    billNo = InvoiceNo(PFNo, billNo, section);
                }

            }
            return billNo;
        }
        private bool BillExist(string PFNo)
        {
            var Exists = db.ProFormas.Any(c => c.BillNo == PFNo);
            bool res = (Exists) ? true : false;
            return res;
        }
        [HttpPost]
        public ActionResult GetHireInvoiceNum(string hiretype)
        {
            string hirerate = (hiretype == "Hire") ? InvoiceNo(0, null, hiretype) : InvoiceNo();
            return Json(hirerate);
        }
        [HttpGet]
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "ProForma" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
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

            var MR = db.ProFormas.Where(a => a.ProFormaId == id).FirstOrDefault();

            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "ProForma").OrderByDescending(a => a.CreatedDate).FirstOrDefault();

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
                AppUp.Type = "ProForma";

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
                            join d in db.ProFormas on b.TransEntry equals d.ProFormaId into team
                            from d in team.DefaultIfEmpty()
                            join e in db.Employees on b.RequestBy equals e.UserId into emp
                            from e in emp.DefaultIfEmpty()
                            join u in db.Users on d.CreatedBy equals u.Id into req
                            from u in req.DefaultIfEmpty()
                            where b.TransEntry == MCId && b.Type == "ProForma"
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

        //------------------Uploading the attachment Files-------------//
        public ActionResult UploadFiles()
        {
            if (Request.Form.Files.Count > 0)
            {
                string SaleId = Request.Form.GetValues("id").First();
                long SId = 0;

                if (SaleId.Contains("undefined"))
                {
                    var LastId = db.ProFormas.OrderByDescending(a => a.ProFormaId).FirstOrDefault();
                    SId = LastId.ProFormaId;
                }
                else
                {
                    SId = Convert.ToInt64(SaleId);
                }

                try
                {
                    IFormFileCollection files = Request.Form.Files;

                    if (files.Count > 0)
                    {
                        string path = LegacyWeb.MapPath("~/uploads/ProformaDocuments/");

                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        for (int i = 0; i < files.Count; i++)
                        {
                            IFormFile file = files[i];

                            if (file.Length > 0)
                            {
                                var fileCount = db.AttachmentDocuments.Select(a => a.DocumentID).AsEnumerable().DefaultIfEmpty(0).Max();

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
                                    thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/Proformadocuments/"), thumbName);

                                    resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/Proformadocuments/"), resizeName);
                                    newFName = "resize_" + newFName;
                                    FStatus = Status.inactive;
                                }
                                else
                                {
                                    var commonfilename = "Docs-Thump.png";
                                }
                                string Realname = newName;
                                newName = Path.Combine(LegacyWeb.MapPath("~/uploads/Proformadocuments/"), newName);
                                file.SaveAs(newName);

                                var PODocument = new AttachmentDocuments
                                {
                                    TransactionID = SId,
                                    TransactionType = "Proforma",
                                    FileName = Realname,
                                    Status = FStatus,
                                    CreatedDate = Convert.ToDateTime(System.DateTime.Now)
                                };
                                db.AttachmentDocuments.Add(PODocument);
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
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/ProformaDocuments/"), resizeName);
                                        thumbs.Save(resizeName);
                                    }
                                    else
                                    {
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/ProformaDocuments/"), resizeName);
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

        //------------------ End of Uploading the attachment Files-------------//

        //For Delete images from folder
        public JsonResult ImageDelete(long key)
        {
            //To remove the attached file(single row) from database
            AttachmentDocuments Document = db.AttachmentDocuments.Find(key);
            db.AttachmentDocuments.Remove(Document);
            db.SaveChanges();

            //To remove the attached file from folder
            string fullpath = LegacyWeb.MapPath("~/uploads/ProformaDocuments/" + Document.FileName);

            if (System.IO.File.Exists(fullpath))
            {
                System.IO.File.Delete(fullpath);
            }
            string fullPaththumb = LegacyWeb.MapPath("~/uploads/ProformaDocuments/" + "thumb_" + Document.FileName);
            if (System.IO.File.Exists(fullPaththumb))
            {
                System.IO.File.Delete(fullPaththumb);
            }
            string fullPathresize = LegacyWeb.MapPath("~/uploads/ProformaDocuments/" + "resize_" + Document.FileName);
            if (System.IO.File.Exists(fullPathresize))
            {
                System.IO.File.Delete(fullPathresize);
            }
            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "Proforma", "AttachmentDocuments", findip(), Document.DocumentID, "Proforma Deleted Successfully");

            bool status = true;
            string message = "Successfully deleted Proforma attachment details.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = status, message = message, Id = key } };
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
