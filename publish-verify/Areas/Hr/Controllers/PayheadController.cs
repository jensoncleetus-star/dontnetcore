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
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Areas.Hr.Controllers
{
    [Microsoft.AspNetCore.Mvc.Area("Hr")]
    public class PayheadController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PayheadController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Hr/Payhead
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, AttendanceType")]
        public ActionResult GetPayhead()
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

            var UserView = (from a in db.Payheads
                            join b in db.AccountsGroups on a.Accountgroup equals b.AccountsGroupID into pro
                            from b in pro.DefaultIfEmpty()
                            select new
                            {
                                a.ID,
                                a.Name,
                                Group = b.Name,
                                a.AttendanceType,
                                a.CalculationType,
                                a.CalculationPeriod,
                                a.affectnetsalary,
                                a.IncomeType,
                                a.Leave,
                                a.NameinSlip,
                                a.Type,
                                a.Status
                            }).AsEnumerable().Select(o => new
                            {
                                id = o.ID,
                                o.Name,
                                o.Group,
                                o.AttendanceType,
                                CalculationType = (o.CalculationType == CalcTypePayHead.DefinedValue) ? "As User Defined Value" : ((o.CalculationType == CalcTypePayHead.AsComputedValue) ? "As Computed Value" : (o.CalculationType == CalcTypePayHead.FlatRate) ? "Flat Rate" : (o.CalculationType == CalcTypePayHead.OnAttendance) ? "On Attendance" : (o.CalculationType == CalcTypePayHead.OnProduction) ? "On Production" : ""), //Enum.GetName(typeof(CalcTypePayHead), o.CalculationType),
                                o.CalculationPeriod,
                                o.affectnetsalary,
                                o.IncomeType,
                                o.Leave,
                                Payslip = o.NameinSlip,
                                Type = Enum.GetName(typeof(PayHeadType), o.Type),
                                o.Status
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
        public ActionResult Create()
        {

            ViewBag.IncType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "", Value=""},
                new SelectListItem() {Text = "Fixed", Value="Fixed"},
                new SelectListItem() {Text = "Variable", Value="Variable"},
            }, "Value", "Text");
            ViewBag.NetSalary = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Yes", Value="Yes"},
                new SelectListItem() {Text = "No", Value="No"},
            }, "Value", "Text");

            var attendancetype = db.AttendanceTypes
                    .Select(s => new
                    {
                        ID = s.Name,
                        Name = s.Name
                    })
                    .ToList();
            ViewBag.AttType = QkSelect.List(attendancetype, "ID", "Name");

            ViewBag.CalPeriod = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Days", Value="Days"},
                new SelectListItem() {Text = "Fortnights", Value="Fortnights"},
                new SelectListItem() {Text = "Months", Value="Months"},
                new SelectListItem() {Text = "Weeks", Value="Weeks"},
            }, "Value", "Text");

            ViewBag.LeaveType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Absent", Value="Absent"},
            }, "Value", "Text");
            ViewBag.Grp = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "", Value=""},
            }, "Value", "Text");
            var speformula = db.SpecifiedFormulas
                   .Select(s => new
                   {
                       ID = s.Name,
                       Name = s.Name
                   })
                   .ToList();
            ViewBag.formula = QkSelect.List(speformula, "ID", "Name");

            //var prodtyp = db.payrollunits
            //       .Select(s => new
            //       {
            //           ID = s.Id,
            //           Name = s.UnitName
            //       })
            //       .ToList();
            var prodtyp = db.AttendanceTypes.Where(x=>x.Type=="3")
                   .Select(s => new
                   {
                       ID = s.Id,
                       Name = s.Name
                   })
                   .ToList();
            ViewBag.ProdType = QkSelect.List(prodtyp, "ID", "Name");

            ViewBag.LastEntry = db.Payheads.Select(p => p.ID).AsEnumerable().DefaultIfEmpty(0).Max();
            PayheadViewModel PV = new PayheadViewModel();
            PV.OpnBalance = 0;
            return View(PV);
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, Create Payhead")]
        public JsonResult Create(PayheadViewModel vmodel)
        {
            bool stat = false;
            string msg;

            var Exists = db.Payheads.Any(c => c.Name == vmodel.Name);
            if (Exists)
            {
                msg = "Payhead Name already exists.";
                stat = false;
            }
            else
            {
                var UserId = User.Identity.GetUserId();
                decimal OpnBalance = 0;
                decimal OpnBalanceCr = 0;
                if (vmodel.DC == DC.Debit)
                {
                    OpnBalance = vmodel.OpnBalance;
                    OpnBalanceCr = 0;
                }
                if (vmodel.DC == DC.Credit)
                {
                    OpnBalance = 0;
                    OpnBalanceCr = vmodel.OpnBalance;
                }
                var Acc = new Accounts
                {
                    Name = vmodel.Name,
                    PrintName = vmodel.Name,
                    Alias = vmodel.Name,
                    PrevBalance = 0,
                    Status = Status.active,
                    Group = vmodel.Accountgroup,
                    CreatedDate = Convert.ToDateTime(System.DateTime.Now),
                    CreatedBy = UserId,
                    Editable = 0,
                    OpnBalanceCr = OpnBalanceCr,
                    OpnBalance = OpnBalance
                };
                db.Accountss.Add(Acc);
                db.SaveChanges();
                Int64 AccID = Acc.AccountsID;

                var Phead = new Payhead
                {
                    Name = vmodel.Name,
                    Accountgroup = vmodel.Accountgroup,
                    AttendanceType = vmodel.AttendanceType,
                    CalculationPeriod = vmodel.CalculationPeriod,
                    CalculationType = vmodel.CalculationType,
                    IncomeType = vmodel.IncomeType,
                    Leave = vmodel.Leave,
                    NameinSlip = vmodel.NameinSlip,
                    affectnetsalary = vmodel.affectnetsalary == "Yes" ? true : false,
                    Type = vmodel.Type,
                    days = vmodel.days,
                    CalculationBasis = vmodel.CalculationBasis,
                    Compute = vmodel.Compute,
                    Specifiedformula = vmodel.Specifiedformula,
                    Account = AccID,
                    UseGratuity=vmodel.UseGratuity == "Yes" ? true : false,
                    ProductionType= vmodel.ProductionType,
                    Status = Status.active,
                    GratuityDays=vmodel.GratuityDays,
                };
                db.Payheads.Add(Phead);
                db.SaveChanges();
                Int64 ID = Phead.ID;
                if (vmodel.Type == PayHeadType.Gratuity)
                {                    
                    foreach (var arr in vmodel.GratModel)
                    {
                        if (arr.datefrom != null)
                        {
                            var Grat = new GratuityDetails
                            {
                                Payhead = ID,
                                Days = arr.days,
                                From = arr.datefrom,
                                To = arr.dateto
                            };
                            db.GratuityDetailss.Add(Grat);
                            db.SaveChanges();
                        }
                    }
                }
                if (vmodel.compinfo != null)
                {
                    foreach (var arr in vmodel.compinfo)
                    {
                        if (arr.Slabtype != "0")
                        {
                            Computeinfo comp = new Computeinfo();
                            comp.Payhead = (long)ID;
                            comp.Effectivefrom = DateTime.Parse(arr.Effectivefrom, new CultureInfo("en-GB"));
                            comp.Amountgreatethan = arr.Amountgreatethan;
                            comp.Amountupto = arr.Amountupto;
                            comp.Slabtype = arr.Slabtype;
                            comp.value = arr.value;

                            db.Computeinfos.Add(comp);
                            db.SaveChanges();
                        }
                    }
                }
                com.addlog(LogTypes.Created, UserId, "Payhead", "Payhead", findip(), ID, "Payhead Type Added Successfully");
                msg = "Successfully added Payhead details.";
                stat = true;
            }


            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payhead Phead = db.Payheads.Find(id);

            if (Phead == null)
            {
                return NotFound();
            }
            PayheadViewModel vmodel = new PayheadViewModel();
            vmodel.ID = Phead.ID;
            vmodel.Name = Phead.Name;
            vmodel.Leave = Phead.Leave;
            vmodel.IncomeType = Phead.IncomeType;
            vmodel.NameinSlip = Phead.NameinSlip;
            vmodel.CalculationBasis = Phead.CalculationBasis;
            vmodel.CalculationPeriod = Phead.CalculationPeriod;
            vmodel.CalculationType = Phead.CalculationType;
            vmodel.days = Phead.days;
            vmodel.Type = Phead.Type;
            vmodel.AttendanceType = Phead.AttendanceType;
            vmodel.affectnetsalary = (Phead.affectnetsalary == true) ? "Yes" : "No";
            vmodel.Accountgroup = Phead.Accountgroup;
            vmodel.Specifiedformula = Phead.Specifiedformula;
            vmodel.Compute = Phead.Compute;
            vmodel.UseGratuity = Phead.UseGratuity == true ? "Yes" : "No";
            vmodel.ProductionType = Phead.ProductionType;
            vmodel.GratuityDays = Phead.GratuityDays;
              
            ViewBag.IncType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "", Value=""},
                new SelectListItem() {Text = "Fixed", Value="Fixed"},
                new SelectListItem() {Text = "Variable", Value="Variable"},
            }, "Value", "Text");
            ViewBag.NetSalary = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Yes", Value="Yes"},
                new SelectListItem() {Text = "No", Value="No"},
            }, "Value", "Text");

            var expparentid = new SqlParameter("@parentid", 13);
            var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
            var expgpid = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            var expgrp = (from a in db.AccountsGroups
                          where (a.AccountsGroupID == vmodel.Accountgroup)
                          select new
                          {
                              ID = a.AccountsGroupID,
                              Name = a.Name,
                          });
            ViewBag.Grp = QkSelect.List(expgrp, "ID", "Name");

            var attendancetype = db.AttendanceTypes
                    .Select(s => new
                    {
                        ID = s.Name,
                        Name = s.Name
                    })
                    .ToList();
            ViewBag.AttType = QkSelect.List(attendancetype, "ID", "Name");
            ViewBag.CalPeriod = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Days", Value="Days"},
                new SelectListItem() {Text = "Fortnights", Value="Fortnights"},
                new SelectListItem() {Text = "Months", Value="Months"},
                new SelectListItem() {Text = "Weeks", Value="Weeks"},
            }, "Value", "Text");

            ViewBag.LeaveType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Absent", Value="Absent"},
            }, "Value", "Text");

            var speformula = db.SpecifiedFormulas
                   .Select(s => new
                   {
                       ID = s.Name,
                       Name = s.Name
                   })
                   .ToList();
            ViewBag.formula = QkSelect.List(speformula, "ID", "Name");

            ViewBag.preEntry = db.Payheads.Where(a => a.ID < id).Select(a => a.ID).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.Payheads.Where(a => a.ID > id).Select(a => a.ID).DefaultIfEmpty().Min();

            var prodtyp = db.AttendanceTypes.Where(x => x.Type == "3")
                   .Select(s => new
                   {
                       ID = s.Id,
                       Name = s.Name
                   })
                   .ToList();
            ViewBag.ProdType = QkSelect.List(prodtyp, "ID", "Name");

            return View(vmodel);
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, Edit Payhead")]
        public JsonResult UpdatePayhead(PayheadViewModel vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var name = "";
                var Exists = db.Payheads.Any(c => c.Name == vmodel.Name && c.ID != vmodel.ID);
                if (Exists)
                {
                    msg = "Name already exists.";
                    stat = false;
                }
                else
                {
                    Payhead Phead = db.Payheads.Find(vmodel.ID);
                    Phead.Name = vmodel.Name;
                    Phead.Accountgroup = vmodel.Accountgroup;
                    Phead.AttendanceType = vmodel.AttendanceType;
                    Phead.CalculationPeriod = vmodel.CalculationPeriod;
                    Phead.CalculationType = vmodel.CalculationType;
                    Phead.IncomeType = vmodel.IncomeType;
                    Phead.Leave = vmodel.Leave;
                    Phead.NameinSlip = vmodel.NameinSlip;
                    Phead.Type = vmodel.Type;
                    Phead.days = vmodel.days;
                    Phead.CalculationBasis = vmodel.CalculationBasis;
                    Phead.affectnetsalary = vmodel.affectnetsalary == "Yes" ? true : false;
                    Phead.ProductionType = vmodel.ProductionType;
                    Phead.UseGratuity = (vmodel.UseGratuity== "Yes") ? true : false;
                    Phead.GratuityDays = vmodel.GratuityDays;
                    db.Entry(Phead).State = EntityState.Modified;
                    db.SaveChanges();
                    var Id = Phead.ID;

                    decimal OpnBalance = 0;
                    decimal OpnBalanceCr = 0;
                    if (vmodel.DC == DC.Debit)
                    {
                        OpnBalance = vmodel.OpnBalance;
                        OpnBalanceCr = 0;
                    }
                    if (vmodel.DC == DC.Credit)
                    {
                        OpnBalance = 0;
                        OpnBalanceCr = vmodel.OpnBalance;
                    }
                    Accounts Acc = db.Accountss.Find(Phead.Account);
                    Acc.Group = vmodel.Accountgroup;
                    Acc.OpnBalanceCr = OpnBalanceCr;
                    Acc.OpnBalance = OpnBalance;
                    db.Entry(Acc).State = EntityState.Modified;
                    db.SaveChanges();
                    var grat = (db.GratuityDetailss.Where(a => a.Payhead == vmodel.ID)).Any();
                    if (grat == true)
                    {
                        db.GratuityDetailss.RemoveRange(db.GratuityDetailss.Where(a => a.Payhead == vmodel.ID));
                        db.SaveChanges();
                    }
                    if (vmodel.Type == PayHeadType.Gratuity)
                    {
                        foreach (var arr in vmodel.GratModel)
                        {
                            if (arr.datefrom != null)
                            {
                                var Grat = new GratuityDetails
                                {
                                    Payhead = Id,
                                    Days = arr.days,
                                    From = arr.datefrom,
                                    To = arr.dateto
                                };
                                db.GratuityDetailss.Add(Grat);
                                db.SaveChanges();
                            }
                        }
                    }
                    var phead1 = (db.Computeinfos.Where(a => a.Payhead == vmodel.ID)).Any();
                    if (phead1 == true)
                    {
                        db.Computeinfos.RemoveRange(db.Computeinfos.Where(a => a.Payhead == vmodel.ID));
                        db.SaveChanges();
                    }
                    if (vmodel.compinfo != null)
                    {
                        foreach (var arr in vmodel.compinfo)
                        {
                            if (arr.Slabtype != "0")
                            {
                                Computeinfo comp = new Computeinfo();
                                comp.Payhead = (long)Id;
                                comp.Effectivefrom = DateTime.Parse(arr.Effectivefrom, new CultureInfo("en-GB"));
                                comp.Amountgreatethan = arr.Amountgreatethan;
                                comp.Amountupto = arr.Amountupto;
                                comp.Slabtype = arr.Slabtype;
                                comp.value = arr.value;

                                db.Computeinfos.Add(comp);
                                db.SaveChanges();
                            }
                        }
                    }

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "Payhead", "Payhead", findip(), (long)vmodel.ID, "Payhead Updated Successfully");
                    msg = "Successfully Updated Payhead details.";
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
        //[RedirectingAction]
        [Authorize(Roles = "Dev,Delete Payhead")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payhead Pay = db.Payheads.Find(id);
            if (Pay == null)
            {
                return NotFound();
            }
            return PartialView(Pay);
        }

        // POST: /Delete/5

        [RedirectingAction]
        [Authorize(Roles = "Dev,Delete Payhead")]
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
                msg = "Successfully Deleted Payhead details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            if (db.SalaryStrDetails.Any(c => c.PayHeadId == id))
            {
                msg = "PayHead Already used in Salary Structure !!";
            }
            else if (db.EmpGradeSalaryDetails.Any(c => c.Payhead == id))
            {
                msg = "PayHead Already used in Employee Grade !!";
            }
            else if (db.PayrollVoucherSalarys.Any(c => c.PayHeadId == id))
            {
                msg = "PayHead Already used in Payroll Voucher !!";
            }
            else
            {
                msg = null;
            }
            return msg;
        }
        public bool DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            Payhead pay = db.Payheads.Find(id);
            db.Payheads.Remove(pay);
            db.SaveChanges();


            var payh = db.Computeinfos.Where(a => a.Payhead == id);
            if (payh != null)
            {
                db.Computeinfos.RemoveRange(db.Computeinfos.Where(a => a.Payhead == pay.ID));
                db.SaveChanges();
            }


            com.addlog(LogTypes.Deleted, UserId, "Payhead", "Payheads", findip(), pay.ID, "Payhead Deleted Successfully");
            return true;
        }

        //Effectivefrom, Amountgreatethan, Amountupto, Slabtype, value
        [HttpGet]
        public JsonResult GetComputeinfo(long PayID)
        {
            var ConD = (from a in db.Computeinfos
                        where a.Payhead == PayID
                        select new
                        {
                            Effectivefrom = a.Effectivefrom,
                            Amountgreatethan = a.Amountgreatethan,
                            Amountupto = a.Amountupto,
                            Slabtype = a.Slabtype,
                            value = a.value
                        }).ToList();
            return Json(ConD);
        }

        public JsonResult SearchPayhead(string q, string x, string page)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            var start = Convert.ToInt32(page);
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Payheads.Where(p => p.Name.ToLower().Contains(q.ToLower()) || p.Name.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.Name, //each json object will have 
                                      id = b.ID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Payheads.Select(b => new SelectFormat
                {
                    text = b.Name, //each json object will have 
                    id = b.ID
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Payhead" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult SearchPayheadForSalary(string q, string x, string page)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            var start = Convert.ToInt32(page);
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Payheads.Where(p => p.Type != PayHeadType.EarningsInAnnualLeave && p.Type != PayHeadType.DeductionInAnnualLeave && p.Name.ToLower().Contains(q.ToLower()) || p.Name.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.Name, //each json object will have 
                                      id = b.ID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Payheads.Where(b => b.Type != PayHeadType.EarningsInAnnualLeave && b.Type != PayHeadType.DeductionInAnnualLeave).Select(b => new SelectFormat
                {
                    text = b.Name, //each json object will have 
                    id = b.ID
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Payhead" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public ActionResult GetPayheadById(int pyID)
        {
            var data = (from b in db.Payheads
                        where b.ID == pyID
                        select new
                        {
                            b.CalculationPeriod,
                            b.Type,
                            b.CalculationType,
                            b.Compute
                        }).AsEnumerable().Select(o => new
                        {
                            Per = o.CalculationPeriod,
                            HeadType = o.Type != null ? Enum.GetName(typeof(PayHeadType), o.Type) : "",
                            CalType = o.CalculationType != null ? Enum.GetName(typeof(CalcTypePayHead), o.CalculationType) : "",
                            Computed = o.Compute != null ? Enum.GetName(typeof(ComputPayHead), o.Compute) : "",
                        }).FirstOrDefault();

            return Json(data);
        }

        [QkAuthorize(Roles = "Dev, Create Payhead")]
        public ActionResult AddPayhead()
        {
            ViewBag.IncType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Fixed", Value="Fixed"},
                new SelectListItem() {Text = "Variable", Value="Variable"},
            }, "Value", "Text");
            ViewBag.NetSalary = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Yes", Value="1"},
                new SelectListItem() {Text = "No", Value="2"},
            }, "Value", "Text");

            var expparentid = new SqlParameter("@parentid", 13);
            var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
            var expgpid = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            var expgrp = (from a in db.AccountsGroups
                          where (expgpid.Contains(a.AccountsGroupID))
                          select new
                          {
                              ID = a.AccountsGroupID,
                              Name = a.Name,
                          });
            ViewBag.Grp = QkSelect.List(expgrp, "ID", "Name");


            var attendancetype = db.AttendanceTypes
                    .Select(s => new
                    {
                        ID = s.Name,
                        Name = s.Name
                    })
                    .ToList();
            ViewBag.AttType = QkSelect.List(attendancetype, "ID", "Name");

            ViewBag.CalPeriod = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Days", Value="Days"},
                new SelectListItem() {Text = "Fortnights", Value="Fortnights"},
                new SelectListItem() {Text = "Months", Value="Months"},
                new SelectListItem() {Text = "Weeks", Value="Weeks"},
            }, "Value", "Text");


            ViewBag.LeaveType = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Absent", Value="Absent"},
            }, "Value", "Text");

            var speformula = db.SpecifiedFormulas
                   .Select(s => new
                   {
                       ID = s.Name,
                       Name = s.Name
                   })
                   .ToList();
            ViewBag.formula = QkSelect.List(speformula, "ID", "Name");

            ViewBag.LastEntry = db.Payheads.Select(p => p.ID).AsEnumerable().DefaultIfEmpty(0).Max();

            return PartialView();
        }

        public JsonResult SearchPayGroup(string q, string x, string page, long? Type)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            // expense =13
            long[] par = new long[] { };
            if (Type == 0 || Type == 3 || Type == 5 || Type == 8 || Type == 9)
            {
                par = new long[] { 13 };
            }
            else if (Type == 1)
            {
                par = new long[] { 2, 3, 13, 31, 32 };
            }
            else if (Type == 2)
            {
                par = new long[] { 3 /*24*/ };
            }
            else if (Type == 6 || Type == 4 || Type == 10)
            {
                par = new long[] { 3 };
            }
            else if (Type == 7)
            {
                par = new long[] { 21 };
            }
            long[] arr = new long[] { };
            //arr.Add(GenQty);
            foreach (var arra in par)
            {
                var expparentid = new SqlParameter("@parentid", arra);
                var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
                long[] array = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();
                arr = arr.Concat(array).ToArray();
            }

            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.AccountsGroups.Where(p => (p.Name.ToLower().Contains(q.ToLower()) || p.Name.Contains(q) || p.Alias.Contains(q)) && (arr.Contains(p.Parent) || par.Contains(p.AccountsGroupID)) && p.Status == Status.active && !par.Contains(p.AccountsGroupID)
                                    && p.AccountsGroupID != 12 && p.AccountsGroupID != 14 && p.AccountsGroupID != 8
                                    && p.AccountsGroupID != 1 && p.AccountsGroupID != 2 && p.AccountsGroupID != 3 && p.AccountsGroupID != 6 && p.AccountsGroupID != 7
                                    && p.AccountsGroupID != 10 && p.AccountsGroupID != 11 && p.AccountsGroupID != 13 && p.AccountsGroupID!=9)
                              .Select(b => new SelectFormat
                              {
                                  text = b.Name, //each json object will have 
                                  id = b.AccountsGroupID
                              })
                              .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.AccountsGroups.Where(p => (arr.Contains(p.Parent) || par.Contains(p.AccountsGroupID)) && p.Status == Status.active && !(par.Contains(p.AccountsGroupID))
                                    && p.AccountsGroupID != 12 && p.AccountsGroupID != 14 && p.AccountsGroupID != 8
                                    && p.AccountsGroupID != 1 && p.AccountsGroupID != 2 && p.AccountsGroupID != 3 && p.AccountsGroupID != 6 && p.AccountsGroupID != 7
                                    && p.AccountsGroupID != 10 && p.AccountsGroupID != 11 && p.AccountsGroupID != 13 && p.AccountsGroupID != 9)
                              .Select(b => new SelectFormat
                              {
                                  text = b.Name, //each json object will have 
                                  id = b.AccountsGroupID
                              })
                              .OrderBy(b => b.text).ToList();

            }
            return Json(serialisedJson);

        }

        [HttpGet]
        public JsonResult GetGratuity(long PayID)
        {
            var ConD = (from a in db.GratuityDetailss
                        where a.Payhead == PayID
                        select new
                        {
                            a.Days,
                            a.From,
                            a.To,
                        }).ToList();
            return Json(ConD);
        }

        public JsonResult searchFinalAddType(string q, string x, string page, long[] Add)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            var exlist = Add == null?null: Add.ToArray();
            var start = Convert.ToInt32(page);
            //var existitems = PayAdd.ToArray();
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Payheads.Where(p => p.Name.ToLower().Contains(q.ToLower()) || p.Name.Contains(q) && p.Type==PayHeadType.EarningsInAnnualLeave
                                    && (Add == null || !exlist.Contains(p.ID)))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.Name, //each json object will have 
                                      id = b.ID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Payheads.Where(p=>p.Type==PayHeadType.EarningsInAnnualLeave/* && (PayAdd==null || !existitems.Contains(p.ID))*/).Select(b => new SelectFormat
                {
                    text = b.Name, //each json object will have 
                    id = b.ID
                }).OrderBy(b => b.text).ToList();

            }//
            //if (x == "all" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            //{
            //    var initial = new SelectFormat() { id = 0, text = stt };
            //    serialisedJson.Insert(0, initial);
            //}
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Payhead" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult searchFinaldedType(string q, string x, string page, long[] Ded)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            var start = Convert.ToInt32(page);
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Payheads.Where(p => p.Name.ToLower().Contains(q.ToLower()) || p.Name.Contains(q) && p.Type == PayHeadType.DeductionInAnnualLeave)
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.Name, //each json object will have 
                                      id = b.ID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Payheads.Where(p => p.Type == PayHeadType.DeductionInAnnualLeave).Select(b => new SelectFormat
                {
                    text = b.Name, //each json object will have 
                    id = b.ID
                }).OrderBy(b => b.text).ToList();

            }//
            //if (x == "all" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            //{
            //    var initial = new SelectFormat() { id = 0, text = stt };
            //    serialisedJson.Insert(0, initial);
            //}
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Payhead" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
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