using QuickSoft.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using QuickSoft.Controllers;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Areas.Property.Controllers
{
    [Microsoft.AspNetCore.Mvc.Area("Property")]
    public class AdditionalFieldController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public AdditionalFieldController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Property/AdditionalField
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, DocumentType")]
        public ActionResult GetAdditionalField()
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

            var UserView = (from a in db.AdditionalFields
                            select new
                            {
                                id = a.ID,
                                a.Name,
                                a.Section
                            });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(p => p.Name.ToString().ToLower().Contains(search.ToLower()));
            }
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                try { UserView = UserView.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir); } catch { /* grid column name not in projection - keep default order */ }
            }
            recordsTotal = UserView.Count();
            var data = UserView.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        public ActionResult Create()
        {
            ViewBag.Sect = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Property", Value="Property"},
                new SelectListItem() {Text = "Rental", Value="Rental"},
                 new SelectListItem() {Text = "Property Registration", Value="Property Registration"},
                 new SelectListItem() {Text = "Rental Proforma", Value="Rental Proforma"},
                  new SelectListItem() {Text = "Maintenance", Value="Maintenance"},
            }, "Value", "Text");
            return PartialView();
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, Create PayrollUnits")]
        public JsonResult Create(AdditionalField vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.AdditionalFields.Any(c => c.Name == vmodel.Name);
                if (Exists)
                {
                    msg = "Type Name already exists.";
                    stat = false;
                }
                else
                {
                    var proptype = new AdditionalField
                    {
                        Name = vmodel.Name,
                        Section=vmodel.Section
                    };
                    db.AdditionalFields.Add(proptype);
                    db.SaveChanges();
                    Int64 ID = proptype.ID;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "AdditionalField", "AdditionalFields", findip(), ID, "Property Feature Added Successfully");
                    msg = "Successfully added Document Type details.";
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
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AdditionalField protyp = db.AdditionalFields.Find(id);

            if (protyp == null)
            {
                return NotFound();
            }

            AdditionalField vmodel = new AdditionalField();

            vmodel.ID = (long)id;
            vmodel.Name = protyp.Name;
            vmodel.Section = protyp.Section;
            ViewBag.Sect = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Property", Value="Property"},
                new SelectListItem() {Text = "Rental", Value="Rental"},
                 new SelectListItem() {Text = "Property Registration", Value="Property Registration"},
                 new SelectListItem() {Text = "Rental Proforma", Value="Rental Proforma"},
                  new SelectListItem() {Text = "Maintenance", Value="Maintenance"},
            }, "Value", "Text");
            return PartialView(vmodel);
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, Create DocumentType")]
        public JsonResult Update(AdditionalField vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.AdditionalFields.Any(c => c.Name == vmodel.Name && c.ID != vmodel.ID);
                if (Exists)
                {
                    msg = "Type Name already exists.";
                    stat = false;
                }
                else
                {
                    AdditionalField protyp = db.AdditionalFields.Find(vmodel.ID);

                    protyp.Name = vmodel.Name;
                    protyp.Section = vmodel.Section;
                    db.Entry(protyp).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "AdditionalField", "AdditionalFields", findip(), vmodel.ID, "Property Feature Updated Successfully");
                    msg = "Successfully Updated Property Feature details.";
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

        [RedirectingAction]
        //[Authorize(Roles = "Dev,Delete DocumentType")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AdditionalField ptype = db.AdditionalFields.Find(id);
            if (ptype == null)
            {
                return NotFound();
            }
            return PartialView(ptype);
        }
        [RedirectingAction]
        //[Authorize(Roles = "Dev,Delete DocumentType")]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
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
                stat = DeleteFn(id);
                msg = "Successfully Deleted Property Feature details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public bool DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            AdditionalField pt = db.AdditionalFields.Find(id);

            db.AdditionalFields.Remove(pt);
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "AdditionalField", "AdditionalFields", findip(), pt.ID, "Property Feature Deleted Successfully");
            return true;
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;

            return msg;
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