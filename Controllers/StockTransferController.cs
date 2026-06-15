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
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic;
using System.Net;
using System.Text;
using System.Data;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Drawing;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class StockTransferController : BaseController
    {
        public StockTransferController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        ApplicationDbContext db;
        Common com;
        // GET: StockTransfer
        [QkAuthorize(Roles = "Dev,StockTransfer List")]
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
                        string path = LegacyWeb.MapPath("~/uploads/stocktransfer/");
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        for (int i = 0; i < files.Count; i++)
                        {
                            IFormFile file = files[i];
                            if (file.Length > 0)
                            {


                                var fileCount = db.StockTransfers.Select(a => a.Id).AsEnumerable().DefaultIfEmpty(20000).Max();

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
                                    thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/stocktransfer/"), thumbName);

                                    resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/stocktransfer/"), resizeName);
                                    newFName = "resize_" + newFName;
                                    FStatus = Status.inactive;
                                }
                                else
                                {
                                    var commonfilename = "Docs-Thump.png";

                                }
                                string Realname = newName;

                                newName = Path.Combine(LegacyWeb.MapPath("~/uploads/stocktransfer/"), newName);
                                if (System.IO.File.Exists(newName))
                                {
                                    //delete existing file
                                    System.IO.File.Delete(newName);
                                }
                                file.SaveAs(newName);


                                var stocktransferdoc = new AttachmentDocuments
                                {
                                    TransactionID = id,
                                    TransactionType = "StockTransfer",
                                    FileName = newFName,
                                    Status = FStatus,
                                    CreatedDate = Convert.ToDateTime(System.DateTime.Now)
                                };
                                db.AttachmentDocuments.Add(stocktransferdoc);
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
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/stocktransfer/"), resizeName);
                                        thumbs.Save(resizeName);
                                    }
                                    else
                                    {
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/stocktransfer/"), resizeName);
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
        [QkAuthorize(Roles = "Dev,StockTransfer Entry")]
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

            var warningMsg = db.EnableSettings.Where(a => a.EnableType == "WarningOnSave").FirstOrDefault();
            var warnmsg = warningMsg != null ? (warningMsg.Status == Status.active ? 0 : 1) : 1;
            ViewBag.WarnMsg = warnmsg;

            var STTo = new StockTransferViewModel
            {
                Voucher = InvoiceNo(),
                Date = DateTime.Now
            };
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            var userpermission = User.IsInRole("All StockTransfers Entry");
            ViewBag.LastEntry = db.StockTransfers.Where(p => (userpermission == true || p.CreatedBy == UserId)).Select(p => p.Id).AsEnumerable().DefaultIfEmpty(0).Max();

            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
            var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;
            ViewBag.StockTrnsfrUpdate = brcheck;
            if (brcheck == Status.active)
            {

                //                where (a.McId == mcchk2)
                //                    ID = b.EmployeeId,
                //                    Name = b.FirstName + " " + b.LastName
                //                 where (a.UserId == UserId)
                //                     ID = a.EmployeeId,
                //                     Name = a.FirstName + " " + a.LastName
                //              post => post.UserId,
                //              meta => meta.UserId,


                var appby = db.Employees.Where(a => a.UserStatus == true)
                      .Select(s => new
                      {
                          ID = s.EmployeeId,
                          Name = s.FirstName + " " + s.LastName
                      })
                      .ToList();
                ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");
            }
            else
            {
                var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                      .Select(s => new
                      {
                          ID = s.EmployeeId,
                          Name = s.FirstName + " " + s.LastName
                      })
                      .ToList();
                ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");
            }

            var MlaSTran = db.EnableSettings.Where(a => a.EnableType == "MLASTran").FirstOrDefault();
            var MlaSTrans = MlaSTran != null ? MlaSTran.Status : Status.inactive;
            ViewBag.MLASTran = MlaSTrans;

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;

            //field mapping
            STTo.FieldMap = db.FieldMappings.Where(a => a.Section == "StkTrans" && a.Status == Status.active).ToList();
            companySet();

            var FDate = db.FinancialYears.Select(a => a.Start).FirstOrDefault();
            STTo.StkDate = String.Format("{0:dd-MM-yyyy}", STTo.Date);
            STTo.FinancialDate = String.Format("{0:dd-MM-yyyy}", FDate);
            return View(STTo);
        }

        [QkAuthorize(Roles = "Dev,StockTransfer Entry")]
        public ActionResult Createmobile()
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

            var warningMsg = db.EnableSettings.Where(a => a.EnableType == "WarningOnSave").FirstOrDefault();
            var warnmsg = warningMsg != null ? (warningMsg.Status == Status.active ? 0 : 1) : 1;
            ViewBag.WarnMsg = warnmsg;

            var STTo = new StockTransferViewModel
            {
                Voucher = InvoiceNo(),
                Date = DateTime.Now
            };
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            var userpermission = User.IsInRole("All StockTransfers Entry");
            ViewBag.LastEntry = db.StockTransfers.Where(p => (userpermission == true || p.CreatedBy == UserId)).Select(p => p.Id).AsEnumerable().DefaultIfEmpty(0).Max();

            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;
            var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
            var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;
            ViewBag.StockTrnsfrUpdate = brcheck;
            if (brcheck == Status.active)
            {

                //                where (a.McId == mcchk2)
                //                    ID = b.EmployeeId,
                //                    Name = b.FirstName + " " + b.LastName
                //                 where (a.UserId == UserId)
                //                     ID = a.EmployeeId,
                //                     Name = a.FirstName + " " + a.LastName
                //              post => post.UserId,
                //              meta => meta.UserId,


                var appby = db.Employees.Where(a => a.UserStatus == true)
                      .Select(s => new
                      {
                          ID = s.EmployeeId,
                          Name = s.FirstName + " " + s.LastName
                      })
                      .ToList();
                ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");
            }
            else
            {
                var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                      .Select(s => new
                      {
                          ID = s.EmployeeId,
                          Name = s.FirstName + " " + s.LastName
                      })
                      .ToList();
                ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");
            }

            var MlaSTran = db.EnableSettings.Where(a => a.EnableType == "MLASTran").FirstOrDefault();
            var MlaSTrans = MlaSTran != null ? MlaSTran.Status : Status.inactive;
            ViewBag.MLASTran = MlaSTrans;

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;

            //field mapping
            STTo.FieldMap = db.FieldMappings.Where(a => a.Section == "StkTrans" && a.Status == Status.active).ToList();
            companySet();

            var FDate = db.FinancialYears.Select(a => a.Start).FirstOrDefault();
            STTo.StkDate = String.Format("{0:dd-MM-yyyy}", STTo.Date);
            STTo.FinancialDate = String.Format("{0:dd-MM-yyyy}", FDate);
            return View(STTo);
        }

        [RedirectingAction]
        [HttpPost]
        [QkAuthorize(Roles = "Dev,StockTransfer Entry")]
        public JsonResult Create(string[][] array, string[] mtdata, string action, StockTransferBSundryViewModel bsmodel)
        {
            var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
            var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;

            if (brcheck == Status.active)
            {
                DummyStkTrsItem2 dItem = new DummyStkTrsItem2();
                foreach (var arr in array)
                {
                    
                    dItem.Item = Convert.ToInt64(arr[0]);
                    dItem.Unit = arr[1] != null ? (long?)Convert.ToInt64(arr[1]) : null;
                    dItem.Quantity = Convert.ToDecimal(arr[2]);
                    dItem.Price = Convert.ToDecimal(arr[3]);
                    dItem.Amount = Convert.ToDecimal(arr[5]);
                    
                }
                var Appby = Convert.ToString(mtdata[9]);
                if (Appby == null || Appby == "")
                {
                    string msgg = "Select Approved By";
                    bool statt = false;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = statt, message = msgg } };
                }
            }
         

            string billno = (mtdata[0]);
            var Billcheck = db.StockTransfers.Where(a => a.Voucher == billno).Any();
            bool stat = false;
            string msg = "";
            if (!Billcheck)
            {
                var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
                var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

                var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
                var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

                var UserId = User.Identity.GetUserId();
                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                var CurrentDate = Convert.ToDateTime(System.DateTime.Now);


                StockTransfer sl = new StockTransfer();

                sl.STNo = GetVTNo();
                sl.Voucher = (mtdata[0]);
                sl.Date = DateTime.Parse(mtdata[1], new CultureInfo("en-GB"));
                sl.MCFrom = Convert.ToInt64(mtdata[2]);
                sl.MCTo = Convert.ToInt64(mtdata[3]);
                sl.Remarks = mtdata[4] != "" ? (mtdata[4]) : "";
                sl.TotalAmount = Convert.ToDecimal(mtdata[8]);
                sl.CreatedDate = CurrentDate;
                sl.CreatedBy = UserId;
                sl.Status = Status.active;
                sl.editable = choice.Yes;
                sl.Branch = Convert.ToInt64(BranchID);

                string str = mtdata[10];
                StockType Stype = (StockType)Enum.Parse(typeof(StockType), str);
                sl.StockType = Stype;
                sl.Ref1 = Convert.ToString(mtdata[11]);
                sl.Ref2 = Convert.ToString(mtdata[12]);
                sl.Ref3 = Convert.ToString(mtdata[13]);
                sl.Ref4 = Convert.ToString(mtdata[14]);
                sl.Ref5 = Convert.ToString(mtdata[15]);


                db.StockTransfers.Add(sl);
                db.SaveChanges();

                Int64 STId = sl.Id;
                StockTransferItem mt = new StockTransferItem();

                //To Update the quantity in Create Mode(ItemTransaction Table)
                com.addlog(LogTypes.Created, UserId, "StockTransfer", "StockTransfers", findip(), STId, "Successfully added StockTransfer details");

                stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
                 brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;

                if (brcheck == Status.active)
                {
                    DummyStkTrsItem2 dItem = new DummyStkTrsItem2();
                    foreach (var arr in array)
                    {
                        dItem.StockTransferId = STId;
                        dItem.Item = Convert.ToInt64(arr[0]);
                        dItem.Unit = arr[1] != null ? (long?)Convert.ToInt64(arr[1]) : null;
                        dItem.Quantity = Convert.ToDecimal(arr[2]);
                        dItem.Price = Convert.ToDecimal(arr[3]);
                        dItem.Amount = Convert.ToDecimal(arr[5]);
                        db.DummyStkTrsItem2.Add(dItem);
                        db.SaveChanges();
                    }
                }
                else
                {
                    foreach (var arr in array)
                    {
                        mt.StockTransferId = STId;
                        mt.Item = Convert.ToInt64(arr[0]);
                        mt.Unit = arr[1] != null ? (long?)Convert.ToInt64(arr[1]) : null;
                        mt.Quantity = Convert.ToDecimal(arr[2]);
                        mt.Price = Convert.ToDecimal(arr[3]);
                        mt.Amount = Convert.ToDecimal(arr[5]);
                        db.StockTransferItems.Add(mt);
                        db.SaveChanges();
                    }
                }
                var sen = bsmodel.sebsundrys;
                if (sen != null)
                {
                    StockTransferBSundry mtbs = new StockTransferBSundry();
                    foreach (var bs in bsmodel.sebsundrys)
                    {
                        mtbs.StockTransferId = STId;
                        mtbs.BillSundry = bs.BillSundry;
                        mtbs.BsValue = bs.BsValue;
                        mtbs.AmountType = bs.AmountType;
                        mtbs.BsType = bs.BsType;
                        mtbs.BsAmount = bs.BsAmount;
                        db.StockTransferBSundrys.Add(mtbs);
                        db.SaveChanges();
                    }
                }



                //Approved By
                var Appby = Convert.ToString(mtdata[9]);
                if (Appby != null && Appby != "")
                {
                    long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                    Approval approval = new Approval();
                    foreach (var emp in Approve)
                    {
                        approval.TransEntry = STId;
                        approval.Type = "StockTransfer";
                        approval.EmployeeId = emp;
                        db.Approvals.Add(approval);
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
                    if (STId != 0)
                    {
                        var StockTrans = com.StockTransferData(STId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, ComHeadCheck);
                        item = StockTrans["item"];
                        summary = StockTrans["summary"];
                        billsundry = StockTrans["billsundry"];
                        cdetails = StockTrans["cdetails"];
                    }
                    var def = Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                    def = def == 0 ? 1 : def;
                    var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, cdetails, layout, fmapp, STId } };
                }
                else
                {
                    msg = "Stock Transfer Successfully Transfered..";
                    stat = true;
                   
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, STId = STId } };
                }
            }
            else
            {
                msg = "Voucher No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

        }
        [QkAuthorize(Roles = "Dev,Edit StockTransfer")]
        public ActionResult Edit(long? id)
        {
            
        ViewBag.image = (from b in db.AttachmentDocuments
                             join c in db.StockTransfers on b.TransactionID equals c.Id
                             where c.Id == id &&  b.TransactionType == "StockTransfer"
                             select new quotationdocumentviewmodel
                             {
                                 qutid = b.DocumentID,
                                quotationID = b.TransactionID,
                                 FileName = b.FileName,
                             }).ToList();

            var mcs = db.MCs.Select(s => new
            {
                McId = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.MCbag = QkSelect.List(mcs, "McId", "Name");

            var UserId = User.Identity.GetUserId();

            var today = DateTime.Now;
            var editableDay = DateTime.Now;
            var tem = 0;
            var userEditDays = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.stkdays).FirstOrDefault();
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
                editableDay = today.AddDays(-userEditDays);
            }


            StockTransfer stk = db.StockTransfers.Where(x =>x.Id == id).FirstOrDefault();
            if ((stk.Date - editableDay).TotalDays < 0 || tem == 1)
            {
                return NotFound();

            }


            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            if (mcchk != null)
            {
                var mcs1 = db.MCs.Select(s => new
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

            var warningMsg = db.EnableSettings.Where(a => a.EnableType == "WarningOnSave").FirstOrDefault();
            var warnmsg = warningMsg != null ? (warningMsg.Status == Status.active ? 0 : 1) : 1;
            ViewBag.WarnMsg = warnmsg;



            var MlaSTran = db.EnableSettings.Where(a => a.EnableType == "MLASTran").FirstOrDefault();
            var MlaSTrans = MlaSTran != null ? MlaSTran.Status : Status.inactive;
            ViewBag.MLASTran = MlaSTrans;

            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            StockTransferViewModel vmodel = new StockTransferViewModel();

            vmodel = (from a in db.StockTransfers
                      join b in db.MCs on a.MCFrom equals b.MCId into mc
                      from b in mc.DefaultIfEmpty()
                      join c in db.MCs on a.MCTo equals c.MCId into mct
                      from c in mc.DefaultIfEmpty()
                      where a.Id == id

                      select new StockTransferViewModel
                      {
                          Voucher = a.Voucher,
                          Date = a.Date,
                          MCFrom = a.MCFrom,
                          MCTo = a.MCTo,
                          Remarks = a.Remarks,
                          TotalAmount = a.TotalAmount,
                          Ref1 = a.Ref1,
                          Ref2 = a.Ref2,
                          Ref3 = a.Ref3,
                          Ref4 = a.Ref4,
                          Ref5 = a.Ref5,
                          StockType = a.StockType
                      }).FirstOrDefault();

            var FDate = db.FinancialYears.Select(a => a.Start).FirstOrDefault();
            vmodel.StkDate = String.Format("{0:dd-MM-yyyy}", vmodel.Date);
            vmodel.FinancialDate = String.Format("{0:dd-MM-yyyy}", FDate);

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
            ViewBag.ChkApp = false; com.chkApproved((long)id, EditPermission, "StockTransfer", UserId);

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            var EnableBatchWise = db.EnableSettings.Where(a => a.EnableType == "BatchWiseStock").FirstOrDefault();
            var BatchCheck = EnableBatchWise != null ? EnableBatchWise.Status : Status.inactive;
            ViewBag.BatchWiseStock = BatchCheck;

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "StkTrans" && a.Status == Status.active).ToList();

            var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
            var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;
            ViewBag.StockTrnsfrUpdate = brcheck;
            var dummyvalue = db.DummyStkTrsItem2.Where(a => a.StockTransferId == id).FirstOrDefault();
            if (dummyvalue != null)
            {
                ViewBag.itemAproval = 1;
            }
            else
            {
                ViewBag.itemAproval = 0;
            }
            if (brcheck == Status.inactive)
            {
                //dummy table operations
                var DItem = db.DummyStkTrsItems.Where(a => a.StockTransferId == id).FirstOrDefault();
                var SItem = db.StockTransferItems.Where(a => a.StockTransferId == id).FirstOrDefault();
                if (SItem == null && DItem != null)
                {
                    var DItems = db.DummyStkTrsItems.Where(a => a.StockTransferId == id).ToList();
                    foreach (var arr in DItems)
                    {
                        //add to se-item table
                        StockTransferItem sItem = new StockTransferItem();
                        sItem.Unit = arr.Unit;
                        sItem.Quantity = arr.Quantity;
                        sItem.Price = arr.Price;
                        sItem.Amount = arr.Amount;
                        sItem.StockTransferId = arr.StockTransferId;
                        sItem.Item = arr.Item;
                        db.StockTransferItems.Add(sItem);
                        db.SaveChanges();
                    }

                    db.DummyStkTrsItems.RemoveRange(db.DummyStkTrsItems.Where(a => a.StockTransferId == id));
                    db.SaveChanges();
                }

                var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "StockTransfer").Select(a => a.EmployeeId).ToList();
                long[] empIds = emp.ToArray();

                var appby = db.Employees.Where(a => a.UserStatus == true )
                      .Select(s => new
                      {
                          FieldName = s.EmployeeId,
                          FieldID = s.FirstName + " " + s.LastName
                      })
                      .ToList();
                ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);
            }
            else
            {
                var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "StockTransfer").Select(a => a.EmployeeId).ToList();
                long[] empIds = emp.ToArray();

                var appby = db.Employees.Where(a => a.UserStatus == true)
                      .Select(s => new
                      {
                          FieldName = s.EmployeeId,
                          FieldID = s.FirstName + " " + s.LastName
                      })
                      .ToList();
                ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);
                //                where (a.McId == mcchk2)
                //                    ID = b.EmployeeId,
                //                    Name = b.FirstName + " " + b.LastName
                //                 where (a.UserId == UserId)
                //                     ID = a.EmployeeId,
                //                     Name = a.FirstName + " " + a.LastName
                //              post => post.UserId,
                //              meta => meta.UserId,


            }
            return View(vmodel);
        }

        [QkAuthorize(Roles = "Dev,Edit StockTransfer")]
        public ActionResult Editmobile(long? id)
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

            var warningMsg = db.EnableSettings.Where(a => a.EnableType == "WarningOnSave").FirstOrDefault();
            var warnmsg = warningMsg != null ? (warningMsg.Status == Status.active ? 0 : 1) : 1;
            ViewBag.WarnMsg = warnmsg;



            var MlaSTran = db.EnableSettings.Where(a => a.EnableType == "MLASTran").FirstOrDefault();
            var MlaSTrans = MlaSTran != null ? MlaSTran.Status : Status.inactive;
            ViewBag.MLASTran = MlaSTrans;

            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            StockTransferViewModel vmodel = new StockTransferViewModel();

            vmodel = (from a in db.StockTransfers
                      join b in db.MCs on a.MCFrom equals b.MCId into mc
                      from b in mc.DefaultIfEmpty()
                      join c in db.MCs on a.MCTo equals c.MCId into mct
                      from c in mc.DefaultIfEmpty()
                      where a.Id == id

                      select new StockTransferViewModel
                      {
                          Voucher = a.Voucher,
                          Date = a.Date,
                          MCFrom = a.MCFrom,
                          MCTo = a.MCTo,
                          Remarks = a.Remarks,
                          TotalAmount = a.TotalAmount,
                          Ref1 = a.Ref1,
                          Ref2 = a.Ref2,
                          Ref3 = a.Ref3,
                          Ref4 = a.Ref4,
                          Ref5 = a.Ref5,
                          StockType = a.StockType
                      }).FirstOrDefault();

            var FDate = db.FinancialYears.Select(a => a.Start).FirstOrDefault();
            vmodel.StkDate = String.Format("{0:dd-MM-yyyy}", vmodel.Date);
            vmodel.FinancialDate = String.Format("{0:dd-MM-yyyy}", FDate);

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
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "StkTrans" && a.Status == Status.active).ToList();

            var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
            var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;
            ViewBag.StockTrnsfrUpdate = brcheck;
            var dummyvalue = db.DummyStkTrsItem2.Where(a => a.StockTransferId == id).FirstOrDefault();
            if (dummyvalue != null)
            {
                ViewBag.itemAproval = 1;
            }
            else
            {
                ViewBag.itemAproval = 0;
            }
            if (brcheck == Status.inactive)
            {
                //dummy table operations
                var DItem = db.DummyStkTrsItems.Where(a => a.StockTransferId == id).FirstOrDefault();
                var SItem = db.StockTransferItems.Where(a => a.StockTransferId == id).FirstOrDefault();
                if (SItem == null && DItem != null)
                {
                    var DItems = db.DummyStkTrsItems.Where(a => a.StockTransferId == id).ToList();
                    foreach (var arr in DItems)
                    {
                        //add to se-item table
                        StockTransferItem sItem = new StockTransferItem();
                        sItem.Unit = arr.Unit;
                        sItem.Quantity = arr.Quantity;
                        sItem.Price = arr.Price;
                        sItem.Amount = arr.Amount;
                        sItem.StockTransferId = arr.StockTransferId;
                        sItem.Item = arr.Item;
                        db.StockTransferItems.Add(sItem);
                        db.SaveChanges();
                    }

                    db.DummyStkTrsItems.RemoveRange(db.DummyStkTrsItems.Where(a => a.StockTransferId == id));
                    db.SaveChanges();
                }

                var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "StockTransfer").Select(a => a.EmployeeId).ToList();
                long[] empIds = emp.ToArray();

                var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                      .Select(s => new
                      {
                          FieldName = s.EmployeeId,
                          FieldID = s.FirstName + " " + s.LastName
                      })
                      .ToList();
                ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);
            }
            else
            {
                var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "StockTransfer").Select(a => a.EmployeeId).ToList();
                long[] empIds = emp.ToArray();

                var appby = db.Employees.Where(a => a.UserStatus == true)
                      .Select(s => new
                      {
                          FieldName = s.EmployeeId,
                          FieldID = s.FirstName + " " + s.LastName
                      })
                      .ToList();
                ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);
                //                where (a.McId == mcchk2)
                //                    ID = b.EmployeeId,
                //                    Name = b.FirstName + " " + b.LastName
                //                 where (a.UserId == UserId)
                //                     ID = a.EmployeeId,
                //                     Name = a.FirstName + " " + a.LastName
                //              post => post.UserId,
                //              meta => meta.UserId,


            }
            return View(vmodel);
        }

        [RedirectingAction]
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit StockTransfer")]
        public JsonResult Edit(string[][] array, string[] mtdata, string action, SEBillSundryViewModel bsmodel)
        {
            bool stat = false;
            string msg;
            string Vou = mtdata[0];
            var Exists = db.StockTransfers.Any(c => c.Voucher == Vou);
            var UserId = User.Identity.GetUserId();
            var CurrentDate = Convert.ToDateTime(System.DateTime.Now);

            var TheId = Convert.ToInt64(mtdata[9]);
            StockTransfer STs = db.StockTransfers.Find(TheId);

            if (BillExist(Convert.ToString(mtdata[0])) && Convert.ToString(mtdata[0]) != STs.Voucher)
            {
                msg = "Invoice No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

            var EditPermission = User.IsInRole("Disable StkTransfer Edit After Approval");
            if (1==1)
            {
                STs.Voucher = mtdata[0];
                STs.Date = DateTime.Parse(mtdata[1], new CultureInfo("en-GB"));
                STs.MCFrom = Convert.ToInt64(mtdata[2]);
                STs.MCTo = Convert.ToInt64(mtdata[3]);
                STs.Remarks = mtdata[4];
                STs.TotalAmount = Convert.ToDecimal(mtdata[8]);

                string str = mtdata[11];
                StockType Stype = (StockType)Enum.Parse(typeof(StockType), str);
                STs.StockType = Stype;


                STs.Ref1 = Convert.ToString(mtdata[12]);
                STs.Ref2 = Convert.ToString(mtdata[13]);
                STs.Ref3 = Convert.ToString(mtdata[14]);
                STs.Ref4 = Convert.ToString(mtdata[15]);
                STs.Ref5 = Convert.ToString(mtdata[16]);

                db.Entry(STs).State = EntityState.Modified;
                Int64 MtId = STs.Id;
                com.addlog(LogTypes.Updated, UserId, "StockTransfer", "StockTransfers", findip(), MtId, "StockTransfer Updated Successfully");
                //To Update the quantity in Edit Mode(ItemTransaction Table)               
                com.ItemTransInEditMode("StockTransfer", 0, STs.MCFrom, STs.MCTo, array, TheId, UserId, CurrentDate);


                var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
                var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;

                if(1==1)
                {
                    var ST = db.StockTransferItems.Where(a => a.StockTransferId == MtId).FirstOrDefault();

                    if (ST != null)
                    {

                        var SItems = db.StockTransferItems.Where(a => a.StockTransferId == MtId).ToList();

                        foreach (var arr in SItems)
                        {
                            //add to dummy table
                            DummyStkTrsItem dItem = new DummyStkTrsItem();
                            dItem.Unit = arr.Unit;
                            dItem.Quantity = arr.Quantity;
                            dItem.Price = arr.Price;
                            dItem.Amount = arr.Amount;
                            dItem.StockTransferId = arr.StockTransferId;
                            dItem.Item = arr.Item;
                            db.DummyStkTrsItems.Add(dItem);
                            db.SaveChanges();
                        }

                        db.StockTransferItems.RemoveRange(db.StockTransferItems.Where(a => a.StockTransferId == MtId));
                        db.SaveChanges();
                    }

                    if (brcheck == Status.active)
                    {
                        db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(o => o.TransEntry == MtId && o.Type == "StockTransfer"));
                        db.SaveChanges();
                        db.DummyStkTrsItem2.RemoveRange(db.DummyStkTrsItem2.Where(a => a.StockTransferId == MtId));
                        db.SaveChanges();
                    }
                        foreach (var arr in array)
                    {
                        if (brcheck == Status.active)
                        {

                            DummyStkTrsItem2 dItem = new DummyStkTrsItem2();

                            dItem.Item = Convert.ToInt64(arr[0]);
                            dItem.Unit = arr[1] != null ? (long?)Convert.ToInt64(arr[1]) : null;
                            dItem.Quantity = Convert.ToDecimal(arr[2]);
                            dItem.Price = Convert.ToDecimal(arr[3]);
                            dItem.Amount = Convert.ToDecimal(arr[5]);
                            dItem.StockTransferId = MtId;


                            db.DummyStkTrsItem2.Add(dItem);
                            var ret = db.SaveChanges();
                            if (ret > 0)
                            {
                                db.DummyStkTrsItems.RemoveRange(db.DummyStkTrsItems.Where(a => a.StockTransferId == MtId));
                                db.SaveChanges();
                            }

                        }
                        else
                        {
                            if (ST != null)
                            {
                                ST.Item = Convert.ToInt64(arr[0]);
                                ST.Unit = arr[1] != null ? (long?)Convert.ToInt64(arr[1]) : null;
                                ST.Quantity = Convert.ToDecimal(arr[2]);
                                ST.Price = Convert.ToDecimal(arr[3]);
                                ST.Amount = Convert.ToDecimal(arr[5]);
                                ST.StockTransferId = MtId;


                                db.StockTransferItems.Add(ST);
                               
                            }
                            else
                            {
                                StockTransferItem STn = new StockTransferItem();
                                STn.Item = Convert.ToInt64(arr[0]);
                                STn.Unit = arr[1] != null ? (long?)Convert.ToInt64(arr[1]) : null;
                                STn.Quantity = Convert.ToDecimal(arr[2]);
                                STn.Price = Convert.ToDecimal(arr[3]);
                                STn.Amount = Convert.ToDecimal(arr[5]);
                                STn.StockTransferId = MtId;


                                db.StockTransferItems.Add(STn);
                                
                            }

                            var ret = db.SaveChanges();
                            if (ret > 0)
                            {
                                db.DummyStkTrsItems.RemoveRange(db.DummyStkTrsItems.Where(a => a.StockTransferId == MtId));
                                db.SaveChanges();
                            }
                        }
                    }
                }


                var mtbs = db.StockTransferBSundrys.Where(a => a.StockTransferId == MtId).FirstOrDefault();
                if (mtbs != null)
                {
                    db.StockTransferBSundrys.RemoveRange(db.StockTransferBSundrys.Where(a => a.StockTransferId == MtId));
                    db.SaveChanges();
                }
                var sen = bsmodel.sebsundrys;
                if (sen != null)
                {
                    StockTransferBSundry mtbsry = new StockTransferBSundry();
                    foreach (var bs in bsmodel.sebsundrys)
                    {
                        mtbsry.StockTransferId = MtId;
                        mtbsry.BillSundry = bs.BillSundry;
                        mtbsry.BsValue = bs.BsValue;
                        mtbsry.AmountType = bs.AmountType;
                        mtbsry.BsType = bs.BsType;
                        mtbsry.BsAmount = bs.BsAmount;
                        db.StockTransferBSundrys.Add(mtbsry);
                        db.SaveChanges();
                    }
                }




                msg = "Successfully Updated StockTransfer.";
                stat = true;


                //Approved By
                if (brcheck == Status.active)
                {
                    var tempo = db.Approvals.Where(a => a.TransEntry == MtId && a.Type == "StockTransfer").FirstOrDefault();
                    if (tempo != null)
                    {
                        db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == MtId && a.Type == "StockTransfer"));
                        db.SaveChanges();
                    }
                    var Appby = Convert.ToString(mtdata[10]);
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = MtId;
                            approval.Type = "StockTransfer";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    var empuser = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                    var chkapp = db.Approvals.Where(a => a.EmployeeId == empuser && a.TransEntry == MtId && a.Type == "StockTransfer").FirstOrDefault();

                    var MrnPO = db.Approvals.Where(a => a.TransEntry == MtId && a.Type == "StockTransfer").FirstOrDefault();
                    if (MrnPO != null)
                    {
                        if (chkapp != null)
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.EmployeeId != empuser && a.TransEntry == MtId && a.Type == "StockTransfer"));
                            db.SaveChanges();
                        }
                        else
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == MtId && a.Type == "StockTransfer"));
                            db.SaveChanges();
                        }
                    }
                    var Appby = Convert.ToString(mtdata[10]);
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = MtId;
                            approval.Type = "StockTransfer";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }
                }

            }

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

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
                if (TheId != 0)
                {
                    var StockTrans = com.StockTransferData(TheId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, ComHeadCheck);
                    item = StockTrans["item"];
                    summary = StockTrans["summary"];
                    billsundry = StockTrans["billsundry"];
                    cdetails = StockTrans["cdetails"];
                }
                var def = Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
                def = def == 0 ? 1 : def;
                var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, cdetails, layout, fmapp , TheId } };
            }
            else
            {
                msg = "Stock Transfer Successfully Updated..";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, TheId } };
            }
        }
        private bool BillExist(string STNo)
        {
            var Exists = db.StockTransfers.Any(c => c.Voucher == (STNo));
            bool res = (Exists) ? true : false;
            return res;

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
        private string InvoiceNo(Int64 SENo = 0, string billNo = "")
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "StockTransfer").Select(a => a.prefix).FirstOrDefault();
            if (billNo == "" || billNo == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "StockTransfer").Select(a => a.number).FirstOrDefault();
                if ((db.StockTransfers.Select(p => p.STNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    SENo = db.StockTransfers.Max(p => p.STNo + 1);
                    billNo = companyPrefix + SENo;
                    if (BillExist(billNo))
                    {
                        billNo = InvoiceNo(SENo, billNo);
                    }
                }
            }
            else
            {
                SENo = SENo + 1;
                billNo = companyPrefix + SENo;
                if (BillExist(billNo))
                {
                    billNo = InvoiceNo(SENo, billNo);
                }

            }
            return billNo;
        }

        [RedirectingAction]
        [HttpPost]
        [QkAuthorize(Roles = "Dev,StockTransfer List")]
        public ActionResult GetStockTransfer(string Voucher, string FromDate, string ToDate, long? MFrom, long? MTo, string appstat)
        {
            bool isranan = db.companys.Any(o => o.CPName.Contains("RANNAN TAILORING"));

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
            var uEdit = User.IsInRole("Edit Sales Entry");
            var uDelete = User.IsInRole("Delete Sales Entry");

            // EF Core 10 cannot translate the nested-collection / GroupBy-latest projections (the `app`,
            // `AppStatus`, `chkAppStatus` lets) inside the executed select. Split SERVER from CLIENT:
            // materialize only entity columns + simple scalars into serverRows, build client lookups keyed by
            // StockTransfer Id, then re-project client-side with the SAME member names + order.
            var serverQuery = (from a in db.StockTransfers
                     join b in db.MCs on a.MCFrom equals b.MCId into mc
                     from b in mc.DefaultIfEmpty()
                     join c in db.MCs on a.MCTo equals c.MCId into mct
                     from c in mct.DefaultIfEmpty()
                     join d in db.Users on a.CreatedBy equals d.Id into usr
                     from d in usr.DefaultIfEmpty()

                         // app/AppStatus/chkAppStatus (nested collections + GroupBy-latest) are computed
                         // client-side after materialization — EF Core 10 can't translate them inside this query.

                     where
                     (Voucher == "" || a.Voucher == Voucher) &&
                     (FromDate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                     (ToDate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                     (MFrom == 0 || MFrom == null || a.MCFrom == MFrom) &&
                     //(MTo == 0 || MTo == null || MTo == null || a.MCTo == MTo)
                     (MTo == 0 || MTo == null || MTo == a.MCTo)
                     //&& (userpermission == true || a.CreatedBy == UserId)
                     select new
                     {
                         validornot = tem != 1 && (EF.Functions.DateDiffDay(a.Date, editableDay) <= 0 && EF.Functions.DateDiffDay(a.Date, today) >= 0) ? "valid" : "invalid",
                         userEditDays = userEditDays,
                         Id = a.Id,
                         Voucher = a.Voucher,
                         Date = a.Date,
                         MCFrom = b.MCName,
                         MCTo = c.MCName,
                         Remarks = a.Remarks,
                         TotalAmount = a.TotalAmount,
                         User = d.UserName,
                         a.CreatedDate,
                         CreatedBy=a.CreatedBy,
                     });

            // Performance (audit P2, hybrid): server paging when no search, plain-column sort, AND no client-side
            // filter is active (the conditions below mirror the filters' own guards); else original path.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase){ "CreatedBy","CreatedDate","Date","Id","MCFrom","MCTo","Remarks","TotalAmount","User","userEditDays","validornot","Voucher" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0 && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn)) && !(appstat != "");
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn) ? serverQuery.OrderBy("Id asc") : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

            // CLIENT-side lookups keyed by StockTransfer Id (missing key -> empty, no KeyNotFound).
            var stIds = serverRows.Select(o => o.Id).ToList();
            // app = approver EmployeeIds (nested collection, keyed by TransEntry == StockTransfer Id).
            var appLookup = db.Approvals
                .Where(x => x.Type == "StockTransfer" && stIds.Contains(x.TransEntry))
                .Select(x => new { x.TransEntry, x.EmployeeId })
                .ToList()
                .ToLookup(x => x.TransEntry);
            // AppStatus = all ApprovalUpdate statuses; raw rows materialized once and reused for chkAppStatus.
            var appUpdRows = db.ApprovalUpdates
                .Where(x => x.Type == "StockTransfer" && stIds.Contains(x.TransEntry))
                .Select(x => new { x.TransEntry, x.ApprovalStatus, x.ApprovedBy, x.CreatedDate })
                .ToList();
            var appStatusLookup = appUpdRows.ToLookup(x => x.TransEntry);
            // chkAppStatus = latest ApprovalUpdate status per ApprovedBy (GroupBy-latest-per-group), per transfer.
            var chkAppStatusLookup = appUpdRows
                .GroupBy(x => x.TransEntry)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(l => l.ApprovedBy)
                    .Select(grp => grp.OrderByDescending(c => c.CreatedDate).First())
                    .Select(x => x.ApprovalStatus).ToList());

            var v = serverRows.
            Select(b => new
            {
                b.validornot,
                b.userEditDays,
                b.Id,
                b.Voucher,
                b.Date,
                b.MCFrom,
                b.MCTo,
                b.Remarks,
                b.TotalAmount,
                b.User,

                Dev = uDev,
                Edit = uEdit,
                Delete = uDelete,

                app = appLookup[b.Id].Select(x => x.EmployeeId).ToList(),
                AppStatus = appStatusLookup[b.Id].Select(x => x.ApprovalStatus).ToList(),
                chkAppStatus = chkAppStatusLookup.TryGetValue(b.Id, out var ck) ? ck : new List<ApprovalStatus>(),
                b.CreatedDate,
                b.CreatedBy,
            }).ToList().Select(o => new
            {
                o.validornot,
                o.userEditDays,
                o.Id,
                o.Voucher,
                o.Date,
                o.MCFrom,
                o.MCTo,
                o.Remarks,
                o.TotalAmount,
                o.User,
                o.Dev,
                o.Edit,
                o.Delete,
                o.app,
                o.CreatedDate,
                o.CreatedBy,
                Approval = (o.app != null && empl.EmployeeId != null) ? (o.app.Contains(empl.EmployeeId) ? true : false) : false,
                ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,
                CreatedUserToEdit=(o.CreatedBy== UserId) ? true : false,
            });
            if (appstat != "")
            {
                v = v.Where(a => a.ApprovalStatus == AppSt);
            }
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.Voucher.ToString().ToLower().Contains(search.ToLower()));
            }
            if (!fastPage && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (isranan == true && sortColumn == "Voucher")
                {
                    v = v.OrderByDescending(o=>long.Parse(o.Voucher) );
                }
                else
                {
                    v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
                }
            }
            if (!fastPage) { recordsTotal = v.Count(); }
            var data = (fastPage ? v.ToList() : v.Skip(skip).Take(pageSize).ToList());
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        [HttpGet]
        public ActionResult GetSTItems(long EntryID)
        {
            var ConD = (from a in db.StockTransferItems
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.StockTransferId == EntryID
                        select new
                        {
                            a.Item,
                            a.Quantity,
                            a.Unit,
                            a.Price,
                            a.Amount,

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
                        }).AsEnumerable().Select(o => new
                        {
                            o.Item,
                            o.Quantity,
                            o.Unit,
                            o.Price,
                            o.Amount,
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
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

        [HttpGet]
        public JsonResult GetSTItems2(long EntryID)
        {
            var ConD = (from a in db.DummyStkTrsItem2
                        join b in db.Items on a.Item equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.StockTransferId == EntryID
                        select new
                        {
                            a.Item,
                            a.Quantity,
                            a.Unit,
                            a.Price,
                            a.Amount,

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
                        }).AsEnumerable().Select(o => new
                        {
                            o.Item,
                            o.Quantity,
                            o.Unit,
                            o.Price,
                            o.Amount,
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
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
            return Json(ConD);
        }

        [HttpGet]
        public JsonResult GetApprovalby(long mc)
        {
            var ConD = (from a in db.purchaseapproval
                        join b in db.Employees on a.UserId equals b.UserId

                        where (a.McId == mc)
                        select new
                        {
                            ID = b.EmployeeId,
                            Name = b.FirstName + " " + b.LastName
                        }).ToList();
            var ConD2 = (from a in db.MCs
                         join b in db.Employees on a.AssignedUser equals b.UserId

                         where (a.MCId == mc)
                         select new
                         {
                             ID = b.EmployeeId,
                             Name = b.FirstName + " " + b.LastName
                         }).ToList();

            var ConD3 = ConD.ToList();
            return Json(ConD3);
        }

        [HttpGet]
        public JsonResult GetSTBillSundry(long EntryID)
        {
            var SEBs = (from a in db.StockTransferBSundrys
                        join c in db.BillSundrys on a.BillSundry equals c.BillSundryId
                        where a.StockTransferId == EntryID
                        select new
                        {
                            a.AmountType,
                            a.BillSundry,
                            a.BsAmount,
                            a.BsType,
                            a.BsValue,
                            c.BSName
                        }).ToList();
            return Json(SEBs);
        }


        [RedirectingAction]
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Delete StockTransfer")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockTransfer mtd = db.StockTransfers.Find(id);

            if (mtd == null)
            {
                return NotFound();
            }
            return PartialView(mtd);
        }

        [RedirectingAction]
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete StockTransfer")]
        public ActionResult DeleteConfirmed(long Id)
        {
            bool stat = false;
            string msg;

            #region Old Code
            #endregion

            var chk = DeleteST(Id);
            if (chk == true)
            {
                stat = true;
                msg = "Successfully deleted Stock Transfer Entry details.";
            }
            else
            {
                stat = false;
                msg = "Looks like something went wrong. Please check your form.";
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete StockTransfer")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteST(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + " Stock Transfer Entry.", true);
            return RedirectToAction("Index", "StockTransfer");
        }


        private Boolean DeleteST(long Id)
        {

            var UserId = User.Identity.GetUserId();
            var CurrentDate = Convert.ToDateTime(System.DateTime.Now);

            StockTransfer SEen = db.StockTransfers.Find(Id);
            var SEItem = db.StockTransferItems.Where(a => a.StockTransferId == Id);
            var SEItemdummy = db.DummyStkTrsItem2.Where(a => a.StockTransferId == Id);

            var SEBs = db.StockTransferBSundrys.Where(a => a.StockTransferId == Id).FirstOrDefault();

            if (SEItem != null)
            {
                db.StockTransferItems.RemoveRange(db.StockTransferItems.Where(a => a.StockTransferId == Id));
            }
            if(SEItemdummy!=null)
            {

                db.DummyStkTrsItem2.RemoveRange(db.DummyStkTrsItem2.Where(a => a.StockTransferId == Id));
            }

            if (SEBs != null)
            {
                db.StockTransferBSundrys.RemoveRange(db.StockTransferBSundrys.Where(a => a.StockTransferId == Id));
            }
            var appr = db.Approvals.Where(a => a.TransEntry == Id && a.Type == "StockTransfer").FirstOrDefault();
            if (appr != null)
            {
                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == Id && a.Type == "StockTransfer"));
            }
            var app = db.ApprovalUpdates.Where(a => a.TransEntry == Id && a.Type == "StockTransfer").FirstOrDefault();
            if (app != null)
            {
                db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == Id && a.Type == "StockTransfer"));
            }

            /***************** Item Transaction ******************/
            if (SEen != null)
                com.ItemTransInDeleteMode("StockTransfer", 0, SEen.MCFrom, SEen.MCTo, Id, UserId, CurrentDate);

            db.StockTransfers.Remove(SEen);
            db.SaveChanges();
            com.addlog(LogTypes.Deleted, UserId, "StockTransfer", "StockTransfers", findip(), SEen.Id, "Successfully Deleted StockTransfer Entry");

            return true;
        }
        [HttpGet]
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "StockTransfer" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
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
            var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
            var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;

            if (brcheck == Status.active)
            {
                if (App.ApprovalStatus == ApprovalStatus.Approved)
                {
                    var SItems = db.DummyStkTrsItem2.Where(a => a.StockTransferId == id).ToList();
                    foreach (var arr in SItems)
                    {
                        var exist = db.StockTransferItems.Any(o => o.Item == arr.Item && o.StockTransferId == arr.StockTransferId&&o.Quantity==arr.Quantity);
                        if (!exist)
                        {
                            StockTransferItem dItem = new StockTransferItem();
                            dItem.Unit = arr.Unit;
                            dItem.Quantity = arr.Quantity;
                            dItem.Price = arr.Price;
                            dItem.Amount = arr.Amount;
                            dItem.StockTransferId = arr.StockTransferId;
                            dItem.Item = arr.Item;
                            db.StockTransferItems.Add(dItem);
                            db.SaveChanges();
                        }
                    }
                    db.DummyStkTrsItem2.RemoveRange(db.DummyStkTrsItem2.Where(a => a.StockTransferId == id));
                    db.SaveChanges();
                }
            }

            var MR = db.StockTransfers.Where(a => a.Id == id).FirstOrDefault();

            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "StockTransfer").OrderByDescending(a => a.CreatedDate).FirstOrDefault();

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
                AppUp.Type = "StockTransfer";

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
                            join d in db.StockTransfers on b.TransEntry equals d.Id into team
                            from d in team.DefaultIfEmpty()
                            join e in db.Employees on b.RequestBy equals e.UserId into emp
                            from e in emp.DefaultIfEmpty()
                            join u in db.Users on d.CreatedBy equals u.Id into req
                            from u in req.DefaultIfEmpty()
                            where b.TransEntry == MCId && b.Type == "StockTransfer"
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

        [RedirectingAction]
        [HttpGet]
        //[QkAuthorize(Roles = "Dev,View StockTransfer")]
        public ActionResult Details(long? id)
        {
            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            var MOP = db.Receipts.Where(x => x.ReceiptId == id).Select(y => y.MOPayment).FirstOrDefault();
            ViewBag.MOPayment = (MOP == ModeOfPayment.Cash) ? 0 : 1;

            Journal rpt = db.Journals.Find(id);

            StockTransferViewModel vmodel = new StockTransferViewModel();

            vmodel = (from b in db.StockTransfers
                      join c in db.MCs on b.MCFrom equals c.MCId into pay
                      from c in pay.DefaultIfEmpty()
                      join d in db.MCs on b.MCTo equals d.MCId into payfrom
                      from d in payfrom.DefaultIfEmpty()
                      where b.Id == id
                      select new StockTransferViewModel
                      {
                          From = c.MCName,
                          To = d.MCName,
                          Date = b.Date,
                          Voucher = b.Voucher,
                          Remarks = b.Remarks,
                          Emp = (from ab in db.Approvals
                                 join bb in db.Employees on ab.EmployeeId equals bb.EmployeeId
                                 where ab.TransEntry == id && ab.Type == "StockTransfer"
                                 select new ApprovalViewModel
                                 {
                                     EmpName = bb.FirstName + " " + bb.LastName
                                 }).ToList(),
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                      }).FirstOrDefault();
            var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
            var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;
            if (brcheck == Status.active)
            {
                var dummyvalue = db.DummyStkTrsItem2.Where(a => a.StockTransferId == id).FirstOrDefault();
                if (dummyvalue != null)
                {
                    vmodel.STItem = (from a in db.DummyStkTrsItem2
                                     join b in db.Items on a.Item equals b.ItemID into ite
                                     from b in ite.DefaultIfEmpty()
                                     join p in db.ItemUnits on a.Unit equals p.ItemUnitID into proj
                                     from p in proj.DefaultIfEmpty()
                                     where a.StockTransferId == id
                                     select new StockTransferItemViewModel
                                     {
                                         Itemname = b.ItemCode + "-" + b.ItemName,
                                         Price = a.Price,
                                         Unitname = p.ItemUnitName,
                                         Quantity = a.Quantity,
                                         Amount = a.Amount
                                     }).ToList();
                }
                else
                {
                    vmodel.STItem = (from a in db.StockTransferItems
                                     join b in db.Items on a.Item equals b.ItemID into ite
                                     from b in ite.DefaultIfEmpty()
                                     join p in db.ItemUnits on a.Unit equals p.ItemUnitID into proj
                                     from p in proj.DefaultIfEmpty()
                                     where a.StockTransferId == id
                                     select new StockTransferItemViewModel
                                     {
                                         Itemname = b.ItemCode + "-" + b.ItemName,
                                         Price = a.Price,
                                         Unitname = p.ItemUnitName,
                                         Quantity = a.Quantity,
                                         Amount = a.Amount
                                     }).ToList();
                }
            }
            else
            {
                vmodel.STItem = (from a in db.StockTransferItems
                                 join b in db.Items on a.Item equals b.ItemID into ite
                                 from b in ite.DefaultIfEmpty()
                                 join p in db.ItemUnits on a.Unit equals p.ItemUnitID into proj
                                 from p in proj.DefaultIfEmpty()
                                 where a.StockTransferId == id
                                 select new StockTransferItemViewModel
                                 {
                                     Itemname = b.ItemCode + "-" + b.ItemName,
                                     Price = a.Price,
                                     Unitname = p.ItemUnitName,
                                     Quantity = a.Quantity,
                                     Amount = a.Amount
                                 }).ToList();
            }
            vmodel.STbs = db.PEBillSundrys.Where(a => a.PurchaseEntry == id)
           .Select(b => new StockTransferBSundryViewModel
           {
               AmountType = b.AmountType,
               BsAmount = b.BsAmount,
               BsType = b.BsType,
               BsValue = b.BsValue,
               BType = b.BsType == 0 ? "Add" : "Less",
               BAmount = b.AmountType == 0 ? "" : "%",
               name = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
           }).ToList();

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "StkTrans" && a.Status == Status.active).ToList();

            return View(vmodel);
        }


        [HttpGet]
        //[QkAuthorize(Roles = "Dev,Download StockTransfer")]
        public ActionResult Download(long id)
        {
            var SaleDet = db.StockTransfers.Where(s => s.Id == id).FirstOrDefault();

            var billno = SaleDet.Voucher;

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), "inactive");
            return File(ms, "application/pdf", "StockTransfer Invoice" + "-" + billno + ".pdf");

        }
        public StringBuilder generatePdf(long id)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

            var saleData = com.StockTransferDatanew(id, InPrintItemCode, PartNoCheck, TimeOut);
            var item = saleData.pdfItem.ToList();
            var summary = saleData;
            var billsundry = saleData.billsundry.ToList();

            return com.generatepdf(id, summary, item, billsundry, "Stock Transfer");
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

