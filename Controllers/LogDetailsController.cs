using QuickSoft.Web;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Http;
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
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    public class LogDetailsController : BaseController
    {
        ApplicationDbContext db;
        public LogDetailsController()
        {
            db = new ApplicationDbContext();
        }
        // GET: LogDetails
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult logactivity()
        {
            var created = db.Users.Where(o => o.Discount == null).Select(s => new
            {
                ID = s.Id,
                Name = s.UserName
            }).ToList();
            ViewBag.CreatedBy = QkSelect.List(created, "ID", "Name");
            return View();
        }
        [HttpPost]
        // [QkAuthorize(Roles = "Dev")]
        public JsonResult GetDataLogcustomer(string FromDate, string ToDate, string trantype, string user,string reference)
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            DateTime? fdate = System.DateTime.Now.AddYears(-1);
            DateTime? tdate = null;
           
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            var v = (from a in db.LogManagers
                     join b in db.Users on a.User equals b.Id into use
                     from b in use.DefaultIfEmpty()
                     where  a.LogSection == "Customer" &&
                     a.LogID ==reference &&
                    a.LogTime>=fdate 
                 //    (user == null || user == "" || a.User == user)

                     select new
                     {
                         id = a.LogManagerID,
                         name = b.UserName,
                         logtype = a.LogType,
                         section = a.LogSection,
                         ip = a.LogIP,
                         time = a.LogTime,
                         sectionid = a.LogID,
                         logdetails = a.LogDetails,
                         voucherno = "",//(c.BillNo!=null)?c.BillNo:(d.BillNo != null) ? d.BillNo :(e.BillNo != null) ?e.BillNo :(f.BillNo != null) ? f.BillNo :(g.BillNo != null) ? g.BillNo:(h.VoucherNo != null) ? h.VoucherNo : (i.VoucherNo != null) ? i.VoucherNo :(j.VoucherNo != null) ? j.VoucherNo : "",

                         // details = b.UserName + " " + Enum.GetName(typeof(LogTypes), a.LogType) +" "+ a.LogSection +" on "+a.LogTime
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p =>/* p.SENo.ToString().ToLower().Contains(search.ToLower()) ||*/
                                 p.logdetails.ToString().ToLower().Contains(search.ToLower())
                             );
            }

            //SORT
            v = v.OrderByDescending(o => o.time);
            recordsTotal = v.Count();
            var datas = v.ToList().Select(o => new {
                o.id,
                o.ip,
                o.voucherno,
                o.time,

                details = o.name + " " + o.logdetails + " on " + o.time
            });
            var data = datas.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        [HttpPost]
        // [QkAuthorize(Roles = "Dev")]
        public JsonResult GetDataLog(string FromDate, string ToDate,string trantype,string  user)
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (FromDate != "")
            {
                fdate = DateTime.Parse(FromDate, new CultureInfo("en-GB"));
            }
            if (ToDate != "")
            {
                tdate = DateTime.Parse(ToDate, new CultureInfo("en-GB"));
            }
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            var v = (from a in db.LogManagers
                     join b in db.Users on a.User equals b.Id into use
                     from b in use.DefaultIfEmpty()
                     where (trantype==""|| a.LogSection == trantype) &&
                     (FromDate == "" || FromDate == null || EF.Functions.DateDiffDay(a.LogTime, fdate) <= 0) &&
                     (ToDate == "" || FromDate == null || EF.Functions.DateDiffDay(a.LogTime, tdate) >= 0) &&
                     (user == null || user == "" ||a.User == user)

                     select new
                     {
                         id = a.LogManagerID,
                         name = b.UserName,
                         logtype = a.LogType,
                         section = a.LogSection,
                         ip=a.LogIP,
                         time = a.LogTime,
                         sectionid = a.LogID,
                         logdetails = a.LogDetails,
                         voucherno="",//(c.BillNo!=null)?c.BillNo:(d.BillNo != null) ? d.BillNo :(e.BillNo != null) ?e.BillNo :(f.BillNo != null) ? f.BillNo :(g.BillNo != null) ? g.BillNo:(h.VoucherNo != null) ? h.VoucherNo : (i.VoucherNo != null) ? i.VoucherNo :(j.VoucherNo != null) ? j.VoucherNo : "",

                         // details = b.UserName + " " + Enum.GetName(typeof(LogTypes), a.LogType) +" "+ a.LogSection +" on "+a.LogTime
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p =>/* p.SENo.ToString().ToLower().Contains(search.ToLower()) ||*/
                                 p.logdetails.ToString().ToLower().Contains(search.ToLower())
                             );
            }

            //SORT
            v = v.OrderByDescending(o => o.time);
            recordsTotal = v.Count();
            var datas = v.ToList().Select(o => new {
                o.id,
                o.ip,
                o.voucherno,
                o.time,
                
                details = o.name + " " + o.logdetails + " on " + o.time
            });
            var data = datas.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        [RedirectingAction]
        [HttpPost]
       // [QkAuthorize(Roles = "Dev")]
        public JsonResult GetData(string TransId, string TransType)
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

            var v = (from a in db.LogManagers
                     join b in db.Users on a.User equals b.Id
                     into user from b in user.DefaultIfEmpty()
                     where a.LogSection == TransType && a.LogID == TransId
                     select new
                     {
                         id         =   a.LogManagerID,
                         name       =   b.UserName,
                         logtype    =   a.LogType,
                         section    =   a.LogSection,
                         time       =   a.LogTime,
                         sectionid  =   a.LogID,
                         logdetails =  a.LogDetails,

                        // details = b.UserName + " " + Enum.GetName(typeof(LogTypes), a.LogType) +" "+ a.LogSection +" on "+a.LogTime
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p =>/* p.SENo.ToString().ToLower().Contains(search.ToLower()) ||*/
                                 p.name.ToString().ToLower().Contains(search.ToLower())
                             );
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = v.Count();
            var datas = v.ToList().Select(o=> new {
                o.id,
                details = o.name + " " + o.logdetails + " on " + o.time
            });
            var data = datas.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
    }
}
