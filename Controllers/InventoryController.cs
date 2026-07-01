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
using System.Collections;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WooCommerceNET;
using WooCommerceNET.Base;
using WooCommerceNET.WooCommerce.v3;
using WooCommerceNET.WooCommerce.v3.Extension;

namespace QuickSoft.Controllers
{
   
    public class InventoryController : BaseController
    {
        ApplicationDbContext db;
        public InventoryController()
        {
            db = new ApplicationDbContext();
        }

        // ---- Inventory dashboard (styled like the main dashboard) ----
        public ActionResult Dashboard()
        {
            var today = DateTime.Now;
            var lastdate = today.AddMonths(-1);
            ViewBag.today = today.ToString("dd-MM-yyyy");
            ViewBag.lastdate = lastdate.ToString("dd-MM-yyyy");

            var vmodel = new QuickSoft.ViewModel.HomeViewModel();
            vmodel.totCustomerCount = Convert.ToString(db.Items.Count());
            vmodel.totSupplierCount = Convert.ToString(db.ItemCategorys.Count());

            // 12-month trend: Delivery value + Items delivered (quantity)
            try
            {
                var trendStart = new DateTime(today.Year, today.Month, 1).AddMonths(-11);
                var dByM = db.Deliverynotes.Where(n => n.DvDate >= trendStart)
                    .GroupBy(n => new { n.DvDate.Year, n.DvDate.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, V = g.Sum(x => (decimal?)x.DvGrandTotal) ?? 0, Q = g.Sum(x => (decimal?)x.DvItemQuantity) ?? 0 }).ToList();
                var tl = new List<string>(); var tv = new List<decimal>(); var tq = new List<decimal>();
                for (int i = 0; i < 12; i++)
                {
                    var d = trendStart.AddMonths(i);
                    tl.Add(d.ToString("MMM"));
                    var row = dByM.Where(x => x.Year == d.Year && x.Month == d.Month).FirstOrDefault();
                    tv.Add(row != null ? row.V : 0);
                    tq.Add(row != null ? row.Q : 0);
                }
                ViewBag.trendLabels = Newtonsoft.Json.JsonConvert.SerializeObject(tl);
                ViewBag.trendSales = Newtonsoft.Json.JsonConvert.SerializeObject(tv);
                ViewBag.trendPurchase = Newtonsoft.Json.JsonConvert.SerializeObject(tq);
            }
            catch { ViewBag.trendLabels = "[]"; ViewBag.trendSales = "[]"; ViewBag.trendPurchase = "[]"; }

            // Per-period KPIs: Delivery value, Delivery Notes, Items Delivered, Avg Note value (+ delta)
            try
            {
                DateTime endEx = today.Date.AddDays(1);
                DateTime dToday = today.Date, dYest = dToday.AddDays(-1);
                int dow = ((int)today.DayOfWeek + 6) % 7;
                DateTime wkStart = dToday.AddDays(-dow), wkPrev = wkStart.AddDays(-7);
                DateTime moStart = new DateTime(today.Year, today.Month, 1), moPrev = moStart.AddMonths(-1);
                DateTime yrStart = new DateTime(today.Year, 1, 1), yrPrev = yrStart.AddYears(-1);

                Func<DateTime, DateTime, decimal[]> sums = (from, to) => new[]
                {
                    db.Deliverynotes.Where(x => x.DvDate >= from && x.DvDate < to).Select(x => (decimal?)x.DvGrandTotal).Sum() ?? 0m,
                    (decimal)db.Deliverynotes.Count(x => x.DvDate >= from && x.DvDate < to),
                    db.Deliverynotes.Where(x => x.DvDate >= from && x.DvDate < to).Select(x => (decimal?)x.DvItemQuantity).Sum() ?? 0m
                };
                Func<decimal, decimal, double?> dlt = (cur, prev) => prev == 0 ? (double?)null : (double)Math.Round((cur - prev) / prev * 100, 1);
                Func<string, DateTime, DateTime, DateTime, object> mk = (label, from, to, prevFrom) =>
                {
                    var c = sums(from, to); var p = sums(prevFrom, from);
                    decimal avg = c[1] > 0 ? c[0] / c[1] : 0, pavg = p[1] > 0 ? p[0] / p[1] : 0;
                    return new
                    {
                        label,
                        value = c[0], notes = c[1], items = c[2], avg = avg,
                        dValue = dlt(c[0], p[0]), dNotes = dlt(c[1], p[1]), dItems = dlt(c[2], p[2]), dAvg = dlt(avg, pavg)
                    };
                };
                var periodData = new
                {
                    today = mk("today", dToday, endEx, dYest),
                    week = mk("week", wkStart, endEx, wkPrev),
                    month = mk("month", moStart, endEx, moPrev),
                    year = mk("year", yrStart, endEx, yrPrev)
                };
                ViewBag.periodJson = Newtonsoft.Json.JsonConvert.SerializeObject(periodData);
            }
            catch { ViewBag.periodJson = "null"; }

            ViewBag.Active = "Dashboard";
            ViewBag.Title = "Inventory";
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View(vmodel);
        }

        [HttpPost]
        public ActionResult GetRecentDeliveries()
        {
            var v = db.Deliverynotes.OrderByDescending(d => d.DeliverynoteId).Take(6)
                .Select(d => new
                {
                    d.DeliverynoteId,
                    DvNo = d.DvNo,
                    Customer = db.Customers.Where(a => a.CustomerID == d.Customer).Select(a => a.CustomerCode + " - " + a.CustomerName).FirstOrDefault(),
                    d.DvDate,
                    Items = d.DvItemQuantity,
                    d.DvGrandTotal
                }).ToList();
            return Json(new { data = v });
        }

        #region moment
        [QkAuthorize(Roles = "Dev,Stock Moment")]
        public ActionResult Moment()
        {

            ViewBag.Category = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                                }, "Value", "Text", 0);
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;

            var EnableJewellery = db.EnableSettings.Where(a => a.EnableType == "EnableJewellery").FirstOrDefault();
            ViewBag.JewCheck = EnableJewellery != null ? EnableJewellery.Status : Status.inactive;

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

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Stock Moment")]
        public ActionResult Moment(long? ddlItem, long? ddlMC)
        {
            return RedirectToAction("ViewMoment", new { iditem = ddlItem, ddmc = ddlMC });
        }
        [QkAuthorize(Roles = "Dev,Stock Moment")]
        public ActionResult GetMoment(long? iditem, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && ddmc == 0)
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;
            decimal confactor=db.Items.Where(o => o.ItemID == iditem).Select(o => o.ConFactor).FirstOrDefault();
            ddmc = ddmc != null ? ddmc : 0;
            var selitem = new SqlParameter("@ItemId", (object)iditem ?? DBNull.Value);
            var selmc = new SqlParameter("@MCId", (object)ddmc ?? DBNull.Value);
            var brand = new SqlParameter("@BrandId", "0");
            var stkble = new SqlParameter("@Stockble", "");
            var catgry = new SqlParameter("@CategoryId", "0");
            var fromdate = new SqlParameter("@fromdate", "");
            var todate = new SqlParameter("@todate", "");
            var stype = new SqlParameter("@Stype", "0");

