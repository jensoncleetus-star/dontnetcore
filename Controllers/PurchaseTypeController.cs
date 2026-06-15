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
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class PurchaseTypeController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PurchaseTypeController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: PurchaseType
        [QkAuthorize(Roles = "Dev,PurchaseType List")]
        public ActionResult Index()
        {
            return View();
        }
        
        [QkAuthorize(Roles = "Dev,PurchaseType List")]
        public JsonResult getData()
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

            var v = (from a in db.PurchaseTypes
                     select a).AsEnumerable().Select(a => new
                     {
                         a.Id,
                         a.Name,
                         a.TaxType,
                         a.editable,
                         a.CreatedDate,
                         a.CreatedBy,
                         a.Status,
                         TaskTypeName = Enum.GetName(typeof(TaxType), a.TaxType)

                     });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.Name.ToString().ToLower().Contains(search.ToLower()));
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
        [QkAuthorize(Roles = "Dev,Create PurchaseType")]
        public ActionResult Create()
        {
            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            return PartialView();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create PurchaseType")]
        [ValidateAntiForgeryToken]
        public JsonResult Create(PurchaseType ptview)
        {
            bool stat = false;
            string msg;

            var UserId = User.Identity.GetUserId();
            var Exists = db.PurchaseTypes.Any(c => c.Name == ptview.Name && c.CreatedBy == UserId);
            if (Exists)
            {
                msg = "Name already Exists! Please Use Another Name.";
                stat = false;
            }
            else
            {

                long Branch = 0;

                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                if (BranchCheck == Status.active)
                {
                    Branch = ptview.Branch;
                }
                else
                {
                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                }
                var op = new PurchaseType
                {
                    Name = ptview.Name,
                    Status = ptview.Status,
                    CreatedBy = UserId,
                    editable = choice.Yes,
                    TaxType = ptview.TaxType,
                    CreatedDate = Convert.ToDateTime(System.DateTime.Now),
                    Branch = Branch,
                };
                db.PurchaseTypes.Add(op);
                db.SaveChanges();


                com.addlog(LogTypes.Created, UserId, "PurchaseType", "PurchaseTypes", findip(), op.Id, "PurchaseType Created Successfully");
                msg = "Purchase Type Created.";
                stat = true;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //        Name = ptview.Name,
        //        Status = ptview.Status,
        //        CreatedBy = UserId,
        //        editable = choice.Yes,
        //        TaxType = ptview.TaxType,
        //        CreatedDate = Convert.ToDateTime(System.DateTime.Now)

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Edit PurchaseType")]
        public ActionResult Edit(long? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseType pt = db.PurchaseTypes.Find(Id);
            if (pt == null)
            {
                return NotFound();
            }

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");


            return PartialView(pt);
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit PurchaseType")]
        public JsonResult Edit(long Id, PurchaseType ptview)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            var Exists = db.PurchaseTypes.Any(c => c.Name == ptview.Name && c.CreatedBy == UserId && c.Id != Id);
            if (Exists)
            {
                msg = "Name already Exists! Please Use Another Name.";
                stat = false;
            }
            else
            {
                long Branch = 0;

                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                if (BranchCheck == Status.active)
                {
                    Branch = ptview.Branch;
                }
                else
                {
                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                }
                PurchaseType pt = db.PurchaseTypes.Find(Id);
                pt.Name = ptview.Name;
                pt.TaxType = ptview.TaxType;
                pt.Status = ptview.Status;
                pt.Branch = Branch;
                db.SaveChanges();

                com.addlog(LogTypes.Updated, UserId, "PurchaseType", "PurchaseTypes", findip(), ptview.Id, "PurchaseType Updated Successfully");
                msg = "Successfully Updated PurchaseType Details.";
                stat = true;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        [HttpGet]
        [QkAuthorize(Roles = "Dev,Delete PurchaseType")]
        public ActionResult Delete(long? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseType Info = db.PurchaseTypes.Find(Id);
            if (Info == null)
            {
                return NotFound();
            }
            return PartialView(Info);
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete PurchaseType")]
        public JsonResult Delete(long? Id, IFormCollection collection)
        {
            bool stat = false;
            string msg;

            PurchaseType Info = db.PurchaseTypes.Find(Id);
            db.PurchaseTypes.Remove(Info);
            db.SaveChanges();

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "PurchaseType", "PurchaseTypes", findip(), (long)Info.Id, "PurchaseType Deleted Successfully");
            stat = true;
            msg = "Successfully Deleted PurchaseType details.";
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
