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
using System;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.Models;
using Microsoft.AspNetCore.Identity;
using QuickSoft.ViewModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class PrefixMasterController : BaseController
    {
        ApplicationDbContext db;
        Common com;

        public PrefixMasterController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        [HttpGet]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Prefix List")]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Prefix List")]
        public JsonResult GetPrefix(string PCode)
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
            var uEdit = User.IsInRole("Edit Prefix");
            var uDelete = User.IsInRole("Delete Prefix  ");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = db.PrefixMasters.Select(b => new
            {
                Id = b.Id,
                PrefixCode = b.PrefixCode,
                Description = b.Description,
                //LastNo = b.LastNo,
                LastNo = db.ItemPrefixs.Where(a => a.Prefix == b.Id).Select(a => a.No).DefaultIfEmpty().Max(),
                Dev = uDev,
                Edit = uEdit,
                Delete = uDelete

            }).Where(p => p.PrefixCode == PCode || PCode == null || PCode == "");
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                v = v.Where(p => p.Id.ToString().ToLower().Contains(search.ToLower()));
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
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Create Prefix")]
        public ActionResult Create()
        {
            Int64 number = db.ItemPrefixs.Select(a => a.No).AsEnumerable().DefaultIfEmpty(0).Max();
            var pmvm = new PrefixMasterViewModel
            {
                LastNo = number.ToString("D5")
            };

            var currency = db.CurrencyMasters.Select(s => new
            {
                Id = s.Id,
                Name = s.CurrencyCode
            }).ToList();
            ViewBag.Crc = QkSelect.List(currency, "Id", "Name");


            var Category = db.ItemCategorys.Where(p => p.Parent == 1 || p.ItemCategoryID == 1)
                    .Select(s => new
                    {
                        Id = s.ItemCategoryID,
                        CategoryName = s.ItemCategoryName
                    }).ToList();
            ViewBag.ItemCategory = QkSelect.List(Category, "Id", "CategoryName");

            var Brand = db.ItemBrands
                   .Select(s => new
                   {
                       Id = s.ItemBrandID,
                       BrandName = s.ItemBrandName
                   }).ToList();
            ViewBag.ItemBrand = QkSelect.List(Brand, "Id", "BrandName");

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


            return PartialView(pmvm);
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Create Prefix")]
        public JsonResult Create(PrefixMasterViewModel pvm)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            var UserId = User.Identity.GetUserId();
            var Exists = db.PrefixMasters.Any(c => c.PrefixCode == pvm.PrefixCode && c.CreatedBy == UserId);
            if (Exists)
            {
                msg = "Prefix Code already exists.";
                stat = false;
            }
            else
            {
                long Branch = 0;

                var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
                var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

                if (BranchCheck == Status.active)
                {
                    Branch = pvm.Branch;
                }
                else
                {
                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                }
                var today = Convert.ToDateTime(System.DateTime.Now);

                var sn = new PrefixMaster
                {
                    PrefixCode = pvm.PrefixCode,
                    LastNo = "",
                    Branch = Branch,
                    Brand = pvm.Brand,
                    Category = pvm.Category,
                    CCCode = pvm.CCCode,
                    ConRate = (pvm.ConRate==null)?"AED":pvm.ConRate,
                    Country = pvm.Country,
                    Currency = pvm.Currency,
                    Type = pvm.Type,
                    Description = pvm.Description,
                    Status = pvm.Status,
                    editable = choice.Yes,
                    CreatedBy = UserId,
                    
                    CreatedDate = System.DateTime.Now,

                };
                db.PrefixMasters.Add(sn);
                db.SaveChanges();
                Id = sn.Id;
                com.addlog(LogTypes.Created, UserId, "PrefixMaster", "PrefixMasters", findip(), Id, "Prefix Added Successfully");
                msg = "Successfully added Prefix details.";
                stat = true;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id } };

        }

        [HttpGet]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Prefix")]
        public ActionResult Edit(long? id)
        {

            var currency = db.CurrencyMasters.Select(s => new
            {
                Id = s.Id,
                Name = s.CurrencyCode
            }).ToList();
            ViewBag.Crc = QkSelect.List(currency, "Id", "Name");


            var Category = db.ItemCategorys.Where(p => p.Parent == 1 || p.ItemCategoryID == 1)
                    .Select(s => new
                    {
                        Id = s.ItemCategoryID,
                        CategoryName = s.ItemCategoryName
                    }).ToList();
            ViewBag.ItemCategory = QkSelect.List(Category, "Id", "CategoryName");

            var Brand = db.ItemBrands
                   .Select(s => new
                   {
                       Id = s.ItemBrandID,
                       BrandName = s.ItemBrandName
                   }).ToList();
            ViewBag.ItemBrand = QkSelect.List(Brand, "Id", "BrandName");

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

            PrefixMaster Pm = db.PrefixMasters.Find(id);
            Int64 number = db.ItemPrefixs.Where(a => a.Prefix == id).Select(a => a.No).DefaultIfEmpty().Max();           

            PrefixMasterViewModel pvm = new PrefixMasterViewModel();
            pvm.PrefixCode = Pm.PrefixCode;
            pvm.LastNo = number.ToString("D5");
            pvm.Branch = Pm.Branch;
            pvm.Brand = Pm.Brand;
            pvm.Category = Pm.Category;
            pvm.CCCode = Pm.CCCode;
            pvm.ConRate = Pm.ConRate;
            pvm.Country = Pm.Country;
            pvm.Currency = Pm.Currency;
            pvm.Type = Pm.Type;
            pvm.Description = Pm.Description;

            if (Pm == null)
            {
                return NotFound();
            }

            return PartialView(pvm);
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Edit Prefix")]
        public JsonResult Edit(int id, PrefixMasterViewModel pvm)
        {

            bool stat = false;
            string msg;
            Int64 Id = 0;
            var Exists = db.PrefixMasters.Any(c => c.PrefixCode == pvm.PrefixCode && c.Id != id);
            if (Exists)
            {
                msg = "Prefix Code Already exists.";
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
                    Branch = pvm.Branch;
                }
                else
                {
                    Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
                }

                PrefixMaster opm = db.PrefixMasters.Find(id);
                opm.PrefixCode = pvm.PrefixCode;
                opm.Branch = Branch;
                opm.Brand = pvm.Brand;
                opm.Category = pvm.Category;
                opm.CCCode = pvm.CCCode;
                opm.ConRate = pvm.ConRate;
                opm.Country = pvm.Country;
                opm.Currency = pvm.Currency;
                opm.Type = pvm.Type;
                opm.Description = pvm.Description;
                opm.Status = pvm.Status;

                db.SaveChanges();
                Id = opm.Id;
                com.addlog(LogTypes.Updated, UserId, "PrefixMaster", "PrefixMasters", findip(), Id, "Prefix Master Updated Successfully");


                msg = "Successfully Updated Prefix Master Details.";
                stat = true;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id } };

        }

        [HttpGet]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete Prefix")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PrefixMaster info = db.PrefixMasters.Find(id);
            if (info == null)
            {
                return NotFound();
            }

            return PartialView(info);
        }

        [HttpPost]
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Delete Prefix")]
        public JsonResult Delete(int id, IFormCollection collection)
        {
            bool stat = false;
            string msg;

            var Exists = (db.Items.Any(c => c.Prefix == id));
            if (Exists)
            {
                msg = "Unable to Delete Prefix, It's Already Used.";
                stat = false;
            }
            else
            {
                PrefixMaster Desginfo = db.PrefixMasters.Find(id);
                if(Desginfo != null)
                {
                    db.PrefixMasters.RemoveRange(db.PrefixMasters.Where(a => a.Id == id));
                }               

                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "PrefixMaster", "PrefixMasters", findip(), Desginfo.Id, "Prefix Deleted Successfully");
                db.SaveChanges();
                stat = true;
                msg = "Successfully Deleted Prefix masters  details.";
            }


            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        public JsonResult GetPrefixCode(long prefix)
        {
            var pre = PrefixCodes(prefix);
            Regex re = new Regex(@"([a-zA-Z]+)(\d+)");
            Match result = re.Match(pre);
            string alphaPart = result.Groups[1].Value;
            string num = result.Groups[2].Value;

            int length = (result.Groups[2].Value).Length;
            if (length < 5)
            {
                string s = new String('0', 5 - length);
                string newnum = s + num;
                string newprefix = alphaPart + newnum;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { pre = newprefix } };
            }
            else
            {
                return new QuickSoft.Models.LegacyJsonResult { Data = new { pre = pre } };
            }

        }

        public JsonResult SearchPrefix(string q, string x)
        {

            List<SelectFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = db.PrefixMasters.Where(p => p.PrefixCode.ToLower().Contains(q.ToLower()) || p.PrefixCode.Contains(q))
                                  .Select(b => new SelectFormat
                                  {
                                      text = b.PrefixCode, //each json object will have 
                                      id = b.Id
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.PrefixMasters.Select(b => new SelectFormat
                {
                    text = b.PrefixCode, //each json object will have 
                    id = b.Id
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat() { id = 0, text = stt };
                serialisedJson.Insert(0, initial);
            }
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Prefix" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }


        private string PrefixCodes(Int64 pre = 0, Int64 INo = 0, string ICode = null)
        {
            var prefix = (pre != 0) ? db.PrefixMasters.Where(a => a.Id == pre).Select(a => a.PrefixCode).FirstOrDefault() : "";
            if (ICode == null)
            {
                Int64 number = db.ItemPrefixs.Where(a => a.Prefix == pre).Select(a => a.No).AsEnumerable().DefaultIfEmpty(0).Max();
                if ((db.Items.Select(p => p.ItemID).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    if (number == 0)
                    {
                        ICode = prefix + 1;
                    }
                    else
                    {
                        number++;
                        ICode = prefix + number;
                    }
                }
                else
                {
                    INo = number + 1;
                    ICode = prefix + INo;
                    if (PrefixCodeExist(ICode))
                    {
                        ICode = PrefixCodes(pre, INo, ICode);
                    }
                }
            }
            else
            {
                INo = INo + 1;
                ICode = prefix + INo;
                if (PrefixCodeExist(ICode))
                {
                    ICode = PrefixCodes(pre, INo, ICode);
                }
            }

            return ICode;
        }
        private bool PrefixCodeExist(string Code)
        {
            var Exists = db.Items.Any(c => c.ItemCode == Code);
            bool res = (Exists) ? true : false;
            return res;
        }
        private long GetprefixNo(long pre)
        {
            Int64 No = 0;
            Int64 number = db.ItemPrefixs.Where(a => a.Prefix == pre).Select(a => a.No).AsEnumerable().DefaultIfEmpty(0).Max();
            if (number == 0)
            {
                No = 1;
            }
            else
            {
                No = number + 1;
            }
            return No;
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
