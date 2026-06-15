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
using Microsoft.AspNetCore.Identity;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class StickyNoteController : BaseController
    {

        // GET: StickyNotes
        public ActionResult Index()
        {
            return View();
        }

        ApplicationDbContext db;
        Common com;
        public StickyNoteController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }


        public JsonResult GetNote()
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

            var v = (from a in db.StickyNotes
                     join b in db.StickyLabels on a.Label equals b.Id into note
                     from b in note.DefaultIfEmpty()
                     join c in db.Users on a.CreatedBy equals c.Id                                        
                     select new
                     {
                         a.Id,
                         b.LabelName,
                         a.NoteName,
                         a.NoteContent,
                         a.CreatedDate,  
                         a.Status,                       
                         User = c.UserName
                     });

            db.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.NoteName.ToString().ToLower().Contains(search.ToLower()));
                v = v.Where(p => p.NoteContent.ToString().ToLower().Contains(search.ToLower()));
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

        //[QkAuthorize(Roles = "Dev,Create StickyNote")]
        public ActionResult Create()
        {            
            var use = db.StickyLabels
                             .Select(s => new
                             {
                                 ID = s.Id,
                                 Name = s.LabelName
                             })
                             .ToList();
            ViewBag.notes = QkSelect.List(use, "ID", "Name");

            return PartialView();
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Create StickyNote")]
        //[ValidateAntiForgeryToken]
        public JsonResult Create(StickyNoteViewModel snotes)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var UserId = User.Identity.GetUserId();
                var Exists = db.StickyNotes.Any(c => c.NoteName == snotes.NoteName && c.CreatedBy == UserId);
                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                if (Exists)
                {
                    msg = "Note Name already exists.";
                    stat = false;
                }
                else
                {
                    var today = Convert.ToDateTime(System.DateTime.Now);
                    var sn = new StickyNote
                    {
                        NoteName = snotes.NoteName,
                        NoteContent = snotes.NoteContent,
                        Label = snotes.Label,
                        CreatedDate = today,
                        Status = snotes.Status,
                        CreatedBy = UserId,
                        Branch=BranchID
                    };
                    db.StickyNotes.Add(sn);
                    db.SaveChanges();
                    Id = sn.Id;
                    com.addlog(LogTypes.Created, UserId, "StickyNote", "StickyNotes", findip(), Id, "Note Added Successfully");
                    msg = "Successfully added Note details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form!";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id } };
        }

       // [QkAuthorize(Roles = "Dev,Edit StickyNote")]
        public ActionResult Edit(long? id)
        {
            var note = db.StickyNotes.Select(s => new
            {
                NoteId = s.Id
            }).ToList();
            ViewBag.notes = QkSelect.List(note, "NoteId");
            var use = db.StickyLabels
                             .Select(s => new
                             {
                                 ID = s.Id,
                                 Name = s.LabelName
                             })
                             .ToList();
            ViewBag.notes = QkSelect.List(use, "ID", "Name");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StickyNote NoteInfo = db.StickyNotes.Find(id);

            if (NoteInfo == null)
            {
                return NotFound();
            }

            return PartialView(NoteInfo);
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Edit StickyNote")]
        public JsonResult Edit(long? id, StickyNote Snote)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                StickyNote note = db.StickyNotes.Find(id);
                note.Label = Snote.Label;
                note.NoteName = Snote.NoteName;
                note.NoteContent = Snote.NoteContent;
                db.SaveChanges();

                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Updated, UserId, "StickyNote", "StickyNotes", findip(), Snote.Id, "Label Updated Successfully");
                msg = "Successfully Updated Note Details.";
                stat = true;
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }

       // [QkAuthorize(Roles = "Dev,Delete StickyNote")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StickyNote NoteInfo = db.StickyNotes.Find(id);
            if (NoteInfo == null)
            {
                return NotFound();
            }

            return PartialView(NoteInfo);
        }

        // POST: Field/Delete/5
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Delete StickyNote")]
        public JsonResult Delete(long id, IFormCollection collection)
        {
            bool stat = false;
            string msg;

            StickyNote NoteInfo = db.StickyNotes.Find(id);
            db.StickyNotes.Remove(NoteInfo);
            db.SaveChanges();

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "StickyNote", "StickyNotes", findip(), (long)NoteInfo.Id, "Note Deleted Successfully");
            stat = true;
            msg = "Successfully Deleted Note details.";
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
