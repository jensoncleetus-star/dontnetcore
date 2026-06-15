using QuickSoft.Web;
using System.Linq.Dynamic.Core;
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
using System.Drawing;
using Microsoft.AspNetCore.Http;

namespace QuickSoft.Controllers
{
    public class operationprocedureController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public operationprocedureController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        public JsonResult ImageDelete(long key)
        {

            bool stat = false;
            string msg;
            AmcDocument tskImg = db.AmcDocuments.Find(key);
            if (tskImg != null)
            {
                db.AmcDocuments.Remove(tskImg);
                db.SaveChanges();
            }
            string fullPath = LegacyWeb.MapPath("~/uploads/sop/" + tskImg.FileName);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            string fullPaththumb = LegacyWeb.MapPath("~/uploads/sop/" + "thumb_" + tskImg.FileName);
            if (System.IO.File.Exists(fullPaththumb))
            {
                System.IO.File.Delete(fullPaththumb);
            }
            string fullPathresize = LegacyWeb.MapPath("~/uploads/sop/" + "resize_" + tskImg.FileName);
            if (System.IO.File.Exists(fullPathresize))
            {
                System.IO.File.Delete(fullPathresize);
            }

            var UserId = User.Identity.GetUserId();

            com.addlog(LogTypes.Deleted, UserId, "sop", "sop", findip(), tskImg.DocumentId, "Image Deleted Successfully");


            Int64 Id = key;
            stat = true;
            msg = "Successfully deleted Image.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }

