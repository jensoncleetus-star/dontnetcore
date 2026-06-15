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
using Microsoft.AspNetCore.Identity;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{

    [RedirectingAction]
    public class MasterController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        //Get Customer Details From Db
        public MasterController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        #region AccountsGroup
        // for Accounts-Group
        // index 
        [QkAuthorize(Roles = "Dev,Expense Group")]
        public ActionResult AccountsGroup()
        {
            ViewBag.ItStatus = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Active", Value="0"},
                new SelectListItem() {Text = "Inactive", Value="1"},
            }, "Value", "Text");
            ViewBag.ParentGrp = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                               }, "Value", "Text", 1);
            ViewBag.Primary = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Yes", Value="0"},
                new SelectListItem() {Text = "No", Value="1"},
            }, "Value", "Text");
            ViewBag.Accounts = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All Accounts", Value=null},
                new SelectListItem() {Text = "Have Accounts", Value="0"},
                new SelectListItem() {Text = "No Accounts", Value="1"},
            }, "Value", "Text");
            ViewBag.AccountName = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = true, Text = "All", Value = ""},
                               }, "Value", "Text", 1);
            return View();
        }

        // datatable fields listing
        [QkAuthorize(Roles = "Dev,Expense Group")]
        public JsonResult AccountsGroupGetData(long? AccName, long? AccParent, string Stats, string Primary, int? Account)
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
            Status st = new Status();
            choice ch = new choice();
            if (Stats != "")
            {
                st = (Stats == "0") ? Status.active : Status.inactive;
            };
            if (Primary != "")
            {
                ch = (Primary == "0") ? choice.Yes : choice.No;
            };
            var ModList = (from a in db.AccountsGroups
                           join c in db.Users on a.CreatedBy equals c.Id into user
                           from c in user.DefaultIfEmpty()
                           join b in db.AccountsGroups on a.Parent equals b.AccountsGroupID into module
                           from b in module.DefaultIfEmpty()
                           let f = db.Accountss.Where(x => x.Group == a.AccountsGroupID).Count()
                           let g= db.Accountss.Where(x => x.AccountsID==AccName).FirstOrDefault()
                           where (AccName == null || AccName == 0 || a.AccountsGroupID == g.Group) &&
                                 (AccParent == null || AccParent == 0 || AccParent == a.Parent) &&
                                 (Stats == null || Stats == "" || b.Status == st) && (Primary == null || Primary == "" || b.Primary == ch)
                                 && (Account == null || (Account == 0 ? (db.Accountss.Where(x => x.Group == a.AccountsGroupID).Count() > 0) : (db.Accountss.Where(x => x.Group == a.AccountsGroupID).Count() == 0)))
                           //let parent=db.AccountsGroups.Where(p=>p.Parent==a.AccountsGroupID).Select(p=>p.AccountsGroupID).ToList()
                           select new
                           {
                               id = a.AccountsGroupID,
                               a.Name,
                               a.Alias,
                               a.Parent,
                               Primary = (a.Primary == 0) ? "Yes" : "No",
                               a.Status,
                               ParentName = b.Name,
                               a.Editable,
                               TotalAcc = f
                           });
            ////search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                ModList = ModList.Where(p => p.id.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Name.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Alias.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.ParentName.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Status.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Primary.ToString().ToLower().Contains(search.ToLower()));

            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                ModList = ModList.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = ModList.Count();
            var data = ModList.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        // GET: Field/Create
        [QkAuthorize(Roles = "Dev,Create Expense Group")]
        public ActionResult AccountsGroupCreate()
        {
            var stands = db.AccountsGroups.Where(s => s.AccountsGroupID != 12 && s.AccountsGroupID != 14 && s.AccountsGroupID != 8)

                            .Select(s => new
                            {
                                FieldID = s.Name,
                                FieldName = s.AccountsGroupID
                            })
                          .ToList();
            ViewBag.ParentModules = QkSelect.List(stands, "FieldName", "FieldID");
            return PartialView();
        }

        // POST: Dep/Create
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create Expense Group")]
        [ValidateAntiForgeryToken]
        public JsonResult AccountsGroupCreate(AccountsGroup Acc)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.AccountsGroups.Any(c => c.Name == Acc.Name);
                if (Exists)
                {
                    msg = "Accounts Group already exists.";
                    stat = false;
                }
                else
                {
                    var acc = new AccountsGroup
                    {
                        Name = Acc.Name,
                        Alias = Acc.Alias,
                        Parent = Acc.Primary == 0 ? 0 : Acc.Parent,
                        Primary = Acc.Primary,
                        Status = Acc.Status

                    };
                    db.AccountsGroups.Add(acc);
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "Master", "AccountsGroups", findip(), acc.AccountsGroupID, "Accounts Group Added Successfully");


                    msg = "Successfully added Accounts Groups details.";
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


        // GET: dep/Edit/5
        [QkAuthorize(Roles = "Dev,Edit Expense Group")]
        public ActionResult AccountsGroupEdit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AccountsGroup group = db.AccountsGroups.Find(id);
            if (group == null)
            {
                return NotFound();
            }

            AccountsGroup Acc = new AccountsGroup();

            Acc.AccountsGroupID = group.AccountsGroupID;
            Acc.Name = group.Name;
            Acc.Parent = group.Parent;
            Acc.Primary = group.Primary;
            Acc.Status = group.Status;
            Acc.Alias = group.Alias;

            //   .Select(s => new
            //       ID = s.AccountsGroupID,
            //       Name = s.Name
            //   })

            var stands = db.AccountsGroups.Where(s => s.AccountsGroupID == Acc.Parent)
               .Select(s => new
               {
                   ID = s.AccountsGroupID,
                   Name = s.Name
               })
               .ToList();
            ViewBag.ParentModules = QkSelect.List(stands, "ID", "Name");


            return PartialView(Acc);
        }

        // POST: department/Edit/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Expense Group")]
        [ValidateAntiForgeryToken]
        public JsonResult AccountsGroupEdit(AccountsGroup Acc, long id)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Editable = db.AccountsGroups.Any(a => a.Editable == choice.No && a.AccountsGroupID == id);
                if (Editable)
                {
                    msg = "Sorry, It is a Pre-defined Account Group And Cannot be Edited.";
                    stat = false;
                }
                else
                {


                    var Exists = db.AccountsGroups.Any(c => c.Name == Acc.Name && c.AccountsGroupID != id);
                    if (Exists)
                    {
                        msg = "Accounts Group already exists.";
                        stat = false;
                    }
                    else
                    {
                        AccountsGroup group = db.AccountsGroups.Find(id);

                        group.Name = Acc.Name;
                        group.Parent = Acc.Primary == 0 ? 0 : Acc.Parent;
                        group.Primary = Acc.Primary;
                        group.Status = Acc.Status;
                        group.Alias = Acc.Alias;

                        db.Entry(group).State = EntityState.Modified;
                        db.SaveChanges();

                        var UserId = User.Identity.GetUserId();
                        com.addlog(LogTypes.Updated, UserId, "Master", "AccountsGroups", findip(), group.AccountsGroupID, "Accounts Group Updated Successfully");



                        msg = "Successfully updated Accounts Group details.";
                        stat = true;
                    }
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        // GET: Desg/Delete/5
        [QkAuthorize(Roles = "Dev,Edit Expense Group")]
        public ActionResult AccountsGroupDelete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AccountsGroup AccInfo = db.AccountsGroups.Find(id);
            if (AccInfo == null)
            {
                return NotFound();
            }

            return PartialView(AccInfo);
        }

        // POST: Field/Delete/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Expense Group")]
        public JsonResult AccountsGroupDelete(int id, IFormCollection collection)
        {
            bool stat = false;
            string msg;
            var Editable = db.AccountsGroups.Any(a => a.Editable == choice.No && a.AccountsGroupID == id);
            if (Editable)
            {
                msg = "Sorry, It is a Pre-defined Account Group And Cannot be Deleted.";
                stat = false;
            }
            else
            {
                var Exists = db.Accountss.Any(c => c.Group == id);
                if (Exists)
                {
                    msg = "Unable to delete AccountsGroup, accounts with this Accounts Group exists.";
                    stat = false;
                }
                else
                {
                    stat = DeleteFnAccGP(id);
                    msg = "Successfully Deleted Account Group  details.";
                }
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Expense Group Delete")]
        public ActionResult DeleteAllAccGp(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteAccGP(arr) == true) ? count++ : notdel++;
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Account Group, Unable to Delete " + notdel + " Account Group. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Account Group.", true);
            }
            else
            {
                Success("Deleted " + count + " Account Group.", true);
            }
            return RedirectToAction("AccountsGroup", "Master");
        }
        private Boolean DeleteAccGP(long id)
        {
            var Editable = db.AccountsGroups.Any(a => a.Editable == choice.No && a.AccountsGroupID == id);
            if (Editable)
            {
                return false;
            }
            else
            {
                var Exists = db.Accountss.Any(c => c.Group == id);
                bool res = (Exists) ? false : DeleteFnAccGP(id);
                return res;
            }
        }
        public bool DeleteFnAccGP(long id)
        {
            AccountsGroup Accinfo = db.AccountsGroups.Find(id);
            if (Accinfo != null)
            {
                db.AccountsGroups.RemoveRange(db.AccountsGroups.Where(a => a.AccountsGroupID == id));
            }
            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "Master", "AccountsGroups", findip(), Accinfo.AccountsGroupID, "Accounts Group Deleted Successfully");
            db.SaveChanges();
            return true;
        }

        // make active or inactive
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Expense Group Status")]
        public ActionResult AccountsGroupStatus(string type, long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AccountsGroup Acc = db.AccountsGroups.Find(id);
            if (Acc == null)
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
        [QkAuthorize(Roles = "Dev,Expense Group Status")]
        [ValidateAntiForgeryToken]
        public JsonResult AccountsGroupStatus(string type, long? id, AccountsGroup AccG)
        {
            bool stat = false;
            string msg;
            string types = "";
            AccountsGroup Acc = db.AccountsGroups.Find(id);
            if (AccG.Status == Status.inactive)
            {
                types = " Inactive";
                Acc.Status = Status.inactive;
            }
            else
            {
                types = " Active";
                Acc.Status = Status.active;
            }

            db.Entry(Acc).State = EntityState.Modified;
            var updates = db.SaveChanges();

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Changed, UserId, "Master", "AccountsGroups", findip(), Acc.AccountsGroupID, "Successfully Changed the Account Group Status" + types);


            stat = true;
            msg = " Successfully Changed the Account Group to" + types;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        public JsonResult GroupAccounts(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            // expense =13
            var expparentid = new SqlParameter("@parentid", 13);
            var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
            var arr = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.AccountsGroups.Where(p => (p.Name.ToLower().Contains(q.ToLower()) || p.Name.Contains(q) || p.Alias.Contains(q)) && arr.Contains(p.AccountsGroupID) && p.Status == Status.active)
                              .Select(b => new SelectFormat
                              {
                                  text = b.Name, //each json object will have 
                                  id = b.AccountsGroupID
                              })
                              .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.AccountsGroups.Where(p => arr.Contains(p.AccountsGroupID) && p.Status == Status.active)
                              .Select(b => new SelectFormat
                              {
                                  text = b.Name, //each json object will have 
                                  id = b.AccountsGroupID
                              })
                              .OrderBy(b => b.text).ToList();

            }
            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select AccountGroup" };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }
        public JsonResult SearchAllAccountsGroup(string q, string x, string page)
        {
            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.AccountsGroups.Where(p => (p.Name.ToLower().Contains(q.ToLower()) || p.Name.Contains(q) || p.Alias.Contains(q)) && p.Status == Status.active)
                             .Select(b => new SelectFormat
                             {
                                 text = b.Name, //each json object will have 
                                  id = b.AccountsGroupID
                             })
                             .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.AccountsGroups.Where(p => p.Status == Status.active)
                               .Select(b => new SelectFormat
                               {
                                   text = b.Name, //each json object will have 
                                  id = b.AccountsGroupID
                               })
                               .OrderBy(b => b.text).ToList();

            }
            if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        #endregion

        #region ExpenseType
        [QkAuthorize(Roles = "Dev,Expense Account")]
        public ActionResult ExpenseType()
        {
            return View();
        }

        // datatable fields listing
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Expense Account")]
        public JsonResult ExpenseTypeData()
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
            var expparentid = new SqlParameter("@parentid", 13);
            var expgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", expparentid).AsEnumerable().ToList();
            var expgpid = expgroupsdata.Select(a => a.AccountsGroupID).ToArray();
            var v = db.Accountss.Where(a => expgpid.Contains(a.Group))
                .Select(a => new
                {
                    id = a.AccountsID,
                    a.Name,
                    a.Alias,
                    a.PrintName,
                    a.Note,
                    Group = db.AccountsGroups.Where(b => b.AccountsGroupID == a.Group).Select(b => b.Name).FirstOrDefault(),
                    a.Status,
                    a.Editable
                });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.id.ToString().ToLower().Contains(search.ToLower()) ||
                                     p.Name.ToString().ToLower().Contains(search.ToLower()) ||
                                     p.Status.ToString().ToLower().Contains(search.ToLower()) ||
                                     p.Note.ToString().ToLower().Contains(search.ToLower()));
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




        // GET: ExpenseTypecreate
        [QkAuthorize(Roles = "Dev,Create Expense Account")]
        public ActionResult ExpenseTypecreate()
        {

            var stands = db.AccountsGroups.Where(s => s.AccountsGroupID == 13 || s.Parent == 13)
                         .Select(s => new
                         {
                             FieldID = s.Name,
                             FieldName = s.AccountsGroupID
                         })
                         .ToList();
            ViewBag.ParentModules = QkSelect.List(stands, "FieldName", "FieldID");

            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            return PartialView();
        }

        // POST: ExpenseTypecreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Create Expense Account")]
        public JsonResult ExpenseTypecreate(Accounts Acc)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.Accountss.Any(c => c.Name == Acc.Name && c.Group == 13);
                if (Exists)
                {
                    msg = "Expense Type already exists.";
                    stat = false;
                }
                else
                {

                    var UserId = User.Identity.GetUserId();
                    long Branch = 0;

                    var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                    var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                    if (BranchCheck == Status.active)
                    {
                        Branch = Acc.Branch;
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    }

                    Acc.Group = 13;
                    Acc.Branch = Branch;
                    addAccount(Acc);

                    com.addlog(LogTypes.Created, UserId, "Master", "Accountss", findip(), Acc.AccountsID, "Successfully Created Expense Type");


                    msg = "Expense Type Created Successfully.";
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


        // GET: ExpenseTypeEdit/5
        [QkAuthorize(Roles = "Dev,Edit Expense Account")]
        public ActionResult ExpenseTypeEdit(long id)
        {
            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            Accounts group = db.Accountss.Find(id);
            if (group == null)
            {
                return NotFound();
            }
            Accounts Acc = new Accounts();

            Acc.AccountsID = group.AccountsID;
            Acc.Name = group.Name;
            Acc.Note = group.Note;
            Acc.Status = group.Status;
            Acc.Branch = group.Branch;

            return PartialView(Acc);
        }

        // POST: ExpenseTypeEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Edit Expense Account")]
        public JsonResult ExpenseTypeEdit(Accounts Acc, long id)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.Accountss.Any(c => c.Name == Acc.Name && c.Group == 13 && c.AccountsID != id);
                if (Exists)
                {
                    msg = "Expense Type already exists.";
                    stat = false;
                }
                else
                {
                    var Editable = db.Accountss.Any(a => a.Editable == choice.No && a.AccountsID == id);
                    if (Editable)
                    {
                        msg = "Sorry, It is a Pre-defined Account And Cannot be Edited.";
                        stat = false;
                    }
                    else
                    {
                        var UserId = User.Identity.GetUserId();
                        long Branch = 0;

                        var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                        var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                        if (BranchCheck == Status.active)
                        {
                            Branch = Acc.Branch;
                        }
                        else
                        {
                            Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                        }
                        Acc.Branch = Branch;

                        editAccount(Acc, id);
                        com.addlog(LogTypes.Updated, UserId, "Master", "Accountss", findip(), Acc.AccountsID, "Successfully Updated Expense Type");

                        msg = "Successfully updated Expense Type details.";
                        stat = true;
                    }
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check again.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        // GET: Desg/Delete/5
        [QkAuthorize(Roles = "Dev,Delete Expense Account")]
        public ActionResult ExpenseTypeDelete(long id)
        {
            Accounts AccInfo = db.Accountss.Find(id);
            if (AccInfo == null)
            {
                return NotFound();
            }
            return PartialView(AccInfo);
        }

        // POST: Field/Delete/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Expense Account")]
        public JsonResult ExpenseTypeDelete(long id, IFormCollection collection)
        {
            bool stat = false;
            string msg;
            var Editable = db.Accountss.Any(a => a.Editable == choice.No && a.AccountsID == id);
            if (Editable)
            {
                msg = "Sorry, It is a Pre-defined Account And Cannot be Deleted.";
                stat = false;
            }
            else
            {
                var Msg = chkDeleteWithMsg(id);
                if (Msg != null)
                {
                    msg = Msg;
                    stat = false;
                }
                else
                {
                    Accounts Accinfo = db.Accountss.Find(id);
                    db.Accountss.Remove(Accinfo);
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Deleted, UserId, "Master", "Accountss", findip(), Accinfo.AccountsID, "Successfully Deleted Expense Type");


                    stat = true;
                    msg = "Successfully Deleted Expense Type details.";
                }
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            if (db.Payments.Any(c => c.PayTo == id))
            {
                msg = "Unable to delete Expense Type, Payment with this Expense Type exists.";
            }
            else if (db.Receipts.Any(c => c.PayFrom == id))
            {
                msg = "Unable to delete Expense Type, Receipt with this Expense Type exists.";
            }
            else
            {
                msg = null;
            }
            return msg;
        }

        // make active or inactive
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Expense Account Status")]
        public ActionResult ExpenseTypeStatus(string type, long id)
        {
            Accounts Acc = db.Accountss.Find(id);
            if (Acc == null)
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
        [QkAuthorize(Roles = "Dev,Expense Account Status")]
        public JsonResult ExpenseTypeStatus(string type, long id, Accounts AccG)
        {
            bool stat = false;
            string msg;
            string types = ""; var Editable = db.Accountss.Any(a => a.Editable == choice.No && a.AccountsID == id);
            if (Editable)
            {
                msg = "Sorry, It is a Pre-defined Account And Cannot be Changed.";
                stat = false;
            }
            else
            {
                Accounts Acc = db.Accountss.Find(id);
                if (AccG.Status == Status.inactive)
                {
                    types = " Inactive";
                    Acc.Status = Status.inactive;
                }
                else
                {
                    types = " Active";
                    Acc.Status = Status.active;
                }

                db.Entry(Acc).State = EntityState.Modified;
                var updates = db.SaveChanges();

                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Changed, UserId, "Master", "Accountss", findip(), Acc.AccountsID, "Successfully Changed the Expense Type to" + types);


                stat = true;
                msg = " Successfully Changed the Expense Type to" + types;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        #endregion

        #region BankAccounts

        [QkAuthorize(Roles = "Dev,Bank Accounts")]
        public ActionResult BankAccounts()
        {
            ViewBag.Accts = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);
            ViewBag.AccNo = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);

            ViewBag.ItStatus = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "All", Value= null},
                new SelectListItem() {Text = "Active", Value="0"},
                new SelectListItem() {Text = "Inactive", Value="1"},
            }, "Value", "Text");
            return View();
        }

        // datatable fields listing
        [QkAuthorize(Roles = "Dev,Bank Accounts")]
        public JsonResult BankAccountsData(long? ddlAccount, string BrName, string IbanNo, string SwiftNo, string Stats, long? ddlAccNum, string Alias)
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
            Status st = new Status();
            if (Stats != "")
            {
                st = (Stats == "0") ? Status.active : Status.inactive;
            };
            var ModList = (
                from b in db.Accountss
                join a in db.Banks on b.AccountsID equals a.AccountId into bnk
                from a in bnk.DefaultIfEmpty()
                join c in db.Users on b.CreatedBy equals c.Id into user
                from c in user.DefaultIfEmpty()
                where b.Group == 8
                && (ddlAccount == null || ddlAccount == 0 || a.AccountId == ddlAccount)
                && (BrName == null || BrName == "" || a.BranchName == BrName)
                && (IbanNo == null || IbanNo == "" || a.IbanNo == IbanNo)
                && (SwiftNo == null || SwiftNo == "" || a.Swift == SwiftNo)
                && (Stats == null || Stats == "" || b.Status == st)
                && (ddlAccNum == null || ddlAccNum == 0 || a.AccountId == ddlAccNum)
                && (Alias == null || Alias == "" || b.Alias == Alias)

                select new
                {
                    id = (a.BankId == null) ? -1 : a.BankId,
                    AccountNo = (a.AccountNo == null) ? "" : a.AccountNo,
                    BranchName = (a.BranchName == null) ? "" : a.BranchName,
                    IbanNo = (a.IbanNo == null) ? "" : a.IbanNo,
                    Swift = (a.Swift == null) ? "" : a.Swift,
                    b.Name,
                    b.Alias,
                    b.PrintName,
                    b.Note,
                    OpnBalance = (b.OpnBalanceCr > 0) ? b.OpnBalanceCr + " Cr." : b.OpnBalance + " Dr.",
                    Credit = (db.AccountsTransactions.Where(d => d.Account == b.AccountsID && d.Status == null).Sum(d => (decimal?)d.Credit) ?? 0),
                    Debit = (db.AccountsTransactions.Where(x => x.Account == b.AccountsID && x.Status == null).Sum(x => (decimal?)x.Debit) ?? 0),
                    FirstName = c.Name,
                    b.Status,
                });
            ////searchatus,
       
            ////search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                ModList = ModList.Where(p => p.id.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.AccountNo.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.BranchName.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.IbanNo.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Swift.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Name.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Status.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Note.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Alias.ToString().ToLower().Contains(search.ToLower()));


            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                ModList = ModList.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = ModList.Count();
            var data = ModList.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        // GET: ExpenseTypecreate
        [QkAuthorize(Roles = "Dev,Create Bank Accounts")]
        public ActionResult BankAccountscreate()
        {
            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            return PartialView();
        }

        // POST: ExpenseTypecreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Create Bank Accounts")]
        public JsonResult BankAccountscreate(BankViewModel bnkview)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = (from b in db.Banks
                              join c in db.Accountss on b.AccountId equals c.AccountsID
                              where (b.AccountNo == bnkview.AccountNo && c.Name == bnkview.Name)
                              select new
                              {
                                  c.Name
                              }).Any();
                if (Exists)
                {
                    msg = "Bank Account already exists.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    long Branch = 0;

                    var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                    var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                    if (BranchCheck == Status.active)
                    {
                        Branch = bnkview.Branch;
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    }

                    Accounts acc = new Accounts();
                    acc.Name = bnkview.Name;
                    acc.Group = 8;
                    acc.Note = bnkview.Note;
                    acc.Status = bnkview.Status;
                    acc.Branch = Branch;
                    acc.Alias = bnkview.Alias;
                    if (bnkview.DC == DC.Debit)
                    {
                        acc.OpnBalance = bnkview.OpnBalance;
                        acc.OpnBalanceCr = 0;
                    }
                    if (bnkview.DC == DC.Credit)
                    {
                        acc.OpnBalance = 0;
                        acc.OpnBalanceCr = bnkview.OpnBalance;
                    }


                    Int64 accId = addAccount(acc);// acc.AccountsID;

                    Bank bnk = new Bank();
                    bnk.AccountNo = bnkview.AccountNo;
                    bnk.IbanNo = bnkview.IbanNo;
                    bnk.BranchName = bnkview.BranchName;
                    bnk.Swift = bnkview.Swift;
                    bnk.AccountId = accId;
                    db.Banks.Add(bnk);
                    db.SaveChanges();


                    if (bnkview.OpnBalance > 0)
                    {
                        if (bnkview.DC == DC.Debit)
                        {
                            com.addAccountTrasaction(bnkview.OpnBalance, 0, accId, "Opening Balance", accId, DC.Debit);

                        }
                        if (bnkview.DC == DC.Credit)
                        {
                            com.addAccountTrasaction(0, bnkview.OpnBalance, accId, "Opening Balance", accId, DC.Credit);
                        }
                    }
                    com.addlog(LogTypes.Created, UserId, "Master", "Banks", findip(), bnk.BankId, "Successfully Created Bank Account");


                    msg = "Bank Account Created Successfully.";
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


        // GET: ExpenseTypeEdit/5
        [QkAuthorize(Roles = "Dev,Edit Bank Accounts")]
        public ActionResult BankAccountsEdit(long? id)
        {
            var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
            var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
            ViewBag.BranchCheck = BranchCheck;

            var Bnch = db.Branchs
               .Select(s => new
               {
                   Id = s.BranchID,
                   Name = s.BranchName
               }).ToList();
            ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bank bnk = db.Banks.Find(id);
            if (bnk == null)
            {
                return NotFound();
            }
            Accounts account = db.Accountss.Find(bnk.AccountId);

            BankViewModel bnkmodel = new BankViewModel();

            bnkmodel.BankId = bnk.BankId;
            bnkmodel.Name = account.Name;
            bnkmodel.AccountNo = bnk.AccountNo;
            bnkmodel.IbanNo = bnk.IbanNo;
            bnkmodel.BranchName = bnk.BranchName;
            bnkmodel.Swift = bnk.Swift;
            bnkmodel.Note = account.Note;
            bnkmodel.Status = account.Status;
            bnkmodel.Branch = account.Branch;
            bnkmodel.Alias = account.Alias;
            if (account.OpnBalance == 0)
            {
                bnkmodel.DC = DC.Credit;
                bnkmodel.OpnBalance = account.OpnBalanceCr;
            }
            else
            {
                bnkmodel.DC = DC.Debit;
                bnkmodel.OpnBalance = account.OpnBalance;
            }
            return PartialView(bnkmodel);
        }

        //POST: ExpenseTypeEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Edit Bank Accounts")]
        public JsonResult BankAccountsEdit(BankViewModel bnkview, long id)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                Bank bnk = db.Banks.Find(id);
                bnk.AccountId = db.Banks.Where(a => a.BankId == id).Select(a => a.AccountId).First();
                var Exists = db.Accountss.Any(c => c.Name == bnkview.Name && c.Group == 8 && c.AccountsID != bnk.AccountId);
                if (Exists)
                {
                    msg = "Bank Account already exists.";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    long Branch = 0;

                    var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                    var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                    if (BranchCheck == Status.active)
                    {
                        Branch = bnkview.Branch;
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    }

                    bnk.AccountNo = bnkview.AccountNo;
                    bnk.BranchName = bnkview.BranchName;
                    bnk.IbanNo = bnkview.IbanNo;
                    bnk.Swift = bnkview.Swift;


                    db.Entry(bnk).State = EntityState.Modified;
                    db.SaveChanges();
                    Accounts Acc = new Accounts();
                    Acc.Name = bnkview.Name;

                    if (bnkview.DC == DC.Debit)
                    {
                        Acc.OpnBalance = bnkview.OpnBalance;
                        Acc.OpnBalanceCr = 0;
                    }
                    if (bnkview.DC == DC.Credit)
                    {
                        Acc.OpnBalance = 0;
                        Acc.OpnBalanceCr = bnkview.OpnBalance;
                    }
                    Acc.Note = bnkview.Note;
                    Acc.Status = bnkview.Status;
                    Acc.Branch = Branch;
                    Acc.Alias = bnkview.Alias;
                    editAccount(Acc, bnk.AccountId);

                    bool delete = com.DeleteAllAccountTransaction("Opening Balance", bnk.AccountId);

                    if (bnkview.OpnBalance > 0)
                    {
                        if (bnkview.DC == DC.Debit)
                        {
                            com.addAccountTrasaction(bnkview.OpnBalance, 0, bnk.AccountId, "Opening Balance", bnk.AccountId, DC.Debit);

                        }
                        if (bnkview.DC == DC.Credit)
                        {
                            com.addAccountTrasaction(0, bnkview.OpnBalance, bnk.AccountId, "Opening Balance", bnk.AccountId, DC.Credit);
                        }
                    }
                    com.addlog(LogTypes.Updated, UserId, "Master", "Banks", findip(), bnk.BankId, "Successfully Updated Bank Account");

                    msg = "Successfully updated Bank Account details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check again.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        // GET: Desg/Delete/5
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Delete Bank Accounts")]
        public ActionResult BankAccountsDelete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bank bnk = db.Banks.Find(id);
            if (bnk == null)
            {
                return NotFound();
            }
            return PartialView(bnk);
        }

        // POST: Field/Delete/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Bank Accounts")]
        public JsonResult BankAccountsDelete(int id, IFormCollection collection)
        {
            bool stat = false;
            string msg;

            var Msg = chkDeleteWithMsgBk(id);
            if (Msg != null)
            {
                msg = Msg;
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Successfully Deleted Bank Account details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Bank Accounts")]
        public ActionResult DeleteAllBankAcc(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteBankAcc(arr) == true) ? count++ : notdel++;
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Bank Accounts, Unable to Delete " + notdel + " Bank Accounts. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Bank Accounts.", true);
            }
            else
            {
                Success("Deleted " + count + " Bank Accounts.", true);
            }
            return RedirectToAction("BankAccounts", "Master");
        }

        private Boolean DeleteBankAcc(long id)
        {
            var Msg = chkDeleteWithMsgBk(id);
            bool res = (Msg != null) ? false : DeleteFn(id);
            return res;
        }

        public bool DeleteFn(long id)
        {
            Bank bnk = db.Banks.Find(id);
            Accounts Accinfo = db.Accountss.Find(bnk.AccountId);
            if (Accinfo != null)
            {
                db.Accountss.RemoveRange(db.Accountss.Where(a => a.AccountsID == bnk.AccountId));
            }

            if (bnk != null)
            {
                db.Banks.RemoveRange(db.Banks.Where(a => a.BankId == id));
            }

            var UserId = User.Identity.GetUserId();
            db.SaveChanges();
            com.addlog(LogTypes.Deleted, UserId, "Master", "Banks", findip(), bnk.BankId, "Successfully Deleted Bank Account");

            return true;
        }

        public string chkDeleteWithMsgBk(long id)
        {
            string msg = null;
            Bank bnk = db.Banks.Find(id);
            bnk.AccountId = db.Banks.Where(a => a.BankId == id).Select(a => a.AccountId).First();
            if (db.Payments.Any(c => c.PayFrom == bnk.AccountId))
            {
                msg = "Bank Account Already used in Payments !!";
            }
            else if (db.Receipts.Any(c => c.PayTo == bnk.AccountId))
            {
                msg = "Bank Account Already used in Receipts !!";
            }
            else if (db.ContraVouchers.Any(c => c.PayTo == bnk.AccountId) || db.ContraVouchers.Any(c => c.PayFrom == bnk.AccountId))
            {
                msg = "Bank Account Already used in ContraVouchers !!";
            }
            else if (db.Journals.Any(c => c.PayTo == bnk.AccountId) || db.Journals.Any(c => c.PayFrom == bnk.AccountId))
            {
                msg = "Bank Account Already used in Journals !!";
            }
            else if (db.CreditNotes.Any(c => c.PayTo == bnk.AccountId) || db.CreditNotes.Any(c => c.PayFrom == bnk.AccountId))
            {
                msg = "Bank Account Already used in CreditNotes !!";
            }
            else
            {
                msg = null;
            }
            return msg;
        }

        // make active or inactive
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Bank Accounts Status")]
        public ActionResult BankAccountsStatus(string type, long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Bank bnk = db.Banks.Find(id);
            bnk.AccountId = db.Banks.Where(a => a.BankId == id).Select(a => a.AccountId).First();
            Accounts Acc = db.Accountss.Find(bnk.AccountId);
            if (Acc == null)
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
        [QkAuthorize(Roles = "Dev,Bank Accounts Status")]
        public JsonResult BankAccountsStatus(string type, long? id, Accounts AccG)
        {
            bool stat = false;
            string msg;
            string types = "";
            Bank bnk = db.Banks.Find(id);
            bnk.AccountId = db.Banks.Where(a => a.BankId == id).Select(a => a.AccountId).First();

            Accounts Acc = db.Accountss.Find(bnk.AccountId);
            if (AccG.Status == Status.inactive)
            {
                types = " Inactive";
                Acc.Status = Status.inactive;
            }
            else
            {
                types = " Active";
                Acc.Status = Status.active;
            }

            db.Entry(Acc).State = EntityState.Modified;
            var updates = db.SaveChanges();

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Changed, UserId, "Master", "Banks", findip(), bnk.BankId, " Successfully Changed the Bank Account to" + types);


            stat = true;
            msg = " Successfully Changed the Bank Account to" + types;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }


        public JsonResult SearchBankAccountNo(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.Banks
                                  join d in db.Accountss on b.AccountId equals d.AccountsID
                                  where (d.Alias.ToLower().Contains(q.ToLower()) || b.AccountNo.ToLower().Contains(q.ToLower()) || d.Alias.Contains(q) || b.AccountNo.Contains(q))
                                  select new
                                  {
                                      text = b.AccountNo, //each json object will have 
                                      id = b.AccountId
                                  }).Distinct().ToList().Select(o => new SelectFormat
                                  {
                                      text = o.text, //each json object will have 
                                      id = o.id
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Banks//.Where(p => p.Group == 8)
                              .Select(b => new SelectFormat
                              {
                                  text = b.AccountNo, //each json object will have 
                                  id = b.AccountId
                              })
                              .OrderBy(b => b.text).ToList();

            }

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }
        // list all bank accounts except walking customerse
        public JsonResult SearchBankAccounts(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Accountss.Where(p => (p.Name.ToLower().Contains(q.ToLower()) || p.Alias.ToLower().Contains(q.ToLower()) || p.Name.Contains(q) || p.Alias.Contains(q)) && p.Group == 8)
                              .Select(b => new SelectFormat
                              {
                                  text = b.Name, //each json object will have 
                                  id = b.AccountsID
                              })
                              .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Accountss.Where(p => p.Group == 8)
                              .Select(b => new SelectFormat
                              {
                                  text = b.Name, //each json object will have 
                                  id = b.AccountsID
                              })
                              .OrderBy(b => b.text).ToList();

            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Bank Account" };
                serialisedJson.Insert(0, initial);
            }
            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }

        public JsonResult BankAccCheck(string Account, string Bankname, long? BankId)
        {
            var BankCheck = //db.Banks.Where(x => x.AccountNo == Account).Any();
                            (from b in db.Banks
                             join c in db.Accountss on b.AccountId equals c.AccountsID
                             where (b.AccountNo == Account && c.Name == Bankname)
                             select new
                             {
                                 c.Name
                             }).Any();
            var rslt = false;
            if (BankCheck == true)
            {
                if (BankId != null)
                {
                    var bank = db.Banks.Where(x => x.BankId == BankId).FirstOrDefault();
                    BankCheck = (bank.AccountNo == Account) ? false : true;
                }
            }
            rslt = (BankCheck) ? true : false;
            return Json(rslt);
        }

        public ActionResult BulkUpload()
        {
            var viewModel = new BankViewModel();

            return View(viewModel);
        }

        [HttpPost]
        public JsonResult BulkUploadBankAccount(string[][] array)
        {
            bool stat = false;
            string msg = "";

            foreach (var arr in array)
            {
                var name = arr[0];
                var accno = (arr[1]);
                var Exists = (from b in db.Banks
                              join c in db.Accountss on b.AccountId equals c.AccountsID
                              where (b.AccountNo == accno && c.Name == name)
                              select new
                              {
                                  c.Name
                              }).Any();
                if (!Exists)
                {
                    var UserId = User.Identity.GetUserId();
                    long Branch = 0;

                    var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                    var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                    if (BranchCheck == Status.active)
                    {
                    }
                    else
                    {
                        Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                    }
                    Status st = arr[8] == "active" ? Status.active : Status.inactive;
                    DC debcre = arr[9] == "Debit" ? DC.Debit : DC.Credit;
                    var opbal = ((arr[6]) == "") ? 0 : Convert.ToDecimal(arr[6]);
                    Accounts acc = new Accounts();
                    acc.Name = name;
                    acc.Group = 8;
                    acc.Note = arr[7];
                    acc.Status = st;
                    acc.Branch = Branch;
                    acc.Alias = arr[2];
                    if (debcre == DC.Debit)
                    {
                        acc.OpnBalance = opbal;
                        acc.OpnBalanceCr = 0;
                    }
                    if (debcre == DC.Credit)
                    {
                        acc.OpnBalance = 0;
                        acc.OpnBalanceCr = opbal;
                    }


                    Int64 accId = addAccount(acc);// acc.AccountsID;

                    Bank bnk = new Bank();
                    bnk.AccountNo = arr[1];
                    bnk.IbanNo = arr[3];
                    bnk.BranchName = arr[4];
                    bnk.Swift = arr[5];
                    bnk.AccountId = accId;
                    db.Banks.Add(bnk);
                    db.SaveChanges();


                    if (opbal > 0)
                    {
                        if (debcre == DC.Debit)
                        {
                            com.addAccountTrasaction(opbal, 0, accId, "Opening Balance", accId, DC.Debit);

                        }
                        if (debcre == DC.Credit)
                        {
                            com.addAccountTrasaction(0, opbal, accId, "Opening Balance", accId, DC.Credit);
                        }
                    }
                    com.addlog(LogTypes.Created, UserId, "Master", "Banks", findip(), bnk.BankId, "Successfully Created Bank Account");


                    msg = "Bank Account Created Successfully.";
                    stat = true;                   
                }
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpGet]
        public virtual ActionResult DownloadExcel(string file)
        {
            string fullPath = "";
            string fileName = "";

            fullPath = Path.Combine(LegacyWeb.MapPath("~/uploads/excelitem/excelformat/BankAccounts.xlsx"));
            fileName = "BankAccounts.xlsx";
            return File(fullPath, "application/vnd.ms-excel", fileName);
        }

        [HttpGet]
        //[QkAuthorize(Roles = "Dev,View Bank")]
        public ActionResult ViewDetails(long? id)
        {
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                Bank con = db.Banks.Find(id);

                if (con == null)
                {
                    return NotFound();
                }

                BankViewModel acc = new BankViewModel();

                acc = (from a in db.Banks
                       join b in db.Accountss on a.AccountId equals b.AccountsID into cong
                       from b in cong.DefaultIfEmpty()
                       where a.AccountId == con.AccountId
                       select new BankViewModel
                       {
                           Name = b.Name,
                           Alias = b.Alias,
                           AccountNo = a.AccountNo,
                           IbanNo = a.IbanNo,
                           StatusName = b.Status == Status.active ? "active" : "inactive",
                           Note = b.Note,
                           opbal = (b.OpnBalance == 0) ? b.OpnBalanceCr + " Cr" : b.OpnBalance + " Dr",
                           BranchName=a.BranchName,
                           Swift=a.Swift
                       }).FirstOrDefault();

                return PartialView(acc);
            }
        }
        #endregion

        #region expense group

        [QkAuthorize(Roles = "Dev,Expense Group")]
        public ActionResult ExpenseGroup()
        {
            return View();
        }

        // datatable fields listing
        [QkAuthorize(Roles = "Dev,Expense Group")]
        public JsonResult ExpenseGroupGetData()
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

            var ModList = (from a in db.AccountsGroups
                           join c in db.Users on a.CreatedBy equals c.Id into user
                           from c in user.DefaultIfEmpty()
                           where a.Parent == 13
                           select new
                           {
                               id = a.AccountsGroupID,
                               a.Name,
                               a.Alias,
                               a.Parent,
                               Primary = (a.Primary == 0) ? "Yes" : "No",
                               a.Status,
                               a.Editable,
                               ParentName = db.AccountsGroups.Where(a => a.AccountsGroupID == 13).Select(a => a.Name).FirstOrDefault(),
                           });
            ////search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                ModList = ModList.Where(p => p.id.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Name.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Alias.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.ParentName.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Status.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Primary.ToString().ToLower().Contains(search.ToLower()));

            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                ModList = ModList.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = ModList.Count();
            var data = ModList.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        // GET: Field/Create
        [QkAuthorize(Roles = "Dev,Create Expense Group")]
        public ActionResult ExpenseGroupCreate()
        {
            //pass 13-- expense in account gp
            AccountsGroup accgp = new AccountsGroup();
            accgp.Parent = 13;
            return PartialView(accgp);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Create Expense Group")]
        public JsonResult ExpenseGroupCreate(AccountsGroup Acc)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.AccountsGroups.Any(c => c.Name == Acc.Name);
                if (Exists)
                {
                    msg = "Expense Group already exists.";
                    stat = false;
                }
                else
                {
                    var accgp = new AccountsGroup
                    {
                        Name = Acc.Name,
                        Alias = Acc.Alias,
                        Parent = Acc.Parent,
                        //  Primary = Acc.Primary,
                        Status = Acc.Status,
                        //CreatedDate=System.DateTime.Now

                    };
                    db.AccountsGroups.Add(accgp);
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, UserId, "Master", "ExpenseGroups", findip(), accgp.AccountsGroupID, "Expense Group Added Successfully");


                    msg = "Successfully added Expense Group details.";
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

        // GET: dep/Edit/5
        [QkAuthorize(Roles = "Dev,Edit Expense Group")]
        public ActionResult ExpenseGroupEdit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AccountsGroup group = db.AccountsGroups.Find(id);
            if (group == null)
            {
                return NotFound();
            }
            AccountsGroup Acc = new AccountsGroup();

            Acc.AccountsGroupID = group.AccountsGroupID;
            Acc.Name = group.Name;
            Acc.Parent = group.Parent;
            Acc.Status = group.Status;
            Acc.Alias = group.Alias;

            return PartialView(Acc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Edit Expense Group")]
        public JsonResult ExpenseGroupEdit(AccountsGroup Acc, long id)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                var Exists = db.AccountsGroups.Any(c => c.Name == Acc.Name && c.AccountsGroupID != id);
                if (Exists)
                {
                    msg = "Expense Group already exists.";
                    stat = false;
                }
                else
                {
                    AccountsGroup group = db.AccountsGroups.Find(id);

                    group.Name = Acc.Name;
                    group.Parent = Acc.Parent;
                    group.Status = Acc.Status;
                    group.Alias = Acc.Alias;

                    db.Entry(group).State = EntityState.Modified;
                    db.SaveChanges();

                    var UserId = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, UserId, "Master", "ExpenseGroups", findip(), group.AccountsGroupID, "Expense Group Updated Successfully");



                    msg = "Successfully updated Expense Group details.";
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


        // GET: Desg/Delete/5
        [QkAuthorize(Roles = "Dev,Expense Group Delete")]
        public ActionResult ExpenseGroupDelete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AccountsGroup accgp = db.AccountsGroups.Find(id);
            if (accgp == null)
            {
                return NotFound();
            }
            if (accgp.Editable == choice.No)
            {
                return NotFound();
            }

            return PartialView(accgp);
        }

        // POST: Field/Delete/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Expense Group Delete")]
        public JsonResult ExpenseGroupDelete(long id, IFormCollection collection)
        {
            bool stat = false;
            string msg;
            var Exists = db.Accountss.Any(c => c.Group == id);
            if (Exists)
            {
                msg = "Unable to delete ExpenseGroup, accounts with this Expense Group exists.";
                stat = false;
            }
            else
            {
                AccountsGroup accgp = db.AccountsGroups.Find(id);
                db.AccountsGroups.Remove(accgp);
                db.SaveChanges();

                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "Master", "ExpenseGroups", findip(), accgp.AccountsGroupID, "Expense Group Deleted Successfully");


                stat = true;
                msg = "Successfully Deleted Expense Group details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,Expense Group Status")]
        public ActionResult ExpenseGroupStatus(string type, long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AccountsGroup acc = db.AccountsGroups.Find(id);
            if (acc == null)
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
        [QkAuthorize(Roles = "Dev,Expense Group Status")]
        public JsonResult ExpenseGroupStatus(string type, long? id, AccountsGroup Acc)
        {
            bool stat = false;
            string msg;
            string types = "";
            AccountsGroup accgp = db.AccountsGroups.Find(id);
            if (Acc.Status == Status.inactive)
            {
                types = " Inactive";
                accgp.Status = Status.inactive;
            }
            else
            {
                types = " Active";
                accgp.Status = Status.active;
            }

            db.Entry(accgp).State = EntityState.Modified;
            var updates = db.SaveChanges();

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Changed, UserId, "Master", "ExpenseGroups", findip(), accgp.AccountsGroupID, "Successfully Changed the Expense Group Status" + types);


            stat = true;
            msg = " Successfully Changed the Expense Group to" + types;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }




        #endregion

        #region Accounts
        // GET: /CRUD/create/5  
        private long addAccount(Accounts ACS)
        {
            var UserId = User.Identity.GetUserId();
            var account = new Accounts
            {
                Name = ACS.Name,
                Alias = ACS.Alias,
                PrintName = ACS.PrintName,
                OpnBalance = ACS.OpnBalance,
                OpnBalanceCr = ACS.OpnBalanceCr,
                PrevBalance = ACS.PrevBalance,
                Group = ACS.Group,
                Note = ACS.Note,
                Status = ACS.Status,
                Branch = ACS.Branch,
                CreatedDate = System.DateTime.Now,
                CreatedBy = UserId
            };
            db.Accountss.Add(account);
            db.SaveChanges();
            return account.AccountsID;
        }
        private long editAccount(Accounts ACS, long id)
        {
            Accounts account = db.Accountss.Find(id);
            account.PrintName = ACS.PrintName;
            account.Name = ACS.Name;
            account.Alias = ACS.Alias;
            account.OpnBalance = ACS.OpnBalance;
            account.OpnBalanceCr = ACS.OpnBalanceCr;
            account.PrevBalance = ACS.PrevBalance;
            account.Note = ACS.Note;
            account.Status = ACS.Status;
            account.Branch = ACS.Branch;
            db.Entry(account).State = EntityState.Modified;
            db.SaveChanges();

            return db.SaveChanges();
        }
        #endregion

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
