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
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class SalesExecutiveController : BaseController
    {
        ApplicationDbContext db;
        Models.Common com;
        public SalesExecutiveController()
        {
            db = new ApplicationDbContext();
            com = new Models.Common();
        }
        // GET: SalesExecutive
     
        public ActionResult Index()
        {
            var name = db.Employees.Select(s => new
            {
                ID = s.EmployeeId,
                Name = s.FirstName
            }).ToList();
            ViewBag.ExeName = QkSelect.List(name, "ID", "Name");
            var code = db.Employees.Select(s => new
            {
                ID = s.EmployeeId,
                Name = s.EMPCode
            }).ToList();
            ViewBag.SalesExeCode = QkSelect.List(code, "ID", "Name");
            return View();
        }
        
        public ActionResult Create()
        {
            var stands = db.Branchs.Where(s => s.Status == Status.active)
                .Select(s => new
                {
                    BranchID = s.BranchID,
                    BranchDetails = s.BranchCode + " - " + s.BranchName
                }).ToList();
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

            ViewBag.Branch = QkSelect.List(stands, "BranchID", "BranchDetails");
            ViewBag.Department = QkSelect.List(Deps, "DepartmentID", "DepartmentName");
            ViewBag.Designation = QkSelect.List(Desgns, "DesignationID", "DesignationName");

            EmployeeViewModel empmodel = new EmployeeViewModel
            {
                EMPCode = getEmpCode(),
                dept = db.Departments.ToList(),
                degn = db.Designations.ToList()
            };
            return View(empmodel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
       
        public ActionResult Create(EmployeeViewModel model)
        {
            var custExists = db.Employees.Any(u => u.EMPCode == model.EMPCode);
            if (custExists)
            {
                Danger("A Sales Executive with same Code exists.", true);
                return RedirectToAction("Create", "SalesExecutive");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var UserId = "";
                    // add Permanent address
                    var name = model.Prefix + " " + model.FirstName + " " + model.MiddleName + " " + model.LastName;
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();

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
                            Group = 2,
                            Status = Status.active
                        };
                        db.Contacts.Add(Pcontact);
                        db.SaveChanges();
                        PcontactId = Pcontact.ContactID;
                        CcontactId = PcontactId;
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
                            };
                            db.Contacts.Add(Ccontact);
                            db.SaveChanges();
                            CcontactId = Ccontact.ContactID;
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
                    }
                    // add data to employee table
                    var Emp = new Employee
                    {
                        EMPCode = model.EMPCode,
                        Prefix = model.Prefix,
                        FirstName = model.FirstName,
                        MiddleName = model.MiddleName,
                        LastName = model.LastName,
                        Gender = model.Gender,
                        PassportNo = model.PassportNo,
                        OtherIdNo = model.OtherIdNo,
                        ImgFileName = fileName != null ? fileName : "default.jpg",
                        DesignationID = model.DesignationID,
                        DepartmentID = model.DepartmentID,
                        PAddress = PcontactId,
                        CAddress = (long)CcontactId,

                        BranchID = BranchID,
                        UserStatus = model.UserStatus,
                        UserId = UserId,
                        Status = 1,
                        CreatedDate = System.DateTime.Now
                    };

                    db.Employees.Add(Emp);
                    db.SaveChanges();

                    com.addlog(LogTypes.Created, UserId, "Employee", "Employees", findip(), Emp.EmployeeId, "Sales Executive Created Successfully");

                    Success("Sales Executive details added successfully.", true);
                    return RedirectToAction("Create", "SalesExecutive");
                }
                else
                {
                    Warning("Looks like something went wrong. Please check your form.", true);
                    return (View());
                }
            }
        }

        [HttpPost]
  
        public ActionResult GetData(long? Name, string Code, string Email, string Phone)
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

            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit Sales Executive");
            var uDelete = User.IsInRole("Delete Sales Executive");

            var v = (from a in db.Employees
                     join b in db.Contacts on a.CAddress equals b.ContactID into tmp
                     from b in tmp.DefaultIfEmpty()
                     join c in db.Branchs on a.BranchID equals c.BranchID into br
                     from c in br.DefaultIfEmpty()
                     where
                     (Name == null || a.EmployeeId == Name) && (Email == "" || b.EmailId == Email) && (Phone == "" || b.Phone == Phone)
                     //&& (Code == "" || a.EMPCode == Code)                      
                     select new
                     {
                         id = a.EmployeeId,
                         a.EmployeeId,
                         a.EMPCode,
                         Name = a.Prefix + " " + a.FirstName + " " + a.LastName,
                         Address = b.Address != null ? b.Address : "" + "<br/>" +
                         b.City != null ? b.City : "" +
                         " " + b.State != null ? b.State : "" +
                         " " + b.Country != null ? b.Country : "" +
                         "<br/>" + b.Zip != null ? b.Zip : "",
                         Phone = b.Phone,
                         Mobile = (from ac in db.Mobiles
                                   where (ac.Contact == b.ContactID)
                                   select new MobileViewModel
                                   {
                                       Num = ac.MobileNum,
                                       Name = ac.Name
                                   }).ToList(),
                         Email = b.EmailId,
                         a.UserStatus,
                         Branch = c.BranchName,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete

                     });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.Name.ToString().ToLower().Contains(search.ToLower())
                                 //p.Branch.ToString().ToLower().Contains(search.ToLower())
                                 );
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

    
        public ActionResult Edit(long? id)
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

            var EMP = new EmployeeViewModel
            {
                id = Employee.EmployeeId,
                EMPCode = Employee.EMPCode,
                Prefix = Employee.Prefix,
                FirstName = Employee.FirstName,
                MiddleName = Employee.MiddleName,
                LastName = Employee.LastName,
                Gender = Employee.Gender,
                PassportNo = Employee.PassportNo,
                OtherIdNo = Employee.OtherIdNo,
                DepartmentID = Employee.DepartmentID,
                DesignationID = Employee.DesignationID,
                ImgFileName = Employee.ImgFileName,
                BranchID = (long)Employee.BranchID,
                UserStatus = Employee.UserStatus,

                dept = db.Departments.ToList(),
                degn = db.Designations.ToList()
            };

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
                }
            }


            var stands = db.Branchs.Where(s => s.Status == Status.active)
              .Select(s => new
              {
                  BranchID = s.BranchID,
                  BranchDetails = s.BranchCode + " - " + s.BranchName
              }).ToList();
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

            ViewBag.Branch = QkSelect.List(stands, "BranchID", "BranchDetails");
            ViewBag.Department = QkSelect.List(Deps, "DepartmentID", "DepartmentName");
            ViewBag.Designation = QkSelect.List(Desgns, "DesignationID", "DesignationName");
            return View(EMP);

        }
        // POST: customer/Edit/5
        [HttpPost]

        public ActionResult Edit(EmployeeViewModel model, long? id)
        {
            if (ModelState.IsValid)
            {
                var CodeExists = db.Employees.Any(u => u.EMPCode == model.EMPCode && u.EmployeeId != id);
                if (CodeExists)
                {
                    Danger("A Sales Executive with same Employee code exists.", true);
                    return RedirectToAction("Edit/" + id, "SalesExecutive");
                }
                else
                {
                    var UserId = User.Identity.GetUserId();

                    Employee emp = db.Employees.Find(id);
                    // add Permanent address
                    var name = model.Prefix + " " + model.FirstName + " " + model.MiddleName + " " + model.LastName;
                    long PcontactId = 0;
                    long CcontactId = 0;


                    Contact con1 = db.Contacts.Find(emp.PAddress);
                    Contact con2 = db.Contacts.Find(emp.CAddress);
                    if (con1 != null)
                        db.Contacts.Remove(con1);
                    if (con2 != null)
                        db.Contacts.Remove(con2);
                    db.SaveChanges();
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
                            Group = 2,
                            Status = Status.active
                        };
                        db.Contacts.Add(Pcontact);
                        db.SaveChanges();
                        PcontactId = Pcontact.ContactID;
                        CcontactId = PcontactId;
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
                            };
                            db.Contacts.Add(Ccontact);
                            db.SaveChanges();
                            CcontactId = Ccontact.ContactID;
                        }
                    }

                    if (model.ImgFileName != null)
                    {
                        // files upload
                        IFormFile file = Request.Form.Files[0];
                        var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                        var uploadUrl = LegacyWeb.MapPath("~/uploads/profile/");
                        file.SaveAs(Path.Combine(uploadUrl, fileName));
                        emp.ImgFileName = fileName;
                    }
                    emp.EMPCode = model.EMPCode;
                    emp.Prefix = model.Prefix;
                    emp.FirstName = model.FirstName;
                    emp.MiddleName = model.MiddleName;
                    emp.LastName = model.LastName;
                    emp.Gender = model.Gender;
                    emp.PassportNo = model.PassportNo;
                    emp.OtherIdNo = model.OtherIdNo;
                    emp.DesignationID = model.DesignationID;
                    emp.DepartmentID = model.DepartmentID;
                    emp.PAddress = PcontactId;
                    emp.CAddress = CcontactId;
                    emp.UserStatus = model.UserStatus;
                    emp.Status = 1;
                    db.Entry(emp).State = EntityState.Modified;
                    db.SaveChanges();


                    com.addlog(LogTypes.Updated, UserId, "Employee", "Employees", findip(), emp.EmployeeId, "Sales Executive Updated Successfully");
                    Success("Successfully Updated Sales Executive Details.", true);
                    return RedirectToAction("Edit/" + id, "SalesExecutive");


                }
            }
            else
            {
                Warning("Looks like something went wrong. Please check your form.", true);
                return (View());
            }
        }


        [HttpGet]
        [QkAuthorize(Roles = "Dev,Delete Sales Executive")]
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
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete Sales Executive")]
        public ActionResult DeleteAction(long id)
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
                msg = "Successfully deleted SalesExecutive details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Sales Executive")]
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
                Success("Deleted " + count + " Sales Executive, Unable to Delete " + notdel + "Sales Executive", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Sales Executive", true);
            }
            else
            {
                Success("Deleted " + count + " Sales Executive", true);
            }
            return RedirectToAction("Index", "SalesExecutive");
        }
        private Boolean DeleteItem(long id)
        {
            var Msg = chkDeleteWithMsg(id);
            bool res = (Msg != null) ? false : DeleteFn(id);
            return res;
        }

        public bool DeleteFn(long? id)
        {
            var UserId = User.Identity.GetUserId();
            Employee Emp = db.Employees.Find(id);
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
            db.Employees.Remove(Emp);
            db.SaveChanges();
            com.addlog(LogTypes.Deleted, UserId, "Employee", "Employees", findip(), Emp.EmployeeId, "Sales Executive Deleted Successfully");

            return true;
        }


        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            if (db.SalesEntrys.Any(c => c.SECashier == id))
            {
                msg = "Sales Executive Already used in Sales Entry !!";
            }
            else if (db.Quotations.Any(c => c.QuotCashier == id))
            {
                msg = "Sales Executive Already used in Quotation !!";
            }
            else if (db.PurchaseEntrys.Any(c => c.PECashier == id))
            {
                msg = "Sales Executive Already used in Purchase Entry !!";
            }
            else if (db.PurchaseReturns.Any(c => c.PRCashier == id))
            {
                msg = "Sales Executive Already used in Purchase Return !!";
            }
            else if (db.SalesReturns.Any(c => c.SRCashier == id))
            {
                msg = "Sales Executive Already used in Sales Return !!";
            }
            else if (db.Deliverynotes.Any(c => c.DvCashier == id))
            {
                msg = "Sales Executive Already used in Deliverynote !!";
            }
            else if (db.ProFormas.Any(c => c.PFCashier == id))
            {
                msg = "Sales Executive Already used in ProForma !!";
            }
            else if (db.SalesOrders.Any(c => c.SOCashier == id))
            {
                msg = "Sales Executive Already used in SalesOrder !!";
            }
            else if (db.PurchaseOrders.Any(c => c.POCashier == id))
            {
                msg = "Sales Executive Already used in PurchaseOrder !!";
            }
            else if (db.StockJournals.Any(c => c.Employee == id))
            {
                msg = "Sales Executive Already used in Stock Journal !!";
            }
            else if (db.JobCards.Any(c => c.ReceivedBy == id) || db.JobCards.Any(c => c.Mechanic == id))
            {
                msg = "Sales Executive Already used in JobCard !!";
            }
            else if (db.HireReturns.Any(c => c.Cashier == id))
            {
                msg = "Sales Executive Already used in Hire Return !!";
            }
            else if (db.Customers.Any(c => c.CustomerID == id))
            {
                msg = "Sales Executive Already used in Customer !!";
            }
            else if (db.PackingLists.Any(c => c.Employee == id))
            {
                msg = "Sales Executive Already used in Packing List !!";
            }
            else if (db.Projects.Any(c => c.SalesPerson == id))
            {
                msg = "Sales Executive Already used in Project !!";
            }
            else
            {
                msg = null;
            }
            return msg;
        }

        [QkAuthorize(Roles = "Dev,Create Sales Executive")]
        public ActionResult AddSalesExecutive()
        {
            var stands = db.Branchs.Where(s => s.Status == Status.active)
                .Select(s => new
                {
                    BranchID = s.BranchID,
                    BranchDetails = s.BranchCode + " - " + s.BranchName
                }).ToList();
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

            ViewBag.Branch = QkSelect.List(stands, "BranchID", "BranchDetails");
            ViewBag.Department = QkSelect.List(Deps, "DepartmentID", "DepartmentName");
            ViewBag.Designation = QkSelect.List(Desgns, "DesignationID", "DesignationName");

            EmployeeViewModel empmodel = new EmployeeViewModel
            {
                EMPCode = getEmpCode(),
                dept = db.Departments.ToList(),
                degn = db.Designations.ToList()
            };

            return PartialView(empmodel);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Create Sales Executive")]
        public JsonResult AddSalesExecutive(EmployeeViewModel model)
        {
            bool stat = false;
            string msg;
            var Exists = db.Employees.Any(u => u.EMPCode == model.EMPCode);
            if (Exists)
            {
                msg = "A Sales Executive with same Code exists.";
                stat = false;
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var UserId = User.Identity.GetUserId();
                    // add Permanent address
                    var name = model.Prefix + " " + model.FirstName + " " + model.MiddleName + " " + model.LastName;
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();

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
                            Group = 2,
                            Status = Status.active
                        };
                        db.Contacts.Add(Pcontact);
                        db.SaveChanges();
                        PcontactId = Pcontact.ContactID;
                        //    // current address
                        //        Name = name,
                        //        Address = model.CAddress,
                        //        City = model.CCity,
                        //        State = model.CState,
                        //        Country = model.CCountry,
                        //        Zip = model.CPostalCode,
                    }

                    //// files upload
                    //var uploadUrl = LegacyWeb.MapPath("~/uploads/profile/");
                    //// add data to employee table
                    var Emp = new Employee
                    {
                        EMPCode = model.EMPCode,
                        Prefix = model.Prefix,
                        FirstName = model.FirstName,
                        MiddleName = model.MiddleName,
                        LastName = model.LastName,
                        Gender = model.Gender,
                        PassportNo = model.PassportNo,
                        OtherIdNo = model.OtherIdNo,
                        // ImgFileName = fileName != null ? fileName : "default.jpg",
                        DesignationID = model.DesignationID,
                        DepartmentID = model.DepartmentID,
                        PAddress = PcontactId,
                        CAddress = (long)CcontactId,

                        BranchID = BranchID,
                        UserStatus = model.UserStatus,
                        UserId = UserId,
                        Status = 1,
                        CreatedDate = System.DateTime.Now
                    };

                    db.Employees.Add(Emp);
                    db.SaveChanges();

                    com.addlog(LogTypes.Created, UserId, "Employee", "Employees", findip(), Emp.EmployeeId, "Sales Executive Created Successfully");
                    msg = "Sales Executive details added successfully.";
                    stat = true;
                }
                else
                {
                    msg = "Looks like something went wrong. Please check your form.";
                    stat = false;
                }
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        private string getEmpCode(Int64 SNo = 0, string SCode = null)
        {
            var prefix = db.CodePrefixs.Where(a => a.section == "Employee").Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == "Employee").Select(a => a.number).FirstOrDefault();
            if (SCode == null)
            {
                if ((db.Employees.Select(p => p.EmployeeId).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    if (number == 0)
                    {
                        SCode = prefix + 1;
                    }
                    else
                    {
                        SCode = prefix + number;
                    }
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