        [Authorize(Roles = "Dev,Delete SOP")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            sop pro = db.sops.Find(id);
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
           Deletesop(id);

            stat = true;
            msg = "Successfully Deleted Task details.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }
        public void Deletesop(long id)
        {
            var v = db.sops.Where(o=>o.sopid==id);
            db.sops.RemoveRange(v);
            db.SaveChanges();
            var vd = db.sopdets.Where(o => o.sopid == id);
            db.sopdets.RemoveRange(vd);
            db.SaveChanges();
        }
        [HttpPost]
        public ActionResult Edit(sopViewModel vn)
        {
            var vnn = db.sops.Find((long)vn.id);
            long id = (long)vn.id;
            long sopid = id;
                vnn.title = vn.title;
            vnn.note = vn.note;
            vnn.logtime = System.DateTime.Now;
            db.Entry(vnn).State = EntityState.Modified;
            db.SaveChanges();




            IFormFileCollection files = Request.Form.Files;
            if (files.Count > 0)
            {
                string path = LegacyWeb.MapPath("~/uploads/sop/");
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(path);

                for (int i = 0; i < files.Count; i++)
                {
                    IFormFile file = files[i];
                    if (file.Length > 0)
                    {
                        var fileCount = db.AmcDocuments.Select(a => a.DocumentId).AsEnumerable().DefaultIfEmpty(0).Max();
                        var fileName = Path.GetFileName(file.FileName);

                        String extension = Path.GetExtension(fileName);

                        String newName = fileCount + extension;
                        string newFName = fileCount + extension;
                        string newFileName = fileCount + extension;
                        var FStatus = Status.active;
                        var thumbName = "";
                        var resizeName = "";
                        if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                        {
                            thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                            thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/AmcDocuments/"), thumbName);

                            resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                            resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/AmcDocuments/"), resizeName);
                            newFName = "resize_" + newFName;
                            FStatus = Status.inactive;
                        }
                        else
                        {
                            var commonfilename = "Docs-Thump.png";
                        }
                        newName = Path.Combine(LegacyWeb.MapPath("~/uploads/sop/"), newName);
                        file.SaveAs(newName);

                        var AmcImage = new AmcDocument
                        {
                            TransId = sopid,
                            TransType = "sop",
                            FileName = newFileName,//Path.GetFileName(file.FileName),
                            Status = FStatus,
                            CreatedDate = System.DateTime.Now,

                        };
                        db.AmcDocuments.Add(AmcImage);
                        db.SaveChanges();

                        if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                        {
                            Image img = Image.FromFile(newName);
                            int imgHeight = 100;
                            int imgWidth = 100;
                            if (img.Width < img.Height)
                            {
                                //portrait image  
                                imgHeight = 100;
                                var imgRatio = (float)imgHeight / (float)img.Height;
                                imgWidth = Convert.ToInt32(img.Height * imgRatio);
                            }
                            else if (img.Height < img.Width)
                            {
                                //landscape image  
                                imgWidth = 100;
                                var imgRatio = (float)imgWidth / (float)img.Width;
                                imgHeight = Convert.ToInt32(img.Height * imgRatio);
                            }
                            Image thumb = img.GetThumbnailImage(imgWidth, imgHeight, () => false, IntPtr.Zero);
                            thumb.Save(thumbName);

                            Image lgimg = Image.FromFile(newName);
                            if (lgimg.Width > 1800 || lgimg.Height > 1800)
                            {
                                Image imgs = Image.FromFile(newName);
                                System.Drawing.Image thumbs = Common.resizeImage(imgs, new Size(1800, 1800));
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/sop/"), resizeName);
                                thumbs.Save(resizeName);
                            }
                            else
                            {
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/sop/"), resizeName);
                                lgimg.Save(resizeName);
                            }

                        }
                    }
                }
            }




            var oo = db.sopdets.Where(o => o.sopid == id);
                db.sopdets.RemoveRange(oo);
            db.SaveChanges();
            db.Approvals.RemoveRange(db.Approvals.Where(a => a.TransEntry == id && a.Type == "SOP"));
            db.SaveChanges();
            db.ApprovalUpdates.RemoveRange(db.ApprovalUpdates.Where(a => a.TransEntry == id && a.Type == "SOP"));
            db.SaveChanges();
            var reminders = db.Reminders.Where(o => o.Note.Contains("SOP Notice") && o.Reference == id);

            db.Reminders.RemoveRange(reminders);
            db.SaveChanges();
            var userid = User.Identity.GetUserId();
            #region reminders
           var remid=db.Reminders.Where(o => o.Note.Contains("SOP Notice") && o.Reference==id).Select(o=>o.ReminderId).FirstOrDefault();
           
            
            Reminder reminds = new Reminder();
            reminds.Reference = id;
            reminds.Note = "SOP Notice  : " + vn.title;

            //seleted date added,for fullcalender



            reminds.RDate = System.DateTime.Now;
            reminds.Type = "/operationprocedure/mysop/";
            reminds.RStatus = "Close";
            reminds.RequestBy = userid;

            reminds.CreatedBy = userid;
            reminds.Status = Status.active;
            reminds.CreatedDate = System.DateTime.Now;
            db.Reminders.Add(reminds);
            db.SaveChanges();
            long RId = reminds.ReminderId;
            if (!vn.AssignedMembers.Contains(0))
            {
                foreach (var e in vn.AssignedMembers)
                {
                    sopdet sd = new sopdet
                    {
                        employeeid = e,
                        sopid = id,
                    };
                    db.sopdets.Add(sd);
                    db.SaveChanges();

                    Approval approval = new Approval();
                    approval.TransEntry = id;
                    approval.Type = "SOP";
                    approval.EmployeeId = e;
                    db.Approvals.Add(approval);
                    db.SaveChanges();
                              if (1 == 1)
                    {

                        // v.documenttype  "ADMCC Certificate","ADMMC","AMC Contract"




                        if (1 == 1)
                        {








                            db.ReminderAssigneds.RemoveRange(db.ReminderAssigneds.Where(o => o.Type == "sopnotification" && o.ReminderId == remid && o.EmployeeId ==e));
                            db.SaveChanges();

                            ReminderAssigned remAs = new ReminderAssigned();

                            remAs.ReminderId = RId;
                            remAs.EntryId = id;
                            remAs.Type = "sopnotification";
                            remAs.EmployeeId = e;
                            db.ReminderAssigneds.Add(remAs);
                            db.SaveChanges();





                        }





















                    }

                }
            }
            else
            {
                sopdet sd = new sopdet
                {
                    employeeid = 0,
                    sopid = id,
                };
                db.sopdets.Add(sd);
                db.SaveChanges();
                var em = db.Employees.Where(o => o.Status == 1).Select(o => o.EmployeeId).ToArray();
                db.ReminderAssigneds.RemoveRange(db.ReminderAssigneds.Where(o => o.Type == "sopnotification" && o.ReminderId == remid));
                db.SaveChanges();
                foreach (var e in em)
                {
                  

                    Approval approval = new Approval();
                    approval.TransEntry = id;
                    approval.Type = "SOP";
                    approval.EmployeeId = e;
                    db.Approvals.Add(approval);
                    db.SaveChanges();
                   
                    if (1 == 1)
                    {

                        // v.documenttype  "ADMCC Certificate","ADMMC","AMC Contract"




                        if (1 == 1)
                        {









                           

                            ReminderAssigned remAs = new ReminderAssigned();

                            remAs.ReminderId = RId;
                            remAs.EntryId = id;
                            remAs.Type = "sopnotification";
                            remAs.EmployeeId = e;
                            db.ReminderAssigneds.Add(remAs);
                            db.SaveChanges();





                        }





















                    }

                }
            }
            #endregion
            Success("updated", true);
            return RedirectToAction("Index");

        }
       // [Authorize(Roles = "Dev,Edit SOP")]
        public ActionResult Edit(long id)
        {
            sopViewModel vmodel = new sopViewModel();
            var v = db.sops.Find(id);
            vmodel.note = v.note;
            vmodel.title = v.title;
            vmodel.id = id;
          
            var assmebers = db.sopdets.Where(o => o.sopid == id).Select(o => o.employeeid).Distinct().ToArray() ?? null;
            List<SelectFormat> serialisedJson;
            serialisedJson = db.Employees
                   .Select(s => new SelectFormat
                   {
                       id = s.EmployeeId,
                       text = s.FirstName + " " + s.LastName
                   })
                   .ToList();
            var initial = new SelectFormat() { id = 0, text = "All" };
            serialisedJson.Insert(0, initial);

            ViewBag.team = new MultiSelectList(serialisedJson, "id", "text",assmebers);
            ViewBag.image = (from b in db.AmcDocuments
                             
                             where b.TransType=="sop" && b.TransId  == id
                             select new quotationdocumentviewmodel
                             {
                                 qutid = b.DocumentId,
                                 quotationID = b.TransId,
                                 FileName = b.FileName,
                             }).ToList();
            return View(vmodel);
        }  
      //  [Authorize(Roles = "Dev,Create SOP")]
        public ActionResult create()
        {
            sopViewModel vmodel = new sopViewModel();
            List<SelectFormat> serialisedJson;
             serialisedJson = db.Employees
                    .Select(s => new SelectFormat
                    {
                        id = s.EmployeeId,
                        text = s.FirstName + " " + s.LastName
                    })
                    .ToList();
            var initial = new SelectFormat() { id = 0, text = "All" };
            serialisedJson.Insert(0, initial);
     
            ViewBag.team = new MultiSelectList(serialisedJson, "id", "text");

            return View(vmodel);
        }
        public string ViewNote(long id)
        {
            var v = db.sops.Where(o => o.sopid == id).Select(o => o.note).FirstOrDefault();
            var doc = (from b in db.AmcDocuments

                             where b.TransType == "sop" && b.TransId == id
                             select new quotationdocumentviewmodel
                             {
                                 qutid = b.DocumentId,
                                 quotationID = b.TransId,
                                 FileName = b.FileName,
                             }).ToList();
           
            if(doc.Count()>0)
            {
                string str = "";
                foreach(var d in doc)
                {
                    str+= "<a href='/uploads/sop/" + d.FileName + "'>Download</a><br>";



                }
                v = v + str;
            }
            return v;

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


            var v = (from a in db.sops.AsEnumerable()
                     select new
                     {
                         a.sopid,
                         a.title,
                         ldate=a.logtime,
                         AssignedTo = (from z in db.sopdets
                                       join y in db.Employees on z.employeeid equals y.EmployeeId
                                       where z.sopid == a.sopid
                                       select new
                                       {
                                           id = y.EmployeeId,
                                           LastName = (y.LastName != null) ? y.LastName : "",
                                           FirstName = (y.FirstName != null) ? y.FirstName : "",
                                           MiddleName = (y.MiddleName != null) ? y.MiddleName : "",


                                       }).Distinct().ToList(),
                     }).OrderByDescending(a => a.ldate).ToList();
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
               v=v.Where(o => o.title.ToUpper().Contains(search.ToUpper())).ToList();
            }
            //SORT
           
            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        [RedirectingAction]
        [HttpPost]
        public ActionResult GetmyDetails(long? AmcId, string PDate)
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
            Employee empl = new Employee();
            empl.EmployeeId = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var assemployeesuserid = (from z in db.sopdets
                                      join y in db.Employees on z.employeeid equals y.EmployeeId
                                      join x in db.Users on y.UserId equals x.Id
                                      where x.Id == UserId
                                      select new
                                      {
                                          y.EmployeeId

                                      }).Select(o => o.EmployeeId).FirstOrDefault();
                 
            var v = (from a in db.sops.AsEnumerable()
                     let assign = db.sopdets.Where(x => x.sopid == a.sopid).Select(x => x.employeeid).ToList()
                     let app = db.Approvals.Where(x => x.TransEntry == a.sopid && x.Type == "SOP").Select(x => x.EmployeeId).ToList()
                     let AppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.sopid && x.Type == "SOP").Select(x => x.ApprovalStatus).ToList()
                     let chkAppStatus = db.ApprovalUpdates.Where(x => x.TransEntry == a.sopid && x.Type == "SOP").AsEnumerable().GroupBy(l => l.ApprovedBy)
                                        .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                        .ToList().Select(x => x.ApprovalStatus).ToList()


                     where (assign.Contains(assemployeesuserid)|| assign.Contains(0))
                     select new
                     {
                         a.sopid,
                         a.title,
                         ldate = a.logtime,
                         app = app,

                         AppStatus = AppStatus,
                         chkAppStatus = chkAppStatus,
                         AssignedTo = (from z in db.sopdets
                                       join y in db.Employees on z.employeeid equals y.EmployeeId
                                       where z.sopid == a.sopid
                                       select new
                                       {
                                           id = y.EmployeeId,
                                           LastName = (y.LastName != null) ? y.LastName : "",
                                           FirstName = (y.FirstName != null) ? y.FirstName : "",
                                           MiddleName = (y.MiddleName != null) ? y.MiddleName : "",


                                       }).Distinct().ToList(),
                     }).OrderByDescending(a => a.ldate).ToList().Select(o => new
                     {
o.sopid,
o.AssignedTo,
o.app,
                         Approval = (o.app != null && empl.EmployeeId != null) ? ((o.app.Contains(empl.EmployeeId)||o.app.Contains(0)) ? true : false) : false,
                         ApprovalStatus = (o.app.Count > 0 && o.AppStatus.Count > 0 && o.chkAppStatus.Count > 0) ? (o.chkAppStatus.Contains(ApprovalStatus.Rejected) ? ApprovalStatus.Rejected : (o.app != null && o.chkAppStatus != null && o.app.Count != 0 && o.chkAppStatus.Count != 0 ? ApprovalStatus.Approved : ApprovalStatus.PendingApproval)) : ApprovalStatus.PendingApproval,

                         o.ldate,

o.title,
                     });
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
        public ActionResult create(sopViewModel vm)
        {
            sop s = new sop
            {
                note = vm.note,
                title = vm.title,
                logtime=System.DateTime.Now

            };
            db.sops.Add(s);
            db.SaveChanges();
            var sopid = s.sopid;




            IFormFileCollection files = Request.Form.Files;
            if (files.Count > 0)
            {
                string path = LegacyWeb.MapPath("~/uploads/sop/");
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(path);

                for (int i = 0; i < files.Count; i++)
                {
                    IFormFile file = files[i];
                    if (file.Length > 0)
                    {
                        var fileCount = db.AmcDocuments.Select(a => a.DocumentId).AsEnumerable().DefaultIfEmpty(0).Max();
                        var fileName = Path.GetFileName(file.FileName);

                        String extension = Path.GetExtension(fileName);

                        String newName = fileCount + extension;
                        string newFName = fileCount + extension;
                        string newFileName = fileCount + extension;
                        var FStatus = Status.active;
                        var thumbName = "";
                        var resizeName = "";
                        if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                        {
                            thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                            thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/AmcDocuments/"), thumbName);

                            resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                            resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/AmcDocuments/"), resizeName);
                            newFName = "resize_" + newFName;
                            FStatus = Status.inactive;
                        }
                        else
                        {
                            var commonfilename = "Docs-Thump.png";
                        }
                        newName = Path.Combine(LegacyWeb.MapPath("~/uploads/sop/"), newName);
                        file.SaveAs(newName);

                        var AmcImage = new AmcDocument
                        {
                            TransId = sopid,
                            TransType = "sop",
                            FileName = newFileName,//Path.GetFileName(file.FileName),
                            Status = FStatus,
                            CreatedDate = System.DateTime.Now,

                        };
                        db.AmcDocuments.Add(AmcImage);
                        db.SaveChanges();

                        if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                        {
                            Image img = Image.FromFile(newName);
                            int imgHeight = 100;
                            int imgWidth = 100;
                            if (img.Width < img.Height)
                            {
                                //portrait image  
                                imgHeight = 100;
                                var imgRatio = (float)imgHeight / (float)img.Height;
                                imgWidth = Convert.ToInt32(img.Height * imgRatio);
                            }
                            else if (img.Height < img.Width)
                            {
                                //landscape image  
                                imgWidth = 100;
                                var imgRatio = (float)imgWidth / (float)img.Width;
                                imgHeight = Convert.ToInt32(img.Height * imgRatio);
                            }
                            Image thumb = img.GetThumbnailImage(imgWidth, imgHeight, () => false, IntPtr.Zero);
                            thumb.Save(thumbName);

                            Image lgimg = Image.FromFile(newName);
                            if (lgimg.Width > 1800 || lgimg.Height > 1800)
                            {
                                Image imgs = Image.FromFile(newName);
                                System.Drawing.Image thumbs = Common.resizeImage(imgs, new Size(1800, 1800));
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/sop/"), resizeName);
                                thumbs.Save(resizeName);
                            }
                            else
                            {
                                resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/sop/"), resizeName);
                                lgimg.Save(resizeName);
                            }

                        }
                    }
                }
            }















            var userid = User.Identity.GetUserId();
            #region reminders
            var vn = vm;
            var id = sopid;
            Reminder reminds = new Reminder();
            reminds.Reference = id;
            reminds.Note = "SOP Notice  : " + vn.title;

            //seleted date added,for fullcalender



            reminds.RDate = System.DateTime.Now;
            reminds.Type = "/operationprocedure/mysop/";
            reminds.RStatus = "Close";
            reminds.RequestBy = userid;

            reminds.CreatedBy = userid;
            reminds.Status = Status.active;
            reminds.CreatedDate = System.DateTime.Now;
            db.Reminders.Add(reminds);
            db.SaveChanges();
            long RId = reminds.ReminderId;
        
           
            if (!vn.AssignedMembers.Contains(0))
            {
                foreach (var e in vn.AssignedMembers)
                {
                    sopdet sd = new sopdet
                    {
                        employeeid = e,
                        sopid = id,
                    };
                    db.sopdets.Add(sd);
                    db.SaveChanges();

                    Approval approval = new Approval();
                    approval.TransEntry = id;
                    approval.Type = "SOP";
                    approval.EmployeeId = e;
                    db.Approvals.Add(approval);
                    db.SaveChanges();
                    if (1 == 1)
                    {

                        // v.documenttype  "ADMCC Certificate","ADMMC","AMC Contract"




                        if (1 == 1)
                        {










                            ReminderAssigned remAs = new ReminderAssigned();

                            remAs.ReminderId = RId;
                            remAs.EntryId = id;
                            remAs.Type = "sopnotification";
                            remAs.EmployeeId = e;
                            db.ReminderAssigneds.Add(remAs);
                            db.SaveChanges();





                        }





















                    }

                }
            }
            else
            {
                sopdet sd = new sopdet
                {
                    employeeid = 0,
                    sopid = id,
                };
                db.sopdets.Add(sd);
                db.SaveChanges();
                var em = db.Employees.Where(o => o.Status == 1).Select(o => o.EmployeeId).ToArray();

                foreach (var e in em)
                {


                    Approval approval = new Approval();
                    approval.TransEntry = id;
                    approval.Type = "SOP";
                    approval.EmployeeId = e;
                    db.Approvals.Add(approval);
                    db.SaveChanges();

                    if (1 == 1)
                    {

                        // v.documenttype  "ADMCC Certificate","ADMMC","AMC Contract"




                        if (1 == 1)
                        {










                            ReminderAssigned remAs = new ReminderAssigned();

                            remAs.ReminderId = RId;
                            remAs.EntryId = id;
                            remAs.Type = "sopnotification";
                            remAs.EmployeeId = e;
                            db.ReminderAssigneds.Add(remAs);
                            db.SaveChanges();





                        }





















                    }

                }
            }
            #endregion
            Success("success", true);
            return RedirectToAction("create");
        }
     //   [Authorize(Roles = "Dev,List SOP")]
        public ActionResult Index()
        {
            return View();
        }
    
