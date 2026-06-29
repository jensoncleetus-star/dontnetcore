using QuickSoft.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class EmployeeController : BaseController
    {

        private ApplicationUserManager _userManager;

        ApplicationDbContext db;
        Common com;
        public EmployeeController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // BOS: Human Resources dashboard. Defensive — each metric isolated in try/catch -> 0/empty on any error.
        // Gated on the "HR Dashboard" menu role too, so menu-visibility == page-access (no 403 on a visible link).
        [QkAuthorize(Roles = "Dev,Employee List,HR Dashboard")]
        public ActionResult Dashboard()
        {
            var vm = new QuickSoft.ViewModel.HrDashboardViewModel();
            var today = DateTime.Now.Date;
            var soon = today.AddDays(30);
            int month = DateTime.Now.Month;
            int year = DateTime.Now.Year;

            try { vm.TotalEmployees = db.Employees.Count(); } catch { }
            try { vm.ActiveEmployees = db.Employees.Count(e => e.Status == 1); } catch { }
            vm.InactiveEmployees = vm.TotalEmployees - vm.ActiveEmployees;
            if (vm.InactiveEmployees < 0) vm.InactiveEmployees = 0;
            try { vm.Departments = db.Departments.Count(); } catch { }
            try { vm.OnLeave = db.Employees.Count(e => e.DutyResumeDate != null && e.DutyResumeDate > today); } catch { }
            try { vm.DocsExpiring = db.EmployeeDocuments.Count(d => d.ExpiryDate != null && d.ExpiryDate >= today && d.ExpiryDate <= soon); } catch { }
            try { vm.BirthdaysThisMonth = db.EmployeePersonals.Count(p => p.DOB != null && p.DOB.Value.Month == month); } catch { }
            try { vm.JoinersThisMonth = db.Employees.Count(e => e.JoinDate != null && e.JoinDate.Value.Month == month && e.JoinDate.Value.Year == year); } catch { }

            // Headcount by department: EF-safe groupby -> scalar count, then join names in memory.
            try
            {
                var counts = db.Employees.Where(e => e.DepartmentID != null && e.Status == 1)
                                .GroupBy(e => e.DepartmentID)
                                .Select(g => new { DeptId = g.Key, Count = g.Count() })
                                .ToList();
                var names = db.Departments.ToDictionary(d => (long?)d.DepartmentID, d => d.DepartmentName);
                vm.DeptHeadcount = counts
                    .Select(x => new QuickSoft.ViewModel.HrNameCount
                    {
                        Name = (x.DeptId != null && names.ContainsKey(x.DeptId) && !string.IsNullOrWhiteSpace(names[x.DeptId])) ? names[x.DeptId] : "Unassigned",
                        Count = x.Count
                    })
                    .OrderByDescending(x => x.Count).Take(10).ToList();
            }
            catch { }

            // Documents (visa/passport/labour-card) expiring within 30 days.
            try
            {
                var docs = db.EmployeeDocuments
                    .Where(d => d.ExpiryDate != null && d.ExpiryDate >= today && d.ExpiryDate <= soon)
                    .OrderBy(d => d.ExpiryDate)
                    .Select(d => new { d.EmployeeId, d.DocumentName, d.DocumentNo, d.ExpiryDate })
                    .Take(12).ToList();
                var ids = docs.Select(d => d.EmployeeId).Distinct().ToList();
                var emps = db.Employees.Where(e => ids.Contains(e.EmployeeId))
                             .Select(e => new { e.EmployeeId, e.FirstName, e.LastName }).ToList()
                             .ToDictionary(e => e.EmployeeId, e => ((((e.FirstName ?? "") + " " + (e.LastName ?? "")).Trim())));
                vm.ExpiringDocs = docs.Select(d => new QuickSoft.ViewModel.HrDocExpiry
                {
                    EmployeeId = d.EmployeeId,
                    Employee = emps.ContainsKey(d.EmployeeId) ? emps[d.EmployeeId] : "—",
                    DocumentName = d.DocumentName,
                    DocumentNo = d.DocumentNo,
                    ExpiryDate = d.ExpiryDate,
                    DaysLeft = d.ExpiryDate.HasValue ? (int)(d.ExpiryDate.Value.Date - today).TotalDays : 0
                }).ToList();
            }
            catch { }

            // Birthdays this month.
            try
            {
                var bdays = db.EmployeePersonals.Where(p => p.DOB != null && p.DOB.Value.Month == month)
                    .Select(p => new { p.EmployeeId, p.DOB }).ToList();
                var ids = bdays.Select(b => b.EmployeeId).Distinct().ToList();
                var emps = db.Employees.Where(e => ids.Contains(e.EmployeeId))
                             .Select(e => new { e.EmployeeId, e.FirstName, e.LastName }).ToList()
                             .ToDictionary(e => e.EmployeeId, e => ((((e.FirstName ?? "") + " " + (e.LastName ?? "")).Trim())));
                vm.Birthdays = bdays.Where(b => b.DOB.HasValue).OrderBy(b => b.DOB.Value.Day)
                    .Select(b => new QuickSoft.ViewModel.HrBirthday
                    {
                        EmployeeId = b.EmployeeId,
                        Employee = emps.ContainsKey(b.EmployeeId) ? emps[b.EmployeeId] : "—",
                        DOB = b.DOB,
                        When = b.DOB.Value.ToString("dd MMM")
                    }).Take(12).ToList();
            }
            catch { }

            return View(vm);
        }

        [HttpPost]
        public JsonResult setdocument(long docid, long empid)
        {

            var doc = db.FileDocuments.Find(docid);

            var attachmts = db.MultipleDocuments.Where(o => o.RelationID == docid);

            foreach (var arr in attachmts)
            {

                string storePath = LegacyWeb.MapPath("~/uploads/empdocuments/" + empid);
                if (!Directory.Exists(storePath))
                    Directory.CreateDirectory(storePath);

                // files upload

                var fileNames = Path.Combine(LegacyWeb.MapPath("~/uploads/FileDocuments/"), arr.Document);
                var uploadUrl = LegacyWeb.MapPath("~/uploads/empdocuments/" + empid + "/" + arr.Document);
                if (System.IO.File.Exists(uploadUrl))
                    System.IO.File.Delete(uploadUrl);
                System.IO.File.Copy(fileNames, uploadUrl);
                EmployeeDocument empss = new EmployeeDocument();

                empss.EmployeeId = empid;
                empss.DocumentName = doc.DocumentName;



                empss.ExpiryDate = doc.ExpiryDate;

                empss.Note = doc.Note;
                empss.Attachments = arr.Document;
                empss.Attachments = arr.Document;
                db.EmployeeDocuments.Add(empss);

            }

            bool stat = true;
            string msg = "success";
            db.SaveChanges();
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };


        }
        public bool appstatus()
        {
            var u = User.Identity.GetUserId();
            try
            {
                var emp = db.Employees.Where(o => o.UserId == u).FirstOrDefault();
                if (emp != null)
                {
                    if (emp.appaccessonly == true)
                        return true;
                    else
                        return false;
                }
                else
                { return false; }
            }
            catch (Exception e)
            {
                return false;
            }
        }
        [HttpGet]
        public ActionResult setopenclose(long id)
        {
            var Stat = QkSelect.List(
                new List<SelectListItem> {

                new SelectListItem { Value="0",Text="Close"}, new SelectListItem { Value="1",Text="Open"},}, "Value", "Text");
            ViewBag.openclose = Stat;
            ViewBag.protask = id;
            return PartialView();
        }
        [HttpPost]
        public ActionResult approvecompain(long approve, long empid, string reason)
        {
            var apemp = db.Employees.Find(empid);
            var orempid = db.Employees.Where(o => o.EmployeeId == (long)apemp.profileupdate).FirstOrDefault();

            if (approve == 1)
            {



                orempid.ImgFileName = apemp.ImgFileName;

                // add data to employee table

                orempid.FirstName = apemp.FirstName;
                orempid.MiddleName = apemp.MiddleName;
                orempid.LastName = apemp.LastName;

                orempid.Gender = apemp.Gender;
                orempid.PassportNo = apemp.PassportNo;


                if (apemp.JoinDate != null)
                {
                    orempid.JoinDate = apemp.JoinDate;
                }
                orempid.PAddress = apemp.PAddress;
                orempid.CAddress = apemp.CAddress;


                apemp.profileupdateaccept = 2;
                db.Entry(apemp).State = EntityState.Modified;
                db.SaveChanges();

                db.Entry(orempid).State = EntityState.Modified;
                db.SaveChanges();
                //employee personal
                var eper = db.EmployeePersonals.Where(a => a.EmployeeId == orempid.EmployeeId).FirstOrDefault();
                if (eper != null)
                {
                    EmployeePersonal empper = db.EmployeePersonals.Where(o => o.EmployeeId == apemp.EmployeeId).FirstOrDefault();
                    if (empper != null)
                    {
                        eper.DOB = empper.DOB;

                        db.Entry(eper).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }




            }
            else
            {
                apemp.profileupdateaccept = 3;
                apemp.profileupdatenote = reason;
                db.Entry(apemp).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index", "Employee");
        }
        [HttpPost]
        public ActionResult setopencloseupdate(int OpenClose, long protaskid)
        {
            var pr = db.Employees.Find(protaskid);
            pr.Status = OpenClose;
            db.Entry(pr).State = EntityState.Modified;
            db.SaveChanges();
            bool stat = true;
            string msg = "Successfully Updated Status.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        // GET: Employee
        [QkAuthorize(Roles = "Dev,Employee List")]
        public ActionResult Index()
        {
            return View();
        }
        [QkAuthorize(Roles = "Dev,Employee List")]
        public ActionResult Indexforapproval()
        {
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Employee List")]
        public ActionResult GetData(long? opcls)
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
            var UserId = User.Identity.GetUserId();
            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit Employee");
            var uDelete = User.IsInRole("Delete Employee");

            var v = (from a in db.Employees
                     join b in db.Contacts on a.CAddress equals b.ContactID into cont1
                     from b in cont1.DefaultIfEmpty()
                     join c in db.Contacts on a.PAddress equals c.ContactID into cont2
                     from c in cont2.DefaultIfEmpty()
                     join d in db.Users on a.UserId equals d.Id into usr
                     from d in usr.DefaultIfEmpty()
                     join e in db.Branchs on a.BranchID equals e.BranchID into bra
                     from e in bra.DefaultIfEmpty()
                     where (opcls == null || a.Status == opcls) &&
                     (a.profileupdate == 0 || a.profileupdate == null)
                     select new
                     {
                         id = a.EmployeeId,
                         currentUser = UserId,
                         a.UserId,
                         Name = a.Prefix + " " + a.FirstName + " " + a.LastName,
                         a.EMPCode,
                         Address = b.Address + "<br/>" + b.City + " " + b.State + " " + b.Country + "<br/>" + b.Zip,
                         Phone = c.Phone,
                         Email = c.EmailId,
                         a.UserStatus,
                         Branch = e.BranchName,
                         Status = (int?)d.Status,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete

                     });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.Name.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.EMPCode.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Address.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Phone.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Email.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.UserStatus.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Branch.ToString().ToLower().Contains(search.ToLower()));
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
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Employee List")]
        public ActionResult GetDataprovileupdate(long? opcls)
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
            var UserId = User.Identity.GetUserId();
            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit Employee");
            var uDelete = User.IsInRole("Delete Employee");

            var v = (from a in db.Employees
                     join b in db.Contacts on a.CAddress equals b.ContactID into cont1
                     from b in cont1.DefaultIfEmpty()
                     join c in db.Contacts on a.PAddress equals c.ContactID into cont2
                     from c in cont2.DefaultIfEmpty()
                     join d in db.Users on a.UserId equals d.Id into usr
                     from d in usr.DefaultIfEmpty()
                     join e in db.Branchs on a.BranchID equals e.BranchID into bra
                     from e in bra.DefaultIfEmpty()
                     where
                     a.profileupdateaccept == 1
                     select new
                     {
                         id = a.EmployeeId,
                         currentUser = UserId,
                         a.UserId,
                         Name = a.Prefix + " " + a.FirstName + " " + a.LastName,
                         a.EMPCode,
                         Address = b.Address + "<br/>" + b.City + " " + b.State + " " + b.Country + "<br/>" + b.Zip,
                         Phone = c.Phone,
                         Email = c.EmailId,
                         a.UserStatus,
                         Branch = e.BranchName,
                         Status = (int?)d.Status,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete

                     });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.Name.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.EMPCode.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Address.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Phone.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Email.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.UserStatus.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Branch.ToString().ToLower().Contains(search.ToLower()));
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
        [QkAuthorize(Roles = "Dev,Create Employee")]
        public async Task<ActionResult> Create(long? id, string type)
        {
            //    .Select(s => new
            //               BranchID = s.BranchID,
            //               BranchDetails = s.BranchCode + " - " + s.BranchName

            var Deps = db.Departments.Select(s => new
            {
                DepartmentID = s.DepartmentID,
                DepartmentName = s.DepartmentName
            }).ToList();
            var Desgns = db.Designations.Select(s => new
            {
                DesignationID = s.DesignationID,
                DesignationName = s.DesignationName
            }).ToList();
            var Asnuser = db.Employees.Select(s => new
            {
                EmployeeID = s.EmployeeId,
                EmployeeName = s.FirstName + " " + s.LastName
            }).ToList();
            ViewBag.Department = QkSelect.List(Deps, "DepartmentID", "DepartmentName");
            ViewBag.Designation = QkSelect.List(Desgns, "DesignationID", "DesignationName");
            ViewBag.Assignuser = QkSelect.List(Asnuser, "EmployeeID", "EmployeeName");
            EmployeeViewModel empmodel = new EmployeeViewModel
            {
                EMPCode = getEmpCode(),
                dept = db.Departments.ToList(),
                degn = db.Designations.ToList(),
                isemployee = true,
            };
            empmodel.EmpSummarys = (from b in db.AttendanceTypes
                                    select new EmployeeAttendanceSummarysViewModel
                                    {
                                        Id = b.Id,
                                        Name = b.Name
                                    }).ToList();
            var UserId = User.Identity.GetUserId();
            var empl = (from b in db.Users
                        join c in db.Employees on b.Id equals c.UserId into user
                        from c in user.DefaultIfEmpty()
                        where (b.Id != c.UserId) && b.Id != UserId
                        select new
                        {
                            Id = b.Id,
                            Name = b.UserName,
                        }).ToList();
            ViewBag.Asnuser = QkSelect.List(empl, "Id", "Name");
            if (id != null && type == "Copy")
            {
                Employee Employee = db.Employees.Find(id);
                if (Employee != null)
                {
                    empmodel.DepartmentID = Employee.DepartmentID;
                    empmodel.DesignationID = Employee.DesignationID;
                    empmodel.UserStatus = true;
                    if (Employee.UserId != "")
                    {
                        IList<string> data = await UserManager.GetRolesAsync(Employee.UserId);
                        ViewBag.selectedRoles = data;
                    }
                }

            }

            MenuViewModel vmodel = new MenuViewModel();
            vmodel.Menu = db.AppModuless.OrderBy(a => a.MenuOrder).ToList();
            ViewBag.UserRole = vmodel;

            //check no of users
            var currentuser = db.Users.Count();
            var details = db.SystemConfigs.SingleOrDefault();
            var givenuser = Security.Decrypt(details.NumberOfUsers, General.keyval);
            if (currentuser == Convert.ToInt32(givenuser))
            {
                ViewBag.NoOfUsers = false;
            }
            else
            {
                ViewBag.NoOfUsers = true;
            }

            var DiscPerc = db.EnableSettings.Where(a => a.EnableType == "DiscountPercentage").FirstOrDefault();
            var Discper = DiscPerc != null ? (DiscPerc.Status == Status.active ? 0 : 1) : 1;
            ViewBag.Discount = Discper;

            var EnablePayroll = db.EnableSettings.Where(a => a.EnableType == "EnablePayroll").FirstOrDefault();
            var EnablePayrolls = EnablePayroll != null ? EnablePayroll.Status : Status.inactive;
            ViewBag.EnablePayroll = EnablePayrolls;

            var wkshft = db.WorkShifts.Select(s => new
            {
                Id = s.WorkShiftId,
                Name = s.WorkShiftName
            }).ToList();
            ViewBag.WShift = QkSelect.List(wkshft, "Id", "Name");

            var empgrd = db.EmployeeGrades.Select(s => new
            {
                Id = s.EmployeeGradeId,
                Name = s.GradeName
            }).ToList();
            ViewBag.EmpGrade = QkSelect.List(empgrd, "Id", "Name");

            var sacc = db.Accountss.Where(a => a.Group != 12 && a.Group != 14 && a.Group != 8).Select(a => new
            {
                ID = a.AccountsID,
                Name = a.Name
            }).ToList();
            ViewBag.LoanAcc = QkSelect.List(sacc, "Id", "Name");

            var Acc = db.Accountss.Where(a => a.AccountsID != 4 && a.Group != 23).Select(a => new
            {
                ID = a.AccountsID,
                Name = a.Name
            }).ToList();
            ViewBag.Acc = QkSelect.List(Acc, "Id", "Name");

            if (db.companys.Select(x => x.Payrolldate).FirstOrDefault() != null)
                empmodel.Payrollstratdate = db.companys.Select(x => x.Payrolldate).FirstOrDefault().Value.ToString("dd-MM-yyyy");
            else
                empmodel.Payrollstratdate = "01-01-1900";

            return View(empmodel);
        }


        public ActionResult updateemployee(long? id, string type)
        {
            //    .Select(s => new
            //               BranchID = s.BranchID,
            //               BranchDetails = s.BranchCode + " - " + s.BranchName

            var Deps = db.Departments.Select(s => new
            {
                DepartmentID = s.DepartmentID,
                DepartmentName = s.DepartmentName
            }).ToList();
            var Desgns = db.Designations.Select(s => new
            {
                DesignationID = s.DesignationID,
                DesignationName = s.DesignationName
            }).ToList();
            var Asnuser = db.Employees.Select(s => new
            {
                EmployeeID = s.EmployeeId,
                EmployeeName = s.FirstName + " " + s.LastName
            }).ToList();
            ViewBag.Department = QkSelect.List(Deps, "DepartmentID", "DepartmentName");
            ViewBag.Designation = QkSelect.List(Desgns, "DesignationID", "DesignationName");
            ViewBag.Assignuser = QkSelect.List(Asnuser, "EmployeeID", "EmployeeName");
            EmployeeViewModel empmodel = new EmployeeViewModel
            {
                EMPCode = getEmpCode(),
                dept = db.Departments.ToList(),
                degn = db.Designations.ToList(),
            };
            empmodel.EmpSummarys = (from b in db.AttendanceTypes
                                    select new EmployeeAttendanceSummarysViewModel
                                    {
                                        Id = b.Id,
                                        Name = b.Name
                                    }).ToList();
            var UserId = User.Identity.GetUserId();
            var empl = (from b in db.Users
                        join c in db.Employees on b.Id equals c.UserId into user
                        from c in user.DefaultIfEmpty()
                        where (b.Id != c.UserId) && b.Id != UserId
                        select new
                        {
                            Id = b.Id,
                            Name = b.UserName,
                        }).ToList();
            ViewBag.Asnuser = QkSelect.List(empl, "Id", "Name");
            if (id != null && type == "Copy")
            {
                Employee Employee = db.Employees.Find(id);
                if (Employee != null)
                {
                    empmodel.DepartmentID = Employee.DepartmentID;
                    empmodel.DesignationID = Employee.DesignationID;
                    empmodel.UserStatus = true;

                }

            }

            MenuViewModel vmodel = new MenuViewModel();
            vmodel.Menu = db.AppModuless.OrderBy(a => a.MenuOrder).ToList();
            ViewBag.UserRole = vmodel;

            //check no of users
            var currentuser = db.Users.Count();
            var details = db.SystemConfigs.SingleOrDefault();
            var givenuser = Security.Decrypt(details.NumberOfUsers, General.keyval);
            if (currentuser == Convert.ToInt32(givenuser))
            {
                ViewBag.NoOfUsers = false;
            }
            else
            {
                ViewBag.NoOfUsers = true;
            }

            var DiscPerc = db.EnableSettings.Where(a => a.EnableType == "DiscountPercentage").FirstOrDefault();
            var Discper = DiscPerc != null ? (DiscPerc.Status == Status.active ? 0 : 1) : 1;
            ViewBag.Discount = Discper;

            var EnablePayroll = db.EnableSettings.Where(a => a.EnableType == "EnablePayroll").FirstOrDefault();
            var EnablePayrolls = EnablePayroll != null ? EnablePayroll.Status : Status.inactive;
            ViewBag.EnablePayroll = EnablePayrolls;

            var wkshft = db.WorkShifts.Select(s => new
            {
                Id = s.WorkShiftId,
                Name = s.WorkShiftName
            }).ToList();
            ViewBag.WShift = QkSelect.List(wkshft, "Id", "Name");

            var empgrd = db.EmployeeGrades.Select(s => new
            {
                Id = s.EmployeeGradeId,
                Name = s.GradeName
            }).ToList();
            ViewBag.EmpGrade = QkSelect.List(empgrd, "Id", "Name");

            var sacc = db.Accountss.Where(a => a.Group != 12 && a.Group != 14 && a.Group != 8).Select(a => new
            {
                ID = a.AccountsID,
                Name = a.Name
            }).ToList();
            ViewBag.LoanAcc = QkSelect.List(sacc, "Id", "Name");

            var Acc = db.Accountss.Where(a => a.AccountsID != 4 && a.Group != 23).Select(a => new
            {
                ID = a.AccountsID,
                Name = a.Name
            }).ToList();
            ViewBag.Acc = QkSelect.List(Acc, "Id", "Name");

            if (db.companys.Select(x => x.Payrolldate).FirstOrDefault() != null)
                empmodel.Payrollstratdate = db.companys.Select(x => x.Payrolldate).FirstOrDefault().Value.ToString("dd-MM-yyyy");
            else
                empmodel.Payrollstratdate = "01-01-1900";

            return View(empmodel);
        }
        [HttpPost]
        // [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Create Employee")]
        public async Task<ActionResult> Create(EmployeeViewModel model)
        {
            var custExists = db.Employees.Any(u => u.EMPCode == model.EMPCode);
            var name = model.Prefix + " " + model.FirstName + " " + model.MiddleName + " " + model.LastName;
            var NameExists = db.Contacts.Any(u => u.Name == name);
            var EmailExists = db.Contacts.Any(u => u.EmailId == model.Email);
            if (1 == 2)
            {
                Danger("An Employee with same Name exists.", true);
                return RedirectToAction("Create", "Employee");
            }
            if (1 == 1)
            {
                if (!ModelState.IsValid)
                {
                    var modelErrors = new List<string>();
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var modelError in modelState.Errors)
                        {
                            modelErrors.Add(modelError.ErrorMessage);
                        }
                    }
                }
                if (ModelState.IsValid)
                {
                    bool checkUpdate = true;
                    String UserId = "";
                    var branchId = db.Branchs.Select(a => a.BranchID).FirstOrDefault();
                    if (model.Assingeduser == null && model.UserName != null)
                    {
                        var currentuser = db.Users.Count();
                        var details = db.SystemConfigs.SingleOrDefault();
                        var givenuser = Security.Decrypt(details.NumberOfUsers, General.keyval);
                        if (currentuser == Convert.ToInt32(givenuser))
                        {
                            return View("~/Views/Shared/Unauthorized.cshtml");
                        }
                        else
                        {
                            var Exists = db.Users.Any(c => c.Email == model.Email);
                            if (1 == 2)
                            {
                                Warning("Email already exists in user.", true);
                                checkUpdate = false;
                            }
                            else
                            {
                                if(model.Password.Length<6)
                                {
                                    Danger("Passwrod Must be 6 charector", true);
                                    return RedirectToAction("Create", "Employee");
                                }
                                if (model.UserName.Length < 5)
                                {
                                    Danger("username Must be 5 charector", true);
                                    return RedirectToAction("Create", "Employee");
                                }
                                var user = new ApplicationUser
                                {
                                    PhoneNumber = model.PhoneNumber,
                                    Email = model.UserName + model.PhoneNumber + "@gmail.com",
                                    Status = 1,
                                    BranchID = branchId,
                                    UserName = model.UserName,
                                    BranchAccess = BranchAccess.Current,
                                    Discount = model.Discount,
                                    Name = model.FirstName,
                                };
                                var result = await UserManager.CreateAsync(user, model.Password);
                                if (result.Succeeded)
                                {
                                    UserId = user.Id;
                                    if (model.Role != null)
                                    {
                                        await this.UserManager.AddToRolesAsync(UserId, model.Role.ToArray().Distinct().ToArray());
                                    }

                                }
                                else
                                {
                                    checkUpdate = false;
                                    AddErrors(result);

                                    ViewBag.error = result.Errors;
                                    ViewBag.data = result;
                                }
                            }
                        }
                    }
                    if (checkUpdate)
                    {
                        long PcontactId = 0;
                        long CcontactId = 0;
                        if (model.Address != null || model.City != null || model.State != null || model.Country != null || model.PostalCode != null || model.PhoneNumber != null || model.Email != null)
                        {
                            var Pcontact = new Contact
                            {
                                Name = name,
                                Address = model.Address,
                                City = model.City,
                                State = model.State,
                                Country = model.Country,
                                Zip = model.PostalCode,
                                Phone = model.PhoneNumber,
                                EmailId = model.Email,
                                Group = 4,
                                Status = Status.active
                            };
                            db.Contacts.Add(Pcontact);
                            db.SaveChanges();
                            PcontactId = Pcontact.ContactID;
                            CcontactId = PcontactId;
                            if (model.mobmodel != null)
                            {
                                foreach (var arr in model.mobmodel)
                                {
                                    if (arr.Num != null)
                                    {
                                        var mob = new Mobile
                                        {
                                            Contact = PcontactId,
                                            MobileNum = arr.Num,
                                            Name = arr.Name
                                        };
                                        db.Mobiles.Add(mob);
                                        db.SaveChanges();
                                    }
                                }
                            }
                            if (!model.ChkAddress)
                            {
                                // current address
                                var Ccontact = new Contact
                                {
                                    Name = name,
                                    Address = model.CAddress,
                                    City = model.CCity,
                                    State = model.CState,
                                    Country = model.CCountry,
                                    Zip = model.CPostalCode,
                                    Group = 4,
                                };
                                db.Contacts.Add(Ccontact);
                                db.SaveChanges();
                                CcontactId = Ccontact.ContactID;

                                foreach (var arr in model.mobmodel)
                                {
                                    if (arr.Num != null)
                                    {
                                        var mob = new Mobile
                                        {
                                            Contact = CcontactId,
                                            MobileNum = arr.Num,
                                            Name = arr.Name
                                        };
                                        db.Mobiles.Add(mob);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                        var fileName = "";
                        if (model.ImgFileName != null)
                        {
                            // files upload
                            IFormFile file = Request.Form.Files[0];
                            fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                            var uploadUrl = LegacyWeb.MapPath("~/uploads/profile/");
                            file.SaveAs(Path.Combine(uploadUrl, fileName));
                            model.ImgFileName = fileName;
                        }
                        DateTime? edob = null;
                        if (model.DOB != null)
                        {
                            edob = DateTime.Parse(model.DOB.ToString(), new CultureInfo("en-GB"));
                        }
                        DateTime? jdob = null;
                        if (model.JoinDate != null)
                        {
                            jdob = DateTime.Parse(model.JoinDate.ToString(), new CultureInfo("en-GB"));
                        }
                        DateTime? LDRD = null;
                        if (model.LastDutyResumeDate != null)
                        {
                            LDRD = DateTime.Parse(model.LastDutyResumeDate.ToString(), new CultureInfo("en-GB"));
                        }
                        // add data to employee table
                        if (model.PPhoneNumber != null || model.PEmail != null)
                        {
                            var Pcontact = new Contact
                            {
                                Name = name,
                                Address = model.Address,
                                City = model.City,
                                State = model.State,
                                Country = model.Country,
                                Zip = model.PostalCode,
                                Phone = model.PPhoneNumber,
                                EmailId = model.PEmail,
                                Group = 4,
                                Status = Status.active
                            };
                            db.Contacts.Add(Pcontact);
                            db.SaveChanges();

                            CcontactId = Pcontact.ContactID;
                        }
                        var Emp = new Employee
                        {
                            EMPCode = model.EMPCode,
                            Prefix = model.Prefix,
                            FirstName = model.FirstName,
                            MiddleName = model.MiddleName,
                            LastName = model.LastName,
                            Alias = model.Alias,
                            Gender = model.Gender,
                            PassportNo = model.PassportNo,
                            OtherIdNo = model.OtherIdNo,
                            ImgFileName = fileName != null ? fileName : "default.jpg",
                            DesignationID = model.DesignationID,
                            DepartmentID = model.DepartmentID,
                            PAddress = PcontactId,
                            CAddress = CcontactId,

                            BranchID = branchId,
                            UserStatus = true,

                            UserId = (model.Assingeduser != null) ? model.Assingeduser : UserId,
                            Status = 1,
                            CreatedDate = System.DateTime.Now,
                            Note = model.Note,
                            JoinDate = jdob,
                            //DutyResumeDate= drd,
                            JobStatus = JobStatus.Working,
                            LastDutyResumeDate = LDRD,
                            NoDaysWorked = model.NoDaysWorked,
                            TotalWorkingDays = model.TotalWorkingDays,
                            perhour = model.perhour,
                            isemployee=true,
                        };

                        db.Employees.Add(Emp);
                        db.SaveChanges();

                        //employee personal
                        EmployeePersonal empper = new EmployeePersonal();
                        empper.EmployeeId = Emp.EmployeeId;
                        empper.DOB = edob;
                        empper.MaritalStatus = model.MaritalStatus;
                        empper.BloodGroup = model.BloodGroup;
                        empper.Nationality = model.CCountry;
                        db.EmployeePersonals.Add(empper);
                        db.SaveChanges();

                        //employee bank
                        EmployeeBank empbank = new EmployeeBank();
                        empbank.EmployeeId = Emp.EmployeeId;
                        empbank.BankName = model.BankName;
                        empbank.AccountNo = model.AccountNo;
                        empbank.IbanNo = model.IbanNo;
                        empbank.BranchName = model.BranchName;
                        empbank.Swift = model.Swift;
                        db.EmployeeBanks.Add(empbank);
                        db.SaveChanges();

                        //employee work
                        EmployeeWorkDetail empworks = new EmployeeWorkDetail();
                        empworks.EmployeeId = Emp.EmployeeId;
                        empworks.SalaryMode = model.SalaryMode;
                        empworks.WorkShift = model.WorkShift;
                        empworks.BonusApplicable = model.BonusApplicable;
                        empworks.OTApplicable = model.OTApplicable;
                        empworks.SpecifyLoanAccount = model.SpecifyLoanAccount;
                        empworks.LoanAccount = model.LoanAccount;
                        empworks.SpecifyAdvanceAccount = model.SpecifyAdvanceAccount;
                        empworks.AdvanceAccount = model.AdvanceAccount;
                        empworks.EmployeeGrade = model.EmployeeGrade;
                        empworks.EmployeeAccount = model.EmployeeAccount;
                        empworks.AnnualLeaveApplicable = model.AnnualLeaveApplicable;
                        db.EmployeeWorkDetails.Add(empworks);
                        db.SaveChanges();

                        //educationaldetails
                        if (model.edumodel != null)
                        {
                            foreach (var arr in model.edumodel)
                            {
                                var emp = new EmployeeEducation
                                {
                                    EmployeeId = Emp.EmployeeId,
                                    CourseTitle = arr.Course,
                                    Specialization = arr.Specialization,
                                    Institute = arr.Institute,
                                    BoardOrUniversity = arr.University,
                                    PassYear = arr.PassingYear,
                                    Percentage = arr.Percentage
                                };
                                db.EmployeeEducations.Add(emp);
                                db.SaveChanges();

                            }
                        }

                        //professional
                        if (model.promodel != null)
                        {
                            foreach (var arr in model.promodel)
                            {
                                EmployeeProfession emps = new EmployeeProfession();
                                emps.EmployeeId = Emp.EmployeeId;
                                emps.Organization = arr.Organization;
                                emps.Designation = arr.Designation;
                                if (arr.FromDate != null)
                                {
                                    emps.FromDate = DateTime.Parse(arr.FromDate, new CultureInfo("en-GB"));
                                }
                                if (arr.ToDate != null)
                                {
                                    emps.ToDate = DateTime.Parse(arr.ToDate, new CultureInfo("en-GB"));
                                }
                                emps.Responsibility = arr.Responsibility;
                                emps.Skills = arr.Skills;
                                db.EmployeeProfessions.Add(emps);
                                db.SaveChanges();

                            }
                        }
                        //document
                        if (model.docmodel != null)
                        {
                            foreach (var arr in model.docmodel)
                            {
                                EmployeeDocument empss = new EmployeeDocument();

                                empss.EmployeeId = Emp.EmployeeId;
                                empss.DocumentName = arr.DocumentName;
                                empss.DocumentNo = arr.DocumentNo;
                                empss.PersonalNo = arr.PersonalNo;
                                if (arr.IssueDate != null)
                                {
                                    empss.IssueDate = DateTime.Parse(arr.IssueDate, new CultureInfo("en-GB"));
                                }
                                if (arr.ExpiryDate != null)
                                {
                                    empss.ExpiryDate = DateTime.Parse(arr.ExpiryDate, new CultureInfo("en-GB"));
                                }
                                empss.Note = arr.Note;


                                if (arr.Attachments != null)
                                {
                                    string storePath = LegacyWeb.MapPath("~/uploads/empdocuments/" + Emp.EmployeeId);
                                    if (!Directory.Exists(storePath))
                                        Directory.CreateDirectory(storePath);

                                    // files upload
                                    IFormFile file = Request.Form.Files[1];
                                    var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                                    var uploadUrl = LegacyWeb.MapPath("~/uploads/empdocuments/" + Emp.EmployeeId + "/");
                                    file.SaveAs(Path.Combine(uploadUrl, fileNames));
                                    empss.Attachments = fileNames;
                                }


                                db.EmployeeDocuments.Add(empss);
                                db.SaveChanges();

                            }
                        }
                        //Attendance Details
                        if (model.EmpSummarys != null)
                        {
                            foreach (EmployeeAttendanceSummarysViewModel Ad in model.EmpSummarys)
                            {
                                var Att = new EmployeeAttendanceSummary
                                {
                                    EmployeeId = Emp.EmployeeId,
                                    AttendanceType = Ad.Id,
                                    Days = Ad.Days,

                                };
                                db.EmployeeAttendanceSummarys.Add(Att);
                                db.SaveChanges();
                            }
                        }
                        com.addlog(LogTypes.Created, UserId, "Employee", "Employees", findip(), Emp.EmployeeId, "Employee Added Successfully");
                        Success("Employee details added successfully.", true);

                    }
                    return RedirectToAction("Index", "Employee");
                }
                //    .Select(s => new
                //        BranchID = s.BranchID,
                //        BranchDetails = s.BranchCode + " - " + s.BranchName
                var Deps = db.Departments.Select(s => new
                {
                    DepartmentID = s.DepartmentID,
                    DepartmentName = s.DepartmentName
                }).ToList();
                var Desgns = db.Designations.Select(s => new
                {
                    DesignationID = s.DesignationID,
                    DesignationName = s.DesignationName
                }).ToList();

                /// ViewBag.UserRole = QkSelect.List(db.Roles.ToList(), "Id", "Name");
                ViewBag.Department = QkSelect.List(Deps, "DepartmentID", "DepartmentName");
                ViewBag.Designation = QkSelect.List(Desgns, "DesignationID", "DesignationName");

                ViewBag.UserRole = db.AppModuless.ToList();

                model.dept = db.Departments.ToList();
                model.degn = db.Designations.ToList();
                return View(model);
            }
        }

        [HttpPost]

        public async Task<ActionResult> Editprofile(EmployeeViewModel model)
        {

            var usid = User.Identity.GetUserId();
            ViewBag.laststatus = "";
            long id = db.Employees.Where(o => o.UserId == usid).Select(o => o.EmployeeId).FirstOrDefault();

            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var es = db.Employees.Where(o => o.profileupdate == id);
            db.Employees.RemoveRange(es);
            db.SaveChanges();
            Employee emp = db.Employees.Find(id);


            if (1 == 1)
            {
                var CodeExists = db.Employees.Any(u => u.EMPCode == model.EMPCode && u.EmployeeId != id);
                if (1 == 2)
                {
                    Danger("An Employee with same Employee code exists.", true);
                    return RedirectToAction("Edit", "Employee");
                }
                else
                {
                    bool checkUpdate = true;
                    String UserId = "";
                    var branchId = db.Branchs.Select(a => a.BranchID).FirstOrDefault();
                    Employee Employee = db.Employees.Find(id);

                    if (checkUpdate)
                    {
                        long PcontactId = 0;
                        long CcontactId = 0;



                        if (model.ImgFileName != null)
                        {
                            // files upload
                            IFormFile file = Request.Form.Files[0];
                            var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                            var uploadUrl = LegacyWeb.MapPath("~/uploads/profile/");
                            file.SaveAs(Path.Combine(uploadUrl, fileName));
                            Employee.ImgFileName = fileName;
                        }
                        else
                        {
                            model.ImgFileName = "default.jpg";
                        }
                        // add data to employee table

                        Employee.FirstName = model.FirstName;
                        Employee.MiddleName = model.MiddleName;
                        Employee.LastName = model.LastName;

                        Employee.Gender = model.Gender;
                        Employee.PassportNo = model.PassportNo;
                        Employee.UserStatus = true;
                        if (model.Address != null || model.City != null || model.State != null || model.Country != null || model.PostalCode != null || model.PhoneNumber != null || model.Email != null)
                        {
                            var name = model.Prefix + " " + model.FirstName + " " + model.MiddleName + " " + model.LastName;
                            var Pcontact = new Contact
                            {
                                Name = name,
                                Address = model.Address,
                                City = model.City,
                                State = model.State,
                                Country = model.Country,
                                Zip = model.PostalCode,
                                Phone = model.PhoneNumber,
                                EmailId = model.Email,
                                Group = 4,
                                Status = Status.active
                            };
                            db.Contacts.Add(Pcontact);
                            db.SaveChanges();
                            PcontactId = Pcontact.ContactID;
                            CcontactId = PcontactId;
                            var mbp = db.Mobiles.Where(o => o.Contact == Employee.PAddress);
                            if (mbp != null)
                            {
                                foreach (var arr in mbp.ToList())
                                {
                                    if (arr.MobileNum != null)
                                    {
                                        var mob = new Mobile
                                        {
                                            Contact = CcontactId,
                                            MobileNum = arr.MobileNum,
                                            Name = arr.Name
                                        };
                                        db.Mobiles.Add(mob);
                                        db.SaveChanges();
                                    }
                                }
                            }
                            if (!model.ChkAddress)
                            {
                                // current address
                                var Ccontact = new Contact
                                {
                                    Name = name,
                                    EmailId = model.PEmail,
                                    Phone = model.PhoneNumber,


                                    Address = model.CAddress,
                                    City = model.CCity,
                                    State = model.CState,
                                    Country = model.CCountry,
                                    Zip = model.CPostalCode,
                                    Group = 4
                                };
                                db.Contacts.Add(Ccontact);
                                db.SaveChanges();
                                CcontactId = Ccontact.ContactID;

                                var mbcp = db.Mobiles.Where(o => o.Contact == Employee.CAddress);
                                if (mbcp != null)
                                {
                                    foreach (var arr in mbcp.ToList())
                                    {
                                        if (arr.MobileNum != null)
                                        {
                                            var mob = new Mobile
                                            {
                                                Contact = CcontactId,
                                                MobileNum = arr.MobileNum,
                                                Name = arr.Name
                                            };
                                            db.Mobiles.Add(mob);
                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }
                        //        Name = name,
                        //        Address = model.Address,
                        //        City = model.City,
                        //        State = model.State,
                        //        Country = model.Country,
                        //        Zip = model.PostalCode,
                        //        Phone = model.PPhoneNumber,
                        //        EmailId = model.PEmail,
                        //        Group = 4,
                        //        Status = Status.active


                        //                    Contact = CcontactId,
                        //                    MobileNum = arr.MobileNum,
                        //                    Name = arr.Name
                        Employee.CAddress = CcontactId;
                        Employee.PAddress = PcontactId;

                        if (model.JoinDate != null)
                        {
                            Employee.JoinDate = DateTime.Parse(model.JoinDate, new CultureInfo("en-GB"));
                        }
                        else
                        {
                            Employee.JoinDate = null;
                        }
                        Employee.profileupdate = (int)id;
                        Employee.profileupdateaccept = 1;

                        db.Employees.Add(Employee);
                        db.SaveChanges();
                        id = Employee.EmployeeId;
                        DateTime? edob = null;
                        if (model.DOB != null)
                        {
                            edob = DateTime.Parse(model.DOB, new CultureInfo("en-GB"));
                        }
                        //employee personal
                        var eper = db.EmployeePersonals.Where(a => a.EmployeeId == id).FirstOrDefault();
                        if (eper != null)
                        {
                            EmployeePersonal empper = db.EmployeePersonals.Find(eper.EmployeePersonalId);
                            empper.EmployeeId = Employee.EmployeeId;
                            empper.DOB = edob;
                            empper.MaritalStatus = model.MaritalStatus;
                            empper.BloodGroup = model.BloodGroup;
                            empper.Nationality = model.CCountry;
                            db.Entry(empper).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            //employee personal
                            EmployeePersonal empper = new EmployeePersonal();
                            empper.EmployeeId = Employee.EmployeeId;
                            empper.DOB = edob;
                            empper.MaritalStatus = model.MaritalStatus;
                            empper.BloodGroup = model.BloodGroup;
                            empper.Nationality = model.CCountry;
                            db.EmployeePersonals.Add(empper);
                            db.SaveChanges();
                        }





                        Success("Employee details Updated successfully.", true);
                        com.addlog(LogTypes.Updated, UserId, "Employee", "Employees", findip(), Employee.EmployeeId, "Employee Updated Successfully");
                        return RedirectToAction("Index", "Home");

                    }

                }
            }
            //    .Select(s => new
            //        BranchID = s.BranchID,
            //        BranchDetails = s.BranchCode + " - " + s.BranchName
            var Deps = db.Departments.Select(s => new
            {
                DepartmentID = s.DepartmentID,
                DepartmentName = s.DepartmentName
            }).ToList();
            var Desgns = db.Designations.Select(s => new
            {
                DesignationID = s.DesignationID,
                DesignationName = s.DesignationName
            }).ToList();

            var wkshft = db.WorkShifts.Select(s => new
            {
                Id = s.WorkShiftId,
                Name = s.WorkShiftName
            }).ToList();
            ViewBag.WShift = QkSelect.List(wkshft, "Id", "Name");
            ViewBag.Department = QkSelect.List(Deps, "DepartmentID", "DepartmentName");
            ViewBag.Designation = QkSelect.List(Desgns, "DesignationID", "DesignationName");

            var empgrd = db.EmployeeGrades.Select(s => new
            {
                Id = s.EmployeeGradeId,
                Name = s.GradeName
            }).ToList();
            ViewBag.EmpGrade = QkSelect.List(empgrd, "Id", "Name");
            var sacc = db.Accountss.Where(a => a.Group != 12 && a.Group != 14 && a.Group != 8).Select(a => new
            {
                ID = a.AccountsID,
                Name = a.Name
            }).ToList();
            ViewBag.LoanAcc = QkSelect.List(sacc, "Id", "Name");

            model.dept = db.Departments.ToList();
            model.degn = db.Designations.ToList();
            ViewBag.UserRole = db.AppModuless.ToList();
            MenuViewModel vmodel = new MenuViewModel();
            vmodel.Menu = db.AppModuless.OrderBy(a => a.MenuOrder).ToList();
            ViewBag.UserRole = vmodel;
            return View(model);

        }
        public async Task<ActionResult> Editprofile()
        {
            var usid = User.Identity.GetUserId();
            long id = db.Employees.Where(o => o.UserId == usid).Select(o => o.EmployeeId).FirstOrDefault();
            var empap = db.Employees.Where(o => o.profileupdate == id).FirstOrDefault();
            ViewBag.laststatus = "";
            if (empap != null)
            {
                if (empap.profileupdateaccept == 2)
                {
                    ViewBag.laststatus = "<span style='color:green;font-weight:bold'>Approved</span>";
                }
                else if (empap.profileupdateaccept == 3)
                {
                    ViewBag.laststatus = "<span style='color:red;font-weight:bold'>Rejectd </span>Reason : " + empap.profileupdatenote;
                }
                else
                {
                    ViewBag.laststatus = "<span style='color:orange;font-weight:bold'>Pending </span>";

                }
            }
            if (id == null || id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee Employee = db.Employees.Find(id);
            if (Employee == null)
            {
                return NotFound();
            }


            EmployeeViewModel EMP = new EmployeeViewModel();

            EMP.id = Employee.EmployeeId;
            EMP.EMPCode = Employee.EMPCode;
            EMP.Prefix = Employee.Prefix;
            EMP.FirstName = Employee.FirstName;
            EMP.MiddleName = Employee.MiddleName;
            EMP.LastName = Employee.LastName;
            EMP.Gender = Employee.Gender;
            EMP.PassportNo = Employee.PassportNo;
            EMP.OtherIdNo = Employee.OtherIdNo;
            EMP.DepartmentID = Employee.DepartmentID;
            EMP.DesignationID = Employee.DesignationID;
            EMP.ImgFileName = Employee.ImgFileName;
            //BranchID = (long)Employee.BranchID,
            EMP.UserStatus = true;
            EMP.Note = Employee.Note;
            EMP.UserId = User.Identity.GetUserId();
            EMP.currentUser = Employee.UserId;
            EMP.Alias = Employee.Alias;
            EMP.perhour = Employee.perhour;
            EMP.appaccessonly = Employee.appaccessonly;
            EMP.isemployee = Employee.isemployee;
            if (Employee.JoinDate != null)
            {
                EMP.JoinDate = Employee.JoinDate.Value.ToString("dd-MM-yyyy");
            }
            if (Employee.LeavingDate != null)
            {
                EMP.LeavingDate = Employee.LeavingDate.Value.ToString("dd-MM-yyyy");
            }
            if (Employee.DutyResumeDate != null)
            {
                EMP.DutyResumeDate = Employee.DutyResumeDate.Value.ToString("dd-MM-yyyy");
            }
            EMP.ReasonForLeaving = Employee.ReasonForLeaving;


            EMP.dept = db.Departments.ToList();
            EMP.degn = db.Designations.ToList();


            var eper = db.EmployeePersonals.Where(a => a.EmployeeId == id).FirstOrDefault();
            if (eper != null)
            {
                if (eper.DOB != null)
                {
                    EMP.DOB = eper.DOB.Value.ToString("dd-MM-yyyy");
                }
                EMP.MaritalStatus = eper.MaritalStatus;
                EMP.BloodGroup = eper.BloodGroup;
            }
            var ebank = db.EmployeeBanks.Where(a => a.EmployeeId == id).FirstOrDefault();
            if (ebank != null)
            {
                EMP.BankName = ebank.BankName;
                EMP.AccountNo = ebank.AccountNo;
                EMP.IbanNo = ebank.IbanNo;
                EMP.BranchName = ebank.BranchName;
                EMP.Swift = ebank.Swift;
            }
            //employee work
            var ework = db.EmployeeWorkDetails.Where(a => a.EmployeeId == id).FirstOrDefault();
            if (ework != null)
            {
                EMP.SalaryMode = ework.SalaryMode;
                EMP.WorkShift = ework.WorkShift;
                EMP.BonusApplicable = ework.BonusApplicable;
                EMP.OTApplicable = ework.OTApplicable;
                EMP.SpecifyLoanAccount = ework.SpecifyLoanAccount;
                EMP.LoanAccount = ework.LoanAccount;
                EMP.SpecifyAdvanceAccount = ework.SpecifyAdvanceAccount;
                EMP.AdvanceAccount = ework.AdvanceAccount;
                EMP.EmployeeGrade = ework.EmployeeGrade;
                EMP.EmployeeAccount = ework.EmployeeAccount;
                EMP.AnnualLeaveApplicable = ework.AnnualLeaveApplicable;
            }


            if (Employee.UserId != "")
            {
                var exists = db.Users.Where(o => o.Id == Employee.UserId).FirstOrDefault();
                if (exists != null)
                {
                    IList<string> data = await UserManager.GetRolesAsync(Employee.UserId);
                    ViewBag.selectedRoles = data;
                }
            }
            MenuViewModel vmodel = new MenuViewModel();
            vmodel.Menu = db.AppModuless.OrderBy(a => a.MenuOrder).ToList();
            ViewBag.UserRole = vmodel;

            var user = await UserManager.FindByIdAsync(Employee.UserId);
            if (user == null)
                Employee.UserStatus = true;

            if (Employee.UserStatus)
            {
                if (user != null)
                {
                    EMP.UserName = user.UserName;
                    EMP.PhoneNumber = user.PhoneNumber;
                    EMP.Discount = user.Discount;
                }
                //    Text = x.Name,
                //    Value = x.Name
            }
            else
            {
                if (user != null)
                {
                    EMP.PhoneNumber = user.PhoneNumber;
                }
                //    Selected = false,
                //    Text = x.Name,
                //    Value = x.Name

            }
            Contact Pcnt = db.Contacts.Find(Employee.PAddress);
            if (Pcnt != null)
            {
                EMP.Address = Pcnt.Address;
                EMP.City = Pcnt.City;
                EMP.State = Pcnt.State;
                EMP.Country = Pcnt.Country;
                EMP.PostalCode = Pcnt.Zip;
                EMP.PhoneNumber = Pcnt.Phone;
                EMP.Email = Pcnt.EmailId;

            }
            if (Employee.CAddress == Employee.PAddress)
            {
                EMP.ChkAddress = true;
            }
            else
            {
                EMP.ChkAddress = false;
                Contact Ccnt = db.Contacts.Find(Employee.CAddress);

                if (Ccnt != null)
                {
                    EMP.CAddress = Ccnt.Address;
                    EMP.CCity = Ccnt.City;
                    EMP.CCountry = Ccnt.Country;
                    EMP.CState = Ccnt.State;
                    EMP.CPostalCode = Ccnt.Zip;
                    if (Ccnt.EmailId != null)
                        EMP.PEmail = Ccnt.EmailId;
                    if (Ccnt.Phone != null)
                        EMP.PPhoneNumber = Ccnt.Phone;

                }
            }
            //    Select(s => new
            //    BranchID = s.BranchID,
            //    BranchDetails = s.BranchCode + " - " + s.BranchName
            var Deps = db.Departments.Select(s => new
            {
                DepartmentID = s.DepartmentID,
                DepartmentName = s.DepartmentName
            }).ToList();
            var Desgns = db.Designations.Select(s => new
            {
                DesignationID = s.DesignationID,
                DesignationName = s.DesignationName
            }).ToList();

            ViewBag.Department = QkSelect.List(Deps, "DepartmentID", "DepartmentName");
            ViewBag.Designation = QkSelect.List(Desgns, "DesignationID", "DesignationName");


            //check isuser is employee
            ViewBag.Status = Employee.UserStatus;
            var UserId = User.Identity.GetUserId();
            var userExists = db.Employees.Where(u => u.UserId == UserId).FirstOrDefault();
            if (userExists == null)
            {
                ViewBag.Hidden = null;
            }
            else
            {
                ViewBag.Hidden = UserId;
            }

            var empl = (from b in db.Users

                        select new
                        {
                            Id = b.Id,
                            Name = b.UserName,
                        }).ToList();
            ViewBag.Asnuser = QkSelect.List(empl, "Id", "Name");

            var DiscPerc = db.EnableSettings.Where(a => a.EnableType == "DiscountPercentage").FirstOrDefault();
            var Discper = DiscPerc != null ? (DiscPerc.Status == Status.active ? 0 : 1) : 1;
            ViewBag.Discount = Discper;

            var EnablePayroll = db.EnableSettings.Where(a => a.EnableType == "EnablePayroll").FirstOrDefault();
            var EnablePayrolls = EnablePayroll != null ? EnablePayroll.Status : Status.inactive;
            ViewBag.EnablePayroll = EnablePayrolls;

            var wkshft = db.WorkShifts.Select(s => new
            {
                Id = s.WorkShiftId,
                Name = s.WorkShiftName
            }).ToList();
            ViewBag.WShift = QkSelect.List(wkshft, "Id", "Name");

            var empgrd = db.EmployeeGrades.Select(s => new
            {
                Id = s.EmployeeGradeId,
                Name = s.GradeName
            }).ToList();
            ViewBag.EmpGrade = QkSelect.List(empgrd, "Id", "Name");

            var sacc = db.Accountss.Where(a => a.Group != 12 && a.Group != 14 && a.Group != 8).Select(a => new
            {
                ID = a.AccountsID,
                Name = a.Name
            }).ToList();
            ViewBag.LoanAcc = QkSelect.List(sacc, "Id", "Name");

            var Acc = db.Accountss.Where(a => a.AccountsID != 4 && a.Group != 23).Select(a => new
            {
                ID = a.AccountsID,
                Name = a.Name
            }).ToList();
            ViewBag.Acc = QkSelect.List(Acc, "Id", "Name");

            var LS = db.LeaveSettlements.Where(a => a.Employee == id).FirstOrDefault();
            ViewBag.ChkLS = (LS != null) ? true : false;
            EMP.EmpSummarys = (from b in db.AttendanceTypes
                               select new EmployeeAttendanceSummarysViewModel
                               {
                                   Id = b.Id,
                                   Name = b.Name
                               }).ToList();
            EMP.EmpSummaryData = (from b in db.EmployeeAttendanceSummarys
                                  join c in db.AttendanceTypes on b.AttendanceType equals c.Id into use
                                  from c in use.DefaultIfEmpty()
                                  where b.EmployeeId == id
                                  select new EmployeeAttendanceSummarysViewModel
                                  {
                                      Id = b.AttendanceType,
                                      EmployeeId = b.EmployeeId,
                                      Days = b.Days,
                                      Name = c.Name
                                  }).ToList();

            if (Employee.LastDutyResumeDate != null)
            {
                EMP.LastDutyResumeDate = Employee.LastDutyResumeDate.Value.ToString("dd-MM-yyyy");
            }
            EMP.TotalWorkingDays = Employee.TotalWorkingDays;
            EMP.NoDaysWorked = Employee.NoDaysWorked;

            var Payrolldate = db.companys.Select(x => x.Payrolldate).FirstOrDefault();
            if (Payrolldate != null)
            {
                EMP.Payrollstratdate = Payrolldate.Value.ToString("dd-MM-yyyy");
            }
            return View(EMP);
        }
        [QkAuthorize(Roles = "Dev,Edit Employee")]
        public async Task<ActionResult> Edit(long? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee Employee = db.Employees.Find(id);
            if (Employee == null)
            {
                return NotFound();
            }


            EmployeeViewModel EMP = new EmployeeViewModel();

            EMP.id = Employee.EmployeeId;
            EMP.EMPCode = Employee.EMPCode;
            EMP.Prefix = Employee.Prefix;
            EMP.FirstName = Employee.FirstName;
            EMP.MiddleName = Employee.MiddleName;
            EMP.LastName = Employee.LastName;
            EMP.Gender = Employee.Gender;
            EMP.PassportNo = Employee.PassportNo;
            EMP.OtherIdNo = Employee.OtherIdNo;
            EMP.DepartmentID = Employee.DepartmentID;
            EMP.DesignationID = Employee.DesignationID;
            EMP.ImgFileName = Employee.ImgFileName;
            //BranchID = (long)Employee.BranchID,
            EMP.UserStatus = true;
            EMP.Note = Employee.Note;
            EMP.UserId = User.Identity.GetUserId();
            EMP.currentUser = Employee.UserId;
            EMP.Alias = Employee.Alias;
            EMP.perhour = Employee.perhour;
            EMP.appaccessonly = Employee.appaccessonly;
            EMP.isemployee = Employee.isemployee;
            if (Employee.JoinDate != null)
            {
                EMP.JoinDate = Employee.JoinDate.Value.ToString("dd-MM-yyyy");
            }
            if (Employee.LeavingDate != null)
            {
                EMP.LeavingDate = Employee.LeavingDate.Value.ToString("dd-MM-yyyy");
            }
            if (Employee.DutyResumeDate != null)
            {
                EMP.DutyResumeDate = Employee.DutyResumeDate.Value.ToString("dd-MM-yyyy");
            }
            EMP.ReasonForLeaving = Employee.ReasonForLeaving;


            EMP.dept = db.Departments.ToList();
            EMP.degn = db.Designations.ToList();


            var eper = db.EmployeePersonals.Where(a => a.EmployeeId == id).FirstOrDefault();
            if (eper != null)
            {
                if (eper.DOB != null)
                {
                    EMP.DOB = eper.DOB.Value.ToString("dd-MM-yyyy");
                }
                EMP.MaritalStatus = eper.MaritalStatus;
                EMP.BloodGroup = eper.BloodGroup;
            }
            var ebank = db.EmployeeBanks.Where(a => a.EmployeeId == id).FirstOrDefault();
            if (ebank != null)
            {
                EMP.BankName = ebank.BankName;
                EMP.AccountNo = ebank.AccountNo;
                EMP.IbanNo = ebank.IbanNo;
                EMP.BranchName = ebank.BranchName;
                EMP.Swift = ebank.Swift;
            }
            //employee work
            var ework = db.EmployeeWorkDetails.Where(a => a.EmployeeId == id).FirstOrDefault();
            if (ework != null)
            {
                EMP.SalaryMode = ework.SalaryMode;
                EMP.WorkShift = ework.WorkShift;
                EMP.BonusApplicable = ework.BonusApplicable;
                EMP.OTApplicable = ework.OTApplicable;
                EMP.SpecifyLoanAccount = ework.SpecifyLoanAccount;
                EMP.LoanAccount = ework.LoanAccount;
                EMP.SpecifyAdvanceAccount = ework.SpecifyAdvanceAccount;
                EMP.AdvanceAccount = ework.AdvanceAccount;
                EMP.EmployeeGrade = ework.EmployeeGrade;
                EMP.EmployeeAccount = ework.EmployeeAccount;
                EMP.AnnualLeaveApplicable = ework.AnnualLeaveApplicable;
            }


            if (Employee.UserId != "")
            {
                var exists = db.Users.Where(o => o.Id == Employee.UserId).FirstOrDefault();
                if (exists != null)
                {
                    IList<string> data = await UserManager.GetRolesAsync(Employee.UserId);
                    ViewBag.selectedRoles = data;
                }
            }
            MenuViewModel vmodel = new MenuViewModel();
            vmodel.Menu = db.AppModuless.OrderBy(a => a.MenuOrder).ToList();
            ViewBag.UserRole = vmodel;

            var user = await UserManager.FindByIdAsync(Employee.UserId);
            if (user == null)
                Employee.UserStatus = true;

            if (Employee.UserStatus)
            {
                if (user != null)
                {
                    EMP.UserName = user.UserName;
                    EMP.PhoneNumber = user.PhoneNumber;
                    EMP.Discount = user.Discount;
                }
                //    Text = x.Name,
                //    Value = x.Name
            }
            else
            {
                if (user != null)
                {
                    EMP.PhoneNumber = user.PhoneNumber;
                }
                //    Selected = false,
                //    Text = x.Name,
                //    Value = x.Name

            }
            Contact Pcnt = db.Contacts.Find(Employee.PAddress);
            if (Pcnt != null)
            {
                EMP.Address = Pcnt.Address;
                EMP.City = Pcnt.City;
                EMP.State = Pcnt.State;
                EMP.Country = Pcnt.Country;
                EMP.PostalCode = Pcnt.Zip;
                EMP.PhoneNumber = Pcnt.Phone;
                EMP.Email = Pcnt.EmailId;

            }
            if (Employee.CAddress == Employee.PAddress)
            {
                EMP.ChkAddress = true;
            }
            else
            {
                EMP.ChkAddress = false;
                Contact Ccnt = db.Contacts.Find(Employee.CAddress);

                if (Ccnt != null)
                {
                    EMP.CAddress = Ccnt.Address;
                    EMP.CCity = Ccnt.City;
                    EMP.CCountry = Ccnt.Country;
                    EMP.CState = Ccnt.State;
                    EMP.CPostalCode = Ccnt.Zip;
                    if (Ccnt.EmailId != null)
                        EMP.PEmail = Ccnt.EmailId;
                    if (Ccnt.Phone != null)
                        EMP.PPhoneNumber = Ccnt.Phone;

                }
            }
            //    Select(s => new
            //    BranchID = s.BranchID,
            //    BranchDetails = s.BranchCode + " - " + s.BranchName
            var Deps = db.Departments.Select(s => new
            {
                DepartmentID = s.DepartmentID,
                DepartmentName = s.DepartmentName
            }).ToList();
            var Desgns = db.Designations.Select(s => new
            {
                DesignationID = s.DesignationID,
                DesignationName = s.DesignationName
            }).ToList();

            ViewBag.Department = QkSelect.List(Deps, "DepartmentID", "DepartmentName");
            ViewBag.Designation = QkSelect.List(Desgns, "DesignationID", "DesignationName");


            //check isuser is employee
            ViewBag.Status = Employee.UserStatus;
            var UserId = User.Identity.GetUserId();
            var userExists = db.Employees.Where(u => u.UserId == UserId).FirstOrDefault();
            if (userExists == null)
            {
                ViewBag.Hidden = null;
            }
            else
            {
                ViewBag.Hidden = UserId;
            }

            var empl = (from b in db.Users

                        select new
                        {
                            Id = b.Id,
                            Name = b.UserName,
                        }).ToList();
            ViewBag.Asnuser = QkSelect.List(empl, "Id", "Name");

            var DiscPerc = db.EnableSettings.Where(a => a.EnableType == "DiscountPercentage").FirstOrDefault();
            var Discper = DiscPerc != null ? (DiscPerc.Status == Status.active ? 0 : 1) : 1;
            ViewBag.Discount = Discper;

            var EnablePayroll = db.EnableSettings.Where(a => a.EnableType == "EnablePayroll").FirstOrDefault();
            var EnablePayrolls = EnablePayroll != null ? EnablePayroll.Status : Status.inactive;
            ViewBag.EnablePayroll = EnablePayrolls;

            var wkshft = db.WorkShifts.Select(s => new
            {
                Id = s.WorkShiftId,
                Name = s.WorkShiftName
            }).ToList();
            ViewBag.WShift = QkSelect.List(wkshft, "Id", "Name");

            var empgrd = db.EmployeeGrades.Select(s => new
            {
                Id = s.EmployeeGradeId,
                Name = s.GradeName
            }).ToList();
            ViewBag.EmpGrade = QkSelect.List(empgrd, "Id", "Name");

            var sacc = db.Accountss.Where(a => a.Group != 12 && a.Group != 14 && a.Group != 8).Select(a => new
            {
                ID = a.AccountsID,
                Name = a.Name
            }).ToList();
            ViewBag.LoanAcc = QkSelect.List(sacc, "Id", "Name");

            var Acc = db.Accountss.Where(a => a.AccountsID != 4 && a.Group != 23).Select(a => new
            {
                ID = a.AccountsID,
                Name = a.Name
            }).ToList();
            ViewBag.Acc = QkSelect.List(Acc, "Id", "Name");

            var LS = db.LeaveSettlements.Where(a => a.Employee == id).FirstOrDefault();
            ViewBag.ChkLS = (LS != null) ? true : false;
            EMP.EmpSummarys = (from b in db.AttendanceTypes
                               select new EmployeeAttendanceSummarysViewModel
                               {
                                   Id = b.Id,
                                   Name = b.Name
                               }).ToList();
            EMP.EmpSummaryData = (from b in db.EmployeeAttendanceSummarys
                                  join c in db.AttendanceTypes on b.AttendanceType equals c.Id into use
                                  from c in use.DefaultIfEmpty()
                                  where b.EmployeeId == id
                                  select new EmployeeAttendanceSummarysViewModel
                                  {
                                      Id = b.AttendanceType,
                                      EmployeeId = b.EmployeeId,
                                      Days = b.Days,
                                      Name = c.Name
                                  }).ToList();

            if (Employee.LastDutyResumeDate != null)
            {
                EMP.LastDutyResumeDate = Employee.LastDutyResumeDate.Value.ToString("dd-MM-yyyy");
            }
            EMP.TotalWorkingDays = Employee.TotalWorkingDays;
            EMP.NoDaysWorked = Employee.NoDaysWorked;

            var Payrolldate = db.companys.Select(x => x.Payrolldate).FirstOrDefault();
            if (Payrolldate != null)
            {
                EMP.Payrollstratdate = Payrolldate.Value.ToString("dd-MM-yyyy");
            }
            var taskmanner = (from c in db.ProTaskManners

                              select c)
              .Select(s => new
              {
                  ID = s.TaskTypeId,
                  Name = s.TypeName
              })
              .ToList();
            var prtasks = db.TaskAssigneds.Where(o => o.EmployeeId == id).Select(o => o.ProTaskId).ToList().ToArray();
           var TaskManner =(from a in db.AssignTaskManners
                                where prtasks.Contains(a.ProTaskId)
                                select new
                                {
                                    a.TaskMannerId
                                }).Distinct().ToList().Select(o=>o.TaskMannerId).ToArray() ?? null;

            var TaskManner2 = db.AssignTaskManners.Where(o=>o.EmployeeId==id&&o.ProTaskId==0).Select(o => o.TaskMannerId).ToArray() ?? null;

            var TaskMannerjoin = TaskManner.Concat(TaskManner2);

            ViewBag.taskmanners = new MultiSelectList(taskmanner, "ID", "Name", TaskMannerjoin.Distinct().ToArray());
            var scworks = db.LeadTypes.Select(s => new { ID = s.TypeId, Name = s.Type }).ToList();
            var scops = (from a in db.ScopeOfWorksDatas
                         join b in db.LeadTypes on a.ScopeId equals b.TypeId
                         where a.employeeid ==id
                              select new
                              {
                                 b.TypeId
                              }).Distinct().ToList().Select(o => o.TypeId).ToArray() ?? null;
            ViewBag.LeadType = new MultiSelectList(scworks, "ID", "Name", scops);
            return View(EMP);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Edit Employee")]
        public async Task<ActionResult> Edit(EmployeeViewModel model, long? id)
        {


            Employee emp = db.Employees.Find(id);
            var userchk = UserManager.FindByIdAsync(emp.UserId).Result;
            if (userchk != null)
            {
                ModelState.Remove("Password");
                ModelState.Remove("ConfirmPassword");
            }
            if (ModelState.IsValid)
            {
                var CodeExists = db.Employees.Any(u => u.EMPCode == model.EMPCode && u.EmployeeId != id);
                if (1 == 2)
                {
                    Danger("An Employee with same Employee code exists.", true);
                    return RedirectToAction("Edit", "Employee");
                }
                else
                {
                    bool checkUpdate = true;
                    String UserId = "";
                    var branchId = db.Branchs.Select(a => a.BranchID).FirstOrDefault();
                    Employee Employee = db.Employees.Find(id);
                    var assman = db.AssignTaskManners.Where(a => a.ProTaskId == 0 && a.EmployeeId==id);
                    if (assman != null)
                    {
                        db.AssignTaskManners.RemoveRange(db.AssignTaskManners.Where(a => a.ProTaskId == 0 && a.EmployeeId == id));
                        db.SaveChanges();
                    }
                    if (model.TaskManner != null)
                    {
                        AssignTaskManner tskax = new AssignTaskManner();
                        foreach (var arr in model.TaskManner)
                        {
                            tskax.ProTaskId = 0;
                            tskax.TaskMannerId = arr;
                            tskax.EmployeeId = id;
                            db.AssignTaskManners.Add(tskax);
                            db.SaveChanges();
                        }
                    }
                    var Exists = db.Users.Any(c => c.Email == model.Email && c.Id != Employee.UserId && Employee.EmployeeId != id);
                    if (1 == 2)
                    {
                        Warning("User with this Email already exists.", true);
                        checkUpdate = false;
                        return RedirectToAction("Edit", "Employee");
                    }
                    else
                    {
                        var ExistsName = db.Users.Any(c => c.UserName == model.UserName && c.Id != Employee.UserId);
                        if (ExistsName)
                        {
                            Warning("User with Similar UserName already exists.", true);
                            checkUpdate = false;
                            return RedirectToAction("Edit", "Employee");
                        }
                        else
                        {
                            if (Employee.UserStatus)
                            {
                                var userUp = UserManager.FindByIdAsync(Employee.UserId).Result;
                                if (userUp != null)
                                {
                                    userUp.Email = userUp.UserName + "@gmail.com";
                                    userUp.PhoneNumber = model.PhoneNumber;
                                    userUp.BranchID = db.Branchs.Select(a => a.BranchID).FirstOrDefault();
                                    userUp.UserName = model.UserName;
                                    userUp.BranchAccess = BranchAccess.Current;
                                    userUp.Discount = model.Discount;
                                    userUp.Name = model.FirstName;

                                    var result1 = await UserManager.UpdateAsync(userUp);
                                    if (result1.Succeeded)
                                    {
                                        if (model.Role != null)
                                        {
                                            //remove
                                            UserId = userUp.Id;
                                            var roles = await UserManager.GetRolesAsync(UserId);
                                            await UserManager.RemoveFromRolesAsync(UserId, roles.ToArray());
                                            //add 
                                            await this.UserManager.AddToRolesAsync(UserId, model.Role.ToArray().Distinct().ToArray());

                                        }
                                    }
                                    else
                                    {
                                        checkUpdate = false;
                                        AddErrors(result1);

                                        ViewBag.error = result1.Errors;
                                        ViewBag.data = result1;
                                    }
                                }
                            }
                            else
                            {
                                if (model.UserStatus && model.Assingeduser == null)
                                {
                                    var user = new ApplicationUser
                                    {
                                        PhoneNumber = model.PhoneNumber,
                                        Email = model.UserName + model.PhoneNumber + "@gmail.com",
                                        Status = 1,
                                        BranchID = db.Branchs.Select(a => a.BranchID).FirstOrDefault(),
                                        UserName = model.UserName,
                                        BranchAccess = BranchAccess.Current,
                                        Discount = model.Discount,
                                        Name = model.FirstName,
                                    };
                                    var result1 = await UserManager.CreateAsync(user, model.Password);
                                    if (result1.Succeeded)
                                    {
                                        UserId = user.Id;
                                        await this.UserManager.AddToRolesAsync(UserId, model.Role);
                                    }
                                    else
                                    {
                                        checkUpdate = false;
                                        AddErrors(result1);

                                        ViewBag.error = result1.Errors;
                                        ViewBag.data = result1;
                                    }
                                }

                            }
                        }

                    }
                    if (checkUpdate)
                    {
                        long PcontactId = 0;
                        long CcontactId = 0;
                        Contact con1 = db.Contacts.Find(Employee.PAddress);
                        Contact con2 = db.Contacts.Find(Employee.CAddress);
                        if (con1 != null)
                            db.Contacts.Remove(con1);
                        db.Mobiles.RemoveRange(db.Mobiles.Where(a => a.Contact == Employee.PAddress));
                        if (con2 != null)
                            db.Contacts.Remove(con2);
                        db.Mobiles.RemoveRange(db.Mobiles.Where(a => a.Contact == Employee.CAddress));
                        db.SaveChanges();
                        if (model.Address != null || model.City != null || model.State != null || model.Country != null || model.PostalCode != null || model.PhoneNumber != null || model.Email != null)
                        {
                            var name = model.Prefix + " " + model.FirstName + " " + model.MiddleName + " " + model.LastName;
                            var Pcontact = new Contact
                            {
                                Name = name,
                                Address = model.Address,
                                City = model.City,
                                State = model.State,
                                Country = model.Country,
                                Zip = model.PostalCode,
                                Phone = model.PhoneNumber,
                                EmailId = model.Email,
                                Group = 4,
                                Status = Status.active
                            };
                            db.Contacts.Add(Pcontact);
                            db.SaveChanges();
                            PcontactId = Pcontact.ContactID;
                            CcontactId = PcontactId;
                            if (model.mobmodel != null)
                            {
                                foreach (var arr in model.mobmodel)
                                {
                                    if (arr.Num != null)
                                    {
                                        var mob = new Mobile
                                        {
                                            Contact = CcontactId,
                                            MobileNum = arr.Num,
                                            Name = arr.Name
                                        };
                                        db.Mobiles.Add(mob);
                                        db.SaveChanges();
                                    }
                                }
                            }
                            if (!model.ChkAddress)
                            {
                                // current address
                                var Ccontact = new Contact
                                {
                                    Name = name,
                                    EmailId = model.Email,


                                    Address = model.CAddress,
                                    City = model.CCity,
                                    State = model.CState,
                                    Country = model.CCountry,
                                    Zip = model.CPostalCode,
                                    Group = 4
                                };
                                db.Contacts.Add(Ccontact);
                                db.SaveChanges();
                                CcontactId = Ccontact.ContactID;

                                foreach (var arr in model.mobmodel)
                                {
                                    if (arr.Num != null)
                                    {
                                        var mob = new Mobile
                                        {
                                            Contact = CcontactId,
                                            MobileNum = arr.Num,
                                            Name = arr.Name
                                        };
                                        db.Mobiles.Add(mob);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                        if (model.PPhoneNumber != null || model.PEmail != null)
                        {
                            var name = model.Prefix + " " + model.FirstName + " " + model.MiddleName + " " + model.LastName;
                            var Pcontact = new Contact
                            {
                                Name = name,
                                Address = model.Address,
                                City = model.City,
                                State = model.State,
                                Country = model.Country,
                                Zip = model.PostalCode,
                                Phone = model.PPhoneNumber,
                                EmailId = model.PEmail,
                                Group = 4,
                                Status = Status.active
                            };
                            db.Contacts.Add(Pcontact);
                            db.SaveChanges();

                            CcontactId = Pcontact.ContactID;
                            if (model.mobmodel != null)
                            {
                                foreach (var arr in model.mobmodel)
                                {
                                    if (arr.Num != null)
                                    {
                                        var mob = new Mobile
                                        {
                                            Contact = CcontactId,
                                            MobileNum = arr.Num,
                                            Name = arr.Name
                                        };
                                        db.Mobiles.Add(mob);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }

                        if (model.ImgFileName != null)
                        {
                            // files upload
                            IFormFile file = Request.Form.Files[0];
                            var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                            var uploadUrl = LegacyWeb.MapPath("~/uploads/profile/");
                            file.SaveAs(Path.Combine(uploadUrl, fileName));
                            Employee.ImgFileName = fileName;
                        }
                        else
                        {
                            model.ImgFileName = "default.jpg";
                        }
                        // add data to employee table
                        Employee.appaccessonly = model.appaccessonly;
                        Employee.isemployee = model.isemployee;
                        Employee.EMPCode = model.EMPCode;
                        Employee.Prefix = model.Prefix;
                        Employee.FirstName = model.FirstName;
                        Employee.MiddleName = model.MiddleName;
                        Employee.LastName = model.LastName;
                        Employee.Alias = model.Alias;
                        Employee.Gender = model.Gender;
                        Employee.PassportNo = model.PassportNo;
                        Employee.OtherIdNo = model.OtherIdNo;
                        Employee.DesignationID = model.DesignationID;
                        Employee.DepartmentID = model.DepartmentID;
                        Employee.PAddress = PcontactId;
                        Employee.CAddress = CcontactId;
                        Employee.perhour = model.perhour;
                        Employee.BranchID = branchId;
                        Employee.UserStatus = true;
                        Employee.Note = model.Note;

                        if (model.JoinDate != null)
                        {
                            Employee.JoinDate = DateTime.Parse(model.JoinDate, new CultureInfo("en-GB"));
                        }
                        else
                        {
                            Employee.JoinDate = null;
                        }
                        if (model.LeavingDate != null)
                        {
                            Employee.LeavingDate = DateTime.Parse(model.LeavingDate, new CultureInfo("en-GB"));
                        }
                        else
                        {
                            Employee.LeavingDate = null;
                        }
                        if (model.DutyResumeDate != null)
                        {
                            Employee.DutyResumeDate = DateTime.Parse(model.DutyResumeDate, new CultureInfo("en-GB"));
                        }
                        else
                        {
                            Employee.DutyResumeDate = null;
                        }

                        Employee.ReasonForLeaving = model.ReasonForLeaving;
                        
                  if (model.Assingeduser != null)
                        {
                            Employee.UserId = model.Assingeduser;
                            Employee.UserStatus = true;
                        }
                        Employee.Status = 1;

                        Employee.JobStatus = model.JobStatus;
                        if (model.LastDutyResumeDate != null)
                        {
                            Employee.LastDutyResumeDate = DateTime.Parse(model.LastDutyResumeDate, new CultureInfo("en-GB"));
                        }
                        else
                        {
                            Employee.LastDutyResumeDate = null;
                        }
                        Employee.TotalWorkingDays = model.TotalWorkingDays;
                        Employee.NoDaysWorked = model.NoDaysWorked;
                        db.Entry(Employee).State = EntityState.Modified;
                        db.SaveChanges();

                        DateTime? edob = null;
                        if (model.DOB != null)
                        {
                            edob = DateTime.Parse(model.DOB, new CultureInfo("en-GB"));
                        }
                        //employee personal
                        var eper = db.EmployeePersonals.Where(a => a.EmployeeId == id).FirstOrDefault();
                        if (eper != null)
                        {
                            EmployeePersonal empper = db.EmployeePersonals.Find(eper.EmployeePersonalId);
                            empper.EmployeeId = Employee.EmployeeId;
                            empper.DOB = edob;
                            empper.MaritalStatus = model.MaritalStatus;
                            empper.BloodGroup = model.BloodGroup;
                            empper.Nationality = model.CCountry;
                            db.Entry(empper).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            //employee personal
                            EmployeePersonal empper = new EmployeePersonal();
                            empper.EmployeeId = Employee.EmployeeId;
                            empper.DOB = edob;
                            empper.MaritalStatus = model.MaritalStatus;
                            empper.BloodGroup = model.BloodGroup;
                            empper.Nationality = model.CCountry;
                            db.EmployeePersonals.Add(empper);
                            db.SaveChanges();
                        }

                        //employee bank
                        var ebank = db.EmployeeBanks.Where(a => a.EmployeeId == id).FirstOrDefault();
                        if (ebank != null)
                        {
                            EmployeeBank empbank = db.EmployeeBanks.Find(ebank.EmployeeBankId);
                            empbank.EmployeeId = Employee.EmployeeId;
                            empbank.BankName = model.BankName;
                            empbank.AccountNo = model.AccountNo;
                            empbank.IbanNo = model.IbanNo;
                            empbank.BranchName = model.BranchName;
                            empbank.Swift = model.Swift;
                            db.Entry(empbank).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            //employee bank
                            EmployeeBank empbank = new EmployeeBank();
                            empbank.EmployeeId = Employee.EmployeeId;
                            empbank.BankName = model.BankName;
                            empbank.AccountNo = model.AccountNo;
                            empbank.IbanNo = model.IbanNo;
                            empbank.BranchName = model.BranchName;
                            empbank.Swift = model.Swift;
                            db.EmployeeBanks.Add(empbank);
                            db.SaveChanges();
                        }

                        //employee work
                        var ework = db.EmployeeWorkDetails.Where(a => a.EmployeeId == id).FirstOrDefault();
                        if (ework != null)
                        {
                            EmployeeWorkDetail empworks = db.EmployeeWorkDetails.Find(ework.EmployeeWorkDetailId);
                            empworks.EmployeeId = Employee.EmployeeId;
                            empworks.SalaryMode = model.SalaryMode;
                            empworks.WorkShift = model.WorkShift;
                            empworks.BonusApplicable = model.BonusApplicable;
                            empworks.OTApplicable = model.OTApplicable;
                            empworks.SpecifyLoanAccount = model.SpecifyLoanAccount;
                            empworks.LoanAccount = model.LoanAccount;
                            empworks.SpecifyAdvanceAccount = model.SpecifyAdvanceAccount;
                            empworks.AdvanceAccount = model.AdvanceAccount;
                            empworks.EmployeeGrade = model.EmployeeGrade;
                            empworks.EmployeeAccount = model.EmployeeAccount;
                            empworks.AnnualLeaveApplicable = model.AnnualLeaveApplicable;
                            db.Entry(empworks).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            //employee work
                            EmployeeWorkDetail empworks = new EmployeeWorkDetail();
                            empworks.EmployeeId = Employee.EmployeeId;
                            empworks.SalaryMode = model.SalaryMode;
                            empworks.WorkShift = model.WorkShift;
                            empworks.BonusApplicable = model.BonusApplicable;
                            empworks.OTApplicable = model.OTApplicable;
                            empworks.SpecifyLoanAccount = model.SpecifyLoanAccount;
                            empworks.LoanAccount = model.LoanAccount;
                            empworks.SpecifyAdvanceAccount = model.SpecifyAdvanceAccount;
                            empworks.AdvanceAccount = model.AdvanceAccount;
                            empworks.EmployeeGrade = model.EmployeeGrade;
                            empworks.EmployeeAccount = model.EmployeeAccount;
                            empworks.AnnualLeaveApplicable = model.AnnualLeaveApplicable;
                            db.EmployeeWorkDetails.Add(empworks);
                            db.SaveChanges();
                        }

                        //educationaldetails
                        var eedu = db.EmployeeEducations.Where(a => a.EmployeeId == id).FirstOrDefault();
                        if (eedu != null)
                        {
                            db.EmployeeEducations.RemoveRange(db.EmployeeEducations.Where(a => a.EmployeeId == id));
                            db.SaveChanges();
                        }
                        if (model.edumodel != null)
                        {
                            foreach (var arr in model.edumodel)
                            {
                                var emped = new EmployeeEducation
                                {
                                    EmployeeId = Employee.EmployeeId,
                                    CourseTitle = arr.Course,
                                    Specialization = arr.Specialization,
                                    Institute = arr.Institute,
                                    BoardOrUniversity = arr.University,
                                    PassYear = arr.PassingYear,
                                    Percentage = arr.Percentage
                                };
                                db.EmployeeEducations.Add(emped);
                                db.SaveChanges();

                            }
                        }

                        //professional
                        var eprof = db.EmployeeProfessions.Where(a => a.EmployeeId == id).FirstOrDefault();
                        if (eprof != null)
                        {
                            db.EmployeeProfessions.RemoveRange(db.EmployeeProfessions.Where(a => a.EmployeeId == id));
                            db.SaveChanges();
                        }
                        if (model.promodel != null)
                        {
                            foreach (var arr in model.promodel)
                            {
                                EmployeeProfession emps = new EmployeeProfession();
                                emps.EmployeeId = Employee.EmployeeId;
                                emps.Organization = arr.Organization;
                                emps.Designation = arr.Designation;
                                if (arr.FromDate != null)
                                {
                                    emps.FromDate = DateTime.Parse(arr.FromDate, new CultureInfo("en-GB"));
                                }
                                if (arr.ToDate != null)
                                {
                                    emps.ToDate = DateTime.Parse(arr.ToDate, new CultureInfo("en-GB"));
                                }
                                emps.Responsibility = arr.Responsibility;
                                emps.Skills = arr.Skills;
                                db.EmployeeProfessions.Add(emps);
                                db.SaveChanges();
                            }
                        }
                        //document

                        var empdocs = db.EmployeeDocuments.Where(o => o.EmployeeId == Employee.EmployeeId);
                        db.EmployeeDocuments.RemoveRange(empdocs);

                        if (model.docmodel != null)
                        {
                            int count = 0;
                            foreach (var arr in model.docmodel)
                            {
                                var empdoc = db.EmployeeDocuments.Where(o => o.EmployeeDocumentId == arr.EmployeeDocumentId && o.EmployeeId == Employee.EmployeeId).FirstOrDefault();


                                EmployeeDocument empss = new EmployeeDocument();

                                empss.EmployeeId = Employee.EmployeeId;
                                empss.DocumentName = arr.DocumentName;
                                empss.DocumentNo = arr.DocumentNo;
                                empss.PersonalNo = arr.PersonalNo;
                                if (arr.IssueDate != null)
                                {
                                    empss.IssueDate = DateTime.Parse(arr.IssueDate, new CultureInfo("en-GB"));
                                }
                                if (arr.ExpiryDate != null)
                                {
                                    empss.ExpiryDate = DateTime.Parse(arr.ExpiryDate, new CultureInfo("en-GB"));
                                }
                                empss.Note = arr.Note;


                                if (arr.Attachments != null)
                                {


                                    string storePath = LegacyWeb.MapPath("~/uploads/empdocuments/" + Employee.EmployeeId);
                                    if (!Directory.Exists(storePath))
                                        Directory.CreateDirectory(storePath);

                                    // files upload

                                    IFormFile file = Request.Form.Files["docmodel[" + count + "].Attachments"]; // Request.Form.Files[1];
                                    var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                                    var uploadUrl = LegacyWeb.MapPath("~/uploads/empdocuments/" + Employee.EmployeeId + "/");
                                    file.SaveAs(Path.Combine(uploadUrl, fileNames));
                                    empss.Attachments = fileNames;

                                }

                                count++;
                                if (empdoc == null)
                                {

                                    db.EmployeeDocuments.Add(empss);

                                }
                                else
                                {

                                    empdoc.DocumentNo = arr.DocumentNo;
                                    empdoc.PersonalNo = arr.PersonalNo;
                                    if (arr.IssueDate != null)
                                    {
                                        empdoc.IssueDate = DateTime.Parse(arr.IssueDate, new CultureInfo("en-GB"));
                                    }
                                    if (arr.ExpiryDate != null)
                                    {
                                        empdoc.ExpiryDate = DateTime.Parse(arr.ExpiryDate, new CultureInfo("en-GB"));
                                    }
                                    empdoc.Note = arr.Note;
                                    if (arr.Attachments != null)
                                        empdoc.Attachments = empss.Attachments;
                                    db.Entry(empdoc).State = EntityState.Modified;
                                }



                            }

                        }
                        //Attendance Details
                        db.SaveChanges();
                        if (model.EmpSummarys != null)
                        {
                            var EAS = db.EmployeeAttendanceSummarys.Where(a => a.EmployeeId == id).FirstOrDefault();
                            if (EAS != null)
                            {
                                db.EmployeeAttendanceSummarys.RemoveRange(db.EmployeeAttendanceSummarys.Where(a => a.EmployeeId == id));
                                db.SaveChanges();
                            }
                            foreach (EmployeeAttendanceSummarysViewModel Ad in model.EmpSummarys)
                            {
                                var Att = new EmployeeAttendanceSummary
                                {
                                    EmployeeId = (long)id,
                                    AttendanceType = Ad.Id,
                                    Days = Ad.Days,
                                };
                                db.EmployeeAttendanceSummarys.Add(Att);
                                db.SaveChanges();
                            }
                        }

                        Success("Employee details Updated successfully.", true);
                        com.addlog(LogTypes.Updated, UserId, "Employee", "Employees", findip(), Employee.EmployeeId, "Employee Updated Successfully");
                        return RedirectToAction("Index", "Employee");

                    }

                }
            }
            //    .Select(s => new
            //        BranchID = s.BranchID,
            //        BranchDetails = s.BranchCode + " - " + s.BranchName
            var Deps = db.Departments.Select(s => new
            {
                DepartmentID = s.DepartmentID,
                DepartmentName = s.DepartmentName
            }).ToList();
            var Desgns = db.Designations.Select(s => new
            {
                DesignationID = s.DesignationID,
                DesignationName = s.DesignationName
            }).ToList();

            var wkshft = db.WorkShifts.Select(s => new
            {
                Id = s.WorkShiftId,
                Name = s.WorkShiftName
            }).ToList();
            ViewBag.WShift = QkSelect.List(wkshft, "Id", "Name");
            ViewBag.Department = QkSelect.List(Deps, "DepartmentID", "DepartmentName");
            ViewBag.Designation = QkSelect.List(Desgns, "DesignationID", "DesignationName");

            var empgrd = db.EmployeeGrades.Select(s => new
            {
                Id = s.EmployeeGradeId,
                Name = s.GradeName
            }).ToList();
            ViewBag.EmpGrade = QkSelect.List(empgrd, "Id", "Name");
            var sacc = db.Accountss.Where(a => a.Group != 12 && a.Group != 14 && a.Group != 8).Select(a => new
            {
                ID = a.AccountsID,
                Name = a.Name
            }).ToList();
            ViewBag.LoanAcc = QkSelect.List(sacc, "Id", "Name");

            model.dept = db.Departments.ToList();
            model.degn = db.Designations.ToList();
            ViewBag.UserRole = db.AppModuless.ToList();
            MenuViewModel vmodel = new MenuViewModel();
            vmodel.Menu = db.AppModuless.OrderBy(a => a.MenuOrder).ToList();
            ViewBag.UserRole = vmodel;
            return View(model);

        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Delete Employee")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Employee emp = db.Employees.Find(id);
            if (emp == null)
            {
                return NotFound();
            }
            return PartialView(emp);
        }
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete Employee")]
        public async Task<ActionResult> DeleteConfirmed(long? id)
        {
            bool stat = false;
            string msg = "";
            bool checkIsUser = true;
            var UserId = User.Identity.GetUserId();
            Employee Emp = db.Employees.Find(id);


            //isuser
            var EmpUser = Emp.UserId;
            if (UserId == EmpUser)
            {
                var IsuserS = db.SalesEntrys.Any(c => c.CreatedBy == UserId);
                var IsuserQ = db.Quotations.Any(c => c.CreatedUserId == UserId);
                var IsuserP = db.PurchaseEntrys.Any(c => c.CreatedBy == UserId);
                var IsuserSR = db.SalesReturns.Any(c => c.CreatedBy == UserId);
                var IsuserPR = db.PurchaseReturns.Any(c => c.CreatedBy == UserId);
                var IsuserD = db.Deliverynotes.Any(c => c.CreatedUserId == UserId);
                var IsuserPF = db.ProFormas.Any(c => c.CreatedBy == UserId);

                var IsuserPro = db.Projects.Any(c => c.CreatedBy == UserId);
                var IsuserTsk = db.ProTasks.Any(c => c.CreatedBy == UserId);


                if (IsuserS || IsuserQ || IsuserP || IsuserSR || IsuserPR || IsuserD || IsuserPF || IsuserPro || IsuserTsk)
                {
                    checkIsUser = false;
                }
            }

            var ExistsS = db.SalesEntrys.Any(c => c.SECashier == id);
            var ExistsQ = db.Quotations.Any(c => c.QuotCashier == id);
            var ExistsP = db.PurchaseEntrys.Any(c => c.PECashier == id);
            var ExistsSR = db.SalesReturns.Any(c => c.SRCashier == id);
            var ExistsPR = db.PurchaseReturns.Any(c => c.PRCashier == id);
            var ExistsD = db.Deliverynotes.Any(c => c.DvCashier == id);
            var ExistsPF = db.ProFormas.Any(c => c.PFCashier == id);

            var ExistsPro = db.Projects.Any(c => c.SalesPerson == id);
            var ExistsTask = db.ProTasks.Any(c => c.SalesPerson == id);


            //
            var TeamLead = db.Teams.Any(c => c.TeamLead == Emp.EmployeeId);
            var TeamMem = db.TeamMembers.Any(c => c.EmployeeId == Emp.EmployeeId);
            var taskAsgn = db.TaskAssigneds.Any(c => c.EmployeeId == Emp.EmployeeId);
            var remAsgn = db.ReminderAssigneds.Any(c => c.EmployeeId == Emp.EmployeeId);
            var approval = db.Approvals.Any(c => c.EmployeeId == Emp.EmployeeId);


            if (checkIsUser == true && (ExistsS || ExistsQ || ExistsP || ExistsSR || ExistsPR || ExistsD || ExistsPF || ExistsPro || ExistsTask || TeamLead || TeamMem || taskAsgn || remAsgn || approval))
            {
                msg = "Unable to delete Employee, It Already Used In Entries.";
                stat = false;
            }
            else
            {
                Contact con1 = db.Contacts.Find(Emp.PAddress);
                Contact con2 = db.Contacts.Find(Emp.CAddress);

                if (con1 != null)
                {
                    db.Contacts.Remove(con1);
                }
                if (con2 != null)
                {
                    db.Contacts.Remove(con2);
                }

                var user = await UserManager.FindByIdAsync(Emp.UserId);
                if (user != null)
                {
                    var result = await UserManager.DeleteAsync(user);
                }
                db.Employees.Remove(Emp);
                db.SaveChanges();


                var ebank = db.EmployeeBanks.Where(a => a.EmployeeId == id).FirstOrDefault();
                if (ebank != null)
                {
                    db.EmployeeBanks.RemoveRange(db.EmployeeBanks.Where(a => a.EmployeeId == id));
                    db.SaveChanges();
                }

                var edoc = db.EmployeeDocuments.Where(a => a.EmployeeId == id).FirstOrDefault();
                if (edoc != null)
                {
                    db.EmployeeDocuments.RemoveRange(db.EmployeeDocuments.Where(a => a.EmployeeId == id));
                    db.SaveChanges();
                }
                var empper = db.EmployeePersonals.Where(a => a.EmployeeId == id).FirstOrDefault();
                if (empper != null)
                {
                    db.EmployeePersonals.RemoveRange(db.EmployeePersonals.Where(a => a.EmployeeId == id));
                    db.SaveChanges();
                }
                var emppro = db.EmployeeProfessions.Where(a => a.EmployeeId == id).FirstOrDefault();
                if (emppro != null)
                {
                    db.EmployeeProfessions.RemoveRange(db.EmployeeProfessions.Where(a => a.EmployeeId == id));
                    db.SaveChanges();
                }
                var empwork = db.EmployeeWorkDetails.Where(a => a.EmployeeId == id).FirstOrDefault();
                if (empwork != null)
                {
                    db.EmployeeWorkDetails.RemoveRange(db.EmployeeWorkDetails.Where(a => a.EmployeeId == id));
                    db.SaveChanges();
                }

                var empedu = db.EmployeeEducations.Where(a => a.EmployeeId == id).FirstOrDefault();
                if (empedu != null)
                {
                    db.EmployeeEducations.RemoveRange(db.EmployeeEducations.Where(a => a.EmployeeId == id));
                    db.SaveChanges();
                }

                stat = true;
                msg = "Successfully deleted Employee details.";
                com.addlog(LogTypes.Deleted, UserId, "Employee", "Employees", findip(), Emp.EmployeeId, "Employee Deleted Successfully");

            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        [HttpGet]
        [Authorize(Roles = "Dev,Profile")]
        public ActionResult Profiles()
        {
            var UserId = User.Identity.GetUserId();
            UserDetailViewModel vmodel = new UserDetailViewModel();
            vmodel = (from b in db.Users
                      join c in db.Employees on b.Id equals c.UserId into user
                      from c in user.DefaultIfEmpty()
                      join d in db.Contacts on c.PAddress equals d.ContactID into cnt
                      from d in cnt.DefaultIfEmpty()
                      where b.Id == UserId
                      select new UserDetailViewModel
                      {
                          Email = b.Email,
                          Name = b.Name,
                          PhoneNumber = b.PhoneNumber,
                          //took permananet address PAddress
                          Address = d.Address,
                          City = d.City,
                          ImgFileName = c.ImgFileName != null ? c.ImgFileName : "default.jpg",
                          State = d.State,
                          Country = d.Country,
                          PostalCode = d.Zip,
                          Branch = db.Branchs.Where(c => c.BranchID == c.BranchID).Select(c => c.BranchCode + " - " + c.BranchName).FirstOrDefault(),
                          Department = db.Departments.Where(c => c.DepartmentID == c.DepartmentID).Select(c => c.DepartmentName).FirstOrDefault(),
                          Designation = db.Designations.Where(c => c.DesignationID == c.DesignationID).Select(c => c.DesignationName).FirstOrDefault()

                      }).FirstOrDefault();

            return View(vmodel);
        }

        [RedirectingAction]
        // [Authorize(Roles = "Dev,View Employee")]
        public ActionResult detailscompair(long id)
        {
            List<EmployeeDetailViewModel> vmodel = new List<EmployeeDetailViewModel>();
            var emp1 = (from a in db.Employees
                        join b in db.Contacts on a.CAddress equals b.ContactID into cont1
                        from b in cont1.DefaultIfEmpty()
                        join c in db.Contacts on a.PAddress equals c.ContactID into cont2
                        from c in cont2.DefaultIfEmpty()
                        join d in db.Users on a.UserId equals d.Id into usr
                        from d in usr.DefaultIfEmpty()
                        join e in db.Branchs on a.BranchID equals e.BranchID

                        join p in db.EmployeePersonals on a.EmployeeId equals p.EmployeeId into empper
                        from p in empper.DefaultIfEmpty()
                        join bk in db.EmployeeBanks on a.EmployeeId equals bk.EmployeeId into ban
                        from bk in ban.DefaultIfEmpty()

                        join wk in db.EmployeeWorkDetails on a.EmployeeId equals wk.EmployeeId into wrk
                        from wk in wrk.DefaultIfEmpty()

                        join de in db.Departments on a.DepartmentID equals de.DepartmentID into dep
                        from de in dep.DefaultIfEmpty()
                        join ds in db.Designations on a.DesignationID equals ds.DesignationID into desg
                        from ds in desg.DefaultIfEmpty()
                        where a.EmployeeId == id
                        select new
                        {
                            a.EMPCode,
                            a.EmployeeId,
                            a.FirstName,
                            a.MiddleName,
                            a.LastName,
                            a.PassportNo,
                            a.OtherIdNo,
                            a.JoinDate,
                            a.LeavingDate,
                            a.ReasonForLeaving,
                            a.Note,
                            b.Phone,
                            a.Gender,
                            b.EmailId,
                            a.JobStatus,
                            pphone = b.Phone,
                            pemail = b.EmailId,

                            b.Address,
                            b.City,
                            b.State,
                            b.Country,
                            b.Zip,

                            CAddress = c.Address,
                            CCity = c.City,
                            CState = c.State,
                            CCountry = c.Country,
                            CZip = c.Zip,

                            DOB = p != null ? p.DOB : null,
                            MaritalStatus = p != null ? p.MaritalStatus : null,
                            BloodGroup = p != null ? p.BloodGroup : null,

                            Mobile = (from ac in db.Mobiles
                                      where (ac.Contact == c.ContactID)
                                      select new MobileViewModel
                                      {
                                          Num = ac.MobileNum,
                                          Name = ac.Name
                                      }).ToList(),
                            ImgFileName = a.ImgFileName,
                            de.DepartmentName,
                            ds.DesignationName,

                            BankName = bk != null ? bk.BankName : "",
                            AccountNo = bk != null ? bk.AccountNo : "",
                            IbanNo = bk != null ? bk.IbanNo : "",
                            BranchName = bk != null ? bk.BranchName : "",
                            Swift = bk != null ? bk.Swift : "",

                            SalaryMode = wk != null ? wk.SalaryMode : 0,
                            WorkShift = wk != null ? wk.WorkShift : null,
                            BonusApplicable = wk != null ? (wk.BonusApplicable == true ? "Yes" : "No") : "No",
                            OTApplicable = wk != null ? (wk.OTApplicable == true ? "Yes" : "No") : "No",
                            SpecifyLoanAccount = wk != null ? (wk.SpecifyLoanAccount == true ? "Yes" : "No") : "No",
                            SpecifyAdvanceAccount = wk != null ? (wk.SpecifyAdvanceAccount == true ? "Yes" : "No") : "No",

                            LoanAccount = wk != null ? wk.LoanAccount : null,
                            AdvanceAccount = wk != null ? wk.AdvanceAccount : null,
                            EmployeeGrade = wk != null ? wk.EmployeeGrade : null,
                            Address1 = b.Address != null ? b.Address : "" + "<br/>" +
                               b.City != null ? b.City : "" +
                               " " + b.State != null ? b.State : "" +
                               " " + b.Country != null ? b.Country : "" +
                               "<br/>" + b.Zip != null ? b.Zip : "",
                            Address2 = c.Address != null ? c.Address : "" + "<br/>" +
                               c.City != null ? c.City : "" +
                               " " + c.State != null ? c.State : "" +
                               " " + c.Country != null ? c.Country : "" +
                               "<br/>" + c.Zip != null ? c.Zip : "",
                        }).ToList().Select(o => new EmployeeDetailViewModel
                        {
                            EMPCode = o.EMPCode,
                            FirstName = o.FirstName,
                            MiddleName = o.MiddleName,
                            LastName = o.LastName,
                            PhoneNumber = o.Phone,
                            Email = o.EmailId,
                            EmployeeId = o.EmployeeId,
                            Gender = Enum.GetName(typeof(Gender), o.Gender),
                            mob = o.Mobile,
                            DOB = o.DOB,
                            PassportNo = o.PassportNo,
                            OtherIdNo = o.OtherIdNo,
                            MaritalStatus = o.MaritalStatus,
                            BloodGroup = o.BloodGroup,
                            Jobstatus = Enum.GetName(typeof(Gender), o.JobStatus),
                            PEmail = o.pemail,
                            PPhoneNumber = o.pphone,

                            JoinDate = o.JoinDate,
                            LeavingDate = o.LeavingDate,
                            ReasonForLeaving = o.ReasonForLeaving,
                            ImgFileName = o.ImgFileName != "" ? o.ImgFileName : "default.jpg",
                            Note = o.Note,
                            Department = o.DepartmentName,
                            Designation = o.DesignationName,

                            Address = o.Address,
                            City = o.City,
                            State = o.State,
                            Country = o.Country,
                            Zip = o.Zip,

                            CAddress = o.CAddress,
                            CCity = o.CCity,
                            CState = o.CState,
                            CCountry = o.CCountry,
                            CZip = o.CZip,

                            BankName = o.BankName,
                            AccountNo = o.AccountNo,
                            IbanNo = o.IbanNo,
                            BranchName = o.BranchName,
                            Swift = o.Swift,

                            SalaryMode = Enum.GetName(typeof(SalaryMode), o.SalaryMode),
                            WorkShift = o.WorkShift != null ? (db.WorkShifts.Where(a => a.WorkShiftId == o.WorkShift).Select(a => a.WorkShiftName).FirstOrDefault()) : "",
                            BonusApplicable = o.BonusApplicable,
                            OTApplicable = o.OTApplicable,
                            SpecifyLoanAccount = o.SpecifyLoanAccount,

                            LoanAccount = o.LoanAccount != null ? (db.Accountss.Where(a => a.AccountsID == o.LoanAccount).Select(a => a.Name).FirstOrDefault()) : "",
                            AdvanceAccount = o.AdvanceAccount != null ? (db.Accountss.Where(a => a.AccountsID == o.AdvanceAccount).Select(a => a.Name).FirstOrDefault()) : "",
                            EmployeeGrade = o.EmployeeGrade != null ? (db.EmployeeGrades.Where(a => a.EmployeeGradeId == o.EmployeeGrade).Select(a => a.GradeName).FirstOrDefault()) : "",
                        }).FirstOrDefault();
            var orgempid = Convert.ToInt64(db.Employees.Find(id).profileupdate);


            var emp2 = (from a in db.Employees
                        join b in db.Contacts on a.CAddress equals b.ContactID into cont1
                        from b in cont1.DefaultIfEmpty()
                        join c in db.Contacts on a.PAddress equals c.ContactID into cont2
                        from c in cont2.DefaultIfEmpty()
                        join d in db.Users on a.UserId equals d.Id into usr
                        from d in usr.DefaultIfEmpty()
                        join e in db.Branchs on a.BranchID equals e.BranchID

                        join p in db.EmployeePersonals on a.EmployeeId equals p.EmployeeId into empper
                        from p in empper.DefaultIfEmpty()
                        join bk in db.EmployeeBanks on a.EmployeeId equals bk.EmployeeId into ban
                        from bk in ban.DefaultIfEmpty()

                        join wk in db.EmployeeWorkDetails on a.EmployeeId equals wk.EmployeeId into wrk
                        from wk in wrk.DefaultIfEmpty()

                        join de in db.Departments on a.DepartmentID equals de.DepartmentID into dep
                        from de in dep.DefaultIfEmpty()
                        join ds in db.Designations on a.DesignationID equals ds.DesignationID into desg
                        from ds in desg.DefaultIfEmpty()
                        where a.EmployeeId == orgempid
                        select new
                        {
                            a.EMPCode,
                            a.EmployeeId,
                            a.FirstName,
                            a.MiddleName,
                            a.LastName,
                            a.PassportNo,
                            a.OtherIdNo,
                            a.JoinDate,
                            a.LeavingDate,
                            a.ReasonForLeaving,
                            a.Note,
                            b.Phone,
                            a.Gender,
                            b.EmailId,
                            a.JobStatus,

                            b.Address,
                            b.City,
                            b.State,
                            b.Country,
                            b.Zip,
                            pphone = b.Phone,
                            pemail = b.EmailId,
                            CAddress = c.Address,
                            CCity = c.City,
                            CState = c.State,
                            CCountry = c.Country,
                            CZip = c.Zip,

                            DOB = p != null ? p.DOB : null,
                            MaritalStatus = p != null ? p.MaritalStatus : null,
                            BloodGroup = p != null ? p.BloodGroup : null,

                            Mobile = (from ac in db.Mobiles
                                      where (ac.Contact == c.ContactID)
                                      select new MobileViewModel
                                      {
                                          Num = ac.MobileNum,
                                          Name = ac.Name
                                      }).ToList(),
                            ImgFileName = a.ImgFileName,
                            de.DepartmentName,
                            ds.DesignationName,

                            BankName = bk != null ? bk.BankName : "",
                            AccountNo = bk != null ? bk.AccountNo : "",
                            IbanNo = bk != null ? bk.IbanNo : "",
                            BranchName = bk != null ? bk.BranchName : "",
                            Swift = bk != null ? bk.Swift : "",

                            SalaryMode = wk != null ? wk.SalaryMode : 0,
                            WorkShift = wk != null ? wk.WorkShift : null,
                            BonusApplicable = wk != null ? (wk.BonusApplicable == true ? "Yes" : "No") : "No",
                            OTApplicable = wk != null ? (wk.OTApplicable == true ? "Yes" : "No") : "No",
                            SpecifyLoanAccount = wk != null ? (wk.SpecifyLoanAccount == true ? "Yes" : "No") : "No",
                            SpecifyAdvanceAccount = wk != null ? (wk.SpecifyAdvanceAccount == true ? "Yes" : "No") : "No",

                            LoanAccount = wk != null ? wk.LoanAccount : null,
                            AdvanceAccount = wk != null ? wk.AdvanceAccount : null,
                            EmployeeGrade = wk != null ? wk.EmployeeGrade : null,
                            Address1 = b.Address != null ? b.Address : "" + "<br/>" +
                               b.City != null ? b.City : "" +
                               " " + b.State != null ? b.State : "" +
                               " " + b.Country != null ? b.Country : "" +
                               "<br/>" + b.Zip != null ? b.Zip : "",
                            Address2 = c.Address != null ? c.Address : "" + "<br/>" +
                               c.City != null ? c.City : "" +
                               " " + c.State != null ? c.State : "" +
                               " " + c.Country != null ? c.Country : "" +
                               "<br/>" + c.Zip != null ? c.Zip : "",
                        }).ToList().Select(o => new EmployeeDetailViewModel
                        {
                            EMPCode = o.EMPCode,
                            FirstName = o.FirstName,
                            MiddleName = o.MiddleName,
                            LastName = o.LastName,
                            PhoneNumber = o.Phone,
                            Email = o.EmailId,
                            EmployeeId = o.EmployeeId,
                            Gender = Enum.GetName(typeof(Gender), o.Gender),
                            mob = o.Mobile,
                            DOB = o.DOB,
                            PPhoneNumber = o.pphone,
                            PEmail = o.pemail,

                            PassportNo = o.PassportNo,
                            OtherIdNo = o.OtherIdNo,
                            MaritalStatus = o.MaritalStatus,
                            BloodGroup = o.BloodGroup,
                            Jobstatus = Enum.GetName(typeof(Gender), o.JobStatus),

                            JoinDate = o.JoinDate,
                            LeavingDate = o.LeavingDate,
                            ReasonForLeaving = o.ReasonForLeaving,
                            ImgFileName = o.ImgFileName != "" ? o.ImgFileName : "default.jpg",
                            Note = o.Note,
                            Department = o.DepartmentName,
                            Designation = o.DesignationName,

                            Address = o.Address,
                            City = o.City,
                            State = o.State,
                            Country = o.Country,
                            Zip = o.Zip,

                            CAddress = o.CAddress,
                            CCity = o.CCity,
                            CState = o.CState,
                            CCountry = o.CCountry,
                            CZip = o.CZip,

                            BankName = o.BankName,
                            AccountNo = o.AccountNo,
                            IbanNo = o.IbanNo,
                            BranchName = o.BranchName,
                            Swift = o.Swift,

                            SalaryMode = Enum.GetName(typeof(SalaryMode), o.SalaryMode),
                            WorkShift = o.WorkShift != null ? (db.WorkShifts.Where(a => a.WorkShiftId == o.WorkShift).Select(a => a.WorkShiftName).FirstOrDefault()) : "",
                            BonusApplicable = o.BonusApplicable,
                            OTApplicable = o.OTApplicable,
                            SpecifyLoanAccount = o.SpecifyLoanAccount,

                            LoanAccount = o.LoanAccount != null ? (db.Accountss.Where(a => a.AccountsID == o.LoanAccount).Select(a => a.Name).FirstOrDefault()) : "",
                            AdvanceAccount = o.AdvanceAccount != null ? (db.Accountss.Where(a => a.AccountsID == o.AdvanceAccount).Select(a => a.Name).FirstOrDefault()) : "",
                            EmployeeGrade = o.EmployeeGrade != null ? (db.EmployeeGrades.Where(a => a.EmployeeGradeId == o.EmployeeGrade).Select(a => a.GradeName).FirstOrDefault()) : "",
                        }).FirstOrDefault();




            vmodel.Add(emp1);
            vmodel.Add(emp2);
            return View(vmodel);
        }

        [RedirectingAction]
        // [Authorize(Roles = "Dev,View Employee")]
        public ActionResult Details(long id)
        {
            EmployeeDetailViewModel vmodel = new EmployeeDetailViewModel();
            vmodel = (from a in db.Employees
                      join b in db.Contacts on a.CAddress equals b.ContactID into cont1
                      from b in cont1.DefaultIfEmpty()
                      join c in db.Contacts on a.PAddress equals c.ContactID into cont2
                      from c in cont2.DefaultIfEmpty()
                      join d in db.Users on a.UserId equals d.Id into usr
                      from d in usr.DefaultIfEmpty()
                      join e in db.Branchs on a.BranchID equals e.BranchID

                      join p in db.EmployeePersonals on a.EmployeeId equals p.EmployeeId into empper
                      from p in empper.DefaultIfEmpty()
                      join bk in db.EmployeeBanks on a.EmployeeId equals bk.EmployeeId into ban
                      from bk in ban.DefaultIfEmpty()

                      join wk in db.EmployeeWorkDetails on a.EmployeeId equals wk.EmployeeId into wrk
                      from wk in wrk.DefaultIfEmpty()

                      join de in db.Departments on a.DepartmentID equals de.DepartmentID into dep
                      from de in dep.DefaultIfEmpty()
                      join ds in db.Designations on a.DesignationID equals ds.DesignationID into desg
                      from ds in desg.DefaultIfEmpty()
                      where a.EmployeeId == id
                      select new
                      {
                          a.EMPCode,
                          a.EmployeeId,
                          a.FirstName,
                          a.MiddleName,
                          a.LastName,
                          a.PassportNo,
                          a.OtherIdNo,
                          a.JoinDate,
                          a.LeavingDate,
                          a.ReasonForLeaving,
                          a.Note,
                          b.Phone,
                          a.Gender,
                          b.EmailId,
                          a.JobStatus,

                          b.Address,
                          b.City,
                          b.State,
                          b.Country,
                          b.Zip,

                          CAddress = c.Address,
                          CCity = c.City,
                          CState = c.State,
                          CCountry = c.Country,
                          CZip = c.Zip,

                          DOB = p != null ? p.DOB : null,
                          MaritalStatus = p != null ? p.MaritalStatus : null,
                          BloodGroup = p != null ? p.BloodGroup : null,

                          Mobile = (from ac in db.Mobiles
                                    where (ac.Contact == c.ContactID)
                                    select new MobileViewModel
                                    {
                                        Num = ac.MobileNum,
                                        Name = ac.Name
                                    }).ToList(),
                          ImgFileName = a.ImgFileName,
                          de.DepartmentName,
                          ds.DesignationName,

                          BankName = bk != null ? bk.BankName : "",
                          AccountNo = bk != null ? bk.AccountNo : "",
                          IbanNo = bk != null ? bk.IbanNo : "",
                          BranchName = bk != null ? bk.BranchName : "",
                          Swift = bk != null ? bk.Swift : "",

                          SalaryMode = wk != null ? wk.SalaryMode : 0,
                          WorkShift = wk != null ? wk.WorkShift : null,
                          BonusApplicable = wk != null ? (wk.BonusApplicable == true ? "Yes" : "No") : "No",
                          OTApplicable = wk != null ? (wk.OTApplicable == true ? "Yes" : "No") : "No",
                          SpecifyLoanAccount = wk != null ? (wk.SpecifyLoanAccount == true ? "Yes" : "No") : "No",
                          SpecifyAdvanceAccount = wk != null ? (wk.SpecifyAdvanceAccount == true ? "Yes" : "No") : "No",

                          LoanAccount = wk != null ? wk.LoanAccount : null,
                          AdvanceAccount = wk != null ? wk.AdvanceAccount : null,
                          EmployeeGrade = wk != null ? wk.EmployeeGrade : null,
                          Address1 = b.Address != null ? b.Address : "" + "<br/>" +
                             b.City != null ? b.City : "" +
                             " " + b.State != null ? b.State : "" +
                             " " + b.Country != null ? b.Country : "" +
                             "<br/>" + b.Zip != null ? b.Zip : "",
                          Address2 = c.Address != null ? c.Address : "" + "<br/>" +
                             c.City != null ? c.City : "" +
                             " " + c.State != null ? c.State : "" +
                             " " + c.Country != null ? c.Country : "" +
                             "<br/>" + c.Zip != null ? c.Zip : "",
                      }).ToList().Select(o => new EmployeeDetailViewModel
                      {
                          EMPCode = o.EMPCode,
                          FirstName = o.FirstName,
                          MiddleName = o.MiddleName,
                          LastName = o.LastName,
                          PhoneNumber = o.Phone,
                          Email = o.EmailId,
                          EmployeeId = o.EmployeeId,
                          Gender = Enum.GetName(typeof(Gender), o.Gender),
                          mob = o.Mobile,
                          DOB = o.DOB,
                          PassportNo = o.PassportNo,
                          OtherIdNo = o.OtherIdNo,
                          MaritalStatus = o.MaritalStatus,
                          BloodGroup = o.BloodGroup,
                          Jobstatus = Enum.GetName(typeof(Gender), o.JobStatus),

                          JoinDate = o.JoinDate,
                          LeavingDate = o.LeavingDate,
                          ReasonForLeaving = o.ReasonForLeaving,
                          ImgFileName = o.ImgFileName != "" ? o.ImgFileName : "default.jpg",
                          Note = o.Note,
                          Department = o.DepartmentName,
                          Designation = o.DesignationName,

                          Address = o.Address,
                          City = o.City,
                          State = o.State,
                          Country = o.Country,
                          Zip = o.Zip,

                          CAddress = o.CAddress,
                          CCity = o.CCity,
                          CState = o.CState,
                          CCountry = o.CCountry,
                          CZip = o.CZip,

                          BankName = o.BankName,
                          AccountNo = o.AccountNo,
                          IbanNo = o.IbanNo,
                          BranchName = o.BranchName,
                          Swift = o.Swift,

                          SalaryMode = Enum.GetName(typeof(SalaryMode), o.SalaryMode),
                          WorkShift = o.WorkShift != null ? (db.WorkShifts.Where(a => a.WorkShiftId == o.WorkShift).Select(a => a.WorkShiftName).FirstOrDefault()) : "",
                          BonusApplicable = o.BonusApplicable,
                          OTApplicable = o.OTApplicable,
                          SpecifyLoanAccount = o.SpecifyLoanAccount,

                          LoanAccount = o.LoanAccount != null ? (db.Accountss.Where(a => a.AccountsID == o.LoanAccount).Select(a => a.Name).FirstOrDefault()) : "",
                          AdvanceAccount = o.AdvanceAccount != null ? (db.Accountss.Where(a => a.AccountsID == o.AdvanceAccount).Select(a => a.Name).FirstOrDefault()) : "",
                          EmployeeGrade = o.EmployeeGrade != null ? (db.EmployeeGrades.Where(a => a.EmployeeGradeId == o.EmployeeGrade).Select(a => a.GradeName).FirstOrDefault()) : "",
                      }).FirstOrDefault();

            vmodel.edumodel = (from b in db.EmployeeEducations
                               where b.EmployeeId == id
                               select new EmpEduViewModel
                               {
                                   Course = b.CourseTitle,
                                   Specialization = b.Specialization,
                                   Institute = b.Institute,
                                   University = b.BoardOrUniversity,
                                   PassingYear = b.PassYear,
                                   Percentage = b.Percentage
                               }).ToList();

            vmodel.promodel = (from b in db.EmployeeProfessions
                               where b.EmployeeId == id
                               select new EmpProViewModel
                               {
                                   Organization = b.Organization,
                                   Designation = b.Designation,
                                   FromDate = b.FromDate.ToString(),
                                   ToDate = b.ToDate.ToString(),
                                   Responsibility = b.Responsibility,
                                   Skills = b.Skills
                               }).ToList();


            vmodel.docmodel = (from b in db.EmployeeDocuments
                               where b.EmployeeId == id
                               select new EmpDocViewModel
                               {
                                   DocumentName = b.DocumentName,
                                   DocumentNo = b.DocumentNo,
                                   IssueDate = b.IssueDate.ToString(),
                                   ExpiryDate = b.ExpiryDate.ToString(),
                                   Note = b.Note,
                                   Attachments = b.Attachments
                               }).ToList();
            var latestsalstr = db.SalaryStructures.Where(x => x.EmployeeId == id).OrderByDescending(z => z.CreatedDate).FirstOrDefault();
            if (latestsalstr != null)
            {
                vmodel.SalModel = (from a in db.SalaryStructures
                                   join c in db.SalaryStrDetails on a.SalaryStrId equals c.SalaryStrId into sla
                                   from c in sla.DefaultIfEmpty()
                                   join d in db.Payheads on c.PayHeadId equals d.ID into pay
                                   from d in pay.DefaultIfEmpty()
                                   where (a.EmployeeId == id && a.SalaryStrId == latestsalstr.SalaryStrId)
                                   select new SalaryStrViewModel
                                   {
                                       SalaryStrId = a.SalaryStrId,
                                       EFDate = a.EFDate,
                                       Payhead = d.NameinSlip,
                                       Amount = c.Rate,
                                       Date = a.CreatedDate,
                                   }).ToList();
            }
            return View(vmodel);
        }



        //        serialisedJson = db.Employees.Where(p => p.FirstName.Contains(q) || p.MiddleName.Contains(q) || p.LastName.Contains(q))
        //                .Select(b => new
        //                    text = b.FirstName+" "+b.LastName, //each json object will have 
        //                    id = b.EmployeeId
        //        serialisedJson = db.Employees.Select(b => new
        //            text = b.FirstName + " " + b.LastName, //each json object will have 
        //            id = b.EmployeeId

        public JsonResult SearchEmployeedepartment(string q, string x, string page)
        {
            var UserId = User.Identity.GetUserId();
            long? department = db.Employees.Where(xx => xx.UserId == UserId).Select(xx => xx.DepartmentID).SingleOrDefault();



            List<SelectFormat> serialisedJson;
            string stt = "All";
            var start = Convert.ToInt32(page);
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Employees.Where(p => p.LeavingDate == null && p.DepartmentID == department && p.FirstName.ToLower().Contains(q.ToLower()) || p.MiddleName.ToLower().Contains(q.ToLower()) || p.LastName.ToLower().Contains(q.ToLower()) || p.FirstName.Contains(q) || p.MiddleName.Contains(q) || p.LastName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.FirstName + " " + b.LastName, //each json object will have 
                                      id = b.EmployeeId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Employees.Where(a => a.LeavingDate == null && a.DepartmentID == department).Select(b => new SelectFormat
                {
                    text = b.FirstName + " " + b.LastName, //each json object will have 
                    id = b.EmployeeId
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Employee" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public JsonResult SearchEmployeeTech(string q, string x, string page)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            var start = Convert.ToInt32(page);
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.TeamMembers
                                  join p in db.Employees on a.EmployeeId equals p.EmployeeId
                                  where a.TeamId == 4 &&
                                (p.FirstName.ToLower().Contains(q.ToLower()) || p.MiddleName.ToLower().Contains(q.ToLower()) || p.LastName.ToLower().Contains(q.ToLower()) || p.FirstName.Contains(q) || p.MiddleName.Contains(q) || p.LastName.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = p.FirstName + " " + p.LastName, //each json object will have 
                                      id = p.EmployeeId
                                  })
                                  .OrderBy(p => p.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.TeamMembers
                                  join p in db.Employees on a.EmployeeId equals p.EmployeeId
                                  where a.TeamId == 4
                                  select new SelectFormat
                                  {
                                      text = p.FirstName + " " + p.LastName, //each json object will have 
                                      id = p.EmployeeId
                                  })
                 .OrderBy(p => p.text).ToList();

            }//

            return Json(serialisedJson);
        }

        public JsonResult SearchEmployee(string q, string x, string page)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            var start = Convert.ToInt32(page);
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Employees
                                  join b in db.Users on a.UserId equals b.Id
                                  where b.Status == 1
                                  && !a.FirstName.Contains("(old)")
                                  select a)
                                  .Where(p => p.LeavingDate == null && p.FirstName.ToLower().Contains(q.ToLower()) || p.MiddleName.ToLower().Contains(q.ToLower()) || p.LastName.ToLower().Contains(q.ToLower()) || p.FirstName.Contains(q) || p.MiddleName.Contains(q) || p.LastName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.FirstName + " " + b.LastName, //each json object will have 
                                      id = b.EmployeeId
                                  }).Distinct()
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Employees
                                  join b in db.Users on a.UserId equals b.Id
                                  where b.Status == 1
                                  select a).Where(a => a.LeavingDate == null).Select(b => new SelectFormat
                                  {
                                      text = b.FirstName + " " + b.LastName, //each json object will have 
                                      id = b.EmployeeId
                                  }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Employee" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public JsonResult SearchEmployeenouser(string q, string x, string page)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            var start = Convert.ToInt32(page);
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Employees
                                  
                                  select a)
                                  .Where(p => p.LeavingDate == null && p.FirstName.ToLower().Contains(q.ToLower()) || p.MiddleName.ToLower().Contains(q.ToLower()) || p.LastName.ToLower().Contains(q.ToLower()) || p.FirstName.Contains(q) || p.MiddleName.Contains(q) || p.LastName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.FirstName + " " + b.LastName, //each json object will have 
                                      id = b.EmployeeId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Employees
                                
                                  select a).Where(a => a.LeavingDate == null).Select(b => new SelectFormat
                                  {
                                      text = b.FirstName + " " + b.LastName, //each json object will have 
                                      id = b.EmployeeId
                                  }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Employee" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public JsonResult SearchEmployeeWithUser(string q, string x, string page)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            var start = Convert.ToInt32(page);
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Employees
                                  join c in db.Users on a.UserId equals c.Id
                                  join d in db.EmployeeWorkDetails on a.EmployeeId equals d.EmployeeId into emp
                                  from d in emp.DefaultIfEmpty()
                                  where d.EmployeeAccount != null &&
                                  c.Status == 1
                                  && a.LeavingDate == null && (a.FirstName.ToLower().Contains(q.ToLower()) || a.MiddleName.ToLower().Contains(q.ToLower()) || a.LastName.ToLower().Contains(q.ToLower()) || a.FirstName.Contains(q) || a.MiddleName.Contains(q) || a.LastName.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = a.FirstName + " " + a.LastName, //each json object will have 
                                      id = a.EmployeeId
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Employees
                                  join c in db.Users on a.UserId equals c.Id
                                  join d in db.EmployeeWorkDetails on a.EmployeeId equals d.EmployeeId into emp
                                  from d in emp.DefaultIfEmpty()
                                  where d.EmployeeAccount != null && a.LeavingDate == null
                                  && c.Status == 1
                                  select new SelectFormat
                                  {
                                      text = a.FirstName + " " + a.LastName, //each json object will have 
                                      id = a.EmployeeId
                                  }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Employee" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult SearchEmployeeWithAcc(string q, string x, string page)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            var start = Convert.ToInt32(page);
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Employees
                                  join d in db.EmployeeWorkDetails on a.EmployeeId equals d.EmployeeId into emp
                                  from d in emp.DefaultIfEmpty()
                                  where d.EmployeeAccount != null && a.LeavingDate == null && (a.FirstName.ToLower().Contains(q.ToLower()) || a.MiddleName.ToLower().Contains(q.ToLower()) || a.LastName.ToLower().Contains(q.ToLower()) || a.FirstName.Contains(q) || a.MiddleName.Contains(q) || a.LastName.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = a.FirstName + " " + a.LastName, //each json object will have 
                                      id = a.EmployeeId
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Employees
                                  join d in db.EmployeeWorkDetails on a.EmployeeId equals d.EmployeeId into emp
                                  from d in emp.DefaultIfEmpty()
                                  where d.EmployeeAccount != null && a.LeavingDate == null
                                  select new SelectFormat
                                  {
                                      text = a.FirstName + " " + a.LastName, //each json object will have 
                                      id = a.EmployeeId
                                  }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Employee" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult SearchEmployeeForTask(string q, string x)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Employees
                                  join b in db.Users on a.UserId equals b.Id
                                  where b.Status == 1
                                  select a).Where(p => p.LeavingDate == null && p.FirstName.ToLower().Contains(q.ToLower()) || p.MiddleName.ToLower().Contains(q.ToLower()) || p.LastName.ToLower().Contains(q.ToLower()) || p.FirstName.Contains(q) || p.MiddleName.Contains(q) || p.LastName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.FirstName + " " + b.LastName, //each json object will have 
                                      id = b.EmployeeId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Employees
                                  join b in db.Users on a.UserId equals b.Id
                                  where b.Status == 1
                                  select a).Where(p => p.LeavingDate == null).Select(b => new SelectFormat
                                  {
                                      text = b.FirstName + " " + b.LastName, //each json object will have 
                                      id = b.EmployeeId
                                  }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Employee" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult SearchTeamMember(string q, string x, long? lead)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {


                serialisedJson = (from c in db.Employees
                                  join d in db.Users on c.UserId equals d.Id into usr
                                  from d in usr.DefaultIfEmpty()
                                  where d.Status == 1
                                  select c).Where(p => p.LeavingDate == null && p.FirstName.ToLower().Contains(q.ToLower()) || p.MiddleName.ToLower().Contains(q.ToLower()) || p.LastName.ToLower().Contains(q.ToLower()) || p.FirstName.Contains(q) || p.MiddleName.Contains(q) || p.LastName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.FirstName + " " + b.LastName, //each json object will have 
                                      id = b.EmployeeId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from c in db.Employees
                                  join d in db.Users on c.UserId equals d.Id into usr
                                  from d in usr.DefaultIfEmpty()
                                  where d.Status == 1
                                  select c).Where(p => p.LeavingDate == null).Select(b => new SelectFormat
                                  {
                                      text = b.FirstName + " " + b.LastName, //each json object will have 
                                      id = b.EmployeeId
                                  }).OrderBy(b => b.text).ToList();

            }
            if (lead != null)
            {
                serialisedJson = serialisedJson.Where(a => a.id != lead).ToList();
            }
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Employee " };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }


        public JsonResult SearchEmployeeUser(string q, string x)
        {

            var UserId = User.Identity.GetUserId();
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from c in db.Employees

                                  where c.Status==1
                                  select c).Where(p => p.UserStatus == true && (p.FirstName.ToLower().Contains(q.ToLower()) || p.MiddleName.ToLower().Contains(q.ToLower()) || p.LastName.ToLower().Contains(q.ToLower()) || p.FirstName.Contains(q) || p.MiddleName.Contains(q) || p.LastName.Contains(q)))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.FirstName + " " + b.LastName, //each json object will have 
                                      id = b.EmployeeId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from c in db.Employees
                                  where c.Status == 1
                                  select c).Where(b => b.UserStatus == true).Select(b => new SelectFormat
                                  {
                                      text = b.FirstName + " " + b.LastName, //each json object will have 
                                      id = b.EmployeeId
                                  }).OrderBy(b => b.text).ToList();

            }//
             //         serialisedJson = db.Employees.Where(p => p.LeavingDate == null &&  (p.FirstName.ToLower().Contains(q.ToLower()) || p.MiddleName.ToLower().Contains(q.ToLower()) || p.LastName.ToLower().Contains(q.ToLower()) || p.FirstName.Contains(q) || p.MiddleName.Contains(q) || p.LastName.Contains(q)))
             //                           .Select(b => new SelectFormat
             //                               text = b.FirstName + " " + b.LastName, //each json object will have 
             //                           id = b.EmployeeId
             //                           })
             //         serialisedJson = db.Employees.Where(b => b.LeavingDate == null ).Select(b => new SelectFormat
             //             text = b.FirstName + " " + b.LastName, //each json object will have 
             //             id = b.EmployeeId

            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Employee" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult SearchEmployeeUser2(string q, string x)
        {

            var UserId = User.Identity.GetUserId();
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Employees
                                  join b in db.Users on a.UserId equals b.Id
                                  where b.Status == 1
                                  select a).Where(p => p.LeavingDate == null && p.UserStatus == true && (p.FirstName.ToLower().Contains(q.ToLower()) || p.MiddleName.ToLower().Contains(q.ToLower()) || p.LastName.ToLower().Contains(q.ToLower()) || p.FirstName.Contains(q) || p.MiddleName.Contains(q) || p.LastName.Contains(q)))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.FirstName + " " + b.LastName, //each json object will have 
                                      id = b.EmployeeId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Employees
                                  join b in db.Users on a.UserId equals b.Id
                                  where b.Status == 1
                                  select a).Where(b => b.LeavingDate == null && b.UserStatus == true).Select(b => new SelectFormat
                                  {
                                      text = b.FirstName + " " + b.LastName, //each json object will have 
                                      id = b.EmployeeId
                                  }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Employee" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult SearchEmployeeUserRem(string q, string x)
        {

            var UserId = User.Identity.GetUserId();
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Employees
                                  join b in db.Users on a.UserId equals b.Id
                                  where b.Status == 1 && a.LeavingDate == null && a.UserStatus == true && (b.Name.ToLower().Contains(q.ToLower()) || b.Name.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = b.Name, //each json object will have 
                                      id = a.EmployeeId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Employees
                                  join b in db.Users on a.UserId equals b.Id
                                  where b.Status == 1 && a.UserStatus == true && a.LeavingDate == null
                                  select new SelectFormat
                                  {
                                      text = b.Name, //each json object will have 
                                      id = a.EmployeeId
                                  })
                                 .OrderBy(b => b.text).ToList();
            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Employee" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult SearchEmployeeByDept(string q, string x, string page, long? dept)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            var start = Convert.ToInt32(page);
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Employees
                                  join d in db.EmployeeWorkDetails on a.EmployeeId equals d.EmployeeId into emp
                                  from d in emp.DefaultIfEmpty()
                                  where d.EmployeeAccount != null && a.LeavingDate == null &&
                                  (dept == 0 || dept == null || a.DepartmentID == dept) && a.LeavingDate == null && a.FirstName.ToLower().Contains(q.ToLower()) || a.MiddleName.ToLower().Contains(q.ToLower()) || a.LastName.ToLower().Contains(q.ToLower()) || a.FirstName.Contains(q) || a.MiddleName.Contains(q) || a.LastName.Contains(q)
                                  select new SelectFormat
                                  {
                                      text = a.FirstName + " " + a.LastName, //each json object will have 
                                      id = a.EmployeeId
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Employees
                                  join d in db.EmployeeWorkDetails on a.EmployeeId equals d.EmployeeId into emp
                                  from d in emp.DefaultIfEmpty()
                                  where d.EmployeeAccount != null && a.LeavingDate == null &&
                                  (dept == 0 || dept == null || a.DepartmentID == dept) && a.LeavingDate == null
                                  select new SelectFormat
                                  {
                                      text = a.FirstName + " " + a.LastName, //each json object will have 
                                      id = a.EmployeeId
                                  }).OrderBy(b => b.text).ToList();

            }
            if (x == "all" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Employee " };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult SearchEmployeeLeaveApp(string q, string x, string page)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            var start = Convert.ToInt32(page);
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Employees
                                  join d in db.EmployeeWorkDetails on a.EmployeeId equals d.EmployeeId into emp
                                  from d in emp.DefaultIfEmpty()
                                  where d.EmployeeAccount != null && a.LeavingDate == null && d.AnnualLeaveApplicable == true &&
                                  a.LeavingDate == null && a.FirstName.ToLower().Contains(q.ToLower()) || a.MiddleName.ToLower().Contains(q.ToLower()) || a.LastName.ToLower().Contains(q.ToLower()) || a.FirstName.Contains(q) || a.MiddleName.Contains(q) || a.LastName.Contains(q)
                                  select new SelectFormat
                                  {
                                      text = a.FirstName + " " + a.LastName, //each json object will have 
                                      id = a.EmployeeId
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Employees
                                  join d in db.EmployeeWorkDetails on a.EmployeeId equals d.EmployeeId into emp
                                  from d in emp.DefaultIfEmpty()
                                  where d.AnnualLeaveApplicable == true && d.EmployeeAccount != null && a.LeavingDate == null
                                  select new SelectFormat
                                  {
                                      text = a.FirstName + " " + a.LastName, //each json object will have 
                                      id = a.EmployeeId
                                  }).OrderBy(b => b.text).ToList();

            }
            if (x == "all" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Employee " };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult SearchEmployeeFinalApp(string q, string x, string page)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            var start = Convert.ToInt32(page);
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Employees
                                  where a.FirstName.ToLower().Contains(q.ToLower()) || a.MiddleName.ToLower().Contains(q.ToLower()) || a.LastName.ToLower().Contains(q.ToLower()) || a.FirstName.Contains(q) || a.MiddleName.Contains(q) || a.LastName.Contains(q)
                                  && a.JobStatus != JobStatus.Resigned && a.JobStatus != JobStatus.Absconding
                                  select new SelectFormat
                                  {
                                      text = a.FirstName + " " + a.LastName, //each json object will have 
                                      id = a.EmployeeId
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Employees
                                  where a.JobStatus != JobStatus.Resigned && a.JobStatus != JobStatus.Absconding
                                  select new SelectFormat
                                  {
                                      text = a.FirstName + " " + a.LastName, //each json object will have 
                                      id = a.EmployeeId
                                  }).OrderBy(b => b.text).ToList();

            }
            if (x == "all" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Employee " };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public JsonResult SearchEmployeeResigned(string q, string x, string page)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            var start = Convert.ToInt32(page);
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Employees
                                  where a.FirstName.ToLower().Contains(q.ToLower()) || a.MiddleName.ToLower().Contains(q.ToLower()) || a.LastName.ToLower().Contains(q.ToLower()) || a.FirstName.Contains(q) || a.MiddleName.Contains(q) || a.LastName.Contains(q)
                                  && (a.JobStatus == JobStatus.Resigned || a.JobStatus == JobStatus.Absconding)
                                  select new SelectFormat
                                  {
                                      text = a.FirstName + " " + a.LastName, //each json object will have 
                                      id = a.EmployeeId
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Employees
                                  where (a.JobStatus == JobStatus.Resigned || a.JobStatus == JobStatus.Absconding)
                                  select new SelectFormat
                                  {
                                      text = a.FirstName + " " + a.LastName, //each json object will have 
                                      id = a.EmployeeId
                                  }).OrderBy(b => b.text).ToList();

            }
            if (x == "all" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "All" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        [HttpGet]
        public JsonResult GetAllEmployeeByDept(long dept)
        {
            var teamss = (from a in db.Employees
                          join d in db.EmployeeWorkDetails on a.EmployeeId equals d.EmployeeId into emp
                          from d in emp.DefaultIfEmpty()
                          where a.DepartmentID == dept && d.EmployeeAccount != null
                          select new
                          {
                              EmpName = a.FirstName + " " + a.LastName,
                              a.EmployeeId,
                          }).ToList();
            return Json(teamss);
        }

        public ActionResult GetEmployeeById(int empID)
        {
            var data = (from b in db.Employees
                        where b.EmployeeId == empID
                        select new
                        {
                            b.EMPCode
                        }).FirstOrDefault();
            return Json(data);
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? LegacyIdentity.UserManager(db);
            }
            private set
            {
                _userManager = value;
            }
        }
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }


        public string getEmpCode(Int64 SNo = 0, string SCode = null)
        {
            var prefix = db.CodePrefixs.Where(a => a.section == "Employee").Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == "Employee").Select(a => a.number).FirstOrDefault();
            if (SCode == null)
            {
                if ((db.Employees.Select(p => p.EmployeeId).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    SCode = (number == 0) ? (prefix + 1) : (prefix + number);
                }
                else
                {
                    SNo = db.Employees.Max(p => p.EmployeeId + 1);
                    SCode = prefix + SNo;
                    if (CodeExist(SCode))
                    {
                        SCode = getEmpCode(SNo, SCode);
                    }

                }
            }
            else
            {
                SNo = SNo + 1;
                SCode = prefix + SNo;
                if (CodeExist(SCode))
                {
                    SCode = getEmpCode(SNo, SCode);
                }
            }
            return SCode;
        }
        private bool CodeExist(string Code)
        {
            var Exists = db.Employees.Any(c => c.EMPCode == Code);
            bool res = (Exists) ? true : false;
            return res;

        }
        [HttpGet]
        public JsonResult GetEmployeeName(int EmpID)
        {
            var Name = (from a in db.Employees
                        where a.EmployeeId == EmpID
                        select new
                        {
                            Name = a.FirstName + " " + a.LastName
                        }).FirstOrDefault();
            return Json(Name);

        }

        [HttpGet]
        public JsonResult GetMobile(long CnId)
        {
            var ConD = (from a in db.Mobiles
                        join b in db.Employees on a.Contact equals b.CAddress
                        where b.EmployeeId == CnId
                        select new
                        {
                            Mob = a.MobileNum,
                            Name = a.Name
                        }).ToList();
            return Json(ConD);
        }

        [HttpGet]
        public JsonResult GetEmployeeEducation(long EmpId)
        {
            var ConD = (from a in db.EmployeeEducations
                        where a.EmployeeId == EmpId
                        select new
                        {
                            a.BoardOrUniversity,
                            a.CourseTitle,
                            a.Institute,
                            a.PassYear,
                            a.Percentage,
                            a.Specialization,
                        }).ToList();
            return Json(ConD);
        }
        [HttpGet]
        public JsonResult GetEmployeeProfession(long EmpId)
        {
            var ConD = (from a in db.EmployeeProfessions
                        where a.EmployeeId == EmpId
                        select new
                        {
                            a.Designation,
                            a.FromDate,
                            a.ToDate,
                            a.Organization,
                            a.Responsibility,
                            a.Skills
                        }).ToList();
            return Json(ConD);
        }
        [HttpGet]
        public JsonResult GetEmployeeDocument(long EmpId)
        {
            var ConD = (from a in db.EmployeeDocuments
                        where a.EmployeeId == EmpId
                        select new
                        {
                            a.Attachments,
                            a.DocumentName,
                            a.DocumentNo,
                            a.ExpiryDate,
                            a.IssueDate,
                            a.Note,
                            a.PersonalNo,
                            a.EmployeeDocumentId
                        }).ToList();
            return Json(ConD);
        }

        [HttpGet]
        public JsonResult GetEmployeeGradeById(int empId)
        {
            var empgrade = db.EmployeeWorkDetails.Where(a => a.EmployeeId == empId).Select(a => a.EmployeeGrade).FirstOrDefault();
            if (empgrade != null)
            {
                var v = (from a in db.EmpGradeSalaryDetails
                         join b in db.Payheads on a.Payhead equals b.ID into phead
                         from b in phead.DefaultIfEmpty()
                         where a.EmployeeGradeId == empgrade
                         select new
                         {
                             a.Payhead,
                             b.Name,
                             a.Rate,
                             a.EffectFrom,
                             b.CalculationPeriod,
                             b.Type,
                             b.CalculationType,
                             b.Compute
                         }).AsEnumerable().Select(o => new
                         {
                             PayHeadId = o.Payhead,
                             PayHead = o.Name,
                             o.Rate,
                             Date = o.EffectFrom,
                             Per = o.CalculationPeriod,
                             HeadType = Enum.GetName(typeof(PayHeadType), o.Type),
                             CalType = Enum.GetName(typeof(CalcTypePayHead), o.CalculationType),
                             Computed = Enum.GetName(typeof(ComputPayHead), o.Compute),
                         }).ToList();

                return Json(v);
            }
            else
            {
                return Json(0);
            }

        }

        // [HttpGet]
        public JsonResult GetEmployeeDetails(long EmpId)
        {
            var ConD = (from a in db.Employees
                        join b in db.LeaveSettlements on a.EmployeeId equals b.Employee into leave
                        from b in leave.DefaultIfEmpty()
                        where a.EmployeeId == EmpId
                        select new
                        {
                            a.JoinDate,
                            DutyResumeDate = a.DutyResumeDate != null ? a.DutyResumeDate : a.JoinDate,

                            bsalary = (from a in db.SalaryStructures
                                       join b in db.SalaryStrDetails on a.SalaryStrId equals b.SalaryStrId into sal
                                       from b in sal.DefaultIfEmpty()
                                       let payh = db.Payheads.Where(a => a.Name == "Basic Pay").Select(a => a.ID).FirstOrDefault()
                                       where a.EmployeeId == EmpId && b.PayHeadId == payh
                                       select new
                                       {
                                           b.Rate
                                       }).FirstOrDefault(),
                        }).FirstOrDefault();
            return Json(ConD);
        }

        public JsonResult SearchEmployeeDoc(string q, string x)
        {
            List<SelectFormatDisabled> serialisedJson;
            string stt = " ";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.EmployeeDocuments
                                  where b.DocumentName.ToLower().Contains(q.ToLower()) || b.DocumentName.Contains(q)
                                  select new SelectFormatDisabled
                                  {
                                      text = b.DocumentName, //each json object will have 
                                      id = b.DocumentName

                                  }).Distinct()
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from b in db.EmployeeDocuments
                                  select new SelectFormatDisabled
                                  {
                                      text = b.DocumentName, //each json object will have 
                                      id = b.DocumentName

                                  }).Distinct().OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormatDisabled() { id = "0", text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
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
