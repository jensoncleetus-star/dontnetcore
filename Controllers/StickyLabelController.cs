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
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Net;
using QuickSoft.ViewModel;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class StickyLabelController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public StickyLabelController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: StickyLabel
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetLabel()
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var UserId = User.Identity.GetUserId();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            db.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.StickyLabels
                     join b in db.Users on a.CreatedBy equals b.Id into stype
                     from b in stype.DefaultIfEmpty()
                     select new
                     {
                         a.Id,
                         a.LabelName,
                         a.LabelColor,
                         a.Status,
                         a.Branch,
                         User = b.UserName,
                         a.CreatedDate,
                     });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.LabelName.ToString().ToLower().Contains(search.ToLower()));
                v = v.Where(p => p.LabelColor.ToString().ToLower().Contains(search.ToLower()));
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
        
       // [QkAuthorize(Roles = "Dev,Create StickyLabel")]
        public ActionResult Create()
        {
            return PartialView();
        }

        [HttpPost]        
       // [ValidateAntiForgeryToken]
        //[QkAuthorize(Roles = "Dev,Create StickyLabel")]
        public JsonResult Create(StickyLabelViewModel SLabel)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            
            if (ModelState.IsValid)
            {
                var UserId = User.Identity.GetUserId();
                var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                var today = Convert.ToDateTime(System.DateTime.Now);
                var Exists1 = db.StickyLabels.Any(c => c.LabelName == SLabel.LabelName);
                if (Exists1)
                {
                    msg = "Label Name already exists.";
                    stat = false;
                }
                else
                {
                    var sl = new StickyLabel
                    {
                        LabelName = SLabel.LabelName,
                        LabelColor = SLabel.LabelColor,
                        CreatedBy = UserId,
                        CreatedDate = today,
                        Status = SLabel.Status,
                        Branch = BranchID
                    };
                    db.StickyLabels.Add(sl);
                    db.SaveChanges();
                    Id = sl.Id;
                    com.addlog(LogTypes.Created, UserId, "StickyLabel", "StickyLabels", findip(), (long)sl.Id, "Label Added Successfully");

                    msg = "Successfully added Label details.";
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

       // [QkAuthorize(Roles = "Dev,Edit StickyLabel")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            StickyLabel LabelInfo = db.StickyLabels.Find(id);

            if (LabelInfo == null)
            {
                return NotFound();
            }
            return PartialView(LabelInfo);
        }

        [HttpPost]
       // [QkAuthorize(Roles = "Dev,Edit StickyLabel")]
        public JsonResult Edit(long id, StickyLabel Slabel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.StickyLabels.Any(c => c.LabelName == Slabel.LabelName && c.Id != id);
                if (Exists)
                {
                    msg = "Label already exists.";
                    stat = false;
                }
                else
                {
                    StickyLabel sl = db.StickyLabels.Find(id);
                   
                    sl.LabelName = Slabel.LabelName;
                    sl.LabelColor = Slabel.LabelColor;
                    sl.Status = Slabel.Status;
                    db.Entry(sl).State = EntityState.Modified;
                    db.SaveChanges();
                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "StickyLabel", "StickyLabels", findip(), sl.Id, "Label Updated Successfully");


                    msg = "Successfully Updated Label Details.";
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


       // [QkAuthorize(Roles = "Dev,Delete StickyLabel")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StickyLabel LabelInfo = db.StickyLabels.Find(id);
            if (LabelInfo == null)
            {
                return NotFound();
            }

            return PartialView(LabelInfo);
        }

        // POST: Field/Delete/5
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Delete StickyLabel")]
        public JsonResult Delete(long id, IFormCollection collection)
        {
            bool stat = false;
            string msg;
            StickyLabel LabelInfo = db.StickyLabels.Find(id);
            db.StickyLabels.Remove(LabelInfo);
            db.SaveChanges();

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "StickyLabel", "StickyLabels", findip(), (long)LabelInfo.Id, "Label Deleted Successfully");

            stat = true;
            msg = "Successfully Deleted Label  details.";
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
