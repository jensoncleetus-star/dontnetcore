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
using QuickSoft.Models;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic;
using System.Net;
using QuickSoft.ViewModel;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class TaxController : BaseController
    {
        // GET: Tax
        ApplicationDbContext db;
        Common com;
        public TaxController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        #region Tax
        [QkAuthorize(Roles = "Dev,Tax & TaxGroup")]
        public ActionResult Index()
        {
            return View();
        }
        // datatable fields listing

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Tax & TaxGroup")]
        public JsonResult TaxGetData()
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
            var uEdit = User.IsInRole("Edit Tax");
            var uDelete = User.IsInRole("Delete Tax");

            var v = db.Taxs.Where(b => b.TaxType == "Tax").Select(b => new
            {
                id = b.TaxID,
                b.TaxName,
                b.TaxType,
                b.Percentage,
                b.Status,
                Dev = uDev,
                Edit = uEdit,
                Delete = uDelete
            });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.TaxName.ToString().ToLower().Contains(search.ToLower()));
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

        // GET: Field/Create
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Create Tax")]
        public ActionResult Create()
        {
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Create Tax")]
        public JsonResult Create(Tax Tx)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;

            var Exists = db.Taxs.Any(c => c.TaxName == Tx.TaxName);
            if (Exists)
            {
                msg = "Tax Name already exists.";
                stat = false;
            }
            else
            {
                if (ModelState.IsValid)
                {

                    var UserId = User.Identity.GetUserId();
                    var taxs = new Tax
                    {
                        TaxName = Tx.TaxName,
                        TaxType = "Tax",
                        Percentage = Tx.Percentage,
                        Status = Status.active
                    };
                    db.Taxs.Add(taxs);
                    db.SaveChanges();
                    Id = taxs.TaxID;

                    com.addlog(LogTypes.Created, UserId, "Tax", "Taxs", findip(), taxs.TaxID, "Successfully Added Tax details");

                    msg = "Successfully added Tax details.";
                    stat = true;
                }
                else
                {
                    msg = "Looks like something went wrong. Please check your form.";
                    stat = false;
                }
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }
        // GET: tax/Edit/5
        [QkAuthorize(Roles = "Dev,Edit Tax")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Tax taxs = db.Taxs.Find(id);

            if (taxs == null)
            {
                return NotFound();
            }
            return PartialView(taxs);
        }

        // POST: tax/Edit/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Tax")]
        public JsonResult Edit(Tax tx, int? id)
        {
            bool stat = false;
            string msg;

            var Exists = db.Taxs.Any(c => c.TaxName == tx.TaxName && c.TaxID != id);
            if (Exists)
            {
                msg = "Tax already exists.";
                stat = false;
            }
            else
            {
                if (ModelState.IsValid)
                {
                    tx.TaxType = "Tax";
                    db.Entry(tx).State = EntityState.Modified;
                    db.SaveChanges();
                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "Tax", "Taxs", findip(), tx.TaxID, "Successfully Updated Tax details");

                    msg = "Successfully Updated Tax Details.";
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

        // make active or inactive
        [HttpGet]
        public ActionResult TaxStatus(long? id, string type)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tax tx = db.Taxs.Find(id);
            if (tx == null)
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
        // POST: tax/ChangeStatus/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult TaxStatus(string type, long? id, Tax tx)
        {
            bool stat = false;
            string msg;
            string types = "";
            Tax taxs = db.Taxs.Find(id);
            if (tx.Status == Status.inactive)
            {
                types = " Inactive";
                taxs.Status = Status.inactive;
            }
            else
            {
                types = " Active";
                taxs.Status = Status.active;
            }

            db.Entry(taxs).State = EntityState.Modified;
            var updates = db.SaveChanges();

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Changed, UserId, "Tax", "Taxs", findip(), tx.TaxID, "Successfully Changed the Tax to" + types);


            stat = true;
            msg = " Successfully Changed the Tax to" + types;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }



        // GET: Tax/Delete/5
        [QkAuthorize(Roles = "Dev,Delete Tax")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tax tx = db.Taxs.Find(id);
            if (tx == null)
            {
                return NotFound();
            }

            return PartialView(tx);
        }

        // POST: Field/Delete/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Tax")]
        public JsonResult Delete(long id, IFormCollection collection)
        {
            bool stat = false;
            string msg;
            var Exists = db.Items.Any(c => c.TaxID == id);
            if (Exists)
            {
                msg = "Unable to delete Tax, Item with this Tax exists.";
                stat = false;
            }
            else
            {
                Tax tx = db.Taxs.Find(id);
                db.Taxs.Remove(tx);
                db.SaveChanges();
                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "Tax", "Taxs", findip(), tx.TaxID, "Successfully Deleted Tax details");

                stat = true;
                msg = "Successfully Deleted Tax details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        [HttpPost]
        public JsonResult ChekTax(long id)
        {
            Tax tx = db.Taxs.Find(id);
            var tax = tx.Percentage;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { tax = tax } };
        }
        #endregion
        #region TAX group



        [HttpPost]
        [QkAuthorize(Roles = "Dev,Tax & TaxGroup")]
        public JsonResult TaxGroupGetData()
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


            //--------------------------------------------------------


            //                             let ces = from ce in db.Taxs
            //                             where ces.Contains(b.parent)




            var v = (from a in db.Taxs
                     where a.TaxType == "TaxGroup"
                     select new
                     {
                         id = a.TaxID,
                         a.Status,
                         a.Percentage,
                         a.TaxName,

                         taxtot = (from d in db.TaxGroups
                                   join f in db.Taxs on d.child equals f.TaxID
                                   where d.parent == a.TaxID
                                   select f.Percentage).Sum(),
                         subtaxes = (from d in db.TaxGroups
                                     join f in db.Taxs on d.child equals f.TaxID
                                     where d.parent == a.TaxID
                                     select new
                                     {
                                         f.TaxName
                                     })
                     });


            //------------------------------------------------------------------

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.TaxName.ToString().ToLower().Contains(search.ToLower()));
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


        //get tax percentage by id
        public ActionResult GetPercentage(long[] array)
        {
            if (array != null)
            {
                var lbs = (from g in db.Taxs where array.Contains(g.TaxID) select g.Percentage).Sum();
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                string result = javaScriptSerializer.Serialize(lbs);
                return Json(result);
            }
            else
            {
                string result = "0.00";
                return Json(result);
            }
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Create Tax")]
        public ActionResult GroupCreate()
        {
            var stands = db.Taxs.Where(b => b.TaxType == "Tax" && b.Status == Status.active)
                         .Select(s => new
                         {
                             FieldID = s.TaxName,
                             FieldName = s.TaxID
                         })
                         .ToList();
            ViewBag.ParentModules = QkSelect.List(stands, "FieldName", "FieldID");
            return PartialView();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create Tax")]
        [ValidateAntiForgeryToken]
        public JsonResult GroupCreate(TaxViewModel Tx)
        {
            bool stat = false;
            string msg;

            var Exists = db.Taxs.Any(c => c.TaxName == Tx.TaxName);
            if (Exists)
            {
                msg = "Tax Name already exists.";
                stat = false;
            }
            else
            {
                if (ModelState.IsValid)
                {

                    var UserId = User.Identity.GetUserId();
                    //--------------------------------------------------------
                    Int64 TaxId = 0;
                    var taxs = new Tax
                    {
                        TaxName = Tx.TaxName,
                        TaxType = "TaxGroup",
                        Percentage = Tx.Percentage,
                        Status = Status.active
                    };
                    db.Taxs.Add(taxs);
                    db.SaveChanges();
                    TaxId = taxs.TaxID;

                    foreach (var chd in Tx.child)
                    {
                        var taxGp = new TaxGroup
                        {
                            parent = TaxId,
                            child = chd

                        };
                        db.TaxGroups.Add(taxGp);

                    }
                    //--------------------------------------------------------

                    db.SaveChanges();

                    com.addlog(LogTypes.Created, UserId, "Tax", "Taxs", findip(), taxs.TaxID, "Successfully Added Tax details");

                    msg = "Successfully added Tax Group details.";
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




        // GET: tax/Edit/5
        [QkAuthorize(Roles = "Dev,Edit Tax")]
        public ActionResult GroupEdit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Tax tax = db.Taxs.Find(id);

            if (tax == null)
            {
                return NotFound();
            }

            var taxes = db.TaxGroups.Where(a => a.parent == tax.TaxID).Select(a => a.child).ToList();
            long[] taxIds = taxes.ToArray();

            var stands = db.Taxs.Where(b => b.TaxType == "Tax" && b.Status == Status.active)
                       .Select(s => new
                       {
                           FieldID = s.TaxName,
                           FieldName = s.TaxID
                       })
                         .ToList();
            ViewBag.ParentModules = new MultiSelectList(stands, "FieldName", "FieldID", taxIds);

            TaxViewModel Emodel = new TaxViewModel();
            Int64 taxId = tax.TaxID;
            Emodel.TaxName = tax.TaxName;
            Emodel.Percentage = tax.Percentage;
            return PartialView(Emodel);
        }

        // POST: tax/Edit/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Tax")]
        public JsonResult GroupEdit(TaxViewModel taxView, long id)
        {
            bool stat = false;
            string msg;

            var Exists = db.Taxs.Any(c => c.TaxName == taxView.TaxName && c.TaxID != id);
            if (Exists)
            {
                msg = "Tax Group already exists.";
                stat = false;
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var UserId = User.Identity.GetUserId();
                    Tax tax = db.Taxs.Find(id);
                    tax.TaxName = taxView.TaxName;
                    tax.Percentage = taxView.Percentage;
                    db.Entry(tax).State = EntityState.Modified;
                    db.SaveChanges();

                    var taxgpID = db.TaxGroups.Where(a => a.parent == tax.TaxID).Select(a => a.TaxGroupID).ToList();
                    long[] taxIds = taxgpID.ToArray();
                    foreach (var txgpid in taxIds)
                    {
                        var txgp = db.TaxGroups.Find(txgpid);
                        db.TaxGroups.Remove(txgp);
                    }
                    db.SaveChanges();

                    foreach (var chd in taxView.child)
                    {
                        var taxGp = new TaxGroup
                        {
                            parent = id,
                            child = chd

                        };
                        db.TaxGroups.Add(taxGp);
                    }
                    db.SaveChanges();

                    com.addlog(LogTypes.Updated, UserId, "Tax", "Taxs", findip(), tax.TaxID, "Successfully Updated Tax details");


                    msg = "Successfully Updated Tax Group Details.";
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


        //  GET: Tax group/Delete/5
        [QkAuthorize(Roles = "Dev,Delete Tax")]
        public ActionResult GroupDelete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tax tx = db.Taxs.Find(id);
            if (tx == null)
            {
                return NotFound();
            }

            return PartialView(tx);
        }

        // POST:tax group/Delete/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Tax")]
        public JsonResult GroupDelete(long id, IFormCollection collection)
        {
            bool stat = false;
            string msg;
            var Exists = db.Items.Any(c => c.TaxID == id);
            if (Exists)
            {
                msg = "Unable to delete Tax group, Item with this Tax exists.";
                stat = false;
            }
            else
            {
                Tax tx = db.Taxs.Find(id);
                db.Taxs.Remove(tx);

                var taxgpID = db.TaxGroups.Where(a => a.parent == tx.TaxID).Select(a => a.TaxGroupID).ToList();
                long[] taxIds = taxgpID.ToArray();
                foreach (var txgpid in taxIds)
                {
                    var txgp = db.TaxGroups.Find(txgpid);
                    db.TaxGroups.Remove(txgp);
                }

                db.SaveChanges();
                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "Tax", "Taxs", findip(), tx.TaxID, "Successfully Deleted Tax details");

                stat = true;
                msg = "Successfully Deleted Tax Group details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        #endregion

      
        public JsonResult SearchTax(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.Taxs
                                  where b.TaxType == "Tax" && (b.TaxName.ToLower().Contains(q.ToLower()) || b.TaxName.Contains(q))
                                  select new SelectFormat
                                  {
                                      text = b.TaxName, //each json object will have 
                                      id = b.TaxID
                                  }).OrderBy(b => b.text).ToList();

            }
            else
            {
                serialisedJson = db.Taxs.Where(b => b.TaxType == "Tax").Select(b => new SelectFormat
                {
                    text = b.TaxName, //each json object will have 
                    id = b.TaxID
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Tax" };
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
