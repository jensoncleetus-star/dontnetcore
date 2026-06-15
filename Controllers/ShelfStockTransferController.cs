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
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

using Microsoft.AspNetCore.Http;

namespace QuickSoft.Controllers
{
    public class ShelfStockTransferController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ShelfStockTransferController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: ShelfStockTransfer
        public ActionResult Index()
        {
            var mcs = db.MCs.Select(s => new
            {
                McId = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.MCbag = QkSelect.List(mcs, "McId", "Name");

            var MlaSTran = db.EnableSettings.Where(a => a.EnableType == "MLASTran").FirstOrDefault();
            var MlaSTrans = MlaSTran != null ? MlaSTran.Status : Status.inactive;
            ViewBag.MLASTran = MlaSTrans; ;

            var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
            var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;
            ViewBag.StockTrnsfrUpdate = brcheck;

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindStkTrans").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

            return View();
        }

        [RedirectingAction]
        [HttpPost]
        public ActionResult GetShelfStockTransfer(string Voucher, string FromDate, string ToDate, long? MFrom, long? MTo)
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            DateTime? fdate = null;
            DateTime? tdate = null;
            if (FromDate != "")
            {
                fdate = DateTime.Parse(FromDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (ToDate != "")
            {
                tdate = DateTime.Parse(ToDate, new CultureInfo("en-GB").DateTimeFormat);
            }

            var UserId = User.Identity.GetUserId();
            var today = DateTime.Now;
            var editableDay = DateTime.Now;
            var userEditDays = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.stkdays).FirstOrDefault();
            var userEdit = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.id).FirstOrDefault();
            var tem = 0;
            if (userEditDays == 0 && userEdit != 0)
            {
                editableDay = today.AddYears(-10);
            }
            else if (userEdit == 0)
            {
                tem = 1;

            }
            else
            {
                editableDay = today.AddDays(-userEditDays);
            }


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            ApprovalStatus AppSt = new ApprovalStatus();
          
            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit Sales Entry");
            var uDelete = User.IsInRole("Delete Sales Entry");

            var v = (from a in db.ShelfStockTransfers
                     join b in db.rackmaterialcentres on a.FromRackMcId equals b.rackmcid into temp1
                     from b in temp1.DefaultIfEmpty()
                     join c in db.rackmaterialcentres on a.ToRackMcId equals c.rackmcid into temp2
                     from c in temp2.DefaultIfEmpty()
                     join d in db.Users on a.createdBy equals d.Id into temp3
                     from d in temp3.DefaultIfEmpty()
                     join e in db.Racks on b.rackid equals e.RackId into temp4
                     from e in temp4.DefaultIfEmpty()
                     join f in db.Shelves on b.shelfid equals f.ShelfId into temp5
                     from f in temp5.DefaultIfEmpty()
                     join g in db.Racks on c.rackid equals g.RackId into temp6
                     from g in temp6.DefaultIfEmpty()
                     join h in db.Shelves on c.shelfid equals h.ShelfId into temp7
                     from h in temp7.DefaultIfEmpty()
                     join i in db.MCs on b.mcid equals i.MCId into temp8
                     from i in temp8.DefaultIfEmpty()
                     join j in db.MCs on c.mcid equals j.MCId into temp9
                     from j in temp9.DefaultIfEmpty()
                         //let app = db.Approvals.Where(x => x.TransEntry == a.Id && x.Type == "StockTransfer").Select(x => x.EmployeeId).ToList()
                         //let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.Id && x.Type == "StockTransfer").Select(x => x.ApprovalStatus).ToList()
                         //let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.Id && x.Type == "StockTransfer").GroupBy(l => l.ApprovedBy)
                         //                   .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                         //                   .ToList().Select(x => x.ApprovalStatus).ToList()

                     where
                     (Voucher == "" || a.VoucherNo == Voucher) &&
                     (FromDate == "" || EF.Functions.DateDiffDay(a.createdDate, fdate) <= 0) &&
                     (ToDate == "" || EF.Functions.DateDiffDay(a.createdDate, tdate) >= 0) &&
                     (MFrom == 0 || MFrom == null || i.MCId == MFrom) &&
                     (MTo == 0 || MTo == null ||  j.MCId == MTo)
                     select new
                     {
                         //validornot = tem != 1 && (EF.Functions.DateDiffDay(a.Date, editableDay) <= 0 && EF.Functions.DateDiffDay(a.Date, today) >= 0) ? "valid" : "invalid",
                         //userEditDays = userEditDays,
                         Id = a.shelftransferId,
                         Voucher = a.VoucherNo,
                         Date = a.createdDate,
                         fromrack = e.RackName,
                         fromshelf = f.shelfName,
                         fromMC=i.MCName,
                         torack = g.RackName,
                         toshelf=h.shelfName,
                         toMC=j.MCName,
                         User = d.UserName,
                         a.createdDate,
                         a.transactionType,
                     }).ToList().
            Select(b => new
            {
               
                b.Id,
                b.Voucher,
                b.Date,
                b.fromrack,
                b.fromshelf,
                b.fromMC,
                b.torack,
                b.toshelf,
                b.toMC,
                b.User,
                b.createdDate,
                b.transactionType,
            }).ToList().Select(o => new
            {
                o.Id,
                o.Voucher,
                o.Date,
                o.fromrack,
                o.fromshelf,
                o.fromMC,
                o.torack,
                o.toshelf,
                o.toMC,
                o.User,
                o.createdDate,
                o.transactionType
            });
            
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.Voucher.ToString().ToLower().Contains(search.ToLower()));
            }
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        public ActionResult Create(long? Rid, long? Sid, long? MCid)
        {
            ShelfTransferViewModel Quotentry = new ShelfTransferViewModel();
            Quotentry.createdDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
            ViewBag.rk = QkSelect.List(
                   new List<SelectListItem>
                   {
             new SelectListItem { Selected = false, Text = null, Value = null},
                   }, "Value", "Text", 1);
            ViewBag.slf = QkSelect.List(
           new List<SelectListItem>
           {
             new SelectListItem { Selected = false, Text = null, Value = null},
           }, "Value", "Text", 1);
            ViewBag.mc = QkSelect.List(
            new List<SelectListItem>
            {
             new SelectListItem { Selected = false, Text = null, Value = null},
            }, "Value", "Text", 1);
            return View(Quotentry);
        }
        public ActionResult Edit(long? id)
        {
            ViewBag.Id = id;

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
            ViewBag.image = (from b in db.quotationdocuments
                             join c in db.Quotations on b.quotationID equals c.QuotationId
                             where c.QuotationId == id
                             select new quotationdocumentviewmodel
                             {
                                 qutid = b.qutid,
                                 quotationID = b.quotationID,
                                 FileName = b.FileName,
                             }).ToList();
            var userpermission = User.IsInRole("All Quotation Entry");
            var UserId = User.Identity.GetUserId();

            ShelfStockTransferViewModel vmodel = new ShelfStockTransferViewModel();
            var cust = db.Customers
                .Select(s => new
                {
                    CustomerID = s.CustomerID,
                    CustomerDetails = s.CustomerCode + " - " + s.CustomerName
                }).ToList();
            ViewBag.Custer = QkSelect.List(cust, "CustomerID", "CustomerDetails");
            var leads = db.Customers
    .Where(o => o.Type == CRMCustomerType.Leads)
  .Select(s => new
  {
      CustomerID = s.CustomerID,
      CustomerDetails = s.CustomerCode + " - " + s.CustomerName
  }).ToList();
            ViewBag.leads = QkSelect.List(leads, "CustomerID", "CustomerDetails");

            var use = db.Employees
                             .Select(s => new
                             {
                                 ID = s.EmployeeId,
                                 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                             })
                             .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            var quot = db.QuotationTypes
                             .Select(s => new
                             {
                                 ID = s.QuotId,
                                 Name = s.QuotType
                             })
                             .ToList();
            ViewBag.Quot = QkSelect.List(quot, "ID", "Name");


            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjCheck = ProjectCheck;

            var EnableCRM = db.EnableSettings.Where(a => a.EnableType == "EnableCRM").FirstOrDefault();
            var EnableCRMs = EnableCRM != null ? EnableCRM.Status : Status.inactive;
            ViewBag.EnableCRM = EnableCRMs;
            var pr = db.Projects
                            .Select(s => new
                            {
                                ID = s.ProjectId,
                                Name = s.ProCode + "-" + s.ProjectName
                            })
                            .ToList();
            ViewBag.Proj = QkSelect.List(pr, "ID", "Name");
            var tsk = db.ProTasks
                .Select(s => new
                {
                    ID = s.ProTaskId,
                    Name = s.TaskName
                })
                .ToList();
            ViewBag.Templates = QkSelect.List(
            new List<SelectListItem>
            {
              new SelectListItem { Selected = false},
            }, "Value", "Text", 1);


            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "Quotation").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaQuot = db.EnableSettings.Where(a => a.EnableType == "MLAQuot").FirstOrDefault();
            var MlaQuots = MlaQuot != null ? MlaQuot.Status : Status.inactive;
            ViewBag.MLAQuot = MlaQuots;

