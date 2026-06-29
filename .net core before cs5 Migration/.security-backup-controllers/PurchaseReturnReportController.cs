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
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Mvc;
using CustomHtml;
using Microsoft.AspNetCore.Identity;

namespace QuickSoft.Controllers
{
    public class PurchaseReturnReportController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PurchaseReturnReportController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: PurchaseReturnReport
        public ActionResult Index()
        {
            return View();
        }


        #region itemwise
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Item Wise")]
        public ActionResult ItemWise()
        {
            var OptAll = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Item = OptAll;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();


            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            return View();
        }

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Item Wise")]
        public ActionResult ViewItemWise(long? ddlItem, string From, string To, long? ddlMC)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            if (ddlItem != 0)
            {
                ViewBag.item = (from a in db.Items
                                where a.ItemID == ddlItem
                                select new
                                {
                                    ItemName = a.ItemCode + "-" + a.ItemName
                                }).FirstOrDefault().ItemName;
            }
            else
            {
                ViewBag.item = "All";
            }
            ViewBag.fromdate = From;
            ViewBag.todate = To;
            ViewBag.ddlmc = ddlMC;
            companySet();
            return View();
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Item Wise")]
        public ActionResult GetItemWise(long? item, string fromdate, string todate, long? ddlMC)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddlMC == 0 || ddlMC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

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
            var v = (from a in db.Items
                     join e in db.ItemUnits on a.ItemUnitID equals e.ItemUnitID into primary
                     from e in primary.DefaultIfEmpty()
                     join f in db.ItemUnits on a.SubUnitId equals f.ItemUnitID into second
                     from f in second.DefaultIfEmpty()
                     where (item == 0 || item == null || a.ItemID == item)

                     select new
                     {
                         a.ItemID,
                         a.ItemName,
                         Item = a.ItemCode + "-" + a.ItemName,
                         PriUnit = e.ItemUnitName,
                         SubUnit = f.ItemUnitName,
                         ConFactor = a.ConFactor != 0 ? a.ConFactor : 1,
                         a.ItemUnitID,
                         a.SubUnitId,
                         a.SellingPrice,
                         a.PartNumber,
                         SaleType = a.Branch,
                         PriRetQty = (decimal?)(from i in db.PRItemss
                                                join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                                where (fromdate == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                                (todate == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                                (i.Item == a.ItemID && i.ItemUnit == a.ItemUnitID)
                                                && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)

                                                group i by i.Item into g
                                                select new
                                                {
                                                    Total = g.Sum(x => x.ItemQuantity)
                                                }).FirstOrDefault().Total ?? 0,

                         SubRetQty = (decimal?)(from i in db.PRItemss
                                                join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                                where (fromdate == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                                (todate == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                                (i.Item == a.ItemID && i.ItemUnit == a.SubUnitId) &&
                                                 e.ItemUnitName != f.ItemUnitName
                                                && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)
                                                group i by i.Item into g
                                                select new
                                                {
                                                    Total = g.Sum(x => x.ItemQuantity)
                                                }).FirstOrDefault().Total ?? 0,

                         RetunAmt = (decimal?)(from i in db.PRItemss
                                               join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                               where (fromdate == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                               (todate == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                               (i.Item == a.ItemID)
                                               && ((!MCList.Any() && ddlMC == null) || MCArray.Contains(j.MaterialCenter) || ddlMC == j.MaterialCenter)
                                               group i by i.Item into g
                                               select new
                                               {
                                                   Total = g.Sum(x => x.ItemSubTotal - x.ItemDiscount)
                                               }).FirstOrDefault().Total ?? 0,


                     }).Distinct().AsEnumerable().Select(o => new
                     {
                         o.ItemID,
                         o.Item,
                         o.ItemUnitID,
                         o.SubUnitId,
                         PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                         SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                         o.ConFactor,
                         PartNumber = (o.PartNumber != null && o.PartNumber != "") ? o.PartNumber : "",


                         PriRetQty = (o.PriRetQty + (int)(o.SubRetQty / o.ConFactor)),
                         SubRetQty = (o.SubRetQty % o.ConFactor),

                         NetQty = ((o.PriRetQty * o.ConFactor) + (o.SubRetQty)),

                         o.RetunAmt,
                         o.ItemName
                     }).OrderBy(a => a.ItemName);

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
        #endregion

        #region Supplier itemwise
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Supplier ItemWise")]
        public ActionResult SupplierItemWise()
        {
            ViewBag.Item = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                            }, "Value", "Text", 0);


            var cust = db.Suppliers.Select(s => new
            {
                SupplierID = s.SupplierID,
                SupplierDetails = s.SupplierCode + " - " + s.SupplierName
            }).ToList();
            ViewBag.Supplier = QkSelect.List(cust, "SupplierID", "SupplierDetails");
            companySet();
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            return View();
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Supplier ItemWise")]
        public ActionResult GetSupplierItemWise(long? ddlSupplier, long? ddlItem, string From, string To, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddmc == 0 || ddmc == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (From != "")
            {
                fdate = DateTime.Parse(From, new CultureInfo("en-GB"));
            }
            if (To != "")
            {
                tdate = DateTime.Parse(To, new CultureInfo("en-GB"));
            }

            var v = (from a in db.Items
                     join e in db.ItemUnits on a.ItemUnitID equals e.ItemUnitID into primary
                     from e in primary.DefaultIfEmpty()
                     join f in db.ItemUnits on a.SubUnitId equals f.ItemUnitID into second
                     from f in second.DefaultIfEmpty()
                     where (ddlItem == 0 || a.ItemID == ddlItem)
                     select new
                     {
                         a.ItemID,
                         a.ItemName,
                         Item = a.ItemCode + "-" + a.ItemName,
                         PriUnit = e.ItemUnitName,
                         SubUnit = f.ItemUnitName,
                         ConFactor = a.ConFactor != 0 ? a.ConFactor : 1,
                         a.ItemUnitID,
                         a.SubUnitId,
                         a.SellingPrice,

                         PriRetQty = (decimal?)(from i in db.PRItemss
                                                join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                                where (From == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                                (To == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                                (i.Item == a.ItemID && i.ItemUnit == a.ItemUnitID) && (j.Supplier == ddlSupplier)
                                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                group i by i.Item into g
                                                select new
                                                {
                                                    Total = g.Sum(x => x.ItemQuantity)
                                                }).FirstOrDefault().Total ?? 0,

                         SubRetQty = (decimal?)(from i in db.PRItemss
                                                join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                                where (From == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                                (To == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                                (i.Item == a.ItemID && i.ItemUnit == a.SubUnitId) &&
                                                 e.ItemUnitName != f.ItemUnitName && (j.Supplier == ddlSupplier)
                                                 && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                group i by i.Item into g
                                                select new
                                                {
                                                    Total = g.Sum(x => x.ItemQuantity)
                                                }).FirstOrDefault().Total ?? 0,


                         RetunAmt = (decimal?)(from i in db.PRItemss
                                               join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                               where (From == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                               (To == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                               (i.Item == a.ItemID) && (j.Supplier == ddlSupplier)
                                               && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                               group i by i.Item into g
                                               select new
                                               {
                                                   Total = g.Sum(x => x.ItemSubTotal - x.ItemDiscount)
                                               }).FirstOrDefault().Total ?? 0,

                         NoOfVchReturn = (int?)(from i in db.PRItemss
                                                join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                                where (From == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                                (To == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                                (i.Item == a.ItemID) && (j.Supplier == ddlSupplier)
                                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                //  group i by i.Item into g
                                                select new
                                                {
                                                    saleid = i.PurchaseReturnId
                                                }).GroupBy(x => x.saleid).Count() ?? 0,

                     }).Distinct().AsEnumerable().Select(o => new
                     {
                         o.ItemID,
                         o.Item,
                         o.ItemUnitID,
                         o.SubUnitId,
                         PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                         SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                         o.ConFactor,

                         PriRetQty = (o.PriRetQty + (int)(o.SubRetQty / o.ConFactor)),
                         SubRetQty = (o.SubRetQty % o.ConFactor),

                         NetQty = ((o.PriRetQty * o.ConFactor) + o.SubRetQty),

                         o.RetunAmt,
                         o.NoOfVchReturn,
                         o.ItemName,
                     }).Where(a => a.NoOfVchReturn != 0).OrderBy(a => a.ItemName);

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


        #endregion

        #region Supplier Wise
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Supplier Wise")]
        public ActionResult SupplierWise()
        {
            ViewBag.Supplier = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                }, "Value", "Text", 0);

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            ViewBag.MC = QkSelect.List(
           new List<SelectListItem>
           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
           }, "Value", "Text", 0);

            ViewBag.getProj = QkSelect.List(
                                  new List<SelectListItem>
                                  {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                  }, "Value", "Text", 0);

            ViewBag.getProTask = QkSelect.List(
                                  new List<SelectListItem>
                                  {
                                      new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                  }, "Value", "Text", 0);

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View();
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Supplier Wise")]
        public ActionResult SupplierWise(long? ddlSupplier, string From, string To, long? ddlMC, long? ddlProject, long? ddlProTask)
        {
            return RedirectToAction("ViewSupplierWise", new { cust = ddlSupplier, from = From, to = To, ddMC = ddlMC, project = ddlProject, task = ddlProTask });
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Supplier Wise")]
        public ActionResult GetSupplierWise(long? Supplier, string fromdate, string todate, long? ddMC, long? project, long? task)
        {

            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddMC == 0 || ddMC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

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
            var v = (from a in db.Suppliers
                     join f in db.Accountss on a.Accounts equals f.AccountsID into acc
                     from f in acc.DefaultIfEmpty()
                     where (Supplier == 0 || a.SupplierID == Supplier)
                     select new
                     {
                         a.SupplierID,
                         Supplier = a.SupplierCode + "-" + a.SupplierName,
                         TRN = f.TRN,
                         RetunAmt = (decimal?)(from i in db.PurchaseReturns
                                               where (fromdate == "" || EF.Functions.DateDiffDay(i.PRDate, fdate) <= 0) &&
                                               (todate == "" || EF.Functions.DateDiffDay(i.PRDate, tdate) >= 0) &&
                                               (i.Supplier == a.SupplierID)
                                                && ((!MCList.Any() && ddMC == null) || MCArray.Contains(i.MaterialCenter) || ddMC == i.MaterialCenter)
                                               group i by i.Supplier into g
                                               select new
                                               {
                                                   Total = g.Sum(x => x.PRSubTotal - x.PRDiscount)
                                               }).FirstOrDefault().Total ?? 0,
                         RetuntaxAmt = (decimal?)(from i in db.PurchaseReturns
                                                  where (fromdate == "" || EF.Functions.DateDiffDay(i.PRDate, fdate) <= 0) &&
                                                  (todate == "" || EF.Functions.DateDiffDay(i.PRDate, tdate) >= 0) &&
                                                  (i.Supplier == a.SupplierID)
                                                  && ((!MCList.Any() && ddMC == null) || MCArray.Contains(i.MaterialCenter) || ddMC == i.MaterialCenter)
                                                  group i by i.Supplier into g
                                                  select new
                                                  {
                                                      Total = g.Sum(x => x.PRTaxAmount)
                                                  }).FirstOrDefault().Total ?? 0,
                         RetuntotAmt = (decimal?)(from i in db.PurchaseReturns
                                                  where (fromdate == "" || EF.Functions.DateDiffDay(i.PRDate, fdate) <= 0) &&
                                                  (todate == "" || EF.Functions.DateDiffDay(i.PRDate, tdate) >= 0) &&
                                                  (i.Supplier == a.SupplierID)
                                                  && ((!MCList.Any() && ddMC == null) || MCArray.Contains(i.MaterialCenter) || ddMC == i.MaterialCenter)
                                                  group i by i.Supplier into g
                                                  select new
                                                  {
                                                      Total = g.Sum(x => x.PRGrandTotal)
                                                  }).FirstOrDefault().Total ?? 0,
                                                  
                         NoOfVchReturn = (int?)(from j in db.PurchaseReturns
                                                where (fromdate == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                                (todate == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                                (j.Supplier == a.SupplierID)
                                                 && ((!MCList.Any() && ddMC == null) || MCArray.Contains(j.MaterialCenter) || ddMC == j.MaterialCenter)
                                                 select new
                                                {
                                                    saleid = j.PurchaseReturnId
                                                }).Count() ?? 0,
                     }).OrderBy(b => b.Supplier);

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
        [HttpGet]
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Supplier Wise")]
        public ActionResult ViewSupplierWise(long? cust, string from, string to, long? ddMC)
        {
            if (cust != 0)
            {
                ViewBag.custName = (from a in db.Suppliers
                                    join b in db.PurchaseReturns on a.SupplierID equals b.Supplier into cat
                                    from b in cat.DefaultIfEmpty()
                                    join f in db.Accountss on a.Accounts equals f.AccountsID into acc
                                    from f in acc.DefaultIfEmpty()
                                    where a.SupplierID == cust
                                    select new
                                    {
                                        SupplierName = a.SupplierName + (f.TRN != null ? " ; TRN :" + f.TRN : "")
                                    }).FirstOrDefault().SupplierName;
            }
            else
            {
                ViewBag.custName = "All";
            }
            ViewBag.ddlmc = ddMC;
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            companySet();
            return View();
        }

        #endregion

        #region category wise
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Category Wise")]
        public ActionResult CategoryWise()
        {
            var OptAll = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                            }, "Value", "Text", 0);

            ViewBag.Category = OptAll;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View();
        }
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Category Wise")]
        public ActionResult CategoryWises(long? ddlItemCategory, string From, string To, long? ddlMC)
        {
            return RedirectToAction("ViewCategoryWise", new { category = ddlItemCategory, from = From, to = To, ddMC = ddlMC });
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Category Wise")]
        public ActionResult GetCategoryWise(long? category, string fromdate, string todate, long? ddMC)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddMC == 0 || ddMC == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

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
            var v = (from a in db.ItemCategorys

                     where (category == 0 || a.ItemCategoryID == category)
                     select new
                     {
                         Category = a.ItemCategoryName,
                         a.ItemCategoryID,

                         RetunAmt = (decimal?)(from i in db.PRItemss
                                               join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                               join k in db.Items on i.Item equals k.ItemID into itm
                                               from k in itm.DefaultIfEmpty()
                                               where (fromdate == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                               (todate == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                               (k.ItemCategoryID == a.ItemCategoryID)
                                               && ((!MCList.Any() && ddMC == null) || MCArray.Contains(j.MaterialCenter) || ddMC == j.MaterialCenter)
                                               select new
                                               {
                                                   Total = i.ItemSubTotal - i.ItemDiscount
                                               }).Sum(x => x.Total) ?? 0,




                         NoOfVchReturn = (int?)(from i in db.PRItemss
                                                join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                                join k in db.Items on i.Item equals k.ItemID into itm
                                                from k in itm.DefaultIfEmpty()
                                                where (fromdate == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                                (todate == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                                (k.ItemCategoryID == a.ItemCategoryID)
                                                && ((!MCList.Any() && ddMC == null) || MCArray.Contains(j.MaterialCenter) || ddMC == j.MaterialCenter)
                                                select new
                                                {
                                                    saleid = i.PurchaseReturnId
                                                }).GroupBy(x => x.saleid).Count() ?? 0,

                     }).Distinct().AsEnumerable().Select(o => new
                     {
                         o.Category,
                         o.ItemCategoryID,
                         o.RetunAmt,
                         o.NoOfVchReturn
                     }).OrderBy(x => x.Category);

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

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Category Wise")]
        public ActionResult ViewCategoryWise(long? category, string from, string to, long? ddMC)
        {
            if (category != 0)
            {
                ViewBag.category = (from a in db.ItemCategorys
                                    where a.ItemCategoryID == category
                                    select new
                                    {
                                        ItemCategory = a.ItemCategoryName
                                    }).FirstOrDefault().ItemCategory;
            }
            else
            {
                ViewBag.category = "All";
            }
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            companySet();
            return View();
        }

        #endregion

        #region brand wise
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Brand Wise")]
        public ActionResult BrandWise()
        {
            ViewBag.Brand = QkSelect.List(
             new List<SelectListItem>
             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
             }, "Value", "Text", 1);
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            return View();
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Brand Wise")]
        public ActionResult BrandWise(long? ddlItemBrand, string From, string To, long? ddlMC)
        {
            return RedirectToAction("ViewBrandWise", new { brand = ddlItemBrand, from = From, to = To, ddmc = ddlMC });
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Brand Wise")]
        public ActionResult GetBrandWise(long? brand, string fromdate, string todate, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddmc == 0 || ddmc == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

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
            var v = (from a in db.ItemBrands
                     where (brand == 0 || a.ItemBrandID == brand)
                     select new
                     {
                         Brand = a.ItemBrandName,
                         a.ItemBrandID,

                         RetunAmt = (decimal?)(from i in db.PRItemss
                                               join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                               join k in db.Items on i.Item equals k.ItemID into itm
                                               from k in itm.DefaultIfEmpty()
                                               where (fromdate == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                               (todate == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                                (k.ItemBrandID == a.ItemBrandID)
                                                 && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                               select new
                                               {
                                                   Total = i.ItemSubTotal - i.ItemDiscount
                                               }).Sum(x => x.Total) ?? 0,

                         NoOfVchReturn = (int?)(from i in db.PRItemss
                                                join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                                join k in db.Items on i.Item equals k.ItemID into itm
                                                from k in itm.DefaultIfEmpty()
                                                where (fromdate == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                                (todate == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                                (k.ItemBrandID == a.ItemBrandID)
                                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                //  group i by i.Item into g
                                                select new
                                                {
                                                    saleid = i.PurchaseReturnId
                                                }).GroupBy(x => x.saleid).Count() ?? 0,

                     }).Distinct().AsEnumerable().Select(o => new
                     {
                         o.Brand,
                         o.ItemBrandID,
                         o.RetunAmt,
                         o.NoOfVchReturn
                     }).OrderBy(x => x.Brand);

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
        [HttpGet]
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Brand Wise")]
        public ActionResult ViewBrandWise(long? brand, string from, string to, long? ddmc)
        {
            if (brand != 0)
            {
                ViewBag.brand = (from a in db.ItemBrands
                                 where a.ItemBrandID == brand
                                 select new
                                 {
                                     BName = a.ItemBrandName
                                 }).FirstOrDefault().BName;
            }
            else
            {
                ViewBag.brand = "All";
            }
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            companySet();
            return View();
        }

        #endregion

        #region executive wise
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Executive Wise")]
        public ActionResult ExecutiveWise()
        {
            ViewBag.SalesExec = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
            }, "Value", "Text", 0);
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);

            return View();
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Executive Wise")]
        public ActionResult SalesExecutiveWise(long? ddlEmployee, string From, string To, long? ddlMC)
        {
            return RedirectToAction("ViewSalesExecutiveWise", new { salesexec = ddlEmployee, from = From, to = To, ddmc = ddlMC });
        }
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Executive Wise")]
        public ActionResult GetExecutiveWise(long? salesexec, string fromdate, string todate, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddmc == 0 || ddmc == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

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
            var v = (from a in db.Employees
                     where (salesexec == 0 || a.EmployeeId == salesexec)
                     select new
                     {
                         a.EmployeeId,
                         a.FirstName,
                         employee = a.FirstName + " " + a.MiddleName + " " + a.LastName,

                         RetunAmt = (decimal?)(from i in db.PurchaseReturns
                                               where (fromdate == "" || EF.Functions.DateDiffDay(i.PRDate, fdate) <= 0) &&
                                               (todate == "" || EF.Functions.DateDiffDay(i.PRDate, tdate) >= 0) &&
                                               (i.PRCashier == a.EmployeeId)
                                               && ((!MCList.Any() && ddmc == null) || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                               group i by i.Supplier into g
                                               select new
                                               {
                                                   Total = g.Sum(x => x.PRSubTotal - x.PRDiscount)
                                               }).FirstOrDefault().Total ?? 0,
                         RetuntaxAmt = (decimal?)(from i in db.PurchaseReturns
                                                  where (fromdate == "" || EF.Functions.DateDiffDay(i.PRDate, fdate) <= 0) &&
                                                  (todate == "" || EF.Functions.DateDiffDay(i.PRDate, tdate) >= 0) &&
                                                  (i.PRCashier == a.EmployeeId)
                                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                                  group i by i.Supplier into g
                                                  select new
                                                  {
                                                      Total = g.Sum(x => x.PRTaxAmount)
                                                  }).FirstOrDefault().Total ?? 0,
                         RetuntotAmt = (decimal?)(from i in db.PurchaseReturns
                                                  where (fromdate == "" || EF.Functions.DateDiffDay(i.PRDate, fdate) <= 0) &&
                                                  (todate == "" || EF.Functions.DateDiffDay(i.PRDate, tdate) >= 0) &&
                                                  (i.PRCashier == a.EmployeeId)
                                                  && ((!MCList.Any() && ddmc == null) || MCArray.Contains(i.MaterialCenter) || ddmc == i.MaterialCenter)
                                                  group i by i.Supplier into g
                                                  select new
                                                  {
                                                      Total = g.Sum(x => x.PRGrandTotal)
                                                  }).FirstOrDefault().Total ?? 0,

                         NoOfVchReturn = (int?)(from j in db.PurchaseReturns
                                                where (fromdate == "" || EF.Functions.DateDiffDay(j.PRDate, fdate) <= 0) &&
                                                (todate == "" || EF.Functions.DateDiffDay(j.PRDate, tdate) >= 0) &&
                                                (j.PRCashier == a.EmployeeId)
                                                && ((!MCList.Any() && ddmc == null) || MCArray.Contains(j.MaterialCenter) || ddmc == j.MaterialCenter)
                                                select new
                                                {
                                                    saleid = j.PurchaseReturnId
                                                }).Count() ?? 0,
                     }).OrderBy(a => a.FirstName);

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

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Executive Wise")]
        public ActionResult ViewSalesExecutiveWise(long? salesexec, string from, string to, long? ddmc)
        {
            if (salesexec != 0)
            {
                ViewBag.salesExec = (from a in db.Employees
                                     where a.EmployeeId == salesexec
                                     select new
                                     {
                                         EmpName = a.FirstName + " " + a.MiddleName + " " + a.LastName
                                     }).FirstOrDefault().EmpName;
            }
            else
            {
                ViewBag.salesExec = "All";
            }
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            companySet();
            return View();
        }
        #endregion

        #region Invoice item wise
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Invoice ItemWise")]
        public ActionResult InvoiceItemWise()
        {
            var select = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                            }, "Value", "Text", 0);
            ViewBag.Supplier = select;
            ViewBag.Item = select;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();

            ViewBag.MC = QkSelect.List(
            new List<SelectListItem>
            {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
            }, "Value", "Text", 0);
            return View();
        }
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Invoice ItemWise")]
        public ActionResult InvoiceWise(long? ddlItem, long? ddlSupplier, string from, string to, long? ddlMC)
        {
            if (ddlItem != 0)
            {
                ViewBag.item = (from a in db.Items
                                where a.ItemID == ddlItem
                                select new
                                {
                                    ItemName = a.ItemCode + "-" + a.ItemName
                                }).FirstOrDefault().ItemName;
            }
            else
            {
                ViewBag.item = "All";
            }
            ViewBag.fromdate = from;
            ViewBag.todate = to;
            ViewBag.Supplier = ddlSupplier;
            companySet();
            return View();
        }
        //[QkAuthorize(Roles = "Dev,PurchaseReturn Invoice ItemWise")]
        public ActionResult getInvoiceWise(long? item, long? Supplier, string fromdate, string to, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && (ddmc == 0 || ddmc == 1))
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

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
            if (to != "")
            {
                tdate = DateTime.Parse(to, new CultureInfo("en-GB"));
            }

            var v = (from a in db.PRItemss
                     join b in db.Items on a.Item equals b.ItemID
                     join g in db.PurchaseReturns on a.PurchaseReturnId equals g.PurchaseReturnId
                     join c in db.Suppliers on g.Supplier equals c.SupplierID
                     join e in db.ItemUnits on b.ItemUnitID equals e.ItemUnitID into primary
                     from e in primary.DefaultIfEmpty()
                     join f in db.ItemUnits on b.SubUnitId equals f.ItemUnitID into second
                     from f in second.DefaultIfEmpty()
                     where (item == 0 || a.Item == item) && (Supplier == 0 || Supplier == null || g.Supplier == Supplier)
                     && (fromdate == "" || EF.Functions.DateDiffDay(g.PRDate, fdate) <= 0)
                     && (to == "" || EF.Functions.DateDiffDay(g.PRDate, tdate) >= 0)
                     && ((!MCList.Any() && ddmc == null) || MCArray.Contains(g.MaterialCenter) || ddmc == g.MaterialCenter)
                     select new
                     {
                         b.ItemID,
                         b.ItemName,
                         b.ItemCode,
                         Item = b.ItemCode + "-" + b.ItemName,
                         Unit = (a.ItemUnit == b.ItemUnitID) ? e.ItemUnitName : f.ItemUnitName,
                         g.BillNo,
                         a.ItemQuantity,
                         a.ItemSubTotal,
                         a.ItemTaxAmount,
                         a.ItemTotalAmount,
                         a.ItemDiscount,
                         a.itemNote,
                         a.ItemUnitPrice,
                         PriUnit = e.ItemUnitName,
                         SubUnit = f.ItemUnitName,
                         ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                         b.ItemUnitID,
                         b.SubUnitId,
                         b.SellingPrice,
                         Supplier = c.SupplierName,
                         Date = g.PRDate
                     }).AsEnumerable().OrderBy(a => a.ItemName);

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
        #endregion
    }
}
