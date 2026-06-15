using QuickSoft.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using QuickSoft.Controllers;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace QuickSoft.Areas.Hr.Controllers
{
    [RedirectingAction]
    [Microsoft.AspNetCore.Mvc.Area("Hr")]
    public class CalendarTemplateController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public CalendarTemplateController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Hr/CalendarTemplate
        [Authorize(Roles = "Dev,CalendarTemplate List")]
        public ActionResult Index()
        {
            return View();
        }

        [RedirectingAction]
        [Authorize(Roles = "Dev,CalendarTemplate List")]
        [HttpPost]
        public ActionResult GetCalendarTemplate()
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

            var UserView = (from a in db.CalendarTemplates
                            join c in db.Users on a.CreatedBy equals c.Id
                            select new
                            {
                                a.CalendarTemplateID,
                                a.TemplateName,
                                a.DefaultValue,
                                c.UserName,
                                WeeklyHoliday = (from z in db.WeeklyHolidays
                                                 where z.TemplateID == a.CalendarTemplateID
                                                 select new
                                                 {
                                                     Days = z.SelDay,
                                                 }).ToList(),

                            }).ToList().Select(o => new
                            {
                                id = o.CalendarTemplateID,
                                o.TemplateName,
                                o.DefaultValue,
                                o.UserName,
                                o.WeeklyHoliday,
                            });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(p => p.TemplateName.ToString().ToLower().Contains(search.ToLower()));
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

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Create CalendarTemplate")]
        public ActionResult Create(long? id)
        {
            return PartialView();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create CalendarTemplate")]
        //[ValidateAntiForgeryToken]
        public JsonResult Create(CalendarTemplateViewModel vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.CalendarTemplates.Any(c => c.TemplateName == vmodel.TemplateName);
                if (Exists)
                {
                    msg = "Calendar Templates with The Same Name already exists.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    var today = Convert.ToDateTime(System.DateTime.Now);

                    var ctemp = new CalendarTemplate
                    {
                        TemplateName = vmodel.TemplateName,
                        DefaultValue = vmodel.DefaultValue,
                        CreatedDate = today,
                        CreatedBy = UserId,
                        Status = Status.active,
                        Branch = BranchID
                    };
                    db.CalendarTemplates.Add(ctemp);
                    db.SaveChanges();
                    Int64 ctempId = ctemp.CalendarTemplateID;

                    if (ctemp.DefaultValue == true)
                    {
                        db.CalendarTemplates.Where(a => a.CalendarTemplateID != ctempId).ToList().ForEach(c => c.DefaultValue = false);
                    }
                    //days
                    if (vmodel.WeeklyHoliday != null)
                    {
                        WeeklyHoliday weekholiday = new WeeklyHoliday();
                        foreach (var arr in vmodel.WeeklyHoliday)
                        {
                            weekholiday.SelDay = arr;
                            weekholiday.TemplateID = ctempId;
                            db.WeeklyHolidays.Add(weekholiday);
                            db.SaveChanges();
                        }
                    }
                    com.addlog(LogTypes.Created, UserId, "CalendarTemplate", "CalendarTemplates", findip(), ctempId, "Calendar Templates Added Successfully");
                    msg = "Successfully added Calendar Templates details.";
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

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit CalendarTemplate")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CalendarTemplate ctype = db.CalendarTemplates.Find(id);

            if (ctype == null)
            {
                return NotFound();
            }
            var holiday = db.WeeklyHolidays.Where(a => a.TemplateID == id).Select(a => a.SelDay).ToList().ToArray();
            ViewBag.WHoliday = new MultiSelectList(new List<SelectListItem>{
                new SelectListItem() {Text = "Sunday", Value="Sunday"},
                new SelectListItem() {Text = "Monday", Value="Monday"},
                new SelectListItem() {Text = "Tuesday", Value="Tuesday"},
                new SelectListItem() {Text = "Wednesday", Value="Wednesday"},
                new SelectListItem() {Text = "Thursday", Value="Thursday"},
                new SelectListItem() {Text = "Friday", Value="Friday"},
                new SelectListItem() {Text = "Saturday", Value="Saturday"},
            }, "Value", "Text", holiday);

            CalendarTemplateViewModel vmodel = new CalendarTemplateViewModel();
            vmodel.CalendarTemplateID = ctype.CalendarTemplateID;
            vmodel.TemplateName = ctype.TemplateName;
            vmodel.DefaultValue = ctype.DefaultValue;

            return PartialView(vmodel);
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit CalendarTemplate")]
        [HttpPost]
        public JsonResult Edit(long? id, CalendarTemplateViewModel vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.CalendarTemplates.Any(c => c.TemplateName == vmodel.TemplateName && c.CalendarTemplateID != id);
                if (Exists)
                {
                    msg = "Calendar Template with The Same Name already exists.";
                    stat = false;
                }
                else
                {

                    var UserId = User.Identity.GetUserId();
                    CalendarTemplate ctemp = db.CalendarTemplates.Find(id);

                    ctemp.TemplateName = vmodel.TemplateName;
                    ctemp.DefaultValue = vmodel.DefaultValue;

                    db.Entry(ctemp).State = EntityState.Modified;
                    db.SaveChanges();
                    Int64 ctempId = ctemp.CalendarTemplateID;

                    if (ctemp.DefaultValue == true)
                    {
                        db.CalendarTemplates.Where(a => a.CalendarTemplateID != ctempId).ToList().ForEach(c => c.DefaultValue = false);
                    }

                    var weekholi = db.WeeklyHolidays.Where(a => a.TemplateID == ctempId);
                    if (weekholi != null)
                    {
                        db.WeeklyHolidays.RemoveRange(db.WeeklyHolidays.Where(a => a.TemplateID == ctempId));
                        db.SaveChanges();
                    }
                    //days
                    if (vmodel.WeeklyHoliday != null)
                    {
                        WeeklyHoliday weekholiday = new WeeklyHoliday();
                        foreach (var arr in vmodel.WeeklyHoliday)
                        {
                            weekholiday.SelDay = arr;
                            weekholiday.TemplateID = ctempId;
                            db.WeeklyHolidays.Add(weekholiday);
                            db.SaveChanges();
                        }
                    }

                    com.addlog(LogTypes.Updated, UserId, "CalendarTemplate", "CalendarTemplates", findip(), ctempId, "Calendar Template Updated Successfully");
                    msg = "Successfully Updated Calendar Template details.";
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

        // GET: /Delete/5
        [RedirectingAction]
        [Authorize(Roles = "Dev,Delete CalendarTemplate")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CalendarTemplate ctype = db.CalendarTemplates.Find(id);
            if (ctype == null)
            {
                return NotFound();
            }
            return PartialView(ctype);
        }

        // POST: /Delete/5

        [RedirectingAction]
        [Authorize(Roles = "Dev,Delete CalendarTemplate")]
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
                msg = "Successfully Deleted Calendar Template details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public bool DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            CalendarTemplate ctemp = db.CalendarTemplates.Find(id);

            var days = db.WeeklyHolidays.Where(a => a.TemplateID == id).ToList();
            if (days.Any())
            {
                db.WeeklyHolidays.RemoveRange(db.WeeklyHolidays.Where(a => a.TemplateID == id));
                db.SaveChanges();
            }
            db.CalendarTemplates.Remove(ctemp);

            com.addlog(LogTypes.Deleted, UserId, "CalendarTemplate", "CalendarTemplates", findip(), id, "Calendar Template Deleted Successfully");
            return true;
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            //if (db..Any(c => c.TeamId == id))
            //{
            //    msg = "Team Already used in  !!";
            //}
            //else
            //{
            //    msg = null;
            //}
            return msg;
        }

        [HttpGet]
        public JsonResult GetWeeklyHolidayById(long CaID)
        {
            var days = (from a in db.WeeklyHolidays
                        where a.TemplateID == CaID
                        select new
                        {
                            Day = a.SelDay,
                        }).ToList();
            return Json(days);
        }

        public JsonResult SearchCalendarTemplate(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.CalendarTemplates.Where(p => p.TemplateName.ToLower().Contains(q.ToLower()) || p.TemplateName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.TemplateName, //each json object will have 
                                      id = b.CalendarTemplateID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.CalendarTemplates
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.TemplateName, //each json object will have 
                                      id = b.CalendarTemplateID
                                  }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Calendar Template" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }


        [RedirectingAction]
        [HttpGet]
        [QkAuthorize(Roles = "Dev,CalendarTemplate List")]
        public ActionResult Details(long? id)
        {
            return View();
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,CalendarTemplate List")]
        public JsonResult GetMonthlyLeave(DateTime start, DateTime end,long EntryID)
        {

            var dd = start.AddMonths(1).Month;
            var wh = db.WeeklyHolidays.Where(x => x.TemplateID == EntryID).Select(x => x.SelDay).ToList();
            var holiday = db.Holidays.Where(a => (a.FromDate.Month == dd || a.ToDate.Month == dd) && a.CalendarTemplateID == EntryID).ToList();


            var holidays = com.GetDates(start.AddMonths(1), wh, holiday).ToList();


            var viewModel = new EventViewModel();
            var events = new List<EventViewModel>();
            var count = 1;
            foreach (var dt in holidays)
            {
                events.Add(new EventViewModel()
                {
                    id = count,
                    title = dt.HName.ToString(),
                    start = dt.Date.ToString(),
                    allDay = false
                });
                count++;
            }


            return Json(events.ToArray());
        }

    }
}