using QuickSoft.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using QuickSoft.Controllers;
using QuickSoft.Models;
using QuickSoft.ViewModel;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace QuickSoft.Areas.Property.Controller
{
    [Microsoft.AspNetCore.Mvc.Area("Property")]
    public class UnitController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public UnitController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Property/Unit
        public ActionResult Index()
        {
            ViewBag.Alldata = QkSelect.List(
             new List<SelectListItem>
             {
                  new SelectListItem { Selected = true, Text = "All", Value = "0"},
             }, "Value", "Text", 0);
            return View();
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, DocumentType")]
        public ActionResult GetUnit(long? Property, long? UnitType, long? Unit, string FromDate, string ToDate)
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

            DateTime? fdate = null;
            DateTime? tdate = null;
            if (FromDate != "")
            {
                fdate = DateTime.Parse(FromDate, new CultureInfo("en-GB").DateTimeFormat);
            }
            if (ToDate != "")
            {
                tdate = DateTime.Parse(ToDate, new CultureInfo("en-GB").DateTimeFormat);
            }

            var UserView = (from a in db.PropertyUnits
                            join b in db.PropertyUnitTypes on a.UnitType equals b.ID into protype
                            from b in protype.DefaultIfEmpty()
                            join c in db.PropertyMains on a.Property equals c.Id into doc
                            from c in doc.DefaultIfEmpty()
                            where
                            (FromDate == "" || EF.Functions.DateDiffDay(a.CreatedDate, fdate) <= 0) &&
                              (ToDate == "" || EF.Functions.DateDiffDay(a.CreatedDate, tdate) >= 0) &&
                            (Property==0||Property==null||a.Property==Property)
                            && (UnitType == 0 || UnitType==null || a.UnitType==UnitType)
                            && (Unit == 0 || Unit==null || a.Id == Unit)
                            select new
                            {
                                id = a.Id,
                                a.Name,
                                a.TnC,
                                Property = c.Name,
                                a.Description,
                                UnitType=b.Name,
                                a.Code,
                            });
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
            {
                // Apply search   
                UserView = UserView.Where(p => p.Name.ToString().ToLower().Contains(search.ToLower()));
            }
            //SORT
            if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                try { UserView = UserView.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir); } catch { /* grid column name not in projection - keep default order */ }
            }
            recordsTotal = UserView.Count();
            var pageRows = UserView.Skip(skip).Take(pageSize).ToList();
            // attach Feature collection in memory (EF Core 10 can't translate the .ToList() projection once units have rows)
            var uids = pageRows.Select(r => r.id).ToList();
            var feats = db.SelectedUnitFeatures.Where(z => uids.Contains(z.Unit)).Select(z => new { z.Unit, z.Feature }).ToList();
            var data = pageRows.Select(r => new {
                r.id, r.Name, r.TnC, r.Property, r.Description, r.UnitType, r.Code,
                feature = feats.Where(f => f.Unit == r.id).Select(f => new { f.Feature }).ToList()
            }).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        public ActionResult Create(long? id)
        {
            ViewBag.Alldata = QkSelect.List(
              new List<SelectListItem>
              {
                  new SelectListItem { Selected = true, Text = "", Value = "0"},
              }, "Value", "Text", 0);
            ViewBag.PropFeat = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                                }, "Value", "Text", 0);
            var Pop = db.PropertyMains
                    .Select(s => new
                    {
                        ID = s.Id,
                        Name = s.Name
                    })
                    .ToList();
            ViewBag.Prop = QkSelect.List(Pop, "ID", "Name");

            var PopUnit = db.PropertyUnitTypes
                    .Select(s => new
                    {
                        ID = s.ID,
                        Name = s.Name
                    })
                    .ToList();
            ViewBag.unittype = QkSelect.List(PopUnit, "ID", "Name");
            
            var doc = db.DocumentTypes
                    .Select(s => new
                    {
                        ID = s.ID,
                        Name = s.Name
                    })
                    .ToList();
            ViewBag.doctype = QkSelect.List(doc, "ID", "Name");
            ViewBag.LastEntry = db.PropertyUnits.Select(p => p.Id).AsEnumerable().DefaultIfEmpty(0).Max();
            PropertyUnitViewModel vmodel = new PropertyUnitViewModel();
            vmodel.Code = CustCode();
            if (id != 0)
            {
                vmodel.Property = id;
                //var Property=db.PropertyMains.Where(x=>x.Id==id).Select()
            }
            var tag = db.PropertyUnitFeatures
                 .Select(s => new
                 {
                     ID = s.Feature,
                     Name = s.Feature
                 }).Distinct().ToList();
            ViewBag.TagFeature = QkSelect.List(tag, "ID", "Name");
            vmodel.Section = "Unit";
            return View(vmodel);
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, Create Property")]
        public ActionResult Create(PropertyUnitViewModel vmodel)
        {
            bool stat = false;
            string msg;

            var Exists = db.PropertyUnits.Any(c => c.Name == vmodel.Name);
            if (Exists)
            {
                msg = "Property Name already exists.";
                stat = false;
            }
            else
            {
                var UserId = User.Identity.GetUserId();
                var today = Convert.ToDateTime(System.DateTime.Now);
                var Entry = db.PropertyUnits.Select(x => x.EntryNo).AsEnumerable().DefaultIfEmpty(0).Max();
                var Phead = new PropertyUnit
                {
                    Name = vmodel.Name,
                    Code = vmodel.Code,
                    UnitType = vmodel.UnitType,
                    Description = vmodel.Description,
                    File = vmodel.File,
                    Document = vmodel.Document,
                    Property = vmodel.Property,
                   
                    Rent = vmodel.Rent,
                    Deposit = vmodel.Deposit,
                    UnitUsage=vmodel.UnitUsage,
                    Area=vmodel.Area,
                     PremisesNo =vmodel.PremisesNo,
                      NoofRooms =vmodel.NoofRooms,
                    CreatedBy = UserId,
                    CreatedDate = today,

                    EntryNo = Entry + 1,
                };
                db.PropertyUnits.Add(Phead);
                db.SaveChanges();
                Int64 ID = Phead.Id;

                if (vmodel.Feature != null)
                {
                    SelectedUnitFeature SF = new SelectedUnitFeature();
                    foreach (var arr in vmodel.Feature)
                    {
                        SF.Unit = ID;
                        SF.Feature = arr;
                        db.SelectedUnitFeatures.Add(SF);
                        db.SaveChanges();
                    }
                }

                //ItemViewModel It = new ItemViewModel();
                //It.ItemImage = vmodel.ItemImage;
                //It.ItemDocument = vmodel.ItemDocument;

                if (vmodel.ItemImage != null)
                {
                    var itimage = com.PropUnitImages(vmodel, ID);
                }
                if (vmodel.docmodel != null)
                {                    
                    foreach (var arr in vmodel.docmodel)
                    {
                        if (arr.Type!="")
                        {
                            PropertyDocumentType doc = new PropertyDocumentType();
                            doc.DocumentType = Convert.ToInt64(arr.Type);
                            doc.Purpose = "PropertyUnit";
                            doc.Reference = ID;
                            if(arr.Date!=null)
                            doc.ExpDate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));
                            db.PropertyDocumentTypes.Add(doc);
                            db.SaveChanges();
                            Int64 docid = doc.ID;

                            if (arr.Attachments != null)
                            {
                                if (arr.Attachments != null)
                                {
                                    string storePath = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "PropertyUnit_" + docid);
                                    if (!Directory.Exists(storePath))
                                        Directory.CreateDirectory(storePath);

                                    // files upload
                                    IFormFile file = Request.Form.Files[0];
                                    var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                                    var uploadUrl = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "PropertyUnit_" + docid + "/");
                                    file.SaveAs(Path.Combine(uploadUrl, fileNames));

                                    DocumentFile docfile = new DocumentFile();
                                    docfile.attachments = fileNames;
                                    docfile.Document = docid;
                                    db.DocumentFiles.Add(docfile);
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                }

                
                com.addlog(LogTypes.Created, UserId, "Unit", "Unit", findip(), ID, "Unit Added Successfully");
                msg = "Successfully added Unit details.";
                stat = true;
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PropertyUnit PM = db.PropertyUnits.Find(id);

            if (PM == null)
            {
                return NotFound();
            }

            PropertyUnitViewModel vmodel = new PropertyUnitViewModel();
            var PropertyFeat = db.PropertyUnitFeatures
                   .Select(s => new
                   {
                       ID = s.ID,
                       Name = s.Feature
                   })
                   .ToList();
            ViewBag.PropFeat = QkSelect.List(PropertyFeat, "ID", "Name");

            var propuni = db.PropertyUnitTypes
                    .Select(s => new
                    {
                        ID = s.ID,
                        Name = s.ID+" - "+s.Name
                    })
                    .ToList();
            ViewBag.proptype = QkSelect.List(propuni, "ID", "Name");

            var doc = db.DocumentTypes
                    .Select(s => new
                    {
                        ID = s.ID,
                        Name = s.Name
                    })
                    .ToList();
            ViewBag.doctype = QkSelect.List(doc, "ID", "Name");

            ViewBag.Alldata = QkSelect.List(
             new List<SelectListItem>
             {
                  new SelectListItem { Selected = true, Text = "All", Value = "0"},
             }, "Value", "Text", 0);
            //var viewModel = new PropertyViewModel
            //{
            vmodel.Id = PM.Id;
            vmodel.Name = PM.Name;
            vmodel.Code = PM.Code;
            vmodel.Description = PM.Description;
            vmodel.Property = PM.Property;


            vmodel.UnitUsage = PM.UnitUsage;
            vmodel.Area = PM.Area;
            vmodel.PremisesNo = PM.PremisesNo;
            vmodel.NoofRooms = PM.NoofRooms;
            vmodel.TnC = PM.TnC;
            vmodel.Rent = PM.Rent;
            vmodel.Deposit = PM.Deposit;
            vmodel.File = PM.File;
            vmodel.Document = PM.Document;
            vmodel.UnitType = PM.UnitType;
            vmodel.Section = "Unit";
            //vmodel.Features=

            var ImageBag = (from b in db.PropertyUnitImages
                            where b.UnitID == id
                            select new
                            {
                                ImgId = b.ID,
                                FileName = b.FileName,
                                ItemImId = b.UnitID
                            }).FirstOrDefault();
            if (ImageBag != null)
            {
                vmodel.ImageName = ImageBag.FileName;
                vmodel.ImageId = ImageBag.ImgId;
                vmodel.ItmImageId = ImageBag.ItemImId;
            }

            var property = db.PropertyMains
                   .Select(s => new
                   {
                       ID = s.Id,
                       Name = s.Id + " - " + s.Name
                   })
                   .ToList();
            ViewBag.prop = QkSelect.List(property, "ID", "Name");

            var tag1 = db.SelectedUnitFeatures.Where(x=>x.Unit==id)
                 .Select(s => new
                 {
                     ID = s.Feature,
                     Name = s.Feature
                 }).Distinct().ToList();
            var tag2 = db.PropertyUnitFeatures
                 .Select(s => new
                 {
                     ID = s.Feature,
                     Name = s.Feature
                 }).Distinct().ToList();
            var tag = tag1.Union(tag2);
            ViewBag.TagFeature = QkSelect.List(tag, "ID", "Name");
            ViewBag.preEntry = db.PropertyUnits.Where(a => a.Id < id).Select(a => a.Id).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.PropertyUnits.Where(a => a.Id > id).Select(a => a.Id).DefaultIfEmpty().Min();
            return View(vmodel);
        }

        public ActionResult Update(PropertyUnitViewModel vmodel)
        {
            bool stat = false;
            string msg;

            var Exists = db.PropertyUnits.Any(c => c.Name == vmodel.Name && vmodel.Id != c.Id);
            if (Exists)
            {
                msg = "Unit Name already exists.";
                stat = false;
            }
            else
            {
                PropertyUnit protyp = db.PropertyUnits.Find(vmodel.Id);

                protyp.Name = vmodel.Name;

                protyp.Code = vmodel.Code;
                protyp.Description = vmodel.Description;
                protyp.Rent = vmodel.Rent;
                protyp.Property = vmodel.Property;
                protyp.TnC = vmodel.TnC;
                protyp.UnitType = vmodel.UnitType;
                protyp.Deposit = vmodel.Deposit;
                protyp.File = vmodel.File;
                protyp.Document = vmodel.Document;

                db.Entry(protyp).State = EntityState.Modified;
                db.SaveChanges();

                Int64 ID = vmodel.Id;
                db.SelectedUnitFeatures.RemoveRange(db.SelectedUnitFeatures.Where(a => a.Unit == ID));

                if (vmodel.Feature != null)
                {
                    SelectedUnitFeature SF = new SelectedUnitFeature();
                    foreach (var arr in vmodel.Feature)
                    {
                        SF.Unit = ID;
                        SF.Feature = arr;
                        db.SelectedUnitFeatures.Add(SF);
                        db.SaveChanges();
                    }
                }

                if (vmodel.ItemImage.ToList().First() != null)
                {
                    var itimage = com.PropUnitImages(vmodel, ID);
                }




                var count = 0;
                long docid = 0;

                foreach (var arr in vmodel.docmodel)
                {

                    PropertyDocumentType doc = new PropertyDocumentType();

                    if (Convert.ToInt32(arr.ID) == 0 && arr.Type != null)
                    {




                        doc.DocumentType = Convert.ToInt64(arr.Type);
                        doc.Purpose = "PropertyUnit";
                        doc.Reference = vmodel.Id;
                        if(arr.Date!=null)
                        doc.ExpDate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));


                        db.PropertyDocumentTypes.Add(doc);
                        db.SaveChanges();
                        docid = doc.ID;

                        string storePath = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "PropertyUnit_" + docid);
                        if (!Directory.Exists(storePath))
                            Directory.CreateDirectory(storePath);



                        IFormFile file = Request.Form.Files["docmodel[" + count + "].Attachments"];

                        if (file.FileName != "")
                        {
                            var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                            var uploadUrl = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "PropertyUnit_" + docid + "/");
                            file.SaveAs(Path.Combine(uploadUrl, fileNames));

                            DocumentFile docfile = new DocumentFile();
                            docfile.attachments = fileNames;
                            docfile.Document = docid;
                            db.DocumentFiles.Add(docfile);
                            db.SaveChanges();
                        }




                    }
                    else if (arr.Type != null)
                    {
                        doc = db.PropertyDocumentTypes.Find(arr.ID);
                        doc.DocumentType = Convert.ToInt64(arr.Type);
                        doc.Purpose = "PropertyUnit";
                        doc.Reference = vmodel.Id;
                        doc.ExpDate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));

                        db.Entry(doc).State = EntityState.Modified;
                        db.SaveChanges();
                        docid = arr.ID;

                        string storePath = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "PropertyUnit_" + docid);
                        if (!Directory.Exists(storePath))
                            Directory.CreateDirectory(storePath);



                        IFormFile file = Request.Form.Files["docmodel[" + count + "].Attachments"];

                        if (file.FileName != "")
                        {
                            var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                            var uploadUrl = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "PropertyUnit_" + docid + "/");
                            file.SaveAs(Path.Combine(uploadUrl, fileNames));

                            DocumentFile docfile = new DocumentFile();
                            docfile.attachments = fileNames;
                            docfile.Document = docid;
                            db.DocumentFiles.Add(docfile);
                            db.SaveChanges();
                        }

                    }





                    count++;

                }









             

                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Created, UserId, "Unit", "Unit", findip(), ID, "Unit Updated Successfully");
                msg = "Successfully Updated Unit details.";
                stat = true;
            }



            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        [HttpPost]
        //[QkAuthorize(Roles = "Dev,Delete Broker")]
        public ActionResult DeleteAll(long[] bill)
        {
            Int32 count = 0;
            Int32 notdel = 0;
            foreach (var arr in bill)
            {
                var chk = (DeleteCust(arr) == true) ? count++ : notdel++;
            }
            if (count > 0 && notdel > 0)
            {
                Success("Deleted " + count + " Units, Unable to Delete " + notdel + " Units. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Units.", true);
            }
            else
            {
                Success("Deleted " + count + " Units.", true);
            }
            return RedirectToAction("Index", "Unit");
        }
        private Boolean DeleteCust(long custid)
        {
            var Msg = chkDeleteWithMsg(custid);
            if (Msg != null)
            {
                return false;
            }
            else
            {
                return DeleteFn(custid);
            }

        }
        [RedirectingAction]
        //[Authorize(Roles = "Dev,Delete DocumentType")]
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PropertyUnit ptype = db.PropertyUnits.Find(id);
            if (ptype == null)
            {
                return NotFound();
            }
            return PartialView(ptype);
        }
        [RedirectingAction]
        //[Authorize(Roles = "Dev,Delete DocumentType")]
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
                msg = "Successfully Deleted Unit details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public bool DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            PropertyUnit pt = db.PropertyUnits.Find(id);

            db.PropertyUnits.Remove(pt);
            db.SaveChanges();

            com.addlog(LogTypes.Deleted, UserId, "PropertyUnit", "PropertyUnit", findip(), pt.Id, "Property Unit Deleted Successfully");
            return true;
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            if (db.RentalProformas.Any(c => c.Unit == id))
            {
                msg = "Unit Already used in Rental Proformas !!";
            }
            else if (db.Rentals.Any(c => c.Unit == id))
            {
                msg = "Unit Already used in Rental Proformas !!";
            }
            else if (db.TenancyContracts.Any(c => c.Unit == id))
            {
                msg = "Unit Already used in Tenancy Contracts !!";
            }
            else
            {
                msg = null;
            }
            return msg;
        }
        public ActionResult Details(long? id)
        {
            PropertyUnitViewModel vmodel = new PropertyUnitViewModel();

            vmodel = (from a in db.PropertyUnits
                      join b in db.PropertyUnitTypes on a.UnitType equals b.ID into cat
                      from b in cat.DefaultIfEmpty()
                      join c in db.PropertyMains on a.Property equals c.Id into brand
                      from c in brand.DefaultIfEmpty()
                      where a.Id == id
                      select new PropertyUnitViewModel
                      {
                          Id = a.Id,
                          Code = a.Code,
                          Name = a.Name,
                          Description = a.Description,
                          Rent=a.Rent,
                          Deposit=a.Deposit,
                          TnC=a.TnC,
                          Unitname = b.Name,
                          Propertyname=c.Name,
                      }).FirstOrDefault();
            return View(vmodel);
        }

        [HttpGet]
        public JsonResult GetAllFeatures(long? Pid)
        {
            var teamss = (from z in db.SelectedUnitFeatures
                          join x in db.PropertyUnits on z.Unit equals x.Id
                          where x.Id == Pid
                          select new
                          {
                              ID=z.Feature,

                              //lead = a.TeamLead,
                          }).ToList();
            return Json(teamss);
        }

        public JsonResult SearchUnit(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.PropertyUnits
                                  where a.Name.ToLower().Contains(q.ToLower()) || a.Name.Contains(q)
                                  select new SelectFormat
                                  {
                                      text = a.Name,
                                      id = a.Id
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.PropertyUnits.Select(b => new SelectFormat
                {
                    text = b.Name,
                    id = b.Id
                }).OrderBy(b => b.text).ToList();

            }//
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Unit" };
                serialisedJson.Insert(0, initial);
            }
            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "All" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }

        public ActionResult AddUnit()
        {
            ViewBag.PropFeat = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                                 }, "Value", "Text", 0);


            var PopUnit = db.PropertyUnitTypes
                    .Select(s => new
                    {
                        ID = s.ID,
                        Name = s.Name
                    })
                    .ToList();
            ViewBag.unittype = QkSelect.List(PopUnit, "ID", "Name");
            var prop = db.PropertyMains
                    .Select(s => new
                    {
                        ID = s.Id,
                        Name = s.Name
                    })
                    .ToList();
            ViewBag.prop = QkSelect.List(prop, "ID", "Name");
            var doc = db.DocumentTypes
                    .Select(s => new
                    {
                        ID = s.ID,
                        Name = s.Name
                    })
                    .ToList();
            ViewBag.doctype = QkSelect.List(doc, "ID", "Name");
        
            return PartialView();
        }
        [HttpGet]
        //[QkAuthorize(Roles = "Dev,View Landlord")]
        public ActionResult Details(int? id)
        {
            ViewBag.Prjct = QkSelect.List(
                               new List<SelectListItem>
                               {
                                    new SelectListItem { Selected = true, Text = "All", Value = null},
                               }, "Value", "Text", 1);
            PropertyUnitViewModel cusmodel = new PropertyUnitViewModel();
            cusmodel = (from a in db.PropertyUnits
                        join b in db.PropertyUnitTypes on a.UnitType equals b.ID into tmp
                        from b in tmp.DefaultIfEmpty()
                        join c in db.PropertyMains on a.Property equals c.Id into pro
                        from c in pro.DefaultIfEmpty()
                        where a.Id == id
                        select new PropertyUnitViewModel
                        {
                            Id = a.Id,
                            Name = a.Name,
                            Code = a.Code,
                            Rent = a.Rent,
                            Deposit=a.Deposit,
                            TnC=a.TnC,
                            Propertyname=c.Name,
                            Unitname=b.Name,


                            
                            //PropFeature = (from ac in db.SelectedFeatures
                            //                join ab in db.PropertyFeatures on ac.Feature equals ab.ID
                            //                where (ac.Property == a.Id)
                            //                select new PropertyFeature
                            //                {
                            //                    Feature= ab.Feature
                            //                }).ToList(),
                            Description = a.Description,
                        }).FirstOrDefault();
            return View(cusmodel);

        }

        private long GetEntryNo()
        {
            Int64 ENo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "PropertyUnit").Select(a => a.number).FirstOrDefault();
            if ((db.PropertyUnits.Select(p => p.EntryNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
            {
                if (number == 0)
                {
                    ENo = 1;
                }
                else
                {
                    ENo = number;
                }
            }
            else
            {
                ENo = db.PropertyUnits.Max(p => p.EntryNo + 1);
            }
            return ENo;
        }

        private string CustCode(Int64 CNo = 0, string CCode = null)
        {
            var prefix = db.CodePrefixs.Where(a => a.section == "Property").Select(a => a.prefix).FirstOrDefault();

            if (CCode == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "Property").Select(a => a.number).FirstOrDefault();
                if ((db.PropertyUnits.Select(p => p.EntryNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
                {
                    if (number == 0)
                    {
                        CCode = prefix + 1;
                    }
                    else
                    {
                        CCode = prefix + number;
                    }
                }
                else
                {
                    CNo = db.PropertyUnits.Max(p => p.EntryNo + 1);
                    CCode = prefix + CNo;
                    if (CodeExist(CCode))
                    {
                        CCode = CustCode(CNo, CCode);
                    }
                }
            }
            else
            {
                CNo = CNo + 1;
                CCode = prefix + CNo;
                if (CodeExist(CCode))
                {
                    CCode = CustCode(CNo, CCode);
                }
            }
            return CCode;
        }
        private bool CodeExist(string Code)
        {
            var Exists = db.PropertyUnits.Any(c => c.Code == Code);
            bool res = (Exists) ? true : false;
            return res;
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