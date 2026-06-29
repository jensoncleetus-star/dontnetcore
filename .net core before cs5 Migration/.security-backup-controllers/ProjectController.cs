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
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.ViewModel;
using Microsoft.AspNetCore.Identity;
using System.Globalization;
using System.Net;
using System.Drawing;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Data.SqlClient;
namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class ProjectController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ProjectController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Project
        [RedirectingAction]
        ////[QkAuthorize(Roles = "Dev,Project List")]
        public ActionResult Index()
        {
            var OpAll = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                               }, "Value", "Text", 1);
            var name = db.Contacts.Select(s => new
            {
                ID = s.Name,
                Name = s.Name
            }).Distinct().ToList();
            ViewBag.Contact = QkSelect.List(name, "ID", "Name");

            ViewBag.Cust = OpAll;

            ViewBag.SalesExecutive = OpAll;

            ViewBag.Prjct = OpAll;

            ViewBag.PType = OpAll;

            ViewBag.Stat = OpAll;

            ProjectViewModel vmodel = new ProjectViewModel();
            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Project" && a.Status == Status.active).ToList();
            vmodel.FieldMapAll = db.FieldMappings.Where(a => a.Section == "Project").ToList();
            return View(vmodel);
        }


        [RedirectingAction]
        [Authorize(Roles = "Dev,Project List")]
        [HttpPost]
        public ActionResult GetAllProjects(long? Project, long? customer, long? PType, string FromDate, string ToDate, string CPerson, string CNumber, long? salesperson, string location, long? prostat, string ref1, string ref2, string ref3, string ref4, string ref5)
        {
            DateTime? fdate = null;
            DateTime? tdate = null;
            if (FromDate != "")
            {
                fdate = DateTime.Parse(FromDate, new CultureInfo("en-GB"));
            }
            if (ToDate != "")
            {
                tdate = DateTime.Parse(ToDate, new CultureInfo("en-GB"));
            }

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
            var today = Convert.ToDateTime(System.DateTime.Now);

            var uDev = User.IsInRole("Dev");
            var uProjectView = User.IsInRole("View Project");

            bool check = User.IsInRole("All Project");


            var UserView = (from a in db.Projects
                            join b in db.Customers on a.Customer equals b.CustomerID into cust
                            from b in cust.DefaultIfEmpty()
                            join c in db.ProjectTypes on a.ProType equals c.ProjectTypeID into pro
                            from c in pro.DefaultIfEmpty()
                            join d in db.Employees on a.SalesPerson equals d.EmployeeId into emp
                            from d in emp.DefaultIfEmpty()
                            join e in db.ProjectStatus on a.ProjectStatus equals e.ProjectStatusId into pstat
                            from e in pstat.DefaultIfEmpty()
                            join f in db.Users on a.CreatedBy equals f.Id
                            where (Project == null || Project == 0 || a.ProjectId == Project) && (customer == null || customer == 0 || b.CustomerID == customer)
                            && (PType == null || PType == 0 || c.ProjectTypeID == PType) && (salesperson == null || salesperson == 0 || d.EmployeeId == salesperson)
                            && (FromDate == null || FromDate == "" || EF.Functions.DateDiffDay(a.StartDate, fdate) <= 0)
                            && (ToDate == null || ToDate == "" || EF.Functions.DateDiffDay(a.EndDate, tdate) >= 0)
                            && (CPerson == null || CPerson == "" || a.ContactPerson == CPerson) && (CNumber == null || CNumber == "" || a.ContactNumber == CNumber)
                            && (location == null || location == "" || a.Location == location)
                            && (prostat == 0 || prostat == null || a.ProjectStatus == prostat)
                            && (check == true || a.CreatedBy == UserId) &&
                              (ref1 == "" || ref1 == null || a.Ref1 == ref1) &&
                              (ref2 == "" || ref2 == null || a.Ref2 == ref2) &&
                              (ref3 == "" || ref3 == null || a.Ref3 == ref3) &&
                              (ref4 == "" || ref4 == null || a.Ref4 == ref4) &&
                              (ref5 == "" || ref5 == null || a.Ref5 == ref5)
                            select new
                            {
                                a.ProjectId,
                                a.ProjectName,
                                f.UserName,
                                a.ProCode,
                                a.StartDate,
                                a.EndDate,
                                c.TypeName,
                                Customer = b.CustomerCode + "-" + b.CustomerName,
                                CustomerID = b != null ? b.CustomerID : 0,
                                SalesPerson = d.FirstName + " " + d.LastName,
                                a.Location,
                                a.Branch,
                                a.Status,
                                a.ContactNumber,
                                a.ContactPerson,
                                Dev = uDev,
                                Details = uProjectView,
                                ProStatus = e.StatusName,
                                a.CreatedDate,
                                a.Ref1,
                                a.Ref2,
                                a.Ref3,
                                a.Ref4,
                                a.Ref5,
                            });

            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(p => p.ProCode.ToString().ToLower().Equals(search.ToLower()) ||
                                p.Customer.ToString().ToLower().Contains(search.ToLower()) ||
                                p.ProStatus.ToString().ToLower().Contains(search.ToLower()) ||
                                p.ProjectName.ToString().ToLower().Contains(search.ToLower())
                                //p.Ref5.ToString().ToLower().Contains(search.ToLower())
                                );
            }
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                UserView = UserView.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }

            recordsTotal = UserView.Count();
            var data = UserView.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        // make active or inactive
        [HttpGet]
        public ActionResult ChangeStatus(long? id, string type)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project tx = db.Projects.Find(id);
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
        public JsonResult ChangeStatus(string type, long? id, Project tx)
        {
            bool stat = false;
            string msg;
            string types = "";
            Project pro = db.Projects.Find(id);
            if (tx.Status == Status.inactive)
            {
                types = " Inactive";
                pro.Status = Status.inactive;
            }
            else
            {
                types = " Active";
                pro.Status = Status.active;
            }

            db.Entry(pro).State = EntityState.Modified;
            var updates = db.SaveChanges();

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Changed, UserId, "Project", "Projects", findip(), tx.ProjectId, "Successfully Changed the Project to" + types);


            stat = true;
            msg = " Successfully Changed the Project to" + types;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [RedirectingAction]
       [QkAuthorize(Roles = "Dev,Create Project")]
        public ActionResult Create(long? id)
        {
            var project = new ProjectViewModel
            {
                ProCode = InvoiceNo(),
            };

            if (id != null)
            {
                Quotation quentry = db.Quotations.Find(id);
                if (quentry == null)
                {
                    return NotFound();
                }
                project.Customer = quentry.Customer;
                project.SalesPerson = (long)quentry.QuotCashier;
                project.Location = db.Customers.Where(a => a.CustomerID == quentry.Customer).Select(a => a.Location).FirstOrDefault();
            }

            var cust = db.Customers.Select(s => new
            {
                CustomerID = s.CustomerID,
                CustomerDetails = s.CustomerCode + " - " + s.CustomerName
            }).ToList();
            ViewBag.Customr = QkSelect.List(cust, "CustomerID", "CustomerDetails");

            var use = db.Employees
                    .Select(s => new
                    {
                        ID = s.EmployeeId,
                        Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                    })
                    .ToList();
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");

            var ptype = db.ProjectTypes.Select(r => new
            {
                ID = r.ProjectTypeID,
                Name = r.TypeName
            }).ToList();
            ViewBag.proType = QkSelect.List(ptype, "ID", "Name");

            List<SelectListItem> type = new List<SelectListItem>();
            type.Add(new SelectListItem { Text = "%", Value = "%" });
            type.Add(new SelectListItem { Text = "", Value = "$" });
            ViewBag.DType = QkSelect.List(type, "Text", "Value");


            var pstat = db.ProjectStatus
                .Select(s => new
                {
                    ID = s.ProjectStatusId,
                    Name = s.StatusName
                })
                .ToList();
            ViewBag.Stat = QkSelect.List(pstat, "ID", "Name");

            ViewBag.IncomeAcc = QkSelect.List(
                             new List<SelectListItem>
                             {
                                    new SelectListItem { Selected = true, Text = "Sale", Value = "1"},
                             }, "Value", "Text", 0);

            //field mapping
            project.FieldMap = db.FieldMappings.Where(a => a.Section == "Project" && a.Status == Status.active).ToList();

            var userpermission = User.IsInRole("All Project");

            ViewBag.LastEntry = db.Projects.Where(p => (userpermission == true)).Select(p => p.ProjectId).AsEnumerable().DefaultIfEmpty(0).Max();

            project.FieldMap = db.FieldMappings.Where(a => a.Section == "Project" && a.Status == Status.active).ToList();

            var loc = db.Projects
                .Select(s => new
                {
                    ID = s.Location,
                    Name = s.Location
                }).Distinct().ToList();
            ViewBag.Local = QkSelect.List(loc, "ID", "Name");

            var pimg = db.ProjectImages.Where(a => a.Status == Status.inactive).ToList();
            string path = LegacyWeb.MapPath("~/uploads/projectdocuments/");
            foreach (var arr in pimg)
            {
                try
                {
                    string[] splitlist = arr.FileName.Split('_');
                    if (splitlist.Length > 1)
                    {
                        string newpath = LegacyWeb.MapPath("~/uploads/projectdocuments/" + splitlist[1]);
                        string filepath = LegacyWeb.MapPath("~/uploads/projectdocuments/" + arr.FileName);
                        if (System.IO.File.Exists(newpath) && System.IO.File.Exists(filepath))
                        {
                            System.IO.File.Delete(Path.Combine(path, splitlist[1]));
                        }
                    }
                    ProjectImage doc = db.ProjectImages.Find(arr.ProjectImageId);
                    doc.Status = Status.active;
                    db.Entry(doc).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch
                {
                    Exception ex;
                }
            }

            return View(project);
        }
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Create Project")]
        [HttpPost]
        public JsonResult Create(long? id, ProjectViewModel vmodel)
        {
            bool stat = false;
            string msg;
            var Exists = db.Projects.Any(u => u.ProjectName == vmodel.ProjectName && u.Customer == vmodel.Customer);
            if (Exists)
            {
                msg = "Project Name Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var UserId = User.Identity.GetUserId();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                    var today = Convert.ToDateTime(System.DateTime.Now);
                    DateTime? sDate = null;
                    DateTime? eDate = null;
                    if (vmodel.ExStartDate != null)
                    {
                        sDate = DateTime.Parse(vmodel.ExStartDate.ToString(), new CultureInfo("en-GB"));

                    }
                    if (vmodel.ExEndDate != null)
                    {
                        eDate = DateTime.Parse(vmodel.ExEndDate.ToString(), new CultureInfo("en-GB"));
                    }
                    var pro = new Project
                    {
                        ProNo = GetProNo(),
                        ProCode = vmodel.ProCode,
                        ProjectName = vmodel.ProjectName,
                        Customer = vmodel.Customer,
                        ContactPerson = vmodel.ContactPerson,

                        StartDate = sDate,
                        EndDate = eDate,

                        SalesPerson = vmodel.SalesPerson,
                        SalesContact = vmodel.SalesContact,
                        ProType = vmodel.ProType,

                        Location = vmodel.Location,
                        Details = vmodel.Details,
                        Note = vmodel.Note,
                        ContactNumber = vmodel.ContactNumber,

                        Editable = choice.Yes,
                        CreatedDate = today,
                        CreatedBy = UserId,
                        Status = Status.active,
                        Branch = BranchID,
                        ProjectStatus = vmodel.ProjectStatus,
                        IncomeAccount = vmodel.IncomeAccount,

                        Ref1 = vmodel.Ref1,
                        Ref2 = vmodel.Ref2,
                        Ref3 = vmodel.Ref3,
                        Ref4 = vmodel.Ref4,
                        Ref5 = vmodel.Ref5,
                    };
                    db.Projects.Add(pro);
                    db.SaveChanges();



                    // fileupload
                    IFormFileCollection files = Request.Form.Files;
                    if (files.Count > 0)
                    {
                        string path = LegacyWeb.MapPath("~/uploads/projectdocuments/");
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        for (int i = 0; i < files.Count; i++)
                        {
                            IFormFile file = files[i];
                            if (file.Length > 0)
                            {

                                var fileCount = db.ProjectImages.Select(a => a.ProjectImageId).AsEnumerable().DefaultIfEmpty(0).Max();

                                var fileName = Path.GetFileName(file.FileName);

                                String extension = Path.GetExtension(fileName);


                                String newName = fileCount + extension;
                                string newFName = fileCount + extension;
                                var FStatus = Status.active;
                                var thumbName = "";
                                var resizeName = "";
                                if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                                {
                                    thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/projectdocuments/"), thumbName);

                                    resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/projectdocuments/"), resizeName);
                                    newFName = "resize_" + newFName;
                                    FStatus = Status.inactive;
                                }
                                else
                                {
                                    var commonfilename = "Docs-Thump.png";

                                }
                                newName = Path.Combine(LegacyWeb.MapPath("~/uploads/projectdocuments/"), newName);
                                file.SaveAs(newName);

                                var PImg = new ProjectImage
                                {
                                    ProjectId = pro.ProjectId,
                                    FileName = newFName,//Path.GetFileName(file.FileName),
                                    Status = FStatus,
                                    CreatedDate = Convert.ToDateTime(System.DateTime.Now)
                                };
                                db.ProjectImages.Add(PImg);
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
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/projectdocuments/"), resizeName);
                                        thumbs.Save(resizeName);
                                    }
                                    else
                                    {
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/projectdocuments/"), resizeName);
                                        lgimg.Save(resizeName);
                                    }

                                }
                            }
                        }
                    }

                    com.addlog(LogTypes.Created, UserId, "Project", "Projects", findip(), pro.ProjectId, "Project added Successfully");
                    msg = "Successfully Added Project details.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {
                    msg = "Looks like something went wrong. Please check your form..";
                    stat = false;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
            }
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Create Project")]
        public ActionResult AddProject()
        {
            var project = new ProjectViewModel
            {
                ProCode = InvoiceNo(),
            };

            var cust = db.Customers.Select(s => new
            {
                CustomerID = s.CustomerID,
                CustomerDetails = s.CustomerCode + " - " + s.CustomerName
            }).ToList();
            ViewBag.Customr = QkSelect.List(cust, "CustomerID", "CustomerDetails");

            var use = db.Employees
                    .Select(s => new
                    {
                        ID = s.EmployeeId,
                        Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                    })
                    .ToList();
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");

            var ptype = db.ProjectTypes.Select(r => new
            {
                ID = r.ProjectTypeID,
                Name = r.TypeName
            }).ToList();
            ViewBag.proType = QkSelect.List(ptype, "ID", "Name");

            var pstat = db.ProjectStatus
                .Select(s => new
                {
                    ID = s.ProjectStatusId,
                    Name = s.StatusName
                })
                .ToList();
            ViewBag.Stat = QkSelect.List(pstat, "ID", "Name");

            ViewBag.IncomeAcc = QkSelect.List(
                           new List<SelectListItem>
                           {
                                    new SelectListItem { Selected = true, Text = "Sale", Value = "1"},
                           }, "Value", "Text", 0);

            //field mapping
            project.FieldMap = db.FieldMappings.Where(a => a.Section == "Project" && a.Status == Status.active).ToList();

            var loc = db.Projects
               .Select(s => new
               {
                   ID = s.Location,
                   Name = s.Location
               }).Distinct().ToList();
            ViewBag.Local = QkSelect.List(loc, "ID", "Name");

            return PartialView(project);
        }

        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Project")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project pro = db.Projects.Find(id);

            if (pro == null)
            {
                return NotFound();
            }

            var project = new ProjectViewModel
            {
                ProCode = pro.ProCode,
                ProjectName = pro.ProjectName,
                Customer = pro.Customer,
                ContactPerson = pro.ContactPerson,

                ExStartDate = pro.StartDate != null ? Convert.ToDateTime(pro.StartDate).ToString("dd-MM-yyyy") : "",
                ExEndDate = pro.EndDate != null ? Convert.ToDateTime(pro.EndDate).ToString("dd-MM-yyyy") : "",

                SalesPerson = pro.SalesPerson,
                SalesContact = pro.SalesContact,
                ProType = pro.ProType,
                ProTypeId = pro.ProTypeId,
                ContactNumber = pro.ContactNumber,

                Location = pro.Location,
                Details = pro.Details,
                Note = pro.Note,
                IncomeAccount = pro.IncomeAccount,
                IncomeAccName = db.Accountss.Where(a => a.AccountsID == pro.IncomeAccount).Select(a => a.Name).FirstOrDefault(),
                ProjectStatus = pro.ProjectStatus,

                Ref1 = pro.Ref1,
                Ref2 = pro.Ref2,
                Ref3 = pro.Ref3,
                Ref4 = pro.Ref4,
                Ref5 = pro.Ref5,
            };



            if (pro.Customer == -2)
            {
                ViewBag.Customr = QkSelect.List(
                          new List<SelectListItem>
                          {
                                                new SelectListItem { Selected = true, Text = "--No Customers--", Value = "-2"},
                          }, "Value", "Text", 0);
            }
            else
            {
                var cus = db.Customers
                  .Select(s => new
                  {
                      CustomerID = s.CustomerID,
                      CustomerDetails = s.CustomerCode + " - " + s.CustomerName
                  }).ToList();
                ViewBag.Customr = QkSelect.List(cus, "CustomerID", "CustomerDetails");
            }

            var use = db.Employees
                    .Select(s => new
                    {
                        ID = s.EmployeeId,
                        Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
                    })
                    .ToList();
            ViewBag.Employee = QkSelect.List(use, "ID", "Name");

            var ptype = db.ProjectTypes.Select(r => new
            {
                ID = r.ProjectTypeID,
                Name = r.TypeName
            }).ToList();
            ViewBag.proTypes = QkSelect.List(ptype, "ID", "Name");

            var qtno = db.Quotations.Select(r => new
            {
                ID = r.QuotationId,
                Name = r.BillNo
            }).ToList();
            ViewBag.QtNo = QkSelect.List(qtno, "ID", "Name");

            var pstat = db.ProjectStatus
                .Select(s => new
                {
                    ID = s.ProjectStatusId,
                    Name = s.StatusName
                })
                .ToList();
            ViewBag.Stat = QkSelect.List(pstat, "ID", "Name");

            var loc = db.Projects
              .Select(s => new
              {
                  ID = s.Location,
                  Name = s.Location
              }).Distinct().ToList();
            ViewBag.Local = QkSelect.List(loc, "ID", "Name");

            List<SelectListItem> type = new List<SelectListItem>();
            type.Add(new SelectListItem { Text = "%", Value = "%" });
            type.Add(new SelectListItem { Text = "", Value = "$" });
            ViewBag.DType = QkSelect.List(type, "Text", "Value");


            ViewBag.image = (from b in db.ProjectImages
                             join c in db.Projects on b.ProjectId equals c.ProjectId
                             where c.ProjectId == id
                             select new ProjectImageViewModel
                             {
                                 ProjectImageId = b.ProjectImageId,
                                 ProjectId = b.ProjectId,
                                 FileName = b.FileName,
                                 ProjectName = c.ProjectName
                             }).ToList();


            if (project.IncomeAccount == 1)
            {
                ViewBag.IncomeAcc = QkSelect.List(
                                new List<SelectListItem>
                                {
                                        new SelectListItem { Selected = true, Text = "Sale", Value = "1"},
                                }, "Value", "Text", 0);
            }
            else
            {

                List<SelectFormat> serialisedJson;
                //sale account
                var sparentid = new SqlParameter("@parentid", 15);
                var sgroupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", sparentid).AsEnumerable().ToList();
                var sgpid = sgroupsdata.Select(a => a.AccountsGroupID).ToArray();

                //Income (Direct/Opr.)
                var parentid = new SqlParameter("@parentid", 31);
                var groupsdata = db.Database.SqlQueryRaw<AccountsGroup>("allchildGroups @parentid", parentid).AsEnumerable().ToList();
                var gpid = groupsdata.Select(a => a.AccountsGroupID).ToArray();



                var incacc = (from a in db.Accountss
                              join b in db.AccountsGroups on a.Group equals b.AccountsGroupID into gp
                              from b in gp.DefaultIfEmpty()
                              where a.Status == Status.active && a.AccountsID != 1 && (sgpid.Contains(a.Group) || gpid.Contains(a.Group))
                              select new
                              {

                                  Name = a.Name, //each json object will have
                                  ID = a.AccountsID
                              }).OrderBy(b => b.Name).ToList();
                ViewBag.IncomeAcc = QkSelect.List(incacc, "ID", "Name");

            }

            //field mapping
            project.FieldMap = db.FieldMappings.Where(a => a.Section == "Project" && a.Status == Status.active).ToList();

            var userpermission = User.IsInRole("All Project");
            ViewBag.preEntry = db.Projects.Where(a => a.ProjectId < id && (userpermission == true)).Select(a => a.ProjectId).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.Projects.Where(a => a.ProjectId > id && (userpermission == true)).Select(a => a.ProjectId).DefaultIfEmpty().Min();

            return View(project);
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Project")]
        public JsonResult Edit(ProjectViewModel vmodel, Int64 id)
        {
            bool stat = false;
            string msg;
            var Exists = db.Projects.Any(u => u.ProjectName == vmodel.ProjectName && u.Customer == vmodel.Customer && u.ProjectId != id);
            if (Exists)
            {
                msg = "Project Name Already Exists.";
                stat = false;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var UserId = User.Identity.GetUserId();

                    DateTime? sDate = null;
                    DateTime? eDate = null;

                    if (vmodel.ExStartDate != null)
                    {
                        sDate = DateTime.Parse(vmodel.ExStartDate.ToString(), new CultureInfo("en-GB"));

                    }
                    if (vmodel.ExEndDate != null)
                    {
                        eDate = DateTime.Parse(vmodel.ExEndDate.ToString(), new CultureInfo("en-GB"));
                    }

                    Project pro = db.Projects.Find(id);


                    pro.ProjectName = vmodel.ProjectName;
                    pro.Customer = vmodel.Customer;
                    pro.ContactPerson = vmodel.ContactPerson;
                    pro.StartDate = sDate;
                    pro.EndDate = eDate;

                    pro.SalesPerson = vmodel.SalesPerson;
                    pro.SalesContact = vmodel.SalesContact;
                    pro.ProType = vmodel.ProType;

                    pro.Location = vmodel.Location;
                    pro.Details = vmodel.Details;
                    pro.Note = vmodel.Note;
                    pro.ContactNumber = vmodel.ContactNumber;
                    pro.ProjectStatus = vmodel.ProjectStatus;
                    pro.IncomeAccount = vmodel.IncomeAccount;

                    pro.Ref1 = vmodel.Ref1;
                    pro.Ref2 = vmodel.Ref2;
                    pro.Ref3 = vmodel.Ref3;
                    pro.Ref4 = vmodel.Ref4;
                    pro.Ref5 = vmodel.Ref5;

                    db.Entry(pro).State = EntityState.Modified;
                    db.SaveChanges();


                    // fileupload
                    IFormFileCollection files = Request.Form.Files;
                    if (files.Count > 0)
                    {
                        string path = LegacyWeb.MapPath("~/uploads/projectdocuments/");
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        for (int i = 0; i < files.Count; i++)
                        {
                            IFormFile file = files[i];
                            if (file.Length > 0)
                            {

                                var fileCount = db.ProjectImages.Select(a => a.ProjectImageId).AsEnumerable().DefaultIfEmpty(0).Max();

                                var fileName = Path.GetFileName(file.FileName);

                                String extension = Path.GetExtension(fileName);


                                String newName = fileCount + extension;
                                string newFName = fileCount + extension;
                                var FStatus = Status.active;
                                var thumbName = "";
                                var resizeName = "";
                                if (extension == ".jpg" || extension == ".jfif" || extension == ".png" || extension == ".jpeg")
                                {
                                    thumbName = "thumb_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    thumbName = Path.Combine(LegacyWeb.MapPath("~/uploads/projectdocuments/"), thumbName);

                                    resizeName = "resize_" + newName.Split('.').ElementAt(0) + "." + newName.Split('.').ElementAt(1);
                                    resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/projectdocuments/"), resizeName);
                                    newFName = "resize_" + newFName;
                                    FStatus = Status.inactive;
                                }
                                else
                                {
                                    var commonfilename = "Docs-Thump.png";

                                }
                                newName = Path.Combine(LegacyWeb.MapPath("~/uploads/projectdocuments/"), newName);
                                file.SaveAs(newName);

                                var PImg = new ProjectImage
                                {
                                    ProjectId = pro.ProjectId,
                                    FileName = newFName,//Path.GetFileName(file.FileName),
                                    Status = FStatus,
                                    CreatedDate = Convert.ToDateTime(System.DateTime.Now)
                                };
                                db.ProjectImages.Add(PImg);
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
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/projectdocuments/"), resizeName);
                                        thumbs.Save(resizeName);
                                    }
                                    else
                                    {
                                        resizeName = Path.Combine(LegacyWeb.MapPath("~/uploads/projectdocuments/"), resizeName);
                                        lgimg.Save(resizeName);
                                    }

                                }
                            }
                        }
                    }



                    com.addlog(LogTypes.Updated, UserId, "Project", "Projects", findip(), pro.ProjectId, "Project Updated Successfully");
                    msg = "Successfully Updated Project details.";
                    stat = true;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                }
                else
                {
                    msg = "Looks like something went wrong. Please check your form..";
                    stat = false;
                    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

                }
            }
        }


        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Project")]
        public JsonResult ImageDelete(long key)
        {
            bool stat = false;
            string msg;
            ProjectImage pImg = db.ProjectImages.Find(key);
            db.ProjectImages.Remove(pImg);
            db.SaveChanges();

            string fullPath = LegacyWeb.MapPath("~/uploads/projectdocuments/" + pImg.FileName);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            string fullPaththumb = LegacyWeb.MapPath("~/uploads/projectdocuments/" + "thumb_" + pImg.FileName);
            if (System.IO.File.Exists(fullPaththumb))
            {
                System.IO.File.Delete(fullPaththumb);
            }

            var UserId = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, UserId, "ProjectImage", "ProjectImages", findip(), pImg.ProjectImageId, "Project Image Deleted Successfully");


            Int64 Id = key;
            stat = true;
            msg = "Successfully deleted Project Image.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }


        // GET: /Delete/5
        [RedirectingAction]
        [Authorize(Roles = "Dev,Delete Project")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project pro = db.Projects.Find(id);
            if (pro == null)
            {
                return NotFound();
            }
            return PartialView(pro);
        }

        // POST: /Delete/5

        [RedirectingAction]
        [Authorize(Roles = "Dev,Delete Project")]
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
                msg = "Successfully Deleted Project details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpGet]
        [RedirectingAction]
        [Authorize(Roles = "Dev,View Project")]
        public ActionResult Details(long? id)
        {
            Project project = db.Projects.Find(id);

            ProjectDetailViewModel vmodel = new ProjectDetailViewModel();

            if (project.Customer != -2)
            {

                vmodel = (from a in db.Projects
                          join b in db.Customers on a.Customer equals b.CustomerID into cust
                          from b in cust.DefaultIfEmpty()
                          join c in db.ProjectTypes on a.ProType equals c.ProjectTypeID into pro
                          from c in pro.DefaultIfEmpty()

                          join d in db.Employees on a.SalesPerson equals d.EmployeeId into emp
                          from d in emp.DefaultIfEmpty()

                          join e in db.Users on a.CreatedBy equals e.Id into user
                          from e in user.DefaultIfEmpty()

                          let acc = db.Accountss.Where(s => s.AccountsID == b.Accounts).FirstOrDefault()
                          where a.ProjectId == id
                          select new
                          {
                              a.ProjectId,
                              a.ProjectName,
                              a.Customer,
                              e.UserName,
                              a.ProCode,
                              a.StartDate,
                              a.EndDate,
                              a.CreatedDate,
                              a.ContactNumber,

                              a.Status,
                              c.TypeName,
                              b.CustomerCode,
                              b.CustomerName,
                              d.FirstName,
                              d.LastName,
                              a.ContactPerson,
                              a.SalesPerson,
                              a.Location,
                              a.Details,
                              a.Note,
                              b.Contact,
                              custsale = b.SalesPerson,
                              acc.AccountsID,
                              acc.OpnBalance,
                              acc.OpnBalanceCr,
                              b.Type,
                              a.Ref1,
                              a.Ref2,
                              a.Ref3,
                              a.Ref4,
                              a.Ref5,
                              Mobile = (from ac in db.Mobiles
                                        where (ac.Contact == b.Contact)
                                        select new MobileViewModel
                                        {
                                            Num = ac.MobileNum,
                                            Name = ac.Name
                                        }).ToList(),

                          }).ToList().Select(o => new ProjectDetailViewModel
                          {
                              ProjectId = o.ProjectId,
                              ProjectName = o.ProjectName,
                              UserName = o.UserName,
                              ProCode = o.ProCode,
                              CreatedDate = o.CreatedDate,
                              newExStartDate = o.StartDate,
                              newExEndDate = o.EndDate,
                              ProStatus = o.Status.ToString(),
                              StartDate = o.StartDate,
                              EndDate = o.EndDate,

                              TypeName = o.TypeName,
                              CustomerName = o.CustomerName,
                              CustomerCode = o.CustomerCode,
                              SalesPersonName = o.FirstName + " " + o.LastName,
                              ContactPerson = o.ContactPerson,
                              SalesPerson = o.SalesPerson,
                              Location = o.Location,
                              Details = o.Details,
                              Note = o.Note,
                              ContactNumber = o.ContactNumber,

                              //customer
                              Customers = db.Customers.Find(o.Customer),

                              Accounts = db.Accountss.Find(o.AccountsID),
                              DC = o.OpnBalance == 0 ? "Credit" : "Debit",
                              OpenBalance = o.OpnBalance == 0 ? o.OpnBalanceCr : o.OpnBalance,
                              CustType = Enum.GetName(typeof(CRMCustomerType), o.Type),
                              custSalePerson = db.Employees.Where(a => a.EmployeeId == o.custsale).Select(a => a.FirstName + " " + a.LastName).FirstOrDefault(),
                              Ref1 = o.Ref1,
                              Ref2 = o.Ref2,
                              Ref3 = o.Ref3,
                              Ref4 = o.Ref4,
                              Ref5 = o.Ref5,
                              Contact = (from ac in db.Contacts
                                         where (ac.ContactID == o.Contact)
                                         select new ProjectContactViewModel
                                         {
                                             Address = ac.Address,
                                             City = ac.City,
                                             State = ac.State,
                                             Country = ac.Country,
                                             Zip = ac.Zip,
                                             Phone = ac.Phone,
                                             Fax = ac.Fax,
                                             EmailId = ac.EmailId,
                                             Reference = ac.Reference,
                                             ContactPerson = ac.ContactPerson
                                         }).FirstOrDefault(),
                              mob = o.Mobile,
                          }).FirstOrDefault();

            }
            else
            {
                vmodel = (from a in db.Projects
                          join c in db.ProjectTypes on a.ProType equals c.ProjectTypeID into pro
                          from c in pro.DefaultIfEmpty()

                          join d in db.Employees on a.SalesPerson equals d.EmployeeId into emp
                          from d in emp.DefaultIfEmpty()

                          join e in db.Users on a.CreatedBy equals e.Id into user
                          from e in user.DefaultIfEmpty()

                          where a.ProjectId == id
                          select new
                          {
                              a.ProjectId,
                              a.ProjectName,
                              a.Customer,
                              e.UserName,
                              a.ProCode,
                              a.StartDate,
                              a.EndDate,
                              a.CreatedDate,
                              a.ContactNumber,

                              a.Status,
                              c.TypeName,
                              d.FirstName,
                              d.LastName,
                              a.ContactPerson,
                              a.SalesPerson,
                              a.Location,
                              a.Details,
                              a.Note,
                              a.Ref1,
                              a.Ref2,
                              a.Ref3,
                              a.Ref4,
                              a.Ref5,
                          }).ToList().Select(o => new ProjectDetailViewModel
                          {
                              ProjectId = o.ProjectId,
                              ProjectName = o.ProjectName,
                              UserName = o.UserName,
                              ProCode = o.ProCode,
                              CreatedDate = o.CreatedDate,
                              newExStartDate = o.StartDate,
                              newExEndDate = o.EndDate,
                              ProStatus = o.Status.ToString(),
                              StartDate = o.StartDate,
                              EndDate = o.EndDate,

                              TypeName = o.TypeName,
                              CustomerName = "No Customer",
                              CustomerCode = "",
                              SalesPersonName = o.FirstName + " " + o.LastName,
                              ContactPerson = o.ContactPerson,
                              SalesPerson = o.SalesPerson,
                              Location = o.Location,
                              Details = o.Details,
                              Note = o.Note,
                              ContactNumber = o.ContactNumber,

                              //customer
                              Customers = db.Customers.Find(o.Customer),
                              Accounts = null,
                              DC = null,
                              OpenBalance = 0,
                              custSalePerson = "",
                              CustType = "No Customer",
                              Ref1 = o.Ref1,
                              Ref2 = o.Ref2,
                              Ref3 = o.Ref3,
                              Ref4 = o.Ref4,
                              Ref5 = o.Ref5,
                          }).FirstOrDefault();
            }

            vmodel.FieldMap = db.FieldMappings.Where(a => a.Section == "Project" && a.Status == Status.active).ToList();

            return View(vmodel);
        }


        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Project")]
        public ActionResult DeleteAllProject(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteProject(arr) == true) ? count++ : notdel++;
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Projects, Unable to Delete " + notdel + " Projects. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Projects.", true);
            }
            else
            {
                Success("Deleted " + count + " Projects.", true);
            }
            return RedirectToAction("Index", "Project");
        }

        private Boolean DeleteProject(long id)
        {
            var Msg = chkDeleteWithMsg(id);
            bool res = (Msg != null) ? false : DeleteFn(id);
            return res;
        }
        public bool DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();

            Project pro = db.Projects.Find(id);
            if (pro != null)
            {
                db.Projects.Remove(pro);
                db.SaveChanges();
                com.addlog(LogTypes.Deleted, UserId, "Project", "Projects", findip(), pro.ProjectId, "Project Deleted Successfully");
            }
            return true;
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            if (db.Quotations.Any(c => c.Project == id))
            {
                msg = "Project Already used in Quotation !!";
            }
            else if (db.SalesEntrys.Any(c => c.Project == id))
            {
                msg = "Project Already used in Sales !!";
            }
            else if (db.SalesReturns.Any(c => c.Project == id))
            {
                msg = "Project Already used in Sales Return !!";
            }
            else if (db.ProTasks.Any(c => c.ProjectId == id))
            {
                msg = "Project Already used in Task !!";
            }
            else if (db.CrossHireReturns.Any(c => c.Project == id))
            {
                msg = "Project Already used in Cross Hire Return !!";
            }
            else if (db.CrossHireReturns.Any(c => c.Project == id))
            {
                msg = "Project Already used in Cross Hire Return !!";
            }
            else if (db.Deliverynotes.Any(c => c.Project == id))
            {
                msg = "Project Already used in Deliverynote !!";
            }
            else if (db.HireReturns.Any(c => c.Project == id))
            {
                msg = "Project Already used in HireReturn !!";
            }
            else if (db.MaterialRequisitions.Any(c => c.Project == id))
            {
                msg = "Project Already used in Material Requisition !!";
            }
            else if (db.Payments.Any(c => c.Project == id))
            {
                msg = "Project Already used in Payment !!";
            }
            else if (db.Productions.Any(c => c.Project == id))
            {
                msg = "Project Already used in Production !!";
            }
            else if (db.ProFormas.Any(c => c.Project == id))
            {
                msg = "Project Already used in ProForma !!";
            }
            else if (db.PurchaseQuotations.Any(c => c.Project == id))
            {
                msg = "Project Already used in Purchase Quotation !!";
            }
            else if (db.Receipts.Any(c => c.Project == id))
            {
                msg = "Project Already used in Receipt !!";
            }
            else if (db.SalesOrders.Any(c => c.Project == id))
            {
                msg = "Project Already used in Sales Order !!";
            }
            else if (db.Unassembles.Any(c => c.Project == id))
            {
                msg = "Project Already used in Unassemble !!";
            }
            else if (db.PEItemss.Any(c => c.ProjectId == id))
            {
                msg = "Project Already used in Purchase !!";
            }
            else if (db.PurchaseOrderItems.Any(c => c.ProjectId == id))
            {
                msg = "Project Already used in Purchase Order !!";
            }
            else if (db.MRNoteItems.Any(c => c.ProjectId == id))
            {
                msg = "Project Already used in Material Receive Note !!";
            }
            else if (db.AccountsTransactions.Any(c => c.Project == id && c.Purpose == "Journal"))
            {
                msg = "Project Already used in Journal !!";
            }
            else
            {
                msg = null;
            }
            return msg;
        }

        public JsonResult SearchProject(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.Projects
                                  join c in db.Customers on b.Customer equals c.CustomerID into cust
                                  from c in cust.DefaultIfEmpty()
                                  join d in db.Contacts on c.Contact equals d.ContactID into cont
                                  from d in cont.DefaultIfEmpty()
                                  join j in db.Mobiles on c.Contact equals j.Contact into mobi
                                  from j in mobi.DefaultIfEmpty()
                                  where (b.ProjectName.ToLower().Contains(q.ToLower()) || b.ProCode.ToLower().Contains(q.ToLower()) || b.ProjectName.Contains(q)) || (b.ProCode.Contains(q)) ||
                                        (c.CustomerName.ToLower().Contains(q.ToLower()) || c.CustomerName.Contains(q)) || (j.MobileNum.Contains(q)) || (d.Phone.Contains(q))
                                  //&& b.ProjectStatus == 0
                                  select new SelectFormat
                                  {
                                      text = b.ProCode + " - " + b.ProjectName,
                                      id = b.ProjectId
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Projects.Select(b => new SelectFormat //Where(b => b.ProjectStatus == 0)
                {
                    text = b.ProjectName, //each json object will have 
                    id = b.ProjectId
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Project" };
                serialisedJson.Insert(0, initial);
            }

            if (x == "" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "All" };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }

        public JsonResult SearchAllProject(string q, string x, string y, string z)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.Projects
                                  join c in db.Customers on b.Customer equals c.CustomerID into cust
                                  from c in cust.DefaultIfEmpty()
                                  join d in db.Contacts on c.Contact equals d.ContactID into cont
                                  from d in cont.DefaultIfEmpty()
                                  join j in db.Mobiles on c.Contact equals j.Contact into mobi
                                  from j in mobi.DefaultIfEmpty()
                                  where (b.ProjectName.ToLower().Contains(q.ToLower()) || b.ProCode.ToLower().Contains(q.ToLower()) || b.ProjectName.Contains(q)) || (b.ProCode.Contains(q)) ||
                                        (c.CustomerName.ToLower().Contains(q.ToLower()) || c.CustomerName.Contains(q)) || (j.MobileNum.Contains(q)) || (d.Phone.Contains(q))
                                  // && b.ProjectStatus == 0
                                  select new SelectFormat
                                  {
                                      text = b.ProCode + "-" + b.ProjectName,
                                      id = b.ProjectId
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Projects.Select(b => new SelectFormat//Where(b => b.ProjectStatus == 0)
                {
                    text = b.ProCode + " - " + b.ProjectName, //each json object will have 
                    id = b.ProjectId
                }).OrderBy(b => b.text).ToList();

            }
            if (z == "inactive")
            {
                var inact = new SelectFormat() { id = -2, text = "InActive Projects" };
                serialisedJson.Insert(0, inact);
            }
            if (y == "active")
            {
                var act = new SelectFormat() { id = -1, text = "Active Projects" };
                serialisedJson.Insert(0, act);
            }
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }
        public JsonResult SearchAllProjectByCustomer(string q, long? customer, string x, string y, string z)
        {
            string NoProj = "--No Project--";
            var check = (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q));
            List<SelectStatusFormat> serialisedJson = (from b in db.Projects
                                                       join c in db.Customers on b.Customer equals c.CustomerID into cust
                                                       from c in cust.DefaultIfEmpty()
                                                       join d in db.Contacts on c.Contact equals d.ContactID into cont
                                                       from d in cont.DefaultIfEmpty()
                                                       join j in db.Mobiles on c.Contact equals j.Contact into mobi
                                                       from j in mobi.DefaultIfEmpty()
                                                       where customer != null && b.Customer == customer && (check == true || (b.ProjectName.ToLower().Contains(q.ToLower()) || b.ProCode.ToLower().Contains(q.ToLower()) || b.ProjectName.Contains(q)) || (b.ProCode.Contains(q)) ||
                                                             (c.CustomerName.ToLower().Contains(q.ToLower()) || c.CustomerName.Contains(q)) || (j.MobileNum.Contains(q)) || (d.Phone.Contains(q)))
                                                       //&& b.ProjectStatus == 0
                                                       select new SelectStatusFormat
                                                       {
                                                           id = b.ProjectId,
                                                           text = b.ProCode + " - " + b.ProjectName,
                                                           status = b.Status
                                                       }).OrderBy(b => b.text).ToList();

            if (x == "--No Project--" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || NoProj.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectStatusFormat() { id = 0, text = NoProj };
                serialisedJson.Insert(0, initial);
            }
            if (z == "inactive")
            {
                var inact = new SelectStatusFormat() { id = -2, text = "InActive Projects" };
                serialisedJson.Insert(0, inact);
            }
            if (y == "active")
            {
                var act = new SelectStatusFormat() { id = -1, text = "Active Projects" };
                serialisedJson.Insert(0, act);
            }
            if (x == "all" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectStatusFormat() { id = 0, text = "All" };
                serialisedJson.Insert(0, initial);
            }

            IEnumerable<SelectStatusFormat> otherproject = (from b in db.Projects
                                                            where b.Customer == -2
                                                            && (q == null || b.ProjectName.ToLower().Contains(q.ToLower()) || b.ProCode.ToLower().Contains(q.ToLower()) || b.ProjectName.Contains(q) || b.ProCode.Contains(q))
                                                            select new SelectStatusFormat
                                                            {
                                                                id = b.ProjectId,
                                                                text = b.ProCode + " - " + b.ProjectName,
                                                                status = b.Status
                                                            }).Distinct().OrderBy(b => b.text).ToList();

            var projectlist = serialisedJson.Union(otherproject);

            return Json(projectlist);
        }


        public JsonResult SearchProjectByCustomer(string q, string x, long? customer)
        {
            string NoProj = "--No Project--";
            string stt = "All";
            var check = (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q));
            List<SelectStatusFormat> serialisedJson = (from b in db.Projects
                                                       join c in db.Customers on b.Customer equals c.CustomerID into cust
                                                       from c in cust.DefaultIfEmpty()
                                                       where (customer == 0 || (customer != null && b.Customer == customer))
                                                       //&& b.ProjectStatus == 0
                                                       && (q == null || b.ProjectName.ToLower().Contains(q.ToLower()) || b.ProCode.ToLower().Contains(q.ToLower()) || b.ProjectName.Contains(q) || b.ProCode.Contains(q))
                                                       select new SelectStatusFormat
                                                       {
                                                           id = b.ProjectId,
                                                           text = b.ProCode + " - " + b.ProjectName,
                                                           status = b.Status
                                                       }).Distinct().OrderBy(b => b.text).ToList();

            if (x == "--No Project--" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || NoProj.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectStatusFormat() { id = 0, text = NoProj };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectStatusFormat() { id = 0, text = "Select Project" };
                serialisedJson.Insert(0, initial);
            }
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectStatusFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            IEnumerable<SelectStatusFormat> otherproject = (from b in db.Projects
                                                            where b.Customer == -2
                                                            && (q == null || b.ProjectName.Contains(q) || b.ProCode.Contains(q))
                                                            select new SelectStatusFormat
                                                            {
                                                                id = b.ProjectId,
                                                                text = b.ProCode + " - " + b.ProjectName,
                                                                status = b.Status
                                                            }).Distinct().OrderBy(b => b.text).ToList();

            var projectlist = serialisedJson.Union(otherproject);

            return Json(projectlist.Take(10).ToList());
        }
        public JsonResult SearchProjectByCustomerOnly(string q, string x, long? customer)
        {
            string NoProj = "--No Project--";
            string stt = "All";
            var check = (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q));
            List<SelectStatusFormat> serialisedJson = (from b in db.Projects
                                                       join c in db.Customers on b.Customer equals c.CustomerID into cust
                                                       from c in cust.DefaultIfEmpty()
                                                       where customer != null && b.Customer == customer
                                                       //&& b.ProjectStatus == 0
                                                       && (q == null || b.ProjectName.ToLower().Contains(q.ToLower()) || b.ProCode.ToLower().Contains(q.ToLower()) || b.ProjectName.Contains(q) || b.ProCode.Contains(q))
                                                       select new SelectStatusFormat
                                                       {
                                                           id = b.ProjectId,
                                                           text = b.ProCode + " - " + b.ProjectName,
                                                           status = b.Status
                                                       }).Distinct().OrderBy(b => b.text).ToList();

            if (x == "--No Project--" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || NoProj.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectStatusFormat() { id = 0, text = NoProj };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectStatusFormat() { id = 0, text = "Select Project" };
                serialisedJson.Insert(0, initial);
            }
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectStatusFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }

            var projectlist = serialisedJson;

            return Json(projectlist.Take(10).ToList());
        }
        public JsonResult SearchProjectByAcc(string q, string x, long? account)
        {
            var customer = db.Customers.Where(a => a.Accounts == account).Select(a => a.CustomerID).FirstOrDefault();

            string NoProj = "--No Project--";
            var check = (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q));
            List<SelectStatusFormat> serialisedJson = (from b in db.Projects
                                                       join c in db.Customers on b.Customer equals c.CustomerID into cust
                                                       from c in cust.DefaultIfEmpty()
                                                       where b.Customer == customer
                                                       && (q == null || b.ProjectName.ToLower().Contains(q.ToLower()) || b.ProCode.ToLower().Contains(q.ToLower()) || b.ProjectName.Contains(q) || b.ProCode.Contains(q))
                                                       //&& b.ProjectStatus == 0
                                                       select new SelectStatusFormat
                                                       {
                                                           id = b.ProjectId,
                                                           text = b.ProCode + " - " + b.ProjectName,
                                                           status = b.Status
                                                       }).Distinct().OrderBy(b => b.text).ToList();

            if (x == "--No Project--" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || NoProj.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectStatusFormat() { id = 0, text = NoProj };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectStatusFormat() { id = 0, text = "Select Project" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public JsonResult SearchProjectByCustomerActive(string q, long? customer, string x)
        {
            string NoProj = "--No Project--";
            var check = (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q));
            List<SelectStatusFormat> serialisedJson = (from b in db.Projects
                                                       join c in db.Customers on b.Customer equals c.CustomerID into cust
                                                       from c in cust.DefaultIfEmpty()
                                                       join d in db.Contacts on c.Contact equals d.ContactID into cont
                                                       from d in cont.DefaultIfEmpty()
                                                       join j in db.Mobiles on c.Contact equals j.Contact into mobi
                                                       from j in mobi.DefaultIfEmpty()
                                                       where customer != null && b.Customer == customer && (check == true || b.ProjectName.ToLower().Contains(q.ToLower()) || b.ProCode.ToLower().Contains(q.ToLower()) || (b.ProjectName.Contains(q)) || (b.ProCode.Contains(q)) ||
                                                             (c.CustomerName.ToLower().Contains(q.ToLower()) || c.CustomerName.Contains(q)) || (j.MobileNum.Contains(q)) || (d.Phone.Contains(q)))
                                                             && b.Status == Status.active
                                                       //&& b.ProjectStatus == 0
                                                       select new SelectStatusFormat
                                                       {
                                                           id = b.ProjectId,
                                                           text = b.ProCode + " - " + b.ProjectName,
                                                           status = b.Status
                                                       }).OrderBy(b => b.text).ToList();

            if (x == "--No Project--" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || NoProj.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectStatusFormat() { id = 0, text = NoProj };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectStatusFormat() { id = 0, text = "Select Project" };
                serialisedJson.Insert(0, initial);
            }
            IEnumerable<SelectStatusFormat> otherproject = (from b in db.Projects
                                                            where b.Customer == -2
                                                            && (q == null || b.ProjectName.Contains(q) || b.ProCode.Contains(q))
                                                            select new SelectStatusFormat
                                                            {
                                                                id = b.ProjectId,
                                                                text = b.ProCode + " - " + b.ProjectName,
                                                                status = b.Status
                                                            }).Distinct().OrderBy(b => b.text).ToList();

            var projectlist = serialisedJson.Union(otherproject);

            return Json(projectlist);
        }

        public JsonResult ActiveProjectByCustomer(string q, long? customer, string x)
        {
            List<SelectFormat> serialisedJson;
            string NoProj = "--No Project--";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from b in db.Projects
                                  join c in db.Customers on b.Customer equals c.CustomerID into cust
                                  from c in cust.DefaultIfEmpty()
                                  join d in db.Contacts on c.Contact equals d.ContactID into cont
                                  from d in cont.DefaultIfEmpty()
                                  join j in db.Mobiles on c.Contact equals j.Contact into mobi
                                  from j in mobi.DefaultIfEmpty()
                                  where customer != null && b.Customer == customer && (b.ProjectName.ToLower().Contains(q.ToLower()) || b.ProCode.ToLower().Contains(q.ToLower()) || (b.ProjectName.Contains(q)) || (b.ProCode.Contains(q)) ||
                                  (c.CustomerName.ToLower().Contains(q.ToLower()) || c.CustomerName.Contains(q)) || (j.MobileNum.Contains(q)) || (d.Phone.Contains(q))) && b.Status == Status.active
                                  //&& b.ProjectStatus == 0
                                  select new SelectFormat
                                  {
                                      text = b.ProCode + " - " + b.ProjectName,
                                      id = b.ProjectId
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Projects.Where(b => b.Customer == customer && customer != null && b.Status == Status.active).Select(b => new SelectFormat//b.ProjectStatus == 0
                {
                    text = b.ProCode + " - " + b.ProjectName, //each json object will have 
                    id = b.ProjectId
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "--No Project--" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || NoProj.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = NoProj };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Project" };
                serialisedJson.Insert(0, initial);
            }
            IEnumerable<SelectFormat> otherproject = (from b in db.Projects
                                                      where b.Customer == -2
                                                      && (q == null || b.ProjectName.Contains(q) || b.ProCode.Contains(q))
                                                      select new SelectFormat
                                                      {
                                                          id = b.ProjectId,
                                                          text = b.ProCode + " - " + b.ProjectName,
                                                      }).Distinct().OrderBy(b => b.text).ToList();

            var projectlist = serialisedJson.Union(otherproject);

            return Json(projectlist);
        }


        [HttpGet]
        public JsonResult GetProjectById(int proId)
        {
            var v = (from a in db.Projects
                     where a.ProjectId == proId
                     select new
                     {
                         a.Location,
                     }).FirstOrDefault();
            return Json(v);
        }

        private long GetProNo()
        {
            Int64 SENo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "Project").Select(a => a.number).FirstOrDefault();
            if ((db.Projects.Select(p => p.ProNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                if (number == 0)
                {
                    SENo = 1;
                }
                else
                {
                    SENo = number;
                }
            }
            else
            {
                SENo = db.Projects.Max(p => p.ProNo + 1);
            }

            return SENo;
        }
        private string InvoiceNo(Int64 SENo = 0, string billNo = null)
        {
            var companyPrefix = db.CodePrefixs.Where(a => a.section == "Project").Select(a => a.prefix).FirstOrDefault();
            if (billNo == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "Project").Select(a => a.number).FirstOrDefault();
                if ((db.Projects.Select(p => p.ProNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    billNo = (number == 0) ? (companyPrefix + 1) : (companyPrefix + number);
                }
                else
                {
                    SENo = db.Projects.Max(p => p.ProNo + 1);
                    billNo = companyPrefix + SENo;
                    if (BillExist(billNo))
                    {
                        billNo = InvoiceNo(SENo, billNo);
                    }
                }
            }
            else
            {
                SENo = SENo + 1;
                billNo = companyPrefix + SENo;
                if (BillExist(billNo))
                {
                    billNo = InvoiceNo(SENo, billNo);
                }

            }
            return billNo;
        }
        private bool BillExist(string SENo)
        {
            var Exists = db.Projects.Any(c => c.ProCode == SENo);
            bool res = (Exists) ? true : false;
            return res;
        }
        public JsonResult SearchProjectName(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Projects.Where(p => p.ProjectName.ToLower().Contains(q.ToLower()) || p.ProCode.ToLower().Contains(q.ToLower()) || p.ProjectName.Contains(q) || p.ProCode.Contains(q))//Where(b => b.ProjectStatus == 0)
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.ProCode + " - " + b.ProjectName, //each json object will have 
                                      id = b.ProjectId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Projects.Select(b => new SelectFormat//Where(b => b.ProjectStatus == 0)
                {
                    text = b.ProCode + " - " + b.ProjectName, //each json object will have 
                    id = b.ProjectId
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public ActionResult GetCustDetailProject(long Cuss)
        {
            var v = (from a in db.Projects
                     join c in db.Customers on a.Customer equals c.CustomerID into rec
                     from c in rec.DefaultIfEmpty()
                     where c.CustomerID == Cuss && c.Type == CRMCustomerType.Customer
                     select new
                     {
                         ProjectId = a.ProjectId,
                         ProjectName = a.ProCode + " " + a.ProjectName
                     }).OrderBy(b => b.ProjectName);

            var data = v.ToList();
            return Json(new { data = data });
        }
        public JsonResult SearchProjectLocation(string q, string x)
        {
            List<SelectUserFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.Projects.Where(p => p.Location.ToLower().Contains(q.ToLower()) || p.Location.Contains(q))//p.TaskStat==Stat.Open 
                                  .Select(b => new SelectUserFormat
                                  {
                                      text = b.Location, //each json object will have 
                                      id = b.Location
                                  }).Distinct().OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Projects.Select(b => new SelectUserFormat//Where(b=> b.TaskStat == Stat.Open)
                {
                    text = b.Location, //each json object will have 
                    id = b.Location
                }).Distinct().OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectUserFormat() { id = "0", text = stt };
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
