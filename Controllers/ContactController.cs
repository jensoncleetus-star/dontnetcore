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
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace QuickSoft.Controllers
{
    [RedirectingAction]
    public class ContactController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ContactController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Contact
        [QkAuthorize(Roles = "Dev,Contact List")]
        public ActionResult Index()
        {
            var Parent = db.ContactGroups.Select(s => new
            {
                ID = s.ContactGroupID,
                Name = s.Name
            }).ToList();
            ViewBag.ParentModules = QkSelect.List(Parent, "ID", "Name");
            var name = db.Contacts.Select(s => new
            {
                ID = s.Name,
                Name = s.Name
            }).Distinct().ToList();
            ViewBag.Contact = QkSelect.List(name, "ID", "Name");

            var email = db.Contacts.Select(s => new
            {
                ID = s.Name,
                Name = s.EmailId
            }).Distinct().ToList();
            ViewBag.Email = QkSelect.List(email, "ID", "Name");
            return View();
        }


        [HttpPost]
        [QkAuthorize(Roles = "Dev,Contact List")]
        public JsonResult getContact(string Name, string Email, string Phone, string Mobile, long? Group)
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
            var uEdit = User.IsInRole("Edit Contact");
            var uDelete = User.IsInRole("Delete Contact");

            // Distinct names that have at least one record carrying real contact details. LEFT-JOINed
            // below so EF emits a single hash anti-join (fast) instead of a per-row correlated subquery
            // (which times out on 215k rows). Distinct() is essential — without it the join would
            // multiply rows for names that have several populated records.
            var populatedNames = db.Contacts
                .Where(x => (x.EmailId != null && x.EmailId != "")
                         || (x.Mobile != null && x.Mobile != "")
                         || (x.Phone != null && x.Phone != ""))
                .Select(x => x.Name)
                .Distinct();

            var v = (from a in db.Contacts
                     join b in db.ContactGroups on a.Group equals b.ContactGroupID into con
                     from b in con.DefaultIfEmpty()
                     join pn in populatedNames on a.Name equals pn into pnj
                     from pn in pnj.DefaultIfEmpty()   // pn != null  ⇒ this name has a populated record
                         // (removed an unused ContactRelation join that duplicated every contact row)
                         //let mob = (from z in db.Contacts
                         //           where z.ContactID == a.ContactID
                         //               Num = z.Mobile == null ? z.Phone : z.Mobile,
                         //               Name = z.EmailId
                         //           }).ToList()
                         //let mob = (from z in db.Mobiles
                         //           where z.Contact == a.ContactID
                         //               Num = z.MobileNum,
                         //               Name = z.Name
                         //           }).ToList()
                     where
                     (Name == "" || a.Name.ToUpper().Contains(Name.ToUpper())) &&
                     (Email == "" || a.EmailId.ToLower().Contains(Email.ToLower())) &&
                     (Group == null || a.Group == Group) &&
                     (Mobile == "" || a.Mobile.Contains(Mobile)) && //c.MobileNum.Contains(Mobile)) &&
                     (Phone == "" || a.Phone.Contains(Phone)) &&
                     // De-dup (display only, nothing deleted): hide a blank "shadow" record — one with
                     // no email/mobile/phone — when another record with the SAME name DOES have details
                     // (i.e. its name matched the populatedNames anti-join, so pn != null).
                     !((string.IsNullOrEmpty(a.EmailId) && string.IsNullOrEmpty(a.Mobile) && string.IsNullOrEmpty(a.Phone))
                       && pn != null)
                     select new
                     {
                         ContactName = a.Name,
                         id = a.ContactID,
                         Address = a.Address != null ? a.Address : "" +
                         "<br/>" + a.City != null ? a.City : "" +
                         " " + a.State != null ? a.State : "" +
                         " " + a.Country != null ? a.Country : "" +
                         "<br/>" + a.Zip != null ? a.Zip : "",
                         a.Phone,
                         a.Mobile,
                         a.EmailId,
                         //a.Status,
                         EdByGroup = (b.Editable == choice.Yes) ? 0 : 1,
                         Group = b.Name,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete,
                         mobmodel = new MobileViewModel(),
                         //(from ac in db.Mobiles
                         //            where (ac.Contact == a.ContactID) && (Mobile == "" || ac.MobileNum.Contains(Mobile))
                         //                Num = ac.MobileNum,
                         //                Name = ac.Name
                         //mobmodel = mob
                     });


            //search
          
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search across all useful fields (name, mobile, phone, email, address, group)
                var s = search.ToLower();
                v = v.Where(p => (p.ContactName != null && p.ContactName.ToString().ToLower().Contains(s)) ||
                                 (p.Mobile != null && p.Mobile.ToString().ToLower().Contains(s)) ||
                                 (p.Phone != null && p.Phone.ToString().ToLower().Contains(s)) ||
                                 (p.EmailId != null && p.EmailId.ToString().ToLower().Contains(s)) ||
                                 (p.Address != null && p.Address.ToString().ToLower().Contains(s)) ||
                                 (p.Group != null && p.Group.ToString().ToLower().Contains(s)));

            }

            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);
            }

            recordsTotal = v.Count();   // real total so the footer is correct and you can page through ALL contacts
            var data = v.Skip(skip).Take(pageSize).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        [QkAuthorize(Roles = "Dev,Create Contact")]
        public ActionResult Create()
        {

            var cust = db.Contacts
                .Select(r => new
            {
                ID = r.ContactID,
                Name= r.Mobile
            }).ToList();

            ViewBag.CustName = QkSelect.List(cust, "ID", "Name");

            ViewBag.MobileNumber = QkSelect.List("ID", "Name");

            var mail = db.Contacts
               .Select(r => new
               {
                   ID = r.ContactID,
                   Name = r.EmailId
               }).ToList();
            ViewBag.EmailId = QkSelect.List(mail, "ID", "Name");
            var stands = db.ContactGroups
                       .Select(s => new
                       {
                           FieldID = s.Name,
                           FieldName = s.ContactGroupID
                       })
                       .ToList();

            ViewBag.ParentModules = QkSelect.List(stands, "FieldName", "FieldID");

            var maxVal = db.Contacts.Select(p => p.ContactID).AsEnumerable().DefaultIfEmpty(0).Max();

            ViewBag.LastEntry = maxVal;

            ViewBag.ContactId = "CT" + (maxVal + 1).ToString();

            ViewBag.contactGrp = db.ContactGroups.Select(x => new
            {
                ContactGroupID = x.ContactGroupID,
                Name = x.Name
            }).ToList();
            ViewBag.counties = db.Country.Select(x => new
            {
                countryID = x.CountryID,                                                                                                                                                                                                                                                
                countryName = x.CountryName+" ("+x.CountryCode+")"
            }).ToList();

            //    documentID = x.ID,
            //    documentName = x.Name
            ViewBag.DocumentType = QkSelect.List(db.DocumentTypes, "Name", "ID");


            return View();
        }

        public ActionResult GetDocTypes()
        {
            return Json(db.DocumentTypes.Select(x => new
            {
                ID = x.ID,
                Name = x.Name
            }).ToList());
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Create Contact")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ContactformViewModel contact)
        {
            string Mob1 = null;
            var contactExists = db.Contacts.Any(u => u.ContactID == contact.ContactID);

            if (contact.MobileViewModel != null)
            {
                Mob1 = contact.MobileViewModel.ElementAt(0).Num;
            }


            if (contactExists)
            {
                Danger("A Contact with same Contact Id exists.", true);
                return RedirectToAction("Create", "Contact");
            }
            else if(Mob1 == null)
            {
                Warning("Enter Mobile No. for first row..", true);
                return RedirectToAction("Create", "Contact");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var  MobNo = contact.MobileViewModel.ElementAt(0).Num.TrimStart(new char[] { '0' });
                  
                    var con = new Contact
                     {
                        Name = contact.Name,
                        Address = contact.Address,
                        City = contact.City,
                        State = contact.State,
                        Country = contact.Country,
                        Zip = contact.Zip,
                        Phone = contact.Phone,
                        Mobile = MobNo,
                        Fax = contact.Fax,
                        EmailId = contact.MobileViewModel.ElementAt(0).Name,
                        Reference = contact.Reference,
                        ContactPerson = contact.ContactPerson,
                        Group = contact.Group,
                        Status = Status.active,
                        FirstName = contact.FirstName,
                        LastName = contact.LastName,
                        ContactGroupID = contact.ContactGroupID,
                        //CreatedDate = System.DateTime.Now,
                        Website = contact.Website,
                        ContactCode = contact.ContactCode,
                        CountryID= contact.CountryID

                    };
                    db.Contacts.Add(con);
                    db.SaveChanges();

                    Int64 ContactId = con.ContactID;
                    /*********For saving multiple rows of Mobile & EmailID into table Mobiles****************/
                    if (contact.MobileViewModel != null)
                    {
                        foreach (var contactdet in contact.MobileViewModel)
                        {

                            if (contactdet.Num != null || contactdet.Name != null)
                            {

                                if (contactdet.Num != null)
                                {
                                    contactdet.Num = contactdet.Num.TrimStart(new char[] { '0' });
                                }

                                var det = new Mobile
                                {
                                    Contact = ContactId,
                                    MobileNum = contactdet.Num,
                                    Name = contactdet.Name
                                };

                                db.Mobiles.Add(det);
                                db.SaveChanges();
                            }
                        }

                    }
                    /***********************************************************/

                    if (contact.CustomerDocumentViewModel != null)
                    {
                        foreach (var docs in contact.CustomerDocumentViewModel)
                        {
                            if (docs.DocumentTypeID != null)
                            {
                                if (docs.File != null)
                                {
                                    string FileName = string.Empty;
                                    if (docs.File.Length > 0)
                                    {
                                        FileName = Path.GetFileName(docs.File.FileName);
                                        string _path = Path.Combine(LegacyWeb.MapPath("~/UploadedFiles"), FileName);
                                        docs.File.SaveAs(_path);
                                    }

                                    var doc = new CustomerDocument
                                    {
                                        ContactId = ContactId,
                                        DoucumentType = docs.DocumentTypeID.ToString(),
                                        Expiry = docs.Expiry,
                                        FilePath = FileName,
                                        Notes = docs.Notes,
                                        DocumentTypeID=docs.DocumentTypeID
                                        
                                    };
                                    db.CustomerDocuments.Add(doc);
                                    db.SaveChanges();
                                }
                            }
                        }
                    }

                    // last inserted item id con.id
                    var userid = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, userid, "Contact", "Contacts", findip(), con.ContactID, "contacts added Successfully");
                    // end log updates
                    Success("Successfully added Contact details.", true);
                    return RedirectToAction("Create", "Contact");
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
        public ActionResult ContactStatus(long? id, string type)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contact cnt = db.Contacts.Find(id);
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
        // POST: contact/ChangeStatus/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ContactStatus(string type, long? id, Contact contact)
        {
            bool stat = false;
            string msg;
            string types = "";
            var userid = User.Identity.GetUserId();
            Contact cnt = db.Contacts.Find(id);
            if (contact.Status == Status.inactive)
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

            com.addlog(LogTypes.Changed, userid, "Contact", "Contacts", findip(), cnt.ContactID, "Successfully Changed the Contact to" + types);


            stat = true;
            msg = " Successfully Changed the Contact to" + types;
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        // GET: contact/Edit/5
        [QkAuthorize(Roles = "Dev,Edit Contact")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var cust = db.Contacts
               .Select(r => new
               {
                   ID = r.ContactID,
                   Name = r.Mobile
               }).ToList();

            Contact cnt = db.Contacts.Find(id);

            if (cnt == null)
            {
                return NotFound();
            }

            //For return viewmodel
            ContactformViewModel contactformmodel = new ContactformViewModel();

            //For
            //displaying data
            List<MobileViewModel> cnmobile = new List<MobileViewModel>();
            MobileViewModel aaa = new MobileViewModel();

            //    Num = contactformmodel.Mobile,
            //    Name = contactformmodel.FirstName


            string phonenumber = "";
            string cemail = "";
            if (!String.IsNullOrEmpty(contactformmodel.Phone))
                phonenumber = contactformmodel.Phone;
            else
                phonenumber = contactformmodel.Mobile;
            cemail = contactformmodel.EmailId;
            if (phonenumber != null)
            {
                cnmobile.Add(new MobileViewModel { Name = cemail, Num = phonenumber });
            }

            contactformmodel.ContactCode = cnt.ContactCode;
            contactformmodel.FirstName = cnt.FirstName;
            contactformmodel.LastName = cnt.LastName;
            contactformmodel.Name = cnt.Name;
            contactformmodel.Website = cnt.Website;
            contactformmodel.CountryID = cnt.CountryID;
            contactformmodel.ContactGroupID = cnt.ContactGroupID;
            contactformmodel.Phone = cnt.Phone;
            contactformmodel.MobileViewModel = cnmobile.ToList();
            //For Country dropdown
            var Country = db.Country.Select(s => new
            {
                Id = s.CountryID,
                Name = s.CountryName,
            }).ToList();

            ViewBag.Country = QkSelect.List(Country, "Id", "Name");

            //For contact Group dropdown
            var ContactGroup = db.ContactGroups.Select(x => new
            {
                Id = x.ContactGroupID,
                Name = x.Name
            }).ToList();

            ViewBag.ContactGroup = QkSelect.List(ContactGroup, "Id", "Name");           

            var stands = db.ContactGroups
                       .Select(s => new
                       {
                           FieldID = s.Name,
                           FieldName = s.ContactGroupID
                       })
                       .ToList();

            ViewBag.ParentModules = QkSelect.List(stands, "FieldName", "FieldID");

            ViewBag.preEntry = db.Contacts.Where(a => a.ContactID < id).Select(a => a.ContactID).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.Contacts.Where(a => a.ContactID > id).Select(a => a.ContactID).DefaultIfEmpty().Min();                        

            List<CustomerDocument> cuslists = new List<CustomerDocument>(); 
            cuslists = db.CustomerDocuments.Where(x => x.ContactId == cnt.ContactID).ToList();
            ViewBag.cuslists = cuslists;

            ViewBag.DocList = db.DocumentTypes.ToList();

            return View(contactformmodel);
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Contact")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ContactformViewModel cnt, long? id)
        {
            if (ModelState.IsValid)
            {
                string Mob1 = null;

                if (cnt.MobileViewModel != null)
                {
                    Mob1 = cnt.MobileViewModel.ElementAt(0).Num;
                }

                var Exists = db.Contacts.Any(c => c.Name == cnt.Name && c.ContactID != id);

                if (Exists)
                {
                    Warning("Contact already exists.", true);
                }
                else if (Mob1 == null)
                {
                    Warning("Enter Mobile No. for first row..", true);
                    return RedirectToAction("Edit", "Contact", id);
                }
                else
                {
                    Contact contact = db.Contacts.Find(id);
                    
                    contact.Name = cnt.Name;
                    contact.Address = cnt.Address;
                    contact.City = cnt.City;
                    contact.State = cnt.State;
                    contact.Country = cnt.Country;
                    contact.Zip = cnt.Zip;
                    contact.Mobile = cnt.MobileViewModel.ElementAt(0).Num.TrimStart(new char[] { '0' });
                    contact.Fax = cnt.Fax;
                    contact.EmailId = cnt.MobileViewModel.ElementAt(0).Name;
                    contact.Reference = cnt.Reference;
                    contact.ContactPerson = cnt.ContactPerson;
                    contact.Group = cnt.Group;
                    contact.ContactCode = cnt.ContactCode;
                    contact.Website = cnt.Website;
                    contact.ContactGroupID = cnt.ContactGroupID;
                    contact.FirstName = cnt.FirstName;
                    contact.LastName = cnt.LastName;
                    contact.CountryID = cnt.CountryID;

                    db.Entry(contact).State = EntityState.Modified;
                    db.SaveChanges();

                    Int64 ContactId = contact.ContactID;

                    //Mobile lists for corresponding ContactID

                    //To delete all the rows in Mobile table
                    db.Mobiles.RemoveRange(db.Mobiles.Where(a => a.Contact == ContactId));
                    db.SaveChanges();

                    if (cnt.MobileViewModel != null)
                    {                         
                            foreach (var i in cnt.MobileViewModel)
                            {
                                //for adding new rows
                                    if (i.Num != null || i.Name != null)
                                    {
                                        if (i.Num != null)
                                        {
                                            i.Num = i.Num.TrimStart(new char[] { '0' });
                                        }

                                        var ContactDetails = new Mobile
                                        {
                                            Contact = ContactId,
                                            MobileNum = i.Num,
                                            Name = i.Name
                                        };
                                        db.Mobiles.Add(ContactDetails);
                                        db.SaveChanges();
                                    }
                                //for updating existing rows


                            }


                            ////For deleting any rows

                            //    //delete the rows which are not in ViewModel
                    }

                    if (cnt.CustomerDocumentViewModel != null)
                    {
                        foreach (var docs in cnt.CustomerDocumentViewModel)
                        {
                            if (docs.DocumnetId == 0)
                            {
                                if (docs.DocumentTypeID != null)
                                {
                                    string FileName = string.Empty;
                                    if (docs.File != null)
                                    {
                                        if (docs.File.Length > 0)
                                        {
                                            FileName = Path.GetFileName(docs.File.FileName);
                                            string _path = Path.Combine(LegacyWeb.MapPath("~/UploadedFiles"), FileName);
                                            docs.File.SaveAs(_path);
                                        }
                                    }
                                    var doc = new CustomerDocument
                                    {
                                        ContactId = ContactId,
                                        DoucumentType = docs.DocumentTypeID.ToString(),
                                        Expiry = docs.Expiry,
                                        FilePath = FileName,
                                        Notes = docs.Notes,
                                        DocumentTypeID=docs.DocumentTypeID
                                    };
                                    db.CustomerDocuments.Add(doc);
                                    db.SaveChanges();

                                }
                            }
                            else
                            {
                               
                                var editDoc = db.CustomerDocuments.Find(docs.DocumnetId);
                                editDoc.DoucumentType = docs.DocumentTypeID.ToString();
                                editDoc.Expiry = docs.Expiry;
                                editDoc.Notes = docs.Notes;
                                editDoc.DocumentTypeID = docs.DocumentTypeID;
                                db.SaveChanges();
                            }
                        }
                    }
                                       

                    var userid = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, userid, "Contact", "Contacts", findip(), contact.ContactID, "Contacts Updated Successfully");

                    ViewBag.counties = db.Country.Select(x => new
                    {
                        countryID = x.CountryID,
                        countryName = x.CountryName + " (" + x.CountryCode + ")"
                    }).ToList();

                    Success("Successfully Updated Contact Details.", true);
                    return RedirectToAction("Index", "Contact");
                }
            }
            else
            {
                Warning("Looks like something went wrong. Please check your form..", true);
                return View();
            }
            return View();
        }
        // GET: CONTACT/Delete/5
        [HttpGet]
        [QkAuthorize(Roles = "Dev,Delete Contact")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contact con = db.Contacts.Find(id);
            if (con == null)
            {
                return NotFound();
            }
            return PartialView(con);
        }

        // POST: CONTACT/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Delete Contact")]
        public ActionResult DeleteAction(long id)
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
                msg = "Successfully deleted Contact details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost, ActionName("DeleteDocument")]
      
        [QkAuthorize(Roles = "Dev,Delete Contact")]
        public ActionResult DeleteDocument(long id)
        {
            bool stat;
            string msg;
            var obj = db.CustomerDocuments.Find(id);
            if (obj == null)
            {
                msg = "Failed";
                stat = false;
            }
            else
            {
                db.CustomerDocuments.Remove(obj);
                db.SaveChanges();
                stat = true;
                msg = "Successfully deleted Document details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Contact")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteItem(arr) == true) ? count++ : notdel++;
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Contact, Unable to Delete " + notdel + " Item Size. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + "  Contact.", true);
            }
            else
            {
                Success("Deleted " + count + "  Contact.", true);
            }
            return Json("OK");
        }
        private Boolean DeleteItem(long id)
        {
            var Msg = chkDeleteWithMsg(id);
            if (Msg != null)
            {
                return false;
            }
            else
            {
                return DeleteFn(id);
            }
        }

        public bool DeleteFn(long? id)
        {
            Contact Con = db.Contacts.Find(id);
            db.Contacts.Remove(Con);
            db.SaveChanges();
            db.Mobiles.RemoveRange(db.Mobiles.Where(a => a.Contact == id));
            db.SaveChanges();
            var userid = User.Identity.GetUserId();
            com.addlog(LogTypes.Deleted, userid, "Contact", "Contacts", findip(), Con.ContactID, "Contacts Deleted Successfully");
            return true;
        }

        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            if (db.Customers.Any(c => c.Contact == id))
            {
                msg = "Contact Already used in Customers !!";
            }
            else if (db.Suppliers.Any(c => c.Contact == id))
            {
                msg = "Contact Already used in Suppliers !!";
            }
            else
            {
                msg = null;
            }

            return msg;
        }

        [HttpGet]
        [QkAuthorize(Roles = "Dev,View Contact")]
        public ActionResult ViewDetails(long? id)
        {
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                Contact con = db.Contacts.Find(id);

                if (con == null)
                {
                    return NotFound();
                }

                ContactViewModel cnts = new ContactViewModel();

                cnts = (from a in db.Contacts
                        join b in db.ContactGroups on a.Group equals b.ContactGroupID into cong
                        from b in cong.DefaultIfEmpty()
                        join c in db.Mobiles on a.ContactID equals c.Contact into cont
                        from c in cont.DefaultIfEmpty()
                        where a.ContactID == con.ContactID
                        select new ContactViewModel
                        {
                            Name = a.Name,
                            Address = a.Address,
                            City = a.City,
                            State = a.State,
                            Country = a.Country,
                            Zip = a.Zip,
                            Phone = a.Phone,
                            mobmodel = (from ac in db.Mobiles
                                        where (ac.Contact == con.ContactID)
                                        select new MobileViewModel
                                        {
                                            Num = ac.MobileNum
                                        }).ToList(),
                            Fax = a.Fax,
                            EmailId = a.EmailId,
                            Reference = a.Reference,
                            ContactPerson = a.ContactPerson,
                            GroupName = b.Name,
                        }).FirstOrDefault();

                return View(cnts);
            }
        }

        [HttpGet]
        public JsonResult GetMobile(long CnId)
        {
            //Retrieving data from table Mobile
            var ConD = (from a in db.Mobiles
                        where a.Contact == CnId
                        select new
                        {
                            //ID = a.ID,
                            Mob = a.MobileNum,
                            Name = a.Name
                        }).ToList();

            //Retrieving data from table Contacts
            var cn = ( from c in db.Contacts
                        where c.ContactID == CnId
                        select new
                        {
                            // ID =c.ContactID,                   
                            Mob = c.Mobile == null? c.Phone : c.Mobile,
                            Name = c.EmailId
                        }).ToList();

            if(ConD.Count()>0)
            {
                cn = cn.Union(ConD).Distinct().ToList();
            }            
            return Json(cn);
        }
    }

    public class ContactGroupController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public ContactGroupController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Contact
        [QkAuthorize(Roles = "Dev,Contact Group List")]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Contact Group List")]
        public JsonResult GroupGetData()
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
            var uEdit = User.IsInRole("Edit Contact Group");
            var uDelete = User.IsInRole("Delete Contact Group");

            var v = (from a in db.ContactGroups
                     join b in db.ContactGroups on a.Parent equals b.ContactGroupID into congp
                     from b in congp.DefaultIfEmpty()
                     select new
                     {
                         id = a.ContactGroupID,
                         a.Name,
                         Parent = b.Name,
                         Editable = a.Editable,
                         Dev = uDev,
                         Edit = uEdit,
                         Delete = uDelete
                     });
            //search
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                v = v.Where(p => p.Name.ToString().ToLower().Contains(search.ToLower()) ||
                                 p.Parent.ToString().ToLower().Contains(search.ToLower()));
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

        [QkAuthorize(Roles = "Dev,Create Contact Group")]
        public ActionResult Create()
        {
            //              .Select(s => new
            //                 FieldID = s.Name,
            //                 FieldName = s.ContactGroupID
            //             })

            var stands = db.ContactGroups
                         .Select(s => new
                         {
                             FieldID = s.Name,
                             FieldName = s.ContactGroupID
                         })
                         .ToList();

            ViewBag.ParentModules = QkSelect.List(stands, "FieldName", "FieldID");
            return PartialView();

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [QkAuthorize(Roles = "Dev,Create Contact Group")]
        public JsonResult Create(ContactGroup contact)
        {
            bool stat = false;
            string msg;
            Int64 id = 0;
            if (ModelState.IsValid)
            {
                var contactExists = db.ContactGroups.Any(u => u.Name == contact.Name);
                if (contactExists)
                {
                    msg = "A Group with same Name exists.";
                    stat = false;
                }
                else
                {
                    var con = new ContactGroup
                    {
                        Name = contact.Name,
                        Parent = contact.Parent,
                        Editable = choice.Yes
                    };
                    db.ContactGroups.Add(con);
                    db.SaveChanges();
                    id = con.ContactGroupID;

                    var userid = User.Identity.GetUserId();
                    com.addlog(LogTypes.Created, userid, "ContactGroup", "ContactGroups", findip(), con.ContactGroupID, "Contact Group Added Successfully");


                    msg = "Successfully added Contact Group details.";
                    stat = true;
                }
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }

            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg, id } };
        }


        // GET: contact/Edit/5
        [QkAuthorize(Roles = "Dev,Edit Contact Group")]
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ContactGroup con = db.ContactGroups.Find(id);

            if (con == null)
            {
                return NotFound();
            }



            var stands = db.ContactGroups.Where(s => s.ContactGroupID != id)
                          .Select(s => new
                          {
                              FieldID = s.Name,
                              FieldName = s.ContactGroupID
                          })
                         .ToList();
            ViewBag.ParentModules = QkSelect.List(stands, "FieldName", "FieldID", con.Parent);

            ContactGroup conGp = new ContactGroup();
            conGp.Name = con.Name;
            conGp.Parent = con.Parent;
            return PartialView(conGp);
        }

        // POST: contact/Edit/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Edit Contact Group")]
        public JsonResult Edit(ContactGroup con, long id)
        {
            bool stat = false;
            string msg;
            if (ModelState.IsValid)
            {
                ContactGroup congp = db.ContactGroups.Find(id);
                congp.Name = con.Name;
                congp.Parent = con.Parent;
                congp.Editable = choice.Yes;
                db.Entry(congp).State = EntityState.Modified;
                db.SaveChanges();

                var userid = User.Identity.GetUserId();
                com.addlog(LogTypes.Updated, userid, "ContactGroup", "ContactGroups", findip(), congp.ContactGroupID, "Contact Group Updated Successfully");


                msg = "Successfully Updated Group Details.";
                stat = true;
            }
            else
            {
                msg = "Looks like something went wrong. Please check your form.";
                stat = false;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        //  GET:  group/Delete/5
        [QkAuthorize(Roles = "Dev,Delete Contact Group")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ContactGroup congp = db.ContactGroups.Find(id);
            if (congp == null)
            {
                return NotFound();
            }

            return PartialView(congp);
        }

        // POST:tax group/Delete/5
        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Contact Group")]
        public JsonResult Delete(int id, IFormCollection collection)
        {
            bool stat = false;
            string msg;
            var Exists = db.Contacts.Any(c => c.Group == id);
            if (Exists)
            {
                msg = "Unable to delete group, Contact with this Group exists.";
                stat = false;
            }
            else
            {
                stat = DeleteFn(id);
                msg = "Successfully Deleted Group details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        [QkAuthorize(Roles = "Dev,Delete Contact Group")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteItem(arr) == true) ? count++ : notdel++;
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Contact Group, Unable to Delete " + notdel + " Contact Group. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Contact Group.", true);
            }
            else
            {
                Success("Deleted " + count + " Contact Group.", true);
            }
            return RedirectToAction("Index", "ContactGroup");
        }
        private Boolean DeleteItem(long id)
        {
            var Exists = db.Contacts.Any(c => c.Group == id);
            bool res = (Exists) ? false : DeleteFn(id);
            return res;
        }

        public bool DeleteFn(long id)
        {
            ContactGroup congp = db.ContactGroups.Find(id);
            if (congp != null)
            {
                db.ContactGroups.Remove(congp);
                db.SaveChanges();

                var userid = User.Identity.GetUserId();
                com.addlog(LogTypes.Deleted, userid, "ContactGroup", "ContactGroups", findip(), congp.ContactGroupID, "Contact Group Deleted Successfully");
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


        public JsonResult SearchMobilecontacts(string q,string x)
        {
            List<SelectFormat2> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (
                                  from c in db.Contacts
                                  
                                  where (c.Mobile.Contains(q))
                                  select new SelectFormat2
                                  {
                                      text = c.Mobile, //each json object will have 
                                      id = c.Mobile
                                  })
                                  .OrderBy(b => b.text).Distinct().ToList();

            }
            else
            {
                //serialisedJson = (from b in db.Customers
                //                  //where (b.Type ==  CRMCustomerType.Customer)
                //                      text = j.MobileNum, //each json object will have 
                //                      id =c.ContactID
                //                  })

                serialisedJson = (from c in db.Contacts
                                 
                                  where (c.Mobile.Contains(q))
                                  select new SelectFormat2
                                  {
                                      text =c.Mobile, //each json object will have 
                                      id = c.Mobile
                                  })
                                 .OrderBy(b => b.text).Distinct().ToList();


            }//

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
            }

            return Json(serialisedJson);
        }

        public JsonResult SearchEmailcontacts(string q, string x)
        {
            List<SelectFormat2> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (
                                  from c in db.Contacts
                                 
                                  where (c.EmailId.Contains(q))
                                  select new SelectFormat2
                                  {
                                      text = c.EmailId, //each json object will have 
                                      id = c.EmailId
                                  })
                                  .OrderBy(b => b.text).Distinct().ToList();

            }
            else
            {
                //serialisedJson = (from b in db.Customers
                //                  //where (b.Type ==  CRMCustomerType.Customer)
                //                      text = j.MobileNum, //each json object will have 
                //                      id =c.ContactID
                //                  })

                serialisedJson = (from c in db.Contacts
                                  
                                  where (c.EmailId.Contains(q))
                                  select new SelectFormat2
                                  {
                                      text = c.EmailId, //each json object will have 
                                      id = c.EmailId
                                  })
                                 .OrderBy(b => b.text).Distinct().ToList();


            }//

            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            {
            }

            return Json(serialisedJson);
        }
        public JsonResult getcontactsbymobiles(string mobile)
        {
            var serialisedJson = (from c in db.Contacts
                                  
                                  where (c.Mobile.Contains(mobile))
                                  select new
                                  {
                                      c.FirstName,
                                      c.ContactID,
                                      c.LastName,
                                      c.Name,
                                      c.Address,
                                      c.Mobile,
                                      c.EmailId,
                                      c.Website,
                                      c.CountryID,
                                      c.ContactTypeID
                                  }).ToList();
            return Json(serialisedJson);

        }

    }
}
