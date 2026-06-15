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
    public class UnitFeatureController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public UnitFeatureController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Property/PropertyFeature
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, DocumentType")]                //UnitFeature
        public ActionResult GetUnitFeature()
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

            var UserView = (from a in db.PropertyUnitFeatures
                            select new
                            {
                                id = a.ID,
                                a.Feature,
                            });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(p => p.Feature.ToString().ToLower().Contains(search.ToLower()));
            }
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                UserView = UserView.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = UserView.Count();
            var data = UserView.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        public ActionResult Create()
        {
            return PartialView();
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, Create PayrollUnits")]
        public JsonResult Create(PropertyUnitFeature vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.PropertyUnitFeatures.Any(c => c.Feature == vmodel.Feature);
                if (Exists)
                {
                    msg = "Type Name already exists.";
                    stat = false;
                }
                else
                {
                    var proptype = new PropertyUnitFeature
                    {
                        Feature = vmodel.Feature,
                    };
                    db.PropertyUnitFeatures.Add(proptype);
                    db.SaveChanges();
                    Int64 ID = proptype.ID;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "UnitFeature", "UnitFeatures", findip(), ID, "Unit Feature Added Successfully");
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
            PropertyUnitFeature protyp = db.PropertyUnitFeatures.Find(id);

            if (protyp == null)
            {
                return NotFound();
            }

            PropertyUnitFeature vmodel = new PropertyUnitFeature();

            vmodel.ID = (long)id;
            vmodel.Feature = protyp.Feature;

            return PartialView(vmodel);
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, Create DocumentType")]
        public JsonResult Update(PropertyUnitFeature vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.PropertyUnitFeatures.Any(c => c.Feature == vmodel.Feature && c.ID != vmodel.ID);
                if (Exists)
                {
                    msg = "Type Name already exists.";
                    stat = false;
                }
                else
                {
                    PropertyUnitFeature protyp = db.PropertyUnitFeatures.Find(vmodel.ID);

                    protyp.Feature = vmodel.Feature;
                    db.Entry(protyp).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "UnitFeature", "UnitFeatures", findip(), vmodel.ID, "Unit Feature Updated Successfully");
                    msg = "Successfully Updated Unit Feature details.";
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
            PropertyUnitFeature ptype = db.PropertyUnitFeatures.Find(id);
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
                msg = "Successfully Deleted Unit Feature details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public bool DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            PropertyUnitFeature pt = db.PropertyUnitFeatures.Find(id);

            db.PropertyUnitFeatures.Remove(pt);
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "UnitFeature", "UnitFeatures", findip(), pt.ID, "Unit Feature Deleted Successfully");
            return true;
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;

            return msg;
        }

        public JsonResult SearchFeature(string q, string x)
        {
            var UserId = User.Identity.GetUserId();
            List<SelectFormat> serialisedJson;
            //string stt = "Individual";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.PropertyUnitFeatures
                                  where b.Feature.ToLower().Contains(q.ToLower()) || b.Feature.Contains(q)
                                  select new SelectFormat
                                  {
                                      text = b.Feature,
                                      id = b.ID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.PropertyUnitFeatures.Select(b => new SelectFormat
                {
                    text = b.Feature,
                    id = b.ID
                }).OrderBy(b => b.text).ToList();

            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Feature" };
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