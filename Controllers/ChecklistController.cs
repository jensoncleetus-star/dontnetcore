using QuickSoft.Web;
using System.Linq.Dynamic.Core;
using ApplicationUserManager = Microsoft.AspNetCore.Identity.UserManager<QuickSoft.Models.ApplicationUser>;
using ApplicationSignInManager = Microsoft.AspNetCore.Identity.SignInManager<QuickSoft.Models.ApplicationUser>;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class ChecklistController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ChecklistController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Checklist
        public ActionResult Index()
        {
            return View();
        }

        [RedirectingAction]
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Checklist List")]
        public ActionResult GetCheck(long? Stage)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            int recordsTotal = 0;
            var userpermission = User.IsInRole("All Sales Entry");
            var UserId = User.Identity.GetUserId();

            string search = Request.Form.GetValues("search[value]")[0];
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();


            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var v = (from a in db.Checklists
                     join d in db.TaskStatus on a.Stage equals d.TaskStatusId into cst
                     from d in cst.DefaultIfEmpty()
                     where ((Stage==null) || (Stage==0) || (a.Stage == Stage))
                     select new
                     {
                         a.ChecklistId,
                         Stage=d.StatusName,

                     });
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }

            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        [QkAuthorize(Roles = "Dev,Create Checklist")]
        public ActionResult Create()
        {
            var Check = new ChecklistViewModel();
            var tsks = db.TaskStatus
                  .Select(s => new
                  {
                      ID = s.TaskStatusId,
                      Name = s.StatusName
                  })
                  .ToList();
            ViewBag.Taskstatus = QkSelect.List(tsks, "ID", "Name");
            ViewBag.LastEntry = db.Checklists.Select(p => p.ChecklistId).AsEnumerable().DefaultIfEmpty(0).Max();

            return View(Check);
        }
        [RedirectingAction]
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create Checklist")]
        public ActionResult CreateChecklist(string[][] array, string[] data)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {

                var UserId = User.Identity.GetUserId();
                var stage = data[0];

                Checklist check = new Checklist();
                check.Stage = Convert.ToInt64(stage);
                db.Checklists.Add(check);
                db.SaveChanges();
                Id = check.ChecklistId;

                foreach (var arr in array)
                {
                    ChecklistItem checkit = new ChecklistItem();
                    checkit.ListName = arr[0];
                    checkit.AddNote = (arr[1] == "true") ? true : false;
                    checkit.Checklist = Id;
                    db.ChecklistItems.Add(checkit);
                    db.SaveChanges();
                }

                msg = "Checklist Status added successfully.";
                stat = true;
                com.addlog(LogTypes.Created, UserId, "Checklist", "Checklist", findip(), Id, "Checklist Successfully");

            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [QkAuthorize(Roles = "Dev,Edit Checklist")]
        public ActionResult Edit(long? id)
        {
            ChecklistViewModel vmodel = (from b in db.Checklists
                                         join d in db.ChecklistItems on b.ChecklistId equals d.Checklist into cst
                                         from d in cst.DefaultIfEmpty()
                                         where b.ChecklistId == id
                                         select new ChecklistViewModel
                                         {
                                             Stage = b.Stage
                                         }).FirstOrDefault();
            var tsks = db.TaskStatus
                  .Select(s => new
                  {
                      ID = s.TaskStatusId,
                      Name = s.StatusName
                  })
                  .ToList();
            ViewBag.Taskstatus = QkSelect.List(tsks, "ID", "Name");

            ViewBag.preEntry = db.Checklists.Where(a => a.ChecklistId < id).Select(a => a.ChecklistId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.Checklists.Where(a => a.ChecklistId > id).Select(a => a.ChecklistId).DefaultIfEmpty().Min();

            return View(vmodel);
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Checklist")]
        public ActionResult Update(string[][] array, string[] data)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            var UserId = User.Identity.GetUserId();

            if (ModelState.IsValid)
            {
                var Checkid = Convert.ToInt64(data[1]);
                var stage = data[0];
                Checklist check = db.Checklists.Find(Checkid);
                check.Stage = Convert.ToInt64(stage); 
                db.Entry(check).State = EntityState.Modified;
                db.SaveChanges();

                db.ChecklistItems.RemoveRange(db.ChecklistItems.Where(a => a.Checklist == Checkid));
                db.SaveChanges();

                foreach (var arr in array)
                {
                    ChecklistItem checkit = new ChecklistItem();
                    checkit.ListName = arr[0];
                    checkit.AddNote = (arr[1] == "true") ? true : false;
                    checkit.Checklist = Checkid;
                    db.ChecklistItems.Add(checkit);
                    db.SaveChanges();
                }

                msg = "Checklist Updated successfully.";
                stat = true;
                com.addlog(LogTypes.Created, UserId, "Checklist", "Checklist", findip(), Id, "Checklist Successfully");

            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
                
        [HttpGet]
        public JsonResult GetListItems(long CheckID)
        {
            var ConD = (from a in db.ChecklistItems
                        where a.Checklist == CheckID
                        select new
                        {
                            Name = a.ListName,
                            Note = a.AddNote,
                        }).ToList();
            return Json(ConD);
        }

        [HttpGet]
        public JsonResult GetCheckItems(long CheckID)
        {
            var ConD = (from a in db.ChecklistItems
                        join c in db.Checklists on a.Checklist equals c.ChecklistId into chk
                        from c in chk.DefaultIfEmpty()
                        where c.Stage == CheckID
                        select new
                        {
                            Id=a.Id,
                            Name = a.ListName,
                            Note = a.AddNote,
                        }).ToList();
            return Json(ConD);
        }
        [RedirectingAction]
        //[Authorize(Roles = "Dev,Delete Checklist")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Checklist check = db.Checklists.Find(id);
            if (check == null)
            {
                return NotFound();
            }
            return PartialView(check);
        }

        // POST: /Delete/5

        [RedirectingAction]
        //[Authorize(Roles = "Dev,Delete Checklist")]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            Checklist check = db.Checklists.Find(id);
            
            var checkitems= db.ChecklistItems.Where(a => a.Checklist==id).Select(a => a.Id).ToList();
            var remarklist= db.RemarkChecklists.Where(a => checkitems.Contains((long)a.Checklistitemid)).Select(a => a.Id).ToList();
            
                
            var checkitem = db.ChecklistItems.Where(a => a.Checklist == id).ToList();
            if (checkitem.Any() && !remarklist.Any())
            {
                foreach (var arr in checkitem)
                {
                    var item = db.ChecklistItems.Where(a => a.Checklist == arr.Checklist);
                    if (item != null)
                    {
                        db.ChecklistItems.RemoveRange(db.ChecklistItems.Where(a => a.Checklist == arr.Checklist));
                        db.SaveChanges();
                    }
                }
                db.Checklists.RemoveRange(db.Checklists.Where(a => a.ChecklistId == id));
                db.SaveChanges();
                com.addlog(LogTypes.Deleted, UserId, "Checklist", "Checklist", findip(), check.ChecklistId, "Checklist Deleted Successfully");
                stat = true;
                msg = "Successfully Deleted Checklist details.";
            }
            else
            {
                stat = false;
                msg = "Checklist is used in Remark.";
            }           

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
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
