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
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    public class ShowRoomItemForecastController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ShowRoomItemForecastController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
              public ActionResult indexshowroom()
        {
            ViewBag.Item = QkSelect.List(
                                       new List<SelectListItem>
                                       {
                                    new SelectListItem { Selected = false,Text = "Select Item", Value = "0"},
                                       }, "Value", "Text", 1);

            ViewBag.Supplier = QkSelect.List(
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

            long[] selmc = { 1, 2, 3, 4, 5, 6 };

            List<SelectFormat> use = new List<SelectFormat>();
            use.Add(new SelectFormat { id = 0, text = "All" });
            use.Add(new SelectFormat { id = 1, text = "Mother Company" });
            use.Add(new SelectFormat { id = 2, text = "abu dhabi" });
            use.Add(new SelectFormat { id = 3, text = "mussafa" });
            use.Add(new SelectFormat { id = 4, text = "aln" });
            use.Add(new SelectFormat { id = 5, text = "dubai" });
            use.Add(new SelectFormat { id = 6, text = "moderate" });
            use.Add(new SelectFormat { id = 7, text = "quick vision" });


            ViewBag.mc = new MultiSelectList(use, "id", "text", selmc);

            ViewBag.Brand = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                }, "Value", "Text", 0);

            ViewBag.Category = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                }, "Value", "Text", 0);
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

        public ActionResult Index()
        {
            ViewBag.Item = QkSelect.List(
                                       new List<SelectListItem>
                                       {
                                    new SelectListItem { Selected = false,Text = "Select Item", Value = "0"},
                                       }, "Value", "Text", 1);

            ViewBag.Supplier = QkSelect.List(
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

            var use = db.MCs.Select(s => new SelectFormat { id = s.MCId, text = s.MCName }).ToList();
            var initial = new SelectFormat() { id = 0, text = "All" };
            use.Insert(0, initial);
            long[] selmc = { 2, 3, 4, 5, 20047, 20069 };


            ViewBag.mc = new MultiSelectList(use, "id", "text", selmc);

            ViewBag.Brand = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                }, "Value", "Text", 0);

            ViewBag.Category = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                }, "Value", "Text", 0);
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
        public ActionResult MCenterReport()
        {
            ViewBag.MC = QkSelect.List(
                                  new List<SelectListItem>
                                  {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                  }, "Value", "Text", 0);

            return View();
        }
    
        public ActionResult GetStock2(string srchtxt, bool zerostockitem, long?[] ddmc, long? itemid, long? categories, long? brandId, long? supplier, string datefrom, string dateto, long? company)
        {

            DateTime today = System.DateTime.Now;
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            DateTime datefroms = DateTime.Parse(datefrom, new CultureInfo("en-GB"));
            DateTime datetos = DateTime.Parse(dateto, new CultureInfo("en-GB"));
            var datediff = (datetos - datefroms).TotalDays;

            if (!MCList.Any() && ddmc == null)
            {

                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }

            var MCArray = MCList.ToArray();
            foreach (var items in MCArray)
            {
                var foree = items;
            }
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var keepstk = 0;
            var zerostock = zerostockitem == true ? 1 : 0;
            string result="";
            List<StockDetailsmovement> dddata = new List<StockDetailsmovement>();

            List<StockDetailsmovement> datadd = new List<StockDetailsmovement>();
            string[] constrings = { "abudhabi", "musafa", "aln", "dubai", "moderate", "quickvision" };
            //supplier != 0 && itemid==0 && srchtxt==""
            List<StockDetailsmovement> data = new List<StockDetailsmovement>();
            if (company==-1)
            {
               
            }
            else
            {

                
                constrings[0] = constrings[(long)company];
                Array.Resize(ref constrings, 1);
            }
            foreach (string conn in constrings)
            {
                db = new ApplicationDbContext(conn);
                if (1 == 2)
                {
                    var supplitem = (from a in db.PurchaseEntrys
                                     join b in db.PEItemss on a.PurchaseEntryId equals b.PurchaseEntry
                                     where a.Supplier == supplier
                                     select new
                                     {
                                         b.Item
                                     }).Distinct().ToList();
                    if (ddmc != null)
                    {
                        decimal netmovment = 0;
                        for (var g = 0; g < ddmc.Count(); g++)
                        {

                            var ddmc1 = ddmc[g];
                            var itm = String.Join(",", supplitem.Select(o => o.Item).ToArray());
                            var selitem = new SqlParameter("@ItemId", itm);
                            var catgry = new SqlParameter("@CategoryId", "");

                            if (itemid == 0)
                                selitem = new SqlParameter("@ItemId", itemid);
                            else
                                selitem = new SqlParameter("@ItemId", itemid);
                            if (categories == 0)
                                catgry = new SqlParameter("@CategoryId", "");
                            else
                                catgry = new SqlParameter("@CategoryId", categories);

                            var brand = new SqlParameter("@BrandId", "");
                            if (brandId == 0)
                                brand = new SqlParameter("@BrandId", "");
                            else
                                brand = new SqlParameter("@BrandId", brandId);

                            var selmc = new SqlParameter("@MCId", ddmc1);

                            var stkble = new SqlParameter("@Stockble", keepstk);
                            var fromdate = new SqlParameter("@fromdate", datefroms);
                            var todate = new SqlParameter("@todate", datetos);
                            var stype = new SqlParameter("@Stype", "1");


                            var datadd1 = db.Database.SqlQueryDedup<StockDetailsmovement>("SP_AVCOMethod3 @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().ToList();

                            var i = 0;
                            for (i = 0; i < datadd1.Count(); i++)
                                datadd1[i].netvalue = 0;
                            for (i = 0; i < datadd1.Count; i++)
                            {


                                datadd1[i].currstock = GetItemWisestock(datadd1[i].IItemID, ddmc1);

                            }

                            if (dddata.Count() == 0)
                            {
                                dddata = datadd1;
                                for (i = 0; i < dddata.Count; i++)
                                {

                                    dddata[i].netvalue = getnetqty(ddmc1, datefroms, datetos, dddata[i].IItemID, supplier);
                                    datadd1[i].onweek = getnetqty(ddmc1, today.AddDays(-7), today, dddata[i].IItemID, supplier);
                                    datadd1[i].onemonth = getnetqty(ddmc1, today.AddMonths(-1), today, dddata[i].IItemID, supplier);
                                    datadd1[i].twomonth = getnetqty(ddmc1, today.AddMonths(-2), today, dddata[i].IItemID, supplier);
                                    datadd1[i].threemonth = getnetqty(ddmc1, today.AddMonths(-3), today, dddata[i].IItemID, supplier);

                                    datadd1[i].twelvemonth = getnetqty(ddmc1, today.AddMonths(-12), today, dddata[i].IItemID, supplier);


                                }
                            }
                            else
                            {

                                for (var h = 0; h < datadd1.Count(); h++)
                                {


                                    var itempresent = dddata.Where(o => o.IItemID == datadd1[h].IItemID).FirstOrDefault();
                                    if (itempresent != null)
                                    {
                                        int inx = dddata.FindIndex(o => o.IItemID == datadd1[h].IItemID);


                                        dddata[inx].currstock = itempresent.currstock + datadd1[h].currstock;
                                        netmovment = netmovment + (getnetqty(ddmc1, datefroms, datetos, datadd1[h].IItemID, supplier));
                                        dddata[inx].netvalue = dddata[inx].netvalue + getnetqty(ddmc1, datefroms, datetos, datadd1[h].IItemID, supplier);
                                        dddata[inx].onweek = dddata[inx].onweek + getnetqty(ddmc1, today.AddDays(-7), today, datadd1[h].IItemID, supplier);
                                        dddata[inx].onemonth = dddata[inx].onemonth + getnetqty(ddmc1, today.AddMonths(-1), today, datadd1[h].IItemID, supplier);
                                        dddata[inx].twomonth = dddata[inx].twomonth + getnetqty(ddmc1, today.AddMonths(-2), today, datadd1[h].IItemID, supplier);
                                        dddata[inx].threemonth = dddata[inx].threemonth + getnetqty(ddmc1, today.AddMonths(-3), today, datadd1[h].IItemID, supplier);

                                        dddata[inx].twelvemonth = dddata[inx].twelvemonth + getnetqty(ddmc1, today.AddMonths(-12), today, datadd1[h].IItemID, supplier);


                                    }
                                    else
                                    {


                                    }

                                }
                            }

                        }
                    }

                }
                else if (srchtxt != "")
                {
                    var supplitem = (from a in db.SalesEntrys
                                     join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                                     join e in db.Items on b.Item equals e.ItemID

                                     let supp = (from c in db.PurchaseEntrys
                                                 join d in db.PEItemss on c.PurchaseEntryId equals d.PurchaseEntry
                                                 where d.Item == b.Item &&

                                                 c.Supplier == supplier
                                                 select new
                                                 {
                                                     d.Item
                                                 }).FirstOrDefault()

                                     where
                                     (supplier == 0 || supplier == null || b.Item == supp.Item) &&
                                     (e.ItemName.Contains(srchtxt)) &&


                                     (EF.Functions.DateDiffDay(a.SEDate, datefroms) <= 0) &&
                                      (EF.Functions.DateDiffDay(a.SEDate, datetos) >= 0) &&
                                      b.ItemUnit == e.ItemUnitID
                                     select new
                                     {
                                         e.ItemID
                                     }).Distinct().ToList();
                    var itm = String.Join(",", supplitem.Select(o => o.ItemID).ToArray());

                    if (1 == 1)
                    {
                        decimal netmovment = 0;


                        if (1 == 1)
                        {
                            Int64? ddmc1 = 0;


                            var selitem = new SqlParameter("@ItemId", itm);
                            var catgry = new SqlParameter("@CategoryId", "");
                            if (itemid == 0)
                                selitem = new SqlParameter("@ItemId", itemid);
                            else
                                selitem = new SqlParameter("@ItemId", itemid);
                            if (categories == 0)
                                catgry = new SqlParameter("@CategoryId", "0");
                            else
                                catgry = new SqlParameter("@CategoryId", categories);

                            var brand = new SqlParameter("@BrandId", "");
                            if (brandId == 0)
                                brand = new SqlParameter("@BrandId", "");
                            else
                                brand = new SqlParameter("@BrandId", brandId);

                            var selmc = new SqlParameter("@MCId", ddmc1);

                            var stkble = new SqlParameter("@Stockble", keepstk);
                            var fromdate = new SqlParameter("@fromdate", datefroms);
                            var todate = new SqlParameter("@todate", datetos);
                            var stype = new SqlParameter("@Stype", "1");
                            var srchtextsql = new SqlParameter("@searchtext", srchtxt);
                            var suppliersql = new SqlParameter("@supplier", supplier);
                            string storepro = "";
                            if (ddmc1 == 0)
                                storepro = "SP_AVCOMethod5";
                            else
                                storepro = "SP_AVCOMethod6";
                            var datadd1 = db.Database.SqlQueryDedup<StockDetailsmovement>(storepro + " @supplier,@searchtext,@ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@Stype,@fromdate,@todate", suppliersql, srchtextsql, selitem, selmc, brand, stkble, catgry, stype, fromdate, todate).AsEnumerable().ToList();

                            var i = 0;
                            int starting = 0;






                            if (1 == 1)
                            {
                                if (dddata.Count() == 0)
                                    starting = 0;
                                else
                                    starting = dddata.Count();
                                dddata.AddRange(datadd1);
                                for (i = starting; i < dddata.Count; i++)
                                {

                                    dddata[i].netvalue = getnetqty(ddmc1, datefroms, datetos, dddata[i].IItemID, supplier);
                                    dddata[i].onweek = getnetqty(ddmc1, today.AddDays(-7), today, dddata[i].IItemID, supplier);
                                    dddata[i].onemonth = getnetqty(ddmc1, today.AddMonths(-1), today, dddata[i].IItemID, supplier);
                                    dddata[i].twomonth = getnetqty(ddmc1, today.AddMonths(-2), today, dddata[i].IItemID, supplier);
                                    dddata[i].threemonth = getnetqty(ddmc1, today.AddMonths(-3), today, dddata[i].IItemID, supplier);

                                    dddata[i].twelvemonth = getnetqty(ddmc1, today.AddMonths(-12), today, dddata[i].IItemID, supplier);


                                }
                            }
                            else
                            {

                                for (var h = 0; h < datadd1.Count(); h++)
                                {


                                    var itempresent = dddata.Where(o => o.IItemID == datadd1[h].IItemID).FirstOrDefault();
                                    if (itempresent != null)
                                    {
                                        int inx = dddata.FindIndex(o => o.IItemID == datadd1[h].IItemID);


                                        dddata[inx].currstock = itempresent.currstock + datadd1[h].currstock;
                                        netmovment = netmovment + (getnetqty(ddmc1, datefroms, datetos, datadd1[h].IItemID, supplier));
                                        dddata[inx].netvalue = dddata[inx].netvalue + getnetqty(ddmc1, datefroms, datetos, datadd1[h].IItemID, supplier);
                                        dddata[inx].onemonth = dddata[inx].onemonth + (getnetqty(ddmc1, datefroms, datetos, datadd1[h].IItemID, supplier) / (decimal)datediff) * 30;
                                        dddata[inx].onweek = dddata[inx].onweek + getnetqty(ddmc1, today.AddDays(-7), today, dddata[i].IItemID, supplier);
                                        dddata[inx].twomonth = dddata[inx].twomonth + getnetqty(ddmc1, today.AddMonths(-2), today, dddata[i].IItemID, supplier);
                                        dddata[inx].threemonth = dddata[inx].threemonth + getnetqty(ddmc1, today.AddMonths(-3), today, dddata[i].IItemID, supplier);

                                        dddata[inx].twelvemonth = dddata[inx].twelvemonth + getnetqty(ddmc1, today.AddMonths(-12), today, dddata[i].IItemID, supplier);


                                    }
                                    else
                                    {


                                    }

                                }
                            }
                        }
                    }
                }
                else
                {




                    decimal netmovment = 0;
                    if (1 == 1)
                    {
                        Int64? ddmc1 = 0;



                        var saleit = (from a in db.SalesEntrys
                                      join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                                      where (EF.Functions.DateDiffDay(a.SEDate, datefroms) <= 0) &&
                  (EF.Functions.DateDiffDay(a.SEDate, datetos) >= 0)
                                      select new
                                      {
                                          b.Item
                                      }).Distinct().ToList();
                        var itm = String.Join(",", saleit.Select(o => o.Item).ToArray());
                        if (1 == 1)
                        {
                            var selitem = new SqlParameter("@ItemId", itm);
                            var catgry = new SqlParameter("@CategoryId", "");
                            if (itemid == 0)
                                selitem = new SqlParameter("@ItemId", "");
                            else
                                selitem = new SqlParameter("@ItemId", itemid);
                            if (categories == 0)
                                catgry = new SqlParameter("@CategoryId", "0");
                            else
                                catgry = new SqlParameter("@CategoryId", categories);

                            var brand = new SqlParameter("@BrandId", "");
                            if (brandId == 0)
                                brand = new SqlParameter("@BrandId", "");
                            else
                                brand = new SqlParameter("@BrandId", brandId);

                            var selmc = new SqlParameter("@MCId", ddmc1);

                            var stkble = new SqlParameter("@Stockble", keepstk);
                            var fromdate = new SqlParameter("@fromdate", datefroms);
                            var todate = new SqlParameter("@todate", datetos);
                            var stype = new SqlParameter("@Stype", "1");
                            var srchtextsql = new SqlParameter("@searchtext", srchtxt);
                            var suppliersql = new SqlParameter("@supplier", supplier);
                            string storepro = "";
                            if (company == 1)//if (ddmc1 == 0)
                                storepro = "SP_AVCOMethod5";
                            else
                                storepro = "SP_AVCOMethod6";
                            var datadd1 = db.Database.SqlQueryDedup<StockDetailsmovement>(storepro + " @supplier,@searchtext,@ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@Stype,@fromdate,@todate", suppliersql, srchtextsql, selitem, selmc, brand, stkble, catgry, stype, fromdate, todate).AsEnumerable().ToList();


                            var i = 0;
                            int starting = 0;





                            if (1 == 1)
                            {
                                starting = dddata.Count();
                                dddata.AddRange(datadd1);
                                for (i = starting; i < dddata.Count; i++)
                                {

                                    dddata[i].netvalue = getnetqty(ddmc1, datefroms, datetos, dddata[i].IItemID, supplier);
                                    dddata[i].onweek = getnetqty(ddmc1, today.AddDays(-7), today, dddata[i].IItemID, supplier);
                                    dddata[i].onemonth = getnetqty(ddmc1, today.AddMonths(-1), today, dddata[i].IItemID, supplier);
                                    dddata[i].twomonth = getnetqty(ddmc1, today.AddMonths(-2), today, dddata[i].IItemID, supplier);
                                    dddata[i].threemonth = getnetqty(ddmc1, today.AddMonths(-3), today, dddata[i].IItemID, supplier);

                                    dddata[i].twelvemonth = getnetqty(ddmc1, today.AddMonths(-12), today, dddata[i].IItemID, supplier);


                                }
                            }
                            else
                            {

                                for (var h = 0; h < datadd1.Count(); h++)
                                {


                                    var itempresent = dddata.Where(o => o.IItemID == datadd1[h].IItemID).FirstOrDefault();
                                    if (itempresent != null)
                                    {
                                        int inx = dddata.FindIndex(o => o.IItemID == datadd1[h].IItemID);


                                        dddata[inx].currstock = itempresent.currstock + datadd1[h].currstock;
                                        netmovment = netmovment + (getnetqty(ddmc1, datefroms, datetos, datadd1[h].IItemID, supplier));
                                        dddata[inx].netvalue = dddata[inx].netvalue + getnetqty(ddmc1, datefroms, datetos, datadd1[h].IItemID, supplier);
                                        dddata[inx].onemonth = dddata[inx].onemonth + (getnetqty(ddmc1, datefroms, datetos, datadd1[h].IItemID, supplier) / (decimal)datediff) * 30;
                                        dddata[inx].onweek = dddata[inx].onweek + getnetqty(ddmc1, today.AddDays(-7), today, dddata[i].IItemID, supplier);
                                        dddata[inx].twomonth = dddata[inx].twomonth + getnetqty(ddmc1, today.AddMonths(-2), today, dddata[i].IItemID, supplier);
                                        dddata[inx].threemonth = dddata[inx].threemonth + getnetqty(ddmc1, today.AddMonths(-3), today, dddata[i].IItemID, supplier);

                                        dddata[inx].twelvemonth = dddata[inx].twelvemonth + getnetqty(ddmc1, today.AddMonths(-12), today, dddata[i].IItemID, supplier);


                                    }
                                    else
                                    {


                                    }

                                }
                            }
                        }

                    }

                }

                

                datadd = dddata;

                int total = datadd.Count();
                var datadup = datadd.ToList();


                //for supplier checking
                if (supplier != 0)
                {

                    for (int j = 0; (j < total); j++)
                    {
                        var dataz = issupplierexist(datadup[j].IItemID, supplier);

                        if (!dataz)

                            datadd.Remove(datadup[j]);

                    }

                }
                //end


                
            }
            if (zerostockitem == true)
            {
                data.AddRange(datadd.Where(a => a.netvalue != 0).OrderByDescending(a => a.netvalue).ToList());
            }
            else
            {
                data.AddRange(datadd.OrderByDescending(a => a.netvalue).ToList());
            }

            data = (from a in data
                                 group new
                                 {

                                     a.IItemName,
                                     a.IPurchasePrice,
                                     a.netvalue,
                                     a.currstock,
                                     a.onweek,
                                     a.onemonth,
                                     a.twomonth,
                                     a.threemonth,
                                     a.twelvemonth,


                                 } by a.IItemName into grp

                                 select new StockDetailsmovement
                                 {
                                     IItemID = 0,
                                     IItemCode = "",
                                     IItemName = grp.Key,
                                     IPurchasePrice = grp.Average(o => o.IPurchasePrice),
                                     netvalue = grp.Sum(o => o.netvalue),
                                     currstock = 0,
                                     onweek = grp.Sum(o => o.onweek),
                                     onemonth=grp.Sum(o=>o.onemonth),
                                     twomonth=grp.Sum(o=>o.twomonth),
                                     twelvemonth=grp.Sum(o=>o.twelvemonth),
                                 })
               .ToList();
            foreach (string con in constrings)
            {
                db = new ApplicationDbContext(con);
                for (int i = 0; i < data.Count(); i++)
                {
                    data[i].currstock = data[i].currstock + GetItemWisestock4showroom(data[i].IItemName);
                }
            }
            db = new ApplicationDbContext();
                    var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
                var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
                var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

                var mydata = (PreCheck == Status.active || BusinessType == "Scaffold") ? data.OrderBy(a => a.IItemCode).ToList() : data;

                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
             result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = mydata, ddmc, itemid, datefrom, dateto });
            
                var results = new ContentResult
                {
                    Content = result,
                    ContentType = "application/json"
                };

                return results;
           
          
        }

        private Decimal getnetqty(long? ddmc1, DateTime datefroms, DateTime dateto, long? itemid, long? supplierid)
        {
            decimal SENo = 0;
            decimal netqty2 = 0;
            var confactor = db.Items.Where(o => o.ItemID == itemid).Select(o => o.ConFactor).FirstOrDefault();

            var netqty = (decimal)(from a in db.SalesEntrys
                                   join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                                   join e in db.Items on b.Item equals e.ItemID

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
                                   b.Item == itemid &&

                                   (ddmc1 == 0 || a.MaterialCenter == ddmc1) &&
                                   (EF.Functions.DateDiffDay(a.SEDate, datefroms) <= 0) &&
                                    (EF.Functions.DateDiffDay(a.SEDate, dateto) >= 0) &&
                                    b.ItemUnit == e.ItemUnitID
                                   select new
                                   {
                                       ItemQuantity = b.ItemQuantity
                                   }).ToList().Sum(c => c.ItemQuantity);


            SENo = netqty * confactor;

            if (confactor != 1)
            {
                netqty2 = (from a in db.SalesEntrys
                           join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                           join e in db.Items on b.Item equals e.ItemID

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
                           b.Item == itemid && a.MaterialCenter == ddmc1 &&
                           (EF.Functions.DateDiffDay(a.SEDate, datefroms) <= 0) &&
                            (EF.Functions.DateDiffDay(a.SEDate, dateto) >= 0) &&
                            b.ItemUnit == e.SubUnitId
                           select new
                           {
                               ItemQuantity = b.ItemQuantity
                           }).ToList().Sum(c => c.ItemQuantity);


            }











            var totalqty = SENo + netqty2;


            var sales = totalqty / confactor;
            SENo = 0;
            confactor = db.Items.Where(o => o.ItemID == itemid).Select(o => o.ConFactor).FirstOrDefault();

            netqty = (decimal)(from a in db.SalesReturns
                               join b in db.SRItemss on a.SalesReturnId equals b.SalesReturnId
                               join e in db.Items on b.Item equals e.ItemID

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
                               b.Item == itemid && a.MaterialCenter == ddmc1 &&
                               (EF.Functions.DateDiffDay(a.SRDate, datefroms) <= 0) &&
                                (EF.Functions.DateDiffDay(a.SRDate, dateto) >= 0) &&
                                b.ItemUnit == e.ItemUnitID
                               select new
                               {
                                   ItemQuantity = b.ItemQuantity
                               }).ToList().Sum(c => c.ItemQuantity);


            SENo = netqty * confactor;

            if (confactor != 1)
            {

                netqty2 = (from a in db.SalesReturns
                           join b in db.SRItemss on a.SalesReturnId equals b.SalesReturnId
                           join e in db.Items on b.Item equals e.ItemID

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
                           b.Item == itemid && a.MaterialCenter == ddmc1 &&
                           (EF.Functions.DateDiffDay(a.SRDate, datefroms) <= 0) &&
                            (EF.Functions.DateDiffDay(a.SRDate, dateto) >= 0) &&
                            b.ItemUnit == e.SubUnitId
                           select new
                           {
                               ItemQuantity = b.ItemQuantity
                           }).ToList().Sum(c => c.ItemQuantity);










            }



            totalqty = SENo + netqty2;


            var salesreturn = totalqty / confactor;
            return sales - 0;
        }
        public ActionResult GetMc2(bool zerostockitem, long? ddmc, long? itemid, long? categories, string datefrom, string dateto)
        {
            itemid = 0;
            categories = 0;
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            DateTime datefroms = DateTime.Parse(datefrom, new CultureInfo("en-GB"));
            DateTime datetos = DateTime.Parse(dateto, new CultureInfo("en-GB"));
            if (!MCList.Any() && ddmc == 0)
            {

                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();
            foreach (var items in MCArray)
            {
                var foree = items;
            }
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            var keepstk = 0;
            var zerostock = zerostockitem == true ? 1 : 0;

            ddmc = ddmc != null ? ddmc : 0;
            var selitem = new SqlParameter("@ItemId", "");
            var catgry = new SqlParameter("@CategoryId", "");
            if (itemid == 0)
                selitem = new SqlParameter("@ItemId", "");
            else
                selitem = new SqlParameter("@ItemId", itemid);
            if (categories == 0)
                catgry = new SqlParameter("@CategoryId", "");
            else
                catgry = new SqlParameter("@CategoryId", categories);

            var selmc = new SqlParameter("@MCId", ddmc);
            var brand = new SqlParameter("@BrandId", "");
            var stkble = new SqlParameter("@Stockble", keepstk);
            var fromdate = new SqlParameter("@fromdate", datefroms);
            var todate = new SqlParameter("@todate", datetos);
            var stype = new SqlParameter("@Stype", "1");

            IEnumerable<StockDetailsmovement> data = new List<StockDetailsmovement>();
            var datadd = db.Database.SqlQueryDedup<StockDetailsmovement>("SP_AVCOMethod3 @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().ToList();
            var i = 0;
            for (i = 0; i < datadd.Count; i++)
            {

                datadd[i].currstock = GetItemWisestock(datadd[i].IItemID, ddmc);
            }
            if (zerostockitem == true)
            {
                data = datadd.Where(a => a.netvalue > 0).OrderByDescending(a => a.netvalue).ToList();
            }
            else
            {
                data = datadd.OrderByDescending(a => a.netvalue).ToList();
            }
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumn != "" && sortColumn != "IItemID")
                {
                    data = data.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
                }
            }
            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var mydata = (PreCheck == Status.active || BusinessType == "Scaffold") ? data.OrderBy(a => a.IItemCode).ToList() : data;

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = mydata, ddmc, itemid, datefrom, dateto });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;
        }
        //supplier checking function
        public bool issupplierexist(long? itemid, long? supplier)
        {
            var data = (from a in db.Suppliers
                        join b in db.PurchaseEntrys on a.SupplierID equals b.Supplier
                        join c in db.PEItemss on b.PurchaseEntryId equals c.PurchaseEntry
                        where c.Item == itemid && b.Supplier == supplier
                        select new
                        {
                            a.SupplierName
                        }
                      ).ToList().Count();
            if (data > 0)
                return true;
            else
                return false;
        }
        public decimal? GetItemWisestock4mc(long? itemid, long? ddmc)
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
            var selitem = new SqlParameter("@ItemId", itemid);
            var selmc = new SqlParameter("@MCId", ddmc);
            var brand = new SqlParameter("@BrandId", "0");

            //for supllier
            //end

            var stkble = new SqlParameter("@Stockble", "");
            var catgry = new SqlParameter("@CategoryId", "0");
            var stype = new SqlParameter("@Stype", "1");
            var serchitem = new SqlParameter("@searchtext", "");
            var suppliersql = new SqlParameter("@supplier", "0");
            string storepro = "";
            if (ddmc == 0)
                storepro = "SP_AVCOMethod5";
            else
                storepro = "SP_AVCOMethod6";
            var data = db.Database.SqlQueryDedup<StockDetails>(storepro + " @supplier,@searchtext,@ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@Stype", suppliersql, serchitem, selitem, selmc, brand, stkble, catgry, stype).AsEnumerable().OrderBy(a => a.IItemName).ToList();


            return data[0].ITotalQty;



        }
        public decimal? GetItemWisestock4showroom(string itemname)
        {
            var UserId = User.Identity.GetUserId();
          
            var AVCOMethod = db.EnableSettings.Where(a => a.EnableType == "AVCOMethod").FirstOrDefault();
            var AVCOMethods = AVCOMethod != null ? AVCOMethod.Status : Status.inactive;


            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            long itemid = db.Items.Where(o => o.ItemName == itemname).Select(o => o.ItemID).FirstOrDefault();

           long ddmc =  0;
            var selitem = new SqlParameter("@ItemId", itemid);
            var selmc = new SqlParameter("@MCId", ddmc);
            var brand = new SqlParameter("@BrandId", "0");

            //for supllier
            //end

            var stkble = new SqlParameter("@Stockble", "");
            var catgry = new SqlParameter("@CategoryId", "0");
            var stype = new SqlParameter("@Stype", "1");
            var serchitem = new SqlParameter("@searchtext", "");
            var suppliersql = new SqlParameter("@supplier", "0");
            string storepro = "";
            if (ddmc == 0)
                storepro = "SP_AVCOMethod5";
            else
                storepro = "SP_AVCOMethod6";
            var data = db.Database.SqlQueryDedup<StockDetails>(storepro + " @supplier,@searchtext,@ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@Stype", suppliersql, serchitem, selitem, selmc, brand, stkble, catgry, stype).AsEnumerable().OrderBy(a => a.IItemName).ToList();

            if (data.Count > 0)
                return data[0].ITotalQty;
            else
                return 0;



        }
        //end
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
            var selitem = new SqlParameter("@ItemId", itemid);
            var selmc = new SqlParameter("@MCId", ddmc);
            var brand = new SqlParameter("@BrandId", "0");

            //for supllier
            //end

            var stkble = new SqlParameter("@Stockble", "");
            var catgry = new SqlParameter("@CategoryId", "0");
            var fromdate = new SqlParameter("@fromdate", "");
            var todate = new SqlParameter("@todate", "");
            var stype = new SqlParameter("@Stype", "1");

            var data = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).ToList();


            return data[0].ITotalQty;



        }
        public JsonResult CategorySearch(string q, string x, string page)
        {

            var start = Convert.ToInt32(page);
            int pageSize = 10000;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                string[] items = q.Split(' ');
                List<SelectFormat> serialisedJson3 = new List<SelectFormat>();
                foreach (var qa in items)
                {
                    List<SelectFormat> serialisedJson2 = db.ItemCategorys.Select(b => new SelectFormat
                    {
                        text = b.ItemCategoryName, //each json object will have 
                        id = b.ItemCategoryID
                    })
                                      .OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();
                    serialisedJson3.AddRange(serialisedJson2);
                }
                serialisedJson = serialisedJson3;
            }
            else
            {
                serialisedJson = db.ItemCategorys.Select(b => new SelectFormat
                {
                    text = b.ItemCategoryName, //each json object will have 
                    id = b.ItemCategoryID
                }).OrderBy(b => b.text).Skip(skip).Take(pageSize).ToList();

            }//
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult BrandSearch(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";

            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.ItemBrands
                                  where (q == null || a.ItemBrandName.ToLower().Contains(q.ToLower()) || a.ItemBrandName.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = a.ItemBrandName,
                                      id = a.ItemBrandID
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.ItemBrands
                                  select new SelectFormat
                                  {
                                      text = a.ItemBrandName,
                                      id = a.ItemBrandID
                                  }).OrderBy(b => b.text).ToList();
            }

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        //------------Employee Attentance Report
        public ActionResult EmpAttentanceReport()
        {
            var created = db.Users.Select(s => new
            {
                ID = s.Id,
                Name = s.UserName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult GetReport(string user, string From, string To)
        {
            DateTime? exdate = null;
            DateTime? crdate = null;

            if (To != "")
            {
                exdate = DateTime.Parse(To, new CultureInfo("en-GB"));
            }
            if (From != "")
            {
                crdate = DateTime.Parse(From, new CultureInfo("en-GB"));
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


            var v = (from a in db.EmpAttendances
                     where (From == null || From == "" || EF.Functions.DateDiffDay(a.login, crdate) <= 0) &&
                     (To == null || To == "" || EF.Functions.DateDiffDay(a.login, exdate) >= 0)
                     group a by a.EmployeeName into g
                     join c in db.Users on g.Key equals c.Id
                     where (user == null || user == "" || c.Id == user)



                     select new
                     {

                         id = g.Min(x => x.Id),
                         user = c.UserName,
                     });


            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.id.ToString().ToLower().Equals(search.ToLower()));

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





    }
}
