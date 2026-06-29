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
using System;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace QuickSoft.Controllers
{
    public class AMCContractNameController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public AMCContractNameController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: AMCContractName
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetContractName(string ColName)
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
            var uEdit = User.IsInRole("Edit Contract Name");
            var uDelete = User.IsInRole("Delete Contract Name");

            var v = (from a in db.AmcContracts
                     where (ColName == "" || a.ContractName == ColName)
                     select new
                     {
                         a.ContractId,
                         a.ContractName,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete
                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.ContractName.ToString().ToLower().Contains(search.ToLower()));
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
        public JsonResult getContractName(string q, string x, string page)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            var start = Convert.ToInt32(page);
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.AmcContracts.Where(p => p.ContractName.ToLower().Contains(q.ToLower()))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.ContractName,
                                      id = b.ContractId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.AmcContracts
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.ContractName,
                                      id = b.ContractId
                                  }).OrderBy(b => b.text).ToList();

            }
            if (x == "all" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Contract Name" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public ActionResult Create()
        {
            return PartialView();
        }

        // POST: Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Create LeadType")]
        public JsonResult Create([Bind("ContractId,ContractName")] AmcContract Contract)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid)
            {
                var Exists = db.AmcContracts.Any(c => c.ContractName == Contract.ContractName);
                if (Exists)
                {
                    msg = "Details already exists.";
                    stat = false;
                }
                else
                {
                    db.AmcContracts.Add(Contract);
                    db.SaveChanges();
                    Id = Contract.ContractId;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "AMCContractName", "AmcContracts", findip(), Contract.ContractId, "Contract Name Added Successfully");

                    msg = "Contract Name added successfully.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }

        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AmcContract Contract = db.AmcContracts.Find(id);
            if (Contract == null)
            {
                return NotFound();
            }
            return PartialView(Contract);
        }



        // POST: AMCContractName/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public JsonResult Edit([Bind("ContractId,ContractName")] AmcContract Contract)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {
                var Exists = db.AmcContracts.Any(c => c.ContractName == Contract.ContractName && c.ContractId != Contract.ContractId);
                if (Exists)
                {
                    msg = "Contract Name already exists.";
                    stat = false;
                }
                else
                {
                    db.Entry(Contract).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "AMCContractName", "AmcContracts", findip(), Contract.ContractId, "Contract Updated Successfully");


                    msg = "Successfully updated Contract Name details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AmcContract Contract = db.AmcContracts.Find(id);
            if (Contract == null)
            {
                return NotFound();
            }
            return PartialView(Contract);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var Exists = db.Amcs.Any(c => c.ContractId == id);
            if (Exists)
            {
                msg = "Unable to delete Contract Name, AMC with this Contract Name exists.";
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Successfully deleted Contract Name.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;

            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Contract Names, Unable to Delete " + notdel + " Contract. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + "Contract Name.", true);
            }
            else
            {
                Success("Deleted " + count + " Contract Names.", true);
            }
            return RedirectToAction("Index", "AMCContractName");
        }


        public bool DeleteFn(long id)
        {
            AmcContract Contract = db.AmcContracts.Find(id);
            if (Contract != null)
            {
                db.AmcContracts.RemoveRange(db.AmcContracts.Where(a => a.ContractId == id));


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "AMCContractName", "AmcContracts", findip(), Contract.ContractId, "Contract Name Deleted Successfully");
                db.SaveChanges();
            }
            return true;
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
   