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
using QuickSoft.Models;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net;
using System.Drawing;
using System.Globalization;
using QuickSoft.ViewModel;
using System.Collections;
using System.Collections.Generic;
using System.Data;
namespace QuickSoft.Controllers
{

    public class AssetpurchaseController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public AssetpurchaseController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        [HttpGet]
        public ActionResult Create()
        {
            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            ViewBag.AssetAccounts = QkSelect.List(
                      new List<SelectListItem>
                      {
                        new SelectListItem { Selected = false, Text = "", Value = "0"},
                      }, "Value", "Text", 1);
            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "", Value = "0"},
                                 }, "Value", "Text", 0);
           
            var Createview = new AssetPurchaseViewModel
            {
                InvoiceNo = Convert.ToInt32(GetEntryNo()),

                AssetEntryDate = DateTime.Now
            };
            Createview.FieldMap = db.FieldMappings.Where(a => a.Section == "PReturn" && a.Status == Status.active).ToList();
            return View(Createview);
        }
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult GetAssets(string fromdate , string todate)
        {
            DateTime? fromdates = null;
            DateTime? todates = null;

            if (fromdate !=  "")
            {
                fromdates = DateTime.Parse(fromdate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (todate  != "")
            {
                todates = DateTime.Parse(todate, new CultureInfo("en-GB").DateTimeFormat);
            }

            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();


            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit FileDocument");
            var uDelete = User.IsInRole("Delete FileDocument");
            var v = (from a in db.AssetTransferMasters
                     join b in db.Suppliers on a.VendorName equals b.SupplierID into Vendor
                     from b in Vendor.DefaultIfEmpty()
                     where 
                     (fromdate == null || fromdate == "" || EF.Functions.DateDiffDay(a.AssetEntryDate, fromdates) <= 0)&&
                     (todate == null || todate == "" || EF.Functions.DateDiffDay(a.AssetEntryDate, todates) >= 0)
                     && (a.VendorName != null)



                     select new
                     {

                         a.AssetEntryId,
                         a.InvoiceNo,
                         a.PurchaseEntry,
                         b.SupplierName,
                         a.TotalAssetValue,
                         a.Vat,
                         a.AssetEntryDate,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete,
                     });


            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.InvoiceNo.ToString().ToLower().Equals(search.ToLower()));

            }
            //SORT

            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        [HttpPost]
        public ActionResult Create(string[][] array, string[] mtdata, AssetPurchaseViewModel assetvm)
        {

            Int64 assetid = 0;
            bool stat = false;
            string msg = "";
            long NameCheck = 0;
            long ValVat = 0;
            if (mtdata != null)
            {

                if ((mtdata[2] == null))
                {
                    NameCheck = 0;
                }
                else
                {
                    NameCheck = Convert.ToInt32(mtdata[2]);
                }
                if ((mtdata[4] == ""))
                {
                    ValVat = 0;
                }
                else
                {
                    ValVat = Convert.ToInt32(mtdata[4]);
                }


                AssetTransferMasters asset = new AssetTransferMasters
                {

                    InvoiceNo = Convert.ToInt32(mtdata[0]),
                    PurchaseEntry = Convert.ToString(mtdata[1]),
                    VendorName = NameCheck,
                    AssetEntryDate = DateTime.Parse(mtdata[3], new CultureInfo("en-GB")),
                    Vat = ValVat,
                    TotalAssetValue = Convert.ToDecimal(mtdata[5]),

                };
                db.AssetTransferMasters.Add(asset);
                db.SaveChanges();
                assetid = asset.AssetEntryId;
            }

            if (array != null)
            {
                var datez = DateTime.Parse(mtdata[3], new CultureInfo("en-GB"));
                foreach (var arr in array)
                {
                    AssetTransferDetail itm = new AssetTransferDetail();
                    itm.AssetEntryId = assetid;
                    itm.AssetName = Convert.ToString(arr[0]);
                    itm.Barcode = Convert.ToString(arr[1]);
                    itm.UnitId = Convert.ToInt32(arr[2]);
                    itm.Quantity = Convert.ToDecimal(arr[3]);
                    itm.Price = Convert.ToDecimal(arr[4]);
                    itm.TotalPrice = Convert.ToDecimal(arr[6]);
                    itm.DepreciationPercentage = Convert.ToInt32(arr[8]);
                    itm.AssetAccountId = Convert.ToInt32(arr[9]);
                    itm.DepreciationAccountId = Convert.ToInt32(arr[10]);
                    db.AssetTransferDetails.Add(itm);
                    var StockAccount = db.Accountss.Where(x => x.Name == "Stock").FirstOrDefault();
                    com.addAccountTrasaction(itm.TotalPrice, 0, StockAccount.AccountsID, "AssetPurchase", assetid, DC.Debit, datez, null, null, null, null);
                    com.addAccountTrasaction(0, itm.TotalPrice, (long)itm.AssetAccountId, "AssetPurchase", assetid, DC.Credit, datez, null, null, null, null);
                    db.SaveChanges();

                }

            }


            IFormFileCollection files = Request.Form.Files;
            if (files.Count > 0)
            {
                string path = LegacyWeb.MapPath("~/uploads/Assetpurchase/");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                for (int i = 0; i < files.Count; i++)
                {
                    IFormFile file = files[i];
                    if (file.Length > 0)
                    {

                        var fileCount = db.AssetTransferMasters.Select(a => a.AssetEntryId).AsEnumerable().DefaultIfEmpty(0).Max();

                        var fileName = Path.GetFileName(file.FileName);

                        String extension = Path.GetExtension(fileName);


                        String newName = fileCount + extension;
                        string newFName = fileCount + extension;
                        var FStatus = Status.active;

                        var thumbName = "";
                        var resizeName = "";
                        if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                        {
                            thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                            thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/Assetpurchase/"), thumbName);
                            resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                            resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/Assetpurchase/"), resizeName);
                            newFName = "resize_" + newFName;
                            FStatus = Status.inactive;
                        }
                        else
                        {
                            var commonfilename = "Docs-Thump.png";

                        }
                        string Realname = newName;
                        newName = Path.Combine(LegacyWeb.MapPath("~/uploads/Assetpurchase/"), newName);
                        file.SaveAs(newName);

                        AttachmentDocuments attach = new AttachmentDocuments
                        {
                            TransactionID = assetid,
                            TransactionType = "AssetPurchase",
                            CreatedDate = Convert.ToDateTime(System.DateTime.Now),
                            FileName = Realname,
                            Status = FStatus,

                        };
                        db.AttachmentDocuments.Add(attach);
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
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/Assetpurchase/"), resizeName);
                                thumbs.Save(resizeName);
                            }
                            else
                            {
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/Assetpurchase/"), resizeName);
                                lgimg.Save(resizeName);
                            }

                        }
                    }
                }
            }


            msg = "Asset Purchase Successfull";
            stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        private long GetEntryNo()
        {
            Int64 EntryNo = 0;
            Int64 LastNo = Convert.ToInt64(db.AssetTransferMasters.Where(p => p.VendorName != null).Select(p => p.InvoiceNo).DefaultIfEmpty().Max());
            if (LastNo == 0)
                EntryNo = 1;
            else
                EntryNo = LastNo + 1;
            return EntryNo;
        }
        private bool CodeExist(long Code)
        {
            var Exists = db.AssetTransferMasters.Any(c => c.InvoiceNo == Code);
            bool res = (Exists) ? true : false;
            return res;
        }

        [QkAuthorize]
        public JsonResult AccountSearch(string q, string x, string page)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";

            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Accountss
                                  join b in db.AccountsGroups on a.Group equals b.AccountsGroupID
                                  where (b.Parent == 2 || b.Parent == 4)
                                   && (b.AccountsGroupID != 8 && b.AccountsGroupID != 9 && b.AccountsGroupID != 11 && b.AccountsGroupID != 21 && b.AccountsGroupID != 22 && b.AccountsGroupID != 23)
                                  || (a.Group == 2 || a.Group == 4) &&
                                  (q == null || a.Name.ToLower().Contains(q.ToLower()) || a.Name.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = a.Name,
                                      id = a.AccountsID
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Accountss
                                  join b in db.AccountsGroups on a.Group equals b.AccountsGroupID
                                  where (b.Parent == 2 || b.Parent == 4)
                                  && (b.AccountsGroupID != 8 && b.AccountsGroupID != 9 && b.AccountsGroupID != 11 && b.AccountsGroupID != 21 && b.AccountsGroupID != 22 && b.AccountsGroupID != 23)
                                  || (a.Group == 2 || a.Group == 4)
                                  
                                  select new SelectFormat
                                  {
                                      text = a.Name,
                                      id = a.AccountsID
                                  }).OrderBy(b => b.text).ToList();
            }

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }
        public JsonResult DepreciationSearch(string q, string x, string page)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";

            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Accountss
                                  join b in db.AccountsGroups on a.Group equals b.AccountsGroupID
                                  where ((b.Parent == 29 || b.Parent == 30) || (b.AccountsGroupID == 29 || b.AccountsGroupID == 30)) 
                                  && (a.Name.ToLower().Contains(q.ToLower()) || a.Alias.ToLower().Contains(q.ToLower()) || a.Alias.Contains(q) || a.Name.Contains(q)) && (a.Status == Status.active) //&& a.Group != 23

                                  select new SelectFormat
                                  {
                                      text = a.Name,
                                      id = a.AccountsID
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Accountss
                                  join b in db.AccountsGroups on a.Group equals b.AccountsGroupID
                                  where ((b.Parent == 29 || b.Parent == 30) || (b.AccountsGroupID == 29 || b.AccountsGroupID == 30))

                                  select new SelectFormat
                                  {
                                      text = a.Name,
                                      id = a.AccountsID
                                  }).OrderBy(b => b.text).ToList();
            }

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
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
        public JsonResult VendorSearch(string q, string x, string page)
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
                    List<SelectFormat> serialisedJson2 = db.Suppliers.Select(b => new SelectFormat
                    {
                        text = b.SupplierName, //each json object will have 
                        id = b.SupplierID
                    })
                                      .OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
                    serialisedJson3.AddRange(serialisedJson2);
                }
                serialisedJson = serialisedJson3;
            }
            else
            {
                serialisedJson = db.Suppliers.Select(b => new SelectFormat
                {
                    text = b.SupplierName, //each json object will have 
                    id = b.SupplierID
                }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }//
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public JsonResult McSearch(string q, string x, string page)
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
                    List<SelectFormat> serialisedJson2 = db.MCs.Select(b => new SelectFormat
                    {
                        text = b.MCName, //each json object will have  
                        id = b.MCId
                    })
                                      .OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
                    serialisedJson3.AddRange(serialisedJson2);
                }
                serialisedJson = serialisedJson3;
            }
            else
            {
                serialisedJson = db.Suppliers.Select(b => new SelectFormat
                {
                    text = b.SupplierName, //each json object will have 
                    id = b.SupplierID
                }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }//
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public ActionResult UploadFiles()
        {
            if (Request.Form.Files.Count > 0)
            {
                string PurchOrdId = Request.Form.GetValues("id").First();
                long POId = 0;

                if (PurchOrdId.Contains("undefined"))
                {
                    var LastId = db.AssetTransferMasters.OrderByDescending(a => a.AssetEntryId).FirstOrDefault();
                    POId = LastId.AssetEntryId;
                }
                else
                {
                    POId = Convert.ToInt64(PurchOrdId);
                }

                try
                {
                    IFormFileCollection files = Request.Form.Files;

                    if (files.Count > 0)
                    {
                        string path = LegacyWeb.MapPath("~/uploads/Assetpurchase/");

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
                                    thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/Assetpurchase/"), thumbName);

                                    resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/Assetpurchase/"), resizeName);
                                    newFName = "resize_" + newFName;
                                    FStatus = Status.inactive;
                                }
                                else
                                {
                                    var commonfilename = "Docs-Thump.png";
                                }
                                string Realname = newName;
                                newName = Path.Combine(LegacyWeb.MapPath("~/uploads/Assetpurchase/"), newName);
                                file.SaveAs(newName);

                                AttachmentDocuments attach = new AttachmentDocuments
                                {
                                    TransactionID = POId,
                                    TransactionType = "AssetPurchase",
                                    CreatedDate = Convert.ToDateTime(System.DateTime.Now),
                                    FileName = Realname,
                                    Status = FStatus,

                                };
                                db.AttachmentDocuments.Add(attach);
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
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/Assetpurchase/"), resizeName);
                                        thumbs.Save(resizeName);
                                    }
                                    else
                                    {
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/Assetpurchase/"), resizeName);
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
        //Function to delete all the transactions

        public ActionResult UploadFilesfrommob()
        {
            if (Request.Form.Files.Count > 0)
            {
                

             
                try
                {
                    IFormFileCollection files = Request.Form.Files;

                    if (files.Count > 0)
                    {
                        string path = LegacyWeb.MapPath("~/uploads/Assetpurchase/");

                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        for (int i = 0; i < files.Count; i++)
                        {
                            IFormFile file = files[i];

                            if (file.Length > 0)
                            {

                                string newFName = System.DateTime.Now.ToString() + ".3gpp";


                                file.SaveAs(newFName);

                              

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

        [HttpGet]
        public ActionResult Delete(long? Id)

        {

            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AssetTransferMasters Assets = db.AssetTransferMasters.Where(x => (x.AssetEntryId == Id)).FirstOrDefault();
            if (Assets == null)
            {
                return NotFound();
            }
            return PartialView(Assets);
        }

        [HttpPost]
        public JsonResult Delete(long Id)
        {
             var Msg = chkDeleteWithMsg(Id);
            
            bool stat = false;
            string msg;
            if (Msg != null)
            {
                stat = false;
                msg = "Asset used in Asset To INventory";
                
            }
            else
            {
                AssetTransferMasters docId = db.AssetTransferMasters.Find(Id);
                if (docId != null)
                {
                    //folder delete
                    foreach (var filesattach in db.AttachmentDocuments.Where(d => d.TransactionID == Id).ToList())
                    {

                    if (filesattach != null)
                        {
                            db.AttachmentDocuments.Remove(filesattach);
                            db.SaveChanges();


                            string fullPath = LegacyWeb.MapPath("~/uploads/Assetpurchase/" + filesattach.FileName);
                            if (System.IO.File.Exists(fullPath))
                            {
                                System.IO.File.Delete(fullPath);
                            }

                            string fullPaththumb = LegacyWeb.MapPath("~/uploads/Assetpurchase/" + "thumb_" + filesattach.FileName);
                            if (System.IO.File.Exists(fullPaththumb))
                            {
                                System.IO.File.Delete(fullPaththumb);
                            }
                            string fullPathresize = LegacyWeb.MapPath("~/uploads/Assetpurchase/" + "resize_" + filesattach.FileName);
                            if (System.IO.File.Exists(fullPathresize))
                            {
                                System.IO.File.Delete(fullPathresize);
                            }
                        }
                    }
                    //------------folder delete end-----------
                    db.AttachmentDocuments.RemoveRange(db.AttachmentDocuments.Where(a => a.TransactionID == Id));
                    db.AssetTransferDetails.RemoveRange(db.AssetTransferDetails.Where(a => a.AssetEntryId == Id));
                    db.AssetTransferMasters.Remove(docId);
                    bool delete = com.DeleteAllAccountTransaction("AssetPurchase", Id);
                    db.SaveChanges();
                    var UserId = User.Identity.GetUserId();


                }

                stat = true;
                msg = "Asset Purchase Deleted Successfully.";
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };


        }
        public string chkDeleteWithMsg(long Id)
        {
            string msg = null;
            var exist = (from a in db.AssetTransferMasters
                         from b in db.AssetTransferDetails
                         from c in db.AssetToInventoryDetails
                         where a.AssetEntryId == Id && a.AssetEntryId == b.AssetEntryId &&
                         b.AssetitementryId == c.AssetId
                         select c.AssetName);

            if (exist.FirstOrDefault() != null)
            {
                msg = "Asset Already used in Asset To Inventory ";
            }
            else
            {
                msg = null;
            }

            return msg;
        }
        [HttpGet]
        public ActionResult Edit(long? id)
        {
            AssetPurchaseViewModel vmodel = new AssetPurchaseViewModel();

            vmodel = (from a in db.AssetTransferMasters
                      where a.AssetEntryId == id

                      select new AssetPurchaseViewModel
                      {
                          AssetEntryId = a.AssetEntryId,
                          PurchaseEntry = a.PurchaseEntry,
                          InvoiceNo = a.InvoiceNo,
                          AssetEntryDate = a.AssetEntryDate,
                          VendorName = a.VendorName,
                          TotalAssetValue = a.TotalAssetValue,
                          Vat = a.Vat
                      }).FirstOrDefault();

            ViewBag.image = (from m in db.AttachmentDocuments
                             where id == m.TransactionID
                             select new Assetdocvmodel
                             {
                                 DocumentID = m.DocumentID,
                                 TransactionID = m.TransactionID,
                                 FileName = m.FileName,
                                 CreatedDate = m.CreatedDate,

                             }).ToList();
            var supp = db.Suppliers
            .Select(s => new
            {
                SupplierID = s.SupplierID,
                SupplierDetails = s.SupplierName
            }).ToList();

            ViewBag.Suppl = QkSelect.List(supp, "SupplierID", "SupplierDetails");
           
            return View(vmodel);
        }
        [HttpPost]
        public JsonResult Edit(string[][] array, string[] mtdata, string action)
        {
            bool stat = false;
            string msg;


            if (mtdata != null)
            {
                var TheId = Convert.ToInt64(mtdata[0]);
                AssetTransferMasters STs = db.AssetTransferMasters.Find(TheId);
                STs.PurchaseEntry = Convert.ToString(mtdata[1]);
                STs.InvoiceNo = Convert.ToInt32(mtdata[2]);
                STs.VendorName = Convert.ToInt64(mtdata[3]);
                STs.AssetEntryDate = DateTime.Parse(mtdata[4], new CultureInfo("en-GB"));
                STs.Vat = Convert.ToInt64(mtdata[5]);
                STs.TotalAssetValue = Convert.ToDecimal(mtdata[6]);
                db.Entry(STs).State = EntityState.Modified;
                db.SaveChanges();

                Int64 MtId = STs.AssetEntryId;
                db.AssetTransferDetails.RemoveRange(db.AssetTransferDetails.Where(a => a.AssetEntryId == MtId));
                db.SaveChanges();

                if (array != null)
                {


                    AssetTransferDetail ST = new AssetTransferDetail();
                    if (ST != null)
                    {

                        bool delete = com.DeleteAllAccountTransaction("AssetPurchase", MtId);
                        foreach (var arr in array)
                        {
                            ST.AssetEntryId = MtId;
                            ST.AssetName = Convert.ToString(arr[0]);
                            ST.Barcode = Convert.ToString(arr[1]);
                            ST.UnitId = Convert.ToInt32(arr[2]);
                            ST.Quantity = Convert.ToDecimal(arr[3]);
                            ST.Price = Convert.ToDecimal(arr[4]);
                            ST.TotalPrice = Convert.ToDecimal(arr[6]);
                            ST.DepreciationPercentage = Convert.ToInt32(arr[8]);
                            ST.AssetAccountId = Convert.ToInt32(arr[9]);
                            ST.DepreciationAccountId = Convert.ToInt32(arr[10]);
                            db.AssetTransferDetails.Add(ST);
                            var Datz = DateTime.Parse(mtdata[4], new CultureInfo("en-GB"));
                            var StockAccount = db.Accountss.Where(x => x.Name == "Stock").FirstOrDefault();
                            com.addAccountTrasaction(ST.TotalPrice, 0, StockAccount.AccountsID, "AssetPurchase", MtId, DC.Debit, Datz, null, null, null, null);
                            com.addAccountTrasaction(0, ST.TotalPrice, (long)ST.AssetAccountId, "AssetPurchase", MtId, DC.Credit, Datz, null, null, null, null);
                            db.SaveChanges();


                        }

                        
                        

                    }


                }

                IFormFileCollection files = Request.Form.Files;
                if (files.Count > 0)
                {
                    string path = LegacyWeb.MapPath("~/uploads/Assetpurchase/");
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    for (int i = 0; i < files.Count; i++)
                    {
                        IFormFile file = files[i];
                        if (file.Length > 0)
                        {

                            var fileCount = db.AssetTransferMasters.Select(a => a.AssetEntryId).AsEnumerable().DefaultIfEmpty(0).Max();

                            var fileName = Path.GetFileName(file.FileName);

                            String extension = Path.GetExtension(fileName);


                            String newName = fileCount + extension;
                            string newFName = fileCount + extension;
                            
                            var FStatus = Status.active;

                            var thumbName = "";
                            var resizeName = "";
                            if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                            {
                                thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/Assetpurchase/"), thumbName);

                                resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/Assetpurchase/"), resizeName);
                                newFName = "resize_" + newFName;
                                FStatus = Status.inactive;
                            }
                            else
                            {
                                var commonfilename = "Docs-Thump.png";

                            }
                            newName = Path.Combine(LegacyWeb.MapPath("~/uploads/Assetpurchase/"), newName);
                            file.SaveAs(newName);

                            AttachmentDocuments attach = new AttachmentDocuments
                            {
                                TransactionID = MtId,
                                TransactionType = "AssetPurchase",
                                CreatedDate = Convert.ToDateTime(System.DateTime.Now),
                                FileName = newFName,
                                Status = FStatus,

                            };
                            db.AttachmentDocuments.Add(attach);
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
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/Assetpurchase/"), resizeName);
                                    thumbs.Save(resizeName);
                                }
                                else
                                {
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/Assetpurchase/"), resizeName);
                                    lgimg.Save(resizeName);
                                }

                            }
                        }
                    }
                }












                msg = "Successfully Updated AssetPurchase";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                msg = "All Fields Required";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }
        [HttpGet]
        public JsonResult GetSTItems(long EntryID)
        {
            var ConD = (from a in db.AssetTransferDetails
                        join c in db.Accountss on a.AssetAccountId equals c.AccountsID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.Accountss on a.DepreciationAccountId equals d.AccountsID into sec
                        from d in sec.DefaultIfEmpty()
                        join e in db.ItemUnits on a.UnitId equals e.ItemUnitID into Uni
                        from e in Uni.DefaultIfEmpty()
                        where a.AssetEntryId == EntryID
                        select new
                        {

                            a.AssetName,
                            a.Barcode,
                            a.UnitId,
                            a.Quantity,
                            a.Price,
                            a.TotalPrice,
                            a.DepreciationPercentage,
                            a.AssetAccountId,
                            a.DepreciationAccountId,
                            AccName = c.Name,
                            DccName = d.Name,
                            Uni = e.ItemUnitName

                        }).AsEnumerable().Select(o => new
                        {
                            o.AssetName,
                            o.Barcode,
                            o.UnitId,
                            o.Quantity,
                            o.Price,
                            o.TotalPrice,
                            o.DepreciationPercentage,
                            o.AssetAccountId,
                            o.DepreciationAccountId,
                            AccName = (o.AccName != null) ? o.AccName : "",
                            DccName = (o.DccName != null) ? o.DccName : "",
                            Uni = (o.Uni != null) ? o.Uni : "",
                        }).ToList();
            return Json(ConD);
        }
        public JsonResult ImageDelete(long key)
        {
            bool stat = false;
            string msg;
            AttachmentDocuments FilemultipleDocument = db.AttachmentDocuments.Find(key);
            if (FilemultipleDocument != null)
            {
                db.AttachmentDocuments.Remove(FilemultipleDocument);
                db.SaveChanges();
            }
            string fullPath = LegacyWeb.MapPath("~/uploads/Assetpurchase/" + FilemultipleDocument.FileName);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            string fullPaththumb = LegacyWeb.MapPath("~/uploads/Assetpurchase/" + "thumb_" + FilemultipleDocument.FileName);
            if (System.IO.File.Exists(fullPaththumb))
            {
                System.IO.File.Delete(fullPaththumb);
            }
            string fullPathresize = LegacyWeb.MapPath("~/uploads/Assetpurchase/" + "resize_" + FilemultipleDocument.FileName);
            if (System.IO.File.Exists(fullPathresize))
            {
                System.IO.File.Delete(fullPathresize);
            }

            var UserId = User.Identity.GetUserId();




            Int64 Id = key;
            stat = true;
            msg = "Successfully deleted  Image.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }
        [HttpPost]
        public ActionResult DeleteAll(long[] bill)
        {  
            Int32 count = 0;

            foreach (var arr in bill)
            {
                var chk = (DeleteEntry(arr) == true) ? count++ : count;
            }
            if (count != 0)
            {
                Success("Deleted " + count + " Asset Purchase Entry.", true);
                return RedirectToAction("Index", "Assetpurchase");
            }
            else
            {
                Danger("Asset used in Asset To INventory ", true);
                return RedirectToAction("Index", "Assetpurchase");
            }
        }

        //Function To Delete Each Entry
        private Boolean DeleteEntry(long AssetEntryId)
        {
            var Msg = chkDeleteWithMsg(AssetEntryId);

            if (Msg != null)
            {
                return false;

            }
            else
            {


                int i = 0;
                //folder delete
                foreach (var filesattach in db.AttachmentDocuments.Where(d => d.TransactionID == AssetEntryId).ToList())
                {
                    if (filesattach != null)
                    {
                        if (filesattach.FileName != null)
                        {
                            db.AttachmentDocuments.Remove(filesattach);
                            db.SaveChanges();


                            string fullPath = LegacyWeb.MapPath("~/uploads/Assetpurchase/" + filesattach.FileName);
                            if (System.IO.File.Exists(fullPath))
                            {
                                System.IO.File.Delete(fullPath);
                            }

                            string fullPaththumb = LegacyWeb.MapPath("~/uploads/Assetpurchase/" + "thumb_" + filesattach.FileName);
                            if (System.IO.File.Exists(fullPaththumb))
                            {
                                System.IO.File.Delete(fullPaththumb);
                            }
                            string fullPathresize = LegacyWeb.MapPath("~/uploads/Assetpurchase/" + "resize_" + filesattach.FileName);
                            if (System.IO.File.Exists(fullPathresize))
                            {
                                System.IO.File.Delete(fullPathresize);
                            }
                        }
                    }
                }
                //------------folder delete end-----------

                List<AssetTransferMasters> AssetLists = new List<AssetTransferMasters>();


                db.AttachmentDocuments.RemoveRange(db.AttachmentDocuments.Where(a => a.TransactionID == AssetEntryId));
                db.AssetTransferDetails.RemoveRange(db.AssetTransferDetails.Where(a => a.AssetEntryId == AssetEntryId));
                db.AssetTransferMasters.RemoveRange(db.AssetTransferMasters.Where(a => a.AssetEntryId == AssetEntryId));
                //***********Delete from table AccountTransaction
                bool delete = com.DeleteAllAccountTransaction("AssetPurchase", AssetEntryId);
                db.SaveChanges();



                return true;
            }
        }

    }


}
