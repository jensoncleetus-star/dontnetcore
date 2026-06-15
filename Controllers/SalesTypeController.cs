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
    public class SalesTypeController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public SalesTypeController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: SalesType
        //[QkAuthorize(Roles = "Dev,PurchaseType List")]
        public ActionResult Index()
        {
            return View();
        }

        //[QkAuthorize(Roles = "Dev,PurchaseType List")]
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

            var v = (from a in db.SalesTypes
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
        //[QkAuthorize(Roles = "Dev,Create PurchaseType")]
        public ActionResult Create()
        {
            return PartialView();
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Create PurchaseType")]
        //[ValidateAntiForgeryToken]
        public JsonResult Create(SalesType stview)
        {
            bool stat = false;
            string msg;

            var UserId = User.Identity.GetUserId();
            var Exists = db.SalesTypes.Any(c => c.Name == stview.Name && c.CreatedBy == UserId);
            if (Exists)
            {
                msg = "Name already Exists! Please Use Another Name.";
                stat = false;
            }
            else
            {
                var op = new SalesType
                {
                    Name = stview.Name,
                    Status = stview.Status,
                    CreatedBy = UserId,
                    editable = choice.Yes,
                    TaxType = stview.TaxType,
                    CreatedDate = Convert.ToDateTime(System.DateTime.Now)
                };
                db.SalesTypes.Add(op);
                db.SaveChanges();


                com.addlog(LogTypes.Created, UserId, "SalesType", "SalesTypes", findip(), op.Id, "SaleType Created Successfully");
                msg = "Sale Type Created.";
                stat = true;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }        

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,Edit PurchaseType")]
        public ActionResult Edit(long? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SalesType st = db.SalesTypes.Find(Id);
            if (st == null)
            {
                return NotFound();
            }
            return PartialView(st);
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Edit PurchaseType")]
        public JsonResult Edit(long Id, SalesType stview)
        {
            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            var Exists = db.SalesTypes.Any(c => c.Name == stview.Name && c.CreatedBy == UserId && c.Id != Id);
            if (Exists)
            {
                msg = "Name already Exists! Please Use Another Name.";
                stat = false;
            }
            else
            {
                SalesType st = db.SalesTypes.Find(Id);
                st.Name = stview.Name;
                st.TaxType = stview.TaxType;
                st.Status = stview.Status;
                db.SaveChanges();

                com.addlog(LogTypes.Updated, UserId, "SalesType", "SalesTypes", findip(), stview.Id, "SalesType Updated Successfully");
                msg = "Successfully Updated SalesType Details.";
                stat = true;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        [HttpGet]
        //[QkAuthorize(Roles = "Dev,Delete PurchaseType")]
        public ActionResult Delete(long? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SalesType Info = db.SalesTypes.Find(Id);
            if (Info == null)
            {
                return NotFound();
            }
            return PartialView(Info);
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Delete PurchaseType")]
        public JsonResult Delete(long? Id, IFormCollection collection)
        {
            bool stat = false;
            string msg;

            SalesType Info = db.SalesTypes.Find(Id);
            db.SalesTypes.Remove(Info);
            db.SaveChanges();

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "SalesType", "SalesTypes", findip(), (long)Info.Id, "SalesTypes Deleted Successfully");
            stat = true;
            msg = "Successfully Deleted SalesTypes details.";
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