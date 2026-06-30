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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class CompanyController : BaseController
    {
        // GET: Company
        ApplicationDbContext db;
        Common com;
        public CompanyController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Company Edit")]
        public ActionResult Edit()
        {
            Company com = db.companys.Find(1L);
            CompanyViewModel vmodel = new CompanyViewModel
            {
                Company = com,
                DbBackUpPath = com.DbBackUpPath,
                Customer = db.CodePrefixs.Where(a => a.section == "Customer").Select(a => a.prefix).FirstOrDefault(),
                Supplier = db.CodePrefixs.Where(a => a.section == "Supplier").Select(a => a.prefix).FirstOrDefault(),
                Item = db.CodePrefixs.Where(a => a.section == "Item").Select(a => a.prefix).FirstOrDefault(),
                Branch = db.CodePrefixs.Where(a => a.section == "Branch").Select(a => a.prefix).FirstOrDefault(),
                Deliverynote = db.CodePrefixs.Where(a => a.section == "Deliverynote").Select(a => a.prefix).FirstOrDefault(),
                Reciept = db.CodePrefixs.Where(a => a.section == "Reciept").Select(a => a.prefix).FirstOrDefault(),
                Payment = db.CodePrefixs.Where(a => a.section == "Payment").Select(a => a.prefix).FirstOrDefault(),

                PReturn = db.CodePrefixs.Where(a => a.section == "PurchaseReturn").Select(a => a.prefix).FirstOrDefault(),
                SReturn = db.CodePrefixs.Where(a => a.section == "SalsReturn").Select(a => a.prefix).FirstOrDefault(),
                Employee = db.CodePrefixs.Where(a => a.section == "Employee").Select(a => a.prefix).FirstOrDefault(),
                InvoicePrefix = db.CodePrefixs.Where(a => a.section == "Invoice").Select(a => a.prefix).FirstOrDefault(),
                ProFormaPrefix = db.CodePrefixs.Where(a => a.section == "ProForma").Select(a => a.prefix).FirstOrDefault(),
                PurchasePrefix = db.CodePrefixs.Where(a => a.section == "Purchase").Select(a => a.prefix).FirstOrDefault(),
                QuotationPrefix = db.CodePrefixs.Where(a => a.section == "Quotation").Select(a => a.prefix).FirstOrDefault(),
                POrder = db.CodePrefixs.Where(a => a.section == "PurchaseOrder").Select(a => a.prefix).FirstOrDefault(),


                HireInvoicePrefix = db.CodePrefixs.Where(a => a.section == "HireInvoice").Select(a => a.prefix).FirstOrDefault(),
                HireSalesOrderPrefix = db.CodePrefixs.Where(a => a.section == "HireSalesOrder").Select(a => a.prefix).FirstOrDefault(),
                HireProformaPrefix = db.CodePrefixs.Where(a => a.section == "HireQuotation").Select(a => a.prefix).FirstOrDefault(),
                HireQuotationPrefix = db.CodePrefixs.Where(a => a.section == "HireProforma").Select(a => a.prefix).FirstOrDefault(),
                HireDelivernotePrefix = db.CodePrefixs.Where(a => a.section == "HireDelivernote").Select(a => a.prefix).FirstOrDefault(),
                HireReturn = db.CodePrefixs.Where(a => a.section == "HireReturn").Select(a => a.prefix).FirstOrDefault(),
                CrossHireInvoicePrefix = db.CodePrefixs.Where(a => a.section == "CrossHireInvoice").Select(a => a.prefix).FirstOrDefault(),
                TaxExempt = db.CodePrefixs.Where(o => o.section == "TaxExempt").Select(a => a.prefix).FirstOrDefault(),
                Packinglist = db.CodePrefixs.Where(a => a.section == "Packinglist").Select(a => a.prefix).FirstOrDefault(),

                Production= db.CodePrefixs.Where(a => a.section == "Production").Select(a => a.prefix).FirstOrDefault(),
                Unassemble = db.CodePrefixs.Where(a => a.section == "Unassemble").Select(a => a.prefix).FirstOrDefault(),
                MR = db.CodePrefixs.Where(a => a.section == "MaterialRequisition").Select(a => a.prefix).FirstOrDefault(),
                MRNote = db.CodePrefixs.Where(a => a.section == "MRNote").Select(a => a.prefix).FirstOrDefault(),
                PQuotation = db.CodePrefixs.Where(a => a.section == "PurchaseQuotation").Select(a => a.prefix).FirstOrDefault(),
                Journal = db.CodePrefixs.Where(a => a.section == "Journal").Select(a => a.prefix).FirstOrDefault(),
                JobCard = db.CodePrefixs.Where(a => a.section == "JobCard").Select(a => a.prefix).FirstOrDefault(),
                MC = db.CodePrefixs.Where(a => a.section == "MC").Select(a => a.prefix).FirstOrDefault(),
                Project = db.CodePrefixs.Where(a => a.section == "Project").Select(a => a.prefix).FirstOrDefault(),
                Task = db.CodePrefixs.Where(a => a.section == "Task").Select(a => a.prefix).FirstOrDefault(),
                SalesOrder = db.CodePrefixs.Where(a => a.section == "SalesOrder").Select(a => a.prefix).FirstOrDefault(),
                StockAdjustment = db.CodePrefixs.Where(a => a.section == "StockAdjustment").Select(a => a.prefix).FirstOrDefault(),
                StockJournal = db.CodePrefixs.Where(a => a.section == "StockJournal").Select(a => a.prefix).FirstOrDefault(),
                StockTransfer = db.CodePrefixs.Where(a => a.section == "StockTransfer").Select(a => a.prefix).FirstOrDefault(),
                StockVerification = db.CodePrefixs.Where(a => a.section == "StockVerification").Select(a => a.prefix).FirstOrDefault(),


                Header = db.CompanyHeaders.Where(a => a.CompanyHeaderID == 1).Select(a => a.Header).FirstOrDefault(),
                Footer = db.CompanyHeaders.Where(a => a.CompanyHeaderID == 1).Select(a => a.Footer).FirstOrDefault()

            };
            ViewBag.SystemCode = db.SystemConfigs.Select(a => a.SystemCode).FirstOrDefault();

            var Bran = db.Branchs.Where(s => s.Status == Status.active)
                .Select(s => new
                {
                    ID = s.BranchID,
                    Name = s.BranchName
                }).ToList();
            ViewBag.Branch = QkSelect.List(Bran, "ID", "Name");

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var sacc = db.Accountss.Where(a=> a.Group != 12 && a.Group != 14 && a.Group != 8).Select(a => new
            {
                ID = a.AccountsID,
                Name = a.Name
            }).ToList();
            if (com.SaleAccount != 1)
            {
                ViewBag.SaleAcc = QkSelect.List(sacc, "ID", "Name", com.SaleAccount);
            }
            else
            {
                ViewBag.SaleAcc = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = true, Text = "Sale", Value = "1"},
                                }, "Value", "Text", 0);
            }
            if (com.PurchaseAccount != 2)
            {
                ViewBag.PurAcc = QkSelect.List(sacc, "ID", "Name", com.PurchaseAccount);
            }
            else
            {
                ViewBag.PurAcc = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "Purchase", Value = "2"},
                           }, "Value", "Text", 0);
            }

            if (com.SReturnAccount != 1)
            {
                ViewBag.SRetAcc = QkSelect.List(sacc, "ID", "Name", com.SReturnAccount);
            }
            else
            {
                ViewBag.SRetAcc = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "Sale", Value = "1"},
                           }, "Value", "Text", 0);
            }
            if (com.PReturnAccount != 2)
            {
                ViewBag.PRetAcc = QkSelect.List(sacc, "ID", "Name", com.PReturnAccount);
            }
            else
            {
                ViewBag.PRetAcc = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "Purchase", Value = "2"},
                           }, "Value", "Text", 0);
            }

            var Acc = db.Accountss.Where(a => a.AccountsID != 4 && a.Group != 23).Select(a => new
            {
                ID = a.AccountsID,
                Name = a.Name
            }).ToList();
            ViewBag.Acc = QkSelect.List(Acc, "Id", "Name");
            if (com.SalaryAccount != null)
            {
                ViewBag.SalAcc = QkSelect.List(Acc, "ID", "Name", com.SalaryAccount);
            }
            else
            {
                ViewBag.SalAcc = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "", Value = ""},
                           }, "Value", "Text", 0);
            }
            //Property
            if (com.TCAccount != 0)
            {
                ViewBag.TCAcc = QkSelect.List(sacc, "ID", "Name", com.TCAccount);
            }
            else
            {
                ViewBag.TCAcc = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "Tenancy Contract", Value = "0"},
                           }, "Value", "Text", 0);
            }
            if (com.TCSAccount != 0)
            {
                ViewBag.TCSAcc = QkSelect.List(sacc, "ID", "Name", com.TCSAccount);
            }
            else
            {
                ViewBag.TCSAcc = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "Tenancy Contract Security Deposit", Value = "0"},
                           }, "Value", "Text", 0);
            }
            if (com.RentalAccount != 0)
            {
                ViewBag.RentalAcc = QkSelect.List(sacc, "ID", "Name", com.RentalAccount);
            }
            else
            {
                ViewBag.RentalAcc = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "Rental Income", Value = "0"},
                           }, "Value", "Text", 0);
            }
            if (com.RegdepositAccount != 0)
            {
                ViewBag.RegAcc = QkSelect.List(sacc, "ID", "Name", com.RegdepositAccount);
            }
            else
            {
                ViewBag.RegAcc = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "Registration Deposit Account", Value = "0"},
                           }, "Value", "Text", 0);
            }
            var BAcc = (from a in db.Accountss
                        where a.Group == 8
                        select new
                        {
                            ID = a.AccountsID,
                            Name = a.Name
                        }).ToList();

            if (com.BankAccount != 0)
            {
                ViewBag.BankAcc = QkSelect.List(BAcc, "ID", "Name", com.BankAccount);
            }
            else
            {
                ViewBag.BankAcc = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "Select", Value = ""},
                           }, "Value", "Text", 0);
            }

            var EnablePayroll = db.EnableSettings.Where(a => a.EnableType == "EnablePayroll").FirstOrDefault();
            var EnablePayrolls = EnablePayroll != null ? EnablePayroll.Status : Status.inactive;
            ViewBag.EnablePayroll = EnablePayrolls;

            var Accgp = db.AccountsGroups
                        .Select(s => new
                        {
                            ID = s.AccountsGroupID,
                            Name = s.Name
                        })
                        .ToList();
            ViewBag.AccGrp = QkSelect.List(Accgp, "ID", "Name");
            var TMembers = db.Employees
                 .Select(s => new
                 {
                     ID = s.EmployeeId,
                     Name = s.FirstName + " " + s.LastName
                 })
                 .ToList();
            var AssignedTo = db.AssignedTos.Where(a => a.CustomerID == -1).Select(a => a.EmployeeId).ToList().ToArray();
            vmodel.AssignedToo = AssignedTo;
            ViewBag.Employees = new MultiSelectList(TMembers, "ID", "Name", vmodel.AssignedTo);
            vmodel.Broker = com.Broker;
            vmodel.Contractor = com.Contractor;
            vmodel.Developer = com.Developer;
            vmodel.Landlord = com.Landlord;
            vmodel.Tenant = com.Tenant;
            vmodel.Payrolldate = (com.Payrolldate==null)?"": com.Payrolldate.Value.ToString("dd-MM-yyyy");
            _FinancialYear();
            return View(vmodel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Company Edit")]
        public ActionResult Edit(CompanyViewModel cmp)
        {
            if (ModelState.IsValid)
            {
                Company company = db.companys.Find(1L);
                company.CPName = cmp.Company.CPName;

                company.TRN = cmp.Company.TRN;
                company.CPEmail = cmp.Company.CPEmail;
                company.CPPhone = cmp.Company.CPPhone;
                company.CPMobile = cmp.Company.CPMobile;
                company.CPFax = cmp.Company.CPFax;
                company.CPMainBranch = cmp.Company.CPMainBranch;
                company.DbBackUpPath = cmp.DbBackUpPath;
                company.CPWebsite = cmp.Company.CPWebsite;
                company.CPAddress = cmp.Company.CPAddress;
                company.SMTPEmail = cmp.Company.SMTPEmail;
                company.SMTPHost = cmp.Company.SMTPHost;
                company.SMTPUsername = cmp.Company.SMTPUsername;
                company.SMTPPassword = cmp.Company.SMTPPassword;
                company.SMTPPort = cmp.Company.SMTPPort;
                company.EnableSsl = cmp.Company.EnableSsl;
                company.smssenderid = cmp.Company.smssenderid;
                company.username = cmp.Company.username;
                company.password = cmp.Company.password;
                company.SaleAccount = cmp.SaleAccount;
                company.PurchaseAccount = cmp.PurchaseAccount;
                company.SReturnAccount = cmp.SReturnAccount;
                company.PReturnAccount = cmp.PReturnAccount;
                company.SalaryAccount = cmp.SalaryAccount;

                company.TCAccount = cmp.tenancycontractaccount;
                company.TCSAccount = cmp.tenancycontractSecurityDepositaccount;
                company.RentalAccount = cmp.RentalIncomeaccount;
                company.RegdepositAccount = cmp.Regdepositaccount;
                company.Broker = cmp.Broker;
                company.Contractor = cmp.Contractor;
                company.Developer = cmp.Developer;
                company.Landlord = cmp.Landlord;
                company.Tenant = cmp.Tenant;
            
                company.BankAccount = cmp.BankAccount;

                if (cmp.RemoveLogo == false)
                {
                    if (cmp.Company.CPLogo != null && Request.Form.Files["Company.CPLogo"] != null)
                    {
                        // files upload
                        IFormFile file = Request.Form.Files["Company.CPLogo"];
                        var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                        var uploadUrl = LegacyWeb.MapPath("~/uploads/company/");
                        file.SaveAs(Path.Combine(uploadUrl, fileName));
                        company.CPLogo = fileName;
                    }
                }
                else
                {
                    string fullPath = LegacyWeb.MapPath("~/uploads/company/" + company.CPLogo);
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                    company.CPLogo = null;
                }
                if (cmp.Payrolldate != null&& cmp.Payrolldate!="")
                {
                    company.Payrolldate = DateTime.Parse(cmp.Payrolldate.ToString(), new CultureInfo("en-GB"));
                }
                else
                {
                    company.Payrolldate = null;
                }
                db.Entry(company).State = EntityState.Modified;
                db.SaveChanges();

                CompanyHeader CompanyHead = db.CompanyHeaders.Find((long)1);
                if (cmp.RemoveHeaderFooter == false)
                {
                    if (cmp.Header != null && Request.Form.Files["Header"] != null)
                    {
                        IFormFile file = Request.Form.Files["Header"];
                        var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                        var uploadUrl = LegacyWeb.MapPath("~/uploads/companyheader/header");
                        file.SaveAs(Path.Combine(uploadUrl, fileName));
                        CompanyHead.Header = fileName;
                    }
                    if (cmp.Footer != null && Request.Form.Files["Footer"] != null&& cmp.Footer!="")
                    {
                        IFormFile file = Request.Form.Files["Footer"];
                        var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                        var uploadUrl = LegacyWeb.MapPath("~/uploads/companyheader/footer");
                        file.SaveAs(Path.Combine(uploadUrl, fileName));
                        CompanyHead.Footer = fileName;

                        //    string storePath = LegacyWeb.MapPath("~/uploads/companyheader/footer");
                        //    //Save file to server folder  
                    }
                    db.Entry(CompanyHead).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {

                    string fullPath1 = LegacyWeb.MapPath("~/uploads/companyheader/header/" + CompanyHead.Header);
                    if (System.IO.File.Exists(fullPath1))
                    {
                        System.IO.File.Delete(fullPath1);
                    }

                    string fullPath2 = LegacyWeb.MapPath("~/uploads/companyheader/footer/" + CompanyHead.Footer);
                    if (System.IO.File.Exists(fullPath2))
                    {
                        System.IO.File.Delete(fullPath2);
                    }

                    CompanyHead.Header = null;
                    CompanyHead.Footer = null;
                    db.Entry(CompanyHead).State = EntityState.Modified;
                    db.SaveChanges();
                }


                addCodePrefix("Invoice", cmp.InvoicePrefix);
                addCodePrefix("ProForma", cmp.ProFormaPrefix);
                addCodePrefix("Purchase", cmp.PurchasePrefix);
                addCodePrefix("Quotation", cmp.QuotationPrefix);

                addCodePrefix("Customer", cmp.Customer);
                addCodePrefix("Supplier", cmp.Supplier);
                addCodePrefix("Item", cmp.Item);
                addCodePrefix("Branch", cmp.Branch);
                addCodePrefix("Deliverynote", cmp.Deliverynote);
                addCodePrefix("Reciept", cmp.Reciept);
                addCodePrefix("Payment", cmp.Payment);
                addCodePrefix("Employee", cmp.Employee);
                addCodePrefix("PurchaseReturn", cmp.PReturn);
                addCodePrefix("SalsReturn", cmp.SReturn);
                addCodePrefix("PurchaseOrder", cmp.POrder);
                addCodePrefix("HireInvoice", cmp.HireInvoicePrefix);
                addCodePrefix("HireSalesOrder", cmp.HireSalesOrderPrefix);
                addCodePrefix("HireQuotation", cmp.HireQuotationPrefix);
                addCodePrefix("HireProforma", cmp.HireProformaPrefix);
                addCodePrefix("HireDelivernote", cmp.HireDelivernotePrefix);
                addCodePrefix("HireReturn", cmp.HireReturn);
                addCodePrefix("CrossHireInvoice", cmp.CrossHireInvoicePrefix);
                addCodePrefix("Packinglist", cmp.Packinglist);
                addCodePrefix("Production", cmp.Production);
                addCodePrefix("Unassemble", cmp.Unassemble);
                addCodePrefix("MaterialRequisition", cmp.MR);
                addCodePrefix("MRNote", cmp.MRNote);
                addCodePrefix("PurchaseQuotation", cmp.PQuotation);
                addCodePrefix("Journal", cmp.Journal);
                addCodePrefix("JobCard", cmp.JobCard);
                addCodePrefix("MC", cmp.MC);
                addCodePrefix("Project", cmp.Project);
                addCodePrefix("Task", cmp.Task);
                addCodePrefix("SalesOrder", cmp.SalesOrder);
                addCodePrefix("StockAdjustment", cmp.StockAdjustment);
                addCodePrefix("StockJournal", cmp.StockJournal);
                addCodePrefix("StockTransfer", cmp.StockTransfer);
                addCodePrefix("StockVerification", cmp.StockVerification);

                addCodePrefix("TaxExempt", cmp.TaxExempt);


                if (cmp.AssignedToo != null)
                {
                    db.AssignedTos.RemoveRange(db.AssignedTos.Where(a => a.CustomerID == -1));
                    db.SaveChanges();
                    IList<AssignedTo> Assigned = new List<AssignedTo>();


                    foreach (var arr in cmp.AssignedToo)
                    {



                        Assigned.Add(new AssignedTo()
                        {
                            CustomerID = -1,
                            EmployeeId = arr,
                            Status = "Assigned",
                            AssignBy = User.Identity.GetUserId(),
                            CreatedDate = Convert.ToDateTime(System.DateTime.Now).AddMilliseconds(100),
                            ChkStatus = (int)Status.active
                        });
                    }
                    if (Assigned != null)
                    {
                        db.AssignedTos.AddRange(Assigned);
                        db.SaveChanges();
                    }
                }
                var userid = User.Identity.GetUserId();
                com.addlog(LogTypes.Updated, userid, "Company", "companys", findip(), company.CompanyID, "Company Updated Successfully");
                Success("Successfully Updated Company Details.", true);
                return RedirectToAction("Edit", "Company");
            }
            else
            {
                Warning("Looks like something went wrong. Please check your form..", true);
            }
            var Bran = db.Branchs.Where(s => s.Status == Status.active)
              .Select(s => new
              {
                  ID = s.BranchID,
                  Name = s.BranchName
              }).ToList();
            ViewBag.Branch = QkSelect.List(Bran, "ID", "Name");
            return View(cmp);
        }
        private void addCodePrefix(string section, string cprefix)
        {
            var Exists = db.CodePrefixs.Any(c => c.section == section);
            if (Exists)
            {
                if(cprefix != null)
                {
                    CodePrefix cpfx = db.CodePrefixs.Where(a => a.section == section).FirstOrDefault();
                    cpfx.prefix = cprefix;
                    cpfx.number = 0;
                    db.Entry(cpfx).State = EntityState.Modified;
                    db.SaveChanges();
                } 
                else
                {
                    CodePrefix cpfx = db.CodePrefixs.Where(a => a.section == section).FirstOrDefault();
                    cpfx.prefix = "";
                   
                    db.Entry(cpfx).State = EntityState.Modified;
                    db.SaveChanges();

                }
            }
            else
            {
                CodePrefix cfx = new CodePrefix();
                cfx.prefix = cprefix;
                cfx.section = section;
                db.CodePrefixs.Add(cfx);
                db.SaveChanges();
            }

        }
        [HttpGet]
        public JsonResult CheckPayrollDate(string Date)
        {
            DateTime? Seldate = null;
            var msg = "";
            if (Date != "")
            {
                Seldate = DateTime.Parse(Date, new CultureInfo("en-GB"));
            }
            //Payroll Voucher first date
            var PVCount = db.PayrollVouchers.Count();
            if (PVCount != 0)
            {
                var PVDate = from n in db.PayrollVouchers
                             group n by n.CreatedDate > Seldate into g
                             select g.OrderBy(t => t.PRDate).FirstOrDefault();
                var PVfirst = PVDate.Select(x => x.PRDate).First();
                //Daily attendance first date
                var AtDate = from n in db.DailyAttendances
                             group n by n.MonthYear > Seldate into g
                             select g.OrderBy(t => t.MonthYear).FirstOrDefault();
                var DAfirst = AtDate.Select(x => x.MonthYear).First();
                if (PVfirst < Seldate && DAfirst < Seldate)
                {
                    msg = "Payroll Voucher entry is earlier than payroll voucher and Attendance..";
                }
                else if (PVfirst < Seldate)
                {
                    msg = "Payroll Voucher entry is earlier than payroll voucher..";
                }
                else if (DAfirst < Seldate)
                {
                    msg = "Daily Attendance entry is earlier than Attendance..";
                }
                else
                {
                    msg = "";
                }
            }            
            return Json(msg);
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
