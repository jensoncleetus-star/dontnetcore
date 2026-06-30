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
using BindAcc;
using Microsoft.AspNetCore.Identity;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class BalanceSheetController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public BalanceSheetController()
        {
            db = new ApplicationDbContext();
            com = new Common();

        }
        [HttpGet]
        //[QkAuthorize(Roles = "Dev,BalanceSheet")]
        public ActionResult BalanceSheet()
        {
            _FinancialYear();
            return View();
        }

        [HttpPost]

        public ActionResult GetBalanceSheet(string fromdate, string todate)
        {
            db.SetCommandTimeOut(60 * 60);
            BalanceSheetViewModel vmodel = new BalanceSheetViewModel();
            String format = "dd-MM-yyyy";
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? from = null;
            DateTime? to = null;
            var fun = 2;
            var funbal = 1;

            if (fromdate != "")
            {
                fdate = DateTime.ParseExact(fromdate, format, new CultureInfo("en-GB"));
                from = fdate;
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate.ToString(), new CultureInfo("en-GB"));
                to = tdate;
            }
            vmodel.from = from;
            vmodel.to = to;
            ViewBag.ToDt = todate;
            ViewBag.frmDt = fromdate;

            //==================================LIABILITY================================================
            int increL = 1;
            int LiabCount = 1;

            //--------------------------profit brought forward-------------------------------------\
            var datec = (DateTime)fdate;
            DateTime seldate = datec.AddDays(-1);
            var passdate = seldate.ToString("dd-MM-yyyy");
            var profitAndLossBf =  getProftAndLoss("", passdate, fun);

            var amtbf = (decimal)profitAndLossBf["amount"];
            var typebf = (string)profitAndLossBf["type"];

            var acctypez = typebf == "Profit" ? "asset" : "liability";

            //BalanceSheet pbf = new BalanceSheet()
            //    AccountsGroupID = -1,
            //    Particulars = typebf + " brought forward",
            //    Parent = 0,
            //    GpName = typebf + " brought forward",
            //    AccType = acctypez,
            //    Debit = amtbf,
            //    Credit = 0,
            //    ParentName = typebf + " brought forward",
            //    orderB = 1,
            //    Temp = LiabCount,

            //--------------------------Profit for the period--------------------------------------
            #region Profit for the period

            var profitAndLoss = getProftAndLoss(fromdate, todate, fun);

            var pfamt = (decimal)profitAndLoss["amount"];
            var pftype = (string)profitAndLoss["type"];
            var acctype = pftype == "Profit" ? "asset" : "liability";
            var avalstock = (decimal)profitAndLoss["stock"];// Convert.ToDecimal(1437606.25);// Convert.ToDecimal(322147.49);// //Convert.ToDecimal(1437606.25);//getOpeningStock(todate, "close");//

            BalanceSheet bsheet = new BalanceSheet()
            {
                AccountsGroupID = -1,
                Particulars = pftype + " for the period",
                Parent = 0,
                GpName = pftype + " for the period",
                AccType = acctype,
                Debit = pfamt,
                Credit = 0,
                ParentName = pftype + " for the period",
                orderB = 1,
                Temp = LiabCount,
            };
            List<BalanceSheet> pandl = new List<BalanceSheet>();
            pandl.Add(bsheet);
            var ProfitLoss = pandl;

            #endregion

            //capital account
            var CapitalAcoount = Common.GetChildAccGroupNew(1, "Capital Account", "liability", null, to, funbal, LiabCount);
            // increL = getSubTotal(LiabCount, CapitalAcoount, 1);//calculate child amounts
            decimal CAccount = CapitalAcoount != null ? (decimal)CapitalAcoount.Distinct().Sum(a => a.Credit - a.Debit) : 0;

            //capital account



            LiabCount++;
            //current liabilities

            var CurrentLiabilities = Common.GetChildAccGroupNew(3, "Current Liabilities", "liability", null, to, funbal, LiabCount);
            LiabCount = increL;
            decimal CLiability = CurrentLiabilities != null ? (decimal)CurrentLiabilities.Sum(a => a.Credit - a.Debit) : 0;

            //current liability

            //Loans (Liability)
            var LoansLiability = Common.GetChildAccGroupNew(6, "Loans (Liability)", "liability", null, to, funbal, LiabCount);
            LiabCount = increL;
            decimal LLiability = LoansLiability != null ? (decimal)LoansLiability.Sum(a => a.Credit - a.Debit) : 0;

            //loans liability


            //==================================ASSETS================================================
            int increA = 0;
            //Fixed Asset
            var FixedAssets = Common.GetChildAccGroupNew(4, "Fixed Assets", "asset", null, to, funbal, 1);
            decimal fassetAcc = FixedAssets != null ? (decimal)FixedAssets.Sum(a => a.Debit - a.Credit) : 0;

            //fixed asset

            //Current Asset
            var CurrentAssets = Common.GetChildAccGroupNew(2, "Current Assets", "asset", null, to, funbal, 2);
            decimal cassetAcc = CurrentAssets != null ? (decimal)CurrentAssets.Sum(a => a.Debit - a.Credit) : 0;

            //current asset
            var cuparent = CurrentAssets.Where(a => a.Parent == 0).ToList();//parent // not include stock-in-hand 
            var cuchild = CurrentAssets.Where(a => a.Parent != 0 && a.AccountsGroupID != 23 && a.Temp != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
            var cuasset = cuparent.Union(cuchild);

            //-----------------------------------------------------------------------------------------


            IEnumerable<BalanceSheet> liab = new List<BalanceSheet>();
            if (typebf == "Profit" && amtbf != 0)
            {
            }
            if (pftype == "Profit" && pfamt != 0)
            {
                liab = liab.Union(ProfitLoss);
            }
            if (CAccount != 0)
            {
                liab = liab.Union(CapitalAcoount);
            }
            liab = liab.Union(CurrentLiabilities);
            if (LLiability != 0)
            {
                liab = liab.Union(LoansLiability);
            }


            //-------stock in hand---------
            BalanceSheet stkhand = new BalanceSheet()
            {
                AccountsGroupID = 2,
                Particulars = "Stock-in-hand",
                Parent = 0,
                GpName = "Stock-in-hand",
                AccType = "asset",
                Debit = avalstock,
                Credit = 0,
                ParentName = "Current Assets",
                orderB = 2,
                Temp = LiabCount,
            };
            List<BalanceSheet> stkhands = new List<BalanceSheet>();
            stkhands.Add(stkhand);
            var stkh = stkhands;
            //-------------


            cuasset = cuasset.Union(stkh);
            IEnumerable<BalanceSheet> asset = new List<BalanceSheet>();
            if (typebf == "Loss" && amtbf != 0)
            {
            }
            if (pftype == "Loss" && pfamt != 0)
            {
                asset = asset.Union(ProfitLoss);
            }
            asset = asset.Union(FixedAssets);

            asset = asset.Union(CurrentAssets);
            asset = asset.Union(stkh);
            #region all Base account groups except Current Assets,Current Liabilities,Fixed Assets,Revenue Accounts,Capital Account
            long[] groups = { 1, 2, 3, 4, 7 };
            var ACCGroups = db.AccountsGroups.Where(a => a.Parent == 0 && !groups.Contains(a.AccountsGroupID)).ToList();

            /*  foreach (var acc in ACCGroups)
              {
                  var GroupItem = Common.GetChildAccGroupNew(acc.AccountsGroupID, acc.Name, "asset", null, to, funbal, 3);
                  decimal Amount = GroupItem != null ? (decimal)GroupItem.Sum(a => a.Debit - a.Credit) : 0;
                  var GroupParent = GroupItem.Where(a => a.Parent == 0 && (a.Debit != 0 || a.Credit != 0)).ToList();
                  var GroupChild = GroupItem.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
                  var GroupI = GroupParent.Union(GroupChild);
                  if (Amount > 0)
                  {
                      asset = asset.Union(GroupI);
                  }
                  else
                  {
                      liab = liab.Union(GroupI);
                  }
              }
             */ //------------------------------------------------------
            #endregion

            //diff. in open balance 
            #region diff. in open balance 
            decimal debitsum1 = (decimal)asset.Where(a => a.Parent == 0).Select(a => a.Debit).Sum();
            decimal debitsum2 = (decimal)asset.Where(a => a.Parent == 0).Select(a => a.Credit).Sum();

            decimal creditsum1 = (decimal)liab.Where(a => a.Parent == 0).Select(a => a.Debit).Sum();
            decimal creditsum2 = (decimal)liab.Where(a => a.Parent == 0).Select(a => a.Credit).Sum();

            decimal debitval = debitsum1 - debitsum2;
            decimal creditval = (creditsum2 + creditsum1);

            string type = "";
            if (debitval != 0 || creditval != 0)
            {
                decimal diffopnval = debitval - creditval;
                if (debitval < creditval)
                {
                    diffopnval = diffopnval * -1;
                    type = "asset";
                }
                else
                {
                    type = "liability";
                }

                if (Math.Round(diffopnval, 2) > 0)
                {
                    BalanceSheet sheet = new BalanceSheet()
                    {
                        AccountsGroupID = -11,
                        Particulars = "Diff In OpenBalance",
                        Parent = 0,
                        GpName = "Diff In OpenBalance",
                        AccType = type,
                        Debit = diffopnval,
                        Credit = 0,
                        ParentName = "OpenBalance",
                        orderB = 10,
                        Temp = 0,
                    };
                    List<BalanceSheet> OpnBalDiffer = new List<BalanceSheet>();
                    OpnBalDiffer.Add(sheet);


                    if (OpnBalDiffer[0].AccType == "liability")
                    {
                    }
                    else
                    {
                    }
                }
            }
            #endregion

            var count = getOrdering(liab.ToList());
            var counts = getOrdering(asset.ToList());

            //outer joins-for single row
            var leftOuterJoin = (from a in liab
                                 join b in asset on a.Temp equals b.Temp into astt
                                 from b in astt.DefaultIfEmpty()
                                 select new
                                 {
                                     AccountsGroupIdA = b?.AccountsGroupID,
                                     ParticularA = b?.Particulars,
                                     DebitA = b?.Debit,
                                     CreditA = b?.Credit,
                                     AccountsGroupIdL = a?.AccountsGroupID,
                                     ParticularL = a?.Particulars,
                                     DebitL = a?.Debit,
                                     CreditL = a?.Credit,

                                     ParentA = b?.Parent,
                                     ParentL = a?.Parent,
                                 });

            var rightOuterJoin = (from a in asset
                                  join b in liab on a.Temp equals b.Temp into lib
                                  from b in lib.DefaultIfEmpty()
                                  select new
                                  {
                                      AccountsGroupIdA = a?.AccountsGroupID,
                                      ParticularA = a?.Particulars,
                                      DebitA = a?.Debit,
                                      CreditA = a?.Credit,
                                      AccountsGroupIdL = b?.AccountsGroupID,
                                      ParticularL = b?.Particulars,
                                      DebitL = b?.Debit,
                                      CreditL = b?.Credit,

                                      ParentA = a?.Parent,
                                      ParentL = b?.Parent,
                                  });

            var full = leftOuterJoin.Union(rightOuterJoin);

            //change to view model
            vmodel.BalanceSheetDisplay = (from a in full
                                          where a.ParentA == 0 || a.ParentL == 0
                                          select new BalanceSheetDisplay
                                          {
                                              AccountGroupIDA = a?.AccountsGroupIdA,
                                              ParticularA = a?.ParticularA,
                                              DebitA = a?.DebitA,
                                              CreditA = a?.CreditA,
                                              AccountGroupIDL = a?.AccountsGroupIdL,
                                              ParticularL = a?.ParticularL,
                                              DebitL = a?.DebitL,
                                              CreditL = a?.CreditL,

                                              ParentA = a?.ParentA,
                                              ParentL = a?.ParentL,
                                          }).ToList();

            companySet();
            return View(vmodel);
        }
        [HttpPost]

        public ActionResult GetBalanceSheetqucknet(string fromdate, string todate)
        {
            db.SetCommandTimeOut(60 * 60);
            BalanceSheetViewModel vmodel = new BalanceSheetViewModel();
            String format = "dd-MM-yyyy";
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? from = null;
            DateTime? to = null;
            var fun = 2;
            var funbal = 1;

            if (fromdate != "")
            {
                fdate = DateTime.ParseExact(fromdate, format, new CultureInfo("en-GB"));
                from = fdate;
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate.ToString(), new CultureInfo("en-GB"));
                to = tdate;
            }
            vmodel.from = from;
            vmodel.to = to;
            ViewBag.ToDt = todate;
            ViewBag.frmDt = fromdate;

            //==================================LIABILITY================================================
            int increL = 1;
            int LiabCount = 1;

            //--------------------------profit brought forward-------------------------------------\
            var datec = (DateTime)fdate;
            DateTime seldate = datec.AddDays(-1);
            var passdate = seldate.ToString("dd-MM-yyyy");



            //BalanceSheet pbf = new BalanceSheet()
            //    AccountsGroupID = -1,
            //    Particulars = typebf + " brought forward",
            //    Parent = 0,
            //    GpName = typebf + " brought forward",
            //    AccType = acctypez,
            //    Debit = amtbf,
            //    Credit = 0,
            //    ParentName = typebf + " brought forward",
            //    orderB = 1,
            //    Temp = LiabCount,

            //--------------------------Profit for the period--------------------------------------
            #region Profit for the period

            var profitconts = new Dictionary<string, object>();
            profitconts.Add("type", "Profit");
            profitconts.Add("amount", Convert.ToDecimal("165272.98"));
            profitconts.Add("stock", Convert.ToDecimal("1690250.53"));
            var profitAndLoss = getProftAndLoss(fromdate, todate, fun);

            var pfamt = (decimal)profitAndLoss["amount"];
            var pftype = (string)profitAndLoss["type"];
            var acctype = pftype == "Profit" ? "asset" : "liability";
            var avalstock = Convert.ToDecimal(profitAndLoss["stock"]);// Convert.ToDecimal(1437606.25);// Convert.ToDecimal(322147.49);// //Convert.ToDecimal(1437606.25);//getOpeningStock(todate, "close");//

            BalanceSheet bsheet = new BalanceSheet()
            {
                AccountsGroupID = -1,
                Particulars =  "Retained Earnings",
                Parent = 0,
                GpName = "Retained Earnings",
                AccType = acctype,
                Debit = pfamt,
                Credit = 0,
                ParentName = "Retained Earnings",
                orderB = 1,
                Temp = LiabCount,
            };
            List<BalanceSheet> pandl = new List<BalanceSheet>();
            pandl.Add(bsheet);
            var ProfitLoss = pandl;

            #endregion

            //capital account
            var CapitalAcoount = Common.GetChildAccGroupNew(82, "Capital Account", "liability", null, to, funbal, LiabCount);
            // increL = getSubTotal(LiabCount, CapitalAcoount, 1);//calculate child amounts
            decimal CAccount = CapitalAcoount != null ? (decimal)CapitalAcoount.Distinct().Sum(a => a.Credit - a.Debit) : 0;

            //capital account



            LiabCount++;
            //current liabilities

            var CurrentLiabilities = Common.GetChildAccGroupNew(3, "Current Liabilities", "liability", null, to, funbal, LiabCount);
            LiabCount = increL;
            decimal CLiability = CurrentLiabilities != null ? (decimal)CurrentLiabilities.Sum(a => a.Credit - a.Debit) : 0;

            //current liability

            //Loans (Liability)
            var LoansLiability = Common.GetChildAccGroupNew(6, "Loans (Liability)", "liability", null, to, funbal, LiabCount);
            LiabCount = increL;
            decimal LLiability = LoansLiability != null ? (decimal)LoansLiability.Sum(a => a.Credit - a.Debit) : 0;

            //loans liability


            //==================================ASSETS================================================
            int increA = 0;
            //Fixed Asset

            //fixed asset

            //Current Asset
            var CurrentAssets = Common.GetChildAccGroupNew(2, "Assets", "asset", null, to, funbal, 2);
            decimal cassetAcc = CurrentAssets != null ? (decimal)CurrentAssets.Sum(a => a.Debit - a.Credit) : 0;

            //current asset
            var cuparent = CurrentAssets.Where(a => a.Parent == 0).ToList();//parent // not include stock-in-hand 
            var cuchild = CurrentAssets.Where(a => a.Parent != 0 && a.AccountsGroupID != 23 && a.Temp != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
            var cuasset = cuparent.Union(cuchild);

            //-----------------------------------------------------------------------------------------


            IEnumerable<BalanceSheet> liab = new List<BalanceSheet>();
            if (pftype == "Profit" && pfamt != 0)
            {
                liab = liab.Union(ProfitLoss);
            }
            if (CAccount != 0)
            {
                liab = liab.Union(CapitalAcoount);
            }
            liab = liab.Union(CurrentLiabilities);
            if (LLiability != 0)
            {
                liab = liab.Union(LoansLiability);
            }


            //-------stock in hand---------
            BalanceSheet stkhand = new BalanceSheet()
            {
                AccountsGroupID = 2,
                Particulars = "Stock-in-hand",
                Parent = 0,
                GpName = "Stock-in-hand",
                AccType = "asset",
                Debit = avalstock,
                Credit = 0,
                ParentName = "Current Assets",
                orderB = 2,
                Temp = LiabCount,
            };
            List<BalanceSheet> stkhands = new List<BalanceSheet>();
            stkhands.Add(stkhand);
            var stkh = stkhands;
            //-------------


            cuasset = cuasset.Union(stkh);
            IEnumerable<BalanceSheet> asset = new List<BalanceSheet>();
            if (pftype == "Loss" && pfamt != 0)
            {
                asset = asset.Union(ProfitLoss);
            }

            asset = asset.Union(CurrentAssets);
            asset = asset.Union(stkh);
            #region all Base account groups except Current Assets,Current Liabilities,Fixed Assets,Revenue Accounts,Capital Account
            long[] groups = { 1, 2, 3, 4, 7 };
            var ACCGroups = db.AccountsGroups.Where(a => a.Parent == 0 && !groups.Contains(a.AccountsGroupID)).ToList();

            /*  foreach (var acc in ACCGroups)
              {
                  var GroupItem = Common.GetChildAccGroupNew(acc.AccountsGroupID, acc.Name, "asset", null, to, funbal, 3);
                  decimal Amount = GroupItem != null ? (decimal)GroupItem.Sum(a => a.Debit - a.Credit) : 0;
                  var GroupParent = GroupItem.Where(a => a.Parent == 0 && (a.Debit != 0 || a.Credit != 0)).ToList();
                  var GroupChild = GroupItem.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
                  var GroupI = GroupParent.Union(GroupChild);
                  if (Amount > 0)
                  {
                      asset = asset.Union(GroupI);
                  }
                  else
                  {
                      liab = liab.Union(GroupI);
                  }
              }
             */ //------------------------------------------------------
            #endregion

            //diff. in open balance 
            #region diff. in open balance 
            decimal debitsum1 = (decimal)asset.Where(a => a.Parent == 0).Select(a => a.Debit).Sum();
            decimal debitsum2 = (decimal)asset.Where(a => a.Parent == 0).Select(a => a.Credit).Sum();

            decimal creditsum1 = (decimal)liab.Where(a => a.Parent == 0).Select(a => a.Debit).Sum();
            decimal creditsum2 = (decimal)liab.Where(a => a.Parent == 0).Select(a => a.Credit).Sum();

            decimal debitval = debitsum1 - debitsum2;
            decimal creditval = (creditsum2 + creditsum1);

            string type = "";
            if (debitval != 0 || creditval != 0)
            {
                decimal diffopnval = debitval - creditval;
                if (debitval < creditval)
                {
                    diffopnval = diffopnval * -1;
                    type = "asset";
                }
                else
                {
                    type = "liability";
                }

                if (Math.Round(diffopnval, 2) > 0)
                {
                    BalanceSheet sheet = new BalanceSheet()
                    {
                        AccountsGroupID = -11,
                        Particulars = "Diff In OpenBalance",
                        Parent = 0,
                        GpName = "Diff In OpenBalance",
                        AccType = type,
                        Debit = diffopnval,
                        Credit = 0,
                        ParentName = "OpenBalance",
                        orderB = 10,
                        Temp = 0,
                    };
                    List<BalanceSheet> OpnBalDiffer = new List<BalanceSheet>();
                    OpnBalDiffer.Add(sheet);


                    if (OpnBalDiffer[0].AccType == "liability")
                    {
                    }
                    else
                    {
                    }
                }
            }
            #endregion

            var count = getOrdering(liab.ToList());
            var counts = getOrdering(asset.ToList());

            //outer joins-for single row
            var leftOuterJoin = (from a in liab
                                 join b in asset on a.Temp equals b.Temp into astt
                                 from b in astt.DefaultIfEmpty()
                                 select new
                                 {
                                     AccountsGroupIdA = b?.AccountsGroupID,
                                     ParticularA = b?.Particulars,
                                     DebitA = b?.Debit,
                                     CreditA = b?.Credit,
                                     AccountsGroupIdL = a?.AccountsGroupID,
                                     ParticularL = a?.Particulars,
                                     DebitL = a?.Debit,
                                     CreditL = a?.Credit,

                                     ParentA = b?.Parent,
                                     ParentL = a?.Parent,
                                 });

            var rightOuterJoin = (from a in asset
                                  join b in liab on a.Temp equals b.Temp into lib
                                  from b in lib.DefaultIfEmpty()
                                  select new
                                  {
                                      AccountsGroupIdA = a?.AccountsGroupID,
                                      ParticularA = a?.Particulars,
                                      DebitA = a?.Debit,
                                      CreditA = a?.Credit,
                                      AccountsGroupIdL = b?.AccountsGroupID,
                                      ParticularL = b?.Particulars,
                                      DebitL = b?.Debit,
                                      CreditL = b?.Credit,

                                      ParentA = a?.Parent,
                                      ParentL = b?.Parent,
                                  });

            var full = leftOuterJoin.Union(rightOuterJoin);

            //change to view model
            vmodel.BalanceSheetDisplay = (from a in full
                                          where a.ParentA == 0 || a.ParentL == 0
                                          select new BalanceSheetDisplay
                                          {
                                              AccountGroupIDA = a?.AccountsGroupIdA,
                                              ParticularA = a?.ParticularA,
                                              DebitA = a?.DebitA,
                                              CreditA = a?.CreditA,
                                              AccountGroupIDL = a?.AccountsGroupIdL,
                                              ParticularL = a?.ParticularL,
                                              DebitL = a?.DebitL,
                                              CreditL = a?.CreditL,

                                              ParentA = a?.ParentA,
                                              ParentL = a?.ParentL,
                                          }).ToList();

            companySet();
            return View(vmodel);
        }
        [HttpPost]
        public ActionResult GetBalanceSheetemirtech(string fromdate, string todate)
        {
            db.SetCommandTimeOut(60 * 60);
            BalanceSheetViewModel vmodel = new BalanceSheetViewModel();
            String format = "dd-MM-yyyy";
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? from = null;
            DateTime? to = null;
            var fun = 2;
            var funbal = 1;

            if (fromdate != "")
            {
                fdate = DateTime.ParseExact(fromdate, format, new CultureInfo("en-GB"));
                from = fdate;
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate.ToString(), new CultureInfo("en-GB"));
                to = tdate;
            }
            vmodel.from = from;
            vmodel.to = to;
            ViewBag.ToDt = todate;
            ViewBag.frmDt = fromdate;

            //==================================LIABILITY================================================
            int increL = 1;
            int LiabCount = 1;

            //--------------------------profit brought forward-------------------------------------\



            //BalanceSheet pbf = new BalanceSheet()
            //    AccountsGroupID = -1,
            //    Particulars = typebf + " brought forward",
            //    Parent = 0,
            //    GpName = typebf + " brought forward",
            //    AccType = acctypez,
            //    Debit = amtbf,
            //    Credit = 0,
            //    ParentName = typebf + " brought forward",
            //    orderB = 1,
            //    Temp = LiabCount,

            //--------------------------Profit for the period--------------------------------------
            #region Profit for the period
            bool isemirtech = db.companys.Any(o => o.CPName.Contains("EMIRTECH TECHNOLOGY"));

            if (1==2&&isemirtech && fdate.Value.Year > 2025)
            {
                var profitAndLosspre = getProftAndLoss("01-01-2025", fdate.Value.AddDays(-1).Date.ToString("dd-MM-yyyy"), fun);

                var pfamtpre = (decimal)profitAndLosspre["amount"];
                var pftypepre = (string)profitAndLosspre["type"];
                var acctypepre = pftypepre == "Profit" ? "asset" : "liability";

                BalanceSheet bsheetpre = new BalanceSheet()
                {
                    AccountsGroupID = -1,
                    Particulars = "profit brought forward",
                    Parent = 0,
                    GpName = "profit brought forward",
                    AccType = acctypepre,
                    Debit = pfamtpre,
                    Credit = 0,
                    ParentName = "profit brought forward",
                    orderB = 1,
                    Temp = LiabCount,
                };
                List<BalanceSheet> pandlpre = new List<BalanceSheet>();
                pandlpre.Add(bsheetpre);
            }
            var profitconts = new Dictionary<string, object>();
            profitconts.Add("type", "Profit");
            profitconts.Add("amount", Convert.ToDecimal("99345.44"));
            profitconts.Add("stock", Convert.ToDecimal("97481.48"));
           
            var profitAndLoss = getProftAndLoss(fromdate, todate, fun);
          
            var pfamt = (decimal)profitAndLoss["amount"];
            var pftype = (string)profitAndLoss["type"];
            var acctype = pftype == "Profit" ? "asset" : "liability";
            var avalstock = Convert.ToDecimal(profitAndLoss["stock"]);// Convert.ToDecimal(1437606.25);// Convert.ToDecimal(322147.49);// //Convert.ToDecimal(1437606.25);//getOpeningStock(todate, "close");//

            BalanceSheet bsheet = new BalanceSheet()
            {
                AccountsGroupID = -1,
                Particulars = "profit this period",
                Parent = 0,
                GpName = "profit this period",
                AccType = acctype,
                Debit = pfamt,
                Credit = 0,
                ParentName = "profit this period",
                orderB = 1,
                Temp = LiabCount,
            };
            List<BalanceSheet> pandl = new List<BalanceSheet>();
            pandl.Add(bsheet);

            var ProfitLoss = pandl;
     
            #endregion

            //capital account
            var CapitalAcoount = Common.GetChildAccGroupNew(91, "Capital Account", "liability", null, to, funbal, LiabCount);
            // increL = getSubTotal(LiabCount, CapitalAcoount, 1);//calculate child amounts
            decimal CAccount = CapitalAcoount != null ? (decimal)CapitalAcoount.Distinct().Sum(a => a.Credit - a.Debit) : 0;

            //capital account



            LiabCount++;
            //current liabilities

            var CurrentLiabilities = Common.GetChildAccGroupNew(3, "Current Liabilities", "liability", null, to, funbal, LiabCount);
            LiabCount = increL;
            decimal CLiability = CurrentLiabilities != null ? (decimal)CurrentLiabilities.Sum(a => a.Credit - a.Debit) : 0;

            //current liability

            //Loans (Liability)
            var LoansLiability = Common.GetChildAccGroupNew(93, "Loans (Liability)", "liability", null, to, funbal, LiabCount);
            LiabCount = increL;
            decimal LLiability = LoansLiability != null ? (decimal)LoansLiability.Sum(a => a.Credit - a.Debit) : 0;

            //loans liability


            //==================================ASSETS================================================
            int increA = 0;
            //Fixed Asset
            var FixedAssets = Common.GetChildAccGroupNew(90, " Assets", "asset", null, to, funbal, 1);
            decimal fassetAcc = FixedAssets != null ? (decimal)FixedAssets.Sum(a => a.Debit - a.Credit) : 0;

            //fixed asset

            //Current Asset

            //current asset

            //-----------------------------------------------------------------------------------------


            IEnumerable<BalanceSheet> liab = new List<BalanceSheet>();
            if (pftype == "Profit" && pfamt != 0)
            {
                liab = liab.Union(ProfitLoss);
            }
           
            if (CAccount != 0)
            {
                liab = liab.Union(CapitalAcoount);
            }
            liab = liab.Union(CurrentLiabilities);
            if (LLiability != 0)
            {
                liab = liab.Union(LoansLiability);
            }


            //-------stock in hand---------
            BalanceSheet stkhand = new BalanceSheet()
            {
                AccountsGroupID = 2,
                Particulars = "Stock-in-hand",
                Parent = 0,
                GpName = "Stock-in-hand",
                AccType = "asset",
                Debit = avalstock,
                Credit = 0,
                ParentName = "Current Assets",
                orderB = 2,
                Temp = LiabCount,
            };
            List<BalanceSheet> stkhands = new List<BalanceSheet>();
            stkhands.Add(stkhand);
            var stkh = stkhands;
            //-------------


            IEnumerable<BalanceSheet> asset = new List<BalanceSheet>();
            if (pftype == "Loss" && pfamt != 0)
            {
                asset = asset.Union(ProfitLoss);
            }
            asset = asset.Union(FixedAssets);

            asset = asset.Union(stkh);
            #region all Base account groups except Current Assets,Current Liabilities,Fixed Assets,Revenue Accounts,Capital Account
            long[] groups = { 1, 2, 3, 4, 7 };
            var ACCGroups = db.AccountsGroups.Where(a => a.Parent == 0 && !groups.Contains(a.AccountsGroupID)).ToList();

            /*  foreach (var acc in ACCGroups)
              {
                  var GroupItem = Common.GetChildAccGroupNew(acc.AccountsGroupID, acc.Name, "asset", null, to, funbal, 3);
                  decimal Amount = GroupItem != null ? (decimal)GroupItem.Sum(a => a.Debit - a.Credit) : 0;
                  var GroupParent = GroupItem.Where(a => a.Parent == 0 && (a.Debit != 0 || a.Credit != 0)).ToList();
                  var GroupChild = GroupItem.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
                  var GroupI = GroupParent.Union(GroupChild);
                  if (Amount > 0)
                  {
                      asset = asset.Union(GroupI);
                  }
                  else
                  {
                      liab = liab.Union(GroupI);
                  }
              }
             */ //------------------------------------------------------
            #endregion

            //diff. in open balance 
            #region diff. in open balance 
            decimal debitsum1 = (decimal)asset.Where(a => a.Parent == 0).Select(a => a.Debit).Sum();
            decimal debitsum2 = (decimal)asset.Where(a => a.Parent == 0).Select(a => a.Credit).Sum();

            decimal creditsum1 = (decimal)liab.Where(a => a.Parent == 0).Select(a => a.Debit).Sum();
            decimal creditsum2 = (decimal)liab.Where(a => a.Parent == 0).Select(a => a.Credit).Sum();

            decimal debitval = debitsum1 - debitsum2;
            decimal creditval = (creditsum2 + creditsum1);

            string type = "";
            if (debitval != 0 || creditval != 0)
            {
                decimal diffopnval = debitval - creditval;
                if (debitval < creditval)
                {
                    diffopnval = diffopnval * -1;
                    type = "asset";
                }
                else
                {
                    type = "liability";
                }

                if (Math.Round(diffopnval, 2) > 0)
                {
                    BalanceSheet sheet = new BalanceSheet()
                    {
                        AccountsGroupID = -11,
                        Particulars = "Diff In OpenBalance",
                        Parent = 0,
                        GpName = "Diff In OpenBalance",
                        AccType = type,
                        Debit = diffopnval,
                        Credit = 0,
                        ParentName = "OpenBalance",
                        orderB = 10,
                        Temp = 0,
                    };
                    List<BalanceSheet> OpnBalDiffer = new List<BalanceSheet>();
                    OpnBalDiffer.Add(sheet);


                    if (OpnBalDiffer[0].AccType == "liability")
                    {
                    }
                    else
                    {
                    }
                }
            }
            #endregion

            var count = getOrdering(liab.ToList());
            var counts = getOrdering(asset.ToList());

            //outer joins-for single row
            var leftOuterJoin = (from a in liab
                                 join b in asset on a.Temp equals b.Temp into astt
                                 from b in astt.DefaultIfEmpty()
                                 select new
                                 {
                                     AccountsGroupIdA = b?.AccountsGroupID,
                                     ParticularA = b?.Particulars,
                                     DebitA = b?.Debit,
                                     CreditA = b?.Credit,
                                     AccountsGroupIdL = a?.AccountsGroupID,
                                     ParticularL = a?.Particulars,
                                     DebitL = a?.Debit,
                                     CreditL = a?.Credit,

                                     ParentA = b?.Parent,
                                     ParentL = a?.Parent,
                                 });

            var rightOuterJoin = (from a in asset
                                  join b in liab on a.Temp equals b.Temp into lib
                                  from b in lib.DefaultIfEmpty()
                                  select new
                                  {
                                      AccountsGroupIdA = a?.AccountsGroupID,
                                      ParticularA = a?.Particulars,
                                      DebitA = a?.Debit,
                                      CreditA = a?.Credit,
                                      AccountsGroupIdL = b?.AccountsGroupID,
                                      ParticularL = b?.Particulars,
                                      DebitL = b?.Debit,
                                      CreditL = b?.Credit,

                                      ParentA = a?.Parent,
                                      ParentL = b?.Parent,
                                  });

            var full = leftOuterJoin.Union(rightOuterJoin);

            //change to view model
            vmodel.BalanceSheetDisplay = (from a in full
                                          where a.ParentA == 0 || a.ParentL == 0
                                          select new BalanceSheetDisplay
                                          {
                                              AccountGroupIDA = a?.AccountsGroupIdA,
                                              ParticularA = a?.ParticularA,
                                              DebitA = a?.DebitA,
                                              CreditA = a?.CreditA,
                                              AccountGroupIDL = a?.AccountsGroupIdL,
                                              ParticularL = a?.ParticularL,
                                              DebitL = a?.DebitL,
                                              CreditL = a?.CreditL,

                                              ParentA = a?.ParentA,
                                              ParentL = a?.ParentL,
                                          }).ToList();

            companySet();
            return View(vmodel);
        }

        //calcute child sum
        public int getSubTotal(int increA, IList<BalanceSheet> Data, int position)
        {
            decimal? TotalAmt = 0;
            decimal? subAmt = position == 1 ? Math.Abs(Convert.ToDecimal(Data.Sum(a => a.Credit - a.Debit))) : Math.Abs(Convert.ToDecimal(Data.Sum(a => a.Debit - a.Credit)));
            foreach (var item in Data)
            {
                decimal? SubAmt = 0;
                decimal? SubTotalCr = 0;
                decimal? SubTotalDr = 0;

                if (item.Parent == 0)//parent 
                {
                    if (position == 1)
                    {
                        SubAmt = Math.Abs(Convert.ToDecimal(Data.Sum(a => a.Credit - a.Debit)));
                    }
                    if (position == 2)
                    {
                        SubAmt = Math.Abs(Convert.ToDecimal(Data.Sum(a => a.Debit - a.Credit)));
                    }

                    item.Debit = SubAmt;
                    item.Credit = 0;
                    item.Temp = increA + 1;
                    increA++;
                }
                var chk = Data.Where(a => a.AccountsGroupID == item.Parent).Select(a => a.Parent).FirstOrDefault();
                if (chk == 0) //and 2nd child
                {
                    var tempval = item.Temp;
                    SubTotalDr = BindAccounts.getAmountDr(Data, item.AccountsGroupID, 0);
                    SubTotalCr = BindAccounts.getAmountCr(Data, item.AccountsGroupID, 0);

                    //item.Credit = item.Credit + SubTotalCr; //0;//
                    if (position == 1)
                    {
                        item.Debit = Math.Abs(Convert.ToDecimal((item.Credit + SubTotalCr) - (item.Debit + SubTotalDr)));
                    }
                    if (position == 2)
                    {
                        item.Debit = Math.Abs(Convert.ToDecimal((item.Debit + SubTotalDr) - (item.Credit + SubTotalCr)));
                    }
                    item.Credit = 0;//

                    TotalAmt += item.Debit;
                    item.Temp = increA + 1;
                    increA++;
                }
                if (chk > 0)
                {
                    item.Temp = 0;
                }

            }
            var parent = Data.Where(a => a.Parent == 0).FirstOrDefault();
            if (parent != null)
                parent.Debit = subAmt;

            return increA;

        }

        public int FindParent(IList<BalanceSheet> Data, int GpId, int position)
        {
            foreach (var items in Data)
            {
                if (items.AccountsGroupID == GpId)
                {
                    if (Data.Count >= 1)
                    {
                        items.orderB = 0;//for identify parent
                    }
                    else
                    {
                        items.orderB = 1;
                    }


                    if (position == 1)
                    {
                        items.Credit = Data.Sum(a => a.Credit - a.Debit);
                    }
                    if (position == 2)
                    {
                        items.Debit = Data.Sum(a => a.Debit - a.Credit);
                    }
                }
                else
                {
                    items.orderB = 1;
                }
            }
            return 0;
        }


        public int addToParentSubTotal(int increA, IList<BalanceSheet> Data, decimal Amt, int position)
        {
            decimal? TotalAmt = 0;
            foreach (var item in Data)
            {
                decimal? SubAmt = 0;
                decimal? SubTotalCr = 0;
                decimal? SubTotalDr = 0;

                if (item.Parent == 0)//parent 
                {
                    if (position == 1)
                    {
                        SubAmt = Data.Sum(a => a.Credit - a.Debit);
                    }
                    if (position == 2)
                    {
                        SubAmt = Data.Sum(a => a.Debit - a.Credit);
                    }

                    item.Debit = SubAmt;
                    item.Credit = 0;
                    item.Temp = increA + 1;
                    increA++;

                }
                var chk = Data.Where(a => a.AccountsGroupID == item.Parent).Select(a => a.Parent).FirstOrDefault();
                if (chk == 0) //and 2nd child
                {
                    var tempval = item.Temp;
                    SubTotalDr = BindAccounts.getAmountDr(Data, item.AccountsGroupID, 0);
                    SubTotalCr = BindAccounts.getAmountCr(Data, item.AccountsGroupID, 0);

                    //item.Credit = item.Credit + SubTotalCr; //0;//
                    if (position == 1)
                    {
                        item.Debit = (item.Credit + SubTotalCr) - (item.Debit + SubTotalDr);
                    }
                    if (position == 2)
                    {
                        item.Debit = (item.Debit + SubTotalDr) - (item.Credit + SubTotalCr);
                    }
                    item.Credit = 0;//

                    TotalAmt += item.Debit;
                    item.Temp = increA + 1;
                    increA++;
                }

            }
            var parent = Data.Where(a => a.Parent == 0).FirstOrDefault();

            parent.Debit = TotalAmt + Amt;

            return increA;

        }

        public int getOrdering(IList<BalanceSheet> Data)
        {
            var count = 0;
            foreach (var item in Data)
            {
                count++;
                item.Temp = count;
            }
            return count;
        }
        public int getOrderingPF(IEnumerable<ProfitAndLoss> Data)
        {
            var count = 0;
            foreach (var item in Data)
            {
                count++;
                item.Temp = count;
            }
            return count;
        }


        public int chkDebitCredit(IList<BalanceSheet> Data)
        {
            var count = 0;
            foreach (var item in Data)
            {
                count++;
                if (item.Debit < item.Credit)
                {
                    item.AccType = "liability";
                }
                else
                {
                    item.AccType = "asset";
                }
            }
            return count;
        }

        public int getTotalDrCr(int increA, IList<BalanceSheet> Data)
        {
            decimal? TotalAmt = 0;
            foreach (var item in Data)
            {
                decimal? SubAmt = 0;
                decimal? SubTotalCr = 0;
                decimal? SubTotalDr = 0;

                if (item.Parent == 0)//parent 
                {
                    SubAmt = Data.Sum(a => a.Debit - a.Credit);

                    SubAmt = SubAmt < 0 ? (0 - SubAmt) : SubAmt;
                    item.Debit = SubAmt;
                    item.Temp = increA + 1;
                    increA++;
                }
                var chk = Data.Where(a => a.AccountsGroupID == item.Parent).Select(a => a.Parent).FirstOrDefault();
                if (chk == 0) //and 2nd child
                {
                    var tempval = item.Temp;
                    SubTotalDr = BindAccounts.getAmountDr(Data, item.AccountsGroupID, 0);
                    SubTotalCr = BindAccounts.getAmountCr(Data, item.AccountsGroupID, 0);

                    if ((item.Debit + SubTotalDr) > (item.Credit + SubTotalCr))
                    {
                        item.Debit = (item.Debit + SubTotalDr) - (item.Credit + SubTotalCr);
                        item.Credit = 0;

                        TotalAmt += item.Debit;
                    }
                    else
                    {
                        item.Credit = (item.Credit + SubTotalCr) - (item.Debit + SubTotalDr);
                        item.Debit = 0;

                        TotalAmt += item.Credit;
                    }
                    item.Temp = increA + 1;
                    increA++;

                }

            }
            var parent = Data.Where(a => a.Parent == 0).FirstOrDefault();
            parent.Debit = TotalAmt;

            return increA;

        }

        public int getSubTotalTrial(int increA, IList<BalanceSheet> Data)
        {
            decimal? TotalAmt = 0;
            decimal? subAmt = Data.Where(a => a.Parent == 0).Sum(a => a.Debit - a.Credit);
            foreach (var item in Data)
            {
                decimal? SubAmt = 0;
                decimal? SubTotalCr = 0;
                decimal? SubTotalDr = 0;


                if (item.Parent == 0)//parent 
                {
                    var SCr = Data.Sum(a => a.Credit);
                    var SDr = Data.Sum(a => a.Debit);

                    if (SDr > SCr)
                    {
                        SubAmt = Data.Sum(a => a.Debit - a.Credit);
                        SubAmt = SubAmt < 0 ? (0 - SubAmt) : SubAmt;
                        item.Debit = SubAmt;
                        item.Credit = 0;
                    }
                    else
                    {
                        SubAmt = Data.Sum(a => a.Credit - a.Debit);
                        SubAmt = SubAmt < 0 ? (0 - SubAmt) : SubAmt;
                        item.Credit = SubAmt;
                    }

                    item.Temp = increA + 1;
                    increA++;
                }
                var chk = Data.Where(a => a.AccountsGroupID == item.Parent).Select(a => a.Parent).FirstOrDefault();
                if (chk == 0) //and 2nd child
                {
                    var tempval = item.Temp;
                    SubTotalDr = BindAccounts.getAmountDr(Data, item.AccountsGroupID, 0);
                    SubTotalCr = BindAccounts.getAmountCr(Data, item.AccountsGroupID, 0);

                    if (item.Credit > item.Debit)
                    {
                        item.Credit = (item.Credit + SubTotalCr) - (item.Debit + SubTotalDr);
                        item.Debit = 0;//item.Debit + SubTotalDr;
                        TotalAmt += item.Credit;
                    }
                    else
                    {
                        item.Debit = (item.Debit + SubTotalDr) - (item.Credit + SubTotalCr);
                        item.Credit = 0;
                        TotalAmt += item.Debit;
                    }

                    if (item.Credit < 0)
                    {
                        item.Debit = item.Credit * -1;
                        item.Credit = 0;
                    }
                    if (item.Debit < 0)
                    {
                        item.Credit = item.Debit * -1;
                        item.Debit = 0;
                    }


                    item.Temp = increA + 1;
                    increA++;
                }

            }

            return increA;

        }


        //Profit for the period
        public Dictionary<string, object> getProftAndLoss(string fromdate, string todate, int fun)
        {
            DateTime? fdate = null;
            DateTime? tdate = null;
            db.SetCommandTimeOut(60 * 60);
            fun = fromdate != "" ? 2 : 1;

            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            var isemirtech = db.companys.Any(o => o.CPName.Contains("EMIRTECH TECHNOLOGY"));
            var openstock = fromdate != "" ? getOpeningStock(fromdate, "open") : 0;
            var closestock =  fromdate != "" ? getOpeningStock(todate, "close") : getOpeningStock(todate, "open");
            if (fromdate == "")
            {

            }

            //sales price.
            var sprices = (from i in db.AccountsTransactions
                           where (fromdate == "" || EF.Functions.DateDiffDay(i.Date, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0) && i.Account == 1
                           && i.Purpose == "Sale" && i.Status == null
                           //group i by i.SalesEntryId into g
                           select new
                           {
                               Total = i.Credit
                           }).ToList();


            decimal sprice = sprices != null ? sprices.Sum(a => a.Total) : 0;


            //sales return price.
            var sretprices = (from i in db.AccountsTransactions
                              where (fromdate == "" || EF.Functions.DateDiffDay(i.Date, fdate) <= 0) &&
                              (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0)
                              && i.Account == 1 && i.Purpose == "Sale Return"
                              && i.Status == null
                              select new
                              {
                                  Total = i.Debit
                              }).ToList();
            decimal sretprice = (sretprices != null) ? sretprices.Sum(a => a.Total) : 0;



            //purchase price
            var pprices = (from i in db.AccountsTransactions
                           where (fromdate == "" || EF.Functions.DateDiffDay(i.Date, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0)
                            && i.Account == 2 && i.Purpose == "Purchase"
                            && i.Status == null
                           select new
                           {
                               Total = i.Debit
                           }).ToList();
            decimal pprice = (pprices != null) ? pprices.Sum(a => a.Total) : 0;


            //purchase return price
            var pretprices = (from i in db.AccountsTransactions
                              where (fromdate == "" || EF.Functions.DateDiffDay(i.Date, fdate) <= 0) &&
                              (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0)
                              && i.Account == 2 && i.Purpose == "Purchase Return"
                              && i.Status == null
                              select new
                              {
                                  Total = i.Credit
                              }).ToList();
            decimal pretprice = (pretprices != null) ? pretprices.Sum(a => a.Total) : 0;



            //direct expenses
            var dexpense = Common.GetChildAccGroup(29, "Direct Expenses", "liability", fdate, tdate, fun, 1);

            //in direct expenses
            var indexpense = Common.GetChildAccGroup(30, "InDirect Expenses", "liability", fdate, tdate, fun, 1);

            //direct income
            var dirincome = Common.GetChildAccGroup(31, "Direct Income", "asset", fdate, tdate, fun, 1);

            //in direct income
            var indirincome = Common.GetChildAccGroup(32, "InDirect Income", "asset", fdate, tdate, fun, 1);



      








            var DirectExp = dexpense != null ? (decimal)dexpense.Sum(a => a.Debit - a.Credit) : 0;
            var InDirectExp = indexpense != null ? (decimal)indexpense.Sum(a => a.Debit - a.Credit) : 0;
            var DirectIncome = dirincome != null ? (decimal)dirincome.Sum(a => a.Credit - a.Debit) : 0;
            var InDirectIncome = indirincome != null ? (decimal)indirincome.Sum(a => a.Credit - a.Debit) : 0;

            InDirectExp = InDirectExp ;

            var Sales = sprice - sretprice;
            var Purchase = pprice - pretprice;
            if(isemirtech && todate=="31-12-2025")
            {
                openstock = Convert.ToDecimal("99329.23");
                closestock = Convert.ToDecimal("97484.40");

            }
            bool isquicknet = db.companys.Any(o => o.CPName.Contains("QUICK NET COMPUTERS"));
            if (isquicknet && todate == "31-12-2025")
            {
                openstock = Convert.ToDecimal("1404265.35");
                closestock = Convert.ToDecimal("1688792.52");

            }
            var Debit = openstock + Purchase + DirectExp;
            var Credit = closestock + Sales + DirectIncome;

            decimal TotalDr = 0;
            decimal TotalCr = 0;
            if ((decimal)Debit < (decimal)Credit)
            {
                TotalDr = (decimal)Credit - (decimal)Debit;
            }
            else
            {
                TotalCr = (decimal)Debit - (decimal)Credit;
            }
            TotalDr = (decimal)TotalDr + (decimal)InDirectIncome;
            TotalCr = (decimal)TotalCr + (decimal)InDirectExp;
            decimal Profit = 0;
            string type = "";
            if ((decimal)TotalDr > (decimal)TotalCr)
            {//profit
                Profit = (decimal)TotalDr - (decimal)TotalCr;
                type = "Profit";
            }
            else
            {//loss
                Profit = (decimal)TotalCr - (decimal)TotalDr;
                type = "Loss";
            }

            var profit = new Dictionary<string, object>();
            profit.Add("type", type);
            profit.Add("amount", Profit);
            profit.Add("stock", closestock);

            return profit;
        }

        //opening stock
        public decimal getavgpurchase(long itemid, string ondate)
        {
            DateTime ondates = DateTime.Parse(ondate, new CultureInfo("en-GB"));
            var stock = db.Items.Where(i => i.ItemID == itemid).Select(o => new { o.PurchasePrice, o.OpeningStock }).FirstOrDefault();
            decimal? opstockvalue = stock.OpeningStock * stock.PurchasePrice;
            var avgpurchaseprice = (from o in db.PEItemss
                                    join p in db.PurchaseEntrys on o.PurchaseEntry equals p.PurchaseEntryId
                                    join b in db.Items on o.Item equals b.ItemID
                                    where o.Item == itemid &&
                                    (ondate == "" || EF.Functions.DateDiffDay(p.PEDate, ondates) >= 0)
                                    group new { o.ItemUnitPrice, o.ItemQuantity, b.ItemUnitID, b.SubUnitId, o.ItemUnit, b.ConFactor } by new { o.Item, o.ItemUnitPrice, o.ItemUnit } into g
                                    select new
                                    {

                                        PriTotal = (decimal?)g.Where(x => x.ItemUnit == g.FirstOrDefault().ItemUnit && x.ItemUnitPrice == g.FirstOrDefault().ItemUnitPrice).Sum(x => x.ItemQuantity) ?? 0,
                                        SubTotal = (decimal?)g.Where(x => x.ItemUnit == g.FirstOrDefault().SubUnitId && x.ItemUnitPrice == g.FirstOrDefault().ItemUnitPrice).Sum(x => x.ItemQuantity) ?? 0,
                                        PriTotalavg = (g.Key.ItemUnit == g.FirstOrDefault().ItemUnitID) ? (decimal)g.Key.ItemUnitPrice : (decimal)g.Key.ItemUnitPrice / g.FirstOrDefault().ConFactor,
                                        //SubTotalavg = (decimal?)g.Where(x => x.ItemUnit == g.FirstOrDefault().SubUnitId && x.ItemUnitPrice == g.FirstOrDefault().ItemUnitPrice).Average(x => x.ItemUnitPrice) ?? 0,
                                        confactor = g.FirstOrDefault().ConFactor
                                    }).ToList();
            decimal avpricetotal = 0;
            decimal totalstock = 0;
            for (int i = 0; i < avgpurchaseprice.Count(); i++)
            {
                decimal priceavgp = 0;
                decimal avgprice = 0;
                decimal stocks = 0;

                if (avgpurchaseprice != null)
                {
                    priceavgp = avgpurchaseprice[i].PriTotalavg;
                    stocks = avgpurchaseprice[i].PriTotal + (avgpurchaseprice[i].SubTotal / avgpurchaseprice[i].confactor);
                    ////priceavgs = avgpurchaseprice[i].SubTotalavg / avgpurchaseprice[i].confactor;
                    avgprice = 0;
                    if (priceavgp != 0)
                    {
                        avgprice = stocks * priceavgp;
                    }
                    totalstock = totalstock + stocks;
                    avpricetotal = avpricetotal + avgprice;
                }
            }
            decimal avgp = 0;
            if (avgpurchaseprice.Count() != 0 && totalstock != 0)   // Calc fix: guard divisor (was checking row count, not totalstock) — prevents DivideByZeroException when rows net to zero qty.
            {
                avgp = avpricetotal / totalstock;
            }
            return avgpurchaseprice.Count() == 0 ? stock.PurchasePrice : avgp;
        }

        //Function To Get Item Purchase Price (If Any Exists With in SEDate)
        public decimal GetItemPurchasePrice(long? ItemId, DateTime? SEDate, long? mc, long? salesentrydetailid, bool moment = false)
        {
            decimal confactor = 1;
            var sellingunit = db.SEItemss.Where(o => o.SEItemsId == salesentrydetailid).Select(o => o.ItemUnit).FirstOrDefault();
            var items = db.Items.Where(o => o.ItemID == ItemId).FirstOrDefault();
            if (items.SubUnitId == sellingunit)
            {
                confactor = items.ConFactor;
            }
            DateTime fromdate = SEDate.Value.AddMonths(-5);
            moment = false;

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
                return items.PurchasePrice / confactor;
            }
            if (newstocktransfer != null && NewPurPrice != null)
            {
                if (newstocktransfer.date > NewPurPrice.date)
                {
                    return newstocktransfer.unitprice / confactor;
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

        public decimal getOpeningStockmcshowroom(string ondate, string type, long? mc)
        {
            db.SetCommandTimeOut(60 * 60);
            int recordsTotal = 0;
            DateTime? ondates = null;
            if (ondate != "")
            {
                if (type == "open")
                {
                    DateTime opendate = DateTime.Parse(ondate, new CultureInfo("en-GB"));
                    DateTime seldate = opendate.AddDays(-1);
                    ondates = DateTime.Parse(seldate.ToString("dd-MM-yyyy"), new CultureInfo("en-GB"));
                }
                else
                {
                    ondates = DateTime.Parse(ondate, new CultureInfo("en-GB"));
                }
            }
            decimal stkval = 0;
            var ddlMC = mc;
            var IVMethod = db.EnableSettings.Where(a => a.EnableType == "InventoryMethod").Select(x => x.TypeValue).FirstOrDefault();
            if (IVMethod == "Average")
            {


                var sprices = (from i in db.SalesEntrys
                               join j in db.SEItemss on i.SalesEntryId equals j.SalesEntry
                               where i.SEDate <= ondates &&
                              i.MaterialCenter == ddlMC
                              && j.Item != 30018 && j.Item != 75021
                               select new
                               {
                                   j.ItemQuantity,
                                   j.Item,
                                   i.SalesEntryId,
                                   i.SEDate,
                                   j.SEItemsId



                               }).ToList().Select(o => new
                               {

                                   Total = (o.ItemQuantity - getsalesreturn(o.SalesEntryId, o.Item)) * GetItemPurchasePrice(o.Item, o.SEDate, ddlMC, o.SEItemsId, false)


                               }).ToList();


                decimal sprice = sprices != null ? sprices.Sum(a => a.Total) : 0;



                decimal sretprice = 0;



                //purchase price
                var pprices = (from i in db.PurchaseEntrys
                               join j in db.PEItemss on i.PurchaseEntryId equals j.PurchaseEntry
                               where i.PEDate <= ondates
                                && i.MaterialCenter == ddlMC
                               select new
                               {
                                   i.PurchaseEntryId,
                                   Total = i.PEGrandTotal,
                               }).Distinct().ToList();
                decimal pprice = (pprices != null) ? pprices.Sum(a => a.Total) : 0;


                //purchase return price
                var pretprices = (from i in db.PurchaseReturns
                                  join j in db.PRItemss on i.PurchaseReturnId equals j.PurchaseReturnId
                                  where i.PRDate <= ondates
                                   && i.MaterialCenter == ddlMC

                                  select new
                                  {
                                      i.PurchaseReturnId,
                                      Total = i.PRGrandTotal
                                  }).Distinct().ToList();
                decimal pretprice = (pretprices != null) ? pretprices.Sum(a => a.Total) : 0;
                var stockin = (from i in db.StockTransfers
                               join j in db.StockTransferItems on i.Id equals j.StockTransferId
                               where i.Date <= ondates && i.MCTo == ddlMC

                               select new
                               {
                                   i.Id,
                                   //Total = (j.Unit == k.ItemUnitID) ? j.Quantity * k.PurchasePrice : j.Quantity * k.PurchasePrice / k.ConFactor
                                   Total = i.TotalAmount
                               }).Distinct().ToList();
                decimal stockinprice = (stockin != null) ? stockin.Sum(a => a.Total) : 0;
                var stockout = (from i in db.StockTransfers
                                join j in db.StockTransferItems on i.Id equals j.StockTransferId
                                where i.Date <= ondates &&
                                 i.MCFrom == ddlMC

                                select new
                                {
                                    i.Id,
                                    //Total = (j.Unit == k.ItemUnitID) ? j.Quantity * k.PurchasePrice : j.Quantity * k.PurchasePrice / k.ConFactor
                                    Total = i.TotalAmount
                                }).Distinct().ToList();
                decimal stockoutprice = (stockout != null) ? stockout.Sum(a => a.Total) : 0;
                var assetinvtransfer = (from i in db.AssetToInventoryMasters
                                        join j in db.AssetToInventoryDetails on i.EntryId equals j.EntryId
                                        join k in db.Items on j.RefItemId equals k.ItemID
                                        where i.EntryDate <= ondates &&
                                         i.McFromId == ddlMC

                                        select new
                                        {
                                            Total = j.Price * j.Quantity

                                        }).Distinct().ToList();
                decimal assetin = (assetinvtransfer != null) ? assetinvtransfer.Sum(a => a.Total) : 0;
                var assettransfer = (from i in db.AssetTransferMasters
                                     join j in db.AssetTransferDetails on i.AssetEntryId equals j.AssetEntryId
                                     join k in db.Items on j.RefItemId equals k.ItemID
                                     where i.AssetEntryDate <= ondates &&
                                      i.McFromId == ddlMC

                                     select new
                                     {
                                         Total = j.Price * j.Quantity
                                     }).Distinct().ToList();
                decimal assetout = (assettransfer != null) ? assettransfer.Sum(a => a.Total) : 0;

                var damagein = (from i in db.StockAdjustments

                                where i.AdjDate <= ondates &&

                        i.MaterialCenter == mc
                        &&
                        i.AdjustmentType == AdjustmentType.Add


                                select new
                                {
                                    Total = i.PurchaseRate * i.ItemQuantity
                                }).ToList();
                decimal damageintotal = (damagein != null) ? damagein.Sum(a => a.Total) : 0;

                var damageout = (from i in db.StockAdjustments
                                 where i.AdjDate <= ondates &&

                          i.MaterialCenter == mc
                          &&
                          i.AdjustmentType == AdjustmentType.Less


                                 select new
                                 {
                                     Total = i.PurchaseRate * i.ItemQuantity
                                 }).ToList();
                decimal damageouttotal = (damageout != null) ? damageout.Sum(a => a.Total) : 0;


                var Sales = sprice + stockoutprice + damageouttotal + assetout - sretprice;
                var Purchase = stockinprice + pprice + damageintotal + assetin - pretprice;
                stkval = Sales - Purchase;


            }
            return stkval;

        }
        public decimal getOpeningStockmc(string ondate, string type, long? mc)
        {
            db.SetCommandTimeOut(60 * 60);
            int recordsTotal = 0;
            DateTime? ondates = null;
            if (ondate != "")
            {
                if (type == "open")
                {
                    DateTime opendate = DateTime.Parse(ondate, new CultureInfo("en-GB"));
                    DateTime seldate = opendate.AddDays(-1);
                    ondates = DateTime.Parse(seldate.ToString("dd-MM-yyyy"), new CultureInfo("en-GB"));
                }
                else
                {
                    ondates = DateTime.Parse(ondate, new CultureInfo("en-GB"));
                }
            }
            decimal stkval = 0;
            var IVMethod = db.EnableSettings.Where(a => a.EnableType == "InventoryMethod").Select(x => x.TypeValue).FirstOrDefault();
            if (IVMethod == "Average")
            {
                var itmids = db.Items.Select(o => o.ItemID).ToList();

                List<StockDetails> data = new List<StockDetails>();
                foreach (var it in itmids)
                {
                    var selitem = new SqlParameter("@ItemId", it);
                    var selmc = new SqlParameter("@MCId", mc);
                    var brand = new SqlParameter("@BrandId", "");
                    var stkble = new SqlParameter("@Stockble", 1);
                    var catgry = new SqlParameter("@CategoryId", "");
                    var fromdate = new SqlParameter("@fromdate", "");
                    var todate = new SqlParameter("@todate", ondates);
                    var stype = new SqlParameter("@Stype", "1");

                    var dataadd = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethodcasa @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().OrderBy(a => a.IItemName).FirstOrDefault();


                    data.Add(dataadd);

                    var totStockval = data.Select(a => a.ITotalStockValue).Sum();
                    var IVMethod2 = db.EnableSettings.Where(a => a.EnableType == "StockValue").Select(x => x.TypeValue).FirstOrDefault();
                    var tqty = data.Select(a => a.ITotalQty).Sum();
                    if (totStockval > 0)
                        stkval = stkval + (decimal)totStockval;

                }
                var sum = data.Sum(o => o.ITotalStockValue);
                stkval = (decimal)sum;

            }

            return stkval;

        }

        public decimal getOpeningStock(string ondate, string type)
        {
            db.SetCommandTimeOut(60 * 60);
            int recordsTotal = 0;
            DateTime? ondates = null;
            if (ondate != "")
            {
                if (type == "open")
                {
                    DateTime opendate = DateTime.Parse(ondate, new CultureInfo("en-GB"));
                    DateTime seldate = opendate.AddDays(-1);
                    ondates = DateTime.Parse(seldate.ToString("dd-MM-yyyy"), new CultureInfo("en-GB"));
                }
                else
                {
                    ondates = DateTime.Parse(ondate, new CultureInfo("en-GB"));
                }
            }
            decimal stkval = 0;
            var IVMethod = db.EnableSettings.Where(a => a.EnableType == "InventoryMethod").Select(x => x.TypeValue).FirstOrDefault();
            if (IVMethod == "Average")
            {
               
                db.SetCommandTimeOut(60 * 60);

                var selitem = new SqlParameter("@ItemId", "");
                var selmc = new SqlParameter("@MCId", "0");
                var brand = new SqlParameter("@BrandId", "");
                var stkble = new SqlParameter("@Stockble", 1);
                var catgry = new SqlParameter("@CategoryId", "");
                var fromdate = new SqlParameter("@fromdate", "");
                var todate = new SqlParameter("@todate", ondates);
                var stype = new SqlParameter("@Stype", "1");

                var dataadd = db.Database.SqlQueryDedup<StockDetails>("SP_AVCOMethod @ItemId,@MCId,@BrandId,@Stockble,@CategoryId,@fromdate,@todate,@stype", selitem, selmc, brand, stkble, catgry, fromdate, todate, stype).AsEnumerable().Where(o=>o.ITotalStockValue>0).ToList();




                var sum = (decimal)dataadd.Sum(o => o.ITotalStockValue);


                return sum;
            }
            else
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
                         where b.KeepStock == true
                         orderby b.ItemID ascending
                         select new
                         {
                             b.ItemID,
                             ItemCode = b.ItemCode,
                             ItemName = b.ItemName,
                             b.ItemUnitID,
                             b.SubUnitId,
                             PriUnit = c.ItemUnitName,
                             SubUnit = d.ItemUnitName,
                             ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
                             OpeningStock = b.OpeningStock != null ? b.OpeningStock : 0,
                             b.KeepStock,

                             Purchase = (from i in db.PEItemss
                                         join j in db.PurchaseEntrys on i.PurchaseEntry equals j.PurchaseEntryId
                                         where i.Item == b.ItemID && (ondate == "" || EF.Functions.DateDiffDay(j.PEDate, ondates) >= 0)
                                         && j.PurType != PurchaseHireType.CrossHire
                                         group i by i.ItemId into g
                                         select new
                                         {
                                             PriTotal = (decimal?)g.Where(x => x.ItemUnit == b.ItemUnitID).Sum(x => x.ItemQuantity) ?? 0,
                                             SubTotal = (decimal?)g.Where(x => x.ItemUnit == b.SubUnitId).Sum(x => x.ItemQuantity) ?? 0
                                         }).FirstOrDefault(),



                             Sale = (from i in db.SEItemss
                                     join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                                     where i.Item == b.ItemID && (ondate == "" || EF.Functions.DateDiffDay(j.SEDate, ondates) >= 0)
                                     && j.SaleType != SaleType.Hire
                                     group i by i.ItemId into g
                                     select new
                                     {
                                         PriTotal = (decimal?)g.Where(x => x.ItemUnit == b.ItemUnitID).Sum(x => x.ItemQuantity) ?? 0,
                                         SubTotal = (decimal?)g.Where(x => x.ItemUnit == b.SubUnitId).Sum(x => x.ItemQuantity) ?? 0
                                     }).FirstOrDefault(),


                             PReturn = (from i in db.PRItemss
                                        join j in db.PurchaseReturns on i.PurchaseReturnId equals j.PurchaseReturnId
                                        where i.Item == b.ItemID && (ondate == "" || EF.Functions.DateDiffDay(j.PRDate, ondates) >= 0)
                                        && j.PurType != PurchaseHireType.CrossHire
                                        group i by i.Item into g
                                        select new
                                        {
                                            PriTotal = (decimal?)g.Where(x => x.ItemUnit == b.ItemUnitID).Sum(x => x.ItemQuantity) ?? 0,
                                            SubTotal = (decimal?)g.Where(x => x.ItemUnit == b.SubUnitId).Sum(x => x.ItemQuantity) ?? 0
                                        }).FirstOrDefault(),


                             SReturn = (from i in db.SRItemss
                                        join j in db.SalesReturns on i.SalesReturnId equals j.SalesReturnId
                                        where i.Item == b.ItemID && (ondate == "" || EF.Functions.DateDiffDay(j.SRDate, ondates) >= 0)
                                        && j.SaleType != SaleType.Hire
                                        group i by i.Item into g
                                        select new
                                        {
                                            PriTotal = (decimal?)g.Where(x => x.ItemUnit == b.ItemUnitID).Sum(x => x.ItemQuantity) ?? 0,
                                            SubTotal = (decimal?)g.Where(x => x.ItemUnit == b.SubUnitId).Sum(x => x.ItemQuantity) ?? 0

                                        }).FirstOrDefault(),


                             //stock adjustment---
                             AddAdj = (from i in db.StockAdjustments
                                       where i.ItemID == b.ItemID && i.AdjustmentType == AdjustmentType.Add && (ondate == "" || EF.Functions.DateDiffDay(i.AdjDate, ondates) >= 0)
                                       group i by i.ItemID into g
                                       select new
                                       {
                                           PriTotal = (decimal?)g.Where(x => x.ItemUnitID == b.ItemUnitID).Sum(x => x.ItemQuantity) ?? 0,
                                           SubTotal = (decimal?)g.Where(x => x.ItemUnitID == b.SubUnitId).Sum(x => x.ItemQuantity) ?? 0
                                       }).FirstOrDefault(),

                             LessAdj = (from i in db.StockAdjustments
                                        where i.ItemID == b.ItemID && i.AdjustmentType == AdjustmentType.Less && (ondate == "" || EF.Functions.DateDiffDay(i.AdjDate, ondates) >= 0)
                                        group i by i.ItemID into g
                                        select new
                                        {
                                            PriTotal = (decimal?)g.Where(x => x.ItemUnitID == b.ItemUnitID).Sum(x => x.ItemQuantity) ?? 0,
                                            SubTotal = (decimal?)g.Where(x => x.ItemUnitID == b.SubUnitId).Sum(x => x.ItemQuantity) ?? 0
                                        }).FirstOrDefault(),


                             // production ----
                             // main item

                             ProdItem = (from i in db.GeneratedItem
                                         where i.Item == b.ItemID //&&i.ItemUnit == b.ItemUnitID &&
                                         group i by i.Item into g
                                         select new
                                         {
                                             PriTotal = (decimal?)g.Where(x => x.Unit == b.ItemUnitID).Sum(x => x.Qty) ?? 0,
                                             SubTotal = (decimal?)g.Where(x => x.Unit == b.SubUnitId).Sum(x => x.Qty) ?? 0
                                         }).FirstOrDefault(),

                             // compined item
                             ProdCItem = (from i in db.ProItems
                                          where i.ItemId == b.ItemID //&&i.ItemUnit == b.ItemUnitID &&
                                          group i by i.ItemId into g
                                          select new
                                          {
                                              PriTotal = (decimal?)g.Where(x => x.Unit == b.ItemUnitID).Sum(x => x.Quantity) ?? 0,
                                              SubTotal = (decimal?)g.Where(x => x.Unit == b.SubUnitId).Sum(x => x.Quantity) ?? 0
                                          }).FirstOrDefault(),

                             // unassemble -----
                             // main item

                             UnItem = (from i in db.ConsumedItem
                                       where i.Item == b.ItemID //&&i.ItemUnit == b.ItemUnitID &&
                                       group i by i.Item into g
                                       select new
                                       {
                                           PriTotal = (decimal?)g.Where(x => x.Unit == b.ItemUnitID).Sum(x => x.Qty) ?? 0,
                                           SubTotal = (decimal?)g.Where(x => x.Unit == b.SubUnitId).Sum(x => x.Qty) ?? 0
                                       }).FirstOrDefault(),

                             // compined item
                             UnCItem = (from i in db.UnassembleItems
                                        where i.ItemId == b.ItemID  //&&i.ItemUnit == b.ItemUnitID &&
                                        group i by i.ItemId into g
                                        select new
                                        {
                                            PriTotal = (decimal?)g.Where(x => x.Unit == b.ItemUnitID).Sum(x => x.Quantity) ?? 0,
                                            SubTotal = (decimal?)g.Where(x => x.Unit == b.SubUnitId).Sum(x => x.Quantity) ?? 0
                                        }).FirstOrDefault(),


                             ////stktrns
                             //StkTrn  = (from i in db.StockTransferItems
                             //           group i by i.Item into g
                             //               PriTotal = (decimal?)g.Where(x => x.Unit == b.ItemUnitID).Sum(x => x.Quantity) ?? 0,
                             //               SubTotal = (decimal?)g.Where(x => x.Unit == b.SubUnitId).Sum(x => x.Quantity) ?? 0

                             //hire
                             ////Hire = (from i in db.SEItemss
                             ////        join j in db.SalesEntrys on i.SalesEntry equals j.SalesEntryId
                             ////        where i.Item == b.ItemID && (ondate == "" || EF.Functions.DateDiffDay(j.SEDate, ondates) >= 0)
                             ////        && j.SaleType == SaleType.Hire
                             ////        group i by i.ItemId into g
                             ////        select new
                             ////        {
                             ////            PriTotal = (decimal?)g.Where(x => x.ItemUnit == b.ItemUnitID).Sum(x => x.ItemQuantity) ?? 0,
                             ////            SubTotal = (decimal?)g.Where(x => x.ItemUnit == b.SubUnitId).Sum(x => x.ItemQuantity) ?? 0
                             ////        }).FirstOrDefault(),

                             ////HReturn = (from i in db.HrItems
                             ////           join j in db.HireReturns on i.Hr equals j.HireReturnId
                             ////           where i.Item == b.ItemID && (ondate == "" || EF.Functions.DateDiffDay(j.Date, ondates) >= 0) && j.RtType == "Return"
                             ////           group i by i.ItemId into g
                             ////           select new
                             ////           {
                             ////               PriTotal = (decimal?)g.Where(x => x.ItemUnit == b.ItemUnitID).Sum(x => x.ItemQuantity) ?? 0,
                             ////               SubTotal = (decimal?)g.Where(x => x.ItemUnit == b.SubUnitId).Sum(x => x.ItemQuantity) ?? 0
                             ////           }).FirstOrDefault(),

                             ////HMiss = (from i in db.HrItems
                             ////         join j in db.HireReturns on i.Hr equals j.HireReturnId
                             ////         where i.Item == b.ItemID && (ondate == "" || EF.Functions.DateDiffDay(j.Date, ondates) >= 0) && j.RtType == "Missing"
                             ////         group i by i.ItemId into g
                             ////         select new
                             ////         {
                             ////             PriTotal = (decimal?)g.Where(x => x.ItemUnit == b.ItemUnitID).Sum(x => x.ItemQuantity) ?? 0,
                             ////             SubTotal = (decimal?)g.Where(x => x.ItemUnit == b.SubUnitId).Sum(x => x.ItemQuantity) ?? 0
                             ////         }).FirstOrDefault(),

                             cost = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnitPrice : b.PurchasePrice),//(decimal?)f.ItemUnitPrice,
                             costu = (decimal?)((f.ItemUnitPrice != null) ? f.ItemUnit : b.ItemUnitID),//(decimal?)f.ItemUnit,
                             b.PurchasePrice
                         }).Distinct().AsEnumerable().Select(k => new
                         {
                             k.ItemID,
                             k.ItemCode,
                             k.ItemName,
                             k.ItemUnitID,
                             k.SubUnitId,
                             k.PriUnit,
                             k.SubUnit,
                             k.OpeningStock,
                             k.ConFactor,

                             PriPurchase = (k.Purchase != null) ? k.Purchase.PriTotal : 0,
                             SubPurchase = (k.Purchase != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.Purchase.SubTotal : 0,

                             PriSale = (k.Sale != null) ? k.Sale.PriTotal : 0,
                             SubSale = (k.Sale != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.Sale.SubTotal : 0,

                             PriPReturn = (k.PReturn != null) ? k.PReturn.PriTotal : 0,
                             SubPReturn = (k.PReturn != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.PReturn.SubTotal : 0,

                             PriSReturn = (k.SReturn != null) ? k.SReturn.PriTotal : 0,
                             SubSReturn = (k.SReturn != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.SReturn.SubTotal : 0,

                             //PriHDNote = (k.HDNote != null) ? k.HDNote.PriTotal : 0,
                             //SubHDNote = (k.HDNote != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.HDNote.SubTotal : 0,

                             //PriRetNote = (k.RetNote != null) ? k.RetNote.PriTotal : 0,
                             //SubRetNote = (k.RetNote != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.RetNote.SubTotal : 0,

                             //PriHireMiss = (k.HireMiss != null) ? k.HireMiss.PriTotal : 0,
                             //SubHireMiss = (k.HireMiss != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.HireMiss.SubTotal : 0,

                             PriAddAdj = (k.AddAdj != null) ? k.AddAdj.PriTotal : 0,
                             SubAddAdj = (k.AddAdj != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.AddAdj.SubTotal : 0,

                             PriLessAdj = (k.LessAdj != null) ? k.LessAdj.PriTotal : 0,
                             subLessAdj = (k.LessAdj != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.LessAdj.SubTotal : 0,

                             PriProdItem = (k.ProdItem != null) ? k.ProdItem.PriTotal : 0,
                             SubProdItem = (k.ProdItem != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.ProdItem.SubTotal : 0,

                             PriUnCItem = (k.UnCItem != null) ? k.UnCItem.PriTotal : 0,
                             SubUnCItem = (k.UnCItem != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.UnCItem.SubTotal : 0,

                             PriProdCItem = (k.ProdCItem != null) ? k.ProdCItem.PriTotal : 0,
                             SubProdCItem = (k.ProdCItem != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.ProdCItem.SubTotal : 0,

                             PriUnItem = (k.UnItem != null) ? k.UnItem.PriTotal : 0,
                             SubUnItem = (k.UnItem != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.UnItem.SubTotal : 0,

                             ////hire
                             //PriHire = (k.Hire != null) ? k.Hire.PriTotal : 0,
                             //SubHire = (k.Hire != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.Hire.SubTotal : 0,

                             ////Hreturn
                             //PriHReturn = (k.HReturn != null) ? k.HReturn.PriTotal : 0,
                             //SubHReturn = (k.HReturn != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.HReturn.SubTotal : 0,

                             ////Hmiss
                             //PriHMiss = (k.HMiss != null) ? k.HReturn.PriTotal : 0,
                             //SubHMiss = (k.HMiss != null && k.ItemUnitID != k.SubUnitId && k.ItemUnitID != null) ? k.HMiss.SubTotal : 0,

                             cost = k.cost,
                             costu = k.costu,
                             k.PurchasePrice
                         })
                        .Distinct().AsEnumerable().Select(o => new
                        {
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemUnitID,
                            o.SubUnitId,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            OpeningStock = o.OpeningStock,
                            o.ConFactor,

                            pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)),
                            subtotal = ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),
                            total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),

                            cost = (o.costu == o.ItemUnitID) ? o.cost : (o.cost * o.ConFactor),
                            o.PurchasePrice
                        }).Distinct().AsEnumerable().Select(o => new
                        {
                            o.ItemName,
                            stockvalue = (o.total / o.ConFactor) * (o.cost),
                            StockValueWithPurchase = (o.total / o.ConFactor) * (o.PurchasePrice),
                            //realstock=com.getmaterialcost(o.ItemID,(DateTime)ondates,(Decimal)(o.total / o.ConFactor))
                        });

                recordsTotal = v.Count();
                var data = v.ToList();
                stkval = (decimal)data.Sum(a => a.stockvalue);
                //stkval=com.getbatchmaterialcost(*)
            }
            return stkval;

        }
        public ActionResult ExpenseReport()
        {
            var cn = System.Configuration.ConfigurationManager.ConnectionStrings.Count;


            var a = System.Configuration.ConfigurationManager.ConnectionStrings;

            int i = 0;
            List<SelectListItem> sel = new List<SelectListItem>();
            for (i = 0; i < cn; i++)
            {
                SelectListItem k = new SelectListItem { Text = a[i].Name, Value = i.ToString() };
                sel.Add(k);
            }
            ViewBag.MC = QkSelect.List(
           new List<SelectListItem>
           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
           }, "Value", "Text", 0);
            ViewBag.connections = QkSelect.List(sel, "Value", "Text");
            _FinancialYear();
            return View();
        }
        #region Profit and Loss
        [QkAuthorize(Roles = "Dev,ProfitAndLoss")]
        public ActionResult ProfitAndLoss()
        {
            var cn = System.Configuration.ConfigurationManager.ConnectionStrings.Count;


            var a = System.Configuration.ConfigurationManager.ConnectionStrings;

            int i = 0;
            List<SelectListItem> sel = new List<SelectListItem>();
            for (i = 0; i < cn; i++)
            {
                SelectListItem k = new SelectListItem { Text = a[i].Name, Value = i.ToString() };
                sel.Add(k);
            }
            ViewBag.MC = QkSelect.List(
           new List<SelectListItem>
           {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
           }, "Value", "Text", 0);
            ViewBag.connections = QkSelect.List(sel, "Value", "Text");
            _FinancialYear();
            return View();
        }
        [QkAuthorize(Roles = "Dev,ProfitAndLoss")]
        public ActionResult cashflow()
        {
         
          
            _FinancialYear();
            return View();
        }
        [QkAuthorize(Roles = "Dev,ProfitAndLoss")]
        public ActionResult cashflowdetailed()
        {


            _FinancialYear();
            return View();
        }
        [QkAuthorize(Roles = "Dev,ProfitAndLoss")]
        public ActionResult Getcashflowdetailed(long? selcompany, long? ddlMC, string fromdate, string todate)
        {
            db.SetCommandTimeOut(60 * 60);
            BalanceSheetViewModel vmodel = new BalanceSheetViewModel();
            DateTime? fdate = null;
            DateTime? tdate = null;
            var fun = 2;
            int Ret = 0;
            companySet();
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            vmodel.from = fdate;
            vmodel.to = tdate;


            var cashinhand = Common.GetChildAccGroup((long)9, "", "", fdate, tdate, 2, 1);
            var subcashinhand = cashinhand.Where(o => o.Parent == 9).ToList();

            var bank = Common.GetChildAccGroup((long)8, "", "", fdate, tdate, 2, 1);
            var subbank = bank.Where(o => o.Parent == 8).ToList();

            var allgroup = cashinhand.ToList();
            var newgrup = allgroup.Union(subcashinhand);
            newgrup = newgrup.Union(bank);
            newgrup = newgrup.Union(subbank);

            List<BalanceSheet> summry = new List<BalanceSheet>();


            var acgroups = newgrup.Select(o => o.AccountsGroupID).Distinct().ToList().ToArray();

            var userpermission = User.IsInRole("All Journal Entry");
            var uid = User.Identity.GetUserId();
            var reference = (from a in db.AccountsTransactions
                             join b in db.Accountss on a.Account equals b.AccountsID

                             where (acgroups.Contains(b.Group)) &&

                             (a.Status == null) &&
       b.Status == Status.active &&

                                              (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                                              (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&

                                              (a.Debit != 0 || a.Credit != 0)

                             select new
                             {
                                 a.reference
                             }).Distinct().Select(o => o.reference);   // perf (audit batch 10): keep as IQueryable -> SQL "IN (subquery)" instead of inlining ~30k refs as literal constants (caused SQL 8623 plan blow-up at FY scale). Output identical.
            var TrialBalanceDisplay = (from a in db.AccountsTransactions
                                       join b in db.Accountss on a.Account equals b.AccountsID into deve
                                       from b in deve.DefaultIfEmpty()
                                       join c in db.AccountsGroups on b.Group equals c.AccountsGroupID into accgroups
                                       from c in accgroups.DefaultIfEmpty()

                                       where (reference.Contains(a.reference)) &&
                                       (a.Status == null) &&

                                           b.Group != 8 && b.Group != 9 &&

                                       b.Status == Status.active &&

                                       (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                                       (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&

                                       (a.Debit != 0 || a.Credit != 0)
                                       group new {c.Name,accountname=b.Name , c.AccountsGroupID, a.Account, a.Debit, a.Credit } by new { c.AccountsGroupID,b.AccountsID } into g

                                       select new TrialBalanceDisplay
                                       {
                                           AccountsGroupID = g.Key.AccountsGroupID,
                                           Particular = g.FirstOrDefault().accountname,
                                           groupname = g.FirstOrDefault().Name,
                                           Parent = g.FirstOrDefault().Account,
                                           Debit = g.Sum(k => k.Debit),
                                           Credit = g.Sum(k => k.Credit),
                                           AccType = "asset",
                                       }).ToList().Select(o => new
                                       {

                                           o.AccountsGroupID,
                                           o.Particular,
                                           o.groupname,
                                           o.Parent,
                                           o.Debit,
                                           o.Credit,
                                           o.AccType,
                                           balance = o.Debit - o.Credit

                                       }).OrderBy(o=>o.groupname).ToList();
            var cc = new TrialBalanceDisplay
            {
                AccountsGroupID = 0,
                Particular = "<span style='color:red'>CASH INFLOWS</span>",


            };
            List<TrialBalanceDisplay> all = new List<TrialBalanceDisplay>();
            all.Add(cc);
            vmodel.TrialBalanceDisplay = all;
            var inflow = TrialBalanceDisplay.Where(o => o.balance <= 0).Select(o =>
            new TrialBalanceDisplay
            {
                AccountsGroupID = o.AccountsGroupID,
                Particular = o.Particular,
                groupname=o.groupname,
                Parent = o.Parent,
                Debit = o.Debit,
                Credit = o.Credit,
                AccType = o.AccType
            }).OrderBy(o=>o.groupname).ToList();
            all.AddRange(inflow);
            cc = new TrialBalanceDisplay
            {
                AccountsGroupID = 0,
                Particular = "<span style='color:red'>CASH OUTFLOWS</span>",


            };

            all.Add(cc);
            var outflow = TrialBalanceDisplay.Where(o => o.balance > 0).Select(o =>
        new TrialBalanceDisplay
        {
            AccountsGroupID = o.AccountsGroupID,
            Particular = o.Particular,
            groupname = o.groupname,
            Parent = o.Parent,
            Debit = o.Debit,
            Credit = o.Credit,
            AccType = o.AccType
        }).OrderBy(o => o.groupname).ToList();
            all.AddRange(outflow);
            vmodel.TrialBalanceDisplay = all.ToList();

            return View(vmodel);

        }

        [QkAuthorize(Roles = "Dev,ProfitAndLoss")]
        public ActionResult Getcashflow(long? selcompany, long? ddlMC, string fromdate, string todate)
        {
            db.SetCommandTimeOut(60 * 60);
            BalanceSheetViewModel vmodel = new BalanceSheetViewModel();
            DateTime? fdate = null;
            DateTime? tdate = null;
            var fun = 2;
            int Ret = 0;
            companySet();
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            vmodel.from = fdate;
            vmodel.to = tdate;

            
                  var cashinhand = Common.GetChildAccGroup((long)9, "", "", fdate, tdate, 2, 1);
                var subcashinhand = cashinhand.Where(o => o.Parent == 9).ToList();

            var bank = Common.GetChildAccGroup((long)8, "", "", fdate, tdate, 2, 1);
            var subbank = bank.Where(o => o.Parent == 8).ToList();
    
            var allgroup = cashinhand.ToList();
            var newgrup= allgroup.Union(subcashinhand);
            newgrup = newgrup.Union(bank);
            newgrup = newgrup.Union(subbank);

            List<BalanceSheet> summry = new List<BalanceSheet>();
               
               
            var acgroups = newgrup.Select(o => o.AccountsGroupID).Distinct().ToList().ToArray();
            
                var userpermission = User.IsInRole("All Journal Entry");
                var uid = User.Identity.GetUserId();
            var reference = (from a in db.AccountsTransactions
                             join b in db.Accountss on a.Account equals b.AccountsID 
                            
                             where (acgroups.Contains(b.Group)) &&
                         
                             (a.Status == null) &&
       b.Status == Status.active &&

                                              (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                                              (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&

                                              (a.Debit != 0 || a.Credit != 0)

                             select new
                             {
                                 a.reference
                             }).Distinct().Select(o => o.reference);   // perf (audit batch 10): IQueryable subquery (IN-subquery) not a materialized 30k-const IN-list. Output identical.
                var TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID into deve
                                              from b in deve.DefaultIfEmpty()
                                              join c in db.AccountsGroups on b.Group equals c.AccountsGroupID into accgroups
                                              from c in accgroups.DefaultIfEmpty()

                                              where (reference.Contains(a.reference)) &&
                                              (a.Status == null) &&
                                          
                                                  b.Group != 8 && b.Group != 9 &&

                                              b.Status == Status.active &&

                                              (fromdate == null || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                                              (todate == null || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&

                                              (a.Debit != 0 || a.Credit != 0)
                                              group new { c.Name, c.AccountsGroupID, a.Account, a.Debit, a.Credit } by new { c.AccountsGroupID } into g

                                              select new TrialBalanceDisplay
                                              {
                                                  AccountsGroupID = g.Key.AccountsGroupID,
                                                  Particular = g.FirstOrDefault().Name,
                                                  Parent = g.FirstOrDefault().Account,
                                                  Debit = g.Sum(k => k.Debit),
                                                  Credit = g.Sum(k => k.Credit),
                                                  AccType = "asset",
                                              }).ToList().Select(o => new
                                              {

                                                  o.AccountsGroupID,
                                                  o.Particular,
                                                  o.Parent,
                                                  o.Debit,
                                                  o.Credit,
                                                  o.AccType,
                                                  balance = o.Debit - o.Credit

                                              }).ToList();
            var cc = new TrialBalanceDisplay
            {
                AccountsGroupID = 0,
                Particular = "<span style='color:red'>CASH INFLOWS</span>",


            };
            List<TrialBalanceDisplay> all = new List<TrialBalanceDisplay>();
            all.Add(cc);
            vmodel.TrialBalanceDisplay = all;
            var inflow = TrialBalanceDisplay.Where(o => o.balance <= 0).Select(o =>
            new TrialBalanceDisplay
            {
                AccountsGroupID = o.AccountsGroupID,
                Particular = o.Particular,
                Parent = o.Parent,
                Debit = o.Debit,
                Credit = o.Credit,
                AccType = o.AccType
            }).ToList();
            all.AddRange(inflow);
            cc = new TrialBalanceDisplay
            {
                AccountsGroupID = 0,
                Particular = "<span style='color:red'>CASH OUTFLOWS</span>",


            };

            all.Add(cc);
            var outflow = TrialBalanceDisplay.Where(o => o.balance > 0).Select(o =>
        new TrialBalanceDisplay
        {
            AccountsGroupID = o.AccountsGroupID,
            Particular = o.Particular,
            Parent = o.Parent,
            Debit = o.Debit,
            Credit = o.Credit,
            AccType = o.AccType
        }).ToList();
            all.AddRange(outflow);
            vmodel.TrialBalanceDisplay = all.ToList();  

            return View(vmodel);

        }

            [QkAuthorize(Roles = "Dev,ProfitAndLoss")]
        public ActionResult GetProfitAndLoss(long? selcompany, long? ddlMC, string fromdate, string todate)
        {
            long? mc = ddlMC;
            BalanceSheetViewModel vmodel = new BalanceSheetViewModel();
            DateTime? fdate = null;
            DateTime? tdate = null;
            var fun = 2;
            int Ret = 0;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            vmodel.from = fdate;
            vmodel.to = tdate;
            if (selcompany != null)
            {
                var cn = System.Configuration.ConfigurationManager.ConnectionStrings.Count;


                var aa = System.Configuration.ConfigurationManager.ConnectionStrings;

                db = new ApplicationDbContext(aa[(int)selcompany].Name);
            }
            #region PLCALCU
            var openstock = getOpeningStock(fromdate, "open");
            var closestock = getOpeningStock(todate, "close");


            var salesaccount = db.Accountss.Where(o => o.Group == 15).Select(o => o.AccountsID).ToList().ToArray();
            var sprices = (from i in db.AccountsTransactions
                           where (fromdate == "" || EF.Functions.DateDiffDay(i.Date, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0) &&salesaccount.Contains(i.Account)
                           && i.Status == null
                           //group i by i.SalesEntryId into g
                           select new
                           {
                               Total = i.Credit
                           }).ToList();


            decimal sprice = sprices != null ? sprices.Sum(a => a.Total) : 0;


            //sales return price.
            var sretprices = (from i in db.AccountsTransactions
                              where (fromdate == "" || EF.Functions.DateDiffDay(i.Date, fdate) <= 0) &&
                              (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0)
                              && i.Account == 1
                              && i.Status == null
                              select new
                              {
                                  Total = i.Debit
                              }).ToList();
            decimal sretprice = (sretprices != null) ? sretprices.Sum(a => a.Total) : 0;



            //purchase price
            var pprices = (from i in db.AccountsTransactions
                           where (fromdate == "" || EF.Functions.DateDiffDay(i.Date, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0)
                            && i.Account == 2 && i.Purpose == "Purchase"
                            && i.Status == null
                           select new
                           {
                               Total = i.Debit
                           }).ToList();
            decimal pprice = (pprices != null) ? pprices.Sum(a => a.Total) : 0;


            //purchase return price
            var pretprices = (from i in db.AccountsTransactions
                              where (fromdate == "" || EF.Functions.DateDiffDay(i.Date, fdate) <= 0) &&
                              (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0)
                              && i.Account == 2 && i.Purpose == "Purchase Return"
                              && i.Status == null
                              select new
                              {
                                  Total = i.Credit
                              }).ToList();
            decimal pretprice = (pretprices != null) ? pretprices.Sum(a => a.Total) : 0;


            //sales price.
            //     sprices = (from i in db.AccountsTransactions
            //                   (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0) && i.Account == 1
            //                   j.MaterialCenter==mc
            //                   //group i by i.SalesEntryId into g
            //                       Total = i.Credit




            //    //sales return price.
            //     sretprices = (from i in db.AccountsTransactions
            //                      (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0)
            //                      && i.Account == 1 && i.Purpose == "Sale Return"
            //                   j.MaterialCenter == mc
            //                          Total = i.Debit



            //    //purchase price
            //     pprices = (from i in db.AccountsTransactions
            //                   (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0)
            //                    && i.Account == 2 && i.Purpose == "Purchase"
            //                   j.MaterialCenter == mc
            //                       Total = i.Debit


            //    //purchase return price
            //     pretprices = (from i in db.AccountsTransactions
            //                      (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0)
            //                      && i.Account == 2 && i.Purpose == "Purchase Return"
            //                   j.MaterialCenter == mc
            //                          Total = i.Credit


            ////direct expenses

            ////in direct expenses



            //direct income
            var dirincome = Common.GetChildAccGroup(31, "Direct Income", "asset", fdate, tdate, fun, 1);


            //in direct income
            var indirincome = Common.GetChildAccGroup(32, "InDirect Income", "asset", fdate, tdate, fun, 1);


            //direct expenses
            var dexpenses = Common.GetChildAccGroup(29, "Expenses (Direct/Mfg.)", "liability", fdate, tdate, fun, 1).ToList();


            //in direct expenses
            var indexpenses = com.GetChildAccGroupNewindirectexpense(13, "Expenses", "liability", fdate, tdate, fun, 1).ToList();


            var Sales = sprice - sretprice;
            var Purchase = pprice - pretprice;

            var DirectExp = dexpenses != null ? (decimal)dexpenses.Where(a => a.orderB == 0).Sum(a => a.Debit - a.Credit) : 0;
            var InDirectExp = indexpenses != null ? (decimal)indexpenses.Where(a => a.orderB == 0).Sum(a => a.Debit - a.Credit) : 0;
            var DirectIncome = dirincome != null ? (decimal)dirincome.Where(a => a.orderB == 0).Sum(a => a.Credit - a.Debit) : 0;
            var InDirectIncome = indirincome != null ? (decimal)indirincome.Where(a => a.orderB == 0).Sum(a => a.Credit - a.Debit) : 0;

            DirectExp = DirectExp < 0 ? (DirectExp * -1) : DirectExp;
            InDirectExp = InDirectExp < 0 ? (InDirectExp * -1) : InDirectExp;
            DirectIncome = DirectIncome < 0 ? (DirectIncome * -1) : DirectIncome;
            InDirectIncome = InDirectIncome < 0 ? (InDirectIncome * -1) : InDirectIncome;


            var Debit = openstock + Purchase + DirectExp;
            var Credit = closestock + Sales + DirectIncome;

            decimal TotalDr = 0;
            decimal TotalCr = 0;
            decimal GTotalDr = 0;
            decimal GTotalCr = 0;
            decimal Profit = 0;

            ////for gross profit b/d
            List<ProfitAndLoss> baddepthdr = new List<ProfitAndLoss>();
            List<ProfitAndLoss> baddepthcr = new List<ProfitAndLoss>();

            List<ProfitAndLoss> net = new List<ProfitAndLoss>();


            //--------------------------------------DEBIT SIDE---------------------------------------------

            //1

            int increDr = 0;
            increDr++;
            ProfitAndLoss opens = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "Opening Stock",
                ParentName = "Opening Stock",
                Amount = openstock,
                Orders = 0,
                Temp = increDr
            };
            List<ProfitAndLoss> opstk = new List<ProfitAndLoss>();
            opstk.Add(opens);

            //2
            increDr++;
            ProfitAndLoss purchase = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "Purchase",
                ParentName = "Purchase",
                Amount = Purchase,
                Orders = 0,
                Temp = increDr
            };
            List<ProfitAndLoss> purchases = new List<ProfitAndLoss>();
            purchases.Add(purchase);

            //3
            increDr++;
            ProfitAndLoss purchaseentrys = new ProfitAndLoss()
            {
                Parent = 1,
                AccountId = 2,
                Particulars = "Purchase",
                ParentName = "Purchase",
                Amount = pprice,
                Orders = 0,
                Temp = increDr
            };
            List<ProfitAndLoss> purchaseentry = new List<ProfitAndLoss>();
            purchaseentry.Add(purchaseentrys);

            //4
            increDr++;
            ProfitAndLoss purchasereturns = new ProfitAndLoss()
            {
                Parent = 1,
                AccountId = 2,
                Particulars = "Purchase Return",
                ParentName = "Purchase Return",
                Amount = pretprice,
                Orders = 0,
                Temp = increDr
            };
            List<ProfitAndLoss> purchasereturn = new List<ProfitAndLoss>();
            purchasereturn.Add(purchasereturns);



            int increCr = 0;
            increCr++;
            ProfitAndLoss closes = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "Closing Stock",
                Amount = closestock,
                Orders = 0,
                Temp = increCr
            };
            List<ProfitAndLoss> clsstk = new List<ProfitAndLoss>();
            clsstk.Add(closes);

            //2
            increCr++;
            ProfitAndLoss sale = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "Sale",
                Amount = Sales,
                Orders = 0,
                Temp = increCr
            };
            List<ProfitAndLoss> sales = new List<ProfitAndLoss>();
            sales.Add(sale);

            //3
            var salesaccounts = db.Accountss.Where(o => o.Group == 15).Select(o => o.AccountsID).ToList().ToArray();
            var spricesss = (from i in db.AccountsTransactions
                           join a in db.Accountss on i.Account equals a.AccountsID 
                             where (fromdate == "" || EF.Functions.DateDiffDay(i.Date, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0) && salesaccount.Contains(i.Account)
                           && i.Status == null
                           group new { a.Name, i.Credit } by i.Account  into g
                           select new
                           {
                               Total = g.Sum(o=>o.Credit),
                               perti=g.Select(o=>o.Name).FirstOrDefault()

                           }).ToList();
            List<ProfitAndLoss> saleentry = new List<ProfitAndLoss>();
            foreach (var sa in spricesss)
            {
                increCr++;
                ProfitAndLoss saleentrys = new ProfitAndLoss()
                {
                    Parent = 1,
                    AccountId = 1,
                    Particulars = sa.perti,
                    Amount = sa.Total,
                    Orders = 0,
                    Temp = increCr
                };
               
                saleentry.Add(saleentrys);
            }
            //4
            increCr++;
            ProfitAndLoss salereturns = new ProfitAndLoss()
            {
                Parent = 1,
                AccountId = 1,
                Particulars = "Sales Return",
                Amount = sretprice,
                Orders = 0,
                Temp = increCr
            };
            List<ProfitAndLoss> salereturn = new List<ProfitAndLoss>();
            salereturn.Add(salereturns);




            List<ProfitAndLoss> nullrows = new List<ProfitAndLoss>();
            ProfitAndLoss gfnulls = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "",
                ParentName = "",
                Amount = null,
                Orders = 1,
                Temp = 0
            };
            nullrows.Add(gfnulls);


            //debit-union
            var debit = opstk.Union(purchases);
            debit = debit.Union(purchaseentry);

            var pret = purchasereturn.Select(a => a.Amount).FirstOrDefault();
            var sret = salereturn.Select(a => a.Amount).FirstOrDefault();

            if (pret != 0)
            {
                debit = debit.Union(purchasereturn);
            }
            else
            {
                if (pret != 0 || sret != 0)
                {
                    nullrows[0].Temp = debit.Count() + 1;
                    debit = debit.Union(nullrows);
                }
            }


            //credit -union
            var credit = clsstk.Union(sales);
            credit = credit.Union(saleentry);

            if (sret != 0)
            {
                credit = credit.Union(salereturn);
            }
            else
            {
                if (pret != 0 || sret != 0)
                {
                    nullrows[0].Temp = credit.Count() + 1;
                    credit = credit.Union(nullrows);
                }
            }


            //5
            increDr = debit.Count() + 1;
            //                 Parent = a.Parent,
            //                 AccountId = null,
            //                 Particulars = a.Particulars,
            //                 ParentName = a.ParentName,
            //                 Amount = a.Debit - a.Credit,
            //                 Orders = 1,
            //                 Temp = increDr

            List<ProfitAndLoss> diexpacc = new List<ProfitAndLoss>();
            foreach (var entry in dexpenses)
            {
                ProfitAndLoss entrys = new ProfitAndLoss()
                {
                    Parent = 0,
                    AccountId = null,
                    Particulars = entry.Particulars,
                    ParentName = entry.ParentName,
                    Amount = (entry.Debit - entry.Credit) < 0 ? (entry.Debit - entry.Credit) * -1 : (entry.Debit - entry.Credit),
                    Orders = 0,
                    Temp = increDr
                };
                diexpacc.Add(entrys);

                var accounts = db.Accountss.Where(a => a.Group == entry.AccountsGroupID).ToList();
                foreach (var acc in accounts)
                {
                    var chkacc = db.AccountsTransactions.Where(a => a.Account == acc.AccountsID).ToList();
                    if (chkacc.Count() > 0)
                    {

                        decimal debits = (from a in db.AccountsTransactions
                                          where a.Account == acc.AccountsID && a.Status == null
                                          && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
                                          && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
                                          select new
                                          {
                                              a.Debit,
                                          }).ToList().Sum(x => x.Debit);

                        decimal credits = (from a in db.AccountsTransactions
                                           where a.Account == acc.AccountsID && a.Status == null
                                           && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
                                           && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
                                           select new
                                           {
                                               a.Credit,
                                           }).ToList().Sum(x => x.Credit);


                        diexpacc.Add(new ProfitAndLoss
                        {
                            Parent = 1,
                            AccountId = acc.AccountsID,
                            Particulars = acc.Name,
                            ParentName = acc.Name,
                            Amount = (debits - credits) < 0 ? (debits - credits) * -1 : (debits - credits),
                            Orders = 1,
                            Temp = increDr,
                        });
                        increDr++;
                    }
                }

            }


            //6
            //                    //join b in db.AccountsTransactions on a.AccountsID equals b.Account into accs
            //                    //from b in accs.DefaultIfEmpty()
            //                where a.Group == 29
            //                let acdebit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Debit)
            //                let accredit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Credit)

            //                //(todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)
            //                    Parent = 1,
            //                    AccountId = a.AccountsID,
            //                    Particulars = a.Name,
            //                    ParentName = a.Name,
            //                    //Amount = b != null ? db.AccountsTransactions.Where(c => c.Account == a.AccountsID && (fromdate == "" || EF.Functions.DateDiffDay(b.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)).Select(c => c.Debit - c.Credit).Sum() : 0,
            //                    Orders = 1,
            //                    Temp = increDr


            //                where b.Group == 29 //&& (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)

            //                    b.AccountsID,
            //                    b.Name,

            //                    Orders = 1,
            //                    Temp = increDr
            //                }).Distinct().Select(o => new ProfitAndLoss
            //                    Parent = 1,
            //                    AccountId = o.AccountsID,
            //                    Particulars = o.Name,
            //                    ParentName = o.Name,

            //                    Orders = 1,
            //                    Temp = increDr


            //--for ordering account based on index--direct expense
            var diexpaccs = diexpacc.Where(a => a.Amount != 0 && a.Amount != null).AsEnumerable()
            .Select((o, index) => new ProfitAndLoss
            {
                Parent = o.Parent,
                AccountId = o.AccountId,
                Particulars = o.Particulars,
                ParentName = o.ParentName,
                Amount = o.Amount,
                Orders = o.Orders,
                Temp = (increDr + index)
            });
            List<ProfitAndLoss> diexpaccnew = diexpaccs.ToList(); //convert to list
            int CountExp2 = diexpaccnew.Count();


            ////5
            increCr = credit.Count() + 1;
            //                    Parent = 0,//a.Parent 
            //                    AccountId = null,
            //                    Particulars = a.Particulars,
            //                    ParentName = a.ParentName,
            //                    Amount = a.Credit - a.Debit,
            //                    Orders = 0,
            //                    Temp = increCr


            List<ProfitAndLoss> dirincomacc = new List<ProfitAndLoss>();
            foreach (var entry in dirincome)
            {
                ProfitAndLoss entrys = new ProfitAndLoss()
                {
                    Parent = 0,
                    AccountId = null,
                    Particulars = entry.Particulars,
                    ParentName = entry.ParentName,
                    Amount = (entry.Credit - entry.Debit) < 0 ? (entry.Credit - entry.Debit) * -1 : (entry.Credit - entry.Debit),
                    Orders = 0,
                    Temp = increCr
                };
                dirincomacc.Add(entrys);

                var accounts = db.Accountss.Where(a => a.Group == entry.AccountsGroupID).ToList();
                foreach (var acc in accounts)
                {
                    var chkacc = db.AccountsTransactions.Where(a => a.Account == acc.AccountsID).ToList();
                    if (chkacc.Count() > 0)
                    {

                        decimal debits = (from a in db.AccountsTransactions
                                          where a.Account == acc.AccountsID && a.Status == null
                                          && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
                                          && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
                                          select new
                                          {
                                              a.Debit,
                                          }).ToList().Sum(x => x.Debit);

                        decimal credits = (from a in db.AccountsTransactions
                                           where a.Account == acc.AccountsID && a.Status == null
                                           && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
                                           && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
                                           select new
                                           {
                                               a.Credit,
                                           }).ToList().Sum(x => x.Credit);

                        dirincomacc.Add(new ProfitAndLoss
                        {
                            Parent = 1,
                            AccountId = acc.AccountsID,
                            Particulars = acc.Name,
                            ParentName = acc.Name,
                            Amount = (credits - debits) < 0 ? (credits - debits) * -1 : (credits - debits),
                            Orders = 1,
                            Temp = increCr,
                        });
                        increCr++;
                    }
                }

            }


            //6
            //                       //join b in db.AccountsTransactions on a.AccountsID equals b.Account into accs
            //                       //from b in accs.DefaultIfEmpty()
            //                   where a.Group == 31
            //                   let acdebit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Debit)
            //                   let accredit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Credit)

            //                   //(todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)
            //                       Parent = 1,
            //                       AccountId = a.AccountsID,
            //                       Particulars = a.Name,
            //                       ParentName = a.Name,
            //                       Amount = accredit - acdebit,
            //                       //Amount = b != null ? db.AccountsTransactions.Where(c => c.Account == a.AccountsID && (fromdate == "" || EF.Functions.DateDiffDay(b.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)).Select(c => c.Debit - c.Credit).Sum() : 0,
            //                       Orders = 1,
            //                       Temp = increCr





            //--for ordering account based on index--direct income
            var dirincomaccs = dirincomacc.Where(a => a.Amount != 0 && a.Amount != null).AsEnumerable()
            .Select((o, index) => new ProfitAndLoss
            {
                Parent = o.Parent,
                AccountId = o.AccountId,
                Particulars = o.Particulars,
                ParentName = o.ParentName,
                Amount = o.Amount,
                Orders = o.Orders,
                Temp = (increCr + index)
            });

            List<ProfitAndLoss> dirincomaccnew = dirincomaccs.ToList(); //convert to list
            int CountDIncome2 = dirincomaccnew.Count();





            var expcount = CountExp2;
            var incomecount = CountDIncome2;

            //fiilling null rows direct exp & income
            List<ProfitAndLoss> NullListCr1 = new List<ProfitAndLoss>();
            List<ProfitAndLoss> NullListDr1 = new List<ProfitAndLoss>();
            if (expcount > incomecount)
            {
                increCr = credit.Count() + 1;
                var totcount = expcount - incomecount;
                for (int i = 0; i < totcount; i++)
                {
                    increCr++;
                    var tcount = increCr;
                    dirincomaccnew.Add(new ProfitAndLoss
                    {
                        Parent = 0,
                        AccountId = null,
                        Particulars = "",
                        ParentName = "",
                        Amount = null,
                        Orders = 1,
                        Temp = tcount
                    });
                }
            }
            if (expcount < incomecount)
            {
                increDr = debit.Count() + 1;
                var totcount = incomecount - expcount;
                for (int i = 0; i < totcount; i++)
                {
                    increDr++;
                    var tcount = increDr;
                    diexpaccnew.Add(new ProfitAndLoss
                    {
                        Parent = 0,
                        AccountId = null,
                        Particulars = "",
                        ParentName = "",
                        Amount = null,
                        Orders = 1,
                        Temp = tcount
                    });
                }
            }
            debit = debit.Union(diexpaccnew);
            credit = credit.Union(dirincomaccnew);



            //for gross profit
            List<ProfitAndLoss> pandf = new List<ProfitAndLoss>();
            List<ProfitAndLoss> nullrow1 = new List<ProfitAndLoss>();
            ProfitAndLoss gfnull1 = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "",
                ParentName = "",
                Amount = null,
                Orders = 0,
                Temp = 0
            };
            nullrow1.Add(gfnull1);


            List<ProfitAndLoss> nullrow2 = new List<ProfitAndLoss>();
            ProfitAndLoss gfnull2 = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "",
                ParentName = "",
                Amount = null,
                Orders = 0,
                Temp = 0
            };
            nullrow2.Add(gfnull2);

            if (Debit < Credit)
            {
                TotalDr = (decimal)Credit - (decimal)Debit;
                increDr = debit.Count() + 2;
                pandf.Clear();
                ProfitAndLoss gf = new ProfitAndLoss()
                {
                    Parent = 0,
                    AccountId = null,
                    Particulars = "Gross Profit",
                    ParentName = "Gross Profit",
                    Amount = TotalDr,
                    Orders = 0,
                    Temp = increDr
                };
                pandf.Add(gf);
            }


            if (Debit > Credit)
            {
                increCr = credit.Count() + 2;
                pandf.Clear();
                TotalCr = (decimal)Debit - (decimal)Credit;
                ProfitAndLoss gl = new ProfitAndLoss()
                {
                    Parent = 0,
                    AccountId = null,
                    Particulars = "Gross Loss",
                    ParentName = "Gross Loss",
                    Amount = TotalCr,
                    Orders = 0,
                    Temp = increCr
                };
                pandf.Add(gl);
            }



            //first total
            increDr = debit.Count() + 1;
            decimal FirstTotalDr = Debit + TotalDr;
            ProfitAndLoss TotalDr1 = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "Total",
                ParentName = "Total",
                Amount = FirstTotalDr,
                Orders = 0,
                Temp = increDr
            };
            List<ProfitAndLoss> TotalDrOne = new List<ProfitAndLoss>();
            TotalDrOne.Add(TotalDr1);

            //first total
            increCr = credit.Count() + 1;
            decimal FirstTotalCr = TotalCr + Credit;
            ProfitAndLoss TotalCr1 = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "Total",
                ParentName = "Total",
                Amount = FirstTotalCr,
                Orders = 0,
                Temp = increCr
            };
            List<ProfitAndLoss> TotalCrOne = new List<ProfitAndLoss>();
            TotalCrOne.Add(TotalCr1);


            if (Debit > Credit)
            {
                increDr = debit.Count() + 1;
                TotalCr = (decimal)Debit - (decimal)Credit;

                baddepthdr.Clear();
                ProfitAndLoss badde = new ProfitAndLoss()
                {
                    Parent = 0,
                    AccountId = null,
                    Particulars = "Gross Loss b/d",
                    ParentName = "Gross Loss b/d",
                    Amount = TotalCr,
                    Orders = 0,
                    Temp = increDr
                };
                baddepthdr.Add(badde);

            }

            if (Debit < Credit)
            {
                increCr = credit.Count() + 1;
                baddepthcr.Clear();
                ProfitAndLoss baddep = new ProfitAndLoss()
                {
                    Parent = 0,
                    AccountId = null,
                    Particulars = "Gross Profit b/d",
                    ParentName = "Gross Profit b/d",
                    Amount = TotalDr,
                    Orders = 0,
                    Temp = increCr
                };
                baddepthcr.Add(baddep);
            }




            //gross profit/loss union
            if (Debit < Credit)
            {
                debit = debit.Union(pandf);
                nullrow1[0].Temp = debit.Count() + 1;
                credit = credit.Union(nullrow1);
            }
            if (Debit > Credit)
            {
                credit = credit.Union(pandf);
                nullrow1[0].Temp = credit.Count() + 1;
                debit = debit.Union(nullrow1);
            }

            //first total union
            TotalDrOne[0].Temp = debit.Count() + 2;
            TotalCrOne[0].Temp = credit.Count() + 2;
            debit = debit.Union(TotalDrOne);
            credit = credit.Union(TotalCrOne);


            //gross profit/loss b/d union
            if (Debit > Credit)
            {
                baddepthdr[0].Temp = debit.Count() + 2;
                debit = debit.Union(baddepthdr);
                nullrow2[0].Temp = debit.Count() + 1;
                credit = credit.Union(nullrow2);
            }
            if (Debit < Credit)
            {
                baddepthcr[0].Temp = credit.Count() + 2;
                credit = credit.Union(baddepthcr);
                nullrow2[0].Temp = credit.Count() + 1;
                debit = debit.Union(nullrow2);
            }

            //indirect expense
            //                   Parent = 0,//a.Parent,
            //                   AccountId = null,
            //                   Particulars = a.Particulars,
            //                   ParentName = a.ParentName,
            //                   Amount = a.Debit - a.Credit,
            //                   Orders = a.orderB,
            //                   Temp = increDr

            ////indirect expense accounts
            //                    where a.Group == 30
            //                    let acdebit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Debit)
            //                    let accredit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Credit)

            //                    //(todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)
            //                        Parent = 1,
            //                        AccountId = a.AccountsID,
            //                        Particulars = a.Name,
            //                        ParentName = a.Name,
            //                        //Amount = b != null ? 
            //                        //Amount = b != null ? db.AccountsTransactions.Where(c => c.Account == a.AccountsID).Select(c => c.Debit - c.Credit).Sum() : 0,
            //                        Amount = acdebit - accredit,
            //                        Orders = 1,
            //                        Temp = increDr


            List<ProfitAndLoss> indiexpaccss = new List<ProfitAndLoss>();
            foreach (var entry in indexpenses)
            {
                ProfitAndLoss entrys = new ProfitAndLoss()
                {
                    Parent = 0,
                    AccountId = entry.AccountsGroupID,
                    Particulars = entry.Particulars,
                    ParentName = entry.ParentName,
                    Amount = (entry.Debit - entry.Credit) < 0 ? (entry.Debit - entry.Credit) * -1 : (entry.Debit - entry.Credit),
                    Orders = 0,
                    Temp = increDr
                };
                indiexpaccss.Add(entrys);

                var accounts = db.Accountss.Where(a => a.Group == entry.AccountsGroupID).ToList();
                foreach (var acc in accounts)
                {
                    var chkacc = db.AccountsTransactions.Where(a => a.Account == acc.AccountsID).ToList();
                    if (chkacc.Count() > 0)
                    {

                        decimal debits = (from a in db.AccountsTransactions
                                          where a.Account == acc.AccountsID && a.Status == null
                                          && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
                                          && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
                                          select new
                                          {
                                              a.Debit,
                                          }).ToList().Sum(x => x.Debit);

                        decimal credits = (from a in db.AccountsTransactions
                                           where a.Account == acc.AccountsID && a.Status == null
                                           && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
                                           && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
                                           select new
                                           {
                                               a.Credit,
                                           }).ToList().Sum(x => x.Credit);


                        indiexpaccss.Add(new ProfitAndLoss
                        {
                            //Parent = 1,
                            //AccountId = acc.AccountsID,
                            //Particulars = acc.Name,
                            //ParentName = acc.Name,
                            //Orders = 1,
                            //Temp = increDr,
                        });
                        increDr++;
                    }
                }

            }


            //--for ordering account based on index-indirect exp
            var indiexpacc = indiexpaccss.Where(a => a.Amount != 0 && a.Amount != null).AsEnumerable()
            .Select((o, index) => new ProfitAndLoss
            {
                Parent = o.Parent,
                AccountId = o.AccountId,
                Particulars = o.Particulars,
                ParentName = o.ParentName,
                Amount = o.Amount,
                Orders = o.Orders,
                Temp = (increDr + index)
            });
            List<ProfitAndLoss> indiexpaccnew = indiexpacc.ToList();
            Decimal xxx = indiexpaccnew.Where(o => o.Parent == 0).Sum(o => o.Amount) ?? 0;

            int CountIExp2 = indiexpaccnew.Count();


            //indirect income
            //                      Parent = 0, //a.Parent,
            //                      AccountId = null,
            //                      Particulars = a.Particulars,
            //                      ParentName = a.ParentName,
            //                      Amount = a.Credit - a.Debit,
            //                      Orders = 1,
            //                      Temp = increCr

            ////indirect income accounts
            //                           //join b in db.AccountsTransactions on a.AccountsID equals b.Account into accs
            //                           //from b in accs.DefaultIfEmpty()
            //                       where a.Group == 32
            //                       let acdebit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Debit)
            //                       let accredit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Credit)

            //                       //(todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)
            //                           Parent = 1,
            //                           AccountId = a.AccountsID,
            //                           Particulars = a.Name,
            //                           ParentName = a.Name,
            //                           Amount = accredit - acdebit,
            //                           //Amount = b != null ? db.AccountsTransactions.Where(c => c.Account == a.AccountsID && (fromdate == "" || EF.Functions.DateDiffDay(b.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)).Select(c => c.Debit - c.Credit).Sum() : 0,
            //                           Orders = 1,
            //                           Temp = increCr




            List<ProfitAndLoss> indirincomaccss = new List<ProfitAndLoss>();
            foreach (var entry in indirincome)
            {
                ProfitAndLoss entrys = new ProfitAndLoss()
                {
                    Parent = 0,
                    AccountId = entry.AccountsGroupID,
                    Particulars = entry.Particulars,
                    ParentName = entry.ParentName,
                    Amount = (entry.Credit - entry.Debit) < 0 ? (entry.Credit - entry.Debit) * -1 : (entry.Credit - entry.Debit),
                    Orders = 0,
                    Temp = increCr
                };
                indirincomaccss.Add(entrys);

                var accounts = db.Accountss.Where(a => a.Group == entry.AccountsGroupID).ToList();
                foreach (var acc in accounts)
                {
                    var chkacc = db.AccountsTransactions.Where(a => a.Account == acc.AccountsID).ToList();
                    if (chkacc.Count() > 0)
                    {

                        decimal debits = (from a in db.AccountsTransactions
                                          where a.Account == acc.AccountsID && a.Status == null
                                          && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
                                          && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
                                          select new
                                          {
                                              a.Debit,
                                          }).ToList().Sum(x => x.Debit);

                        decimal credits = (from a in db.AccountsTransactions
                                           where a.Account == acc.AccountsID && a.Status == null
                                           && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
                                           && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
                                           select new
                                           {
                                               a.Credit,
                                           }).ToList().Sum(x => x.Credit);


                        indirincomaccss.Add(new ProfitAndLoss
                        {
                            //Parent = 1,
                            //AccountId = acc.AccountsID,
                            //Particulars = acc.Name,
                            //ParentName = acc.Name,
                            //Orders = 1,
                            //Temp = increCr,
                        });
                        increCr++;
                    }
                }

            }


            //--for ordering account based on index-indirect income
            var indirincomacc = indirincomaccss.Where(a => a.Amount != 0 && a.Amount != null).AsEnumerable()
            .Select((o, index) => new ProfitAndLoss
            {
                Parent = o.Parent,
                AccountId = o.AccountId,
                Particulars = o.Particulars,
                ParentName = o.ParentName,
                Amount = o.Amount,
                Orders = o.Orders,
                Temp = (increCr + index)
            });
            List<ProfitAndLoss> indirincomaccnew = indirincomacc.ToList();
            Decimal zzz = indirincomaccnew.Where(o => o.Parent == 0).Sum(o => o.Amount) ?? 0;

            int CountIDIncome2 = indirincomaccnew.Count();


            var inexpcount = CountIExp2;
            var inincomecount = CountIDIncome2;

            //fiilling null rows indirect expense/income
            List<ProfitAndLoss> NullListCr = new List<ProfitAndLoss>();
            List<ProfitAndLoss> NullListDr = new List<ProfitAndLoss>();
            if (inexpcount > inincomecount)
            {
                increCr = credit.Count() + inincomecount + 1;
                var totcount = inexpcount - inincomecount;
                for (int i = 0; i < totcount; i++)
                {
                    increCr++;
                    var tcount = increCr;
                    indirincomaccnew.Add(new ProfitAndLoss
                    {
                        Parent = 0,
                        AccountId = null,
                        Particulars = "",
                        ParentName = "",
                        Amount = null,
                        Orders = 1,
                        Temp = tcount
                    });
                }
            }
            if (inexpcount < inincomecount)
            {
                increDr = debit.Count() + inexpcount + 1;
                var totcount = inincomecount - inexpcount;
                for (int i = 0; i < totcount; i++)
                {
                    increDr++;
                    var tcount = increDr;
                    indiexpaccnew.Add(new ProfitAndLoss
                    {
                        Parent = 0,
                        AccountId = null,
                        Particulars = "",
                        ParentName = "",
                        Amount = null,
                        Orders = 1,
                        Temp = tcount
                    });
                }
            }

            debit = debit.Union(indiexpaccnew);
            credit = credit.Union(indirincomaccnew);


            // --------------------------------------------------------

            GTotalDr = (decimal)TotalCr + xxx;// (decimal)InDirectExp;
            GTotalCr = (decimal)TotalDr + zzz;// + (decimal)InDirectIncome;
            if ((decimal)GTotalDr < (decimal)GTotalCr)
            {//profit
                increDr = debit.Count() + 2;
                Profit = (decimal)GTotalCr - (decimal)GTotalDr;
                net.Clear();
                ProfitAndLoss netp = new ProfitAndLoss()
                {
                    Parent = 0,
                    AccountId = null,
                    Particulars = "Net Profit",
                    ParentName = "Net Profit",
                    Amount = Profit,
                    Orders = 0,
                    Temp = increDr
                };
                net.Add(netp);
                TotalCr = TotalCr + Profit;
            }

            //Second total
            increDr = debit.Count() + 1;
            decimal SecondTotalDr = (decimal)TotalCr + xxx;// (decimal)InDirectExp;
            ProfitAndLoss TotalDr2 = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "Total",
                ParentName = "Total",
                Amount = SecondTotalDr,
                Orders = 0,
                Temp = increDr
            };
            List<ProfitAndLoss> TotalDrTwo = new List<ProfitAndLoss>();
            TotalDrTwo.Add(TotalDr2);



            if ((decimal)GTotalDr > (decimal)GTotalCr)
            {//loss
                increCr = credit.Count() + 2;
                Profit = (decimal)GTotalDr - (decimal)GTotalCr;
                net.Clear();
                ProfitAndLoss netl = new ProfitAndLoss()
                {
                    Parent = 0,
                    AccountId = null,
                    Particulars = "Net Loss",
                    ParentName = "Net Loss",
                    Amount = Profit,
                    Orders = 0,
                    Temp = increCr
                };
                net.Add(netl);

                TotalDr = TotalDr + Profit;
            }

            //second total
            increCr = credit.Count() + 1;
            decimal SecondTotalCr = (decimal)TotalDr + zzz;// (decimal)InDirectIncome;
            ProfitAndLoss TotalCr2 = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "Total",
                ParentName = "Total",
                Amount = SecondTotalCr,
                Orders = 0,
                Temp = increCr
            };
            List<ProfitAndLoss> TotalCrTwo = new List<ProfitAndLoss>();
            TotalCrTwo.Add(TotalCr2);


            List<ProfitAndLoss> nullrow3 = new List<ProfitAndLoss>();
            ProfitAndLoss gfnull3 = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "",
                ParentName = "",
                Amount = null,
                Orders = 0,
                Temp = 0
            };
            nullrow3.Add(gfnull3);

            //net profit/loss union
            if ((decimal)GTotalDr < (decimal)GTotalCr)
            {
                debit = debit.Union(net);
                nullrow3[0].Temp = debit.Count() + 1;
                credit = credit.Union(nullrow3);
            }
            if ((decimal)GTotalDr > (decimal)GTotalCr)
            {
                credit = credit.Union(net);
                nullrow3[0].Temp = credit.Count() + 1;
                debit = debit.Union(nullrow3);
            }

            //final total union
            TotalDrTwo[0].Temp = debit.Count() + 2;
            TotalCrTwo[0].Temp = credit.Count() + 2;
            debit = debit.Union(TotalDrTwo);
            credit = credit.Union(TotalCrTwo);


            var count1 = getOrderingPF(debit);
            var count2 = getOrderingPF(credit);


            //outer joins-for single row
            var leftOuterJoin = (from a in debit
                                 join b in credit on a.Temp equals b.Temp into cr
                                 from b in cr.DefaultIfEmpty()

                                 select new
                                 {
                                     ParticularA = b?.Particulars,
                                     DebitA = b?.Amount,


                                     ParticularL = a?.Particulars,
                                     DebitL = a?.Amount,


                                     ParentA = b?.Parent,
                                     ParentL = a?.Parent,

                                     AccountIdDr = a?.AccountId,
                                     AccountIdCr = b?.AccountId,
                                     Orders = (a.Orders == 0 || b.Orders == 0) ? 0 : 1
                                 });

            var rightOuterJoin = (from a in credit
                                  join b in debit on a.Temp equals b.Temp into dr
                                  from b in dr.DefaultIfEmpty()
                                  select new
                                  {
                                      ParticularA = a?.Particulars,
                                      DebitA = a?.Amount,

                                      ParticularL = b?.Particulars,
                                      DebitL = b?.Amount,

                                      ParentA = a?.Parent,
                                      ParentL = b?.Parent,

                                      AccountIdDr = b?.AccountId,
                                      AccountIdCr = a?.AccountId,
                                      Orders = (a.Orders == 0 || b.Orders == 0) ? 0 : 1
                                  });

            var full = leftOuterJoin.Union(rightOuterJoin);

            #endregion





            //    #region PLCALCU1


            //    //sales price.
            //                   (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0) && i.Account == 1
            //                   && i.Purpose == "Sale" && i.Status == null
            //                   //group i by i.SalesEntryId into g
            //                       Total = i.Credit



            //    //sales return price.
            //                      (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0)
            //                      && i.Account == 1 && i.Purpose == "Sale Return"
            //                      && i.Status == null
            //                          Total = i.Debit


            //    //purchase price
            //                   (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0)
            //                    && i.Account == 2 && i.Purpose == "Purchase"
            //                    && i.Status == null
            //                       Total = i.Debit

            //    //purchase return price
            //                      (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0)
            //                      && i.Account == 2 && i.Purpose == "Purchase Return"
            //                      && i.Status == null
            //                          Total = i.Credit


            //    ////direct expenses

            //    ////in direct expenses



            //    //direct income


            //    //in direct income


            //    //direct expenses


            //    //in direct expenses








            //    ////for gross profit b/d



            //    //--------------------------------------DEBIT SIDE---------------------------------------------

            //    //1

            //    ProfitAndLoss opens1 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "Opening Stock",
            //        ParentName = "Opening Stock",
            //        Amount = openstock,
            //        Orders = 0,
            //        Temp = increDr1

            //    //2
            //    ProfitAndLoss purchase1 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "Purchase",
            //        ParentName = "Purchase",
            //        Amount = Purchase,
            //        Orders = 0,
            //        Temp = increDr1

            //    //3
            //    ProfitAndLoss purchaseentrys1 = new ProfitAndLoss()
            //        Parent = 1,
            //        AccountId = 2,
            //        Particulars = "Purchase",
            //        ParentName = "Purchase",
            //        Amount = pprice1,
            //        Orders = 0,
            //        Temp = increDr1

            //    //4
            //    ProfitAndLoss purchasereturns1 = new ProfitAndLoss()
            //        Parent = 1,
            //        AccountId = 2,
            //        Particulars = "Purchase Return",
            //        ParentName = "Purchase Return",
            //        Amount = pretprice1,
            //        Orders = 0,
            //        Temp = increDr1



            //    ProfitAndLoss closes1 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "Closing Stock",
            //        Amount = closestock,
            //        Orders = 0,
            //        Temp = increCr1

            //    //2
            //    ProfitAndLoss sale1 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "Sale",
            //        Amount = Sales,
            //        Orders = 0,
            //        Temp = increCr1

            //    //3
            //    ProfitAndLoss saleentrys1 = new ProfitAndLoss()
            //        Parent = 1,
            //        AccountId = 1,
            //        Particulars = "Sale",
            //        Amount = sprice1,
            //        Orders = 0,
            //        Temp = increCr1

            //    //4
            //    ProfitAndLoss salereturns1 = new ProfitAndLoss()
            //        Parent = 1,
            //        AccountId = 1,
            //        Particulars = "Sales Return",
            //        Amount = sretprice1,
            //        Orders = 0,
            //        Temp = increCr1




            //    ProfitAndLoss gfnulls1 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "",
            //        ParentName = "",
            //        Amount = null,
            //        Orders = 1,
            //        Temp = 0


            //    //debit-union




            //    //credit -union



            //    //5
            //    //var dirxp = (from a in dexpenses
            //    //             select new ProfitAndLoss
            //    //                 Parent = a.Parent,
            //    //                 AccountId = null,
            //    //                 Particulars = a.Particulars,
            //    //                 ParentName = a.ParentName,
            //    //                 Amount = a.Debit - a.Credit,
            //    //                 Orders = 1,
            //    //                 Temp = increDr

            //        ProfitAndLoss entrys = new ProfitAndLoss()
            //            Parent = 0,
            //            AccountId = null,
            //            Particulars = entry.Particulars,
            //            ParentName = entry.ParentName,
            //            Orders = 0,
            //            Temp = increDr1


            //                                  where a.Account == acc.AccountsID && a.Status == null
            //                                  && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
            //                                  && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
            //                                      a.Debit,

            //                                   where a.Account == acc.AccountsID && a.Status == null
            //                                   && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
            //                                   && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
            //                                       a.Credit,


            //                diexpacc1.Add(new ProfitAndLoss
            //                    Parent = 1,
            //                    AccountId = acc.AccountsID,
            //                    Particulars = acc.Name,
            //                    ParentName = acc.Name,
            //                    Orders = 1,
            //                    Temp = increDr1,


            //    .Select((o, index) => new ProfitAndLoss
            //        Parent = o.Parent,
            //        AccountId = o.AccountId,
            //        Particulars = o.Particulars,
            //        ParentName = o.ParentName,
            //        Amount = o.Amount,
            //        Orders = o.Orders,
            //        Temp = (increDr1 + index)
            //    List<ProfitAndLoss> diexpaccnew1 = diexpaccs1.ToList(); //convert to list


            //    ////5
            //    //var dirincom = (from a in dirincome
            //    //                select new ProfitAndLoss
            //    //                    Parent = 0,//a.Parent 
            //    //                    AccountId = null,
            //    //                    Particulars = a.Particulars,
            //    //                    ParentName = a.ParentName,
            //    //                    Amount = a.Credit - a.Debit,
            //    //                    Orders = 0,
            //    //                    Temp = increCr


            //        ProfitAndLoss entrys = new ProfitAndLoss()
            //            Parent = 0,
            //            AccountId = null,
            //            Particulars = entry.Particulars,
            //            ParentName = entry.ParentName,
            //            Orders = 0,
            //            Temp = increCr1


            //                                  where a.Account == acc.AccountsID && a.Status == null
            //                                  && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
            //                                  && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
            //                                      a.Debit,

            //                                   where a.Account == acc.AccountsID && a.Status == null
            //                                   && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
            //                                   && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
            //                                       a.Credit,

            //                dirincomacc1.Add(new ProfitAndLoss
            //                    Parent = 1,
            //                    AccountId = acc.AccountsID,
            //                    Particulars = acc.Name,
            //                    ParentName = acc.Name,
            //                    Orders = 1,
            //                    Temp = increCr1,



            //    .Select((o, index) => new ProfitAndLoss
            //        Parent = o.Parent,
            //        AccountId = o.AccountId,
            //        Particulars = o.Particulars,
            //        ParentName = o.ParentName,
            //        Amount = o.Amount,
            //        Orders = o.Orders,
            //        Temp = (increCr1 + index)

            //    List<ProfitAndLoss> dirincomaccnew1 = dirincomaccs1.ToList(); //convert to list






            //    //fiilling null rows direct exp & income
            //            dirincomaccnew1.Add(new ProfitAndLoss
            //                Parent = 0,
            //                AccountId = null,
            //                Particulars = "",
            //                ParentName = "",
            //                Amount = null,
            //                Orders = 1,
            //                Temp = tcount
            //            diexpaccnew1.Add(new ProfitAndLoss
            //                Parent = 0,
            //                AccountId = null,
            //                Particulars = "",
            //                ParentName = "",
            //                Amount = null,
            //                Orders = 1,
            //                Temp = tcount1



            //    //for gross profit
            //    ProfitAndLoss gfnull11 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "",
            //        ParentName = "",
            //        Amount = null,
            //        Orders = 0,
            //        Temp = 0


            //    ProfitAndLoss gfnull21 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "",
            //        ParentName = "",
            //        Amount = null,
            //        Orders = 0,
            //        Temp = 0

            //        ProfitAndLoss gf1 = new ProfitAndLoss()
            //            Parent = 0,
            //            AccountId = null,
            //            Particulars = "Gross Profit",
            //            ParentName = "Gross Profit",
            //            Amount = TotalDr11,
            //            Orders = 0,
            //            Temp = increDr1


            //        ProfitAndLoss gl1 = new ProfitAndLoss()
            //            Parent = 0,
            //            AccountId = null,
            //            Particulars = "Gross Loss",
            //            ParentName = "Gross Loss",
            //            Amount = TotalDr11,
            //            Orders = 0,
            //            Temp = increCr1



            //    //first total
            //    ProfitAndLoss TotalDr111 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "Total",
            //        ParentName = "Total",
            //        Amount = FirstTotalDr1,
            //        Orders = 0,
            //        Temp = increDr1

            //    //first total
            //    ProfitAndLoss TotalCr111 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "Total",
            //        ParentName = "Total",
            //        Amount = FirstTotalCr1,
            //        Orders = 0,
            //        Temp = increCr1



            //        ProfitAndLoss badde1 = new ProfitAndLoss()
            //            Parent = 0,
            //            AccountId = null,
            //            Particulars = "Gross Loss b/d",
            //            ParentName = "Gross Loss b/d",
            //            Amount = TotalDr11,
            //            Orders = 0,
            //            Temp = increDr1


            //        ProfitAndLoss baddep1 = new ProfitAndLoss()
            //            Parent = 0,
            //            AccountId = null,
            //            Particulars = "Gross Profit b/d",
            //            ParentName = "Gross Profit b/d",
            //            Amount = TotalDr11,
            //            Orders = 0,
            //            Temp = increCr1




            //    //gross profit/loss union

            //    //first total union


            //    //gross profit/loss b/d union

            //    //indirect expense
            //    //var indirxp = (from a in indexpenses
            //    //               select new ProfitAndLoss
            //    //                   Parent = 0,//a.Parent,
            //    //                   AccountId = null,
            //    //                   Particulars = a.Particulars,
            //    //                   ParentName = a.ParentName,
            //    //                   Amount = a.Debit - a.Credit,
            //    //                   Orders = a.orderB,
            //    //                   Temp = increDr

            //    ////indirect expense accounts
            //    //var indiexpaccss = (from a in db.Accountss
            //    //                    where a.Group == 30
            //    //                    let acdebit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Debit)
            //    //                    let accredit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Credit)

            //    //                    //(todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)
            //    //                    select new ProfitAndLoss
            //    //                        Parent = 1,
            //    //                        AccountId = a.AccountsID,
            //    //                        Particulars = a.Name,
            //    //                        ParentName = a.Name,
            //    //                        //Amount = b != null ? 
            //    //                        //Amount = b != null ? db.AccountsTransactions.Where(c => c.Account == a.AccountsID).Select(c => c.Debit - c.Credit).Sum() : 0,
            //    //                        Amount = acdebit - accredit,
            //    //                        Orders = 1,
            //    //                        Temp = increDr


            //        ProfitAndLoss entrys = new ProfitAndLoss()
            //            Parent = 0,
            //            AccountId = entry.AccountsGroupID,
            //            Particulars = entry.Particulars,
            //            ParentName = entry.ParentName,
            //            Orders = 0,
            //            Temp = increDr1


            //                                  where a.Account == acc.AccountsID && a.Status == null
            //                                  && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
            //                                  && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
            //                                      a.Debit,

            //                                   where a.Account == acc.AccountsID && a.Status == null
            //                                   && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
            //                                   && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
            //                                       a.Credit,


            //                indiexpaccss1.Add(new ProfitAndLoss
            //                    //Parent = 1,
            //                    //AccountId = acc.AccountsID,
            //                    //Particulars = acc.Name,
            //                    //ParentName = acc.Name,
            //                    //Orders = 1,
            //                    //Temp = increDr,



            //    //--for ordering account based on index-indirect exp
            //    .Select((o, index) => new ProfitAndLoss
            //        Parent = o.Parent,
            //        AccountId = o.AccountId,
            //        Particulars = o.Particulars,
            //        ParentName = o.ParentName,
            //        Amount = o.Amount,
            //        Orders = o.Orders,
            //        Temp = (increDr1 + index)



            //    //indirect income
            //    //var indirincom = (from a in indirincome
            //    //                  select new ProfitAndLoss
            //    //                      Parent = 0, //a.Parent,
            //    //                      AccountId = null,
            //    //                      Particulars = a.Particulars,
            //    //                      ParentName = a.ParentName,
            //    //                      Amount = a.Credit - a.Debit,
            //    //                      Orders = 1,
            //    //                      Temp = increCr

            //    ////indirect income accounts
            //    //var indirincomaccss = (from a in db.Accountss
            //    //                           //join b in db.AccountsTransactions on a.AccountsID equals b.Account into accs
            //    //                           //from b in accs.DefaultIfEmpty()
            //    //                       where a.Group == 32
            //    //                       let acdebit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Debit)
            //    //                       let accredit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Credit)

            //    //                       //(todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)
            //    //                       select new ProfitAndLoss
            //    //                           Parent = 1,
            //    //                           AccountId = a.AccountsID,
            //    //                           Particulars = a.Name,
            //    //                           ParentName = a.Name,
            //    //                           Amount = accredit - acdebit,
            //    //                           //Amount = b != null ? db.AccountsTransactions.Where(c => c.Account == a.AccountsID && (fromdate == "" || EF.Functions.DateDiffDay(b.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)).Select(c => c.Debit - c.Credit).Sum() : 0,
            //    //                           Orders = 1,
            //    //                           Temp = increCr




            //        ProfitAndLoss entrys = new ProfitAndLoss()
            //            Parent = 0,
            //            AccountId = entry.AccountsGroupID,
            //            Particulars = entry.Particulars,
            //            ParentName = entry.ParentName,
            //            Orders = 0,
            //            Temp = increCr1


            //                                  where a.Account == acc.AccountsID && a.Status == null
            //                                  && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
            //                                  && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
            //                                      a.Debit,

            //                                   where a.Account == acc.AccountsID && a.Status == null
            //                                   && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
            //                                   && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
            //                                       a.Credit,


            //                indirincomaccss1.Add(new ProfitAndLoss
            //                    //Parent = 1,
            //                    //AccountId = acc.AccountsID,
            //                    //Particulars = acc.Name,
            //                    //ParentName = acc.Name,
            //                    //Orders = 1,
            //                    //Temp = increCr,



            //    //--for ordering account based on index-indirect income
            //    .Select((o, index) => new ProfitAndLoss
            //        Parent = o.Parent,
            //        AccountId = o.AccountId,
            //        Particulars = o.Particulars,
            //        ParentName = o.ParentName,
            //        Amount = o.Amount,
            //        Orders = o.Orders,
            //        Temp = (increCr1 + index)




            //    //fiilling null rows indirect expense/income
            //            indirincomaccnew1.Add(new ProfitAndLoss
            //                Parent = 0,
            //                AccountId = null,
            //                Particulars = "",
            //                ParentName = "",
            //                Amount = null,
            //                Orders = 1,
            //                Temp = tcount
            //            indiexpaccnew1.Add(new ProfitAndLoss
            //                Parent = 0,
            //                AccountId = null,
            //                Particulars = "",
            //                ParentName = "",
            //                Amount = null,
            //                Orders = 1,
            //                Temp = tcount



            //    // --------------------------------------------------------

            //    {//profit
            //        ProfitAndLoss netp1 = new ProfitAndLoss()
            //            Parent = 0,
            //            AccountId = null,
            //            Particulars = "Net Profit",
            //            ParentName = "Net Profit",
            //            Amount = Profit1,
            //            Orders = 0,
            //            Temp = increDr1

            //    //Second total
            //    ProfitAndLoss TotalDr21 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "Total",
            //        ParentName = "Total",
            //        Amount = SecondTotalDr,
            //        Orders = 0,
            //        Temp = increDr1



            //    {//loss
            //        ProfitAndLoss netl = new ProfitAndLoss()
            //            Parent = 0,
            //            AccountId = null,
            //            Particulars = "Net Loss",
            //            ParentName = "Net Loss",
            //            Amount = Profit1,
            //            Orders = 0,
            //            Temp = increCr1


            //    //second total
            //    ProfitAndLoss TotalCr21 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "Total",
            //        ParentName = "Total",
            //        Amount = SecondTotalCr1,
            //        Orders = 0,
            //        Temp = increCr1



            //    ProfitAndLoss gfnull31 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "",
            //        ParentName = "",
            //        Amount = null,
            //        Orders = 0,
            //        Temp = 0

            //    //net profit/loss union

            //    //final total union




            //    //outer joins-for single row

            //                             ParticularA = b?.Particulars,
            //                             DebitA = b?.Amount,


            //                             ParticularL = a?.Particulars,
            //                             DebitL = a?.Amount,


            //                             ParentA = b?.Parent,
            //                             ParentL = a?.Parent,

            //                             AccountIdDr = a?.AccountId,
            //                             AccountIdCr = b?.AccountId,
            //                             Orders = (a.Orders == 0 || b.Orders == 0) ? 0 : 1

            //                              ParticularA = a?.Particulars,
            //                              DebitA = a?.Amount,

            //                              ParticularL = b?.Particulars,
            //                              DebitL = b?.Amount,

            //                              ParentA = a?.Parent,
            //                              ParentL = b?.Parent,

            //                              AccountIdDr = b?.AccountId,
            //                              AccountIdCr = a?.AccountId,
            //                              Orders = (a.Orders == 0 || b.Orders == 0) ? 0 : 1


            //    #endregion


            //change to view model
            vmodel.ProfitAndLossDisplay = (from a in full
                                           select new ProfitAndLossDisplay
                                           {
                                               ParticularA = a?.ParticularA,
                                               DebitA = a?.DebitA,

                                               ParticularL = a?.ParticularL,
                                               DebitL = a?.DebitL,

                                               ParentA = a?.ParentA,
                                               ParentL = a?.ParentL,

                                               AccountIdDr = a?.AccountIdDr,
                                               AccountIdCr = a?.AccountIdCr,

                                               Orders = a.Orders
                                           }).ToList();
            return View(vmodel);
        }
        #endregion

        // [QkAuthorize(Roles = "Dev,ProfitAndLossExpense")]
        public ActionResult ProfitAndLossExpense()
        {
            ViewBag.ListCust = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);
            companySet();
            return View();
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,ProfitAndLossExpense")]
        public ActionResult GetProfitAndLossExpense(string fromdate, string todate, long? accgroup, bool excluderoudoff = true)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            BalanceSheetViewModel vmodel = new BalanceSheetViewModel();
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
            vmodel.from = fdate;
            vmodel.to = tdate;






            var v = (from a in db.Accountss
                     join b in db.AccountsTransactions on a.AccountsID equals b.Account
                     join c in db.AccountsGroups on a.Group equals c.AccountsGroupID
                     where (c.Parent == 30 || c.AccountsGroupID == 30 || c.Parent == 29 || c.AccountsGroupID == 29) &&
                     b.Date >= fdate && b.Date <= tdate &&
                     (accgroup == null || accgroup == 0 || accgroup == -1 || a.Group == accgroup)
                     select new expenseclas
                     {

                         pupose = b.Purpose,
                         reference = b.reference,
                         Date = b.Date,

                         balance = (b.Debit - b.Credit),
                         Name = c.Name,
                         FullName = (string)(from x in db.AccountsTransactions
                                             join y in db.Accountss on x.Account equals y.AccountsID
                                             where x.reference == b.reference && x.Credit > 0 && x.Purpose == b.Purpose
                                             select new
                                             {
                                                 y.Name
                                             }).FirstOrDefault().Name + "->"
                                             +
                                             (from x in db.AccountsTransactions
                                              join y in db.Accountss on x.Account equals y.AccountsID
                                              where x.reference == b.reference && x.Debit > 0
                                              && x.Purpose == b.Purpose
                                              select new
                                              {
                                                  y.Name
                                              }).FirstOrDefault().Name




                     }).Where(o => o.balance > 0).OrderByDescending(o => o.Name).ThenBy(o => o.FullName);
            string headname = "";
            int i = 0;
            var da = v.ToList();
            decimal zero = 0;
            foreach (var vv in v)
            {
                if (headname != vv.Name)
                {
                    expenseclas x = new expenseclas();



                    x.Name = vv.Name;
                    x.balance = null;
                    x.FullName = "";
                    x.pupose = "";


                    da.Insert(i, x);
                    i = i + 1;
                    headname = vv.Name;
                }
                i++;
            }

            var data = da;
            recordsTotal = da.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string resultss = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = resultss,
                ContentType = "application/json"
            };
            return results;
        }

        public class expenseclas
        {
            public string pupose { get; set; }
            public long reference { get; set; }
            public DateTime? Date { get; set; }
            public decimal? balance { get; set; }
            public string Name { get; set; }
            public string FullName { get; set; }

        }
        #region Profit and Loss
        [QkAuthorize(Roles = "Dev,ProfitAndLoss")]
        public ActionResult ProfitAndLossShowroom()
        {
            var cn = System.Configuration.ConfigurationManager.ConnectionStrings.Count;


            var a = System.Configuration.ConfigurationManager.ConnectionStrings;

            int i = 0;
            List<SelectListItem> sel = new List<SelectListItem>();
            for (i = 0; i < cn; i++)
            {
                SelectListItem k = new SelectListItem { Text = a[i].Name, Value = i.ToString() };
                sel.Add(k);
            }
            var mcs = db.MCs.Where(p => p.Status == Status.active && !p.MCName.Contains("old-")).Select(s => new
            {
                Id = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
            ViewBag.connections = QkSelect.List(sel, "Value", "Text");
            _FinancialYear();
            return View();
        }
        [QkAuthorize(Roles = "Dev,ProfitAndLoss")]
        public ActionResult GetProfitAndLossShowroom(long? selcompany, long? ddlMC, string fromdate, string todate)
        {
            long? mc = ddlMC;
            BalanceSheetViewModel vmodel = new BalanceSheetViewModel();
            DateTime? fdate = null;
            DateTime? tdate = null;
            var fun = 2;
            int Ret = 0;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            vmodel.from = fdate;
            vmodel.to = tdate;
            vmodel.showroom = db.MCs.Where(o => o.MCId == ddlMC).Select(o => o.MCName).FirstOrDefault();
            if (selcompany != null)
            {
                var cn = System.Configuration.ConfigurationManager.ConnectionStrings.Count;


                var aa = System.Configuration.ConfigurationManager.ConnectionStrings;

                db = new ApplicationDbContext(aa[(int)selcompany].Name);
            }
            #region PLCALCU
            var openstock = getOpeningStockmcshowroom(fromdate, "open", mc);
            var closestock = getOpeningStockmcshowroom(todate, "close", mc);

            openstock = openstock < 0 ? openstock * -1 : openstock;
            closestock = closestock < 0 ? closestock * -1 : closestock;
            var sprices = (from i in db.SalesEntrys
                           join j in db.SEItemss on i.SalesEntryId equals j.SalesEntry
                           where (fromdate == "" || EF.Functions.DateDiffDay(i.SEDate, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(i.SEDate, tdate) >= 0) &&
                          i.MaterialCenter == ddlMC
                              && j.Item != 30018 && j.Item != 75021
                           select new
                           {
                               j.ItemQuantity,
                               j.Item,
                               i.SalesEntryId,
                               i.SEDate,
                               j.SEItemsId,
                               j.ItemUnitPrice



                           }).ToList().Select(o => new
                           {

                               Total = (o.ItemQuantity - getsalesreturn(o.SalesEntryId, o.Item)) * o.ItemUnitPrice


                           }).ToList();


            decimal sprice = sprices != null ? sprices.Sum(a => a.Total) : 0;

            var discountt = (decimal?)(from ayy in db.BillSundrys
                                       join azz in db.SEBillSundrys on ayy.BillSundryId equals azz.BillSundry
                                       join i in db.SalesEntrys on azz.SalesEntry equals i.SalesEntryId

                                       where (fromdate == "" || EF.Functions.DateDiffDay(i.SEDate, fdate) <= 0) &&
                                       (todate == "" || EF.Functions.DateDiffDay(i.SEDate, tdate) >= 0) &&
                                      i.MaterialCenter == ddlMC
                                       where ayy.BSName == "DISCOUNT"
                                       select new
                                       {
                                           BsAmount = (azz.BsAmount == null) ? 0 : azz.BsAmount
                                       }
                                              ).Sum(x => x.BsAmount);
            var subpaymentexpense = (from i in db.SalesEntrys
                                     let PaymentExpense = (decimal?)(db.Payments.Where(x => x.InvoiceNo == i.BillNo).Select(x => x.Paying).Sum()) ?? 0
                                     where (fromdate == "" || EF.Functions.DateDiffDay(i.SEDate, fdate) <= 0) &&
                                     (todate == "" || EF.Functions.DateDiffDay(i.SEDate, tdate) >= 0) &&
                                     i.MaterialCenter == ddlMC
                                     select new
                                     {
                                         pay = PaymentExpense
                                     }).Sum(x => x.pay);
            var jornalexpense = (from i in db.SalesEntrys
                                 let PaymentExpense = (decimal?)(db.Journals.Where(y => y.InvoiceNo == i.BillNo).Select(y => y.Paying).Sum()) ?? 0
                                 where (fromdate == "" || EF.Functions.DateDiffDay(i.SEDate, fdate) <= 0) &&
                                     (todate == "" || EF.Functions.DateDiffDay(i.SEDate, tdate) >= 0) &&
                                     i.MaterialCenter == ddlMC
                                 select new
                                 {
                                     pay = PaymentExpense
                                 }).Sum(x => x.pay);


            sprice = sprice - (decimal)discountt - (decimal)subpaymentexpense - (decimal)jornalexpense;

            decimal sretprice = 0;

            //               //let discountt = (decimal?)(from ayy in db.BillSundrys
            //               //                       join azz in db.SEBillSundrys on ayy.BillSundryId equals azz.BillSundry

            //               //                       where ayy.BSName == "DISCOUNT" && azz.SalesEntry == i.SalesEntryId
            //               //                       select new
            //               //                           BsAmount = (azz.BsAmount == null) ? 0 : azz.BsAmount
            //               //                   ).Sum(x => x.BsAmount) ?? 0
            //              i.MaterialCenter == ddlMC

            //                   i.SalesEntryId,
            //                   Total = i.SESubTotal,//  i.SEGrandTotal




            //              i.MaterialCenter == ddlMC 
            //              && (j.Item== 30018 || j.Item== 75021)

            //                   i.SalesEntryId,
            //                   Total = i.SESubTotal,//  i.SEGrandTotal



            //sales return price.
            //                  (todate == "" || EF.Functions.DateDiffDay(i.SRDate, tdate) >= 0)
            //                  && i.MaterialCenter == ddlMC

            //                      i.SalesReturnId,
            //                      Total = i.SRSubTotal

            //                  (todate == "" || EF.Functions.DateDiffDay(i.SRDate, tdate) >= 0)
            //                  && i.MaterialCenter == ddlMC
            //                  && (j.Item == 30018 || j.Item == 75021)
            //                      i.SalesReturnId,
            //                      Total =i.SRSubTotal
            ////sretprice =sretprice - tempsretprice;
            //purchase price
            var pprices = (from i in db.PurchaseEntrys
                           join j in db.PEItemss on i.PurchaseEntryId equals j.PurchaseEntry
                           where (fromdate == "" || EF.Functions.DateDiffDay(i.PEDate, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(i.PEDate, tdate) >= 0)
                            && i.MaterialCenter == ddlMC
                           select new
                           {
                               i.PurchaseEntryId,
                               Total = i.PEGrandTotal,
                           }).Distinct().ToList();
            decimal pprice = (pprices != null) ? pprices.Sum(a => a.Total) : 0;


            //purchase return price
            var pretprices = (from i in db.PurchaseReturns
                              join j in db.PRItemss on i.PurchaseReturnId equals j.PurchaseReturnId
                              where (fromdate == "" || EF.Functions.DateDiffDay(i.PRDate, fdate) <= 0) &&
                              (todate == "" || EF.Functions.DateDiffDay(i.PRDate, tdate) >= 0)
                               && i.MaterialCenter == ddlMC

                              select new
                              {
                                  i.purchaseEntryId,
                                  Total = i.PRGrandTotal
                              }).Distinct().ToList();
            decimal pretprice = (pretprices != null) ? pretprices.Sum(a => a.Total) : 0;
            var stockin = (from i in db.StockTransfers
                           join j in db.StockTransferItems on i.Id equals j.StockTransferId
                           where (fromdate == "" || EF.Functions.DateDiffDay(i.Date, fdate) <= 0) &&
                              (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0) &&
                             i.MCTo == ddlMC

                           select new
                           {
                               i.Id,
                               //Total = (j.Unit == k.ItemUnitID) ? j.Quantity * k.PurchasePrice : j.Quantity * k.PurchasePrice / k.ConFactor
                               Total = i.TotalAmount
                           }).Distinct().ToList();
            decimal stockinprice = (stockin != null) ? stockin.Sum(a => a.Total) : 0;
            var stockout = (from i in db.StockTransfers
                            join j in db.StockTransferItems on i.Id equals j.StockTransferId
                            where (fromdate == "" || EF.Functions.DateDiffDay(i.Date, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0) &&
                          i.MCFrom == ddlMC

                            select new
                            {
                                i.Id,
                                //Total = (j.Unit == k.ItemUnitID) ? j.Quantity * k.PurchasePrice : j.Quantity * k.PurchasePrice / k.ConFactor
                                Total = i.TotalAmount
                            }).Distinct().ToList();
            decimal stockoutprice = (stockout != null) ? stockout.Sum(a => a.Total) : 0;
            var assetinvtransfer = (from i in db.AssetToInventoryMasters
                                    join j in db.AssetToInventoryDetails on i.EntryId equals j.EntryId
                                    join k in db.Items on j.RefItemId equals k.ItemID
                                    where (fromdate == "" || EF.Functions.DateDiffDay(i.EntryDate, fdate) <= 0) &&
                                    (todate == "" || EF.Functions.DateDiffDay(i.EntryDate, tdate) >= 0) &&
                                   i.McFromId == ddlMC

                                    select new
                                    {
                                        Total = j.Price * j.Quantity

                                    }).Distinct().ToList();
            decimal assetin = (assetinvtransfer != null) ? assetinvtransfer.Sum(a => a.Total) : 0;
            var assettransfer = (from i in db.AssetTransferMasters
                                 join j in db.AssetTransferDetails on i.AssetEntryId equals j.AssetEntryId
                                 join k in db.Items on j.RefItemId equals k.ItemID
                                 where (fromdate == "" || EF.Functions.DateDiffDay(i.AssetEntryDate, fdate) <= 0) &&
                                 (todate == "" || EF.Functions.DateDiffDay(i.AssetEntryDate, tdate) >= 0) &&
                                i.McFromId == ddlMC

                                 select new
                                 {
                                     Total = j.Price * j.Quantity
                                 }).Distinct().ToList();
            decimal assetout = (assettransfer != null) ? assettransfer.Sum(a => a.Total) : 0;

            var damagein = (from i in db.StockAdjustments

                            where (fromdate == "" || EF.Functions.DateDiffDay(i.AdjDate, fdate) <= 0) &&
                                (todate == "" || EF.Functions.DateDiffDay(i.AdjDate, tdate) >= 0) &&


                    i.MaterialCenter == mc
                    &&
                    i.AdjustmentType == AdjustmentType.Add


                            select new
                            {
                                Total = i.PurchaseRate * i.ItemQuantity
                            }).ToList();
            decimal damageintotal = (damagein != null) ? damagein.Sum(a => a.Total) : 0;

            var damageout = (from i in db.StockAdjustments
                             where (fromdate == "" || EF.Functions.DateDiffDay(i.AdjDate, fdate) <= 0) &&
                                 (todate == "" || EF.Functions.DateDiffDay(i.AdjDate, tdate) >= 0) &&


                      i.MaterialCenter == mc
                      &&
                      i.AdjustmentType == AdjustmentType.Less


                             select new
                             {
                                 Total = i.PurchaseRate * i.ItemQuantity
                             }).ToList();
            decimal damageouttotal = (damageout != null) ? damageout.Sum(a => a.Total) : 0;





            decimal TotalDr = 0;
            decimal TotalCr = 0;
            decimal GTotalDr = 0;
            decimal GTotalCr = 0;
            decimal Profit = 0;

            ////for gross profit b/d
            List<ProfitAndLoss> baddepthdr = new List<ProfitAndLoss>();
            List<ProfitAndLoss> baddepthcr = new List<ProfitAndLoss>();

            List<ProfitAndLoss> net = new List<ProfitAndLoss>();


            //--------------------------------------DEBIT SIDE---------------------------------------------

            //1


            int increDr = 0;
            increDr++;
            ProfitAndLoss opens = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "Opening Stock",
                ParentName = "Opening Stock",
                Amount = (decimal)openstock,
                Orders = 0,
                Temp = increDr
            };
            List<ProfitAndLoss> opstk = new List<ProfitAndLoss>();
            opstk.Add(opens);

            //2
            increDr++;
            var Sales = sprice + stockoutprice + damageouttotal + assetout - sretprice;
            var Purchase = stockinprice + pprice + damageintotal + assetin - pretprice;
            ProfitAndLoss purchaseentryss = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "Total Purchase",
                ParentName = "Total Purchase",
                Amount = Purchase,
                Orders = 0,
                Temp = increDr
            };
            List<ProfitAndLoss> purchaseses = new List<ProfitAndLoss>();
            purchaseses.Add(purchaseentryss);
            var debit = opstk.Union(purchaseses);
            increDr++;
            ProfitAndLoss purchaseentrys = new ProfitAndLoss()
            {
                Parent = 1,
                AccountId = 2,
                Particulars = "Purchase",
                ParentName = "Purchase",
                Amount = pprice,
                Orders = 0,
                Temp = increDr
            };
            List<ProfitAndLoss> purchases = new List<ProfitAndLoss>();
            purchases.Add(purchaseentrys);
            increDr++;

            debit = debit.Union(purchases);


            ProfitAndLoss stocktransfer = new ProfitAndLoss()
            {
                Parent = 1,
                AccountId = null,
                Particulars = "Stock Recieved",
                ParentName = "Stock Recieved",
                Amount = stockinprice,
                Orders = 0,
                Temp = increDr
            };
            List<ProfitAndLoss> stocktransfers = new List<ProfitAndLoss>();
            stocktransfers.Add(stocktransfer);
            debit = debit.Union(stocktransfers);
            ProfitAndLoss stockadjin = new ProfitAndLoss()
            {
                Parent = 1,
                AccountId = null,
                Particulars = "Stock Adj IN",
                ParentName = "Stock Adj IN",
                Amount = damageintotal,
                Orders = 0,
                Temp = increDr
            };
            List<ProfitAndLoss> stockadjins = new List<ProfitAndLoss>();
            stockadjins.Add(stockadjin);
            debit = debit.Union(stockadjins);
            ProfitAndLoss assetrecived = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = 1,
                Particulars = "Asset Recieved",
                ParentName = "Asset Recieved",
                Amount = assetin,
                Orders = 0,
                Temp = increDr
            };
            List<ProfitAndLoss> assetins = new List<ProfitAndLoss>();
            assetins.Add(assetrecived);
            //4
            increDr++;
            ProfitAndLoss assetgone = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = 1,
                Particulars = "Transfer To Asset",
                ParentName = "Transfer To Asset",
                Amount = assetin,
                Orders = 0,
                Temp = increDr
            };
            List<ProfitAndLoss> assetgones = new List<ProfitAndLoss>();
            assetgones.Add(assetgone);

            increDr++;




            ProfitAndLoss purchasereturns = new ProfitAndLoss()
            {
                Parent = 1,
                AccountId = 2,
                Particulars = "Purchase Return",
                ParentName = "Purchase Return",
                Amount = pretprice,
                Orders = 0,
                Temp = increDr
            };
            List<ProfitAndLoss> purchasereturn = new List<ProfitAndLoss>();
            purchasereturn.Add(purchasereturns);
            debit = debit.Union(purchasereturn);


            int increCr = 0;
            increCr++;
            ProfitAndLoss closes = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "Closing Stock",
                Amount = (decimal)closestock,
                Orders = 0,
                Temp = increCr
            };
            List<ProfitAndLoss> clsstk = new List<ProfitAndLoss>();
            clsstk.Add(closes);

            //2
            increCr++;



            //3
            ProfitAndLoss saleentrys = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "Total Sale",
                Amount = Sales,
                Orders = 0,
                Temp = increCr
            };
            List<ProfitAndLoss> saleentry = new List<ProfitAndLoss>();
            saleentry.Add(saleentrys);
            var credit = clsstk.Union(saleentry);
            increCr++;

            ProfitAndLoss saleentryss = new ProfitAndLoss()
            {
                Parent = 1,
                AccountId = 1,
                Particulars = "Sale",
                Amount = sprice,
                Orders = 0,
                Temp = increCr
            };
            List<ProfitAndLoss> saleentryy = new List<ProfitAndLoss>();
            saleentryy.Add(saleentryss);
            credit = credit.Union(saleentryy);

            //4
            increCr++;
            ProfitAndLoss salereturns = new ProfitAndLoss()
            {
                Parent = 1,
                AccountId = null,
                Particulars = "Sales Return",
                Amount = sretprice,
                Orders = 0,
                Temp = increCr
            };
            increCr++;

            List<ProfitAndLoss> salereturn = new List<ProfitAndLoss>();
            salereturn.Add(salereturns);
            ProfitAndLoss stocktransferout = new ProfitAndLoss()
            {
                Parent = 1,
                AccountId = null,
                Particulars = "Stock Transfer",
                ParentName = "Stock Transfer",
                Amount = stockoutprice,
                Orders = 0,
                Temp = increDr
            };
            List<ProfitAndLoss> stocktransferouts = new List<ProfitAndLoss>();
            stocktransferouts.Add(stocktransferout);
            credit = credit.Union(stocktransferouts);


            ProfitAndLoss stocktransferoutss = new ProfitAndLoss()
            {
                Parent = 1,
                AccountId = null,
                Particulars = "Stock adj out",
                ParentName = "Stock adj out",
                Amount = damageouttotal,
                Orders = 0,
                Temp = increDr
            };
            List<ProfitAndLoss> stocktransferoutsss = new List<ProfitAndLoss>();
            stocktransferouts.Add(stocktransferoutss);
            credit = credit.Union(stocktransferoutsss);



            var count1 = getOrderingPF(debit);
            var count2 = getOrderingPF(credit);
            //outer joins-for single row
            //for gross profit
            List<ProfitAndLoss> pandf = new List<ProfitAndLoss>();
            List<ProfitAndLoss> nullrow1 = new List<ProfitAndLoss>();
            ProfitAndLoss gfnull1 = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "",
                ParentName = "",
                Amount = null,
                Orders = 0,
                Temp = 0
            };
            nullrow1.Add(gfnull1);


            List<ProfitAndLoss> nullrow2 = new List<ProfitAndLoss>();
            ProfitAndLoss gfnull2 = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "",
                ParentName = "",
                Amount = null,
                Orders = 0,
                Temp = 0
            };
            nullrow2.Add(gfnull2);
            var Debit = (decimal)openstock + Purchase;
            var Credit = (decimal)closestock + Sales;
            if (Debit < Credit)
            {
                TotalDr = (decimal)Credit - (decimal)Debit;
                increDr = debit.Count() + 2;
                pandf.Clear();
                ProfitAndLoss gf = new ProfitAndLoss()
                {
                    Parent = 0,
                    AccountId = null,
                    Particulars = "Gross Profit",
                    ParentName = "Gross Profit",
                    Amount = TotalDr,
                    Orders = 0,
                    Temp = increDr
                };
                pandf.Add(gf);
            }


            if (Debit > Credit)
            {
                increCr = credit.Count() + 2;
                pandf.Clear();
                TotalCr = (decimal)Debit - (decimal)Credit;
                ProfitAndLoss gl = new ProfitAndLoss()
                {
                    Parent = 0,
                    AccountId = null,
                    Particulars = "Gross Loss",
                    ParentName = "Gross Loss",
                    Amount = TotalCr,
                    Orders = 0,
                    Temp = increCr
                };
                pandf.Add(gl);
            }

            if (Debit < Credit)
            {
                debit = debit.Union(pandf);
                nullrow1[0].Temp = debit.Count() + 1;
                credit = credit.Union(nullrow1);
            }
            if (Debit > Credit)
            {
                credit = credit.Union(pandf);
                nullrow1[0].Temp = credit.Count() + 1;
                debit = debit.Union(nullrow1);
            }

            var leftOuterJoin = (from a in debit
                                 join b in credit on a.Temp equals b.Temp into cr
                                 from b in cr.DefaultIfEmpty()

                                 select new
                                 {
                                     ParticularA = b?.Particulars,
                                     DebitA = b?.Amount,


                                     ParticularL = a?.Particulars,
                                     DebitL = a?.Amount,


                                     ParentA = b?.Parent,
                                     ParentL = a?.Parent,

                                     AccountIdDr = a?.AccountId,
                                     AccountIdCr = b?.AccountId,
                                     Orders = (a.Orders == 0 || b.Orders == 0) ? 0 : 1
                                 });

            var rightOuterJoin = (from a in credit
                                  join b in debit on a.Temp equals b.Temp into dr
                                  from b in dr.DefaultIfEmpty()
                                  select new
                                  {
                                      ParticularA = a?.Particulars,
                                      DebitA = a?.Amount,

                                      ParticularL = b?.Particulars,
                                      DebitL = b?.Amount,

                                      ParentA = a?.Parent,
                                      ParentL = b?.Parent,

                                      AccountIdDr = b?.AccountId,
                                      AccountIdCr = a?.AccountId,
                                      Orders = (a.Orders == 0 || b.Orders == 0) ? 0 : 1
                                  });

            var full = leftOuterJoin.Union(rightOuterJoin);


            vmodel.ProfitAndLossDisplay = (from a in full
                                           select new ProfitAndLossDisplay
                                           {
                                               ParticularA = a?.ParticularA,
                                               DebitA = a?.DebitA,

                                               ParticularL = a?.ParticularL,
                                               DebitL = a?.DebitL,

                                               ParentA = a?.ParentA,
                                               ParentL = a?.ParentL,

                                               AccountIdDr = a?.AccountIdDr,
                                               AccountIdCr = a?.AccountIdCr,

                                               Orders = a.Orders
                                           }).ToList();
            return View(vmodel);
        }
        #endregion
        #endregion
        public ActionResult GetProfitAndLossMC(long? selcompany, long? ddlMC, string fromdate, string todate)
        {
            long? mc = ddlMC;
            BalanceSheetViewModel vmodel = new BalanceSheetViewModel();
            DateTime? fdate = null;
            DateTime? tdate = null;
            var fun = 2;
            int Ret = 0;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            vmodel.from = fdate;
            vmodel.to = tdate;



            #region PLCALCU
            var openstock = 0;// getOpeningStockmc(fromdate, "open", mc);
            var closestock = 0;// getOpeningStockmc(todate, "close", mc);

            openstock = openstock < 0 ? openstock * -1 : openstock;
            closestock = closestock < 0 ? closestock * -1 : closestock;

            var sprices = (from i in db.AccountsTransactions
                           where (fromdate == "" || EF.Functions.DateDiffDay(i.Date, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0) && i.Account == 1
                           && i.Purpose == "Sale" && i.Status == null
                           //group i by i.SalesEntryId into g
                           select new
                           {
                               Total = i.Credit
                           }).ToList();


            decimal sprice = sprices != null ? sprices.Sum(a => a.Total) : 0;


            //sales return price.
            var sretprices = (from i in db.AccountsTransactions
                              where (fromdate == "" || EF.Functions.DateDiffDay(i.Date, fdate) <= 0) &&
                              (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0)
                              && i.Account == 1 && i.Purpose == "Sale Return"
                              && i.Status == null
                              select new
                              {
                                  Total = i.Debit
                              }).ToList();
            decimal sretprice = (sretprices != null) ? sretprices.Sum(a => a.Total) : 0;



            //purchase price
            var pprices = (from i in db.AccountsTransactions
                           where (fromdate == "" || EF.Functions.DateDiffDay(i.Date, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0)
                            && i.Account == 2 && i.Purpose == "Purchase"
                            && i.Status == null
                           select new
                           {
                               Total = i.Debit
                           }).ToList();
            decimal pprice = (pprices != null) ? pprices.Sum(a => a.Total) : 0;


            //purchase return price
            var pretprices = (from i in db.AccountsTransactions
                              where (fromdate == "" || EF.Functions.DateDiffDay(i.Date, fdate) <= 0) &&
                              (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0)
                              && i.Account == 2 && i.Purpose == "Purchase Return"
                              && i.Status == null
                              select new
                              {
                                  Total = i.Credit
                              }).ToList();
            decimal pretprice = (pretprices != null) ? pretprices.Sum(a => a.Total) : 0;


            //sales price.
            if (mc != 0)
            {
                sprices = (from i in db.AccountsTransactions
                           join j in db.SalesEntrys on i.reference equals j.SalesEntryId
                           where (fromdate == "" || EF.Functions.DateDiffDay(i.Date, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0) && i.Account == 1
                           && i.Purpose == "Sale" && i.Status == null &&
                           j.MaterialCenter == mc
                           //group i by i.SalesEntryId into g
                           select new
                           {
                               Total = i.Credit
                           }).ToList();


                sprice = sprices != null ? sprices.Sum(a => a.Total) : 0;


                //sales return price.
                sretprices = (from i in db.AccountsTransactions
                              join j in db.SalesReturns on i.reference equals j.SalesReturnId
                              where (fromdate == "" || EF.Functions.DateDiffDay(i.Date, fdate) <= 0) &&
                              (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0)
                              && i.Account == 1 && i.Purpose == "Sale Return"
                              && i.Status == null &&
                           j.MaterialCenter == mc
                              select new
                              {
                                  Total = i.Debit
                              }).ToList();
                sretprice = (sretprices != null) ? sretprices.Sum(a => a.Total) : 0;



                //purchase price
                pprices = (from i in db.AccountsTransactions
                           join j in db.PurchaseEntrys on i.reference equals j.PurchaseEntryId
                           where (fromdate == "" || EF.Functions.DateDiffDay(i.Date, fdate) <= 0) &&
                           (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0)
                            && i.Account == 2 && i.Purpose == "Purchase"
                            && i.Status == null &&
                           j.MaterialCenter == mc
                           select new
                           {
                               Total = i.Debit
                           }).ToList();
                pprice = (pprices != null) ? pprices.Sum(a => a.Total) : 0;


                //purchase return price
                pretprices = (from i in db.AccountsTransactions
                              join j in db.PurchaseReturns on i.reference equals j.PurchaseReturnId
                              where (fromdate == "" || EF.Functions.DateDiffDay(i.Date, fdate) <= 0) &&
                              (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0)
                              && i.Account == 2 && i.Purpose == "Purchase Return"
                              && i.Status == null &&
                           j.MaterialCenter == mc
                              select new
                              {
                                  Total = i.Credit
                              }).ToList();
                pretprice = (pretprices != null) ? pretprices.Sum(a => a.Total) : 0;

            }

            ////direct expenses

            ////in direct expenses



            //direct income
            var dirincome = Common.GetChildAccGroupmc(31, "Direct Income", "asset", fdate, tdate, fun, 1, mc);


            //in direct income
            var indirincome = Common.GetChildAccGroupmc(32, "InDirect Income", "asset", fdate, tdate, fun, 1, mc);


            //direct expenses
            var dexpenses = Common.GetChildAccGroupmc(29, "Expenses (Direct/Mfg.)", "liability", fdate, tdate, fun, 1, mc).ToList();


            //in direct expenses
            var indexpenses = Common.GetChildAccGroupmc(13, "Expenses (Indirect/Admn.)", "liability", fdate, tdate, fun, 1, mc).ToList();


            var Sales = sprice - sretprice;
            var Purchase = pprice - pretprice;

            var DirectExp = dexpenses != null ? (decimal)dexpenses.Where(a => a.orderB == 0).Sum(a => a.Debit - a.Credit) : 0;
            var InDirectExp = indexpenses != null ? (decimal)indexpenses.Where(a => a.orderB == 0).Sum(a => a.Debit - a.Credit) : 0;
            var DirectIncome = dirincome != null ? (decimal)dirincome.Where(a => a.orderB == 0).Sum(a => a.Credit - a.Debit) : 0;
            var InDirectIncome = indirincome != null ? (decimal)indirincome.Where(a => a.orderB == 0).Sum(a => a.Credit - a.Debit) : 0;

            DirectExp = DirectExp < 0 ? (DirectExp * -1) : DirectExp;
            InDirectExp = InDirectExp < 0 ? (InDirectExp * -1) : InDirectExp;
            DirectIncome = DirectIncome < 0 ? (DirectIncome * -1) : DirectIncome;
            InDirectIncome = InDirectIncome < 0 ? (InDirectIncome * -1) : InDirectIncome;


            var Debit = openstock + Purchase + DirectExp;
            var Credit = closestock + Sales + DirectIncome;

            decimal TotalDr = 0;
            decimal TotalCr = 0;
            decimal GTotalDr = 0;
            decimal GTotalCr = 0;
            decimal Profit = 0;

            ////for gross profit b/d
            List<ProfitAndLoss> baddepthdr = new List<ProfitAndLoss>();
            List<ProfitAndLoss> baddepthcr = new List<ProfitAndLoss>();

            List<ProfitAndLoss> net = new List<ProfitAndLoss>();


            //--------------------------------------DEBIT SIDE---------------------------------------------

            //1

            int increDr = 0;
            increDr++;
            ProfitAndLoss opens = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "Opening Stock",
                ParentName = "Opening Stock",
                Amount = openstock,
                Orders = 0,
                Temp = increDr
            };
            List<ProfitAndLoss> opstk = new List<ProfitAndLoss>();
            opstk.Add(opens);

            //2
            increDr++;
            ProfitAndLoss purchase = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "Purchase",
                ParentName = "Purchase",
                Amount = Purchase,
                Orders = 0,
                Temp = increDr
            };
            List<ProfitAndLoss> purchases = new List<ProfitAndLoss>();
            purchases.Add(purchase);

            //3
            increDr++;
            ProfitAndLoss purchaseentrys = new ProfitAndLoss()
            {
                Parent = 1,
                AccountId = 2,
                Particulars = "Purchase",
                ParentName = "Purchase",
                Amount = pprice,
                Orders = 0,
                Temp = increDr
            };
            List<ProfitAndLoss> purchaseentry = new List<ProfitAndLoss>();
            purchaseentry.Add(purchaseentrys);

            //4
            increDr++;
            ProfitAndLoss purchasereturns = new ProfitAndLoss()
            {
                Parent = 1,
                AccountId = 2,
                Particulars = "Purchase Return",
                ParentName = "Purchase Return",
                Amount = pretprice,
                Orders = 0,
                Temp = increDr
            };
            List<ProfitAndLoss> purchasereturn = new List<ProfitAndLoss>();
            purchasereturn.Add(purchasereturns);



            int increCr = 0;
            increCr++;
            ProfitAndLoss closes = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "Closing Stock",
                Amount = closestock,
                Orders = 0,
                Temp = increCr
            };
            List<ProfitAndLoss> clsstk = new List<ProfitAndLoss>();
            clsstk.Add(closes);

            //2
            increCr++;
            ProfitAndLoss sale = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "Sale",
                Amount = Sales,
                Orders = 0,
                Temp = increCr
            };
            List<ProfitAndLoss> sales = new List<ProfitAndLoss>();
            sales.Add(sale);

            //3
            increCr++;
            ProfitAndLoss saleentrys = new ProfitAndLoss()
            {
                Parent = 1,
                AccountId = 1,
                Particulars = "Sale",
                Amount = sprice,
                Orders = 0,
                Temp = increCr
            };
            List<ProfitAndLoss> saleentry = new List<ProfitAndLoss>();
            saleentry.Add(saleentrys);

            //4
            increCr++;
            ProfitAndLoss salereturns = new ProfitAndLoss()
            {
                Parent = 1,
                AccountId = 1,
                Particulars = "Sales Return",
                Amount = sretprice,
                Orders = 0,
                Temp = increCr
            };
            List<ProfitAndLoss> salereturn = new List<ProfitAndLoss>();
            salereturn.Add(salereturns);




            List<ProfitAndLoss> nullrows = new List<ProfitAndLoss>();
            ProfitAndLoss gfnulls = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "",
                ParentName = "",
                Amount = null,
                Orders = 1,
                Temp = 0
            };
            nullrows.Add(gfnulls);


            //debit-union
            var debit = opstk.Union(purchases);
            debit = debit.Union(purchaseentry);

            var pret = purchasereturn.Select(a => a.Amount).FirstOrDefault();
            var sret = salereturn.Select(a => a.Amount).FirstOrDefault();

            if (pret != 0)
            {
                debit = debit.Union(purchasereturn);
            }
            else
            {
                if (pret != 0 || sret != 0)
                {
                    nullrows[0].Temp = debit.Count() + 1;
                    debit = debit.Union(nullrows);
                }
            }


            //credit -union
            var credit = clsstk.Union(sales);
            credit = credit.Union(saleentry);

            if (sret != 0)
            {
                credit = credit.Union(salereturn);
            }
            else
            {
                if (pret != 0 || sret != 0)
                {
                    nullrows[0].Temp = credit.Count() + 1;
                    credit = credit.Union(nullrows);
                }
            }


            //5
            increDr = debit.Count() + 1;
            //                 Parent = a.Parent,
            //                 AccountId = null,
            //                 Particulars = a.Particulars,
            //                 ParentName = a.ParentName,
            //                 Amount = a.Debit - a.Credit,
            //                 Orders = 1,
            //                 Temp = increDr

            List<ProfitAndLoss> diexpacc = new List<ProfitAndLoss>();
            foreach (var entry in dexpenses)
            {
                ProfitAndLoss entrys = new ProfitAndLoss()
                {
                    Parent = 0,
                    AccountId = null,
                    Particulars = entry.Particulars,
                    ParentName = entry.ParentName,
                    Amount = (entry.Debit - entry.Credit) < 0 ? (entry.Debit - entry.Credit) * -1 : (entry.Debit - entry.Credit),
                    Orders = 0,
                    Temp = increDr
                };
                diexpacc.Add(entrys);

                var accounts = db.Accountss.Where(a => a.Group == entry.AccountsGroupID).ToList();
                foreach (var acc in accounts)
                {
                    var chkacc = db.AccountsTransactions.Where(a => a.Account == acc.AccountsID).ToList();
                    if (chkacc.Count() > 0)
                    {

                        decimal debits = (from a in db.AccountsTransactions
                                          where a.Account == acc.AccountsID && a.Status == null
                                          && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
                                          && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
                                          select new
                                          {
                                              a.Debit,
                                          }).ToList().Sum(x => x.Debit);

                        decimal credits = (from a in db.AccountsTransactions
                                           where a.Account == acc.AccountsID && a.Status == null
                                           && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
                                           && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
                                           select new
                                           {
                                               a.Credit,
                                           }).ToList().Sum(x => x.Credit);


                        diexpacc.Add(new ProfitAndLoss
                        {
                            Parent = 1,
                            AccountId = acc.AccountsID,
                            Particulars = acc.Name,
                            ParentName = acc.Name,
                            Amount = (debits - credits) < 0 ? (debits - credits) * -1 : (debits - credits),
                            Orders = 1,
                            Temp = increDr,
                        });
                        increDr++;
                    }
                }

            }


            //6
            //                    //join b in db.AccountsTransactions on a.AccountsID equals b.Account into accs
            //                    //from b in accs.DefaultIfEmpty()
            //                where a.Group == 29
            //                let acdebit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Debit)
            //                let accredit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Credit)

            //                //(todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)
            //                    Parent = 1,
            //                    AccountId = a.AccountsID,
            //                    Particulars = a.Name,
            //                    ParentName = a.Name,
            //                    //Amount = b != null ? db.AccountsTransactions.Where(c => c.Account == a.AccountsID && (fromdate == "" || EF.Functions.DateDiffDay(b.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)).Select(c => c.Debit - c.Credit).Sum() : 0,
            //                    Orders = 1,
            //                    Temp = increDr


            //                where b.Group == 29 //&& (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)

            //                    b.AccountsID,
            //                    b.Name,

            //                    Orders = 1,
            //                    Temp = increDr
            //                }).Distinct().Select(o => new ProfitAndLoss
            //                    Parent = 1,
            //                    AccountId = o.AccountsID,
            //                    Particulars = o.Name,
            //                    ParentName = o.Name,

            //                    Orders = 1,
            //                    Temp = increDr


            //--for ordering account based on index--direct expense
            var diexpaccs = diexpacc.Where(a => a.Amount != 0 && a.Amount != null).AsEnumerable()
            .Select((o, index) => new ProfitAndLoss
            {
                Parent = o.Parent,
                AccountId = o.AccountId,
                Particulars = o.Particulars,
                ParentName = o.ParentName,
                Amount = o.Amount,
                Orders = o.Orders,
                Temp = (increDr + index)
            });
            List<ProfitAndLoss> diexpaccnew = diexpaccs.ToList(); //convert to list
            int CountExp2 = diexpaccnew.Count();


            ////5
            increCr = credit.Count() + 1;
            //                    Parent = 0,//a.Parent 
            //                    AccountId = null,
            //                    Particulars = a.Particulars,
            //                    ParentName = a.ParentName,
            //                    Amount = a.Credit - a.Debit,
            //                    Orders = 0,
            //                    Temp = increCr


            List<ProfitAndLoss> dirincomacc = new List<ProfitAndLoss>();
            foreach (var entry in dirincome)
            {
                ProfitAndLoss entrys = new ProfitAndLoss()
                {
                    Parent = 0,
                    AccountId = null,
                    Particulars = entry.Particulars,
                    ParentName = entry.ParentName,
                    Amount = (entry.Credit - entry.Debit) < 0 ? (entry.Credit - entry.Debit) * -1 : (entry.Credit - entry.Debit),
                    Orders = 0,
                    Temp = increCr
                };
                dirincomacc.Add(entrys);

                var accounts = db.Accountss.Where(a => a.Group == entry.AccountsGroupID).ToList();
                foreach (var acc in accounts)
                {
                    var chkacc = db.AccountsTransactions.Where(a => a.Account == acc.AccountsID).ToList();
                    if (chkacc.Count() > 0)
                    {

                        decimal debits = (from a in db.AccountsTransactions
                                          where a.Account == acc.AccountsID && a.Status == null
                                          && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
                                          && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
                                          select new
                                          {
                                              a.Debit,
                                          }).ToList().Sum(x => x.Debit);

                        decimal credits = (from a in db.AccountsTransactions
                                           where a.Account == acc.AccountsID && a.Status == null
                                           && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
                                           && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
                                           select new
                                           {
                                               a.Credit,
                                           }).ToList().Sum(x => x.Credit);

                        dirincomacc.Add(new ProfitAndLoss
                        {
                            Parent = 1,
                            AccountId = acc.AccountsID,
                            Particulars = acc.Name,
                            ParentName = acc.Name,
                            Amount = (credits - debits) < 0 ? (credits - debits) * -1 : (credits - debits),
                            Orders = 1,
                            Temp = increCr,
                        });
                        increCr++;
                    }
                }

            }


            //6
            //                       //join b in db.AccountsTransactions on a.AccountsID equals b.Account into accs
            //                       //from b in accs.DefaultIfEmpty()
            //                   where a.Group == 31
            //                   let acdebit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Debit)
            //                   let accredit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Credit)

            //                   //(todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)
            //                       Parent = 1,
            //                       AccountId = a.AccountsID,
            //                       Particulars = a.Name,
            //                       ParentName = a.Name,
            //                       Amount = accredit - acdebit,
            //                       //Amount = b != null ? db.AccountsTransactions.Where(c => c.Account == a.AccountsID && (fromdate == "" || EF.Functions.DateDiffDay(b.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)).Select(c => c.Debit - c.Credit).Sum() : 0,
            //                       Orders = 1,
            //                       Temp = increCr





            //--for ordering account based on index--direct income
            var dirincomaccs = dirincomacc.Where(a => a.Amount != 0 && a.Amount != null).AsEnumerable()
            .Select((o, index) => new ProfitAndLoss
            {
                Parent = o.Parent,
                AccountId = o.AccountId,
                Particulars = o.Particulars,
                ParentName = o.ParentName,
                Amount = o.Amount,
                Orders = o.Orders,
                Temp = (increCr + index)
            });

            List<ProfitAndLoss> dirincomaccnew = dirincomaccs.ToList(); //convert to list
            int CountDIncome2 = dirincomaccnew.Count();





            var expcount = CountExp2;
            var incomecount = CountDIncome2;

            //fiilling null rows direct exp & income
            List<ProfitAndLoss> NullListCr1 = new List<ProfitAndLoss>();
            List<ProfitAndLoss> NullListDr1 = new List<ProfitAndLoss>();
            if (expcount > incomecount)
            {
                increCr = credit.Count() + 1;
                var totcount = expcount - incomecount;
                for (int i = 0; i < totcount; i++)
                {
                    increCr++;
                    var tcount = increCr;
                    dirincomaccnew.Add(new ProfitAndLoss
                    {
                        Parent = 0,
                        AccountId = null,
                        Particulars = "",
                        ParentName = "",
                        Amount = null,
                        Orders = 1,
                        Temp = tcount
                    });
                }
            }
            if (expcount < incomecount)
            {
                increDr = debit.Count() + 1;
                var totcount = incomecount - expcount;
                for (int i = 0; i < totcount; i++)
                {
                    increDr++;
                    var tcount = increDr;
                    diexpaccnew.Add(new ProfitAndLoss
                    {
                        Parent = 0,
                        AccountId = null,
                        Particulars = "",
                        ParentName = "",
                        Amount = null,
                        Orders = 1,
                        Temp = tcount
                    });
                }
            }
            debit = debit.Union(diexpaccnew);
            credit = credit.Union(dirincomaccnew);



            //for gross profit
            List<ProfitAndLoss> pandf = new List<ProfitAndLoss>();
            List<ProfitAndLoss> nullrow1 = new List<ProfitAndLoss>();
            ProfitAndLoss gfnull1 = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "",
                ParentName = "",
                Amount = null,
                Orders = 0,
                Temp = 0
            };
            nullrow1.Add(gfnull1);


            List<ProfitAndLoss> nullrow2 = new List<ProfitAndLoss>();
            ProfitAndLoss gfnull2 = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "",
                ParentName = "",
                Amount = null,
                Orders = 0,
                Temp = 0
            };
            nullrow2.Add(gfnull2);

            if (Debit < Credit)
            {
                TotalDr = (decimal)Credit - (decimal)Debit;
                increDr = debit.Count() + 2;
                pandf.Clear();
                ProfitAndLoss gf = new ProfitAndLoss()
                {
                    Parent = 0,
                    AccountId = null,
                    Particulars = "Gross Profit",
                    ParentName = "Gross Profit",
                    Amount = TotalDr,
                    Orders = 0,
                    Temp = increDr
                };
                pandf.Add(gf);
            }


            if (Debit > Credit)
            {
                increCr = credit.Count() + 2;
                pandf.Clear();
                TotalCr = (decimal)Debit - (decimal)Credit;
                ProfitAndLoss gl = new ProfitAndLoss()
                {
                    Parent = 0,
                    AccountId = null,
                    Particulars = "Gross Loss",
                    ParentName = "Gross Loss",
                    Amount = TotalCr,
                    Orders = 0,
                    Temp = increCr
                };
                pandf.Add(gl);
            }



            //first total
            increDr = debit.Count() + 1;
            decimal FirstTotalDr = Debit + TotalDr;
            ProfitAndLoss TotalDr1 = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "Total",
                ParentName = "Total",
                Amount = FirstTotalDr,
                Orders = 0,
                Temp = increDr
            };
            List<ProfitAndLoss> TotalDrOne = new List<ProfitAndLoss>();
            TotalDrOne.Add(TotalDr1);

            //first total
            increCr = credit.Count() + 1;
            decimal FirstTotalCr = TotalCr + Credit;
            ProfitAndLoss TotalCr1 = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "Total",
                ParentName = "Total",
                Amount = FirstTotalCr,
                Orders = 0,
                Temp = increCr
            };
            List<ProfitAndLoss> TotalCrOne = new List<ProfitAndLoss>();
            TotalCrOne.Add(TotalCr1);


            if (Debit > Credit)
            {
                increDr = debit.Count() + 1;
                TotalCr = (decimal)Debit - (decimal)Credit;

                baddepthdr.Clear();
                ProfitAndLoss badde = new ProfitAndLoss()
                {
                    Parent = 0,
                    AccountId = null,
                    Particulars = "Gross Loss b/d",
                    ParentName = "Gross Loss b/d",
                    Amount = TotalCr,
                    Orders = 0,
                    Temp = increDr
                };
                baddepthdr.Add(badde);

            }

            if (Debit < Credit)
            {
                increCr = credit.Count() + 1;
                baddepthcr.Clear();
                ProfitAndLoss baddep = new ProfitAndLoss()
                {
                    Parent = 0,
                    AccountId = null,
                    Particulars = "Gross Profit b/d",
                    ParentName = "Gross Profit b/d",
                    Amount = TotalDr,
                    Orders = 0,
                    Temp = increCr
                };
                baddepthcr.Add(baddep);
            }




            //gross profit/loss union
            if (Debit < Credit)
            {
                debit = debit.Union(pandf);
                nullrow1[0].Temp = debit.Count() + 1;
                credit = credit.Union(nullrow1);
            }
            if (Debit > Credit)
            {
                credit = credit.Union(pandf);
                nullrow1[0].Temp = credit.Count() + 1;
                debit = debit.Union(nullrow1);
            }

            //first total union
            TotalDrOne[0].Temp = debit.Count() + 2;
            TotalCrOne[0].Temp = credit.Count() + 2;
            debit = debit.Union(TotalDrOne);
            credit = credit.Union(TotalCrOne);


            //gross profit/loss b/d union
            if (Debit > Credit)
            {
                baddepthdr[0].Temp = debit.Count() + 2;
                debit = debit.Union(baddepthdr);
                nullrow2[0].Temp = debit.Count() + 1;
                credit = credit.Union(nullrow2);
            }
            if (Debit < Credit)
            {
                baddepthcr[0].Temp = credit.Count() + 2;
                credit = credit.Union(baddepthcr);
                nullrow2[0].Temp = credit.Count() + 1;
                debit = debit.Union(nullrow2);
            }

            //indirect expense
            //                   Parent = 0,//a.Parent,
            //                   AccountId = null,
            //                   Particulars = a.Particulars,
            //                   ParentName = a.ParentName,
            //                   Amount = a.Debit - a.Credit,
            //                   Orders = a.orderB,
            //                   Temp = increDr

            ////indirect expense accounts
            //                    where a.Group == 30
            //                    let acdebit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Debit)
            //                    let accredit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Credit)

            //                    //(todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)
            //                        Parent = 1,
            //                        AccountId = a.AccountsID,
            //                        Particulars = a.Name,
            //                        ParentName = a.Name,
            //                        //Amount = b != null ? 
            //                        //Amount = b != null ? db.AccountsTransactions.Where(c => c.Account == a.AccountsID).Select(c => c.Debit - c.Credit).Sum() : 0,
            //                        Amount = acdebit - accredit,
            //                        Orders = 1,
            //                        Temp = increDr


            List<ProfitAndLoss> indiexpaccss = new List<ProfitAndLoss>();
            foreach (var entry in indexpenses)
            {
                ProfitAndLoss entrys = new ProfitAndLoss()
                {
                    Parent = 0,
                    AccountId = entry.AccountsGroupID,
                    Particulars = entry.Particulars,
                    ParentName = entry.ParentName,
                    Amount = (entry.Debit - entry.Credit) < 0 ? (entry.Debit - entry.Credit) * -1 : (entry.Debit - entry.Credit),
                    Orders = 0,
                    Temp = increDr
                };
                indiexpaccss.Add(entrys);

                var accounts = db.Accountss.Where(a => a.Group == entry.AccountsGroupID).ToList();
                foreach (var acc in accounts)
                {
                    var chkacc = db.AccountsTransactions.Where(a => a.Account == acc.AccountsID).ToList();
                    if (chkacc.Count() > 0)
                    {

                        decimal debits = (from a in db.AccountsTransactions
                                          where a.Account == acc.AccountsID && a.Status == null
                                          && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
                                          && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
                                          select new
                                          {
                                              a.Debit,
                                          }).ToList().Sum(x => x.Debit);

                        decimal credits = (from a in db.AccountsTransactions
                                           where a.Account == acc.AccountsID && a.Status == null
                                           && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
                                           && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
                                           select new
                                           {
                                               a.Credit,
                                           }).ToList().Sum(x => x.Credit);


                        indiexpaccss.Add(new ProfitAndLoss
                        {
                            //Parent = 1,
                            //AccountId = acc.AccountsID,
                            //Particulars = acc.Name,
                            //ParentName = acc.Name,
                            //Orders = 1,
                            //Temp = increDr,
                        });
                        increDr++;
                    }
                }

            }


            //--for ordering account based on index-indirect exp
            var indiexpacc = indiexpaccss.Where(a => a.Amount != 0 && a.Amount != null).AsEnumerable()
            .Select((o, index) => new ProfitAndLoss
            {
                Parent = o.Parent,
                AccountId = o.AccountId,
                Particulars = o.Particulars,
                ParentName = o.ParentName,
                Amount = o.Amount,
                Orders = o.Orders,
                Temp = (increDr + index)
            });
            List<ProfitAndLoss> indiexpaccnew = indiexpacc.ToList();
            Decimal xxx = indiexpaccnew.Where(o => o.Parent == 0).Sum(o => o.Amount) ?? 0;

            int CountIExp2 = indiexpaccnew.Count();


            //indirect income
            //                      Parent = 0, //a.Parent,
            //                      AccountId = null,
            //                      Particulars = a.Particulars,
            //                      ParentName = a.ParentName,
            //                      Amount = a.Credit - a.Debit,
            //                      Orders = 1,
            //                      Temp = increCr

            ////indirect income accounts
            //                           //join b in db.AccountsTransactions on a.AccountsID equals b.Account into accs
            //                           //from b in accs.DefaultIfEmpty()
            //                       where a.Group == 32
            //                       let acdebit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Debit)
            //                       let accredit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Credit)

            //                       //(todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)
            //                           Parent = 1,
            //                           AccountId = a.AccountsID,
            //                           Particulars = a.Name,
            //                           ParentName = a.Name,
            //                           Amount = accredit - acdebit,
            //                           //Amount = b != null ? db.AccountsTransactions.Where(c => c.Account == a.AccountsID && (fromdate == "" || EF.Functions.DateDiffDay(b.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)).Select(c => c.Debit - c.Credit).Sum() : 0,
            //                           Orders = 1,
            //                           Temp = increCr




            List<ProfitAndLoss> indirincomaccss = new List<ProfitAndLoss>();
            foreach (var entry in indirincome)
            {
                ProfitAndLoss entrys = new ProfitAndLoss()
                {
                    Parent = 0,
                    AccountId = entry.AccountsGroupID,
                    Particulars = entry.Particulars,
                    ParentName = entry.ParentName,
                    Amount = (entry.Credit - entry.Debit) < 0 ? (entry.Credit - entry.Debit) * -1 : (entry.Credit - entry.Debit),
                    Orders = 0,
                    Temp = increCr
                };
                indirincomaccss.Add(entrys);

                var accounts = db.Accountss.Where(a => a.Group == entry.AccountsGroupID).ToList();
                foreach (var acc in accounts)
                {
                    var chkacc = db.AccountsTransactions.Where(a => a.Account == acc.AccountsID).ToList();
                    if (chkacc.Count() > 0)
                    {

                        decimal debits = (from a in db.AccountsTransactions
                                          where a.Account == acc.AccountsID && a.Status == null
                                          && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
                                          && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
                                          select new
                                          {
                                              a.Debit,
                                          }).ToList().Sum(x => x.Debit);

                        decimal credits = (from a in db.AccountsTransactions
                                           where a.Account == acc.AccountsID && a.Status == null
                                           && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
                                           && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
                                           select new
                                           {
                                               a.Credit,
                                           }).ToList().Sum(x => x.Credit);


                        indirincomaccss.Add(new ProfitAndLoss
                        {
                            //Parent = 1,
                            //AccountId = acc.AccountsID,
                            //Particulars = acc.Name,
                            //ParentName = acc.Name,
                            //Orders = 1,
                            //Temp = increCr,
                        });
                        increCr++;
                    }
                }

            }


            //--for ordering account based on index-indirect income
            var indirincomacc = indirincomaccss.Where(a => a.Amount != 0 && a.Amount != null).AsEnumerable()
            .Select((o, index) => new ProfitAndLoss
            {
                Parent = o.Parent,
                AccountId = o.AccountId,
                Particulars = o.Particulars,
                ParentName = o.ParentName,
                Amount = o.Amount,
                Orders = o.Orders,
                Temp = (increCr + index)
            });
            List<ProfitAndLoss> indirincomaccnew = indirincomacc.ToList();
            Decimal zzz = indirincomaccnew.Where(o => o.Parent == 0).Sum(o => o.Amount) ?? 0;

            int CountIDIncome2 = indirincomaccnew.Count();


            var inexpcount = CountIExp2;
            var inincomecount = CountIDIncome2;

            //fiilling null rows indirect expense/income
            List<ProfitAndLoss> NullListCr = new List<ProfitAndLoss>();
            List<ProfitAndLoss> NullListDr = new List<ProfitAndLoss>();
            if (inexpcount > inincomecount)
            {
                increCr = credit.Count() + inincomecount + 1;
                var totcount = inexpcount - inincomecount;
                for (int i = 0; i < totcount; i++)
                {
                    increCr++;
                    var tcount = increCr;
                    indirincomaccnew.Add(new ProfitAndLoss
                    {
                        Parent = 0,
                        AccountId = null,
                        Particulars = "",
                        ParentName = "",
                        Amount = null,
                        Orders = 1,
                        Temp = tcount
                    });
                }
            }
            if (inexpcount < inincomecount)
            {
                increDr = debit.Count() + inexpcount + 1;
                var totcount = inincomecount - inexpcount;
                for (int i = 0; i < totcount; i++)
                {
                    increDr++;
                    var tcount = increDr;
                    indiexpaccnew.Add(new ProfitAndLoss
                    {
                        Parent = 0,
                        AccountId = null,
                        Particulars = "",
                        ParentName = "",
                        Amount = null,
                        Orders = 1,
                        Temp = tcount
                    });
                }
            }

            debit = debit.Union(indiexpaccnew);
            credit = credit.Union(indirincomaccnew);


            // --------------------------------------------------------

            GTotalDr = (decimal)TotalCr + xxx;// (decimal)InDirectExp;
            GTotalCr = (decimal)TotalDr + zzz;// + (decimal)InDirectIncome;
            if ((decimal)GTotalDr < (decimal)GTotalCr)
            {//profit
                increDr = debit.Count() + 2;
                Profit = (decimal)GTotalCr - (decimal)GTotalDr;
                net.Clear();
                ProfitAndLoss netp = new ProfitAndLoss()
                {
                    Parent = 0,
                    AccountId = null,
                    Particulars = "Net Profit",
                    ParentName = "Net Profit",
                    Amount = Profit,
                    Orders = 0,
                    Temp = increDr
                };
                net.Add(netp);
                TotalCr = TotalCr + Profit;
            }

            //Second total
            increDr = debit.Count() + 1;
            decimal SecondTotalDr = (decimal)TotalCr + xxx;// (decimal)InDirectExp;
            ProfitAndLoss TotalDr2 = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "Total",
                ParentName = "Total",
                Amount = SecondTotalDr,
                Orders = 0,
                Temp = increDr
            };
            List<ProfitAndLoss> TotalDrTwo = new List<ProfitAndLoss>();
            TotalDrTwo.Add(TotalDr2);



            if ((decimal)GTotalDr > (decimal)GTotalCr)
            {//loss
                increCr = credit.Count() + 2;
                Profit = (decimal)GTotalDr - (decimal)GTotalCr;
                net.Clear();
                ProfitAndLoss netl = new ProfitAndLoss()
                {
                    Parent = 0,
                    AccountId = null,
                    Particulars = "Net Loss",
                    ParentName = "Net Loss",
                    Amount = Profit,
                    Orders = 0,
                    Temp = increCr
                };
                net.Add(netl);

                TotalDr = TotalDr + Profit;
            }

            //second total
            increCr = credit.Count() + 1;
            decimal SecondTotalCr = (decimal)TotalDr + zzz;// (decimal)InDirectIncome;
            ProfitAndLoss TotalCr2 = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "Total",
                ParentName = "Total",
                Amount = SecondTotalCr,
                Orders = 0,
                Temp = increCr
            };
            List<ProfitAndLoss> TotalCrTwo = new List<ProfitAndLoss>();
            TotalCrTwo.Add(TotalCr2);


            List<ProfitAndLoss> nullrow3 = new List<ProfitAndLoss>();
            ProfitAndLoss gfnull3 = new ProfitAndLoss()
            {
                Parent = 0,
                AccountId = null,
                Particulars = "",
                ParentName = "",
                Amount = null,
                Orders = 0,
                Temp = 0
            };
            nullrow3.Add(gfnull3);

            //net profit/loss union
            if ((decimal)GTotalDr < (decimal)GTotalCr)
            {
                debit = debit.Union(net);
                nullrow3[0].Temp = debit.Count() + 1;
                credit = credit.Union(nullrow3);
            }
            if ((decimal)GTotalDr > (decimal)GTotalCr)
            {
                credit = credit.Union(net);
                nullrow3[0].Temp = credit.Count() + 1;
                debit = debit.Union(nullrow3);
            }

            //final total union
            TotalDrTwo[0].Temp = debit.Count() + 2;
            TotalCrTwo[0].Temp = credit.Count() + 2;
            debit = debit.Union(TotalDrTwo);
            credit = credit.Union(TotalCrTwo);


            var count1 = getOrderingPF(debit);
            var count2 = getOrderingPF(credit);


            //outer joins-for single row
            var leftOuterJoin = (from a in debit
                                 join b in credit on a.Temp equals b.Temp into cr
                                 from b in cr.DefaultIfEmpty()

                                 select new
                                 {
                                     ParticularA = b?.Particulars,
                                     DebitA = b?.Amount,


                                     ParticularL = a?.Particulars,
                                     DebitL = a?.Amount,


                                     ParentA = b?.Parent,
                                     ParentL = a?.Parent,

                                     AccountIdDr = a?.AccountId,
                                     AccountIdCr = b?.AccountId,
                                     Orders = (a.Orders == 0 || b.Orders == 0) ? 0 : 1
                                 });

            var rightOuterJoin = (from a in credit
                                  join b in debit on a.Temp equals b.Temp into dr
                                  from b in dr.DefaultIfEmpty()
                                  select new
                                  {
                                      ParticularA = a?.Particulars,
                                      DebitA = a?.Amount,

                                      ParticularL = b?.Particulars,
                                      DebitL = b?.Amount,

                                      ParentA = a?.Parent,
                                      ParentL = b?.Parent,

                                      AccountIdDr = b?.AccountId,
                                      AccountIdCr = a?.AccountId,
                                      Orders = (a.Orders == 0 || b.Orders == 0) ? 0 : 1
                                  });

            var full = leftOuterJoin.Union(rightOuterJoin);

            #endregion





            //    #region PLCALCU1


            //    //sales price.
            //                   (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0) && i.Account == 1
            //                   && i.Purpose == "Sale" && i.Status == null
            //                   //group i by i.SalesEntryId into g
            //                       Total = i.Credit



            //    //sales return price.
            //                      (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0)
            //                      && i.Account == 1 && i.Purpose == "Sale Return"
            //                      && i.Status == null
            //                          Total = i.Debit


            //    //purchase price
            //                   (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0)
            //                    && i.Account == 2 && i.Purpose == "Purchase"
            //                    && i.Status == null
            //                       Total = i.Debit

            //    //purchase return price
            //                      (todate == "" || EF.Functions.DateDiffDay(i.Date, tdate) >= 0)
            //                      && i.Account == 2 && i.Purpose == "Purchase Return"
            //                      && i.Status == null
            //                          Total = i.Credit


            //    ////direct expenses

            //    ////in direct expenses



            //    //direct income


            //    //in direct income


            //    //direct expenses


            //    //in direct expenses








            //    ////for gross profit b/d



            //    //--------------------------------------DEBIT SIDE---------------------------------------------

            //    //1

            //    ProfitAndLoss opens1 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "Opening Stock",
            //        ParentName = "Opening Stock",
            //        Amount = openstock,
            //        Orders = 0,
            //        Temp = increDr1

            //    //2
            //    ProfitAndLoss purchase1 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "Purchase",
            //        ParentName = "Purchase",
            //        Amount = Purchase,
            //        Orders = 0,
            //        Temp = increDr1

            //    //3
            //    ProfitAndLoss purchaseentrys1 = new ProfitAndLoss()
            //        Parent = 1,
            //        AccountId = 2,
            //        Particulars = "Purchase",
            //        ParentName = "Purchase",
            //        Amount = pprice1,
            //        Orders = 0,
            //        Temp = increDr1

            //    //4
            //    ProfitAndLoss purchasereturns1 = new ProfitAndLoss()
            //        Parent = 1,
            //        AccountId = 2,
            //        Particulars = "Purchase Return",
            //        ParentName = "Purchase Return",
            //        Amount = pretprice1,
            //        Orders = 0,
            //        Temp = increDr1



            //    ProfitAndLoss closes1 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "Closing Stock",
            //        Amount = closestock,
            //        Orders = 0,
            //        Temp = increCr1

            //    //2
            //    ProfitAndLoss sale1 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "Sale",
            //        Amount = Sales,
            //        Orders = 0,
            //        Temp = increCr1

            //    //3
            //    ProfitAndLoss saleentrys1 = new ProfitAndLoss()
            //        Parent = 1,
            //        AccountId = 1,
            //        Particulars = "Sale",
            //        Amount = sprice1,
            //        Orders = 0,
            //        Temp = increCr1

            //    //4
            //    ProfitAndLoss salereturns1 = new ProfitAndLoss()
            //        Parent = 1,
            //        AccountId = 1,
            //        Particulars = "Sales Return",
            //        Amount = sretprice1,
            //        Orders = 0,
            //        Temp = increCr1




            //    ProfitAndLoss gfnulls1 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "",
            //        ParentName = "",
            //        Amount = null,
            //        Orders = 1,
            //        Temp = 0


            //    //debit-union




            //    //credit -union



            //    //5
            //    //var dirxp = (from a in dexpenses
            //    //             select new ProfitAndLoss
            //    //                 Parent = a.Parent,
            //    //                 AccountId = null,
            //    //                 Particulars = a.Particulars,
            //    //                 ParentName = a.ParentName,
            //    //                 Amount = a.Debit - a.Credit,
            //    //                 Orders = 1,
            //    //                 Temp = increDr

            //        ProfitAndLoss entrys = new ProfitAndLoss()
            //            Parent = 0,
            //            AccountId = null,
            //            Particulars = entry.Particulars,
            //            ParentName = entry.ParentName,
            //            Orders = 0,
            //            Temp = increDr1


            //                                  where a.Account == acc.AccountsID && a.Status == null
            //                                  && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
            //                                  && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
            //                                      a.Debit,

            //                                   where a.Account == acc.AccountsID && a.Status == null
            //                                   && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
            //                                   && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
            //                                       a.Credit,


            //                diexpacc1.Add(new ProfitAndLoss
            //                    Parent = 1,
            //                    AccountId = acc.AccountsID,
            //                    Particulars = acc.Name,
            //                    ParentName = acc.Name,
            //                    Orders = 1,
            //                    Temp = increDr1,


            //    .Select((o, index) => new ProfitAndLoss
            //        Parent = o.Parent,
            //        AccountId = o.AccountId,
            //        Particulars = o.Particulars,
            //        ParentName = o.ParentName,
            //        Amount = o.Amount,
            //        Orders = o.Orders,
            //        Temp = (increDr1 + index)
            //    List<ProfitAndLoss> diexpaccnew1 = diexpaccs1.ToList(); //convert to list


            //    ////5
            //    //var dirincom = (from a in dirincome
            //    //                select new ProfitAndLoss
            //    //                    Parent = 0,//a.Parent 
            //    //                    AccountId = null,
            //    //                    Particulars = a.Particulars,
            //    //                    ParentName = a.ParentName,
            //    //                    Amount = a.Credit - a.Debit,
            //    //                    Orders = 0,
            //    //                    Temp = increCr


            //        ProfitAndLoss entrys = new ProfitAndLoss()
            //            Parent = 0,
            //            AccountId = null,
            //            Particulars = entry.Particulars,
            //            ParentName = entry.ParentName,
            //            Orders = 0,
            //            Temp = increCr1


            //                                  where a.Account == acc.AccountsID && a.Status == null
            //                                  && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
            //                                  && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
            //                                      a.Debit,

            //                                   where a.Account == acc.AccountsID && a.Status == null
            //                                   && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
            //                                   && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
            //                                       a.Credit,

            //                dirincomacc1.Add(new ProfitAndLoss
            //                    Parent = 1,
            //                    AccountId = acc.AccountsID,
            //                    Particulars = acc.Name,
            //                    ParentName = acc.Name,
            //                    Orders = 1,
            //                    Temp = increCr1,



            //    .Select((o, index) => new ProfitAndLoss
            //        Parent = o.Parent,
            //        AccountId = o.AccountId,
            //        Particulars = o.Particulars,
            //        ParentName = o.ParentName,
            //        Amount = o.Amount,
            //        Orders = o.Orders,
            //        Temp = (increCr1 + index)

            //    List<ProfitAndLoss> dirincomaccnew1 = dirincomaccs1.ToList(); //convert to list






            //    //fiilling null rows direct exp & income
            //            dirincomaccnew1.Add(new ProfitAndLoss
            //                Parent = 0,
            //                AccountId = null,
            //                Particulars = "",
            //                ParentName = "",
            //                Amount = null,
            //                Orders = 1,
            //                Temp = tcount
            //            diexpaccnew1.Add(new ProfitAndLoss
            //                Parent = 0,
            //                AccountId = null,
            //                Particulars = "",
            //                ParentName = "",
            //                Amount = null,
            //                Orders = 1,
            //                Temp = tcount1



            //    //for gross profit
            //    ProfitAndLoss gfnull11 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "",
            //        ParentName = "",
            //        Amount = null,
            //        Orders = 0,
            //        Temp = 0


            //    ProfitAndLoss gfnull21 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "",
            //        ParentName = "",
            //        Amount = null,
            //        Orders = 0,
            //        Temp = 0

            //        ProfitAndLoss gf1 = new ProfitAndLoss()
            //            Parent = 0,
            //            AccountId = null,
            //            Particulars = "Gross Profit",
            //            ParentName = "Gross Profit",
            //            Amount = TotalDr11,
            //            Orders = 0,
            //            Temp = increDr1


            //        ProfitAndLoss gl1 = new ProfitAndLoss()
            //            Parent = 0,
            //            AccountId = null,
            //            Particulars = "Gross Loss",
            //            ParentName = "Gross Loss",
            //            Amount = TotalDr11,
            //            Orders = 0,
            //            Temp = increCr1



            //    //first total
            //    ProfitAndLoss TotalDr111 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "Total",
            //        ParentName = "Total",
            //        Amount = FirstTotalDr1,
            //        Orders = 0,
            //        Temp = increDr1

            //    //first total
            //    ProfitAndLoss TotalCr111 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "Total",
            //        ParentName = "Total",
            //        Amount = FirstTotalCr1,
            //        Orders = 0,
            //        Temp = increCr1



            //        ProfitAndLoss badde1 = new ProfitAndLoss()
            //            Parent = 0,
            //            AccountId = null,
            //            Particulars = "Gross Loss b/d",
            //            ParentName = "Gross Loss b/d",
            //            Amount = TotalDr11,
            //            Orders = 0,
            //            Temp = increDr1


            //        ProfitAndLoss baddep1 = new ProfitAndLoss()
            //            Parent = 0,
            //            AccountId = null,
            //            Particulars = "Gross Profit b/d",
            //            ParentName = "Gross Profit b/d",
            //            Amount = TotalDr11,
            //            Orders = 0,
            //            Temp = increCr1




            //    //gross profit/loss union

            //    //first total union


            //    //gross profit/loss b/d union

            //    //indirect expense
            //    //var indirxp = (from a in indexpenses
            //    //               select new ProfitAndLoss
            //    //                   Parent = 0,//a.Parent,
            //    //                   AccountId = null,
            //    //                   Particulars = a.Particulars,
            //    //                   ParentName = a.ParentName,
            //    //                   Amount = a.Debit - a.Credit,
            //    //                   Orders = a.orderB,
            //    //                   Temp = increDr

            //    ////indirect expense accounts
            //    //var indiexpaccss = (from a in db.Accountss
            //    //                    where a.Group == 30
            //    //                    let acdebit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Debit)
            //    //                    let accredit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Credit)

            //    //                    //(todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)
            //    //                    select new ProfitAndLoss
            //    //                        Parent = 1,
            //    //                        AccountId = a.AccountsID,
            //    //                        Particulars = a.Name,
            //    //                        ParentName = a.Name,
            //    //                        //Amount = b != null ? 
            //    //                        //Amount = b != null ? db.AccountsTransactions.Where(c => c.Account == a.AccountsID).Select(c => c.Debit - c.Credit).Sum() : 0,
            //    //                        Amount = acdebit - accredit,
            //    //                        Orders = 1,
            //    //                        Temp = increDr


            //        ProfitAndLoss entrys = new ProfitAndLoss()
            //            Parent = 0,
            //            AccountId = entry.AccountsGroupID,
            //            Particulars = entry.Particulars,
            //            ParentName = entry.ParentName,
            //            Orders = 0,
            //            Temp = increDr1


            //                                  where a.Account == acc.AccountsID && a.Status == null
            //                                  && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
            //                                  && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
            //                                      a.Debit,

            //                                   where a.Account == acc.AccountsID && a.Status == null
            //                                   && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
            //                                   && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
            //                                       a.Credit,


            //                indiexpaccss1.Add(new ProfitAndLoss
            //                    //Parent = 1,
            //                    //AccountId = acc.AccountsID,
            //                    //Particulars = acc.Name,
            //                    //ParentName = acc.Name,
            //                    //Orders = 1,
            //                    //Temp = increDr,



            //    //--for ordering account based on index-indirect exp
            //    .Select((o, index) => new ProfitAndLoss
            //        Parent = o.Parent,
            //        AccountId = o.AccountId,
            //        Particulars = o.Particulars,
            //        ParentName = o.ParentName,
            //        Amount = o.Amount,
            //        Orders = o.Orders,
            //        Temp = (increDr1 + index)



            //    //indirect income
            //    //var indirincom = (from a in indirincome
            //    //                  select new ProfitAndLoss
            //    //                      Parent = 0, //a.Parent,
            //    //                      AccountId = null,
            //    //                      Particulars = a.Particulars,
            //    //                      ParentName = a.ParentName,
            //    //                      Amount = a.Credit - a.Debit,
            //    //                      Orders = 1,
            //    //                      Temp = increCr

            //    ////indirect income accounts
            //    //var indirincomaccss = (from a in db.Accountss
            //    //                           //join b in db.AccountsTransactions on a.AccountsID equals b.Account into accs
            //    //                           //from b in accs.DefaultIfEmpty()
            //    //                       where a.Group == 32
            //    //                       let acdebit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Debit)
            //    //                       let accredit = db.AccountsTransactions.Where(c => c.Account == a.AccountsID && c.Status == null && (fromdate == "" || EF.Functions.DateDiffDay(c.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(c.Date, tdate) >= 0)).Sum(c => c.Credit)

            //    //                       //(todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)
            //    //                       select new ProfitAndLoss
            //    //                           Parent = 1,
            //    //                           AccountId = a.AccountsID,
            //    //                           Particulars = a.Name,
            //    //                           ParentName = a.Name,
            //    //                           Amount = accredit - acdebit,
            //    //                           //Amount = b != null ? db.AccountsTransactions.Where(c => c.Account == a.AccountsID && (fromdate == "" || EF.Functions.DateDiffDay(b.Date, fdate) <= 0) && (todate == "" || EF.Functions.DateDiffDay(b.Date, tdate) >= 0)).Select(c => c.Debit - c.Credit).Sum() : 0,
            //    //                           Orders = 1,
            //    //                           Temp = increCr




            //        ProfitAndLoss entrys = new ProfitAndLoss()
            //            Parent = 0,
            //            AccountId = entry.AccountsGroupID,
            //            Particulars = entry.Particulars,
            //            ParentName = entry.ParentName,
            //            Orders = 0,
            //            Temp = increCr1


            //                                  where a.Account == acc.AccountsID && a.Status == null
            //                                  && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
            //                                  && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
            //                                      a.Debit,

            //                                   where a.Account == acc.AccountsID && a.Status == null
            //                                   && (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0)
            //                                   && (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0)
            //                                       a.Credit,


            //                indirincomaccss1.Add(new ProfitAndLoss
            //                    //Parent = 1,
            //                    //AccountId = acc.AccountsID,
            //                    //Particulars = acc.Name,
            //                    //ParentName = acc.Name,
            //                    //Orders = 1,
            //                    //Temp = increCr,



            //    //--for ordering account based on index-indirect income
            //    .Select((o, index) => new ProfitAndLoss
            //        Parent = o.Parent,
            //        AccountId = o.AccountId,
            //        Particulars = o.Particulars,
            //        ParentName = o.ParentName,
            //        Amount = o.Amount,
            //        Orders = o.Orders,
            //        Temp = (increCr1 + index)




            //    //fiilling null rows indirect expense/income
            //            indirincomaccnew1.Add(new ProfitAndLoss
            //                Parent = 0,
            //                AccountId = null,
            //                Particulars = "",
            //                ParentName = "",
            //                Amount = null,
            //                Orders = 1,
            //                Temp = tcount
            //            indiexpaccnew1.Add(new ProfitAndLoss
            //                Parent = 0,
            //                AccountId = null,
            //                Particulars = "",
            //                ParentName = "",
            //                Amount = null,
            //                Orders = 1,
            //                Temp = tcount



            //    // --------------------------------------------------------

            //    {//profit
            //        ProfitAndLoss netp1 = new ProfitAndLoss()
            //            Parent = 0,
            //            AccountId = null,
            //            Particulars = "Net Profit",
            //            ParentName = "Net Profit",
            //            Amount = Profit1,
            //            Orders = 0,
            //            Temp = increDr1

            //    //Second total
            //    ProfitAndLoss TotalDr21 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "Total",
            //        ParentName = "Total",
            //        Amount = SecondTotalDr,
            //        Orders = 0,
            //        Temp = increDr1



            //    {//loss
            //        ProfitAndLoss netl = new ProfitAndLoss()
            //            Parent = 0,
            //            AccountId = null,
            //            Particulars = "Net Loss",
            //            ParentName = "Net Loss",
            //            Amount = Profit1,
            //            Orders = 0,
            //            Temp = increCr1


            //    //second total
            //    ProfitAndLoss TotalCr21 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "Total",
            //        ParentName = "Total",
            //        Amount = SecondTotalCr1,
            //        Orders = 0,
            //        Temp = increCr1



            //    ProfitAndLoss gfnull31 = new ProfitAndLoss()
            //        Parent = 0,
            //        AccountId = null,
            //        Particulars = "",
            //        ParentName = "",
            //        Amount = null,
            //        Orders = 0,
            //        Temp = 0

            //    //net profit/loss union

            //    //final total union




            //    //outer joins-for single row

            //                             ParticularA = b?.Particulars,
            //                             DebitA = b?.Amount,


            //                             ParticularL = a?.Particulars,
            //                             DebitL = a?.Amount,


            //                             ParentA = b?.Parent,
            //                             ParentL = a?.Parent,

            //                             AccountIdDr = a?.AccountId,
            //                             AccountIdCr = b?.AccountId,
            //                             Orders = (a.Orders == 0 || b.Orders == 0) ? 0 : 1

            //                              ParticularA = a?.Particulars,
            //                              DebitA = a?.Amount,

            //                              ParticularL = b?.Particulars,
            //                              DebitL = b?.Amount,

            //                              ParentA = a?.Parent,
            //                              ParentL = b?.Parent,

            //                              AccountIdDr = b?.AccountId,
            //                              AccountIdCr = a?.AccountId,
            //                              Orders = (a.Orders == 0 || b.Orders == 0) ? 0 : 1


            //    #endregion


            //change to view model
            vmodel.ProfitAndLossDisplay = (from a in full
                                           select new ProfitAndLossDisplay
                                           {
                                               ParticularA = a?.ParticularA,
                                               DebitA = a?.DebitA,

                                               ParticularL = a?.ParticularL,
                                               DebitL = a?.DebitL,

                                               ParentA = a?.ParentA,
                                               ParentL = a?.ParentL,

                                               AccountIdDr = a?.AccountIdDr,
                                               AccountIdCr = a?.AccountIdCr,

                                               Orders = a.Orders
                                           }).ToList();
            return View(vmodel);
        }

        #region Trial Balance
        [QkAuthorize(Roles = "Dev,TrialBalance")]
        public ActionResult TrialBalance()
        {
            return View();
        }

        [QkAuthorize(Roles = "Dev,TrialBalance")]
        public ActionResult GetGroupTrialBalancebalancesheetcashflow(long? AccGroup, string type2, string frmdate, string todate, string parent, bool pdc = false)
        {
            BalanceSheetViewModel vmodel = new BalanceSheetViewModel();
            ViewBag.Parent = parent;
            String format = "dd-MM-yyyy";
            ViewBag.TDte = todate;
            int AccGrp = Convert.ToInt32(AccGroup);
            var todt = DateTime.ParseExact(todate, format, new CultureInfo("en-GB"));
            var fdt = new DateTime();
            vmodel.to = todt;
            ViewBag.Typ = type2;

            if (frmdate != null && frmdate != "")
            {
                ViewBag.FDte = frmdate;
                fdt = DateTime.ParseExact(frmdate, format, new CultureInfo("en-GB"));
                vmodel.from = DateTime.ParseExact(frmdate, format, new CultureInfo("en-GB")); ;
            }
            else
            {
                fdt = DateTime.ParseExact("01-01-2000", format, new CultureInfo("en-GB"));
                ViewBag.FDte = todt.AddYears(-25);
            }

            if (AccGroup == 10)
            {
                AccGroup = 14;
            }
            else if (AccGroup == 11)
            {
                AccGroup = 12;
            }
            if (type2 == "Purchase")
            {
                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID
                                              join c in db.PurchaseEntrys on a.reference equals c.PurchaseEntryId
                                              join d in db.Suppliers on c.Supplier equals d.SupplierID
                                              let bb = db.Payments.Where(at => a.Purpose == "Purchase Payment" && at.editable == choice.No && at.Reference == c.PurchaseEntryId && at.RefType == "Purchase").Join(db.Accountss, f1 => f1.PayFrom, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                                              let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                                              where (frmdate == "" || EF.Functions.DateDiffDay(c.PEDate, fdt) <= 0) &&
                                              (todate == "" || EF.Functions.DateDiffDay(c.PEDate, todt) >= 0) &&
                                              (a.Account == AccGroup) && (a.Purpose == "Purchase" || a.Purpose == "Purchase Payment")
                                              && (a.Status == null)
                                              select new TrialBalanceDisplay
                                              {
                                                  to = (DateTime?)c.PEDate,
                                                  Particular = a.Purpose == "Purchase Payment" ? "" : c.BillNo,
                                                  AccType = (d.Accounts == a.Account) ? ((a.Purpose != "Purchase Payment") ? "Purchase" : bb.Name) : d.SupplierName,
                                                  Debit = (decimal?)a.Debit,
                                                  Credit = (decimal?)a.Credit,
                                                  AccountsGroupID = a.reference,



                                              }).ToList();

            }
            else if (type2 == "Purchase Return")
            {
                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID
                                              join c in db.PurchaseReturns on a.reference equals c.PurchaseReturnId
                                              join d in db.Suppliers on c.Supplier equals d.SupplierID
                                              let bb = db.Receipts.Where(at => a.Purpose == "Purchase Return Payment" && at.editable == choice.No && at.Reference == c.PurchaseReturnId && at.Remark == "Receipt From Purchase Return").Join(db.Accountss, f1 => f1.PayTo, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                                              let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                                              where (frmdate == "" || EF.Functions.DateDiffDay(c.PRDate, fdt) <= 0) &&
                                              (todate == "" || EF.Functions.DateDiffDay(c.PRDate, todt) >= 0) &&
                                              (a.Account == AccGroup) && (a.Purpose == "Purchase Return" || a.Purpose == "Purchase Return Payment")
                                              && (a.Status == null)
                                              select new TrialBalanceDisplay
                                              {
                                                  to = (DateTime?)c.PRDate,
                                                  Particular = a.Purpose == "Purchase Return Payment" ? "" : c.BillNo,
                                                  AccType = (d.Accounts == a.Account) ? "Purchase" : d.SupplierName,
                                                  Debit = (decimal?)a.Debit,
                                                  Credit = (decimal?)a.Credit,
                                                  AccountsGroupID = a.reference,



                                              }).ToList();


            }
            else if (type2 == "Sales Return")
            {
                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID
                                              join c in db.SalesReturns on a.reference equals c.SalesReturnId
                                              join d in db.Customers on c.Customer equals d.CustomerID
                                              let bb = db.Payments.Where(at => a.Purpose == "Sale Return Payment" && at.editable == choice.No && at.Reference == c.SalesReturnId && at.RefType == "SalesReturn").Join(db.Accountss, f1 => f1.PayFrom, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                                              let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                                              where (frmdate == null || EF.Functions.DateDiffDay(c.SRDate, fdt) <= 0) &&
                                              (todate == null || EF.Functions.DateDiffDay(c.SRDate, todt) >= 0) &&
                                              (a.Account == AccGroup) && (a.Purpose == "Sale Return" || a.Purpose == "Sale Return Payment")
                                              && (a.Status == null)
                                              select new TrialBalanceDisplay
                                              {
                                                  to = (DateTime?)c.SRDate,
                                                  Particular = a.Purpose != "Sale Return Payment" ? c.BillNo : "",
                                                  AccType = (d.Accounts == a.Account) ? ((a.Purpose != "Sale Return Payment") ? "Sales Return" : bb.Name) : d.CustomerName,
                                                  Debit = (decimal?)a.Debit,
                                                  Credit = (decimal?)a.Credit,
                                                  AccountsGroupID = a.reference,
                                              }).ToList();
            }
            else if (type2 == "Sale")
            {
                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID
                                              join c in db.SalesEntrys on a.reference equals c.SalesEntryId
                                              join d in db.Customers on c.Customer equals d.CustomerID
                                              let bb = db.Receipts.Where(at => a.Purpose == "Sale Payment" && at.editable == choice.No && at.Reference == c.SalesEntryId && at.Remark == "Direct Reciept From Sale Entry").Join(db.Accountss, f1 => f1.PayTo, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                                              let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()

                                              where (frmdate == null || EF.Functions.DateDiffDay(c.SEDate, fdt) <= 0) &&
                                              (todate == null || EF.Functions.DateDiffDay(c.SEDate, todt) >= 0) &&
                                              (a.Account == AccGroup) && (a.Purpose == "Sale" || a.Purpose == "Sale Payment")
                                              && (a.Status == null)
                                              select new TrialBalanceDisplay
                                              {
                                                  to = (DateTime?)c.SEDate,
                                                  Particular = (a.Purpose != "Sale Payment" || (d.Accounts != a.Account)) ? c.BillNo : "",
                                                  AccType = (d.Accounts == a.Account) ? ((a.Purpose != "Sale Payment") ? "Sale" : bb.Name) : d.CustomerName,
                                                  Debit = (decimal?)a.Debit,
                                                  Credit = (decimal?)a.Credit,
                                                  AccountsGroupID = a.reference,
                                              }).ToList();
            }
            else
            {
                var goupacc = Common.GetChildAccGroup((long)AccGroup, "", "", fdt, todt, 2, 1);
                var subparent = goupacc.Where(o => o.Parent == AccGroup).ToList();
                List<BalanceSheet> summry = new List<BalanceSheet>();
                foreach (var v in subparent)
                {
                    var f = com.GetChildAccGroupsummary(v.AccountsGroupID, "", "", fdt, todt, 2, 1).ToList();
                    foreach (var ff in f)
                    {
                        summry.Add(ff);
                    }

                }
                vmodel.BalanceSheet = summry.ToList();
                var userpermission = User.IsInRole("All Journal Entry");
                var uid = User.Identity.GetUserId();
                var cashinhand = Common.GetChildAccGroup((long)9, "", "", fdt, todt, 2, 1);
                var subcashinhand = cashinhand.Where(o => o.Parent == 9).ToList();

                var bank = Common.GetChildAccGroup((long)8, "", "", fdt,todt, 2, 1);
                var subbank = bank.Where(o => o.Parent == 8).ToList();
                var allgroup = cashinhand.ToList();
                var newgrup = allgroup.Union(subcashinhand);
                newgrup = newgrup.Union(bank);
                newgrup = newgrup.Union(subbank);

             
                var acgroups = newgrup.Select(o => o.AccountsGroupID).Distinct().ToList().ToArray();
                var refs = (from a in db.AccountsTransactions
                                 join b in db.Accountss on a.Account equals b.AccountsID

                                 where (acgroups.Contains(b.Group)) &&

                                 (a.Status == null) &&
           b.Status == Status.active &&

                                                  (frmdate == null || EF.Functions.DateDiffDay(a.Date, fdt) <= 0) &&
                                                  (todate == null || EF.Functions.DateDiffDay(a.Date, todt) >= 0) &&

                                                  (a.Debit != 0 || a.Credit != 0)

                                 select new
                                 {
                                     a.reference
                                 }).Distinct().Select(o => o.reference);   // perf (audit batch 10): IQueryable subquery (IN-subquery) not a materialized ~30k-const IN-list (SQL 8623). Output identical; refs still scopes to vouchers touching cash/bank groups.
                var acaccounts = db.Accountss.Where(o => acgroups.Contains(o.Group)).Select(o => o.AccountsID).Distinct().ToList().ToArray();
               var TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID into deve
                                              from b in deve.DefaultIfEmpty()

                                              where (b.Group == AccGroup) &&
                                     refs.Contains(a.reference) &&
                                              (pdc == true || a.Status == null) &&
                                              (AccGroup != 68 || a.Status == null) &&
                                              (a.Account != 499) &&
                                              (a.Account != 3) &&
                                              b.Status == Status.active &&

                                              (frmdate == null || EF.Functions.DateDiffDay(a.Date, fdt) <= 0) &&
                                              (todate == null || EF.Functions.DateDiffDay(a.Date, todt) >= 0) &&

                                              (a.Debit != 0 || a.Credit != 0)
                                              group new { b.Name, b.AccountsID, a.Account, a.Debit, a.Credit } by new { b.AccountsID } into g

                                              select new TrialBalanceDisplay
                                              {
                                                  AccountsGroupID = g.Key.AccountsID,
                                                  Particular = g.FirstOrDefault().Name,
                                                  Parent = g.FirstOrDefault().Account,
                                                  Debit = g.Sum(k => k.Debit),
                                                  Credit = g.Sum(k => k.Credit),
                                                  AccType = type2,
                                              }).ToList().Select(o => new
                                              {

                                                  o.AccountsGroupID,
                                                  o.Particular,
                                                  o.Parent,
                                                  o.Debit,
                                                  o.Credit,
                                                  o.AccType,
                                                  balance=o.Debit-o.Credit

                                              }).ToList();
                var c = new TrialBalanceDisplay
                {
                    AccountsGroupID = 0,
                    Particular = "<span style='color:red'>CASH INFLOW</span>",


                };
                List<TrialBalanceDisplay> all = new List<TrialBalanceDisplay>();
                all.Add(c);
                vmodel.TrialBalanceDisplay = all;
                var inflow = TrialBalanceDisplay.Where(o => o.balance <= 0).Select(o =>
                new TrialBalanceDisplay
                {
                       AccountsGroupID= o.AccountsGroupID,
                    Particular=  o.Particular,
                    Parent=   o.Parent,
                    Debit=    o.Debit,
                    Credit=      o.Credit,
                    AccType=    o.AccType
                }).ToList();
                all.AddRange(inflow);
                c = new TrialBalanceDisplay
                {
                    AccountsGroupID = 0,
                    Particular = "<span style='color:red'>CASH OUTFLOW</span>",


                };
                
                all.Add(c);
                var outflow = TrialBalanceDisplay.Where(o => o.balance >0).Select(o =>
           new TrialBalanceDisplay
           {
               AccountsGroupID = o.AccountsGroupID,
               Particular = o.Particular,
               Parent = o.Parent,
               Debit = o.Debit,
               Credit = o.Credit,
               AccType = o.AccType
           }).ToList();
                all.AddRange(outflow);
                vmodel.TrialBalanceDisplay = all;
            }
            return View(vmodel);
        }

        [QkAuthorize(Roles = "Dev,TrialBalance")]
        public ActionResult GetGroupTrialBalancebalancesheet(long? AccGroup, string type2, string frmdate, string todate, string parent, bool pdc = false)
        {
            BalanceSheetViewModel vmodel = new BalanceSheetViewModel();
            ViewBag.Parent = parent;
            String format = "dd-MM-yyyy";
            ViewBag.TDte = todate;
            int AccGrp = Convert.ToInt32(AccGroup);
            var todt = DateTime.ParseExact(todate, format, new CultureInfo("en-GB"));
            var fdt = new DateTime();
            vmodel.to = todt;
            ViewBag.Typ = type2;

            if (frmdate != null && frmdate != "")
            {
                ViewBag.FDte = frmdate;
                fdt = DateTime.ParseExact("01-01-2000" ,format, new CultureInfo("en-GB"));
                vmodel.from = DateTime.ParseExact(frmdate, format, new CultureInfo("en-GB")); ;
            }
            else
            {
                fdt = DateTime.ParseExact("01-01-2000", format, new CultureInfo("en-GB"));
                ViewBag.FDte = todt.AddYears(-25);
            }

            if (AccGroup == 10)
            {
                AccGroup = 14;
            }
            else if (AccGroup == 11)
            {
                AccGroup = 12;
            }
            if (type2 == "Purchase")
            {
                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID
                                              join c in db.PurchaseEntrys on a.reference equals c.PurchaseEntryId
                                              join d in db.Suppliers on c.Supplier equals d.SupplierID
                                              let bb = db.Payments.Where(at => a.Purpose == "Purchase Payment" && at.editable == choice.No && at.Reference == c.PurchaseEntryId && at.RefType == "Purchase").Join(db.Accountss, f1 => f1.PayFrom, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                                              let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                                              where (frmdate == "" || EF.Functions.DateDiffDay(c.PEDate, fdt) <= 0) &&
                                              (todate == "" || EF.Functions.DateDiffDay(c.PEDate, todt) >= 0) &&
                                              (a.Account == AccGroup) && (a.Purpose == "Purchase" || a.Purpose == "Purchase Payment")
                                              && (a.Status == null)
                                              select new TrialBalanceDisplay
                                              {
                                                  to = (DateTime?)c.PEDate,
                                                  Particular = a.Purpose == "Purchase Payment" ? "" : c.BillNo,
                                                  AccType = (d.Accounts == a.Account) ? ((a.Purpose != "Purchase Payment") ? "Purchase" : bb.Name) : d.SupplierName,
                                                  Debit = (decimal?)a.Debit,
                                                  Credit = (decimal?)a.Credit,
                                                  AccountsGroupID = a.reference,



                                              }).ToList();

            }
            else if (type2 == "Purchase Return")
            {
                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID
                                              join c in db.PurchaseReturns on a.reference equals c.PurchaseReturnId
                                              join d in db.Suppliers on c.Supplier equals d.SupplierID
                                              let bb = db.Receipts.Where(at => a.Purpose == "Purchase Return Payment" && at.editable == choice.No && at.Reference == c.PurchaseReturnId && at.Remark == "Receipt From Purchase Return").Join(db.Accountss, f1 => f1.PayTo, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                                              let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                                              where (frmdate == "" || EF.Functions.DateDiffDay(c.PRDate, fdt) <= 0) &&
                                              (todate == "" || EF.Functions.DateDiffDay(c.PRDate, todt) >= 0) &&
                                              (a.Account == AccGroup) && (a.Purpose == "Purchase Return" || a.Purpose == "Purchase Return Payment")
                                              && (a.Status == null)
                                              select new TrialBalanceDisplay
                                              {
                                                  to = (DateTime?)c.PRDate,
                                                  Particular = a.Purpose == "Purchase Return Payment" ? "" : c.BillNo,
                                                  AccType = (d.Accounts == a.Account) ? "Purchase" : d.SupplierName,
                                                  Debit = (decimal?)a.Debit,
                                                  Credit = (decimal?)a.Credit,
                                                  AccountsGroupID = a.reference,



                                              }).ToList();


            }
            else if (type2 == "Sales Return")
            {
                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID
                                              join c in db.SalesReturns on a.reference equals c.SalesReturnId
                                              join d in db.Customers on c.Customer equals d.CustomerID
                                              let bb = db.Payments.Where(at => a.Purpose == "Sale Return Payment" && at.editable == choice.No && at.Reference == c.SalesReturnId && at.RefType == "SalesReturn").Join(db.Accountss, f1 => f1.PayFrom, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                                              let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                                              where (frmdate == null || EF.Functions.DateDiffDay(c.SRDate, fdt) <= 0) &&
                                              (todate == null || EF.Functions.DateDiffDay(c.SRDate, todt) >= 0) &&
                                              (a.Account == AccGroup) && (a.Purpose == "Sale Return" || a.Purpose == "Sale Return Payment")
                                              && (a.Status == null)
                                              select new TrialBalanceDisplay
                                              {
                                                  to = (DateTime?)c.SRDate,
                                                  Particular = a.Purpose != "Sale Return Payment" ? c.BillNo : "",
                                                  AccType = (d.Accounts == a.Account) ? ((a.Purpose != "Sale Return Payment") ? "Sales Return" : bb.Name) : d.CustomerName,
                                                  Debit = (decimal?)a.Debit,
                                                  Credit = (decimal?)a.Credit,
                                                  AccountsGroupID = a.reference,
                                              }).ToList();
            }
            else if (type2 == "Sale")
            {
                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID
                                              join c in db.SalesEntrys on a.reference equals c.SalesEntryId
                                              join d in db.Customers on c.Customer equals d.CustomerID
                                              let bb = db.Receipts.Where(at => a.Purpose == "Sale Payment" && at.editable == choice.No && at.Reference == c.SalesEntryId && at.Remark == "Direct Reciept From Sale Entry").Join(db.Accountss, f1 => f1.PayTo, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                                              let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()

                                              where (frmdate == null || EF.Functions.DateDiffDay(c.SEDate, fdt) <= 0) &&
                                              (todate == null || EF.Functions.DateDiffDay(c.SEDate, todt) >= 0) &&
                                              (a.Account == AccGroup) && (a.Purpose == "Sale" || a.Purpose == "Sale Payment")
                                              && (a.Status == null)
                                              select new TrialBalanceDisplay
                                              {
                                                  to = (DateTime?)c.SEDate,
                                                  Particular = (a.Purpose != "Sale Payment" || (d.Accounts != a.Account)) ? c.BillNo : "",
                                                  AccType = (d.Accounts == a.Account) ? ((a.Purpose != "Sale Payment") ? "Sale" : bb.Name) : d.CustomerName,
                                                  Debit = (decimal?)a.Debit,
                                                  Credit = (decimal?)a.Credit,
                                                  AccountsGroupID = a.reference,
                                              }).ToList();
            }
            else
            {
                var goupacc = Common.GetChildAccGroup((long)AccGroup, "", "", fdt, todt, 2, 1);
                var subparent = goupacc.Where(o => o.Parent == AccGroup).ToList();
                List<BalanceSheet> summry = new List<BalanceSheet>();
                foreach (var v in subparent)
                {
                    var f = com.GetChildAccGroupsummary(v.AccountsGroupID, "", "", fdt, todt, 2, 1).ToList();
                    foreach (var ff in f)
                    {
                        summry.Add(ff);
                    }

                }
                vmodel.BalanceSheet = summry.ToList();
                var userpermission = User.IsInRole("All Journal Entry");
                var uid = User.Identity.GetUserId();

                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID into deve
                                              from b in deve.DefaultIfEmpty()

                                              where (b.Group == AccGroup) &&
                                              (pdc == true || a.Status == null) &&
                                              (AccGroup != 68 || a.Status == null) &&
                                              (a.Account != 499) &&
                                              (a.Account != 3) &&
                                              b.Status == Status.active &&

                                              (frmdate == null || EF.Functions.DateDiffDay(a.Date, fdt) <= 0) &&
                                              (todate == null || EF.Functions.DateDiffDay(a.Date, todt) >= 0) &&

                                              (a.Debit != 0 || a.Credit != 0)
                                              group new { b.Name, b.AccountsID, a.Account, a.Debit, a.Credit } by new { b.AccountsID } into g

                                              select new TrialBalanceDisplay
                                              {
                                                  AccountsGroupID = g.Key.AccountsID,
                                                  Particular = g.FirstOrDefault().Name,
                                                  Parent = g.FirstOrDefault().Account,
                                                  Debit = g.Sum(k => k.Debit),
                                                  Credit = g.Sum(k => k.Credit),
                                                  AccType = type2,
                                              }).ToList();
            }
            return View(vmodel);
        }

        [QkAuthorize(Roles = "Dev,TrialBalance")]
        public ActionResult GetGroupTrialBalance(long? AccGroup, string type2, string frmdate, string todate, string parent, bool pdc = false)
        {
            BalanceSheetViewModel vmodel = new BalanceSheetViewModel();
            ViewBag.Parent = parent;
            String format = "dd-MM-yyyy";
            ViewBag.TDte = todate;
            int AccGrp = Convert.ToInt32(AccGroup);
            var todt = DateTime.ParseExact(todate, format, new CultureInfo("en-GB"));
            var fdt = new DateTime();
            vmodel.to = todt;
            ViewBag.Typ = type2;
           
            if (frmdate != null&&frmdate!="")
            {
                ViewBag.FDte = frmdate;
                fdt = DateTime.ParseExact(frmdate, format, new CultureInfo("en-GB"));
                vmodel.from = fdt;
            }
            else
            {
                fdt = DateTime.ParseExact("01-01-2000", format, new CultureInfo("en-GB"));
                ViewBag.FDte = todt.AddYears(-25);
            }

            if (AccGroup == 10)
            {
                AccGroup = 14;
            }
            else if (AccGroup == 11)
            {
                AccGroup = 12;
            }
            if (type2 == "Purchase")
            {
                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID
                                              join c in db.PurchaseEntrys on a.reference equals c.PurchaseEntryId
                                              join d in db.Suppliers on c.Supplier equals d.SupplierID
                                              let bb = db.Payments.Where(at => a.Purpose == "Purchase Payment" && at.editable == choice.No && at.Reference == c.PurchaseEntryId && at.RefType == "Purchase").Join(db.Accountss, f1 => f1.PayFrom, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                                              let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                                              where (frmdate == "" || EF.Functions.DateDiffDay(c.PEDate, fdt) <= 0) &&
                                              (todate == "" || EF.Functions.DateDiffDay(c.PEDate, todt) >= 0) &&
                                              (a.Account == AccGroup) && (a.Purpose == "Purchase" || a.Purpose == "Purchase Payment")
                                              && (a.Status == null)
                                              select new TrialBalanceDisplay
                                              {
                                                  to = (DateTime?)c.PEDate,
                                                  Particular = a.Purpose == "Purchase Payment" ? "" : c.BillNo,
                                                  AccType = (d.Accounts == a.Account) ? ((a.Purpose != "Purchase Payment") ? "Purchase" : bb.Name) : d.SupplierName,
                                                  Debit = (decimal?)a.Debit,
                                                  Credit = (decimal?)a.Credit,
                                                  AccountsGroupID = a.reference,



                                              }).ToList();

            }
            else if (type2 == "Purchase Return")
            {
                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID
                                              join c in db.PurchaseReturns on a.reference equals c.PurchaseReturnId
                                              join d in db.Suppliers on c.Supplier equals d.SupplierID
                                              let bb = db.Receipts.Where(at => a.Purpose == "Purchase Return Payment" && at.editable == choice.No && at.Reference == c.PurchaseReturnId && at.Remark == "Receipt From Purchase Return").Join(db.Accountss, f1 => f1.PayTo, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                                              let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                                              where (frmdate == "" || EF.Functions.DateDiffDay(c.PRDate, fdt) <= 0) &&
                                              (todate == "" || EF.Functions.DateDiffDay(c.PRDate, todt) >= 0) &&
                                              (a.Account == AccGroup) && (a.Purpose == "Purchase Return" || a.Purpose == "Purchase Return Payment")
                                              && (a.Status == null)
                                              select new TrialBalanceDisplay
                                              {
                                                  to = (DateTime?)c.PRDate,
                                                  Particular = a.Purpose == "Purchase Return Payment" ? "" : c.BillNo,
                                                  AccType = (d.Accounts == a.Account) ? "Purchase" : d.SupplierName,
                                                  Debit = (decimal?)a.Debit,
                                                  Credit = (decimal?)a.Credit,
                                                  AccountsGroupID = a.reference,



                                              }).ToList();


            }
            else if (type2 == "Sales Return")
            {
                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID
                                              join c in db.SalesReturns on a.reference equals c.SalesReturnId
                                              join d in db.Customers on c.Customer equals d.CustomerID
                                              let bb = db.Payments.Where(at => a.Purpose == "Sale Return Payment" && at.editable == choice.No && at.Reference == c.SalesReturnId && at.RefType == "SalesReturn").Join(db.Accountss, f1 => f1.PayFrom, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                                              let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                                              where (frmdate == null || EF.Functions.DateDiffDay(c.SRDate, fdt) <= 0) &&
                                              (todate == null || EF.Functions.DateDiffDay(c.SRDate, todt) >= 0) &&
                                              (a.Account == AccGroup) && (a.Purpose == "Sale Return" || a.Purpose == "Sale Return Payment")
                                              && (a.Status == null)
                                              select new TrialBalanceDisplay
                                              {
                                                  to = (DateTime?)c.SRDate,
                                                  Particular = a.Purpose != "Sale Return Payment" ? c.BillNo : "",
                                                  AccType = (d.Accounts == a.Account) ? ((a.Purpose != "Sale Return Payment") ? "Sales Return" : bb.Name) : d.CustomerName,
                                                  Debit = (decimal?)a.Debit,
                                                  Credit = (decimal?)a.Credit,
                                                  AccountsGroupID = a.reference,
                                              }).ToList();
            }
            else if (type2 == "Sale")
            {
                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID
                                              join c in db.SalesEntrys on a.reference equals c.SalesEntryId
                                              join d in db.Customers on c.Customer equals d.CustomerID
                                              let bb = db.Receipts.Where(at => a.Purpose == "Sale Payment" && at.editable == choice.No && at.Reference == c.SalesEntryId && at.Remark == "Direct Reciept From Sale Entry").Join(db.Accountss, f1 => f1.PayTo, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                                              let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()

                                              where (frmdate == null || EF.Functions.DateDiffDay(c.SEDate, fdt) <= 0) &&
                                              (todate == null || EF.Functions.DateDiffDay(c.SEDate, todt) >= 0) &&
                                              (a.Account == AccGroup) && (a.Purpose == "Sale" || a.Purpose == "Sale Payment")
                                              && (a.Status == null)
                                              select new TrialBalanceDisplay
                                              {
                                                  to = (DateTime?)c.SEDate,
                                                  Particular = (a.Purpose != "Sale Payment" || (d.Accounts != a.Account)) ? c.BillNo : "",
                                                  AccType = (d.Accounts == a.Account) ? ((a.Purpose != "Sale Payment") ? "Sale" : bb.Name) : d.CustomerName,
                                                  Debit = (decimal?)a.Debit,
                                                  Credit = (decimal?)a.Credit,
                                                  AccountsGroupID = a.reference,
                                              }).ToList();
            }
            else
            {
                var goupacc = Common.GetChildAccGroup((long)AccGroup, "", "", fdt, todt, 2, 1);
                var subparent = goupacc.Where(o => o.Parent == AccGroup).ToList();
                List<BalanceSheet> summry = new List<BalanceSheet>();
                foreach (var v in subparent)
                {
                    var f = com.GetChildAccGroupsummary(v.AccountsGroupID, "", "", fdt, todt, 2, 1).ToList();
                    foreach (var ff in f)
                    {
                        summry.Add(ff);
                    }

                }
                vmodel.BalanceSheet = summry.ToList();
                var userpermission = User.IsInRole("All Journal Entry");
                var uid = User.Identity.GetUserId();
                
                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID into deve
                                              from b in deve.DefaultIfEmpty()

                                              where (b.Group == AccGroup) &&
                                              (pdc == true || a.Status == null) &&
                                              (AccGroup!=68||a.Status==null)&&
                                              (a.Account != 499) &&
                                              (a.Account != 3) &&
                                              b.Status==Status.active &&

                                              (frmdate == null || EF.Functions.DateDiffDay(a.Date, fdt) <= 0) &&
                                              (todate == null || EF.Functions.DateDiffDay(a.Date, todt) >= 0) &&

                                              (a.Debit != 0 || a.Credit != 0)
                                              group new { b.Name, b.AccountsID, a.Account, a.Debit, a.Credit } by new { b.AccountsID } into g

                                              select new TrialBalanceDisplay
                                              {
                                                  AccountsGroupID = g.Key.AccountsID,
                                                  Particular = g.FirstOrDefault().Name,
                                                  Parent = g.FirstOrDefault().Account,
                                                  Debit = g.Sum(k => k.Debit),
                                                  Credit = g.Sum(k => k.Credit),
                                                  AccType = type2,
                                              }).ToList();
            }
            return View(vmodel);
        }
        [QkAuthorize(Roles = "Dev,TrialBalance")]
        public ActionResult GetGroupTrialBalancetrial(long? AccGroup, string type2, string frmdate, string todate, string parent, bool pdc = false)
        {
            BalanceSheetViewModel vmodel = new BalanceSheetViewModel();
            ViewBag.Parent = parent;
            String format = "dd-MM-yyyy";
            ViewBag.TDte = todate;
            int AccGrp = Convert.ToInt32(AccGroup);
            var todt = DateTime.ParseExact(todate, format, new CultureInfo("en-GB"));
            var fdt = new DateTime();
            vmodel.to = todt;
            ViewBag.Typ = type2;
            if (frmdate != null)
            {
                ViewBag.FDte = frmdate;
                fdt = DateTime.ParseExact(frmdate, format, new CultureInfo("en-GB"));
                vmodel.from = fdt;
            }
            else
            {
                ViewBag.FDte = todt.AddYears(-2);
            }

            if (AccGroup == 10)
            {
                AccGroup = 14;
            }
            else if (AccGroup == 11)
            {
                AccGroup = 12;
            }
            if (type2 == "Purchase")
            {
                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID
                                              join c in db.PurchaseEntrys on a.reference equals c.PurchaseEntryId
                                              join d in db.Suppliers on c.Supplier equals d.SupplierID
                                              let bb = db.Payments.Where(at => a.Purpose == "Purchase Payment" && at.editable == choice.No && at.Reference == c.PurchaseEntryId && at.RefType == "Purchase").Join(db.Accountss, f1 => f1.PayFrom, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                                              let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                                              where (frmdate == "" || EF.Functions.DateDiffDay(c.PEDate, fdt) <= 0) &&
                                              (todate == "" || EF.Functions.DateDiffDay(c.PEDate, todt) >= 0) &&
                                              (a.Account == AccGroup) && (a.Purpose == "Purchase" || a.Purpose == "Purchase Payment")
                                              && (a.Status == null)
                                              select new TrialBalanceDisplay
                                              {
                                                  to = (DateTime?)c.PEDate,
                                                  Particular = a.Purpose == "Purchase Payment" ? "" : c.BillNo,
                                                  AccType = (d.Accounts == a.Account) ? ((a.Purpose != "Purchase Payment") ? "Purchase" : bb.Name) : d.SupplierName,
                                                  Debit = (decimal?)a.Debit,
                                                  Credit = (decimal?)a.Credit,
                                                  AccountsGroupID = a.reference,



                                              }).ToList();

            }
            else if (type2 == "Purchase Return")
            {
                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID
                                              join c in db.PurchaseReturns on a.reference equals c.PurchaseReturnId
                                              join d in db.Suppliers on c.Supplier equals d.SupplierID
                                              let bb = db.Receipts.Where(at => a.Purpose == "Purchase Return Payment" && at.editable == choice.No && at.Reference == c.PurchaseReturnId && at.Remark == "Receipt From Purchase Return").Join(db.Accountss, f1 => f1.PayTo, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                                              let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                                              where (frmdate == "" || EF.Functions.DateDiffDay(c.PRDate, fdt) <= 0) &&
                                              (todate == "" || EF.Functions.DateDiffDay(c.PRDate, todt) >= 0) &&
                                              (a.Account == AccGroup) && (a.Purpose == "Purchase Return" || a.Purpose == "Purchase Return Payment")
                                              && (a.Status == null)
                                              select new TrialBalanceDisplay
                                              {
                                                  to = (DateTime?)c.PRDate,
                                                  Particular = a.Purpose == "Purchase Return Payment" ? "" : c.BillNo,
                                                  AccType = (d.Accounts == a.Account) ? "Purchase" : d.SupplierName,
                                                  Debit = (decimal?)a.Debit,
                                                  Credit = (decimal?)a.Credit,
                                                  AccountsGroupID = a.reference,



                                              }).ToList();


            }
            else if (type2 == "Sales Return")
            {
                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID
                                              join c in db.SalesReturns on a.reference equals c.SalesReturnId
                                              join d in db.Customers on c.Customer equals d.CustomerID
                                              let bb = db.Payments.Where(at => a.Purpose == "Sale Return Payment" && at.editable == choice.No && at.Reference == c.SalesReturnId && at.RefType == "SalesReturn").Join(db.Accountss, f1 => f1.PayFrom, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                                              let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                                              where (frmdate == null || EF.Functions.DateDiffDay(c.SRDate, fdt) <= 0) &&
                                              (todate == null || EF.Functions.DateDiffDay(c.SRDate, todt) >= 0) &&
                                              (a.Account == AccGroup) && (a.Purpose == "Sale Return" || a.Purpose == "Sale Return Payment")
                                              && (a.Status == null)
                                              select new TrialBalanceDisplay
                                              {
                                                  to = (DateTime?)c.SRDate,
                                                  Particular = a.Purpose != "Sale Return Payment" ? c.BillNo : "",
                                                  AccType = (d.Accounts == a.Account) ? ((a.Purpose != "Sale Return Payment") ? "Sales Return" : bb.Name) : d.CustomerName,
                                                  Debit = (decimal?)a.Debit,
                                                  Credit = (decimal?)a.Credit,
                                                  AccountsGroupID = a.reference,
                                              }).ToList();
            }
            else if (type2 == "Sale")
            {
                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID
                                              join c in db.SalesEntrys on a.reference equals c.SalesEntryId
                                              join d in db.Customers on c.Customer equals d.CustomerID
                                              let bb = db.Receipts.Where(at => a.Purpose == "Sale Payment" && at.editable == choice.No && at.Reference == c.SalesEntryId && at.Remark == "Direct Reciept From Sale Entry").Join(db.Accountss, f1 => f1.PayTo, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                                              let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()

                                              where (frmdate == null || EF.Functions.DateDiffDay(c.SEDate, fdt) <= 0) &&
                                              (todate == null || EF.Functions.DateDiffDay(c.SEDate, todt) >= 0) &&
                                              (a.Account == AccGroup) && (a.Purpose == "Sale" || a.Purpose == "Sale Payment")
                                              && (a.Status == null)
                                              select new TrialBalanceDisplay
                                              {
                                                  to = (DateTime?)c.SEDate,
                                                  Particular = (a.Purpose != "Sale Payment" || (d.Accounts != a.Account)) ? c.BillNo : "",
                                                  AccType = (d.Accounts == a.Account) ? ((a.Purpose != "Sale Payment") ? "Sale" : bb.Name) : d.CustomerName,
                                                  Debit = (decimal?)a.Debit,
                                                  Credit = (decimal?)a.Credit,
                                                  AccountsGroupID = a.reference,
                                              }).ToList();
            }
            else
            {

                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID into deve
                                              from b in deve.DefaultIfEmpty()
                                              where (b.Group == AccGroup) &&
                                              (pdc == true || a.Status == null) &&
                                              (a.Account != 499) &&
                                            


                                              (frmdate == null || EF.Functions.DateDiffDay(a.Date, fdt) <= 0) &&
                                              (todate == null || EF.Functions.DateDiffDay(a.Date, todt) >= 0) &&

                                              (a.Debit != 0 || a.Credit != 0)
                                              group new { b.Name, b.AccountsID, a.Account, a.Debit, a.Credit } by new { b.AccountsID } into g

                                              select new TrialBalanceDisplay
                                              {
                                                  AccountsGroupID = g.Key.AccountsID,
                                                  Particular = g.FirstOrDefault().Name,
                                                  Parent = g.FirstOrDefault().Account,
                                                  Debit = g.Sum(k => k.Debit),
                                                  Credit = g.Sum(k => k.Credit),
                                                  AccType = type2,
                                              }).ToList();
            }
            return View(vmodel);
        }

        [QkAuthorize(Roles = "Dev,TrialBalance")]
        public ActionResult GetGroupTrialBalance3(long? AccGroup, string type2, string frmdate, string todate, string parent)
        {
            BalanceSheetViewModel vmodel = new BalanceSheetViewModel();
            ViewBag.Parent = parent;
            String format = "dd-MM-yyyy";
            ViewBag.TDte = todate;
            int AccGrp = Convert.ToInt32(AccGroup);
            var todt = DateTime.ParseExact(todate, format, new CultureInfo("en-GB"));
            var fdt = new DateTime();
            vmodel.to = todt;
            ViewBag.Typ = type2;
            if (frmdate != null)
            {
                ViewBag.FDte = frmdate;
                fdt = DateTime.ParseExact(frmdate, format, new CultureInfo("en-GB"));
                vmodel.from = fdt;
            }
            else
            {
                ViewBag.FDte = todt.AddYears(-2);
            }

            if (AccGroup == 10)
            {
                AccGroup = 14;
            }
            else if (AccGroup == 11)
            {
                AccGroup = 12;
            }
            if (type2 == "Purchase")
            {
                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID
                                              join c in db.PurchaseEntrys on a.reference equals c.PurchaseEntryId
                                              join d in db.Suppliers on c.Supplier equals d.SupplierID
                                              let bb = db.Payments.Where(at => a.Purpose == "Purchase Payment" && at.editable == choice.No && at.Reference == c.PurchaseEntryId && at.RefType == "Purchase").Join(db.Accountss, f1 => f1.PayFrom, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                                              let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                                              where (frmdate == "" || EF.Functions.DateDiffDay(c.PEDate, fdt) <= 0) &&
                                              (todate == "" || EF.Functions.DateDiffDay(c.PEDate, todt) >= 0) &&
                                              (a.Account == AccGroup) && (a.Purpose == "Purchase" || a.Purpose == "Purchase Payment")
                                              && (a.Status == null)
                                              select new TrialBalanceDisplay
                                              {
                                                  to = (DateTime?)c.PEDate,
                                                  Particular = a.Purpose == "Purchase Payment" ? "" : c.BillNo,
                                                  AccType = (d.Accounts == a.Account) ? ((a.Purpose != "Purchase Payment") ? "Purchase" : bb.Name) : d.SupplierName,
                                                  Debit = (decimal?)a.Debit,
                                                  Credit = (decimal?)a.Credit,
                                                  AccountsGroupID = a.reference,



                                              }).ToList();

            }
            else if (type2 == "Purchase Return")
            {
                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID
                                              join c in db.PurchaseReturns on a.reference equals c.PurchaseReturnId
                                              join d in db.Suppliers on c.Supplier equals d.SupplierID
                                              let bb = db.Receipts.Where(at => a.Purpose == "Purchase Return Payment" && at.editable == choice.No && at.Reference == c.PurchaseReturnId && at.Remark == "Receipt From Purchase Return").Join(db.Accountss, f1 => f1.PayTo, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                                              let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                                              where (frmdate == "" || EF.Functions.DateDiffDay(c.PRDate, fdt) <= 0) &&
                                              (todate == "" || EF.Functions.DateDiffDay(c.PRDate, todt) >= 0) &&
                                              (a.Account == AccGroup) && (a.Purpose == "Purchase Return" || a.Purpose == "Purchase Return Payment")
                                              && (a.Status == null)
                                              select new TrialBalanceDisplay
                                              {
                                                  to = (DateTime?)c.PRDate,
                                                  Particular = a.Purpose == "Purchase Return Payment" ? "" : c.BillNo,
                                                  AccType = (d.Accounts == a.Account) ? "Purchase" : d.SupplierName,
                                                  Debit = (decimal?)a.Debit,
                                                  Credit = (decimal?)a.Credit,
                                                  AccountsGroupID = a.reference,



                                              }).ToList();


            }
            else if (type2 == "Sales Return")
            {
                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID
                                              join c in db.SalesReturns on a.reference equals c.SalesReturnId
                                              join d in db.Customers on c.Customer equals d.CustomerID
                                              let bb = db.Payments.Where(at => a.Purpose == "Sale Return Payment" && at.editable == choice.No && at.Reference == c.SalesReturnId && at.RefType == "SalesReturn").Join(db.Accountss, f1 => f1.PayFrom, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                                              let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()
                                              where (frmdate == null || EF.Functions.DateDiffDay(c.SRDate, fdt) <= 0) &&
                                              (todate == null || EF.Functions.DateDiffDay(c.SRDate, todt) >= 0) &&
                                              (a.Account == AccGroup) && (a.Purpose == "Sale Return" || a.Purpose == "Sale Return Payment")
                                              && (a.Status == null)
                                              select new TrialBalanceDisplay
                                              {
                                                  to = (DateTime?)c.SRDate,
                                                  Particular = a.Purpose != "Sale Return Payment" ? c.BillNo : "",
                                                  AccType = (d.Accounts == a.Account) ? ((a.Purpose != "Sale Return Payment") ? "Sales Return" : bb.Name) : d.CustomerName,
                                                  Debit = (decimal?)a.Debit,
                                                  Credit = (decimal?)a.Credit,
                                                  AccountsGroupID = a.reference,
                                              }).ToList();
            }
            else if (type2 == "Sale")
            {
                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID
                                              join c in db.SalesEntrys on a.reference equals c.SalesEntryId
                                              join d in db.Customers on c.Customer equals d.CustomerID
                                              let bb = db.Receipts.Where(at => a.Purpose == "Sale Payment" && at.editable == choice.No && at.Reference == c.SalesEntryId && at.Remark == "Direct Reciept From Sale Entry").Join(db.Accountss, f1 => f1.PayTo, f2 => f2.AccountsID, (f1, f2) => new { f2.Name, f2.TRN }).FirstOrDefault()
                                              let AC = db.Accountss.Where(at => (d.Accounts != a.Account && at.AccountsID == d.Accounts) || (d.Accounts == a.Account && at.AccountsID == d.Accounts)).FirstOrDefault()

                                              where (frmdate == null || EF.Functions.DateDiffDay(c.SEDate, fdt) <= 0) &&
                                              (todate == null || EF.Functions.DateDiffDay(c.SEDate, todt) >= 0) &&
                                              (a.Account == AccGroup) && (a.Purpose == "Sale" || a.Purpose == "Sale Payment")
                                              && (a.Status == null)
                                              select new TrialBalanceDisplay
                                              {
                                                  to = (DateTime?)c.SEDate,
                                                  Particular = (a.Purpose != "Sale Payment" || (d.Accounts != a.Account)) ? c.BillNo : "",
                                                  AccType = (d.Accounts == a.Account) ? ((a.Purpose != "Sale Payment") ? "Sale" : bb.Name) : d.CustomerName,
                                                  Debit = (decimal?)a.Debit,
                                                  Credit = (decimal?)a.Credit,
                                                  AccountsGroupID = a.reference,
                                              }).ToList();
            }
            else
            {

                vmodel.TrialBalanceDisplay = (from a in db.AccountsTransactions
                                              join b in db.Accountss on a.Account equals b.AccountsID into deve
                                              from b in deve.DefaultIfEmpty()
                                              where (b.Group == AccGroup) &&
                                              (a.Status == null) &&
                                              (a.Account != 499) &&


                                              (todate == null || EF.Functions.DateDiffDay(a.Date, todt) >= 0) &&

                                              (a.Debit != 0 || a.Credit != 0)
                                              group new { b.Name, b.AccountsID, a.Account, a.Debit, a.Credit } by new { b.AccountsID } into g

                                              select new TrialBalanceDisplay
                                              {
                                                  AccountsGroupID = g.Key.AccountsID,
                                                  Particular = g.FirstOrDefault().Name,
                                                  Parent = g.FirstOrDefault().Account,
                                                  Debit = g.Sum(k => k.Debit),
                                                  Credit = g.Sum(k => k.Credit),
                                                  AccType = type2,
                                              }).ToList();
            }
            return View(vmodel);
        }

        [QkAuthorize(Roles = "Dev,TrialBalance")]
        public ActionResult GetTrialBalance(string fromdate, string todate)
        {
            BalanceSheetViewModel vmodel = new BalanceSheetViewModel();
            String format = "dd-MM-yyyy";
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? from = null;
            DateTime? to = null;
            if (fromdate == "")
            {
                fromdate = "01-01-2000";
            }

            if (fromdate != "")
            {
                fdate = DateTime.ParseExact(fromdate, format, new CultureInfo("en-GB"));
                from = fdate;
            }
            if (todate != "")
            {
                tdate = DateTime.ParseExact(todate, format, new CultureInfo("en-GB"));
                to = tdate;

            }
            DateTime too = DateTime.ParseExact(todate, format, new CultureInfo("en-GB"));
            vmodel.to = to;
            vmodel.from = from;
            ViewBag.TDte = todate;
            var today = DateTime.Now;
            ViewBag.FDte = too.AddYears(-2);
            ViewBag.fromdate = fromdate;
            var fun = 3;
            var Count = 0;
            var RetVal = 0;

            //capital acccount

            var CapitalAcoount = Common.GetChildAccGroupTrial(1, "Capital Account", "liability", to, from);
            RetVal = getSubTotalTrial(Count, CapitalAcoount);//calculate child amounts
            RetVal = chkDebitCredit(CapitalAcoount);

            var capparent = CapitalAcoount.Where(a => a.Parent == 0).ToList();//parent
            var capchild = CapitalAcoount.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
            var capacc = capparent.Union(capchild);
            //Current Asset
            var CurrentAssets = Common.GetChildAccGroupTrial(2, "Current Assets", "asset", to, from);
            RetVal = getSubTotalTrial(Count, CurrentAssets);
            RetVal = chkDebitCredit(CurrentAssets);


            var cuparent = CurrentAssets.Where(a => a.Parent == 0).ToList();//parent
            var cuchild = CurrentAssets.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)); //child 

            //-------stock in hand---------
                // (Phase-3 cleanup 2026-06-12) removed a dead legacy block: a hardcoded "Stock-in-hand 2,783,344.19"
                // row was built here but its Union into the result was already commented out — never displayed.
            var cuasset = cuparent.Union(cuchild);

            //current liabilities
            var CurrentLiabilities = Common.GetChildAccGroupTrial(3, "Current Liabilities", "liability", to, from);
            RetVal = getSubTotalTrial(Count, CurrentLiabilities);
            RetVal = chkDebitCredit(CurrentLiabilities);

            var clparent = CurrentLiabilities.Where(a => a.Parent == 0).ToList();//parent
            var clchild = CurrentLiabilities.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
            var crlib = clparent.Union(clchild);

            //Investments
            var Investments = Common.GetChildAccGroupTrial(5, "Investments", "asset", to, from);
            RetVal = getSubTotalTrial(Count, Investments);
            RetVal = chkDebitCredit(Investments);

            var invparent = Investments.Where(a => a.Parent == 0).ToList();//parent
            var invchild = Investments.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
            var invest = invparent.Union(invchild);

            //Fixed Asset
            var FixedAssets = Common.GetChildAccGroupTrial(4, "Fixed Assets", "asset", to, from);
            RetVal = getSubTotalTrial(Count, FixedAssets);
            RetVal = chkDebitCredit(FixedAssets);

            var fixparent = FixedAssets.Where(a => a.Parent == 0).ToList();//parent
            var fixchild = FixedAssets.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
            var fxasset = fixparent.Union(fixchild);

            //Loans (Liability)
            var LoansLiability = Common.GetChildAccGroupTrial(6, "Loans (Liability)", "liability", to, from);
            RetVal = getSubTotalTrial(Count, LoansLiability);
            RetVal = chkDebitCredit(LoansLiability);

            var llparent = LoansLiability.Where(a => a.Parent == 0).ToList();//parent
            var llchild = LoansLiability.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
            var loanlib = llparent.Union(llchild);


            //profit and loss account



            //Revenue Accounts //stored procedure fun=2
            var RevenueAccounts = Common.GetChildAccGroupTrial(7, "Revenue Accounts", "asset", to, from).ToList();


            RetVal = getTotalDrCr(Count, RevenueAccounts);
            RetVal = chkDebitCredit(RevenueAccounts);


            var revparent = RevenueAccounts.Where(a => a.Parent == 0).ToList();//parent
            var revchild = RevenueAccounts.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child

            var revacc = revparent.Union(revchild);




            var uni = capacc.Union(cuasset);
            uni = uni.Union(crlib);//p & f
            uni = uni.Union(invest);
            uni = uni.Union(fxasset);
            uni = uni.Union(loanlib);
            uni = uni.Union(revacc);


            var ACCGroups = db.AccountsGroups.Where(a => a.Parent == 0 && a.Primary == 0 && a.Editable == 0).ToList();
            foreach (var acc in ACCGroups)
            {
                var GroupItem = Common.GetChildAccGroupTrial((int)acc.AccountsGroupID, acc.Name, "asset", to, from);
                decimal Amount = GroupItem != null ? (decimal)GroupItem.Sum(a => a.Debit - a.Credit) : 0;
                RetVal = getSubTotalTrialNew(Count, GroupItem);
                RetVal = chkDebitCredit(GroupItem);

                var GroupParent = GroupItem.Where(a => a.Parent == 0 && (a.Debit != 0 || a.Credit != 0)).ToList();
                var GroupChild = GroupItem.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
                var GroupI = GroupParent.Union(GroupChild);
                if (Amount != 0)
                {
                    GroupI.ToList().ForEach(i => i.Temp = 1);
                    GroupI.ToList().ForEach(i => i.Parent = acc.AccountsGroupID);
                    uni = uni.Union(GroupI);
                }
            }



            var trial = uni.Where(a => a.Temp != 0).ToList();


            var count = getOrdering(trial);

            //assign variable
            var trialbalance = (from a in trial
                                select new
                                {
                                    AccountsGroupID = a.AccountsGroupID,
                                    Particular = a?.Particulars,
                                    Debit = a?.Debit,
                                    Credit = a?.Credit,
                                    AccType = a.AccType,
                                    Parent = a?.Parent,
                                });

            //change to view model
            vmodel.TrialBalanceDisplay = (from a in trialbalance
                                          select new TrialBalanceDisplay
                                          {
                                              AccountsGroupID = a.AccountsGroupID,
                                              Particular = a?.Particular,
                                              Debit = a?.Debit,
                                              Credit = a?.Credit,
                                              Parent = a?.Parent,
                                              AccType = a.AccType,
                                          }).ToList();


            decimal? DTAmount = 0;
            decimal? CTAmount = 0;
            if (vmodel.TrialBalanceDisplay != null)
            {
                if (vmodel.TrialBalanceDisplay.Any())
                {
                    int TotCount = 0;
                    foreach (var item in vmodel.TrialBalanceDisplay)
                    {
                        decimal? AmountCr = 0;
                        decimal? AmountDr = 0;
                        decimal? Debit = 0;
                        decimal? Credit = 0;

                        Debit = item.Debit;
                        AmountDr = (Debit);
                        AmountDr = AmountDr != null ? AmountDr : 0;

                        Credit = item.Credit;
                        AmountCr = (Credit);
                        AmountCr = AmountCr != null ? AmountCr : 0;

                        if (item.Parent == 0)
                        {
                            if (item.AccType == "asset" && item.AccountsGroupID != 7 && item.Parent != 7)
                            {
                                if (Debit.Value > 0)
                                {
                                }
                                else
                                {
                                }
                                DTAmount += AmountDr;
                            }
                            if (item.AccType == "liability" && item.AccountsGroupID != 7 && item.Parent != 7)
                            {
                                if (Credit.Value > 0)
                                {
                                }
                                else
                                {
                                }
                                CTAmount += AmountCr;
                            }
                            if (item.AccountsGroupID == 7 || item.Parent == 7)
                            {
                            }

                        }
                        else
                        {
                            if (item.Parent == item.AccountsGroupID)
                            {
                            }
                            else
                            {


                            }

                            if (item.AccType == "asset" || item.AccType == "liability")
                            {
                                if (item.AccountsGroupID == 7 || item.Parent == 7)
                                { }
                                else
                                {
                                }

                            }
                        }

                        if (item.Parent == 7)
                        {
                            DTAmount += AmountDr;
                            CTAmount += AmountCr;
                        }

                        TotCount++;
                    }
                }
            }










            var diff = DTAmount - CTAmount;



            var rev = db.AccountsTransactions.Where(o => o.Account == 107162 && o.Purpose == "Opening Balance").FirstOrDefault();

            if (rev != null && diff != 0)
            {
                rev.Debit = rev.Debit - (decimal)diff;
                db.Entry(rev).State = EntityState.Modified;
                db.SaveChanges();

            }




            vmodel = new BalanceSheetViewModel();

            if (todate != "")
            {
                tdate = DateTime.ParseExact(todate, format, new CultureInfo("en-GB"));
                to = tdate;

            }
            too = DateTime.ParseExact(todate, format, new CultureInfo("en-GB"));
            vmodel.to = to;
            vmodel.from = from;
            ViewBag.TDte = todate;
            today = DateTime.Now;
            ViewBag.FDte = too.AddYears(-2);

            fun = 3;
            Count = 0;
            RetVal = 0;

            //capital acccount

            CapitalAcoount = Common.GetChildAccGroupTrial(1, "Capital Account", "liability", to, from);
            RetVal = getSubTotalTrial(Count, CapitalAcoount);//calculate child amounts
            RetVal = chkDebitCredit(CapitalAcoount);

            capparent = CapitalAcoount.Where(a => a.Parent == 0).ToList();//parent
            capchild = CapitalAcoount.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
            capacc = capparent.Union(capchild);

            //Current Asset
            CurrentAssets = Common.GetChildAccGroupTrial(2, "Current Assets", "asset", to, from);
            RetVal = getSubTotalTrial(Count, CurrentAssets);
            RetVal = chkDebitCredit(CurrentAssets);


            cuparent = CurrentAssets.Where(a => a.Parent == 0).ToList();//parent
            cuchild = CurrentAssets.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)); //child 

            //-------stock in hand---------
                // (Phase-3 cleanup 2026-06-12) removed a dead legacy block: a hardcoded "Stock-in-hand 2,783,344.19"
                // row was built here but its Union into the result was already commented out — never displayed.
            cuasset = cuparent.Union(cuchild);

            //current liabilities
            CurrentLiabilities = Common.GetChildAccGroupTrial(3, "Current Liabilities", "liability", to, from);
            RetVal = getSubTotalTrial(Count, CurrentLiabilities);
            RetVal = chkDebitCredit(CurrentLiabilities);

            clparent = CurrentLiabilities.Where(a => a.Parent == 0).ToList();//parent
            clchild = CurrentLiabilities.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
            crlib = clparent.Union(clchild);

            //Investments
            Investments = Common.GetChildAccGroupTrial(5, "Investments", "asset", to, from);
            RetVal = getSubTotalTrial(Count, Investments);
            RetVal = chkDebitCredit(Investments);

            invparent = Investments.Where(a => a.Parent == 0).ToList();//parent
            invchild = Investments.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
            invest = invparent.Union(invchild);

            //Fixed Asset
            FixedAssets = Common.GetChildAccGroupTrial(4, "Fixed Assets", "asset", to, from);
            RetVal = getSubTotalTrial(Count, FixedAssets);
            RetVal = chkDebitCredit(FixedAssets);

            fixparent = FixedAssets.Where(a => a.Parent == 0).ToList();//parent
            fixchild = FixedAssets.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
            fxasset = fixparent.Union(fixchild);

            //Loans (Liability)
            LoansLiability = Common.GetChildAccGroupTrial(6, "Loans (Liability)", "liability", to, from);
            RetVal = getSubTotalTrial(Count, LoansLiability);
            RetVal = chkDebitCredit(LoansLiability);

            llparent = LoansLiability.Where(a => a.Parent == 0).ToList();//parent
            llchild = LoansLiability.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
            loanlib = llparent.Union(llchild);


            //profit and loss account

            //            pfparent = pandf.Where(a => a.Parent == 0).ToList();//parent
            //          pfchild = pandf.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child


            //Revenue Accounts //stored procedure fun=2
            RevenueAccounts = Common.GetChildAccGroupTrial(7, "Revenue Accounts", "asset", to, from).ToList();


            RetVal = getTotalDrCr(Count, RevenueAccounts);
            RetVal = chkDebitCredit(RevenueAccounts);


            revparent = RevenueAccounts.Where(a => a.Parent == 0).ToList();//parent
            revchild = RevenueAccounts.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child

            revacc = revparent.Union(revchild);




            uni = capacc.Union(cuasset);
            uni = uni.Union(crlib);//p & f
            uni = uni.Union(invest);
            uni = uni.Union(fxasset);
            uni = uni.Union(loanlib);
            uni = uni.Union(revacc);


            ACCGroups = db.AccountsGroups.Where(a => a.Parent == 0 && a.Primary == 0 && a.Editable == 0).ToList();
            foreach (var acc in ACCGroups)
            {
                var GroupItem = Common.GetChildAccGroupTrial((int)acc.AccountsGroupID, acc.Name, "asset", to, from);
                decimal Amount = GroupItem != null ? (decimal)GroupItem.Sum(a => a.Debit - a.Credit) : 0;
                RetVal = getSubTotalTrialNew(Count, GroupItem);
                RetVal = chkDebitCredit(GroupItem);

                var GroupParent = GroupItem.Where(a => a.Parent == 0 && (a.Debit != 0 || a.Credit != 0)).ToList();
                var GroupChild = GroupItem.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
                var GroupI = GroupParent.Union(GroupChild);
                if (Amount != 0)
                {
                    GroupI.ToList().ForEach(i => i.Temp = 1);
                    GroupI.ToList().ForEach(i => i.Parent = acc.AccountsGroupID);
                    uni = uni.Union(GroupI);
                }
            }



            trial = uni.Where(a => a.Temp != 0).ToList();


            count = getOrdering(trial);

















            //assign variable
            trialbalance = (from a in trial
                            select new
                            {
                                AccountsGroupID = a.AccountsGroupID,
                                Particular = a?.Particulars,
                                Debit = a?.Debit,
                                Credit = a?.Credit,
                                AccType = a.AccType,
                                Parent = a?.Parent,
                            });

            //change to view model
            vmodel.TrialBalanceDisplay = (from a in trialbalance
                                          select new TrialBalanceDisplay
                                          {
                                              AccountsGroupID = a.AccountsGroupID,
                                              Particular = a?.Particular,
                                              Debit = a?.Debit,
                                              Credit = a?.Credit,
                                              Parent = a?.Parent,
                                              AccType = a.AccType,
                                          }).ToList();

            companySet();
            return View(vmodel);
        }
        [QkAuthorize(Roles = "Dev,TrialBalance")]
        public ActionResult GetTrialBalancefortrial(string fromdate, string todate)
        {
            BalanceSheetViewModel vmodel = new BalanceSheetViewModel();
            String format = "dd-MM-yyyy";
            DateTime? fdate = null;
            DateTime? tdate = null;
            DateTime? from = null;
            DateTime? to = null;
            if (fromdate == "")
            {
                fromdate = "01-01-2000";
            }

            if (fromdate != "")
            {
                fdate = DateTime.ParseExact(fromdate, format, new CultureInfo("en-GB"));
                from = fdate;
            }
            if (todate != "")
            {
                tdate = DateTime.ParseExact(todate, format, new CultureInfo("en-GB"));
                to = tdate;

            }
            DateTime too = DateTime.ParseExact(todate, format, new CultureInfo("en-GB"));
            vmodel.to = to;
            vmodel.from = from;
            ViewBag.TDte = todate;
            var today = DateTime.Now;
            ViewBag.FDte = too.AddYears(-2);
            ViewBag.fromdate = fromdate;
            var fun = 3;
            var Count = 0;
            var RetVal = 0;

            //capital acccount

            var CapitalAcoount = Common.GetChildAccGroupTrial(1, "Capital Account", "liability", to, from);
            RetVal = getSubTotalTrial(Count, CapitalAcoount);//calculate child amounts
            RetVal = chkDebitCredit(CapitalAcoount);

            var capparent = CapitalAcoount.Where(a => a.Parent == 0).ToList();//parent
            var capchild = CapitalAcoount.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
            var capacc = capparent.Union(capchild);
            //Current Asset
            var CurrentAssets = Common.GetChildAccGroupTrial(2, "Current Assets", "asset", to, from);
            RetVal = getSubTotalTrial(Count, CurrentAssets);
            RetVal = chkDebitCredit(CurrentAssets);


            var cuparent = CurrentAssets.Where(a => a.Parent == 0).ToList();//parent
            var cuchild = CurrentAssets.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)); //child 

            //-------stock in hand---------
                // (Phase-3 cleanup 2026-06-12) removed a dead legacy block: a hardcoded "Stock-in-hand 2,783,344.19"
                // row was built here but its Union into the result was already commented out — never displayed.
            var cuasset = cuparent.Union(cuchild);

            //current liabilities
            var CurrentLiabilities = Common.GetChildAccGroupTrial(3, "Current Liabilities", "liability", to, from);
            RetVal = getSubTotalTrial(Count, CurrentLiabilities);
            RetVal = chkDebitCredit(CurrentLiabilities);

            var clparent = CurrentLiabilities.Where(a => a.Parent == 0).ToList();//parent
            var clchild = CurrentLiabilities.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
            var crlib = clparent.Union(clchild);

            //Investments
            var Investments = Common.GetChildAccGroupTrial(5, "Investments", "asset", to, from);
            RetVal = getSubTotalTrial(Count, Investments);
            RetVal = chkDebitCredit(Investments);

            var invparent = Investments.Where(a => a.Parent == 0).ToList();//parent
            var invchild = Investments.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
            var invest = invparent.Union(invchild);

            //Fixed Asset
            var FixedAssets = Common.GetChildAccGroupTrial(4, "Fixed Assets", "asset", to, from);
            RetVal = getSubTotalTrial(Count, FixedAssets);
            RetVal = chkDebitCredit(FixedAssets);

            var fixparent = FixedAssets.Where(a => a.Parent == 0).ToList();//parent
            var fixchild = FixedAssets.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
            var fxasset = fixparent.Union(fixchild);

            //Loans (Liability)
            var LoansLiability = Common.GetChildAccGroupTrial(6, "Loans (Liability)", "liability", to, from);
            RetVal = getSubTotalTrial(Count, LoansLiability);
            RetVal = chkDebitCredit(LoansLiability);

            var llparent = LoansLiability.Where(a => a.Parent == 0).ToList();//parent
            var llchild = LoansLiability.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
            var loanlib = llparent.Union(llchild);


            //profit and loss account



            //Revenue Accounts //stored procedure fun=2
            var RevenueAccounts = Common.GetChildAccGroupTrial(7, "Revenue Accounts", "asset", to, from).ToList();


            RetVal = getTotalDrCr(Count, RevenueAccounts);
            RetVal = chkDebitCredit(RevenueAccounts);


            var revparent = RevenueAccounts.Where(a => a.Parent == 0).ToList();//parent
            var revchild = RevenueAccounts.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child

            var revacc = revparent.Union(revchild);




            var uni = capacc.Union(cuasset);
            uni = uni.Union(crlib);//p & f
            uni = uni.Union(invest);
            uni = uni.Union(fxasset);
            uni = uni.Union(loanlib);
            uni = uni.Union(revacc);


            var ACCGroups = db.AccountsGroups.Where(a => a.Parent == 0 && a.Primary == 0 && a.Editable == 0).ToList();
            foreach (var acc in ACCGroups)
            {
                var GroupItem = Common.GetChildAccGroupTrial((int)acc.AccountsGroupID, acc.Name, "asset", to, from);
                decimal Amount = GroupItem != null ? (decimal)GroupItem.Sum(a => a.Debit - a.Credit) : 0;
                RetVal = getSubTotalTrialNew(Count, GroupItem);
                RetVal = chkDebitCredit(GroupItem);

                var GroupParent = GroupItem.Where(a => a.Parent == 0 && (a.Debit != 0 || a.Credit != 0)).ToList();
                var GroupChild = GroupItem.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
                var GroupI = GroupParent.Union(GroupChild);
                if (Amount != 0)
                {
                    GroupI.ToList().ForEach(i => i.Temp = 1);
                    GroupI.ToList().ForEach(i => i.Parent = acc.AccountsGroupID);
                    uni = uni.Union(GroupI);
                }
            }



            var trial = uni.Where(a => a.Temp != 0).ToList();


            var count = getOrdering(trial);

            //assign variable
            var trialbalance = (from a in trial
                                select new
                                {
                                    AccountsGroupID = a.AccountsGroupID,
                                    Particular = a?.Particulars,
                                    Debit = a?.Debit,
                                    Credit = a?.Credit,
                                    AccType = a.AccType,
                                    Parent = a?.Parent,
                                });

            //change to view model
            vmodel.TrialBalanceDisplay = (from a in trialbalance
                                          select new TrialBalanceDisplay
                                          {
                                              AccountsGroupID = a.AccountsGroupID,
                                              Particular = a?.Particular,
                                              Debit = a?.Debit,
                                              Credit = a?.Credit,
                                              Parent = a?.Parent,
                                              AccType = a.AccType,
                                          }).ToList();


            decimal? DTAmount = 0;
            decimal? CTAmount = 0;
            if (vmodel.TrialBalanceDisplay != null)
            {
                if (vmodel.TrialBalanceDisplay.Any())
                {
                    int TotCount = 0;
                    foreach (var item in vmodel.TrialBalanceDisplay)
                    {
                        decimal? AmountCr = 0;
                        decimal? AmountDr = 0;
                        decimal? Debit = 0;
                        decimal? Credit = 0;

                        Debit = item.Debit;
                        AmountDr = (Debit);
                        AmountDr = AmountDr != null ? AmountDr : 0;

                        Credit = item.Credit;
                        AmountCr = (Credit);
                        AmountCr = AmountCr != null ? AmountCr : 0;

                        if (item.Parent == 0)
                        {
                            if (item.AccType == "asset" && item.AccountsGroupID != 7 && item.Parent != 7)
                            {
                                if (Debit.Value > 0)
                                {
                                }
                                else
                                {
                                }
                                DTAmount += AmountDr;
                            }
                            if (item.AccType == "liability" && item.AccountsGroupID != 7 && item.Parent != 7)
                            {
                                if (Credit.Value > 0)
                                {
                                }
                                else
                                {
                                }
                                CTAmount += AmountCr;
                            }
                            if (item.AccountsGroupID == 7 || item.Parent == 7)
                            {
                            }

                        }
                        else
                        {
                            if (item.Parent == item.AccountsGroupID)
                            {
                            }
                            else
                            {


                            }

                            if (item.AccType == "asset" || item.AccType == "liability")
                            {
                                if (item.AccountsGroupID == 7 || item.Parent == 7)
                                { }
                                else
                                {
                                }

                            }
                        }

                        if (item.Parent == 7)
                        {
                            DTAmount += AmountDr;
                            CTAmount += AmountCr;
                        }

                        TotCount++;
                    }
                }
            }










            var diff = DTAmount - CTAmount;



            var rev = db.AccountsTransactions.Where(o => o.Account == 107162 && o.Purpose == "Opening Balance").FirstOrDefault();

            if (rev != null && diff != 0)
            {
                rev.Debit = rev.Debit - (decimal)diff;
                db.Entry(rev).State = EntityState.Modified;
                db.SaveChanges();

            }




            vmodel = new BalanceSheetViewModel();

            if (todate != "")
            {
                tdate = DateTime.ParseExact(todate, format, new CultureInfo("en-GB"));
                to = tdate;

            }
            too = DateTime.ParseExact(todate, format, new CultureInfo("en-GB"));
            vmodel.to = to;
            vmodel.from = from;
            ViewBag.TDte = todate;
            today = DateTime.Now;
            ViewBag.FDte = too.AddYears(-2);

            fun = 3;
            Count = 0;
            RetVal = 0;

            //capital acccount

            CapitalAcoount = Common.GetChildAccGroupTrial(1, "Capital Account", "liability", to, from);
            RetVal = getSubTotalTrial(Count, CapitalAcoount);//calculate child amounts
            RetVal = chkDebitCredit(CapitalAcoount);

            capparent = CapitalAcoount.Where(a => a.Parent == 0).ToList();//parent
            capchild = CapitalAcoount.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
            capacc = capparent.Union(capchild);

            //Current Asset
            CurrentAssets = Common.GetChildAccGroupTrial(2, "Current Assets", "asset", to, from);
            RetVal = getSubTotalTrial(Count, CurrentAssets);
            RetVal = chkDebitCredit(CurrentAssets);


            cuparent = CurrentAssets.Where(a => a.Parent == 0).ToList();//parent
            cuchild = CurrentAssets.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)); //child 

            //-------stock in hand---------
                // (Phase-3 cleanup 2026-06-12) removed a dead legacy block: a hardcoded "Stock-in-hand 2,783,344.19"
                // row was built here but its Union into the result was already commented out — never displayed.
            cuasset = cuparent.Union(cuchild);

            //current liabilities
            CurrentLiabilities = Common.GetChildAccGroupTrial(3, "Current Liabilities", "liability", to, from);
            RetVal = getSubTotalTrial(Count, CurrentLiabilities);
            RetVal = chkDebitCredit(CurrentLiabilities);

            clparent = CurrentLiabilities.Where(a => a.Parent == 0).ToList();//parent
            clchild = CurrentLiabilities.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
            crlib = clparent.Union(clchild);

            //Investments
            Investments = Common.GetChildAccGroupTrial(5, "Investments", "asset", to, from);
            RetVal = getSubTotalTrial(Count, Investments);
            RetVal = chkDebitCredit(Investments);

            invparent = Investments.Where(a => a.Parent == 0).ToList();//parent
            invchild = Investments.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
            invest = invparent.Union(invchild);

            //Fixed Asset
            FixedAssets = Common.GetChildAccGroupTrial(4, "Fixed Assets", "asset", to, from);
            RetVal = getSubTotalTrial(Count, FixedAssets);
            RetVal = chkDebitCredit(FixedAssets);

            fixparent = FixedAssets.Where(a => a.Parent == 0).ToList();//parent
            fixchild = FixedAssets.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
            fxasset = fixparent.Union(fixchild);

            //Loans (Liability)
            LoansLiability = Common.GetChildAccGroupTrial(6, "Loans (Liability)", "liability", to, from);
            RetVal = getSubTotalTrial(Count, LoansLiability);
            RetVal = chkDebitCredit(LoansLiability);

            llparent = LoansLiability.Where(a => a.Parent == 0).ToList();//parent
            llchild = LoansLiability.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
            loanlib = llparent.Union(llchild);


            //profit and loss account

            //            pfparent = pandf.Where(a => a.Parent == 0).ToList();//parent
            //          pfchild = pandf.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child


            //Revenue Accounts //stored procedure fun=2
            RevenueAccounts = Common.GetChildAccGroupTrial(7, "Revenue Accounts", "asset", to, from).ToList();


            RetVal = getTotalDrCr(Count, RevenueAccounts);
            RetVal = chkDebitCredit(RevenueAccounts);


            revparent = RevenueAccounts.Where(a => a.Parent == 0).ToList();//parent
            revchild = RevenueAccounts.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child

            revacc = revparent.Union(revchild);




            uni = capacc.Union(cuasset);
            uni = uni.Union(crlib);//p & f
            uni = uni.Union(invest);
            uni = uni.Union(fxasset);
            uni = uni.Union(loanlib);
            uni = uni.Union(revacc);


            ACCGroups = db.AccountsGroups.Where(a => a.Parent == 0 && a.Primary == 0 && a.Editable == 0).ToList();
            foreach (var acc in ACCGroups)
            {
                var GroupItem = Common.GetChildAccGroupTrial((int)acc.AccountsGroupID, acc.Name, "asset", to, from);
                decimal Amount = GroupItem != null ? (decimal)GroupItem.Sum(a => a.Debit - a.Credit) : 0;
                RetVal = getSubTotalTrialNew(Count, GroupItem);
                RetVal = chkDebitCredit(GroupItem);

                var GroupParent = GroupItem.Where(a => a.Parent == 0 && (a.Debit != 0 || a.Credit != 0)).ToList();
                var GroupChild = GroupItem.Where(a => a.Parent != 0 && (a.Debit != 0 || a.Credit != 0)).ToList();//child
                var GroupI = GroupParent.Union(GroupChild);
                if (Amount != 0)
                {
                    GroupI.ToList().ForEach(i => i.Temp = 1);
                    GroupI.ToList().ForEach(i => i.Parent = acc.AccountsGroupID);
                    uni = uni.Union(GroupI);
                }
            }



            trial = uni.Where(a => a.Temp != 0).ToList();


            count = getOrdering(trial);

















            //assign variable
            trialbalance = (from a in trial
                            select new
                            {
                                AccountsGroupID = a.AccountsGroupID,
                                Particular = a?.Particulars,
                                Debit = a?.Debit,
                                Credit = a?.Credit,
                                AccType = a.AccType,
                                Parent = a?.Parent,
                            });

            //change to view model
            vmodel.TrialBalanceDisplay = (from a in trialbalance
                                          select new TrialBalanceDisplay
                                          {
                                              AccountsGroupID = a.AccountsGroupID,
                                              Particular = a?.Particular,
                                              Debit = a?.Debit,
                                              Credit = a?.Credit,
                                              Parent = a?.Parent,
                                              AccType = a.AccType,
                                          }).ToList();

            companySet();
            return View(vmodel);
        }

        public int getSubTotalTrialNew(int increA, IList<BalanceSheet> Data)
        {
            decimal? TotalAmt = 0;
            decimal? subAmt = Data.Where(a => a.Parent == 0).Sum(a => a.Debit - a.Credit);
            foreach (var item in Data)
            {
                decimal? SubAmt = 0;
                decimal? SubTotalCr = 0;
                decimal? SubTotalDr = 0;

                if (item.Parent == 0)//parent 
                {
                    if (item.Debit > item.Credit)
                    {
                        SubAmt = Data.Sum(a => a.Debit - a.Credit);
                        SubAmt = SubAmt < 0 ? (0 - SubAmt) : SubAmt;
                        item.Debit = SubAmt;
                    }
                    else
                    {
                        SubAmt = Data.Sum(a => a.Credit - a.Debit);
                        SubAmt = SubAmt < 0 ? (0 - SubAmt) : SubAmt;
                        item.Credit = SubAmt;
                    }

                    item.Temp = increA + 1;
                    increA++;
                }
                var chk = Data.Where(a => a.AccountsGroupID == item.Parent).Select(a => a.Parent).FirstOrDefault();
                if (chk == 0) //and 2nd child
                {
                    var tempval = item.Temp;
                    SubTotalDr = BindAccounts.getAmountDr(Data, item.AccountsGroupID, 0);
                    SubTotalCr = BindAccounts.getAmountCr(Data, item.AccountsGroupID, 0);

                    if (item.Credit > item.Debit)
                    {
                        item.Credit = (item.Credit + SubTotalCr) - (item.Debit + SubTotalDr);
                        item.Debit = 0;//item.Debit + SubTotalDr;
                        TotalAmt += item.Credit;
                    }
                    else
                    {
                        item.Debit = (item.Debit + SubTotalDr) - (item.Credit + SubTotalCr);
                        item.Credit = 0;
                        TotalAmt += item.Debit;
                    }

                    if (item.Credit < 0)
                    {
                        item.Debit = item.Credit * -1;
                        item.Credit = 0;
                    }
                    if (item.Debit < 0)
                    {
                        item.Credit = item.Debit * -1;
                        item.Debit = 0;
                    }


                    item.Temp = increA + 1;
                    increA++;
                }
                if (item.Credit < item.Debit)
                {
                    item.Debit = item.Debit;
                }
                else
                {
                    item.Debit = item.Credit;
                }
            }

            return increA;

        }

        #endregion

        #region trial balance Acccount Wise
        [QkAuthorize(Roles = "Dev,TrialBalance")]
        public ActionResult TrialBalanceACC()
        {
            return View();
        }
        [QkAuthorize(Roles = "Dev,TrialBalance")]
        public ActionResult TrialBalanceACC2()
        {
            return View();
        }
        [QkAuthorize(Roles = "Dev,TrialBalance")]
        public ActionResult GetTrialBalanceAC(string fromdate, string todate)
        {
            String format = "dd-MM-yyyy";
            DateTime? tdate = null;
            DateTime? to = null;
            DateTime? from = null;
            DateTime? fromdt = null;
            if (todate != "")
            {
                tdate = DateTime.ParseExact(todate, format, new CultureInfo("en-GB"));
                to = tdate;
            }
            if (fromdate != "")
            {
                from = DateTime.ParseExact(fromdate, format, new CultureInfo("en-GB"));
                fromdt = from;
            }
            TrialBalanceAccViewModel vmodel = new TrialBalanceAccViewModel();
            vmodel.To = to;
            vmodel.From = fromdt;

            var Group = db.AccountsGroups.ToList();
            var Acc = db.Accountss.ToList().ToList();
            var Acctrans = db.AccountsTransactions.Where(a => a.Date <= to).ToList();

            List<AccountsGroup> newGroup = new List<AccountsGroup>();
            List<AccountsGroup> parentGroup = new List<AccountsGroup>();
            List<AccountsGroup> parentGroup2 = new List<AccountsGroup>();

            parentGroup = Group.Where(a => (a.Parent == 0)).ToList(); //First Parent
            foreach (var x in Group)
            {
                if (x.Parent != 0)
                {
                    var parent2 = Group.Where(a => (a.Parent == x.AccountsGroupID)).Select(a => a.Parent).FirstOrDefault();//Sub Parent
                    parentGroup2 = parentGroup2.Union(Group.Where(a => (a.AccountsGroupID == parent2))).ToList();
                }
            }
            parentGroup.AddRange(parentGroup2);
            foreach (var entry in parentGroup)
            {
                var groupItem = Group.Where(a => (a.AccountsGroupID == entry.AccountsGroupID) || (a.Parent == entry.AccountsGroupID)).ToList();
                foreach (var x in groupItem)
                {
                    var aId = Acc.Where(a => (a.Group == x.AccountsGroupID)).Select(a => a.AccountsID).ToList();
                    foreach (var acct in aId)
                    {
                        var id = Acctrans.Where(y => y.Account == acct).Select(a => a.Account).ToList();
                        if (id.Count != 0)
                        {
                            var childid = Acc.Where(a => a.Group == x.AccountsGroupID).Select(a => a.Group).FirstOrDefault();
                            newGroup = newGroup.Union(groupItem.Where(a => (a.AccountsGroupID == childid) || (a.Parent == 0) || (a.AccountsGroupID == entry.AccountsGroupID))).ToList();
                        }
                    }
                }
            }

            var GroupList = newGroup.Select(a => new TrialBalanceAcc
            {
                ID = a.AccountsGroupID,
                text = a.Name,
                Parent = a.Parent,
                Type = "Group",
                AccountId = null,
                Debit = null,
                Credit = null
            }).ToList();

            var AccList = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           where (a.Date <= to && a.Date >= fromdt)
                           select new TrialBalanceAcc
                           {
                               ID = null,
                               text = b.Name,
                               Parent = b.Group,
                               Type = "Account",
                               AccountId = b.AccountsID,
                               Debit = db.AccountsTransactions.Where(a => a.Account == b.AccountsID && a.Status == null && a.Date <= to && a.Date >= fromdt).Select(b => (decimal?)b.Debit).Sum(),
                               Credit = db.AccountsTransactions.Where(a => a.Account == b.AccountsID && a.Status == null && a.Date <= to && a.Date >= fromdt).Select(b => (decimal?)b.Credit).Sum(),
                           }).Distinct().ToList();


            var List = GroupList.Union(AccList).ToList();
            vmodel.Data = List;
            companySet();
            return View(vmodel);
        }
        [QkAuthorize(Roles = "Dev,TrialBalance")]
        public ActionResult GetTrialBalanceAC2(string fromdate, string todate)
        {
            String format = "dd-MM-yyyy";
            DateTime? tdate = null;
            DateTime? to = null;
            DateTime? from = null;
            DateTime? fromdt = null;
            if (todate != "")
            {
                tdate = DateTime.ParseExact(todate, format, new CultureInfo("en-GB"));
                to = tdate;
            }
            if (fromdate != "")
            {
                from = DateTime.ParseExact(fromdate, format, new CultureInfo("en-GB"));
                fromdt = from;
            }
            TrialBalanceAccViewModeltwo vmodel = new TrialBalanceAccViewModeltwo();
            vmodel.To = to;
            vmodel.From = fromdt;

            var Group = db.AccountsGroups.ToList();
            var Acc = db.Accountss.ToList().ToList();
            var Acctrans = db.AccountsTransactions.Where(a => a.Date <= to).ToList();

            List<AccountsGroup> newGroup = new List<AccountsGroup>();
            List<AccountsGroup> parentGroup = new List<AccountsGroup>();
            List<AccountsGroup> parentGroup2 = new List<AccountsGroup>();

            parentGroup = Group.Where(a => (a.Parent == 0)).ToList(); //First Parent
            foreach (var x in Group)
            {
                if (x.Parent != 0)
                {
                    var parent2 = Group.Where(a => (a.Parent == x.AccountsGroupID)).Select(a => a.Parent).FirstOrDefault();//Sub Parent
                    parentGroup2 = parentGroup2.Union(Group.Where(a => (a.AccountsGroupID == parent2))).ToList();
                }
            }
            parentGroup.AddRange(parentGroup2);
            foreach (var entry in parentGroup)
            {
                var groupItem = Group.Where(a => (a.AccountsGroupID == entry.AccountsGroupID) || (a.Parent == entry.AccountsGroupID)).ToList();
                foreach (var x in groupItem)
                {
                    var aId = Acc.Where(a => (a.Group == x.AccountsGroupID)).Select(a => a.AccountsID).ToList();
                    foreach (var acct in aId)
                    {
                        var id = Acctrans.Where(y => y.Account == acct).Select(a => a.Account).ToList();
                        if (id.Count != 0)
                        {
                            var childid = Acc.Where(a => a.Group == x.AccountsGroupID).Select(a => a.Group).FirstOrDefault();
                            newGroup = newGroup.Union(groupItem.Where(a => (a.AccountsGroupID == childid) || (a.Parent == 0) || (a.AccountsGroupID == entry.AccountsGroupID))).ToList();
                        }
                    }
                }
            }

            var GroupList = newGroup.Select(a => new TrialBalanceAccmodal2
            {
                ID = a.AccountsGroupID,
                text = a.Name,
                Parent = a.Parent,
                Type = "Group",
                AccountId = null,
                Debit = null,
                Credit = null,
                opening = 0,
                closing = 0
            }).ToList();

            var AccList = (from a in db.AccountsTransactions
                           join b in db.Accountss on a.Account equals b.AccountsID
                           where a.Date <= to
                           select new TrialBalanceAccmodal2
                           {
                               ID = null,
                               text = b.Name,
                               Parent = b.Group,
                               Type = "Account",
                               AccountId = b.AccountsID,
                               closing = db.AccountsTransactions.Where(o => o.Account == b.AccountsID && o.Status == null && o.Date <= to).Select(x => (decimal?)x.Debit - x.Credit).Sum(),
                               opening = db.AccountsTransactions.Where(o => o.Account == b.AccountsID && o.Status == null && o.Date < fromdt).Select(x => (decimal?)x.Debit - x.Credit).Sum(),
                               Debit = db.AccountsTransactions.Where(o => o.Account == b.AccountsID && o.Status == null && o.Date <= to && o.Date >= fromdt).Select(y => (decimal?)y.Debit).Sum(),
                               Credit = db.AccountsTransactions.Where(o => o.Account == b.AccountsID && o.Status == null && o.Date <= to && o.Date >= fromdt).Select(y => (decimal?)y.Credit).Sum(),
                           }).Distinct().ToList();


            var List = GroupList.Union(AccList).ToList();
            vmodel.Data = List;
            companySet();
            return View(vmodel);
        }

        public ActionResult Tree()
        {
            TrialBalanceAccViewModel vmodel = new TrialBalanceAccViewModel();
            var Group = db.AccountsGroups.ToList();
            var Acc = db.Accountss.ToList();
            var GroupList = Group.Select(a => new TrialBalanceAcc
            {
                ID = a.AccountsGroupID,
                text = a.Name,
                Parent = a.Parent,
                Type = "Group",
                AccountId = null,
                Debit = null,
                Credit = null
            }).ToList();
            var AccList = Acc.Select(a => new TrialBalanceAcc
            {
                ID = null,
                text = a.Name,
                Parent = a.Group,
                Type = "Account",
                AccountId = a.AccountsID,
                Debit = db.AccountsTransactions.Where(b => b.Account == a.AccountsID && b.Status == null).Select(b => (decimal?)b.Debit).Sum(),
                Credit = db.AccountsTransactions.Where(b => b.Account == a.AccountsID && b.Status == null).Select(b => (decimal?)b.Credit).Sum(),
            }).ToList();
            var List = GroupList.Union(AccList).ToList();
            vmodel.Data = List;
            return View(vmodel);
        }
        #endregion

    }
}
