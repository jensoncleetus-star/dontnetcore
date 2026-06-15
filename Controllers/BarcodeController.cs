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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class BarcodeController : BaseController
    {
        //download the font from the link
        //www.idautomation.com/fonts/free/IDAutomationCode39.zip

        // GET: Barcode
        ApplicationDbContext db;
        Common com;
        public BarcodeController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        [QkAuthorize(Roles = "Dev,Print BarCode")]
        public ActionResult Index()
        {
            ViewBag.ddlItem = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                               }, "Value", "Text", 1);
            companySet();
            return View();
           
        }
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Print BarCode,Jewellery Barcode")]
        public JsonResult GetBarcode(int ItemId)
        {
            
            var item = (from b in db.Items
                        where b.ItemID == ItemId //&& b.Barcode!=null
                        select new
                        {
                            Barcode = b.Barcode,
                            ItemName = b.ItemName,
                            ItemPrice = b.SellingPrice
                        }).FirstOrDefault();
            return Json(item);

        }
        [HttpGet]
        //[QkAuthorize(Roles = "Dev,Print BarCode")]
        public ActionResult Config()
        {
            return View();
        }
      
        public ActionResult CreateJewellery()
        {
            ViewBag.JewItems = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                               }, "Value", "Text", 1);
            var Prefix = db.PrefixMasters
                            .Select(s => new
                            {
                                ID = s.Id,
                                Name = s.PrefixCode,
                            })
                            .ToList();
            ViewBag.Prefix = QkSelect.List(Prefix, "ID", "Name");

            ViewBag.SelItems = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                              }, "Value", "Text", 1);

            companySet();
            return View();

        }
      
        public ActionResult CreateAsset()
        {
            //For Dropdown Asset Account
            ViewBag.AssetAccounts = QkSelect.List(
                      new List<SelectListItem>
                      {
                        new SelectListItem { Selected = false, Text = "", Value = ""},
                      }, "Value", "Text", 1);

            //For Dropdown Assets
            ViewBag.ddlAssets = QkSelect.List(
                              new List<SelectListItem>
                              {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                              }, "Value", "Text", 1);

            companySet();
            return View();
        }

        //                    Barcode = a.Barcode,
        //                    ItemName = a.ItemName,
        //                    ItemPrice = a.SellingPrice,
        //                    TagLine1 = b.TagLine1,
        //                    TagLine2 = b.TagLine2,
        //                    TagLine3 = b.TagLine3,
        //                    TagLine4 = b.TagLine4,
        //                    TagLine5 = b.TagLine5,

        public ActionResult ViewJBarCode()
        {
            return View();
        }

        //[HttpGet]
        //            //The Image is drawn based on length of Barcode text.
        //                //The Graphics library object is generated for the Image.
        //                    //The installed Barcode font.

        //                    //White Brush is used to fill the Image with white color.

        //                    //Black Brush is used to draw the Barcode over the Image.

        //                //The Bitmap is saved to Memory Stream.

        //                //The Image is finally converted to Base64 string.


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
