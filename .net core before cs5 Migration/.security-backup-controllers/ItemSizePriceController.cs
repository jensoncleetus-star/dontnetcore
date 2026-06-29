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
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace QuickSoft.Controllers
{
    public class ItemSizePriceController : BaseController
    {
        ApplicationDbContext db;
        Common com;

        public ItemSizePriceController()
        {
            db  = new ApplicationDbContext();
            com = new Common();
        }

        //Get-Create
        public ActionResult Create()
        {
            var Model = new itemsizeprice
            {
            };          

            //DropDown Items
            ViewBag.Items = QkSelect.List(
                                         new List<SelectListItem>
                                         {
                                            new SelectListItem { Selected = false, Text = "", Value = ""},
                                         }, "Value", "Text", 1);


            //DropDown ItemSizes
            ViewBag.ItemSizes = QkSelect.List(
                                         new List<SelectListItem>
                                         {
                                            new SelectListItem { Selected = false, Text = "", Value = ""},
                                         }, "Value", "Text", 1);

            return PartialView(Model);
        }

        //Saving
        [HttpPost]
        [ValidateAntiForgeryToken]       
        public JsonResult Create(itemsizeprice ModelObj)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;

            if (ModelState.IsValid)
            {
                var SizePriceExists = db.itemsizeprice.Any(u => u.itemid == ModelObj.itemid && u.sizeid == ModelObj.sizeid);
                if (SizePriceExists)
                {
                    msg = "Item Size Price exists..";
                    stat = false;
                }       
                else if(ModelObj.price <= 0 )
                {
                    msg = "Please Enter a price greater than zero....";
                    stat = false;
                }
                else
                {
                    var UserId = User.Identity.GetUserId();
                    var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();
                    
                    var Obj = new itemsizeprice
                    {
                        itemid = ModelObj.itemid,
                        sizeid = ModelObj.sizeid,
                        price = ModelObj.price
                    };
                    db.itemsizeprice.Add(Obj);
                    db.SaveChanges();
                    Id = Obj.sizepriceid;

                    com.addlog(LogTypes.Created, UserId, "ItemSizePrice", "ItemSizePrice", findip(), Id, "Item Size Price Added Successfully");
                    msg = "Successfully added Item Size Price details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form..";
                stat = false;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id } };
        }

        //Get Index
        public ActionResult Index()
        {
            return View();
        }

        //Function to retrieve data in Index page
        [HttpPost]       
        public JsonResult GetData()
        {
            string search   = Request.Form.GetValues("search[value]")[0];
            var draw        = Request.Form.GetValues("draw").FirstOrDefault();
            var start       = Request.Form.GetValues("start").FirstOrDefault();
            var length      = Request.Form.GetValues("length").FirstOrDefault();

            //Find Order Column
            var sortColumn      = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir   = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

                    var v = (from a in db.itemsizeprice
                             join b in db.Items
                             on a.itemid equals b.ItemID
                             join c in db.ItemSizes
                             on a.sizeid equals c.ItemSizeID
                             select new
                             {
                                 a.sizepriceid,
                                 b.ItemName,
                                 c.ItemSizeName,
                                 a.price
                             });
            //Search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.ItemName.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.ItemSizeName.ToString().ToLower().Contains(search.ToLower()));
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

        // GET: Edit
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            itemsizeprice RetObj = db.itemsizeprice.Find(id);

            if (RetObj == null)
            {
                return NotFound();
            }

            //****** Items    
            var Items = db.Items.Select(s => new
            {
                Id = s.ItemID,
                Name = s.ItemName
            }).ToList();
            ViewBag.Items = QkSelect.List(Items, "Id", "Name");

            //****** ItemSizes             
            var ItemSize = db.ItemSizes.Select(s => new
            {
                Id = s.ItemSizeID,
                Name = s.ItemSizeName
            }).ToList();
            ViewBag.ItemSizes = QkSelect.List(ItemSize, "Id", "Name");

            itemsizeprice Obj = new itemsizeprice();

            Obj.itemid = RetObj.itemid;
            Obj.sizeid = RetObj.sizeid;
            Obj.price  = RetObj.price;         

            return PartialView(Obj);
        }

        // POST: Edit/5
        [HttpPost]
        public JsonResult Edit(itemsizeprice ModelObj, long id)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                itemsizeprice RetObj = db.itemsizeprice.Find(id);

                if (RetObj != null)
                {
                    db.itemsizeprice.RemoveRange(db.itemsizeprice.Where(a => a.sizepriceid == id));
                    db.SaveChanges();
                }

                var SizePriceExists = db.itemsizeprice.Any(u => u.itemid == ModelObj.itemid && u.sizeid == ModelObj.sizeid);
                if (SizePriceExists)
                {
                    msg = "Item Size Price exists..";
                    stat = false;
                }
                else if (ModelObj.price <= 0)
                {
                    msg = "Please Enter a price greater than zero....";
                    stat = false;
                }
                else
                {
                    var Obj = new itemsizeprice
                    {
                        itemid = ModelObj.itemid,
                        sizeid = ModelObj.sizeid,
                        price = ModelObj.price
                    };
                    db.itemsizeprice.Add(Obj);
                    db.SaveChanges();

                    var userid = User.Identity.GetUserId();
                    com.addlog(LogTypes.Updated, userid, "ItemSizePrice", "ItemSizePrices", findip(), Obj.sizepriceid, "Item Size Price Updated Successfully");

                    msg = "Successfully updated Item Size Price details..";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form..";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        //GET:  Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            itemsizeprice congp = db.itemsizeprice.Find(id);
            if (congp == null)
            {
                return NotFound();
            }

            return PartialView(congp);
        }

        //POST Delete
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteAction(long id)
        {
            bool stat;
            string msg;

            //***********Delete from table ItemSizePrices
            var Obj = db.itemsizeprice.Where(a => a.sizepriceid == id).FirstOrDefault();

            if (Obj != null)
            {
                db.itemsizeprice.RemoveRange(db.itemsizeprice.Where(a => a.sizepriceid == id));
                db.SaveChanges();
            }

            stat = true;
            msg = "Successfully deleted Item Size Price details..";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

    }
}