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
using System.Collections.Generic;
using System.Linq.Dynamic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QuickSoft.ViewModel;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.IO;
using System.Net;

namespace QuickSoft.Controllers
{
    public class ServiceTypeController: BaseController
    {
        ApplicationDbContext db;
    Common com;
    public ServiceTypeController()
    {
        db = new ApplicationDbContext();
        com = new Common();
    }
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            servicetype pro = db.servicetypes.Find(id);
            if (pro == null)
            {
                return NotFound();
            }

            return PartialView(pro);
        }

        // POST: /Delete/5
        //[RedirectingAction]
        //[Authorize(Roles = "Dev,Delete ProTask")]
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            var UserId = User.Identity.GetUserId();
            var exists = db.Quotations.Where(o => o.servicetype == id).FirstOrDefault();
            if(exists==null)
            Deletesop(id);
            else
            {
                stat = false;
                msg = "Canot delete  Used In quotation Bill no : "+exists.BillNo;

                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

            stat = true;
            msg = "Successfully Deleted Task details.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }
        public void Deletesop(long id)
        {

            var v = db.servicetypes.Where(o => o.servicetypeid == id);
            db.servicetypes.RemoveRange(v);
            db.SaveChanges();
   
        }
        [HttpPost]
        public ActionResult Edit(servicetypeViewModel vn)
        {
            var vnn = db.servicetypes.Find((long)vn.id);
            long id = (long)vn.id;
            vnn.title = vn.title;
            vnn.note = vn.note;
            vnn.logtime = System.DateTime.Now;
            db.Entry(vnn).State = EntityState.Modified;
            db.SaveChanges();
  
            var userid = User.Identity.GetUserId();

            Success("updated", true);
            return RedirectToAction("Index");

        }
        public JsonResult SearchType(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.servicetypes.Where(p => p.title.ToLower().Contains(q.ToLower()) || p.title.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.title, //each json object will have 
                                      id = b.servicetypeid
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.servicetypes.Select(b => new SelectFormat
                {
                    text = b.title, //each json object will have 
                    id = b.servicetypeid
                }).OrderBy(b => b.text).ToList();

            }//          

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }


        // [Authorize(Roles = "Dev,Edit SOP")]
        public ActionResult Edit(long id)
        {
            servicetypeViewModel vmodel = new servicetypeViewModel();
            var v = db.servicetypes.Find(id);
            vmodel.note = v.note;
            vmodel.title = v.title;
            vmodel.id = id;

         
            return View(vmodel);
        }
        //  [Authorize(Roles = "Dev,Create SOP")]
        public ActionResult create()
        {
            servicetypeViewModel vmodel = new servicetypeViewModel();
            List<SelectFormat> serialisedJson;
          

            return View(vmodel);
        }
 
        [RedirectingAction]
        [HttpPost]
        public ActionResult GetDetails(long? AmcId, string PDate)
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

            var UserId = User.Identity.GetUserId();


            var v = (from a in db.servicetypes
                    
                     select new
                     {
                         a.servicetypeid,
                         a.title,
                         ldate = a.logtime,
                        
                     }).OrderByDescending(a => a.ldate).ToList();
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(o => o.title.ToUpper().Contains(search.ToUpper())).ToList();
            }
            //SORT

            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

       

        [HttpPost]
        public ActionResult create(servicetypeViewModel vm)
        {
            servicetype s = new servicetype
            {
                note = vm.note,
                title = vm.title,
                logtime = System.DateTime.Now

            };
            db.servicetypes.Add(s);
            db.SaveChanges();
            var sopid = s.servicetypeid;
            var userid = User.Identity.GetUserId();
         
            //seleted date added,for fullcalender



           


            Success("success", true);
            return RedirectToAction("create");
        }
        //   [Authorize(Roles = "Dev,List SOP")]
        public ActionResult Index()
        {
            return View();
        }
    }
}
