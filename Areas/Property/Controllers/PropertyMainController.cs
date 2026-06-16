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

namespace QuickSoft.Areas.Property.Controllers
{
    [Microsoft.AspNetCore.Mvc.Area("Property")]
    public class PropertyMainController : BaseController
    {
        ApplicationDbContext db;
        Common com;
        public PropertyMainController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
        // GET: Property/Property
        public ActionResult Index()
        {
            //landlord
            var OpAll = QkSelect.List(
                                new List<SelectListItem>
                                {
                                    new SelectListItem { Selected = true, Text = "All", Value = "0"},
                                }, "Value", "Text", 1);

            ViewBag.Cust = OpAll;
            //end
            ViewBag.Alldata = QkSelect.List(
              new List<SelectListItem>
              {
                  new SelectListItem { Selected = true, Text = "All", Value = "0"},
              }, "Value", "Text", 0);
            return View();
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, DocumentType")]
        public ActionResult GetProperty(long? Property, long? DocumentType,long? Landlord, long? Feature, long? PropertyType, string FromDate, string ToDate)
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
            var UserView = (from a in db.PropertyMains
                            join b in db.PropertyTypes on a.PropertyType equals b.ID into protype
                            from b in protype.DefaultIfEmpty()
                            join c in db.DocumentTypes on a.DocumentType equals c.ID into doc
                            from c in doc.DefaultIfEmpty()
                            join d in db.Landlords on a.LandlordID equals d.LandlordID into land
                            from d in land.DefaultIfEmpty()
                                //join d in db.SelectedFeatures on a.Id equals d.Property into selfeat
                                //from d in selfeat.DefaultIfEmpty()
                                //join e in db.PropertyFeatures on d.Feature equals e.ID into feat
                                //from e in feat.DefaultIfEmpty()
                            where
                              (FromDate == "" || EF.Functions.DateDiffDay(a.CreatedDate, fdate) <= 0) &&
                              (ToDate == "" || EF.Functions.DateDiffDay(a.CreatedDate, tdate) >= 0) &&
                             //(Feature == 0 || d.Feature == Feature) &&
                             (Property == 0 || Property == null || a.Id == Property)
                             && (Landlord == 0 || Landlord == null || a.LandlordID == Landlord)&&
                             (DocumentType == 0 || DocumentType == null || a.DocumentType == DocumentType)
                             && (PropertyType == 0 || PropertyType == null || a.PropertyType == PropertyType)
                            select new
                            {
                                id = a.Id,
                                a.Name,
                                Property = b.Name,
                                d.LandlordName,
                                a.Remark,
                                a.Description,
                                a.Code,
                                Date=a.CreatedDate,
                                DocumentType = db.DocumentTypes.Where(x => x.ID == a.DocumentType).Select(y => y.Name).FirstOrDefault(),//c.Name,
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
            // EF Core 10 cannot translate the .ToList() Feature collection-projection inside the server query
            // (it only errored once PropertyMains had rows). Attach Features in memory instead.
            var pids = pageRows.Select(r => r.id).ToList();
            var feats = db.SelectedFeatures.Where(z => pids.Contains(z.Property)).Select(z => new { z.Property, z.Feature }).ToList();
            var data = pageRows.Select(r => new {
                r.id, r.Name, r.Property, r.LandlordName, r.Remark, r.Description, r.Code, r.Date, r.DocumentType,
                feature = feats.Where(f => f.Property == r.id).Select(f => new { f.Feature }).ToList()
            }).ToList();
            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

        }
        public ActionResult Create()
        {
            //var PropertyFeat = db.PropertyFeatures
            //        .Select(s => new
            //        {
            //            ID = s.ID,
            //            Name = s.Feature
            //        })
            //        .ToList();
            //ViewBag.PropFeat = QkSelect.List(PropertyFeat, "ID", "Name");
            ViewBag.PropFeat = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                                 }, "Value", "Text", 0);
            var OpAll = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                                 }, "Value", "Text", 0);
            ViewBag.Cust = OpAll;
            var doc = db.DocumentTypes
                    .Select(s => new
                    {
                        ID = s.ID,
                        Name = s.Name
                    })
                    .ToList();
            ViewBag.doctype = QkSelect.List(doc, "ID", "Name");
            var prop = db.PropertyTypes
                    .Select(s => new
                    {
                        ID = s.ID,
                        Name = s.Name
                    })
                    .ToList();
            ViewBag.proptype = QkSelect.List(prop, "ID", "Name");
            var viewModel = new PropertyViewModel
            {

                AdditionalField = db.AdditionalFields.Where(x=>x.Section=="Property").ToList()
            };
            var tag = db.PropertyFeatures
                 .Select(s => new
                 {
                     ID = s.Feature,
                     Name = s.Feature
                 }).Distinct().ToList();
            ViewBag.TagFeature = QkSelect.List(tag, "ID", "Name");
            ViewBag.LastEntry = db.PropertyMains.Select(p => p.Id).AsEnumerable().DefaultIfEmpty(0).Max();
            PropertyViewModel vmodel = new PropertyViewModel();
            viewModel.Code = CustCode();
            viewModel.Section = "Property";
            return View(viewModel);
        }
        [HttpPost]
        //[QkAuthorize(Roles = "Dev, Create Property")]
        public ActionResult Create(PropertyViewModel vmodel)
        {
            bool stat = false;
            string msg;

            var Exists = db.PropertyMains.Any(c => c.Name == vmodel.Name);
            if (Exists)
            {
                msg = "Property Name already exists.";
                stat = false;
            }
            else
            {


                var UserId = User.Identity.GetUserId();
                Accounts account = new Accounts();
                account.Name = vmodel.Name;
                account.Alias = vmodel.Code;
                account.PrintName = vmodel.Name;

                account.Status = Status.active;
                account.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
                account.CreatedBy = UserId;
              

              
                    account.OpnBalance =0;
                    account.OpnBalanceCr = 0;
              
            

                account.Group = 4;
                db.Accountss.Add(account);
                db.SaveChanges();
                long accountId = account.AccountsID;

               
                var today = Convert.ToDateTime(System.DateTime.Now);
                var Entry = db.PropertyMains.Select(c => c.EntryNo).AsEnumerable().DefaultIfEmpty(0).Max();
                var Phead = new PropertyMain
                {
                    Name = vmodel.Name,
                    Code = vmodel.Code,
                     Municipality=vmodel.Municipality,
        Zone =vmodel.Zone,
        Sector =vmodel.Sector,
        RoadName =vmodel.RoadName,
        PlotNo =vmodel.PlotNo,
        PlotAddress=vmodel.PlotAddress,

        PropertyRegistrationNo=vmodel.PropertyRegistrationNo,
        PropertyType = vmodel.PropertyType,
                    Description = vmodel.Description,
                    Remark = vmodel.Remark,
                    State = vmodel.State,
                    City = vmodel.City,
                    Zip = vmodel.Zip,
                    Address = vmodel.Address,
                    Country = vmodel.Country,
                    DocumentType = vmodel.DocumentType,
                    File = vmodel.File,
                    Document = vmodel.Document,
                    CreatedBy = UserId,
                    CreatedDate = today,
                   LandlordID=vmodel.ddlLandlord,
                    //Feature=vmodel.Feature,
                    EntryNo = Entry + 1
                };
                db.PropertyMains.Add(Phead);
                db.SaveChanges(); 
                Int64 ID = Phead.Id;

                if (vmodel.Feature != null)
                {
                    SelectedFeature SF = new SelectedFeature();
                    foreach (var arr in vmodel.Feature)
                    {
                        SF.Property = ID;
                        SF.Feature = arr;
                        db.SelectedFeatures.Add(SF);
                        db.SaveChanges();
                    }
                }

                //ItemViewModel It = new ItemViewModel();
                //It.ItemImage = vmodel.ItemImage;
                //It.ItemDocument = vmodel.ItemDocument;

                if (vmodel.ItemImage != null && vmodel.ItemImage.ToList().First() != null)
                {
                    var itimage = com.PropImages(vmodel, ID);
                }
                if (vmodel.docmodel != null)
                {
                    int count = 0;
                    foreach (var arr in vmodel.docmodel)
                    {
                        if (arr.Type!=null)
                        {
                            PropertyDocumentType doc = new PropertyDocumentType();
                            doc.DocumentType = Convert.ToInt64(arr.Type);
                            doc.Reference = ID;
                            doc.Purpose = "Property";
                            //if (arr.Date == null)
                            //    arr.Date = "01-01-3000";
                            if (arr.Date != null)
                               doc.ExpDate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));
                            db.PropertyDocumentTypes.Add(doc);
                            db.SaveChanges();
                            Int64 docid = doc.ID;

                            if (arr.Attachments != null)
                            {
                                if (arr.Attachments != null)
                                {
                                    string storePath = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "Property_" + docid);
                                    if (!Directory.Exists(storePath))
                                        Directory.CreateDirectory(storePath);

                                    // files upload
                                    IFormFile file = Request.Form.Files["docmodel[" + count + "].Attachments"];
                                    var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                                    var uploadUrl = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "Property_" + docid + "/");
                                    file.SaveAs(Path.Combine(uploadUrl, fileNames));

                                    DocumentFile docfile = new DocumentFile();
                                    docfile.attachments = fileNames;
                                    docfile.Document = docid;
                                    db.DocumentFiles.Add(docfile);
                                    db.SaveChanges();
                                }
                            }
                        }
                        count++;
                    }
                }
                if (vmodel.AdditionalField != null)
                {
                    foreach (AdditionalField Ad in vmodel.AdditionalField)
                    {
                        var rate = new AdditionalFieldData
                        {
                            Reference = ID,
                            Name = Ad.Name,
                            Purpose = "Property",
                            Field=Ad.ID
                        };
                        db.AdditionalFieldDatas.Add(rate);
                        db.SaveChanges();
                    }
                }



                com.addlog(LogTypes.Created, UserId, "Property", "Property", findip(), ID, "Property Type Added Successfully");
                msg = "Successfully added Property details.";
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
            PropertyMain PM = db.PropertyMains.Find(id);

            if (PM == null)
            {
                return NotFound();
            }

            PropertyViewModel vmodel = new PropertyViewModel();
            var PropertyFeat = db.PropertyFeatures
                   .Select(s => new
                   {
                       ID = s.ID,
                       Name = s.Feature
                   })
                   .ToList();
            ViewBag.PropFeat = QkSelect.List(PropertyFeat, "ID", "Name");
            var landlords = db.Landlords
                .Select(s => new
                {
                   ID=s.LandlordID,
                   Name=s.LandlordName,
                }).ToList();
            ViewBag.Cust = QkSelect.List(landlords, "ID", "Name");
            var doc = db.DocumentTypes
                    .Select(s => new
                    {
                        ID = s.ID,
                        Name = s.Name
                    })
                    .ToList();
            ViewBag.doctype = QkSelect.List(doc, "ID", "Name");
            var prop = db.PropertyTypes
                    .Select(s => new
                    {
                        ID = s.ID,
                        Name = s.Name
                    })
                    .ToList();
            ViewBag.proptype = QkSelect.List(prop, "ID", "Name");

            var tag = db.PropertyFeatures
                 .Select(s => new
                 {
                     ID = s.Feature,
                     Name = s.Feature
                 }).Distinct().ToList();
            ViewBag.TagFeature = QkSelect.List(tag, "ID", "Name");
            //var viewModel = new PropertyViewModel
            //{
            vmodel.AdditionalField = db.AdditionalFields.Where(x => x.Section == "Miantanance").ToList();
            vmodel.AdditionalFieldVieModels = (from b in db.AdditionalFieldDatas
                                               where b.Reference == id && b.Purpose == "Property"
                                               select new AdditionalFieldVieModel
                                               {
                                                   ID = b.ID,
                                                   Entrydata = b.Name,
                                                   Field=b.Field,
                                                   Name=b.Name
                                               }).ToList();
            vmodel.Id = PM.Id;
            vmodel.Name = PM.Name;
            vmodel.Code = PM.Code;
            vmodel.City = PM.City;
            vmodel.Municipality = PM.Municipality;
            vmodel.Zone = PM.Zone;
            vmodel.Sector = PM.Sector;
            vmodel.RoadName = PM.RoadName;
            vmodel.PlotNo = PM.PlotNo;
            vmodel.PlotAddress = PM.PlotAddress;

            vmodel.PropertyRegistrationNo = PM.PropertyRegistrationNo;
            vmodel.PropertyType = PM.PropertyType;
            vmodel.Description = PM.Description;
            vmodel.Remark = PM.Remark;
            vmodel.State = PM.State;
            vmodel.City = PM.City;
            vmodel.Zip = PM.Zip;
            vmodel.Address = PM.Address;
            vmodel.Country = PM.Country;
            vmodel.DocumentType = PM.DocumentType;
            vmodel.File = PM.File;
            vmodel.Document = PM.Document;
            vmodel.Section = "Property";
            vmodel.ddlLandlord = PM.LandlordID;
            var ImageBag = (from b in db.PropertyImages
                            where b.PropertyID == id
                            select new
                            {
                                ImgId = b.ID,
                                FileName = b.FileName,
                                ItemImId = b.PropertyID
                            }).FirstOrDefault();
            if (ImageBag != null)
            {
                vmodel.ImageName = ImageBag.FileName;
                vmodel.ImageId = ImageBag.ImgId;
                vmodel.ItmImageId = ImageBag.ItemImId;
            }

            var DocBag = (from b in db.PropertyDocuments
                          where b.PropertyID == id
                          select new
                          {
                              DocId = b.ID,
                              FileName = b.FileName,
                              ItemDoId = b.PropertyID,
                          }).FirstOrDefault();
            if (DocBag != null)
            {
                vmodel.DocId = DocBag.DocId;
                vmodel.DocName = DocBag.FileName;
                vmodel.ItmDocId = DocBag.ItemDoId;


            }

            ViewBag.preEntry = db.PropertyMains.Where(a => a.Id < id).Select(a => a.Id).DefaultIfEmpty().Max();
            ViewBag.nxtEntry = db.PropertyMains.Where(a => a.Id > id).Select(a => a.Id).DefaultIfEmpty().Min();
            return View(vmodel);
        }

        public ActionResult Update(PropertyViewModel vmodel)
        {
            bool stat = false;
            string msg;

            var Exists = db.PropertyMains.Any(c => c.Name == vmodel.Name && vmodel.Id != c.Id);
            if (Exists)
            {
                msg = "Property Name already exists.";
                stat = false;
            }
            else
            {
                PropertyMain protyp = db.PropertyMains.Find(vmodel.Id);

                protyp.Name = vmodel.Name;
                protyp.Code = vmodel.Code;
                protyp.PropertyType = vmodel.PropertyType;
                protyp.Description = vmodel.Description;
                protyp.Remark = vmodel.Remark;
                protyp.State = vmodel.State;
                protyp.City = vmodel.City;
                protyp.Zip = vmodel.Zip;
                protyp.Address = vmodel.Address;
                protyp.Country = vmodel.Country;
                protyp.DocumentType = vmodel.DocumentType;
                protyp.File = vmodel.File;
                protyp.Document = vmodel.Document;
                protyp.LandlordID = vmodel.ddlLandlord;
                protyp.Municipality = vmodel.Municipality;
                protyp.Zone = vmodel.Zone;
                protyp.Sector = vmodel.Sector;
                protyp.RoadName = vmodel.RoadName;
                protyp.PlotNo = vmodel.PlotNo;
                protyp.PlotAddress = vmodel.PlotAddress;

                protyp.PropertyRegistrationNo = vmodel.PropertyRegistrationNo;
                db.Entry(protyp).State = EntityState.Modified;
                db.SaveChanges();

                Int64 ID = vmodel.Id;


                if (vmodel.ItemImage != null && vmodel.ItemImage.ToList().First() != null)
                {
                    var itimage = com.PropImages(vmodel, ID);
                }

                var count = 0;
                long docid = 0;

                foreach (var arr in vmodel.docmodel)
                {

                    PropertyDocumentType doc = new PropertyDocumentType();

                    if (Convert.ToInt32(arr.ID) == 0 && arr.Type != null)
                    {



                        doc.DocumentType = Convert.ToInt64(arr.Type);
                        doc.Purpose = "Property";
                        doc.Reference = vmodel.Id;
                        if(arr.Date!=null)
                        doc.ExpDate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));


                        db.PropertyDocumentTypes.Add(doc);
                        db.SaveChanges();
                        docid = doc.ID;

                        string storePath = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "Property_" + docid);
                        if (!Directory.Exists(storePath))
                            Directory.CreateDirectory(storePath);



                        IFormFile file = Request.Form.Files["docmodel[" + count + "].Attachments"];

                        if (file.FileName != "")
                        {
                            var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                            var uploadUrl = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "Property_" + docid + "/");
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
                        doc.Purpose = "Property";
                        doc.Reference = vmodel.Id;
                        if(arr.Date!=null)
                        doc.ExpDate = DateTime.Parse(arr.Date, new CultureInfo("en-GB"));

                        db.Entry(doc).State = EntityState.Modified;
                        db.SaveChanges();
                        docid = arr.ID;

                        string storePath = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "Property_" + docid);
                        if (!Directory.Exists(storePath))
                            Directory.CreateDirectory(storePath);



                        IFormFile file = Request.Form.Files["docmodel[" + count + "].Attachments"];

                        if (file.FileName != "")
                        {
                            var fileNames = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                            var uploadUrl = LegacyWeb.MapPath("~/uploads/TenancyContractDocument/" + "Property_" + docid + "/");
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


                if (vmodel.Feature != null)
                {
                    db.SelectedFeatures.RemoveRange(db.SelectedFeatures.Where(a => a.Property == ID));
                    SelectedFeature SF = new SelectedFeature();
                    foreach (var arr in vmodel.Feature)
                    {
                        SF.Property = ID;
                        SF.Feature = arr;
                        db.SelectedFeatures.Add(SF);
                        db.SaveChanges();
                    }
                }
                if (vmodel.AdditionalField != null)
                {
                    db.AdditionalFieldDatas.RemoveRange(db.AdditionalFieldDatas.Where(a => a.Reference == ID && a.Purpose == "Property"));
                    foreach (AdditionalField Ad in vmodel.AdditionalField)
                    {
                        var rate = new AdditionalFieldData
                        {
                            Reference = ID,
                            Name = Ad.Name,
                            Purpose = "Property",
                            Field=Ad.ID
                        };
                        db.AdditionalFieldDatas.Add(rate);
                        db.SaveChanges();
                    }
                }
                var UserId = User.Identity.GetUserId();
                com.addlog(LogTypes.Created, UserId, "Property", "Property", findip(), ID, "Property Type Updated Successfully");
                msg = "Successfully Updated Property details.";
                stat = true;
            }
            //return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
           return RedirectToAction("Index", "PropertyMain");
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
                Success("Deleted " + count + " Properties, Unable to Delete " + notdel + " Properties. ", true);
            }
            else if (notdel > 0)
            {
                Danger("Unable to Delete " + notdel + " Properties.", true);
            }
            else
            {
                Success("Deleted " + count + " Properties.", true);
            }
            return RedirectToAction("Index", "PropertyMain");
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
            PropertyMain ptype = db.PropertyMains.Find(id);
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
                msg = "Successfully Deleted Property details.";
            }
            return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
        }

        public bool DeleteFn(long id)
        {
            var UserId = User.Identity.GetUserId();
            db.PropertyDocumentTypes.RemoveRange(db.PropertyDocumentTypes.Where(a => a.Reference == id && a.Purpose == "Property"));
            db.SaveChanges();
            db.SelectedFeatures.RemoveRange(db.SelectedFeatures.Where(a => a.Property == id));
            db.SaveChanges();
            db.AdditionalFieldDatas.RemoveRange(db.AdditionalFieldDatas.Where(a => a.Reference == id && a.Purpose == "Property"));
            db.SaveChanges();

            PropertyMain pt = db.PropertyMains.Find(id);
            if (pt != null)
            {
                db.PropertyMains.Remove(pt);
                db.SaveChanges();
            }
            com.addlog(LogTypes.Deleted, UserId, "PropertyMain", "PropertyMains", findip(), pt.Id, "Property Deleted Successfully");
            return true;
        }
        public string chkDeleteWithMsg(long id)
        {
            string msg = null;
            if (db.PropertyRegistrations.Any(c => c.Property == id))
            {
                msg = "Property Already used in Property Registration !!";
            }
            else if (db.PropertyUnits.Any(c => c.Property == id))
            {
                msg = "Property Already used in Units !!";
            }
            else if (db.TenancyContracts.Any(c => c.Property == id))
            {
                msg = "Property Already used in Tenancy Contracts !!";
            }
            else if (db.Rentals.Any(c => c.Property == id))
            {
                msg = "Property Already used in Rentals !!";
            }
            else if (db.RentalProformas.Any(c => c.Property == id))
            {
                msg = "Property Already used in Rental Proformas !!";
            }
            else if (db.Maintenances.Any(c => c.Property == id))
            {
                msg = "Property Already used in Maintenances !!";
            }
            else
            {
                msg = null;
            }
            return msg;
        }
        [HttpGet]
        // [QkAuthorize(Roles = "Dev,View Property")]
        public ActionResult ViewDetails(long? id)
        {

            PropertyViewModel vmodel = new PropertyViewModel();

            vmodel = (from a in db.PropertyMains
                      join b in db.PropertyTypes on a.PropertyType equals b.ID into cat
                      from b in cat.DefaultIfEmpty()
                          //join c in db.ItemBrands on a.ItemBrandID equals c.ItemBrandID into brand
                          //from c in brand.DefaultIfEmpty()
                      where a.Id == id
                      select new PropertyViewModel
                      {
                          Id = a.Id,
                          Code = a.Code,
                          Name = a.Name,
                          Address = a.Address,
                          City = a.City,
                          //Featur = (from z in db.SelectedFeatures
                          //          join y in db.PropertyFeatures on z.Feature equals y.ID into pro
                          //          from y in pro.DefaultIfEmpty()
                          //          where z.Property == a.Id
                          //          select new HireType
                          //               {
                          //                   z.Feature,
                          //                   Name =y.Feature
                          //               }).ToList(),
                          Description = a.Description,
                          PropertyTypeName = b.Name,

                          Country = a.Country,

                          //HType = (from d in db.PropertyFeatures
                          //             join e in db.SelectedFeatures on d.ID equals e.Feature
                          //             where e.Property == a.Id
                          //             select new HireType
                          //             {
                          //                 Name = d.Feature,
                          //             }).ToList(),

                          AdditionalFieldVieModels = (from g in db.AdditionalFieldDatas
                                                      join h in db.AdditionalFields on g.ID equals h.ID
                                                      where g.Reference == a.Id && g.Purpose == "Property"
                                                      select new AdditionalFieldVieModel
                                                      {
                                                          Name = h.Name,
                                                          Entrydata = g.Name
                                                      }).ToList(),
                      }).FirstOrDefault();
            return View(vmodel);
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
            PropertyViewModel cusmodel = new PropertyViewModel();
            cusmodel = (from a in db.PropertyMains
                        join b in db.PropertyTypes on a.PropertyType equals b.ID into tmp
                        from b in tmp.DefaultIfEmpty()
                        where a.Id == id
                        select new PropertyViewModel
                        {
                            Id = a.Id,
                            Name = a.Name,
                            Code = a.Code,
                            Remark = a.Remark,

                            PropertyTypeName = b.Name,


                            Address = a.Address,
                            City = a.City,
                            State = a.State,
                            Country = a.Country,
                            Zip = a.Zip,
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

        [HttpGet]
        public JsonResult GetAllFeatures(long? Pid)
        {
            var teamss = (from z in db.SelectedFeatures
                          join x in db.PropertyMains on z.Property equals x.Id
                          where x.Id == Pid
                          select new
                          {
                              ID = z.Feature,
                              Name = z.Feature

                          }).ToList();
            return Json(teamss);
        }

        public JsonResult SearchProperty(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.PropertyMains
                                  join b in db.PropertyTypes on a.PropertyType equals b.ID into cat
                                  from b in cat.DefaultIfEmpty()
                                  join d in db.PropertyRegistrations on a.Id equals d.Property into protype
                                  from d in protype.DefaultIfEmpty()
                                  join c in db.Accountss on d.Owner equals c.AccountsID into doc
                                  from c in doc.DefaultIfEmpty()
                                  where a.Name.ToLower().Contains(q.ToLower()) || a.Name.Contains(q) || a.Code.ToLower().Contains(q.ToLower()) || a.Code.Contains(q)
                                  select new SelectFormat
                                  {
                                      text = a.Code + "-" + a.Name,
                                      id = a.Id,
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.PropertyMains.Select(a => new SelectFormat
                {
                    text = a.Code + "-" + a.Name,
                    id = a.Id
                }).OrderBy(b => b.text).ToList();
            }//
            return Json(serialisedJson);
        }
        public JsonResult SearchPropertyAll(string q, string x,long? owner)
        {
            List<SelectFormat> serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.PropertyMains
                                  where a.Name.ToLower().Contains(q.ToLower()) || a.Name.Contains(q) || a.Code.ToLower().Contains(q.ToLower()) || a.Code.Contains(q)
                                  select new SelectFormat
                                  {
                                      text = a.Code + "-" + a.Name,
                                      id = a.Id,
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = db.PropertyMains.Select(a => new SelectFormat
                {
                    text = a.Code + "-" + a.Name,
                    id = a.Id
                }).OrderBy(b => b.text).ToList();
            }//
            if (x == "empty" && (string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "Select Property" };
                serialisedJson.Insert(0, initial);
            }
            if (x == "All" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                var initial = new SelectFormat() { id = 0, text = "All" };
                serialisedJson.Insert(0, initial);
            }
            return Json(serialisedJson);
        }
        public JsonResult SearchPropertyWithOwner(string q, string x, string page)
        {
            var UserId = User.Identity.GetUserId();

            var start = Convert.ToInt32(page);
            int pageSize = 10;
            int skip = (start != null || start != 0) ? (pageSize * start) : 0;

            List<SelectMultiFormat> serialisedJson;
            string stt = "All";
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                var hmt = (from a in db.PropertyMains
                           join d in db.PropertyRegistrations on a.Id equals d.Property into protype
                           from d in protype.DefaultIfEmpty()
                           join c in db.Accountss on d.Owner equals c.AccountsID into doc
                           from c in doc.DefaultIfEmpty()
                           where a.Name.ToLower().Contains(q.ToLower()) || a.Name.Contains(q) || a.Code.ToLower().Contains(q.ToLower()) || a.Code.Contains(q)
                           select new SelectMultiFormat
                           {
                               text = a.Code + "-" + a.Name,
                               id = a.Id,
                               Name = c.Name
                           }).OrderBy(b => b.text).ToList();
                serialisedJson = hmt;
            }
            else
            {
                var hmt = (from a in db.PropertyMains
                           join d in db.PropertyRegistrations on a.Id equals d.Property into protype
                           from d in protype.DefaultIfEmpty()
                           join c in db.Accountss on d.Owner equals c.AccountsID into doc
                           from c in doc.DefaultIfEmpty()
                           select new SelectMultiFormat
                           {
                               text = a.Code + "-" + a.Name,//+"       "+c.Name,
                               id = a.Id,
                               Name = c.Name
                           }).OrderBy(b => b.text).ToList();
                serialisedJson = hmt;

            }
            //if (x == "All" && start == 0 && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
            //{
            //   // var initial = new SelectMultiFormat() { id = 0, text = stt };
            //    serialisedJson.Insert(0, initial);
            //}
            //if (x == "empty" && start == 0 && (string.IsNullOrEmpty(q)))
            //{
            //    var initial = new SelectMultiFormat() { id = 0, text = "Select Account" };
            //    serialisedJson.Insert(0, initial);
            //}
            return Json(serialisedJson);
        }

        public JsonResult SearchPropertyNotReg(string q, string x)
        {
            List<SelectFormat> serialisedJson;
            if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
            {
                serialisedJson = (from a in db.PropertyMains
                                  join d in db.PropertyRegistrations on a.Id equals d.Property into protype
                                  from d in protype.DefaultIfEmpty()
                                  where a.Id!=d.Property && a.Name.ToLower().Contains(q.ToLower()) || a.Name.Contains(q) || a.Code.ToLower().Contains(q.ToLower()) || a.Code.Contains(q)
                                  select new SelectFormat
                                  {
                                      text = a.Code + "-" + a.Name,
                                      id = a.Id,
                                  })
                                  .OrderBy(b => b.text).ToList();
            }
            else
            {
                serialisedJson = (from a in db.PropertyMains
                                  join d in db.PropertyRegistrations on a.Id equals d.Property into protype
                                  from d in protype.DefaultIfEmpty()
                                  where a.Id != d.Property
                                  select new SelectFormat
                                  {
                                      text = a.Code + "-" + a.Name,
                                      id = a.Id,
                                  })
                                  .OrderBy(b => b.text).ToList();
            }//
            return Json(serialisedJson);
        }
        //public JsonResult (string q, string x)
        //{
        //    List<SelectFormat> serialisedJson;
        //    if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
        //    {
        //        serialisedJson = (from a in db.PropertyMains
        //                          join b in db.PropertyTypes on a.PropertyType equals b.ID into cat
        //                          from b in cat.DefaultIfEmpty()
        //                          join d in db.PropertyRegistrations on a.Id equals d.Property into protype
        //                          from d in protype.DefaultIfEmpty()
        //                          join c in db.Accountss on d.Owner equals c.AccountsID into doc
        //                          from c in doc.DefaultIfEmpty()
        //                          where a.Name.ToLower().Contains(q.ToLower()) || a.Name.Contains(q) || a.Code.ToLower().Contains(q.ToLower()) || a.Code.Contains(q)
        //                          select new SelectMultiFormat
        //                          {
        //                              text = a.Code + "-" + a.Name,
        //                              id = a.Id,
        //                              Name = c.Name
        //                          })
        //                          .OrderBy(b => b.text).ToList();
        //    }
        //    else
        //    {
        //        serialisedJson = db.PropertyMains.Select(a => new SelectFormat
        //        {
        //            text = a.Code + "-" + a.Name,
        //            id = a.Id
        //        }).OrderBy(b => b.text).ToList();

        //    }//
        //    if (x == "empty" && (string.IsNullOrEmpty(q)))
        //    {
        //        var initial = new SelectFormat() { id = 0, text = "Select Property" };
        //        serialisedJson.Insert(0, initial);
        //    }
        //    return Json(serialisedJson);
        //}
        //[QkAuthorize(Roles = "Dev,Create Property")]
        public ActionResult AddProperty()
        {
            ViewBag.PropFeat = QkSelect.List(
                                 new List<SelectListItem>
                                 {
                                    new SelectListItem { Selected = false, Text = "", Value = ""},
                                 }, "Value", "Text", 0);


            var doc = db.DocumentTypes
                    .Select(s => new
                    {
                        ID = s.ID,
                        Name = s.Name
                    })
                    .ToList();
            ViewBag.doctype = QkSelect.List(doc, "ID", "Name");
            var prop = db.PropertyTypes
                    .Select(s => new
                    {
                        ID = s.ID,
                        Name = s.Name
                    })
                    .ToList();
            ViewBag.proptype = QkSelect.List(prop, "ID", "Name");
            var viewModel = new PropertyViewModel
            {

                AdditionalField = db.AdditionalFields.Where(x => x.Section == "Property").ToList()
            };
            return PartialView(viewModel);
        }

        private long GetEntryNo()
        {
            Int64 ENo = 0;
            Int32 number = db.CodePrefixs.Where(a => a.section == "Property").Select(a => a.number).FirstOrDefault();
            if ((db.PropertyMains.Select(p => p.EntryNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
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
                ENo = db.PropertyMains.Max(p => p.EntryNo + 1);
            }
            return ENo;
        }

        private string CustCode(Int64 CNo = 0, string CCode = null)
        {
            var prefix = db.CodePrefixs.Where(a => a.section == "Property").Select(a => a.prefix).FirstOrDefault();

            if (CCode == null)
            {
                Int32 number = db.CodePrefixs.Where(a => a.section == "Property").Select(a => a.number).FirstOrDefault();
                if ((db.PropertyMains.Select(p => p.EntryNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
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
                    CNo = db.PropertyMains.Max(p => p.EntryNo + 1);
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
            var Exists = db.PropertyMains.Any(c => c.Code == Code);
            bool res = (Exists) ? true : false;
            return res;
        }

        [HttpGet]
        public JsonResult GetDocument(long CnId, string purpose)
        {
            var ConD = (from a in db.PropertyDocumentTypes
                        join b in db.DocumentFiles on a.ID equals b.Document                        
                        join c in db.DocumentTypes on a.DocumentType equals c.ID 
                        where a.Reference == CnId && a.Purpose == purpose
                        select new
                        {
                            AttAch = b.attachments,
                            type = c.Name,
                            Value = c.ID,
                            Id = a.ID,
                            Date = a.ExpDate,
                            Purpose = purpose + "_",
                        }).ToList();
            return Json(ConD);
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