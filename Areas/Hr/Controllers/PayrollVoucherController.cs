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
    public class PayrollVoucherController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PayrollVoucherController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: Hr/PayrollVoucher
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,PayrollVoucher List")]
        public ActionResult Index()
        {
            return View();
        }

        [RedirectingAction]
        [HttpPost]
        [QkAuthorize(Roles = "Dev,PayrollVoucher List")]
        public ActionResult GetPayrollVoucher()
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

            var v = (from a in db.PayrollVouchers
                     join b in db.Accountss on a.Acccount equals b.AccountsID into acc
                     from b in acc.DefaultIfEmpty()
                     join e in db.Users on a.CreatedBy equals e.Id
                     select new
                     {
                         a.PayrollVoucherId,
                         a.VoucherNo,
                         a.FromDate,
                         a.ToDate,
                         a.PRDate,
                         Account = b.Name,
                         e.UserName,
                         a.CreatedDate
                     });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.VoucherNo.ToString().ToLower().Equals(search.ToLower()) ||
                                 p.Account.ToString().ToLower().Contains(search.ToLower()) );
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
        [QkAuthorize(Roles = "Dev,Create PayrollVoucher")]
        public ActionResult Create()
        {
            PayrollVoucherViewModel vmodel = new PayrollVoucherViewModel();
            vmodel.VoucherNo = InvoiceNo();
            vmodel.PRDate = System.DateTime.Now.ToString("dd-MM-yyyy");

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

            var Acc = db.Accountss.Where(a => a.AccountsID != 4 && a.Group != 23).Select(a => new
            {
                ID = a.AccountsID,
                Name = a.Name
            }).ToList();
            ViewBag.Acc = QkSelect.List(Acc, "Id", "Name");
            _FinancialYear();
            return View(vmodel);
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create PayrollVoucher")]
        public JsonResult CreatePayrollVoucher(PayrollVoucherViewModel vmodel)
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

                PayrollVoucher payv = new PayrollVoucher();
                payv.VoucherNo = vmodel.VoucherNo;
                payv.PRNo = GetPRNo();
                payv.PRDate = DateTime.Parse(vmodel.PRDate.ToString(), new CultureInfo("en-GB"));
                payv.Acccount = vmodel.Acccount;
                payv.Branch = Branch;
                payv.Note = vmodel.Note;
                payv.Details = vmodel.Details;
                payv.GrandTotal = vmodel.GrandTotal;

                //if (vmodel.FromDate != null && vmodel.FromDate!="")
                //{
                payv.FromDate = DateTime.Parse(vmodel.FromDate.ToString(), new CultureInfo("en-GB"));
                //}
                //if (vmodel.ToDate != null && vmodel.ToDate != "")
                //{
                payv.ToDate = DateTime.Parse(vmodel.ToDate.ToString(), new CultureInfo("en-GB"));
                //}

                payv.CreatedDate = today;
                payv.CreatedBy = UserId;
                payv.Status = Status.active;
                db.PayrollVouchers.Add(payv);
                db.SaveChanges();
                Int64 PRId = payv.PayrollVoucherId;

                if (vmodel.employee != null)
                {
                    foreach (var arr in vmodel.employee)
                    {
                        if (arr.Employee > 0)
                        {
                            var empacc = db.EmployeeWorkDetails.Where(a => a.EmployeeId == arr.Employee).Select(a => a.EmployeeAccount).FirstOrDefault();
                            PayrollVoucherEmployee payemp = new PayrollVoucherEmployee
                            {
                                EmployeeId = arr.Employee,
                                PayrollVoucherId = PRId,
                                EmpAccount = empacc
                            };
                            db.PayrollVoucherEmployees.Add(payemp);
                            db.SaveChanges();
                            Int64 PREmpId = payemp.PayrollEmployeeId;

                            Int64? SAccountEmp = db.EmployeeWorkDetails.Where(a => a.EmployeeId == arr.Employee).Select(a => a.EmployeeAccount).FirstOrDefault();
                            Int64? SAccount = db.companys.Select(a => a.SalaryAccount).FirstOrDefault();


                            if (vmodel.salarystr != null)
                            {
                                decimal TotalRate = 0;
                                foreach (var arry in vmodel.salarystr)
                                {
                                    if (arry.PayHeadId > 0 && arry.EmpId == arr.Employee)
                                    {
                                        PayrollVoucherSalary paysalary = new PayrollVoucherSalary
                                        {
                                            PayrollVoucherId = PRId,
                                            EmployeeId = arry.EmpId,
                                            PayrollEmployeeId = PREmpId,
                                            PayHeadId = arry.PayHeadId,
                                            Rate = arry.Rate,
                                            CrDr = arry.CrDr
                                        };
                                        db.PayrollVoucherSalarys.Add(paysalary);
                                        db.SaveChanges();
                                        if (arry.CrDr == "Dr")
                                        {
                                            TotalRate = Convert.ToDecimal(TotalRate + arry.Rate);
                                        }
                                        else
                                        {
                                            TotalRate = Convert.ToDecimal(TotalRate - arry.Rate);
                                        }
                                        //pass to acc transaction
                                    }
                                }
                                if (SAccountEmp != null && SAccount != null)
                                {
                                    com.addAccountTrasaction(0, TotalRate, (long)SAccount, "Payroll Voucher", PREmpId, DC.Credit, payv.PRDate, null, null, null, null);

                                    com.addAccountTrasaction(TotalRate, 0, (long)SAccountEmp, "Payroll Voucher", PREmpId, DC.Debit, payv.PRDate, null, null, null, null);
                                }
                            }

                        }
                    }
                }

                com.addlog(LogTypes.Created, UserId, "PayrollVoucher", "PayrollVouchers", findip(), PRId, "Successfully Submitted Payroll Vouchers");
                msg = "Successfully added Payroll Voucher details.";
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

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit PayrollVoucher")]
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
            PayrollVoucher payvch = db.PayrollVouchers.Find(id);

            var Bnch = db.Branchs
                   .Select(s => new
                   {
                       Id = s.BranchID,
                       Name = s.BranchName
                   }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            var Acc = db.Accountss.Where(a => a.AccountsID != 4 && a.Group != 23).Select(a => new
            {
                ID = a.AccountsID,
                Name = a.Name
            }).ToList();
            ViewBag.Acc = QkSelect.List(Acc, "Id", "Name");

            if (payvch == null)
            {
                return NotFound();
            }

            PayrollVoucherViewModel vmodel = new PayrollVoucherViewModel();
            vmodel.PayrollVoucherId = (long)id;
            vmodel.Acccount = payvch.Acccount;
            vmodel.VoucherNo = payvch.VoucherNo;
            vmodel.PRDate = payvch.PRDate.ToString("dd-MM-yyyy");
            if (payvch.FromDate != null)
            {
                vmodel.FromDate = payvch.FromDate.ToString("dd-MM-yyyy");
            }
            if (payvch.ToDate != null)
            {
                vmodel.ToDate = payvch.ToDate.ToString("dd-MM-yyyy");
            }

            vmodel.GrandTotal = payvch.GrandTotal;
            vmodel.Note = payvch.Note;
            vmodel.Details = payvch.Details;
            _FinancialYear();
            return View(vmodel);
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit PayrollVoucher")]
        public ActionResult EditPayrollVoucher(PayrollVoucherViewModel vmodel)
        {
            bool stat = false;
            string msg;
            PayrollVoucher payvch = db.PayrollVouchers.Find(vmodel.PayrollVoucherId);

            if (BillExist(vmodel.VoucherNo) && vmodel.VoucherNo != payvch.VoucherNo)
            {

                msg = "Voucher No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

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

            payvch.VoucherNo = vmodel.VoucherNo;
            payvch.Acccount = vmodel.Acccount;
            payvch.Details = vmodel.Details;
            payvch.Note = vmodel.Note;
            payvch.GrandTotal = vmodel.GrandTotal;
            payvch.PRDate = DateTime.Parse(vmodel.PRDate.ToString(), new CultureInfo("en-GB"));

            //if (vmodel.FromDate != null && vmodel.FromDate != "")
            //{
            payvch.FromDate = DateTime.Parse(vmodel.FromDate.ToString(), new CultureInfo("en-GB"));
            //}
            //if (vmodel.ToDate != null && vmodel.ToDate != "")
            //{
            payvch.ToDate = DateTime.Parse(vmodel.ToDate.ToString(), new CultureInfo("en-GB"));
            //}

            db.Entry(payvch).State = EntityState.Modified;
            db.SaveChanges();
            Int64 PRId = payvch.PayrollVoucherId;

            var paysal = db.PayrollVoucherSalarys.Where(a => a.PayrollVoucherId == PRId).FirstOrDefault();
            if (paysal != null)
            {
                db.PayrollVoucherSalarys.RemoveRange(db.PayrollVoucherSalarys.Where(a => a.PayrollVoucherId == PRId));
                db.SaveChanges();
            }
            var payemps = db.PayrollVoucherEmployees.Where(a => a.PayrollVoucherId == PRId).FirstOrDefault();
            if (payemps != null)
            {
                db.PayrollVoucherEmployees.RemoveRange(db.PayrollVoucherEmployees.Where(a => a.PayrollVoucherId == PRId));
                db.SaveChanges();
            }
           
            if (vmodel.employee != null)
            {
                foreach (var arr in vmodel.employee)
                {
                    if (arr.Employee > 0)
                    {
                        var empacc = db.EmployeeWorkDetails.Where(a => a.EmployeeId == arr.Employee).Select(a => a.EmployeeAccount).FirstOrDefault();
                        PayrollVoucherEmployee payemp = new PayrollVoucherEmployee
                        {
                            EmployeeId = arr.Employee,
                            PayrollVoucherId = PRId,
                            EmpAccount = empacc
                        };
                        db.PayrollVoucherEmployees.Add(payemp);
                        db.SaveChanges();
                        Int64 PREmpId = payemp.PayrollEmployeeId;

                        Int64? SAccountEmp = db.EmployeeWorkDetails.Where(a => a.EmployeeId == arr.Employee).Select(a => a.EmployeeAccount).FirstOrDefault();
                        Int64? SAccount = db.companys.Select(a => a.SalaryAccount).FirstOrDefault();

                        if (vmodel.salarystr != null)
                        {
                            decimal TotalRate = 0;
                            foreach (var arry in vmodel.salarystr)
                            {
                                if (arry.PayHeadId > 0 && arry.EmpId == arr.Employee)
                                {
                                    PayrollVoucherSalary paysalary = new PayrollVoucherSalary
                                    {
                                        PayrollVoucherId = PRId,
                                        EmployeeId = arry.EmpId,
                                        PayrollEmployeeId = PREmpId,
                                        PayHeadId = arry.PayHeadId,
                                        Rate = arry.Rate,
                                        CrDr = arry.CrDr
                                    };
                                    db.PayrollVoucherSalarys.Add(paysalary);
                                    db.SaveChanges();
                                    if (arry.CrDr == "Dr")
                                    {
                                        TotalRate = Convert.ToDecimal(TotalRate + arry.Rate);
                                    }
                                    else
                                    {
                                        TotalRate = Convert.ToDecimal(TotalRate - arry.Rate);
                                    }
                                }
                            }
                            if (SAccountEmp != null && SAccount != null)
                            {
                                //delete based on Payroll Employee Id
                                bool delete = com.DeleteAllAccountTransaction("Payroll Voucher", payemps.PayrollEmployeeId);

                                com.addAccountTrasaction(0, TotalRate, (long)SAccount, "Payroll Voucher", PREmpId, DC.Credit, payvch.PRDate, null, null, null, null);

                                com.addAccountTrasaction(TotalRate, 0, (long)SAccountEmp, "Payroll Voucher", PREmpId, DC.Debit, payvch.PRDate, null, null, null, null);
                            }
                        }
                    }
                }
            }

            com.addlog(LogTypes.Updated, UserId, "PayrollVoucher", "PayrollVouchers", findip(), PRId, "Successfully Updated Payroll Voucher");
            msg = "Successfully Updated Payroll Voucher .";
            stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete PayrollVoucher")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PayrollVoucher payvch = db.PayrollVouchers.Find(id);
            if (payvch == null)
            {
                return NotFound();
            }
            return PartialView(payvch);
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete PayrollVoucher")]
        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            PayrollVoucher pvch = db.PayrollVouchers.Find(id);

            var Msg = chkDeleteWithMsg(id);
            if (Msg != null)
            {
                msg = Msg;
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Successfully deleted Payroll Voucher.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete PayrollVoucher")]
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
                Success("Deleted " + count + " Payroll Voucher, Unable to Delete " + notdel + " Payroll Voucher. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Payroll Voucher.", true);
            }
            else
            {
                Success("Deleted " + count + " Payroll Voucher.", true);
            }
            return RedirectToAction("Index", "PayrollVoucher");
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
            PayrollVoucher pvchr = db.PayrollVouchers.Find(id);

            var sdetail = db.PayrollVoucherEmployees.Where(a => a.PayrollVoucherId == id);
            if (sdetail != null)
            {
                db.PayrollVoucherEmployees.RemoveRange(db.PayrollVoucherEmployees.Where(a => a.PayrollVoucherId == id));
                db.SaveChanges();
            }

            var pvsalary = db.PayrollVoucherSalarys.Where(a => a.PayrollVoucherId == id);
            if (pvsalary != null)
            {
                db.PayrollVoucherSalarys.RemoveRange(db.PayrollVoucherSalarys.Where(a => a.PayrollVoucherId == id));
                db.SaveChanges();
            }

            if (pvchr != null)
            {
                db.PayrollVouchers.RemoveRange(db.PayrollVouchers.Where(a => a.PayrollVoucherId == id));
                db.SaveChanges();
            }
            bool deletepay = com.DeleteAllAccountTransaction("Payroll Voucher", id);
            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "PayrollVoucher", "PayrollVouchers", findip(), id, "Successfully Deleted Payroll Voucher");
            db.SaveChanges();
            return true;
        }

        public ActionResult AutoFill()
        {
            ViewBag.ProFor = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "User Defined", Value="User Defined"},
                new SelectListItem() {Text = "ESI Contribution", Value="ESI Contribution"},
                new SelectListItem() {Text = "NPS Contribution", Value="NPS Contribution"},
                new SelectListItem() {Text = "PF Contribution", Value="PF Contribution"},
                new SelectListItem() {Text = "Salary", Value="Salary"},
            }, "Value", "Text");

            var emp = (from a in db.SalaryStructures
                       join b in db.Employees on a.EmployeeId equals b.EmployeeId into empz
                       from b in empz.DefaultIfEmpty()
                       select new
                       {
                           Name = b.FirstName + " " + b.LastName,
                           Id = b.EmployeeId
                       }).Distinct().OrderBy(b => b.Name).ToList();
            ViewBag.SelVal = QkSelect.List(emp, "Id", "Name");

            var Acc = db.Accountss.Where(a => a.AccountsID != 4 && a.Group != 23).Select(a => new
            {
                ID = a.AccountsID,
                Name = a.Name
            }).ToList();
            ViewBag.Acc = QkSelect.List(Acc, "Id", "Name");

            return PartialView();
        }


        [HttpGet]
        public JsonResult GetPayrollVoucherAllById(int PayVID)
        {
            var emps = (from a in db.PayrollVoucherEmployees
                        join b in db.Employees on a.EmployeeId equals b.EmployeeId into emp
                        from b in emp.DefaultIfEmpty()
                        where a.PayrollVoucherId == PayVID
                        select new
                        {
                            EmployeeId = a.EmployeeId,
                            FirstName = b.FirstName,
                            LastName = b.LastName,
                        }).ToList();

            var v = emps.Select(a => new
            {
                EMPId = a.EmployeeId,
                EMPName = a.FirstName + " " + a.LastName,

                salary = (from aa in db.PayrollVoucherSalarys
                          join bb in db.Payheads on aa.PayHeadId equals bb.ID into phead
                          from bb in phead.DefaultIfEmpty()
                          join ff in db.Employees on aa.EmployeeId equals ff.EmployeeId into empl
                          from ff in empl.DefaultIfEmpty()
                          //let attend = db.AttendanceDetails.Where(xx => xx.EmployeeId == a.EmployeeId).FirstOrDefault()
                          where aa.EmployeeId == a.EmployeeId && aa.PayrollVoucherId == PayVID
                          select new
                          {
                              aa.PayHeadId,
                              bb.Name,
                              aa.Rate,
                              aa.CrDr,
                              bb.Type,
                              bb.CalculationType,
                              bb.Leave,
                              bb.Compute,
                              bb.days,
                              ff.EmployeeId,
                              ff.FirstName,
                              ff.LastName,

                              bb.CalculationPeriod,
                              bb.CalculationBasis,

                          }).AsEnumerable().Select(o => new
                          {
                              o.PayHeadId,
                              PayHead = o.Name,
                              Rate = o.Rate,
                              o.CrDr,
                              days = o.days,
                              Per = o.CalculationPeriod,
                              HeadType = o.Type,
                              CalType = o.CalculationType,//Enum.GetName(typeof(CalcTypePayHead), o.CalculationType),
                              Computed = o.Compute,
                              Basis = o.CalculationBasis,
                              EmpId = o.EmployeeId,
                              EmpName = o.FirstName + " " + o.LastName,
                              o.Leave,
                              o.CalculationPeriod,
                          }).ToList(),

            }).ToList();

            return Json(v);
        }

        [HttpGet]
        public JsonResult GetAutoFillVoucherDetail(string ProcessFor, string FromDate, string ToDate, string SelEmp, long? Acc)
        {
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

            if (!fdate.HasValue || !tdate.HasValue) { return Json(0); }
            TimeSpan ts = tdate.Value.Date - fdate.Value.Date;
            int days = ts.Days + 1; // Calc fix (N1): inclusive calendar-day count — matches SalaryStructureController:446. Was ts.Days, so a 30-day month divided salary by 29 (per-day ~3.4% off, inconsistent with the salary structure) and From==To crashed (Rate/0).


            List<long> listemp = new List<long>();
            string[] empList = SelEmp.Split(',');
            foreach (var arr in empList)
            {
                listemp.Add(Convert.ToInt64(arr));
            }


            var AtVoucher = db.EnableSettings.Where(a => a.EnableType == "PayAttendance").FirstOrDefault();
            if (AtVoucher.TypeValue == "Attendance Voucher")
            {
                //pending
                var emps = (from a in db.Employees
                            where listemp.Contains(a.EmployeeId)
                            select new
                            {
                                a.EmployeeId,
                                a.FirstName,
                                a.LastName,
                            }).ToList();

                var v = emps.Select(a => new
                {
                    EMPId = a.EmployeeId,
                    EMPName = a.FirstName + " " + a.LastName,

                    salary = (from aa in db.SalaryStrDetails
                              join bb in db.Payheads on aa.PayHeadId equals bb.ID into phead
                              from bb in phead.DefaultIfEmpty()
                              join ee in db.SalaryStructures on aa.SalaryStrId equals ee.SalaryStrId
                              join ff in db.Employees on ee.EmployeeId equals ff.EmployeeId into empl
                              from ff in empl.DefaultIfEmpty()
                              where ee.EmployeeId == a.EmployeeId && bb.Name != "Gratuity" && bb.Name != "Earnings In Annual Leave"
                              && bb.Name != "Deduction In Annual Leave"
                              select new
                              {
                                  aa.PayHeadId,
                                  bb.Name,
                                  aa.Rate,

                                  bb.Type,
                                  bb.CalculationType,
                                  bb.Leave,
                                  bb.Compute,
                                  bb.days,
                                  ff.EmployeeId,
                                  ff.FirstName,
                                  ff.LastName,

                                  bb.CalculationPeriod,
                                  bb.CalculationBasis,

                                  AtType = (from x in db.AttendanceDetails
                                            join y in db.Attendances on x.AttendanceId equals y.AttendanceId
                                            where x.EmployeeId == a.EmployeeId && y.AtDate.Month == fdate.Value.Month
                                            && y.AtDate.Year == fdate.Value.Year
                                            select new
                                            {
                                                x.AttendanceType
                                            }).FirstOrDefault(),

                                  AtUnit = (from x in db.AttendanceDetails
                                            join y in db.Attendances on x.AttendanceId equals y.AttendanceId
                                            where x.EmployeeId == a.EmployeeId && y.AtDate.Month == fdate.Value.Month
                                            && y.AtDate.Year == fdate.Value.Year
                                            select new
                                            {
                                                x.Unit
                                            }).FirstOrDefault(),

                                  AtValue = (from x in db.AttendanceDetails
                                             join y in db.Attendances on x.AttendanceId equals y.AttendanceId
                                             where x.EmployeeId == a.EmployeeId && y.AtDate.Month == fdate.Value.Month
                                             && y.AtDate.Year == fdate.Value.Year
                                             select new
                                             {
                                                 x.Value
                                             }).FirstOrDefault(),

                                  compute = db.Computeinfos.Where(x => x.Payhead == aa.PayHeadId).ToList()
                              }).AsEnumerable().Select(o => new
                              {
                                  o.PayHeadId,
                                  PayHead = o.Name,
                                  Rate = o.Rate,
                                  days = o.days,
                                  Per = o.CalculationPeriod,
                                  //Computed = Enum.GetName(typeof(ComputPayHead), o.Compute),
                                  //Basis = Enum.GetName(typeof(CalcBasisPayHead), o.CalculationBasis),
                                  HeadType = o.Type,
                                  CalType = o.CalculationType,
                                  Computed = o.Compute,
                                  Basis = o.CalculationBasis,
                                  EmpId = o.EmployeeId,
                                  EmpName = o.FirstName + " " + o.LastName,
                                  o.Leave,
                                  o.AtType,
                                  o.AtUnit,
                                  o.AtValue,
                                  o.CalculationPeriod,
                                  o.compute
                              }).ToList(),
                }).ToList();
                return Json(v);
            }
            else
            {
                var first = (from a in db.Employees
                             where listemp.Contains(a.EmployeeId)
                             select new
                             {
                                 EMPId = a.EmployeeId,
                                 EMPName = a.FirstName + " " + a.LastName,
                             }).ToList();


                SalaryStructureDetailPR vmodel = new SalaryStructureDetailPR();
                List<SalaryStructureDetailWithEmp> sal = new List<SalaryStructureDetailWithEmp>();
                SalaryStructureDetailWithEmp SLstr = new SalaryStructureDetailWithEmp();


                foreach (var arry in first)
                {
                    SLstr.EMPId = arry.EMPId;
                    SLstr.EMPName = arry.EMPName;

                    var salval = (from a in db.SalaryStrDetails
                                  join b in db.Payheads on a.PayHeadId equals b.ID into phead
                                  from b in phead.DefaultIfEmpty()
                                  join e in db.SalaryStructures on a.SalaryStrId equals e.SalaryStrId
                                  join f in db.Employees on e.EmployeeId equals f.EmployeeId into empl
                                  from f in empl.DefaultIfEmpty()
                                  where e.EmployeeId == arry.EMPId && b.Name != "Gratuity" && b.Name != "Earnings In Annual Leave"
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
                                      o.CalculationPeriod,
                                      o.AttendanceType,
                                      o.compute
                                  }).ToList();

                    List<SalaryStructureDetail> SStr = new List<SalaryStructureDetail>();

                    //earning sum
                    decimal earnSum = salval.Where(a => a.PayHeadId == 0).Select(a => a.Rate).ToList().Sum() ?? 0;

                    //deduction sum
                    decimal deductSum = salval.Where(a => a.PayHeadId == 1).Select(a => a.Rate).ToList().Sum() ?? 0;

                    foreach (var arr in salval)
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


                        if (arr.HeadType == "EarningsforEmployees")
                        {
                            Str.type = "Dr";
                            Str.rateprice = GetPayrollRate(arr.CalType, arr.CalculationPeriod, arr.Basis, arr.Rate, arr.Computed, arr.EmpId, arr.PayHeadId, days, attCount, overtime, earnSum, deductSum);

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


                        SStr.Add(Str);
                        SLstr.SalStr = SStr;
                    }
                }
                sal.Add(SLstr);
                vmodel.SLEmp = sal;

                return Json(vmodel);
            }




            //var sal = (from a in db.SalaryStrDetails
            //           join b in db.Payheads on a.PayHeadId equals b.ID into phead
            //           from b in phead.DefaultIfEmpty()
            //           join e in db.SalaryStructures on a.SalaryStrId equals e.SalaryStrId
            //           join f in db.Employees on e.EmployeeId equals f.EmployeeId into empl
            //           from f in empl.DefaultIfEmpty()
            //           where listemp.Contains(e.EmployeeId)
            //           select new
            //           {
            //               a.PayHeadId,
            //               b.Name,
            //               a.Rate,
            //               b.CalculationPeriod,
            //               b.Type,
            //               b.CalculationType,
            //               b.Compute,
            //               f.EmployeeId,
            //               f.FirstName,
            //               f.LastName,
            //           }).AsEnumerable().Select(o => new
            //           {
            //               o.PayHeadId,
            //               PayHead = o.Name,
            //               o.Rate,
            //               Per = o.CalculationPeriod,
            //               HeadType = Enum.GetName(typeof(ModeOfPayment), o.Type),
            //               CalType = Enum.GetName(typeof(ModeOfPayment), o.CalculationType),
            //               Computed = Enum.GetName(typeof(ModeOfPayment), o.Compute),
            //               EmpId = o.EmployeeId,
            //               EmpName = o.FirstName + " " + o.LastName
            //           }).ToList();

            //return Json(sal);
        }

        public decimal? GetPayrollRate(string CalType, string CalculationPeriod, string Basis, decimal? Rate, string Computed, long EmpId, long PayHeadId, int days, int attCount, decimal? overtime ,decimal? earnSum, decimal? deductSum)
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
                retrate = ComputeValue(Computed, EmpId, PayHeadId, earnSum, deductSum);
            }
            return retrate;
        }

        public decimal ComputeValue(string Computed, long EmpId, long PayHeadId, decimal? earnSum, decimal? deductSum)
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


        private string InvoiceNo(Int64 PNo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "PayrollVoucher").Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == "PayrollVoucher").Select(a => a.number).FirstOrDefault();

            if (billNo == null)
            {
                if ((db.PayrollVouchers.Select(p => p.PRNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    PNo = db.PayrollVouchers.Max(p => p.PRNo + 1);
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
            var Exists = db.PayrollVouchers.Any(c => c.VoucherNo == VcNo);
            bool res = (Exists) ? true : false;
            return res;
        }
        private long GetPRNo()
        {
            Int64 AtNo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "PayrollVoucher").Select(a => a.number).FirstOrDefault();
            if ((db.PayrollVouchers.Select(p => p.PRNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                AtNo = (number == 0) ? 1 : number;
            }
            else
            {
                AtNo = db.PayrollVouchers.Max(p => p.PRNo + 1);
            }

            return AtNo;
        }

    }
}