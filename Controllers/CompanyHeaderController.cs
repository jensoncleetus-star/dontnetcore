using System.Linq.Dynamic.Core;
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.Models;
using System.IO;
using Microsoft.AspNetCore.Identity;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class CompanyHeaderController : BaseController
    {
        // GET: CompanyHeader
        ApplicationDbContext db;
        Common com;
        public CompanyHeaderController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET:
        [HttpGet] 
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public JsonResult GetCompanyHeader()
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
            var v = db.CompanyHeaders.Select(b => new
            {
                b.CompanyHeaderID,
                //b.Name,
                b.Header,
                b.Footer
            });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.Header.ToString().ToLower().Contains(search.ToLower()));
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
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CompanyHeader CompanyHder, IFormFile Header, IFormFile Footer)
        {
            if (ModelState.IsValid)
            {
                var UserId = User.Identity.GetUserId();
                var CompanyHeaderDetails = new CompanyHeader
                {
                    //Name = CompanyHder.Name,
                    Header = Path.GetFileName(Header.FileName),
                    Footer = Path.GetFileName(Footer.FileName),
                };
                db.CompanyHeaders.Add(CompanyHeaderDetails);
                db.SaveChanges();
                Int64 HeadID = CompanyHeaderDetails.CompanyHeaderID;

                com.addlog(LogTypes.Created, UserId, "CompanyHeader", "CompanyHeaders", findip(), CompanyHeaderDetails.CompanyHeaderID, "Company Header Details Added Successfully");


                Success("Successfully Added Company Header Details .", true);
                return RedirectToAction("Create", "CompanyHeader");
            }
            else
            {
                Warning("Looks like something went wrong. Please check your form.", true);

            }

            return RedirectToAction("Create", "CompanyHeader");
        }
        // GET: CompanyHeader/Edit
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CompanyHeader CompanyHeaderinformation = db.CompanyHeaders.Find(id);

            if (CompanyHeaderinformation == null)
            {
                return NotFound();
            }
            return View(CompanyHeaderinformation);
        }

        // POST: CompanyHeader/Edit
        [HttpPost]
        public ActionResult Edit(long id, CompanyHeader CompanyH)
        {
            if (ModelState.IsValid)
            {
                var UserId = User.Identity.GetUserId();
                CompanyHeader CompanyHead = db.CompanyHeaders.Find(id);
                CompanyHead.Header = CompanyH.Header;
                CompanyHead.Footer = CompanyH.Footer;
                db.Entry(CompanyHead).State = EntityState.Modified;
                db.SaveChanges();

                com.addlog(LogTypes.Updated, UserId, "CompanyHeader", "CompanyHeaders", findip(), CompanyHead.CompanyHeaderID, "Company Header Details Updated Successfully");


                Success("Successfully Updated Company Header Details .", true);
                return RedirectToAction("Index", "CompanyHeader");
            }
            else
            {
                Warning("Looks like something went wrong. Please check your form.", true);
            }
            return View();
        }
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CompanyHeader CompanyHeaderInfo = db.CompanyHeaders.Find(id);
            if (CompanyHeaderInfo == null)
            {
                return NotFound();
            }

            return View(CompanyHeaderInfo);
        }

        // POST: CompanyHeader/Delete
        [HttpPost]
        public ActionResult Delete(long id, IFormCollection collection)
        {

            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            CompanyHeader CompanyHeaderInfom = db.CompanyHeaders.Find(id);
            db.CompanyHeaders.Remove(CompanyHeaderInfom);
            db.SaveChanges();


            com.addlog(LogTypes.Deleted, UserId, "CompanyHeader", "CompanyHeaders", findip(), CompanyHeaderInfom.CompanyHeaderID, "Company Header Details Deleted Successfully");


            stat = true;
            msg = "Successfully Deleted Company Header details.";
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
