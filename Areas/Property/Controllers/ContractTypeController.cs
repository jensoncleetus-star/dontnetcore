using QuickSoft.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using Microsoft.AspNetCore.Identity;
using QuickSoft.Controllers;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Collections.Generic;

namespace QuickSoft.Areas.Property.Controllers
{
    [Microsoft.AspNetCore.Mvc.Area("Property")]
    public class ContractTypeController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ContractTypeController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Property/ContractType
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, PropertyType")]
        public ActionResult GetContractType()
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

            var UserView = (from a in db.ContractTypes
                            select new
                            {
                                id = a.ID,
                                a.Name,
                                a.Account
                            });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(p => p.Name.ToString().ToLower().Contains(search.ToLower()));
            }
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
               // UserView = UserView.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = UserView.Count();
            var data = UserView.ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        public ActionResult Create()
        {
            var expparentid = new SqlParameter("@parentid", 13);
            var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
            var expgpid = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            var expgrp = (from a in db.AccountsGroups
                          where (expgpid.Contains(a.AccountsGroupID))
                          select new
                          {
                              ID = a.AccountsGroupID,
                              Name = a.Name,
                          });
            ViewBag.Acc = QkSelect.List(expgrp, "ID", "Name");
            return PartialView();
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, Create PayrollUnits")]
        public JsonResult Create(ContractType vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.ContractTypes.Any(c => c.Name == vmodel.Name);
                if (Exists)
                {
                    msg = "Type Name already exists.";
                    stat = false;
                }
                else
                {
                    var proptype = new ContractType
                    {
                        Name = vmodel.Name,
                        Account = vmodel.Account
                    };
                    db.ContractTypes.Add(proptype);
                    db.SaveChanges();
                    Int64 ID = proptype.ID;

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "ContractTypes", "ContractTypes", findip(), ID, "Contract Type Added Successfully");
                    msg = "Successfully added Contract Type details.";
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
            ContractType protyp = db.ContractTypes.Find(id);

            if (protyp == null)
            {
                return NotFound();
            }

            ContractType vmodel = new ContractType();

            vmodel.ID = (long)id;
            vmodel.Name = protyp.Name;
            vmodel.Account = protyp.Account;
            var expparentid = new SqlParameter("@parentid", 13);
            var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
            var expgpid = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            var expgrp = (from a in db.AccountsGroups
                          where (expgpid.Contains(a.AccountsGroupID))
                          select new
                          {
                              ID = a.AccountsGroupID,
                              Name = a.Name,
                          });
            ViewBag.Acc = QkSelect.List(expgrp, "ID", "Name");

            return PartialView(vmodel);
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, Create ContractType")]
        public JsonResult Update(ContractType vmodel)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.ContractTypes.Any(c => c.Name == vmodel.Name && c.ID != vmodel.ID);
                if (Exists)
                {
                    msg = "Type Name already exists.";
                    stat = false;
                }
                else
                {
                    ContractType protyp = db.ContractTypes.Find(vmodel.ID);

                    protyp.Name = vmodel.Name;
                    protyp.Account = vmodel.Account;
                    db.Entry(protyp).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "ContractType", "ContractTypes", findip(), vmodel.ID, "Contract Type Updated Successfully");
                    msg = "Successfully Updated Contract Type details.";
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
        //[Authorize(Roles = "Dev,Delete ContractType")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ContractType ptype = db.ContractTypes.Find(id);
            if (ptype == null)
            {
                return NotFound();
            }
            return PartialView(ptype);
        }
        [RedirectingAction]
        //[Authorize(Roles = "Dev,Delete ContractType")]
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
                msg = "Successfully Deleted Contract Type details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public bool DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            ContractType pt = db.ContractTypes.Find(id);

            db.ContractTypes.Remove(pt);
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "ContractType", "ContractTypes", findip(), pt.ID, "Contract Type Deleted Successfully");
            return true;
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;

            return msg;
        }

        public JsonResult SearchContractType(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.ContractTypes
                                  where (a.Name.ToLower().Contains(q.ToLower()) || a.Name.Contains(q))
                                  //&& a.Account == Sectn
                                  select new SelectFormat
                                  {
                                      text = a.Name,
                                      id = a.ID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.ContractTypes/*.Where(z => z.Account == Sectn)*/.Select(b => new SelectFormat
                {
                    text = b.Name,
                    id = b.ID
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Contract Type" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public JsonResult SearchTypebasedContract(string q, string x, long? Contractor)
        {
            List<SelectFormat> serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.ContractTypes
                                  join b in db.Contractors on a.ID equals b.ContractType into bro
                                  from b in bro.DefaultIfEmpty()
                                  where (a.Name.ToLower().Contains(q.ToLower()) || a.Name.Contains(q)) && (Contractor==0 || Contractor == null || b.ContractorID== Contractor)
                                  //&& a.Account == Sectn
                                  select new SelectFormat
                                  {
                                      text = a.Name,
                                      id = a.ID
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.ContractTypes
                                  join b in db.Contractors on a.ID equals b.ContractType into bro
                                  from b in bro.DefaultIfEmpty()
                                  where (Contractor == 0 || Contractor == null || b.ContractorID == Contractor)
                                  //&& a.Account == Sectn
                                  select new SelectFormat
                                  {
                                      text = a.Name,
                                      id = a.ID
                                  }).OrderBy(b => b.text).ToList();

            }//
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Contract Type" };
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