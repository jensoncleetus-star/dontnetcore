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
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Areas.Hr.Controllers
{
    [Microsoft.AspNetCore.Mvc.Area("Hr")]
    public class AttendanceTypeController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public AttendanceTypeController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Hr/AttendanceType
        [QkAuthorize(Roles = "Dev, AttendanceType")]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev, AttendanceType")]
        public ActionResult GetAttType()
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

            var UserView = (from a in db.AttendanceTypes
                            join b in db.payrollunits on a.Unit equals b.Id into pro
                            from b in pro.DefaultIfEmpty()
                            select new
                            {
                                id = a.Id,
                                a.Name,
                                a.Group,
                                a.PeriodType,
#pragma warning disable format
                                Unit=b.UnitName,
#pragma warning restore format
                                a.Type,
                                a.Status,
                            });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(p => p.Name.ToString().ToLower().Contains(search.ToLower()));
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
        [QkAuthorize(Roles = "Dev, Create AttendanceType")]
        public ActionResult Create()
        {
            ViewBag.AtType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "", Value=""},
                new SelectListItem() {Text = "Attendance /Leave with Pay", Value="1"},
                new SelectListItem() {Text = "Leave without Pay", Value="2"},
                new SelectListItem() {Text = "Production", Value="3"},
                new SelectListItem() {Text = "User Defined Calender Type", Value="4"},
            }, "Value", "Text");
            ViewBag.GroupType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Primary", Value="1"},
            }, "Value", "Text");
            ViewBag.unit = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = true, Text = "", Value = null},
                            }, "Value", "Text", 1);
            return PartialView();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev, Create AttendanceType")]
        public JsonResult Create(AttendanceTypeViewModel vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.AttendanceTypes.Any(c => c.Name == vmodel.Name);
                if (Exists)
                {
                    msg = "unit Name already exists.";
                    stat = false;
                }
                else
                {
#pragma warning disable format
                    var name = "";                   
#pragma warning restore format
                    var Attype = new AttendanceType
                    {
                        Name = vmodel.Name,
                        Group = (vmodel.Group !=null)? "Primary": "",
                        Type = vmodel.Type,
                        PeriodType = (vmodel.Type == "1")|| (vmodel.Type == "2") || (vmodel.Type == "4") ? vmodel.PeriodType:"",
                        Unit=(vmodel.Type=="3")?vmodel.Unit:null,
                        Status = Status.active
                    };
                    db.AttendanceTypes.Add(Attype);
                    db.SaveChanges();
                    Int64 ID = Attype.Id;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "Attendance Type", "Attendance Types", findip(), ID, "Attendance Type Added Successfully");
                    msg = "Successfully added Attendance Type details.";
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
        [QkAuthorize(Roles = "Dev, Edit AttendanceType")]
        public ActionResult Edit(long? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AttendanceType AtType = db.AttendanceTypes.Find(id);
           
            if (AtType == null)
            {
                return NotFound();
            }

            AttendanceTypeViewModel vmodel = new AttendanceTypeViewModel();
            vmodel.id = (long)id;
            vmodel.Name = AtType.Name;
            vmodel.Group = AtType.Group;
            vmodel.Type = AtType.Type;
            vmodel.PeriodType = AtType.PeriodType;
            vmodel.Unit = AtType.Unit;
            
            ViewBag.AtType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "", Value=""},
                new SelectListItem() {Text = "Attendance /Leave with Pay", Value="1"},
                new SelectListItem() {Text = "Leave without Pay", Value="2"},
                new SelectListItem() {Text = "Production", Value="3"},
                new SelectListItem() {Text = "User Defined Calender Type", Value="4"},
            }, "Value", "Text");
            ViewBag.GroupType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Primary", Value="1"},
            }, "Value", "Text");
            //ViewBag.unit = QkSelect.List(
            //                new List<SelectListItem>
            //                {
            //                        new SelectListItem { Selected = true, Text = "", Value = null},
            //                }, "Value", "Text", 1);
            var uses = db.payrollunits
                    .Select(s => new
                    {
                        ID = s.Id,
                        Name = s.UnitName
                    })
                    .ToList();
            ViewBag.uni = QkSelect.List(uses, "ID", "Name");
            return PartialView(vmodel);
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev, Edit AttendanceType")]
        public JsonResult UpdateAttType(AttendanceTypeViewModel vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {

                var Exists = db.AttendanceTypes.Any(c => c.Name == vmodel.Name && c.Id != vmodel.id);
                if (Exists)
                {
                    msg = "Type Name already exists.";
                    stat = false;
                }
                else
                {

                    AttendanceType Attype = db.AttendanceTypes.Find(vmodel.id);

                    Attype.Name = vmodel.Name;
                    Attype.Group = vmodel.Group;
                    Attype.Type = vmodel.Type;
                    Attype.PeriodType = (vmodel.Type == "1") || (vmodel.Type == "2") || (vmodel.Type == "4") ? vmodel.PeriodType : "";
                    Attype.Unit = (vmodel.Type == "3") ? vmodel.Unit : null;

                    db.Entry(Attype).State = EntityState.Modified;
                    db.SaveChanges();


                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "Attendance Type", "Attendance Types", findip(), (long)vmodel.id, "Attendance Type Updated Successfully");
                    msg = "Successfully Updated Attendance Type details.";
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

        [QkAuthorize(Roles = "Dev, Delete AttendanceType")]
        public ActionResult Delete(long? id)
        {
            AttendanceType Attype = db.AttendanceTypes.Find(id);
            if (Attype == null)
            {
                return NotFound();
            }
            return PartialView(Attype);
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev, AttendanceType")]
        public JsonResult Delete(long id, IFormCollection collection)
        {
            bool stat = false;
            string msg;
            var check = db.AttendanceTypes.Any(a => a.Id == id);
            if (!check)
            {
                msg = "Sorry, This type was not found.";
                stat = false;
            }
            else
            {
                var Msg = chkDeleteWithMsg(id);
                if (Msg != null)
                {
                    msg = Msg;
                    stat = false;
                }
                else
                {
                    stat = DeleteFn(id);
                    msg = "Successfully deleted Attendance Type.";
                }
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            if (db.AttendanceDetails.Any(c => c.AttendanceType == id))
            {
                msg = "Attendance Type Already used in Attendance !!";
            }
            if (db.DailyAttendanceDetails.Any(c => c.AtType == id))
            {
                msg = "Attendance Type Already used in Daily Attendance !!";
            }
            else
            {
                msg = null;
            }
            return msg;
        }

        public bool DeleteFn(long id)
        {
            db.AttendanceTypes.RemoveRange(db.AttendanceTypes.Where(a => a.Id == id));
            db.SaveChanges();
            return true;
        }
        public JsonResult SearchAttendanceType(string q, string x, string page)
        {
            var start = Convert.ToInt32(page);
            List<SelectFormat> serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.AttendanceTypes.Where(p => start == 0 && (p.Type == "1" || p.Type == "2") && p.Name.ToLower().Contains(q.ToLower()) || p.Name.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.Name, //each json object will have 
                                      id = b.Id
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                    serialisedJson = db.AttendanceTypes.Where(p => start == 0 && (p.Type == "1" || p.Type == "2")).Select(b => new SelectFormat
                    {
                        text = b.Name,
                        id = b.Id
                    }).OrderBy(b => b.text).ToList();

            }//
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Name" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public ActionResult AttendanceTypeById(int typeID)
        {
            var data = (from b in db.AttendanceTypes
                        where b.Id == typeID
                        select new
                        {
                            Unit = (b.Type == "3") ? db.ItemUnits.Where(a => a.ItemUnitID == b.Unit).Select(a => a.ItemUnitName).FirstOrDefault() : b.PeriodType,
                        }).FirstOrDefault();
            return Json(data);
        }
    }
}