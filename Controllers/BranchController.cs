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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.Models;
using Microsoft.AspNetCore.Identity;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class BranchController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public BranchController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Branch
        [QkAuthorize(Roles = "Dev,Branch List")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetBranch()
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
            var v = db.Branchs.Select(b => new
            {
                b.BranchID,
                b.BranchCode,
                b.BranchName,
                DisAddress = b.Address + "<br/>" + b.City + " " + b.State + " " + b.Country + "<br/>" + b.ZipCode,
                b.LandLineNumber,
                b.MobileNumber,
                b.EmailId,
                b.Status,
                b.MainBranch,
                b.Editable
            });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.BranchCode.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.BranchName.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.DisAddress.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.LandLineNumber.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.MobileNumber.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.EmailId.ToString().ToLower().Contains(search.ToLower()));
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
        public JsonResult GetBranchById(int CId)
        {
            var v = (from a in db.Branchs
                     where a.BranchID == CId
                     select new
                     {
                         a.BranchID,
                         a.BranchName,
                     }).FirstOrDefault();
            return Json(v);
        }
        [QkAuthorize(Roles = "Dev,Create Branch")]
        public ActionResult Create()
        {
            Branch brn = new Branch();
            brn.BranchCode = BrCode();
            return View(brn);
        }
        private string BrCode(Int64 BNo = 0, string BCode = null)
        {
            var prefix = db.CodePrefixs.Where(a => a.section == "Branch").Select(a => a.prefix).FirstOrDefault();
            if (BCode == null)
            {
                if ((db.Branchs.Select(p => p.BranchID).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    BCode = prefix + 1;
                }
                else
                {
                    BNo = db.Branchs.Max(p => p.BranchID + 1);
                    BCode = prefix + BNo;
                    if (CodeExist(BCode))
                    {
                        BCode = BrCode(BNo, BCode);
                    }

                }
            }
            else
            {
                BNo = BNo + 1;
                BCode = prefix + BNo;
                if (CodeExist(BCode))
                {
                    BCode = BrCode(BNo, BCode);
                }
            }
            return BCode;
        }
        private bool CodeExist(string Code)
        {
            var Exists = db.Branchs.Any(c => c.BranchCode == Code);
            bool res = (Exists) ? true : false;
            return res;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Create Branch")]
        public ActionResult Create(Branch branch)
        {
            var BranchExists = db.Branchs.Any(u => u.BranchCode == branch.BranchCode);
            if (BranchExists)
            {
                Danger("A Branch with same Branch Code exists.", true);
                return RedirectToAction("Create", "Branch");
            }
            else
            {
                var UserId = User.Identity.GetUserId();
                if (ModelState.IsValid)
                {
                    Branch bran = new Branch();
                    bran.BranchCode = branch.BranchCode;
                    bran.BranchName = branch.BranchName;
                    bran.Address = branch.Address;
                    bran.Country = branch.Country;
                    bran.State = branch.State;
                    bran.City = branch.City;
                    bran.ZipCode = branch.ZipCode;
                    bran.LandLineNumber = branch.LandLineNumber;
                    bran.MobileNumber = branch.MobileNumber;
                    bran.EmailId = branch.EmailId;
                    bran.Status = Status.active;
                    if (branch.MainBranch)
                    {
                        db.Branchs.ToList().ForEach(x => x.MainBranch = false);
                    }
                    bran.MainBranch = branch.MainBranch;
                    db.Branchs.Add(bran);
                    db.SaveChanges();

                    com.addlog(LogTypes.Created, UserId, "Branch", "Branchs", findip(), bran.BranchID, "Branch Added Successfully");


                    Success("Successfully added branch details.", true);
                    return RedirectToAction("Create", "Branch");
                }
                else
                {
                    Warning("Looks like something went wrong. Please check your form.", true);
                    return (View());
                }
            }
        }

        [QkAuthorize(Roles = "Dev,Edit Branch")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Branch branch = db.Branchs.Find(id);
            if (branch == null)
            {
                return NotFound();
            }
            return PartialView(branch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Edit Branch")]
        public JsonResult Edit(Branch branch, Int64 id)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var UserId = User.Identity.GetUserId();
                Branch bran = db.Branchs.Find(id);

                bran.BranchName = branch.BranchName;
                bran.Address = branch.Address;
                bran.Country = branch.Country;
                bran.State = branch.State;
                bran.City = branch.City;
                bran.ZipCode = branch.ZipCode;
                bran.LandLineNumber = branch.LandLineNumber;
                bran.MobileNumber = branch.MobileNumber;
                bran.EmailId = branch.EmailId;
                if (branch.MainBranch)
                {
                    db.Branchs.ToList().ForEach(x => x.MainBranch = false);
                }
                bran.MainBranch = branch.MainBranch;

                db.Entry(bran).State = EntityState.Modified;
                db.SaveChanges();

                com.addlog(LogTypes.Updated, UserId, "Branch", "Branchs", findip(), bran.BranchID, "Branch Updated Successfully");


                msg = "Successfully updated Branch details.";
                stat = true;
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }

        [QkAuthorize(Roles = "Dev,Delete Branch")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Branch branch = db.Branchs.Find(id);
            if (branch == null)
            {
                return NotFound();
            }
            return PartialView(branch);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete Branch")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var Exists = db.Employees.Any(c => c.BranchID == id);
            if (Exists)
            {
                msg = "Unable to delete Branch, Branch is already used.";
                stat = false;
            }
            else
            {
                var UserId = User.Identity.GetUserId();
                Branch branch = db.Branchs.Find(id);
                db.Branchs.Remove(branch);
                db.SaveChanges();

                com.addlog(LogTypes.Deleted, UserId, "Branch", "Branchs", findip(), branch.BranchID, "Branch Deleted Successfully");


                stat = true;
                msg = "Successfully deleted branch details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        [HttpGet]
        [QkAuthorize(Roles = "Dev,Branch List")]
        public ActionResult ChangeStatus(string type, long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Branch branch = db.Branchs.Find(id);
            if (branch == null)
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
        [QkAuthorize(Roles = "Dev,Branch List")]
        public JsonResult ChangeStatus(string type, long? id, Branch brn)
        {
            bool stat = false;
            string msg;
            string types = "";
            Branch branch = db.Branchs.Find(id);
            if (brn.Status == Status.inactive)
            {
                types = " Inactive";
                branch.Status = Status.inactive;
            }
            else
            {
                types = " Active";
                branch.Status = Status.active;
            }

            db.Entry(branch).State = EntityState.Modified;
            db.SaveChanges();

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Changed, UserId, "Branch", "Branchs", findip(), branch.BranchID, "Successfully Changed the Branch to" + types);


            stat = true;
            msg = " Successfully Changed the Branch to" + types;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public JsonResult SearchBranch(string q, string x)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Branchs.Where(p => p.BranchCode.ToLower().Contains(q.ToLower()) || p.BranchName.ToLower().Contains(q.ToLower()) || p.BranchCode.Contains(q) || p.BranchName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.BranchCode + "-" + b.BranchName, //each json object will have 
                                      id = b.BranchID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Branchs.Select(b => new SelectFormat
                {
                    text = b.BranchCode + "-" + b.BranchName, //each json object will have 
                    id = b.BranchID
                }).OrderBy(b => b.text).ToList();

            }//
            return Json(serialisedJson);
        }



        //[HttpGet]

        //// POST: Customer/ChangeStatus/
        //[HttpPost, ActionName("ChangeStatusActive")]
        //[ValidateAntiForgeryToken]




        //    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        ////GET: /Change Status Inactive
        //[HttpGet]

        //// POST: Customer/ChangeStatus/
        //[HttpPost, ActionName("ChangeStatusInActive")]
        //[ValidateAntiForgeryToken]




        //    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        [QkAuthorize(Roles = "Dev,Create Branch")]
        public ActionResult AddBranch()
        {
            Branch brn = new Branch();
            brn.BranchCode = BrCode();
            return PartialView(brn);
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create Branch")]
        public ActionResult AddBranch(Branch branch)
        {
            var BranchExists = db.Branchs.Any(u => u.BranchCode == branch.BranchCode);
            bool stat = false;
            string msg;
            if (BranchExists)
            {
                msg = "A Branch with same Branch Code exists..";
                stat = false;
            }
            else
            {
                var UserId = User.Identity.GetUserId();
                if (ModelState.IsValid)
                {
                    var bran = new Branch
                    {
                        BranchCode = branch.BranchCode,
                        BranchName = branch.BranchName,
                        Address = branch.Address,
                        Country = branch.Country,
                        State = branch.State,
                        City = branch.City,
                        ZipCode = branch.ZipCode,
                        LandLineNumber = branch.LandLineNumber,
                        MobileNumber = branch.MobileNumber,
                        EmailId = branch.EmailId,
                        Status = Status.active,
                        MainBranch = branch.MainBranch
                    };
                    db.Branchs.Add(bran);
                    db.SaveChanges();

                    com.addlog(LogTypes.Created, UserId, "Branch", "Branchs", findip(), bran.BranchID, "Branch Added Successfully");
                    msg = "Successfully added branch details.";
                    stat = true;
                }
                else
                {
                    msg = "Looks like something went wrong. Please check your form.";
                    stat = false;
                }

            }
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
