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
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using Microsoft.AspNetCore.Identity;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Globalization;
using System.Collections.Generic;
using System.Text;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class PackingListController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PackingListController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: PackingList
        [QkAuthorize(Roles = "Dev,PackingList List")]
        public ActionResult Index()
        {
            var created = db.Users.Select(s => new
            {
                ID = s.Id,
                Name = s.UserName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");
            ViewBag.Customer = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);
            _FinancialYear();

            var MlaPList = db.EnableSettings.Where(a => a.EnableType == "MLAPList").FirstOrDefault();
            var MlaPLists = MlaPList != null ? MlaPList.Status : Status.inactive;
            ViewBag.MLAPList = MlaPLists;

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindPackList").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

            return View();
        }
        [QkAuthorize(Roles = "Dev,PackingList Create")]
        public ActionResult Create()
        {
            var entry = new PackinglistViewModel
            {
                BillNo = InvoiceNo(),
                PLDate = Convert.ToDateTime(System.DateTime.Now),
                //TermsCondition = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "sorder").Select(a => a.TermsCondit).FirstOrDefault()
            };
            var cust = db.Customers.Where(s => s.Type == CRMCustomerType.Customer).Select(s => new
            {
                CustomerID = s.CustomerID,
                CustomerDetails = s.CustomerCode + " - " + s.CustomerName
            }).ToList();
            ViewBag.Custer = QkSelect.List(cust, "CustomerID", "CustomerDetails");
            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            var UserId = User.Identity.GetUserId();

            ViewBag.LastEntry = db.PackingLists.Select(p => p.PackinglistId).AsEnumerable().DefaultIfEmpty(0).Max();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      ID = s.EmployeeId,
                      Name = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var MlaPList = db.EnableSettings.Where(a => a.EnableType == "MLAPList").FirstOrDefault();
            var MlaPLists = MlaPList != null ? MlaPList.Status : Status.inactive;
            ViewBag.MLAPList = MlaPLists;

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            entry.FieldMap = db.FieldMappings.Where(a => a.Section == "Pklist" && a.Status == Status.active).ToList();

            return View(entry);
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,PackingList Create")]
        public JsonResult CreatePL(string[][] array, string[] pldata, string action)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                if (!BillExist(Convert.ToString(pldata[10])))
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
                        Branch = Convert.ToInt64(pldata[9]);
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    }

                    var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
                    var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;

                    //[CustomerID, Employee, PLDate, Items,, tAndc, custMail, CreatedUserEmail, LPONo, Remarks, Branch]

                    //sales entry
                    PackingList PLentry = new PackingList();
                    PLentry.Invoice = GetSeNo();
                    PLentry.BillNo = pldata[10];
                    PLentry.Customer = Convert.ToInt64(pldata[0]);
                    PLentry.Employee = Convert.ToInt64(pldata[1]);
                    PLentry.PLDate = DateTime.Parse(pldata[2], new CultureInfo("en-GB"));
                    PLentry.LPO = pldata[6];
                    PLentry.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                    PLentry.CreatedBy = UserId;
                    PLentry.Status = Status.active;
                    PLentry.Branch = Branch;
                    PLentry.Remarks = pldata[7];
                    PLentry.TermAndCondition = pldata[4];
                    PLentry.HSCode = pldata[11];

                    PLentry.Ref1 = Convert.ToString(pldata[13]);
                    PLentry.Ref2 = Convert.ToString(pldata[14]);
                    PLentry.Ref3 = Convert.ToString(pldata[15]);
                    PLentry.Ref4 = Convert.ToString(pldata[16]);
                    PLentry.Ref5 = Convert.ToString(pldata[17]);

                    db.PackingLists.Add(PLentry);
                    db.SaveChanges();
                    Int64 plId = 0;
                    plId = PLentry.PackinglistId;

                    ////// add to SEItem
                    string result = string.Empty;
                    DataTable PLItem = new DataTable();
                    PLItem.Columns.Add("ItemUnit");
                    PLItem.Columns.Add("ItemQuantity");
                    PLItem.Columns.Add("Item");
                    PLItem.Columns.Add("Packet");
                    PLItem.Columns.Add("MinQty");
                    PLItem.Columns.Add("ItemDiscount");
                    PLItem.Columns.Add("ItemNote");
                    PLItem.Columns.Add("PackingListId");


                    foreach (var arr in array)
                    {
                        DataRow dr = PLItem.NewRow();

                        dr["ItemUnit"] = arr[1];
                        dr["ItemQuantity"] = Convert.ToDecimal(arr[2]);
                        dr["Item"] = Convert.ToInt32(arr[0]);
                        dr["Packet"] = arr[3] != "" ? Convert.ToDecimal(arr[3]) : 0;
                        dr["MinQty"] = arr[4] != "" ? Convert.ToDecimal(arr[4]) : 0;
                        dr["ItemDiscount"] = arr[6] != "" ? Convert.ToDecimal(arr[6]) : 0;
                        dr["ItemNote"] = arr[9] == "bundle" ? "-:{Bundle_Item}" : Convert.ToString(arr[5].Replace("\n", "<br />"));
                        dr["PackingListId"] = plId;

                        PLItem.Rows.Add(dr);
                        var item = Convert.ToInt32(arr[0]);

                    }

                    ////// create parameter 
                    SqlParameter parameter = new SqlParameter("@TableType", PLItem);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "TableTypePLItems";
                    //// execute sp sql 
                    string sql = String.Format("EXEC {0} {1};", "SP_InsertPLItems", "@TableType");
                    //// execute sql 
                    db.Database.ExecuteSqlRaw(sql, parameter);

                    //Approved By
                    var Appby = Convert.ToString(pldata[12]);
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = plId;
                            approval.Type = "PackingList";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }

                    com.addlog(LogTypes.Created, UserId, "PackingList", "PackingLists", findip(), plId, "Successfully Submitted PackingList");

                    Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                    TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

                    if (action == "print")
                    {
                        var fmapp = db.FieldMappings.Where(a => a.Section == "Pklist" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                        var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                        var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                        var DvNoteData = com.PackingListData(plId, InPrintItemCode, PartNoCheck, TimeOut, ComHeadCheck);
                        var item = DvNoteData.pdfItem.ToList();
                        var summary = DvNoteData;

                        var def = Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                        def = def == 0 ? 1 : def;
                        var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

                        stat = true;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, layout, fmapp } };
                    }
                    msg = "Successfully Submitted PackingList.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

                    //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {
                    msg = "Packing List No. Already Exists.";
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

        [QkAuthorize(Roles = "Dev,Edit PackingList")]
        public ActionResult Edit(long? id)
        {
            var PLentry = new PackinglistViewModel
            {
                BillNo = InvoiceNo(),
                PLDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                TermsCondition = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "sorder").Select(a => a.TermsCondit).FirstOrDefault()
            };


            PackinglistViewModel vmodel = new PackinglistViewModel();
            vmodel = (from a in db.PackingLists
                      join b in db.Customers on a.Customer equals b.CustomerID into payfrom
                      from b in payfrom.DefaultIfEmpty()
                      where a.PackinglistId == id
                      select new PackinglistViewModel
                      {
                          BillNo = a.BillNo,
                          Customer = a.Customer,
                          CustomerName = b.CustomerCode + "-" + b.CustomerName,
                          PLDate = a.PLDate,
                          Invoice = a.PackinglistId,
                          Employee = a.Employee,
                          LPO = a.LPO,
                          Remarks = a.Remarks,
                          TermsCondition = a.TermAndCondition,
                          HSCode = a.HSCode,
                          Ref1 = a.Ref1,
                          Ref2 = a.Ref2,
                          Ref3 = a.Ref3,
                          Ref4 = a.Ref4,
                          Ref5 = a.Ref5,
                      }).FirstOrDefault();
            companySet();

            var cust = db.Customers.Where(s => s.Type == CRMCustomerType.Customer).Select(s => new
            {
                CustomerID = s.CustomerID,
                CustomerDetails = s.CustomerCode + " - " + s.CustomerName
            }).ToList();
            ViewBag.Custer = QkSelect.List(cust, "CustomerID", "CustomerDetails");

            var use = db.Employees.Select(s => new
            {
                ID = s.EmployeeId,
                Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
            }).ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            ViewBag.preEntry = db.PackingLists.Where(a => (a.PackinglistId < id)).Select(a => a.PackinglistId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.PackingLists.Where(a => (a.PackinglistId > id)).Select(a => a.PackinglistId).DefaultIfEmpty().Min();

            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "PackingList").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var userpermission = User.IsInRole("All Packlist Entry");
            var UserId = User.Identity.GetUserId();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaPList = db.EnableSettings.Where(a => a.EnableType == "MLAPList").FirstOrDefault();
            var MlaPLists = MlaPList != null ? MlaPList.Status : Status.inactive;
            ViewBag.MLAPList = MlaPLists;

            var EditPermission = User.IsInRole("Disable PkList Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "PackingList", UserId);

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Pklist" && a.Status == Status.active).ToList();

            return View(vmodel);
        }
        [QkAuthorize(Roles = "Dev,Edit PackingList")]
        public JsonResult EditPL(string[][] array, string[] pldata, string action)
        {
            bool stat = false;
            string msg;
            var plId = Convert.ToInt64(pldata[9]);
            PackingList PLentry = db.PackingLists.Find(plId);
            if (BillExist(Convert.ToString(pldata[10])) && Convert.ToString(pldata[10]) != PLentry.BillNo)
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

                var EditPermission = User.IsInRole("Disable PkList After Approval");
                if (com.chkApproved(plId, EditPermission, "PackingList", UserId) == true)
                {
                    //sales entry

                    PLentry.BillNo = pldata[10];
                    PLentry.Customer = Convert.ToInt64(pldata[0]);
                    PLentry.Employee = (pldata[1] != "") ? Convert.ToInt64(pldata[1]) : 0;
                    PLentry.PLDate = DateTime.Parse(pldata[2], new CultureInfo("en-GB"));
                    PLentry.LPO = pldata[6];

                    PLentry.Remarks = pldata[7];
                    PLentry.TermAndCondition = pldata[4];
                    PLentry.HSCode = pldata[11];

                    PLentry.Ref1 = Convert.ToString(pldata[13]);
                    PLentry.Ref2 = Convert.ToString(pldata[14]);
                    PLentry.Ref3 = Convert.ToString(pldata[15]);
                    PLentry.Ref4 = Convert.ToString(pldata[16]);
                    PLentry.Ref5 = Convert.ToString(pldata[17]);

                    db.Entry(PLentry).State = EntityState.Modified;
                    db.SaveChanges();


                    var PLItemz = db.PLItems.Where(a => a.PackingListId == plId).FirstOrDefault();
                    if (PLItemz != null)
                    {
                        db.PLItems.RemoveRange(db.PLItems.Where(a => a.PackingListId == plId));
                        db.SaveChanges();
                    }
                    ////// add to SEItem
                    string result = string.Empty;
                    DataTable PLItem = new DataTable();
                    PLItem.Columns.Add("ItemUnit");
                    PLItem.Columns.Add("ItemQuantity");
                    PLItem.Columns.Add("Item");
                    PLItem.Columns.Add("Packet");
                    PLItem.Columns.Add("MinQty");
                    PLItem.Columns.Add("ItemDiscount");
                    PLItem.Columns.Add("ItemNote");
                    PLItem.Columns.Add("PackingListId");


                    foreach (var arr in array)
                    {
                        DataRow dr = PLItem.NewRow();

                        dr["ItemUnit"] = arr[1];
                        dr["ItemQuantity"] = Convert.ToDecimal(arr[2]);
                        dr["Item"] = Convert.ToInt32(arr[0]);
                        dr["Packet"] = arr[3] != "" ? Convert.ToDecimal(arr[3]) : 0;
                        dr["MinQty"] = arr[4] != "" ? Convert.ToDecimal(arr[4]) : 0;
                        dr["ItemDiscount"] = arr[6] != "" ? Convert.ToDecimal(arr[6]) : 0;
                        dr["ItemNote"] = arr[9] == "bundle" ? "-:{Bundle_Item}" : Convert.ToString(arr[5].Replace("\n", "<br />"));
                        dr["PackingListId"] = plId;

                        PLItem.Rows.Add(dr);
                        var item = Convert.ToInt32(arr[0]);

                    }

                    ////// create parameter 
                    SqlParameter parameter = new SqlParameter("@TableType", PLItem);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "TableTypePLItems";
                    //// execute sp sql 
                    string sql = String.Format("EXEC {0} {1};", "SP_InsertPLItems", "@TableType");
                    //// execute sql 
                    db.Database.ExecuteSqlRaw(sql, parameter);

                    //Approved By
                    var empuser = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                    var chkapp = db.Approvals.Where(a => a.EmployeeId == empuser && a.TransEntry == plId && a.Type == "PackingList").FirstOrDefault();

                    var MrnPO = db.Approvals.Where(a => a.TransEntry == plId && a.Type == "PackingList").FirstOrDefault();
                    if (MrnPO != null)
                    {
                        if (chkapp != null)
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.EmployeeId != empuser && a.TransEntry == plId && a.Type == "PackingList"));
                            db.SaveChanges();
                        }
                        else
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == plId && a.Type == "PackingList"));
                            db.SaveChanges();
                        }
                    }
                    var Appby = Convert.ToString(pldata[12]);
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = plId;
                            approval.Type = "PackingList";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }

                    com.addlog(LogTypes.Created, UserId, "PackingList", "PackingLists", findip(), plId, "Successfully Submitted PackingList");
                }
                Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

                if (action == "print")
                {
                    var fmapp = db.FieldMappings.Where(a => a.Section == "Pklist" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                    var DvNoteData = com.PackingListData(plId, InPrintItemCode, PartNoCheck, TimeOut, ComHeadCheck);
                    var item = DvNoteData.pdfItem.ToList();
                    var summary = DvNoteData;

                    var def = Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                    def = def == 0 ? 1 : def;
                    var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, layout, fmapp } };
                }
                msg = "Successfully submitted PackingList.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

                //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                //    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };


            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

            }
        }
        [HttpGet]
        public ActionResult Download(long id)
        {
            var pkl = db.PackingLists.Where(s => s.PackinglistId == id).FirstOrDefault();
            var custname = db.Customers.Where(s => s.CustomerID == pkl.Customer).Select(a => a.CustomerName).FirstOrDefault();

            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = pkl.BillNo;

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), "inactive");
            return File(ms, "application/pdf", "Packing List" + "-" + custname + "-" + billno + ".pdf");

        }
        public StringBuilder generatePdf(long id)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

            var DvNoteData = com.PackingListData(id, InPrintItemCode, PartNoCheck, TimeOut);
            var item = DvNoteData.pdfItem.ToList();
            var summary = DvNoteData;

            return com.generatepdf(id, summary, item, null, "Packing List");

        }



        [HttpPost]
        public JsonResult SearchItemById(long? ItemId, long? entryId)
        {
            var v = (from b in db.Items
                     join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                     from c in primary.DefaultIfEmpty()
                     join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                     from d in second.DefaultIfEmpty()
                     join g in db.ItemBundles on b.ItemID equals g.mainItem into bndl
                     from g in bndl.DefaultIfEmpty()
                     where b.ItemID == ItemId
                     select new
                     {
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
                         b.SellingPrice,
                         b.PurchasePrice,
                         b.BasePrice,
                         b.MRP,
                         b.KeepStock,
                         ItemNote = "",
                         Bundle = (long?)g.ItemBundleId
                     }).ToList();

            var data = (from o in v
                        select new
                        {
                            Item = o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            OpeningStock = o.OpeningStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ConFactor,
                            o.SellingPrice,
                            o.PurchasePrice,
                            o.BasePrice,
                            o.MRP,
                            price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            o.KeepStock,
                            id = o.ItemID,
                            text = o.ItemCode + " - " + o.ItemName,
                            ItemDiscount = 0,
                            o.ItemNote,
                            bundle = (from ab in db.BundleItems
                                      join bb in db.Items on ab.ItemId equals bb.ItemID
                                      join cb in db.ItemUnits on ab.ItemUnit equals cb.ItemUnitID into bprimary
                                      from cb in bprimary.DefaultIfEmpty()
                                      where ab.ItemBundle == o.Bundle
                                      select new
                                      {
                                          Item = bb.ItemID,
                                          bb.ItemCode,
                                          bb.ItemName,
                                          ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                          bb.ItemUnitID,
                                          bb.SubUnitId,
                                          OpeningStock = bb.OpeningStock,
                                          MinStock = (bb.MinStock != null) ? bb.MinStock : 0,
                                          bb.ConFactor,
                                          bb.SellingPrice,
                                          bb.PurchasePrice,
                                          bb.BasePrice,
                                          bb.MRP,
                                          price = (bb.SellingPrice != 0) ? bb.SellingPrice : bb.MRP,
                                          bb.KeepStock,
                                          cb.ItemUnitName,
                                          id = bb.ItemID,
                                          text = bb.ItemCode + " - " + bb.ItemName,
                                          BaseQty = ab.ItemQuantity,
                                          ab.ItemUnit,
                                          UnitName = (ab.ItemUnit != null) ? cb.ItemUnitName : "",
                                          ItemDiscount = o.ItemID,
                                          ItemNote = "-:{Bundle_Item}",
                                      }).ToList(),
                        }).ToList();
            return Json(data);
        }

        [HttpPost]
        public JsonResult SearchPackItemById(long? ItemId, long entryId)
        {
            var v = (from a in db.PLItems
                     join b in db.Items on a.Item equals b.ItemID into items
                     from b in items.DefaultIfEmpty()
                     join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                     from c in primary.DefaultIfEmpty()
                     join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                     from d in second.DefaultIfEmpty()
                     join g in db.ItemBundles on b.ItemID equals g.mainItem into bndl
                     from g in bndl.DefaultIfEmpty()
                     join h in db.PackingLists on a.PackingListId equals h.PackinglistId
                     where a.PackingListId == entryId && a.ItemNote != "-:{Bundle_Item}"
                     select new
                     {
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
                         b.SellingPrice,
                         b.PurchasePrice,
                         b.BasePrice,
                         b.MRP,
                         b.KeepStock,
                         a.ItemNote,
                         a.ItemQuantity,
                         a.Packet,
                         a.MinQty,
                         Bundle = (long?)g.ItemBundleId
                     }).ToList();

            var data = (from o in v
                        select new
                        {
                            Item = o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            o.ItemQuantity,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            OpeningStock = o.OpeningStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ConFactor,
                            o.SellingPrice,
                            o.PurchasePrice,
                            o.BasePrice,
                            o.MRP,
                            price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            o.KeepStock,
                            o.Packet,
                            o.MinQty,
                            id = o.ItemID,
                            text = o.ItemCode + " - " + o.ItemName,
                            ItemDiscount = 0,
                            o.ItemNote,
                            bundle = (from ab in db.PLItems
                                      join bb in db.Items on ab.Item equals bb.ItemID
                                      join cb in db.ItemUnits on ab.ItemUnit equals cb.ItemUnitID into bprimary
                                      from cb in bprimary.DefaultIfEmpty()
                                      where ab.PackingListId == entryId && ab.ItemNote == "-:{Bundle_Item}"
                                      && o.ItemID == ab.ItemDiscount
                                      select new
                                      {
                                          Item = bb.ItemID,
                                          bb.ItemCode,
                                          bb.ItemName,
                                          ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                          bb.ItemUnitID,
                                          bb.SubUnitId,
                                          OpeningStock = bb.OpeningStock,
                                          MinStock = (bb.MinStock != null) ? bb.MinStock : 0,
                                          bb.ConFactor,
                                          bb.SellingPrice,
                                          bb.PurchasePrice,
                                          bb.BasePrice,

                                          bb.MRP,
                                          price = (bb.SellingPrice != 0) ? bb.SellingPrice : bb.MRP,
                                          bb.KeepStock,
                                          ab.Packet,
                                          ab.MinQty,
                                          id = bb.ItemID,
                                          text = bb.ItemCode + " - " + bb.ItemName,
                                          BaseQty = ab.ItemQuantity,
                                          ab.ItemUnit,
                                          UnitName = (ab.ItemUnit != null) ? cb.ItemUnitName : "",
                                          ItemDiscount = o.ItemID,
                                          ab.ItemNote
                                      }).ToList(),
                        }).ToList();
            return Json(data);
        }

        [QkAuthorize(Roles = "Dev,PackingList List")]
        public JsonResult GetPackingList(string InvoiceNo, string LPO, string FromDate, string ToDate, long? Customer, string user, string appstat)
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

            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit PackingList");
            var uDelete = User.IsInRole("Delete PackingList");

            var v = (from a in db.PackingLists
                     join b in db.Customers on a.Customer equals b.CustomerID into payfrom
                     from b in payfrom.DefaultIfEmpty()
                     join g in db.Users on a.CreatedBy equals g.Id

                     where ((InvoiceNo == null || InvoiceNo == "" || a.BillNo == InvoiceNo) &&
                     (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(a.PLDate, fdate) <= 0) &&
                     (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(a.PLDate, tdate) >= 0)) &&
                     (user == null || user == "" || g.Id == user) &&
                     (Customer == null || Customer == 0 || a.Customer == Customer) &&
                     (LPO == null || LPO == "" || a.LPO == LPO)
                     select new
                     {
                         a.PackinglistId,
                         Invoice = a.BillNo,
                         LPO = a.LPO,
                         Date = a.PLDate,
                         Customer = b.CustomerName,
                         CreatedBy = g.Name,
                         Billno = a.BillNo,
                         Status = a.Status,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete,
                         a.CreatedDate
                     }).ToList().Select(o =>
                     {
                         var app = db.Approvals.Where(x => x.TransEntry == o.PackinglistId && x.Type == "PackingList").Select(x => x.EmployeeId).ToList();
                         var AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == o.PackinglistId && x.Type == "PackingList").Select(x => x.ApprovalStatus).ToList();
                         var chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == o.PackinglistId && x.Type == "PackingList").GroupBy(l => l.ApprovedBy)
                                            .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                            .ToList().Select(x => x.ApprovalStatus).ToList();
                         return new
                         {

                             o.PackinglistId,
                             o.Invoice,
                             o.LPO,
                             o.Date,
                             o.Customer,
                             o.CreatedBy,
                             o.Billno,
                             o.Status,
                             o.Dev,
                             o.Edit,
                             o.Delete,
                             app,
                             Approval = (app != null && empl.EmployeeId != null) ? (app.Contains(empl.EmployeeId) ? true : false) : false,
                             ApprovalStatus = (app.Count > 0 && AppStatus.Count > 0 && chkAppStatus.Count > 0) ? (chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (app != null && chkAppStatus != null && app.Count != 0 && chkAppStatus.Count != 0 && chkAppStatus.Count() == app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                             o.CreatedDate
                         };
                     });
            if (appstat != "")
            {
                v = v.Where(a => a.ApprovalStatus == AppSt);
            }
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }


        [QkAuthorize(Roles = "Dev,Delete PackingList")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Delete
            var userpermission = User.IsInRole("All Packlist Entry");
            var UserId = User.Identity.GetUserId();
            PackingList pack = db.PackingLists.Where(x => x.PackinglistId == id).FirstOrDefault();

            if (pack == null)
            {
                return NotFound();
            }
            return PartialView(pack);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete PackingList")]
        public JsonResult Delete(long id)
        {
            bool stat = false;
            string msg;


            var chk = DeletePack(id);
            if (chk == true)
            {
                stat = true;
                msg = "Successfully deleted PackingList details.";
            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete PackingList")]
        public ActionResult DeleteAllPacklist(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = (DeletePack(arr) == true) ? count++ : notdel++;
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Packinglists, Unable to Delete " + notdel + " Packinglists. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Packinglist.", true);
            }
            else
            {
                Success("Deleted " + count + " Packinglist.", true);
            }
            return RedirectToAction("Index", "PackingList");
        }

        private Boolean DeletePack(long Id)
        {
            var UserId = User.Identity.GetUserId();
            PackingList Pack = db.PackingLists.Find(Id);
            var PLitem = db.PLItems.Where(a => a.PackingListId == Id);
            if (PLitem != null)
            {
                db.PLItems.RemoveRange(db.PLItems.Where(a => a.PackingListId == Id));

            }
            // delete from petransaction and adjest rate of pepayment
            var appr = db.Approvals.Where(a => a.TransEntry == Id && a.Type == "PackingList").FirstOrDefault();
            if (appr != null)
            {
                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == Id && a.Type == "PackingList"));
            }
            var app = db.ApprovalUpdates.Where(a => a.TransEntry == Id && a.Type == "PackingList").FirstOrDefault();
            if (app != null)
            {
                db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == Id && a.Type == "PackingList"));
            }
            db.PackingLists.Remove(Pack);
            db.SaveChanges();
            com.addlog(LogTypes.Deleted, UserId, "Packinglist", "Packinglists", findip(), Id, "Packinglist Deleted Successfully");
            return true;

        }

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,View PackingList")]
        public ActionResult Details(long? id)
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            PackinglistViewModel vmodel = new PackinglistViewModel();

            vmodel = (from b in db.PackingLists
                      join d in db.Employees on b.Employee equals d.EmployeeId into emp
                      from d in emp.DefaultIfEmpty()
                      join f in db.Customers on b.Customer equals f.CustomerID into cust
                      from f in cust.DefaultIfEmpty()
                      join x in db.Contacts on f.Contact equals x.ContactID into cnt
                      from x in cnt.DefaultIfEmpty()
                      where b.PackinglistId == id
                      select new PackinglistViewModel
                      {
                          CustomerName = f.CustomerCode + " - " + f.CustomerName,
                          PackinglistId = b.PackinglistId,
                          BillNo = b.BillNo,
                          PLDate = b.PLDate,
                          Remarks = b.Remarks.Replace("\n", "<br />"),
                          TermsCondition = b.TermAndCondition.Replace("\n", "<br />"),
                          EmployeeName = d.FirstName + " " + d.LastName,
                          LPO = b.LPO,

                          HSCode = b.HSCode,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          Emp = (from ab in db.Approvals
                                 join bb in db.Employees on ab.EmployeeId equals bb.EmployeeId
                                 where ab.TransEntry == id && ab.Type == "PackingList"
                                 select new ApprovalViewModel
                                 {
                                     EmpName = bb.FirstName + " " + bb.LastName
                                 }).ToList()
                      }).FirstOrDefault();

            vmodel.PLItems = db.PLItems.Where(a => a.PackingListId == id &&  a.ItemNote != "-:{Bundle_Item}")
            .Select(b => new PLItemViewModel
            {
                ItemQuantity = b.ItemQuantity,
                Packet=(decimal)b.Packet,
                MinQty= (decimal)b.MinQty,
                ItemNote=b.ItemNote,
                ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault(),


                subitem = (from ab in db.PLItems
                              join bb in db.Items on ab.Item equals bb.ItemID
                              join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                              from cb in primary.DefaultIfEmpty()
                              join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                              from bd in second.DefaultIfEmpty()
                              where ab.PackingListId == id && ab.ItemNote == "-:{Bundle_Item}"
                              && b.Item == ab.ItemDiscount
                              select new ItemPkListViewModel
                              {
                                  ItemCode = bb.ItemCode,
                                  ItemName = bb.ItemName,
                                  ItemUnit = cb.ItemUnitName,
                                  ItemQuantity = ab.ItemQuantity,
                                  Packet = (decimal)b.Packet,
                                  MinQty = (decimal)b.MinQty,
                              }).ToList()
            }).ToList();

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Pklist" && a.Status == Status.active).ToList();

            return View(vmodel);
        }

        //[QkAuthorize(Roles = "Dev,Print Sticker")]
        public ActionResult Sticker()
        {
            ViewBag.ddlItem = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                               }, "Value", "Text", 1);
            companySet();
            return View();

        }



        private long GetSeNo()
        {
            Int64 SENo = 0;
            string prefix = "Packinglist";
            var companyPrefix = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.number).FirstOrDefault();
            if ((db.PackingLists.Select(p => p.Invoice).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
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
                SENo = db.PackingLists.Max(p => p.Invoice + 1);
            }

            return SENo;
        }
        private string InvoiceNo(Int64 QENo = 0, string billNo = null, string section = null)
        {
            string prefix = "Packinglist";
            var companyPrefix = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.number).FirstOrDefault();
            if (billNo == null)
            {
                if ((db.PackingLists.Select(p => p.Invoice).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    QENo = db.PackingLists.Max(p => p.Invoice + 1);
                    billNo = companyPrefix + QENo;
                    if (BillExist(billNo))
                    {
                        billNo = InvoiceNo(QENo, billNo, section);
                    }
                }
            }
            else
            {
                QENo = QENo + 1;
                billNo = companyPrefix + QENo;
                if (BillExist(billNo))
                {
                    billNo = InvoiceNo(QENo, billNo, section);
                }

            }
            return billNo;
        }
        private bool BillExist(string QENo)
        {
            var Exists = db.PackingLists.Any(c => c.BillNo == QENo);
            bool res = (Exists) ? true : false;
            return res;
        }

        [QkAuthorize]
        public JsonResult Search(string q, string x, string page)
        {

            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.PackingLists.Where(p => (p.Status == Status.active) && (p.BillNo.ToLower().Contains(q.ToLower()) || p.BillNo.Contains(q)))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.BillNo + "-" + db.Customers.Where(z => z.CustomerID == b.Customer).Select(y => y.CustomerName).FirstOrDefault(), //each json object will have 
                                      id = b.PackinglistId
                                  })
                                  .OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
            }
            else
            {
                serialisedJson = db.PackingLists.Where(p => p.Status == Status.active).Select(b => new SelectFormat
                {
                    text = b.BillNo + "-" + db.Customers.Where(z => z.CustomerID == b.Customer).Select(y => y.CustomerName).FirstOrDefault(), //each json object will have 
                    id = b.PackinglistId
                }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }//
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        //[QkAuthorize(Roles = "Dev,PackingList List")]
        [HttpPost]
        public JsonResult GetPack(long Packinglistid)
        {
            var data = (from b in db.PLItems
                        join c in db.Items on b.Item equals c.ItemID
                        where (Packinglistid == b.PackingListId)
                        select new
                        {
                            b.PLItemId,
                            id = b.PackingListId,
                            qty = b.ItemQuantity,
                            mainitem = b.Item,//e.mainItem, 
                            MinQty = b.MinQty,
                            Itemname = c.ItemName
                        }).OrderBy(x => x.PLItemId).ToList();



            List<StickerViewModel> final = new List<StickerViewModel>();
            decimal tqty = 0;
            int itemcount = 1;
            foreach (var arr in data)
            {
                if (arr.MinQty != 0)
                {
                    decimal? totpack = 0;

                    totpack = arr.qty / arr.MinQty;
                    var reminder = arr.qty % arr.MinQty;
                    int Pack = Convert.ToInt32(totpack);
                    
                    var packcount = 0;
                    char alph = 'A';
                    int sn = 1;
                    tqty = tqty + arr.qty;
                    for (packcount = 1; packcount <= Pack; packcount++)
                    {
                        StickerViewModel SVM = new StickerViewModel();

                        SVM.SN = sn;
                        SVM.itemSticker = arr.Itemname;
                        SVM.quant = itemcount;
                        SVM.alph = alph;
                        var UsedQty = arr.MinQty * packcount;
                        if((arr.MinQty*packcount <= arr.qty))
                        {
                            SVM.qty = "(" + arr.MinQty + " Pcs) of " + Pack + " pkt";
                        }
                        else
                        {
                            SVM.qty = "(" + reminder + " Pcs) of " + Pack + " pkt";
                        }
                        
                        final.Add(SVM);
                        alph++;
                    }
                    itemcount++;
                }
            }

            var count = data.Count();
            return Json(new { data = final, count = count });
        }
        [HttpGet]
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "PackingList" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
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

            var MR = db.PackingLists.Where(a => a.PackinglistId == id).FirstOrDefault();

            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "PackingList").OrderByDescending(a => a.CreatedDate).FirstOrDefault();

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
                AppUp.Type = "PackingList";

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
                            join d in db.PackingLists on b.TransEntry equals d.PackinglistId into team
                            from d in team.DefaultIfEmpty()
                            join e in db.Employees on b.RequestBy equals e.UserId into emp
                            from e in emp.DefaultIfEmpty()
                            join u in db.Users on d.CreatedBy equals u.Id into req
                            from u in req.DefaultIfEmpty()
                            where b.TransEntry == MCId && b.Type == "PackingList"
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
