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
    public class AttendanceController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        EmployeeController empcntrl;
        public AttendanceController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: Hr/Attendance
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Attendance List")]
        public ActionResult Index()
        {
            return View();
        }

        [RedirectingAction]
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Attendance List")]
        public ActionResult GetAttendance(long? BName, long? Item, decimal? Qty, long? Unit)
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

            var v = (from a in db.Attendances
                     join e in db.Users on a.CreatedBy equals e.Id
                     select new
                     {
                         a.AttendanceId,
                         a.VoucherNo,
                         a.AtDate,
                         e.UserName,
                         a.CreatedDate
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.VoucherNo.ToString().ToLower().Equals(search.ToLower()));
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
        [QkAuthorize(Roles = "Dev,Create Attendance")]
        public ActionResult Create()
        {
            AttendanceViewModel vmodel = new AttendanceViewModel();
            vmodel.VoucherNo = InvoiceNo();
            vmodel.AtDate = System.DateTime.Now.ToString("dd-MM-yyyy");

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
            return View(vmodel);
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create Attendance")]
        public JsonResult CreateAttendance(AttendanceViewModel vmodel)
        {
            string msg;
            bool stat;
            if (!BillExist(Convert.ToString(vmodel.VoucherNo)))
            {
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

                Attendance attend = new Attendance
                {
                    VoucherNo = vmodel.VoucherNo,
                    AtNo = GetAttNo(),
                    AtDate = DateTime.Parse(vmodel.AtDate.ToString(), new CultureInfo("en-GB")),
                    Branch = Branch,
                    Note = vmodel.Note,
                    Remarks = vmodel.Remarks,

                    CreatedDate = today,
                    CreatedBy = UserId,
                    Status = Status.active,
                };
                db.Attendances.Add(attend);
                db.SaveChanges();
                Int64 AttId = attend.AttendanceId;

                AttendanceDetail attdetail = new AttendanceDetail();
                foreach (var arr in vmodel.empitems)
                {
#pragma warning disable format
                    if (arr.EmpName  !=null || arr.EmployeeId > 0)
#pragma warning restore format
                    {
                        attdetail.AttendanceId = AttId;
                        //employee
                        if (arr.EmployeeId == 0)
                        {
                            attdetail.EmployeeId = getEmployeeId(arr.EmpName,arr.EmpCode); ;
                        }
                        else
                        {
                            attdetail.EmployeeId = arr.EmployeeId;
                        }

                        //attemnedence type
                        if (arr.AttendanceType == 0)
                        {
                            attdetail.AttendanceType = createAttType(arr.ATypeName);
                        }
                        else
                        {
                            attdetail.AttendanceType = arr.AttendanceType;
                        }

                        attdetail.Value = arr.Value;
                        attdetail.Unit = arr.Unit;
                        db.AttendanceDetails.Add(attdetail);
                        db.SaveChanges();
                    }
                }

                com.addlog(LogTypes.Created, UserId, "Attendance", "Attendances", findip(), AttId, "Successfully Submitted Attendance");

                msg = "Successfully added Attendance details.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                msg = "Voucher No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }

        public long createAttType(string ATypeName)
        {
            Int64 atID = 0;
            var Exists = db.AttendanceTypes.Any(c => c.Name == ATypeName);
            if (Exists)
            {
                atID = db.AttendanceTypes.Where(c => c.Name == ATypeName).Select(c => c.Id).FirstOrDefault();
            }
            else
            {
                AttendanceType attype = new AttendanceType();
                attype.Name = ATypeName;
                db.AttendanceTypes.Add(attype);
                db.SaveChanges();
                atID = attype.Id;
            }
            return atID;
        }
        public long getEmployeeId(string EmpName,string EmpCode)
        {
            Int64 empID = 0;
            var Exists = db.Employees.Any(c => c.EMPCode == EmpCode);
            var NameExists = db.Employees.Any(c => c.FirstName == EmpName);
            if (Exists)
            {
                empID = db.Employees.Where(c => c.EMPCode == EmpCode).Select(c => c.EmployeeId).FirstOrDefault();
            }
            else if ((EmpCode == "" || EmpCode == null) || NameExists)
            {
                empID = db.Employees.Where(c => c.FirstName == EmpName).Select(c => c.EmployeeId).FirstOrDefault();
            }
            else
            {
                var UserId = User.Identity.GetUserId();
                var Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                Employee emp = new Employee();
                emp.FirstName = EmpName;
                if (EmpCode == "" || EmpCode == null)
                {
                    emp.EMPCode = empcntrl.getEmpCode();
                }
                else
                {
                    emp.EMPCode = EmpCode;
                }
                emp.PAddress = 0;
                emp.CAddress = 0;
                emp.BranchID = Branch;
                emp.CreatedDate = System.DateTime.Now;
                db.Employees.Add(emp);
                db.SaveChanges();
                empID = emp.EmployeeId;
            }
            return empID;
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Attendance")]
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
            Attendance atten = db.Attendances.Find(id);

            var Bnch = db.Branchs
                   .Select(s => new
                   {
                       Id = s.BranchID,
                       Name = s.BranchName
                   }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            if (atten == null)
            {
                return NotFound();
            }

            AttendanceViewModel vmodel = new AttendanceViewModel();
            vmodel.AttendanceId = (long)id;
            vmodel.VoucherNo = atten.VoucherNo;
            vmodel.Note = atten.Note;
            vmodel.Remarks = atten.Remarks;
            vmodel.AtDate = atten.AtDate.ToString("dd-MM-yyyy");

            return View(vmodel);
        }
        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Attendance")]
        public ActionResult EditAttendance(AttendanceViewModel vmodel)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            var today = Convert.ToDateTime(System.DateTime.Now);
            long Branch = 0;

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            Attendance attend = db.Attendances.Find(vmodel.AttendanceId);

            if (BranchCheck == Status.active)
            {
                Branch = (long)vmodel.Branch;
            }
            else
            {
                Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
            }

            if (BillExist(Convert.ToString(vmodel.VoucherNo)) && Convert.ToString(vmodel.VoucherNo) != attend.VoucherNo)
            {
                msg = "Voucher No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

            attend.VoucherNo = vmodel.VoucherNo;
            attend.Note = vmodel.Note;
            attend.Remarks = vmodel.Remarks;
            attend.Branch = Branch;
            attend.AtDate = DateTime.Parse(vmodel.AtDate.ToString(), new CultureInfo("en-GB"));

            db.Entry(attend).State = EntityState.Modified;
            db.SaveChanges();
            Int64 attendId = attend.AttendanceId;


            var bItems = db.AttendanceDetails.Where(a => a.AttendanceId == attendId).FirstOrDefault();
            if (bItems != null)
            {
                db.AttendanceDetails.RemoveRange(db.AttendanceDetails.Where(a => a.AttendanceId == attendId));
                db.SaveChanges();
            }
            AttendanceDetail attdetail = new AttendanceDetail();
            foreach (var arr in vmodel.empitems)
            {
                if (arr.EmployeeId > 0)
                {
                    attdetail.AttendanceId = attendId;
                    attdetail.EmployeeId = arr.EmployeeId;
                    attdetail.AttendanceType = arr.AttendanceType;
                    attdetail.Value = arr.Value;
                    attdetail.Unit = arr.Unit;
                    db.AttendanceDetails.Add(attdetail);
                    db.SaveChanges();
                }
            }

            com.addlog(LogTypes.Updated, UserId, "Attendance", "Attendances", findip(), attendId, "Successfully Updated Attendance");
            msg = "Successfully Updated Attendance .";
            stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete Attendance")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Attendance atten = db.Attendances.Find(id);
            if (atten == null)
            {
                return NotFound();
            }
            return PartialView(atten);
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete Attendance")]
        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            Attendance attend = db.Attendances.Find(id);

            var Msg = chkDeleteWithMsg(id);
            if (Msg != null)
            {
                msg = Msg;
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Successfully deleted Attendances.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Attendance")]
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
                Success("Deleted " + count + " Attendance, Unable to Delete " + notdel + " Attendance. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Attendance.", true);
            }
            else
            {
                Success("Deleted " + count + " Attendance.", true);
            }
            return RedirectToAction("Index", "Attendance");
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

        public ActionResult AutoFill()
        {
            var use = db.Employees
                  .Select(s => new
                  {
                      ID = s.EmployeeId,
                      Name = s.FirstName + " " + s.LastName
                  })
                  .ToList();

            ViewBag.Employee = QkSelect.List(use, "ID", "Name");

            AttendanceAutoFillViewModel vmodel = new AttendanceAutoFillViewModel();
            return PartialView(vmodel);
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
            Attendance att = db.Attendances.Find(id);
            var attend = db.AttendanceDetails.Where(a => a.AttendanceId == id);
            if (attend != null)
            {
                db.AttendanceDetails.RemoveRange(db.AttendanceDetails.Where(a => a.AttendanceId == id));
                db.SaveChanges();
            }
            if (att != null)
            {
                db.Attendances.RemoveRange(db.Attendances.Where(a => a.AttendanceId == id));
                db.SaveChanges();
            }
            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "Attendance", "Attendances", findip(), id, "Successfully Deleted Attendance");
            db.SaveChanges();
            return true;
        }
        [HttpGet]
        public JsonResult GetAttendanceById(int AttID)
        {
            var v = (from a in db.AttendanceDetails
                     join b in db.Employees on a.EmployeeId equals b.EmployeeId into emp
                     from b in emp.DefaultIfEmpty()
                     join c in db.AttendanceTypes on a.AttendanceType equals c.Id into atype
                     from c in atype.DefaultIfEmpty()
                     where a.AttendanceId == AttID
                     select new
                     {
                         b.EMPCode,
                         a.EmployeeId,
                         EmpName = b.FirstName + " " + b.LastName,
                         a.AttendanceType,
                         AttType = c.Name,
                         a.Unit,
                         a.Value
                     }).ToList();

            return Json(v);

        }

        [HttpGet]
        public JsonResult GetAutoFillEmployee(decimal AFValue, string AFUnit, long AtType, string SelEmp)
        {
            var aftype = db.AttendanceTypes.Where(a => a.Id == AtType).FirstOrDefault();

            List<long> listemp = new List<long>();

            if (SelEmp == "0") {
                listemp = db.EmployeeWorkDetails.Where(a => a.EmployeeAccount != null).Select(a => a.EmployeeId).ToList();
            }
            else
            {
                string[] empList = SelEmp.Split(',');
                foreach (var arr in empList)
                {
                    listemp.Add(Convert.ToInt64(arr));
                }
            }
            var v = (from a in db.Employees
                     where listemp.Contains(a.EmployeeId)
                     select new
                     {
                         EmpCode = a.EMPCode,
                         EmployeeId = a.EmployeeId,
                         EmpName = a.FirstName + " " + a.LastName,
                         AttendanceType = aftype.Id,
                         AttType = aftype.Name,
                         Unit = AFUnit,
                         Value = AFValue
                     }).ToList();

            return Json(v);

        }

        [HttpGet]
        public virtual ActionResult DownloadExcel(string file)
        {
            string fullPath = Path.Combine(LegacyWeb.MapPath("~/uploads/excelitem/excelformat/Attendance.xlsx"));
            string fileName = "Attendance.xlsx";
            
            return File(fullPath, "application/vnd.ms-excel", fileName);
        }

        private string InvoiceNo(Int64 PNo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "Attendance").Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == "Attendance").Select(a => a.number).FirstOrDefault();

            if (billNo == null)
            {
                if ((db.Attendances.Select(p => p.AtNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    PNo = db.Attendances.Max(p => p.AtNo + 1);
                    billNo = companyPrefix + PNo;
                    if (BillExist(billNo))
                    {
                        billNo = InvoiceNo(PNo, billNo);
                    }
                }
            }
            else
            {
                PNo = PNo + 1;
                billNo = companyPrefix + PNo;
                if (BillExist(billNo))
                {
                    billNo = InvoiceNo(PNo, billNo);
                }

            }
            return billNo;
        }
        private bool BillExist(string VcNo)
        {
            var Exists = db.Attendances.Any(c => c.VoucherNo == VcNo);
            bool res = (Exists) ? true : false;
            return res;
        }
        private long GetAttNo()
        {
            Int64 AtNo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "Attendance").Select(a => a.number).FirstOrDefault();
            if ((db.Attendances.Select(p => p.AtNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                AtNo = (number == 0) ? 1 : number;
            }
            else
            {
                AtNo = db.Attendances.Max(p => p.AtNo + 1);
            }

            return AtNo;
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