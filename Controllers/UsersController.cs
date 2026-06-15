using QuickSoft.Web;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http.Extensions;
using ApplicationUserManager = Microsoft.AspNetCore.Identity.UserManager<QuickSoft.Models.ApplicationUser>;
using ApplicationSignInManager = Microsoft.AspNetCore.Identity.SignInManager<QuickSoft.Models.ApplicationUser>;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using QuickSoft.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using Microsoft.EntityFrameworkCore;
using System.Net;
using QuickSoft.ViewModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mail;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http.Headers;

namespace QuickSoft.Controllers
{
    public class UsersController : BaseController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        public RoleManager<IdentityRole> RoleManager { get; private set; }

        ApplicationDbContext db;
        Common com;
        public async Task<ActionResult> CreateApplication(long? id, string type)
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
                EMPCode ="1",
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
                    empmodel.UserStatus = Employee.UserStatus;
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

        public async Task<ActionResult> qrcodeprint(long? id)
        {
           
            var customer = db.SalesEntrys.Where(o => o.SalesEntryId == id).Select(o => o.Customer).FirstOrDefault();
            var cus = db.Customers.Find(customer);
            var username = cus.AccountNo;
            var password = cus.IbanNo;
            if (username == null)
            {
                Danger("User Name And Password Not Created. please contact Office");
                return RedirectToAction("failed", "Users");
            }

            // RoleManager = LegacyIdentity.RoleManager(db);
            com = new Common();

            var user = await UserManager.FindByEmailAsync(username + "@gmail.com");
            if (user == null)
            {
                user = await UserManager.FindByNameAsync(username);
            }



            if (1 == 1)
            {
                if (user != null)
                {
                    Response.Cookies.Delete("FinYearID");
                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie); 

                    // Security hardening (audit S7): failed logins now count towards lockout
                    // (5 attempts -> 5-minute lock; configured in Program.cs Identity options).
                    var result = (await SignInManager.PasswordSignInAsync(username, password, true, lockoutOnFailure: true)).ToSignInStatus();

                    switch (result)
                    {
                        case SignInStatus.Success:

                            Session["Name"] = user.UserName;
                            string userId = user.Id;
                            var hash = db.Users.Find(userId);

                            var Company = db.companys.Where(a => a.CompanyID == 1).Select(a => a.CPName).FirstOrDefault();
                            Session["Company"] = Company;
                            Session["BusinessType"] = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                            var cu = db.Customers.Where(o => o.BankName == user.Id).Select(o => o.BankName).FirstOrDefault();
                            long customerid = db.Customers.Where(o => o.BankName == user.Id).Select(o => o.CustomerID).FirstOrDefault();

                            if (cu != null)
                            {
                                var UserId = user.Id;
                                var roles = await UserManager.GetRolesAsync(UserId);
                                await UserManager.RemoveFromRolesAsync(UserId, roles.ToArray());
                                string[] myrols = { "View Customer", "Sales Entry List", "Quotation List", "Recievable Outstanding", "Project List", "ProTask List", "All Customers", "Download Sales Entry","Download Quotation","Ledger" };
                               
                                await this.UserManager.AddToRolesAsync(UserId, myrols);
                                return RedirectToLocal("/Leads/customerdashboard");
                            }
                            else if (user.Discount == 0)
                            {
                                return RedirectToLocal("/Leads/customerdashboard");

                            }
                            else if (Session["BusinessType"].ToString().Contains("Property"))
                                return RedirectToLocal("/Property/PropertyHome/Index");
                            else

                                return RedirectToLocal("/Leads/customerdashboard");

                        case SignInStatus.LockedOut:
                            // brute-force protection: too many failed attempts -> temporary lock
                            return View("Lockout");
                        default:
                            ModelState.AddModelError("", "Invalid login attempt.");
                            return RedirectToAction("failed", "Users");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return RedirectToAction("failed", "Users");
                }
            }






            ModelState.AddModelError("", "Invalid login attempt.");
            Danger("username and password wrong", true);
            RedirectToAction("failed", "User");


        }
        public UsersController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, RoleManager<IdentityRole> roleManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
            db = new ApplicationDbContext();
            com = new Common();
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? _signInManager;
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? _userManager;
            }
            private set
            {
                _userManager = value;
            }
        }
        [AllowAnonymous]
        public ActionResult resize()
        {
            return PartialView();
        }
        //
        // GET: /Users/Login
        [AllowAnonymous]
        [RedirectingAction]
        public ActionResult Login(string returnUrl)
        {
            LoginViewModel model = new LoginViewModel();
            if (Request.Cookies["QuickERP2"] != null)
            {
                if (1 == 1 && (Request.Cookies["QuickERP2"] != null || Request.Cookies["QuickERP2"] != ""))
                {
                    string phash = Request.Cookies["QuickERP2"];
                    db = new ApplicationDbContext();

                    // RoleManager = LegacyIdentity.RoleManager(db);
                    var us = db.Users.Where(o => o.PasswordHash == phash).FirstOrDefault();
                    if (us != null)
                    {

                        var role = db.Roles.Where(o => o.Name == "Auto Login").ToList();
                        //&& o.Users.Select(x => x.UserId).ToList().ToArray().Contains(us.Id)
                        var connnn = role.Any(o => db.UserRoles.Any(a => a.RoleId == o.Id && a.UserId == us.Id));
                        if (connnn)
                        {
                            model.branch = "saveme";
                            model.Email = "";
                            model.Password = "";
                        }
                        else
                        {
                            model.Email = "";
                            model.Password = "";
                        }

                    }
                }
            }  //--


                if (User.Identity.IsAuthenticated)
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            }
            ViewBag.ReturnUrl = returnUrl;
            companySet();
            return View(model);
        }

        public ActionResult changepwd()
        {

            return View();
        }
        public HttpCookie createerpcookie(string val)
        {
            HttpCookie StudentCookies = new HttpCookie("QuickERP2");
            StudentCookies.Value = val;
            StudentCookies.Expires = DateTime.Now.AddDays(365);
            StudentCookies.Secure = false;
            return StudentCookies;
        }

        //some action method
        
        [HttpPost]
        public async Task<ActionResult> changepwd(changepwd model)
        {
            var userid = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                var result = await UserManager.ChangePasswordAsync(userid, model.oldpassword, model.newpassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(userid);
                    if (user != null)
                    {

                        await UserManager.UpdateSecurityStampAsync(User.Identity.GetUserId());
                        //
                    }
                    Success("Change Password Success", true);
                    Response.Cookies.Delete("FinYearID");
                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                    return RedirectToAction("Login", "Users");

                }
                else
                {
                    Danger("Change Password failed", true);

                }
            }
            else
            {
                Warning("Please Check Form");
            }

            return RedirectToAction("changepwd", "Users");

        }
        [AllowAnonymous]
        [RedirectingAction]
        public ActionResult failed()
        {
            return View();
        }
        [AllowAnonymous]
        public void downloadq(int noq, int op, string answer)
        {
            new WebClient().DownloadFile("http://quicknet.fortiddns.com:1200/users/qpaper/?noq="+noq+"&op="+op+"&answer="+answer, @"C:\shiyas\q.html");

            // Or you can get the file content without saving it
           
        }
        [AllowAnonymous]


        public ActionResult qpaper(int noq,int op,string answer)
        {
            ViewBag.noq = noq;
            ViewBag.op = op;
            ViewBag.answer = answer;
                return PartialView();

        }

        [AllowAnonymous]
    
    
        public ActionResult register()
        {
            return View();
        }
 
        [HttpPost]
        public async Task<ActionResult> registerAsync(Register vmodel)
        {
            if (ModelState.IsValid)
            {
              
           
                var UserId = User.Identity.GetUserId();
                var Exists = db.Users.Any(c => c.Email == vmodel.email);
                if (Exists)
                {
                    Warning("Email already exists.", true);
                    //// return RedirectToAction("register", "Users",a=>a.);
                    return RedirectToAction("register", "Users", vmodel);
                }
                else
                {
                    long Branch = 0;

                 
                    
                    var user = new ApplicationUser
                    {
                        Name = vmodel.FirstName,
                        PhoneNumber = vmodel.mobile,
                        Email = vmodel.email,
                        Status = 1,
                        BranchID = Branch,
                        UserName = vmodel.UserName,
                        BranchAccess = BranchAccess.Current,
                        Discount =0,
                        
                    };
                    ApplicationUser userchk = await UserManager.FindByNameAsync(vmodel.UserName);
                    if (userchk != null)
                    {
                        Warning("User Name already exists.", true);
                        return RedirectToAction("register", "Users", vmodel);
                    }
                    else
                    {
                        var result = await UserManager.CreateAsync(user, vmodel.Password);
                        if (result.Succeeded)
                        {
                            UserId = user.Id;
                            if (UserId != null)
                            {
                                await UserManager.UpdateSecurityStampAsync(UserId);

                                 var hash = db.Users.Find(UserId);
                                HttpCookie StudentCookies = new HttpCookie("QuickERP2");
                                StudentCookies.Value = hash.PasswordHash.ToString();
                                StudentCookies.Expires = DateTime.Now.AddDays(365);
                                Response.SetCookie(StudentCookies);

                                UserId = user.Id;
                                string[] myrols = {"POS Entry"};
                                await this.UserManager.AddToRolesAsync(UserId, myrols);

                                Contact cn = new Contact
                                {
                                    FirstName = vmodel.FirstName,
                                    LastName = vmodel.LastName,
                                    Name = vmodel.Enqury,

                                    Mobile = vmodel.mobile,
                                    EmailId = vmodel.email,
                                    Address = vmodel.Address,
                                    ContactTypeID = 999,
                                    Reference = UserId,
                                };
                                db.Contacts.Add(cn);
                                db.SaveChanges();
                                Success("User details added successfully.", true);
                                return RedirectToAction("Login", "Users");

                            }
                        }
                        else
                        {
                            AddErrors(result);

                            ViewBag.error = result.Errors;
                            ViewBag.data = result;
                            Warning(ViewBag.error, true);
                            return RedirectToAction("register", "Users", vmodel);
                        }
                    }
                    return RedirectToAction("Login", "Users");
                }
            }


            return RedirectToAction("Login", "Users");
        }

        [AllowAnonymous]
        [RedirectingAction]

        public ActionResult Training(string returnUrl, Training vmodel)
        {
            ModelState.Clear();
      
            Training model = new Training();
            if (User.Identity.IsAuthenticated)
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            }
            ViewBag.ReturnUrl = returnUrl;
            companySet();
            if (vmodel.FirstName != null)
                return View(vmodel);
            else
                return View(model);

        }

        [HttpPost]
        public ActionResult Trainingcreate(Training vmodel)
        {







            Contact cn = new Contact
            {
                FirstName = vmodel.FirstName,
                LastName = vmodel.LastName,
                Name = vmodel.Company,
                Phone = vmodel.mobile,
                EmailId = vmodel.email,
                Address = vmodel.Address,
                ContactTypeID = 998,
                Group = 10007
            };

            db.Contacts.Add(cn);
            db.SaveChanges();
            var cnid = cn.ContactID;
            SendMail sm = new SendMail();
            MailMessage message = new MailMessage();
            string ToMail = vmodel.email;
            string CcMail = "";
            
            string mess = db.EmailTemplates.Find(2).EmailBody;
            var em = db.EmailTemplates.Where(a => a.Head == "Deliverynote").FirstOrDefault();
            if (em != null)
            {
                message.Subject = em.Subject;
                message.Body = em.EmailBody;

                message.Subject = "Iftar Party 02-04-2024 at 6 PM";
                sm.sendMailwithoutattachment(ToMail, CcMail, cnid.ToString(), message);
            }
                return RedirectToAction("Training", new { success = 1 });

        }
        //
        // POST: /Users/Login
        [HttpPost]
        [AllowAnonymous]
        // [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }
           if(model.branch!=null)
            {
                db = new ApplicationDbContext(model.branch);
              
                RoleManager = LegacyIdentity.RoleManager(db);
                com = new Common();
                Session["branch"] = model.branch;
            }
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                user = await UserManager.FindByNameAsync(model.Email);
            }



            if (user != null && user.Status == 0)
            {
                ModelState.AddModelError("", "Your Account Disabled. Please Contact System Admin.");
                return View(model);
            }
            else
            {
                if (user != null)
                {
                    // Security hardening (audit S7): failed logins count towards lockout (5 attempts ->
                    // 5-minute lock; Identity options in Program.cs). The LockedOut case below shows the lock view.
                    var result = (await SignInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: true)).ToSignInStatus();

                    switch (result)
                    {
                        case SignInStatus.Success:
                            
                                Session["Name"] = user.UserName;
                            string userId = user.Id;
                            var hash = db.Users.Find(userId);
                            
                                if (Request.Cookies["QuickERP2"] == null)
                            {


                                Response.SetCookie(createerpcookie(hash.PasswordHash.ToString()));

                            }
                            else
                            {

                                HttpCookie StudentCookies = new HttpCookie("QuickERP2");
                                StudentCookies.Value = hash.PasswordHash.ToString();
                                StudentCookies.Expires = DateTime.Now.AddDays(365);
                                Response.SetCookie(StudentCookies);

                            }

                            var Company = db.companys.Where(a => a.CompanyID == 1).Select(a => a.CPName).FirstOrDefault();
                            Session["Company"] = Company;
                            //}s
                            Session["BusinessType"] = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
                            var cus = db.Customers.Where(o => o.BankName == user.Id).Select(o => o.BankName).FirstOrDefault();
                            if(cus!=null)
                            {
                                long customerid = db.Customers.Where(o => o.BankName == user.Id).Select(o => o.CustomerID).FirstOrDefault();
                                return RedirectToLocal("/Leads/customerdashboard");
                            }
                            else if(user.Discount==0)
                            {
                                long customerid = db.Customers.Where(o => o.CustomerName == user.Name).Select(o => o.CustomerID).FirstOrDefault();
                                return RedirectToLocal("/Leads/customerdashboard");
                            }
                            else if (model.fromapp== "valid")
                                return RedirectToLocal("/Employeeattendance/Create");
                            else if(Session["BusinessType"].ToString().Contains("Property"))
                                return RedirectToLocal("/Property/PropertyHome/Index");
                            else
                               
                            return RedirectToLocal(returnUrl);
                        case SignInStatus.LockedOut:
                            return View("Lockout");
                        case SignInStatus.RequiresVerification:
                            return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                        case SignInStatus.Failure:
                        default:
                            ModelState.AddModelError("", "Invalid username or password.");
                            return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(model);
                }
            }
        }
        public static void SetCookie(HttpContext context, string key, string value, int expireDay = 1)
        {
            var cookie = new HttpCookie(key, value);
            cookie.Expires = DateTime.Now.AddDays(365);
            context.Response.SetCookie(cookie);
        }
        [AllowAnonymous]
        // [ValidateAntiForgeryToken]
        [HttpGet]
        public async Task<ActionResult> loginapp(string username,string password,string app,string lat,string log)
        {

            var useid = User.Identity.GetUserId();
           


            var user = await UserManager.FindByEmailAsync(username);

            string msg = "";
            bool stat = false;

       
                    // This doesn't count login failures towards account lockout
                    // To enable password failures to trigger account lockout, change to lockoutOnFailure: true
                    var result = (await SignInManager.PasswordSignInAsync(username, password,true, lockoutOnFailure: false)).ToSignInStatus();
            string userid = HttpContext.User.Identity.GetUserId();
            string pos = "?lat=" + lat + "&log=" + log + "&fromapp=" + app;
            if (result == SignInStatus.Failure)
            {
                return NotFound();
            }
            return RedirectToAction("createatt", "Table");


        }
        public ActionResult LogOff()
        {
            
            Response.Cookies.Delete("FinYearID");
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Users");
        }
        //
        // GET: /Users/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Users/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        // [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = (await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberClient: model.RememberBrowser)).ToSignInStatus();
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }
        //
        // GET: /Users/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Users/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        // [ValidateAntiForgeryToken]
        public async Task<JsonResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            bool stat = false;
            string msg = "";
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    msg = "Please check your email to reset your password.";
                    stat = false;
                }
                else
                {
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Scheme);
                    await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                    msg = "Please check your email to reset your password.";
                    stat = true;
                }
            }

            // If we got this far, something failed, redisplay form
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            var exist = db.companys.Any(o => o.CPName.Contains("HEIRS"));
            if(exist)
                return RedirectToAction("accountsdashboard", "Leads");
            else
            return RedirectToAction("Index", "Home");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
        //[HttpPost]
        //    //Find Order Column


        //    // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
        //        b.Id,
        //        b.Name,
        //        b.PhoneNumber,
        //        b.Email,
        //        b.Status

        //    //search
        //        // Apply search   

        //    //SORT



        // [HttpGet]

        // // POST: Customer/ChangeStatus/
        // [HttpPost, ActionName("ChangeStatusActive")]
        // [ValidateAntiForgeryToken]



        //     return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        // //GET: /Change Status Inactive
        // [HttpGet]

        // // POST: Customer/ChangeStatus/
        // [HttpPost, ActionName("ChangeStatusInActive")]
        // [ValidateAntiForgeryToken]


        //     return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        // [HttpGet]


        //                 .Select(s => new
        //                     BranchID = s.BranchID,
        //                     BranchDetails = s.BranchCode + " - " + s.BranchName
        //                 .Select(s => new
        //                     DepartmentID = s.DepartmentID,
        //                     DepartmentName = s.DepartmentName
        //                 .Select(s => new
        //                     DesignationID = s.DesignationID,
        //                     DesignationName = s.DesignationName






















        // [HttpPost]
        // [ValidateAntiForgeryToken]
















        //             //Update the user details

        //             //If user has existing Role then remove the user from the role
        //             // This also accounts for the case when the Admin selected Empty from the drop-down and
        //             // this means that all roles for the user must be removed
        //             //if (!String.IsNullOrEmpty(UserRoles.ToString()))
        //             //    //Find Role
        //             //    //Add user to new role
        //             //    if (!result.Succeeded)



        //     return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        // [HttpGet]
        // [HttpPost, ActionName("Delete")]
        // [ValidateAntiForgeryToken]
        //     return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        // [HttpGet]




        //     UserDetailViewModel userView = new UserDetailViewModel
        //         EmployeeId = User.EmployeeId,
        //         FirstName = User.FirstName,
        //         MiddleName = User.MiddleName,
        //         LastName = User.LastName,
        //         Email = User.Email,
        //         PhoneNumber = User.PhoneNumber,
        //         PassportNo = User.PassportNo,
        //         OtherIdNo = User.OtherIdNo,

        //         Address = User.Address,
        //         City = User.City,
        //         State = User.State,
        //         Country = User.Country,
        //         PostalCode = User.PostalCode,

        //         CAddress = User.CAddress,
        //         CCity = User.CCity,
        //         CState = User.CState,
        //         CCountry = User.CCountry,
        //         CPostalCode = User.CPostalCode,





        //         ImgFileName = User.ImgFileName,



        // [HttpGet]

        //     //var user = context.Users.Select(b => new
        //     //    id= b.Id,
        //     //    text = b.Prefix + " " + b.FirstName + " " + b.MiddleName + " " + b.LastName


        //     List<ApplicationUser> userList = context.Users.ToList(); //fetch list of items from db table
        //     List<ApplicationUser> resultsList = new List<ApplicationUser>(); //create empty results list

        //         //if any item contains the query string
        //             resultsList.Add(item); //then add item to the results list

        //     resultsList.Sort(delegate (ApplicationUser c1, ApplicationUser c2) { return c1.FirstName.CompareTo(c2.FirstName); }); //sort the results list alphabetically by ItemName
        //                              text = result.FirstName + " " + result.MiddleName + " " + result.LastName, //each json object will have 
        //                              id = result.Id      //these two variables [name, id]





        // [HttpPost]
        // [ValidateAntiForgeryToken]

        //     return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, error=result.Errors } };

        // #region roles
        // // GET: Roles
        // [HttpPost]
        //     //Find Order Column


        //     // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
        //         b.Id,
        //         b.Name
        //     //search
        //         // Apply search   

        //     //SORT


        // // GET: Field/Create

        // ////POST: Roles/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]


        //             context.Roles.Add(new Microsoft.AspNet.Identity.EntityFramework.IdentityRole()
        //                 Name = model.Name


        //             //var user = new ApplicationUser
        //             //    Name = model.Name,

        //             //if (result.Succeeded)

        //     return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        // // GET: dep/Edit/5
        // [HttpGet]




        // [HttpPost]

        //     return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        // // GET: Desg/Delete/5
        // [HttpGet]

        // // POST: Field/Delete/5
        // [HttpPost]
        //     return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };


        public JsonResult SearchUser(string q, string x)
        {

            List<SelectUserFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Users.Where(p =>p.Discount==null&&p.Status==1 && p.UserName.ToLower().Contains(q.ToLower()) || p.UserName.Contains(q))
                                  .Select(b => new SelectUserFormat
                                  {
                                      text = b.UserName, //each json object will have 
                                      id = b.Id
                                  }).Take(10)
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Users.Where(p=>p.Discount == null&&p.Status==1).Select(b => new SelectUserFormat
                {
                    text = b.UserName,
                    id = b.Id
                }).Take(10).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectUserFormat() { id = stt, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectUserFormat() { id = null, text = "Select" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);





            //    serialisedJson = db.Users.Where(p => p.Name.Contains(q))
            //            .Select(b => new
            //                text = b.Name, //each json object will have 
            //                id = b.Id
            //    serialisedJson = db.Users.Select(b => new
            //        text = b.Name, //each json object will have 
            //        id = b.Id



            //    id= b.Id,
            //    text = b.Prefix + " " + b.FirstName + " " + b.MiddleName + " " + b.LastName


            //    List<ApplicationUser> userList = context.Users.ToList(); //fetch list of items from db table
            //List<ApplicationUser> resultsList = new List<ApplicationUser>(); //create empty results list

            //    //if any item contains the query string
            //        resultsList.Add(item); //then add item to the results list

            //resultsList.Sort(delegate (ApplicationUser c1, ApplicationUser c2) { return c1.Name.CompareTo(c2.Name); }); //sort the results list alphabetically by ItemName
            //                         text = result.Name, //each json object will have 
            //                         id = result.Id      //these two variables [name, id]


        }

        public JsonResult SearchUserMC(string q, string x)
        {

            List<SelectUserFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.Users
                                      /*join c in db.MCs on b.Id equals c.AssignedUser into mc
                                      from c in mc.DefaultIfEmpty()
                                      where b.Id != c.AssignedUser &&*/
                                  where b.UserName.ToLower().Contains(q.ToLower()) || b.UserName.Contains(q)
                                  select new SelectUserFormat
                                  {
                                      text = b.UserName, //each json object will have 
                                      id = b.Id
                                  }).Take(10)
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from b in db.Users
                                  //where b.Id != c.AssignedUser
                                  select new SelectUserFormat
                                  {
                                      text = b.UserName,
                                      id = b.Id
                                  }).Take(10).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectUserFormat() { id = stt, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectUserFormat() { id = "0", text = "Select" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);

        }

       public ActionResult sendsms()
        {
            smssend vmodel = new smssend();
            return View(vmodel);
        }
        [HttpPost]
        public ActionResult sendsms(smssend vmodel)
        {
            var config = (from a in db.companys
                          select new
                          {
                              a.smssenderid,
                              a.username,
                              a.password
                          }).FirstOrDefault();
            if (config.smssenderid == "" || config.smssenderid == null)
                Danger("Please Set Sms Settings");
            else
            {
                var client = new WebClient();
                string url = "https://rslr.connectbind.com:8443/bulksms/bulksms?username=" +
                    config.username + "&password=" + config.password + "&type=0&dlr=1&destination=971" +
                    vmodel.mobileno + "&source=" + config.smssenderid + "&message=" + vmodel.Message;
                var content = client.DownloadString(url);
                var data = content.Split('|').ToArray();
                if (data[0] == "1701")
                {
                    Success("sms send success",true);
                }
                else
                {
                    Warning("sms send failed", true);
                }
            }
            return View();
        }



        public ActionResult sendnotificationtoall()
        {
            smssend vmodel = new smssend();
            return View(vmodel);
        }
        public ActionResult sendwhatsapp()
        {
            smssend vmodel = new smssend();
            return View(vmodel);
        }
        [HttpPost]
        public async Task<ActionResult> sendwhatsapp(smssend vmodel)
        {
           
         string serverKey = "AAAASPbOfEM:APA91bHOPztKxzZZZqh53OUKC7Us623hlqjcnk9UUu2qEA_UAgJBx4BShtlaCJHlIlwAxgwW9QHHT10Qe2XcqTLjk2LXmcTErza7fouCg4Jhzfk7RvVcWoZNmuImZ-hbK5-QDBBHR-fn";
     string senderId = "313378372675";
       string webAddr = "https://fcm.googleapis.com/fcm/send";
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri("https://fcm.googleapis.com/v1/projects/myproject-b5ae1/messages:send");
            client.DefaultRequestHeaders
                                  .Accept
                                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("key", "=" + serverKey);
            client.DefaultRequestHeaders.Add("Sender", "id=" + senderId);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "relativeAddress");
            var data = new
            {
                to = "eZou9tF3R9aNRqOU7UlslI:APA91bFUMmFIWKu2j7__mQnLzL7ThFrBlUZcdGY-zSSWSmLDFYur3k_SXCnZ4dRnUnm6r4cI8tmfeKUApbvM2C2_RLm3r8EKTjjYJRFW32UfkrNcDCHyK9I",
                notification = new
                {
                    body = vmodel.Message,
                    title = "This is the title",
                    icon = "myicon"
                }
            };

            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(data);
            request.Content = new StringContent(json,
                                                Encoding.UTF8,
                                                "application/json");//CONTENT-TYPE header

            var data1 = client.PostAsync("send", request.Content);
            var d = data1.Result.Content.ReadAsStringAsync();

           
                   
                
            
            return View();
        }
        public ActionResult UpdateProfile()
        {
            ChangePasswordViewModel model = new ChangePasswordViewModel();
            var users = UserManager.FindByIdAsync(User.Identity.GetUserId()).Result;

            var UserId = User.Identity.GetUserId();

            model.UserName = users.UserName;
            model.EmailId = users.Email;

            return View(model);
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<ActionResult> UpdateProfile(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);

            var users = UserManager.FindByIdAsync(User.Identity.GetUserId()).Result;
            users.UserName = model.UserName;
            users.Email = model.EmailId;
            var result1 = await UserManager.UpdateAsync(users);

            if (result.Succeeded && result1.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await UserManager.UpdateSecurityStampAsync(User.Identity.GetUserId());

                    await SignInManager.SignInAsync(user, isPersistent: false);
                }
                var UserId = User.Identity.GetUserId();
                //passed 0 as LogID
                com.addlog(LogTypes.Logged, UserId,model.OldPassword, model.NewPassword, findip(), 0, "UpdateProfile");
                string userId = user.Id;
                var hash = db.Users.Find(userId);
                HttpCookie StudentCookies = new HttpCookie("QuickERP2");
                StudentCookies.Value = hash.PasswordHash.ToString();
                StudentCookies.Expires = DateTime.Now.AddDays(365);
                Response.SetCookie(StudentCookies);
                return RedirectToAction("Login", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }
        private QuickSoft.Models.LegacyAuthManager AuthenticationManager => new QuickSoft.Models.LegacyAuthManager(HttpContext);
        // #endregion
        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //User

        [QkAuthorize(Roles = "Dev,User List")]
        public ActionResult Index()
        {
            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

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

            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,User List")]
        public JsonResult GetUsers(long? ddlType)
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
            var uEdit = User.IsInRole("Edit User");
            var uDelete = User.IsInRole("Delete User");

            var v = (from a in db.Users
                     join b in db.Branchs on a.BranchID equals b.BranchID into cust
                     from b in cust.DefaultIfEmpty()
                     where a.Discount==null && 
                     (ddlType==3||a.Status==ddlType)
                     select new
                     {
                         Names = a.Name,
                         id = a.Id,
                         currentUser = UserId,
                         a.Email,
                         a.PhoneNumber,
                         a.UserName,
                         Branch = b.BranchName,
                         status = a.Status,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete

                     });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.Email.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.PhoneNumber.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.UserName.ToString().ToLower().Contains(search.ToLower()));

            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }

            recordsTotal = v.Count();
            var data = v.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
      
        public ActionResult copypermission()
        {
            var emp1 = db.Users.Where(o=>o.Discount==null && o.Status==1).Select(o => new { o.Id, o.Name }).ToList();
            ViewBag.emp1 = QkSelect.List(emp1, "Id", "Name");
            var emp2 = db.Users.Where(o => o.Discount == null && o.Status == 1).Select(o => new { o.Id, o.Name }).ToList();
            ViewBag.emp2 = QkSelect.List(emp2, "Id", "Name");
            return View();
        }
        [HttpPost]
        
        public async Task<ActionResult> copypermissionAsync(permissioncopy per)
        {
            if (ModelState.IsValid)
            {
                IList<string> data = await UserManager.GetRolesAsync(per.emp1);
               foreach(var emp in per.emp2)
                {
                    IList<string> existroles = await UserManager.GetRolesAsync(emp);
                    await this.UserManager.RemoveFromRolesAsync(emp, existroles.ToArray());

                    await this.UserManager.AddToRolesAsync(emp, data.ToArray().Distinct().ToArray());
                }
                Success("Copy Success", true);
            }
            else
            {
                Warning("Please Check Form",true);


            }
            var emp1 = db.Users.Where(o => o.Discount == null).Select(o => new { o.Id, o.Name }).ToList();
            ViewBag.emp1 = QkSelect.List(emp1, "Id", "Name");
            var emp2 = db.Users.Where(o => o.Discount == null).Select(o => new { o.Id, o.Name }).ToList();
            ViewBag.emp2 = QkSelect.List(emp2, "Id", "Name");
            return RedirectToAction("copypermission","Users");
        }
        [QkAuthorize(Roles = "Dev,Create User")]
        public ActionResult Create()
        {
            var list = QkSelect.List(
                       new List<SelectListItem>
                       {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                       }, "Value", "Text", 1);
            ViewBag.MC = list;
            string[] ps = { "", "Sales", "override credit limit and credit period", "Purchase", "SalesReturn", "PurchaseReurn", "StockTransfer", "Payment", "Reciept", "Journal" };
            var pur = ps.Select(o => new
            {
                id = o,
                text = o,

            }).ToList();
            ViewBag.purposes = new MultiSelectList(pur, "id", "text");
            var currentuser = db.Users.Count();
            var details = db.SystemConfigs.SingleOrDefault();
            var givenuser= Security.Decrypt(details.NumberOfUsers, General.keyval);

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

            if (currentuser == Convert.ToInt32(givenuser))
            {
                ViewBag.UserExceed = true;
                return View();

            }
            else
            {
                ViewBag.UserExceed = false;
                //    .Select(s => new
                //        BranchID = s.BranchID,
                //        BranchDetails = s.BranchCode + " - " + s.BranchName
                MenuViewModel vmodel = new MenuViewModel();
                vmodel.Menu = db.AppModuless.OrderBy(a => a.MenuOrder).ToList();
                ViewBag.UserRole = vmodel;
                var DiscPerc = db.EnableSettings.Where(a => a.EnableType == "DiscountPercentage").FirstOrDefault();
                var Discper = DiscPerc != null ? (DiscPerc.Status == Status.active ? 0 : 1) : 1;
                ViewBag.Discount = Discper;

                return View();
            }
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Create User")]
        public async Task<ActionResult> Create(UserViewModel model)
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
                if (ModelState.IsValid)
                {
                    var UserId = User.Identity.GetUserId();
                   
                        long Branch = 0;

                        var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                        var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                        if (BranchCheck == Status.active)
                        {
                            Branch = Convert.ToInt64(model.Branch);
                        }
                        else
                        {
                            Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                        }
                        var user = new ApplicationUser
                        {
                            Name = model.Name,
                            PhoneNumber = model.PhoneNumber,
                            Email = model.Email,
                            Status = 1,
                            BranchID = Branch,
                            UserName = model.UserName,
                            BranchAccess = BranchAccess.Current,
                            Discount = model.Discount,
                        };
                        ApplicationUser userchk = await UserManager.FindByNameAsync(model.UserName);
                        if (userchk != null)
                        {
                            Warning("User Name already exists.", true);
                            return RedirectToAction("Create", "Users");
                        }
                        else
                        {
                            var result = await UserManager.CreateAsync(user, model.Password);
                            if (result.Succeeded)
                            {
                                UserId = user.Id;
                                if (model.Role != null)
                                {
                                    await this.UserManager.AddToRolesAsync(UserId, model.Role.Distinct().ToArray());
                                }
                                Success("User details added successfully.", true);
                            }
                            else
                            {
                                AddErrors(result);

                                ViewBag.error = result.Errors;
                                ViewBag.data = result;
                            }
                        }
                        return RedirectToAction("Index", "Users");
               
            }
                //    .Select(s => new
                //        BranchID = s.BranchID,
                //        BranchDetails = s.BranchCode + " - " + s.BranchName
                ViewBag.UserRole = db.AppModuless.ToList();
                return View(model);
            }
        }
        public string getshortcutmenu()
        {
            var UserId = User.Identity.GetUserId();
            var rem = db.LogManagers.Where(o => o.User == UserId && o.LogDetails == "shortcuts");
            string ret = "<li><a href='/Users/updateshortcut' style='color:black'>Create New Shortcut</a></li>";
            foreach(var t in rem)
            {
                ret =ret+ "<li><a href='"+t.LogTable+ "'  style='color:blue'>" + t.LogSection+"</a></li>";
            }
            return ret;
        }
        [HttpPost]
    
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> updateshortcut(UserViewModel model, string id)
        {
            var user = User.Identity.GetUserId();
            id = user;
            if (user == null)
            {
                return NotFound();
            }
           
                String UserId = id;
                
                      

                          
                                //remove
                                UserId = id;
            int i = 0;
                                if (model.Role != null)
                                {
                                    string[] dist = model.Role.Distinct().ToArray();
                var rem = db.LogManagers.Where(o => o.User == UserId && o.LogDetails == "shortcuts");
                db.LogManagers.RemoveRange(rem);
                db.SaveChanges();
                                    foreach(var r in dist)
                {
                    var da = db.AppModuless.Where(o => o.Name == r&&o.Link !="#").FirstOrDefault();
                    if (da != null)
                    {
                        com.addlog(LogTypes.Updated, UserId, r, da.Link, findip(), 1, "shortcuts");
                        i = i + 1;
                    }
                    if (i > 15)
                        break;
                }
                                }







            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public async Task<ActionResult> updateshortcut()
        {
            var id = User.Identity.GetUserId();
            var list = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                          }, "Value", "Text", 1);
            ViewBag.MC = list;
            ViewBag.UsrID = id;
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


            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = UserManager.FindByIdAsync(id).Result;
            if (user == null)
            {
                return NotFound();
            }
            UserViewModel vmodel = new UserViewModel();
            vmodel.Name = user.Name;
            vmodel.Email = user.Email;
            vmodel.PhoneNumber = user.PhoneNumber;
            vmodel.Branch = user.BranchID;
            vmodel.UserName = user.UserName;
            vmodel.Discount = user.Discount;
           var data = await UserManager.GetRolesAsync(user.Id);
            ViewBag.selectedRoles = (from a in data
                                     join c in db.LogManagers on a.ToString() equals c.LogSection
                                     where c.LogDetails == "shortcuts" && c.User == id
                                     select new
                                     {
                                         a
                                     }).ToList().Select(o => o.a).ToArray();

            //    .Select(s => new
            //        BranchID = s.BranchID,
            //        BranchDetails = s.BranchCode + " - " + s.BranchName

            MenuViewModel vmodel1 = new MenuViewModel();
            vmodel1.Menu =(from a in db.AppModuless
                           join b in data on a.Name.ToString() equals b.ToString()
                           where   a.addMenu==choice.Yes
                           select a).OrderBy(a => a.MenuOrder)
                           .ToList();
            ViewBag.UserRole = vmodel1;

            var DiscPerc = db.EnableSettings.Where(a => a.EnableType == "DiscountPercentage").FirstOrDefault();
            var Discper = DiscPerc != null ? (DiscPerc.Status == Status.active ? 0 : 1) : 1;
            ViewBag.Discount = Discper;
            var empid = db.Employees.Where(o => o.UserId == id).Select(o => o.EmployeeId).FirstOrDefault();
            if (empid != null)
            {


                var sup = db.SuperUsers.Where(o => o.emailid == vmodel.Email && o.employeeid == empid).Select(o => new
                {
                    o.emailid,
                    o.employeeid,
                    o.mcid,
                    o.purpose,
                    o.superuserid
                })
                   .ToList();

                var mcs = db.MCs.Where(o => !o.MCName.Contains("ssdssdfs")).Select(o => new
                {
                    id = o.MCId,
                    text = o.MCName
                }).ToList().ToArray();
                vmodel.purpose = sup.Select(o => o.purpose).ToList().ToArray();
                var mm = sup.Select(o => o.mcid).ToList().ToArray();

                ViewBag.MC = new MultiSelectList(mcs, "id", "text", mm);
                string[] ps = { "", "Sales", "override credit limit and credit period", "Purchase", "SalesReturn", "PurchaseReurn", "StockTransfer", "Payment", "Reciept", "Journal" };
                var pur = ps.Select(o => new
                {
                    id = o,
                    text = o,

                }).ToList();
                ViewBag.purposes = new MultiSelectList(pur, "id", "text", vmodel.purpose);

            }

            return View(vmodel);
        }

        [HttpGet]
        public async Task<ActionResult> removepermission()
        {
            var id = User.Identity.GetUserId();
            var list = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                          }, "Value", "Text", 1);
            ViewBag.MC = list;
            ViewBag.UsrID = id;
            var emp1 = db.Users.Where(o => o.Discount == null && o.Status == 1).Select(o => new { o.Id, o.Name }).ToList();
            ViewBag.emp1 = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
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


            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = UserManager.FindByIdAsync(id).Result;
            if (user == null)
            {
                return NotFound();
            }
            UserViewModel vmodel = new UserViewModel();
            vmodel.Name = user.Name;
            vmodel.Email = user.Email;
            vmodel.PhoneNumber = user.PhoneNumber;
            vmodel.Branch = user.BranchID;
            vmodel.UserName = user.UserName;
            vmodel.Discount = user.Discount;
            /* ViewBag.selectedRoles = (from a in data
                                      join c in db.LogManagers on a.ToString() equals c.LogSection
                                      where c.LogDetails == "shortcuts" && c.User == id
                                      select new
                                      {
                                          a
                                      }).ToList().Select(o => o.a).ToArray();*/

            //    .Select(s => new
            //        BranchID = s.BranchID,
            //        BranchDetails = s.BranchCode + " - " + s.BranchName

            MenuViewModel vmodel1 = new MenuViewModel();
            vmodel1.Menu = (from a in db.AppModuless
                           
                            select a).OrderBy(a => a.MenuOrder)
                           .ToList();
            ViewBag.UserRole = vmodel1;

            var DiscPerc = db.EnableSettings.Where(a => a.EnableType == "DiscountPercentage").FirstOrDefault();
            var Discper = DiscPerc != null ? (DiscPerc.Status == Status.active ? 0 : 1) : 1;
            ViewBag.Discount = Discper;


            return View(vmodel);
        }
        [HttpPost]

        [ValidateAntiForgeryToken]
        public async Task<ActionResult> removepermission(UserViewModel model, string id)
        {
            var user = User.Identity.GetUserId();
            id = user;
            if (user == null)
            {
                return NotFound();
            }

            String UserId = id;




            //remove
            UserId = id;
            int i = 0;
            var selecredallemployees = (model.mcid.Contains(0)) ? true : false;

            var users = (from a in db.Users
                         join b in db.Employees on a.Id equals b.UserId
                         where (selecredallemployees == true || model.mcid.Contains(b.EmployeeId))
                         && a.Status == 1
                         select new
                         {
                             a.Id
                         }).Select(o => o.Id).ToList().ToArray();


            if (model.Role != null)
            {
                string[] dist = model.Role.Distinct().ToArray();
               


                if (model.Role != null)
                {
                    var notparentroles = (from a in db.AppModuless
                                          join b in model.Role on a.Name equals b
                                          where a.Link != "#" && a.Link != null && a.Link != ""
                                          select new
                                          {
                                              a.Name
                                          }
                                   ).Select(o => o.Name).ToList().ToArray();

                    foreach (var uid in users)
                    {

                        await this.UserManager.RemoveFromRolesAsync(uid, notparentroles.ToArray());
                    }
                }



            }

            Success("Updated successfully.", true);






            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public async Task<ActionResult> assignpermission()
        {
            var id = User.Identity.GetUserId();
            var list = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                          }, "Value", "Text", 1);
            ViewBag.MC = list;
            ViewBag.UsrID = id;
            var emp1 = db.Users.Where(o => o.Discount == null && o.Status == 1).Select(o => new { o.Id, o.Name }).ToList();
            ViewBag.emp1 = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
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


            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = UserManager.FindByIdAsync(id).Result;
            if (user == null)
            {
                return NotFound();
            }
            UserViewModel vmodel = new UserViewModel();
            vmodel.Name = user.Name;
            vmodel.Email = user.Email;
            vmodel.PhoneNumber = user.PhoneNumber;
            vmodel.Branch = user.BranchID;
            vmodel.UserName = user.UserName;
            vmodel.Discount = user.Discount;
           /* ViewBag.selectedRoles = (from a in data
                                     join c in db.LogManagers on a.ToString() equals c.LogSection
                                     where c.LogDetails == "shortcuts" && c.User == id
                                     select new
                                     {
                                         a
                                     }).ToList().Select(o => o.a).ToArray();*/

            //    .Select(s => new
            //        BranchID = s.BranchID,
            //        BranchDetails = s.BranchCode + " - " + s.BranchName

            MenuViewModel vmodel1 = new MenuViewModel();
            vmodel1.Menu = (from a in db.AppModuless
                            
                            select a).OrderBy(a => a.MenuOrder)
                           .ToList();
            ViewBag.UserRole = vmodel1;

            var DiscPerc = db.EnableSettings.Where(a => a.EnableType == "DiscountPercentage").FirstOrDefault();
            var Discper = DiscPerc != null ? (DiscPerc.Status == Status.active ? 0 : 1) : 1;
            ViewBag.Discount = Discper;
           

            return View(vmodel);
        }
        [HttpPost]

        [ValidateAntiForgeryToken]
        public async Task<ActionResult> assignpermission(UserViewModel model, string id)
        {
            var user = User.Identity.GetUserId();
            id = user;
            if (user == null)
            {
                return NotFound();
            }

            String UserId = id;




            //remove
            UserId = id;
            int i = 0;
            var selecredallemployees = (model.mcid.Contains(0))?true:false;

            var users = (from a in db.Users
                         join b in db.Employees on a.Id equals b.UserId
                         where (selecredallemployees==true|| model.mcid.Contains(b.EmployeeId))
                         && a.Status==1
                         select new
                         {
                             a.Id
                         }).Select(o => o.Id).ToList().ToArray();


            if (model.Role != null)
            {
                string[] dist = model.Role.Distinct().ToArray();

                  

                    if (model.Role != null)
                    {

                        foreach (var uid in users)
                        {

                        var roles = await UserManager.GetRolesAsync(uid);
                        await UserManager.RemoveFromRolesAsync(uid, roles.ToArray());
                        var newroles = roles.Union(dist).Distinct();
                        await UserManager.AddToRolesAsync(uid, newroles.ToArray());
                        }
                    }
                    


                }

                Success("Updated successfully.", true);
            





            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Edit User")]
        public async Task<ActionResult> Edit(string id)
        {
            var list = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                          }, "Value", "Text", 1);
            ViewBag.MC = list;
            ViewBag.UsrID = id;
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


            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = UserManager.FindByIdAsync(id).Result;
            if (user == null)
            {
                return NotFound();
            }
            UserViewModel vmodel = new UserViewModel();
            vmodel.Name = user.Name;
            vmodel.Email = user.Email;
            vmodel.PhoneNumber = user.PhoneNumber;
            vmodel.Branch = user.BranchID;
            vmodel.UserName = user.UserName;
            vmodel.Discount = user.Discount;
            IList<string> data = await UserManager.GetRolesAsync(user.Id);
            ViewBag.selectedRoles = data;

            //    .Select(s => new
            //        BranchID = s.BranchID,
            //        BranchDetails = s.BranchCode + " - " + s.BranchName

            MenuViewModel vmodel1 = new MenuViewModel();
            vmodel1.Menu = db.AppModuless.OrderBy(a => a.MenuOrder).ToList();
            ViewBag.UserRole = vmodel1;
            
            var DiscPerc = db.EnableSettings.Where(a => a.EnableType == "DiscountPercentage").FirstOrDefault();
            var Discper = DiscPerc != null ? (DiscPerc.Status == Status.active ? 0 : 1) : 1;
            ViewBag.Discount = Discper;
            var empid = db.Employees.Where(o => o.UserId == id).Select(o => o.EmployeeId).FirstOrDefault();
            if (empid != null)
            {


                var sup = db.SuperUsers.Where(o => o.emailid == vmodel.Email && o.employeeid == empid).Select(o=>new
                {
                o.emailid ,
                o.employeeid,
                o.mcid,
                o.purpose,
                o.superuserid 
                })
                   .ToList();
             
                    var mcs = db.MCs.Where(o=>!o.MCName.Contains("ssdssdfs")).Select(o => new
                    {
                        id=o.MCId,
                        text=o.MCName
                    }).ToList().ToArray();
                    vmodel.purpose = sup.Select(o=>o.purpose).ToList().ToArray();
                    var mm = sup.Select(o => o.mcid).ToList().ToArray();
                    
                    ViewBag.MC = new MultiSelectList(mcs, "id", "text",mm);
                    string[] ps = {"", "Sales","override credit limit and credit period", "Purchase", "SalesReturn", "PurchaseReurn", "StockTransfer", "Payment", "Reciept", "Journal" };
                    var pur = ps.Select(o => new
                    {
                        id = o,
                        text = o,

                    }).ToList();
                        ViewBag.purposes = new MultiSelectList(pur, "id","text", vmodel.purpose);
                
            }  

            return View(vmodel);
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit User")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UserViewModel model, string id)
        {
            var user = UserManager.FindByIdAsync(id).Result;
            if (user == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                String UserId = id;
                var Exists = db.Users.Any(c => c.Email == model.Email && c.Id != id);
                if (Exists)
                {
                    Warning("Email already exists.", true);
                    return RedirectToAction("Edit", "Users");
                }
                else
                {
                    long Branch = 0;

                    var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                    var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                    if (BranchCheck == Status.active)
                    {
                        Branch = Convert.ToInt64(model.Branch);
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    }

                    user.Name = model.Name;
                    user.Email = model.Email;
                    user.PhoneNumber = model.PhoneNumber;
                    user.BranchID = Branch;
                    user.UserName = model.UserName;
                    user.BranchAccess = BranchAccess.Current;
                    user.Discount = model.Discount;
                    var userExists = db.Users.Any(c => c.UserName == model.UserName && c.Id != id);
                    if (userExists)
                    {
                        Warning("User Name Already exists.", true);
                        return RedirectToAction("Edit", "Users");
                    }
                    else
                    {
                        var result = await UserManager.UpdateAsync(user);
                        if (result.Succeeded)
                        {

                            if (model.Password != null)
                            {
                                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                                var results = await UserManager.ResetPasswordAsync(user.Id, code, model.Password);
                            }
                            if (model.Role != null)
                            {
                                //remove
                                UserId = user.Id;
                                var roles = await UserManager.GetRolesAsync(UserId);
                                await UserManager.RemoveFromRolesAsync(UserId, roles.ToArray());
                                //add 
                                var x = (from c in model.Role
                                         group c by c into grp
                                         where grp.Count() > 1
                                         select grp.Key).ToList();

                                if (model.Role != null)
                                {
                                    string[] dist = model.Role.Distinct().ToArray();

                                    Debug.WriteLine(string.Join("\n", dist));
                                    await this.UserManager.AddToRolesAsync(UserId, dist);
                                }
                            }
                            Success("User details Updated successfully.", true);
                        }

                        var empid = db.Employees.Where(o => o.UserId == UserId).Select(o => o.EmployeeId).FirstOrDefault();
                        if (model.purpose != null && model.mcid != null)
                        {
                            if (model.purpose.Count() > 0 && model.mcid.Count() > 0)
                            {
                                db.SuperUsers.RemoveRange(db.SuperUsers.Where(o => o.employeeid == empid));
                                
                                db.SaveChanges();
                                foreach (var mm in model.mcid)
                                {
                                    foreach (var pr in model.purpose)
                                    {
                                        SuperUser c = new SuperUser
                                        {
                                            emailid = model.Email,
                                            employeeid = empid,
                                            mcid = (long)mm,
                                            purpose = pr,


                                        };
                                        db.SuperUsers.Add(c);
                                        db.SaveChanges();
                                    }
                                }

                            }
                        }
                            return RedirectToAction("Index", "Users");
                    }
                }
            }
            UserViewModel vmodel = new UserViewModel();
            vmodel.Email = user.Email;
            vmodel.PhoneNumber = user.PhoneNumber;
            vmodel.UserName = user.UserName;
            var userRoles = await UserManager.GetRolesAsync(user.Id);

            //    .Select(s => new
            //        BranchID = s.BranchID,
            //        BranchDetails = s.BranchCode + " - " + s.BranchName
            ViewBag.UserRole = db.AppModuless.Select(x => new
            {
                Selected = userRoles.Contains(x.Name),
                x.Name,
                x.Id,
                x.Parent,
                x.IsParent,
                x.Status,
                x.viewName,
                x.ModulesID,
                x.Editable
            }).ToList();
            return View(model);
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Edit User")]
        public async Task<ActionResult> Ban(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return PartialView();
        }

        [HttpPost, ActionName("Ban")]
        [QkAuthorize(Roles = "Dev,Edit User")]
        public async Task<JsonResult> BanAction(string id)
        {
            bool stat = false;
            string msg = "";
            var user = UserManager.FindByIdAsync(id).Result;
            if (user == null)
            {
                msg = "Sorry, There is an Error ";
                stat = false;
            }
            if (ModelState.IsValid)
            {
                user.Status = 0;
                var result = await UserManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    stat = true;
                    msg = "Successfully Banned User.";
                }
                else
                {
                    msg = "Unable to Ban User.";
                    stat = false;
                }
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Edit User")]
        public async Task<ActionResult> UnBan(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return PartialView();
        }

        [HttpPost, ActionName("UnBan")]
        [QkAuthorize(Roles = "Dev,Edit User")]
        public async Task<JsonResult> UnBanAction(string id)
        {
            bool stat = false;
            string msg = "";
            var user = UserManager.FindByIdAsync(id).Result;
            if (user == null)
            {
                msg = "Sorry, There is an Error ";
                stat = false;
            }
            if (ModelState.IsValid)
            {
                user.Status = 1;
                var result = await UserManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    stat = true;
                    msg = "Successfully Unbanned User.";
                }
                else
                {
                    msg = "Unable to Unban User.";
                    stat = false;
                }
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Delete User")]
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return PartialView(user);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete User")]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            bool stat = false;
            string msg = "";
            //------condition------
            var Msg = chkDeleteWithMsg(id);
            if (Msg != null)
            {
                msg = Msg;
                stat = false;
            }
            else
            {
                var user = await UserManager.FindByIdAsync(id);
                var result = await UserManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    stat = true;
                    msg = "Successfully deleted user details.";
                }
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpGet]
        public JsonResult EmailCheck(string Email, long? cusid)
        {
            var Exists = db.Users.Where(c => c.Email == Email).Any();
            
            
            
            var rslt = (Exists) ? true : false;
            return Json(rslt);
        }
        [HttpGet]
        public JsonResult EmailCheck2(string Email, string usid)
        {
            var Exists = db.Users.Where(c => c.Email == Email && c.Id!=usid).Any();



            var rslt = (Exists) ? true : false;
            return Json(rslt);
        }


        public string chkDeleteWithMsg(string id)
        {
            string msg = null;
            if (db.Items.Any(c => c.CreatedUserID == id))
            {
                msg = "Unable to delete User, Already used in Item !!";
            }
            else if (db.SalesEntrys.Any(c => c.CreatedBy == id))
            {
                msg = "Unable to delete User, Already used in Sales Entry !!";
            }
            else if (db.SalesReturns.Any(c => c.CreatedBy == id))
            {
                msg = "Unable to delete User, Already used in Sales Return !!";
            }
            else if (db.PurchaseEntrys.Any(c => c.CreatedBy == id))
            {
                msg = "Unable to delete User, Already used in Purchase Entry !!";
            }
            else if (db.PurchaseReturns.Any(c => c.CreatedBy == id))
            {
                msg = "Unable to delete User, Already used in Purchase Return !!";
            }
            else if (db.Quotations.Any(c => c.CreatedUserId == id))
            {
                msg = "Unable to delete User, Already used in Quotation !!";
            }
            else if (db.ProFormas.Any(c => c.CreatedBy == id))
            {
                msg = "Unable to delete User, Already used in ProForma !!";
            }
            else if (db.Deliverynotes.Any(c => c.CreatedUserId == id))
            {
                msg = "Unable to delete User, Already used in Deliverynote !!";
            }
            else if (db.Journals.Any(c => c.CreatedBy == id))
            {
                msg = "Unable to delete User, Already used in Journals !!";
            }
            else if (db.Payments.Any(c => c.CreatedBy == id))
            {
                msg = "Unable to delete User, Already used in Payments !!";
            }
            else if (db.Receipts.Any(c => c.CreatedBy == id))
            {
                msg = "Unable to delete User, Already used in Receipts !!";
            }
            else if (db.Productions.Any(c => c.CreatedBy == id))
            {
                msg = "Unable to delete User, Already used in Production !!";
            }
            else if (db.Unassembles.Any(c => c.CreatedBy == id))
            {
                msg = "Unable to delete User, Already used in Unassemble !!";
            }
            else if (db.PurchaseOrders.Any(c => c.CreatedUserId == id))
            {
                msg = "Unable to delete User, Already used in Purchase Order !!";
            }
            else if (db.SalesOrders.Any(c => c.CreatedUserId == id))
            {
                msg = "Unable to delete User, Already used in Sales Order !!";
            }
            else if (db.ContraVouchers.Any(c => c.CreatedBy == id))
            {
                msg = "Unable to delete User, Already used in Contra Voucher !!";
            }
            else if (db.StockJournals.Any(c => c.CreatedBy == id))
            {
                msg = "Unable to delete User, Already used in Stock Journal !!";
            }
            else if (db.StockTransfers.Any(c => c.CreatedBy == id))
            {
                msg = "Unable to delete User, Already used in Stock Transfer !!";
            }
            else if (db.PackingLists.Any(c => c.CreatedBy == id))
            {
                msg = "Unable to delete User, Already used in Packing List !!";
            }
            else if (db.PurchaseQuotations.Any(c => c.CreatedUserId == id))
            {
                msg = "Unable to delete User, Already used in Purchase Quotation !!";
            }
            else if (db.Projects.Any(c => c.CreatedBy == id))
            {
                msg = "Unable to delete User, Already used in Projects !!";
            }
            else if (db.ProTasks.Any(c => c.CreatedBy == id))
            {
                msg = "Unable to delete User, Already used in Tasks !!";
            }
            else
            {
                msg = null;
            }
            return msg;
        }


        private const string XsrfKey = "XsrfId";
        internal class ChallengeResult : UnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ActionContext context)
            {
                // External-login challenge deferred (OWIN Authentication.Challenge -> Core HttpContext.ChallengeAsync).
            }
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
