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
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    public class AdditionalMCController : Controller
    {
        ApplicationDbContext db;
        Common com;
        // GET: AdditionalMC
        public AdditionalMCController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        //Get Create
        public ActionResult Create()
        {
            var ViewModel = new AdditionalMCViewModel
            {
            };

            //DropDown Material Centre
            var Mcs = db.MCs.Select(s => new
            {
                McId = s.MCId,
                Name = s.MCName
            }).ToList();
            ViewBag.MCbag = QkSelect.List(Mcs, "McId", "Name");

            //DropDown Employee
            ViewBag.Employee = QkSelect.List(
                                         new List<SelectListItem>
                                         {
                                            new SelectListItem { Selected = false, Text = "All", Value = "0"},
                                         }, "Value", "Text", 1);

            return PartialView(ViewModel);
        }

        //Post Create(Saving)
        [HttpPost]
        public JsonResult Create(AdditionalMCViewModel ViewModel)
        {
            bool stat = false;
            string msg = "";
            Int64 EmployeeId = 0;

            if (ModelState.IsValid)
            {
                if (ViewModel.Employees != null)
                {
                    foreach (var arr in ViewModel.Employees)
                    {
                        EmployeeId = Convert.ToInt64(arr);

                        //Getting the Material Centre Name
                        var McName = db.MCs.Where(o => o.MCId == ViewModel.McId).Select(o => o.MCName).FirstOrDefault();

                        //Getting the UserId of Employee
                        var UserId = db.Employees.Where(o => o.EmployeeId == EmployeeId).Select(o => o.UserId).FirstOrDefault();

                        var Mc = db.AdditionalMc.Where(a => a.UserId == UserId && a.McName == McName).FirstOrDefault();

                        if (Mc != null)
                        {
                            msg     =   "Employee already assigned to the same Material Centre..";
                            stat    =   false;
                        }
                        else
                        {

                            AdditionalMc mc = new AdditionalMc
                            {
                                UserId      =   UserId,
                                McId        =   ViewModel.McId,
                                McName      =   McName
                            };

                            db.AdditionalMc.Add(mc);
                            db.SaveChanges();

                            msg     =   "Successfully added Additional MC details.";
                            stat    =   true;
                        }

                    }
                }
                else
                {
                    msg     =   "Please select Employee....";
                    stat    =   false;
                }
            }
            else
            {
                msg     =   "Looks like something went wrong. Please check your form.";
                stat    =   false;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        //Get Index
        public ActionResult Index()
        {
            return View();
        }

        //Function to retrieve data in Index page
        public JsonResult GetAddMc()
        {
            string search       =   Request.Form.GetValues("search[value]")[0];
            var draw            =   Request.Form.GetValues("draw").FirstOrDefault();
            var start           =   Request.Form.GetValues("start").FirstOrDefault();
            var length          =   Request.Form.GetValues("length").FirstOrDefault();
                       
            int pageSize        =   length != null ? Convert.ToInt32(length) : 0;
            int skip            =   start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal    =   0;

            var UserView = (from a in db.AdditionalMc
                            join b in db.Employees on a.UserId equals b.UserId into emp
                            from b in emp.DefaultIfEmpty()
                            select new
                            {
                                a.McId,

                                Employees = (from z in db.AdditionalMc
                                             join y in db.Employees on z.UserId equals y.UserId
                                             where z.McId == a.McId
                                             select new
                                             {
                                                 id         =   y.EmployeeId,
                                                 LastName   =   (y.LastName != null) ? y.LastName : "",
                                                 FirstName  =   (y.FirstName != null) ? y.FirstName : ""
                                             }).ToList(),
                                a.McName
                            }).ToList().Select(o => new {
                                o.McId,
                                o.Employees,
                                o.McName
                            }).GroupBy(x => x.McName, (key, g) => g.OrderBy(m => m.McName).FirstOrDefault());

            //SEARCH
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(p => p.McName.ToString().ToLower().Contains(search.ToLower()));
            }
           
            recordsTotal    =   UserView.Count();
            var data        =   UserView.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        // GET: AdditionalMC/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var AdditionalMc = db.AdditionalMc.Where(a => a.McId == id).FirstOrDefault();

            if (AdditionalMc == null)
            {
                return NotFound();
            }                       

            AdditionalMCViewModel ViewModel = new AdditionalMCViewModel();

            //****** MC
            ViewModel.McId      =   AdditionalMc.McId;
            ViewModel.McName    =   AdditionalMc.McName;

            var Mcs = db.MCs.Select(s => new { ID = s.MCId, Name = s.MCName }).ToList();
            ViewBag.Mc = QkSelect.List(Mcs, "ID", "Name");

            //****** Employees            
            var Employees = (from a in db.AdditionalMc
                            join c in db.Employees
                            on a.UserId equals c.UserId
                            where a.McId == id
                            select c.EmployeeId
                            ).ToList();

            ViewModel.EmployeeMembers = Employees.ToArray();  

            var use = db.Employees.Select(s => new { ID = s.EmployeeId, Name = s.FirstName + " " + s.LastName }).ToList();
            
            ViewBag.Employees = new MultiSelectList(use, "ID", "Name", ViewModel.EmployeeMembers);

            return PartialView(ViewModel);
        }

        // POST: Additional MC/Edit/5        
        [RedirectingAction]       
        [HttpPost]
        public JsonResult Edit(long? id, AdditionalMCViewModel ViewModel)
        {
            bool stat = false;
            string msg;
            long EmployeeId = 0, McId =0;

            if (ModelState.IsValid)
            {
                McId = Convert.ToInt64(id);

                //Delete All Employees for corresponding Material Centre
                var Employees = db.AdditionalMc.Where(a => a.McId == McId);
                if (Employees != null)
                {
                    db.AdditionalMc.RemoveRange(db.AdditionalMc.Where(a => a.McId == McId));
                    db.SaveChanges();
                }

                if (ViewModel.EmployeeMembers != null)
                {
                    foreach (var arr in ViewModel.EmployeeMembers)
                    {
                        EmployeeId = Convert.ToInt64(arr);

                        //Getting the Material Centre Name
                        var McName = db.MCs.Where(o => o.MCId == McId).Select(o => o.MCName).FirstOrDefault();

                        //Getting the UserId of Employee
                        var UserId = db.Employees.Where(o => o.EmployeeId == EmployeeId).Select(o => o.UserId).FirstOrDefault();
                                                
                        AdditionalMc mc = new AdditionalMc
                        {
                            UserId      =   UserId,
                            McId        =   McId,
                            McName      =   McName
                        };

                        db.AdditionalMc.Add(mc);
                        db.SaveChanges();                       
                    }

                    msg     =   "Successfully Updated Material Centre details...";
                    stat    =   true;
                }
                else
                {
                    msg     =   "Please select Employee...";
                    stat    =   false;
                }
            }
            else
            {
                msg     =   "Looks like something went wrong. Please check your form.";
                stat    =   false;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        //Delete GET
        [HttpGet]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var Obj = db.AdditionalMc.Where(a => a.McId == id).FirstOrDefault();

            if (Obj == null)
            {
                return NotFound();
            }
            return PartialView(Obj);
        }

        //POST Delete
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteAction(long id)
        {
            bool stat;
            string msg;
            long McId = id;

            //***********Delete from table AdditionalMc

            var Obj = db.AdditionalMc.Where(a => a.McId == McId).FirstOrDefault();

            if (Obj != null)
            {
                db.AdditionalMc.RemoveRange(db.AdditionalMc.Where(a => a.McId == McId));
                db.SaveChanges();
            }

            stat = true;
            msg = "Successfully deleted Additinal MC details.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        //DropDown Employee
        public JsonResult SearchEmployee(string q, string x, string page)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            var start = Convert.ToInt32(page);

            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Employees
                                  join b in db.Users on a.UserId equals b.Id
                                  where b.Status == 1
                                  select a).Where(p =>  (p.FirstName.ToLower().Contains(q.ToLower()) || p.MiddleName.ToLower().Contains(q.ToLower()) || p.LastName.ToLower().Contains(q.ToLower()) || p.FirstName.Contains(q) || p.MiddleName.Contains(q) || p.LastName.Contains(q)))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.FirstName + " " + b.LastName, //each json object will have 
                                      id = b.EmployeeId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.Employees
                                  join b in db.Users on a.UserId equals b.Id
                                  where b.Status == 1
                                  select a).Select(b => new SelectFormat
                {
                    text = b.FirstName + " " + b.LastName, //each json object will have 
                    id = b.EmployeeId
                }).OrderBy(b => b.text).ToList();
            }
            if (x == "all" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Employee" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
    }
}
