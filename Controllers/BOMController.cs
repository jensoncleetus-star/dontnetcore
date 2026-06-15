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
	public class BOMController : BaseController
	{
		ApplicationDbContext db;
		Common com;
		public BOMController()
		{
			db = new ApplicationDbContext();
			com = new Common();
		}

		// GET: BOM
		[RedirectingAction]
		[QkAuthorize(Roles = "Dev,BOM List")]
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
		[QkAuthorize(Roles = "Dev,BOM List")]
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
			var uBomView = User.IsInRole("View BOM");
			var uEdit = User.IsInRole("Edit BOM");
			var uDelete = User.IsInRole("Delete BOM");

			var v = (from a in db.BillOfMaterials
					 join b in db.Items on a.ItemId equals b.ItemID into item
					 from b in item.DefaultIfEmpty()
					 join c in db.ItemUnits on a.Unit equals c.ItemUnitID into unit
					 from c in unit.DefaultIfEmpty()
                     join h in db.MCs on a.MaterialCenter equals h.MCId into mcs
                     from h in mcs.DefaultIfEmpty()
                     join e in db.Users on a.CreatedBy equals e.Id
					 where (BName == null || BName == 0 || a.BOMId == BName) &&
							(Item == null || Item == 0 || a.ItemId == Item) &&
							(Qty == null || Qty == 0 || Qty == a.Quantity) &&
							(Unit == null || Unit == 0 || c.ItemUnitID == Unit)
					 select new
					 {
						 a.BOMId,
						 a.BOMName,
						 b.ItemName,
						 a.Quantity,
						 Expense=a.Expense+((a.Labourcost==null)?0:a.Labourcost)+((a.meterialcost==null)?0:a.meterialcost),
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
		[QkAuthorize(Roles = "Dev,Create BOM")]
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
            BOMViewModel vmodel = new BOMViewModel();
            vmodel.BOMDate = (System.DateTime.Now).ToString("dd-MM-yyyy");

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
		[QkAuthorize(Roles = "Dev,Create BOM")]
		public ActionResult Create(BOMViewModel vmodel)
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

            DateTime? Date = null;
            if (vmodel.BOMDate != null)
            {
                Date = DateTime.Parse(vmodel.BOMDate, new CultureInfo("en-GB"));
            }
            BillOfMaterial bom = new BillOfMaterial
            {
                BOMName = vmodel.bomdata.BOMName,
                ItemId = vmodel.bomdata.ItemId,
                Quantity = vmodel.bomdata.Quantity,
                Unit = vmodel.bomdata.Unit,
				Expense = vmodel.bomdata.Expense,
				Labourcost = vmodel.bomdata.Labourcost,
				meterialcost = vmodel.bomdata.meterialcost,
				BOMDate = Date,
                MaterialCenter = vmodel.bomdata.MaterialCenter,
                CreatedDate = today,
                CreatedBy = UserId,
                Status = Status.active,

            };
			db.BillOfMaterials.Add(bom);
			db.SaveChanges();
			Int64 bomId = bom.BOMId;

			BOMItem bomItem = new BOMItem();
			foreach (var arr in vmodel.bomitems)
			{
				bomItem.BOMId = bomId;
				bomItem.ItemId = arr.ItemId;
				bomItem.Quantity = arr.Quantity;
				bomItem.Unit = arr.Unit;
				db.BOMItems.Add(bomItem);
				db.SaveChanges();
			}


			com.addlog(LogTypes.Created, UserId, "BillOfMaterial", "BillOfMaterials", findip(), bomId, "Successfully Submitted Bill Of Materials");

			msg = "Successfully Added Bills Of Materials .";
			stat = true;
			return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

		}
		[RedirectingAction]
		[QkAuthorize(Roles = "Dev,Edit BOM")]
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
			BillOfMaterial bom = db.BillOfMaterials.Find(id);

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

			BOMViewModel vmodel = new BOMViewModel();
			vmodel.BOMId = (long)id;
			vmodel.BOMName = bom.BOMName;
			vmodel.ItemId = bom.ItemId;
			vmodel.Quantity = bom.Quantity;
			vmodel.Unit = bom.Unit;
			vmodel.Expense = bom.Expense;
			vmodel.Labourcost = bom.Labourcost;
			vmodel.meterialcost = bom.meterialcost;
			vmodel.Branch = bom.Branch;
            vmodel.MaterialCenter = bom.MaterialCenter;

            vmodel.BOMDate = bom.BOMDate !=null ? bom.BOMDate.Value.ToString("dd-MM-yyyy"):null;

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
		[QkAuthorize(Roles = "Dev,Edit BOM")]
		public JsonResult Edit(BOMViewModel vmodel, long id)
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
            DateTime? Date = null;
            if (vmodel.BOMDate != null)
            {
                Date = DateTime.Parse(vmodel.BOMDate, new CultureInfo("en-GB"));
            }

            BillOfMaterial bom = db.BillOfMaterials.Find(id);

			bom.BOMName = vmodel.bomdata.BOMName;
			bom.ItemId = vmodel.bomdata.ItemId;
			bom.Quantity = vmodel.bomdata.Quantity;
			bom.Unit = vmodel.bomdata.Unit;
			bom.Expense = vmodel.bomdata.Expense;

			bom.Labourcost = vmodel.bomdata.Labourcost;
			bom.meterialcost = vmodel.bomdata.meterialcost;
			bom.Branch = Branch;
            bom.BOMDate= Date;
            bom.MaterialCenter = vmodel.bomdata.MaterialCenter;

            db.Entry(bom).State = EntityState.Modified;
			db.SaveChanges();
			Int64 bomId = bom.BOMId;


			var bItems = db.BOMItems.Where(a => a.BOMId == bomId).FirstOrDefault();
			if (bItems != null)
			{
				db.BOMItems.RemoveRange(db.BOMItems.Where(a => a.BOMId == bomId));
				db.SaveChanges();
			}

			BOMItem bomItem = new BOMItem();
			foreach (var arr in vmodel.bomitems)
			{
				bomItem.BOMId = bomId;
				bomItem.ItemId = arr.ItemId;
				bomItem.Quantity = arr.Quantity;
				bomItem.Unit = arr.Unit;
				db.BOMItems.Add(bomItem);
				db.SaveChanges();
			}

			com.addlog(LogTypes.Updated, UserId, "BillOfMaterial", "BillOfMaterials", findip(), bomId, "Successfully Updated Bill Of Materials");
			msg = "Successfully Updated Bills Of Materials .";
			stat = true;
			return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
		}


		[HttpPost]
		public JsonResult GetBOMDetails(long BomID)
		{
			var bom = (from a in db.BillOfMaterials
					   join b in db.Items on a.ItemId equals b.ItemID 
					   join c in db.ItemUnits on a.Unit equals c.ItemUnitID into unit
					   from c in unit.DefaultIfEmpty()
					   join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
					   from d in second.DefaultIfEmpty()
					   where a.BOMId == BomID
					   select new
					   {
						   a.BOMId,
						   BOMNamewithcode = a.BOMId + " - " + a.BOMName,
						   ItemNamewithcode = b.ItemCode + " - " + b.ItemName,
						   a.BOMName,
						   b.ItemName,
						   a.ItemId,
						   a.Quantity,
						   Expense = a.Expense + ((a.Labourcost == null) ? 0 : a.Labourcost) + ((a.meterialcost == null) ? 0 : a.meterialcost),
						   prductioncost= a.Expense,
						   labourcost= (a.Labourcost == null) ? 0 : a.Labourcost,
						   materialcost= (a.meterialcost == null) ? 0 : a.meterialcost,
						   a.Unit,
						   b.ItemUnitID,
						   b.SubUnitId,
						   PriUnit = c.ItemUnitName,
						   SubUnit = d.ItemUnitName,
						   c.ItemUnitName,
						   Price = b.PurchasePrice,
						   Amount = "",

					   }).FirstOrDefault();

			var item = (from a in db.BOMItems
						join b in db.Items on a.ItemId equals b.ItemID
						join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
						from c in primary.DefaultIfEmpty()
						join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
						from d in second.DefaultIfEmpty()
						where a.BOMId == BomID
						select new
						{
							a.BOMItemId,
							a.ItemId,
							a.Quantity,
							a.Unit,
							a.BOMId,
							b.ItemCode,
							b.ItemName,
							ItemWithCode = b.ItemCode + " - " + b.ItemName,
							b.ItemUnitID,
							b.SubUnitId,
							PriUnit = c.ItemUnitName,
							SubUnit = d.ItemUnitName,
							ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
							// b.ItemID removed — collides with a.ItemId by case-insensitive JSON name (same value via the join).
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
							// o.ItemID removed — collides with o.ItemId by case-insensitive JSON name (same value).
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
							o.BOMId,
						}).ToList();


			return new QuickSoft.Models.LegacyJsonResult { Data = new { bom, item } };
		}

		[RedirectingAction]
		[HttpGet]
		[QkAuthorize(Roles = "Dev,View BOM")]
		public ActionResult Details(long? id)
		{
			BOMViewModel vmodel = new BOMViewModel();
			vmodel = (from a in db.BillOfMaterials
					  join b in db.Items on a.ItemId equals b.ItemID into item
					  from b in item.DefaultIfEmpty()
					  join c in db.ItemUnits on a.Unit equals c.ItemUnitID into unit
					  from c in unit.DefaultIfEmpty()
                      join h in db.MCs on a.MaterialCenter equals h.MCId into mcs
                      from h in mcs.DefaultIfEmpty()
                      join e in db.Users on a.CreatedBy equals e.Id
					  where a.BOMId == id
					  select new BOMViewModel
					  {
						  BOMId = a.BOMId,
						  BOMName = a.BOMName,
						  ItemName = b.ItemName,
						  Quantity = a.Quantity,
						  Expense = a.Expense,
						  Unit = a.Unit,
						  ItemUnitName = c.ItemUnitName,
						  UserName = e.UserName
					  }).FirstOrDefault();
			vmodel.BOMItemvmodel = db.BOMItems.Where(a => a.BOMId == id)
			.Select(b => new BOMItemViewModel
			{
				Quantity = b.Quantity,
				ItemName = db.Items.Where(a => a.ItemID == b.ItemId).Select(a => a.ItemName).FirstOrDefault(),
				ItemUnit = db.ItemUnits.Where(a => a.ItemUnitID == b.Unit).Select(a => a.ItemUnitName).FirstOrDefault()
			}).ToList();

			return View(vmodel);
		}



		[RedirectingAction]
		[QkAuthorize(Roles = "Dev,Delete BOM")]
		public ActionResult Delete(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			BillOfMaterial bom = db.BillOfMaterials.Find(id);
			if (bom == null)
			{
				return NotFound();
			}
			return PartialView(bom);
		}

		[RedirectingAction]
		[QkAuthorize(Roles = "Dev,Delete BOM")]
		[ValidateAntiForgeryToken]
		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(long id)
		{
			bool stat = false;
			string msg;
			var UserId = User.Identity.GetUserId();
			BillOfMaterial bom = db.BillOfMaterials.Find(id);

			var Msg = chkDeleteWithMsg(id);
			if (Msg != null)
			{
				msg = Msg;
				stat = false;
			}
			else
			{
				stat = DeleteFn(id);
				msg = "Successfully deleted Bill Of Materials.";
			}
			return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
		}

		[HttpPost]
		[QkAuthorize(Roles = "Dev,Delete BOM")]
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
			return RedirectToAction("Index", "BOM");
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
			if (db.GeneratedItem.Any(c => c.BOM == id))
			{
				msg = "BOM Already used in Production !!";
			}
			else if (db.ConsumedItem.Any(c => c.BOM == id))
			{
				msg = "BOM Already used in Unassemble !!";
			}
			else
			{
				msg = null;
			}

			return msg;
		}

		public bool DeleteFn(long id)
		{
			BillOfMaterial bom = db.BillOfMaterials.Find(id);
			var bomItem = db.BOMItems.Where(a => a.BOMId == id);
			if (bomItem != null)
			{
				db.BOMItems.RemoveRange(db.BOMItems.Where(a => a.BOMId == id));
			}
			if (bom != null)
			{
				db.BillOfMaterials.RemoveRange(db.BillOfMaterials.Where(a => a.BOMId == id));
			}
			var UserId = User.Identity.GetUserId();
			com.addlog(LogTypes.Deleted, UserId, "BillOfMaterial", "BillOfMaterials", findip(), id, "Successfully Deleted Bill Of Materials");
			db.SaveChanges();
			return true;
		}

		public JsonResult SearchBOM(string q, string x)
		{
			List<SelectFormat> serialisedJson;
			string stt = "All";
			if (!(string.IsNullOrEmpty(q) && string.IsNullOrEmpty(q)))
			{
				serialisedJson = (from a in db.BillOfMaterials
								  join b in db.Items on a.ItemId equals b.ItemID
								  select new
                                  {
									  a.BOMId,
									  a.BOMName,
									  b.ItemCode,
									  b.ItemName
                                  })
								  .Where(p => p.BOMName.ToLower().Contains(q.ToLower()) || p.BOMName.Contains(q)|| p.ItemCode.Contains(q) || p.ItemName.Contains(q))
								  .Select(b => new SelectFormat
								  {
									  text = b.BOMName + " " +b.ItemCode + "-"+b.ItemName, //each json object will have 
									  id = b.BOMId
								  })
								  .OrderBy(b => b.text).ToList();
			}
			else
			{
				serialisedJson = (from a in db.BillOfMaterials
								  join b in db.Items on a.ItemId equals b.ItemID
								  select new
								  {
									  a.BOMId,
									  a.BOMName,
									  b.ItemCode,
									  b.ItemName
								  }).Select(b => new SelectFormat
				{
					text = b.BOMName + " " + b.ItemCode + "-" + b.ItemName, //each json object will have 
					id = b.BOMId
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
			var bs = db.BillOfMaterials.Where(c => c.BOMId == bomID).Select(b => new
			{
				b.BOMName,
				b.BOMId,
				b.Expense,
				b.Quantity,
				b.Status,
				b.Unit,
				b.Branch
			}).FirstOrDefault();
			return Json(bs);

		}
		[HttpPost]
		public JsonResult GetItemDetails(long? item)
		{
			var bitems = (from a in db.BOMItems
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
