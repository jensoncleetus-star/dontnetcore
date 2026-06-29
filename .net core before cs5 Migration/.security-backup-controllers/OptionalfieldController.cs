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
using Microsoft.AspNetCore.Identity;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class OptionalfieldController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public OptionalfieldController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Checklist
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {


            ViewBag.Fields = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Sales", Value="Sales"},
                new SelectListItem() {Text = "LPO", Value="LPO"},
                new SelectListItem() {Text = "Sales Return", Value="SReturn"},
                new SelectListItem() {Text = "Purchase", Value="Purchase"},
                new SelectListItem() {Text = "Purchase Return", Value="PReturn"},
                new SelectListItem() {Text = "Quotation", Value="Quot"},
                new SelectListItem() {Text = "ProForma", Value="ProForma"},
                new SelectListItem() {Text = "DeliveryNote", Value="DvNote"},
                new SelectListItem() {Text = "Sales Order", Value="SOrder"},
                new SelectListItem() {Text = "Purchase Quotation", Value="PQuot"},
                new SelectListItem() {Text = "Payment", Value="Payment"},
                new SelectListItem() {Text = "Receipt", Value="Receipt"},
                new SelectListItem() {Text = "Journal", Value="Journal"},
                new SelectListItem() {Text = "Production", Value="Production"},
                new SelectListItem() {Text = "Unassemble", Value="Unassemble"},
                new SelectListItem() {Text = "ContraVoucher", Value="CVoucher"},
                new SelectListItem() {Text = "StockTransfer", Value="StkTrans"},
                new SelectListItem() {Text = "Material Receive Note", Value="MRNote"},
                new SelectListItem() {Text = "Material Requisition", Value="MR"},
                new SelectListItem() {Text = "JobCard", Value="JobCard"},
                new SelectListItem() {Text = "Hire Return", Value="HReturn"},
                new SelectListItem() {Text = "PackingList", Value="Pklist"},
                new SelectListItem() {Text = "StockJournal", Value="StkJnl"},
                new SelectListItem() {Text = "Task", Value="Task"},
                new SelectListItem() {Text = "Project", Value="Project"},
                new SelectListItem() {Text = "Leads", Value="Leads"},
                new SelectListItem() {Text = "AMC", Value="AMC"},
                new SelectListItem() {Text = "WorkCompletion", Value="WorkCompletion"},
                new SelectListItem() {Text = "Warrenty", Value="Warrenty"},
                new SelectListItem() {Text = "SOA", Value="SOA"},
            }, "Value", "Text");


            return View();
        }
                
        [HttpGet]
        public JsonResult GetmappingItems(string Name)
        {
            var ConD = (from a in db.FieldMappings
                        where a.Section == Name
                        select new
                        {
                            Print = a.Print,
                            Active = a.Status,
                            FieldName=a.FieldName,
                            Id=a.FieldMappingId,
                            a.Type
                        }).ToList();
           if(ConD.Count()>0)
            return Json(ConD);
            else
            {
                FieldMapping a = new FieldMapping();
                a.Section = Name;
                a.Print = FMPrint.No;
                a.Status = Status.inactive;
                a.FieldName = "ref1";
                a.Field = "Ref1";
                a.Type = "Text";
                db.FieldMappings.Add(a);
                db.SaveChanges();
                
                a = new FieldMapping();
                a.Section = Name;
                a.Print = FMPrint.No;
                a.Status = Status.inactive;
                a.FieldName = "ref2";
                a.Field = "Ref2";
                a.Type = "Text";
                db.FieldMappings.Add(a);
                db.SaveChanges();
                a = new FieldMapping();
                a.Section = Name;
                a.Print = FMPrint.No;
                a.Status = Status.inactive;
                a.FieldName = "ref3";
                a.Field = "Ref3";
                a.Type = "Text";
                db.FieldMappings.Add(a);
                db.SaveChanges();
                a = new FieldMapping();
                a.Section = Name;
                a.Print = FMPrint.No;
                a.Status = Status.inactive;
                a.FieldName = "ref4";
                a.Field = "Ref4";
                a.Type = "Text";
                db.FieldMappings.Add(a);
                db.SaveChanges();
                a = new FieldMapping();
                a.Section = Name;
                a.Print = FMPrint.No;
                a.Status = Status.inactive;
                a.FieldName = "ref5";
                a.Field = "Ref5";
                a.Type = "Text";
                db.FieldMappings.Add(a);
                db.SaveChanges();
                var Con = (from aa in db.FieldMappings
                            where aa.Section == Name
                            select new
                            {
                                Print = aa.Print,
                                Active = aa.Status,
                                FieldName = aa.FieldName,
                                Id = aa.FieldMappingId,
                                aa.Type
                            }).ToList();
            
                    return Json(Con);
            }
        }
        [HttpGet]
        public JsonResult GetmappingItemsLock(string Name)
        {
            var ConD = (from a in db.FieldMappingsLocks 
                        where a.Section == Name
                        select new
                        {
                            fromdate=a.fromdate,
                            todate=a.todate
                        }).ToList();
            return Json(ConD);
        }
        [RedirectingAction]
        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Create Checklist")]
        public ActionResult CreateOptionalField(string[][] array, string[] data)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid && array.Length>0)
            {

                var UserId = User.Identity.GetUserId();
                var stage = data[0];
                
                foreach (var arr in array)
                {
                    int id = Convert.ToInt32(arr[3]);
                    FieldMapping fm = db.FieldMappings.Find(id);                    
                    fm.FieldName = arr[0];
                    fm.Status = (arr[0]=="")?Status.inactive: ((arr[1] == "true") ? Status.active : Status.inactive);
                    fm.Print = (data[0] != "Project" && data[0] != "Task")? ((arr[1] == "true") ? FMPrint.Yes : FMPrint.No): FMPrint.No;
                    fm.Type= Convert.ToString(arr[2]);
                    db.Entry(fm).State = EntityState.Modified;
                    db.SaveChanges();
                }

                msg = "FieldMapping Status added successfully.";
                stat = true;
                com.addlog(LogTypes.Created, UserId, "FieldMapping", "FieldMapping", findip(), Id, "FieldMapping Successfully");

            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public ActionResult CreateOptionalFieldLock(string[][] array, string[] data)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;
            if (ModelState.IsValid && array.Length > 0)
            {

                var UserId = User.Identity.GetUserId();
                var stage = data[0];

                foreach (var arr in array)
                {

                    var fm = db.FieldMappingsLocks.Any(o => o.Section == stage);
                    if(fm)
                    {
                        db.FieldMappingsLocks.RemoveRange(db.FieldMappingsLocks.Where(o => o.Section == stage));
                    }
                    if (arr[0] != "")
                    {
                        FieldMappingLock a = new FieldMappingLock();
                        a.fromdate = Convert.ToDateTime(arr[0]);
                        a.todate = Convert.ToDateTime(arr[1]);
                        a.Section = stage;

                        db.FieldMappingsLocks.Add(a);
                    }
                    db.SaveChanges();
                }

                msg = "Lock Date added successfully.";
                stat = true;
                com.addlog(LogTypes.Created, UserId, "Lock Date", "Lock Date", findip(), Id, "FieldMapping Successfully");

            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

    }
}
