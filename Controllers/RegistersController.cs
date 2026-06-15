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
using QuickSoft.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    public class RegistersController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public RegistersController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Registers
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Payment()
        {
            var Paidfrom = db.Accountss.Where(p => p.Group == 9 || p.Group == 8).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.Paidfrom = QkSelect.List(Paidfrom, "ID", "Name");

            ViewBag.PaidTo = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                           }, "Value", "Text", 1);

            _FinancialYear();
            companySet();
            return View();
        }
        public ActionResult Receipt()
        {
            var PaidTo = db.Accountss.Where(p => p.Group == 9 || p.Group == 8).Select(r => new
            {
                ID = r.AccountsID,
                Name = r.Name
            }).ToList();
            ViewBag.PaidTo = QkSelect.List(PaidTo, "ID", "Name");

            ViewBag.Paidfrom = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);
            companySet();
            _FinancialYear();
            return View();
        }

        //[QkAuthorize(Roles = "Dev,Receipt Register")]
        public ActionResult GetReceipt(string vno, long? payfrom, long? payto, string fromdate, string todate, int[] MOPay)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            List<ModeOfPayment> Mop = new List<ModeOfPayment>();
            if (MOPay != null && MOPay.Contains(1))
            {
                Mop.Add(ModeOfPayment.Cash);
            }
            if (MOPay != null && MOPay.Contains(2))
            {
                Mop.Add(ModeOfPayment.PDC);
            }
            if (MOPay != null && MOPay.Contains(3))
            {
                Mop.Add(ModeOfPayment.CDC);
            }
            var count = Mop.Count();
            var v = (from a in db.Receipts
                     join b in db.Accountss on a.PayFrom equals b.AccountsID into payf
                     from b in payf.DefaultIfEmpty()
                     join c in db.Accountss on a.PayTo equals c.AccountsID into payt
                     from c in payt.DefaultIfEmpty()
                     where (vno == "" || a.VoucherNo == vno) &&
                      (payfrom == 0 || a.PayFrom == payfrom) &&
                      (payto == 0 || payto == null || a.PayTo == payto) &&
                     (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                     (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                     a.editable == choice.Yes
                     && ((count == 0) || (Mop.Contains(a.MOPayment)))
                     select new
                     {
                         VoucherNo = a.VoucherNo,
                         Payer = b.Name,
                         Reciever = c.Name,
                         a.ReceiptId,
                         a.Date,
                         a.MOPayment,
                         a.PDCDate,
                         a.PayFrom,
                         a.PayTo,
                         a.SubTotal,
                         a.GrandTotal,
                         a.Paying,
                         a.CreatedDate,
                         a.Remark,
                         a.Discount,
                         DiscountAc="Discount Allowed",
                         Details= (from y in db.Receipts
                                   join w in db.ReceiptBills on y.ReceiptId equals w.Receipt
                                   where a.ReceiptId == y.ReceiptId
                                   select new
                                   {
                                       RefType=(w.Type== "Against Reference" || w.Type== "New Reference")?"":w.Type,
                                       Amount=w.Amount,
                                       Reference= (w.Type == "Against Reference" || w.Type == "New Reference") ? w.NewRefName: "",
                                       Payto =a.PayTo,
                                       Dicount=a.Discount,
                                       a.Paying
                                   }).ToList(),
                     }).AsEnumerable().Select(o => new
                     {
                         o.VoucherNo,
                         o.Payer,
                         o.Reciever,
                         o.ReceiptId,
                         o.Date,
                         modeofpay = Enum.GetName(typeof(ModeOfPayment), o.MOPayment),
                         o.MOPayment,
                         o.PayFrom,
                         o.PayTo,
                         o.SubTotal,
                         o.GrandTotal,
                         o.Paying,
                         o.Discount,
                         o.DiscountAc,
                         o.Details,
                         DebtAmt=""
                     }).OrderBy(a => a.Date);

            var data = v.Skip(skip).Take(pageSize).ToList();
            

            recordsTotal = v.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data});
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

        }

        //[QkAuthorize(Roles = "Dev,Payment Register")]
        public ActionResult GetPayment(string vno, long? payfrom, long? payto, string fromdate, string todate, int[] MOPay)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            int recordsTotal = 0;
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (fromdate != "")
            {
                fdate = DateTime.Parse(fromdate, new CultureInfo("en-GB"));
            }
            if (todate != "")
            {
                tdate = DateTime.Parse(todate, new CultureInfo("en-GB"));
            }
            List<ModeOfPayment> Mop = new List<ModeOfPayment>();
            if (MOPay != null && MOPay.Contains(1))
            {
                Mop.Add(ModeOfPayment.Cash);
            }
            if (MOPay != null && MOPay.Contains(2))
            {
                Mop.Add(ModeOfPayment.PDC);
            }
            if (MOPay != null && MOPay.Contains(3))
            {
                Mop.Add(ModeOfPayment.CDC);
            }
            var count = Mop.Count();
            var v = (from a in db.Payments
                     join b in db.Accountss on a.PayFrom equals b.AccountsID into payf
                     from b in payf.DefaultIfEmpty()
                     join c in db.Accountss on a.PayTo equals c.AccountsID into payt
                     from c in payt.DefaultIfEmpty()
                     where (vno == "" || a.VoucherNo == vno) &&
                    (payfrom == 0 || payfrom == null || a.PayFrom == payfrom) &&
                    (payto == 0 || a.PayTo == payto) &&
                    (fromdate == "" || EF.Functions.DateDiffDay(a.Date, fdate) <= 0) &&
                    (todate == "" || EF.Functions.DateDiffDay(a.Date, tdate) >= 0) &&
                    a.editable == choice.Yes
                    && ((count == 0) || (Mop.Contains(a.MOPayment)))
                     select new
                     {
                         VoucherNo = a.VoucherNo,
                         Payer = b.Name,
                         Reciever = c.Name,
                         a.PaymentId,
                         a.Date,
                         a.MOPayment,
                         a.PDCDate,
                         a.PayFrom,
                         a.PayTo,
                         a.TaxAmount,
                         a.SubTotal,
                         a.GrandTotal,
                         a.Paying,
                         a.CreatedDate,
                         a.Discount,
                         a.Remark,
                         DiscountAc = "Discount Received",
                         Details = (from y in db.Payments
                                    join w in db.PaymentBills on y.PaymentId equals w.Payment
                                    where a.PaymentId == y.PaymentId
                                    select new
                                    {
                                        RefType = (w.Type == "Against Reference" || w.Type == "New Reference") ? "" : w.Type,
                                        Amount = w.Amount,
                                        Reference = (w.Type == "Against Reference" || w.Type == "New Reference") ? w.NewRefName : "",
                                        payfrom = a.PayFrom,
                                        Dicount = a.Discount,
                                        a.Paying
                                    }).ToList(),
                     }).AsEnumerable().Select(o => new
                     {
                         o.VoucherNo,
                         o.Payer,
                         o.Reciever,
                         o.PaymentId,
                         o.Date,
                         modeofpay = Enum.GetName(typeof(ModeOfPayment), o.MOPayment),
                         o.MOPayment,
                         o.PDCDate,
                         o.PayFrom,
                         o.PayTo,
                         o.TaxAmount,
                         o.SubTotal,
                         o.GrandTotal,
                         o.Paying,
                         o.CreatedDate,
                         o.Remark,
                         o.Discount,
                         o.DiscountAc,
                         o.Details,
                         Tax=o.TaxAmount,
                         crdtAmt = ""
                     }).OrderBy(a => a.Date);

            var data = v.Skip(skip).Take(pageSize).ToList();
            recordsTotal = v.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;


        }
    }
}