            vmodel = (from a in db.ShelfStockTransfers
                      join b in db.rackmaterialcentres on a.ToRackMcId equals b.rackmcid into temp1
                      from b in temp1.DefaultIfEmpty()
                      join c in db.rackmaterialcentres on a.FromRackMcId equals c.rackmcid into temp2
                      from c in temp2.DefaultIfEmpty()
                      where a.shelftransferId == id
                      select new ShelfStockTransferViewModel
                      {
                          shelftransferId = a.shelftransferId,
                          VoucherNo = a.VoucherNo,
                          Torackid = b.rackid,
                          Toshelfid = b.shelfid,
                          Tomcid = b.mcid,
                          Fromrackid = c.rackid,
                          Fromshelfid = c.shelfid,
                          Frommcid = c.mcid,
                          createdDate=a.createdDate,
                          transactionType=a.transactionType,
                      }).FirstOrDefault();
            companySet();

            ViewBag.transtyp = vmodel.transactionType;
            ViewBag.preEntry = db.Quotations.Where(a => a.QuotationId < id && (userpermission == true || a.CreatedUserId == UserId)).Select(a => a.QuotationId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.Quotations.Where(a => a.QuotationId > id && (userpermission == true || a.CreatedUserId == UserId)).Select(a => a.QuotationId).DefaultIfEmpty().Min();

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            var mailcheck = mail != null ? mail.Status : Status.inactive;
            ViewBag.CheckMail = mailcheck;

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

            var EditPermission = User.IsInRole("Disable Quot Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "Quotation", UserId);


            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            


            //dummy table operations
            var DItem = db.DummyQuotItems.Where(a => a.Quotation == id).FirstOrDefault();
            var QItem = db.QuotationItems.Where(a => a.Quotation == id).FirstOrDefault();
            if (QItem == null && DItem != null)
            {
                var DItems = db.DummyQuotItems.Where(a => a.Quotation == id).ToList();
                foreach (var arr in DItems)
                {
                    //add to se-item table
                    QuotationItem QItems = new QuotationItem();
                    QItems.ItemUnit = arr.ItemUnit;
                    QItems.ItemUnitPrice = arr.ItemUnitPrice;
                    QItems.ItemQuantity = arr.ItemQuantity;
                    QItems.ItemSubTotal = arr.ItemSubTotal;
                    QItems.ItemDiscount = arr.ItemDiscount;
                    QItems.ItemTax = arr.ItemTax;
                    QItems.ItemTaxAmount = arr.ItemTaxAmount;
                    QItems.ItemTotalAmount = arr.ItemTotalAmount;
                    QItems.ItemNote = arr.ItemNote;
                    QItems.Quotation = arr.Quotation;
                    QItems.Item = arr.Item;
                    db.QuotationItems.Add(QItems);
                    db.SaveChanges();
                }

                db.DummyQuotItems.RemoveRange(db.DummyQuotItems.Where(a => a.Quotation == id));
                db.SaveChanges();
                QuickSoft.Helpers.DocumentTotals.RecomputeQuotation(db, id); // forward-correctness: header = SUM(lines)
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

           
            var mcs = db.Racks.Select(s => new
            {
                rId = s.RackId,
                Name = s.RackName
            }).ToList();
            ViewBag.rk = QkSelect.List(mcs, "rId", "Name");

            var mcs2 = db.Shelves.Select(s => new
            {
                sId = s.ShelfId,
                Name = s.shelfName
            }).ToList();
            ViewBag.slf = QkSelect.List(mcs2, "sId", "Name");

            var mcs3 = db.MCs.Select(s => new
            {
                mcId = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.mc = QkSelect.List(mcs3, "mcId", "Name");
            var mcs4 = db.Racks.Select(s => new
            {
                rId = s.RackId,
                Name = s.RackName
            }).ToList();
            ViewBag.rk2 = QkSelect.List(mcs4, "rId", "Name");

            var mcs5 = db.Shelves.Select(s => new
            {
                sId = s.ShelfId,
                Name = s.shelfName
            }).ToList();
            ViewBag.slf2 = QkSelect.List(mcs5, "sId", "Name");

            var mcs6 = db.MCs.Select(s => new
            {
                mcId = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.mc2 = QkSelect.List(mcs6, "mcId", "Name");

            return View(vmodel);
        }
        [HttpGet]
        public ActionResult GetItems(long QuoteEntryID)
        {
            var ConD = (from a in db.SSTItems
                        join b in db.Items on a.item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.shelfTransfer == QuoteEntryID 
                        orderby a.STItemId
                        select new
                        {
                            Item=a.item,
                            ItemQuantity=a.itemQuantity,
                            ItemUnit=a.itemUnit,
                            ItemUnitPrice=0.00,
                            ItemTax=0.00,
                            ItemSubTotal=0.00,
                            ItemTaxAmount=0.00,
                            ItemDiscount=0.00,
                            note = "",
                            ItemNote = "",
                            ItemTotalAmount=0.00,
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
        public ActionResult ShelfStockTransferCreate()
        {
            ShelfStockTransferViewModel Quotentry = new ShelfStockTransferViewModel();
            Quotentry.createdDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
            Quotentry.VoucherNo = InvoiceNo2();
            ViewBag.rk = QkSelect.List(
                   new List<SelectListItem>
                   {
             new SelectListItem { Selected = false, Text = null, Value = null},
                   }, "Value", "Text", 1);
            ViewBag.slf = QkSelect.List(
           new List<SelectListItem>
           {
             new SelectListItem { Selected = false, Text = null, Value = null},
           }, "Value", "Text", 1);
            ViewBag.mc = QkSelect.List(
            new List<SelectListItem>
            {
             new SelectListItem { Selected = false, Text = null, Value = null},
            }, "Value", "Text", 1);

            ViewBag.rk2 = QkSelect.List(
                   new List<SelectListItem>
                   {
             new SelectListItem { Selected = false, Text = null, Value = null},
                   }, "Value", "Text", 1);
            ViewBag.slf2 = QkSelect.List(
           new List<SelectListItem>
           {
             new SelectListItem { Selected = false, Text = null, Value = null},
           }, "Value", "Text", 1);
            ViewBag.mc2 = QkSelect.List(
            new List<SelectListItem>
            {
             new SelectListItem { Selected = false, Text = null, Value = null},
            }, "Value", "Text", 1);
            return View(Quotentry);
        }
        private string InvoiceNo2(Int64 PENo = 0, string billNo = null, string section = null)
        {
            string prefix = (section == "Hire") ? "CrossHireInvoices" : "WorkCompletion";
            var companyPrefix = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.prefix).FirstOrDefault();
            PurchaseHireType type = (section != "Hire") ? PurchaseHireType.Purchase : PurchaseHireType.CrossHire;

            if (billNo == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.number).FirstOrDefault();
                var num = db.ShelfStockTransfers.OrderByDescending(p => p.shelftransferId).Select(p => p.VoucherNo).FirstOrDefault();
                if (num == null)
                {
                    billNo = Convert.ToString(1);
                }
                else
                {
                    var num2 = Convert.ToInt64(num);
                    var num3 = num2 + 1;
                    billNo = Convert.ToString(num3);
                }
            }
            else
            {


            }
            return billNo;
        }
        [HttpPost]
        public JsonResult Create(string[][] array, string[] quotdata, string action, ICollection<QtBillSundry> bsmodel)
        {




            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                string check = "true";
                var UserId = User.Identity.GetUserId();
                WorkCompletion Quoentry = new WorkCompletion();
                var rackID = Convert.ToInt64(quotdata[1]);
                var shelfID = Convert.ToInt64(quotdata[2]);
                var mcID = Convert.ToInt64(quotdata[3]);
                var rackmcID = db.rackmaterialcentres.Where(a => a.rackid == rackID && a.shelfid == shelfID && a.mcid == mcID).Select(b => b.rackmcid).FirstOrDefault();
                var Exists = db.ShelfStockTransfers.Any(c => c.ToRackMcId == rackmcID);
                var ExistID = db.ShelfStockTransfers.Where(a => a.ToRackMcId == rackmcID).Select(a => a.shelftransferId).FirstOrDefault();
                if (Exists)
                {
                    var sstID= Convert.ToInt64(ExistID);
                    ShelfStockTransfer shelfST = db.ShelfStockTransfers.Find(sstID);
                    shelfST.createdDate= DateTime.Parse(quotdata[0], new CultureInfo("en-GB"));
                    shelfST.createdBy = UserId;
                    shelfST.ToRackMcId = rackmcID;
                    shelfST.transactionType = "Open";
                    db.Entry(shelfST).State = EntityState.Modified;
                    db.SaveChanges();
                    var itm = db.SSTItems.Where(a => a.shelfTransfer == sstID).FirstOrDefault();
                    if (itm != null)
                    {
                        db.SSTItems.RemoveRange(db.SSTItems.Where(a => a.shelfTransfer == sstID));
                        db.SaveChanges();
                    }
                    var itm2 = db.shelfstockmovements.Where(a => a.referenceid == sstID).FirstOrDefault();
                    if (itm2 != null)
                    {
                        db.shelfstockmovements.RemoveRange(db.shelfstockmovements.Where(a => a.referenceid == sstID));
                        db.SaveChanges();
                    }
                    foreach (var arr in array)
                    {
                        SSTItem sitem2 = new SSTItem();
                        sitem2.itemUnit = Convert.ToInt64(arr[1]);
                        sitem2.itemQuantity = (arr[2] == "") ? 0 : Convert.ToDecimal(arr[2]);
                        sitem2.shelfTransfer = sstID;
                        sitem2.item = Convert.ToInt32(arr[0]);
                        db.SSTItems.Add(sitem2);
                        db.SaveChanges();

                        shelfstockmovement movement3 = new shelfstockmovement();
                        movement3.rackmciid = rackmcID;
                        movement3.referenceid = sstID;
                        movement3.purpose = "Open";
                        movement3.itemid = Convert.ToInt32(arr[0]);
                        movement3.unitid = Convert.ToInt64(arr[1]);
                        movement3.qty = (arr[2] == "") ? 0 : Convert.ToDecimal(arr[2]);
                        movement3.createdby = UserId;
                        movement3.createddate = DateTime.Parse(quotdata[0], new CultureInfo("en-GB"));
                        db.shelfstockmovements.Add(movement3);
                        db.SaveChanges();
                    }

                }
                else
                {
                    ShelfStockTransfer SST = new ShelfStockTransfer();
                    SST.createdDate = DateTime.Parse(quotdata[0], new CultureInfo("en-GB"));
                    SST.createdBy = UserId;
                    SST.ToRackMcId = rackmcID;
                    SST.transactionType = "Open";
                    db.ShelfStockTransfers.Add(SST);
                    db.SaveChanges();
                    ExistID = SST.shelftransferId;
                    foreach (var arr in array)
                    {
                        SSTItem sitem = new SSTItem();
                        sitem.itemUnit = Convert.ToInt64(arr[1]);
                        sitem.itemQuantity = (arr[2] == "") ? 0 : Convert.ToDecimal(arr[2]);
                        sitem.shelfTransfer = ExistID;
                        sitem.item = Convert.ToInt32(arr[0]);
                        db.SSTItems.Add(sitem);
                        db.SaveChanges();

                        shelfstockmovement movement4 = new shelfstockmovement();
                        movement4.rackmciid = rackmcID;
                        movement4.referenceid = ExistID;
                        movement4.purpose = "Open";
                        movement4.itemid = Convert.ToInt32(arr[0]);
                        movement4.unitid = Convert.ToInt64(arr[1]);
                        movement4.qty = (arr[2] == "") ? 0 : Convert.ToDecimal(arr[2]);
                        movement4.createdby = UserId;
                        movement4.createddate = DateTime.Parse(quotdata[0], new CultureInfo("en-GB"));
                        db.shelfstockmovements.Add(movement4);
                        db.SaveChanges();
                    }
                }


                Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
                msg = "Successfully submitted Shelf Stock Transfer.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, qtnid = ExistID } };

            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }
        [HttpPost]
        public JsonResult Edit(string[][] array, string[] quotdata, string action, ICollection<QtBillSundry> bsmodel)
        {
            bool stat = false;
            string msg;
            bool temp = Convert.ToBoolean(quotdata[7]);
            if (quotdata[9] != null)
            {
                Int64 sstId = Convert.ToInt64(quotdata[9]);
                var transactionType = Convert.ToString(quotdata[10]);
                if (ModelState.IsValid)
                {
                    if (transactionType == "StockTransfer")
                    {
                        ShelfStockTransfer sstentry = db.ShelfStockTransfers.Find(sstId);
                        SSTItem sitms = db.SSTItems.Find(sstId);
                        var UserId = User.Identity.GetUserId();
                    var rackID = Convert.ToInt64(quotdata[1]);
                    var shelfID = Convert.ToInt64(quotdata[2]);
                    var mcID = Convert.ToInt64(quotdata[3]);
                    var rackID2 = Convert.ToInt64(quotdata[4]);
                    var shelfID2 = Convert.ToInt64(quotdata[5]);
                    var mcID2 = Convert.ToInt64(quotdata[6]);
                    var FromrackmcID = db.rackmaterialcentres.Where(a => a.rackid == rackID && a.shelfid == shelfID && a.mcid == mcID).Select(b => b.rackmcid).FirstOrDefault();
                    var TorackmcID = db.rackmaterialcentres.Where(a => a.rackid == rackID2 && a.shelfid == shelfID2 && a.mcid == mcID2).Select(b => b.rackmcid).FirstOrDefault();
                    sstentry.createdBy = UserId;
                    sstentry.createdDate = DateTime.Parse(quotdata[0], new CultureInfo("en-GB"));
                    sstentry.VoucherNo = Convert.ToString(quotdata[8]);
                    sstentry.FromRackMcId = FromrackmcID;
                    sstentry.ToRackMcId = TorackmcID;
                    sstentry.transactionType = "Stock Transfered";
                    db.Entry(sstentry).State = EntityState.Modified;
                    db.SaveChanges();
                    var itm = db.SSTItems.Where(a => a.shelfTransfer == sstId).FirstOrDefault();
                    if (itm != null)
                    {
                        db.SSTItems.RemoveRange(db.SSTItems.Where(a => a.shelfTransfer == sstId));
                        db.SaveChanges();
                    }
                    var itm2 = db.shelfstockmovements.Where(a => a.referenceid == sstId).FirstOrDefault();
                    if (itm2 != null)
                    {
                        db.shelfstockmovements.RemoveRange(db.shelfstockmovements.Where(a => a.referenceid == sstId));
                        db.SaveChanges();
                    }
                    foreach (var arr in array)
                    {
                        SSTItem sitem2 = new SSTItem();
                        sitem2.itemUnit = Convert.ToInt64(arr[1]);
                        sitem2.itemQuantity = (arr[2] == "") ? 0 : Convert.ToDecimal(arr[2]);
                        sitem2.shelfTransfer = sstId;
                        sitem2.item = Convert.ToInt32(arr[0]);
                        db.SSTItems.Add(sitem2);
                        db.SaveChanges();

                        shelfstockmovement movement = new shelfstockmovement();
                        movement.rackmciid = FromrackmcID;
                        movement.referenceid = sstId;
                        movement.purpose = "Stock Transfered";
                        movement.itemid = Convert.ToInt32(arr[0]);
                        movement.unitid = Convert.ToInt64(arr[1]);
                        movement.qty = (arr[2] == "") ? 0 : Convert.ToDecimal(arr[2]);
                        movement.createdby = UserId;
                        movement.createddate = DateTime.Parse(quotdata[0], new CultureInfo("en-GB"));
                        db.shelfstockmovements.Add(movement);
                        db.SaveChanges();

                        shelfstockmovement movement2 = new shelfstockmovement();
                        movement2.rackmciid = TorackmcID;
                        movement2.referenceid = sstId;
                        movement2.purpose = "Stock Received";
                        movement2.itemid = Convert.ToInt32(arr[0]);
                        movement2.unitid = Convert.ToInt64(arr[1]);
                        movement2.qty = (arr[2] == "") ? 0 : Convert.ToDecimal(arr[2]);
                        movement2.createdby = UserId;
                        movement2.createddate = DateTime.Parse(quotdata[0], new CultureInfo("en-GB"));
                        db.shelfstockmovements.Add(movement2);
                        db.SaveChanges();
                    }
                    msg = "Successfully submitted Shelf Stock Transfer.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, qtnid = sstId } };
                    }
                    else
                    {
                        ShelfStockTransfer sstentry2 = db.ShelfStockTransfers.Find(sstId);
                        SSTItem sitms2 = db.SSTItems.Find(sstId);
                        var UserId = User.Identity.GetUserId();
                        var rackID2 = Convert.ToInt64(quotdata[4]);
                        var shelfID2 = Convert.ToInt64(quotdata[5]);
                        var mcID2 = Convert.ToInt64(quotdata[6]);
                        var TorackmcID = db.rackmaterialcentres.Where(a => a.rackid == rackID2 && a.shelfid == shelfID2 && a.mcid == mcID2).Select(b => b.rackmcid).FirstOrDefault();
                        sstentry2.createdBy = UserId;
                        sstentry2.createdDate = DateTime.Parse(quotdata[0], new CultureInfo("en-GB"));
                        sstentry2.VoucherNo = Convert.ToString(quotdata[8]);
                        sstentry2.ToRackMcId = TorackmcID;
                        sstentry2.transactionType = transactionType;
                        db.Entry(sstentry2).State = EntityState.Modified;
                        db.SaveChanges();
                        var itm = db.SSTItems.Where(a => a.shelfTransfer == sstId).FirstOrDefault();
                        if (itm != null)
                        {
                            db.SSTItems.RemoveRange(db.SSTItems.Where(a => a.shelfTransfer == sstId));
                            db.SaveChanges();
                        }
                        var itm2 = db.shelfstockmovements.Where(a => a.referenceid == sstId).FirstOrDefault();
                        if (itm2 != null)
                        {
                            db.shelfstockmovements.RemoveRange(db.shelfstockmovements.Where(a => a.referenceid == sstId));
                            db.SaveChanges();
                        }
                        foreach (var arr in array)
                        {
                            SSTItem sitem2 = new SSTItem();
                            sitem2.itemUnit = Convert.ToInt64(arr[1]);
                            sitem2.itemQuantity = (arr[2] == "") ? 0 : Convert.ToDecimal(arr[2]);
                            sitem2.shelfTransfer = sstId;
                            sitem2.item = Convert.ToInt32(arr[0]);
                            db.SSTItems.Add(sitem2);
                            db.SaveChanges();

                            shelfstockmovement movement = new shelfstockmovement();
                            movement.rackmciid = TorackmcID;
                            movement.referenceid = sstId;
                            movement.purpose = transactionType;
                            movement.itemid = Convert.ToInt32(arr[0]);
                            movement.unitid = Convert.ToInt64(arr[1]);
                            movement.qty = (arr[2] == "") ? 0 : Convert.ToDecimal(arr[2]);
                            movement.createdby = UserId;
                            movement.createddate = DateTime.Parse(quotdata[0], new CultureInfo("en-GB"));
                            db.shelfstockmovements.Add(movement);
                            db.SaveChanges();
                        }
                        msg = "Successfully submitted Shelf Stock Transfer.";
                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, qtnid = sstId } };
                    }
                    
                }
                else
                {
                    stat = false;
                    msg = "Looks like something went wrong. Please check your form.";
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

        [HttpPost]
        public JsonResult ShelfStockTransferCreate(string[][] array, string[] quotdata, string action, ICollection<QtBillSundry> bsmodel)
        {
            bool stat = false;
            string msg;
            bool temp = Convert.ToBoolean(quotdata[7]);
            var transactionType= Convert.ToString(quotdata[9]);
            if (ModelState.IsValid)
            {
                if (transactionType== "StockTransfer")
                {
                    string check = "true";
                    var UserId = User.Identity.GetUserId();
                    var rackID = Convert.ToInt64(quotdata[1]);
                    var shelfID = Convert.ToInt64(quotdata[2]);
                    var mcID = Convert.ToInt64(quotdata[3]);
                    var rackID2 = Convert.ToInt64(quotdata[4]);
                    var shelfID2 = Convert.ToInt64(quotdata[5]);
                    var mcID2 = Convert.ToInt64(quotdata[6]);
                    var FromrackmcID = db.rackmaterialcentres.Where(a => a.rackid == rackID && a.shelfid == shelfID && a.mcid == mcID).Select(b => b.rackmcid).FirstOrDefault();
                    var TorackmcID = db.rackmaterialcentres.Where(a => a.rackid == rackID2 && a.shelfid == shelfID2 && a.mcid == mcID2).Select(b => b.rackmcid).FirstOrDefault();
                    ShelfStockTransfer SST1 = new ShelfStockTransfer();
                    SST1.createdDate = DateTime.Parse(quotdata[0], new CultureInfo("en-GB"));
                    SST1.createdBy = UserId;
                    SST1.VoucherNo = Convert.ToString(quotdata[8]);
                    SST1.FromRackMcId = FromrackmcID;
                    SST1.ToRackMcId = TorackmcID;
                    SST1.transactionType = "Stock Transfered";
                    db.ShelfStockTransfers.Add(SST1);
                    db.SaveChanges();
                    Int64 ExistID = SST1.shelftransferId;
                    foreach (var arr in array)
                    {
                        SSTItem sitem = new SSTItem();
                        sitem.itemUnit = Convert.ToInt64(arr[1]);
                        sitem.itemQuantity = (arr[2] == "") ? 0 : Convert.ToDecimal(arr[2]);
                        sitem.shelfTransfer = ExistID;
                        sitem.item = Convert.ToInt32(arr[0]);
                        db.SSTItems.Add(sitem);
                        db.SaveChanges();

                        shelfstockmovement movement = new shelfstockmovement();
                        movement.rackmciid = FromrackmcID;
                        movement.referenceid = ExistID;
                        movement.purpose = "Stock Transfered";
                        movement.itemid = Convert.ToInt32(arr[0]);
                        movement.unitid = Convert.ToInt64(arr[1]);
                        movement.qty = (arr[2] == "") ? 0 : Convert.ToDecimal(arr[2]);
                        movement.createdby = UserId;
                        movement.createddate = DateTime.Parse(quotdata[0], new CultureInfo("en-GB"));
                        db.shelfstockmovements.Add(movement);
                        db.SaveChanges();

                        shelfstockmovement movement2 = new shelfstockmovement();
                        movement2.rackmciid = TorackmcID;
                        movement2.referenceid = ExistID;
                        movement2.purpose = "Stock Received";
                        movement2.itemid = Convert.ToInt32(arr[0]);
                        movement2.unitid = Convert.ToInt64(arr[1]);
                        movement2.qty = (arr[2] == "") ? 0 : Convert.ToDecimal(arr[2]);
                        movement2.createdby = UserId;
                        movement2.createddate = DateTime.Parse(quotdata[0], new CultureInfo("en-GB"));
                        db.shelfstockmovements.Add(movement2);
                        db.SaveChanges();
                    }



                    msg = "Successfully submitted Shelf Stock Transfer.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, qtnid = ExistID } };
                }
                else
                {
                    string check = "true";
                    var UserId = User.Identity.GetUserId();
                    var rackID = Convert.ToInt64(quotdata[4]);
                    var shelfID = Convert.ToInt64(quotdata[5]);
                    var mcID = Convert.ToInt64(quotdata[6]);
                    var rackmcID = db.rackmaterialcentres.Where(a => a.rackid == rackID && a.shelfid == shelfID && a.mcid == mcID).Select(b => b.rackmcid).FirstOrDefault();
                    ShelfStockTransfer SST = new ShelfStockTransfer();
                    SST.createdDate = DateTime.Parse(quotdata[0], new CultureInfo("en-GB"));
                    SST.createdBy = UserId;
                    SST.VoucherNo = Convert.ToString(quotdata[8]);
                    SST.ToRackMcId = rackmcID;
                    SST.transactionType = transactionType;
                    db.ShelfStockTransfers.Add(SST);
                    db.SaveChanges();
                    Int64 ExistID = SST.shelftransferId;
                    foreach (var arr in array)
                    {
                        SSTItem sitem = new SSTItem();
                        sitem.itemUnit = Convert.ToInt64(arr[1]);
                        sitem.itemQuantity = (arr[2] == "") ? 0 : Convert.ToDecimal(arr[2]);
                        sitem.shelfTransfer = ExistID;
                        sitem.item = Convert.ToInt32(arr[0]);
                        db.SSTItems.Add(sitem);
                        db.SaveChanges();

                        shelfstockmovement movement4 = new shelfstockmovement();
                        movement4.rackmciid = rackmcID;
                        movement4.referenceid = ExistID;
                        movement4.purpose = transactionType;
                        movement4.itemid = Convert.ToInt32(arr[0]);
                        movement4.unitid = Convert.ToInt64(arr[1]);
                        movement4.qty = (arr[2] == "") ? 0 : Convert.ToDecimal(arr[2]);
                        movement4.createdby = UserId;
                        movement4.createddate = DateTime.Parse(quotdata[0], new CultureInfo("en-GB"));
                        db.shelfstockmovements.Add(movement4);
                        db.SaveChanges();
                    }
                    msg = "Successfully submitted Shelf Stock Transfer.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, qtnid = ExistID } };
                }
            }

            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }
        public JsonResult SearchRack(string q, string x, long MCid)
        {
            List<SelectFormat3> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.rackmaterialcentres
                                  join b in db.Racks on a.rackid equals b.RackId into temp1
                                  from b in temp1.DefaultIfEmpty()
                                  where (MCid != 0 && a.mcid == MCid) &&
                                        (q == null || b.RackName.ToLower().Contains(q.ToLower()) || b.RackName.Contains(q))
                                  select new SelectFormat3
                                  {
                                      text = b.RackName,
                                      id = b.RackId
                                  }).OrderBy(b => b.text).Distinct().ToList();
            }
            else
            {
                serialisedJson = (from a in db.rackmaterialcentres
                                  join b in db.Racks on a.rackid equals b.RackId into temp1
                                  from b in temp1.DefaultIfEmpty()
                                  where (a.mcid == MCid)
                                  select new SelectFormat3
                                  {
                                      text = b.RackName,
                                      id = b.RackId
                                  }).OrderBy(b => b.text).Distinct().ToList();

            }

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat3() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }
        public JsonResult SearchShelf(string q, string x, long MCid,long RACKid)
        {
            List<SelectFormat3> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.rackmaterialcentres
                                  join b in db.Shelves on a.shelfid equals b.ShelfId into temp1
                                  from b in temp1.DefaultIfEmpty()
                                  where (MCid != 0 && a.mcid == MCid) &&
                                        (RACKid != 0 && a.rackid == RACKid)&&
                                        (q == null || b.shelfName.ToLower().Contains(q.ToLower()) || b.shelfName.Contains(q))
                                  select new SelectFormat3
                                  {
                                      text = b.shelfName,
                                      id = b.ShelfId
                                  }).OrderBy(b => b.text).Distinct().ToList();
            }
            else
            {
                serialisedJson = (from a in db.rackmaterialcentres
                                  join b in db.Shelves on a.shelfid equals b.ShelfId into temp1
                                  from b in temp1.DefaultIfEmpty()
                                  where (a.mcid == MCid) &&
                                  (RACKid != 0 && a.rackid == RACKid) 
                                  select new SelectFormat3
                                  {
                                      text = b.shelfName,
                                      id = b.ShelfId
                                  }).OrderBy(b => b.text).Distinct().ToList();

            }

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat3() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }
        [QkAuthorize]
        public JsonResult SearchItem(string q, string x, string page, long MCid, long RACKid,long SHELFid)
        {
            var rackID = Convert.ToInt64(RACKid);
            var shelfID = Convert.ToInt64(SHELFid);
            var mcID = Convert.ToInt64(MCid);
            var rackmcID = db.rackmaterialcentres.Where(a => a.rackid == rackID && a.shelfid == shelfID && a.mcid == mcID).Select(b => b.rackmcid).FirstOrDefault();
            var Exists = db.ShelfStockTransfers.Any(c => c.ToRackMcId == rackmcID);
            var ExistID = db.ShelfStockTransfers.Where(a => a.ToRackMcId == rackmcID).Select(a => a.shelftransferId).FirstOrDefault();
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectFormat4> serialisedJson;
            List<SelectFormat4> serialisedJson2;
            string stt = "All";

            string[] searchkey = q.Split(' ');
            string secnd = "";
            string third = "";
            bool checksecnd = false;
            bool thirdcheck = false;
            if (searchkey.Count() > 1)
            {
                secnd = searchkey[1];
                q = searchkey[0];
                checksecnd = true;
            }
            if (searchkey.Count() > 2)
            {
                third = searchkey[2];
                thirdcheck = true;
            }
            var itemID = db.SSTItems.Where(a => a.shelfTransfer == ExistID).Select(a => a.item).ToArray();

            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.Items
                                  where (itemID.Contains(b.ItemID)) &&
                                        (b.ItemName.ToLower().Contains(q.ToLower()) || b.ItemCode.ToLower().Contains(q.ToLower()) || b.Barcode.Contains(q))
                                        && (secnd == "" || b.ItemName.ToLower().Contains(secnd.ToLower()))
                                        && (third == "" || b.ItemName.ToLower().Contains(third.ToLower()))
                                  select new SelectFormat4
                                  {
                                      text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                                      id = b.ItemID,
                                      pprice = b.PurchasePrice,
                                      sprice = b.SellingPrice,
                                      ItemDescription = b.ItemDescription,
                                      ItemImId = b.ItemID,
                                      FileName = db.ItemImages.Where(o => o.ItemID == b.ItemID).Select(o => o.FileName).FirstOrDefault(),
                                  }).OrderBy(b => b.text).ToList();

            }
            else
            {
                serialisedJson = (from b in db.Items
                                  where (itemID.Contains(b.ItemID))
                                  select new SelectFormat4
                                  {
                                      text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                                      id = b.ItemID,
                                      pprice = b.PurchasePrice,
                                      sprice = b.SellingPrice,
                                      ItemDescription = b.ItemDescription,
                                      ItemImId = b.ItemID,
                                      FileName = db.ItemImages.Where(o => o.ItemID == b.ItemID).Select(o => o.FileName).FirstOrDefault(),
                                  }).OrderBy(b => b.text).ToList();

            }//
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat4() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        [HttpGet]
        public JsonResult GetSSTItems(long? RID, long? SID, long? MCID)
        {
            var rackmcID = db.rackmaterialcentres.Where(a => a.rackid == RID && a.shelfid == SID && a.mcid == MCID).Select(b => b.rackmcid).FirstOrDefault();
            List<ListItem> data1 = new List<ListItem>();
            List<ListItem> data1GroupBy = new List<ListItem>();
            List<ListItem> data2 = new List<ListItem>();
            List<ListItem> data2GroupBy = new List<ListItem>();
            List<ListItem> FinalResult = new List<ListItem>();
            data1 = (from a in db.shelfstockmovements
                        join b in db.Items on a.itemid equals b.ItemID into temp2
                        from b in temp2.DefaultIfEmpty()
                        where (a.rackmciid == rackmcID) &&
                        (a.purpose=="Open" || a.purpose == "Stock Received" || a.purpose == "Purchase" || a.purpose == "Sales Return") 
                        select new ListItem
                        {

                            itemQuantity=a.qty,
                            PriUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnitID).Select(a => a.ItemUnitName).FirstOrDefault(),
                            SubUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.SubUnitId).Select(a => a.ItemUnitName).FirstOrDefault(),
                            ItemID=b.ItemID,
                            ItemCode=b.ItemCode,
                            ItemName=b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            ItemUnit = a.unitid,
                            ItemUnitID=b.ItemUnitID,
                            SubUnitId=b.SubUnitId,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                            
                        }).ToList();
            data1GroupBy = (from a in data1
                            group new { a.itemQuantity, a.PriUnit, a.SubUnit, a.ItemID, a.ItemCode, a.ItemName, a.ItemWithCode, a.ItemUnit, a.ItemUnitID,a.SubUnitId, a.ConFactor } by new { a.ItemID } into g
                            select new ListItem
                            {
                                ItemID = g.FirstOrDefault().ItemID,
                                itemQuantity = g.Sum(k => k.itemQuantity),
                                PriUnit = g.FirstOrDefault().PriUnit,
                                SubUnit = g.FirstOrDefault().SubUnit,
                                ItemCode = g.FirstOrDefault().ItemCode,
                                ItemName = g.FirstOrDefault().ItemName,
                                ItemWithCode = g.FirstOrDefault().ItemWithCode,
                                ItemUnit = g.FirstOrDefault().ItemUnit,
                                ItemUnitID = g.FirstOrDefault().ItemUnitID,
                                SubUnitId= g.FirstOrDefault().SubUnitId,
                                ConFactor = g.FirstOrDefault().ConFactor,
                            }).ToList();
            data2 = (from a in db.shelfstockmovements
                     join b in db.Items on a.itemid equals b.ItemID into temp2
                     from b in temp2.DefaultIfEmpty()
                     where (a.rackmciid == rackmcID) &&
                     ( a.purpose == "Stock Transfered" || a.purpose == "Sales" || a.purpose == "Purchase Return")
                     select new ListItem
                     {

                         itemQuantity = a.qty,
                         PriUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnitID).Select(a => a.ItemUnitName).FirstOrDefault(),
                         SubUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.SubUnitId).Select(a => a.ItemUnitName).FirstOrDefault(),
                         ItemID = b.ItemID,
                         ItemCode = b.ItemCode,
                         ItemName = b.ItemName,
                         ItemWithCode = b.ItemCode + " - " + b.ItemName,
                         ItemUnit = a.unitid,
                         ItemUnitID = b.ItemUnitID,
                         SubUnitId = b.SubUnitId,
                         ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,

                     }).ToList();
            data2GroupBy = (from a in data2
                            group new { a.itemQuantity, a.PriUnit, a.SubUnit, a.ItemID, a.ItemCode, a.ItemName, a.ItemWithCode, a.ItemUnit, a.ItemUnitID, a.SubUnitId, a.ConFactor } by new { a.ItemID } into g
                            select new ListItem
                            {
                                ItemID = g.FirstOrDefault().ItemID,
                                itemQuantity = g.Sum(k => -k.itemQuantity),
                                PriUnit = g.FirstOrDefault().PriUnit,
                                SubUnit = g.FirstOrDefault().SubUnit,
                                ItemCode = g.FirstOrDefault().ItemCode,
                                ItemName = g.FirstOrDefault().ItemName,
                                ItemWithCode = g.FirstOrDefault().ItemWithCode,
                                ItemUnit = g.FirstOrDefault().ItemUnit,
                                ItemUnitID = g.FirstOrDefault().ItemUnitID,
                                SubUnitId = g.FirstOrDefault().SubUnitId,
                                ConFactor = g.FirstOrDefault().ConFactor,
                            }).ToList();
            data1GroupBy.AddRange(data2GroupBy);
            FinalResult = (from a in data1GroupBy
                            group new { a.itemQuantity, a.PriUnit, a.SubUnit, a.ItemID, a.ItemCode, a.ItemName, a.ItemWithCode, a.ItemUnit, a.ItemUnitID, a.SubUnitId, a.ConFactor } by new { a.ItemID } into g
                            select new ListItem
                            {
                                ItemID = g.FirstOrDefault().ItemID,
                                itemQuantity = g.Sum(k => k.itemQuantity),
                                PriUnit = g.FirstOrDefault().PriUnit,
                                SubUnit = g.FirstOrDefault().SubUnit,
                                ItemCode = g.FirstOrDefault().ItemCode,
                                ItemName = g.FirstOrDefault().ItemName,
                                ItemWithCode = g.FirstOrDefault().ItemWithCode,
                                ItemUnit = g.FirstOrDefault().ItemUnit,
                                ItemUnitID = g.FirstOrDefault().ItemUnitID,
                                SubUnitId = g.FirstOrDefault().SubUnitId,
                                ConFactor = g.FirstOrDefault().ConFactor,
                            }).ToList();
            FinalResult = FinalResult.Where(a => a.itemQuantity > 0).ToList();
            return Json(FinalResult);

        }
        [HttpPost]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteST(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + "Shelf Stock Transfer Entry.", true);
            return RedirectToAction("Index", "ShelfStockTransfer");
        }
        private Boolean DeleteST(long Id)
        {
            var UserId = User.Identity.GetUserId();
            var CurrentDate = Convert.ToDateTime(System.DateTime.Now);
            ShelfStockTransfer SEen = db.ShelfStockTransfers.Find(Id);
            var SEItem = db.SSTItems.Where(a => a.shelfTransfer == Id);
            var sstmovement = db.shelfstockmovements.Where(a => a.referenceid == Id);
            if (SEItem != null)
            {
                db.SSTItems.RemoveRange(db.SSTItems.Where(a => a.shelfTransfer == Id));
            }
            if (sstmovement != null)
            {
                db.shelfstockmovements.RemoveRange(db.shelfstockmovements.Where(a => a.referenceid == Id));
            }
            db.ShelfStockTransfers.Remove(SEen);
            db.SaveChanges();
            return true;
        }
        [RedirectingAction]
        [HttpGet]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ShelfStockTransfer mtd = db.ShelfStockTransfers.Find(id);

            if (mtd == null)
            {
                return NotFound();
            }
            return PartialView(mtd);
        }

        [RedirectingAction]
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete StockTransfer")]
        public ActionResult DeleteConfirmed(long shelftransferId)
        {
            bool stat = false;
            string msg;

            var chk = DeleteST(shelftransferId);
            if (chk == true)
            {
                stat = true;
                msg = "Successfully deleted Shelf Stock Transfer Entry details.";
            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

    }
}
