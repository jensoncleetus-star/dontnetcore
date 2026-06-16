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
    public class PropertySettingsController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PropertySettingsController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Property/PropertySettings
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev, PropertyType")]
        public ActionResult GetPropertySettings()
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

            var UserView = (from a in db.PropertySettingss
                            select new
                            {
                                id = a.Id,
                                a.Module,
                                a.Type,
                                SValue=a.SValue,
                                LValue=a.LValue,
                                a.Description,
                                a.Status
                            });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(p => p.Module.ToString().ToLower().Contains(search.ToLower()));
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
            ViewBag.Proptype = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "landlord Account Group", Value="1"},
                new SelectListItem() {Text = "Tenant Account Group", Value="2"},
                new SelectListItem() {Text = "Developer", Value="3"},
                new SelectListItem() {Text = "Broker", Value="4"},
            }, "Value", "Text");
            return View();
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, Create PayrollUnits")]
        public JsonResult Create(PropertySettings vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.PropertySettingss.Any(c => c.Module == vmodel.Module);
                if (Exists)
                {
                    msg = "Module Name already exists.";
                    stat = false;
                }
                else
                {
                    var prop = new PropertySettings
                    {
                        Module = vmodel.Module,
                        LValue=vmodel.LValue,
                        SValue=vmodel.SValue,
                        Status=vmodel.Status,
                        Type=vmodel.Type,
                        Description=vmodel.Description,
                        
                    };
                    db.PropertySettingss.Add(prop);
                    db.SaveChanges();
                    Int64 ID = prop.Id;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "PropertySettings", "PropertySettingss", findip(), ID, "Property Settings Added Successfully");
                    msg = "Successfully added Property Settings details.";
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
            PropertySettings protyp = db.PropertySettingss.Find(id);

            if (protyp == null)
            {
                return NotFound();
            }

            PropertySettings vmodel = new PropertySettings();

            vmodel.Id = (long)id;
            vmodel.Module = protyp.Module;
            vmodel.Type = protyp.Type;
            vmodel.SValue = protyp.SValue;
            vmodel.LValue = protyp.LValue;
            vmodel.Description = protyp.Description;
            vmodel.Status = protyp.Status;

            ViewBag.Proptype = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "landlord Account Group", Value="1"},
                new SelectListItem() {Text = "Tenant Account Group", Value="2"},
                new SelectListItem() {Text = "Developer", Value="3"},
                new SelectListItem() {Text = "Broker", Value="4"},
            }, "Value", "Text");

            return View(vmodel);
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, Create PropertyType")]
        public JsonResult Update(PropertySettings vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.PropertySettingss.Any(c => c.Module == vmodel.Module && c.Id!=vmodel.Id);
                if (Exists)
                {
                    msg = "Type Name already exists.";
                    stat = false;
                }
                else
                {
                    PropertySettings prop = db.PropertySettingss.Find(vmodel.Id);

                    prop.Module = vmodel.Module;
                    prop.LValue = vmodel.LValue;
                    prop.SValue = vmodel.SValue;
                    prop.Type = vmodel.Type;
                    prop.Description = vmodel.Description;
                    prop.Status = vmodel.Status;

                    db.Entry(prop).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "PropertySettings", "PropertySettingss", findip(), vmodel.Id, "Property Settings Updated Successfully");
                    msg = "Successfully Updated Property Settings details.";
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
        //[Authorize(Roles = "Dev,Delete PropertyType")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PropertySettings ptype = db.PropertySettingss.Find(id);
            if (ptype == null)
            {
                return NotFound();
            }
            return PartialView(ptype);
        }
        [RedirectingAction]
        //[Authorize(Roles = "Dev,Delete PropertyType")]
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
                msg = "Successfully Deleted Property Settings details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public bool DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            PropertySettings pt = db.PropertySettingss.Find(id);

            db.PropertySettingss.Remove(pt);
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "PropertySettings", "PropertySettingss", findip(), pt.Id, "Property Settings Deleted Successfully");
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