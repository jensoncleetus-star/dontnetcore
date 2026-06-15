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
using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Areas.Property.Controllers
{
    [Microsoft.AspNetCore.Mvc.Area("Property")]
    public class PropertyRegistrationController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PropertyRegistrationController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // [RedirectingAction]
        //[QkAuthorize(Roles = "Dev,PropertyRegistration")]
        public ActionResult Index()
        {
            ViewBag.Alldata = QkSelect.List(
               new List<SelectListItem>
               {
                                        new SelectListItem { Selected = true, Text = "All", Value = "0"},
               }, "Value", "Text", 0);

            return View();
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev, PropertyRegistration")]
        public ActionResult GetPropertyReg(string InvoiceNo, string FromDate, string ToDate, long? Developer, long? Owner, long? Property, long? Broker)
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
            var UserView = (from a in db.PropertyRegistrations
                            join b in db.Developers on a.Developer equals b.DeveloperID into dev
                            from b in dev.DefaultIfEmpty()
                            join c in db.Accountss on a.Owner equals c.AccountsID into own
                            from c in own.DefaultIfEmpty()
                            join d in db.PropertyMains on a.Property equals d.Id into pro
                            from d in pro.DefaultIfEmpty()
                            join e in db.Brokers on a.Broker equals e.BrokerID into bro
                            from e in bro.DefaultIfEmpty()
                            where
                              (InvoiceNo == "" || a.VoucherNo == InvoiceNo) &&
                               (FromDate == "" || EF.Functions.DateDiffDay(a.RDate, fdate) <= 0) &&
                               (ToDate == "" || EF.Functions.DateDiffDay(a.RDate, tdate) >= 0) &&
                              (Developer == 0 || a.Developer == Developer) &&
                              (Owner == 0 || a.Owner == Owner) &&
                              (Property == 0 || a.Property == Property) &&
                              (Broker == 0 || a.Broker == Broker)
                            select new
                            {
                                id = a.RegistrationID,
                                a.VoucherNo,
                                a.RDate,
                                Developer = b.DeveloperCode + " " + b.DeveloperName,
                                Owner = c.Name,
                                Property = d.Code + " " + d.Name,
                                Broker = e.BrokerCode + " " + e.BrokerName,
                                a.Remark,
                                a.Note,
                                a.TermsCondition,

                            });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(p => p.VoucherNo.ToString().ToLower().Contains(search.ToLower()));
            }
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                UserView = UserView.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = UserView.Count();
            var data = UserView.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        //[RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Create ProRegistration")]
        [HttpGet]
        public ActionResult Create()
        {
            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;
            var proreg = new PropertyRegistrationViewModel
            {
                VoucherNo = InvoiceNo(),
                RDate = System.DateTime.Now.ToString("dd-MM-yyyy"),
            };


            proreg.AdditionalField = db.AdditionalFields.Where(x => x.Section == "Property Registration").ToList();

            var dev = db.Developers.Select(s => new
            {
                DeveloperID = s.DeveloperID,
                DeveloperDetails = s.DeveloperCode + " - " + s.DeveloperName
            }).ToList();
            ViewBag.Develop = QkSelect.List(dev, "DeveloperID", "DeveloperDetails");

            var own = db.Accountss.Where(a => a.AccountsID != 4 && a.Group != 23).Select(a => new
            {
                ID = a.AccountsID,
                Name = a.Name
            }).ToList();
            ViewBag.Own = QkSelect.List(own, "Id", "Name");

            var pro = (from a in db.PropertyMains
                       join b in db.PropertyRegistrations on a.Id equals b.Property into deve
                       from b in deve.DefaultIfEmpty()
                       where (a.Id != b.Property)
                       select new
                       {
                           ID = a.Id,
                           Name = a.Code + " " + a.Name
                       })
                       .ToList();
            ViewBag.Proper = QkSelect.List(pro, "ID", "Name");

            var br = db.Brokers
                            .Select(s => new
                            {
                                ID = s.BrokerID,
                                Name = s.BrokerCode + " " + s.BrokerName
                            })
                            .ToList();
            ViewBag.Broker = QkSelect.List(br, "ID", "Name");

            ViewBag.Measurement = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "In Square feet", Value="1"},
                new SelectListItem() {Text = "In Square meter", Value="2"},
            }, "Value", "Text");
            ViewBag.PaymentTypes = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Cash", Value="1"},
                new SelectListItem() {Text = "Cheque", Value="2"},
            }, "Value", "Text");

            ViewBag.LastEntry = db.PropertyRegistrations.Select(p => p.RegistrationID).AsEnumerable().DefaultIfEmpty(0).Max();
            return View(proreg);
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Create ProRegistration")]
        public JsonResult Create(PropertyRegistrationViewModel vmodel, string fnval)
        {
            string msg = "";
            bool stat = false;
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
            if (!BillExist(vmodel.VoucherNo))
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


                PropertyRegistration preg = new PropertyRegistration();
                preg.VoucherNo = vmodel.VoucherNo;
                preg.PRNo = GetPRNo();
                preg.RDate = DateTime.Parse(vmodel.RDate.ToString(), new CultureInfo("en-GB"));

                preg.Developer = vmodel.Developer;
                preg.Owner = vmodel.Owner;
                preg.Property = vmodel.Property;
                if (vmodel.Broker != null)
                {
                    preg.Broker =Convert.ToInt64(vmodel.Broker);
                }
                else
                {
                    preg.Broker = null;
                }

                preg.Branch = Branch;
                preg.Note = vmodel.Note;
                preg.Remark = vmodel.Remark;
                preg.TermsCondition = vmodel.TermsCondition;
                preg.Amount = vmodel.Amount;
                preg.PaymentType = vmodel.PaymentType;
                preg.PlotNumber = vmodel.PlotNumber;
                preg.PlotArea = vmodel.PlotArea;
                preg.PlotOption = vmodel.PlotOption;
                preg.BuildupArea = vmodel.BuildupArea;
                preg.PAMeasurement = vmodel.PAMeasurement;
                preg.BAMeasurement = vmodel.BAMeasurement;
                preg.Area = vmodel.Area;
                preg.Hector = vmodel.Hector;
                preg.ADDCNo = vmodel.ADDCNo;
                if (vmodel.BookingDate != null)
                {
                    preg.BookingDate = DateTime.Parse(vmodel.BookingDate.ToString(), new CultureInfo("en-GB"));
                }
                if (vmodel.HandoverDate != null)
                {
                    preg.HandoverDate = DateTime.Parse(vmodel.HandoverDate.ToString(), new CultureInfo("en-GB"));
                }
                preg.PermissionId = vmodel.PermissionId;
                preg.PermitId = vmodel.PermitId;

                preg.CreatedDate = today;
                preg.CreatedBy = UserId;
                preg.Status = Status.active;
                db.PropertyRegistrations.Add(preg);
                db.SaveChanges();
                Int64 ID = preg.RegistrationID;


                if (vmodel.docmodel != null)
                {
                   var  count2 = 0;
                    foreach (var arr in vmodel.docmodel)
                    {
                        if (arr.Attachments != null)
                        {
                            PropertyDocumentType doc = new PropertyDocumentType();
                            doc.DocumentType = Convert.ToInt64(arr.Type);
                            doc.Reference = ID;
                            doc.Purpose = "PropertyRegistration";
                            if (arr.Date == null)
                                arr.Date = "01-01-3000";

                            doc.ExpDate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));
                            db.PropertyDocumentTypes.Add(doc);
                            db.SaveChanges();
                            Int64 docid = doc.ID;


                            string storePath = LegacyWeb.MapPath("~/uploads/PropertyContractDocument/" + "Property_" + docid);
                            if (!Directory.Exists(storePath))
                                Directory.CreateDirectory(storePath);

                            // files upload
                            IFormFile file = Request.Form.Files["docmodel[" + count2 + "].Attachments"];
                            //IFormFile file = Request.Form.Files[0];
                            var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                            var uploadUrl = LegacyWeb.MapPath("~/uploads/PropertyContractDocument/" + "Property_" + docid + "/");

                            file.SaveAs(Path.Combine(uploadUrl, fileNames));

                            DocumentFile docfile = new DocumentFile();
                            docfile.attachments = fileNames;
                            docfile.Document = docid;
                            db.DocumentFiles.Add(docfile);
                            db.SaveChanges();



                        }
                    }
                }






                if (vmodel.AdditionalField != null)
                {
                    foreach (AdditionalField Ad in vmodel.AdditionalField)
                    {
                        var rate = new AdditionalFieldData
                        {
                            Reference = ID,
                            Name = Ad.Name,
                            Purpose = "PropertyRegistration",
                            Field = Ad.ID
                        };
                        db.AdditionalFieldDatas.Add(rate);
                        db.SaveChanges();
                    }
                }

                //cheq
                var count=0;
                if (vmodel.PaymentType == 2)
                {
                    foreach (var arr in vmodel.cheqmodel)
                    {
                    
                        if (arr.Amount != null)
                        {
                            var cheqdate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));
                            Cheque cheq = new Cheque();
                            cheq.Reference = ID;
                            cheq.Purpose = "PropertyRegistrations";
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
                                    //  IFormFile file = Request.Form.Files[0];
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
                                PDCType = "PropertyRegistrations",
                                Reference = ID,
                                CheckNo = arr.ChequeNo,
                                Bank = null,
                                Note = null,
                                RegStatus = choice.No,
                                Status = Status.active,
                                CreatedBy = UserId,
                                CreatedDate = today,
                                Branch = Branch,
                                editable = choice.No,
                                Bills = vmodel.VoucherNo,
                                Type = (today == cheqdate) ? 1 : 0
                            };
                            db.PDCs.Add(pd);
                            db.SaveChanges();
                            //Add Account Transactions
                            //deposit amount
                            if (vmodel.Amount > 0)
                            {
                             //   com.addAccountTrasaction(0, Convert.ToDecimal(arr.Amount), (long)arr.Bank, "RegistrationDeposit", ID, DC.Credit, today, null, null, vmodel.Property, null);
                              //  com.addAccountTrasaction(Convert.ToDecimal(arr.Amount), 0, vmodel.Owner, "RegistrationDeposit", ID, DC.Debit, today, null, null, vmodel.Property, null);
                            }
                        }
                        count++;

                    }
                  
                }
                else
                {
                    //Add Account Transactions
                    Company comp = db.companys.Find(1L);
                    //deposit amount
                    if (vmodel.Amount > 0)
                    {
                      //  com.addAccountTrasaction(0, (decimal)vmodel.Amount, (long)comp.RegdepositAccount, "RegistrationDeposit", ID, DC.Credit, today, null, null, vmodel.Property, null);
                      //  com.addAccountTrasaction((decimal)vmodel.Amount, 0, vmodel.Owner, "RegistrationDeposit", ID, DC.Debit, today, null, null, vmodel.Property, null);
                    }
                }


                com.addlog(LogTypes.Created, UserId, "PropertyRegistration", "PropertyRegistrations", findip(), preg.RegistrationID, "Successfully Submitted Property Registration");
                msg = "Successfully added Property Registration.";
                if ((fnval) == "print")
                {
                    //var data = vmodel;
                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";
                    var prodata = vmodel;
                    var Data = (from a in db.PropertyRegistrations
                                join b in db.Developers on a.Developer equals b.DeveloperID into item
                                from b in item.DefaultIfEmpty()
                                join c in db.Accountss on a.Owner equals c.AccountsID into Br
                                from c in Br.DefaultIfEmpty()
                                join p in db.PropertyMains on a.Property equals p.Id into prjct
                                from p in prjct.DefaultIfEmpty()
                                join t in db.Brokers on a.Broker equals t.BrokerID into ptask
                                from t in ptask.DefaultIfEmpty()
                                where a.VoucherNo == vmodel.VoucherNo
                                select new
                                {
                                    date = a.RDate,
                                    developer = b.DeveloperName,
                                    owner = c.Name,
                                    property = p.Name,
                                    broker = t.BrokerName,
                                    note = a.Note,
                                    Remark = a.Remark,
                                    tnc = a.TermsCondition,
                                    a.VoucherNo
                                }).FirstOrDefault();
                    var additionalfield = (from a in db.AdditionalFieldDatas
                                           join b in db.AdditionalFields on a.Field equals b.ID into item
                                           from b in item.DefaultIfEmpty()
                                           where a.Reference == ID && a.Purpose == "PropertyRegistration"
                                           select new
                                           {
                                               Field = b.Name,
                                               data = a.Name
                                           }).ToList();


                    var arr2 = new ArrayList();
                    arr2.Add(Data);
                    arr2.Add(additionalfield);
                   














                    //var fmapp = db.FieldMappings.Where(a => a.Section == "PropertyRegistration" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                    msg = "Successfully Added Property Registration .";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Data, additionalfield, arr2, ComHeadCheck } };
                }
                else
                {
                    msg = "Successfully Added Property Registration .";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }

            }
            else
            {
                msg = "Voucher No. Name already exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }


        [HttpGet]
        //[QkAuthorize(Roles = "Dev,Edit ProRegistration")]
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
            PropertyRegistration pr = db.PropertyRegistrations.Find(id);

            PropertyRegistrationViewModel vmodel = new PropertyRegistrationViewModel();
            vmodel.RegistrationID = (long)id;
            vmodel.VoucherNo = pr.VoucherNo;
            vmodel.Note = pr.Note;
            vmodel.Remark = pr.Remark;
            vmodel.TermsCondition = pr.TermsCondition;
            vmodel.RDate = pr.RDate.ToString("dd-MM-yyyy");

            vmodel.Developer = pr.Developer;
            vmodel.Owner = pr.Owner;
            vmodel.Property = pr.Property;
          
                vmodel.Broker =Convert.ToString(pr.Broker);
         
            vmodel.AdditionalField = db.AdditionalFields.Where(x => x.Section == "Property Registration").ToList();
            vmodel.AdditionalFieldVieModels = (from b in db.AdditionalFieldDatas
                                               where b.Reference == id && b.Purpose == "PropertyRegistration"
                                               select new AdditionalFieldVieModel
                                               {
                                                   ID = b.ID,
                                                   Entrydata = b.Name,
                                                   Name = b.Name,
                                                   Field = b.Field
                                               }).ToList();
            vmodel.Amount = pr.Amount;
            vmodel.PlotArea = pr.PlotArea;
            vmodel.PlotNumber = pr.PlotNumber;
            vmodel.PlotOption = pr.PlotOption;
            vmodel.PAMeasurement = pr.PAMeasurement;
            vmodel.BAMeasurement = pr.BAMeasurement;
            vmodel.BuildupArea = pr.BuildupArea;
            vmodel.PaymentType = pr.PaymentType;
            vmodel.Hector = pr.Hector;
            vmodel.ADDCNo = pr.ADDCNo;
            vmodel.BookingDate = (pr.BookingDate == null) ? null : Convert.ToDateTime(pr.BookingDate, CultureInfo.CurrentCulture).ToString("dd-MM-yyyy"); 
            vmodel.HandoverDate = (pr.HandoverDate == null) ? null: Convert.ToDateTime(pr.HandoverDate, CultureInfo.CurrentCulture).ToString("dd-MM-yyyy");  
            vmodel.PermissionId = pr.PermissionId;
            vmodel.PermitId = pr.PermitId;
            vmodel.Area = pr.Area;
            var dev = db.Developers.Select(s => new
            {
                DeveloperID = s.DeveloperID,
                DeveloperDetails = s.DeveloperCode + " - " + s.DeveloperName
            }).ToList();
            ViewBag.Develop = QkSelect.List(dev, "DeveloperID", "DeveloperDetails");

            var own = db.Accountss.Where(a => a.AccountsID != 4 && a.Group != 23).Select(a => new
            {
                ID = a.AccountsID,
                Name = a.Name
            }).ToList();
            ViewBag.Own = QkSelect.List(own, "Id", "Name");

            var pro = db.PropertyMains
                             .Select(s => new
                             {
                                 ID = s.Id,
                                 Name = s.Code + " " + s.Name
                             })
                             .ToList();
            ViewBag.Proper = QkSelect.List(pro, "ID", "Name");

            var br = db.Brokers
                            .Select(s => new
                            {
                                ID = s.BrokerID,
                                Name = s.BrokerCode + " " + s.BrokerName
                            })
                            .ToList();
       
            ViewBag.Brokerlist = QkSelect.List(br, "ID", "Name");

            ViewBag.preEntry = db.PropertyRegistrations.Where(a => a.RegistrationID < id).Select(a => a.RegistrationID).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.PropertyRegistrations.Where(a => a.RegistrationID > id).Select(a => a.RegistrationID).DefaultIfEmpty().Min();
            ViewBag.Measurement = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "In Square feet", Value="1"},
                new SelectListItem() {Text = "In Square meter", Value="2"},
            }, "Value", "Text");
            ViewBag.PaymentTypes = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Cash", Value="1"},
                new SelectListItem() {Text = "Cheque", Value="2"},
            }, "Value", "Text");
            if (pr == null)
            {
                return NotFound();
            }
            return View(vmodel);
        }
        [HttpPost]
        //[RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Edit ProRegistration")]
        public JsonResult Edit(PropertyRegistrationViewModel vmodel, string fnval)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            var today = Convert.ToDateTime(System.DateTime.Now);
            long Branch = 0;
           
            if (vmodel.PaymentType == 2 && vmodel.cheqmodel != null)
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
            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            PropertyRegistration proreg = db.PropertyRegistrations.Find(vmodel.RegistrationID);
            com.DeleteAllAccountTransaction("RegistrationDeposit", Convert.ToInt64(vmodel.RegistrationID));
            if (BranchCheck == Status.active)
            {
                Branch = (long)vmodel.Branch;
            }
            else
            {
                Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
            }

            if (BillExist(Convert.ToString(vmodel.VoucherNo)) && Convert.ToString(vmodel.VoucherNo) != proreg.VoucherNo)
            {
                msg = "Voucher No. Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

            proreg.VoucherNo = vmodel.VoucherNo;
            proreg.Note = vmodel.Note;
            proreg.Remark = vmodel.Remark;
            proreg.TermsCondition = vmodel.TermsCondition;
            proreg.Note = vmodel.Note;
            proreg.Branch = Branch;
            proreg.RDate = DateTime.Parse(vmodel.RDate.ToString(), new CultureInfo("en-GB"));
            proreg.PaymentType = vmodel.PaymentType;

            proreg.Developer = vmodel.Developer;
            proreg.Owner = vmodel.Owner;
            proreg.Property = vmodel.Property;
            if (vmodel.Broker != null)
            {
                proreg.Broker = Convert.ToInt64(vmodel.Broker);
            }
            else
            {
                proreg.Broker = null;
            }
            proreg.Amount = vmodel.Amount;
            proreg.PlotNumber = vmodel.PlotNumber;
            proreg.PlotArea = vmodel.PlotArea;
            proreg.PlotOption = vmodel.PlotOption;
            proreg.BuildupArea = vmodel.BuildupArea;
            proreg.PAMeasurement = vmodel.PAMeasurement;
            proreg.BAMeasurement = vmodel.BAMeasurement;
            proreg.Hector = vmodel.Hector;
            proreg.ADDCNo = vmodel.ADDCNo;
            proreg.Area = vmodel.Area;

            if (vmodel.BookingDate != null)
            {
                proreg.BookingDate = DateTime.Parse(vmodel.BookingDate.ToString(), new CultureInfo("en-GB"));
            }
            else
            {
                proreg.BookingDate = null;
            }
            if (vmodel.HandoverDate != null)
            {
                proreg.HandoverDate = DateTime.Parse(vmodel.HandoverDate.ToString(), new CultureInfo("en-GB"));
            }else
            {
                proreg.HandoverDate = null;
            }
            proreg.PermissionId = vmodel.PermissionId;
            proreg.PermitId = vmodel.PermitId;

            db.Entry(proreg).State = EntityState.Modified;
            db.SaveChanges();
            Int64 prId = proreg.RegistrationID;



            if (vmodel.docmodel != null)
            {
                var count2 = 0;
                foreach (var arr in vmodel.docmodel)
                {
                    if (arr.Attachments != null)
                    {

                        if(arr.ID==0)
                        {

                            PropertyDocumentType doc = new PropertyDocumentType();
                            doc.DocumentType = Convert.ToInt64(arr.Type);
                            doc.Reference = prId;
                            doc.Purpose = "PropertyRegistration";
                            if(arr.Date!=null)
                            doc.ExpDate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));
                            db.PropertyDocumentTypes.Add(doc);
                            db.SaveChanges();
                            Int64 docid = doc.ID;


                            string storePath = LegacyWeb.MapPath("~/uploads/PropertyContractDocument/" + "Property_" + docid);
                            if (!Directory.Exists(storePath))
                                Directory.CreateDirectory(storePath);

                            // files upload
                            IFormFile file = Request.Form.Files["docmodel[" + count2 + "].Attachments"];
                            //IFormFile file = Request.Form.Files[0];
                            var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                            var uploadUrl = LegacyWeb.MapPath("~/uploads/PropertyContractDocument/" + "Property_" + docid + "/");

                            file.SaveAs(Path.Combine(uploadUrl, fileNames));

                            DocumentFile docfile = new DocumentFile();
                            docfile.attachments = fileNames;
                            docfile.Document = docid;
                            db.DocumentFiles.Add(docfile);
                            db.SaveChanges();

                        }
                        else
                        { 
                        PropertyDocumentType doc = db.PropertyDocumentTypes.Find(arr.ID);
                        doc.DocumentType = Convert.ToInt64(arr.Type);
                        doc.Reference = prId;
                        doc.Purpose = "PropertyRegistration";
                        doc.ExpDate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));
                        db.PropertyDocumentTypes.Add(doc);
                        
                        db.Entry(doc).State = EntityState.Modified;
                        db.SaveChanges();
                        Int64 docid = doc.ID;


                        string storePath = LegacyWeb.MapPath("~/uploads/PropertyContractDocument/" + "Property_" + docid);
                        if (!Directory.Exists(storePath))
                            Directory.CreateDirectory(storePath);

                        // files upload
                        IFormFile file = Request.Form.Files["docmodel[" + count2 + "].Attachments"];
                        //IFormFile file = Request.Form.Files[0];
                        var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                        var uploadUrl = LegacyWeb.MapPath("~/uploads/PropertyContractDocument/" + "Property_" + docid + "/");

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









            if (vmodel.AdditionalField != null)
            {
                db.AdditionalFieldDatas.RemoveRange(db.AdditionalFieldDatas.Where(a => a.Reference == prId && a.Purpose == "PropertyRegistration"));
                foreach (AdditionalField Ad in vmodel.AdditionalField)
                {
                    var rate = new AdditionalFieldData
                    {
                        Reference = prId,
                        Name = Ad.Name,
                        Purpose = "PropertyRegistration",
                        Field = Ad.ID
                    };
                    db.AdditionalFieldDatas.Add(rate);
                    db.SaveChanges();
                }
            }
            //cheq
          //  db.Cheques.RemoveRange(db.Cheques.Where(a => a.Reference == prId && a.Purpose == "PropertyRegistrations"));
           // db.SaveChanges();
          db.PDCs.RemoveRange(db.PDCs.Where(a => a.Reference == prId && a.PDCType == "PropertyRegistrations"));
          db.SaveChanges();
            if(vmodel.PaymentType == 1)
            {
                 db.Cheques.RemoveRange(db.Cheques.Where(a => a.Reference == prId && a.Purpose == "PropertyRegistrations"));
                 db.SaveChanges();
                Company comp = db.companys.Find(1L);
                //deposit amount
                if (vmodel.Amount > 0)
                {
                   // com.addAccountTrasaction(0, (decimal)vmodel.Amount, (long)comp.RegdepositAccount, "RegistrationDeposit", prId, DC.Credit, today, null, null, vmodel.Property, null);
                   // com.addAccountTrasaction((decimal)vmodel.Amount, 0, vmodel.Owner, "RegistrationDeposit", prId, DC.Debit, today, null, null, vmodel.Property, null);
                }
            }
            else if (vmodel.PaymentType == 2 && vmodel.cheqmodel !=null)
            {
                int count = 0;
                foreach (var arr in vmodel.cheqmodel)
                {
                    if (arr.Amount != null)
                    {
                        var cheqdate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));
                        Cheque cheq = new Cheque();
                        Int64 cheqid = cheq.ID;
                        
                        if (Convert.ToInt32(arr.ID) == 0)
                        {
                            cheq.Reference = prId;
                            cheq.Purpose = "PropertyRegistrations";
                            cheq.Amount = (decimal)(arr.Amount);
                            cheq.Date = cheqdate;
                            cheq.ChequeNo = arr.ChequeNo;
                            cheq.Bank = arr.Bank;
                            db.Cheques.Add(cheq);
                            db.SaveChanges();

                            cheqid = cheq.ID;

                            string storePath = LegacyWeb.MapPath("~/uploads/chequeimage/" + cheqid + arr.Attachments);
                            if (!Directory.Exists(storePath))
                                Directory.CreateDirectory(storePath);

                            // files upload
                            IFormFile file = Request.Form.Files[count];
                            if (file.FileName != "")
                            {
                                var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                      /*   if(file.ContentType!= "image/jpeg")
                                { 
                                    msg = "please select jpg image";
                                stat = false;
                                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                                    }
                      */

                                var uploadUrl = LegacyWeb.MapPath("~/uploads/chequeimage/");
                                file.SaveAs(Path.Combine(uploadUrl, fileNames));

                            
                                
                             
                                ChequeImage cheqImgs = new ChequeImage();
                                cheqImgs.attachments = fileNames;
                                cheqImgs.Cheque = cheqid;
                                db.ChequeImages.Add(cheqImgs);
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            cheq = db.Cheques.Find((long)arr.ID);
                            cheq.Reference = prId;
                            cheq.Purpose = "PropertyRegistrations";
                            cheq.Amount = (decimal)(arr.Amount);
                            cheq.Date = cheqdate;
                            cheq.ChequeNo = arr.ChequeNo;
                            
                            cheq.Bank = arr.Bank;
                            db.Entry(cheq).State = EntityState.Modified;
                            db.SaveChanges();
                            cheqid = arr.ID;


                            string storePath = LegacyWeb.MapPath("~/uploads/chequeimage/" + cheqid + arr.Attachments);
                            if (!Directory.Exists(storePath))
                                Directory.CreateDirectory(storePath);

                            // files upload
                            IFormFile file = Request.Form.Files[count];
                            if (file.FileName != "")
                            {
                                var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);

                                var uploadUrl = LegacyWeb.MapPath("~/uploads/chequeimage/");
                                file.SaveAs(Path.Combine(uploadUrl, fileNames));

                                //   ChequeImage cheqImg = new ChequeImage();
                                ChequeImage cheqImg = db.ChequeImages.Where(o => o.Cheque == cheqid).FirstOrDefault();

                                if (cheqImg != null)
                                {
                                    cheqImg.attachments = fileNames;
                                    cheqImg.Cheque = cheqid;
                                    db.Entry(cheqImg).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    ChequeImage cheqImgs = new ChequeImage();
                                    cheqImgs.attachments = fileNames;
                                    cheqImgs.Cheque = cheqid;
                                    db.ChequeImages.Add(cheqImgs);
                                    db.SaveChanges();
                                }
                            }
                      
                     
                            


                        }
                    

                       
                       

                  /*      if (arr.Attachments != null)
                        {
                            if (arr.Attachments != null)
                            {
                  */
                            
                     /*       }
                        }
                     */
                        //add pdc
                        PDC pd = new PDC
                        {
                            PDCDate = cheqdate,
                            PDCType = "PropertyRegistrations",
                            Reference = prId,
                            CheckNo = arr.ChequeNo,
                            Bank = arr.Bank.ToString(),
                            Note = null,
                            RegStatus = choice.No,
                            Status = Status.active,
                            CreatedBy = UserId,
                            CreatedDate = today,
                            Branch = Branch,
                            editable = choice.No,
                            Bills = vmodel.VoucherNo,
                            Type = (today == cheqdate) ? 1 : 0
                        };
                        db.PDCs.Add(pd);
                        db.SaveChanges();

                        //Add Account Transactions
                        Company comp = db.companys.Find(1L);
                        //deposit amount
                        if (vmodel.Amount > 0)
                        {
                           // com.addAccountTrasaction(0, Convert.ToDecimal(arr.Amount), (long)arr.Bank, "RegistrationDeposit", prId, DC.Credit, today, null, null, vmodel.Property, null);
                           // com.addAccountTrasaction(Convert.ToDecimal(arr.Amount), 0, vmodel.Owner, "RegistrationDeposit", prId, DC.Debit, today, null, null, vmodel.Property, null);
                        }
                    }
                    count++;
                }
               














            }
            else
            {
                //Add Account Transactions
                Company comp = db.companys.Find(1L);
                //deposit amount
                if (vmodel.Amount > 0)
                {
                  //  com.addAccountTrasaction(0, (decimal)vmodel.Amount, (long)comp.RegdepositAccount, "RegistrationDeposit", prId, DC.Credit, today, null, null, vmodel.Property, null);
                  //  com.addAccountTrasaction((decimal)vmodel.Amount, 0, vmodel.Owner, "RegistrationDeposit", prId, DC.Debit, today, null, null, vmodel.Property, null);
                }
            }
            com.addlog(LogTypes.Updated, UserId, "PropertyRegistration", "PropertyRegistrations", findip(), prId, "Successfully Updated Property Registration");
            if ((fnval) == "print")
            {
                //var data = vmodel;
                var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                var ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";
                var prodata = vmodel;
                var Data = (from a in db.PropertyRegistrations
                            join b in db.Developers on a.Developer equals b.DeveloperID into item
                            from b in item.DefaultIfEmpty()
                            join c in db.Accountss on a.Owner equals c.AccountsID into Br
                            from c in Br.DefaultIfEmpty()
                            join p in db.PropertyMains on a.Property equals p.Id into prjct
                            from p in prjct.DefaultIfEmpty()
                            join t in db.Brokers on a.Broker equals t.BrokerID into ptask
                            from t in ptask.DefaultIfEmpty()
                            where a.RegistrationID == prId
                            select new
                            {
                                date = a.RDate,
                                developer = b.DeveloperName,
                                owner = c.Name,
                                property = p.Name,
                                broker = t.BrokerName,
                                note = a.Note,
                                Remark = a.Remark,
                                tnc = a.TermsCondition,
                                a.VoucherNo
                            }).FirstOrDefault();
                var additionalfield = (from a in db.AdditionalFieldDatas
                                       join b in db.AdditionalFields on a.Field equals b.ID into item
                                       from b in item.DefaultIfEmpty()
                                       where a.Reference == prId && a.Purpose == "PropertyRegistration" && a.Field == b.ID
                                       select new
                                       {
                                           Field = b.Name,
                                           data = a.Name
                                       }).ToList();

                var arr = new ArrayList();
                arr.Add(Data);
                arr.Add(additionalfield);

                //var fmapp = db.FieldMappings.Where(a => a.Section == "PropertyRegistration" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

                msg = "Successfully Updated Property Registration .";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Data, additionalfield, arr, ComHeadCheck } };
            }
            else
            {
                msg = "Successfully Updated Property Registration .";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

        }

        public ActionResult Deletecheque(long id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
           Cheque chk= db.Cheques.Where(a => a.ID == id && a.Purpose == "PropertyRegistrations").FirstOrDefault();

            return PartialView(chk);
        }
        [RedirectingAction]
        [HttpPost, ActionName("Deletecheque")]
        public ActionResult Deletechequepost(long id)
        {
            bool stat = false;
            string msg;
            db.Cheques.RemoveRange(db.Cheques.Where(a => a.ID==id));
            db.SaveChanges();

            stat =true;
            msg = "Successfully deleted .";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [RedirectingAction]
        [HttpPost]
        public ActionResult Deletetenentdoc(long id)
        {
            bool stat = false;
            string msg;
   
          db.PropertyDocumentTypes.RemoveRange(db.PropertyDocumentTypes.Where(a => a.ID == id));
            
             db.SaveChanges();

            stat = true;
            msg = "Successfully deleted .";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        [RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Delete ProRegistration")]
        public ActionResult Delete(long id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            PropertyRegistration preg = db.PropertyRegistrations.Where(x => x.RegistrationID == id).FirstOrDefault();


            if (preg == null)
            {
                return NotFound();
            }
            return PartialView(preg);
        }

        [RedirectingAction]
        [HttpPost, ActionName("Delete")]
        //[QkAuthorize(Roles = "Dev,Delete ProRegistration")]
        //[ValidateAntiForgeryToken]
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
                msg = "Successfully deleted Property Registration.";
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Delete ProRegistration")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteSale(arr) == true) ? count++ : count;
            }
          
            Success("Deleted " + count + " Property Registration.", true);
            return RedirectToAction("Index", "PropertyRegistration");
        }

        private Boolean DeleteSale(long saleId)
        {
            var Msg = chkDeleteWithMsg(saleId);
            if (Msg != null)
            {
                return false;
            }
            else
            {
                return DeleteFn(saleId);
            }
        }

        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            return msg;
        }

        public bool DeleteFn(long Id)
        {
            var UserId = User.Identity.GetUserId();

            db.AdditionalFieldDatas.RemoveRange(db.AdditionalFieldDatas.Where(a => a.Reference == Id && a.Purpose == "PropertyRegistration"));
            db.SaveChanges();
            PropertyRegistration preg = db.PropertyRegistrations.Find(Id);
            db.PropertyRegistrations.Remove(preg);
            db.SaveChanges();
            db.PropertyDocumentTypes.RemoveRange(db.PropertyDocumentTypes.Where(a => a.Reference == Id && a.Purpose == "PropertyRegistration"));
            db.SaveChanges();
            db.PDCs.RemoveRange(db.PDCs.Where(o => o.Reference == Id && o.PDCType == "PropertyRegistrations"));
            db.SaveChanges();
            bool delete = com.DeleteAllAccountTransaction("RegistrationDeposit", Id);
            db.SaveChanges();
            com.DeleteAllAccountTransaction("RegistrationDeposit", Convert.ToInt64(Id));
            com.addlog(LogTypes.Deleted, UserId, "PropertyRegistration", "PropertyRegistrations", findip(), preg.RegistrationID, "Successfully Deleted Property Registration");

            return true;
        }




        private string InvoiceNo(Int64 PNo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "ProRegistration").Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == "ProRegistration").Select(a => a.number).FirstOrDefault();

            if (billNo == null)
            {
                if ((db.PropertyRegistrations.Select(p => p.PRNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    PNo = db.PropertyRegistrations.Max(p => p.PRNo + 1);
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
            var Exists = db.PropertyRegistrations.Any(c => c.VoucherNo == VcNo);
            bool res = (Exists) ? true : false;
            return res;
        }
        private long GetPRNo()
        {
            Int64 AtNo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "ProRegistration").Select(a => a.number).FirstOrDefault();
            if ((db.PropertyRegistrations.Select(p => p.PRNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                AtNo = (number == 0) ? 1 : number;
            }
            else
            {
                AtNo = db.PropertyRegistrations.Max(p => p.PRNo + 1);
            }

            return AtNo;
        }

        public JsonResult SearchOwner(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Accountss.Where(a => a.AccountsID != 4 && a.Group != 23
                                  && a.Name.ToLower().Contains(q.ToLower()) || a.Name.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = a.Name,
                                      id = a.AccountsID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Accountss.Where(a => a.AccountsID != 4 && a.Group != 23).Select(b => new SelectFormat
                {
                    text = b.Name,
                    id = b.AccountsID
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "All" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,Download Receipt")]
        public ActionResult Download(long id)
        {
            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            string HFCheck = ComHeadCheck.ToString();

            var Data = db.PropertyRegistrations.Where(s => s.RegistrationID == id).FirstOrDefault();

            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = Data.VoucherNo;

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), HFCheck);
            return File(ms, "application/pdf", "Property Registration Voucher" + "-" + billno + ".pdf");

            //var RecDet = db.Receipts.Where(s => s.ReceiptId == id).FirstOrDefault();
            //var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            //var billno = RecDet.VoucherNo;
            //SendMail sm = new SendMail();
            //byte[] ms = sm.DownloadPdf(generatePdf(id), HFCheck);
            //return File(ms, "application/pdf", "Receipt Voucher" + "-" + billno + ".pdf");           

        }

        public StringBuilder generatePdf(long id)
        {
            var details = (from a in db.PropertyRegistrations
                           join b in db.Developers on a.Developer equals b.DeveloperID into dev
                           from b in dev.DefaultIfEmpty()
                           join c in db.Accountss on a.Owner equals c.AccountsID into own
                           from c in own.DefaultIfEmpty()
                           join d in db.PropertyMains on a.Property equals d.Id into pro
                           from d in pro.DefaultIfEmpty()
                           join e in db.Brokers on a.Broker equals e.BrokerID into bro
                           from e in bro.DefaultIfEmpty()
                           where a.RegistrationID == id
                           select new
                           {
                               id = a.RegistrationID,
                               a.VoucherNo,
                               Date = a.RDate,
                               Developer = b.DeveloperName,
                               Owner = c.Name,
                               Property = d.Name,
                               Broker = e.BrokerName,
                               a.Remark,
                               a.Note,
                               a.TermsCondition,

                           }).FirstOrDefault();
            var additionalfield = (from a in db.AdditionalFieldDatas
                                   join b in db.AdditionalFields on a.Field equals b.ID into item
                                   from b in item.DefaultIfEmpty()
                                   where a.Reference == id && a.Purpose == "PropertyRegistration"
                                   select new
                                   {
                                       Field = b.Name,
                                       data = a.Name
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
                    sb.Append("<table width='100%' style='border: 0px;text-align:center;'><tr><td><b>Property Registration Voucher</b></td></tr></table>");

                    string partyDetails = "<table style='border:.1px #ccc;' width='100%'><tr style='border-top:.1px #ccc; '> " +
                        "<td width='50%' style='border - right: 0px;'> " +
                        "<table  style='border: 0px; width: 100 %;'><tr><td style='font-size:14px;font-weight:normal;'><b>Invoice No </b>: " + details.VoucherNo + "</td><td style='font-size:14px;font-weight:normal;'><b>Date</b> : " + details.Date.ToString("dd-MM-yyyy") + "</td></tr>" +
                        "< tr >< td style = 'font-size:14px;font-weight:normal;' >< b > Property </ b >: " + details.Property + " </ td >< td style = 'font-size:14px;font-weight:normal;' >< b > Owner </ b > : " + details.Owner + " </ td ></ tr > " +
                        "< tr >< td style = 'font-size:14px;font-weight:normal;' >< b > Developer </ b >: " + details.Developer + " </ td >< td style = 'font-size:14px;font-weight:normal;' >< b > Broker </ b > : " + details.Broker + " </ td ></ tr ></table> " +
                        "</td>" +
                        "<td style='border: 0px;'>";
                    partyDetails += "</td></tr></table>";

                    sb.Append(partyDetails);

                    string addfield = "<table style='border:.1px #ccc;' width='100%'><tr style='border-top:.1px #ccc; '> < td width = '50%' style = 'border - right: 0px;' >  ";
                    if (additionalfield.Count > 0)
                    {
                        foreach (var arr in additionalfield)
                        {
                            if (arr.data != "0" && arr.data != "" && arr.data != null)
                            {
                                addfield += "<table style='border: 0px;'><tr style='border: 0px;'><td style='font-size:14px;font-weight:normal;'><b>" + arr.Field + " </b>   " + arr.data + "</td></tr></table>";
                            }
                        }
                    }
                    addfield += "</td></tr></table>";

                    sb.Append(addfield);



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
            PropertyRegistrationViewModel cusmodel = new PropertyRegistrationViewModel();
            cusmodel = (from a in db.PropertyRegistrations
                        join b in db.Developers on a.Developer equals b.DeveloperID into tmp
                        from b in tmp.DefaultIfEmpty()
                        join c in db.PropertyMains on a.Property equals c.Id into pro
                        from c in pro.DefaultIfEmpty()
                        join d in db.Brokers on a.Broker equals d.BrokerID into uni
                        from d in uni.DefaultIfEmpty()
                        join e in db.Accountss on a.Owner equals e.AccountsID into dur
                        from e in dur.DefaultIfEmpty()
                        where a.RegistrationID == id
                        select new PropertyRegistrationViewModel
                        {
                            RegistrationID = a.RegistrationID,
                            Date = a.CreatedDate,
                            PropertyName = c.Name,
                            BrokerName = d.BrokerName,
                            OwnerName = e.Name,
                            DeveloperName = b.DeveloperName,
                            Remark = a.Remark,
                            Note = a.Note,
                            TermsCondition = a.TermsCondition,
                            VoucherNo = a.VoucherNo,
                            AdditionalFieldVieModels = (from aa in db.AdditionalFieldDatas
                                                        join ab in db.AdditionalFields on aa.Field equals ab.ID into item
                                                        from ab in item.DefaultIfEmpty()
                                                        where aa.Reference == id && aa.Purpose == "PropertyRegistration"
                                                        select new AdditionalFieldVieModel
                                                        {
                                                            Name = ab.Name,
                                                            Entrydata = aa.Name
                                                        }).ToList(),

                        }).FirstOrDefault();
            return View(cusmodel);

        }
        [HttpGet]
        public JsonResult GetCheque(long CnId)
        {
            var ConD = (from a in db.Cheques
                        join b in db.ChequeImages on a.ID equals b.Cheque into che
                        from b in che.DefaultIfEmpty()
                        join c in db.Accountss on a.Bank equals c.AccountsID into Acc
                        from c in Acc.DefaultIfEmpty()
                        where a.Reference == CnId && a.Purpose == "PropertyRegistrations"
                        select new
                        {
                            ID=a.ID,
                            AttAch = b.attachments,
                            Amt = a.Amount,
                            No = a.ChequeNo,
                            Date = a.Date,
                            Id = a.ID,
                            Bank=a.Bank,
                            BankName=c.Name
                        }).Distinct().ToList();
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