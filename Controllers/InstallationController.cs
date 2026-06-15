using System.Linq.Dynamic.Core;
using ApplicationUserManager = Microsoft.AspNetCore.Identity.UserManager<QuickSoft.Models.ApplicationUser>;
using ApplicationSignInManager = Microsoft.AspNetCore.Identity.SignInManager<QuickSoft.Models.ApplicationUser>;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Data;
using System.Globalization;
using Microsoft.AspNetCore.Identity;

namespace QuickSoft.Controllers
{
    [AllowAnonymous]
    public class InstallationController : Controller
    {
        ApplicationDbContext db;
        public InstallationController()
        {
            db = new ApplicationDbContext();

        }
        private ApplicationUserManager _userManager;
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
        public async Task<JsonResult> Addmenu()
        {
            bool stat = false;
            string msg;



            //SqlConnection myConn = new SqlConnection("Data Source=.\\SQLExpress;Initial Catalog=master;Integrated Security=SSPI");
            //                BEGIN
            //                    CREATE LOGIN [IIS APPPOOL\DefaultAppPool] 
            //                      FROM WINDOWS WITH DEFAULT_DATABASE=[master], 
            //                      DEFAULT_LANGUAGE=[us_english]
            //                END
            //                GO
            //                CREATE USER [QuickERP] 
            //                  FOR LOGIN [IIS APPPOOL\DefaultAppPool]
            //                GO
            //                EXEC sp_addrolemember 'db_owner', 'QuickERP'






            //catch








            var UserId = User.Identity.GetUserId();              //disable price updation and Stockable Items in Sales Entry
            string[] useRole = db.AppModuless.Where(x => x.ModulesID != 384 || x.ModulesID != 415).Select(a => a.Name).ToArray();
            await this.UserManager.AddToRolesAsync(UserId, useRole);

            msg = "Looks like something went wrong. Please check your form.";
            stat = false;

            return this.Json("you result");

        }
        public ActionResult Index()
        {

            // SqlConnection myConn = new SqlConnection("Data Source=.\\SQLExpress;Initial Catalog=master;Integrated Security=SSPI");
            //// var query = @"IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Accounts') SELECT 1 ELSE SELECT 0";

            //notworking show db not found error
            //SqlConnection myConn = new SqlConnection("Data Source=.\\SQLExpress;Initial Catalog=master;Integrated Security=SSPI");

            var qry = @"IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Accounts') SELECT 1 ELSE SELECT 0";
            int rslt = db.Database.SqlQueryRaw<int>(qry).AsEnumerable().FirstOrDefault();
            if (rslt == 1)
            {
                var exist = db.SystemConfigs.Any();
                if (exist)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Error", "Home");
                }
            }
            SysValidate st = new SysValidate();
            InstallationViewModel vmodel = new InstallationViewModel();
            vmodel.StartDate = System.DateTime.Now.ToString("dd-MM-yyyy");
            vmodel.SystemCode = Security.kEYgEN();
            return View(vmodel);
        }
        [HttpPost]
        public async Task<JsonResult> Index(InstallationViewModel vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {


                //SqlConnection myConn = new SqlConnection("Data Source=.\\SQLExpress;Initial Catalog=master;Integrated Security=SSPI");
                //                BEGIN
                //                    CREATE LOGIN [IIS APPPOOL\DefaultAppPool] 
                //                      FROM WINDOWS WITH DEFAULT_DATABASE=[master], 
                //                      DEFAULT_LANGUAGE=[us_english]
                //                END
                //                GO
                //                CREATE USER [QuickERP] 
                //                  FOR LOGIN [IIS APPPOOL\DefaultAppPool]
                //                GO
                //                EXEC sp_addrolemember 'db_owner', 'QuickERP'



                string text1 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\table.txt"));
                var result1 = db.Database.ExecuteSqlRaw(text1);

                //string text12 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\table2.txt"));
                string texttable22 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\data.txt"));
                var result2 = db.Database.ExecuteSqlRaw(texttable22);


                string text3 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_allchildGroups.txt"));
                var result3 = db.Database.ExecuteSqlRaw(text3);

                string text4 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_allparentGroups.txt"));
                var result4 = db.Database.ExecuteSqlRaw(text4);

                string text5 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_BackUpAndRestore.txt"));
                var result5 = db.Database.ExecuteSqlRaw(text5);

                string text6 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertDvItems.txt"));
                var result6 = db.Database.ExecuteSqlRaw(text6);

                string text7 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertPEBillSundry.txt"));
                var result7 = db.Database.ExecuteSqlRaw(text7);

                string text8 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertPEItems.txt"));
                var result8 = db.Database.ExecuteSqlRaw(text8);

                string text9 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertPFBillSundry.txt"));
                var result9 = db.Database.ExecuteSqlRaw(text9);

                string text10 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertPFItems.txt"));
                var result10 = db.Database.ExecuteSqlRaw(text10);

                string text11 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertPRBillSundry.txt"));
                var result11 = db.Database.ExecuteSqlRaw(text11);

                string texttable12 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertPRItems.txt"));
                var resulttable12 = db.Database.ExecuteSqlRaw(texttable12);

                //SP_InsertPRItemsNote.txt
                string texttable12note = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertPRItemsNote.txt"));
                var resulttable12note = db.Database.ExecuteSqlRaw(texttable12note);

                string text13 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertQuotationItems.txt"));
                var result13 = db.Database.ExecuteSqlRaw(text13);

                string text14 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertSEBillSundry.txt"));
                var result14 = db.Database.ExecuteSqlRaw(text14);

                string text15 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertSEItems.txt"));
                var result15 = db.Database.ExecuteSqlRaw(text15);

                string text16 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertSRBillSundry.txt"));
                var result16 = db.Database.ExecuteSqlRaw(text16);

                string text17 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertSRItems.txt"));
                var result17 = db.Database.ExecuteSqlRaw(text17);
                string text17note = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertSRNoteItems.txt"));
                var result17note = db.Database.ExecuteSqlRaw(text17note);
                string text18 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertPOrderItems.txt"));
                var result18 = db.Database.ExecuteSqlRaw(text18);

                string text19 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertSalesOrderItems.txt"));
                var result19 = db.Database.ExecuteSqlRaw(text19);

                string text20 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\balancesheet.txt"));
                var result20 = db.Database.ExecuteSqlRaw(text20);

                string text21 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\trialbalance.txt"));
                var result21 = db.Database.ExecuteSqlRaw(text21);

                string text22 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\balancesheet2.txt"));
                var result22 = db.Database.ExecuteSqlRaw(text22);

                string text23 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertPLItems.txt"));
                var result23 = db.Database.ExecuteSqlRaw(text23);

                string text24 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertPurchaseQuotItems.txt"));
                var result24 = db.Database.ExecuteSqlRaw(text24);

                string text25 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertMRNoteItems.txt"));
                var result25 = db.Database.ExecuteSqlRaw(text25);

                string text26 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertMRItems.txt"));
                var result26 = db.Database.ExecuteSqlRaw(text26);
                string text32 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertPOSOrderItems.txt"));
                var result32 = db.Database.ExecuteSqlRaw(text32);



                string text27 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_AVCOMethod.txt"));
                var result27 = db.Database.ExecuteSqlRaw(text27);
                string text277 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertDummyPeItems.txt"));
                var result277 = db.Database.ExecuteSqlRaw(text277);

                string text28 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertCrossHrItems.txt"));
                var result28 = db.Database.ExecuteSqlRaw(text28);

                string text29 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertHrItems.txt"));
                var result29 = db.Database.ExecuteSqlRaw(text29);

                string text30 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_InsertSTItems.txt"));
                var result30 = db.Database.ExecuteSqlRaw(text30);

                string text31 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\property.txt"));
                var result31 = db.Database.ExecuteSqlRaw(text31);
                string text33 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_AVCOMethod2.txt"));
                var result33 = db.Database.ExecuteSqlRaw(text33);
                string text333 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_AVCOMethod3.txt"));
                var result333 = db.Database.ExecuteSqlRaw(text333);
                string text3333 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_AVCOMethod4.txt"));
                var result3333 = db.Database.ExecuteSqlRaw(text3333);

                string text44 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_AVCOMethod5.txt"));
                var result44 = db.Database.ExecuteSqlRaw(text44);

                string text55 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_AVCOMethod6.txt"));
                var result55 = db.Database.ExecuteSqlRaw(text55);

                string text585 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_AVCOMethodcasa.txtq6 "));
                var result585 = db.Database.ExecuteSqlRaw(text585);
                string text34 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_ITEMSEARCH.txt"));
                var result34 = db.Database.ExecuteSqlRaw(text34);
                string text35 = System.IO.File.ReadAllText(LegacyWeb.MapPath("~\\dbscript\\SP_ITEMSEARCH2.txt"));
                var result35 = db.Database.ExecuteSqlRaw(text35);
                //-----------delete directory--------
                string path = LegacyWeb.MapPath("~\\dbscript\\");
                foreach (string filename in Directory.GetFiles(path))
                {
                }
               /// -----------------------------------



                String format = "dd-MM-yyyy";
                SystemConfig sysconfig = new SystemConfig();
                CultureInfo culture = CultureInfo.InvariantCulture;
                DateTime startDate = DateTime.ParseExact(vmodel.StartDate, "dd-MM-yyyy", culture);



                try
                {
                    startDate = Convert.ToDateTime(vmodel.StartDate, new CultureInfo("en-GB"));
                }
                catch
                {
                    startDate = Convert.ToDateTime(vmodel.StartDate);
                }

                var insdate = Security.Encrypt(startDate.ToString(), General.keyval);
                sysconfig.StartDate = Convert.ToString(insdate);
                sysconfig.sld = Convert.ToString(insdate);

                //Security.Encrypt(Convert.ToString(vmodel.StartDate), General.keyval);
                sysconfig.SystemTypes = Security.Encrypt(Convert.ToString(vmodel.SystemTypes), General.keyval);

                if (vmodel.SystemTypes == SystemType.Demo)
                {
                    Int32 period = Convert.ToInt32(vmodel.DemoPeriod);
                    sysconfig.Extentdays = Security.Encrypt(Convert.ToString(period), General.keyval);

                    DateTime addays = startDate.AddDays(period);
                    sysconfig.EndDate = Security.Encrypt(Convert.ToString(addays), General.keyval);
                }
                else
                {
                    sysconfig.SystemCode = vmodel.SystemCode;
                    sysconfig.LicenseKey = vmodel.LicenseKey;
                }

                //no of users
                sysconfig.NumberOfUsers = Security.Encrypt(Convert.ToString(vmodel.NumberOfUsers), General.keyval);


                db.SystemConfigs.Add(sysconfig);
                db.SaveChanges();

                //system versions
                AppVersion appversion = new AppVersion();
                appversion.InstallDate = Convert.ToDateTime(System.DateTime.Now);
                appversion.Versions = "1.2";
                db.AppVersions.Add(appversion);
                db.SaveChanges();


                //company
                Company company = new Company();
                company.CPName = vmodel.CPName;
                company.TRN = vmodel.TRN;
                company.CPEmail = vmodel.CPEmail;
                company.CPPhone = vmodel.CPPhone;
                company.CPMobile = vmodel.CPMobile;
                company.CPAddress = vmodel.CPAddress;


                if (vmodel.CPLogo != null)
                {
                    // files upload
                    IFormFile file = Request.Form.Files[0];
                    var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                    var uploadUrl = LegacyWeb.MapPath("~/uploads/company/");
                    file.SaveAs(Path.Combine(uploadUrl, fileName));
                    company.CPLogo = fileName;
                }
                else
                {
                    company.CPLogo = null;
                }
                company.SMTPEmail = vmodel.SMTPEmail;
                company.SMTPHost = vmodel.SMTPHost;
                company.SMTPUsername = vmodel.SMTPUsername;
                company.SMTPPassword = vmodel.SMTPPassword;
                company.SMTPPort = vmodel.SMTPPort;
                company.EnableSsl = vmodel.EnableSsl;

                company.SaleAccount = 1;
                company.PurchaseAccount = 2;
                company.SReturnAccount = 1;
                company.PReturnAccount = 2;


                db.companys.Add(company);
                db.SaveChanges();
                Int64 companyId = company.CompanyID;

                //FDinancial Year
                if (vmodel.FinStartDate != null && vmodel.FinStartDate != "")
                {
                    DateTime NewStartDate = DateTime.ParseExact(vmodel.FinStartDate, "dd-MM-yyyy", culture);
                    FinancialYear FY = new FinancialYear();
                    FY.Company = companyId;
                    FY.Start = NewStartDate;
                    FY.Status = Status.active;
                    FY.Active = choice.Yes;
                    FY.End = FY.Start.Value.AddYears(1).AddDays(-1);
                    db.FinancialYears.Add(FY);
                    db.SaveChanges();
                }

                //branch
                Branch brnch = new Branch();
                brnch.BranchCode = "BR1";
                brnch.BranchName = "Branch 1";
                db.Branchs.Add(brnch);
                db.SaveChanges();


                //user
                String UserId = "";
                var user = new ApplicationUser
                {
                    Email = vmodel.Email,
                    Status = 1,
                    BranchID = db.Branchs.Select(a => a.BranchID).FirstOrDefault(),
                    UserName = vmodel.UserName,
                    BranchAccess = BranchAccess.Current,
                };
                var result = await UserManager.CreateAsync(user, vmodel.Password);
                if (result.Succeeded)
                {
                    UserId = user.Id;               //disable price updation and Stockable Items in Sales Entry
                    string[] useRole = db.AppModuless.Where(x => x.ModulesID != 384 || x.ModulesID != 415).Select(a => a.Name).ToArray();
                    await this.UserManager.AddToRolesAsync(UserId, useRole);
                }


                msg = "Success";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }
        public ActionResult Redirect()
        {
            //var datenew = Security.Decrypt(date, General.keyval);
            //var enddatenew = Security.Decrypt(enddate, General.keyval);
            //var systypenew = Security.Decrypt(systype, General.keyval);




            //var daysnew = Security.Decrypt(days, General.keyval);


            return View();
        }

        private DateTime FinEndDate(DateTime Start)
        {
            var End = Start.AddMonths(12).AddDays(-1);

            return End;
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
