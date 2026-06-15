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
using System.Collections.Generic;
using System.Linq.Dynamic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QuickSoft.ViewModel;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.IO;
using System.Net;

namespace QuickSoft.Controllers
{
    public class PasswordController: BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PasswordController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }  // GET: Password
        
             public ActionResult MyPassword()
        {
            var mcs = db.Customers.Where(o => o.CustomerName.StartsWith("OLD-")).Take(1).Select(s => new
            {
                Id = s.CustomerID,
                Name = s.CustomerName
            }).ToList();
            ViewBag.pwdgroup = QkSelect.List(mcs, "Id", "Name");

            return View();
        }
        

  
      //  [RedirectingAction]
     //   [Authorize(Roles = "Dev,List")]
        public ActionResult Index()
        {

            ViewBag.Employee = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                          }, "Value", "Text", 1);
            var mcs = db.Customers.Where(o => o.CustomerName.StartsWith("OLD-")).Take(1).Select(s => new
            {
                Id = s.CustomerID,
                Name = s.CustomerName
            }).ToList();
            ViewBag.pwdgroup = QkSelect.List(mcs, "Id", "Name");

            return View();
        }
        public JsonResult GetData( long? AccGroup,long empid)
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
            Status st = new Status();

            // The Assigned To dropdown's "All" option sets val(0) (see Index.cshtml), so an unset filter arrives
            // as 0, not null. Treat 0 as "no assignee filter" (employee id 0 never exists) so the default list
            // isn't empty. empid is a non-nullable long, so use a nullable local for the (now client-side) filter.
            long? empfilter = (empid == 0) ? (long?)null : empid;

            // SERVER: joins + WHERE, projecting ONLY entity columns / simple scalars. The AssignedTo nested
            // collection (a let-subquery .ToList() inside the select) can't be translated by EF Core 10
            // ("Unable to translate a collection subquery in a projection ..."), so it is computed client-side
            // below. customers = d.CustomerName is left-joined entity access kept server-side so it stays null-safe.
            var serverRows = (from a in db.passworddetails
                           join b in db.LeadTypes on a.LeadType equals b.TypeId into ls
                           from b in ls.DefaultIfEmpty()
                         join d in db.Customers on a.LeadType equals d.CustomerID into cst
                         from d in cst.DefaultIfEmpty()
                           where (AccGroup == null || AccGroup == 0 || a.LeadType == AccGroup)

                           select  new
                           {

                               a.createdby,
                               a.createddate,
                               customers = d.CustomerName,
                               a.Notes,
                               a.Password,
                               a.PasswordDataId,
                               a.Title,

                               a.Url,
                               a.UserName,


                           }).ToList();

            // CLIENT: nested AssignedTo collection, keyed by PasswordDataId (missing key -> empty sequence).
            var pwdIds = serverRows.Select(o => o.PasswordDataId).ToList();
            var assignLookup = (from z in db.passworddetailsass
                                join y in db.Employees on z.employeeid equals y.EmployeeId
                                where pwdIds.Contains(z.passworddetailid)
                                select new
                                {
                                    z.passworddetailid,
                                    id = y.EmployeeId,
                                    LastName = (y.LastName != null) ? y.LastName : "",
                                    FirstName = (y.FirstName != null) ? y.FirstName : "",
                                    MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                                }).ToList().ToLookup(x => x.passworddetailid);

            // CLIENT re-projection: reproduce the SAME member names + order as the original projection (the page
            // columns reference customers, AssignedTo, etc.), pulling AssignedTo from the lookup.
            var ModList = serverRows.Select(o => new
            {
                o.createdby,
                o.createddate,
                o.customers,
                o.Notes,
                o.Password,
                o.PasswordDataId,
                o.Title,

                o.Url,
                o.UserName,
                AssignedTo = assignLookup[o.PasswordDataId]
                                .Select(z => new { z.id, z.LastName, z.FirstName, z.MiddleName })
                                .Distinct().ToList(),


            }).Where(x => empfilter == null || x.AssignedTo.Select(z => z.id).ToList().Contains((long)empfilter));

            ////search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                ModList = ModList.Where(p => p.Password.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.UserName.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Url.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Notes.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Title.ToString().ToLower().Contains(search.ToLower()));

            }

            //SORT
           
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                ModList = ModList.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = ModList.Count();
            var data = ModList.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });


        }

        public JsonResult GetmyData(long? AccGroup)
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
            Status st = new Status();
            var UserId = User.Identity.GetUserId();
            var empId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var assemployeesuserid = (from z in db.passworddetailsass
                                      join y in db.Employees on z.employeeid equals y.EmployeeId
                                      join x in db.Users on y.UserId equals x.Id
                                      where x.Id == UserId
                                      select new
                                      {
                                          y.EmployeeId

                                      }).Select(o => o.EmployeeId).FirstOrDefault();
            DateTime assdate = DateTime.Now.AddDays(-90);
            var taskassign = (from z in db.TaskAssigneds
                              join y in db.Employees on z.EmployeeId equals y.EmployeeId
                              where z.Status == "Assigned" && z.chkStatus == Status.active &&
                              (z.CreatedDate >= assdate)
                              && z.EmployeeId==empId
                              select z);


            var ModList = (from x in db.ProTasks

                join a in db.passworddetails on x.CustomerID equals a.LeadType
                           join c in db.Customers on x.CustomerID equals c.CustomerID into cus
                           from c in cus.DefaultIfEmpty()

                           let taskassigns = taskassign.Where(x => x.ProTaskId == x.ProTaskId && x.Status == "Assigned" && x.chkStatus == Status.active).Select(x => x.EmployeeId).ToList()

                           where (AccGroup == null || AccGroup == 0 || a.LeadType == AccGroup)
                           && 
                            (taskassigns.Contains(empId))

                           select new
                           {
                               a.createdby,
                               a.createddate,
                               customers=(c==null)?"":c.CustomerName,
                               a.Notes,
                               a.Password,
                               a.PasswordDataId,
                               a.Title,
                               a.Url,
                               a.UserName,
                               


                                
                           }
                           ).Distinct();
            var ModListtwo = (

                           from a in db.passworddetails


                           join d in db.Customers on a.LeadType equals d.CustomerID into cust
                           from d in cust.DefaultIfEmpty()
                           let assign = db.passworddetailsass.Where(x => x.passworddetailid == a.PasswordDataId).Select(x => x.employeeid).ToList()
                           
                           where (AccGroup == null || AccGroup == 0 || a.LeadType == AccGroup)
                           && (assign.Contains(assemployeesuserid))

                           select new
                           {
                               a.createdby,
                               a.createddate,
                               customers = (d == null) ? "" : d.CustomerName,
                               a.Notes,
                               a.Password,
                               a.PasswordDataId,
                               a.Title,
                               a.Url,
                               a.UserName,




                           }
                          );
            ////search
            ///
            ModList = ModListtwo;
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                ModList = ModList.Where(p => p.Password.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.UserName.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Url.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Notes.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Title.ToString().ToLower().Contains(search.ToLower()));

            }

            //SORT

            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                ModList = ModList.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = ModList.Count();
            var data = ModList.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });


        }

        [HttpPost]
        public ActionResult Edit(passworddetailsViewModel vm)         
        {
            var today = System.DateTime.Now;
            var userid = User.Identity.GetUserId();
            var v = db.passworddetails.Find((long)vm.passworddetailsid);

            var oldpassword = v.Password;
            var oldusername = v.UserName;
            var oldurl = v.Url;
            var oldCustomer = db.Customers.Where(o => o.CustomerID == v.LeadType).Select(o => o.CustomerName).FirstOrDefault();
            var newCustomer = db.Customers.Where(o => o.CustomerID == vm.LeadType).Select(o => o.CustomerName).FirstOrDefault();


            v.createdby = userid;
            v.createddate = today;
            v.LeadType = vm.LeadType;
            v.Notes = vm.Notes;
            v.Password = vm.Password;
            v.Title = vm.Title;
            v.Url = vm.Url;
            v.UserName = vm.UserName;

          
            db.Entry(v).State=EntityState.Modified;
            db.SaveChanges();

            db.SaveChanges();
            if (vm.AssignedMembers != null)
            {
                var assmemb = db.passworddetailsass.Where(o => o.passworddetailid == vm.passworddetailsid);
                db.passworddetailsass.RemoveRange(assmemb);
                db.SaveChanges();
                foreach (var k in vm.AssignedMembers)
                {
                    passworddetailsas n = new passworddetailsas
                    {
                        employeeid = k,
                        passworddetailid = (long)vm.passworddetailsid,
                    };
                    db.passworddetailsass.Add(n);
                    db.SaveChanges();
                }
            }
            var changes = "Passwords Updated Successfully";
            if(vm.Password !=oldpassword)
            {
                 changes ="Password Change From "+oldpassword+"To "+vm.Password;
                com.addlog(LogTypes.Updated, userid, "Passwords", "Passwords", findip(), v.PasswordDataId, changes);

            }
            if (vm.UserName != oldusername)
            {
                changes = "User Name Change From " + oldusername + "To " + vm.UserName;
                com.addlog(LogTypes.Updated, userid, "Passwords", "Passwords", findip(), v.PasswordDataId, changes);

            }
            if (vm.Url != oldurl)
            {
                changes = "Url Change From " + oldurl + "To " + vm.Url;
                com.addlog(LogTypes.Updated, userid, "Passwords", "Passwords", findip(), v.PasswordDataId, changes);

            } 
            if (newCustomer != oldCustomer)
            {
                changes = "Customer Change From " + oldCustomer + "To " +newCustomer;
                com.addlog(LogTypes.Updated, userid, "Passwords", "Passwords", findip(), v.PasswordDataId, changes);

            }
            
            com.addlog(LogTypes.Updated, userid, "Passwords", "Passwords", findip(), v.PasswordDataId, "Updated");

            Success("Success", true);
            if(vm.comefromtask)
                return RedirectToAction("mytask","protask");
            else
            return RedirectToAction("MyPassword");
        }
        [HttpPost]
        public ActionResult Create(passworddetailsViewModel vm)
        {
            var today = System.DateTime.Now;
            var userid = User.Identity.GetUserId();
            passworddetail a = new passworddetail
            {
                 createdby =userid,
                  createddate =today,
                 LeadType =vm.LeadType,
                 Notes =vm.Notes,
                 Password=vm.Password,
                 Title=vm.Title,
                 Url=vm.Url,
                 UserName=vm.UserName,

            };
            db.passworddetails.Add(a);
            db.SaveChanges();
            var ids = a.PasswordDataId;
            if (vm.AssignedMembers != null)
            {
                foreach (var k in vm.AssignedMembers)
                {
                    passworddetailsas n = new passworddetailsas
                    {
                        employeeid = k,
                        passworddetailid = (long)ids,
                    };
                    db.passworddetailsass.Add(n);
                    db.SaveChanges();
                }
            }
                Success("Success", true);

           com.addlog(LogTypes.Created, userid, "Passwords", "Passwords", findip(), a.PasswordDataId, "Passwords Created Successfully");
            if(vm.comefromtask)
                return RedirectToAction("mytask", "protask");
            else
            return RedirectToAction("create");
        }
        public ActionResult Create()
        {
            var mcs = db.Customers.Where(o=>!o.CustomerName.StartsWith("OLD-")).Select(s => new
            {
                Id = s.CustomerID,
                Name = s.CustomerName
            }).Take(1).ToList();
            ViewBag.pwdgroup = QkSelect.List(mcs, "Id", "Name");
            List<SelectFormat> serialisedJson;
            serialisedJson = db.Employees
                   .Select(s => new SelectFormat
                   {
                       id = s.EmployeeId,
                       text = s.FirstName + " " + s.LastName
                   })
                   .ToList();
            var initial = new SelectFormat() { id = 0, text = "All" };
            serialisedJson.Insert(0, initial);

            ViewBag.team = new MultiSelectList(serialisedJson, "id", "text");

            passworddetailsViewModel vmodel = new passworddetailsViewModel();
            return View(vmodel);
        }
        public ActionResult Createnew(long id)
        {
            var custid = id;
            passworddetail v = db.passworddetails.Where(o => o.LeadType == custid).OrderByDescending(o=>o.PasswordDataId).FirstOrDefault();
            if (v != null)
            {


                passworddetailsViewModel vmodel = new passworddetailsViewModel();
                vmodel.LeadType = v.LeadType;
                vmodel.Notes = v.Notes;
                vmodel.Password = v.Password;
                vmodel.passworddetailsid = v.PasswordDataId;
                vmodel.Title = v.Title;
                vmodel.Url = v.Url;


                vmodel.UserName = v.UserName;
                var mcs = db.Customers.Where(o => o.CustomerID == v.LeadType).Where(o => !o.CustomerName.StartsWith("OLD-")).Select(s => new
                {
                    Id = s.CustomerID,
                    Name = s.CustomerName
                }).ToList();
                ViewBag.pwdgroup = QkSelect.List(mcs, "Id", "Name");
                var assmebers = db.passworddetailsass.Where(o => o.passworddetailid == id).Select(o => o.employeeid).Distinct().ToArray() ?? null;
                List<SelectFormat> serialisedJson;
                serialisedJson = db.Employees
                       .Select(s => new SelectFormat
                       {
                           id = s.EmployeeId,
                           text = s.FirstName + " " + s.LastName
                       })
                       .ToList();
                var initial = new SelectFormat() { id = 0, text = "All" };
                serialisedJson.Insert(0, initial);
                vmodel.comefromtask = true;
                ViewBag.team = new MultiSelectList(serialisedJson, "id", "text", assmebers);
                return View("Editnew", vmodel);
            }
            else
            {




                var mcs = db.Customers.Where(o => o.CustomerID == custid).Select(s => new
                {
                    Id = s.CustomerID,
                    Name = s.CustomerName
                }).Take(1).ToList();
                ViewBag.pwdgroup = QkSelect.List(mcs, "Id", "Name");
                List<SelectFormat> serialisedJson;
                serialisedJson = db.Employees
                       .Select(s => new SelectFormat
                       {
                           id = s.EmployeeId,
                           text = s.FirstName + " " + s.LastName
                       })
                       .ToList();
                var initial = new SelectFormat() { id = 0, text = "All" };
                serialisedJson.Insert(0, initial);

                ViewBag.team = new MultiSelectList(serialisedJson, "id", "text");

                passworddetailsViewModel vmodel = new passworddetailsViewModel();
                vmodel.LeadType = custid;
                vmodel.comefromtask = true;
                return View(vmodel);
            }
        }

        public ActionResult Edit(long id)
        {
            passworddetail v = db.passworddetails.Find(id);
            passworddetailsViewModel vmodel = new passworddetailsViewModel();
            vmodel.LeadType = v.LeadType;
            vmodel.Notes = v.Notes;
            vmodel.Password = v.Password;
            vmodel.passworddetailsid = id;
            vmodel.Title = v.Title;
            vmodel.Url = v.Url;
 

            vmodel.UserName = v.UserName;
            var mcs = db.Customers.Where(o => o.CustomerID == v.LeadType).Where(o =>!o.CustomerName.StartsWith("OLD-")).Select(s => new
            {
                Id = s.CustomerID,
                Name = s.CustomerName
            }).ToList();
            ViewBag.pwdgroup = QkSelect.List(mcs, "Id", "Name");
            List<SelectFormat> serialisedJson;
            serialisedJson = db.Employees
                   .Select(s => new SelectFormat
                   {
                       id = s.EmployeeId,
                       text = s.FirstName + " " + s.LastName
                   })
                   .ToList();
            var initial = new SelectFormat() { id = 0, text = "All" };
            serialisedJson.Insert(0, initial);
            var assmebers = db.passworddetailsass.Where(o => o.passworddetailid == id).Select(o => o.employeeid).Distinct().ToArray() ?? null;

            ViewBag.team = new MultiSelectList(serialisedJson, "id", "text", assmebers);

            return View(vmodel);
        }
        public ActionResult Delete(long id)
        {
            var pro = db.passworddetails.Find(id);
            return PartialView(pro);
        }
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            var UserId = User.Identity.GetUserId();
            Deletepass(id);

            stat = true;
            msg = "Successfully Deleted Task details.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }
        public void Deletepass(long id)
        {
            var v = db.passworddetails.Where(o => o.PasswordDataId == id);
            db.passworddetails.RemoveRange(v);
            db.SaveChanges();
   
        }
    }
}
