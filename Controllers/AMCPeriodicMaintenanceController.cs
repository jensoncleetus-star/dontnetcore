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
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic;
using System.Data;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Drawing;
namespace QuickSoft.Controllers
{
    public class AMCPeriodicMaintenanceController : BaseController
    {
        ApplicationDbContext db;
        Common com;

        public AMCPeriodicMaintenanceController()
        {
            db  = new ApplicationDbContext();
            com = new Common();
        }

        // GET: AMCPeriodicMaintenance
        public ActionResult Index()
        {
            ViewBag.DropDowns = QkSelect.List(
                      new List<SelectListItem>
                      {
                        new SelectListItem { Selected = false, Text = "All", Value = "0"},
                      }, "Value", "Text", 1);
            return View();
        }

        // GET: AMCPeriodicMaintenance Create
        [RedirectingAction]
        [HttpGet]
        public ActionResult Create(long? id)
        {
            ViewBag.Dropdowns = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = null, Value = null},
                            }, "Value", "Text", 1);

            var use = db.Employees
                   .Select(s => new
                   {
                       ID = s.EmployeeId,
                       Name = s.FirstName + " " + s.LastName
                   })
                   .ToList();
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");

            ViewBag.AssignTypes = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                              }, "Value", "Text", 0);
           
            AmcPeriodicMaintenanceViewModel ViewModel = new AmcPeriodicMaintenanceViewModel
            {
                PMaintNo = GetPMaintNo(),
                PDate = System.DateTime.Now.ToString("dd-MM-yyyy"),
            };                      


            ViewBag.NewDate = System.DateTime.Now.ToString("dd-MM-yyyy");
            ViewBag.Mode = "Create";            

            //To fill details in Edit Mode
            if(id != null )
            {

              var FillDtls =   (from a in db.PeriodicMaintenances
                                join b in db.PeriodicMaintenanceDetails
                                on a.PeriodicMaintenanceId equals b.PeriodicMaintenanceId
                                join c in db.Amcs
                                on a.AmcId equals c.AmcId
                                join d in db.Customers
                                on c.CustomerId equals d.CustomerID
                                join e in db.AmcContracts
                                on c.ContractId equals e.ContractId
                                where b.PeriodicMaintDetailsId == id
                                select new
                                {
                                    PerdcMaintNo = a.PeriodicMaintenanceNo,
                                    AmcId = a.AmcId,
                                    CustomerName = d.CustomerName,
                                    ContractName = e.ContractName,
                                    PDate = b.PDate,
                                    Notes = b.Notes
                                }).FirstOrDefault();

                if (FillDtls != null)
                {
                    ViewModel.PMainDetailId = Convert.ToInt64(id);
                    ViewModel.AmcId = FillDtls.AmcId;
                    ViewModel.PMaintNo = FillDtls.PerdcMaintNo;
                    ViewModel.CustomerName = FillDtls.CustomerName;
                    ViewModel.ContractName = FillDtls.ContractName;
                    ViewModel.PDate = FillDtls.PDate.ToString("dd-MM-yyyy");
                    ViewModel.Notes = FillDtls.Notes;
                }

                //Amcs
                var Amcs = (from a in db.Amcs
                            join b in db.Customers
                            on a.CustomerId equals b.CustomerID
                            select new
                            {
                                Id      = a.AmcId,
                                Name    = a.AmcNo.ToString() + " - " + b.CustomerName,
                            }).ToList();
                ViewBag.Amcs = QkSelect.List(Amcs, "Id", "Name");
              
                //Assign Team
                ViewModel.AssignTypeAll = db.PeriodicMaintAssignedTeams.Where(a => a.PeriodicMaintDtlId == id).Select(a => a.TeamId).ToList().ToArray() ?? null;

                var Teams = db.Teams
                        .Select(s => new
                        {
                            ID = s.TeamId,
                            Name = s.TeamName
                        })
                        .ToList();
                ViewBag.AssignTeam = new MultiSelectList(Teams, "ID", "Name", ViewModel.AssignTypeAll);

                //Assigned Too
                ViewModel.AssignedTo = db.PeriodicMaintAssignedToes.Where(x => x.PeriodicMaintDtlId == id && x.Status == "Assigned" && x.ChkStatus == (int)Status.active).Select(a => a.EmployeeId).ToList().ToArray() ?? null;

                var TMembers = db.Employees
                       .Select(s => new
                       {
                           id = s.EmployeeId,
                           text = s.FirstName + " " + s.LastName
                       })
                       .ToList();
                ViewBag.AssignTo = new MultiSelectList(TMembers, "id", "text", ViewModel.AssignedTo);
                ViewBag.Mode = "Edit";                
            }

            return View(ViewModel);
        }
        [RedirectingAction]
        [HttpGet]
        public ActionResult Edit(long? id)
        {

            ViewBag.Dropdowns = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = null, Value = null},
                            }, "Value", "Text", 1);

            var use = db.Employees
                   .Select(s => new
                   {
                       ID = s.EmployeeId,
                       Name = s.FirstName + " " + s.LastName
                   })
                   .ToList();
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");

            ViewBag.AssignTypes = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                              }, "Value", "Text", 0);

            AmcPeriodicMaintenanceViewModel ViewModel = new AmcPeriodicMaintenanceViewModel
            {
                PMaintNo = GetPMaintNo(),
                PDate = System.DateTime.Now.ToString("dd-MM-yyyy"),
            };


            ViewBag.NewDate = System.DateTime.Now.ToString("dd-MM-yyyy");
            ViewBag.Mode = "Edit";

            //To fill details in Edit Mode
            if (id != null)
            {

                var FillDtls = (from a in db.PeriodicMaintenances
                                join b in db.PeriodicMaintenanceDetails
                                on a.PeriodicMaintenanceId equals b.PeriodicMaintenanceId
                                join c in db.Amcs
                                on a.AmcId equals c.AmcId
                                join d in db.Customers
                                on c.CustomerId equals d.CustomerID
                                join e in db.AmcContracts
                                on c.ContractId equals e.ContractId
                                where b.PeriodicMaintDetailsId == id
                                select new
                                {
                                    PerdcMaintNo = a.PeriodicMaintenanceNo,
                                    AmcId = a.AmcId,
                                    CustomerName = d.CustomerName,
                                    ContractName = e.ContractName,
                                    PDate = b.PDate,
                                    Notes = b.Notes
                                }).FirstOrDefault();

                if (FillDtls != null)
                {
                    ViewModel.PMainDetailId = Convert.ToInt64(id);
                    ViewModel.AmcId = FillDtls.AmcId;
                    ViewModel.PMaintNo = FillDtls.PerdcMaintNo;
                    ViewModel.CustomerName = FillDtls.CustomerName;
                    ViewModel.ContractName = FillDtls.ContractName;
                    ViewModel.PDate = FillDtls.PDate.ToString("dd-MM-yyyy");
                    ViewModel.Notes = FillDtls.Notes;
                }

                //Amcs
                var Amcs = (from a in db.Amcs
                            join b in db.Customers
                            on a.CustomerId equals b.CustomerID
                            select new
                            {
                                Id = a.AmcId,
                                Name = a.AmcNo.ToString() + " - " + b.CustomerName,
                            }).ToList();
                ViewBag.Amcs = QkSelect.List(Amcs, "Id", "Name");

                //Assign Team
                ViewModel.AssignTypeAll = db.PeriodicMaintAssignedTeams.Where(a => a.PeriodicMaintDtlId == id).Select(a => a.TeamId).ToList().ToArray() ?? null;

                var Teams = db.Teams
                        .Select(s => new
                        {
                            ID = s.TeamId,
                            Name = s.TeamName
                        })
                        .ToList();
                ViewBag.AssignTeam = new MultiSelectList(Teams, "ID", "Name", ViewModel.AssignTypeAll);

                //Assigned Too
                ViewModel.AssignedTo = db.PeriodicMaintAssignedToes.Where(x => x.PeriodicMaintDtlId == id && x.Status == "Assigned" && x.ChkStatus == (int)Status.active).Select(a => a.EmployeeId).ToList().ToArray() ?? null;

                var TMembers = db.Employees
                       .Select(s => new
                       {
                           id = s.EmployeeId,
                           text = s.FirstName + " " + s.LastName
                       })
                       .ToList();
                ViewBag.AssignTo = new MultiSelectList(TMembers, "id", "text", ViewModel.AssignedTo);
                ViewBag.Mode = "Edit";
            }

            return View(ViewModel);
        }

        //Function to display Periodic Maintenance Details 
        [HttpGet]
        public JsonResult GetFillDetails(long AmcId)
        {
               Int64 MaintNo = GetPMaintNo();
               var ConD = (from a in db.Amcs
                           join b in db.PeriodicMaintenances
                           on a.AmcId equals b.AmcId into primary
                           from b in primary.DefaultIfEmpty()
                           join d in db.Customers
                           on a.CustomerId equals d.CustomerID
                           join e in db.AmcContracts
                           on a.ContractId equals e.ContractId
                           where a.AmcId == AmcId
                            select new
                            {
                                a.AmcId,
                                PMaintId    = (b.PeriodicMaintenanceId != null)? b.PeriodicMaintenanceId : 0,
                                PMaintNo    = (b.PeriodicMaintenanceNo != null) ? b.PeriodicMaintenanceNo : 0,
                                d.CustomerName,
                                e.ContractName,
                                NewMaintNo = MaintNo
                            });
            return Json(ConD);     
        }

        //Function for Index--POST
        [HttpPost]
        [RedirectingAction]
        public ActionResult GetPeriodicMaintDtls(long AmcId)
        {
            string search   =   Request.Form.GetValues("search[value]")[0];
            var draw        =   Request.Form.GetValues("draw").FirstOrDefault();
            var start       =   Request.Form.GetValues("start").FirstOrDefault();
            var length      =   Request.Form.GetValues("length").FirstOrDefault();

            //Find Order Column
            var sortColumn      =   Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir   =   Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            // SERVER: only translatable columns. The latest-AmcUpdation join and the nested AssignedTo
            // collection are computed CLIENT-side below (EF Core 10 can't translate either inside this query).
            var serverQuery = (from a in db.PeriodicMaintenances
                     join b in db.PeriodicMaintenanceDetails
                     on a.PeriodicMaintenanceId equals b.PeriodicMaintenanceId
                     join c in db.Amcs on a.AmcId equals c.AmcId
                     join d in db.AmcStatuss on b.PeriodicMaintStatus equals d.AmcStatusId into temp1
                     from d in temp1.DefaultIfEmpty()
                     where a.AmcId == AmcId
                     select new
                     {
                        a.AmcId,
                        b.PDate,
                        b.Notes,
                        PMaintId    =   a.PeriodicMaintenanceId,
                        PerMaintDetailsId  =   b.PeriodicMaintDetailsId,
                        StatusName = d.StatusName,
                        LogTime = b.LogTime,
                     });

            // Performance (audit P2): server-side paging on the common path; search/computed sorts fall back unchanged.
            var serverSortable = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase){ "AmcId","LogTime","Notes","PDate","PerMaintDetailsId","PMaintId","StatusName" };
            bool fastPage = string.IsNullOrWhiteSpace(search) && pageSize > 0 && (string.IsNullOrEmpty(sortColumn) || serverSortable.Contains(sortColumn));
            if (fastPage)
            {
                recordsTotal = serverQuery.Count();
                serverQuery = string.IsNullOrEmpty(sortColumn) ? serverQuery.OrderBy("AmcId asc") : serverQuery.OrderBy(sortColumn + " " + (string.IsNullOrEmpty(sortColumnDir) ? "asc" : sortColumnDir));
                serverQuery = serverQuery.Skip(skip).Take(pageSize);
            }
            var serverRows = serverQuery.ToList();

            // CLIENT: latest AmcUpdation per PeriodicMaintenanceDetail, keyed by PeriodicMaintDetailsId.
            var pmIds = serverRows.Select(o => o.PerMaintDetailsId).ToList();
            var periodicUpLookup = db.AmcUpdations
                .Where(u => u.TransType == "PeriodicMaintenance" && pmIds.Contains(u.TransId))
                .ToList().GroupBy(u => u.TransId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.CreatedDate).First());
            var pmAssignLookup = (from z in db.PeriodicMaintAssignedToes
                                  join y in db.Employees on z.EmployeeId equals y.EmployeeId
                                  where z.Status == "Assigned" && z.ChkStatus == Status.active && pmIds.Contains(z.PeriodicMaintDtlId)
                                  select new { z.PeriodicMaintDtlId, y.EmployeeId, y.LastName, y.FirstName, y.MiddleName, y.ImgFileName, y.Status })
                                 .ToList().ToLookup(x => x.PeriodicMaintDtlId);

            var v = serverRows.Select(o =>
            {
                DateTime? ldateVal = o.LogTime;
                if (periodicUpLookup.TryGetValue(o.PerMaintDetailsId, out var up) && up.CreatedDate > o.LogTime)
                    ldateVal = up.CreatedDate;
                return new
                {
                    o.AmcId,
                    o.PDate,
                    o.Notes,
                    PMaintId = o.PMaintId,
                    PerMaintDetailsId = o.PerMaintDetailsId,
                    StatusName = o.StatusName,
                    AssignedTo = pmAssignLookup[o.PerMaintDetailsId].Select(y => new
                    {
                        id = y.EmployeeId,
                        LastName = (y.LastName != null) ? y.LastName : "",
                        FirstName = (y.FirstName != null) ? y.FirstName : "",
                        MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                        Img = (y.ImgFileName != null) ? y.ImgFileName : "",
                        y.Status
                    }).Distinct().ToList(),
                    ldate = ldateVal,
                };
            }).OrderByDescending(a => a.ldate).ToList();
            //SORT
            if (!fastPage && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir).ToList();
            }

            if (!fastPage) { recordsTotal = v.Count(); }
            var data = (fastPage ? v.ToList() : v.Skip(skip).Take(pageSize).ToList());
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data});
        }

        //Function To Get Periodic Maintenance No
        private long GetPMaintNo()
        {
            Int64 PMaintNo = 0, LastNo = 0 ;

            LastNo = db.PeriodicMaintenances.Select(p => p.PeriodicMaintenanceNo).DefaultIfEmpty().Max();

            if (LastNo == 0)
                PMaintNo = 1000;
            else
                PMaintNo = LastNo + 1;

            return PMaintNo;
        }

        //Saving-- CREATE ---POST
        [HttpPost]
        public JsonResult Create(AmcPeriodicMaintenanceViewModel PerdcViewModel)
        {           
            Int64 AmcId = PerdcViewModel.AmcId;
            string msg;
            bool stat;
            var DataExists = db.PeriodicMaintenances.Any(u => u.PeriodicMaintenanceNo == PerdcViewModel.PMaintNo && u.AmcId != AmcId);
            if (DataExists)
            {
              
                stat = false;
                msg = "A Periodic Maintenance with same Periodic Sl No. exists...";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                if (ModelState.IsValid)
                {
                    Int64 PerdcMainId = 0, PDetailId = 0;
                    
                    var UserId      =   User.Identity.GetUserId();
                    DateTime Today  =   System.DateTime.Now;                    
                    DateTime PDate  =   DateTime.Parse(PerdcViewModel.PDate.ToString(), new CultureInfo("en-GB"));
                                       
                    /***************************  Create Mode *******************************/
                    if (PerdcViewModel.PMainDetailId == 0 || PerdcViewModel.PMainDetailId == null)
                    {
                        PeriodicMaintenance PMasterObj = db.PeriodicMaintenances.Where(a => a.AmcId == AmcId).FirstOrDefault();

                        /********************* PeriodicMaintenances ************************/
                        //If Already Periodic Maintenance Exists, Edit The Existing PeriodicMaintenance table
                        if (PMasterObj != null)
                        {
                            PMasterObj.NoOfPMaintenance = PMasterObj.NoOfPMaintenance + 1;
                            PMasterObj.LogTime = Today;

                            db.Entry(PMasterObj).State = EntityState.Modified;
                            db.SaveChanges();

                            PerdcMainId = PMasterObj.PeriodicMaintenanceId;
                        }
                        //If No Periodic Maintenance Exists, Add to PeriodicMaintenance table
                        else
                        {
                            PeriodicMaintenance MasterObj = new PeriodicMaintenance();

                            MasterObj.PeriodicMaintenanceNo =   GetPMaintNo();
                            MasterObj.AmcId                 =   AmcId;
                            MasterObj.NoOfPMaintenance      =   1;
                            MasterObj.CreatedDate           =   Today;
                            MasterObj.LogTime               =   Today;

                            db.PeriodicMaintenances.Add(MasterObj);
                            db.SaveChanges();

                            PerdcMainId = MasterObj.PeriodicMaintenanceId;

                            //To Update 'PeriodicMaintReqrd' column of Amc Table while adding new data
                            Amc AmcObj = db.Amcs.Find(AmcId);

                            AmcObj.PeriodicMaintReqrd = true;
                            AmcObj.LogTime            = Today;

                            db.Entry(AmcObj).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        /****************** PeriodicMaintenanceDetail **********************/
                        //Add new row to PeriodicMaintenanceDetail table
                        PeriodicMaintenanceDetail NwDetObj = new PeriodicMaintenanceDetail();

                        NwDetObj.PeriodicMaintenanceId  =   PerdcMainId;
                        NwDetObj.PDate                  =   PDate;
                        NwDetObj.Notes                  =   PerdcViewModel.Notes;
                        NwDetObj.CreatedDate            =   Today;
                        NwDetObj.LogTime                =   Today;                       

                        db.PeriodicMaintenanceDetails.Add(NwDetObj);
                        db.SaveChanges();

                        PDetailId = NwDetObj.PeriodicMaintDetailsId;
                        var exist = db.ProTasks.Any(o => o.VManuId == PDetailId);//o.StartDate == row.PDate && o.TaskName.Contains("AMC - Periodic Maintenance"));
                        if (!exist)
                        {
                            var existpro = db.ProTasks.Any(o => o.VModId == AmcId);//o.StartDate == row.PDate && o.TaskName.Contains("AMC - Periodic Maintenance"));

                           
                            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                            var today = Convert.ToDateTime(System.DateTime.Now);
                            if (1==1)
                            {
                                Amc AmcObj = db.Amcs.Find(AmcId);

                                ProTask amctask = new ProTask();
                                amctask.TaskNo = GetProNo();
                                amctask.TaskCode = InvoiceNo();
                                amctask.TaskName = "AMC - Periodic Maintenance " + " Customer :" + db.Customers.Find(AmcObj.CustomerId).CustomerName + " Date : " + PDate;
                                amctask.StartDate = PDate;
                                amctask.CreatedDate = PDate;
                                amctask.CreatedBy = UserId;
                                amctask.Status = Status.active;
                                amctask.Lattitude = AmcObj.Lattitude;
                                amctask.Longitude = AmcObj.Longitude;
                                amctask.Branch = BranchID;
                                amctask.logtime = PDate;
                                amctask.CustomerID = AmcObj.CustomerId;
                                amctask.VManuId = PDetailId;
                                amctask.VModId = AmcId;
                                var tasktype = db.ProTaskTypes.Where(o => o.TypeName == "PERIODIC MAINTEANCE").Select(o => o.TaskTypeId).FirstOrDefault();
                                if (tasktype != null && tasktype != 0)
                                {
                                    amctask.TaskType = tasktype;
                                }

                                db.ProTasks.Add(amctask);
                                db.SaveChanges();
                                Int64 proId = amctask.ProTaskId;
                                ProTaskUpdation TaskUp = new ProTaskUpdation
                                {
                                    ProTaskId = proId,
                                    //Status = TKUpdateStatus.Created,
                                    CreatedBy = UserId,
                                    CreatedDate = today,
                                    //TaskTeamId = teamId
                                };
                                db.ProTaskUpdations.Add(TaskUp);
                                db.SaveChanges();
                                Int64 TaskUpdId = TaskUp.TaskUpdationID;
                          
                            }

                        }
                        /************************ Assigned Team ***************************/
                        if (PerdcViewModel.AssignTypeAll != null)
                        {
                            PeriodicMaintAssignedTeam TeamObj = new PeriodicMaintAssignedTeam();
                            foreach (var arr in PerdcViewModel.AssignTypeAll)
                            {
                                TeamObj.PeriodicMaintDtlId = Convert.ToInt64(PDetailId);
                                TeamObj.TeamId = arr;
                                db.PeriodicMaintAssignedTeams.Add(TeamObj);
                                db.SaveChanges();
                            }
                        }

                        /************************* Assigned To ***************************/
                        if (PerdcViewModel.AssignedTo != null)
                        {
                            PeriodicMaintAssignedTo AssignTo = new PeriodicMaintAssignedTo();

                            foreach (var arr in PerdcViewModel.AssignedTo)
                            {
                                AssignTo.PeriodicMaintDtlId =   Convert.ToInt64(PDetailId);
                                AssignTo.EmployeeId         =   arr;
                                AssignTo.Status             =   "Assigned";
                                AssignTo.AssignBy           =   UserId;
                                AssignTo.ChkStatus          =   Status.active;
                                AssignTo.CreatedDate        =   Convert.ToDateTime(Today);
                                db.PeriodicMaintAssignedToes.Add(AssignTo);
                                db.SaveChanges();
                            }
                        }
                    }
                    /******************************* Edit Mode *****************************/
                    else
                    {
                        /****************** PeriodicMaintenanceDetail **********************/
                        //Edit existing  row in PeriodicMaintenanceDetail table
                        PDetailId = PerdcViewModel.PMainDetailId;

                        PeriodicMaintenanceDetail PDtlObj = db.PeriodicMaintenanceDetails.Find(PDetailId);
                        
                        if (PDtlObj != null)
                        {
                            PDtlObj.PDate   =   PDate;
                            PDtlObj.Notes   =   PerdcViewModel.Notes;
                            PDtlObj.LogTime =   Today;

                            db.Entry(PDtlObj).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        
                        var NewAmcStat = GetStatusName(PDtlObj.PeriodicMaintStatus);

                        /*********************** Periodic Updations *************************/
                        var statusId = db.AmcUpdations.Where(a => a.TransId == PDetailId && a.TransType == "PeriodicMaintenance");

                        if (statusId != null)
                        {
                            db.AmcUpdations.RemoveRange(db.AmcUpdations.Where(a => a.TransId == PDetailId && a.TransType == "PeriodicMaintenance"));
                            db.SaveChanges();
                        }
                        
                        AmcUpdation PeriodicUps = new AmcUpdation
                        {
                            TransId     =   PDetailId,
                            TransType   =   "PeriodicMaintenance",
                            CreatedBy   =   UserId,
                            CreatedDate =   Today,
                            Remarks     =   NewAmcStat,
                        };
                        db.AmcUpdations.Add(PeriodicUps);
                        db.SaveChanges();

                        Int64 AmcUpdationID = PeriodicUps.UpdationID;

                        /************************ Assigned Team ***************************/
                        
                        //********** Remove From Assigned Team 
                        var AssgnTeams = db.PeriodicMaintAssignedTeams.Where(a => a.PeriodicMaintDtlId == PDetailId).FirstOrDefault();

                        if (AssgnTeams != null)
                        {
                            db.PeriodicMaintAssignedTeams.RemoveRange(db.PeriodicMaintAssignedTeams.Where(a => a.PeriodicMaintDtlId == PDetailId));
                            db.SaveChanges();
                        }
                        
                        if (PerdcViewModel.AssignTypeAll != null)
                        {
                            PeriodicMaintAssignedTeam TeamObj = new PeriodicMaintAssignedTeam();
                            foreach (var arr in PerdcViewModel.AssignTypeAll)
                            {
                                TeamObj.PeriodicMaintDtlId = Convert.ToInt64(PDetailId);
                                TeamObj.TeamId = arr;
                                db.PeriodicMaintAssignedTeams.Add(TeamObj);
                                db.SaveChanges();
                            }
                        }

                        /************************* Assigned To ***************************/
                        if (PerdcViewModel.AssignedTo != null)
                        {
                            var PrevAsgn = db.PeriodicMaintAssignedToes.Where(a => a.PeriodicMaintDtlId == PDetailId && a.Status == "Assigned" && a.ChkStatus == Status.active).Select(a => a.EmployeeId).ToArray();
                            var NewUsers = PerdcViewModel.AssignedTo.ToArray();

                            PeriodicMaintAssignedTo NewObj = new PeriodicMaintAssignedTo();

                            foreach (var arr in PrevAsgn)
                            {
                                var AssgndToId = db.PeriodicMaintAssignedToes.Where(a => a.PeriodicMaintDtlId == PDetailId && a.Status == "Assigned" && a.ChkStatus == Status.active && a.EmployeeId == arr).Select(a => a.AssignedToId).FirstOrDefault();
                                PeriodicMaintAssignedTo tskassr = db.PeriodicMaintAssignedToes.Find(AssgndToId);

                                //for removed members
                                if (!NewUsers.Contains(arr))
                                {
                                    tskassr.ChkStatus = Status.inactive;
                                    db.Entry(tskassr).State = EntityState.Modified;
                                    db.SaveChanges();

                                    NewObj.PeriodicMaintDtlId   =   PDetailId;
                                    NewObj.EmployeeId           =   arr;
                                    NewObj.Status               =   "Removed";
                                    NewObj.AssignBy             =   UserId;
                                    NewObj.CreatedDate          =   Convert.ToDateTime(Today).AddMilliseconds(100); ;
                                    db.PeriodicMaintAssignedToes.Add(NewObj);
                                    db.SaveChanges();
                                }
                                //for existing members--Updating status
                                else
                                {
                                    tskassr.ChkStatus = Status.inactive;
                                    db.Entry(tskassr).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }

                            foreach (var arr in PerdcViewModel.AssignedTo)
                            {
                                NewObj.PeriodicMaintDtlId   =   PDetailId;
                                NewObj.EmployeeId           =   arr;
                                NewObj.Status               =   "Assigned";
                                NewObj.AssignBy             =   UserId;
                                NewObj.ChkStatus            =   Status.active;
                                NewObj.CreatedDate          =   Convert.ToDateTime(Today).AddMilliseconds(100);
                                db.PeriodicMaintAssignedToes.Add(NewObj);
                                db.SaveChanges();
                            }
                        }
                    }

                    PerdcViewModel.PMainDetailId = 0;
                    com.addlog(LogTypes.Created, UserId, "AMCPeriodicMaintenance", "PeriodicMaintenances", findip(), PerdcMainId, "Periodic Maintenance added successfully");
                    stat = true;
                    msg = "Successfully added Periodic Maintenance Details..";
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {
                    stat = false;
                    msg = "Failed To Add Periodic Maintenance Details..";
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
            }
        }

        [HttpPost]
        public JsonResult Edit(AmcPeriodicMaintenanceViewModel PerdcViewModel)
        {
            Int64 AmcId = PerdcViewModel.AmcId;
            string msg;
            bool stat;
            var DataExists = db.PeriodicMaintenances.Any(u => u.PeriodicMaintenanceNo == PerdcViewModel.PMaintNo && u.AmcId != AmcId);
            if (DataExists)
            {

                stat = false;
                msg = "A Periodic Maintenance with same Periodic Sl No. exists...";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                if (ModelState.IsValid)
                {
                    Int64 PerdcMainId = 0, PDetailId = 0;

                    var UserId = User.Identity.GetUserId();
                    DateTime Today = System.DateTime.Now;
                    DateTime PDate = DateTime.Parse(PerdcViewModel.PDate.ToString(), new CultureInfo("en-GB"));

                    /***************************  Create Mode *******************************/
                    if (PerdcViewModel.PMainDetailId == 0 || PerdcViewModel.PMainDetailId == null)
                    {
                        PeriodicMaintenance PMasterObj = db.PeriodicMaintenances.Where(a => a.AmcId == AmcId).FirstOrDefault();

                        /********************* PeriodicMaintenances ************************/
                        //If Already Periodic Maintenance Exists, Edit The Existing PeriodicMaintenance table
                        if (PMasterObj != null)
                        {
                            PMasterObj.NoOfPMaintenance = PMasterObj.NoOfPMaintenance + 1;
                            PMasterObj.LogTime = Today;

                            db.Entry(PMasterObj).State = EntityState.Modified;
                            db.SaveChanges();

                            PerdcMainId = PMasterObj.PeriodicMaintenanceId;
                        }
                        //If No Periodic Maintenance Exists, Add to PeriodicMaintenance table
                        else
                        {
                            PeriodicMaintenance MasterObj = new PeriodicMaintenance();

                            MasterObj.PeriodicMaintenanceNo = GetPMaintNo();
                            MasterObj.AmcId = AmcId;
                            MasterObj.NoOfPMaintenance = 1;
                            MasterObj.CreatedDate = Today;
                            MasterObj.LogTime = Today;

                            db.PeriodicMaintenances.Add(MasterObj);
                            db.SaveChanges();

                            PerdcMainId = MasterObj.PeriodicMaintenanceId;

                            //To Update 'PeriodicMaintReqrd' column of Amc Table while adding new data
                            Amc AmcObj = db.Amcs.Find(AmcId);

                            AmcObj.PeriodicMaintReqrd = true;
                            AmcObj.LogTime = Today;

                            db.Entry(AmcObj).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        /****************** PeriodicMaintenanceDetail **********************/
                        //Add new row to PeriodicMaintenanceDetail table
                        PeriodicMaintenanceDetail NwDetObj = new PeriodicMaintenanceDetail();

                        NwDetObj.PeriodicMaintenanceId = PerdcMainId;
                        NwDetObj.PDate = PDate;
                        NwDetObj.Notes = PerdcViewModel.Notes;
                        NwDetObj.CreatedDate = Today;
                        NwDetObj.LogTime = Today;

                        db.PeriodicMaintenanceDetails.Add(NwDetObj);
                        db.SaveChanges();

                        PDetailId = NwDetObj.PeriodicMaintDetailsId;
                        var exist = db.ProTasks.Any(o => o.VManuId == PDetailId);//o.StartDate == row.PDate && o.TaskName.Contains("AMC - Periodic Maintenance"));
                        if (!exist)
                        {
                            var existpro = db.ProTasks.Any(o => o.VModId == AmcId);//o.StartDate == row.PDate && o.TaskName.Contains("AMC - Periodic Maintenance"));


                            var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                            var today = Convert.ToDateTime(System.DateTime.Now);
                            if (1 == 1)
                            {
                                Amc AmcObj = db.Amcs.Find(AmcId);

                                ProTask amctask = new ProTask();
                                amctask.TaskNo = GetProNo();
                                amctask.TaskCode = InvoiceNo();
                                amctask.TaskName = "AMC - Periodic Maintenance " + " Customer :" + db.Customers.Find(AmcObj.CustomerId).CustomerName + " Date : " + PDate;
                                amctask.StartDate = PDate;
                                amctask.CreatedDate = PDate;
                                amctask.CreatedBy = UserId;
                                amctask.Status = Status.active;
                                amctask.Lattitude = AmcObj.Lattitude;
                                amctask.Longitude = AmcObj.Longitude;
                                amctask.Branch = BranchID;
                                amctask.logtime = PDate;
                                amctask.CustomerID = AmcObj.CustomerId;
                                amctask.VManuId = PDetailId;
                                amctask.VModId = AmcId;
                                var tasktype = db.ProTaskTypes.Where(o => o.TypeName == "PERIODIC MAINTEANCE").Select(o => o.TaskTypeId).FirstOrDefault();
                                if (tasktype != null && tasktype != 0)
                                {
                                    amctask.TaskType = tasktype;
                                }

                                db.ProTasks.Add(amctask);
                                db.SaveChanges();
                                Int64 proId = amctask.ProTaskId;
                                ProTaskUpdation TaskUp = new ProTaskUpdation
                                {
                                    ProTaskId = proId,
                                    //Status = TKUpdateStatus.Created,
                                    CreatedBy = UserId,
                                    CreatedDate = PDate,
                                    //TaskTeamId = teamId
                                };
                                db.ProTaskUpdations.Add(TaskUp);
                                db.SaveChanges();
                                Int64 TaskUpdId = TaskUp.TaskUpdationID;

                            }

                        }
                        /************************ Assigned Team ***************************/
                        if (PerdcViewModel.AssignTypeAll != null)
                        {
                            PeriodicMaintAssignedTeam TeamObj = new PeriodicMaintAssignedTeam();
                            foreach (var arr in PerdcViewModel.AssignTypeAll)
                            {
                                TeamObj.PeriodicMaintDtlId = Convert.ToInt64(PDetailId);
                                TeamObj.TeamId = arr;
                                db.PeriodicMaintAssignedTeams.Add(TeamObj);
                                db.SaveChanges();
                            }
                        }

                        /************************* Assigned To ***************************/
                        if (PerdcViewModel.AssignedTo != null)
                        {
                            PeriodicMaintAssignedTo AssignTo = new PeriodicMaintAssignedTo();

                            foreach (var arr in PerdcViewModel.AssignedTo)
                            {
                                AssignTo.PeriodicMaintDtlId = Convert.ToInt64(PDetailId);
                                AssignTo.EmployeeId = arr;
                                AssignTo.Status = "Assigned";
                                AssignTo.AssignBy = UserId;
                                AssignTo.ChkStatus = Status.active;
                                AssignTo.CreatedDate = Convert.ToDateTime(Today);
                                db.PeriodicMaintAssignedToes.Add(AssignTo);
                                db.SaveChanges();
                            }
                        }
                    }
                    /******************************* Edit Mode *****************************/
                    else
                    {
                        /****************** PeriodicMaintenanceDetail **********************/
                        //Edit existing  row in PeriodicMaintenanceDetail table
                        PDetailId = PerdcViewModel.PMainDetailId;

                        PeriodicMaintenanceDetail PDtlObj = db.PeriodicMaintenanceDetails.Find(PDetailId);

                        if (PDtlObj != null)
                        {
                            PDtlObj.PDate = PDate;
                            PDtlObj.Notes = PerdcViewModel.Notes;
                            PDtlObj.LogTime = Today;

                            db.Entry(PDtlObj).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        var NewAmcStat = GetStatusName(PDtlObj.PeriodicMaintStatus);

                        /*********************** Periodic Updations *************************/
                        var statusId = db.AmcUpdations.Where(a => a.TransId == PDetailId && a.TransType == "PeriodicMaintenance");

                        if (statusId != null)
                        {
                            db.AmcUpdations.RemoveRange(db.AmcUpdations.Where(a => a.TransId == PDetailId && a.TransType == "PeriodicMaintenance"));
                            db.SaveChanges();
                        }

                        AmcUpdation PeriodicUps = new AmcUpdation
                        {
                            TransId = PDetailId,
                            TransType = "PeriodicMaintenance",
                            CreatedBy = UserId,
                            CreatedDate = Today,
                            Remarks = NewAmcStat,
                        };
                        db.AmcUpdations.Add(PeriodicUps);
                        db.SaveChanges();

                        Int64 AmcUpdationID = PeriodicUps.UpdationID;

                        /************************ Assigned Team ***************************/

                        //********** Remove From Assigned Team 
                        var AssgnTeams = db.PeriodicMaintAssignedTeams.Where(a => a.PeriodicMaintDtlId == PDetailId).FirstOrDefault();

                        if (AssgnTeams != null)
                        {
                            db.PeriodicMaintAssignedTeams.RemoveRange(db.PeriodicMaintAssignedTeams.Where(a => a.PeriodicMaintDtlId == PDetailId));
                            db.SaveChanges();
                        }

                        if (PerdcViewModel.AssignTypeAll != null)
                        {
                            PeriodicMaintAssignedTeam TeamObj = new PeriodicMaintAssignedTeam();
                            foreach (var arr in PerdcViewModel.AssignTypeAll)
                            {
                                TeamObj.PeriodicMaintDtlId = Convert.ToInt64(PDetailId);
                                TeamObj.TeamId = arr;
                                db.PeriodicMaintAssignedTeams.Add(TeamObj);
                                db.SaveChanges();
                            }
                        }

                        /************************* Assigned To ***************************/
                        if (PerdcViewModel.AssignedTo != null)
                        {
                            var PrevAsgn = db.PeriodicMaintAssignedToes.Where(a => a.PeriodicMaintDtlId == PDetailId && a.Status == "Assigned" && a.ChkStatus == Status.active).Select(a => a.EmployeeId).ToArray();
                            var NewUsers = PerdcViewModel.AssignedTo.ToArray();

                            PeriodicMaintAssignedTo NewObj = new PeriodicMaintAssignedTo();

                            foreach (var arr in PrevAsgn)
                            {
                                var AssgndToId = db.PeriodicMaintAssignedToes.Where(a => a.PeriodicMaintDtlId == PDetailId && a.Status == "Assigned" && a.ChkStatus == Status.active && a.EmployeeId == arr).Select(a => a.AssignedToId).FirstOrDefault();
                                PeriodicMaintAssignedTo tskassr = db.PeriodicMaintAssignedToes.Find(AssgndToId);

                                //for removed members
                                if (!NewUsers.Contains(arr))
                                {
                                    tskassr.ChkStatus = Status.inactive;
                                    db.Entry(tskassr).State = EntityState.Modified;
                                    db.SaveChanges();

                                    NewObj.PeriodicMaintDtlId = PDetailId;
                                    NewObj.EmployeeId = arr;
                                    NewObj.Status = "Removed";
                                    NewObj.AssignBy = UserId;
                                    NewObj.CreatedDate = Convert.ToDateTime(Today).AddMilliseconds(100); ;
                                    db.PeriodicMaintAssignedToes.Add(NewObj);
                                    db.SaveChanges();
                                }
                                //for existing members--Updating status
                                else
                                {
                                    tskassr.ChkStatus = Status.inactive;
                                    db.Entry(tskassr).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }

                            foreach (var arr in PerdcViewModel.AssignedTo)
                            {
                                NewObj.PeriodicMaintDtlId = PDetailId;
                                NewObj.EmployeeId = arr;
                                NewObj.Status = "Assigned";
                                NewObj.AssignBy = UserId;
                                NewObj.ChkStatus = Status.active;
                                NewObj.CreatedDate = Convert.ToDateTime(Today).AddMilliseconds(100);
                                db.PeriodicMaintAssignedToes.Add(NewObj);
                                db.SaveChanges();
                            }
                        }
                    }

                    PerdcViewModel.PMainDetailId = 0;
                    com.addlog(LogTypes.Created, UserId, "AMCPeriodicMaintenance", "PeriodicMaintenances", findip(), PerdcMainId, "Periodic Maintenance added successfully");
                    stat = true;
                    msg = "Successfully added Periodic Maintenance Details..";
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {
                    stat = false;
                    msg = "Failed To Add Periodic Maintenance Details..";
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
            }
        }

        //Delete GET
        [HttpGet]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var Obj = db.PeriodicMaintenanceDetails.Where(a => a.PeriodicMaintDetailsId == id).FirstOrDefault();

            if (Obj == null)
            {
                return NotFound();
            }
            return PartialView(Obj);
        }

        //POST Delete
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteAction(long id)
        {
            bool stat;
            string msg;
           
            //***********Delete from table Amc Documents
            var AmcDocs = db.AmcDocuments.Where(a => a.TransId == id && a.TransType == "PeriodicMaintenance").ToList();
            if (AmcDocs != null)
            {          
                foreach (var row in AmcDocs)
                {
                    //To remove the attached file from folder
                    string FullPath = LegacyWeb.MapPath("~/uploads/AmcDocuments/PeriodicDocuments/" + row.FileName);

                    if (System.IO.File.Exists(FullPath))
                    {
                        System.IO.File.Delete(FullPath);
                    }
                    
                    //To remove resize files from folder
                    string ResizePath = LegacyWeb.MapPath("~/uploads/AmcDocuments/PeriodicDocuments/resize_" + row.FileName);

                    if (System.IO.File.Exists(ResizePath))
                    {
                        System.IO.File.Delete(ResizePath);
                    }

                    //To remove thumb files from folder
                    string ThumbPath = LegacyWeb.MapPath("~/uploads/AmcDocuments/PeriodicDocuments/thumb_" + row.FileName);

                    if (System.IO.File.Exists(ThumbPath))
                    {
                        System.IO.File.Delete(ThumbPath);
                    }                    
                }

                db.AmcDocuments.RemoveRange(db.AmcDocuments.Where(a => a.TransId == id && a.TransType == "PeriodicMaintenance"));
                db.SaveChanges();
            }

            //********************Delete from table AmcUpdations
            var AmcUpdates = db.AmcUpdations.Where(a => a.TransId == id && a.TransType == "PeriodicMaintenance");
            if (AmcUpdates != null)
            {
                db.AmcUpdations.RemoveRange(db.AmcUpdations.Where(a => a.TransId == id && a.TransType == "PeriodicMaintenance"));
                db.SaveChanges();
            }

            //********************Delete from table AmcRemarks
            var AmcRemarks = db.AmcRemarks.Where(a => a.TransId == id && a.TransType == "PeriodicMaintenance");
            if (AmcRemarks != null)
            {
                db.AmcRemarks.RemoveRange(db.AmcRemarks.Where(a => a.TransId == id && a.TransType == "PeriodicMaintenance"));
                db.SaveChanges();
            }

            //***********Delete from table PeriodicMaintAssignedToes
            var AssgnTos = db.PeriodicMaintAssignedToes.Where(a => a.PeriodicMaintDtlId == id);
            if (AssgnTos != null)
            {
                db.PeriodicMaintAssignedToes.RemoveRange(db.PeriodicMaintAssignedToes.Where(a => a.PeriodicMaintDtlId == id));
                db.SaveChanges();
            }

            //***********Delete from table PeriodicMaintAssignedTeams
            var AsgnTeams = db.PeriodicMaintAssignedTeams.Where(a => a.PeriodicMaintDtlId == id);
            if (AsgnTeams != null)
            {
                db.PeriodicMaintAssignedTeams.RemoveRange(db.PeriodicMaintAssignedTeams.Where(a => a.PeriodicMaintDtlId == id));
                db.SaveChanges();
            }

            //***********Delete from table PeriodicMaintenanceDetails

            var DtlObj = db.PeriodicMaintenanceDetails.Where(a => a.PeriodicMaintDetailsId == id).FirstOrDefault();
                       
            if (DtlObj != null)
            {
                var PMaintId = DtlObj.PeriodicMaintenanceId;

                db.PeriodicMaintenanceDetails.RemoveRange(db.PeriodicMaintenanceDetails.Where(a => a.PeriodicMaintDetailsId == id));
                db.SaveChanges();


                //    //***********Delete from table PeriodicMaintenance

                //    //To Update 'PeriodicMaintReqrd' column of Amc Table while deleting all data



                    //***********Delete from table PeriodicMaintenance
                    PeriodicMaintenance MastObj = db.PeriodicMaintenances.Find(PMaintId);

                    MastObj.NoOfPMaintenance    =   MastObj.NoOfPMaintenance - 1;
                    MastObj.LogTime             =   System.DateTime.Now;
                    db.Entry(MastObj).State     =   EntityState.Modified;
                    db.SaveChanges();
            }
            
            stat = true;
            msg = "Successfully deleted Periodic Maintenance Details..";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        // GET: My Periodic
        public ActionResult MyPeriodic()
        {
            //For Dropdown Asset Account
            ViewBag.DropDowns = QkSelect.List(
                      new List<SelectListItem>
                      {
                        new SelectListItem { Selected = false, Text = "All", Value = "0"},
                      }, "Value", "Text", 1);

            return View();
        }

        //My Periodic List ---- GET
        [RedirectingAction]
        [HttpPost]
        public ActionResult GetMyPeriodicDetails(long? AmcId, string PDate)
        {
            string search   =   Request.Form.GetValues("search[value]")[0];
            var draw        =   Request.Form.GetValues("draw").FirstOrDefault();
            var start       =   Request.Form.GetValues("start").FirstOrDefault();
            var length      =   Request.Form.GetValues("length").FirstOrDefault();

            //Find Order Column
            var sortColumn      = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir   = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            DateTime? pdate = null;
            if (PDate != "")
            {
                pdate = DateTime.Parse(PDate, new CultureInfo("en-GB").DateTimeFormat);
            }

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            var UserId  = User.Identity.GetUserId();
            var EmpId   = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            // SERVER: only translatable columns. The "assigned to me" filter is kept as a correlated EXISTS
            // (translatable); latest AmcUpdation + nested AssignedTo are computed CLIENT-side below.
            var serverRows2 = (from a in db.PeriodicMaintenances
                     join b in db.PeriodicMaintenanceDetails
                     on a.PeriodicMaintenanceId equals b.PeriodicMaintenanceId
                     join c in db.Amcs on a.AmcId equals c.AmcId
                     join d in db.AmcStatuss on b.PeriodicMaintStatus equals d.AmcStatusId into temp1
                     from d in temp1.DefaultIfEmpty()
                     where
                     (AmcId == 0  || AmcId == null || c.AmcId == AmcId) &&
                     (PDate == "" || PDate == null || EF.Functions.DateDiffDay(b.PDate, pdate) == 0) &&
                     db.PeriodicMaintAssignedToes.Any(x => x.PeriodicMaintDtlId == b.PeriodicMaintDetailsId && x.Status == "Assigned" && x.ChkStatus == Status.active && x.EmployeeId == EmpId)
                     select new
                     {
                         a.AmcId,
                         c.AmcNo,
                         b.PDate,
                         b.Notes,
                         PMaintId = a.PeriodicMaintenanceId,
                         PerMaintDetailsId = b.PeriodicMaintDetailsId,
                         StatusName = d.StatusName,
                         LogTime = b.LogTime,
                     }).ToList();

            // CLIENT: latest AmcUpdation per PeriodicMaintenanceDetail, keyed by PeriodicMaintDetailsId.
            var pmIds2 = serverRows2.Select(o => o.PerMaintDetailsId).ToList();
            var periodicUpLookup2 = db.AmcUpdations
                .Where(u => u.TransType == "PeriodicMaintenance" && pmIds2.Contains(u.TransId))
                .ToList().GroupBy(u => u.TransId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.CreatedDate).First());
            var pmAssignLookup2 = (from z in db.PeriodicMaintAssignedToes
                                   join y in db.Employees on z.EmployeeId equals y.EmployeeId
                                   where z.Status == "Assigned" && z.ChkStatus == Status.active && pmIds2.Contains(z.PeriodicMaintDtlId)
                                   select new { z.PeriodicMaintDtlId, y.EmployeeId, y.LastName, y.FirstName, y.MiddleName, y.ImgFileName, y.Status })
                                  .ToList().ToLookup(x => x.PeriodicMaintDtlId);

            var v = serverRows2.Select(o =>
            {
                DateTime? ldateVal = o.LogTime;
                if (periodicUpLookup2.TryGetValue(o.PerMaintDetailsId, out var up) && up.CreatedDate > o.LogTime)
                    ldateVal = up.CreatedDate;
                return new
                {
                    o.AmcId,
                    o.AmcNo,
                    o.PDate,
                    o.Notes,
                    PMaintId = o.PMaintId,
                    PerMaintDetailsId = o.PerMaintDetailsId,
                    StatusName = o.StatusName,
                    AssignedTo = pmAssignLookup2[o.PerMaintDetailsId].Select(y => new
                    {
                        id = y.EmployeeId,
                        LastName = (y.LastName != null) ? y.LastName : "",
                        FirstName = (y.FirstName != null) ? y.FirstName : "",
                        MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                        Img = (y.ImgFileName != null) ? y.ImgFileName : "",
                        y.Status
                    }).Distinct().ToList(),
                    ldate = ldateVal,
                };
            }).OrderByDescending(a => a.ldate).ToList();
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir).ToList();
            }

            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        [RedirectingAction]
        [HttpPost]
        public ActionResult GetPeriodicDetails2(long? AmcId, string PDate)
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            DateTime? pdate = null;
            if (PDate != "")
            {
                pdate = DateTime.Parse(PDate, new CultureInfo("en-GB").DateTimeFormat);
            }

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            var UserId = User.Identity.GetUserId();
            var EmpId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            // SERVER: only translatable columns. Latest AmcUpdation + nested AssignedTo are computed CLIENT-side below.
            var serverRows3 = (from a in db.PeriodicMaintenances
                     join b in db.PeriodicMaintenanceDetails
                     on a.PeriodicMaintenanceId equals b.PeriodicMaintenanceId
                     join c in db.Amcs on a.AmcId equals c.AmcId
                     join d in db.AmcStatuss on b.PeriodicMaintStatus equals d.AmcStatusId into temp1
                     from d in temp1.DefaultIfEmpty()
                     where
                     (AmcId == 0 || AmcId == null || c.AmcId == AmcId) &&
                     (PDate == "" || PDate == null || EF.Functions.DateDiffDay(b.PDate, pdate) == 0)
                     select new
                     {
                         a.AmcId,
                         c.AmcNo,
                         b.PDate,
                         b.Notes,
                         PMaintId = a.PeriodicMaintenanceId,
                         PerMaintDetailsId = b.PeriodicMaintDetailsId,
                         StatusName = d.StatusName,
                         LogTime = b.LogTime,
                     }).ToList();

            // CLIENT: latest AmcUpdation per PeriodicMaintenanceDetail, keyed by PeriodicMaintDetailsId.
            var pmIds3 = serverRows3.Select(o => o.PerMaintDetailsId).ToList();
            var periodicUpLookup3 = db.AmcUpdations
                .Where(u => u.TransType == "PeriodicMaintenance" && pmIds3.Contains(u.TransId))
                .ToList().GroupBy(u => u.TransId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.CreatedDate).First());
            var pmAssignLookup3 = (from z in db.PeriodicMaintAssignedToes
                                   join y in db.Employees on z.EmployeeId equals y.EmployeeId
                                   where z.Status == "Assigned" && z.ChkStatus == Status.active && pmIds3.Contains(z.PeriodicMaintDtlId)
                                   select new { z.PeriodicMaintDtlId, y.EmployeeId, y.LastName, y.FirstName, y.MiddleName, y.ImgFileName, y.Status })
                                  .ToList().ToLookup(x => x.PeriodicMaintDtlId);

            var v = serverRows3.Select(o =>
            {
                DateTime? ldateVal = o.LogTime;
                if (periodicUpLookup3.TryGetValue(o.PerMaintDetailsId, out var up) && up.CreatedDate > o.LogTime)
                    ldateVal = up.CreatedDate;
                return new
                {
                    o.AmcId,
                    o.AmcNo,
                    o.PDate,
                    o.Notes,
                    PMaintId = o.PMaintId,
                    PerMaintDetailsId = o.PerMaintDetailsId,
                    StatusName = o.StatusName,
                    AssignedTo = pmAssignLookup3[o.PerMaintDetailsId].Select(y => new
                    {
                        id = y.EmployeeId,
                        LastName = (y.LastName != null) ? y.LastName : "",
                        FirstName = (y.FirstName != null) ? y.FirstName : "",
                        MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                        Img = (y.ImgFileName != null) ? y.ImgFileName : "",
                        y.Status
                    }).Distinct().ToList(),
                    ldate = ldateVal,
                };
            }).OrderByDescending(a => a.ldate).ToList();
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir).ToList();
            }

            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        //Status-->GET(From MyPeriodic)
        public ActionResult AddStatusUpdate(long id)
        {
            var ViewModel = new StatusUpdateViewModel
            {
                TransId     =   id,
                TransType   =   "PeriodicMaintenance",
            };

            var use = db.Employees.Select(s => new
                      {
                           ID = s.EmployeeId,
                           Name = s.FirstName + " " + s.LastName
                      }).ToList();

            var UserId = User.Identity.GetUserId();
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");

            ViewBag.Dropdowns = QkSelect.List(
                           new List<SelectListItem>
                           {
                               new SelectListItem { Selected = false, Text = null, Value = null},
                           }, "Value", "Text", 1);

            Int32? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
            TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
            ViewBag.TMOut = TimeOut;

            return PartialView(ViewModel);
        }

        //POST--->Add Status
        [HttpPost]
        public JsonResult AddStatusUpdate(StatusUpdateViewModel ViewModel)
        {
            string msg;
            bool stat = false;
            string lat = "";
            string log = "";
            lat = Request.Form["lat"];
            log = Request.Form["log"];

            if (ModelState.IsValid)
            {
                var UserId = User.Identity.GetUserId();
                var today = System.DateTime.Now;

                Int64 PerdDtlId = ViewModel.TransId;

                var StatusId    =   db.PeriodicMaintenanceDetails.Where(a => a.PeriodicMaintDetailsId == PerdDtlId).Select(a => a.PeriodicMaintStatus).FirstOrDefault();
                var OldStatus   =   GetStatusName(StatusId);
                var NewStatus   =   GetStatusName(ViewModel.PMaintStatusId);

                var statusId = db.AmcUpdations.Where(a => a.TransId == PerdDtlId && a.TransType == "PeriodicMaintenance");
                if (statusId != null)
                {
                    db.AmcUpdations.RemoveRange(db.AmcUpdations.Where(a => a.TransId == PerdDtlId && a.TransType == "PeriodicMaintenance"));
                    db.SaveChanges();
                }

                AmcUpdation AmcUps = new AmcUpdation
                {
                    TransId     =   PerdDtlId,
                    TransType   =   "PeriodicMaintenance",
                    CreatedBy   =   UserId,
                    CreatedDate =   today,
                    Remarks     =   OldStatus + " Change To " + NewStatus,
                };
                db.AmcUpdations.Add(AmcUps);
                db.SaveChanges();

                Int64 AmcUpdId = AmcUps.UpdationID;

                var RemarkInfo = new AmcRemark
                {
                    CreatedDate =   today,
                    StatusID    =   ViewModel.PMaintStatusId,
                    Remark      =   ViewModel.Remark + lat + "," + log,
                    AddedUser   =   UserId,
                    TransId     =   PerdDtlId,
                    TransType   =   "PeriodicMaintenance",
                    UpdationID  =   AmcUpdId
                };
                db.AmcRemarks.Add(RemarkInfo);
                db.SaveChanges();

                //To Update Status and LogTime in Amc Table
                PeriodicMaintenanceDetail Obj = db.PeriodicMaintenanceDetails.Find(PerdDtlId);

                Obj.PeriodicMaintStatus = ViewModel.PMaintStatusId;
                Obj.LogTime = today;

                db.Entry(Obj).State = EntityState.Modified;
                db.SaveChanges();

                // fileupload
                IFormFileCollection files = Request.Form.Files;
                if (files.Count > 0)
                {
                    string path = LegacyWeb.MapPath("~/uploads/AmcDocuments/PeriodicDocuments/");
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    for (int i = 0; i < files.Count; i++)
                    {
                        IFormFile file = files[i];
                        if (file.Length > 0)
                        {
                            var fileCount   =   db.AmcDocuments.Select(a => a.DocumentId).AsEnumerable().DefaultIfEmpty(0).Max();
                            var fileName    =   Path.GetFileName(file.FileName);

                            String extension = Path.GetExtension(fileName);

                            String newName      =   fileCount + extension;
                            string newFName     =   fileCount + extension;
                            string newFileName  =   fileCount + extension;
                            var FStatus         =   Status.active;
                            var thumbName       =   "";
                            var resizeName      =   "";
                            if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                            {
                                thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/AmcDocuments/PeriodicDocuments/"), thumbName);

                                resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/AmcDocuments/PeriodicDocuments/"), resizeName);
                                newFName = "resize_" + newFName;
                                FStatus = Status.inactive;
                            }
                            else
                            {
                                var commonfilename = "Docs-Thump.png";
                            }
                            newName = Path.Combine(LegacyWeb.MapPath("~/uploads/AmcDocuments/PeriodicDocuments/"), newName);
                            file.SaveAs(newName);

                            var AmcImage = new AmcDocument
                            {
                                TransId     =   PerdDtlId,
                                TransType   =   "PeriodicMaintenance",
                                FileName    =   newFileName,//Path.GetFileName(file.FileName),
                                Status      =   FStatus,
                                CreatedDate =   today
                            };
                            db.AmcDocuments.Add(AmcImage);
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
                                thumb.Save(thumbName);

                                Image lgimg = Image.FromFile(newName);
                                if (lgimg.Width > 1800 || lgimg.Height > 1800)
                                {
                                    Image imgs = Image.FromFile(newName);
                                    System.Drawing.Image thumbs = Common.resizeImage(imgs, new Size(1800, 1800));
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/AmcDocuments/PeriodicDocuments/"), resizeName);
                                    thumbs.Save(resizeName);
                                }
                                else
                                {
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/AmcDocuments/PeriodicDocuments/"), resizeName);
                                    lgimg.Save(resizeName);
                                }
                            }
                        }
                    }
                }
             
                if (ViewModel.PMaintStatusId != null)
                {
                    PeriodicMaintenanceDetail DtlObj = db.PeriodicMaintenanceDetails.Find(PerdDtlId);

                    var pflow = db.PeriodicProcessFlows.Where(a => a.PeriodicStatus == ViewModel.PMaintStatusId).FirstOrDefault();

                    if (pflow != null)
                    {
                        if (pflow.RemoveUpdateUser == true)
                        {
                            var UserEmp = db.Employees.Where(a => a.UserId == UserId && a.UserStatus == true).Select(a => a.EmployeeId).FirstOrDefault();

                            if (UserEmp != null)
                            {
                                PeriodicMaintAssignedTo NewAssgnTo = new PeriodicMaintAssignedTo();
                                var PrevAssgnTo = db.PeriodicMaintAssignedToes.Where(a => a.PeriodicMaintDtlId == DtlObj.PeriodicMaintDetailsId && a.Status == "Assigned" && a.EmployeeId == UserEmp && a.ChkStatus == Status.active).ToList();
                                if (PrevAssgnTo != null)
                                {
                                    foreach (var arr in PrevAssgnTo)
                                    {
                                        PeriodicMaintAssignedTo Obj1 = db.PeriodicMaintAssignedToes.Find(arr.AssignedToId);
                                        Obj1.ChkStatus = Status.inactive;
                                        db.Entry(Obj1).State = EntityState.Modified;
                                        db.SaveChanges();

                                        NewAssgnTo.PeriodicMaintDtlId   =   PerdDtlId;
                                        NewAssgnTo.EmployeeId           =   arr.EmployeeId;
                                        NewAssgnTo.Status               =   "Removed";
                                        NewAssgnTo.AssignBy             =   UserId;
                                        NewAssgnTo.CreatedDate          =   Convert.ToDateTime(today).AddMilliseconds(100); ;
                                        db.PeriodicMaintAssignedToes.Add(NewAssgnTo);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }

                        if (pflow.RemoveUpdateUserTeams == true)
                        {
                            var UserEmp = db.Employees.Where(a => a.UserId == UserId && a.UserStatus == true).Select(a => a.EmployeeId).FirstOrDefault();

                            var team        =   db.Teams.Where(a => a.TeamLead == UserEmp).Select(a => a.TeamId).Distinct().ToList();
                            var teams       =   db.TeamMembers.Where(a => a.EmployeeId == UserEmp).Select(a => a.TeamId).Distinct().ToList();
                            var fulldata    =   team.Union(teams);
                            ////team lead as user
                            var PeriodicTeam = (from a in db.Teams
                                               where fulldata.Contains(a.TeamId) && (pflow.RemoveUpdateUser == false || a.TeamLead != UserEmp)
                                               //where a.TeamLead == UserEmp
                                               select new
                                               {
                                                   EmployeeId = a.TeamLead
                                               }).ToArray();
                            ////team mebers 
                            var PeriodicMem = (from a in db.Teams
                                              join b in db.TeamMembers on a.TeamId equals b.TeamId into tea
                                              from b in tea.DefaultIfEmpty()
                                              where fulldata.Contains(a.TeamId) && (pflow.RemoveUpdateUser == false || b.EmployeeId != UserEmp)
                                              select new
                                              {
                                                  b.EmployeeId
                                              }).ToArray();

                            var emp = PeriodicTeam.Union(PeriodicMem).Select(a => a.EmployeeId).ToList();
                            var AmcAssgn = db.PeriodicMaintAssignedToes.Where(a => a.PeriodicMaintDtlId == PerdDtlId && emp.Contains(a.EmployeeId) && a.Status == "Assigned" && a.ChkStatus == Status.active).ToList();

                            PeriodicMaintAssignedTo NewObj = new PeriodicMaintAssignedTo();
                            if (AmcAssgn != null)
                            {
                                foreach (var arr in AmcAssgn)
                                {
                                    PeriodicMaintAssignedTo PrevObj = db.PeriodicMaintAssignedToes.Find(arr.AssignedToId);
                                    PrevObj.ChkStatus = Status.inactive;
                                    db.Entry(PrevObj).State = EntityState.Modified;
                                    db.SaveChanges();

                                    NewObj.PeriodicMaintDtlId   =   PerdDtlId;
                                    NewObj.EmployeeId           =   arr.EmployeeId;
                                    NewObj.Status               =   "Removed";
                                    NewObj.AssignBy             =   UserId;
                                    NewObj.CreatedDate          =   Convert.ToDateTime(today).AddMilliseconds(100); ;
                                    db.PeriodicMaintAssignedToes.Add(NewObj);
                                    db.SaveChanges();
                                }
                            }
                        }

                        // process flow members assigned
                        var chkassgn = db.PeriodicMaintAssignedToes.Where(a => a.PeriodicMaintDtlId == PerdDtlId && a.Status == "Assigned" && a.ChkStatus == Status.active).Select(a => a.EmployeeId).ToList();

                        var pfmembers = db.PeriodicProcessFlowAssignUsers.Where(a => a.PerdcProcessFlowId == pflow.PeriodicProcessFlowId).Select(a => a.EmployeeId).ToList();

                        PeriodicMaintAssignedTo AssObj = new PeriodicMaintAssignedTo();
                        foreach (var arr in pfmembers)
                        {
                            if (!chkassgn.Contains(arr))
                            {
                                AssObj.PeriodicMaintDtlId = PerdDtlId;
                                AssObj.EmployeeId       =   arr;
                                AssObj.Status           =   "Assigned";
                                AssObj.AssignBy         =   UserId;
                                AssObj.ChkStatus        =   Status.active;
                                AssObj.CreatedDate      =   Convert.ToDateTime(today).AddMilliseconds(100);
                                db.PeriodicMaintAssignedToes.Add(AssObj);
                                db.SaveChanges();
                            }
                        }
                    }
                }

                msg = "Remark added successfully.";
                stat = true;

                com.addlog(LogTypes.Created, UserId, "AMCPeriodicMaintenance", "AmcRemarks", findip(), PerdDtlId, "Remark Added Successfully");
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form..";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }
        private long GetProNo()
        {
            Int64 SENo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "Task").Select(a => a.number).FirstOrDefault();
            if ((db.ProTasks.Select(p => p.TaskNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
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
                SENo = db.ProTasks.Max(p => p.TaskNo + 1);
            }

            return SENo;
        }
        private string InvoiceNo(Int64 SENo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "Task").Select(a => a.prefix).FirstOrDefault();
            if (billNo == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "Task").Select(a => a.number).FirstOrDefault();
                if ((db.ProTasks.Select(p => p.TaskNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    SENo = db.ProTasks.Max(p => p.TaskNo + 1);
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
        private bool BillExist(string SENo)
        {
            var Exists = db.ProTasks.Any(c => c.TaskCode == SENo);
            bool res = (Exists) ? true : false;
            return res;
        }
        //Function to Get the data into view of Status update table
        [HttpPost]
        public ActionResult GetAllStatusUpdates(long PeriodicDtlId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();

            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();

            var v = (from a in db.AmcRemarks
                     join b in db.Users on a.AddedUser equals b.Id into emp
                     from b in emp.DefaultIfEmpty()
                     join c in db.AmcStatuss on a.StatusID equals c.AmcStatusId 
                     where a.TransId == PeriodicDtlId && a.TransType == "PeriodicMaintenance"
                     orderby a.CreatedDate descending
                     select new
                     {
                         RemarkId = a.RemarkId,
                         a.CreatedDate,
                         EmpName = b.UserName,
                         a.Remark,
                         Status = c.StatusName,
                         StatusId = c.AmcStatusId
                     }).ToList().Select(o => new
                     {
                         o.RemarkId,
                         o.CreatedDate,
                         o.EmpName,
                         o.Remark,
                         o.Status,
                         o.StatusId
                     });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = v.Count();
            var data = v.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        //Function to get Status Name
        public string GetStatusName(long? Id)
        {
            string Name = db.AmcStatuss.Where(a => a.AmcStatusId == Id).Select(a => a.StatusName).FirstOrDefault();
            return Name;
        }
    }
}
