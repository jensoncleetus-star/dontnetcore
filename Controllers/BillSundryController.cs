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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class BillSundryController : BaseController
    {
        // GET: BillSundry
        ApplicationDbContext db;
        Common com;
        public BillSundryController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        [QkAuthorize(Roles = "Dev,BillSundry")]
        public ActionResult Index()
        {
            ViewBag.ItStatus = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Active", Value="0"},
                new SelectListItem() {Text = "Inactive", Value="1"},
            }, "Value", "Text");
            ViewBag.AmtType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Absolute Amount", Value="0"},
                new SelectListItem() {Text = "Percentage", Value="1"},
            }, "Value", "Text");
            ViewBag.Type = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Additive", Value="0"},
                new SelectListItem() {Text = "Subtractive", Value="1"},
            }, "Value", "Text");

            var ThText = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                              }, "Value", "Text", 1);
            ViewBag.Acc = ThText;

            return View();
        }
        [QkAuthorize(Roles = "Dev,BillSundry")]
        public JsonResult GetBillSundry(string BSName, decimal? DefValue, string Amount_Type, string Type, string Stats,long? SAcc, long? PAcc)
        {            
            AmountType At=new AmountType();
            Status st = new Status();
            BSType Bt = new BSType();
            if (Stats != "")
            {
                st = (Stats == "0") ? Status.active : Status.inactive;
            };
            if (Amount_Type != "")
            {
                At = (Amount_Type == "0") ? AmountType.AbsoluteAmount : AmountType.Percentage;
            };
            if (Type != "")
            {
                Bt = (Type == "0") ? BSType.Additive : BSType.Subtractive;
            };
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

            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit BillSundry");
            var uDelete = User.IsInRole("Delete BillSundry");

            var ModList = (from a in db.BillSundrys
                           join c in db.Users on a.CreatedBy equals c.Id into user
                           from c in user.DefaultIfEmpty()
                           join d in db.BSNatures on a.BSNature equals d.BSNatureId into ntr
                           from d in ntr.DefaultIfEmpty()
                           join e in db.Accountss on a.SAccount equals e.AccountsID into sacc
                           from e in sacc.DefaultIfEmpty()
                           join f in db.Accountss on a.PAccount equals f.AccountsID into pacc
                           from f in pacc.DefaultIfEmpty()
                           where (BSName == null || BSName == "" || BSName == a.BSName) &&
                                 (DefValue == null || DefValue == 0 || DefValue == a.DefaultValue) &&
                                 (Amount_Type == null || Amount_Type == "" || a.AmountType == At) &&
                                 (Stats == null || Stats == "" || a.Status == st) &&
                                 (Type == null || Type == "" || a.BSType == Bt) &&
                                 (SAcc == null || SAcc == 0 || a.SAccount == SAcc) && 
                                 (PAcc == null || PAcc == 0 || a.PAccount == PAcc) 
                           select new
                           {
                               id = a.BillSundryId,
                               a.BSName,
                               a.BSType,
                               a.DefaultValue,
                               a.Status,
                               Nature = d.BSNatureName,
                               User=c.Name,
                               AmountType=a.AmountType,
                               SAccName= e.Name,
                               PAccName= f.Name,
                               Dev = uDev,
                               Edit = uEdit,
                               Delete = uDelete,
                               chckstat = (db.PEBillSundrys.Any(c => c.BillSundry == a.BillSundryId)) ? 1 : (db.PRBillSundrys.Any(c => c.BillSundry == a.BillSundryId)) ? 1 : (db.SEBillSundrys.Any(c => c.BillSundry == a.BillSundryId)) ? 1 : (db.SRBillSundrys.Any(c => c.BillSundry == a.BillSundryId)) ? 1 : (db.MtFromPartyBSundrys.Any(c => c.BillSundry == a.BillSundryId)) ? 1 : (db.MtToPartyBSundrys.Any(c => c.BillSundry == a.BillSundryId)) ? 1 : (db.PFBillSundrys.Any(c => c.BillSundry == a.BillSundryId)) ? 1 : (db.POBillSundrys.Any(c => c.BillSundry == a.BillSundryId)) ? 1 : (db.QtBillSundrys.Any(c => c.BillSundry == a.BillSundryId)) ? 1 : 0,
                           });
            ////search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                ModList = ModList.Where(p => p.id.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.BSName.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.BSType.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.DefaultValue.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Status.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.User.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.AmountType.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Nature.ToString().ToLower().Contains(search.ToLower()));

            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                ModList = ModList.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = ModList.Count();
            var data = ModList.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        [QkAuthorize(Roles = "Dev,Create BillSundry")]
        public ActionResult Create()
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

            ViewBag.SetAccount = QkSelect.List(
                      new List<SelectListItem>
                      {
                                    new SelectListItem { Selected = false, Text = "Select Account", Value = "0"},
                      }, "Value", "Text", 1);
                var acc = db.Accountss.Where(a => a.AccountsID != 4 && a.Group != 12 && a.Group != 14 && a.Group != 23 && a.Status == Status.active).Select(s => new
                {
                    ID = s.AccountsID,
                    Name = s.Name
                }).ToList();
                ViewBag.SetAccount = QkSelect.List(acc, "ID", "Name");
    
           
                var acc2 = db.Accountss.Where(a => a.AccountsID != 4 && a.Group != 23 && a.Status == Status.active).Select(s => new
                {
                    ID = s.AccountsID,
                    Name = s.Name
                }).ToList();
                ViewBag.SetAccount = QkSelect.List(acc2, "ID", "Name");
            
    return PartialView();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create BillSundry")]
        [ValidateAntiForgeryToken]
        public JsonResult Create(BillSundry BS)
        {
            bool stat = false;
            string msg;
            long Branch = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.BillSundrys.Any(c => c.BSName == BS.BSName);
              
                if (Exists)
                {
                    msg = "Bill Sundrys already exists.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();                    
                    Int64 bsNature = db.BSNatures.Where(a => a.BSNatureId == 1).Select(a => a.BSNatureId).SingleOrDefault();
                    var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                    var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                    if (BranchCheck == Status.active)
                    {
                        Branch = BS.Branch;
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    }

                    var bls = new BillSundry
                    {
                        BSName = BS.BSName,
                        BSType = BS.BSType,
                        BSNature= bsNature,
                        DefaultValue=BS.DefaultValue!=null? BS.DefaultValue:0,
                        CreatedBy = UserId,
                        CreatedDate = Convert.ToDateTime(System.DateTime.Now),
                        Branch = Branch,
                        AmountType=BS.AmountType,
                        Status = BS.Status,
                        SAccount = BS.SAccount,
                        PAccount = BS.PAccount,
                    };
                    db.BillSundrys.Add(bls);
                    db.SaveChanges();

                    
                    com.addlog(LogTypes.Created, UserId, "BillSundry", "BillSundrys", findip(), bls.BillSundryId, "Bill Sundry Added Successfully");


                    msg = "Successfully added Bill Sundry details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        // GET: dep/Edit/5
        [QkAuthorize(Roles = "Dev,Edit BillSundry")]
        public ActionResult Edit(long? id)
        {
            ViewBag.billsId = id;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BillSundry bls = db.BillSundrys.Find(id);
            if (bls == null)
            {
                return NotFound();
            }
            BillSundry BS = new BillSundry();

            BS.BillSundryId = bls.BillSundryId;
            BS.BSName = bls.BSName;
            BS.BSType = bls.BSType;
            BS.DefaultValue = bls.DefaultValue;
            BS.Status = bls.Status;
            BS.AmountType = bls.AmountType;
            BS.Branch = bls.Branch;
            BS.SAccount = bls.SAccount != null ? bls.SAccount : 0;
            BS.PAccount = bls.PAccount != null ? bls.PAccount : 0;

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

            var Nature = db.BillSundrys
                          .Select(s => new
                          {
                              FieldID = s.BSName,
                              FieldName = s.BillSundryId
                          })
                          .ToList();
            ViewBag.Nature = QkSelect.List(Nature, "FieldName", "FieldID");

            if (BS.SAccount != 0)
            {
                var acc = db.Accountss.Where(a => a.AccountsID != 4 && a.Group != 12 && a.Group != 14 && a.Group != 23 && a.Status == Status.active).Select(s => new
                {
                    ID = s.AccountsID,
                    Name = s.Name
                }).ToList();
                ViewBag.SetAccount = QkSelect.List(acc, "ID", "Name");
            }
            else
            {
                ViewBag.SetAccount = QkSelect.List(
                         new List<SelectListItem>
                         {
                                    new SelectListItem { Selected = false, Text = "Select Account", Value = "0"},
                         }, "Value", "Text", 1);
            }

            if (BS.PAccount != 0)
            {
                var acc = db.Accountss.Where(a => a.AccountsID != 4 && a.Group != 12 && a.Group != 14 && a.Group != 23 && a.Status == Status.active).Select(s => new
                {
                    ID = s.AccountsID,
                    Name = s.Name
                }).ToList();
                ViewBag.SetAccount = QkSelect.List(acc, "ID", "Name");
            }
            else
            {
                ViewBag.SetAccount = QkSelect.List(
                         new List<SelectListItem>
                         {
                                    new SelectListItem { Selected = false, Text = "Select Account", Value = "0"},
                         }, "Value", "Text", 1);
            }
            var chckstat = (db.PEBillSundrys.Any(c => c.BillSundry == id)) ? 1 : (db.PRBillSundrys.Any(c => c.BillSundry == id)) ? 1 : (db.SEBillSundrys.Any(c => c.BillSundry == id)) ? 1 : (db.SRBillSundrys.Any(c => c.BillSundry == id)) ? 1 : 0;
            ViewBag.stat = chckstat;
            return PartialView(BS);
        }
        public ActionResult GetBillSundryUsedList(long? idd)
        {
            var UserId = User.Identity.GetUserId();
            var today = DateTime.Now;
            var editableDay = DateTime.Now;
            var tem = 0;
            var userEditDays = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.days).FirstOrDefault();
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
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;
            var v1 = (from a in db.PEBillSundrys
                      join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId into primary
                      from c in primary.DefaultIfEmpty()
                      where a.BillSundry == idd
                      select new
                      {
                          validornot = "valid",
                          userEditDays = 0,
                          id = (c.PurchaseEntryId == null) ? 0 : c.PurchaseEntryId,
                          billNo = c.BillNo,
                          text = "Purchase Entry",
                      }
                   ).ToList();
            var v2 = (from a in db.PRBillSundrys
                      join c in db.PurchaseReturns on a.PurchaseReturnId equals c.PurchaseReturnId into primary
                      from c in primary.DefaultIfEmpty()
                      where a.BillSundry == idd
                      select new
                      {
                          validornot = "valid",
                          userEditDays = 0,
                          id = (c.PurchaseReturnId == null) ? 0 : c.PurchaseReturnId,
                          billNo = c.BillNo,
                          text = "Purchase Return",
                      }
                   ).ToList();
            var v3 = (from a in db.SEBillSundrys
                      join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId into primary
                      from c in primary.DefaultIfEmpty()
                      where a.BillSundry == idd
                      select new
                      {
                          validornot = tem != 1 && (EF.Functions.DateDiffDay(c.SEDate, editableDay) <= 0 && EF.Functions.DateDiffDay(c.SEDate, today) >= 0) ? "valid" : "invalid",
                          userEditDays = userEditDays,
                          id = (c.SalesEntryId == null) ? 0 : c.SalesEntryId,
                          billNo = c.BillNo,
                          text = "Sales Entry",
                      }
                   ).ToList();
            var v4 = (from a in db.SRBillSundrys
                      join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId into primary
                      from c in primary.DefaultIfEmpty()
                      where a.BillSundry == idd
                      select new
                      {
                          validornot = "valid",
                          userEditDays = 0,
                          id = (c.SalesReturnId == null) ? 0 : c.SalesReturnId,
                          billNo = c.BillNo,
                          text = "Sales Return",
                      }
                   ).ToList();
            var v5 = (from a in db.MtFromPartyBSundrys
                      join c in db.MtFromPartys on a.MtFromParty equals c.MtFromPartyId into primary
                      from c in primary.DefaultIfEmpty()
                      where a.BillSundry == idd
                      select new
                      {
                          validornot = "valid",
                          userEditDays = 0,
                          id = (c.MtFromPartyId == null) ? 0 : c.MtFromPartyId,
                          billNo = c.VoucherNo,
                          text = "Materials Received From Party Entry",
                      }
                   ).ToList();
            var v6 = (from a in db.MtToPartyBSundrys
                      join c in db.MtToPartys on a.MtToParty equals c.MtToPartyId into primary
                      from c in primary.DefaultIfEmpty()
                      where a.BillSundry == idd
                      select new
                      {
                          validornot = "valid",
                          userEditDays = 0,
                          id = (c.MtToPartyId == null) ? 0 : c.MtToPartyId,
                          billNo = c.VoucherNo,
                          text = "Materials Issued to Party Entry",
                      }
                   ).ToList();
            var v7 = (from a in db.POBillSundrys
                      join c in db.PurchaseOrders on a.PurchaseOrder equals c.PurchaseOrderId into primary
                      from c in primary.DefaultIfEmpty()
                      where a.BillSundry == idd
                      select new
                      {
                          validornot = "valid",
                          userEditDays = 0,
                          id = (c.PurchaseOrderId == null) ? 0 : c.PurchaseOrderId,
                          billNo = c.BillNo,
                          text = "Purchase Order",
                      }
                   ).ToList();
            var v8 = (from a in db.QtBillSundrys
                      join c in db.Quotations on a.Quotation equals c.QuotationId into primary
                      from c in primary.DefaultIfEmpty()
                      where a.BillSundry == idd
                      select new
                      {
                          validornot = "valid",
                          userEditDays = 0,
                          id = (c.QuotationId == null) ? 0 : c.QuotationId,
                          billNo = c.BillNo,
                          text = "Quotation",
                      }
                   ).ToList();
            var v9 = (from a in db.PFBillSundrys
                      join c in db.ProFormas on a.ProForma equals c.ProFormaId into primary
                      from c in primary.DefaultIfEmpty()
                      where a.BillSundry == idd
                      select new
                      {
                          validornot = "valid",
                          userEditDays = 0,
                          id = (c.ProFormaId == null) ? 0 : c.ProFormaId,
                          billNo = c.BillNo,
                          text = "Proforma",
                      }
                  ).ToList();

            var uni = v1.Union(v2).Union(v3).Union(v4).Union(v5).Union(v6).Union(v7).Union(v8).Union(v9);
            uni = uni.Where(a => a.billNo != null).ToList();






            recordsTotal = uni.Count();
            var data = uni.ToList();

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
        public ActionResult GetBillSundryUsedList2(long? idd)
        {
            var UserId = User.Identity.GetUserId();
            var today = DateTime.Now;
            var editableDay = DateTime.Now;
            var tem = 0;
            var userEditDays = db.UserEditDayss.Where(a => a.userid == UserId).Select(a => a.days).FirstOrDefault();
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
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;
            var v1 = (from a in db.PEBillSundrys
                      join c in db.PurchaseEntrys on a.PurchaseEntry equals c.PurchaseEntryId into primary
                      from c in primary.DefaultIfEmpty()
                      where a.BillSundry == idd
                      select new
                      {
                          validornot = "valid",
                          userEditDays = 0,
                          id = (c.PurchaseEntryId == null) ? 0 : c.PurchaseEntryId,
                          billNo = c.BillNo,
                          text = "Purchase Entry",
                      }
                   ).ToList();
            var v2 = (from a in db.PRBillSundrys
                      join c in db.PurchaseReturns on a.PurchaseReturnId equals c.PurchaseReturnId into primary
                      from c in primary.DefaultIfEmpty()
                      where a.BillSundry == idd
                      select new
                      {
                          validornot = "valid",
                          userEditDays = 0,
                          id = (c.PurchaseReturnId == null) ? 0 : c.PurchaseReturnId,
                          billNo = c.BillNo,
                          text = "Purchase Return",
                      }
                   ).ToList();
            var v3 = (from a in db.SEBillSundrys
                      join c in db.SalesEntrys on a.SalesEntry equals c.SalesEntryId into primary
                      from c in primary.DefaultIfEmpty()
                      where a.BillSundry == idd
                      select new
                      {
                          validornot = tem != 1 && (EF.Functions.DateDiffDay(c.SEDate, editableDay) <= 0 && EF.Functions.DateDiffDay(c.SEDate, today) >= 0) ? "valid" : "invalid",
                          userEditDays = userEditDays,
                          id = (c.SalesEntryId == null) ? 0 : c.SalesEntryId,
                          billNo = c.BillNo,
                          text = "Sales Entry",
                      }
                   ).ToList();
            var v4 = (from a in db.SRBillSundrys
                      join c in db.SalesReturns on a.SalesReturnId equals c.SalesReturnId into primary
                      from c in primary.DefaultIfEmpty()
                      where a.BillSundry == idd
                      select new
                      {
                          validornot = "valid",
                          userEditDays = 0,
                          id = (c.SalesReturnId == null) ? 0 : c.SalesReturnId,
                          billNo = c.BillNo,
                          text = "Sales Return",
                      }
                   ).ToList();
            //          where a.BillSundry == idd
            //              id = (c.MtFromPartyId == null) ? 0 : c.MtFromPartyId,
            //              billNo = c.VoucherNo,
            //              text = "Materials Received From Party Entry",
            //          where a.BillSundry == idd
            //              id = (c.MtToPartyId == null) ? 0 : c.MtToPartyId,
            //              billNo = c.VoucherNo,
            //              text = "Materials Issued to Party Entry",
            //          where a.BillSundry == idd
            //              id = (c.PurchaseOrderId == null) ? 0 : c.PurchaseOrderId,
            //              billNo = c.BillNo,
            //              text = "Purchase Order",
            //          where a.BillSundry == idd
            //              id = (c.QuotationId == null) ? 0 : c.QuotationId,
            //              billNo = c.BillNo,
            //              text = "Quotation",
            //          where a.BillSundry == idd
            //              id = (c.ProFormaId == null) ? 0 : c.ProFormaId,
            //              billNo = c.BillNo,
            //              text = "Proforma",

            var uni = v1.Union(v2).Union(v3).Union(v4);//.Union(v5).Union(v6).Union(v7).Union(v8).Union(v9);
            uni = uni.Where(a => a.billNo != null).ToList();







            recordsTotal = uni.Count();
            var data = uni.ToList();

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
        // POST: department/Edit/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit BillSundry")]
        [ValidateAntiForgeryToken]
        public JsonResult Edit(BillSundry BS, long id)
        {
            bool stat = false;
            string msg;
            long Branch = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.BillSundrys.Any(c => c.BSName == BS.BSName && c.BillSundryId != id);
                if (Exists)
                {
                    msg = "Bill Sundry already exists.";
                    stat = false;
                }
                else if (id==BS.BSNature)
                {
                    msg = "Bill Sundry And Nature Should be Different.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                    var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                    if (BranchCheck == Status.active)
                    {
                        Branch = BS.Branch;
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    }
                    BillSundry bls = db.BillSundrys.Find(id);
                    Int64 bsNature = db.BSNatures.Where(a => a.BSNatureId == 1).Select(a => a.BSNatureId).SingleOrDefault();

                    bls.BSName = BS.BSName;
                    bls.BSType = BS.BSType;
                    bls.DefaultValue = BS.DefaultValue != null ? BS.DefaultValue : 0;
                    bls.Status = BS.Status;
                    bls.BSNature = bsNature;
                    bls.AmountType = BS.AmountType;
                    bls.Branch = Branch;
                    bls.SAccount = BS.SAccount != null ? BS.SAccount : 0;
                    bls.PAccount = BS.PAccount != null ? BS.PAccount : 0;

                    db.Entry(bls).State = EntityState.Modified;
                    db.SaveChanges();
                   
                    com.addlog(LogTypes.Updated, UserId, "BillSundry", "BillSundrys", findip(), bls.BillSundryId, "Bill Sundry Updated Successfully");

                    msg = "Successfully updated Bill Sundry details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        // GET: Desg/Delete/5
        [QkAuthorize(Roles = "Dev,Delete BillSundry")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BillSundry bls = db.BillSundrys.Find(id);
            if (bls == null)
            {
                return NotFound();
            }
            if (bls.editable == choice.No)
            {
                return NotFound();
            }

            return PartialView(bls);
        }

        // POST: Field/Delete/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete BillSundry")]
        public JsonResult Delete(int id, IFormCollection collection)
        {
            bool stat = false;
            string msg;
            var Editable = db.BillSundrys.Any(a => a.editable == choice.No && a.BillSundryId == id);
            if (Editable)
            {
                msg = "Sorry, It is a Pre-defined BillSundry And Cannot be Deleted.";
                stat = false;
            }
            else
            {
                var Msg = chkDeleteWithMsg(id);
                if (Msg != null)
                {
                    msg = Msg;
                    stat = false;
                }
                else
                {
                    stat = DeleteFn(id);
                    msg = "Successfully Deleted Bill Sundry details.";
                }
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete BillSundry")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteItem(arr) == true) ? count++ : notdel++;
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " BillSundry, Unable to Delete " + notdel + " BillSundry. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " BillSundry.", true);
            }
            else
            {
                Success("Deleted " + count + " BillSundry.", true);
            }
            return RedirectToAction("Index", "BillSundry");
        }
        private Boolean DeleteItem(long id)
        {
            //     || db.SEBillSundrys.Any(c => c.BillSundry == id) || db.SRBillSundrys.Any(c => c.BillSundry == id)
            //     || db.MtFromPartyBSundrys.Any(c => c.BillSundry == id) || db.MtToPartyBSundrys.Any(c => c.BillSundry == id)
            //     || db.PFBillSundrys.Any(c => c.BillSundry == id) || db.POBillSundrys.Any(c => c.BillSundry == id)
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
            if (db.PEBillSundrys.Any(c => c.BillSundry == id))
            {
                msg = "BillSundry Already used in Purchase !!";
            }
            else if (db.PRBillSundrys.Any(c => c.BillSundry == id))
            {
                msg = "BillSundry Already used in Purchase Return !!";
            }
            else if (db.SEBillSundrys.Any(c => c.BillSundry == id))
            {
                msg = "BillSundry Already used in Sales Entry !!";
            }
            else if (db.SRBillSundrys.Any(c => c.BillSundry == id))
            {
                msg = "BillSundry Already used in Sales Return !!";
            }
            else if (db.MtFromPartyBSundrys.Any(c => c.BillSundry == id))
            {
                msg = "Item Already used in Materials Received From Party Entry !!";
            }
            else if (db.MtToPartyBSundrys.Any(c => c.BillSundry == id))
            {
                msg = "Item Already used in Materials Issued to Party Entry !!";
            }
            else if (db.PFBillSundrys.Any(c => c.BillSundry == id))
            {
                msg = "BillSundry Already used in Purchase Entry !!";
            }
            else if (db.POBillSundrys.Any(c => c.BillSundry == id))
            {
                msg = "Account Already used in Purchase Order !!";
            }
            else if (db.QtBillSundrys.Any(c => c.BillSundry == id))
            {
                msg = "Account Already used in Quotation !!";
            }
            else
            {
                msg = null;
            }

            return msg;
        }


        public bool DeleteFn(long id)
        {
            BillSundry bls = db.BillSundrys.Find(id);
            if (bls != null)
            {
                db.BillSundrys.RemoveRange(db.BillSundrys.Where(a => a.BillSundryId == id));


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "BillSundry", "BillSundrys", findip(), bls.BillSundryId, "Bill Sundry Deleted Successfully");
                db.SaveChanges();
            }
            return true;
        }

        // make active or inactive
        [HttpGet]
        [QkAuthorize(Roles = "Dev,BillSundry Status")]
        public ActionResult BillSundryStatus(string type, long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BillSundry bls = db.BillSundrys.Find(id);
            if (bls == null)
            {
                return NotFound();
            }
            ViewBag.id = id;   // carried into the confirm form so the POST always knows which record to toggle
            if (type == "active")
            {
                ViewBag.type = "Active";
                ViewBag.link = "active";
                ViewBag.status = Status.active;
            }
            else
            {
                ViewBag.type = "Inactive";
                ViewBag.link = "inactive";
                ViewBag.status = Status.inactive;
            }
            return PartialView();
        }
        // POST: master/ChangeStatus/
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,BillSundry Status")]
        public JsonResult BillSundryStatus(string type, long? id, BillSundry bls)
        {
            bool stat = false;
            string msg;
            string types = "";
            BillSundry Bls = db.BillSundrys.Find(id);
            // Decide from the reliable `type` route/hidden value ("inactive"/"active"). The old code read
            // bls.Status from the form's HiddenFor, but that field renders EMPTY (null model + capital-V
            // @Value), so it always defaulted to active — i.e. Deactivate never worked.
            if (type == "inactive" || (bls != null && bls.Status == Status.inactive))
            {
                types = " Inactive";
                Bls.Status = Status.inactive;
            }
            else
            {
                types = " Active";
                Bls.Status = Status.active;
            }

            db.Entry(Bls).State = EntityState.Modified;
            var updates = db.SaveChanges();

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Changed, UserId, "BillSundry", "BillSundrys", findip(), Bls.BillSundryId, "Successfully Changed the Bill Sundrys Status" + types);


            stat = true;
            msg = " Successfully Changed the Bill Sundrys to" + types;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public JsonResult Search(string q)
        {

            object serialisedJson;
            // ONLY active bill sundries appear in the quote/invoice dropdowns — an inactive one must be
            // re-enabled from the Bill Sundries list before it can be picked anywhere.
            var qActive = db.BillSundrys.Where(x => x.Status == Status.active);
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = qActive.Where(p => p.BSName.ToLower().Contains(q.ToLower()) || p.BSName.Contains(q))
                        .Select(b => new
                        {
                            text = b.BSName, //each json object will have
                            id = b.BillSundryId,
                            b.PAccount,
                            b.SAccount,
                        }).Where(o=>o.PAccount!=null && o.SAccount!=null).Select(b=>new
                        {
                            text = b.text, //each json object will have
                            id = b.id,
                        }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = qActive.Select(b => new
                {
                    text = b.BSName, //each json object will have
                    id = b.BillSundryId,

                    b.PAccount,
                    b.SAccount,
                }).Where(o => o.PAccount != null && o.SAccount != null).Select(b => new
                {
                    text = b.text, //each json object will have
                    id = b.id,
                }).OrderBy(b => b.text).ToList();
            }
            return Json(serialisedJson);
        }
        public JsonResult Search3(string q)
        {

            object serialisedJson;
            var qActive = db.BillSundrys.Where(x => x.Status == Status.active);   // active-only in dropdowns
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = qActive.Where(p => (p.BSName.ToLower().Contains(q.ToLower()) || p.BSName.Contains(q))&&p.AmountType==AmountType.AbsoluteAmount)
                        .Select(b => new
                        {
                            text = b.BSName, //each json object will have
                            id = b.BillSundryId,
                            b.PAccount,
                            b.SAccount,
                        }).Where(o => o.PAccount != 0 && o.SAccount != 0).Select(b => new
                        {
                            text = b.text, //each json object will have
                            id = b.id,
                        }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = qActive.Where(o => o.PAccount != 0 && o.SAccount != 0& o.AmountType == AmountType.AbsoluteAmount).Select(b => new
                {
                    text = b.BSName, //each json object will have 
                    id = b.BillSundryId,

                    b.PAccount,
                    b.SAccount,
                }).Select(b => new
                {
                    text = b.text, //each json object will have 
                    id = b.id,
                }).OrderBy(b => b.text).ToList();
            }
            return Json(serialisedJson);
        }
        public JsonResult Search2(string q)
        {

            object serialisedJson;
            var qActive = db.BillSundrys.Where(x => x.Status == Status.active);   // active-only in dropdowns
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = qActive.Where(p => (p.BSName.ToLower().Contains(q.ToLower()) || p.BSName.Contains(q)))
                        .Select(b => new
                        {
                            text = b.BSName, //each json object will have
                            id = b.BillSundryId,
                            b.PAccount,
                            b.SAccount,
                        }).Where(o => o.PAccount != 0 && o.SAccount != 0).Select(b => new
                        {
                            text = b.text, //each json object will have
                            id = b.id,
                        }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = qActive.Where(o => o.PAccount != 0 && o.SAccount != 0).Select(b => new
                {
                    text = b.BSName, //each json object will have 
                    id = b.BillSundryId,

                    b.PAccount,
                    b.SAccount,
                }).Select(b => new
                {
                    text = b.text, //each json object will have 
                    id = b.id,
                }).OrderBy(b => b.text).ToList();
            }
            return Json(serialisedJson);
        }
        public ActionResult GetBillSundryById(int bsID)
        {
            var bs = db.BillSundrys.Where(c => c.BillSundryId == bsID).Select(b => new
            {
                b.BillSundryId,
                b.AmountType,
                b.Branch,
                b.BSName,
                BSNature= db.BSNatures.Where(a => a.BSNatureId == b.BSNature).Select(a => a.BSNatureName).FirstOrDefault(),
                b.BSType,
                b.DefaultValue
             }).FirstOrDefault();
            return Json(bs);

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
