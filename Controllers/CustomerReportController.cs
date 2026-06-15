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
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using Microsoft.AspNetCore.Mvc;
using CustomHtml;
using Microsoft.AspNetCore.Identity;
using System.Net;
using Microsoft.Data.SqlClient;
using System.Runtime;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class CustomerReportController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public CustomerReportController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }

        // GET: CustomerReport


        //Customer region

        [QkAuthorize(Roles = "Dev,Customer")]
        public ActionResult CustomerWises()
        {
            var OpAll = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                 }, "Value", "Text", 1);



            ViewBag.Cust = OpAll;
            ViewBag.Customer = OpAll;
            ViewBag.Type = OpAll;
            ViewBag.MC = OpAll;
            ViewBag.getProj = OpAll;
            ViewBag.getProTask = OpAll;
            ViewBag.Empl = OpAll;
            ViewBag.CustomerType = OpAll;
            ViewBag.Mobile = OpAll;
            ViewBag.TxType = QkSelect.List(new List<SelectListItem>
                {
                    new SelectListItem { Text = "Item Wise", Value = "0"},
                    new SelectListItem { Text = "Exempt", Value = "1"},
                }, "Value", "Text");

            var code = db.Customers.Select(s => new
            {
                ID = s.CustomerCode,
                Name = s.CustomerCode
            }).ToList();
            ViewBag.CustomerCode = QkSelect.List(code, "ID", "Name");

            var name = db.Customers.Select(s => new
            {
                ID = s.CustomerID,
                Name = s.CustomerName
            }).ToList();
            ViewBag.CustomerName = QkSelect.List(name, "ID", "Name");
            ViewBag.MC = QkSelect.List(
          new List<SelectListItem>
          {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
          }, "Value", "Text", 0);
            var cat = db.PriceCategoryMasters
                .Select(s => new
                {
                    ID = s.CategoryId,
                    Name = s.Category
                }).Distinct()
                .ToList().OrderBy(a => a.Name);
            ViewBag.Categorys = QkSelect.List(cat, "ID", "Name");

            var employee = db.Employees.Select(s => new
            {
                ID = s.EmployeeId,
                Name = s.FirstName + " " + s.LastName
            }).ToList();
            ViewBag.Employee = QkSelect.List(name, "ID", "Name");
            return View();
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Customer")]
        public ActionResult GetCustomer(long? Customer, string LastUpdDays,long mc)
        {

            int days = 0;
            if (LastUpdDays != "")
            {
                days = Convert.ToInt32(LastUpdDays);


            }
            DateTime datecheck = DateTime.Now.AddDays(-days);

            DateTime today = DateTime.Now;

            //              group element by element.SalesEntryId
            //           into groups






            DateTime datenow = DateTime.Now;
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


            var userpermission = User.IsInRole("All Customers");
            var UserId = User.Identity.GetUserId();

            var uDev = User.IsInRole("Dev");
            var uCustView = User.IsInRole("View Customer");
            var uEdit = User.IsInRole("Edit Customer");
            var uDelete = User.IsInRole("Delete Customer");
            //           where (bc.Customer == null)

            //               id = ab.CustomerID



            var v = (from a in db.Customers
                     join x in db.Accountss on a.Accounts equals x.AccountsID
                     join g in db.SalesEntrys on a.CustomerID equals g.Customer

                     //let h = db.SalesOrders.Where(sa => sa.Customer == a.CustomerID).FirstOrDefault()
                     //let i = db.SalesReturns.Where(sl => sl.Customer == a.CustomerID).FirstOrDefault()
                     //let j = db.Quotations.Where(qu => qu.Customer == a.CustomerID).OrderByDescending(o=>o.QuotCreatedDate).FirstOrDefault()
                     //let k = db.Deliverynotes.Where(de => de.Customer == a.CustomerID).FirstOrDefault()
                     //let m = db.ProFormas.Where(pr => pr.Customer == a.CustomerID).FirstOrDefault()
                     //let n = db.SalesOrders.Where(sa => sa.Customer == a.CustomerID).FirstOrDefault()
                     //let o = db.JobCards.Where(jo => jo.Customer == a.CustomerID).FirstOrDefault()
                     //let p = db.Projects.Where(pr => pr.Customer == a.CustomerID).FirstOrDefault()
                     //let q = db.ProTasks.Where(ot => ot.CustomerID == a.CustomerID).FirstOrDefault()
                     //let r = db.HireReturns.Where(hi => hi.Customer == a.CustomerID).FirstOrDefault()
                     //let mob = (
                     //where (rrr.RelationID == a.CustomerID && rrr.RelationType == 0)
                     //    Num = "+" + con.CountryCode + co.Mobile,
                     //    Name = co.FirstName + "  " + co.LastName,
                     //    emails = co.EmailId,
                     //}).ToList()

                     where a.Type == CRMCustomerType.Customer &&
                           (Customer == 0 || a.CustomerType == Customer) &&







                 (EF.Functions.DateDiffDay(g.SEDate, datecheck) <= 0) &&
                         (EF.Functions.DateDiffDay(g.SEDate, today) >= 0) && 
                         (mc==0||g.MaterialCenter==mc)

                     //)


                     select new
                     {
                         a.CustomerID,


                     }).Select(o => (long)o.CustomerID).ToList().ToArray();
            var v2src = (from a in db.Customers
                      join x in db.Accountss on a.Accounts equals x.AccountsID
                      join cr in db.ContactRelation on a.CustomerID equals cr.RelationID into cnt
                      from cr in cnt.DefaultIfEmpty()
                      join b in db.Contacts on cr.ContactID equals b.ContactID into tmp
                      from b in tmp.DefaultIfEmpty()
                      where a.Type == CRMCustomerType.Customer &&
                      (Customer == 0 || a.CustomerType == Customer) &&
                      !v.Contains(a.CustomerID)
                      select new
                      {

                          id = a.CustomerID,
                          a.CustomerCode,
                          a.CustomerName,
                          TaxRegNo = x.TRN,
                          a.Location,
                          Addr = b.Address,
                          City = b.City,
                          State = b.State,
                          Country = b.Country,
                          Zip = b.Zip,
                          Phone = b.Phone,
                          //Mobile = b.Mobile,
                          Email = b.EmailId,
                          CreditLimit = a.CreditLimit,
                          CreditPeriod = a.CreditPeriod,
                          OpnBalanceCr = x.OpnBalanceCr,
                          OpnBal = x.OpnBalance,
                          Credit = (db.AccountsTransactions.Where(d => d.Account == a.Accounts && d.Status == null).Sum(d => (decimal?)d.Credit) ?? 0),
                          Debit = (db.AccountsTransactions.Where(b => b.Account == a.Accounts && b.Status == null).Sum(b => (decimal?)b.Debit) ?? 0),
                          Dev = uDev,
                          Details = uCustView,
                          Edit = uEdit,
                          Delete = uDelete,
                          Alias = x.Alias,

                      }).ToList();

            var crCustIds = v2src.Select(o => o.id).ToList();
            var crMobLookup = (from co in db.Contacts
                               join rrr in db.ContactRelation on co.ContactID equals rrr.ContactID
                               join con in db.Country on co.CountryID equals con.CountryID into conn
                               from con in conn.DefaultIfEmpty()
                               where rrr.RelationType == 0 && crCustIds.Contains(rrr.RelationID)
                               select new { rrr.RelationID, CountryCode = con.CountryCode, co.Mobile, co.FirstName, co.LastName, co.EmailId })
                              .ToList()
                              .ToLookup(x => x.RelationID);
            var v2 = v2src.Select(o => new
            {
                o.id,
                o.CustomerCode,
                o.CustomerName,
                o.TaxRegNo,
                o.Location,
                Address = o.Addr != null ? o.Addr : "" +
               "<br/>" + o.City != null ? o.City : "" +
               " " + o.State != null ? o.State : "" +
               " " + o.Country != null ? o.Country : "" +
               "<br/>" + o.Zip != null ? o.Zip : "",
                o.Phone,
                o.Email,
                o.CreditLimit,
                o.CreditPeriod,
                OpnBalance = (o.OpnBalanceCr > 0) ? (o.OpnBalanceCr != 0 ? o.OpnBalanceCr + " Cr." : "0.00") : (o.OpnBal != 0 ? o.OpnBal + " Dr." : "0.00"),
                o.Credit,
                o.Debit,
                o.Dev,
                o.Details,
                o.Edit,
                o.Delete,
                o.Alias,
                mobmodel = crMobLookup[o.id].Select(m => new
                {
                    Num = "+" + m.CountryCode + m.Mobile,
                    Name = m.FirstName + "  " + m.LastName,
                    emails = m.EmailId,
                }).ToList(),
                currentbalance = (o.Debit > o.Credit) ? ((o.Debit - o.Credit) + " Dr.") : ((o.Credit - o.Debit) + " Cr."),
            }).OrderByDescending(a => a.id).ToList();


            var data = v2.ToList();
            recordsTotal = v2.Count();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
            string result = javaScriptSerializer.Serialize(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            var results = new ContentResult
            {
                Content = result,
                ContentType = "application/json"
            };
            return results;

        }



        public void companySet()
        {
            var cdetails = db.companys
                            .Select(s => new
                            {
                                CName = s.CPName,
                                CAddress = s.CPAddress,
                                CEmail = s.CPEmail,
                                CTaxRegNo = s.TRN,
                                CPhone = s.CPPhone,
                                s.CPMobile,
                                s.CPFax,
                                CLogo = s.CPLogo,
                            }).FirstOrDefault();
            ViewBag.CName = cdetails.CName;
            ViewBag.CAddress = cdetails.CAddress;
            ViewBag.CEmail = cdetails.CEmail;
            ViewBag.CTaxRegNo = cdetails.CTaxRegNo;
            ViewBag.CPhone = cdetails.CPhone;
            ViewBag.CPMob = cdetails.CPMobile;
            ViewBag.CPFax = cdetails.CPFax;
            ViewBag.CLogo = cdetails.CLogo;

            var comHead = db.CompanyHeaders.FirstOrDefault();
            ViewBag.Header = comHead.Header;
            ViewBag.Footer = comHead.Footer;
        }

        public ActionResult AddCustomerRemark(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Customer cus = db.Customers.Find(id);

            if (cus == null)
            {
                return NotFound();
            }
            var CusRemark = new RemarkCustomer
            {
                CustomerId = cus.CustomerID
            };

            return PartialView(CusRemark);
        }

        //Saving of Remarks
        [HttpPost]
        public ActionResult AddCustomerRemark(RemarkCustomer CusRemark, int? id)
        {
            Int64 CustomerId = CusRemark.CustomerId;

            if (ModelState.IsValid)
            {
                if (CusRemark.Remark != null)
                {
                    Common com = new Common();
                    var UserId = User.Identity.GetUserId();
                    var Today = Convert.ToDateTime(System.DateTime.Now);

                    RemarkCustomer Obj = new RemarkCustomer
                    {
                        CustomerId = CusRemark.CustomerId,
                        Remark = CusRemark.Remark,
                        AddedUser = UserId,
                        CreatedDate = Today,
                    };
                    db.RemarkCustomers.Add(Obj);
                    db.SaveChanges();

                    com.addlog(LogTypes.Created, UserId, "CustomerReport", "RemarkCustomers", findip(), CustomerId, "Remarks Added Successfully..");
                    Success("Remark added successfully...", true);
                }
            }
            else
            {
                Danger("Failed to add Remarks...", true);
            }
            return RedirectToAction("CustomerWises", "CustomerReport");
        }


     



        //Function to list the Remarks from table CustomerRemarks
        [HttpPost]
        public ActionResult GetAllRemarks(long? CustomerId)
        {
            var draw = Request.Form.GetValues("draw").FirstOrDefault();

            int recordsTotal = 0;
            var UserId = User.Identity.GetUserId();

            DateTime rmdate = System.DateTime.Now.AddDays(-30);

            var v = (from a in db.RemarkCustomers
                     join b in db.Users on a.AddedUser equals b.Id into emp
                     from b in emp.DefaultIfEmpty()
                     where a.CustomerId == CustomerId && a.Remark != null && a.CreatedDate >= rmdate
                     orderby a.CreatedDate descending
                    
                     select new
                     {
                         id = a.RemarkId,
                         a.CreatedDate,
                         EmpName = b.UserName,
                         a.Remark,
                     });

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            recordsTotal = v.Count();
            var data = v.ToList();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }




    }



}
