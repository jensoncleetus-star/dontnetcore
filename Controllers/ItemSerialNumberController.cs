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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    public class ItemSerialNumberController  : BaseController
    {
        // GET: ItemSerialNumber
        ApplicationDbContext db;
        Common com;
        public ItemSerialNumberController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create(long? itemid)
        {
            ViewBag.ddlItem = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                              }, "Value", "Text", 1);
            if(itemid!=null)
            {
                ViewBag.itemname = (from i in db.Items where i.ItemID == itemid select new { i.ItemName }).ToArray();
                ViewBag.itemname = ViewBag.itemname[0].ItemName;
            }
            companySet();
            return View();
        }
        public ActionResult Create2(long itemid)
        {
            ViewBag.ddlItem = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                              }, "Value", "Text", 1);
            ViewBag.serialnos = (from a in db.ItemSerialNo
                                 where a.itemid == itemid
                              select new lstitems
                              {
                                    serialno= a.serialno,
                                    itemid= a.itemid,
                                     expirydate= a.expirydate,
                                     
                                 }
                ).ToList();
            companySet();
            return View();
        }
        [HttpPost]
        public ActionResult Create(ItemSerialNumberView modals)
        {
            long itemid = modals.itemid;
            foreach(var serial in modals.serialnoobjs)
            {
                if (serial.serialno !=null)
                {
                    db.ItemSerialNo.RemoveRange(db.ItemSerialNo.Where(o => o.serialno == serial.serialno && o.itemid == itemid));
                    db.SaveChanges();
                    ItemSerialNumber cn = new ItemSerialNumber();
                    cn.expirydate = DateTime.Parse(serial.expirydate, new CultureInfo("en-GB"));
                    cn.itemid = itemid;
                    cn.serialno = serial.serialno;
                    db.ItemSerialNo.Add(cn);
                    db.SaveChanges();
                }

            }

            return RedirectToAction("Create", "ItemSerialNumber");
        }
    }
}