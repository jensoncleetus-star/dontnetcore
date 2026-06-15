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
    [RedirectingAction]
    public class ProjectTypeController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ProjectTypeController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        
        [QkAuthorize(Roles = "Dev,ProjectType List")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,ProjectType List")]
        public ActionResult GetProjectType()
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
            var v = (from a in db.ProjectTypes
                     join b in db.Users on a.CreatedBy equals b.Id into usr
                     from b in usr.DefaultIfEmpty()
                     select new
                     {
                         a.ProjectTypeID,
                         a.TypeName,
                         a.Details,
                         b.UserName
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.TypeName.ToString().ToLower().Contains(search.ToLower()));
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

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Create ProjectType")]
        public ActionResult Create()
        {
            return PartialView();
        }


        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create ProjectType")]
        public JsonResult Create([Bind("ProjectTypeID,TypeName,Details")] ProjectType ptype)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.ProjectTypes.Any(c => c.TypeName == ptype.TypeName);
                if (Exists)
                {
                    msg = "Project Type already exists.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                    var today = Convert.ToDateTime(System.DateTime.Now);

                    ptype.Status = Status.active;
                    ptype.CreatedDate = today;
                    ptype.CreatedBy = UserId;
                    ptype.Branch = BranchID;
                    ptype.Editable = choice.Yes;

                    db.ProjectTypes.Add(ptype);
                    db.SaveChanges();
                    Id = ptype.ProjectTypeID;
                    msg = "Project Type added successfully.";
                    stat = true;
                    com.addlog(LogTypes.Created, UserId, "ProjectType", "ProjectTypes", findip(), Id, "Project Type Added Successfully");

                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }

        [QkAuthorize(Roles = "Dev,Edit ProjectType")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProjectType ptype = db.ProjectTypes.Find(id);
            if (ptype == null)
            {
                return NotFound();
            }
            return PartialView(ptype);
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit ProjectType")]
        public JsonResult Edit(ProjectType ptype, long? id)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.ProjectTypes.Any(c => c.TypeName == ptype.TypeName && c.ProjectTypeID != ptype.ProjectTypeID);
                if (Exists)
                {
                    msg = "Project Type already exists.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    ProjectType prtype = db.ProjectTypes.Find(id);
                    prtype.TypeName = ptype.TypeName;
                    prtype.Details = ptype.Details;

                    db.Entry(prtype).State = EntityState.Modified;
                    db.SaveChanges();
                    msg = "Successfully updated Project Type details.";
                    stat = true;

                    com.addlog(LogTypes.Updated, UserId, "ProjectType", "ProjectTypes", findip(), ptype.ProjectTypeID, "Project Types Updated Successfully");

                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        
        [QkAuthorize(Roles = "Dev,Delete ProjectType")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProjectType ptype = db.ProjectTypes.Find(id);
            if (ptype == null)
            {
                return NotFound();
            }
            return PartialView(ptype);
        }

        
        [HttpPost, ActionName("Delete")]
        [QkAuthorize(Roles = "Dev,Delete ProjectType")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var Exists = db.Projects.Any(c => c.ProType == id);
            if (Exists)
            {
                msg = "Unable to delete Type, Projects with this Type exists.";
                stat = false;
            }
            else
            {
                var UserId = User.Identity.GetUserId();

                ProjectType ptype = db.ProjectTypes.Find(id);
                db.ProjectTypes.Remove(ptype);
                db.SaveChanges();
                stat = true;
                msg = "Successfully deleted Project Type details.";

                com.addlog(LogTypes.Deleted, UserId, "ProjectType", "ProjectTypes", findip(), ptype.ProjectTypeID, "Project Type Deleted Successfully");

            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public JsonResult SearchProjectType(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.ProjectTypes.Where(p => p.TypeName.ToLower().Contains(q.ToLower()) || p.TypeName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.TypeName, //each json object will have 
                                      id = b.ProjectTypeID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.ProjectTypes.Select(b => new SelectFormat
                {
                    text = b.TypeName, //each json object will have 
                    id = b.ProjectTypeID
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
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
