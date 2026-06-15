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
using Microsoft.AspNetCore.Http;
using System.Drawing;

namespace QuickSoft.Controllers
{
    public class filedocumentstoreController :  BaseController
    {
        ApplicationDbContext db;
        Common com;
        public filedocumentstoreController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }  // GET: Password
        public ActionResult AddamcImage(long id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var docdownload = db.AmcDocuments.Where(a => a.TransId == id).ToList();
         
                  var filedoc = (from m in db.MultipleDocuments
                                           
                                           where m.RelationID == id && m.DocumentName == "commondocument"
                          
                                           select new FilemultipleDocumentsview
                                           {
                                              
                                              Id=m.Id,
                                              Document=m.Document,
                                  
                                              CreatedBy=m.CreatedBy,
                                              CreatedDate=m.CreatedDate,
                                              Status=m.Status



                                             

                                           }
                                        ).Distinct().ToList();
                


           

            return PartialView(filedoc);
        }
        public JsonResult GetData( long empid)
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


            // EF Core 10 can't translate the nested-collection projection (the "let AssignedTo = (...).ToList()"
            // assigned-users list inside the executed select new {}) nor the .Where() that filters on that client
            // collection. SERVER: materialize only entity scalars; CLIENT: build the assigned-users lookup keyed by
            // filedocumentDataId and re-project with the SAME member names/order so the grid columns + JSON are unchanged.
            var serverRows = (from a in db.filedocumentdetails
                              select new
                              {
                                  a.createdby,
                                  a.createddate,
                                  a.filedocumentDataId,
                                  a.Title,
                                  a.Notes,
                              }).ToList();

            var idList = serverRows.Select(o => o.filedocumentDataId).ToList();
            var assignLookup = (from z in db.filedocumentdetailsas
                                join y in db.Employees on z.employeeid equals y.EmployeeId
                                where idList.Contains(z.filedocumentdetailid)
                                select new
                                {
                                    z.filedocumentdetailid,
                                    id = y.EmployeeId,
                                    LastName = (y.LastName != null) ? y.LastName : "",
                                    FirstName = (y.FirstName != null) ? y.FirstName : "",
                                    MiddleName = (y.MiddleName != null) ? y.MiddleName : "",
                                }).Distinct().ToList().ToLookup(z => z.filedocumentdetailid);

            var ModList = serverRows.Select(a => new
                           {

                               a.createdby,
                               a.createddate,
                             a.filedocumentDataId,
                               a.Title,
                               a.Notes,
                               AssignedTo = assignLookup[a.filedocumentDataId]
                                            .Select(z => new { z.id, z.LastName, z.FirstName, z.MiddleName })
                                            .Distinct().ToList(),


                           }).Where(x => empid == 0 || empid == null || x.AssignedTo.Select(z => z.id).ToList().Contains(empid));

            ////search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                ModList = ModList.Where(p =>     p.Notes.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Title.ToString().ToLower().Contains(search.ToLower()));

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
        public JsonResult GetmyData(long empid)
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

            var UserId = User.Identity.GetUserId();
            var empIdd = db.Employees.Where(a => a.UserId == UserId).Select(a => a.EmployeeId).FirstOrDefault();

            var ModList = (from a in db.filedocumentdetails
                           join b in db.filedocumentdetailsas on a.filedocumentDataId equals b.filedocumentdetailid into fass
                           from b in fass.DefaultIfEmpty()
                           where b.employeeid==empIdd
                      

                           select new
                           {

                               a.createdby,
                               a.createddate,
                               a.filedocumentDataId,
                               a.Title,
                               a.Notes,
                       


                           }).Distinct();

            ////search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                ModList = ModList.Where(p => p.Notes.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Title.ToString().ToLower().Contains(search.ToLower()));

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

        // GET: filedocumentstore
        public ActionResult Index()
        {
            ViewBag.Employee = QkSelect.List(
                          new List<SelectListItem>
                          {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                          }, "Value", "Text", 1);

            return View();
        }

        public ActionResult Delete(long id)
        {
            var pro = db.filedocumentdetails.Find(id);
            return PartialView(pro);
        }
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;

            var UserId = User.Identity.GetUserId();
            Deletepass(id);

