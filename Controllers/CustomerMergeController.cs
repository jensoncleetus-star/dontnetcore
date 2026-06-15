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
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
    public class CustomerMergeController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public CustomerMergeController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Team
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Customer Merge List")]
        public ActionResult Index()
        {
            return View();
        }

        [RedirectingAction]
        [Authorize(Roles = "Dev,Customer Merge List")]
        [HttpPost]
        public ActionResult getmergecustomer()
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

            var UserView = (from a in db.customermerges
                            join b in db.Customers on a.oldcustomerid equals b.CustomerID 
                            join c in db.Customers on a.newcustomerid equals c.CustomerID
                            join d in db.Users on a.createduser equals d.Id
                            join e in db.Employees on d.Id equals e.UserId into emp
                            from e in emp.DefaultIfEmpty()
                            select new
                            {
                                a.custmergeid,
                                oldcustomer=b.CustomerName,
                                newcustomer=c.CustomerName,
                                employee=(e!=null)?(e.FirstName+" "+e.MiddleName + " "+e.LastName):d.UserName,
                                });
              //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                UserView = UserView.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }
            recordsTotal = UserView.Count();
            var data = UserView.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Create Customer Merge")]
        public ActionResult Create(long? id, long? type)
        {
            var OpAll = QkSelect.List(
                                  new List<SelectListItem>
                                  {
                                    new SelectListItem { Selected = true, Text = "--Select--", Value = "0"},
                                  }, "Value", "Text", 1);

            ViewBag.cust = QkSelect.List(OpAll, "Value", "Text");
            return PartialView();
        }
        [RedirectingAction]
        [QkAuthorize(Roles = "Dev,Create Customer Merge")]
        public ActionResult FindDuplicate(long? id, long? type)
        {
            var OpAll = QkSelect.List(
                                  new List<SelectListItem>
                                  {
                                    new SelectListItem { Selected = true, Text = "--Select--", Value = "0"},
                                  }, "Value", "Text", 1);
            //                  where a.RelationType == 0 && b.Mobile != ""
            //                  group b.Mobile by b.Mobile into grp
            //                      mobile = grp.Key,
            //                      cn = grp.Count()


            var customers = (from x in db.Customers
                             let dupnumbers = (from a in db.ContactRelation
                                               join b in db.Contacts on a.ContactID equals b.ContactID
                                               where a.RelationType == 0 && b.Mobile != ""
                                               && a.RelationID == x.CustomerID
                                               group b.Mobile by b.Mobile into grp
                                               select new
                                               {
                                                   mobile = grp.Key,
                                                   cn = grp.Count()


                                               }
                              ).FirstOrDefault()
                             where (dupnumbers != null) &&
                             (dupnumbers == null || dupnumbers.cn > 1)
                             select new
                             {
                                 customers = (from a in db.ContactRelation
                                              join b in db.Contacts on a.ContactID equals b.ContactID
                                              join c in db.Customers on a.RelationID equals c.CustomerID
                                              where a.RelationType == 0 && b.Mobile != ""
                                              && b.Mobile == dupnumbers.mobile

                                              select new
                                              {
                                                  c.CustomerID,
                                                  c.CustomerName


                                              }).ToList(),
                                 mobile = dupnumbers.cn
                             }
                           ).ToList();

            ViewBag.cust = QkSelect.List(OpAll, "Value", "Text");
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create Customer Merge")]
        [ValidateAntiForgeryToken]
        public JsonResult Create(CustomerMergeViewModel vmodel)
        {
            bool stat = false;
            string msg="";
            var userid = User.Identity.GetUserId();
            DateTime crdate = System.DateTime.Now;
            
            foreach(long oldcus in vmodel.OldCurstomerIds)
            {
                customermerge mer = new customermerge();
                mer.oldcustomerid = oldcus;
                mer.newcustomerid = vmodel.CustomerId;
                mer.createddate = crdate;
                mer.createduser = userid;
                db.customermerges.Add(mer);
                db.SaveChanges();
               long oldcustomeraccountid = db.Customers.Find(oldcus).Accounts;
                long newcustomeraccountid = db.Customers.Find(vmodel.CustomerId).Accounts;
                long oldcustomeraccountidid = db.Customers.Find(oldcus).CustomerID;
                long newcustomeraccountidid= db.Customers.Find(vmodel.CustomerId).CustomerID;
                var cusupdate = db.Customers.Find(oldcus);
                cusupdate.CustomerName = "OLD-" + cusupdate.CustomerName;
                db.Entry(cusupdate).State = EntityState.Modified;
                db.SaveChanges();
                var acupdate = db.Accountss.Find(cusupdate.Accounts);
                acupdate.Name = "OLD-" + acupdate.Name;
                db.Entry(acupdate).State = EntityState.Modified;
                db.SaveChanges();
                transferaccounttransactions(oldcustomeraccountid, newcustomeraccountid,mer.custmergeid);
                transferrecieptpaymentjournal(oldcustomeraccountid, newcustomeraccountid, mer.custmergeid);
                transfersales(oldcustomeraccountidid, newcustomeraccountidid, mer.custmergeid);
                transfersalesreturn(oldcustomeraccountidid, newcustomeraccountidid, mer.custmergeid);
                trsferboq(oldcustomeraccountidid, newcustomeraccountidid, mer.custmergeid);
                performa(oldcustomeraccountidid, newcustomeraccountidid, mer.custmergeid);
                task(oldcustomeraccountidid, newcustomeraccountidid, mer.custmergeid);
                transferamc(oldcustomeraccountidid, newcustomeraccountidid, mer.custmergeid);
                transfercontacts(oldcustomeraccountidid, newcustomeraccountidid, mer.custmergeid); 
                transferdocuments(oldcustomeraccountidid, newcustomeraccountidid, mer.custmergeid);
                transferquotation(oldcustomeraccountidid, newcustomeraccountidid, mer.custmergeid);
                transferprojects(oldcustomeraccountidid, newcustomeraccountidid, mer.custmergeid);
                transfersalesorder(oldcustomeraccountidid, newcustomeraccountidid, mer.custmergeid);
tranferdeliverynote(oldcustomeraccountidid, newcustomeraccountidid, mer.custmergeid);
            }
            stat = true;
            msg = "Transactions Transfered";

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public void transferaccounttransactions(long oldcustomeraccountid, long newcustomeraccountid,long custmergeid)
        {
            var accounttrans = db.AccountsTransactions.Where(o => o.Account == oldcustomeraccountid).ToList();
            foreach(var ac in accounttrans)
            {
                ac.Account = newcustomeraccountid;
                db.Entry(ac).State = EntityState.Modified;
                db.SaveChanges();
                customermergelog cusmerlog = new customermergelog
                {
                    customermergeid =custmergeid ,
                     entryid =ac.Id,
                     purpose =CustomerMergePurposeId.accountstransaction,
                };
                db.customermergelogs.Add(cusmerlog);
                db.SaveChanges();


            }
        }
        public void transfersales(long oldcustomeraccountid, long newcustomeraccountid, long custmergeid)
        {
            var accounttrans = db.SalesEntrys.Where(o => o.Customer == oldcustomeraccountid).ToList();
            foreach (var ac in accounttrans)
            {
                ac.Customer = newcustomeraccountid;
                db.Entry(ac).State = EntityState.Modified;
                db.SaveChanges();
                customermergelog cusmerlog = new customermergelog
                {
                    customermergeid = custmergeid,
                    entryid = ac.SalesEntryId,
                    purpose = CustomerMergePurposeId.sales,
                };
                db.customermergelogs.Add(cusmerlog);
                db.SaveChanges();


            }
        }
        public void transfersalesreturn(long oldcustomeraccountid, long newcustomeraccountid, long custmergeid)
        {
            var accounttrans = db.SalesReturns.Where(o => o.Customer == oldcustomeraccountid).ToList();
            foreach (var ac in accounttrans)
            {
                ac.Customer = newcustomeraccountid;
                db.Entry(ac).State = EntityState.Modified;
                db.SaveChanges();
                customermergelog cusmerlog = new customermergelog
                {
                    customermergeid = custmergeid,
                    entryid = ac.SalesReturnId,
                    purpose = CustomerMergePurposeId.quotation,
                };
                db.customermergelogs.Add(cusmerlog);
                db.SaveChanges();


            }
        }
        public void transfersalesorder(long oldcustomeraccountid, long newcustomeraccountid, long custmergeid)
        {
            var accounttrans = db.SalesOrders.Where(o => o.Customer == oldcustomeraccountid).ToList();
            foreach (var ac in accounttrans)
            {
                ac.Customer = newcustomeraccountid;
                db.Entry(ac).State = EntityState.Modified;
                db.SaveChanges();
                customermergelog cusmerlog = new customermergelog
                {
                    customermergeid = custmergeid,
                    entryid = ac.SalesOrderId,
                    purpose = CustomerMergePurposeId.salesorder,
                };
                db.customermergelogs.Add(cusmerlog);
                db.SaveChanges();


            }
        }

        public void trsferboq(long oldcustomeraccountid, long newcustomeraccountid, long custmergeid)
        {
            var accounttrans = db.BillOfQyts.Where(o => o.Customer == oldcustomeraccountid).ToList();
            foreach (var ac in accounttrans)
            {
                ac.Customer = newcustomeraccountid;
                db.Entry(ac).State = EntityState.Modified;
                db.SaveChanges();
                customermergelog cusmerlog = new customermergelog
                {
                    customermergeid = custmergeid,
                    entryid = ac.BoqId,
                    purpose = CustomerMergePurposeId.boq,
                };
                db.customermergelogs.Add(cusmerlog);
                db.SaveChanges();


            }
        }
        public void performa(long oldcustomeraccountid, long newcustomeraccountid, long custmergeid)
        {
            var accounttrans = db.ProFormas.Where(o => o.Customer == oldcustomeraccountid).ToList();
            foreach (var ac in accounttrans)
            {
                ac.Customer = newcustomeraccountid;
                db.Entry(ac).State = EntityState.Modified;
                db.SaveChanges();
                customermergelog cusmerlog = new customermergelog
                {
                    customermergeid = custmergeid,
                    entryid = ac.ProFormaId,
                    purpose = CustomerMergePurposeId.performa,
                };
                db.customermergelogs.Add(cusmerlog);
                db.SaveChanges();


            }
        }
        public void task(long oldcustomeraccountid, long newcustomeraccountid, long custmergeid)
        {
            var accounttrans = db.ProTasks.Where(o => o.CustomerID == oldcustomeraccountid).ToList();
            foreach (var ac in accounttrans)
            {
                ac.CustomerID = newcustomeraccountid;
                db.Entry(ac).State = EntityState.Modified;
                db.SaveChanges();
                customermergelog cusmerlog = new customermergelog
                {
                    customermergeid = custmergeid,
                    entryid = ac.ProTaskId,
                    purpose = CustomerMergePurposeId.task,
                };
                db.customermergelogs.Add(cusmerlog);
                db.SaveChanges();


            }
        }

        public void transferamc(long oldcustomeraccountid, long newcustomeraccountid, long custmergeid)
        {
            var accounttrans = db.Amcs.Where(o => o.CustomerId == oldcustomeraccountid).ToList();
            foreach (var ac in accounttrans)
            {
                ac.CustomerId = newcustomeraccountid;
                db.Entry(ac).State = EntityState.Modified;
                db.SaveChanges();
                customermergelog cusmerlog = new customermergelog
                {
                    customermergeid = custmergeid,
                    entryid = ac.AmcId,
                    purpose = CustomerMergePurposeId.amc,
                };
                db.customermergelogs.Add(cusmerlog);
                db.SaveChanges();


            }
        }
        public void transfercontacts(long oldcustomeraccountid, long newcustomeraccountid, long custmergeid)
        {
            var accounttrans = db.ContactRelation.Where(o => o.RelationID == oldcustomeraccountid&&o.RelationType==(int)ContctRelation.Customer).ToList();
            foreach (var ac in accounttrans)
            {
                ac.RelationID = newcustomeraccountid;
                db.Entry(ac).State = EntityState.Modified;
                db.SaveChanges();
                customermergelog cusmerlog = new customermergelog
                {
                    customermergeid = custmergeid,
                    entryid = ac.ContactRelationID,
                    purpose = CustomerMergePurposeId.conactrelation,
                };
                db.customermergelogs.Add(cusmerlog);
                db.SaveChanges();


            }
        }
        public void transferdocuments(long oldcustomeraccountid, long newcustomeraccountid, long custmergeid)
        {
            var accounttrans = db.CustomerDocuments.Where(o => o.CutomerID == oldcustomeraccountid&& o.DoucumentType == "customer").ToList();
            foreach (var ac in accounttrans)
            {
                ac.CutomerID = newcustomeraccountid;
                db.Entry(ac).State = EntityState.Modified;
                db.SaveChanges();
                customermergelog cusmerlog = new customermergelog
                {
                    customermergeid = custmergeid,
                    entryid = ac.DocumnetId,
                    purpose = CustomerMergePurposeId.documents,
                };
                db.customermergelogs.Add(cusmerlog);
                db.SaveChanges();


            }
        }
        public void tranferdeliverynote(long oldcustomeraccountid, long newcustomeraccountid, long custmergeid)
        {
            var accounttrans = db.Deliverynotes.Where(o => o.Customer == oldcustomeraccountid).ToList();
            foreach (var ac in accounttrans)
            {
                ac.Customer = newcustomeraccountid;
                db.Entry(ac).State = EntityState.Modified;
                db.SaveChanges();
                customermergelog cusmerlog = new customermergelog
                {
                    customermergeid = custmergeid,
                    entryid = ac.DeliverynoteId,
                    purpose = CustomerMergePurposeId.deliverynote,
                };
                db.customermergelogs.Add(cusmerlog);
                db.SaveChanges();


            }
        }

        public void transferquotation(long oldcustomeraccountid, long newcustomeraccountid, long custmergeid)
        {
            var accounttrans = db.Quotations.Where(o => o.Customer == oldcustomeraccountid).ToList();
            foreach (var ac in accounttrans)
            {
                ac.Customer = newcustomeraccountid;
                db.Entry(ac).State = EntityState.Modified;
                db.SaveChanges();
                customermergelog cusmerlog = new customermergelog
                {
                    customermergeid = custmergeid,
                    entryid = ac.QuotationId,
                    purpose = CustomerMergePurposeId.quotation,
                };
                db.customermergelogs.Add(cusmerlog);
                db.SaveChanges();


            }
        }
        public void transferprojects(long oldcustomeraccountid, long newcustomeraccountid, long custmergeid)
        {
            var accounttrans = db.Projects.Where(o => o.Customer == oldcustomeraccountid).ToList();
            foreach (var ac in accounttrans)
            {
                ac.Customer = newcustomeraccountid;
                db.Entry(ac).State = EntityState.Modified;
                db.SaveChanges();
                customermergelog cusmerlog = new customermergelog
                {
                    customermergeid = custmergeid,
                    entryid = ac.ProjectId,
                    purpose = CustomerMergePurposeId.quotation,
                };
                db.customermergelogs.Add(cusmerlog);
                db.SaveChanges();


            }
        }
        public void transferrecieptpaymentjournal(long oldcustomeraccountid, long newcustomeraccountid, long custmergeid)
        {
            var accounttrans = db.Receipts.Where(o => o.PayFrom == oldcustomeraccountid).ToList();
            foreach (var ac in accounttrans)
            {
                ac.PayFrom = newcustomeraccountid;
                db.Entry(ac).State = EntityState.Modified;
                db.SaveChanges();
                customermergelog cusmerlog = new customermergelog
                {
                    customermergeid = custmergeid,
                    entryid = ac.ReceiptId,
                    purpose = CustomerMergePurposeId.recieptpayfrom,
                };
                db.customermergelogs.Add(cusmerlog);
                db.SaveChanges();


            }
            accounttrans = db.Receipts.Where(o => o.PayTo == oldcustomeraccountid).ToList();
            foreach (var ac in accounttrans)
            {
                ac.PayTo = newcustomeraccountid;
                db.Entry(ac).State = EntityState.Modified;
                db.SaveChanges();
                customermergelog cusmerlog = new customermergelog
                {
                    customermergeid = custmergeid,
                    entryid = ac.ReceiptId,
                    purpose = CustomerMergePurposeId.recieptpayto,
                };
                db.customermergelogs.Add(cusmerlog);
                db.SaveChanges();


            }

            var accounttranspay = db.Payments.Where(o => o.PayTo == oldcustomeraccountid).ToList();
            foreach (var ac in accounttranspay)
            {
                ac.PayTo = newcustomeraccountid;
                db.Entry(ac).State = EntityState.Modified;
                db.SaveChanges();
                customermergelog cusmerlog = new customermergelog
                {
                    customermergeid = custmergeid,
                    entryid = ac.PaymentId,
                    purpose = CustomerMergePurposeId.paymentpayto,
                };
                db.customermergelogs.Add(cusmerlog);
                db.SaveChanges();
            }
            accounttranspay = db.Payments.Where(o => o.PayFrom == oldcustomeraccountid).ToList();
            foreach (var ac in accounttranspay)
            {
                ac.PayFrom = newcustomeraccountid;
                db.Entry(ac).State = EntityState.Modified;
                db.SaveChanges();
                customermergelog cusmerlog = new customermergelog
                {
                    customermergeid = custmergeid,
                    entryid = ac.PaymentId,
                    purpose = CustomerMergePurposeId.paymentpayfrom,
                };
                db.customermergelogs.Add(cusmerlog);
                db.SaveChanges();

            }
            var accounttransjou = db.Journals.Where(o => o.PayFrom == oldcustomeraccountid).ToList();
            foreach (var ac in accounttransjou)
            {
                ac.PayFrom = newcustomeraccountid;
                db.Entry(ac).State = EntityState.Modified;
                db.SaveChanges();
                customermergelog cusmerlog = new customermergelog
                {
                    customermergeid = custmergeid,
                    entryid = ac.JournalId,
                    purpose = CustomerMergePurposeId.journalpayfrom,
                };
                db.customermergelogs.Add(cusmerlog);
                db.SaveChanges();

            }
             accounttransjou = db.Journals.Where(o => o.PayTo == oldcustomeraccountid).ToList();

            foreach (var ac in accounttransjou)
            {
                ac.PayTo = newcustomeraccountid;
                db.Entry(ac).State = EntityState.Modified;
                db.SaveChanges();
                customermergelog cusmerlog = new customermergelog
                {
                    customermergeid = custmergeid,
                    entryid = ac.JournalId,
                    purpose = CustomerMergePurposeId.journalpayto,
                };
                db.customermergelogs.Add(cusmerlog);
                db.SaveChanges();
            }
        }
        // GET: /Delete/5
        [RedirectingAction]
        [Authorize(Roles = "Dev,Delete Customer Merge")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            return PartialView();
        }

        // POST: /Delete/5

        [RedirectingAction]
        [Authorize(Roles = "Dev,Delete Customer Merge")]
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
                msg = "Successfully Deleted Customer Merge  details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public bool DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            bool stat = false;
            string msg = "";
            var userid = User.Identity.GetUserId();
            DateTime crdate = System.DateTime.Now;
            var cu = db.customermerges.Find(id);

            if(1==1 && cu!=null)
            {
                
                db.customermerges.Remove(cu);
                db.SaveChanges();
                long oldcustomeraccountid = db.Customers.Find(cu.oldcustomerid).Accounts;
                long newcustomeraccountid = db.Customers.Find(cu.newcustomerid).Accounts;
                long oldcustomeraccountidid = db.Customers.Find(cu.oldcustomerid).CustomerID;
                long newcustomeraccountidid = db.Customers.Find(cu.newcustomerid).CustomerID;
                long custmergeid = 0;
                var cusupdate = db.Customers.Find(cu.oldcustomerid);
                StringBuilder builder = new StringBuilder(cusupdate.CustomerName);
                builder.Replace("OLD-", "");

                cusupdate.CustomerName = builder.ToString();
                db.Entry(cusupdate).State = EntityState.Modified;
                db.SaveChanges();
                var acupdate = db.Accountss.Find(cusupdate.Accounts);
                 builder = new StringBuilder(acupdate.Name);
                builder.Replace("OLD-", "");
                acupdate.Name = builder.ToString();
                db.Entry(acupdate).State = EntityState.Modified;
                db.SaveChanges();
                var trasid = db.customermergelogs.Where(o => o.customermergeid == cu.custmergeid).ToList();
                foreach (var tr in trasid)
                {
                    db.customermergelogs.Remove(tr);
                    db.SaveChanges();
                    if (tr.purpose == CustomerMergePurposeId.accountstransaction)
                    {
                        var ac = db.AccountsTransactions.Find(tr.entryid);
                        ac.Account = oldcustomeraccountid;
                        db.Entry(ac).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else if (tr.purpose == CustomerMergePurposeId.recieptpayfrom)
                    {
                        var ac = db.Receipts.Find(tr.entryid);
                        ac.PayFrom = oldcustomeraccountid;
                        db.Entry(ac).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else if (tr.purpose == CustomerMergePurposeId.recieptpayto)
                    {
                        var ac = db.Receipts.Find(tr.entryid);
                        ac.PayTo = oldcustomeraccountid;
                        db.Entry(ac).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else if (tr.purpose == CustomerMergePurposeId.paymentpayfrom)
                    {
                        var ac = db.Payments.Find(tr.entryid);
                        ac.PayFrom = oldcustomeraccountid;
                        db.Entry(ac).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else if (tr.purpose == CustomerMergePurposeId.paymentpayto)
                    {
                        var ac = db.Payments.Find(tr.entryid);
                        ac.PayTo = oldcustomeraccountid;
                        db.Entry(ac).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else if (tr.purpose == CustomerMergePurposeId.journalpayfrom)
                    {
                        var ac = db.Journals.Find(tr.entryid);
                        ac.PayFrom = oldcustomeraccountid;
                        db.Entry(ac).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else if (tr.purpose == CustomerMergePurposeId.journalpayto)
                    {
                        var ac = db.Journals.Find(tr.entryid);
                        ac.PayTo = oldcustomeraccountid;
                        db.Entry(ac).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else if (tr.purpose == CustomerMergePurposeId.sales)
                    {
                        var ac = db.SalesEntrys.Find(tr.entryid);
                        ac.Customer = oldcustomeraccountidid;
                        db.Entry(ac).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else if (tr.purpose == CustomerMergePurposeId.salesreturn)
                    {
                        var ac = db.SalesReturns.Find(tr.entryid);
                        ac.Customer = oldcustomeraccountidid;
                        db.Entry(ac).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else if (tr.purpose == CustomerMergePurposeId.quotation)
                    {
                        var ac = db.Quotations.Find(tr.entryid);
                        if (ac != null)
                        {
                            ac.Customer = oldcustomeraccountidid;
                            db.Entry(ac).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    else if (tr.purpose == CustomerMergePurposeId.performa)
                    {
                        var ac = db.ProFormas.Find(tr.entryid);
                        ac.Customer = oldcustomeraccountidid;
                        db.Entry(ac).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else if (tr.purpose == CustomerMergePurposeId.task)
                    {
                        var ac = db.ProTasks.Find(tr.entryid);
                        ac.CustomerID = oldcustomeraccountidid;
                        db.Entry(ac).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else if (tr.purpose == CustomerMergePurposeId.amc)
                    {
                        var ac = db.Amcs.Find(tr.entryid);
                        ac.CustomerId = oldcustomeraccountidid;
                        db.Entry(ac).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else if (tr.purpose == CustomerMergePurposeId.deliverynote)
                    {
                        var ac = db.Deliverynotes.Find(tr.entryid);
                        ac.Customer = oldcustomeraccountidid;
                        db.Entry(ac).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else if (tr.purpose == CustomerMergePurposeId.salesorder)
                    {
                        var ac = db.SalesOrders.Find(tr.entryid);
                        ac.Customer = oldcustomeraccountidid;
                        db.Entry(ac).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else if (tr.purpose == CustomerMergePurposeId.project)
                    {
                        var ac = db.Projects.Find(tr.entryid);
                        ac.Customer = oldcustomeraccountidid;
                        db.Entry(ac).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else if (tr.purpose == CustomerMergePurposeId.boq)
                    {
                        var ac = db.BillOfQyts.Find(tr.entryid);
                        ac.Customer = oldcustomeraccountidid;
                        db.Entry(ac).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else if (tr.purpose == CustomerMergePurposeId.conactrelation)
                    {
                        var accounttrans = db.ContactRelation.Find(tr.entryid);
                        accounttrans.RelationID = oldcustomeraccountidid;
                            db.Entry(accounttrans).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else if (tr.purpose == CustomerMergePurposeId.documents)
                    {
                        var ac = db.CustomerDocuments.Find(tr.entryid);
                        ac.CutomerID = oldcustomeraccountidid;
                        db.Entry(ac).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }

                
                   
                    }

            return true;
             }
        public string chkDeleteWithMsg(long id)
            {

            string msg = null;
            return msg;
        }

        //[HttpGet]
        //                       where a.TeamId == Team.TeamId
        //                           a.EmployeeId


        [HttpGet]
        public JsonResult GetAllMembers(long[] assign)
        {
            var teamss = (from a in db.Teams
                          join b in db.TeamMembers on a.TeamId equals b.TeamId into teams
                          from b in teams.DefaultIfEmpty()
                         where assign.Contains(a.TeamId) 
                          select new
                          {
                              emp = (b.EmployeeId != null)? b.EmployeeId : 0,
                              lead = a.TeamLead,
                          }).ToList();
            return Json(teamss);
        }

        public JsonResult SearchTeam(string q, string x)
        {

            var UserId = User.Identity.GetUserId();
            List<SelectFormat> serialisedJson;
            string stt = "Individual";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.Teams
                                  join b in db.Employees on a.TeamLead equals b.EmployeeId into teams
                                  from b in teams.DefaultIfEmpty()
                                  where a.TeamName.Contains(q) || a.TeamName.ToLower().Contains(q.ToLower()) //|| b.FirstName.ToLower().Contains(q.ToLower()) || b.LastName.ToLower().Contains(q.ToLower()) || b.FirstName.Contains(q) || b.LastName.Contains(q)
                                  select new SelectFormat
                                  {
                                      text = a.TeamName, //db.Employees.Where(c => c.EmployeeId == a.TeamLead).Select(c => c.FirstName + " "+ c.LastName).FirstOrDefault(), //each json object will have 
                                      id = a.TeamId
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.Teams.Select(b => new SelectFormat
                {
                    text = b.TeamName,// db.Employees.Where(a => a.EmployeeId == b.TeamLead).Select(a => a.FirstName + " " + a.LastName).FirstOrDefault(),//each json object will have 
                    id = b.TeamId
                }).OrderBy(b => b.text).ToList();

            }//
            return Json(serialisedJson);
        }
    }
}
