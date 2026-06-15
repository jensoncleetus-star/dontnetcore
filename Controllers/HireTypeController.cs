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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class HireTypeController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public HireTypeController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Contact
        //     [QkAuthorize(Roles = "Dev,HireType List")]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        //  [QkAuthorize(Roles = "Dev,HireType List")]
        public JsonResult GetData()
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

            var uDev = User.IsInRole("Dev");
            var uEdit = User.IsInRole("Edit HireType");

            var v = (from a in db.HireTypes
                     join b in db.Users on a.CreatedBy equals b.Id into usr
                     from b in usr.DefaultIfEmpty()
                     select new
                     {
                         id = a.HireTypeId,
                         a.Name,
                         a.Period,
                         a.PeriodType,
                         a.Note,
                         a.Status,
                         a.Editable,
                         CreatedBy = b.Name,
                         a.CreatedDate,
                         Dev = uDev,
                         Edit = uEdit,
                     });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.Period.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Name.ToString().ToLower().Contains(search.ToLower()));
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

        // [QkAuthorize(Roles = "Dev,Create HireType")]
        public ActionResult Create()
        {
            var stands = db.HireTypes
                         .Select(s => new
                         {
                             FieldID = s.Name,
                             FieldName = s.HireTypeId
                         })
                         .ToList();

            List<SelectListItem> type = new List<SelectListItem>();
            type.Add(new SelectListItem { Text = "Days", Value = "Days" });
            type.Add(new SelectListItem { Text = "Week", Value = "Week" });
            type.Add(new SelectListItem { Text = "Month", Value = "Month" });
            ViewBag.PType = QkSelect.List(type, "Text", "Value");
            return PartialView();

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //   [QkAuthorize(Roles = "Dev,Create HireType")]
        public JsonResult Create(HireType material)
        {
            bool stat = false;
            string msg;
            Int64 id = 0;
            if (ModelState.IsValid)
            {
                var contactExists = db.HireTypes.Any(u => u.Name == material.Name);
                if (contactExists)
                {
                    msg = "Hire Type Name exists.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                    var con = new HireType
                    {
                        Name = material.Name,
                        Period = material.Period,
                        PeriodType = material.PeriodType,
                        Note = material.Note,
                        Status = material.Status,
                        CreatedBy = UserId,
                        CreatedDate = Convert.ToDateTime(System.DateTime.Now),
                        CreatedBranch = BranchID,
                        Editable = choice.Yes,
                    };
                    db.HireTypes.Add(con);
                    db.SaveChanges();
                    id = con.HireTypeId;

                    com.addlog(LogTypes.Created, UserId, "HireType", "HireTypes", findip(), con.HireTypeId, "Hire Type Added Successfully");
                    msg = "Successfully added Hire Type details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, id } };
        }


        // GET: contact/Edit/5
        // [QkAuthorize(Roles = "Dev,Edit HireType")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            HireType con = db.HireTypes.Find(id);

            if (con == null)
            {
                return NotFound();
            }
            var stands = db.HireTypes
                         .Select(s => new
                         {
                             FieldID = s.Name,
                             FieldName = s.HireTypeId
                         })
                         .ToList();

            List<SelectListItem> type = new List<SelectListItem>();
            type.Add(new SelectListItem { Text = "Days", Value = "Days" });
            type.Add(new SelectListItem { Text = "Week", Value = "Week" });
            type.Add(new SelectListItem { Text = "Month", Value = "Month" });
            ViewBag.PType = QkSelect.List(type, "Text", "Value");

            HireType conGp = new HireType();
            conGp.Name = con.Name;
            conGp.Period = con.Period;
            conGp.PeriodType = con.PeriodType;
            conGp.Note = con.Note;
            conGp.Status = con.Status;
            return PartialView(conGp);
        }

        // POST: contact/Edit/5
        [HttpPost]
        // [QkAuthorize(Roles = "Dev,Edit HireType")]
        public JsonResult Edit(HireType con, long id)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                HireType conGp = db.HireTypes.Find(id);
                var contactExists = db.HireTypes.Any(u => u.Name == con.Name && u.HireTypeId != conGp.HireTypeId);
                if (contactExists)
                {
                    msg = "A Contact with same Name exists.";
                    stat = false;
                }
                else
                {
                    conGp.Name = con.Name;
                    conGp.Period = con.Period;
                    conGp.PeriodType = con.PeriodType;
                    conGp.Note = con.Note;
                    conGp.Status = con.Status;
                    db.Entry(conGp).State = EntityState.Modified;
                    db.SaveChanges();

                    var userid = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, userid, "HireType", "HireTypes", findip(), conGp.HireTypeId, "Hire Type Updated Successfully");


                    msg = "Successfully Updated Hire Type Details.";
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

        //  GET:  Hire Type/Delete/5
        // [QkAuthorize(Roles = "Dev,Delete HireType")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HireType congp = db.HireTypes.Find(id);
            if (congp == null)
            {
                return NotFound();
            }

            return PartialView(congp);
        }

        // POST:tax Hire Type/Delete/5
        [HttpPost]
        //   [QkAuthorize(Roles = "Dev,Delete HireType")]
        public JsonResult Delete(int id, IFormCollection collection)
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
                HireType congp = db.HireTypes.Find(id);
                db.HireTypes.Remove(congp);
                db.SaveChanges();

                var userid = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, userid, "HireType", "HireTypes", findip(), congp.HireTypeId, "Hire Type Deleted Successfully");

                stat = true;
                msg = "Successfully Deleted Hire Type details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            if (db.HireRates.Any(c => c.type == id))
            {
                msg = "Unable to delete Hire Type, Item with this Hire Type exists.";
            }
            else
            {
                msg = null;
            }
            return msg;
        }

        [HttpGet]
        //   [QkAuthorize(Roles = "Dev,Edit HireType")]
        public ActionResult ChangeStatus(string type, long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HireType material = db.HireTypes.Find(id);
            if (material == null)
            {
                return NotFound();
            }
            if (type == "active")
            {
                ViewBag.type = "Active";
                ViewBag.link = "active";
                ViewBag.status = Status.active;
            }
            else
            {
                ViewBag.type = "Inactive";
                ViewBag.link = "inactive";
                ViewBag.status = Status.inactive;
            }
            return PartialView();
        }
        // POST: master/ChangeStatus/
        [HttpPost]
        [ValidateAntiForgeryToken]
        //   [QkAuthorize(Roles = "Dev,Edit HireType")]
        public JsonResult ChangeStatus(string type, long? id, Branch brn)
        {
            bool stat = false;
            string msg;
            string types = "";
            HireType material = db.HireTypes.Find(id);
            if (brn.Status == Status.inactive)
            {
                types = " Inactive";
                material.Status = Status.inactive;
            }
            else
            {
                types = " Active";
                material.Status = Status.active;
            }

            db.Entry(material).State = EntityState.Modified;
            db.SaveChanges();

            var UserId = User.Identity.GetUserId();
            stat = true;
            msg = " Successfully Changed the Hire Type to" + types;
            com.addlog(LogTypes.Changed, UserId, "MC", "MCs", findip(), material.HireTypeId, msg);

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        public ActionResult GetHireRatebyTypeAndId(int hiretype, long item)
        {
            var hirerate = db.HireRates.Where(a => a.type == hiretype && a.ItemId == item).Select(a => a.Rate).FirstOrDefault();
            return Json(hirerate);
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