            stat = true;
            msg = "Successfully Deleted Task details.";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

        }
        public void Deletepass(long id)
        {
            var filess = db.MultipleDocuments.Where(o => o.RelationID == id && o.DocumentName == "commondocument").Select(o=>o.Id).ToList().ToArray();
            foreach (var i in filess)
            {
                ImageDelete(i);
            }
              var v = db.filedocumentdetails.Where(o => o.filedocumentDataId == id);
            db.filedocumentdetails.RemoveRange(v);
            db.SaveChanges();

        }


    public JsonResult ImageDelete(long key)
        {
            bool stat = false;
            string msg;
            FilemultipleDocuments tskImg = db.MultipleDocuments.Find(key);
            if (tskImg != null)
            {
                db.MultipleDocuments.Remove(tskImg);
                db.SaveChanges();
            }
            string fullPath = LegacyWeb.MapPath("~/uploads/commondocuments/" + tskImg.Document);
       

            string fullPaththumb = LegacyWeb.MapPath("~/uploads/commondocuments/" + "thumb_" + tskImg.Document);
            if (System.IO.File.Exists(fullPaththumb))
            {
                System.IO.File.Delete(fullPaththumb);
            }

            var UserId = User.Identity.GetUserId();

            com.addlog(LogTypes.Deleted, UserId, "filedocument", "filecommon", findip(), tskImg.RelationID, "Task Image Deleted Successfully");


            Int64 Id = key;
            stat = true;
            msg = "Successfully deleted ";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }

        public ActionResult Edit(long id)
        {
            var fi = db.filedocumentdetails.Find(id);
            var ass = db.filedocumentdetailsas.Where(o => o.filedocumentdetailid == id).Select(o => o.employeeid).ToList().ToArray() ?? null;
            List<SelectFormat> serialisedJson;
            filedocumentsViewModel vmodel = new filedocumentsViewModel
            {
                filedocumentsid=id,
                Title = fi.Title,
                Notes = fi.Notes

            };

            serialisedJson = db.Employees
                   .Select(s => new SelectFormat
                   {
                       id = s.EmployeeId,
                       text = s.FirstName + " " + s.LastName
                   })
                   .ToList();
            var initial = new SelectFormat() { id = 0, text = "All" };
            serialisedJson.Insert(0, initial);

            ViewBag.team = new MultiSelectList(serialisedJson, "id", "text", ass);
            ViewBag.image = (from b in db.MultipleDocuments
                                 where b.RelationID == id && b.DocumentName == "commondocument"
                             select new TaskImageViewModel
                             {
                                 TaskImageId = b.Id,
                                 TaskId = id,
                                 FileName = b.Document,
                                 
                             }).ToList();
            return View(vmodel);
        }
            public ActionResult myfiles()
        {
            ViewBag.Employee = QkSelect.List(
                            new List<SelectListItem>
                            {
                                    new SelectListItem { Selected = false, Text = "All", Value = "0"},
                            }, "Value", "Text", 1);

            return View();
        }
        public ActionResult create()
        {
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

            filedocumentsViewModel vmodel = new filedocumentsViewModel();
            return View(vmodel);
        }
        [HttpPost]
        public ActionResult Edit(filedocumentsViewModel vm)
        {
            var today = System.DateTime.Now;
            var userid = User.Identity.GetUserId();
            var v = db.filedocumentdetails.Find((long)vm.filedocumentsid);

           

            v.createdby = userid;
            v.createddate = today;
            
            v.Notes = vm.Notes;
           
            v.Title = vm.Title;
        


            db.Entry(v).State = EntityState.Modified;
            db.SaveChanges();

         
            if (vm.AssignedMembers != null)
            {
                var assmemb = db.filedocumentdetailsas.Where(o => o.filedocumentdetailid == vm.filedocumentsid);
                db.filedocumentdetailsas.RemoveRange(assmemb);
                db.SaveChanges();
                foreach (var k in vm.AssignedMembers)
                {
                    filedocumentdetailsa n = new filedocumentdetailsa
                    {
                        employeeid = k,
                        filedocumentdetailid = (long)vm.filedocumentsid,
                    };
                    db.filedocumentdetailsas.Add(n);
                    db.SaveChanges();
                }
            }
            IFormFileCollection files = Request.Form.Files;
            var uploadUrl = LegacyWeb.MapPath("~/uploads/commondocuments/");

            if (!System.IO.Directory.Exists(uploadUrl))
                System.IO.Directory.CreateDirectory(uploadUrl);
            for (int i = 0; i < files.Count; i++)
            {
                IFormFile file = files[i];
                if (file.Length > 0)
                {

                    var fileCount = db.MultipleDocuments.Select(x => x.Id).AsEnumerable().DefaultIfEmpty(0).Max();

                    var fileName = Path.GetFileName(file.FileName);

                    String extension = Path.GetExtension(fileName);


                    String newName = fileCount + extension;
                    string newFName = fileCount + extension;
                    string newSName = fileCount + extension;
                    var FStatus = Status.active;

                    var thumbName = "";
                    var resizeName = "";
                    if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                    {
                        thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                        thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/commondocuments/"), thumbName);

                        resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/commondocuments/"), resizeName);
                        newFName = "resize_" + newFName;
                        FStatus = Status.inactive;
                    }
                    else
                    {
                        var commonfilename = "Docs-Thump.png";

                    }

                    newName = Path.Combine(LegacyWeb.MapPath("~/uploads/commondocuments/"), newName);
                    file.SaveAs(newName);

                    var FilemultipleDocument = new FilemultipleDocuments
                    {
                        Document = newSName,
                        RelationID = (long)vm.filedocumentsid,
                        DocumentName = "commondocument",
                        CreatedBy = userid,
                        Note = "",
                        CreatedDate = System.DateTime.Now,
                        Status = Status.active,
                        ExpiryDate = System.DateTime.Now.AddYears(100)
                    };
                    db.MultipleDocuments.Add(FilemultipleDocument);
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
                            resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/commondocuments/"), resizeName);
                            thumbs.Save(resizeName);
                        }
                        else
                        {
                            resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/commondocuments/"), resizeName);
                            lgimg.Save(resizeName);
                        }

                    }
                }
            }

            var changes = "fle Updated Successfully";
      
            com.addlog(LogTypes.Updated, userid, "filecommon", "filecommon", findip(), v.filedocumentDataId, "Updated");

            Success("Success", true);
         
                return RedirectToAction("index","home");
        }
        [HttpPost]
        public ActionResult create(filedocumentsViewModel vm)
        {
            var today = System.DateTime.Now;
            var userid = User.Identity.GetUserId();
            filedocumentdetail a = new filedocumentdetail
            {
                createdby = userid,
                createddate = today,
               
                Notes = vm.Notes,
             
                Title = vm.Title,
            

            };
            db.filedocumentdetails.Add(a);
            db.SaveChanges();
            var ids = a.filedocumentDataId;
            if (vm.AssignedMembers != null)
            {
                foreach (var k in vm.AssignedMembers)
                {
                    filedocumentdetailsa n = new filedocumentdetailsa
                    {
                        employeeid = k,
                         filedocumentdetailid = (long)ids,
                    };
                    db.filedocumentdetailsas.Add(n);
                    db.SaveChanges();
                }
            }
            IFormFileCollection files = Request.Form.Files;
            var uploadUrl = LegacyWeb.MapPath("~/uploads/commondocuments/");

            if (!System.IO.Directory.Exists(uploadUrl))
                System.IO.Directory.CreateDirectory(uploadUrl);
            for (int i = 0; i < files.Count; i++)
            {
                IFormFile file = files[i];
                if (file.Length > 0)
                {

                    var fileCount = db.MultipleDocuments.Select(x => x.Id).AsEnumerable().DefaultIfEmpty(0).Max();

                    var fileName = Path.GetFileName(file.FileName);

                    String extension = Path.GetExtension(fileName);


                    String newName = fileCount + extension;
                    string newFName = fileCount + extension;
                    string newSName = fileCount + extension;
                    var FStatus = Status.active;

                    var thumbName = "";
                    var resizeName = "";
                    if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                    {
                        thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                        thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/commondocuments/"), thumbName);

                        resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/commondocuments/"), resizeName);
                        newFName = "resize_" + newFName;
                        FStatus = Status.inactive;
                    }
                    else
                    {
                        var commonfilename = "Docs-Thump.png";

                    }
               
                    newName = Path.Combine(LegacyWeb.MapPath("~/uploads/commondocuments/"), newName);
                    file.SaveAs(newName);

                    var FilemultipleDocument = new FilemultipleDocuments
                    {
                        Document = newSName,
                        RelationID = ids,
                        DocumentName = "commondocument",
                        CreatedBy = userid,
                        Note = "",
                        CreatedDate = System.DateTime.Now,
                        Status = Status.active,
                        ExpiryDate = System.DateTime.Now.AddYears(100)
                    };
                    db.MultipleDocuments.Add(FilemultipleDocument);
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
                            resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/commondocuments/"), resizeName);
                            thumbs.Save(resizeName);
                        }
                        else
                        {
                            resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/commondocuments/"), resizeName);
                            lgimg.Save(resizeName);
                        }

                    }
                }
            }



            Success("Success", true);

            com.addlog(LogTypes.Created, userid, "commonfiles", "commonfiles", findip(), a.filedocumentDataId, "commonfiles Created Successfully");
          
                return RedirectToAction("create");
        }
 
        public ActionResult view(long id)
        {
            return View();
        }
    }
}
