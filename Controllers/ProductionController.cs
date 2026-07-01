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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class ProductionController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ProductionController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: Production
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Production List")]
        public ActionResult Index()
        {
            ViewBag.BomName = QkSelect.List(
             new List<SelectListItem>
             {
               new SelectListItem { Selected = false, Text = "All", Value = "0"},
             }, "Value", "Text", 1);


            ViewBag.ItemName = QkSelect.List(
            new List<SelectListItem>
            {
               new SelectListItem { Selected = false, Text = "All", Value = "0"},
            }, "Value", "Text", 1);
            var created = db.Users.Select(s => new
            {
                ID = s.Id,
                Name = s.UserName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");

            var MlaProd = db.EnableSettings.Where(a => a.EnableType == "MLAProd").FirstOrDefault();
            var MlaProds = MlaProd != null ? MlaProd.Status : Status.inactive;
            ViewBag.MLAProd = MlaProds;

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");
            _FinancialYear();

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindProd").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

            return View();
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Production List")]
        public ActionResult GetProduction(string BillNo, string FromDate, string ToDate, long? BomName, long? ItemName, string user, string appstat)
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

            string search = Request.Form.GetValues("search[value]")[0];
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

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
            var userpermission = User.IsInRole("All Production Entry");
            var UserId = User.Identity.GetUserId();
            var MCList1 = db.MCs.Where(a => a.AssignedUser == UserId).Select(a => (long?)a.MCId).ToList();
            var MCList = MCList1;
            if (!MCList.Any())
            {
                MCList = db.MCs.Select(a => (long?)a.MCId).ToList();
            }
            var MCArray = MCList.ToArray();

            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var uDev = User.IsInRole("Dev");
            var uProductionView = User.IsInRole("View Production");
            var uEdit = User.IsInRole("Edit Production");
            var uDelete = User.IsInRole("Delete Production");

            var serverRows = (from a in db.Productions
                     join z in db.GeneratedItem on a.ProductionId equals z.Production into con
                     from z in con.DefaultIfEmpty()
                     join b in db.BillOfMaterials on z.BOM equals b.BOMId into bom
                     from b in bom.DefaultIfEmpty()
                     join c in db.Items on z.Item equals c.ItemID into item
                     from c in item.DefaultIfEmpty()
                     join d in db.ItemUnits on z.Unit equals d.ItemUnitID into unit
                     from d in unit.DefaultIfEmpty()
                     join e in db.Users on a.CreatedBy equals e.Id
                     join f in db.MCs on a.MaterialCenter equals f.MCId
                     join g in db.Branchs on a.Branch equals g.BranchID
                     //let mc = db.MCs.Where(x => x.AssignedUser == a.CreatedBy).Select(x => x.MCId).FirstOrDefault()
                     // app/AppStatus/chkAppStatus (nested collections + GroupBy-latest) can't be translated by
                     // EF Core 10 inside this projection — computed CLIENT-side after materialization via lookups
                     // keyed by ProductionId (same split as QuotationController.GetQuotation / EstimateController.GetEstimate).
                     where (BillNo == null || BillNo == "" || a.VoucherNo == BillNo) &&
                     (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(a.PEDate, fdate) <= 0) &&
                     (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(a.PEDate, tdate) >= 0) &&
                     (BomName == 0 || z.BOM == BomName) &&
                     (ItemName == 0 || z.Item == ItemName)
                     //&& (mc == 0 || mc == a.MaterialCenter)
                     && (!MCList.Any() || MCArray.Contains(a.MaterialCenter))
                     && (userpermission == true || a.CreatedBy == UserId) && (user == null || user == "" || e.Id == user)
                     select new
                     {
                         a.ProductionId,
                         a.VoucherNo,
                         a.PEDate,
                         b.BOMName,
                         c.ItemName,
                         z.Qty,
                         d.ItemUnitName,
                         z.Expense,
                         z.Price,
                         z.Amount,
                         e.UserName,
                         a.Note,
                         Dev = uDev,
                         MC = f.MCName,
                         Total = (db.GeneratedItem.Where(x => x.Production == z.Production).Select(y => y.Amount).Sum()),
                         Details = uProductionView,
                         Edit = uEdit,
                         Delete = uDelete,
                         g.BranchName,

                     }).GroupBy(x => x.ProductionId).Select(x => x.FirstOrDefault()).ToList();

            // CLIENT-side approval lookups keyed by ProductionId (missing key -> empty/absent, no KeyNotFound).
            var prodIds = serverRows.Select(o => o.ProductionId).ToList();
            // app = approver EmployeeIds for the production (keyed by TransEntry == ProductionId).
            var appLookup = db.Approvals
                .Where(a => a.Type == "Production" && prodIds.Contains(a.TransEntry))
                .Select(a => new { a.TransEntry, a.EmployeeId })
                .ToList()
                .ToLookup(a => a.TransEntry);
            // AppStatus = all ApprovalUpdate statuses; raw rows materialized once and reused for chkAppStatus.
            var appUpdRows = db.ApprovalUpdates
                .Where(a => a.Type == "Production" && prodIds.Contains(a.TransEntry))
                .Select(a => new { a.TransEntry, a.ApprovalStatus, a.ApprovedBy, a.CreatedDate })
                .ToList();
            var appStatusLookup = appUpdRows.ToLookup(a => a.TransEntry);
            // chkAppStatus = latest ApprovalUpdate status per ApprovedBy (GroupBy-latest-per-group), per production.
            var chkAppStatusLookup = appUpdRows
                .GroupBy(a => a.TransEntry)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(l => l.ApprovedBy)
                    .Select(grp => grp.OrderByDescending(c => c.CreatedDate).First())
                    .Select(a => a.ApprovalStatus).ToList());

            var v = serverRows.Select(o =>
                     {
                         var app = appLookup[o.ProductionId].Select(x => x.EmployeeId).ToList();
                         var AppStatus = appStatusLookup[o.ProductionId].Select(x => x.ApprovalStatus).ToList();
                         var chkAppStatus = chkAppStatusLookup.TryGetValue(o.ProductionId, out var ck) ? ck : new List<ApprovalStatus>();
                         return new
                     {
                         o.ProductionId,
                         o.VoucherNo,
                         o.PEDate,
                         o.BOMName,
                         o.ItemName,
                         o.Qty,
                         o.ItemUnitName,
                         o.Expense,
                         o.Price,
                         o.Amount,
                         o.UserName,
                         o.Note,
                         o.Dev,
                         o.MC,
                         o.Total,
                         o.Details,
                         o.Edit,
                         o.Delete,
                         o.BranchName,
                         app = app,
                         Approval = (app != null && empl.EmployeeId != null) ? (app.Contains(empl.EmployeeId) ? true : false) : false,
                         ApprovalStatus = (app.Count > 0 && AppStatus.Count > 0 && chkAppStatus.Count > 0) ? (chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (app != null && chkAppStatus != null && app.Count != 0 && chkAppStatus.Count != 0 && chkAppStatus.Count() == app.Count() ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,

                     };
                     });

            if (appstat != "")
            {
                v = v.Where(a => a.ApprovalStatus == AppSt);
            }
            else
            {
                v = v.GroupBy(x => x.VoucherNo).Select(z => z.FirstOrDefault());
            }
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.VoucherNo.ToString().ToLower().Contains(search.ToLower()));
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



        // GET: Production/Create
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Create Production")]
        public ActionResult Create()
        {
            var ventry = new ProductionViewModel
            {
                VoucherNo = VoucherNo(),
                PEDate = (System.DateTime.Now).ToString("dd-MM-yyyy"),
            };

            ViewBag.bom = QkSelect.List(
                      new List<SelectListItem>
                      {
                                    new SelectListItem { Selected = false, Text = "", Value = "0"},
                      }, "Value", "Text", 1);

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

            var proj = db.Projects
                      .Select(s => new
                      {
                          ID = s.ProjectId,
                          Name = s.ProCode + " " + s.ProjectName
                      })
                      .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");
            var tsk = db.ProTasks
              .Select(s => new
              {
                  ID = s.ProTaskId,
                  Name = s.TaskName
              })
              .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

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
            }
            else
            {
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
            }
            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            _FinancialYear();
            companySet();
            var userpermission = User.IsInRole("All Production Entry");
            ViewBag.LastEntry = db.Productions.Where(p => (userpermission == true || p.CreatedBy == UserId)).Select(p => p.ProductionId).AsEnumerable().DefaultIfEmpty(0).Max();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                 .Select(s => new
                 {
                     ID = s.EmployeeId,
                     Name = s.FirstName + " " + s.LastName
                 })
                 .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var MlaProd = db.EnableSettings.Where(a => a.EnableType == "MLAProd").FirstOrDefault();
            var MlaProds = MlaProd != null ? MlaProd.Status : Status.inactive;
            ViewBag.MLAProd = MlaProds;

            //field mapping
            ventry.FieldMap = db.FieldMappings.Where(a => a.Section == "Production" && a.Status == Status.active).ToList();

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var ItemOutOfStocks = db.EnableSettings.Where(a => a.EnableType == "ItemOutOfStock").FirstOrDefault();
            var ItemOutOfStock = ItemOutOfStocks != null ? ItemOutOfStocks.Status : Status.inactive;
            ViewBag.ItemOutOfStock = ItemOutOfStock;

            return View(ventry);
        }


        [QkAuthorize(Roles = "Dev,Create Production")]
        public bool Createfast(long bomid)
        {
            
                bool stat = false;
                string msg;
              
                var UserId = User.Identity.GetUserId();
                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                var today = Convert.ToDateTime(System.DateTime.Now);

                var Date = today;

                long Branch = 0;

                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                

                var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
                var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;

                long MC = 0;
            
                    MC = 1;
                Int64 PrNom = GetVchNo();
                string voucher = PrNom.ToString();
                string Note ="";
                long MaterialCenter = MC;
                Int64 proId = 0;


                Production pro = new Production
                {
                    VoucherNo = voucher,
                    PrNo = PrNom,
                    PEDate = Date,
                    Note = "",
                    MaterialCenter = MC,
                    CreatedDate = today,
                    CreatedBy = UserId,
                    Status = Status.active,
                    Branch = Branch,
                   
                };
                db.Productions.Add(pro);
                db.SaveChanges();
                proId = pro.ProductionId;

                //To Update the quantity in Create Mode(ItemTransaction Table)
                var bomitem = db.BillOfMaterials.Find(bomid).ItemId;
               
                    long? BOM = bomid;

                var it = db.Items.Find(bomitem);


                            GeneratedItems con = new GeneratedItems
                            {
                                Production = proId,
                                BOM = bomid,
                                Item = bomitem,
                                Qty = 1,
                                Unit = it.ItemUnitID,
                                Expense = 0,
                                Price = it.SellingPrice,
                                Amount = it.SellingPrice,
                            };
                            db.GeneratedItem.Add(con);
                            db.SaveChanges();
                var proitem = db.BOMItems.Where(o => o.BOMId == bomid).ToList();
                        foreach (var arrItem in proitem)
                        {
                            if (BOM == arrItem.BOMId)
                            {
                        var itt = db.Items.Find(arrItem.ItemId);

                        ProItem prItem = new ProItem();
                                {
                                    prItem.Production = proId;
                                    prItem.ItemId = arrItem.ItemId;
                                    prItem.Unit = arrItem.Unit;
                                    prItem.Quantity = arrItem.Quantity;
                                    prItem.PPrice = it.SellingPrice;
                                    prItem.PAmount = it.SellingPrice*arrItem.Quantity;
                                    db.ProItems.Add(prItem);
                                    db.SaveChanges();
                                }

                              
                            }
                        }
                        BOM = null;
        
                
             
                com.addlog(LogTypes.Created, UserId, "Production", "Productions", findip(), proId, "Successfully Submitted Productions");





                return true;
            

        }


        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Create Production")]
        public ActionResult Create(ProdViewModel vmodel)
        {
            try
            {
                bool stat = false;
                string msg;
                if (BillExist(vmodel.prodata.VoucherNo))
                {
                    msg = "Voucher No. Already Exists.";
                    stat = false;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                var UserId = User.Identity.GetUserId();
                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                var today = Convert.ToDateTime(System.DateTime.Now);

                var Date = DateTime.Parse(vmodel.PEDate.ToString(), new CultureInfo("en-GB"));

                long Branch = 0;

                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                if (BranchCheck == Status.active)
                {
                    Branch = Convert.ToInt64(vmodel.prodata.Branch);
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
                    MC = Convert.ToInt64(vmodel.prodata.MaterialCenter);
                }
                else
                {
                    MC = 1;
                }
                string voucher = vmodel.prodata.VoucherNo;
                string Note = vmodel.prodata.Note;
                long MaterialCenter = Convert.ToInt64(vmodel.prodata.MaterialCenter);
                Int64 proId = 0;
                Int64 PrNom = GetVchNo();

                Production pro = new Production
                {
                    VoucherNo = voucher,
                    PrNo = PrNom,
                    PEDate = Date,
                    Note = vmodel.prodata.Note,
                    MaterialCenter = MC,
                    Productioncost = vmodel.prodata.Productioncost,
                    meterialcost =vmodel.prodata.meterialcost,
                    Labourcost=vmodel.prodata.Labourcost,
                    CreatedDate = today,
                    CreatedBy = UserId,
                    Status = Status.active,
                    Branch = Branch,
                    Ref1 = vmodel.Ref1,
                    Ref2 = vmodel.Ref2,
                    Ref3 = vmodel.Ref3,
                    Ref4 = vmodel.Ref4,
                    Ref5 = vmodel.Ref5,
                    Project = vmodel.Project,
                    ProTask = vmodel.ProTask,
                };
                db.Productions.Add(pro);
                db.SaveChanges();
                proId = pro.ProductionId;

                //To Update the quantity in Create Mode(ItemTransaction Table)
                ItemTransaction("Create", MC, vmodel, UserId, today, 0);

                foreach (var arr in vmodel.bom)
                {
                    long? BOM = arr.BOM_Id;


                    foreach (var arrGen in vmodel.progenerated.ToList())
                    {
                        if (BOM == arrGen.BOMId)
                        {
                            GeneratedItems con = new GeneratedItems
                            {
                                Production = proId,
                                BOM = (long)arrGen.BOMId,
                                Item = arrGen.ItemId,
                                Qty = arrGen.Quantity,
                                Unit = Convert.ToInt64(arrGen.ItemUnit),
                                Expense = (decimal)arrGen.Expense,
                                Price = arrGen.Price,
                                Amount = arrGen.Amount,
                            };
                            db.GeneratedItem.Add(con);
                            db.SaveChanges();
                            var it = db.Items.Find(arrGen.ItemId);
                            it.PurchasePrice = (decimal)arrGen.Expense;
                            it.SellingPrice = (decimal)arrGen.Expense;
                            it.cashprice = (decimal)arrGen.Expense;
                            it.creditprice = (decimal)arrGen.Expense;
                            it.BasePrice = arrGen.Expense;
                            it.MRP = arrGen.Expense;
                            db.Entry(it).State = EntityState.Modified;
                            db.SaveChanges();
                            vmodel.progenerated.Remove(arrGen);
                        }
                        foreach (var arrItem in vmodel.proitem.ToList())
                        {
                            if (BOM == arrItem.BOM)
                            {
                                ProItem prItem = new ProItem();
                                {
                                    prItem.Production = proId;
                                    prItem.ItemId = arrItem.ItemId;
                                    prItem.Unit = arrItem.Unit;
                                    prItem.Quantity = arrItem.Quantity;
                                    prItem.PPrice = arrItem.PPrice;
                                    prItem.PAmount = arrItem.PAmount;
                                    db.ProItems.Add(prItem);
                                    db.SaveChanges();
                                }                              

                                vmodel.proitem.Remove(arrItem);
                            }
                        }
                        BOM = null;
                    }                    
                }               

                ////com.addStock(vmodel.prodata.Qty, 0, vmodel.prodata.Item, vmodel.prodata.Unit, "Produced-Item", proId, vmodel.prodata.Price, StockVal, null, vmodel.prodata.MaterialCenter, Status.active);


                //Approved By
                var Appby = vmodel.ApprovedBy;
                if (Appby != null && Appby != "")
                {
                    long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                    Approval approval = new Approval();
                    foreach (var emp in Approve)
                    {
                        approval.TransEntry = proId;
                        approval.Type = "Production";
                        approval.EmployeeId = emp;
                        db.Approvals.Add(approval);
                        db.SaveChanges();
                    }
                }
                com.addlog(LogTypes.Created, UserId, "Production", "Productions", findip(), proId, "Successfully Submitted Productions");
                if (Convert.ToString(vmodel.fnval) == "print")
                {
                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";
                    var prodata = vmodel.prodata;
                    var array = vmodel.bom.Select(x => x.BOM_Id).ToArray();
                    var BOM = (from a in db.BillOfMaterials
                               where array.Contains(a.BOMId)
                               select new
                               {
                                   BOMName = a.BOMName
                               });
                    var Data = (from a in db.Productions
                                join b in db.MCs on a.MaterialCenter equals b.MCId into item
                                from b in item.DefaultIfEmpty()
                                join c in db.Branchs on a.Branch equals c.BranchID into Br
                                from c in Br.DefaultIfEmpty()
                                join p in db.Projects on a.Project equals p.ProjectId into prjct
                                from p in prjct.DefaultIfEmpty()
                                join t in db.ProTasks on a.Project equals t.ProjectId into ptask
                                from t in ptask.DefaultIfEmpty()
                                where a.VoucherNo == vmodel.prodata.VoucherNo
                                select new
                                {
                                    a.PEDate,
                                    b.MCName,
                                    c.BranchName,
                                    a.Ref1,
                                    a.Ref2,
                                    a.Ref3,
                                    a.Ref4,
                                    a.Ref5,
                                    a.Project,
                                    a.ProTask,
                                    ProjectName = p.ProCode + "-" + p.ProjectName,
                                    TaskName = t.TaskCode + "-" + t.TaskName
                                }).FirstOrDefault();
                    var progen = (from a in db.GeneratedItem
                                  join z in db.Productions on a.Production equals z.ProductionId
                                  join b in db.Items on a.Item equals b.ItemID into item
                                  from b in item.DefaultIfEmpty()
                                  join c in db.ItemUnits on a.Unit equals c.ItemUnitID into unit
                                  from c in unit.DefaultIfEmpty()
                                  where z.VoucherNo == vmodel.prodata.VoucherNo
                                  select new
                                  {
                                      Item = b.ItemName,
                                      Unit = c.ItemUnitName,
                                      Qty = a.Qty,
                                      Expense = a.Expense,
                                      Price = a.Price,
                                      Amount = a.Amount
                                  });
                    var GenTotal = progen.Select(x => x.Amount).Sum();
                    var GenQty = progen.Select(x => x.Qty).Sum();
                    var proitems = (from a in db.ProItems
                                    join z in db.Productions on a.Production equals z.ProductionId into items
                                    from z in items.DefaultIfEmpty()
                                    join b in db.Items on a.ItemId equals b.ItemID into item
                                    from b in item.DefaultIfEmpty()
                                    join c in db.ItemUnits on a.Unit equals c.ItemUnitID into unit
                                    from c in unit.DefaultIfEmpty()
                                    where z.VoucherNo == vmodel.prodata.VoucherNo
                                    select new
                                    {
                                        Item = b.ItemName,
                                        Unit = c.ItemUnitName,
                                        Qty = a.Quantity,
                                        Price = a.PPrice,
                                        Amount = a.PAmount
                                    });
                    var ItemTotal = proitems.Select(x => x.Amount).Sum();
                    var ItemQty = proitems.Select(x => x.Qty).Sum();

                    var arr = new ArrayList();
                    arr.Add(GenQty);
                    arr.Add(GenTotal);
                    arr.Add(ItemQty);
                    arr.Add(ItemTotal);
                    arr.Add(BOM);

                    var fmapp = db.FieldMappings.Where(a => a.Section == "Production" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                    msg = "Successfully Added Production .";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Data, prodata, BOM, progen, proitems, arr, fmapp = fmapp, ComHeadCheck } };
                }
                else
                {
                    msg = "Successfully Added Production .";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }

            }
            catch
            {
                return View();
            }

        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Production")]
        [HttpGet]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userpermission = User.IsInRole("All Production Entry");
            var UserId = User.Identity.GetUserId();
            Production pro = db.Productions.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.ProductionId == id).FirstOrDefault();

            if (pro == null)
            {
                return NotFound();
            }

            var proj = db.Projects
                          .Select(s => new
                          {
                              ID = s.ProjectId,
                              Name = s.ProCode + " " + s.ProjectName
                          })
                          .ToList();
            ViewBag.getProj = QkSelect.List(proj, "ID", "Name");
            var tsk = db.ProTasks
              .Select(s => new
              {
                  ID = s.ProTaskId,
                  Name = s.TaskName
              })
              .ToList();
            ViewBag.getProTask = QkSelect.List(tsk, "ID", "Name");

            ProductionViewModel vmodel = new ProductionViewModel();
            vmodel.ProductionId = (long)id;
            vmodel.VoucherNo = pro.VoucherNo;
            vmodel.PEDate = pro.PEDate.ToString("dd-MM-yyyy");
            vmodel.Productioncost = pro.Productioncost;
            vmodel.meterialcost = pro.meterialcost;
            vmodel.Labourcost = pro.Labourcost;

            vmodel.Note = pro.Note;
            vmodel.Branch = pro.Branch;
            vmodel.MaterialCenter = pro.MaterialCenter;
            vmodel.Ref1 = pro.Ref1;
            vmodel.Ref2 = pro.Ref2;
            vmodel.Ref3 = pro.Ref3;
            vmodel.Ref4 = pro.Ref4;
            vmodel.Ref5 = pro.Ref5;
            vmodel.Project = pro.Project;
            vmodel.ProTask = pro.ProTask;
            //    ID = s.BOMId,
            //    Name = s.BOMName

            //    ID = s.ItemID,
            //    Name = s.ItemCode + " - " + s.ItemName

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

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            if (mcchk != null)
            {
                var mcs = db.MCs.Where(s => s.AssignedUser == UserId).Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
            }
            else
            {
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
            }

            _FinancialYear();
            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
            ViewBag.preEntry = db.Productions.Where(a => a.ProductionId < id && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.ProductionId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.Productions.Where(a => a.ProductionId > id && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.ProductionId).DefaultIfEmpty().Min();

            companySet();

            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "Production").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaProd = db.EnableSettings.Where(a => a.EnableType == "MLAProd").FirstOrDefault();
            var MlaProds = MlaProd != null ? MlaProd.Status : Status.inactive;
            ViewBag.MLAProd = MlaProds;

            var EditPermission = User.IsInRole("Disable Production Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "Production", UserId);

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Production" && a.Status == Status.active).ToList();

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

            var ItemOutOfStocks = db.EnableSettings.Where(a => a.EnableType == "ItemOutOfStock").FirstOrDefault();
            var ItemOutOfStock = ItemOutOfStocks != null ? ItemOutOfStocks.Status : Status.inactive;
            ViewBag.ItemOutOfStock = ItemOutOfStock;

            return View(vmodel);
        }

        // POST: Production/Edit/5
        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Production")]
        public ActionResult Edit(ProdViewModel vmodel, long? id)
        {
            try
            {
                bool stat = false;
                string msg;
                var UserId = User.Identity.GetUserId();

                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
                long Branch = 0;
                if (BranchCheck == Status.active)
                {
                    Branch = vmodel.prodata.Branch;
                }
                else
                {
                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                }

                var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
                var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
                var Date = DateTime.Parse(vmodel.PEDate.ToString(), new CultureInfo("en-GB"));
                var today = Convert.ToDateTime(System.DateTime.Now);
                long MC = 0;
                if (MCcheck == Status.active)
                {
                    MC = Convert.ToInt64(vmodel.prodata.MaterialCenter);
                }
                else
                {
                    MC = 1;
                }

                var EditPermission = User.IsInRole("Disable Production Edit After Approval");
                if (com.chkApproved((long)id, EditPermission, "Production", UserId) == true)
                {
                    string voucher = vmodel.prodata.VoucherNo;
                    string Note = vmodel.prodata.Note;
                    long MaterialCenter = Convert.ToInt64(vmodel.prodata.MaterialCenter);
                    Production pro = db.Productions.Find(id);

                    if (BillExist(vmodel.prodata.VoucherNo) && vmodel.prodata.VoucherNo != pro.VoucherNo)
                    {
                        msg = "Voucher No. Already Exists.";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }

                    pro.VoucherNo = vmodel.prodata.VoucherNo;
                    pro.PEDate = DateTime.Parse(vmodel.PEDate.ToString(), new CultureInfo("en-GB"));
                    pro.Note = vmodel.prodata.Note;
                    pro.Branch = Branch;
                    pro.MaterialCenter = MC;
                    pro.Productioncost = vmodel.prodata.Productioncost;
                  pro.meterialcost = vmodel.prodata.meterialcost ;
                    pro.Labourcost = vmodel.prodata.Labourcost;
                    pro.Ref1 = vmodel.Ref1;
                    pro.Ref2 = vmodel.Ref2;
                    pro.Ref3 = vmodel.Ref3;
                    pro.Ref4 = vmodel.Ref4;
                    pro.Ref5 = vmodel.Ref5;

                    pro.Project = vmodel.Project;
                    pro.ProTask = vmodel.ProTask;

                    db.Entry(pro).State = EntityState.Modified;

                    //To Update the quantity in Edit Mode(ItemTransaction Table)
                    ItemTransaction("Edit", MC, vmodel, UserId, today, id);

                    var procons = db.GeneratedItem.Where(a => a.Production == id).FirstOrDefault();
                    if (procons.Production != 0)
                    {
                        var proItem = db.ProItems.Where(a => a.Production == procons.Production);
                        if (proItem != null)
                        {
                            db.ProItems.RemoveRange(db.ProItems.Where(a => a.Production == id));
                        }
                        db.GeneratedItem.RemoveRange(db.GeneratedItem.Where(a => a.Production == id));

                    }

                    foreach (var arr in vmodel.bom)
                    {
                        long? BOM = arr.BOM_Id;

                        foreach (var arrGen in vmodel.progenerated.ToList())
                        {
                            if (BOM == arrGen.BOMId)
                            {
                                GeneratedItems con = new GeneratedItems();
                                {
                                    con.Unit = Convert.ToInt64(arrGen.ItemUnit);
                                    con.Production = (long)id;
                                    con.BOM = (long)BOM;
                                    con.Item = arrGen.ItemId;
                                    con.Qty = arrGen.Quantity;
                                    con.Expense = (decimal)arrGen.Expense;
                                    con.Price = arrGen.Price;
                                    con.Amount = arrGen.Amount;
                                };
                                db.GeneratedItem.Add(con);
                                db.SaveChanges();
                                var it = db.Items.Find(arrGen.ItemId);
                                it.PurchasePrice = (decimal)arrGen.Expense;
                                it.SellingPrice = (decimal)arrGen.Expense;
                                it.cashprice = (decimal)arrGen.Expense;
                                it.creditprice = (decimal)arrGen.Expense;
                                it.BasePrice = arrGen.Expense;
                                it.MRP = arrGen.Expense;
                                vmodel.progenerated.Remove(arrGen);
                            }
                            foreach (var arrItem in vmodel.proitem.ToList())
                            {
                                if (BOM == arrItem.BOM)
                                {
                                    ProItem prItem = new ProItem();

                                    prItem.Production = (long)id;
                                    prItem.ItemId = arrItem.ItemId;
                                    prItem.Unit = arrItem.Unit;
                                    prItem.Quantity = arrItem.Quantity;
                                    prItem.PPrice = arrItem.PPrice;
                                    prItem.PAmount = arrItem.PAmount;
                                    db.ProItems.Add(prItem);
                                    db.SaveChanges();
                                    vmodel.proitem.Remove(arrItem);
                                }
                            }
                            BOM = null;
                        }
                    }
                    //Approved By
                    var empuser = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                    var chkapp = db.Approvals.Where(a => a.EmployeeId == empuser && a.TransEntry == (long)id && a.Type == "Production").FirstOrDefault();
                    var MrnPO = db.Approvals.Where(a => a.TransEntry == (long)id && a.Type == "Production").FirstOrDefault();
                    if (MrnPO != null)
                    {
                        if (chkapp != null)
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.EmployeeId != empuser && a.TransEntry == (long)id && a.Type == "Production"));
                            db.SaveChanges();
                        }
                        else
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == (long)id && a.Type == "Production"));
                            db.SaveChanges();
                        }
                    }
                    var Appby = vmodel.ApprovedBy;
                    if (Appby != null && Appby != "")
                    {
                        long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                        Approval approval = new Approval();
                        foreach (var emp in Approve)
                        {
                            approval.TransEntry = (long)id;
                            approval.Type = "Production";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }
                    com.addlog(LogTypes.Updated, UserId, "Production", "Productions", findip(), (long)id, "Successfully Updated Production");
                }
                if (Convert.ToString(vmodel.fnval) == "updateandprint")
                {
                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";
                    var prodata = vmodel.prodata;
                    var array = vmodel.bom.Select(x => x.BOM_Id).ToArray();
                    var BOM = (from a in db.BillOfMaterials
                               where array.Contains(a.BOMId)
                               select new
                               {
                                   BOMName = a.BOMName
                               });
                    var progen = (from a in db.GeneratedItem
                                  join z in db.Productions on a.Production equals z.ProductionId into con
                                  from z in con.DefaultIfEmpty()
                                  join b in db.Items on a.Item equals b.ItemID into item
                                  from b in item.DefaultIfEmpty()
                                  join c in db.ItemUnits on a.Unit equals c.ItemUnitID into unit
                                  from c in unit.DefaultIfEmpty()
                                  where z.VoucherNo == vmodel.prodata.VoucherNo
                                  select new
                                  {
                                      Item = b.ItemName,
                                      Unit = c.ItemUnitName,
                                      Qty = a.Qty,
                                      Expense = a.Expense,
                                      Price = a.Price,
                                      Amount = a.Amount,
                                  });
                    var GenTotal = progen.Select(x => x.Amount).Sum();
                    var GenQty = progen.Select(x => x.Qty).Sum();
                    var Data = (from a in db.Productions
                                join b in db.MCs on a.MaterialCenter equals b.MCId into item
                                from b in item.DefaultIfEmpty()
                                join c in db.Branchs on a.Branch equals c.BranchID into Br
                                from c in Br.DefaultIfEmpty()
                                join p in db.Projects on a.Project equals p.ProjectId into prjct
                                from p in prjct.DefaultIfEmpty()
                                join t in db.ProTasks on a.Project equals t.ProjectId into ptask
                                from t in ptask.DefaultIfEmpty()
                                where a.VoucherNo == vmodel.prodata.VoucherNo
                                select new
                                {
                                    a.PEDate,
                                    b.MCName,
                                    c.BranchName,
                                    a.Ref1,
                                    a.Ref2,
                                    a.Ref3,
                                    a.Ref4,
                                    a.Ref5,
                                    a.Project,
                                    a.ProTask,
                                    ProjectName = p.ProCode + "-" + p.ProjectName,
                                    TaskName = t.TaskCode + "-" + t.TaskName
                                }).FirstOrDefault();
                    var proitems = (from a in db.ProItems
                                    join z in db.Productions on a.Production equals z.ProductionId into items
                                    from z in items.DefaultIfEmpty()
                                    join b in db.Items on a.ItemId equals b.ItemID into item
                                    from b in item.DefaultIfEmpty()
                                    join c in db.ItemUnits on a.Unit equals c.ItemUnitID into unit
                                    from c in unit.DefaultIfEmpty()
                                    where z.VoucherNo == vmodel.prodata.VoucherNo
                                    select new
                                    {
                                        Item = b.ItemName,
                                        Unit = c.ItemUnitName,
                                        Qty = a.Quantity,
                                        Price = a.PPrice,
                                        Amount = a.PAmount
                                    });
                    var ItemTotal = proitems.Select(x => x.Amount).Sum();
                    var ItemQty = proitems.Select(x => x.Qty).Sum();
                    var arr = new ArrayList();
                    arr.Add(GenQty);
                    arr.Add(GenTotal);
                    arr.Add(ItemQty);
                    arr.Add(ItemTotal);
                    arr.Add(BOM);

                    var fmapp = db.FieldMappings.Where(a => a.Section == "Production" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                    msg = "Successfully Updated Production .";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Data, prodata, BOM, progen, proitems, arr, fmapp = fmapp, ComHeadCheck } };
                }
                else
                {
                    msg = "Successfully Updated Production.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
            }
            catch (Exception e)
            {
                var msg = "Error Production." + e.InnerException.Message;
                var stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }

        // GET: Production/Delete/5
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete Production")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Delete
            var userpermission = User.IsInRole("All Production Entry");
            var UserId = User.Identity.GetUserId();
            Production pro = db.Productions.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.ProductionId == id).FirstOrDefault();

            if (pro == null)
            {
                return NotFound();
            }
            return PartialView(pro);
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete Production")]
        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("Delete")]
        public ActionResult Delete(long id)
        {
            try
            {
                bool stat = false;
                string msg;
                var Msg = chkDeleteWithMsg(id);
                if (Msg != null)
                {
                    msg = Msg;
                    stat = false;
                }
                else
                {
                    stat = DeleteFn(id);
                    msg = "Successfully deleted Production.";
                }
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            catch
            {
                return View();
            }
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Production")]
        public ActionResult DeleteAllProduction(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeletePro(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + " Production.", true);
            return RedirectToAction("Index", "Production");
        }
        private Boolean DeletePro(long Id)
        {
            var Msg = chkDeleteWithMsg(Id);
            if (Msg != null)
            {
                return false;
            }
            else
            {
                return DeleteFn(Id);
            }
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            return msg;
        }
        public bool DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            var CurrentDate = Convert.ToDateTime(System.DateTime.Now);
            
            Production pro = db.Productions.Find(id);

            //To Update the quantity in Delete Mode(ItemTransaction Table)
            if (pro != null)                
                ItemTransaction("Delete", pro.MaterialCenter, null, UserId, CurrentDate, id);

            var proItem = db.ProItems.Where(a => a.Production == id);
            if (proItem != null)
            { 
                db.ProItems.RemoveRange(db.ProItems.Where(a => a.Production == id));
            }
            var progen = db.GeneratedItem.Where(a => a.Production == id);
            if (progen != null)
            {
                db.GeneratedItem.RemoveRange(db.GeneratedItem.Where(a => a.Production == id));
            }
            var appr = db.Approvals.Where(a => a.TransEntry == id && a.Type == "Production").FirstOrDefault();
            if (appr != null)
            {
                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == id && a.Type == "Production"));
            }
            var app = db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "Production").FirstOrDefault();
            if (app != null)
            {
                db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "Production"));
            }
            db.Productions.Remove(pro);
            com.addlog(LogTypes.Deleted, UserId, "Production", "Productions", findip(), id, "Successfully Deleted Production");
            db.SaveChanges();
            return true;
        }

        [RedirectingAction]
        [HttpGet]
        [QkAuthorize(Roles = "Dev,View Production")]
        public ActionResult Details(long? id)
        {
            ProductionViewModel vmodel = new ProductionViewModel();
            vmodel = (from a in db.GeneratedItem
                      join z in db.Productions on a.Production equals z.ProductionId into con
                      from z in con.DefaultIfEmpty()
                      join b in db.BillOfMaterials on a.BOM equals b.BOMId into bom
                      from b in bom.DefaultIfEmpty()
                      join c in db.Items on a.Item equals c.ItemID into item
                      from c in item.DefaultIfEmpty()
                      join d in db.ItemUnits on a.Unit equals d.ItemUnitID into unit
                      from d in unit.DefaultIfEmpty()
                      join e in db.Users on z.CreatedBy equals e.Id
                      join f in db.MCs on z.MaterialCenter equals f.MCId
                      join g in db.Branchs on z.Branch equals g.BranchID
                      join p in db.Projects on z.Project equals p.ProjectId into prjct
                      from p in prjct.DefaultIfEmpty()
                      join t in db.ProTasks on z.Project equals t.ProjectId into ptask
                      from t in ptask.DefaultIfEmpty()
                      where a.Production == id
                      select new ProductionViewModel
                      {
                          ProductionId = z.ProductionId,
                          BOMName = b.BOMName,
                          VoucherNo = z.VoucherNo,
                          PEDate = z.PEDate.ToString(),
                          MC = f.MCName,
                          BranchName = g.BranchName,
                          UserName = e.UserName,
                          Note = z.Note.Replace("\n", "<br />"),
                          Ref1 = z.Ref1,
                          Ref2 = z.Ref2,
                          Ref3 = z.Ref3,
                          Ref4 = z.Ref4,
                          Ref5 = z.Ref5,
                          Project=z.Project,
                          ProTask=z.ProTask,
                          ProjectName = p.ProCode + "-" + p.ProjectName,
                          TaskName = t.TaskCode + "-" + t.TaskName,
                          Emp = (from ab in db.Approvals
                                 join bb in db.Employees on ab.EmployeeId equals bb.EmployeeId
                                 where ab.TransEntry == id && ab.Type == "Production"
                                 select new ApprovalViewModel
                                 {
                                     EmpName = bb.FirstName + " " + bb.LastName
                                 }).ToList()
                      }).FirstOrDefault();
            var billof = (from a in db.BillOfMaterials
                          join z in db.GeneratedItem on a.BOMId equals z.BOM into con
                          from z in con.DefaultIfEmpty()//db.GeneratedItem.Where(a => a.Production == id)
                          where z.Production == id
                          select new
                          {
                              BOM_Id = a.BOMId,
                              BOMName = a.BOMName,
                          }).ToList();
            var BOMName = "";
            foreach (var arr in billof)
            {
                BOMName = BOMName + ',' + arr.BOMName;
            };
            vmodel.ItemName = BOMName;
            vmodel.ProItemvmodel = db.ProItems.Where(a => a.Production == id)
            .Select(b => new ProItemViewModel
            {
                Quantity = b.Quantity,
                ItemName = db.Items.Where(a => a.ItemID == b.ItemId).Select(a => a.ItemName).FirstOrDefault(),
                ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.Unit).Select(a => a.ItemUnitName).FirstOrDefault(),
                Price = b.PPrice,
                Amount = b.PAmount
            }).ToList();
            vmodel.progen = db.GeneratedItem.Where(a => a.Production == id)
            .Select(b => new ProItemViewModel
            {
                Quantity = b.Qty,
                ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.Unit).Select(a => a.ItemUnitName).FirstOrDefault(),
                Price = b.Price,
                Amount = b.Amount
            }).ToList();

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Production" && a.Status == Status.active).ToList();

            return View(vmodel);
        }

        [HttpPost]
        public JsonResult GetProDetails(long proId)
        {

            var prod = (from a in db.GeneratedItem
                        join z in db.Productions on a.Production equals z.ProductionId into con
                        from z in con.DefaultIfEmpty()
                        join b in db.BillOfMaterials on a.BOM equals b.BOMId into boms
                        from b in boms.DefaultIfEmpty()
                        join c in db.Items on a.Item equals c.ItemID into items
                        from c in items.DefaultIfEmpty()
                        join d in db.ItemUnits on a.Unit equals d.ItemUnitID into unit
                        from d in unit.DefaultIfEmpty()
                        join e in db.Users on z.CreatedBy equals e.Id
                        where a.Production == proId
                        select new
                        {
                            z.ProductionId,
                            z.VoucherNo,
                            z.PEDate,
                            b.BOMName,
                            c.ItemName,
                            a.Qty,
                            a.Unit,
                            d.ItemUnitName,
                            a.Expense,
                            a.Price,
                            a.Amount,
                            e.UserName,
                            z.Note
                        }).FirstOrDefault();

            var item = (from a in db.ProItems
                        join b in db.Items on a.ItemId equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.Production == proId
                        select new
                        {
                            a.ItemId,
                            a.Quantity,
                            a.Unit,
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

                            //for min stock check
                            PriPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,


                            PriSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,


                            //stock adjustment---
                            PriAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            subLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            //-------
                            // production ----
                            // main item
                            PriProdItem = (decimal?)db.GeneratedItem.Where(a => a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
                            SubProdItem = (decimal?)db.GeneratedItem.Where(a => a.Item == b.ItemID && a.Unit == b.SubUnitId).Select(a => a.Qty).Sum() ?? 0,
                            // compined item
                            PriProdCItem = (decimal?)db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,
                            SubProdCItem = (decimal?)db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId).Select(a => a.Quantity).Sum() ?? 0,
                            // prodsemble -----
                            // main item
                            PriUnItem = (decimal?)db.ConsumedItem.Where(a => a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
                            SubUnItem = (decimal?)db.ConsumedItem.Where(a => a.Item == b.ItemID && a.Unit == b.SubUnitId).Select(a => a.Qty).Sum() ?? 0,
                            // compined item
                            PriUnCItem = (decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,
                            SubUnCItem = (decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId).Select(a => a.Quantity).Sum() ?? 0,


                        }).AsEnumerable().Select(o => new
                        {
                            o.ItemId,
                            o.Quantity,
                            o.Unit,
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            OpeningStock = o.OpeningStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ConFactor,
                            o.SellingPrice,
                            o.PurchasePrice,
                            o.BasePrice,
                            o.MRP,
                            price = (o.SellingPrice != 0) ? o.SellingPrice : o.MRP,
                            o.KeepStock,

                            PriPurchase = (o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)),
                            SubPurchase = (o.SubPurchase % o.ConFactor),

                            PriSale = (o.PriSale + (int)(o.SubSale / o.ConFactor)),
                            SubSale = (o.SubSale % o.ConFactor),

                            PriPReturn = (o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)),
                            SubPReturn = (o.SubPReturn % o.ConFactor),

                            PriSReturn = (o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)),
                            SubSReturn = (o.SubSReturn % o.ConFactor),

                            pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)),
                            subtotal = ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),
                            total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),


                        }).ToList();


            return new QuickSoft.Models.LegacyJsonResult { Data = new { prod, item } };
        }
        public ActionResult GetProd(long? proId)
        {
            var voucher = db.Productions.Where(x => x.ProductionId == proId).Select(y => y.VoucherNo).FirstOrDefault();

            var prod = (from a in db.GeneratedItem
                        join z in db.Productions on a.Production equals z.ProductionId into con
                        from z in con.DefaultIfEmpty()
                        join b in db.BillOfMaterials on a.BOM equals b.BOMId into boms
                        from b in boms.DefaultIfEmpty()
                        join c in db.Items on a.Item equals c.ItemID into items
                        from c in items.DefaultIfEmpty()
                        join d in db.ItemUnits on a.Unit equals d.ItemUnitID into unit
                        from d in unit.DefaultIfEmpty()
                        join f in db.ItemUnits on c.SubUnitId equals f.ItemUnitID into second
                        from f in second.DefaultIfEmpty()
                        join e in db.Users on z.CreatedBy equals e.Id
                        where a.Production == proId
                        select new
                        {
                            z.ProductionId,
                            z.VoucherNo,
                            z.PEDate,
                            b.BOMName,
                            c.ItemName,
                            Quantity = a.Qty,
                            a.Unit,
                            d.ItemUnitName,
                            a.Expense,
                            a.Price,
                            a.Amount,
                            e.UserName,
                            c.ItemUnitID,
                            c.SubUnitId,
                            PriUnit = d.ItemUnitName,
                            SubUnit = f.ItemUnitName,
                            z.Note,
                            b.BOMId,
                            ItemId = c.ItemID,
                            ItemNamewithcode = c.ItemCode + " - " + c.ItemName,
                        }).ToList();

            var BOMArray = prod.Select(x => x.BOMId).ToArray();
            var BillOFMat = (from i in db.BillOfMaterials
                             where
                             (BOMArray.Contains(i.BOMId))
                             select new
                             {
                                 BOMName = i.BOMName,
                                 BOMId = i.BOMId
                             }).ToList();
            var bom = (from a in db.GeneratedItem
                       join x in db.Productions on a.Production equals x.ProductionId into con
                       from x in con.DefaultIfEmpty()
                       join z in db.BillOfMaterials on a.BOM equals z.BOMId into BM
                       from z in BM.DefaultIfEmpty()
                       join b in db.Items on z.ItemId equals b.ItemID into items
                       from b in items.DefaultIfEmpty()
                       join c in db.ItemUnits on a.Unit equals c.ItemUnitID into unit
                       from c in unit.DefaultIfEmpty()
                       join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                       from d in second.DefaultIfEmpty()
                       where a.Production == proId
                       select new
                       {
                           BOMId = a.BOM,
                           z.BOMName,
                           b.ItemName,
                           z.ItemId,
                           Quantity = a.Qty,
                           a.Expense,
                           a.Unit,
                           b.ItemUnitID,
                           b.SubUnitId,
                           PriUnit = c.ItemUnitName,
                           SubUnit = d.ItemUnitName,
                           c.ItemUnitName,
                           a.Price,
                       }).ToList();

            var item = (from a in db.ProItems
                        join e in db.BOMItems on a.ItemId equals e.ItemId into pri
                        from e in pri.DefaultIfEmpty()
                        join b in db.Items on a.ItemId equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        let BOM = db.BOMItems.Where(x => x.BOMItemId == e.BOMItemId).Select(y => y.BOMId).FirstOrDefault()
                        where a.Production == proId && (BOMArray.Contains(e.BOMId))
                        select new
                        {
                            e.BOMItemId,
                            a.ProItemId,
                            a.ItemId,
                            a.Quantity,
                            bqty = e.Quantity,
                            a.Unit,
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
                            BOMId = BOM,//db.GeneratedItem.Where(x => x.Item == a.ItemId && x.Production==proId).Select(y => y.BOM).FirstOrDefault(),
                            a.PPrice,
                            //for min stock check
                            PriPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,

                            //stock adjustment---
                            PriAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,
                            SubAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,

                            PriLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            subLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
                            //-------
                            // production ----
                            // main item
                            PriProdItem = (decimal?)db.GeneratedItem.Where(a => a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
                            SubProdItem = (decimal?)db.GeneratedItem.Where(a => a.Item == b.ItemID && a.Unit == b.SubUnitId).Select(a => a.Qty).Sum() ?? 0,
                            // compined item
                            PriProdCItem = (decimal?)db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,
                            SubProdCItem = (decimal?)db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId).Select(a => a.Quantity).Sum() ?? 0,
                            // prodsemble -----
                            // main item
                            PriUnItem = (decimal?)db.ConsumedItem.Where(a => a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
                            SubUnItem = (decimal?)db.ConsumedItem.Where(a => a.Item == b.ItemID && a.Unit == b.SubUnitId).Select(a => a.Qty).Sum() ?? 0,
                            // compined item
                            PriUnCItem = (decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,
                            SubUnCItem = (decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId).Select(a => a.Quantity).Sum() ?? 0,

                        }).Distinct().AsEnumerable().Select(o => new
                        {
                            o.BOMItemId,
                            o.ProItemId,
                            o.ItemId,
                            o.Quantity,
                            o.bqty,
                            o.Unit,
                            o.ItemID,
                            o.ItemCode,
                            o.ItemName,
                            o.ItemWithCode,
                            o.ItemUnitID,
                            o.SubUnitId,
                            PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
                            SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
                            OpeningStock = o.OpeningStock,
                            MinStock = (o.MinStock != null) ? o.MinStock : 0,
                            o.ConFactor,
                            o.SellingPrice,
                            o.PurchasePrice,
                            o.BasePrice,
                            o.MRP,
                            price = (o.PPrice != 0) ? o.PPrice : o.PurchasePrice,
                            o.KeepStock,
                            o.BOMId,

                            PriPurchase = (o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)),
                            SubPurchase = (o.SubPurchase % o.ConFactor),

                            PriSale = (o.PriSale + (int)(o.SubSale / o.ConFactor)),
                            SubSale = (o.SubSale % o.ConFactor),

                            PriPReturn = (o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)),
                            SubPReturn = (o.SubPReturn % o.ConFactor),

                            PriSReturn = (o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)),
                            SubSReturn = (o.SubSReturn % o.ConFactor),

                            pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)),
                            subtotal = ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),
                            total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),
                        }).Distinct().AsEnumerable().ToList();

            return LegacyJson(new { prod, bom, item });
        }
        public long GetVchNo()
        {
            Int64 PENo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "Production").Select(a => a.number).FirstOrDefault();
            if ((db.Productions.Select(p => p.PrNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
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
                PENo = db.Productions.Max(p => p.PrNo + 1);
            }
            return PENo;
        }
        private string VoucherNo(Int64 PrNo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "Production").Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == "Production").Select(a => a.number).FirstOrDefault();
            if (billNo == null)
            {
                if ((db.Productions.Select(p => p.PrNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    PrNo = db.Productions.Max(p => p.PrNo + 1);
                    billNo = companyPrefix + PrNo;
                    if (BillExist(billNo))
                    {
                        billNo = VoucherNo(PrNo, billNo);
                    }
                }
            }
            else
            {
                PrNo = PrNo + 1;
                billNo = companyPrefix + PrNo;
                if (BillExist(billNo))
                {
                    billNo = VoucherNo(PrNo, billNo);
                }

            }
            return billNo;
        }
        private bool BillExist(string SENo)
        {
            var Exists = db.Productions.Any(c => c.VoucherNo == SENo);
            bool res = (Exists) ? true : false;
            return res;
        }
        [HttpGet]
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "Production" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
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

            var MR = db.Productions.Where(a => a.ProductionId == id).FirstOrDefault();

            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "Production").OrderByDescending(a => a.CreatedDate).FirstOrDefault();

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
                AppUp.Type = "Production";

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
                            join d in db.Productions on b.TransEntry equals d.ProductionId into team
                            from d in team.DefaultIfEmpty()
                            join e in db.Employees on b.RequestBy equals e.UserId into emp
                            from e in emp.DefaultIfEmpty()
                            join u in db.Users on d.CreatedBy equals u.Id into req
                            from u in req.DefaultIfEmpty()
                            where b.TransEntry == MCId && b.Type == "Production"
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

        //Function To Update Generated & Consumed Item Quantity in ItemTransaction Table(Modes==>Create, Edit, Delete)
        public ActionResult ItemTransaction(string Action, long? MC, ProdViewModel ViewModel, string UserId, DateTime CurrentDate, long? ProductionId)
        {
            /********************************* Create Mode *********************************/
            if (Action == "Create")
            {
                decimal ConvrtdQty = 0, Quantity = 0;
                long    UnitId = 0, ItemId = 0;

                //=========>For Generated Items   
                foreach (var GeneratedItem in ViewModel.progenerated)
                {
                    ItemId      =   GeneratedItem.ItemId;
                    UnitId      =   Convert.ToInt64(GeneratedItem.ItemUnit);
                    Quantity    =   GeneratedItem.Quantity;

                    ConvrtdQty = (from b in db.Items
                                    where (b.ItemID == ItemId)
                                    select (UnitId == b.ItemUnitID) ?
                                        (b.SubUnitId != null) ? (Quantity * b.ConFactor) : Quantity
                                        : Quantity).FirstOrDefault();

                    //Add the generated Item Quantity
                    ItemTransactions McItemObj = db.ItemTransactions.Where(a => (a.ItemId == ItemId && a.McId == MC)).FirstOrDefault();

                    if (McItemObj == null)
                        //Add Quantity to ItemTransaction Table
                        com.AddItemTransaction(ItemId, MC, ConvrtdQty, UserId, CurrentDate);
                    else
                        com.UpdateItemTransaction(ItemId, MC, ConvrtdQty, UserId, CurrentDate);
                }

                //=========>For Consumed Items
                ItemId = 0; UnitId = 0; Quantity = 0; ConvrtdQty = 0;
                
                foreach (var ConsumedItem in ViewModel.proitem)
                {
                    ItemId      =   ConsumedItem.ItemId;
                    UnitId      =   Convert.ToInt64(ConsumedItem.Unit);
                    Quantity    =   ConsumedItem.Quantity;

                    ConvrtdQty  =   (from b in db.Items
                                     where (b.ItemID == ItemId)
                                     select (UnitId == b.ItemUnitID) ?
                                        (b.SubUnitId != null) ? (Quantity * b.ConFactor) : Quantity
                                        : Quantity).FirstOrDefault();

                    //Subtract the consumed Item Quantity
                    ItemTransactions McItemObj = db.ItemTransactions.Where(a => (a.ItemId == ItemId && a.McId == MC)).FirstOrDefault();

                    if (McItemObj == null)
                        //Add Quantity to ItemTransaction Table
                        com.AddItemTransaction(ItemId, MC, -ConvrtdQty, UserId, CurrentDate);
                    else
                        com.UpdateItemTransaction(ItemId, MC, -ConvrtdQty, UserId, CurrentDate);
                }                       
            }
            /***********************************************************************************/

            /*************************************** Edit Mode *********************************/
            if (Action == "Edit")
            {
                long PrevItemId = 0, ItemId = 0, UnitId = 0;
                long? PrevMc = 0;
                decimal PreviousQty = 0, ConvrtdQty = 0, Quantity = 0;

                //=========>For Generated Items 
                var PrevGenItems =  (from a in db.GeneratedItem
                                     join b in db.Items on a.Item equals b.ItemID
                                     join c in db.Productions
                                     on a.Production equals c.ProductionId
                                     where (c.ProductionId == ProductionId)
                                     select new 
                                     {
                                         ItemId         =   a.Item,
                                         Quantity       =   ((a.Unit == b.ItemUnitID) ?
                                                            (b.SubUnitId != null) ? (a.Qty * b.ConFactor) : a.Qty
                                                            : a.Qty),
                                         MaterialCenter =   c.MaterialCenter,
                                     }).ToList();

                PrevMc = PrevGenItems.Select(x => x.MaterialCenter).FirstOrDefault();

                /*********** Delete last Transaction from ItemTransaction *********/
                foreach (var prevrow in PrevGenItems)
                {
                    PrevItemId  = prevrow.ItemId;
                    PreviousQty = prevrow.Quantity;

                    //Subtract the Previous generated Item quantity in previous Material Centre                  
                    com.UpdateItemTransaction(PrevItemId, PrevMc, -PreviousQty, UserId, CurrentDate);                   
                }

                /*********** Insert new transaction into ItemTransaction *********/
                foreach (var GeneratedItem in ViewModel.progenerated)
                {
                    ItemId      =   GeneratedItem.ItemId;
                    UnitId      =   Convert.ToInt64(GeneratedItem.ItemUnit);
                    Quantity    =   GeneratedItem.Quantity;

                    ConvrtdQty = (from b in db.Items
                                  where (b.ItemID == ItemId)
                                  select  (UnitId == b.ItemUnitID) ?
                                            (b.SubUnitId != null) ? (Quantity * b.ConFactor) : Quantity
                                            : Quantity).FirstOrDefault();

                    //Add the generated Item Quantity
                    ItemTransactions McItemObj = db.ItemTransactions.Where(a => (a.ItemId == ItemId && a.McId == MC)).FirstOrDefault();

                    if (McItemObj == null)
                        //Add Quantity to ItemTransaction Table
                        com.AddItemTransaction(ItemId, MC, ConvrtdQty, UserId, CurrentDate);
                    else
                        com.UpdateItemTransaction(ItemId, MC, ConvrtdQty, UserId, CurrentDate);
                }

                //=========>For Consumed Items 
                PrevItemId = 0; PrevMc = 0; PreviousQty = 0; ItemId = 0; UnitId = 0; Quantity = 0; ConvrtdQty = 0;

                var PrevConsItems = (from a in db.ProItems
                                     join b in db.Items on a.ItemId equals b.ItemID
                                     join c in db.Productions
                                     on a.Production equals c.ProductionId
                                     where (c.ProductionId == ProductionId)
                                     select new
                                     {
                                        ItemId          =   a.ItemId,
                                        Quantity        =   ((a.Unit == b.ItemUnitID) ?
                                                            (b.SubUnitId != null) ? (a.Quantity * b.ConFactor) : a.Quantity
                                                            : a.Quantity),
                                        MaterialCenter  =   c.MaterialCenter,
                                    }).ToList();

                PrevMc = PrevConsItems.Select(x => x.MaterialCenter).FirstOrDefault();

                /*********** Delete last Transaction from ItemTransaction *********/
                foreach (var prevrow in PrevConsItems)
                {
                    PrevItemId  = prevrow.ItemId;
                    PreviousQty = prevrow.Quantity;

                    //Add the Previous Consumed Item quantity in previous Material Centre                  
                    com.UpdateItemTransaction(PrevItemId, PrevMc, PreviousQty, UserId, CurrentDate);
                }

                /*********** Insert new transaction into ItemTransaction *********/    
                foreach (var ConsumedItem in ViewModel.proitem)
                {
                    ItemId      =   ConsumedItem.ItemId;
                    UnitId      =   Convert.ToInt64(ConsumedItem.Unit);
                    Quantity    =   ConsumedItem.Quantity;

                    ConvrtdQty = (from b in db.Items
                                  where (b.ItemID == ItemId)
                                  select (UnitId == b.ItemUnitID) ?
                                          (b.SubUnitId != null) ? (Quantity * b.ConFactor) : Quantity
                                          : Quantity).FirstOrDefault();

                    //Subtract the consumed Item Quantity
                    ItemTransactions McItemObj = db.ItemTransactions.Where(a => (a.ItemId == ItemId && a.McId == MC)).FirstOrDefault();

                    if (McItemObj == null)
                        //Add Quantity to ItemTransaction Table
                        com.AddItemTransaction(ItemId, MC, -ConvrtdQty, UserId, CurrentDate);
                    else
                        com.UpdateItemTransaction(ItemId, MC, -ConvrtdQty, UserId, CurrentDate);
                }
            }
            /***********************************************************************************/

            /************************************ Delete Mode *********************************/
            if (Action == "Delete")
            {
                long    ItemId   = 0;
                decimal Quantity = 0;

                //===========>For Generated Items 
                var GenItemList =  (from a in db.GeneratedItem
                                    join b in db.Items on a.Item equals b.ItemID
                                    where (a.Production == ProductionId)
                                    select new 
                                    {
                                        ItemId      =   a.Item,
                                        Quantity    =   (a.Unit == b.ItemUnitID) ?
                                                        (b.SubUnitId != null) ? (a.Qty * b.ConFactor) : a.Qty
                                                        : a.Qty
                                    }).ToList();

                if (GenItemList != null && GenItemList.Count != 0)
                {
                    foreach (var row in GenItemList)
                    {
                        ItemId      =   row.ItemId;
                        Quantity    =   row.Quantity; 
                             
                        //Subtract the generated Item Quantity
                        com.UpdateItemTransaction(ItemId, MC, -Quantity, UserId, CurrentDate);
                    }
                }

                //=========>For Consumed Items 
                ItemId = 0; Quantity = 0;

                var ConsmdItemList  =  (from a in db.ProItems
                                        join b in db.Items on a.ItemId equals b.ItemID
                                        where (a.Production == ProductionId)
                                        select new
                                        {
                                            ItemId      =   a.ItemId,
                                            Quantity    =   (a.Unit == b.ItemUnitID) ?
                                                            (b.SubUnitId != null) ? (a.Quantity * b.ConFactor) : a.Quantity
                                                            : a.Quantity
                                        }).ToList();

                if (ConsmdItemList != null && ConsmdItemList.Count != 0)
                {
                    foreach (var row in ConsmdItemList)
                    {
                        ItemId      =   row.ItemId;
                        Quantity    =   row.Quantity;

                        //Add the consumed Item Quantity                                    
                        com.UpdateItemTransaction(ItemId, MC, Quantity, UserId, CurrentDate);
                    }
                }
            }
            /***********************************************************************************/
            return Json("Item Transaction Updated Successfully");
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
