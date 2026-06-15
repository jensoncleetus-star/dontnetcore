using QuickSoft.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using QuickSoft.Controllers;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Areas.Property.Controllers
{
    [Microsoft.AspNetCore.Mvc.Area("Property")]
    public class ContractorController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ContractorController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        //[QkAuthorize(Roles = "Dev,Contractor")]
        public ActionResult Index()
        {
            var OpAll = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);

            ViewBag.Cust = OpAll;
            ViewBag.Empl = OpAll;
            ViewBag.Mobile = OpAll;
            ViewBag.Phone = OpAll;
            ViewBag.TxType = QkSelect.List(new List<SelectListItem>
                {
                    new SelectListItem { Text = "All"},
                    new SelectListItem { Text = "Item Wise", Value = "0"},
                    new SelectListItem { Text = "Exempt", Value = "1"},
                }, "Value", "Text");

            return View();
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Contractor")]
        public JsonResult GetContractor(long? Contractor, string TaxReg, long? Mobile, long? Phone, decimal? CLimit, int? CPeriod, long? Employee, string Type, string MailId, string Alias)
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

            //TaxType ttype = new TaxType();
            //if (TxType != null && TxType != "")
            //{
            //    ttype = (TxType == "0") ? TaxType.ItemWise : TaxType.Exempt;
            //}

            var userpermission = User.IsInRole("All Contractor");
            var UserId = User.Identity.GetUserId();

            var uDev = User.IsInRole("Dev");
            var uCustView = User.IsInRole("View Contractor");
            var uEdit = User.IsInRole("Edit Contractor");
            var uDelete = User.IsInRole("Delete Contractor");

            var v = (from a in db.Contractors
                     join x in db.Accountss on a.Accounts equals x.AccountsID
                     join b in db.Contacts on a.Contact equals b.ContactID into tmp
                     from b in tmp.DefaultIfEmpty()
                     where
                           (Contractor == null || Contractor == 0 || a.ContractorID == Contractor) &&
                           (TaxReg == null || TaxReg == "" || x.TRN == TaxReg) &&
                           (Mobile == null || Mobile == 0 || b.ContactID == Mobile) &&
                           (Phone == null || Phone == 0 || b.ContactID == Phone) &&
                           (CLimit == null || CLimit == 0 || a.CreditLimit == CLimit) &&
                           (CPeriod == null || CPeriod == 0 || a.CreditPeriod == CPeriod) &&
                           //(Type == null || Type == "" || a.Type == Type) &&
                           (MailId == null || MailId == "" || b.EmailId == MailId)
                           && (userpermission == true || x.CreatedBy == UserId)
                           && (Alias == null || Alias == "" || x.Alias == Alias)
                     select new
                     {
                         id = a.ContractorID,
                         a.ContractorCode,
                         a.ContractorName,
                         TaxRegNo = x.TRN,
                         a.Location,
                         Address = b.Address != null ? b.Address : "" +
                         "<br/>" + b.City != null ? b.City : "" +
                         " " + b.State != null ? b.State : "" +
                         " " + b.Country != null ? b.Country : "" +
                         "<br/>" + b.Zip != null ? b.Zip : "",
                         Phone = b.Phone,
                         //Mobile = b.Mobile,
                         Email = b.EmailId,
                         CreditLimit = a.CreditLimit,
                         CreditPeriod = a.CreditPeriod,
                         OpnBalance = (x.OpnBalanceCr > 0) ? (x.OpnBalanceCr != 0 ? x.OpnBalanceCr + " Cr." : "0.00") : (x.OpnBalance != 0 ? x.OpnBalance + " Dr." : "0.00"),
                         Credit = (db.AccountsTransactions.Where(d => d.Account == a.Accounts && d.Status == null).Sum(d => (decimal?)d.Credit) ?? 0),
                         Debit = (db.AccountsTransactions.Where(b => b.Account == a.Accounts && b.Status == null).Sum(b => (decimal?)b.Debit) ?? 0),
                         Dev = uDev,
                         Details = uCustView,
                         Edit = uEdit,
                         Delete = uDelete,
                         Alias = x.Alias,
                         mobmodel = (from ac in db.Mobiles
                                     where (ac.Contact == a.Contact)
                                     select new MobileViewModel
                                     {
                                         Num = (ac.Name == "" || ac.Name == null) ? ac.MobileNum : ac.MobileNum + "-" + ac.Name,
                                         Name = ac.Name
                                     }).ToList(),

                     }).Select(o => new
                     {
                         o.id,
                         o.ContractorCode,
                         o.ContractorName,
                         o.TaxRegNo,
                         o.Location,
                         o.Address,
                         o.Phone,
                         o.Email,
                         o.CreditLimit,
                         o.CreditPeriod,
                         o.OpnBalance,
                         o.Credit,
                         o.Debit,
                         o.Dev,
                         o.Details,
                         o.Edit,
                         o.Delete,
                         o.Alias,
                         o.mobmodel,
                         currentbalance = (o.Debit > o.Credit) ? ((o.Debit - o.Credit) + " Dr.") : ((o.Credit - o.Debit) + " Cr."),
                     });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.ContractorName.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.ContractorCode.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.TaxRegNo.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Alias.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.OpnBalance.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.currentbalance.ToString().ToLower().Contains(search.ToLower())
                                 //p.Phone.ToString().ToLower().Contains(search.ToLower()) ||
                                 //p.Mobile.ToString().ToLower().Contains(search.ToLower()) ||
                                 //p.Email.ToString().ToLower().Contains(search.ToLower()) ||
                                 //p.CreditLimit.ToString().ToLower().Contains(search.ToLower()) ||
                                 //p.CreditPeriod.ToString().ToLower().Contains(search.ToLower())
                                 );
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
                //v = v.OrderBy(c => c.ProductCategoryID);
            }
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }


        public ActionResult Create()
        {
            var emp = db.Employees.Select(s => new
            {
                Id = s.EmployeeId,
                Name = s.FirstName + " " + s.LastName,
            }).ToList();
            ViewBag.Empl = QkSelect.List(emp, "Id", "Name");

            ViewBag.CustName = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = false, Text = null, Value = null},
                           }, "Value", "Text", 1);

            var PredefinedCity = db.EnableSettings.Where(a => a.EnableType == "PredefinedCity").FirstOrDefault();
            ViewBag.CityCheck = PredefinedCity != null ? PredefinedCity.Status : Status.inactive;

            var ToReceipt = db.EnableSettings.Where(a => a.EnableType == "BillToBillReceipt").FirstOrDefault();
            var BillTo = ToReceipt != null ? (ToReceipt.Status == Status.active ? 0 : 1) : 1;
            ViewBag.BillToReceipt = BillTo;

            var EnableCRM = db.EnableSettings.Where(a => a.EnableType == "EnableCRM").FirstOrDefault();
            var EnableCRMs = EnableCRM != null ? EnableCRM.Status : Status.inactive;
            ViewBag.EnableCRM = EnableCRMs;


            if (ViewBag.CityCheck == Status.active)
            {
                var city = db.Cities.Select(s => new
                {
                    Id = s.CityName,
                    CityName = s.CityName
                }).ToList();
                ViewBag.SCity = QkSelect.List(city, "Id", "CityName");
            }

            var use = db.Employees
                        .Select(s => new
                        {
                            ID = s.EmployeeId,
                            Name = s.FirstName + " " + s.LastName
                        })
                        .ToList();
            ViewBag.users = QkSelect.List(use, "ID", "Name");
            var cont = db.ContractTypes
                        .Select(s => new
                        {
                            ID = s.ID,
                            Name = s.Name
                        })
                        .ToList();
            ViewBag.ContType = QkSelect.List(cont, "ID", "Name");
            
            ViewBag.LastEntry = db.Contractors.Select(p => p.ContractorID).AsEnumerable().DefaultIfEmpty(0).Max();
            ContractorViewModel vmodel = new ContractorViewModel();
            vmodel.ContractorCode = CustCode();
            vmodel.Section = "Accounts";
            var contType = db.ContractTypes
                             .Select(s => new
                             {
                                 ID = s.ID,
                                 Name = s.Name
                             })
                             .ToList();
            ViewBag.ContraType = QkSelect.List(contType, "ID", "Name");
            //vmodel.Type = ProType();
            return View(vmodel);

        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, Create PayrollUnits")]
        public JsonResult Create(ContractorViewModel vmodel)
        {
            string msg = "";
            bool stat = false;
            var custExists = db.Contractors.Any(u => u.ContractorCode == vmodel.ContractorCode);
            if (custExists)
            {
                msg = "Contractor Code already exists.";
                stat = false;
                //Danger("A Contractor with same Contractor Code exists.", true);
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                //if (!ModelState.IsValid)
                //{
                //    var modelErrors = new List<string>();
                //    foreach (var modelState in ModelState.Values)
                //    {
                //        foreach (var modelError in modelState.Errors)
                //        {
                //            modelErrors.Add(modelError.ErrorMessage);
                //        }
                //    }
                //}
                if (ModelState.IsValid)
                {
                    var bro = db.companys.Select(a => a.Contractor).FirstOrDefault();
                    if (bro == null)
                    {
                        msg = "Please give Account Group for Contractor in Company.";
                        stat = false;
                        //Danger("A Landlord with same Landlord Code exists.", true);
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    Int64 contactId = 0;
                    Int64 accountId = 0;
                    Int64 CustId = 0;
                    var UserId = User.Identity.GetUserId();
                    var Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();


                    Accounts account = new Accounts();
                    account.Name = vmodel.ContractorName;
                    account.Alias = vmodel.Alias;
                    account.PrintName = vmodel.ContractorName;
                    
                    account.Status = Status.active;
                    account.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                    account.CreatedBy = UserId;
                    account.TRN = vmodel.TaxRegNo;

                    if (vmodel.DC == DC.Debit)
                    {
                        account.OpnBalance = (vmodel.OpnBalance == null) ? 0 : (decimal)vmodel.OpnBalance;
                        account.OpnBalanceCr = 0;
                    }
                    if (vmodel.DC == DC.Credit)
                    {
                        account.OpnBalance = 0;
                        account.OpnBalanceCr = (vmodel.OpnBalance == null) ? 0 : (decimal)vmodel.OpnBalance;
                    }
                    account.Group = (bro == null) ? 0 : Convert.ToInt64(bro);
                    db.Accountss.Add(account);
                    db.SaveChanges();
                    accountId = account.AccountsID;

                    if (vmodel.ContractorID != 0 && vmodel.ContractorID != null)
                    {
                        Contractor cus = db.Contractors.Find(vmodel.ContractorID);
                        cus.Accounts = account.AccountsID;
                        cus.ContractorName = vmodel.ContractorName;
                        cus.ContractorCode = vmodel.ContractorCode;
                        cus.CreditLimit = vmodel.CreditLimit != null ? (decimal)vmodel.CreditLimit : 0;
                        cus.CreditPeriod = vmodel.CreditPeriod != null ? (int)vmodel.CreditPeriod : 0;
                        //cus.SalesPerson = vmodel.SalesPerson;
                        //cus.Lattitude = vmodel.Lattitude;
                        //cus.Longitude = vmodel.Longitude;

                        cus.Location = vmodel.Location;
                        //cus.TaxRegNo = vmodel.TaxRegNo;
                        cus.Remark = vmodel.Remark;

                        cus.BankName = vmodel.BankName;
                        cus.AccountNo = vmodel.AccountNo;
                        cus.BranchName = vmodel.BranchName;
                        cus.IbanNo = vmodel.IbanNo;
                        cus.Swift = vmodel.Swift;
                        cus.Type = vmodel.Type;
                        cus.ContractType = vmodel.ContractType;
                        //cus.SourceOfLead = vmodel.SourceOfLead;
                        //cus.Type = CRMContractorType.Contractor;
                        cus.EntryNo = GetEntryNo();
                        db.Entry(cus).State = EntityState.Modified;
                        db.SaveChanges();
                        CustId = cus.ContractorID;

                        Contact cont = db.Contacts.Find(cus.Contact);

                        cont.Name = vmodel.ContractorName;
                        cont.Address = vmodel.Address;
                        cont.City = vmodel.City;
                        cont.State = vmodel.State;
                        cont.Country = vmodel.Country;
                        cont.Zip = vmodel.Zip;
                        cont.Phone = vmodel.Phone;
                        //cont.Mobile = vmodel.Mobile;
                        cont.Fax = vmodel.Fax;
                        cont.EmailId = vmodel.EmailId;
                        cont.Reference = vmodel.Reference;
                        cont.ContactPerson = vmodel.ContactPerson;

                        db.Entry(cont).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Mobiles.RemoveRange(db.Mobiles.Where(a => a.Contact == cus.Contact));
                        db.SaveChanges();
                        if (vmodel.mobmodel != null)
                        {
                            foreach (var arr in vmodel.mobmodel)
                            {
                                if (arr.Num != "null" && arr.Num != null)
                                {
                                    var mobchk = db.Mobiles.Where(x => x.MobileNum == arr.Num && x.Contact == cus.Contact).Any();
                                    if (!mobchk)
                                    {
                                        var mob = new Mobile
                                        {
                                            Contact = cus.Contact,
                                            MobileNum = arr.Num,
                                            Name = arr.Name
                                        };
                                        db.Mobiles.Add(mob);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var contact = new Contact
                        {
                            Name = vmodel.ContractorName,
                            Address = vmodel.Address,
                            City = vmodel.City,
                            State = vmodel.State,
                            Country = vmodel.Country,
                            Zip = vmodel.Zip,
                            Phone = vmodel.Phone,
                            //Mobile = vmodel.Mobile,
                            Fax = vmodel.Fax,
                            EmailId = vmodel.EmailId,
                            Reference = vmodel.Reference,
                            ContactPerson = vmodel.ContactPerson,
                            Group = 2,
                            Status = Status.active,

                        };
                        db.Contacts.Add(contact);
                        db.SaveChanges();
                        contactId = contact.ContactID;
                        if (vmodel.mobmodel != null)
                        {
                            foreach (var arr in vmodel.mobmodel)
                            {
                                if (arr.Num != "null" && arr.Num != null)
                                {
                                    var mobchk = db.Mobiles.Where(x => x.MobileNum == arr.Num && x.Contact == contactId).Any();
                                    if (!mobchk)
                                    {
                                        var mob = new Mobile
                                        {
                                            Contact = contactId,
                                            MobileNum = arr.Num,
                                            Name = arr.Name
                                        };
                                        db.Mobiles.Add(mob);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                        Contractor cus = new Contractor
                        {
                            EntryNo = GetEntryNo(),
                            Contact = contactId,
                            Accounts = accountId,
                            ContractorName = vmodel.ContractorName,
                            ContractorCode = vmodel.ContractorCode,
                            CreditLimit = vmodel.CreditLimit != null ? (decimal)vmodel.CreditLimit : 0,
                            CreditPeriod = vmodel.CreditPeriod != null ? (int)vmodel.CreditPeriod : 0,
                            //SalesPerson = vmodel.SalesPerson,
                            //Lattitude = "",
                            //Longitude = "",
                            Location = vmodel.Location,
                            //TaxRegNo = vmodel.TaxRegNo,
                            Remark = vmodel.Remark,
                            BankName = vmodel.BankName,
                            AccountNo = vmodel.AccountNo,
                            IbanNo = vmodel.IbanNo,
                            BranchName = vmodel.BranchName,
                            Swift = vmodel.Swift,
                            Type = vmodel.Type,
                            ContractType=vmodel.ContractType,
                            //Type = CRMContractorType.Contractor,
                            //SourceOfLead = vmodel.SourceOfLead,
                        };
                        db.Contractors.Add(cus);
                        db.SaveChanges();
                        CustId = cus.ContractorID;

                    }

                    var invoices = "";
                    if (vmodel.docmodel != null)
                    {
                        foreach (var arr in vmodel.docmodel)
                        {
                            if (arr.Date != null)
                            {
                                PropertyDocumentType doc = new PropertyDocumentType();
                                doc.DocumentType = Convert.ToInt64(arr.Type);
                                doc.Reference = CustId;
                                doc.Purpose = "Contractor";
                                doc.ExpDate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));
                                db.PropertyDocumentTypes.Add(doc);
                                db.SaveChanges();
                                Int64 docid = doc.ID;

                                if (arr.Attachments != null)
                                {
                                    if (arr.Attachments != null)
                                    {
                                        string storePath = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "Contractor_" + docid);
                                        if (!Directory.Exists(storePath))
                                            Directory.CreateDirectory(storePath);

                                        // files upload
                                        IFormFile file = Request.Form.Files[0];
                                        var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                                        var uploadUrl = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "Contractor_" + docid + "/");
                                        file.SaveAs(Path.Combine(uploadUrl, fileNames));

                                        DocumentFile docfile = new DocumentFile();
                                        docfile.attachments = fileNames;
                                        docfile.Document = docid;
                                        db.DocumentFiles.Add(docfile);
                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                    }


                    if (vmodel.OpnBalance > 0)
                    {
                        if (vmodel.DC == DC.Debit)
                        {
                            com.addAccountTrasaction((decimal)vmodel.OpnBalance, 0, accountId, "Opening Balance", accountId, DC.Debit);

                        }
                        if (vmodel.DC == DC.Credit)
                        {
                            com.addAccountTrasaction(0, (decimal)vmodel.OpnBalance, accountId, "Opening Balance", accountId, DC.Credit);
                        }
                    }


                    var convert = "";
                    var remark = "";
                    if (vmodel.ConvertFrom != null)
                    {
                        convert = vmodel.ConvertFrom;
                        remark = "Convert";

                    }
                    else
                    {
                        convert = "Direct";
                        remark = "Direct";
                    }

                    com.addlog(LogTypes.Created, UserId, "Contractor", "Contractor", findip(), CustId, "Contractor Added Successfully");

                    msg = "Successfully added Contractor details.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {
                    var emp = db.Employees.Select(s => new
                    {
                        Id = s.EmployeeId,
                        Name = s.FirstName + " " + s.LastName,
                    }).ToList();
                    ViewBag.Empl = QkSelect.List(emp, "Id", "Name");

                    msg = "Looks like something went wrong. Please check your form.";
                    stat = false;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
            }
        }

        private bool BillExist(string SENo)
        {
            var Exists = db.SalesEntrys.Any(c => c.BillNo == SENo);
            bool res = (Exists) ? true : false;
            return res;
        }
        public Boolean InsertToSales(ReferenceAccountViewModel arr, long CustId, string UserId, long branch)
        {
            SalesEntry SEentry = new SalesEntry();
            SEentry.SaleType = SaleType.Sale;
            SEentry.SENo = 0;
            SEentry.BillNo = arr.Invoice;
            SEentry.SEDate = DateTime.Parse(arr.RADate, new CultureInfo("en-GB"));
            SEentry.SECashier = 0;
            SEentry.CustomerType = CustomerType.Walking;
            SEentry.Customer = CustId;
            SEentry.PONo = "";
            SEentry.PayType = "invoice";
            SEentry.SEItems = 0;


            SEentry.SEItemQuantity = 0;
            SEentry.SESubTotal = 0;
            SEentry.SETax = 0;
            SEentry.SETaxAmount = 0;
            SEentry.SEDiscount = 0;
            SEentry.SEGrandTotal = 0;
            SEentry.SENote = "";
            SEentry.ConvertType = null;
            SEentry.ConvertNo = null;

            SEentry.Print = 1;
            SEentry.SECreatedDate = Convert.ToDateTime(System.DateTime.Now);
            SEentry.CreatedBy = UserId;
            SEentry.Status = 0;
            SEentry.Branch = branch;
            SEentry.Remarks = "Sales Entry From Contractor Creation";


            db.SalesEntrys.Add(SEentry);
            db.SaveChanges();
            Int64 salesEntryId = SEentry.SalesEntryId;


            SEPayment SEpay = new SEPayment();
            SEpay.SEDate = DateTime.Parse(arr.RADate, new CultureInfo("en-GB"));
            SEpay.SEEntryDate = Convert.ToDateTime(System.DateTime.Now);
            SEpay.SEBillAmount = (decimal)arr.Amount;
            SEpay.SEPaidAmount = 0;

            SEpay.CustomerId = CustId;
            SEpay.CreatedBranch = branch;
            SEpay.CreatedUserId = UserId;
            SEpay.SECreatedDate = Convert.ToDateTime(System.DateTime.Now);
            SEpay.Status = 0;
            SEpay.SalesEntry = salesEntryId;
            db.SEPayments.Add(SEpay);
            db.SaveChanges();
            return true;
        }

        public ActionResult Edit(long? id)
        {
            var PredefinedCity = db.EnableSettings.Where(a => a.EnableType == "PredefinedCity").FirstOrDefault();
            ViewBag.CityCheck = PredefinedCity != null ? PredefinedCity.Status : Status.inactive;

            var ToReceipt = db.EnableSettings.Where(a => a.EnableType == "BillToBillReceipt").FirstOrDefault();
            var BillTo = ToReceipt != null ? (ToReceipt.Status == Status.active ? 0 : 1) : 1;
            ViewBag.BillToReceipt = BillTo;

            var EnableCRM = db.EnableSettings.Where(a => a.EnableType == "EnableCRM").FirstOrDefault();
            var EnableCRMs = EnableCRM != null ? EnableCRM.Status : Status.inactive;
            ViewBag.EnableCRM = EnableCRMs;

            if (ViewBag.CityCheck == Status.active)
            {
                var city = db.Cities.Select(s => new
                {
                    Id = s.CityName,
                    CityName = s.CityName
                }).ToList();
                ViewBag.SCity = QkSelect.List(city, "Id", "CityName");
            }

            var userpermission = User.IsInRole("All Contractor");
            var UserId = User.Identity.GetUserId();

            var cus = (from a in db.Contractors
                       join b in db.Accountss on a.Accounts equals b.AccountsID
                       where a.ContractorID == id && (userpermission == true || b.CreatedBy == b.CreatedBy)
                       select new
                       {
                           a.Contact,
                           a.Accounts,
                           a.ContractorName,
                           a.ContractorCode,
                           a.CreditLimit,
                           a.CreditPeriod,
                           a.Remark,
                           a.Location,
                           TaxRegNo = b.TRN,
                           a.Type,
                           a.BankName,
                           a.AccountNo,
                           a.IbanNo,
                           a.BranchName,
                           a.Swift,
                           a.ContractorID,
                           b.Alias,
                           a.ContractType
                       }).FirstOrDefault();
            if (cus == null)
            {
                return NotFound();
            }

            Contact cont = db.Contacts.Find(cus.Contact);
            Accounts account = db.Accountss.Find(cus.Accounts);

            ContractorSubmitViewModel landmodel = new ContractorSubmitViewModel();
            landmodel.ContractorID = cus.ContractorID;
            landmodel.ContractorName = cus.ContractorName;
            landmodel.ContractorCode = cus.ContractorCode;
            landmodel.CreditLimit = cus.CreditLimit;
            landmodel.CreditPeriod = cus.CreditPeriod;
            landmodel.Remark = cus.Remark;
            landmodel.Location = cus.Location;
            landmodel.TaxRegNo = account.TRN;
            landmodel.Address = cont.Address;
            landmodel.City = cont.City;
            landmodel.State = cont.State;
            landmodel.Country = cont.Country;
            landmodel.Zip = cont.Zip;
            landmodel.Phone = cont.Phone;
            landmodel.mobmodel = (from ac in db.Mobiles
                                  where (ac.Contact == cus.Contact)
                                  select new MobileViewModel
                                  {
                                      Num = ac.MobileNum,
                                      Name = ac.Name
                                  }).ToList();
            landmodel.Fax = cont.Fax;
            landmodel.EmailId = cont.EmailId;
            landmodel.Reference = cont.Reference;
            landmodel.ContactPerson = cont.ContactPerson;
            landmodel.Type = cus.Type;
            landmodel.EmailId = cont.EmailId;
            landmodel.Reference = cont.Reference;
            landmodel.ContactPerson = cont.ContactPerson;
            landmodel.BankName = cus.BankName;
            landmodel.AccountNo = cus.AccountNo;
            landmodel.IbanNo = cus.IbanNo;
            landmodel.BranchName = cus.BranchName;
            landmodel.Swift = cus.Swift;
            landmodel.Alias = account.Alias;
            landmodel.Section = "Accounts";
            landmodel.ContractType = cus.ContractType;
            if (account.OpnBalance == 0)
            {
                landmodel.DC = DC.Credit;
                landmodel.OpnBalance = account.OpnBalanceCr;
            }
            else
            {
                landmodel.DC = DC.Debit;
                landmodel.OpnBalance = account.OpnBalance;
            }

            var emp = db.Employees.Select(s => new
            {
                Id = s.EmployeeId,
                Name = s.FirstName + " " + s.LastName,
            }).ToList();
            ViewBag.Empl = QkSelect.List(emp, "Id", "Name");

            var cust = db.Contractors.Select(r => new
            {
                ID = r.ContractorName,
                Name = r.ContractorName
            }).ToList();
            ViewBag.CustName = QkSelect.List(cust, "ID", "Name");

            var use = db.Employees
                         .Select(s => new
                         {
                             ID = s.EmployeeId,
                             Name = s.FirstName + " " + s.LastName
                         })
                         .ToList();
            ViewBag.users = QkSelect.List(use, "ID", "Name");
            var lead = db.SourceOfLeads.Select(r => new
            {
                ID = r.SourceOfLeadId,
                Name = r.SrcName
            }).ToList();
            ViewBag.SrcLead = QkSelect.List(lead, "ID", "Name");
            var contratype = db.ContractTypes
                        .Select(s => new
                        {
                            ID = s.ID,
                            Name = s.Name
                        })
                        .ToList();
            ViewBag.ContType = QkSelect.List(contratype, "ID", "Name");

            ViewBag.preEntry = db.Contractors.Where(a => a.ContractorID < id && (userpermission == true)).Select(a => a.ContractorID).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.Contractors.Where(a => a.ContractorID > id && (userpermission == true)).Select(a => a.ContractorID).DefaultIfEmpty().Min();


            ViewBag.SrcOfLead = QkSelect.List(lead, "ID", "Name");
            var contType = db.ContractTypes
                             .Select(s => new
                             {
                                 ID = s.ID,
                                 Name = s.Name
                             })
                             .ToList();
            ViewBag.ContraType = QkSelect.List(contType, "ID", "Name");
            var rtype = Request.Query["rtype"];
            if (rtype == "APP")
            {
                return View("App/Edit", landmodel);
            }
            else
            {
                return View(landmodel);
            }
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, Create Contractor")]        
        public JsonResult Update(ContractorSubmitViewModel cusmodel)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            var Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
            var id = cusmodel.ContractorID;
            if (ModelState.IsValid)
            {
                var CodeExists = db.Contractors.Any(u => u.ContractorCode == cusmodel.ContractorCode && u.ContractorID != id);
                if (CodeExists)
                {
                    msg = "A Contractor with same Contractor Code exists.";
                    stat = false;
                }
                else
                {
                    Contractor cus = db.Contractors.Find(id);
                    cus.ContractorName = cusmodel.ContractorName;
                    cus.ContractorCode = cusmodel.ContractorCode;
                    cus.CreditLimit = cusmodel.CreditLimit != null ? (decimal)cusmodel.CreditLimit : 0;
                    cus.CreditPeriod = cusmodel.CreditPeriod != null ? (int)cusmodel.CreditPeriod : 0;
                    cus.Remark = cusmodel.Remark;

                    cus.Location = cusmodel.Location;
                    //cus.TaxRegNo = cusmodel.TaxRegNo;

                    cus.BankName = cusmodel.BankName;
                    cus.AccountNo = cusmodel.AccountNo;
                    cus.BranchName = cusmodel.BranchName;
                    cus.IbanNo = cusmodel.IbanNo;
                    cus.Swift = cusmodel.Swift;
                    cus.Type = cusmodel.Type;
                    cus.ContractType = cusmodel.ContractType;
                    db.Entry(cus).State = EntityState.Modified;
                    db.SaveChanges();
                    Int64 CustId = cus.ContractorID;

                    Contact cont = db.Contacts.Find(cus.Contact);

                    cont.Name = cusmodel.ContractorName;
                    cont.Address = cusmodel.Address;
                    cont.City = cusmodel.City;
                    cont.State = cusmodel.State;
                    cont.Country = cusmodel.Country;
                    cont.Zip = cusmodel.Zip;
                    cont.Phone = cusmodel.Phone;
                    //cont.Mobile = cusmodel.Mobile;
                    cont.Fax = cusmodel.Fax;
                    cont.EmailId = cusmodel.EmailId;
                    cont.Reference = cusmodel.Reference;
                    cont.ContactPerson = cusmodel.ContactPerson;

                    db.Entry(cont).State = EntityState.Modified;
                    db.SaveChanges();

                    db.Mobiles.RemoveRange(db.Mobiles.Where(a => a.Contact == cus.Contact));
                    db.SaveChanges();
                    foreach (var arr in cusmodel.mobmodel)
                    {
                        var mob = new Mobile
                        {
                            Contact = cus.Contact,
                            MobileNum = arr.Num,
                            Name = arr.Name
                        };
                        db.Mobiles.Add(mob);
                        db.SaveChanges();
                    }

                    Accounts account = db.Accountss.Find(cus.Accounts);
                    account.PrintName = cusmodel.ContractorName;
                    account.Name = cusmodel.ContractorName;
                    account.Alias = cusmodel.Alias;
                    account.TRN = cusmodel.TaxRegNo;
                    if (cusmodel.DC == DC.Debit)
                    {
                        account.OpnBalance = cusmodel.OpnBalance;
                        account.OpnBalanceCr = 0;
                    }
                    if (cusmodel.DC == DC.Credit)
                    {
                        account.OpnBalance = 0;
                        account.OpnBalanceCr = cusmodel.OpnBalance;
                    }
                    var bro = db.companys.Select(a => a.Contractor).FirstOrDefault();// db.EnableSettings.Where(a => a.EnableType == "Contractor").Select(a => a.TypeValue).FirstOrDefault();
                    account.Group = (bro == null) ? 0 : Convert.ToInt64(bro);
                    db.Entry(account).State = EntityState.Modified;
                    db.SaveChanges();

                    bool delete = com.DeleteAllAccountTransaction("Opening Balance", account.AccountsID);

                    //var SRTran = (from a in db.SRTransactions
                    //              where a.CustomerId == id && a.PaymentId == 0
                    //              orderby a.SRTransactionId
                    //              select new
                    //              {
                    //                  a.SalesReturnId,
                    //                  a.SRPayAmount
                    //              }).ToList();
                    //if (SRTran.Count > 0)
                    //{
                    //    foreach (var ditem in SRTran)
                    //    {
                    //        var paying = ditem.SRPayAmount;
                    //        SRPayment PEP = db.SRPayments.Where(a => a.SalesReturnId == ditem.SalesReturnId).FirstOrDefault();
                    //        PEP.SReturnAmount = PEP.SReturnAmount - Convert.ToDecimal(paying);
                    //        db.Entry(PEP).State = EntityState.Modified;
                    //        db.SaveChanges();
                    //    }
                    //    db.SRTransactions.RemoveRange(db.SRTransactions.Where(a => a.PaymentId == id));
                    //}
                    // sales reciept delete
                    //var SETran = (from a in db.SETransactions
                    //              where a.ContractorId == id && a.Recieptid == 0
                    //              orderby a.SETransactionId
                    //              select new
                    //              {
                    //                  a.SalesEntry,
                    //                  a.SEPayAmount
                    //              }).ToList();
                    //if (SETran.Count > 0)
                    //{
                    //    foreach (var ditem in SETran)
                    //    {
                    //        var paying = ditem.SEPayAmount;
                    //        SEPayment SEP = db.SEPayments.Where(a => a.SalesEntry == ditem.SalesEntry).FirstOrDefault();
                    //        SEP.SEPaidAmount = SEP.SEPaidAmount - Convert.ToDecimal(paying);
                    //        db.Entry(SEP).State = EntityState.Modified;
                    //        db.SaveChanges();
                    //    }
                    //    db.SETransactions.RemoveRange(db.SETransactions.Where(a => a.Recieptid == id));
                    //}
                    //db.SaveChanges();

                    // var invoices = "";
                    //if (cusmodel.invoicedata != null)
                    //{
                    //    // bool first = true;
                    //    List<string> bills = new List<string>();
                    //    foreach (var arr in cusmodel.invoicedata)
                    //    {
                    //        if (arr.Invoice != null && arr.Amount > 0)
                    //        {
                    //            var saleentry = db.SalesEntrys.Where(a => a.BillNo == arr.Invoice && a.Status == 0).FirstOrDefault();
                    //            if (saleentry != null)
                    //            {
                    //                var update = UpdateSale(arr, CustId, UserId, Branch);
                    //                bills.Add(arr.Invoice);
                    //            }
                    //            else
                    //            {
                    //                if (!BillExist(arr.Invoice))
                    //                {
                    //                    var sale = InsertToSales(arr, CustId, UserId, Branch);
                    //                }
                    //                //else
                    //                //{
                    //                //    invoices += (first == true) ? arr.Invoice : " , " + arr.Invoice;
                    //                //    first = false;
                    //                //}
                    //            }
                    //        }
                    //    }

                    //    //if (invoices != "")
                    //    //{
                    //    //    Warning(invoices + " Already exists");
                    //    //}

                    //    //delete other sales
                    //    var salelist = db.SalesEntrys.Where(a => a.Contractor == CustId && a.Status == 0).ToList();
                    //    foreach (var slist in salelist)
                    //    {
                    //        if (!bills.Contains(slist.BillNo))
                    //        {
                    //            SalesEntry SEen = db.SalesEntrys.Find(slist.SalesEntryId);
                    //            DeleteBills(SEen);
                    //        }
                    //    }

                    //}








                    var count = 0;
                    long docid = 0;

                    foreach (var arr in cusmodel.docmodel)
                    {

                        PropertyDocumentType doc = new PropertyDocumentType();

                        if (Convert.ToInt32(arr.ID) == 0 && arr.Type != null)
                        {



                            doc.DocumentType = Convert.ToInt64(arr.Type);
                            doc.Purpose = "Contractor";
                            doc.Reference = cusmodel.ContractorID;
                            if (arr.Date != null)
                                doc.ExpDate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));
                            else
                                doc.ExpDate = null;
                            db.PropertyDocumentTypes.Add(doc);
                            db.SaveChanges();
                            docid = doc.ID;

                            string storePath = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "Contractor_" + docid);
                            if (!Directory.Exists(storePath))
                                Directory.CreateDirectory(storePath);



                            IFormFile file = Request.Form.Files["docmodel[" + count + "].Attachments"];

                            if (file.FileName != "")
                            {
                                var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                                var uploadUrl = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "Contractor_" + docid + "/");
                                file.SaveAs(Path.Combine(uploadUrl, fileNames));

                                DocumentFile docfile = new DocumentFile();
                                docfile.attachments = fileNames;
                                docfile.Document = docid;
                                db.DocumentFiles.Add(docfile);
                                db.SaveChanges();
                            }




                        }
                        else if (arr.Type != null)
                        {
                            doc = db.PropertyDocumentTypes.Find(arr.ID);
                            doc.DocumentType = Convert.ToInt64(arr.Type);
                            doc.Purpose = "Contractor";
                            doc.Reference = cusmodel.ContractorID;
                            doc.ExpDate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));

                            db.Entry(doc).State = EntityState.Modified;
                            db.SaveChanges();
                            docid = arr.ID;

                            string storePath = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "Contractor_" + docid);
                            if (!Directory.Exists(storePath))
                                Directory.CreateDirectory(storePath);



                            IFormFile file = Request.Form.Files["docmodel[" + count + "].Attachments"];

                            if (file.FileName != "")
                            {
                                var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                                var uploadUrl = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "Contractor_" + docid + "/");
                                file.SaveAs(Path.Combine(uploadUrl, fileNames));

                                DocumentFile docfile = new DocumentFile();
                                docfile.attachments = fileNames;
                                docfile.Document = docid;
                                db.DocumentFiles.Add(docfile);
                                db.SaveChanges();
                            }

                        }





                        count++;

                    }





                    
                    if (cusmodel.OpnBalance > 0)
                    {
                        if (cusmodel.DC == DC.Debit)
                        {
                            com.addAccountTrasaction(cusmodel.OpnBalance, 0, account.AccountsID, "Opening Balance", account.AccountsID, DC.Debit);

                        }
                        if (cusmodel.DC == DC.Credit)
                        {
                            com.addAccountTrasaction(0, cusmodel.OpnBalance, account.AccountsID, "Opening Balance", account.AccountsID, DC.Credit);
                        }
                    }
                    com.addlog(LogTypes.Updated, UserId, "Contractor", "Contractors", findip(), cus.ContractorID, "Contractor Updated Successfully");

                    msg = "Successfully updated Contractor details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Delete Contractor")]
        public ActionResult DeleteAllContractor(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteCust(arr) == true) ? count++ : notdel++;
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Contractors, Unable to Delete " + notdel + " Contractors. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Contractors.", true);
            }
            else
            {
                Success("Deleted " + count + " Contractors.", true);
            }
            return RedirectToAction("Index", "Contractor");
        }
        private Boolean DeleteCust(long custid)
        {
            var Msg = chkDeleteWithMsg(custid);
            if (Msg != null)
            {
                return false;
            }
            else
            {
                return DeleteFn(custid);
            }

        }
        [RedirectingAction]
        //[Authorize(Roles = "Dev,Delete Contractor")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contractor ptype = db.Contractors.Find(id);
            if (ptype == null)
            {
                return NotFound();
            }
            return PartialView(ptype);
        }
        [RedirectingAction]
        //[Authorize(Roles = "Dev,Delete Contractor")]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
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
                msg = "Successfully Deleted Contractor details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public bool DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            db.PropertyDocumentTypes.RemoveRange(db.PropertyDocumentTypes.Where(a => a.Reference == id && a.Purpose == "Contractor"));
            db.SaveChanges();
            Contractor pt = db.Contractors.Find(id);

            db.Contractors.Remove(pt);
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "Contractor", "Contractor", findip(), pt.ContractorID, "Contractor Deleted Successfully");
            return true;
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            if (db.Maintenances.Any(c => c.Contractor == id))
            {
                msg = "Contractor Already used in Property Maintenances !!";
            }
            //else if (db.PropertyRegistrations.Any(c => c.Property == id))
            //{
            //    msg = "Customer Already used in Property Registration !!";
            //}
            else
            {
                msg = null;
            }
            return msg;
        }

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,View Contractor")]
        public ActionResult Details(int? id)
        {
            ViewBag.Prjct = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = true, Text = "All", Value = null},
                               }, "Value", "Text", 1);
            ContractorSubmitViewModel cusmodel = new ContractorSubmitViewModel();
            cusmodel = (from a in db.Contractors
                        join b in db.Contacts on a.Contact equals b.ContactID into tmp
                        from b in tmp.DefaultIfEmpty()
                        join e in db.Accountss on a.Accounts equals e.AccountsID
                        where a.ContractorID == id
                        select new ContractorSubmitViewModel
                        {
                            ContractorID = a.ContractorID,
                            ContractorName = a.ContractorName,
                            ContractorCode = a.ContractorCode,
                            CreditLimit = a.CreditLimit,
                            CreditPeriod = a.CreditPeriod,
                            Remark = a.Remark,

                            Location = a.Location,
                            //TaxRegNo = e.TRN,
                            Type = a.Type,
                            //Lattitude = a.Lattitude,
                            //Longitude = a.Longitude,

                            BankName = a.BankName,
                            AccountNo = a.AccountNo,
                            BranchName = a.BranchName,
                            IbanNo = a.IbanNo,
                            Swift = a.Swift,

                            Address = b.Address,
                            City = b.City,
                            State = b.State,
                            Country = b.Country,
                            Zip = b.Zip,
                            Phone = b.Phone,
                            mobmodel = (from ac in db.Mobiles
                                        where (ac.Contact == a.Contact)
                                        select new MobileViewModel
                                        {
                                            Num = ac.MobileNum,
                                            Name = ac.Name
                                        }).ToList(),
                            Fax = b.Fax,
                            EmailId = b.EmailId,
                            Reference = b.Reference,
                            ContactPerson = b.ContactPerson,
                            AccountsID = e.AccountsID,
                            OpnBalance = e.OpnBalance != 0 ? e.OpnBalance : e.OpnBalanceCr,
                            DCname = e.OpnBalanceCr == 0 ? "Dr." : "Cr.",
                            Alias = e.Alias
                        }).FirstOrDefault();
            return View(cusmodel);

        }

        [HttpGet]
        public JsonResult GetMobile(long CnId)
        {
            var ConD = (from a in db.Mobiles
                        join b in db.Contractors on a.Contact equals b.Contact
                        where b.ContractorID == CnId
                        select new
                        {
                            Mob = a.MobileNum,
                            Name = a.Name
                        }).ToList();
            return Json(ConD);
        }

        public JsonResult SearchContractor(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Contractors
                                  where a.ContractorName.ToLower().Contains(q.ToLower()) || a.ContractorName.Contains(q)
                                  select new SelectFormat
                                  {
                                      text = a.ContractorName,
                                      id = a.ContractorID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Contractors.Select(b => new SelectFormat
                {
                    text = b.ContractorName,
                    id = b.ContractorID
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "All" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult SearchMobile(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.Contractors
                                  join c in db.Contacts on b.Contact equals c.ContactID into cus
                                  from c in cus.DefaultIfEmpty()
                                  join j in db.Mobiles on b.Contact equals j.Contact into mobi
                                  from j in mobi.DefaultIfEmpty()
                                  where (j.MobileNum.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = j.MobileNum, //each json object will have 
                                      id = c.ContactID
                                  })
                                  .OrderBy(b => b.text).ToList();

            }
            else
            {
                serialisedJson = (from b in db.Contractors
                                  join c in db.Contacts on b.Contact equals c.ContactID into cus
                                  from c in cus.DefaultIfEmpty()
                                  join j in db.Mobiles on b.Contact equals j.Contact into mobi
                                  from j in mobi.DefaultIfEmpty()
                                  select new SelectFormat
                                  {
                                      text = j.MobileNum, //each json object will have 
                                      id = c.ContactID
                                  })
                                  .OrderBy(b => b.text).ToList();

            }//
            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }

        public JsonResult SearchPhone(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.Contractors
                                  join c in db.Contacts on b.Contact equals c.ContactID into cus
                                  from c in cus.DefaultIfEmpty()
                                  where (c.Phone.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = c.Phone, //each json object will have 
                                      id = c.ContactID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from b in db.Contractors
                                  join c in db.Contacts on b.Contact equals c.ContactID into cus
                                  from c in cus.DefaultIfEmpty()
                                  select new SelectFormat
                                  {
                                      text = c.Phone, //each json object will have 
                                      id = c.ContactID
                                  })
                                  .OrderBy(b => b.text).ToList();

            }//
            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }

        //[QkAuthorize(Roles = "Dev,Create Contractor")]
        public ActionResult AddContractor()
        {
            var emp = db.Employees.Select(s => new
            {
                Id = s.EmployeeId,
                Name = s.FirstName + " " + s.LastName,
            }).ToList();
            ViewBag.Empl = QkSelect.List(emp, "Id", "Name");

            ViewBag.CustName = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = false, Text = null, Value = null},
                           }, "Value", "Text", 1);

            var PredefinedCity = db.EnableSettings.Where(a => a.EnableType == "PredefinedCity").FirstOrDefault();
            ViewBag.CityCheck = PredefinedCity != null ? PredefinedCity.Status : Status.inactive;

            var ToReceipt = db.EnableSettings.Where(a => a.EnableType == "BillToBillReceipt").FirstOrDefault();
            var BillTo = ToReceipt != null ? (ToReceipt.Status == Status.active ? 0 : 1) : 1;
            ViewBag.BillToReceipt = BillTo;

            var EnableCRM = db.EnableSettings.Where(a => a.EnableType == "EnableCRM").FirstOrDefault();
            var EnableCRMs = EnableCRM != null ? EnableCRM.Status : Status.inactive;
            ViewBag.EnableCRM = EnableCRMs;


            if (ViewBag.CityCheck == Status.active)
            {
                var city = db.Cities.Select(s => new
                {
                    Id = s.CityName,
                    CityName = s.CityName
                }).ToList();
                ViewBag.SCity = QkSelect.List(city, "Id", "CityName");
            }

            var use = db.Employees
                        .Select(s => new
                        {
                            ID = s.EmployeeId,
                            Name = s.FirstName + " " + s.LastName
                        })
                        .ToList();
            ViewBag.users = QkSelect.List(use, "ID", "Name");
            ContractorViewModel vmodel = new ContractorViewModel();
            vmodel.ContractorCode = CustCode();
            vmodel.Section = "Accounts";

            var cont = db.ContractTypes
                        .Select(s => new
                        {
                            ID = s.ID,
                            Name = s.Name
                        })
                        .ToList();
            ViewBag.ContType = QkSelect.List(cont, "ID", "Name");
            return PartialView(vmodel);
        }

        [HttpGet]
        public JsonResult ContractorCheck(string Contractor, long? cusid)
        {
            var CusCheck = db.Contractors.Where(x => x.ContractorName == Contractor).Any();
            var rslt = false;
            if (CusCheck == true)
            {
                if (cusid != 0)
                {
                    var cust = db.Contractors.Where(x => x.ContractorName == Contractor).FirstOrDefault();
                    CusCheck = (cust.ContractorID == cusid) ? false : true;
                }
            }
            rslt = (CusCheck) ? true : false;
            return Json(rslt);
        }

        private long GetEntryNo()
        {
            Int64 ENo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "Contractor").Select(a => a.number).FirstOrDefault();
            if ((db.Contractors.Select(p => p.EntryNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                if (number == 0)
                {
                    ENo = 1;
                }
                else
                {
                    ENo = number;
                }
            }
            else
            {
                ENo = db.Contractors.Max(p => p.EntryNo + 1);
            }
            return ENo;
        }

        private string CustCode(Int64 CNo = 0, string CCode = null)
        {
            var prefix = db.CodePrefixs.Where(a => a.section == "Contractor").Select(a => a.prefix).FirstOrDefault();

            if (CCode == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "Contractor").Select(a => a.number).FirstOrDefault();
                if ((db.Contractors.Select(p => p.EntryNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    if (number == 0)
                    {
                        CCode = prefix + 1;
                    }
                    else
                    {
                        CCode = prefix + number;
                    }
                }
                else
                {
                    CNo = db.Contractors.Max(p => p.EntryNo + 1);
                    CCode = prefix + CNo;
                    if (CodeExist(CCode))
                    {
                        CCode = CustCode(CNo, CCode);
                    }

                }
            }
            else
            {
                CNo = CNo + 1;
                CCode = prefix + CNo;
                if (CodeExist(CCode))
                {
                    CCode = CustCode(CNo, CCode);
                }
            }
            return CCode;
        }
        private bool CodeExist(string Code)
        {
            var Exists = db.Contractors.Any(c => c.ContractorCode == Code);
            bool res = (Exists) ? true : false;
            return res;
        }

        [HttpGet]
        public JsonResult GetDocument(long CnId, string purpose)
        {
            var ConD = (from a in db.PropertyDocumentTypes
                        join b in db.DocumentFiles on a.ID equals b.Document into che
                        from b in che.DefaultIfEmpty()
                        join c in db.DocumentTypes on a.DocumentType equals c.ID into doc
                        from c in doc.DefaultIfEmpty()
                        where a.Reference == CnId && a.Purpose == purpose
                        select new
                        {
                            AttAch = b.attachments,
                            type = c.Name,
                            Value = c.ID,
                            Id = a.ID,
                            Date = a.ExpDate,
                            Purpose = purpose + "_",
                        }).ToList();
            return Json(ConD);
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