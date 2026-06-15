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
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Areas.Hr.Controllers
{
    [Microsoft.AspNetCore.Mvc.Area("Hr")]
    public class PayrollUnitController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PayrollUnitController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Hr/PayrollUnits
        [QkAuthorize(Roles = "Dev, PayrollUnits")]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev, PayrollUnits")]
        public ActionResult GetPayrollUnit()
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

            var UserView = (from a in db.payrollunits
                            select new
                            {
                                id = a.Id,
                                a.UnitName,
                                a.Symbol,
                                a.Type,
                            });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(p => p.UnitName.ToString().ToLower().Contains(search.ToLower()));
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
        [QkAuthorize(Roles = "Dev, Create PayrollUnits")]
        public ActionResult Create(long? Id)
        {
            //var PR = db.payrollunits
            //                .Select(s => new
            //                {
            //                    ID = s.Id,
            //                    Name = s.UnitName,
            //                })
            //                .ToList();
            //ViewBag.firstunit = QkSelect.List(PR, "ID", "Name");
            ViewBag.secondunit = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = true, Text = "", Value = null},
                             }, "Value", "Text", 1);
            ViewBag.firstunit = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = true, Text = "", Value = null},
                             }, "Value", "Text", 1);
            return PartialView();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev, Create PayrollUnits")]
        public JsonResult Create(PayrollUnitViewModel vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.payrollunits.Any(c => c.UnitName == vmodel.UnitName);
                if (Exists)
                {
                    msg = "Unit Name already exists.";
                    stat = false;
                }
                else
                {
                    var name = "";
                    if (vmodel.first != null)
                    {
                        var unit1 = (vmodel.first);
                        var unit2 = (vmodel.second);
                        var firstname = db.payrollunits.Where(x => x.Id == unit1).Select(z => z.Symbol).FirstOrDefault();
                        var secondname = db.payrollunits.Where(x => x.Id == unit2).Select(z => z.Symbol).FirstOrDefault();
                        name = firstname + " of " + vmodel.convertion + " " + secondname;
                    }
                    var payroll = new PayrollUnit
                    {
                        UnitName = (vmodel.first != null) ? name : vmodel.UnitName,
                        Symbol = (vmodel.first != null) ? name : vmodel.Symbol,
                        Type = vmodel.Type,
                        Convertion = vmodel.convertion
                    };
                    db.payrollunits.Add(payroll);
                    db.SaveChanges();
                    Int64 ID = payroll.Id;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "PayrollUnit", "PayrollUnits", findip(), ID, "Payroll Unit Added Successfully");
                    msg = "Successfully added Payroll Unit details.";
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
        [QkAuthorize(Roles = "Dev, Edit PayrollUnits")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PayrollUnit payr = db.payrollunits.Find(id);

            if (payr == null)
            {
                return NotFound();
            }
            var first = "";
            var second = "";
            PayrollUnitViewModel vmodel = new PayrollUnitViewModel();
            if (payr.Type == "Compound")
            {
                first = Regex.Replace(payr.UnitName.Split()[0], @"[^0-9a-zA-Z\ ]+", "");
                second = Regex.Replace(payr.UnitName.Split()[3], @"[^0-9a-zA-Z\ ]+", "");
                var firstunit = db.payrollunits.Where(x => x.Symbol == first).Select(y => y.Id).FirstOrDefault();
                var secondunit = db.payrollunits.Where(x => x.Symbol == second).Select(y => y.Id).FirstOrDefault();
                vmodel.first = firstunit;
                vmodel.second = secondunit;
                vmodel.convertion = payr.Convertion;

            }
            else
            {
                vmodel.Symbol = payr.Symbol;
                vmodel.UnitName = payr.UnitName;
            }
            vmodel.Id = (long)id;
            vmodel.Type = payr.Type;

            var use = db.payrollunits.Where(x=>x.Id==vmodel.first)
                    .Select(s => new
                    {
                        ID = s.Id,
                        Name = s.UnitName
                    })
                    .ToList();
            ViewBag.firs = QkSelect.List(use, "ID", "Name");
            var uses = db.payrollunits.Where(x => x.Id == vmodel.second)
                    .Select(s => new
                    {
                        ID = s.Id,
                        Name = s.UnitName
                    })
                    .ToList();
            ViewBag.secnd = QkSelect.List(uses, "ID", "Name");
            //vmodel.first=
            ViewBag.secondunit = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = true, Text = "", Value = null},
                             }, "Value", "Text", 1);
            ViewBag.firstunit = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = true, Text = "", Value = null},
                             }, "Value", "Text", 1);
            return PartialView(vmodel);
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev, Edit PayrollUnits")]
        public JsonResult UpdatePayunit(PayrollUnitViewModel vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.payrollunits.Any(c => c.UnitName == vmodel.UnitName && c.Id != vmodel.Id);
                if (Exists)
                {
                    msg = "Unit Name already exists.";
                    stat = false;
                }
                else
                {
                    var name = "";
                    if (vmodel.first != null)
                    {
                        var unit1 = Convert.ToInt64(vmodel.first);
                        var unit2 = Convert.ToInt64(vmodel.second);
                        var firstname = db.payrollunits.Where(x => x.Id == unit1).Select(z => z.Symbol).FirstOrDefault();
                        var secondname = db.payrollunits.Where(x => x.Id == unit2).Select(z => z.Symbol).FirstOrDefault();
                        name = firstname + " of " + vmodel.convertion + " " + secondname;
                    }

                    PayrollUnit payroll = db.payrollunits.Find(vmodel.Id);

                    payroll.UnitName = (vmodel.first != null) ? name : vmodel.UnitName;
                    payroll.Symbol = (vmodel.first != null) ? name : vmodel.Symbol;
                    payroll.Type = vmodel.Type;
                    payroll.Convertion = vmodel.convertion;
                    db.Entry(payroll).State = EntityState.Modified;
                    db.SaveChanges();


                    Int64 ID = payroll.Id;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "PayrollUnit", "PayrollUnits", findip(), ID, "Payroll Unit Updated Successfully");
                    msg = "Successfully Updated Payroll Unit details.";
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

        [QkAuthorize(Roles = "Dev, Delete PayrollUnits")]
        public ActionResult Delete(long? id)
        {
            PayrollUnit Attype = db.payrollunits.Find(id);
            if (Attype == null)
            {
                return NotFound();
            }
            return PartialView(Attype);
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev, Delete PayrollUnits")]
        public JsonResult Delete(long id)
        {
            bool stat = false;
            string msg="";
            var check = db.payrollunits.Any(a => a.Id == id);
            if (!check)
            {
                msg = "Sorry, This Unit was not found.";
                stat = false;
            }
            else
            {
                var payr= db.payrollunits.Where(x=>x.Id==id).FirstOrDefault();
                var complist = db.payrollunits.Where(x => x.Type == "Compound").ToList();
                var count = 0;
                var attunit = db.AttendanceTypes.Where(x => x.Id == id).Any();
                if (!attunit || payr.Type== "Simple")
                {
                    foreach (var arr in complist)
                    {
                        var first = Regex.Replace(arr.UnitName.Split()[0], @"[^0-9a-zA-Z\ ]+", "");
                        var second = Regex.Replace(arr.UnitName.Split()[3], @"[^0-9a-zA-Z\ ]+", "");
                        var firstunit = db.payrollunits.Where(x => x.Symbol == first).FirstOrDefault();// (y => y.Id).FirstOrDefault();
                        var secondunit = db.payrollunits.Where(x => x.Symbol == second).FirstOrDefault();//Select(y => y.Id).FirstOrDefault();
                        if (payr.UnitName == firstunit.UnitName || payr.UnitName == secondunit.UnitName)
                        {
                            msg = "Sorry, This Unit is used in Compound Unit.";
                            stat = false;
                            count++;
                        }
                    }
                }      
                if(!attunit || payr.Type == "Compound" || (count == 0))
                {
                    db.payrollunits.RemoveRange(db.payrollunits.Where(a => a.Id == id));
                    db.SaveChanges();
                    msg = "Unit was deleted successfully.";
                    stat = true;
                }                
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public JsonResult SearchSecondUnit(string q, string x, long? first = 0)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.payrollunits.Where(p => p.UnitName.ToLower().Contains(q.ToLower()) && p.Id != first)
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.UnitName, //each json object will have 
                                      id = b.Id
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.payrollunits.Where(z => z.Id != first).Select(b => new SelectFormat
                {
                    text = b.UnitName, //each json object will have 
                    id = b.Id
                }).OrderBy(b => b.text).ToList();

            }
            return Json(serialisedJson);
        }

    }
}