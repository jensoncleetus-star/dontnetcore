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
    public class ForecastController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ForecastController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        [HttpGet]
        //  [QkAuthorize(Roles = "Dev,Get Start Attendance")]
        public ActionResult AttentanceCreate(long? Id)
        {



            if (db.EmpAttendances.Count() != 0)
            {
                var Userid = User.Identity.GetUserId();
                var max = from a in db.EmpAttendances
                          where a.EmployeeName == Userid
                          orderby a.Id descending
                          select a.Status;
                if (max.FirstOrDefault() == "Active")
                {
                    ViewBag.laststatus = max.FirstOrDefault();
                }
                else
                {
                    ViewBag.laststatus = "Expired";
                }

            }
            else
            {
                ViewBag.laststatus = "Expired";
            }

            var Useriddd = User.Identity.GetUserId();
            var dura = from a in db.EmpAttendances
                       where a.EmployeeName == Useriddd
                       orderby a.Id descending
                       select a.login;
            var durb = from a in db.EmpAttendances
                       where a.EmployeeName == Useriddd
                       orderby a.Id descending
                       select a.logout;
            var Durationsa = dura.FirstOrDefault();
            var Durationsb = durb.FirstOrDefault();
            if (Durationsb == null)
            {
                if (Durationsa != null)
                {
                    ViewBag.duration = Durationsa;
                }
                else
                {
                    ViewBag.duration = "0";
                }

            }
            else
            {
                ViewBag.duration = "0";
            }


            var created = db.Users.Select(s => new
            {
                ID = s.Id,
                Name = s.UserName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");

            var Useridd = User.Identity.GetUserId();
            var lname = from f in db.Users
                        where f.Id == Useridd
                        select f.UserName;
            ViewBag.lastname = lname.FirstOrDefault();


            return View();
        }

        [HttpPost]
        // [QkAuthorize(Roles = "Dev,Create Start Attendance")]
        public ActionResult AttentanceCreate(string login, string logout, string dura, string lat, string log)
        {

            bool stat = false;
            string msg;
            if (ModelState["ID"] != null)
            {
                ModelState["ID"].Errors.Clear();
            }
            if (ModelState.IsValid)
            {
                var Userid = User.Identity.GetUserId();

                var DateTime = System.DateTime.Now;
                if (login != null)
                {
                    var atttendance = new EmpAttendance
                    {
                        EmployeeName = User.Identity.GetUserId(),
                        Status = "Active",
                        login = DateTime,
                        latitude = lat,
                        logitude = log,
                    };
                    db.EmpAttendances.Add(atttendance);
                    db.SaveChanges();
                }
                if (logout != null)
                {
                    var maxo = from a in db.EmpAttendances
                               where a.EmployeeName == Userid
                               orderby a.login descending
                               select a.Id;
                    var lastid = maxo.FirstOrDefault();


                    EmpAttendance lastlog = db.EmpAttendances.Find(lastid);
                    var duration = (DateTime) - (lastlog.login);
                    lastlog.EmployeeName = User.Identity.GetUserId();
                    lastlog.Status = "Expired";
                    lastlog.logout = DateTime;
                    lastlog.endlatitude = lat;
                    lastlog.endlogitude = log;
                    db.Entry(lastlog).State = EntityState.Modified;
                    db.SaveChanges();

                }





            }
            msg = "Successfully Uploaded";
            stat = true;
            Success("Attendance Registered", true);
            return Redirect(ControllerContext.HttpContext.Request.GetUrlReferrer().ToString());
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
            use.Add(new SelectFormat { id = 1, text = "Mother Company" });
            use.Add(new SelectFormat { id = 2, text = "abu dhabi" });
            use.Add(new SelectFormat { id = 3, text = "mussafa" });
            use.Add(new SelectFormat { id = 4, text = "aln" });
            use.Add(new SelectFormat { id = 5, text = "dubai" });
            use.Add(new SelectFormat { id = 6, text = "moderate" });


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
        public ActionResult Indexnew()
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
            long[] selmc = { 20085,
20086,
20087 };


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
        public ActionResult stockcomparison()
        {
            var mcs = db.MCs.Select(s => new
            {
                McId = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.MCbag = QkSelect.List(mcs, "McId", "Name");

            return View();
        }
        public List<StockDetailsmovement>  getforecast(long ddmcfrom, long ddmcto, string datefrom, string dateto)
        {
            long itemid = 0;
            int recordsTotal = 0;
            DateTime today = System.DateTime.Now;
            string srchtxt = "";
            var keepstk = 0;
            var zerostock = 0;
            List<StockDetailsmovement> dddata = new List<StockDetailsmovement>();

            db.SetCommandTimeOut(60 * 60);




            DateTime datefroms = DateTime.Parse(datefrom, new CultureInfo("en-GB"));
            DateTime datetos = DateTime.Parse(dateto, new CultureInfo("en-GB"));
            var datediff = (datetos - datefroms).TotalDays;
            long supplier = 0;
            decimal netmovment = 0;

            Int64? ddmc1 = 0;

            ddmc1 = ddmcfrom;
            long[] ddmc = { ddmcfrom };


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

                selitem = new SqlParameter("@ItemId", "");
                catgry = new SqlParameter("@CategoryId", "0");

                var brand = new SqlParameter("@BrandId", "");
                brand = new SqlParameter("@BrandId", "");

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
                for (i = 0; i < datadd1.Count; i++)
                {

                    datadd1[i].currstock = Convert.ToDecimal(datadd1[i].IPartNumber); //GetItemWisestock4mc(datadd1[i].IItemID, ddmc1);


                    datadd1[i].suppliername = suppliername(datadd1[i].IItemID);
                }

                if (dddata.Count() == 0)
                {
                    dddata = datadd1;
                    for (i = 0; i < dddata.Count; i++)
                    {

                        dddata[i].netvalue = getnetqty(ddmc1, datefroms, datetos, dddata[i].IItemID, supplier);
                        datadd1[i].onemonth = getnetqty(ddmc1, today.AddMonths(-1), today, dddata[i].IItemID, supplier);



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
                            //


                        }
                        else
                        {


                        }

                    }
                }


            }



            List<StockDetailsmovement> data = new List<StockDetailsmovement>();
            var datadd = dddata.ToList();

            int total = datadd.Count();

            var datadup = datadd.ToList();
            data = datadd.Where(a => a.netvalue != 0).OrderByDescending(a => a.netvalue).ToList();
            return data;


        }
        public decimal? getonemonthforcast(StockDetailsmovement v)
        {
            decimal? purchaseqty = 0;
            decimal? currentStock = 0;
            if (v.currstock < 0)
            {
                currentStock = 0;
            }
            else
            {
                currentStock = v.currstock;
            }
            var moment = Math.Abs(1 * v.onemonth);
            if (currentStock > moment)
            {
                purchaseqty = 0;
            }

            else
            {
                var balacestock = currentStock - (moment);

                if (balacestock > moment)
                {
                    purchaseqty = 0;
                }
                else if (balacestock < 0)
                {
                    decimal? Revbalancestock = (moment - currentStock);

                    if (Revbalancestock == moment)
                    {
                        purchaseqty = Revbalancestock;


                    }
                    else if (Revbalancestock < moment)
                    {

                        purchaseqty = moment - (moment - Revbalancestock);

                    }
                }
                else
                {
                    if (balacestock > 0)
                    {
                        if (balacestock > moment)
                        {
                            purchaseqty = 0;
                        }
                        else
                        {
                            purchaseqty = moment - (moment - balacestock);
                        }
                    }
                    else
                    {
                        purchaseqty = moment;
                    }
                }
            }
            return purchaseqty;
        }
        [HttpPost]
        public ActionResult GetStockcomparison(long ddmcfrom,long ddmcto,string datefrom,string dateto)
            {
            long itemid = 0;
            int recordsTotal = 0;
            DateTime today = System.DateTime.Now;
            string srchtxt = "";
            var keepstk = 0;
            var zerostock = 0;
            DateTime datefroms = DateTime.Parse(datefrom, new CultureInfo("en-GB"));
            DateTime datetos = DateTime.Parse(dateto, new CultureInfo("en-GB"));
            var datediff = (datetos - datefroms).TotalDays;
            long supplier = 0;
            decimal netmovment = 0;

            Int64? ddmc1 = 0;

            ddmc1 = ddmcfrom;
            long[] ddmc = { ddmcfrom };

            var datafirst = getforecast(ddmcfrom, ddmcto, datefrom, dateto);

            var data = getforecast(ddmcto, ddmcto, datefrom, dateto);
            //for supplier checking
            List<StockDetailsmovement> datafinal = new List<StockDetailsmovement>();
            foreach(var v in datafirst)
            {
               var datasec= data.Where(o => o.IItemID == v.IItemID).FirstOrDefault();
             
                if(datasec!=null)
                {
                    v.onemonthmcto = datasec.onemonth;
                    v.currstockmcto = datasec.currstock;
                    var firstoneforcast = getonemonthforcast(v);
                    var secondoneforcast = getonemonthforcast(datasec);
                    if (firstoneforcast > secondoneforcast&& firstoneforcast>0)
                    {
                        datafinal.Add(v);
                    }
                }
                else
                {
                    var firstoneforcast = getonemonthforcast(v);
                    if (firstoneforcast > 0)
                    {
                        v.onemonthmcto = 0;
                        v.currstockmcto = com.GetItemWisestock(v.IItemID, ddmcto);
                        datafinal.Add(v);
                    }
                }
            }
            //end




            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();


            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;



            string result = javaScriptSerializer.Serialize(new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = datafinal, ddmc, itemid, datefrom, dateto });

            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };

            return results;


          
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
            long[] selmc = { 20085,
20086,
20087 ,20084};

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
        /*public ActionResult GetStockshowroom(string srchtxt, bool zerostockitem,  long? itemid, long? categories, long? brandId, long? supplier, string datefrom, string dateto)
        {

            DateTime today = System.DateTime.Now;
            var UserId = User.Identity.GetUserId();
           
            DateTime datefroms = DateTime.Parse(datefrom, new CultureInfo("en-GB"));
            DateTime datetos = DateTime.Parse(dateto, new CultureInfo("en-GB"));
            var datediff = (datetos - datefroms).TotalDays;

            
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

           
            IEnumerable<StockDetailsmovement> data = new List<StockDetailsmovement>();
            IEnumerable<StockDetailsmovement> dataabudabi = new List<StockDetailsmovement>();
            IEnumerable<StockDetailsmovement> datamussafa = new List<StockDetailsmovement>();
            IEnumerable<StockDetailsmovement> dataalin = new List<StockDetailsmovement>();
            IEnumerable<StockDetailsmovement> datadubai = new List<StockDetailsmovement>();
            IEnumerable<StockDetailsmovement> datamoderate = new List<StockDetailsmovement>();
            //supplier != 0 && itemid==0 && srchtxt==""
            db = new ApplicationDbContext("abudhabi");
            {
                List<StockDetailsmovement> dddata = new List<StockDetailsmovement>();
                if (srchtxt != "")
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
                            for (i = 0; i < datadd1.Count; i++)
                            {

                                datadd1[i].currstock = GetItemWisestock(datadd1[i].IItemID, ddmc1);// GetItemWisestock(datadd1[i].IItemID, ddmc1);
                                datadd1[i].invoicecount = getinvoicecount(datadd1[i].IItemID, ddmc1, datefrom, dateto);//GetItemWisestock4mc(datadd1[i].IItemID, ddmc1);



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
                    if (1 == 1)
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
                                if (ddmc1 == 0)
                                    storepro = "SP_AVCOMethod5";
                                else
                                    storepro = "SP_AVCOMethod6";
                                var datadd1 = db.Database.SqlQueryDedup<StockDetailsmovement>(storepro + " @supplier,@searchtext,@ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@Stype,@fromdate,@todate", suppliersql, srchtextsql, selitem, selmc, brand, stkble, catgry, stype, fromdate, todate).AsEnumerable().ToList();


                                var i = 0;
                                for (i = 0; i < datadd1.Count; i++)
                                {

                                    datadd1[i].currstock = GetItemWisestock(datadd1[i].IItemID, ddmc1);
                                    datadd1[i].invoicecount = getinvoicecount(datadd1[i].IItemID, ddmc1, datefrom, dateto);//GetItemWisestock4mc(datadd1[i].IItemID, ddmc1);


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

                }

                var datadd = dddata;

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


                if (zerostockitem == true)
                {
                    dataabudabi = datadd.Where(a => a.netvalue != 0).OrderByDescending(a => a.netvalue).ToList();
                }
                else
                {
                    dataabudabi = datadd.OrderByDescending(a => a.netvalue).ToList();
                }

            }
            db = new ApplicationDbContext("musafa");
            {
                List<StockDetailsmovement> dddata = new List<StockDetailsmovement>();
                if (srchtxt != "")
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
                            for (i = 0; i < datadd1.Count; i++)
                            {

                                datadd1[i].currstock = GetItemWisestock(datadd1[i].IItemID, ddmc1);
                                datadd1[i].invoicecount = getinvoicecount(datadd1[i].IItemID, ddmc1, datefrom, dateto);//GetItemWisestock4mc(datadd1[i].IItemID, ddmc1);


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
                    if (1 == 1)
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
                                if (ddmc1 == 0)
                                    storepro = "SP_AVCOMethod5";
                                else
                                    storepro = "SP_AVCOMethod6";
                                var datadd1 = db.Database.SqlQueryDedup<StockDetailsmovement>(storepro + " @supplier,@searchtext,@ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@Stype,@fromdate,@todate", suppliersql, srchtextsql, selitem, selmc, brand, stkble, catgry, stype, fromdate, todate).AsEnumerable().ToList();


                                var i = 0;
                                for (i = 0; i < datadd1.Count; i++)
                                {

                                    datadd1[i].currstock = GetItemWisestock(datadd1[i].IItemID, ddmc1);
                                    datadd1[i].invoicecount = getinvoicecount(datadd1[i].IItemID, ddmc1, datefrom, dateto);//GetItemWisestock4mc(datadd1[i].IItemID, ddmc1);


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

                }

                var datadd = dddata;

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


                if (zerostockitem == true)
                {
                    datamussafa = datadd.Where(a => a.netvalue != 0).OrderByDescending(a => a.netvalue).ToList();
                }
                else
                {
                    datamussafa = datadd.OrderByDescending(a => a.netvalue).ToList();
                }

            }
            db = new ApplicationDbContext("aln");
            {
                List<StockDetailsmovement> dddata = new List<StockDetailsmovement>();
                if (srchtxt != "")
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
                            for (i = 0; i < datadd1.Count; i++)
                            {

                                datadd1[i].currstock = GetItemWisestock(datadd1[i].IItemID, ddmc1);
                                datadd1[i].invoicecount = getinvoicecount(datadd1[i].IItemID, ddmc1, datefrom, dateto);//GetItemWisestock4mc(datadd1[i].IItemID, ddmc1);


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
                    if (1 == 1)
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
                                if (ddmc1 == 0)
                                    storepro = "SP_AVCOMethod5";
                                else
                                    storepro = "SP_AVCOMethod6";
                                var datadd1 = db.Database.SqlQueryDedup<StockDetailsmovement>(storepro + " @supplier,@searchtext,@ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@Stype,@fromdate,@todate", suppliersql, srchtextsql, selitem, selmc, brand, stkble, catgry, stype, fromdate, todate).AsEnumerable().ToList();


                                var i = 0;
                                for (i = 0; i < datadd1.Count; i++)
                                {

                                    datadd1[i].currstock = GetItemWisestock(datadd1[i].IItemID, ddmc1);
                                    datadd1[i].invoicecount = getinvoicecount(datadd1[i].IItemID, ddmc1, datefrom, dateto);//GetItemWisestock4mc(datadd1[i].IItemID, ddmc1);


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

                }

                var datadd = dddata;

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


                if (zerostockitem == true)
                {
                    dataalin = datadd.Where(a => a.netvalue != 0).OrderByDescending(a => a.netvalue).ToList();
                }
                else
                {
                    dataalin = datadd.OrderByDescending(a => a.netvalue).ToList();
                }

            }
            db = new ApplicationDbContext("dubai");
            {
                List<StockDetailsmovement> dddata = new List<StockDetailsmovement>();
                if (srchtxt != "")
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
                    if (1 == 1)
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
                                if (ddmc1 == 0)
                                    storepro = "SP_AVCOMethod5";
                                else
                                    storepro = "SP_AVCOMethod6";
                                var datadd1 = db.Database.SqlQueryDedup<StockDetailsmovement>(storepro + " @supplier,@searchtext,@ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@Stype,@fromdate,@todate", suppliersql, srchtextsql, selitem, selmc, brand, stkble, catgry, stype, fromdate, todate).AsEnumerable().ToList();


                                var i = 0;
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

                }

                var datadd = dddata;

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


                if (zerostockitem == true)
                {
                    datadubai = datadd.Where(a => a.netvalue != 0).OrderByDescending(a => a.netvalue).ToList();
                }
                else
                {
                    datadubai = datadd.OrderByDescending(a => a.netvalue).ToList();
                }

            }
            db = new ApplicationDbContext("moderate");
            {
                List<StockDetailsmovement> dddata = new List<StockDetailsmovement>();
                if (srchtxt != "")
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
                    if (1 == 1)
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
                                if (ddmc1 == 0)
                                    storepro = "SP_AVCOMethod5";
                                else
                                    storepro = "SP_AVCOMethod6";
                                var datadd1 = db.Database.SqlQueryDedup<StockDetailsmovement>(storepro + " @supplier,@searchtext,@ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@Stype,@fromdate,@todate", suppliersql, srchtextsql, selitem, selmc, brand, stkble, catgry, stype, fromdate, todate).AsEnumerable().ToList();


                                var i = 0;
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

                }

                var datadd = dddata;

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


                if (zerostockitem == true)
                {
                    datamoderate = datadd.Where(a => a.netvalue != 0).OrderByDescending(a => a.netvalue).ToList();
                }
                else
                {
                    datamoderate = datadd.OrderByDescending(a => a.netvalue).ToList();
                }

            }
            data=data.Union(datadubai);
            data = data.Union(datamoderate);
            data = data.Union(dataalin);
            data = data.Union(datamussafa);
            data = data.Union(dataabudabi);

            var finaldata = (from a in data
                             group new { a } by new { a.IItemID, a.IItemCode, a.IItemName } into g
                             select new
                             {
                                 IItemID = g.Key.IItemID,
                                 IItemCode = g.Key.IItemCode,
                                 IItemName = g.Key.IItemName,
                                 IPurchasePrice = g.Max(o => o.a.IPurchasePrice),
                                 currstock = g.Sum(o => o.a.currstock),
                                 datediff = g.Max(o => o.a.datediff),
                                 netvalue = g.Sum(o => o.a.netvalue),
                                 onweek = g.Sum(o => o.a.onweek),
                                 onemonth = g.Sum(o => o.a.onemonth),

                                 twomonth = g.Sum(o => o.a.twomonth),
                                 threemonth = g.Sum(o => o.a.threemonth),
                                 twelvemonth = g.Sum(o => o.a.twelvemonth),

                             }
                    );
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumn != "" && sortColumn != "IItemID")
                {
                    finaldata = finaldata.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
                }
            }
            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var mydata = (PreCheck == Status.active || BusinessType == "Scaffold") ? finaldata.OrderBy(a => a.IItemCode).ToList() : finaldata;

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = mydata,  itemid, datefrom, dateto });

            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };

            return results;
        }
        */

        public int getinvoicecountall(long IItemID, long? ddmc1, string datefrom, string dateto, long?[] ddmc)
        {
            DateTime datefroms = DateTime.Parse(datefrom, new CultureInfo("en-GB"));
            DateTime datetos = DateTime.Parse(dateto, new CultureInfo("en-GB"));
            long?[] allmc = { 20085, 20086, 20087, 20084 };

            if (allmc.Count() > 1)
            {
                var c = (from a in db.SalesEntrys
                         join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                         where b.Item == IItemID && allmc.Contains(a.MaterialCenter)
                         && (EF.Functions.DateDiffDay(a.SEDate, datefroms) <= 0) &&
                     (EF.Functions.DateDiffDay(a.SEDate, datetos) >= 0)

                         select new
                         {
                             a.SEitem
                         }).Count();
                return c;
            }

            else
            {
                var c = (from a in db.SalesEntrys
                         join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                         where b.Item == IItemID && a.MaterialCenter == ddmc1
                         && (EF.Functions.DateDiffDay(a.SEDate, datefroms) <= 0) &&
                     (EF.Functions.DateDiffDay(a.SEDate, datetos) >= 0)

                         select new
                         {
                             a.SEitem
                         }).Count();

                return c;
            }
        }

        public int getinvoicecount(long IItemID, long? ddmc1, string datefrom, string dateto, long?[] ddmc)
        {
            DateTime datefroms = DateTime.Parse(datefrom, new CultureInfo("en-GB"));
            DateTime datetos = DateTime.Parse(dateto, new CultureInfo("en-GB"));
            if (ddmc.Count() > 1)
            { 
                var c = (from a in db.SalesEntrys
                         join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                         where b.Item == IItemID && ddmc.Contains(a.MaterialCenter)
                         && (EF.Functions.DateDiffDay(a.SEDate, datefroms) <= 0) &&
                     (EF.Functions.DateDiffDay(a.SEDate, datetos) >= 0)

                         select new
                         {
                             a.SEitem
                         }).Count();
            return c;
        }

            else
            { 
                var c = (from a in db.SalesEntrys
                         join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                         where b.Item == IItemID && a.MaterialCenter == ddmc1
                         && (EF.Functions.DateDiffDay(a.SEDate, datefroms) <= 0) &&
                     (EF.Functions.DateDiffDay(a.SEDate, datetos) >= 0)

                         select new
                         {
                             a.SEitem
                         }).Count();

            return c;
        }
        }
        public ActionResult GetStock2(long? invoicecount, string srchtxt, bool zerostockitem, bool zeroforcastitem, long?[] ddmc, long? itemid, long? categories, long? brandId, long? supplier, string datefrom, string dateto)
        {
            db.SetCommandTimeOut(60 * 60);
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

            int recordsTotal = 0;
            var keepstk = 0;
            var zerostock = zerostockitem == true ? 1 : 0;
            
            List<StockDetailsmovement> dddata = new List<StockDetailsmovement>();
            //supplier != 0 && itemid==0 && srchtxt==""
            if(itemid!=0)
            {
                srchtxt = db.Items.Where(o => o.ItemID == itemid).Select(o=>o.ItemName).FirstOrDefault();

            }
            if (supplier != 0 && itemid == 0 && srchtxt == "")
            {
                var saleit = (from a in db.SalesEntrys
                              join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                              where (EF.Functions.DateDiffDay(a.SEDate, datefroms) <= 0) &&
          (EF.Functions.DateDiffDay(a.SEDate, datetos) >= 0)
                              select new
                              {
                                  b.Item
                              }).Distinct();

                var supplitem = (from a in db.PurchaseEntrys
                                 join b in db.PEItemss on a.PurchaseEntryId equals b.PurchaseEntry
                                 join c in saleit on b.Item equals c.Item
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

                            datadd1[i].currstock = Convert.ToDecimal(datadd1[i].IPartNumber);// GetItemWisestock(datadd1[i].IItemID, ddmc1);
                            datadd1[i].invoicecount = getinvoicecount(datadd1[i].IItemID, ddmc1, datefrom, dateto,ddmc);//GetItemWisestock4mc(datadd1[i].IItemID, ddmc1);
                            datadd1[i].suppliername = suppliername(datadd1[i].IItemID);
                            datadd1[i].currstock = Convert.ToDecimal(datadd1[i].currstock);//GetItemWisestock4mc(datadd1[i].IItemID, ddmc1);

                        }

                        if (dddata.Count() == 0)
                        {
                            dddata = datadd1;
                            for (i = 0; i < dddata.Count; i++)
                            {

                                dddata[i].netvalue = getnetqty(ddmc1, datefroms, datetos, dddata[i].IItemID, supplier);
                                datadd1[i].onemonth = getnetqty(ddmc1, today.AddMonths(-1), today, dddata[i].IItemID, supplier);



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
                                  dddata[inx].onemonth = dddata[inx].onemonth + getnetqty(ddmc1, today.AddMonths(-1), today, datadd1[h].IItemID, supplier);

                                    dddata[inx].twelvemonth = dddata[inx].twelvemonth + getnetqty(ddmc1, today.AddMonths(-12), today, datadd1[h].IItemID, supplier);


                                }
                                else
                                {


                                    dddata.Add(datadd1[h]);
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

                if (ddmc != null&& itm!="")
                {
                    decimal netmovment = 0;


                    if (1 == 1)
                    {

                        foreach (var ddmc1 in ddmc)
                        {
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
                            for (i = 0; i < datadd1.Count; i++)
                            {
                                datadd1[i].currstock = Convert.ToDecimal(datadd1[i].IPartNumber);

                                datadd1[i].invoicecount = getinvoicecount(datadd1[i].IItemID, ddmc1, datefrom, dateto, ddmc);//GetItemWisestock4mc(datadd1[i].IItemID, ddmc1);
                                datadd1[i].suppliername = suppliername(datadd1[i].IItemID);

                            }

                            if (dddata.Count() == 0)
                            {
                                dddata = datadd1;
                                for (i = 0; i < dddata.Count; i++)
                                {

                                    dddata[i].netvalue = getnetqty(ddmc1, datefroms, datetos, dddata[i].IItemID, supplier);
                                    datadd1[i].onemonth = getnetqty(ddmc1, today.AddMonths(-1), today, dddata[i].IItemID, supplier);



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

                                        

                                    }
                                    else
                                    {


                                        dddata.Add(datadd1[h]);
                                    }

                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (ddmc != null)
                {




                    decimal netmovment = 0;
                    if (1 == 1)
                    {
                        Int64? ddmc1 = 0;
                        if (ddmc.Count() > 1)
                            ddmc1 = 0;// ddmc[g];
                        else
                            ddmc1 = ddmc[0];



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
                            if (ddmc1 == 0)
                                storepro = "SP_AVCOMethod5";
                            else
                                storepro = "SP_AVCOMethod6";
                            var datadd1 = db.Database.SqlQueryDedup<StockDetailsmovement>(storepro + " @supplier,@searchtext,@ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@Stype,@fromdate,@todate", suppliersql, srchtextsql, selitem, selmc, brand, stkble, catgry, stype, fromdate, todate).AsEnumerable().ToList();


                            var i = 0;
                            for (i = 0; i < datadd1.Count; i++)
                            {

                                datadd1[i].currstock =Convert.ToDecimal(datadd1[i].IPartNumber); //GetItemWisestock4mc(datadd1[i].IItemID, ddmc1);

                                datadd1[i].invoicecount = getinvoicecount(datadd1[i].IItemID,ddmc1, datefrom, dateto, ddmc);//GetItemWisestock4mc(datadd1[i].IItemID, ddmc1);

                                datadd1[i].suppliername = suppliername(datadd1[i].IItemID);
                            }

                            if (dddata.Count() == 0)
                            {
                                dddata = datadd1;
                                for (i = 0; i < dddata.Count; i++)
                                {

                                    dddata[i].netvalue = getnetqty(ddmc1, datefroms, datetos, dddata[i].IItemID, supplier);
                                    datadd1[i].onemonth = getnetqty(ddmc1, today.AddMonths(-1), today, dddata[i].IItemID, supplier);



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
                                        //


                                    }
                                    else
                                    {


                                    }

                                }
                            }
                        }

                    }
                }

            }
            IEnumerable<StockDetailsmovement> data = new List<StockDetailsmovement>();
            var datadd = dddata.Where(o=>(invoicecount==null||invoicecount==0||o.invoicecount>invoicecount)).ToList();

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


            if (zerostockitem == true)
            {
                data = datadd.Where(a => a.netvalue != 0).OrderByDescending(a => a.netvalue).ToList();
            }
            else
            {
                data = datadd.OrderByDescending(a => a.netvalue).ToList();
            }
            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var mydata = (PreCheck == Status.active || BusinessType == "Scaffold") ? data.OrderBy(a => a.IItemCode).ToList() : data;

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;



            string result = javaScriptSerializer.Serialize(new {  recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = mydata, ddmc, itemid, datefrom, dateto  });

            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };

            return results;
        }

        public ActionResult GetStock2all(long? invoicecount, string srchtxt, bool zerostockitem, bool zeroforcastitem,long?[] ddmc, long? itemid, long? categories, long? brandId, long? supplier, string datefrom, string dateto)
        {
            db.SetCommandTimeOut(60 * 60);
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

            int recordsTotal = 0;
            var keepstk = 0;
            var zerostock = zerostockitem == true ? 1 : 0;

            List<StockDetailsmovement> dddata = new List<StockDetailsmovement>();
            //supplier != 0 && itemid==0 && srchtxt==""
            if (itemid != 0)
            {
                srchtxt = db.Items.Where(o => o.ItemID == itemid).Select(o => o.ItemName).FirstOrDefault();

            }
            if (supplier != 0 && itemid == 0 && srchtxt == "")
            {
                var saleit = (from a in db.SalesEntrys
                              join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                              where (EF.Functions.DateDiffDay(a.SEDate, datefroms) <= 0) &&
          (EF.Functions.DateDiffDay(a.SEDate, datetos) >= 0)
                              select new
                              {
                                  b.Item
                              }).Distinct();

                var supplitem = (from a in db.PurchaseEntrys
                                 join b in db.PEItemss on a.PurchaseEntryId equals b.PurchaseEntry
                                 join c in saleit on b.Item equals c.Item
                                 where a.Supplier == supplier
                                 select new
                                 {
                                     b.Item
                                 }).Distinct().ToList();
                if (ddmc != null)
                {
                    decimal netmovment = 0;
                 

                        var ddmc1 = ddmc[0];
                        var itm = String.Join(",", supplitem.Select(o => o.Item).ToArray());
                        var selitem = new SqlParameter("@ItemId", itm);
                        var catgry = new SqlParameter("@CategoryId", "");

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


                        var datadd1 = db.Database.SqlQueryDedup<StockDetailsmovement>("SP_AVCOMethod33 @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().ToList();

                        var i = 0;
                        for (i = 0; i < datadd1.Count(); i++)
                            datadd1[i].netvalue = 0;
                        for (i = 0; i < datadd1.Count; i++)
                        {

                            datadd1[i].invoicecount = getinvoicecountall(datadd1[i].IItemID, ddmc1, datefrom, dateto, ddmc);//GetItemWisestock4mc(datadd1[i].IItemID, ddmc1);
                            datadd1[i].suppliername = suppliername(datadd1[i].IItemID);
                            datadd1[i].currstock = Convert.ToDecimal(datadd1[i].IPartNumber);//GetItemWisestock4mc(datadd1[i].IItemID, ddmc1);

                        }

                        if (dddata.Count() == 0)
                        {
                            dddata = datadd1;
                            for (i = 0; i < dddata.Count; i++)
                            {

                                dddata[i].netvalue = getnetqtyall(ddmc1, datefroms, datetos, dddata[i].IItemID, supplier);
                                datadd1[i].onemonth = getnetqtyall(ddmc1, today.AddMonths(-1), today, dddata[i].IItemID, supplier);



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
                                    netmovment = netmovment + (getnetqtyall(ddmc1, datefroms, datetos, datadd1[h].IItemID, supplier));
                                    dddata[inx].netvalue = dddata[inx].netvalue + getnetqtyall(ddmc1, datefroms, datetos, datadd1[h].IItemID, supplier);
                                    dddata[inx].onemonth = dddata[inx].onemonth + getnetqtyall(ddmc1, today.AddMonths(-1), today, datadd1[h].IItemID, supplier);

                                    dddata[inx].twelvemonth = dddata[inx].twelvemonth + getnetqtyall(ddmc1, today.AddMonths(-12), today, datadd1[h].IItemID, supplier);


                                }
                                else
                                {


                                    dddata.Add(datadd1[h]);
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

                if (ddmc != null && itm != "")
                {
                    decimal netmovment = 0;


                    if (1 == 1)
                    {

                      
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

                            var selmc = new SqlParameter("@MCId", ddmc[0]);

                            var stkble = new SqlParameter("@Stockble", keepstk);
                            var fromdate = new SqlParameter("@fromdate", datefroms);
                            var todate = new SqlParameter("@todate", datetos);
                            var stype = new SqlParameter("@Stype", "1");
                            var srchtextsql = new SqlParameter("@searchtext", srchtxt);
                            var suppliersql = new SqlParameter("@supplier", supplier);
                            string storepro = "";
                           
                                storepro = "SP_AVCOMethod66";
                            var datadd1 = db.Database.SqlQueryDedup<StockDetailsmovement>(storepro + " @supplier,@searchtext,@ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@Stype,@fromdate,@todate", suppliersql, srchtextsql, selitem, selmc, brand, stkble, catgry, stype, fromdate, todate).AsEnumerable().ToList();

                            var i = 0;
                            for (i = 0; i < datadd1.Count; i++)
                            {
                                datadd1[i].currstock = Convert.ToDecimal(datadd1[i].IPartNumber);

                                datadd1[i].invoicecount = getinvoicecountall(datadd1[i].IItemID, ddmc[0], datefrom, dateto, ddmc);//GetItemWisestock4mc(datadd1[i].IItemID, ddmc1);
                                datadd1[i].suppliername = suppliername(datadd1[i].IItemID);

                            }

                            if (dddata.Count() == 0)
                            {
                                dddata = datadd1;
                                for (i = 0; i < dddata.Count; i++)
                                {

                                    dddata[i].netvalue = getnetqtyall(ddmc[0], datefroms, datetos, dddata[i].IItemID, supplier);
                                    datadd1[i].onemonth = getnetqtyall(ddmc[0], today.AddMonths(-1), today, dddata[i].IItemID, supplier);



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
                                        netmovment = netmovment + (getnetqtyall(ddmc[0], datefroms, datetos, datadd1[h].IItemID, supplier));
                                        dddata[inx].netvalue = dddata[inx].netvalue + getnetqtyall(ddmc[0], datefroms, datetos, datadd1[h].IItemID, supplier);
                                        dddata[inx].onemonth = dddata[inx].onemonth + (getnetqtyall(ddmc[0], datefroms, datetos, datadd1[h].IItemID, supplier) / (decimal)datediff) * 30;



                                    }
                                    else
                                    {


                                        dddata.Add(datadd1[h]);
                                    }

                                
                            }
                        }
                    }
                }
            }
            else
            {
                if (ddmc != null)
                {




                    decimal netmovment = 0;
                    if (1 == 1)
                    {
                        Int64? ddmc1 = 0;
                        if (ddmc.Count() > 1)
                            ddmc1 = 0;// ddmc[g];
                        else
                            ddmc1 = ddmc[0];



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
                            if (ddmc1 == 0)
                                storepro = "SP_AVCOMethod66";
                            else
                                storepro = "SP_AVCOMethod66";
                            var datadd1 = db.Database.SqlQueryDedup<StockDetailsmovement>(storepro + " @supplier,@searchtext,@ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@Stype,@fromdate,@todate", suppliersql, srchtextsql, selitem, selmc, brand, stkble, catgry, stype, fromdate, todate).AsEnumerable().ToList();


                            var i = 0;
                            for (i = 0; i < datadd1.Count; i++)
                            {

                                datadd1[i].currstock = Convert.ToDecimal(datadd1[i].IPartNumber); //GetItemWisestock4mc(datadd1[i].IItemID, ddmc1);

                                datadd1[i].invoicecount = getinvoicecountall(datadd1[i].IItemID, ddmc1, datefrom, dateto, ddmc);//GetItemWisestock4mc(datadd1[i].IItemID, ddmc1);

                                datadd1[i].suppliername = suppliername(datadd1[i].IItemID);
                            }

                            if (dddata.Count() == 0)
                            {
                                dddata = datadd1;
                                for (i = 0; i < dddata.Count; i++)
                                {

                                    dddata[i].netvalue = getnetqtyall(ddmc1, datefroms, datetos, dddata[i].IItemID, supplier);
                                    datadd1[i].onemonth = getnetqtyall(ddmc1, today.AddMonths(-1), today, dddata[i].IItemID, supplier);



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
                                        netmovment = netmovment + (getnetqtyall(ddmc1, datefroms, datetos, datadd1[h].IItemID, supplier));
                                        dddata[inx].netvalue = dddata[inx].netvalue + getnetqtyall(ddmc1, datefroms, datetos, datadd1[h].IItemID, supplier);
                                        dddata[inx].onemonth = dddata[inx].onemonth + (getnetqtyall(ddmc1, datefroms, datetos, datadd1[h].IItemID, supplier) / (decimal)datediff) * 30;
                                        //


                                    }
                                    else
                                    {


                                    }

                                }
                            }
                        }

                    }
                }

            }
            IEnumerable<StockDetailsmovement> data = new List<StockDetailsmovement>();
            var datadd = dddata.Where(o => (invoicecount == null || invoicecount == 0 || o.invoicecount > invoicecount)).ToList();

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


            if (zerostockitem == true)
            {
                data = datadd.Where(a => a.netvalue != 0).OrderByDescending(a => a.netvalue).ToList();
            }
            else
            {
                data = datadd.OrderByDescending(a => a.netvalue).ToList();
            }
            var EnablePrefix = db.EnableSettings.Where(a => a.EnableType == "EnablePrefixCode").FirstOrDefault();
            var PreCheck = EnablePrefix != null ? EnablePrefix.Status : Status.inactive;
            var BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var mydata = (PreCheck == Status.active || BusinessType == "Scaffold") ? data.OrderBy(a => a.IItemCode).ToList() : data;
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;



            string result = javaScriptSerializer.Serialize(new { recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = mydata, ddmc, itemid, datefrom, dateto });

            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };

            return results;
        }
        private Decimal getnetqtyall(long? ddmc1, DateTime datefroms, DateTime dateto, long? itemid, long? supplierid)
        {
            decimal SENo = 0;
            decimal netqty2 = 0;
            long?[] allmc = { 20085, 20086, 20087, 20084 };
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

                               allmc.Contains(a.MaterialCenter) &&
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
                           b.Item == itemid &&
                               allmc.Contains(a.MaterialCenter)  &&
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
                               b.Item == itemid &&
                               allmc.Contains(a.MaterialCenter) &&
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
                           b.Item == itemid &&
                               allmc.Contains(a.MaterialCenter) &&
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

        private Decimal getnetqty(long? ddmc1, DateTime datefroms, DateTime dateto,long? itemid,long? supplierid)
        {
            decimal SENo = 0;
            decimal netqty2 = 0;
            var confactor = db.Items.Where(o => o.ItemID == itemid).Select(o => o.ConFactor).FirstOrDefault();

            var netqty =(decimal) (from a in db.SalesEntrys
             join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                         join e in db.Items on b.Item equals e.ItemID

                         let supp =(from c in db.PurchaseEntrys
                       join d in db.PEItemss on c.PurchaseEntryId equals d.PurchaseEntry
                       where d.Item==b.Item && 
                      
                       c.Supplier==supplierid
                       select new
                       {
                           d.Item
                       }).FirstOrDefault()

             where
             (supplierid==0||supplierid==null|| b.Item==supp.Item) &&
             b.Item == itemid &&
             
             (ddmc1==0|| a.MaterialCenter == ddmc1) &&
             ( EF.Functions.DateDiffDay(a.SEDate, datefroms) <= 0) &&
              ( EF.Functions.DateDiffDay(a.SEDate, dateto) >= 0) &&
              b.ItemUnit==e.ItemUnitID
             select new
             {
                 ItemQuantity=b.ItemQuantity
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
            
            
            var sales= totalqty / confactor;
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
        public string suppliername(long? itemid)
        {
            DateTime supdate = DateTime.Now.AddMonths(-5);
            var data = (from a in db.Suppliers
                        join b in db.PurchaseEntrys on a.SupplierID equals b.Supplier
                        join c in db.PEItemss on b.PurchaseEntryId equals c.PurchaseEntry
                        where c.Item == itemid && b.PEDate >supdate
                        select new
                        {
                            a.SupplierName
                        }
                      ).Distinct().Select(o=>o.SupplierName).ToList().ToArray();
            return string.Join("<br/>", data);
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


            int recordsTotal = 0;


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
            var data = db.Database.SqlQueryDedup<StockDetails>(storepro+" @supplier,@searchtext,@ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@Stype", suppliersql,serchitem, selitem, selmc, brand, stkble, catgry, stype).AsEnumerable().OrderBy(a => a.IItemName).ToList();


            return data[0].ITotalQty;



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


       
            int recordsTotal = 0;
          


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
                     join c in db.Users on g.FirstOrDefault().EmployeeName equals c.Id
                     where (user == null || user == "" || c.Id == user)



                     select new
                     {

                         id = g.FirstOrDefault().Id,
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
