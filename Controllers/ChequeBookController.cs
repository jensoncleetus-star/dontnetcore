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
using System.Collections.Generic;
using System.Globalization;

namespace QuickSoft.Controllers
{
    public class ChequeBookController :  BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ChequeBookController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: ChequeBook
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public void transaction(long id)
        {
           
        }
        [HttpPost]
        public ActionResult GetChequeBook()
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

       


            var v = (from a in db.ChequeBooks
                     
                        select new
                     {
                         a.bookid,
                         a.bookname,
                         booktype=(a.booktype==Docbooktype.cheque)?"Cheque": (a.booktype == Docbooktype.reciept)?"Reciept Book":"Payment Book",
                         a.numberstarting,
                         a.endnumbering,
                         totalleaf=a.endnumbering-a.numberstarting,
                         balanceleaf=(a.endnumbering - a.numberstarting)-a.usedleaf-a.cancelledleaf,
                         a.usedleaf,
                         a.cancelledleaf,


                     });

            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.numberstarting.ToString().ToLower().Contains(search.ToLower()));
            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);

            }
            v=v.OrderByDescending(o => o.balanceleaf);

            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        [HttpPost]
        public ActionResult GetChequeBooktransaction(long id)
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




            var v = (from a in db.ChequeBooks
                     join b in db.chequetransactions on a.bookid equals b.bookid

                     where a.bookid==id
                     select new
                     {
                         a.bookid,
                         a.bookname,
                         b.docserialno,
                         booktype = (a.booktype == Docbooktype.cheque) ? "Cheque" : (a.booktype == Docbooktype.reciept) ? "Reciept Book" : "Payment Book",
                         purpose=(b.purpose==null)?"cancel":b.purpose,
                         b.remarks,
                         


                     });

           

            recordsTotal = v.Count();
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        public ActionResult Create()
        {


           

            return PartialView();



        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Create Item Size")]
        public JsonResult Create(ChequeBook vmodel)
        {
            bool stat = false;
            string msg;
            Int64 Id = 0;

            if (ModelState.IsValid)
            {


                db.ChequeBooks.Add(vmodel);
                db.SaveChanges();
                Id = vmodel.bookid;

                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Created, UserId, "Cheque Book", "Cheque Book", findip(), vmodel.bookid, "Cheque Book Added Successfully");

                msg = "Cheque Book/Reciept added successfully.";
                stat = true;

            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, Id = Id } };
        }

        public ActionResult Edit(long? id)
        {



            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChequeBook chk = db.ChequeBooks.Find(id);
            if (chk == null)
            {
                return NotFound();
            }

            

            return PartialView(chk);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        // [QkAuthorize(Roles = "Dev,Edit Item Size")]
        public JsonResult Edit(ChequeBook vmodel)
        {
            bool stat = false;
            string msg;

            if (ModelState.IsValid)
            {


                db.Entry(vmodel).State = EntityState.Modified;
                db.SaveChanges();

                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Updated, UserId, "ChequeBook", "ChequeBooks", findip(), vmodel.bookid, "cheque/reciept Updated Successfully");


                msg = "Successfully updated Location details.";
                stat = true;

            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public ActionResult cancel(long? id)
        {



            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChequeBook chk = db.ChequeBooks.Find(id);
            if (chk == null)
            {
                return NotFound();
            }
            chequetransactionviewmodal vmodel = new chequetransactionviewmodal
            {
                bookid = (long)id,
                transdate = (DateTime.Now).ToString("dd-MM-yyyy"),
                transtype = Docbooktype.cheque
            };


            return PartialView(vmodel);
        }
        [HttpPost]
        public JsonResult cancel(chequetransactionviewmodal ch)
        {

            bool stat = false;
            string msg;
            var UserId = User.Identity.GetUserId();
            ch.transtype = Docbooktype.cheque;
            if (ModelState.IsValid)
            {
                var bk = db.ChequeBooks.Any(a=>a.bookid==ch.bookid && a.numberstarting<=ch.docserialno && a.endnumbering>=ch.docserialno);
                if(bk)
                {
                    var exist = db.chequetransactions.Any(o => o.bookid == ch.bookid && o.docserialno == ch.docserialno);
                    if(!exist)
                    {
                        ch.transtype = db.ChequeBooks.Where(o => o.bookid == ch.bookid).Select(o => o.booktype).FirstOrDefault();
                        var Date = DateTime.Parse(ch.transdate.ToString(), new CultureInfo("en-GB"));
                        chequetransaction nch = new chequetransaction
                        {
 bookid=ch.bookid,
  remarks =ch.remarks,
   transdate=Date,
   transtype=ch.transtype,
   docserialno=ch.docserialno,
                        };
                        db.chequetransactions.Add(nch);
                        db.SaveChanges();
                        db.ChequeBooks.Where(o => o.bookid == ch.bookid).ToList().ForEach(o => o.cancelledleaf = o.cancelledleaf + 1);
                        db.SaveChanges();
                            msg = "Sccess";
                        stat = true;

                        com.addlog(LogTypes.Updated, UserId, "Cancel ChequeBook", "ChequeBooks", findip(), ch.chequetransid, "cheque/reciept cancel Successfully");

                    }
                    else
                    {
                        msg = "Leaf Already Cancelled Or Used";
                    }
                }
                else
                {
                    msg = "Looks like something went wrong. Please check your form.";
                }





               

            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };





        }
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ChequeBook chk = db.ChequeBooks.Find(id);
            if (chk == null)
            {
                return NotFound();
            }
            return PartialView(chk);
        }

        // POST: ItemCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //  [QkAuthorize(Roles = "Dev,Delete Item Color")]
        public JsonResult DeleteConfirmed(long id)
        {
            bool stat = false;
            string msg;
            var Exists = db.chequetransactions.Any(c => c.bookid == id);
            if (Exists)
            {
                msg = "Unable to delete , transaction exists.";
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Successfully deleted Location.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }



        public bool DeleteFn(long id)
        {
            ChequeBook vmodel = db.ChequeBooks.Find(id);
            if (vmodel != null)
            {
                db.ChequeBooks.RemoveRange(db.ChequeBooks.Where(a => a.bookid == id));


                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, UserId, "ChequeBook", "ChequeBooks", findip(), vmodel.bookid, "cheque/reciept Deleted Successfully");
                db.SaveChanges();
            }
            return true;

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
