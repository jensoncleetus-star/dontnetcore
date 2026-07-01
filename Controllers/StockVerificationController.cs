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
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class StockVerificationController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public StockVerificationController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // create 
        //[QkAuthorize(Roles = "Dev,Create StockVerification")]
        public ActionResult Create()
        {
            var SV = new SVViewModel
            {
                Voucher = VoucherNo(),
                Date = System.DateTime.Now.ToShortDateString()
            };
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
            }
            ViewBag.Item = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                           }, "Value", "Text", 1);

            var Bnch = db.Branchs
             .Select(s => new
             {
                 Id = s.BranchID,
                 Name = s.BranchName
             }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            companySet();
            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            ViewBag.BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

            var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
            ViewBag.BCEnable = enable != null ? enable.Status : Status.inactive;
           
            return View(SV);
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Create StockVerification")]
        public JsonResult Create(SVViewModel vmodel)
        {
            string msg;
            bool stat = false;
            var UserId = User.Identity.GetUserId();
          
            if (!BillExist(Convert.ToString(vmodel.Voucher)))
            {
             
                   
                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                var vNo = Maxvoucher();

                StockVerification SV = new StockVerification();

                SV.Voucher = vmodel.Voucher;
                SV.VoNo = vNo;
                SV.Date = Convert.ToDateTime(vmodel.Date);

                SV.Note = vmodel.Note;
                SV.Remarks = vmodel.Remarks;

                SV.Status = Status.active;
                SV.CreatedBy = UserId;
                SV.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                SV.Branch = BranchID;
                SV.editable = choice.Yes;

                db.StockVerifications.Add(SV);
                db.SaveChanges();
                Int64 stkvId = SV.StockVerificationId;

                SVItems svItem = new SVItems();
                foreach (var arr in vmodel.svItemzz)
                {
                    svItem.StockVerification = stkvId;
                    svItem.Item = arr.Item;
                    svItem.ItemUnit = arr.ItemUnitId;
                    svItem.CSPcs = arr.CSPcs;
                    svItem.CSqty = arr.CSqty;
                    svItem.PSPcs = arr.PSPcs;
                    svItem.PSqty = arr.PSqty;
                    svItem.SDPcs = arr.SDPcs;
                    svItem.SDqty = arr.SDqty;
                    db.SVItemss.Add(svItem);
                    db.SaveChanges();
                }


                com.addlog(LogTypes.Created, UserId, "StockVerification", "StockVerifications", findip(), stkvId, "Successfully added Stock Verifications details");
                if (vmodel.action == "print")
                {
                    var summary = (from a in db.StockVerifications
                                   where a.StockVerificationId == stkvId
                                   select new
                                   {
                                       a.Voucher,
                                       a.VoNo,
                                       a.Date,
                                       a.Note,
                                       a.Remarks,

                                   }).FirstOrDefault();

                    var item = db.SVItemss.Where(n => n.StockVerification == stkvId).Select(b => new
                    {
                        ItemId = b.Item,
                        CSPcs = b.CSPcs,
                        CSQty = b.CSqty,
                        PSPcs = b.PSPcs,
                        PSQty = b.PSqty,
                        SDPcs = b.SDPcs,
                        SDQty = b.SDqty,
                        ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                        ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                        ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault(),
                    }).ToList();

                    //pending stock
                    //pending stock
                    var ItemIdList = item.Select(a => a.ItemId).ToArray();
                    var pstock = GetItemStock(ItemIdList);

                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, pstock } };
                }
                else
                {
                    msg = "Successfully Created Stock Verification details.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, data = vmodel, message = msg } };
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

        //[QkAuthorize(Roles = "Dev,StockVerification List")]
        public ActionResult Index()
        {
            ViewBag.PayFrom = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);
            ViewBag.PayTo = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);

            return View();
        }
        //[QkAuthorize(Roles = "Dev,StockVerification List")]
        public JsonResult GetData(string InvoiceNo, string FromDate, string ToDate, long? PayFrom, long? PayTo)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;

            DateTime? fdate = null;
            DateTime? tdate = null;

            string search = Request.Form.GetValues("search[value]")[0];
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var uDev = User.IsInRole("Dev");
            var uStockVerificationView = User.IsInRole("View StockVerification");
            var uEdit = User.IsInRole("Edit StockVerification");
            var uDelete = User.IsInRole("Delete StockVerification");

            var v = (from a in db.StockVerifications
                     where (a.editable == choice.Yes)
                     //(PayTo == 0 || PayTo == null || a.PayTo == PayTo))
                     join g in db.Users on a.CreatedBy equals g.Id
                     select new
                     {
                         VoucherNo = a.Voucher,
                         a.StockVerificationId,
                         a.Date,
                         a.Note,
                         a.Remarks,
                         UserName = g.UserName,
                         a.editable,
                         a.CreatedDate

                     }).AsEnumerable().Select(o => new
                     {
                         o.VoucherNo,
                         o.StockVerificationId,
                         o.Date,
                         o.Remarks,
                         o.Note,
                         o.UserName,
                         o.editable,
                         Dev = uDev,
                         Details = uStockVerificationView,
                         Edit = uEdit,
                         Delete = uDelete,
                         o.CreatedDate
                     });

            ////search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.VoucherNo.ToString().ToLower().Contains(search.ToLower()));
            }

            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,Edit StockVerification")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockVerification stv = db.StockVerifications.Find(id);
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
            }
            var Bnch = db.Branchs
                   .Select(s => new
                   {
                       Id = s.BranchID,
                       Name = s.BranchName
                   }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            if (stv == null)
            {
                return NotFound();
            }
            SVViewModel vmodel = new SVViewModel();
            vmodel.StockVerificationId = (long)id;
            vmodel.Voucher = stv.Voucher;
            vmodel.Date = stv.Date.ToString("dd-MM-yyyy");
            vmodel.Remarks = stv.Remarks;
            vmodel.Note = stv.Note;
            vmodel.VoNo = stv.VoNo;
            vmodel.Branch = stv.Branch;

            var itm = db.Items.Select(s => new
            {
                ID = s.ItemID,
                Name = s.ItemCode + " " + s.ItemName
            }).ToList();
            ViewBag.Item = QkSelect.List(itm, "ID", "Name");

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            ViewBag.BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

            var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
            ViewBag.BCEnable = enable != null ? enable.Status : Status.inactive;

            return View(vmodel);
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Edit StockVerification")]
        public JsonResult Edit(SVViewModel vmodel, long id)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            var today = Convert.ToDateTime(System.DateTime.Now);
            long Branch = 0;

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

            if (BranchCheck == Status.active)
            {
                Branch = vmodel.Branch;
            }
            else
            {
                Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
            }

            StockVerification stkvy = db.StockVerifications.Find(id);
            stkvy.Date = DateTime.Parse(vmodel.Date, new CultureInfo("en-GB"));
            stkvy.Note = vmodel.Note;
            stkvy.Remarks = vmodel.Remarks;

            db.Entry(stkvy).State = EntityState.Modified;
            db.SaveChanges();
            Int64 stkId = stkvy.StockVerificationId;


            var svItems = db.SVItemss.Where(a => a.StockVerification == stkId).FirstOrDefault();
            if (svItems != null)
            {
                db.SVItemss.RemoveRange(db.SVItemss.Where(a => a.StockVerification == stkId));
                db.SaveChanges();
            }

            SVItems svItem = new SVItems();
            foreach (var arr in vmodel.svItemzz)
            {
                svItem.StockVerification = stkId;
                svItem.Item = arr.Item;
                svItem.ItemUnit = arr.ItemUnitId;
                svItem.CSPcs = arr.CSPcs;
                svItem.CSqty = arr.CSqty;
                svItem.PSPcs = arr.PSPcs;
                svItem.PSqty = arr.PSqty;
                svItem.SDPcs = arr.SDPcs;
                svItem.SDqty = arr.SDqty;
                db.SVItemss.Add(svItem);
                db.SaveChanges();
            }


            com.addlog(LogTypes.Updated, UserId, "StockVerification", "StockVerifications", findip(), stkId, "Successfully Updated StockVerification");
            if (vmodel.action == "print")
            {
                var summary = (from a in db.StockVerifications
                               where a.StockVerificationId == stkId
                               select new
                               {
                                   a.Voucher,
                                   a.VoNo,
                                   a.Date,
                                   a.Note,
                                   a.Remarks,

                               }).FirstOrDefault();

                var item = db.SVItemss.Where(n => n.StockVerification == stkId).Select(b => new
                {
                    ItemId = b.Item,
                    CSPcs = b.CSPcs,
                    CSQty = b.CSqty,
                    PSPcs = b.PSPcs,
                    PSQty = b.PSqty,
                    SDPcs = b.SDPcs,
                    SDQty = b.SDqty,
                    ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                    ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                    ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault(),
                }).ToList();


                //pending stock
                var ItemIdList = item.Select(a => a.ItemId).ToArray();
                var pstock = GetItemStock(ItemIdList);

                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, pstock } };
            }
            else
            {
                msg = "Successfully Updated Stock Verification details.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, data = vmodel, message = msg } };
            }
        }

        //[QkAuthorize(Roles = "Dev,Delete StockVerification")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockVerification bom = db.StockVerifications.Find(id);
            if (bom == null)
            {
                return NotFound();
            }
            return PartialView(bom);
        }

        //[QkAuthorize(Roles = "Dev,Delete StockVerification")]
        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            StockVerification stk = db.StockVerifications.Find(id);
            var svItem = db.SVItemss.Where(a => a.StockVerification == id);
            if (svItem != null)
            {
                db.SVItemss.RemoveRange(db.SVItemss.Where(a => a.StockVerification == id));
            }
            db.StockVerifications.Remove(stk);
            com.addlog(LogTypes.Deleted, UserId, "StockVerification", "StockVerifications", findip(), id, "Successfully Deleted Stock Verifications");
            db.SaveChanges();
            stat = true;
            msg = "Successfully deleted StockVerifications.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,View StockVerification")]
        public ActionResult Details(long? id)
        {
            SVViewModel vmodel = new SVViewModel();
            vmodel = (from a in db.StockVerifications

                      where a.StockVerificationId == id
                      select new SVViewModel
                      {
                          StockVerificationId = a.StockVerificationId,
                          Date = a.Date.ToString(),
                          Voucher = a.Voucher,
                          Remarks = a.Remarks,
                          Note = a.Note,
                      }).FirstOrDefault();
            vmodel.svItemzz = db.SVItemss.Where(a => a.StockVerification == id)
            .Select(b => new SVItemViewModel
            {
                ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault(),
                CSPcs = b.CSPcs,
                CSqty = b.CSqty,
                PSPcs = b.PSPcs,
                PSqty = b.PSqty,
                SDPcs = b.SDPcs,
                SDqty = b.SDqty

            }).ToList();

            return View(vmodel);
        }

        [HttpGet]
        public JsonResult GetSVItems(long EntryID)
        {
            var ConD = (from a in db.SVItemss
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                        from e in cat.DefaultIfEmpty()
                        join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                        from f in taxss.DefaultIfEmpty()
                        where a.StockVerification == EntryID
                        select new
                        {
                            a.Item,
                            a.CSPcs,
                            a.CSqty,
                            a.PSPcs,
                            a.PSqty,
                            a.SDPcs,
                            a.SDqty,
                            a.ItemUnit,

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

                        }).ToList();
            return Json(ConD);
        }


        public IList<RemainStkViewModel> GetItemStock(long[] ItemId)
        {

            var v = (from b in db.Items
                     join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                     from c in primary.DefaultIfEmpty()
                     join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                     from d in second.DefaultIfEmpty()
                     join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                     from e in cat.DefaultIfEmpty()
                     join f in db.PEItemss on new { f1 = b.ItemID, f2 = db.PEItemss.Where(x => x.Item == b.ItemID && x.ItemUnitPrice != 0).Select(x => x.PEItemsId).Max() }
                     equals new { f1 = f.Item, f2 = f.PEItemsId } into pur
                     from f in pur.DefaultIfEmpty()

                     where (b.KeepStock == true) && !ItemId.Contains(b.ItemID)
                     orderby b.ItemID ascending
                     select new
                     {
                         ItemCode = b.ItemCode,
                         ItemName = b.ItemName,
                         ItemWithCode = b.ItemName,
                         b.ItemUnitID,
                         b.SubUnitId,
                         PriUnit = c.ItemUnitName,
                         SubUnit = d.ItemUnitName,
                         ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                         b.ItemID,
                         OpeningStock = b.OpeningStock != null ? b.OpeningStock : 0,
                         b.MinStock,
                         categoryname = e.ItemCategoryName,
                         b.KeepStock,
                         b.PartNumber,
                         PIUnitName = c.ItemUnitName,
                         SIUnitName = d.ItemUnitName,

                         cost = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnitPrice : b.PurchasePrice),
                         costu = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnit : b.ItemUnitID),
                     }).Distinct();

            v = v.OrderBy(b => b.ItemName);
            var data = v.ToList();
            var mydata =
                (from b in data

                 let PriPurchase = db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).ToList().Sum()
                 let SubPurchase = db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PIUnitName != b.SIUnitName).Select(a => a.ItemQuantity).ToList().Sum()

                 let PriSale = db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).ToList().Sum()
                 let SubSale = db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PIUnitName != b.SIUnitName).Select(a => a.ItemQuantity).ToList().Sum()

                 let PriPReturn = db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).ToList().Sum()
                 let SubPReturn = db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PIUnitName != b.SIUnitName).Select(a => a.ItemQuantity).ToList().Sum()

                 let PriSReturn = db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).ToList().Sum()
                 let SubSReturn = db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId && b.PIUnitName != b.SIUnitName).Select(a => a.ItemQuantity).ToList().Sum()

                 let PriAddAdj = db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).ToList().Sum()
                 let SubAddAdj = db.StockAdjustments.Where(a => a.ItemID == b.ItemID && b.PIUnitName != b.SIUnitName && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).ToList().Sum()

                 let PriLessAdj = db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).ToList().Sum()
                 let subLessAdj = db.StockAdjustments.Where(a => a.ItemID == b.ItemID && b.PIUnitName != b.SIUnitName && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).ToList().Sum()

                 // main item
                 let PriProdItem = db.GeneratedItem.Where(a => a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).ToList().Sum()
                 let SubProdItem = db.GeneratedItem.Where(a => a.Item == b.ItemID && a.Unit == b.SubUnitId && b.PIUnitName != b.SIUnitName).Select(a => a.Qty).ToList().Sum()

                 // compined item
                 let PriProdCItem = db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).ToList().Sum()
                 let SubProdCItem = db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId && b.PIUnitName != b.SIUnitName).Select(a => a.Quantity).ToList().Sum()

                 // unassemble -----                 
                 let PriUnItem = db.ConsumedItem.Where(a => a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).ToList().Sum()
                 let SubUnItem = db.ConsumedItem.Where(a => a.Item == b.ItemID && a.Unit == b.SubUnitId && b.PIUnitName != b.SIUnitName).Select(a => a.Qty).ToList().Sum()

                 // compined item
                 let PriUnCItem = db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).ToList().Sum()
                 let SubUnCItem = db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId && b.PIUnitName != b.SIUnitName).Select(a => a.Quantity).ToList().Sum()

                 select new
                 {
                     b.ItemID,
                     b.ItemCode,
                     b.ItemName,
                     b.ItemWithCode,
                     b.ItemUnitID,
                     b.SubUnitId,
                     PartNumber = (b.PartNumber != null && b.PartNumber != "") ? b.PartNumber : "",
                     PriUnit = (b.PriUnit != null) ? b.PriUnit : "",
                     SubUnit = (b.SubUnit != null) ? b.SubUnit : "",
                     b.categoryname,
                     OpeningStock = b.OpeningStock,
                     MinStock = (b.MinStock != null) ? b.MinStock : 0,
                     b.ConFactor,
                     stockable = b.KeepStock,

                     PriPurchase = (PriPurchase + (int)(SubPurchase / b.ConFactor)),
                     SubPurchase = (SubPurchase % b.ConFactor),


                     PriSale = (PriSale + (int)(SubSale / b.ConFactor)),
                     SubSale = (SubSale % b.ConFactor),

                     PriPReturn = (PriPReturn + (int)(SubPReturn / b.ConFactor)),
                     SubPReturn = (SubPReturn % b.ConFactor),


                     PriSReturn = (PriSReturn + (int)(SubSReturn / b.ConFactor)),
                     SubSReturn = (SubSReturn % b.ConFactor),

                     PriAddAdj = PriAddAdj,
                     SubAddAdj = SubAddAdj,
                     PriLessAdj = PriLessAdj,
                     subLessAdj = subLessAdj,

                     pritotal = ((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem)),
                     subtotal = ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem)),
                     total = (((b.OpeningStock + PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem) - (PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem)) * b.ConFactor) + ((SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem) - (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem)),

                     // stock in without opening stock
                     stockIn = ((PriPurchase + PriSReturn + PriAddAdj + PriProdItem + PriUnCItem) * b.ConFactor) + (SubPurchase + SubSReturn + SubAddAdj + SubProdItem + SubUnCItem),
                     stockout = ((PriSale + PriPReturn + PriLessAdj + PriProdCItem + PriUnItem) * b.ConFactor) + (SubSale + SubPReturn + subLessAdj + SubProdCItem + SubUnItem),

                     cost = (b.costu == b.ItemUnitID) ? b.cost : (b.cost * b.ConFactor)
                 }).Select(o => new RemainStkViewModel
                 {
                     ItemId = o.ItemID,
                     ItemName = o.ItemName,
                     ItemCode = o.ItemCode,
                     ItemUnit = o.PriUnit,
                     RemainQty = (o.total / o.ConFactor) * (o.cost),
                 }).Where(a => a.RemainQty != 0).OrderBy(a => a.ItemName).ToList();
            return mydata;

        }


        public long Maxvoucher()
        {
            Int64 SENo = 0;
            if ((db.StockVerifications.Select(p => p.VoNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                SENo = 1;
            }
            else
            {
                SENo = db.StockVerifications.Max(p => p.VoNo + 1);
            }

            return SENo;
        }
        public string VoucherNo(Int64 SENo = 0, string billNo = null)
        {
            Int32 number = db.CodePrefixs.Where(a => a.section == "StockVerification").Select(a => a.number).FirstOrDefault();
            var prefix = db.CodePrefixs.Where(a => a.section == "StockVerification").Select(a => a.prefix).FirstOrDefault();
            if (billNo == null)
            {
                if ((db.StockVerifications.Select(p => p.VoNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    if (number == 0)
                    {
                        billNo = prefix + 1;
                    }
                    else
                    {
                        billNo = prefix + number;
                    }
                }
                else
                {
                    SENo = db.StockVerifications.Max(p => p.VoNo + 1);
                    billNo = prefix + SENo;
                    if (BillExist(billNo))
                    {
                        billNo = VoucherNo(SENo, billNo);
                    }
                }
            }
            else
            {
                SENo = SENo + 1;
                billNo = prefix + SENo;
                if (BillExist(billNo))
                {
                    billNo = VoucherNo(SENo, billNo);
                }

            }
            return billNo;
        }
        private bool BillExist(string SENo)
        {
            var Exists = db.StockVerifications.Any(c => c.Voucher == SENo);
            bool res = (Exists) ? true : false;
            return res;
        }
        private bool BillExist(string SENo, long? recid = null)
        {
            bool res;
            if (recid != null)
            {
                var Exists = db.StockVerifications.Any(c => c.Voucher == SENo);
                res = (Exists) ? true : false;
            }
            else
            {
                var Exists = db.StockVerifications.Where(a => a.StockVerificationId != recid).Any(c => c.Voucher == SENo);
                res = (Exists) ? true : false;
            }
            return res;
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
