using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;
using System.Data;
using System.Collections;
using System.Drawing;
using System.Net.Mail;
using System.Text;
using System.Drawing.Drawing2D;
using System.Collections.ObjectModel;
using System.Net.Http;

namespace QuickSoft.Models
{
    public class Common
    {
        ApplicationDbContext db;
        public Common()
        {
            db = new ApplicationDbContext();
        }
        #region log
        // add log details 
        public bool islocked(string section,DateTime dt)
        {
            bool inlock = db.FieldMappingsLocks.Any(o => o.Section == section &&  dt>=o.fromdate && dt<=o.todate);
            return inlock;
        }
        public decimal? GetItemWisestock(long? itemid, long? ddmc)
        {

            var AVCOMethod = db.EnableSettings.Where(a => a.EnableType == "AVCOMethod").FirstOrDefault();
            var AVCOMethods = AVCOMethod != null ? AVCOMethod.Status : Status.inactive;


            ddmc = ddmc != null ? ddmc : 0;
            var selitem = new SqlParameter("@ItemId", itemid);
            var selmc = new SqlParameter("@MCId", ddmc);
            var brand = new SqlParameter("@BrandId", "0");
            var stkble = new SqlParameter("@Stockble", "");
            var catgry = new SqlParameter("@CategoryId", "0");
            var fromdate = new SqlParameter("@fromdate", "");
            var todate = new SqlParameter("@todate", "");
            var stype = new SqlParameter("@Stype", "1");

            // var cust = new SqlParameter("@Customer", DBNull.Value);
            var data = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).OrderBy(a => a.IItemName).ToList();

            if (data.Count() > 0)
                return (data[0].ITotalQty == null) ? 0 : data[0].ITotalQty;
            else
                return 0;



        }
        public string ResolveShortUrl(string shortUrl)
        {
            // Security (audit S17 / SSRF): only resolve Google-Maps short links. Refuse arbitrary or internal
            // hosts so a user-supplied "maps URL" can't make the server fetch intranet / cloud-metadata endpoints.
            if (string.IsNullOrWhiteSpace(shortUrl)
                || !System.Uri.TryCreate(shortUrl, System.UriKind.Absolute, out var _u)
                || (_u.Scheme != System.Uri.UriSchemeHttp && _u.Scheme != System.Uri.UriSchemeHttps)
                || !(_u.Host.EndsWith(".google.com", System.StringComparison.OrdinalIgnoreCase)
                     || _u.Host.Equals("google.com", System.StringComparison.OrdinalIgnoreCase)
                     || _u.Host.EndsWith("goo.gl", System.StringComparison.OrdinalIgnoreCase)))
                return shortUrl;
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3"); // Mimic a browser
                var response = httpClient.GetAsync(shortUrl, HttpCompletionOption.ResponseHeadersRead);
                return response.Result.RequestMessage.RequestUri.ToString(); // This will be the final redirected URL

            }
        }
        public Dictionary<string, object> ExtractCoordinates(string surl)
        {
            // var httpClient = new HttpClient();
            Dictionary<string, Object> ret = new Dictionary<string, object>();
            // Example URL: https://www.google.com/maps/preview/place/Brandenburg+Gate,+Pariser+Platz,+10117+Berlin,+Germany/@52.5162746,13.3777041,2428a,13.1y/data=!4m2!3m1!1s0x47a851c655f20989:0x26bbfb4e84674c63
            // httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3"); // Mimic a browser
            // var response = httpClient.GetAsync(surl, HttpCompletionOption.ResponseHeadersRead);
            // string fullUrl = response.Result.RequestMessage.RequestUri.ToString(); // This will be the final redirected URL
            try
            {
                string fullUrl = ResolveShortUrl(surl);
                fullUrl = ResolveShortUrl(fullUrl);
                var match = Regex.Match(fullUrl, @"@(-?\d+\.\d+),(-?\d+\.\d+)");
                var val = fullUrl.Split(',');
                var val1 = val[0].Split('/');
                var len = val1.Length;
                var lat = val1[len - 1];
                var val2 = val[1].Split('/');
                var log = val2[0];
                if (log.Contains("?"))
                {
                    var val3 = log.Split('?');
                    log = val3[0];
                }
                ret.Add("lat", lat.Replace("@", "").Replace("+", ""));
                ret.Add("log", log.Replace("@", "").Replace("+", ""));

                return ret;
                if (match.Success && match.Groups.Count == 3)
                {
                    //if (double.TryParse(match.Groups[1].Value, out double latitude) &&
                    //    double.TryParse(match.Groups[2].Value, out double longitude))
                    if (match.Groups[1].Value != "")
                    {
                        ret.Add("lat", match.Groups[1].Value);
                        ret.Add("log", match.Groups[2].Value);
                        return ret;

                    }
                }

                ret.Add("lat", "0");
                ret.Add("log", "0");
                return ret;
            }
            catch(Exception e)
            {
                ret.Add("lat", "0");
                ret.Add("log", "0");
                return ret;
            }
        }
        public void stocktransfertotask(string purpuse, long tomc, string userid, List<BOMItem> bitem)
        {
            StockTransfer sl = new StockTransfer();
            DateTime today = System.DateTime.Now;
            sl.STNo = GetVTNo();
            sl.Voucher = purpuse;
            sl.Date = today;
            sl.MCFrom = tomc;
            sl.MCTo = db.MCs.Where(o => o.MCName == "TASK CENTER").Select(o => o.MCId).SingleOrDefault();

            sl.Remarks = "Item used internal transfer";
            //TotalQuantity = Convert.ToDecimal(mtdata[7]),
            sl.TotalAmount = 0;
            sl.CreatedDate = today;

            sl.CreatedBy = userid;
            sl.Status = Status.active;
            sl.editable = choice.Yes;
            sl.Branch = 1;

            string str = "";
            StockType Stype = StockType.StockTransfer;
            sl.StockType = Stype;
            sl.Ref1 = "";
            sl.Ref2 = "";
            sl.Ref3 = "";
            sl.Ref4 = "";
            sl.Ref5 = "";

            db.StockTransfers.Add(sl);
            db.SaveChanges();

            Int64 STId = sl.Id;
            foreach (var arr in bitem)
            {
                if (arr.ItemId != 0 && arr.Quantity > 0)
                {


                    StockTransferItem mt = new StockTransferItem();

                    mt.StockTransferId = STId;
                    mt.Item = arr.ItemId;
                    mt.Unit = arr.Unit;
                    mt.Quantity = arr.Quantity;
                    mt.Price = db.Items.Where(o => o.ItemID == arr.ItemId).Select(o => o.PurchasePrice).SingleOrDefault();
                    mt.Amount = 0;
                    db.StockTransferItems.Add(mt);
                    db.SaveChanges();

                }
            }
        }
        public void stocktransfer(string purpuse, long tomc, string userid, List<BOMItem> bitem)
        {
            StockTransfer sl = new StockTransfer();
            DateTime today = System.DateTime.Now;
            sl.STNo = GetVTNo();
            sl.Voucher = purpuse;
            sl.Date = today.Date;
            sl.MCFrom = db.MCs.Where(o => o.MCName == "TASK CENTER").Select(o => o.MCId).SingleOrDefault();
            sl.MCTo = tomc;
            sl.Remarks = "Item used internal transfer";
            //TotalQuantity = Convert.ToDecimal(mtdata[7]),
            sl.TotalAmount = 0;
            sl.CreatedDate = today;

            sl.CreatedBy = userid;
            sl.Status = Status.active;
            sl.editable = choice.Yes;
            sl.Branch = 1;

            string str = "";
            StockType Stype = StockType.StockTransfer;
            sl.StockType = Stype;
            sl.Ref1 = "";
            sl.Ref2 = "";
            sl.Ref3 = "";
            sl.Ref4 = "";
            sl.Ref5 = "";

            db.StockTransfers.Add(sl);
            db.SaveChanges();

            Int64 STId = sl.Id;
            foreach (var arr in bitem)
            {
                if (arr.ItemId != 0 && arr.Quantity > 0)
                {


                    StockTransferItem mt = new StockTransferItem();

                    mt.StockTransferId = STId;
                    mt.Item = arr.ItemId;
                    mt.Unit = arr.Unit;
                    mt.Quantity = arr.Quantity;
                    mt.Price = db.Items.Where(o => o.ItemID == arr.ItemId).Select(o => o.PurchasePrice).SingleOrDefault();
                    mt.Amount = 0;
                    db.StockTransferItems.Add(mt);
                    db.SaveChanges();

                }
            }
        }
        public long GetVTNo()
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
        public long? getrackmcid(long mc, long? rack, long? shelf)
        {
            long? rackmcid = 0;
            rackmcid = db.rackmaterialcentres.Where(o => o.mcid == mc && o.shelfid == (long)shelf && o.rackid == (long)rack).Select(o => o.rackmcid).SingleOrDefault();
            return rackmcid;
        }
        public long? getshelfno(long rackmcid)
        {
            long? shelfid = 0;
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
        public void remideradd(string url, long assigned, string from, string note, long reference = 1)
        {
            var today = Convert.ToDateTime(System.DateTime.Now);


            var created = "";
            Reminder reminds = new Reminder();
            reminds.Reference = reference;
            reminds.Note = note;

            var rDate = System.DateTime.Now.Date;
            //seleted date added,for fullcalender
            TimeSpan time = rDate.TimeOfDay;
            DateTime date = rDate;


            reminds.RDate = date;
            reminds.Type = url;
            reminds.RStatus = "Close";
            reminds.RequestBy = from;

            reminds.CreatedBy = from;
            reminds.Status = Status.active;
            reminds.CreatedDate = today;
            db.Reminders.Add(reminds);
            db.SaveChanges();
            long Id = reminds.ReminderId;


            //Approved By

            ReminderAssigned remAs = new ReminderAssigned();

            remAs.ReminderId = Id;
            remAs.EntryId = 1;

            remAs.Type = "";
            remAs.EmployeeId = assigned;
            db.ReminderAssigneds.Add(remAs);
            db.SaveChanges();


        }
        public decimal getmaterialcost(long itemid, DateTime SEDate, decimal stock)
        {
            //#if DEBUG
            //            if (itemid == 22020)
            //                System.Diagnostics.Debugger.Break();
            //         #endif
            var data = (from a in db.PEItemss
                        join b in db.PurchaseEntrys on a.PurchaseEntry equals b.PurchaseEntryId
                        join c in db.Items on a.Item equals c.ItemID
                        where a.Item == itemid &&
                        b.PEDate <= SEDate
                        select new
                        {
                            a.ItemId,
                            // a.ItemQuantity,
                            ItemQuantity = (c.ItemUnitID == a.ItemUnit) ? a.ItemQuantity : a.ItemQuantity / c.ConFactor,
                            a.ItemUnitPrice

                        }).ToList();
            decimal stk = 0;
            decimal mtcost = 0;
            //if (stock > data.Sum(o => o.ItemQuantity))
            //{
            //    mtcost = stock *(db.Items.Where(o=>o.ItemID==itemid).Select(o=>o.PurchasePrice).SingleOrDefault());
            //    return mtcost;
            //}
            for (int i = 0; i < data.Count(); i++)
            {
                //if (stk == 0)
                //    stk = data[i].ItemQuantity;


                if (stock < 0)
                {
                    mtcost = stock * data[i].ItemUnitPrice;
                    return mtcost;
                }
                else if (stk >= stock)
                {
                    mtcost = mtcost + (stk - stock) * data[i].ItemUnitPrice;
                    return mtcost;
                }
                else
                {
                    if (stock < (stk + data[i].ItemQuantity) && i != 0)
                    {
                        stk = stk + (data[i].ItemQuantity - stock);
                        mtcost = mtcost + (data[i].ItemQuantity - stock) * data[i].ItemUnitPrice;

                    }
                    else if (stock < (stk + data[i].ItemQuantity))
                    {
                        stk = stk + (data[i].ItemQuantity - stock);
                        mtcost = mtcost + stock * data[i].ItemUnitPrice;

                    }
                    else
                    {
                        stk = stk + data[i].ItemQuantity;
                        mtcost = mtcost + data[i].ItemQuantity * data[i].ItemUnitPrice;
                    }
                }


            }

            return mtcost;
        }
        public class itemq
        {
            public long item { get; set; }
            public decimal ItemQuantity { get; set; }


        }
        public class itempur
        {
            public long ItemId { get; set; }
            // a.ItemQuantity,
            public decimal ItemQuantity { get; set; }
            public decimal ItemUnitPrice { get; set; }
            public long saleseid { get; set; }
            public long salesqty { get; set; }
            public long? itemunitid { get; set; }
            public decimal confactor { get; set; }
        }
        public decimal getbatchmaterialcost(DateTime SEDate)
        {
            //#if DEBUG
            //            if (itemid == 22020)
            //                System.Diagnostics.Debugger.Break();
            //         #endif

            var data = (from a in db.PEItemss
                        join b in db.PurchaseEntrys on a.PurchaseEntry equals b.PurchaseEntryId
                        join c in db.Items on a.Item equals c.ItemID
                        where
                        b.PEDate <= SEDate
                        select new itempur
                        {
                            ItemId = a.Item,
                            // a.ItemQuantity,
                            ItemQuantity = (c.ItemUnitID == a.ItemUnit) ? a.ItemQuantity : (c.SubUnitId == a.ItemUnit) ? a.ItemQuantity / c.ConFactor : a.ItemQuantity,
                            ItemUnitPrice = (c.ItemUnitID == a.ItemUnit) ? a.ItemUnitPrice : (c.SubUnitId == a.ItemUnit) ? a.ItemUnitPrice * c.ConFactor : a.ItemUnitPrice,
                            saleseid = 0,
                            salesqty = 0,
                            itemunitid = a.ItemUnit,
                            confactor = c.ConFactor
                        }).ToList();
            decimal stk = 0;
            decimal mtcost = 0;

            var saless = (from a in db.SalesEntrys
                          join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                          join c in db.Items on b.Item equals c.ItemID
                          where
                            a.SEDate <= SEDate
                          select new itempur
                          {
                              ItemId = b.Item,
                              ItemQuantity = (c.ItemUnitID == b.ItemUnit) ? b.ItemQuantity : (c.SubUnitId == b.ItemUnit) ? b.ItemQuantity / c.ConFactor : b.ItemQuantity,
                              itemunitid = b.ItemUnit,
                              confactor = c.ConFactor
                          }
                          ).ToList();
            for (int i = 0; i < saless.Count(); i++)
            {
                var sal = data.Where(o => o.ItemId == saless[i].ItemId).ToList();

                for (int j = 0; j < sal.Count(); j++)
                {
                    data.Remove(sal[j]);
                    if (sal[j].ItemQuantity >= saless[i].ItemQuantity)
                    {
                        sal[j].ItemQuantity = sal[j].ItemQuantity - saless[i].ItemQuantity;
                        data.Add(sal[j]);
                        break;
                    }



                    else
                    {

                        saless[i].ItemQuantity = saless[i].ItemQuantity - sal[j].ItemQuantity;
                        sal[j].ItemQuantity = 0;

                    }
                    data.Add(sal[j]);
                }

            }
            return data.Sum(o => o.ItemQuantity * o.ItemUnitPrice);
        }

        public decimal getbatchmaterialcostdt(DateTime SEDatefrom, DateTime SEDate)
        {
            //#if DEBUG
            //            if (itemid == 22020)
            //                System.Diagnostics.Debugger.Break();
            //         #endif

            var data = (from a in db.PEItemss
                        join b in db.PurchaseEntrys on a.PurchaseEntry equals b.PurchaseEntryId
                        join c in db.Items on a.Item equals c.ItemID
                        where
                        b.PEDate <= SEDate
                        select new itempur
                        {
                            ItemId = a.Item,
                            // a.ItemQuantity,
                            ItemQuantity = (c.ItemUnitID == a.ItemUnit) ? a.ItemQuantity : (c.SubUnitId == a.ItemUnit) ? a.ItemQuantity / c.ConFactor : a.ItemQuantity,
                            ItemUnitPrice = (c.ItemUnitID == a.ItemUnit) ? a.ItemUnitPrice : (c.SubUnitId == a.ItemUnit) ? a.ItemUnitPrice / c.ConFactor : a.ItemUnitPrice,
                            saleseid = 0,
                            salesqty = 0,
                            itemunitid = a.ItemUnit,
                            confactor = c.ConFactor
                        }).ToList();
            decimal stk = 0;
            decimal mtcost = 0;

            var saless = (from a in db.SalesEntrys
                          join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                          join c in db.Items on b.Item equals c.ItemID
                          where
                            a.SEDate <= SEDatefrom
                          select new itempur
                          {
                              ItemId = b.Item,
                              ItemQuantity = (c.ItemUnitID == b.ItemUnit) ? b.ItemQuantity : (c.SubUnitId == b.ItemUnit) ? b.ItemQuantity / c.ConFactor : b.ItemQuantity,
                              itemunitid = b.ItemUnit,
                              confactor = c.ConFactor
                          }
                          ).ToList();
            decimal salevalue = 0;
            for (int i = 0; i < saless.Count(); i++)
            {
                var sal = data.Where(o => o.ItemId == saless[i].ItemId).ToList();

                for (int j = 0; j < sal.Count(); j++)
                {
                    data.Remove(sal[j]);
                    if (sal[j].ItemQuantity >= saless[i].ItemQuantity)
                    {

                        sal[j].ItemQuantity = sal[j].ItemQuantity - saless[i].ItemQuantity;
                        data.Add(sal[j]);
                        break;
                    }



                    else
                    {

                        saless[i].ItemQuantity = saless[i].ItemQuantity - sal[j].ItemQuantity;
                        sal[j].ItemQuantity = 0;

                    }
                    data.Add(sal[j]);
                }

            }




            var salessfromto = (from a in db.SalesEntrys
                                join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                                join c in db.Items on b.Item equals c.ItemID
                                where
                                  a.SEDate >= SEDatefrom && a.SEDate <= SEDate
                                select new itempur
                                {
                                    ItemId = b.Item,
                                    ItemQuantity = (c.ItemUnitID == b.ItemUnit) ? b.ItemQuantity : (c.SubUnitId == b.ItemUnit) ? b.ItemQuantity / c.ConFactor : b.ItemQuantity,
                                    itemunitid = b.ItemUnit,
                                    confactor = c.ConFactor
                                }
                          ).ToList();
            for (int i = 0; i < salessfromto.Count(); i++)
            {
                var sal = data.Where(o => o.ItemId == salessfromto[i].ItemId).ToList();

                for (int j = 0; j < sal.Count(); j++)
                {
                    data.Remove(sal[j]);
                    if (sal[j].ItemQuantity >= salessfromto[i].ItemQuantity)
                    {
                        salevalue = salevalue + salessfromto[i].ItemQuantity * sal[j].ItemUnitPrice;

                        sal[j].ItemQuantity = sal[j].ItemQuantity - salessfromto[i].ItemQuantity;
                        data.Add(sal[j]);
                        break;
                    }



                    else
                    {
                        salevalue = salevalue + salessfromto[i].ItemQuantity * sal[j].ItemUnitPrice;

                        salessfromto[i].ItemQuantity = salessfromto[i].ItemQuantity - sal[j].ItemQuantity;
                        sal[j].ItemQuantity = 0;

                    }
                    data.Add(sal[j]);
                }

            }

            return salevalue;
        }
        public decimal getbatchmaterialcostsalesid(long salesid)
        {
            //#if DEBUG
            //            if (itemid == 22020)
            //                System.Diagnostics.Debugger.Break();
            //         #endif
            var secrdate = db.SalesEntrys.Where(o => o.SalesEntryId == salesid).Select(o => o.SECreatedDate).SingleOrDefault();
            var data = (from a in db.PEItemss
                        join b in db.PurchaseEntrys on a.PurchaseEntry equals b.PurchaseEntryId
                        join c in db.Items on a.Item equals c.ItemID
                        where
                        b.PECreatedDate < secrdate
                        select new itempur
                        {
                            ItemId = a.Item,
                            // a.ItemQuantity,
                            ItemQuantity = (c.ItemUnitID == a.ItemUnit) ? a.ItemQuantity : (c.SubUnitId == a.ItemUnit) ? a.ItemQuantity / c.ConFactor : a.ItemQuantity,
                            ItemUnitPrice = (c.ItemUnitID == a.ItemUnit) ? a.ItemUnitPrice : (c.SubUnitId == a.ItemUnit) ? a.ItemUnitPrice / c.ConFactor : a.ItemUnitPrice,
                            saleseid = 0,
                            salesqty = 0,
                            itemunitid = a.ItemUnit,
                            confactor = c.ConFactor
                        }).ToList();
            decimal stk = 0;
            decimal mtcost = 0;

            var saless = (from a in db.SalesEntrys
                          join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                          join c in db.Items on b.Item equals c.ItemID
                          where
                            a.SECreatedDate < secrdate
                          select new itempur
                          {
                              ItemId = b.Item,
                              ItemQuantity = (c.ItemUnitID == b.ItemUnit) ? b.ItemQuantity : (c.SubUnitId == b.ItemUnit) ? b.ItemQuantity / c.ConFactor : b.ItemQuantity,
                              itemunitid = b.ItemUnit,
                              confactor = c.ConFactor
                          }
                          ).ToList();
            decimal salevalue = 0;
            for (int i = 0; i < saless.Count(); i++)
            {
                var sal = data.Where(o => o.ItemId == saless[i].ItemId).ToList();

                for (int j = 0; j < sal.Count(); j++)
                {
                    data.Remove(sal[j]);
                    if (sal[j].ItemQuantity >= saless[i].ItemQuantity)
                    {

                        sal[j].ItemQuantity = sal[j].ItemQuantity - saless[i].ItemQuantity;
                        data.Add(sal[j]);
                        break;
                    }



                    else
                    {

                        saless[i].ItemQuantity = saless[i].ItemQuantity - sal[j].ItemQuantity;
                        sal[j].ItemQuantity = 0;

                    }
                    data.Add(sal[j]);
                }

            }




            var salessfromto = (from a in db.SalesEntrys
                                join b in db.SEItemss on a.SalesEntryId equals b.SalesEntry
                                join c in db.Items on b.Item equals c.ItemID
                                where
                                  a.SalesEntryId == salesid
                                select new itempur
                                {
                                    ItemId = b.Item,
                                    ItemQuantity = (c.ItemUnitID == b.ItemUnit) ? b.ItemQuantity : (c.SubUnitId == b.ItemUnit) ? b.ItemQuantity / c.ConFactor : b.ItemQuantity,
                                    itemunitid = b.ItemUnit,
                                    confactor = c.ConFactor
                                }
                          ).ToList();
            for (int i = 0; i < salessfromto.Count(); i++)
            {
                var sal = data.Where(o => o.ItemId == salessfromto[i].ItemId).ToList();

                for (int j = 0; j < sal.Count(); j++)
                {
                    data.Remove(sal[j]);
                    if (sal[j].ItemQuantity >= salessfromto[i].ItemQuantity)
                    {
                        salevalue = salevalue + salessfromto[i].ItemQuantity * sal[j].ItemUnitPrice;

                        sal[j].ItemQuantity = sal[j].ItemQuantity - salessfromto[i].ItemQuantity;
                        data.Add(sal[j]);
                        break;
                    }



                    else
                    {
                        salevalue = salevalue + salessfromto[i].ItemQuantity * sal[j].ItemUnitPrice;

                        salessfromto[i].ItemQuantity = salessfromto[i].ItemQuantity - sal[j].ItemQuantity;
                        sal[j].ItemQuantity = 0;

                    }
                    data.Add(sal[j]);
                }

            }

            return salevalue;
        }



        public int addlog(LogTypes type, string user, string LogSection, string LogTable, string LogIP, long LogID, string details = "")
        {
            var con = new LogManager
            {
                LogType = type,
                User = user,
                LogDetails = details,
                LogSection = LogSection,
                LogTable = LogTable,
                LogID = LogID.ToString(),
                LogIP = LogIP,
                Status = 1,
                LogTime = System.DateTime.Now
                //CreatedDate = System.DateTime.Now,
            };
            db.LogManagers.Add(con);
            return db.SaveChanges();
        }
        public int updateprotaskdate(long LogID)
        {
            long id = LogID;
            ProTask task = db.ProTasks.Find(id);
            task.logtime = System.DateTime.Now;

            db.Entry(task).State = EntityState.Modified;
            return db.SaveChanges();

        }
        public int updateleaddate(long LogID)
        {
            long id = LogID;
            Customer cst = db.Customers.Find(id);
            if (cst != null)
            {
                cst.logtime = System.DateTime.Now;

                db.Entry(cst).State = EntityState.Modified;
            }
            return db.SaveChanges();

        }


        #endregion
        #region Account Transaction
        // add account transaction
        public int addAccountTrasaction(decimal Debit, decimal Credit, long Account, string Purpose, long reference = 0, DC type = DC.Credit, DateTime? Date = null, bool? status = null, string Narration = null, long? project = null, long? task = null)
        {
            DateTime? accdate;
            var SDate = db.FinancialYears.Select(i => i.Start).FirstOrDefault();
            var Request = LegacyWeb.Current.Request;
            HttpCookie Newcookie = Request.Cookies["FinYearID"];
            if (SDate != null)
            {

                if (Newcookie != null)
                {
                    Int32 TempYearid = Convert.ToInt32(Newcookie.Value);
                    accdate = Date == null ? db.FinancialYears.Where(x => x.id == TempYearid).Select(i => i.Start).First() : Date;
                }
                else
                {
                    accdate = Date == null ? db.FinancialYears.Max(i => i.Start) : Date;
                }
            }
            else
            {
                accdate = Date == null ? Convert.ToDateTime("01/01/2010") : Date;
            }

            var acc = new AccountsTransaction
            {
                Debit = Debit,
                Credit = Credit,
                Account = Account,
                Purpose = Purpose,
                reference = reference,
                Type = type,
                Date = accdate,
                Status = status,
                Narration = Narration,
                Project = project,
                ProTask = task,
                CreatedDate = Convert.ToDateTime(System.DateTime.Now),
            };
            db.AccountsTransactions.Add(acc);

            return db.SaveChanges();
        }
        public int adddummyAccountTrasaction(decimal Debit, decimal Credit, long Account, string Purpose, long reference = 0, DC type = DC.Credit, DateTime? Date = null, bool? status = null, string Narration = null, long? project = null, long? task = null)
        {
            DateTime? accdate;
            var SDate = db.FinancialYears.Select(i => i.Start).FirstOrDefault();
            var Request = LegacyWeb.Current.Request;
            HttpCookie Newcookie = Request.Cookies["FinYearID"];
            if (SDate != null)
            {

                if (Newcookie != null)
                {
                    Int32 TempYearid = Convert.ToInt32(Newcookie.Value);
                    accdate = Date == null ? db.FinancialYears.Where(x => x.id == TempYearid).Select(i => i.Start).First() : Date;
                }
                else
                {
                    accdate = Date == null ? db.FinancialYears.Max(i => i.Start) : Date;
                }
            }
            else
            {
                accdate = Date == null ? Convert.ToDateTime("01/01/2010") : Date;
            }

            var acc = new dummyAccountsTransactions
            {
                Debit = Debit,
                Credit = Credit,
                Account = Account,
                Purpose = Purpose,
                reference = reference,
                Type = type,
                Date = accdate,
                Status = status,
                Narration = Narration,
                Project = project,
                ProTask = task,
                CreatedDate = Convert.ToDateTime(System.DateTime.Now),
            };
            db.dummyAccountsTransactions.Add(acc);
            return db.SaveChanges();
        }

        // Update account transaction
        public int UpdateAccountTrasaction(long id, decimal Debit, decimal Credit, long Account, string Purpose, long reference = 0, DC type = DC.Credit, DateTime? date = null, bool? status = null, long? project = null, long? task = null)
        {
            AccountsTransaction acc = db.AccountsTransactions.Find(id);
            acc.Debit = Debit;
            acc.Credit = Credit;
            acc.Account = Account;
            acc.Purpose = Purpose;
            acc.reference = reference;
            acc.Type = type;
            acc.Date = date;
            acc.Status = status;
            if (project != null)
            {
                acc.Project = project;
                acc.ProTask = task;
            }
            db.Entry(acc).State = EntityState.Modified;
            return db.SaveChanges();
        }
        // delete account transaction
        public bool DeleteAccountTransaction(string purpose, long reference = 0, DC type = DC.Credit)
        {
            db.AccountsTransactions.RemoveRange(db.AccountsTransactions.Where(a => a.Purpose == purpose && a.reference == reference && a.Type == type));
            int delete = db.SaveChanges();
            if (delete != 0)
                return true;
            else
                return false;
        }
        // delete all related account transaction
        public bool DeleteAllAccountTransaction(string purpose, long reference = 0)
        {
            db.AccountsTransactions.RemoveRange(db.AccountsTransactions.Where(a => a.Purpose == purpose && a.reference == reference));
            int delete = db.SaveChanges();
            if (delete != 0)
                return true;
            else
                return false;
        }
        #endregion
        public bool DeleteAlldummyAccountTransaction(string purpose, long reference = 0)
        {
            db.dummyAccountsTransactions.RemoveRange(db.dummyAccountsTransactions.Where(a => a.Purpose == purpose && a.reference == reference));
            int delete = db.SaveChanges();
            if (delete != 0)
                return true;
            else
                return false;
        }



        // Get all Modules
        //public object GetAllowedModules(string Userid)
        //{
        //    var v = (from a in db.AppModuless
        //             join b in db.AppModuless on a.ModulesID equals b.Parent into child
        //             from b in child.DefaultIfEmpty()
        //             join f in db.RoleGroups on a.SalesEntryId equals f.SalesEntryId into walk
        //             from f in walk.DefaultIfEmpty()
        //             join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry into pay
        //             from c in pay.DefaultIfEmpty()
        //             join d in db.Users on a.SECashier equals d.Id into user
        //             from d in user.DefaultIfEmpty()

        //             select new
        //             {

        //             });


        //    return v;
        //}

        // find account balance

        // Account Balance Calculation
        #region Account Balance
        public Dictionary<string, object> Accbalance(long id, string to = "")
        {
            string type = "Cr";
            decimal amount = 0;
            var acctype = "Other";
            DateTime? tdate = null;
            if (to != "")
            {
                tdate = DateTime.Parse(to, new CultureInfo("en-GB"));
            }
            // find parent group is expense or other
            Accounts acc = db.Accountss.Find(id);
            var cusparentid = new SqlParameter("@parentid", acc.Group);
            var cusgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", cusparentid).ToList();
            var cusgpid = cusgroupsdata.Where(a => a.AccountsGroupID == 13).SingleOrDefault();
            if (cusgpid != null)
            {
                acctype = "Expense";
            }
            // 

            var credit = (from b in db.AccountsTransactions where b.Account == id && b.Status == null && (to == "" || b.Date <= tdate) select b.Credit).AsEnumerable().DefaultIfEmpty(0).Sum();
            var debit = (from b in db.AccountsTransactions where b.Account == id && b.Status == null && (to == "" || b.Date <= tdate) select b.Debit).AsEnumerable().DefaultIfEmpty(0).Sum();


            if (debit > credit)
            {
                type = "Dr";
                amount = debit - credit;
            }
            else
            {
                type = "Cr";
                amount = credit - debit;
            }
            var balance = new Dictionary<string, object>();
            balance.Add("type", type);
            balance.Add("amount", amount);
            balance.Add("acctype", acctype);
            return balance;
        }
        // get Account type
        public string AccType(long AccId, long? group = null)
        {
            if (AccId != -1)
                group = db.Accountss.Where(a => a.AccountsID == AccId).Select(a => a.Group).FirstOrDefault();
            var groupid = new SqlParameter("@childid", group);
            var data = db.Database.SqlQueryRaw<AccountsGroup>("allparentGroups @childid", groupid).ToList();
            var actype = "";
            // Customer
            var cus = data.Where(a => a.Name == "Customer").Select(a => a.Name).FirstOrDefault();
            if (cus == "Customer")
            {
                actype = "Customer";
            }
            else
            {
                cus = data.Where(a => a.Name == "Supplier").Select(a => a.Name).FirstOrDefault();
                if (cus == "Supplier")
                {
                    actype = "Supplier";
                }
                else
                {
                    cus = data.Where(a => a.Name == "Expense" || a.Name == "Expenses (Indirect/Admn.)" || a.Name == "Expenses (Direct/Mfg.)").Select(a => a.Name).FirstOrDefault();
                    if (cus == "Expense")
                    {
                        actype = "Expense";
                    }
                    else
                    {
                        cus = data.Where(a => a.Name == "Bank Accounts").Select(a => a.Name).FirstOrDefault();
                        if (cus == "Bank Accounts")
                        {
                            actype = "Bank";
                        }
                        else
                        {
                            cus = data.Where(a => a.Name == "Cash-in-hand").Select(a => a.Name).FirstOrDefault();
                            if (cus == "Cash-in-hand")
                            {
                                actype = "Cash";
                            }
                            else
                            {
                                cus = data.Where(a => a.Name == "Duties & Taxes").Select(a => a.Name).FirstOrDefault();
                                if (cus == "Cash-in-hand")
                                {
                                    actype = "Duties & Taxes";
                                }
                                else
                                {
                                    actype = "Other";
                                }

                            }
                        }
                    }
                }
            }
            return actype;
        }
        public decimal? GetItemWisestock4(long? ddmc2, string ondate)
        {
            db.SetCommandTimeOut(60 * 60);
      
           DateTime? ondates = DateTime.Parse(ondate, new CultureInfo("en-GB"));

            var AVCOMethod = db.EnableSettings.Where(a => a.EnableType == "AVCOMethod").FirstOrDefault();
            var AVCOMethods = AVCOMethod != null ? AVCOMethod.Status : Status.inactive;


 

            var stock = Convert.ToDecimal(0);
            var itmids = (from a in db.Items
                          where a.Status==Status.active && a.KeepStock==true
                          select new
                          {
                              Itemid = a.ItemID
                          }).Distinct();

           
            //var itm = String.Join(",", itmids.Select(o => o.Itemid).ToArray());
            var itemlist = itmids.Select(o => o.Itemid).Distinct().ToArray();
            var TotAmt = Convert.ToDecimal(0);
            int i = 0;
            long[][] arrays = itemlist.GroupBy(s => i++ / 100).Select(s => s.ToArray()).ToArray();
            
                foreach (var it in arrays)
                {
                    var itm = String.Join(",", it);
                    var selitem = new SqlParameter("@ItemId", itm);
                    var selmc = new SqlParameter("@MCId", ddmc2);
                    var brand = new SqlParameter("@BrandId", "0");
                 
                        brand = new SqlParameter("@BrandId", "0");
                    
                    var stkble = new SqlParameter("@Stockble", "1");
                    var searchtext = new SqlParameter("@searchtext", "");
                    
                    var catgry = new SqlParameter("@CategoryId", "0");
                    
                        catgry = new SqlParameter("@CategoryId", "0");
                    

                    var fromdate = new SqlParameter("@fromdate", "");
                    var todate = new SqlParameter("@todate", ondates);
                    var stype = new SqlParameter("@Stype", "1");
                    IEnumerable<StockDetails> data = new List<StockDetails>();
                    // var cust = new SqlParameter("@Customer", DBNull.Value);
                    var datadd = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod3 @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype,@searchtext", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype, searchtext).OrderBy(a => a.IItemName).ToList();
                    datadd = datadd.Where(o => o.ITotalQty != 0).ToList();
                    foreach (var datas in datadd)

                    {
                        var qty = Convert.ToDecimal(datas.ITotalQty);
                        var prc = Convert.ToDecimal(datas.ISellingPrice);
                        // TotAmt = TotAmt + (qty * prc);
                        TotAmt = TotAmt + (decimal)datas.ITotalStockValue;
                    }
                }
            
            

            return TotAmt;

        }

        // FIND Account OPENING BALANCE Do not use
        public Dictionary<string, object> AccOpenBlnc(long id, DateTime? to = null, string actype = "", bool? pdc = null)
        {
            string type = "Cr";
            decimal amount = 0;
            DateTime? tdate = null;
            Accounts acc = db.Accountss.Find(id);
            if (to != null)
            {
                //tdate = DateTime.Parse(to.ToString(), new CultureInfo("en-GB"));
                tdate = to;
            }
            if (actype == "Customer")
            {
                var Account = (from a in db.Customers
                               join b in db.Accountss on a.Accounts equals b.AccountsID
                               where a.Accounts == id
                               select new
                               {
                                   a.CustomerCode,
                                   a.CustomerName,
                                   a.CustomerID,
                                   b.OpnBalance,
                                   b.OpnBalanceCr
                               }).FirstOrDefault();
                var Sale = (from a in db.SalesEntrys
                            join b in db.Customers on a.Customer equals b.CustomerID
                            where (to == null || EF.Functions.DateDiffDay(a.SEDate, tdate) > 0) && b.Accounts == id
                            select new
                            {
                                Debit = (decimal?)a.SEGrandTotal
                            }).Sum(a => a.Debit) ?? 0;
                var SReturn = (from a in db.SalesReturns
                               join b in db.Customers on a.Customer equals b.CustomerID
                               where (to == null || EF.Functions.DateDiffDay(a.SRDate, tdate) > 0) && b.Accounts == id
                               select new
                               {
                                   Credit = (decimal?)a.SRGrandTotal,
                               }).Sum(a => a.Credit) ?? 0;
                var Reciept = (from a in db.Receipts
                               join b in db.Accountss on a.PayTo equals b.AccountsID
                               join c in db.Accountss on a.PayFrom equals c.AccountsID
                               let bb = db.AccountsTransactions.Any(at => at.Purpose == "Receipt" && at.reference == a.ReceiptId)
                               where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) && (a.PayFrom == id) &&
                               (pdc != true || bb || a.editable == choice.No)
                               select new
                               {
                                   Credit = (decimal?)a.Paying,
                               }).Sum(s => s.Credit) ?? 0;
                var Payment = (from a in db.Payments
                               join b in db.Accountss on a.PayFrom equals b.AccountsID
                               join c in db.Accountss on a.PayTo equals c.AccountsID
                               let bb = db.AccountsTransactions.Any(at => at.Purpose == "Payment" && at.reference == a.PaymentId)
                               where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) && a.PayTo == id &&
                               (pdc != true || bb || a.editable == choice.No)
                               select new
                               {
                                   Debit = (decimal?)a.Paying
                               }).Sum(s => s.Debit) ?? 0;
                var JournalDr = (from a in db.Journals
                                 join b in db.Accountss on a.PayFrom equals b.AccountsID
                                 join c in db.Accountss on a.PayTo equals c.AccountsID
                                 where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) && a.PayTo == id
                                 select new
                                 {
                                     Debit = (decimal?)a.Paying
                                 }).Sum(s => s.Debit) ?? 0;
                var JournalCr = (from a in db.Journals
                                 join b in db.Accountss on a.PayTo equals b.AccountsID
                                 join c in db.Accountss on a.PayFrom equals c.AccountsID
                                 where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) && a.PayFrom == id
                                 select new
                                 {
                                     Credit = (decimal?)a.Paying
                                 }).Sum(s => s.Credit) ?? 0;
                decimal OpnBalance = Account != null ? Account.OpnBalance : 0;
                decimal OpnBalanceCr = Account != null ? Account.OpnBalanceCr : 0;
                decimal debit = OpnBalance + JournalDr + Payment + Sale;
                decimal credit = OpnBalanceCr + JournalCr + Reciept + SReturn;
                amount = debit - credit;
                if (amount > 0)
                {
                    type = "Dr.";
                }
                else
                {
                    amount = Math.Abs(amount);
                }
            }
            else if (actype == "Supplier")
            {
                var Account = (from a in db.Suppliers
                               join b in db.Accountss on a.Accounts equals b.AccountsID
                               where a.Accounts == id
                               select new
                               {
                                   a.SupplierCode,
                                   a.SupplierName,
                                   a.SupplierID,
                                   b.OpnBalance,
                                   b.OpnBalanceCr
                               }).FirstOrDefault();

                var Purchase = (from a in db.PurchaseEntrys
                                join b in db.Suppliers on a.Supplier equals b.SupplierID
                                where (to == null || EF.Functions.DateDiffDay(a.PEDate, tdate) > 0) && b.Accounts == id
                                select new
                                {
                                    Credit = (decimal?)a.PEGrandTotal
                                }).Sum(s => s.Credit) ?? 0;
                var PReturn = (from a in db.PurchaseReturns
                               join b in db.Suppliers on a.Supplier equals b.SupplierID
                               where (to == null || EF.Functions.DateDiffDay(a.PRDate, tdate) > 0) && b.Accounts == id
                               select new
                               {
                                   Debit = (decimal?)a.PRGrandTotal
                               }).Sum(s => s.Debit) ?? 0;
                var Reciept = (from a in db.Receipts
                               join b in db.Accountss on a.PayTo equals b.AccountsID
                               join c in db.Accountss on a.PayFrom equals c.AccountsID
                               let bb = db.AccountsTransactions.Any(at => at.Purpose == "Receipt" && at.reference == a.ReceiptId)
                               where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) && a.PayFrom == id &&
                               (pdc != true || bb || a.editable == choice.No)
                               select new
                               {
                                   Credit = (decimal?)a.Paying,
                               }).Sum(s => s.Credit) ?? 0;
                var Payment = (from a in db.Payments
                               join b in db.Accountss on a.PayFrom equals b.AccountsID
                               join c in db.Accountss on a.PayTo equals c.AccountsID
                               let bb = db.AccountsTransactions.Any(at => at.Purpose == "Payment" && at.reference == a.PaymentId)
                               where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) && a.PayTo == id &&
                               (pdc != true || bb || a.editable == choice.No)
                               select new
                               {
                                   Debit = (decimal?)a.Paying,
                               }).Sum(s => s.Debit) ?? 0;
                var JournalDr = (from a in db.Journals
                                 join b in db.Accountss on a.PayFrom equals b.AccountsID
                                 join c in db.Accountss on a.PayTo equals c.AccountsID
                                 where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) && a.PayTo == id
                                 select new
                                 {
                                     Debit = (decimal?)a.Paying
                                 }).Sum(s => s.Debit) ?? 0;
                var JournalCr = (from a in db.Journals
                                 join b in db.Accountss on a.PayTo equals b.AccountsID
                                 join c in db.Accountss on a.PayFrom equals c.AccountsID
                                 where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) && a.PayFrom == id
                                 select new
                                 {
                                     Credit = (decimal?)a.Paying,
                                 }).Sum(s => s.Credit) ?? 0;

                decimal OpnBalance = Account != null ? Account.OpnBalance : 0;
                decimal OpnBalanceCr = Account != null ? Account.OpnBalanceCr : 0;
                decimal debit = OpnBalance + JournalDr + Payment + PReturn;
                decimal credit = OpnBalanceCr + JournalCr + Reciept + Purchase;
                amount = debit - credit;
                if (amount > 0)
                {
                    type = "Dr.";
                }
                else
                {
                    amount = Math.Abs(amount);
                }
            }
            else if (actype == "Expense")
            {

                var Account = (from a in db.Accountss
                               where a.AccountsID == id
                               select new
                               {
                                   a.AccountsID,
                                   a.Name,
                                   a.Alias,
                                   a.OpnBalance,
                                   a.OpnBalanceCr
                               }).FirstOrDefault();

                var Reciept = (from a in db.Receipts
                               join b in db.Accountss on a.PayTo equals b.AccountsID
                               join c in db.Accountss on a.PayFrom equals c.AccountsID
                               let bb = db.AccountsTransactions.Any(at => at.Purpose == "Receipt" && at.reference == a.ReceiptId)
                               where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) && a.PayFrom == id &&
                               (pdc != true || bb || a.editable == choice.No)
                               select new
                               {
                                   Credit = (decimal?)a.Paying,
                               }).Sum(s => s.Credit) ?? 0;
                var Payment = (from a in db.Payments
                               join b in db.Accountss on a.PayFrom equals b.AccountsID
                               join c in db.Accountss on a.PayTo equals c.AccountsID
                               let bb = db.AccountsTransactions.Any(at => at.Purpose == "Payment" && at.reference == a.PaymentId)
                               where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) && a.PayTo == id &&
                               (pdc != true || bb || a.editable == choice.No)
                               select new
                               {
                                   Debit = (decimal?)a.Paying,
                               }).Sum(s => s.Debit) ?? 0;
                var JournalDr = (from a in db.Journals
                                 join b in db.Accountss on a.PayFrom equals b.AccountsID
                                 join c in db.Accountss on a.PayTo equals c.AccountsID
                                 where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) && a.PayTo == id
                                 select new
                                 {
                                     Debit = (decimal?)a.Paying,
                                 }).Sum(s => s.Debit) ?? 0;
                var JournalCr = (from a in db.Journals
                                 join b in db.Accountss on a.PayTo equals b.AccountsID
                                 join c in db.Accountss on a.PayFrom equals c.AccountsID
                                 where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) && a.PayFrom == id
                                 select new
                                 {
                                     Credit = (decimal?)a.Paying
                                 }).Sum(s => s.Credit) ?? 0;
                decimal OpnBalance = Account != null ? Account.OpnBalance : 0;
                decimal OpnBalanceCr = Account != null ? Account.OpnBalanceCr : 0;
                decimal debit = OpnBalance + JournalDr + Payment;
                decimal credit = OpnBalanceCr + JournalCr + Reciept;
                amount = debit - credit;
                if (amount > 0)
                {
                    type = "Dr.";
                }
                else
                {
                    amount = Math.Abs(amount);
                }
            }
            else if (actype == "Bank" || actype == "Cash")
            {
                var Account = (from a in db.Accountss
                               where a.AccountsID == id
                               select new
                               {
                                   a.AccountsID,
                                   a.Name,
                                   a.Alias,
                                   a.OpnBalance,
                                   a.OpnBalanceCr
                               }).FirstOrDefault();

                var Reciept = (from a in db.Receipts
                               join b in db.Accountss on a.PayFrom equals b.AccountsID
                               join c in db.Accountss on a.PayTo equals c.AccountsID
                               join d in db.SalesEntrys on a.Reference equals d.SalesEntryId into sale
                               from d in sale.DefaultIfEmpty()
                               let bb = db.AccountsTransactions.Any(at => at.Purpose == "Receipt" && at.reference == a.ReceiptId)
                               where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) && a.PayTo == id &&
                               (pdc != true || bb || a.editable == choice.No)
                               select new
                               {
                                   Debit = (decimal?)a.Paying,
                               }).Sum(s => s.Debit) ?? 0;
                var Payment = (from a in db.Payments
                               join b in db.Accountss on a.PayTo equals b.AccountsID
                               join c in db.Accountss on a.PayFrom equals c.AccountsID
                               join d in db.PurchaseEntrys on a.Reference equals d.PurchaseEntryId into pur
                               from d in pur.DefaultIfEmpty()
                               let bb = db.AccountsTransactions.Any(at => at.Purpose == "Payment" && at.reference == a.PaymentId)
                               where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) && a.PayFrom == id &&
                               (pdc != true || bb || a.editable == choice.No)
                               select new
                               {
                                   Credit = (decimal?)a.Paying
                               }).Sum(s => s.Credit) ?? 0;
                var JournalDr = (from a in db.Journals
                                 join b in db.Accountss on a.PayFrom equals b.AccountsID
                                 join c in db.Accountss on a.PayTo equals c.AccountsID
                                 where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) && a.PayTo == id
                                 select new
                                 {
                                     Debit = (decimal?)a.Paying
                                 }).Sum(s => s.Debit) ?? 0;
                var JournalCr = (from a in db.Journals
                                 join b in db.Accountss on a.PayTo equals b.AccountsID
                                 join c in db.Accountss on a.PayFrom equals c.AccountsID
                                 where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) && a.PayFrom == id
                                 select new
                                 {
                                     Credit = (decimal?)a.Paying
                                 }).Sum(s => s.Credit) ?? 0;
                var ContrVDr = (from a in db.ContraVouchers
                                join b in db.Accountss on a.PayFrom equals b.AccountsID
                                join c in db.Accountss on a.PayTo equals c.AccountsID
                                where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) &&
                                a.PayTo == id
                                select new
                                {
                                    Debit = (decimal?)a.Amount
                                }).Sum(s => s.Debit) ?? 0;
                var ContrVCr = (from a in db.ContraVouchers
                                join b in db.Accountss on a.PayTo equals b.AccountsID
                                join c in db.Accountss on a.PayFrom equals c.AccountsID
                                where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) &&
                                a.PayFrom == id
                                select new
                                {
                                    Credit = (decimal?)a.Amount
                                }).Sum(s => s.Credit) ?? 0;
                decimal OpnBalance = Account != null ? Account.OpnBalance : 0;
                decimal OpnBalanceCr = Account != null ? Account.OpnBalanceCr : 0;
                decimal debit = OpnBalance + JournalDr + Reciept + ContrVDr;
                decimal credit = OpnBalanceCr + JournalCr + Payment + ContrVCr;
                amount = debit - credit;
                if (amount > 0)
                {
                    type = "Dr.";
                }
                else
                {
                    amount = Math.Abs(amount);
                }

            }
            else
            {
                var Account = (from a in db.Accountss
                               where a.AccountsID == id
                               select new
                               {
                                   a.AccountsID,
                                   a.Name,
                                   a.Alias,
                                   a.OpnBalance,
                                   a.OpnBalanceCr
                               }).FirstOrDefault();

                var Reciept = (from a in db.AccountsTransactions
                               join b in db.Accountss on a.Account equals b.AccountsID
                               join c in db.Receipts on a.reference equals c.ReceiptId
                               join d in db.Accountss on c.PayFrom equals d.AccountsID
                               where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) &&
                               a.Account == id && a.Purpose == "Receipt"
                               select new
                               {
                                   Debit = (decimal?)a.Debit,
                                   Credit = (decimal?)a.Credit,
                               });
                var Payment = (from a in db.AccountsTransactions
                               join b in db.Accountss on a.Account equals b.AccountsID
                               join c in db.Payments on a.reference equals c.PaymentId
                               join d in db.Accountss on c.PayTo equals d.AccountsID
                               where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) &&
                               a.Account == id && (a.Purpose == "Payment" || a.Purpose == "Expense Payment")
                               select new
                               {
                                   Debit = (decimal?)a.Debit,
                                   Credit = (decimal?)a.Credit,
                               });

                var Purchase = (from a in db.AccountsTransactions
                                join b in db.Accountss on a.Account equals b.AccountsID
                                join c in db.PurchaseEntrys on a.reference equals c.PurchaseEntryId
                                join d in db.Suppliers on c.Supplier equals d.SupplierID
                                where (to == null || EF.Functions.DateDiffDay(c.PEDate, tdate) > 0) &&
                                a.Account == id && a.Purpose == "Purchase"
                                select new
                                {
                                    Debit = (decimal?)a.Debit,
                                    Credit = (decimal?)a.Credit,
                                });
                var PReturn = (from a in db.AccountsTransactions
                               join b in db.Accountss on a.Account equals b.AccountsID
                               join c in db.PurchaseReturns on a.reference equals c.PurchaseReturnId
                               join d in db.Suppliers on c.Supplier equals d.SupplierID
                               where (to == null || EF.Functions.DateDiffDay(c.PRDate, tdate) > 0) &&
                               a.Account == id && a.Purpose == "Purchase Return"
                               select new
                               {
                                   Debit = (decimal?)a.Debit,
                                   Credit = (decimal?)a.Credit,
                               });
                var Sale = (from a in db.AccountsTransactions
                            join b in db.Accountss on a.Account equals b.AccountsID
                            join c in db.SalesEntrys on a.reference equals c.SalesEntryId
                            join d in db.Customers on c.Customer equals d.CustomerID
                            where (to == null || EF.Functions.DateDiffDay(c.SEDate, tdate) > 0) &&
                            a.Account == id && a.Purpose == "Sale"
                            select new
                            {
                                Debit = (decimal?)a.Debit,
                                Credit = (decimal?)a.Credit,
                            });
                var SReturn = (from a in db.AccountsTransactions
                               join b in db.Accountss on a.Account equals b.AccountsID
                               join c in db.SalesReturns on a.reference equals c.SalesReturnId
                               join d in db.Customers on c.Customer equals d.CustomerID
                               where (to == null || EF.Functions.DateDiffDay(c.SRDate, tdate) > 0) &&
                               a.Account == id && a.Purpose == "Sale Return"
                               select new
                               {
                                   Debit = (decimal?)a.Debit,
                                   Credit = (decimal?)a.Credit,
                               });

                //var JournalDr = (from a in db.Journals
                //                 join b in db.Accountss on a.PayFrom equals b.AccountsID
                //                 join c in db.Accountss on a.PayTo equals c.AccountsID
                //                 where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) && a.PayTo == id
                //                 select new
                //                 {
                //                     Debit = (decimal?)a.Paying,
                //                 }).Sum(s => s.Debit) ?? 0;
                //var JournalCr = (from a in db.Journals
                //                 join b in db.Accountss on a.PayTo equals b.AccountsID
                //                 join c in db.Accountss on a.PayFrom equals c.AccountsID
                //                 where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) && a.PayFrom == id
                //                 select new
                //                 {
                //                     Credit = (decimal?)a.Paying,
                //                 }).Sum(s => s.Credit) ?? 0;
                var Journal = (from a in db.AccountsTransactions
                               join b in db.Accountss on a.Account equals b.AccountsID
                               join c in db.Journals on a.reference equals c.JournalId
                               where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0)
                               && a.Purpose == "Journal" && a.Account == id
                               select new
                               {
                                   Debit = (decimal?)a.Debit,
                                   Credit = (decimal?)a.Credit,
                               });
                var Contra = (from a in db.AccountsTransactions
                              join b in db.Accountss on a.Account equals b.AccountsID
                              join c in db.ContraVouchers on a.reference equals c.ContraVoucherId
                              where (to == null || EF.Functions.DateDiffDay(a.Date, tdate) > 0) && a.Account == id
                              && a.Purpose == "ContraVoucher"
                              select new
                              {
                                  Debit = (decimal?)a.Debit,
                                  Credit = (decimal?)a.Credit,
                              });
                decimal OpnBalance = Account != null ? Account.OpnBalance : 0;
                decimal OpnBalanceCr = Account != null ? Account.OpnBalanceCr : 0;

                decimal SReturnCr = SReturn != null ? SReturn.Sum(s => s.Credit) ?? 0 : 0;
                decimal SReturnDr = SReturn != null ? SReturn.Sum(s => s.Debit) ?? 0 : 0;

                decimal SaleCr = Sale != null ? Sale.Sum(s => s.Credit) ?? 0 : 0;
                decimal SaleDr = Sale != null ? Sale.Sum(s => s.Debit) ?? 0 : 0;

                decimal PReturnCr = PReturn != null ? PReturn.Sum(s => s.Credit) ?? 0 : 0;
                decimal PReturnDr = PReturn != null ? PReturn.Sum(s => s.Debit) ?? 0 : 0;

                decimal PurchaseCr = Purchase != null ? Purchase.Sum(s => s.Credit) ?? 0 : 0;
                decimal PurchaseDr = Purchase != null ? Purchase.Sum(s => s.Debit) ?? 0 : 0;

                decimal PaymentCr = Payment != null ? Payment.Sum(s => s.Credit) ?? 0 : 0;
                decimal PaymentDr = Payment != null ? Payment.Sum(s => s.Debit) ?? 0 : 0;

                decimal RecieptCr = Reciept != null ? Reciept.Sum(s => s.Credit) ?? 0 : 0;
                decimal RecieptDr = Reciept != null ? Reciept.Sum(s => s.Debit) ?? 0 : 0;

                decimal JournalCr = Journal != null ? Journal.Sum(s => s.Credit) ?? 0 : 0;
                decimal JournalDr = Journal != null ? Journal.Sum(s => s.Debit) ?? 0 : 0;

                decimal ContraCr = Contra != null ? Contra.Sum(s => s.Credit) ?? 0 : 0;
                decimal ContraDr = Contra != null ? Contra.Sum(s => s.Debit) ?? 0 : 0;

                decimal debit = OpnBalance + JournalDr + RecieptDr + PaymentDr + PurchaseDr + PReturnDr + SaleDr + SReturnDr + ContraDr;
                decimal credit = OpnBalanceCr + JournalCr + RecieptCr + PaymentCr + PurchaseCr + PReturnCr + SaleCr + SReturnCr + ContraCr;
                amount = debit - credit;
                if (amount > 0)
                {
                    type = "Dr.";
                }
                else
                {
                    amount = Math.Abs(amount);
                }
            }
            var balance = new Dictionary<string, object>();
            balance.Add("type", type);
            balance.Add("amount", amount);
            balance.Add("acctype", actype);
            return balance;
        }

        // FIND OPENING BALANCE sum of group
        public Dictionary<string, object> GroupOpenBlnc(long id, DateTime? to = null, string actype = "", bool? pdc = null, long[] arry = null)
        {
            string type = "Cr";
            decimal Amount = 0;
            DateTime tdate = (DateTime)to;
            tdate = tdate.AddDays(-1);
            var balance = new Dictionary<string, object>();

            var AccountsAmount = GetChildAccGroup(id, "Capital Account", "liability", null, tdate, 1, 1);
            Amount = AccountsAmount != null ? (decimal)AccountsAmount.Sum(a => a.Credit - a.Debit) : 0;
            if (AccountsAmount != null)
            {
                decimal Credit = (decimal)AccountsAmount.Sum(a => a.Credit);
                decimal Debit = (decimal)AccountsAmount.Sum(a => a.Debit);
                if (Credit >= Debit)
                {
                    Amount = Credit - Debit;
                    balance.Add("type", type);
                    balance.Add("amount", Amount);
                    balance.Add("acctype", actype);
                }
                else
                {
                    Amount = Debit - Credit;
                    type = "Dr";
                    balance.Add("type", type);
                    balance.Add("amount", Amount);
                    balance.Add("acctype", actype);
                }
            }
            return balance;
        }
        public void stockadjtotask(long itemid,decimal req,decimal avail,decimal rate,long unit)
        {
            var stad = new StockAdjustment
            {
                VoucherNo = "to task",
                SANo = 1,
                ItemID =itemid,
                ItemQuantity = req-avail,
                AdjustmentType = AdjustmentType.Add,
                Reason = "-ve stock",
                PurchaseRate =rate,
                ItemUnitID = unit,
                AdjDate = System.DateTime.Now,
                MaterialCenter = 20017,
                CreatedDate = System.DateTime.Now,
                CreatedBy = "",
                Branch = 1,
                Status = Status.active
            };
            db.StockAdjustments.Add(stad);
            db.SaveChanges();
        }
        // FIND OPENING BALANCE for single account or a group of accounts
        // Balance not included to date
        public Dictionary<string, object> OpenBlnc(long id, DateTime to, bool? pdc = null, long[] arry = null)
        {
            string type = "Cr";
            decimal amount = 0;
            var balance = new Dictionary<string, object>();
            var values = (from a in db.AccountsTransactions
                          where EF.Functions.DateDiffDay(a.Date, to) > 0
                          && (a.Account == id || ((id == -1) && arry.Contains(a.Account)))
                          && (pdc == true || a.Status == null)
                          select new
                          {
                              a.Debit,
                              a.Credit,
                              a.Account
                          }).ToList();
            var Debit = values.Sum(a => a.Debit);
            var Credit = values.Sum(a => a.Credit);
            amount = Debit - Credit;
            if (amount > 0)
            {
                type = "Dr.";
            }
            else
            {
                amount = Math.Abs(amount);
            }
            balance.Add("type", type);
            balance.Add("amount", amount);
            return balance;
        }

        #endregion

        #region Bill Adjust Function

        // payment or reciept auto bill adjest function for customer
        public long CusPayment(long AccId, DateTime Date, long BranchID, string UserId)
        {
            clearsepayment();
            var Balance = Accbalance(AccId);
            var amount = Convert.ToDecimal(Balance["amount"]);

            decimal payamount = 0;
            decimal recamount = 0;
            // check is that account is a supplier account and its have pending purchase payment bills
            var customerid = (from a in db.Customers where a.Accounts == AccId select new { a.CustomerID }).SingleOrDefault();
            if (customerid != null)
            {
                if (Balance["type"] == (object)"Dr" || amount == 0)
                {
                    // find balance amount base on sales invoices
                    var custBalance = (from a in db.SalesEntrys
                                       join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                                       //let recbill = db.ReceiptBills.Where(x => x.InvoiceNo == a.SalesEntryId).Select(x => x.Amount).FirstOrDefault()
                                       where a.Customer == customerid.CustomerID && c.SEPaidAmount != c.SEBillAmount
                                       orderby a.SalesEntryId
                                       select new
                                       {
                                           balance = c.SEBillAmount - c.SEPaidAmount //+ (recbill != null ? recbill : 0)),
                                       }).ToList();

                    decimal invoiceBalance = (custBalance.Any()) ? custBalance.Sum(a => a.balance) : 0;
                    payamount = invoiceBalance - amount;
                }
                else
                {

                    var custBalance = (from a in db.SalesReturns
                                       join c in db.SRPayments on a.SalesReturnId equals c.SalesReturnId
                                       where a.Customer == customerid.CustomerID && c.SReturnAmount != c.SRBillAmount
                                       orderby a.SalesReturnId
                                       select new
                                       {
                                           balance = c.SRBillAmount - c.SReturnAmount,
                                       }).ToList();
                    decimal invoiceBalance = (custBalance.Any()) ? custBalance.Sum(a => a.balance) : 0;
                    recamount = invoiceBalance - amount;
                }

                if ((Balance["type"] == (object)"Cr" || payamount > 0) && recamount <= 0)
                {
                    // sales reciept updates
                    var data = (from a in db.SalesEntrys
                                join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                                let recbill = db.ReceiptBills.Where(x => x.InvoiceNo == a.SalesEntryId).Select(x => x.Amount).FirstOrDefault()
                                let ReceiptId = db.ReceiptBills.Where(x => x.InvoiceNo == a.SalesEntryId).Select(x => x.Receipt).FirstOrDefault()
                                where a.Customer == customerid.CustomerID && c.SEPaidAmount != c.SEBillAmount
                                orderby a.SalesEntryId
                                select new
                                {
                                    invoiceno = a.BillNo,
                                    Date = a.SEDate,
                                    total = a.SEGrandTotal,
                                    paid = c.SEPaidAmount,
                                    sid = a.SalesEntryId,
                                    Amount = recbill != null ? recbill : 0,
                                    ReceiptId = ReceiptId
                                }).ToList();
                    if (data.Count > 0)
                    {
                        var paying = (payamount > 0) ? payamount : amount;

                        foreach (var ditem in data)
                        {
                            if (paying > 0)
                            {
                                //if (ditem.Amount > 0)
                                //{
                                //    paying = ditem.Amount;
                                //}

                                SEPayment SEP = db.SEPayments.Where(a => a.SalesEntry == ditem.sid).FirstOrDefault();
                                //add to petransactions
                                SETransaction SEPT = new SETransaction();
                                SEPT.SalesEntry = SEP.SalesEntry;
                                SEPT.CustomerId = Convert.ToInt64(SEP.CustomerId);
                                SEPT.SEPayDate = Convert.ToDateTime(Date);
                                SEPT.SECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                SEPT.CreatedBranch = Convert.ToInt64(BranchID);
                                SEPT.CreatedUserId = UserId;
                                SEPT.Status = 0;
                                SEPT.Recieptid = ditem.ReceiptId;

                                // transaction 
                                var balnceamount = SEP.SEBillAmount - SEP.SEPaidAmount;
                                if (balnceamount >= paying)
                                {
                                    SEP.SEPaidAmount = SEP.SEPaidAmount + Convert.ToDecimal(paying);
                                    SEPT.SEPayAmount = Convert.ToDecimal(paying);
                                    //if (ditem.Amount == 0)
                                    //{
                                    paying = 0;
                                    //}
                                }
                                else
                                {
                                    SEP.SEPaidAmount = SEP.SEPaidAmount + Convert.ToDecimal(balnceamount);
                                    SEPT.SEPayAmount = Convert.ToDecimal(balnceamount);
                                    paying -= balnceamount;
                                }
                                SEP.SECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                if (SEP.SEBillAmount == SEP.SEPaidAmount)
                                {
                                    SEP.Status = 1;
                                }
                                db.Entry(SEP).State = EntityState.Modified;
                                db.SaveChanges();
                                // update transaction
                                db.SETransactions.Add(SEPT);
                                db.SaveChanges();
                            }
                        }

                    }
                    return 1;
                }
                else
                {
                    // sales return Payment updates
                    var data = (from a in db.SalesReturns
                                join c in db.SRPayments on a.SalesReturnId equals c.SalesReturnId
                                where a.Customer == customerid.CustomerID && c.SReturnAmount != c.SRBillAmount
                                orderby a.SalesEntryId
                                select new
                                {
                                    invoiceno = a.BillNo,
                                    Date = a.SRDate,
                                    total = a.SRGrandTotal,
                                    paid = c.SReturnAmount,
                                    sid = a.SalesReturnId
                                }).ToList();
                    if (data.Count > 0)
                    {
                        var paying = (recamount > 0) ? recamount : amount;
                        foreach (var ditem in data)
                        {
                            if (paying > 0)
                            {
                                SRPayment SEP = db.SRPayments.Where(a => a.SalesReturnId == ditem.sid).FirstOrDefault();
                                //add to petransactions
                                SRTransaction SEPT = new SRTransaction();
                                SEPT.SalesReturnId = SEP.SalesReturnId;
                                SEPT.CustomerId = Convert.ToInt64(SEP.CustomerId);
                                SEPT.SRPayDate = Convert.ToDateTime(Date);
                                SEPT.SRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                SEPT.CreatedBranch = Convert.ToInt64(BranchID);
                                SEPT.CreatedUserId = UserId;
                                SEPT.Status = 0;

                                // transaction 
                                var balnceamount = SEP.SRBillAmount - SEP.SReturnAmount;
                                if (balnceamount >= paying)
                                {
                                    SEP.SReturnAmount = SEP.SReturnAmount + Convert.ToDecimal(paying);
                                    SEPT.SRPayAmount = Convert.ToDecimal(paying);
                                    paying = 0;
                                }
                                else
                                {
                                    SEP.SReturnAmount = SEP.SReturnAmount + Convert.ToDecimal(balnceamount);
                                    SEPT.SRPayAmount = Convert.ToDecimal(balnceamount);
                                    paying -= balnceamount;
                                }
                                SEP.SRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                if (SEP.SRBillAmount == SEP.SReturnAmount)
                                {
                                    SEP.Status = 1;
                                }
                                db.Entry(SEP).State = EntityState.Modified;
                                db.SaveChanges();
                                // update transaction
                                db.SRTransactions.Add(SEPT);
                                db.SaveChanges();
                            }
                        }
                    }
                    return 1;
                }
            }
            else
            {
                return 0;
            }
        }
        // payment or reciept auto bill adjest function for customer
        public long SuplPayment(long AccId, DateTime Date, long BranchID, string UserId)
        {
            var Balance = Accbalance(AccId);
            var amount = Convert.ToDecimal(Balance["amount"]);
            decimal payamount = 0;
            decimal recamount = 0;
            // check is that account is a supplier account and its have pending purchase payment bills
            var supplierid = (from a in db.Suppliers where a.Accounts == AccId select new { a.SupplierID }).SingleOrDefault();
            if (supplierid != null)
            {
                if (Balance["type"] == (object)"Cr" || amount == 0)
                {
                    // find balance amount base on purchase invoices
                    var suplierBalance = (from a in db.PurchaseEntrys
                                          join c in db.PEPayments on a.PurchaseEntryId equals c.PurchaseEntry
                                          where a.Supplier == supplierid.SupplierID && c.PEPaidAmount != c.PEBillAmount
                                          orderby a.PurchaseEntryId
                                          select new
                                          {
                                              balance = c.PEBillAmount - c.PEPaidAmount,

                                          }).ToList();
                    decimal invoiceBalance = (suplierBalance.Any()) ? suplierBalance.Sum(a => a.balance) : 0;
                    payamount = invoiceBalance - amount;
                }
                else
                {

                    var suplierBalance = (from a in db.PurchaseReturns
                                          join c in db.PRPayments on a.PurchaseReturnId equals c.PurchaseReturnId
                                          where a.Supplier == supplierid.SupplierID && c.PReturnAmount != c.PRBillAmount
                                          orderby a.PurchaseReturnId
                                          select new
                                          {
                                              balance = c.PRBillAmount - c.PReturnAmount,
                                          }).ToList();
                    decimal invoiceBalance = (suplierBalance.Any()) ? suplierBalance.Sum(a => a.balance) : 0;
                    recamount = invoiceBalance - amount;
                }
                if ((Balance["type"] == (object)"Dr" || payamount > 0) && recamount <= 0)
                {
                    // purchase payment updates
                    var data = (from a in db.PurchaseEntrys
                                join c in db.PEPayments on a.PurchaseEntryId equals c.PurchaseEntry
                                where a.Supplier == supplierid.SupplierID && c.PEPaidAmount != c.PEBillAmount
                                orderby a.PurchaseEntryId
                                select new
                                {
                                    invoiceno = a.BillNo,
                                    Date = a.PEDate,
                                    total = a.PEGrandTotal,
                                    paid = c.PEPaidAmount,
                                    pid = a.PurchaseEntryId
                                }).ToList();
                    if (data.Count > 0)
                    {
                        var paying = (payamount > 0) ? payamount : amount;
                        foreach (var ditem in data)
                        {
                            if (paying > 0)
                            {
                                PEPayment PEP = db.PEPayments.Where(a => a.PurchaseEntry == ditem.pid).FirstOrDefault();
                                //add to petransactions
                                PETransaction PEPT = new PETransaction();
                                PEPT.PurchaseEntry = PEP.PurchaseEntry;
                                PEPT.SupplierId = Convert.ToInt64(PEP.SupplierId);
                                PEPT.PEPayDate = Convert.ToDateTime(Date);
                                PEPT.PECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                PEPT.CreatedBranch = Convert.ToInt64(BranchID);
                                PEPT.CreatedUserId = UserId;
                                PEPT.Status = 0;
                                // transaction 
                                var balnceamount = PEP.PEBillAmount - PEP.PEPaidAmount;
                                if (balnceamount >= paying)
                                {
                                    PEP.PEPaidAmount = PEP.PEPaidAmount + Convert.ToDecimal(paying);
                                    PEPT.PEPayAmount = Convert.ToDecimal(paying);
                                    paying = 0;
                                }
                                else
                                {
                                    PEP.PEPaidAmount = PEP.PEPaidAmount + Convert.ToDecimal(balnceamount);
                                    PEPT.PEPayAmount = Convert.ToDecimal(balnceamount);
                                    paying -= balnceamount;
                                }
                                PEP.PECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                if (PEP.PEBillAmount == PEP.PEPaidAmount)
                                {
                                    PEP.Status = 1;
                                }
                                db.Entry(PEP).State = EntityState.Modified;
                                db.SaveChanges();
                                // update transaction
                                db.PETransactions.Add(PEPT);
                                db.SaveChanges();
                            }
                        }

                    }
                    return 1;
                }
                else
                {
                    // purchase return reciept updates
                    var data = (from a in db.PurchaseReturns
                                join c in db.PRPayments on a.PurchaseReturnId equals c.PurchaseReturnId
                                where a.Supplier == supplierid.SupplierID && c.PReturnAmount != c.PRBillAmount
                                orderby a.PurchaseReturnId
                                select new
                                {
                                    invoiceno = a.BillNo,
                                    Date = a.PRDate,
                                    total = a.PRGrandTotal,
                                    paid = c.PReturnAmount,
                                    pid = a.PurchaseReturnId
                                }).ToList();
                    if (data.Count > 0)
                    {
                        var paying = (recamount > 0) ? recamount : amount;
                        foreach (var ditem in data)
                        {
                            if (paying > 0)
                            {
                                PRPayment PEP = db.PRPayments.Where(a => a.PurchaseReturnId == ditem.pid).FirstOrDefault();
                                //add to petransactions
                                PRTransaction PEPT = new PRTransaction();
                                PEPT.PurchaseReturnId = PEP.PurchaseReturnId;
                                PEPT.SupplierId = Convert.ToInt64(PEP.SupplierId);
                                PEPT.PRPayDate = Convert.ToDateTime(Date);
                                PEPT.PRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                PEPT.CreatedBranch = Convert.ToInt64(BranchID);
                                PEPT.CreatedUserId = UserId;
                                PEPT.Status = 0;
                                // transaction 
                                var balnceamount = PEP.PRBillAmount - PEP.PReturnAmount;
                                if (balnceamount >= paying)
                                {
                                    PEP.PReturnAmount = PEP.PReturnAmount + Convert.ToDecimal(paying);
                                    PEPT.PRPayAmount = Convert.ToDecimal(paying);
                                    paying = 0;
                                }
                                else
                                {
                                    PEP.PReturnAmount = PEP.PReturnAmount + Convert.ToDecimal(balnceamount);
                                    PEPT.PRPayAmount = Convert.ToDecimal(balnceamount);
                                    paying -= balnceamount;
                                }
                                PEP.PRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                if (PEP.PRBillAmount == PEP.PReturnAmount)
                                {
                                    PEP.Status = 1;
                                }
                                db.Entry(PEP).State = EntityState.Modified;
                                db.SaveChanges();
                                // update transaction
                                db.PRTransactions.Add(PEPT);
                                db.SaveChanges();
                            }
                        }

                    }
                    return 1;
                }
            }
            else
            {
                return 0;
            }
        }


        public long RecBillAdjust(long id)
        {
            //receipt
            Receipt Rec = db.Receipts.Find(id);
            var recbill = db.ReceiptBills.Where(a => a.Receipt == Rec.ReceiptId && a.Type != "Against Reference").ToList();
            decimal amt = recbill != null ? recbill.Select(a => a.Amount).Sum() : 0;
            // delete from petransaction and adjust rate of pepayment
            decimal Amtsum1 = 0;
            decimal Amtsum2 = 0;
            // delete from petransaction and adjust rate of pepayment
            //if (amt > 0)
            //{
            var Customerid = (from a in db.Customers where a.Accounts == Rec.PayFrom select new { a.CustomerID }).SingleOrDefault();
            if (Customerid != null)
            {
                var data = (from a in db.SETransactions
                            where a.CustomerId == Customerid.CustomerID && (a.Recieptid == 0 || a.Recieptid == id)
                            orderby a.SETransactionId
                            select new
                            {
                                a.SalesEntry,
                                a.SEPayAmount
                            }).ToList();
                if (data.Count > 0)
                {
                    foreach (var ditem in data)
                    {
                        var paying = ditem.SEPayAmount;
                        if (paying > 0)
                        {

                            SEPayment SEP = db.SEPayments.Where(a => a.SalesEntry == ditem.SalesEntry).FirstOrDefault();
                            SEP.SEPaidAmount = SEP.SEPaidAmount - Convert.ToDecimal(paying);
                            db.Entry(SEP).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        Amtsum1 += ditem.SEPayAmount;

                    }
                    db.SETransactions.RemoveRange(db.SETransactions.Where(a => a.Recieptid == 0 || a.Recieptid == id));
                    db.SaveChanges();
                    Amtsum1 = Amtsum1 + amt;

                }
            }
            // in case of supplier
            var supplierid = (from a in db.Suppliers where a.Accounts == Rec.PayFrom select new { a.SupplierID }).SingleOrDefault();
            if (supplierid != null)
            {
                var data = (from a in db.PRTransactions
                            where a.SupplierId == supplierid.SupplierID && (a.Recieptid == 0 || a.Recieptid == id)
                            orderby a.PRTransactionId
                            select new
                            {
                                a.PurchaseReturnId,
                                a.PRPayAmount
                            }).ToList();
                if (data.Count > 0)
                {
                    foreach (var ditem in data)
                    {
                        var paying = ditem.PRPayAmount;
                        if (paying > 0)
                        {
                            PRPayment SEP = db.PRPayments.Where(a => a.PurchaseReturnId == ditem.PurchaseReturnId).FirstOrDefault();
                            SEP.PReturnAmount = SEP.PReturnAmount - Convert.ToDecimal(paying);
                            db.Entry(SEP).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        Amtsum2 += ditem.PRPayAmount;

                    }
                    db.PRTransactions.RemoveRange(db.PRTransactions.Where(a => a.Recieptid == 0 || a.Recieptid == id));
                    db.SaveChanges();

                    Amtsum2 = Amtsum2 + amt;

                }

            }
            var Check = db.Accountss.Where(x => x.AccountsID == Rec.PayFrom).FirstOrDefault();
            if ((Amtsum1 != 0 && Rec.GrandTotal > Amtsum1) || (Amtsum2 != 0 && Rec.GrandTotal > Amtsum2))
            {
                var call = (Check.Group == 12) ? CusPayment(Check.AccountsID, (DateTime)Rec.Date, Rec.Branch, Rec.CreatedBy) : SuplPayment(Check.AccountsID, (DateTime)Rec.Date, Rec.Branch, Rec.CreatedBy);
            }
            return 1;
            //}
            //else
            //{
            //    return 0;
            //}
        }

        public long PayBillAdjust(long sId)
        {
            Payment Pay = db.Payments.Find(sId);
            decimal Amtsum1 = 0;
            decimal Amtsum2 = 0;
            var paybill = db.PaymentBills.Where(a => a.Payment == Pay.PaymentId && a.Type != "Against Reference").ToList();
            decimal amt = paybill != null ? paybill.Select(a => a.Amount).Sum() : 0;
            //if (amt > 0)
            //{
            var supplierid = (from a in db.Suppliers where a.Accounts == Pay.PayTo select new { a.SupplierID }).SingleOrDefault();
            if (supplierid != null)
            {
                var data = (from a in db.PETransactions
                            where a.SupplierId == supplierid.SupplierID && (a.PaymentId == 0 || a.PaymentId == sId)
                            orderby a.PETransactionId
                            select new
                            {
                                a.PurchaseEntry,
                                a.PEPayAmount
                            }).ToList();
                if (data.Count > 0)
                {
                    foreach (var ditem in data)
                    {
                        var paying = ditem.PEPayAmount;
                        if (paying > 0)
                        {
                            PEPayment PEP = db.PEPayments.Where(a => a.PurchaseEntry == ditem.PurchaseEntry).FirstOrDefault();
                            PEP.PEPaidAmount = PEP.PEPaidAmount - Convert.ToDecimal(paying);
                            db.Entry(PEP).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        Amtsum1 += ditem.PEPayAmount;
                    }
                    db.PETransactions.RemoveRange(db.PETransactions.Where(a => a.PaymentId == 0 || a.PaymentId == sId));
                    db.SaveChanges();
                    Amtsum1 = Amtsum1 + amt;
                }
            }

            var custid = (from a in db.Customers where a.Accounts == Pay.PayTo select new { a.CustomerID }).SingleOrDefault();
            if (custid != null)
            {
                // sales return Payment delete
                var data = (from a in db.SRTransactions
                            where a.CustomerId == custid.CustomerID && (a.PaymentId == 0 || a.PaymentId == sId)
                            orderby a.SRTransactionId
                            select new
                            {
                                a.SalesReturnId,
                                a.SRPayAmount
                            }).ToList();
                if (data.Count > 0)
                {
                    foreach (var ditem in data)
                    {
                        var paying = ditem.SRPayAmount;
                        if (paying > 0)
                        {
                            SRPayment PEP = db.SRPayments.Where(a => a.SalesReturnId == ditem.SalesReturnId).FirstOrDefault();
                            PEP.SReturnAmount = PEP.SReturnAmount - Convert.ToDecimal(paying);
                            db.Entry(PEP).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        Amtsum2 += ditem.SRPayAmount;
                    }
                    db.SRTransactions.RemoveRange(db.SRTransactions.Where(a => a.PaymentId == 0 || a.PaymentId == sId));
                    db.SaveChanges();
                    Amtsum2 = Amtsum2 + amt;
                }

                var AccCheck = db.Accountss.Where(x => x.AccountsID == Pay.PayTo).FirstOrDefault();
                if ((Amtsum1 != 0 && Pay.GrandTotal > Amtsum1) || (Amtsum2 != 0 && Pay.GrandTotal > Amtsum2))
                {
                    var callfn = (AccCheck.Group == 12) ? CusPayment(AccCheck.AccountsID, (DateTime)Pay.Date, Pay.Branch, Pay.CreatedBy) : SuplPayment(AccCheck.AccountsID, (DateTime)Pay.Date, Pay.Branch, Pay.CreatedBy);
                }

            }
            return 1;
            //}
            //else
            //{
            //    return 0;
            //}
        }

        #endregion

        #region payment
        // add payment table data
        public long addPayment(DateTime date, long PayFrom, long PayTo, decimal SubTotal, decimal Paying, decimal GrandTotal, string Remark, string UserId, long Branch, long? Reference = 0, string RefType = "Purchase", decimal TaxPer = 0, decimal TaxAmount = 0, ModeOfPayment MOPayment = ModeOfPayment.Cash, long? tax = null, DateTime? PDCDate = null, choice editble = choice.No, string CheckNo = "", string Bank = "", string pdcNote = "", string VoucherNo = "", ICollection<PaymentBill> bill = null, decimal discount = 0, long? project = null, long? task = null, string Ref1 = "", string Ref2 = "", string Ref3 = "", string Ref4 = "", string Ref5 = "", string InvoiceNo = "", long? PaymentStatus = null, string OverrideStatus = "")
        {
            var today = Convert.ToDateTime(System.DateTime.Now);
            long payNo;
            if (editble == choice.No)
            {
                payNo = 0;
            }
            else
            {
                payNo = payMaxvoucher();
                if (VoucherNo == "")
                {
                    VoucherNo = PayVoucherNo();
                }
            }

            Payment pay = new Payment
            {
                Voucher = payNo,
                VoucherNo = VoucherNo,
                InvoiceNo = InvoiceNo,
                Date = date,
                MOPayment = MOPayment,
                PayFrom = PayFrom,
                PayTo = PayTo,
                Tax = tax,
                TaxPer = TaxPer,
                TaxAmount = TaxAmount,
                Remark = Remark,
                PDCDate = PDCDate,

                Balance = 0,
                SubTotal = SubTotal,
                GrandTotal = GrandTotal,
                Paying = Paying,
                Status = Status.active,
                CreatedBy = UserId,
                CreatedDate = today,
                Branch = Branch,
                editable = editble,
                Reference = Reference,
                RefType = RefType,
                Discount = discount,
                Project = project,
                ProTask = task,
                Ref1 = Ref1,
                Ref2 = Ref2,
                Ref3 = Ref3,
                Ref4 = Ref4,
                Ref5 = Ref5,
                PaymentStatus = PaymentStatus,
                OverrideStatus = OverrideStatus
            };
            db.Payments.Add(pay);
            db.SaveChanges();
            Int64 PaymentId = pay.PaymentId;


            if (bill != null)
            {
                PaymentBill paybill = new PaymentBill();
                foreach (var arr in bill)
                {
                    if (arr.Type == "Against Reference")
                    {
                        decimal payAmt = db.PETransactions.Where(a => a.PurchaseEntry == arr.InvoiceNo && a.PaymentId == 0).FirstOrDefault() != null ? (decimal?)db.PETransactions.Where(a => a.PaymentId == arr.InvoiceNo && a.PaymentId == 0).ToList().Select(a => a.PEPayAmount).Sum() ?? 0 : 0;
                        if (payAmt > 0)
                        {
                            PEPayment PEpay = db.PEPayments.Where(a => a.PurchaseEntry == arr.InvoiceNo).FirstOrDefault();
                            PEpay.PEPaidAmount = PEpay.PEPaidAmount - payAmt;
                            db.Entry(PEpay).State = EntityState.Modified;

                            db.PETransactions.RemoveRange(db.PETransactions.Where(a => a.PaymentId == 0 && a.PurchaseEntry == arr.InvoiceNo));
                            db.SaveChanges();
                        }
                        decimal? payAmtSr = db.SRTransactions.Where(a => a.SalesReturnId == arr.InvoiceNo && a.PaymentId == 0).FirstOrDefault() != null ? (decimal?)db.SRTransactions.Where(a => a.PaymentId == arr.InvoiceNo && a.PaymentId == 0).Select(a => (decimal?)a.SRPayAmount).Sum() ?? 0 : 0;
                        if (payAmtSr > 0)
                        {
                            SRPayment SRpay = db.SRPayments.Where(a => a.SalesReturnId == arr.InvoiceNo).FirstOrDefault();
                            SRpay.SReturnAmount = (decimal)(SRpay.SReturnAmount) - (decimal)(payAmtSr);
                            db.Entry(SRpay).State = EntityState.Modified;

                            db.SRTransactions.RemoveRange(db.SRTransactions.Where(a => a.PaymentId == 0 && a.SalesReturnId == arr.InvoiceNo));
                            db.SaveChanges();
                        }
                    }
                    paybill.InvoiceNo = arr.InvoiceNo;
                    paybill.NewRefName = arr.NewRefName;
                    paybill.Payment = PaymentId;
                    paybill.BillType = arr.BillType;
                    paybill.Amount = arr.Amount;
                    paybill.Type = arr.Type; //arr.Type;
                    paybill.Status = arr.Status;

                    db.PaymentBills.Add(paybill);
                    db.SaveChanges();
                };
            }


            if (MOPayment == ModeOfPayment.PDC || MOPayment == ModeOfPayment.CDC)
            {
                var Bills = "";
                if (bill != null && bill.Where(p => p.Type == "Against Reference").ToList() != null)
                {
                    Bills = String.Join(";", bill.Where(p => p.Type == "Against Reference").Select(p => p.InvoiceNo.ToString()).ToArray());
                }
                PDC pd = new PDC
                {
                    PDCDate = (DateTime)PDCDate,
                    PDCType = "Payment",
                    Reference = PaymentId,
                    CheckNo = CheckNo,
                    Bank = Bank,
                    Note = pdcNote,
                    RegStatus = choice.No,
                    Status = Status.active,
                    CreatedBy = UserId,
                    CreatedDate = today,
                    Branch = Branch,
                    editable = editble,
                    Bills = Bills,
                    Type = (MOPayment == ModeOfPayment.PDC) ? 0 : 1
                };
                db.PDCs.Add(pd);
            }
            db.SaveChanges();

            return PaymentId;
        }

        public long addPayment2(DateTime date, long PayFrom, long PayTo, decimal SubTotal, decimal Paying, decimal GrandTotal, string Remark, string UserId, long Branch, long? Reference = 0, string RefType = "Purchase", decimal TaxPer = 0, decimal TaxAmount = 0, ModeOfPayment MOPayment = ModeOfPayment.Cash, long? tax = null, DateTime? PDCDate = null, choice editble = choice.No, string CheckNo = "", string Bank = "", string pdcNote = "", string VoucherNo = "", ICollection<PaymentBill> bill = null, decimal discount = 0, long? project = null, long? task = null, string Ref1 = "", string Ref2 = "", string Ref3 = "", string Ref4 = "", string Ref5 = "", string InvoiceNo = "", long? PaymentStatus = null, string OverrideStatus = "")
        {
            var today = Convert.ToDateTime(System.DateTime.Now);
            long payNo;
            if (editble == choice.No)
            {
                payNo = 0;
            }
            else
            {
                payNo = payMaxvoucher2();
                if (VoucherNo == "")
                {
                    VoucherNo = PayVoucherNo2();
                }
            }

            DummyPayment pay = new DummyPayment
            {
                Voucher = payNo,
                VoucherNo = VoucherNo,
                InvoiceNo = InvoiceNo,
                Date = date,
                MOPayment = MOPayment,
                PayFrom = PayFrom,
                PayTo = PayTo,
                Tax = tax,
                TaxPer = TaxPer,
                TaxAmount = TaxAmount,
                Remark = Remark,
                PDCDate = PDCDate,

                Balance = 0,
                SubTotal = SubTotal,
                GrandTotal = GrandTotal,
                Paying = Paying,
                Status = Status.active,
                CreatedBy = UserId,
                CreatedDate = today,
                Branch = Branch,
                editable = editble,
                Reference = Reference,
                RefType = RefType,
                Discount = discount,
                Project = project,
                ProTask = task,
                Ref1 = Ref1,
                Ref2 = Ref2,
                Ref3 = Ref3,
                Ref4 = Ref4,
                Ref5 = Ref5,
                PaymentStatus = PaymentStatus,
                OverrideStatus = OverrideStatus,
                CheckNo = CheckNo,
                Bank = Bank,
                PDCNote = pdcNote,
                stat = Status.active,
            };
            db.DummyPayments.Add(pay);
            db.SaveChanges();
            Int64 PaymentId = pay.PaymentId;


            if (bill != null)
            {
                DummyPayBill paybill = new DummyPayBill();
                foreach (var arr in bill)
                {
                    paybill.InvoiceNo = arr.InvoiceNo;
                    paybill.NewRefName = arr.NewRefName;
                    paybill.Payment = PaymentId;
                    paybill.BillType = arr.BillType;
                    paybill.Amount = arr.Amount;
                    paybill.Type = arr.Type; //arr.Type;
                    paybill.Status = arr.Status;

                    db.DummyPayBills.Add(paybill);
                    db.SaveChanges();
                };
            }
            return PaymentId;
        }
        public long addPayment3(DateTime date, long PayFrom, long PayTo, decimal SubTotal, decimal Paying, decimal GrandTotal, string Remark, string UserId, long Branch, long? Reference = 0, string RefType = "Purchase", decimal TaxPer = 0, decimal TaxAmount = 0, ModeOfPayment MOPayment = ModeOfPayment.Cash, long? tax = null, DateTime? PDCDate = null, choice editble = choice.No, string CheckNo = "", string Bank = "", string pdcNote = "", string VoucherNo = "", ICollection<DummyPayBill> bill = null, decimal discount = 0, long? project = null, long? task = null, string Ref1 = "", string Ref2 = "", string Ref3 = "", string Ref4 = "", string Ref5 = "", string InvoiceNo = "", long? PaymentStatus = null, string OverrideStatus = "")
        {

            var today = Convert.ToDateTime(System.DateTime.Now);
            long payNo;
            if (editble == choice.No)
            {
                payNo = 0;
            }
            else
            {
                payNo = payMaxvoucher();
                VoucherNo = PayVoucherNo();

            }

            Payment pay = new Payment
            {
                Voucher = payNo,
                VoucherNo = VoucherNo,
                InvoiceNo = InvoiceNo,
                Date = date,
                MOPayment = MOPayment,
                PayFrom = PayFrom,
                PayTo = PayTo,
                Tax = tax,
                TaxPer = TaxPer,
                TaxAmount = TaxAmount,
                Remark = Remark,
                PDCDate = PDCDate,

                Balance = 0,
                SubTotal = SubTotal,
                GrandTotal = GrandTotal,
                Paying = Paying,
                Status = Status.active,
                CreatedBy = UserId,
                CreatedDate = today,
                Branch = Branch,
                editable = editble,
                Reference = Reference,
                RefType = RefType,
                Discount = discount,
                Project = project,
                ProTask = task,
                Ref1 = Ref1,
                Ref2 = Ref2,
                Ref3 = Ref3,
                Ref4 = Ref4,
                Ref5 = Ref5,
                PaymentStatus = PaymentStatus,
                OverrideStatus = OverrideStatus
            };
            db.Payments.Add(pay);
            db.SaveChanges();
            Int64 PaymentId = pay.PaymentId;


            if (bill != null)
            {
                PaymentBill paybill = new PaymentBill();
                foreach (var arr in bill)
                {
                    if (arr.Type == "Against Reference")
                    {
                        decimal payAmt = db.PETransactions.Where(a => a.PurchaseEntry == arr.InvoiceNo && a.PaymentId == 0).FirstOrDefault() != null ? (decimal?)db.PETransactions.Where(a => a.PaymentId == arr.InvoiceNo && a.PaymentId == 0).ToList().Select(a => a.PEPayAmount).Sum() ?? 0 : 0;
                        if (payAmt > 0)
                        {
                            PEPayment PEpay = db.PEPayments.Where(a => a.PurchaseEntry == arr.InvoiceNo).FirstOrDefault();
                            PEpay.PEPaidAmount = PEpay.PEPaidAmount - payAmt;
                            db.Entry(PEpay).State = EntityState.Modified;

                            db.PETransactions.RemoveRange(db.PETransactions.Where(a => a.PaymentId == 0 && a.PurchaseEntry == arr.InvoiceNo));
                            db.SaveChanges();
                        }
                        decimal? payAmtSr = db.SRTransactions.Where(a => a.SalesReturnId == arr.InvoiceNo && a.PaymentId == 0).FirstOrDefault() != null ? (decimal?)db.SRTransactions.Where(a => a.PaymentId == arr.InvoiceNo && a.PaymentId == 0).Select(a => (decimal?)a.SRPayAmount).Sum() ?? 0 : 0;
                        if (payAmtSr > 0)
                        {
                            SRPayment SRpay = db.SRPayments.Where(a => a.SalesReturnId == arr.InvoiceNo).FirstOrDefault();
                            SRpay.SReturnAmount = (decimal)(SRpay.SReturnAmount) - (decimal)(payAmtSr);
                            db.Entry(SRpay).State = EntityState.Modified;

                            db.SRTransactions.RemoveRange(db.SRTransactions.Where(a => a.PaymentId == 0 && a.SalesReturnId == arr.InvoiceNo));
                            db.SaveChanges();
                        }
                    }
                    paybill.InvoiceNo = arr.InvoiceNo;
                    paybill.NewRefName = arr.NewRefName;
                    paybill.Payment = PaymentId;
                    paybill.BillType = arr.BillType;
                    paybill.Amount = arr.Amount;
                    paybill.Type = arr.Type; //arr.Type;
                    paybill.Status = arr.Status;

                    db.PaymentBills.Add(paybill);
                    db.SaveChanges();
                };
            }


            if (MOPayment == ModeOfPayment.PDC || MOPayment == ModeOfPayment.CDC)
            {
                var Bills = "";
                if (bill != null && bill.Where(p => p.Type == "Against Reference").ToList() != null)
                {
                    Bills = String.Join(";", bill.Where(p => p.Type == "Against Reference").Select(p => p.InvoiceNo.ToString()).ToArray());
                }
                PDC pd = new PDC
                {
                    PDCDate = (DateTime)PDCDate,
                    PDCType = "Payment",
                    Reference = PaymentId,
                    CheckNo = CheckNo,
                    Bank = Bank,
                    Note = pdcNote,
                    RegStatus = choice.No,
                    Status = Status.active,
                    CreatedBy = UserId,
                    CreatedDate = today,
                    Branch = Branch,
                    editable = editble,
                    Bills = Bills,
                    Type = (MOPayment == ModeOfPayment.PDC) ? 0 : 1
                };
                db.PDCs.Add(pd);
            }
            db.SaveChanges();

            return PaymentId;
        }
        public int BillClearPayment2(long PayTo, decimal payAmount, long PaymentId, DateTime Date, long BranchID, String UserId, string acctype, long[] bill, ICollection<DummyPayBill> billset = null)
        {
            if (acctype == "Expense")
            {
            }
            else
            {
                var paying = payAmount;
                bool chk1 = false;
                // check is that account is a supplier account and its have pending purchase payment bills
                var supplierid = (from a in db.Suppliers where a.Accounts == PayTo select new { a.SupplierID }).SingleOrDefault();
                if (supplierid != null)
                {
                    //based on checkbox selection 
                    var v = (from a in db.PurchaseEntrys
                             join c in db.PEPayments on a.PurchaseEntryId equals c.PurchaseEntry
                             //join d in db.PaymentBills on a.PurchaseEntryId equals d.InvoiceNo into rec
                             //from d in rec.DefaultIfEmpty()
                             let paybill = db.PaymentBills.Where(x => x.InvoiceNo == a.PurchaseEntryId).Select(x => x.Amount).Sum()
                             where a.Supplier == supplierid.SupplierID && c.PEPaidAmount != c.PEBillAmount
                             orderby a.PurchaseEntryId
                             select new
                             {
                                 invoiceno = a.BillNo,
                                 Date = a.PEDate,
                                 total = a.PEGrandTotal,
                                 paid = c.PEPaidAmount,
                                 pid = a.PurchaseEntryId,
                                 Amount = paybill != null ? paybill : 0
                             });
                    if (bill != null)
                    {
                        v = v.Where(a => bill.Contains(a.pid));
                        paying = v.Count() > 0 ? v.Select(a => a.Amount).Sum() : payAmount;
                    }
                    if (billset != null)
                    {
                        var newbill = billset.Where(a => a.Type == "Against Reference").Select(a => Convert.ToInt64(a.InvoiceNo)).ToArray();
                        decimal payAmt = (decimal?)billset.Where(a => a.Type != "Against Reference").Select(a => a.Amount).Sum() ?? 0;
                        decimal payAmtAR = (decimal?)billset.Where(a => a.Type == "Against Reference").Select(a => a.Amount).Sum() ?? 0;

                        if (newbill.Length > 0 && (payAmt < payAmtAR))
                        {
                            v = v.Where(a => newbill.Contains(a.pid));
                            chk1 = true;
                        }
                        paying = payAmtAR + payAmt;
                    }
                    var data = v.ToList();
                    if (data.Count > 0)
                    {
                        foreach (var ditem in data)
                        {
                            if (paying > 0)
                            {
                                if (billset != null && chk1 == true)
                                {
                                    paying = billset.Where(a => a.InvoiceNo == ditem.pid).Select(a => a.Amount).FirstOrDefault();
                                }

                                PEPayment PEP = db.PEPayments.Where(a => a.PurchaseEntry == ditem.pid).FirstOrDefault();
                                //add to petransactions
                                PETransaction PEPT = new PETransaction();
                                PEPT.PurchaseEntry = PEP.PurchaseEntry;
                                PEPT.SupplierId = Convert.ToInt64(PEP.SupplierId);
                                PEPT.PEPayDate = Date;
                                PEPT.PECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                PEPT.CreatedBranch = Convert.ToInt64(BranchID);
                                PEPT.CreatedUserId = UserId;
                                PEPT.PaymentId = PaymentId;
                                PEPT.Status = 0;
                                // transaction 
                                var balnceamount = PEP.PEBillAmount - PEP.PEPaidAmount;
                                if (balnceamount >= paying)
                                {
                                    PEP.PEPaidAmount = PEP.PEPaidAmount + Convert.ToDecimal(paying);
                                    PEPT.PEPayAmount = Convert.ToDecimal(paying);
                                    if (chk1 == false)
                                        paying = 0;
                                }
                                else
                                {
                                    PEP.PEPaidAmount = PEP.PEPaidAmount + Convert.ToDecimal(balnceamount);
                                    PEPT.PEPayAmount = Convert.ToDecimal(balnceamount);
                                    paying -= balnceamount;
                                }
                                PEP.PECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                if (PEP.PEBillAmount == PEP.PEPaidAmount)
                                {
                                    PEP.Status = 1;
                                }
                                db.Entry(PEP).State = EntityState.Modified;
                                db.SaveChanges();
                                // update transaction
                                db.PETransactions.Add(PEPT);
                                db.SaveChanges();
                            }
                        }
                    }

                }
                // in case of customer update sales return
                var custid = (from a in db.Customers where a.Accounts == PayTo select new { a.CustomerID }).SingleOrDefault();
                if (custid != null)
                {
                    bool chk2 = false;
                    //based on checkbox selection 
                    // sales return Payment updates
                    var v = (from a in db.SalesReturns
                             join c in db.SRPayments on a.SalesReturnId equals c.SalesReturnId
                             //join d in db.PaymentBills on a.SalesReturnId equals d.InvoiceNo into rec
                             //from d in rec.DefaultIfEmpty()
                             let paybill = db.PaymentBills.Where(x => x.InvoiceNo == a.SalesReturnId).Select(x => x.Amount).Sum()
                             where a.Customer == custid.CustomerID && c.SReturnAmount != c.SRBillAmount
                             orderby a.SalesEntryId
                             select new
                             {
                                 invoiceno = a.BillNo,
                                 Date = a.SRDate,
                                 total = a.SRGrandTotal,
                                 paid = c.SReturnAmount,
                                 sid = a.SalesReturnId,
                                 Amount = paybill != null ? paybill : 0
                             });
                    if (bill != null)
                    {
                        v = v.Where(a => bill.Contains(a.sid));
                        paying = v != null ? v.Select(a => a.Amount).Sum() : payAmount;
                    }
                    if (billset != null)
                    {
                        var newbill = billset.Where(a => a.Type == "Against Reference").Select(a => Convert.ToInt64(a.InvoiceNo)).ToArray();
                        decimal payAmt = (decimal?)billset.Where(a => a.Type != "Against Reference").Select(a => a.Amount).Sum() ?? 0;
                        decimal payAmtAR = (decimal?)billset.Where(a => a.Type == "Against Reference").Select(a => a.Amount).Sum() ?? 0;

                        if (newbill.Length > 0 && (payAmt < payAmtAR))
                        {
                            v = v.Where(a => newbill.Contains(a.sid));
                            chk2 = true;
                        }
                        paying = payAmtAR + payAmt;
                    }
                    var data = v.ToList();
                    if (data.Count > 0)
                    {
                        foreach (var ditem in data)
                        {
                            if (paying > 0)
                            {
                                if (billset != null && chk2 == true)
                                {
                                    paying = billset.Where(a => a.InvoiceNo == ditem.sid).Select(a => a.Amount).FirstOrDefault();
                                }

                                SRPayment SEP = db.SRPayments.Where(a => a.SalesReturnId == ditem.sid).FirstOrDefault();
                                //add to petransactions
                                SRTransaction SEPT = new SRTransaction();
                                SEPT.SalesReturnId = SEP.SalesReturnId;
                                SEPT.CustomerId = Convert.ToInt64(SEP.CustomerId);
                                SEPT.SRPayDate = Date;
                                SEPT.SRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                SEPT.CreatedBranch = Convert.ToInt64(BranchID);
                                SEPT.CreatedUserId = UserId;
                                SEPT.PaymentId = PaymentId;
                                SEPT.Status = 0;

                                // transaction 
                                var balnceamount = SEP.SRBillAmount - SEP.SReturnAmount;
                                if (balnceamount >= paying)
                                {
                                    SEP.SReturnAmount = SEP.SReturnAmount + Convert.ToDecimal(paying);
                                    SEPT.SRPayAmount = Convert.ToDecimal(paying);
                                    if (chk2 == false)
                                        paying = 0;
                                }
                                else
                                {
                                    SEP.SReturnAmount = SEP.SReturnAmount + Convert.ToDecimal(balnceamount);
                                    SEPT.SRPayAmount = Convert.ToDecimal(balnceamount);
                                    paying -= balnceamount;
                                }
                                SEP.SRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                if (SEP.SRBillAmount == SEP.SReturnAmount)
                                {
                                    SEP.Status = 1;
                                }
                                db.Entry(SEP).State = EntityState.Modified;
                                db.SaveChanges();
                                // update transaction
                                db.SRTransactions.Add(SEPT);
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }
            clearpepayment();
            return 1;
        }
        public long payMaxvoucher2()
        {
            Int64 SENo = 0;
            if ((db.DummyPayments.Select(p => p.Voucher).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                SENo = 1;
            }
            else
            {
                SENo = db.DummyPayments.Max(p => p.Voucher + 1);
            }

            return SENo;
        }
        public string PayVoucherNo2(Int64 SENo = 0, string billNo = null)
        {
            var prefix = db.CodePrefixs.Where(a => a.section == "Payment").Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == "Payment").Select(a => a.number).FirstOrDefault();
            if (billNo == null)
            {
                if ((db.DummyPayments.Select(p => p.Voucher).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
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
                    SENo = db.DummyPayments.Max(p => p.Voucher + 1);
                    billNo = prefix + SENo;
                    if (payBillExist(billNo))
                    {
                        billNo = PayVoucherNo2(SENo, billNo);
                    }
                }
            }
            else
            {
                SENo = SENo + 1;
                billNo = prefix + SENo;
                if (payBillExist2(billNo))
                {
                    billNo = PayVoucherNo2(SENo, billNo);
                }

            }
            return billNo;
        }
        public bool payBillExist2(string SENo, long? payid = null)
        {
            bool res;
            if (payid != null)
            {
                var Exists = db.DummyPayments.Any(c => c.VoucherNo == SENo);
                res = (Exists) ? true : false;
            }
            else
            {
                var Exists = db.DummyPayments.Where(a => a.PaymentId != payid).Any(c => c.VoucherNo == SENo);
                res = (Exists) ? true : false;
            }
            return res;
        }
        // get voucher no

        public long payMaxvoucher()
        {
            Int64 SENo = 0;
            if ((db.Payments.Select(p => p.Voucher).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                SENo = 1;
            }
            else
            {
                SENo = db.Payments.Max(p => p.Voucher + 1);
            }

            return SENo;
        }
        public string PayVoucherNo(Int64 SENo = 0, string billNo = null)
        {
            var prefix = db.CodePrefixs.Where(a => a.section == "Payment").Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == "Payment").Select(a => a.number).FirstOrDefault();
            if (billNo == null)
            {
                if ((db.Payments.Select(p => p.Voucher).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
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
                    SENo = db.Payments.Max(p => p.Voucher + 1);
                    billNo = prefix + SENo;
                    if (payBillExist(billNo))
                    {
                        billNo = PayVoucherNo(SENo, billNo);
                    }
                }
            }
            else
            {
                SENo = SENo + 1;
                billNo = prefix + SENo;
                if (payBillExist(billNo))
                {
                    billNo = PayVoucherNo(SENo, billNo);
                }

            }
            return billNo;
        }
        public bool payBillExist(string SENo, long? payid = null)
        {
            bool res;
            if (payid != null)
            {
                var Exists = db.Payments.Any(c => c.VoucherNo == SENo);
                res = (Exists) ? true : false;
            }
            else
            {
                var Exists = db.Payments.Where(a => a.PaymentId != payid).Any(c => c.VoucherNo == SENo);
                res = (Exists) ? true : false;
            }
            return res;
        }
        #endregion

        #region Receipt

        // add receipt payment
        public long addReceipt(DateTime date, long PayFrom, long PayTo, decimal Paying, decimal GrandTotal, string Remark, string UserId, long Branch, long? Reference = 0, string RefType = "Sales", ModeOfPayment MOPayment = ModeOfPayment.Cash, DateTime? PDCDate = null, choice editble = choice.No, string CheckNo = "", string Bank = "", string pdcNote = "", string VoucherNo = "", ICollection<ReceiptBill> bill = null, decimal? discount = 0, long? project = null, long? task = null, string Ref1 = "", string Ref2 = "", string Ref3 = "", string Ref4 = "", string Ref5 = "", long? ReceiptStatus = null, string OverrideStatus = "")
        {
            var today = Convert.ToDateTime(System.DateTime.Now);
            long recNo;
            if (editble == choice.No)
            {
                recNo = 0;
            }
            else
            {
                recNo = recMaxvoucher();
                if (VoucherNo == "")
                {
                    VoucherNo = recVoucherNo();
                }
            }
            Receipt REC = new Receipt
            {
                Voucher = recNo,
                VoucherNo = VoucherNo,
                Date = date,
                MOPayment = MOPayment,
                PayFrom = PayFrom,
                PayTo = PayTo,
                Remark = Remark,
                PDCDate = PDCDate,

                Balance = 0,
                GrandTotal = GrandTotal,
                Paying = Paying,
                Status = Status.active,
                CreatedBy = UserId,
                CreatedDate = today,
                Branch = Branch,
                editable = editble,
                Reference = Reference,
                RefType = RefType,
                Discount = discount,
                Project = project,
                ProTask = task,
                Ref1 = Ref1,
                Ref2 = Ref2,
                Ref3 = Ref3,
                Ref4 = Ref4,
                Ref5 = Ref5,
                ReceiptStatus = ReceiptStatus,
                OverrideStatus = OverrideStatus
            };
            db.Receipts.Add(REC);
            db.SaveChanges();
            Int64 ReceiptId = REC.ReceiptId;

            if (bill != null)
            {
                ReceiptBill recbill = new ReceiptBill();
                foreach (var arr in bill)
                {

                    if (arr.Type == "Against Reference")
                    {
                        decimal payAmt = db.SETransactions.Where(a => a.SalesEntry == arr.InvoiceNo && a.Recieptid == 0).FirstOrDefault() != null ? (decimal?)db.SETransactions.Where(a => a.SalesEntry == arr.InvoiceNo && a.Recieptid == 0).ToList().Select(a => a.SEPayAmount).Sum() ?? 0 : 0;
                        if (payAmt > 0)
                        {
                            SEPayment SEpay = db.SEPayments.Where(a => a.SalesEntry == arr.InvoiceNo).FirstOrDefault();
                            SEpay.SEPaidAmount = SEpay.SEPaidAmount - payAmt;
                            db.Entry(SEpay).State = EntityState.Modified;

                            db.SETransactions.RemoveRange(db.SETransactions.Where(a => a.Recieptid == 0 && a.SalesEntry == arr.InvoiceNo));
                            db.SaveChanges();
                        }
                        decimal payAmtTr = db.PRTransactions.Where(a => a.PurchaseReturnId == arr.InvoiceNo && a.Recieptid == 0).FirstOrDefault() != null ? (decimal?)db.PRTransactions.Where(a => a.PurchaseReturnId == arr.InvoiceNo && a.Recieptid == 0).Select(a => a.PRPayAmount).Sum() ?? 0 : 0;
                        if (payAmtTr > 0)
                        {
                            PRPayment PRpay = db.PRPayments.Where(a => a.PurchaseReturnId == arr.InvoiceNo).FirstOrDefault();
                            PRpay.PReturnAmount = PRpay.PReturnAmount - payAmtTr;
                            db.Entry(PRpay).State = EntityState.Modified;

                            db.PRTransactions.RemoveRange(db.PRTransactions.Where(a => a.Recieptid == 0 && a.PurchaseReturnId == arr.InvoiceNo));
                            db.SaveChanges();
                        }
                    }

                    recbill.InvoiceNo = arr.InvoiceNo;
                    recbill.NewRefName = arr.NewRefName;
                    recbill.Receipt = ReceiptId;
                    recbill.BillType = arr.BillType;
                    recbill.Amount = arr.Amount;
                    recbill.Type = arr.Type; //arr.Type;
                    recbill.Status = arr.Status;

                    db.ReceiptBills.Add(recbill);
                    db.SaveChanges();
                };
            }

            if (MOPayment == ModeOfPayment.PDC || MOPayment == ModeOfPayment.CDC)
            {
                var Bills = "";
                if (bill != null && bill.Where(p => p.Type == "Against Reference").ToList() != null)
                {
                    Bills = String.Join(";", bill.Where(p => p.Type == "Against Reference").Select(p => p.InvoiceNo.ToString()).ToArray());
                }
                PDC pd = new PDC
                {
                    PDCDate = (DateTime)PDCDate,
                    PDCType = "Receipt",
                    Reference = ReceiptId,
                    CheckNo = CheckNo,
                    Bank = Bank,
                    Note = pdcNote,
                    RegStatus = choice.No,
                    Status = Status.active,
                    CreatedBy = UserId,
                    CreatedDate = today,
                    Branch = Branch,
                    editable = editble,
                    Bills = Bills,
                    Type = (MOPayment == ModeOfPayment.PDC) ? 0 : 1,
                };
                db.PDCs.Add(pd);
            }
            db.SaveChanges();

            return ReceiptId;
        }




        public long recMaxvoucher()
        {
            Int64 SENo = 0;
            if ((db.Receipts.Select(p => p.Voucher).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                SENo = 1;
            }
            else
            {
                SENo = db.Receipts.Max(p => p.Voucher + 1);
            }

            return SENo;
        }
        public string recVoucherNo(Int64 SENo = 0, string billNo = null)
        {

            Int32 number = db.CodePrefixs.Where(a => a.section == "Receipt").Select(a => a.number).FirstOrDefault();
            var prefix = db.CodePrefixs.Where(a => a.section == "Receipt").Select(a => a.prefix).FirstOrDefault();
            if (billNo == null)
            {
                if ((db.Receipts.Select(p => p.Voucher).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
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
                    SENo = db.Receipts.Max(p => p.Voucher + 1);
                    billNo = prefix + SENo;
                    if (recBillExist(billNo))
                    {
                        billNo = recVoucherNo(SENo, billNo);
                    }
                }
            }
            else
            {
                SENo = SENo + 1;
                billNo = prefix + SENo;
                if (recBillExist(billNo))
                {
                    billNo = recVoucherNo(SENo, billNo);
                }

            }
            return billNo;
        }
        private bool recBillExist(string SENo)
        {
            var Exists = db.Receipts.Any(c => c.VoucherNo == SENo);
            bool res = (Exists) ? true : false;
            return res;
        }
        public bool recBillExist(string SENo, long? recid = null)
        {
            bool res;
            if (recid != null)
            {
                var Exists = db.Receipts.Any(c => c.VoucherNo == SENo);
                res = (Exists) ? true : false;
            }
            else
            {
                var Exists = db.Receipts.Where(a => a.ReceiptId != recid).Any(c => c.VoucherNo == SENo);
                res = (Exists) ? true : false;
            }
            return res;
        }

        #endregion

        #region number conversion
        //convert number to string
        public string ConvertToWords(string numb)
        {
            String val = "", wholeNo = numb, points = "", andStr = "", pointStr = "";
            String endStr = " Only";
            String money = " Dirhams";
            try
            {
                int decimalPlace = numb.IndexOf(".");
                if (decimalPlace > 0)
                {
                    wholeNo = numb.Substring(0, decimalPlace);
                    points = numb.Substring(decimalPlace + 1);
                    if (Convert.ToInt32(points) > 0)
                    {
                        andStr = " And";// just to separate whole numbers from points/cents  
                        endStr = " Fils " + endStr;//Cents  
                        pointStr = ConvertDecimals(points);
                    }
                }
                var number = ConvertWholeNumber(wholeNo).Trim();
                val = String.Format("{0}{1}{2}{3}{4}", number, money, andStr, pointStr, endStr);
            }
            catch { }
            return val;
        }
        private string ConvertDecimals(String number)
        {
            String cd = "", digit = "", engOne = "";
            for (int i = 0; i < number.Length; i++)
            {
                digit = number[i].ToString();
                if (digit.Equals("0"))
                {
                    engOne = "Zero";
                }
                else
                {
                    engOne = ones(digit);
                }
                cd += " " + engOne;
            }
            return cd;
        }
        private string ConvertWholeNumber(String Number)
        {
            string word = "";
            try
            {
                bool beginsZero = false;//tests for 0XX  
                bool isDone = false;//test if already translated  
                double dblAmt = (Convert.ToDouble(Number));
                //if ((dblAmt > 0) && number.StartsWith("0"))  
                if (dblAmt > 0)
                {//test for zero or digit zero in a nuemric  
                    beginsZero = Number.StartsWith("0");

                    int numDigits = Number.Length;
                    int pos = 0;//store digit grouping  
                    String place = "";//digit grouping name:hundres,thousand,etc...  
                    switch (numDigits)
                    {
                        case 1://ones' range  

                            word = ones(Number);
                            isDone = true;
                            break;
                        case 2://tens' range  
                            word = tens(Number);
                            isDone = true;
                            break;
                        case 3://hundreds' range  
                            pos = (numDigits % 3) + 1;
                            place = " Hundred ";
                            break;
                        case 4://thousands' range  
                        case 5:
                        case 6:
                            pos = (numDigits % 4) + 1;
                            place = " Thousand ";
                            break;
                        case 7://millions' range  
                        case 8:
                        case 9:
                            pos = (numDigits % 7) + 1;
                            place = " Million ";
                            break;
                        case 10://Billions's range  
                        case 11:
                        case 12:

                            pos = (numDigits % 10) + 1;
                            place = " Billion ";
                            break;
                        //add extra case options for anything above Billion...  
                        default:
                            isDone = true;
                            break;
                    }
                    if (!isDone)
                    {//if transalation is not done, continue...(Recursion comes in now!!)  
                        if (Number.Substring(0, pos) != "0" && Number.Substring(pos) != "0")
                        {
                            try
                            {
                                word = ConvertWholeNumber(Number.Substring(0, pos)) + place + ConvertWholeNumber(Number.Substring(pos));
                            }
                            catch { }
                        }
                        else
                        {
                            word = ConvertWholeNumber(Number.Substring(0, pos)) + ConvertWholeNumber(Number.Substring(pos));
                        }

                        //check for trailing zeros  
                        //if (beginsZero) word = " and " + word.Trim();  
                    }
                    //ignore digit grouping names  
                    if (word.Trim().Equals(place.Trim())) word = "";
                }
            }
            catch { }
            return word.Trim();
        }

        private string ones(String Number)
        {
            int _Number = Convert.ToInt32(Number);
            String name = "";
            switch (_Number)
            {

                case 1:
                    name = "One";
                    break;
                case 2:
                    name = "Two";
                    break;
                case 3:
                    name = "Three";
                    break;
                case 4:
                    name = "Four";
                    break;
                case 5:
                    name = "Five";
                    break;
                case 6:
                    name = "Six";
                    break;
                case 7:
                    name = "Seven";
                    break;
                case 8:
                    name = "Eight";
                    break;
                case 9:
                    name = "Nine";
                    break;
            }
            return name;
        }

        private string tens(String Number)
        {
            int _Number = Convert.ToInt32(Number);
            String name = null;
            switch (_Number)
            {
                case 10:
                    name = "Ten";
                    break;
                case 11:
                    name = "Eleven";
                    break;
                case 12:
                    name = "Twelve";
                    break;
                case 13:
                    name = "Thirteen";
                    break;
                case 14:
                    name = "Fourteen";
                    break;
                case 15:
                    name = "Fifteen";
                    break;
                case 16:
                    name = "Sixteen";
                    break;
                case 17:
                    name = "Seventeen";
                    break;
                case 18:
                    name = "Eighteen";
                    break;
                case 19:
                    name = "Nineteen";
                    break;
                case 20:
                    name = "Twenty";
                    break;
                case 30:
                    name = "Thirty";
                    break;
                case 40:
                    name = "Fourty";
                    break;
                case 50:
                    name = "Fifty";
                    break;
                case 60:
                    name = "Sixty";
                    break;
                case 70:
                    name = "Seventy";
                    break;
                case 80:
                    name = "Eighty";
                    break;
                case 90:
                    name = "Ninety";
                    break;
                default:
                    if (_Number > 0)
                    {
                        name = tens(Number.Substring(0, 1) + "0") + " " + ones(Number.Substring(1));
                    }
                    break;
            }
            return name;
        }
        #endregion
        public List<BalanceSheet> GetChildAccGroupNewindirectexpense(long gpId, string group, string type, DateTime? fromdate, DateTime? todate, int fun, int order)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var childid = new SqlParameter("@childid", gpId);
            var GpName = new SqlParameter("@GpName", group);
            var AccType = new SqlParameter("@AccType", type);

            //fromdate = fromdate != null ? fromdate : 0;

            var DateFrom = fromdate != null ? new SqlParameter("@fromdate", fromdate) : null;
            var DateTo = new SqlParameter("@todate", todate);

            var fval = new SqlParameter("@fval", fun);
            var orderB = new SqlParameter("@order", order);

            if (fromdate != null)
            {
                var supgroupsdata = db.Database.SqlQueryRaw<BalanceSheet>("balancesheet @childid,@GpName,@AccType,@fromdate,@todate,@fval,@order", childid, GpName, AccType, DateFrom, DateTo, fval, orderB).ToList();
                var sumdebit = supgroupsdata.Sum(o => o.Debit);
                var sumcredit = supgroupsdata.Sum(o => o.Credit);
                supgroupsdata.Where(o => o.AccountsGroupID == 13).Take(1).ToList().ForEach(o => { o.Debit = sumdebit; o.Credit = sumcredit; });
                var accounts = supgroupsdata.Where(o => o.AccountsGroupID == 13).ToList();
                return accounts;
            }
            else
            {
                var supgroupsdata = db.Database.SqlQueryRaw<BalanceSheet>("balancesheet2 @childid,@GpName,@AccType,@todate,@fval,@order", childid, GpName, AccType, DateTo, fval, orderB).ToList();

                //return accounts;
                var sumdebit = supgroupsdata.Sum(o => o.Debit);
                var sumcredit = supgroupsdata.Sum(o => o.Credit);
                supgroupsdata.Where(o => o.Parent == 0).Take(1).ToList().ForEach(o => { o.Debit = sumdebit; o.Credit = sumcredit; });
                var accounts = supgroupsdata;
                return accounts;
            }
        }

        public static IList<BalanceSheet> GetChildAccGroupNew(long gpId, string group, string type, DateTime? fromdate, DateTime? todate, int fun, int order)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            db.SetCommandTimeOut(60 * 60);
            var childid = new SqlParameter("@childid", gpId);
            var GpName = new SqlParameter("@GpName", group);
            var AccType = new SqlParameter("@AccType", type);

            //fromdate = fromdate != null ? fromdate : 0;

            var DateFrom = fromdate != null ? new SqlParameter("@fromdate", fromdate) : null;
            var DateTo = new SqlParameter("@todate", todate);

            var fval = new SqlParameter("@fval", fun);
            var orderB = new SqlParameter("@order", order);

            if (fromdate != null)
            {
                var supgroupsdata = db.Database.SqlQueryRaw<BalanceSheet>("balancesheet @childid,@GpName,@AccType,@fromdate,@todate,@fval,@order", childid, GpName, AccType, DateFrom, DateTo, fval, orderB).ToList();
                var accounts = supgroupsdata.ToArray();
                return accounts;
            }
            else
            {
                var supgroupsdata = db.Database.SqlQueryRaw<BalanceSheet>("balancesheet2 @childid,@GpName,@AccType,@todate,@fval,@order", childid, GpName, AccType, DateTo, fval, orderB).ToList();

                //return accounts;
                var sumdebit = supgroupsdata.Sum(o => o.Debit);
                var sumcredit = supgroupsdata.Sum(o => o.Credit);
                supgroupsdata.Where(o => o.Parent == 0).Take(1).ToList().ForEach(o => { o.Debit = sumdebit; o.Credit = sumcredit; });
                var accounts = supgroupsdata.ToArray();
                return accounts;
            }
        }
        public static IList<BalanceSheet> GetChildAccGrouptree(long gpId, string group, string type, DateTime? fromdate, DateTime? todate, int fun, int order)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var childid = new SqlParameter("@childid", gpId);
            var GpName = new SqlParameter("@GpName", group);
            var AccType = new SqlParameter("@AccType", type);

            //fromdate = fromdate != null ? fromdate : 0;

            var DateFrom = fromdate != null ? new SqlParameter("@fromdate", fromdate) : null;
            var DateTo = new SqlParameter("@todate", todate);

            var fval = new SqlParameter("@fval", fun);
            var orderB = new SqlParameter("@order", order);

            if (fromdate != null)
            {
                var supgroupsdata = db.Database.SqlQueryRaw<BalanceSheet>("balancesheetnew @childid,@GpName,@AccType,@fromdate,@todate,@fval,@order", childid, GpName, AccType, DateFrom, DateTo, fval, orderB).ToList();
                var accounts = supgroupsdata.ToArray();
                return accounts;
            }
            else
            {
                var supgroupsdata = db.Database.SqlQueryRaw<BalanceSheet>("balancesheet2 @childid,@GpName,@AccType,@todate,@fval,@order", childid, GpName, AccType, DateTo, fval, orderB).ToList();
                var accounts = supgroupsdata.ToArray();
                return accounts;
            }
        }
        public List<BalanceSheet> GetChildAccGroupsummary(long gpId, string group, string type, DateTime? fromdate, DateTime? todate, int fun, int order)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            db.SetCommandTimeOut(60 * 60);
            var childid = new SqlParameter("@childid", gpId);
            var GpName = new SqlParameter("@GpName", group);
            var AccType = new SqlParameter("@AccType", type);

            //fromdate = fromdate != null ? fromdate : 0;

            var DateFrom = fromdate != null ? new SqlParameter("@fromdate", fromdate) : null;
            var DateTo = new SqlParameter("@todate", todate);

            var fval = new SqlParameter("@fval", fun);
            var orderB = new SqlParameter("@order", order);
            
            if (fromdate != null)
            {
                var supgroupsdata = db.Database.SqlQueryRaw<BalanceSheet>("balancesheet @childid,@GpName,@AccType,@fromdate,@todate,@fval,@order", childid, GpName, AccType, DateFrom, DateTo, fval, orderB).ToList();
                var sumdebit = supgroupsdata.Sum(o => o.Debit);
                var sumcredit = supgroupsdata.Sum(o => o.Credit);
                supgroupsdata.Where(o => o.AccountsGroupID == gpId).Take(1).ToList().ForEach(o => { o.Debit = sumdebit; o.Credit = sumcredit; });
                var accounts = supgroupsdata.Where(o => o.AccountsGroupID == gpId);
                return accounts.ToList();

            }
            else
            {
                var supgroupsdata = db.Database.SqlQueryRaw<BalanceSheet>("balancesheet2 @childid,@GpName,@AccType,@todate,@fval,@order", childid, GpName, AccType, DateTo, fval, orderB).ToList();
                var sumdebit = supgroupsdata.Sum(o => o.Debit);
                var sumcredit = supgroupsdata.Sum(o => o.Credit);
                supgroupsdata.Where(o => o.AccountsGroupID == gpId).Take(1).ToList().ForEach(o => { o.Debit = sumdebit; o.Credit = sumcredit; });
                var accounts = supgroupsdata.Where(o => o.AccountsGroupID == gpId);
                return accounts.ToList();
            }
        }

        #region get child acc group
        public static IList<BalanceSheet> GetChildAccGroup(long gpId, string group, string type, DateTime? fromdate, DateTime? todate, int fun, int order)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            db.SetCommandTimeOut(60 * 60);
            var childid = new SqlParameter("@childid", gpId);
            var GpName = new SqlParameter("@GpName", group);
            var AccType = new SqlParameter("@AccType", type);

            //fromdate = fromdate != null ? fromdate : 0;

            var DateFrom = fromdate != null ? new SqlParameter("@fromdate", fromdate) : null;
            var DateTo = new SqlParameter("@todate", todate);

            var fval = new SqlParameter("@fval", fun);
            var orderB = new SqlParameter("@order", order);

            if (fromdate != null)
            {
                var supgroupsdata = db.Database.SqlQueryRaw<BalanceSheet>("balancesheet @childid,@GpName,@AccType,@fromdate,@todate,@fval,@order", childid, GpName, AccType, DateFrom, DateTo, fval, orderB).ToList();
                var accounts = supgroupsdata.ToArray();
                return accounts;
            }
            else
            {
                var supgroupsdata = db.Database.SqlQueryRaw<BalanceSheet>("balancesheet2 @childid,@GpName,@AccType,@todate,@fval,@order", childid, GpName, AccType, DateTo, fval, orderB).ToList();
                var accounts = supgroupsdata.ToArray();
                return accounts;
            }
        }
        public static IList<BalanceSheet> GetChildAccGroupmc(long gpId, string group, string type, DateTime? fromdate, DateTime? todate, int fun, int order, long? mc)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            db.SetCommandTimeOut(60 * 60);
            var childid = new SqlParameter("@childid", gpId);
            var GpName = new SqlParameter("@GpName", group);
            var AccType = new SqlParameter("@AccType", type);
            var ddlmc = new SqlParameter("@mc", mc);
            //fromdate = fromdate != null ? fromdate : 0;

            var DateFrom = fromdate != null ? new SqlParameter("@fromdate", fromdate) : null;
            var DateTo = new SqlParameter("@todate", todate);

            var fval = new SqlParameter("@fval", fun);
            var orderB = new SqlParameter("@order", order);

            if (fromdate != null)
            {
                var supgroupsdata = db.Database.SqlQueryRaw<BalanceSheet>("balancesheet @childid,@GpName,@AccType,@fromdate,@todate,@fval,@order,@mc", childid, GpName, AccType, DateFrom, DateTo, fval, orderB, ddlmc).ToList();
                var accounts = supgroupsdata.ToArray();
                return accounts;
            }
            else
            {
                var supgroupsdata = db.Database.SqlQueryRaw<BalanceSheet>("balancesheet2 @childid,@GpName,@AccType,@todate,@fval,@order,@mc", childid, GpName, AccType, DateTo, fval, orderB, ddlmc).ToList();
                var accounts = supgroupsdata.ToArray();
                return accounts;
            }
        }
        public static IList<BalanceSheet> GetChildAccGroupmctree(long gpId, string group, string type, DateTime? fromdate, DateTime? todate, int fun, int order, long? mc)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var childid = new SqlParameter("@childid", gpId);
            var GpName = new SqlParameter("@GpName", group);
            var AccType = new SqlParameter("@AccType", type);
            var ddlmc = new SqlParameter("@mc", mc);
            //fromdate = fromdate != null ? fromdate : 0;

            var DateFrom = fromdate != null ? new SqlParameter("@fromdate", fromdate) : null;
            var DateTo = new SqlParameter("@todate", todate);

            var fval = new SqlParameter("@fval", fun);
            var orderB = new SqlParameter("@order", order);

            if (fromdate != null)
            {
                var supgroupsdata = db.Database.SqlQueryRaw<BalanceSheet>("balancesheetnew @childid,@GpName,@AccType,@fromdate,@todate,@fval,@order,@mc", childid, GpName, AccType, DateFrom, DateTo, fval, orderB, ddlmc).ToList();
                var accounts = supgroupsdata.ToArray();
                return accounts;
            }
            else
            {
                var supgroupsdata = db.Database.SqlQueryRaw<BalanceSheet>("balancesheet2 @childid,@GpName,@AccType,@todate,@fval,@order,@mc", childid, GpName, AccType, DateTo, fval, orderB, ddlmc).ToList();
                var accounts = supgroupsdata.ToArray();
                return accounts;
            }
        }

        public static IList<BalanceSheet> GetChildAccGroupTrial(int gpId, string group, string type, DateTime? todate, DateTime? fromdate)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            var childid = new SqlParameter("@childid", gpId);
            var GpName = new SqlParameter("@GpName", group);
            var AccType = new SqlParameter("@AccType", type);
            var DateTo = new SqlParameter("@todate", todate);
            var DateFrom = new SqlParameter("@fromdate", fromdate);
            var supgroupsdata = db.Database.SqlQueryRaw<BalanceSheet>("trialbalance @childid,@GpName,@AccType,@todate,@fromdate", childid, GpName, AccType, DateTo, DateFrom).ToList();
            var accounts = supgroupsdata.ToArray();
            return accounts;
        }
        #endregion

        #region Bill Clearence For Payment
        public int BillClearPayment(long PayTo, decimal payAmount, long PaymentId, DateTime Date, long BranchID, String UserId, string acctype, long[] bill, ICollection<PaymentBill> billset = null)
        {
            if (acctype == "Expense")
            {
            }
            else
            {
                var paying = payAmount;
                bool chk1 = false;
                // check is that account is a supplier account and its have pending purchase payment bills
                var supplierid = (from a in db.Suppliers where a.Accounts == PayTo select new { a.SupplierID }).SingleOrDefault();
                if (supplierid != null)
                {
                    //based on checkbox selection 
                    var v = (from a in db.PurchaseEntrys
                             join c in db.PEPayments on a.PurchaseEntryId equals c.PurchaseEntry
                             //join d in db.PaymentBills on a.PurchaseEntryId equals d.InvoiceNo into rec
                             //from d in rec.DefaultIfEmpty()
                             let paybill = db.PaymentBills.Where(x => x.InvoiceNo == a.PurchaseEntryId).Select(x => x.Amount).Sum()
                             where a.Supplier == supplierid.SupplierID && c.PEPaidAmount != c.PEBillAmount
                             orderby a.PurchaseEntryId
                             select new
                             {
                                 invoiceno = a.BillNo,
                                 Date = a.PEDate,
                                 total = a.PEGrandTotal,
                                 paid = c.PEPaidAmount,
                                 pid = a.PurchaseEntryId,
                                 Amount = paybill != null ? paybill : 0
                             });
                    if (bill != null)
                    {
                        v = v.Where(a => bill.Contains(a.pid));
                        paying = v.Count() > 0 ? v.Select(a => a.Amount).Sum() : payAmount;
                    }
                    if (billset != null)
                    {
                        var newbill = billset.Where(a => a.Type == "Against Reference").Select(a => Convert.ToInt64(a.InvoiceNo)).ToArray();
                        decimal payAmt = (decimal?)billset.Where(a => a.Type != "Against Reference").Select(a => a.Amount).Sum() ?? 0;
                        decimal payAmtAR = (decimal?)billset.Where(a => a.Type == "Against Reference").Select(a => a.Amount).Sum() ?? 0;

                        if (newbill.Length > 0 && (payAmt < payAmtAR))
                        {
                            v = v.Where(a => newbill.Contains(a.pid));
                            chk1 = true;
                        }
                        paying = payAmtAR + payAmt;
                    }
                    var data = v.ToList();
                    if (data.Count > 0)
                    {
                        foreach (var ditem in data)
                        {
                            if (paying > 0)
                            {
                                if (billset != null && chk1 == true)
                                {
                                    paying = billset.Where(a => a.InvoiceNo == ditem.pid).Select(a => a.Amount).FirstOrDefault();
                                }

                                PEPayment PEP = db.PEPayments.Where(a => a.PurchaseEntry == ditem.pid).FirstOrDefault();
                                //add to petransactions
                                PETransaction PEPT = new PETransaction();
                                PEPT.PurchaseEntry = PEP.PurchaseEntry;
                                PEPT.SupplierId = Convert.ToInt64(PEP.SupplierId);
                                PEPT.PEPayDate = Date;
                                PEPT.PECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                PEPT.CreatedBranch = Convert.ToInt64(BranchID);
                                PEPT.CreatedUserId = UserId;
                                PEPT.PaymentId = PaymentId;
                                PEPT.Status = 0;
                                // transaction 
                                var balnceamount = PEP.PEBillAmount - PEP.PEPaidAmount;
                                if (balnceamount >= paying)
                                {
                                    PEP.PEPaidAmount = PEP.PEPaidAmount + Convert.ToDecimal(paying);
                                    PEPT.PEPayAmount = Convert.ToDecimal(paying);
                                    if (chk1 == false)
                                        paying = 0;
                                }
                                else
                                {
                                    PEP.PEPaidAmount = PEP.PEPaidAmount + Convert.ToDecimal(balnceamount);
                                    PEPT.PEPayAmount = Convert.ToDecimal(balnceamount);
                                    paying -= balnceamount;
                                }
                                PEP.PECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                if (PEP.PEBillAmount == PEP.PEPaidAmount)
                                {
                                    PEP.Status = 1;
                                }
                                db.Entry(PEP).State = EntityState.Modified;
                                db.SaveChanges();
                                // update transaction
                                db.PETransactions.Add(PEPT);
                                db.SaveChanges();
                            }
                        }
                    }

                }
                // in case of customer update sales return
                var custid = (from a in db.Customers where a.Accounts == PayTo select new { a.CustomerID }).SingleOrDefault();
                if (custid != null)
                {
                    bool chk2 = false;
                    //based on checkbox selection 
                    // sales return Payment updates
                    var v = (from a in db.SalesReturns
                             join c in db.SRPayments on a.SalesReturnId equals c.SalesReturnId
                             //join d in db.PaymentBills on a.SalesReturnId equals d.InvoiceNo into rec
                             //from d in rec.DefaultIfEmpty()
                             let paybill = db.PaymentBills.Where(x => x.InvoiceNo == a.SalesReturnId).Select(x => x.Amount).Sum()
                             where a.Customer == custid.CustomerID && c.SReturnAmount != c.SRBillAmount
                             orderby a.SalesEntryId
                             select new
                             {
                                 invoiceno = a.BillNo,
                                 Date = a.SRDate,
                                 total = a.SRGrandTotal,
                                 paid = c.SReturnAmount,
                                 sid = a.SalesReturnId,
                                 Amount = paybill != null ? paybill : 0
                             });
                    if (bill != null)
                    {
                        v = v.Where(a => bill.Contains(a.sid));
                        paying = v != null ? v.Select(a => a.Amount).Sum() : payAmount;
                    }
                    if (billset != null)
                    {
                        var newbill = billset.Where(a => a.Type == "Against Reference").Select(a => Convert.ToInt64(a.InvoiceNo)).ToArray();
                        decimal payAmt = (decimal?)billset.Where(a => a.Type != "Against Reference").Select(a => a.Amount).Sum() ?? 0;
                        decimal payAmtAR = (decimal?)billset.Where(a => a.Type == "Against Reference").Select(a => a.Amount).Sum() ?? 0;

                        if (newbill.Length > 0 && (payAmt < payAmtAR))
                        {
                            v = v.Where(a => newbill.Contains(a.sid));
                            chk2 = true;
                        }
                        paying = payAmtAR + payAmt;
                    }
                    var data = v.ToList();
                    if (data.Count > 0)
                    {
                        foreach (var ditem in data)
                        {
                            if (paying > 0)
                            {
                                if (billset != null && chk2 == true)
                                {
                                    paying = billset.Where(a => a.InvoiceNo == ditem.sid).Select(a => a.Amount).FirstOrDefault();
                                }

                                SRPayment SEP = db.SRPayments.Where(a => a.SalesReturnId == ditem.sid).FirstOrDefault();
                                //add to petransactions
                                SRTransaction SEPT = new SRTransaction();
                                SEPT.SalesReturnId = SEP.SalesReturnId;
                                SEPT.CustomerId = Convert.ToInt64(SEP.CustomerId);
                                SEPT.SRPayDate = Date;
                                SEPT.SRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                SEPT.CreatedBranch = Convert.ToInt64(BranchID);
                                SEPT.CreatedUserId = UserId;
                                SEPT.PaymentId = PaymentId;
                                SEPT.Status = 0;

                                // transaction 
                                var balnceamount = SEP.SRBillAmount - SEP.SReturnAmount;
                                if (balnceamount >= paying)
                                {
                                    SEP.SReturnAmount = SEP.SReturnAmount + Convert.ToDecimal(paying);
                                    SEPT.SRPayAmount = Convert.ToDecimal(paying);
                                    if (chk2 == false)
                                        paying = 0;
                                }
                                else
                                {
                                    SEP.SReturnAmount = SEP.SReturnAmount + Convert.ToDecimal(balnceamount);
                                    SEPT.SRPayAmount = Convert.ToDecimal(balnceamount);
                                    paying -= balnceamount;
                                }
                                SEP.SRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                if (SEP.SRBillAmount == SEP.SReturnAmount)
                                {
                                    SEP.Status = 1;
                                }
                                db.Entry(SEP).State = EntityState.Modified;
                                db.SaveChanges();
                                // update transaction
                                db.SRTransactions.Add(SEPT);
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }
            clearpepayment();
            return 1;
        }

        public int BillClearPaymentjornal(long PayTo, decimal payAmount, long PaymentId, DateTime Date, long BranchID, String UserId, string acctype, long[] bill, ICollection<PaymentBill> billset = null)
        {
            if (acctype == "Expense")
            {
            }
            else
            {
                var paying = payAmount;
                bool chk1 = false;
                // check is that account is a supplier account and its have pending purchase payment bills
                var supplierid = (from a in db.Suppliers where a.Accounts == PayTo select new { a.SupplierID }).SingleOrDefault();
                if (supplierid != null)
                {
                    //based on checkbox selection 
                    var v = (from a in db.PurchaseEntrys
                             join c in db.PEPayments on a.PurchaseEntryId equals c.PurchaseEntry
                             //join d in db.PaymentBills on a.PurchaseEntryId equals d.InvoiceNo into rec
                             //from d in rec.DefaultIfEmpty()
                             let paybill = db.PaymentBills.Where(x => x.InvoiceNo == a.PurchaseEntryId).Select(x => x.Amount).Sum()
                             where a.Supplier == supplierid.SupplierID && c.PEPaidAmount != c.PEBillAmount
                             orderby a.PurchaseEntryId
                             select new
                             {
                                 invoiceno = a.BillNo,
                                 Date = a.PEDate,
                                 total = a.PEGrandTotal,
                                 paid = c.PEPaidAmount,
                                 pid = a.PurchaseEntryId,
                                 Amount = paybill != null ? paybill : 0
                             });
                    if (bill != null)
                    {
                        v = v.Where(a => bill.Contains(a.pid));
                        paying = v.Count() > 0 ? v.Select(a => a.Amount).Sum() : payAmount;
                    }
                    if (billset != null)
                    {
                        var newbill = billset.Where(a => a.Type == "Against Reference").Select(a => Convert.ToInt64(a.InvoiceNo)).ToArray();
                        decimal payAmt = (decimal?)billset.Where(a => a.Type != "Against Reference").Select(a => a.Amount).Sum() ?? 0;
                        decimal payAmtAR = (decimal?)billset.Where(a => a.Type == "Against Reference").Select(a => a.Amount).Sum() ?? 0;

                        if (newbill.Length > 0 && (payAmt < payAmtAR))
                        {
                            v = v.Where(a => newbill.Contains(a.pid));
                            chk1 = true;
                        }
                        paying = payAmtAR + payAmt;
                    }
                    var data = v.ToList();
                    if (data.Count > 0)
                    {
                        foreach (var ditem in data)
                        {
                            if (paying > 0)
                            {
                                if (billset != null && chk1 == true)
                                {
                                    paying = billset.Where(a => a.InvoiceNo == ditem.pid).Select(a => a.Amount).FirstOrDefault();
                                }

                                PEPayment PEP = db.PEPayments.Where(a => a.PurchaseEntry == ditem.pid).FirstOrDefault();
                                //add to petransactions
                                PETransaction PEPT = new PETransaction();
                                PEPT.PurchaseEntry = PEP.PurchaseEntry;
                                PEPT.SupplierId = Convert.ToInt64(PEP.SupplierId);
                                PEPT.PEPayDate = Date;
                                PEPT.PECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                PEPT.CreatedBranch = Convert.ToInt64(BranchID);
                                PEPT.CreatedUserId = UserId;
                                PEPT.PaymentId = PaymentId;
                                PEPT.Status = 0;
                                // transaction 
                                var balnceamount = PEP.PEBillAmount - PEP.PEPaidAmount;
                                if (balnceamount >= paying)
                                {
                                    PEP.PEPaidAmount = PEP.PEPaidAmount + Convert.ToDecimal(paying);
                                    PEPT.PEPayAmount = Convert.ToDecimal(paying);
                                    if (chk1 == false)
                                        paying = 0;
                                }
                                else
                                {
                                    PEP.PEPaidAmount = PEP.PEPaidAmount + Convert.ToDecimal(balnceamount);
                                    PEPT.PEPayAmount = Convert.ToDecimal(balnceamount);
                                    paying -= balnceamount;
                                }
                                PEP.PECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                if (PEP.PEBillAmount == PEP.PEPaidAmount)
                                {
                                    PEP.Status = 1;
                                }
                                db.Entry(PEP).State = EntityState.Modified;
                                db.SaveChanges();
                                // update transaction
                                db.PETransactions.Add(PEPT);
                                db.SaveChanges();
                            }
                        }
                    }

                }
                // in case of customer update sales return
                var custid = (from a in db.Customers where a.Accounts == PayTo select new { a.CustomerID }).SingleOrDefault();
                if (custid != null)
                {
                    bool chk2 = false;
                    //based on checkbox selection 
                    // sales return Payment updates
                    var v = (from a in db.SalesReturns
                             join c in db.SRPayments on a.SalesReturnId equals c.SalesReturnId
                             //join d in db.PaymentBills on a.SalesReturnId equals d.InvoiceNo into rec
                             //from d in rec.DefaultIfEmpty()
                             let paybill = db.PaymentBills.Where(x => x.InvoiceNo == a.SalesReturnId).Select(x => x.Amount).Sum()
                             where a.Customer == custid.CustomerID && c.SReturnAmount != c.SRBillAmount
                             orderby a.SalesEntryId
                             select new
                             {
                                 invoiceno = a.BillNo,
                                 Date = a.SRDate,
                                 total = a.SRGrandTotal,
                                 paid = c.SReturnAmount,
                                 sid = a.SalesReturnId,
                                 Amount = paybill != null ? paybill : 0
                             });
                    if (bill != null)
                    {
                        v = v.Where(a => bill.Contains(a.sid));
                        paying = v != null ? v.Select(a => a.Amount).Sum() : payAmount;
                    }
                    if (billset != null)
                    {
                        var newbill = billset.Where(a => a.Type == "Against Reference").Select(a => Convert.ToInt64(a.InvoiceNo)).ToArray();
                        decimal payAmt = (decimal?)billset.Where(a => a.Type != "Against Reference").Select(a => a.Amount).Sum() ?? 0;
                        decimal payAmtAR = (decimal?)billset.Where(a => a.Type == "Against Reference").Select(a => a.Amount).Sum() ?? 0;

                        if (newbill.Length > 0 && (payAmt < payAmtAR))
                        {
                            v = v.Where(a => newbill.Contains(a.sid));
                            chk2 = true;
                        }
                        paying = payAmtAR + payAmt;
                    }
                    var data = v.ToList();
                    if (data.Count > 0)
                    {
                        foreach (var ditem in data)
                        {
                            if (paying > 0)
                            {
                                if (billset != null && chk2 == true)
                                {
                                    paying = billset.Where(a => a.InvoiceNo == ditem.sid).Select(a => a.Amount).FirstOrDefault();
                                }

                                SRPayment SEP = db.SRPayments.Where(a => a.SalesReturnId == ditem.sid).FirstOrDefault();
                                //add to petransactions
                                SRTransaction SEPT = new SRTransaction();
                                SEPT.SalesReturnId = SEP.SalesReturnId;
                                SEPT.CustomerId = Convert.ToInt64(SEP.CustomerId);
                                SEPT.SRPayDate = Date;
                                SEPT.SRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                SEPT.CreatedBranch = Convert.ToInt64(BranchID);
                                SEPT.CreatedUserId = UserId;
                                SEPT.PaymentId = PaymentId;
                                SEPT.Status = 0;

                                // transaction 
                                var balnceamount = SEP.SRBillAmount - SEP.SReturnAmount;
                                if (balnceamount >= paying)
                                {
                                    SEP.SReturnAmount = SEP.SReturnAmount + Convert.ToDecimal(paying);
                                    SEPT.SRPayAmount = Convert.ToDecimal(paying);
                                    if (chk2 == false)
                                        paying = 0;
                                }
                                else
                                {
                                    SEP.SReturnAmount = SEP.SReturnAmount + Convert.ToDecimal(balnceamount);
                                    SEPT.SRPayAmount = Convert.ToDecimal(balnceamount);
                                    paying -= balnceamount;
                                }
                                SEP.SRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                                if (SEP.SRBillAmount == SEP.SReturnAmount)
                                {
                                    SEP.Status = 1;
                                }
                                db.Entry(SEP).State = EntityState.Modified;
                                db.SaveChanges();
                                // update transaction
                                db.SRTransactions.Add(SEPT);
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }
            return 1;
        }

        #endregion
        #region Bill Clearence For Reciept
        public int BillClearReciept(long PayFrom, decimal payAmount, long ReceiptId, DateTime Date, long BranchID, String UserId, long[] bill, ICollection<ReceiptBill> billset = null)
        {
            // check is that account is a supplier account and its have pending purchase payment bills
            var customerid = (from a in db.Customers where a.Accounts == PayFrom select new { a.CustomerID }).SingleOrDefault();
            if (customerid != null)
            {
                //decimal payAmt = 0;
                //based on checkbox selection 
                var paying = payAmount;
                bool chk1 = false;
                var v = (from a in db.SalesEntrys
                         join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry

                         let recbill = db.ReceiptBills.Where(x => x.InvoiceNo == a.SalesEntryId).Select(x => x.Amount).Sum()
                         where a.Customer == customerid.CustomerID && c.SEPaidAmount != c.SEBillAmount

                         orderby a.SalesEntryId
                         select new
                         {
                             invoiceno = a.BillNo,
                             Date = a.SEDate,
                             total = a.SEGrandTotal,
                             paid = c.SEPaidAmount,
                             sid = a.SalesEntryId,
                             Amount = recbill != null ? recbill : 0
                         });
                if (bill != null)
                {
                    v = v.Where(a => bill.Contains(a.sid));
                    paying = v.Count() > 0 ? v.Select(a => a.Amount).Sum() : payAmount;
                }
                if (billset != null)
                {
                    var newbill = billset.Where(a => a.Type == "Against Reference").Select(a => Convert.ToInt64(a.InvoiceNo)).ToArray();
                    decimal payAmt = (decimal?)billset.Where(a => a.Type != "Against Reference").Select(a => a.Amount).Sum() ?? 0;
                    decimal payAmtAR = (decimal?)billset.Where(a => a.Type == "Against Reference").Select(a => a.Amount).Sum() ?? 0;

                    if (newbill.Length > 0 && (payAmt < payAmtAR))
                    {
                        v = v.Where(a => newbill.Contains(a.sid));
                        chk1 = true;
                    }
                    paying = payAmtAR + payAmt;
                }
                var data = v.ToList();
                if (data.Count > 0)
                {
                    foreach (var ditem in data)
                    {
                        if (paying > 0)
                        {
                            if (billset != null && chk1 == true)
                            {
                                paying = billset.Where(a => a.InvoiceNo == ditem.sid).Select(a => a.Amount).FirstOrDefault();
                            }

                            SEPayment SEP = db.SEPayments.Where(a => a.SalesEntry == ditem.sid).FirstOrDefault();
                            //add to petransactions
                            SETransaction SEPT = new SETransaction();
                            SEPT.SalesEntry = SEP.SalesEntry;
                            SEPT.CustomerId = Convert.ToInt64(SEP.CustomerId);
                            SEPT.SEPayDate = Date;
                            SEPT.SECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            SEPT.CreatedBranch = Convert.ToInt64(BranchID);
                            SEPT.CreatedUserId = UserId;
                            SEPT.Status = 0;

                            SEPT.Recieptid = ReceiptId;
                            // transaction 
                            var balnceamount = SEP.SEBillAmount - ((SEP.SEPaidAmount < 0) ? 0 : SEP.SEPaidAmount);
                            if (balnceamount >= paying)
                            {
                                SEP.SEPaidAmount = ((SEP.SEPaidAmount < 0) ? 0 : SEP.SEPaidAmount) + Convert.ToDecimal(paying);
                                SEPT.SEPayAmount = Convert.ToDecimal(paying);
                                if (chk1 == false)
                                    paying = 0;
                            }
                            else
                            {
                                SEP.SEPaidAmount = SEP.SEPaidAmount + Convert.ToDecimal(balnceamount);
                                SEPT.SEPayAmount = Convert.ToDecimal(balnceamount);
                                paying -= balnceamount;
                            }
                            SEP.SECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            if (SEP.SEBillAmount == SEP.SEPaidAmount)
                            {
                                SEP.Status = 1;
                            }
                            db.Entry(SEP).State = EntityState.Modified;
                            db.SaveChanges();
                            // update transaction
                            db.SETransactions.Add(SEPT);
                            db.SaveChanges();
                        }
                    }
                }
                clearsepayment();
            }
            // check is that account is a supplier account and its have pending purchase return payment bills
            var supplierid = (from a in db.Suppliers where a.Accounts == PayFrom select new { a.SupplierID }).SingleOrDefault();
            if (supplierid != null)
            {
                //based on checkbox selection 
                var paying = payAmount;
                bool chk2 = false;
                var v = (from a in db.PurchaseReturns
                         join c in db.PRPayments on a.PurchaseReturnId equals c.PurchaseReturnId
                         let recbill = db.ReceiptBills.Where(x => x.InvoiceNo == a.PurchaseReturnId).Select(x => x.Amount).Sum()
                         where a.Supplier == supplierid.SupplierID && c.PReturnAmount != c.PRBillAmount
                         orderby a.PurchaseReturnId
                         select new
                         {
                             invoiceno = a.BillNo,
                             Date = a.PRDate,
                             total = a.PRGrandTotal,
                             paid = c.PReturnAmount,
                             sid = a.PurchaseReturnId,
                             Amount = (recbill != null) ? recbill : 0
                         });
                if (bill != null)
                {
                    v = v.Where(a => bill.Contains(a.sid));
                    paying = v != null ? v.Select(a => a.Amount).Sum() : payAmount;
                }
                if (billset != null)
                {
                    var newbill = billset.Where(a => a.Type == "Against Reference").Select(a => Convert.ToInt64(a.InvoiceNo)).ToArray();
                    decimal payAmt = (decimal?)billset.Where(a => a.Type != "Against Reference").Select(a => a.Amount).Sum() ?? 0;
                    decimal payAmtAR = (decimal?)billset.Where(a => a.Type == "Against Reference").Select(a => a.Amount).Sum() ?? 0;

                    if (newbill.Length > 0 && (payAmt < payAmtAR))
                    {
                        v = v.Where(a => newbill.Contains(a.sid));
                        chk2 = true;
                    }
                    paying = payAmtAR + payAmt;
                }
                var data = v.ToList();
                if (data.Count > 0)
                {
                    foreach (var ditem in data)
                    {
                        if (paying > 0)
                        {

                            if (billset != null && chk2 == true)
                            {
                                paying = billset.Where(a => a.InvoiceNo == ditem.sid).Select(a => a.Amount).FirstOrDefault();
                            }

                            PRPayment SEP = db.PRPayments.Where(a => a.PurchaseReturnId == ditem.sid).FirstOrDefault();
                            //add to petransactions
                            PRTransaction SEPT = new PRTransaction();
                            SEPT.PurchaseReturnId = SEP.PurchaseReturnId;
                            SEPT.SupplierId = Convert.ToInt64(SEP.SupplierId);
                            SEPT.PRPayDate = Date;
                            SEPT.PRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            SEPT.CreatedBranch = Convert.ToInt64(BranchID);
                            SEPT.CreatedUserId = UserId;
                            SEPT.Status = 0;

                            SEPT.Recieptid = ReceiptId;
                            // transaction 
                            var balnceamount = SEP.PRBillAmount - SEP.PReturnAmount;
                            if (balnceamount >= paying)
                            {
                                SEP.PReturnAmount = SEP.PReturnAmount + Convert.ToDecimal(paying);
                                SEPT.PRPayAmount = Convert.ToDecimal(paying);
                                if (chk2 == false)
                                    paying = 0;
                            }
                            else
                            {
                                SEP.PReturnAmount = SEP.PReturnAmount + Convert.ToDecimal(balnceamount);
                                SEPT.PRPayAmount = Convert.ToDecimal(balnceamount);
                                paying -= balnceamount;
                            }
                            SEP.PRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            if (SEP.PRBillAmount == SEP.PReturnAmount)
                            {
                                SEP.Status = 1;
                            }
                            db.Entry(SEP).State = EntityState.Modified;
                            db.SaveChanges();
                            // update transaction
                            db.PRTransactions.Add(SEPT);
                            db.SaveChanges();
                        }
                    }

                }

            }
            return 1;
        }
        public int clearsepayment()
        {
            //return (1);
            (from a in db.SalesEntrys
             join b in db.SEPayments on a.SalesEntryId equals b.SalesEntry
             join c in db.ReceiptBills on a.SalesEntryId equals c.InvoiceNo into cnio
             from c in cnio.DefaultIfEmpty()
             where a.CustomerType == CustomerType.Customer &&
             (b.SEPaidAmount > 0 || b.SEPaidAmount < 0) && c.Receipt == null
             select b

                      ).ToList().ForEach(o => o.SEPaidAmount = 0);

            db.SaveChanges();
            string qry1 = @"update JornalBills set InvoiceNo=q1.SalesEntryId  from JornalBills a join (
select a.SalesEntryId ,b.InvoiceNo,b.NewRefName,a.BillNo   from JornalBills b
join SalesEntries a on a.BillNo =b.NewRefName where a.SENo  =0 and CreatedBy is null and 
a.SalesEntryId !=b.InvoiceNo ) as q1 on q1.NewRefName =a.NewRefName";
            var data1 = db.Database.ExecuteSqlRaw(qry1);
            string qry22 = @"update ReceiptBills set InvoiceNo=b.SalesEntryId 
from ReceiptBills a join
 SalesEntries b on b.BillNo = a.NewRefName
 where b.SENo = 0 and a.InvoiceNo != b.SalesEntryId";
            var data22 = db.Database.ExecuteSqlRaw(qry22);
            string qry2 = @"update AccountsTransactions set Date =a.PDCDate  from AccountsTransactions b 
   join
PDCs a on a.Reference = b.reference
where PDCType = 'Journal' and
 a.RegStatus = 0 and b.Purpose = 'Journal' and a.PDCDate != b.Date ";
            var data2 = db.Database.ExecuteSqlRaw(qry2);
            string qry = @"update SEPayments set SEPaidAmount = ISNULL(q3.recieptamount,0)
from SEPayments  join (
select q1.paidamount,q1.recieptamount,q1.SalesEntryId  from (select max(paidamount) as paidamount,sum(recieptamount) as recieptamount,SalesEntryId from (select max(a.SEPaidAmount) as paidamount, (ISNULL(sum(b.Amount),0)) as  recieptamount,
c.SalesEntryId as SalesEntryId   from SEPayments a
join SalesEntries c on a.SalesEntry = c.SalesEntryId
left join ReceiptBills b on a.SalesEntry = b.InvoiceNo

       
group by c.SalesEntryId
union all
select 0,sum(credit) as  recieptamount,max(reference) as SalesEntryId from
SalesEntries y  join AccountsTransactions x
 on y.SalesEntryId =x.reference 
and Purpose ='Sale Payment' and y.CustomerType =1 group by y.SalesEntryId 
union all
select max(a.SEPaidAmount) as paidamount, (ISNULL(sum(d.Amount),0)) as  recieptamount,
c.SalesEntryId as SalesEntryId  from SEPayments a
join SalesEntries c on a.SalesEntry = c.SalesEntryId

left join JornalBills d on a.SalesEntry =d.InvoiceNo
       
group by c.SalesEntryId) q2 group by q2.SalesEntryId ) as q1) as q3

on SEPayments.SalesEntry = q3.SalesEntryId";
            var data = db.Database.ExecuteSqlRaw(qry);

            //             qry = @"update SEPayments set SEPaidAmount = q1.recieptamount
            //from SEPayments join (select max(a.SEPaidAmount) as paidamount, sum(b.Amount) recieptamount,
            //b.InvoiceNo from SEPayments a
            //join SalesEntries c on a.SalesEntry = c.SalesEntryId
            //join JornalBills b on a.SalesEntry = b.InvoiceNo
            //        where  CustomerType = 0
            //group by b.InvoiceNo
            //        having   max(a.SEPaidAmount) != sum(b.Amount)) as q1 on SEPayments.SalesEntry = q1.InvoiceNo";
            //             data = db.Database.ExecuteSqlRaw(qry);
            return (1);

        }
        public int clearpepayment()
        {
            // return (1);
            (from a in db.PurchaseEntrys
             join b in db.PEPayments on a.PurchaseEntryId equals b.PurchaseEntry
             join c in db.PaymentBills on a.PurchaseEntryId equals c.InvoiceNo into cnio
             from c in cnio.DefaultIfEmpty()
             where a.SupplierType == SupplierType.CreditSale &&
             b.PEPaidAmount > 0 && c.Payment == null
             select b

                      ).ToList().ForEach(o => o.PEPaidAmount = 0);
            db.SaveChanges();
            string qry = @"update PEPayments set PEPaidAmount = ISNULL(q3.recieptamount,0)
from  PEPayments join (
select q1.paidamount,q1.recieptamount,q1.PurchaseEntryId  from (select max(paidamount) as paidamount,sum(recieptamount) as recieptamount,PurchaseEntryId from
(select max(a.PEPaidAmount) as paidamount, (ISNULL(sum(b.Amount),0)) as  recieptamount,
c.PurchaseEntryId as PurchaseEntryId   from PEPayments a
join PurchaseEntries c on a.PurchaseEntry = c.PurchaseEntryId
left join PaymentBills b on a.PurchaseEntry = b.InvoiceNo

        where  SupplierType = 0  and billtype!='Sales Return'
group by c.PurchaseEntryId 
union all
select max(a.PEPaidAmount) as paidamount, (ISNULL(sum(d.Amount),0)) as  recieptamount,
c.PurchaseEntryId as PurchaseEntryId  from PEPayments a
join PurchaseEntries c on a.PurchaseEntry = c.PurchaseEntryId

left join JornalPaymentBills d on a.PurchaseEntry =d.InvoiceNo
        where  SupplierType = 0 
group by c.PurchaseEntryId

	union all
select max(a.PEPaidAmount) as paidamount, (ISNULL(sum(d.PRGrandTotal), 0)) as recieptamount,
c.PurchaseEntryId as PurchaseEntryId  from PEPayments a
join PurchaseEntries c on a.PurchaseEntry = c.PurchaseEntryId

left
                              join PurchaseReturns d on a.PurchaseEntry = d.purchaseEntryId  where d.SupplierType=1

                              group by c.PurchaseEntryId	


) q2 group by q2.PurchaseEntryId ) as q1) as q3

on PEPayments.PurchaseEntry = q3.PurchaseEntryId";
            var data = db.Database.ExecuteSqlRaw(qry);
            return (1);

        }

        public int BillClearJornal(long PayFrom, decimal payAmount, long ReceiptId, DateTime Date, long BranchID, String UserId, long[] bill, ICollection<ReceiptBill> billset = null)
        {
            // check is that account is a supplier account and its have pending purchase payment bills
            var customerid = (from a in db.Customers where a.Accounts == PayFrom select new { a.CustomerID }).SingleOrDefault();
            if (customerid != null)
            {
                //decimal payAmt = 0;
                //based on checkbox selection 
                var paying = payAmount;
                bool chk1 = false;
                var v = (from a in db.SalesEntrys
                         join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry

                         let recbill = db.JornaltBills.Where(x => x.InvoiceNo == a.SalesEntryId).Select(x => x.Amount).Sum()
                         where a.Customer == customerid.CustomerID && c.SEPaidAmount != c.SEBillAmount

                         orderby a.SalesEntryId
                         select new
                         {
                             invoiceno = a.BillNo,
                             Date = a.SEDate,
                             total = a.SEGrandTotal,
                             paid = c.SEPaidAmount,
                             sid = a.SalesEntryId,
                             Amount = recbill != null ? recbill : 0
                         });
                if (bill != null)
                {
                    v = v.Where(a => bill.Contains(a.sid));
                    paying = v.Count() > 0 ? v.Select(a => a.Amount).Sum() : payAmount;
                }
                if (billset != null)
                {
                    var newbill = billset.Where(a => a.Type == "Against Reference").Select(a => Convert.ToInt64(a.InvoiceNo)).ToArray();
                    decimal payAmt = (decimal?)billset.Where(a => a.Type != "Against Reference").Select(a => a.Amount).Sum() ?? 0;
                    decimal payAmtAR = (decimal?)billset.Where(a => a.Type == "Against Reference").Select(a => a.Amount).Sum() ?? 0;

                    if (newbill.Length > 0 && (payAmt < payAmtAR))
                    {
                        v = v.Where(a => newbill.Contains(a.sid));
                        chk1 = true;
                    }
                    paying = payAmtAR + payAmt;
                }
                var data = v.ToList();
                if (data.Count > 0)
                {
                    foreach (var ditem in data)
                    {
                        if (paying > 0)
                        {
                            if (billset != null && chk1 == true)
                            {
                                paying = billset.Where(a => a.InvoiceNo == ditem.sid).Select(a => a.Amount).FirstOrDefault();
                            }

                            SEPayment SEP = db.SEPayments.Where(a => a.SalesEntry == ditem.sid).FirstOrDefault();
                            //add to petransactions
                            SETransaction SEPT = new SETransaction();
                            SEPT.SalesEntry = SEP.SalesEntry;
                            SEPT.CustomerId = Convert.ToInt64(SEP.CustomerId);
                            SEPT.SEPayDate = Date;
                            SEPT.SECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            SEPT.CreatedBranch = Convert.ToInt64(BranchID);
                            SEPT.CreatedUserId = UserId;
                            SEPT.Status = 0;

                            SEPT.Recieptid = ReceiptId;
                            // transaction 
                            var balnceamount = SEP.SEBillAmount - SEP.SEPaidAmount;
                            if (balnceamount >= paying)
                            {
                                SEP.SEPaidAmount = SEP.SEPaidAmount + Convert.ToDecimal(paying);
                                SEPT.SEPayAmount = Convert.ToDecimal(paying);
                                if (chk1 == false)
                                    paying = 0;
                            }
                            else
                            {
                                SEP.SEPaidAmount = SEP.SEPaidAmount + Convert.ToDecimal(balnceamount);
                                SEPT.SEPayAmount = Convert.ToDecimal(balnceamount);
                                paying -= balnceamount;
                            }
                            SEP.SECreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            if (SEP.SEBillAmount == SEP.SEPaidAmount)
                            {
                                SEP.Status = 1;
                            }
                            db.Entry(SEP).State = EntityState.Modified;
                            db.SaveChanges();
                            // update transaction
                            db.SETransactions.Add(SEPT);
                            db.SaveChanges();
                        }
                    }
                }

            }
            // check is that account is a supplier account and its have pending purchase return payment bills
            var supplierid = (from a in db.Suppliers where a.Accounts == PayFrom select new { a.SupplierID }).SingleOrDefault();
            if (supplierid != null)
            {
                //based on checkbox selection 
                var paying = payAmount;
                bool chk2 = false;
                var v = (from a in db.PurchaseReturns
                         join c in db.PRPayments on a.PurchaseReturnId equals c.PurchaseReturnId
                         let recbill = db.ReceiptBills.Where(x => x.InvoiceNo == a.PurchaseReturnId).Select(x => x.Amount).Sum()
                         where a.Supplier == supplierid.SupplierID && c.PReturnAmount != c.PRBillAmount
                         orderby a.PurchaseReturnId
                         select new
                         {
                             invoiceno = a.BillNo,
                             Date = a.PRDate,
                             total = a.PRGrandTotal,
                             paid = c.PReturnAmount,
                             sid = a.PurchaseReturnId,
                             Amount = (recbill != null) ? recbill : 0
                         });
                if (bill != null)
                {
                    v = v.Where(a => bill.Contains(a.sid));
                    paying = v != null ? v.Select(a => a.Amount).Sum() : payAmount;
                }
                if (billset != null)
                {
                    var newbill = billset.Where(a => a.Type == "Against Reference").Select(a => Convert.ToInt64(a.InvoiceNo)).ToArray();
                    decimal payAmt = (decimal?)billset.Where(a => a.Type != "Against Reference").Select(a => a.Amount).Sum() ?? 0;
                    decimal payAmtAR = (decimal?)billset.Where(a => a.Type == "Against Reference").Select(a => a.Amount).Sum() ?? 0;

                    if (newbill.Length > 0 && (payAmt < payAmtAR))
                    {
                        v = v.Where(a => newbill.Contains(a.sid));
                        chk2 = true;
                    }
                    paying = payAmtAR + payAmt;
                }
                var data = v.ToList();
                if (data.Count > 0)
                {
                    foreach (var ditem in data)
                    {
                        if (paying > 0)
                        {

                            if (billset != null && chk2 == true)
                            {
                                paying = billset.Where(a => a.InvoiceNo == ditem.sid).Select(a => a.Amount).FirstOrDefault();
                            }

                            PRPayment SEP = db.PRPayments.Where(a => a.PurchaseReturnId == ditem.sid).FirstOrDefault();
                            //add to petransactions
                            PRTransaction SEPT = new PRTransaction();
                            SEPT.PurchaseReturnId = SEP.PurchaseReturnId;
                            SEPT.SupplierId = Convert.ToInt64(SEP.SupplierId);
                            SEPT.PRPayDate = Date;
                            SEPT.PRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            SEPT.CreatedBranch = Convert.ToInt64(BranchID);
                            SEPT.CreatedUserId = UserId;
                            SEPT.Status = 0;

                            SEPT.Recieptid = ReceiptId;
                            // transaction 
                            var balnceamount = SEP.PRBillAmount - SEP.PReturnAmount;
                            if (balnceamount >= paying)
                            {
                                SEP.PReturnAmount = SEP.PReturnAmount + Convert.ToDecimal(paying);
                                SEPT.PRPayAmount = Convert.ToDecimal(paying);
                                if (chk2 == false)
                                    paying = 0;
                            }
                            else
                            {
                                SEP.PReturnAmount = SEP.PReturnAmount + Convert.ToDecimal(balnceamount);
                                SEPT.PRPayAmount = Convert.ToDecimal(balnceamount);
                                paying -= balnceamount;
                            }
                            SEP.PRCreatedDate = Convert.ToDateTime(System.DateTime.Now);
                            if (SEP.PRBillAmount == SEP.PReturnAmount)
                            {
                                SEP.Status = 1;
                            }
                            db.Entry(SEP).State = EntityState.Modified;
                            db.SaveChanges();
                            // update transaction
                            db.PRTransactions.Add(SEPT);
                            db.SaveChanges();
                        }
                    }

                }

            }
            return 1;
        }
        #endregion

        #region ItemCreate
        public Int64 Item(ItemViewModel ProdViewModel)
        {
            // var it = new ItemController();
            var UserId = LegacyWeb.Current.User.Identity.GetUserId();
            long Branch = 0;
            long? secUnit = 0;
            if (ProdViewModel.ShowSecUnits == true)
            {
                secUnit = ProdViewModel.SubUnitId;
            }
            else
            {
                secUnit = ProdViewModel.ItemUnitID;
            }
            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            decimal stkvalue = (ProdViewModel.StockValue != null && ProdViewModel.StockValue != 0 && ProdViewModel.OpeningStock != null && ProdViewModel.OpeningStock != 0) ? (decimal)(ProdViewModel.StockValue / ProdViewModel.OpeningStock) : 0; // Calc fix: also guard OpeningStock divisor (prevents DivideByZeroException on item save).
            if (BranchCheck == Status.active)
            {
                Branch = ProdViewModel.Branch;
            }
            else
            {
                Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
            }
            var items = new Item
            {
                ItemCode = ProdViewModel.ItemCode,
                ItemName = ProdViewModel.ItemName,
                ItemArabic = ProdViewModel.ItemArabic,
                ItemDescription = ProdViewModel.ItemDescription,
                SellingPrice = ProdViewModel.SellingPrice,
                accountid = ProdViewModel.accountid,
                PurchasePrice = ProdViewModel.PurchasePrice,
                MRP = ProdViewModel.MRP,
                BasePrice = ProdViewModel.BasePrice,
                slreq = ProdViewModel.slreq,
                accmap = ProdViewModel.accmap,
                lockprice = ProdViewModel.lockprice,

                KeepStock = ProdViewModel.KeepStock,
                ItemCategoryID = ProdViewModel.ItemCategoryID,
                ItemBrandID = ProdViewModel.ItemBrandID,
                PartNumber = ProdViewModel.PartNumber,
                cashprice = (ProdViewModel.cashprice == null) ? ProdViewModel.SellingPrice : ProdViewModel.cashprice,
                creditprice = (ProdViewModel.creditprice == null) ? ProdViewModel.SellingPrice : ProdViewModel.creditprice,

                ItemColorID = ProdViewModel.ItemColorID,
                ItemSizeID = ProdViewModel.ItemSizeID,
                TaxID = ProdViewModel.TaxID,
                CreatedBy = 1,
                Status = ProdViewModel.Status,
                Branch = Branch,
                //------------------------------------
                ItemUnitID = ProdViewModel.ItemUnitID,
                SubUnitId = secUnit,
                ConFactor = ProdViewModel.ConFactor,

                OpeningStock = ProdViewModel.OpeningStock,
                MinStock = ProdViewModel.MinStock,
                Commission = ProdViewModel.Commission == null ? 0 : ProdViewModel.Commission,
                //------------------------------------
                ItemType = ProdViewModel.ItemType,
                CreatedUserID = UserId,
                Barcode = ProdViewModel.Barcode,
                Supplier = ProdViewModel.Supplier,
                SupplierRef = ProdViewModel.SupplierRef,
                ConRate = ProdViewModel.ConRate,
                Currency = ProdViewModel.Currency,
                Prefix = ProdViewModel.Prefix,
                StockValue = ProdViewModel.StockValue,
                OpeningCost = stkvalue,
                InSaleInvoice = ProdViewModel.InSaleInvoice,
                daysexpirty = ProdViewModel.daysexpirty,

            };
            db.Items.Add(items);
            db.SaveChanges();
            Int64 ItemID = items.ItemID;
            if (ProdViewModel.ItemType == 2 || ProdViewModel.ItemType == 3 || ProdViewModel.ItemType == 4)
            {
                var Jewellerys = new Jewellery
                {
                    Item = ItemID,
                    Country = ProdViewModel.Country,
                    Style = ProdViewModel.Style,
                    Type = ProdViewModel.Type,
                    SetRef = ProdViewModel.SetRef,
                    TagLine1 = ProdViewModel.Tagline1,
                    TagLine2 = ProdViewModel.Tagline2,
                    TagLine3 = ProdViewModel.Tagline3,
                    TagLine4 = ProdViewModel.Tagline4,
                    TagLine5 = ProdViewModel.Tagline5,
                };
                db.Jewellerys.Add(Jewellerys);
                db.SaveChanges();
                if (ProdViewModel.ItemType == 2)
                {
                    var Diamonds = new Diamond
                    {
                        Item = ItemID,
                        Design = ProdViewModel.Design,
                        Clarify = ProdViewModel.Clarify,
                        Fluorescence = ProdViewModel.Fluorescence,
                        Range = ProdViewModel.Range,
                        CertificateNo = ProdViewModel.CertificateNo,
                        Time = ProdViewModel.Time,
                    };
                    db.Diamonds.Add(Diamonds);
                    db.SaveChanges();
                }
                else if (ProdViewModel.ItemType == 3)
                {
                    var watchs = new Watch
                    {
                        Item = ItemID,
                        Refno = ProdViewModel.Refno,
                        Warranty = ProdViewModel.Warranty,
                        ModelNo = ProdViewModel.ModelNo,
                        ModelName = ProdViewModel.ModelName,
                        Straptype = ProdViewModel.Straptype,
                        DialShape = ProdViewModel.DialShape,
                        DialColor = ProdViewModel.DialColor,
                        Material = ProdViewModel.Material,
                        Movement = ProdViewModel.Movement,
                        Weight = ProdViewModel.Weight,
                        StoneType = ProdViewModel.StoneType,

                    };
                    db.Watchs.Add(watchs);
                    db.SaveChanges();
                }
            }
            if (ProdViewModel.ItemType == 5)//scafold
            {
                var scafold = new Scaffold
                {
                    CBM = ProdViewModel.CBM,
                    Weight = ProdViewModel.SCWeight,
                    Item = ItemID
                };
                db.Scaffolds.Add(scafold);
                db.SaveChanges();
            }

            if (ProdViewModel.Barcode != null)
            {
                var brcode = new Barcode
                {
                    BarcodeNumber = ProdViewModel.Barcode,
                    ItemID = ItemID
                };
                db.Barcodes.Add(brcode);
                db.SaveChanges();
            }
            if (ProdViewModel.Prefix != null)
            {
                var lastNo = (ProdViewModel.ItemCode.Length >= 5) ? ProdViewModel.ItemCode.Substring(ProdViewModel.ItemCode.Length - 5) : "0";
                Int64 No = (lastNo != "0") ? Convert.ToInt64(lastNo) : GetprefixNo((long)ProdViewModel.Prefix);
                var Pre = new ItemPrefix
                {
                    Prefix = (long)ProdViewModel.Prefix,
                    No = No
                    //No =  it.GetprefixNo((long)ProdViewModel.Prefix)
                };
                db.ItemPrefixs.Add(Pre);
                db.SaveChanges();
            }
            //stock add
            //decimal OpSt = Convert.ToDecimal(ProdViewModel.OpeningStock);
            //decimal Stockvalue = Convert.ToDecimal(ProdViewModel.PurchasePrice )*OpSt ;
            //addStock(OpSt, 0,ItemID, ProdViewModel.ItemUnitID, "OpeningStock", ItemID, ProdViewModel.PurchasePrice, Stockvalue,DateTime.Now,null, Status.active);

            // batch stock
            if (ProdViewModel.bstmodel != null && ProdViewModel.OpeningStock > 0 && ProdViewModel.KeepStock == true)
            {
                foreach (var bst in ProdViewModel.bstmodel)
                {
                    decimal totBtch = 0;
                    decimal BOStock = (totBtch <= ProdViewModel.OpeningStock) ? bst.StockIn : (decimal)(ProdViewModel.OpeningStock - bst.StockIn);
                    decimal bStock = BOStock * ProdViewModel.ConFactor;
                    if (bst.BatchNo != "" && bst.BatchNo != null && bStock > 0)
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
                        BatchStock Btst = new BatchStock();
                        Btst.BatchNo = bst.BatchNo;
                        Btst.EXP = exp;
                        Btst.MFG = mfg;
                        Btst.StockIn = bStock;
                        Btst.StockOut = 0;

                        Btst.Item = ItemID;
                        Btst.Unit = ProdViewModel.ItemUnitID;
                        Btst.Cost = ProdViewModel.PurchasePrice;
                        Btst.Order = 1;
                        Btst.Reference = ItemID;
                        Btst.Type = "Opening";

                        Btst.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                        Btst.Date = Convert.ToDateTime(System.DateTime.Now);


                        db.BatchStocks.Add(Btst);
                    }
                }
                db.SaveChanges();
            }

            return ItemID;
        }

        public bool Images(ItemViewModel ProdViewModel, long ItemID)
        {
            if (ProdViewModel.ItemImage != null)
            {
                var ItemImg = db.ItemImages.Where(a => a.ItemID == ItemID).FirstOrDefault();
                if (ItemImg != null)
                {
                    db.ItemImages.RemoveRange(db.ItemImages.Where(a => a.ItemID == ItemID));
                    string storePath = LegacyWeb.MapPath("~/uploads/itemimages/" + ItemID);
                    if (Directory.Exists(storePath))
                        try
                        {
                            Directory.Delete(storePath, true);
                        }
                        catch (Exception e)
                        {

                        }
                }
            }

            int flag = 0;
            foreach (IFormFile file in ProdViewModel.ItemImage)
            {
                //Checking file is available to save.  
                if (file != null)
                {
                    var ProdImg = new ItemImage
                    {
                        ItemID = ItemID,
                        FileName = Path.GetFileName(file.FileName),
                        Status = 1
                    };
                    db.ItemImages.Add(ProdImg);
                    db.SaveChanges();
                    string storePath = LegacyWeb.MapPath("~/uploads/itemimages/" + ItemID);
                    if (!Directory.Exists(storePath))
                        Directory.CreateDirectory(storePath);
                    var InputFileName = Path.GetFileName(file.FileName);
                    var ServerSavePath = Path.Combine(storePath + "/" + InputFileName);
                    //Save file to server folder  
                    file.SaveAs(ServerSavePath);
                    flag = 1;


                    String noextension = Path.GetFileNameWithoutExtension(InputFileName);
                    String extension = Path.GetExtension(InputFileName);
                    //string date = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                    String newName = noextension + extension;
                    var thumbName = "";
                    if (extension == ".jpg" || extension == ".png" || extension == ".jpeg")
                    {
                        thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                        thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/itemimages/" + ItemID), thumbName);

                        Image img = Image.FromFile(ServerSavePath);
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
                    }

                }
            }

            if (flag == 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }
        public bool Document(ItemViewModel ProdViewModel, long ItemID)
        {
            var ItemDoc = db.ItemDocuments.Where(a => a.ItemID == ItemID).FirstOrDefault();
            if (ItemDoc != null)
            {
                db.ItemDocuments.RemoveRange(db.ItemDocuments.Where(a => a.ItemID == ItemID));
                string storePath = LegacyWeb.MapPath("~/uploads/itemdocuments/" + ItemID);
                if (Directory.Exists(storePath))
                    try
                    {
                        Directory.Delete(storePath, true);
                    }
                    catch (Exception e)
                    {

                    }
            }

            int flag = 0;
            foreach (IFormFile file in ProdViewModel.ItemDocument)
            {
                //Checking file is available to save.  
                if (file != null)
                {
                    var ProdDocument = new ItemDocument
                    {
                        ItemID = ItemID,
                        FileName = Path.GetFileName(file.FileName),
                        Status = 1
                    };
                    db.ItemDocuments.Add(ProdDocument);
                    db.SaveChanges();

                    string storePath = LegacyWeb.MapPath("~/uploads/itemdocuments/" + ItemID);
                    if (!Directory.Exists(storePath))
                        Directory.CreateDirectory(storePath);
                    var InputFileName = Path.GetFileName(file.FileName);
                    var ServerSavePath = Path.Combine(storePath + "/" + InputFileName);
                    //Save file to server folder  
                    file.SaveAs(ServerSavePath);
                    flag = 1;
                }
            }
            if (flag == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion
        #region slug
        public string GenerateSlug(string Name)
        {
            string str = RemoveAccent(Name).ToLower();
            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens   
            return str;
        }

        private string RemoveAccent(string text)
        {
            byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(text);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }
        #endregion

        #region Item related functions get color size unit tax prefix barcode category brand supplier
        public long GetprefixNo(long pre)
        {
            Int64 No = 0;
            Int64 number = db.ItemPrefixs.Where(a => a.Prefix == pre).Select(a => a.No).AsEnumerable().DefaultIfEmpty(0).Max();
            if (number == 0)
            {
                No = 1;
            }
            else
            {
                No = number + 1;
            }
            return No;
        }
        public long GetCategoryId(string Category)
        {
            long CatID = 0;
            var Exists = db.ItemCategorys.Any(c => c.ItemCategoryName == Category);
            if (Exists)
            {
                CatID = db.ItemCategorys.Where(c => c.ItemCategoryName == Category).Select(c => c.ItemCategoryID).FirstOrDefault();
            }
            else
            {
                ItemCategory catgy = new ItemCategory();
                catgy.ItemCategoryName = Category;
                db.ItemCategorys.Add(catgy);
                db.SaveChanges();
                CatID = catgy.ItemCategoryID;
            }
            return CatID;
        }


        public long GetBrandId(string Brand)
        {
            long BranchID = 0;
            var Exists = db.ItemBrands.Any(c => c.ItemBrandName == Brand);
            if (Exists)
            {
                BranchID = db.ItemBrands.Where(c => c.ItemBrandName == Brand).Select(c => c.ItemBrandID).FirstOrDefault();
            }
            else
            {
                ItemBrand brd = new ItemBrand();
                brd.ItemBrandName = Brand;
                db.ItemBrands.Add(brd);
                db.SaveChanges();
                BranchID = brd.ItemBrandID;
            }
            return BranchID;
        }

        public long GetSupplierId(string Supplier)
        {
            long SupID = 0;
            var Exists = db.Suppliers.Any(c => c.SupplierName == Supplier);
            if (Exists)
            {
                SupID = db.Suppliers.Where(c => c.SupplierName == Supplier).Select(c => c.SupplierID).FirstOrDefault();
            }
            else
            {
                Supplier catgy = new Supplier();
                catgy.SupplierName = Supplier;
                db.Suppliers.Add(catgy);
                db.SaveChanges();
                SupID = catgy.SupplierID;
            }
            return SupID;
        }


        public long GetUnitId(string Unit)
        {
            long UnitID = 0;
            var Exists = db.ItemUnits.Any(c => c.ItemUnitName == Unit);
            if (Exists)
            {
                UnitID = db.ItemUnits.Where(c => c.ItemUnitName == Unit).Select(c => c.ItemUnitID).FirstOrDefault();
            }
            else
            {
                ItemUnit NewUnit = new ItemUnit();
                NewUnit.ItemUnitName = Unit;
                db.ItemUnits.Add(NewUnit);
                db.SaveChanges();
                UnitID = NewUnit.ItemUnitID;
            }
            return UnitID;
        }


        public long GetColorId(string Color)
        {
            long ColorID = 0;
            var Exists = db.ItemColors.Any(c => c.ItemColorName == Color);
            if (Exists)
            {
                ColorID = db.ItemColors.Where(c => c.ItemColorName == Color).Select(c => c.ItemColorID).FirstOrDefault();
            }
            else
            {
                ItemColor NewColor = new ItemColor();
                NewColor.ItemColorName = Color;
                db.ItemColors.Add(NewColor);
                db.SaveChanges();
                ColorID = NewColor.ItemColorID;
            }
            return ColorID;
        }

        public long GetSizeId(string Size)
        {
            long SizeID = 0;
            var Exists = db.ItemSizes.Any(c => c.ItemSizeName == Size);
            if (Exists)
            {
                SizeID = db.ItemSizes.Where(c => c.ItemSizeName == Size).Select(c => c.ItemSizeID).FirstOrDefault();
            }
            else
            {
                ItemSize NewSize = new ItemSize();
                NewSize.ItemSizeName = Size;
                db.ItemSizes.Add(NewSize);
                db.SaveChanges();
                SizeID = NewSize.ItemSizeID;
            }
            return SizeID;
        }


        public long GetTaxId(decimal Tax)
        {
            long TaxID;
            TaxID = db.Taxs.Where(c => c.Percentage == (Tax)).Select(c => c.TaxID).FirstOrDefault();
            return TaxID;
        }


        public long createBarcode(Int64 INo = 0, Int64 ICode = 0)
        {
            if (ICode == 0)
            {
                if ((db.Barcodes.Select(p => p.BarcodeId).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    ICode = 10000001;
                }
                else
                {
                    INo = db.Barcodes.Max(p => p.BarcodeId + 1);
                    ICode = 10000000 + INo;
                    if (barCodeExist(ICode.ToString()))
                    {
                        ICode = createBarcode(INo, ICode);
                    }

                }
            }
            else
            {
                ICode = ICode + 1;
                if (barCodeExist(ICode.ToString()))
                {
                    ICode = createBarcode(INo, ICode);
                }
            }
            return ICode;
        }
        private bool barCodeExist(string Code)
        {
            var Exists = db.Barcodes.Any(c => c.BarcodeNumber == Code);
            bool res = (Exists) ? true : false;
            return res;
        }
        #endregion

        #region Accounts Under a group and its child
        public long[] AllAccounts(long? AccGroup)
        {
            var arr = AllGroups(AccGroup);
            long[] arry = db.Accountss.Where(a => arr.Contains(a.Group)).Select(a => a.AccountsID).ToArray();
            return arry;
        }
        public List<long> AllGroups(long? AccGroup)
        {
            List<long> arr = new List<long>();
            var cusparentid = new SqlParameter("@parentid", AccGroup);
            var cusgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", cusparentid).ToList();
            arr = cusgroupsdata.Select(a => a.AccountsGroupID).ToList();
            return arr;
        }
        #endregion

        #region stock
        // add stock transaction
        public int addStock(decimal StockIn, decimal StockOut, long Item, long? Unit = null, string Purpose = "", long reference = 0, decimal Cost = 0, decimal StockValue = 0, DateTime? Date = null, long? MC = null, Status status = Status.active)
        {
            var Stock = new Stock
            {
                Item = Item,
                Unit = Unit,
                stockIn = StockIn,
                stockOut = StockOut,
                Cost = Cost,
                StockValue = StockValue,
                Purpose = Purpose,
                reference = reference,
                MC = MC,
                Status = status,
                Date = Date,
                CreatedDate = Convert.ToDateTime(System.DateTime.Now)
            };
            db.Stocks.Add(Stock);

            return db.SaveChanges();
        }

        public int addStockbulk(ArrayList array)
        {
            DataTable STEntry = new DataTable();
            STEntry.Columns.Add("Item");
            STEntry.Columns.Add("Unit");
            STEntry.Columns.Add("stockIn");
            STEntry.Columns.Add("stockOut");
            STEntry.Columns.Add("Cost");
            STEntry.Columns.Add("StockValue");
            STEntry.Columns.Add("Purpose");
            STEntry.Columns.Add("reference");
            STEntry.Columns.Add("MC");
            STEntry.Columns.Add("Date");
            STEntry.Columns.Add("Status");
            STEntry.Columns.Add("CreatedDate");
            List<Stock> STList = array.Cast<Stock>().ToList();
            List<Stock> StockAddList = new List<Stock>();
            for (int i = 0; i < STList.Count; i++)
            {
                int Stat = STList[i].Status == Status.active ? 0 : 1;
                DataRow st = STEntry.NewRow();
                st["Item"] = STList[i].Item;
                st["Unit"] = STList[i].Unit;
                st["stockIn"] = STList[i].stockIn;
                st["stockOut"] = STList[i].stockOut;
                st["Cost"] = STList[i].Cost;
                st["StockValue"] = STList[i].StockValue;
                st["Purpose"] = STList[i].Purpose;
                st["reference"] = STList[i].reference;
                st["MC"] = STList[i].MC;
                st["Date"] = STList[i].Date;
                st["Status"] = Stat;
                st["CreatedDate"] = DateTime.Now;
                STEntry.Rows.Add(st);
            }
            ////// create parameter 
            SqlParameter parameter = new SqlParameter("@TableType", STEntry);
            parameter.SqlDbType = SqlDbType.Structured;
            parameter.TypeName = "TableTypeSTItems";
            //// execute sp sql 
            string sql = String.Format("EXEC {0} {1};", "SP_InsertSTItems", "@TableType");
            //// execute sql
            var result = db.Database.ExecuteSqlRaw(sql, parameter);

            return result;
        }
        // Update stock 
        public int UpdateStock(decimal StockIn, decimal StockOut, long Item, long? Unit = null, string Purpose = "", long reference = 0, decimal Cost = 0, decimal StockValue = 0, DateTime? Date = null, long? MC = null, Status status = Status.active)
        {
            Stock Stock = db.Stocks.Find(Item);
            Stock.Unit = Unit;
            Stock.stockIn = StockIn;
            Stock.stockOut = StockOut;
            Stock.Cost = Cost;
            Stock.StockValue = StockValue;
            Stock.Purpose = Purpose;
            Stock.reference = reference;
            Stock.MC = MC;
            Stock.Date = Date;
            db.Entry(Stock).State = EntityState.Modified;
            return db.SaveChanges();
        }
        // delete stock transaction
        public bool DeleteStock(string purpose, long reference = 0)
        {
            db.Stocks.RemoveRange(db.Stocks.Where(a => a.Purpose == purpose && a.reference == reference));
            int delete = db.SaveChanges();
            if (delete != 0)
                return true;
            else
                return false;
        }
        // delete all related stock transaction
        public bool DeleteAllStock(string purpose, long reference = 0)
        {
            db.Stocks.RemoveRange(db.Stocks.Where(a => a.Purpose == purpose && a.reference == reference));
            int delete = db.SaveChanges();
            if (delete != 0)
                return true;
            else
                return false;
        }
        #endregion

        #region Ledger
        public LedgerViewModel LedgerDatacommend(long AccId, string fromdate, string todate, long? AccGroup, bool? pdc)
        {

            LedgerViewModel vmodel = new LedgerViewModel();
            String format = "dd-MM-yyyy";
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? from = null;
            DateTime? to = null;
            Dictionary<string, object> Balance = null;
            long[] Accounts = { };
            if (fromdate != "")
            {
                //  fdate = DateTime.Parse(fromdate.ToString(), new CultureInfo("en-GB"));
                fdate = DateTime.ParseExact(fromdate, format, new CultureInfo("en-GB"));
                from = fdate;
            }
            else
                {
                //  fdate = DateTime.Parse(fromdate.ToString(), new CultureInfo("en-GB"));
                fdate = DateTime.ParseExact("01-01-2011", format, new CultureInfo("en-GB"));
                from = fdate;

            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate.ToString(), new CultureInfo("en-GB"));
                to = tdate;
            }
            if (AccId == -1)
            {
                Accounts = AllAccounts(AccGroup);
            }
            else
            {
                Array.Resize(ref Accounts, Accounts.Length + 1);
                Accounts[0] = AccId;
            }
            Balance = OpenBlnc(AccId, (DateTime)fdate, pdc, Accounts);

            if ((string)Balance["type"] == "Cr")
            {
                vmodel.OpeningBalance = (decimal)Balance["amount"];
                vmodel.blnceType = (string)Balance["type"];
            }
            else
            {
                vmodel.OpeningBalance = (0 - (decimal)Balance["amount"]);
                vmodel.blnceType = (string)Balance["type"];

            }
            var Group = (from a in db.AccountsGroups
                         where a.AccountsGroupID == AccGroup
                         select new
                         {
                             a.Name,
                             a.Alias
                         }).FirstOrDefault();
            var Account = (from a in db.Accountss
                           where a.AccountsID == AccId
                           select new
                           {
                               a.Name,
                               a.Alias,
                               a.OpnBalance,
                               a.OpnBalanceCr
                           }).FirstOrDefault();
            vmodel.MainAccount = (AccId != -1) ? Account.Name : Group.Name;
            vmodel.MainAccountID = AccId;
            vmodel.from = from;
            vmodel.to = to;
            // Transactions


            var ProperytReg = (from a in db.AccountsTransactions
                               join b in db.Accountss on a.Account equals b.AccountsID
                               join c in db.PropertyRegistrations on a.reference equals c.RegistrationID


                               where (fromdate == null || EF.Functions.DateDiffDay(c.CreatedDate, fdate) <= 0) &&
                               (todate == null || EF.Functions.DateDiffDay(c.CreatedDate, tdate) >= 0) &&
                               (a.Account == AccId && a.Purpose == "RegistrationDeposit")
                               && (pdc == true || a.Status == null)
                               select new
                               {
                                   id = c.RegistrationID,
                                   particulars = b.Name,
                                   a.Project,
                                   Date = (DateTime?)c.CreatedDate,
                                   Invoice = c.VoucherNo,
                                   Type = "RegistrationDeposit",
                                   RAccount = "",
                                   RAccountID = b.AccountsID,
                                   Debit = (decimal?)a.Debit,
                                   Credit = (decimal?)a.Credit,
                                   entry = (DateTime?)a.CreatedDate,
                                   Remark = "RegistrationDeposit",
                                   Amount = c.Amount,
                                   TRN = "",
                                   TransactionId = a.Id,
                                   Account = a.Account,
                                   reference = a.reference
                               });



            var tenancyReg = (from a in db.AccountsTransactions
                              join b in db.Accountss on a.Account equals b.AccountsID
                              join c in db.TenancyContracts on a.reference equals c.Id


                              where (fromdate == null || EF.Functions.DateDiffDay(c.CreatedDate, fdate) <= 0) &&
                              (todate == null || EF.Functions.DateDiffDay(c.CreatedDate, tdate) >= 0) &&
                              (a.Account == AccId && a.Purpose.Contains("Tenancy"))
                              && (pdc == true || a.Status == null)
                              select new
                              {
                                  id = c.Id,
                                  particulars = b.Name,
                                  a.Project,
                                  Date = (DateTime?)c.CreatedDate,
                                  Invoice = c.Id.ToString(),
                                  Type = "TenancyContract ",
                                  RAccount = "",
                                  RAccountID = b.AccountsID,
                                  Debit = (decimal?)a.Debit,
                                  Credit = (decimal?)a.Credit,
                                  entry = (DateTime?)a.CreatedDate,
                                  Remark = "TenancyContract",
                                  Amount = (decimal)c.Rent + (decimal)c.Deposit,
                                  TRN = "",
                                  TransactionId = a.Id,
                                  Account = a.Account,
                                  reference = a.reference
                              });




            var maintanacerpt = (from a in db.AccountsTransactions
                                 join b in db.Accountss on a.Account equals b.AccountsID
                                 join c in db.Maintenances on a.reference equals c.ID


                                 where (fromdate == null || EF.Functions.DateDiffDay(c.CreatedDate, fdate) <= 0) &&
                                 (todate == null || EF.Functions.DateDiffDay(c.CreatedDate, tdate) >= 0) &&
                                 (a.Account == AccId && a.Purpose.Contains("Maintenance"))
                                 && (pdc == true || a.Status == null)
                                 select new
                                 {
                                     id = c.ID,
                                     particulars = b.Name,
                                     a.Project,
                                     Date = (DateTime?)c.CreatedDate,
                                     Invoice = c.ID.ToString(),
                                     Type = "Maintenance ",
                                     RAccount = "",
                                     RAccountID = b.AccountsID,
                                     Debit = (decimal?)a.Debit,
                                     Credit = (decimal?)a.Credit,
                                     entry = (DateTime?)a.CreatedDate,
                                     Remark = "Maintenance",
                                     Amount = c.Amount,
                                     TRN = "",
                                     TransactionId = a.Id,
                                     Account = a.Account,
                                     reference = a.reference
                                 });

            var Asset = (from a in db.AccountsTransactions
                         join b in db.Accountss on a.Account equals b.AccountsID
                        
                         where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                         (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                         (a.Account == AccId && a.Purpose.Contains("Asset From Inventory"))
                         && (pdc == true || a.Status == null)
                         select new
                         {
                             id = a.Id,
                             particulars = b.Name,
                             Project=a.Project,
                             Date = (DateTime?)a.Date,
                             Invoice = a.reference.ToString(),
                             Type = "Asset From Inventory ",
                             RAccount = (b.AccountsID == a.Account) ? b.Name : "Asset From Inventory",
                             RAccountID = b.AccountsID,
                             Debit = (decimal?)a.Debit,
                             Credit = (decimal?)a.Credit,
                             entry = (DateTime?)a.CreatedDate,
                             Remark = "Asset From Inventory",
                             Amount = a.Debit,//db.AssetTransferDetails.Where(z => z.AssetEntryId == a.reference && c.AssetAccountId == b.AccountsID).Select(z => z.TotalPrice).Sum(),
                            
                             TRN = "",
                             TransactionId = a.Id,
                             Account = a.Account,
                             reference = a.reference
                         });

            var AssetPurchase = (from a in db.AccountsTransactions
                                 join b in db.Accountss on a.Account equals b.AccountsID
                                 join c in db.AssetTransferDetails on a.reference equals c.AssetEntryId
                                 join d in db.AssetTransferMasters on c.AssetEntryId equals d.AssetEntryId

                                 where (fromdate == null || EF.Functions.DateDiffDay(d.AssetEntryDate, fdate) <= 0) &&
                                 (todate == null || EF.Functions.DateDiffDay(d.AssetEntryDate, tdate) >= 0) &&
                                 (a.Account == AccId && a.Purpose == "AssetPurchase")
                                 && (pdc == true || a.Status == null)
                                 select new
                                 {
                                     id = c.AssetEntryId,
                                     particulars = b.Name,
                                     Project=a.Project,
                                     Date = (DateTime?)d.AssetEntryDate,
                                     Invoice = c.AssetEntryId.ToString(),
                                     Type = "AssetPurchase",
                                     RAccount = (b.AccountsID == a.Account) ? b.Name : "AssetPurchase",
                                     RAccountID = b.AccountsID,
                                     Debit = (decimal?)a.Debit,
                                     Credit = (decimal?)a.Credit,
                                     entry = (DateTime?)a.CreatedDate,
                                     Remark = "AssetPurchase",
                                     Amount = db.AssetTransferDetails.Where(z => z.AssetEntryId == a.reference && c.AssetAccountId == b.AccountsID).Select(z => z.TotalPrice).Sum(),
                                     TRN = "",
                                     TransactionId = a.Id,
                                     Account = a.Account,
                                     reference = a.reference,
                                    
                                 });

            //Asset To Inventory
            var AssetToInventory = (from a in db.AccountsTransactions
                                    join b in db.Accountss on a.Account equals b.AccountsID
                                    join c in db.AssetToInventoryMasters on a.reference equals c.EntryId
                                    where (fromdate == null || EF.Functions.DateDiffDay(c.EntryDate, fdate) <= 0) &&
                                    (todate == null || EF.Functions.DateDiffDay(c.EntryDate, tdate) >= 0) &&
                                    (a.Account == AccId && a.Purpose.Contains("Asset To Inventory"))
                                    && (pdc == true || a.Status == null)
                                    select new
                                    {
                                        id = c.EntryId,
                                        particulars = b.Name,
                                        Project = a.Project,
                                        Date = (DateTime?)c.EntryDate,
                                        Invoice = c.EntryNo.ToString(),
                                        Type = "Asset To Inventory ",
                                        RAccount = (b.AccountsID == a.Account) ? b.Name : "Asset To Inventory",
                                        RAccountID = b.AccountsID,
                                        Debit = (decimal?)a.Debit,
                                        Credit = (decimal?)a.Credit,
                                        entry = (DateTime?)a.CreatedDate,
                                        Remark = "Asset To Inventory",
                                        Amount = c.TotalAmount,
                                        TRN = "",
                                   
                                        TransactionId = a.Id,
                                        Account = a.Account,
                                        reference = a.reference
                                    });


            var Reciept = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Receipts on a.reference equals c.ReceiptId
                           join d in db.PDCs on new { f1 = c.ReceiptId, f2 = "Receipt" } equals new { f1 = d.Reference, f2 = d.PDCType } into pdcs
                           from d in pdcs.DefaultIfEmpty()
                           let bb = db.Accountss.Where(at => (c.PayFrom != a.Account && at.AccountsID == c.PayFrom) || (c.PayFrom == a.Account && at.AccountsID == c.PayTo)).FirstOrDefault()
                           where (d.PDCType == "Receipt" && fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Receipt" || a.Purpose == "Discount Allowed")
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.ReceiptId,
                               particulars = b.Name,
                               Project = a.Project,
                               Date = (a.Status == null) ? (DateTime?)a.Date : c.Date,
                               Invoice = c.VoucherNo,
                               Type = a.Purpose,
                              
                               RAccount = bb.Name,
                               RAccountID = bb.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = c.Remark + ((d.PDCDate != null) ? " Pdc Date :" + d.PDCDate : ""),
                               Amount = c.GrandTotal - (decimal)c.Discount - ((decimal)a.Debit == 0 ? (decimal)a.Credit : (decimal)a.Debit),
                               TRN = bb.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            var Payment = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Payments on a.reference equals c.PaymentId
                           join d in db.PDCs on new { f1 = c.PaymentId, f2 = "Payment" } equals new { f1 = d.Reference, f2 = d.PDCType } into pdcs
                           from d in pdcs.DefaultIfEmpty()
                           let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
                           where (d.PDCType == "Payment" && fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Payment" || a.Purpose == "Expense Payment" || a.Purpose == "Discount Received")
                           && (pdc == true || a.Status == null)
                           && a.Purpose == "Payment"
                           select new
                           {
                               id = c.PaymentId,
                               particulars = b.Name,
                               Project=a.Project,
                               Date = (a.Status == null) ? (DateTime?)a.Date : c.Date,
                               Invoice = c.VoucherNo,
                               Type = a.Purpose,
                               RAccount = bb.Name,
                               RAccountID = bb.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = c.Remark + ((d.PDCDate != null) ? " Pdc Date :" + d.PDCDate : ""),
                               Amount = c.GrandTotal - c.Discount - ((decimal)a.Debit == 0 ? (decimal)a.Credit : (decimal)a.Debit),
                               TRN = bb.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            //var RecieptDiscount = (from a in db.AccountsTransactions
            //                       join b in db.Accountss on a.Account equals b.AccountsID
            //                       join c in db.Receipts on a.reference equals c.ReceiptId
            //                       let bb = db.Accountss.Where(at => (c.PayFrom != a.Account && at.AccountsID == c.PayFrom) || (c.PayFrom == a.Account && at.AccountsID == c.PayTo)).FirstOrDefault()
            //                       where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
            //                       (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
            //                       (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Discount Allowed")
            //                       && (pdc == true || a.Status == null)
            //                       select new
            //                       {
            //                           id = c.ReceiptId,
            //                           particulars = b.Name,
            //                           Date = (DateTime?)a.Date,
            //                           Invoice = c.VoucherNo,
            //                           Type = a.Purpose,
            //                           RAccount = bb.Name,
            //                           RAccountID = bb.AccountsID,
            //                           Debit = (decimal?)a.Debit,
            //                           Credit = (decimal?)a.Credit,
            //                           entry = (DateTime?)a.CreatedDate,
            //                           Remark = c.Remark
            //                       });
            //var PaymentDiscount = (from a in db.AccountsTransactions
            //                       join b in db.Accountss on a.Account equals b.AccountsID
            //                       join c in db.Payments on a.reference equals c.PaymentId
            //                       let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
            //                       where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
            //                       (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
            //                       (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Discount Recieve")
            //                       && (pdc == true || a.Status == null)
            //                       select new
            //                       {
            //                           id = c.PaymentId,
            //                           particulars = b.Name,
            //                           Date = (DateTime?)a.Date,
            //                           Invoice = c.VoucherNo,
            //                           Type = a.Purpose,
            //                           RAccount = bb.Name,
            //                           RAccountID = bb.AccountsID,
            //                           Debit = (decimal?)a.Debit,
            //                           Credit = (decimal?)a.Credit,
            //                           entry = (DateTime?)a.CreatedDate,
            //                           Remark = c.Remark
            //                       });
                                                      
            var Sale = (from a in db.AccountsTransactions
                        join b in db.Accountss on a.Account equals b.AccountsID
                        join c in db.SalesEntrys on a.reference equals c.SalesEntryId
                        join d in db.Customers on c.Customer equals d.CustomerID
                        let bb = db.Receipts.Where(at => a.Purpose == "Sale Payment" && at.editable == choice.No && at.Reference == c.SalesEntryId && at.Remark == "Direct Reciept From Sale Entry").Join(db.Accountss, f1 => f1.PayTo, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                        let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()

                        where (fromdate == null || EF.Functions.DateDiffDay(c.SEDate, fdate) <= 0) &&
                        (todate == null || EF.Functions.DateDiffDay(c.SEDate, tdate) >= 0) &&
                        (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Sale" || a.Purpose == "Sale Payment")
                        && (pdc == true || a.Status == null)
                        select new
                        {
                            id = c.SalesEntryId,
                            particulars = b.Name,
                            Project= a.Project,
                            Date = (DateTime?)c.SEDate,
                            Invoice = (a.Purpose != "Sale Payment" || (d.Accounts != a.Account)) ? c.BillNo : "",
                            Type = (a.Purpose != "Sale Payment") ? a.Purpose : (d.Accounts != a.Account) ? "Sale" : "",
                            RAccount = (d.Accounts == a.Account) ? ((a.Purpose != "Sale Payment") ? "Sale" : bb.Name) : d.CustomerName,
                            RAccountID = b.AccountsID,
                            Debit = (decimal?)a.Debit,
                            Credit = (decimal?)a.Credit,
                            entry = (DateTime?)a.CreatedDate,
                            Remark = (a.Purpose != "Sale Payment") ? c.Remarks : "",
                            Amount = (c.SalesType != 3 && a.Account != 502) ? c.SESubTotal - c.SEDiscount : ((a.Credit / (decimal)5) * 100),
                            TRN = AC.TRN,
                            TransactionId = a.Id,
                            Account = a.Account,
                            reference = a.reference
                        });
            var SReturn = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.SalesReturns on a.reference equals c.SalesReturnId
                           join d in db.Customers on c.Customer equals d.CustomerID
                           let bb = db.Payments.Where(at => a.Purpose == "Sale Return Payment" && at.editable == choice.No && at.Reference == c.SalesReturnId && at.RefType == "SalesReturn").Join(db.Accountss, f1 => f1.PayFrom, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                           let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                           where (fromdate == null || EF.Functions.DateDiffDay(c.SRDate, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(c.SRDate, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Sale Return" || a.Purpose == "Sale Return Payment")
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.SalesReturnId,
                               particulars = b.Name,
                               Project=a.Project,
                               Date = (DateTime?)c.SRDate,
                               Invoice = a.Purpose != "Sale Return Payment" ? c.BillNo : "",
                               Type = a.Purpose != "Sale Return Payment" ? a.Purpose : "",
                               RAccount = (d.Accounts == a.Account) ? ((a.Purpose != "Sale Return Payment") ? "Sales Return" : bb.Name) : d.CustomerName,
                               RAccountID = b.AccountsID,
                          
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = a.Purpose != "Sale Return Payment" ? c.Remarks : "",
                               Amount = c.SRSubTotal - c.SRDiscount,
                               TRN = AC.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference,
                          
                           });
            var Purchase = (from a in db.AccountsTransactions
                            join b in db.Accountss on a.Account equals b.AccountsID
                            join c in db.PurchaseEntrys on a.reference equals c.PurchaseEntryId
                            join d in db.Suppliers on c.Supplier equals d.SupplierID

                            let sundry = (from g in db.AccountsTransactions

                                          join i in db.PurchaseEntrys on g.reference equals i.PurchaseEntryId
                                          join j in db.PEBillSundrys on i.PurchaseEntryId equals j.PurchaseEntry
                                          join k in db.BillSundrys on j.BillSundry equals k.BillSundryId
                                          where g.reference == c.PurchaseEntryId && g.Purpose == "Purchase" && a.Type == 0
                                          select new
                                          {
                                              k.BSName
                                          }).FirstOrDefault().BSName
                            let bb = db.Payments.Where(at => a.Purpose == "Purchase Payment" && at.editable == choice.No && at.Reference == c.PurchaseEntryId && at.RefType == "Purchase").Join(db.Accountss, f1 => f1.PayFrom, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                            let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                            where (fromdate == "" || EF.Functions.DateDiffDay(c.PEDate, fdate) <= 0) &&
                            (todate == "" || EF.Functions.DateDiffDay(c.PEDate, tdate) >= 0) &&
                            (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Purchase" || a.Purpose == "Purchase Payment")
                            && (pdc == true || a.Status == null)
                            select new
                            {
                                id = c.PurchaseEntryId,
                                particulars = sundry,
                                Project = a.Project,
                                Date = (DateTime?)c.PEDate,
                                Invoice = a.Purpose == "Purchase Payment" ? "" : c.BillNo,
                                Type = a.Purpose == "Purchase Payment" ? "" : (sundry == null) ? a.Purpose : sundry,
                                RAccount = (d.Accounts == a.Account) ? ((a.Purpose != "Purchase Payment") ? "Purchase" : bb.Name) : d.SupplierName,
                                RAccountID = b.AccountsID,
                                Debit = (decimal?)a.Debit,
                                Credit = (decimal?)a.Credit,
                                entry = (DateTime?)a.CreatedDate,
                                Remark = a.Purpose == "Purchase Payment" ? "" : c.Remarks,
                                Amount = c.PESubTotal - c.PEDiscount,
                                TRN = AC.TRN,
                                TransactionId = a.Id,
                                Account = a.Account,
                                reference = a.reference

                            });
            var PReturn = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.PurchaseReturns on a.reference equals c.PurchaseReturnId
                           join d in db.Suppliers on c.Supplier equals d.SupplierID
                           let bb = db.Receipts.Where(at => a.Purpose == "Purchase Return Payment" && at.editable == choice.No && at.Reference == c.PurchaseReturnId && at.Remark == "Receipt From Purchase Return").Join(db.Accountss, f1 => f1.PayTo, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                           let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                           where (fromdate == "" || EF.Functions.DateDiffDay(c.PRDate, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(c.PRDate, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Purchase Return" || a.Purpose == "Purchase Return Payment")
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.PurchaseReturnId,
                               particulars = b.Name,
                               Project = a.Project,
                               Date = (DateTime?)c.PRDate,
                               Invoice = a.Purpose == "Purchase Return Payment" ? "" : c.BillNo,
                               Type = a.Purpose == "Purchase Return Payment" ? "" : a.Purpose,
                               RAccount = (d.Accounts == a.Account) ? "Purchase" : d.SupplierName,
                               RAccountID = b.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = a.Purpose == "Purchase Return Payment" ? "" : c.Remarks,
                               Amount = c.PRSubTotal - c.PRDiscount,
                               TRN = AC.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            var Journal = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Journals on a.reference equals c.JournalId
                           //let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
                           join d in db.PDCs on new { f1 = c.JournalId, f2 = "Journal" } equals new { f1 = d.Reference, f2 = d.PDCType } into pdcs
                           from d in pdcs.DefaultIfEmpty()
                           let bd = db.Accountss.Where(at => (a.Type == DC.Credit && at.AccountsID == c.PayTo) || (a.Type == DC.Debit && at.AccountsID == c.PayFrom)).FirstOrDefault()
                           let acCount = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && z.Account == a.Account && z.Type == a.Type).Select(z => a.Account).Count()
                           let prev = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && a.Type != z.Type && (a.Type == DC.Credit && a.Id > z.Id) && z.Account != 501 && z.Account != 502).OrderByDescending(z => z.Id).Select(z => z.Account).FirstOrDefault()
                           let next = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && a.Type != z.Type && (a.Type == DC.Debit && a.Id < z.Id) && z.Account != 501 && z.Account != 502).OrderBy(z => z.Id).Select(z => z.Account).FirstOrDefault()
                           where (d.PDCType == "Journal" && fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && a.Purpose == "Journal"
                           && (pdc == true || a.Status == null) &&
                           a.Purpose == "Journal"

                           select new
                           {
                               id = c.JournalId,
                               particulars = b.Name,
                               Project = a.Project,
                               Date = (DateTime?)a.Date,
                               Invoice = c.VoucherNo,
                               Type = "Journal Entry",
                             
                               RAccount = (acCount <= 1) ? bd.Name : db.Accountss.Where(y => (a.Type == DC.Credit && y.AccountsID == prev) || (a.Type == DC.Debit && y.AccountsID == next)).Select(y => y.Name).FirstOrDefault(),
                               RAccountID = bd.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = a.Narration+" "+c.Remark + ((d.PDCDate != null) ? " Pdc Date :" + d.PDCDate : ""),
                               Amount = (decimal)c.GrandTotal,
                               TRN = bd.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            var Contra = (from a in db.AccountsTransactions
                          join b in db.Accountss on a.Account equals b.AccountsID
                          join c in db.ContraVouchers on a.reference equals c.ContraVoucherId
                          let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
                          where (fromdate == null || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) &&
                          (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) &&
                          (todate == null || EF.Functions.DateDiffDay(c.Date, tdate) >= 0) && a.Purpose == "ContraVoucher"
                          && (pdc == true || a.Status == null)
                          select new
                          {
                              id = c.ContraVoucherId,
                              particulars = b.Name,
                              Project = a.Project,
                              Date = (DateTime?)a.Date,
                              Invoice = c.VoucherNo,
                              Type = "Contra Voucher",
                            
                              RAccount = bb.Name,
                              RAccountID = bb.AccountsID,
                              Debit = (decimal?)a.Debit,
                              Credit = (decimal?)a.Credit,
                              entry = (DateTime?)a.CreatedDate,
                              Remark = c.Remark,
                              Amount = c.Amount,
                              TRN = bb.TRN,
                              TransactionId = a.Id,
                              Account = a.Account,
                              reference = a.reference

                          });
            var StockAdjustment = (from a in db.AccountsTransactions
                                   join b in db.Accountss on a.Account equals b.AccountsID
                                   join c in db.StockAdjustments on a.reference equals c.StockAdjustmentId
                                   where (fromdate == null || EF.Functions.DateDiffDay(c.AdjDate, fdate) <= 0) &&
                                   (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) &&
                                   (todate == null || EF.Functions.DateDiffDay(c.AdjDate, tdate) >= 0) && a.Purpose == "Stock Adjustment"
                                   select new
                                   {
                                       id = c.StockAdjustmentId,
                                       particulars = b.Name,
                                       Project=a.Project,
                                       Date = (DateTime?)a.Date,
                                       Invoice = c.VoucherNo,
                                       Type = "Contra Voucher",
                                      
                                       RAccount = b.Name,
                                       RAccountID = b.AccountsID,
                                       Debit = (decimal?)a.Debit,
                                       Credit = (decimal?)a.Credit,
                                       entry = (DateTime?)a.CreatedDate,
                                       Remark = c.Reason,
                                       Amount = c.PurchaseRate,
                                       TRN = b.TRN,
                                       TransactionId = a.Id,
                                       Account = a.Account,
                                       reference = a.reference
                                   });
            decimal dumy = 100;
            var payroll = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && a.Purpose == "Payroll Voucher"
                           select new
                           {
                               id = a.Id,
                               particulars = b.Name,
                               Project=a.Project,
                               Date = (DateTime?)a.Date,
                               Invoice = a.reference.ToString(),
                               Type = "Payroll Voucher",
                              
                               RAccount = b.Name,
                               RAccountID = b.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = "Payroll",
                               Amount = dumy,
                               TRN = b.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            
            var rent = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && a.Purpose == "Rent Receivable"
                        select new
                           {
                               id = a.Id,
                               particulars = b.Name,
                            Project = a.Project,
                            Date = (DateTime?)a.Date,
                               Invoice = a.reference.ToString(),
                               Type = "Rent Receivable",
                         
                            RAccount = b.Name,
                               RAccountID = b.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = "Rent",
                               Amount = (decimal)a.Credit,
                               TRN = b.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            var full = Payment.Union(Reciept);
            // var disc = PaymentDiscount.Union(RecieptDiscount);

            var pur = Purchase.Union(PReturn);
            var sal = Sale.Union(SReturn);
            var joc = Journal.Union(Contra);

            // full = full.Union(disc);
            full = full.Union(pur);
            full = full.Union(payroll);
            full = full.Union(sal);
            full = full.Union(joc);
            full = full.Union(StockAdjustment);
            full = full.Union(ProperytReg);
            full = full.Union(tenancyReg);
            full = full.Union(maintanacerpt);
            full = full.Union(Asset);
            full = full.Union(AssetPurchase);
            full = full.Union(AssetToInventory);
            //full = full.Union(RecieptDiscount);
            full = full.Union(rent);
            full = full.AsQueryable().OrderBy("Date asc, entry asc");
            vmodel.Ledger = (from a in full
                             select new Ledger
                             {
                                 Date = a.Date,
                                 Invoice = a.Invoice,
                                 Type = a.Type,
                                 RAccount = a.RAccount,
                                 RAccountID = a.RAccountID,
                                 Debit = a.Debit,
                                 Credit = a.Credit,
                                 particulars = a.particulars,
                                 Remark = a.Remark,
                                 Amount = a.Amount,
                                 MainId = a.id,
                                 TRN = a.TRN,
                                 TransactionId = a.TransactionId,
                                 Account = a.Account,
                                 Reference = a.reference,
                                 projectname = db.Projects.Where(o=>o.ProjectId==a.Project).Select(o=>o.ProjectName).FirstOrDefault()
                             }).ToList();
            return vmodel;
        }
        #endregion
        #region Ledgerps
        public LedgerViewModel LedgerDatacommendpos(long AccId, string fromdate, string todate, long? AccGroup, bool? pdc)
        {

            LedgerViewModel vmodel = new LedgerViewModel();
            String format = "dd-MM-yyyy";
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? from = null;
            DateTime? to = null;
            Dictionary<string, object> Balance = null;
            long[] Accounts = { };
            if (fromdate != "")
            {
                //  fdate = DateTime.Parse(fromdate.ToString(), new CultureInfo("en-GB"));
                fdate = DateTime.ParseExact(fromdate, format, new CultureInfo("en-GB"));
                from = fdate;
            }
            else
            {
                //  fdate = DateTime.Parse(fromdate.ToString(), new CultureInfo("en-GB"));
                fdate = DateTime.ParseExact("01-01-2011", format, new CultureInfo("en-GB"));
                from = fdate;

            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate.ToString(), new CultureInfo("en-GB"));
                to = tdate;
            }
            if (AccId == -1)
            {
                Accounts = AllAccounts(AccGroup);
            }
            else
            {
                Array.Resize(ref Accounts, Accounts.Length + 1);
                Accounts[0] = AccId;
            }
            Balance = OpenBlnc(AccId, (DateTime)fdate, pdc, Accounts);

            if ((string)Balance["type"] == "Cr")
            {
                vmodel.OpeningBalance = (decimal)Balance["amount"];
                vmodel.blnceType = (string)Balance["type"];
            }
            else
            {
                vmodel.OpeningBalance = (0 - (decimal)Balance["amount"]);
                vmodel.blnceType = (string)Balance["type"];

            }
            var Group = (from a in db.AccountsGroups
                         where a.AccountsGroupID == AccGroup
                         select new
                         {
                             a.Name,
                             a.Alias
                         }).FirstOrDefault();
            var Account = (from a in db.Accountss
                           where a.AccountsID == AccId
                           select new
                           {
                               a.Name,
                               a.Alias,
                               a.OpnBalance,
                               a.OpnBalanceCr
                           }).FirstOrDefault();
            vmodel.MainAccount = (AccId != -1) ? Account.Name : Group.Name;
            vmodel.MainAccountID = AccId;
            vmodel.from = from;
            vmodel.to = to;
            // Transactions


            var ProperytReg = (from a in db.AccountsTransactions
                               join b in db.Accountss on a.Account equals b.AccountsID
                               join c in db.PropertyRegistrations on a.reference equals c.RegistrationID


                               where (fromdate == null || EF.Functions.DateDiffDay(c.CreatedDate, fdate) <= 0) &&
                               (todate == null || EF.Functions.DateDiffDay(c.CreatedDate, tdate) >= 0) &&
                               (a.Account == AccId && a.Purpose == "RegistrationDeposit")
                               && (pdc == true || a.Status == null)
                               select new
                               {
                                   id = c.RegistrationID,
                                   particulars = b.Name,
                                   a.Project,
                                   Date = (DateTime?)c.CreatedDate,
                                   Invoice = c.VoucherNo,
                                   Type = "RegistrationDeposit",
                                   RAccount = "",
                                   RAccountID = b.AccountsID,
                                   Debit = (decimal?)a.Debit,
                                   Credit = (decimal?)a.Credit,
                                   entry = (DateTime?)a.CreatedDate,
                                   Remark = "RegistrationDeposit",
                                   Amount = c.Amount,
                                   TRN = "",
                                   TransactionId = a.Id,
                                   Account = a.Account,
                                   reference = a.reference
                               });



            var tenancyReg = (from a in db.AccountsTransactions
                              join b in db.Accountss on a.Account equals b.AccountsID
                              join c in db.TenancyContracts on a.reference equals c.Id


                              where (fromdate == null || EF.Functions.DateDiffDay(c.CreatedDate, fdate) <= 0) &&
                              (todate == null || EF.Functions.DateDiffDay(c.CreatedDate, tdate) >= 0) &&
                              (a.Account == AccId && a.Purpose.Contains("Tenancy"))
                              && (pdc == true || a.Status == null)
                              select new
                              {
                                  id = c.Id,
                                  particulars = b.Name,
                                  a.Project,
                                  Date = (DateTime?)c.CreatedDate,
                                  Invoice = c.Id.ToString(),
                                  Type = "TenancyContract ",
                                  RAccount = "",
                                  RAccountID = b.AccountsID,
                                  Debit = (decimal?)a.Debit,
                                  Credit = (decimal?)a.Credit,
                                  entry = (DateTime?)a.CreatedDate,
                                  Remark = "TenancyContract",
                                  Amount = (decimal)c.Rent + (decimal)c.Deposit,
                                  TRN = "",
                                  TransactionId = a.Id,
                                  Account = a.Account,
                                  reference = a.reference
                              });




            var maintanacerpt = (from a in db.AccountsTransactions
                                 join b in db.Accountss on a.Account equals b.AccountsID
                                 join c in db.Maintenances on a.reference equals c.ID


                                 where (fromdate == null || EF.Functions.DateDiffDay(c.CreatedDate, fdate) <= 0) &&
                                 (todate == null || EF.Functions.DateDiffDay(c.CreatedDate, tdate) >= 0) &&
                                 (a.Account == AccId && a.Purpose.Contains("Maintenance"))
                                 && (pdc == true || a.Status == null)
                                 select new
                                 {
                                     id = c.ID,
                                     particulars = b.Name,
                                     a.Project,
                                     Date = (DateTime?)c.CreatedDate,
                                     Invoice = c.ID.ToString(),
                                     Type = "Maintenance ",
                                     RAccount = "",
                                     RAccountID = b.AccountsID,
                                     Debit = (decimal?)a.Debit,
                                     Credit = (decimal?)a.Credit,
                                     entry = (DateTime?)a.CreatedDate,
                                     Remark = "Maintenance",
                                     Amount = c.Amount,
                                     TRN = "",
                                     TransactionId = a.Id,
                                     Account = a.Account,
                                     reference = a.reference
                                 });

            var Asset = (from a in db.AccountsTransactions
                         join b in db.Accountss on a.Account equals b.AccountsID

                         where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                         (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                         (a.Account == AccId && a.Purpose.Contains("Asset From Inventory"))
                         && (pdc == true || a.Status == null)
                         select new
                         {
                             id = a.Id,
                             particulars = b.Name,
                             Project = a.Project,
                             Date = (DateTime?)a.Date,
                             Invoice = a.reference.ToString(),
                             Type = "Asset From Inventory ",
                             RAccount = (b.AccountsID == a.Account) ? b.Name : "Asset From Inventory",
                             RAccountID = b.AccountsID,
                             Debit = (decimal?)a.Debit,
                             Credit = (decimal?)a.Credit,
                             entry = (DateTime?)a.CreatedDate,
                             Remark = "Asset From Inventory",
                             Amount = a.Debit,//db.AssetTransferDetails.Where(z => z.AssetEntryId == a.reference && c.AssetAccountId == b.AccountsID).Select(z => z.TotalPrice).Sum(),

                             TRN = "",
                             TransactionId = a.Id,
                             Account = a.Account,
                             reference = a.reference
                         });

            var AssetPurchase = (from a in db.AccountsTransactions
                                 join b in db.Accountss on a.Account equals b.AccountsID
                                 join c in db.AssetTransferDetails on a.reference equals c.AssetEntryId
                                 join d in db.AssetTransferMasters on c.AssetEntryId equals d.AssetEntryId

                                 where (fromdate == null || EF.Functions.DateDiffDay(d.AssetEntryDate, fdate) <= 0) &&
                                 (todate == null || EF.Functions.DateDiffDay(d.AssetEntryDate, tdate) >= 0) &&
                                 (a.Account == AccId && a.Purpose == "AssetPurchase")
                                 && (pdc == true || a.Status == null)
                                 select new
                                 {
                                     id = c.AssetEntryId,
                                     particulars = b.Name,
                                     Project = a.Project,
                                     Date = (DateTime?)d.AssetEntryDate,
                                     Invoice = c.AssetEntryId.ToString(),
                                     Type = "AssetPurchase",
                                     RAccount = (b.AccountsID == a.Account) ? b.Name : "AssetPurchase",
                                     RAccountID = b.AccountsID,
                                     Debit = (decimal?)a.Debit,
                                     Credit = (decimal?)a.Credit,
                                     entry = (DateTime?)a.CreatedDate,
                                     Remark = "AssetPurchase",
                                     Amount = db.AssetTransferDetails.Where(z => z.AssetEntryId == a.reference && c.AssetAccountId == b.AccountsID).Select(z => z.TotalPrice).Sum(),
                                     TRN = "",
                                     TransactionId = a.Id,
                                     Account = a.Account,
                                     reference = a.reference,

                                 });

            //Asset To Inventory
            var AssetToInventory = (from a in db.AccountsTransactions
                                    join b in db.Accountss on a.Account equals b.AccountsID
                                    join c in db.AssetToInventoryMasters on a.reference equals c.EntryId
                                    where (fromdate == null || EF.Functions.DateDiffDay(c.EntryDate, fdate) <= 0) &&
                                    (todate == null || EF.Functions.DateDiffDay(c.EntryDate, tdate) >= 0) &&
                                    (a.Account == AccId && a.Purpose.Contains("Asset To Inventory"))
                                    && (pdc == true || a.Status == null)
                                    select new
                                    {
                                        id = c.EntryId,
                                        particulars = b.Name,
                                        Project = a.Project,
                                        Date = (DateTime?)c.EntryDate,
                                        Invoice = c.EntryNo.ToString(),
                                        Type = "Asset To Inventory ",
                                        RAccount = (b.AccountsID == a.Account) ? b.Name : "Asset To Inventory",
                                        RAccountID = b.AccountsID,
                                        Debit = (decimal?)a.Debit,
                                        Credit = (decimal?)a.Credit,
                                        entry = (DateTime?)a.CreatedDate,
                                        Remark = "Asset To Inventory",
                                        Amount = c.TotalAmount,
                                        TRN = "",

                                        TransactionId = a.Id,
                                        Account = a.Account,
                                        reference = a.reference
                                    });


            var Reciept = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Receipts on a.reference equals c.ReceiptId
                           join d in db.PDCs on new { f1 = c.ReceiptId, f2 = "Receipt" } equals new { f1 = d.Reference, f2 = d.PDCType } into pdcs
                           from d in pdcs.DefaultIfEmpty()
                           let bb = db.Accountss.Where(at => (c.PayFrom != a.Account && at.AccountsID == c.PayFrom) || (c.PayFrom == a.Account && at.AccountsID == c.PayTo)).FirstOrDefault()
                           where (d.PDCType == "Receipt" && fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Receipt" || a.Purpose == "Discount Allowed")
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.ReceiptId,
                               particulars = b.Name,
                               Project = a.Project,
                               Date = (a.Status == null) ? (DateTime?)a.Date : c.Date,
                               Invoice = c.VoucherNo,
                               Type = a.Purpose,

                               RAccount = bb.Name,
                               RAccountID = bb.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = c.Remark + ((d.PDCDate != null) ? " Pdc Date :" + d.PDCDate : ""),
                               Amount = c.GrandTotal - (decimal)c.Discount - ((decimal)a.Debit == 0 ? (decimal)a.Credit : (decimal)a.Debit),
                               TRN = bb.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            var Payment = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Payments on a.reference equals c.PaymentId
                           join d in db.PDCs on new { f1 = c.PaymentId, f2 = "Payment" } equals new { f1 = d.Reference, f2 = d.PDCType } into pdcs
                           from d in pdcs.DefaultIfEmpty()
                           let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
                           where (d.PDCType == "Payment" && fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Payment" || a.Purpose == "Expense Payment" || a.Purpose == "Discount Received")
                           && (pdc == true || a.Status == null)
                           && a.Purpose == "Payment"
                           select new
                           {
                               id = c.PaymentId,
                               particulars = b.Name,
                               Project = a.Project,
                               Date = (a.Status == null) ? (DateTime?)a.Date : c.Date,
                               Invoice = c.VoucherNo,
                               Type = a.Purpose,
                               RAccount = bb.Name,
                               RAccountID = bb.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = c.Remark + ((d.PDCDate != null) ? " Pdc Date :" + d.PDCDate : ""),
                               Amount = c.GrandTotal - c.Discount - ((decimal)a.Debit == 0 ? (decimal)a.Credit : (decimal)a.Debit),
                               TRN = bb.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            //var RecieptDiscount = (from a in db.AccountsTransactions
            //                       join b in db.Accountss on a.Account equals b.AccountsID
            //                       join c in db.Receipts on a.reference equals c.ReceiptId
            //                       let bb = db.Accountss.Where(at => (c.PayFrom != a.Account && at.AccountsID == c.PayFrom) || (c.PayFrom == a.Account && at.AccountsID == c.PayTo)).FirstOrDefault()
            //                       where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
            //                       (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
            //                       (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Discount Allowed")
            //                       && (pdc == true || a.Status == null)
            //                       select new
            //                       {
            //                           id = c.ReceiptId,
            //                           particulars = b.Name,
            //                           Date = (DateTime?)a.Date,
            //                           Invoice = c.VoucherNo,
            //                           Type = a.Purpose,
            //                           RAccount = bb.Name,
            //                           RAccountID = bb.AccountsID,
            //                           Debit = (decimal?)a.Debit,
            //                           Credit = (decimal?)a.Credit,
            //                           entry = (DateTime?)a.CreatedDate,
            //                           Remark = c.Remark
            //                       });
            //var PaymentDiscount = (from a in db.AccountsTransactions
            //                       join b in db.Accountss on a.Account equals b.AccountsID
            //                       join c in db.Payments on a.reference equals c.PaymentId
            //                       let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
            //                       where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
            //                       (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
            //                       (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Discount Recieve")
            //                       && (pdc == true || a.Status == null)
            //                       select new
            //                       {
            //                           id = c.PaymentId,
            //                           particulars = b.Name,
            //                           Date = (DateTime?)a.Date,
            //                           Invoice = c.VoucherNo,
            //                           Type = a.Purpose,
            //                           RAccount = bb.Name,
            //                           RAccountID = bb.AccountsID,
            //                           Debit = (decimal?)a.Debit,
            //                           Credit = (decimal?)a.Credit,
            //                           entry = (DateTime?)a.CreatedDate,
            //                           Remark = c.Remark
            //                       });

            var Sale = (from a in db.AccountsTransactions
                        join b in db.Accountss on a.Account equals b.AccountsID
                        join c in db.SalesEntrys on a.reference equals c.SalesEntryId
                        join d in db.Customers on c.Customer equals d.CustomerID
                        let bb = db.Receipts.Where(at => a.Purpose == "Sale Payment" && at.editable == choice.No && at.Reference == c.SalesEntryId && at.Remark == "Direct Reciept From Sale Entry").Join(db.Accountss, f1 => f1.PayTo, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                        let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()

                        where (fromdate == null || EF.Functions.DateDiffDay(c.SEDate, fdate) <= 0) &&
                        (todate == null || EF.Functions.DateDiffDay(c.SEDate, tdate) >= 0) &&
                        (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Sale" || a.Purpose == "Sale Payment")
                        && (pdc == true || a.Status == null)
                        select new
                        {
                            id = c.SalesEntryId,
                            particulars = b.Name,
                            Project = a.Project,
                            Date = (DateTime?)c.SEDate,
                            Invoice = (a.Purpose != "Sale Payment" || (d.Accounts != a.Account)) ? c.BillNo : "",
                            Type = (a.Purpose != "Sale Payment") ? a.Purpose : (d.Accounts != a.Account) ? "Sale" : "",
                            RAccount = (d.Accounts == a.Account) ? ((a.Purpose != "Sale Payment") ? "Sale" : bb.Name) : d.CustomerName,
                            RAccountID = b.AccountsID,
                            Debit = (decimal?)0,
                            Credit = (decimal?)(decimal)((c.SEGrandTotal / (decimal)1.05)*5/100),
                            entry = (DateTime?)a.CreatedDate,
                            Remark = (a.Purpose != "Sale Payment") ? c.Remarks : "",
                            Amount = (c.SalesType != 3 && a.Account != 502) ? (decimal)(c.SEGrandTotal / (decimal)1.05 ): (decimal)(c.SEGrandTotal/(decimal)1.05 ),
                            TRN = AC.TRN,
                            TransactionId = a.Id,
                            Account = a.Account,
                            reference = a.reference
                        });
            var SReturn = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.SalesReturns on a.reference equals c.SalesReturnId
                           join d in db.Customers on c.Customer equals d.CustomerID
                           let bb = db.Payments.Where(at => a.Purpose == "Sale Return Payment" && at.editable == choice.No && at.Reference == c.SalesReturnId && at.RefType == "SalesReturn").Join(db.Accountss, f1 => f1.PayFrom, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                           let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                           where (fromdate == null || EF.Functions.DateDiffDay(c.SRDate, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(c.SRDate, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Sale Return" || a.Purpose == "Sale Return Payment")
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.SalesReturnId,
                               particulars = b.Name,
                               Project = a.Project,
                               Date = (DateTime?)c.SRDate,
                               Invoice = a.Purpose != "Sale Return Payment" ? c.BillNo : "",
                               Type = a.Purpose != "Sale Return Payment" ? a.Purpose : "",
                               RAccount = (d.Accounts == a.Account) ? ((a.Purpose != "Sale Return Payment") ? "Sales Return" : bb.Name) : d.CustomerName,
                               RAccountID = b.AccountsID,

                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = a.Purpose != "Sale Return Payment" ? c.Remarks : "",
                               Amount = c.SRSubTotal - c.SRDiscount,
                               TRN = AC.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference,

                           });
            var Purchase = (from a in db.AccountsTransactions
                            join b in db.Accountss on a.Account equals b.AccountsID
                            join c in db.PurchaseEntrys on a.reference equals c.PurchaseEntryId
                            join d in db.Suppliers on c.Supplier equals d.SupplierID

                            let sundry = (from g in db.AccountsTransactions

                                          join i in db.PurchaseEntrys on g.reference equals i.PurchaseEntryId
                                          join j in db.PEBillSundrys on i.PurchaseEntryId equals j.PurchaseEntry
                                          join k in db.BillSundrys on j.BillSundry equals k.BillSundryId
                                          where g.reference == c.PurchaseEntryId && g.Purpose == "Purchase" && a.Type == 0
                                          select new
                                          {
                                              k.BSName
                                          }).FirstOrDefault().BSName
                            let bb = db.Payments.Where(at => a.Purpose == "Purchase Payment" && at.editable == choice.No && at.Reference == c.PurchaseEntryId && at.RefType == "Purchase").Join(db.Accountss, f1 => f1.PayFrom, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                            let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                            where (fromdate == "" || EF.Functions.DateDiffDay(c.PEDate, fdate) <= 0) &&
                            (todate == "" || EF.Functions.DateDiffDay(c.PEDate, tdate) >= 0) &&
                            (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Purchase" || a.Purpose == "Purchase Payment")
                            && (pdc == true || a.Status == null)
                            select new
                            {
                                id = c.PurchaseEntryId,
                                particulars = sundry,
                                Project = a.Project,
                                Date = (DateTime?)c.PEDate,
                                Invoice = a.Purpose == "Purchase Payment" ? "" : c.BillNo,
                                Type = a.Purpose == "Purchase Payment" ? "" : (sundry == null) ? a.Purpose : sundry,
                                RAccount = (d.Accounts == a.Account) ? ((a.Purpose != "Purchase Payment") ? "Purchase" : bb.Name) : d.SupplierName,
                                RAccountID = b.AccountsID,
                                Debit = (decimal?)a.Debit,
                                Credit = (decimal?)a.Credit,
                                entry = (DateTime?)a.CreatedDate,
                                Remark = a.Purpose == "Purchase Payment" ? "" : c.Remarks,
                                Amount = c.PESubTotal - c.PEDiscount,
                                TRN = AC.TRN,
                                TransactionId = a.Id,
                                Account = a.Account,
                                reference = a.reference

                            });
            var PReturn = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.PurchaseReturns on a.reference equals c.PurchaseReturnId
                           join d in db.Suppliers on c.Supplier equals d.SupplierID
                           let bb = db.Receipts.Where(at => a.Purpose == "Purchase Return Payment" && at.editable == choice.No && at.Reference == c.PurchaseReturnId && at.Remark == "Receipt From Purchase Return").Join(db.Accountss, f1 => f1.PayTo, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                           let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                           where (fromdate == "" || EF.Functions.DateDiffDay(c.PRDate, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(c.PRDate, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Purchase Return" || a.Purpose == "Purchase Return Payment")
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.PurchaseReturnId,
                               particulars = b.Name,
                               Project = a.Project,
                               Date = (DateTime?)c.PRDate,
                               Invoice = a.Purpose == "Purchase Return Payment" ? "" : c.BillNo,
                               Type = a.Purpose == "Purchase Return Payment" ? "" : a.Purpose,
                               RAccount = (d.Accounts == a.Account) ? "Purchase" : d.SupplierName,
                               RAccountID = b.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = a.Purpose == "Purchase Return Payment" ? "" : c.Remarks,
                               Amount = c.PRSubTotal - c.PRDiscount,
                               TRN = AC.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            var Journal = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Journals on a.reference equals c.JournalId
                           //let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
                           join d in db.PDCs on new { f1 = c.JournalId, f2 = "Journal" } equals new { f1 = d.Reference, f2 = d.PDCType } into pdcs
                           from d in pdcs.DefaultIfEmpty()
                           let bd = db.Accountss.Where(at => (a.Type == DC.Credit && at.AccountsID == c.PayTo) || (a.Type == DC.Debit && at.AccountsID == c.PayFrom)).FirstOrDefault()
                           let acCount = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && z.Account == a.Account && z.Type == a.Type).Select(z => a.Account).Count()
                           let prev = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && a.Type != z.Type && (a.Type == DC.Credit && a.Id > z.Id) && z.Account != 501 && z.Account != 502).OrderByDescending(z => z.Id).Select(z => z.Account).FirstOrDefault()
                           let next = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && a.Type != z.Type && (a.Type == DC.Debit && a.Id < z.Id) && z.Account != 501 && z.Account != 502).OrderBy(z => z.Id).Select(z => z.Account).FirstOrDefault()
                           where (d.PDCType == "Journal" && fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && a.Purpose == "Journal"
                           && (pdc == true || a.Status == null) &&
                           a.Purpose == "Journal"

                           select new
                           {
                               id = c.JournalId,
                               particulars = b.Name,
                               Project = a.Project,
                               Date = (DateTime?)a.Date,
                               Invoice = c.VoucherNo,
                               Type = "Journal Entry",

                               RAccount = (acCount <= 1) ? bd.Name : db.Accountss.Where(y => (a.Type == DC.Credit && y.AccountsID == prev) || (a.Type == DC.Debit && y.AccountsID == next)).Select(y => y.Name).FirstOrDefault(),
                               RAccountID = bd.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = a.Narration + " " + c.Remark + ((d.PDCDate != null) ? " Pdc Date :" + d.PDCDate : ""),
                               Amount = (decimal)c.GrandTotal,
                               TRN = bd.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            var Contra = (from a in db.AccountsTransactions
                          join b in db.Accountss on a.Account equals b.AccountsID
                          join c in db.ContraVouchers on a.reference equals c.ContraVoucherId
                          let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
                          where (fromdate == null || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) &&
                          (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) &&
                          (todate == null || EF.Functions.DateDiffDay(c.Date, tdate) >= 0) && a.Purpose == "ContraVoucher"
                          && (pdc == true || a.Status == null)
                          select new
                          {
                              id = c.ContraVoucherId,
                              particulars = b.Name,
                              Project = a.Project,
                              Date = (DateTime?)a.Date,
                              Invoice = c.VoucherNo,
                              Type = "Contra Voucher",

                              RAccount = bb.Name,
                              RAccountID = bb.AccountsID,
                              Debit = (decimal?)a.Debit,
                              Credit = (decimal?)a.Credit,
                              entry = (DateTime?)a.CreatedDate,
                              Remark = c.Remark,
                              Amount = c.Amount,
                              TRN = bb.TRN,
                              TransactionId = a.Id,
                              Account = a.Account,
                              reference = a.reference

                          });
            var StockAdjustment = (from a in db.AccountsTransactions
                                   join b in db.Accountss on a.Account equals b.AccountsID
                                   join c in db.StockAdjustments on a.reference equals c.StockAdjustmentId
                                   where (fromdate == null || EF.Functions.DateDiffDay(c.AdjDate, fdate) <= 0) &&
                                   (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) &&
                                   (todate == null || EF.Functions.DateDiffDay(c.AdjDate, tdate) >= 0) && a.Purpose == "Stock Adjustment"
                                   select new
                                   {
                                       id = c.StockAdjustmentId,
                                       particulars = b.Name,
                                       Project = a.Project,
                                       Date = (DateTime?)a.Date,
                                       Invoice = c.VoucherNo,
                                       Type = "Contra Voucher",

                                       RAccount = b.Name,
                                       RAccountID = b.AccountsID,
                                       Debit = (decimal?)a.Debit,
                                       Credit = (decimal?)a.Credit,
                                       entry = (DateTime?)a.CreatedDate,
                                       Remark = c.Reason,
                                       Amount = c.PurchaseRate,
                                       TRN = b.TRN,
                                       TransactionId = a.Id,
                                       Account = a.Account,
                                       reference = a.reference
                                   });
            decimal dumy = 100;
            var payroll = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && a.Purpose == "Payroll Voucher"
                           select new
                           {
                               id = a.Id,
                               particulars = b.Name,
                               Project = a.Project,
                               Date = (DateTime?)a.Date,
                               Invoice = a.reference.ToString(),
                               Type = "Payroll Voucher",

                               RAccount = b.Name,
                               RAccountID = b.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = "Payroll",
                               Amount = dumy,
                               TRN = b.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });

            var rent = (from a in db.AccountsTransactions
                        join b in db.Accountss on a.Account equals b.AccountsID
                        where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                        (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) &&
                        (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && a.Purpose == "Rent Receivable"
                        select new
                        {
                            id = a.Id,
                            particulars = b.Name,
                            Project = a.Project,
                            Date = (DateTime?)a.Date,
                            Invoice = a.reference.ToString(),
                            Type = "Rent Receivable",

                            RAccount = b.Name,
                            RAccountID = b.AccountsID,
                            Debit = (decimal?)a.Debit,
                            Credit = (decimal?)a.Credit,
                            entry = (DateTime?)a.CreatedDate,
                            Remark = "Rent",
                            Amount = (decimal)a.Credit,
                            TRN = b.TRN,
                            TransactionId = a.Id,
                            Account = a.Account,
                            reference = a.reference
                        });
            var full = Payment.Union(Reciept);
            // var disc = PaymentDiscount.Union(RecieptDiscount);

            var pur = Purchase.Union(PReturn);
            var sal = Sale.Union(SReturn);
            var joc = Journal.Union(Contra);

            // full = full.Union(disc);
            full = full.Union(pur);
            full = full.Union(payroll);
            full = full.Union(sal);
            full = full.Union(joc);
            full = full.Union(StockAdjustment);
            full = full.Union(ProperytReg);
            full = full.Union(tenancyReg);
            full = full.Union(maintanacerpt);
            full = full.Union(Asset);
            full = full.Union(AssetPurchase);
            full = full.Union(AssetToInventory);
            //full = full.Union(RecieptDiscount);
            full = full.Union(rent);
            full = full.AsQueryable().OrderBy("Date asc, entry asc");
            vmodel.Ledger = (from a in full
                             select new Ledger
                             {
                                 Date = a.Date,
                                 Invoice = a.Invoice,
                                 Type = a.Type,
                                 RAccount = a.RAccount,
                                 RAccountID = a.RAccountID,
                                 Debit = a.Debit,
                                 Credit = a.Credit,
                                 particulars = a.particulars,
                                 Remark = a.Remark,
                                 Amount = a.Amount,
                                 MainId = a.id,
                                 TRN = a.TRN,
                                 TransactionId = a.TransactionId,
                                 Account = a.Account,
                                 Reference = a.reference,
                                 projectname = db.Projects.Where(o => o.ProjectId == a.Project).Select(o => o.ProjectName).FirstOrDefault()
                             }).ToList();
            return vmodel;
        }
        #endregion

        public decimal? GetDebit(long AccId, string fromdate, string todate, long? AccGroup, bool? pdc)
        {

            LedgerViewModel vmodel = new LedgerViewModel();
            String format = "dd-MM-yyyy";
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? from = null;
            DateTime? to = null;
            Dictionary<string, object> Balance = null;
            long[] Accounts = { };
            if (fromdate != "")
            {
                //  fdate = DateTime.Parse(fromdate.ToString(), new CultureInfo("en-GB"));
                fdate = DateTime.ParseExact(fromdate, format, new CultureInfo("en-GB"));
                from = fdate;
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate.ToString(), new CultureInfo("en-GB"));
                to = tdate;
            }
            if (AccId == -1)
            {
                Accounts = AllAccounts(AccGroup);
            }
            else
            {
                Array.Resize(ref Accounts, Accounts.Length + 1);
                Accounts[0] = AccId;
            }
            Balance = OpenBlnc(AccId, (DateTime)fdate, pdc, Accounts);

            if ((string)Balance["type"] == "Cr")
            {
                vmodel.OpeningBalance = (decimal)Balance["amount"];
                vmodel.blnceType = (string)Balance["type"];
            }
            else
            {
                vmodel.OpeningBalance = (0 - (decimal)Balance["amount"]);
                vmodel.blnceType = (string)Balance["type"];

            }
            decimal? sum = vmodel.OpeningBalance;
            var Group = (from a in db.AccountsGroups
                         where a.AccountsGroupID == AccGroup
                         select new
                         {
                             a.Name,
                             a.Alias
                         }).FirstOrDefault();
            var Account = (from a in db.Accountss
                           where a.AccountsID == AccId
                           select new
                           {
                               a.Name,
                               a.Alias,
                               a.OpnBalance,
                               a.OpnBalanceCr
                           }).FirstOrDefault();
            vmodel.MainAccount = (AccId != -1) ? Account.Name : Group.Name;
            vmodel.MainAccountID = AccId;
            vmodel.from = from;
            vmodel.to = to;
            // Transactions


            var ProperytReg = (from a in db.AccountsTransactions
                               join b in db.Accountss on a.Account equals b.AccountsID
                               join c in db.PropertyRegistrations on a.reference equals c.RegistrationID


                               where (fromdate == null || EF.Functions.DateDiffDay(c.CreatedDate, fdate) <= 0) &&
                               (todate == null || EF.Functions.DateDiffDay(c.CreatedDate, tdate) >= 0) &&
                               (a.Account == AccId && a.Purpose == "RegistrationDeposit")
                               && (pdc == true || a.Status == null)
                               select new
                               {
                                   id = c.RegistrationID,
                                   particulars = b.Name,

                                   Date = (DateTime?)c.CreatedDate,
                                   Invoice = c.VoucherNo,
                                   Type = "RegistrationDeposit",
                                   RAccount = "",
                                   RAccountID = b.AccountsID,
                                   Debit = (decimal?)a.Debit,
                                   Credit = (decimal?)a.Credit,
                                   entry = (DateTime?)a.CreatedDate,
                                   Remark = "RegistrationDeposit",
                                   Amount = c.Amount,
                                   TRN = "",
                                   TransactionId = a.Id,
                                   Account = a.Account,
                                   reference = a.reference
                               });



            var tenancyReg = (from a in db.AccountsTransactions
                              join b in db.Accountss on a.Account equals b.AccountsID
                              join c in db.TenancyContracts on a.reference equals c.Id


                              where (fromdate == null || EF.Functions.DateDiffDay(c.CreatedDate, fdate) <= 0) &&
                              (todate == null || EF.Functions.DateDiffDay(c.CreatedDate, tdate) >= 0) &&
                              (a.Account == AccId && a.Purpose.Contains("Tenancy"))
                              && (pdc == true || a.Status == null)
                              select new
                              {
                                  id = c.Id,
                                  particulars = b.Name,

                                  Date = (DateTime?)c.CreatedDate,
                                  Invoice = c.Id.ToString(),
                                  Type = "TenancyContract ",
                                  RAccount = "",
                                  RAccountID = b.AccountsID,
                                  Debit = (decimal?)a.Debit,
                                  Credit = (decimal?)a.Credit,
                                  entry = (DateTime?)a.CreatedDate,
                                  Remark = "TenancyContract",
                                  Amount = (decimal)c.Rent + (decimal)c.Deposit,
                                  TRN = "",
                                  TransactionId = a.Id,
                                  Account = a.Account,
                                  reference = a.reference
                              });




            var maintanacerpt = (from a in db.AccountsTransactions
                                 join b in db.Accountss on a.Account equals b.AccountsID
                                 join c in db.Maintenances on a.reference equals c.ID


                                 where (fromdate == null || EF.Functions.DateDiffDay(c.CreatedDate, fdate) <= 0) &&
                                 (todate == null || EF.Functions.DateDiffDay(c.CreatedDate, tdate) >= 0) &&
                                 (a.Account == AccId && a.Purpose.Contains("Maintenance"))
                                 && (pdc == true || a.Status == null)
                                 select new
                                 {
                                     id = c.ID,
                                     particulars = b.Name,

                                     Date = (DateTime?)c.CreatedDate,
                                     Invoice = c.ID.ToString(),
                                     Type = "Maintenance ",
                                     RAccount = "",
                                     RAccountID = b.AccountsID,
                                     Debit = (decimal?)a.Debit,
                                     Credit = (decimal?)a.Credit,
                                     entry = (DateTime?)a.CreatedDate,
                                     Remark = "Maintenance",
                                     Amount = c.Amount,
                                     TRN = "",
                                     TransactionId = a.Id,
                                     Account = a.Account,
                                     reference = a.reference
                                 });

            var Asset = (from a in db.AccountsTransactions
                         join b in db.Accountss on a.Account equals b.AccountsID
                         join c in db.AssetTransferDetails on a.reference equals c.AssetEntryId
                         join d in db.AssetTransferMasters on c.AssetEntryId equals d.AssetEntryId
                         where (fromdate == null || EF.Functions.DateDiffDay(d.AssetEntryDate, fdate) <= 0) &&
                         (todate == null || EF.Functions.DateDiffDay(d.AssetEntryDate, tdate) >= 0) &&
                          (a.Account == c.AssetAccountId) &&
                         (a.Account == AccId && a.Purpose.Contains("Asset From Inventory"))
                         && (pdc == true || a.Status == null)
                         select new
                         {
                             id = c.AssetEntryId,
                             particulars = b.Name,
                             Date = (DateTime?)d.AssetEntryDate,
                             Invoice = c.AssetEntryId.ToString(),
                             Type = "Asset From Inventory ",
                             RAccount = (b.AccountsID == a.Account) ? b.Name : "Asset From Inventory",
                             RAccountID = b.AccountsID,
                             Debit = (decimal?)a.Debit,
                             Credit = (decimal?)a.Credit,
                             entry = (DateTime?)a.CreatedDate,
                             Remark = "Asset From Inventory",
                             Amount = db.AssetTransferDetails.Where(z => z.AssetEntryId == a.reference && c.AssetAccountId == b.AccountsID).Select(z => z.TotalPrice).Sum(),

                             TRN = "",
                             TransactionId = a.Id,
                             Account = a.Account,
                             reference = a.reference
                         });

            var AssetPurchase = (from a in db.AccountsTransactions
                                 join b in db.Accountss on a.Account equals b.AccountsID
                                 join c in db.AssetTransferDetails on a.reference equals c.AssetEntryId
                                 join d in db.AssetTransferMasters on c.AssetEntryId equals d.AssetEntryId

                                 where (fromdate == null || EF.Functions.DateDiffDay(d.AssetEntryDate, fdate) <= 0) &&
                                 (todate == null || EF.Functions.DateDiffDay(d.AssetEntryDate, tdate) >= 0) &&
                                 (a.Account == AccId && a.Purpose == "AssetPurchase")
                                 && (pdc == true || a.Status == null)
                                 select new
                                 {
                                     id = c.AssetEntryId,
                                     particulars = b.Name,
                                     Date = (DateTime?)d.AssetEntryDate,
                                     Invoice = c.AssetEntryId.ToString(),
                                     Type = "AssetPurchase",
                                     RAccount = (b.AccountsID == a.Account) ? b.Name : "AssetPurchase",
                                     RAccountID = b.AccountsID,
                                     Debit = (decimal?)a.Debit,
                                     Credit = (decimal?)a.Credit,
                                     entry = (DateTime?)a.CreatedDate,
                                     Remark = "AssetPurchase",
                                     Amount = db.AssetTransferDetails.Where(z => z.AssetEntryId == a.reference && c.AssetAccountId == b.AccountsID).Select(z => z.TotalPrice).Sum(),
                                     TRN = "",
                                     TransactionId = a.Id,
                                     Account = a.Account,
                                     reference = a.reference
                                 });

            //Asset To Inventory
            var AssetToInventory = (from a in db.AccountsTransactions
                                    join b in db.Accountss on a.Account equals b.AccountsID
                                    join c in db.AssetToInventoryMasters on a.reference equals c.EntryId
                                    where (fromdate == null || EF.Functions.DateDiffDay(c.EntryDate, fdate) <= 0) &&
                                    (todate == null || EF.Functions.DateDiffDay(c.EntryDate, tdate) >= 0) &&
                                    (a.Account == AccId && a.Purpose.Contains("Asset To Inventory"))
                                    && (pdc == true || a.Status == null)
                                    select new
                                    {
                                        id = c.EntryId,
                                        particulars = b.Name,
                                        Date = (DateTime?)c.EntryDate,
                                        Invoice = c.EntryNo.ToString(),
                                        Type = "Asset To Inventory ",
                                        RAccount = (b.AccountsID == a.Account) ? b.Name : "Asset To Inventory",
                                        RAccountID = b.AccountsID,
                                        Debit = (decimal?)a.Debit,
                                        Credit = (decimal?)a.Credit,
                                        entry = (DateTime?)a.CreatedDate,
                                        Remark = "Asset To Inventory",
                                        Amount = c.TotalAmount,
                                        TRN = "",
                                        TransactionId = a.Id,
                                        Account = a.Account,
                                        reference = a.reference
                                    });


            var Reciept = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Receipts on a.reference equals c.ReceiptId
                           join d in db.PDCs on new { f1 = c.ReceiptId, f2 = "Receipt" } equals new { f1 = d.Reference, f2 = d.PDCType } into pdcs
                           from d in pdcs.DefaultIfEmpty()
                           let bb = db.Accountss.Where(at => (c.PayFrom != a.Account && at.AccountsID == c.PayFrom) || (c.PayFrom == a.Account && at.AccountsID == c.PayTo)).FirstOrDefault()
                           where (d.PDCType == "Receipt" && fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Receipt" || a.Purpose == "Discount Allowed")
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.ReceiptId,
                               particulars = b.Name,
                               Date = (a.Status == null) ? (DateTime?)a.Date : c.Date,
                               Invoice = c.VoucherNo,
                             
                               Type = a.Purpose,
                               RAccount = bb.Name,
                               RAccountID = bb.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = c.Remark + ((d.PDCDate != null) ? " Pdc Date :" + d.PDCDate : ""),
                               Amount = c.GrandTotal - (decimal)c.Discount - ((decimal)a.Debit == 0 ? (decimal)a.Credit : (decimal)a.Debit),
                               TRN = bb.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            var Payment = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Payments on a.reference equals c.PaymentId
                           join d in db.PDCs on new { f1 = c.PaymentId, f2 = "Payment" } equals new { f1 = d.Reference, f2 = d.PDCType } into pdcs
                           from d in pdcs.DefaultIfEmpty()
                           let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
                           where (d.PDCType == "Payment" && fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Payment" || a.Purpose == "Expense Payment" || a.Purpose == "Discount Received")
                           && (pdc == true || a.Status == null)
                           && a.Purpose == "Payment"
                           select new
                           {
                               id = c.PaymentId,
                               particulars = b.Name,

                               Date = (a.Status == null) ? (DateTime?)a.Date : c.Date,
                               Invoice = c.VoucherNo,
                               Type = a.Purpose,
                               RAccount = bb.Name,
                               RAccountID = bb.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = c.Remark + ((d.PDCDate != null) ? " Pdc Date :" + d.PDCDate : ""),
                               Amount = c.GrandTotal - c.Discount - ((decimal)a.Debit == 0 ? (decimal)a.Credit : (decimal)a.Debit),
                               TRN = bb.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            //var RecieptDiscount = (from a in db.AccountsTransactions
            //                       join b in db.Accountss on a.Account equals b.AccountsID
            //                       join c in db.Receipts on a.reference equals c.ReceiptId
            //                       let bb = db.Accountss.Where(at => (c.PayFrom != a.Account && at.AccountsID == c.PayFrom) || (c.PayFrom == a.Account && at.AccountsID == c.PayTo)).FirstOrDefault()
            //                       where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
            //                       (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
            //                       (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Discount Allowed")
            //                       && (pdc == true || a.Status == null)
            //                       select new
            //                       {
            //                           id = c.ReceiptId,
            //                           particulars = b.Name,
            //                           Date = (DateTime?)a.Date,
            //                           Invoice = c.VoucherNo,
            //                           Type = a.Purpose,
            //                           RAccount = bb.Name,
            //                           RAccountID = bb.AccountsID,
            //                           Debit = (decimal?)a.Debit,
            //                           Credit = (decimal?)a.Credit,
            //                           entry = (DateTime?)a.CreatedDate,
            //                           Remark = c.Remark
            //                       });
            //var PaymentDiscount = (from a in db.AccountsTransactions
            //                       join b in db.Accountss on a.Account equals b.AccountsID
            //                       join c in db.Payments on a.reference equals c.PaymentId
            //                       let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
            //                       where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
            //                       (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
            //                       (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Discount Recieve")
            //                       && (pdc == true || a.Status == null)
            //                       select new
            //                       {
            //                           id = c.PaymentId,
            //                           particulars = b.Name,
            //                           Date = (DateTime?)a.Date,
            //                           Invoice = c.VoucherNo,
            //                           Type = a.Purpose,
            //                           RAccount = bb.Name,
            //                           RAccountID = bb.AccountsID,
            //                           Debit = (decimal?)a.Debit,
            //                           Credit = (decimal?)a.Credit,
            //                           entry = (DateTime?)a.CreatedDate,
            //                           Remark = c.Remark
            //                       });
            var Sale = (from a in db.AccountsTransactions
                        join b in db.Accountss on a.Account equals b.AccountsID
                        join c in db.SalesEntrys on a.reference equals c.SalesEntryId
                        join d in db.Customers on c.Customer equals d.CustomerID
                        let bb = db.Receipts.Where(at => a.Purpose == "Sale Payment" && at.editable == choice.No && at.Reference == c.SalesEntryId && at.Remark == "Direct Reciept From Sale Entry").Join(db.Accountss, f1 => f1.PayTo, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                        let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()

                        where (fromdate == null || EF.Functions.DateDiffDay(c.SEDate, fdate) <= 0) &&
                        (todate == null || EF.Functions.DateDiffDay(c.SEDate, tdate) >= 0) &&
                        (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Sale" || a.Purpose == "Sale Payment")
                        && (pdc == true || a.Status == null)
                        select new
                        {
                            id = c.SalesEntryId,
                            particulars = b.Name,

                            Date = (DateTime?)c.SEDate,
                            Invoice = (a.Purpose != "Sale Payment" || (d.Accounts != a.Account)) ? c.BillNo : "",
                            Type = (a.Purpose != "Sale Payment") ? a.Purpose : (d.Accounts != a.Account) ? "Sale" : "",
                            RAccount = (d.Accounts == a.Account) ? ((a.Purpose != "Sale Payment") ? "Sale" : bb.Name) : d.CustomerName,
                            RAccountID = b.AccountsID,
                            Debit = (decimal?)a.Debit,
                            Credit = (decimal?)a.Credit,
                            entry = (DateTime?)a.CreatedDate,
                            Remark = (a.Purpose != "Sale Payment") ? c.Remarks : "",
                            Amount = (c.SalesType != 3 && a.Account != 502) ? c.SESubTotal - c.SEDiscount : ((a.Credit / (decimal)5) * 100),
                            TRN = AC.TRN,
                            TransactionId = a.Id,
                            Account = a.Account,
                            reference = a.reference
                        });
            var SReturn = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.SalesReturns on a.reference equals c.SalesReturnId
                           join d in db.Customers on c.Customer equals d.CustomerID
                           let bb = db.Payments.Where(at => a.Purpose == "Sale Return Payment" && at.editable == choice.No && at.Reference == c.SalesReturnId && at.RefType == "SalesReturn").Join(db.Accountss, f1 => f1.PayFrom, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                           let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                           where (fromdate == null || EF.Functions.DateDiffDay(c.SRDate, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(c.SRDate, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Sale Return" || a.Purpose == "Sale Return Payment")
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.SalesReturnId,
                               particulars = b.Name,
                               Date = (DateTime?)c.SRDate,
                               Invoice = a.Purpose != "Sale Return Payment" ? c.BillNo : "",
                               Type = a.Purpose != "Sale Return Payment" ? a.Purpose : "",
                               RAccount = (d.Accounts == a.Account) ? ((a.Purpose != "Sale Return Payment") ? "Sales Return" : bb.Name) : d.CustomerName,
                               RAccountID = b.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = a.Purpose != "Sale Return Payment" ? c.Remarks : "",
                               Amount = c.SRSubTotal - c.SRDiscount,
                               TRN = AC.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            var Purchase = (from a in db.AccountsTransactions
                            join b in db.Accountss on a.Account equals b.AccountsID
                            join c in db.PurchaseEntrys on a.reference equals c.PurchaseEntryId
                            join d in db.Suppliers on c.Supplier equals d.SupplierID

                            let sundry = (from g in db.AccountsTransactions

                                          join i in db.PurchaseEntrys on g.reference equals i.PurchaseEntryId
                                          join j in db.PEBillSundrys on i.PurchaseEntryId equals j.PurchaseEntry
                                          join k in db.BillSundrys on j.BillSundry equals k.BillSundryId
                                          where g.reference == c.PurchaseEntryId && g.Purpose == "Purchase" && a.Type == 0
                                          select new
                                          {
                                              k.BSName
                                          }).FirstOrDefault().BSName
                            let bb = db.Payments.Where(at => a.Purpose == "Purchase Payment" && at.editable == choice.No && at.Reference == c.PurchaseEntryId && at.RefType == "Purchase").Join(db.Accountss, f1 => f1.PayFrom, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                            let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                            where (fromdate == "" || EF.Functions.DateDiffDay(c.PEDate, fdate) <= 0) &&
                            (todate == "" || EF.Functions.DateDiffDay(c.PEDate, tdate) >= 0) &&
                            (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Purchase" || a.Purpose == "Purchase Payment")
                            && (pdc == true || a.Status == null)
                            select new
                            {
                                id = c.PurchaseEntryId,
                                particulars = sundry,

                                Date = (DateTime?)c.PEDate,
                                Invoice = a.Purpose == "Purchase Payment" ? "" : c.BillNo,
                                Type = a.Purpose == "Purchase Payment" ? "" : (sundry == null) ? a.Purpose : sundry,
                                RAccount = (d.Accounts == a.Account) ? ((a.Purpose != "Purchase Payment") ? "Purchase" : bb.Name) : d.SupplierName,
                                RAccountID = b.AccountsID,
                                Debit = (decimal?)a.Debit,
                                Credit = (decimal?)a.Credit,
                                entry = (DateTime?)a.CreatedDate,
                                Remark = a.Purpose == "Purchase Payment" ? "" : c.Remarks,
                                Amount = c.PESubTotal - c.PEDiscount,
                                TRN = AC.TRN,
                                TransactionId = a.Id,
                                Account = a.Account,
                                reference = a.reference

                            });
            var PReturn = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.PurchaseReturns on a.reference equals c.PurchaseReturnId
                           join d in db.Suppliers on c.Supplier equals d.SupplierID
                           let bb = db.Receipts.Where(at => a.Purpose == "Purchase Return Payment" && at.editable == choice.No && at.Reference == c.PurchaseReturnId && at.Remark == "Receipt From Purchase Return").Join(db.Accountss, f1 => f1.PayTo, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                           let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                           where (fromdate == "" || EF.Functions.DateDiffDay(c.PRDate, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(c.PRDate, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Purchase Return" || a.Purpose == "Purchase Return Payment")
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.PurchaseReturnId,
                               particulars = b.Name,
                               Date = (DateTime?)c.PRDate,
                               Invoice = a.Purpose == "Purchase Return Payment" ? "" : c.BillNo,
                               Type = a.Purpose == "Purchase Return Payment" ? "" : a.Purpose,
                               RAccount = (d.Accounts == a.Account) ? "Purchase" : d.SupplierName,
                               RAccountID = b.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = a.Purpose == "Purchase Return Payment" ? "" : c.Remarks,
                               Amount = c.PRSubTotal - c.PRDiscount,
                               TRN = AC.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            var Journal = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Journals on a.reference equals c.JournalId
                           //let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
                           join d in db.PDCs on new { f1 = c.JournalId, f2 = "Journal" } equals new { f1 = d.Reference, f2 = d.PDCType } into pdcs
                           from d in pdcs.DefaultIfEmpty()
                           let bd = db.Accountss.Where(at => (a.Type == DC.Credit && at.AccountsID == c.PayTo) || (a.Type == DC.Debit && at.AccountsID == c.PayFrom)).FirstOrDefault()
                           let acCount = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && z.Account == a.Account && z.Type == a.Type).Select(z => a.Account).Count()
                           let prev = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && a.Type != z.Type && (a.Type == DC.Credit && a.Id > z.Id) && z.Account != 501 && z.Account != 502).OrderByDescending(z => z.Id).Select(z => z.Account).FirstOrDefault()
                           let next = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && a.Type != z.Type && (a.Type == DC.Debit && a.Id < z.Id) && z.Account != 501 && z.Account != 502).OrderBy(z => z.Id).Select(z => z.Account).FirstOrDefault()
                           where (d.PDCType == "Journal" && fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && a.Purpose == "Journal"
                           && (pdc == true || a.Status == null) &&
                           a.Purpose == "Journal"

                           select new
                           {
                               id = c.JournalId,
                               particulars = b.Name,
                               Date = (DateTime?)a.Date,
                               Invoice = c.VoucherNo,
                               Type = "Journal Entry",
                               RAccount = (acCount <= 1) ? bd.Name : db.Accountss.Where(y => (a.Type == DC.Credit && y.AccountsID == prev) || (a.Type == DC.Debit && y.AccountsID == next)).Select(y => y.Name).FirstOrDefault(),
                               RAccountID = bd.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = c.Remark + ((d.PDCDate != null) ? " Pdc Date :" + d.PDCDate : ""),
                               Amount = (decimal)c.GrandTotal,
                               TRN = bd.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            var Contra = (from a in db.AccountsTransactions
                          join b in db.Accountss on a.Account equals b.AccountsID
                          join c in db.ContraVouchers on a.reference equals c.ContraVoucherId
                          let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
                          where (fromdate == null || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) &&
                          (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) &&
                          (todate == null || EF.Functions.DateDiffDay(c.Date, tdate) >= 0) && a.Purpose == "ContraVoucher"
                          && (pdc == true || a.Status == null)
                          select new
                          {
                              id = c.ContraVoucherId,
                              particulars = b.Name,
                              Date = (DateTime?)a.Date,
                              Invoice = c.VoucherNo,
                              Type = "Contra Voucher",
                              RAccount = bb.Name,
                              RAccountID = bb.AccountsID,
                              Debit = (decimal?)a.Debit,
                              Credit = (decimal?)a.Credit,
                              entry = (DateTime?)a.CreatedDate,
                              Remark = c.Remark,
                              Amount = c.Amount,
                              TRN = bb.TRN,
                              TransactionId = a.Id,
                              Account = a.Account,
                              reference = a.reference

                          });
            var StockAdjustment = (from a in db.AccountsTransactions
                                   join b in db.Accountss on a.Account equals b.AccountsID
                                   join c in db.StockAdjustments on a.reference equals c.StockAdjustmentId
                                   where (fromdate == null || EF.Functions.DateDiffDay(c.AdjDate, fdate) <= 0) &&
                                   (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) &&
                                   (todate == null || EF.Functions.DateDiffDay(c.AdjDate, tdate) >= 0) && a.Purpose == "Stock Adjustment"
                                   select new
                                   {
                                       id = c.StockAdjustmentId,
                                       particulars = b.Name,
                                       Date = (DateTime?)a.Date,
                                       Invoice = c.VoucherNo,
                                       Type = "Contra Voucher",
                                       RAccount = b.Name,
                                       RAccountID = b.AccountsID,
                                       Debit = (decimal?)a.Debit,
                                       Credit = (decimal?)a.Credit,
                                       entry = (DateTime?)a.CreatedDate,
                                       Remark = c.Reason,
                                       Amount = c.PurchaseRate,
                                       TRN = b.TRN,
                                       TransactionId = a.Id,
                                       Account = a.Account,
                                       reference = a.reference
                                   });
            decimal dumy = 100;
            var payroll = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && a.Purpose == "Payroll Voucher"
                           select new
                           {
                               id = a.Id,
                               particulars = b.Name,
                               Date = (DateTime?)a.Date,
                               Invoice = a.reference.ToString(),
                               Type = "Payroll Voucher",
                               RAccount = b.Name,
                               RAccountID = b.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = "Payroll",
                               Amount = dumy,
                               TRN = b.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });

            var full = Payment.Union(Reciept);
            // var disc = PaymentDiscount.Union(RecieptDiscount);

            var pur = Purchase.Union(PReturn);
            var sal = Sale.Union(SReturn);
            var joc = Journal.Union(Contra);

            // full = full.Union(disc);
            full = full.Union(pur);
            full = full.Union(payroll);
            full = full.Union(sal);
            full = full.Union(joc);
            full = full.Union(StockAdjustment);
            full = full.Union(ProperytReg);
            full = full.Union(tenancyReg);
            full = full.Union(maintanacerpt);
            full = full.Union(Asset);
            full = full.Union(AssetPurchase);
            full = full.Union(AssetToInventory);
            //full = full.Union(RecieptDiscount);
            //full = full.Union(PaymentDiscount);
            full = full.AsQueryable().OrderBy("Date asc, entry asc");
            vmodel.Ledger = (from a in full
                             select new Ledger
                             {
                                 Date = a.Date,
                                 Invoice = a.Invoice,
                                 Type = a.Type,
                                 RAccount = a.RAccount,
                                 RAccountID = a.RAccountID,
                                 Debit = a.Debit,
                                 Credit = a.Credit,
                                 particulars = a.particulars,
                                 Remark = a.Remark,
                                 Amount = a.Amount,
                                 MainId = a.id,
                                 TRN = a.TRN,
                                 TransactionId = a.TransactionId,
                                 Account = a.Account,
                                 Reference = a.reference
                             }).ToList();
            decimal? grandsum = 0;
            if (vmodel.Ledger.Count() != 0)
            {
                var debitsum = (from a in full
                                select new
                                {
                                    Debit = (decimal?)a.Debit,
                                }).Sum(a => a.Debit);
                var creditsum = (from a in full
                                 select new
                                 {
                                     Credit = (decimal?)a.Credit,
                                 }).Sum(a => a.Credit);
                if (sum > 0)
                {
                    grandsum = sum - ((debitsum > creditsum) ? (debitsum - creditsum) : (creditsum - debitsum));
                }
                else
                {
                    grandsum = ((debitsum > creditsum) ? (debitsum - creditsum) : (creditsum - debitsum)) + sum;
                }

            }
            else
            {
                grandsum = sum * -1;
            }

            return Convert.ToDecimal(grandsum);
        }
        public LedgerminiViewModel LedgerDatacommendmini(long AccId, string fromdate, string todate, long? AccGroup, bool? pdc)
        {
            LedgerminiViewModel vmodel = new LedgerminiViewModel();
            String format = "dd-MM-yyyy";
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? from = null;
            DateTime? to = null;
            Dictionary<string, object> Balance = null;
            long[] Accounts = { };
            if (fromdate != "")
            {
                //  fdate = DateTime.Parse(fromdate.ToString(), new CultureInfo("en-GB"));
                fdate = DateTime.ParseExact(fromdate, format, new CultureInfo("en-GB"));
                from = fdate;
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate.ToString(), new CultureInfo("en-GB"));
                to = tdate;
            }
            if (AccId == -1)
            {
                Accounts = AllAccounts(AccGroup);
            }
            else
            {
                Array.Resize(ref Accounts, Accounts.Length + 1);
                Accounts[0] = AccId;
            }
            Balance = OpenBlnc(AccId, (DateTime)fdate, pdc, Accounts);

            if ((string)Balance["type"] == "Cr")
            {
                vmodel.OpeningBalance = (decimal)Balance["amount"];
                vmodel.blnceType = (string)Balance["type"];
            }
            else
            {
                vmodel.OpeningBalance = (0 - (decimal)Balance["amount"]);
                vmodel.blnceType = (string)Balance["type"];

            }
            var Group = (from a in db.AccountsGroups
                         where a.AccountsGroupID == AccGroup
                         select new
                         {
                             a.Name,
                             a.Alias
                         }).FirstOrDefault();
            var Account = (from a in db.Accountss
                           where a.AccountsID == AccId
                           select new
                           {
                               a.Name,
                               a.Alias,
                               a.OpnBalance,
                               a.OpnBalanceCr
                           }).FirstOrDefault();
            vmodel.MainAccount = (AccId != -1) ? Account.Name : Group.Name;
            vmodel.MainAccountID = AccId;
            vmodel.from = from;
            vmodel.to = to;
            // Transactions


            var Reciept = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Receipts on a.reference equals c.ReceiptId
                           join d in db.PDCs on new { f1 = c.ReceiptId, f2 = "Receipt" } equals new { f1 = d.Reference, f2 = d.PDCType } into pdcs
                           from d in pdcs.DefaultIfEmpty()
                           let bb = db.Accountss.Where(at => (c.PayFrom != a.Account && at.AccountsID == c.PayFrom) || (c.PayFrom == a.Account && at.AccountsID == c.PayTo)).FirstOrDefault()
                           where (d.PDCType == "Receipt" && fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Receipt" || a.Purpose == "Discount Allowed")
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.ReceiptId,
                               particulars = b.Name,
                               Date = (a.Status == null) ? (DateTime?)a.Date : c.Date,
                               Invoice = c.VoucherNo,
                               Type = a.Purpose,
                               RAccount = bb.Name,
                               RAccountID = bb.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = c.Remark + ((d.PDCDate != null) ? " Pdc Date :" + d.PDCDate : ""),
                               Amount = c.GrandTotal - (decimal)c.Discount - ((decimal)a.Debit == 0 ? (decimal)a.Credit : (decimal)a.Debit),
                               TRN = bb.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            var Payment = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Payments on a.reference equals c.PaymentId
                           join d in db.PDCs on new { f1 = c.PaymentId, f2 = "Payment" } equals new { f1 = d.Reference, f2 = d.PDCType } into pdcs
                           from d in pdcs.DefaultIfEmpty()
                           let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
                           where (d.PDCType == "Payment" && fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Payment" || a.Purpose == "Expense Payment" || a.Purpose == "Discount Received")
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.PaymentId,
                               particulars = b.Name,

                               Date = (a.Status == null) ? (DateTime?)a.Date : c.Date,
                               Invoice = c.VoucherNo,
                               Type = a.Purpose,
                               RAccount = bb.Name,
                               RAccountID = bb.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = c.Remark + ((d.PDCDate != null) ? " Pdc Date :" + d.PDCDate : ""),
                               Amount = c.GrandTotal - c.Discount - ((decimal)a.Debit == 0 ? (decimal)a.Credit : (decimal)a.Debit),
                               TRN = bb.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            //var RecieptDiscount = (from a in db.AccountsTransactions
            //                       join b in db.Accountss on a.Account equals b.AccountsID
            //                       join c in db.Receipts on a.reference equals c.ReceiptId
            //                       let bb = db.Accountss.Where(at => (c.PayFrom != a.Account && at.AccountsID == c.PayFrom) || (c.PayFrom == a.Account && at.AccountsID == c.PayTo)).FirstOrDefault()
            //                       where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
            //                       (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
            //                       (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Discount Allowed")
            //                       && (pdc == true || a.Status == null)
            //                       select new
            //                       {
            //                           id = c.ReceiptId,
            //                           particulars = b.Name,
            //                           Date = (DateTime?)a.Date,
            //                           Invoice = c.VoucherNo,
            //                           Type = a.Purpose,
            //                           RAccount = bb.Name,
            //                           RAccountID = bb.AccountsID,
            //                           Debit = (decimal?)a.Debit,
            //                           Credit = (decimal?)a.Credit,
            //                           entry = (DateTime?)a.CreatedDate,
            //                           Remark = c.Remark
            //                       });
            //var PaymentDiscount = (from a in db.AccountsTransactions
            //                       join b in db.Accountss on a.Account equals b.AccountsID
            //                       join c in db.Payments on a.reference equals c.PaymentId
            //                       let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
            //                       where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
            //                       (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
            //                       (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Discount Recieve")
            //                       && (pdc == true || a.Status == null)
            //                       select new
            //                       {
            //                           id = c.PaymentId,
            //                           particulars = b.Name,
            //                           Date = (DateTime?)a.Date,
            //                           Invoice = c.VoucherNo,
            //                           Type = a.Purpose,
            //                           RAccount = bb.Name,
            //                           RAccountID = bb.AccountsID,
            //                           Debit = (decimal?)a.Debit,
            //                           Credit = (decimal?)a.Credit,
            //                           entry = (DateTime?)a.CreatedDate,
            //                           Remark = c.Remark
            //                       });
            var Sale = (from a in db.AccountsTransactions
                        join b in db.Accountss on a.Account equals b.AccountsID
                        join c in db.SalesEntrys on a.reference equals c.SalesEntryId
                        join d in db.Customers on c.Customer equals d.CustomerID
                        let bb = db.Receipts.Where(at => a.Purpose == "Sale Payment" && at.editable == choice.No && at.Reference == c.SalesEntryId && at.Remark == "Direct Reciept From Sale Entry").Join(db.Accountss, f1 => f1.PayTo, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                        let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()

                        where (fromdate == null || EF.Functions.DateDiffDay(c.SEDate, fdate) <= 0) &&
                        (todate == null || EF.Functions.DateDiffDay(c.SEDate, tdate) >= 0) &&
                        (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Sale" || a.Purpose == "Sale Payment")
                        && (pdc == true || a.Status == null)
                        select new
                        {
                            id = c.SalesEntryId,
                            particulars = b.Name,

                            Date = (DateTime?)c.SEDate,
                            Invoice = (a.Purpose != "Sale Payment" || (d.Accounts != a.Account)) ? c.BillNo : "",
                            Type = (a.Purpose != "Sale Payment") ? a.Purpose : (d.Accounts != a.Account) ? "Sale" : "",
                            RAccount = (d.Accounts == a.Account) ? ((a.Purpose != "Sale Payment") ? "Sale" : bb.Name) : d.CustomerName,
                            RAccountID = b.AccountsID,
                            Debit = (decimal?)a.Debit,
                            Credit = (decimal?)a.Credit,
                            entry = (DateTime?)a.CreatedDate,
                            Remark = (a.Purpose != "Sale Payment") ? c.Remarks : "",
                            Amount = (c.SalesType != 3 && a.Account != 502) ? c.SESubTotal - c.SEDiscount : ((a.Credit / (decimal)5) * 100),
                            TRN = AC.TRN,
                            TransactionId = a.Id,
                            Account = a.Account,
                            reference = a.reference
                        });
            var SReturn = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.SalesReturns on a.reference equals c.SalesReturnId
                           join d in db.Customers on c.Customer equals d.CustomerID
                           let bb = db.Payments.Where(at => a.Purpose == "Sale Return Payment" && at.editable == choice.No && at.Reference == c.SalesReturnId && at.RefType == "SalesReturn").Join(db.Accountss, f1 => f1.PayFrom, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                           let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                           where (fromdate == null || EF.Functions.DateDiffDay(c.SRDate, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(c.SRDate, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Sale Return" || a.Purpose == "Sale Return Payment")
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.SalesReturnId,
                               particulars = b.Name,
                               Date = (DateTime?)c.SRDate,
                               Invoice = a.Purpose != "Sale Return Payment" ? c.BillNo : "",
                               Type = a.Purpose != "Sale Return Payment" ? a.Purpose : "",
                               RAccount = (d.Accounts == a.Account) ? ((a.Purpose != "Sale Return Payment") ? "Sales Return" : bb.Name) : d.CustomerName,
                               RAccountID = b.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = a.Purpose != "Sale Return Payment" ? c.Remarks : "",
                               Amount = c.SRSubTotal - c.SRDiscount,
                               TRN = AC.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            var Purchase = (from a in db.AccountsTransactions
                            join b in db.Accountss on a.Account equals b.AccountsID
                            join c in db.PurchaseEntrys on a.reference equals c.PurchaseEntryId
                            join d in db.Suppliers on c.Supplier equals d.SupplierID
                            let bb = db.Payments.Where(at => a.Purpose == "Purchase Payment" && at.editable == choice.No && at.Reference == c.PurchaseEntryId && at.RefType == "Purchase").Join(db.Accountss, f1 => f1.PayFrom, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                            let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                            where (fromdate == "" || EF.Functions.DateDiffDay(c.PEDate, fdate) <= 0) &&
                            (todate == "" || EF.Functions.DateDiffDay(c.PEDate, tdate) >= 0) &&
                            (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Purchase" || a.Purpose == "Purchase Payment")
                            && (pdc == true || a.Status == null)
                            select new
                            {
                                id = c.PurchaseEntryId,
                                particulars = b.Name,

                                Date = (DateTime?)c.PEDate,
                                Invoice = a.Purpose == "Purchase Payment" ? "" : c.BillNo,
                                Type = a.Purpose == "Purchase Payment" ? "" : a.Purpose,
                                RAccount = (d.Accounts == a.Account) ? ((a.Purpose != "Purchase Payment") ? "Purchase" : bb.Name) : d.SupplierName,
                                RAccountID = b.AccountsID,
                                Debit = (decimal?)a.Debit,
                                Credit = (decimal?)a.Credit,
                                entry = (DateTime?)a.CreatedDate,
                                Remark = a.Purpose == "Purchase Payment" ? "" : c.Remarks,
                                Amount = c.PESubTotal - c.PEDiscount,
                                TRN = AC.TRN,
                                TransactionId = a.Id,
                                Account = a.Account,
                                reference = a.reference

                            });
            var PReturn = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.PurchaseReturns on a.reference equals c.PurchaseReturnId
                           join d in db.Suppliers on c.Supplier equals d.SupplierID
                           let bb = db.Receipts.Where(at => a.Purpose == "Purchase Return Payment" && at.editable == choice.No && at.Reference == c.PurchaseReturnId && at.Remark == "Receipt From Purchase Return").Join(db.Accountss, f1 => f1.PayTo, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                           let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                           where (fromdate == "" || EF.Functions.DateDiffDay(c.PRDate, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(c.PRDate, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Purchase Return" || a.Purpose == "Purchase Return Payment")
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.PurchaseReturnId,
                               particulars = b.Name,
                               Date = (DateTime?)c.PRDate,
                               Invoice = a.Purpose == "Purchase Return Payment" ? "" : c.BillNo,
                               Type = a.Purpose == "Purchase Return Payment" ? "" : a.Purpose,
                               RAccount = (d.Accounts == a.Account) ? "Purchase" : d.SupplierName,
                               RAccountID = b.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = a.Purpose == "Purchase Return Payment" ? "" : c.Remarks,
                               Amount = c.PRSubTotal - c.PRDiscount,
                               TRN = AC.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });

            var Journal = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Journals on a.reference equals c.JournalId
                           //let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
                           join d in db.PDCs on new { f1 = c.JournalId, f2 = "Journal" } equals new { f1 = d.Reference, f2 = d.PDCType } into pdcs
                           from d in pdcs.DefaultIfEmpty()
                           let bd = db.Accountss.Where(at => (a.Type == DC.Credit && at.AccountsID == c.PayTo) || (a.Type == DC.Debit && at.AccountsID == c.PayFrom)).FirstOrDefault()
                           let acCount = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && z.Account == a.Account && z.Type == a.Type).Select(z => a.Account).Count()
                           let prev = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && a.Type != z.Type && (a.Type == DC.Credit && a.Id > z.Id) && z.Account != 501 && z.Account != 502).OrderByDescending(z => z.Id).Select(z => z.Account).FirstOrDefault()
                           let next = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && a.Type != z.Type && (a.Type == DC.Debit && a.Id < z.Id) && z.Account != 501 && z.Account != 502).OrderBy(z => z.Id).Select(z => z.Account).FirstOrDefault()
                           where (d.PDCType == "Journal" && fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) && a.Purpose == "Journal"
                           && (pdc == true || a.Status == null) &&
                           a.Purpose == "Journal"

                           select new
                           {
                               id = c.JournalId,
                               particulars = b.Name,
                               Date = (DateTime?)a.Date,
                               Invoice = c.VoucherNo,
                               Type = "Journal Entry",
                               RAccount = (acCount <= 1) ? bd.Name : db.Accountss.Where(y => (a.Type == DC.Credit && y.AccountsID == prev) || (a.Type == DC.Debit && y.AccountsID == next)).Select(y => y.Name).FirstOrDefault(),
                               RAccountID = bd.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = c.Remark + ((d.PDCDate != null) ? " Pdc Date :" + d.PDCDate : ""),
                               Amount = (decimal)c.GrandTotal,
                               TRN = bd.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });

            var Contra = (from a in db.AccountsTransactions
                          join b in db.Accountss on a.Account equals b.AccountsID
                          join c in db.ContraVouchers on a.reference equals c.ContraVoucherId
                          let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
                          where (fromdate == null || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) &&
                          (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) &&
                          (todate == null || EF.Functions.DateDiffDay(c.Date, tdate) >= 0) && a.Purpose == "ContraVoucher"
                          && (pdc == true || a.Status == null)
                          select new
                          {
                              id = c.ContraVoucherId,
                              particulars = b.Name,
                              Date = (DateTime?)a.Date,
                              Invoice = c.VoucherNo,
                              Type = "Contra Voucher",
                              RAccount = bb.Name,
                              RAccountID = bb.AccountsID,
                              Debit = (decimal?)a.Debit,
                              Credit = (decimal?)a.Credit,
                              entry = (DateTime?)a.CreatedDate,
                              Remark = c.Remark,
                              Amount = c.Amount,
                              TRN = bb.TRN,
                              TransactionId = a.Id,
                              Account = a.Account,
                              reference = a.reference

                          });
            var StockAdjustment = (from a in db.AccountsTransactions
                                   join b in db.Accountss on a.Account equals b.AccountsID
                                   join c in db.StockAdjustments on a.reference equals c.StockAdjustmentId
                                   where (fromdate == null || EF.Functions.DateDiffDay(c.AdjDate, fdate) <= 0) &&
                                   (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) &&
                                   (todate == null || EF.Functions.DateDiffDay(c.AdjDate, tdate) >= 0) && a.Purpose == "Stock Adjustment"
                                   select new
                                   {
                                       id = c.StockAdjustmentId,
                                       particulars = b.Name,
                                       Date = (DateTime?)a.Date,
                                       Invoice = c.VoucherNo,
                                       Type = "Contra Voucher",
                                       RAccount = b.Name,
                                       RAccountID = b.AccountsID,
                                       Debit = (decimal?)a.Debit,
                                       Credit = (decimal?)a.Credit,
                                       entry = (DateTime?)a.CreatedDate,
                                       Remark = c.Reason,
                                       Amount = c.PurchaseRate,
                                       TRN = b.TRN,
                                       TransactionId = a.Id,
                                       Account = a.Account,
                                       reference = a.reference
                                   });

            var full = Payment.Union(Reciept);
            // var disc = PaymentDiscount.Union(RecieptDiscount);

            var pur = Purchase.Union(PReturn);
            var sal = Sale.Union(SReturn);
            var joc = Journal.Union(Contra);

            // full = full.Union(disc);
            full = full.Union(pur);
            full = full.Union(sal);
            full = full.Union(joc);
            full = full.Union(StockAdjustment);

            //full = full.Union(RecieptDiscount);
            //full = full.Union(PaymentDiscount);
            full = full.AsQueryable().OrderBy("Date asc, entry asc");
            //full = full.Union(RecieptDiscount);
            //full = full.Union(PaymentDiscount);
            //var full = Sale.OrderBy("Date asc, entry asc");
            vmodel.Ledger = (from a in full
                             select new Ledgermini
                             {
                                 Date = a.Date,
                                 Invoice = a.Invoice,
                                 Type = a.Type,
                                 RAccount = a.RAccount,
                                 RAccountID = a.RAccountID,
                                 Debit = a.Debit,
                                 Credit = a.Credit,
                                 particulars = a.particulars,
                                 Remark = a.Remark,
                                 Amount = a.Amount,
                                 MainId = a.id,
                                 TRN = a.TRN,
                                 TransactionId = a.TransactionId,
                                 Account = a.Account,
                                 Reference = a.reference,
                                 // paymenttype=a.paymenttype,
                                 //ordertype=a.ott,


                             }).ToList();
            return vmodel;
        }

        public LedgertaxViewModel LedgerDatacommendtax(long AccId, string fromdate, string todate, long? AccGroup, bool? pdc)
        {
            LedgertaxViewModel vmodel = new LedgertaxViewModel();
            String format = "dd-MM-yyyy";
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? from = null;
            DateTime? to = null;
            Dictionary<string, object> Balance = null;
            long[] Accounts = { };
            if (fromdate != "")
            {
                //  fdate = DateTime.Parse(fromdate.ToString(), new CultureInfo("en-GB"));
                fdate = DateTime.ParseExact(fromdate, format, new CultureInfo("en-GB"));
                from = fdate;
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate.ToString(), new CultureInfo("en-GB"));
                to = tdate;
            }
            if (AccId == -1)
            {
                Accounts = AllAccounts(AccGroup);
            }
            else
            {
                Array.Resize(ref Accounts, Accounts.Length + 1);
                Accounts[0] = AccId;
            }
            Balance = OpenBlnc(AccId, (DateTime)fdate, pdc, Accounts);

            if ((string)Balance["type"] == "Cr")
            {
                vmodel.OpeningBalance = (decimal)Balance["amount"];
                vmodel.blnceType = (string)Balance["type"];
            }
            else
            {
                vmodel.OpeningBalance = (0 - (decimal)Balance["amount"]);
                vmodel.blnceType = (string)Balance["type"];

            }
            var Group = (from a in db.AccountsGroups
                         where a.AccountsGroupID == AccGroup
                         select new
                         {
                             a.Name,
                             a.Alias
                         }).FirstOrDefault();
            var Account = (from a in db.Accountss
                           where a.AccountsID == AccId
                           select new
                           {
                               a.Name,
                               a.Alias,
                               a.OpnBalance,
                               a.OpnBalanceCr
                           }).FirstOrDefault();
            vmodel.MainAccount = (AccId != -1) ? Account.Name : Group.Name;
            vmodel.MainAccountID = AccId;
            vmodel.from = from;
            vmodel.to = to;
            // Transactions
            var Reciept = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Receipts on a.reference equals c.ReceiptId
                           join d in db.PDCs on new { f1 = c.ReceiptId, f2 = "Receipt" } equals new { f1 = d.Reference, f2 = d.PDCType } into pdcs
                           from d in pdcs.DefaultIfEmpty()
                           let bb = db.Accountss.Where(at => (c.PayFrom != a.Account && at.AccountsID == c.PayFrom) || (c.PayFrom == a.Account && at.AccountsID == c.PayTo)).FirstOrDefault()
                           where (d.PDCType == "Receipt" && fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Receipt" || a.Purpose == "Discount Allowed")
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.ReceiptId,
                               particulars = b.Name,
                               Date = (DateTime?)a.Date,
                               Invoice = c.VoucherNo,
                               Type = a.Purpose,
                               RAccount = bb.Name,
                               RAccountID = bb.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Debitwithouttax = "",
                               tax = "",
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = c.Remark + ((d.PDCDate != null) ? " Pdc Date :" + d.PDCDate : ""),
                               Amount = c.GrandTotal - (decimal)c.Discount - ((decimal)a.Debit == 0 ? (decimal)a.Credit : (decimal)a.Debit),
                               TRN = bb.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            var Payment = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Payments on a.reference equals c.PaymentId
                           join d in db.PDCs on new { f1 = c.PaymentId, f2 = "Payment" } equals new { f1 = d.Reference, f2 = d.PDCType } into pdcs
                           from d in pdcs.DefaultIfEmpty()
                           let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
                           where (d.PDCType == "Payment" && fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Payment" || a.Purpose == "Expense Payment" || a.Purpose == "Discount Received")
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.PaymentId,
                               particulars = b.Name,

                               Date = (DateTime?)a.Date,
                               Invoice = c.VoucherNo,
                               Type = a.Purpose,
                               RAccount = bb.Name,
                               RAccountID = bb.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Debitwithouttax = "",
                               tax = "",
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = c.Remark + ((d.PDCDate != null) ? " Pdc Date :" + d.PDCDate : ""),
                               Amount = c.GrandTotal - c.Discount - ((decimal)a.Debit == 0 ? (decimal)a.Credit : (decimal)a.Debit),
                               TRN = bb.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });


            var Sale = (from a in db.AccountsTransactions
                        join b in db.Accountss on a.Account equals b.AccountsID
                        join c in db.SalesEntrys on a.reference equals c.SalesEntryId
                        join d in db.Customers on c.Customer equals d.CustomerID
                        let bb = db.Receipts.Where(at => a.Purpose == "Sale Payment" && at.editable == choice.No && at.Reference == c.SalesEntryId && at.Remark == "Direct Reciept From Sale Entry").Join(db.Accountss, f1 => f1.PayTo, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                        let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()

                        where (fromdate == null || EF.Functions.DateDiffDay(c.SEDate, fdate) <= 0) &&
                        (todate == null || EF.Functions.DateDiffDay(c.SEDate, tdate) >= 0) &&
                        (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Sale" || a.Purpose == "Sale Payment")
                        && (pdc == true || a.Status == null)
                        select new
                        {
                            id = c.SalesEntryId,
                            particulars = b.Name,

                            Date = (DateTime?)c.SEDate,
                            Invoice = (a.Purpose != "Sale Payment" || (d.Accounts != a.Account)) ? c.BillNo : "",
                            Type = (a.Purpose != "Sale Payment") ? a.Purpose : (d.Accounts != a.Account) ? "Sale" : "",
                            RAccount = (d.Accounts == a.Account) ? ((a.Purpose != "Sale Payment") ? "Sale" : bb.Name) : d.CustomerName,
                            RAccountID = b.AccountsID,
                            Debit = (decimal?)a.Debit,
                            Debitwithouttax = c.SESubTotal.ToString(),
                            tax = c.SETax.ToString(),
                            Credit = (decimal?)a.Credit,
                            entry = (DateTime?)a.CreatedDate,
                            Remark = (a.Purpose != "Sale Payment") ? c.Remarks : "",
                            Amount = (c.SalesType != 3 && a.Account != 502) ? c.SESubTotal - c.SEDiscount : ((a.Credit / (decimal)5) * 100),
                            TRN = AC.TRN,
                            TransactionId = a.Id,
                            Account = a.Account,
                            reference = a.reference



                        });
            var SReturn = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.SalesReturns on a.reference equals c.SalesReturnId
                           join d in db.Customers on c.Customer equals d.CustomerID
                           let bb = db.Payments.Where(at => a.Purpose == "Sale Return Payment" && at.editable == choice.No && at.Reference == c.SalesReturnId && at.RefType == "SalesReturn").Join(db.Accountss, f1 => f1.PayFrom, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                           let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                           where (fromdate == null || EF.Functions.DateDiffDay(c.SRDate, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(c.SRDate, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Sale Return" || a.Purpose == "Sale Return Payment")
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.SalesReturnId,
                               particulars = b.Name,
                               Date = (DateTime?)c.SRDate,
                               Invoice = a.Purpose != "Sale Return Payment" ? c.BillNo : "",
                               Type = a.Purpose != "Sale Return Payment" ? a.Purpose : "",
                               RAccount = (d.Accounts == a.Account) ? ((a.Purpose != "Sale Return Payment") ? "Sales Return" : bb.Name) : d.CustomerName,
                               RAccountID = b.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Debitwithouttax = "",
                               tax = "",
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = a.Purpose != "Sale Return Payment" ? c.Remarks : "",
                               Amount = c.SRSubTotal - c.SRDiscount,
                               TRN = AC.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            var Purchase = (from a in db.AccountsTransactions
                            join b in db.Accountss on a.Account equals b.AccountsID
                            join c in db.PurchaseEntrys on a.reference equals c.PurchaseEntryId
                            join d in db.Suppliers on c.Supplier equals d.SupplierID
                            let bb = db.Payments.Where(at => a.Purpose == "Purchase Payment" && at.editable == choice.No && at.Reference == c.PurchaseEntryId && at.RefType == "Purchase").Join(db.Accountss, f1 => f1.PayFrom, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                            let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                            where (fromdate == "" || EF.Functions.DateDiffDay(c.PEDate, fdate) <= 0) &&
                            (todate == "" || EF.Functions.DateDiffDay(c.PEDate, tdate) >= 0) &&
                            (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Purchase" || a.Purpose == "Purchase Payment")
                            && (pdc == true || a.Status == null)
                            select new
                            {
                                id = c.PurchaseEntryId,
                                particulars = b.Name,

                                Date = (DateTime?)c.PEDate,
                                Invoice = a.Purpose == "Purchase Payment" ? "" : c.BillNo,
                                Type = a.Purpose == "Purchase Payment" ? "" : a.Purpose,
                                RAccount = (d.Accounts == a.Account) ? ((a.Purpose != "Purchase Payment") ? "Purchase" : bb.Name) : d.SupplierName,
                                RAccountID = b.AccountsID,
                                Debit = (decimal?)a.Debit,
                                Debitwithouttax = "",
                                tax = "",
                                Credit = (decimal?)a.Credit,
                                entry = (DateTime?)a.CreatedDate,
                                Remark = a.Purpose == "Purchase Payment" ? "" : c.Remarks,
                                Amount = c.PESubTotal - c.PEDiscount,
                                TRN = AC.TRN,
                                TransactionId = a.Id,
                                Account = a.Account,
                                reference = a.reference

                            });
            var PReturn = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.PurchaseReturns on a.reference equals c.PurchaseReturnId
                           join d in db.Suppliers on c.Supplier equals d.SupplierID
                           let bb = db.Receipts.Where(at => a.Purpose == "Purchase Return Payment" && at.editable == choice.No && at.Reference == c.PurchaseReturnId && at.Remark == "Receipt From Purchase Return").Join(db.Accountss, f1 => f1.PayTo, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                           let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                           where (fromdate == "" || EF.Functions.DateDiffDay(c.PRDate, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(c.PRDate, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Purchase Return" || a.Purpose == "Purchase Return Payment")
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.PurchaseReturnId,
                               particulars = b.Name,
                               Date = (DateTime?)c.PRDate,
                               Invoice = a.Purpose == "Purchase Return Payment" ? "" : c.BillNo,
                               Type = a.Purpose == "Purchase Return Payment" ? "" : a.Purpose,
                               RAccount = (d.Accounts == a.Account) ? "Purchase" : d.SupplierName,
                               RAccountID = b.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Debitwithouttax = "",
                               tax = "",
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = a.Purpose == "Purchase Return Payment" ? "" : c.Remarks,
                               Amount = c.PRSubTotal - c.PRDiscount,
                               TRN = AC.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            var Journal = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Journals on a.reference equals c.JournalId
                           //let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
                           join d in db.PDCs on c.JournalId equals d.Reference into pdcs
                           from d in pdcs.DefaultIfEmpty()
                           let bd = db.Accountss.Where(at => (a.Type == DC.Credit && at.AccountsID == c.PayTo) || (a.Type == DC.Debit && at.AccountsID == c.PayFrom)).FirstOrDefault()
                           let acCount = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && z.Account == a.Account && z.Type == a.Type).Select(z => a.Account).Count()
                           let prev = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && a.Type != z.Type && (a.Type == DC.Credit && a.Id > z.Id) && z.Account != 501 && z.Account != 502).OrderByDescending(z => z.Id).Select(z => z.Account).FirstOrDefault()
                           let next = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && a.Type != z.Type && (a.Type == DC.Debit && a.Id < z.Id) && z.Account != 501 && z.Account != 502).OrderBy(z => z.Id).Select(z => z.Account).FirstOrDefault()
                           where (d.PDCType == "Journal" && fromdate == null || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) &&
                           (todate == null || EF.Functions.DateDiffDay(c.Date, tdate) >= 0) && a.Purpose == "Journal"
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.JournalId,
                               particulars = b.Name,
                               Date = (DateTime?)a.Date,
                               Invoice = c.VoucherNo,
                               Type = "Journal Entry",
                               RAccount = (acCount <= 1) ? bd.Name : db.Accountss.Where(y => (a.Type == DC.Credit && y.AccountsID == prev) || (a.Type == DC.Debit && y.AccountsID == next)).Select(y => y.Name).FirstOrDefault(),
                               RAccountID = bd.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Debitwithouttax = "",
                               tax = "",
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = c.Remark + ((d.PDCDate != null) ? " Pdc Date :" + d.PDCDate : ""),
                               Amount = (decimal)c.GrandTotal,
                               TRN = bd.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            var Contra = (from a in db.AccountsTransactions
                          join b in db.Accountss on a.Account equals b.AccountsID
                          join c in db.ContraVouchers on a.reference equals c.ContraVoucherId
                          let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
                          where (fromdate == null || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) &&
                          (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) &&
                          (todate == null || EF.Functions.DateDiffDay(c.Date, tdate) >= 0) && a.Purpose == "ContraVoucher"
                          && (pdc == true || a.Status == null)
                          select new
                          {
                              id = c.ContraVoucherId,
                              particulars = b.Name,
                              Date = (DateTime?)a.Date,
                              Invoice = c.VoucherNo,
                              Type = "Contra Voucher",
                              RAccount = bb.Name,
                              RAccountID = bb.AccountsID,
                              Debit = (decimal?)a.Debit,
                              Debitwithouttax = "",
                              tax = "",
                              Credit = (decimal?)a.Credit,
                              entry = (DateTime?)a.CreatedDate,
                              Remark = c.Remark,
                              Amount = c.Amount,
                              TRN = bb.TRN,
                              TransactionId = a.Id,
                              Account = a.Account,
                              reference = a.reference

                          });
            var StockAdjustment = (from a in db.AccountsTransactions
                                   join b in db.Accountss on a.Account equals b.AccountsID
                                   join c in db.StockAdjustments on a.reference equals c.StockAdjustmentId
                                   where (fromdate == null || EF.Functions.DateDiffDay(c.AdjDate, fdate) <= 0) &&
                                   (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) &&
                                   (todate == null || EF.Functions.DateDiffDay(c.AdjDate, tdate) >= 0) && a.Purpose == "Stock Adjustment"
                                   select new
                                   {
                                       id = c.StockAdjustmentId,
                                       particulars = b.Name,
                                       Date = (DateTime?)a.Date,
                                       Invoice = c.VoucherNo,
                                       Type = "Contra Voucher",
                                       RAccount = b.Name,
                                       RAccountID = b.AccountsID,
                                       Debit = (decimal?)a.Debit,
                                       Debitwithouttax = "",
                                       tax = "",
                                       Credit = (decimal?)a.Credit,
                                       entry = (DateTime?)a.CreatedDate,
                                       Remark = c.Reason,
                                       Amount = c.PurchaseRate,
                                       TRN = b.TRN,
                                       TransactionId = a.Id,
                                       Account = a.Account,
                                       reference = a.reference
                                   });



            var full = Payment.Union(Reciept);


            var pur = Purchase.Union(PReturn);
            var sal = Sale.Union(SReturn);
            var joc = Journal.Union(Contra);


            full = full.Union(pur);
            full = full.Union(sal);
            full = full.Union(joc);
            full = full.Union(StockAdjustment);
            full = full.AsQueryable().OrderBy("Date asc, entry asc");
            vmodel.Ledger = (from a in full
                             select new Ledgertax
                             {
                                 Date = a.Date,
                                 Invoice = a.Invoice,
                                 Type = a.Type,
                                 RAccount = a.RAccount,
                                 RAccountID = a.RAccountID,
                                 Debit = a.Debit,
                                 Credit = a.Credit,
                                 particulars = a.particulars,
                                 Remark = a.Remark,
                                 Amount = a.Amount,
                                 MainId = a.id,
                                 TRN = a.TRN,
                                 TransactionId = a.TransactionId,
                                 Account = a.Account,
                                 Reference = a.reference,

                                 Debitwithouttax = a.Debitwithouttax,
                                 tax = a.tax


                             }).ToList();
            return vmodel;
        }

        public LedgerProViewModel LedgerDataProp(long AccId, string fromdate, string todate, long? AccGroup, bool? pdc)
        {
            LedgerProViewModel vmodel = new LedgerProViewModel();
            String format = "dd-MM-yyyy";
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? from = null;
            DateTime? to = null;
            Dictionary<string, object> Balance = null;
            long[] Accounts = { };
            if (fromdate != "")
            {
                //  fdate = DateTime.Parse(fromdate.ToString(), new CultureInfo("en-GB"));
                fdate = DateTime.ParseExact(fromdate, format, new CultureInfo("en-GB"));
                from = fdate;
            }
            else
                fromdate = null;
            if (todate != "")
            {
                tdate = DateTime.Parse(todate.ToString(), new CultureInfo("en-GB"));
                to = tdate;
            }
            else
                todate = null;
            if (AccId == -1)
            {
                Accounts = AllAccounts(AccGroup);
            }
            else
            {
                Array.Resize(ref Accounts, Accounts.Length + 1);
                Accounts[0] = AccId;
            }
            if (fdate != null)
            {
                Balance = OpenBlnc(AccId, (DateTime)fdate, pdc, Accounts);


                if ((string)Balance["type"] == "Cr")
                {
                    vmodel.OpeningBalance = (decimal)Balance["amount"];
                    vmodel.blnceType = (string)Balance["type"];
                }
                else
                {
                    vmodel.OpeningBalance = (0 - (decimal)Balance["amount"]);
                    vmodel.blnceType = (string)Balance["type"];

                }
            }

            var Group = (from a in db.AccountsGroups
                         where a.AccountsGroupID == AccGroup
                         select new
                         {
                             a.Name,
                             a.Alias
                         }).FirstOrDefault();
            var Account = (from a in db.Accountss
                           where a.AccountsID == AccId
                           select new
                           {
                               a.Name,
                               a.Alias,
                               a.OpnBalance,
                               a.OpnBalanceCr
                           }).FirstOrDefault();
            vmodel.MainAccount = (AccId != -1) ? Account.Name : Group.Name;
            vmodel.MainAccountID = AccId;
            vmodel.from = from;
            vmodel.to = to;







            var Reciept = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Receipts on a.reference equals c.ReceiptId
                           join d in db.PDCs on c.ReceiptId equals d.Reference into ps
                           from p in ps.DefaultIfEmpty()
                           let bb = db.Accountss.Where(at => (c.PayFrom != a.Account && at.AccountsID == c.PayFrom) || (c.PayFrom == a.Account && at.AccountsID == c.PayTo)).FirstOrDefault()
                           where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Receipt" || a.Purpose == "Discount Allowed")
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.ReceiptId,
                               particulars = b.Name,
                               Date = (DateTime?)a.Date,
                               PdcDate = p.PDCDate,

                               chequeno = p.CheckNo,
                               bank = p.Bank,
                               Invoice = c.VoucherNo,
                               Type = a.Purpose,
                               RAccount = bb.Name,
                               RAccountID = bb.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = c.Remark,
                               Amount = c.GrandTotal - (decimal)c.Discount - ((decimal)a.Debit == 0 ? (decimal)a.Credit : (decimal)a.Debit),
                               TRN = bb.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });

            var Asset = (from a in db.AccountsTransactions
                         join b in db.Accountss on a.Account equals b.AccountsID
                         join c in db.AssetTransferDetails on a.reference equals c.AssetEntryId
                         join d in db.AssetTransferMasters on c.AssetEntryId equals d.AssetEntryId
                         where (fromdate == null || EF.Functions.DateDiffDay(d.AssetEntryDate, fdate) <= 0) &&
                         (todate == null || EF.Functions.DateDiffDay(d.AssetEntryDate, tdate) >= 0) &&
                         (a.Account == AccId && a.Purpose.Contains("Asset From Inventory"))
                         && (pdc == true || a.Status == null)
                         select new
                         {
                             id = c.AssetEntryId,
                             particulars = b.Name,
                             Date = (DateTime?)d.AssetEntryDate,
                             Invoice = c.AssetEntryId.ToString(),
                             Type = "Asset From Inventory ",
                             RAccount = "",
                             RAccountID = b.AccountsID,
                             Debit = (decimal?)a.Debit,
                             Credit = (decimal?)a.Credit,
                             entry = (DateTime?)a.CreatedDate,
                             Remark = "Asset From Inventory",
                             Amount = d.TotalAssetValue,
                             TRN = "",
                             TransactionId = a.Id,
                             Account = a.Account,
                             reference = a.reference
                         });



            var Payment = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Payments on a.reference equals c.PaymentId
                           join d in db.PDCs on c.PaymentId equals d.Reference into ps
                           from p in ps.DefaultIfEmpty()
                           let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
                           where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Payment" || a.Purpose == "Expense Payment" || a.Purpose == "Discount Received")
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.PaymentId,
                               particulars = b.Name,

                               Date = (DateTime?)a.Date,
                               PdcDate = p.PDCDate,
                               chequeno = p.CheckNo,
                               bank = p.Bank,
                               Invoice = c.VoucherNo,
                               Type = a.Purpose,
                               RAccount = bb.Name,
                               RAccountID = bb.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = c.Remark,
                               Amount = c.GrandTotal - c.Discount - ((decimal)a.Debit == 0 ? (decimal)a.Credit : (decimal)a.Debit),
                               TRN = bb.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });

            var Journal = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Journals on a.reference equals c.JournalId
                           join d in db.PDCs on c.JournalId equals d.Reference into ps
                           from p in ps.DefaultIfEmpty()
                               //let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
                           let bd = db.Accountss.Where(at => (a.Type == DC.Credit && at.AccountsID == c.PayTo) || (a.Type == DC.Debit && at.AccountsID == c.PayFrom)).FirstOrDefault()
                           let acCount = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && z.Account == a.Account && z.Type == a.Type).Select(z => a.Account).Count()
                           let prev = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && a.Type != z.Type && (a.Type == DC.Credit && a.Id > z.Id) && z.Account != 501 && z.Account != 502).OrderByDescending(z => z.Id).Select(z => z.Account).FirstOrDefault()
                           let next = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && a.Type != z.Type && (a.Type == DC.Debit && a.Id < z.Id) && z.Account != 501 && z.Account != 502).OrderBy(z => z.Id).Select(z => z.Account).FirstOrDefault()


                           where (fromdate == null || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) &&
                           (todate == null || EF.Functions.DateDiffDay(c.Date, tdate) >= 0) && a.Purpose == "Journal"
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.JournalId,
                               particulars = b.Name,
                               Date = (DateTime?)a.Date,

                               PdcDate = p.PDCDate,
                               chequeno = p.CheckNo,
                               bank = p.Bank,
                               Invoice = c.VoucherNo,
                               Type = "Journal Entry",
                               RAccount = (acCount <= 1) ? bd.Name : db.Accountss.Where(y => (a.Type == DC.Credit && y.AccountsID == prev) || (a.Type == DC.Debit && y.AccountsID == next)).Select(y => y.Name).FirstOrDefault(),
                               RAccountID = bd.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = c.Remark,
                               Amount = (decimal)c.GrandTotal,
                               TRN = bd.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });

            var full = Payment.Union(Reciept);

            full = full.Union(Journal);
            //full = full.Union(RecieptDiscount);
            //full = full.Union(PaymentDiscount);
            full = full.AsQueryable().OrderBy("Date asc, entry asc");
            vmodel.Ledger = (from a in full
                             select new Ledgerpro
                             {
                                 Date = a.Date,
                                 PdcDate = a.PdcDate,
                                 chequeno = a.chequeno,
                                 bank = a.bank,
                                 Invoice = a.Invoice,
                                 Type = a.Type,
                                 RAccount = a.RAccount,
                                 RAccountID = a.RAccountID,
                                 Debit = a.Debit,
                                 Credit = a.Credit,
                                 particulars = a.particulars,
                                 Remark = a.Remark,
                                 Amount = a.Amount,
                                 MainId = a.id,
                                 TRN = a.TRN,
                                 TransactionId = a.TransactionId,
                                 Account = a.Account,
                                 Reference = a.reference
                             }).ToList();
            return vmodel;
        }

        public LedgerProViewModel LedgerData(long AccId, string fromdate, string todate, long? AccGroup, bool? pdc)
        {
            LedgerProViewModel vmodel = new LedgerProViewModel();
            String format = "dd-MM-yyyy";
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? from = null;
            DateTime? to = null;
            Dictionary<string, object> Balance = null;
            long[] Accounts = { };
            if (fromdate != "")
            {
                //  fdate = DateTime.Parse(fromdate.ToString(), new CultureInfo("en-GB"));
                fdate = DateTime.ParseExact(fromdate, format, new CultureInfo("en-GB"));
                from = fdate;
            }
            else
                fromdate = null;
            if (todate != "")
            {
                tdate = DateTime.Parse(todate.ToString(), new CultureInfo("en-GB"));
                to = tdate;
            }
            else
                todate = null;
            if (AccId == -1)
            {
                Accounts = AllAccounts(AccGroup);
            }
            else
            {
                Array.Resize(ref Accounts, Accounts.Length + 1);
                Accounts[0] = AccId;
            }
            if (fdate != null)
            {
                Balance = OpenBlnc(AccId, (DateTime)fdate, pdc, Accounts);


                if ((string)Balance["type"] == "Cr")
                {
                    vmodel.OpeningBalance = (decimal)Balance["amount"];
                    vmodel.blnceType = (string)Balance["type"];
                }
                else
                {
                    vmodel.OpeningBalance = (0 - (decimal)Balance["amount"]);
                    vmodel.blnceType = (string)Balance["type"];

                }
            }

            var Group = (from a in db.AccountsGroups
                         where a.AccountsGroupID == AccGroup
                         select new
                         {
                             a.Name,
                             a.Alias
                         }).FirstOrDefault();
            var Account = (from a in db.Accountss
                           where a.AccountsID == AccId
                           select new
                           {
                               a.Name,
                               a.Alias,
                               a.OpnBalance,
                               a.OpnBalanceCr
                           }).FirstOrDefault();
            vmodel.MainAccount = (AccId != -1) ? Account.Name : Group.Name;
            vmodel.MainAccountID = AccId;
            vmodel.from = from;
            vmodel.to = to;







            var Reciept = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Receipts on a.reference equals c.ReceiptId
                           join d in db.PDCs on c.ReceiptId equals d.Reference into ps
                           from p in ps.DefaultIfEmpty()
                           let bb = db.Accountss.Where(at => (c.PayFrom != a.Account && at.AccountsID == c.PayFrom) || (c.PayFrom == a.Account && at.AccountsID == c.PayTo)).FirstOrDefault()
                           where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Receipt" || a.Purpose == "Discount Allowed")
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.ReceiptId,
                               particulars = b.Name,
                               Date = (DateTime?)a.Date,
                               PdcDate = p.PDCDate,

                               chequeno = p.CheckNo,
                               bank = p.Bank,
                               Invoice = c.VoucherNo,
                               Type = a.Purpose,
                               RAccount = bb.Name,
                               RAccountID = bb.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = c.Remark,
                               Amount = c.GrandTotal - (decimal)c.Discount - ((decimal)a.Debit == 0 ? (decimal)a.Credit : (decimal)a.Debit),
                               TRN = bb.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });
            var Payment = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Payments on a.reference equals c.PaymentId
                           join d in db.PDCs on c.PaymentId equals d.Reference into ps
                           from p in ps.DefaultIfEmpty()
                           let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
                           where (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                           (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) && (a.Purpose == "Payment" || a.Purpose == "Expense Payment" || a.Purpose == "Discount Received")
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.PaymentId,
                               particulars = b.Name,

                               Date = (DateTime?)a.Date,
                               PdcDate = p.PDCDate,
                               chequeno = p.CheckNo,
                               bank = p.Bank,
                               Invoice = c.VoucherNo,
                               Type = a.Purpose,
                               RAccount = bb.Name,
                               RAccountID = bb.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = c.Remark,
                               Amount = c.GrandTotal - c.Discount - ((decimal)a.Debit == 0 ? (decimal)a.Credit : (decimal)a.Debit),
                               TRN = bb.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });

            var Journal = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           join c in db.Journals on a.reference equals c.JournalId
                           join d in db.PDCs on c.JournalId equals d.Reference into ps
                           from p in ps.DefaultIfEmpty()
                               //let bb = db.Accountss.Where(at => (c.PayTo != a.Account && at.AccountsID == c.PayTo) || (c.PayTo == a.Account && at.AccountsID == c.PayFrom)).FirstOrDefault()
                           let bd = db.Accountss.Where(at => (a.Type == DC.Credit && at.AccountsID == c.PayTo) || (a.Type == DC.Debit && at.AccountsID == c.PayFrom)).FirstOrDefault()
                           let acCount = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && z.Account == a.Account && z.Type == a.Type).Select(z => a.Account).Count()
                           let prev = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && a.Type != z.Type && (a.Type == DC.Credit && a.Id > z.Id) && z.Account != 501 && z.Account != 502).OrderByDescending(z => z.Id).Select(z => z.Account).FirstOrDefault()
                           let next = db.AccountsTransactions.Where(z => z.Purpose == "Journal" && z.reference == a.reference && a.Type != z.Type && (a.Type == DC.Debit && a.Id < z.Id) && z.Account != 501 && z.Account != 502).OrderBy(z => z.Id).Select(z => z.Account).FirstOrDefault()


                           where (fromdate == null || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) &&
                           (a.Account == AccId || (AccId == -1 && Accounts.Contains(a.Account))) &&
                           (todate == null || EF.Functions.DateDiffDay(c.Date, tdate) >= 0) && a.Purpose == "Journal"
                           && (pdc == true || a.Status == null)
                           select new
                           {
                               id = c.JournalId,
                               particulars = b.Name,
                               Date = (DateTime?)a.Date,

                               PdcDate = p.PDCDate,
                               chequeno = p.CheckNo,
                               bank = p.Bank,
                               Invoice = c.VoucherNo,
                               Type = "Journal Entry",
                               RAccount = (acCount <= 1) ? bd.Name : db.Accountss.Where(y => (a.Type == DC.Credit && y.AccountsID == prev) || (a.Type == DC.Debit && y.AccountsID == next)).Select(y => y.Name).FirstOrDefault(),
                               RAccountID = bd.AccountsID,
                               Debit = (decimal?)a.Debit,
                               Credit = (decimal?)a.Credit,
                               entry = (DateTime?)a.CreatedDate,
                               Remark = c.Remark,
                               Amount = (decimal)c.GrandTotal,
                               TRN = bd.TRN,
                               TransactionId = a.Id,
                               Account = a.Account,
                               reference = a.reference
                           });

            var full = Payment.Union(Reciept);

            full = full.Union(Journal);
            //full = full.Union(RecieptDiscount);
            //full = full.Union(PaymentDiscount);
            full = full.AsQueryable().OrderBy("Date asc, entry asc");
            vmodel.Ledger = (from a in full
                             select new Ledgerpro
                             {
                                 Date = a.Date,
                                 PdcDate = a.PdcDate,
                                 chequeno = a.chequeno,
                                 bank = a.bank,
                                 Invoice = a.Invoice,
                                 Type = a.Type,
                                 RAccount = a.RAccount,
                                 RAccountID = a.RAccountID,
                                 Debit = a.Debit,
                                 Credit = a.Credit,
                                 particulars = a.particulars,
                                 Remark = a.Remark,
                                 Amount = a.Amount,
                                 MainId = a.id,
                                 TRN = a.TRN,
                                 TransactionId = a.TransactionId,
                                 Account = a.Account,
                                 Reference = a.reference
                             }).ToList();
            return vmodel;
        }
        #region Common Print Methods From Controllers

        #region Print Sales Entry
        #endregion
     
        public static string GenerateZatcaBase64(string sellerName, string vatNumber, DateTime? timestamp, decimal? invoiceTotal, decimal? vatTotal)
        {
            List<byte> tlvBytes = new List<byte>();

            // ഓരോ ഫീൽഡും അതിന്റെ Tag നമ്പറിനൊപ്പം TLV ബൈറ്റുകളാക്കി മാറ്റുന്നു
            tlvBytes.AddRange(EncodeTlvField(1, sellerName));
            tlvBytes.AddRange(EncodeTlvField(2, (vatNumber == null) ? "" : vatNumber));
            tlvBytes.AddRange(EncodeTlvField(3, timestamp.Value.ToString("yyyy-MM-ddTHH:mm:sszzz")));
            tlvBytes.AddRange(EncodeTlvField(4, Convert.ToString(invoiceTotal)));
            tlvBytes.AddRange(EncodeTlvField(5, Convert.ToString(vatTotal)));

            // മുഴുവൻ ബൈറ്റുകളെയും ഒന്നിച്ച് ചേർത്ത് Base64 ലേക്ക് മാറ്റുന്നു
            return Convert.ToBase64String(tlvBytes.ToArray());
        }

        private static byte[] EncodeTlvField(int tag, string value)
        {
            // വാല്യൂവിനെ UTF-8 ബൈറ്റ് അറേ ആക്കുന്നു (അറബിക് സപ്പോർട്ടിനായി)
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);

            List<byte> fieldBytes = new List<byte>();

            fieldBytes.Add((byte)tag);               // 1. Tag (1 byte)
            fieldBytes.Add((byte)valueBytes.Length); // 2. Length (1 byte)
            fieldBytes.AddRange(valueBytes);         // 3. Value (Bytes)

            return fieldBytes.ToArray();
        }
        #region Print Sales Entry
        public pdfSummaryViewModel SaleData(long id, Status? PrintCode = null, Status? HideItmName = null, Status? PNoStatus = null, Int64? TOut = 0, Status? ProjectCheck = null, string ConvertFrom = "", string ConvertBill = "")
        {
            Int64 temp = 0;
            pdfSummaryViewModel vmodel = new pdfSummaryViewModel();
            vmodel = (from a in db.SalesEntrys
                      join b in db.Customers on a.Customer equals b.CustomerID into cust
                      from b in cust.DefaultIfEmpty()
                      join c in db.Contacts on b.Contact equals c.ContactID into cnt
                      from c in cnt.DefaultIfEmpty()
                      join d in db.SEPayments on a.SalesEntryId equals d.SalesEntry into pay
                      from d in pay.DefaultIfEmpty()
                      join e in db.Employees on a.SECashier equals e.EmployeeId into user
                      from e in user.DefaultIfEmpty()
                      join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                      from f in paymeth.DefaultIfEmpty()
                      join g in db.HireDetails on new { g1 = a.SalesEntryId, g2 = "Sales" }
                      equals new { g1 = g.Reference, g2 = g.Section } into hir
                      from g in hir.DefaultIfEmpty()
                      join h in db.HireTypes on g.HireType equals h.HireTypeId into Htype
                      from h in Htype.DefaultIfEmpty()
                      join p in db.Projects on a.Project equals p.ProjectId into prjct
                      from p in prjct.DefaultIfEmpty()
                      join t in db.ProTasks on a.ProTask equals t.ProTaskId into ptask
                      from t in ptask.DefaultIfEmpty()
                      join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                      from i in acc.DefaultIfEmpty()
                      join k in db.Contacts on e.PAddress equals k.ContactID into empcon
                      from k in empcon.DefaultIfEmpty()
                      join l in db.Mobiles on b.Contact equals l.Contact into mobi
                      from l in mobi.DefaultIfEmpty()
                      join rrr in db.ContactRelation on new { g1 = a.Customer, g2 = temp } equals new { g1 = rrr.RelationID, g2 = rrr.RelationType } into cnr
                      from rrr in cnr.DefaultIfEmpty()
                      join co in db.Contacts on rrr.ContactID equals co.ContactID into coo
                      from co in coo.DefaultIfEmpty()
                      join sr in db.SalesReturns on a.SalesEntryId equals sr.SalesEntryId into srr
                      from sr in srr.DefaultIfEmpty()
                      let bon = db.customerbonus.Where(o => o.salesentryid == id).Select(o => o.claimamount).FirstOrDefault()

                      where (a.SalesEntryId == id)

                      select new pdfSummaryViewModel
                      {
                          PartyName = (b.CustomerPrintName == null) ? b.CustomerName : b.CustomerPrintName,
                          customercode = b.CustomerCode,
                          BillNo = a.BillNo,
                          salesretunid = sr.SalesReturnId,
                          mc = a.MaterialCenter,
                          Date = a.SEDate,
                          Note = a.SENote,
                          Cashier = e.FirstName,
                          Discount = a.SEDiscount,
                          GrandTotal = a.SEGrandTotal,
                          Paid = d.SEPaidAmount,
                          Balance = a.SEGrandTotal - d.SEPaidAmount,
                          SubTotal = a.SESubTotal,
                          TaxAmount = a.SETaxAmount,
                          Address = b.Addres.Replace(System.Environment.NewLine, "<br/>"),
                          City = c.City,
                          State = c.State,
                          Country = c.Country,
                          Zip = c.Zip,
                          Email = c.EmailId,
                          Phone = co.Mobile,
                          Mobile = "",//c.Mobile,
                          bonusclaimed = bon,
                          mobmodel = (from ac in db.Mobiles
                                      where (ac.Contact == b.Contact)
                                      select new MobileViewModel
                                      {
                                          Num = ac.MobileNum,
                                          Name = ac.Name
                                      }).ToList(),
                          settlment = (from aa in db.accountmaps
                                       join bb in db.Accountss
                                       on aa.AccountId equals bb.AccountsID
                                       where (aa.EmployeeId == a.SECashier)
                                       select new msettlement
                                       {

                                           PaymentType = aa.PaymentTypeId,
                                           Amount = Math.Round((db.AccountsTransactions.Where(x => x.reference == a.SalesEntryId && x.Purpose == "Sale Payment" && x.Account == aa.AccountId).Select(a => a.Debit).FirstOrDefault()), 2),


                                       }).ToList(),
                          TRN = b.TaxID_TRN,
                          // paytype = (a.CustomerType == CustomerType.Walking ? ((a.PaymentMethod == null || a.PaymentMethod == 0) ? "Cash" : f.MethodName) : "Credit"),
                          paytype = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                          CreditPeriod = b.CreditPeriod,

                          BillId = a.SENo,
                          PONo = a.PONo,
                          ConvertType = a.ConvertType,
                          ConvertNo = a.ConvertNo,
                          Location = a.Location,
                          Remarks = a.Remarks,
                          chkCode = PrintCode,
                          HideItemName = HideItmName,
                          Currency = a.Currency,
                          ConvertionRate = a.ConvertionRate,
                          FCTotal = a.FCTotal,
                          TimeOut = TOut,
                          SaleType = a.SaleType,
                          SalesType = a.SalesType,
                          HireType = h.Name,
                          FromDate = g.StartDate,
                          ToDate = g.EndDate,
                          ContactPerson = c.ContactPerson,
                          HSCode = a.HSCode,
                          PaymentTerms = a.PaymentTerms,
                          ProCheck = ProjectCheck,
                          PrjNameCode = (p.ProjectName != null && p.ProjectName != "") ? p.ProCode + "-" + p.ProjectName : "",
                          TaskCode = (t.TaskCode != null && t.TaskCode != "") ? t.TaskCode : "",
                          TaskName = (t.TaskName != null && t.TaskName != "") ? t.TaskName : "",
                          ConvertFrom = ConvertFrom,
                          ConvertBill = ConvertBill,
                          Ref1 = (a.Ref1 == "") ? ((t.Ref1 != "") ? t.Ref1 : a.Ref1) : a.Ref1,
                          Ref2 = (a.Ref2 == "") ? ((t.Ref2 != "") ? t.Ref2 : a.Ref2) : a.Ref2,
                          Ref3 = (a.Ref3 == "") ? ((t.Ref3 != "") ? t.Ref3 : a.Ref3) : a.Ref3,
                          Ref4 = (a.Ref4 == "") ? ((t.Ref4 != "") ? t.Ref4 : a.Ref4) : a.Ref4,
                          Ref5 = (a.Ref5 == "") ? ((t.Ref5 != "") ? t.Ref5 : a.Ref5) : a.Ref5,
                          ContactNo = k.Phone,
                          empemail = k.EmailId,
                          CreatedDate = a.SECreatedDate,


                      }).FirstOrDefault();
            vmodel.zatcaBase64 = GenerateZatcaBase64(vmodel.PartyName, vmodel.TRN, vmodel.CreatedDate, vmodel.GrandTotal, vmodel.TaxAmount);

            vmodel.pdfItem = (from b in db.SEItemss
                              join c in db.Items on b.Item equals c.ItemID
                              join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                              from d in scaffold.DefaultIfEmpty()
                              join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                              from e in punit.DefaultIfEmpty()
                                  //join z in db.BatchStocks on new { f1 = c.ItemID, f2 = b.SalesEntry,f3="Sales" } equals new { f1 = z.Item, f2 = z.Reference,f3=z.Type  }  into bat
                                  //from z in bat.DefaultIfEmpty()
                                  //join z in db.BatchStocks on b.Item equals z.Item into bat
                                  //from z in bat.DefaultIfEmpty()

                              join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                              from g in bundle.DefaultIfEmpty()
                              let chkbom = db.BillOfMaterials.Where(a => a.ItemId == b.Item).FirstOrDefault()
                              let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
                              where b.SalesEntry == id && b.itemNote != "-:{Bundle_Item}" && b.Type == false
                              select new pdfItemViewModel
                              {
                                  Id = c.ItemID,
                                  Barcode = c.Barcode,
                                  ItemUnitPrice = b.ItemUnitPrice,
                                  ItemQuantity = b.ItemQuantity,
                                  ItemSubTotal = b.ItemSubTotal,
                                  ItemNote = b.itemNote,
                                  ItemTax = b.ItemTax,
                                  ItemTaxAmount = b.ItemTaxAmount,
                                  ItemTotalAmount = b.ItemTotalAmount,
                                  ItemDiscount = b.ItemDiscount,
                                  ItemCode = c.ItemCode,
                                  ItemName = c.ItemName,
                                  ItemUnit = e.ItemUnitName,
                                  PartNumber = c.PartNumber,
                                  PNoStatus = PNoStatus,
                                  CBM = d.CBM,
                                  Weight = d.Weight,
                                  img = img,
                                  ItemDescription = c.ItemDescription,
                                  KeepStock = c.KeepStock,
                                  BomExist = chkbom != null ? true : false,
                                  InSaleInvoice = c.InSaleInvoice,
                                  BatchNo = id.ToString(),
                                  bundle = (from ay in db.BundleItems
                                            join az in db.ItemBundles on ay.ItemBundle equals az.ItemBundleId
                                            //join ab in db.QuotationItems on ay.ItemId equals ab.Item into quot
                                            //from ab in quot.DefaultIfEmpty()
                                            join bb in db.Items on ay.ItemId equals bb.ItemID
                                            join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                            from dd in scaffold.DefaultIfEmpty()
                                            join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
                                            from eb in bpunit.DefaultIfEmpty()
                                            let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                            where az.mainItem == b.Item
                                            select new pdfBundleViewModel
                                            {
                                                Id = bb.ItemID,
                                                ItemUnitPrice = ay.ItemUnitPrice,
                                                ItemQuantity = (ay.ItemQuantity * b.ItemQuantity),
                                                ItemSubTotal = (ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice),
                                                ItemNote = "",
                                                ItemTax = ay.ItemTax,
                                                ItemTaxAmount = ((ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice) * ay.ItemTax / 100),
                                                ItemTotalAmount = ay.ItemTotalAmount,
                                                ItemCode = bb.ItemCode,
                                                ItemName = bb.ItemName,
                                                ItemUnit = eb.ItemUnitName,
                                                PartNumber = bb.PartNumber,
                                                PNoStatus = PNoStatus,
                                                CBM = dd.CBM,
                                                Weight = dd.Weight,
                                                img = bimg,
                                                KeepStock = bb.KeepStock,
                                                InSaleInvoice = bb.InSaleInvoice,
                                                Item = ay.ItemId,
                                                ItemDiscount = 0,
                                                ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                ItemUnitID = bb.ItemUnitID,
                                                SubUnitId = bb.SubUnitId,
                                                ItemArabic = bb.ItemArabic,
                                                ItemDescription = bb.ItemDescription
                                            }).ToList(),
                              }).ToList();
            vmodel.billsundry = (from b in db.SEBillSundrys
                                 //join c in db.BillSundrys on b.BillSundry equals c.BillSundryId
                                 let bs = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
                                 where b.SalesEntry == id
                                 select new pdfBillSundryViewModel
                                 {
                                     AmountType = b.AmountType,
                                     BsAmount = b.BsAmount,
                                     BsType = b.BsType,
                                     BsValue = b.BsValue != null ? b.BsValue : 0,
                                     BillSundry = bs
                                 }).ToList();
            if (vmodel.ConvertType == "DVNote" && ConvertFrom == "" && (ConvertBill == "" || ConvertBill == null))
            {
                vmodel.ConvertBill = vmodel.ConvertNo;
                vmodel.ConvertFrom = vmodel.ConvertType + " No";
            }
            if (vmodel.ConvertType == "DVNote")
            {
                string[] tempstring = vmodel.ConvertNo.Split(',');
                string temp2 = tempstring[0];
                vmodel.DVNoteDate = db.Deliverynotes.Where(c => c.BillNo == temp2).Select(c => c.DvDate).FirstOrDefault();

            }

            return vmodel;
        }

        public void setsalesprofit(string seno, decimal? bonusclimed)
        {
            db.SetCommandTimeOut(60 * 60);
            var enbonusforcustomer = db.EnableSettings.Where(a => a.EnableType == "bonusforcustomer").FirstOrDefault();
            var bonusforcustomer = enbonusforcustomer != null ? enbonusforcustomer.Status : Status.inactive;
            if (bonusforcustomer == Status.inactive)
            {
                return;
            }
            SalesEntry e = db.SalesEntrys.Where(o => o.BillNo == seno).FirstOrDefault();
            Customer cus = db.Customers.Find(e.Customer);
            if (cus.bonuscheck == false)
            {
                return;

            }


            //var draw = Request.Form.GetValues("draw").FirstOrDefault();
            //var start = Request.Form.GetValues("start").FirstOrDefault();
            //var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            //int pageSize = length != null ? Convert.ToInt32(length) : 0;
            //int skip = start != null ? Convert.ToInt32(start) : 0;

            SaleType St = new SaleType();

            DateTime? fdate = null;
            DateTime? tdate = null;


            DateTime? hfrmdate = null;
            DateTime? htodate = null;

            string srchtxt = "";
            Int64 sac = 1;
            var v = (from a in db.SalesEntrys
                     join b in db.Customers on a.Customer equals b.CustomerID into cust
                     from b in cust.DefaultIfEmpty()
                     join c in db.SEPayments on a.SalesEntryId equals c.SalesEntry
                     join d in db.Employees on a.SECashier equals d.EmployeeId into emp
                     from d in emp.DefaultIfEmpty()
                     join f in db.PaymentMethods on a.PaymentMethod equals f.PaymentMethodId into paymeth
                     from f in paymeth.DefaultIfEmpty()
                     join g in db.MCs on a.MaterialCenter equals g.MCId into mcs
                     from g in mcs.DefaultIfEmpty()
                     join h in db.HireDetails on new { h1 = a.SalesEntryId, h2 = "Sales" }
                     equals new { h1 = h.Reference, h2 = h.Section } into hir
                     from h in hir.DefaultIfEmpty()
                     join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                     from i in acc.DefaultIfEmpty()
                     join j in db.AccountsTransactions on new { j1 = a.SalesEntryId, j2 = "Sale", j3 = sac }
                        equals new { j1 = j.reference, j2 = j.Purpose, j3 = j.Account } into hir1
                     from j in hir1.DefaultIfEmpty()

                     where (a.BillNo == seno) && a.Status == 1
                     select new
                     {

                         a.SalesEntryId,
                         a.SENo,
                         a.BillNo,
                         a.SEDate,
                         SEGrandTotal = (srchtxt == "") ? a.SEGrandTotal : 0,
                         SETaxAmount = (srchtxt == "") ? a.SETaxAmount : 0,

                         Customer = b.CustomerName,
                         TaxRegNo = i.TRN,
                         EmpName = d.FirstName + " " + d.LastName,
                         MCName = g.MCName,
                         SEPaidAmount = (srchtxt == "") ? c.SEPaidAmount : 0,
                         a.CustomerType,
                         SEBalanceAmount = (srchtxt == "") ? (a.SEGrandTotal - c.SEPaidAmount) : 0,
                         //for expense
                         PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == a.BillNo).Select(x => x.Paying).Sum()),
                         salesreturns = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 != "Credit Note").Select(x => x.SRSubTotal).Sum()),
                         salesreturnsnote = (decimal?)(db.SalesReturns.Where(o => o.SalesEntryId == a.SalesEntryId && o.Ref5 == "Credit Note").Select(x => x.SRGrandTotal).Sum()),

                         discountt = (decimal?)(from ayy in db.BillSundrys
                                                join azz in db.SEBillSundrys on ayy.BillSundryId equals azz.BillSundry

                                                where ayy.BSName == "DISCOUNT" && azz.SalesEntry == a.SalesEntryId
                                                select new
                                                {
                                                    BsAmount = (azz.BsAmount == null) ? 0 : azz.BsAmount
                                                }
                                   ).Sum(x => x.BsAmount) ?? 0,

                         JournalExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == a.BillNo).Select(y => y.Paying).Sum()),

                         a.SECreatedDate,
                         PayMethod = (a.CustomerType == CustomerType.Card ? f.MethodName : (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit")),
                         SaleType = a.SaleType,
                         FromDate = h.StartDate,
                         ToDate = h.EndDate,
                         HireType = h.HireType,
                         a.SalesStatus,
                         j.Credit,

                     }).AsEnumerable().Select(o => new
                     {
                         o.SalesEntryId,
                         o.SENo,
                         o.Credit,
                         o.BillNo,
                         o.SEDate,

                         o.SEGrandTotal,
                         o.SETaxAmount,
                         o.Customer,
                         o.TaxRegNo,
                         o.discountt,
                         o.EmpName,
                         o.MCName,
                         o.SEPaidAmount,
                         o.CustomerType,
                         o.SEBalanceAmount,
                         NewExpense = (srchtxt == "") ? ((o.PaymentExpense == null && o.JournalExpense == null) ? 0 :
                         ((o.PaymentExpense != null && o.JournalExpense != null) ? o.PaymentExpense + o.JournalExpense : ((o.PaymentExpense == null && o.JournalExpense != null) ? o.JournalExpense : o.PaymentExpense))) : 0,
                         salesrtn = (o.salesreturns == null) ? 0 : o.salesreturns,
                         salesrtnnote = (o.salesreturnsnote == null) ? 0 : o.salesreturnsnote,

                         //Calling Function To Get Total Item Price for each Sales Entry
                         itemprice = GetTotalItemPrice(o.SalesEntryId, o.SEDate, srchtxt),
                         // = (GetTotalEmpCost(o.SalesEntryId) * perhourcost) : GetTotalEmpCostwithrate(o.SalesEntryId),
                         o.SECreatedDate,
                         o.PayMethod,
                         o.SaleType,
                         o.FromDate,
                         o.ToDate,
                         o.HireType,
                         o.SalesStatus,
                     }).ToList().FirstOrDefault();
            if (v != null)
            {
                customerbonus cb = new customerbonus();
                cb.customerid = cus.CustomerID;
                cb.expenses = v.NewExpense;
                cb.materialcost = v.itemprice;
                cb.invoiceamountwithouttax = v.Credit;
                var profit = (v.Credit) - (v.itemprice) - (v.NewExpense) - (v.salesrtn) - (v.salesrtnnote);
                cb.netprofit = profit;
                cb.salesentryid = e.SalesEntryId;
                cb.salesreturn = v.salesrtn;
                if (bonusclimed != null)
                {
                    cb.claimamount = bonusclimed;
                }
                if (cus.bonusbaseamount == BonusBase.SalesProfit)
                {
                    cb.climableamount = (cb.netprofit * cus.bonuspercentage / 100) * cus.bonusclimembility / 100;

                }
                else
                {
                    cb.climableamount = (cb.invoiceamountwithouttax * cus.bonuspercentage / 100) * cus.bonusclimembility / 100;
                }
                db.customerbonus.RemoveRange(db.customerbonus.Where(o => o.salesentryid == cb.salesentryid));
                db.SaveChanges();
                db.customerbonus.Add(cb);
                db.SaveChanges();
            }
            //var Data = new Dictionary<string, object>();
            //Data.Add("profit", v);

            //return Data;




        }
        public decimal GetTotalItemPrice(long? SalesEntryId, DateTime? SEDate, string srchtxt)
        {

            //Getting All Items In Sales Entry
            var ItemList = (from se in db.SEItemss
                            join seen in db.SalesEntrys on se.SalesEntry equals seen.SalesEntryId
                            join seit in db.Items on se.Item equals seit.ItemID
                            //join srentry in db.SalesReturns on seen.SalesEntryId equals srentry.SalesEntryId into dftf
                            //from srentry in dftf.DefaultIfEmpty()
                            //join srit in db.SRItemss on srentry.SalesReturnId equals srit.SalesReturnId into sritt
                            //from srit in sritt.DefaultIfEmpty()



                            where se.SalesEntry == SalesEntryId && (seit.KeepStock == true || seit.accmap == true) &&
                            (srchtxt == "" || seit.ItemName.Contains(srchtxt))
                            select new
                            {
                                SEDate = seen.SEDate,
                                DetailId = se.SEItemsId,
                                ItemId = se.Item,
                                seItemUnit = se.ItemUnit,
                                seen.SalesEntryId,
                                seItemQuantity = se.ItemQuantity,
                                seitItemUnitID = seit.ItemUnitID,
                                seitPurchasePrice = seit.PurchasePrice,
                                seitConFactor = seit.ConFactor,
                                seen.MaterialCenter,
                                se.Type,
                                ItemUnitPrice = (seit.SubUnitId == se.ItemUnit) ? seit.SellingPrice / seit.ConFactor : seit.SellingPrice,

                            }).AsEnumerable().Select(o => new
                            {
                                o.SEDate,
                                o.DetailId,
                                o.ItemId,
                                o.seItemUnit,
                                o.seItemQuantity,
                                o.seitItemUnitID,
                                o.seitPurchasePrice,
                                o.seitConFactor,

                                o.SalesEntryId,
                                o.Type,
                                retnqty = getsalesreturn(o.SalesEntryId, o.ItemId),
                                ItemUnitPrice = o.Type == true ? o.ItemUnitPrice : 0,
                                //Calling Function To Get Item Purchase Price (If Exists Any With in SEDate)for each Item
                                NewPurchPrice = GetItemPurchasePrice(o.ItemId, o.SEDate, o.MaterialCenter, o.DetailId, false)

                            }).Select(s => new
                            {
                                s.SEDate,
                                s.DetailId,
                                s.ItemId,
                                s.seItemUnit,
                                s.seItemQuantity,
                                s.seitItemUnitID,
                                s.seitPurchasePrice,
                                s.seitConFactor,
                                s.ItemUnitPrice,
                                s.Type,
                                s.retnqty,
                                // reutrnprice= (s.Type == true) ? Math.Round(Math.Round(s.ItemUnitPrice, 2) * s.ret, 2) : ((s.seItemUnit == s.seitItemUnitID) ? ((s.NewPurchPrice == 0) ? (s.seitPurchasePrice * s.seItemQuantity) : (s.NewPurchPrice * s.seItemQuantity)) : (((s.NewPurchPrice == 0) ? (s.seitPurchasePrice * s.ret) : (s.NewPurchPrice * s.re)) / s.seitConFactor)),
                                //Calculating ItemPrice * Quantity(If Secondary Unit Exists ==> Considering Conversion Factor)
                                ItemPrice = (1 == 2) ? Math.Round(Math.Round(s.ItemUnitPrice, 2) * (s.seItemQuantity - s.retnqty), 2) : ((s.seItemUnit == s.seitItemUnitID) ? ((s.NewPurchPrice == 0) ? (s.seitPurchasePrice * (s.seItemQuantity - s.retnqty)) : (s.NewPurchPrice * (s.seItemQuantity - s.retnqty))) : (((s.NewPurchPrice == 0) ? (s.seitPurchasePrice * (s.seItemQuantity - s.retnqty)) : (s.NewPurchPrice * (s.seItemQuantity - s.retnqty))) / s.seitConFactor))

                            }).ToList();

            var j = 0;
            decimal ItemPrice = 0;

            //Taking Sum of Item Price ==> Item Price Of Each Item
            for (j = 0; j < ItemList.Count; j++)
            {
                ItemPrice = Convert.ToDecimal(ItemPrice + ItemList[j].ItemPrice);
            }

            return ItemPrice;
        }
        public decimal getsalesreturn(long salesentryid, long itemid)
        {
            var v = (from a in db.SalesReturns
                     join b in db.SRItemss on a.SalesReturnId equals b.SalesReturnId
                     where a.SalesEntryId == salesentryid && b.Item == itemid
                     select new
                     {
                         b.ItemQuantity
                     }
                  ).ToList();
            if (v.Count() <= 0)
                return 0;
            else if (v.Count() == 1)
                return v.Sum(o => o.ItemQuantity);
            else
            {
                return v.Average(o => o.ItemQuantity);
            }

        }
        public decimal getsalesreturndetailid(string billno, long itemid)
        {
            var exist = (from a in db.SalesEntrys
                         join b in db.SalesReturns on a.SalesEntryId equals b.SalesEntryId
                         join c in db.SRItemss on b.SalesReturnId equals c.SalesReturnId
                         where a.BillNo == billno && c.Item == itemid
                         select new
                         { c.ItemQuantity })
                         .Any();
            if (!exist)
            {
                return 0;
            }
            var salesentryid = db.SalesEntrys.Where(o => o.BillNo == billno).Select(o => o.SalesEntryId).FirstOrDefault();
            var v = (from a in db.SalesReturns
                     join b in db.SRItemss on a.SalesReturnId equals b.SalesReturnId
                     join c in db.Items on b.Item equals c.ItemID
                     where a.SalesEntryId == salesentryid && b.Item == itemid
                     select new
                     {
                         ItemQuantity = (c.ItemUnitID == b.ItemUnit) ? b.ItemQuantity * c.ConFactor : b.ItemQuantity
                     }
                  ).ToList();
            if (v.Count() <= 0)
                return 0;
            else if (v.Count() == 1)
                return v.Sum(o => o.ItemQuantity);
            else
            {
                return v.Average(o => o.ItemQuantity);
            }

        }
        public decimal GetItemPurchasePriceold(long ItemId, DateTime? SEDate, long? mc)
        {
            decimal confactor = 1;
            var items = db.Items.Where(o => o.ItemID == ItemId).FirstOrDefault();

            confactor = 1;
            bool isquicknet = db.companys.Any(o => o.CPName.Contains("QUICK NET COMPUTERS"));
            DateTime fromdate = SEDate.Value.AddMonths(-7);
            var NewPurPrice = (from aa in db.PEItemss
                               join bb in db.PurchaseEntrys on aa.PurchaseEntry equals bb.PurchaseEntryId
                               where (aa.Item == ItemId &&
                               bb.PEDate >= fromdate &&

                               bb.PEDate <= SEDate) &&
                               bb.MaterialCenter == mc
                               orderby bb.PEDate descending
                               select new
                               {
                                   unitprice = aa.ItemUnitPrice,
                                   date = bb.PEDate


                               }).FirstOrDefault();
            var newstocktransfer = (from aa in db.StockTransferItems
                                    join bb in db.StockTransfers on aa.StockTransferId equals bb.Id
                                    where (aa.Item == ItemId &&
                                       bb.Date >= fromdate &&
                                    bb.Date <= SEDate) &&
                                    bb.MCTo == mc
                                    orderby bb.Date descending
                                    select new
                                    {
                                        unitprice = aa.Price,
                                        date = bb.Date,
                                        aa.Unit,
                                        aa.Quantity,


                                    }).FirstOrDefault();

            if (newstocktransfer == null && NewPurPrice == null)
            {
                decimal a = 0;
                return items.PurchasePrice / confactor;
            }
            if (newstocktransfer != null && NewPurPrice != null)
            {
                if (newstocktransfer.date > NewPurPrice.date)
                {
                    if (newstocktransfer.Unit == items.SubUnitId && !isquicknet)
                    {
                        var actualprice = NewPurPrice.unitprice;// (newstocktransfer.unitprice / newstocktransfer.Quantity) * items.ConFactor;
                        return actualprice;
                    }
                    else
                    {
                        return newstocktransfer.unitprice / confactor;
                    }
                }
                else
                {
                    return NewPurPrice.unitprice / confactor;
                }
            }
            else if (newstocktransfer != null)
            {
                return newstocktransfer.unitprice / confactor;
            }
            else if (NewPurPrice != null)
            {
                return NewPurPrice.unitprice / confactor;
            }
            else
            {
                decimal a = 0;
                return items.PurchasePrice / confactor;
            }
        }
        public decimal GetItemPurchasePriceoldnew(long ItemId, DateTime? SEDate)
        {
            decimal confactor = 1;
            var items = db.Items.Where(o => o.ItemID == ItemId).FirstOrDefault();

            confactor = 1;

            DateTime fromdate = SEDate.Value.AddMonths(-7);
            var NewPurPrice = (from aa in db.PEItemss
                               join bb in db.PurchaseEntrys on aa.PurchaseEntry equals bb.PurchaseEntryId
                               where (aa.Item == ItemId &&
                               bb.PEDate >= fromdate &&

                               bb.PEDate <= SEDate)
                               orderby bb.PEDate descending
                               select new
                               {
                                   unitprice = aa.ItemUnitPrice,
                                   date = bb.PEDate


                               }).FirstOrDefault();
            var newstocktransfer = (from aa in db.StockTransferItems
                                    join bb in db.StockTransfers on aa.StockTransferId equals bb.Id
                                    where (aa.Item == ItemId &&
                                       bb.Date >= fromdate &&
                                    bb.Date <= SEDate)
                                    orderby bb.Date descending
                                    select new
                                    {
                                        unitprice = aa.Price,
                                        date = bb.Date,
                                        aa.Unit,
                                        aa.Quantity,


                                    }).FirstOrDefault();

            if (newstocktransfer == null && NewPurPrice == null)
            {
                decimal a = 0;
                return items.PurchasePrice / confactor;
            }
            if (newstocktransfer != null && NewPurPrice != null)
            {
                if (newstocktransfer.date > NewPurPrice.date)
                {
                    if (newstocktransfer.Unit == items.SubUnitId)
                    {
                        var actualprice = NewPurPrice.unitprice;// (newstocktransfer.unitprice / newstocktransfer.Quantity) * items.ConFactor;
                        return actualprice;
                    }
                    else
                    {
                        return newstocktransfer.unitprice / confactor;
                    }
                }
                else
                {
                    return NewPurPrice.unitprice / confactor;
                }
            }
            else if (newstocktransfer != null)
            {
                return newstocktransfer.unitprice / confactor;
            }
            else if (NewPurPrice != null)
            {
                return NewPurPrice.unitprice / confactor;
            }
            else
            {
                decimal a = 0;
                return items.PurchasePrice / confactor;
            }
        }
        public decimal GetItemPurchasePrice(long? ItemId, DateTime? SEDate, long? mc, long? salesentrydetailid, bool moment = false)
        {
            decimal confactor = 1;
            decimal confactoract = 1;
            decimal sellqty = 0;
            var sellingunit = db.SEItemss.Where(o => o.SEItemsId == salesentrydetailid).Select(o => o.ItemUnit).FirstOrDefault();
            var items = db.Items.Where(o => o.ItemID == ItemId).FirstOrDefault();
            if (items.ConFactor != null)
            {
                confactor = items.ConFactor;
            }
            if (sellingunit == items.SubUnitId)
            {
                confactoract = confactor;
            }
            if (salesentrydetailid != null)
                sellqty = db.SEItemss.Where(o => o.SEItemsId == salesentrydetailid).Select(o => o.ItemQuantity).FirstOrDefault();
            else
                sellqty = 0;
            DateTime fromdate = SEDate.Value.AddMonths(-3);
            List<DateTime> stockin = new List<DateTime>();


            moment = false;
            //if (moment==true)
            //{
            //    var selitem = new SqlParameter("@ItemId", ItemId);
            //    var selmc = new SqlParameter("@MCId", mc);
            //    var brand = new SqlParameter("@BrandId", "0");
            //    var stkble = new SqlParameter("@Stockble", "");
            //    var catgry = new SqlParameter("@CategoryId", "0");
            //    var fromdatte = new SqlParameter("@fromdate", "");
            //    var todate = new SqlParameter("@todate", SEDate);
            //    var stype = new SqlParameter("@Stype", "0");

            //    // var cust = new SqlParameter("@Customer", DBNull.Value);
            //    var data = db.Database.SqlQueryDedup<StockDataDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdatte, todate, stype).OrderBy(a => a.TDate).ToList();
            //    var cost = data.OrderByDescending(o => o.TDate).Select(o=>o.BCost).FirstOrDefault();

            //    if (cost > 0)
            //    {

            //        return (decimal)cost / confactor;
            //    }
            //    else
            //        return 0;


            //}
            var selitem = new SqlParameter("@ItemId", ItemId);
            var selmc = new SqlParameter("@MCId", mc);
            var brand = new SqlParameter("@BrandId", "0");
            var stkble = new SqlParameter("@Stockble", "");
            var catgry = new SqlParameter("@CategoryId", "0");
            var fromdatee = new SqlParameter("@fromdate", fromdate);
            var todate = new SqlParameter("@todate", SEDate.Value.AddDays(30));
            var stype = new SqlParameter("@Stype", "0");


            var data = db.Database.SqlQueryDedup<StockDataDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@Stype", selitem, selmc, brand, stkble, catgry, fromdatee, todate, stype).OrderBy(a => a.TDate).ToList();
            var data2 = data.Where(o => (o.TItemId != salesentrydetailid && (o.TItemType == "Purchase" || o.TItemType == "Stock Received" || o.TItemType == "Stock Receivedadj"))).ToList().Select
                (o => new batchstock
                {
                    TDate = o.TDate,
                    OQty = o.Qty,
                    BQty = o.Qty,
                    UnitPrice = o.UnitPrice,
                    TItemType = o.TItemType,
                    currstock = o.Qty,
                    confactor = confactor,
                    transactiondid = o.TItemId,
                    itemid = o.ItemId
                }
                ).OrderBy(o => o.TDate).ToList();
            //for (int j = 0; j < data2.Count() - 1; j++)
            //{
            //    var lastprice = data2[j].UnitPrice;
            //    if (data2[j + 1].UnitPrice == 0)
            //    {
            //        var sedate = getsaledetailid((long)ItemId, data2[j + 1].transactiondid);

            //        if (sedate == null)
            //        {
            //            data2[j + 1].UnitPrice = lastprice;
            //        }
            //        else
            //        {
            //            data2[j + 1].UnitPrice = GetItemPurchasePrice(ItemId, sedate, mc, data2[j + 1].transactiondid);
            //        }
            //        }
            //}

            var data3 = data.Where(o => (o.TItemId != salesentrydetailid && o.TDate <= SEDate.Value.AddDays(-1) && (o.TItemType == "Sales" || o.TItemType == "Stock Transfered" || o.TItemType == "Stock Transferedadj" || o.TItemType == "Purchase Return")))
                .GroupBy(x => new { x.TDate, x.TItemType, x.UnitPrice, x.ItemId, x.Invoice }, (key, group) => new batchstock

                {

                    TDate = key.TDate,
                    OQty = group.Sum(o => o.Qty),
                    BQty = group.Sum(o => o.Qty),
                    UnitPrice = key.UnitPrice,
                    TItemType = key.TItemType,
                    currstock = group.Sum(o => o.Qty),
                    confactor = confactor,
                    transactiondid = group.Max(o => o.TItemId),
                    itemid = key.ItemId,
                    invoice = key.Invoice

                }).ToList()


                .Select
                (o => new batchstock
                {
                    TDate = o.TDate,
                    OQty = o.OQty - getsalesreturndetailid(o.invoice, o.itemid),
                    BQty = o.OQty,
                    UnitPrice = o.UnitPrice,
                    TItemType = o.TItemType,
                    currstock = o.OQty,
                    confactor = confactor,
                    transactiondid = o.itemid,
                    itemid = o.itemid
                }
                ).OrderBy(o => o.TDate).ToList();

            //var data4 = data2.Where(o => o.TItemType == "Sales Return").ToList();
            //foreach(var d4 in data4)
            //{

            //    var salesentrydetid = salesreturnseid(d4.transactiondid, d4.itemid);
            //      if(salesentrydetid > 0)
            //    {
            //        // data2.Where(o => o.TItemType == "Sales Return" && o.transactiondid == d4.transactiondid && o.itemid == d4.itemid).ToList().ForEach(o => o.OQty = 0);
            //        data2.Remove(d4);
            //         data3.Where(o => o.TItemType == "Sales" && o.transactiondid == salesentrydetid && o.itemid == d4.itemid).ToList().ForEach(o => o.OQty = o.OQty-d4.OQty);

            //    }
            //}


            decimal? sumseqty = 0;

            int i = 0;
            foreach (var dt3 in data3)
            {
                if (data2.Count() > i)
                {

                    sumseqty = sumseqty + dt3.OQty;

                    if (sumseqty >= data2[i].OQty)
                    {
                        sumseqty = sumseqty - data2[i].OQty;/// data2[i].confactor);
                        data2[i].currstock = 0;
                        if ((i + 1) < data2.Count())
                        {
                            data2[i + 1].OQty = data2[i + 1].OQty - sumseqty;
                            sumseqty = 0;
                            data2[i + 1].BQty = data2[i + 1].OQty;
                            data2[i + 1].currstock = data2[i + 1].OQty;

                        }

                        i = i + 1;
                    }
                    else
                    {

                        data2[i].currstock = data2[i].currstock - dt3.OQty;
                    }
                }
            }
            var finaldata = data2.Where(o => o.currstock > 0).ToList();
            decimal? mcvalue = 0;
            var sellqtyorg = sellqty / confactoract;
            int flag = 0;
            foreach (var fidt in finaldata)
            {
                if (sellqty <= 0)
                    break;
                if ((sellqty / confactoract) >= (fidt.currstock / confactor))
                {

                    mcvalue = mcvalue + (decimal)(((decimal)fidt.currstock / confactor) * (decimal)fidt.UnitPrice);
                    sellqty = (sellqty / confactoract) - (decimal)fidt.currstock / confactor;
                }
                else
                {

                    mcvalue = mcvalue + (decimal)(sellqty / confactoract * (decimal)fidt.UnitPrice);
                    sellqty = (sellqty / confactoract) - (decimal)fidt.currstock / confactor;

                }



            }
            if (mcvalue > 0)
            {

                //#if DEBUG
                //                TelemetryConfiguration.Active.DisableTelemetry = true;
                //#endif
                // System.Diagnostics.Debug.WriteLine(Convert.ToString( sellqtyorg)); 
                return Convert.ToDecimal(mcvalue / sellqtyorg);
            }
            else
            {
                var NewPurPrice = (from aa in db.PEItemss
                                   join bb in db.PurchaseEntrys on aa.PurchaseEntry equals bb.PurchaseEntryId
                                   where (aa.Item == ItemId &&
                                   bb.PEDate >= fromdate &&

                                   bb.PEDate <= SEDate) &&
                                   bb.MaterialCenter == mc
                                   orderby bb.PEDate descending
                                   select new
                                   {
                                       unitprice = aa.ItemUnitPrice,
                                       date = bb.PEDate


                                   }).FirstOrDefault();
                var newstocktransfer = (from aa in db.StockTransferItems
                                        join bb in db.StockTransfers on aa.StockTransferId equals bb.Id
                                        where (aa.Item == ItemId &&
                                           bb.Date >= fromdate &&
                                        bb.Date <= SEDate) &&
                                        bb.MCTo == mc
                                        orderby bb.Date descending
                                        select new
                                        {
                                            unitprice = aa.Price,
                                            date = bb.Date,


                                        }).FirstOrDefault();


                if (newstocktransfer == null && NewPurPrice == null)
                {
                    decimal a = 0;
                    return items.PurchasePrice / confactoract;
                }
                if (newstocktransfer != null && NewPurPrice != null)
                {
                    if (newstocktransfer.date > NewPurPrice.date)
                    {
                        return newstocktransfer.unitprice / confactoract;
                    }
                    else
                    {
                        return NewPurPrice.unitprice / confactoract;
                    }
                }
                else if (newstocktransfer != null)
                {
                    return newstocktransfer.unitprice / confactoract;
                }
                else if (NewPurPrice != null)
                {
                    return NewPurPrice.unitprice / confactoract;
                }
                else
                {
                    decimal a = 0;
                    return items.PurchasePrice / confactoract;
                }

            }


        }



        #endregion
        #region print Sales Return Details
        public Dictionary<string, object> SalesReturnData(long id, Status? PrintCode = null, Status? PNoStatus = null, Int64? TOut = 0, Status? ProjectCheck = null)
        {
            var summary = (from a in db.SalesReturns
                           join b in db.Customers on a.Customer equals b.CustomerID into cust
                           from b in cust.DefaultIfEmpty()
                           join c in db.Contacts on b.Contact equals c.ContactID into cnt
                           from c in cnt.DefaultIfEmpty()
                           join d in db.SRPayments on a.SalesReturnId equals d.SalesReturnId into pay
                           from d in pay.DefaultIfEmpty()
                           join e in db.Employees on a.SRCashier equals e.EmployeeId into user
                           from e in user.DefaultIfEmpty()
                           join f in db.SalesEntrys on a.SalesEntryId equals f.SalesEntryId into sale
                           from f in sale.DefaultIfEmpty()
                           join p in db.Projects on a.Project equals p.ProjectId into prjct
                           from p in prjct.DefaultIfEmpty()
                           join t in db.ProTasks on a.Project equals t.ProjectId into ptask
                           from t in ptask.DefaultIfEmpty()
                           join i in db.Accountss on b.Accounts equals i.AccountsID into acc
                           from i in acc.DefaultIfEmpty()
                           join j in db.Contacts on e.PAddress equals j.ContactID into empcon
                           from j in empcon.DefaultIfEmpty()
                           join k in db.Mobiles on b.Contact equals k.Contact into mobi
                           from k in mobi.DefaultIfEmpty()
                           where a.SalesReturnId == id
                           select new
                           {
                               PartyName = b.CustomerName,
                               BillNo = a.BillNo,
                               Date = a.SRDate,
                               Note = a.SRNote,
                               Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                               Discount = a.SRDiscount,
                               Total = a.SRDiscount + a.SRGrandTotal,
                               GrandTotal = a.SRGrandTotal,
                               Paid = (d != null) ? d.SReturnAmount : 0,
                               Balance = a.SRGrandTotal - ((d != null) ? d.SReturnAmount : 0),
                               SubTotal = a.SRSubTotal,
                               TaxAmount = a.SRTaxAmount,
                               b.Addres,
                               c.City,
                               c.State,
                               c.Country,
                               c.Zip,
                               Email = c.EmailId,
                               Phone = c.Phone,
                               Mobile = k.MobileNum,// c.Mobile,
                               TRN = b.TaxID_TRN,
                               paytype = (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit"),
                               BillId = a.SalesReturnId,
                               a.Remarks,
                               chkCode = PrintCode,
                               TimeOut = TOut,
                               ContactPerson = c.ContactPerson,
                               AgainstInvoice = (a.ReturnType == 0) ? f.BillNo : null,
                               ProCheck = ProjectCheck,
                               PrjNameCode = (p.ProjectName != null && p.ProjectName != "") ? p.ProCode + "-" + p.ProjectName : "",
                               TaskName = (t.TaskName != null && t.TaskName != "") ? t.TaskName : "",
                               Ref1 = a.Ref1,
                               Ref2 = a.Ref2,
                               Ref3 = a.Ref3,
                               Ref4 = a.Ref4,
                               Ref5 = a.Ref5,
                               ContactNo = j.Phone,
                               CreatedDate = a.SRCreatedDate
                           }).FirstOrDefault();
            var item = (from b in db.SRItemss
                        join c in db.Items on b.Item equals c.ItemID

                        join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                        from d in scaffold.DefaultIfEmpty()
                        join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                        from e in punit.DefaultIfEmpty()

                        join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                        from g in bundle.DefaultIfEmpty()
                        let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new { im.FileName, im.Status, im.ItemImageID }).ToList()
                        where b.SalesReturnId == id && b.itemNote != "-:{Bundle_Item}"
                        select new
                        {
                            ItemUnitPrice = b.ItemUnitPrice,
                            ItemQuantity = b.ItemQuantity,
                            ItemSubTotal = b.ItemSubTotal,
                            ItemNote = b.itemNote,
                            ItemTax = b.ItemTax,
                            ItemTaxAmount = b.ItemTaxAmount,
                            ItemTotalAmount = b.ItemTotalAmount,
                            ItemDiscount = b.ItemDiscount,
                            ItemCode = c.ItemCode,
                            ItemName = c.ItemName,
                            ItemUnit = e.ItemUnitName,
                            PartNumber = c.PartNumber,
                            PNoStatus = PNoStatus,
                            CBM = d.CBM,
                            Weight = d.Weight,
                            img = img,
                            c.ItemDescription,
                            KeepStock = c.KeepStock,
                            bundle = (from ay in db.BundleItems
                                      join az in db.ItemBundles on ay.ItemBundle equals az.ItemBundleId
                                      //join ab in db.QuotationItems on ay.ItemId equals ab.Item into quot
                                      //from ab in quot.DefaultIfEmpty()
                                      join bb in db.Items on ay.ItemId equals bb.ItemID
                                      join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                      from dd in scaffold.DefaultIfEmpty()
                                      join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
                                      from eb in bpunit.DefaultIfEmpty()
                                      let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new { bim.FileName, bim.Status, bim.ItemImageID }).ToList()
                                      where az.mainItem == b.Item
                                      select new
                                      {
                                          Id = bb.ItemID,
                                          ItemUnitPrice = ay.ItemUnitPrice,
                                          ItemQuantity = (ay.ItemQuantity * b.ItemQuantity),
                                          ItemSubTotal = (ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice),
                                          ItemNote = "",
                                          ItemTax = ay.ItemTax,
                                          ItemTaxAmount = ((ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice) * ay.ItemTax / 100),
                                          ItemTotalAmount = ay.ItemTotalAmount,
                                          ItemCode = bb.ItemCode,
                                          ItemName = bb.ItemName,
                                          ItemUnit = eb.ItemUnitName,
                                          PartNumber = bb.PartNumber,
                                          PNoStatus = PNoStatus,
                                          CBM = dd.CBM,
                                          Weight = dd.Weight,
                                          img = bimg,
                                          KeepStock = bb.KeepStock,
                                          Item = ay.ItemId,
                                          ItemDiscount = 0,
                                          ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                          bb.ItemUnitID,
                                          bb.SubUnitId,
                                          bb.ItemArabic,
                                          bb.ItemDescription
                                      }).ToList()
                        }).ToList();
            //    bundle = (from ab in db.SRItemss
            //              join bb in db.Items on ab.Item equals bb.ItemID
            //              join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
            //              from dd in scaffold.DefaultIfEmpty()

            //              join eb in db.ItemUnits on ab.ItemUnit equals eb.ItemUnitID into bpunit
            //              from eb in bpunit.DefaultIfEmpty()

            //              let bimg = db.ItemImages.Where(bim => bim.ItemID == ab.Item).Select(bim => new { bim.FileName, bim.Status, bim.ItemImageID }).ToList()
            //              where ab.SalesReturnId == id && ab.itemNote == "-:{Bundle_Item}"
            //              && b.Item == ab.ItemDiscount
            //              select new
            //              {
            //                  Id = bb.ItemID,
            //                  ItemUnitPrice = ab.ItemUnitPrice,
            //                  ItemQuantity = ab.ItemQuantity,
            //                  ItemSubTotal = ab.ItemSubTotal,
            //                  ItemNote = "",
            //                  ItemTax = ab.ItemTax,
            //                  ItemTaxAmount = ab.ItemTaxAmount,
            //                  ItemTotalAmount = ab.ItemTotalAmount,

            //                  ItemCode = bb.ItemCode,
            //                  ItemName = bb.ItemName,
            //                  ItemUnit = eb.ItemUnitName,
            //                  PartNumber = bb.PartNumber,
            //                  PNoStatus = PNoStatus,
            //                  CBM = dd.CBM,
            //                  Weight = dd.Weight,
            //                  img = bimg,


            //                  KeepStock = bb.KeepStock,

            //                  ab.Item,
            //                  ItemDiscount = 0,
            //                  ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
            //                  bb.ItemUnitID,
            //                  bb.SubUnitId,
            //                  bb.ItemArabic,
            //                  bb.ItemDescription
            //              }).ToList(),
            //}).ToList();
            var billsundry = db.SRBillSundrys.Where(n => n.SalesReturnId == id).Select(b => new
            {
                AmountType = b.AmountType,
                BsAmount = b.BsAmount,
                BsType = b.BsType,
                BsValue = b.BsValue != null ? b.BsValue : 0,
                BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
            }).ToList();

            var Data = new Dictionary<string, object>();
            Data.Add("summary", summary);
            Data.Add("item", item);
            Data.Add("billsundry", billsundry);
            return Data;
        }
        #endregion

        #region  Print SalesOrder Details
        public pdfSummaryViewModel SalesOrderData(long id, Status? PrintCode = null, Status? PNoStatus = null, Int64? TOut = 0, Status? ProjectCheck = null, Status? ComHeadCheck = null)
        {
            pdfSummaryViewModel vmodel = new pdfSummaryViewModel();
            vmodel = (from b in db.SalesOrders
                      join c in db.Customers on b.Customer equals c.CustomerID into cust
                      from c in cust.DefaultIfEmpty()
                      join d in db.Contacts on c.Contact equals d.ContactID into cnt
                      from d in cnt.DefaultIfEmpty()
                      join e in db.Employees on b.SOCashier equals e.EmployeeId into emp
                      from e in emp.DefaultIfEmpty()
                      join g in db.HireDetails on new { g1 = b.SalesOrderId, g2 = "Sales order" }
                      equals new { g1 = g.Reference, g2 = g.Section } into hir
                      from g in hir.DefaultIfEmpty()
                      join h in db.HireTypes on g.HireType equals h.HireTypeId into Htype
                      from h in Htype.DefaultIfEmpty()
                      join p in db.Projects on b.Project equals p.ProjectId into prjct
                      from p in prjct.DefaultIfEmpty()
                      join t in db.ProTasks on b.Project equals t.ProjectId into ptask
                      from t in ptask.DefaultIfEmpty()
                      join f in db.Accountss on c.Accounts equals f.AccountsID into acc
                      from f in acc.DefaultIfEmpty()
                      join i in db.Contacts on e.PAddress equals i.ContactID into empcon
                      from i in empcon.DefaultIfEmpty()
                      join k in db.Mobiles on c.Contact equals k.Contact into mobi
                      from k in mobi.DefaultIfEmpty()
                      where b.SalesOrderId == id
                      select new pdfSummaryViewModel
                      {
                          PartyName = c.CustomerName,
                          
                          BillNo = b.BillNo,
                          Date = b.SODate,
                          Note = b.TermsCondition,
                          Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                          Discount = b.SODiscount,
                          GrandTotal = b.SOGrandTotal,
                          SubTotal = b.SOSubTotal,
                          TaxAmount = b.SOTaxAmount,
                          Address = c.Addres,
                          City = d.City,
                          State = d.State,
                          Country = d.Country,
                          Zip = d.Zip,
                          Email = d.EmailId,
                          Phone = d.Phone,
                          Mobile = k.MobileNum,// d.Mobile,
                          TRN = c.TaxRegNo,
                          PTax = b.SOTax,
                          validity = (DateTime.Now <= b.SODate.AddDays((b.SOValidity == null) ? 0 : (b.SOValidity.Value + 1))) ? "Active" : "Expired",
                          BillId = b.SONo,
                          Remarks = b.Remarks,
                          chkCode = PrintCode,
                          TimeOut = TOut,
                          SaleType = b.SaleType,
                          HireType = h.Name,
                          FromDate = g.StartDate,
                          ToDate = g.EndDate,
                          ContactPerson = d.ContactPerson,
                          ProCheck = ProjectCheck,
                          PrjNameCode = (p.ProjectName != null && p.ProjectName != "") ? p.ProCode + "-" + p.ProjectName : "",
                          TaskName = (t.TaskName != null && t.TaskName != "") ? t.TaskName : "",
                          ComHeadCheck = ComHeadCheck,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          ContactNo = i.Phone,
                          CreatedDate = b.SOCreatedDate
                      }).FirstOrDefault();

            vmodel.pdfItem = (from b in db.SalesOrderItems
                              join c in db.Items on b.Item equals c.ItemID

                              join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                              from d in scaffold.DefaultIfEmpty()
                              join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                              from e in punit.DefaultIfEmpty()

                              join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                              from g in bundle.DefaultIfEmpty()
                              let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
                              where b.SalesOrder == id && b.ItemNote != "-:{Bundle_Item}"
                              select new pdfItemViewModel
                              {
                                  ItemUnitPrice = b.ItemUnitPrice,
                                  ItemQuantity = b.ItemQuantity,
                                  ItemSubTotal = b.ItemSubTotal,
                                  ItemTax = b.ItemTax,
                                  ItemNote = b.ItemNote,
                                  ItemTaxAmount = b.ItemTaxAmount,
                                  ItemTotalAmount = b.ItemTotalAmount,
                                  ItemDiscount = b.ItemDiscount,
                                  ItemCode = c.ItemCode,
                                  ItemName = c.ItemName,
                                  ItemUnit = e.ItemUnitName,
                                  PartNumber = c.PartNumber,
                                  PNoStatus = PNoStatus,
                                  CBM = d.CBM,
                                  Weight = d.Weight,
                                  img = img,
                                  ItemDescription = c.ItemDescription,
                                  KeepStock = c.KeepStock,
                                  bundle = (from ay in db.BundleItems
                                            join az in db.ItemBundles on ay.ItemBundle equals az.ItemBundleId
                                            //join ab in db.QuotationItems on ay.ItemId equals ab.Item into quot
                                            //from ab in quot.DefaultIfEmpty()
                                            join bb in db.Items on ay.ItemId equals bb.ItemID
                                            join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                            from dd in scaffold.DefaultIfEmpty()
                                            join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
                                            from eb in bpunit.DefaultIfEmpty()
                                            let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                            where az.mainItem == b.Item
                                            select new pdfBundleViewModel
                                            {
                                                Id = bb.ItemID,
                                                ItemUnitPrice = ay.ItemUnitPrice,
                                                ItemQuantity = (ay.ItemQuantity * b.ItemQuantity),
                                                ItemSubTotal = (ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice),
                                                ItemNote = "",
                                                ItemTax = ay.ItemTax,
                                                ItemTaxAmount = ((ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice) * ay.ItemTax / 100),
                                                ItemTotalAmount = ay.ItemTotalAmount,
                                                ItemCode = bb.ItemCode,
                                                ItemName = bb.ItemName,
                                                ItemUnit = eb.ItemUnitName,
                                                PartNumber = bb.PartNumber,
                                                PNoStatus = PNoStatus,
                                                CBM = dd.CBM,
                                                Weight = dd.Weight,
                                                img = bimg,
                                                KeepStock = bb.KeepStock,
                                                Item = ay.ItemId,
                                                ItemDiscount = 0,
                                                ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                ItemUnitID = bb.ItemUnitID,
                                                SubUnitId = bb.SubUnitId,
                                                ItemArabic = bb.ItemArabic,
                                                ItemDescription = bb.ItemDescription
                                            }).ToList(),

                              }).ToList();

            return vmodel;
        }

        #endregion

        #region Print Quotation Details
        public pdfSummaryViewModel QuotationData(long id, Status? PNoStatus = null, Status? PrintCode = null, Int64? TOut = 0, Status? ProjectCheck = null, Status? ComHeadCheck = null, long? checkid = 0)
        {
            var chk = checkid;
            pdfSummaryViewModel vmodel = new pdfSummaryViewModel();
            vmodel = (from b in db.Quotations
                      join c in db.Customers on b.Customer equals c.CustomerID into cust
                      from c in cust.DefaultIfEmpty()
                      join bb in db.QuotationTypes on b.quotationtype equals bb.QuotId into qutt
                      from bb in qutt.DefaultIfEmpty()
                      join st in db.States on c.StateID equals st.StateID into stt
                      from st in stt.DefaultIfEmpty()
                      join lc in db.LocationNames on st.StateID equals lc.StateId into lcc
                      from lc in lcc.DefaultIfEmpty()


                          //join d in db.ContactRelation on c.CustomerID equals d.RelationID into cnt
                          //from d in cnt.DefaultIfEmpty()

                      join e in db.Employees on b.QuotCashier equals e.EmployeeId into emp
                      from e in emp.DefaultIfEmpty()
                      join f in db.Projects on b.Project equals f.ProjectId into prjct
                      from f in prjct.DefaultIfEmpty()
                      where b.QuotationId == id
                      join g in db.HireDetails on new { g1 = b.QuotationId, g2 = "Quotation" }
                      equals new { g1 = g.Reference, g2 = g.Section } into hir
                      from g in hir.DefaultIfEmpty()
                      join h in db.HireTypes on g.HireType equals h.HireTypeId into Htype
                      from h in Htype.DefaultIfEmpty()
                      join t in db.ProTasks on b.Project equals t.ProjectId into ptask
                      from t in ptask.DefaultIfEmpty()
                      join i in db.Accountss on c.Accounts equals i.AccountsID into acc
                      from i in acc.DefaultIfEmpty()
                      join j in db.Contacts on e.PAddress equals j.ContactID into empcon
                      from j in empcon.DefaultIfEmpty()
                      join k in db.Mobiles on c.Contact equals k.Contact into mobi
                      from k in mobi.DefaultIfEmpty()
                      join l in db.CurrencyMasters on b.Currency equals l.Id into curr
                       
                      from  l in  curr.DefaultIfEmpty()
                      let cnt = (from d in db.ContactRelation
                                 join c in db.Customers on d.RelationID equals c.CustomerID
                                 join con in db.Contacts on d.ContactID equals con.ContactID
                                 join qtn in db.Quotations on c.CustomerID equals qtn.Customer
                                 where qtn.QuotationId == id && d.RelationType == 0
                                 select con
                                 ).FirstOrDefault()

                      select new pdfSummaryViewModel
                      {
                          chid = checkid,
                          PartyName = (c.CustomerPrintName == null) ? c.CustomerName : c.CustomerPrintName,
                          BillNo = b.BillNo,
                          qutationtype=bb.QuotType,
                          Date = b.QuotDate,
                          Note = b.TermsCondition,
                          Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,//+"\n"+j.EmailId,
                          Discount = b.QuotDiscount,
                          GrandTotal = b.QuotGrandTotal,
                          SubTotal = b.QuotSubTotal,
                          TaxAmount = b.QuotTaxAmount,
                          Address = c.Addres,
                          ProCheck = ProjectCheck,
                          PrjNameCode = (f.ProjectName != null && f.ProjectName != "") ? f.ProCode + "-" + f.ProjectName : "",
                          City = "",
                          State = "",
                          Country = "U.A.E",
                          Zip = "",
                          Email = cnt.EmailId,
                          Phone = "",
                          Mobile = cnt.Mobile,
                          TRN = c.TaxID_TRN,
                          PTax = b.QuotTax,
                          Remarks = b.Remarks,
                          validity = (DateTime.Now <= DbFunctionsCompat.AddDays(b.QuotDate, (b.QuotValidity == null) ? 0 : b.QuotValidity + 1)) ? "Active" : "Expired",
                          BillId = b.QuotNo,
                          chkCode = PrintCode,
                          TimeOut = TOut,
                          SaleType = b.SaleType,
                          HireType = h.Name,
                          FromDate = g.StartDate,
                          ToDate = g.EndDate,
                          ContactPerson = cnt.FirstName,
                          PaymentTerms = b.PaymentTerms,
                          TaskName = (t.TaskName != null && t.TaskName != "") ? t.TaskName : "",
                          TaskCode = (t.TaskCode != null && t.TaskCode != "") ? t.TaskCode : "",
                          ComHeadCheck = ComHeadCheck,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          ContactNo = j.Phone,
                          empemail = j.EmailId,
                          CreatedDate = b.QuotCreatedDate,
                          revision = b.revision,
                          validityqtn = b.QuotValidity,
                               Currency = b.Currency,
                          ConvertionRate = b.ConvertionRate,
                          FCTotal = b.FCTotal,
                         currencycode=l.CurrencyCode,
                         currencysymbol=l.Symbol

                        

                      }).FirstOrDefault();
            //var pdfHeading = (

            //                       from a in db.QuotationItemsHeading
            //                       join b in db.QuotationItems on a.Quotation equals b.Quotation
            //                       join c in db.Items on b.Item equals c.ItemID


            //                       join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
            //                       from d in scaffold.DefaultIfEmpty()
            //                       join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
            //                       from e in punit.DefaultIfEmpty()

            //                       join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
            //                       from g in bundle.DefaultIfEmpty()
            //                       let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
            //                       where b.Quotation == id && b.ItemNote != "-:{Bundle_Item}"
            //                       && a.ItemNote!=null
            //                       select new pdfItemViewModel
            //                       {
            //                           Id = c.ItemID,
            //                           InSaleInvoice = c.InSaleInvoice,
            //                           ItemUnitPrice = b.ItemUnitPrice,
            //                           ItemQuantity = b.ItemQuantity,
            //                           ItemSubTotal = b.ItemSubTotal,
            //                           ItemNote = a.ItemNote,
            //                           ItemTax = b.ItemTax,
            //                           ItemTaxAmount = b.ItemTaxAmount,
            //                           ItemTotalAmount = b.ItemTotalAmount,
            //                           ItemDiscount = b.ItemDiscount,
            //                           ItemCode = c.ItemCode,
            //                           ItemName = c.ItemName,
            //                           ItemUnit = e.ItemUnitName,
            //                           PartNumber = c.PartNumber,
            //                           PNoStatus = PNoStatus,
            //                           CBM = d.CBM,
            //                           Weight = d.Weight,
            //                           img = img,
            //                           ItemDescription = c.ItemDescription,
            //                           bundle = (from ay in db.BundleItems
            //                                     join az in db.ItemBundles on ay.ItemBundle equals az.ItemBundleId
            //                                     //join ab in db.QuotationItems on ay.ItemId equals ab.Item into quot
            //                                     //from ab in quot.DefaultIfEmpty()
            //                                     join bb in db.Items on ay.ItemId equals bb.ItemID
            //                                     join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
            //                                     from dd in scaffold.DefaultIfEmpty()
            //                                     join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
            //                                     from eb in bpunit.DefaultIfEmpty()
            //                                     let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
            //                                     where az.mainItem == b.Item
            //                                     select new pdfBundleViewModel
            //                                     {

            //                                         Id = bb.ItemID,
            //                                         ItemUnitPrice = ay.ItemUnitPrice,
            //                                         ItemQuantity = (ay.ItemQuantity * b.ItemQuantity),
            //                                         ItemSubTotal = (ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice),
            //                                         ItemNote = "",
            //                                         ItemTax = ay.ItemTax,
            //                                         ItemTaxAmount = ((ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice) * ay.ItemTax / 100),
            //                                         ItemTotalAmount = ay.ItemTotalAmount,
            //                                         ItemCode = bb.ItemCode,
            //                                         ItemName = bb.ItemName,
            //                                         ItemUnit = eb.ItemUnitName,
            //                                         PartNumber = bb.PartNumber,
            //                                         PNoStatus = PNoStatus,
            //                                         CBM = dd.CBM,
            //                                         Weight = dd.Weight,
            //                                         img = bimg,
            //                                         KeepStock = bb.KeepStock,
            //                                         Item = ay.ItemId,
            //                                         ItemDiscount = 0,
            //                                         ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
            //                                         ItemUnitID = bb.ItemUnitID,
            //                                         SubUnitId = bb.SubUnitId,
            //                                         ItemArabic = bb.ItemArabic,
            //                                         ItemDescription = bb.ItemDescription
            //                                     }).ToList(),


            //                       }).ToList();
            vmodel.pdfItem = (from b in db.QuotationItems
                              join c in db.Items on b.Item equals c.ItemID
                             

                              join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                              from d in scaffold.DefaultIfEmpty()
                              join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                              from e in punit.DefaultIfEmpty()

                              join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                              from g in bundle.DefaultIfEmpty()
                              let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
                              where b.Quotation == id && b.ItemNote != "-:{Bundle_Item}"
                              select new pdfItemViewModel
                              {
                                  Id = c.ItemID,
                                  InSaleInvoice = c.InSaleInvoice,
                                  ItemUnitPrice = b.ItemUnitPrice,
                                  ItemQuantity = b.ItemQuantity,
                                  ItemSubTotal = b.ItemSubTotal,
                                  ItemNote = b.ItemNote,
                                  ItemTax = b.ItemTax,
                                  ItemTaxAmount = b.ItemTaxAmount,
                                  ItemTotalAmount = b.ItemTotalAmount,
                                  ItemDiscount = b.ItemDiscount,
                                  ItemCode = c.ItemCode,
                                  ItemName = c.ItemName,
                                  ItemUnit = e.ItemUnitName,
                                  PartNumber = c.PartNumber,
                                  PNoStatus = PNoStatus,
                                  CBM = d.CBM,
                                  Weight = d.Weight,
                                  img = img,
                                  ItemDescription = c.ItemDescription,
                                  bundle = (from ay in db.BundleItems
                                            join az in db.ItemBundles on ay.ItemBundle equals az.ItemBundleId
                                            //join ab in db.QuotationItems on ay.ItemId equals ab.Item into quot
                                            //from ab in quot.DefaultIfEmpty()
                                            join bb in db.Items on ay.ItemId equals bb.ItemID
                                            join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                            from dd in scaffold.DefaultIfEmpty()
                                            join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
                                            from eb in bpunit.DefaultIfEmpty()
                                            let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                            where az.mainItem == b.Item
                                            select new pdfBundleViewModel
                                            {

                                                Id = bb.ItemID,
                                                ItemUnitPrice = ay.ItemUnitPrice,
                                                ItemQuantity = (ay.ItemQuantity * b.ItemQuantity),
                                                ItemSubTotal = (ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice),
                                                ItemNote = "",
                                                ItemTax = ay.ItemTax,
                                                ItemTaxAmount = ((ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice) * ay.ItemTax / 100),
                                                ItemTotalAmount = ay.ItemTotalAmount,
                                                ItemCode = bb.ItemCode,
                                                ItemName = bb.ItemName,
                                                ItemUnit = eb.ItemUnitName,
                                                PartNumber = bb.PartNumber,
                                                PNoStatus = PNoStatus,
                                                CBM = dd.CBM,
                                                Weight = dd.Weight,
                                                img = bimg,
                                                KeepStock = bb.KeepStock,
                                                Item = ay.ItemId,
                                                ItemDiscount = 0,
                                                ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                ItemUnitID = bb.ItemUnitID,
                                                SubUnitId = bb.SubUnitId,
                                                ItemArabic = bb.ItemArabic,
                                                ItemDescription = bb.ItemDescription
                                            }).ToList(),
                           
                              }).ToList();
         
            vmodel.billsundry = db.QtBillSundrys.Where(n => n.Quotation == id).Select(b => new pdfBillSundryViewModel
            {
                AmountType = b.AmountType,
                BsAmount = b.BsAmount,
                BsType = b.BsType,
                BsValue = b.BsValue != null ? b.BsValue : 0,
                BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
            }).ToList();

            return vmodel;

        }
        #endregion
        public pdfSummaryViewModel WarrantyCertificateData(long id, Status? PNoStatus = null, Status? PrintCode = null, Int64? TOut = 0, Status? ProjectCheck = null, Status? ComHeadCheck = null, long? checkid = 0)
        {
            var chk = checkid;
            pdfSummaryViewModel vmodel = new pdfSummaryViewModel();
            vmodel = (from b in db.WarrantyCertificates
                      join c in db.Customers on b.Customer equals c.CustomerID into cust
                      from c in cust.DefaultIfEmpty()
                          //join st in db.States on c.StateID equals st.StateID into stt
                          //from st in stt.DefaultIfEmpty()
                          //join lc in db.LocationNames on st.StateID equals lc.StateId into lcc
                          //from lc in lcc.DefaultIfEmpty()


                          //join d in db.ContactRelation on c.CustomerID equals d.RelationID into cnt
                          //from d in cnt.DefaultIfEmpty()

                      join e in db.Employees on b.WCashier equals e.EmployeeId into emp
                      from e in emp.DefaultIfEmpty()

                      where b.WarrantyId == id
                      //join g in db.HireDetails on new { g1 = b.QuotationId, g2 = "Quotation" }
                      //equals new { g1 = g.Reference, g2 = g.Section } into hir
                      //from g in hir.DefaultIfEmpty()
                      //join h in db.HireTypes on g.HireType equals h.HireTypeId into Htype
                      //from h in Htype.DefaultIfEmpty()
                      //join t in db.ProTasks on b.Project equals t.ProjectId into ptask
                      //from t in ptask.DefaultIfEmpty()
                      //join i in db.Accountss on c.Accounts equals i.AccountsID into acc
                      //from i in acc.DefaultIfEmpty()
                      //join j in db.Contacts on e.PAddress equals j.ContactID into empcon
                      //from j in empcon.DefaultIfEmpty()
                      //join k in db.Mobiles on c.Contact equals k.Contact into mobi
                      //from k in mobi.DefaultIfEmpty()
                      //let cnt = (from d in db.ContactRelation
                      //           join c in db.Customers on d.RelationID equals c.CustomerID
                      //           join con in db.Contacts on d.ContactID equals con.ContactID
                      //           join qtn in db.Quotations on c.CustomerID equals qtn.Customer
                      //           where qtn.QuotationId == id && d.RelationType == 0
                      //           select con
                      //           ).FirstOrDefault()

                      select new pdfSummaryViewModel
                      {
                          chid = checkid,
                          PartyName = (c.CustomerPrintName == null) ? c.CustomerName : c.CustomerPrintName,
                          BillNo = b.BillNo,
                          Date = b.WDate,
                          Note = b.WNote,
                          Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,//+"\n"+j.EmailId,
                          Discount = b.WDiscount,
                          GrandTotal = b.WGrandTotal,
                          SubTotal = b.WSubTotal,
                          TaxAmount = b.WTaxAmount,
                          Address = c.Addres,
                          ProCheck = ProjectCheck,
                          //PrjNameCode = (f.ProjectName != null && f.ProjectName != "") ? f.ProCode + "-" + f.ProjectName : "",
                          //City = lc.Location,
                          //State = st.StateName,
                          Country = "U.A.E",
                          Zip = "",
                          //Email = cnt.EmailId,
                          Phone = "",
                          //Mobile = cnt.Mobile,
                          TRN = c.TaxID_TRN,
                          PTax = b.WTax,
                          Remarks=b.WNote,
                          //validity = (DateTime.Now <= DbFunctionsCompat.AddDays(b.QuotDate, (b.QuotValidity == null) ? 0 : b.QuotValidity + 1)) ? "Active" : "Expired",
                          //BillId = Convert.ToInt32(b.BillNo),
                          chkCode = PrintCode,
                          TimeOut = TOut,
                          //SaleType = b.SaleType,
                          //HireType = h.Name,
                          //FromDate = g.StartDate,
                          //ToDate = g.EndDate,
                          //ContactPerson = cnt.FirstName,
                          //PaymentTerms = b.PaymentTerms,
                          //TaskName = (t.TaskName != null && t.TaskName != "") ? t.TaskName : "",
                          //TaskCode = (t.TaskCode != null && t.TaskCode != "") ? t.TaskCode : "",
                          ComHeadCheck = ComHeadCheck,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          //ContactNo = j.Phone,
                          //empemail = j.EmailId,
                          //CreatedDate = b.QuotCreatedDate
                      }).FirstOrDefault();
            vmodel.pdfItem = (from b in db.WItems
                              join c in db.Items on b.Item equals c.ItemID

                              join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                              from d in scaffold.DefaultIfEmpty()
                              join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                              from e in punit.DefaultIfEmpty()

                              join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                              from g in bundle.DefaultIfEmpty()
                              let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
                              where b.Warranty == id && b.itemNote != "-:{Bundle_Item}"
                              select new pdfItemViewModel
                              {
                                  Id = c.ItemID,
                                  InSaleInvoice = c.InSaleInvoice,
                                  WarrantyPeriod = b.WarrantyPeriod,
                                  ItemUnitPrice = b.ItemUnitPrice,
                                  ItemQuantity = b.ItemQuantity,
                                  ItemSubTotal = b.ItemSubTotal,
                                  ItemNote = b.itemNote,
                                  ItemTax = b.ItemTax,
                                  ItemTaxAmount = b.ItemTaxAmount,
                                  ItemTotalAmount = b.ItemTotalAmount,
                                  ItemDiscount = b.ItemDiscount,
                                  ItemCode = c.ItemCode,
                                  ItemName = c.ItemName,
                                  ItemUnit = e.ItemUnitName,
                                  PartNumber = c.PartNumber,
                                  PNoStatus = PNoStatus,
                                  CBM = d.CBM,
                                  Weight = d.Weight,
                                  img = img,
                                  ItemDescription = c.ItemDescription,
                                  //bundle = (from ay in db.BundleItems
                                  //          join az in db.ItemBundles on ay.ItemBundle equals az.ItemBundleId
                                  //          //join ab in db.QuotationItems on ay.ItemId equals ab.Item into quot
                                  //          //from ab in quot.DefaultIfEmpty()
                                  //          join bb in db.Items on ay.ItemId equals bb.ItemID
                                  //          join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                  //          from dd in scaffold.DefaultIfEmpty()
                                  //          join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
                                  //          from eb in bpunit.DefaultIfEmpty()
                                  //          let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                  //          where az.mainItem == b.Item
                                  //          select new pdfBundleViewModel
                                  //          {

                                  //              Id = bb.ItemID,
                                  //              ItemUnitPrice = ay.ItemUnitPrice,
                                  //              ItemQuantity = (ay.ItemQuantity * b.ItemQuantity),
                                  //              ItemSubTotal = (ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice),
                                  //              ItemNote = "",
                                  //              ItemTax = ay.ItemTax,
                                  //              ItemTaxAmount = ((ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice) * ay.ItemTax / 100),
                                  //              ItemTotalAmount = ay.ItemTotalAmount,
                                  //              ItemCode = bb.ItemCode,
                                  //              ItemName = bb.ItemName,
                                  //              ItemUnit = eb.ItemUnitName,
                                  //              PartNumber = bb.PartNumber,
                                  //              PNoStatus = PNoStatus,
                                  //              CBM = dd.CBM,
                                  //              Weight = dd.Weight,
                                  //              img = bimg,
                                  //              KeepStock = bb.KeepStock,
                                  //              Item = ay.ItemId,
                                  //              ItemDiscount = 0,
                                  //              ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                  //              ItemUnitID = bb.ItemUnitID,
                                  //              SubUnitId = bb.SubUnitId,
                                  //              ItemArabic = bb.ItemArabic,
                                  //              ItemDescription = bb.ItemDescription
                                  //          }).ToList(),
                                  //bundle = (from ab in db.QuotationItems

                                  //          join bb in db.Items on ab.Item equals bb.ItemID
                                  //          join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                  //          from dd in scaffold.DefaultIfEmpty()
                                  //          join eb in db.ItemUnits on ab.ItemUnit equals eb.ItemUnitID into bpunit
                                  //          from eb in bpunit.DefaultIfEmpty()
                                  //          let bimg = db.ItemImages.Where(bim => bim.ItemID == ab.Item).Select(bim => new { bim.FileName, bim.Status, bim.ItemImageID }).ToList()
                                  //          where ab.Quotation == id && ab.ItemNote == "-:{Bundle_Item}"
                                  //          && b.Item == ab.ItemDiscount
                                  //          select new
                                  //          {
                                  //              Id = bb.ItemID,
                                  //              ItemUnitPrice = ab.ItemUnitPrice,
                                  //              ItemQuantity = ab.ItemQuantity,
                                  //              ItemSubTotal = ab.ItemSubTotal,
                                  //              ItemNote = "",
                                  //              ItemTax = ab.ItemTax,
                                  //              ItemTaxAmount = ab.ItemTaxAmount,
                                  //              ItemTotalAmount = ab.ItemTotalAmount,
                                  //              ItemCode = bb.ItemCode,
                                  //              ItemName = bb.ItemName,
                                  //              ItemUnit = eb.ItemUnitName,
                                  //              PartNumber = bb.PartNumber,
                                  //              PNoStatus = PNoStatus,
                                  //              CBM = dd.CBM,
                                  //              Weight = dd.Weight,
                                  //              img = bimg,
                                  //              KeepStock = bb.KeepStock,
                                  //              ab.Item,
                                  //              ItemDiscount = 0,
                                  //              ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                  //              bb.ItemUnitID,
                                  //              bb.SubUnitId,
                                  //              bb.ItemArabic,
                                  //              bb.ItemDescription
                                  //          }).ToList().Distinct(),
                              }).ToList();

            vmodel.billsundry = db.WBillSundries.Where(n => n.Warranty == id).Select(b => new pdfBillSundryViewModel
            {
                AmountType = b.AmountType,
                BsAmount = b.BsAmount,
                BsType = b.BsType,
                BsValue = b.BsValue != null ? b.BsValue : 0,
                BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
            }).ToList();

            return vmodel;

        }
        public pdfSummaryViewModel WarrantyEntryData(long id, Status? PNoStatus = null, Status? PrintCode = null, Int64? TOut = 0, Status? ProjectCheck = null, Status? ComHeadCheck = null, long? checkid = 0)
        {
            var chk = checkid;
            pdfSummaryViewModel vmodel = new pdfSummaryViewModel();
            vmodel = (from b in db.WarrantyEntries
                      join c in db.Customers on b.Customer equals c.CustomerID into cust
                      from c in cust.DefaultIfEmpty()
                          //join st in db.States on c.StateID equals st.StateID into stt
                          //from st in stt.DefaultIfEmpty()
                          //join lc in db.LocationNames on st.StateID equals lc.StateId into lcc
                          //from lc in lcc.DefaultIfEmpty()


                          //join d in db.ContactRelation on c.CustomerID equals d.RelationID into cnt
                          //from d in cnt.DefaultIfEmpty()

                      join e in db.Employees on b.WCashier equals e.EmployeeId into emp
                      from e in emp.DefaultIfEmpty()

                      where b.WarrantyId == id
                      //join g in db.HireDetails on new { g1 = b.QuotationId, g2 = "Quotation" }
                      //equals new { g1 = g.Reference, g2 = g.Section } into hir
                      //from g in hir.DefaultIfEmpty()
                      //join h in db.HireTypes on g.HireType equals h.HireTypeId into Htype
                      //from h in Htype.DefaultIfEmpty()
                      //join t in db.ProTasks on b.Project equals t.ProjectId into ptask
                      //from t in ptask.DefaultIfEmpty()
                      //join i in db.Accountss on c.Accounts equals i.AccountsID into acc
                      //from i in acc.DefaultIfEmpty()
                      //join j in db.Contacts on e.PAddress equals j.ContactID into empcon
                      //from j in empcon.DefaultIfEmpty()
                      //join k in db.Mobiles on c.Contact equals k.Contact into mobi
                      //from k in mobi.DefaultIfEmpty()
                      //let cnt = (from d in db.ContactRelation
                      //           join c in db.Customers on d.RelationID equals c.CustomerID
                      //           join con in db.Contacts on d.ContactID equals con.ContactID
                      //           join qtn in db.Quotations on c.CustomerID equals qtn.Customer
                      //           where qtn.QuotationId == id && d.RelationType == 0
                      //           select con
                      //           ).FirstOrDefault()

                      select new pdfSummaryViewModel
                      {
                          chid = checkid,
                          PartyName = (c.CustomerPrintName == null) ? c.CustomerName : c.CustomerPrintName,
                          BillNo = b.BillNo,
                          Date = b.WDate,
                          Note = b.WNote,
                          Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,//+"\n"+j.EmailId,
                          Discount = b.WDiscount,
                          GrandTotal = b.WGrandTotal,
                          SubTotal = b.WSubTotal,
                          TaxAmount = b.WTaxAmount,
                          Address = c.Addres,
                          ProCheck = ProjectCheck,
                          //PrjNameCode = (f.ProjectName != null && f.ProjectName != "") ? f.ProCode + "-" + f.ProjectName : "",
                          //City = lc.Location,
                          //State = st.StateName,
                          Country = "U.A.E",
                          Zip = "",
                          //Email = cnt.EmailId,
                          Phone = "",
                          //Mobile = cnt.Mobile,
                          TRN = c.TaxID_TRN,
                          PTax = b.WTax,
                          //Remarks = b.Remarks,
                          //validity = (DateTime.Now <= DbFunctionsCompat.AddDays(b.QuotDate, (b.QuotValidity == null) ? 0 : b.QuotValidity + 1)) ? "Active" : "Expired",
                          //BillId = Convert.ToInt32(b.BillNo),
                          chkCode = PrintCode,
                          TimeOut = TOut,
                          //SaleType = b.SaleType,
                          //HireType = h.Name,
                          //FromDate = g.StartDate,
                          //ToDate = g.EndDate,
                          //ContactPerson = cnt.FirstName,
                          //PaymentTerms = b.PaymentTerms,
                          //TaskName = (t.TaskName != null && t.TaskName != "") ? t.TaskName : "",
                          //TaskCode = (t.TaskCode != null && t.TaskCode != "") ? t.TaskCode : "",
                          ComHeadCheck = ComHeadCheck,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          //ContactNo = j.Phone,
                          //empemail = j.EmailId,
                          //CreatedDate = b.QuotCreatedDate
                      }).FirstOrDefault();
            vmodel.pdfItem = (from b in db.WEItems
                              join c in db.Items on b.Item equals c.ItemID

                              join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                              from d in scaffold.DefaultIfEmpty()
                              join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                              from e in punit.DefaultIfEmpty()

                              join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                              from g in bundle.DefaultIfEmpty()
                              let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
                              where b.Warranty == id && b.itemNote != "-:{Bundle_Item}"
                              select new pdfItemViewModel
                              {
                                  Id = c.ItemID,
                                  InSaleInvoice = c.InSaleInvoice,
                                  WarrantyPeriod = b.WarrantyPeriod,
                                  ItemUnitPrice = b.ItemUnitPrice,
                                  ItemQuantity = b.ItemQuantity,
                                  ItemSubTotal = b.ItemSubTotal,
                                  ItemNote = b.itemNote,
                                  ItemTax = b.ItemTax,
                                  ItemTaxAmount = b.ItemTaxAmount,
                                  ItemTotalAmount = b.ItemTotalAmount,
                                  ItemDiscount = b.ItemDiscount,
                                  ItemCode = c.ItemCode,
                                  ItemName = c.ItemName,
                                  ItemUnit = e.ItemUnitName,
                                  PartNumber = c.PartNumber,
                                  PNoStatus = PNoStatus,
                                  CBM = d.CBM,
                                  Weight = d.Weight,
                                  img = img,
                                  ItemDescription = c.ItemDescription,
                                  //bundle = (from ay in db.BundleItems
                                  //          join az in db.ItemBundles on ay.ItemBundle equals az.ItemBundleId
                                  //          //join ab in db.QuotationItems on ay.ItemId equals ab.Item into quot
                                  //          //from ab in quot.DefaultIfEmpty()
                                  //          join bb in db.Items on ay.ItemId equals bb.ItemID
                                  //          join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                  //          from dd in scaffold.DefaultIfEmpty()
                                  //          join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
                                  //          from eb in bpunit.DefaultIfEmpty()
                                  //          let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                  //          where az.mainItem == b.Item
                                  //          select new pdfBundleViewModel
                                  //          {

                                  //              Id = bb.ItemID,
                                  //              ItemUnitPrice = ay.ItemUnitPrice,
                                  //              ItemQuantity = (ay.ItemQuantity * b.ItemQuantity),
                                  //              ItemSubTotal = (ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice),
                                  //              ItemNote = "",
                                  //              ItemTax = ay.ItemTax,
                                  //              ItemTaxAmount = ((ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice) * ay.ItemTax / 100),
                                  //              ItemTotalAmount = ay.ItemTotalAmount,
                                  //              ItemCode = bb.ItemCode,
                                  //              ItemName = bb.ItemName,
                                  //              ItemUnit = eb.ItemUnitName,
                                  //              PartNumber = bb.PartNumber,
                                  //              PNoStatus = PNoStatus,
                                  //              CBM = dd.CBM,
                                  //              Weight = dd.Weight,
                                  //              img = bimg,
                                  //              KeepStock = bb.KeepStock,
                                  //              Item = ay.ItemId,
                                  //              ItemDiscount = 0,
                                  //              ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                  //              ItemUnitID = bb.ItemUnitID,
                                  //              SubUnitId = bb.SubUnitId,
                                  //              ItemArabic = bb.ItemArabic,
                                  //              ItemDescription = bb.ItemDescription
                                  //          }).ToList(),
                                  //bundle = (from ab in db.QuotationItems

                                  //          join bb in db.Items on ab.Item equals bb.ItemID
                                  //          join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                  //          from dd in scaffold.DefaultIfEmpty()
                                  //          join eb in db.ItemUnits on ab.ItemUnit equals eb.ItemUnitID into bpunit
                                  //          from eb in bpunit.DefaultIfEmpty()
                                  //          let bimg = db.ItemImages.Where(bim => bim.ItemID == ab.Item).Select(bim => new { bim.FileName, bim.Status, bim.ItemImageID }).ToList()
                                  //          where ab.Quotation == id && ab.ItemNote == "-:{Bundle_Item}"
                                  //          && b.Item == ab.ItemDiscount
                                  //          select new
                                  //          {
                                  //              Id = bb.ItemID,
                                  //              ItemUnitPrice = ab.ItemUnitPrice,
                                  //              ItemQuantity = ab.ItemQuantity,
                                  //              ItemSubTotal = ab.ItemSubTotal,
                                  //              ItemNote = "",
                                  //              ItemTax = ab.ItemTax,
                                  //              ItemTaxAmount = ab.ItemTaxAmount,
                                  //              ItemTotalAmount = ab.ItemTotalAmount,
                                  //              ItemCode = bb.ItemCode,
                                  //              ItemName = bb.ItemName,
                                  //              ItemUnit = eb.ItemUnitName,
                                  //              PartNumber = bb.PartNumber,
                                  //              PNoStatus = PNoStatus,
                                  //              CBM = dd.CBM,
                                  //              Weight = dd.Weight,
                                  //              img = bimg,
                                  //              KeepStock = bb.KeepStock,
                                  //              ab.Item,
                                  //              ItemDiscount = 0,
                                  //              ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                  //              bb.ItemUnitID,
                                  //              bb.SubUnitId,
                                  //              bb.ItemArabic,
                                  //              bb.ItemDescription
                                  //          }).ToList().Distinct(),
                              }).ToList();

            vmodel.billsundry = db.WBillSundries.Where(n => n.Warranty == id).Select(b => new pdfBillSundryViewModel
            {
                AmountType = b.AmountType,
                BsAmount = b.BsAmount,
                BsType = b.BsType,
                BsValue = b.BsValue != null ? b.BsValue : 0,
                BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
            }).ToList();

            return vmodel;

        }

        public pdfSummaryViewModel WorkCompletionData(long id, Status? PNoStatus = null, Status? PrintCode = null, Int64? TOut = 0, Status? ProjectCheck = null, Status? ComHeadCheck = null, long? checkid = 0)
        {
            var chk = checkid;
            pdfSummaryViewModel vmodel = new pdfSummaryViewModel();
            vmodel = (from b in db.WorkCompletions
                      join c in db.Customers on b.Customer equals c.CustomerID into cust
                      from c in cust.DefaultIfEmpty()
                          //join st in db.States on c.StateID equals st.StateID into stt
                          //from st in stt.DefaultIfEmpty()
                          //join lc in db.LocationNames on st.StateID equals lc.StateId into lcc
                          //from lc in lcc.DefaultIfEmpty()


                          //join d in db.ContactRelation on c.CustomerID equals d.RelationID into cnt
                          //from d in cnt.DefaultIfEmpty()

                      join e in db.Employees on b.WcCashier equals e.EmployeeId into emp
                      from e in emp.DefaultIfEmpty()

                      where b.WorkCompletionId == id
                      //join g in db.HireDetails on new { g1 = b.QuotationId, g2 = "Quotation" }
                      //equals new { g1 = g.Reference, g2 = g.Section } into hir
                      //from g in hir.DefaultIfEmpty()
                      //join h in db.HireTypes on g.HireType equals h.HireTypeId into Htype
                      //from h in Htype.DefaultIfEmpty()
                      //join t in db.ProTasks on b.Project equals t.ProjectId into ptask
                      //from t in ptask.DefaultIfEmpty()
                      //join i in db.Accountss on c.Accounts equals i.AccountsID into acc
                      //from i in acc.DefaultIfEmpty()
                      //join j in db.Contacts on e.PAddress equals j.ContactID into empcon
                      //from j in empcon.DefaultIfEmpty()
                      //join k in db.Mobiles on c.Contact equals k.Contact into mobi
                      //from k in mobi.DefaultIfEmpty()
                      //let cnt = (from d in db.ContactRelation
                      //           join c in db.Customers on d.RelationID equals c.CustomerID
                      //           join con in db.Contacts on d.ContactID equals con.ContactID
                      //           join qtn in db.Quotations on c.CustomerID equals qtn.Customer
                      //           where qtn.QuotationId == id && d.RelationType == 0
                      //           select con
                      //           ).FirstOrDefault()

                      select new pdfSummaryViewModel
                      {
                          chid = checkid,
                          PartyName = (c.CustomerPrintName == null) ? c.CustomerName : c.CustomerPrintName,
                          BillNo = b.BillNo,
                          Ref1 = b.Ref1,
                          Date = b.WCDate,
                          Note = b.WCNote,
                          Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,//+"\n"+j.EmailId,
                          Discount = b.WCDiscount,
                          GrandTotal = b.WCGrandTotal,
                          SubTotal = b.WCSubTotal,
                          TaxAmount = b.WCTaxAmount,
                          Address = c.Addres,
                          ProCheck = ProjectCheck,
                          //PrjNameCode = (f.ProjectName != null && f.ProjectName != "") ? f.ProCode + "-" + f.ProjectName : "",
                          //City = lc.Location,
                          //State = st.StateName,
                          Country = "U.A.E",
                          Zip = "",
                          //Email = cnt.EmailId,
                          Phone = "",
                          //Mobile = cnt.Mobile,
                          TRN = c.TaxID_TRN,
                          PTax = b.WCTax,
                          //Remarks = b.Remarks,
                          //validity = (DateTime.Now <= DbFunctionsCompat.AddDays(b.QuotDate, (b.QuotValidity == null) ? 0 : b.QuotValidity + 1)) ? "Active" : "Expired",
                          //BillId = Convert.ToInt32(b.BillNo),
                          chkCode = PrintCode,
                          TimeOut = TOut,
                          //SaleType = b.SaleType,
                          //HireType = h.Name,
                          //FromDate = g.StartDate,
                          //ToDate = g.EndDate,
                          //ContactPerson = cnt.FirstName,
                          //PaymentTerms = b.PaymentTerms,
                          //TaskName = (t.TaskName != null && t.TaskName != "") ? t.TaskName : "",
                          //TaskCode = (t.TaskCode != null && t.TaskCode != "") ? t.TaskCode : "",
                          ComHeadCheck = ComHeadCheck,
                         
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          //ContactNo = j.Phone,
                          //empemail = j.EmailId,
                          //CreatedDate = b.QuotCreatedDate
                      }).FirstOrDefault();
            vmodel.pdfItem = (from b in db.WCItems
                              join c in db.Items on b.Item equals c.ItemID

                              join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                              from d in scaffold.DefaultIfEmpty()
                              join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                              from e in punit.DefaultIfEmpty()

                              join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                              from g in bundle.DefaultIfEmpty()
                              let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
                              where b.WorkCompletion == id && b.itemNote != "-:{Bundle_Item}"
                              select new pdfItemViewModel
                              {
                                  Id = c.ItemID,
                                  InSaleInvoice = c.InSaleInvoice,
                                  ItemUnitPrice = b.ItemUnitPrice,
                                  ItemQuantity = b.ItemQuantity,
                                  ItemSubTotal = b.ItemSubTotal,
                                  ItemNote = b.itemNote,
                                  ItemTax = b.ItemTax,
                                  ItemTaxAmount = b.ItemTaxAmount,
                                  ItemTotalAmount = b.ItemTotalAmount,
                                  ItemDiscount = b.ItemDiscount,
                                  ItemCode = c.ItemCode,
                                  ItemName = c.ItemName,
                                  ItemUnit = e.ItemUnitName,
                                  PartNumber = c.PartNumber,
                                  PNoStatus = PNoStatus,
                                  CBM = d.CBM,
                                  Weight = d.Weight,
                                  img = img,
                                  ItemDescription = c.ItemDescription,
                                  //bundle = (from ay in db.BundleItems
                                  //          join az in db.ItemBundles on ay.ItemBundle equals az.ItemBundleId
                                  //          //join ab in db.QuotationItems on ay.ItemId equals ab.Item into quot
                                  //          //from ab in quot.DefaultIfEmpty()
                                  //          join bb in db.Items on ay.ItemId equals bb.ItemID
                                  //          join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                  //          from dd in scaffold.DefaultIfEmpty()
                                  //          join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
                                  //          from eb in bpunit.DefaultIfEmpty()
                                  //          let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                  //          where az.mainItem == b.Item
                                  //          select new pdfBundleViewModel
                                  //          {

                                  //              Id = bb.ItemID,
                                  //              ItemUnitPrice = ay.ItemUnitPrice,
                                  //              ItemQuantity = (ay.ItemQuantity * b.ItemQuantity),
                                  //              ItemSubTotal = (ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice),
                                  //              ItemNote = "",
                                  //              ItemTax = ay.ItemTax,
                                  //              ItemTaxAmount = ((ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice) * ay.ItemTax / 100),
                                  //              ItemTotalAmount = ay.ItemTotalAmount,
                                  //              ItemCode = bb.ItemCode,
                                  //              ItemName = bb.ItemName,
                                  //              ItemUnit = eb.ItemUnitName,
                                  //              PartNumber = bb.PartNumber,
                                  //              PNoStatus = PNoStatus,
                                  //              CBM = dd.CBM,
                                  //              Weight = dd.Weight,
                                  //              img = bimg,
                                  //              KeepStock = bb.KeepStock,
                                  //              Item = ay.ItemId,
                                  //              ItemDiscount = 0,
                                  //              ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                  //              ItemUnitID = bb.ItemUnitID,
                                  //              SubUnitId = bb.SubUnitId,
                                  //              ItemArabic = bb.ItemArabic,
                                  //              ItemDescription = bb.ItemDescription
                                  //          }).ToList(),
                                  //bundle = (from ab in db.QuotationItems

                                  //          join bb in db.Items on ab.Item equals bb.ItemID
                                  //          join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                  //          from dd in scaffold.DefaultIfEmpty()
                                  //          join eb in db.ItemUnits on ab.ItemUnit equals eb.ItemUnitID into bpunit
                                  //          from eb in bpunit.DefaultIfEmpty()
                                  //          let bimg = db.ItemImages.Where(bim => bim.ItemID == ab.Item).Select(bim => new { bim.FileName, bim.Status, bim.ItemImageID }).ToList()
                                  //          where ab.Quotation == id && ab.ItemNote == "-:{Bundle_Item}"
                                  //          && b.Item == ab.ItemDiscount
                                  //          select new
                                  //          {
                                  //              Id = bb.ItemID,
                                  //              ItemUnitPrice = ab.ItemUnitPrice,
                                  //              ItemQuantity = ab.ItemQuantity,
                                  //              ItemSubTotal = ab.ItemSubTotal,
                                  //              ItemNote = "",
                                  //              ItemTax = ab.ItemTax,
                                  //              ItemTaxAmount = ab.ItemTaxAmount,
                                  //              ItemTotalAmount = ab.ItemTotalAmount,
                                  //              ItemCode = bb.ItemCode,
                                  //              ItemName = bb.ItemName,
                                  //              ItemUnit = eb.ItemUnitName,
                                  //              PartNumber = bb.PartNumber,
                                  //              PNoStatus = PNoStatus,
                                  //              CBM = dd.CBM,
                                  //              Weight = dd.Weight,
                                  //              img = bimg,
                                  //              KeepStock = bb.KeepStock,
                                  //              ab.Item,
                                  //              ItemDiscount = 0,
                                  //              ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                  //              bb.ItemUnitID,
                                  //              bb.SubUnitId,
                                  //              bb.ItemArabic,
                                  //              bb.ItemDescription
                                  //          }).ToList().Distinct(),
                              }).ToList();

            vmodel.billsundry = db.WCBillSundries.Where(n => n.WorkCompletion == id).Select(b => new pdfBillSundryViewModel
            {
                AmountType = b.AmountType,
                BsAmount = b.BsAmount,
                BsType = b.BsType,
                BsValue = b.BsValue != null ? b.BsValue : 0,
                BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
            }).ToList();

            return vmodel;

        }
        #region Print PurchaseQuotation Details
        public pdfSummaryViewModel PurchaseQuotationData(long id, Status? PNoStatus = null, Status? PrintCode = null, Int64? TOut = 0, Status? ProjectCheck = null, Status? ComHeadCheck = null, string CMReqNo = null)
        {
            pdfSummaryViewModel vmodel = new pdfSummaryViewModel();
            vmodel = (from b in db.PurchaseQuotations
                      join c in db.Suppliers on b.Supplier equals c.SupplierID into cust
                      from c in cust.DefaultIfEmpty()
                      join d in db.Contacts on c.Contact equals d.ContactID into cnt
                      from d in cnt.DefaultIfEmpty()
                      join e in db.Employees on b.PQuotCashier equals e.EmployeeId into emp
                      from e in emp.DefaultIfEmpty()
                      join f in db.Projects on b.Project equals f.ProjectId into prjct
                      from f in prjct.DefaultIfEmpty()
                      where b.PQuotationId == id
                      join g in db.HireDetails on new { g1 = b.PQuotationId, g2 = "PurchaseQuotation" }
                      equals new { g1 = g.Reference, g2 = g.Section } into hir
                      from g in hir.DefaultIfEmpty()
                      join h in db.HireTypes on g.HireType equals h.HireTypeId into Htype
                      from h in Htype.DefaultIfEmpty()
                      join i in db.Accountss on c.Accounts equals i.AccountsID
                      join j in db.Contacts on e.PAddress equals j.ContactID into empcon
                      from j in empcon.DefaultIfEmpty()
                      join l in db.Mobiles on c.Contact equals l.Contact into mobi
                      from l in mobi.DefaultIfEmpty()
                      select new pdfSummaryViewModel
                      {
                          PartyName = c.SupplierName,
                          BillNo = b.BillNo,
                          Date = b.PQuotDate,
                          Note = b.TermsCondition,
                          Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                          Discount = b.PQuotDiscount,
                          GrandTotal = b.PQuotGrandTotal,
                          SubTotal = b.PQuotSubTotal,
                          TaxAmount = b.PQuotTaxAmount,
                          Address = d.Address,
                          ProCheck = ProjectCheck,
                          PrjNameCode = (f.ProjectName != null && f.ProjectName != "") ? f.ProCode + "-" + f.ProjectName : "",
                          City = d.City,
                          State = d.State,
                          Country = d.Country,
                          Zip = d.Zip,
                          Email = d.EmailId,
                          Phone = d.Phone,
                          Mobile = l.MobileNum,// d.Mobile,
                          TRN = i.TRN,
                          PTax = b.PQuotTax,
                          Remarks = b.Remarks,
                          validity = (DateTime.Now <= b.PQuotDate.AddDays((b.PQuotValidity == null) ? 0 : (b.PQuotValidity.Value + 1))) ? "Active" : "Expired",
                          BillId = b.PQuotNo,
                          chkCode = PrintCode,
                          TimeOut = TOut,
                          CMReqNo = CMReqNo,
                          ContactPerson = d.ContactPerson,
                          PaymentTerms = b.PaymentTerms,
                          ComHeadCheck = ComHeadCheck,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          ContactNo = j.Phone,
                          CreatedDate = b.PQuotCreatedDate
                      }).FirstOrDefault();
            vmodel.pdfItem = (from b in db.PurchaseQuotationItems
                              join c in db.Items on b.Item equals c.ItemID

                              join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                              from d in scaffold.DefaultIfEmpty()
                              join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                              from e in punit.DefaultIfEmpty()

                              join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                              from g in bundle.DefaultIfEmpty()
                              join h in db.ItemBrands on b.Make equals h.ItemBrandID into brn
                              from h in brn.DefaultIfEmpty()
                              let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
                              where b.PQuotation == id && b.ItemNote != "-:{Bundle_Item}"
                              select new pdfItemViewModel
                              {
                                  Id = c.ItemID,
                                  ItemUnitPrice = b.ItemUnitPrice,
                                  ItemQuantity = b.ItemQuantity,
                                  ItemSubTotal = b.ItemSubTotal,
                                  ItemNote = b.ItemNote,
                                  ItemTax = b.ItemTax,
                                  ItemTaxAmount = b.ItemTaxAmount,
                                  ItemTotalAmount = b.ItemTotalAmount,
                                  ItemDiscount = b.ItemDiscount,
                                  ItemCode = c.ItemCode,
                                  ItemName = c.ItemName,
                                  ItemUnit = e.ItemUnitName,
                                  PartNumber = c.PartNumber,
                                  PNoStatus = PNoStatus,
                                  CBM = d.CBM,
                                  Weight = d.Weight,
                                  img = img,
                                  ItemDescription = c.ItemDescription,
                                  Make = h != null ? h.ItemBrandName : "",
                                  bundle = (from ab in db.PurchaseQuotationItems
                                            join bb in db.Items on ab.Item equals bb.ItemID
                                            join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                            from dd in scaffold.DefaultIfEmpty()

                                            join eb in db.ItemUnits on ab.ItemUnit equals eb.ItemUnitID into bpunit
                                            from eb in bpunit.DefaultIfEmpty()

                                            let bimg = db.ItemImages.Where(bim => bim.ItemID == ab.Item).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                            where ab.PQuotation == id && ab.ItemNote == "-:{Bundle_Item}"
                                            && b.Item == ab.ItemDiscount
                                            select new pdfBundleViewModel
                                            {
                                                Id = bb.ItemID,
                                                ItemUnitPrice = ab.ItemUnitPrice,
                                                ItemQuantity = ab.ItemQuantity,
                                                ItemSubTotal = ab.ItemSubTotal,
                                                ItemNote = "",
                                                ItemTax = ab.ItemTax,
                                                ItemTaxAmount = ab.ItemTaxAmount,
                                                ItemTotalAmount = ab.ItemTotalAmount,

                                                ItemCode = bb.ItemCode,
                                                ItemName = bb.ItemName,
                                                ItemUnit = eb.ItemUnitName,
                                                PartNumber = bb.PartNumber,
                                                PNoStatus = PNoStatus,
                                                CBM = dd.CBM,
                                                Weight = dd.Weight,
                                                img = bimg,

                                                KeepStock = bb.KeepStock,

                                                Item = ab.Item,
                                                ItemDiscount = 0,
                                                ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                ItemUnitID = bb.ItemUnitID,
                                                SubUnitId = bb.SubUnitId,
                                                ItemArabic = bb.ItemArabic,
                                                ItemDescription = bb.ItemDescription
                                            }).ToList(),
                              }).ToList();

            vmodel.billsundry = db.PQtBillSundrys.Where(n => n.PQuotation == id).Select(b => new pdfBillSundryViewModel
            {
                AmountType = b.AmountType,
                BsAmount = b.BsAmount,
                BsType = b.BsType,
                BsValue = b.BsValue != null ? b.BsValue : 0,
                BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
            }).ToList();

            return vmodel;

        }
        #endregion

        #region Print ProForma Details
        public pdfSummaryViewModel ProFormaData(long id, Status? PrintCode = null, Status? PNoStatus = null, Int64? TOut = 0, Status? ProjectCheck = null, Status? ComHeadCheck = null)
        {
            pdfSummaryViewModel vmodel = new pdfSummaryViewModel();
            vmodel = (from a in db.ProFormas
                      join b in db.Customers on a.Customer equals b.CustomerID into cust
                      from b in cust.DefaultIfEmpty()
                      join c in db.Contacts on b.Contact equals c.ContactID into cnt
                      from c in cnt.DefaultIfEmpty()
                      join e in db.Employees on a.PFCashier equals e.EmployeeId into user
                      from e in user.DefaultIfEmpty()
                      join g in db.HireDetails on new { g1 = a.ProFormaId, g2 = "Proforma" }
                      equals new { g1 = g.Reference, g2 = g.Section } into hir
                      from g in hir.DefaultIfEmpty()
                      join h in db.HireTypes on g.HireType equals h.HireTypeId into Htype
                      from h in Htype.DefaultIfEmpty()
                      join p in db.Projects on a.Project equals p.ProjectId into prjct
                      from p in prjct.DefaultIfEmpty()
                      join t in db.ProTasks on a.Project equals t.ProjectId into ptask
                      from t in ptask.DefaultIfEmpty()
                      join f in db.Accountss on b.Accounts equals f.AccountsID into acc
                      from f in acc.DefaultIfEmpty()
                      join i in db.Contacts on e.PAddress equals i.ContactID into empcon
                      from i in empcon.DefaultIfEmpty()
                      join l in db.Mobiles on b.Contact equals l.Contact into mobi
                      from l in mobi.DefaultIfEmpty()
                      where a.ProFormaId == id
                      select new pdfSummaryViewModel
                      {
                          PartyName = b.CustomerName,

                          BillId = a.PFNo,
                          BillNo = a.BillNo,
                          Date = a.PFDate,
                          Note = a.PFNote,
                          Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                          Discount = a.PFDiscount,
                          //Total = a.PFDiscount + a.PFGrandTotal,
                          GrandTotal = a.PFGrandTotal,
                          SubTotal = a.PFSubTotal,
                          TaxAmount = a.PFTaxAmount,
                          //Address = c.Address,
                          Address = b.Addres,
                          City = c.City,
                          State = c.State,
                          Country = c.Country,
                          Zip = c.Zip,
                          Email = c.EmailId,
                          Phone = c.Phone,
                          Mobile = l.MobileNum,// c.Mobile,
                          //TRN = f.TRN,
                          TRN = b.TaxID_TRN,
                          CreditPeriod = b.CreditPeriod,
                          Location = a.Location,
                          Remarks = a.Remarks,
                          chkCode = PrintCode,
                          paytype = (a.CustomerType == CustomerType.Walking ? "Cash" : "Credit"),
                          TimeOut = TOut,
                          SaleType = a.SaleType,
                          HireType = h.Name,
                          FromDate = g.StartDate,
                          ToDate = g.EndDate,
                          ContactPerson = c.ContactPerson,
                          HSCode = a.HSCode,
                          PaymentTerms = a.PaymentTerms,
                          ProCheck = ProjectCheck,
                          PrjNameCode = (p.ProjectName != null && p.ProjectName != "") ? p.ProCode + "-" + p.ProjectName : "",
                          TaskName = (t.TaskName != null && t.TaskName != "") ? t.TaskName : "",
                          ComHeadCheck = ComHeadCheck,
                          Ref1 = a.Ref1,
                          Ref2 = a.Ref2,
                          Ref3 = a.Ref3,
                          Ref4 = a.Ref4,
                          Ref5 = a.Ref5,
                          ContactNo = i.Phone,
                          CreatedDate = a.PFCreatedDate
                      }).FirstOrDefault();

            vmodel.pdfItem = (from b in db.PFItemss
                              join c in db.Items on b.Item equals c.ItemID

                              join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                              from d in scaffold.DefaultIfEmpty()
                              join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                              from e in punit.DefaultIfEmpty()

                              join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                              from g in bundle.DefaultIfEmpty()
                              let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
                              where b.ProForma == id && b.itemNote != "-:{Bundle_Item}"
                              select new pdfItemViewModel
                              {
                                  Id = c.ItemID,
                                  ItemUnitPrice = b.ItemUnitPrice,
                                  ItemQuantity = b.ItemQuantity,
                                  ItemSubTotal = b.ItemSubTotal,
                                  ItemNote = b.itemNote,
                                  ItemTax = b.ItemTax,
                                  ItemTaxAmount = b.ItemTaxAmount,
                                  ItemTotalAmount = b.ItemTotalAmount,
                                  ItemDiscount = b.ItemDiscount,
                                  ItemCode = c.ItemCode,
                                  ItemName = c.ItemName,
                                  ItemUnit = e.ItemUnitName,
                                  PartNumber = c.PartNumber,
                                  InSaleInvoice = c.InSaleInvoice,
                                  PNoStatus = PNoStatus,
                                  CBM = d.CBM,
                                  Weight = d.Weight,
                                  img = img,
                                  ItemDescription = c.ItemDescription,
                                  KeepStock = c.KeepStock,
                                  bundle = (from ay in db.BundleItems
                                            join az in db.ItemBundles on ay.ItemBundle equals az.ItemBundleId
                                            //join ab in db.QuotationItems on ay.ItemId equals ab.Item into quot
                                            //from ab in quot.DefaultIfEmpty()
                                            join bb in db.Items on ay.ItemId equals bb.ItemID
                                            join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                            from dd in scaffold.DefaultIfEmpty()
                                            join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
                                            from eb in bpunit.DefaultIfEmpty()
                                            let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                            where az.mainItem == b.Item
                                            select new pdfBundleViewModel
                                            {
                                                Id = bb.ItemID,
                                                ItemUnitPrice = ay.ItemUnitPrice,
                                                ItemQuantity = (ay.ItemQuantity * b.ItemQuantity),
                                                ItemSubTotal = (ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice),
                                                ItemNote = "",
                                                ItemTax = ay.ItemTax,
                                                ItemTaxAmount = ((ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice) * ay.ItemTax / 100),
                                                ItemTotalAmount = ay.ItemTotalAmount,
                                                ItemCode = bb.ItemCode,
                                                ItemName = bb.ItemName,
                                                ItemUnit = eb.ItemUnitName,
                                                PartNumber = bb.PartNumber,
                                                PNoStatus = PNoStatus,
                                                CBM = dd.CBM,
                                                Weight = dd.Weight,
                                                img = bimg,
                                                KeepStock = bb.KeepStock,
                                                Item = ay.ItemId,
                                                ItemDiscount = 0,
                                                ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                ItemUnitID = bb.ItemUnitID,
                                                SubUnitId = bb.SubUnitId,
                                                ItemArabic = bb.ItemArabic,
                                                ItemDescription = bb.ItemDescription
                                            }).ToList(),
                              }).ToList();

            vmodel.billsundry = db.PFBillSundrys.Where(n => n.ProForma == id).Select(b => new pdfBillSundryViewModel
            {
                AmountType = b.AmountType,
                BsAmount = b.BsAmount,
                BsType = b.BsType,
                BsValue = b.BsValue != null ? b.BsValue : 0,
                BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
            }).ToList();
            return vmodel;
        }
        #endregion

        #region Print ProForma Details
        public pdfSummaryViewModel BOQData(long id, Status? PrintCode = null, Status? PNoStatus = null, Int64? TOut = 0, Status? ProjectCheck = null, Status? ComHeadCheck = null)
        {
            var bono = Convert.ToString(db.BillOfQyts.Where(a => a.BoqId == id).Select(a => a.BillNo).FirstOrDefault());
            pdfSummaryViewModel vmodel = new pdfSummaryViewModel();
            vmodel = (from a in db.BillOfQyts
                      join b in db.Customers on a.Customer equals b.CustomerID into cust
                      from b in cust.DefaultIfEmpty()
                      join c in db.Contacts on b.Contact equals c.ContactID into cnt
                      from c in cnt.DefaultIfEmpty()
                      join e in db.Employees on a.SalesExecutive equals e.EmployeeId into user
                      from e in user.DefaultIfEmpty()

                      join i in db.Contacts on e.PAddress equals i.ContactID into empcon
                      from i in empcon.DefaultIfEmpty()
                      join l in db.Mobiles on b.Contact equals l.Contact into mobi
                      from l in mobi.DefaultIfEmpty()


                      let boq = (from d in db.ContactRelation
                                 join c in db.Customers on d.RelationID equals c.CustomerID
                                 join con in db.Contacts on d.ContactID equals con.ContactID
                                 join qtn in db.BillOfQyts on c.CustomerID equals qtn.Customer
                                 where qtn.BoqId == id
                                 select con
                                 ).FirstOrDefault()



                      where a.BoqId == id
                      select new pdfSummaryViewModel
                      {

                          PartyName = b.CustomerName,

                          BillNo = bono,

                          // BillNo = a.BoqId,

                          Date = a.BOQDate,

                          Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,

                          Address = b.Addres,
                          City = c.City,
                          State = c.State,
                          Country = c.Country,
                          Zip = c.Zip,
                          Email = boq.EmailId,
                          Phone = "",
                          Mobile = boq.Mobile,

                          //TRN = f.TRN,
                          TRN = b.TaxID_TRN,
                          CreditPeriod = b.CreditPeriod,

                          chkCode = PrintCode,

                          TimeOut = TOut,

                          ContactPerson = c.ContactPerson,

                          ProCheck = ProjectCheck,


                          ContactNo = i.Phone,
                          CreatedDate = a.CreatedDate
                      }).FirstOrDefault();

            vmodel.pdfItem = (from b in db.BoqItems
                              join c in db.Items on b.ItemId equals c.ItemID

                              join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                              from d in scaffold.DefaultIfEmpty()
                              join e in db.ItemUnits on b.Unit equals e.ItemUnitID into punit
                              from e in punit.DefaultIfEmpty()

                              join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                              from g in bundle.DefaultIfEmpty()
                              let img = db.ItemImages.Where(im => im.ItemID == b.ItemId).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
                              where b.BoqId == id
                              select new pdfItemViewModel
                              {
                                  Id = c.ItemID,
                                  //ItemUnitPrice = b.ItemUnitPrice,
                                  ItemQuantity = b.Quantity,
                                  //ItemSubTotal = b.ItemSubTotal,
                                  ItemNote = b.ItemNote,
                                  // ItemTax = b.ItemTax,
                                  // ItemTaxAmount = b.ItemTaxAmount,
                                  //ItemTotalAmount = b.ItemTotalAmount,
                                  //ItemDiscount = b.ItemDiscount,
                                  ItemCode = c.ItemCode,
                                  ItemName = c.ItemName,
                                  ItemUnit = e.ItemUnitName,
                                  PartNumber = c.PartNumber,
                                  InSaleInvoice = c.InSaleInvoice,
                                  PNoStatus = PNoStatus,
                                  CBM = d.CBM,
                                  Weight = d.Weight,
                                  img = img,
                                  ItemDescription = c.ItemDescription,
                                  KeepStock = c.KeepStock,
                                  bundle = (from ay in db.BundleItems
                                            join az in db.ItemBundles on ay.ItemBundle equals az.ItemBundleId
                                            //join ab in db.QuotationItems on ay.ItemId equals ab.Item into quot
                                            //from ab in quot.DefaultIfEmpty()
                                            join bb in db.Items on ay.ItemId equals bb.ItemID
                                            join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                            from dd in scaffold.DefaultIfEmpty()
                                            join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
                                            from eb in bpunit.DefaultIfEmpty()
                                            let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                            where az.mainItem == b.ItemId
                                            select new pdfBundleViewModel
                                            {
                                                Id = bb.ItemID,
                                                //ItemUnitPrice = ay.ItemUnitPrice,
                                                ItemQuantity = (ay.ItemQuantity * b.Quantity),
                                                //ItemSubTotal = (ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice),
                                                // ItemNote = "",
                                                // ItemTax = ay.ItemTax,
                                                //ItemTaxAmount = ((ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice) * ay.ItemTax / 100),
                                                // ItemTotalAmount = ay.ItemTotalAmount,
                                                ItemCode = bb.ItemCode,
                                                ItemName = bb.ItemName,
                                                ItemUnit = eb.ItemUnitName,
                                                PartNumber = bb.PartNumber,
                                                PNoStatus = PNoStatus,
                                                CBM = dd.CBM,
                                                Weight = dd.Weight,
                                                img = bimg,
                                                KeepStock = bb.KeepStock,
                                                Item = ay.ItemId,
                                                ItemDiscount = 0,
                                                ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                ItemUnitID = bb.ItemUnitID,
                                                SubUnitId = bb.SubUnitId,
                                                ItemArabic = bb.ItemArabic,
                                                ItemDescription = bb.ItemDescription
                                            }).ToList(),
                              }).ToList();

            vmodel.billsundry = db.PFBillSundrys.Where(n => n.ProForma == id).Select(b => new pdfBillSundryViewModel
            {
                AmountType = b.AmountType,
                BsAmount = b.BsAmount,
                BsType = b.BsType,
                BsValue = b.BsValue != null ? b.BsValue : 0,
                BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
            }).ToList();
            return vmodel;
        }
        #endregion


        #region Print DeliveryNote Details
        public pdfSummaryViewModel DeliveryNoteData(long id, Status? PrintCode = null, Status? PNoStatus = null, Int64? TOut = 0, Status? ProjectCheck = null, Status? ComHeadCheck = null)
        {
            pdfSummaryViewModel vmodel = new pdfSummaryViewModel();
            vmodel = (from b in db.Deliverynotes
                      join c in db.Customers on b.Customer equals c.CustomerID into cust
                      from c in cust.DefaultIfEmpty()
                      join d in db.Contacts on c.Contact equals d.ContactID into cnt
                      from d in cnt.DefaultIfEmpty()
                      join e in db.Employees on b.DvCashier equals e.EmployeeId into emp
                      from e in emp.DefaultIfEmpty()
                      join g in db.HireDetails on new { g1 = b.DeliverynoteId, g2 = "Delivernote" }
                      equals new { g1 = g.Reference, g2 = g.Section } into hir
                      from g in hir.DefaultIfEmpty()
                      join h in db.HireTypes on g.HireType equals h.HireTypeId into Htype
                      from h in Htype.DefaultIfEmpty()
                      join p in db.Projects on b.Project equals p.ProjectId into prjct
                      from p in prjct.DefaultIfEmpty()
                      join t in db.ProTasks on b.Project equals t.ProjectId into ptask
                      from t in ptask.DefaultIfEmpty()
                      join f in db.Accountss on c.Accounts equals f.AccountsID into acc
                      from f in acc.DefaultIfEmpty()
                      join i in db.Contacts on e.PAddress equals i.ContactID into empcon
                      from i in empcon.DefaultIfEmpty()
                      join l in db.Mobiles on c.Contact equals l.Contact into mobi
                      from l in mobi.DefaultIfEmpty()
                      where b.DeliverynoteId == id
                      select new pdfSummaryViewModel
                      {
                          PartyName = (c.CustomerPrintName == null || c.CustomerPrintName == "") ? c.CustomerName : c.CustomerPrintName,
                          BillId = b.DvNo,
                          BillNo = b.BillNo,
                          Date = b.DvDate,
                          Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                          Note = b.TermsCondition,
                          Discount = b.DvDiscount,
                          GrandTotal = b.DvGrandTotal,
                          Paid = null,
                          Balance = null,
                          //Total = b.DvDiscount + b.DvGrandTotal,
                          SubTotal = b.DvSubTotal,
                          TaxAmount = b.DvTaxAmount,
                          //Address = d.Address,
                          Address = c.Addres,

                          City = d.City,
                          State = d.State,
                          Country = d.Country,
                          Zip = d.Zip,
                          PONo = b.LPONo,
                          Email = d.EmailId,
                          Phone = d.Phone,
                          Mobile = l.MobileNum,// d.Mobile,
                                               // TRN = f.TRN,
                          TRN = c.TaxID_TRN,

                          //b.DvItems,
                          TermsCondition = b.TermsCondition,
                          //b.DvItemQuantity,
                          //b.DvSubTotal,
                          //b.DvTax,

                          id = b.DeliverynoteId,
                          CreditPeriod = c.CreditPeriod,
                          Location = b.Location,
                          paytype = (b.CustomerType == CustomerType.Walking ? "Cash" : "Credit"),
                          Remarks = b.Remarks,
                          chkCode = PrintCode,
                          TimeOut = TOut,
                          SaleType = b.SaleType,
                          HireType = h.Name,
                          FromDate = g.StartDate,
                          ToDate = g.EndDate,
                          ContactPerson = d.ContactPerson,
                          PaymentTerms = b.PaymentTerms,
                          ProCheck = ProjectCheck,
                          PrjNameCode = (p.ProjectName != null && p.ProjectName != "") ? p.ProCode + "-" + p.ProjectName : "",
                          TaskName = (t.TaskName != null && t.TaskName != "") ? t.TaskName : "",
                          ComHeadCheck = ComHeadCheck,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          ContactNo = i.Phone,
                          CreatedDate = b.DvCreatedDate
                      }).FirstOrDefault();

            vmodel.pdfItem = (from b in db.DvItems
                              join c in db.Items on b.Item equals c.ItemID

                              join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                              from d in scaffold.DefaultIfEmpty()
                              join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                              from e in punit.DefaultIfEmpty()

                              join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                              from g in bundle.DefaultIfEmpty()
                              let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
                              where b.Dv == id && b.ItemNote != "-:{Bundle_Item}"
                              select new pdfItemViewModel
                              {
                                  Id = c.ItemID,
                                  ItemUnitPrice = b.ItemUnitPrice,
                                  ItemQuantity = b.ItemQuantity,
                                  ItemSubTotal = b.ItemSubTotal,
                                  ItemTax = b.ItemTax,
                                  ItemNote = b.ItemNote,
                                  ItemTaxAmount = b.ItemTaxAmount,
                                  ItemTotalAmount = b.ItemTotalAmount,
                                  ItemDiscount = b.ItemDiscount,
                                  ItemCode = c.ItemCode,
                                  ItemName = c.ItemName,
                                  ItemUnit = e.ItemUnitName,
                                  InSaleInvoice = c.InSaleInvoice,
                                  PartNumber = c.PartNumber,
                                  PNoStatus = PNoStatus,
                                  CBM = d.CBM,
                                  Weight = d.Weight,
                                  img = img,
                                  ItemDescription = c.ItemDescription,
                                  KeepStock = c.KeepStock,
                                  bundle = (from ay in db.BundleItems
                                            join az in db.ItemBundles on ay.ItemBundle equals az.ItemBundleId
                                            //join ab in db.QuotationItems on ay.ItemId equals ab.Item into quot
                                            //from ab in quot.DefaultIfEmpty()
                                            join bb in db.Items on ay.ItemId equals bb.ItemID
                                            join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                            from dd in scaffold.DefaultIfEmpty()
                                            join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
                                            from eb in bpunit.DefaultIfEmpty()
                                            let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                            where az.mainItem == b.Item
                                            select new pdfBundleViewModel
                                            {
                                                Id = bb.ItemID,
                                                ItemUnitPrice = ay.ItemUnitPrice,
                                                ItemQuantity = (ay.ItemQuantity * b.ItemQuantity),
                                                ItemSubTotal = (ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice),
                                                ItemNote = "",
                                                ItemTax = ay.ItemTax,
                                                ItemTaxAmount = ((ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice) * ay.ItemTax / 100),
                                                ItemTotalAmount = ay.ItemTotalAmount,
                                                ItemCode = bb.ItemCode,
                                                ItemName = bb.ItemName,
                                                ItemUnit = eb.ItemUnitName,
                                                PartNumber = bb.PartNumber,
                                                PNoStatus = PNoStatus,
                                                CBM = dd.CBM,
                                                Weight = dd.Weight,
                                                img = bimg,
                                                KeepStock = bb.KeepStock,
                                                Item = ay.ItemId,
                                                ItemDiscount = 0,
                                                ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                ItemUnitID = bb.ItemUnitID,
                                                SubUnitId = bb.SubUnitId,
                                                ItemArabic = bb.ItemArabic,
                                                ItemDescription = bb.ItemDescription
                                            }).ToList(),
                              }).ToList();

            return vmodel;
        }
        #endregion

        #region Print PurchaseEntry Details
        public pdfSummaryViewModel PurchaseData(long id, Status? PrintCode = null, Status? PNoStatus = null, Int64? TOut = 0, Status? ComHeadCheck = null, Int64? CMRNoteNo = 0, Int64? CPorderNo = 0, Int64? CPQuotNo = 0, string CMReqNo = null)
        {
            pdfSummaryViewModel vmodel = new pdfSummaryViewModel();
            vmodel = (from a in db.PurchaseEntrys
                      join b in db.Suppliers on a.Supplier equals b.SupplierID into cust
                      from b in cust.DefaultIfEmpty()
                      join c in db.Contacts on b.Contact equals c.ContactID into cnt
                      from c in cnt.DefaultIfEmpty()
                      join e in db.PEPayments on a.PurchaseEntryId equals e.PurchaseEntry into pay
                      from e in pay.DefaultIfEmpty()
                      join d in db.Employees on a.PECashier equals d.EmployeeId into emp
                      from d in emp.DefaultIfEmpty()
                      join f in db.Accountss on b.Accounts equals f.AccountsID
                      join i in db.Contacts on d.PAddress equals i.ContactID into empcon
                      from i in empcon.DefaultIfEmpty()
                      join l in db.Mobiles on b.Contact equals l.Contact into mobi
                      from l in mobi.DefaultIfEmpty()
                      join m in db.MCs on a.MaterialCenter equals m.MCId into mcc
                      from m in mcc.DefaultIfEmpty()

                      where a.PurchaseEntryId == id
                      select new pdfSummaryViewModel
                      {
                          PartyName = b.SupplierName,
                          BillNo = a.BillNo,
                          Date = a.PEDate,
                          Note = a.PENote,
                          Cashier = d.FirstName + " " + d.MiddleName + " " + d.LastName,
                          Discount = a.PEDiscount,
                          MCTo = m.MCName,
                          GrandTotal = a.PEGrandTotal,
                          Paid = e.PEPaidAmount,
                          Balance = a.PEGrandTotal - e.PEPaidAmount,
                          SubTotal = a.PESubTotal,
                          TaxAmount = a.PETaxAmount,
                          Address = c.Address,
                          City = c.City,
                          State = c.State,
                          Country = c.Country,
                          Zip = c.Zip,
                          Email = c.EmailId,
                          Phone = c.Phone,
                          Mobile = l.MobileNum,// c.Mobile,
                          TRN = f.TRN,

                          paytype = (a.SupplierType == SupplierType.CashSale ? "Cash" : "Credit"),
                          BillId = a.PENo,
                          Remarks = a.Remarks,
                          Currency = a.Currency,
                          ConvertionRate = a.ConvertionRate,
                          FCTotal = a.FCTotal,
                          chkCode = PrintCode,
                          TimeOut = TOut,
                          ContactPerson = c.ContactPerson,
                          CMRNoteNo = CMRNoteNo,
                          CPorderNo = CPorderNo,
                          CPQuotNo = CPQuotNo,
                          CMReqNo = CMReqNo,
                          ComHeadCheck = ComHeadCheck,
                          Ref1 = a.Ref1,
                          Ref2 = a.Ref2,
                          Ref3 = a.Ref3,
                          Ref4 = a.Ref4,
                          Ref5 = a.Ref5,
                          ContactNo = i.Phone,
                          CreatedDate = a.PECreatedDate,
                          PurchaseType = a.PurchaseType
                      }).FirstOrDefault();

            var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
            var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;
            //ViewBag.StockTrnsfrUpdate = brcheck;
            var dummyvalue = db.DummyPEItems2.Where(a => a.PurchaseEntry == id).FirstOrDefault();

            if (brcheck == Status.active)
            {
                if (dummyvalue != null)
                {
                    vmodel.pdfItem = (from a in db.DummyPEItems2
                                      join b in db.Items on a.Item equals b.ItemID

                                      join d in db.Scaffolds on b.ItemID equals d.Item into scaffold
                                      from d in scaffold.DefaultIfEmpty()
                                      join c in db.ItemUnits on a.ItemUnit equals c.ItemUnitID into unit
                                      from c in unit.DefaultIfEmpty()

                                      join g in db.ItemBundles on b.ItemID equals g.mainItem into bundle
                                      from g in bundle.DefaultIfEmpty()
                                      let img = db.ItemImages.Where(im => im.ItemID == a.Item).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
                                      where a.PurchaseEntry == id && a.itemNote != "-:{Bundle_Item}"
                                      select new pdfItemViewModel
                                      {
                                          Id = b.ItemID,
                                          ItemUnitPrice = a.ItemUnitPrice,
                                          ItemQuantity = a.ItemQuantity,
                                          ItemSubTotal = a.ItemSubTotal,
                                          ItemTax = a.ItemTax,
                                          ItemNote = a.itemNote,
                                          ItemTaxAmount = a.ItemTaxAmount,
                                          ItemTotalAmount = a.ItemTotalAmount,
                                          ItemCode = b.ItemCode,
                                          ItemName = b.ItemName,
                                          ItemPrice = b.SellingPrice,
                                          Barcode = b.Barcode,
                                          ItemUnit = c.ItemUnitName,
                                          PartNumber = b.PartNumber,
                                          PNoStatus = PNoStatus,
                                          ItemDiscount = a.ItemDiscount,

                                          CBM = d.CBM,
                                          Weight = d.Weight,
                                          img = img,
                                          ItemDescription = b.ItemDescription,
                                          KeepStock = b.KeepStock,
                                          bundle = (from ab in db.PEItemss
                                                    join bb in db.Items on ab.Item equals bb.ItemID
                                                    join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                                    from dd in scaffold.DefaultIfEmpty()

                                                    join eb in db.ItemUnits on ab.ItemUnit equals eb.ItemUnitID into bpunit
                                                    from eb in bpunit.DefaultIfEmpty()

                                                    let bimg = db.ItemImages.Where(bim => bim.ItemID == ab.Item).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                                    where ab.PurchaseEntry == id && ab.itemNote == "-:{Bundle_Item}"
                                                    && b.ItemID == ab.ItemDiscount
                                                    select new pdfBundleViewModel
                                                    {
                                                        Id = bb.ItemID,
                                                        ItemUnitPrice = ab.ItemUnitPrice,
                                                        ItemQuantity = ab.ItemQuantity,
                                                        ItemSubTotal = ab.ItemSubTotal,
                                                        ItemNote = "",
                                                        ItemTax = ab.ItemTax,
                                                        ItemTaxAmount = ab.ItemTaxAmount,
                                                        ItemTotalAmount = ab.ItemTotalAmount,

                                                        ItemCode = bb.ItemCode,
                                                        ItemName = bb.ItemName,
                                                        ItemUnit = eb.ItemUnitName,
                                                        PartNumber = bb.PartNumber,
                                                        PNoStatus = PNoStatus,
                                                        CBM = dd.CBM,
                                                        Weight = dd.Weight,
                                                        img = bimg,

                                                        KeepStock = bb.KeepStock,

                                                        Item = ab.Item,
                                                        ItemDiscount = 0,
                                                        ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                        ItemUnitID = bb.ItemUnitID,
                                                        SubUnitId = bb.SubUnitId,
                                                        ItemArabic = bb.ItemArabic,
                                                        ItemDescription = bb.ItemDescription
                                                    }).ToList(),
                                      }).ToList();
                }
                else
                {
                    vmodel.pdfItem = (from a in db.PEItemss
                                      join b in db.Items on a.Item equals b.ItemID

                                      join d in db.Scaffolds on b.ItemID equals d.Item into scaffold
                                      from d in scaffold.DefaultIfEmpty()
                                      join c in db.ItemUnits on a.ItemUnit equals c.ItemUnitID into unit
                                      from c in unit.DefaultIfEmpty()

                                      join g in db.ItemBundles on b.ItemID equals g.mainItem into bundle
                                      from g in bundle.DefaultIfEmpty()
                                      let img = db.ItemImages.Where(im => im.ItemID == a.Item).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
                                      where a.PurchaseEntry == id && a.itemNote != "-:{Bundle_Item}"
                                      select new pdfItemViewModel
                                      {
                                          Id = b.ItemID,
                                          ItemUnitPrice = a.ItemUnitPrice,
                                          ItemQuantity = a.ItemQuantity,
                                          ItemSubTotal = a.ItemSubTotal,
                                          ItemTax = a.ItemTax,
                                          ItemNote = a.itemNote,
                                          ItemTaxAmount = a.ItemTaxAmount,
                                          ItemTotalAmount = a.ItemTotalAmount,
                                          ItemCode = b.ItemCode,
                                          ItemName = b.ItemName,
                                          ItemPrice = b.SellingPrice,
                                          Barcode = b.Barcode,
                                          ItemUnit = c.ItemUnitName,
                                          PartNumber = b.PartNumber,
                                          PNoStatus = PNoStatus,
                                          ItemDiscount = a.ItemDiscount,

                                          CBM = d.CBM,
                                          Weight = d.Weight,
                                          img = img,
                                          ItemDescription = b.ItemDescription,
                                          KeepStock = b.KeepStock,
                                          bundle = (from ab in db.PEItemss
                                                    join bb in db.Items on ab.Item equals bb.ItemID
                                                    join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                                    from dd in scaffold.DefaultIfEmpty()

                                                    join eb in db.ItemUnits on ab.ItemUnit equals eb.ItemUnitID into bpunit
                                                    from eb in bpunit.DefaultIfEmpty()

                                                    let bimg = db.ItemImages.Where(bim => bim.ItemID == ab.Item).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                                    where ab.PurchaseEntry == id && ab.itemNote == "-:{Bundle_Item}"
                                                    && b.ItemID == ab.ItemDiscount
                                                    select new pdfBundleViewModel
                                                    {
                                                        Id = bb.ItemID,
                                                        ItemUnitPrice = ab.ItemUnitPrice,
                                                        ItemQuantity = ab.ItemQuantity,
                                                        ItemSubTotal = ab.ItemSubTotal,
                                                        ItemNote = "",
                                                        ItemTax = ab.ItemTax,
                                                        ItemTaxAmount = ab.ItemTaxAmount,
                                                        ItemTotalAmount = ab.ItemTotalAmount,

                                                        ItemCode = bb.ItemCode,
                                                        ItemName = bb.ItemName,
                                                        ItemUnit = eb.ItemUnitName,
                                                        PartNumber = bb.PartNumber,
                                                        PNoStatus = PNoStatus,
                                                        CBM = dd.CBM,
                                                        Weight = dd.Weight,
                                                        img = bimg,

                                                        KeepStock = bb.KeepStock,

                                                        Item = ab.Item,
                                                        ItemDiscount = 0,
                                                        ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                        ItemUnitID = bb.ItemUnitID,
                                                        SubUnitId = bb.SubUnitId,
                                                        ItemArabic = bb.ItemArabic,
                                                        ItemDescription = bb.ItemDescription
                                                    }).ToList(),
                                      }).ToList();
                }
            }
            else
            {
                vmodel.pdfItem = (from a in db.PEItemss
                                  join b in db.Items on a.Item equals b.ItemID

                                  join d in db.Scaffolds on b.ItemID equals d.Item into scaffold
                                  from d in scaffold.DefaultIfEmpty()
                                  join c in db.ItemUnits on a.ItemUnit equals c.ItemUnitID into unit
                                  from c in unit.DefaultIfEmpty()

                                  join g in db.ItemBundles on b.ItemID equals g.mainItem into bundle
                                  from g in bundle.DefaultIfEmpty()
                                  let img = db.ItemImages.Where(im => im.ItemID == a.Item).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
                                  where a.PurchaseEntry == id && a.itemNote != "-:{Bundle_Item}"
                                  select new pdfItemViewModel
                                  {
                                      Id = b.ItemID,
                                      ItemUnitPrice = a.ItemUnitPrice,
                                      ItemQuantity = a.ItemQuantity,
                                      ItemSubTotal = a.ItemSubTotal,
                                      ItemTax = a.ItemTax,
                                      ItemNote = a.itemNote,
                                      ItemTaxAmount = a.ItemTaxAmount,
                                      ItemTotalAmount = a.ItemTotalAmount,
                                      ItemCode = b.ItemCode,
                                      ItemName = b.ItemName,
                                      ItemPrice = b.SellingPrice,
                                      Barcode = b.Barcode,
                                      ItemUnit = c.ItemUnitName,
                                      PartNumber = b.PartNumber,
                                      PNoStatus = PNoStatus,
                                      ItemDiscount = a.ItemDiscount,

                                      CBM = d.CBM,
                                      Weight = d.Weight,
                                      img = img,
                                      ItemDescription = b.ItemDescription,
                                      KeepStock = b.KeepStock,
                                      bundle = (from ab in db.PEItemss
                                                join bb in db.Items on ab.Item equals bb.ItemID
                                                join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                                from dd in scaffold.DefaultIfEmpty()

                                                join eb in db.ItemUnits on ab.ItemUnit equals eb.ItemUnitID into bpunit
                                                from eb in bpunit.DefaultIfEmpty()

                                                let bimg = db.ItemImages.Where(bim => bim.ItemID == ab.Item).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                                where ab.PurchaseEntry == id && ab.itemNote == "-:{Bundle_Item}"
                                                && b.ItemID == ab.ItemDiscount
                                                select new pdfBundleViewModel
                                                {
                                                    Id = bb.ItemID,
                                                    ItemUnitPrice = ab.ItemUnitPrice,
                                                    ItemQuantity = ab.ItemQuantity,
                                                    ItemSubTotal = ab.ItemSubTotal,
                                                    ItemNote = "",
                                                    ItemTax = ab.ItemTax,
                                                    ItemTaxAmount = ab.ItemTaxAmount,
                                                    ItemTotalAmount = ab.ItemTotalAmount,

                                                    ItemCode = bb.ItemCode,
                                                    ItemName = bb.ItemName,
                                                    ItemUnit = eb.ItemUnitName,
                                                    PartNumber = bb.PartNumber,
                                                    PNoStatus = PNoStatus,
                                                    CBM = dd.CBM,
                                                    Weight = dd.Weight,
                                                    img = bimg,

                                                    KeepStock = bb.KeepStock,

                                                    Item = ab.Item,
                                                    ItemDiscount = 0,
                                                    ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                    ItemUnitID = bb.ItemUnitID,
                                                    SubUnitId = bb.SubUnitId,
                                                    ItemArabic = bb.ItemArabic,
                                                    ItemDescription = bb.ItemDescription
                                                }).ToList(),
                                  }).ToList();
            }
            vmodel.billsundry = db.PEBillSundrys.Where(n => n.PurchaseEntry == id).Select(b => new pdfBillSundryViewModel
            {
                AmountType = b.AmountType,
                BsAmount = b.BsAmount,
                BsType = b.BsType,
                BsValue = b.BsValue != null ? b.BsValue : 0,
                BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
            }).ToList();

            return vmodel;
        }
        #endregion

        #region Print PurchaseReturn Details 
        public pdfSummaryViewModel PurchaseReturnData(long id, Status? PrintCode = null, Status? PNoStatus = null, Int64? TOut = 0, Status? ComHeadCheck = null)
        {
            pdfSummaryViewModel vmodel = new pdfSummaryViewModel();
            vmodel = (from a in db.PurchaseReturns
                      join b in db.Suppliers on a.Supplier equals b.SupplierID into cust
                      from b in cust.DefaultIfEmpty()
                      join c in db.Contacts on b.Contact equals c.ContactID into cnt
                      from c in cnt.DefaultIfEmpty()
                      join d in db.PRPayments on a.PurchaseReturnId equals d.PurchaseReturnId into pay
                      from d in pay.DefaultIfEmpty()
                      join e in db.Employees on a.PRCashier equals e.EmployeeId into emp
                      from e in emp.DefaultIfEmpty()
                      join i in db.Contacts on e.PAddress equals i.ContactID into empcon
                      from i in empcon.DefaultIfEmpty()
                      join f in db.Accountss on b.Accounts equals f.AccountsID
                      join l in db.Mobiles on b.Contact equals l.Contact into mobi
                      from l in mobi.DefaultIfEmpty()
                      where a.PurchaseReturnId == id
                      select new pdfSummaryViewModel
                      {
                          PartyName = b.SupplierName,
                          BillNo = a.BillNo,
                          Date = a.PRDate,
                          Note = a.PRNote,
                          Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                          Discount = a.PRDiscount,
                          //Total = a.PRDiscount + a.PRGrandTotal,
                          GrandTotal = a.PRGrandTotal,
                          Paid = d.PReturnAmount,
                          Balance = a.PRGrandTotal - d.PReturnAmount,
                          SubTotal = a.PRSubTotal,
                          TaxAmount = a.PRTaxAmount,
                          Address = c.Address,
                          City = c.City,
                          State = c.State,
                          Country = c.Country,
                          Zip = c.Zip,
                          Remarks = a.Remarks,
                          Email = c.EmailId,
                          Phone = c.Phone,
                          Mobile = l.MobileNum,//c.Mobile,
                          TRN = f.TRN,
                          paytype = (a.SupplierType == SupplierType.CashSale ? "Cash" : "Credit"),

                          BillId = a.PurchaseReturnId,
                          chkCode = PrintCode,
                          TimeOut = TOut,
                          ContactPerson = c.ContactPerson,
                          ComHeadCheck = ComHeadCheck,
                          Ref1 = a.Ref1,
                          Ref2 = a.Ref2,
                          Ref3 = a.Ref3,
                          Ref4 = a.Ref4,
                          Ref5 = a.Ref5,
                          ContactNo = i.Phone,
                          CreatedDate = a.PRCreatedDate
                      }).FirstOrDefault();

            vmodel.pdfItem = (from b in db.PRItemss
                              join c in db.Items on b.Item equals c.ItemID

                              join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                              from d in scaffold.DefaultIfEmpty()
                              join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                              from e in punit.DefaultIfEmpty()

                              join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                              from g in bundle.DefaultIfEmpty()
                              let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
                              where b.PurchaseReturnId == id && b.itemNote != "-:{Bundle_Item}"
                              select new pdfItemViewModel
                              {
                                  Id = c.ItemID,
                                  ItemUnitPrice = b.ItemUnitPrice,
                                  ItemQuantity = b.ItemQuantity,
                                  ItemSubTotal = b.ItemSubTotal,
                                  ItemTax = b.ItemTax,
                                  ItemNote = b.itemNote,
                                  ItemTaxAmount = b.ItemTaxAmount,
                                  ItemTotalAmount = b.ItemTotalAmount,
                                  ItemDiscount = b.ItemDiscount,
                                  ItemCode = c.ItemCode,
                                  ItemName = c.ItemName,
                                  ItemUnit = e.ItemUnitName,
                                  PartNumber = c.PartNumber,
                                  PNoStatus = PNoStatus,
                                  CBM = d.CBM,
                                  Weight = d.Weight,
                                  img = img,
                                  ItemDescription = c.ItemDescription,
                                  KeepStock = c.KeepStock,
                                  bundle = (from ay in db.BundleItems
                                            join az in db.ItemBundles on ay.ItemBundle equals az.ItemBundleId
                                            //join ab in db.QuotationItems on ay.ItemId equals ab.Item into quot
                                            //from ab in quot.DefaultIfEmpty()
                                            join bb in db.Items on ay.ItemId equals bb.ItemID
                                            join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                            from dd in scaffold.DefaultIfEmpty()
                                            join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
                                            from eb in bpunit.DefaultIfEmpty()
                                            let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                            where az.mainItem == b.Item
                                            select new pdfBundleViewModel
                                            {
                                                Id = bb.ItemID,
                                                ItemUnitPrice = ay.ItemUnitPrice,
                                                ItemQuantity = (ay.ItemQuantity * b.ItemQuantity),
                                                ItemSubTotal = (ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice),
                                                ItemNote = "",
                                                ItemTax = ay.ItemTax,
                                                ItemTaxAmount = ((ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice) * ay.ItemTax / 100),
                                                ItemTotalAmount = ay.ItemTotalAmount,
                                                ItemCode = bb.ItemCode,
                                                ItemName = bb.ItemName,
                                                ItemUnit = eb.ItemUnitName,
                                                PartNumber = bb.PartNumber,
                                                PNoStatus = PNoStatus,
                                                CBM = dd.CBM,
                                                Weight = dd.Weight,
                                                img = bimg,
                                                KeepStock = bb.KeepStock,
                                                Item = ay.ItemId,
                                                ItemDiscount = 0,
                                                ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                ItemUnitID = bb.ItemUnitID,
                                                SubUnitId = bb.SubUnitId,
                                                ItemArabic = bb.ItemArabic,
                                                ItemDescription = bb.ItemDescription
                                            }).ToList(),
                              }).ToList();
            vmodel.billsundry = db.PRBillSundrys.Where(n => n.PurchaseReturnId == id).Select(b => new pdfBillSundryViewModel
            {
                AmountType = b.AmountType,
                BsAmount = b.BsAmount,
                BsType = b.BsType,
                BsValue = b.BsValue != null ? b.BsValue : 0,
                BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
            }).ToList();

            return vmodel;
        }
        #endregion

        #region Print PurchaseOrder Details
        public pdfSummaryViewModel PurchaseOrderData(long id, Status? PrintCode = null, Status? PNoStatus = null, Int64? TOut = 0, Status? ComHeadCheck = null, string CMReqNo = null, Int64? CPQuotNo = 0)
        {
            pdfSummaryViewModel vmodel = new pdfSummaryViewModel();
            vmodel = (from b in db.PurchaseOrders
                      join c in db.Suppliers on b.Supplier equals c.SupplierID into supp
                      from c in supp.DefaultIfEmpty()
                      join d in db.Contacts on c.Contact equals d.ContactID into cnt
                      from d in cnt.DefaultIfEmpty()
                      join e in db.Employees on b.POCashier equals e.EmployeeId into emp
                      from e in emp.DefaultIfEmpty()
                      join f in db.Contacts on e.PAddress equals f.ContactID into empcon
                      from f in empcon.DefaultIfEmpty()
                      join h in db.Accountss on c.Accounts equals h.AccountsID
                      join g in db.Users on b.CreatedUserId equals g.Id
                      join l in db.Mobiles on c.Contact equals l.Contact into mobi
                      from l in mobi.DefaultIfEmpty()
                      join ff in db.CurrencyMasters on b.Currency equals ff.Id into curr
                      from ff in curr.DefaultIfEmpty()
                      where b.PurchaseOrderId == id
                      select new pdfSummaryViewModel
                      {

                          PartyName = c.SupplierName,
                          BillNo = b.BillNo,
                          Date = b.PODate,
                          Note = b.TermsCondition,
                          Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                          Discount = b.PODiscount,
                          GrandTotal = b.POGrandTotal,
                          SubTotal = b.POSubTotal,
                          TaxAmount = b.POTaxAmount,
                          Address = c.Addres,
                          City = d.City,
                          State = d.State,
                          Country = d.Country,
                          Zip = d.Zip,
                          Email = d.EmailId,
                          Phone = d.Phone,
                          
                          Currency = b.Currency,
                          ConvertionRate = b.ConvertionRate,
                       
                          currencycode = ff.CurrencyCode,
                          currencysymbol = ff.Symbol,
                          //FirstMobile no of Supplier fetcing
                          Mobile = (from co in db.Contacts
                                    join rrr in db.ContactRelation on co.ContactID equals rrr.ContactID
                                    where (rrr.RelationID == c.SupplierID && rrr.RelationType == 1)
                                    select (co.Mobile)).FirstOrDefault(),
                          //end

                          TRN = h.TRN,
                          PTax = b.POTax,
                          Remarks = b.Remarks,
                          validity = (DateTime.Now <= b.PODate.AddDays((b.POValidity == null) ? 0 : (b.POValidity.Value + 1))) ? "Active" : "Expired",
                          BillId = b.PONo,
                          paytype = (b.SupplierType == SupplierType.CashSale ? "Cash" : "Credit"),
                          //CreditPeriod = c.CreditPeriod,
                          chkCode = PrintCode,
                          TimeOut = TOut,
                          ContactPerson = d.ContactPerson,
                          CMReqNo = CMReqNo,
                          CPQuotNo = CPQuotNo,
                          ContactNo = f.Phone,
                          ComHeadCheck = ComHeadCheck,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          CreatedDate = b.POCreatedDate,
                          PreparedBy = g.UserName,
                          PurchaseType = b.PurchaseType
                      }).FirstOrDefault();

            vmodel.pdfItem = (from b in db.PurchaseOrderItems
                              join c in db.Items on b.Item equals c.ItemID

                              join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                              from d in scaffold.DefaultIfEmpty()
                              join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                              from e in punit.DefaultIfEmpty()

                              join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                              from g in bundle.DefaultIfEmpty()
                              join h in db.ItemBrands on b.Make equals h.ItemBrandID into brn
                              from h in brn.DefaultIfEmpty()

                              let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
                              where b.PurchaseOrder == id && b.ItemNote != "-:{Bundle_Item}"
                              select new pdfItemViewModel
                              {
                                  ItemUnitPrice = b.ItemUnitPrice,
                                  ItemQuantity = b.ItemQuantity,
                                  ItemSubTotal = b.ItemSubTotal,
                                  Barcode=c.Barcode,
                                  ItemTax = b.ItemTax,
                                  ItemNote = b.ItemNote.Replace("<br/>", "").Replace("<br>", "").Replace("<p>", "").Replace("</p>", "").Replace("<br />", ""),
                                  ItemTaxAmount = b.ItemTaxAmount,
                                  ItemTotalAmount = b.ItemTotalAmount,
                                  ItemDiscount = b.ItemDiscount,
                                  ItemCode = c.ItemCode,
                                  ItemName = c.ItemName,
                                  ItemUnit = e.ItemUnitName,
                                  PartNumber = c.PartNumber,
                                  PNoStatus = PNoStatus,
                                  CBM = d.CBM,
                                  Weight = d.Weight,
                                  img = img,
                                  ItemDescription = c.ItemDescription,
                                  KeepStock = c.KeepStock,
                                  Make = h != null ? h.ItemBrandName : "",
                                  bundle = (from ay in db.BundleItems
                                            join az in db.ItemBundles on ay.ItemBundle equals az.ItemBundleId
                                            //join ab in db.QuotationItems on ay.ItemId equals ab.Item into quot
                                            //from ab in quot.DefaultIfEmpty()
                                            join bb in db.Items on ay.ItemId equals bb.ItemID
                                            join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                            from dd in scaffold.DefaultIfEmpty()
                                            join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
                                            from eb in bpunit.DefaultIfEmpty()
                                            let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                            where az.mainItem == b.Item
                                            select new pdfBundleViewModel
                                            {
                                                Id = bb.ItemID,
                                                ItemUnitPrice = ay.ItemUnitPrice,
                                                ItemQuantity = (ay.ItemQuantity * b.ItemQuantity),
                                                ItemSubTotal = (ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice),
                                                ItemNote = "",
                                                ItemTax = ay.ItemTax,
                                                ItemTaxAmount = ((ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice) * ay.ItemTax / 100),
                                                ItemTotalAmount = ay.ItemTotalAmount,
                                                ItemCode = bb.ItemCode,
                                                ItemName = bb.ItemName,
                                                ItemUnit = eb.ItemUnitName,
                                                PartNumber = bb.PartNumber,
                                                PNoStatus = PNoStatus,
                                                CBM = dd.CBM,
                                                Weight = dd.Weight,
                                                img = bimg,
                                                KeepStock = bb.KeepStock,
                                                Item = ay.ItemId,
                                                ItemDiscount = 0,
                                                ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                ItemUnitID = bb.ItemUnitID,
                                                SubUnitId = bb.SubUnitId,
                                                ItemArabic = bb.ItemArabic,
                                                ItemDescription = bb.ItemDescription
                                            }).ToList(),
                              }).ToList();

            vmodel.billsundry = db.POBillSundrys.Where(n => n.PurchaseOrder == id).Select(b => new pdfBillSundryViewModel
            {
                AmountType = b.AmountType,
                BsAmount = b.BsAmount,
                BsType = b.BsType,
                BsValue = b.BsValue != null ? b.BsValue : 0,
                BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
            }).ToList();
            return vmodel;
        }
        #endregion

        #region Print MR Note Details
        //public pdfSummaryViewModel MRNoteData(long id, Status? PrintCode = null, Status? PNoStatus = null, Int64? TOut = 0)
        //{
        //    pdfSummaryViewModel vmodel = new pdfSummaryViewModel();
        //    vmodel = (from b in db.MaterialReceiveNotes
        //              join c in db.Suppliers on b.Supplier equals c.SupplierID into supp
        //              from c in supp.DefaultIfEmpty()
        //              join d in db.Contacts on c.Contact equals d.ContactID into cnt
        //              from d in cnt.DefaultIfEmpty()
        //              join e in db.Employees on b.Cashier equals e.EmployeeId into emp
        //              from e in emp.DefaultIfEmpty()
        //              where b.MRId == id
        //              select new pdfSummaryViewModel
        //              {

        //                  PartyName = c.SupplierName,
        //                  BillNo = b.BillNo,
        //                  Date = b.MRDate,
        //                  Note = b.Remarks,
        //                  Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
        //                  Discount = 0,
        //                  GrandTotal = 0,
        //                  SubTotal = 0,
        //                  TaxAmount = 0,
        //                  Address = d.Address,
        //                  City = d.City,
        //                  State = d.State,
        //                  Country = d.Country,
        //                  Zip = d.Zip,
        //                  Email = d.EmailId,
        //                  Phone = d.Phone,
        //                  Mobile = d.Mobile,
        //                  TRN = c.TaxRegNo,
        //                  PTax = 0,
        //                  //Remarks = b.Remarks,
        //                  //validity = (DateTime.Now <= DbFunctionsCompat.AddDays(b.PODate, (b.POValidity == null) ? 0 : b.POValidity + 1)) ? "Active" : "Expired",
        //                  BillId = b.MRNo,
        //                  //paytype = (b.SupplierType == SupplierType.CashSale ? "Cash" : "Credit"),
        //                  CreditPeriod = c.CreditPeriod,
        //                  chkCode = PrintCode,
        //                  TimeOut = TOut,
        //                  ContactPerson = d.ContactPerson
        //              }).FirstOrDefault();

        //    vmodel.pdfItem = (from b in db.MRNoteItems
        //                      join c in db.Items on b.Item equals c.ItemID

        //                      join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
        //                      from d in scaffold.DefaultIfEmpty()
        //                      join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
        //                      from e in punit.DefaultIfEmpty()

        //                      join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
        //                      from g in bundle.DefaultIfEmpty()
        //                      let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
        //                      where b.MRNote == id && b.ItemNote != "-:{Bundle_Item}"
        //                      select new pdfItemViewModel
        //                      {
        //                          ItemQuantity = b.ItemQuantity,
        //                          ItemNote = b.ItemNote,
        //                          ItemDiscount = b.ItemDiscount,
        //                          ItemCode = c.ItemCode,
        //                          ItemName = c.ItemName,
        //                          ItemUnit = e.ItemUnitName,
        //                          PartNumber = c.PartNumber,
        //                          PNoStatus = PNoStatus,
        //                          CBM = d.CBM,
        //                          Weight = d.Weight,
        //                          img = img,
        //                          ItemDescription = c.ItemDescription,
        //                          KeepStock = c.KeepStock,
        //                          bundle = (from ay in db.BundleItems
        //                                    join az in db.ItemBundles on ay.ItemBundle equals az.ItemBundleId
        //                                    //join ab in db.QuotationItems on ay.ItemId equals ab.Item into quot
        //                                    //from ab in quot.DefaultIfEmpty()
        //                                    join bb in db.Items on ay.ItemId equals bb.ItemID
        //                                    join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
        //                                    from dd in scaffold.DefaultIfEmpty()
        //                                    join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
        //                                    from eb in bpunit.DefaultIfEmpty()
        //                                    let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
        //                                    where az.mainItem == b.Item
        //                                    select new pdfBundleViewModel
        //                                    {
        //                                        Id = bb.ItemID,
        //                                        ItemUnitPrice = ay.ItemUnitPrice,
        //                                        ItemQuantity = (ay.ItemQuantity * b.ItemQuantity),
        //                                        ItemSubTotal = (ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice),
        //                                        ItemNote = "",
        //                                        ItemTax = ay.ItemTax,
        //                                        ItemTaxAmount = ((ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice) * ay.ItemTax / 100),
        //                                        ItemTotalAmount = ay.ItemTotalAmount,
        //                                        ItemCode = bb.ItemCode,
        //                                        ItemName = bb.ItemName,
        //                                        ItemUnit = eb.ItemUnitName,
        //                                        PartNumber = bb.PartNumber,
        //                                        PNoStatus = PNoStatus,
        //                                        CBM = dd.CBM,
        //                                        Weight = dd.Weight,
        //                                        img = bimg,
        //                                        KeepStock = bb.KeepStock,
        //                                        Item = ay.ItemId,
        //                                        ItemDiscount = 0,
        //                                        ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
        //                                        ItemUnitID = bb.ItemUnitID,
        //                                        SubUnitId = bb.SubUnitId,
        //                                        ItemArabic = bb.ItemArabic,
        //                                        ItemDescription = bb.ItemDescription
        //                                    }).ToList(),
        //                      }).ToList();

        //    return vmodel;
        //}

        public Dictionary<string, object> MRNoteData(long id, Status? PrintCode = null, Status? PNoStatus = null, Int64? TOut = 0, Status? ProjectCheck = null, Int64? CPorderNo = 0, Int64? CPQuotNo = 0, Int64? CMReqNo = 0, Status? ComHeadCheck = null)
        {
            var summary = (from b in db.MaterialReceiveNotes
                           join c in db.Suppliers on b.Supplier equals c.SupplierID into supp
                           from c in supp.DefaultIfEmpty()
                           join d in db.Contacts on c.Contact equals d.ContactID into cnt
                           from d in cnt.DefaultIfEmpty()
                           join e in db.Employees on b.Cashier equals e.EmployeeId into emp
                           from e in emp.DefaultIfEmpty()
                           join g in db.Users on b.CreatedUserId equals g.Id
                           join f in db.Accountss on c.Accounts equals f.AccountsID
                           join i in db.Contacts on e.PAddress equals i.ContactID into empcon
                           from i in empcon.DefaultIfEmpty()
                           join l in db.Mobiles on c.Contact equals l.Contact into mobi
                           from l in mobi.DefaultIfEmpty()
                           where b.MRId == id
                           select new
                           {
                               BillNo = b.BillNo,
                               Date = b.MRDate,
                               Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                               Remarks = b.Remarks,
                               RequestedDate = b.RequestedDate,
                               BillId = b.MRNo,
                               TimeOut = TOut,
                               CreatedBy = g.UserName,
                               PartyName = c.SupplierCode + "-" + c.SupplierName,
                               Address = d.Address,
                               City = d.City,
                               State = d.State,
                               Country = d.Country,
                               Zip = d.Zip,
                               Email = d.EmailId,
                               Phone = d.Phone,
                               Mobile = l.MobileNum,//d.Mobile,
                               TRN = f.TRN,
                               CPorderNo = CPorderNo,
                               CPQuotNo = CPQuotNo,
                               CMReqNo = CMReqNo,
                               b.Ref1,
                               b.Ref2,
                               b.Ref3,
                               b.Ref4,
                               b.Ref5,
                               ComHeadCheck = ComHeadCheck,
                               ContactNo = i.Phone,
                               CreatedDate = b.CreatedDate
                           }).FirstOrDefault();

            var item = (from b in db.MRNoteItems
                        join c in db.Items on b.Item equals c.ItemID

                        join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                        from d in scaffold.DefaultIfEmpty()
                        join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                        from e in punit.DefaultIfEmpty()

                        join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                        from g in bundle.DefaultIfEmpty()
                        join h in db.ItemBrands on b.Make equals h.ItemBrandID into brn
                        from h in brn.DefaultIfEmpty()
                        join p in db.Projects on b.ProjectId equals p.ProjectId into proj
                        from p in proj.DefaultIfEmpty()
                        join t in db.ProTasks on b.TaskId equals t.ProTaskId into protask
                        from t in protask.DefaultIfEmpty()
                        let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
                        where b.MRNote == id && b.ItemNote != "-:{Bundle_Item}"
                        select new
                        {

                            ItemNote = b.ItemNote,
                            ItemQuantity = b.ItemQuantity,
                            ItemCode = c.ItemCode,
                            ItemName = c.ItemName,
                            ItemUnit = e.ItemUnitName,
                            PartNumber = c.PartNumber,
                            PNoStatus = PNoStatus,
                            CBM = d.CBM,
                            Weight = d.Weight,
                            img = img,
                            ItemDescription = c.ItemDescription,
                            KeepStock = c.KeepStock,
                            ItemMakeID = b.Make,
                            Make = h.ItemBrandName,
                            Remarks = b.Remarks,
                            p.ProjectName,
                            t.TaskName,
                            bundle = (from ay in db.BundleItems
                                      join az in db.ItemBundles on ay.ItemBundle equals az.ItemBundleId
                                      //join ab in db.QuotationItems on ay.ItemId equals ab.Item into quot
                                      //from ab in quot.DefaultIfEmpty()
                                      join bb in db.Items on ay.ItemId equals bb.ItemID
                                      join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                      from dd in scaffold.DefaultIfEmpty()
                                      join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
                                      from eb in bpunit.DefaultIfEmpty()
                                      let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                      where az.mainItem == b.Item
                                      select new
                                      {
                                          Id = bb.ItemID,
                                          ItemUnitPrice = ay.ItemUnitPrice,
                                          ItemQuantity = (ay.ItemQuantity * b.ItemQuantity),
                                          ItemSubTotal = (ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice),
                                          ItemNote = "",
                                          ItemTax = ay.ItemTax,
                                          ItemTaxAmount = ((ay.ItemQuantity * b.ItemQuantity * ay.ItemUnitPrice) * ay.ItemTax / 100),
                                          ItemTotalAmount = ay.ItemTotalAmount,
                                          ItemCode = bb.ItemCode,
                                          ItemName = bb.ItemName,
                                          ItemUnit = eb.ItemUnitName,
                                          PartNumber = bb.PartNumber,
                                          PNoStatus = PNoStatus,
                                          CBM = dd.CBM,
                                          Weight = dd.Weight,
                                          img = bimg,
                                          KeepStock = bb.KeepStock,
                                          Item = ay.ItemId,
                                          ItemDiscount = 0,
                                          ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                          ItemUnitID = bb.ItemUnitID,
                                          SubUnitId = bb.SubUnitId,
                                          ItemArabic = bb.ItemArabic,
                                          ItemDescription = bb.ItemDescription
                                      }).ToList(),
                        }).ToList();

            var approval = db.ApprovalUpdates.Where(n => n.TransEntry == id && n.Type == "MRNote").Select(b => new
            {
                ApprovedBy = b.ApprovedBy,

                approvalBy = db.Employees.Where(a => a.UserId == b.ApprovedBy).Select(a => a.FirstName).FirstOrDefault()

            }).ToList();

            var Data = new Dictionary<string, object>();
            Data.Add("summary", summary);
            Data.Add("item", item);
            Data.Add("approval", approval);
            return Data;
        }


        #endregion

        #region Print HireReturn Details
        public pdfSummaryViewModel HireReturnData(long id, Status? PrintCode = null, Status? PNoStatus = null, Int64? TOut = 0, Status? ProjectCheck = null, Status? ComHeadCheck = null)
        {
            pdfSummaryViewModel vmodel = new pdfSummaryViewModel();
            vmodel = (from b in db.HireReturns
                      join c in db.Customers on b.Customer equals c.CustomerID into cust
                      from c in cust.DefaultIfEmpty()
                      join d in db.Contacts on c.Contact equals d.ContactID into cnt
                      from d in cnt.DefaultIfEmpty()
                      join e in db.Employees on b.Cashier equals e.EmployeeId into emp
                      from e in emp.DefaultIfEmpty()
                      join p in db.Projects on b.Project equals p.ProjectId into prjct
                      from p in prjct.DefaultIfEmpty()
                      where b.HireReturnId == id
                      join t in db.ProTasks on b.Project equals t.ProjectId into ptask
                      from t in ptask.DefaultIfEmpty()
                      join f in db.Accountss on c.Accounts equals f.AccountsID into acc
                      from f in acc.DefaultIfEmpty()
                      join i in db.Contacts on e.PAddress equals i.ContactID into empcon
                      from i in empcon.DefaultIfEmpty()
                      join l in db.Mobiles on c.Contact equals l.Contact into mobi
                      from l in mobi.DefaultIfEmpty()
                      select new pdfSummaryViewModel
                      {
                          PartyName = c.CustomerName,
                          BillId = b.HrNo,
                          BillNo = b.BillNo,
                          Date = b.Date,
                          Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                          Note = b.TermsCondition,
                          Discount = 0,
                          GrandTotal = 0,
                          Paid = null,
                          Balance = null,
                          //Total = 0,
                          SubTotal = 0,
                          TaxAmount = 0,
                          Address = d.Address,
                          City = d.City,
                          State = d.State,
                          Country = d.Country,
                          Zip = d.Zip,
                          Email = d.EmailId,
                          Phone = d.Phone,
                          Mobile = l.MobileNum,// d.Mobile,
                          TRN = f.TRN,

                          //b.Items,
                          TermsCondition = b.TermsCondition,
                          //b.ItemQuantity,

                          id = b.HireReturnId,
                          CreditPeriod = c.CreditPeriod,
                          Remarks = b.Remarks,
                          chkCode = PrintCode,
                          TimeOut = TOut,
                          ContactPerson = d.ContactPerson,
                          ProCheck = ProjectCheck,
                          PrjNameCode = (p.ProjectName != null && p.ProjectName != "") ? p.ProCode + "-" + p.ProjectName : "",
                          TaskName = (t.TaskName != null && t.TaskName != "") ? t.TaskName : "",
                          ComHeadCheck = ComHeadCheck,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          ContactNo = i.Phone,
                      }).FirstOrDefault();

            var v = (from b in db.HrItems
                     join c in db.Items on b.Item equals c.ItemID

                     join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                     from d in scaffold.DefaultIfEmpty()
                     join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                     from e in punit.DefaultIfEmpty()

                     join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                     from g in bundle.DefaultIfEmpty()
                     join h in db.HireReturns on b.Hr equals h.HireReturnId

                     where b.Hr == id && b.ItemNote != "-:{Bundle_Item}"
                     select new
                     {
                         c.ItemID,
                         b.ItemUnitPrice,
                         b.ItemQuantity,
                         b.ItemNote,
                         b.ItemDiscount,
                         c.ItemCode,
                         c.ItemName,
                         e.ItemUnitName,
                         c.PartNumber,
                         PNoStatus,
                         d.CBM,
                         d.Weight,
                         c.ItemDescription,
                         c.KeepStock,
                         b.DamageQty,
                         b.MissingQty,
                         b.ReceivedQty,

                         RetItemQuantity = (decimal?)(from aa in db.HrItems
                                                      join bb in db.HireReturns on aa.Hr equals bb.HireReturnId
                                                      where aa.Item == b.Item && bb.Invoice == h.Invoice
                                                      && aa.ItemNote != "-:{Bundle_Item}"
                                                      select new
                                                      {
                                                          aa.ItemQuantity
                                                      }).Sum(a => a.ItemQuantity) ?? 0,

                         DvItemQuantity = (decimal?)(from ab in db.SEItemss
                                                     join ba in db.SalesEntrys on ab.SalesEntry equals ba.SalesEntryId
                                                     where ab.Item == b.Item && ba.SaleType == SaleType.Hire
                                                     && ba.SalesEntryId == h.Invoice
                                                     && ab.itemNote != "-:{Bundle_Item}"
                                                     select new
                                                     {
                                                         ab.ItemQuantity
                                                     }).Sum(a => a.ItemQuantity) ?? 0,
                     }).ToList();

            vmodel.pdfItem = (from o in v
                              let img = db.ItemImages.Where(im => im.ItemID == o.ItemID).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
                              select new pdfItemViewModel
                              {

                                  Id = o.ItemID,
                                  ItemUnitPrice = o.ItemUnitPrice,
                                  ItemQuantity = o.ItemQuantity,
                                  ItemNote = o.ItemNote,
                                  ItemDiscount = o.ItemDiscount,
                                  ItemCode = o.ItemCode,
                                  ItemName = o.ItemName,
                                  ItemUnit = o.ItemUnitName,
                                  PartNumber = o.PartNumber,
                                  PNoStatus = PNoStatus,
                                  CBM = o.CBM,
                                  Weight = o.Weight,
                                  img = img,
                                  ItemDescription = o.ItemDescription,
                                  KeepStock = o.KeepStock,

                                  ItemTotalAmount = 0,
                                  ItemSubTotal = 0,
                                  ItemTaxAmount = 0,

                                  Damage = o.DamageQty,
                                  Missing = o.MissingQty,
                                  Received = o.ReceivedQty,
                                  DvItemQuantity = o.DvItemQuantity,
                                  RetItemQuantity = o.RetItemQuantity,

                                  bundle = (from ab in db.HrItems
                                            join bb in db.Items on ab.Item equals bb.ItemID
                                            join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                            from dd in scaffold.DefaultIfEmpty()

                                            join eb in db.ItemUnits on ab.ItemUnit equals eb.ItemUnitID into bpunit
                                            from eb in bpunit.DefaultIfEmpty()
                                            join zz in db.HireReturns on ab.Hr equals zz.HireReturnId
                                            let bimg = db.ItemImages.Where(bim => bim.ItemID == ab.Item).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                            where ab.Hr == id && ab.ItemNote == "-:{Bundle_Item}"
                                            && o.ItemID == ab.ItemDiscount
                                            select new pdfBundleViewModel
                                            {
                                                Id = bb.ItemID,
                                                ItemUnitPrice = ab.ItemUnitPrice,
                                                ItemQuantity = ab.ItemQuantity,
                                                ItemNote = "",
                                                ItemCode = bb.ItemCode,
                                                ItemName = bb.ItemName,
                                                ItemUnit = eb.ItemUnitName,
                                                PartNumber = bb.PartNumber,
                                                PNoStatus = PNoStatus,
                                                CBM = dd.CBM,
                                                Weight = dd.Weight,
                                                img = bimg,
                                                KeepStock = bb.KeepStock,
                                                Item = ab.Item,
                                                ItemDiscount = 0,
                                                ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                ItemUnitID = bb.ItemUnitID,
                                                SubUnitId = bb.SubUnitId,
                                                ItemArabic = bb.ItemArabic,

                                                ItemTotalAmount = 0,
                                                ItemSubTotal = 0,
                                                ItemTaxAmount = 0,

                                                ItemDescription = bb.ItemDescription,
                                                Damage = ab.DamageQty,
                                                Missing = ab.MissingQty,
                                                Received = ab.ReceivedQty,

                                                RetItemQuantity = (decimal?)(from xx in db.HrItems
                                                                             join yy in db.HireReturns on xx.Hr equals yy.HireReturnId
                                                                             where xx.ItemNote == "-:{Bundle_Item}"
                                                                             && yy.HireReturnId == id
                                                                             && ab.Item == xx.Item
                                                                             && xx.ItemDiscount == ab.ItemDiscount
                                                                             select new
                                                                             {
                                                                                 xx.ItemQuantity
                                                                             }).Sum(a => a.ItemQuantity) ?? 0,

                                                DvItemQuantity = (decimal?)(from xx in db.SEItemss
                                                                            join yy in db.SalesEntrys on xx.SalesEntry equals yy.SalesEntryId
                                                                            where xx.itemNote == "-:{Bundle_Item}"
                                                                            && yy.SalesEntryId == zz.Invoice
                                                                            && ab.Item == xx.Item
                                                                            && xx.ItemDiscount == ab.ItemDiscount
                                                                            select new
                                                                            {
                                                                                xx.ItemQuantity
                                                                            }).Sum(a => a.ItemQuantity) ?? 0,

                                            }).ToList(),
                              }).ToList();


            return vmodel;
        }
        #endregion
        #region Print HireReturn Details
        public pdfSummaryViewModel CrossHireReturnData(long id, Status? PrintCode = null, Status? PNoStatus = null, Int64? TOut = 0, Status? ProjectCheck = null, Status? ComHeadCheck = null)
        {
            pdfSummaryViewModel vmodel = new pdfSummaryViewModel();
            vmodel = (from b in db.CrossHireReturns
                      join c in db.Suppliers on b.Supplier equals c.SupplierID into cust
                      from c in cust.DefaultIfEmpty()
                      join d in db.Contacts on c.Contact equals d.ContactID into cnt
                      from d in cnt.DefaultIfEmpty()
                      join e in db.Employees on b.Cashier equals e.EmployeeId into emp
                      from e in emp.DefaultIfEmpty()
                      join p in db.Projects on b.Project equals p.ProjectId into prjct
                      from p in prjct.DefaultIfEmpty()
                      where b.HireReturnId == id
                      join t in db.ProTasks on b.Project equals t.ProjectId into ptask
                      from t in ptask.DefaultIfEmpty()
                      join f in db.Accountss on c.Accounts equals f.AccountsID into acc
                      from f in acc.DefaultIfEmpty()
                      join i in db.Contacts on e.PAddress equals i.ContactID into empcon
                      from i in empcon.DefaultIfEmpty()
                      join l in db.Mobiles on c.Contact equals l.Contact into mobi
                      from l in mobi.DefaultIfEmpty()
                      select new pdfSummaryViewModel
                      {
                          PartyName = c.SupplierName,
                          BillId = b.HrNo,
                          BillNo = b.BillNo,
                          Date = b.Date,
                          Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                          Note = b.TermsCondition,
                          Discount = 0,
                          GrandTotal = 0,
                          Paid = null,
                          Balance = null,
                          //Total = 0,
                          SubTotal = 0,
                          TaxAmount = 0,
                          Address = d.Address,
                          City = d.City,
                          State = d.State,
                          Country = d.Country,
                          Zip = d.Zip,
                          Email = d.EmailId,
                          Phone = d.Phone,
                          Mobile = l.MobileNum,//d.Mobile,
                          TRN = f.TRN,

                          //b.Items,
                          TermsCondition = b.TermsCondition,
                          //b.ItemQuantity,

                          id = b.HireReturnId,
                          CreditPeriod = c.CreditPeriod,
                          Remarks = b.Remarks,
                          chkCode = PrintCode,
                          TimeOut = TOut,
                          ContactPerson = d.ContactPerson,
                          ProCheck = ProjectCheck,
                          PrjNameCode = (p.ProjectName != null && p.ProjectName != "") ? p.ProCode + "-" + p.ProjectName : "",
                          TaskName = (t.TaskName != null && t.TaskName != "") ? t.TaskName : "",
                          ComHeadCheck = ComHeadCheck,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          ContactNo = i.Phone,
                          CreatedDate = b.CreatedDate
                      }).FirstOrDefault();

            var v = (from b in db.CrossHrItems
                     join c in db.Items on b.Item equals c.ItemID

                     join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                     from d in scaffold.DefaultIfEmpty()
                     join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                     from e in punit.DefaultIfEmpty()

                     join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                     from g in bundle.DefaultIfEmpty()
                     join h in db.CrossHireReturns on b.Hr equals h.HireReturnId

                     where b.Hr == id && b.ItemNote != "-:{Bundle_Item}"
                     select new
                     {
                         c.ItemID,
                         b.ItemUnitPrice,
                         b.ItemQuantity,
                         b.ItemNote,
                         b.ItemDiscount,
                         c.ItemCode,
                         c.ItemName,
                         e.ItemUnitName,
                         c.PartNumber,
                         PNoStatus,
                         d.CBM,
                         d.Weight,
                         c.ItemDescription,
                         c.KeepStock,
                         b.DamageQty,
                         b.MissingQty,
                         b.ReceivedQty,

                         RetItemQuantity = (decimal?)(from aa in db.CrossHrItems
                                                      join bb in db.HireReturns on aa.Hr equals bb.HireReturnId
                                                      where aa.Item == b.Item && bb.Invoice == h.Invoice
                                                      && aa.ItemNote != "-:{Bundle_Item}"
                                                      select new
                                                      {
                                                          aa.ItemQuantity
                                                      }).Sum(a => a.ItemQuantity) ?? 0,

                         DvItemQuantity = (decimal?)(from ab in db.SEItemss
                                                     join ba in db.SalesEntrys on ab.SalesEntry equals ba.SalesEntryId
                                                     where ab.Item == b.Item && ba.SaleType == SaleType.Hire
                                                     && ba.SalesEntryId == h.Invoice
                                                     && ab.itemNote != "-:{Bundle_Item}"
                                                     select new
                                                     {
                                                         ab.ItemQuantity
                                                     }).Sum(a => a.ItemQuantity) ?? 0,
                     }).ToList();

            vmodel.pdfItem = (from o in v
                              let img = db.ItemImages.Where(im => im.ItemID == o.ItemID).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
                              select new pdfItemViewModel
                              {

                                  Id = o.ItemID,
                                  ItemUnitPrice = o.ItemUnitPrice,
                                  ItemQuantity = o.ItemQuantity,
                                  ItemNote = o.ItemNote,
                                  ItemDiscount = o.ItemDiscount,
                                  ItemCode = o.ItemCode,
                                  ItemName = o.ItemName,
                                  ItemUnit = o.ItemUnitName,
                                  PartNumber = o.PartNumber,
                                  PNoStatus = PNoStatus,
                                  CBM = o.CBM,
                                  Weight = o.Weight,
                                  img = img,
                                  ItemDescription = o.ItemDescription,
                                  KeepStock = o.KeepStock,

                                  ItemTotalAmount = 0,
                                  ItemSubTotal = 0,
                                  ItemTaxAmount = 0,

                                  Damage = o.DamageQty,
                                  Missing = o.MissingQty,
                                  Received = o.ReceivedQty,
                                  DvItemQuantity = o.DvItemQuantity,
                                  RetItemQuantity = o.RetItemQuantity,

                                  bundle = (from ab in db.CrossHrItems
                                            join bb in db.Items on ab.Item equals bb.ItemID
                                            join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                            from dd in scaffold.DefaultIfEmpty()

                                            join eb in db.ItemUnits on ab.ItemUnit equals eb.ItemUnitID into bpunit
                                            from eb in bpunit.DefaultIfEmpty()
                                            join zz in db.CrossHireReturns on ab.Hr equals zz.HireReturnId
                                            let bimg = db.ItemImages.Where(bim => bim.ItemID == ab.Item).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                            where ab.Hr == id && ab.ItemNote == "-:{Bundle_Item}"
                                            && o.ItemID == ab.ItemDiscount
                                            select new pdfBundleViewModel
                                            {
                                                Id = bb.ItemID,
                                                ItemUnitPrice = ab.ItemUnitPrice,
                                                ItemQuantity = ab.ItemQuantity,
                                                ItemNote = "",
                                                ItemCode = bb.ItemCode,
                                                ItemName = bb.ItemName,
                                                ItemUnit = eb.ItemUnitName,
                                                PartNumber = bb.PartNumber,
                                                PNoStatus = PNoStatus,
                                                CBM = dd.CBM,
                                                Weight = dd.Weight,
                                                img = bimg,
                                                KeepStock = bb.KeepStock,
                                                Item = ab.Item,
                                                ItemDiscount = 0,
                                                ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                ItemUnitID = bb.ItemUnitID,
                                                SubUnitId = bb.SubUnitId,
                                                ItemArabic = bb.ItemArabic,

                                                ItemTotalAmount = 0,
                                                ItemSubTotal = 0,
                                                ItemTaxAmount = 0,

                                                ItemDescription = bb.ItemDescription,
                                                Damage = ab.DamageQty,
                                                Missing = ab.MissingQty,
                                                Received = ab.ReceivedQty,

                                                RetItemQuantity = (decimal?)(from xx in db.CrossHrItems
                                                                             join yy in db.CrossHireReturns on xx.Hr equals yy.HireReturnId
                                                                             where xx.ItemNote == "-:{Bundle_Item}"
                                                                             && yy.HireReturnId == id
                                                                             && ab.Item == xx.Item
                                                                             && xx.ItemDiscount == ab.ItemDiscount
                                                                             select new
                                                                             {
                                                                                 xx.ItemQuantity
                                                                             }).Sum(a => a.ItemQuantity) ?? 0,

                                                DvItemQuantity = (decimal?)(from xx in db.SEItemss
                                                                            join yy in db.SalesEntrys on xx.SalesEntry equals yy.SalesEntryId
                                                                            where xx.itemNote == "-:{Bundle_Item}"
                                                                            && yy.SalesEntryId == zz.Invoice
                                                                            && ab.Item == xx.Item
                                                                            && xx.ItemDiscount == ab.ItemDiscount
                                                                            select new
                                                                            {
                                                                                xx.ItemQuantity
                                                                            }).Sum(a => a.ItemQuantity) ?? 0,

                                            }).ToList(),
                              }).ToList();


            return vmodel;
        }
        #endregion

        #region Print Packing List Details
        public pdfSummaryViewModel PackingListData(long id, Status? PrintCode = null, Status? PNoStatus = null, Int64? TOut = 0, Status? ComHeadCheck = null)
        {
            pdfSummaryViewModel vmodel = new pdfSummaryViewModel();
            vmodel = (from b in db.PackingLists
                      join c in db.Customers on b.Customer equals c.CustomerID into cust
                      from c in cust.DefaultIfEmpty()
                      join d in db.Contacts on c.Contact equals d.ContactID into cnt
                      from d in cnt.DefaultIfEmpty()
                      join e in db.Employees on b.Employee equals e.EmployeeId into emp
                      from e in emp.DefaultIfEmpty()
                      join f in db.Accountss on c.Accounts equals f.AccountsID into acc
                      from f in acc.DefaultIfEmpty()
                      join i in db.Contacts on e.PAddress equals i.ContactID into empcon
                      from i in empcon.DefaultIfEmpty()
                      join l in db.Mobiles on c.Contact equals l.Contact into mobi
                      from l in mobi.DefaultIfEmpty()
                      where b.PackinglistId == id
                      select new pdfSummaryViewModel
                      {
                          PartyName = c.CustomerName,
                          BillId = b.PackinglistId,
                          BillNo = b.BillNo,
                          Date = b.PLDate,
                          Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                          Note = b.TermAndCondition,
                          Discount = 0,
                          GrandTotal = 0,
                          Paid = null,
                          Balance = null,
                          //Total = 0,
                          SubTotal = 0,
                          TaxAmount = 0,
                          Address = d.Address,
                          City = d.City,
                          State = d.State,
                          Country = d.Country,
                          Zip = d.Zip,
                          Email = d.EmailId,
                          Phone = d.Phone,
                          Mobile = l.MobileNum,//d.Mobile,
                          TRN = f.TRN,

                          id = b.PackinglistId,
                          CreditPeriod = c.CreditPeriod,
                          Remarks = b.Remarks,
                          chkCode = PrintCode,
                          TimeOut = TOut,
                          ContactPerson = d.ContactPerson,
                          HSCode = b.HSCode,
                          TermsCondition = b.TermAndCondition,
                          ComHeadCheck = ComHeadCheck,
                          Ref1 = b.Ref1,
                          Ref2 = b.Ref2,
                          Ref3 = b.Ref3,
                          Ref4 = b.Ref4,
                          Ref5 = b.Ref5,
                          ContactNo = i.Phone,
                          CreatedDate = b.CreatedDate
                      }).FirstOrDefault();

            var v = (from b in db.PLItems
                     join c in db.Items on b.Item equals c.ItemID

                     join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                     from d in scaffold.DefaultIfEmpty()
                     join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                     from e in punit.DefaultIfEmpty()

                     join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                     from g in bundle.DefaultIfEmpty()
                     join h in db.PackingLists on b.PackingListId equals h.PackinglistId

                     //let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new { im.FileName, im.Status, im.ItemImageID }).ToList()
                     where b.PackingListId == id && b.ItemNote != "-:{Bundle_Item}"
                     select new
                     {
                         c.ItemID,
                         b.ItemQuantity,
                         b.ItemNote,
                         b.ItemDiscount,
                         c.ItemCode,
                         c.ItemName,
                         e.ItemUnitName,
                         c.PartNumber,
                         PNoStatus,
                         d.CBM,
                         d.Weight,
                         //img,
                         c.ItemDescription,
                         c.KeepStock,

                         b.Packet,
                         b.MinQty
                     }).ToList();

            vmodel.pdfItem = (from o in v
                              let img = db.ItemImages.Where(im => im.ItemID == o.ItemID).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()

                              select new pdfItemViewModel
                              {

                                  Id = o.ItemID,
                                  ItemQuantity = o.ItemQuantity,
                                  ItemNote = o.ItemNote,
                                  ItemDiscount = o.ItemDiscount,
                                  ItemCode = o.ItemCode,
                                  ItemName = o.ItemName,
                                  ItemUnit = o.ItemUnitName,
                                  PartNumber = o.PartNumber,
                                  PNoStatus = PNoStatus,
                                  CBM = o.CBM,
                                  Weight = o.Weight,
                                  img = img,
                                  ItemDescription = o.ItemDescription,
                                  KeepStock = o.KeepStock,
                                  Packet = o.Packet,
                                  MinQty = o.MinQty,

                                  bundle = (from ay in db.BundleItems
                                            join az in db.ItemBundles on ay.ItemBundle equals az.ItemBundleId
                                            join ab in db.PLItems on ay.ItemId equals ab.Item
                                            join bb in db.Items on ay.ItemId equals bb.ItemID
                                            join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                            from dd in scaffold.DefaultIfEmpty()
                                            join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
                                            from eb in bpunit.DefaultIfEmpty()
                                            let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                            where az.mainItem == o.ItemID && ab.PackingListId == id && ab.ItemNote == "-:{Bundle_Item}"
                                            && o.ItemID == ab.ItemDiscount
                                            select new pdfBundleViewModel
                                            {
                                                Id = bb.ItemID,
                                                ItemUnitPrice = ay.ItemUnitPrice,
                                                ItemQuantity = (ay.ItemQuantity * o.ItemQuantity),
                                                ItemSubTotal = (ay.ItemQuantity * o.ItemQuantity * ay.ItemUnitPrice),
                                                ItemNote = "",
                                                ItemTax = ay.ItemTax,
                                                ItemTaxAmount = ((ay.ItemQuantity * o.ItemQuantity * ay.ItemUnitPrice) * ay.ItemTax / 100),
                                                ItemTotalAmount = ay.ItemTotalAmount,
                                                ItemCode = bb.ItemCode,
                                                ItemName = bb.ItemName,
                                                ItemUnit = eb.ItemUnitName,
                                                PartNumber = bb.PartNumber,
                                                PNoStatus = PNoStatus,
                                                CBM = dd.CBM,
                                                Weight = dd.Weight,
                                                img = bimg,
                                                KeepStock = bb.KeepStock,
                                                Item = ay.ItemId,
                                                ItemDiscount = 0,
                                                ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                ItemUnitID = bb.ItemUnitID,
                                                SubUnitId = bb.SubUnitId,
                                                ItemArabic = bb.ItemArabic,
                                                ItemDescription = bb.ItemDescription,
                                                Packet = ab.Packet,
                                                MinQty = ab.MinQty,
                                            }).ToList(),
                              }).ToList();


            return vmodel;
        }
        #endregion

        #region Print MaterialRequisition Details


        public Dictionary<string, object> MaterialRequisitionData(long id, Status? PrintCode = null, Status? PNoStatus = null, Int64? TOut = 0, Status? ProjectCheck = null, Status? BranchCheck = null, Status? ComHeadCheckk = null)
        {
            var summary = (from b in db.MaterialRequisitions
                           join e in db.Employees on b.MRCashier equals e.EmployeeId into emp
                           from e in emp.DefaultIfEmpty()
                           join p in db.Projects on b.Project equals p.ProjectId into prjct
                           from p in prjct.DefaultIfEmpty()
                           join t in db.ProTasks on b.Project equals t.ProjectId into ptask
                           from t in ptask.DefaultIfEmpty()
                           join c in db.Branchs on b.Branch equals c.BranchID into branch
                           from c in branch.DefaultIfEmpty()
                           join i in db.Contacts on e.PAddress equals i.ContactID into empcon
                           from i in empcon.DefaultIfEmpty()
                           where b.MaterialRequisitionId == id
                           select new
                           {
                               BillNo = b.BillNo,
                               Date = b.MRDate,
                               Cashier = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                               Remarks = b.Remarks,
                               validity = b.MRValidity,
                               BillId = b.MRNo,
                               ProCheck = ProjectCheck,
                               PrjNameCode = (p.ProjectName != null && p.ProjectName != "") ? p.ProCode + "-" + p.ProjectName : "",
                               TaskName = (t.TaskName != null && t.TaskName != "") ? t.TaskName : "",
                               chkCode = PrintCode,
                               TimeOut = TOut,
                               BranchCheck = BranchCheck,
                               BranchNameCode = (c.BranchName != null && c.BranchName != "") ? c.BranchCode + "-" + c.BranchName : "",
                               b.Ref1,
                               b.Ref2,
                               b.Ref3,
                               b.Ref4,
                               b.Ref5,
                               ComHeadCheckk = ComHeadCheckk,
                               ContactNo = i.Phone,
                               CreatedDate = b.MRCreatedDate
                           }).FirstOrDefault();

            var item = (from b in db.MaterialRequisitionItems
                        join c in db.Items on b.Item equals c.ItemID

                        join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                        from d in scaffold.DefaultIfEmpty()
                        join e in db.ItemUnits on b.ItemUnit equals e.ItemUnitID into punit
                        from e in punit.DefaultIfEmpty()

                        join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                        from g in bundle.DefaultIfEmpty()
                        join h in db.ItemBrands on b.Make equals h.ItemBrandID into brn
                        from h in brn.DefaultIfEmpty()
                        let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()

                        where b.MaterialRequisition == id && b.ItemNote != "-:{Bundle_Item}"
                        select new
                        {

                            ItemNote = b.ItemNote,
                            ItemRemark = b.ItemRemark,
                            ItemQuantity = b.ItemQuantity,
                            ItemCode = c.ItemCode,
                            ItemName = c.ItemName,
                            ItemUnit = e.ItemUnitName,
                            PartNumber = c.PartNumber,
                            PNoStatus = PNoStatus,
                            CBM = d.CBM,
                            Weight = d.Weight,
                            img = img,
                            ItemDescription = c.ItemDescription,
                            KeepStock = c.KeepStock,
                            ItemMakeID = b.Make,
                            Make = h.ItemBrandName,
                            bundle = (from ay in db.BundleItems
                                      join az in db.ItemBundles on ay.ItemBundle equals az.ItemBundleId

                                      join bb in db.Items on ay.ItemId equals bb.ItemID
                                      join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                      from dd in scaffold.DefaultIfEmpty()
                                      join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
                                      from eb in bpunit.DefaultIfEmpty()
                                      let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                      where az.mainItem == b.Item
                                      select new
                                      {
                                          Id = bb.ItemID,

                                          ItemQuantity = (ay.ItemQuantity * b.ItemQuantity),

                                          ItemNote = "",
                                          ItemCode = bb.ItemCode,
                                          ItemName = bb.ItemName,
                                          ItemUnit = eb.ItemUnitName,
                                          PartNumber = bb.PartNumber,
                                          PNoStatus = PNoStatus,
                                          CBM = dd.CBM,
                                          Weight = dd.Weight,
                                          img = bimg,
                                          KeepStock = bb.KeepStock,
                                          Item = ay.ItemId,

                                          ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                          ItemUnitID = bb.ItemUnitID,
                                          SubUnitId = bb.SubUnitId,
                                          ItemArabic = bb.ItemArabic,
                                          ItemDescription = bb.ItemDescription
                                      }).ToList(),
                        }).ToList();


            var cdetails = db.companys
            .Select(s => new
            {
                CName = s.CPName,
                CAddress = s.CPAddress,
                CEmail = s.CPEmail,

            }).FirstOrDefault();

            var approval = db.ApprovalUpdates.Where(n => n.TransEntry == id && n.Type == "MaterialRequisition").Select(b => new
            {
                ApprovedBy = b.ApprovedBy,

                approvalBy = db.Employees.Where(a => a.UserId == b.ApprovedBy).Select(a => a.FirstName).FirstOrDefault()

            }).ToList();

            var Data = new Dictionary<string, object>();
            Data.Add("summary", summary);
            Data.Add("item", item);
            Data.Add("cdetails", cdetails);
            Data.Add("approval", approval);
            return Data;
        }
        #endregion

        #region print Stock Transfer Details
        public pdfSummaryViewModel StockTransferDatanew(long id, Status? PrintCode = null, Status? PNoStatus = null, Int64? TOut = 0, Status? ProjectCheck = null, Status? ComHeadCheck = null)
        {
            pdfSummaryViewModel vmodel = new pdfSummaryViewModel();

            vmodel = (from a in db.StockTransfers
                      join b in db.MCs on a.MCFrom equals b.MCId
                      join c in db.MCs on a.MCTo equals c.MCId
                      join d in db.Employees on a.CreatedBy equals d.UserId
                      where a.Id == id
                      select new pdfSummaryViewModel
                      {
                          PartyName = d.FirstName,
                          Date = a.Date,
                          BillNo = a.Voucher,
                          MCFrom = b.MCName,
                          MCTo = c.MCName,
                          GrandTotal = a.TotalAmount,
                          Remarks = a.Remarks,
                          CreatedDate = a.CreatedDate,
                          chkCode = PrintCode,
                          TimeOut = TOut,
                          ProCheck = ProjectCheck,
                          ComHeadCheck = ComHeadCheck
                      }).FirstOrDefault();
            var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
            var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;
            //ViewBag.StockTrnsfrUpdate = brcheck;
            if (brcheck == Status.active)
            {
                var dummyvalue = db.DummyStkTrsItem2.Where(a => a.StockTransferId == id).FirstOrDefault();
                if (dummyvalue != null)
                {
                    vmodel.pdfItem = (from b in db.DummyStkTrsItem2
                                      join c in db.Items on b.Item equals c.ItemID

                                      join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                                      from d in scaffold.DefaultIfEmpty()
                                      join e in db.ItemUnits on b.Unit equals e.ItemUnitID into punit
                                      from e in punit.DefaultIfEmpty()

                                      join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                                      from g in bundle.DefaultIfEmpty()
                                      let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
                                      where b.StockTransferId == id /*&& b.Note != "-:{Bundle_Item}"*/
                                      select new pdfItemViewModel
                                      {
                                          ItemPrice = b.Price,
                                          ItemUnitPrice = b.Price,
                                          ItemQuantity = b.Quantity,
                                          ItemTotalAmount = b.Amount,
                                          ItemSubTotal = b.Amount,
                                          ItemTax = 0,
                                          ItemTaxAmount = 0,
                                          ItemDiscount = 0,
                                          ItemCode = c.ItemCode,
                                          ItemName = c.ItemName,
                                          ItemUnit = e.ItemUnitName,
                                          PartNumber = c.PartNumber,
                                          PNoStatus = PNoStatus,
                                          CBM = d.CBM,
                                          Weight = d.Weight,
                                          img = img,
                                          ItemDescription = c.ItemDescription,
                                          KeepStock = c.KeepStock,
                                          bundle = (from ay in db.BundleItems
                                                    join az in db.StockTransferItems on ay.ItemBundle equals az.Id
                                                    //join ab in db.QuotationItems on ay.ItemId equals ab.Item into quot
                                                    //from ab in quot.DefaultIfEmpty()
                                                    join bb in db.Items on ay.ItemId equals bb.ItemID
                                                    join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                                    from dd in scaffold.DefaultIfEmpty()
                                                    join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
                                                    from eb in bpunit.DefaultIfEmpty()
                                                    let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                                    where az.Item == b.Item
                                                    select new pdfBundleViewModel
                                                    {
                                                        Id = bb.ItemID,
                                                        ItemUnitPrice = ay.ItemUnitPrice,
                                                        ItemQuantity = (ay.ItemQuantity * b.Quantity),
                                                        ItemSubTotal = (ay.ItemQuantity * b.Quantity * ay.ItemUnitPrice),
                                                        ItemNote = "",
                                                        ItemTax = ay.ItemTax,
                                                        ItemTaxAmount = ((ay.ItemQuantity * b.Quantity * ay.ItemUnitPrice) * ay.ItemTax / 100),
                                                        ItemTotalAmount = ay.ItemTotalAmount,
                                                        ItemCode = bb.ItemCode,
                                                        ItemName = bb.ItemName,
                                                        ItemUnit = eb.ItemUnitName,
                                                        PartNumber = bb.PartNumber,
                                                        PNoStatus = PNoStatus,
                                                        CBM = dd.CBM,
                                                        Weight = dd.Weight,
                                                        img = bimg,
                                                        KeepStock = bb.KeepStock,
                                                        Item = ay.ItemId,
                                                        ItemDiscount = 0,
                                                        ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                        ItemUnitID = bb.ItemUnitID,
                                                        SubUnitId = bb.SubUnitId,
                                                        ItemArabic = bb.ItemArabic,
                                                        ItemDescription = bb.ItemDescription
                                                    }).ToList()
                                      }).ToList();

                }
                else
                {
                    vmodel.pdfItem = (from b in db.StockTransferItems
                                      join c in db.Items on b.Item equals c.ItemID

                                      join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                                      from d in scaffold.DefaultIfEmpty()
                                      join e in db.ItemUnits on b.Unit equals e.ItemUnitID into punit
                                      from e in punit.DefaultIfEmpty()

                                      join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                                      from g in bundle.DefaultIfEmpty()
                                      let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
                                      where b.StockTransferId == id /*&& b.Note != "-:{Bundle_Item}"*/
                                      select new pdfItemViewModel
                                      {
                                          ItemPrice = b.Price,
                                          ItemUnitPrice = b.Price,
                                          ItemQuantity = b.Quantity,
                                          ItemTotalAmount = b.Amount,
                                          ItemSubTotal = b.Amount,
                                          ItemTax = 0,
                                          ItemTaxAmount = 0,
                                          ItemDiscount = 0,
                                          ItemCode = c.ItemCode,
                                          ItemName = c.ItemName,
                                          ItemUnit = e.ItemUnitName,
                                          PartNumber = c.PartNumber,
                                          PNoStatus = PNoStatus,
                                          CBM = d.CBM,
                                          Weight = d.Weight,
                                          img = img,
                                          ItemDescription = c.ItemDescription,
                                          KeepStock = c.KeepStock,
                                          bundle = (from ay in db.BundleItems
                                                    join az in db.StockTransferItems on ay.ItemBundle equals az.Id
                                                    //join ab in db.QuotationItems on ay.ItemId equals ab.Item into quot
                                                    //from ab in quot.DefaultIfEmpty()
                                                    join bb in db.Items on ay.ItemId equals bb.ItemID
                                                    join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                                    from dd in scaffold.DefaultIfEmpty()
                                                    join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
                                                    from eb in bpunit.DefaultIfEmpty()
                                                    let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                                    where az.Item == b.Item
                                                    select new pdfBundleViewModel
                                                    {
                                                        Id = bb.ItemID,
                                                        ItemUnitPrice = ay.ItemUnitPrice,
                                                        ItemQuantity = (ay.ItemQuantity * b.Quantity),
                                                        ItemSubTotal = (ay.ItemQuantity * b.Quantity * ay.ItemUnitPrice),
                                                        ItemNote = "",
                                                        ItemTax = ay.ItemTax,
                                                        ItemTaxAmount = ((ay.ItemQuantity * b.Quantity * ay.ItemUnitPrice) * ay.ItemTax / 100),
                                                        ItemTotalAmount = ay.ItemTotalAmount,
                                                        ItemCode = bb.ItemCode,
                                                        ItemName = bb.ItemName,
                                                        ItemUnit = eb.ItemUnitName,
                                                        PartNumber = bb.PartNumber,
                                                        PNoStatus = PNoStatus,
                                                        CBM = dd.CBM,
                                                        Weight = dd.Weight,
                                                        img = bimg,
                                                        KeepStock = bb.KeepStock,
                                                        Item = ay.ItemId,
                                                        ItemDiscount = 0,
                                                        ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                        ItemUnitID = bb.ItemUnitID,
                                                        SubUnitId = bb.SubUnitId,
                                                        ItemArabic = bb.ItemArabic,
                                                        ItemDescription = bb.ItemDescription
                                                    }).ToList()
                                      }).ToList();

                }
            }
            else
            {
                vmodel.pdfItem = (from b in db.StockTransferItems
                                  join c in db.Items on b.Item equals c.ItemID

                                  join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                                  from d in scaffold.DefaultIfEmpty()
                                  join e in db.ItemUnits on b.Unit equals e.ItemUnitID into punit
                                  from e in punit.DefaultIfEmpty()

                                  join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                                  from g in bundle.DefaultIfEmpty()
                                  let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new pdfItemImg { FileName = im.FileName, Status = im.Status, ItemImageID = im.ItemImageID }).ToList()
                                  where b.StockTransferId == id /*&& b.Note != "-:{Bundle_Item}"*/
                                  select new pdfItemViewModel
                                  {
                                      ItemPrice = b.Price,
                                      ItemUnitPrice = b.Price,
                                      ItemQuantity = b.Quantity,
                                      ItemTotalAmount = b.Amount,
                                      ItemSubTotal = b.Amount,
                                      ItemTax = 0,
                                      ItemTaxAmount = 0,
                                      ItemDiscount = 0,
                                      ItemCode = c.ItemCode,
                                      ItemName = c.ItemName,
                                      ItemUnit = e.ItemUnitName,
                                      PartNumber = c.PartNumber,
                                      PNoStatus = PNoStatus,
                                      CBM = d.CBM,
                                      Weight = d.Weight,
                                      img = img,
                                      ItemDescription = c.ItemDescription,
                                      KeepStock = c.KeepStock,
                                      bundle = (from ay in db.BundleItems
                                                join az in db.StockTransferItems on ay.ItemBundle equals az.Id
                                                //join ab in db.QuotationItems on ay.ItemId equals ab.Item into quot
                                                //from ab in quot.DefaultIfEmpty()
                                                join bb in db.Items on ay.ItemId equals bb.ItemID
                                                join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                                from dd in scaffold.DefaultIfEmpty()
                                                join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
                                                from eb in bpunit.DefaultIfEmpty()
                                                let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new pdfItemImg { FileName = bim.FileName, Status = bim.Status, ItemImageID = bim.ItemImageID }).ToList()
                                                where az.Item == b.Item
                                                select new pdfBundleViewModel
                                                {
                                                    Id = bb.ItemID,
                                                    ItemUnitPrice = ay.ItemUnitPrice,
                                                    ItemQuantity = (ay.ItemQuantity * b.Quantity),
                                                    ItemSubTotal = (ay.ItemQuantity * b.Quantity * ay.ItemUnitPrice),
                                                    ItemNote = "",
                                                    ItemTax = ay.ItemTax,
                                                    ItemTaxAmount = ((ay.ItemQuantity * b.Quantity * ay.ItemUnitPrice) * ay.ItemTax / 100),
                                                    ItemTotalAmount = ay.ItemTotalAmount,
                                                    ItemCode = bb.ItemCode,
                                                    ItemName = bb.ItemName,
                                                    ItemUnit = eb.ItemUnitName,
                                                    PartNumber = bb.PartNumber,
                                                    PNoStatus = PNoStatus,
                                                    CBM = dd.CBM,
                                                    Weight = dd.Weight,
                                                    img = bimg,
                                                    KeepStock = bb.KeepStock,
                                                    Item = ay.ItemId,
                                                    ItemDiscount = 0,
                                                    ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                                    ItemUnitID = bb.ItemUnitID,
                                                    SubUnitId = bb.SubUnitId,
                                                    ItemArabic = bb.ItemArabic,
                                                    ItemDescription = bb.ItemDescription
                                                }).ToList()
                                  }).ToList();
            }
            vmodel.billsundry = db.StockTransferBSundrys.Where(n => n.StockTransferId == id).Select(b => new pdfBillSundryViewModel
            {
                AmountType = b.AmountType,
                BsAmount = b.BsAmount,
                BsType = b.BsType,
                BsValue = b.BsValue != null ? b.BsValue : 0,
                BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
            }).ToList();

            var cdetails = db.companys
            .Select(s => new
            {
                CName = s.CPName,
                CAddress = s.CPAddress,
                CEmail = s.CPEmail,

            }).FirstOrDefault();

            return vmodel;

        }
        #endregion

        #region print Stock Transfer Details
        public Dictionary<string, object> StockTransferData(long id, Status? PrintCode = null, Status? PNoStatus = null, Int64? TOut = 0, Status? ProjectCheck = null, Status? ComHeadCheck = null)
        {
            var stktrnfrupdt = db.EnableSettings.Where(a => a.EnableType == "StockTransferUpdate").FirstOrDefault();
            var brcheck = stktrnfrupdt != null ? stktrnfrupdt.Status : Status.inactive;
            //ViewBag.StockTrnsfrUpdate = brcheck;
            var dumit = db.DummyStkTrsItem2.Any(o => o.StockTransferId == id);
            if (brcheck == Status.active && dumit == true)
            {

                var summary = (from a in db.StockTransfers
                               join b in db.MCs on a.MCFrom equals b.MCId
                               join c in db.MCs on a.MCTo equals c.MCId
                               join d in db.Employees on a.CreatedBy equals d.UserId into yy
                               from d in yy.DefaultIfEmpty()
                               where a.Id == id
                               select new
                               {
                                   Date = a.Date,
                                   VoucherNo = a.Voucher,
                                   MCFrom = b.MCName,
                                   MCTo = c.MCName,
                                   GrandTotal = a.TotalAmount,
                                   Remark = a.Remarks,
                                   item = a.Item,
                                   CreatedBy = a.CreatedBy,
                                   CreatedEmp = d.FirstName,
                                   CreatedDate = a.CreatedDate,
                                   chkCode = PrintCode,
                                   TimeOut = TOut,
                                   ProCheck = ProjectCheck,
                                   a.Ref1,
                                   a.Ref2,
                                   a.Ref3,
                                   a.Ref4,
                                   a.Ref5,
                                   ComHeadCheck = ComHeadCheck
                               }).FirstOrDefault();

                var item = (from b in db.DummyStkTrsItem2
                            join c in db.Items on b.Item equals c.ItemID

                            join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                            from d in scaffold.DefaultIfEmpty()
                            join e in db.ItemUnits on b.Unit equals e.ItemUnitID into punit
                            from e in punit.DefaultIfEmpty()

                            join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                            from g in bundle.DefaultIfEmpty()
                            let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new { im.FileName, im.Status, im.ItemImageID }).ToList()
                            where b.StockTransferId == id /*&& b.Note != "-:{Bundle_Item}"*/
                            select new
                            {
                                Price = b.Price,
                                ItemQuantity = b.Quantity,
                                ItemTotalAmount = b.Amount,
                                ItemCode = c.ItemCode,
                                ItemName = c.ItemName,
                                ItemUnit = e.ItemUnitName,
                                PartNumber = c.PartNumber,
                                PNoStatus = PNoStatus,
                                CBM = d.CBM,
                                Weight = d.Weight,
                                img = img,
                                c.ItemDescription,
                                KeepStock = c.KeepStock,
                                bundle = (from ay in db.BundleItems
                                          join az in db.DummyStkTrsItem2 on ay.ItemBundle equals az.Id
                                          //join ab in db.QuotationItems on ay.ItemId equals ab.Item into quot
                                          //from ab in quot.DefaultIfEmpty()
                                          join bb in db.Items on ay.ItemId equals bb.ItemID
                                          join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                          from dd in scaffold.DefaultIfEmpty()
                                          join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
                                          from eb in bpunit.DefaultIfEmpty()
                                          let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new { bim.FileName, bim.Status, bim.ItemImageID }).ToList()
                                          where az.Item == b.Item
                                          select new
                                          {
                                              Id = bb.ItemID,
                                              ItemUnitPrice = ay.ItemUnitPrice,
                                              ItemQuantity = (ay.ItemQuantity * b.Quantity),
                                              ItemSubTotal = (ay.ItemQuantity * b.Quantity * ay.ItemUnitPrice),
                                              ItemNote = "",
                                              ItemTax = ay.ItemTax,
                                              ItemTaxAmount = ((ay.ItemQuantity * b.Quantity * ay.ItemUnitPrice) * ay.ItemTax / 100),
                                              ItemTotalAmount = ay.ItemTotalAmount,
                                              ItemCode = bb.ItemCode,
                                              ItemName = bb.ItemName,
                                              ItemUnit = eb.ItemUnitName,
                                              PartNumber = bb.PartNumber,
                                              PNoStatus = PNoStatus,
                                              CBM = dd.CBM,
                                              Weight = dd.Weight,
                                              img = bimg,
                                              KeepStock = bb.KeepStock,
                                              Item = ay.ItemId,
                                              ItemDiscount = 0,
                                              ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                              bb.ItemUnitID,
                                              bb.SubUnitId,
                                              bb.ItemArabic,
                                              bb.ItemDescription
                                          }).ToList()

                            }).ToList();

                var billsundry = db.StockTransferBSundrys.Where(n => n.StockTransferId == id).Select(b => new
                {
                    AmountType = b.AmountType,
                    BsAmount = b.BsAmount,
                    BsType = b.BsType,
                    BsValue = b.BsValue != null ? b.BsValue : 0,
                    BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
                }).ToList();

                var cdetails = db.companys
                .Select(s => new
                {
                    CName = s.CPName,
                    CAddress = s.CPAddress,
                    CEmail = s.CPEmail,

                }).FirstOrDefault();

                var Data = new Dictionary<string, object>();
                Data.Add("summary", summary);
                Data.Add("item", item);
                Data.Add("billsundry", billsundry);
                Data.Add("cdetails", cdetails);
                return Data;

            }
            else
            {


                var summary = (from a in db.StockTransfers
                               join b in db.MCs on a.MCFrom equals b.MCId
                               join c in db.MCs on a.MCTo equals c.MCId
                               join d in db.Employees on a.CreatedBy equals d.UserId into yy
                               from d in yy.DefaultIfEmpty()
                               where a.Id == id
                               select new
                               {
                                   Date = a.Date,
                                   VoucherNo = a.Voucher,
                                   MCFrom = b.MCName,
                                   MCTo = c.MCName,
                                   GrandTotal = a.TotalAmount,
                                   Remark = a.Remarks,
                                   item = a.Item,
                                   CreatedBy = a.CreatedBy,
                                   CreatedEmp = d.FirstName,
                                   CreatedDate = a.CreatedDate,
                                   chkCode = PrintCode,
                                   TimeOut = TOut,
                                   ProCheck = ProjectCheck,
                                   a.Ref1,
                                   a.Ref2,
                                   a.Ref3,
                                   a.Ref4,
                                   a.Ref5,
                                   ComHeadCheck = ComHeadCheck
                               }).FirstOrDefault();

                var item = (from b in db.StockTransferItems
                            join c in db.Items on b.Item equals c.ItemID

                            join d in db.Scaffolds on c.ItemID equals d.Item into scaffold
                            from d in scaffold.DefaultIfEmpty()
                            join e in db.ItemUnits on b.Unit equals e.ItemUnitID into punit
                            from e in punit.DefaultIfEmpty()

                            join g in db.ItemBundles on c.ItemID equals g.mainItem into bundle
                            from g in bundle.DefaultIfEmpty()
                            let img = db.ItemImages.Where(im => im.ItemID == b.Item).Select(im => new { im.FileName, im.Status, im.ItemImageID }).ToList()
                            where b.StockTransferId == id /*&& b.Note != "-:{Bundle_Item}"*/
                            select new
                            {
                                Price = b.Price,
                                ItemQuantity = b.Quantity,
                                ItemTotalAmount = b.Amount,
                                ItemCode = c.ItemCode,
                                ItemName = c.ItemName,
                                ItemUnit = e.ItemUnitName,
                                PartNumber = c.PartNumber,
                                PNoStatus = PNoStatus,
                                CBM = d.CBM,
                                Weight = d.Weight,
                                img = img,
                                c.ItemDescription,
                                KeepStock = c.KeepStock,
                                bundle = (from ay in db.BundleItems
                                          join az in db.StockTransferItems on ay.ItemBundle equals az.Id
                                          //join ab in db.QuotationItems on ay.ItemId equals ab.Item into quot
                                          //from ab in quot.DefaultIfEmpty()
                                          join bb in db.Items on ay.ItemId equals bb.ItemID
                                          join dd in db.Scaffolds on bb.ItemID equals dd.Item into scaffold
                                          from dd in scaffold.DefaultIfEmpty()
                                          join eb in db.ItemUnits on ay.ItemUnit equals eb.ItemUnitID into bpunit
                                          from eb in bpunit.DefaultIfEmpty()
                                          let bimg = db.ItemImages.Where(bim => bim.ItemID == ay.ItemId).Select(bim => new { bim.FileName, bim.Status, bim.ItemImageID }).ToList()
                                          where az.Item == b.Item
                                          select new
                                          {
                                              Id = bb.ItemID,
                                              ItemUnitPrice = ay.ItemUnitPrice,
                                              ItemQuantity = (ay.ItemQuantity * b.Quantity),
                                              ItemSubTotal = (ay.ItemQuantity * b.Quantity * ay.ItemUnitPrice),
                                              ItemNote = "",
                                              ItemTax = ay.ItemTax,
                                              ItemTaxAmount = ((ay.ItemQuantity * b.Quantity * ay.ItemUnitPrice) * ay.ItemTax / 100),
                                              ItemTotalAmount = ay.ItemTotalAmount,
                                              ItemCode = bb.ItemCode,
                                              ItemName = bb.ItemName,
                                              ItemUnit = eb.ItemUnitName,
                                              PartNumber = bb.PartNumber,
                                              PNoStatus = PNoStatus,
                                              CBM = dd.CBM,
                                              Weight = dd.Weight,
                                              img = bimg,
                                              KeepStock = bb.KeepStock,
                                              Item = ay.ItemId,
                                              ItemDiscount = 0,
                                              ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                              bb.ItemUnitID,
                                              bb.SubUnitId,
                                              bb.ItemArabic,
                                              bb.ItemDescription
                                          }).ToList()

                            }).ToList();

                var billsundry = db.StockTransferBSundrys.Where(n => n.StockTransferId == id).Select(b => new
                {
                    AmountType = b.AmountType,
                    BsAmount = b.BsAmount,
                    BsType = b.BsType,
                    BsValue = b.BsValue != null ? b.BsValue : 0,
                    BillSundry = db.BillSundrys.Where(a => a.BillSundryId == b.BillSundry).Select(a => a.BSName).FirstOrDefault()
                }).ToList();

                var cdetails = db.companys
                .Select(s => new
                {
                    CName = s.CPName,
                    CAddress = s.CPAddress,
                    CEmail = s.CPEmail,

                }).FirstOrDefault();

                var Data = new Dictionary<string, object>();
                Data.Add("summary", summary);
                Data.Add("item", item);
                Data.Add("billsundry", billsundry);
                Data.Add("cdetails", cdetails);
                return Data;
            }
        }
        #endregion
        public Boolean SendToCompanyMailCustom(CompanyEmailFormat CEmail, long CustId)
        {
            var email = db.companys.Select(a => a.CPEmail).FirstOrDefault();
            if (email != null && MailIsValid(email) == true)
            {
                SendMail sm = new SendMail();
                MailMessage message = new MailMessage();
                StringBuilder sb = new StringBuilder();
                SendMail mailsend = new SendMail();

                string ToMail = email;
                string CcMail = "";

                message.Subject = CEmail.BillNo;
                message.Body = CEmail.Subject;

                message.IsBodyHtml = true;
                DateTime datenow = DateTime.Now;
                var creditperiod = db.Customers.Find(CustId).CreditPeriod;

                decimal ret = 0;


                var ConD = (from a in db.Customers
                                //join b in db.SalesEntrys on a.CustomerID equals b.Customer into primary
                                //from b in primary.DefaultIfEmpty()
                            join c in db.Employees on a.SalesPerson equals c.EmployeeId into secondary
                            from c in secondary.DefaultIfEmpty()
                            join d in db.CustomerTyps on a.CustomerType equals d.TypeId into temp
                            from d in temp.DefaultIfEmpty()
                            where a.CustomerID == CustId

                            let Credit = (from d in db.AccountsTransactions where d.Account == a.Accounts && d.Status == null select d.Credit).AsEnumerable().DefaultIfEmpty(0).Sum()
                            let Debit = (from b in db.AccountsTransactions where b.Account == a.Accounts && b.Status == null select b.Debit).AsEnumerable().DefaultIfEmpty(0).Sum()

                            select new
                            {
                                a.Accounts,
                                CusType = a.CustomerType == null ? "" : d.Type,
                                CreditLimit = a.CreditLimit == null ? 0 : a.CreditLimit,
                                currentbalance = ((Debit > Credit) ? ((Debit - Credit)) : 0),
                                acbalance = (Debit > Credit) ? ((Debit - Credit) + " Dr.") : ((Credit - Debit) + " Cr."),
                                mob = (from co in db.Contacts
                                       join rrr in db.ContactRelation on co.ContactID equals rrr.ContactID
                                       join con in db.Country on co.CountryID equals con.CountryID into conn
                                       from con in conn.DefaultIfEmpty()
                                       where (rrr.RelationID == CustId && rrr.RelationType == 0)
                                       select new MobileViewModel
                                       {
                                           Num = "+" + con.CountryCode + co.Mobile,
                                           Name = co.FirstName + "  " + co.LastName,
                                           emails = co.EmailId,
                                       }).ToList(),
                                pdc = (decimal?)(from b in db.AccountsTransactions
                                                 join c in db.Customers on b.Account equals c.Accounts

                                                 where b.Status != null && c.CustomerID == CustId
                                                 group new { b.Debit, b.Credit, b.Account } by new { b.Account } into g
                                                 select new
                                                 {

                                                     GrandTotal = g.Sum(o => o.Credit)


                                                 }).Sum(x => x.GrandTotal) ?? 0,
                                //pdc = (decimal?)(
                                //       from aa in db.PDCs
                                //       join r in db.Receipts on new { g1 = aa.Reference, g2 = aa.PDCType } equals
                                //       new { g1 = r.ReceiptId, g2 = "Receipt" }

                                //       where r.PayFrom == a.Accounts &&
                                //       aa.RegStatus == choice.No &&
                                //       r.MOPayment == ModeOfPayment.PDC
                                //       select new
                                //       {
                                //           r.GrandTotal
                                //       }

                                //        ).Sum(x => x.GrandTotal) ?? 0,

                                ddlEmployee = c.EmployeeId == null ? 0 : c.EmployeeId,

                                billage = (from a in db.SalesEntrys
                                           join b in db.SEPayments on a.SalesEntryId equals b.SalesEntry into pay
                                           from b in pay.DefaultIfEmpty()
                                           join c in db.Customers on a.Customer equals c.CustomerID into rec
                                           from c in rec.DefaultIfEmpty()
                                           join d in db.SalesReturns on a.SalesEntryId equals d.SalesEntryId into slret
                                           from d in slret.DefaultIfEmpty()

                                           where
                                             (a.Customer == CustId)




                                           select new
                                           {
                                               id = a.SalesEntryId,
                                               Date = a.SEDate,
                                               Invoice = a.BillNo,
                                               c.CreditPeriod,
                                               Amount = (((b.SEBillAmount == null) ? 0 : b.SEBillAmount) - ((b.SEPaidAmount == null) ? 0 : b.SEPaidAmount) - ((d.SRGrandTotal == null) ? 0 : d.SRGrandTotal)),
                                               total = ((b.SEBillAmount == null) ? 0 : b.SEBillAmount),
                                               paid = ((b.SEPaidAmount == null) ? 0 : b.SEPaidAmount),

                                               Days = EF.Functions.DateDiffDay(a.SEDate, datenow),
                                               Ctyp = "Cus"
                                           }).Where(o => o.Amount > 3).OrderByDescending(o => o.Days).Distinct().ToList(),
                                includepdc = a.includepdc,

                            }).ToList();
                mailsend.sendMail(ToMail, CcMail, message);
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
        #region Sendmail to Company
        public Boolean SendToCompanyMail(CompanyEmailFormat CEmail)
        {
            var email = db.companys.Select(a => a.CPEmail).FirstOrDefault();
            if (email != null && MailIsValid(email) == true)
            {
                SendMail sm = new SendMail();
                MailMessage message = new MailMessage();
                StringBuilder sb = new StringBuilder();
                SendMail mailsend = new SendMail();

                string ToMail = email;
                string CcMail = "";

                message.Subject = CEmail.BillNo;
                message.Body = CEmail.Subject;

                message.IsBodyHtml = true;

                mailsend.sendMail(ToMail, CcMail, message);
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool MailIsValid(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        #endregion

        #region company details
        public Dictionary<string, object> CompanyInfo()
        {
            var cdetails = db.companys
                            .Select(s => new
                            {
                                CName = s.CPName,
                                CAddress = s.CPAddress,
                                CEmail = s.CPEmail,
                                CTaxRegNo = s.TRN,
                                CPhone = s.CPPhone,
                                s.CPMobile,
                                s.CPFax,
                                CLogo = s.CPLogo,
                                header = db.CompanyHeaders.FirstOrDefault()
                            }).FirstOrDefault();

            var Data = new Dictionary<string, object>();
            Data.Add("company", cdetails);
            return Data;
        }
        #endregion

        #region gen-pdf-common
        public StringBuilder generatepdf(long id, pdfSummaryViewModel pdfSummary, List<pdfItemViewModel> pdfItem, List<pdfBillSundryViewModel> billsundry, string type)
        {

            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

            var conv = db.ConvertTransactionss.Any(u => u.To == id);
            List<ConvertTransactionsViewModel> ConvExt = new List<ConvertTransactionsViewModel>();
            //var ConvModel=;
            if (conv)
            {
                List<string> ExList = new List<string>();
                List<ConvertTransactions> ExtList = new List<ConvertTransactions>();
                ExtList = ExtNum(id, ExtList);
                var Extended = ExtList.Select(z => z.To).ToList();
                Int32 count = 0;
                ConvExt = (from a in db.ConvertTransactionss
                           join b in db.SalesEntrys on a.To equals b.SalesEntryId into primary
                           from b in primary.DefaultIfEmpty()
                           where Extended.Contains(a.To)
                           select new ConvertTransactionsViewModel
                           {
                               ConvertFrom = (a.ConvertFrom == "SaleExtend") ? "Sale" : (a.ConvertFrom == "Quote") ? "Quotation" : (a.ConvertFrom == "DVNote") ? "Delivery Note" : a.ConvertFrom,
                               Id = a.Id,
                               BillNo = b.BillNo,
                               CreatedDate = a.CreatedDate,
                               From = a.From
                           }).OrderBy(b => b.CreatedDate).ToList();
            }

            var st = ConvExt.Find(c => c.BillNo == pdfSummary.BillNo);
            ConvExt.Remove(st);
            var ConvExtList = ConvExt;

            var cdetails = db.companys
            .Select(s => new
            {
                CName = s.CPName,
                CAddress = s.CPAddress,
                CEmail = s.CPEmail,
                CTaxRegNo = s.TRN,
                CPhone = s.CPPhone,
                s.CPMobile,
                CLogo = s.CPLogo,

            }).FirstOrDefault();

            int SI = 1;
            string address = "";
            if (pdfSummary.Address != null)
            {
                address += pdfSummary.Address;
            }
            if (pdfSummary.City != null)
            {
                address += pdfSummary.Address != null ? "<br />" + pdfSummary.City : pdfSummary.City;
            }
            else if (pdfSummary.State != null)
            {
                address += address != "" ? "<br />" + pdfSummary.State : pdfSummary.State;
            }
            else if (pdfSummary.Country != null)
            {
                address += address != "" ? "<br />" + pdfSummary.Country : pdfSummary.Country;
            }
            else if (pdfSummary.Zip != null)
            {
                address += address != "" ? "<br />" + pdfSummary.Zip : pdfSummary.Zip;
            }
            address += " <br/> Phone : ";
            if (pdfSummary.Mobile != null)
            {
                address += pdfSummary.Mobile;
                if (pdfSummary.Phone != null)
                {
                    address += ", " + pdfSummary.Phone;
                }
            }
            else
            {
                if (pdfSummary.Phone != null)
                {
                    address += pdfSummary.Phone;
                }
            }
            if (pdfSummary.Email != null)
            {
                address += "<br/> Email : " + pdfSummary.Email;
            }
            if (pdfSummary.TRN != "")
            {
                address += "<br/><b>TRN</b> : " + pdfSummary.TRN;
            }


            var def = Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
            def = def == 0 ? 1 : def;
            var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();
            string layName = (layout == null || layout.Name == "undefined") ? "Default" : layout.Name;


            //InvoiceLayout layout = db.InvoiceLayouts.Find(1);
            InvoiceLayoutViewModel Vmodel = new InvoiceLayoutViewModel();
            //Vmodel.Id = layout.Id;
            //Vmodel.Name = layout.Name;
            //Vmodel.Status = layout.Status;
            var section = type != "" ? type : "Sale";
            Vmodel.InvoiceField = db.InvoiceFields.Where(a => a.Section == section || a.Section == null).ToList();


            var title = Vmodel.InvoiceField.Where(a => a.Type == "Title").FirstOrDefault();

            var Customer = (type == "LPO" || type == "Purchase") ? Vmodel.InvoiceField.Where(a => a.Type == "Supplier").FirstOrDefault() : Vmodel.InvoiceField.Where(a => a.Type == "Customer").FirstOrDefault();

            var Cust_Name = Vmodel.InvoiceField.Where(a => a.Type == "Cust_Name").FirstOrDefault();
            var Cust_Address = Vmodel.InvoiceField.Where(a => a.Type == "Cust_Address").FirstOrDefault();
            var Cust_City = Vmodel.InvoiceField.Where(a => a.Type == "Cust_City").FirstOrDefault();
            var Cust_State = Vmodel.InvoiceField.Where(a => a.Type == "Cust_State").FirstOrDefault();
            var Cust_Country = Vmodel.InvoiceField.Where(a => a.Type == "Cust_Country").FirstOrDefault();
            var Cust_Zip = Vmodel.InvoiceField.Where(a => a.Type == "Cust_Zip").FirstOrDefault();
            var Cust_Mobile = Vmodel.InvoiceField.Where(a => a.Type == "Cust_Mobile").FirstOrDefault();
            var Cust_Phone = Vmodel.InvoiceField.Where(a => a.Type == "Cust_Phone").FirstOrDefault();
            var Cust_Email = Vmodel.InvoiceField.Where(a => a.Type == "Cust_Email").FirstOrDefault();
            var Cust_TRN = Vmodel.InvoiceField.Where(a => a.Type == "Cust_TRN").FirstOrDefault();
            var Cust_Fax = Vmodel.InvoiceField.Where(a => a.Type == "Cust_Fax").FirstOrDefault();
            var Cust_Credit_Period = Vmodel.InvoiceField.Where(a => a.Type == "Cust_Credit_Period").FirstOrDefault();

            var Bill_Amount = Vmodel.InvoiceField.Where(a => a.Type == "Bill_Amount").FirstOrDefault();
            var Bill_Tax = Vmodel.InvoiceField.Where(a => a.Type == "Bill_Tax").FirstOrDefault();
            var Bill_Discount = Vmodel.InvoiceField.Where(a => a.Type == "Bill_Discount").FirstOrDefault();
            var Bill_roundN = Vmodel.InvoiceField.Where(a => a.Type == "Bill_roundN").FirstOrDefault();
            var Bill_roundP = Vmodel.InvoiceField.Where(a => a.Type == "Bill_roundP").FirstOrDefault();
            var Bill_Total = Vmodel.InvoiceField.Where(a => a.Type == "Bill_Total").FirstOrDefault();
            var Bill_TotalWord = Vmodel.InvoiceField.Where(a => a.Type == "Bill_TotalWord").FirstOrDefault();

            var InvoiceNo = Vmodel.InvoiceField.Where(a => a.Type == "InvoiceNo").FirstOrDefault();
            var Date = Vmodel.InvoiceField.Where(a => a.Type == "Date").FirstOrDefault();
            var PO_No = Vmodel.InvoiceField.Where(a => a.Type == "PO_No").FirstOrDefault();
            var SalesExecutive = Vmodel.InvoiceField.Where(a => a.Type == "SalesExecutive").FirstOrDefault();
            var PaymentType = Vmodel.InvoiceField.Where(a => a.Type == "PaymentType").FirstOrDefault();
            var DeliveryNote = Vmodel.InvoiceField.Where(a => a.Type == "DeliveryNote").FirstOrDefault();


            var Terms = Vmodel.InvoiceField.Where(a => a.Type == "Terms").FirstOrDefault();
            var Foot_1 = Vmodel.InvoiceField.Where(a => a.Type == "Foot_1").FirstOrDefault();
            var Foot_2 = Vmodel.InvoiceField.Where(a => a.Type == "Foot_2").FirstOrDefault();

            var MakeIn = db.EnableSettings.Where(a => a.EnableType == "MakeInTrans").FirstOrDefault();
            var Make = MakeIn != null ? MakeIn.Status : Status.inactive;
            if (type == "Stock Transfer")
            {
                InPrintItemCode = 0;
            }
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    if (layName == "Jewellery")
                    {
                        sb.Append("<table width='100%' style='border: 0px;text-align:center;'><tr><td><b>" + title.Value + " " + title.Lang + "</b></td></tr></table>");
                        string partyDetails = "<table style='border: 0px;' width='100%'>" +
                            "<tr style='border-top:0px'>" +
                            "<td width='50%' style='padding:0px 10px 0px 0px; border-right:0px'>" +
                            "<table class='table-nob jewel-cus' style='border:1px solid #000;width: 100%;'>" +
                            "<tr><th><i><b>" + Customer.Value + "</b></i><i><b>" + Customer.Lang + " </b></i></th>" +
                            "<td style='text-align:left;'>: " + pdfSummary.PartyName + "</td>" +
                            "</tr><tr><td colspan='3'>" + address + "" +
                            "</td></tr>" +
                            //<tr><td colspan ='3'>" +
                            //"TRN:" + pdfSummary.TRN + "</td></tr>" +
                            "</table></td>" +
                            "<td style='padding: 0px;'><table id='Cust_de' class='table-nob jewel-inv' style='border: 1px solid #000;width: 100%;'> " +
                                        "<tr><th> " + InvoiceNo.Value + " " + InvoiceNo.Lang + " </th>" +
                                        "<td>:" + pdfSummary.BillNo + "</td>" +
                                        "</tr><tr><th>" + Date.Value + " " + Date.Lang + "</th>" +
                                        "<td>:" + pdfSummary.Date.ToString("dd-MM-yyyy") + "</td></tr>";

                        if (PO_No != null && PO_No.Status == Status.active)
                        {
                            partyDetails += "<tr><th> " + PO_No.Value + "</th><td>" + pdfSummary.PONo + "</td></tr>";
                        }
                        if (SalesExecutive.Status == Status.active)
                        {
                            partyDetails += "<tr><th> " + SalesExecutive.Value + " " + SalesExecutive.Lang + "</th><td>" + pdfSummary.Cashier + "</td></tr>";
                        }

                        if (DeliveryNote.Status == Status.active)
                        {
                            partyDetails += "<tr><th> " + DeliveryNote.Value + "</th><td>" + pdfSummary.ConvertNo + "</td></tr>";
                        }
                        if (PaymentType.Status == Status.active)
                        {
                            partyDetails += "<tr><th> " + PaymentType.Value + "</th><td>" + pdfSummary.paytype + "</td></tr>";
                        }
                        if (Cust_Credit_Period.Status == Status.active)
                        {
                            partyDetails += "<tr><th> " + Cust_Credit_Period.Value + "</th><td>" + pdfSummary.CreditPeriod + "</td></tr>";
                        }
                        if (pdfSummary.PrjNameCode != null)
                        {
                            partyDetails += "<tr><th>Project</th><td>" + pdfSummary.PrjNameCode + "</td></tr>";
                        }
                        if (pdfSummary.AgainstInvoice != null)
                        {
                            partyDetails += "<tr><th>Against Invoice</th><td>" + pdfSummary.AgainstInvoice + "</td></tr>";
                        }
                        partyDetails += "</table></td></tr></table>";
                        sb.Append(partyDetails);

                    }
                    else if (layName == "Scaffold")
                    {
                        sb.Append("<table width='100%' style='border: 0px;text-align:center;'><tr><td><b>" + title.Value + " " + title.Lang + "</b></td></tr></table>");

                        string partyDetails = "<table style='border: 0px;' width='100%'><tr> " +
                            "<td width= '59%' style='padding: 2px 8px; border: 1px solid #000'>" +
                            "<table width= '100%' style='border: 0px;'> " +

                        "<tr><th style='width:28% !important;'><i><b>" + Customer.Value + "</b></i><i><b>" + Customer.Lang + " </b></i></th><td>:</td><td width='69%'>" + pdfSummary.PartyName + "</td></tr>" +
                        "<tr><th style='width:28% !important;'>ADDRESS</th><td>:</td><td>" + pdfSummary.Address + "</td></tr>" +
                        "<tr><th style='width:28% !important;'>MOBILE NO</th><td>:</td><td>" + pdfSummary.Mobile + "</td></tr>";

                        if (pdfSummary.Email != null)
                        {
                            partyDetails += "<tr><th style='width:28% !important;'>EMAIL</th><td>:</td><td>" + pdfSummary.Email + "</td></tr>";
                        }
                        partyDetails += "<tr><th style='width:28% !important;'>TRN</th><td>:</td><td>" + pdfSummary.TRN + "</td></tr>";
                        partyDetails += "<tr><th style='width:28% !important;'>CONTACT PERSON</th><td>:</td><td>" + pdfSummary.ContactPerson + "</td></tr>";
                        partyDetails += "</table></td>";

                        partyDetails += "<td width='1%' style='border:0px;padding: 0px;'></td>" +
                            "<td width='40%' style='padding: 0px; border: 0px;'>" +
                            "<table style='border:0px;border-collapse:collapse; width:100%; border-right:.1px solid;'>" +
                            "<tr><td style='border:1px solid #000 !important;padding: 4px;'>" + InvoiceNo.Value + "</td> " +
                            "<td colspan='2' style='border:1px solid #000 !important;padding: 4px;padding-bottom: 0px;padding-top: 0px;'>" + pdfSummary.BillNo + "</td>" +
                            "<td style='border: 1px solid #000 !important;padding: 4px;'>" + Date.Value + "</td>" +
                            "<td style='border: 1px solid #000 !important;padding: 4px;'>" + pdfSummary.Date.ToString("dd-MM-yyyy") + "</td>" +
                            "</tr>";

                        if (pdfSummary.Cashier != null && SalesExecutive.Status == Status.active)
                        {
                            partyDetails += "<tr><td colspan='2' style='border: 1px solid #000 !important;padding: 4px;'>" + SalesExecutive.Value + "</td>" +
                                "<td colspan='3' style='border: 1px solid #000 !important;padding: 4px;'>" + pdfSummary.Cashier + "</td></tr>";
                        }
                        if (pdfSummary.PONo != null && (PO_No != null && PO_No.Status == Status.active))
                        {
                            partyDetails += "<tr><td colspan='2' style='border: 1px solid #000 !important;padding: 4px;'>" + PO_No.Value + "</td>" +
                                "<td colspan='3' style='border: 1px solid #000 !important;padding: 4px;'>" + pdfSummary.PONo + "</td></tr>";
                        }
                        if (pdfSummary.ConvertNo != null && DeliveryNote.Status == Status.active)
                        {
                            partyDetails += "<tr><td colspan='2' style='border: 1px solid #000 !important;padding: 4px;'>" + DeliveryNote.Value + "</td>" +
                                "<td colspan='3' style='border: 1px solid #000 !important;padding: 4px;'>" + pdfSummary.ConvertNo + "</td></tr>";
                        }

                        partyDetails += "<tr><td colspan='2' style='border: 1px solid #000 !important;padding: 4px;'>PAYMENT TERMS</td>" +
                              "<td colspan='3' style='border: 1px solid #000 !important;padding: 4px;'>" + pdfSummary.PaymentTerms + "</td></tr>";

                        if (pdfSummary.PrjNameCode != "")
                        {
                            partyDetails += "<tr><td colspan='2' style='border: 1px solid #000 !important;padding: 4px;'>Project</td>" +
                              "<td colspan='3' style='border: 1px solid #000 !important;padding: 4px;'>" + pdfSummary.PrjNameCode + "</td></tr>";
                        }
                        if (pdfSummary.AgainstInvoice != null)
                        {
                            partyDetails += "<tr><th>Against Invoice</th>" +
                              "<td>" + pdfSummary.AgainstInvoice + "</td></tr>";
                        }

                        partyDetails += "</table>";
                        partyDetails += "</td></tr></table>";
                        sb.Append(partyDetails);
                    }
                    else
                    {

                        if (type != "Stock Transfer")
                        {
                            sb.Append("<table width='100%' style='border: 0px;color:blue;text-align:center;'><tr><td><b><u>" + title.Value + " " + title.Lang + "</u></b></td></tr></table>");
                            string partyDetails = "<table style='border:.1px #ccc;' width='100%'><tr style='border-top:.1px #ccc; '> " +
                                "<td width='46.4%'> " +
                                "<table  style='border: 0px; width: 100 %;'><tr><th><i><b>" + Customer.Value + " " + Customer.Lang + "</b></i></th></tr><tr><td>" + pdfSummary.PartyName + "</td></tr><tr><td style='font-size:14px;font-weight:normal;'>" + address + "</td></tr></table></td><td width='50%' style='border-left: 1px solid #ccc;'>" +
                                "<table  style='border: 0px; width: 100 %;'><tr><th>" + InvoiceNo.Value + " " + InvoiceNo.Lang + "</th><td style='font-size:14px;font-weight:normal;'>: " + pdfSummary.BillNo + "</td></tr><tr><th>" + Date.Value + " " + Date.Lang + "</th><td style='font-size:14px;font-weight:normal;'>: " + pdfSummary.Date.ToString("dd-MM-yyyy") + "</td></tr>";
                            //check the salesexecutive(employee)
                            if (pdfSummary.Cashier != "  " && pdfSummary.Cashier != null && pdfSummary.Cashier != "")
                            {
                                partyDetails += "<tr><th>" + SalesExecutive.Value + " " + SalesExecutive.Lang + "</th><td style='font-size:14px;font-weight:normal;'>: " + pdfSummary.Cashier + "</td></tr>";
                                //checking the email id of corresponding employee
                                if (pdfSummary.empemail != null)
                                {
                                    partyDetails += "<tr><th >" + "e-Mail" + " </ th ><td style='font-size:14px;font-weight:normal;'>: " + pdfSummary.empemail + " </ td ></ tr > ";
                                }
                                //checking the phone no
                                if (pdfSummary.ContactNo != null)
                                {
                                    partyDetails += "<tr><th >" + "Phone" + " </ th ><td style='font-size:14px;font-weight:normal;'>: " + pdfSummary.ContactNo + " </td ></ tr > ";
                                }

                            }
                            if (pdfSummary.MCTo != null || pdfSummary.MCTo != "")
                            {
                                partyDetails += "<tr><th >" + "MC" + " </ th ><td style='font-size:14px;font-weight:normal;'>: " + pdfSummary.MCTo + " </td ></ tr > ";

                            }

                            if (pdfSummary.AgainstInvoice != null)
                            {
                                partyDetails += "<tr><th>Against Invoice</th><td style='font-size:14px;font-weight:normal;'>: " + pdfSummary.AgainstInvoice + "</td></tr>";
                            }
                            partyDetails += "</table></td></tr></table>";

                            sb.Append(partyDetails);
                        }
                        else
                        {
                            //string partyDetails = "<table style='border:.1px #ccc;' width='100%'><tr style='border-top:.1px #ccc; '>" +
                            //                      "<td width='46.4%'> " +
                            //                      "<table  style='border: 0px; width: 100 %;'><tr><th><i><b>Voucher No</b></i></th></tr><tr><td>" + pdfSummary.BillNo + "</td></tr></table></td></tr></table>";
                            string partyDetails = "<table style='border:.1px #ccc;' width='100%'><tr style='border-top:.1px #ccc;'><td width='46.4%'><table  style='border: 0px; width: 50 %;'><tr style=''><th style='font-size:13px;padding:5px!important;'>Voucher No</th><td style='font-size:14px;font-weight:normal;padding:5px!important;'>: " + pdfSummary.BillNo + "</td></tr><tr style=''><th style='font-size:13px;padding:5px!important;'>Date</th><td style='font-size:14px;font-weight:normal;padding:5px!important;'>: " + pdfSummary.Date.ToString("dd-MM-yyyy") + "</td></tr><tr style=''><th style='font-size:13px;padding:5px!important;'>Created By</th><td style='font-size:14px;font-weight:normal;padding:5px!important;'>: " + pdfSummary.PartyName + "</td></tr></table></td><td></td></tr></table>";
                            //string partyDetails = "<table style='border:.1px #ccc;' width='100%'><tr style='border-top:.1px #ccc; '><td width='46.4%'><table  style='border: 0px; width: 100 %;'><tr><th>Voucher No</th><td style='font-size:14px;font-weight:normal;'>: " + pdfSummary.BillNo + "</td></tr><tr><th>Created By</th><td style='font-size:14px;font-weight:normal;'>: " + pdfSummary.PartyName + "</td></tr></table></td><td><table  style='border: 0px; width: 100 %;'><tr><th>Date</th><td style='font-size:14px;font-weight:normal;'>: " + pdfSummary.Date.ToString("dd-MM-yyyy") + "</td></tr></table></td></tr></table>";

                            //partyDetails += "</table>";
                            sb.Append(partyDetails);
                        }
                    }


                    //sb.Append("<div style='min-height:558px;>");
                    sb.Append("<table width='100%' style='border-collapse:collapse;font-size:12px;border: .1px solid #ccc;repeat-header:yes;'>");
                    sb.Append("<thead>");

                    if (layName == "Scaffold")
                    {
                        var SaleType = pdfSummary.SaleType;
                        //console.log("Sale type: " + SaleType);
                        var Subject = "Subject : ";
                        if (SaleType == SaleType.Sale)
                        {
                            Subject += "<b class='text-green' style='font-size: large;font-weight: 600;'>SALE</b>";
                        }
                        else if (SaleType == SaleType.Hire)
                        {
                            var From = pdfSummary.FromDate;
                            var To = pdfSummary.ToDate;
                            //var diff = moment.preciseDiff(e.summary.FromDate, e.summary.ToDate);
                            decimal diff = 0;
                            var startDate = string.Format("{0:dd.MM.yyyy HH:mm:ss}", pdfSummary.FromDate);
                            var endDate = string.Format("{0:dd.MM.yyyy HH:mm:ss}", pdfSummary.ToDate);
                            var HireType = pdfSummary.HireType;
                            var Htype = (HireType == "Weekly") ? "week" : (HireType == "Monthly") ? "month" : "days";
                            var HtypeV = (HireType == "Weekly") ? "Week" : (HireType == "Monthly") ? "Month" : "Days";
                            if (Htype == "days")
                            {
                                DateTime dt1 = Convert.ToDateTime(pdfSummary.ToDate);
                                DateTime dt2 = Convert.ToDateTime(pdfSummary.FromDate);
                                TimeSpan tspan = dt2.Subtract(dt1);
                                diff = tspan.Days;
                            }
                            else if (Htype == "week")
                            {
                                diff = tocountweek(endDate, startDate);
                            }
                            else
                            {
                                diff = tocountmonth(endDate, startDate);
                            }

                            //console.log(diff);
                            Subject += "<b>HIRE OF ALUMINIUM SCAFFOLDING FOR " + diff + " " + HtypeV + "(STARTING FROM " + string.Format("{0:dd-MM-yyyy}", pdfSummary.FromDate) + " TO " + string.Format("{0:dd-MM-yyyy}", pdfSummary.ToDate) + " ) </b>";
                        }

                        sb.Append("<tr><td colspan='3' style='padding: 5px;'>" + Subject + "</td></tr>");

                    }
                    if (layName == "Jewellery")
                    {
                        sb.Append("<tr style='font-size:13px;'>");
                        sb.Append("<th width='1%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;text-align:center;'> م.ر <br /> No</th>");
                        sb.Append("<th width='2%'  style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;text-align:center;'>ةعطقا مقر <br />Item Code</th>");
                        sb.Append("<th width='10%' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;text-align:center;'>تافصاوما Description <br />بند</th>");
                        sb.Append("<th width='2%' style='border: .5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;text-align:center;'>عطقا <br />PCS</th>");
                        sb.Append("<th width='4%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;text-align:center;'>الوزن الجمالي <br />Qty(Gms/Cts)</th>");
                        sb.Append("<th width='3%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;text-align:center;'>ةدحوا ةميق <br />Rate</th>");
                        sb.Append("<th width='3%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;text-align:center;'>المبلغ الجمالى <br />Amount</th>");
                        sb.Append("</tr>");
                    }
                    else if (layName == "Scaffold")
                    {

                        sb.Append("<tr style='font-size:13px;'>");
                        sb.Append("<th width='5%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>S/N</th>");
                        if (PartNoCheck == Status.active)
                        {
                            sb.Append("<th style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Part No</th>");
                        }

                        //sb.Append("<th style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'>Part No</th>");
                        sb.Append("<th style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' colspan='2'>Description of goods</th>");
                        //if (Make == Status.active)
                        //{
                        //    sb.Append("<th style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' width='7%'>Make</th>");
                        //}
                        sb.Append("<th style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' width='7%'>Wt(KG)</th>");
                        sb.Append("<th style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' width='5%'>CBM</th>");
                        sb.Append("<th style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' width='8%'>Qty</th>");

                        sb.Append("<th style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' width='6%'>Rate<br /> (AED)</th>");
                        sb.Append("<th style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' width='8%'>Taxable Amount</th>");
                        sb.Append("<th style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' width='6%'>VAT %</th>");
                        sb.Append("<th style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' width='7%'>VAT Amount</th>");
                        sb.Append("<th style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' width='8%'>Amount <br /> (AED)</th>");


                        sb.Append("</tr>");


                    }
                    else
                    {
                        sb.Append("<tr style='font-size:13px;'>");
                        sb.Append("<th width='5%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>S/N</th>");
                        if (PartNoCheck == Status.active)//&& type != "Stock Transfer")
                        {
                            sb.Append("<th width='5%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Part No</th>");
                        }
                        sb.Append("<th style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Item</th>");
                        if (Make == Status.active)// && type != "Stock Transfer")
                        {
                            sb.Append("<th width='5%' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>Make</th>");
                        }

                        sb.Append("<th width='4%' style='border: .5px #000000;padding: 3px;vertical-align: top;border: 1px solid #ccc;'>Unit</th>");
                        sb.Append("<th width='5%' style='border: .5px #000000;padding: 3px;vertical-align: top;border: .1px solid #ccc;'>Qty</th>");
                        sb.Append("<th width='6%' style='border:.5px #000000;padding: 3px;vertical-align: top;border: .1px solid #ccc;'>UPrice</th>");
                        sb.Append("<th width='8%' style='border:.5px #000000;padding: 3px;vertical-align: top;border: .1px solid #ccc;'>Amount</th>");
                        //if (type != "Stock Transfer")
                        //{
                        sb.Append("<th width='8%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Tax(5.00%)</th>");
                        //}
                        sb.Append("<th width='8%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Total</th>");
                        sb.Append("</tr>");
                    }

                    sb.Append("</thead>");
                    sb.Append("<tbody>");


                    string stritem = bindItem(pdfSummary, pdfItem, layName, InPrintItemCode, type);
                    sb.Append(stritem);

                    sb.Append("</tbody>");
                    sb.Append("</table>");


                    sb.Append("<table width='100%' style='border: 0px; border-spacing:0px;text-align:center;'>");

                    var str1 = "";
                    var str2 = "";
                    var str3 = "";

                    var str4 = "";
                    var count = 2;
                    var dvitem = "inactive";

                    var tc = "";
                    var termc = "";
                    if (pdfSummary.Note != null)
                    {
                        tc = pdfSummary.Note.Replace("\n", "<br />");
                    }
                    if (pdfSummary.TermsCondition != null)
                    {
                        termc = pdfSummary.TermsCondition.Replace("\n", "<br />");
                    }

                    var remark = "";
                    if (layName == "Scaffold" && pdfSummary.PaymentTerms != null)
                    {
                        remark = pdfSummary.PaymentTerms.Replace("\n", "<br />");
                    }
                    else
                    {
                        if (pdfSummary.Remarks != null)
                        {
                            remark = pdfSummary.Remarks.Replace("\n", "<br />");
                        }
                    }
                    string words = ConvertToWords(pdfSummary.GrandTotal.ToString());

                    if (layName == "Default")
                    {
                        if (pdfSummary.Discount > 0)
                        {
                            str2 += "<td style='border: .1px solid #ccc;padding: 5px;font-size:13px;'>" + Bill_Discount.Value + "</td><td style='border: .1px solid #ccc;padding: 5px;font-size:13px;' class='text-right'>" + pdfSummary.Discount + "</td></tr> ";
                            //str2 += "<tr class='border-top'><td style='border: .1px solid #ccc;padding: 5px;font-size:13px;'>" + Bill_Discount.Value + "</td><td style='border: .1px solid #ccc;padding: 5px;font-size:13px;' class='text-right'>" + pdfSummary.Discount + "</td></tr></tr> ";
                            count++;
                            //str2 += "<tr class='border-top'><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'>" + Bill_Tax.Value + "</td><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' class='text-right'>" + pdfSummary.TaxAmount + "</td></tr>";
                        }
                        else
                        {
                            //str2 += "<td>VAT<span style='direction:ltr'>(5.00%)</span> برميل </td><td class='text-right'>" + parseFloat(e.summary.TaxAmount).toFixed(2) + "</td></tr>";
                            // str2 += "<td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'>" + Bill_Tax.Value + "</td><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' class='text-right'>" + pdfSummary.TaxAmount + "</td></tr>";
                            str2 += "</tr>";
                        }
                        if (pdfSummary.SalesType == 1)
                        {
                            str2 += "<tr class='border-top'><td style='border: .1px solid #ccc;padding: 5px;font-size:13px;'>VAT</td><td style='border: .1px solid #ccc;padding: 5px;font-size:13px;' class='text-right'>" + pdfSummary.TaxAmount + "</td></tr> ";
                            count++;
                            //str2 += "<tr class='border-top'><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'>" + Bill_Tax.Value + "</td><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' class='text-right'>" + pdfSummary.TaxAmount + "</td></tr>";

                        }
                        else
                        {
                            //str2 += "<td>VAT<span style='direction:ltr'>(5.00%)</span> برميل </td><td class='text-right'>" + parseFloat(e.summary.TaxAmount).toFixed(2) + "</td></tr>";
                            // str2 += "<td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'>" + Bill_Tax.Value + "</td><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' class='text-right'>" + pdfSummary.TaxAmount + "</td></tr>";
                            str2 += "";
                        }
                        if (billsundry != null)
                        {
                            // bind bill sundry
                            str2 += bindSundry(billsundry, layName);
                            if (billsundry.Count > 0)
                            {
                                count += billsundry.Count;
                            }
                        }
                        str2 += "<tr style='vertical-align: middle;padding: 5px;'><th style='vertical-align: middle;padding: 5px;font-size:14px;'>" + Bill_Total.Value + "</th><th style='vertical-align: middle;padding: 6px;font-size:12px;background-color:dodgerblue;color:white' class='text-right'>" + pdfSummary.GrandTotal + "</th></tr><hr style = 'border:1px solid;color:rgb(58 107 196 / 0.88) !important;' ></ hr >";

                        var wordHtml = "<tr style='width:1100%;' class='border-top'><td style='vertical-align: middle;width:104%;padding: 5px;font-size:12px;' colspan='5'><strong>" + words + "</strong></td><td style='vertical-align: middle;width:14%;padding: 5px;font-size:14px;'>" + Bill_Amount.Value + "</td><td style='vertical-align: middle;padding: 5px;font-size:12px;background-color:;width:14%' class='text-right'>" + pdfSummary.SubTotal + "</td></tr>";
                        str3 = "<tr class='border-top'><td></td></tr><tr class='border-top'><td style='text-align:left;padding: 5px;font-size:12px;' colspan='5' rowspan='" + count + "'>< div style='width:500px'>" + tc + " </div></td>";

                        var remarks = "";
                        //if (remark != "")
                        //{
                        //    remarks = "<tr class='border-top'><td style='text-align:left; padding: 5px;' colspan='8'><strong>Remarks </strong><br /> <span style='font-size:12px;'>" + remark + " </span></td></tr>";
                        //}
                        if (dvitem == "active")
                        {
                            str1 = str3 + "</tr>";
                        }
                        else
                        {
                            str1 = wordHtml + str3 + str2;
                        }

                        sb.Append(str1);

                    }
                    else if (layName == "General")
                    {
                        var remarks = "";
                        if (remark != "")
                        {
                            remarks += "<tr class='border-top'><td style='text-align:left; vertical-align: middle;border: .1px solid #ccc;padding: 5px;' colspan='8'><strong>Remarks </strong><br /> " + remark + "</td></tr>";
                        }
                        if (tc != "")
                        {
                            remarks += "<tr class='border-top'><td style='text-align:left; vertical-align: middle;border: .1px solid #ccc;padding: 5px;' colspan='8'><strong><u>Note :</u></strong><br /> " + tc + "</td></tr>";
                        }
                        if (termc != "")
                        {
                            remarks += "<tr class='border-top'><td style='text-align:left; vertical-align: middle;border: .1px solid #ccc;padding: 5px;' colspan='8'><strong><u>" + Terms.Value + " :</u></strong><br /> " + termc + "</td></tr>";
                        }

                        if (dvitem == "active")
                        {
                            str1 = str3 + remarks;
                        }
                        else
                        {
                            str1 = str3 + str2 + remarks + "<tr><td></td></tr>";
                        }
                        sb.Append(str1);
                    }
                    else if (layName == "Scaffold")
                    {
                        var amttable = "<tr class='border-top'><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'><table class='table table-bordered' style='width:100%;'>" +
                                       "<tr class='border-top'><td colspan='5' style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'></td><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' class='text-right'>Total w/o VAT </td><td class='text-center' style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'>" + pdfSummary.SubTotal + "</td></tr>";

                        if (pdfSummary.Discount > 0)
                        {
                            amttable += "<tr class='border-top'><td colspan='5' style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'></td><td class='text-right' style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'>Discount </td><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' class='text-center'>" + pdfSummary.Discount + "</td></tr>";
                        }
                        if (pdfSummary.TaxAmount > 0)
                        {
                            amttable += "<tr class='border-top'><td colspan='5' style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'></td><td class='text-right' style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'>VAT 5% </td><td class='text-center' style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'>" + pdfSummary.TaxAmount + "</td></tr>";
                        }
                        if (billsundry != null)
                        {
                            // bind bill sundry
                            str2 += bindSundry(billsundry, layName);
                            if (billsundry.Count > 0)
                            {
                                count += billsundry.Count;
                            }
                        }
                        amttable += "<tr class='border-top'><td colspan='5' style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'><strong>UAE Dirham " + words + "</strong></td><td class='text-center' style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'><b>Net Total</b></td><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' class='text-center'><b>" + pdfSummary.GrandTotal + "</b></td></tr></table></td></tr>";
                        str1 = amttable;

                        //extend invoices
                        var exinv = "";
                        foreach (var exitem in ConvExtList)
                        {
                            exinv += exitem.BillNo + ",";
                        }
                        var extend = "<tr><p><span>Extended Invoices: </span><span>" + exinv + "</span></p></tr>";

                        if (tc != "")
                        {
                            str1 = "<tr style='border:1px solid;'><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'><p style='text-align:left;text-decoration: underline;font-weight: 800;margin-bottom: 5px;'>Terms & Conditions</p><div style='padding-left:5px;'>" + tc + "</div></td></tr>";
                        }
                        sb.Append(str1);
                    }
                    else
                    {
                        string disc = "";
                        string gtotal = "";
                        string taxamt = "";
                        string subtotal = "";
                        if (layName == "NewDefault")
                        {
                            disc = String.Format("{0:#,###0.00}", pdfSummary.Discount);
                            gtotal = String.Format("{0:#,###0.00}", pdfSummary.GrandTotal);
                            taxamt = String.Format("{0:#,###0.00}", pdfSummary.TaxAmount);
                            subtotal = String.Format("{0:#,###0.00}", pdfSummary.SubTotal);
                        }
                        else
                        {
                            disc = Convert.ToString(pdfSummary.Discount);
                            gtotal = Convert.ToString(pdfSummary.GrandTotal);
                            taxamt = Convert.ToString(pdfSummary.TaxAmount);
                            subtotal = Convert.ToString(pdfSummary.SubTotal);
                        }

                        count = 1;
                        if (pdfSummary.Discount > 0)
                        {
                            str2 += "<tr class='border-top'><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'>Discount</td><td class='text-right'>" + disc + "</td></tr> ";
                            count++;
                        }
                        else
                        { }
                        if (billsundry != null)
                        {
                            // bind bill sundry
                            str2 += bindSundry(billsundry, layName);
                            if (billsundry.Count > 0)
                            {
                                count += billsundry.Count;
                            }
                        }

                        var MpayTable = "<table class='table table-bordered' style='border:0px;border-collapse:collapse; width:100%;'><tr class='text-center'><th rowspan='2' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>Mode of Payment<br />عفدا ةقيرط</th><th class='text-center' rowspan='2' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>Curr</th><td colspan='2' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'><b>Amount المبلغ الجمالى</b></td></tr><tr class='border-top text-center'><td style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>FC</td><td style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>LC</td></tr>" +
                        "<tr class='text-center border-top'><td class='text-center' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>" + pdfSummary.paytype + " <br /> <b>Receipt Total</b></td><td style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>" + pdfSummary.Currency + "</td><td style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>" + pdfSummary.FCTotal + "</td><td style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>" + gtotal + " <br /><b>" + gtotal + "</b></td></tr></table>";

                        var TotalTable = "<table class='table table-bordered' style='border:0px;border-collapse:collapse; width:100%;'><tr class='text-center'><th class='text-center' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>Total VATةبيرضلا عومجم </th><th class='text-right' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>" + taxamt + "</th></tr><tr><td class='text-center' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>Sub Total</td><td class='text-right' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>" + subtotal + "</td></tr>" + str2 +
                            "<tr><td style='border-bottom: 1px solid !important;border-top: 1px solid !important;'><b>Net Total ةيلامجالا ةميقلا </b></td><td class='text-right' style='border-bottom: 1px solid !important;border-top: 1px solid !important;'><b>" + gtotal + "</b></td><td></td></tr></table>";
                        var wordHtml = "<tr class='text-center border-top'><td class='no-padding' style='width: 65%;padding-right: 5% !important;'>" + MpayTable + "</td><td></td><td style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;' class='no-padding'>" + TotalTable + "</td></tr>";
                        var nettotal = "<tr style='border:0px;'><td class='noborder' colspan='3' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;text-align:left;'><strong>" + words + "</strong></td></tr>";

                        var finaltotal = "<tr class='border-top' style='border:0px;border-collapse:collapse;'><td style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'><b>Total Taxable Amount</b></td><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'><b> ةبيرضلل عضاخلا غلبملا يلامجإ</b></td><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' class='text-right'><b>" + subtotal + "</b></td></tr>" +
                            "<tr class='border-top' style='border:0px;border-collapse:collapse;'><td style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'><b>Total VAT </b></td><td style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'><b> ةبيرضلا عومجم</b></td><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' class='text-right'><b>" + taxamt + "</b></td></tr>" +
                            "<tr class='border-top' style='border:0px;border-collapse:collapse;'><td style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'><b>TOTAL</b></td><td style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'><b> عومجملا </b></td><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'><b>" + gtotal + "</b></td></tr>";

                        var remarks = "";
                        if (remark != "")
                        {
                            remarks = "<tr class='border-top'><td colspan='8'><strong>Remarks </strong><br /> " + remark + "</td></tr>";
                        }
                        str1 = wordHtml + nettotal;
                        sb.Append(str1);
                        sb.Append(finaltotal);
                    }
                    sb.Append("</table>");


                    if (layName == "Jewellery")
                    {
                        sb.Append("<table width='100%' style='border: .1px solid #ccc;border-collapse:collapse;'>");
                        sb.Append("<tr>");
                        sb.Append("<td  style='width:50%;'>Confirmed on behalf of :<br /> " + pdfSummary.PartyName + "");
                        sb.Append("</td>");
                        sb.Append("<td style='width:50%;text-align: right;' class='text-bold text-center'><br /> " + cdetails.CName + "<br />" + Foot_2.Lang + "");
                        sb.Append("</td>");
                        sb.Append("</tr>");
                        sb.Append("</table>");
                        sb.Append("<table width='100%' style='border: .1px solid #ccc;border-collapse:collapse;'>");
                        sb.Append("<tr>");
                        sb.Append("<td class='text-center text-bold' style='width:35%;'>");
                        sb.Append("<div>");
                        sb.Append("<hr class='hrfoot'/>");
                        sb.Append(" ليمعا عيقوت<br />" + Customer.Value + "'S SIGNATURE");
                        sb.Append("</div>");
                        sb.Append("</td>");
                        sb.Append("<td class='text-center text-bold'  style='width:30%;text-align: center;'>");
                        sb.Append("<div>");
                        sb.Append("<hr class='hrfoot' style='text-align:right;'/>");
                        sb.Append(" ةعجارما <br /> CHECKED BY");
                        sb.Append("</div>");
                        sb.Append("</td>");
                        sb.Append("<td class='text-center text-bold'  style='width:35%;text-align: right;'>");
                        sb.Append("<div>");
                        sb.Append("<hr class='hrfoot'/>");
                        sb.Append("لوئسما فظوما عيقوت<br /> AUTHORISED SIGNATORY");
                        sb.Append("</div>");
                        sb.Append("</td>");
                        sb.Append("</tr>");
                        sb.Append("</table>");
                    }
                    else if (layName == "Scaffold")
                    {
                        sb.Append("<table width='100%' style='border: .1px solid #ccc;border-collapse:collapse;'>");
                        sb.Append("<tr>");
                        sb.Append("<td class='text -center text-bold' style='width:24%;'>");

                        if (pdfSummary.HSCode != "")
                        {
                            sb.Append("<div id = 'hscode'>");
                            sb.Append("<p> HS CODE : <span>" + pdfSummary.HSCode + " </span></p>");
                            sb.Append("<p> U.A.E.ORIGIN </p>");
                            sb.Append("<p> MADE ACE</p>");
                            sb.Append("</div>");
                        }
                        sb.Append("<hr class='hrfoot' style='margin-top: 100px;' /> " + Customer.Value + "'S SIGNATURE");
                        sb.Append("</td>");

                        sb.Append("<td style='width:56%; padding: 10px;'>");
                        sb.Append("<p class='text-bold' style='text-align: center;margin: 0px;padding-top: 0px;'>BANK DETAILS</p>");
                        sb.Append("<table class='table-nob noborder scaff-cus' style='border: 1px solid;width: 100%;' id='partyhead'>");
                        sb.Append("<tr><td style = 'padding-left: 3px;width: 100px;' ><b> Account Name</b></td>");
                        sb.Append("<td style = 'width:2%;'>:</td>");
                        sb.Append("<td>ACE ALUMINIUM</td>");
                        sb.Append("</tr>");
                        sb.Append("<tr><td style = 'padding-left: 3px;' ><b> Bank Name</b></td><td>:</td>");
                        sb.Append("<td> HABIB BANK AG ZURICH, AL FALAH BR, ABU DHABI</td>");
                        sb.Append("</tr><tr><td style = 'padding-left: 3px;' ><b> Swift Code</b></td>");
                        sb.Append("<td>:</td>");
                        sb.Append("<td>HBZUAEADXXX</td>");
                        sb.Append("</tr><tr><td style = 'padding-left: 3px;' ><b> IBAN </b></td><td>:</td>");
                        sb.Append("<td>AE1602907203111050877735</td>");
                        sb.Append("</tr><tr>");
                        sb.Append("<td style = 'padding -left: 3px;' ><b> Account No</b></td>");
                        sb.Append("<td>:</td>");
                        sb.Append("<td>0203070203111050877735</td>");
                        sb.Append("</tr></table></td>");

                        sb.Append("<td class='text - center text - bold' style='width:24%;'>");
                        sb.Append("<div><hr class='hrfoot' style='margin - top: 100px;'/>For " + cdetails.CName + "</div>");
                        sb.Append("</td>");
                        sb.Append("</tr>");
                        sb.Append("</table>");
                    }
                    else
                    {
                        //sb.Append("<table width='100%' style='border: .1px solid #ccc;border-collapse:collapse;'>");
                        //sb.Append("<tr>");
                        //sb.Append("<td align='left' width='50%' style='border: .1px solid #ccc;padding: 10px;vertical-align: top;'>");
                        //sb.Append("<div style='font-size: 14px;text-align: left;'>Receiver's Signature:<br />توقيع المتلقي</div>");
                        //sb.Append("</td>");
                        //sb.Append("<td style='border: .1px solid #ccc;padding: 10px;vertical-align: top;'>");
                        //sb.Append("<div style='font-size: 14px;text-align: left;'>");
                        //sb.Append("For " + cdetails.CName + "");
                        //sb.Append("</div>");
                        //sb.Append("</td>");
                        //sb.Append("</tr>");
                        //sb.Append("</table>");
                    }
                    if (pdfSummary.chid == 1)
                    {
                        //    str4= "<tr style='border:1px solid;'><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'><p style='text-align:left;text-decoration: underline;font-weight: 800;margin-bottom: 5px;'>Terms & Conditions</p><div style='padding-left:5px;'>" + tc + "</div></td></tr>";

                        //str31 = "<div style='bottom:150 px;position:absolute !important'></div>";
                        //"<img width='108' height='76'style='display:block; margin-right: auto; margin-left: auto;border:2 px;' src='" + LegacyWeb.MapPath("/uploads/companyheader/footer/zeal.jpg") + "'/>";

                        str4 = "<table width='100%' style='border: 0px;text-align:center; '><tr><td > <img width='108' height='76'style='display:block; margin-right: auto; margin-left: auto;border:2 px;' src='" + LegacyWeb.MapPath("/logos/zeal.jpg") + "'/>  </td></tr></table>";


                    }

                    sb.Append(str4);
                }
            }
            if (System.IO.File.Exists(LegacyWeb.MapPath("/logos/zeal.jpg")))
            {
                string zeal = "<img src='" + LegacyWeb.MapPath("/logos/zeal.jpg") + "' width='100px' height ='50px'/>";

                sb.Append("<table><tr><td>" + zeal + "</td></tr></table>");
            }
            return sb;
        }

        public StringBuilder generatepdf2(long id, pdfSummaryViewModel pdfSummary, List<pdfItemViewModel> pdfItem, List<pdfBillSundryViewModel> billsundry, string type)
        {

            var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
            var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

            var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
            var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

            Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

            var conv = db.ConvertTransactionss.Any(u => u.To == id);
            List<ConvertTransactionsViewModel> ConvExt = new List<ConvertTransactionsViewModel>();
            //var ConvModel=;
            if (conv)
            {
                List<string> ExList = new List<string>();
                List<ConvertTransactions> ExtList = new List<ConvertTransactions>();
                ExtList = ExtNum(id, ExtList);
                var Extended = ExtList.Select(z => z.To).ToList();
                Int32 count = 0;
                ConvExt = (from a in db.ConvertTransactionss
                           join b in db.SalesEntrys on a.To equals b.SalesEntryId into primary
                           from b in primary.DefaultIfEmpty()
                           where Extended.Contains(a.To)
                           select new ConvertTransactionsViewModel
                           {
                               ConvertFrom = (a.ConvertFrom == "SaleExtend") ? "Sale" : (a.ConvertFrom == "Quote") ? "Quotation" : (a.ConvertFrom == "DVNote") ? "Delivery Note" : a.ConvertFrom,
                               Id = a.Id,
                               BillNo = b.BillNo,
                               CreatedDate = a.CreatedDate,
                               From = a.From
                           }).OrderBy(b => b.CreatedDate).ToList();
            }

            var st = ConvExt.Find(c => c.BillNo == pdfSummary.BillNo);
            ConvExt.Remove(st);
            var ConvExtList = ConvExt;

            var cdetails = db.companys
            .Select(s => new
            {
                CName = s.CPName,
                CAddress = s.CPAddress,
                CEmail = s.CPEmail,
                CTaxRegNo = s.TRN,
                CPhone = s.CPPhone,
                s.CPMobile,
                CLogo = s.CPLogo,

            }).FirstOrDefault();

            int SI = 1;
            string address = "";
            if (pdfSummary.Address != null)
            {
                address += pdfSummary.Address;
            }
            if (pdfSummary.City != null)
            {
                address += pdfSummary.Address != null ? "<br />" + pdfSummary.City : pdfSummary.City;
            }
            else if (pdfSummary.State != null)
            {
                address += address != "" ? "<br />" + pdfSummary.State : pdfSummary.State;
            }
            else if (pdfSummary.Country != null)
            {
                address += address != "" ? "<br />" + pdfSummary.Country : pdfSummary.Country;
            }
            else if (pdfSummary.Zip != null)
            {
                address += address != "" ? "<br />" + pdfSummary.Zip : pdfSummary.Zip;
            }
            address += " <br/> Phone : ";
            if (pdfSummary.Mobile != null)
            {
                address += pdfSummary.Mobile;
                if (pdfSummary.Phone != null)
                {
                    address += ", " + pdfSummary.Phone;
                }
            }
            else
            {
                if (pdfSummary.Phone != null)
                {
                    address += pdfSummary.Phone;
                }
            }
            if (pdfSummary.Email != null)
            {
                address += "<br/> Email : " + pdfSummary.Email;
            }
            if (pdfSummary.TRN != "")
            {
                address += "<br/><b>TRN</b> : " + pdfSummary.TRN;
            }


            var def = Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
            def = def == 0 ? 1 : def;
            var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();
            string layName = (layout == null || layout.Name == "undefined") ? "Default" : layout.Name;


            //InvoiceLayout layout = db.InvoiceLayouts.Find(1);
            InvoiceLayoutViewModel Vmodel = new InvoiceLayoutViewModel();
            //Vmodel.Id = layout.Id;
            //Vmodel.Name = layout.Name;
            //Vmodel.Status = layout.Status;
            var section = type != "" ? type : "Sale";
            Vmodel.InvoiceField = db.InvoiceFields.Where(a => a.Section == section || a.Section == null).ToList();


            var title = Vmodel.InvoiceField.Where(a => a.Type == "Title").FirstOrDefault();

            var Customer = (type == "LPO" || type == "Purchase") ? Vmodel.InvoiceField.Where(a => a.Type == "Supplier").FirstOrDefault() : Vmodel.InvoiceField.Where(a => a.Type == "Customer").FirstOrDefault();

            var Cust_Name = Vmodel.InvoiceField.Where(a => a.Type == "Cust_Name").FirstOrDefault();
            var Cust_Address = Vmodel.InvoiceField.Where(a => a.Type == "Cust_Address").FirstOrDefault();
            var Cust_City = Vmodel.InvoiceField.Where(a => a.Type == "Cust_City").FirstOrDefault();
            var Cust_State = Vmodel.InvoiceField.Where(a => a.Type == "Cust_State").FirstOrDefault();
            var Cust_Country = Vmodel.InvoiceField.Where(a => a.Type == "Cust_Country").FirstOrDefault();
            var Cust_Zip = Vmodel.InvoiceField.Where(a => a.Type == "Cust_Zip").FirstOrDefault();
            var Cust_Mobile = Vmodel.InvoiceField.Where(a => a.Type == "Cust_Mobile").FirstOrDefault();
            var Cust_Phone = Vmodel.InvoiceField.Where(a => a.Type == "Cust_Phone").FirstOrDefault();
            var Cust_Email = Vmodel.InvoiceField.Where(a => a.Type == "Cust_Email").FirstOrDefault();
            var Cust_TRN = Vmodel.InvoiceField.Where(a => a.Type == "Cust_TRN").FirstOrDefault();
            var Cust_Fax = Vmodel.InvoiceField.Where(a => a.Type == "Cust_Fax").FirstOrDefault();
            var Cust_Credit_Period = Vmodel.InvoiceField.Where(a => a.Type == "Cust_Credit_Period").FirstOrDefault();

            var Bill_Amount = Vmodel.InvoiceField.Where(a => a.Type == "Bill_Amount").FirstOrDefault();
            var Bill_Tax = Vmodel.InvoiceField.Where(a => a.Type == "Bill_Tax").FirstOrDefault();
            var Bill_Discount = Vmodel.InvoiceField.Where(a => a.Type == "Bill_Discount").FirstOrDefault();
            var Bill_roundN = Vmodel.InvoiceField.Where(a => a.Type == "Bill_roundN").FirstOrDefault();
            var Bill_roundP = Vmodel.InvoiceField.Where(a => a.Type == "Bill_roundP").FirstOrDefault();
            var Bill_Total = Vmodel.InvoiceField.Where(a => a.Type == "Bill_Total").FirstOrDefault();
            var Bill_TotalWord = Vmodel.InvoiceField.Where(a => a.Type == "Bill_TotalWord").FirstOrDefault();

            var InvoiceNo = Vmodel.InvoiceField.Where(a => a.Type == "InvoiceNo").FirstOrDefault();
            var Date = Vmodel.InvoiceField.Where(a => a.Type == "Date").FirstOrDefault();
            var PO_No = Vmodel.InvoiceField.Where(a => a.Type == "PO_No").FirstOrDefault();
            var SalesExecutive = Vmodel.InvoiceField.Where(a => a.Type == "SalesExecutive").FirstOrDefault();
            var PaymentType = Vmodel.InvoiceField.Where(a => a.Type == "PaymentType").FirstOrDefault();
            var DeliveryNote = Vmodel.InvoiceField.Where(a => a.Type == "DeliveryNote").FirstOrDefault();


            var Terms = Vmodel.InvoiceField.Where(a => a.Type == "Terms").FirstOrDefault();
            var Foot_1 = Vmodel.InvoiceField.Where(a => a.Type == "Foot_1").FirstOrDefault();
            var Foot_2 = Vmodel.InvoiceField.Where(a => a.Type == "Foot_2").FirstOrDefault();

            var MakeIn = db.EnableSettings.Where(a => a.EnableType == "MakeInTrans").FirstOrDefault();
            var Make = MakeIn != null ? MakeIn.Status : Status.inactive;

            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    if (layName == "Jewellery")
                    {
                        sb.Append("<table width='100%' style='border: 0px;text-align:center;'><tr><td><b>" + title.Value + " " + title.Lang + "</b></td></tr></table>");
                        string partyDetails = "<table style='border: 0px;' width='100%'>" +
                            "<tr style='border-top:0px'>" +
                            "<td width='50%' style='padding:0px 10px 0px 0px; border-right:0px'>" +
                            "<table class='table-nob jewel-cus' style='border:1px solid #000;width: 100%;'>" +
                            "<tr><th><i><b>" + Customer.Value + "</b></i><i><b>" + Customer.Lang + " </b></i></th>" +
                            "<td style='text-align:left;'>: " + pdfSummary.PartyName + "</td>" +
                            "</tr><tr><td colspan='3'>" + address + "" +
                            "</td></tr>" +
                            //<tr><td colspan ='3'>" +
                            //"TRN:" + pdfSummary.TRN + "</td></tr>" +
                            "</table></td>" +
                            "<td style='padding: 0px;'><table id='Cust_de' class='table-nob jewel-inv' style='border: 1px solid #000;width: 100%;'> " +
                                        "<tr><th> " + InvoiceNo.Value + " " + InvoiceNo.Lang + " </th>" +
                                        "<td>:" + pdfSummary.BillNo + "</td>" +
                                        "</tr><tr><th>" + Date.Value + " " + Date.Lang + "</th>" +
                                        "<td>:" + pdfSummary.Date.ToString("dd-MM-yyyy") + "</td></tr>";

                        if (PO_No != null && PO_No.Status == Status.active)
                        {
                            partyDetails += "<tr><th> " + PO_No.Value + "</th><td>" + pdfSummary.PONo + "</td></tr>";
                        }
                        if (SalesExecutive.Status == Status.active)
                        {
                            partyDetails += "<tr><th> " + SalesExecutive.Value + " " + SalesExecutive.Lang + "</th><td>" + pdfSummary.Cashier + "</td></tr>";
                        }

                        if (DeliveryNote.Status == Status.active)
                        {
                            partyDetails += "<tr><th> " + DeliveryNote.Value + "</th><td>" + pdfSummary.ConvertNo + "</td></tr>";
                        }
                        if (PaymentType.Status == Status.active)
                        {
                            partyDetails += "<tr><th> " + PaymentType.Value + "</th><td>" + pdfSummary.paytype + "</td></tr>";
                        }
                        if (Cust_Credit_Period.Status == Status.active)
                        {
                            partyDetails += "<tr><th> " + Cust_Credit_Period.Value + "</th><td>" + pdfSummary.CreditPeriod + "</td></tr>";
                        }
                        if (pdfSummary.PrjNameCode != null)
                        {
                            partyDetails += "<tr><th>Project</th><td>" + pdfSummary.PrjNameCode + "</td></tr>";
                        }
                        if (pdfSummary.AgainstInvoice != null)
                        {
                            partyDetails += "<tr><th>Against Invoice</th><td>" + pdfSummary.AgainstInvoice + "</td></tr>";
                        }
                        partyDetails += "</table></td></tr></table>";
                        sb.Append(partyDetails);

                    }
                    else if (layName == "Scaffold")
                    {
                        sb.Append("<table width='100%' style='border: 0px;text-align:center;'><tr><td><b>" + title.Value + " " + title.Lang + "</b></td></tr></table>");

                        string partyDetails = "<table style='border: 0px;' width='100%'><tr> " +
                            "<td width= '59%' style='padding: 2px 8px; border: 1px solid #000'>" +
                            "<table width= '100%' style='border: 0px;'> " +

                        "<tr><th style='width:28% !important;'><i><b>" + Customer.Value + "</b></i><i><b>" + Customer.Lang + " </b></i></th><td>:</td><td width='69%'>" + pdfSummary.PartyName + "</td></tr>" +
                        "<tr><th style='width:28% !important;'>ADDRESS</th><td>:</td><td>" + pdfSummary.Address + "</td></tr>" +
                        "<tr><th style='width:28% !important;'>MOBILE NO</th><td>:</td><td>" + pdfSummary.Mobile + "</td></tr>";

                        if (pdfSummary.Email != null)
                        {
                            partyDetails += "<tr><th style='width:28% !important;'>EMAIL</th><td>:</td><td>" + pdfSummary.Email + "</td></tr>";
                        }
                        partyDetails += "<tr><th style='width:28% !important;'>TRN</th><td>:</td><td>" + pdfSummary.TRN + "</td></tr>";
                        partyDetails += "<tr><th style='width:28% !important;'>CONTACT PERSON</th><td>:</td><td>" + pdfSummary.ContactPerson + "</td></tr>";
                        partyDetails += "</table></td>";

                        partyDetails += "<td width='1%' style='border:0px;padding: 0px;'></td>" +
                            "<td width='40%' style='padding: 0px; border: 0px;'>" +
                            "<table style='border:0px;border-collapse:collapse; width:100%; border-right:.1px solid;'>" +
                            "<tr><td style='border:1px solid #000 !important;padding: 4px;'>" + InvoiceNo.Value + "</td> " +
                            "<td colspan='2' style='border:1px solid #000 !important;padding: 4px;padding-bottom: 0px;padding-top: 0px;'>" + pdfSummary.BillNo + "</td>" +
                            "<td style='border: 1px solid #000 !important;padding: 4px;'>" + Date.Value + "</td>" +
                            "<td style='border: 1px solid #000 !important;padding: 4px;'>" + pdfSummary.Date.ToString("dd-MM-yyyy") + "</td>" +
                            "</tr>";

                        if (pdfSummary.Cashier != null && SalesExecutive.Status == Status.active)
                        {
                            partyDetails += "<tr><td colspan='2' style='border: 1px solid #000 !important;padding: 4px;'>" + SalesExecutive.Value + "</td>" +
                                "<td colspan='3' style='border: 1px solid #000 !important;padding: 4px;'>" + pdfSummary.Cashier + "</td></tr>";
                        }
                        if (pdfSummary.PONo != null && (PO_No != null && PO_No.Status == Status.active))
                        {
                            partyDetails += "<tr><td colspan='2' style='border: 1px solid #000 !important;padding: 4px;'>" + PO_No.Value + "</td>" +
                                "<td colspan='3' style='border: 1px solid #000 !important;padding: 4px;'>" + pdfSummary.PONo + "</td></tr>";
                        }
                        if (pdfSummary.ConvertNo != null && DeliveryNote.Status == Status.active)
                        {
                            partyDetails += "<tr><td colspan='2' style='border: 1px solid #000 !important;padding: 4px;'>" + DeliveryNote.Value + "</td>" +
                                "<td colspan='3' style='border: 1px solid #000 !important;padding: 4px;'>" + pdfSummary.ConvertNo + "</td></tr>";
                        }

                        partyDetails += "<tr><td colspan='2' style='border: 1px solid #000 !important;padding: 4px;'>PAYMENT TERMS</td>" +
                              "<td colspan='3' style='border: 1px solid #000 !important;padding: 4px;'>" + pdfSummary.PaymentTerms + "</td></tr>";

                        if (pdfSummary.PrjNameCode != "")
                        {
                            partyDetails += "<tr><td colspan='2' style='border: 1px solid #000 !important;padding: 4px;'>Project</td>" +
                              "<td colspan='3' style='border: 1px solid #000 !important;padding: 4px;'>" + pdfSummary.PrjNameCode + "</td></tr>";
                        }
                        if (pdfSummary.AgainstInvoice != null)
                        {
                            partyDetails += "<tr><th>Against Invoice</th>" +
                              "<td>" + pdfSummary.AgainstInvoice + "</td></tr>";
                        }

                        partyDetails += "</table>";
                        partyDetails += "</td></tr></table>";
                        sb.Append(partyDetails);
                    }
                    else
                    {

                        if (type != "Stock Transfer")
                        {

                            //sb.Append("<table width='100%' style='border: 0px;color:red;text-align:center;'><tr><td><b><u></u></b></td></tr></table>");
                            //sb.Append("<table width='100%' style='border: 0px;color:red;text-align:center;'><tr><td><b><u></u></b></td></tr></table>");


                            sb.Append("<table width='100%' style='color:rgb(58 107 196 / 0.88) !important;text-align:center;height:30px;'><tr style='height:30px;'><td style='height:30px;'><b>" + title.Value + " " + title.Lang + "</b><hr style='border:1px solid red;color:rgb(58 107 196 / 0.88) !important;'></hr></td></tr></table>");
                            //sb.Append("<table width='100%' style='color:white;background-color:rgb(58 107 196 / 0.88) !important;text-align:center;padding-top:5px !important;;height:30px;'><tr style='height:30px;'><td style='height:30px;'><b>" + title.Value + " " + title.Lang + "</b><hr style='border:1px solid red;color:rgb(58 107 196 / 0.88) !important;'></hr></td></tr></table>");
                            //sb.Append("<table width='100%' style='border: 0px;color:red;text-align:center;'><tr><td><b><u></u></b></td></tr></table>");
                            //sb.Append("<table width='100%' style='border: 0px;color:red;text-align:center;'><tr><td><b><u></u></b></td></tr></table>");
                            string partyDetails = "<table><tr > " +
                                "<td width='46.4%'> " +
                                "<table  style=' width: 100 %;'><tr><th><i><b>" + Customer.Value + " " + Customer.Lang + "</b></i></th></tr><tr><td>" + pdfSummary.PartyName + "</td></tr><tr><td style='font-size:14px;font-weight:normal;'>" + address + "</td></tr></table></td><td width='50%' >" +
                                "<table  style=' width: 100 %;'><tr><th style='font-size:14px;'>" + InvoiceNo.Value + " " + InvoiceNo.Lang + "</th><td style='font-size:14px;font-weight:normal;'>: " + pdfSummary.BillNo + "</td></tr><br/><br/><tr><th style='font-size:14px;'>" + Date.Value + " " + Date.Lang + "</th><td style='font-size:14px;font-weight:normal;'>: " + pdfSummary.Date.ToString("dd-MM-yyyy") + "</td></tr>";
                            //check the salesexecutive(employee)
                            if (pdfSummary.Cashier != "  " && pdfSummary.Cashier != null && pdfSummary.Cashier != "")
                            {
                                partyDetails += "<tr><th style='font-size:14px;'>" + SalesExecutive.Value + " " + SalesExecutive.Lang + "</th><td style='font-size:14px;font-weight:normal;'>: " + pdfSummary.Cashier + "</td></tr>";
                                //checking the email id of corresponding employee
                                if (pdfSummary.empemail != null)
                                {
                                    partyDetails += "<tr><th style='font-size:14px;'>" + "e-Mail" + " </ th ><td style='font-size:14px;font-weight:normal;'>: " + pdfSummary.empemail + " </ td ></ tr > <tr><td></td></tr>";
                                }
                                //checking the phone no
                                if (pdfSummary.ContactNo != null)
                                {
                                    partyDetails += "<tr><th style='font-size:14px;'>" + "Phone" + " </ th ><td style='font-size:14px;font-weight:normal;'>: " + pdfSummary.ContactNo + " </td ></ tr > ";
                                }

                            }
                            if (pdfSummary.AgainstInvoice != null)
                            {
                                partyDetails += "<tr><th>Against Invoice</th><td style='font-size:14px;font-weight:normal;'>: " + pdfSummary.AgainstInvoice + "</td></tr>";
                            }
                            partyDetails += "</table></td></tr></table>";

                            sb.Append(partyDetails);
                        }
                    }


                    //sb.Append("<div style='min-height:558px;>");
                    sb.Append("<table width='100%' style='font-size:12px;repeat-header:yes;'>");
                    sb.Append("<thead>");

                    if (layName == "Scaffold")
                    {
                        var SaleType = pdfSummary.SaleType;
                        //console.log("Sale type: " + SaleType);
                        var Subject = "Subject : ";
                        if (SaleType == SaleType.Sale)
                        {
                            Subject += "<b class='text-green' style='font-size: large;font-weight: 600;'>SALE</b>";
                        }
                        else if (SaleType == SaleType.Hire)
                        {
                            var From = pdfSummary.FromDate;
                            var To = pdfSummary.ToDate;
                            //var diff = moment.preciseDiff(e.summary.FromDate, e.summary.ToDate);
                            decimal diff = 0;
                            var startDate = string.Format("{0:dd.MM.yyyy HH:mm:ss}", pdfSummary.FromDate);
                            var endDate = string.Format("{0:dd.MM.yyyy HH:mm:ss}", pdfSummary.ToDate);
                            var HireType = pdfSummary.HireType;
                            var Htype = (HireType == "Weekly") ? "week" : (HireType == "Monthly") ? "month" : "days";
                            var HtypeV = (HireType == "Weekly") ? "Week" : (HireType == "Monthly") ? "Month" : "Days";
                            if (Htype == "days")
                            {
                                DateTime dt1 = Convert.ToDateTime(pdfSummary.ToDate);
                                DateTime dt2 = Convert.ToDateTime(pdfSummary.FromDate);
                                TimeSpan tspan = dt2.Subtract(dt1);
                                diff = tspan.Days;
                            }
                            else if (Htype == "week")
                            {
                                diff = tocountweek(endDate, startDate);
                            }
                            else
                            {
                                diff = tocountmonth(endDate, startDate);
                            }

                            //console.log(diff);
                            Subject += "<b>HIRE OF ALUMINIUM SCAFFOLDING FOR " + diff + " " + HtypeV + "(STARTING FROM " + string.Format("{0:dd-MM-yyyy}", pdfSummary.FromDate) + " TO " + string.Format("{0:dd-MM-yyyy}", pdfSummary.ToDate) + " ) </b>";
                        }

                        sb.Append("<tr><td colspan='3' style='padding: 5px;'>" + Subject + "</td></tr>");

                    }
                    if (layName == "Jewellery")
                    {
                        sb.Append("<tr style='font-size:13px;'>");
                        sb.Append("<th width='1%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;text-align:center;'> م.ر <br /> No</th>");
                        sb.Append("<th width='2%'  style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;text-align:center;'>ةعطقا مقر <br />Item Code</th>");
                        sb.Append("<th width='10%' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;text-align:center;'>تافصاوما Description <br />بند</th>");
                        sb.Append("<th width='2%' style='border: .5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;text-align:center;'>عطقا <br />PCS</th>");
                        sb.Append("<th width='4%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;text-align:center;'>الوزن الجمالي <br />Qty(Gms/Cts)</th>");
                        sb.Append("<th width='3%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;text-align:center;'>ةدحوا ةميق <br />Rate</th>");
                        sb.Append("<th width='3%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;text-align:center;'>المبلغ الجمالى <br />Amount</th>");
                        sb.Append("</tr>");
                    }
                    else if (layName == "Scaffold")
                    {

                        sb.Append("<tr style='font-size:13px;'>");
                        sb.Append("<th width='5%' style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>S/N</th>");
                        if (PartNoCheck == Status.active)
                        {
                            sb.Append("<th style='border:.5px #000000;padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Part No</th>");
                        }

                        //sb.Append("<th style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'>Part No</th>");
                        sb.Append("<th style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' colspan='2'>Description of goods</th>");
                        //if (Make == Status.active)
                        //{
                        //    sb.Append("<th style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' width='7%'>Make</th>");
                        //}
                        sb.Append("<th style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' width='7%'>Wt(KG)</th>");
                        sb.Append("<th style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' width='5%'>CBM</th>");
                        sb.Append("<th style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' width='8%'>Qty</th>");

                        sb.Append("<th style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' width='6%'>Rate<br /> (AED)</th>");
                        sb.Append("<th style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' width='8%'>Taxable Amount</th>");
                        sb.Append("<th style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' width='6%'>VAT %</th>");
                        sb.Append("<th style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' width='7%'>VAT Amount</th>");
                        sb.Append("<th style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' width='8%'>Amount <br /> (AED)</th>");


                        sb.Append("</tr>");


                    }
                    else
                    {
                        sb.Append("<hr style='border:1px solid red;color:rgb(58 107 196 / 0.88) !important;'></hr><tr style='font-size:13px;'>");
                        sb.Append("<th width='5%' style='padding: 5px;color:rgb(58 107 196 / 0.88) !important;vertical-align: top;'>S/N</th>");
                        if (PartNoCheck == Status.active)//&& type != "Stock Transfer")
                        {
                            sb.Append("<th width='5%' style='color:rgb(58 107 196 / 0.88) !important;padding: 5px;vertical-align: top;'>Part No</th>");
                        }
                        sb.Append("<th style=';color:rgb(58 107 196 / 0.88) !important;padding: 5px;vertical-align: top;'>Item</th>");
                        if (Make == Status.active)// && type != "Stock Transfer")
                        {
                            sb.Append("<th width='5%' style='color:rgb(58 107 196 / 0.88) !important;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>Make</th>");
                        }

                        sb.Append("<th width='4%' style='color:rgb(58 107 196 / 0.88) !important;padding: 3px;vertical-align: top;'>Unit</th>");
                        sb.Append("<th width='5%' style='color:rgb(58 107 196 / 0.88) !important;padding: 3px;vertical-align: top;'>Qty</th>");
                        sb.Append("<th width='6%' style='color:rgb(58 107 196 / 0.88) !important;padding: 3px;vertical-align: top;'>UPrice</th>");
                        sb.Append("<th width='8%' style='color:rgb(58 107 196 / 0.88) !important;padding: 3px;vertical-align: top;'>Amount</th>");
                        //if (type != "Stock Transfer")
                        //{
                        sb.Append("<th width='8%' style='color:rgb(58 107 196 / 0.88) !important;padding: 5px;vertical-align: top;'>Tax(5%)</th>");
                        //}
                        sb.Append("<th width='8%' style='padding: 5px; color:rgb(58 107 196 / 0.88) !important; vertical-align: top;'>Total</th>");
                        sb.Append("</tr>");
                    }

                    sb.Append("</thead>");
                    sb.Append("<tbody>");


                    string stritem = bindItems(pdfSummary, pdfItem, layName, InPrintItemCode, type);
                    sb.Append(stritem);

                    sb.Append("</tbody>");
                    sb.Append("</table>");


                    sb.Append("<table width='100%' style='border: 0px; border-spacing:0px;text-align:center;'>");

                    var str1 = "";
                    var str2 = "";
                    var str3 = "";
                    var count = 2;
                    var dvitem = "inactive";

                    var tc = "";
                    var termc = "";
                    if (pdfSummary.Note != null)
                    {
                        tc = pdfSummary.Note.Replace("\n", "<br />");
                    }
                    if (pdfSummary.TermsCondition != null)
                    {
                        termc = pdfSummary.TermsCondition.Replace("\n", "<br />");
                    }

                    var remark = "";
                    if (layName == "Scaffold" && pdfSummary.PaymentTerms != null)
                    {
                        remark = pdfSummary.PaymentTerms.Replace("\n", "<br />");
                    }
                    else
                    {
                        if (pdfSummary.Remarks != null)
                        {
                            remark = pdfSummary.Remarks.Replace("\n", "<br />");
                        }
                    }
                    string words = ConvertToWords(pdfSummary.GrandTotal.ToString());

                    if (layName == "Default")
                    {
                        if (pdfSummary.Discount > 0)
                        {
                            str2 += "<td style='vertical-align: middle;padding: 5px;font-size:14px;'>" + Bill_Discount.Value + "</td><td style='vertical-align: middle; padding: 5px;font-size:12px;' class='text-right'>" + pdfSummary.Discount + "</td></tr> ";
                            count++;
                            //str2 += "<tr class='border-top'><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'>" + Bill_Tax.Value + "</td><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' class='text-right'>" + pdfSummary.TaxAmount + "</td></tr>";
                        }
                        else
                        {
                            //str2 += "<td>VAT<span style='direction:ltr'>(5.00%)</span> برميل </td><td class='text-right'>" + parseFloat(e.summary.TaxAmount).toFixed(2) + "</td></tr>";
                            // str2 += "<td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'>" + Bill_Tax.Value + "</td><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' class='text-right'>" + pdfSummary.TaxAmount + "</td></tr>";
                            str2 += "</tr>";
                        }
                        if (billsundry != null)
                        {
                            // bind bill sundry
                            str2 += bindSundry(billsundry, layName);
                            if (billsundry.Count > 0)
                            {
                                count += billsundry.Count;
                            }
                        }
                        str2 += "<tr style='vertical-align: middle;padding: 5px;'><th style='vertical-align: middle;padding: 5px;font-size:14px;'>" + Bill_Total.Value + "</th><th style='vertical-align: middle;padding: 6px;font-size:12px;background-color:dodgerblue;color:white' class='text-right'>" + pdfSummary.GrandTotal + "</th></tr><hr style = 'border:1px solid;color:rgb(58 107 196 / 0.88) !important;' ></ hr >";

                        var wordHtml = "<tr style='width:1100%;' class='border-top'><td style='vertical-align: middle;width:104%;padding: 5px;font-size:12px;' colspan='5'><strong>" + words + "</strong></td><td style='vertical-align: middle;width:14%;padding: 5px;font-size:14px;'>" + Bill_Amount.Value + "</td><td style='vertical-align: middle;padding: 5px;font-size:12px;background-color:;width:14%' class='text-right'>" + pdfSummary.SubTotal + "</td></tr>";
                        str3 = "<tr class='border-top'><td></td></tr><tr class='border-top'><td style='text-align:left;padding: 5px;font-size:12px;' colspan='5' rowspan='" + count + "'>< div style='width:500px'>" + tc + " </div></td>";

                        var remarks = "";
                        //if (remark != "")
                        //{
                        //    remarks = "<tr class='border-top'><td style='text-align:left; padding: 5px;' colspan='8'><strong>Remarks </strong><br /> <span style='font-size:12px;'>" + remark + " </span></td></tr>";
                        //}
                        if (dvitem == "active")
                        {
                            str1 = str3 + "</tr>";
                        }
                        else
                        {
                            str1 = wordHtml + str3 + str2;
                        }

                        sb.Append(str1);

                    }
                    else if (layName == "General")
                    {
                        var remarks = "";
                        if (remark != "")
                        {
                            remarks += "<tr class='border-top style='color:rgb(58 107 196 / 0.88) !important;'><td style='text-align:left;color:rgb(58 107 196 / 0.88) !important; vertical-align: middle;border: .1px solid #ccc;padding: 5px;' colspan='8'><strong>Remarks </strong><br /> " + remark + "</td></tr>";
                        }
                        if (tc != "")
                        {
                            remarks += "<tr class='border-top style='color:rgb(58 107 196 / 0.88) !important;'><td style='text-align:left;color:rgb(58 107 196 / 0.88) !important; vertical-align: middle;border: .1px solid #ccc;padding: 5px;' colspan='8'><strong><u>Note :</u></strong><br /> " + tc + "</td></tr>";
                        }
                        if (termc != "")
                        {
                            remarks += "<tr class='border-topstyle='color:rgb(58 107 196 / 0.88) !important;'><td style='text-align:left;color:rgb(58 107 196 / 0.88) !important; vertical-align: middle;border: .1px solid #ccc;padding: 5px;' colspan='8'><strong><u>" + Terms.Value + " :</u></strong><br /> " + termc + "</td></tr>";
                        }

                        if (dvitem == "active")
                        {
                            str1 = str3 + remarks;
                        }
                        else
                        {
                            str1 = str3 + str2 + remarks + "<tr><td></td></tr>";
                        }
                        sb.Append(str1);
                    }
                    else if (layName == "Scaffold")
                    {
                        var amttable = "<tr class='border-top'><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'><table class='table table-bordered' style='width:100%;'>" +
                                       "<tr class='border-top'><td colspan='5' style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'></td><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' class='text-right'>Total w/o VAT </td><td class='text-center' style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'>" + pdfSummary.SubTotal + "</td></tr>";

                        if (pdfSummary.Discount > 0)
                        {
                            amttable += "<tr class='border-top'><td colspan='5' style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'></td><td class='text-right' style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'>Discount </td><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' class='text-center'>" + pdfSummary.Discount + "</td></tr>";
                        }
                        if (pdfSummary.TaxAmount > 0)
                        {
                            amttable += "<tr class='border-top'><td colspan='5' style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'></td><td class='text-right' style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'>VAT 5% </td><td class='text-center' style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'>" + pdfSummary.TaxAmount + "</td></tr>";
                        }
                        if (billsundry != null)
                        {
                            // bind bill sundry
                            str2 += bindSundry(billsundry, layName);
                            if (billsundry.Count > 0)
                            {
                                count += billsundry.Count;
                            }
                        }
                        amttable += "<tr class='border-top'><td colspan='5' style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'><strong>UAE Dirham " + words + "</strong></td><td class='text-center' style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'><b>Net Total</b></td><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' class='text-center'><b>" + pdfSummary.GrandTotal + "</b></td></tr></table></td></tr>";
                        str1 = amttable;

                        //extend invoices
                        var exinv = "";
                        foreach (var exitem in ConvExtList)
                        {
                            exinv += exitem.BillNo + ",";
                        }
                        var extend = "<tr><p><span>Extended Invoices: </span><span>" + exinv + "</span></p></tr>";

                        if (tc != "")
                        {
                            str1 = "<tr style='border:1px solid;'><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'><p style='text-align:left;text-decoration: underline;font-weight: 800;margin-bottom: 5px;'>Terms & Conditions</p><div style='padding-left:5px;'>" + tc + "</div></td></tr>";
                        }
                        sb.Append(str1);
                    }
                    else
                    {
                        string disc = "";
                        string gtotal = "";
                        string taxamt = "";
                        string subtotal = "";
                        if (layName == "NewDefault")
                        {
                            disc = String.Format("{0:#,###0.00}", pdfSummary.Discount);
                            gtotal = String.Format("{0:#,###0.00}", pdfSummary.GrandTotal);
                            taxamt = String.Format("{0:#,###0.00}", pdfSummary.TaxAmount);
                            subtotal = String.Format("{0:#,###0.00}", pdfSummary.SubTotal);
                        }
                        else
                        {
                            disc = Convert.ToString(pdfSummary.Discount);
                            gtotal = Convert.ToString(pdfSummary.GrandTotal);
                            taxamt = Convert.ToString(pdfSummary.TaxAmount);
                            subtotal = Convert.ToString(pdfSummary.SubTotal);
                        }

                        count = 1;
                        if (pdfSummary.Discount > 0)
                        {
                            str2 += "<tr class='border-top'><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'>Discount</td><td class='text-right'>" + disc + "</td></tr> ";
                            count++;
                        }
                        else
                        { }
                        if (billsundry != null)
                        {
                            // bind bill sundry
                            str2 += bindSundry(billsundry, layName);
                            if (billsundry.Count > 0)
                            {
                                count += billsundry.Count;
                            }
                        }

                        var MpayTable = "<table class='table table-bordered' style='border:0px;border-collapse:collapse; width:100%;'><tr class='text-center'><th rowspan='2' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>Mode of Payment<br />عفدا ةقيرط</th><th class='text-center' rowspan='2' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>Curr</th><td colspan='2' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'><b>Amount المبلغ الجمالى</b></td></tr><tr class='border-top text-center'><td style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>FC</td><td style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>LC</td></tr>" +
                        "<tr class='text-center border-top'><td class='text-center' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>" + pdfSummary.paytype + " <br /> <b>Receipt Total</b></td><td style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>" + pdfSummary.Currency + "</td><td style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>" + pdfSummary.FCTotal + "</td><td style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>" + gtotal + " <br /><b>" + gtotal + "</b></td></tr></table>";

                        var TotalTable = "<table class='table table-bordered' style='border:0px;border-collapse:collapse; width:100%;'><tr class='text-center'><th class='text-center' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>Total VATةبيرضلا عومجم </th><th class='text-right' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>" + taxamt + "</th></tr><tr><td class='text-center' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>Sub Total</td><td class='text-right' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'>" + subtotal + "</td></tr>" + str2 +
                            "<tr><td style='border-bottom: 1px solid !important;border-top: 1px solid !important;'><b>Net Total ةيلامجالا ةميقلا </b></td><td class='text-right' style='border-bottom: 1px solid !important;border-top: 1px solid !important;'><b>" + gtotal + "</b></td><td></td></tr></table>";
                        var wordHtml = "<tr class='text-center border-top'><td class='no-padding' style='width: 65%;padding-right: 5% !important;'>" + MpayTable + "</td><td></td><td style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;' class='no-padding'>" + TotalTable + "</td></tr>";
                        var nettotal = "<tr style='border:0px;'><td class='noborder' colspan='3' style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;text-align:left;'><strong>" + words + "</strong></td></tr>";

                        var finaltotal = "<tr class='border-top' style='border:0px;border-collapse:collapse;'><td style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'><b>Total Taxable Amount</b></td><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'><b> ةبيرضلل عضاخلا غلبملا يلامجإ</b></td><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' class='text-right'><b>" + subtotal + "</b></td></tr>" +
                            "<tr class='border-top' style='border:0px;border-collapse:collapse;'><td style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'><b>Total VAT </b></td><td style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'><b> ةبيرضلا عومجم</b></td><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;' class='text-right'><b>" + taxamt + "</b></td></tr>" +
                            "<tr class='border-top' style='border:0px;border-collapse:collapse;'><td style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'><b>TOTAL</b></td><td style='border: .5px #000000;padding: 5px;vertical-align: top;border: 1px solid #ccc;'><b> عومجملا </b></td><td style='vertical-align: middle;border: .1px solid #ccc;padding: 5px;'><b>" + gtotal + "</b></td></tr>";

                        var remarks = "";
                        if (remark != "")
                        {
                            remarks = "<tr class='border-top'><td colspan='8'><strong>Remarks </strong><br /> " + remark + "</td></tr>";
                        }
                        str1 = wordHtml + nettotal;
                        sb.Append(str1);
                        sb.Append(finaltotal);
                    }
                    sb.Append("</table>");
                    //LegacyWeb.MapPath("/logos/zeal.jpg")


                    if (layName == "Jewellery")
                    {
                        sb.Append("<table width='100%' style='border: .1px solid #ccc;border-collapse:collapse;'>");
                        sb.Append("<tr>");
                        sb.Append("<td  style='width:50%;'>Confirmed on behalf of :<br /> " + pdfSummary.PartyName + "");
                        sb.Append("</td>");
                        sb.Append("<td style='width:50%;text-align: right;' class='text-bold text-center'><br /> " + cdetails.CName + "<br />" + Foot_2.Lang + "");
                        sb.Append("</td>");
                        sb.Append("</tr>");
                        sb.Append("</table>");
                        sb.Append("<table width='100%' style='border: .1px solid #ccc;border-collapse:collapse;'>");
                        sb.Append("<tr>");
                        sb.Append("<td class='text-center text-bold' style='width:35%;'>");
                        sb.Append("<div>");
                        sb.Append("<hr class='hrfoot'/>");
                        sb.Append(" ليمعا عيقوت<br />" + Customer.Value + "'S SIGNATURE");
                        sb.Append("</div>");
                        sb.Append("</td>");
                        sb.Append("<td class='text-center text-bold'  style='width:30%;text-align: center;'>");
                        sb.Append("<div>");
                        sb.Append("<hr class='hrfoot' style='text-align:right;'/>");
                        sb.Append(" ةعجارما <br /> CHECKED BY");
                        sb.Append("</div>");
                        sb.Append("</td>");
                        sb.Append("<td class='text-center text-bold'  style='width:35%;text-align: right;'>");
                        sb.Append("<div>");
                        sb.Append("<hr class='hrfoot'/>");
                        sb.Append("لوئسما فظوما عيقوت<br /> AUTHORISED SIGNATORY");
                        sb.Append("</div>");
                        sb.Append("</td>");
                        sb.Append("</tr>");
                        sb.Append("</table>");
                    }
                    else if (layName == "Scaffold")
                    {
                        sb.Append("<table width='100%' style='border: .1px solid #ccc;border-collapse:collapse;'>");
                        sb.Append("<tr>");
                        sb.Append("<td class='text -center text-bold' style='width:24%;'>");

                        if (pdfSummary.HSCode != "")
                        {
                            sb.Append("<div id = 'hscode'>");
                            sb.Append("<p> HS CODE : <span>" + pdfSummary.HSCode + " </span></p>");
                            sb.Append("<p> U.A.E.ORIGIN </p>");
                            sb.Append("<p> MADE ACE</p>");
                            sb.Append("</div>");
                        }
                        sb.Append("<hr class='hrfoot' style='margin-top: 100px;' /> " + Customer.Value + "'S SIGNATURE");
                        sb.Append("</td>");

                        sb.Append("<td style='width:56%; padding: 10px;'>");
                        sb.Append("<p class='text-bold' style='text-align: center;margin: 0px;padding-top: 0px;'>BANK DETAILS</p>");
                        sb.Append("<table class='table-nob noborder scaff-cus' style='border: 1px solid;width: 100%;' id='partyhead'>");
                        sb.Append("<tr><td style = 'padding-left: 3px;width: 100px;' ><b> Account Name</b></td>");
                        sb.Append("<td style = 'width:2%;'>:</td>");
                        sb.Append("<td>ACE ALUMINIUM</td>");
                        sb.Append("</tr>");
                        sb.Append("<tr><td style = 'padding-left: 3px;' ><b> Bank Name</b></td><td>:</td>");
                        sb.Append("<td> HABIB BANK AG ZURICH, AL FALAH BR, ABU DHABI</td>");
                        sb.Append("</tr><tr><td style = 'padding-left: 3px;' ><b> Swift Code</b></td>");
                        sb.Append("<td>:</td>");
                        sb.Append("<td>HBZUAEADXXX</td>");
                        sb.Append("</tr><tr><td style = 'padding-left: 3px;' ><b> IBAN </b></td><td>:</td>");
                        sb.Append("<td>AE1602907203111050877735</td>");
                        sb.Append("</tr><tr>");
                        sb.Append("<td style = 'padding -left: 3px;' ><b> Account No</b></td>");
                        sb.Append("<td>:</td>");
                        sb.Append("<td>0203070203111050877735</td>");
                        sb.Append("</tr></table></td>");

                        sb.Append("<td class='text - center text - bold' style='width:24%;'>");
                        sb.Append("<div><hr class='hrfoot' style='margin - top: 100px;'/>For " + cdetails.CName + "</div>");
                        sb.Append("</td>");
                        sb.Append("</tr>");
                        sb.Append("</table>");
                    }
                    else
                    {
                        //sb.Append("<table width='100%' style='border: .1px solid #ccc;border-collapse:collapse;'>");
                        //sb.Append("<tr>");
                        //sb.Append("<td align='left' width='50%' style='border: .1px solid #ccc;padding: 10px;vertical-align: top;'>");
                        //sb.Append("<div style='font-size: 14px;text-align: left;'>Receiver's Signature:<br />توقيع المتلقي</div>");
                        //sb.Append("</td>");
                        //sb.Append("<td style='border: .1px solid #ccc;padding: 10px;vertical-align: top;'>");
                        //sb.Append("<div style='font-size: 14px;text-align: left;'>");
                        //sb.Append("For " + cdetails.CName + "");
                        //sb.Append("</div>");
                        //sb.Append("</td>");
                        //sb.Append("</tr>");
                        //sb.Append("</table>");
                    }

                }
            }
            if (System.IO.File.Exists(LegacyWeb.MapPath("/logos/zeal.jpg")))
            {
                string zeal = "<img src='" + LegacyWeb.MapPath("/logos/zeal.jpg") + "' width='100px' height ='50px'/>";

                sb.Append("<table><tr><td>" + zeal + "</td></tr></table>");
            }
            return sb;
        }






        private decimal tocountweek(string To, string From)
        {
            DateTime todate = DateTime.Parse(To, new CultureInfo("en-GB"));
            DateTime fromdate = DateTime.Parse(From, new CultureInfo("en-GB"));
            var frommonth = fromdate.Month;
            var fromyear = fromdate.Year;
            var tomonth = todate.Month;
            var toyear = todate.Year;
            var daysdiff = daysdifference(fromdate, todate);
            var wholeweek = daysdiff / 7;
            var quotient = daysdiff % 7;
            var totalweek = (quotient > 0) ? (wholeweek + 1) : wholeweek;
            return totalweek;
        }
        private decimal tocountmonth(string To, string From)
        {
            DateTime todate = DateTime.Parse(To, new CultureInfo("en-GB"));
            DateTime fromdate = DateTime.Parse(From, new CultureInfo("en-GB"));

            var fromyear = fromdate.Year;
            var toyear = todate.Year;

            var frommonth = fromdate.Month;
            var tomonth = todate.Month;
            var month = 0;
            if (fromyear != toyear)
            {
                var fromtotmonth = ((fromyear - 1) * 12) + frommonth;
                var tototmonth = ((toyear - 1) * 12) + tomonth;
                var Netdiff = tototmonth - fromtotmonth;
                month = (todate >= fromdate) ? (Netdiff + 1) : Netdiff;
            }
            else
            {
                month = (todate >= fromdate) ? (tomonth - frommonth) + 1 : (tomonth - frommonth);
            }
            return month;
        }
        private decimal daysdifference(DateTime date1, DateTime date2)
        {
            var ONEDAY = 1000 * 60 * 60 * 24;
            decimal newdate1 = Convert.ToInt64(date1.TimeOfDay.TotalHours);
            decimal newdate2 = Convert.ToInt64(date2.TimeOfDay.TotalHours);
            decimal difference = newdate1 - newdate2;
            return (difference / ONEDAY);
        }
        private string bindItem(pdfSummaryViewModel pdfSummary, List<pdfItemViewModel> pdfItem, string Layout, Status InPrintCode, string type)
        {
            decimal? qty = 0;
            decimal? total = 0;
            var count = 1;
            var dvitem = "inactive";

            var str = "";
            decimal wgt = 0;
            decimal cbm = 0;

            decimal Totwgt = 0;
            decimal Totcbm = 0;

            decimal TotTaxableAmount = 0;
            decimal TotTaxAmount = 0;
            decimal GrandTot = 0;
            decimal QtyTot = 0;
            var rtype = "";
            foreach (var item in pdfItem)
            {
                qty += item.ItemQuantity;
                var subtot = item.ItemTotalAmount;
                total += subtot;


                var itSubtotal = item.ItemSubTotal != null ? item.ItemSubTotal : 0;
                var itDiscount = item.ItemDiscount != null ? item.ItemDiscount : 0;
                var itTaxable = itSubtotal - itDiscount;
                decimal? TaxableAmount = (Layout != "Scaffold") ? item.ItemSubTotal : itTaxable;

                TotTaxAmount += rtype != "bundle" ? (decimal)item.ItemTaxAmount : 0;
                TotTaxableAmount += rtype != "bundle" ? (decimal)TaxableAmount : 0;
                GrandTot += rtype != "bundle" ? (decimal)item.ItemTotalAmount : 0;
                // QtyTot += (rtype != "bundle" && item.KeepStock) ? (decimal)item.ItemQuantity : 0;
                QtyTot += (decimal)item.ItemQuantity;



                str += ItemsBind(item, Layout, dvitem, wgt, cbm, count, InPrintCode, TaxableAmount, type);
                count++;




                // bundle items
                if (item.bundle != null && item.bundle.Count > 0)
                {
                    var countz = 0;
                    foreach (var itemz in item.bundle)
                    {
                        var bcount = countz + 1;
                        str += ItemsBindb(itemz, Layout, dvitem, wgt, cbm, count, InPrintCode, TaxableAmount, "bundle", bcount);
                        countz++;

                        Totwgt += Convert.ToDecimal(itemz.Weight) * Convert.ToDecimal(itemz.ItemQuantity);
                        Totcbm += Convert.ToDecimal(itemz.CBM) * Convert.ToDecimal(itemz.ItemQuantity);
                    };
                }
            }
            if (Layout == "Jewellery")
            {
                str += "<tr id='jwltotal' class='border-top'><td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;' colspan='2'><b>(" + (count - 1) + " items)</b></td><td class='text-center' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b> Total الجمالى</b></td>";
                str += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + qty + "</b></td><td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + qty + "</b></td><td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td><td class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + total + "</b></td></tr>";
            }
            if (Layout == "Scaffold")
            {
                var weihtv = Totwgt != 0 ? String.Format("{0:0.00}", Totwgt) : "";
                var cbmv = Totcbm != 0 ? String.Format("{0:0.00}", Totcbm) : "";
                str += "<tr class='border-top'><td colspan='3' class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>TOTAL</b></td><td class='text-center' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + weihtv + "</b></td><td class='text-center' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + cbmv + "</b></td><td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td><td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td><td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td><td colspan='2'></td><td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td></tr>";
                str += "<tr class='border-top'><td colspan='5' class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>TOTAL</b></td><td class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + QtyTot + "</td><td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td><td class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + TotTaxableAmount + "</td><td colspan='2' class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + TotTaxAmount + "</td><td class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + GrandTot + "</b></td></tr>";
            }

            return str;
        }
        private string ItemsBind(pdfItemViewModel ritem, string Layout, string dvitem, decimal? wgt, decimal? cbm, int count, Status InPrintCode, decimal? TaxableAmount, string rtype = "", long bcount = 0, string type = "")
        {
            var Row = "";
            var unit = (ritem.ItemUnit != null) ? ritem.ItemUnit : "";
            var PartNo = (ritem.PartNumber != null && ritem.PartNumber != "") ? ritem.PartNumber : "";
            var itemnote = "";
            var MakeIn = db.EnableSettings.Where(a => a.EnableType == "MakeInTrans").FirstOrDefault();
            var Make = MakeIn != null ? MakeIn.Status : Status.inactive;

            if (ritem.ItemNote != "" && ritem.ItemNote != "-:{Bundle_Item}")
            {
                itemnote = "<br /><small>" + ritem.ItemNote + "</small>";
            }
            if (rtype == "Quote" && ritem.InSaleInvoice == true && ritem.ItemNote != "" && ritem.ItemNote != "-:{Bundle_Item}")
            {
                itemnote = "<small>" + ritem.ItemNote + "</small>";
            }
            var dvField1 = "";
            var dvField2 = "";
            var trcount = count;
            var bold = (Layout != "Scaffold") ? "<b>" : "";
            var boldend = (Layout != "Scaffold") ? "</b>" : "";
            if (dvitem != "active" && rtype != "bundle")
            {
                dvField1 += "<td class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemUnitPrice + "</td>";
                //if(type != "Stock Transfer")
                //{
                dvField1 += "<td class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + TaxableAmount + "</td>";
                dvField2 += "<td class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemTaxAmount + "</td>";

                //}
                dvField2 += "<td class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemTotalAmount + "</td>";
            }

            Row += (Layout != "Scaffold") ? "<tr class='noborder'>" : "<tr class='border-top'>";
            Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + trcount + "</td>";
            if (ritem.PNoStatus == 0)
            {
                // $("#PoNo").show();
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + PartNo + "</td>";
            }
            var itemcode = "";
            // Default Invoice Structure
            if (Layout == "Default" || Layout == "NewDefault" || Layout == "General")
            {
                if (InPrintCode == 0)
                {
                    itemcode = ritem.ItemCode + " - ";
                }

                var itemdetail = "";
                if (Layout == "NewDefault")
                {
                    if (ritem.BomExist == true && itemnote != null)
                    {
                        itemdetail = itemnote;
                    }
                    else
                    {
                        itemdetail = itemcode + ritem.ItemName;
                    }
                }
                else
                {
                    if (rtype == "Quote" && ritem.InSaleInvoice == true)
                    {
                        itemdetail = itemcode + itemnote;
                    }
                    else
                    {

                        itemdetail = itemcode + ritem.ItemName + itemnote;

                    }

                }


                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;width:34%;text-align: left;border: .1px solid #ccc;'>" + itemdetail + "</td>";
                if (Make == Status.active)// && type != "Stock Transfer")
                {
                    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align:left;border: .1px solid #ccc;'>" + ritem.Make + "</td>";
                }

                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + unit + "</td>";
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemQuantity + "</td>";
                Row += dvField1 + dvField2;
            }
            else if (Layout == "Jewellery")
            {
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemCode + "</td>";
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;'>" + ritem.ItemName + itemnote + "</td>";
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemQuantity + "</td>";
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemQuantity + "</td>";
                //Row += dvField1;
                Row += "<td class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + bold + TaxableAmount + boldend + "</td>";
            }
            else if (Layout == "Scaffold")
            {
                var cbm1 = ritem.CBM != null ? Convert.ToDecimal(ritem.CBM) : 0;
                var CBM = cbm1 * ritem.ItemQuantity;
                var weigh1 = ritem.Weight != null ? Convert.ToDecimal(ritem.Weight) : 0;
                var Weight = weigh1 * ritem.ItemQuantity;
                var img = "";
                wgt = wgt + Weight;
                cbm = cbm + CBM;

                if (ritem.img != null && ritem.img.Count > 0)
                {
                    foreach (var itemimg in ritem.img)
                    {
                        var im = "/uploads/itemimages/" + ritem.Id + "/thumb_" + itemimg.FileName;
                        img = "<img width='68' height='46' src='" + LegacyWeb.MapPath("/uploads/itemimages/" + ritem.Id + "/thumb_" + itemimg.FileName) + "' />";
                    }
                }

                var itnamecols = (Weight == 0) ? ((CBM == 0) ? 3 : 2) : 1;
                //  console.log("weight :"+Weight+" CBM:"+CBM+" cols:"+itnamecols);
                if (img == "")
                {
                    itnamecols++;
                }
                //  console.log(" IMG:" + img + " cols:" + itnamecols);
                if (rtype != "bundle")
                {
                    if (ritem.ItemDescription != "" && ritem.ItemDescription != null)
                    {
                        itemnote += "<br /><small>" + ritem.ItemDescription + "</small>";
                    }
                    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;' colspan='" + itnamecols + "'><b>" + ritem.ItemName + "</b>" + itemnote + "</td>";
                }

                if (img != "")
                {
                    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;' style='width:70px; padding:1px;'>" + img + "</td>";
                }
                //if (Make == Status.active)
                //{
                //    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.Make + "</td>";
                //}

                if (Weight != 0)
                {
                    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b" + Weight + "</b></td>";
                    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + CBM + "</b></td>";
                }
                if (Weight == 0 && CBM != 0)
                {
                    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + CBM + "</b></td>";
                }
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + ritem.ItemQuantity + " " + unit + "</b></td>";


                Row += dvField1;


                Row += "<td class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemTax + "</td>";

                Row += dvField2;

            }
            Row += "</tr>";
            return Row;
        }

        private string ItemsBindb(pdfBundleViewModel ritem, string Layout, string dvitem, decimal? wgt, decimal? cbm, int count, Status InPrintCode, decimal? TaxableAmount, string rtype = "", long bcount = 0)
        {

            var Row = "";
            var unit = (ritem.ItemUnit != null) ? ritem.ItemUnit : "";
            var PartNo = (ritem.PartNumber != null && ritem.PartNumber != "") ? ritem.PartNumber : "";
            var itemnote = "";
            if (ritem.ItemNote != "" && ritem.ItemNote != "-:{Bundle_Item}")
            {
                itemnote = "<br /><small>" + ritem.ItemNote + "</small>";
            }
            var dvField1 = "";
            var dvField2 = "";
            var trcount = bcount;
            var bold = (Layout != "Scaffold") ? "<b>" : "";
            var boldend = (Layout != "Scaffold") ? "</b>" : "";

            if (dvitem != "active" && rtype == "bundle")
            {
                dvField1 += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td><td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td>";
                dvField2 += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td><td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td>";
            }
            Row += (Layout != "Scaffold") ? "<tr class='noborder'>" : "<tr class='border-top'>";
            Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + trcount + "</td>";
            if (ritem.PNoStatus == 0)
            {
                // $("#PoNo").show();
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + PartNo + "</td>";
            }
            var itemcode = "";
            // Default Invoice Structure
            if (Layout == "Default" || Layout == "General")
            {
                if (InPrintCode == 0)
                {
                    itemcode = ritem.ItemCode + " - ";
                }
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;'>" + itemcode + ritem.ItemName + itemnote + "</td>";
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + unit + "</td>";
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemQuantity + "</td>";
                Row += dvField1 + dvField2;
            }
            else if (Layout == "Jewellery")
            {
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemCode + "</td>";
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;'>" + ritem.ItemName + itemnote + "</td>";
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemQuantity + "</td>";
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemQuantity + "</td>";
                //Row += dvField1;
                Row += "<td class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + bold + TaxableAmount + boldend + "</td>";

            }
            else if (Layout == "Scaffold")
            {
                var cbm1 = ritem.CBM != null ? Convert.ToDecimal(ritem.CBM) : 0;
                var CBM = cbm1 * ritem.ItemQuantity;
                var weigh1 = ritem.Weight != null ? Convert.ToDecimal(ritem.Weight) : 0;
                var Weight = weigh1 * ritem.ItemQuantity;
                var img = "";
                wgt = wgt + Weight;
                cbm = cbm + CBM;

                if (ritem.img != null && ritem.img.Count > 0)
                {
                    foreach (var itemimg in ritem.img)
                    {
                        var im = "/uploads/itemimages/" + ritem.Id + "/thumb_" + itemimg.FileName;
                        img = "<img width='68' height='46' src='" + LegacyWeb.MapPath("/uploads/itemimages/" + ritem.Id + "/thumb_" + itemimg.FileName) + "' />";
                    }
                }

                var itnamecols = (Weight == 0) ? ((CBM == 0) ? 3 : 2) : 1;
                //  console.log("weight :"+Weight+" CBM:"+CBM+" cols:"+itnamecols);
                if (img == "")
                {
                    itnamecols++;
                }
                //  console.log(" IMG:" + img + " cols:" + itnamecols);

                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;' colspan='" + itnamecols + "'><i style='color: #747474 !important;'>" + ritem.ItemName + itemnote + "</i></td>";

                if (img != "")
                {
                    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;' style='width:70px; padding:1px;'>" + img + "</td>";
                }

                if (Weight != 0)
                {
                    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + String.Format("{0:0.00}", Weight) + "</td>";
                    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + String.Format("{0:0.00}", CBM) + "</td>";
                }
                if (Weight == 0 && CBM != 0)
                {
                    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + String.Format("{0:0.00}", CBM) + "</td>";
                }
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + String.Format("{0:0.00}", ritem.ItemQuantity) + " " + unit + "</td>";

                Row += dvField1;
                if (dvitem != "active" && rtype == "bundle")
                {
                    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td>";
                }
                else
                {
                    Row += "<td class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemTax + "</td>";
                }
                Row += dvField2;

            }
            Row += "</tr>";
            return Row;
        }

        private string bindItems(pdfSummaryViewModel pdfSummary, List<pdfItemViewModel> pdfItem, string Layout, Status InPrintCode, string type)
        {
            decimal? qty = 0;
            decimal? total = 0;
            var count = 1;
            var dvitem = "inactive";

            var str = "";
            decimal wgt = 0;
            decimal cbm = 0;

            decimal Totwgt = 0;
            decimal Totcbm = 0;

            decimal TotTaxableAmount = 0;
            decimal TotTaxAmount = 0;
            decimal GrandTot = 0;
            decimal QtyTot = 0;
            var rtype = "";
            foreach (var item in pdfItem)
            {
                qty += item.ItemQuantity;
                var subtot = item.ItemTotalAmount;
                total += subtot;


                var itSubtotal = item.ItemSubTotal != null ? item.ItemSubTotal : 0;
                var itDiscount = item.ItemDiscount != null ? item.ItemDiscount : 0;
                var itTaxable = itSubtotal - itDiscount;
                decimal? TaxableAmount = (Layout != "Scaffold") ? item.ItemSubTotal : itTaxable;

                TotTaxAmount += rtype != "bundle" ? (decimal)item.ItemTaxAmount : 0;
                TotTaxableAmount += rtype != "bundle" ? (decimal)TaxableAmount : 0;
                GrandTot += rtype != "bundle" ? (decimal)item.ItemTotalAmount : 0;
                // QtyTot += (rtype != "bundle" && item.KeepStock) ? (decimal)item.ItemQuantity : 0;
                QtyTot += (decimal)item.ItemQuantity;



                str += ItemsBinds(item, Layout, dvitem, wgt, cbm, count, InPrintCode, TaxableAmount, type);
                count++;




                // bundle items
                if (item.bundle != null && item.bundle.Count > 0)
                {
                    var countz = 0;
                    foreach (var itemz in item.bundle)
                    {
                        var bcount = countz + 1;
                        str += ItemsBindbs(itemz, Layout, dvitem, wgt, cbm, count, InPrintCode, TaxableAmount, "bundle", bcount);
                        countz++;

                        Totwgt += Convert.ToDecimal(itemz.Weight) * Convert.ToDecimal(itemz.ItemQuantity);
                        Totcbm += Convert.ToDecimal(itemz.CBM) * Convert.ToDecimal(itemz.ItemQuantity);
                    };
                }
            }
            if (Layout == "Jewellery")
            {
                str += "<tr id='jwltotal' class='border-top'><td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;' colspan='2'><b>(" + (count - 1) + " items)</b></td><td class='text-center' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b> Total الجمالى</b></td>";
                str += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + qty + "</b></td><td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + qty + "</b></td><td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td><td class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + total + "</b></td></tr>";
            }
            if (Layout == "Scaffold")
            {
                var weihtv = Totwgt != 0 ? String.Format("{0:0.00}", Totwgt) : "";
                var cbmv = Totcbm != 0 ? String.Format("{0:0.00}", Totcbm) : "";
                str += "<tr class='border-top'><td colspan='3' class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>TOTAL</b></td><td class='text-center' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + weihtv + "</b></td><td class='text-center' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + cbmv + "</b></td><td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td><td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td><td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td><td colspan='2'></td><td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td></tr>";
                str += "<tr class='border-top'><td colspan='5' class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>TOTAL</b></td><td class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + QtyTot + "</td><td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td><td class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + TotTaxableAmount + "</td><td colspan='2' class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + TotTaxAmount + "</td><td class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + GrandTot + "</b></td></tr>";
            }

            return str;
        }
        public string GetAllSerialNos2(string SalesEntryId, long ItemId)
        {
            var tempcon = Convert.ToInt32(SalesEntryId);
            var ItemList = (from a in db.SalesEntrys
                            join b in db.BatchStocks
                            on a.SalesEntryId equals b.Reference
                            where (a.SalesEntryId == tempcon && b.Item == ItemId && b.Type == "Sales")
                            select new
                            {
                                BatchNo = b.BatchNo
                            }).ToList();

            var j = 0;
            string SlNos = "";

            //Appending SerialNo.s
            for (j = 0; j < ItemList.Count; j++)
            {
                SlNos += ItemList[j].BatchNo + ", ";
            }

            return SlNos;
        }

        private string ItemsBinds(pdfItemViewModel ritem, string Layout, string dvitem, decimal? wgt, decimal? cbm, int count, Status InPrintCode, decimal? TaxableAmount, string rtype = "", long bcount = 0, string type = "")
        {
            var Row = "";
            var unit = (ritem.ItemUnit != null) ? ritem.ItemUnit : "";
            var PartNo = (ritem.PartNumber != null && ritem.PartNumber != "") ? ritem.PartNumber : "";
            var itemnote = "";
            var MakeIn = db.EnableSettings.Where(a => a.EnableType == "MakeInTrans").FirstOrDefault();
            var Make = MakeIn != null ? MakeIn.Status : Status.inactive;

            if (ritem.ItemNote != "" && ritem.ItemNote != "-:{Bundle_Item}")
            {
                itemnote = "<br /><small>" + ritem.ItemNote + "</small>";
            }
            if (rtype == "Quote" && ritem.InSaleInvoice == true && ritem.ItemNote != "" && ritem.ItemNote != "-:{Bundle_Item}")
            {
                itemnote = "<small>" + ritem.ItemNote + "</small>";
            }
            var dvField1 = "";
            var dvField2 = "";
            var trcount = count;
            var bold = (Layout != "Scaffold") ? "<b>" : "";
            var boldend = (Layout != "Scaffold") ? "</b>" : "";
            if (dvitem != "active" && rtype != "bundle")
            {
                dvField1 += "<td class='text-right' style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemUnitPrice + "</td>";
                //if(type != "Stock Transfer")
                //{
                dvField1 += "<td class='text-right' style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + TaxableAmount + "</td>";
                dvField2 += "<td class='text-right' style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemTaxAmount + "</td>";

                //}
                dvField2 += "<td class='text-right' style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemTotalAmount + "</td>";
            }

            Row += (Layout != "Scaffold") ? "<tr class='noborder'>" : "<tr class='border-top'>";
            Row += "<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;'>" + trcount + "</td>";
            if (ritem.PNoStatus == 0)
            {
                // $("#PoNo").show();
                Row += "<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;'>" + PartNo + "</td>";
            }
            var itemcode = "";
            // Default Invoice Structure
            if (Layout == "Default" || Layout == "NewDefault" || Layout == "General")
            {
                if (InPrintCode == 0)
                {
                    itemcode = ritem.ItemCode + " - ";
                }

                var itemdetail = "";
                if (Layout == "NewDefault")
                {
                    if (ritem.BomExist == true && itemnote != null)
                    {
                        itemdetail = itemnote;
                    }
                    else
                    {
                        itemdetail = itemcode + ritem.ItemName;
                    }
                }
                else
                {
                    if (rtype == "Quote" && ritem.InSaleInvoice == true)
                    {
                        itemdetail = itemcode + itemnote;
                    }
                    else
                    {

                        var rt = GetAllSerialNos2(ritem.BatchNo, ritem.Id);
                        if (rt != "" && rt != null)
                        {
                            //itemdetail = "<span>" + ritem.ItemName + " <br />" + "S/N : " + rt + "</span>" + itemnote;
                            itemdetail = itemcode + ritem.ItemName + itemnote + " <br /> < span >" + "S/N : " + rt + "</span>";
                        }
                        else
                        {
                            //itemdetail = "<span>" + itemcode + ritem.ItemName + "</span>" + itemnote;
                            itemdetail = itemcode + ritem.ItemName + itemnote;
                        }
                        //itemdetail = itemcode + ritem.ItemName + itemnote+ "<br /><span>hi</span>";



                    }

                }


                Row += "<td style='font-size: 12px;padding: 6px;vertical-align: top;width:34%;text-align: left;border: .1px solid #ccc;'>" + itemdetail + "</td>";
                if (Make == Status.active)// && type != "Stock Transfer")
                {
                    Row += "<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align:left;border: .1px solid #ccc;'>" + ritem.Make + "</td>";
                }

                Row += "<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + unit + "</td>";
                Row += "<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemQuantity + "</td>";
                Row += dvField1 + dvField2;
            }
            else if (Layout == "Jewellery")
            {
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemCode + "</td>";
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;'>" + ritem.ItemName + itemnote + "</td>";
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemQuantity + "</td>";
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemQuantity + "</td>";
                //Row += dvField1;
                Row += "<td class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + bold + TaxableAmount + boldend + "</td>";
            }
            else if (Layout == "Scaffold")
            {
                var cbm1 = ritem.CBM != null ? Convert.ToDecimal(ritem.CBM) : 0;
                var CBM = cbm1 * ritem.ItemQuantity;
                var weigh1 = ritem.Weight != null ? Convert.ToDecimal(ritem.Weight) : 0;
                var Weight = weigh1 * ritem.ItemQuantity;
                var img = "";
                wgt = wgt + Weight;
                cbm = cbm + CBM;

                if (ritem.img != null && ritem.img.Count > 0)
                {
                    foreach (var itemimg in ritem.img)
                    {
                        var im = "/uploads/itemimages/" + ritem.Id + "/thumb_" + itemimg.FileName;
                        img = "<img width='68' height='46' src='" + LegacyWeb.MapPath("/uploads/itemimages/" + ritem.Id + "/thumb_" + itemimg.FileName) + "' />";
                    }
                }

                var itnamecols = (Weight == 0) ? ((CBM == 0) ? 3 : 2) : 1;
                //  console.log("weight :"+Weight+" CBM:"+CBM+" cols:"+itnamecols);
                if (img == "")
                {
                    itnamecols++;
                }
                //  console.log(" IMG:" + img + " cols:" + itnamecols);
                if (rtype != "bundle")
                {
                    if (ritem.ItemDescription != "" && ritem.ItemDescription != null)
                    {
                        itemnote += "<br /><small>" + ritem.ItemDescription + "</small>";
                    }
                    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;' colspan='" + itnamecols + "'><b>" + ritem.ItemName + "</b>" + itemnote + "</td>";
                }

                if (img != "")
                {
                    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;' style='width:70px; padding:1px;'>" + img + "</td>";
                }
                //if (Make == Status.active)
                //{
                //    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.Make + "</td>";
                //}

                if (Weight != 0)
                {
                    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b" + Weight + "</b></td>";
                    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + CBM + "</b></td>";
                }
                if (Weight == 0 && CBM != 0)
                {
                    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'><b>" + CBM + "</b></td>";
                }
                Row += "<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;'><b>" + ritem.ItemQuantity + " " + unit + "</b></td>";


                Row += dvField1;


                Row += "<td class='text-right' style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;'>" + ritem.ItemTax + "</td>";

                Row += dvField2;

            }
            Row += "</tr>";
            return Row;
        }

        private string ItemsBindbs(pdfBundleViewModel ritem, string Layout, string dvitem, decimal? wgt, decimal? cbm, int count, Status InPrintCode, decimal? TaxableAmount, string rtype = "", long bcount = 0)
        {

            var Row = "";
            var unit = (ritem.ItemUnit != null) ? ritem.ItemUnit : "";
            var PartNo = (ritem.PartNumber != null && ritem.PartNumber != "") ? ritem.PartNumber : "";
            var itemnote = "";
            if (ritem.ItemNote != "" && ritem.ItemNote != "-:{Bundle_Item}")
            {
                itemnote = "<br /><small>" + ritem.ItemNote + "</small>";
            }
            var dvField1 = "";
            var dvField2 = "";
            var trcount = bcount;
            var bold = (Layout != "Scaffold") ? "<b>" : "";
            var boldend = (Layout != "Scaffold") ? "</b>" : "";

            if (dvitem != "active" && rtype == "bundle")
            {
                dvField1 += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td><td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td>";
                dvField2 += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td><td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td>";
            }
            Row += (Layout != "Scaffold") ? "<tr class='noborder'>" : "<tr class='border-top'>";
            Row += "<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;'>" + trcount + "</td>";
            if (ritem.PNoStatus == 0)
            {
                // $("#PoNo").show();
                Row += "<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;'>" + PartNo + "</td>";
            }
            var itemcode = "";
            // Default Invoice Structure
            if (Layout == "Default" || Layout == "General")
            {
                if (InPrintCode == 0)
                {
                    itemcode = ritem.ItemCode + " - ";
                }
                Row += "<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;'>" + itemcode + ritem.ItemName + itemnote + "</td>";
                Row += "<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;'>" + unit + "</td>";
                Row += "<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;'>" + ritem.ItemQuantity + "</td>";
                Row += dvField1 + dvField2;
            }
            else if (Layout == "Jewellery")
            {
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemCode + "</td>";
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;'>" + ritem.ItemName + itemnote + "</td>";
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemQuantity + "</td>";
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + ritem.ItemQuantity + "</td>";
                //Row += dvField1;
                Row += "<td class='text-right' style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + bold + TaxableAmount + boldend + "</td>";

            }
            else if (Layout == "Scaffold")
            {
                var cbm1 = ritem.CBM != null ? Convert.ToDecimal(ritem.CBM) : 0;
                var CBM = cbm1 * ritem.ItemQuantity;
                var weigh1 = ritem.Weight != null ? Convert.ToDecimal(ritem.Weight) : 0;
                var Weight = weigh1 * ritem.ItemQuantity;
                var img = "";
                wgt = wgt + Weight;
                cbm = cbm + CBM;

                if (ritem.img != null && ritem.img.Count > 0)
                {
                    foreach (var itemimg in ritem.img)
                    {
                        var im = "/uploads/itemimages/" + ritem.Id + "/thumb_" + itemimg.FileName;
                        img = "<img width='68' height='46' src='" + LegacyWeb.MapPath("/uploads/itemimages/" + ritem.Id + "/thumb_" + itemimg.FileName) + "' />";
                    }
                }

                var itnamecols = (Weight == 0) ? ((CBM == 0) ? 3 : 2) : 1;
                //  console.log("weight :"+Weight+" CBM:"+CBM+" cols:"+itnamecols);
                if (img == "")
                {
                    itnamecols++;
                }
                //  console.log(" IMG:" + img + " cols:" + itnamecols);

                Row += "<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;' colspan='" + itnamecols + "'><i style='color: #747474 !important;'>" + ritem.ItemName + itemnote + "</i></td>";

                if (img != "")
                {
                    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;' style='width:70px; padding:1px;'>" + img + "</td>";
                }

                if (Weight != 0)
                {
                    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + String.Format("{0:0.00}", Weight) + "</td>";
                    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + String.Format("{0:0.00}", CBM) + "</td>";
                }
                if (Weight == 0 && CBM != 0)
                {
                    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + String.Format("{0:0.00}", CBM) + "</td>";
                }
                Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + String.Format("{0:0.00}", ritem.ItemQuantity) + " " + unit + "</td>";

                Row += dvField1;
                if (dvitem != "active" && rtype == "bundle")
                {
                    Row += "<td style='font-size: 12px;border:.5px #000000;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'></td>";
                }
                else
                {
                    Row += "<td class='text-right' style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;'>" + ritem.ItemTax + "</td>";
                }
                Row += dvField2;

            }
            Row += "</tr>";
            return Row;
        }


        private string bindSundry(List<pdfBillSundryViewModel> billsundry, string layName)
        {
            string str = "";
            if (layName == "Scaffold")
            {
                foreach (var bilsun in billsundry)
                {
                    str += "<tr class='border-top'><td colspan='5'></td><td class='text-right' style:'font-size:15px;'>" + bilsun.BillSundry + "</td><td class='text-center' style:'font-size: 12px;'>" + bilsun.BsAmount + "</td></tr>";
                }
            }
            else
            {
                foreach (var bilsun in billsundry)
                {
                    str += "<tr class='border-top'>";
                    str += "<td style='border: .1px solid #ccc;padding: 5px;font-size:13px;'>" + bilsun.BillSundry + "</td>";
                    str += "<td style='border: .1px solid #ccc;padding: 5px;font-size:13px;' class='text-right'>" + Convert.ToDecimal(bilsun.BsAmount).ToString() + "</td>";
                    str += "</tr>";
                }
            }
            return str;
        }
        public List<ConvertTransactions> ExtNum(long id, List<ConvertTransactions> ExtList)
        {
            ConvertTransactions Ext = db.ConvertTransactionss.Where(x => x.ConvertFrom == "SaleExtend" && x.To == id).FirstOrDefault();
            if (Ext != null)
            {
                ExtList.Add(Ext);
                ExtNum(Ext.From, ExtList);
            }
            return ExtList;
        }
        #endregion

        #region Batch Stock
        // add Batch Stock
        public int addBatchStock(DateTime? MFG, DateTime? EXP, long? Item, long? Reference, string Type, DateTime? Date, DateTime? CreatedDate, decimal Cost, string BatchNo = null, decimal StockIn = 0, decimal StockOut = 0)
        {
            var batch = new BatchStock
            {
                BatchNo = BatchNo,
                MFG = (DateTime)MFG,
                EXP = (DateTime)EXP,
                StockIn = StockIn,
                StockOut = StockOut,
                Item = (long)Item,
                Reference = (long)Reference,
                Type = Type,
                Date = (DateTime)Date,
                Cost = Cost,
                CreatedDate = Convert.ToDateTime(System.DateTime.Now),
            };
            db.BatchStocks.Add(batch);
            return db.SaveChanges();
        }

        // delete Batch Stock
        public bool DeleteBatchStock(long? Item, long Reference, string Type)
        {
            db.BatchStocks.RemoveRange(db.BatchStocks.Where(a => a.Item == Item && a.Reference == Reference && a.Type == Type));
            int delete = db.SaveChanges();
            if (delete != 0)
                return true;
            else
                return false;
        }
        #endregion

        #region check Approved
        public bool chkApproved(long EntryId, bool EditPermission, string Type, string user)
        {
            var empid = db.Employees.Where(a => a.UserStatus == true && a.UserId == user).Select(a => a.EmployeeId).FirstOrDefault();
            var chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == EntryId && x.Type == Type).GroupBy(l => l.ApprovedBy)
                                       .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                       .ToList().Select(x => x.ApprovalStatus).ToList().ToArray();


            ApprovalStatus appstat = (ApprovalStatus)1;
            var app = db.Approvals.Where(x => x.TransEntry == EntryId && x.Type == Type).Select(x => x.EmployeeId).ToList();
            var AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == EntryId && x.Type == Type).Select(x => x.ApprovalStatus).ToList();
            var ApprovalStatus = (app.Count > 0 && AppStatus.Count > 0 && chkAppStatus.Length > 0) ? (chkAppStatus.Contains(appstat) ? "Rejected" : (app != null && chkAppStatus != null && app.Count != 0 && chkAppStatus.Length != 0 && chkAppStatus.Count() == app.Count() ? "Approved" : "PendingApproval")) : "PendingApproval";

            var Ret = (AppStatus.Count == 0 || EditPermission == true || (ApprovalStatus == "Approved" && app.Contains(empid))) ? true : false;
            return Ret;
        }
        public bool chkApproved2(long EntryId, bool EditPermission, string Type, string user)
        {
            var empid = db.Employees.Where(a => a.UserStatus == true && a.UserId == user).Select(a => a.EmployeeId).FirstOrDefault();
            var chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == EntryId && x.Type == Type).GroupBy(l => l.ApprovedBy)
                                       .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                       .ToList().Select(x => x.ApprovalStatus).ToList().ToArray();


            ApprovalStatus appstat = (ApprovalStatus)1;
            var app = db.Approvals.Where(x => x.TransEntry == EntryId && x.Type == Type).Select(x => x.EmployeeId).ToList();
            var AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == EntryId && x.Type == Type).Select(x => x.ApprovalStatus).ToList();
            var ApprovalStatus = (app.Count > 0 && AppStatus.Count > 0 && chkAppStatus.Length > 0) ? (chkAppStatus.Contains(appstat) ? "Rejected" : (app != null && chkAppStatus != null && app.Count != 0 && chkAppStatus.Length != 0 && chkAppStatus.Count() == app.Count() ? "Approved" : "PendingApproval")) : "PendingApproval";

            var Ret = (ApprovalStatus == "Approved") ? true : false;
            return Ret;
        }
        #endregion
        #region check ApprovedBy
        public bool chkApprovedBy(long EntryId, string Type, string user)
        {
            var empid = db.Employees.Where(a => a.UserStatus == true && a.UserId == user).Select(a => a.EmployeeId).FirstOrDefault();
            var chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == EntryId && x.Type == Type).GroupBy(l => l.ApprovedBy)
                                       .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                       .ToList().Select(x => x.ApprovalStatus).ToList().ToArray();


            ApprovalStatus appstat = (ApprovalStatus)1;
            var app = db.Approvals.Where(x => x.TransEntry == EntryId && x.Type == Type).Select(x => x.EmployeeId).ToList();
            var AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == EntryId && x.Type == Type).Select(x => x.ApprovalStatus).ToList();
            var ApprovalStatus = (app.Count > 0 && AppStatus.Count > 0 && chkAppStatus.Length > 0) ? (chkAppStatus.Contains(appstat) ? "Rejected" : (app != null && chkAppStatus != null && app.Count != 0 && chkAppStatus.Length != 0 && chkAppStatus.Count() == app.Count() ? "Approved" : "PendingApproval")) : "PendingApproval";

            var Ret = (ApprovalStatus == "Approved") ? true : false;
            return Ret;
        }
        #endregion

        #region Property image and document
        public bool PropImages(PropertyViewModel ProdViewModel, long ItemID)
        {
            if (ProdViewModel.ItemImage != null)
            {
                var ItemImg = db.PropertyImages.Where(a => a.PropertyID == ItemID).FirstOrDefault();
                if (ItemImg != null)
                {
                    db.PropertyImages.RemoveRange(db.PropertyImages.Where(a => a.PropertyID == ItemID));
                    string storePath = LegacyWeb.MapPath("~/uploads/propertyimages/" + ItemID);
                    if (Directory.Exists(storePath))
                        Directory.Delete(storePath, true);
                }
            }

            int flag = 0;
            foreach (IFormFile file in ProdViewModel.ItemImage)
            {
                //Checking file is available to save.  
                if (file != null)
                {
                    var ProdImg = new PropertyImage
                    {
                        PropertyID = ItemID,
                        FileName = Path.GetFileName(file.FileName),
                        Status = 1
                    };
                    db.PropertyImages.Add(ProdImg);
                    db.SaveChanges();
                    string storePath = LegacyWeb.MapPath("~/uploads/propertyimages/" + ItemID);
                    if (!Directory.Exists(storePath))
                        Directory.CreateDirectory(storePath);
                    var InputFileName = Path.GetFileName(file.FileName);
                    var ServerSavePath = Path.Combine(storePath + "/" + InputFileName);
                    //Save file to server folder  
                    file.SaveAs(ServerSavePath);
                    flag = 1;


                    String noextension = Path.GetFileNameWithoutExtension(InputFileName);
                    String extension = Path.GetExtension(InputFileName);
                    //string date = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                    String newName = noextension + extension;
                    var thumbName = "";
                    if (extension == ".jpg" || extension == ".png" || extension == ".jpeg")
                    {
                        thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                        thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/propertyimages/" + ItemID), thumbName);

                        Image img = Image.FromFile(ServerSavePath);
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
                    }

                }
            }

            if (flag == 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }
        public bool PropDocument(PropertyViewModel ProdViewModel, long ItemID)
        {
            var ItemDoc = db.PropertyDocuments.Where(a => a.PropertyID == ItemID).FirstOrDefault();
            if (ItemDoc != null)
            {
                db.PropertyDocuments.RemoveRange(db.PropertyDocuments.Where(a => a.PropertyID == ItemID));
                string storePath = LegacyWeb.MapPath("~/uploads/propertydocuments/" + ItemID);
                if (Directory.Exists(storePath))
                    Directory.Delete(storePath, true);
            }

            int flag = 0;
            foreach (IFormFile file in ProdViewModel.ItemDocument)
            {
                //Checking file is available to save.  
                if (file != null)
                {
                    var ProdDocument = new PropertyDocument
                    {
                        PropertyID = ItemID,
                        FileName = Path.GetFileName(file.FileName),
                        Status = 1
                    };
                    db.PropertyDocuments.Add(ProdDocument);
                    db.SaveChanges();

                    string storePath = LegacyWeb.MapPath("~/uploads/propertydocuments/" + ItemID);
                    if (!Directory.Exists(storePath))
                        Directory.CreateDirectory(storePath);
                    var InputFileName = Path.GetFileName(file.FileName);
                    var ServerSavePath = Path.Combine(storePath + "/" + InputFileName);
                    //Save file to server folder  
                    file.SaveAs(ServerSavePath);
                    flag = 1;
                }
            }
            if (flag == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public bool PropUnitImages(PropertyUnitViewModel ProdViewModel, long ItemID)
        {
            if (ProdViewModel.ItemImage != null)
            {
                var ItemImg = db.PropertyUnitImages.Where(a => a.UnitID == ItemID).FirstOrDefault();
                if (ItemImg != null)
                {
                    db.PropertyUnitImages.RemoveRange(db.PropertyUnitImages.Where(a => a.UnitID == ItemID));
                    string storePath = LegacyWeb.MapPath("~/uploads/propertyunitimages/" + ItemID);
                    if (Directory.Exists(storePath))
                        Directory.Delete(storePath, true);
                }
            }

            int flag = 0;
            foreach (IFormFile file in ProdViewModel.ItemImage)
            {
                //Checking file is available to save.  
                if (file != null)
                {
                    var ProdImg = new PropertyUnitImage
                    {
                        UnitID = ItemID,
                        FileName = Path.GetFileName(file.FileName),
                        Status = 1
                    };
                    db.PropertyUnitImages.Add(ProdImg);
                    db.SaveChanges();
                    string storePath = LegacyWeb.MapPath("~/uploads/propertyunitimages/" + ItemID);
                    if (!Directory.Exists(storePath))
                        Directory.CreateDirectory(storePath);
                    var InputFileName = Path.GetFileName(file.FileName);
                    var ServerSavePath = Path.Combine(storePath + "/" + InputFileName);
                    //Save file to server folder  
                    file.SaveAs(ServerSavePath);
                    flag = 1;


                    String noextension = Path.GetFileNameWithoutExtension(InputFileName);
                    String extension = Path.GetExtension(InputFileName);
                    //string date = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                    String newName = noextension + extension;
                    var thumbName = "";
                    if (extension == ".jpg" || extension == ".png" || extension == ".jpeg")
                    {
                        thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                        thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/propertyunitimages/" + ItemID), thumbName);

                        Image img = Image.FromFile(ServerSavePath);
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
                    }

                }
            }

            if (flag == 0)
            {
                return false;
            }
            else
            {
                return true;
            }

        }
        public bool PropUnitDocument(PropertyUnitViewModel ProdViewModel, long ItemID)
        {
            var ItemDoc = db.PropertyUnitDocuments.Where(a => a.UnitID == ItemID).FirstOrDefault();
            if (ItemDoc != null)
            {
                db.PropertyUnitDocuments.RemoveRange(db.PropertyUnitDocuments.Where(a => a.UnitID == ItemID));
                string storePath = LegacyWeb.MapPath("~/uploads/propertyunitdocuments/" + ItemID);
                if (Directory.Exists(storePath))
                    Directory.Delete(storePath, true);
            }

            int flag = 0;
            foreach (IFormFile file in ProdViewModel.ItemDocument)
            {
                //Checking file is available to save.  
                if (file != null)
                {
                    var ProdDocument = new PropertyUnitDocument
                    {
                        UnitID = ItemID,
                        FileName = Path.GetFileName(file.FileName),
                        Status = 1
                    };
                    db.PropertyUnitDocuments.Add(ProdDocument);
                    db.SaveChanges();

                    string storePath = LegacyWeb.MapPath("~/uploads/propertyunitdocuments/" + ItemID);
                    if (!Directory.Exists(storePath))
                        Directory.CreateDirectory(storePath);
                    var InputFileName = Path.GetFileName(file.FileName);
                    var ServerSavePath = Path.Combine(storePath + "/" + InputFileName);
                    //Save file to server folder  
                    file.SaveAs(ServerSavePath);
                    flag = 1;
                }
            }
            if (flag == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool ContractDocument(TenancyContractViewModel ProdViewModel, long ItemID)
        {
            var ItemDoc = db.PropertyDocuments.Where(a => a.PropertyID == ItemID).FirstOrDefault();
            if (ItemDoc != null)
            {
                db.ContractDocuments.RemoveRange(db.ContractDocuments.Where(a => a.Tenancy == ItemID));
                string storePath = LegacyWeb.MapPath("~/uploads/contractdocuments/" + ItemID);
                if (Directory.Exists(storePath))
                    Directory.Delete(storePath, true);
            }

            int flag = 0;
            foreach (IFormFile file in ProdViewModel.ItemDocument)
            {
                //Checking file is available to save.  
                if (file != null)
                {
                    var ProdDocument = new ContractDocument
                    {
                        Tenancy = ItemID,
                        FileName = Path.GetFileName(file.FileName),
                        Status = 1
                    };
                    db.ContractDocuments.Add(ProdDocument);
                    db.SaveChanges();

                    string storePath = LegacyWeb.MapPath("~/uploads/contractdocuments/" + ItemID);
                    if (!Directory.Exists(storePath))
                        Directory.CreateDirectory(storePath);
                    var InputFileName = Path.GetFileName(file.FileName);
                    var ServerSavePath = Path.Combine(storePath + "/" + InputFileName);
                    //Save file to server folder  
                    file.SaveAs(ServerSavePath);
                    flag = 1;
                }
            }
            if (flag == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion


        public static System.Drawing.Image resizeImage(System.Drawing.Image imgToResize, Size size)
        {

            //Get the image current width

            int sourceWidth = imgToResize.Width;

            //Get the image current height

            int sourceHeight = imgToResize.Height;



            float nPercent = 0;

            float nPercentW = 0;

            float nPercentH = 0;

            //Calulate  width with new desired size

            nPercentW = ((float)size.Width / (float)sourceWidth);

            //Calculate height with new desired size

            nPercentH = ((float)size.Height / (float)sourceHeight);



            if (nPercentH < nPercentW)

                nPercent = nPercentH;

            else

                nPercent = nPercentW;

            //New Width

            int destWidth = (int)(sourceWidth * nPercent);

            //New Height

            int destHeight = (int)(sourceHeight * nPercent);



            Bitmap b = new Bitmap(destWidth, destHeight);

            Graphics g = Graphics.FromImage((System.Drawing.Image)b);

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Draw image with new width and height

            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);

            g.Dispose();

            return (System.Drawing.Image)b;

        }


        #region holiday
        public List<HolidayListViewModel> GetDates(DateTime DatMonth, List<string> days, List<Holiday> hday)
        {
            var name = hday.Select(a => a.HolidayName).FirstOrDefault();
            DateTime fdate = hday.Select(a => a.FromDate).FirstOrDefault();
            DateTime tdate = hday.Select(a => a.ToDate).FirstOrDefault();
            List<HolidayListViewModel> hlist = new List<HolidayListViewModel>();

            for (DateTime i = fdate; i <= tdate; i = i.AddDays(1))
            {
                HolidayListViewModel HModel = new HolidayListViewModel();
                HModel.Date = i;
                HModel.HName = name;
                hlist.Add(HModel);
            }

            var firstDayOfMonth = new DateTime(DatMonth.Year, DatMonth.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            DatMonth = firstDayOfMonth;
            while (DatMonth <= lastDayOfMonth)
            {
                HolidayListViewModel HModel = new HolidayListViewModel();
                if (days.Contains(DatMonth.DayOfWeek.ToString()))
                {
                    HModel.Date = DatMonth;
                    HModel.HName = "Weekly Holidays";
                    hlist.Add(HModel);
                }
                DatMonth = DatMonth.AddDays(1.0);
            }
            return hlist;
        }
        #endregion

        #region Item Transaction

        //Function to Update the quantity in Create Mode
        public bool ItemTransInCreateMode(string TransType, long MaterialCenter, long McFrom, long McTo, string[][] array, string UserId, DateTime CurrentDate)
        {
            decimal ConvrtdQty = 0, Quantity = 0;
            long ItemId = 0, UnitId = 0;

            foreach (var arr in array)
            {
                if (TransType != "CreditSaleUsedMaterials" || arr[1] != null)
                {
                    ItemId = Convert.ToInt64(arr[0]);
                    UnitId = Convert.ToInt64(arr[1]);
                    Quantity = Convert.ToDecimal(arr[2]);

                    ConvrtdQty = (from b in db.Items
                                  where (b.ItemID == ItemId)
                                  select (UnitId == b.ItemUnitID) ?
                                      (b.SubUnitId != null) ? (Quantity * b.ConFactor) : Quantity
                                      : Quantity).FirstOrDefault();

                    /******************************** ItemTransaction *******************************/
                    if (TransType == "StockTransfer")
                    {
                        //*******Mc From
                        ItemTransactions McFromItemObj = db.ItemTransactions.Where(a => (a.ItemId == ItemId && a.McId == McFrom)).FirstOrDefault();

                        if (McFromItemObj != null)
                            //Update Quantity to ItemTransaction Table(Subtract the Quantity)
                            UpdateItemTransaction(ItemId, McFrom, -(ConvrtdQty), UserId, CurrentDate);

                        //*******Mc To
                        ItemTransactions McToItemObj = db.ItemTransactions.Where(a => (a.ItemId == ItemId && a.McId == McTo)).FirstOrDefault();

                        if (McToItemObj == null)
                            //Add Quantity to ItemTransaction Table
                            AddItemTransaction(ItemId, McTo, (ConvrtdQty), UserId, CurrentDate);
                        else
                            //Update Quantity to ItemTransaction Table
                            UpdateItemTransaction(ItemId, McTo, (ConvrtdQty), UserId, CurrentDate);
                    }
                    else
                    {
                        if (TransType == "CreditSale" || TransType == "PurchaseReturn" || TransType == "CreditSaleUsedMaterials")
                        {
                            ConvrtdQty = -ConvrtdQty;
                        }

                        //*******Material Centre
                        ItemTransactions McItemObj = db.ItemTransactions.Where(a => (a.ItemId == ItemId && a.McId == MaterialCenter)).FirstOrDefault();

                        if (McItemObj == null)
                            //Add Quantity to ItemTransaction Table
                            AddItemTransaction(ItemId, MaterialCenter, ConvrtdQty, UserId, CurrentDate);
                        else
                            //Update Quantity to ItemTransaction Table
                            UpdateItemTransaction(ItemId, MaterialCenter, ConvrtdQty, UserId, CurrentDate);
                    }
                    /************************************************************************************/
                }
            }
            return true;
        }

        //Function to Update the quantity in Edit Mode
        public bool ItemTransInEditMode(string TransType, long MaterialCenter, long McFrom, long McTo, string[][] array, long TransactionId, string UserId, DateTime CurrentDate)
        {
            long PrevItemId = 0, ItemId = 0, UnitId = 0, PrevMcFrom = 0, PrevMcTo = 0;
            decimal PreviousQty = 0, ConvrtdQty = 0, Quantity = 0;
            long? PrevMc = 0;

            List<SelectMultiFormatForItem> PrevItems = new List<SelectMultiFormatForItem>();

            if (TransType != "CreditSaleUsedMaterials")
            {
                switch (TransType)
                {
                    case "StockTransfer":

                        PrevItems = (from a in db.StockTransferItems
                                     join b in db.Items on a.Item equals b.ItemID
                                     join c in db.StockTransfers
                                     on a.StockTransferId equals c.Id
                                     where (a.StockTransferId == TransactionId)
                                     select new SelectMultiFormatForItem
                                     {
                                         ItemId = a.Item,
                                         Quantity = ((a.Unit == b.ItemUnitID) ?
                                                           (b.SubUnitId != null) ? (a.Quantity * b.ConFactor) : a.Quantity
                                                           : a.Quantity),
                                         McFrom = c.MCFrom,
                                         McTo = c.MCTo
                                     }).ToList();
                        break;

                    case "CreditSale":

                        PrevItems = (from a in db.SEItemss
                                     join b in db.Items on a.Item equals b.ItemID
                                     join c in db.SalesEntrys
                                     on a.SalesEntry equals c.SalesEntryId
                                     where (c.SalesEntryId == TransactionId)
                                     select new SelectMultiFormatForItem
                                     {
                                         ItemId = a.Item,
                                         Quantity = ((a.ItemUnit == b.ItemUnitID) ?
                                                                (b.SubUnitId != null) ? (a.ItemQuantity * b.ConFactor) : a.ItemQuantity
                                                                : a.ItemQuantity),
                                         MaterialCenter = c.MaterialCenter,
                                     }).ToList();
                        break;

                    case "SalesReturn":

                        PrevItems = (from a in db.SRItemss
                                     join b in db.Items on a.Item equals b.ItemID
                                     join c in db.SalesReturns
                                     on a.SalesReturnId equals c.SalesReturnId
                                     where (c.SalesReturnId == TransactionId)
                                     select new SelectMultiFormatForItem
                                     {
                                         ItemId = a.Item,
                                         Quantity = ((a.ItemUnit == b.ItemUnitID) ?
                                                                (b.SubUnitId != null) ? (a.ItemQuantity * b.ConFactor) : a.ItemQuantity
                                                                : a.ItemQuantity),
                                         MaterialCenter = c.MaterialCenter,
                                     }).ToList();
                        break;

                    case "Purchase":

                        PrevItems = (from a in db.PEItemss
                                     join b in db.Items on a.Item equals b.ItemID
                                     join c in db.PurchaseEntrys
                                     on a.PurchaseEntry equals c.PurchaseEntryId
                                     where (c.PurchaseEntryId == TransactionId)
                                     select new SelectMultiFormatForItem
                                     {
                                         ItemId = a.Item,
                                         Quantity = ((a.ItemUnit == b.ItemUnitID) ?
                                                                (b.SubUnitId != null) ? (a.ItemQuantity * b.ConFactor) : a.ItemQuantity
                                                                : a.ItemQuantity),
                                         MaterialCenter = c.MaterialCenter,
                                     }).ToList();
                        break;

                    case "PurchaseReturn":

                        PrevItems = (from a in db.PRItemss
                                     join b in db.Items on a.Item equals b.ItemID
                                     join c in db.PurchaseReturns
                                     on a.PurchaseReturnId equals c.PurchaseReturnId
                                     where (c.PurchaseReturnId == TransactionId)
                                     select new SelectMultiFormatForItem
                                     {
                                         ItemId = a.Item,
                                         Quantity = ((a.ItemUnit == b.ItemUnitID) ?
                                                                (b.SubUnitId != null) ? (a.ItemQuantity * b.ConFactor) : a.ItemQuantity
                                                                : a.ItemQuantity),
                                         MaterialCenter = c.MaterialCenter,
                                     }).ToList();
                        break;
                    default:
                        break;
                }
                if (PrevItems.Count() != 0)
                {
                    PrevMcFrom = PrevItems.Select(x => x.McFrom).First();
                    PrevMcTo = PrevItems.Select(x => x.McTo).FirstOrDefault();
                    PrevMc = PrevItems.Select(x => x.MaterialCenter).FirstOrDefault();
                }
                /************* delete last Transaction from ItemTransaction *************/
                foreach (var prevrow in PrevItems)
                {
                    PrevItemId = prevrow.ItemId;
                    PreviousQty = prevrow.Quantity;

                    if (TransType == "StockTransfer")
                    {
                        //*******Previous Mc From  
                        //Update the quantity in previous McFrom                    
                        UpdateItemTransaction(PrevItemId, PrevMcFrom, PreviousQty, UserId, CurrentDate);

                        //*******Previous Mc To
                        //Update the quantity in previous McTo
                        UpdateItemTransaction(PrevItemId, PrevMcTo, -(PreviousQty), UserId, CurrentDate);
                    }
                    else
                    {
                        if (TransType == "SalesReturn" || TransType == "Purchase")
                        {
                            PreviousQty = -PreviousQty;
                        }
                        //*******Material Centre
                        //Update the quantity in previous Material Centre
                        UpdateItemTransaction(PrevItemId, PrevMc, PreviousQty, UserId, CurrentDate);
                    }
                }
            }

            /************* Insert new transaction into ItemTransaction ***********/
            foreach (var arr in array)
            {
                if (TransType != "CreditSaleUsedMaterials" || arr[1] != null)
                {
                    ItemId = Convert.ToInt64(arr[0]);
                    UnitId = Convert.ToInt64(arr[1]);
                    Quantity = Convert.ToDecimal(arr[2]);

                    ConvrtdQty = (from b in db.Items
                                  where (b.ItemID == ItemId)
                                  select (UnitId == b.ItemUnitID) ?
                                     (b.SubUnitId != null) ? (Quantity * b.ConFactor) : Quantity
                                     : Quantity).FirstOrDefault();

                    if (TransType == "StockTransfer")
                    {
                        //*******Mc From
                        ItemTransactions McFromItemObj = db.ItemTransactions.Where(a => (a.ItemId == ItemId && a.McId == McFrom)).FirstOrDefault();

                        if (McFromItemObj == null)
                            //Add Quantity to ItemTransaction Table
                            AddItemTransaction(ItemId, McFrom, -(ConvrtdQty), UserId, CurrentDate);
                        else
                            //Update Quantity to ItemTransaction Table
                            UpdateItemTransaction(ItemId, McFrom, -(ConvrtdQty), UserId, CurrentDate);

                        //*******Mc To
                        ItemTransactions McToItemObj = db.ItemTransactions.Where(a => (a.ItemId == ItemId && a.McId == McTo)).FirstOrDefault();

                        if (McToItemObj == null)
                            //Add Quantity to ItemTransaction Table
                            AddItemTransaction(ItemId, McTo, (ConvrtdQty), UserId, CurrentDate);
                        else
                            //Update Quantity to ItemTransaction Table
                            UpdateItemTransaction(ItemId, McTo, (ConvrtdQty), UserId, CurrentDate);
                    }
                    else
                    {
                        if (TransType == "CreditSale" || TransType == "PurchaseReturn" || TransType == "CreditSaleUsedMaterials")
                        {
                            ConvrtdQty = -ConvrtdQty;
                        }

                        //*******Material Center
                        ItemTransactions McItemObj = db.ItemTransactions.Where(a => (a.ItemId == ItemId && a.McId == MaterialCenter)).FirstOrDefault();

                        if (McItemObj == null)
                            //Add Quantity to ItemTransaction Table
                            AddItemTransaction(ItemId, MaterialCenter, ConvrtdQty, UserId, CurrentDate);
                        else
                            //Update Quantity to ItemTransaction Table
                            UpdateItemTransaction(ItemId, MaterialCenter, ConvrtdQty, UserId, CurrentDate);
                    }
                }
                /****************************************************************************************/
            }
            return true;
        }

        //Function to Update the quantity in Delete Mode
        public bool ItemTransInDeleteMode(string TransType, long? MaterialCenter, long McFrom, long McTo, long TransactionId, string UserId, DateTime CurrentDate)
        {
            long ItemId = 0;
            decimal Quantity = 0;

            List<SelectMultiFormatForItem> ItemList = new List<SelectMultiFormatForItem>();

            switch (TransType)
            {
                case "StockTransfer":

                    ItemList = (from a in db.StockTransferItems
                                join b in db.Items on a.Item equals b.ItemID
                                where (a.StockTransferId == TransactionId)
                                select new SelectMultiFormatForItem
                                {
                                    ItemId = a.Item,
                                    Quantity = (a.Unit == b.ItemUnitID) ?
                                                      (b.SubUnitId != null) ? (a.Quantity * b.ConFactor) : a.Quantity
                                                      : a.Quantity
                                }).ToList();
                    break;

                case "CreditSale":

                    ItemList = (from a in db.SEItemss
                                join b in db.Items on a.Item equals b.ItemID
                                where (a.SalesEntry == TransactionId)
                                select new SelectMultiFormatForItem
                                {
                                    ItemId = a.Item,
                                    Quantity = (a.ItemUnit == b.ItemUnitID) ?
                                                        (b.SubUnitId != null) ? (a.ItemQuantity * b.ConFactor) : a.ItemQuantity
                                                        : a.ItemQuantity
                                }).ToList();
                    break;

                case "SalesReturn":

                    ItemList = (from a in db.SRItemss
                                join b in db.Items on a.Item equals b.ItemID
                                where (a.SalesReturnId == TransactionId)
                                select new SelectMultiFormatForItem
                                {
                                    ItemId = a.Item,
                                    Quantity = (a.ItemUnit == b.ItemUnitID) ?
                                                        (b.SubUnitId != null) ? (a.ItemQuantity * b.ConFactor) : a.ItemQuantity
                                                        : a.ItemQuantity
                                }).ToList();
                    break;

                case "Purchase":

                    ItemList = (from a in db.PEItemss
                                join b in db.Items on a.Item equals b.ItemID
                                where (a.PurchaseEntry == TransactionId)
                                select new SelectMultiFormatForItem
                                {
                                    ItemId = a.Item,
                                    Quantity = (a.ItemUnit == b.ItemUnitID) ?
                                                     (b.SubUnitId != null) ? (a.ItemQuantity * b.ConFactor) : a.ItemQuantity
                                                     : a.ItemQuantity
                                }).ToList();
                    break;

                case "PurchaseReturn":

                    ItemList = (from a in db.PRItemss
                                join b in db.Items on a.Item equals b.ItemID
                                where (a.PurchaseReturnId == TransactionId)
                                select new SelectMultiFormatForItem
                                {
                                    ItemId = a.Item,
                                    Quantity = (a.ItemUnit == b.ItemUnitID) ?
                                                     (b.SubUnitId != null) ? (a.ItemQuantity * b.ConFactor) : a.ItemQuantity
                                                     : a.ItemQuantity
                                }).ToList();
                    break;

                default:
                    break;
            }

            if (ItemList != null && ItemList.Count != 0)
            {
                foreach (var row in ItemList)
                {
                    ItemId = row.ItemId;
                    Quantity = row.Quantity;

                    if (TransType == "StockTransfer")
                    {
                        //*******Mc From
                        ItemTransactions McFromItemObj = db.ItemTransactions.Where(a => (a.ItemId == ItemId && a.McId == McFrom)).FirstOrDefault();

                        // Update Quantity to ItemTransaction Table(Add the Quantity)
                        if (McFromItemObj != null)
                            UpdateItemTransaction(ItemId, McFrom, Quantity, UserId, CurrentDate);

                        // *******Mc To
                        ItemTransactions McToItemObj = db.ItemTransactions.Where(a => (a.ItemId == ItemId && a.McId == McTo)).FirstOrDefault();

                        //Update Quantity to ItemTransaction Table(Subtract the Quantity)
                        if (McToItemObj != null)
                            UpdateItemTransaction(ItemId, McTo, -(Quantity), UserId, CurrentDate);
                    }
                    else
                    {
                        if (TransType == "SalesReturn" || TransType == "Purchase")
                        {
                            Quantity = -Quantity;
                        }
                        //*******Material Centre                                     
                        UpdateItemTransaction(ItemId, MaterialCenter, Quantity, UserId, CurrentDate);
                    }
                }
            }
            return true;
        }

        //Add Item transaction
        public int AddItemTransaction(long ItemId, long? McId, decimal Quantity, string UserId, DateTime UpdatedDate)
        {
            var ItemTrans = new ItemTransactions
            {
                ItemId = ItemId,
                McId = McId,
                TotalStock = Quantity,
                LastUpdatedBy = UserId,
                LastUpdatedDate = UpdatedDate
            };
            db.ItemTransactions.Add(ItemTrans);
            return db.SaveChanges();
        }

        // Update Item transaction
        public int UpdateItemTransaction(long ItemId, long? McId, decimal Quantity, string UserId, DateTime UpdatedDate)
        {
            ItemTransactions ItemObj = db.ItemTransactions.Where(a => (a.ItemId == ItemId && a.McId == McId)).FirstOrDefault();

            if (ItemObj != null)
            {
                ItemObj.TotalStock = ItemObj.TotalStock + Quantity;
                ItemObj.LastUpdatedBy = UserId;
                ItemObj.LastUpdatedDate = UpdatedDate;

                db.Entry(ItemObj).State = EntityState.Modified;
                return db.SaveChanges();
            }
            else
                return 0;
        }

        // Delete Item transaction
        public bool DeleteItemTransaction(long ItemId)
        {
            db.ItemTransactions.RemoveRange(db.ItemTransactions.Where(a => a.ItemId == ItemId));
            int delete = db.SaveChanges();
            if (delete != 0)
                return true;
            else
                return false;
        }

        #endregion       

        public string chequevalidation(ChequeViewModel cheq)
        {
            var msg = "";
            if (cheq.Amount == null)
            {
                msg = msg + "Cheque Amount, ";
            }
            if (cheq.ChequeNo == null)
            {
                msg = msg + "Cheque No., ";
            }
            if (cheq.Bank == null)
            {
                msg = msg + "Bank, ";
            }
            if (cheq.Date == null)
            {
                msg = msg + "Cheque Date, ";
            }
            if (msg != "")
            {
                msg += msg + " is mandatory";
            }
            return msg;
        }
    }


    class SelectFormatDisabled
    {
        public string id { get; set; }
        public string text { get; set; }
        public string disabled { get; set; }
    }
    class SelectFormat
    {
        public long id { get; set; }
        public string text { get; set; }
    }
    class SelectFormat4
    {
        public long id { get; set; }
        public string text { get; set; }
        public decimal? pprice { get; set; }
        public decimal? sprice { get; set; }
        public decimal? cashprice { get; set; }
        public decimal? creditprice { get; set; }
        public string ItemDescription { get; set; }
        public long? ItemImId { get; set; }
        public string FileName { get; set; }
    }
    class SelectFormat5
    {
        public string id { get; set; }
        public string text { get; set; }
        public decimal? pprice { get; set; }
        public decimal? sprice { get; set; }
        public string ItemDescription { get; set; }
        public long? ItemImId { get; set; }
        public string FileName { get; set; }
    }
    class SelectFormat3
    {
        public long? id { get; set; }
        public string text { get; set; }
    }
    class SelectFormat2
    {
        public string id { get; set; }
        public string text { get; set; }
    }
    class SelectStatusFormat
    {
        public long id { get; set; }
        public string text { get; set; }
        public Status? status { get; set; }
    }

    class SelectMultiFormat
    {
        public long id { get; set; }
        public string text { get; set; }
        public string Name { get; set; }
    }
    class SelectUserFormat
    {
        public string id { get; set; }
        public string text { get; set; }
    }
    class SelectFormatNew
    {
        public string id { get; set; }
        public string text { get; set; }
    }
    public class MonthWise
    {
        public int Year { get; set; }
        public int? January { get; set; }
        public int? February { get; set; }
        public int? March { get; set; }
        public int? April { get; set; }
        public int? May { get; set; }
        public int? June { get; set; }
        public int? July { get; set; }
        public int? August { get; set; }
        public int? September { get; set; }
        public int? October { get; set; }
        public int? November { get; set; }
        public int? December { get; set; }
        public int? total { get; set; }
    }
    public class MonthWiseDecimal
    {
        public int Year { get; set; }
        public decimal? January { get; set; }
        public decimal? February { get; set; }
        public decimal? March { get; set; }
        public decimal? April { get; set; }
        public decimal? May { get; set; }
        public decimal? June { get; set; }
        public decimal? July { get; set; }
        public decimal? August { get; set; }
        public decimal? September { get; set; }
        public decimal? October { get; set; }
        public decimal? November { get; set; }
        public decimal? December { get; set; }
        public decimal? total { get; set; }
    }
    public class SelectMultiFormatForItem
    {
        public long ItemId { get; set; }
        public long McFrom { get; set; }
        public long McTo { get; set; }
        public long? MaterialCenter { get; set; }
        public decimal Quantity { get; set; }
    }
}