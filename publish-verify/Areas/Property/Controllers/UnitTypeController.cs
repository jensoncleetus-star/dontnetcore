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
    public class UnitTypeController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public UnitTypeController()
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
        //[QkAuthorize(Roles = "Dev, UnitType")]
        public ActionResult GetUnitType()
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

            var UserView = (from a in db.PropertyUnitTypes
                            select new
                            {
                                id = a.ID,
                                a.Name,
                            });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(p => p.Name.ToString().ToLower().Contains(search.ToLower()));
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
        public JsonResult Create(PropertyUnitType vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.PropertyUnitTypes.Any(c => c.Name == vmodel.Name);
                if (Exists)
                {
                    msg = "Type Name already exists.";
                    stat = false;
                }
                else
                {
                    var proptype = new PropertyUnitType
                    {
                        Name = vmodel.Name,
                    };
                    db.PropertyUnitTypes.Add(proptype);
                    db.SaveChanges();
                    Int64 ID = proptype.ID;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "UnitType", "UnitTypes", findip(), ID, "Unit Type Added Successfully");
                    msg = "Successfully added Unit Type details.";
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
            PropertyUnitType protyp = db.PropertyUnitTypes.Find(id);

            if (protyp == null)
            {
                return NotFound();
            }

            PropertyUnitType vmodel = new PropertyUnitType();

            vmodel.ID = (long)id;
            vmodel.Name = protyp.Name;


            return PartialView(vmodel);
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, Create UnitType")]
        public JsonResult Update(PropertyUnitType vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.PropertyUnitTypes.Any(c => c.Name == vmodel.Name && c.ID != vmodel.ID);
                if (Exists)
                {
                    msg = "Type Name already exists.";
                    stat = false;
                }
                else
                {
                    PropertyUnitType protyp = db.PropertyUnitTypes.Find(vmodel.ID);

                    protyp.Name = vmodel.Name;
                    db.Entry(protyp).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "UnitType", "UnitTypes", findip(), vmodel.ID, "Unit Type Updated Successfully");
                    msg = "Successfully Updated Unit Type details.";
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
        //[Authorize(Roles = "Dev,Delete UnitType")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PropertyUnitType ptype = db.PropertyUnitTypes.Find(id);
            if (ptype == null)
            {
                return NotFound();
            }
            return PartialView(ptype);
        }
        [RedirectingAction]
        //[Authorize(Roles = "Dev,Delete UnitType")]
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
                msg = "Successfully Deleted Unit Type details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public bool DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            PropertyUnitType pt = db.PropertyUnitTypes.Find(id);

            db.PropertyUnitTypes.Remove(pt);
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "UnitType", "UnitTypes", findip(), pt.ID, "Unit Type Deleted Successfully");
            return true;
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;

            return msg;
        }

        public JsonResult SearchUnitType(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.PropertyUnitTypes
                                  where a.Name.ToLower().Contains(q.ToLower()) || a.Name.Contains(q)
                                  select new SelectFormat
                                  {
                                      text = a.Name,
                                      id = a.ID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.PropertyUnitTypes.Select(b => new SelectFormat
                {
                    text = b.Name,
                    id = b.ID
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Unit Type" };
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