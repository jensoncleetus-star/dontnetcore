using QuickSoft.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using QuickSoft.Controllers;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace QuickSoft.Areas.Hr.Controllers
{
    [RedirectingAction]
    [Microsoft.AspNetCore.Mvc.Area("Hr")]
    public class SalaryStructureController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public SalaryStructureController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: Hr/Attendance
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,SalaryStructure List")]
        public ActionResult Index()
        {
            return View();
        }

        [RedirectingAction]
        [HttpPost]
        [QkAuthorize(Roles = "Dev,SalaryStructure List")]
        public ActionResult GetSalaryStructure()
        {
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

            var v = (from a in db.SalaryStructures
                     join b in db.Employees on a.EmployeeId equals b.EmployeeId into emp
                     from b in emp.DefaultIfEmpty()
                     join e in db.Users on a.CreatedBy equals e.Id
                     select new
                     {
                         a.SalaryStrId,
                         a.EFDate,
                         a.EmployeeId,
                         EmpName = b.FirstName + " " + b.LastName,
                         e.UserName,
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.EmpName.ToString().ToLower().Contains(search.ToLower()));
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

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Create SalaryStructure")]
        public ActionResult Create()
        {
            SalaryStructuresViewModel vmodel = new SalaryStructuresViewModel();
            vmodel.EFDate = System.DateTime.Now.ToString("dd-MM-yyyy");

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

            //var use = db.Employees.Where(a => a.LeavingDate == null)
            //                 .Select(s => new
            //                 {
            //                     ID = s.EmployeeId,
            //                     Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
            //                 })
            //                 .ToList();
            //ViewBag.Employ = QkSelect.List(use, "ID", "Name");


            ViewBag.SType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Start Afresh", Value="0"},
                new SelectListItem() {Text = "Copy From Parent Grade", Value="1"},
                new SelectListItem() {Text = "Copy From Employee Grade", Value="2"},
                new SelectListItem() {Text = "Copy From Employee", Value="3"},
            }, "Value", "Text");

            ViewBag.SelVal = QkSelect.List(
                              new List<SelectListItem>
                              {
                                   new SelectListItem { Selected = true, Text = "Select", Value = ""},
                              }, "Value", "Text", 0);

            return View(vmodel);
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create SalaryStructure")]
        public JsonResult CreateSalaryStructure(SalaryStructuresViewModel vmodel)
        {
            string msg;
            bool stat;

            var UserId = User.Identity.GetUserId();
            var today = Convert.ToDateTime(System.DateTime.Now);

            long Branch = 0;

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

            if (BranchCheck == Status.active)
            {
                Branch = (long)vmodel.Branch;
            }
            else
            {
                Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
            }

            SalaryStructure sstruct = new SalaryStructure
            {
                EmployeeId = vmodel.EmployeeId,
                EFDate = DateTime.Parse(vmodel.EFDate.ToString(), new CultureInfo("en-GB")),
                Branch = Branch,
                CreatedDate = today,
                CreatedBy = UserId,
                Status = Status.active,
            };
            db.SalaryStructures.Add(sstruct);
            db.SaveChanges();
            Int64 SSId = sstruct.SalaryStrId;

            SalaryStrDetail sstr = new SalaryStrDetail();
            foreach (var arr in vmodel.salarystr)
            {
                if (arr.PayHeadId > 0)
                {
                    sstr.SalaryStrId = SSId;
                    sstr.PayHeadId = arr.PayHeadId;
                    sstr.Rate = arr.Rate;
                    db.SalaryStrDetails.Add(sstr);
                    db.SaveChanges();
                }
            }

            com.addlog(LogTypes.Created, UserId, "SalaryStructure", "SalaryStructures", findip(), SSId, "Successfully Submitted Salary Structure");

            msg = "Successfully Added Salary Structure .";
            stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit SalaryStructure")]
        [HttpGet]
        public ActionResult Edit(long? id)
        {
            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            var UserId = User.Identity.GetUserId();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SalaryStructure salstr = db.SalaryStructures.Find(id);

            var Bnch = db.Branchs
                   .Select(s => new
                   {
                       Id = s.BranchID,
                       Name = s.BranchName
                   }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            var use = db.Employees.Where(a => a.LeavingDate == null)
                            .Select(s => new
                            {
                                ID = s.EmployeeId,
                                Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                            })
                            .ToList();
            ViewBag.Employ = QkSelect.List(use, "ID", "Name");

            if (salstr == null)
            {
                return NotFound();
            }

            SalaryStructuresViewModel vmodel = new SalaryStructuresViewModel();
            vmodel.SalaryStrId = (long)id;
            vmodel.EmployeeId = salstr.EmployeeId;
            vmodel.EFDate = salstr.EFDate.ToString("dd-MM-yyyy");

            return View(vmodel);
        }
        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit SalaryStructure")]
        public ActionResult EditSalaryStructure(SalaryStructuresViewModel vmodel)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            var today = Convert.ToDateTime(System.DateTime.Now);
            long Branch = 0;

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            SalaryStructure salstr = db.SalaryStructures.Find(vmodel.SalaryStrId);

            if (BranchCheck == Status.active)
            {
                Branch = (long)vmodel.Branch;
            }
            else
            {
                Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
            }

            salstr.EmployeeId = vmodel.EmployeeId;
            salstr.EFDate = DateTime.Parse(vmodel.EFDate.ToString(), new CultureInfo("en-GB"));

            db.Entry(salstr).State = EntityState.Modified;
            db.SaveChanges();
            Int64 sstrId = salstr.SalaryStrId;


            var bItems = db.SalaryStrDetails.Where(a => a.SalaryStrId == sstrId).FirstOrDefault();
            if (bItems != null)
            {
                db.SalaryStrDetails.RemoveRange(db.SalaryStrDetails.Where(a => a.SalaryStrId == sstrId));
                db.SaveChanges();
            }
            SalaryStrDetail sstr = new SalaryStrDetail();
            foreach (var arr in vmodel.salarystr)
            {
                if (arr.PayHeadId > 0)
                {
                    sstr.SalaryStrId = sstrId;
                    sstr.PayHeadId = arr.PayHeadId;
                    sstr.Rate = arr.Rate;
                    db.SalaryStrDetails.Add(sstr);
                    db.SaveChanges();
                }
            }

            com.addlog(LogTypes.Updated, UserId, "SalaryStructure", "SalaryStructures", findip(), sstrId, "Successfully Updated Salary Structure");
            msg = "Successfully Updated Salary Structure .";
            stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete SalaryStructure")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SalaryStructure sstr = db.SalaryStructures.Find(id);
            if (sstr == null)
            {
                return NotFound();
            }
            return PartialView(sstr);
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete SalaryStructure")]
        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            SalaryStructure sstr = db.SalaryStructures.Find(id);

            var Msg = chkDeleteWithMsg(id);
            if (Msg != null)
            {
                msg = Msg;
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Successfully deleted Salary Structure.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete SalaryStructure")]
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
                Success("Deleted " + count + " Salary Structure, Unable to Delete " + notdel + " Salary Structure. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Salary Structure.", true);
            }
            else
            {
                Success("Deleted " + count + " Salary Structure.", true);
            }
            return RedirectToAction("Index", "Salary Structure");
        }
        private Boolean DeleteItem(long id)
        {
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
            //if ()
            //{
            //}
            //else
            //{
            //    msg = null;
            //}
            return msg;
        }

        public bool DeleteFn(long id)
        {
            SalaryStructure sstr = db.SalaryStructures.Find(id);
            var sdetail = db.SalaryStrDetails.Where(a => a.SalaryStrId == id);
            if (sdetail != null)
            {
                db.SalaryStrDetails.RemoveRange(db.SalaryStrDetails.Where(a => a.SalaryStrId == id));
                db.SaveChanges();
            }
            if (sstr != null)
            {
                db.SalaryStructures.RemoveRange(db.SalaryStructures.Where(a => a.SalaryStrId == id));
                db.SaveChanges();
            }
            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "SalaryStructure", "SalaryStructures", findip(), id, "Successfully Deleted Salary Structure");
            db.SaveChanges();
            return true;
        }
        [HttpGet]
        public JsonResult GetSalaryStructureById(int SalID)
        {
            var v = (from a in db.SalaryStrDetails
                     join b in db.Payheads on a.PayHeadId equals b.ID into phead
                     from b in phead.DefaultIfEmpty()
                     where a.SalaryStrId == SalID
                     select new
                     {
                         a.PayHeadId,
                         b.Name,
                         a.Rate,
                         b.CalculationPeriod,
                         b.Type,
                         b.CalculationType,
                         b.Compute
                     }).AsEnumerable().Select(o => new
                     {
                         o.PayHeadId,
                         PayHead = o.Name,
                         o.Rate,
                         Per = o.CalculationPeriod,
                         HeadType = o.Type != null ? Enum.GetName(typeof(PayHeadType), o.Type) : "",
                         CalType = o.CalculationType != null ? Enum.GetName(typeof(CalcTypePayHead), o.CalculationType) : "",
                         Computed = o.Compute != null ? Enum.GetName(typeof(ComputPayHead), o.Compute) : "",
                     }).ToList();

            return Json(v);

        }

        [HttpPost]
        public JsonResult GetSalaryStrDetails(long EmpID, string FDate, string TDate)
        {
            if (string.IsNullOrWhiteSpace(FDate) || string.IsNullOrWhiteSpace(TDate)) { return Json(0); }
            DateTime fdate = DateTime.Parse(FDate, new CultureInfo("en-GB"));
            DateTime tdate = DateTime.Parse(TDate, new CultureInfo("en-GB"));

            TimeSpan ts = tdate.Date - fdate.Date;
            int days = ts.Days+1;

            var AtVoucher = db.EnableSettings.Where(a => a.EnableType == "PayAttendance").FirstOrDefault();
            if (AtVoucher.TypeValue == "Attendance Voucher")
            {
                //issue in multiple payhead in attandance
                var sal = (from a in db.SalaryStrDetails
                           join b in db.Payheads on a.PayHeadId equals b.ID into phead
                           from b in phead.DefaultIfEmpty()
                           join e in db.SalaryStructures on a.SalaryStrId equals e.SalaryStrId
                           join f in db.Employees on e.EmployeeId equals f.EmployeeId into empl
                           from f in empl.DefaultIfEmpty()
                               //let attend = db.AttendanceDetails.Where(a => a.EmployeeId == e.EmployeeId).FirstOrDefault()
                           where e.EmployeeId == EmpID && b.Name != "Gratuity" && b.Name != "Earnings In Annual Leave"
                           && b.Name != "Deduction In Annual Leave"
                           select new
                           {
                               a.PayHeadId,
                               b.Name,
                               a.Rate,

                               b.Type,
                               b.CalculationType,
                               b.Leave,
                               b.Compute,
                               b.days,
                               f.EmployeeId,
                               f.FirstName,
                               f.LastName,

                               b.CalculationPeriod,
                               b.CalculationBasis,

                               AtType = (from x in db.AttendanceDetails
                                         join y in db.Attendances on x.AttendanceId equals y.AttendanceId
                                         where x.EmployeeId == e.EmployeeId && y.AtDate.Month == fdate.Month
                                         && y.AtDate.Year == fdate.Year
                                         select new
                                         {
                                             x.AttendanceType
                                         }).FirstOrDefault(),

                               AtUnit = (from x in db.AttendanceDetails
                                         join y in db.Attendances on x.AttendanceId equals y.AttendanceId
                                         where x.EmployeeId == e.EmployeeId && y.AtDate.Month == fdate.Month
                                         && y.AtDate.Year == fdate.Year
                                         select new
                                         {
                                             x.Unit
                                         }).FirstOrDefault(),

                               AtValue = (from x in db.AttendanceDetails
                                          join y in db.Attendances on x.AttendanceId equals y.AttendanceId
                                          where x.EmployeeId == e.EmployeeId && y.AtDate.Month == fdate.Month
                                          && y.AtDate.Year == fdate.Year
                                          select new
                                          {
                                              x.Value
                                          }).FirstOrDefault(),

                               compute = db.Computeinfos.Where(x => x.Payhead == b.ID).ToList()

                           }).AsEnumerable().Select(o => new
                           {
                               o.PayHeadId,
                               PayHead = o.Name,
                               Rate = o.Rate,
                               days = o.days,
                               Per = o.CalculationPeriod,
                               HeadType = o.Type != null ? Enum.GetName(typeof(PayHeadType), o.Type) : "",
                               CalType = o.CalculationType != null ? Enum.GetName(typeof(CalcTypePayHead), o.CalculationType) : "",
                               Computed = o.Compute != null ? Enum.GetName(typeof(ComputPayHead), o.Compute) : "",
                               Basis = o.CalculationBasis != null ? Enum.GetName(typeof(CalcBasisPayHead), o.CalculationBasis) : "",
                               EmpId = o.EmployeeId,
                               EmpName = o.FirstName + " " + o.LastName,
                               o.Leave,
                               AtType = o.AtType.AttendanceType,
                               AtUnit = o.AtUnit.Unit,
                               AtValue = o.AtValue.Value,
                               o.CalculationPeriod,
                               o.compute
                           }).ToList();

                return new QuickSoft.Models.LegacyJsonResult { Data = new { sal } };
            }
            else
            {

                var first = (from a in db.SalaryStrDetails
                             join b in db.Payheads on a.PayHeadId equals b.ID into phead
                             from b in phead.DefaultIfEmpty()
                             join e in db.SalaryStructures on a.SalaryStrId equals e.SalaryStrId
                             join f in db.Employees on e.EmployeeId equals f.EmployeeId into empl
                             from f in empl.DefaultIfEmpty()
                             where e.EmployeeId == EmpID && b.Name != "Gratuity" && b.Name != "Earnings In Annual Leave"
                             && b.Name != "Deduction In Annual Leave"
                             select new
                             {
                                 a.PayHeadId,
                                 b.Name,
                                 a.Rate,

                                 b.Type,
                                 b.CalculationType,
                                 b.Leave,
                                 b.Compute,
                                 b.days,
                                 f.EmployeeId,
                                 f.FirstName,
                                 f.LastName,

                                 b.CalculationPeriod,
                                 b.CalculationBasis,
                                 b.AttendanceType,

                                 //compute = db.Computeinfos.Where(x => x.Payhead == b.ID).ToList()

                             }).AsEnumerable().Select(o => new
                             {
                                 o.PayHeadId,
                                 PayHead = o.Name,
                                 Rate = o.Rate,
                                 days = o.days,
                                 Per = o.CalculationPeriod,
                                 HeadType = o.Type != null ? Enum.GetName(typeof(PayHeadType), o.Type) : "",
                                 CalType = o.CalculationType != null ? Enum.GetName(typeof(CalcTypePayHead), o.CalculationType) : "",
                                 Computed = o.Compute != null ? Enum.GetName(typeof(ComputPayHead), o.Compute) : "",
                                 Basis = o.CalculationBasis != null ? Enum.GetName(typeof(CalcBasisPayHead), o.CalculationBasis) : "",
                                 EmpId = o.EmployeeId,
                                 EmpName = o.FirstName + " " + o.LastName,
                                 o.Leave,
                                 o.CalculationPeriod,
                                 o.AttendanceType,
                                 //o.compute
                             }).ToList();


                List<SalaryStructureDetail> sal = new List<SalaryStructureDetail>();

                //earning sum
                decimal earnSum = first.Where(a => a.PayHeadId == 0).Select(a => a.Rate).ToList().Sum() ?? 0;

                //deduction sum
                decimal deductSum = first.Where(a => a.PayHeadId == 1).Select(a => a.Rate).ToList().Sum() ?? 0;

                foreach (var arr in first)
                {
                    SalaryStructureDetail Str = new SalaryStructureDetail();
                    Str.PayHeadId = arr.PayHeadId;
                    Str.PayHead = arr.PayHead;
                    Str.EmpId = arr.EmpId;
                    Str.EmpName = arr.EmpName;

                    Str.Rate = arr.Rate;
                    Str.CalType = arr.CalType;
                    Str.Computed = arr.Computed;

                    var attCount = (from x in db.DailyAttendanceDetails
                                    join y in db.DailyAttendances on x.DailyAttendanceId equals y.DailyAttendanceId
                                    join z in db.AttendanceTypes on x.AtType equals z.Id
                                    where x.EmployeeId == arr.EmpId
                                    && (EF.Functions.DateDiffDay(x.AtDate, fdate) <= 0)
                                    && (EF.Functions.DateDiffDay(x.AtDate, tdate) >= 0)
                                    && z.Type == "2" //LEAVE WITHOUT PAY
                                    select new
                                    {
                                        x.DailyAttendanceDetailId
                                    }).Count();

                    var overtime = (decimal?)(from x in db.DailyAttendanceDetails
                                              join y in db.DailyAttendances on x.DailyAttendanceId equals y.DailyAttendanceId
                                              join z in db.AttendanceTypes on x.AtType equals z.Id
                                              where x.EmployeeId == arr.EmpId
                                              && (EF.Functions.DateDiffDay(x.AtDate, fdate) <= 0)
                                              && (EF.Functions.DateDiffDay(x.AtDate, tdate) >= 0)
                                              select new
                                              {
                                                  x.Overtime
                                              }).ToList().Sum(c => c.Overtime) ?? 0;

                    //var unit = (from a in db.AttendanceTypes
                    //            join b in db.payrollunits on a.Unit equals b.Id into pro
                    //            from b in pro.DefaultIfEmpty()
                    //            where a.Name == "OVER TIME HOURS"
                    //            select new
                    //            {
                    //                b.UnitName,
                    //            }).FirstOrDefault();
                    //var OTHr = unit.UnitName != null ? unit.UnitName.ToString() : "Hr";

                    if (arr.HeadType == "EarningsforEmployees")
                    {
                        Str.type = "Dr";
                        Str.rateprice = GetPayrollRate(arr.CalType,arr.CalculationPeriod,arr.Basis,arr.Rate, arr.Computed, arr.EmpId, arr.PayHeadId,days, attCount, overtime, earnSum,deductSum);

                    }
                    if (arr.HeadType == "DeductionsfromEmployees")
                    {
                        Str.type = "Cr";
                        Str.rateprice = GetPayrollRate(arr.CalType, arr.CalculationPeriod, arr.Basis, arr.Rate, arr.Computed, arr.EmpId, arr.PayHeadId, days, attCount, overtime, earnSum, deductSum);
                    }
                    if (arr.HeadType == "EmployeesStatutoryDeductions")
                    {
                        Str.type = "Cr";
                        Str.rateprice = GetPayrollRate(arr.CalType, arr.CalculationPeriod, arr.Basis, arr.Rate, arr.Computed, arr.EmpId, arr.PayHeadId, days, attCount, overtime, earnSum, deductSum);

                    }
                    if (arr.HeadType == "EmployeesStatutoryContributions")
                    {
                        Str.type = "Dr";
                        Str.rateprice = GetPayrollRate(arr.CalType, arr.CalculationPeriod, arr.Basis, arr.Rate, arr.Computed, arr.EmpId, arr.PayHeadId, days, attCount, overtime, earnSum, deductSum);

                    }
                    if (arr.HeadType == "EmployersOtherCharges")
                    {
                        Str.type = "Dr";
                        Str.rateprice = GetPayrollRate(arr.CalType, arr.CalculationPeriod, arr.Basis, arr.Rate, arr.Computed, arr.EmpId, arr.PayHeadId, days, attCount, overtime, earnSum, deductSum);

                    }
                    if (arr.HeadType == "Bonus")
                    {
                        Str.type = "Dr";
                        Str.rateprice = GetPayrollRate(arr.CalType, arr.CalculationPeriod, arr.Basis, arr.Rate, arr.Computed, arr.EmpId, arr.PayHeadId, days, attCount, overtime, earnSum, deductSum);


                    }
                    if (arr.HeadType == "LoansandAdvances")
                    {
                        Str.type = "Cr";
                        Str.rateprice = GetPayrollRate(arr.CalType, arr.CalculationPeriod, arr.Basis, arr.Rate, arr.Computed, arr.EmpId, arr.PayHeadId, days, attCount, overtime, earnSum, deductSum);

                    }
                    if (arr.HeadType == "ReimbursmentstoEmployees")
                    {
                        Str.type = "Dr";
                        Str.rateprice = GetPayrollRate(arr.CalType, arr.CalculationPeriod, arr.Basis, arr.Rate, arr.Computed, arr.EmpId, arr.PayHeadId, days, attCount, overtime, earnSum, deductSum);

                    }
                    sal.Add(Str);
                }


                return new QuickSoft.Models.LegacyJsonResult { Data = new { sal } };
            }
        }
        public decimal? GetPayrollRate(string CalType,string CalculationPeriod,string Basis,decimal? Rate, string Computed, long EmpId, long PayHeadId, int days,int attCount,decimal? overtime,decimal? earnSum, decimal? deductSum)
        {
            decimal? retrate = 0;
            if (CalType == "OnAttendance")
            {
                if (CalculationPeriod == "Months")
                {
                    if (Basis == "AsperCalenderPeriod")
                    {
                        var monthsal = Rate / days;
                        var minval = monthsal * attCount;
                        retrate = Rate - minval;
                    }
                }
            }
            if (CalType == "FlatRate")
            {
                retrate = Rate;
            }
            if (CalType == "DefinedValue")
            {
                retrate = 0;
            }
            if (CalType == "OnProduction")
            {
                var salVal = Rate * overtime;
                retrate = salVal;
            }
            if (CalType == "AsComputedValue")
            {
                retrate = ComputeValue(Computed, EmpId, PayHeadId,earnSum,deductSum);
            }
            return retrate;
        }

        public decimal ComputeValue(string Computed,long EmpId,long PayHeadId,decimal? earnSum, decimal? deductSum)
        {
            decimal? basicpay = 0;
            if (Computed == "DeductionsTotal")
            {
                basicpay = deductSum;
            }
            else if (Computed == "EarningsTotal")
            {
                basicpay = earnSum;
            }
            else if (Computed == "CurrentSubtotal")
            {
                basicpay = earnSum - deductSum;
            }

            //compute info
            decimal computeAmt = (decimal?)(from a in db.Computeinfos
                                             where a.Payhead == PayHeadId
                                             && a.Amountgreatethan <= basicpay && basicpay <= a.Amountupto
                                             select new
                                             {
                                                 Amt = (a.Slabtype == "1") ? (basicpay * (a.value / 100)) : a.value
                                             }).FirstOrDefault().Amt ?? 0;
            return computeAmt;
        }


        //public decimal CalculateRate(long PayHead, long EmpId, decimal? Rate)
        //{
        //    decimal rate = 0;
        //    int monthdays = 30;
        //    var payheads = db.Payheads.Where(a => a.ID == PayHead).FirstOrDefault();
        //    var attend = db.AttendanceDetails.Where(a => a.EmployeeId == EmpId).FirstOrDefault();
        //    if (payheads.Type == PayHeadType.EarningsforEmployees)
        //    {
        //        if (payheads.CalculationType == CalcTypePayHead.OnAttendance)
        //        {
        //            if (payheads.CalculationPeriod == "Months")
        //            {
        //                if (payheads.CalculationBasis == CalcBasisPayHead.AsperCalenderPeriod)
        //                {
        //                    if (attend.AttendanceType == 1)//default set absent
        //                    {
        //                        if (attend.Unit == "Days")
        //                        {
        //                            decimal monthsal = rate / monthdays;
        //                            rate = rate - (monthsal * attend.Value);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }


        //    return rate;
        //}

        public JsonResult SearchParentGrade(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.SalaryStructures
                                  join b in db.Employees on a.EmployeeId equals b.EmployeeId into emp
                                  from b in emp.DefaultIfEmpty()
                                  where (b.FirstName.ToLower().Contains(q.ToLower()) || b.MiddleName.ToLower().Contains(q.ToLower()) || b.LastName.ToLower().Contains(q.ToLower()))
                                  select new SelectFormat
                                  {
                                      text = b.FirstName + " " + b.LastName,
                                      id = b.EmployeeId
                                  }).Distinct().OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.SalaryStructures
                                  join b in db.Employees on a.EmployeeId equals b.EmployeeId into emp
                                  from b in emp.DefaultIfEmpty()
                                  select new SelectFormat
                                  {
                                      text = b.FirstName + " " + b.LastName,
                                      id = b.EmployeeId
                                  }).Distinct().OrderBy(b => b.text).ToList();
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Name" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        [HttpGet]
        public JsonResult GetSalaryStructureByParentGrade(long empid)
        {
            var emp = db.EmployeeWorkDetails.Where(a => a.EmployeeId == empid).FirstOrDefault();
            var v = (from a in db.EmpGradeSalaryDetails
                     join b in db.Payheads on a.Payhead equals b.ID into phead
                     from b in phead.DefaultIfEmpty()
                     where a.EmployeeGradeId == emp.EmployeeGrade
                     //join e in db.SalaryStructures on a.SalaryStrId equals e.SalaryStrId
                     //where e.EmployeeId == pgrade
                     select new
                     {
                         //e.EFDate,
                         a.Payhead,
                         b.Name,
                         a.Rate,
                         b.CalculationPeriod,
                         b.Type,
                         b.CalculationType,
                         b.Compute
                     }).AsEnumerable().Select(o => new
                     {
                         //o.EFDate,
                         PayHeadId = o.Payhead,
                         PayHead = o.Name,
                         o.Rate,
                         Per = o.CalculationPeriod,
                         HeadType = Enum.GetName(typeof(PayHeadType), o.Type),
                         CalType = Enum.GetName(typeof(CalcTypePayHead), o.CalculationType),
                         Computed = Enum.GetName(typeof(ComputPayHead), o.Compute),
                     }).ToList();

            return Json(v);

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