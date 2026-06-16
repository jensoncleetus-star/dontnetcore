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
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Areas.Property.Controllers
{
    [Microsoft.AspNetCore.Mvc.Area("Property")]
    public class RentalProformaController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public RentalProformaController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Property/Rental
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
        //[QkAuthorize(Roles = "Dev, Rental")]
        public ActionResult GetData(string InvoiceNo, string FromDate, string ToDate, long? Tenant, long? Property)
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
            var UserView = (from a in db.RentalProformas
                            join b in db.Tenants on a.Tenant equals b.TenantID into dev
                            from b in dev.DefaultIfEmpty()
                            join d in db.PropertyMains on a.Property equals d.Id into pro
                            from d in pro.DefaultIfEmpty()
                            where
                              (InvoiceNo == "" || a.VoucherNo == InvoiceNo) &&
                               (FromDate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                               (ToDate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                              (Tenant == 0 || Tenant == null || a.Tenant == Tenant) &&
                              (Property == 0 || Property == null || a.Property == Property)
                            select new
                            {
                                id = a.ID,
                                a.VoucherNo,
                                a.Date,
                                Tenant = b.TenantCode + " " + b.TenantName,
                                Property = d.Code + " " + d.Name,
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
        public ActionResult Create()
        {
            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;


            var dev = db.Tenants.Select(s => new
            {
                TenantID = s.TenantID,
                TenantDetails = s.TenantCode + " - " + s.TenantName
            }).ToList();
            ViewBag.Tenan = QkSelect.List(dev, "TenantID", "TenantDetails");

            var pro = db.PropertyMains
                             .Select(s => new
                             {
                                 ID = s.Id,
                                 Name = s.Code + " " + s.Name
                             })
                             .ToList();
            ViewBag.Proper = QkSelect.List(pro, "ID", "Name");
            var unii = db.PropertyUnits
                             .Select(s => new
                             {
                                 ID = s.Id,
                                 Name = s.Code + " " + s.Name
                             })
                             .ToList();
            ViewBag.Uni = QkSelect.List(unii, "ID", "Name");
            ViewBag.LastEntry = db.RentalProformas.Select(x => x.ID).AsEnumerable().DefaultIfEmpty(0).Max();

            var viewModel = new ProRentalViewModel
            {
                VoucherNo = InvoiceNo(),
                Date = System.DateTime.Now.ToString("dd-MM-yyyy"),
                AdditionalField = db.AdditionalFields.Where(x=>x.Section== "Rental Proforma").ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Create Rental")]
        public JsonResult Create(ProRentalViewModel vmodel, string fnval)
        {
            string msg = "";
            bool stat = false;
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


                RentalProforma preg = new RentalProforma();
                preg.VoucherNo = vmodel.VoucherNo;
                preg.PRNo = GetPRNo();
                preg.Date = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));

                preg.Tenant = vmodel.Tenant;
                preg.Property = vmodel.Property;
                preg.Unit = vmodel.Unit;

                preg.Branch = Branch;
                preg.Note = vmodel.Note;
                preg.Remark = vmodel.Remark;
                preg.TermsCondition = vmodel.TermsCondition;

                preg.Amount = vmodel.Amount;

                preg.CreatedDate = today;
                preg.CreatedBy = UserId;
                preg.Status = Status.active;
                db.RentalProformas.Add(preg);
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
                            Purpose = "Rental Proforma",
                            Field=Ad.ID
                        };
                        db.AdditionalFieldDatas.Add(rate);
                        db.SaveChanges();
                    }
                }

                com.addlog(LogTypes.Created, UserId, "RentalProforma", "RentalProformas", findip(), preg.ID, "Successfully Submitted Rental Proforma");
                if ((fnval) == "print")
                {
                    //var data = vmodel;
                    var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                    var ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";
                    var prodata = vmodel;
                    var Data = (from a in db.RentalProformas
                                join b in db.Tenants on a.Tenant equals b.TenantID into dev
                                from b in dev.DefaultIfEmpty()
                                join d in db.PropertyMains on a.Property equals d.Id into pro
                                from d in pro.DefaultIfEmpty()
                                join e in db.PropertyUnits on a.Unit equals e.Id into uni
                                from e in uni.DefaultIfEmpty()
                                where a.ID == ID
                                select new
                                {
                                    id = a.ID,
                                    a.VoucherNo,
                                    a.Date,
                                    Tenant = b.TenantName,
                                    Property = d.Name,
                                    a.Remark,
                                    a.Note,
                                    a.TermsCondition,
                                    Unit = e.Name,
                                    a.Amount
                                }).FirstOrDefault();
                    var additionalfield = (from a in db.AdditionalFieldDatas
                                           join b in db.AdditionalFields on a.Field equals b.ID into item
                                           from b in item.DefaultIfEmpty()
                                           where a.Reference == ID && a.Purpose == "Rental Proforma"
                                           select new
                                           {
                                               Field = b.Name,
                                               data = a.Name
                                           }).ToList();

                    msg = "Successfully Created Rental Proforma.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Data, additionalfield, ComHeadCheck } };
                    //}
                }
                msg = "Successfully Created Rental Proforma.";
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

        public ActionResult Edit(long? id)
        {
            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;
            var dev = db.Tenants.Select(s => new
            {
                TenantID = s.TenantID,
                TenantDetails = s.TenantCode + " - " + s.TenantName
            }).ToList();
            ViewBag.Tenan = QkSelect.List(dev, "TenantID", "TenantDetails");

            var pro = db.PropertyMains
                             .Select(s => new
                             {
                                 ID = s.Id,
                                 Name = s.Code + " " + s.Name
                             })
                             .ToList();
            ViewBag.Proper = QkSelect.List(pro, "ID", "Name");
            var unii = db.PropertyUnits
                             .Select(s => new
                             {
                                 ID = s.Id,
                                 Name = s.Code + " " + s.Name
                             })
                             .ToList();
            ViewBag.Uni = QkSelect.List(unii, "ID", "Name");
            RentalProforma pr = db.RentalProformas.Find(id);

            ProRentalViewModel vmodel = new ProRentalViewModel();
            vmodel.ID = (long)id;
            vmodel.VoucherNo = pr.VoucherNo;
            vmodel.Note = pr.Note;
            vmodel.Remark = pr.Remark;
            vmodel.TermsCondition = pr.TermsCondition;
            vmodel.Date = pr.Date.ToString("dd-MM-yyyy");

            vmodel.Tenant = pr.Tenant;
            vmodel.Amount = pr.Amount;
            vmodel.Unit = pr.Unit;
            vmodel.Property = pr.Property;

            vmodel.AdditionalField = db.AdditionalFields.Where(x => x.Section == "Rental Proforma").ToList();
            vmodel.AdditionalFieldVieModels = (from b in db.AdditionalFieldDatas
                                               where b.Reference == id && b.Purpose == "Rental Proforma"
                                               select new AdditionalFieldVieModel
                                               {
                                                   ID = b.ID,
                                                   Entrydata = b.Name,
                                                   Name = b.Name,
                                                   Field=b.Field
                                               }).ToList();
            ViewBag.preEntry = db.RentalProformas.Where(a => a.ID < id).Select(a => a.ID).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.RentalProformas.Where(a => a.ID > id).Select(a => a.ID).DefaultIfEmpty().Min();
            return View(vmodel);
        }
        [HttpPost]
        //[RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Edit Rental")]
        public JsonResult Update(ProRentalViewModel vmodel, string fnval)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            var today = Convert.ToDateTime(System.DateTime.Now);
            long Branch = 0;

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            RentalProforma proreg = db.RentalProformas.Find(vmodel.ID);

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
            proreg.Date = DateTime.Parse(vmodel.Date.ToString(), new CultureInfo("en-GB"));

            proreg.Tenant = vmodel.Tenant;
            proreg.Unit = vmodel.Unit;
            proreg.Property = vmodel.Property;
            proreg.Amount = vmodel.Amount;

            db.Entry(proreg).State = EntityState.Modified;
            db.SaveChanges();
            Int64 prId = proreg.ID;

            if (vmodel.AdditionalField != null)
            {
                db.AdditionalFieldDatas.RemoveRange(db.AdditionalFieldDatas.Where(a => a.Reference == prId && a.Purpose == "Rental Proforma"));
                foreach (AdditionalField Ad in vmodel.AdditionalField)
                {
                    var rate = new AdditionalFieldData
                    {
                        Reference = prId,
                        Name = Ad.Name,
                        Purpose = "RentalProforma",
                        Field=Ad.ID
                    };
                    db.AdditionalFieldDatas.Add(rate);
                    db.SaveChanges();
                }
            }

            com.addlog(LogTypes.Updated, UserId, "RentalProforma", "RentalProformas", findip(), prId, "Successfully Updated Rental Proforma");
            if ((fnval) == "print")
            {
                //var data = vmodel;
                var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
                var ComHeadCheck = EnableHead != null ? (EnableHead.Status == Status.active ? "0" : "1") : "1";
                var prodata = vmodel;
                var Data = (from a in db.RentalProformas
                            join b in db.Tenants on a.Tenant equals b.TenantID into dev
                            from b in dev.DefaultIfEmpty()
                            join d in db.PropertyMains on a.Property equals d.Id into pro
                            from d in pro.DefaultIfEmpty()
                            join e in db.PropertyUnits on a.Unit equals e.Id into uni
                            from e in uni.DefaultIfEmpty()
                            where a.ID == prId
                            select new
                            {
                                id = a.ID,
                                a.VoucherNo,
                                a.Date,
                                Tenant = b.TenantName,
                                Property = d.Name,
                                a.Remark,
                                a.Note,
                                a.TermsCondition,
                                Unit = e.Name,
                                a.Amount
                            }).FirstOrDefault();
                var additionalfield = (from a in db.AdditionalFieldDatas
                                       join b in db.AdditionalFields on a.Field equals b.ID into item
                                       from b in item.DefaultIfEmpty()
                                       where a.Reference == prId && a.Purpose == "Rental"
                                       select new
                                       {
                                           Field = b.Name,
                                           data = a.Name
                                       }).ToList();

                msg = "Successfully Updated Rental Proforma.";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Data, additionalfield, ComHeadCheck } };
                //}
            }
            msg = "Successfully Updated Rental Proforma.";
            stat = true;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        [RedirectingAction]
        //[QkAuthorize(Roles = "Dev,Delete Rental")]
        public ActionResult Delete(long id)
        {
            RentalProforma preg = db.RentalProformas.Where(x => x.ID == id).FirstOrDefault();

            if (preg == null)
            {
                return NotFound();
            }
            return PartialView(preg);
        }

        [RedirectingAction]
        [HttpPost, ActionName("Delete")]
        //[QkAuthorize(Roles = "Dev,Delete Rental")]
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
                msg = "Successfully deleted Rental Proforma.";
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Delete Rental")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteSale(arr) == true) ? count++ : count;
            }
            Success("Deleted " + count + " Rental Proforma.", true);
            return RedirectToAction("Index", "RentalProforma");
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
            db.AdditionalFieldDatas.RemoveRange(db.AdditionalFieldDatas.Where(a => a.Reference == Id && a.Purpose == "Rental Proforma"));

            RentalProforma preg = db.RentalProformas.Find(Id);
            db.RentalProformas.Remove(preg);
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "RentalProforma", "RentalProformas", findip(), preg.ID, "Successfully Deleted Rental Proforma");

            return true;
        }


        private string InvoiceNo(Int64 PNo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "ProRentalInvoice").Select(a => a.prefix).FirstOrDefault();
            Int32 number = db.CodePrefixs.Where(a => a.section == "ProRentalInvoice").Select(a => a.number).FirstOrDefault();

            if (billNo == null)
            {
                if ((db.RentalProformas.Select(p => p.PRNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    PNo = db.RentalProformas.Max(p => p.PRNo + 1);
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
            var Exists = db.RentalProformas.Any(c => c.VoucherNo == VcNo);
            bool res = (Exists) ? true : false;
            return res;
        }
        private long GetPRNo()
        {
            Int64 AtNo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "ProRentalInvoice").Select(a => a.number).FirstOrDefault();
            if ((db.RentalProformas.Select(p => p.PRNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                AtNo = (number == 0) ? 1 : number;
            }
            else
            {
                AtNo = db.RentalProformas.Max(p => p.PRNo + 1);
            }

            return AtNo;
        }

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,Download Receipt")]
        public ActionResult Download(long id)
        {
            var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
            var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;
            string HFCheck = ComHeadCheck.ToString();

            var Data = db.RentalProformas.Where(s => s.ID == id).FirstOrDefault();

            var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            var billno = Data.VoucherNo;

            SendMail sm = new SendMail();
            byte[] ms = sm.DownloadPdf(generatePdf(id), HFCheck);
            return File(ms, "application/pdf", "Rental Proforma Voucher" + "-" + billno + ".pdf");

            //var RecDet = db.Receipts.Where(s => s.ReceiptId == id).FirstOrDefault();
            //var cname = db.companys.Select(s => s.CPName).FirstOrDefault();
            //var billno = RecDet.VoucherNo;
            //SendMail sm = new SendMail();
            //byte[] ms = sm.DownloadPdf(generatePdf(id), HFCheck);
            //return File(ms, "application/pdf", "Receipt Voucher" + "-" + billno + ".pdf");

        }

        // Custom branded, print-ready Rental Proforma document (standalone page -> browser print / PDF).
        // Read-only; every piece materialized before shaping (EF Core 10 safe).
        [HttpGet]
        public ActionResult Print(long id)
        {
            var r = db.RentalProformas.Where(x => x.ID == id)
                .Select(x => new
                {
                    x.ID, x.VoucherNo, x.Date, x.Tenant, x.Property, x.Unit,
                    x.Amount, x.Note, x.Remark, x.TermsCondition, x.CreatedDate
                }).FirstOrDefault();
            if (r == null) return NotFound();

            var comp = db.companys.Select(x => new { x.CPName, x.CPAddress, x.CPPhone, x.CPEmail, x.TRN }).FirstOrDefault();
            var hdr = db.CompanyHeaders.Select(h => h.Header).FirstOrDefault();

            long propId = r.Property;
            var prop = db.PropertyMains.Where(p => p.Id == propId)
                         .Select(p => new { p.Name, p.Code, p.City, p.State, p.Address }).FirstOrDefault();

            long tnId = r.Tenant;
            var tenant = (from t in db.Tenants
                          where t.TenantID == tnId
                          join ct in db.Contacts on t.Contact equals ct.ContactID into cc from ct in cc.DefaultIfEmpty()
                          select new { t.TenantName, t.TenantCode, ct.Mobile, ct.Phone, ct.EmailId, ct.Address }).FirstOrDefault();

            long unitId = r.Unit;
            var unit = db.PropertyUnits.Where(u => u.Id == unitId).Select(u => new { u.Name, u.Code }).FirstOrDefault();

            var fields = (from a in db.AdditionalFieldDatas
                          where a.Reference == r.ID && a.Purpose == "Rental Proforma"
                          join b in db.AdditionalFields on a.Field equals b.ID into item from b in item.DefaultIfEmpty()
                          select new { label = b.Name, val = a.Name }).ToList();

            ViewBag.Id = r.ID;
            ViewBag.Code = string.IsNullOrEmpty(r.VoucherNo) ? ("RPF-" + r.ID) : r.VoucherNo;
            ViewBag.CompName = comp != null ? comp.CPName : "Company";
            ViewBag.CompAddr = comp != null ? comp.CPAddress : "";
            ViewBag.CompPhone = comp != null ? comp.CPPhone : "";
            ViewBag.CompEmail = comp != null ? comp.CPEmail : "";
            ViewBag.CompTRN = comp != null ? comp.TRN : "";
            ViewBag.HeaderImg = string.IsNullOrEmpty(hdr) ? "" : ("/uploads/companyheader/header/" + hdr);
            ViewBag.PropName = prop != null ? prop.Name : "-";
            ViewBag.PropAddr = prop == null ? "" : ((prop.Address ?? "") + " " + (prop.City ?? "") + " " + (prop.State ?? "")).Trim();
            ViewBag.UnitName = unit != null ? (unit.Name ?? "-") : "-";
            ViewBag.TenantName = tenant != null ? (tenant.TenantName ?? "-") : "-";
            ViewBag.TenantContact = tenant == null ? "" : (((tenant.Mobile ?? tenant.Phone) ?? "") + (string.IsNullOrEmpty(tenant.EmailId) ? "" : "  ·  " + tenant.EmailId));
            ViewBag.TenantAddr = tenant != null ? (tenant.Address ?? "") : "";
            ViewBag.TenantCode = tenant != null ? (tenant.TenantCode ?? "") : "";
            ViewBag.InvoiceDate = r.Date.ToString("dd MMM yyyy");
            ViewBag.Amount = r.Amount.ToString("#,##0.00");
            ViewBag.Note = r.Note ?? "";
            ViewBag.Remark = r.Remark ?? "";
            ViewBag.TnC = r.TermsCondition ?? "";
            ViewBag.Lines = System.Text.Json.JsonSerializer.Serialize(
                new[] { new { desc = "Rent — " + (prop != null ? (prop.Name ?? "Property") : "Property")
                                     + (unit != null && !string.IsNullOrEmpty(unit.Name) ? " / " + unit.Name : ""),
                              amt = r.Amount.ToString("#,##0.00") } });
            ViewBag.Fields = System.Text.Json.JsonSerializer.Serialize(
                fields.Where(x => !string.IsNullOrWhiteSpace(x.val) && x.val != "0")
                      .Select(x => new { label = x.label ?? "Field", val = x.val }).ToList());
            return View();
        }

        public StringBuilder generatePdf(long id)
        {
            var details = (from a in db.RentalProformas
                           join b in db.Tenants on a.Tenant equals b.TenantID into dev
                           from b in dev.DefaultIfEmpty()
                           join d in db.PropertyMains on a.Property equals d.Id into pro
                           from d in pro.DefaultIfEmpty()
                           join e in db.PropertyUnits on a.Unit equals e.Id into uni
                           from e in uni.DefaultIfEmpty()
                           where a.ID == id
                           select new
                           {
                               id = a.ID,
                               a.VoucherNo,
                               Date = a.Date,
                               Tenant = b.TenantName,
                               Property = d.Name,
                               a.Remark,
                               a.Note,
                               a.TermsCondition,
                               Unit = e.Name,
                               a.Amount
                           }).FirstOrDefault();
            var additionalfield = (from a in db.AdditionalFieldDatas
                                   join b in db.AdditionalFields on a.Field equals b.ID into item
                                   from b in item.DefaultIfEmpty()
                                   where a.Reference == id && a.Purpose == "RentalProforma"
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
                    sb.Append("<table width='100%' style='border: 0px;text-align:center;'><tr><td><b>Rental Proforma</b></td></tr></table>");

                    string partyDetails = "<table style='border:.1px #ccc;' width='100%'><tr style='border-top:.1px #ccc; '> " +
                        "<td width='50%' style='border - right: 0px;'> " +
                        "<table  style='border: 0px; width: 100 %;'><tr><td style='font-size:14px;font-weight:normal;'><b>Invoice No </b>: " + details.VoucherNo + "</td><td style='font-size:14px;font-weight:normal;'><b>Date</b> : " + details.Date.ToString("dd-MM-yyyy") + "</td></tr>" +
                        "< tr >< td style = 'font-size:14px;font-weight:normal;' >< b > Property </ b >: " + details.Property + " </ td >< td style = 'font-size:14px;font-weight:normal;' >< b > Tenant </ b > : " + details.Tenant + " </ td ></ tr > " +
                        "< tr >< td style = 'font-size:14px;font-weight:normal;' >< b > Unit </ b >: " + details.Unit + " </ td ></ tr > " +
                        "< tr >< td style = 'font-size:14px;font-weight:normal;' >< b > Amount </ b >: " + details.Amount + "  AED </ td ></ tr ></table> " +
                        "</td>" +
                        "<td style='border: 0px;'>";
                    partyDetails += "</td></tr></table>";

                    sb.Append(partyDetails);

                    string addfield = "<table style='border:.1px #ccc;' width='100%'><tr style='border-top:.1px #ccc; '> < td width = '50%' style = 'border - right: 0px;' >  ";
                    if (additionalfield.Count > 0)
                    {
                        foreach (var arr in additionalfield)
                        {
                            if (arr.data != "0" && arr.data != "")
                            {
                                addfield += "<table style='border: 0px;'><tr style='border: 0px;'><td style='font-size:14px;font-weight:normal;'>" + arr.Field + "    " + arr.data + "</td></tr></table>";
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
                    if (details.Note != null)
                    {
                        sb.Append("<table width='100%' style='border: .1px solid #ccc;border-collapse:collapse;'>");
                        sb.Append("<tr style='font-size:10px;'><td style='font-size: 12px;padding: 6px;vertical-align: top;border: .1px solid #ccc;'><b>Note</b> :  " + details.Note + "</td></tr>");
                        sb.Append("</table>");
                    }
                    if (details.TermsCondition != null)
                    {
                        sb.Append("<table width='100%' style='border: .1px solid #ccc;border-collapse:collapse;'>");
                        sb.Append("<tr style='font-size:10px;'><td style='font-size: 12px;padding: 6px;vertical-align: top;border: .1px solid #ccc;'><b>Terms and Condition</b> :  " + details.TermsCondition + "</td></tr>");
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
            RentalViewModel cusmodel = new RentalViewModel();
            cusmodel = (from a in db.RentalProformas
                        join b in db.Tenants on a.Tenant equals b.TenantID into tmp
                        from b in tmp.DefaultIfEmpty()
                        join c in db.PropertyMains on a.Property equals c.Id into pro
                        from c in pro.DefaultIfEmpty()
                        join d in db.PropertyUnits on a.Unit equals d.Id into uni
                        from d in uni.DefaultIfEmpty()
                        where a.ID == id
                        select new RentalViewModel
                        {
                            RentalID = a.ID,
                            TenantName = b.TenantName,
                            Date = a.Date,
                            PropertyName = c.Name,
                            UnitName = d.Name,
                            Remark = a.Remark,
                            Note = a.Note,
                            TermsCondition = a.TermsCondition,
                            Amount = a.Amount,
                            AdditionalFieldVieModels = (from aa in db.AdditionalFieldDatas
                                                        join ab in db.AdditionalFields on aa.Field equals ab.ID into item
                                                        from ab in item.DefaultIfEmpty()
                                                        where aa.Reference == id && aa.Purpose == "RentalProforma"
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