            var data = db.Database.SqlQueryDedup<StockDataDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.TDate).ToList();





            for (int i = 0; i < data.Count(); i++)
            {
                data[i].confactor = confactor;
                if (data[i].TItemType.Contains("Stock")&& data[i].Invoice!=null)
                {

                    data[i].invoiceid = getstocktransferid(data[i].Invoice, iditem,data[i].TDate);
                }
                else
                {
                    data[i].TItemId = gettransactionid(data[i].TItemId,data[i].TItemType);
                }
            }
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
        public long gettransactionid(long itranid, string trantype)
        {
            long stt = 0;
           if(trantype=="Sales")
            {
                stt = (from a in db.SalesEntrys
                       join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry 
                              where b.SEItemsId==itranid
                              select a.SalesEntryId
                              ).FirstOrDefault();
            }
            else if (trantype == "Sales Return")
            {
                stt = (from a in db.SalesReturns
                       join b in db.SRItemss on a.SalesReturnId equals b.SalesReturnId
                       where b.SRItemsId == itranid
                       select a.SalesReturnId
                              ).FirstOrDefault();

            }
            else if (trantype == "Purchase")
            {
                stt = (from a in db.PurchaseEntrys
                       join b in db.PEItemss on a.PurchaseEntryId equals b.PurchaseEntry
                       where b.PEItemsId == itranid
                       select a.PurchaseEntryId
                              ).FirstOrDefault();

            }
            else if (trantype == "Purchase Return")
            {
                stt = (from a in db.PurchaseReturns
                       join b in db.PRItemss on a.PurchaseReturnId equals b.PurchaseReturnId
                       where b.PRItemsId == itranid
                       select a.PurchaseReturnId
                              ).FirstOrDefault();

            }

            return stt;
            

        }

        public long getstocktransferid(string voucher,long? itemid,DateTime transferdate)
        {
            var stt = (from a in db.StockTransfers
                       join b in db.StockTransferItems on a.Id equals b.StockTransferId
                       where a.Voucher == voucher && b.Item == itemid && a.Date==transferdate
                       select new
                       {
                           voucherid = a.Id
                       }).FirstOrDefault();
            if (stt != null)
                return stt.voucherid;
            else
                return 0;

        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Stock Moment")]
        public ActionResult ViewMoment(long? iditem, long? ddmc)
        {
            if (iditem != 0)
            {
                ViewBag.ItemName = (from a in db.Items
                                    where a.ItemID == iditem
                                    select new 
                                    {
                                        Name =a.ItemCode + "-" + a.ItemName
                                    }).FirstOrDefault().Name;
            }
            else
            {
                ViewBag.ItemName = "All";
            }

            if (ddmc != null && ddmc != 0)
            {
                string mcn = db.MCs.Where(z => z.MCId == ddmc).Select(z => z.MCName).FirstOrDefault();
                ViewBag.MCSName = "Material Center : " + mcn;
            }
            else
            {
                ViewBag.MCSName = "";
            }
            companySet();
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;

            var EnableJewellery = db.EnableSettings.Where(a => a.EnableType == "EnableJewellery").FirstOrDefault();
            ViewBag.JewCheck = EnableJewellery != null ? EnableJewellery.Status : Status.inactive;

            return View();
        }
        #endregion

        #region itemwise
        [QkAuthorize(Roles = "Dev,Stock Item Wise")]
        public ActionResult ItemWise()
        {
            var UserId = User.Identity.GetUserId();
       
                var mcs = db.MCs.Select(s => new SelectFormat
                {
                    id = s.MCId,
                    text = s.MCName
                }).ToList();
                var initial = new SelectFormat() { id = 0, text = "All" };
                mcs.Insert(0, initial);
                ViewBag.MC = QkSelect.List(mcs, "id", "text");


            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;
            var OptAll = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                           }, "Value", "Text", 0);

            ViewBag.Brand = OptAll;

            ViewBag.Category = OptAll;

            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Stock Item Wise")]
        public ActionResult ItemWise(long? ddlItem, long? ddlMC)
        {
            return RedirectToAction("ViewItemWise", new { itemid = ddlItem, ddmc = ddlMC });
        }



        public decimal? GetItemWisestock(long? itemid, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && ddmc == 0)
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            var AVCOMethod = db.EnableSettings.Where(a => a.EnableType == "AVCOMethod").FirstOrDefault();
            var AVCOMethods = AVCOMethod != null ? AVCOMethod.Status : Status.inactive;


            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;


            ddmc = ddmc != null ? ddmc : 0;
            var selitem = new SqlParameter("@ItemId", (object)itemid ?? DBNull.Value);
            var selmc = new SqlParameter("@MCId", (object)ddmc ?? DBNull.Value);
            var brand = new SqlParameter("@BrandId", "0");
            var stkble = new SqlParameter("@Stockble", "");
            var catgry = new SqlParameter("@CategoryId", "0");
            var fromdate = new SqlParameter("@fromdate", "");
            var todate = new SqlParameter("@todate", "");
            var stype = new SqlParameter("@Stype", "1");

            var data = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).ToList();


            return (data[0].ITotalQty==null)?0: data[0].ITotalQty;
           
           

        }


        public decimal? GetItemWisestocks(long? itemid, long[] ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (ddmc == null)
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            else
            {
                MCList = db.MCs.Where(o=>ddmc.Contains(o.MCId)).Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            var AVCOMethod = db.EnableSettings.Where(a => a.EnableType == "AVCOMethod").FirstOrDefault();
            var AVCOMethods = AVCOMethod != null ? AVCOMethod.Status : Status.inactive;


            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            decimal? totalqty = 0;
            foreach (var ddm in MCArray)
            {
                var selitem = new SqlParameter("@ItemId", (object)itemid ?? DBNull.Value);
                var selmc = new SqlParameter("@MCId", (object)ddm ?? DBNull.Value);
                var brand = new SqlParameter("@BrandId", "0");
                var stkble = new SqlParameter("@Stockble", "");
                var catgry = new SqlParameter("@CategoryId", "0");
                var fromdate = new SqlParameter("@fromdate", "");
                var todate = new SqlParameter("@todate", "");
                var stype = new SqlParameter("@Stype", "1");

                var data = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).ToList();
              totalqty=totalqty +  data[0].ITotalQty;
            }

            return totalqty;


        }









        [QkAuthorize(Roles = "Dev,Stock Item Wise,HireStock Item Wise")]
        public ActionResult GetItemWise(long? itemid, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && ddmc == 0)
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            var AVCOMethod = db.EnableSettings.Where(a => a.EnableType == "AVCOMethod").FirstOrDefault();
            var AVCOMethods = AVCOMethod != null ? AVCOMethod.Status : Status.inactive;


            int recordsTotal = 0;

            
            ddmc = ddmc != null ? ddmc : 0;
            var selitem = new SqlParameter("@ItemId", (object)itemid ?? DBNull.Value);
            var selmc = new SqlParameter("@MCId", (object)ddmc ?? DBNull.Value);
            var brand = new SqlParameter("@BrandId","0" );
            var stkble = new SqlParameter("@Stockble","");
            var catgry = new SqlParameter("@CategoryId", "0");
            var fromdate = new SqlParameter("@fromdate", "");
            var todate = new SqlParameter("@todate", "");
            var stype = new SqlParameter("@Stype", "1");

            var data = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).ToList();



            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var mydata = (PreCheck == Status.active || BusinessType == "Scaffold") ? data.OrderBy(a => a.IItemCode).ToList() : data;

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new {  recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = mydata });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Stock Item Wise")]
        public ActionResult ViewItemWise(long? itemid, long? ddmc)
        {
            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;
            ViewBag.PartNoCheck = PartNoCheck;

            if (itemid != 0)
            {
                ViewBag.ItemName = (from a in db.Items
                                    where a.ItemID == itemid
                                    select new
                                    {
                                        ItemName = a.ItemName
                                    }).FirstOrDefault().ItemName;
            }
            else
            {
                ViewBag.ItemName = "All";
            }

            if (ddmc != null && ddmc != 0)
            {
                string mcn = db.MCs.Where(z => z.MCId == ddmc).Select(z => z.MCName).FirstOrDefault();
                ViewBag.MCSName = "Material Center : " + mcn;
            }
            else
            {
                ViewBag.MCSName = "";
            }


            companySet();
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            return View();
        }

        #endregion

        #region brand wise

        // GET: StockReport Brand Wise
        [QkAuthorize(Roles = "Dev,Stock Brand Wise")]
        public ActionResult BrandWise()
        {
            ViewBag.Brand = QkSelect.List(
            new List<SelectListItem>
            {
                new SelectListItem { Selected = false, Text = "All", Value = "0"},
            }, "Value", "Text", 1);

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

            companySet();
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Stock Brand Wise")]
        public ActionResult BrandWise(long? ddlBrand, long? ddlMC, bool stockable = false,bool zerostockitem = false)
        {
            return RedirectToAction("ViewBrandWise", new { brandid = ddlBrand, ddmc = ddlMC, stockable = stockable , zerostockitem = zerostockitem });
        }
        [QkAuthorize(Roles = "Dev,Stock Brand Wise,HireStock Brand Wise")]
        public ActionResult GetBrandWise(long? brandid,  long? ddmc, bool stockable, bool zerostockitem)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && ddmc == 0)
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

            var keepstk = stockable == true ? 1 : 0;
            var zerostock = zerostockitem == true ? 1 : 0;

            ddmc = ddmc != null ? ddmc : 0;
            var selitem = new SqlParameter("@ItemId", "");
            var selmc = new SqlParameter("@MCId", (object)ddmc ?? DBNull.Value);
            var brand = new SqlParameter("@BrandId", (object)brandid ?? DBNull.Value);
            var stkble = new SqlParameter("@Stockble", (object)keepstk ?? DBNull.Value);
            var catgry = new SqlParameter("@CategoryId", "0");
            var fromdate = new SqlParameter("@fromdate", "");
            var todate = new SqlParameter("@todate", "");
            var stype = new SqlParameter("@Stype", "1");


            IEnumerable<StockDetails> data = new List<StockDetails>();
            var datadd = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().ToList();
            if (zerostockitem == true)
            {
                 data = datadd.Where(a=>a.ITotalQty > 0).OrderBy(a => a.IItemName).ToList();
            }
            else
            {
                 data = datadd.OrderBy(a => a.IItemName).ToList();
            }


            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var mydata = (PreCheck == Status.active || BusinessType == "Scaffold") ? data.OrderBy(a => a.IItemCode).ToList() : data;

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = mydata });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Stock Brand Wise")]
        public ActionResult ViewBrandWise(long? brandid, bool stockable, bool zerostockitem, long? ddmc)
        {
            if (brandid != 0)
            {
                ViewBag.BrName = (from a in db.ItemBrands
                                  where a.ItemBrandID == brandid
                                  select new
                                  {
                                      Name = a.ItemBrandName
                                  }).FirstOrDefault().Name;
            }
            else
            {
                ViewBag.BrName = "All";
            }
            if (ddmc != null && ddmc != 0)
            {
                string mcn = db.MCs.Where(z => z.MCId == ddmc).Select(z => z.MCName).FirstOrDefault();
                ViewBag.MCSName = "Material Center : " + mcn;
            }
            else
            {
                ViewBag.MCSName = "";
            }
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;
            companySet();

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            return View();
        }


        #endregion

        #region category wise
        // GET: StockReport Category Wise
        [QkAuthorize(Roles = "Dev,Stock Category Wise")]
        public ActionResult CategoryWise()
        {
            ViewBag.Category = QkSelect.List(
                new List<SelectListItem>
                {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                }, "Value", "Text", 1);
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            //        Id = s.MCId,
            //        Name = s.MCName
            //        Id = s.MCId,
            //        Name = s.MCName

            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            companySet();
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Stock Category Wise")]
        public ActionResult CategoryWise(long? ddlCategory, long? ddlMC, bool stockable = false, bool zerostockitem = false)
        {
            return RedirectToAction("ViewCategoryWise", new { cateid = ddlCategory, stockable = stockable, zerostockitem = zerostockitem, ddmc = ddlMC });
        }
        [QkAuthorize(Roles = "Dev,Stock Category Wise,HireStock Category Wise")]
        public ActionResult GetCategoryWise(long? cateid, bool stockable, bool zerostockitem, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && ddmc == 0)
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

            var keepstk = stockable == true ? 1 : 0;
            var zerostock = zerostockitem == true ? 1 : 0;

            ddmc = ddmc != null ? ddmc : 0;
            var selitem = new SqlParameter("@ItemId", "");
            var selmc = new SqlParameter("@MCId", (object)ddmc ?? DBNull.Value);
            var brand = new SqlParameter("@BrandId", "");
            var stkble = new SqlParameter("@Stockble", (object)keepstk ?? DBNull.Value);
            var catgry = new SqlParameter("@CategoryId", (object)cateid ?? DBNull.Value);

            var fromdate = new SqlParameter("@fromdate", "");
            var todate = new SqlParameter("@todate", "");
            var stype = new SqlParameter("@Stype", "1");

            IEnumerable<StockDetails> data = new List<StockDetails>();
            var datadd = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().ToList();
            if (zerostockitem == true)
            {
                data = datadd.Where(a => a.ITotalQty > 0).OrderBy(a => a.IItemName).ToList();
            }
            else
            {
                data = datadd.OrderBy(a => a.IItemName).ToList();
            }

            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var mydata = (PreCheck == Status.active || BusinessType == "Scaffold") ? data.OrderBy(a => a.IItemCode).ToList() : data;

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = mydata });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Stock Category Wise")]
        public ActionResult ViewCategoryWise(long? cateid, bool stockable,bool zerostockitem, long? ddmc)
        {
            if (cateid != 0)
            {
                ViewBag.CatName = (from a in db.ItemCategorys
                                   where a.ItemCategoryID == cateid
                                   select new
                                   {
                                       Name = a.ItemCategoryName
                                   }).FirstOrDefault().Name;
            }
            else
            {
                ViewBag.CatName = "All";
            }

            if (ddmc != null && ddmc != 0)
            {
                string mcn = db.MCs.Where(z => z.MCId == ddmc).Select(z => z.MCName).FirstOrDefault();
                ViewBag.MCSName = "Material Center : " + mcn;
            }
            else
            {
                ViewBag.MCSName = "";
            }
            companySet();
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            return View();
        }

        #endregion

        #region till date
        // GET: StockReport
        [QkAuthorize(Roles = "Dev,Till Date Stock")]
        public ActionResult mcdetails(string ddmc,string srctxt,long? category,long? itemid,long? brandid)
        {
            companySet();
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);



            return View();
        }
        public async Task<int> GetOrCreateCategory(WCObject wc, string categoryName)
        {
            int page = 1;

            while (true)
            {
                var categories = await wc.Category.GetAll(new Dictionary<string, string>()
        {
            { "per_page", "100" },
            { "page", page.ToString() }
        });

                if (categories.Count == 0)
                    break;

                var existing = categories
                    .FirstOrDefault(c => c.name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                    return (int)existing.id;

                page++;
            }

            // Create category if not found
            var newCat = new ProductCategory()
            {
                name = categoryName
            };

            var created = await wc.Category.Add(newCat);
            return (int)created.id;
        }
        public async Task<ActionResult> readorders()
        {

            int page = 1;
            bool hasMore = true;
            RestAPI rest = new RestAPI("http://quicknet.fortiddns.com:1150/wp-json/wc/v3/", "ck_21333aee54f9687e14cb0740747487d47988feae", "cs_1091ee3d6f1fd5ce2525f19ba2d5cc39c08060f4");

            WCObject wc = new WCObject(rest);

            while (hasMore)
            {
                var orders = await wc.Order.GetAll(new Dictionary<string, string>()
    {
        { "per_page", "100" },   // max per request
        { "page", page.ToString() }
    });

                if (orders.Count == 0)
                {
                    hasMore = false;
                    break;
                }

                foreach (var order in orders)
                {
                    Console.WriteLine($"Order ID: {order.id}");
                    Console.WriteLine($"Customer: {order.billing.first_name} {order.billing.last_name}");
                    Console.WriteLine($"Total: {order.total}");
                    Console.WriteLine($"Status: {order.status}");
                    Console.WriteLine("-----------------------------");
                }

                page++;
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public async Task<ActionResult> uploadwordpress(string WordPressUrl)
        {
            DateTime curdate = System.DateTime.Now.AddYears(-5);
            long[] itemids = (from a in db.SEItemss
                            join b in db.SalesEntrys on a.SalesEntry equals b.SalesEntryId
                            where
                            b.SEDate >= curdate
                            select new
                            {
                                a.Item
                            }).Distinct().Select(o => o.Item).ToList().ToArray();
            int page = 1;
            int perPage = 100; // max allowed
            RestAPI rest = new RestAPI("http://quicknet.fortiddns.com:1150/wp-json/wc/v3/", "ck_21333aee54f9687e14cb0740747487d47988feae", "cs_1091ee3d6f1fd5ce2525f19ba2d5cc39c08060f4");

            WCObject wc = new WCObject(rest);
   


            while (true)
            {
                // Get products page by page
                var products = await wc.Product.GetAll(new Dictionary<string, string>()
    {
        { "per_page", perPage.ToString() },
        { "page", page.ToString() }
    });

                if (products.Count == 0)
                    break;

                foreach (var product in products)
                {
                    // Delete product permanently
                    await wc.Product.Delete((ulong)product.id, true);

                
                }

                page++;
            }
             page = 1;
             perPage = 100;

            while (true)
            {
                var categories = await wc.Category.GetAll(new Dictionary<string, string>()
    {
        { "per_page", perPage.ToString() },
        { "page", page.ToString() }
    });

                if (categories.Count == 0)
                    break;

                foreach (var cat in categories)
                {
                    // Skip default "Uncategorized"
                    if (cat.name == "Uncategorized")
                        continue;

                    await wc.Category.Delete((ulong)cat.id, true);


                }

                page++;
            }
            var v = (from a in db.Items
                     
                     join b in db.ItemCategorys on a.ItemCategoryID equals b.ItemCategoryID into cat
                     from b in cat.DefaultIfEmpty()
                     join c in db.ItemBrands on a.ItemBrandID equals c.ItemBrandID into brand
                     from c in brand.DefaultIfEmpty()
                     join d in db.ItemColors on a.ItemColorID equals d.ItemColorID into color
                     from d in color.DefaultIfEmpty()
                     join e in db.Taxs on a.TaxID equals e.TaxID into itax
                     from e in itax.DefaultIfEmpty()
                     join f in db.ItemSizes on a.ItemSizeID equals f.ItemSizeID into isize
                     from f in isize.DefaultIfEmpty()
                     join g in db.ItemUnits on a.ItemUnitID equals g.ItemUnitID into punit
                     from g in punit.DefaultIfEmpty()
                     join h in db.ItemUnits on a.SubUnitId equals h.ItemUnitID into sunit
                     from h in sunit.DefaultIfEmpty()
                     join i in db.Suppliers on a.Supplier equals i.SupplierID into scat
                     from i in scat.DefaultIfEmpty()
                     join j in db.ItemBundles on a.ItemID equals j.mainItem into bundle
                     from j in bundle.DefaultIfEmpty()
                     join k in db.Scaffolds on a.ItemID equals k.Item into scaf
                     from k in scaf.DefaultIfEmpty()
                     join l in db.ItemImages on a.ItemID equals l.ItemID into itimg
                     from l in itimg.DefaultIfEmpty()
                     where a.ItemDescription!=null && a.ItemDescription!=""
                     && a.ItemDescription.Length>5
                    
                     select new
                     {
                         a.ItemID,
                         a.ItemCode,
                         a.ItemName,
                         a.ItemArabic,
                         a.ItemType,
                         a.Barcode,
                         a.ItemDescription,
                         a.SellingPrice,
                         a.PurchasePrice,
                         a.MRP,
                         a.BasePrice,
                         a.Status,
                         a.KeepStock,
                         a.slreq,
                         a.ItemUnitID,
                         a.PricingStrategy,
                           l.FileName,
                         ConFactor = a.ConFactor != 0 ? a.ConFactor : 1,
                         PartNumber = a.PartNumber != "" ? a.PartNumber : "",
                         Supplier = i.SupplierName,
                         SupplierRef = a.SupplierRef,
                         Category = b.ItemCategoryName,
                         Brand = c.ItemBrandName,
                         Color = d.ItemColorName,
                         Tax = e.TaxName,
                         Size = f.ItemSizeName,
                         PUnit = g.ItemUnitName,
                         SUnit = h.ItemUnitName,
                         a.OpeningStock,
                         k.Weight,
                         k.CBM,
                       
                         
                     }).Distinct().ToList().Select(o => new
                     {
                         o.ItemID,
                         o.ItemCode,
                         o.ItemName,
                         o.ItemArabic,
                         o.ItemType,
                         o.Barcode,
                         o.ItemDescription,
                         o.SellingPrice,
                         o.PurchasePrice,
                         o.MRP,
                         o.BasePrice,
                         o.Status,
                         o.KeepStock,
                         o.slreq,
                         o.ItemUnitID,
                         o.PricingStrategy,
                         o.ConFactor,
                         o.PartNumber,
                           o.FileName,
                         o.Supplier,
                         o.SupplierRef,
                         o.Category,
                         o.Brand,
                         o.Color,
                         o.Tax,
                         o.Size,
                         o.PUnit,
                         o.SUnit,
                         o.OpeningStock,
                         o.Weight,
                         o.CBM,

                     }).Distinct();
          
            BatchObject<Product> lis = new BatchObject<Product>();
            //            name = pro.Category
            //Use below code for WCObject only if you would like to have different CultureInfo
            List<Product> ps = new List<Product>();
            foreach (var pro in v)
            {
                int categoryId = await GetOrCreateCategory(wc,pro.Category);
                Product p = new Product()
                {
                    name = pro.ItemName,

                    //id =(ulong?) pro.ItemID,
                    //categories = cat,
    //                images = new List<ProductImage>()
    //    new ProductImage() { src = "http://quicknet.fortiddns.com:1200/uploads/itemimages/"+((pro.FileName==null||pro.FileName=="")?"default.jpg":pro.FileName) }
    //},
                     regular_price =pro.SellingPrice,
                   
                   sale_price =pro.SellingPrice,

                    description = pro.ItemDescription,
                    price =pro.SellingPrice,
                 //   manage_stock = true,
                //    stock_status = "instock",
                    categories = new List<ProductCategoryLine>()
    {
        new ProductCategoryLine() { id = (ulong)categoryId }
    },


                };
                ps.Add(p);
                
            }
            
            lis.create = ps;
            await wc.Product.AddRange(lis);
            //Get all products
            //Get all products

            return RedirectToAction("wordpress","Inventory");
        }
        [QkAuthorize(Roles = "Dev,Till Date Stock")]
        public ActionResult wordpress()
        {
            companySet();
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
           
            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);



            return View();
        }
     
        // GET: StockReport
        [QkAuthorize(Roles = "Dev,Till Date Stock")]
        public ActionResult Index()
        {
            companySet();
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            db.Database.ExecuteSqlRaw("reindex");
            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);



            return View();
        }
        [QkAuthorize(Roles = "Dev,Till Date Stock")]
        public ActionResult reorder()
        {
            companySet();
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);



            return View();
        }


        [QkAuthorize(Roles = "Dev,Till Date Stock")]
        public ActionResult fromto()
        {
            ViewBag.Item = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                             }, "Value", "Text", 1);
            companySet();
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;
            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            ViewBag.MC = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 0);

            long[] selmc = { 2, 3, 4, 5, 20047, 20069 };
            var use = db.MCs.Select(s => new SelectFormat { id = s.MCId, text = s.MCName }).ToList();

            ViewBag.mcavail = new MultiSelectList(use, "id", "text", selmc);

            List<SelectListItem> SelectPeriod = new List<SelectListItem>() {
                    new SelectListItem {
                          Text = "3 Month", Value = "3"
                                        },
                    new SelectListItem {
                        Text = "2 Month", Value = "2"
                                   },
                    new SelectListItem {

                         Text = "1 Month", Value = "1"
                          },

            };
            ViewBag.Period = SelectPeriod;

            return View();
        }
     



        [QkAuthorize(Roles = "Dev,Till Date Stock,Till Date HireStock")]
        public ActionResult GetStock2( bool zerostockitem,long? period, long[] ddmc, long[] ddmcavl, long? itemid, string datefrom, string dateto, string datefromforecast, string datetoforecast)
        {//)
           
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            DateTime datefroms = DateTime.Parse(datefrom, new CultureInfo("en-GB"));
            DateTime datetos = DateTime.Parse(dateto, new CultureInfo("en-GB"));
            DateTime datefromsforecast = DateTime.Parse(datefromforecast, new CultureInfo("en-GB"));
            DateTime datetosforecast = DateTime.Parse(datetoforecast, new CultureInfo("en-GB"));
            var datediff = (datetos - datefroms).TotalDays;
            var datediffforcate = (datetosforecast - datefromsforecast).TotalDays;
            if (!MCList.Any() && ddmc!=null)
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            IEnumerable<StockDetailsmovement> data = new List<StockDetailsmovement>();

            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var keepstk =0;
            var zerostock = zerostockitem == true ? 1 : 0;
            foreach (var mcc in ddmc)
            {
                var selitem = new SqlParameter("@ItemId", "");
                if (itemid == 0)
                    selitem = new SqlParameter("@ItemId", "");
                else
                    selitem = new SqlParameter("@ItemId", (object)itemid ?? DBNull.Value);

                var selmc = new SqlParameter("@MCId", (object)mcc ?? DBNull.Value);
                var brand = new SqlParameter("@BrandId", "");
                var stkble = new SqlParameter("@Stockble", (object)keepstk ?? DBNull.Value);
                var catgry = new SqlParameter("@CategoryId", "");
                var fromdate = new SqlParameter("@fromdate", (object)datefroms ?? DBNull.Value);
                var todate = new SqlParameter("@todate", (object)datetos ?? DBNull.Value);
                var stype = new SqlParameter("@Stype", "1");

                var datadd = db.Database.SqlQueryDedup<StockDetailsmovement>("SP_AVCOMethod2 @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().ToList();

                var i = 0;
                DateTime today = System.DateTime.Now;
                for (i = 0; i < datadd.Count; i++)
                {
                    if (1==1)
                    {
                        datadd[i].currstock = GetItemWisestocks(datadd[i].IItemID, ddmcavl);
                    
                    if (ddmcavl != null)
                    {
                            datadd[i].onweek= getnetqty(ddmcavl, today.AddDays(-7), today, datadd[i].IItemID, 0);
                           
                            datadd[i].onemonth = getnetqty(ddmcavl, today.AddDays(-30), today, datadd[i].IItemID, 0);
                            datadd[i].netvalue = datadd[i].onemonth;// getnetqty(ddmcavl, datefroms, today, datadd[i].IItemID, 0); 
                        }
                    else
                    {
                        datadd[i].onemonth = getnetqty2(ddmcavl, today.AddDays(-30), today, datadd[i].IItemID, 0);
                            datadd[i].netvalue = getnetqty2(ddmcavl, today.AddDays(-30), today, datadd[i].IItemID, 0);
                            datadd[i].onweek = getnetqty2(ddmcavl, today.AddDays(-7), today, datadd[i].IItemID, 0);
                        }
                        
                        datadd[i].datediff = datediff;
                        datadd[i].datediffforcaste = datediffforcate;
                    }
                   
                }
                if (zerostockitem == true)
                {
                    data=data.Union(datadd.Where(a => a.stockout > 0 || a.stockin > 0).OrderByDescending(a => a.netvalue).ToList());
                }
                else
                {
                    data=data.Union(datadd.OrderByDescending(a => a.netvalue).ToList());
                }
            }
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumn != "" && sortColumn != "IItemID")
                {
                    data = data.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
                }
            }
            var datalast = (from a in data
                    group a by new { a.IItemID } into grp
                    select new
                    {
                        IItemID = grp.Key.IItemID,
                        IItemName = grp.FirstOrDefault().IItemName,
                        IItemCode=grp.FirstOrDefault().IItemCode,
                        stockin = grp.Sum(o=>o.stockin),
                        stockout = grp.Sum(o => o.stockout),
                        currstock = grp.Sum(o => o.currstock),
                        onemonth = grp.Max(o => o.onemonth),
                        onweek=grp.Max(o=>o.onweek),
                        netvalue=grp.Sum(o=>o.netvalue),
                        datediff=grp.Max(o=>o.datediff),
                        datediffforcaste=grp.Max(o=>o.datediffforcaste)
                    }
                  );
            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var mydata = (PreCheck == Status.active || BusinessType == "Scaffold") ? data.OrderBy(a => a.IItemCode).ToList() : data;

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = datalast, ddmc,itemid,datefrom,dateto,datediff});
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }
        private Decimal getnetqty(long[] ddmc1, DateTime datefroms, DateTime dateto, long? itemid, long? supplierid)
        {
            var confactor = db.Items.Where(o => o.ItemID == itemid).Select(o => o.ConFactor).FirstOrDefault();
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (ddmc1 == null)
            {
                MCList = db.MCs.Select(o => (long?)o.MCId).ToList();
            }
            else
            {
                MCList = db.MCs.Where(o => ddmc1.Contains(o.MCId)).Select(o => (long?)o.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            decimal datediff = Convert.ToDecimal((dateto - datefroms).TotalDays);
            decimal SENo = 0;
            decimal netqty2 = 0;
            var netqty = (decimal)(from a in db.SalesEntrys
                          join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                          join c in db.Items on b.Item equals c.ItemID
                    where
                    b.Item == itemid &&
                                         b.ItemUnit == c.ItemUnitID &&
                                         (ddmc1.Contains((long)a.MaterialCenter)) &&
                                         (EF.Functions.DateDiffDay(a.SEDate, datefroms) <= 0) &&
                                          (EF.Functions.DateDiffDay(a.SEDate, dateto) >= 0) &&
                                    (!ddmc1.Any() || MCArray.Contains(a.MaterialCenter))
                                   select new
                          {
                              b.ItemQuantity
                          }).ToList().Sum(c => c.ItemQuantity);
            SENo = netqty * confactor;
            if (confactor != 1)
            {
                netqty2 = (decimal)(from a in db.SalesEntrys
                                    join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                                    join c in db.Items on b.Item equals c.ItemID
                                    where
                                    (b.Item == itemid) &&
                                             (b.ItemUnit == c.SubUnitId) &&
                                             (ddmc1.Contains((long)a.MaterialCenter)) &&
                                             (EF.Functions.DateDiffDay(a.SEDate, datefroms) <= 0) &&
                                              (EF.Functions.DateDiffDay(a.SEDate, dateto) >= 0) &&
                                         (!ddmc1.Any() || MCArray.Contains(a.MaterialCenter))
                                    select new
                                    {
                                        b.ItemQuantity
                                    }).ToList().Sum(c => c.ItemQuantity);
            }
            var totalqty = SENo + netqty2;
            SENo = totalqty/confactor;// (totalqty/ (datediff*confactor))*30;
            return SENo;
        }
        private Decimal getnetqty2(long[] ddmc1, DateTime datefroms, DateTime dateto, long? itemid, long? supplierid)
        {

            var confactor = db.Items.Where(o => o.ItemID == itemid).Select(o => o.ConFactor).FirstOrDefault();
            decimal netqty2 = 0;
            decimal datediff = Convert.ToDecimal((dateto - datefroms).TotalDays);
            decimal SENo = 0;
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (ddmc1==null)
            {
                MCList = db.MCs.Select(o => (long?)o.MCId).ToList();
            }
            else
            {
                MCList = db.MCs.Where(o=>ddmc1.Contains(o.MCId)).Select(o => (long?)o.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            var netqty = (decimal)(from a in db.SalesEntrys
                                   join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                                   join c in db.Items on b.Item equals c.ItemID
                                   let supp = (from c in db.PurchaseEntrys
                                               join d in db.PEItemss on c.PurchaseEntryId equals d.PurchaseEntry
                                               where d.Item == b.Item &&
                                               c.Supplier == supplierid
                                               select new
                                               {
                                                   d.Item
                                               }).FirstOrDefault()

                                   where
                                   (supplierid == 0 || supplierid == null || b.Item == supp.Item) &&
                                   (b.Item == itemid) &&
                                   b.ItemUnit==c.ItemUnitID &&
                                   (EF.Functions.DateDiffDay(a.SEDate, datefroms) <= 0) &&
                                    (EF.Functions.DateDiffDay(a.SEDate, dateto) >= 0) &&
                                    (ddmc1==null|| MCArray.Contains(a.MaterialCenter))
                                   select new
                                   {
                                       b.ItemQuantity
                                   }).ToList().Sum(c => c.ItemQuantity);
            SENo = netqty * confactor;
            if (confactor != 1)
            {
                 netqty2 = (decimal)(from a in db.SalesEntrys
                                        join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                                        join c in db.Items on b.Item equals c.ItemID
                                        let supp = (from c in db.PurchaseEntrys
                                                    join d in db.PEItemss on c.PurchaseEntryId equals d.PurchaseEntry
                                                    where d.Item == b.Item &&
                                                    c.Supplier == supplierid
                                                    select new
                                                    {
                                                        d.Item
                                                    }).FirstOrDefault()

                                        where
                                        (supplierid == 0 || supplierid == null || b.Item == supp.Item) &&
                                        (b.Item == itemid) &&
                                        b.ItemUnit == c.SubUnitId &&
                                        (EF.Functions.DateDiffDay(a.SEDate, datefroms) <= 0) &&
                                         (EF.Functions.DateDiffDay(a.SEDate, dateto) >= 0) &&
                                    (ddmc1==null || MCArray.Contains(a.MaterialCenter))
                                     select new
                                        {
                                            b.ItemQuantity
                                        }).ToList().Sum(c => c.ItemQuantity);

            }
            var totalqty = SENo + netqty2;
            SENo = totalqty/ confactor;// (totalqty / (datediff * confactor)) * 30;
            return Convert.ToInt32(SENo);


        }

        public ActionResult getforcast(long? period,  long[] ddmc1, long? itemid,decimal currstock, string datefromforecaste, string datetoforecaste)
        {

            var confactor = db.Items.Where(o => o.ItemID == itemid).Select(o => o.ConFactor).FirstOrDefault();

            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            DateTime datefromsforecast = DateTime.Parse(datefromforecaste, new CultureInfo("en-GB"));
            DateTime datetosforecast = DateTime.Parse(datetoforecaste, new CultureInfo("en-GB"));
            var datediff = (decimal)(datetosforecast - datefromsforecast).TotalDays;
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var netqty = (from a in db.SalesEntrys
                          join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                          join m in db.MCs on a.MaterialCenter equals m.MCId
                          join i in db.Items on b.Item equals i.ItemID
                          let PriSale = (int?)(from v in db.SEItemss
                                               join w in db.SalesEntrys on v.SalesEntry equals w.SalesEntryId
                                               where v.Item == i.ItemID && v.ItemUnit == i.ItemUnitID
                                               && w.MaterialCenter==a.MaterialCenter &&
                                               (EF.Functions.DateDiffDay(w.SEDate, datefromsforecast) <= 0) &&
                           (EF.Functions.DateDiffDay(w.SEDate, datetosforecast) >= 0) &&
                               (v.Item == itemid)
                                               select new
                                               {
                                                   v.ItemQuantity
                                               }).Sum(c => c.ItemQuantity) ?? 0
                          let SubSale = (int?)(from v in db.SEItemss
                                                   join w in db.SalesEntrys on v.SalesEntry equals w.SalesEntryId
                                                   where v.Item == i.ItemID && v.ItemUnit == i.SubUnitId
                                                   && w.MaterialCenter == a.MaterialCenter &&
                                                   (EF.Functions.DateDiffDay(w.SEDate, datefromsforecast) <= 0) &&
                               (EF.Functions.DateDiffDay(w.SEDate, datetosforecast) >= 0) &&
                               (v.Item == itemid)
                                               select new
                                                   {
                                                       v.ItemQuantity
                                                   }).Sum(c => c.ItemQuantity) ?? 0
                          where
                           (b.Item == itemid) &&
                           b.ItemUnit==i.ItemUnitID &&
                          (ddmc1.Contains((long)a.MaterialCenter)) &&
                          (EF.Functions.DateDiffDay(a.SEDate, datefromsforecast) <= 0) &&
                           (EF.Functions.DateDiffDay(a.SEDate, datetosforecast) >= 0)


                          group new { m.MCName, b.ItemQuantity, i.ItemName, i.ItemID,PriSale,SubSale } by new { a.MaterialCenter } into grp
                          select new
                          {
                              grp.Key.MaterialCenter,
                              onemonth = ((grp.FirstOrDefault().PriSale+(grp.FirstOrDefault().SubSale/confactor))/datediff)*30,
                              datedif = datediff,
                              MC = grp.FirstOrDefault().MCName,
                              ItemID = grp.FirstOrDefault().ItemID,
                              currstock = 0,
                              itemname = grp.FirstOrDefault().ItemName,
                              prd = period
                          }).ToList().Select(o => new
                          {
                               o.MaterialCenter,
                               o.onemonth,
                               o.datedif,
                               o.MC,
                               o.ItemID,
                               currstock= GetItemWisestock(o.ItemID,o.MaterialCenter),
                               o.itemname,
                               o.prd
                          }) ;


            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = netqty});
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

        }
        [QkAuthorize(Roles = "Dev,Till Date Stock,Till Date HireStock")]
        public ActionResult GetStock(bool stockable, bool zerostockitem, long? ddmc,string todatte,bool inactive)
        {
            db.SetCommandTimeOut(60 * 60);
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && ddmc == 0)
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            DateTime? ondates = null;
            if (todatte != "")
            {
                ondates = DateTime.Parse(todatte, new CultureInfo("en-GB"));
            }

            int recordsTotal = 0;
            var keepstk = stockable == true ? 1 : 0;
            var zerostock = zerostockitem == true ? 1 : 0;

            ddmc = ddmc != null ? ddmc : 0;


            var itmids = db.Items.Where(o => o.KeepStock == true&&o.Status==Status.active).Select(o => o.ItemID).ToList();
            if(inactive==true)
            {
                itmids=db.Items.Where(o => o.KeepStock == true && o.Status == Status.inactive).Select(o => o.ItemID).ToList();
            }
            List<StockDetails> data = new List<StockDetails>();
            ddmc = ddmc != null ? ddmc : 0;
            //               where c.MCTo == ddmc2 && a.KeepStock == true && a.Status == Status.active
            //                   Itemid = a.ItemID,


            //               where c.MaterialCenter == ddmc2 && a.KeepStock == true && a.Status == Status.active
            //                   Itemid = a.ItemID
            //                where c.MaterialCenter == ddmc2 && a.KeepStock == true && a.Status == Status.active
            //                    Itemid = a.ItemID

            //                 where c.MaterialCenter == ddmc2 && a.Status == Status.active && a.KeepStock == true
            //                     Itemid = a.ItemID













            

           if (1==1)
            {
                var selitem = new SqlParameter("@ItemId", "");
                var selmc = new SqlParameter("@MCId", (object)ddmc ?? DBNull.Value);
                var brand = new SqlParameter("@BrandId", "");
                var stkble = new SqlParameter("@Stockble", 1);
                var catgry = new SqlParameter("@CategoryId", "");
                var fromdate = new SqlParameter("@fromdate", "");
                var todate = new SqlParameter("@todate", (object)ondates ?? DBNull.Value);
                var stype = new SqlParameter("@Stype", "1");
                try
                {
                    var dataadd = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).ToList();

                    foreach (var it in dataadd)
                    {
                        if (zerostockitem == true && it.ITotalQty > 0)
                            data.Add(it);
                        else if (zerostockitem == false)
                            data.Add(it);
                    }
                }
                catch(Exception e)
                {

                }
            }
            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var sum = data.Sum(o => o.ITotalStockValue);
            var mydata = (PreCheck == Status.active || BusinessType == "Scaffold") ? data.OrderBy(a => a.IItemCode).ToList() : data;

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new {  recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = mydata });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }




        [QkAuthorize(Roles = "Dev,Till Date Stock,Till Date HireStock")]
        public ActionResult GetStockmcwise(bool stockable,string srctxt,long? category,long? itemid,long? brandid, bool zerostockitem, long? ddmc, string todatte)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && ddmc == 0)
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            DateTime? ondates = null;
            if (todatte != "")
            {
                ondates = DateTime.Parse(todatte, new CultureInfo("en-GB"));
            }

            int recordsTotal = 0;
            var keepstk = stockable == true ? 1 : 0;
            var zerostock = zerostockitem == true ? 1 : 0;

            ddmc = ddmc != null ? ddmc : 0;

            var itmids = db.Items.Where(o => o.KeepStock == true).Select(o => o.ItemID).ToList();
            List<StockDetails> data = new List<StockDetails>();
            ddmc = ddmc != null ? ddmc : 0;
            if (itemid == null || itemid == 0)
            {
                foreach (var it in itmids)
                {
                    var selitem = new SqlParameter("@ItemId", (object)it ?? DBNull.Value);
                    var selmc = new SqlParameter("@MCId", (object)ddmc ?? DBNull.Value);
                    var brand = new SqlParameter("@BrandId", "");
                    var stkble = new SqlParameter("@Stockble", 1);
                    var catgry = new SqlParameter("@CategoryId", "");
                    var fromdate = new SqlParameter("@fromdate", "");
                    var todate = new SqlParameter("@todate", "");
                    var stype = new SqlParameter("@Stype", "1");

                    var dataadd = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod6 @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).FirstOrDefault();

                    if (zerostockitem == true && dataadd.ITotalQty > 0)
                        data.Add(dataadd);
                    else if (zerostockitem == false)
                        data.Add(dataadd);

                }
            }
            else if(srctxt!="")
            {
                var ids = db.Items.Where(o => o.ItemName.Contains(srctxt)).Select(o => o.ItemID).ToArray();
                foreach (var it in ids)
                {
                    var selitem = new SqlParameter("@ItemId", (object)it ?? DBNull.Value);
                    var selmc = new SqlParameter("@MCId", (object)ddmc ?? DBNull.Value);
                    var brand = new SqlParameter("@BrandId", "");
                    if(brandid==null||brandid==0)
                        brand = new SqlParameter("@BrandId", (object)brandid ?? DBNull.Value);
                    var stkble = new SqlParameter("@Stockble", 1);
                    var catgry = new SqlParameter("@CategoryId", "");
                    if(category==null||category==0)
                         catgry = new SqlParameter("@CategoryId", (object)category ?? DBNull.Value);
                    var fromdate = new SqlParameter("@fromdate", "");
                    var todate = new SqlParameter("@todate", (object)ondates ?? DBNull.Value);
                    var stype = new SqlParameter("@Stype", "1");

                    var dataadd = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).FirstOrDefault();

                    if (zerostockitem == true && dataadd.ITotalQty > 0)
                        data.Add(dataadd);
                    else if (zerostockitem == false)
                        data.Add(dataadd);

                }
            }
            else
            {
                 
                    foreach (var it in itmids)
                    {
                        var selitem = new SqlParameter("@ItemId", (object)it ?? DBNull.Value);
                        var selmc = new SqlParameter("@MCId", (object)ddmc ?? DBNull.Value);
                        var brand = new SqlParameter("@BrandId", "");
                        if (brandid == null || brandid == 0)
                            brand = new SqlParameter("@BrandId", (object)brandid ?? DBNull.Value);
                        var stkble = new SqlParameter("@Stockble", 1);
                        var catgry = new SqlParameter("@CategoryId", "");
                        if (category == null || category == 0)
                            catgry = new SqlParameter("@CategoryId", (object)category ?? DBNull.Value);
                        var fromdate = new SqlParameter("@fromdate", "");
                        var todate = new SqlParameter("@todate", (object)ondates ?? DBNull.Value);
                        var stype = new SqlParameter("@Stype", "1");

                        var dataadd = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).FirstOrDefault();

                        if (zerostockitem == true && dataadd.ITotalQty > 0)
                            data.Add(dataadd);
                        else if (zerostockitem == false)
                            data.Add(dataadd);

                    }
                }


            

            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var sum = data.Sum(o => o.ITotalStockValue);
            var mydata = (PreCheck == Status.active || BusinessType == "Scaffold") ? data.OrderBy(a => a.IItemCode).ToList() : data;

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = mydata });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }






        [QkAuthorize(Roles = "Dev,Till Date Stock,Till Date HireStock")]
        public ActionResult GetStockreorder(bool stockable, bool zerostockitem, long? ddmc)
        {
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            var mcminstock = db.EnableSettings.Where(o => o.EnableType == "materialcentrewiseminstock").SingleOrDefault();
            var mcminstockwise = (mcminstock != null) ? mcminstock.Status : Status.inactive;

            if (!MCList.Any() && ddmc == 0)
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            int recordsTotal = 0;
            var keepstk = stockable == true ? 1 : 0;
            var zerostock = zerostockitem == true ? 1 : 0;

            ddmc = ddmc != null ? ddmc : 0;
            var selitem = new SqlParameter("@ItemId", "");
            var selmc = new SqlParameter("@MCId", (object)ddmc ?? DBNull.Value);
            var brand = new SqlParameter("@BrandId", "");
            var stkble = new SqlParameter("@Stockble", (object)keepstk ?? DBNull.Value);
            var catgry = new SqlParameter("@CategoryId", "");
            var fromdate = new SqlParameter("@fromdate", "");
            var todate = new SqlParameter("@todate", "");
            var stype = new SqlParameter("@Stype", "1");
            var minstock = new SqlParameter("@minstock", "1");

            IEnumerable<StockDetails> data = new List<StockDetails>();
            db.Database.SetCommandTimeout(0);
            var datadd = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype,@minstock", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype, minstock).AsEnumerable().ToList();
            long itemid = 0;
            for(int i=0;i<datadd.Count();i++)
            {
                itemid = datadd[i].IItemID;
                if(db.mcitemminstock.Where(o => o.ItemId ==itemid  && o.MCId == ddmc).FirstOrDefault()!=null)
                {
                    datadd[i].IMinStock = db.mcitemminstock.Where(o => o.ItemId == itemid && o.MCId == ddmc).FirstOrDefault().minstock;
                }
            }
                datadd = datadd.Where(o => o.IMinStock > o.ITotalQty).ToList();
            
            if (zerostockitem == true)
            {
                data = datadd.Where(a => a.ITotalQty > 0).OrderBy(a => a.IItemName).ToList();
            }
            else
            {
                data = datadd.OrderBy(a => a.IItemName).ToList();
            }

            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var mydata = (PreCheck == Status.active || BusinessType == "Scaffold") ? data.OrderBy(a => a.IItemCode).ToList() : data;

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = mydata });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }



        #endregion

        #region as on date
        // GET: StockReport on date
        [QkAuthorize(Roles = "Dev,Stock As On Date")]
        public ActionResult OnDate()
        {
            companySet();
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;

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

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            return View();
        }
        [QkAuthorize(Roles = "Dev,Stock As On Date,HireStock As On Date")]
        public ActionResult GetOnDate(string ondate, bool stockable,bool zerostockitem, long? ddmc)
        {

            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && ddmc == 0)
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

            DateTime? ondates = null;
            if (ondate != "")
            {
                ondates = DateTime.Parse(ondate, new CultureInfo("en-GB"));
            }
            var keepstk = stockable == true ? 1 : 0;
            var zerostock = zerostockitem == true ? 1 : 0;

            ddmc = ddmc != null ? ddmc : 0;
            var selitem = new SqlParameter("@ItemId", "0");
            var selmc = new SqlParameter("@MCId", (object)ddmc ?? DBNull.Value);
            var brand = new SqlParameter("@BrandId", "0");
            var stkble = new SqlParameter("@Stockble", (object)keepstk ?? DBNull.Value);
            var catgry = new SqlParameter("@CategoryId", "0");
            var fromdate= new SqlParameter("@fromdate", "");
            var todate= new SqlParameter("@todate", (object)ondates ?? DBNull.Value);
            var stype = new SqlParameter("@Stype", "1");

            IEnumerable<StockDetails> data = new List<StockDetails>();
            var datadd = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().ToList();
            if (zerostockitem == true)
            {
                data = datadd.Where(a => a.ITotalQty > 0).OrderBy(a => a.IItemName).ToList();
            }
            else
            {
                data = datadd.OrderBy(a => a.IItemName).ToList();
            }

            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var mydata = (PreCheck == Status.active || BusinessType == "Scaffold") ? data.OrderBy(a => a.IItemCode).ToList() : data;

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = mydata });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }

        #endregion

        #region Stock Between Dates

        public ActionResult StockBwDate()
        {
            companySet();
            var BOM = db.EnableSettings.Where(a => a.EnableType == "BOM").FirstOrDefault();
            var poscheck = BOM != null ? BOM.Status : Status.inactive;
            ViewBag.BOM = poscheck;

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

        public ActionResult GetBwDate(string fromd, string to, bool stockable,bool zerostockitem, long? ddmc)
        {

            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any() && ddmc == 0)
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
            if (fromd != "")
            {
                fdate = DateTime.Parse(fromd, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (to != "")
            {
                tdate = DateTime.Parse(to, new CultureInfo("en-GB").DateTimeFormat);
            }
            var itmids = db.Items.Where(o => o.KeepStock == true && o.Status == Status.active).Select(o => o.ItemID).ToList();
            List<StockDetails> data = new List<StockDetails>();

            var keepstk = stockable == true ? 1 : 0;
            var zerostock = zerostockitem == true ? 1 : 0;

            ddmc = ddmc != null ? ddmc : 0;
            foreach (var it in itmids)
            {
                var selitem = new SqlParameter("@ItemId", (object)it ?? DBNull.Value);
                var selmc = new SqlParameter("@MCId", (object)ddmc ?? DBNull.Value);
                var brand = new SqlParameter("@BrandId", "");
                var stkble = new SqlParameter("@Stockble", 1);
                var catgry = new SqlParameter("@CategoryId", "");
                var fromdate = new SqlParameter("@fromdate", (object)fdate ?? DBNull.Value);
                var todate = new SqlParameter("@todate", (object)tdate ?? DBNull.Value);
                var stype = new SqlParameter("@Stype", "1");

                var dataadd = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).FirstOrDefault();

                if (zerostockitem == true && dataadd.ITotalQty > 0)
                    data.Add(dataadd);
                else if (zerostockitem == false)
                    data.Add(dataadd);

            }
            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var sum = data.Sum(o => o.ITotalStockValue);
            var mydata = (PreCheck == Status.active || BusinessType == "Scaffold") ? data.OrderBy(a => a.IItemCode).ToList() : data;

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = mydata });
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
