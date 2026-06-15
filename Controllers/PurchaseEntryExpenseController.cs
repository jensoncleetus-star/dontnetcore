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
using System.Text;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using System.Drawing;
using System.Data.OleDb;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class PurchaseEntryExpenseController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PurchaseEntryExpenseController()
        {
            db = new ApplicationDbContext();
            com = new Common();

        }
        // GET: PurchaseEntry 
        [QkAuthorize(Roles = "Dev,Purchase List")]
        public ActionResult Index()
        {
            var SuperUserEdit = db.EnableSettings.Where(a => a.EnableType == "SuperUserEdit").FirstOrDefault();
            var SuperUserEditvalue = SuperUserEdit != null ? SuperUserEdit.Status : Status.inactive;
            ViewBag.SuperUserEditvalue = SuperUserEditvalue;
            var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
            var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;
            ViewBag.StockTrnsfrUpdate = brcheck;
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

            ViewBag.PurchaseStatus = QkSelect.List(new List<SelectListItem>{

                new SelectListItem() {Text = "Open", Value="1"},
                new SelectListItem() {Text = "Closed", Value="2"},
            }, "Value", "Text");

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

            var MlaPEntry = db.EnableSettings.Where(a => a.EnableType == "MLAPEntry").FirstOrDefault();
            var MlaPEntrys = MlaPEntry != null ? MlaPEntry.Status : Status.inactive;
            ViewBag.MLAPEntry = MlaPEntrys;

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindPurchase").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Purchase", Value="1"},
                new SelectListItem() {Text = "CrossHire", Value="2"},
            }, "Value", "Text");

            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;
            return View();
        }
        [QkAuthorize(Roles = "Dev,Purchase List")]
        public ActionResult Approvals()
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

            ViewBag.PurchaseStatus = QkSelect.List(new List<SelectListItem>{

                new SelectListItem() {Text = "Open", Value="1"},
                new SelectListItem() {Text = "Closed", Value="2"},
            }, "Value", "Text");

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

            var MlaPEntry = db.EnableSettings.Where(a => a.EnableType == "MLAPEntry").FirstOrDefault();
            var MlaPEntrys = MlaPEntry != null ? MlaPEntry.Status : Status.inactive;
            ViewBag.MLAPEntry = MlaPEntrys;

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindPurchase").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Purchase", Value="1"},
                new SelectListItem() {Text = "CrossHire", Value="2"},
            }, "Value", "Text");

            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;
            return View();
        }

        [QkAuthorize(Roles = "Dev,Purchase List")]
        public ActionResult PaymentApprovals()
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

            ViewBag.PurchaseStatus = QkSelect.List(new List<SelectListItem>{

                new SelectListItem() {Text = "Open", Value="1"},
                new SelectListItem() {Text = "Closed", Value="2"},
            }, "Value", "Text");

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

            var MlaPEntry = db.EnableSettings.Where(a => a.EnableType == "MLAPEntry").FirstOrDefault();
            var MlaPEntrys = MlaPEntry != null ? MlaPEntry.Status : Status.inactive;
            ViewBag.MLAPEntry = MlaPEntrys;

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindPurchase").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Purchase", Value="1"},
                new SelectListItem() {Text = "CrossHire", Value="2"},
            }, "Value", "Text");

            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;
            return View();
        }
        public JsonResult ImageDelete(long key)
        {

            bool stat = false;
            string msg;
            purchaseentrydocument tskImg = db.purchaseentrydocuments.Find(key);
            if (tskImg != null)
            {
                db.purchaseentrydocuments.Remove(tskImg);
                db.SaveChanges();
            }
            string fullPath = LegacyWeb.MapPath("~/uploads/purchaseentrydocument/" + tskImg.FileName);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            string fullPaththumb = LegacyWeb.MapPath("~/uploads/purchaseentrydocument/" + "thumb_" + tskImg.FileName);
            if (System.IO.File.Exists(fullPaththumb))
            {
                System.IO.File.Delete(fullPaththumb);
            }
            string fullPathresize = LegacyWeb.MapPath("~/uploads/purchaseentrydocument/" + "resize_" + tskImg.FileName);
            if (System.IO.File.Exists(fullPathresize))
            {
                System.IO.File.Delete(fullPathresize);
            }

            var UserId = User.Identity.GetUserId();

            com.addlog(LogTypes.Deleted, UserId, "Purchase", "purchaseentrydocuments", findip(), tskImg.PurchaseId, "Image Deleted Successfully");


            Int64 Id = key;
            stat = true;
            msg = "Successfully deleted Image.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }

        public ActionResult DownloadFile(long Id)
        {

            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            purchaseentrydocument docdownload = db.purchaseentrydocuments.Where(o=>o.PurchaseId==Id).FirstOrDefault();
            if (docdownload == null)
            {
                return NotFound();
            }
            else
            {
                FileDocumentViewModel filedoc = new FileDocumentViewModel();
                filedoc.lstMultidocview = (from m in db.purchaseentrydocuments
                                           where Id == m.PurchaseId
                                           select new Multiviewmodel
                                           {

                                               Id = m.purid,
                                               Document = m.FileName,
                                               filenamelead = m.FileName,
                                               DocumentName = m.FileName


                                           }
                                        ).ToList();
                ViewBag.document = docdownload.FileName;

                return PartialView(filedoc);
            }










            purchaseentrydocument docId = db.purchaseentrydocuments.Where(o=>o.PurchaseId==Id).FirstOrDefault();
            var fileName = docId.FileName;
            string filePath = LegacyWeb.MapPath("~/uploads/purchaseentrydocument/" + fileName);
            return File(filePath, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
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
                        string path = LegacyWeb.MapPath("~/uploads/purchaseentrydocument/");
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        for (int i = 0; i < files.Count; i++)
                        {
                            IFormFile file = files[i];
                            if (file.Length > 0)
                            {


                                var fileCount = db.purchaseentrydocuments.Select(a => a.purid).AsEnumerable().DefaultIfEmpty(20000).Max();

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
                                    thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/purchaseentrydocument/"), thumbName);

                                    resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/purchaseentrydocument/"), resizeName);
                                    newFName = "resize_" + newFName;
                                    FStatus = Status.inactive;
                                }
                                else
                                {
                                    var commonfilename = "Docs-Thump.png";

                                }
                                string Realname = newName;

                                newName = Path.Combine(LegacyWeb.MapPath("~/uploads/purchaseentrydocument/"), newName);
                                if(System.IO.File.Exists(newName))
{
                                    //delete existing file
                                    System.IO.File.Delete(newName);
                                }
                                file.SaveAs(newName);
                               

                                var qtndoc = new purchaseentrydocument
                                {
                                    PurchaseId = id,
                                    FileName = Realname,//Path.GetFileName(file.FileName),
                                    Status = FStatus,
                                    CreatedDate = Convert.ToDateTime(System.DateTime.Now)
                                };
                                db.purchaseentrydocuments.Add(qtndoc);
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
                                    if (System.IO.File.Exists(thumbName))
                                    {
                                        //delete existing file
                                        System.IO.File.Delete(thumbName);
                                    }
                                    thumb.Save(thumbName);

                                    Image lgimg = Image.FromFile(newName);
                                    if (lgimg.Width > 1800 || lgimg.Height > 1800)
                                    {
                                        Image imgs = Image.FromFile(newName);
                                        System.Drawing.Image thumbs = Common.resizeImage(imgs, new Size(1800, 1800));
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/purchaseentrydocument/"), resizeName);
                                        thumbs.Save(resizeName);
                                    }
                                    else
                                    {
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/purchaseentrydocument/"), resizeName);
                                        if (System.IO.File.Exists(resizeName))
                                        {
                                            //delete existing file
                                            System.IO.File.Delete(resizeName);
                                        }
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
        [HttpGet]
        public ActionResult GetPricePurchase(long itemid,decimal purchaseprice)
        {

            var item = (from a in db.Items
                        where a.ItemID == itemid
                        select new
                        {
                          a.PricingStrategy,
                          a.PricingStrategyAmountType,
                          a.PricingStrategyType,
                          a.PricingStrategyValue,
                          a.PurchasePrice,
                          a.SellingPrice
                          
                        }
                      ).FirstOrDefault();

            decimal purprice = 0;
            decimal salprice = 0;
            if (item.PricingStrategyType == pricingstatagytype.FIFO)
            {



                purprice = item.PurchasePrice;
                salprice = item.SellingPrice;

            }


            else if (item.PricingStrategyType == pricingstatagytype.AVG)
            {
                purprice = (item.PurchasePrice + purchaseprice) / 2;

                if (item.PricingStrategyAmountType == AmountType.AbsoluteAmount)
                    salprice = purprice + (decimal)item.PricingStrategyValue;
                else
                {
                    salprice = purprice + purprice * (decimal)item.PricingStrategyValue / 100;
                }
            }
            else if(item.PricingStrategyType == pricingstatagytype.ABS)
            {
                purprice = purchaseprice;
                salprice = (decimal)item.SellingPrice;



            }
            else
            {
                purprice = purchaseprice;


                if (item.PricingStrategyAmountType == AmountType.AbsoluteAmount)
                    salprice = purprice + (decimal)item.PricingStrategyValue;
                else
                {
                    salprice = purprice + purprice * (decimal)item.PricingStrategyValue / 100;
                }


            }
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            decimal[] pr = { purprice, salprice };
            string result = javaScriptSerializer.Serialize(pr);
            return Json(new { salprice = Math.Round(salprice, 2), purprice = Math.Round(purprice, 2) });
        }


        [HttpGet]
        public ActionResult GetPrice(long itemid, pricingstatagytype pcategory, AmountType amounttype,decimal amount)
        {
            decimal price = 0;
            decimal purchaseprice = 0;
            var item = (
                          from a in db.PurchaseEntrys
                          join b in db.PEItemss on a.PurchaseEntryId equals b.PurchaseEntry
                          join c in db.Items on new { f1 = b.Item, f2 = b.ItemUnit } equals new { f1 = c.ItemID, f2 = c.ItemUnitID }

                          where b.Item == itemid


                          select new
                          {
                              b.Item,
                              b.ItemUnitPrice,
                              b.ItemUnit,
                              c.ConFactor,
                              a.PEDate,
                              a.PurchaseEntryId 

                          });
            if (!item.Any())
            {
                price = db.Items.Where(o => o.ItemID == itemid).FirstOrDefault().SellingPrice;
                purchaseprice = db.Items.Where(o => o.ItemID == itemid).FirstOrDefault().PurchasePrice;


            }
            else
            {
                var pricee = item.OrderBy(o => o.PEDate).FirstOrDefault().ItemUnitPrice;
                if (item == null)
                {
                    price = db.Items.Where(o => o.ItemID == itemid).FirstOrDefault().SellingPrice;
                    purchaseprice = db.Items.Where(o => o.ItemID == itemid).FirstOrDefault().PurchasePrice;


                }
                else
                {
                    price = item.OrderBy(o => o.PEDate).ThenBy(o=>o.PurchaseEntryId).FirstOrDefault().ItemUnitPrice;


                }
                if (pcategory == pricingstatagytype.FIFO)
                {



                    purchaseprice = price;
                    if (amounttype == AmountType.AbsoluteAmount)
                        price = price + amount;
                    else
                        price = price + price * amount / 100;

                }


                else if (pcategory == pricingstatagytype.AVG)
                {
                    price = item.ToList().Average(o => o.ItemUnitPrice);
                    purchaseprice = price;
                    if (amounttype == AmountType.AbsoluteAmount)
                        price = price + amount;
                    else
                    {
                        price = price + price * amount / 100;
                    }
                }
                else
                {
                    price = item.OrderByDescending(o => o.PEDate).ThenByDescending(o => o.PurchaseEntryId).FirstOrDefault().ItemUnitPrice;
                    purchaseprice = price;
                    if (amounttype == AmountType.AbsoluteAmount)
                        price = price + amount;
                    else
                    {
                        price = price + price * amount / 100;
                    }

                }
            }
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            decimal[] pr = {  price, purchaseprice };
            string result = javaScriptSerializer.Serialize(pr);
            return Json(new { salprice =Math.Round(price,2),purprice=Math.Round(purchaseprice,2) });
        }
        [HttpGet]
        public ActionResult GetSalesEntrys(string billno)
        {

            ApplicationDbContext db = new ApplicationDbContext("quicknet");
            var salesid = db.SalesEntrys.Where(o => o.BillNo == billno).Select(o => o.SalesEntryId).FirstOrDefault();

            var sitems = (from a in db.SEItemss
                          join b in db.SalesEntrys on a.SalesEntry equals b.SalesEntryId
                          join c in db.Items on a.Item equals c.ItemID
                          join d in db.ItemUnits on c.ItemUnitID equals d.ItemUnitID into primary
                          from d in primary.DefaultIfEmpty()
                          join e in db.ItemUnits on c.SubUnitId equals e.ItemUnitID into second
                          from e in second.DefaultIfEmpty()
                          join f in db.ItemCategorys on c.ItemCategoryID equals f.ItemCategoryID into cat
                          from f in cat.DefaultIfEmpty()
                          join g in db.ItemBrands on c.ItemBrandID equals g.ItemBrandID into bra
                          from g in bra.DefaultIfEmpty()
                          where b.SalesEntryId == salesid
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
                              prunitname=d.ItemUnitName,
                              secunitname=e.ItemUnitName,
                              catname=f.ItemCategoryName,
                              braname=g.ItemBrandName,
                              note = a.itemNote.Replace("<br />", "\n"),
                              ItemNote = a.itemNote != null ? a.itemNote : "",
                              a.ItemTotalAmount,
                              ItemCode = c.ItemCode,
                              ItemName = c.ItemName,
                              ItemWithCode = c.ItemCode + " - " + c.ItemName,
                              c.ItemUnitID,
                              c.SubUnitId,
                              PriUnit = d.ItemUnitName,
                              SubUnit = e.ItemUnitName,
                              c.BasePrice,
                              c.SellingPrice,
                              c.PurchasePrice,
                              c.MRP,
                              c.ConFactor,
                              c.ItemBrandID,
                              c.ItemCategoryID,
                              c.ItemSizeID,
                              c.ItemColorID,
                              c.KeepStock,
                              c.PricingStrategy,

                          }).ToList();

            db = new ApplicationDbContext();
            
            foreach(var k in sitems)
            {
                var v = db.Items.Any(o => o.ItemCode == k.ItemCode);
                if (!v)
                {
                    Item a = new Item();
                   
                        a.ItemCode = k.ItemCode;
                    a.ItemName = k.ItemName;
                    a.BasePrice = k.BasePrice;
                    a.MRP = k.MRP;
                    a.SellingPrice = k.SellingPrice;
                    a.PurchasePrice = k.PurchasePrice;
                    a.TaxID = 2;
                    var pnameexist = db.ItemUnits.Where(o => o.ItemUnitName == k.prunitname).Select(o=>o.ItemUnitID).FirstOrDefault();
                    
                    if(pnameexist!=0)
                    {
                        a.ItemUnitID = pnameexist;
                        
                    }
                    else
                    {
                        ItemUnit cr = new ItemUnit()
                        {
                            Editable = choice.Yes,
                            ItemUnitName = k.prunitname,

                        };
                        if (k.prunitname != null)
                        {
                            db.ItemUnits.Add(cr);
                            db.SaveChanges();

                            a.ItemUnitID = cr.ItemUnitID;
                        }
                    }

                    var secunit = db.ItemUnits.Where(o => o.ItemUnitName == k.secunitname).Select(o => o.ItemUnitID).FirstOrDefault();

                    if (secunit != 0)
                    {
                        a.SubUnitId = secunit;

                    }
                    else
                    {
                        ItemUnit cr = new ItemUnit()
                        {
                            Editable = choice.Yes,
                            ItemUnitName = k.secunitname,

                        };
                        if (k.secunitname != null)
                        {
                            db.ItemUnits.Add(cr);
                            db.SaveChanges();

                            a.SubUnitId = cr.ItemUnitID;
                        }
                    }

                  
                    a.ConFactor = k.ConFactor;

                    a.SellingPrice = k.SellingPrice;
                    var catid = db.ItemCategorys.Where(o => o.ItemCategoryName == k.catname).Select(o => o.ItemCategoryID).FirstOrDefault();

                    if (catid != 0)
                    {
                        a.ItemCategoryID = catid;

                    }
                    else
                    {
                        ItemCategory cr = new ItemCategory()
                        {
                            Editable=choice.Yes,
                             ItemCategoryName=k.catname,
                             

                        };
                        if (k.catname != null)
                        {
                            db.ItemCategorys.Add(cr);
                            db.SaveChanges();

                            a.ItemCategoryID = cr.ItemCategoryID;
                        }
                    }



                    var branid = db.ItemBrands.Where(o => o.ItemBrandName == k.braname).Select(o => o.ItemBrandID).FirstOrDefault();

                    if (branid != 0)
                    {
                        a.ItemBrandID = branid;

                    }
                    else
                    {
                        ItemBrand cr = new ItemBrand()
                        {
                            Editable=choice.Yes,
                             ItemBrandName=k.braname,


                        };
                        if (k.braname != null)
                        {
                            db.ItemBrands.Add(cr);
                            db.SaveChanges();

                            a.ItemBrandID = cr.ItemBrandID;
                        }
                    }


                    a.ItemColorID =1;
                    a.KeepStock = k.KeepStock;
                    a.ItemSizeID = 2;
                    a.PricingStrategy = k.PricingStrategy;
                    
                    db.Items.Add(a);
                    db.SaveChanges();
                }
            
            else
                {
                    var a = db.Items.Where(o => o.ItemCode == k.ItemCode).FirstOrDefault();
                    var pnameexist = db.ItemUnits.Where(o => o.ItemUnitName == k.prunitname).Select(o => o.ItemUnitID).FirstOrDefault();
                    a.PurchasePrice = k.PurchasePrice;
                    if (pnameexist != 0)
                    {
                        a.ItemUnitID = pnameexist;
                        db.Entry(a).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        ItemUnit cr = new ItemUnit()
                        {
                            Editable = choice.Yes,
                            ItemUnitName = k.prunitname,

                        };
                        if (k.prunitname != null)
                        {
                            db.ItemUnits.Add(cr);
                            db.SaveChanges();

                            a.ItemUnitID = cr.ItemUnitID;
                        }
                    }

                    var secunit = db.ItemUnits.Where(o => o.ItemUnitName == k.secunitname).Select(o => o.ItemUnitID).FirstOrDefault();

                    if (secunit != 0)
                    {
                        a.SubUnitId = secunit;
                        db.Entry(a).State = EntityState.Modified;
                        db.SaveChanges();

                    }
                    else
                    {
                        ItemUnit cr = new ItemUnit()
                        {
                            Editable = choice.Yes,
                            ItemUnitName = k.secunitname,

                        };
                        if (k.secunitname != null)
                        {
                            db.ItemUnits.Add(cr);
                            db.SaveChanges();


                            a.SubUnitId = cr.ItemUnitID;
                        }
                    }


                    a.ConFactor = k.ConFactor;

                    a.SellingPrice = k.SellingPrice;
                    var catid = db.ItemCategorys.Where(o => o.ItemCategoryName == k.catname).Select(o => o.ItemCategoryID).FirstOrDefault();

                    if (catid != 0)
                    {
                        a.ItemCategoryID = catid;
                        db.Entry(a).State = EntityState.Modified;
                        db.SaveChanges();

                    }
                    else
                    {
                        ItemCategory cr = new ItemCategory()
                        {
                            Editable = choice.Yes,
                            ItemCategoryName = k.catname,


                        };
                        if (k.catname != null)
                        {
                            db.ItemCategorys.Add(cr);
                            db.SaveChanges();

                            a.ItemCategoryID = cr.ItemCategoryID;
                        }
                    }



                    var branid = db.ItemBrands.Where(o => o.ItemBrandName == k.braname).Select(o => o.ItemBrandID).FirstOrDefault();

                    if (branid != 0)
                    {
                        a.ItemBrandID = branid;
                        db.Entry(a).State = EntityState.Modified;
                        db.SaveChanges();

                    }
                    else
                    {
                        ItemBrand cr = new ItemBrand()
                        {
                            Editable = choice.Yes,
                            ItemBrandName = k.braname,


                        };
                        if (k.braname != null)
                        {
                            db.ItemBrands.Add(cr);
                            db.SaveChanges();

                            a.ItemBrandID = cr.ItemBrandID;
                        }
                    }
                                }
            }
          var ConD = (from a in sitems
                      join b in db.Items on a.ItemCode equals b.ItemCode
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                       
                        
                       
                        select new
                        {
                           Item=b.ItemID,
                            a.ItemQuantity,
                            a.ItemUnit,
                            a.ItemUnitPrice,
                            a.ItemTax,
                            a.ItemSubTotal,
                            a.ItemTaxAmount,
                            a.ItemDiscount,
                            a.note,
                            a.ItemNote,
                            a.ItemTotalAmount,
                            a.ItemCode,
                            a.ItemName,
                           a.ItemWithCode,
                            b.ItemUnitID,
                            b.SubUnitId,
                            PriUnit = (c==null)? "Pcs." : c.ItemUnitName,
                            SubUnit = (d== null) ? ((c==null)? "Pcs." : c.ItemUnitName) : d.ItemUnitName,
                            b.BasePrice,
                            b.SellingPrice,
                            b.PurchasePrice,
                            b.MRP,
                           
                            
                        });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(ConD);
            return Json(result);
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Purchase Entry")]
        public ActionResult editpull(string billno,string purid)
        {
            long id = Convert.ToInt64(purid);
           ApplicationDbContext db = new ApplicationDbContext();

           
            var dummyvalue = db.DummyPEItems2.Where(a => a.PurchaseEntry == id).FirstOrDefault();
            if (dummyvalue != null)
            {
                ViewBag.itemAproval = 1;
            }
            else
            {
                ViewBag.itemAproval = 0;
            }
            var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
            var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;
            ViewBag.StockTrnsfrUpdate = brcheck;
            ViewBag.image = (from b in db.purchaseentrydocuments
                             join c in db.PurchaseEntrys on b.PurchaseId equals c.PurchaseEntryId
                             where c.PurchaseEntryId == id
                             select new quotationdocumentviewmodel
                             {
                                 qutid = b.purid,
                                 quotationID = b.PurchaseId,
                                 FileName = b.FileName,
                             }).ToList();
            var userpermission = User.IsInRole("All Purchase Entry");
            var UserId = User.Identity.GetUserId();
            PurchaseEntry PEentry = db.PurchaseEntrys.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.PurchaseEntryId == id).FirstOrDefault();


            if (PEentry == null)
            {
                return NotFound();
            }
            Int64 cashier = Convert.ToInt64(PEentry.PECashier);
            Int64 customer = PEentry.Supplier;
            var use = db.Employees
                          .Select(s => new
                          {
                              ID = s.EmployeeId,
                              Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                          })
                          .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            var supp = db.Suppliers
                       .Select(s => new
                       {
                           SupplierID = s.SupplierID,
                           SupplierDetails = s.SupplierID + " - " + s.SupplierName
                       }).ToList();
            ViewBag.Supp = QkSelect.List(supp, "SupplierID", "SupplierDetails");

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            if (mcchk != null)
            {
                //    Id = s.MCId,
                //    Name = s.MCName

                var mcs = db.MCs.Where(s => s.MCId == PEentry.MaterialCenter).Select(s => new
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

            var EnableCurrency = db.EnableSettings.Where(a => a.EnableType == "EnableCurrency").FirstOrDefault();
            var CurrencyEnable = EnableCurrency != null ? EnableCurrency.Status : Status.inactive;
            ViewBag.EnableCurrency = CurrencyEnable;

            var Currency = db.CurrencyMasters
              .Select(s => new
              {
                  Id = s.Id,
                  Name = s.CurrencyCode
              }).ToList();
            ViewBag.CurrencyVal = QkSelect.List(Currency, "Id", "Name");


            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "PurchaseEntry").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaPEntry = db.EnableSettings.Where(a => a.EnableType == "MLAPEntry").FirstOrDefault();
            var MlaPEntrys = MlaPEntry != null ? MlaPEntry.Status : Status.inactive;
            ViewBag.MLAPEntry = MlaPEntrys;


            if (PEentry.PurchaseStatus == 0)
            {
                List<SelectListItem> pstat = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Open", Value = "0"
                },
                new SelectListItem {
                    Text = "Closed", Value = "1"
                }
              };
                ViewBag.PurchaseStatus = pstat;

            }
            else if (PEentry.PurchaseStatus == 1)
            {
                List<SelectListItem> pstat = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Closed", Value = "1"
                },
                new SelectListItem {
                    Text = "Open", Value = "0"
                }
              };
                ViewBag.PurchaseStatus = pstat;
            }
            else
            {
                List<SelectListItem> pstat = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Open", Value = "0"
                },
                new SelectListItem {
                    Text = "Closed", Value = "1"
                },
              };
                ViewBag.PurchaseStatus = pstat;
            }




            var CBill = "";
            var CType = "";
            var ConvertTran = db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "Purchase").FirstOrDefault();
            if (ConvertTran != null)
            {
                CType = ConvertTran.ConvertFrom + " No";
                if (ConvertTran.ConvertFrom == "POrder")
                {
                    CBill = db.PurchaseOrders.Where(a => a.PurchaseOrderId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "PQuote")
                {
                    CBill = db.PurchaseQuotations.Where(a => a.PQuotationId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "MOrder")
                {
                    CBill = db.MaterialRequisitions.Where(a => a.MaterialRequisitionId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "MRNote")
                {
                    CBill = db.MaterialReceiveNotes.Where(a => a.MRId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
            }

            PurchaseEntryViewModel vmodel = new PurchaseEntryViewModel();
            vmodel = (from b in db.PurchaseEntrys
                      join c in db.PEPayments on b.PurchaseEntryId equals c.PurchaseEntry
                      join d in db.Suppliers on b.Supplier equals d.SupplierID
                      join e in db.Contacts on d.Contact equals e.ContactID into cont
                      from e in cont.DefaultIfEmpty()
                      join f in db.HireDetails on new { f1 = b.PurchaseEntryId, f2 = "Purchase" }
                      equals new { f1 = f.Reference, f2 = f.Section } into hir
                      from f in hir.DefaultIfEmpty()
                      where b.PurchaseEntryId == id
                      select new PurchaseEntryViewModel
                      {
                          SupplierName = db.Suppliers.Where(a => a.SupplierID == b.Supplier).Select(a => a.SupplierCode + " - " + a.SupplierName).FirstOrDefault(),

                          PENo = b.PENo,
                          PEDate = b.PEDate,
                          BillNo = b.BillNo,
                          PECashier = b.PECashier,
                          SupplierType = b.SupplierType,
                          Supplier = b.Supplier,
                          PEDiscount = b.PEDiscount,
                          PEGrandTotal = b.PEGrandTotal,
                          PENote = b.PENote,
                          suppEmailId = e.EmailId,
                          PurchaseType = b.PurchaseType,
                          Remarks = b.Remarks,
                          PurchaseTypes = db.PurchaseTypes.ToList(),
                          MaterialCenter = b.MaterialCenter,
                          Branch = b.Branch,
                          Currency = b.Currency,
                          DConvertionRate = b.ConvertionRate,
                          convertBill = CBill,
                          convertFrom = CType,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          PurchaseHireType = b.PurType,
                          FromDate = f.StartDate,
                          ToDate = f.EndDate,
                          CrossHireType = f.HireType,
                          ReferenceNo = b.ReferenceNo,
                          requestpayment = b.requestpayment
                          // PEPaidAmount = c.PEPaidAmount,
                          // PEDueAmount = b.PEGrandTotal - c.PEPaidAmount
                      }).FirstOrDefault();

            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            ViewBag.preEntry = db.PurchaseEntrys.Where(a => a.PurchaseEntryId < id && (!MCList.Any() || MCArray.Contains(a.MaterialCenter)) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.PurchaseEntryId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.PurchaseEntrys.Where(a => a.PurchaseEntryId > id && (!MCList.Any() || MCArray.Contains(a.MaterialCenter)) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.PurchaseEntryId).DefaultIfEmpty().Min();

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            ViewBag.CheckMail = mail != null ? mail.Status : Status.inactive;

            var ItemPriceInPurchase = db.EnableSettings.Where(a => a.EnableType == "ItemPriceInPurchase").FirstOrDefault();
            ViewBag.ItemPriceInPurchase = ItemPriceInPurchase != null ? ItemPriceInPurchase.Status : Status.inactive;

            var checkdata = chkInCompleteTransaction();

            var EnableJewellery = db.EnableSettings.Where(a => a.EnableType == "EnableJewellery").FirstOrDefault();
            var JewCheckable = EnableJewellery != null ? EnableJewellery.Status : Status.inactive;
            ViewBag.JewCheck = JewCheckable;
            _FinancialYear();

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var pos = db.EnableSettings.Where(a => a.EnableType == "POSInvoice").FirstOrDefault();
            var poscheck = pos != null ? pos.Status : Status.inactive;
            ViewBag.POS = poscheck;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var EditPermission = User.IsInRole("Disable Purchase Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "PurchaseEntry", UserId);

            companySet();

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Purchase" && a.Status == Status.active).ToList();

            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");

            ViewBag.DefMc = db.MCs.Where(a => a.MCName == "Hire").Select(a => a.MCId).FirstOrDefault();

            var DisCounPer = db.EnableSettings.Where(a => a.EnableType == "DiscountPercentage").FirstOrDefault();
            var DisPercheck = DisCounPer != null ? DisCounPer.Status : Status.inactive;
            ViewBag.DisPercheck = DisPercheck;

            //dummy table operations
            var DItem = db.DummyPEItems.Where(a => a.PurchaseEntry == id).FirstOrDefault();
            var PItem = db.PEItemss.Where(a => a.PurchaseEntry == id).FirstOrDefault();
            if (PItem == null && DItem != null)
            {
                var DItems = db.DummyPEItems.Where(a => a.PurchaseEntry == id).ToList();
                foreach (var arr in DItems)
                {
                    //add to se-item table
                    PEItems pItem = new PEItems();
                    pItem.ItemUnit = arr.ItemUnit;
                    pItem.ItemUnitPrice = arr.ItemUnitPrice;
                    pItem.ItemQuantity = arr.ItemQuantity;
                    pItem.ItemSubTotal = arr.ItemSubTotal;
                    pItem.ItemDiscount = arr.ItemDiscount;
                    pItem.ItemTax = arr.ItemTax;
                    pItem.ItemTaxAmount = arr.ItemTaxAmount;
                    pItem.ItemTotalAmount = arr.ItemTotalAmount;
                    pItem.itemNote = arr.itemNote;
                    pItem.PurchaseEntry = arr.PurchaseEntry;
                    pItem.Item = arr.Item;
                    pItem.ProjectId = arr.ProjectId;
                    pItem.TaskId = arr.TaskId;
                    db.PEItemss.Add(pItem);
                    db.SaveChanges();
                }

                db.DummyPEItems.RemoveRange(db.DummyPEItems.Where(a => a.PurchaseEntry == id));
                db.SaveChanges();
            }

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


            var ref1 = db.PurchaseEntrys
              .Select(s => new
              {
                  ID = s.Ref1,
                  Name = s.Ref1
              }).Distinct()
              .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name", vmodel.Ref1);

            var ref2 = db.PurchaseEntrys
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name", vmodel.Ref2);

            var ref3 = db.PurchaseEntrys
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name", vmodel.Ref3);

            var ref4 = db.PurchaseEntrys
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name", vmodel.Ref4);

            var ref5 = db.PurchaseEntrys
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name", vmodel.Ref5);

          rtype = Request.Query["rtype"];
            if (rtype == "APP")
            {
                return View("App/Edit", vmodel);
            }
            else
            {
                return View("Edit", vmodel);
            }


        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Purchase Entry")]
        public ActionResult pull(string billno)
        {
            
            ApplicationDbContext db = new ApplicationDbContext("quicknet");

            var purchaseentry = new PurchaseEntryViewModel
            {
                BillNo = InvoiceNo(),
                PEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                PENote = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "purchase").Select(a => a.TermsCondit).FirstOrDefault(),
                PurchaseTypes = db.PurchaseTypes.ToList(),
                Supplier =4
               
            };
            SalesEntry porder = db.SalesEntrys.Where(o => o.BillNo == billno).FirstOrDefault();
            if (porder == null)
            {
                return NotFound();
            }
       
            purchaseentry.BillNo = porder.BillNo;

            purchaseentry.PEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
            purchaseentry.PECashier = porder.SECashier;
            purchaseentry.SupplierType = SupplierType.CreditSale;
            purchaseentry.PEDiscount = porder.SEDiscount;
            purchaseentry.PEGrandTotal = porder.SEGrandTotal;
            purchaseentry.Remarks = porder.Remarks;
            
            purchaseentry.Branch = porder.Branch;

            purchaseentry.PENote = porder.PaymentTerms;
            db = new ApplicationDbContext();
            var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
            var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;
            ViewBag.StockTrnsfrUpdate = brcheck;
            var sup = db.Suppliers.Select(s => new
            {
                SupplierID = s.SupplierID,
                SupplierDetails = s.SupplierCode + " - " + s.SupplierName
            }).ToList();
            ViewBag.Supp = QkSelect.List(sup, "SupplierID", "SupplierDetails");
            var use = db.Employees
                             .Select(s => new
                             {
                                 ID = s.EmployeeId,
                                 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                             })
                             .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");
            companySet();

            var userpermission = User.IsInRole("All Purchase Entry");
            var UserId = User.Identity.GetUserId();

            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            ViewBag.LastEntry = db.PurchaseEntrys.Where(p => (!MCList.Any() || MCArray.Contains(p.MaterialCenter)) && (userpermission == true || p.CreatedBy == UserId)).Select(p => p.PurchaseEntryId).AsEnumerable().DefaultIfEmpty(0).Max();

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

            var EnableCurrency = db.EnableSettings.Where(a => a.EnableType == "EnableCurrency").FirstOrDefault();
            var CurrencyEnable = EnableCurrency != null ? EnableCurrency.Status : Status.inactive;
            ViewBag.EnableCurrency = CurrencyEnable;

            var Currency = db.CurrencyMasters
              .Select(s => new
              {
                  Id = s.Id,
                  Name = s.CurrencyCode
              }).ToList();
            ViewBag.CurrencyVal = QkSelect.List(Currency, "Id", "Name");

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;


            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            ViewBag.CheckMail = mail != null ? mail.Status : Status.inactive;

            var ItemPriceInPurchase = db.EnableSettings.Where(a => a.EnableType == "ItemPriceInPurchase").FirstOrDefault();
            ViewBag.ItemPriceInPurchase = ItemPriceInPurchase != null ? ItemPriceInPurchase.Status : Status.inactive;

            var checkdata = chkInCompleteTransaction();

            var EnableJewellery = db.EnableSettings.Where(a => a.EnableType == "EnableJewellery").FirstOrDefault();
            var JewCheckable = EnableJewellery != null ? EnableJewellery.Status : Status.inactive;
            ViewBag.JewCheck = JewCheckable;
            _FinancialYear();

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var PurchaseInvoice = db.EnableSettings.Where(a => a.EnableType == "EnablePurchaseInvoice").FirstOrDefault();
            ViewBag.PurchaseInvoice = PurchaseInvoice != null ? PurchaseInvoice.Status : Status.inactive;
            var pos = db.EnableSettings.Where(a => a.EnableType == "POSInvoice").FirstOrDefault();
            var poscheck = pos != null ? pos.Status : Status.inactive;
            ViewBag.POS = poscheck;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var appby = db.Employees.Where(a => a.UserStatus == true)
                .Select(s => new
                {
                    ID = s.EmployeeId,
                    Name = s.FirstName + " " + s.LastName
                })
                .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var MlaPEntry = db.EnableSettings.Where(a => a.EnableType == "MLAPEntry").FirstOrDefault();
            var MlaPEntrys = MlaPEntry != null ? MlaPEntry.Status : Status.inactive;
            ViewBag.MLAPEntry = MlaPEntrys;

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            purchaseentry.FieldMap = db.FieldMappings.Where(a => a.Section == "Purchase" && a.Status == Status.active).ToList();

            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.Contype = "pull";

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

            ViewBag.DefMc = db.MCs.Where(a => a.MCName == "Hire").Select(a => a.MCId).FirstOrDefault();

            var DisCounPer = db.EnableSettings.Where(a => a.EnableType == "DiscountPercentage").FirstOrDefault();
            var DisPercheck = DisCounPer != null ? DisCounPer.Status : Status.inactive;
            ViewBag.DisPercheck = DisPercheck;

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
                return View("App/Create", purchaseentry);
            }
            else
            {
                return View("Create",purchaseentry);
            }

           
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Purchase Entry")]
    
      
        public ActionResult Create(long? id, string type)
        {
            var SuperUserEdit = db.EnableSettings.Where(a => a.EnableType == "SuperUserEdit").FirstOrDefault();
            var SuperUserEditvalue = SuperUserEdit != null ? SuperUserEdit.Status : Status.inactive;
            ViewBag.SuperUserEditvalue = SuperUserEditvalue;
            var ref1 = db.PurchaseEntrys
            .Select(s => new
            {
                ID = s.Ref1,
                Name = s.Ref1
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name");

            var ref2 = db.PurchaseEntrys
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name");

            var ref3 = db.PurchaseEntrys
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name");

            var ref4 = db.PurchaseEntrys
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name");

            var ref5 = db.PurchaseEntrys
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name");


            var purchaseentry = new PurchaseEntryViewModel
            {
                BillNo = InvoiceNo(),
                PEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString()),
                PENote = db.TermsAndConditionss.Where(a => a.ConditionTypeID == "purchase").Select(a => a.TermsCondit).FirstOrDefault(),
                PurchaseTypes = db.PurchaseTypes.ToList()
            };
            var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
            var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;
            ViewBag.StockTrnsfrUpdate = brcheck;
            if (id != null)
            {
                if (type == "POrder")
                {

                    PurchaseOrder porder = db.PurchaseOrders.Find(id);
                    if (porder == null)
                    {
                        return NotFound();
                    }
                    purchaseentry.ConTypeId = porder.PurchaseOrderId;
                    purchaseentry.ConType = type;
                    purchaseentry.BillNo = InvoiceNo();

                    purchaseentry.PEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    purchaseentry.PECashier = porder.POCashier;
                    purchaseentry.SupplierType = SupplierType.CreditSale;
                    purchaseentry.Supplier = porder.Supplier;
                    purchaseentry.PEDiscount = porder.PODiscount;
                    purchaseentry.PEGrandTotal = porder.POGrandTotal;
                    purchaseentry.Remarks = porder.Remarks;
                    var supp = db.Suppliers.Find(porder.Supplier);
                    if (supp.Contact != null)
                    {
                        purchaseentry.suppEmailId = db.Contacts.Where(a => a.ContactID == supp.Contact).Select(a => a.EmailId).FirstOrDefault();
                    }
                    purchaseentry.Branch = porder.Branch;

                    purchaseentry.PENote = porder.TermsCondition;
                    purchaseentry.convertFrom = type + " No";//label
                    purchaseentry.convertBill = porder.BillNo;
                    purchaseentry.PENote = porder.TermsCondition;
                }
                if (type == "PQuote")
                {
                    PurchaseQuotation PQuote = db.PurchaseQuotations.Find(id);
                    if (PQuote == null)
                    {
                        return NotFound();
                    }
                    purchaseentry.ConTypeId = PQuote.PQuotationId;
                    purchaseentry.ConType = type;
                    purchaseentry.PEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    purchaseentry.PECashier = PQuote.PQuotCashier;
                    purchaseentry.SupplierType = SupplierType.CreditSale;
                    purchaseentry.Supplier = PQuote.Supplier;
                    purchaseentry.PEDiscount = PQuote.PQuotDiscount;
                    purchaseentry.PEGrandTotal = PQuote.PQuotGrandTotal;
                    purchaseentry.Remarks = PQuote.Remarks;
                    var supp = db.Suppliers.Find(PQuote.Supplier);
                    if (supp.Contact != null)
                    {
                        purchaseentry.suppEmailId = db.Contacts.Where(a => a.ContactID == supp.Contact).Select(a => a.EmailId).FirstOrDefault();
                    }
                    purchaseentry.Branch = PQuote.Branch;

                    purchaseentry.convertFrom = type + " No";//label
                    purchaseentry.convertBill = PQuote.BillNo;
                    purchaseentry.PENote = PQuote.TermsCondition;
                }
                if (type == "MOrder")
                {

                    MaterialRequisition morder = db.MaterialRequisitions.Find(id);
                    if (morder == null)
                    {
                        return NotFound();
                    }
                    purchaseentry.ConTypeId = morder.MaterialRequisitionId;
                    purchaseentry.ConType = type;
                    purchaseentry.PEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    purchaseentry.PECashier = morder.MRCashier;
                    purchaseentry.SupplierType = SupplierType.CreditSale;
                    purchaseentry.Supplier = 0;
                    purchaseentry.PEDiscount = 0;
                    purchaseentry.PEGrandTotal = 0;
                    purchaseentry.Remarks = morder.Remarks;
                    purchaseentry.suppEmailId = "";
                    purchaseentry.Branch = morder.Branch;

                    purchaseentry.convertFrom = type + " No";//label
                    purchaseentry.convertBill = morder.BillNo;

                    purchaseentry.PENote = morder.TermsCondition;

                }
                if (type == "MRNote")
                {

                    MaterialReceiveNote mrnote = db.MaterialReceiveNotes.Find(id);
                    if (mrnote == null)
                    {
                        return NotFound();
                    }
                    purchaseentry.ConTypeId = mrnote.MRId;
                    purchaseentry.ConType = type;
                    purchaseentry.CMRNoteNo = mrnote.MRNo;

                    var CFrom = db.ConvertTransactionss.Where(a => a.To == mrnote.MRId && a.ConvertFrom == "POrder" && a.ConvertTo == "MRNote").Select(a => a.From).FirstOrDefault();
                    purchaseentry.CPorderNo = db.PurchaseOrders.Where(a => a.PurchaseOrderId == CFrom).Select(a => a.PONo).FirstOrDefault();

                    var SFrom = db.ConvertTransactionss.Where(a => a.To == CFrom && a.ConvertFrom == "PQuote" && a.ConvertTo == "POrder").Select(a => a.From).FirstOrDefault();
                    purchaseentry.CPQuotNo = db.PurchaseQuotations.Where(a => a.PQuotationId == SFrom).Select(a => a.PQuotNo).FirstOrDefault();

                    var TFrom = db.ConvertTransactionss.Where(a => a.To == SFrom && a.ConvertFrom == "MOrder" && a.ConvertTo == "PQuote").Select(a => a.From).FirstOrDefault();
                    purchaseentry.CMReqNo = db.MaterialRequisitions.Where(a => a.MaterialRequisitionId == TFrom).Select(a => a.MRNo).FirstOrDefault();


                    purchaseentry.PEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    purchaseentry.PECashier = mrnote.Cashier;
                    purchaseentry.SupplierType = SupplierType.CreditSale;
                    purchaseentry.Supplier = mrnote.Supplier;
                    purchaseentry.PEDiscount = 0;
                    purchaseentry.PEGrandTotal = 0;
                    purchaseentry.Remarks = mrnote.Remarks;
                    var supp = db.Suppliers.Find(mrnote.Supplier);
                    if (supp.Contact != null)
                    {
                        purchaseentry.suppEmailId = db.Contacts.Where(a => a.ContactID == supp.Contact).Select(a => a.EmailId).FirstOrDefault();
                    }
                    purchaseentry.Branch = mrnote.Branch;

                    purchaseentry.convertFrom = type + " No";//label
                    purchaseentry.convertBill = mrnote.BillNo;

                    purchaseentry.PENote = mrnote.TermsCondition;
                }
                if (type == "PurchaseExtend")
                {
                    PurchaseEntry pentry = db.PurchaseEntrys.Find(id);
                    if (pentry == null)
                    {
                        return NotFound();
                    }

                    var Extension = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.To == id).Select(y => y.From).FirstOrDefault();
                    List<ConvertTransactions> Trans;
                    List<ConvertTransactions> ExtList = new List<ConvertTransactions>();
                    int num = 0;
                    if (Extension != 0)
                    {
                        Trans = ExtNum((long)id, ExtList);
                        num = Trans.Count + 1;
                    }
                    else
                    {
                        num = 1;
                    }

                    purchaseentry.BillNo = InvoiceNo(0, null, "Hire") + "/Ex-" + num;
                    purchaseentry.PEDate = Convert.ToDateTime(System.DateTime.Now.ToShortDateString());
                    purchaseentry.PECashier = pentry.PECashier;
                    purchaseentry.SupplierType = SupplierType.CreditSale;
                    purchaseentry.Supplier = pentry.Supplier;
                    purchaseentry.PEDiscount = pentry.PEDiscount;
                    purchaseentry.PEGrandTotal = pentry.PEGrandTotal;
                    var custmr = db.Customers.Find(pentry.Supplier);
                    if (custmr != null)
                    {
                        purchaseentry.suppEmailId = db.Contacts.Where(a => a.ContactID == custmr.Contact).Select(a => a.EmailId).FirstOrDefault();
                    }
                    purchaseentry.ConTypeId = pentry.PurchaseEntryId;
                    purchaseentry.ConType = type;
                    purchaseentry.Remarks = pentry.Remarks;
                    purchaseentry.PurchaseType = pentry.PurchaseType;
                    purchaseentry.PurchaseHireType = pentry.PurType;
                    purchaseentry.MaterialCenter = pentry.MaterialCenter;

                    purchaseentry.convertFrom = type;// + " No";//label
                    purchaseentry.convertBill = pentry.BillNo;
                    purchaseentry.PENote = pentry.PENote;

                    if (pentry.PurType == PurchaseHireType.CrossHire)
                    {
                        var Hdet = db.HireDetails.Where(x => x.Reference == id && x.Section == "Purchase").FirstOrDefault();
                        if (Hdet != null)
                        {
                            purchaseentry.FromDate = Hdet.EndDate;
                            purchaseentry.CrossHireType = Hdet.HireType;
                        }
                    }
                }
            }
            var sup = db.Suppliers.Select(s => new
            {
                SupplierID = s.SupplierID,
                SupplierDetails = s.SupplierCode + " - " + s.SupplierName
            }).ToList();
            ViewBag.Supp = QkSelect.List(sup, "SupplierID", "SupplierDetails");
            var use = db.Employees
                             .Select(s => new
                             {
                                 ID = s.EmployeeId,
                                 Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                             })
                             .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");
            companySet();

            var userpermission = User.IsInRole("All Purchase Entry");
            var UserId = User.Identity.GetUserId();

            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            ViewBag.LastEntry = db.PurchaseEntrys.Where(p => (!MCList.Any() || MCArray.Contains(p.MaterialCenter)) && (userpermission == true || p.CreatedBy == UserId)).Select(p => p.PurchaseEntryId).AsEnumerable().DefaultIfEmpty(0).Max();

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

            var EnableCurrency = db.EnableSettings.Where(a => a.EnableType == "EnableCurrency").FirstOrDefault();
            var CurrencyEnable = EnableCurrency != null ? EnableCurrency.Status : Status.inactive;
            ViewBag.EnableCurrency = CurrencyEnable;

            var Currency = db.CurrencyMasters
              .Select(s => new
              {
                  Id = s.Id,
                  Name = s.CurrencyCode
              }).ToList();
            ViewBag.CurrencyVal = QkSelect.List(Currency, "Id", "Name");

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;


            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            ViewBag.CheckMail = mail != null ? mail.Status : Status.inactive;

            var ItemPriceInPurchase = db.EnableSettings.Where(a => a.EnableType == "ItemPriceInPurchase").FirstOrDefault();
            ViewBag.ItemPriceInPurchase = ItemPriceInPurchase != null ? ItemPriceInPurchase.Status : Status.inactive;

            var checkdata = chkInCompleteTransaction();

            var EnableJewellery = db.EnableSettings.Where(a => a.EnableType == "EnableJewellery").FirstOrDefault();
            var JewCheckable = EnableJewellery != null ? EnableJewellery.Status : Status.inactive;
            ViewBag.JewCheck = JewCheckable;
            _FinancialYear();

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var PurchaseInvoice = db.EnableSettings.Where(a => a.EnableType == "EnablePurchaseInvoice").FirstOrDefault();
            ViewBag.PurchaseInvoice = PurchaseInvoice != null ? PurchaseInvoice.Status : Status.inactive;
            var pos = db.EnableSettings.Where(a => a.EnableType == "POSInvoice").FirstOrDefault();
            var poscheck = pos != null ? pos.Status : Status.inactive;
            ViewBag.POS = poscheck;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var appby = db.Employees.Where(a => a.UserStatus == true )
                .Select(s => new
                {
                    ID = s.EmployeeId,
                    Name = s.FirstName + " " + s.LastName
                })
                .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var MlaPEntry = db.EnableSettings.Where(a => a.EnableType == "MLAPEntry").FirstOrDefault();
            var MlaPEntrys = MlaPEntry != null ? MlaPEntry.Status : Status.inactive;
            ViewBag.MLAPEntry = MlaPEntrys;

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            purchaseentry.FieldMap = db.FieldMappings.Where(a => a.Section == "Purchase" && a.Status == Status.active).ToList();

            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");
            ViewBag.Contype = type;

           
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();


            List<SelectFormat> serialisedJson;
            List<SelectFormat> serialisedJson2;
            var mchkadditional = db.AdditionalMc.Where(a => a.UserId == UserId).FirstOrDefault();
            if (mcchk != null || mchkadditional != null)
            {
                //    Id = s.MCId,
                //    Name = s.MCName

                serialisedJson = db.MCs.Where(p => p.AssignedUser == UserId).Select(b => new SelectFormat
                {
                    text = b.MCName,
                    id = b.MCId
                }).OrderBy(b => b.text).ToList();
                serialisedJson2 = db.AdditionalMc.Where(p => p.UserId == UserId).Select(b => new SelectFormat
                {
                    text = b.McName,
                    id = b.McId,
                }).OrderBy(b => b.text).ToList();

                serialisedJson = serialisedJson2.Union(serialisedJson).ToList();
                ViewBag.MC = QkSelect.List(serialisedJson, "id", "text");
                ViewBag.LastMc = serialisedJson.Select(a => a.id).FirstOrDefault();
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

            ViewBag.DefMc = db.MCs.Where(a => a.MCName == "Hire").Select(a => a.MCId).FirstOrDefault();

            var DisCounPer = db.EnableSettings.Where(a => a.EnableType == "DiscountPercentage").FirstOrDefault();
            var DisPercheck = DisCounPer != null ? DisCounPer.Status : Status.inactive;
            ViewBag.DisPercheck = DisPercheck;

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;
            var EnableRackWise = db.EnableSettings.Where(a => a.EnableType == "RackWiseStock").FirstOrDefault();
            var RackCheck = EnableRackWise != null ? EnableRackWise.Status : Status.inactive;
            ViewBag.RackWiseStock = RackCheck;

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
                return View("App/Create", purchaseentry);
            }
            else
            {
                return View(purchaseentry);
            }
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Purchase Entry")]
        public JsonResult CreatePurchase(string[][] array, string[] purchasedata, PEBillSundryViewModel bsmodel, ICollection<BatchStockPViewModel> bstmodel, ICollection<RackStockPViewModel> bsrackData)
        {
            bool stat = false;
            string msg;
            string billno= Convert.ToString(purchasedata[14]);
            long supp= Convert.ToInt64(purchasedata[0]);
            var exist = db.PurchaseEntrys.Any(c => c.BillNo == billno && c.Supplier == supp);
            if (!exist)
            {
                var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
                var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

                var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
                var PreChk = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;

                var EnableJewellery = db.EnableSettings.Where(a => a.EnableType == "EnableJewellery").FirstOrDefault();
                var EnableJew = EnableJewellery != null ? EnableJewellery.Status : Status.inactive;

                var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
                var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;
                var Today = Convert.ToDateTime(System.DateTime.Now);



                var UserId = User.Identity.GetUserId();
                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();

                long Branch = 0;
                Int64 purAcc = (long)db.companys.Select(a => a.PurchaseAccount).FirstOrDefault();

                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                if (BranchCheck == Status.active)
                {
                    Branch = Convert.ToInt64(purchasedata[22]);
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

                var EnableCurrency = db.EnableSettings.Where(a => a.EnableType == "EnableCurrency").FirstOrDefault();
                var EnableCur = EnableCurrency != null ? EnableCurrency.Status : Status.inactive;

                long Currency = 0;
                var ConRate = "";
                if (EnableCur == Status.active)
                {
                    Currency = Convert.ToInt64(purchasedata[23]);
                    ConRate = purchasedata[24];
                }
                else
                {
                    Currency = db.CurrencyMasters.Where(a => a.editable == choice.No).Select(a => a.Id).FirstOrDefault();
                    ConRate = db.CurrencyMasters.Where(a => a.editable == choice.No).Select(a => a.ConvertionRate).FirstOrDefault();
                }


                var TaxAmount = Convert.ToDecimal(purchasedata[5]);
                var PEGrandTotal = Convert.ToDecimal(purchasedata[7]);
                var Purchaseamount = PEGrandTotal - TaxAmount;
                var subtotal = Convert.ToDecimal(purchasedata[8]);

                //sales entry
                PurchaseEntry PEentry = new PurchaseEntry();

                if (purchasedata[36] != null)
                {
                    string str = purchasedata[36];
                    PurchaseHireType Stype = (PurchaseHireType)Enum.Parse(typeof(PurchaseHireType), str);
                    PEentry.PurType = Stype;
                }
                else
                {
                    PEentry.PurType = PurchaseHireType.Purchase;
                }
                PEentry.PENo = GetPeNo();
                PEentry.BillNo = Convert.ToString(purchasedata[14]);
                PEentry.PEDate = DateTime.Parse(purchasedata[2], new CultureInfo("en-GB"));
                PEentry.PECashier = purchasedata[1] != "" ? Convert.ToInt64(purchasedata[1]) : 0;
                PEentry.Supplier = Convert.ToInt64(purchasedata[0]);
                PEentry.PayType = "";//need change
                PEentry.PEItems = Convert.ToInt32(purchasedata[3]);
                PEentry.PEItemQuantity = Convert.ToDecimal(purchasedata[4]);
                PEentry.PESubTotal = Convert.ToDecimal(purchasedata[8]);
                PEentry.PETax = Convert.ToDecimal(purchasedata[9]);
                PEentry.PETaxAmount = TaxAmount;
                PEentry.PEDiscount = Convert.ToDecimal(purchasedata[6]);
                PEentry.PEGrandTotal = PEGrandTotal;
                PEentry.PENote = purchasedata[11];
                PEentry.Print = 1;
                PEentry.PECreatedDate = Today;
                PEentry.CreatedBy = UserId;
                PEentry.Status = 1;
                PEentry.Branch = Branch;
                PEentry.PurchaseType = Convert.ToInt64(purchasedata[17]);
                PEentry.Remarks = purchasedata[18];
                PEentry.MaterialCenter = MC;
                PEentry.Currency = Currency;
                PEentry.ConvertionRate = ConRate;
                PEentry.FCTotal = Convert.ToDecimal(purchasedata[25]);
                PEentry.PurchaseStatus = Convert.ToInt32(purchasedata[43]);

                if (purchasedata[15] == "1")
                {
                    PEentry.SupplierType = SupplierType.CashSale;
                }
                else
                {
                    PEentry.SupplierType = SupplierType.CreditSale;
                }

                PEentry.Ref1 = Convert.ToString(purchasedata[31]);
                PEentry.Ref2 = Convert.ToString(purchasedata[32]);
                PEentry.Ref3 = Convert.ToString(purchasedata[33]);
                PEentry.Ref4 = Convert.ToString(purchasedata[34]);
                PEentry.Ref5 = Convert.ToString(purchasedata[35]);
                PEentry.PurchaseAccount = purAcc;
                PEentry.ReferenceNo = Convert.ToString(purchasedata[44]);
                db.PurchaseEntrys.Add(PEentry);
                db.SaveChanges();

                //To Update the PurchaseOrderStatus in PurchaseOrders Table(If Create is only from Purchase Order Form)
                if (purchasedata[20] != null && purchasedata[20] != "0" && purchasedata[20] != "" && purchasedata[21] != null && purchasedata[21] != "0" && purchasedata[21] == "POrder")
                {
                    Int64 PurOrderId = Convert.ToInt64(purchasedata[20]);
                    PurchaseOrder POrderObj = db.PurchaseOrders.Find(PurOrderId);
                    POrderObj.PurchaseOrderStatus = 1;
                    db.Entry(POrderObj).State = EntityState.Modified;
                    db.SaveChanges();
                }

                //To Update the quantity in Create Mode(ItemTransaction Table)

                Int64 purchaseEntryId = PEentry.PurchaseEntryId;

                if (PEentry.PurType == PurchaseHireType.CrossHire)
                {
                    HireDetail HDetils = new HireDetail();
                    HDetils.StartDate = DateTime.Parse(purchasedata[37], new CultureInfo("en-GB"));
                    HDetils.EndDate = DateTime.Parse(purchasedata[38], new CultureInfo("en-GB"));
                    HDetils.Section = "Purchase";
                    HDetils.HireType = Convert.ToInt64(purchasedata[39]);
                    HDetils.Reference = purchaseEntryId;
                    db.HireDetails.Add(HDetils);
                    db.SaveChanges();
                }

                if (purchasedata[20] != null && purchasedata[20] != "0" && purchasedata[20] != "" && purchasedata[21] != null && purchasedata[21] != "" && purchasedata[21] != "0")
                {
                    string[] List = purchasedata[20].Split(',');

                    foreach (var arr in List)
                    {
                        ConvertTransactions ConTran = new ConvertTransactions();

                        ConTran.ConvertFrom = purchasedata[21];
                        ConTran.ConvertTo = "Purchase";
                        ConTran.From = Convert.ToInt64(arr);
                        ConTran.To = purchaseEntryId;
                        ConTran.Status = 0;
                        ConTran.CreatedDate = Today;
                        ConTran.CreatedBy = UserId;
                        ConTran.Branch = Convert.ToInt32(BranchID);

                        db.ConvertTransactionss.Add(ConTran);
                        db.SaveChanges();
                        com.addlog(LogTypes.Created, UserId, "ConvertTransactions", "ConvertTransactionss", findip(), ConTran.Id, "Successfully Submitted Conversion");

                    }
                }
                var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
                var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;

                brcheck = Status.inactive;
                //batch stock

                if (bstmodel != null)
                {
                    foreach (var bst in bstmodel)
                    {
                        if (bst.StockIn != 0)
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
                            decimal bStockIn = 0;
                            if (bst.Unit == bst.Priunit)
                            {
                                bStockIn = bst.StockIn * bst.cfactor;
                            }
                            BatchStock Btst = new BatchStock();
                            Btst.BatchNo = bst.BatchNo;
                            Btst.Item = bst.Item;
                            Btst.Unit = bst.Unit;
                            Btst.Cost = bst.Cost;
                            Btst.StockIn = bStockIn;
                            Btst.StockOut = 0;
                            Btst.Order = bst.Order;
                            Btst.EXP = exp;
                            Btst.MFG = mfg;
                            Btst.Reference = purchaseEntryId;
                            Btst.Type = "Purchase";

                            Btst.CreatedDate = Today;
                            Btst.Date = DateTime.Parse(purchasedata[2], new CultureInfo("en-GB"));


                            db.BatchStocks.Add(Btst);
                        }
                    }
                    db.SaveChanges();
                }
                //rackstock
                if (bsrackData != null)
                {
                    foreach (var bst in bsrackData)
                    {
                        if (bst.StockOut != 0)
                        {

                            decimal bStockIn = 0;

                            shelfstockmovement Btst = new shelfstockmovement();
                            Btst.purpose = "Purchase";
                            Btst.itemid = bst.Item;
                            Btst.unitid = (long)bst.Unit;
                            Btst.rackmciid = (long)com.getrackmcid(MC, bst.RackNo, bst.ShelfNo);
                            Btst.qty = bst.StockOut;

                            Btst.referenceid = purchaseEntryId;


                            Btst.createddate = Today;
                            Btst.createdby = UserId;

                            db.shelfstockmovements.Add(Btst);
                        }
                    }
                    db.SaveChanges();
                }
                ////billsundry
                if (bsmodel.pebsundrys != null)
                {
                    string bsResult = string.Empty;


                    DataTable BsEntry = new DataTable();
                    BsEntry.Columns.Add("PurchaseEntry");
                    BsEntry.Columns.Add("BillSundry");
                    BsEntry.Columns.Add("BsValue");
                    BsEntry.Columns.Add("AmountType");
                    BsEntry.Columns.Add("BsType");
                    BsEntry.Columns.Add("BsAmount");

                    foreach (var bs in bsmodel.pebsundrys)
                    {
                        DataRow drw = BsEntry.NewRow();
                        drw["PurchaseEntry"] = purchaseEntryId;
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
                    parameter1.TypeName = "TableTypePEBillSundry";
                    //// execute sp sql 
                    string sql1 = String.Format("EXEC {0} {1};", "SP_InsertPEBillSundry", "@TableType");
                    //// execute sql 
                    db.Database.ExecuteSqlRaw(sql1, parameter1);
                    //-------------------------------------
                }

                //SEPayment
                PEPayment PEpay = new PEPayment();

                PEpay.SupplierId = Convert.ToInt64(purchasedata[0]);
                PEpay.PEDate = DateTime.Parse(purchasedata[2], new CultureInfo("en-GB"));
                PEpay.PEEntryDate = Today;
                PEpay.PEBillAmount = PEGrandTotal;

                if (purchasedata[15] == "1")
                {
                    PEpay.PEPaidAmount = PEGrandTotal;
                }
                else
                {
                    PEpay.PEPaidAmount = Convert.ToDecimal(purchasedata[10]);
                }

                PEpay.CreatedBranch = Convert.ToInt32(BranchID);
                PEpay.CreatedUserId = UserId;
                PEpay.PECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                PEpay.Status = 1;
                PEpay.PurchaseEntry = purchaseEntryId;
                db.PEPayments.Add(PEpay);
                db.SaveChanges();

                decimal amount = Convert.ToDecimal(purchasedata[10]);
                var supid = Convert.ToInt64(purchasedata[0]);
                Int64 cashAccId = db.Accountss.Where(a => a.Group == 9).Select(a => a.AccountsID).FirstOrDefault();
                Int64 suppAccID = db.Suppliers.Where(a => a.SupplierID == supid).Select(a => a.Accounts).FirstOrDefault();
                Int64 purAccId = purAcc; //db.Accountss.Where(a => a.Group == 16).Select(a => a.AccountsID).FirstOrDefault();
                Int64 VATInput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Input").Select(a => a.AccountsID).SingleOrDefault();

                var date = DateTime.Parse(purchasedata[2], new CultureInfo("en-GB"));
                if (Convert.ToDecimal(purchasedata[10]) > 0 || purchasedata[15] == "1")
                {
                    var Remark = "Direct Payment From Purchase Entry";
                    long payid;
                    //SETransaction
                    PETransaction PEtran = new PETransaction();

                    PEtran.SupplierId = Convert.ToInt64(purchasedata[0]);
                    PEtran.PEPayDate = DateTime.Parse(purchasedata[2], new CultureInfo("en-GB"));
                    if (purchasedata[15] == "1")
                    {
                        amount = PEGrandTotal;
                        PEtran.PEPayAmount = amount;
                        payid = com.addPayment(date, cashAccId, suppAccID, amount, amount, amount, Remark, UserId, BranchID, purchaseEntryId);

                    }
                    else
                    {
                        amount = Convert.ToDecimal(purchasedata[10]);
                        PEtran.PEPayAmount = amount;
                        payid = com.addPayment(date, cashAccId, suppAccID, amount, amount, amount, Remark, UserId, BranchID, purchaseEntryId);
                    }
                    PEtran.PaymentId = payid;
                    PEtran.PECreatedDate = Today;
                    PEtran.CreatedBranch = Convert.ToInt32(BranchID);
                    PEtran.CreatedUserId = UserId;
                    PEtran.Status = 1;
                    PEtran.PurchaseEntry = purchaseEntryId;

                    db.PETransactions.Add(PEtran);
                    db.SaveChanges();

                }

                //bill sundry account
                var Gtotal = PEGrandTotal;
                decimal deductions = 0;
                if (bsmodel.pebsundrys != null)
                {
                    foreach (var bs in bsmodel.pebsundrys)
                    {
                        var ChkAcc = db.BillSundrys.Where(a => a.BillSundryId == bs.BillSundry).FirstOrDefault();
                        if (ChkAcc.PAccount != null && ChkAcc.PAccount != 0)
                        {
                            var bsamount = bs.BsAmount == null ? 0 : (decimal)bs.BsAmount;
                            if (ChkAcc.BSType == 0)//additive
                            {
                                Purchaseamount = Purchaseamount - bsamount;
                                if (brcheck == Status.active)
                                {
                                    com.adddummyAccountTrasaction((bs.BsAmount == null) ? 0 : (decimal)bs.BsAmount, 0, (long)ChkAcc.PAccount, "Purchase", purchaseEntryId, DC.Debit, date);

                                }
                                else
                                {
                                    com.addAccountTrasaction((bs.BsAmount == null) ? 0 : (decimal)bs.BsAmount, 0, (long)ChkAcc.PAccount, "Purchase", purchaseEntryId, DC.Debit, date);
                                }
                            }
                            else //substract
                            {
                                Purchaseamount = Purchaseamount + bsamount;
                                if (brcheck == Status.active)
                                {
                                    com.adddummyAccountTrasaction(0, (bs.BsAmount == null) ? 0 : (decimal)bs.BsAmount, (long)ChkAcc.PAccount, "Purchase", purchaseEntryId, DC.Credit, date);

                                }
                                else
                                {
                                    com.addAccountTrasaction(0, (bs.BsAmount == null) ? 0 : (decimal)bs.BsAmount, (long)ChkAcc.PAccount, "Purchase", purchaseEntryId, DC.Credit, date);
                                }
                            }
                        }
                        else
                        {
                            var bsamount = (decimal)bs.BsAmount;
                            if (ChkAcc.BSType == 0)//additive
                            {
                                deductions = deductions - bsamount;
                            }
                            else //substract
                            {
                                deductions = deductions + bsamount;
                            }
                        }
                    }
                }
    
                //add trasaction to purchase account
                if (brcheck == Status.active)
                {
                    if (PEentry.PurchaseType != 3)
                        com.adddummyAccountTrasaction(Purchaseamount, 0, purAccId, "Purchase", purchaseEntryId, DC.Debit, date);
                    else
                        com.adddummyAccountTrasaction(PEentry.PESubTotal - deductions, 0, purAccId, "Purchase", purchaseEntryId, DC.Debit, date);

                    //add purchase trasaction 
                    com.adddummyAccountTrasaction(0, PEGrandTotal, suppAccID, "Purchase", purchaseEntryId, DC.Credit, date);
                    // add vat input in account transaction
                    if (TaxAmount > 0 && PEentry.PurchaseType != 3)
                        com.adddummyAccountTrasaction(TaxAmount, 0, VATInput, "Purchase", purchaseEntryId, DC.Debit, date);

                    if (Convert.ToDecimal(purchasedata[10]) > 0 || purchasedata[15] == "1")
                    {
                        //if payment
                        com.adddummyAccountTrasaction(amount, 0, suppAccID, "Purchase Payment", purchaseEntryId, DC.Debit, date);
                        com.adddummyAccountTrasaction(0, amount, cashAccId, "Purchase Payment", purchaseEntryId, DC.Credit, date);
                    }



                }
                else
                {
                    if (PEentry.PurchaseType != 3)
                        com.addAccountTrasaction(Purchaseamount, 0, purAccId, "Purchase", purchaseEntryId, DC.Debit, date);
                    else
                        com.addAccountTrasaction(PEentry.PESubTotal - deductions, 0, purAccId, "Purchase", purchaseEntryId, DC.Debit, date);

                    //add purchase trasaction 
                    com.addAccountTrasaction(0, PEGrandTotal, suppAccID, "Purchase", purchaseEntryId, DC.Credit, date);
                    // add vat input in account transaction
                    if (TaxAmount > 0 && PEentry.PurchaseType != 3)
                        com.addAccountTrasaction(TaxAmount, 0, VATInput, "Purchase", purchaseEntryId, DC.Debit, date);

                    if (Convert.ToDecimal(purchasedata[10]) > 0 || purchasedata[15] == "1")
                    {
                        //if payment
                        com.addAccountTrasaction(amount, 0, suppAccID, "Purchase Payment", purchaseEntryId, DC.Debit, date);
                        com.addAccountTrasaction(0, amount, cashAccId, "Purchase Payment", purchaseEntryId, DC.Credit, date);
                    }


                }



                var Appby = Convert.ToString(purchasedata[26]);
                if (Appby != null && Appby != "")
                {
                    long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                    Approval approval = new Approval();
                    foreach (var emp in Approve)
                    {
                        approval.TransEntry = PEentry.PurchaseEntryId;
                        approval.Type = "PurchaseEntry";
                        approval.EmployeeId = emp;
                        db.Approvals.Add(approval);
                        db.SaveChanges();
                    }
                }
                //----------------------
                com.addlog(LogTypes.Created, UserId, "PurchaseEntry", "PurchaseEntrys", findip(), purchaseEntryId, "Successfully Submitted Purchase Entry");
                //----------------------
                //---------------------


                string action = purchasedata[12];
                var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
                var barcode = enable != null ? enable.Status : Status.inactive;

                var CMRNoteNo = purchasedata[27] != "" ? Convert.ToInt64(purchasedata[27]) : 0;
                var CPorderNo = purchasedata[28] != "" ? Convert.ToInt64(purchasedata[28]) : 0;
                var CPQuotNo = purchasedata[29] != "" ? Convert.ToInt64(purchasedata[29]) : 0;
                var CMReqNo = purchasedata[30] != "" ? Convert.ToString(purchasedata[30]) : "";

                Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
                TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
                long purid = purchaseEntryId;

                if (action == "print")
                {
                    var conv = db.ConvertTransactionss.Any(u => u.To == purchaseEntryId);
                    List<ConvertTransactionsViewModel> ConvExt = new List<ConvertTransactionsViewModel>();
                    if (conv)
                    {
                        List<string> ExList = new List<string>();
                        List<ConvertTransactions> ExtList = new List<ConvertTransactions>();
                        ExtList = ExtNumDetails((long)purchaseEntryId, ExtList);
                        var Extended = ExtList.Select(z => z.To).ToList();
                        Int32 count = 0;


                        var ConvModel = (from a in db.ConvertTransactionss
                                         join b in db.PurchaseEntrys on a.To equals b.PurchaseEntryId into primary
                                         from b in primary.DefaultIfEmpty()
                                         where Extended.Contains(a.To)
                                         select new ConvertTransactionsViewModel
                                         {
                                             ConvertFrom = (a.ConvertFrom == "PurchaseExtend") ? "Purchase" : a.ConvertFrom,
                                             Id = b.PurchaseEntryId,
                                             BillNo = b.BillNo,
                                             CreatedDate = a.CreatedDate,
                                             From = a.From
                                         }).OrderBy(b => b.CreatedDate).ToList();

                        var parent = ExtList.Where(x => x.From == 0).Select(y => y.To).FirstOrDefault();
                        ConvertTransactionsViewModel parentvm = new ConvertTransactionsViewModel();
                        parentvm.BillNo = db.PurchaseEntrys.Where(x => x.PurchaseEntryId == parent).Select(y => y.BillNo).FirstOrDefault();
                        parentvm.Id = db.PurchaseEntrys.Where(x => x.PurchaseEntryId == parent).Select(y => y.PurchaseEntryId).FirstOrDefault();
                        parentvm.ConvertFrom = "PurchaseExtend";
                        ConvModel.Add(parentvm);
                        ConvExt = ConvModel.OrderBy(x => x.Id).ToList();

                        var str = ConvExt.Find(c => c.Id == purchaseEntryId);
                        ConvExt.Remove(str);
                    }
                    //field mapping
                    var fmapp = db.FieldMappings.Where(a => a.Section == "Purchase" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                    var BusnsType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                    var PrintType = db.EnableSettings.Where(a => a.EnableType == "POSInvoice").Select(x => x.Status).FirstOrDefault();

                    var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();

                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                    var PurchaseData = com.PurchaseData(purchaseEntryId, InPrintItemCode, PartNoCheck, TimeOut, ComHeadCheck, CMRNoteNo, CPorderNo, CPQuotNo, CMReqNo);

                    var item = PurchaseData.pdfItem.ToList();
                    var summary = PurchaseData;
                    var billsundry = PurchaseData.billsundry.ToList();

                    var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                    var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                    var def = (PriLay == Status.active) ? Convert.ToInt64(purchasedata[42]) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                    def = def == 0 ? 1 : def;
                    var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, barcode, layout, fmapp, purid = purchaseEntryId } };
                }
                else if (action == "sendmail")
                {
                    SendMail sm = new SendMail();
                    MailMessage message = new MailMessage();
                    string ToMail = purchasedata[16];
                    string CcMail = "";
                    string InvoiceNo = "_PurchaseEntry_" + PEentry.BillNo;

                    var em = db.EmailTemplates.Where(a => a.Head == "PurchaseEntry").FirstOrDefault();
                    if (em != null)
                    {
                        message.Subject = em.Subject;
                        message.Body = em.EmailBody;
                    }
                    else
                    {
                        message.Subject = "Purchase Entry";
                        message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                            " <p>we are enclosing our purchase entry for the items / services as requested by you during our discussions.<br/></p> " +
                            " <p>Looking forward to hear from you.</p>";
                    }
                    sm.SendPdfMail(generatePdf(purchaseEntryId), ToMail, CcMail, InvoiceNo, message);

                    msg = "Successfully submitted Purchase Entry.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, purid = purchaseEntryId } };

                }
                else
                {
                    msg = "Successfully Submitted Purchase Entry.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, barcode, purid = purchaseEntryId } };
                }
                //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                msg = "Invoice No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, purid = 0 } };

            }
        }
        public Int64 InsertDummyPEItems(string[] arr, Int64 EntryId, Int64 itemval, string BillNo)
        {
            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjChks = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

            var qty = 0;// = db.BatchStocks.Where(x => x.Invoice == BillNo && x.Item==itemval &&  x.type== "Purchase").Select(y => y.Quantity).Sum();

            DummyPEItem2 PEItem = new DummyPEItem2();
            PEItem.ItemUnit = (arr[1] != null) ? (long?)Convert.ToInt64(arr[1]) : null;
            PEItem.ItemUnitPrice = Convert.ToDecimal(arr[3]);
            PEItem.ItemQuantity = (qty > 0) ? qty : Convert.ToDecimal(arr[2]);
            PEItem.ItemSubTotal = Convert.ToDecimal(arr[5]);
            PEItem.ItemDiscount = Convert.ToDecimal(arr[6]);
            PEItem.ItemTax = Convert.ToDecimal(arr[10]);
            PEItem.ItemTaxAmount = Convert.ToDecimal(arr[9]);
            PEItem.ItemTotalAmount = Convert.ToDecimal(arr[11]);

            if (ProjChks == Status.active)
            {
                PEItem.ProjectId = Convert.ToInt64(arr[29]);
                PEItem.TaskId = Convert.ToInt64(arr[30]);
                PEItem.itemNote = Convert.ToString(arr[31].Replace("\n", "<br />"));
            }
            else
            {
                PEItem.ProjectId = 0;
                PEItem.TaskId = 0;
                PEItem.itemNote = Convert.ToString(arr[29].Replace("\n", "<br />"));
            }

            PEItem.PurchaseEntry = EntryId;
            PEItem.Item = itemval;

            db.DummyPEItems2.Add(PEItem);
            db.SaveChanges();
            var it = db.Items.Find(itemval);
            if (it.accountid != null && it.accountid != 0)
            {
                var pedate = db.PurchaseEntrys.Find(EntryId).PEDate;
                com.adddummyAccountTrasaction((PEItem.ItemTotalAmount == null) ? 0 : PEItem.ItemTotalAmount, 0, (long)it.accountid, "Purchase", EntryId, DC.Debit, pedate);
            }
            return PEItem.DummyPEItemId;
        }


        public Int64 InsertPEItems(string[] arr, Int64 EntryId, Int64 itemval, string BillNo)
        {
            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjChks = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

            var qty = 0;// = db.BatchStocks.Where(x => x.Invoice == BillNo && x.Item==itemval &&  x.type== "Purchase").Select(y => y.Quantity).Sum();

            PEItems PEItem = new PEItems();
            PEItem.ItemUnit = (arr[1] != null) ? (long?)Convert.ToInt64(arr[1]) : null;
            PEItem.ItemUnitPrice = Convert.ToDecimal(arr[3]);
            PEItem.ItemQuantity = (qty > 0) ? qty : Convert.ToDecimal(arr[2]);
            PEItem.ItemSubTotal = Convert.ToDecimal(arr[5]);
            PEItem.ItemDiscount = Convert.ToDecimal(arr[6]);
            PEItem.ItemTax = Convert.ToDecimal(arr[10]);
            PEItem.ItemTaxAmount = Convert.ToDecimal(arr[9]);
            PEItem.ItemTotalAmount = Convert.ToDecimal(arr[11]);

            if (ProjChks == Status.active)
            {
                PEItem.ProjectId = Convert.ToInt64(arr[29]);
                PEItem.TaskId = Convert.ToInt64(arr[30]);
                PEItem.itemNote = Convert.ToString(arr[31].Replace("\n", "<br />"));
            }
            else
            {
                PEItem.ProjectId = 0;
                PEItem.TaskId = 0;
                PEItem.itemNote = Convert.ToString(arr[29].Replace("\n", "<br />"));
            }

            PEItem.PurchaseEntry = EntryId;
            PEItem.Item = itemval;

            db.PEItemss.Add(PEItem);
            db.SaveChanges();
            var it = db.Items.Find(itemval);
            if (it.accountid != null && it.accountid != 0)
            {
                var pedate = db.PurchaseEntrys.Find(EntryId).PEDate;
                com.addAccountTrasaction((PEItem.ItemTotalAmount == null) ? 0 : PEItem.ItemTotalAmount, 0, (long)it.accountid, "Purchase", EntryId, DC.Debit, pedate);
            }
            var chkbundle = db.ItemBundles.Where(a => a.mainItem == itemval).Select(a => a.ItemBundleId).FirstOrDefault();
            if (chkbundle > 0)
            {
                var bunQuan = Convert.ToDecimal(arr[2]);
                var itemBundle = (from g in db.ItemBundles
                                  join b in db.Items on g.mainItem equals b.ItemID
                                  where b.ItemID == itemval
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


                    PEItem.ItemUnit = bu.ItemUnit;
                    PEItem.ItemUnitPrice = bu.ItemUnitPrice;
                    PEItem.ItemQuantity = (bunQuan * bu.quantity);
                    PEItem.ItemSubTotal = ItemSubTotal;
                    PEItem.ItemDiscount = itemval;
                    PEItem.ItemTax = itemtax;
                    PEItem.ItemTaxAmount = taxamt;
                    PEItem.ItemTotalAmount = totamt;
                    if (ProjChks == Status.active)
                    {
                        PEItem.ProjectId = Convert.ToInt64(arr[29]);
                        PEItem.TaskId = Convert.ToInt64(arr[30]);
                    }
                    else
                    {
                        PEItem.ProjectId = 0;
                        PEItem.TaskId = 0;
                    }
                    PEItem.itemNote = "-:{Bundle_Item}";
                    PEItem.PurchaseEntry = EntryId;
                    PEItem.Item = bu.Item;

                    db.PEItemss.Add(PEItem);
                    db.SaveChanges();


                }
            }

            return PEItem.PEItemsId;
        }
        [HttpPost]
        public bool validdate(string fdate)
        {
            DateTime? date = null;
            if (fdate != "")
            {
                date = DateTime.Parse(fdate, new CultureInfo("en-GB"));
                var userid = User.Identity.GetUserId();
                var today = DateTime.Now;
                var editableDay = DateTime.Now;
                var tem = 0;
                var userEditDays = db.UserEditDayss.Where(a => a.userid == userid).Select(a => a.pedate).FirstOrDefault();
                var userEdit = db.UserEditDayss.Where(a => a.userid == userid).Select(a => a.id).FirstOrDefault();
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
                    editableDay = today.AddMinutes(-Convert.ToDouble(userEditDays));
                }
                if (((DateTime) date - editableDay).TotalMinutes < 0 || tem == 1)
                {
                    return false;
                }
                else
                {
                    return true;
                }





         
                    }
           else
            {
                return false;
            }

        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Purchase List")]
        public ActionResult GetPurchaseEntryPaymentApproved(string BillNo, string FromDate, string ToDate, long? supplier, long? salesperson, long? type, string user, int? Balance, long? MC, string appstat, string PurchaseType, long? HireType, long? PurchaseStatus, string RefenceNo)
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
            var userpermission = User.IsInRole("All Purchase Entry");
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


            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
            var approveemp = db.AssignedTos.Where(o => o.CustomerID == -1).Select(o => o.EmployeeId).ToArray();

            var uDev = User.IsInRole("Dev");
            var uPurchaseEntryView = User.IsInRole("View Purchase Entry");
            var uEdit = User.IsInRole("Edit Purchase Entry");
            var uDownload = User.IsInRole("Download Purchase Entry");
            var uDelete = User.IsInRole("Delete Purchase Entry");
            var fromv = "Purchase";
            var tov = "PurchaseExtend";
            PurchaseHireType St = new PurchaseHireType();
            if (PurchaseType != "")
            {
                St = (PurchaseType == "2") ? PurchaseHireType.CrossHire : PurchaseHireType.Purchase;
            };
            if (BillNo == "" && FromDate == "" && ToDate == "" && supplier == 0 && salesperson == 0 && type == null && user == "" && Balance == null && MC == null && appstat == "" && PurchaseType == null && HireType == null && PurchaseStatus == null && search == "")
            {
                FromDate = "1";
                fdate = DateTime.Now.AddDays(-30);
            }
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            // chkAppStatus = latest ApprovalUpdate status per ApprovedBy (GroupBy-latest-per-group) per PurchaseEntry.
            // EF Core 10 cannot translate GroupBy(key).Select(g => g.OrderByDescending(...).FirstOrDefault())
            // (whole-row-per-group) inside a server projection, so it is materialized once and computed client-side.
            var chkAppStatusRows = db.ApprovalUpdates
                .Where(x => x.Type == "PurchaseEntryPayment")
                .Select(x => new { x.TransEntry, x.ApprovalStatus, x.ApprovedBy, x.CreatedDate })
                .ToList();
            var chkAppStatusLookup = chkAppStatusRows
                .GroupBy(a => a.TransEntry)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(l => l.ApprovedBy)
                    .Select(grp => grp.OrderByDescending(c => c.CreatedDate).First())
                    .Select(a => a.ApprovalStatus).ToList());
            var v = (from a in db.PurchaseEntrys
                     join b in db.Suppliers on a.Supplier equals b.SupplierID
                     join c in db.PEPayments on a.PurchaseEntryId equals c.PurchaseEntry
                     join d in db.Employees on a.PECashier equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                     join g in db.Users on a.CreatedBy equals g.Id
                     join i in db.MCs on a.MaterialCenter equals i.MCId into mcs
                     from i in mcs.DefaultIfEmpty()
                     join h in db.HireDetails on new { h1 = a.PurchaseEntryId, h2 = "Purchase" }
                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                     from h in hir.DefaultIfEmpty()
                         //let mc = db.MCs.Where(x => x.AssignedUser == a.CreatedBy).Select(x => x.MCId).FirstOrDefault()
                     let app = db.AssignedTos.Where(x => x.CustomerID == -1).Select(x => x.EmployeeId).ToList()
                     let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.PurchaseEntryId && x.Type == "PurchaseEntryPayment").Select(x => x.ApprovalStatus).ToList()
                     let chkAppStatusme = db.ApprovalUpdates.Where(x => x.TransEntry == a.PurchaseEntryId && x.Type == "PurchaseEntryPayment" && x.ApprovalStatus == ApprovalStatus.Approved && x.ApprovedBy == UserId).Select(x => x.ApprovedBy).FirstOrDefault()
                     let sh = db.ConvertTransactionss.Where(ap => ap.From == a.PurchaseEntryId && ap.ConvertFrom == tov && ap.ConvertTo == fromv).FirstOrDefault()

                     where 
                     (a.requestpayment == true) &&

                     (BillNo == null || BillNo == "" || a.BillNo == BillNo) && a.Status == 1 &&
                     (supplier == 0 || supplier == 0 || a.Supplier == supplier) &&
                     (salesperson == 0 || salesperson == null || a.PECashier == salesperson) &&
                     (type == null || a.SupplierType == SupType) &&
                     (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(a.PEDate, fdate) <= 0) &&
                     (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(a.PEDate, tdate) >= 0)
                     && (user == null || user == "" || g.Id == user)
                     //&& (PurchaseStatus == null || a.PurchaseStatus == PurchaseStatus || (PurchaseStatus == 2 && (a.PurchaseStatus == 0 || a.PurchaseStatus == null)))
                     && (PurchaseStatus == null || a.PurchaseStatus == PurchaseStatus)
                     && ((Balance == null) || (Balance == 1 ? (((decimal?)a.PEGrandTotal ?? 0) > ((decimal?)c.PEPaidAmount)) : (((decimal?)a.PEGrandTotal ?? 0) == ((decimal?)c.PEPaidAmount))))
                     //&& (mc == 0 || mc == a.MaterialCenter)
                     // && ((MCArray.Contains(a.MaterialCenter) && MC == a.MaterialCenter) || ((MC == null) && MCArray.Contains(a.MaterialCenter))) //&& (!MCList.Any() || MCArray.Contains(a.MaterialCenter))
                     && (MC == null || MC == a.MaterialCenter) //&& (!MCList.Any() || MCArray.Contains(a.MaterialCenter))
                     && (userpermission == true || a.CreatedBy == UserId)
                     && (PurchaseType == "" || PurchaseType == null || St == a.PurType) && (HireType == 0 || HireType == null || HireType == h.HireType)
                     && (search == "" || search == null || a.BillNo.Contains(search)) &&
                     (RefenceNo == null || RefenceNo == "" || a.ReferenceNo == RefenceNo)
                     select new
                     {
                         a.PurchaseEntryId,
                         a.PENo,
                         a.BillNo,
                         a.PEDate,
                         a.PECreatedDate,
                         a.PEGrandTotal,
                         a.SupplierType,
                         a.PurchaseStatus,
                         Supplier = b.SupplierCode + " - " + b.SupplierName,
                         EmpName = d.FirstName + " " + d.LastName,
                         User = g.UserName,
                         a.PayType,
                         a.Remarks,
                         PaymentStatus = c.Status,
                         PaymentTrans = 0,// db.PETransactions.Any(k => k.PurchaseEntry == a.PurchaseEntryId),
                         c.PEPaidAmount,
                         BalanceAmt = a.PEGrandTotal - c.PEPaidAmount,
                         Dev = uDev,
                         Details = uPurchaseEntryView,
                         Edit = uEdit,
                         Download = uDownload,
                         Delete = uDelete,
                         MC = i.MCName,

                         app = app,

                         AppStatus = AppStatus,
                         CreatedDate = a.PECreatedDate,
                         meexist = chkAppStatusme,
                         a.PurType,
                         HExtent = sh.ConvertFrom,
                         a.ReferenceNo,
                     }).ToList().Select(o =>
                     {
                         var chkAppStatus = chkAppStatusLookup.TryGetValue(o.PurchaseEntryId, out var ck) ? ck : new List<ApprovalStatus>();
                         return new
                     {
                         o.PurchaseEntryId,
                         o.PECreatedDate,
                         o.PENo,
                         o.BillNo,
                         o.PEDate,
                         o.PEGrandTotal,
                         o.SupplierType,
                         o.Supplier,
                         o.PurchaseStatus,
                         o.EmpName,
                         o.User,
                         o.PayType,
                         o.Remarks,
                         o.PaymentStatus,
                         o.PaymentTrans,
                         o.PEPaidAmount,
                         o.BalanceAmt,
                         o.Dev,
                         o.Details,
                         o.Edit,
                         o.Download,
                         o.Delete,
                         o.MC,
                         o.app,
                         Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                         ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && chkAppStatus.Count > 0) ? (chkAppStatus.Contains(ApprovalStatus.Completed))?ApprovalStatus.Completed:(chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && chkAppStatus != null && o.app.Count != 0 && chkAppStatus.Count != 0 && chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PartialApproval)) : ApprovalStatus.PendingApproval,
                         o.CreatedDate,
                         o.PurType,
                         o.HExtent,
                         o.ReferenceNo,
                         o.meexist
                     };
                     });

            v = v.Where(a => (a.ApprovalStatus == ApprovalStatus.Approved || a.ApprovalStatus == ApprovalStatus.PartialApproval));



            //search
            //    // Apply search   
            //                     p.BillNo.ToString().ToLower().Equals(search.ToLower())
            //                     //p.PEPaidAmount.ToString().ToLower().Contains(search.ToLower())
            //                     ////p.SEBalanceAmount.ToString().ToLower().Contains(search.ToLower())

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
        [QkAuthorize(Roles = "Dev,Purchase List")]
        public ActionResult GetPurchaseEntryPayment(string BillNo, string FromDate, string ToDate, long? supplier, long? salesperson, long? type, string user, int? Balance, long? MC, string appstat, string PurchaseType, long? HireType, long? PurchaseStatus, string RefenceNo)
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
            var userpermission = User.IsInRole("All Purchase Entry");
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


            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
            var approveemp = db.AssignedTos.Where(o => o.CustomerID == -1).Select(o => o.EmployeeId).ToArray();
            
            var uDev = User.IsInRole("Dev");
            var uPurchaseEntryView = User.IsInRole("View Purchase Entry");
            var uEdit = User.IsInRole("Edit Purchase Entry");
            var uDownload = User.IsInRole("Download Purchase Entry");
            var uDelete = User.IsInRole("Delete Purchase Entry");
            var fromv = "Purchase";
            var tov = "PurchaseExtend";
            PurchaseHireType St = new PurchaseHireType();
            if (PurchaseType != "")
            {
                St = (PurchaseType == "2") ? PurchaseHireType.CrossHire : PurchaseHireType.Purchase;
            };
            if (BillNo == "" && FromDate == "" && ToDate == "" && supplier == 0 && salesperson == 0 && type == null && user == "" && Balance == null && MC == null && appstat == "" && PurchaseType == null && HireType == null && PurchaseStatus == null && search == "")
            {
                FromDate = "1";
                fdate = DateTime.Now.AddDays(-30);
            }
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            // chkAppStatus = latest ApprovalUpdate status per ApprovedBy (GroupBy-latest-per-group) per PurchaseEntry.
            // EF Core 10 cannot translate GroupBy(key).Select(g => g.OrderByDescending(...).FirstOrDefault())
            // (whole-row-per-group) inside a server projection, so it is materialized once and computed client-side.
            var chkAppStatusRows = db.ApprovalUpdates
                .Where(x => x.Type == "PurchaseEntryPayment")
                .Select(x => new { x.TransEntry, x.ApprovalStatus, x.ApprovedBy, x.CreatedDate })
                .ToList();
            var chkAppStatusLookup = chkAppStatusRows
                .GroupBy(a => a.TransEntry)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(l => l.ApprovedBy)
                    .Select(grp => grp.OrderByDescending(c => c.CreatedDate).First())
                    .Select(a => a.ApprovalStatus).ToList());
            var v = (from a in db.PurchaseEntrys
                     join b in db.Suppliers on a.Supplier equals b.SupplierID
                     join c in db.PEPayments on a.PurchaseEntryId equals c.PurchaseEntry
                     join d in db.Employees on a.PECashier equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                     join g in db.Users on a.CreatedBy equals g.Id
                     join i in db.MCs on a.MaterialCenter equals i.MCId into mcs
                     from i in mcs.DefaultIfEmpty()
                     join h in db.HireDetails on new { h1 = a.PurchaseEntryId, h2 = "Purchase" }
                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                     from h in hir.DefaultIfEmpty()
                         //let mc = db.MCs.Where(x => x.AssignedUser == a.CreatedBy).Select(x => x.MCId).FirstOrDefault()
                     let app = db.AssignedTos.Where(x => x.CustomerID==-1).Select(x => x.EmployeeId).ToList()
                     let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.PurchaseEntryId && x.Type == "PurchaseEntryPayment").Select(x => x.ApprovalStatus).ToList()
                     let chkAppStatusme = db.ApprovalUpdates.Where(x => x.TransEntry == a.PurchaseEntryId && x.Type == "PurchaseEntryPayment"&& x.ApprovalStatus==ApprovalStatus.Approved && x.ApprovedBy ==UserId).Select(x=>x.ApprovedBy).FirstOrDefault()
                     let sh = db.ConvertTransactionss.Where(ap => ap.From == a.PurchaseEntryId && ap.ConvertFrom == tov && ap.ConvertTo == fromv).FirstOrDefault()

                     where (approveemp.Contains(empl.EmployeeId)) &&
                     (a.requestpayment==true) &&
                     
                     (BillNo == null || BillNo == "" || a.BillNo == BillNo) && a.Status == 1 &&
                     (supplier == 0 || supplier == 0 || a.Supplier == supplier) &&
                     (salesperson == 0 || salesperson == null || a.PECashier == salesperson) &&
                     (type == null || a.SupplierType == SupType) &&
                     (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(a.PEDate, fdate) <= 0) &&
                     (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(a.PEDate, tdate) >= 0)
                     && (user == null || user == "" || g.Id == user)
                     //&& (PurchaseStatus == null || a.PurchaseStatus == PurchaseStatus || (PurchaseStatus == 2 && (a.PurchaseStatus == 0 || a.PurchaseStatus == null)))
                     && (PurchaseStatus == null || a.PurchaseStatus == PurchaseStatus)
                     && ((Balance == null) || (Balance == 1 ? (((decimal?)a.PEGrandTotal ?? 0) > ((decimal?)c.PEPaidAmount)) : (((decimal?)a.PEGrandTotal ?? 0) == ((decimal?)c.PEPaidAmount))))
                     //&& (mc == 0 || mc == a.MaterialCenter)
                     // && ((MCArray.Contains(a.MaterialCenter) && MC == a.MaterialCenter) || ((MC == null) && MCArray.Contains(a.MaterialCenter))) //&& (!MCList.Any() || MCArray.Contains(a.MaterialCenter))
                     && (MC == null || MC == a.MaterialCenter) //&& (!MCList.Any() || MCArray.Contains(a.MaterialCenter))
                     && (userpermission == true || a.CreatedBy == UserId)
                     && (PurchaseType == "" || PurchaseType == null || St == a.PurType) && (HireType == 0 || HireType == null || HireType == h.HireType)
                     && (search == "" || search == null || a.BillNo.Contains(search)) &&
                     (RefenceNo == null || RefenceNo == "" || a.ReferenceNo == RefenceNo)
                     select new
                     {
                         a.PurchaseEntryId,
                         a.PENo,
                         a.BillNo,
                         a.PEDate,
                         a.PECreatedDate,
                         a.PEGrandTotal,
                         a.SupplierType,
                         a.PurchaseStatus,
                         Supplier = b.SupplierCode + " - " + b.SupplierName,
                         EmpName = d.FirstName + " " + d.LastName,
                         User = g.UserName,
                         a.PayType,
                         a.Remarks,
                         PaymentStatus = c.Status,
                         PaymentTrans = 0,// db.PETransactions.Any(k => k.PurchaseEntry == a.PurchaseEntryId),
                         c.PEPaidAmount,
                         BalanceAmt = a.PEGrandTotal - c.PEPaidAmount,
                         Dev = uDev,
                         Details = uPurchaseEntryView,
                         Edit = uEdit,
                         Download = uDownload,
                         Delete = uDelete,
                         MC = i.MCName,

                         app = app,

                         AppStatus = AppStatus,
                         CreatedDate = a.PECreatedDate,
                         meexist= chkAppStatusme,
                         a.PurType,
                         HExtent = sh.ConvertFrom,
                         a.ReferenceNo,
                     }).ToList().Select(o =>
                     {
                         var chkAppStatus = chkAppStatusLookup.TryGetValue(o.PurchaseEntryId, out var ck) ? ck : new List<ApprovalStatus>();
                         return new
                     {
                         o.PurchaseEntryId,
                         o.PECreatedDate,
                         o.PENo,
                         o.BillNo,
                         o.PEDate,
                         o.PEGrandTotal,
                         o.SupplierType,
                         o.Supplier,
                         o.PurchaseStatus,
                         o.EmpName,
                         o.User,
                         o.PayType,
                         o.Remarks,
                         o.PaymentStatus,
                         o.PaymentTrans,
                         o.PEPaidAmount,
                         o.BalanceAmt,
                         o.Dev,
                         o.Details,
                         o.Edit,
                         o.Download,
                         o.Delete,
                         o.MC,
                         o.app,
                         Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                         ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && chkAppStatus.Count > 0) ? (chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && chkAppStatus != null && o.app.Count != 0 && chkAppStatus.Count != 0 && chkAppStatus.Count() == o.app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PartialApproval)) : ApprovalStatus.PendingApproval,
                         o.CreatedDate,
                         o.PurType,
                         o.HExtent,
                         o.ReferenceNo,
                         o.meexist
                     };
                     });

                v = v.Where(a => (a.ApprovalStatus == ApprovalStatus.PendingApproval||a.ApprovalStatus==ApprovalStatus.PartialApproval) && a.meexist!=UserId);
           


            //search
            //    // Apply search   
            //                     p.BillNo.ToString().ToLower().Equals(search.ToLower())
            //                     //p.PEPaidAmount.ToString().ToLower().Contains(search.ToLower())
            //                     ////p.SEBalanceAmount.ToString().ToLower().Contains(search.ToLower())

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
        [QkAuthorize(Roles = "Dev,Purchase List")]
        public ActionResult GetPurchaseEntry(string BillNo, string FromDate, string ToDate, long? supplier, long? salesperson, long? type, string user, int? Balance, long? MC, string appstat, string PurchaseType, long? HireType, long? PurchaseStatus, string RefenceNo)
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
            var userpermission = User.IsInRole("All Purchase Entry");
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


            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var uDev = User.IsInRole("Dev");
            var uPurchaseEntryView = User.IsInRole("View Purchase Entry");
            var uEdit = User.IsInRole("Edit Purchase Entry");
            var uDownload = User.IsInRole("Download Purchase Entry");
            var uDelete = User.IsInRole("Delete Purchase Entry");
            var fromv = "Purchase";
            var tov = "PurchaseExtend";
            var Tosales = "Sale";
            PurchaseHireType St = new PurchaseHireType();
            if (PurchaseType != "")
            {
                St = (PurchaseType == "2") ? PurchaseHireType.CrossHire : PurchaseHireType.Purchase;
            };
            if (BillNo == "" && FromDate == "" && ToDate == "" && supplier == 0 && salesperson == 0 && type == null && user == "" && Balance == null && MC == null && appstat == "" && PurchaseType == null && HireType == null && PurchaseStatus == null && search == "")
            {
                FromDate = "1";
                fdate = DateTime.Now.AddDays(-30);
            }
            var today = DateTime.Now;
            var editableDay = DateTime.Now;
            var tem = 0;
            var userEditDays = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.pedays).FirstOrDefault();
            var userEdit = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.id).FirstOrDefault();
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
                editableDay = today.AddMinutes(-Convert.ToDouble(userEditDays));
            }
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            // EF Core 10 cannot translate the nested-collection / GroupBy-latest projections (the `app`,
            // `AppStatus`, `chkAppStatus` lets). Split SERVER from CLIENT: materialize only entity columns +
            // simple scalars (left-joined entity access like Supplier/EmpName/MC stays server-side) into
            // serverRows, then build client lookups keyed by PurchaseEntryId and re-project client-side with
            // the SAME member names + order.
            var serverQuery = (from a in db.PurchaseEntrys
                     join b in db.Suppliers on a.Supplier equals b.SupplierID
                     join c in db.PEPayments on a.PurchaseEntryId equals c.PurchaseEntry
                     join d in db.Employees on a.PECashier equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                     join g in db.Users on a.CreatedBy equals g.Id
                     join i in db.MCs on a.MaterialCenter equals i.MCId into mcs
                     from i in mcs.DefaultIfEmpty()
                     join h in db.HireDetails on new { h1 = a.PurchaseEntryId, h2 = "Purchase" }
                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                     from h in hir.DefaultIfEmpty()
                         //let mc = db.MCs.Where(x => x.AssignedUser == a.CreatedBy).Select(x => x.MCId).FirstOrDefault()
                         // app/AppStatus/chkAppStatus (nested collections + GroupBy-latest) are computed
                         // client-side after materialization — EF Core 10 can't translate them inside this query.
                    // let sh = db.ConvertTransactionss.Where(ap => ap.From == a.PurchaseEntryId && ap.ConvertFrom == tov && ap.ConvertTo == fromv).FirstOrDefault()
                   //  let qs = db.ConvertTransactionss.Where(ap => ap.From == a.PurchaseEntryId && ap.ConvertFrom == fromv && ap.ConvertTo == Tosales).FirstOrDefault()

                     where (BillNo == null || BillNo == "" || a.BillNo == BillNo) && a.Status == 1 &&
                     (supplier == 0 || supplier == 0 || a.Supplier == supplier) &&
                     (salesperson == 0 || salesperson == null || a.PECashier == salesperson) &&
                     (type == null || a.SupplierType == SupType) &&
                     (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(a.PEDate, fdate) <= 0) &&
                     (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(a.PEDate, tdate) >= 0)
                     && (user == null || user == "" || g.Id == user)
                     //&& (PurchaseStatus == null || a.PurchaseStatus == PurchaseStatus || (PurchaseStatus == 2 && (a.PurchaseStatus == 0 || a.PurchaseStatus == null)))
                     && (PurchaseStatus == null || a.PurchaseStatus == PurchaseStatus)
                     && ((Balance == null) || (Balance == 1 ? (((decimal?)a.PEGrandTotal ?? 0) > ((decimal?)c.PEPaidAmount)) : (((decimal?)a.PEGrandTotal ?? 0) == ((decimal?)c.PEPaidAmount))))
                     //&& (mc == 0 || mc == a.MaterialCenter)
                     // && ((MCArray.Contains(a.MaterialCenter) && MC == a.MaterialCenter) || ((MC == null) && MCArray.Contains(a.MaterialCenter))) //&& (!MCList.Any() || MCArray.Contains(a.MaterialCenter))
                     && (MC == null || MC == a.MaterialCenter) //&& (!MCList.Any() || MCArray.Contains(a.MaterialCenter))
                     && (userpermission == true || a.CreatedBy == UserId)
                     && (PurchaseType == "" || PurchaseType == null || St == a.PurType) && (HireType == 0 || HireType == null || HireType == h.HireType)
                     && (search == "" || search == null||a.BillNo.Contains(search))&&
                     (RefenceNo == null || RefenceNo == "" || a.ReferenceNo == RefenceNo)
                     && a.PEItemQuantity==0
                     select new
                     {
                         validornot = tem != 1 && (EF.Functions.DateDiffMinute(a.PEDate, editableDay) <= 0 && EF.Functions.DateDiffMinute(a.PEDate , today) >= 0) ? "valid" : "invalid",
                         userEditDays = userEditDays,
                         SaleConvert =0,// qs.ConvertTo,
                         a.PurchaseEntryId,
                         a.PENo,
                         a.BillNo,
                         a.PEDate,
                         a.PECreatedDate,
                         a.PEGrandTotal,
                         a.SupplierType,
                         a.PurchaseStatus,
                         Supplier = b.SupplierCode + " - " + b.SupplierName,
                         EmpName = d.FirstName + " " + d.LastName,
                         User = g.UserName,
                         a.PayType,
                         a.Remarks,
                         PaymentStatus = c.Status,
                         PaymentTrans =0,// db.PETransactions.Any(k => k.PurchaseEntry == a.PurchaseEntryId),
                         c.PEPaidAmount,
                         BalanceAmt = a.PEGrandTotal - c.PEPaidAmount,
                         Dev = uDev,
                         Details = uPurchaseEntryView,
                         Edit = uEdit,
                         Download = uDownload,
                         Delete = uDelete,
                         MC = i.MCName,

                         CreatedDate = a.PECreatedDate,
                         a.PurType,
                         HExtent =0,// sh.ConvertFrom,
                         a.ReferenceNo,
                     });

            // Performance (audit P2, hybrid): server paging when no search/plain sort AND the approval filter is inactive.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase){ "BalanceAmt","BillNo","CreatedDate","Delete","Details","Dev","Download","Edit","EmpName","HExtent","MC","PaymentStatus","PaymentTrans","PayType","PECreatedDate","PEDate","PEGrandTotal","PENo","PEPaidAmount","PurchaseEntryId","PurchaseStatus","PurType","ReferenceNo","Remarks","SaleConvert","Supplier","SupplierType","User","userEditDays","validornot" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0 && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn)) && !(appstat != ""&& appstat !=null);
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn) ? serverQuery.OrderBy("PurchaseEntryId asc") : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

            // CLIENT-side lookups keyed by PurchaseEntryId (missing key -> empty/absent, no KeyNotFound).
            var peIds = serverRows.Select(o => o.PurchaseEntryId).ToList();
            // app = approver EmployeeIds for the purchase entry (nested collection, keyed by TransEntry == PurchaseEntryId).
            var appLookup = db.Approvals
                .Where(x => x.Type == "PurchaseEntry" && peIds.Contains(x.TransEntry))
                .Select(x => new { x.TransEntry, x.EmployeeId })
                .ToList()
                .ToLookup(x => x.TransEntry);
            // AppStatus = all ApprovalUpdate statuses; raw rows materialized once and reused for chkAppStatus.
            var appUpdRows = db.ApprovalUpdates
                .Where(x => x.Type == "PurchaseEntry" && peIds.Contains(x.TransEntry))
                .Select(x => new { x.TransEntry, x.ApprovalStatus, x.ApprovedBy, x.CreatedDate })
                .ToList();
            var appStatusLookup = appUpdRows.ToLookup(x => x.TransEntry);
            // chkAppStatus = latest ApprovalUpdate status per ApprovedBy (GroupBy-latest-per-group), per purchase entry.
            var chkAppStatusLookup = appUpdRows
                .GroupBy(x => x.TransEntry)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(l => l.ApprovedBy)
                    .Select(grp => grp.OrderByDescending(c => c.CreatedDate).First())
                    .Select(x => x.ApprovalStatus).ToList());

            var v = serverRows.Select(o =>
                     {
                         var app = appLookup[o.PurchaseEntryId].Select(x => x.EmployeeId).ToList();
                         var AppStatus = appStatusLookup[o.PurchaseEntryId].Select(x => x.ApprovalStatus).ToList();
                         var chkAppStatus = chkAppStatusLookup.TryGetValue(o.PurchaseEntryId, out var ck) ? ck : new List<ApprovalStatus>();
                         return new
                     {
                         o.validornot,
                         o.userEditDays,
                         o.SaleConvert,
                         o.PurchaseEntryId,
                         o.PECreatedDate,
                         o.PENo,
                         o.BillNo,
                         o.PEDate,
                         o.PEGrandTotal,
                         o.SupplierType,
                         o.Supplier,
                         o.PurchaseStatus,
                         o.EmpName,
                         o.User,
                         o.PayType,
                         o.Remarks,
                         o.PaymentStatus,
                         o.PaymentTrans,
                         o.PEPaidAmount,
                         o.BalanceAmt,
                         o.Dev,
                         o.Details,
                         o.Edit,
                         o.Download,
                         o.Delete,
                         o.MC,
                         app = app,
                         PurToSale = (o.SaleConvert != null) ? false : true,
                         Approval = (app != null && empl.EmployeeId != null) ? (app.Contains(empl.EmployeeId) ? true : false) : false,
                         ApprovalStatus = (app.Count > 0 && AppStatus.Count > 0 && chkAppStatus.Count > 0) ? (chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (app != null && chkAppStatus != null && app.Count != 0 && chkAppStatus.Count != 0 ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                         o.CreatedDate,
                         o.PurType,
                         o.HExtent,
                         o.ReferenceNo
                         };
                     });
            if (appstat != ""&& appstat !=null)
            {

                v = v.Where(a => a.ApprovalStatus == AppSt&& a.app.Count()>0);
            }


            //search
            //    // Apply search   
            //                     p.BillNo.ToString().ToLower().Equals(search.ToLower())
            //                     //p.PEPaidAmount.ToString().ToLower().Contains(search.ToLower())
            //                     ////p.SEBalanceAmount.ToString().ToLower().Contains(search.ToLower())

            //SORT
            if (!fastPage && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
            }
            v = v.OrderByDescending (o => o.PurchaseEntryId).ThenByDescending(o=>o.ApprovalStatus);
            if (!fastPage) { recordsTotal = v.Count(); }
            var data = (fastPage ? v.ToList() : v.Skip(skip).Take(pageSize).ToList());
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        [HttpGet]
        public ActionResult GetPEItems3(long PurchaseEntryID)
        {
            var ConD = (from a in db.PEItemss
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.PurchaseEntry == PurchaseEntryID && a.itemNote != "-:{Bundle_Item}"
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
                            ItemNote = a.itemNote != null ? a.itemNote : "",
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
        [HttpGet]
        public ActionResult GetPEItems4(long PurchaseEntryID, string ConvertTo)
        {
            var temp = db.ConvertTransactionss.Where(a => a.From == PurchaseEntryID && a.ConvertFrom == "Purchase" && a.ConvertTo == ConvertTo).Select(a => a.To);
            List<ItemList2> SEItems = new List<ItemList2>();
            List<ItemList2> temp6 = new List<ItemList2>();
            List<ItemList2> SEitemsGroupBy = new List<ItemList2>();
            List<ItemList2> RemainingItems = new List<ItemList2>();
            foreach (var tem in temp)
            {
                var temp2 = (from a in db.SEItemss
                             join b in db.Items on a.Item equals b.ItemID into t1
                             from b in t1.DefaultIfEmpty()
                             join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                             from c in primary.DefaultIfEmpty()
                             join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                             from d in second.DefaultIfEmpty()
                             where a.SalesEntry == tem
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
                SEItems.AddRange(temp2);
            }
            SEitemsGroupBy = (from a in SEItems
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

            var ConD = (from a in db.PEItemss
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.PurchaseEntry == PurchaseEntryID && a.itemNote != "-:{Bundle_Item}"
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
            SEitemsGroupBy.AddRange(ConD);
            RemainingItems = (from a in SEitemsGroupBy
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
        [QkAuthorize(Roles = "Dev,View Purchase Entry")]
        public ActionResult Details(long? id)
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            PurchaseEntryViewModel vmodel = new PurchaseEntryViewModel();
            vmodel = (from b in db.PurchaseEntrys
                      join c in db.PEPayments on b.PurchaseEntryId equals c.PurchaseEntry into prs
                      from c in prs.DefaultIfEmpty()
                      join d in db.Employees on b.PECashier equals d.EmployeeId into emp
                      from d in emp.DefaultIfEmpty()
                      join f in db.Suppliers on b.Supplier equals f.SupplierID into supp
                      from f in supp.DefaultIfEmpty()
                      join h in db.MCs on b.MaterialCenter equals h.MCId into mcs
                      from h in mcs.DefaultIfEmpty()
                      join t in db.PurchaseTypes on b.PurchaseType equals t.Id into ptype
                      from t in ptype.DefaultIfEmpty()
                      join x in db.Contacts on f.Contact equals x.ContactID into cnt
                      from x in cnt.DefaultIfEmpty()
                      join u in db.HireDetails on new { h1 = b.PurchaseEntryId, h2 = "Purchase" }
                      equals new { h1 = u.Reference, h2 = u.Section } into hir
                      from u in hir.DefaultIfEmpty()
                      join v in db.HireTypes on u.HireType equals v.HireTypeId into htyp
                      from v in htyp.DefaultIfEmpty()
                      where b.PurchaseEntryId == id
                      select new PurchaseEntryViewModel
                      {
                          SupplierName = f.SupplierCode + " - " + f.SupplierName,
                          PENo = b.PENo,
                          BillNo = b.BillNo,
                          PEDate = b.PEDate,
                          PENote = b.PENote.Replace("\n", "<br />"),
                          EmployeeName = d.FirstName + " " + d.LastName,
                          SupplierType = b.SupplierType,
                          PEDiscount = b.PEDiscount,
                          PETotal = b.PEDiscount + b.PEGrandTotal,
                          PEGrandTotal = b.PEGrandTotal,
                          PEPaidAmount = b.SupplierType == 0 ? c.PEPaidAmount : b.PEGrandTotal,
                          PEDueAmount = b.SupplierType == 0 ? b.PEGrandTotal - c.PEPaidAmount : 0,
                          PayType = (b.SupplierType == SupplierType.CashSale ? "Cash" : "Credit"),
                          Remarks = b.Remarks.Replace("\n", "<br />"),
                          MCName = h.MCName,

                          PurTypeName = (b.PurType == PurchaseHireType.Purchase)?"Purchase": "CrossHire",
                          PursTypeName=t.Name,
                          EmailId=x.EmailId,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,

                          HType = (u != null) ? v.Name : "",
                          StartDate = (u != null) ? u.StartDate : null,
                          EndDate = (u != null) ? u.EndDate : null,
                          Emp = (from ab in db.Approvals
                                 join bb in db.Employees on ab.EmployeeId equals bb.EmployeeId
                                 where ab.TransEntry == id && ab.Type == "PurchaseEntry"
                                 select new ApprovalViewModel
                                 {
                                     EmpName = bb.FirstName + " " + bb.LastName
                                 }).ToList()
                      }).FirstOrDefault();
            var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
            var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;
            brcheck = Status.inactive;
            var dummyvalue = db.DummyPEItems2.Where(a => a.PurchaseEntry == id).FirstOrDefault();

            if (brcheck == Status.active)
            {
                if (dummyvalue != null)
                {
                    vmodel.PEItem = db.DummyPEItems2.Where(a => a.PurchaseEntry == id && a.itemNote != "-:{Bundle_Item}")
        .Select(b => new PEItemViewModel
        {
            ItemUnitPrice = b.ItemUnitPrice,
            ItemQuantity = b.ItemQuantity,
            ItemSubTotal = b.ItemSubTotal,
            ItemTax = b.ItemTax,
            ItemTaxAmount = b.ItemTaxAmount,
            ItemTotalAmount = b.ItemTotalAmount,
            itemNote = b.itemNote,
            ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
            ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
            ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault(),
            PartNumber = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.PartNumber).FirstOrDefault(),
            bundleitem = (from ab in db.DummyPEItems2
                          join bb in db.Items on ab.Item equals bb.ItemID
                          join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                          from cb in primary.DefaultIfEmpty()
                          join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                          from bd in second.DefaultIfEmpty()
                          where ab.PurchaseEntry == id && ab.itemNote == "-:{Bundle_Item}"
                          && b.Item == ab.ItemDiscount
                          select new ItemDetailViewModel
                          {
                              ItemCode = bb.ItemCode,
                              ItemName = bb.ItemName,
                              ItemUnit = cb.ItemUnitName,
                              ItemQuantity = ab.ItemQuantity,
                          }).ToList()
        }).ToList();
                }
              
                    else
                    {
                        vmodel.PEItem = db.PEItemss.Where(a => a.PurchaseEntry == id && a.itemNote != "-:{Bundle_Item}")
                .Select(b => new PEItemViewModel
                {
                    ItemUnitPrice = b.ItemUnitPrice,
                    ItemQuantity = b.ItemQuantity,
                    ItemSubTotal = b.ItemSubTotal,
                    ItemTax = b.ItemTax,
                    ItemTaxAmount = b.ItemTaxAmount,
                    ItemTotalAmount = b.ItemTotalAmount,
                    itemNote = b.itemNote,
                    ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                    ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                    ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault(),
                    PartNumber = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.PartNumber).FirstOrDefault(),
                    bundleitem = (from ab in db.PEItemss
                                  join bb in db.Items on ab.Item equals bb.ItemID
                                  join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                                  from cb in primary.DefaultIfEmpty()
                                  join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                                  from bd in second.DefaultIfEmpty()
                                  where ab.PurchaseEntry == id && ab.itemNote == "-:{Bundle_Item}"
                                  && b.Item == ab.ItemDiscount
                                  select new ItemDetailViewModel
                                  {
                                      ItemCode = bb.ItemCode,
                                      ItemName = bb.ItemName,
                                      ItemUnit = cb.ItemUnitName,
                                      ItemQuantity = ab.ItemQuantity,
                                  }).ToList()
                }).ToList();
                    }
                
            }
            else
            {
                vmodel.PEItem = db.PEItemss.Where(a => a.PurchaseEntry == id && a.itemNote != "-:{Bundle_Item}")
        .Select(b => new PEItemViewModel
        {
            ItemUnitPrice = b.ItemUnitPrice,
            ItemQuantity = b.ItemQuantity,
            ItemSubTotal = b.ItemSubTotal,
            ItemTax = b.ItemTax,
            ItemTaxAmount = b.ItemTaxAmount,
            ItemTotalAmount = b.ItemTotalAmount,
            itemNote = b.itemNote,
            ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
            ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
            ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.ItemUnit).Select(a => a.ItemUnitName).FirstOrDefault(),
            PartNumber = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.PartNumber).FirstOrDefault(),
            bundleitem = (from ab in db.PEItemss
                          join bb in db.Items on ab.Item equals bb.ItemID
                          join cb in db.ItemUnits on bb.ItemUnitID equals cb.ItemUnitID into primary
                          from cb in primary.DefaultIfEmpty()
                          join bd in db.ItemUnits on bb.SubUnitId equals bd.ItemUnitID into second
                          from bd in second.DefaultIfEmpty()
                          where ab.PurchaseEntry == id && ab.itemNote == "-:{Bundle_Item}"
                          && b.Item == ab.ItemDiscount
                          select new ItemDetailViewModel
                          {
                              ItemCode = bb.ItemCode,
                              ItemName = bb.ItemName,
                              ItemUnit = cb.ItemUnitName,
                              ItemQuantity = ab.ItemQuantity,
                          }).ToList()
        }).ToList();
            }
                vmodel.PEbs = db.PEBillSundrys.Where(a => a.PurchaseEntry == id)
           .Select(b => new PEBillSundryViewModel
           {
               AmountType = b.AmountType,
               BsAmount = b.BsAmount,
               BsType = b.BsType,
               BsValue = b.BsValue,
               Type = b.BsType == 0 ? "Add" : "Less",
               AmtType = b.AmountType == 0 ? "" : "%",
               BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
           }).ToList();

            var conv = db.ConvertTransactionss.Any(u => u.To == id);
            List<ConvertTransactionsViewModel> ConvExt = new List<ConvertTransactionsViewModel>();
            if (conv)
            {
                List<string> ExList = new List<string>();
                List<ConvertTransactions> ExtList = new List<ConvertTransactions>();
                ExtList = ExtNumDetails((long)id, ExtList);
                var Extended = ExtList.Select(z => z.To).ToList();
                Int32 count = 0;

                var ConvModel = (from a in db.ConvertTransactionss
                                 join b in db.PurchaseEntrys on a.To equals b.PurchaseEntryId into primary
                                 from b in primary.DefaultIfEmpty()
                                 where Extended.Contains(a.To)
                                 select new ConvertTransactionsViewModel
                                 {
                                     ConvertFrom = (a.ConvertFrom == "PurchaseExtend") ? "Purchase" : a.ConvertFrom,
                                     Id = b.PurchaseEntryId,
                                     BillNo = b.BillNo,
                                     CreatedDate = a.CreatedDate,
                                     From = a.From
                                 }).OrderBy(b => b.CreatedDate).ToList();

                var parent = ExtList.Where(x => x.From == 0).Select(y => y.To).FirstOrDefault();
                ConvertTransactionsViewModel parentvm = new ConvertTransactionsViewModel();
                parentvm.BillNo = db.PurchaseEntrys.Where(x => x.PurchaseEntryId == parent).Select(y => y.BillNo).FirstOrDefault();
                parentvm.Id = db.PurchaseEntrys.Where(x => x.PurchaseEntryId == parent).Select(y => y.PurchaseEntryId).FirstOrDefault();
                parentvm.ConvertFrom = "PurchaseExtend";
                ConvModel.Add(parentvm);
                ConvExt = ConvModel.OrderBy(x => x.Id).ToList();

                var str = ConvExt.Find(c => c.Id == id);
                ConvExt.Remove(str);
            }



            //                     where Extended.Contains(a.To)
            //                         ConvertFrom = a.ConvertFrom,
            //                         Id = b.PurchaseEntryId,
            //                         BillNo = b.BillNo,
            //                         CreatedDate = a.CreatedDate,
            //                         From = a.From


            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Purchase" && a.Status == Status.active).ToList();

            return View(vmodel);
        }
        [HttpGet]
        public ActionResult GetPEItems(long PurchaseEntryID)
        {

            var PEItem = (from a in db.PEItemss
                          join b in db.Items on a.Item equals b.ItemID
                          join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                          from c in primary.DefaultIfEmpty()
                          join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                          from d in second.DefaultIfEmpty()
                          join p in db.Projects on a.ProjectId equals p.ProjectId into proj
                          from p in proj.DefaultIfEmpty()
                          join t in db.ProTasks on a.TaskId equals t.ProTaskId into protask
                          from t in protask.DefaultIfEmpty()
                          where a.PurchaseEntry == PurchaseEntryID && a.itemNote != "-:{Bundle_Item}"
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
                              a.ProjectId,
                              a.TaskId,
                              p.ProjectName,
                              t.TaskName,
                              b.KeepStock,
                              b.slreq,
                              b.ConFactor,
                              batch = (from ay in db.BatchStocks
                                       join az in db.PEItemss on new { f1 = ay.Item, f2 = ay.Unit, f3 = ay.Reference, f4 = ay.Type }
                                            equals new { f1 = az.Item, f2 = az.ItemUnit, f3 = az.PurchaseEntry, f4 = "Purchase" }
                                       where az.PurchaseEntry == a.PurchaseEntry && ay.Item == a.Item
                                       select new BatchStockPViewModel
                                       {
                                           BatchNo = (ay.BatchNo != null)? ay.BatchNo : "",
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
                                           origin = "Purchase",
                                           Order = ay.Order
                                       }).ToList(),
                              rack = (from ay in db.shelfstockmovements
                                      join az in db.PEItemss on new { f1 = ay.itemid, f2 = ay.unitid, f3 = ay.referenceid, f4 = ay.purpose }
                                       equals new { f1 = az.Item, f2 = (long)az.ItemUnit, f3 = az.PurchaseEntry, f4 = "Purchase" }
                                      join aa in db.rackmaterialcentres on ay.rackmciid equals aa.rackmcid
                                      join b in db.Shelves on aa.shelfid equals b.ShelfId
                                      join c in db.Racks on aa.rackid equals c.RackId

                                      where az.PurchaseEntry == a.PurchaseEntry && ay.itemid == a.Item
                                      select new
                                      {
                                          ShelfNo = aa.shelfid,
                                          RackNo = aa.rackid,
                                          ShelfName = b.shelfName,
                                          RackName = c.RackName,
                                          StockIn = ay.qty,
                                      }).ToList()


                          });
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(PEItem);
            return Json(result);
        }
        [HttpGet]
        public ActionResult GetPEItems2(long PurchaseEntryID)
        {

            var PEItem = (from a in db.DummyPEItems2
                          join b in db.Items on a.Item equals b.ItemID
                          join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                          from c in primary.DefaultIfEmpty()
                          join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                          from d in second.DefaultIfEmpty()
                          join p in db.Projects on a.ProjectId equals p.ProjectId into proj
                          from p in proj.DefaultIfEmpty()
                          join t in db.ProTasks on a.TaskId equals t.ProTaskId into protask
                          from t in protask.DefaultIfEmpty()
                          where a.PurchaseEntry == PurchaseEntryID && a.itemNote != "-:{Bundle_Item}"
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
                              a.ProjectId,
                              a.TaskId,
                              p.ProjectName,
                              t.TaskName,
                              b.KeepStock,
                              b.slreq,
                              b.ConFactor,
                              batch = (from ay in db.BatchStocks
                                       join az in db.DummyPEItems2 on new { f1 = ay.Item, f2 = ay.Unit, f3 = ay.Reference, f4 = ay.Type }
                                            equals new { f1 = az.Item, f2 = az.ItemUnit, f3 = az.PurchaseEntry, f4 = "Purchase" }
                                       where az.PurchaseEntry == a.PurchaseEntry && ay.Item == a.Item
                                       select new BatchStockPViewModel
                                       {
                                           BatchNo = (ay.BatchNo != null) ? ay.BatchNo : "",
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
                                           origin = "Purchase",
                                           Order = ay.Order
                                       }).ToList(),
                              rack = (from ay in db.shelfstockmovements
                                      join az in db.DummyPEItems2 on new { f1 = ay.itemid, f2 = ay.unitid, f3 = ay.referenceid, f4 = ay.purpose }
                                       equals new { f1 = az.Item, f2 = (long)az.ItemUnit, f3 = az.PurchaseEntry, f4 = "Purchase" }
                                      join aa in db.rackmaterialcentres on ay.rackmciid equals aa.rackmcid
                                      join b in db.Shelves  on aa.shelfid equals b.ShelfId
                                      join c in db.Racks on aa.rackid equals c.RackId
                                       
                                      where az.PurchaseEntry == a.PurchaseEntry && ay.itemid == a.Item
                                      select new
                                      {
                                          ShelfNo =aa.shelfid,
                                          RackNo = aa.rackid,
                                          ShelfName = b.shelfName,
                                          RackName = c.RackName,
                                          StockIn = ay.qty,
                                      }).ToList()









                          }) ;
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string result = javaScriptSerializer.Serialize(PEItem);
            return Json(result);
        }
        public long getrackmcid(long mc, long? rack, long? shelf)
        {
            long rackmcid = 0;
            rackmcid = db.rackmaterialcentres.Where(o => o.mcid == mc && o.shelfid == (long)shelf && o.rackid == (long)rack).Select(o => o.rackmcid).SingleOrDefault();
            return rackmcid;
        }
        public long getshelfno(long rackmcid)
        {
            long shelfid = 0;
            shelfid = db.rackmaterialcentres.Where(o => o.rackmcid == rackmcid).Select(o => o.shelfid).SingleOrDefault();
            return shelfid;
        }
        public long? getrackno(long rackmcid)
        {
            long? rackid = 0;
            rackid = db.rackmaterialcentres.Where(o => o.rackmcid == rackmcid).Select(o => o.rackid).SingleOrDefault();
            return rackid;
        }
        public string getshelfname(long rackmcid)
        {
            string shelfname = "";
            var shelfid = getshelfno(rackmcid);
            shelfname = db.Shelves.Where(o => o.ShelfId == shelfid).Select(o => o.shelfName).FirstOrDefault();
            return shelfname;
        }
        public string getrackname(long rackmcid)
        {
            string rackname = "";
            var rackid = getrackno(rackmcid);
            rackname = db.Racks.Where(o => o.RackId == rackid).Select(o => o.RackName).FirstOrDefault();
            return rackname;
        }
        [HttpGet]
        public ActionResult GetPEBillSundry(long PurchaseEntryID)
        {
            var PEBs = (from a in db.PEBillSundrys
                        join c in db.BillSundrys on a.BillSundry equals c.BillSundryId
                        where a.PurchaseEntry == PurchaseEntryID
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
            return Json(PEBs);
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Edit Purchase Entry")]
        public ActionResult Edit(long? id)
        {
            var SuperUserEdit = db.EnableSettings.Where(a => a.EnableType == "SuperUserEdit").FirstOrDefault();
            var SuperUserEditvalue = SuperUserEdit != null ? SuperUserEdit.Status : Status.inactive;
            ViewBag.SuperUserEditvalue = SuperUserEditvalue;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var dummyvalue = db.DummyPEItems2.Where(a => a.PurchaseEntry == id).FirstOrDefault();
            if (dummyvalue != null)
            {
                ViewBag.itemAproval = 1;
            }
            else
            {
                ViewBag.itemAproval = 0;
            }
            var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
            var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;
            ViewBag.StockTrnsfrUpdate = brcheck;
            ViewBag.image = (from b in db.purchaseentrydocuments
                             join c in db.PurchaseEntrys on b.PurchaseId equals c.PurchaseEntryId
                             where c.PurchaseEntryId == id
                             select new quotationdocumentviewmodel
                             {
                                 qutid = b.purid,
                                 quotationID = b.PurchaseId,
                                 FileName = b.FileName,
                             }).ToList();
            var userpermission = User.IsInRole("All Purchase Entry");
            var UserId = User.Identity.GetUserId();
            PurchaseEntry PEentry = db.PurchaseEntrys.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.PurchaseEntryId == id).FirstOrDefault();
           
            var today = DateTime.Now;
            var editableDay = DateTime.Now;
            var tem = 0;
            var userEditDays = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.pedays).FirstOrDefault();
            var userEdit = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.id).FirstOrDefault();
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
                editableDay = today.AddMinutes(-Convert.ToDouble(userEditDays));
            }
            ViewBag.editdays = "True";

              if (SuperUserEditvalue == Status.active)
            {
                DateTime dt = System.DateTime.Now;
                var f = db.otpapproves.Where(o => o.entryid == id && o.purpose == "Purchase" && o.requestedby == UserId && o.expdate > dt && o.approvedby == UserId).FirstOrDefault();
                if (f != null)
                {
                    editableDay = editableDay.AddDays(-1000);
                }
            }
            if ((PEentry.PEDate - editableDay).TotalMinutes < 0 || tem == 1)
            {
                ViewBag.editdays = "False";
            }
            var EnableRackWise = db.EnableSettings.Where(a => a.EnableType == "RackWiseStock").FirstOrDefault();
            var RackCheck = EnableRackWise != null ? EnableRackWise.Status : Status.inactive;
            ViewBag.RackWiseStock = RackCheck;

            if (PEentry == null)
            {
                return NotFound();
            }
            Int64 cashier = Convert.ToInt64(PEentry.PECashier);
            Int64 customer = PEentry.Supplier;
            var use = db.Employees
                          .Select(s => new
                          {
                              ID = s.EmployeeId,
                              Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                          })
                          .ToList();
            ViewBag.Cashier = QkSelect.List(use, "ID", "Name");

            var supp = db.Suppliers
                       .Select(s => new
                       {
                           SupplierID = s.SupplierID,
                           SupplierDetails = s.SupplierID + " - " + s.SupplierName
                       }).ToList();
            ViewBag.Supp = QkSelect.List(supp, "SupplierID", "SupplierDetails");

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            List<SelectFormat> mcss;
            List<SelectFormat> serialisedJson;
            List<SelectFormat> serialisedJson2;
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            var mchkadditional = db.AdditionalMc.Where(a => a.UserId == UserId).FirstOrDefault();
            if (mcchk != null || mchkadditional != null)
            {
                //    Id = s.MCId,
                //    Name = s.MCName
                mcss = db.MCs.Where(s => s.MCId == PEentry.MaterialCenter).Select(s => new SelectFormat
                {
                    text = s.MCName,
                    id = s.MCId
                }).OrderBy(b => b.text).ToList().ToList();


                serialisedJson = db.MCs.Where(p => p.AssignedUser == UserId).Select(b => new SelectFormat
                {
                    text = b.MCName,
                    id = b.MCId
                }).OrderBy(b => b.text).ToList();
                serialisedJson2 = db.AdditionalMc.Where(p => p.UserId == UserId).Select(b => new SelectFormat
                {
                    text = b.McName,
                    id = b.McId,
                }).OrderBy(b => b.text).ToList();
                serialisedJson = mcss.Union(serialisedJson).ToList();
                serialisedJson = serialisedJson2.Union(serialisedJson).ToList();
                ViewBag.MC = QkSelect.List(serialisedJson, "id", "text");
                ViewBag.LastMc = serialisedJson.Select(a => a.id).FirstOrDefault();
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

            var EnableCurrency = db.EnableSettings.Where(a => a.EnableType == "EnableCurrency").FirstOrDefault();
            var CurrencyEnable = EnableCurrency != null ? EnableCurrency.Status : Status.inactive;
            ViewBag.EnableCurrency = CurrencyEnable;

            var Currency = db.CurrencyMasters
              .Select(s => new
              {
                  Id = s.Id,
                  Name = s.CurrencyCode
              }).ToList();
            ViewBag.CurrencyVal = QkSelect.List(Currency, "Id", "Name");


            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "PurchaseEntry").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true )
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaPEntry = db.EnableSettings.Where(a => a.EnableType == "MLAPEntry").FirstOrDefault();
            var MlaPEntrys = MlaPEntry != null ? MlaPEntry.Status : Status.inactive;
            ViewBag.MLAPEntry = MlaPEntrys;


            if (PEentry.PurchaseStatus == 0)
            {
                List<SelectListItem> pstat = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Open", Value = "0"
                },
                new SelectListItem {
                    Text = "Closed", Value = "1"
                }
              };
                ViewBag.PurchaseStatus = pstat;

            }
            else if(PEentry.PurchaseStatus == 1)
            {
                List<SelectListItem> pstat = new List<SelectListItem>() {
                new SelectListItem {
                    Text = "Closed", Value = "1"
                },
                new SelectListItem {
                    Text = "Open", Value = "0"
                }
              };
                ViewBag.PurchaseStatus = pstat;
            }
            else
            {
                List<SelectListItem> pstat = new List<SelectListItem>() {                   
                new SelectListItem {
                    Text = "Open", Value = "0"
                },
                new SelectListItem {
                    Text = "Closed", Value = "1"
                },
              };
                ViewBag.PurchaseStatus = pstat;
            }




            var CBill = "";
            var CType = "";
            var ConvertTran = db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "Purchase").FirstOrDefault();
            if (ConvertTran != null)
            {
                CType = ConvertTran.ConvertFrom + " No";
                if (ConvertTran.ConvertFrom == "POrder")
                {
                    CBill = db.PurchaseOrders.Where(a => a.PurchaseOrderId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "PQuote")
                {
                    CBill = db.PurchaseQuotations.Where(a => a.PQuotationId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "MOrder")
                {
                    CBill = db.MaterialRequisitions.Where(a => a.MaterialRequisitionId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
                if (ConvertTran.ConvertFrom == "MRNote")
                {
                    CBill = db.MaterialReceiveNotes.Where(a => a.MRId == ConvertTran.From).Select(a => a.BillNo).FirstOrDefault();
                }
            }

            PurchaseEntryViewModel vmodel = new PurchaseEntryViewModel();
            vmodel = (from b in db.PurchaseEntrys
                      join c in db.PEPayments on b.PurchaseEntryId equals c.PurchaseEntry
                      join d in db.Suppliers on b.Supplier equals d.SupplierID
                      join e in db.Contacts on d.Contact equals e.ContactID into cont
                      from e in cont.DefaultIfEmpty()
                      join f in db.HireDetails on new { f1 = b.PurchaseEntryId, f2 = "Purchase" }
                      equals new { f1 = f.Reference, f2 = f.Section } into hir
                      from f in hir.DefaultIfEmpty()
                      where b.PurchaseEntryId == id
                      select new PurchaseEntryViewModel
                      {
                          SupplierName = db.Suppliers.Where(a => a.SupplierID == b.Supplier).Select(a => a.SupplierCode + " - " + a.SupplierName).FirstOrDefault(),

                          PENo = b.PENo,
                          PEDate = b.PEDate,
                          BillNo = b.BillNo,
                          PECashier = b.PECashier,
                          SupplierType = b.SupplierType,
                          Supplier = b.Supplier,
                          PEDiscount = b.PEDiscount,
                          PEGrandTotal = b.PEGrandTotal,
                          PENote = b.PENote,
                          PETotal=b.PETaxAmount,
                          suppEmailId = e.EmailId,
                          PurchaseType = b.PurchaseType,
                          Remarks = b.Remarks,
                          PurchaseTypes = db.PurchaseTypes.ToList(),
                          MaterialCenter = b.MaterialCenter,
                          Branch = b.Branch,
                          Currency = b.Currency,
                          DConvertionRate = b.ConvertionRate,
                          convertBill = CBill,
                          convertFrom = CType,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          PurchaseHireType = b.PurType,
                          FromDate = f.StartDate,
                          ToDate = f.EndDate,
                          CrossHireType = f.HireType,
                          ReferenceNo=b.ReferenceNo,
                          requestpayment= b.requestpayment
                          // PEPaidAmount = c.PEPaidAmount,
                          // PEDueAmount = b.PEGrandTotal - c.PEPaidAmount
                      }).FirstOrDefault();

            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            ViewBag.preEntry = db.PurchaseEntrys.Where(a => a.PurchaseEntryId < id && (!MCList.Any() || MCArray.Contains(a.MaterialCenter)) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.PurchaseEntryId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.PurchaseEntrys.Where(a => a.PurchaseEntryId > id && (!MCList.Any() || MCArray.Contains(a.MaterialCenter)) && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.PurchaseEntryId).DefaultIfEmpty().Min();

            var mail = db.EnableSettings.Where(a => a.EnableType == "SaveAndMail").FirstOrDefault();
            ViewBag.CheckMail = mail != null ? mail.Status : Status.inactive;

            var ItemPriceInPurchase = db.EnableSettings.Where(a => a.EnableType == "ItemPriceInPurchase").FirstOrDefault();
            ViewBag.ItemPriceInPurchase = ItemPriceInPurchase != null ? ItemPriceInPurchase.Status : Status.inactive;

            var checkdata = chkInCompleteTransaction();

            var EnableJewellery = db.EnableSettings.Where(a => a.EnableType == "EnableJewellery").FirstOrDefault();
            var JewCheckable = EnableJewellery != null ? EnableJewellery.Status : Status.inactive;
            ViewBag.JewCheck = JewCheckable;
            _FinancialYear();

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var pos = db.EnableSettings.Where(a => a.EnableType == "POSInvoice").FirstOrDefault();
            var poscheck = pos != null ? pos.Status : Status.inactive;
            ViewBag.POS = poscheck;

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var EditPermission = User.IsInRole("Disable Purchase Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "PurchaseEntry", UserId);

            companySet();

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Purchase" && a.Status == Status.active).ToList();

            var hiretype = db.HireTypes
                 .Select(s => new
                 {
                     ID = s.HireTypeId,
                     Name = s.Name
                 })
                 .ToList();
            ViewBag.HiType = QkSelect.List(hiretype, "ID", "Name");

            ViewBag.DefMc = db.MCs.Where(a => a.MCName == "Hire").Select(a => a.MCId).FirstOrDefault();

            var DisCounPer = db.EnableSettings.Where(a => a.EnableType == "DiscountPercentage").FirstOrDefault();
            var DisPercheck = DisCounPer != null ? DisCounPer.Status : Status.inactive;
            ViewBag.DisPercheck = DisPercheck;

            //dummy table operations
            var DItem = db.DummyPEItems.Where(a => a.PurchaseEntry == id).FirstOrDefault();
            var PItem = db.PEItemss.Where(a => a.PurchaseEntry == id).FirstOrDefault();
            if (PItem == null && DItem != null)
            {
                var DItems = db.DummyPEItems.Where(a => a.PurchaseEntry == id).ToList();
                foreach (var arr in DItems)
                {
                    //add to se-item table
                    PEItems pItem = new PEItems();
                    pItem.ItemUnit = arr.ItemUnit;
                    pItem.ItemUnitPrice = arr.ItemUnitPrice;
                    pItem.ItemQuantity = arr.ItemQuantity;
                    pItem.ItemSubTotal = arr.ItemSubTotal;
                    pItem.ItemDiscount = arr.ItemDiscount;
                    pItem.ItemTax = arr.ItemTax;
                    pItem.ItemTaxAmount = arr.ItemTaxAmount;
                    pItem.ItemTotalAmount = arr.ItemTotalAmount;
                    pItem.itemNote = arr.itemNote;
                    pItem.PurchaseEntry = arr.PurchaseEntry;
                    pItem.Item = arr.Item;
                    pItem.ProjectId = arr.ProjectId;
                    pItem.TaskId = arr.TaskId;
                    db.PEItemss.Add(pItem);
                    db.SaveChanges();
                }

                db.DummyPEItems.RemoveRange(db.DummyPEItems.Where(a => a.PurchaseEntry == id));
                db.SaveChanges();
            }

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


            var ref1 = db.PurchaseEntrys
              .Select(s => new
              {
                  ID = s.Ref1,
                  Name = s.Ref1
              }).Distinct()
              .ToList().OrderBy(a => a.Name);
            ViewBag.VRef1 = QkSelect.List(ref1, "ID", "Name", vmodel.Ref1);

            var ref2 = db.PurchaseEntrys
             .Select(s => new
             {
                 ID = s.Ref2,
                 Name = s.Ref2
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef2 = QkSelect.List(ref2, "ID", "Name", vmodel.Ref2);

            var ref3 = db.PurchaseEntrys
             .Select(s => new
             {
                 ID = s.Ref3,
                 Name = s.Ref3
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef3 = QkSelect.List(ref3, "ID", "Name", vmodel.Ref3);

            var ref4 = db.PurchaseEntrys
             .Select(s => new
             {
                 ID = s.Ref4,
                 Name = s.Ref4
             }).Distinct()
             .ToList().OrderBy(a => a.Name);
            ViewBag.VRef4 = QkSelect.List(ref4, "ID", "Name", vmodel.Ref4);

            var ref5 = db.PurchaseEntrys
            .Select(s => new
            {
                ID = s.Ref5,
                Name = s.Ref5
            }).Distinct()
            .ToList().OrderBy(a => a.Name);
            ViewBag.VRef5 = QkSelect.List(ref5, "ID", "Name", vmodel.Ref5);


            if (rtype == "APP")
            {
                return View("App/Edit", vmodel);
            }
            else
            {
                return View(vmodel);
            }
        }


        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Purchase Entry")]
        public ActionResult UpdatePurchase(string[][] array, string[] purchasedata, PEBillSundryViewModel bsmodel, ICollection<BatchStockPViewModel> bstmodel, ICollection<RackStockPViewModel> bsrackData)
        {

            bool stat = false;
            string msg;
            var billNo = Convert.ToString(purchasedata[12]);
            Int64 purchaseEntryId = Convert.ToInt64(purchasedata[11]);
            PurchaseEntry PEentry = db.PurchaseEntrys.Find(purchaseEntryId);
            if (BillExist(Convert.ToString(purchasedata[12])) && Convert.ToString(purchasedata[12]) != PEentry.BillNo)
            {
                msg = "Invoice No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

            //    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

            var UserId = User.Identity.GetUserId();
            var CurrentDate = Convert.ToDateTime(System.DateTime.Now);
            long Branch = 0;
            Int64 purAcc = (long)db.companys.Select(a => a.PurchaseAccount).FirstOrDefault();

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            var Today = Convert.ToDateTime(System.DateTime.Now);
            if (BranchCheck == Status.active)
            {
                Branch = Convert.ToInt64(purchasedata[20]);
            }
            else
            {
                Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
            }

            var EnableCurrency = db.EnableSettings.Where(a => a.EnableType == "EnableCurrency").FirstOrDefault();
            ViewBag.EnableCurrency = EnableCurrency != null ? EnableCurrency.Status : Status.inactive;

            long Currency = 0;
            var ConRate = "";
            if (ViewBag.EnableCurrency == 0)
            {

                Currency = Convert.ToInt64(purchasedata[21]);
                ConRate = purchasedata[22];
            }
            else
            {
                Currency = db.CurrencyMasters.Where(a => a.editable == choice.No).Select(a => a.Id).FirstOrDefault();
                ConRate = db.CurrencyMasters.Where(a => a.editable == choice.No).Select(a => a.ConvertionRate).FirstOrDefault();
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
            var UserName = db.Users.Where(a => a.Id == UserId).Select(a => a.Name).First();
            var TaxAmount = Convert.ToDecimal(purchasedata[5]);
            var PEGrandTotal = Convert.ToDecimal(purchasedata[7]);
            var Purchaseamount = PEGrandTotal - TaxAmount;
            var subtotal = Convert.ToDecimal(purchasedata[8]);

            var EditPermission = User.IsInRole("Disable Purchase Edit After Approval");
            if (1==1)
            {

                if (purchasedata[30] != null)
                {
                    string str = purchasedata[30];
                    PurchaseHireType Stype = (PurchaseHireType)Enum.Parse(typeof(PurchaseHireType), str);
                    PEentry.PurType = Stype;
                }
                else
                {
                    PEentry.PurType = PurchaseHireType.Purchase;
                }
                PEentry.PEDate = DateTime.Parse(purchasedata[2], new CultureInfo("en-GB"));
                PEentry.PECashier = purchasedata[1] != "" ? Convert.ToInt64(purchasedata[1]) : 0;
                PEentry.Supplier = Convert.ToInt64(purchasedata[0]);
                PEentry.BillNo = Convert.ToString(purchasedata[12]);
                PEentry.PayType = "";//need change
                PEentry.PEItems = Convert.ToInt32(purchasedata[3]);
                PEentry.PEItemQuantity = Convert.ToDecimal(purchasedata[4]);
                PEentry.PESubTotal = Convert.ToDecimal(purchasedata[8]);
                PEentry.PETax = Convert.ToDecimal(purchasedata[9]);
                PEentry.PETaxAmount = TaxAmount;
                PEentry.PEDiscount = Convert.ToDecimal(purchasedata[6]);
                PEentry.PEGrandTotal = PEGrandTotal;
                PEentry.PENote = purchasedata[13];
                PEentry.Print = 1;
                PEentry.Status = 1;
                PEentry.Branch = Branch;
                PEentry.PurchaseType = Convert.ToInt64(purchasedata[17]);
                PEentry.Remarks = purchasedata[18];
                PEentry.MaterialCenter = MC;
                PEentry.Currency = Currency;
                PEentry.ConvertionRate = ConRate;
                PEentry.FCTotal = Convert.ToDecimal(purchasedata[23]);
                PEentry.PurchaseStatus = Convert.ToInt32(purchasedata[35]);


                var SupType = PEentry.SupplierType;
                if (purchasedata[15] == "1")
                {
                    PEentry.SupplierType = SupplierType.CashSale;
                }
                else
                {
                    PEentry.SupplierType = SupplierType.CreditSale;
                }

                PEentry.Ref1 = Convert.ToString(purchasedata[25]);
                PEentry.Ref2 = Convert.ToString(purchasedata[26]);
                PEentry.Ref3 = Convert.ToString(purchasedata[27]);
                PEentry.Ref4 = Convert.ToString(purchasedata[28]);
                PEentry.Ref5 = Convert.ToString(purchasedata[29]);
                PEentry.PurchaseAccount = purAcc;
                PEentry.ReferenceNo= Convert.ToString(purchasedata[36]);
                PEentry.requestpayment =(purchasedata[37]== "True") ?true:false;
                db.Entry(PEentry).State = EntityState.Modified;

                //To Update the quantity in Edit Mode(ItemTransaction Table)               

                var HireItem = db.HireDetails.Where(a => a.Reference == purchaseEntryId && a.Section == "Purchase").FirstOrDefault();
                if (HireItem != null)
                {
                    db.HireDetails.RemoveRange(db.HireDetails.Where(a => a.Reference == purchaseEntryId && a.Section == "Purchase"));
                    db.SaveChanges();
                }

                if (PEentry.PurType == PurchaseHireType.CrossHire)
                {
                    HireDetail HDetils = new HireDetail();
                    HDetils.StartDate = DateTime.Parse(purchasedata[31], new CultureInfo("en-GB"));
                    HDetils.EndDate = DateTime.Parse(purchasedata[32], new CultureInfo("en-GB"));
                    HDetils.Section = "Purchase";
                    HDetils.Reference = purchaseEntryId;
                    HDetils.HireType = Convert.ToInt64(purchasedata[33]);
                    db.HireDetails.Add(HDetils);
                    db.SaveChanges();
                }
                var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
                var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;
                brcheck = Status.inactive;
                if (brcheck == Status.active)
                {
                    DummyPEItem2 dItem = new DummyPEItem2();
                    db.DummyPEItems2.RemoveRange(db.DummyPEItems2.Where(a => a.PurchaseEntry == purchaseEntryId));

                    db.SaveChanges();
                }

                    var PEItem = db.PEItemss.Where(a => a.PurchaseEntry == purchaseEntryId).FirstOrDefault();
                if (PEItem != null)
                {
                    var PItems = db.PEItemss.Where(a => a.PurchaseEntry == purchaseEntryId).ToList();
                    foreach (var arr in PItems)
                    {
                        //add to dummy table
                        DummyPEItem dItem = new DummyPEItem();
                        dItem.ItemUnit = arr.ItemUnit;
                        dItem.ItemUnitPrice = arr.ItemUnitPrice;
                        dItem.ItemQuantity = arr.ItemQuantity;
                        dItem.ItemSubTotal = arr.ItemSubTotal;
                        dItem.ItemDiscount = arr.ItemDiscount;
                        dItem.ItemTax = arr.ItemTax;
                        dItem.ItemTaxAmount = arr.ItemTaxAmount;
                        dItem.ItemTotalAmount = arr.ItemTotalAmount;
                        dItem.itemNote = arr.itemNote;
                        dItem.PurchaseEntry = arr.PurchaseEntry;
                        dItem.Item = arr.Item;
                        dItem.ProjectId = arr.ProjectId;
                        dItem.TaskId = arr.TaskId;
                        db.DummyPEItems.Add(dItem);
                        db.SaveChanges();
                    }

                    db.PEItemss.RemoveRange(db.PEItemss.Where(a => a.PurchaseEntry == purchaseEntryId));
                    db.SaveChanges();
                }

                var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                var ProjChks = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;



                ////// add to PEItem
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

                dtItem.Columns.Add("ProjectId");
                dtItem.Columns.Add("TaskId");


                dtItem.Columns.Add("itemNote");
                dtItem.Columns.Add("PurchaseEntry");
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
                    if (ProjChks == Status.active)
                    {
                        dr["ProjectId"] = Convert.ToInt64(arr[29]);
                        dr["TaskId"] = Convert.ToInt64(arr[30]);
                        dr["itemNote"] = Convert.ToString(arr[31].Replace("\n", "<br />"));
                    }
                    else
                    {
                        dr["ProjectId"] = 0;
                        dr["TaskId"] = 0;
                        dr["itemNote"] = Convert.ToString(arr[29].Replace("\n", "<br />"));
                    }
                    dr["PurchaseEntry"] = purchaseEntryId;
                    dr["Item"] = Convert.ToInt32(arr[0]);

                    dtItem.Rows.Add(dr);
                    var itemz = Convert.ToInt64(arr[0]);
                    var it = db.Items.Find(itemz);
                    if (it.accountid != null && it.accountid != 0)
                    {
                        var pedate = db.PurchaseEntrys.Find(purchaseEntryId).PEDate;
                        com.adddummyAccountTrasaction((Convert.ToDecimal(arr[11]) == null) ? 0 : Convert.ToDecimal(arr[11]), 0, (long)it.accountid, "Purchase", purchaseEntryId, DC.Debit, pedate);
                    }

                    var chkbundle = db.ItemBundles.Where(a => a.mainItem == itemz).Select(a => a.ItemBundleId).FirstOrDefault();
                    if (chkbundle > 0)
                    {
                        var bunQuan = Convert.ToDecimal(arr[2]);
                        var itemBundle = (from g in db.ItemBundles
                                          join b in db.Items on g.mainItem equals b.ItemID
                                          where b.ItemID == itemz
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
                            dbu["ItemDiscount"] = itemz;
                            dbu["ItemTax"] = itemtax;
                            dbu["ItemTaxAmount"] = taxamt;
                            dbu["ItemTotalAmount"] = totamt;
                            if (ProjChks == Status.active)
                            {
                                dbu["ProjectId"] = Convert.ToInt64(arr[29]);
                                dbu["TaskId"] = Convert.ToInt64(arr[30]);
                            }
                            else
                            {
                                dbu["ProjectId"] = 0;
                                dbu["TaskId"] = 0;
                            }
                            dbu["itemNote"] = "-:{Bundle_Item}";
                            dbu["PurchaseEntry"] = purchaseEntryId;
                            dbu["Item"] = bu.Item;
                            dtItem.Rows.Add(dbu);
                        }
                    }

                }

              
           
                    SqlParameter parameter = new SqlParameter("@TableType", dtItem);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "TableTypePEItems";
                //// execute sp sql 
                string sql = "";
                if (brcheck == Status.active)
                {
                    sql = String.Format("EXEC {0} {1};", "SP_InsertDummyPeItems", "@TableType");
                    var updatetwo = db.ApprovalUpdates.Where(o => o.TransEntry == purchaseEntryId && o.Type == "PurchaseEntry").ToList();
                    
                    foreach(var up in updatetwo)
                    {
                        ApprovalUpdatestwp a = new ApprovalUpdatestwp
                        {
                        ApprovalStatus=ApprovalStatus.PendingApproval,
                        ApprovedBy=UserId,
                        CreatedDate=System.DateTime.Now,
                        Note=up.Note,
                        RequestBy=UserId,
                        Status =up.Status,
                        TransEntry=up.TransEntry,
                        Type=up.Type

                        
                        };
                        db.ApprovalUpdatestwp.Add(a);
                        db.SaveChanges();
                    }

                    db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(o => o.TransEntry == purchaseEntryId && o.Type == "PurchaseEntry"));
                    db.SaveChanges();
                }
                else
                {
                     sql = String.Format("EXEC {0} {1};", "SP_InsertPEItems", "@TableType");
                }
                    //// execute sql 
                    var ret = db.Database.ExecuteSqlRaw(sql, parameter);
                
                    if (ret > 0)
                {
                    db.DummyPEItems.RemoveRange(db.DummyPEItems.Where(a => a.PurchaseEntry == purchaseEntryId));
                    db.SaveChanges();
                }

                // batch stock
                var PEBst = db.BatchStocks.Where(a => a.Reference == purchaseEntryId && a.Type == "Purchase").FirstOrDefault();
                if (PEBst != null)
                {
                    db.BatchStocks.RemoveRange(db.BatchStocks.Where(a => a.Reference == purchaseEntryId && a.Type == "Purchase"));
                    db.SaveChanges();
                }
                if (bstmodel != null)
                {
                    foreach (var bst in bstmodel)
                    {
                        if (bst.StockIn != 0)
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
                            decimal bStockIn = 0;
                            if (bst.Unit == bst.Priunit)
                            {
                                bStockIn = bst.StockIn * bst.cfactor;
                            }
                            BatchStock Btst = new BatchStock();
                            Btst.BatchNo = bst.BatchNo;
                            Btst.Item = bst.Item;
                            Btst.Unit = bst.Unit;
                            Btst.Cost = bst.Cost;
                            Btst.StockIn = bStockIn;
                            Btst.Order = bst.Order;
                            Btst.EXP = exp;
                            Btst.MFG = mfg;
                            Btst.Reference = purchaseEntryId;
                            Btst.Type = "Purchase";

                            Btst.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            Btst.Date = DateTime.Parse(purchasedata[2], new CultureInfo("en-GB"));


                            db.BatchStocks.Add(Btst);
                        }
                    }
                    db.SaveChanges();
                }
                var PERack = db.shelfstockmovements.Where(a => a.referenceid == purchaseEntryId && a.purpose == "Purchase").FirstOrDefault();
                if (PERack != null)
                {
                    db.shelfstockmovements.RemoveRange(db.shelfstockmovements.Where(a => a.referenceid == purchaseEntryId && a.purpose == "Purchase"));
                    db.SaveChanges();
                }
                //rackstock
                if (bsrackData != null)
                {
                    foreach (var bst in bsrackData)
                    {
                        if (bst.StockOut != 0)
                        {

                            decimal bStockIn = 0;

                            shelfstockmovement Btst = new shelfstockmovement();
                            Btst.purpose = "Purchase";
                            Btst.itemid = bst.Item;
                            Btst.unitid = (long)bst.Unit;
                            Btst.rackmciid = (long)com.getrackmcid(MC, bst.RackNo, bst.ShelfNo);
                            Btst.qty = bst.StockOut;

                            Btst.referenceid = purchaseEntryId;


                            Btst.createddate = Today;
                            Btst.createdby = UserId;

                            db.shelfstockmovements.Add(Btst);
                        }
                    }
                    db.SaveChanges();
                }
                // bill sundry
                var PEBs = db.PEBillSundrys.Where(a => a.PurchaseEntry == purchaseEntryId).FirstOrDefault();
                if (PEBs != null)
                {
                    db.PEBillSundrys.RemoveRange(db.PEBillSundrys.Where(a => a.PurchaseEntry == purchaseEntryId));
                    db.SaveChanges();
                }
                if (bsmodel.pebsundrys != null)
                {

                    string bsResult = string.Empty;
                    DataTable BsEntry = new DataTable();
                    BsEntry.Columns.Add("PurchaseEntry");
                    BsEntry.Columns.Add("BillSundry");
                    BsEntry.Columns.Add("BsValue");
                    BsEntry.Columns.Add("AmountType");
                    BsEntry.Columns.Add("BsType");
                    BsEntry.Columns.Add("BsAmount");

                    foreach (var bs in bsmodel.pebsundrys)
                    {
                        DataRow drw = BsEntry.NewRow();
                        drw["PurchaseEntry"] = purchaseEntryId;
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
                    parameter1.TypeName = "TableTypePEBillSundry";
                    //// execute sp sql 
                    string sql1 = String.Format("EXEC {0} {1};", "SP_InsertPEBillSundry", "@TableType");
                    //// execute sql 
                    db.Database.ExecuteSqlRaw(sql1, parameter1);
                    //-------------------------------------
                }


                decimal amount = Convert.ToDecimal(purchasedata[10]);
                var date = DateTime.Parse(purchasedata[2], new CultureInfo("en-GB"));
                Int64 cashAccId = db.Accountss.Where(a => a.Group == 9).Select(a => a.AccountsID).FirstOrDefault();
                Int64 suppAccID = db.Suppliers.Where(a => a.SupplierID == PEentry.Supplier).Select(a => a.Accounts).FirstOrDefault();
                Int64 purAccId = purAcc;// db.Accountss.Where(a => a.Group == 16).Select(a => a.AccountsID).FirstOrDefault();
                Int64 VATInput = db.Accountss.Where(a => a.Group == 24 && a.Name == "VAT Input").Select(a => a.AccountsID).SingleOrDefault();

                // walkin customer
                if (purchasedata[15] == "1")
                {
                    //AccountsTransaction
                    amount = PEGrandTotal;
                }

                //----------new added----------------
                if (purchasedata[15] == "1")//cash
                {
                    deleteAndUpdateTrans(purchaseEntryId, purchasedata, amount, suppAccID, cashAccId, BranchID, UserId);
                    changePayBill(purchaseEntryId);
                }
                if (purchasedata[15] == "0")//credit
                {
                    if (SupType == SupplierType.CashSale)//previous cash
                    {
                        deleteAndUpdateTrans(purchaseEntryId, purchasedata, amount, suppAccID, cashAccId, BranchID, UserId);
                    }
                    if (SupType == SupplierType.CreditSale)//previous credit
                    {

                        decimal sumTran = db.PETransactions.Where(a => a.PurchaseEntry == purchaseEntryId).FirstOrDefault() != null ? (decimal?)db.PETransactions.Where(a => a.PurchaseEntry == purchaseEntryId).Select(a => a.PEPayAmount).Sum() ?? 0 : 0;
                        if (sumTran > PEGrandTotal)
                        {
                            var chkpay = db.PaymentBills.Where(a => a.InvoiceNo == purchaseEntryId && a.BillType == "Purchase" && a.Type == "Against Reference").ToList();
                            if (chkpay != null)
                            {
                                var payamount = sumTran - PEGrandTotal;
                                amount = PEGrandTotal;
                                decimal TotAmt = PEGrandTotal;
                                foreach (var pbill in chkpay)
                                {
                                    TotAmt = TotAmt - pbill.Amount;
                                    if (TotAmt < 0 && pbill.Amount > payamount)
                                    {
                                        var reamt = pbill.Amount - payamount;


                                        PaymentBill paybillz = db.PaymentBills.Find(pbill.PaymentBillId);
                                        paybillz.Amount = reamt;
                                        db.Entry(paybillz).State = EntityState.Modified;
                                        db.SaveChanges();

                                        PaymentBill paybill = new PaymentBill();
                                        paybill.InvoiceNo = pbill.InvoiceNo;
                                        paybill.NewRefName = pbill.NewRefName;
                                        paybill.Payment = Convert.ToInt64(pbill.Payment);
                                        paybill.BillType = null;
                                        paybill.Amount = payamount;
                                        paybill.Type = "New Reference";
                                        paybill.Status = Status.active;

                                        db.PaymentBills.Add(paybill);
                                        db.SaveChanges();

                                    }
                                }
                                updatePepayment(purchaseEntryId, purchasedata, amount, BranchID, 0);

                            }
                            else
                            {
                                deleteAndUpdateTrans(purchaseEntryId, purchasedata, amount, suppAccID, cashAccId, BranchID, UserId);
                            }
                        }
                        else
                        {
                            updatePepayment(purchaseEntryId, purchasedata, amount, BranchID, 1);
                        }

                    }
                }
                bool delete = false;
                bool deletepay = false;
                if (brcheck == Status.active)
                {
                  delete = com.DeleteAlldummyAccountTransaction("Purchase", purchaseEntryId);
                    deletepay = com.DeleteAlldummyAccountTransaction("Purchase Payment", purchaseEntryId);
                    delete = com.DeleteAllAccountTransaction("Purchase", purchaseEntryId);
                    deletepay = com.DeleteAllAccountTransaction("Purchase Payment", purchaseEntryId);

                }
                else
                {
                    delete = com.DeleteAllAccountTransaction("Purchase", purchaseEntryId);
                deletepay = com.DeleteAllAccountTransaction("Purchase Payment", purchaseEntryId);
                }

                //bill sundry account
                var Gtotal = PEGrandTotal;
                decimal deductions = 0;
                if (bsmodel.pebsundrys != null)
                {
                    foreach (var bs in bsmodel.pebsundrys)
                    {
                        var ChkAcc = db.BillSundrys.Where(a => a.BillSundryId == bs.BillSundry).FirstOrDefault();
                        if (ChkAcc.PAccount != null && ChkAcc.PAccount != 0)
                        {
                            var bsamount =(bs.BsAmount==null)?0:(decimal)bs.BsAmount;
                            if (ChkAcc.BSType == 0)//additive
                            {
                                Purchaseamount = Purchaseamount - bsamount;
                                if (brcheck == Status.active)
                                {
                                    com.adddummyAccountTrasaction((bs.BsAmount == null) ? 0 : (decimal)bs.BsAmount, 0, (long)ChkAcc.PAccount, "Purchase", purchaseEntryId, DC.Debit, date);
                                }
                                else
                                {
                                    com.addAccountTrasaction((bs.BsAmount == null) ? 0 : (decimal)bs.BsAmount, 0, (long)ChkAcc.PAccount, "Purchase", purchaseEntryId, DC.Debit, date);
                                }
                            }
                            else //substract
                            {
                                Purchaseamount = Purchaseamount + bsamount;
                                if(brcheck==Status.active)
                                {
                                    com.adddummyAccountTrasaction(0, (bs.BsAmount == null) ? 0 : (decimal)bs.BsAmount, (long)ChkAcc.PAccount, "Purchase", purchaseEntryId, DC.Credit, date);

                                }
                                else
                                {
                                    com.addAccountTrasaction(0, (bs.BsAmount == null) ? 0 : (decimal)bs.BsAmount, (long)ChkAcc.PAccount, "Purchase", purchaseEntryId, DC.Credit, date);

                                }
                            }
                        }

                        else
                        {
                            var bsamount = bs.BsAmount == null ? 0 : (decimal)bs.BsAmount;
                            if (ChkAcc.BSType == 0)//additive
                            {
                                deductions = deductions - bsamount;
                            }
                            else //substract
                            {
                                deductions = deductions + bsamount;
                            }
                        }

                    }
                }
                foreach (var ar in array)
                {
                    var ItemId = Convert.ToInt64(ar[0]);
                    var it = db.Items.Find(ItemId);

                    if (it.accountid != null && it.accountid != 0)
                    {
                        var pedate = db.PurchaseEntrys.Find(purchaseEntryId).PEDate;
                        if (brcheck == Status.active)
                        {
                            var peitem = db.DummyPEItems2.Where(o => o.Item == ItemId && o.PurchaseEntry == purchaseEntryId).Select(o => o.ItemTotalAmount).Sum();
                            Purchaseamount = Purchaseamount - peitem;
                            com.adddummyAccountTrasaction((peitem == null) ? 0 : peitem, 0, (long)it.accountid, "Purchase", purchaseEntryId, DC.Debit, pedate);

                        }
                        else
                        {
                            var peitem = db.PEItemss.Where(o => o.Item == ItemId && o.PurchaseEntry == purchaseEntryId).Select(o => o.ItemTotalAmount).Sum();
                            Purchaseamount = Purchaseamount - peitem;
                            com.addAccountTrasaction((peitem == null) ? 0 : peitem, 0, (long)it.accountid, "Purchase", purchaseEntryId, DC.Debit, pedate);

                        }
                    }
                }
                if (brcheck == Status.active)
                {
                    ///add trasaction to purchase account
                    if (PEentry.PurchaseType != 3)
                        com.adddummyAccountTrasaction(Purchaseamount, 0, purAccId, "Purchase", purchaseEntryId, DC.Debit, date);
                    else
                        com.adddummyAccountTrasaction(PEentry.PESubTotal - deductions, 0, purAccId, "Purchase", purchaseEntryId, DC.Debit, date);

                    //add purchase trasaction 
                    com.adddummyAccountTrasaction(0, PEGrandTotal, suppAccID, "Purchase", purchaseEntryId, DC.Credit, date);
                    // add vat input in account transaction
                    if (TaxAmount > 0 && PEentry.PurchaseType != 3)
                        com.adddummyAccountTrasaction(TaxAmount, 0, VATInput, "Purchase", purchaseEntryId, DC.Debit, date);

                    if (Convert.ToDecimal(purchasedata[10]) > 0 || purchasedata[15] == "1")
                    {
                        //if payment
                        com.adddummyAccountTrasaction(amount, 0, suppAccID, "Purchase Payment", purchaseEntryId, DC.Debit, date);
                        com.adddummyAccountTrasaction(0, amount, cashAccId, "Purchase Payment", purchaseEntryId, DC.Credit, date);
                    }
                }
                else
                {
                    ///add trasaction to purchase account
                    if (PEentry.PurchaseType != 3)
                        com.addAccountTrasaction(Purchaseamount, 0, purAccId, "Purchase", purchaseEntryId, DC.Debit, date);
                    else
                        com.addAccountTrasaction(PEentry.PESubTotal - deductions, 0, purAccId, "Purchase", purchaseEntryId, DC.Debit, date);

                    //add purchase trasaction 
                    com.addAccountTrasaction(0, PEGrandTotal, suppAccID, "Purchase", purchaseEntryId, DC.Credit, date);
                    // add vat input in account transaction
                    if (TaxAmount > 0 && PEentry.PurchaseType != 3)
                        com.addAccountTrasaction(TaxAmount, 0, VATInput, "Purchase", purchaseEntryId, DC.Debit, date);

                    if (Convert.ToDecimal(purchasedata[10]) > 0 || purchasedata[15] == "1")
                    {
                        //if payment
                        com.addAccountTrasaction(amount, 0, suppAccID, "Purchase Payment", purchaseEntryId, DC.Debit, date);
                        com.addAccountTrasaction(0, amount, cashAccId, "Purchase Payment", purchaseEntryId, DC.Credit, date);
                    }
                }
                com.addlog(LogTypes.Updated, UserId, "PurchaseEntry", "PurchaseEntrys", findip(), purchaseEntryId, "Successfully Updated Purchase Entry");

                //---------------------






                //            let img = db.ItemImages.Where(im => im.ItemID == a.Item).Select(im => new { im.FileName, im.Status, im.ItemImageID }).ToList()
                //            where a.PurchaseEntry == purchaseEntryId && a.itemNote != "-:{Bundle_Item}"
                //                Id = b.ItemID,
                //                ItemUnitPrice = a.ItemUnitPrice,
                //                ItemQuantity = a.ItemQuantity,
                //                ItemSubTotal = a.ItemSubTotal,
                //                ItemTax = a.ItemTax,
                //                ItemNote = a.itemNote,
                //                ItemTaxAmount = a.ItemTaxAmount,
                //                ItemTotalAmount = a.ItemTotalAmount,
                //                ItemCode = b.ItemCode,
                //                ItemName = b.ItemName,
                //                ItemPrice = b.SellingPrice,
                //                Barcode = b.Barcode,
                //                ItemUnit = c.ItemUnitName,
                //                PartNumber = b.PartNumber,
                //                PNoStatus = PartNoCheck,
                //                CBM = d.CBM,
                //                Weight = d.Weight,
                //                img = img,
                //                KeepStock = b.KeepStock,
                //                bundle = (from ab in db.PEItemss


                //                          let bimg = db.ItemImages.Where(bim => bim.ItemID == ab.Item).Select(bim => new { bim.FileName, bim.Status, bim.ItemImageID }).ToList()
                //                          where ab.PurchaseEntry == purchaseEntryId && ab.itemNote == "-:{Bundle_Item}"
                //                          && b.ItemID == ab.ItemDiscount
                //                              Id = bb.ItemID,
                //                              ItemUnitPrice = ab.ItemUnitPrice,
                //                              ItemQuantity = ab.ItemQuantity,
                //                              ItemSubTotal = ab.ItemSubTotal,
                //                              ItemNote = "",
                //                              ItemTax = ab.ItemTax,
                //                              ItemTaxAmount = ab.ItemTaxAmount,
                //                              ItemTotalAmount = ab.ItemTotalAmount,

                //                              ItemCode = bb.ItemCode,
                //                              ItemName = bb.ItemName,
                //                              ItemUnit = eb.ItemUnitName,
                //                              PartNumber = bb.PartNumber,
                //                              PNoStatus = PartNoCheck,
                //                              CBM = dd.CBM,
                //                              Weight = dd.Weight,
                //                              img = bimg,


                //                              KeepStock = bb.KeepStock,

                //                              ab.Item,
                //                              ItemDiscount = 0,
                //                              ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                //                              bb.ItemUnitID,
                //                              bb.SubUnitId,
                //                              bb.ItemArabic,

                //Approved By
                var empuser = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                var chkapp = db.Approvals.Where(a => a.EmployeeId == empuser && a.TransEntry == purchaseEntryId && a.Type == "PurchaseEntry").FirstOrDefault();
                var MrnPO = db.Approvals.Where(a => a.TransEntry == purchaseEntryId && a.Type == "PurchaseEntry").FirstOrDefault();
                if (MrnPO != null)
                {
                    if (chkapp != null)
                    {
                        db.Approvals.RemoveRange(db.Approvals.Where(a => a.EmployeeId != empuser && a.TransEntry == purchaseEntryId && a.Type == "PurchaseEntry"));
                        db.SaveChanges();
                    }
                    else
                    {
                        db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == purchaseEntryId && a.Type == "PurchaseEntry"));
                        db.SaveChanges();
                    }
                }
                var Appby = Convert.ToString(purchasedata[24]);
                if (Appby != null && Appby != "")
                {
                    long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                    Approval approval = new Approval();
                    foreach (var emp in Approve)
                    {
                        approval.TransEntry = purchaseEntryId;
                        approval.Type = "PurchaseEntry";
                        approval.EmployeeId = emp;
                        db.Approvals.Add(approval);
                        db.SaveChanges();
                    }
                }
            }
            string action = purchasedata[14];
            var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
            var barcode = enable != null ? enable.Status : Status.inactive;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

            if (action == "print")
            {
                var conv = db.ConvertTransactionss.Any(u => u.To == purchaseEntryId);
                List<ConvertTransactionsViewModel> ConvExt = new List<ConvertTransactionsViewModel>();
                if (conv)
                {
                    List<string> ExList = new List<string>();
                    List<ConvertTransactions> ExtList = new List<ConvertTransactions>();
                    ExtList = ExtNumDetails((long)purchaseEntryId, ExtList);
                    var Extended = ExtList.Select(z => z.To).ToList();
                    Int32 count = 0;


                    var ConvModel = (from a in db.ConvertTransactionss
                                     join b in db.PurchaseEntrys on a.To equals b.PurchaseEntryId into primary
                                     from b in primary.DefaultIfEmpty()
                                     where Extended.Contains(a.To)
                                     select new ConvertTransactionsViewModel
                                     {
                                         ConvertFrom = (a.ConvertFrom == "PurchaseExtend") ? "Purchase" : a.ConvertFrom,
                                         Id = b.PurchaseEntryId,
                                         BillNo = b.BillNo,
                                         CreatedDate = a.CreatedDate,
                                         From = a.From
                                     }).OrderBy(b => b.CreatedDate).ToList();

                    var parent = ExtList.Where(x => x.From == 0).Select(y => y.To).FirstOrDefault();
                    ConvertTransactionsViewModel parentvm = new ConvertTransactionsViewModel();
                    parentvm.BillNo = db.PurchaseEntrys.Where(x => x.PurchaseEntryId == parent).Select(y => y.BillNo).FirstOrDefault();
                    parentvm.Id = db.PurchaseEntrys.Where(x => x.PurchaseEntryId == parent).Select(y => y.PurchaseEntryId).FirstOrDefault();
                    parentvm.ConvertFrom = "PurchaseExtend";
                    ConvModel.Add(parentvm);
                    ConvExt = ConvModel.OrderBy(x => x.Id).ToList();

                    var str = ConvExt.Find(c => c.Id == purchaseEntryId);
                    ConvExt.Remove(str);
                }
                //field mapping
                var fmapp = db.FieldMappings.Where(a => a.Section == "Purchase" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();


                var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

                var PurchaseData = com.PurchaseData(purchaseEntryId, InPrintItemCode, PartNoCheck, TimeOut, ComHeadCheck);
                var item = PurchaseData.pdfItem.ToList();
                var summary = PurchaseData;
                var billsundry = PurchaseData.billsundry.ToList();

                var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
                var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
                var def = (PriLay == Status.active) ? Convert.ToInt64(purchasedata[34]) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                def = def == 0 ? 1 : def;
                var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, barcode, layout, fmapp, purid = purchaseEntryId } };
            }
            else if (action == "sendmail")
            {
                SendMail sm = new SendMail();
                MailMessage message = new MailMessage();
                string ToMail = purchasedata[16];
                string CcMail = "";
                string InvoiceNo = "_PurchaseEntry_" + PEentry.BillNo;

                var em = db.EmailTemplates.Where(a => a.Head == "PurchaseEntry").FirstOrDefault();
                if (em != null)
                {
                    message.Subject = em.Subject;
                    message.Body = em.EmailBody;
                }
                else
                {
                    message.Subject = "Purchase Entry";
                    message.Body = "<p>DEAR SIR</p><p> Thank you for Contacting.</p>" +
                        " <p>we are enclosing our purchase entry for the items / services as requested by you during our discussions.<br/></p> " +
                        " <p>Looking forward to hear from you.</p>";
                }
                sm.SendPdfMail(generatePdf(purchaseEntryId), ToMail, CcMail, InvoiceNo, message);


                msg = "Successfully Updated Purchase Entry.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, purid = purchaseEntryId } };

            }
            else
            {
                msg = "Successfully Updated Purchase Entry.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, barcode, purid = purchaseEntryId } };
            }

            //        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public void updatePepayment(long purchaseEntryId, string[] purchasedata, decimal amount, long BranchID, int chk)
        {
            var PEGrandTotal = Convert.ToDecimal(purchasedata[7]);
            PEPayment PEpay = db.PEPayments.Where(a => a.PurchaseEntry == purchaseEntryId).FirstOrDefault();
            PEpay.SupplierId = Convert.ToInt64(purchasedata[0]);
            PEpay.PEDate = DateTime.Parse(purchasedata[2], new CultureInfo("en-GB"));
            PEpay.PEBillAmount = PEGrandTotal;

            if (chk == 0)
            {
                if (purchasedata[15] == "1")
                {
                    PEpay.PEPaidAmount = PEGrandTotal;
                }
                else
                {
                    PEpay.PEPaidAmount = amount != 0 ? amount : Convert.ToDecimal(purchasedata[10]);
                }
            }

            PEpay.CreatedBranch = Convert.ToInt32(BranchID);
            PEpay.PECreatedDate = Convert.ToDateTime(System.DateTime.Now);
            PEpay.Status = 1;
            db.Entry(PEpay).State = EntityState.Modified;
            db.SaveChanges();
        }

        public void deleteAndUpdateTrans(long purchaseEntryId, string[] purchasedata, decimal amount, long suppAccID, long cashAccId, long BranchID, string UserId)
        {
            var PEGrandTotal = Convert.ToDecimal(purchasedata[7]);
            var date = DateTime.Parse(purchasedata[2], new CultureInfo("en-GB"));


            PEPayment PEpay = db.PEPayments.Where(a => a.PurchaseEntry == purchaseEntryId).FirstOrDefault();
            PEpay.SupplierId = Convert.ToInt64(purchasedata[0]);
            PEpay.PEDate = DateTime.Parse(purchasedata[2], new CultureInfo("en-GB"));
            PEpay.PEEntryDate = Convert.ToDateTime(System.DateTime.Now);
            PEpay.PEBillAmount = PEGrandTotal;

            if (purchasedata[15] == "1")
            {
                PEpay.PEPaidAmount = PEGrandTotal;
            }
            else
            {
                PEpay.PEPaidAmount = amount != 0 ? amount : Convert.ToDecimal(purchasedata[10]);
            }

            PEpay.CreatedBranch = Convert.ToInt32(BranchID);
            PEpay.CreatedUserId = UserId;
            PEpay.PECreatedDate = Convert.ToDateTime(System.DateTime.Now);
            PEpay.Status = 1;

            db.Entry(PEpay).State = EntityState.Modified;

            db.PETransactions.RemoveRange(db.PETransactions.Where(a => a.PurchaseEntry == purchaseEntryId));
            db.Payments.RemoveRange(db.Payments.Where(a => a.Reference == purchaseEntryId && a.RefType == "Purchase"));
            db.SaveChanges();

            if (amount > 0 || Convert.ToDecimal(purchasedata[10]) > 0 || purchasedata[15] == "1")
            {
                //PETransaction
                PETransaction PEtran = new PETransaction();

                var Remark = "Direct Payment From Purchase Entry";
                long payid;
                PEtran.SupplierId = Convert.ToInt64(purchasedata[0]);
                PEtran.PEPayDate = date;
                if (purchasedata[15] == "1")
                {
                    PEtran.PEPayAmount = amount;
                    payid = com.addPayment(date, cashAccId, suppAccID, amount, amount, amount, Remark, UserId, BranchID, purchaseEntryId);
                }
                else
                {
                    amount = Convert.ToDecimal(purchasedata[10]);
                    PEtran.PEPayAmount = amount;
                    payid = com.addPayment(date, cashAccId, suppAccID, amount, amount, amount, Remark, UserId, BranchID, purchaseEntryId);
                }
                PEtran.PaymentId = payid;
                PEtran.PECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                PEtran.CreatedBranch = Convert.ToInt32(BranchID);
                PEtran.CreatedUserId = UserId;
                PEtran.Status = 1;
                PEtran.PurchaseEntry = purchaseEntryId;

                db.PETransactions.Add(PEtran);
                db.SaveChanges();

            }
        }
        public void changePayBill(long purchaseEntryId)
        {
            //receipt bill changes
            var chkrec = db.PaymentBills.Where(a => a.InvoiceNo == purchaseEntryId && a.BillType == "Purchase").ToList();
            if (chkrec != null)
            {
                db.PaymentBills.Where(a => a.InvoiceNo == purchaseEntryId && a.BillType == "Purchase").ToList().ForEach(a => a.Type = "New Reference");
                db.SaveChanges();
            }
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Download Purchase Entry")]
        public ActionResult Download(long id)
        {
            var Data = db.PurchaseEntrys.Where(s => s.PurchaseEntryId == id).FirstOrDefault();
            var supname = db.Suppliers.Where(s => s.SupplierID == Data.Supplier).Select(a => a.SupplierName).FirstOrDefault();

            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = Data.BillNo;

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), "inactive");
            return File(ms, "application/pdf", "Purchase Entry" + "-" + supname + "-" + billno + ".pdf");

        }
        public StringBuilder generatePdf(long purchaseEntryId)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

            var PurchaseData = com.PurchaseData(purchaseEntryId, InPrintItemCode, PartNoCheck, TimeOut);
            var item = PurchaseData.pdfItem.ToList();
            var summary = PurchaseData;
            var billsundry = PurchaseData.billsundry.ToList();


            return com.generatepdf(purchaseEntryId, summary, item, billsundry, "Purchase");
        }


        //                   where a.PurchaseEntryId == purchaseEntryId
        //                       PartyName = b.SupplierName,
        //                       BillNo = a.BillNo,
        //                       Date = a.PEDate,
        //                       Note = a.PENote,
        //                       Cashier = d.FirstName + " " + d.MiddleName + " " + d.LastName,
        //                       Discount = a.PEDiscount,

        //                       GrandTotal = a.PEGrandTotal,
        //                       Paid = e.PEPaidAmount,
        //                       Balance = a.PEGrandTotal - e.PEPaidAmount,
        //                       SubTotal = a.PESubTotal,
        //                       TaxAmount = a.PETaxAmount,
        //                       c.Address,
        //                       c.City,
        //                       c.State,
        //                       c.Country,
        //                       c.Zip,
        //                       Email = c.EmailId,
        //                       Phone = c.Phone,
        //                       Mobile = c.Mobile,
        //                       TRN = b.TaxRegNo,
        //                       TermsCondition = a.PENote,
        //                       BillId = a.PENo,
        //                       a.Remarks,
        //                       Currency = a.Currency,
        //                       ConvertionRate = a.ConvertionRate,
        //                       FCTotal = a.FCTotal,

        //                 where b.PurchaseEntry == purchaseEntryId && b.itemNote != "-:{Bundle_Item}"
        //                     ItemUnitPrice = b.ItemUnitPrice,
        //                     ItemQuantity = b.ItemQuantity,
        //                     ItemSubTotal = b.ItemSubTotal,
        //                     ItemNote = b.itemNote,
        //                     ItemTax = b.ItemTax,
        //                     ItemTaxAmount = b.ItemTaxAmount,
        //                     ItemTotalAmount = b.ItemTotalAmount,
        //                     ItemID = b.Item,
        //                     bundleitem = (from ab in db.PEItemss
        //                                   where ab.PurchaseEntry == purchaseEntryId && ab.itemNote == "-:{Bundle_Item}"
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











        [QkAuthorize(Roles = "Dev,Delete Purchase Entry")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Delete
            var userpermission = User.IsInRole("All Purchase Entry");
            var UserId = User.Identity.GetUserId();
            PurchaseEntry PEen = db.PurchaseEntrys.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.PurchaseEntryId == id).FirstOrDefault();

            if (PEen == null)
            {
                return NotFound();
            }
            return PartialView(PEen);
        }
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete Purchase Entry")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            #region Old Code
            ////var SET = db.AccountsTransactions.Where(a => a.Id == id).FirstOrDefault();
            #endregion
            var chk = DeletePurchase(id);
            if (chk == true)
            {
                stat = true;
                msg = "Successfully deleted Purchase entry details.";
            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Purchase Entry")]
        public ActionResult DeleteAllPurchase(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeletePurchase(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + " Purchase.", true);
            return RedirectToAction("Index", "PurchaseEntry");
        }

        private Boolean DeletePurchase(long saleId)
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

        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            var Ext = db.ConvertTransactionss.Where(x => x.From == id && x.ConvertFrom == "PurchaseExtend").FirstOrDefault();
            if (db.CrossHireReturns.Any(u => u.Invoice == id))
            {
                msg = "Purchase Entry Already used in CrossHire Return !!";
            }
            else if (Ext != null)
            {
                var inv = db.PurchaseEntrys.Where(x => x.PurchaseEntryId == Ext.To).Select(z => z.BillNo).FirstOrDefault();
                msg = "This Invoice was Extended to Invoice: " + inv + ".";
            }
            else if (db.PurchaseReturns.Any(u => u.purchaseEntryId == id))
            {
                msg = "Purchase Entry Already used in Purchase Return !!";
            }
            else if (db.PaymentBills.Any(u => u.InvoiceNo == id && u.Type == "Against Reference"))
            {
                msg = "Purchase Entry Already used in Payment Entry !!";
            }
            else
            {
                msg = null;
            }

            return msg;
        }


        private Boolean DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            PurchaseEntry PEen = db.PurchaseEntrys.Find(id);
            var PEItem = db.PEItemss.Where(a => a.PEItemsId == id);
            var PEP = db.PEPayments.Where(a => a.PurchaseEntry == id).FirstOrDefault();
            var PEPT = db.PETransactions.Where(a => a.PurchaseEntry == id).ToList();
            var PEBs = db.PEBillSundrys.Where(a => a.PurchaseEntry == id).FirstOrDefault();
            var supplierId = db.PurchaseEntrys.Where(a => a.PurchaseEntryId == id).Select(a => a.Supplier).FirstOrDefault();

            var CrossHireItem = db.HireDetails.Where(a => a.Reference == id && a.Section == "Purchase").FirstOrDefault();

            var CurrentDate = Convert.ToDateTime(System.DateTime.Now);

            if (PEen.MaterialCenter != null)
                /***************** Item Transaction ******************/
                com.ItemTransInDeleteMode("Purchase", PEen.MaterialCenter, 0, 0, id, UserId, CurrentDate);

            db.PurchaseEntrys.Remove(PEen);


            if (PEItem != null)
            {
                db.PEItemss.RemoveRange(db.PEItemss.Where(a => a.PurchaseEntry == id));
            }

            if (PEBs != null)
            {
                db.PEBillSundrys.RemoveRange(db.PEBillSundrys.Where(a => a.PurchaseEntry == id));
            }

            if (PEP != null)
            {
                db.PEPayments.RemoveRange(db.PEPayments.Where(a => a.PurchaseEntry == id));
            }

            if (PEPT != null)
            {
                db.PETransactions.RemoveRange(db.PETransactions.Where(a => a.PurchaseEntry == id));
            }

            var pay = db.Payments.Where(a => a.Reference == id && a.RefType == "Purchase").FirstOrDefault();
            if (pay != null)
            {
                db.Payments.RemoveRange(db.Payments.Where(a => a.Reference == id && a.RefType == "Purchase"));
            }

            var paybill = db.PaymentBills.Where(a => a.InvoiceNo == id && a.Type == "Against Reference" && a.BillType == "Purchase").ToList();
            if (paybill != null)
            {
                var recbillz = db.PaymentBills.Where(a => a.InvoiceNo == id && a.Type == "Against Reference" && a.BillType == "Purchase").ToList();
                recbillz.ForEach(a =>
                {
                    a.Type = "New Reference";
                    a.BillType = null;
                    a.InvoiceNo = null;
                });
                db.SaveChanges();
            }

            var ConPur = db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "Purchase").FirstOrDefault();
            if (ConPur != null)
            {
                db.ConvertTransactionss.RemoveRange(db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "Purchase"));
            }
            var appr = db.Approvals.Where(a => a.TransEntry == id && a.Type == "PurchaseEntry").FirstOrDefault();
            if (appr != null)
            {
                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == id && a.Type == "PurchaseEntry"));
            }
            var app = db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "PurchaseEntry").FirstOrDefault();
            if (app != null)
            {
                db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "PurchaseEntry"));
            }
            var CPEntry = db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "Purchase").FirstOrDefault();
            if (CPEntry != null)
            {
                db.ConvertTransactionss.RemoveRange(db.ConvertTransactionss.Where(a => a.To == id && a.ConvertTo == "Purchase"));
            }

            if (CrossHireItem != null)
            {
                db.HireDetails.RemoveRange(db.HireDetails.Where(a => a.Reference == id && a.Section == "Purchase"));
            }

            // batch stock
            var PEBst = db.BatchStocks.Where(a => a.Reference == id && a.Type == "Purchase").FirstOrDefault();
            if (PEBst != null)
            {
                db.BatchStocks.RemoveRange(db.BatchStocks.Where(a => a.Reference == id && a.Type == "Purchase"));
                db.SaveChanges();
            }
            var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
            var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;

            if (brcheck == Status.active)
            {
                bool delete = com.DeleteAlldummyAccountTransaction("Purchase", id);
                bool deletepay = com.DeleteAlldummyAccountTransaction("Purchase Payment", id);
                delete = com.DeleteAllAccountTransaction("Purchase", id);
                deletepay = com.DeleteAllAccountTransaction("Purchase Payment", id);

            }
            else
            {
                bool delete = com.DeleteAllAccountTransaction("Purchase", id);
                bool deletepay = com.DeleteAllAccountTransaction("Purchase Payment", id);
            }

            db.SaveChanges();
            com.addlog(LogTypes.Deleted, UserId, "PurchaseEntry", "PurchaseEntrys", findip(), PEen.PurchaseEntryId, "Successfully Deleted Purchase Entry");

            return true;

        }


        //for payment
        [HttpGet]
        public ActionResult Payment(long? id)
        {

            PEPaymentViewModel PEpay = new PEPaymentViewModel();

            PEpay = (from b in db.PEPayments
                     join c in db.PurchaseEntrys on b.PurchaseEntry equals c.PurchaseEntryId
                     where b.PurchaseEntry == id
                     select new PEPaymentViewModel
                     {
                         SupplierName = db.Suppliers.Where(a => a.SupplierID == b.SupplierId).Select(a => a.SupplierName).FirstOrDefault(),
                         PENo = c.PENo,
                         PEBillAmount = b.PEBillAmount,
                         PEPaidAmount = b.PEPaidAmount,
                         PEBalanceAmount = b.PEBillAmount - b.PEPaidAmount,// b.SEBalanceAmount,
                         PEDate = b.PEDate
                     }).FirstOrDefault();
            PEpay.PEEntryDate = Convert.ToDateTime(System.DateTime.Now).Date;
            return View(PEpay);
        }
        [HttpPost]
        public ActionResult Payment(PEPaymentViewModel PEPVM, long id)
        {
            var UserId = User.Identity.GetUserId();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            if (ModelState.IsValid)
            {
                PEPayment PEP = db.PEPayments.Where(a => a.PurchaseEntry == id).FirstOrDefault();
                PEP.PEPaidAmount = PEP.PEPaidAmount + Convert.ToDecimal(PEPVM.PEPayAmount);
                PEP.PEEntryDate = Convert.ToDateTime(PEP.PEEntryDate);
                PEP.PECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                if (PEP.PEBillAmount == PEP.PEPaidAmount)
                {
                    PEP.Status = 1;
                }

                db.Entry(PEP).State = EntityState.Modified;
                db.SaveChanges();

                PETransaction PEPT = new PETransaction();
                PEPT.PurchaseEntry = id;
                PEPT.SupplierId = Convert.ToInt64(PEP.SupplierId);
                PEPT.PEPayDate = Convert.ToDateTime(PEPVM.PEEntryDate);
                PEPT.PECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                PEPT.PEPayAmount = Convert.ToDecimal(PEPVM.PEPayAmount);
                PEPT.CreatedBranch = Convert.ToInt64(BranchID);
                PEPT.CreatedUserId = UserId;
                PEPT.Status = 0;

                db.PETransactions.Add(PEPT);

                db.SaveChanges();


                Int64 cashAccId = db.Accountss.Where(a => a.Group == 9).Select(a => a.AccountsID).SingleOrDefault();
                //if payment
                com.addAccountTrasaction(Convert.ToDecimal(PEPVM.PEPayAmount), 0, cashAccId, "Purchase Payment", id, DC.Debit);
                com.addAccountTrasaction(0, Convert.ToDecimal(PEPVM.PEPayAmount), cashAccId, "Purchase Payment", id, DC.Credit);



                com.addlog(LogTypes.Created, UserId, "PurchaseEntry", "PEPayments", findip(), PEP.PEPaymentId, "Successfully added Payment details");


                Success("Successfully added Payment details.", true);
                return RedirectToAction("Index", "PurchaseEntry");
            }
            else
            {
                Warning("Looks like something went wrong. Please check your form..", true);
                return Redirect(Request.GetUrlReferrer().ToString());
            }

        }
        
        public JsonResult SearchPurchaseEntry(string q, string page, int ptype)
        {
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;
            List<SelectFormat> serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                if (ptype == 0)
                {

                    serialisedJson = db.PurchaseEntrys.Where(p => p.PurType == PurchaseHireType.Purchase && p.Status == 1 && (p.BillNo.ToLower().Contains(q.ToLower()) || p.BillNo.Contains(q)))
                            .Select(b => new SelectFormat
                            {
                                text = b.BillNo, //each json object will have 
                                id = b.PurchaseEntryId
                            }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
                }
                else
                {
                    serialisedJson = db.PurchaseEntrys.Where(p => p.PurType == PurchaseHireType.CrossHire && p.Status == 1 && (p.BillNo.ToLower().Contains(q.ToLower()) || p.BillNo.Contains(q)))
                           .Select(b => new SelectFormat
                           {
                               text = b.BillNo, //each json object will have 
                               id = b.PurchaseEntryId
                           }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
                }
            }
            else
            {
                if (ptype == 0)
                {
                    serialisedJson = db.PurchaseEntrys.Where(p => p.PurType == PurchaseHireType.Purchase && p.Status == 1).Select(b => new SelectFormat
                    {
                        text = b.BillNo, //each json object will have 
                        id = b.PurchaseEntryId
                    }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
                }
                else
                {
                    serialisedJson = db.PurchaseEntrys.Where(p => p.PurType == PurchaseHireType.CrossHire && p.Status == 1).Select(b => new SelectFormat
                    {
                        text = b.BillNo, //each json object will have 
                        id = b.PurchaseEntryId
                    }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

                }
            }
            return Json(serialisedJson);
        }
        [HttpGet]
        public ActionResult GetPurchaseEntryById(int prentryId)
        {
            var v = (from a in db.PurchaseEntrys
                     join b in db.Suppliers on a.Supplier equals b.SupplierID into supp
                     from b in supp.DefaultIfEmpty()
                     join c in db.PEPayments on a.PurchaseEntryId equals c.PurchaseEntry
                     where a.PurchaseEntryId == prentryId
                     select new
                     {
                         SupplierName = b.SupplierCode + "-" + b.SupplierName,
                         a.Supplier,
                         a.SupplierType,
                         a.PEDiscount,
                         a.PEGrandTotal,
                         c.PEPaidAmount,
                         PEDueAmount = a.PEGrandTotal - c.PEPaidAmount
                     }).FirstOrDefault();
            return Json(v);

        }
        private string InvoiceNo2(Int64 PENo = 0, string billNo = null, string section = null)
        {
            string prefix = (section == "Hire") ? "CrossHireInvoice" : "PurchaseReferenceNo";
            var companyPrefix = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.prefix).FirstOrDefault();
            PurchaseHireType type = (section != "Hire") ? PurchaseHireType.Purchase : PurchaseHireType.CrossHire;

            if (billNo == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.number).FirstOrDefault();
                var tempnum = db.PurchaseEntrys.Where(q => q.PurType == type).Select(p => p.ReferenceNo).Max();
                Int64 num = Convert.ToInt64(tempnum);

                if (num == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    var tempPENo = db.PurchaseEntrys.Where(q => q.PurType == type).Max(p => p.ReferenceNo);
                    PENo = Convert.ToInt64(tempPENo) + 1;
                    billNo = companyPrefix + PENo;
                    if (BillExist(billNo))
                    {
                        billNo = InvoiceNo2(PENo, billNo, section);
                    }
                }
            }
            else
            {
                PENo = PENo + 1;
                billNo = companyPrefix + PENo;
                if (BillExist2(billNo))
                {
                    billNo = InvoiceNo2(PENo, billNo);
                }

            }
            return billNo;
        }
        private bool BillExist2(string PENo)
        {
            var Exists = db.PurchaseEntrys.Any(c => c.ReferenceNo == PENo);
            bool res = (Exists) ? true : false;
            return res;
        }
        private string InvoiceNo(Int64 PENo = 0, string billNo = null, string section = null)
        {
            string prefix = (section == "Hire") ? "CrossHireInvoice" : "Purchase";
            var companyPrefix = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.prefix).FirstOrDefault();
            PurchaseHireType type = (section != "Hire") ? PurchaseHireType.Purchase : PurchaseHireType.CrossHire;

            if (billNo == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.number).FirstOrDefault();
                Int64 num = db.PurchaseEntrys.Where(q => q.PurType == type).Select(p => p.PENo).AsEnumerable().DefaultIfEmpty(0).Max();
                if (num == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    PENo = db.PurchaseEntrys.Where(q => q.PurType == type).Max(p => p.PENo + 1);
                    billNo = companyPrefix + PENo;
                    if (BillExist(billNo))
                    {
                        billNo = InvoiceNo(PENo, billNo, section);
                    }
                }
            }
            else
            {
                PENo = PENo + 1;
                billNo = companyPrefix + PENo;
                if (BillExist(billNo))
                {
                    billNo = InvoiceNo(PENo, billNo);
                }

            }
            return billNo;
        }
        private bool BillExist(string PENo)
        {
            var Exists = db.PurchaseEntrys.Any(c => c.BillNo == PENo);
            bool res = (Exists) ? true : false;
            return res;
        }
        [HttpGet]
        public JsonResult chkBillExist(string SENo)
        {
            bool res = false;
            return Json(res);
        }
        private long GetPeNo()
        {
            Int64 PENo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "Purchase").Select(a => a.number).FirstOrDefault();
            if ((db.PurchaseEntrys.Select(p => p.PENo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
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
                PENo = db.PurchaseEntrys.Max(p => p.PENo + 1);
            }

            return PENo;
        }

        //excel uploads
        [HttpGet]
        public virtual ActionResult DownloadExcel(string file)
        {
            var EnableJewellery = db.EnableSettings.Where(a => a.EnableType == "EnableJewellery").FirstOrDefault();
            var JewCheck = EnableJewellery != null ? EnableJewellery.Status : Status.inactive;
            string fullPath = "";
            string fileName = "";
            if (JewCheck == Status.active)
            {
                fullPath = Path.Combine(LegacyWeb.MapPath("~/uploads/excelitem/excelformat/JewelleryPurchaseItems.xlsx"));
                fileName = "JewelleryPurchaseItems.xlsx";
            }
            else
            {
                fullPath = Path.Combine(LegacyWeb.MapPath("~/uploads/excelitem/excelformat/PurchaseItems.xlsx"));
                fileName = "PurchaseItems.xlsx";
            }
            return File(fullPath, "application/vnd.ms-excel", fileName);
        }
        //[HttpPost]



        //                //time being tax direct given //2 = 5% tax and 1 = 0 tax
        //                //futurw usage




        //                  where itemids.Contains(a.ItemID.ToString())
        //                      Item=a.ItemID,
        //                      ItemQuantity=1,
        //                      a.ItemUnit,
        //                      ItemUnitPrice=a.PurchasePrice,
        //                      ItemTax=e.Percentage,
        //                      ItemSubTotal=0,
        //                      ItemTaxAmount=0,
        //                      ItemDiscount=0,
        //                      ItemTotalAmount=0,
        //                      note = "",
        //                      ItemCode = a.ItemCode,
        //                      ItemName = a.ItemName,
        //                      ItemWithCode = a.ItemCode + " - " + a.ItemName,
        //                      a.ItemUnitID,
        //                      a.SubUnitId,
        //                      PriUnit = c.ItemUnitName,
        //                      SubUnit = d.ItemUnitName,
        //                      a.BasePrice,
        //                      a.SellingPrice,
        //                      a.PurchasePrice,
        //                      a.MRP



        //get categoryId


        private string ItemCodes(Int64 INo = 0, string ICode = null)
        {
            var prefix = db.CodePrefixs.Where(a => a.section == "Item").Select(a => a.prefix).FirstOrDefault();
            if (ICode == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "Item").Select(a => a.number).FirstOrDefault();
                if ((db.Items.Select(p => p.ItemID).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    if (number == 0)
                    {
                        ICode = prefix + 1;
                    }
                    else
                    {
                        ICode = prefix + number;
                    }
                }
                else
                {
                    INo = db.Items.Max(p => p.ItemID + 1);
                    ICode = prefix + INo;
                    if (CodeExist(ICode))
                    {
                        ICode = ItemCodes(INo, ICode);
                    }

                }
            }
            else
            {
                INo = INo + 1;
                ICode = prefix + INo;
                if (CodeExist(ICode))
                {
                    ICode = ItemCodes(INo, ICode);
                }
            }
            return ICode;
        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult confirmotp(long salesEntryId, string otp)
        {
            DateTime nw = System.DateTime.Now;
            var user = User.Identity.GetUserId();

            var con = (

                       from c in db.otpapproves
                       join d in db.SalesEntrys on c.entryid equals d.SalesEntryId
                       join e in db.UserEditDayss on c.approvedby equals e.userid
                       where d.SalesEntryId == salesEntryId && c.otp == otp && c.purpose == "Sales"
                       && c.expdate >= nw
                       select new
                       {
                           e.days,
                           d.SECreatedDate,
                           c.optid
                       }
                       ).FirstOrDefault();
            if (con == null)
            {


                bool st = false;
                return Json(new { status = st, message = "Otp Expired" });
            }
            else
            {
                var editableDay = DateTime.Now;

                if (con.days == 0)
                {
                    editableDay = nw.AddYears(-10);

                }
                else
                {
                    editableDay = nw.AddMinutes(-con.days);
                }
                if ((con.SECreatedDate - editableDay).TotalMinutes < 0)
                {
                    bool st = false;
                    return Json(new { status = st, message = "Super User Edit Days Expired" });

                }
                else
                {
                    var ap = db.otpapproves.Where(o => o.optid == con.optid && o.otp == otp && o.entryid == salesEntryId).FirstOrDefault();
                    ap.approvedby = user;
                    db.Entry(ap).State = EntityState.Modified;
                    db.SaveChanges();
                    bool st = true;
                    return Json(new { status = st, otp = 0 });
                }
            }


        }
      

        [AllowAnonymous]
        [HttpGet]
        public ActionResult getotp(long salesEntryId)
        {
            DateTime nw = System.DateTime.Now;
            var user = User.Identity.GetUserId();
            var otp = db.otpapproves.Where(o => o.entryid == salesEntryId && o.requestedby == user && o.purpose == "Sales" && o.expdate >= nw).Select(o => o.otp).FirstOrDefault();
            Random r = new Random();
            var x = r.Next(0, 1000000);
            string s = x.ToString("000000");
            string otps = "";
            if (otp == "" || otp == null)
            {

                //                where c.SalesEntryId == salesEntryId
                //                && ap.requestedby == user && ap.purpose == "Sales" && ap.expdate >= nw
                //                    aa.days,
                //                    c.SECreatedDate



                var mc = db.SalesEntrys.Where(o => o.SalesEntryId == salesEntryId).Select(o => o.MaterialCenter).FirstOrDefault();
                var mcsuper = (from a in db.SuperUsers
                               join b in db.Employees on a.employeeid equals b.EmployeeId
                               join c in db.Users on b.UserId equals c.Id
                               where a.mcid == mc && a.purpose == "Sales"
                               select new
                               {
                                   c.Id
                               }).ToList().ToArray();
                if (mcsuper.Count() > 0)
                {
                    db.otpapproves.RemoveRange(db.otpapproves.Where(o => o.entryid == salesEntryId && o.purpose == "Sales"));
                    db.SaveChanges();
                    foreach (var c in mcsuper)
                    {
                        Random rr = new Random();
                        var xx = rr.Next(0, 1000000);
                        string ss = xx.ToString("000000");

                        otpapprove a = new otpapprove
                        {
                            entryid = salesEntryId,
                            purpose = "Sales",
                            expdate = nw.AddMinutes(20),
                            requestedby = user,
                            approvedby = c.Id,

                            otp = ss,
                        };

                        otps = "new";
                        db.otpapproves.Add(a);
                        db.SaveChanges();
                    }
                    bool st = true;
                    return Json(new { status = st, otp = mcsuper.Count() });

                }
                else
                {
                    bool st = false;
                    return Json(new { status = st, otp = 0 });

                }



            }
            else
            {
                bool st = true;
                return Json(new { status = st, otp = 0 });

            }
            ////       var userEditDays = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.days).FirstOrDefault();
            ////     var userEdit = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.id).FirstOrDefault();




            var mails = (from a in db.SalesEntrys
                         join b in db.SuperUsers on a.MaterialCenter equals b.mcid
                         where a.SalesEntryId == salesEntryId
                         select new
                         {
                             b.emailid,
                             a.BillNo,
                         }
                       ).ToList();





            bool stat = true;
            return Json(new { status = stat, otp = otps });

        }
        private bool CodeExist(string Code)
        {
            var Exists = db.Items.Any(c => c.ItemCode == Code);
            bool res = (Exists) ? true : false;
            return res;
        }
        private string PrefixCodes(Int64 pre = 0, Int64 INo = 0, string ICode = null)
        {
            var prefix = (pre != 0) ? db.PrefixMasters.Where(a => a.Id == pre).Select(a => a.PrefixCode).FirstOrDefault() : "";
            if (ICode == null)
            {
                Int64 number = db.ItemPrefixs.Where(a => a.Prefix == pre).Select(a => a.No).AsEnumerable().DefaultIfEmpty(0).Max();
                if ((db.Items.Select(p => p.ItemID).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    if (number == 0)
                    {
                        ICode = prefix + 1;
                    }
                    else
                    {
                        number++;
                        ICode = prefix + number;
                    }
                }
                else
                {
                    INo = number + 1;
                    ICode = prefix + INo;
                    if (PrefixCodeExist(ICode))
                    {
                        ICode = PrefixCodes(pre, INo, ICode);
                    }
                }
            }
            else
            {
                INo = INo + 1;
                ICode = prefix + INo;
                if (PrefixCodeExist(ICode))
                {
                    ICode = PrefixCodes(pre, INo, ICode);
                }
            }

            return ICode;
        }
        private bool PrefixCodeExist(string Code)
        {
            var Exists = db.Items.Any(c => c.ItemCode == Code);
            bool res = (Exists) ? true : false;
            return res;
        }

        public int ItemType(string arr)
        {
            var ty = arr.ToUpper().ToString();
            int itemtype = 0;
            if (ty == "D" || ty == "DIAMOND")
            {
                itemtype = 2;
            }
            else if (ty == "W" || ty == "WATCH")
            {
                itemtype = 3;
            }
            else if (ty == "OBJ" || ty == "OBJECT")
            {
                itemtype = 4;
            }
            else
            {
                itemtype = 1;
            }
            return itemtype;
        }
        private string PrefixCode(string Prefix)
        {
            long Preid = 0;
            var NewPrefix = "";
            Preid = db.PrefixMasters.Where(x => x.PrefixCode == Prefix).Select(y => y.Id).FirstOrDefault();
            if (Preid != 0)
            {
                long lastid = 0;
                if (Preid != 0)
                {
                    lastid = db.ItemPrefixs.Where(x => x.Prefix == Preid).Select(x => x.No).Max();
                }
                var newprecode = Prefix + (lastid + 1);
                Regex re = new Regex(@"([a-zA-Z]+)(\d+)");
                Match result = re.Match(newprecode);
                string alphaPart = result.Groups[1].Value;
                string num = result.Groups[2].Value;
                int length = (result.Groups[2].Value).Length;

                if (length < 5)
                {
                    string s = new String('0', 5 - length);
                    string newnum = s + num;
                    NewPrefix = alphaPart + newnum;
                }
                else
                {
                    NewPrefix = Convert.ToString(lastid);
                }
            }
            return NewPrefix;
        }
        public long GetPrefixId(string Prefix, long Brand, long Category)
        {
            var UserId = User.Identity.GetUserId();
            long branch = 0;
            Int64 preID = 0;
            branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
            var Exists = db.PrefixMasters.Any(c => c.PrefixCode == Prefix);
            if (Exists)
            {
                preID = db.PrefixMasters.Where(c => c.PrefixCode == Prefix).Select(c => c.Id).FirstOrDefault();
            }
            else
            {
                PrefixMaster pre = new PrefixMaster();
                pre.PrefixCode = Prefix;
                pre.Branch = branch;
                pre.Brand = Brand;
                pre.Category = Category;
                pre.ConRate = Convert.ToString(1);
                pre.editable = choice.Yes;
                pre.CreatedBy = UserId;
                pre.CreatedDate = System.DateTime.Now;
                db.PrefixMasters.Add(pre);
                db.SaveChanges();
                preID = pre.Id;
                ItemPrefix it = new ItemPrefix();
                it.No = 1;
                it.Prefix = preID;
                db.ItemPrefixs.Add(it);
                db.SaveChanges();
            }
            return preID;
        }
        public Int32 chkInCompleteTransaction()
        {
            var entry = (from a in db.PurchaseEntrys
                         join c in db.PEPayments on a.PurchaseEntryId equals c.PurchaseEntry into pay
                         from c in pay.DefaultIfEmpty()
                         where c.PurchaseEntry == null
                         select new
                         {
                             a.PurchaseEntryId
                         }).ToList();

            foreach (var entryid in entry)
            {
                DeletePurchase(entryid.PurchaseEntryId);
            }


            return 0;
        }
        [HttpGet]
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "PurchaseEntry" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
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

            var MR = db.PurchaseEntrys.Where(a => a.PurchaseEntryId == id).FirstOrDefault();

            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "PurchaseEntry").OrderByDescending(a => a.CreatedDate).FirstOrDefault();
            var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
            var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;

            if (brcheck == Status.active)
            {
                if (App.ApprovalStatus == ApprovalStatus.Approved)
                {
                    var SItems = db.DummyPEItems2.Where(a => a.PurchaseEntry == id).ToList();
                    foreach (var arr in SItems)
                    {
                        //add to dummy table
                        PEItems dItem = new PEItems();
                        dItem.ItemUnit = arr.ItemUnit;
                        dItem.ItemUnitPrice = arr.ItemUnitPrice;
                        dItem.ItemQuantity = arr.ItemQuantity;
                        dItem.ItemSubTotal = arr.ItemSubTotal;
                        dItem.ItemDiscount = arr.ItemDiscount;
                        dItem.ItemTax = arr.ItemTax;
                        dItem.ItemTaxAmount = arr.ItemTaxAmount;
                        dItem.ItemTotalAmount = arr.ItemTotalAmount;
                        dItem.itemNote = arr.itemNote;
                        dItem.PurchaseEntry = arr.PurchaseEntry;
                        dItem.Item = arr.Item;
                        dItem.ProjectId = arr.ProjectId;
                        dItem.TaskId = arr.TaskId;
                        db.PEItemss.Add(dItem);
                        db.SaveChanges();
                    }
                    db.DummyPEItems2.RemoveRange(db.DummyPEItems2.Where(a => a.PurchaseEntry == id));
                    db.SaveChanges();
                    var dummyacc2 = db.dummyAccountsTransactions.Where(
                                     a=> a.reference == id && a.Purpose.Contains("Purchase")).ToList();
                    //                where a.reference == id && a.Purpose.Contains("Purchase")
                    //                    Account = a.Account,
                    //                    Accounts = a.Accounts,
                    //                    CreatedDate = a.CreatedDate,
                    //                    Credit = a.Credit,
                    //                    Date = a.Date,
                    //                    Debit = a.Debit,
                    //                    Narration = a.Narration,
                    //                    Project = a.Project,
                    //                    ProTask = a.ProTask,
                    //                    Purpose = a.Purpose,
                    //                    reference = a.reference,
                    //                    Status = a.Status,
                    //                    Type = a.Type,





                   foreach(var a in dummyacc2)
                    {
                        var acc = new AccountsTransaction
                        {
                            Debit = a.Debit,
                            Credit = a.Credit,
                            Account = a.Account,
                            Purpose = a.Purpose,
                            reference = a.reference,
                            Type = a.Type,
                            Date = a.Date,
                            Status = a.Status,
                            Narration = a.Narration,
                            Project = a.Project,
                            ProTask = a.ProTask,
                            CreatedDate =a.CreatedDate,
                        };
                        db.AccountsTransactions.Add(acc);
                        db.SaveChanges();

                    }
                  
                    db.dummyAccountsTransactions.RemoveRange(dummyacc2);
                        db.SaveChanges();
                    
                }
               
            }
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
                AppUp.Type = "PurchaseEntry";

                db.ApprovalUpdates.Add(AppUp);
                db.SaveChanges();
                ApprovalUpdatestwp AppUpp = new ApprovalUpdatestwp();
                AppUpp.ApprovalStatus = App.ApprovalStatus;
                AppUpp.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                AppUpp.ApprovedBy = UserId;
                AppUpp.Note = App.Note;
                AppUpp.RequestBy = MR.CreatedBy;
                AppUpp.Status = Status.active;
                AppUpp.TransEntry = id;
                AppUpp.Type = "PurchaseEntry";

                db.ApprovalUpdatestwp.Add(AppUpp);
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
        public ActionResult statusupdation(PurchaseEntry App, long id)
        {
            bool stat = false;
            string msg = "";
            var UserId = User.Identity.GetUserId();

            var MR = db.PurchaseEntrys.Find(id);

            MR.PurchaseStatus = App.PurchaseStatus;
            db.Entry(MR).State = EntityState.Modified;
              db.SaveChanges();

                stat = true;
                msg = "Successfully Updated Status.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            
        }

        [HttpGet]
        public ActionResult statusupdation(long id)
        {


            PurchaseEntry PEen = db.PurchaseEntrys.Where(x =>x.PurchaseEntryId == id).FirstOrDefault();

            if (PEen == null)
            {
                return NotFound();
            }
            return PartialView(PEen);

        }

        [HttpGet]
        public ActionResult EditStatusPayment(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "PurchaseEntryPayment" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
                                       .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                       .ToList();

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       where (e != ApprovalStatus.PendingApproval && e != ApprovalStatus.Completed && e != ApprovalStatus.PartialApproval) && (appstat.Count == 0 || e != appstat.Select(a => a.ApprovalStatus).FirstOrDefault())
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            return PartialView();
        }

        [HttpPost]
        public ActionResult EditStatusPayment(ApprovalUpdate App, long id)
        {
            bool stat = false;
            string msg = "";
            var UserId = User.Identity.GetUserId();

            var MR = db.PurchaseEntrys.Where(a => a.PurchaseEntryId == id).FirstOrDefault();

            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "PurchaseEntry").OrderByDescending(a => a.CreatedDate).FirstOrDefault();

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
                AppUp.Type = "PurchaseEntryPayment";

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
        [HttpGet]
        public ActionResult EditStatusPaymentApproved(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "PurchaseEntryPayment" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
                                       .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                       .ToList();

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       where e == ApprovalStatus.Completed && (appstat.Count == 0 || e != appstat.Select(a => a.ApprovalStatus).FirstOrDefault())
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            return PartialView();
        }

        [HttpPost]
        public ActionResult EditStatusPaymentApproved(ApprovalUpdate App, long id)
        {
            bool stat = false;
            string msg = "";
            var UserId = User.Identity.GetUserId();

            var MR = db.PurchaseEntrys.Where(a => a.PurchaseEntryId == id).FirstOrDefault();

            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "PurchaseEntry").OrderByDescending(a => a.CreatedDate).FirstOrDefault();

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
                AppUp.Type = "PurchaseEntryPayment";

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
            var UserView = (from b in db.ApprovalUpdatestwp
                            join c in db.Users on b.ApprovedBy equals c.Id
                            join d in db.PurchaseEntrys on b.TransEntry equals d.PurchaseEntryId into team
                            from d in team.DefaultIfEmpty()
                            join e in db.Employees on b.RequestBy equals e.UserId into emp
                            from e in emp.DefaultIfEmpty()
                            join u in db.Users on d.CreatedBy equals u.Id into req
                            from u in req.DefaultIfEmpty()
                            where b.TransEntry == MCId && b.Type == "PurchaseEntry"
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
        [HttpPost]
        public ActionResult GetAllStatusUpdationPayment(long MCId)
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
                            join d in db.PurchaseEntrys on b.TransEntry equals d.PurchaseEntryId into team
                            from d in team.DefaultIfEmpty()
                            join e in db.Employees on b.RequestBy equals e.UserId into emp
                            from e in emp.DefaultIfEmpty()
                            join u in db.Users on d.CreatedBy equals u.Id into req
                            from u in req.DefaultIfEmpty()
                            where b.TransEntry == MCId && b.Type == "PurchaseEntryPayment"
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

        private long GetSeNo(PurchaseHireType type)
        {
            Int64 PENo = 0;
            string prefix = (type == PurchaseHireType.CrossHire) ? "CrossHireInvoice" : "Invoice";
            Int32 number = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.number).FirstOrDefault();
            if ((db.PurchaseEntrys.Where(a => a.PurType == type).Select(p => p.PENo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                PENo = (number == 0) ? 1 : number;
            }
            else
            {
                PENo = db.PurchaseEntrys.Where(a => a.PurType == type).Max(p => p.PENo + 1);
            }

            return PENo;
        }
        [HttpPost]
        public ActionResult GetHireInvoiceNum(string hiretype)
        {
            string hirerate = (hiretype == "Hire") ? InvoiceNo(0, null, hiretype) : InvoiceNo();
            return Json(hirerate);
        }

        [HttpGet]
        public JsonResult GetPEItemsHire(long PurchaseEntryID)
        {
            var v = (from a in db.PEItemss
                     join b in db.Items on a.Item equals b.ItemID
                     join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                     from c in primary.DefaultIfEmpty()
                     join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                     from d in second.DefaultIfEmpty()
                     join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
                     from e in cat.DefaultIfEmpty()
                     join f in db.Taxs on b.TaxID equals f.TaxID into taxss
                     from f in taxss.DefaultIfEmpty()
                     join g in db.PurchaseEntrys on a.PurchaseEntry equals g.PurchaseEntryId
                     where a.PurchaseEntry == PurchaseEntryID && a.itemNote != "-:{Bundle_Item}"
                     && g.PurType == PurchaseHireType.CrossHire //&& b.KeepStock == true
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

                         RetItemQuantity = (decimal?)(from aa in db.CrossHrItems
                                                      join bb in db.CrossHireReturns on aa.Hr equals bb.HireReturnId
                                                      where aa.Item == a.Item && bb.Invoice == PurchaseEntryID
                                                      && aa.ItemNote != "-:{Bundle_Item}"
                                                      select new
                                                      {
                                                          aa.ItemQuantity
                                                      }).Sum(a => a.ItemQuantity) ?? 0,

                         DvItemQuantity = (decimal?)(from ab in db.PEItemss
                                                     join ba in db.PurchaseEntrys on ab.PurchaseEntry equals ba.PurchaseEntryId
                                                     where ab.Item == a.Item && ba.PurType == PurchaseHireType.CrossHire
                                                     && ba.PurchaseEntryId == PurchaseEntryID
                                                     && ab.itemNote != "-:{Bundle_Item}"
                                                     select new
                                                     {
                                                         ab.ItemQuantity
                                                     }).Sum(a => a.ItemQuantity) ?? 0,

                     }).ToList();

            var ConD = (from o in v
                        select new
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

                            RetItemQuantity = o.RetItemQuantity,
                            DvItemQuantity = o.DvItemQuantity,

                            bundle = (from ab in db.PEItemss
                                      join bb in db.Items on ab.Item equals bb.ItemID
                                      join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                      from dd in scaffold.DefaultIfEmpty()

                                      join eb in db.ItemUnits on ab.ItemUnit equals eb.ItemUnitID into bpunit
                                      from eb in bpunit.DefaultIfEmpty()

                                      let bimg = db.ItemImages.Where(bim => bim.ItemID == ab.Item).Select(bim => new { bim.FileName, bim.Status, bim.ItemImageID }).ToList()
                                      where ab.PurchaseEntry == PurchaseEntryID && ab.itemNote == "-:{Bundle_Item}"
                                      && o.ItemID == ab.ItemDiscount
                                      select new
                                      {
                                          Id = bb.ItemID,
                                          ItemUnitPrice = ab.ItemUnitPrice,
                                          ItemQuantity = ab.ItemQuantity,
                                          ItemSubTotal = ab.ItemSubTotal,
                                          ItemDiscount = ab.ItemDiscount,
                                          ItemNote = "",
                                          ItemTax = ab.ItemTax,
                                          ItemTaxAmount = ab.ItemTaxAmount,
                                          ItemTotalAmount = ab.ItemTotalAmount,

                                          ItemCode = bb.ItemCode,
                                          ItemName = bb.ItemName,
                                          ItemUnit = eb.ItemUnitName,
                                          PartNumber = bb.PartNumber,
                                          PNoStatus = "",
                                          CBM = dd.CBM,
                                          Weight = dd.Weight,
                                          img = bimg,
                                          note = ab.itemNote.Replace("<br />", "\n"),

                                          KeepStock = bb.KeepStock,

                                          ab.Item,
                                          ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                          bb.ItemUnitID,
                                          bb.SubUnitId,
                                          bb.ItemArabic,
                                          bb.ItemDescription,
                                          BaseQty = (ab.ItemQuantity / o.ItemQuantity),

                                          RetItemQuantity = (decimal?)(from xx in db.CrossHrItems
                                                                       join yy in db.CrossHireReturns on xx.Hr equals yy.HireReturnId
                                                                       where xx.ItemNote == "-:{Bundle_Item}"
                                                                       && yy.Invoice == PurchaseEntryID
                                                                       && ab.Item == xx.Item
                                                                       && xx.ItemDiscount == ab.ItemDiscount
                                                                       select new
                                                                       {
                                                                           xx.ItemQuantity
                                                                       }).Sum(a => a.ItemQuantity) ?? 0,

                                          DvItemQuantity = (decimal?)(from xx in db.PEItemss
                                                                      join yy in db.PurchaseEntrys on xx.PurchaseEntry equals yy.PurchaseEntryId
                                                                      where xx.itemNote == "-:{Bundle_Item}"
                                                                      && yy.PurchaseEntryId == PurchaseEntryID
                                                                      && ab.Item == xx.Item
                                                                      && xx.ItemDiscount == ab.ItemDiscount
                                                                      select new
                                                                      {
                                                                          xx.ItemQuantity
                                                                      }).Sum(a => a.ItemQuantity) ?? 0,


                                      }).ToList(),

                        }).ToList();

            return Json(ConD);
        }

        public JsonResult SearchCrossHireEntry(string q, long cust, long? project)
        {
            object serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {

                serialisedJson = (from a in db.PurchaseEntrys
                                  where (a.PurType == PurchaseHireType.CrossHire) && a.Supplier == cust && (a.BillNo.ToLower().Contains(q.ToLower()) || a.BillNo.Contains(q))
                                  select new
                                  {
                                      id = a.PurchaseEntryId,
                                      text = a.BillNo
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.PurchaseEntrys
                                  where (a.PurType == PurchaseHireType.CrossHire) && a.Supplier == cust
                                  select new
                                  {
                                      id = a.PurchaseEntryId,
                                      text = a.BillNo

                                  }).OrderBy(b => b.text).ToList();
            }
            return Json(serialisedJson);
        }
        [HttpPost]
        public ActionResult GetLastStockTransfer(int? purchasecount, long? ItemId, long? mc)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int pcount = (Int32)purchasecount != null ? (Int32)purchasecount : 10;

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();
            var v = (from b in db.StockTransferItems
                     join c in db.Items on b.Item equals c.ItemID

                     join e in db.ItemUnits on b.Unit equals e.ItemUnitID into punit
                     from e in punit.DefaultIfEmpty()

                     join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                     from g in bundle.DefaultIfEmpty()
                     join s in db.StockTransfers on b.StockTransferId equals s.Id into secondary
                     from s in secondary.DefaultIfEmpty()

                     where b.Item == ItemId
                     && (mc==null||s.MCTo==mc)
                    
                     select new
                     {
                         b.Item,
                         s.Date,
                         s.Id,
                         s.Voucher,
                         SupplierName="",
                         ItemUnitPrice = b.Price,
                         ItemQuantity = b.Quantity,
                         ItemSubTotal = b.Amount,
                         ItemDiscount =0 ,
                         ItemTax = 5,
                         itemNote = "",
                         ItemTaxAmount =0,
                         ItemTotalAmount = b.Price*b.Quantity,
                         ItemCode = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemCode).FirstOrDefault(),
                         ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                         ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.Unit).Select(a => a.ItemUnitName).FirstOrDefault(),
                         PartNumber = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.PartNumber).FirstOrDefault(),
                         //bundleitem = (from ab in db.SEItemss
                         //              where ab.itemNote == "-:{Bundle_Item}"
                         //              && b.Item == ab.ItemDiscount
                         //                  ItemCode = bb.ItemCode,
                         //                  ItemName = bb.ItemName,
                         //                  ItemUnit = cb.ItemUnitName,
                         //                  ItemQuantity = ab.ItemQuantity,
                         //              }).ToList()
                     });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = v.Count();
            var data = v.Take(pcount).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        [HttpPost]
        public ActionResult GetLastPurchase(int? purchasecount, long? ItemId,long? mc)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int pcount = (Int32)purchasecount != null ? (Int32)purchasecount :10;

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();
            var v = (from b in db.PEItemss
                     join c in db.Items on b.Item equals c.ItemID

                     join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                     from e in punit.DefaultIfEmpty()

                     join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                     from g in bundle.DefaultIfEmpty()
                     join s in db.PurchaseEntrys on b.PurchaseEntry equals s.PurchaseEntryId into secondary
                     from s in secondary.DefaultIfEmpty()
                     join p in db.Suppliers on s.Supplier equals p.SupplierID 

                     where b.Item == ItemId
                      && (mc == null || s.MaterialCenter == mc)
                     select new
                     {
                         b.PEItemsId,
                         s.PEDate,
                         s.PurchaseEntryId,
                         s.BillNo,
                         p.SupplierName,
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
                         //bundleitem = (from ab in db.SEItemss
                         //              where ab.itemNote == "-:{Bundle_Item}"
                         //              && b.Item == ab.ItemDiscount
                         //                  ItemCode = bb.ItemCode,
                         //                  ItemName = bb.ItemName,
                         //                  ItemUnit = cb.ItemUnitName,
                         //                  ItemQuantity = ab.ItemQuantity,
                         //              }).ToList()
                     });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = v.Count();
            var data = v.Take(pcount).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        public List<ConvertTransactions> ExtNum(long id, List<ConvertTransactions> ExtList)
        {
            ConvertTransactions Ext = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.To == id).FirstOrDefault();
            if (Ext != null)
            {
                ExtList.Add(Ext);
                ExtNum(Ext.From, ExtList);
            }
            return ExtList;
        }
        public List<ConvertTransactions> ExtNumDetails(long id, List<ConvertTransactions> ExtList)
        {
            ConvertTransactions Ext = db.ConvertTransactionss.Where(x => x.ConvertFrom == "PurchaseExtend" && x.To == id).FirstOrDefault();
            if (Ext != null)
            {
                ExtList.Add(Ext);
                ExtNumDetails(Ext.From, ExtList);
            }
            else if (Ext == null)
            {
                ConvertTransactions st = new ConvertTransactions();
                st.To = id;
                st.From = 0;
                ExtList.Add(st);
            }

            return ExtList;
        }

        //-->GET(From Item Serial No)


        [HttpPost]
        public ActionResult ImportVendorPrice(IFormFile file)
        {
            DataSet ds = new DataSet();
            if (Request.Form.Files["file"].Length > 0)
            {
                string fileExtension = System.IO.Path.GetExtension(Request.Form.Files["file"].FileName);
                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {

                    string Files = Request.Form.Files["file"].FileName;
                    Files = string.Concat(Path.GetFileNameWithoutExtension(Files), DateTime.Now.ToString("yyyyMMddHHmmssfff"), Path.GetExtension(Files));

                    string fileLocation = LegacyWeb.MapPath("~/uploads/excelitem/") + Files;//Request.Form.Files["file"].FileName;

                    string fileLoc = LegacyWeb.MapPath("~/uploads/excelitem/");
                    if (!Directory.Exists(fileLoc))
                        Directory.CreateDirectory(fileLoc);

                    Request.Form.Files["file"].SaveAs(fileLocation);
                    string excelConnectionString = string.Empty;
                    excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                    fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    //connection String for xls file format.
                    if (fileExtension == ".xls")
                    {
                        excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                        fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    }
                    //connection String for xlsx file format.
                    else if (fileExtension == ".xlsx")
                    {
                        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                        fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    }
                    //Create Connection to Excel work book and add oledb namespace
                    OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                    excelConnection.Open();
                    DataTable dt = new DataTable();

                    dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    if (dt == null)
                    {
                        return null;
                    }

                    String[] excelSheets = new String[dt.Rows.Count];
                    int t = 0;
                    //excel data saves in temp file here.
                    foreach (DataRow row in dt.Rows)
                    {
                        excelSheets[t] = row["TABLE_NAME"].ToString();
                        t++;
                    }
                    OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);


                    string query = string.Format("Select * from [{0}]", excelSheets[0]);
                    using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                    {
                        dataAdapter.Fill(ds);
                    }



                    DataTable dtCol = new DataTable();
                    dtCol.Columns.Add("serialnumber");

                    var ViewModel = new VenderRateViewModel
                    {
                     
                        entrydate = DateTime.Now,
                    
                    };
                    Int32 chkCount = 0;
                    DataTable newdt = ds.Tables[0];
                   
                    if (1 == 1)
                    {
                        if (newdt.Rows.Count > 0)
                        {
                            Int32 ItmCount = 0;
                            var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
                            var brcheck = enable != null ? enable.Status : Status.inactive;
                            var UserId = User.Identity.GetUserId();
                    
                            List<VendorNewRate> objj = new List<VendorNewRate>();
                            for (int i = 1; i < newdt.Rows.Count; i++)
                            {
                                if (newdt.Rows[i][0] != DBNull.Value)
                                {
                                    if (newdt.Rows[i][0].ToString() != "" && newdt.Rows[i][3].ToString() != "" && newdt.Rows[i][1].ToString()!="")
                                    {
                                        VendorNewRate obj = new VendorNewRate();


                                        obj.ItemType = newdt.Rows[i][0].ToString();
                                        obj.ExternalModal = newdt.Rows[i][1].ToString();
                                        obj.InternalModal = newdt.Rows[i][2].ToString();
                                        obj.Rate = Convert.ToDecimal(newdt.Rows[i][3]);

                                        objj.Add(obj);
                                    }

                                }




                            }
                          
                            ViewModel.vendorNewRates = objj;
                        }
                    }
                    //Item
                    var sup = db.Suppliers.Select(s => new
                    {
                        SupplierID = s.SupplierID,
                        SupplierDetails = s.SupplierCode + " - " + s.SupplierName
                    }).ToList();
                    ViewBag.Supp = QkSelect.List(sup, "SupplierID", "SupplierDetails");

                    return View("GenerateVenderRate", ViewModel);

























}
                else
                {
                    Danger("Please Upload Excel file..", false);
                    
                    return RedirectToAction("GenerateVenderRate", "PurchaseEntry");
                }
            }
            else
            {
                Danger("Please Upload file..", false);
                return RedirectToAction("GenerateVenderRate", "PurchaseEntry");
            }
        }


        [HttpPost]
        //   [QkAuthorize(Roles = "Dev,Item ExcelUpload")]
        public ActionResult Import(IFormFile file,  long item, int rowstart, int colstart,long PurchaseEntryId)
        {
            DataSet ds = new DataSet();
            if (Request.Form.Files["file"].Length > 0)
            {
                string fileExtension = System.IO.Path.GetExtension(Request.Form.Files["file"].FileName);
                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {

                    string Files = Request.Form.Files["file"].FileName;
                    Files = string.Concat(Path.GetFileNameWithoutExtension(Files), DateTime.Now.ToString("yyyyMMddHHmmssfff"), Path.GetExtension(Files));

                    string fileLocation = LegacyWeb.MapPath("~/uploads/excelitem/") + Files;//Request.Form.Files["file"].FileName;

                    string fileLoc = LegacyWeb.MapPath("~/uploads/excelitem/");
                    if (!Directory.Exists(fileLoc))
                        Directory.CreateDirectory(fileLoc);

                    Request.Form.Files["file"].SaveAs(fileLocation);
                    string excelConnectionString = string.Empty;
                    excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                    fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    //connection String for xls file format.
                    if (fileExtension == ".xls")
                    {
                        excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                        fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    }
                    //connection String for xlsx file format.
                    else if (fileExtension == ".xlsx")
                    {
                        excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                        fileLocation + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                    }
                    //Create Connection to Excel work book and add oledb namespace
                    OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                    excelConnection.Open();
                    DataTable dt = new DataTable();

                    dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    if (dt == null)
                    {
                        return null;
                    }

                    String[] excelSheets = new String[dt.Rows.Count];
                    int t = 0;
                    //excel data saves in temp file here.
                    foreach (DataRow row in dt.Rows)
                    {
                        excelSheets[t] = row["TABLE_NAME"].ToString();
                        t++;
                    }
                    OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);


                    string query = string.Format("Select * from [{0}]", excelSheets[0]);
                    using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                    {
                        dataAdapter.Fill(ds);
                    }



                    DataTable dtCol = new DataTable();
                    dtCol.Columns.Add("serialnumber");

                    var ViewModel = new SerialNoViewModel
                    {
                        PurchaseEntryId = PurchaseEntryId
                    };
                    Int32 chkCount = 0;
                    DataTable newdt = ds.Tables[0];
                    ViewModel.PurchaseEntryId = PurchaseEntryId;
                    if (1 == 1)
                    {
                        if (newdt.Rows.Count > 0)
                        {
                            Int32 ItmCount = 0;
                            var enable = db.EnableSettings.Where(a => a.EnableType == "Barcode").FirstOrDefault();
                            var brcheck = enable != null ? enable.Status : Status.inactive;
                            var UserId = User.Identity.GetUserId();
                            DateTime pdate = db.PurchaseEntrys.Where(o => o.PurchaseEntryId == PurchaseEntryId).OrderByDescending(o => o.PurchaseEntryId).Select(o => o.PEDate).FirstOrDefault();
                            List<SerialNoObj> objj = new List<SerialNoObj>();
                            for (int i = rowstart; i < newdt.Rows.Count; i++)
                            {
                                if (newdt.Rows[i][colstart] != DBNull.Value)
                                {

                                    SerialNoObj obj = new SerialNoObj();


                                    obj.SerialNo = newdt.Rows[i][colstart].ToString();
                                    obj.ExpiryDate = pdate.AddDays(365);
                                    obj.MfgDate = pdate;
                                    objj.Add(obj);

                                }




                            }
                            ViewModel.ItemId = item;
                            ViewModel.SerialNoObjs = objj;
                        }
                    }
                    //Item
                    var Item = db.Items.Select(s => new
                    {
                        Id = s.ItemID,
                        Name = s.ItemCode + "-" + s.ItemName,
                    }).ToList();
                    ViewBag.ddlItem = QkSelect.List(Item, "Id", "Name");
                    return View("GenerateSerialNo",ViewModel);


























                    return RedirectToAction("GenerateSerialNo", "PurchaseEntry");
                }
                else
                {
                    Danger("Please Upload Excel file..", false);
                    return RedirectToAction("GenerateSerialNo", "PurchaseEntry");
                }
            }
            else
            {
                Danger("Please Upload file..", false);
                return RedirectToAction("GenerateSerialNo", "PurchaseEntry");
            }
        }
        [HttpPost]
        public ActionResult GetVenderRate(string srchtxt)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var v =(from a in db.VenderRateMaster
                   join b in db.VenterRateDetails on a.VenderRateMasterId equals b.VenderRateMasterId 
                   join c in db.Suppliers on a.SupplierId equals c.SupplierID
                   where (srchtxt == "" || b.ExternalModal.Contains(srchtxt)|| b.InternalModal.Contains(srchtxt))
                   select new
                   {
                       b.VenterRateId,
                       c.SupplierName,
                       a.createdatae,
                       b.ItemType,
                       b.InternalModal,
                       b.ExternalModal,
                       b.Rate,
                       b.promorate,
                       b.promotiondescription

                   }
                   ).Distinct().OrderByDescending(a => a.createdatae).ThenBy(a => a.SupplierName);

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
        [QkAuthorize(Roles = "Dev,Vendor Rate Report")]
        public ActionResult VendorRateReport()
        {
            return View();
        }
        [QkAuthorize(Roles = "Dev,Vendor Rate Report")]
        public ActionResult GenerateVenderRate()
        {
            var ViewModel = new VenderRateViewModel
            {
               
            };

            var sup = db.Suppliers.Select(s => new
            {
                SupplierID = s.SupplierID,
                SupplierDetails = s.SupplierCode + " - " + s.SupplierName
            }).ToList();
            ViewBag.Supp = QkSelect.List(sup, "SupplierID", "SupplierDetails");

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

           

            return View(ViewModel);
        }
        public ActionResult GenerateSerialNo(long id, long? ItemId)
        {          
            var ViewModel = new SerialNoViewModel
            {
                PurchaseEntryId = id
            };

            ViewBag.ddlItem = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text ="", Value = ""},
                             }, "Value", "Text", 1);

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a =>a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            if (ItemId != null)
            {
                //Item
                var Item = db.Items.Select(s => new
                {
                    Id = s.ItemID,
                    Name = s.ItemCode + "-" + s.ItemName,
                }).ToList();
                ViewBag.ddlItem = QkSelect.List(Item, "Id", "Name");

                var BatchStock = (from a in db.BatchStocks
                                  join c in db.PurchaseEntrys
                                  on a.Reference equals c.PurchaseEntryId
                                  join d in db.Items
                                  on a.Item equals d.ItemID
                                  where (a.Reference == id && a.Item == ItemId && a.StockIn == 1)
                                  select new SerialNoObj
                                  {
                                      SerialNo      =   a.BatchNo,
                                      ExpiryDate    =   (a.EXP != null) ? a.EXP : DbFunctionsCompat.AddDays(c.PEDate, (int?)((d.daysexpirty != null)? d.daysexpirty : 360)),
                                      MfgDate       =   (a.MFG != null) ? a.MFG : c.PEDate,
                                      Quantity      =   a.StockIn,
                                      UnitId        =   a.Unit
                                  }).ToList();


                ViewModel.SerialNoObjs = BatchStock;

                //For getting rows that are saved in 'Batch Stock' from 'Purchase Entry Form'(Quantity > 1)
                //Quanity == 1 row will get in Line No. 4754
                 var BatchStockNw = (from a in db.BatchStocks
                                    join c in db.PurchaseEntrys
                                    on a.Reference equals c.PurchaseEntryId
                                    join d in db.Items
                                    on a.Item equals d.ItemID
                                    where (a.Reference == id && a.Item == ItemId && a.StockIn > 1)
                                    select new SerialNoObj
                                    {
                                            SerialNo    =   a.BatchNo,
                                            ExpiryDate  =   (a.EXP != null) ? a.EXP : DbFunctionsCompat.AddDays(c.PEDate, (int?)((d.daysexpirty != null) ? d.daysexpirty : 360)),
                                            MfgDate     =   (a.MFG != null) ? a.MFG : c.PEDate,
                                            Quantity    =   a.StockIn,
                                            UnitId      =   a.Unit
                                    }).ToList();

                foreach (var Row in BatchStockNw)
                {
                    //Have to show rows according to Quantity(If Quantity is 3 ==> 3 rows)
                    if (BatchStockNw != null && BatchStockNw.Count > 0)
                    {
                        for (var i = 0; i < Row.Quantity; i++)
                        {
                            var NewRow = new SerialNoObj
                            {
                                SerialNo    =   Row.SerialNo,
                                ExpiryDate  =   Row.ExpiryDate,
                                MfgDate     =   Row.MfgDate,
                                Quantity    =   1,
                                UnitId      =   Row.UnitId
                            };

                            //Thus appending these new rows with Quantity 1 rows (Line No. 4754)
                            ViewModel.SerialNoObjs.Add(NewRow);

                        }
                    }
                }

                if (ViewModel.SerialNoObjs != null && ViewModel.SerialNoObjs.Count!= 0)
                    ViewModel.Unit = ViewModel.SerialNoObjs[0].UnitId;

                if (BatchStock.Count == 0)
                {
                    ViewBag.Mode = "Create";
                }
            }           

            return View(ViewModel);
        }

        //-->POST(From Item Serial No)
        [HttpPost]
        public ActionResult GenerateSerialNo(SerialNoViewModel ViewModal)
        {
            long ItemId =   ViewModal.ItemId;
            var Today   =   Convert.ToDateTime(System.DateTime.Now);
            var RowNo   =   1;

            db.BatchStocks.RemoveRange(db.BatchStocks.Where(o => o.Reference == ViewModal.PurchaseEntryId && o.Item == ItemId && o.Type == "Purchase"));
            db.SaveChanges();

            foreach (var Row in ViewModal.SerialNoObjs)
            {
                    BatchStock Btst = new BatchStock();
                    Btst.BatchNo        =   Row.SerialNo;
                    Btst.Item           =   ItemId;
                    Btst.StockIn        =   1;
                    Btst.Order          =   RowNo;
                    Btst.EXP            =   Row.ExpiryDate;
                    Btst.MFG            =   Row.MfgDate;
                    Btst.Reference      =   ViewModal.PurchaseEntryId;
                    Btst.Type           =   "Purchase";
                    Btst.Unit           =   ViewModal.Unit;
                    Btst.CreatedDate    =   Today;
                    Btst.Date           =   Convert.ToDateTime(Row.MfgDate);

                    db.BatchStocks.Add(Btst);
                    db.SaveChanges();
                RowNo++;
            }

            return RedirectToAction("GenerateSerialNo/" + ViewModal.PurchaseEntryId, "PurchaseEntry");
        }



        [HttpPost]
        public ActionResult GenerateVenderRate(VenderRateViewModel ViewModal)
        {
            
            var Today = Convert.ToDateTime(System.DateTime.Now);
            var RowNo = 1;

            VenderRateMaster vmaster = new VenderRateMaster();
            vmaster.createdatae = Today;
            vmaster.SupplierId = ViewModal.supplierid;
            db.VenderRateMaster.Add(vmaster);
            db.SaveChanges();
            long vmasterid = vmaster.VenderRateMasterId;
           


            foreach (var Row in ViewModal.vendorNewRates)
            {
                var exist = db.VenterRateDetails.Any(o => o.ExternalModal == Row.ExternalModal && o.supplierid==ViewModal.supplierid);


                VenterRateDetails Btst = new VenterRateDetails();
                Btst.VenderRateMasterId = vmasterid;
                Btst.ExternalModal = Row.ExternalModal;
                Btst.InternalModal = Row.InternalModal;
                Btst.supplierid = ViewModal.supplierid;
                Btst.ItemType = Row.ItemType;
                Btst.Rate = Row.Rate;
                Btst.promorate = Row.promorate;
                Btst.promotiondescription = Row.promotiondescription;
                
               if(exist)
                {
                    db.VenterRateDetails.RemoveRange(db.VenterRateDetails.Where(o => o.ExternalModal == Row.ExternalModal && o.supplierid == ViewModal.supplierid));
                    db.SaveChanges();
                }
                db.VenterRateDetails.Add(Btst);
                db.SaveChanges();
               
            }

            return RedirectToAction("GenerateVenderRate/" + ViewModal.supplierid, "PurchaseEntry");
        }


        //Function to display Serial No. Details In Create Mode
        [HttpGet]
        public JsonResult GetSerialNoDtlInCreateMode(long PurchEntryId, long ItemId)
        {
            var ConD = (from a in db.Items
                        join b in db.PEItemss
                        on a.ItemID equals b.Item
                        join c in db.PurchaseEntrys
                        on b.PurchaseEntry equals c.PurchaseEntryId
                        where (c.PurchaseEntryId == PurchEntryId && a.ItemID == ItemId)
                        select new
                        {
                            a.daysexpirty,
                            MfgDate     =   c.PEDate,
                            Quantity    =   b.ItemQuantity,
                            ExpiryDate  =   c.PEDate.AddDays(a.daysexpirty ?? 360),
                            UnitId      =   b.ItemUnit
                        }).ToList();
            return Json(ConD);
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
