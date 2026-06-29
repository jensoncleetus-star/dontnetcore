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
    public class AssetTransfersController : BaseController
    {
        public AssetTransfersController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        ApplicationDbContext db;
        Common com;
        // GET: StockTransfer
        // [QkAuthorize(Roles = "Dev,AssetTransfers List")]
        public ActionResult Index()
        {
            
            

            //               ID = (int)e,
            //               Name = e.ToString()

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindStkTrans").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;
           

            return View();
        }

        [HttpPost]
       
        public ActionResult GetAssetTransfer(long? AssetID, string FromDate,string ToDate)
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
            var uEdit = User.IsInRole("Edit AssetTransfers");
            var uDelete = User.IsInRole("Delete AssetTransfers");

            var v = (from a in db.AssetTransferMasters
                     
            where
                     ( AssetID == null || a.InvoiceNo == AssetID) &&
                     (a.VendorName == null )&&
                     (FromDate == "" || FromDate == null || EF.Functions.DateDiffDay(a.AssetEntryDate, fdate) <= 0) &&
                     (ToDate == "" || ToDate == null || EF.Functions.DateDiffDay(a.AssetEntryDate, tdate) >= 0)
                    

                     select new
                     {
                         Id = a.AssetEntryId,
                         AssetID = a.InvoiceNo,
                         Date = a.AssetEntryDate,
                         TotalAssetValue =a.TotalAssetValue
                        


                     }).ToList().
            Select(b => new
            {
                b.Id,
                b.AssetID,
                b.Date,
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


        // [QkAuthorize(Roles = "Dev,StockTransfer Entry")]
        public ActionResult Create()
        {
            var mcs = db.MCs.Select(s => new
            {
                McId = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.MCbag = QkSelect.List(mcs, "McId", "Name");


            var UserId = User.Identity.GetUserId();
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
            var today = Convert.ToDateTime(System.DateTime.Now);
            var warningMsg = db.EnableSettings.Where(a => a.EnableType == "WarningOnSave").FirstOrDefault();
            var warnmsg = warningMsg != null ? (warningMsg.Status == Status.active ? 0 : 1) : 1;
            ViewBag.WarnMsg = warnmsg;

            
            var STTo = new AssetTransferViewModel
            {
                InvoiceNo= GetEntryNo(),
                AssetEntryDate = today
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

            //field mapping

            return View(STTo);
        }

        [RedirectingAction]
        [HttpPost]
   //     [QkAuthorize(Roles = "Dev,StockTransfer Entry")]
        public JsonResult Create(string[][] array, string[] mtdata, string action, AssetTransferViewModel bsmodel)
        {
            long billno = Convert.ToInt64(mtdata[0]);
            var Billcheck = db.AssetTransferMasters.Where(a => a.InvoiceNo== billno).Any();
            bool stat = false;
            string msg = "";
            if (!Billcheck)
            {
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
                    sl.AssetEntryDate= DateTime.Parse(mtdata[1], new CultureInfo("en-GB"));
                    sl.TotalAssetValue = Convert.ToDecimal(mtdata[5]);






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
                       asset.DepreciationPercentage = Convert.ToInt64(arr[8]);
                       asset.AssetAccountId = Convert.ToInt64(arr[9]);
                       asset.DepreciationAccountId = Convert.ToInt64(arr[10]);
                       asset.RefItemId = Convert.ToInt64(arr[0]);
                        asset.DeleteYN = "No";

                        db.AssetTransferDetails.Add(asset);
                        db.SaveChanges();

                        //changing the status of item table

                            var assetstatus = db.Items.Find(asset.RefItemId);
                            assetstatus.Status = Status.inactive;
                            db.SaveChanges();
                        



                    }

                   
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


        public ActionResult Edit(long? id)
        {

           

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            
            var warningMsg = db.EnableSettings.Where(a => a.EnableType == "WarningOnSave").FirstOrDefault();
            var warnmsg = warningMsg != null ? (warningMsg.Status == Status.active ? 0 : 1) : 1;
            ViewBag.WarnMsg = warnmsg;



            

            var MlaSTran = db.EnableSettings.Where(a => a.EnableType == "MLASTran").FirstOrDefault();
            var MlaSTrans = MlaSTran != null ? MlaSTran.Status : Status.inactive;
            ViewBag.MLASTran = MlaSTrans;

            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            AssetTransferViewModel vmodel = new AssetTransferViewModel();

            vmodel = (from a in db.AssetTransferMasters
                     
                     
                      
                      where a.AssetEntryId == id

                      select new AssetTransferViewModel
                      {
                          InvoiceNo = a.InvoiceNo,
                          AssetEntryDate= a.AssetEntryDate,
                          TotalAssetValue=a.TotalAssetValue
                         
                          
                          
                         
                      }).FirstOrDefault();

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

            //field mapping

            //dummy table operations
           

            return View(vmodel);
        }
        [RedirectingAction]
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Edit StockTransfer")]
        public JsonResult Edit(string[][] array, string[] mtdata, string action)
        {
            bool stat = false;
            string msg;
            string itemname;
            long Vou = Convert.ToInt64(mtdata[0]);
            var Exists = db.AssetTransferMasters.Any(c => c.InvoiceNo == Vou);
            var UserId = User.Identity.GetUserId();

            var TheId = Convert.ToInt64(mtdata[7]);
            AssetTransferMasters STs = db.AssetTransferMasters.Find(TheId);



            STs.InvoiceNo = Convert.ToInt64(mtdata[0]);
            STs.AssetEntryDate = DateTime.Parse(mtdata[1], new CultureInfo("en-GB"));
            STs.TotalAssetValue = Convert.ToDecimal(mtdata[5]);




            db.Entry(STs).State = EntityState.Modified;
            db.SaveChanges();

            Int64 MtId = STs.AssetEntryId;

            db.AssetTransferDetails.RemoveRange(db.AssetTransferDetails.Where(a => a.AssetEntryId == MtId));
            AssetTransferDetail ST = new AssetTransferDetail();
            long ItemId = 0;

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
                ST.DepreciationPercentage = Convert.ToInt64(arr[8]);
                ST.AssetAccountId = Convert.ToInt64(arr[9]);
                ST.DepreciationAccountId = Convert.ToInt64(arr[10]);
                
                ST.RefItemId = Convert.ToInt64( arr[0]);
                ST.DeleteYN = "No";
                db.AssetTransferDetails.Add(ST);
                db.SaveChanges();

                // changing the status of item table
                var assetstatus = db.Items.Find(ST.RefItemId);
                assetstatus.Status = Status.inactive;

                db.SaveChanges();

            }




            msg = "AssetTransfers Successfully Updated..";
            stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }  
        // get the details in edit
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
                            f.PrintName,
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
                        {   o.AssetitementryId,
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
       // [QkAuthorize(Roles = "Dev,Delete BOM")]
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
        //[QkAuthorize(Roles = "Dev,Delete BOM")]
        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("Delete")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            stat = DeleteFn(id);
            msg = "Successfully deleted AssetTransfers.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
       // [QkAuthorize(Roles = "Dev,Delete BOM")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;

            foreach (var arr in bill)
            {
                var chk = (DeleteFn(arr) == true) ? count++ : count;
            }

            Success("Deleted " +  " Asset Transfer.", true);
            return RedirectToAction("Index", "AssetTransfers");
        }
        private Boolean DeleteItem(long id)
        {
            var Msg = chkDeleteWithMsg(id);
            if (Msg != null)
            {
                return false;
            }
            else
            {
                return DeleteFn(id);
            }
        }

        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            if (db.AssetTransferDetails.Any(c => c.AssetEntryId == id))
            {
                msg = "Asset Already used in Production !!";
            }
           
            else
            {
                msg = null;
            }

            return msg;
        }

        public bool DeleteFn(long id)
        {

            int i=0;
            List<AssetTransferDetail> AssetList = new List<AssetTransferDetail>();
            AssetList = db.AssetTransferDetails.Where(a => a.AssetEntryId == id).ToList();
            var AssetItems = db.Items.Find(AssetList[i].RefItemId);

            if(AssetItems !=null)
            {
                AssetItems.Status = Status.active;
            }
          

            AssetTransferMasters bom = db.AssetTransferMasters.Find(id);
            var bomItem = db.AssetTransferDetails.Where(a => a.AssetEntryId == id);

            

            if (bomItem != null)
            {
                db.AssetTransferDetails.RemoveRange(db.AssetTransferDetails.Where(a => a.AssetEntryId == id));
            }
            if (bom != null)
            {
                db.AssetTransferMasters.RemoveRange(db.AssetTransferMasters.Where(a => a.AssetEntryId == id));
            }
            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "AssetTransfer", "AssetTransfers", findip(), id, "Successfully Deleted AssetTransfers");
            db.SaveChanges();
            return true;
        }







        public JsonResult Search(string q, string x, string page)
        {

            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectMultiFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           join c in db.Customers on a.AccountsID equals c.Accounts into custs
                           from c in custs.DefaultIfEmpty()
                           where b.AccountsGroupID == 4 && (a.Name.ToLower().Contains(q.ToLower()) || a.Alias.ToLower().Contains(q.ToLower()) || a.Alias.Contains(q) || a.Name.Contains(q)) && (a.Status == Status.active) //&& a.Group != 23
                           //&& (userpermissio.n == true || a.CreatedBy == UserId))
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
                serialisedJson = hmt;
            }
            else
            {
                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           join c in db.Customers on a.AccountsID equals c.Accounts into custs
                           from c in custs.DefaultIfEmpty()
                           where (b.AccountsGroupID == 4) && (a.Status == Status.active)//&& a.Group != 23
                                                                                        // && (userpermission == true || a.CreatedBy == UserId)
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

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
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectMultiFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           join c in db.Customers on a.AccountsID equals c.Accounts into custs
                           from c in custs.DefaultIfEmpty()
                           where a.Group == 29 && (a.Name.ToLower().Contains(q.ToLower()) || a.Alias.ToLower().Contains(q.ToLower()) || a.Alias.Contains(q) || a.Name.Contains(q)) && (a.Status == Status.active) //&& a.Group != 23
                           //&& (userpermission == true || a.CreatedBy == UserId))
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
                serialisedJson = hmt;
            }
            else
            {
                var hmt = (from a in db.Accountss
                           join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                           from b in gp.DefaultIfEmpty()
                           join c in db.Customers on a.AccountsID equals c.Accounts into custs
                           from c in custs.DefaultIfEmpty()
                           where (a.Group == 13) && (a.Status == Status.active)//&& a.Group != 23
                                                                               // && (userpermission == true || a.CreatedBy == UserId)
                           select new SelectMultiFormat
                           {
                               text = a.Name,
                               id = a.AccountsID,
                               Name = b.Name
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

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


