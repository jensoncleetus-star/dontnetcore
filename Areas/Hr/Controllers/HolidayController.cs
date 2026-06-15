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
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Areas.Hr.Controllers
{
    [Microsoft.AspNetCore.Mvc.Area("Hr")]
    public class HolidayController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public HolidayController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Hr/PayrollUnits
        [QkAuthorize(Roles = "Dev,HolidayList")]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,HolidayList")]
        public ActionResult GetHolidayList()
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

            var UserView = (from a in db.Holidays
                            join c in db.CalendarTemplates on a.CalendarTemplateID equals c.CalendarTemplateID into ctemp
                            from c in ctemp.DefaultIfEmpty()
                            select new
                            {
                                id = a.HolidayID,
                                c.TemplateName,
                                a.HolidayName,
                                a.FromDate,
                                a.ToDate,
                            });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(p => p.HolidayName.ToString().ToLower().Contains(search.ToLower()));
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
        [QkAuthorize(Roles = "Dev,Create Holiday")]
        public ActionResult Create(long? Id)
        {
            var CTemp = db.CalendarTemplates
                            .Select(s => new
                            {
                                ID = s.CalendarTemplateID,
                                Name = s.TemplateName,
                            })
                            .ToList();
            ViewBag.CTemp = QkSelect.List(CTemp, "ID", "Name");

            return PartialView();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create Holiday")]
        public JsonResult Create(HolidayViewModel vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                DateTime FDate = DateTime.Parse(vmodel.FromDate, new CultureInfo("en-GB"));
                DateTime TDate = DateTime.Parse(vmodel.ToDate, new CultureInfo("en-GB"));
                var Exists1 = db.Holidays.Any(c => c.HolidayName == vmodel.HolidayName);
                var Exists2 = db.Holidays.Any(c => c.FromDate == FDate && c.ToDate == TDate);
                if (Exists1 || Exists2)
                {
                    msg = "Holiday already exists.";
                    stat = false;
                }
                else
                {
                    var hday = new Holiday
                    {
                       HolidayName=vmodel.HolidayName,
                       CalendarTemplateID=vmodel.CalendarTemplateID,
                       FromDate= DateTime.Parse(vmodel.FromDate.ToString(), new CultureInfo("en-GB")),
                       ToDate= DateTime.Parse(vmodel.ToDate.ToString(), new CultureInfo("en-GB")),
                    };
                    db.Holidays.Add(hday);
                    db.SaveChanges();
                    Int64 ID = hday.HolidayID;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "Holiday", "Holidays", findip(), ID, "Holiday Added Successfully");
                    msg = "Successfully added Holiday details.";
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
        [QkAuthorize(Roles = "Dev,Edit Holiday")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Holiday hday = db.Holidays.Find(id);

            if (hday == null)
            {
                return NotFound();
            }
            var CTemp = db.CalendarTemplates
                            .Select(s => new
                            {
                                ID = s.CalendarTemplateID,
                                Name = s.TemplateName,
                            })
                            .ToList();
            ViewBag.CTemp = QkSelect.List(CTemp, "ID", "Name");

            HolidayViewModel vmodel = new HolidayViewModel();
            vmodel.HolidayID = hday.HolidayID;
            vmodel.HolidayName = hday.HolidayName;
            vmodel.CalendarTemplateID = hday.CalendarTemplateID;
            vmodel.FromDate = hday.FromDate.ToString("dd-MM-yyyy");
            vmodel.ToDate = hday.ToDate.ToString("dd-MM-yyyy");

            return PartialView(vmodel);
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Holiday")]
        public JsonResult Edit(HolidayViewModel vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                DateTime FDate = DateTime.Parse(vmodel.FromDate, new CultureInfo("en-GB"));
                DateTime TDate = DateTime.Parse(vmodel.ToDate, new CultureInfo("en-GB"));
                var Exists1 = db.Holidays.Any(c => c.HolidayName == vmodel.HolidayName && c.HolidayID != vmodel.HolidayID);
                var Exists2 = db.Holidays.Any(c => c.FromDate == FDate && c.ToDate == TDate && c.HolidayID != vmodel.HolidayID);
                if (Exists1 || Exists2)
                {
                    msg = "Holiday already exists.";
                    stat = false;
                }
                else
                {
                    Holiday hday = db.Holidays.Find(vmodel.HolidayID);

                    hday.HolidayName = vmodel.HolidayName;
                    hday.CalendarTemplateID = vmodel.CalendarTemplateID;
                    hday.FromDate = DateTime.Parse(vmodel.FromDate.ToString(), new CultureInfo("en-GB"));
                    hday.ToDate = DateTime.Parse(vmodel.ToDate.ToString(), new CultureInfo("en-GB"));
                    db.Entry(hday).State = EntityState.Modified;
                    db.SaveChanges();

                    Int64 ID = hday.HolidayID;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "Holiday", "Holidays", findip(), ID, "Holiday Updated Successfully");
                    msg = "Successfully Updated Holiday details.";
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

        [QkAuthorize(Roles = "Dev,Delete Holiday")]
        public ActionResult Delete(long? id)
        {
            Holiday hday = db.Holidays.Find(id);
            if (hday == null)
            {
                return NotFound();
            }
            return PartialView(hday);
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Holiday")]
        public JsonResult Delete(long id)
        {
            bool stat = false;
            string msg = "";
            var Msg = chkDeleteWithMsg(id);
            if (Msg != null)
            {
                msg = Msg;
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Successfully deleted Holiday.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            //if (db..Any(c => c.AttendanceType == id))
            //{
            //    msg = ""
            //}
            //else
            //{
            //    msg = null;
            //}
            return msg;
        }

        public bool DeleteFn(long id)
        {
            db.Holidays.RemoveRange(db.Holidays.Where(a => a.HolidayID == id));
            db.SaveChanges();
            return true;
        }

    }
}