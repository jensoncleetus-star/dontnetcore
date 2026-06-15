using System.Linq.Dynamic.Core;
using ApplicationUserManager = Microsoft.AspNetCore.Identity.UserManager<QuickSoft.Models.ApplicationUser>;
using ApplicationSignInManager = Microsoft.AspNetCore.Identity.SignInManager<QuickSoft.Models.ApplicationUser>;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using QuickSoft.Models;
using Syncfusion.EJ2.FileManager.Base;
using Syncfusion.EJ2.FileManager.PhysicalFileProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Web.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    public class FileManagerController : BaseController
    {
        // Accessing the File Operations from File Manager package
        PhysicalFileProvider operation = new PhysicalFileProvider();
        ApplicationDbContext db;
      
        public FileManagerController()
        {
            db = new ApplicationDbContext();
            // Map the path of the files to be accessed with the host
            // Assign the mapped path as root folder
            var UserId = System.Web.LegacyWeb.Current.User.Identity.GetUserId();
            var path = "";
            var allemp = (from a in db.Employees
                          join b in db.Users on a.UserId equals b.Id

                          where b.Status==1
                          select new
                          {
                a.FirstName,
                a.LastName
            }).Select(o => o.FirstName + " " + o.LastName).ToList();
            foreach (var e in allemp)
            {
                if (!System.IO.Directory.Exists(HostingEnvironment.MapPath("~/Uploads/Files") + "/" + e))
                {
                    System.IO.Directory.CreateDirectory(HostingEnvironment.MapPath("~/Uploads/Files") + "/" + e);
                }
            }
            var closed = (from a in db.Employees
                          join b in db.Users on a.UserId equals b.Id

                          where b.Status == 0
                          select new
                          {
                              a.FirstName,
                              a.LastName
                          }).Select(o => o.FirstName + " " + o.LastName).ToList();
            foreach (var e in closed)
            {
                if (System.IO.Directory.Exists(HostingEnvironment.MapPath("~/Uploads/Files") + "/" + e))
                {
                    System.IO.Directory.Delete(HostingEnvironment.MapPath("~/Uploads/Files") + "/" + e,true);
                }
            }
            if (UserId == "405c5575-2d86-4c34-9255-9603b9462184"||UserId== "1c7fca9e-cefc-4ff9-a0b5-ce2ea33793ca")

            {
                path = HostingEnvironment.MapPath("~/Uploads/Files");
                operation.RootFolder(path);


            }
            else
            {
                

                var emp = db.Employees.Where(o => o.UserId == UserId).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();
                if (emp != null)
                {
                    if (!System.IO.Directory.Exists(HostingEnvironment.MapPath("~/Uploads/Files") + "/" + emp))
                    {
                        System.IO.Directory.CreateDirectory(HostingEnvironment.MapPath("~/Uploads/Files") + "/" + emp);
                    }
                    path = HostingEnvironment.MapPath("~/Uploads/Files") + "/" + emp;
                    operation.RootFolder(path);
                }
            }
        }
        public ActionResult FileOperations(FileManagerDirectoryContent args)
        {
            JsonResult uploadResponse = null;
            long[] emps = new long[] { };
            // Processing the File Manager operations
            switch (args.Action)
            {
                case "read":
                    // Path - Current path; ShowHiddenItems - Boolean value to show/hide hidden items
                    
                    return Json(operation.ToCamelCase(operation.GetFiles(args.Path, args.ShowHiddenItems)));
                  
                case "delete":
                    {
                        uploadResponse= Json(operation.ToCamelCase(operation.Delete(args.Path, args.Names)));
                        // Path - Current path where of the folder to be deleted; Names - Name of the files to be deleted
                        if (args.RenameFiles != null)
                        {
                            emps = args.RenameFiles.Select(x => long.Parse(x)).ToArray();

                            if (emps.Contains(0))
                            {
                                var allemp = (from a in db.Employees
                                              join b in db.Users on a.UserId equals b.Id

                                              where b.Status == 1
                                              select new
                                              {
                                                  a.FirstName,
                                                  a.LastName
                                              }).Select(o => o.FirstName + " " + o.LastName).ToList();
                                foreach (var e in allemp)
                                {
                                    uploadResponse = Json(operation.ToCamelCase(operation.Delete("/" + e + "/", args.Names)));



                                }
                            }
                            else
                            {
                                foreach (var e in emps)
                                {
                                    var empname = db.Employees.Find(e);


                                    uploadResponse = Json(operation.ToCamelCase(operation.Delete("/" + empname.FirstName + " " + empname.LastName + "/", args.Names)));



                                }
                            }
                            return uploadResponse;
                        }
                        return uploadResponse;
                    }
                  
                    
                   
                case "copy":
                  {
                        uploadResponse=Json(operation.ToCamelCase(operation.Copy(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData)));

                        if (args.RenameFiles!=null)
                        {
                            emps = args.RenameFiles.Select(x => long.Parse(x)).ToArray();
                            
                            if (emps.Contains(0))
                            {
                                var allemp = (from a in db.Employees
                                              join b in db.Users on a.UserId equals b.Id

                                              where b.Status == 1
                                              select new
                                              {
                                                  a.FirstName,
                                                  a.LastName
                                              }).Select(o => o.FirstName + " " + o.LastName).ToList();
                                foreach (var e in allemp)
                                {
                                    uploadResponse= Json(operation.ToCamelCase(operation.Copy(args.Path, "/" + e + "/", args.Names, args.RenameFiles, args.TargetData)));


                                    

                                }
                            }
                            else
                            {
                                foreach (var e in emps)
                                {
                                    var empname = db.Employees.Find(e);

            
                                                uploadResponse = Json(operation.ToCamelCase(operation.Copy(args.Path, "/" + empname.FirstName + " " + empname.LastName + "/", args.Names, args.RenameFiles, args.TargetData)));




                                }
                            }
                            return uploadResponse;
                            }


                        return uploadResponse;    

                    }
                      case "move":
                    // Path - Path from where the file was cut; TargetPath - Path where the file/folder is to be moved; RenameFiles - Files with same name in the moved location that is confirmed for renaming; TargetData - Data of the moved file
                    return Json(operation.ToCamelCase(operation.Move(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData)));
                case "details":
                    if (args.Names == null)
                    {
                        args.Names = new string[] { };
                    }
                    // Path - Current path where details of file/folder is requested; Name - Names of the requested folders
                    return Json(operation.ToCamelCase(operation.Details(args.Path, args.Names)));
                case "create":
                    // Path - Current path where the folder is to be created; Name - Name of the new folder
                    return Json(operation.ToCamelCase(operation.Create(args.Path, args.Name)));
                case "search":
                    // Path - Current path where the search is performed; SearchString - String typed in the searchbox; CaseSensitive - Boolean value which specifies whether the search must be casesensitive
                    return Json(operation.ToCamelCase(operation.Search(args.Path, args.SearchString, args.ShowHiddenItems, args.CaseSensitive)));
                case "rename":
                    // Path - Current path of the renamed file; Name - Old file name; NewName - New file name
                    return Json(operation.ToCamelCase(operation.Rename(args.Path, args.Name, args.NewName)));
            }
            return null;
        }

        // Processing the Upload operation
        public ActionResult Upload(string path, IList<System.Web.IFormFile> uploadFiles, string action, string assign)
        {
            FileManagerResponse uploadResponse;
            //Invoking upload operation with the required paramaters
            // path - Current path where the file is to uploaded; uploadFiles - Files to be uploaded; action - name of the operation(upload)
            if(assign=="")
            uploadResponse = operation.Upload(path, uploadFiles, action, null);
            else
            {
                long[] emps = new long[] { };

                emps = assign.Split(',').Select(x => long.Parse(x)).ToArray();
                if (assign == "0")
                {
                    var allemp = (from a in db.Employees
                                  join b in db.Users on a.UserId equals b.Id

                                  where b.Status == 1
                                  select new
                                  {
                                      a.FirstName,
                                      a.LastName
                                  }).Select(o => o.FirstName + " " + o.LastName).ToList();
                    foreach (var e in allemp)
                    {
                     
                        uploadResponse = operation.Upload("/" +e+ "/", uploadFiles, action, null);

                    }
                }
                else
                {
                    foreach (var e in emps)
                    {
                        var empname = db.Employees.Find(e);
                        uploadResponse = operation.Upload("/" + empname.FirstName + " " + empname.LastName + "/", uploadFiles, action, null);
                    }
                }
            }

            return Content("");
        }
        // Processing the Download operation
        public ActionResult Download(string downloadInput)
        {
            FileManagerDirectoryContent args = JsonConvert.DeserializeObject<FileManagerDirectoryContent>(downloadInput);
            //Invoking download operation with the required paramaters
            return operation.Download(args.Path, args.Names);
        }
        // Processing the GetImage operation
        public ActionResult GetImage(FileManagerDirectoryContent args)
        {
            //Invoking GetImage operation with the required paramaters
            return operation.GetImage(args.Path, args.Id, false, null, null);
        }
        public ActionResult Index()
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
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