   public ActionResult mysop()
    {
        return View();
    }
        public ActionResult EditStatus(long id)
        {
            var UserId = User.Identity.GetUserId();
            var appstat = db.ApprovalUpdates.Where(x => x.TransEntry == id && x.Type == "SOP" && x.ApprovedBy == UserId).GroupBy(l => l.ApprovedBy)
                                       .Select(g => g.OrderByDescending(c => c.CreatedDate).FirstOrDefault())
                                       .ToList();

            var Stat = from ApprovalStatus e in Enum.GetValues(typeof(ApprovalStatus))
                       where e != ApprovalStatus.PendingApproval && (appstat.Count == 0 || e != appstat.Select(a => a.ApprovalStatus).FirstOrDefault())
                       select new
                       {
                           ID = (int)e,
                           Name = e.ToString()
                       };
            ViewBag.AppStat = QkSelect.List(Stat, "Id", "Name");

            return PartialView();
        }

        [HttpPost]
        public ActionResult EditStatus(ApprovalUpdate App, long id)
        {
            bool stat = false;
            string msg = "";
            var UserId = User.Identity.GetUserId();

            var MR = db.sops.Where(a => a.sopid == id).FirstOrDefault();
            var chkappby = db.ApprovalUpdates.Where(a => a.ApprovedBy == UserId && a.TransEntry == id && a.Type == "SOP").OrderByDescending(a => a.CreatedDate).FirstOrDefault();
           
      
            if (App.ApprovalStatus==ApprovalStatus.Approved)
            {
                ApprovalUpdate AppUp = new ApprovalUpdate();
                AppUp.ApprovalStatus = App.ApprovalStatus;
                AppUp.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                AppUp.ApprovedBy = UserId;
                AppUp.Note = App.Note;
                AppUp.RequestBy = UserId;
                AppUp.Status = Status.active;
                AppUp.TransEntry = id;
                AppUp.Type = "SOP";

                db.ApprovalUpdates.Add(AppUp);
                db.SaveChanges();
                var emid = db.Employees.Where(o => o.UserId == UserId).Select(o => o.EmployeeId).FirstOrDefault();
                db.ReminderAssigneds.RemoveRange(db.ReminderAssigneds.Where(o => o.Type == "sopnotification"&&o.EmployeeId== emid&&o.EntryId==id));
                db.SaveChanges();
                stat = true;
                msg = "Successfully Updated Status.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                stat = false;
                msg = "Updating Same Status.";
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }

        }

    }
}
