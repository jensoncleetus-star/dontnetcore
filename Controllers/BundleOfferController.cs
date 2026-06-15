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
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace QuickSoft.Controllers
{
	[RedirectingAction]
	public class BundleOfferController : BaseController
	{
		ApplicationDbContext db;
		Common com;
		public BundleOfferController()
		{
			db = new ApplicationDbContext();
			com = new Common();
		}

		// GET: BOM
		[RedirectingAction]
		[QkAuthorize(Roles = "Dev,BOF List")]
		public ActionResult Index()
		{
			var ThText = QkSelect.List(
								new List<SelectListItem>
								{
									new SelectListItem { Selected = true, Text = "All", Value = "0"},
								}, "Value", "Text", 1);
			ViewBag.Itemss = ThText;
			ViewBag.Unitss = ThText;
			ViewBag.BOMName = ThText;
			return View();
		}
		[RedirectingAction]
		[HttpPost]
		[QkAuthorize(Roles = "Dev,BOF List")]
		public ActionResult GetBOM(long? BName, long? Item, decimal? Qty, long? Unit)
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
			var uBomView = User.IsInRole("View BOF");
			var uEdit = User.IsInRole("Edit BOF");
			var uDelete = User.IsInRole("Delete BOF");

			var v = (from a in db.BillOfMaterialsoffers
					 join b in db.Items on a.ItemId equals b.ItemID into item
					 from b in item.DefaultIfEmpty()
					 join c in db.ItemUnits on a.Unit equals c.ItemUnitID into unit
					 from c in unit.DefaultIfEmpty()
                     join h in db.MCs on a.MaterialCenter equals h.MCId into mcs
                     from h in mcs.DefaultIfEmpty()
                     join e in db.Users on a.CreatedBy equals e.Id
					 where (BName == null || BName == 0 || a.BOMOfferId == BName) &&
							(Item == null || Item == 0 || a.ItemId == Item) &&
						
							(Unit == null || Unit == 0 || c.ItemUnitID == Unit)
					 select new
					 {
						 a.BOMOfferId,
						 a.BOMName,
						 b.ItemName,
					
						 a.Price,
						 a.Unit,
						 c.ItemUnitName,
						 e.UserName,
						 Details = uBomView,
						 Dev = uDev,
						 Edit = uEdit,
						 Delete = uDelete,
                         a.CreatedDate
					 });

			//search
			if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
			{
				v = v.Where(p => p.BOMName.ToString().ToLower().Equals(search.ToLower()) ||
							 p.ItemName.ToString().ToLower().Contains(search.ToLower())
							 //p.UserName.ToString().ToLower().Contains(search.ToLower())
							 );
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



		[RedirectingAction]
		[QkAuthorize(Roles = "Dev,Create BOF")]
		[HttpGet]
		public ActionResult Create()
		{
			ViewBag.Item = QkSelect.List(
						   new List<SelectListItem>
						   {
									new SelectListItem { Selected = false, Text = "", Value = "0"},
						   }, "Value", "Text", 1);


			var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
			var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
			ViewBag.BranchCheck = BranchCheck;

			var Bnch = db.Branchs
				   .Select(s => new
				   {
					   Id = s.BranchID,
					   Name = s.BranchName
				   }).ToList();
			ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");
			var UserId = User.Identity.GetUserId();
			ViewBag.LastEntry = db.BillOfMaterials.Where(p => p.CreatedBy == UserId).Select(p => p.BOMId).AsEnumerable().DefaultIfEmpty(0).Max();
            BOFViewModel vmodel = new BOFViewModel();
            vmodel.BOMDateStart = (System.DateTime.Now).ToString("dd-MM-yyyy");
			vmodel.BOMDateEnd = (System.DateTime.Now.AddDays(30)).ToString("dd-MM-yyyy");

			ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            if (mcchk != null)
            {
                var mcs = db.MCs.Where(s => s.AssignedUser == UserId).Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
            }
            else
            {
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
            }

            return View(vmodel);
		}
		[HttpPost]
		[RedirectingAction]
		[QkAuthorize(Roles = "Dev,Create BOF")]
		public ActionResult Create(BOFViewModel vmodel)
		{
			bool stat = false;
			string msg;
			var UserId = User.Identity.GetUserId();
			var today = Convert.ToDateTime(System.DateTime.Now);

			long Branch = 0;

			var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
			var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

			if (BranchCheck == Status.active)
			{
				Branch = vmodel.bomdata.Branch;
			}
			else
			{
				Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
			}

            DateTime? BOMDateStart = null;
            if (vmodel.BOMDateStart != null)
            {
				BOMDateStart = DateTime.Parse(vmodel.BOMDateStart, new CultureInfo("en-GB"));
            }
			DateTime? BOMDateEnd = null;
			if (vmodel.BOMDateEnd != null)
			{
				BOMDateEnd = DateTime.Parse(vmodel.BOMDateEnd, new CultureInfo("en-GB"));
			}
			BillOfMaterialsoffer bom = new BillOfMaterialsoffer
            {
                BOMName = vmodel.bomdata.BOMName,
                ItemId = vmodel.bomdata.ItemId,
                
                Unit = vmodel.bomdata.Unit,
                Price = vmodel.bomdata.Price,
				BOMDateStart = BOMDateStart,
				BOMDateEnd = BOMDateEnd,
				MaterialCenter = vmodel.bomdata.MaterialCenter,
                CreatedDate = today,
                CreatedBy = UserId,
                Status = Status.active,

            };
			db.BillOfMaterialsoffers.Add(bom);
			db.SaveChanges();
			Int64 bomId = bom.BOMOfferId;

			BOMItemsoffer bomItem = new BOMItemsoffer();
			foreach (var arr in vmodel.bomitems)
			{
				bomItem.BOMOfferId = bomId;
				bomItem.ItemId = arr.ItemId;
				bomItem.Quantity = arr.Quantity;
				bomItem.Unit = arr.Unit;
				db.BOMItemsoffers.Add(bomItem);
				db.SaveChanges();
			}


			com.addlog(LogTypes.Created, UserId, "BillOfMaterialOffer", "BillOfMaterialsOffer", findip(), bomId, "Successfully Submitted Bundle Offer");

			msg = "Successfully Added Bundle Offer";
			stat = true;
			return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

		}
		[RedirectingAction]
		[QkAuthorize(Roles = "Dev,Edit BOF")]
		[HttpGet]
		public ActionResult Edit(long? id)
		{
			var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
			var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
			ViewBag.BranchCheck = BranchCheck;

            var UserId = User.Identity.GetUserId();

            if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			BillOfMaterialsoffer bom = db.BillOfMaterialsoffers.Find(id);

			var Bnch = db.Branchs
				   .Select(s => new
				   {
					   Id = s.BranchID,
					   Name = s.BranchName
				   }).ToList();
			ViewBag.SetBranch = QkSelect.List(Bnch, "Id", "Name");

			if (bom == null)
			{
				return NotFound();
			}

			BOFViewModel vmodel = new BOFViewModel();
			vmodel.BOMId = (long)id;
			vmodel.BOMName = bom.BOMName;
			vmodel.ItemId = bom.ItemId;
		
			vmodel.Unit = bom.Unit;
			vmodel.Price = bom.Price;
			vmodel.Branch = bom.Branch;
            vmodel.MaterialCenter = bom.MaterialCenter;

            vmodel.BOMDateStart = bom.BOMDateStart !=null ? bom.BOMDateStart.Value.ToString("dd-MM-yyyy"):null;
			vmodel.BOMDateEnd = bom.BOMDateEnd != null ? bom.BOMDateEnd.Value.ToString("dd-MM-yyyy") : null;

			var itm = db.Items.Select(s => new
			{
				ID = s.ItemID,
				Name = s.ItemCode + " " + s.ItemName
			}).ToList();
			ViewBag.Item = QkSelect.List(itm, "ID", "Name");

            var mcchk = db.MCs.Where(a => a.AssignedUser == UserId).FirstOrDefault();
            if (mcchk != null)
            {
                var mcs = db.MCs.Where(s => s.AssignedUser == UserId).Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
            }
            else
            {
                var mcs = db.MCs.Select(s => new
                {
                    Id = s.MCId,
                    Name = s.MCName
                }).ToList();
                ViewBag.MC = QkSelect.List(mcs, "Id", "Name");
            }

			ViewBag.preEntry = db.BillOfMaterials.Where(a => a.BOMId < id && (a.CreatedBy == UserId)).Select(a => a.BOMId).DefaultIfEmpty().Max();
			ViewBag.nxtEntry = db.BillOfMaterials.Where(a => a.BOMId > id && (a.CreatedBy == UserId)).Select(a => a.BOMId).DefaultIfEmpty().Min();

            ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

            var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
            var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
            ViewBag.EnableMCcheck = MCcheck;

            return View(vmodel);
		}
		[HttpPost]
		[RedirectingAction]
		[QkAuthorize(Roles = "Dev,Edit BOF")]
		public JsonResult Edit(BOFViewModel vmodel, long id)
		{
			bool stat = false;
			string msg;
			var UserId = User.Identity.GetUserId();
			var today = Convert.ToDateTime(System.DateTime.Now);
			long Branch = 0;

			var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
			var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

			if (BranchCheck == Status.active)
			{
				Branch = vmodel.bomdata.Branch;
			}
			else
			{
				Branch = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).FirstOrDefault();
			}
            DateTime? BOMDateStart = null;
            if (vmodel.BOMDateStart != null)
            {
				BOMDateStart = DateTime.Parse(vmodel.BOMDateStart, new CultureInfo("en-GB"));
            }
			DateTime? BOMDateEnd = null;
			if (vmodel.BOMDateEnd != null)
			{
				BOMDateEnd = DateTime.Parse(vmodel.BOMDateEnd, new CultureInfo("en-GB"));
			}
			BillOfMaterialsoffer bom = db.BillOfMaterialsoffers.Find(id);

			bom.BOMName = vmodel.bomdata.BOMName;
			bom.ItemId = vmodel.bomdata.ItemId;
			
			bom.Unit = vmodel.bomdata.Unit;
			bom.Price = vmodel.bomdata.Price;
			bom.Branch = Branch;
            bom.BOMDateStart= BOMDateStart;
			bom.BOMDateEnd = BOMDateEnd;
			bom.MaterialCenter = vmodel.bomdata.MaterialCenter;

            db.Entry(bom).State = EntityState.Modified;
			db.SaveChanges();
			Int64 bomId = bom.BOMOfferId;


			var bItems = db.BOMItemsoffers.Where(a => a.BOMOfferId == bomId).FirstOrDefault();
			if (bItems != null)
			{
				db.BOMItemsoffers.RemoveRange(db.BOMItemsoffers.Where(a => a.BOMOfferId == bomId));
				db.SaveChanges();
			}

			BOMItemsoffer bomItem = new BOMItemsoffer();
			foreach (var arr in vmodel.bomitems)
			{
				bomItem.BOMOfferId = bomId;
				bomItem.ItemId = arr.ItemId;
				bomItem.Quantity = arr.Quantity;
				bomItem.Unit = arr.Unit;
				db.BOMItemsoffers.Add(bomItem);
				db.SaveChanges();
			}

			com.addlog(LogTypes.Updated, UserId, "BillOfMaterialoffer", "BillOfMaterialsoffer", findip(), bomId, "Successfully Updated Bundle Offer");
			msg = "Successfully Updated Bundle Offer .";
			stat = true;
			return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
		}


		[HttpPost]
		public ActionResult GetBOMDetails(long BomID)
		{
			var bom = (from a in db.BillOfMaterialsoffers
					   join b in db.Items on a.ItemId equals b.ItemID 
					   join c in db.ItemUnits on a.Unit equals c.ItemUnitID into unit
					   from c in unit.DefaultIfEmpty()
					   join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
					   from d in second.DefaultIfEmpty()
					   where a.BOMOfferId == BomID
					   select new
					   {
						   a.BOMOfferId,
						   BOMNamewithcode = a.BOMOfferId + " - " + a.BOMName,
						   ItemNamewithcode = b.ItemCode + " - " + b.ItemName,
						   a.BOMName,
						   b.ItemName,
						   a.ItemId,
						  
						   a.Price,
						   a.Unit,
						   b.ItemUnitID,
						   b.SubUnitId,
						   PriUnit = c.ItemUnitName,
						   SubUnit = d.ItemUnitName,
						   c.ItemUnitName,
						   PPrice = b.PurchasePrice,
						   Amount = "",

					   }).FirstOrDefault();

			var item = (from a in db.BOMItemsoffers
						join b in db.Items on a.ItemId equals b.ItemID
						join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
						from c in primary.DefaultIfEmpty()
						join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
						from d in second.DefaultIfEmpty()
						where a.BOMOfferId == BomID
						select new
						{
							a.BOMItemId,
							a.ItemId,
							a.Quantity,
							a.Unit,
							a.BOMOfferId,
							b.ItemCode,
							b.ItemName,
							ItemWithCode = b.ItemCode + " - " + b.ItemName,
							b.ItemUnitID,
							b.SubUnitId,
							PriUnit = c.ItemUnitName,
							SubUnit = d.ItemUnitName,
							ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
							b.ItemID,
							b.OpeningStock,
							b.MinStock,
							b.SellingPrice,
							b.PurchasePrice,
							b.BasePrice,
							b.MRP,
							b.KeepStock,


							//for min stock check
							PriPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
							SubPurchase = (int?)db.PEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,


							PriSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
							SubSale = (int?)db.SEItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,

							PriPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
							SubPReturn = (int?)db.PRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,

							PriSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.ItemUnitID).Select(a => a.ItemQuantity).Sum() ?? 0,
							SubSReturn = (int?)db.SRItemss.Where(a => a.Item == b.ItemID && a.ItemUnit == b.SubUnitId).Select(a => a.ItemQuantity).Sum() ?? 0,


							//stock adjustment---
							PriAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,
							SubAddAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Add).Select(a => a.ItemQuantity).Sum() ?? 0,

							PriLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.ItemUnitID && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
							subLessAdj = (decimal?)db.StockAdjustments.Where(a => a.ItemID == b.ItemID && a.ItemUnitID == b.SubUnitId && a.AdjustmentType == AdjustmentType.Less).Select(a => a.ItemQuantity).Sum() ?? 0,
							//-------
							// production ----
							// main item
							PriProdItem = (decimal?)db.GeneratedItem.Where(a => a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
							SubProdItem = (decimal?)db.GeneratedItem.Where(a => a.Item == b.ItemID && a.Unit == b.SubUnitId).Select(a => a.Qty).Sum() ?? 0,
							// compined item
							PriProdCItem = (decimal?)db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,
							SubProdCItem = (decimal?)db.ProItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId).Select(a => a.Quantity).Sum() ?? 0,
							// unassemble -----
							// main item
							PriUnItem = (decimal?)db.ConsumedItem.Where(a => a.Item == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Qty).Sum() ?? 0,
							SubUnItem = (decimal?)db.ConsumedItem.Where(a => a.Item == b.ItemID && a.Unit == b.SubUnitId).Select(a => a.Qty).Sum() ?? 0,
							// compined item
							PriUnCItem = (decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.ItemUnitID).Select(a => a.Quantity).Sum() ?? 0,
							SubUnCItem = (decimal?)db.UnassembleItems.Where(a => a.ItemId == b.ItemID && a.Unit == b.SubUnitId).Select(a => a.Quantity).Sum() ?? 0,


						}).AsEnumerable().Select(o => new
						{
                            o.BOMItemId,
							o.ItemId,
							o.Quantity,
							o.Unit,
							o.ItemID,
							o.ItemCode,
							o.ItemName,
							o.ItemWithCode,
							o.ItemUnitID,
							o.SubUnitId,
							PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
							SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
							OpeningStock = o.OpeningStock,
							MinStock = (o.MinStock != null) ? o.MinStock : 0,
							o.ConFactor,
							o.SellingPrice,
							o.PurchasePrice,
							o.BasePrice,
							o.MRP,
							price = o.PurchasePrice,
							o.KeepStock,

							PriPurchase = (o.PriPurchase + (int)(o.SubPurchase / o.ConFactor)),
							SubPurchase = (o.SubPurchase % o.ConFactor),

							PriSale = (o.PriSale + (int)(o.SubSale / o.ConFactor)),
							SubSale = (o.SubSale % o.ConFactor),

							PriPReturn = (o.PriPReturn + (int)(o.SubPReturn / o.ConFactor)),
							SubPReturn = (o.SubPReturn % o.ConFactor),

							PriSReturn = (o.PriSReturn + (int)(o.SubSReturn / o.ConFactor)),
							SubSReturn = (o.SubSReturn % o.ConFactor),

							pritotal = ((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)),
							subtotal = ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),
							total = (((o.OpeningStock + o.PriPurchase + o.PriSReturn + o.PriAddAdj + o.PriProdItem + o.PriUnCItem) - (o.PriSale + o.PriPReturn + o.PriLessAdj + o.PriProdCItem + o.PriUnItem)) * o.ConFactor) + ((o.SubPurchase + o.SubSReturn + o.SubAddAdj + o.SubProdItem + o.SubUnCItem) - (o.SubSale + o.SubPReturn + o.subLessAdj + o.SubProdCItem + o.SubUnItem)),
							o.BOMOfferId,
						}).ToList();


			return LegacyJson(new { bom, item });
		}

		[RedirectingAction]
		[HttpGet]
		[QkAuthorize(Roles = "Dev,View BOF")]
		public ActionResult Details(long? id)
		{
			BOFViewModel vmodel = new BOFViewModel();
			vmodel = (from a in db.BillOfMaterialsoffers
					  join b in db.Items on a.ItemId equals b.ItemID into item
					  from b in item.DefaultIfEmpty()
					  join c in db.ItemUnits on a.Unit equals c.ItemUnitID into unit
					  from c in unit.DefaultIfEmpty()
                      join h in db.MCs on a.MaterialCenter equals h.MCId into mcs
                      from h in mcs.DefaultIfEmpty()
                      join e in db.Users on a.CreatedBy equals e.Id
					  where a.BOMOfferId == id
					  select new BOFViewModel
					  {
						  BOMId = a.BOMOfferId,
						  BOMName = a.BOMName,
						  ItemName = b.ItemName,

						  Price = a.Price,
						  Unit = a.Unit,
						  ItemUnitName = c.ItemUnitName,
						  UserName = e.UserName
					  }).FirstOrDefault();
			vmodel.BOMItemvmodel= db.BOMItemsoffers.Where(a => a.BOMOfferId == id)
			.Select(b => new BOFItemViewModel
			{
				Quantity = b.Quantity,
				ItemName = db.Items.Where(a => a.ItemID == b.ItemId).Select(a => a.ItemName).FirstOrDefault(),
				ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.Unit).Select(a => a.ItemUnitName).FirstOrDefault()
			}).ToList();

			return View(vmodel);
		}



		[RedirectingAction]
		[QkAuthorize(Roles = "Dev,Delete BOF")]
		public ActionResult Delete(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			BillOfMaterialsoffer bom = db.BillOfMaterialsoffers.Find(id);
			if (bom == null)
			{
				return NotFound();
			}
			return PartialView(bom);
		}

		[RedirectingAction]
		[QkAuthorize(Roles = "Dev,Delete BOF")]
		[ValidateAntiForgeryToken]
		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(long id)
		{
			bool stat = false;
			string msg;
			var UserId = User.Identity.GetUserId();
			BillOfMaterialsoffer bom = db.BillOfMaterialsoffers.Find(id);

			var Msg = chkDeleteWithMsg(id);
			if (Msg != null)
			{
				msg = Msg;
				stat = false;
			}
			else
			{
				stat = DeleteFn(id);
				msg = "Successfully deleted Bundle Offer.";
			}
			return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
		}

		[HttpPost]
		[QkAuthorize(Roles = "Dev,Delete BOF")]
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
				Success("Deleted " + count + " BOM, Unable to Delete " + notdel + " BOM. ", true);
			}
			else if (notdel > 0)
			{
				Danger("Unable to Delete " + notdel + " BOM.", true);
			}
			else
			{
				Success("Deleted " + count + " BOM.", true);
			}
			return RedirectToAction("Index", "BundleOffer");
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

		public string chkDeleteWithMsg(long id)
		{
			string msg = null;
			
				msg = null;
			

			return msg;
		}

		public bool DeleteFn(long id)
		{
			BillOfMaterialsoffer bom = db.BillOfMaterialsoffers.Find(id);
			var bomItem = db.BOMItemsoffers.Where(a => a.BOMOfferId == id);
			if (bomItem != null)
			{
				db.BOMItemsoffers.RemoveRange(db.BOMItemsoffers.Where(a => a.BOMOfferId == id));
			}
			if (bom != null)
			{
				db.BillOfMaterialsoffers.RemoveRange(db.BillOfMaterialsoffers.Where(a => a.BOMOfferId == id));
			}
			var UserId = User.Identity.GetUserId();
			com.addlog(LogTypes.Deleted, UserId, "BillOfMaterialoffer", "BillOfMaterialsoffer", findip(), id, "Successfully Deleted Bundle Offer");
			db.SaveChanges();
			return true;
		}

		public JsonResult SearchBOM(string q, string x)
		{
			List<SelectFormat> serialisedJson;
			string stt = "All";
			if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
			{
				serialisedJson = db.BillOfMaterialsoffers.Where(p => p.BOMName.ToLower().Contains(q.ToLower()) || p.BOMName.Contains(q))
								  .Select(b => new SelectFormat
								  {
									  text = b.BOMName, //each json object will have 
									  id = b.BOMOfferId
								  })
								  .OrderBy(b => b.text).ToList();
			}
			else
			{
				serialisedJson = db.BillOfMaterialsoffers.Select(b => new SelectFormat
				{
					text = b.BOMName, //each json object will have 
					id = b.BOMOfferId
				}).OrderBy(b => b.text).ToList();

			}//
			if (x == "all" && (string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q) || stt.ToUpperInvariant().Contains(q.ToUpperInvariant())))
			{
				var initial = new SelectFormat() { id = 0, text = stt };
				serialisedJson.Insert(0, initial);
			}
			return Json(serialisedJson);
		}
		public ActionResult GetBOMById(int bomID)
		{
			var bs = db.BillOfMaterialsoffers.Where(c => c.BOMOfferId == bomID).Select(b => new
			{
				b.BOMName,
				b.BOMOfferId,
				b.Price,
				
				b.Status,
				b.Unit,
				b.Branch
			}).FirstOrDefault();
			return Json(bs);

		}
		[HttpPost]
		public JsonResult GetItemDetails(long? item)
		{
			var bitems = (from a in db.BOMItemsoffers
						  join b in db.Items on a.ItemId equals b.ItemID into it
						  from b in it.DefaultIfEmpty()//db.BOMItems.Where(x => x.ItemId == item).FirstOrDefault();
						  where a.ItemId == item
						  select new
						  {
							  Quantity = a.Quantity,
							  Price = b.SellingPrice
						  }).FirstOrDefault();
			return new QuickSoft.Models.LegacyJsonResult { Data = new { items = bitems } };

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
