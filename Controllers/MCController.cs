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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class MCController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public MCController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Contact
        [QkAuthorize(Roles = "Dev,MC List")]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,MC List")]
        public JsonResult GetData(string Mcs)
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
            var uEdit = User.IsInRole("Edit MC");
            var uDownload = User.IsInRole("Download MC");
            var uDelete = User.IsInRole("Delete MC");

            var v = (from a in db.MCs
                     join b in db.Users on a.CreatedBy equals b.Id
                     join c in db.Users on a.AssignedUser equals c.Id into asuser
                     from c in asuser.DefaultIfEmpty()
                     where (Mcs == null || Mcs == "" || a.MCName == Mcs)
                     select new
                     {
                         id = a.MCId,
                         a.MCName,
                         a.MCCode,
                         a.Note,
                         a.Status,
                         a.Editable,
                         CreatedBy = b.Name,
                         a.CreatedDate,
                         AsUser = c.UserName,
                         Dev = uDev,
                         Edit = uEdit,
                         Download = uDownload,
                         Delete = uDelete
                     });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.MCCode.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.MCName.ToString().ToLower().Contains(search.ToLower()));
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

        [QkAuthorize(Roles = "Dev,Create MC")]
        public ActionResult Create()
        {
            var viewModel = new MC
            {
                MCCode = MCCodes()
            };



            var use = (from b in db.Users
                       where b.Discount==null
                       //where b.Id != c.AssignedUser
                       select new
                       {
                           Name = b.UserName, //each json object will have 
                           ID = b.Id
                       }).OrderBy(b => b.Name).ToList();
            ViewBag.Users = QkSelect.List(use, "ID", "Name");

            var EnableUserMC = db.EnableSettings.Where(a => a.EnableType == "AssignUserMC").FirstOrDefault();
            var MCUsercheck = EnableUserMC != null ? EnableUserMC.Status : Status.inactive;
            ViewBag.EnableUserMC = MCUsercheck;

            return PartialView(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Create MC")]
        public JsonResult Create(MC material)
        {
            bool stat = false;
            string msg;
            Int64 id = 0;
            if (ModelState.IsValid)
            {
                var contactExists = db.MCs.Any(u => u.MCName == material.MCName);
                var ItemCodeExists = db.MCs.Any(u => u.MCCode == material.MCCode);
                if (contactExists)
                {
                    msg = "Material Center Name exists.";
                    stat = false;
                }
                else if (ItemCodeExists)
                {

                    msg = "A Item with same Item code exists.";
                    stat = false;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                    var con = new MC
                    {
                        MCName = material.MCName,
                        MCCode = material.MCCode,
                        Note = material.Note,
                        Status = material.Status,
                        AssignedUser = material.AssignedUser,
                        CreatedBy = UserId,
                        CreatedDate = Convert.ToDateTime(System.DateTime.Now),
                        CreatedBranch = BranchID
                    };
                    db.MCs.Add(con);
                    db.SaveChanges();
                    id = con.MCId;

                    com.addlog(LogTypes.Created, UserId, "MC", "MCs", findip(), con.MCId, "MC Added Successfully");
                    msg = "Successfully added MC details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, id } };
        }


        // GET: contact/Edit/5
        [QkAuthorize(Roles = "Dev,Edit MC")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MC con = db.MCs.Find(id);

            if (con == null)
            {
                return NotFound();
            }
            var stands = db.MCs.Where(s => s.MCId != id)
                          .Select(s => new
                          {
                              FieldID = s.MCName,
                              FieldName = s.MCId
                          })
                         .ToList();

            var use = (from b in db.Users
                       //where(b.Id == con.AssignedUser || b.Id != c.AssignedUser)
                       select new
                       {
                           Name = b.UserName, //each json object will have 
                           ID = b.Id
                       }).OrderBy(b => b.Name).ToList();


            ViewBag.Users = QkSelect.List(use, "ID", "Name");

            MC conGp = new MC();
            conGp.MCName = con.MCName;
            conGp.MCCode = con.MCCode;
            conGp.Note = con.Note;
            conGp.Status = con.Status;
            conGp.AssignedUser = con.AssignedUser;


            var EnableUserMC = db.EnableSettings.Where(a => a.EnableType == "AssignUserMC").FirstOrDefault();
            var MCUsercheck = EnableUserMC != null ? EnableUserMC.Status : Status.inactive;
            ViewBag.EnableUserMC = MCUsercheck;

            return PartialView(conGp);
        }

        // POST: contact/Edit/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit MC")]
        public JsonResult Edit(MC con, long id)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                MC conGp = db.MCs.Find(id);
                var contactExists = db.MCs.Any(u => u.MCName == con.MCName && u.MCId != conGp.MCId);
                if (contactExists)
                {
                    msg = "A Contact with same Name exists.";
                    stat = false;
                }
                else
                {
                    conGp.MCName = con.MCName;
                    conGp.MCCode = con.MCCode;
                    conGp.Note = con.Note;
                    conGp.Status = con.Status;
                    conGp.AssignedUser = con.AssignedUser;
                    db.Entry(conGp).State = EntityState.Modified;
                    db.SaveChanges();

                    var userid = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, userid, "MC", "MCs", findip(), conGp.MCId, "MC Updated Successfully");


                    msg = "Successfully Updated Material Center Details.";
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

        //  GET:  Material Center/Delete/5
        [QkAuthorize(Roles = "Dev,Delete MC")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MC congp = db.MCs.Find(id);
            if (congp == null)
            {
                return NotFound();
            }

            return PartialView(congp);
        }

        // POST:tax Material Center/Delete/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete MC")]
        public JsonResult Delete(long id, IFormCollection collection)
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
                MC congp = db.MCs.Find(id);
                if (congp != null)
                {
                    db.MCs.RemoveRange(db.MCs.Where(a => a.MCId == id));
                }

                var userid = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, userid, "MC", "MCs", findip(), congp.MCId, "MC Deleted Successfully");
                db.SaveChanges();

                stat = true;
                msg = "Successfully Deleted Material Center details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            if (db.SalesEntrys.Any(c => c.MaterialCenter == id))
            {
                msg = "MaterialCenter Already used in Sales Entry !!";
            }
            else if (db.SalesReturns.Any(c => c.MaterialCenter == id))
            {
                msg = "Item Bundle Already used in Sales Return !!";
            }
            else if (db.ProFormas.Any(c => c.MaterialCenter == id))
            {
                msg = "Item Bundle Already used in ProForma !!";
            }
            else if (db.Deliverynotes.Any(c => c.MaterialCenter == id))
            {
                msg = "Item Bundle Already used in Deliverynote !!";
            }
            else if (db.PurchaseEntrys.Any(c => c.MaterialCenter == id))
            {
                msg = "Item Bundle Already used in Purchase Entry !!";
            }
            else if (db.PurchaseReturns.Any(c => c.MaterialCenter == id))
            {
                msg = "Item Bundle Already used in Purchase Return !!";
            }
            else if (db.Productions.Any(c => c.MaterialCenter == id))
            {
                msg = "Item Bundle Already used in Production !!";
            }
            else if (db.Unassembles.Any(c => c.MaterialCenter == id))
            {
                msg = "Item Bundle Already used in Unassemble !!";
            }
            else if (db.StockJournals.Any(c => c.MCFrom == id) || db.StockJournals.Any(c => c.MCTo == id))
            {
                msg = "Item Bundle Already used in Stock Journal !!";
            }
            else if (db.StockAdjustments.Any(c => c.MaterialCenter == id))
            {
                msg = "Item Bundle Already used in Stock Adjustments !!";
            }
            else
            {
                msg = null;
            }

            return msg;
        }






        public JsonResult SearchMCUser(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            List<SelectFormat> serialisedJson2;
            string stt = "All";
            var UserId = User.Identity.GetUserId();

            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            var mchkadditional = db.AdditionalMc.Where(a => a.UserId == UserId).FirstOrDefault();
            if (mcchk != null || mchkadditional!=null)
            {
                if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
                {
                    serialisedJson = db.MCs.Where(p => p.AssignedUser == UserId && p.Status == Status.active && (p.MCName.ToLower().Contains(q.ToLower()) || p.MCName.Contains(q)))
                                      .Select(b => new SelectFormat
                                      {
                                          text = b.MCName,
                                          id = b.MCId
                                      }).OrderBy(b => b.text).ToList();
                    serialisedJson2 = db.AdditionalMc.Where(p => p.UserId == UserId && (p.McName.ToLower().Contains(q.ToLower()) || p.McName.Contains(q))).Select(b => new SelectFormat
                    {
                        text = b.McName,
                        id = b.McId,
                    }).OrderBy(b => b.text).ToList();
                    serialisedJson = serialisedJson2.Union(serialisedJson).ToList();
                }
                else
                {
                    serialisedJson = db.MCs.Where(p => p.AssignedUser == UserId && p.Status == Status.active).Select(b => new SelectFormat
                    {
                        text = b.MCName,
                        id = b.MCId
                    }).OrderBy(b => b.text).ToList();
                    serialisedJson2 = db.AdditionalMc.Where(p => p.UserId == UserId).Select(b => new SelectFormat
                    {
                        text = b.McName,
                        id = b.McId,
                    }).OrderBy(b => b.text).ToList();

                    serialisedJson = serialisedJson2.Union(serialisedJson).ToList();
                }//          
            }
            else
            {
                if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
                {
                    serialisedJson = db.MCs.Where(p => p.Status == Status.active).Where(p => p.MCName.ToLower().Contains(q.ToLower()) || p.MCName.Contains(q))
                                      .Select(b => new SelectFormat
                                      {
                                          text = b.MCName,
                                          id = b.MCId
                                      }).OrderBy(b => b.text).ToList();
                }
                else
                {
                    serialisedJson = db.MCs.Where(p => p.Status == Status.active).Select(b => new SelectFormat
                    {
                        text = b.MCName,
                        id = b.MCId
                    }).OrderBy(b => b.text).ToList();

                }//          
            }
            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }












        //            serialisedJson = db.MCs.Where(p => p.AssignedUser == UserId && (p.MCName.ToLower().Contains(q.ToLower()) || p.MCName.Contains(q)))
        //                              .Select(b => new SelectFormat
        //                                  text = b.MCName,
        //                                  id = b.MCId
        //            serialisedJson = db.MCs.Where(p => p.AssignedUser == UserId).Select(b => new SelectFormat
        //                text = b.MCName, 
        //                id = b.MCId

        //        }//          
        //            serialisedJson = db.MCs.Where(p => p.MCName.ToLower().Contains(q.ToLower()) || p.MCName.Contains(q))
        //                              .Select(b => new SelectFormat
        //                                  text = b.MCName, 
        //                              id = b.MCId
        //            serialisedJson = db.MCs.Select(b => new SelectFormat
        //                text = b.MCName, 
        //                id = b.MCId

        //        }//          
        public JsonResult SearchMCwithoutall(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.MCs.Where(p => p.MCName.ToLower().Contains(q.ToLower()) || p.MCName.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.MCName, //each json object will have 
                                      id = b.MCId
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.MCs.Select(b => new SelectFormat
                {
                    text = b.MCName, //each json object will have 
                    id = b.MCId
                }).OrderBy(b => b.text).ToList();

            }//          


            return Json(serialisedJson);
        }
        public JsonResult SearchMC(string q, string x,string sttransfer="no")
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";

            var UserId = User.Identity.GetUserId();
            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId && a.Status == Status.active).FirstOrDefault();


            List<SelectFormat> serialisedJson2;
            var mchkadditional = db.AdditionalMc.Where(a => a.UserId == UserId).FirstOrDefault();
            if (sttransfer=="noo")
            {
                //    Id = s.MCId,
                //    Name = s.MCName

                serialisedJson = db.MCs.Where(p => p.AssignedUser == UserId && p.Status == Status.active).Select(b => new SelectFormat
                {
                    text = b.MCName,
                    id = b.MCId
                }).OrderBy(b => b.text).ToList();
                serialisedJson2 = db.AdditionalMc.Where(p => p.UserId == UserId ).Select(b => new SelectFormat
                {
                    text = b.McName,
                    id = b.McId,
                }).OrderBy(b => b.text).ToList();

                serialisedJson = serialisedJson2.Union(serialisedJson).ToList();
            }
            else
            {
                if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
                {
                    serialisedJson = db.MCs.Where(p => p.Status == Status.active && p.MCName.ToLower().Contains(q.ToLower()) || p.MCName.Contains(q))
                                      .Select(b => new SelectFormat
                                      {
                                          text = b.MCName, //each json object will have 
                                      id = b.MCId
                                      }).OrderBy(b => b.text).ToList();
                }
                else
                {
                    serialisedJson = db.MCs.Where(p => p.Status == Status.active).Select(b => new SelectFormat
                    {
                        text = b.MCName, //each json object will have 
                        id = b.MCId
                    }).OrderBy(b => b.text).ToList();

                }//
                 //  
                if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
                {
                    var initial = new SelectFormat() { id = 0, text = stt };
                    serialisedJson.Insert(0, initial);
                }
            }
      

            return Json(serialisedJson);
        }
        [HttpGet]
        public ActionResult ChangeStatus(string type, long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MC material = db.MCs.Find(id);
            if (material == null)
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
        public JsonResult ChangeStatus(string type, long? id, Branch brn)
        {
            bool stat = false;
            string msg;
            string types = "";
            MC material = db.MCs.Find(id);
            if (brn.Status == Status.inactive)
            {
                types = " Inactive";
                material.Status = Status.inactive;
            }
            else
            {
                types = " Active";
                material.Status = Status.active;
            }

            db.Entry(material).State = EntityState.Modified;
            db.SaveChanges();

            var UserId = User.Identity.GetUserId();
            stat = true;
            msg = " Successfully Changed the Material Center to" + types;
            com.addlog(LogTypes.Changed, UserId, "MC", "MCs", findip(), material.MCId, msg);

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        private string MCCodes(Int64 INo = 0, string ICode = null)
        {
            var prefix = db.CodePrefixs.Where(a => a.section == "MC").Select(a => a.prefix).FirstOrDefault();
            if (ICode == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "MC").Select(a => a.number).FirstOrDefault();
                if ((db.MCs.Select(p => p.MCId).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    ICode = (number == 0)?prefix + 1 :(prefix + number);
                }
                else
                {
                    INo = db.MCs.Max(p => p.MCId + 1);
                    ICode = prefix + INo;
                    if (CodeExist(ICode))
                    {
                        ICode = MCCodes(INo, ICode);
                    }

                }
            }
            else
            {
                INo = INo + 1;
                ICode = prefix + INo;
                if (CodeExist(ICode))
                {
                    ICode = MCCodes(INo, ICode);
                }
            }
            return ICode;
        }
        private bool CodeExist(string Code)
        {
            var Exists = db.MCs.Any(c => c.MCCode == Code);
            bool res = (Exists) ? true : false;
            return res;
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
