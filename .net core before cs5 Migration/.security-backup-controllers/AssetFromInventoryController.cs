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
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class AssetFromInventoryController : BaseController
    {
        public AssetFromInventoryController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        ApplicationDbContext db;
        Common com;
        // GET: StockTransfer
        // [QkAuthorize(Roles = "Dev,AssetFromInventory List")]
        [HttpGet]
        public ActionResult MoveAssettoasset()
        {

            var Accnt = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = false  },
                          }, "Value", "Text", 0);

            ViewBag.AssetAccount = Accnt;

            companySet();


            return View();
        }
        [RedirectingAction]
        [HttpPost]
        public JsonResult UpdateMoveAssettoasset(long?[] value, long assetto)
        {
            bool stat = false;
            string msg = "";
            if (value != null)
            {
                foreach (var val in value)
                {
                    AssetTransferDetail cust = db.AssetTransferDetails.Find(val);
                    cust.AssetAccountId = assetto;
                    db.Entry(cust).State = EntityState.Modified;
                    db.SaveChanges();
                }
                stat = true;
                msg = "Successfully Updated;";
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public ActionResult Index()
        {
            var mcs = db.MCs.Select(s => new
            {
                McId = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.MCbag = QkSelect.List(mcs, "McId", "Name");

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindStkTrans").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;


            return View();
        }

        [HttpPost]

        public ActionResult GetAssetTransfer(long? AssetID, string FromDate, string ToDate, long? MFrom)
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

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;




            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit AssetFromInventory");
            var uDelete = User.IsInRole("Delete AssetFromInventory");

            var v = (from a in db.AssetTransferMasters
                     join b in db.MCs on a.McFromId equals b.MCId into mc
                     from b in mc.DefaultIfEmpty()

                     where
                     (AssetID == null || a.InvoiceNo == AssetID) &&
                     (a.VendorName == null) &&
                     (FromDate == "" || FromDate == null || EF.Functions.DateDiffDay(a.AssetEntryDate, fdate) <= 0) &&
                     (ToDate == "" || ToDate == null || EF.Functions.DateDiffDay(a.AssetEntryDate, tdate) >= 0) &&
                     (MFrom == 0 || MFrom == null || a.McFromId == MFrom)

                     select new
                     {
                         Id = a.AssetEntryId,
                         AssetID = a.InvoiceNo,
                         Date = a.AssetEntryDate,
                         MCFrom = b.MCName,
                         TotalAssetValue = a.TotalAssetValue

                     }).ToList().
            Select(b => new
            {
                b.Id,
                b.AssetID,
                b.Date,
                b.MCFrom,
                b.TotalAssetValue,


                Dev = uDev,
                Edit = uEdit,
                Delete = uDelete,

            }).ToList().Select(o => new
            {

                o.Id,
                o.AssetID,
                o.Date,
                o.TotalAssetValue,
                o.MCFrom,

                o.Dev,
                o.Edit,
                o.Delete,

            });

            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.AssetID.ToString().ToLower().Contains(search.ToLower()));
            }
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        public ActionResult Create()
        {


            var UserId = User.Identity.GetUserId();

            var today = Convert.ToDateTime(System.DateTime.Now);
            var warningMsg = db.EnableSettings.Where(a => a.EnableType == "WarningOnSave").FirstOrDefault();
            var warnmsg = warningMsg != null ? (warningMsg.Status == Status.active ? 0 : 1) : 1;
            ViewBag.WarnMsg = warnmsg;

            //for 
            var STTo = new AssetTransferViewModel
            {
                InvoiceNo = GetEntryNo(),
                AssetEntryDate = today,

            };


            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      ID = s.EmployeeId,
                      Name = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var MlaSTran = db.EnableSettings.Where(a => a.EnableType == "MLASTran").FirstOrDefault();
            var MlaSTrans = MlaSTran != null ? MlaSTran.Status : Status.inactive;
            ViewBag.MLASTran = MlaSTrans;
            AssetTransferViewModel vmodel = new AssetTransferViewModel();
            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var mcs = db.MCs.Select(s => new
            {
                McId = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.MCbag = QkSelect.List(mcs, "McId", "Name");


            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();

            if (mcchk != null)
            {
                var mcs1 = db.MCs.Where(s => s.AssignedUser == UserId).Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MCToBag = QkSelect.List(mcs1, "Id", "Name");
            }
            else
            {
                var mcs1 = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MCToBag = QkSelect.List(mcs1, "Id", "Name");
            }
            return View(STTo);
        }

        [RedirectingAction]
        [HttpPost]

        public JsonResult Create(string[][] array, string[] mtdata, string action, AssetTransferViewModel bsmodel)
        {
            long billno = Convert.ToInt64(mtdata[0]);
            var Billcheck = db.AssetTransferMasters.Where(a => a.InvoiceNo == billno).Any();
            bool stat = false;
            string msg = "";
            if (!Billcheck)
            {
                long MC = 0;
                var today = Convert.ToDateTime(System.DateTime.Now);
                var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
                var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

                var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
                var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

                var UserId = User.Identity.GetUserId();
                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();







                AssetTransferMasters sl = new AssetTransferMasters();
                if (sl != null)
                {
                    sl.InvoiceNo = Convert.ToInt64(mtdata[0]);
                    sl.AssetEntryDate = DateTime.Parse(mtdata[1], new CultureInfo("en-GB"));
                    sl.McFromId = Convert.ToInt64(mtdata[2]);
                    sl.TotalAssetValue = Convert.ToDecimal(mtdata[6]);

                    db.AssetTransferMasters.Add(sl);
                }
                db.SaveChanges();

                Int64 STId = sl.AssetEntryId;
                long ItemId = 0;
                AssetTransferDetail asset = new AssetTransferDetail();

                if (asset != null)
                {
                    foreach (var arr in array)
                    {
                        asset.AssetEntryId = STId;

                        ItemId = Convert.ToInt64(arr[0]);
                        var demoitem = db.Items.Where(a => a.ItemID == ItemId).FirstOrDefault();
                        asset.AssetName = demoitem.ItemName;
                        asset.Barcode = arr[1];
                        asset.UnitId = Convert.ToInt64(arr[2]);
                        asset.Quantity = Convert.ToDecimal(arr[3]);
                        asset.Price = Convert.ToDecimal(arr[4]);
                        asset.TotalPrice = Convert.ToDecimal(arr[6]);

                        if (arr[8] != "")
                            asset.DepreciationPercentage = Convert.ToInt64(arr[8]);
                        else
                            asset.DepreciationPercentage = 0;

                        asset.AssetAccountId = Convert.ToInt64(arr[9]);
                        asset.DepreciationAccountId = Convert.ToInt64(arr[10]);
                        asset.RefItemId = Convert.ToInt64(arr[0]);
                        db.AssetTransferDetails.Add(asset);
                        db.SaveChanges();
                        var StockAccount = db.Accountss.Where(x => x.Name == "Asset From Inventory").FirstOrDefault();
                        com.addAccountTrasaction(0,asset.TotalPrice,  StockAccount.AccountsID, "Asset From Inventory", STId, DC.Credit, sl.AssetEntryDate, null, null, null, null);
                        com.addAccountTrasaction(asset.TotalPrice,0,  (long)asset.AssetAccountId, "Asset From Inventory", STId, DC.Debit, sl.AssetEntryDate, null, null, null, null);
                  
                    }



                    //    //changing the status of item table



                }

                Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

                if (action == "print")
                {
                    var fmapp = db.FieldMappings.Where(a => a.Section == "StkTrans" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                    var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                    var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                    //sales return
                    object item = "";
                    object summary = "";
                    object billsundry = "";
                    object cdetails = "";

                    var def = Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                    def = def == 0 ? 1 : def;
                    var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, cdetails, layout, fmapp } };
                }
                else
                {
                    msg = "Asset Transfer Successfully Transfered..";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
            }
            else
            {
                msg = "Voucher No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

        }

        private long GetVTNo()
        {
            Int64 SENo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "StockTransfer").Select(a => a.number).FirstOrDefault();
            if ((db.StockTransfers.Select(p => p.STNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
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
                SENo = db.StockTransfers.Max(p => p.STNo + 1);
            }
            return SENo;
        }
        private long GetEntryNo()
        {
            Int64 EntryNo = 0;
            Int64 LastNo = Convert.ToInt64(db.AssetTransferMasters.Select(p => p.InvoiceNo).DefaultIfEmpty().Max());
            if (LastNo == 0)
                EntryNo = 1;
            else
                EntryNo = LastNo + 1;
            return EntryNo;
        }
        public ActionResult ItemWiseReport()
        {
            _FinancialYear();
            var OptAll = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false  },
                            }, "Value", "Text", 0);

            ViewBag.Item = OptAll;
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);


            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            return View();
        }
        public ActionResult AccountWiseReportbylink(long accountid)
        {
           
            return View();
        }
        public ActionResult AccountWiseReport()
        {
            _FinancialYear();
            var OptAll = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false  },
                            }, "Value", "Text", 0);

            ViewBag.Item = OptAll;
            var Accnt = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false  },
                            }, "Value", "Text", 0);

            ViewBag.AssetAccount = Accnt;
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);


            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Sale", Value="1"},
                new SelectListItem() {Text = "Hire", Value="2"},
            }, "Value", "Text");
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            return View();
        }
        [HttpPost]
        public ActionResult GetItemWiseReport(long?[] ddlItem, string fromdate, string todate)
        {



            var UserId = User.Identity.GetUserId();
            // ddlItem can arrive null (no selection) or with blank entries; int.Parse("") threw FormatException.
            // Guard each token (skip empty/null) so an unset/empty filter just yields no ids instead of crashing.
            var items = String.Join(",", ddlItem ?? new long?[0]);
            int[] itemss = items.Split(',')
                                .Where(x => x != "" && x != null)
                                .Select(x => int.Parse(x))
                                .ToArray();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "" && fromdate != null)
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "" && todate != null)
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }

            List<ItemList> v1 = new List<ItemList>();
            foreach (long itemid in itemss)
            {

                // EF Core 10 can't read the non-nullable AssetEntryDate of the left-joined (DefaultIfEmpty) master c
                // into ItemList.AssetEntryDate (non-nullable DateTime) when the outer join produced a NULL row
                // ("Nullable object must have a value"). Materialize entity-only, then re-project client-side null-safe.
                var serverRows = (from a in db.Items
                         join b in db.AssetTransferDetails on a.ItemID equals b.RefItemId into temp1
                         from b in temp1.DefaultIfEmpty()
                         join c in db.AssetTransferMasters on b.AssetEntryId equals c.AssetEntryId into temp2
                         from c in temp2.DefaultIfEmpty()
                         join d in db.Accountss on b.AssetAccountId equals d.AccountsID into temp3
                         from d in temp3.DefaultIfEmpty()
                         join e in db.ItemUnits on b.UnitId equals e.ItemUnitID into temp4
                         from e in temp4.DefaultIfEmpty()
                         where (itemid == 0 || a.ItemID == itemid) &&
                         (fromdate == "" || fromdate == null || EF.Functions.DateDiffDay(c.AssetEntryDate, fdate) <= 0) &&
                         (todate == "" || todate == null || EF.Functions.DateDiffDay(c.AssetEntryDate, tdate) >= 0)
                         select new
                         {
                             ItemID = a.ItemID,
                             ItemCode = a.ItemCode,
                             ItemName = a.ItemName,
                             ItemUnitName = e.ItemUnitName,
                             AccountName = d.Name,
                             // Project only the needed columns as nullable — b is left-joined (may be null), so the
                             // nullable casts avoid the NULL-struct crash when there is no transfer row.
                             bQuantity = (decimal?)b.Quantity,
                             bPrice = (decimal?)b.Price,
                             cAssetEntryDate = (DateTime?)c.AssetEntryDate,
                         }).ToList();

                // Distinct() on an anonymous tuple keeps the original SQL DISTINCT-by-value semantics
                // (ItemList is a class → reference equality would not dedupe), then map to ItemList.
                var v = serverRows.Select(o => new
                         {
                             ItemID = o.ItemID,
                             Item = o.ItemCode + "-" + o.ItemName,
                             PriUnit = o.ItemUnitName,
                             NetQty = o.bQuantity,
                             Price = o.bPrice,
                             AssetAccountName = o.AccountName,
                             AssetEntryDate = o.cAssetEntryDate ?? default(DateTime),
                         }).Distinct()
                         .Select(o => new ItemList
                         {
                             ItemID = o.ItemID,
                             Item = o.Item,
                             PriUnit = o.PriUnit,
                             NetQty = o.NetQty,
                             Price = o.Price,
                             AssetAccountName = o.AssetAccountName,
                             AssetEntryDate = o.AssetEntryDate,
                         });
                v = v.OrderBy(b => b.Item);
                v1.AddRange(v);
            }
            var data = v1.Skip(skip).Take(pageSize).ToList();
            recordsTotal = v1.Count();
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }

        [HttpPost]
        public ActionResult GetAccountWiseReportMOve(long? AssetAccount)
        {
            var UserId = User.Identity.GetUserId();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            List<ItemList> v1 = new List<ItemList>();


            // EF Core 10 throws "Nullable object must have a value" when projecting the non-nullable members of the
            // left-joined (DefaultIfEmpty) entities b/c (Quantity, Price, AssetitementryId, InvoiceNo, AssetEntryDate,
            // AssetEntryId) directly in SQL: a NULL row from the outer join can't be read into a non-nullable struct.
            // Materialize an entity-only projection, then re-project client-side with the SAME member names/order, null-safe.
            var serverRows = (from a in db.Items
                     join b in db.AssetTransferDetails on a.ItemID equals b.RefItemId into temp1
                     from b in temp1.DefaultIfEmpty()
                     join c in db.AssetTransferMasters on b.AssetEntryId equals c.AssetEntryId into temp2
                     from c in temp2.DefaultIfEmpty()
                     join d in db.Accountss on b.AssetAccountId equals d.AccountsID into temp3
                     from d in temp3.DefaultIfEmpty()
                     join e in db.ItemUnits on b.UnitId equals e.ItemUnitID into temp4
                     from e in temp4.DefaultIfEmpty()
                     where (AssetAccount == 0 || b.AssetAccountId == AssetAccount)
                     select new
                     {
                         ItemID = a.ItemID,
                         ItemCode = a.ItemCode,
                         ItemName = a.ItemName,
                         ItemUnitName = e.ItemUnitName,
                         AccountName = d.Name,
                         // Project ONLY the needed columns as nullable (b/c are left-joined → may be null; nullable casts
                         // avoid the "Nullable object must have a value" crash).
                         bQuantity = (decimal?)b.Quantity,
                         bPrice = (decimal?)b.Price,
                         bAssetitementryId = (long?)b.AssetitementryId,
                         cInvoiceNo = (long?)c.InvoiceNo,
                         cAssetEntryDate = (DateTime?)c.AssetEntryDate,
                         cAssetEntryId = (long?)c.AssetEntryId,
                     }).ToList();

            var v = serverRows.Select(o => new
                     {
                         invoice = o.cInvoiceNo ?? 0,
                         ItemID = o.ItemID,
                         Item = o.ItemCode + "-" + o.ItemName,
                         PriUnit = o.ItemUnitName,
                         NetQty = o.bQuantity ?? 0,
                         Price = o.bPrice ?? 0,
                         AssetAccountName = o.AccountName,
                         AssetEntryDate = o.cAssetEntryDate,
                         AssetEntryId = o.cAssetEntryId ?? 0,
                         AssetitementryId = o.bAssetitementryId ?? 0
                     }).Distinct();
            v = v.OrderBy(b => b.AssetEntryDate);
            var data = v.Skip(skip).Take(pageSize).ToList();
            recordsTotal = v.Count();
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }



        [HttpPost]
        public ActionResult GetAccountWiseReport(long? AssetAccount, string fromdate, string todate, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }

            List<ItemList> v1 = new List<ItemList>();


            // EF Core 10 throws "Nullable object must have a value" when projecting the non-nullable members of the
            // left-joined (DefaultIfEmpty) entities b/c (Quantity, Price, InvoiceNo, AssetEntryDate, AssetEntryId)
            // directly in SQL: a NULL row from the outer join can't be read into a non-nullable struct.
            // Materialize an entity-only projection (nullable casts), then re-project client-side with the SAME
            // member names/order/values, null-safe. ConvertedQty stays server-side (already ?? 0 guarded).
            var serverRows = (from a in db.Items
                     join b in db.AssetTransferDetails on a.ItemID equals b.RefItemId into temp1
                     from b in temp1.DefaultIfEmpty()
                     join c in db.AssetTransferMasters on b.AssetEntryId equals c.AssetEntryId into temp2
                     from c in temp2.DefaultIfEmpty()
                     join d in db.Accountss on b.AssetAccountId equals d.AccountsID into temp3
                     from d in temp3.DefaultIfEmpty()
                     join e in db.ItemUnits on b.UnitId equals e.ItemUnitID into temp4
                     from e in temp4.DefaultIfEmpty()
                     let ConvertedQty = (db.AssetToInventoryDetails.Where(x => x.AssetId == b.AssetitementryId).Sum(x => (decimal?)x.Quantity) ?? 0)

                     where (AssetAccount == 0 || b.AssetAccountId == AssetAccount) &&
                     (fromdate == "" || EF.Functions.DateDiffDay(c.AssetEntryDate, fdate) <= 0) &&
                     (todate == "" || EF.Functions.DateDiffDay(c.AssetEntryDate, tdate) >= 0) &&
                     (ddmc == 0 || c.McFromId == ddmc)


                     select new
                     {
                         cInvoiceNo = (long?)c.InvoiceNo,
                         ItemID = a.ItemID,
                         ItemCode = a.ItemCode,
                         ItemName = a.ItemName,
                         ItemUnitName = e.ItemUnitName,
                         bQuantity = (decimal?)b.Quantity,
                         ConvertedQty = ConvertedQty,
                         bPrice = (decimal?)b.Price,
                         AccountName = d.Name,
                         cAssetEntryDate = (DateTime?)c.AssetEntryDate,
                         cAssetEntryId = (long?)c.AssetEntryId,
                     }).ToList();

            var v = serverRows.Select(o => new
                     {
                         invoice = o.cInvoiceNo ?? 0,
                         ItemID = o.ItemID,
                         Item = o.ItemCode + "-" + o.ItemName,
                         PriUnit = o.ItemUnitName,
                         NetQty = (o.bQuantity ?? 0) - o.ConvertedQty,
                         Price = o.bPrice ?? 0,
                         AssetAccountName = o.AccountName,
                         AssetEntryDate = o.cAssetEntryDate ?? default(DateTime),
                         AssetEntryId = o.cAssetEntryId ?? 0,
                     }).Where(o => o.NetQty > 0).Distinct();
            v = v.OrderBy(b => b.AssetEntryDate);
            var data = v.Skip(skip).Take(pageSize).ToList();
            recordsTotal = v.Count();
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }

        public ActionResult Edit(long? id)
        {

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            if (mcchk != null)
            {
                var mcs = db.MCs.Select(s => new
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


            var warningMsg = db.EnableSettings.Where(a => a.EnableType == "WarningOnSave").FirstOrDefault();
            var warnmsg = warningMsg != null ? (warningMsg.Status == Status.active ? 0 : 1) : 1;
            ViewBag.WarnMsg = warnmsg;



            AssetTransferMasters asset = db.AssetTransferMasters.Find(id);

            var MlaSTran = db.EnableSettings.Where(a => a.EnableType == "MLASTran").FirstOrDefault();
            var MlaSTrans = MlaSTran != null ? MlaSTran.Status : Status.inactive;
            ViewBag.MLASTran = MlaSTrans;

            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            //.........AssetMasters..........//for load the datas

            AssetTransferViewModel vmodel = new AssetTransferViewModel();
            vmodel.AssetEntryId = (long)id;
            vmodel.InvoiceNo = asset.InvoiceNo;
            vmodel.AssetEntryDate = asset.AssetEntryDate;
            vmodel.TotalAssetValue = asset.TotalAssetValue;
            vmodel.McFromId = asset.McFromId;

            var FDate = db.FinancialYears.Select(a => a.Start).FirstOrDefault();

            var userpermission = User.IsInRole("All StockTransfers Entry");
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            ViewBag.preEntry = db.StockTransfers.Where(a => a.Id < id && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.Id).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.StockTransfers.Where(a => a.Id > id && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.Id).DefaultIfEmpty().Min();

            companySet();


            var EditPermission = User.IsInRole("Disable StkTransfer Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "StockTransfer", UserId);

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;

            return View(vmodel);
        }
        [RedirectingAction]
        [HttpPost]

        public JsonResult Edit(string[][] array, string[] mtdata, string action)
        {
            bool stat = false;
            string msg;
            string itemname;
            long Vou = Convert.ToInt64(mtdata[0]);
            var Exists = db.AssetTransferMasters.Any(c => c.InvoiceNo == Vou);
            var UserId = User.Identity.GetUserId();


            //----------AssetMasters--------------//

            var TheId = Convert.ToInt64(mtdata[8]);
            AssetTransferMasters STs = db.AssetTransferMasters.Find(TheId);

            STs.InvoiceNo = Convert.ToInt64(mtdata[0]);
            STs.AssetEntryDate = DateTime.Parse(mtdata[1], new CultureInfo("en-GB"));
            STs.TotalAssetValue = Convert.ToDecimal(mtdata[6]);
            db.Entry(STs).State = EntityState.Modified;
            db.SaveChanges();

            ////--------------stockmaster-------------//



            //.........AssetDetails..............//

            Int64 MtId = STs.AssetEntryId;

            db.AssetTransferDetails.RemoveRange(db.AssetTransferDetails.Where(a => a.AssetEntryId == MtId));
            bool delete = com.DeleteAllAccountTransaction("Asset From Inventory", MtId);

            AssetTransferDetail ST = new AssetTransferDetail();
            long ItemId = 0;
            if (ST != null)
            {

                foreach (var arr in array)
                {
                    ST.AssetEntryId = MtId;

                    ItemId = Convert.ToInt64(arr[0]);
                    var demoitem = db.Items.Where(a => a.ItemID == ItemId).FirstOrDefault();
                    ST.AssetName = demoitem.ItemName;
                    ST.Barcode = arr[1];
                    ST.UnitId = Convert.ToInt64(arr[2]);
                    ST.Quantity = Convert.ToDecimal(arr[3]);
                    ST.Price = Convert.ToDecimal(arr[4]);
                    ST.TotalPrice = Convert.ToDecimal(arr[6]);

                    if (arr[8] != "")
                        ST.DepreciationPercentage = Convert.ToInt64(arr[8]);
                    else
                        ST.DepreciationPercentage = 0;

                    ST.AssetAccountId = Convert.ToInt64(arr[9]);
                    ST.DepreciationAccountId = Convert.ToInt64(arr[10]);

                    ST.RefItemId = Convert.ToInt64(arr[0]);

                    db.AssetTransferDetails.Add(ST);

                    db.SaveChanges();

                    //---------------- Account Transaction --------------//
                    var StockAccount = db.Accountss.Where(x => x.Name == "Asset From Inventory").FirstOrDefault();
                    com.addAccountTrasaction(0,ST.TotalPrice, StockAccount.AccountsID, "Asset From Inventory", MtId, DC.Credit, STs.AssetEntryDate, null, null, null, null);

                    com.addAccountTrasaction(ST.TotalPrice,0, (long)ST.AssetAccountId, "Asset From Inventory", MtId, DC.Debit, STs.AssetEntryDate, null, null, null, null);

                }

                //---------stocktransfer detail----------//


                var StockTransferId = db.AssetTransferMasters.Find(TheId).StockTransferId;
                if (StockTransferId != null)
                {
                    StockTransfer stockTrnsfrObj = db.StockTransfers.Find(StockTransferId);

                    if (stockTrnsfrObj != null)
                        db.StockTransfers.Remove(stockTrnsfrObj);
                    db.SaveChanges();

                    db.StockTransferItems.RemoveRange(db.StockTransferItems.Where(a => a.StockTransferId == StockTransferId));
                    db.SaveChanges();
                }





            }
            msg = "Asset From Inventory Successfully Updated..";
            stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }
        //.......... get the details in edit..........//

        public ActionResult GetSTItems(long EntryID)
        {
            var ConD = (from a in db.AssetTransferDetails
                        join b in db.Items on a.RefItemId equals b.ItemID
                        join e in db.Accountss on a.AssetAccountId equals e.AccountsID
                        join f in db.Accountss on a.DepreciationAccountId equals f.AccountsID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()


                        where a.AssetEntryId == EntryID
                        select new
                        {
                            a.AssetitementryId,
                            a.AssetName,
                            a.Barcode,
                            a.Quantity,
                            a.UnitId,
                            a.Price,
                            a.TotalPrice,
                            a.DepreciationPercentage,
                            a.AssetAccountId,
                            a.DepreciationAccountId,
                            b.ItemID,
                            b.ItemCode,
                            b.ItemName,
                            ItemWithCode = b.ItemCode + " - " + b.ItemName,
                            e.Name,

                            PrintName = f.Name,
                            c.ItemUnitName,

                            b.ItemUnitID,

                            b.SubUnitId,
                            PriUnit = c.ItemUnitName,
                            SubUnit = d.ItemUnitName,
                            ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,

                            b.OpeningStock,
                            b.MinStock,
                            b.SellingPrice,
                            b.PurchasePrice,
                            b.BasePrice,
                            b.MRP,
                            b.KeepStock,
                        }).AsEnumerable().Select(o => new
                        {
                            o.AssetitementryId,
                            o.AssetName,

                            o.Barcode,
                            o.Quantity,
                            o.UnitId,
                            o.Price,
                            o.TotalPrice,
                            o.DepreciationPercentage,
                            o.AssetAccountId,
                            o.DepreciationAccountId,

                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.Name,

                            o.PrintName,
                            o.ItemUnitName,
                            o.ItemUnitID,
                            o.SubUnitId,
                            // o.note,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            OpeningStock = o.OpeningStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ConFactor,
                            o.SellingPrice,
                            o.PurchasePrice,
                            o.BasePrice,
                            o.MRP,
                            price = (o.PurchasePrice != 0) ? o.PurchasePrice : o.MRP,
                            o.KeepStock,
                        }).ToList();
            return LegacyJson(ConD);
        }


        private bool BillExist(long STNo)
        {
            var Exists = db.AssetTransferMasters.Any(c => c.InvoiceNo == (STNo));
            bool res = (Exists) ? true : false;
            return res;
        }


        public JsonResult ItemSearch(string q, string x, string page)
        {
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectUserFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                string[] items = q.Split(' ');
                List<SelectUserFormat> serialisedJson3 = new List<SelectUserFormat>();
                foreach (var qa in items)
                {
                    List<SelectUserFormat> serialisedJson2 = db.Items.Where(p => (p.Status == Status.active) && p.ItemName.ToLower().Contains(qa.ToLower()) || p.ItemCode.ToLower().Contains(qa.ToLower()) || p.Barcode.ToLower().Contains(q.ToLower()) || p.ItemName.Contains(qa) || p.ItemCode.Contains(qa) || p.Barcode.Contains(qa))
                                      .Select(b => new SelectUserFormat
                                      {
                                          text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                                          id = b.ItemName
                                      })
                                      .OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
                    serialisedJson3.AddRange(serialisedJson2);
                }
                serialisedJson = serialisedJson3;
            }
            else
            {
                serialisedJson = db.Items.Where(p => p.Status == Status.active).Select(b => new SelectUserFormat
                {
                    text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                    id = b.ItemName
                }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }//
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectUserFormat() { id = stt, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }


        [RedirectingAction]

        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssetTransferMasters bom = db.AssetTransferMasters.Find(id);
            if (bom == null)
            {
                return NotFound();
            }
            return PartialView(bom);
        }

        [RedirectingAction]

        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("Delete")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var chk = DeleteItem(id);
            if (chk == "Success")
            {
                stat = true;
                msg = "Successfully deleted Asset From Inventory.";
            }
            else
            {
                stat = false;
                msg = chk;//"Looks like something went wrong. Please check your form.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        [HttpPost]

        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;

            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = DeleteItem(arr);
                if (chk == "Success")
                {
                    count++;
                }
                else
                {
                    notdel++;
                }
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Items, Unable to Delete " + notdel + " Items. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Items.", true);
            }
            else
            {
                Success("Deleted " + count + " Items.", true);
            }
            return RedirectToAction("Index", "AssetFromInventory");
        }
        private string DeleteItem(long id)
        {
            var Msg = chkDeleteWithMsg(id);
            if (Msg != null)
            {
                return Msg;
            }
            else
            {
                return DeleteFn(id);
            }
        }

        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            var exist = (from a in db.AssetTransferMasters
                         from b in db.AssetTransferDetails
                         from c in db.AssetToInventoryDetails
                         where a.AssetEntryId == id && a.AssetEntryId == b.AssetEntryId &&
                         b.AssetitementryId == c.AssetId
                         select c.AssetName);
            if (exist.FirstOrDefault() != null)
            {
                msg = "Asset Already used in Asset To Inventory !!";
            }
            else
            {
                msg = null;
            }

            return msg;
        }

        public string DeleteFn(long id)
        {



            AssetTransferMasters bom = db.AssetTransferMasters.Find(id);
            var bomItem = db.AssetTransferDetails.Where(a => a.AssetEntryId == id);


            //...........Delete from AssetTransfer Details.....//

            if (bomItem != null)
            {
                db.AssetTransferDetails.RemoveRange(db.AssetTransferDetails.Where(a => a.AssetEntryId == id));
            }

            //...........Delete from AssetTransfer Masters.....//

            if (bom != null)
            {
                db.AssetTransferMasters.RemoveRange(db.AssetTransferMasters.Where(a => a.AssetEntryId == id));
            }
            bool delete = com.DeleteAllAccountTransaction("Asset From Inventory", id);
            //..........Delete from table StockTransfers..............//



            //    //..........Delete from table StockTransferItems...........//




            db.SaveChanges();

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "AssetFromInventory", "AssetFromInventorys", findip(), id, "Successfully Deleted Asset From Inventory");
            db.SaveChanges();
            return "Success";
        }


        public JsonResult Search(string q, string x, string page)
        {

            var start = Convert.ToInt32(page);

            List<SelectMultiFormat> serialisedJson;
            string stt = "All";


            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {

                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID
                           where (b.Parent == 2 || b.Parent == 4)
                           && (b.AccountsGroupID != 8 && b.AccountsGroupID != 9 && b.AccountsGroupID != 11 && b.AccountsGroupID != 21 && b.AccountsGroupID != 22 && b.AccountsGroupID != 23)
                           || (a.Group == 2 || a.Group == 4)
                           && (a.Name.ToLower().Contains(q.ToLower()) || a.Alias.ToLower().Contains(q.ToLower()) || a.Alias.Contains(q) || a.Name.Contains(q)) && (a.Status == Status.active) //&& a.Group != 23

                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).ToList();


                serialisedJson = hmt;
            }
            else
            {
                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID
                           where (b.Parent == 2 || b.Parent == 4)
                            && (b.AccountsGroupID != 8 && b.AccountsGroupID != 9 && b.AccountsGroupID != 11 && b.AccountsGroupID != 21 && b.AccountsGroupID != 22 && b.AccountsGroupID != 23)
                           || (a.Group == 2 || a.Group == 4)
                            && (a.Status == Status.active)

                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).ToList();

                serialisedJson = hmt;

            }
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectMultiFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectMultiFormat() { id = 0, text = "Select Account" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult SearchItem(string q, string x, string page)
        {

            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                string[] items = q.Split(' ');
                List<SelectFormat> serialisedJson3 = new List<SelectFormat>();
                foreach (var qa in items)
                {
                    List<SelectFormat> serialisedJson2 = db.Items.Where(p => (p.Status == Status.active) && p.ItemName.ToLower().Contains(qa.ToLower()) || p.ItemCode.ToLower().Contains(qa.ToLower()) || p.Barcode.ToLower().Contains(q.ToLower()) || p.ItemName.Contains(qa) || p.ItemCode.Contains(qa) || p.Barcode.Contains(qa))
                                      .Select(b => new SelectFormat
                                      {
                                          text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                                          id = b.ItemID
                                      })
                                      .OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
                    serialisedJson3.AddRange(serialisedJson2);
                }
                serialisedJson = serialisedJson3;
            }
            else
            {
                serialisedJson = db.Items.Where(p => p.Status == Status.active).Select(b => new SelectFormat
                {
                    text = b.ItemCode + "-" + b.ItemName, //each json object will have 
                    id = b.ItemID
                }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }//
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult SearchDepreciation(string q, string x, string page)
        {

            var start = Convert.ToInt32(page);

            List<SelectMultiFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           where ((b.Parent == 29 || b.Parent == 30) || (b.AccountsGroupID == 29 || b.AccountsGroupID == 30))
                           && (a.Name.ToLower().Contains(q.ToLower()) || a.Alias.ToLower().Contains(q.ToLower()) || a.Alias.Contains(q) || a.Name.Contains(q)) && (a.Status == Status.active) //&& a.Group != 23
                           //&& (userpermission == true || a.CreatedBy == UserId))
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = a.Name
                           }).OrderBy(b => b.text).ToList();
                serialisedJson = hmt;
            }
            else
            {
                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           join c in db.Customers on a.AccountsID equals c.Accounts into custs
                           from c in custs.DefaultIfEmpty()
                           where ((b.Parent == 29 || b.Parent == 30) || (b.AccountsGroupID == 29 || b.AccountsGroupID == 30))
                           && (a.Status == Status.active)//&& a.Group != 23
                                                         // && (userpermission == true || a.CreatedBy == UserId)
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = a.Name
                           }).OrderBy(b => b.text).ToList();

                serialisedJson = hmt;

            }
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectMultiFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectMultiFormat() { id = 0, text = "Select Account" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult UnitSearch(string q, string x, string page)
        {

            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                string[] items = q.Split(' ');
                List<SelectFormat> serialisedJson3 = new List<SelectFormat>();
                foreach (var qa in items)
                {
                    List<SelectFormat> serialisedJson2 = db.ItemUnits.Select(b => new SelectFormat
                    {
                        text = b.ItemUnitName, //each json object will have 
                        id = b.ItemUnitID
                    })
                                      .OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
                    serialisedJson3.AddRange(serialisedJson2);
                }
                serialisedJson = serialisedJson3;
            }
            else
            {
                serialisedJson = db.ItemUnits.Select(b => new SelectFormat
                {
                    text = b.ItemUnitName, //each json object will have 
                    id = b.ItemUnitID
                }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }//
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }


    }
}


