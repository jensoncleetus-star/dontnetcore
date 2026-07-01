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
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Text;
namespace QuickSoft.Areas.Property.Controllers
{
    [Microsoft.AspNetCore.Mvc.Area("Property")]
    public class TenancyContractController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public TenancyContractController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Property/TenancyContract
        public ActionResult Index()
        {


            var emps = db.Employees.Select(o => o.EmployeeId).ToList().ToArray();
          //  var vehileempids = db.vehicleupdations.Where(o => o.vehicleid == vmodel.vehicleid).Select(o => o.employeeid).Distinct().ToList().ToArray();
           //emps = emps.Concat(vehileempids).ToArray();
            if (1==1)
            {
                db.Reminders.RemoveRange(db.Reminders.Where(o => o.Note.Contains("Tenancy Contract Expired")));
                db.SaveChanges();
                db.ReminderAssigneds.RemoveRange(db.ReminderAssigneds.Where(o =>  o.Type == "tenancyenotification"));
                db.SaveChanges();


                DateTime expaired = System.DateTime.Now.Date.AddMonths(-3);
                DateTime reminder = System.DateTime.Now.Date.AddMonths(1);

                var vm = db.Set<TenancyContractExpiryDto>()
                    .FromSqlInterpolated($@"SELECT a.Id,
                                           b.CustomerName,
                                           TRY_CONVERT(datetime, a.EndDate, 103) AS EndDate
                                      FROM TenancyContracts a
                                      LEFT JOIN Customers b ON a.Tenant = b.CustomerID
                                     WHERE TRY_CONVERT(datetime, a.EndDate, 103) >= {expaired}
                                       AND TRY_CONVERT(datetime, a.EndDate, 103) <= {reminder}")
                    .ToList();
               var userid = User.Identity.GetUserId();
                foreach (var k in vm)
                {
                    
                    //db.vehiclereminder.RemoveRange(db.vehiclereminder.Where(o => o.vehicleid == v.vehicleid && o.note == k.note));
                    //db.SaveChanges();
                    if (1==1)
                    {
                        
                      


                        if (1 == 1)
                        {
                          //  var currkm = db.vehicleupdations.Where(o => o.vehicleid == v.vehicleid).OrderByDescending(o => o.createddate).Select(o => o.readings).FirstOrDefault();
                            if (1==1)
                            {

                                Reminder reminds = new Reminder();
                                reminds.Reference = k.Id;
                                reminds.Note = "Tenancy Contract Expired/or expair soon  , End Date " + k.EndDate + "<br> Tenant : " + k.CustomerName;

                             
                                //seleted date added,for fullcalender
                                


                                reminds.RDate = System.DateTime.Now;
                                reminds.Type = "/property/TenancyContract/edit/" + k.Id;
                                reminds.RStatus = "Close";
                                reminds.RequestBy = userid;

                                reminds.CreatedBy = userid;
                                reminds.Status = Status.active;
                                reminds.CreatedDate = System.DateTime.Now;
                                db.Reminders.Add(reminds);
                                db.SaveChanges();
                                long Id = reminds.ReminderId;
                                foreach (var arr in emps)
                                {
                                    //   com.remideradd("/proTask/mytask/details/" + proId, arr, UserId, "Task Notification <br> Task Status : "+statusname + "<br> Task name : "+task.TaskCode+ " - "+task.TaskName, task.ProTaskId);



                                    if (1 == 1)
                                    {
                                        ReminderAssigned remAs = new ReminderAssigned();

                                        remAs.ReminderId = Id;
                                        remAs.EntryId = k.Id;
                                        remAs.Type = "tenancyenotification";
                                        remAs.EmployeeId = arr;
                                        db.ReminderAssigneds.Add(remAs);
                                        db.SaveChanges();
                                    }

                                }
                            }
                        }
                    }




















                }
            }





















            ViewBag.Alldata = QkSelect.List(
              new List<SelectListItem>
              {
                 new SelectListItem { Selected = true, Text = "All", Value = "0"},
              }, "Value", "Text", 0);
            return View();
        }
        public ActionResult GetTenancyContract(long? Property, long? Tenant, long? Unit, string FromDate, string ToDate)
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
            DateTime? fdate = null;
            DateTime? tdate = null;
            if (FromDate != "")
            {
                fdate = DateTime.Parse(FromDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (ToDate != "")
            {
                tdate = DateTime.Parse(ToDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            var UserView = (from a in db.TenancyContracts
                            join b in db.PropertyMains on a.Property equals b.Id into protype
                            from b in protype.DefaultIfEmpty()
                            join c in db.Customers on a.Tenant equals c.CustomerID into doc
                            from c in doc.DefaultIfEmpty()
                            join d in db.PropertyUnits on a.Unit equals d.Id into uni
                            from d in uni.DefaultIfEmpty()
                            join e in db.Durations on a.Duration equals e.Id into dur
                                            from e in dur.DefaultIfEmpty()
                                            where
                                             (string.IsNullOrEmpty(FromDate) || (fdate.HasValue && EF.Functions.DateDiffDay(a.CreatedDate, fdate.Value) <= 0)) &&
                                             (string.IsNullOrEmpty(ToDate) || (tdate.HasValue && EF.Functions.DateDiffDay(a.CreatedDate, tdate.Value) >= 0)) &&
                                            //(Feature == 0 || d.Feature == Feature) &&
                                            (Property == 0 || Property == null || a.Id == Property)
                                            && (Tenant == 0 || Tenant == null || a.Tenant == Tenant)
                                            && (Unit == 0 || Unit == null || a.Unit == Unit)
                                            select new
                            {
                                id = a.Id,
                                Property = b.Name,
                                Tanant = c.CustomerName,
                                Unit = d.Name,
                                Duration = e.Name,
                                                StartDate=a.StartDate,
                                                EndDate=a.EndDate,
                               date = a.CreatedDate
                            });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(p => p.Tanant.ToString().ToLower().Contains(search.ToLower()));
            }
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                try { UserView = UserView.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir); } catch { /* grid column name not in projection - keep default order */ }
            }
            recordsTotal = UserView.Count();
            var data = UserView.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });


        }
        public ActionResult Create()
        {
            ViewbagSet();
            ViewBag.LastEntry = db.TenancyContracts.Select(p => p.Id).AsEnumerable().DefaultIfEmpty(0).Max();
            TenancyContractViewModel vmodel = new TenancyContractViewModel();
            vmodel.Code = CustCode();
            vmodel.Section = "Tenancy Contract";
            vmodel.EndDate = (System.DateTime.Now).ToString("dd-MM-yyyy");
            ViewBag.Customr = QkSelect.List(
                      new List<SelectListItem>
                      {
                                 new SelectListItem { Selected = false, Text = "", Value = ""},
                      }, "Value", "Text", 0);
            return View(vmodel);
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, Create PayrollUnits")]
        public JsonResult Create(TenancyContractViewModel vmodel, string fnval)
        {
            bool stat = false;
            string msg = "";
            int count = 0;

            if (ModelState.IsValid)
            {
                if (vmodel.PaymentType == 2)
                {
                    foreach (var arr in vmodel.cheqmodel)
                    {
                        if (arr.Amount != null && arr.Bank == null)
                        {
                            msg = "Bank name is empty.";
                            stat = false;
                            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                        }
                    }
                }
                var today = Convert.ToDateTime(System.DateTime.Now);
                var UserId = User.Identity.GetUserId();
                long Branch = 0;
                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
                if (BranchCheck == Status.active)
                {
                    Branch = 0;
                }
                else
                {
                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                }
                var Entry = (db.TenancyContracts.Count() > 0) ? db.TenancyContracts.Select(c => c.EntryNo).Max() : 0;

                var proptype = new TenancyContract
                {
                    Tenant = vmodel.Tenant,
                    Property = vmodel.Property,


                    Duration = vmodel.Duration,
                    Unit = (vmodel.Unit) != null ? (long)vmodel.Unit : 0,
                    StartDate = DateTime.Parse(vmodel.StartDate.ToString(), new CultureInfo("en-GB")),
                    contractvalue=vmodel.contractvalue,
                    issuedate = DateTime.Parse(vmodel.issuedate.ToString(), new CultureInfo("en-GB")),
                    PetsAllowed = vmodel.PetsAllowed,
                    WaterAndElectricityBill=vmodel.WaterAndElectricityBill,
                    NumberofOccupants=vmodel.NumberofOccupants,
                    EndDate = DateTime.Parse(vmodel.EndDate.ToString(), new CultureInfo("en-GB")),
                    Rent = vmodel.Rent,
                    Deposit = vmodel.Deposit,
                    DueDate = vmodel.DueDate,
                    PaymentType = vmodel.PaymentType,
                    PaymentTypeDeposit = vmodel.PaymentTypeDeposit,
                    Schedule = vmodel.Schedule,
                    CreatedBy = UserId,
                    CreatedDate = today,
                    Status = Status.active,
                    editable = choice.Yes,
                    TnC = vmodel.TermsCondition,
                    Remark = vmodel.Remark,
                    Note = vmodel.Note,
                    Code = vmodel.Code,
                    EntryNo = Entry + 1
                };
                db.TenancyContracts.Add(proptype);
                db.SaveChanges();
                Int64 ID = proptype.Id;


                var TenantAcc = db.Tenants.Where(x => x.TenantID == vmodel.Tenant).Select(y => y.Accounts).FirstOrDefault();
                if (vmodel.PaymentType == 2)
                {
                    count = 0;
                    foreach (var arr in vmodel.cheqmodel)
                    {
                        if (arr.Amount != null)
                        {
                            var cheqdate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));
                            Cheque cheq = new Cheque();
                            cheq.Reference = ID;
                            cheq.Purpose = "TenancyContract";
                            cheq.Amount = (decimal)(arr.Amount);
                            cheq.Date = cheqdate;
                            cheq.ChequeNo = arr.ChequeNo;
                            cheq.Bank = arr.Bank;
                            db.Cheques.Add(cheq);
                            db.SaveChanges();
                            Int64 cheqid = cheq.ID;

                            if (arr.Attachments != null)
                            {
                                if (arr.Attachments != null)
                                {
                                    string storePath = LegacyWeb.MapPath("~/uploads/chequeimage/" + cheqid + arr.Attachments);
                                    if (!Directory.Exists(storePath))
                                        Directory.CreateDirectory(storePath);

                                    // files upload
                                  //  IFormFile file = Request.Form.Files[1];
                                    IFormFile file = Request.Form.Files["cheqmodel[" + count + "].Attachments"];
                                    var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                                    var uploadUrl = LegacyWeb.MapPath("~/uploads/chequeimage/");
                                    file.SaveAs(Path.Combine(uploadUrl, fileNames));

                                    ChequeImage cheqImg = new ChequeImage();
                                    cheqImg.attachments = fileNames;
                                    cheqImg.Cheque = cheqid;
                                    db.ChequeImages.Add(cheqImg);
                                    db.SaveChanges();
                                   
                                }
                            }
                            //add pdc
                            PDC pd = new PDC
                            {
                                PDCDate = cheqdate,
                                PDCType = "TenancyContract",
                                Reference = ID,
                                CheckNo = arr.ChequeNo,
                                Bank = arr.Bank.ToString(),
                                Note = null,
                                RegStatus = choice.No,
                                Status = Status.active,
                                CreatedBy = UserId,
                                CreatedDate = today,
                                Branch = Branch,
                                editable = choice.No,
                                Bills = vmodel.Code,
                                Type = (today == cheqdate) ? 1 : 0
                            };
                            db.PDCs.Add(pd);
                            db.SaveChanges();

                            //Add Account Transactions
                            Company comp = db.companys.Find(1L);
                            if (vmodel.Rent > 0)
                            {
                                //rent amount
                                //com.addAccountTrasaction(0, (decimal)arr.Amount, (long)arr.Bank, "TenancyContract", vmodel.Id, DC.Credit, today, null, null, vmodel.Property, vmodel.Unit);
                                //com.addAccountTrasaction((decimal)vmodel.Rent, 0, TenantAcc, "TenancyContract", vmodel.Id, DC.Debit, today, null, null, vmodel.Property, vmodel.Unit);
                            }
                        }
                        count++;
                    }
                }
                else
                {
                    //Add Account Transactions
                    Company comp = db.companys.Find(1L);
                    if (vmodel.Rent > 0)
                    {
                        //rent amount
                        //com.addAccountTrasaction(0, (decimal)vmodel.Rent, (long)comp.TCAccount, "TenancyContract", vmodel.Id, DC.Credit, today, null, null, vmodel.Property, vmodel.Unit);
                        //com.addAccountTrasaction((decimal)vmodel.Rent, 0, TenantAcc, "TenancyContract", vmodel.Id, DC.Debit, today, null, null, vmodel.Property, vmodel.Unit);
                    }
                }
                if (vmodel.PaymentTypeDeposit == 2)
                {
                    count = 0;
                    foreach (var arr in vmodel.cheqmodeldep)
                    {
                      
                        if (arr.Amount != null)
                        {
                            var cheqdate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));
                            Cheque cheq = new Cheque();
                            cheq.Reference = ID;
                            cheq.Purpose = "TenancyContractDeposit";
                            cheq.Amount = (decimal)(arr.Amount);
                            cheq.Date = cheqdate;
                            cheq.ChequeNo = arr.ChequeNo;
                            cheq.Bank = arr.Bank;
                            db.Cheques.Add(cheq);
                            db.SaveChanges();
                            Int64 cheqid = cheq.ID;

                            if (arr.Attachments != null)
                            {
                                if (arr.Attachments != null)
                                {
                                    string storePath = LegacyWeb.MapPath("~/uploads/chequeimage/" + cheqid + arr.Attachments);
                                    if (!Directory.Exists(storePath))
                                        Directory.CreateDirectory(storePath);

                                    // files upload
                                    IFormFile file = Request.Form.Files["cheqmodeldep[" + count + "].Attachments"];
                                  //  IFormFile file = Request.Form.Files[1];
                                    var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                                    var uploadUrl = LegacyWeb.MapPath("~/uploads/chequeimage/");
                                    file.SaveAs(Path.Combine(uploadUrl, fileNames));

                                    ChequeImage cheqImg = new ChequeImage();
                                    cheqImg.attachments = fileNames;
                                    cheqImg.Cheque = cheqid;
                                    db.ChequeImages.Add(cheqImg);
                                    db.SaveChanges();
                                    
                                }
                            }
                            
                            //add pdc
                            PDC pd = new PDC
                            {
                                PDCDate = cheqdate,
                                PDCType = "TenancyContractDeposit",
                                Reference = ID,
                                CheckNo = arr.ChequeNo,
                                Bank = arr.Bank.ToString(),
                                Note = null,
                                RegStatus = choice.No,
                                Status = Status.active,
                                CreatedBy = UserId,
                                CreatedDate = today,
                                Branch = Branch,
                                editable = choice.No,
                                Bills = vmodel.Code,
                                Type = (today == cheqdate) ? 1 : 0
                            };
                            db.PDCs.Add(pd);
                            db.SaveChanges();
                        }
                        //Add Account Transactions
                        Company comp = db.companys.Find(1L);
                        //deposit amount
                        if (vmodel.Deposit > 0)
                        {
                            //com.addAccountTrasaction(0, Convert.ToDecimal(arr.Amount), (long)arr.Bank, "TenancyContractDeposit", vmodel.Id, DC.Credit, today, null, null, vmodel.Property, vmodel.Unit);
                            //com.addAccountTrasaction(Convert.ToDecimal(arr.Amount), 0, TenantAcc, "TenancyContractDeposit", vmodel.Id, DC.Debit, today, null, null, vmodel.Property, vmodel.Unit);
                        }
                        count++;
                    }

                }
                else
                {
                    //Add Account Transactions
                    Company comp = db.companys.Find(1L);
                    //deposit amount
                    if (vmodel.Deposit > 0)
                    {
                        //com.addAccountTrasaction(0, (decimal)vmodel.Deposit, (long)comp.TCSAccount, "TenancyContractDeposit", vmodel.Id, DC.Credit, today, null, null, vmodel.Property, vmodel.Unit);
                        //com.addAccountTrasaction((decimal)vmodel.Deposit, 0, TenantAcc, "TenancyContractDeposit", vmodel.Id, DC.Debit, today, null, null, vmodel.Property, vmodel.Unit);
                    }
                }

                if (vmodel.docmodel != null)
                {
                    count = 0;
                    foreach (var arr in vmodel.docmodel)
                    {
                        if (arr.Attachments != null && arr.Type != "")
                        {
                            PropertyDocumentType doc = new PropertyDocumentType();
                            doc.DocumentType = Convert.ToInt64(arr.Type);
                            doc.Reference = ID;
                            doc.Purpose = "Tenancy";
                            if (arr.Date == null)
                                arr.Date = "01-01-3000";
                            doc.ExpDate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));
                            db.PropertyDocumentTypes.Add(doc);
                            db.SaveChanges();
                            Int64 docid = doc.ID;

                            if (arr.Attachments != null)
                            {
                                if (arr.Attachments != null)
                                {
                                    string storePath = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "Tenancy_" + docid);
                                    if (!Directory.Exists(storePath))
                                        Directory.CreateDirectory(storePath);

                                    // files upload
                                    IFormFile file = Request.Form.Files["docmodel[" + count + "].Attachments"];
                                    //IFormFile file = Request.Form.Files[0];
                                    var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                                    var uploadUrl = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "Tenancy_" + docid + "/");
                                    file.SaveAs(Path.Combine(uploadUrl, fileNames));

                                    DocumentFile docfile = new DocumentFile();
                                    docfile.attachments = fileNames;
                                    docfile.Document = docid;
                                    db.DocumentFiles.Add(docfile);
                                    db.SaveChanges();
                                 
                                }
                            }
                        }
                        count++;
                    }
                }
                if(vmodel.Rent>0)
                {
                    var dur = vmodel.Schedule;
                    DateTime startmonth = proptype.StartDate;
                    DateTime enddate = proptype.EndDate;
                    long customeraccount = db.Customers.Where(o => o.CustomerID == vmodel.Tenant).Select(o => o.Accounts).FirstOrDefault();
                   
                    //if (vmodel.Deposit != null)
                    //    com.addAccountTrasaction(0, (decimal)vmodel.Deposit, customeraccount, "Opening Balance", customeraccount, DC.Credit);

                    //while (startmonth<enddate)
                    //{
                    //   // com.addAccountTrasaction(0, (decimal)vmodel.Rent, customeraccount, "Rent Recievable", 0, DC.Credit, startmonth);
                    //    if(dur==Schedule.Monthly)
                    //    {
                    //        startmonth = startmonth.Date.AddMonths(1);
                    //    }
                    //    else if(dur==Schedule.Month3)
                    //    {
                    //        startmonth = startmonth.Date.AddMonths(3);
                    //    }
                    //    else if (dur == Schedule.Month6)
                    //    {
                    //        startmonth = startmonth.Date.AddMonths(6);
                    //    }
                    //    else if (dur == Schedule.Yearly)
                    //    {
                    //        startmonth = startmonth.Date.AddMonths(12);
                    //    }
                    //    ReferenceAccountViewModel arr = new ReferenceAccountViewModel
                    //    {
                    //        Account = customeraccount,
                    //        Amount = vmodel.Rent,
                    //        Invoice = GetSeNo("").ToString(),
                           
                    //        RADate = startmonth.ToString("dd-MM-yyyy"),
                    //        Type = "Rent Receivable"


                    //    };
                    //    if (startmonth < enddate)
                    //    {
                    //        InsertToSales(arr, (long)vmodel.Tenant, UserId, 1);
                    //    }
                    //}
                    
                }
                com.addlog(LogTypes.Created, UserId, "TenancyContract", "TenancyContracts", findip(), ID, "Tenancy Contract Added Successfully");
                if ((fnval) == "print")
                {
                    //var datfa = vmodel;
                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";
                    var prodata = vmodel;
                    var Data = (from a in db.TenancyContracts
                                join b in db.PropertyMains on a.Property equals b.Id into protype
                                from b in protype.DefaultIfEmpty()
                                join c in db.Tenants on a.Tenant equals c.TenantID into doc
                                from c in doc.DefaultIfEmpty()
                                join d in db.PropertyUnits on a.Unit equals d.Id into uni
                                from d in uni.DefaultIfEmpty()
                                join e in db.Durations on a.Duration equals e.Id into dur
                                from e in dur.DefaultIfEmpty()
                                join f in db.Landlords on b.LandlordID equals f.LandlordID into lands
                                from f in lands.DefaultIfEmpty()
                                join fac in db.Accountss on f.Accounts equals fac.AccountsID into landsac
                                from fac in landsac.DefaultIfEmpty()
                                join g in db.Customers on a.Tenant equals g.CustomerID into cust
                                from g in cust.DefaultIfEmpty()
                                where a.Id == vmodel.Id
                                select new
                                {

                                    id = a.Id,
                                    Property = b.Name,
                                    b.Address,
                                    b.City,
                                    b.Code,
                                    a.StartDate,
                                    a.EndDate,
                                    b.PropertyRegistrationNo,
                                    b.Municipality,
                                    b.PlotAddress,
                                    b.PlotNo,
                                    a.contractvalue,
                                    b.RoadName,
                                    b.Sector,
                                    b.State,
                                    b.Zip,
                                    b.Zone,
                                    UnitUsage = (d == null) ? "" : d.UnitUsage,
                                    UnitName = (d == null) ? "" : d.Name,
                                    d.NoofRooms,
                                    uentryno = (d == null) ? 0 : d.EntryNo,
                                    ucode = (d == null) ? "" : d.Code,
                                    contractentryno = a.EntryNo,
                                    contractcode = (a == null) ? "" : a.Code,

                                    Tanant = g.CustomerName,
                                    landlord = f.LandlordName,
                                    lcontacts = (from lco in db.Contacts

                                                 where lco.ContactID == f.Contact
                                                 select lco
                                                 ).FirstOrDefault(),
                                    landtrn = (fac == null) ? "" : fac.TRN,
                                    cuscontacts = (from cusco in db.Contacts
                                                   join cr in db.ContactRelation
                                                   on new { cusco.ContactID, RelationType = (long)ContctRelation.Customer }
                                                  equals new { cr.ContactID, cr.RelationType }
                                                   where (cr.RelationID == g.CustomerID)
                                                   select new
                                                   {

                                                       ContactID = cusco.ContactID
                                                        ,
                                                       Name = cusco.Name
                                                       ,
                                                       FirstName = cusco.FirstName,
                                                       LastName = cusco.LastName,
                                                       Address = cusco.Address
                                                       ,
                                                       Country = cusco.Country
                                                       ,
                                                       State = cusco.State
                                                       ,
                                                       City = cusco.City
                                                       ,
                                                       Zip = cusco.Zip
                                                       ,
                                                       Phone = cusco.Phone
                                                       ,
                                                       Mobile = cusco.Mobile
                                                       ,
                                                       Fax = cusco.Fax
                                                       ,
                                                       EmailId = cusco.EmailId
                                                       ,
                                                       Reference = cusco.Reference
                                                       ,
                                                       ContactPerson = cusco.ContactPerson
                                                       ,
                                                       Status = cusco.Status
                                                       ,
                                                       Group = cusco.Group
                                                       ,
                                                       SalesPMob = cusco.SalesPMob
                                                       ,
                                                       TypeOfContact = cusco.TypeOfContact
                                                       ,
                                                       Website = cusco.Website
                                                       ,
                                                       CountryID = cusco.CountryID
                                                       ,
                                                       ContactTypeID = cusco.ContactTypeID
                                                   }).FirstOrDefault(),
                                    Unit = (d == null) ? "" : d.Name,
                                    Duration = (e == null) ? "" : e.Name,

                                    a.issuedate,
                                    a.PetsAllowed,
                                    a.WaterAndElectricityBill,
                                    a.NumberofOccupants,
                                    date = a.CreatedDate,
                                    a.Deposit,
                                    a.DueDate,
                                    a.Note,
                                    a.Rent,
                                    a.TnC,
                                    a.Remark,
                                    PaymentType = (a.PaymentType == 1) ? "Cash" : (a.PaymentType == 3) ? "Bank Transfer" : "Cheque",


                                    a.Schedule
                                }).FirstOrDefault();
              

                    var cheque = (from a in db.Cheques
                                  where a.Reference == ID && a.Purpose == "TenancyContract"
                                  select new
                                  {
                                      chequeno = a.ChequeNo,
                                      a.Amount,
                                      a.Date
                                  }).ToList();

                var seccheque = (from a in db.Cheques
                             where a.Reference == ID && a.Purpose == "TenancyContractDeposit"
                                 select new
                             {
                                 chequeno = a.ChequeNo,
                                 a.Amount,
                                 a.Date
                             }).ToList();

                    var docs = (from a in db.PropertyDocumentTypes
                                join b in db.DocumentTypes on a.DocumentType equals b.ID into protype
                                from b in protype.DefaultIfEmpty()
                                where a.Reference == ID && a.Purpose == "Tenancy"
                                select new
                                {
                                    b.Name,
                                    a.ExpDate,
                                }).ToList();
                    //var fmapp = db.FieldMappings.Where(a => a.Section == "PropertyRegistration" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                    msg = "Successfully added Tenancy Contract details.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Data, cheque, seccheque, docs, ComHeadCheck } };
                    //}
                }
                else
                {
                    msg = "Successfully added Tenancy Contract details.";
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
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TenancyContract tc = db.TenancyContracts.Find(id);
            var cust = (from a in db.TenancyContracts
                        join b in db.Customers on a.Tenant equals b.CustomerID
                        select new
                        {
                            id = b.CustomerID,
                            text = b.CustomerName
                        }).ToList();
            ViewBag.Customr = QkSelect.List(
              cust, "id", "text");
            if (tc == null)
            {
                return NotFound();
            }
            TenancyContractViewModel vmodel = new TenancyContractViewModel();
            companySet();
            vmodel.Id = (long)id;
            vmodel.Tenant = tc.Tenant;
            vmodel.Property = tc.Property;
            vmodel.Unit = tc.Unit;
            vmodel.Code = tc.Code;
            vmodel.Duration = tc.Duration;
            vmodel.contractvalue = tc.contractvalue;
            vmodel.EndDate = tc.EndDate.ToString("dd-MM-yyyy");
            vmodel.StartDate = tc.StartDate.ToString("dd-MM-yyyy");
            vmodel.issuedate = (tc.issuedate==null)?"":((DateTime)tc.issuedate).ToString("dd-MM-yyyy");
            vmodel.PetsAllowed = tc.PetsAllowed;
            vmodel.WaterAndElectricityBill = tc.WaterAndElectricityBill;
            vmodel.NumberofOccupants = tc.NumberofOccupants;
            vmodel.Rent = tc.Rent;
            vmodel.Deposit = tc.Deposit;
            vmodel.DueDate = tc.DueDate;
            vmodel.PaymentType = tc.PaymentType;
            vmodel.PaymentTypeDeposit = tc.PaymentTypeDeposit;
            vmodel.Schedule = tc.Schedule;
            vmodel.Section = "Tenancy Contract";
            vmodel.Note = tc.Note;
            vmodel.Remark = tc.Remark;
            vmodel.TermsCondition = tc.TnC;
            ViewBag.preEntry = db.TenancyContracts.Where(a => a.Id < id).Select(a => a.Id).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.TenancyContracts.Where(a => a.Id > id).Select(a => a.Id).DefaultIfEmpty().Min();

            ViewbagSet();

            return View(vmodel);
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, Create TenancyContract")]
        public JsonResult Update(TenancyContractViewModel vmodel, string fnval)
        {
            bool stat = false;
            string msg = "";
            int count = 0;

            if (ModelState.IsValid)
            {
                if (vmodel.PaymentType == 2 && vmodel.cheqmodel!=null)
                {
                    foreach (var arr in vmodel.cheqmodel)
                    {
                        if (arr.Amount != null && arr.Bank == null)
                        {
                            //msg = "Bank name is empty.";
                            //stat = false;
                            //return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                        }
                    }
                }
                //var Exists = db.TenancyContracts.Any(c => c.Tenant == vmodel.Tenant && c.ID != vmodel.ID);
                //if (Exists)
                //{
                //    msg = "Type Name already exists.";
                //    stat = false;
                //}
                //else
                //{
                var today = Convert.ToDateTime(System.DateTime.Now);
                var UserId = User.Identity.GetUserId();
                long Branch = 0;
                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
                if (BranchCheck == Status.active)
                {
                    Branch = 0;
                }
                else
                {
                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                }
                TenancyContract tenan = db.TenancyContracts.Find(vmodel.Id);
             
                tenan.Tenant = vmodel.Tenant;
                tenan.Property = vmodel.Property;
                tenan.Duration = vmodel.Duration;
                tenan.Unit = (vmodel.Unit) != null ? (long)vmodel.Unit : 0;
                tenan.StartDate = DateTime.Parse(vmodel.StartDate.ToString(), new CultureInfo("en-GB"));
                if (vmodel.issuedate != null)
                    tenan.issuedate = DateTime.Parse(vmodel.issuedate.ToString(), new CultureInfo("en-GB"));
                tenan.PetsAllowed = vmodel.PetsAllowed;
                tenan.WaterAndElectricityBill = vmodel.WaterAndElectricityBill;
                tenan.NumberofOccupants = vmodel.NumberofOccupants;
                tenan.EndDate = DateTime.Parse(vmodel.EndDate.ToString(), new CultureInfo("en-GB"));
                tenan.Deposit = vmodel.Deposit;
                tenan.DueDate = vmodel.DueDate;
                tenan.Rent = vmodel.Rent;
                tenan.Code = vmodel.Code;
                tenan.Schedule = vmodel.Schedule;
                tenan.PaymentType = vmodel.PaymentType;
                tenan.PaymentTypeDeposit = vmodel.PaymentTypeDeposit;
                tenan.Remark = vmodel.Remark;
                tenan.Note = vmodel.Note;
                tenan.TnC = vmodel.TermsCondition;
                tenan.contractvalue = vmodel.contractvalue;
                db.Entry(tenan).State = EntityState.Modified;
                db.SaveChanges();

                bool deletedeposit = com.DeleteAllAccountTransaction("TenancyContractDeposit", vmodel.Id);
                bool delete = com.DeleteAllAccountTransaction("TenancyContract", vmodel.Id);

                Company comp = db.companys.Find(1L);
                var TenantAcc = db.Tenants.Where(x => x.TenantID == vmodel.Tenant).Select(y => y.Accounts).FirstOrDefault();

             //   db.Cheques.RemoveRange(db.Cheques.Where(a => a.Reference == vmodel.Id && a.Purpose == "TenancyContract"));
            //    db.SaveChanges();
                db.PDCs.RemoveRange(db.PDCs.Where(a => a.Reference == vmodel.Id && a.PDCType == "TenancyContract"));
                db.SaveChanges();
             //   db.Cheques.RemoveRange(db.Cheques.Where(a => a.Reference == vmodel.Id && a.Purpose == "TenancyContractDeposit"));
              //  db.SaveChanges();
                db.PDCs.RemoveRange(db.PDCs.Where(a => a.Reference == vmodel.Id && a.PDCType == "TenancyContractDeposit"));
                db.SaveChanges();
                if (vmodel.cheqmodel != null)
                {
                    foreach (var arr in vmodel.cheqmodel)
                    {
                        if (arr.Amount != null)
                        {
                            var cheqdate = arr.Date == null ? System.DateTime.Now : DateTime.Parse(arr.Date, new CultureInfo("en-GB"));// DateTime.Parse(arr.Date, new CultureInfo("en-GB"));
                            Cheque cheq = new Cheque();
                            Int64 cheqid = cheq.ID;
                            if (Convert.ToInt32(arr.ID)==0)
                            {
                                cheq.Reference = vmodel.Id;
                            cheq.Purpose = "TenancyContract";
                            cheq.Amount = (decimal)(arr.Amount);
                            cheq.Date = arr.Date==null?System.DateTime.Now: DateTime.Parse(arr.Date, new CultureInfo("en-GB"));
                            cheq.ChequeNo = arr.ChequeNo;
                            cheq.Bank = arr.Bank;
                            db.Cheques.Add(cheq);
                            db.SaveChanges();
                            cheqid = cheq.ID;

              
                                    string storePath = LegacyWeb.MapPath("~/uploads/chequeimage/" + arr.Attachments);
                                    if (!Directory.Exists(storePath))
                                        Directory.CreateDirectory(storePath);

                      
                                    IFormFile file = Request.Form.Files["cheqmodel["+ count + "].Attachments"];
                                if (file.FileName != "")
                                {
                                    var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                                    var uploadUrl = LegacyWeb.MapPath("~/uploads/chequeimage/");
                                    file.SaveAs(Path.Combine(uploadUrl, fileNames));

                                    ChequeImage cheqImg = new ChequeImage();
                                    cheqImg.attachments = fileNames;
                                    cheqImg.Cheque = cheqid;
                                    db.ChequeImages.Add(cheqImg);
                                    db.SaveChanges();

                                    
                                    cheqid = arr.ID;

                                }

                                }

                        else
                            {
                                cheq = db.Cheques.Find((long)arr.ID);
                                cheq.Reference = vmodel.Id;
                                cheq.Purpose = "TenancyContract";
                                cheq.Amount = (decimal)(arr.Amount);
                                cheq.Date = cheqdate;
                                cheq.ChequeNo = arr.ChequeNo;
                                cheq.Bank = arr.Bank;
                      
                                db.Entry(cheq).State = EntityState.Modified;
                                db.SaveChanges();
                                cheqid = arr.ID;
                                string storePath = LegacyWeb.MapPath("~/uploads/chequeimage/" + arr.Attachments);
                                if (!Directory.Exists(storePath))
                                    Directory.CreateDirectory(storePath);


                                // IFormFile file = Request.Form.Files[count];
                                IFormFile file = Request.Form.Files["cheqmodel[" + count + "].Attachments"];
                                if (file.FileName != "")
                                {
                                    var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                                    var uploadUrl = LegacyWeb.MapPath("~/uploads/chequeimage/");
                                    file.SaveAs(Path.Combine(uploadUrl, fileNames));

                                    ChequeImage cheqImg = db.ChequeImages.Where(o => o.Cheque == arr.ID).FirstOrDefault();
                                    if(cheqImg !=null)
                                    {
                                    cheqImg.attachments = fileNames;
                                    cheqImg.Cheque = cheqid;
                                  

                                    db.Entry(cheqImg).State = EntityState.Modified;
                                    db.SaveChanges();
                                    }
                                    else
                                    {
                                        ChequeImage cheqImg2 = new ChequeImage();
                                        cheqImg2.attachments = fileNames;
                                        cheqImg2.Cheque = cheqid;
                                        db.ChequeImages.Add(cheqImg2);
                                        db.SaveChanges();
                                    }
                                    cheqid = arr.ID;

                                }

                            }

                            PDC pd = new PDC
                            {
                                PDCDate = cheqdate,
                                PDCType = "TenancyContract",
                                Reference = (long)vmodel.Id,
                                CheckNo = arr.ChequeNo,
                                Bank = arr.Bank.ToString(),
                                Note = null,
                                RegStatus = choice.No,
                                Status = Status.active,
                                CreatedBy = UserId,
                                CreatedDate = today,
                                Branch = Branch,
                                editable = choice.No,
                                Bills = vmodel.Code,
                                Type = (today == cheqdate) ? 1 : 0
                            };
                            db.PDCs.Add(pd);
                            db.SaveChanges();





                       

                        //add pdc                            

                        //deposit amount
                        if (vmodel.Rent > 0)
                            {
                                //com.addAccountTrasaction(0, Convert.ToDecimal(arr.Amount), (long)arr.Bank, "TenancyContractDeposit", vmodel.Id, DC.Credit, today, null, null, vmodel.Property, vmodel.Unit);
                                //com.addAccountTrasaction(Convert.ToDecimal(arr.Amount), 0, TenantAcc, "TenancyContractDeposit", vmodel.Id, DC.Debit, today, null, null, vmodel.Property, vmodel.Unit);
                            }
                        }
                        count++;
                    }

                }
                
                else
                {
                    //Add Account Transactions
                    if (vmodel.Rent > 0)
                    {
                        //rent amount
                        //com.addAccountTrasaction(0, (decimal)vmodel.Rent, (long)comp.TCAccount, "TenancyContract", vmodel.Id, DC.Credit, today, null, null, vmodel.Property, vmodel.Unit);
                        //com.addAccountTrasaction((decimal)vmodel.Rent, 0, TenantAcc, "TenancyContract", vmodel.Id, DC.Debit, today, null, null, vmodel.Property, vmodel.Unit);
                    }
                }
                if (vmodel.PaymentTypeDeposit == 2 && vmodel.cheqmodeldep!=null)
                {
                    count = 0;
                    foreach (var arr in vmodel.cheqmodeldep)
                    {
                        if (arr.Amount != null)
                        {



                            var cheqdate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));
                            Cheque cheq = new Cheque();
                    




                            Int64 cheqid = cheq.ID;
                            if (Convert.ToInt32(arr.ID)==0)
                            {
                                cheq.Reference = vmodel.Id;
                                cheq.Purpose = "TenancyContractDeposit";
                                cheq.Amount = (decimal)(arr.Amount);
                                cheq.Date = cheqdate;
                                cheq.Bank = arr.Bank;
                                cheq.ChequeNo = arr.ChequeNo;
                                db.Cheques.Add(cheq);
                                db.SaveChanges();
                                cheqid = cheq.ID;


                                string storePath = LegacyWeb.MapPath("~/uploads/chequeimage/" + arr.Attachments);
                                if (!Directory.Exists(storePath))
                                    Directory.CreateDirectory(storePath);


                                 IFormFile file = Request.Form.Files["cheqmodeldep[" + count + "].Attachments"];
                                if (file.FileName != "")
                                {
                                    var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                                    var uploadUrl = LegacyWeb.MapPath("~/uploads/chequeimage/");
                                    file.SaveAs(Path.Combine(uploadUrl, fileNames));




                                    ChequeImage cheqImg = new ChequeImage();
                                    cheqImg.attachments = fileNames;
                                    cheqImg.Cheque = cheqid;
                                    db.ChequeImages.Add(cheqImg);
                                    db.SaveChanges();

                                  
                                    cheqid = arr.ID;

                                }
                            }

                            else
                            {
                                cheq = db.Cheques.Find((long)arr.ID);
                     

                                cheq.Reference = vmodel.Id;
                                cheq.Purpose = "TenancyContractDeposit";
                                cheq.Amount = (decimal)(arr.Amount);
                                cheq.Date = cheqdate;
                                cheq.Bank = arr.Bank;
                                cheq.ChequeNo = arr.ChequeNo;

                                db.Entry(cheq).State = EntityState.Modified;
                                db.SaveChanges();
                                cheqid = arr.ID;
                                string storePath = LegacyWeb.MapPath("~/uploads/chequeimage/" + arr.Attachments);
                                if (!Directory.Exists(storePath))
                                    Directory.CreateDirectory(storePath);


                                  IFormFile file = Request.Form.Files["cheqmodeldep[" + count + "].Attachments"];

                                if (file.FileName != "")
                                {
                                    var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                                    var uploadUrl = LegacyWeb.MapPath("~/uploads/chequeimage/");
                                    file.SaveAs(Path.Combine(uploadUrl, fileNames));

                                    ChequeImage cheqImg = db.ChequeImages.Where(o => o.Cheque == arr.ID).FirstOrDefault();
                                    if (cheqImg != null)
                                    {
                                        cheqImg.attachments = fileNames;
                                        cheqImg.Cheque = cheqid;
                                        cheqid = arr.ID;
                                        db.Entry(cheqImg).State = EntityState.Modified;
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        ChequeImage cheqImg2 = new ChequeImage();
                                        cheqImg2.attachments = fileNames;
                                        cheqImg2.Cheque = cheqid;
                                        db.ChequeImages.Add(cheqImg2);
                                        db.SaveChanges();

                                    }
                                }

                            }









































                         
                            //add pdc
                            PDC pd = new PDC
                            {
                                PDCDate = cheqdate,
                                PDCType = "TenancyContractDeposit",
                                Reference = vmodel.Id,
                                CheckNo = arr.ChequeNo,
                                Bank = arr.Bank.ToString(),
                                Note = null,
                                RegStatus = choice.No,
                                Status = Status.active,
                                CreatedBy = UserId,
                                CreatedDate = today,
                                Branch = Branch,
                                editable = choice.No,
                                Bills = vmodel.Code,
                                Type = (today == cheqdate) ? 1 : 0
                            };
                            db.PDCs.Add(pd);
                            db.SaveChanges();
                            if (vmodel.Deposit > 0)
                            {
                                //com.addAccountTrasaction(0, (decimal)vmodel.Deposit, (long)arr.Bank, "TenancyContractDeposit", vmodel.Id, DC.Credit, today, null, null, vmodel.Property, vmodel.Unit);
                                //com.addAccountTrasaction((decimal)vmodel.Deposit, 0, TenantAcc, "TenancyContractDeposit", vmodel.Id, DC.Debit, today, null, null, vmodel.Property, vmodel.Unit);
                            }
                        }
                        //Add Account Transactions
                        //deposit amount
                   
                        count++;
                    }

                }
                else
                {
                    //Add Account Transactions
                    //deposit amount
                    if (vmodel.Deposit > 0)
                    {
                        //com.addAccountTrasaction(0, (decimal)vmodel.Deposit, (long)comp.TCSAccount, "TenancyContractDeposit", vmodel.Id, DC.Credit, today, null, null, vmodel.Property, vmodel.Unit);
                        //com.addAccountTrasaction((decimal)vmodel.Deposit, 0, TenantAcc, "TenancyContractDeposit", vmodel.Id, DC.Debit, today, null, null, vmodel.Property, vmodel.Unit);
                    }
                }

                if (vmodel.docmodel != null)
                {
                    // db.PropertyDocumentTypes.RemoveRange(db.PropertyDocumentTypes.Where(a => a.Reference == vmodel.Id && a.Purpose == "Tenancy"));
                    //  db.SaveChanges();
                    count = 0;

                    foreach (var arr in vmodel.docmodel)
                    {

                 

                      


                            Cheque cheq = new Cheque();
                            Int64 cheqid = cheq.ID;

                            PropertyDocumentType doc = new PropertyDocumentType();



                            Int64 docid;






                    

                        if (Convert.ToInt32(arr.ID)==0 && arr.Type!= null)
                            {

                           // var cheqdate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));
                            doc.DocumentType = Convert.ToInt64(arr.Type);
                                doc.Purpose = "Tenancy";
                                doc.Reference = vmodel.Id;
                            if (arr.Date != null)
                                doc.ExpDate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));
                            else
                                arr.Date = null;
                            db.PropertyDocumentTypes.Add(doc);
                                db.SaveChanges();
                                 docid = doc.ID;




                            string storePath = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "Tenancy_" + docid);
                            if (!Directory.Exists(storePath))
                                Directory.CreateDirectory(storePath);


                            IFormFile file = Request.Form.Files["docmodel[" + count + "].Attachments"];
                     
                            if (file.FileName != "")
                                {
                                var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                                var uploadUrl = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "Tenancy_" + docid + "/");
                                file.SaveAs(Path.Combine(uploadUrl, fileNames));

                                DocumentFile docfile = new DocumentFile();
                                docfile.attachments = fileNames;
                                docfile.Document = docid;
                                db.DocumentFiles.Add(docfile);
                                db.SaveChanges();

                                docid = doc.ID;

                            }

                            }

                            else if(arr.Type!=null)
                            {
                            doc = db.PropertyDocumentTypes.Find(arr.ID);

                            doc.DocumentType = Convert.ToInt64(arr.Type);
                            doc.Purpose = "Tenancy";
                            doc.Reference = vmodel.Id;
                            if (arr.Date != null)
                            {
                                doc.ExpDate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));
                            }
                            else
                            {
                                doc.ExpDate = null;
                            }
                            db.PropertyDocumentTypes.Add(doc);
                            db.Entry(doc).State = EntityState.Modified;
                            db.SaveChanges();



                            docid = arr.ID;
                            string storePath = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "Tenancy_" + docid);
                            if (!Directory.Exists(storePath))
                                Directory.CreateDirectory(storePath);

                            // IFormFile file = Request.Form.Files[count];
                            IFormFile file = Request.Form.Files["docmodel[" + count + "].Attachments"];
                            if (file.FileName != "")
                                {
                                var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                                var uploadUrl = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "Tenancy_" + docid + "/");
                                file.SaveAs(Path.Combine(uploadUrl, fileNames));

                              /*  DocumentFile docfile = new DocumentFile();
                                docfile.attachments = fileNames;
                                docfile.Document = docid;
                                db.DocumentFiles.Add(docfile);
                                db.SaveChanges();
                              */

                                DocumentFile docfile = db.DocumentFiles.Where(o => o.Document == arr.ID).FirstOrDefault();
                                if (docfile != null)
                                {
                                    docfile.attachments = fileNames;
                                    docfile.Document = docid;
                                    db.Entry(docfile).State = EntityState.Modified;

                                    db.SaveChanges();
                                }
                                else
                                {
                                    DocumentFile docfile2 = new DocumentFile();
                                    docfile2.attachments = fileNames;
                                    docfile2.Document = docid;
                                    db.DocumentFiles.Add(docfile2);
                                    db.SaveChanges();

                                    docid = doc.ID;
                                }
                                docid = arr.ID;
                            }

                            }

                        







                        
                      
                        count++;

































                      
                      

                    }
                }
                var saleids=db.SalesEntrys.Where(o => o.Customer == vmodel.Tenant).Select(o => o.SalesEntryId).ToList().ToArray();
                db.SalesEntrys.RemoveRange(db.SalesEntrys.Where(o => o.Customer == vmodel.Tenant));
                db.SaveChanges();
                db.SEPayments.RemoveRange(db.SEPayments.Where(o=>saleids.Contains(o.SalesEntry)));
                
                db.SaveChanges();
                db.AccountsTransactions.RemoveRange(db.AccountsTransactions.Where(o => o.Purpose== "Rent Receivable" && saleids.Contains(o.reference)));

                db.SaveChanges();
              //if(vmodel.Rent>0)
              //  { 
              //      var cus=db.Customers.Find(vmodel.Tenant);
             


              //      var dur = vmodel.Schedule;
              //      DateTime startmonth = tenan.StartDate;
              //      DateTime enddate = tenan.EndDate;
              //      long customeraccount = db.Customers.Where(o => o.CustomerID == vmodel.Tenant).Select(o => o.Accounts).FirstOrDefault();
                   
              //      db.AccountsTransactions.RemoveRange(db.AccountsTransactions.Where(o => o.Purpose == "Opening Balance" && o.Account == customeraccount));

              //      db.SaveChanges();
              //      if(vmodel.Deposit!=null)
              //      com.addAccountTrasaction(0,(decimal)vmodel.Deposit, customeraccount, "Opening Balance", customeraccount, DC.Credit);
                   
              //      while (startmonth < enddate)
              //      {
              //          // com.addAccountTrasaction(0, (decimal)vmodel.Rent, customeraccount, "Rent Recievable", 0, DC.Credit, startmonth);
              //          if (dur == Schedule.Monthly)
              //          {
              //              startmonth = startmonth.Date.AddMonths(1);
              //          }
              //          else if (dur == Schedule.Month3)
              //          {
              //              startmonth = startmonth.Date.AddMonths(3);
              //          }
              //          else if (dur == Schedule.Month6)
              //          {
              //              startmonth = startmonth.Date.AddMonths(6);
              //          }
              //          else if (dur == Schedule.Yearly)
              //          {
              //              startmonth = startmonth.Date.AddMonths(12);
              //          }
              //          ReferenceAccountViewModel arr = new ReferenceAccountViewModel
              //          {
              //              Account = customeraccount,
              //              Amount = vmodel.Rent,
              //              Invoice = GetSeNo("").ToString(),

              //              RADate = startmonth.ToString("dd-MM-yyyy"),
              //              Type = "Rent Receivable"


              //          };
              //          if (startmonth < enddate)
              //          {
              //              InsertToSales(arr, (long)vmodel.Tenant, UserId, 1);
              //          }
              //      }

              //  }
              com.addlog(LogTypes.Created, UserId, "TenancyContract", "TenancyContracts", findip(), vmodel.Id, "Tenancy Contract Updated Successfully");
                if ((fnval) == "print")
                {
                    //var datfa = vmodel;
                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";
                    var prodata = vmodel;
                    var Data = (from a in db.TenancyContracts
                                join b in db.PropertyMains on a.Property equals b.Id into protype
                                from b in protype.DefaultIfEmpty()
                                join c in db.Tenants on a.Tenant equals c.TenantID into doc
                                from c in doc.DefaultIfEmpty()
                                join d in db.PropertyUnits on a.Unit equals d.Id into uni
                                from d in uni.DefaultIfEmpty()
                                join e in db.Durations on a.Duration equals e.Id into dur
                                from e in dur.DefaultIfEmpty()
                                join f in db.Landlords on b.LandlordID equals f.LandlordID into lands
                                from f in lands.DefaultIfEmpty()
                                join fac in db.Accountss on f.Accounts equals fac.AccountsID into landsac
                                from fac in landsac.DefaultIfEmpty()
                                join g in db.Customers on a.Tenant equals g.CustomerID into cust
                                from g in cust.DefaultIfEmpty()
                                where a.Id == vmodel.Id
                                select new
                                {
                                    id = a.Id,
                                    Property = b.Name,
                                    b.Address,
                                    b.City,
                                    b.Code,
                                    a.StartDate,
                                    a.EndDate,
                                    b.PropertyRegistrationNo,
                                    b.Municipality,
                                    b.PlotAddress,
                                    b.PlotNo,
                                    a.contractvalue,
                                    b.RoadName,
                                    b.Sector,
                                    b.State,
                                    b.Zip,
                                    b.Zone,
                                    UnitUsage=(d == null)? "": d.UnitUsage,
                                    UnitName = (d == null) ? "" : d.Name,
                                    d.NoofRooms,
                                    uentryno = (d == null) ? 0 : d.EntryNo,
                                    ucode = (d == null)?"":d.Code,
                                    contractentryno = a.EntryNo,
                                    contractcode = (a==null)?"":a.Code,

                                    Tanant = g.CustomerName,
                                    landlord = f.LandlordName,
                                    lcontacts = (from lco in db.Contacts

                                                 where lco.ContactID == f.Contact
                                                 select lco
                                                 ).FirstOrDefault(),
                                    landtrn = (fac == null) ? "" : fac.TRN,
                                    cuscontacts = (from cusco in db.Contacts
                                                   join cr in db.ContactRelation
                                                   on new { cusco.ContactID, RelationType = (long)ContctRelation.Customer }
                                                  equals new { cr.ContactID, cr.RelationType }
                                                   where (cr.RelationID == g.CustomerID)
                                                   select new
                                                   {

                                                       ContactID = cusco.ContactID
                                                        ,
                                                       Name = cusco.Name
                                                       ,
                                                       FirstName = cusco.FirstName,
                                                       LastName = cusco.LastName,
                                                       Address = cusco.Address
                                                       ,
                                                       Country = cusco.Country
                                                       ,
                                                       State = cusco.State
                                                       ,
                                                       City = cusco.City
                                                       ,
                                                       Zip = cusco.Zip
                                                       ,
                                                       Phone = cusco.Phone
                                                       ,
                                                       Mobile = cusco.Mobile
                                                       ,
                                                       Fax = cusco.Fax
                                                       ,
                                                       EmailId = cusco.EmailId
                                                       ,
                                                       Reference = cusco.Reference
                                                       ,
                                                       ContactPerson = cusco.ContactPerson
                                                       ,
                                                       Status = cusco.Status
                                                       ,
                                                       Group = cusco.Group
                                                       ,
                                                       SalesPMob = cusco.SalesPMob
                                                       ,
                                                       TypeOfContact = cusco.TypeOfContact
                                                       ,
                                                       Website = cusco.Website
                                                       ,
                                                       CountryID = cusco.CountryID
                                                       ,
                                                       ContactTypeID = cusco.ContactTypeID
                                                   }).FirstOrDefault(),
                                    Unit = (d == null) ? "" : d.Name,
                                    Duration = (e == null) ? "" : e.Name,

                                    a.issuedate,
                                    a.PetsAllowed,
                                    a.WaterAndElectricityBill,
                                    a.NumberofOccupants,
                                    date = a.CreatedDate,
                                    a.Deposit,
                                    a.DueDate,
                                    a.Note,
                                    a.Rent,
                                    a.TnC,
                                    a.Remark,
                                    PaymentType = (a.PaymentType == 1) ? "Cash" : (a.PaymentType == 3) ?"Bank Transfer": "Cheque",

                                    a.Schedule
                                }).FirstOrDefault();
                    var cheque = (from a in db.Cheques
                                  where a.Reference == vmodel.Id && a.Purpose == "TenancyContract"
                                  select new
                                  {
                                      chequeno = a.ChequeNo,
                                      a.Amount,
                                      a.Date
                                  }).ToList();

                    var seccheque = (from a in db.Cheques
                                     where a.Reference == vmodel.Id && a.Purpose == "TenancyContractDeposit"
                                     select new
                                     {
                                         chequeno = a.ChequeNo,
                                         a.Amount,
                                         a.Date
                                     }).ToList();

                    var docs = (from a in db.PropertyDocumentTypes
                                join b in db.DocumentTypes on a.DocumentType equals b.ID into protype
                                from b in protype.DefaultIfEmpty()
                                where a.Reference == vmodel.Id && a.Purpose == "Tenancy"
                                select new
                                {
                                    b.Name,
                                    a.ExpDate,
                                }).ToList();
                    //var fmapp = db.FieldMappings.Where(a => a.Section == "PropertyRegistration" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                    msg = "Successfully added Tenancy Contract details.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Data, cheque, seccheque, docs, ComHeadCheck } };
                    //}
                }
                else
                {
                    msg = "Successfully Updated Tenancy Contract details.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Delete Tenant")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteCust(arr) == true) ? count++ : notdel++;
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Tenancy Contracts, Unable to Delete " + notdel + " Tenancy Contracts. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Tenancy Contracts.", true);
            }
            else
            {
                Success("Deleted " + count + " Tenancy Contracts.", true);
            }
            return RedirectToAction("Index", "TenancyContract");
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
        //[Authorize(Roles = "Dev,Delete PropertyType")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TenancyContract ptype = db.TenancyContracts.Find(id);
            if (ptype == null)
            {
                return NotFound();
            }
            return PartialView(ptype);
        }
        [RedirectingAction]
        //[Authorize(Roles = "Dev,Delete PropertyType")]
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
                msg = "Successfully Deleted Tenancy Contract details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public bool DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            db.PropertyDocumentTypes.RemoveRange(db.PropertyDocumentTypes.Where(a => a.Reference == id && a.Purpose == "Tenancy"));
            db.SaveChanges();
            db.Cheques.RemoveRange(db.Cheques.Where(a => a.Reference == id && a.Purpose == "TenancyContract"));
            db.SaveChanges();
            TenancyContract pt = db.TenancyContracts.Find(id);

            db.TenancyContracts.Remove(pt);
            db.SaveChanges();


            db.PDCs.RemoveRange(db.PDCs.Where(o => o.Reference == id && o.PDCType.Contains("Tenancy") ));
            db.SaveChanges();

            bool deletedeposit = com.DeleteAllAccountTransaction("TenancyContractDeposit", id);
            bool delete = com.DeleteAllAccountTransaction("TenancyContract", id);
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "TenancyContract", "TenancyContracts", findip(), pt.Id, "Tenancy Contract Deleted Successfully");
            return true;
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;

            return msg;
        }
        public void ViewbagSet()
        {
            //var tenan = db.Tenants
            //        .Select(s => new
            //        {
            //            ID = s.TenantID,
            //            Name = s.TenantName
            //        })
            //        .ToList();
            //ViewBag.Tenants = QkSelect.List(tenan, "ID", "Name");
            var cust = (from a in db.Customers
                        join b in db.Accountss on a.Accounts equals b.AccountsID

                        where a.Type == CRMCustomerType.Customer &&
                        !a.CustomerName.StartsWith("OLD-")
                        
                        select new
                        {
                            a.CustomerID,
                            a.CustomerCode,
                            a.CustomerName
                        }).ToList().Select(s => new
                        {
                            CustomerID = s.CustomerID,
                            CustomerDetails = s.CustomerCode + " - " + s.CustomerName
                        }).ToList();
            ViewBag.Tenants = QkSelect.List(cust, "CustomerID", "CustomerDetails");
            var prop = (from a in db.PropertyMains
                        join b in db.PropertyTypes on a.PropertyType equals b.ID into cat
                        from b in cat.DefaultIfEmpty()
                        join d in db.PropertyRegistrations on a.Id equals d.Property into protype
                        from d in protype.DefaultIfEmpty()
                        join c in db.Accountss on d.Owner equals c.AccountsID into doc
                        from c in doc.DefaultIfEmpty()
                        select new SelectMultiFormat
                        {
                            text = a.Code + "-" + a.Name,
                            id = a.Id,
                            Name = a.Code + "-" + a.Name
                        })
                    .ToList();
            ViewBag.Propertys = QkSelect.List(prop, "ID", "Name");
            var uni = db.PropertyUnits
                    .Select(s => new
                    {
                        ID = s.Id,
                        Name = s.Name
                    })
                    .ToList();
            ViewBag.Units = QkSelect.List(uni, "ID", "Name");
            var Durat = db.Durations
                    .Select(s => new
                    {
                        ID = s.Id,
                        Name = s.Name
                    })
                    .ToList();
            ViewBag.Durations = QkSelect.List(Durat, "ID", "Name");
            var DocType = db.DocumentTypes
                    .Select(s => new
                    {
                        ID = s.ID,
                        Name = s.Name
                    })
                    .ToList();
            ViewBag.DocumentTypes = QkSelect.List(DocType, "ID", "Name");
            ViewBag.Schedules = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Monthly", Value="1"},
                new SelectListItem() {Text = "3 Month", Value="2"},
                new SelectListItem() {Text = "6 Month", Value="3"},
                new SelectListItem() {Text = "Yearly", Value="4"},
            }, "Value", "Text");
            ViewBag.DueDates = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "1st", Value="1"},
                new SelectListItem() {Text = "2nd", Value="2"},
                new SelectListItem() {Text = "3rd", Value="3"},
                new SelectListItem() {Text = "4th", Value="4"},
                new SelectListItem() {Text = "5th", Value="5"},
                new SelectListItem() {Text = "6th", Value="6"},
                new SelectListItem() {Text = "7th", Value="7"},
                new SelectListItem() {Text = "8th", Value="8"},
                new SelectListItem() {Text = "9th", Value="9"},
                new SelectListItem() {Text = "10th", Value="10"},
                new SelectListItem() {Text = "11th", Value="11"},
                new SelectListItem() {Text = "12th", Value="12"},
                new SelectListItem() {Text = "13th", Value="13"},
                new SelectListItem() {Text = "14th", Value="14"},
                new SelectListItem() {Text = "15th", Value="15"},
                new SelectListItem() {Text = "16th", Value="16"},
                new SelectListItem() {Text = "17th", Value="17"},
                new SelectListItem() {Text = "18th", Value="18"},
                new SelectListItem() {Text = "19th", Value="19"},
                new SelectListItem() {Text = "20th", Value="20"},
                new SelectListItem() {Text = "21st", Value="21"},
                new SelectListItem() {Text = "22nd", Value="22"},
                new SelectListItem() {Text = "23rd", Value="23"},
                new SelectListItem() {Text = "24th", Value="24"},
                new SelectListItem() {Text = "25th", Value="25"},
                new SelectListItem() {Text = "26th", Value="26"},
                new SelectListItem() {Text = "27th", Value="27"},
                new SelectListItem() {Text = "28th", Value="28"},
                new SelectListItem() {Text = "29th", Value="29"},
                new SelectListItem() {Text = "30th", Value="30"},
                new SelectListItem() {Text = "31st", Value="31"},
            }, "Value", "Text");
            ViewBag.PaymentTypes = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Cash", Value="1"},
                new SelectListItem() {Text = "Bank Transfer", Value="3"},
                new SelectListItem() {Text = "Cheque", Value="2"},
            }, "Value", "Text");

            ViewBag.Alldata = QkSelect.List(
              new List<SelectListItem>
              {
                  new SelectListItem { Selected = true, Text = "", Value = ""},
              }, "Value", "Text", 0);
        }

        [HttpGet]
        public JsonResult GetCheque(long CnId)
        {
            var ConD = (from a in db.Cheques
                        join b in db.ChequeImages on a.ID equals b.Cheque into che
                        from b in che.DefaultIfEmpty()
                        join c in db.Accountss on a.Bank equals c.AccountsID into Acc
                        from c in Acc.DefaultIfEmpty()
                        where a.Reference == CnId && a.Purpose == "TenancyContract"
                        select new
                        {
                            ID = a.ID,
                            AttAch = b.attachments,
                            Amt = a.Amount,
                            No = a.ChequeNo,
                            Date = a.Date,
                            Bank = a.Bank,
                            BankName = c.Name
                        }).ToList();
            return Json(ConD);
        }
        [HttpGet]
        public Boolean deletechequedepoistall(long CnId)
        {
            db.Cheques.RemoveRange(db.Cheques.Where(a => a.Reference == CnId && a.Purpose == "TenancyContractDeposit"));
           
            db.SaveChanges();
            return true;
        }
        [HttpGet]
        public Boolean deletechequerentall(long CnId)
        {
            db.Cheques.RemoveRange(db.Cheques.Where(a => a.Reference == CnId && a.Purpose == "TenancyContract"));
            db.SaveChanges();
            return true;
        }

        [HttpGet]
        public JsonResult GetChequeDeposit(long CnId)
        {
            var ConD = (from a in db.Cheques
                        join b in db.ChequeImages on a.ID equals b.Cheque into che
                        from b in che.DefaultIfEmpty()
                        join c in db.Accountss on a.Bank equals c.AccountsID into Acc
                        from c in Acc.DefaultIfEmpty()
                        where a.Reference == CnId && a.Purpose == "TenancyContractDeposit"
                        select new
                        {
                            ID = a.ID,
                            AttAch = b.attachments,
                            Amt = a.Amount,
                            No = a.ChequeNo,
                            Date = a.Date,
                            Bank = a.Bank,
                            BankName = c.Name
                        }).ToList();
            return Json(ConD);
        }
        //[HttpGet]
        //public JsonResult GetDocument(long CnId)
        //{
        //    var ConD = (from a in db.TenancyDocumentTypes
        //                join b in db.DocumentFiles on a.ID equals b.Document into che
        //                from b in che.DefaultIfEmpty()
        //                join c in db.DocumentTypes on a.DocumentType equals c.ID into doc
        //                from c in doc.DefaultIfEmpty()
        //                where a.TenancyContract == CnId
        //                select new
        //                {
        //                    AttAch = b.attachments,
        //                    type = c.Name,
        //                    Value = c.ID,
        //                    Id = a.ID,
        //                }).ToList();
        //    return Json(ConD);
        //}

        private string CustCode(Int64 CNo = 0, string CCode = null)
        {
            var prefix = db.CodePrefixs.Where(a => a.section == "TenancyContract").Select(a => a.prefix).FirstOrDefault();

            if (CCode == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "TenancyContract").Select(a => a.number).FirstOrDefault();
                if ((db.TenancyContracts.Select(p => p.EntryNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
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
                    CNo = db.TenancyContracts.Max(p => p.EntryNo + 1);
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
            var Exists = db.TenancyContracts.Any(c => c.Code == Code);
            bool res = (Exists) ? true : false;
            return res;
        }

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,Download Receipt")]
        public ActionResult Download(long id)
        {
            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            string HFCheck = ComHeadCheck.ToString();

            var Data = db.TenancyContracts.Where(s => s.Id == id).FirstOrDefault();

            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = Data.Code;

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), HFCheck);
            return File(ms, "application/pdf", "Tenancy Contract Voucher" + "-" + billno + ".pdf");

            //var RecDet = db.Receipts.Where(s => s.ReceiptId == id).FirstOrDefault();
            //var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            //var billno = RecDet.VoucherNo;
            //SendMail sm = new SendMail();
            //byte[] ms = sm.DownloadPdf(generatePdf(id), HFCheck);
            //return File(ms, "application/pdf", "Receipt Voucher" + "-" + billno + ".pdf");
        }

        // Custom branded, print-ready Tenancy Contract document (standalone page -> browser print / PDF).
        // Read-only; every piece materialized before shaping (EF Core 10 safe).
        [HttpGet]
        public ActionResult Print(long id)
        {
            var c = db.TenancyContracts.Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id, x.Code, x.Tenant, x.Property, x.Unit, x.StartDate, x.EndDate, x.issuedate,
                    x.Rent, x.Deposit, x.Duration, x.Schedule, x.DueDate, x.PaymentType,
                    x.contractvalue, x.NumberofOccupants, x.WaterAndElectricityBill, x.PetsAllowed,
                    x.TnC, x.Remark, x.Note, x.CreatedDate
                }).FirstOrDefault();
            if (c == null) return NotFound();

            var comp = db.companys.Select(x => new { x.CPName, x.CPAddress, x.CPPhone, x.CPEmail, x.TRN }).FirstOrDefault();
            var hdr = db.CompanyHeaders.Select(h => h.Header).FirstOrDefault();

            long propId = c.Property ?? 0;
            var prop = db.PropertyMains.Where(p => p.Id == propId)
                         .Select(p => new { p.Name, p.Code, p.City, p.State, p.Address, p.LandlordID }).FirstOrDefault();
            long llId = prop != null ? (prop.LandlordID ?? 0) : 0;
            var landlord = (from l in db.Landlords
                            where l.LandlordID == llId
                            join ct in db.Contacts on l.Contact equals ct.ContactID into cc from ct in cc.DefaultIfEmpty()
                            select new { l.LandlordName, ct.Mobile, ct.Phone, ct.EmailId, ct.Address }).FirstOrDefault();
            long tnId = c.Tenant ?? 0;
            // The lessee picker was switched from Tenants to Customers, so Tenant now holds a CustomerID.
            // Resolve the customer first (current behaviour); fall back to the legacy Tenants table for
            // any older contracts that still reference a real TenantID. Both projections use the same
            // anonymous shape so the fallback is interchangeable.
            var customer = (from cu in db.Customers
                            where cu.CustomerID == tnId
                            join ct in db.Contacts on cu.Contact equals ct.ContactID into cc from ct in cc.DefaultIfEmpty()
                            select new { Name = cu.CustomerName, ct.Mobile, ct.Phone, ct.EmailId, ct.Address }).FirstOrDefault();
            var tenant = customer ?? (from t in db.Tenants
                          where t.TenantID == tnId
                          join ct in db.Contacts on t.Contact equals ct.ContactID into cc from ct in cc.DefaultIfEmpty()
                          select new { Name = t.TenantName, ct.Mobile, ct.Phone, ct.EmailId, ct.Address }).FirstOrDefault();
            var unitName = db.PropertyUnits.Where(u => u.Id == c.Unit).Select(u => u.Name).FirstOrDefault();
            var durName = db.Durations.Where(d => d.Id == (c.Duration ?? 0)).Select(d => d.Name).FirstOrDefault();

            var cheques = db.Cheques.Where(ab => ab.Reference == c.Id && ab.Purpose == "TenancyContract")
                            .Select(ab => new { ab.ChequeNo, ab.Amount, ab.Date }).ToList();
            var docs = (from ac in db.PropertyDocumentTypes
                        where ac.Reference == c.Id && ac.Purpose == "Tenancy"
                        join dt in db.DocumentTypes on ac.DocumentType equals dt.ID into dd from dt in dd.DefaultIfEmpty()
                        select new { type = dt.Name, ac.ExpDate }).ToList();

            string sched = c.Schedule == Schedule.Monthly ? "Monthly" : c.Schedule == Schedule.Month3 ? "Every 3 Months"
                         : c.Schedule == Schedule.Month6 ? "Every 6 Months" : "Yearly";
            long due = c.DueDate ?? 0;
            string dueOrd = (due == 1 || due == 21 || due == 31) ? due + "st" : (due == 2 || due == 22) ? due + "nd"
                          : (due == 3 || due == 23) ? due + "rd" : due + "th";

            ViewBag.Id = c.Id;
            ViewBag.Code = c.Code ?? ("TC-" + c.Id);
            ViewBag.CompName = comp != null ? comp.CPName : "Company";
            ViewBag.CompAddr = comp != null ? comp.CPAddress : "";
            ViewBag.CompPhone = comp != null ? comp.CPPhone : "";
            ViewBag.CompEmail = comp != null ? comp.CPEmail : "";
            ViewBag.CompTRN = comp != null ? comp.TRN : "";
            ViewBag.HeaderImg = string.IsNullOrEmpty(hdr) ? "" : ("/uploads/companyheader/header/" + hdr);
            ViewBag.PropName = prop != null ? prop.Name : "-";
            ViewBag.PropAddr = prop == null ? "" : ((prop.Address ?? "") + " " + (prop.City ?? "") + " " + (prop.State ?? "")).Trim();
            ViewBag.UnitName = unitName ?? "-";
            ViewBag.LandlordName = landlord != null ? (landlord.LandlordName ?? "-") : "-";
            ViewBag.LandlordContact = landlord == null ? "" : (((landlord.Mobile ?? landlord.Phone) ?? "") + (string.IsNullOrEmpty(landlord.EmailId) ? "" : "  ·  " + landlord.EmailId));
            ViewBag.LandlordAddr = landlord != null ? (landlord.Address ?? "") : "";
            ViewBag.TenantName = tenant != null ? (tenant.Name ?? "-") : "-";
            ViewBag.TenantContact = tenant == null ? "" : (((tenant.Mobile ?? tenant.Phone) ?? "") + (string.IsNullOrEmpty(tenant.EmailId) ? "" : "  ·  " + tenant.EmailId));
            ViewBag.TenantAddr = tenant != null ? (tenant.Address ?? "") : "";
            ViewBag.StartDate = c.StartDate.ToString("dd MMM yyyy");
            ViewBag.EndDate = c.EndDate.ToString("dd MMM yyyy");
            ViewBag.IssueDate = (c.issuedate ?? c.CreatedDate).ToString("dd MMM yyyy");
            ViewBag.Rent = (c.Rent ?? 0).ToString("#,##0.00");
            ViewBag.Deposit = (c.Deposit ?? 0).ToString("#,##0.00");
            ViewBag.ContractValue = string.IsNullOrWhiteSpace(c.contractvalue) ? (c.Rent ?? 0).ToString("#,##0.00") : c.contractvalue;
            ViewBag.Duration = durName ?? "-";
            ViewBag.Schedule = sched;
            ViewBag.DueDate = due == 0 ? "-" : (dueOrd + " of the period");
            ViewBag.PaymentMode = (c.PaymentType == 1) ? "Cash" : "Cheque";
            ViewBag.Occupants = string.IsNullOrWhiteSpace(c.NumberofOccupants) ? "-" : c.NumberofOccupants;
            ViewBag.WaterElec = string.IsNullOrWhiteSpace(c.WaterAndElectricityBill) ? "-" : c.WaterAndElectricityBill;
            ViewBag.Pets = string.IsNullOrWhiteSpace(c.PetsAllowed) ? "-" : c.PetsAllowed;
            ViewBag.TnC = c.TnC ?? "";
            ViewBag.Note = c.Note ?? "";
            ViewBag.Remark = c.Remark ?? "";
            ViewBag.Cheques = System.Text.Json.JsonSerializer.Serialize(
                cheques.Select(x => new { no = x.ChequeNo ?? "-", amt = x.Amount.ToString("#,##0.00"), date = x.Date.ToString("dd-MM-yyyy") }).ToList());
            ViewBag.Docs = System.Text.Json.JsonSerializer.Serialize(
                docs.Select(x => new { type = x.type ?? "Document", exp = (x.ExpDate ?? DateTime.Now).ToString("dd-MM-yyyy") }).ToList());
            return View();
        }

        public long GetSeNo(string invoicetype)
        {
            long type = 1;
            if (invoicetype == "2")
                type = 2;

            Int64 SENo = 0;
            string prefix = (type == 1) ? "Invoice" : "TaxExempt";
            Int32 number = db.CodePrefixs.Where(a => a.section == prefix).Select(a => a.number).FirstOrDefault();
            if (type == 1)
            {
                if ((db.SalesEntrys.Where(a =>
                a.SaleType == SaleType.Sale).Select(p => p.SENo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    SENo = (number == 0) ? 1 : number;
                }
                else
                {
                    SENo = db.SalesEntrys.Where(a => a.SaleType == SaleType.Sale).Max(p => p.SENo + 1);
                }
            }
            else
            {
                if ((db.SalesEntrys.Where(a =>
                               a.SalesType == 2).Select(p => p.SENo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    SENo = (number == 0) ? 1 : number;
                }
                else
                {
                    SENo = db.SalesEntrys.Where(a => a.SalesType == 2).Max(p => p.SENo + 1);
                }
            }

            return SENo;
        }

        public Boolean InsertToSales(ReferenceAccountViewModel arr, long CustId, string UserId, long branch)
        {
            SalesEntry SEentry = new SalesEntry();
            SEentry.SaleType = SaleType.Sale;
            SEentry.SENo = 0;
            SEentry.BillNo = GetSeNo("").ToString();
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
            SEentry.Remarks = "Sales Entry From Tendancy Creation";


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
            com.addAccountTrasaction((decimal)arr.Amount, 0, (long)arr.Account, "Rent Receivable", salesEntryId, DC.Credit, SEpay.SEDate);
            return true;
        }

        public StringBuilder generatePdf(long id)
        {
            var details = (from a in db.TenancyContracts
                           join b in db.PropertyMains on a.Property equals b.Id into protype
                           from b in protype.DefaultIfEmpty()
                           join c in db.Tenants on a.Tenant equals c.TenantID into doc
                           from c in doc.DefaultIfEmpty()
                           join d in db.PropertyUnits on a.Unit equals d.Id into uni
                           from d in uni.DefaultIfEmpty()
                           join e in db.Durations on a.Duration equals e.Id into dur
                           from e in dur.DefaultIfEmpty()
                           where a.Id == id
                           select new
                           {
                               id = a.Id,
                               Property = b.Name,
                               Tanant = c.TenantName,
                               Unit = d.Name,
                               Duration = e.Name,
                               a.StartDate,
                               a.EndDate,
                               Date = a.CreatedDate,
                               a.Deposit,
                               a.DueDate,
                               a.Note,
                               a.Rent,
                               a.TnC,
                               a.Remark,
                               PaymentType = (a.PaymentType == 1) ? "Cash" : "Cheque",
                               VoucherNo = a.Code,
                               a.Schedule
                           }).FirstOrDefault();
            var cheque = (from a in db.Cheques
                          where a.Reference == id && a.Purpose == "TenancyContract"
                          select new
                          {
                              chequeno = a.ChequeNo,
                              a.Amount,
                              a.Date
                          }).ToList();

            var docs = (from a in db.PropertyDocumentTypes
                        join b in db.DocumentTypes on a.DocumentType equals b.ID into protype
                        from b in protype.DefaultIfEmpty()
                        where a.Reference == id && a.Purpose == "Tenancy"
                        select new
                        {
                            b.Name,
                            a.ExpDate,
                        }).ToList();
            var comdetails = db.companys
           .Select(s => new
           {
               CName = s.CPName,
               CAddress = s.CPAddress,
               CEmail = s.CPEmail,
               CTaxRegNo = s.TRN,
               CPhone = s.CPPhone,
               s.CPMobile,
               CLogo = s.CPLogo,
           }).FirstOrDefault();

            int SI = 1;
            string fsdata = "";

            if (details.VoucherNo != null && details.VoucherNo != "")
            {
                fsdata += details.VoucherNo;
            }
            if (details.Date != null)
            {
                fsdata += details.Date;
            }

            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    sb.Append("<table width='100%' style='border: 0px;text-align:center;'><tr><td><b>Tenancy Contract Voucher</b></td></tr></table>");

                    string partyDetails = "<table style='border:.1px #ccc;' width='100%'><tr style='border-top:.1px #ccc; '> " +
                        "<td width='50%' style='border - right: 0px;'> " +
                        "<table  style='border: 0px; width: 100 %;'><tr><td style='font-size:14px;font-weight:normal;'><b>Invoice No </b>: " + details.VoucherNo + "</td><td style='font-size:14px;font-weight:normal;'><b>Date</b> : " + details.Date.ToString("dd-MM-yyyy") + "</td></tr>" +
                        "< tr >< td style = 'font-size:14px;font-weight:normal;' >< b > Property </ b >: " + details.Property + " </ td >< td style = 'font-size:14px;font-weight:normal;' >< b > Tenant </ b > : " + details.Tanant + " </ td ></ tr > " +
                        "< tr >< td style = 'font-size:14px;font-weight:normal;' >< b > Unit </ b >: " + details.Unit + " </ td >< td style = 'font-size:14px;font-weight:normal;' >< b > Duration  </ b > : " + details.Duration + " </ td ></ tr > " +
                        "< tr >< td style = 'font-size:14px;font-weight:normal;' >< b > Start Date </ b >: " + details.StartDate + " </ td >< td style = 'font-size:14px;font-weight:normal;' >< b > End Date </ b > : " + details.EndDate + " </ td ></ tr ></table> " +
                        "</td>" +
                        "<td style='border: 0px;'>";
                    partyDetails += "</td></tr></table>";

                    sb.Append(partyDetails);
                    string rentdetails = "<table style='border:.1px #ccc;' width='100%'><tr style='border-top:.1px #ccc; '> " +
                        "<td width='50%' style='border - right: 0px;'> " +
                        "<table  style='border: 0px; width: 100 %;'>< tr >< td style = 'font-size:14px;font-weight:normal;' >< b > Rent Details </ b > </ td ></tr><tr><td style='font-size:14px;font-weight:normal;'><b>Rent Amount </b>: " + details.Rent + "</td><td style='font-size:14px;font-weight:normal;'><b>Security Deposit Amount</b> : " + details.Deposit + "</td></tr>" +
                        "< tr >< td style = 'font-size:14px;font-weight:normal;' >< b > Schedule </ b >: " + details.Schedule + " </ td >< td style = 'font-size:14px;font-weight:normal;' >< b > Due Date </ b > : " + details.DueDate + " </ td ></ tr > " +
                        "< tr >< td style = 'font-size:14px;font-weight:normal;' >< b > Payment Type </ b >: " + details.PaymentType + " </ td ></ tr ></table> " +
                        "</td>" +
                        "<td style='border: 0px;'>";
                    rentdetails += "</td></tr></table>";

                    sb.Append(rentdetails);

                    if (cheque.Count > 0)
                    {
                        //Cheque table
                        sb.Append("<label style='margin-top:5px; margin-bottom:1px; font-size:14px;border: .1px solid #ccc;'><b>Cheque Details</b></label>");
                        sb.Append("<table width='50%' style='border-collapse:collapse;font-size:12px;border: .1px solid #ccc; repeat-header:yes;'>");
                        sb.Append("<thead>");
                        sb.Append("<tr style='font-size:13px;'>");
                        sb.Append("<th width='15%' style='padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Cheque No</th>");
                        sb.Append("<th width='25%' style='padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Date</th>");
                        sb.Append("<th style='padding: 5px;vertical-align: top;border: .1px solid #ccc;text-align:center;'>Amount (AED)</th>");
                        sb.Append("</tr>");
                        sb.Append("</thead>");
                        sb.Append("<tbody>");
                        foreach (var arr in cheque)
                        {
                            sb.Append("<tr style='font-size:10px;'>");
                            {
                                sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;'> " + arr.chequeno + "</td>");
                                sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + arr.Date.ToString("dd-MM-yyyy") + "</td>");
                                sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: right;border: .1px solid #ccc;'>" + arr.Amount + "</td>");
                            }
                            sb.Append("</tr>");

                        }
                        sb.Append("</tbody>");
                        sb.Append("</table>");
                    }

                    if (docs.Count > 0)
                    {
                        //doc table
                        sb.Append("<label style='font-size:14px;border: .1px solid #ccc;'><b>Document Details</b></label>");
                        sb.Append("<table width='50%' style='border-collapse:collapse;font-size:12px;border: .1px solid #ccc; repeat-header:yes;'>");
                        sb.Append("<thead>");
                        sb.Append("<tr style='font-size:13px;'>");
                        sb.Append("<th width='35%' style='padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Document Type</th>");
                        sb.Append("<th style='padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Expiry Date</th>");
                        sb.Append("</tr>");
                        sb.Append("</thead>");
                        sb.Append("<tbody>");
                        foreach (var arr in docs)
                        {
                            sb.Append("<tr style='font-size:10px;'>");
                            {
                                sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;'> " + arr.Name + "</td>");
                                sb.Append("<td style='font-size: 12px;padding: 6px;vertical-align: top;text-align: left;border: .1px solid #ccc;'>" + arr.ExpDate + "</td>");
                            }
                            sb.Append("</tr>");

                        }
                        sb.Append("</tbody>");
                        sb.Append("</table>");
                    }


                    if (details.Remark != null)
                    {
                        sb.Append("<table width='100%' style='border: .1px solid #ccc;border-collapse:collapse;'>");
                        sb.Append("<tr style='font-size:10px;'><td style='font-size: 12px;padding: 6px;vertical-align: top;border: .1px solid #ccc;'><b>Narration</b> :  " + details.Remark + "</td></tr>");
                        sb.Append("</table>");
                    }
                    sb.Append("<table width='100%' style='border: .1px solid #ccc;border-collapse:collapse;'>");
                    sb.Append("<tr>");
                    sb.Append("<td align='left' width='347px' style='border: .1px solid #ccc;padding: 10px;vertical-align: top;'>");
                    sb.Append("<div style='font-size: 14px;text-align: left;'>Receiver's Signature:<br />توقيع المتلقي</div>");
                    sb.Append("</td>");
                    sb.Append("<td style='border: .1px solid #ccc;padding: 10px;vertical-align: top;'>");
                    sb.Append("<div style='font-size: 14px;text-align: left;'>");
                    sb.Append("For " + comdetails.CName + "");
                    sb.Append("</div>");
                    sb.Append("</td>");
                    sb.Append("</tr>");
                    sb.Append("</table>");
                }
            }
            return sb;
        }

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,View Broker")]
        public ActionResult Details(int? id)
        {
            ViewBag.Prjct = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = true, Text = "All", Value = null},
                               }, "Value", "Text", 1);
            TenancyContractViewModel cusmodel = new TenancyContractViewModel();
            cusmodel = (from a in db.TenancyContracts
                        join b in db.Tenants on a.Tenant equals b.TenantID into tmp
                        from b in tmp.DefaultIfEmpty()
                        join c in db.PropertyMains on a.Property equals c.Id into pro
                        from c in pro.DefaultIfEmpty()
                        join d in db.PropertyUnits on a.Unit equals d.Id into uni
                        from d in uni.DefaultIfEmpty()
                        join e in db.Durations on a.Duration equals e.Id into dur
                        from e in dur.DefaultIfEmpty()
                        where a.Id == id
                        select new TenancyContractViewModel
                        {
                            Id = a.Id,
                            TenantName = b.TenantName,
                            Date = a.CreatedDate,
                            PropertyName = c.Name,
                            UnitName = d.Name,
                            StartDate = a.StartDate.ToString("dd-MM-yyyy"),
                            EndDate = a.EndDate.ToString("dd-MM-yyyy"),
                            DurationName = e.Name,
                            Remark = a.Remark,
                            Note = a.Note,
                            TermsCondition = a.TnC,
                            Rent = a.Rent,
                            Deposit = a.Deposit,
                            Due = (a.DueDate == 1 || a.DueDate == 21) ? a.DueDate + "st" : (a.DueDate == 2 || a.DueDate == 22) ? a.DueDate + "nd" : (a.DueDate == 3 || a.DueDate == 23) ? a.DueDate + "rd" : a.DueDate + "th",
                            Payment = (a.PaymentType == 1) ? "Cash" : "Cheque",
                            Schedulename = (a.Schedule == Schedule.Monthly) ? "Monthly" : (a.Schedule == Schedule.Month3) ? "3 Month" : (a.Schedule == Schedule.Month6) ? "6 Month" : "Yearly",
                            docmodel = (from ac in db.PropertyDocumentTypes
                                        where (ac.Reference == a.Id && ac.Purpose == "Tenancy")
                                        select new DocumentTypeViewModel
                                        {
                                            ExpDate = ac.ExpDate,
                                            Type = db.DocumentTypes.Where(y => y.ID == ac.DocumentType).Select(x => x.Name).FirstOrDefault(),
                                        }).ToList(),
                            cheqmodel = (from ab in db.Cheques
                                         where (ab.Reference == a.Id && ab.Purpose == "TenancyContract")
                                         select new ChequeViewModel
                                         {
                                             //Date = ab.Date.ToString("dd-MM-yyyy"),
                                             ChequeNo = ab.ChequeNo,
                                             Amount = ab.Amount,
                                             ViewDate = ab.Date
                                         }).ToList(),
                        }).FirstOrDefault();
            return View(cusmodel);
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