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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class UnassembleController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public UnassembleController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: Unassemble
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Unassemble List")]
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

            var MlaUAssem = db.EnableSettings.Where(a => a.EnableType == "MLAUAssem").FirstOrDefault();
            var MlaUAssems = MlaUAssem != null ? MlaUAssem.Status : Status.inactive;
            ViewBag.MLAUAssem = MlaUAssems;

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            var Remind = db.EnableSettings.Where(a => a.EnableType == "RemindUnass").FirstOrDefault();
            var Reminds = Remind != null ? Remind.Status : Status.inactive;
            ViewBag.Remind = Reminds;

            return View();
        }

        //[RedirectingAction]
        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Unassemble List")]
        public ActionResult GetUnassemble(string BillNo, string FromDate, string ToDate, long? BomName, long? ItemName, string user, string appstat)
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
            var userpermission = User.IsInRole("All Unassemble Entry");
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
            var uUnassembleView = User.IsInRole("View Unassemble");
            var uEdit = User.IsInRole("Edit Unassemble");
            var uDelete = User.IsInRole("Delete Unassemble");

            // EF Core 10 cannot translate the nested-collection / GroupBy-latest projections (the `app`,
            // `AppStatus`, `chkAppStatus` lets) nor the trailing `GroupBy().Select(FirstOrDefault())` inside the
            // executed query. Split SERVER from CLIENT: materialize only entity columns + simple scalars (the
            // scalar `Total` Sum stays server-side) into serverRows, do the GroupBy-first and build the approval
            // lookups client-side, then re-project with the SAME member names + order.
            var serverQuery = (from a in db.Unassembles
                     join y in db.ConsumedItem on a.UnassembleId equals y.Unassemble into unass
                     from y in unass.DefaultIfEmpty()
                     join b in db.BillOfMaterials on y.BOM equals b.BOMId into bom
                     from b in bom.DefaultIfEmpty()
                     join c in db.Items on y.Item equals c.ItemID into item
                     from c in item.DefaultIfEmpty()
                     join d in db.ItemUnits on y.Unit equals d.ItemUnitID into unit
                     from d in unit.DefaultIfEmpty()
                     join e in db.Users on a.CreatedBy equals e.Id
                     join f in db.MCs on a.MaterialCenter equals f.MCId
                     join g in db.Branchs on a.Branch equals g.BranchID
                         //let mc = db.MCs.Where(x => x.AssignedUser == a.CreatedBy).Select(x => x.MCId).FirstOrDefault()
                         // app/AppStatus/chkAppStatus (nested collections + GroupBy-latest) are computed
                         // client-side after materialization — EF Core 10 can't translate them inside this query.

                     where (BillNo == null || BillNo == "" || a.VoucherNo == BillNo) &&
                     (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(a.PEDate, fdate) <= 0) &&
                     (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(a.PEDate, tdate) >= 0) &&
                     (BomName == 0 || b.BOMId == BomName) &&
                     (ItemName == 0 || c.ItemID == ItemName)
                     //&& (mc == 0 || mc == a.MaterialCenter)
                     && (!MCList.Any() || MCArray.Contains(a.MaterialCenter))
                     && (userpermission == true || a.CreatedBy == UserId)
                     && (user == null || user == "" || e.Id == user)
                     select new
                     {
                         a.UnassembleId,
                         a.VoucherNo,
                         a.PEDate,
                         b.BOMName,
                         c.ItemName,
                         y.Qty,
                         d.ItemUnitName,
                         y.Price,
                         y.Amount,
                         e.UserName,
                         a.Note,
                         Dev = uDev,
                         MC = f.MCName,
                         Total = (db.ConsumedItem.Where(x => x.Unassemble == a.UnassembleId).Select(y => y.Amount).Sum()),
                         Details = uUnassembleView,
                         Edit = uEdit,
                         Delete = uDelete,
                         g.BranchName,
                         a.CreatedDate
                     });

            // Performance (audit P2, hybrid): server paging when no search, plain-column sort, AND no client-side
            // filter is active (the conditions below mirror the filters' own guards); else original path.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase){ "Amount","BOMName","BranchName","CreatedDate","Delete","Details","Dev","Edit","ItemName","ItemUnitName","MC","Note","PEDate","Price","Qty","Total","UnassembleId","UserName","VoucherNo" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0 && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn)) && !(appstat != "");
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn) ? serverQuery.OrderBy("UnassembleId asc") : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

            // GroupBy-first (one row per UnassembleId), client-side now that rows are materialized.
            var groupedRows = serverRows.GroupBy(x => x.UnassembleId).Select(x => x.First()).ToList();

            // CLIENT-side approval lookups keyed by UnassembleId (missing key -> empty, no KeyNotFound).
            var unaIds = groupedRows.Select(o => o.UnassembleId).ToList();
            // app = approver EmployeeIds (nested collection, keyed by TransEntry == UnassembleId).
            var appLookup = db.Approvals
                .Where(x => x.Type == "Unassemble" && unaIds.Contains(x.TransEntry))
                .Select(x => new { x.TransEntry, x.EmployeeId })
                .ToList()
                .ToLookup(x => x.TransEntry);
            // AppStatus = all ApprovalUpdate statuses; raw rows materialized once and reused for chkAppStatus.
            var appUpdRows = db.ApprovalUpdates
                .Where(x => x.Type == "Unassemble" && unaIds.Contains(x.TransEntry))
                .Select(x => new { x.TransEntry, x.ApprovalStatus, x.ApprovedBy, x.CreatedDate })
                .ToList();
            var appStatusLookup = appUpdRows.ToLookup(x => x.TransEntry);
            // chkAppStatus = latest ApprovalUpdate status per ApprovedBy (GroupBy-latest-per-group), per unassemble.
            var chkAppStatusLookup = appUpdRows
                .GroupBy(x => x.TransEntry)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(l => l.ApprovedBy)
                    .Select(grp => grp.OrderByDescending(c => c.CreatedDate).First())
                    .Select(x => x.ApprovalStatus).ToList());

            var v = groupedRows.Select(o =>
                     {
                         var app = appLookup[o.UnassembleId].Select(x => x.EmployeeId).ToList();
                         var AppStatus = appStatusLookup[o.UnassembleId].Select(x => x.ApprovalStatus).ToList();
                         var chkAppStatus = chkAppStatusLookup.TryGetValue(o.UnassembleId, out var ck) ? ck : new List<ApprovalStatus>();
                         return new
                     {
                         o.UnassembleId,
                         o.VoucherNo,
                         o.PEDate,
                         o.BOMName,
                         o.ItemName,
                         o.Qty,
                         o.ItemUnitName,
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
                         o.CreatedDate
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
            if (!fastPage && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            if (!fastPage) { recordsTotal = v.Count(); }
            var data = (fastPage ? v.ToList() : v.Skip(skip).Take(pageSize).ToList());
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }



        // GET: Unassemble/Create
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Create Unassemble")]
        public ActionResult Create()
        {
            var ventry = new UnassembleViewModel
            {
                VoucherNo = VoucherNo(),
                PEDate = (System.DateTime.Now).ToString("dd-MM-yyyy"),
            };

            ViewBag.boms = QkSelect.List(
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
            var userpermission = User.IsInRole("All Unassemble Entry");
            ViewBag.LastEntry = db.Unassembles.Where(p => (userpermission == true || p.CreatedBy == UserId)).Select(p => p.UnassembleId).AsEnumerable().DefaultIfEmpty(0).Max();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                 .Select(s => new
                 {
                     ID = s.EmployeeId,
                     Name = s.FirstName + " " + s.LastName
                 })
                 .ToList();
            ViewBag.ApproveBy = QkSelect.List(appby, "ID", "Name");

            var MlaUAssem = db.EnableSettings.Where(a => a.EnableType == "MLAUAssem").FirstOrDefault();
            var MlaUAssems = MlaUAssem != null ? MlaUAssem.Status : Status.inactive;
            ViewBag.MLAUAssem = MlaUAssems;
            companySet();

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;

            //field mapping
            ventry.FieldMap = db.FieldMappings.Where(a => a.Section == "Unassemble" && a.Status == Status.active).ToList();

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;

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
            _FinancialYear();
            return View(ventry);
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Create Unassemble")]
        public ActionResult Create(UnasViewModel vmodel)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
            var today = Convert.ToDateTime(System.DateTime.Now);
            var Date = DateTime.Parse(vmodel.PEDate.ToString(), new CultureInfo("en-GB"));
            long Branch = 0;
            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            if (BranchCheck == Status.active)
            {
                Branch = vmodel.unasdata.Branch;
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
                MC = Convert.ToInt64(vmodel.unasdata.MaterialCenter);
            }
            else
            {
                MC = 1;
            }

            string voucher = vmodel.unasdata.VoucherNo;
            string Note = vmodel.unasdata.Note;
            long MaterialCenter = Convert.ToInt64(vmodel.unasdata.MaterialCenter);
            Int64 unasId = 0;
            Int64 UnNom = GetVchNo();
            Unassemble unas = new Unassemble
            {
                VoucherNo = voucher,
                EntryNo = UnNom,
                PEDate = Date,
                Note = vmodel.unasdata.Note,
                MaterialCenter = MC,
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
            db.Unassembles.Add(unas);
            db.SaveChanges();
            
            //To Update the quantity in Create Mode(ItemTransaction Table)
            ItemTransaction("Create", MC, vmodel, UserId, today, 0);

            unasId = unas.UnassembleId;
            foreach (var arr in vmodel.bom)
            {
                long? BOM = arr.BOM_Id;

                foreach (var arrCons in vmodel.unasconsumed.ToList())
                {
                    if (BOM == arrCons.BOMId)
                    {
                        ConsumedItems con = new ConsumedItems
                        {
                            Unassemble = unasId,
                            BOM = (long)arrCons.BOMId,
                            Item = arrCons.ItemId,
                            Qty = arrCons.Quantity,
                            Unit = Convert.ToInt64(arrCons.ItemUnit),
                            Price = arrCons.Price,
                            Amount = arrCons.Amount,
                        };
                        db.ConsumedItem.Add(con);
                        db.SaveChanges();
                        vmodel.unasconsumed.Remove(arrCons);
                    }
                    foreach (var arrItem in vmodel.unasitem.ToList())
                    {
                        if (BOM == arrItem.BOM)
                        {
                            UnassembleItem unItem = new UnassembleItem();
                            {
                                unItem.Unassemble = unasId;
                                unItem.ItemId = arrItem.ItemId;
                                unItem.Unit = arrItem.Unit;
                                unItem.Quantity = arrItem.Quantity;
                                unItem.PPrice = arrItem.PPrice;
                                unItem.PAmount = arrItem.PAmount;
                                db.UnassembleItems.Add(unItem);
                                db.SaveChanges();
                            }
                            vmodel.unasitem.Remove(arrItem);
                        }
                    }
                    BOM = null;
                }
            }
            //Approved By
            var Appby = vmodel.ApprovedBy;
            if (Appby != null && Appby != "")
            {
                long[] Approve = Appby.Split(',').Select(Int64.Parse).ToArray();

                Approval approval = new Approval();
                foreach (var emp in Approve)
                {
                    approval.TransEntry = unasId;
                    approval.Type = "Unassemble";
                    approval.EmployeeId = emp;
                    db.Approvals.Add(approval);
                    db.SaveChanges();
                }
            }
            com.addlog(LogTypes.Created, UserId, "Unassemble", "Unassembles", findip(), unasId, "Successfully Submitted Unassemble");
            if (Convert.ToString(vmodel.fnval) == "print")
            {
                var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                var ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";

                var unasdata = vmodel.unasdata;
                var array = vmodel.bom.Select(x => x.BOM_Id).ToArray();
                var BOM = (from a in db.BillOfMaterials
                           where array.Contains(a.BOMId)
                           select new
                           {
                               BOMName = a.BOMName
                           });
                var Data = (from a in db.Unassembles
                            join b in db.MCs on a.MaterialCenter equals b.MCId into item
                            from b in item.DefaultIfEmpty()
                            join c in db.Branchs on a.Branch equals c.BranchID into Br
                            from c in Br.DefaultIfEmpty()
                            join p in db.Projects on a.Project equals p.ProjectId into prjct
                            from p in prjct.DefaultIfEmpty()
                            join t in db.ProTasks on a.Project equals t.ProjectId into ptask
                            from t in ptask.DefaultIfEmpty()
                            where a.VoucherNo == vmodel.unasdata.VoucherNo
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
                var unascons = (from a in db.Unassembles
                                join y in db.ConsumedItem on a.UnassembleId equals y.Unassemble into unass
                                from y in unass.DefaultIfEmpty()
                                join b in db.Items on y.Item equals b.ItemID into item
                                from b in item.DefaultIfEmpty()
                                join c in db.ItemUnits on y.Unit equals c.ItemUnitID into unit
                                from c in unit.DefaultIfEmpty()
                                where a.VoucherNo == vmodel.unasdata.VoucherNo
                                select new
                                {
                                    Item = b.ItemName,
                                    Unit = c.ItemUnitName,
                                    Qty = y.Qty,
                                    Price = y.Price,
                                    Amount = y.Amount
                                });
                var ConsTotal = unascons.Select(x => x.Amount).Sum();
                var ConsQty = unascons.Select(x => x.Qty).Sum();
                var unasitems = (from a in db.UnassembleItems
                                 join z in db.Unassembles on a.Unassemble equals z.UnassembleId into items
                                 from z in items.DefaultIfEmpty()
                                 join b in db.Items on a.ItemId equals b.ItemID into item
                                 from b in item.DefaultIfEmpty()
                                 join c in db.ItemUnits on a.Unit equals c.ItemUnitID into unit
                                 from c in unit.DefaultIfEmpty()
                                 where z.VoucherNo == vmodel.unasdata.VoucherNo
                                 select new
                                 {
                                     Item = b.ItemName,
                                     Unit = c.ItemUnitName,
                                     Qty = a.Quantity,
                                     Price = a.PPrice,
                                     Amount = a.PAmount
                                 });
                var fmapp = db.FieldMappings.Where(a => a.Section == "Unassemble" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();


                msg = "Successfully Added Unassemble .";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Data, unasdata, BOM, unascons, unasitems, ConsTotal, ConsQty, fmapp = fmapp, ComHeadCheck } };
            }
            else
            {
                msg = "Successfully Added Unassemble .";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Unassemble")]
        [HttpGet]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var userpermission = User.IsInRole("All Unassemble Entry");
            var UserId = User.Identity.GetUserId();
            Unassemble unas = db.Unassembles.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.UnassembleId == id).FirstOrDefault();

            if (unas == null)
            {
                return NotFound();
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

            UnassembleViewModel vmodel = new UnassembleViewModel();
            vmodel.UnassembleId = (long)id;
            vmodel.VoucherNo = unas.VoucherNo;
            vmodel.PEDate = unas.PEDate.ToString("dd-MM-yyyy");
            vmodel.Note = unas.Note;
            vmodel.Branch = unas.Branch;
            vmodel.MaterialCenter = unas.MaterialCenter;

            vmodel.Ref1 = unas.Ref1;
            vmodel.Ref2 = unas.Ref2;
            vmodel.Ref3 = unas.Ref3;
            vmodel.Ref4 = unas.Ref4;
            vmodel.Ref5 = unas.Ref5;

            vmodel.Project = unas.Project;
            vmodel.ProTask = unas.ProTask;
            //    ID = s.BOMId,
            //    Name = s.BOMName

            //    ID = s.ItemID,
            //    Name = s.ItemCode + " - " + s.ItemName


            ViewBag.preEntry = db.Unassembles.Where(a => a.UnassembleId < id && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.UnassembleId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.Unassembles.Where(a => a.UnassembleId > id && (userpermission == true || a.CreatedBy == UserId)).Select(a => a.UnassembleId).DefaultIfEmpty().Min();

            var emp = db.Approvals.Where(a => a.TransEntry == id && a.Type == "Unassemble").Select(a => a.EmployeeId).ToList();
            long[] empIds = emp.ToArray();

            var appby = db.Employees.Where(a => a.UserStatus == true && a.UserId != UserId)
                  .Select(s => new
                  {
                      FieldName = s.EmployeeId,
                      FieldID = s.FirstName + " " + s.LastName
                  })
                  .ToList();
            ViewBag.ApproveBy = new MultiSelectList(appby, "FieldName", "FieldID", empIds);

            var MlaUAssem = db.EnableSettings.Where(a => a.EnableType == "MLAUAssem").FirstOrDefault();
            var MlaUAssems = MlaUAssem != null ? MlaUAssem.Status : Status.inactive;
            ViewBag.MLAUAssem = MlaUAssems;

            var EditPermission = User.IsInRole("Disable Unassemble Edit After Approval");
            ViewBag.ChkApp = com.chkApproved((long)id, EditPermission, "Unassemble", UserId);
            companySet();

            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            ViewBag.HeadCheck = ComHeadCheck;
            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Unassemble" && a.Status == Status.active).ToList();

            var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
            var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;
            ViewBag.ProjChk = ProjectCheck;
            _FinancialYear();
            return View(vmodel);
        }

        // POST: Unassemble/Edit/5
        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Unassemble")]
        public ActionResult Edit(UnasViewModel vmodel, long? id)
        {
            try
            {
                bool stat = false;
                string msg;
                var UserId = User.Identity.GetUserId();

                long Branch = 0;

                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                if (BranchCheck == Status.active)
                {
                    Branch = vmodel.unasdata.Branch;
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
                    MC = Convert.ToInt64(vmodel.unasdata.MaterialCenter);
                }
                else
                {
                    MC = 1;
                }
                var EditPermission = User.IsInRole("Disable Unassemble Edit After Approval");
                if (com.chkApproved((long)id, EditPermission, "Unassemble", UserId) == true)
                {

                    var Date = DateTime.Parse(vmodel.PEDate.ToString(), new CultureInfo("en-GB"));
                    var today = Convert.ToDateTime(System.DateTime.Now);
                    string voucher = vmodel.unasdata.VoucherNo;
                    string Note = vmodel.unasdata.Note;
                    long MaterialCenter = Convert.ToInt64(vmodel.unasdata.MaterialCenter);

                    Unassemble una = db.Unassembles.Find(id);
                    if (BillExist(vmodel.unasdata.VoucherNo) && vmodel.unasdata.VoucherNo != una.VoucherNo)
                    {
                        msg = "Voucher No. Already Exists.";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }

                    una.VoucherNo = vmodel.unasdata.VoucherNo;
                    una.PEDate = DateTime.Parse(vmodel.PEDate.ToString(), new CultureInfo("en-GB"));
                    una.Note = vmodel.unasdata.Note;
                    una.Branch = Branch;
                    una.MaterialCenter = MC;
                    una.Ref1 = vmodel.Ref1;
                    una.Ref2 = vmodel.Ref2;
                    una.Ref3 = vmodel.Ref3;
                    una.Ref4 = vmodel.Ref4;
                    una.Ref5 = vmodel.Ref5;
                    una.Project = vmodel.Project;
                    una.ProTask = vmodel.ProTask;

                    db.Entry(una).State = EntityState.Modified;

                    //To Update the quantity in Edit Mode(ItemTransaction Table)
                    ItemTransaction("Edit", MC, vmodel, UserId, today, id);

                    var unasconsumed = db.Unassembles.Where(a => a.VoucherNo == voucher).FirstOrDefault();
                    if (unasconsumed.UnassembleId != 0)
                    {
                        var unasItem = db.UnassembleItems.Where(a => a.Unassemble == unasconsumed.UnassembleId);
                        if (unasItem != null)
                        {
                            db.UnassembleItems.RemoveRange(db.UnassembleItems.Where(a => a.Unassemble == id));
                        }
                        db.ConsumedItem.RemoveRange(db.ConsumedItem.Where(a => a.Unassemble == id));

                    }

                    foreach (var arr in vmodel.bom)
                    {
                        long? BOM = arr.BOM_Id;

                        foreach (var arrCons in vmodel.unasconsumed.ToList())
                        {
                            if (BOM == arrCons.BOMId)
                            {
                                ConsumedItems unas = new ConsumedItems();
                                {
                                    unas.Unassemble = (long)id;
                                    unas.Unit = Convert.ToInt64(arrCons.Unit);
                                    unas.BOM = (long)BOM;
                                    unas.Item = arrCons.ItemId;
                                    unas.Qty = arrCons.Quantity;
                                    unas.Price = arrCons.Price;
                                    unas.Amount = arrCons.Amount;
                                };
                                db.ConsumedItem.Add(unas);
                                db.SaveChanges();
                                vmodel.unasconsumed.Remove(arrCons);
                            }
                            foreach (var arrItem in vmodel.unasitem.ToList())
                            {
                                if (BOM == arrItem.BOM)
                                {
                                    UnassembleItem unItem = new UnassembleItem();

                                    unItem.Unassemble = (long)id;
                                    unItem.ItemId = arrItem.ItemId;
                                    unItem.Unit = arrItem.Unit;
                                    unItem.Quantity = arrItem.Quantity;
                                    unItem.PPrice = arrItem.PPrice;
                                    unItem.PAmount = arrItem.PAmount;
                                    db.UnassembleItems.Add(unItem);
                                    db.SaveChanges();
                                    vmodel.unasitem.Remove(arrItem);
                                }
                            }
                            BOM = null;
                        }
                    }

                    //Approved By
                    var empuser = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();
                    var chkapp = db.Approvals.Where(a => a.EmployeeId == empuser && a.TransEntry == (long)id && a.Type == "Unassemble").FirstOrDefault();
                    var MrnPO = db.Approvals.Where(a => a.TransEntry == (long)id && a.Type == "Unassemble").FirstOrDefault();
                    if (MrnPO != null)
                    {
                        if (chkapp != null)
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.EmployeeId != empuser && a.TransEntry == (long)id && a.Type == "Unassemble"));
                            db.SaveChanges();
                        }
                        else
                        {
                            db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == (long)id && a.Type == "Unassemble"));
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
                            approval.Type = "Unassemble";
                            approval.EmployeeId = emp;
                            db.Approvals.Add(approval);
                            db.SaveChanges();
                        }
                    }
                    com.addlog(LogTypes.Updated, UserId, "Unassemble", "Unassembles", findip(), (long)id, "Successfully Updated Unassemble");
                }
                if (Convert.ToString(vmodel.fnval) == "updateandprint")
                {
                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";

                    var unasdata = vmodel.unasdata;
                    var array = vmodel.bom.Select(x => x.BOM_Id).ToArray();
                    var BOM = (from a in db.BillOfMaterials
                               where array.Contains(a.BOMId)
                               select new
                               {
                                   BOMName = a.BOMName
                               });
                    var Data = (from a in db.Unassembles
                                join b in db.MCs on a.MaterialCenter equals b.MCId into item
                                from b in item.DefaultIfEmpty()
                                join c in db.Branchs on a.Branch equals c.BranchID into Br
                                from c in Br.DefaultIfEmpty()
                                join p in db.Projects on a.Project equals p.ProjectId into prjct
                                from p in prjct.DefaultIfEmpty()
                                join t in db.ProTasks on a.Project equals t.ProjectId into ptask
                                from t in ptask.DefaultIfEmpty()
                                where a.VoucherNo == vmodel.unasdata.VoucherNo
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
                    var unascons = (from a in db.Unassembles
                                    join y in db.ConsumedItem on a.UnassembleId equals y.Unassemble into unass
                                    from y in unass.DefaultIfEmpty()
                                    join b in db.Items on y.Item equals b.ItemID into item
                                    from b in item.DefaultIfEmpty()
                                    join c in db.ItemUnits on y.Unit equals c.ItemUnitID into unit
                                    from c in unit.DefaultIfEmpty()
                                    where a.VoucherNo == vmodel.unasdata.VoucherNo
                                    select new
                                    {
                                        Item = b.ItemName,
                                        Unit = c.ItemUnitName,
                                        Qty = y.Qty,
                                        Price = y.Price,
                                        Amount = y.Amount,
                                    });
                    var ConsTotal = unascons.Select(x => x.Amount).Sum();
                    var ConsQty = unascons.Select(x => x.Qty).Sum();
                    var unasitems = (from a in db.UnassembleItems
                                     join z in db.Unassembles on a.Unassemble equals z.UnassembleId into items
                                     from z in items.DefaultIfEmpty()
                                     join b in db.Items on a.ItemId equals b.ItemID into item
                                     from b in item.DefaultIfEmpty()
                                     join c in db.ItemUnits on a.Unit equals c.ItemUnitID into unit
                                     from c in unit.DefaultIfEmpty()
                                     where z.VoucherNo == vmodel.unasdata.VoucherNo
                                     select new
                                     {
                                         Item = b.ItemName,
                                         Unit = c.ItemUnitName,
                                         Qty = a.Quantity,
                                         Price = a.PPrice,
                                         Amount = a.PAmount
                                     });
                    var ItemTotal = unasitems.Select(x => x.Amount).Sum();
                    var ItemQty = unasitems.Select(x => x.Qty).Sum();

                    var fmapp = db.FieldMappings.Where(a => a.Section == "Unassemble" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                    msg = "Successfully Updated Unassemble .";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Data, unasdata, BOM, unascons, unasitems, ConsTotal, ConsQty, ItemQty, ItemTotal, fmapp = fmapp, ComHeadCheck } };
                }
                else
                {
                    msg = "Successfully Updated Unassemble .";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
            }
            catch
            {
                return View();
            }
        }

        // GET: Unassemble/Delete/5
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete Unassemble")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Delete
            var userpermission = User.IsInRole("All Unassemble Entry");
            var UserId = User.Identity.GetUserId();
            Unassemble unas = db.Unassembles.Where(x => (x.CreatedBy == UserId || userpermission == true) && x.UnassembleId == id).FirstOrDefault();

            if (unas == null)
            {
                return NotFound();
            }
            return PartialView(unas);
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete Unassemble")]
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
                    msg = "Successfully deleted Unassemble Voucher.";
                }

                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            catch
            {
                return View();
            }
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Unassemble")]
        public ActionResult DeleteAllUnassemble(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteUnas(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + " Unassemble.", true);
            return RedirectToAction("Index", "Unassemble");
        }
        private Boolean DeleteUnas(long Id)
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
            var UserId      = User.Identity.GetUserId();
            var CurrentDate = Convert.ToDateTime(System.DateTime.Now);

            Unassemble unas = db.Unassembles.Find(id);

            //To Update the quantity in Delete Mode(ItemTransaction Table)
            if (unas != null)
                ItemTransaction("Delete", unas.MaterialCenter, null, UserId, CurrentDate, id);

            var unItem = db.UnassembleItems.Where(a => a.Unassemble == id);
            if (unItem != null)
            {
                db.UnassembleItems.RemoveRange(db.UnassembleItems.Where(a => a.Unassemble == id));
            }
            var consumed = db.ConsumedItem.Where(a => a.Unassemble == id);
            if (consumed != null)
            {
                db.ConsumedItem.RemoveRange(db.ConsumedItem.Where(a => a.Unassemble == id));
            }
            var appr = db.Approvals.Where(a => a.TransEntry == id && a.Type == "Unassemble").FirstOrDefault();
            if (appr != null)
            {
                db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == id && a.Type == "Unassemble"));
            }
            var app = db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "Unassemble").FirstOrDefault();
            if (app != null)
            {
                db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "Unassemble"));
            }

            db.Unassembles.Remove(unas);
            com.addlog(LogTypes.Deleted, UserId, "Unassemble", "Unassembles", findip(), id, "Successfully Deleted Unassemble");
            db.SaveChanges();
            return true;
        }
        [RedirectingAction]
        [HttpGet]
        [QkAuthorize(Roles = "Dev,View Unassemble")]
        public ActionResult Details(long? id)
        {
            UnassembleViewModel vmodel = new UnassembleViewModel();
            vmodel = (from a in db.Unassembles
                      join y in db.ConsumedItem on a.UnassembleId equals y.Unassemble into unass
                      from y in unass.DefaultIfEmpty()
                      join b in db.BillOfMaterials on y.BOM equals b.BOMId into bom
                      from b in bom.DefaultIfEmpty()
                      join c in db.Items on y.Item equals c.ItemID into item
                      from c in item.DefaultIfEmpty()
                      join d in db.ItemUnits on y.Unit equals d.ItemUnitID into unit
                      from d in unit.DefaultIfEmpty()
                      join e in db.Users on a.CreatedBy equals e.Id
                      join f in db.MCs on a.MaterialCenter equals f.MCId
                      join g in db.Branchs on a.Branch equals g.BranchID
                      join p in db.Projects on a.Project equals p.ProjectId into prjct
                      from p in prjct.DefaultIfEmpty()
                      join t in db.ProTasks on a.Project equals t.ProjectId into ptask
                      from t in ptask.DefaultIfEmpty()
                      where a.UnassembleId == id
                      select new UnassembleViewModel
                      {
                          UnassembleId = a.UnassembleId,
                          BOMName = b.BOMName,
                          VoucherNo = a.VoucherNo,
                          PEDate = a.PEDate.ToString(),
                          Note = a.Note.Replace("\n", "<br />"),
                          BranchName = g.BranchName,
                          MC = f.MCName,
                          Ref1 = a.Ref1,
                          Ref2 = a.Ref2,
                          Ref3 = a.Ref3,
                          Ref4 = a.Ref4,
                          Ref5 = a.Ref5,
                          Project = a.Project,
                          ProTask = a.ProTask,
                          ProjectName = p.ProCode + "-" + p.ProjectName,
                          TaskName = t.TaskCode + "-" + t.TaskName,

                          Emp = (from ab in db.Approvals
                                 join bb in db.Employees on ab.EmployeeId equals bb.EmployeeId
                                 where ab.TransEntry == id && ab.Type == "Unassemble"
                                 select new ApprovalViewModel
                                 {
                                     EmpName = bb.FirstName + " " + bb.LastName
                                 }).ToList()
                      }).FirstOrDefault();

            var billof = (from a in db.BillOfMaterials
                          join z in db.ConsumedItem on a.BOMId equals z.BOM into con
                          from z in con.DefaultIfEmpty()//db.GeneratedItem.Where(a => a.Production == id)
                          where z.Unassemble == id
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

            vmodel.UnasItemvmodel = db.UnassembleItems.Where(a => a.Unassemble == id)
            .Select(b => new UnassembleItemViewModel
            {
                Quantity = b.Quantity,
                ItemName = db.Items.Where(a => a.ItemID == b.ItemId).Select(a => a.ItemName).FirstOrDefault(),
                ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.Unit).Select(a => a.ItemUnitName).FirstOrDefault(),
                Price = b.PPrice,
                Amount = b.PAmount
            }).ToList();

            vmodel.UnasCon = db.ConsumedItem.Where(a => a.Unassemble == id)
            .Select(b => new UnasConsViewModel
            {
                Quantity = b.Qty,
                ItemName = db.Items.Where(a => a.ItemID == b.Item).Select(a => a.ItemName).FirstOrDefault(),
                ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.Unit).Select(a => a.ItemUnitName).FirstOrDefault(),
                Price = b.Price,
                Amount = b.Amount
            }).ToList();

            //field mapping
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Unassemble" && a.Status == Status.active).ToList();

            return View(vmodel);
        }

        [HttpPost]
        public JsonResult GetUnasDetails(long unId)
        {

            var prod = (from a in db.Unassembles
                        join y in db.ConsumedItem on a.UnassembleId equals y.Unassemble into unass
                        from y in unass.DefaultIfEmpty()
                        join b in db.BillOfMaterials on y.BOM equals b.BOMId into boms
                        from b in boms.DefaultIfEmpty()
                        join c in db.Items on y.Item equals c.ItemID into items
                        from c in items.DefaultIfEmpty()
                        join d in db.ItemUnits on y.Unit equals d.ItemUnitID into unit
                        from d in unit.DefaultIfEmpty()
                        join e in db.Users on a.CreatedBy equals e.Id
                        where a.UnassembleId == unId
                        select new
                        {
                            a.UnassembleId,
                            a.VoucherNo,
                            a.PEDate,
                            b.BOMName,
                            c.ItemName,
                            y.Qty,
                            y.Unit,
                            d.ItemUnitName,
                            y.Price,
                            y.Amount,
                            e.UserName,
                            a.Note
                        }).FirstOrDefault();

            var item = (from a in db.UnassembleItems
                        join b in db.Items on a.ItemId equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.Unassemble == unId
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
                            // unassemble -----
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

        public ActionResult GetUnas(long unasId)
        {
            var voucher = db.Unassembles.Where(x => x.UnassembleId == unasId).Select(y => y.VoucherNo).FirstOrDefault();

            var unas = (from a in db.Unassembles
                        join y in db.ConsumedItem on a.UnassembleId equals y.Unassemble into unass
                        from y in unass.DefaultIfEmpty()
                        join b in db.BillOfMaterials on y.BOM equals b.BOMId into boms
                        from b in boms.DefaultIfEmpty()
                        join c in db.Items on y.Item equals c.ItemID into items
                        from c in items.DefaultIfEmpty()
                        join d in db.ItemUnits on y.Unit equals d.ItemUnitID into unit
                        from d in unit.DefaultIfEmpty()
                        join f in db.ItemUnits on c.SubUnitId equals f.ItemUnitID into second
                        from f in second.DefaultIfEmpty()
                        join e in db.Users on a.CreatedBy equals e.Id
                        where a.UnassembleId == (long)unasId
                        select new
                        {
                            a.UnassembleId,
                            a.VoucherNo,
                            a.PEDate,
                            b.BOMName,
                            c.ItemName,
                            Quantity = y.Qty,
                            y.Unit,
                            d.ItemUnitName,
                            y.Price,
                            y.Amount,
                            e.UserName,
                            c.ItemUnitID,
                            c.SubUnitId,
                            PriUnit = d.ItemUnitName,
                            SubUnit = f.ItemUnitName,
                            a.Note,
                            b.BOMId,
                            ItemId = c.ItemID,
                            ItemNamewithcode = c.ItemCode + " - " + c.ItemName,
                        }).ToList();

            var BOMArray = unas.Select(x => x.BOMId).ToArray();
            var BillOFMat = (from i in db.BillOfMaterials
                             where
                             (BOMArray.Contains(i.BOMId))
                             select new
                             {
                                 BOMName = i.BOMName,
                                 BOMId = i.BOMId
                             }).ToList();
            var bom = (from a in db.Unassembles
                       join y in db.ConsumedItem on a.UnassembleId equals y.Unassemble into gen
                       from y in gen.DefaultIfEmpty()
                       join z in db.BillOfMaterials on y.BOM equals z.BOMId into BM
                       from z in BM.DefaultIfEmpty()
                       join b in db.Items on z.ItemId equals b.ItemID into items
                       from b in items.DefaultIfEmpty()
                       join c in db.ItemUnits on y.Unit equals c.ItemUnitID into unit
                       from c in unit.DefaultIfEmpty()
                       join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                       from d in second.DefaultIfEmpty()
                       where a.UnassembleId == (long)unasId
                       select new
                       {
                           BOMId = y.BOM,
                           z.BOMName,
                           b.ItemName,
                           z.ItemId,
                           Quantity = y.Qty,
                           y.Unit,
                           b.ItemUnitID,
                           b.SubUnitId,
                           PriUnit = c.ItemUnitName,
                           SubUnit = d.ItemUnitName,
                           c.ItemUnitName,
                           y.Price,

                       }).ToList();

            var item = (from a in db.UnassembleItems
                        join e in db.BOMItems on a.ItemId equals e.ItemId into pri
                        from e in pri.DefaultIfEmpty()
                        join b in db.Items on a.ItemId equals b.ItemID
                        join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
                        from c in primary.DefaultIfEmpty()
                        join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
                        from d in second.DefaultIfEmpty()
                        where a.Unassemble == (long)unasId && (BOMArray.Contains(e.BOMId))
                        select new
                        {
                            a.UnItemId,
                            a.ItemId,
                            a.Quantity,
                            a.Unit,
                           // bqty = a.Quantity,
                            bqty = e.Quantity,
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
                            BOMId = e.BOMId,

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
                            o.UnItemId,
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
                            price = (o.PurchasePrice != 0) ? o.PurchasePrice : o.MRP,
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


                        }).ToList();


            return LegacyJson(new { unas, item, bom });
        }

        private long GetVchNo()
        {
            Int64 PENo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "Unassemble").Select(a => a.number).FirstOrDefault();
            if ((db.Unassembles.Select(p => p.EntryNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
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
                PENo = db.Unassembles.Max(p => p.EntryNo + 1);
            }
            return PENo;
        }
        private string VoucherNo(Int64 SENo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "Unassemble").Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == "Unassemble").Select(a => a.number).FirstOrDefault();
            if (billNo == null)
            {
                if ((db.Unassembles.Select(p => p.EntryNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    SENo = db.Unassembles.Max(p => p.EntryNo + 1);
                    billNo = companyPrefix + SENo;
                    if (BillExist(billNo))
                    {
                        billNo = VoucherNo(SENo, billNo);
                    }
                }
            }
            else
            {
                SENo = SENo + 1;
                billNo = companyPrefix + SENo;
                if (BillExist(billNo))
                {
                    billNo = VoucherNo(SENo, billNo);
                }

            }
            return billNo;
        }
        private bool BillExist(string SENo)
        {
            var Exists = db.Unassembles.Any(c => c.VoucherNo == SENo);
            bool res = (Exists) ? true : false;
            return res;
        }
        [HttpPost]
        public JsonResult SearchItemById(long? ItemId, long? entryId)
        {
            var v = (from b in db.BillOfMaterials
                     join c in db.ItemUnits on b.Unit equals c.ItemUnitID into primary
                     from c in primary.DefaultIfEmpty()
                     where b.ItemId == ItemId
                     select new
                     {
                         b.BOMName,
                         b.BOMId,
                         b.ItemId,
                         c.ItemUnitName,
                         b.Unit
                     }).ToList();

            var data = (from o in v
                        select new
                        {
                            Item = o.ItemId,
                            o.ItemUnitName,
                            o.BOMName,
                            o.Unit,
                            o.BOMId,
                            BOMItem = (from ab in db.BOMItems
                                       join bb in db.Items on ab.ItemId equals bb.ItemID
                                       join cb in db.ItemUnits on ab.Unit equals cb.ItemUnitID into bprimary
                                       from cb in bprimary.DefaultIfEmpty()
                                       where ab.BOMId == o.BOMId
                                       select new
                                       {
                                           Item = bb.ItemID,
                                           bb.ItemCode,
                                           bb.ItemName,
                                           ItemWithCode = bb.ItemCode + " - " + bb.ItemName,
                                           bb.ItemUnitID,
                                           bb.SubUnitId,
                                           OpeningStock = bb.OpeningStock,
                                           MinStock = (bb.MinStock != null) ? bb.MinStock : 0,
                                           bb.ConFactor,
                                           bb.SellingPrice,
                                           bb.PurchasePrice,
                                           bb.BasePrice,
                                           bb.MRP,
                                           price = (bb.SellingPrice != 0) ? bb.SellingPrice : bb.MRP,
                                           bb.KeepStock,
                                           cb.ItemUnitName,
                                           id = bb.ItemID,
                                           text = bb.ItemCode + " - " + bb.ItemName,
                                           BaseQty = ab.Quantity,
                                           ab.Unit,
                                           UnitName = (ab.Unit != null) ? cb.ItemUnitName : "",
                                           ab.BOMId
                                       }).ToList(),
                        }).ToList();
            return Json(data);
        }
        [HttpGet]
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "Unassemble" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
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

            var MR = db.Unassembles.Where(a => a.UnassembleId == id).FirstOrDefault();

            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "Unassemble").OrderByDescending(a => a.CreatedDate).FirstOrDefault();

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
                AppUp.Type = "Unassemble";

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
                            join d in db.Unassembles on b.TransEntry equals d.UnassembleId into team
                            from d in team.DefaultIfEmpty()
                            join e in db.Employees on b.RequestBy equals e.UserId into emp
                            from e in emp.DefaultIfEmpty()
                            join u in db.Users on d.CreatedBy equals u.Id into req
                            from u in req.DefaultIfEmpty()
                            where b.TransEntry == MCId && b.Type == "Unassemble"
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

        //Function To Update Consumed & Unassemble Item Quantity in ItemTransaction Table(Modes==>Create, Edit, Delete)
        public ActionResult ItemTransaction(string Action, long? MC, UnasViewModel ViewModel, string UserId, DateTime CurrentDate, long? UnAssembleId)
        {
            /********************************* Create Mode *********************************/
            if (Action == "Create")
            {
                decimal ConvrtdQty = 0, Quantity = 0;
                long UnitId = 0, ItemId = 0;

                //=========>For Consumed Items   
                foreach (var ConsumedItem in ViewModel.unasconsumed)
                {
                    ItemId = ConsumedItem.ItemId;
                    UnitId = Convert.ToInt64(ConsumedItem.ItemUnit);
                    Quantity = ConsumedItem.Quantity;

                    ConvrtdQty = (from b in db.Items
                                  where (b.ItemID == ItemId)
                                  select (UnitId == b.ItemUnitID) ?
                                      (b.SubUnitId != null) ? (Quantity * b.ConFactor) : Quantity
                                      : Quantity).FirstOrDefault();

                    //Subtract the Consumed Item Quantity
                    ItemTransactions McItemObj = db.ItemTransactions.Where(a => (a.ItemId == ItemId && a.McId == MC)).FirstOrDefault();

                    if (McItemObj == null)
                        //Add Quantity to ItemTransaction Table
                        com.AddItemTransaction(ItemId, MC, -ConvrtdQty, UserId, CurrentDate);
                    else
                        com.UpdateItemTransaction(ItemId, MC, -ConvrtdQty, UserId, CurrentDate);
                }

                //=========>For Unassemble Items
                ItemId = 0; UnitId = 0; Quantity = 0; ConvrtdQty = 0;

                foreach (var UnAssmbItem in ViewModel.unasitem)
                {
                    ItemId      =   UnAssmbItem.ItemId;
                    UnitId      =   Convert.ToInt64(UnAssmbItem.Unit);
                    Quantity    =   UnAssmbItem.Quantity;

                    ConvrtdQty = (from b in db.Items
                                  where (b.ItemID == ItemId)
                                  select (UnitId == b.ItemUnitID) ?
                                     (b.SubUnitId != null) ? (Quantity * b.ConFactor) : Quantity
                                     : Quantity).FirstOrDefault();

                    //Add the UnAssembled Item Quantity
                    ItemTransactions McItemObj = db.ItemTransactions.Where(a => (a.ItemId == ItemId && a.McId == MC)).FirstOrDefault();

                    if (McItemObj == null)
                        //Add Quantity to ItemTransaction Table
                        com.AddItemTransaction(ItemId, MC, ConvrtdQty, UserId, CurrentDate);
                    else
                        com.UpdateItemTransaction(ItemId, MC, ConvrtdQty, UserId, CurrentDate);
                }
            }
            /***********************************************************************************/

            /*************************************** Edit Mode *********************************/
            if (Action == "Edit")
            {
                long    PrevItemId = 0, ItemId = 0, UnitId = 0;
                long?   PrevMc = 0;
                decimal PreviousQty = 0, ConvrtdQty = 0, Quantity = 0;

                //=========>For Consumed Items 
                var PrevConsItems = (from a in db.ConsumedItem
                                    join b in db.Items on a.Item equals b.ItemID
                                    join c in db.Unassembles
                                    on a.Unassemble equals c.UnassembleId
                                    where (c.UnassembleId == UnAssembleId)
                                    select new
                                    {
                                        ItemId          =   a.Item,
                                        Quantity        =   ((a.Unit == b.ItemUnitID) ?
                                                            (b.SubUnitId != null) ? (a.Qty * b.ConFactor) : a.Qty
                                                            : a.Qty),
                                        MaterialCenter  =   c.MaterialCenter,
                                    }).ToList();

                PrevMc = PrevConsItems.Select(x => x.MaterialCenter).FirstOrDefault();

                /*********** Delete last Transaction from ItemTransaction *********/
                foreach (var prevrow in PrevConsItems)
                {
                    PrevItemId  = prevrow.ItemId;
                    PreviousQty = prevrow.Quantity;

                    //Subtract the Previous consumed Item quantity in previous Material Centre                  
                    com.UpdateItemTransaction(PrevItemId, PrevMc, PreviousQty, UserId, CurrentDate);
                }

                /*********** Insert new transaction into ItemTransaction *********/
                foreach (var ConsumedItem in ViewModel.unasconsumed)
                {
                    ItemId      =   ConsumedItem.ItemId;
                    UnitId      =   Convert.ToInt64(ConsumedItem.ItemUnit);
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

                //=========>For UnAssembled Items 
                PrevItemId = 0; PrevMc = 0; PreviousQty = 0; ItemId = 0; UnitId = 0; Quantity = 0; ConvrtdQty = 0;

                var PrevUnAssmbItems = (from a in db.UnassembleItems
                                        join b in db.Items on a.ItemId equals b.ItemID
                                         join c in db.Unassembles
                                         on a.Unassemble equals c.UnassembleId
                                         where (c.UnassembleId == UnAssembleId)
                                         select new
                                         {
                                             ItemId         =   a.ItemId,
                                             Quantity       =   ((a.Unit == b.ItemUnitID) ?
                                                                    (b.SubUnitId != null) ? (a.Quantity * b.ConFactor) : a.Quantity
                                                                    : a.Quantity),
                                             MaterialCenter =   c.MaterialCenter,
                                         }).ToList();

                PrevMc = PrevUnAssmbItems.Select(x => x.MaterialCenter).FirstOrDefault();

                /*********** Delete last Transaction from ItemTransaction *********/
                foreach (var prevrow in PrevUnAssmbItems)
                {
                    PrevItemId  = prevrow.ItemId;
                    PreviousQty = prevrow.Quantity;

                    //Subtract the Previous UnAssembled Item quantity in previous Material Centre                  
                    com.UpdateItemTransaction(PrevItemId, PrevMc, -PreviousQty, UserId, CurrentDate);
                }

                /*********** Insert new transaction into ItemTransaction *********/
                foreach (var UnAssmbdItem in ViewModel.unasitem)
                {
                    ItemId      =   UnAssmbdItem.ItemId;
                    UnitId      =   Convert.ToInt64(UnAssmbdItem.Unit);
                    Quantity    =   UnAssmbdItem.Quantity;

                    ConvrtdQty = (from b in db.Items
                                  where (b.ItemID == ItemId)
                                  select (UnitId == b.ItemUnitID) ?
                                          (b.SubUnitId != null) ? (Quantity * b.ConFactor) : Quantity
                                          : Quantity).FirstOrDefault();

                    //Add the UnAssembled Item Quantity
                    ItemTransactions McItemObj = db.ItemTransactions.Where(a => (a.ItemId == ItemId && a.McId == MC)).FirstOrDefault();

                    if (McItemObj == null)
                        //Add Quantity to ItemTransaction Table
                        com.AddItemTransaction(ItemId, MC, ConvrtdQty, UserId, CurrentDate);
                    else
                        com.UpdateItemTransaction(ItemId, MC, ConvrtdQty, UserId, CurrentDate);
                }
            }
            /***********************************************************************************/

            /************************************ Delete Mode *********************************/
            if (Action == "Delete")
            {
                long ItemId = 0;
                decimal Quantity = 0;

                //===========>For Consumed Items 
                var ConItemList = (from a in db.ConsumedItem
                                   join b in db.Items on a.Item equals b.ItemID
                                   where (a.Unassemble == UnAssembleId)
                                   select new
                                   {
                                       ItemId   = a.Item,
                                       Quantity = (a.Unit == b.ItemUnitID) ?
                                                       (b.SubUnitId != null) ? (a.Qty * b.ConFactor) : a.Qty
                                                       : a.Qty
                                   }).ToList();

                if (ConItemList != null && ConItemList.Count != 0)
                {
                    foreach (var row in ConItemList)
                    {
                        ItemId      =   row.ItemId;
                        Quantity    =   row.Quantity;

                        //Add the Consumed Item Quantity
                        com.UpdateItemTransaction(ItemId, MC, Quantity, UserId, CurrentDate);
                    }
                }

                //=========>For UnAssembled Items 
                ItemId = 0; Quantity = 0;

                var UnAssmbItemList =   (from a in db.UnassembleItems
                                         join b in db.Items on a.ItemId equals b.ItemID
                                          where (a.Unassemble == UnAssembleId)
                                          select new
                                          {
                                              ItemId    =   a.ItemId,
                                              Quantity  =   (a.Unit == b.ItemUnitID) ?
                                                              (b.SubUnitId != null) ? (a.Quantity * b.ConFactor) : a.Quantity
                                                              : a.Quantity
                                          }).ToList();

                if (UnAssmbItemList != null && UnAssmbItemList.Count != 0)
                {
                    foreach (var row in UnAssmbItemList)
                    {
                        ItemId      =   row.ItemId;
                        Quantity    =   row.Quantity;

                        //Subtract the UnAssembled Item Quantity                                    
                        com.UpdateItemTransaction(ItemId, MC, -Quantity, UserId, CurrentDate);
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
