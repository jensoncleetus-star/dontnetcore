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
using System;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace QuickSoft.Controllers
{
    public class UserEditDaysController : BaseController
    {


        ApplicationDbContext db;
        Common com;
        public UserEditDaysController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        public ActionResult Create()
        {
            var stands = db.Users
                        .Select(s => new
                        {
                            FieldID = s.Id,
                            FieldName = s.UserName
                        })
                         .ToList();
            ViewBag.users = QkSelect.List(stands, "FieldID", "FieldName");

            return PartialView();
        }
        public ActionResult Index()
        {
            return View();
        }
        // POST: UserEditDays/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Create UserEditDays")]

        // [QkAuthorize(Roles = "Dev,Create UserEditDays")]
        public ActionResult Create( UserEditDays  ptype)
        {
            bool stat = false;
            Int64 id = ptype.id;
            string msg;
            Int64 Id = 0;
    
               
                    var UserId = User.Identity.GetUserId();
                    UserEditDays c = new UserEditDays();
                    c.id = ptype.id;
                    c.days = ptype.days;
            c.srdays = ptype.srdays;
            c.pedays = ptype.pedays;
            c.prdays = ptype.prdays;
            c.stkdays = ptype.stkdays;
            c.seitem = ptype.seitem;
            c.pedate = ptype.pedate;
                    db.UserEditDayss.Add(ptype);
                    db.SaveChanges(); 
                    com.addlog(LogTypes.Created, UserId, "UserEditDays", "UserEditDayss", findip(), id, "User Edit Days Added Successfully");
                    Success("User Edit Dayss added successfully...", true);
              
            
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = true, message = "Success", Id = id } };
        }
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserEditDays qttype = db.UserEditDayss.Find(id);
            if (qttype == null)
            {
                return NotFound();
            }
            var stands = db.Users
                        .Select(s => new
                        {
                            FieldID = s.Id,
                            FieldName = s.UserName
                        })
                         .ToList();
            ViewBag.users = QkSelect.List(stands, "FieldID", "FieldName");

            return PartialView(qttype);
        }



        // POST: LeadType/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public JsonResult Edit( UserEditDays qttype)
        {
            bool stat = false;
            string msg;

          
                
                    db.Entry(qttype).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "qutoationType", "qutoationTypes", findip(), qttype.id, "Quotation  Type Details Updated Successfully");


                    msg = "Successfully updated User Edit Days details.";
                    stat = true;
                
          
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        // [QkAuthorize(Roles = "Dev,Delete Lead Rejection")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserEditDays qttype = db.UserEditDayss.Find(id);
            if (qttype == null)
            {
                return NotFound();
            }
            return PartialView(qttype);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //  [QkAuthorize(Roles = "Dev,Delete Lead Type")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
           
                stat = DeleteFn(id);
                msg = "Successfully deleted User Edit Days.";
            
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        // [QkAuthorize(Roles = "Dev,Delete LeadType")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;

            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " UserEditDays, Unable to Delete " + notdel + " UserEditDays. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + "LaedType.", true);
            }
            else
            {
                Success("Deleted " + count + " UserEditDays.", true);
            }
            return RedirectToAction("Index", "UserEditDays");
        }


        public bool DeleteFn(long id)
        {
            UserEditDays qttype = db.UserEditDayss.Find(id);
            if (qttype != null)
            {
                db.UserEditDayss.RemoveRange(db.UserEditDayss.Where(a => a.id == id));


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "UserEditDays", "QutoationTypes", findip(), qttype.id, "User Edit Days Deleted Successfully");
                db.SaveChanges();
            }
            return true;
        }
        public ActionResult GetUserEditDays(string ColName)
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

            
           
            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.UserEditDayss
                     join b in db.Users on a.userid equals b.Id
                    
                     select new
                     {

                         a.id,
                         a.days,
                         a.srdays,
                         a.pedays,
                         a.prdays,
                         a.stkdays,
                         b.UserName
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.UserName.ToString().ToLower().Contains(search.ToLower()));
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);

            }

            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        //Used  for search the MCs
      

    }
}

           