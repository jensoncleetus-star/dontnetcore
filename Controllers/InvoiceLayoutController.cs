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
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class InvoiceLayoutController : BaseController
    {
        // GET: Company
        ApplicationDbContext db;
        Common com;
        public InvoiceLayoutController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        #region invoiceFields
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public JsonResult getFields()
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

            var v = db.InvoiceFields.Select(b => new
            {
                b.Type,
                b.Id,
                b.Order,
                b.Position,
                b.Value,
                b.Lang,
                b.Status,
                b.Section
            });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.Type.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Position.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Value.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Order.ToString().ToLower().Contains(search.ToLower()));
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
        
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create( InvoiceField vmodel)
        {
            var Exists = db.InvoiceFields.Any(u => u.Type == vmodel.Type&& u.Section==vmodel.Section);
            if (Exists)
            {
                Danger("A Invoice with same type exists.", true);
                return RedirectToAction("Create", "InvoiceLayout");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var data = new InvoiceField
                    {
                        Type = vmodel.Type,
                        Position = vmodel.Position,
                        Value = vmodel.Value,
                        Order = vmodel.Order,
                        Lang = vmodel.Lang,
                        Status = vmodel.Status,
                        Section = vmodel.Section                        
                    };
                    db.InvoiceFields.Add(data);
                    db.SaveChanges();
                    var userid = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, userid, "InvoiceField", "InvoiceFields", findip(), data.Id, "Invoice Fields added Successfully");
                    // end log updates
                    Success("Successfully added Invoice Field details.", true);
                    return RedirectToAction("Create", "InvoiceLayout");
                }
                else
                {
                    Warning("Looks like something went wrong. Please check your form.", true);
                    return (View());
                }
            }
        }

        // make active or inactive
        [HttpGet]
        public ActionResult ChangeStatus(long? id, string type)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InvoiceField cnt = db.InvoiceFields.Find(id);
            if (cnt == null)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ChangeStatus(string type, long? id, InvoiceField InvoiceField)
        {
            bool stat = false;
            string msg;
            string types = "";
            var userid = User.Identity.GetUserId();
            InvoiceField cnt = db.InvoiceFields.Find(id);
            if (InvoiceField.Status == Status.inactive)
            {
                types = " Inactive";
                cnt.Status = Status.inactive;
            }
            else
            {
                types = " Active";
                cnt.Status = Status.active;
            }

            db.Entry(cnt).State = EntityState.Modified;
            var updates = db.SaveChanges();

            com.addlog(LogTypes.Changed, userid, "InvoiceField", "InvoiceFields", findip(), cnt.Id, "Successfully Changed the Invoice Field  to" + types);


            stat = true;
            msg = " Successfully Changed the Invoice Field  to" + types;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InvoiceField cnt = db.InvoiceFields.Find(id);
            if (cnt == null)
            {
                return NotFound();
            }
            return View(cnt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(InvoiceField vmodel, int? id)
        {
            if (ModelState.IsValid)
            {
                var Exists = db.InvoiceFields.Any(c => c.Type == vmodel.Type  && c.Id != id && c.Section == vmodel.Section);
                if (Exists)
                {
                    Warning("InvoiceField already exists.", true);
                }
                else
                {
                    InvoiceField data = db.InvoiceFields.Find(id);
                    data.Type = vmodel.Type;
                    data.Position = vmodel.Position;
                    data.Section = vmodel.Section;
                    data.Value = vmodel.Value;
                    data.Order = vmodel.Order;

                    data.Lang = vmodel.Lang;
                    data.Status = vmodel.Status;
                    db.Entry(data).State = EntityState.Modified;
                    db.SaveChanges();

                    var userid = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, userid, "InvoiceField", "InvoiceFields", findip(), data.Id, "InvoiceFields Updated Successfully");
                    
                    Success("Successfully Updated InvoiceField Details.", true);
                    return RedirectToAction("Index", "InvoiceLayout");
                }
            }
            else
            {
                Warning("Looks like something went wrong. Please check your form...", true);
                return View();
            }
            return View();
        }

        [HttpGet]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InvoiceField con = db.InvoiceFields.Find(id);
            if (con == null)
            {
                return NotFound();
            }
            return PartialView(con);
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAction(long? id)
        {
            bool stat = false;
            string msg;
            InvoiceField Con = db.InvoiceFields.Find(id);
            db.InvoiceFields.Remove(Con);
            db.SaveChanges();

            var userid = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, userid, "InvoiceField", "Invoice Field", findip(), Con.Id, "Invoice Field Deleted Successfully");
            
            stat = true;
            msg = "Successfully deleted Invoice Field details.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        #endregion 

        // GET: InvoiceLayout
        [QkAuthorize(Roles = "Dev,Invoice Customization")]
        public ActionResult Customization(byte? id)
        {
            if(id == null)
            {
                InvoiceLayout layout = db.InvoiceLayouts.Find(1);
                return View(layout);
            }
            else
            {
                InvoiceLayout layout = db.InvoiceLayouts.Find(id);
                InvoiceLayoutViewModel Vmodel = new InvoiceLayoutViewModel();
                Vmodel.Id = layout.Id;
                Vmodel.Name = layout.Name;
                Vmodel.Status = layout.Status;
                Vmodel.InvoiceField = db.InvoiceFields.ToList();
                companySet();
                string view = "~/Views/Common/Layouts/" + layout.Name + ".cshtml";
                return View(view,Vmodel);
            }
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Invoice Customization")]
        public JsonResult Customization(byte? id, ICollection<InvoiceField> vmodel)
        {
            bool stat = false;
            string msg;
            foreach (var arr in vmodel)
            {
                InvoiceField data = db.InvoiceFields.Find(arr.Id);
                data.Value = arr.Value;
                data.Order = arr.Order;
                data.Lang = arr.Lang;
                data.Status = arr.Status;
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
            }
            stat = true;
            msg = "Successfully Updated Invoice details.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        //
        public ActionResult Invoice(string type)
        {
           var section = type != "" ? type : "Sale";
            string view = "";
            var def =Convert.ToInt64( db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
            def = def == 0 ? 1 : def;
            InvoiceLayout layout = db.InvoiceLayouts.Where(a=>a.Id==def).FirstOrDefault();
            var bluestat = db.EnableSettings.Where(c => c.EnableType == "BlueDesign").SingleOrDefault();
            var plainstat = db.EnableSettings.Where(c => c.EnableType == "PlainDesign").SingleOrDefault();
            switch (section) {
                case "Hire Return":
                    view = "~/Views/HireReturn/_Invoice.cshtml";
                    break;
                case "Packing List":
                    view = "~/Views/PackingList/_Invoice.cshtml";
                    break;
                case "Purchase Order":
                    view = "~/Views/Common/TInvoice/LPO.cshtml";
                    break;
                case "taxexcempt":
                    view = "~/Views/Common/TInvoice/taxexcept.cshtml";
                    break;
                case "LPO":
                    view = "~/Views/Common/TInvoice/LPO.cshtml";
                    break;
                case "Delivery Note":
                    view = "~/Views/Common/TInvoice/DN.cshtml";
                    break;
                case "PurchaseQuotation":
                    view = "~/Views/Common/TInvoice/PQ.cshtml";
                    break;
                case "WorkCompletion":
                    view = "~/Views/Common/TInvoice/WC.cshtml";
                    break;
                case "Warranty":
                    view = "~/Views/Common/TInvoice/Warranty.cshtml";
                    break;
                case "WarranyEntry":
                    view = "~/Views/Common/TInvoice/WarranyEntry.cshtml";
                    break;
                case "MRequisition":
                    view = "~/Views/MaterialRequisition/_Invoice.cshtml";
                    break;
                case "Quote":
                   
                    if (bluestat != null && bluestat.Status == Status.active)
                    {
                        view = "~/Views/Common/TInvoice/Qbeta.cshtml";
                    }
                    else if(plainstat != null && plainstat.Status == Status.active)
                    {
                        view = "~/Views/Common/TInvoice/qdefault.cshtml";
                    }
                    else
                    {
                        view = "~/Views/Common/TInvoice/qdefault.cshtml";
                    }
                    break;
                
                case "Proforma":
                    if (bluestat != null && bluestat.Status == Status.active)
                    {
                        view = "~/Views/Common/TInvoice/ProformaBlue.cshtml";
                    }
                    else if (plainstat != null && plainstat.Status == Status.active)
                    {
                        view = "~/Views/Common/TInvoice/proforma.cshtml";
                    }
                    else
                    {
                        view = "~/Views/Common/TInvoice/proforma.cshtml";
                    }
                   
                    break;

                case "Boq":

                    if (bluestat != null && bluestat.Status == Status.active)
                    {
                        view = "~/Views/Common/TInvoice/BoqBlue.cshtml";
                    }
                    else if (plainstat != null && plainstat.Status == Status.active)
                    {
                        view = "~/Views/Common/TInvoice/Boq.cshtml";
                    }
                    else
                    {
                        view = "~/Views/Common/TInvoice/Boq.cshtml";
                    }

                    break;

                default:
                    
                    if (bluestat != null && bluestat.Status == Status.active)
                    {
                        view = "~/Views/Common/TInvoice/Defaultblue.cshtml";
                    }
                    else if (plainstat != null && plainstat.Status == Status.active)
                    {
                        view = "~/Views/Common/TInvoice/Default.cshtml";
                    }
                    else
                    {
                        view = "~/Views/Common/TInvoice/Default.cshtml";
                    }
                    break;
            }


            InvoiceLayoutViewModel Vmodel = new InvoiceLayoutViewModel();
            if (section == "Purchase Order")
            {
                section = "LPO";
            }
            if(section== "taxexcempt" || section == "WarranyEntry")
            {
                section = "Boq";
            }
            Vmodel.Id = layout.Id;
            Vmodel.Name = layout.Name;
            Vmodel.Status = layout.Status;
            Vmodel.Type = section;
            
            Vmodel.InvoiceField = db.InvoiceFields.Where(a=>a.Section== section||a.Section==null).ToList();
            companySet();
            return PartialView(view,Vmodel);
        }

        //        default:

        
        public ActionResult POSInvoice(string type)
        {
            var section = type != "" ? type : "Sale";
            var user = User.Identity.GetUserId();
            var def = Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "POSInvoice").Select(y => y.TypeValue).FirstOrDefault());
            def = def == 0 ? 1 : def;
            InvoiceLayout layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();

            string view = "~/Views/Common/POS/Default.cshtml";
            InvoiceLayoutViewModel Vmodel = new InvoiceLayoutViewModel();
            Vmodel.Id = 1;
            Vmodel.Name = "Default";
            Vmodel.Status = Status.active;
            Vmodel.Type = section;
            Vmodel.InvoiceField = db.InvoiceFields.Where(a => a.Section == section || a.Section == null).ToList();
            companySet();
            return PartialView(view, Vmodel);
        }

        [QkAuthorize(Roles = "Dev,LPO Customization")]
        public ActionResult LPOCustomization(byte? id)
        {
            if (id == null)
            {
                InvoiceLayout layout = db.InvoiceLayouts.Find(1);
                return View(layout);
            }
            else
            {
                InvoiceLayout layout = db.InvoiceLayouts.Find(id);
                InvoiceLayoutViewModel Vmodel = new InvoiceLayoutViewModel();
                Vmodel.Id = layout.Id;
                Vmodel.Name = layout.Name;
                Vmodel.Status = layout.Status;
                Vmodel.InvoiceField = db.InvoiceFields.ToList();
                companySet();
                string view = "~/Views/Common/Layouts/LPO.cshtml";
                return View(view, Vmodel);
            }
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,LPO Customization")]
        public JsonResult LPOCustomization(byte? id, ICollection<InvoiceField> vmodel)
        {
            bool stat = false;
            string msg;
            foreach (var arr in vmodel)
            {
                InvoiceField data = db.InvoiceFields.Find(arr.Id);
                data.Value = arr.Value;
                data.Order = arr.Order;
                data.Lang = arr.Lang;
                data.Status = arr.Status;
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
            }
            stat = true;
            msg = "Successfully Updated LPO Invoice details.";
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
    }
}
