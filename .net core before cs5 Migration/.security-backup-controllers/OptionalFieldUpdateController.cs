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
using System;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System.Collections.Generic;
using System.Data;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class OptionalFieldUpdateController : BaseController
    {

        ApplicationDbContext db;
        Common com;
        public OptionalFieldUpdateController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: City
        public ActionResult Index()
        {
            ViewBag.Section = QkSelect.List(new List<SelectListItem>{
                new SelectListItem() {Text = "Sales", Value="Sales"},
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
            }, "Value", "Text");
            ViewBag.Field = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = false, Text = "Select the Field", Value = "0"},
                               }, "Value", "Text", 0);
            return View();
        }

        public JsonResult GetRef1(string Name, string Field)
        {
            string search = Request.Form.GetValues("search[value]")[0];
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            //Find Order Column
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();

            var UserId = User.Identity.GetUserId();
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            var c = (from a in db.SalesEntrys
                     where a.Ref1 != null
                     select new
                     {
                         Id = a.Ref1,
                         Ref1 = a.Ref1,
                         Name = Name,
                         Ref = Field
                     });

            if (Name == "Sales")
            {
                if(Field== "Ref1")
                {
                    var temp = (from a in db.SalesEntrys
                             where a.Ref1 != null
                             select new
                             {
                                 Id = a.Ref1,
                                 Ref1 = a.Ref1,
                                 Name = Name,
                                 Ref=Field

                             });
                    c = temp;
                }
                else if(Field == "Ref2")
                {
                    var temp = (from a in db.SalesEntrys
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if(Field == "Ref3")
                {
                    var temp = (from a in db.SalesEntrys
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if(Field == "Ref4")
                {
                    var temp = (from a in db.SalesEntrys
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.SalesEntrys
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }

            }
            else if (Name == "SReturn")
            {   
                if (Field == "Ref1")
                {
                    var temp = (from a in db.SalesReturns
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.SalesReturns
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.SalesReturns
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.SalesReturns
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.SalesReturns
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
            }
            
            else if (Name == "Leads")
            {
                if (Field == "Ref1")
                {
                    var temp = (from a in db.Customers
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.Customers
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.Customers
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.Customers
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.Customers
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
            }
            else if (Name == "Purchase")
            {
                if (Field == "Ref1")
                {
                    var temp = (from a in db.PurchaseEntrys
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.PurchaseEntrys
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.PurchaseEntrys
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.PurchaseEntrys
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.PurchaseEntrys
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }

            }
            else if (Name == "PReturn")
            {
                if (Field == "Ref1")
                {
                    var temp = (from a in db.PurchaseReturns
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.PurchaseReturns
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.PurchaseReturns
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.PurchaseReturns
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.PurchaseReturns
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }

            }
            else if (Name == "Quot")
            {
                if (Field == "Ref1")
                {
                    var temp = (from a in db.Quotations
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.Quotations
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.Quotations
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.Quotations
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.Quotations
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }

            }
            else if (Name == "ProForma")
            {
                if (Field == "Ref1")
                {
                    var temp = (from a in db.ProFormas
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.ProFormas
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.ProFormas
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.ProFormas
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.ProFormas
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }

            }
            else if (Name == "DvNote")
            {
                if (Field == "Ref1")
                {
                    var temp = (from a in db.Deliverynotes
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.Deliverynotes
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.Deliverynotes
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.Deliverynotes
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.Deliverynotes
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }

            }
            else if (Name == "SOrder")
            {
                if (Field == "Ref1")
                {
                    var temp = (from a in db.SalesOrders
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.SalesOrders
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.SalesOrders
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.SalesOrders
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.SalesOrders
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
            }
            else if (Name == "PQuot")
            {
                if (Field == "Ref1")
                {
                    var temp = (from a in db.PurchaseQuotations
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.PurchaseQuotations
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.PurchaseQuotations
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.PurchaseQuotations
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.PurchaseQuotations
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
            }
            else if (Name == "Payment")
            {

                if (Field == "Ref1")
                {
                    var temp = (from a in db.Payments
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.Payments
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.Payments
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.Payments
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.Payments
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
            }
            else if (Name == "Receipt")
            {
              
                if (Field == "Ref1")
                {
                    var temp = (from a in db.Receipts
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.Receipts
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.Receipts
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.Receipts
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.Receipts
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
            }
            else if (Name == "Journal")
            {
             
                if (Field == "Ref1")
                {
                    var temp = (from a in db.Journals
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.Journals
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.Journals
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.Journals
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.Journals
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
            }
            else if (Name == "Production")
            {
                if (Field == "Ref1")
                {
                    var temp = (from a in db.Productions
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.Productions
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.Productions
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.Productions
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.Productions
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
            }
            else if (Name == "Unassemble")
            {
             
                if (Field == "Ref1")
                {
                    var temp = (from a in db.Unassembles
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.Unassembles
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.Unassembles
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.Unassembles
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.Unassembles
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
            }
            else if (Name == "CVoucher")
            {
             
                if (Field == "Ref1")
                {
                    var temp = (from a in db.ContraVouchers
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.ContraVouchers
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.ContraVouchers
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.ContraVouchers
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.ContraVouchers
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
            }
            else if (Name == "StkTrans")
            {
             
                if (Field == "Ref1")
                {
                    var temp = (from a in db.StockTransfers
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.StockTransfers
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.StockTransfers
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.StockTransfers
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.StockTransfers
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
            }
            else if (Name == "MRNote")
            {
             
                if (Field == "Ref1")
                {
                    var temp = (from a in db.MaterialReceiveNotes
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.MaterialReceiveNotes
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.MaterialReceiveNotes
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.MaterialReceiveNotes
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.MaterialReceiveNotes
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
            }
            else if (Name == "MR")
            {
              
                if (Field == "Ref1")
                {
                    var temp = (from a in db.MaterialRequisitions
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.MaterialRequisitions
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.MaterialRequisitions
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.MaterialRequisitions
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.MaterialRequisitions
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
            }
            else if (Name == "JobCard")
            {
            
                if (Field == "Ref1")
                {
                    var temp = (from a in db.JobCards
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.JobCards
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.JobCards
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.JobCards
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.JobCards
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
            }
            else if (Name == "HReturn")
            {
             
                if (Field == "Ref1")
                {
                    var temp = (from a in db.HireReturns
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.HireReturns
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.HireReturns
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.HireReturns
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.HireReturns
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
            }
            else if (Name == "Pklist")
            {
           
                if (Field == "Ref1")
                {
                    var temp = (from a in db.PackingLists
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.PackingLists
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.PackingLists
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.PackingLists
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.PackingLists
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
            }
            else if (Name == "StkJnl")
            {

                if (Field == "Ref1")
                {
                    var temp = (from a in db.StockJournals
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.StockJournals
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.StockJournals
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.StockJournals
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.StockJournals
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
            }
            else if (Name == "Project")
            {
           
             
                if (Field == "Ref1")
                {
                    var temp = (from a in db.Projects
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.Projects
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.Projects
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.Projects
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.Projects
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
            }
            else if (Name == "Task")
            {
                if (Field == "Ref1")
                {
                    var temp = (from a in db.ProTasks
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field

                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.ProTasks
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.ProTasks
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.ProTasks
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.ProTasks
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }

            }

            else
            {
                if (Field == "Ref1")
                {
                    var temp = (from a in db.Amcs
                                where a.Ref1 != null
                                select new
                                {
                                    Id = a.Ref1,
                                    Ref1 = a.Ref1,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref2")
                {
                    var temp = (from a in db.Amcs
                                where a.Ref2 != null
                                select new
                                {
                                    Id = a.Ref2,
                                    Ref1 = a.Ref2,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref3")
                {
                    var temp = (from a in db.Amcs
                                where a.Ref3 != null
                                select new
                                {
                                    Id = a.Ref3,
                                    Ref1 = a.Ref3,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else if (Field == "Ref4")
                {
                    var temp = (from a in db.Amcs
                                where a.Ref4 != null
                                select new
                                {
                                    Id = a.Ref4,
                                    Ref1 = a.Ref4,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }
                else
                {
                    var temp = (from a in db.Amcs
                                where a.Ref5 != null
                                select new
                                {
                                    Id = a.Ref5,
                                    Ref1 = a.Ref5,
                                    Name = Name,
                                    Ref = Field
                                });
                    c = temp;
                }

            }
            db.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
                                                         //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                c = c.Where(p => p.Ref1.ToString().ToLower().Contains(search.ToLower()));
                c = c.Where(p => p.Id.ToString().ToLower().Contains(search.ToLower()));
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                c = c.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = c.Distinct().Count();
            var data = c.Distinct().ToList();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }

        public JsonResult SearchRef1(string q)
        {
            List<SelectUserFormat> serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.SalesEntrys
                                  where (a.Ref1.ToLower().Contains(q.ToLower()) || a.Ref1.Contains(q)) && a.Status == 0
                                  select new SelectUserFormat
                                  {
                                      id = a.Ref1,
                                      text = a.Ref1, //each json object will have
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.SalesEntrys
                .Select(a => new SelectUserFormat
                {
                    text = a.Ref1, //each json object will have 
                        id = a.Ref1
                }).OrderBy(a => a.text).ToList();

            }//
            if (string.IsNullOrEmpty(q))
            {
                var initial = new SelectUserFormat() { id = "" };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);


        }

        public ActionResult Create()
        {

            return PartialView();

        }


        public ActionResult Edit(string ref1, string name, string Field)
        {
            OptionalFieldViewModel Vmodel = new OptionalFieldViewModel();
            if (ref1 == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (name == "Sales")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.SalesEntrys.Where(o => o.Ref1 == ref1).Select(o => o.SalesEntryId).FirstOrDefault();
                Vmodel.Ref1 = db.SalesEntrys.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.SalesEntrys.Where(o => o.Ref2 == ref1).Select(o => o.SalesEntryId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesEntrys.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.SalesEntrys.Where(o => o.Ref3 == ref1).Select(o => o.SalesEntryId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesEntrys.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.SalesEntrys.Where(o => o.Ref4 == ref1).Select(o => o.SalesEntryId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesEntrys.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.SalesEntrys.Where(o => o.Ref5 == ref1).Select(o => o.SalesEntryId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesEntrys.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }

            }
            else if (name == "SReturn")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.SalesReturns.Where(o => o.Ref1 == ref1).Select(o => o.SalesReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesReturns.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.SalesReturns.Where(o => o.Ref2 == ref1).Select(o => o.SalesReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesReturns.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.SalesReturns.Where(o => o.Ref3 == ref1).Select(o => o.SalesReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesReturns.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.SalesReturns.Where(o => o.Ref4 == ref1).Select(o => o.SalesReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesReturns.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.SalesReturns.Where(o => o.Ref5 == ref1).Select(o => o.SalesReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesReturns.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "Leads")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.Customers.Where(o => o.Ref1 == ref1).Select(o => o.CustomerID).FirstOrDefault();
                    Vmodel.Ref1 = db.Customers.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.Customers.Where(o => o.Ref2 == ref1).Select(o => o.CustomerID).FirstOrDefault();
                    Vmodel.Ref1 = db.Customers.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.Customers.Where(o => o.Ref3 == ref1).Select(o => o.CustomerID).FirstOrDefault();
                    Vmodel.Ref1 = db.Customers.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.Customers.Where(o => o.Ref4 == ref1).Select(o => o.CustomerID).FirstOrDefault();
                    Vmodel.Ref1 = db.Customers.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.Customers.Where(o => o.Ref5 == ref1).Select(o => o.CustomerID).FirstOrDefault();
                    Vmodel.Ref1 = db.Customers.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "Task")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.ProTasks.Where(o => o.Ref1 == ref1).Select(o => o.ProTaskId).FirstOrDefault();
                    Vmodel.Ref1 = db.ProTasks.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.ProTasks.Where(o => o.Ref2 == ref1).Select(o => o.ProTaskId).FirstOrDefault();
                    Vmodel.Ref1 = db.ProTasks.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.ProTasks.Where(o => o.Ref3 == ref1).Select(o => o.ProTaskId).FirstOrDefault();
                    Vmodel.Ref1 = db.ProTasks.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.ProTasks.Where(o => o.Ref4 == ref1).Select(o => o.ProTaskId).FirstOrDefault();
                    Vmodel.Ref1 = db.ProTasks.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.ProTasks.Where(o => o.Ref5 == ref1).Select(o => o.ProTaskId).FirstOrDefault();
                    Vmodel.Ref1 = db.ProTasks.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "Purchase")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.PurchaseEntrys.Where(o => o.Ref1 == ref1).Select(o => o.PurchaseEntryId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseEntrys.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.PurchaseEntrys.Where(o => o.Ref2 == ref1).Select(o => o.PurchaseEntryId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseEntrys.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.PurchaseEntrys.Where(o => o.Ref3 == ref1).Select(o => o.PurchaseEntryId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseEntrys.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.PurchaseEntrys.Where(o => o.Ref4 == ref1).Select(o => o.PurchaseEntryId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseEntrys.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.PurchaseEntrys.Where(o => o.Ref5 == ref1).Select(o => o.PurchaseEntryId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseEntrys.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "PReturn")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.PurchaseReturns.Where(o => o.Ref1 == ref1).Select(o => o.PurchaseReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseReturns.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.PurchaseReturns.Where(o => o.Ref2 == ref1).Select(o => o.PurchaseReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseReturns.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.PurchaseReturns.Where(o => o.Ref3 == ref1).Select(o => o.PurchaseReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseReturns.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.PurchaseReturns.Where(o => o.Ref4 == ref1).Select(o => o.PurchaseReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseReturns.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.PurchaseReturns.Where(o => o.Ref5 == ref1).Select(o => o.PurchaseReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseReturns.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "Quot")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.Quotations.Where(o => o.Ref1 == ref1).Select(o => o.QuotationId).FirstOrDefault();
                    Vmodel.Ref1 = db.Quotations.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.Quotations.Where(o => o.Ref2 == ref1).Select(o => o.QuotationId).FirstOrDefault();
                    Vmodel.Ref1 = db.Quotations.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.Quotations.Where(o => o.Ref3 == ref1).Select(o => o.QuotationId).FirstOrDefault();
                    Vmodel.Ref1 = db.Quotations.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.Quotations.Where(o => o.Ref4 == ref1).Select(o => o.QuotationId).FirstOrDefault();
                    Vmodel.Ref1 = db.Quotations.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.Quotations.Where(o => o.Ref5 == ref1).Select(o => o.QuotationId).FirstOrDefault();
                    Vmodel.Ref1 = db.Quotations.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "ProForma")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.ProFormas.Where(o => o.Ref1 == ref1).Select(o => o.ProFormaId).FirstOrDefault();
                    Vmodel.Ref1 = db.ProFormas.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.ProFormas.Where(o => o.Ref2 == ref1).Select(o => o.ProFormaId).FirstOrDefault();
                    Vmodel.Ref1 = db.ProFormas.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.ProFormas.Where(o => o.Ref3 == ref1).Select(o => o.ProFormaId).FirstOrDefault();
                    Vmodel.Ref1 = db.ProFormas.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.ProFormas.Where(o => o.Ref4 == ref1).Select(o => o.ProFormaId).FirstOrDefault();
                    Vmodel.Ref1 = db.ProFormas.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.ProFormas.Where(o => o.Ref5 == ref1).Select(o => o.ProFormaId).FirstOrDefault();
                    Vmodel.Ref1 = db.ProFormas.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "DvNote")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.Deliverynotes.Where(o => o.Ref1 == ref1).Select(o => o.DeliverynoteId).FirstOrDefault();
                    Vmodel.Ref1 = db.Deliverynotes.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.Deliverynotes.Where(o => o.Ref2 == ref1).Select(o => o.DeliverynoteId).FirstOrDefault();
                    Vmodel.Ref1 = db.Deliverynotes.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.Deliverynotes.Where(o => o.Ref3 == ref1).Select(o => o.DeliverynoteId).FirstOrDefault();
                    Vmodel.Ref1 = db.Deliverynotes.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.Deliverynotes.Where(o => o.Ref4 == ref1).Select(o => o.DeliverynoteId).FirstOrDefault();
                    Vmodel.Ref1 = db.Deliverynotes.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.Deliverynotes.Where(o => o.Ref5 == ref1).Select(o => o.DeliverynoteId).FirstOrDefault();
                    Vmodel.Ref1 = db.Deliverynotes.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "SOrder")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.SalesOrders.Where(o => o.Ref1 == ref1).Select(o => o.SalesOrderId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesOrders.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.SalesOrders.Where(o => o.Ref2 == ref1).Select(o => o.SalesOrderId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesOrders.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.SalesOrders.Where(o => o.Ref3 == ref1).Select(o => o.SalesOrderId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesOrders.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.SalesOrders.Where(o => o.Ref4 == ref1).Select(o => o.SalesOrderId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesOrders.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.SalesOrders.Where(o => o.Ref5 == ref1).Select(o => o.SalesOrderId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesOrders.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "PQuot")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.PurchaseQuotations.Where(o => o.Ref1 == ref1).Select(o => o.PQuotationId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseQuotations.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.PurchaseQuotations.Where(o => o.Ref2 == ref1).Select(o => o.PQuotationId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseQuotations.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.PurchaseQuotations.Where(o => o.Ref3 == ref1).Select(o => o.PQuotationId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseQuotations.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.PurchaseQuotations.Where(o => o.Ref4 == ref1).Select(o => o.PQuotationId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseQuotations.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.PurchaseQuotations.Where(o => o.Ref5 == ref1).Select(o => o.PQuotationId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseQuotations.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "Payment")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.Payments.Where(o => o.Ref1 == ref1).Select(o => o.PaymentId).FirstOrDefault();
                    Vmodel.Ref1 = db.Payments.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.Payments.Where(o => o.Ref2 == ref1).Select(o => o.PaymentId).FirstOrDefault();
                    Vmodel.Ref1 = db.Payments.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.Payments.Where(o => o.Ref3 == ref1).Select(o => o.PaymentId).FirstOrDefault();
                    Vmodel.Ref1 = db.Payments.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.Payments.Where(o => o.Ref4 == ref1).Select(o => o.PaymentId).FirstOrDefault();
                    Vmodel.Ref1 = db.Payments.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.Payments.Where(o => o.Ref5 == ref1).Select(o => o.PaymentId).FirstOrDefault();
                    Vmodel.Ref1 = db.Payments.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "Receipt")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.Receipts.Where(o => o.Ref1 == ref1).Select(o => o.ReceiptId).FirstOrDefault();
                    Vmodel.Ref1 = db.Receipts.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.Receipts.Where(o => o.Ref2 == ref1).Select(o => o.ReceiptId).FirstOrDefault();
                    Vmodel.Ref1 = db.Receipts.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.Receipts.Where(o => o.Ref3 == ref1).Select(o => o.ReceiptId).FirstOrDefault();
                    Vmodel.Ref1 = db.Receipts.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.Receipts.Where(o => o.Ref4 == ref1).Select(o => o.ReceiptId).FirstOrDefault();
                    Vmodel.Ref1 = db.Receipts.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.Receipts.Where(o => o.Ref5 == ref1).Select(o => o.ReceiptId).FirstOrDefault();
                    Vmodel.Ref1 = db.Receipts.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "Journal")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.Journals.Where(o => o.Ref1 == ref1).Select(o => o.JournalId).FirstOrDefault();
                    Vmodel.Ref1 = db.Journals.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.Journals.Where(o => o.Ref2 == ref1).Select(o => o.JournalId).FirstOrDefault();
                    Vmodel.Ref1 = db.Journals.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.Journals.Where(o => o.Ref3 == ref1).Select(o => o.JournalId).FirstOrDefault();
                    Vmodel.Ref1 = db.Journals.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.Journals.Where(o => o.Ref4 == ref1).Select(o => o.JournalId).FirstOrDefault();
                    Vmodel.Ref1 = db.Journals.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.Journals.Where(o => o.Ref5 == ref1).Select(o => o.JournalId).FirstOrDefault();
                    Vmodel.Ref1 = db.Journals.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "Production")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.Productions.Where(o => o.Ref1 == ref1).Select(o => o.ProductionId).FirstOrDefault();
                    Vmodel.Ref1 = db.Productions.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.Productions.Where(o => o.Ref2 == ref1).Select(o => o.ProductionId).FirstOrDefault();
                    Vmodel.Ref1 = db.Productions.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.Productions.Where(o => o.Ref3 == ref1).Select(o => o.ProductionId).FirstOrDefault();
                    Vmodel.Ref1 = db.Productions.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.Productions.Where(o => o.Ref4 == ref1).Select(o => o.ProductionId).FirstOrDefault();
                    Vmodel.Ref1 = db.Productions.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.Productions.Where(o => o.Ref5 == ref1).Select(o => o.ProductionId).FirstOrDefault();
                    Vmodel.Ref1 = db.Productions.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "Unassemble")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.Unassembles.Where(o => o.Ref1 == ref1).Select(o => o.UnassembleId).FirstOrDefault();
                    Vmodel.Ref1 = db.Unassembles.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.Unassembles.Where(o => o.Ref2 == ref1).Select(o => o.UnassembleId).FirstOrDefault();
                    Vmodel.Ref1 = db.Unassembles.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.Unassembles.Where(o => o.Ref3 == ref1).Select(o => o.UnassembleId).FirstOrDefault();
                    Vmodel.Ref1 = db.Unassembles.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.Unassembles.Where(o => o.Ref4 == ref1).Select(o => o.UnassembleId).FirstOrDefault();
                    Vmodel.Ref1 = db.Unassembles.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.Unassembles.Where(o => o.Ref5 == ref1).Select(o => o.UnassembleId).FirstOrDefault();
                    Vmodel.Ref1 = db.Unassembles.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "CVoucher")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.ContraVouchers.Where(o => o.Ref1 == ref1).Select(o => o.ContraVoucherId).FirstOrDefault();
                    Vmodel.Ref1 = db.ContraVouchers.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.ContraVouchers.Where(o => o.Ref2 == ref1).Select(o => o.ContraVoucherId).FirstOrDefault();
                    Vmodel.Ref1 = db.ContraVouchers.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.ContraVouchers.Where(o => o.Ref3 == ref1).Select(o => o.ContraVoucherId).FirstOrDefault();
                    Vmodel.Ref1 = db.ContraVouchers.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.ContraVouchers.Where(o => o.Ref4 == ref1).Select(o => o.ContraVoucherId).FirstOrDefault();
                    Vmodel.Ref1 = db.ContraVouchers.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.ContraVouchers.Where(o => o.Ref5 == ref1).Select(o => o.ContraVoucherId).FirstOrDefault();
                    Vmodel.Ref1 = db.ContraVouchers.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "StkTrans")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.StockTransfers.Where(o => o.Ref1 == ref1).Select(o => o.Id).FirstOrDefault();
                    Vmodel.Ref1 = db.StockTransfers.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.StockTransfers.Where(o => o.Ref2 == ref1).Select(o => o.Id).FirstOrDefault();
                    Vmodel.Ref1 = db.StockTransfers.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.StockTransfers.Where(o => o.Ref3 == ref1).Select(o => o.Id).FirstOrDefault();
                    Vmodel.Ref1 = db.StockTransfers.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.StockTransfers.Where(o => o.Ref4 == ref1).Select(o => o.Id).FirstOrDefault();
                    Vmodel.Ref1 = db.StockTransfers.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.StockTransfers.Where(o => o.Ref5 == ref1).Select(o => o.Id).FirstOrDefault();
                    Vmodel.Ref1 = db.StockTransfers.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "MRNote")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.MaterialReceiveNotes.Where(o => o.Ref1 == ref1).Select(o => o.MRId).FirstOrDefault();
                    Vmodel.Ref1 = db.MaterialReceiveNotes.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.MaterialReceiveNotes.Where(o => o.Ref2 == ref1).Select(o => o.MRId).FirstOrDefault();
                    Vmodel.Ref1 = db.MaterialReceiveNotes.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.MaterialReceiveNotes.Where(o => o.Ref3 == ref1).Select(o => o.MRId).FirstOrDefault();
                    Vmodel.Ref1 = db.MaterialReceiveNotes.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.MaterialReceiveNotes.Where(o => o.Ref4 == ref1).Select(o => o.MRId).FirstOrDefault();
                    Vmodel.Ref1 = db.MaterialReceiveNotes.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.MaterialReceiveNotes.Where(o => o.Ref5 == ref1).Select(o => o.MRId).FirstOrDefault();
                    Vmodel.Ref1 = db.MaterialReceiveNotes.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "MR")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.MaterialRequisitions.Where(o => o.Ref1 == ref1).Select(o => o.MaterialRequisitionId).FirstOrDefault();
                    Vmodel.Ref1 = db.MaterialRequisitions.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.MaterialRequisitions.Where(o => o.Ref2 == ref1).Select(o => o.MaterialRequisitionId).FirstOrDefault();
                    Vmodel.Ref1 = db.MaterialRequisitions.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.MaterialRequisitions.Where(o => o.Ref3 == ref1).Select(o => o.MaterialRequisitionId).FirstOrDefault();
                    Vmodel.Ref1 = db.MaterialRequisitions.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.MaterialRequisitions.Where(o => o.Ref4 == ref1).Select(o => o.MaterialRequisitionId).FirstOrDefault();
                    Vmodel.Ref1 = db.MaterialRequisitions.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.MaterialRequisitions.Where(o => o.Ref5 == ref1).Select(o => o.MaterialRequisitionId).FirstOrDefault();
                    Vmodel.Ref1 = db.MaterialRequisitions.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "JobCard")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.JobCards.Where(o => o.Ref1 == ref1).Select(o => o.JobCardId).FirstOrDefault();
                    Vmodel.Ref1 = db.JobCards.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.JobCards.Where(o => o.Ref2 == ref1).Select(o => o.JobCardId).FirstOrDefault();
                    Vmodel.Ref1 = db.JobCards.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.JobCards.Where(o => o.Ref3 == ref1).Select(o => o.JobCardId).FirstOrDefault();
                    Vmodel.Ref1 = db.JobCards.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.JobCards.Where(o => o.Ref4 == ref1).Select(o => o.JobCardId).FirstOrDefault();
                    Vmodel.Ref1 = db.JobCards.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.JobCards.Where(o => o.Ref5 == ref1).Select(o => o.JobCardId).FirstOrDefault();
                    Vmodel.Ref1 = db.JobCards.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "HReturn")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.HireReturns.Where(o => o.Ref1 == ref1).Select(o => o.HireReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.HireReturns.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.HireReturns.Where(o => o.Ref2 == ref1).Select(o => o.HireReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.HireReturns.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.HireReturns.Where(o => o.Ref3 == ref1).Select(o => o.HireReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.HireReturns.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.HireReturns.Where(o => o.Ref4 == ref1).Select(o => o.HireReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.HireReturns.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.HireReturns.Where(o => o.Ref5 == ref1).Select(o => o.HireReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.HireReturns.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "Pklist")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.PackingLists.Where(o => o.Ref1 == ref1).Select(o => o.PackinglistId).FirstOrDefault();
                    Vmodel.Ref1 = db.PackingLists.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.PackingLists.Where(o => o.Ref2 == ref1).Select(o => o.PackinglistId).FirstOrDefault();
                    Vmodel.Ref1 = db.PackingLists.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.PackingLists.Where(o => o.Ref3 == ref1).Select(o => o.PackinglistId).FirstOrDefault();
                    Vmodel.Ref1 = db.PackingLists.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.PackingLists.Where(o => o.Ref4 == ref1).Select(o => o.PackinglistId).FirstOrDefault();
                    Vmodel.Ref1 = db.PackingLists.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.PackingLists.Where(o => o.Ref5 == ref1).Select(o => o.PackinglistId).FirstOrDefault();
                    Vmodel.Ref1 = db.PackingLists.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "StkJnl")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.StockJournals.Where(o => o.Ref1 == ref1).Select(o => o.Id).FirstOrDefault();
                    Vmodel.Ref1 = db.StockJournals.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.StockJournals.Where(o => o.Ref2 == ref1).Select(o => o.Id).FirstOrDefault();
                    Vmodel.Ref1 = db.StockJournals.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.StockJournals.Where(o => o.Ref3 == ref1).Select(o => o.Id).FirstOrDefault();
                    Vmodel.Ref1 = db.StockJournals.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.StockJournals.Where(o => o.Ref4 == ref1).Select(o => o.Id).FirstOrDefault();
                    Vmodel.Ref1 = db.StockJournals.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.StockJournals.Where(o => o.Ref5 == ref1).Select(o => o.Id).FirstOrDefault();
                    Vmodel.Ref1 = db.StockJournals.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "Project")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.Projects.Where(o => o.Ref1 == ref1).Select(o => o.ProjectId).FirstOrDefault();
                    Vmodel.Ref1 = db.Projects.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.Projects.Where(o => o.Ref2 == ref1).Select(o => o.ProjectId).FirstOrDefault();
                    Vmodel.Ref1 = db.Projects.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.Projects.Where(o => o.Ref3 == ref1).Select(o => o.ProjectId).FirstOrDefault();
                    Vmodel.Ref1 = db.Projects.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.Projects.Where(o => o.Ref4 == ref1).Select(o => o.ProjectId).FirstOrDefault();
                    Vmodel.Ref1 = db.Projects.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.Projects.Where(o => o.Ref5 == ref1).Select(o => o.ProjectId).FirstOrDefault();
                    Vmodel.Ref1 = db.Projects.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.Amcs.Where(o => o.Ref1 == ref1).Select(o => o.AmcId).FirstOrDefault();
                    Vmodel.Ref1 = db.Amcs.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.Amcs.Where(o => o.Ref2 == ref1).Select(o => o.AmcId).FirstOrDefault();
                    Vmodel.Ref1 = db.Amcs.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.Amcs.Where(o => o.Ref3 == ref1).Select(o => o.AmcId).FirstOrDefault();
                    Vmodel.Ref1 = db.Amcs.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.Amcs.Where(o => o.Ref4 == ref1).Select(o => o.AmcId).FirstOrDefault();
                    Vmodel.Ref1 = db.Amcs.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.Amcs.Where(o => o.Ref5 == ref1).Select(o => o.AmcId).FirstOrDefault();
                    Vmodel.Ref1 = db.Amcs.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            Vmodel.type = name;
            Vmodel.field = Field;
            if (Vmodel.RefID == null)//|| Vmodel.SalesEntryId == 0
            {
                return NotFound();
            }
            return PartialView(Vmodel);

        }

        [HttpPost]
        // [QkAuthorize(Roles = "Dev,Edit StickyLabels")]
        public JsonResult Edit2(string id, string ref1,string type, string field)
        {
            bool stat = false;
            string msg;

            if (type == "Sales")
            {
                SalesEntry info = new SalesEntry();
                SalesEntry info2 = new SalesEntry();
                if (field == "Ref1")
                {
                    info = db.SalesEntrys.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.SalesEntrys.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.SalesEntrys.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.SalesEntrys.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.SalesEntrys.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.SalesEntrys.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.SalesEntrys.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.SalesEntrys.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.SalesEntrys.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.SalesEntrys.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.SalesEntrys.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.SalesEntrys.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.SalesEntrys.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.SalesEntrys.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.SalesEntrys.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;

                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "SReturn")
            {
                SalesReturn info = new SalesReturn();
                SalesReturn info2 = new SalesReturn();
                if (field == "Ref1")
                {
                    info = db.SalesReturns.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.SalesReturns.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.SalesReturns.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.SalesReturns.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.SalesReturns.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.SalesReturns.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.SalesReturns.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.SalesReturns.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.SalesReturns.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.SalesReturns.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.SalesReturns.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.SalesReturns.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.SalesReturns.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.SalesReturns.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.SalesReturns.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }


                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;

                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "Leads")
            {
                Customer info = new Customer();
                Customer info2 = new Customer();
                if (field == "Ref1")
                {
                    info = db.Customers.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.Customers.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Customers.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.Customers.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.Customers.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Customers.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.Customers.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.Customers.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Customers.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.Customers.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.Customers.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Customers.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.Customers.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.Customers.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Customers.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }


                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;

                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "Task")
            {
                ProTask info = new ProTask();
                ProTask info2 = new ProTask();
                if (field == "Ref1")
                {
                    info = db.ProTasks.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.ProTasks.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.ProTasks.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.ProTasks.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.ProTasks.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.ProTasks.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.ProTasks.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.ProTasks.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.ProTasks.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.ProTasks.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.ProTasks.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.ProTasks.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.ProTasks.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.ProTasks.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.ProTasks.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }


                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;

                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "Purchase")
            {
                //PurchaseEntrys
                PurchaseEntry info = new PurchaseEntry();
                PurchaseEntry info2 = new PurchaseEntry();
                if (field == "Ref1")
                {
                    info = db.PurchaseEntrys.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.PurchaseEntrys.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.PurchaseEntrys.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.PurchaseEntrys.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.PurchaseEntrys.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.PurchaseEntrys.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.PurchaseEntrys.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.PurchaseEntrys.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.PurchaseEntrys.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.PurchaseEntrys.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.PurchaseEntrys.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.PurchaseEntrys.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.PurchaseEntrys.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.PurchaseEntrys.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.PurchaseEntrys.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "PReturn")
            {
                //PurchaseReturns
                PurchaseReturn info = new PurchaseReturn();
                PurchaseReturn info2 = new PurchaseReturn();
                if (field == "Ref1")
                {
                    info = db.PurchaseReturns.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.PurchaseReturns.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.PurchaseReturns.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.PurchaseReturns.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.PurchaseReturns.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.PurchaseReturns.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.PurchaseReturns.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.PurchaseReturns.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.PurchaseReturns.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.PurchaseReturns.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.PurchaseReturns.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.PurchaseReturns.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.PurchaseReturns.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.PurchaseReturns.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.PurchaseReturns.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "Quot")
            {
                //Quotations
                Quotation info = new Quotation();
                Quotation info2 = new Quotation();
                if (field == "Ref1")
                {
                    info = db.Quotations.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.Quotations.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Quotations.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.Quotations.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.Quotations.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Quotations.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.Quotations.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.Quotations.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Quotations.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.Quotations.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.Quotations.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Quotations.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.Quotations.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.Quotations.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Quotations.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "ProForma")
            {
                //ProFormas
                ProForma info = new ProForma();
                ProForma info2 = new ProForma();
                if (field == "Ref1")
                {
                    info = db.ProFormas.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.ProFormas.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.ProFormas.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.ProFormas.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.ProFormas.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.ProFormas.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.ProFormas.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.ProFormas.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.ProFormas.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.ProFormas.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.ProFormas.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.ProFormas.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.ProFormas.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.ProFormas.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.ProFormas.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "DvNote")
            {
                //Deliverynotes
                Deliverynote info = new Deliverynote();
                Deliverynote info2 = new Deliverynote();
                if (field == "Ref1")
                {
                    info = db.Deliverynotes.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.Deliverynotes.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Deliverynotes.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.Deliverynotes.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.Deliverynotes.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Deliverynotes.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.Deliverynotes.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.Deliverynotes.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Deliverynotes.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.Deliverynotes.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.Deliverynotes.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Deliverynotes.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.Deliverynotes.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.Deliverynotes.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Deliverynotes.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "SOrder")
            {
                //SalesOrders
                SalesOrder info = new SalesOrder();
                SalesOrder info2 = new SalesOrder();
                if (field == "Ref1")
                {
                    info = db.SalesOrders.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.SalesOrders.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.SalesOrders.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.SalesOrders.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.SalesOrders.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.SalesOrders.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.SalesOrders.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.SalesOrders.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.SalesOrders.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.SalesOrders.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.SalesOrders.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.SalesOrders.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.SalesOrders.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.SalesOrders.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.SalesOrders.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "PQuot")
            {
                //PurchaseQuotations
                PurchaseQuotation info = new PurchaseQuotation();
                PurchaseQuotation info2 = new PurchaseQuotation();
                if (field == "Ref1")
                {
                    info = db.PurchaseQuotations.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.PurchaseQuotations.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.PurchaseQuotations.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.PurchaseQuotations.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.PurchaseQuotations.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.PurchaseQuotations.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.PurchaseQuotations.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.PurchaseQuotations.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.PurchaseQuotations.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.PurchaseQuotations.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.PurchaseQuotations.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.PurchaseQuotations.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.PurchaseQuotations.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.PurchaseQuotations.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.PurchaseQuotations.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "Payment")
            {
                //Payments
                Payment info = new Payment();
                Payment info2 = new Payment();
                if (field == "Ref1")
                {
                    info = db.Payments.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.Payments.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Payments.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.Payments.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.Payments.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Payments.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.Payments.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.Payments.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Payments.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.Payments.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.Payments.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Payments.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.Payments.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.Payments.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Payments.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "Receipt")
            {
                //Receipts
                Receipt info = new Receipt();
                Receipt info2 = new Receipt();
                if (field == "Ref1")
                {
                    info = db.Receipts.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.Receipts.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Receipts.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.Receipts.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.Receipts.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Receipts.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.Receipts.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.Receipts.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Receipts.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.Receipts.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.Receipts.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Receipts.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.Receipts.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.Receipts.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Receipts.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "Journal")
            {
                //Journals
                Journal info = new Journal();
                Journal info2 = new Journal();
                if (field == "Ref1")
                {
                    info = db.Journals.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.Journals.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Journals.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.Journals.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.Journals.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Journals.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.Journals.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.Journals.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Journals.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.Journals.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.Journals.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Journals.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.Journals.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.Journals.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Journals.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "Production")
            {
                //Productions
                Production info = new Production();
                Production info2 = new Production();
                if (field == "Ref1")
                {
                    info = db.Productions.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.Productions.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Productions.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.Productions.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.Productions.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Productions.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.Productions.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.Productions.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Productions.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.Productions.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.Productions.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Productions.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.Productions.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.Productions.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Productions.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "Unassemble")
            {
                //Unassembles
                Unassemble info = new Unassemble();
                Unassemble info2 = new Unassemble();
                if (field == "Ref1")
                {
                    info = db.Unassembles.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.Unassembles.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Unassembles.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.Unassembles.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.Unassembles.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Unassembles.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.Unassembles.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.Unassembles.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Unassembles.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.Unassembles.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.Unassembles.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Unassembles.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.Unassembles.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.Unassembles.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Unassembles.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "CVoucher")
            {
                //ContraVouchers
                ContraVoucher info = new ContraVoucher();
                ContraVoucher info2 = new ContraVoucher();
                if (field == "Ref1")
                {
                    info = db.ContraVouchers.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.ContraVouchers.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.ContraVouchers.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.ContraVouchers.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.ContraVouchers.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.ContraVouchers.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.ContraVouchers.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.ContraVouchers.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.ContraVouchers.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.ContraVouchers.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.ContraVouchers.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.ContraVouchers.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.ContraVouchers.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.ContraVouchers.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.ContraVouchers.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "StkTrans")
            {
                //StockTransfers
                StockTransfer info = new StockTransfer();
                StockTransfer info2 = new StockTransfer();
                if (field == "Ref1")
                {
                    info = db.StockTransfers.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.StockTransfers.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.StockTransfers.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.StockTransfers.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.StockTransfers.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.StockTransfers.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.StockTransfers.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.StockTransfers.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.StockTransfers.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.StockTransfers.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.StockTransfers.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.StockTransfers.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.StockTransfers.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.StockTransfers.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.StockTransfers.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "MRNote")
            {
                //MaterialReceiveNotes
                MaterialReceiveNote info = new MaterialReceiveNote();
                MaterialReceiveNote info2 = new MaterialReceiveNote();
                if (field == "Ref1")
                {
                    info = db.MaterialReceiveNotes.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.MaterialReceiveNotes.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.MaterialReceiveNotes.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.MaterialReceiveNotes.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.MaterialReceiveNotes.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.MaterialReceiveNotes.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.MaterialReceiveNotes.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.MaterialReceiveNotes.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.MaterialReceiveNotes.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.MaterialReceiveNotes.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.MaterialReceiveNotes.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.MaterialReceiveNotes.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.MaterialReceiveNotes.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.MaterialReceiveNotes.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.MaterialReceiveNotes.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "MR")
            {
                //MaterialRequisitions
                MaterialRequisition info = new MaterialRequisition();
                MaterialRequisition info2 = new MaterialRequisition();
                if (field == "Ref1")
                {
                    info = db.MaterialRequisitions.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.MaterialRequisitions.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.MaterialRequisitions.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.MaterialRequisitions.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.MaterialRequisitions.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.MaterialRequisitions.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.MaterialRequisitions.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.MaterialRequisitions.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.MaterialRequisitions.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.MaterialRequisitions.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.MaterialRequisitions.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.MaterialRequisitions.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.MaterialRequisitions.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.MaterialRequisitions.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.MaterialRequisitions.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "JobCard")
            {
                //JobCards
                JobCard info = new JobCard();
                JobCard info2 = new JobCard();
                if (field == "Ref1")
                {
                    info = db.JobCards.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.JobCards.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.JobCards.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.JobCards.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.JobCards.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.JobCards.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.JobCards.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.JobCards.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.JobCards.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.JobCards.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.JobCards.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.JobCards.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.JobCards.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.JobCards.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.JobCards.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "HReturn")
            { 
                //HireReturns
                HireReturn info = new HireReturn();
                HireReturn info2 = new HireReturn();
                if (field == "Ref1")
                {
                    info = db.HireReturns.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.HireReturns.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.HireReturns.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.HireReturns.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.HireReturns.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.HireReturns.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.HireReturns.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.HireReturns.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.HireReturns.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.HireReturns.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.HireReturns.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.HireReturns.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.HireReturns.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.HireReturns.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.HireReturns.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "Pklist")
            {
                //PackingLists
                PackingList info = new PackingList();
                PackingList info2 = new PackingList();
                if (field == "Ref1")
                {
                    info = db.PackingLists.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.PackingLists.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.PackingLists.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.PackingLists.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.PackingLists.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.PackingLists.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.PackingLists.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.PackingLists.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.PackingLists.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.PackingLists.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.PackingLists.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.PackingLists.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.PackingLists.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.PackingLists.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.PackingLists.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "StkJnl")
            {
                //StockJournals
                StockJournal info = new StockJournal();
                StockJournal info2 = new StockJournal();
                if (field == "Ref1")
                {
                    info = db.StockJournals.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.StockJournals.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.StockJournals.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.StockJournals.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.StockJournals.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.StockJournals.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.StockJournals.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.StockJournals.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.StockJournals.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.StockJournals.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.StockJournals.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.StockJournals.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.StockJournals.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.StockJournals.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.StockJournals.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else if (type == "Project")
            {
                // Projects
                Project info = new Project();
                Project info2 = new Project();
                if (field == "Ref1")
                {
                    info = db.Projects.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.Projects.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Projects.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.Projects.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.Projects.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Projects.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.Projects.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.Projects.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Projects.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.Projects.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.Projects.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Projects.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.Projects.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.Projects.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Projects.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;
                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
            else
            {
                //Amcs
                Amc info = new Amc();
                Amc info2 = new Amc();
                if (field == "Ref1")
                {
                    info = db.Amcs.Where(o => o.Ref1 == id).FirstOrDefault();
                    info2 = db.Amcs.Where(o => o.Ref1 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Amcs.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    info = db.Amcs.Where(o => o.Ref2 == id).FirstOrDefault();
                    info2 = db.Amcs.Where(o => o.Ref2 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Amcs.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    info = db.Amcs.Where(o => o.Ref3 == id).FirstOrDefault();
                    info2 = db.Amcs.Where(o => o.Ref3 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Amcs.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = ref1);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    info = db.Amcs.Where(o => o.Ref4 == id).FirstOrDefault();
                    info2 = db.Amcs.Where(o => o.Ref4 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Amcs.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = ref1);
                    db.SaveChanges();
                }
                else
                {
                    info = db.Amcs.Where(o => o.Ref5 == id).FirstOrDefault();
                    info2 = db.Amcs.Where(o => o.Ref5 == ref1).FirstOrDefault();
                    if (info2 != null)
                    {
                        msg = "Data Already Exist";
                        stat = false;
                        return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
                    }
                    db.Amcs.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = ref1);
                    db.SaveChanges();
                }
                var UserId = User.Identity.GetUserId();
                msg = "success";
                stat = true;

                return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
            }
        }

        public ActionResult Delete(string ref1, string name, string Field)
        {
            OptionalFieldViewModel Vmodel = new OptionalFieldViewModel();
            if (ref1 == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (name == "Sales")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.SalesEntrys.Where(o => o.Ref1 == ref1).Select(o => o.SalesEntryId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesEntrys.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.SalesEntrys.Where(o => o.Ref2 == ref1).Select(o => o.SalesEntryId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesEntrys.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.SalesEntrys.Where(o => o.Ref3 == ref1).Select(o => o.SalesEntryId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesEntrys.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.SalesEntrys.Where(o => o.Ref4 == ref1).Select(o => o.SalesEntryId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesEntrys.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.SalesEntrys.Where(o => o.Ref5 == ref1).Select(o => o.SalesEntryId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesEntrys.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }

            }
            else if (name == "SReturn")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.SalesReturns.Where(o => o.Ref1 == ref1).Select(o => o.SalesReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesReturns.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.SalesReturns.Where(o => o.Ref2 == ref1).Select(o => o.SalesReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesReturns.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.SalesReturns.Where(o => o.Ref3 == ref1).Select(o => o.SalesReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesReturns.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.SalesReturns.Where(o => o.Ref4 == ref1).Select(o => o.SalesReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesReturns.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.SalesReturns.Where(o => o.Ref5 == ref1).Select(o => o.SalesReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesReturns.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "Leads")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.Customers.Where(o => o.Ref1 == ref1).Select(o => o.CustomerID).FirstOrDefault();
                    Vmodel.Ref1 = db.Customers.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.Customers.Where(o => o.Ref2 == ref1).Select(o => o.CustomerID).FirstOrDefault();
                    Vmodel.Ref1 = db.Customers.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.Customers.Where(o => o.Ref3 == ref1).Select(o => o.CustomerID).FirstOrDefault();
                    Vmodel.Ref1 = db.Customers.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.Customers.Where(o => o.Ref4 == ref1).Select(o => o.CustomerID).FirstOrDefault();
                    Vmodel.Ref1 = db.Customers.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.Customers.Where(o => o.Ref5 == ref1).Select(o => o.CustomerID).FirstOrDefault();
                    Vmodel.Ref1 = db.Customers.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "Task")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.ProTasks.Where(o => o.Ref1 == ref1).Select(o => o.ProTaskId).FirstOrDefault();
                    Vmodel.Ref1 = db.ProTasks.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.ProTasks.Where(o => o.Ref2 == ref1).Select(o => o.ProTaskId).FirstOrDefault();
                    Vmodel.Ref1 = db.ProTasks.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.ProTasks.Where(o => o.Ref3 == ref1).Select(o => o.ProTaskId).FirstOrDefault();
                    Vmodel.Ref1 = db.ProTasks.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.ProTasks.Where(o => o.Ref4 == ref1).Select(o => o.ProTaskId).FirstOrDefault();
                    Vmodel.Ref1 = db.ProTasks.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.ProTasks.Where(o => o.Ref5 == ref1).Select(o => o.ProTaskId).FirstOrDefault();
                    Vmodel.Ref1 = db.ProTasks.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "Purchase")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.PurchaseEntrys.Where(o => o.Ref1 == ref1).Select(o => o.PurchaseEntryId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseEntrys.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.PurchaseEntrys.Where(o => o.Ref2 == ref1).Select(o => o.PurchaseEntryId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseEntrys.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.PurchaseEntrys.Where(o => o.Ref3 == ref1).Select(o => o.PurchaseEntryId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseEntrys.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.PurchaseEntrys.Where(o => o.Ref4 == ref1).Select(o => o.PurchaseEntryId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseEntrys.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.PurchaseEntrys.Where(o => o.Ref5 == ref1).Select(o => o.PurchaseEntryId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseEntrys.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "PReturn")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.PurchaseReturns.Where(o => o.Ref1 == ref1).Select(o => o.PurchaseReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseReturns.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.PurchaseReturns.Where(o => o.Ref2 == ref1).Select(o => o.PurchaseReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseReturns.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.PurchaseReturns.Where(o => o.Ref3 == ref1).Select(o => o.PurchaseReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseReturns.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.PurchaseReturns.Where(o => o.Ref4 == ref1).Select(o => o.PurchaseReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseReturns.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.PurchaseReturns.Where(o => o.Ref5 == ref1).Select(o => o.PurchaseReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseReturns.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "Quot")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.Quotations.Where(o => o.Ref1 == ref1).Select(o => o.QuotationId).FirstOrDefault();
                    Vmodel.Ref1 = db.Quotations.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.Quotations.Where(o => o.Ref2 == ref1).Select(o => o.QuotationId).FirstOrDefault();
                    Vmodel.Ref1 = db.Quotations.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.Quotations.Where(o => o.Ref3 == ref1).Select(o => o.QuotationId).FirstOrDefault();
                    Vmodel.Ref1 = db.Quotations.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.Quotations.Where(o => o.Ref4 == ref1).Select(o => o.QuotationId).FirstOrDefault();
                    Vmodel.Ref1 = db.Quotations.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.Quotations.Where(o => o.Ref5 == ref1).Select(o => o.QuotationId).FirstOrDefault();
                    Vmodel.Ref1 = db.Quotations.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "ProForma")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.ProFormas.Where(o => o.Ref1 == ref1).Select(o => o.ProFormaId).FirstOrDefault();
                    Vmodel.Ref1 = db.ProFormas.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.ProFormas.Where(o => o.Ref2 == ref1).Select(o => o.ProFormaId).FirstOrDefault();
                    Vmodel.Ref1 = db.ProFormas.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.ProFormas.Where(o => o.Ref3 == ref1).Select(o => o.ProFormaId).FirstOrDefault();
                    Vmodel.Ref1 = db.ProFormas.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.ProFormas.Where(o => o.Ref4 == ref1).Select(o => o.ProFormaId).FirstOrDefault();
                    Vmodel.Ref1 = db.ProFormas.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.ProFormas.Where(o => o.Ref5 == ref1).Select(o => o.ProFormaId).FirstOrDefault();
                    Vmodel.Ref1 = db.ProFormas.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "DvNote")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.Deliverynotes.Where(o => o.Ref1 == ref1).Select(o => o.DeliverynoteId).FirstOrDefault();
                    Vmodel.Ref1 = db.Deliverynotes.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.Deliverynotes.Where(o => o.Ref2 == ref1).Select(o => o.DeliverynoteId).FirstOrDefault();
                    Vmodel.Ref1 = db.Deliverynotes.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.Deliverynotes.Where(o => o.Ref3 == ref1).Select(o => o.DeliverynoteId).FirstOrDefault();
                    Vmodel.Ref1 = db.Deliverynotes.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.Deliverynotes.Where(o => o.Ref4 == ref1).Select(o => o.DeliverynoteId).FirstOrDefault();
                    Vmodel.Ref1 = db.Deliverynotes.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.Deliverynotes.Where(o => o.Ref5 == ref1).Select(o => o.DeliverynoteId).FirstOrDefault();
                    Vmodel.Ref1 = db.Deliverynotes.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "SOrder")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.SalesOrders.Where(o => o.Ref1 == ref1).Select(o => o.SalesOrderId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesOrders.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.SalesOrders.Where(o => o.Ref2 == ref1).Select(o => o.SalesOrderId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesOrders.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.SalesOrders.Where(o => o.Ref3 == ref1).Select(o => o.SalesOrderId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesOrders.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.SalesOrders.Where(o => o.Ref4 == ref1).Select(o => o.SalesOrderId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesOrders.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.SalesOrders.Where(o => o.Ref5 == ref1).Select(o => o.SalesOrderId).FirstOrDefault();
                    Vmodel.Ref1 = db.SalesOrders.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "PQuot")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.PurchaseQuotations.Where(o => o.Ref1 == ref1).Select(o => o.PQuotationId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseQuotations.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.PurchaseQuotations.Where(o => o.Ref2 == ref1).Select(o => o.PQuotationId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseQuotations.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.PurchaseQuotations.Where(o => o.Ref3 == ref1).Select(o => o.PQuotationId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseQuotations.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.PurchaseQuotations.Where(o => o.Ref4 == ref1).Select(o => o.PQuotationId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseQuotations.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.PurchaseQuotations.Where(o => o.Ref5 == ref1).Select(o => o.PQuotationId).FirstOrDefault();
                    Vmodel.Ref1 = db.PurchaseQuotations.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "Payment")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.Payments.Where(o => o.Ref1 == ref1).Select(o => o.PaymentId).FirstOrDefault();
                    Vmodel.Ref1 = db.Payments.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.Payments.Where(o => o.Ref2 == ref1).Select(o => o.PaymentId).FirstOrDefault();
                    Vmodel.Ref1 = db.Payments.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.Payments.Where(o => o.Ref3 == ref1).Select(o => o.PaymentId).FirstOrDefault();
                    Vmodel.Ref1 = db.Payments.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.Payments.Where(o => o.Ref4 == ref1).Select(o => o.PaymentId).FirstOrDefault();
                    Vmodel.Ref1 = db.Payments.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.Payments.Where(o => o.Ref5 == ref1).Select(o => o.PaymentId).FirstOrDefault();
                    Vmodel.Ref1 = db.Payments.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "Receipt")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.Receipts.Where(o => o.Ref1 == ref1).Select(o => o.ReceiptId).FirstOrDefault();
                    Vmodel.Ref1 = db.Receipts.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.Receipts.Where(o => o.Ref2 == ref1).Select(o => o.ReceiptId).FirstOrDefault();
                    Vmodel.Ref1 = db.Receipts.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.Receipts.Where(o => o.Ref3 == ref1).Select(o => o.ReceiptId).FirstOrDefault();
                    Vmodel.Ref1 = db.Receipts.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.Receipts.Where(o => o.Ref4 == ref1).Select(o => o.ReceiptId).FirstOrDefault();
                    Vmodel.Ref1 = db.Receipts.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.Receipts.Where(o => o.Ref5 == ref1).Select(o => o.ReceiptId).FirstOrDefault();
                    Vmodel.Ref1 = db.Receipts.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "Journal")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.Journals.Where(o => o.Ref1 == ref1).Select(o => o.JournalId).FirstOrDefault();
                    Vmodel.Ref1 = db.Journals.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.Journals.Where(o => o.Ref2 == ref1).Select(o => o.JournalId).FirstOrDefault();
                    Vmodel.Ref1 = db.Journals.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.Journals.Where(o => o.Ref3 == ref1).Select(o => o.JournalId).FirstOrDefault();
                    Vmodel.Ref1 = db.Journals.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.Journals.Where(o => o.Ref4 == ref1).Select(o => o.JournalId).FirstOrDefault();
                    Vmodel.Ref1 = db.Journals.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.Journals.Where(o => o.Ref5 == ref1).Select(o => o.JournalId).FirstOrDefault();
                    Vmodel.Ref1 = db.Journals.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "Production")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.Productions.Where(o => o.Ref1 == ref1).Select(o => o.ProductionId).FirstOrDefault();
                    Vmodel.Ref1 = db.Productions.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.Productions.Where(o => o.Ref2 == ref1).Select(o => o.ProductionId).FirstOrDefault();
                    Vmodel.Ref1 = db.Productions.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.Productions.Where(o => o.Ref3 == ref1).Select(o => o.ProductionId).FirstOrDefault();
                    Vmodel.Ref1 = db.Productions.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.Productions.Where(o => o.Ref4 == ref1).Select(o => o.ProductionId).FirstOrDefault();
                    Vmodel.Ref1 = db.Productions.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.Productions.Where(o => o.Ref5 == ref1).Select(o => o.ProductionId).FirstOrDefault();
                    Vmodel.Ref1 = db.Productions.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "Unassemble")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.Unassembles.Where(o => o.Ref1 == ref1).Select(o => o.UnassembleId).FirstOrDefault();
                    Vmodel.Ref1 = db.Unassembles.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.Unassembles.Where(o => o.Ref2 == ref1).Select(o => o.UnassembleId).FirstOrDefault();
                    Vmodel.Ref1 = db.Unassembles.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.Unassembles.Where(o => o.Ref3 == ref1).Select(o => o.UnassembleId).FirstOrDefault();
                    Vmodel.Ref1 = db.Unassembles.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.Unassembles.Where(o => o.Ref4 == ref1).Select(o => o.UnassembleId).FirstOrDefault();
                    Vmodel.Ref1 = db.Unassembles.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.Unassembles.Where(o => o.Ref5 == ref1).Select(o => o.UnassembleId).FirstOrDefault();
                    Vmodel.Ref1 = db.Unassembles.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "CVoucher")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.ContraVouchers.Where(o => o.Ref1 == ref1).Select(o => o.ContraVoucherId).FirstOrDefault();
                    Vmodel.Ref1 = db.ContraVouchers.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.ContraVouchers.Where(o => o.Ref2 == ref1).Select(o => o.ContraVoucherId).FirstOrDefault();
                    Vmodel.Ref1 = db.ContraVouchers.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.ContraVouchers.Where(o => o.Ref3 == ref1).Select(o => o.ContraVoucherId).FirstOrDefault();
                    Vmodel.Ref1 = db.ContraVouchers.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.ContraVouchers.Where(o => o.Ref4 == ref1).Select(o => o.ContraVoucherId).FirstOrDefault();
                    Vmodel.Ref1 = db.ContraVouchers.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.ContraVouchers.Where(o => o.Ref5 == ref1).Select(o => o.ContraVoucherId).FirstOrDefault();
                    Vmodel.Ref1 = db.ContraVouchers.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "StkTrans")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.StockTransfers.Where(o => o.Ref1 == ref1).Select(o => o.Id).FirstOrDefault();
                    Vmodel.Ref1 = db.StockTransfers.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.StockTransfers.Where(o => o.Ref2 == ref1).Select(o => o.Id).FirstOrDefault();
                    Vmodel.Ref1 = db.StockTransfers.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.StockTransfers.Where(o => o.Ref3 == ref1).Select(o => o.Id).FirstOrDefault();
                    Vmodel.Ref1 = db.StockTransfers.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.StockTransfers.Where(o => o.Ref4 == ref1).Select(o => o.Id).FirstOrDefault();
                    Vmodel.Ref1 = db.StockTransfers.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.StockTransfers.Where(o => o.Ref5 == ref1).Select(o => o.Id).FirstOrDefault();
                    Vmodel.Ref1 = db.StockTransfers.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "MRNote")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.MaterialReceiveNotes.Where(o => o.Ref1 == ref1).Select(o => o.MRId).FirstOrDefault();
                    Vmodel.Ref1 = db.MaterialReceiveNotes.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.MaterialReceiveNotes.Where(o => o.Ref2 == ref1).Select(o => o.MRId).FirstOrDefault();
                    Vmodel.Ref1 = db.MaterialReceiveNotes.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.MaterialReceiveNotes.Where(o => o.Ref3 == ref1).Select(o => o.MRId).FirstOrDefault();
                    Vmodel.Ref1 = db.MaterialReceiveNotes.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.MaterialReceiveNotes.Where(o => o.Ref4 == ref1).Select(o => o.MRId).FirstOrDefault();
                    Vmodel.Ref1 = db.MaterialReceiveNotes.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.MaterialReceiveNotes.Where(o => o.Ref5 == ref1).Select(o => o.MRId).FirstOrDefault();
                    Vmodel.Ref1 = db.MaterialReceiveNotes.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "MR")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.MaterialRequisitions.Where(o => o.Ref1 == ref1).Select(o => o.MaterialRequisitionId).FirstOrDefault();
                    Vmodel.Ref1 = db.MaterialRequisitions.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.MaterialRequisitions.Where(o => o.Ref2 == ref1).Select(o => o.MaterialRequisitionId).FirstOrDefault();
                    Vmodel.Ref1 = db.MaterialRequisitions.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.MaterialRequisitions.Where(o => o.Ref3 == ref1).Select(o => o.MaterialRequisitionId).FirstOrDefault();
                    Vmodel.Ref1 = db.MaterialRequisitions.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.MaterialRequisitions.Where(o => o.Ref4 == ref1).Select(o => o.MaterialRequisitionId).FirstOrDefault();
                    Vmodel.Ref1 = db.MaterialRequisitions.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.MaterialRequisitions.Where(o => o.Ref5 == ref1).Select(o => o.MaterialRequisitionId).FirstOrDefault();
                    Vmodel.Ref1 = db.MaterialRequisitions.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "JobCard")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.JobCards.Where(o => o.Ref1 == ref1).Select(o => o.JobCardId).FirstOrDefault();
                    Vmodel.Ref1 = db.JobCards.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.JobCards.Where(o => o.Ref2 == ref1).Select(o => o.JobCardId).FirstOrDefault();
                    Vmodel.Ref1 = db.JobCards.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.JobCards.Where(o => o.Ref3 == ref1).Select(o => o.JobCardId).FirstOrDefault();
                    Vmodel.Ref1 = db.JobCards.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.JobCards.Where(o => o.Ref4 == ref1).Select(o => o.JobCardId).FirstOrDefault();
                    Vmodel.Ref1 = db.JobCards.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.JobCards.Where(o => o.Ref5 == ref1).Select(o => o.JobCardId).FirstOrDefault();
                    Vmodel.Ref1 = db.JobCards.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "HReturn")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.HireReturns.Where(o => o.Ref1 == ref1).Select(o => o.HireReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.HireReturns.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.HireReturns.Where(o => o.Ref2 == ref1).Select(o => o.HireReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.HireReturns.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.HireReturns.Where(o => o.Ref3 == ref1).Select(o => o.HireReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.HireReturns.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.HireReturns.Where(o => o.Ref4 == ref1).Select(o => o.HireReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.HireReturns.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.HireReturns.Where(o => o.Ref5 == ref1).Select(o => o.HireReturnId).FirstOrDefault();
                    Vmodel.Ref1 = db.HireReturns.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "Pklist")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.PackingLists.Where(o => o.Ref1 == ref1).Select(o => o.PackinglistId).FirstOrDefault();
                    Vmodel.Ref1 = db.PackingLists.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.PackingLists.Where(o => o.Ref2 == ref1).Select(o => o.PackinglistId).FirstOrDefault();
                    Vmodel.Ref1 = db.PackingLists.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.PackingLists.Where(o => o.Ref3 == ref1).Select(o => o.PackinglistId).FirstOrDefault();
                    Vmodel.Ref1 = db.PackingLists.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.PackingLists.Where(o => o.Ref4 == ref1).Select(o => o.PackinglistId).FirstOrDefault();
                    Vmodel.Ref1 = db.PackingLists.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.PackingLists.Where(o => o.Ref5 == ref1).Select(o => o.PackinglistId).FirstOrDefault();
                    Vmodel.Ref1 = db.PackingLists.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "StkJnl")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.StockJournals.Where(o => o.Ref1 == ref1).Select(o => o.Id).FirstOrDefault();
                    Vmodel.Ref1 = db.StockJournals.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.StockJournals.Where(o => o.Ref2 == ref1).Select(o => o.Id).FirstOrDefault();
                    Vmodel.Ref1 = db.StockJournals.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.StockJournals.Where(o => o.Ref3 == ref1).Select(o => o.Id).FirstOrDefault();
                    Vmodel.Ref1 = db.StockJournals.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.StockJournals.Where(o => o.Ref4 == ref1).Select(o => o.Id).FirstOrDefault();
                    Vmodel.Ref1 = db.StockJournals.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.StockJournals.Where(o => o.Ref5 == ref1).Select(o => o.Id).FirstOrDefault();
                    Vmodel.Ref1 = db.StockJournals.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else if (name == "Project")
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.Projects.Where(o => o.Ref1 == ref1).Select(o => o.ProjectId).FirstOrDefault();
                    Vmodel.Ref1 = db.Projects.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.Projects.Where(o => o.Ref2 == ref1).Select(o => o.ProjectId).FirstOrDefault();
                    Vmodel.Ref1 = db.Projects.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.Projects.Where(o => o.Ref3 == ref1).Select(o => o.ProjectId).FirstOrDefault();
                    Vmodel.Ref1 = db.Projects.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.Projects.Where(o => o.Ref4 == ref1).Select(o => o.ProjectId).FirstOrDefault();
                    Vmodel.Ref1 = db.Projects.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.Projects.Where(o => o.Ref5 == ref1).Select(o => o.ProjectId).FirstOrDefault();
                    Vmodel.Ref1 = db.Projects.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            else
            {
                if (Field == "Ref1")
                {
                    Vmodel.RefID = db.Amcs.Where(o => o.Ref1 == ref1).Select(o => o.AmcId).FirstOrDefault();
                    Vmodel.Ref1 = db.Amcs.Where(o => o.Ref1 == ref1).Select(o => o.Ref1).FirstOrDefault();

                }
                else if (Field == "Ref2")
                {
                    Vmodel.RefID = db.Amcs.Where(o => o.Ref2 == ref1).Select(o => o.AmcId).FirstOrDefault();
                    Vmodel.Ref1 = db.Amcs.Where(o => o.Ref2 == ref1).Select(o => o.Ref2).FirstOrDefault();

                }
                else if (Field == "Ref3")
                {
                    Vmodel.RefID = db.Amcs.Where(o => o.Ref3 == ref1).Select(o => o.AmcId).FirstOrDefault();
                    Vmodel.Ref1 = db.Amcs.Where(o => o.Ref3 == ref1).Select(o => o.Ref3).FirstOrDefault();

                }
                else if (Field == "Ref4")
                {
                    Vmodel.RefID = db.Amcs.Where(o => o.Ref4 == ref1).Select(o => o.AmcId).FirstOrDefault();
                    Vmodel.Ref1 = db.Amcs.Where(o => o.Ref4 == ref1).Select(o => o.Ref4).FirstOrDefault();

                }
                else
                {
                    Vmodel.RefID = db.Amcs.Where(o => o.Ref5 == ref1).Select(o => o.AmcId).FirstOrDefault();
                    Vmodel.Ref1 = db.Amcs.Where(o => o.Ref5 == ref1).Select(o => o.Ref5).FirstOrDefault();

                }
            }
            Vmodel.type = name;
            Vmodel.field = Field;
            if (Vmodel.RefID == null)
            {
                return NotFound();
            }
            return PartialView(Vmodel);

        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Delete Sticky Label")]
        public JsonResult Delete(string id, string type, string field,IFormCollection collection)
        {
            bool stat = false;
            string msg;

            if (type == "Sales")
            {
                if (field == "Ref1")
                {
                    db.SalesEntrys.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.SalesEntrys.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.SalesEntrys.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.SalesEntrys.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.SalesEntrys.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if(type== "SReturn")
            {
                if (field == "Ref1")
                {
                    db.SalesReturns.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.SalesReturns.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.SalesReturns.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.SalesReturns.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.SalesReturns.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "Leads")
            {
                if (field == "Ref1")
                {
                    db.Customers.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.Customers.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.Customers.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.Customers.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.Customers.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "Task")
            {
                if (field == "Ref1")
                {
                    db.ProTasks.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.ProTasks.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.ProTasks.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.ProTasks.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.ProTasks.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "Purchase")
            {
                if (field == "Ref1")
                {
                    db.PurchaseEntrys.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.PurchaseEntrys.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.PurchaseEntrys.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.PurchaseEntrys.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.PurchaseEntrys.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "PReturn")
            {
                if (field == "Ref1")
                {
                    db.PurchaseReturns.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.PurchaseReturns.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.PurchaseReturns.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.PurchaseReturns.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.PurchaseReturns.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "Quot")
            {
                //Quotations
                if (field == "Ref1")
                {
                    db.Quotations.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.Quotations.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.Quotations.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.Quotations.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.Quotations.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "ProForma")
            {
                //ProForma
                if (field == "Ref1")
                {
                    db.ProFormas.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.ProFormas.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.ProFormas.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.ProFormas.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.ProFormas.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "DvNote")
            {
                //Deliverynotes
                if (field == "Ref1")
                {
                    db.Deliverynotes.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.Deliverynotes.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.Deliverynotes.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.Deliverynotes.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.Deliverynotes.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "SOrder")
            {
                //SalesOrders
                if (field == "Ref1")
                {
                    db.SalesOrders.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.SalesOrders.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.SalesOrders.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.SalesOrders.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.SalesOrders.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "PQuot")
            {
                //PurchaseQuotations
                if (field == "Ref1")
                {
                    db.PurchaseQuotations.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.PurchaseQuotations.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.PurchaseQuotations.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.PurchaseQuotations.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.PurchaseQuotations.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "Payment")
            {
                //Payments
                if (field == "Ref1")
                {
                    db.Payments.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.Payments.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.Payments.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.Payments.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.Payments.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "Receipt")
            {
                //Receipts
                if (field == "Ref1")
                {
                    db.Receipts.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.Receipts.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.Receipts.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.Receipts.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.Receipts.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "Journal")
            {
                //Journals
                if (field == "Ref1")
                {
                    db.Journals.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.Journals.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.Journals.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.Journals.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.Journals.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "Production")
            {
                //Productions
                if (field == "Ref1")
                {
                    db.Productions.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.Productions.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.Productions.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.Productions.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.Productions.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "Unassemble")
            {
                //Unassembles
                if (field == "Ref1")
                {
                    db.Unassembles.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.Unassembles.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.Unassembles.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.Unassembles.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.Unassembles.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "CVoucher")
            {
                //ContraVouchers
                if (field == "Ref1")
                {
                    db.ContraVouchers.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.ContraVouchers.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.ContraVouchers.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.ContraVouchers.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.ContraVouchers.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "StkTrans")
            {
                //StockTransfers
                if (field == "Ref1")
                {
                    db.StockTransfers.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.StockTransfers.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.StockTransfers.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.StockTransfers.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.StockTransfers.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "MRNote")
            {
                //MaterialReceiveNotes
                if (field == "Ref1")
                {
                    db.MaterialReceiveNotes.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.MaterialReceiveNotes.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.MaterialReceiveNotes.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.MaterialReceiveNotes.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.MaterialReceiveNotes.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "MR")
            {
                //MaterialRequisitions
                if (field == "Ref1")
                {
                    db.MaterialRequisitions.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.MaterialRequisitions.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.MaterialRequisitions.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.MaterialRequisitions.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.MaterialRequisitions.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "JobCard")
            {
                //JobCards
                if (field == "Ref1")
                {
                    db.JobCards.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.JobCards.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.JobCards.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.JobCards.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.JobCards.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "HReturn")
            {
                //HireReturns
                if (field == "Ref1")
                {
                    db.HireReturns.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.HireReturns.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.HireReturns.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.HireReturns.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.HireReturns.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "Pklist")
            {
                //PackingLists
                if (field == "Ref1")
                {
                    db.PackingLists.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.PackingLists.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.PackingLists.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.PackingLists.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.PackingLists.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "StkJnl")
            {
                //StockJournals
                if (field == "Ref1")
                {
                    db.StockJournals.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.StockJournals.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.StockJournals.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.StockJournals.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.StockJournals.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else if (type == "Project")
            {
                //Projects
                if (field == "Ref1")
                {
                    db.Projects.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.Projects.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.Projects.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.Projects.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.Projects.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }
            else 
            {
                //Amcs
                if (field == "Ref1")
                {
                    db.Amcs.Where(o => o.Ref1 == id).ToList().ForEach(o => o.Ref1 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref2")
                {
                    db.Amcs.Where(o => o.Ref2 == id).ToList().ForEach(o => o.Ref2 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref3")
                {
                    db.Amcs.Where(o => o.Ref3 == id).ToList().ForEach(o => o.Ref3 = null);
                    db.SaveChanges();
                }
                else if (field == "Ref4")
                {
                    db.Amcs.Where(o => o.Ref4 == id).ToList().ForEach(o => o.Ref4 = null);
                    db.SaveChanges();
                }
                else
                {
                    db.Amcs.Where(o => o.Ref5 == id).ToList().ForEach(o => o.Ref5 = null);
                    db.SaveChanges();
                }
            }


            var UserId = User.Identity.GetUserId();
               
                stat = true;
                msg = "Successfully Deleted City.";
            
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        public JsonResult SearchOptionField(string q, string x, string VTid)
        {
            List<SelectFormat2> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.FieldMappings
                                  where (VTid != "" && a.Section == VTid) &&
                                   a.Status == Status.active &&
                                        (q == null || a.FieldName.ToLower().Contains(q.ToLower()) || a.FieldName.Contains(q))
                                  select new SelectFormat2
                                  {
                                      text = a.FieldName,
                                      id = a.Field
                                  }).OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.FieldMappings
                                  where (a.Section == VTid)&&
                                  a.Status == Status.active
                                  select new SelectFormat2
                                  {
                                      text = a.FieldName,
                                      id = a.Field
                                  }).OrderBy(b => b.text).ToList();

            }

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
                var initial = new SelectFormat2() { id = "", text = stt };
                serialisedJson.Insert(0, initial);
            }

            return Json(serialisedJson);
        }

        //[HttpGet]
        ////[QkAuthorize(Roles = "Dev,BillSundry Status")]
        //// POST: master/ChangeStatus/
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        ////[QkAuthorize(Roles = "Dev,BillSundry Status")]

        //    return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

    }


}
