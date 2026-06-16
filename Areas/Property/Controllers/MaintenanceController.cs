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
using System.Text;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Areas.Property.Controllers
{
    [Microsoft.AspNetCore.Mvc.Area("Property")]
    public class MaintenanceController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public MaintenanceController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // [RedirectingAction]
        //[QkAuthorize(Roles = "Dev,ProMaintenance")]
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
        //[QkAuthorize(Roles = "Dev, ProMaintenance")]
        public ActionResult GetPropertyReg(string InvoiceNo, string FromDate, string ToDate, long? Contractor, long? Property)
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
            var UserView = (from a in db.Maintenances
                            join d in db.PropertyMains on a.Property equals d.Id into pro
                            from d in pro.DefaultIfEmpty()
                            join e in db.Contractors on a.Contractor equals e.ContractorID into bro
                            from e in bro.DefaultIfEmpty()
                            where
                              (InvoiceNo == "" || a.VoucherNo == InvoiceNo) &&
                               (FromDate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                               (ToDate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                              //(Developer == 0 || a.Developer == Developer) &&
                              //(Owner == 0 || a.Owner == Owner) &&
                              (Property == 0 || Property == null || a.Property == Property) &&
                              (Contractor == 0 || Contractor == null || a.Contractor == Contractor)
                            select new
                            {
                                id = a.ID,
                                a.VoucherNo,
                                a.Date,
                                //Developer = b.co + " " + b.DeveloperName,
                                //Owner = c.Name,
                                Property = d.Code + " " + d.Name,
                                Contractor = e.ContractorCode + " " + e.ContractorName,
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
                try { UserView = UserView.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir); } catch { /* grid column name not in projection - keep default order */ }
            }
            recordsTotal = UserView.Count();
            var data = UserView.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        //[RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Create ProMaintenance")]
        [HttpGet]
        public ActionResult Create()
        {

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;
            var proreg = new MaintenanceViewModel
            {
                VoucherNo = InvoiceNo(),
                Date = System.DateTime.Now.ToString("dd-MM-yyyy"),
                AdditionalField = db.AdditionalFields.Where(x=>x.Section== "Maintenance").ToList()
            };

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

            var contType = db.ContractTypes
                             .Select(s => new
                             {
                                 ID = s.ID,
                                 Name = s.Name
                             })
                             .ToList();
            ViewBag.ContraType = QkSelect.List(contType, "ID", "Name");

            var cn = db.Contractors
                           .Select(s => new
                           {
                               ID = s.ContractorID,
                               Name = s.ContractorCode + " " + s.ContractorName
                           })
                           .ToList();
            ViewBag.Contracto = QkSelect.List(cn, "ID", "Name");
            ViewBag.PaymentTypes = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Cash", Value="1"},
                new SelectListItem() {Text = "Cheque", Value="2"},
            }, "Value", "Text");
            ViewBag.LastEntry = db.Maintenances.Select(p => p.ID).AsEnumerable().DefaultIfEmpty(0).Max();
            return View(proreg);
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Create ProMaintenance")]
        public JsonResult Create(MaintenanceViewModel vmodel, string fnval)
        {
            string msg = "";
            bool stat = false;
            if (!BillExist(vmodel.VoucherNo))
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

                var date= DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));
                Maintenance preg = new Maintenance();
                preg.VoucherNo = vmodel.VoucherNo;
                preg.PRNo = GetPRNo();
                preg.Date = date;

                //preg.Developer = vmodel.Developer;
                //preg.Owner = vmodel.Owner;
                preg.Property = vmodel.Property;
                preg.Contractor = vmodel.Contractor;
                preg.Amount = vmodel.ContractAmount;
                preg.Branch = Branch;
                preg.Note = vmodel.Note;
                preg.Remark = vmodel.Remark;
                preg.TermsCondition = vmodel.TermsCondition;

                preg.StartDate = vmodel.StartDate;
                preg.EndDate = vmodel.EndDate;
                preg.PaymentType = vmodel.PaymentType;
                preg.ContractType = vmodel.ContractType;

                preg.CreatedDate = today;
                preg.CreatedBy = UserId;
                preg.Status = Status.active;
                db.Maintenances.Add(preg);
                db.SaveChanges();
                Int64 ID = preg.ID;

                
                if (vmodel.AdditionalField != null)
                {
                    foreach (AdditionalField Ad in vmodel.AdditionalField)
                    {
                        var rate = new AdditionalFieldData
                        {
                            Reference = ID,
                            Name = Ad.Name,
                            Purpose = "Maintenance",
                            Field = Ad.ID
                        };
                        db.AdditionalFieldDatas.Add(rate);
                        db.SaveChanges();
                    }
                }
                if (vmodel.cheqmodel != null)
                {
                    var count = 0;
                    foreach (var arr in vmodel.cheqmodel)
                    {
                        if (arr.Amount != null)
                        {
                            var cheqdate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));
                            Cheque cheq = new Cheque();
                            cheq.Reference = ID;
                            cheq.Purpose = "Maintenance";
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
                                    var i = 0;
                                    string storePath = LegacyWeb.MapPath("~/uploads/chequeimage/" + arr.Attachments);
                                    if (!Directory.Exists(storePath))
                                        Directory.CreateDirectory(storePath);

                                    // files upload
                                    //IFormFile file = Request.Form.Files[i];
                                    IFormFile file = Request.Form.Files["cheqmodel[" + count + "].Attachments"];
                                    i++;
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
                                PDCType = "Maintenance",
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

                            Company comp = db.companys.Find(1L);
                            // if payment done update to transaction
                            //com.addAccountTrasaction(0, Convert.ToDecimal(arr.Amount), vmodel.Contractor, "Maintenance", ID, DC.Credit, date, null, null, vmodel.Property, null);
                            //com.addAccountTrasaction(Convert.ToDecimal(arr.Amount), 0, (long)arr.Bank, "Maintenance", ID, DC.Debit, date, null, null, vmodel.Property, null);

                        }
                        count++;
                    }
                }
                else
                {
                    Company comp = db.companys.Find(1L);
                    var conType = db.ContractTypes.Where(x => x.ID == vmodel.ContractType).Select(y => y.Account).FirstOrDefault();
                    // if payment done update to transaction
                    //com.addAccountTrasaction(0, vmodel.ContractAmount, vmodel.Contractor, "Maintenance", ID, DC.Credit, date, null, null, vmodel.Property, null);
                    //com.addAccountTrasaction(vmodel.ContractAmount, 0, conType, "Maintenance", ID, DC.Debit, date, null, null, vmodel.Property, null);

                }
                com.addlog(LogTypes.Created, UserId, "Maintenance", "Maintenances", findip(), preg.ID, "Successfully Submitted Maintenance Contract");
                if ((fnval) == "print")
                {
                    //var data = vmodel;
                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";
                    var prodata = vmodel;
                    var Data = (from a in db.Maintenances
                                join d in db.PropertyMains on a.Property equals d.Id into pro
                                from d in pro.DefaultIfEmpty()
                                join e in db.Contractors on a.Contractor equals e.ContractorID into bro
                                from e in bro.DefaultIfEmpty()
                                where a.ID == ID
                                select new
                                {
                                    id = a.ID,
                                    a.VoucherNo,
                                    a.Date,
                                    //Developer = b.co + " " + b.DeveloperName,
                                    //Owner = c.Name,
                                    Property = d.Name,
                                    Contractor = e.ContractorName,
                                    a.StartDate,
                                    a.EndDate,
                                    a.Amount,
                                    PaymentType = (a.PaymentType == 1) ? "Cash" : "Cheque",
                                    a.Remark,
                                    a.Note,
                                    a.TermsCondition,
                                }).FirstOrDefault();
                    var additionalfield = (from a in db.AdditionalFieldDatas
                                           join b in db.AdditionalFields on a.Field equals b.ID into item
                                           from b in item.DefaultIfEmpty()
                                           where a.Reference == ID && a.Purpose == "Maintenance"
                                           select new
                                           {
                                               Field = b.Name,
                                               data = a.Name
                                           }).ToList();
                    var cheque = (from a in db.Cheques
                                  where a.Reference == ID && a.Purpose == "Maintenance"
                                  select new
                                  {
                                      chequeno = a.ChequeNo,
                                      a.Amount,
                                      a.Date
                                  }).ToList();
                    msg = "Successfully added Maintenance Contract.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Data, cheque, additionalfield, ComHeadCheck } };
                    //}
                }
                msg = "Successfully added Maintenance Contract.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

            }
            else
            {
                msg = "Voucher No. already exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }


        [HttpGet]
        //[QkAuthorize(Roles = "Dev,Edit ProMaintenance")]
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
            Maintenance pr = db.Maintenances.Find(id);

            MaintenanceViewModel vmodel = new MaintenanceViewModel();
      
            vmodel.ID = (long)id;
            com.DeleteAllAccountTransaction("RegistrationDeposit", Convert.ToInt64(vmodel.ID));
            vmodel.VoucherNo = pr.VoucherNo;
            vmodel.Note = pr.Note;
            vmodel.Remark = pr.Remark;
            vmodel.TermsCondition = pr.TermsCondition;
            vmodel.Date = pr.Date.ToString("dd-MM-yyyy");
            vmodel.ContractAmount = pr.Amount;
            //vmodel.Developer = pr.Developer;
            //vmodel.Owner = pr.Owner;
            vmodel.Property = pr.Property;
            vmodel.Contractor = pr.Contractor;

            vmodel.StartDate = pr.StartDate;//.ToString("dd-MM-yyyy");
            vmodel.EndDate = pr.EndDate;//.ToString("dd-MM-yyyy");
            vmodel.PaymentType = pr.PaymentType;
            vmodel.ContractType = pr.ContractType;
            vmodel.AdditionalField = db.AdditionalFields.Where(x => x.Section == "Maintenance").ToList();
            vmodel.AdditionalFieldVieModels = (from b in db.AdditionalFieldDatas
                                               where b.Reference == id && b.Purpose == "Maintenance"
                                               select new AdditionalFieldVieModel
                                               {
                                                   ID = b.ID,
                                                   Entrydata = b.Name,
                                                   Name = b.Name,
                                                   Field = b.Field
                                               }).ToList();

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
            var cn = db.Contractors
                           .Select(s => new
                           {
                               ID = s.ContractorID,
                               Name = s.ContractorCode + " " + s.ContractorName
                           })
                           .ToList();
            ViewBag.Contracto = QkSelect.List(cn, "ID", "Name");
            ViewBag.PaymentTypes = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Cash", Value="1"},
                new SelectListItem() {Text = "Cheque", Value="2"},
            }, "Value", "Text");
            var contType = db.ContractTypes
                             .Select(s => new
                             {
                                 ID = s.ID,
                                 Name = s.Name
                             })
                             .ToList();
            ViewBag.ContraType = QkSelect.List(contType, "ID", "Name");

            ViewBag.preEntry = db.Maintenances.Where(a => a.ID < id).Select(a => a.ID).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.Maintenances.Where(a => a.ID > id).Select(a => a.ID).DefaultIfEmpty().Min();

            if (pr == null)
            {
                return NotFound();
            }
            return View(vmodel);
        }
        [HttpPost]
        //[RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Edit ProMaintenance")]
        public JsonResult Edit(MaintenanceViewModel vmodel, string fnval)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            var today = Convert.ToDateTime(System.DateTime.Now);
            long Branch = 0;
            if (vmodel.PaymentType == 2 && vmodel.cheqmodel!=null)
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
            Maintenance proreg = db.Maintenances.Find(vmodel.ID);

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
            var date= DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));
            proreg.VoucherNo = vmodel.VoucherNo;
            proreg.Note = vmodel.Note;
            proreg.Remark = vmodel.Remark;
            proreg.TermsCondition = vmodel.TermsCondition;
            proreg.Note = vmodel.Note;
            proreg.Branch = Branch;
            proreg.Date = date;
            proreg.Amount = vmodel.ContractAmount;
            proreg.PaymentType = vmodel.PaymentType;
            proreg.ContractType = vmodel.ContractType;
            //proreg.Owner = vmodel.Owner;
            proreg.Property = vmodel.Property;
            proreg.Contractor = vmodel.Contractor;

            db.Entry(proreg).State = EntityState.Modified;
            db.SaveChanges();
            Int64 prId = proreg.ID;
            bool delete = com.DeleteAllAccountTransaction("Maintenance", (long)vmodel.ID);

            if (vmodel.AdditionalField != null)
            {
                db.AdditionalFieldDatas.RemoveRange(db.AdditionalFieldDatas.Where(a => a.Reference == prId && a.Purpose == "Maintenance"));
                db.SaveChanges();
                foreach (AdditionalField Ad in vmodel.AdditionalField)
                {
                    var rate = new AdditionalFieldData
                    {
                        Reference = prId,
                        Name = Ad.Name,
                        Purpose = "Maintenance",
                        Field = Ad.ID
                    };
                    db.AdditionalFieldDatas.Add(rate);
                    db.SaveChanges();
                }
            }
            if (vmodel.PaymentType == 1)
            {
                db.Cheques.RemoveRange(db.Cheques.Where(a => a.Reference == prId && a.Purpose == "PropertyRegistrations"));
                db.SaveChanges();
            }
            else if (vmodel.cheqmodel != null)
            {
                // db.Cheques.RemoveRange(db.Cheques.Where(a => a.Reference == prId && a.Purpose == "Maintenance"));
                // db.SaveChanges();
                db.PDCs.RemoveRange(db.PDCs.Where(a => a.Reference == vmodel.ID && a.PDCType == "Maintenance"));
                db.SaveChanges();
                var count = 0;
                string storePath = "";
                IFormFile file;
                foreach (var arr in vmodel.cheqmodel)
                {

                    if (arr.Amount != null)
                    {

                        var i = 0;
                        var cheqdate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));
                        Cheque cheq = new Cheque();
                        Int64 cheqid = 0;
                        //shiyas
                        if (Convert.ToInt32(arr.ID) == 0)
                        {


                            cheq.Reference = prId;
                            cheq.Purpose = "Maintenance";
                            cheq.Amount = (decimal)(arr.Amount);
                            cheq.Date = cheqdate;
                            cheq.ChequeNo = arr.ChequeNo;
                            cheq.Bank = arr.Bank;
                            db.Cheques.Add(cheq);
                            db.SaveChanges();
                            cheqid = cheq.ID;

                            storePath = LegacyWeb.MapPath("~/uploads/chequeimage/" + cheqid + arr.Attachments);
                            if (!Directory.Exists(storePath))
                                Directory.CreateDirectory(storePath);

                            // files upload
                            file = Request.Form.Files[count];
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

                                ChequeImage cheqImg = new ChequeImage();
                                cheqImg.attachments = fileNames;
                                cheqImg.Cheque = cheqid;
                                db.ChequeImages.Add(cheqImg);
                                db.SaveChanges();



                            }
                        }
                        else
                        {
                            cheq = db.Cheques.Find((long)arr.ID);
                            cheq.Reference = prId;
                            cheq.Purpose = "Maintenance";
                            cheq.Amount = (decimal)(arr.Amount);
                            cheq.Date = cheqdate;
                            cheq.ChequeNo = arr.ChequeNo;
                            cheq.Bank = arr.Bank;
                            db.Entry(cheq).State = EntityState.Modified;
                            db.SaveChanges();
                            cheqid = arr.ID;
                            storePath = LegacyWeb.MapPath("~/uploads/chequeimage/" + cheqid + arr.Attachments);
                            if (!Directory.Exists(storePath))
                                Directory.CreateDirectory(storePath);

                            // files upload
                            file = Request.Form.Files[count];
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

                      
                            PDC pd = new PDC
                            {
                                PDCDate = cheqdate,
                                PDCType = "Maintenance",
                                Reference = (long)vmodel.ID,
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
                            Company comp = db.companys.Find(1L);
                            var ContAcc = db.Tenants.Where(x => x.TenantID == vmodel.Contractor).Select(y => y.Accounts).FirstOrDefault();

                            //if payment done update to transaction
                            //com.addAccountTrasaction(0, Convert.ToDecimal(arr.Amount), ContAcc, "Maintenance", (long)vmodel.ID, DC.Credit, date, null, null, vmodel.Property, null);
                            //com.addAccountTrasaction(Convert.ToDecimal(arr.Amount), 0, (long)arr.Bank, "Maintenance", (long)vmodel.ID, DC.Debit, date, null, null, vmodel.Property, null);

                        }

                    
                    count++;
                }
            }
            else
            {

                Company comp = db.companys.Find(1L);
                var ContAcc = db.Tenants.Where(x => x.TenantID == vmodel.Contractor).Select(y => y.Accounts).FirstOrDefault();
                var conType = db.ContractTypes.Where(x => x.ID == vmodel.ContractType).Select(y => y.Account).FirstOrDefault();

                //if payment done update to transaction
                //com.addAccountTrasaction(0, vmodel.ContractAmount, ContAcc, "Maintenance", (long)vmodel.ID, DC.Credit, date, null, null, vmodel.Property, null);
                //com.addAccountTrasaction(vmodel.ContractAmount, 0, conType, "Maintenance", (long)vmodel.ID, DC.Debit, date, null, null, vmodel.Property, null);

            }
            com.addlog(LogTypes.Updated, UserId, "Maintenance", "Maintenances", findip(), prId, "Successfully Updated Maintenance Contract");
            if ((fnval) == "print")
            {
                //var data = vmodel;
                var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                var ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";
                var prodata = vmodel;
                var Data = (from a in db.Maintenances
                            join d in db.PropertyMains on a.Property equals d.Id into pro
                            from d in pro.DefaultIfEmpty()
                            join e in db.Contractors on a.Contractor equals e.ContractorID into bro
                            from e in bro.DefaultIfEmpty()
                            where a.ID == prId
                            select new
                            {
                                id = a.ID,
                                a.VoucherNo,
                                a.Date,
                                //Developer = b.co + " " + b.DeveloperName,
                                //Owner = c.Name,
                                Property = d.Name,
                                Contractor = e.ContractorName,
                                a.StartDate,
                                a.EndDate,
                                a.Amount,
                                PaymentType = (a.PaymentType == 1) ? "Cash" : "Cheque",
                                a.Remark,
                                a.Note,
                                a.TermsCondition,
                            }).FirstOrDefault();
                var additionalfield = (from a in db.AdditionalFieldDatas
                                       join b in db.AdditionalFields on a.Field equals b.ID into item
                                       from b in item.DefaultIfEmpty()
                                       where a.Reference == prId && a.Purpose == "Maintenance"
                                       select new
                                       {
                                           Field = b.Name,
                                           data = a.Name
                                       }).ToList();
                var cheque = (from a in db.Cheques
                              where a.Reference == prId && a.Purpose == "Maintenance"
                              select new
                              {
                                  chequeno = a.ChequeNo,
                                  a.Amount,
                                  a.Date
                              }).ToList();
                msg = "Successfully Updated Maintenance Contract.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Data, cheque, additionalfield, ComHeadCheck } };
                //}
            }
            msg = "Successfully Updated Maintenance Contract .";
            stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Delete ProMaintenance")]
        public ActionResult Delete(long id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Maintenance preg = db.Maintenances.Where(x => x.ID == id).FirstOrDefault();
            com.DeleteAllAccountTransaction("RegistrationDeposit", id);
            if (preg == null)
            {
                return NotFound();
            }
            return PartialView(preg);
        }

        [RedirectingAction]
        [HttpPost, ActionName("Delete")]
        //[QkAuthorize(Roles = "Dev,Delete ProMaintenance")]
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
                msg = "Successfully deleted Maintenance Contract.";
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete ProMaintenance")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteSale(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + " Maintenance Contract.", true);
            return RedirectToAction("Index", "Maintenance");
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

            db.Cheques.RemoveRange(db.Cheques.Where(a => a.Reference == Id && a.Purpose == "Maintenance"));
            db.SaveChanges();
            db.AdditionalFieldDatas.RemoveRange(db.AdditionalFieldDatas.Where(a => a.Reference == Id && a.Purpose == "Maintenance"));
            db.SaveChanges();
            Maintenance preg = db.Maintenances.Find(Id);
            db.Maintenances.Remove(preg);
            db.SaveChanges();

            db.PDCs.RemoveRange(db.PDCs.Where(o => o.Reference == Id && o.PDCType == "Maintenance"));
            db.SaveChanges();
            bool delete = com.DeleteAllAccountTransaction("Maintenance", Id);
            db.SaveChanges();
            com.addlog(LogTypes.Deleted, UserId, "Maintenance", "Maintenances", findip(), preg.ID, "Successfully Deleted Maintenance Contract");

            return true;
        }

        private string InvoiceNo(Int64 PNo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "ProMaintenance").Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == "ProMaintenance").Select(a => a.number).FirstOrDefault();

            if (billNo == null)
            {
                if ((db.Maintenances.Select(p => p.PRNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    PNo = db.Maintenances.Max(p => p.PRNo + 1);
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
            var Exists = db.Maintenances.Any(c => c.VoucherNo == VcNo);
            bool res = (Exists) ? true : false;
            return res;
        }
        private long GetPRNo()
        {
            Int64 AtNo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "ProMaintenance").Select(a => a.number).FirstOrDefault();
            if ((db.Maintenances.Select(p => p.PRNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                AtNo = (number == 0) ? 1 : number;
            }
            else
            {
                AtNo = db.Maintenances.Max(p => p.PRNo + 1);
            }

            return AtNo;
        }

        [HttpGet]
        public JsonResult GetCheque(long CnId)
        {
            var ConD = (from a in db.Cheques
                        join b in db.ChequeImages on a.ID equals b.Cheque into che
                        from b in che.DefaultIfEmpty()
                        join c in db.Accountss on a.Bank equals c.AccountsID into Acc
                        from c in Acc.DefaultIfEmpty()
                        where a.Reference == CnId && a.Purpose == "Maintenance"
                        select new
                        {
                            ID=a.ID,
                            AttAch = b.attachments,
                            Amt = a.Amount,
                            No = a.ChequeNo,
                            Date = a.Date,
                            Id = a.ID,
                            Bank = a.Bank,
                            BankName = c.Name
                        }).ToList();
            return Json(ConD);
        }

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,Download Receipt")]
        public ActionResult Download(long id)
        {
            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            string HFCheck = ComHeadCheck.ToString();

            var Data = db.Maintenances.Where(s => s.ID == id).FirstOrDefault();

            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = Data.VoucherNo;

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), HFCheck);
            return File(ms, "application/pdf", "Maintanance Voucher" + "-" + billno + ".pdf");

            //var RecDet = db.Receipts.Where(s => s.ReceiptId == id).FirstOrDefault();
            //var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            //var billno = RecDet.VoucherNo;
            //SendMail sm = new SendMail();
            //byte[] ms = sm.DownloadPdf(generatePdf(id), HFCheck);
            //return File(ms, "application/pdf", "Receipt Voucher" + "-" + billno + ".pdf");

        }

        // Custom branded, print-ready Maintenance Contract document (standalone page -> browser print / PDF).
        // Read-only; every piece materialized before shaping (EF Core 10 safe).
        [HttpGet]
        public ActionResult Print(long id)
        {
            // single record -> materialize via .Select(...).FirstOrDefault()
            var m = db.Maintenances.Where(x => x.ID == id)
                .Select(x => new
                {
                    x.ID, x.VoucherNo, x.Date, x.Property, x.Contractor, x.Amount,
                    x.StartDate, x.EndDate, x.PaymentType, x.ContractType,
                    x.Note, x.Remark, x.TermsCondition, x.CreatedDate
                }).FirstOrDefault();
            if (m == null) return NotFound();

            var comp = db.companys.Select(x => new { x.CPName, x.CPAddress, x.CPPhone, x.CPEmail, x.TRN }).FirstOrDefault();
            var hdr = db.CompanyHeaders.Select(h => h.Header).FirstOrDefault();

            long propId = m.Property;
            var prop = db.PropertyMains.Where(p => p.Id == propId)
                         .Select(p => new { p.Name, p.Code, p.City, p.State, p.Address }).FirstOrDefault();

            // Contractor (service provider) + contact via LEFT JOIN (into x from y in x.DefaultIfEmpty())
            long conId = m.Contractor;
            var contractor = (from c in db.Contractors
                              where c.ContractorID == conId
                              join ct in db.Contacts on c.Contact equals ct.ContactID into cc
                              from ct in cc.DefaultIfEmpty()
                              select new { c.ContractorName, c.ContractorCode, ct.Mobile, ct.Phone, ct.EmailId, ct.Address }).FirstOrDefault();

            long ctId = m.ContractType ?? 0;
            var contractTypeName = db.ContractTypes.Where(t => t.ID == ctId).Select(t => t.Name).FirstOrDefault();

            // related lists -> materialize via .Select(...).ToList()
            var cheques = db.Cheques.Where(ab => ab.Reference == m.ID && ab.Purpose == "Maintenance")
                            .Select(ab => new { ab.ChequeNo, ab.Amount, ab.Date }).ToList();
            var fields = (from a in db.AdditionalFieldDatas
                          join b in db.AdditionalFields on a.Field equals b.ID into ff
                          from b in ff.DefaultIfEmpty()
                          where a.Reference == m.ID && a.Purpose == "Maintenance"
                          select new { name = b.Name, data = a.Name }).ToList();

            ViewBag.Id = m.ID;
            ViewBag.Code = string.IsNullOrWhiteSpace(m.VoucherNo) ? ("MC-" + m.ID) : m.VoucherNo;
            ViewBag.CompName = comp != null ? comp.CPName : "Company";
            ViewBag.CompAddr = comp != null ? comp.CPAddress : "";
            ViewBag.CompPhone = comp != null ? comp.CPPhone : "";
            ViewBag.CompEmail = comp != null ? comp.CPEmail : "";
            ViewBag.CompTRN = comp != null ? comp.TRN : "";
            ViewBag.HeaderImg = string.IsNullOrEmpty(hdr) ? "" : ("/uploads/companyheader/header/" + hdr);
            ViewBag.PropName = prop != null ? prop.Name : "-";
            ViewBag.PropCode = prop != null ? (prop.Code ?? "") : "";
            ViewBag.PropAddr = prop == null ? "" : ((prop.Address ?? "") + " " + (prop.City ?? "") + " " + (prop.State ?? "")).Trim();
            ViewBag.ContractorName = contractor != null ? (contractor.ContractorName ?? "-") : "-";
            ViewBag.ContractorCode = contractor != null ? (contractor.ContractorCode ?? "") : "";
            ViewBag.ContractorContact = contractor == null ? "" : (((contractor.Mobile ?? contractor.Phone) ?? "") + (string.IsNullOrEmpty(contractor.EmailId) ? "" : "  ·  " + contractor.EmailId));
            ViewBag.ContractorAddr = contractor != null ? (contractor.Address ?? "") : "";
            ViewBag.IssueDate = m.Date.ToString("dd MMM yyyy");
            ViewBag.StartDate = string.IsNullOrWhiteSpace(m.StartDate) ? "-" : m.StartDate;
            ViewBag.EndDate = string.IsNullOrWhiteSpace(m.EndDate) ? "-" : m.EndDate;
            ViewBag.Amount = m.Amount.ToString("#,##0.00");
            ViewBag.ContractType = string.IsNullOrWhiteSpace(contractTypeName) ? "-" : contractTypeName;
            ViewBag.PaymentMode = (m.PaymentType == 1) ? "Cash" : "Cheque";
            ViewBag.TnC = m.TermsCondition ?? "";
            ViewBag.Note = m.Note ?? "";
            ViewBag.Remark = m.Remark ?? "";
            ViewBag.Cheques = System.Text.Json.JsonSerializer.Serialize(
                cheques.Select(x => new { no = x.ChequeNo ?? "-", amt = x.Amount.ToString("#,##0.00"), date = x.Date.ToString("dd-MM-yyyy") }).ToList());
            ViewBag.Fields = System.Text.Json.JsonSerializer.Serialize(
                fields.Select(x => new { name = x.name ?? "-", data = x.data ?? "-" }).ToList());
            return View();
        }

        public StringBuilder generatePdf(long id)
        {
            var details = (from a in db.Maintenances
                           join d in db.PropertyMains on a.Property equals d.Id into pro
                           from d in pro.DefaultIfEmpty()
                           join e in db.Contractors on a.Contractor equals e.ContractorID into bro
                           from e in bro.DefaultIfEmpty()
                           where a.ID == id
                           select new
                           {
                               id = a.ID,
                               a.VoucherNo,
                               a.Date,
                               //Developer = b.co + " " + b.DeveloperName,
                               //Owner = c.Name,
                               Property = d.Name,
                               Contractor = e.ContractorName,
                               a.StartDate,
                               a.EndDate,
                               a.Amount,
                               PaymentType = (a.PaymentType == 1) ? "Cash" : "Cheque",
                               a.Remark,
                               a.Note,
                               a.TermsCondition,
                           }).FirstOrDefault();
            var additionalfield = (from a in db.AdditionalFieldDatas
                                   join b in db.AdditionalFields on a.Field equals b.ID into item
                                   from b in item.DefaultIfEmpty()
                                   where a.Reference == id && a.Purpose == "Maintenance"
                                   select new
                                   {
                                       Field = b.Name,
                                       data = a.Name
                                   }).ToList();
            var cheque = (from a in db.Cheques
                          where a.Reference == id && a.Purpose == "Maintenance"
                          select new
                          {
                              chequeno = a.ChequeNo,
                              a.Amount,
                              a.Date
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
                    sb.Append("<table width='100%' style='border: 0px;text-align:center;'><tr><td><b>Maintanance Contract Voucher</b></td></tr></table>");

                    string partyDetails = "<table style='border:.1px #ccc;' width='100%'><tr style='border-top:.1px #ccc; '> " +
                        "<td width='50%' style='border - right: 0px;'> " +
                        "<table  style='border: 0px; width: 100 %;'><tr><td style='font-size:14px;font-weight:normal;'><b>Invoice No </b>: " + details.VoucherNo + "</td><td style='font-size:14px;font-weight:normal;'><b>Date</b> : " + details.Date.ToString("dd-MM-yyyy") + "</td></tr>" +
                        "< tr >< td style = 'font-size:14px;font-weight:normal;' >< b > Property </ b >: " + details.Property + " </ td >< td style = 'font-size:14px;font-weight:normal;' >< b > Contractor </ b > : " + details.Contractor + " </ td ></ tr > " +
                        "< tr >< td style = 'font-size:14px;font-weight:normal;' >< b > Start Date </ b >: " + details.StartDate + " </ td >< td style = 'font-size:14px;font-weight:normal;' >< b > End Date </ b > : " + details.EndDate + " </ td ></ tr > " +
                        "< tr >< td style = 'font-size:14px;font-weight:normal;' >< b > Amount </ b >: " + details.Amount + " </ td >< td style = 'font-size:14px;font-weight:normal;' >< b > Payment Type  </ b > : " + details.PaymentType + " </ td ></ tr ></table> " +
                        "</td>" +
                        "<td style='border: 0px;'>";
                    partyDetails += "</td></tr></table>";

                    sb.Append(partyDetails);
                    if (additionalfield.Count > 0)
                    {
                        string addfield = "<table style='border:.1px #ccc;' width='100%'><tr style='border-top:.1px #ccc; '> < td width = '50%' style = 'border - right: 0px;' >  ";

                        foreach (var arr in additionalfield)
                        {
                            if (arr.data != "0" && arr.data != "")
                            {
                                addfield += "<table style='border: 0px;'><tr style='border: 0px;'><td style='font-size:14px;font-weight:normal;'>" + arr.Field + "    " + arr.data + "</td></tr></table>";
                            }
                        }

                        addfield += "</td></tr></table>";

                        sb.Append(addfield);
                    }
                    //Cheque table
                    if (cheque.Count > 0)
                    {
                        sb.Append("<table width='100%' style='border-collapse:collapse;font-size:12px;border: .1px solid #ccc; repeat-header:yes;'>");
                        sb.Append("<thead>");
                        sb.Append("<tr style='font-size:13px;'>");
                        sb.Append("<th width='15%' style='padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Cheque No</th>");
                        sb.Append("<th style='padding: 5px;vertical-align: top;border: .1px solid #ccc;'>Date</th>");
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
            MaintenanceViewModel cusmodel = new MaintenanceViewModel();
            cusmodel = (from a in db.Maintenances
                        join b in db.Contractors on a.Contractor equals b.ContractorID into tmp
                        from b in tmp.DefaultIfEmpty()
                        join c in db.PropertyMains on a.Property equals c.Id into pro
                        from c in pro.DefaultIfEmpty()
                        where a.ID == id
                        select new MaintenanceViewModel
                        {
                            ID = a.ID,
                            ContractorName = b.ContractorName,
                            PropertyName=c.Name,
                            CreatedDate = a.CreatedDate,
                            Payment = (a.PaymentType == 1) ? "Cash" : "Cheque",
                            StartDate = a.StartDate,
                            EndDate = a.EndDate,
                            ContractAmount=a.Amount,
                            Remark = a.Remark,
                            Note = a.Note,
                            TermsCondition = a.TermsCondition,
                            cheqmodel = (from ab in db.Cheques
                                         where (ab.Reference == a.ID && ab.Purpose == "Maintenance")
                                         select new ChequeViewModel
                                         {
                                             //Date = ab.Date.ToString("dd-MM-yyyy"),   Maintenance
                                             ChequeNo = ab.ChequeNo,
                                             Amount = ab.Amount,
                                             ViewDate = ab.Date
                                         }).ToList(),
                            AdditionalFieldVieModels = (from aa in db.AdditionalFieldDatas
                                                        join ab in db.AdditionalFields on aa.Field equals ab.ID into item
                                                        from ab in item.DefaultIfEmpty()
                                                        where aa.Reference == id && aa.Purpose == "Maintenance"
                                                        select new AdditionalFieldVieModel
                                                        {
                                                            Name = ab.Name,
                                                            Entrydata = aa.Name
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