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
	public class BoqController : BaseController
	{
		ApplicationDbContext db;
		Common com;
		public BoqController()
		{
			db = new ApplicationDbContext();
			com = new Common();
		}

		// GET: BOM
		[RedirectingAction]
		//[QkAuthorize(Roles = "Dev,BOQ List")]

		public ActionResult Index()
		{
			var ThText = QkSelect.List(
								new List<SelectListItem>
								{
									new SelectListItem { Selected = true, Text = "All", Value = "0"},
								}, "Value", "Text", 1);
			ViewBag.Customer = ThText;
			ViewBag.Employee = ThText;
			ViewBag.BOQName = ThText;
			return View();
		}
		[RedirectingAction]
		[HttpPost]
		////[QkAuthorize(Roles = "Dev,BOQ List")]
		public ActionResult GetBOQ( long? Customer, long? SalesExecutive)
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
			var uBomView = User.IsInRole("View BOQ");
			var uEdit = User.IsInRole("Edit BOQ");
			var uDelete = User.IsInRole("Delete BOQ");

			var v = (from a in db.BillOfQyts
					 join b in db.Customers on a.Customer equals b.CustomerID into cus
					 from b in cus.DefaultIfEmpty()
					 join c in db.Employees on a.SalesExecutive equals c.EmployeeId into emp
					 from c in emp.DefaultIfEmpty()

					 join e in db.Users on a.CreatedBy equals e.Id
					 where 
							(Customer == null || Customer == 0 || a.Customer == Customer) &&
							(SalesExecutive == null || SalesExecutive == 0 || SalesExecutive == a.SalesExecutive)

					 select new
					 {
						 a.BoqId,
						 a.BillNo,
						 b.CustomerName,

						 c.FirstName,
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
				v = v.Where(p => 
							  p.CustomerName.ToString().ToLower().Contains(search.ToLower()) ||
							  p.FirstName.ToString().ToLower().Contains(search.ToLower()) ||
							  p.BillNo.ToString().ToLower().Equals(search.ToLower())
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

		[HttpGet]
		public ActionResult Create(long? id, string type)
		{

			Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
			TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
			ViewBag.TMOut = TimeOut;
			var hiretype = db.HireTypes
				.Select(s => new
				{
					ID = s.HireTypeId,
					Name = s.Name
				})
				.ToList();

			ViewBag.Item = QkSelect.List(
						   new List<SelectListItem>
						   {
									new SelectListItem { Selected = false, Text = "", Value = "0"},
						   }, "Value", "Text", 1);

			var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
			var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
			ViewBag.PrintLayout = PriLay;
			var invoice = db.InvoiceLayouts
							 .Select(s => new
							 {
								 ID = s.Id,
								 Name = s.Name,
							 })
							 .ToList();
			ViewBag.printlay = QkSelect.List(invoice, "ID", "Name");


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
			BoqViewModel vmodel = new BoqViewModel();

			vmodel.BillNo =GetEntryNo();
			vmodel.BoqDate = (System.DateTime.Now).ToString("dd-MM-yyyy");


			if (id != null)
			{
				if (type == "Quote")
				{
					Quotation quentry = db.Quotations.Find(id);

					if (quentry == null)
					{
						return NotFound();
					}
					Quotation qnt = db.Quotations.Where(x =>  x.QuotationId == id).FirstOrDefault();

					vmodel.ConTypeId = quentry.QuotationId;
					vmodel.ConType = type;
					vmodel.Customer = qnt.Customer;
					vmodel.QuotNo = qnt.BillNo;


					vmodel.SalesExecutive = qnt.QuotCashier;


				}
				
			}


			var cust = db.Customers.Where(s => s.Type == CRMCustomerType.Customer).Select(s => new
			{
				CustomerID = s.CustomerID,
				CustomerDetails = s.CustomerCode + " - " + s.CustomerName
			}).ToList();
			ViewBag.Customr = QkSelect.List(cust, "CustomerID", "CustomerDetails");

			var use = db.Employees.Select(s => new
			{
				ID = s.EmployeeId,
				Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
			}).ToList();
			ViewBag.Cashier = QkSelect.List(use, "ID", "Name");


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

		public ActionResult CreateBoq(BoqViewModel vmodel, string[][] boqitemz, string id, string action,string Layout,long conid, string contype)
		{
			bool stat = false;
			string msg;
			var UserId = User.Identity.GetUserId();
			var today = Convert.ToDateTime(System.DateTime.Now);

			long Branch = 0;

			var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
			var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

			var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
			var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;

			var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
			var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
			var BranchID = db.Users.Where(a => a.Id == UserId).Select(a => a.BranchID).First();



			DateTime? Date = null;
			if (vmodel.BoqDate != null)
			{
				Date = DateTime.Parse(vmodel.BoqDate, new CultureInfo("en-GB"));
			}
			BillOfQty bom = new BillOfQty
			{
				BillNo = vmodel.bomdata.BillNo,
				
				
				Customer = vmodel.bomdata.Customer,
				SalesExecutive = vmodel.bomdata.SalesExecutive,

				BOQDate = DateTime.Parse(vmodel.BoqDate, new CultureInfo("en-GB")),

				CreatedDate = today,
				CreatedBy = UserId,
				Status = Status.active,

			};
			db.BillOfQyts.Add(bom);
			db.SaveChanges();
			Int64 bomId = bom.BoqId;

            BoqItem bomItem = new BoqItem();
            foreach (var arr in boqitemz)
            {
                bomItem.BoqId = bomId;
                bomItem.ItemId = Convert.ToInt64(arr[0]);
                bomItem.Quantity = Convert.ToDecimal( arr[2]);
				bomItem.Unit = Convert.ToInt64(arr[1]);
				bomItem.ItemNote = arr[6];
                db.BoqItems.Add(bomItem);
                db.SaveChanges();
            }
            if (conid != null && conid != 0 && contype != null && contype != "" )
			{

				ConvertTransactions ConTran = new ConvertTransactions();

				ConTran.ConvertFrom = contype;
				ConTran.ConvertTo = "Boq";
				ConTran.From = conid;
				ConTran.To = bom.BoqId;
				ConTran.Status = 0;
				ConTran.CreatedDate = Convert.ToDateTime(System.DateTime.Now);
				ConTran.CreatedBy = UserId;
				ConTran.Branch = Convert.ToInt32(BranchID);
				db.ConvertTransactionss.Add(ConTran);
				db.SaveChanges();
				com.addlog(LogTypes.Created, UserId, "ConvertTransactions", "ConvertTransactionss", findip(), ConTran.Id, "Successfully Submitted Convertion");
			}

			com.addlog(LogTypes.Created, UserId, "Boq", "Boqs", findip(), bomId, "Successfully Submitted Bill of Quantity");
			Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
			TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

			if (action == "print")
			{
				var fmapp = db.FieldMappings.Where(a => a.Section == "ProForma" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

				var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
				var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

				var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
				var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

				var BillOfData = com.BOQData(bomId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, ComHeadCheck);
				var item = BillOfData.pdfItem.ToList();
				var summary = BillOfData;
				var billsundry = BillOfData.billsundry.ToList();

				var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
				var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
				var def = (PriLay == Status.active) ? Convert.ToInt64(Layout) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
				def = def == 0 ? 1 : def;
				var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();
				stat = true;
				return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, layout, fmapp } };
			}


			msg = "Successfully Added Bill of Quantity .";
			stat = true;
			return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };

		}
		[RedirectingAction]
		
		[HttpGet]
		public ActionResult Edit(long? id)
		{

			Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
			TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;
			ViewBag.TMOut = TimeOut;
			var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
			var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;
			ViewBag.BranchCheck = BranchCheck;


			var invoice = db.InvoiceLayouts
							 .Select(s => new
							 {
								 ID = s.Id,
								 Name = s.Name,
							 })
							 .ToList();
			var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
			var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
			ViewBag.PrintLayout = PriLay;
			ViewBag.printlay = QkSelect.List(invoice, "ID", "Name");



			var UserId = User.Identity.GetUserId();

			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			BillOfQty bom = db.BillOfQyts.Where(x => x.BoqId == id).FirstOrDefault();
			if(bom!=null)
			{
				Int64 cashier = Convert.ToInt64(bom.SalesExecutive);
			}
			 

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
			string custname = "";

			BoqViewModel vmodel = new BoqViewModel();


			var cust = db.Customers.Where(s => s.Type == CRMCustomerType.Customer).Select(s => new
			{
				CustomerID = s.CustomerID,
				CustomerDetails = s.CustomerCode + " - " + s.CustomerName
			}).ToList();
			ViewBag.Customr = QkSelect.List(cust, "CustomerID", "CustomerDetails");



			var use = db.Employees.Select(s => new
			{
				ID = s.EmployeeId,
				Name = s.FirstName + " " + s.MiddleName + " " + s.LastName
			}).ToList();
			ViewBag.Cashier = QkSelect.List(use, "ID", "Name");


			vmodel.BoqId = (long)id;

			vmodel.BillNo = bom.BillNo;
			vmodel.QuotNo = bom.QuotNo;
			
			vmodel.Customer = bom.Customer;
			vmodel.SalesExecutive = bom.SalesExecutive;
			


			vmodel.BoqDate = bom.BOQDate.ToString("dd-MM-yyyy");
				







			ViewBag.preEntry = db.BillOfQyts.Where(a => a.BoqId < id && (a.CreatedBy == UserId)).Select(a => a.BoqId).DefaultIfEmpty().Max();
			ViewBag.nxtEntry = db.BillOfQyts.Where(a => a.BoqId > id && (a.CreatedBy == UserId)).Select(a => a.BoqId).DefaultIfEmpty().Min();

			ViewBag.BusinessType = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();

			var EnableMC = db.EnableSettings.Where(a => a.EnableType == "MCInTransaction").FirstOrDefault();
			var MCcheck = EnableMC != null ? EnableMC.Status : Status.inactive;
			ViewBag.EnableMCcheck = MCcheck;

			return View(vmodel);
		}

		
		[HttpPost]
		[RedirectingAction]
		public ActionResult UpdateBoq(BoqViewModel vmodel, string[][] boqitemz, long id,string action,string Layout)
		{

			bool stat = false;
			string msg;
			var UserId = User.Identity.GetUserId();
			var today = Convert.ToDateTime(System.DateTime.Now);
			long Branch = 0;
			var EnablePartNumber = db.EnableSettings.Where(a => a.EnableType == "PartNoInItem").FirstOrDefault();
			var PartNoCheck = EnablePartNumber != null ? EnablePartNumber.Status : Status.inactive;

			var ItemCodeInPrint = db.EnableSettings.Where(a => a.EnableType == "EnableItemCodeInPrint").FirstOrDefault();
			var InPrintItemCode = ItemCodeInPrint != null ? ItemCodeInPrint.Status : Status.inactive;
			var EnableBranch = db.EnableSettings.Where(a => a.EnableType == "EnableBranch").FirstOrDefault();
			var BranchCheck = EnableBranch != null ? EnableBranch.Status : Status.inactive;

			DateTime? Date = null;
			if (vmodel.BoqDate != null)
			{
				Date = DateTime.Parse(vmodel.BoqDate, new CultureInfo("en-GB"));
			}

			BillOfQty bom = db.BillOfQyts.Find(id);
			bom.QuotNo = vmodel.bomdata.QuotNo;
			bom.BillNo = vmodel.bomdata.BillNo;
			
			bom.Customer = vmodel.bomdata.Customer;
			bom.SalesExecutive = vmodel.bomdata.SalesExecutive;


			
			bom.BOQDate = DateTime.Parse(vmodel.BoqDate, new CultureInfo("en-GB"));


			db.Entry(bom).State = EntityState.Modified;
			db.SaveChanges();
			Int64 bomId = bom.BoqId;


			var bItems = db.BoqItems.Where(a => a.BoqId == bomId).FirstOrDefault();
			if (bItems != null)
			{
				db.BoqItems.RemoveRange(db.BoqItems.Where(a => a.BoqId == bomId));
				db.SaveChanges();
			}

			BoqItem bomItem = new BoqItem();
			foreach (var arr in boqitemz)
			{
				bomItem.BoqId = bomId;
				bomItem.ItemId = Convert.ToInt64(arr[0]) ;
				bomItem.Quantity = Convert.ToDecimal(arr[2]);
				bomItem.Unit = Convert.ToInt64(arr[1]);
				bomItem.ItemNote = arr[6];
				db.BoqItems.Add(bomItem);
				db.SaveChanges();
			}

			Int64? TimeOut = Convert.ToInt32(db.EnableSettings.Where(a => a.EnableType == "SetTimeOut").Select(a => a.TypeValue).FirstOrDefault());
			TimeOut = (TimeOut != null && TimeOut != 0) ? TimeOut : 10;

			if (action == "print")
			{
				var fmapp = db.FieldMappings.Where(a => a.Section == "ProForma" && a.Print == FMPrint.Yes && a.Status == Status.active).ToList();

				var EnableProject = db.EnableSettings.Where(a => a.EnableType == "BusinessType" && a.Status == 0).Select(x => x.TypeValue).FirstOrDefault();
				var ProjectCheck = EnableProject == "ProjectBasedBusiness" ? Status.active : Status.inactive;

				var EnableHead = db.EnableSettings.Where(a => a.EnableType == "HideComHeaders").FirstOrDefault();
				var ComHeadCheck = EnableHead != null ? EnableHead.Status : Status.inactive;

				var BillOfData = com.BOQData(bomId, InPrintItemCode, PartNoCheck, TimeOut, ProjectCheck, ComHeadCheck);
				var item = BillOfData.pdfItem.ToList();
				var summary = BillOfData;
				var billsundry = BillOfData.billsundry.ToList();

				var PrintLayout = db.EnableSettings.Where(a => a.EnableType == "Printlayout").FirstOrDefault();
				var PriLay = PrintLayout != null ? PrintLayout.Status : Status.inactive;
				var def = (PriLay == Status.active) ? Convert.ToInt64(Layout) : Convert.ToInt64(db.EnableSettings.Where(x => x.EnableType == "Invoice").Select(y => y.TypeValue).FirstOrDefault());
				def = def == 0 ? 1 : def;
				var layout = db.InvoiceLayouts.Where(a => a.Id == def).FirstOrDefault();
				stat = true;
				return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, item, summary, billsundry, layout, fmapp } };
			}

			com.addlog(LogTypes.Updated, UserId, "Boq", "Boqs", findip(), bomId, "Successfully Updated Bill Of Quantitys");
			msg = "Successfully Updated Bills Of Quantitys .";
			stat = true;
			return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
		}


		[HttpPost]
		public JsonResult GetBOQDetails(long BoqId)
		{
			var bom = (from a in db.BillOfQyts
					   join b in db.Customers on a.Customer equals b.CustomerID into cus
					   from b in cus.DefaultIfEmpty()
					   join c in db.Employees on a.SalesExecutive equals c.EmployeeId into emp
					   from c in emp.DefaultIfEmpty()
					   where a.BoqId == BoqId
					   select new
					   {
						   a.BoqId,
						   a.QuotNo,
						   
						   b.CustomerName,
						   c.FirstName,



					   }).FirstOrDefault();

			var item = (from a in db.BoqItems
						join b in db.Items on a.ItemId equals b.ItemID
						join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
						from c in primary.DefaultIfEmpty()
						join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
						from d in second.DefaultIfEmpty()
						where a.BoqId == BoqId
						select new
						{
							a.BoqItemId,
							a.ItemId,
							a.Quantity,
							a.Unit,
							a.BoqId,
							b.ItemCode,
							b.ItemName,
							a.ItemNote,
							ItemWithCode = b.ItemCode + " - " + b.ItemName,
							b.ItemUnitID,
							c.ItemUnitName,
							b.SubUnitId,
							PriUnit = c.ItemUnitName,
							SubUnit = d.ItemUnitName,
							ConFactor = b.ConFactor != 0 ? b.ConFactor : 1,
							note = a.ItemNote,
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
							o.BoqItemId,
							o.ItemId,
							o.Quantity,
							o.Unit,
							// o.ItemID removed: System.Text.Json collides "ItemID" with "ItemId" (case-insensitive)
							// and 500s the action. The Boq/Edit grid binds item.ItemId (the BoqItem FK), not item.ItemID.
							o.ItemCode,
							o.ItemName,
							o.ItemNote,
							o.ItemWithCode,
							o.ItemUnitID,
							o.ItemUnitName,
							o.SubUnitId,
							PriUnit = (o.PriUnit != null) ? o.PriUnit : "",
							SubUnit = (o.SubUnit != null) ? o.SubUnit : "",
							OpeningStock = o.OpeningStock,
							MinStock = (o.MinStock != null) ? o.MinStock : 0,
							o.ConFactor,
							o.note,
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
							o.BoqId,
						}).ToList();


			return new QuickSoft.Models.LegacyJsonResult { Data = new { bom, item } };
		}



		private long GetEntryNo()
		{
			Int64 EntryNo = 0, LastNo = 0;

			LastNo = Convert.ToInt32((db.BillOfQyts.Select(p => p.BillNo).DefaultIfEmpty().Max()));

			if (LastNo == 0)
				EntryNo = 1;
			else
				EntryNo = LastNo + 1;

			return EntryNo;
		}
		[HttpGet]
		//[QkAuthorize(Roles = "Dev, Delete AssetToInventory")]
		public ActionResult Delete(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			BillOfQty MastObj = db.BillOfQyts.Find(id);

			if (MastObj == null)
			{
				return NotFound();
			}
			else
			{
				return PartialView(MastObj);
			}
		}

		//POST Delete
		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteAction(long id)
		{
			bool stat = false;
			string msg;

			var chk = DeleteEntry(id);

			if (chk == true)
			{
				stat = true;
				msg = "Successfully deleted Bill Of Quantity.";
			}
			else
			{
				stat = false;
				msg = "Looks like something went wrong. Please check your form.";
			}

			return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
		}

		//Function to delete all the transactions
		[HttpPost]
		public ActionResult DeleteAll(long[] bill)
		{
			Int32 count = 0;

			foreach (var arr in bill)
			{
				var chk = (DeleteEntry(arr) == true) ? count++ : count;
			}

			Success("Deleted " + count + " Bill Of Quantity.", true);
			return RedirectToAction("Index", "Boq");
		}

		//Function To Delete Each Entry
		private Boolean DeleteEntry(long BoqId)
		{
			

			
			BillOfQty MastObj = db.BillOfQyts.Find(BoqId);
			db.BillOfQyts.Remove(MastObj);

			db.SaveChanges();


			return true;
		}




		//		serialisedJson = db.BillOfQyts.Where(p => p.BoqName.ToLower().Contains(q.ToLower()) || p.BoqName.Contains(q))
		//						  .Select(b => new SelectFormat
		//							  text = b.BoqName, //each json object will have 
		//							  id = b.BoqId
		//						  })
		//		serialisedJson = db.BillOfQyts.Select(b => new SelectFormat
		//			text = b.BoqName, //each json object will have 
		//			id = b.BoqId

		//	}//
		//////public ActionResult GetBOMById(int bomID)
		//////{
		//////	var bs = db.BillOfMaterials.Where(c => c.BOMId == bomID).Select(b => new
		//////	{
		//////		b.BOMName,
		//////		b.BOMId,
		//////		b.Expense,
		//////		b.Quantity,
		//////		b.Status,
		//////		b.Unit,
				
		//////	}).FirstOrDefault();
		//////	return Json(bs);

		//////}
		//[HttpPost]
		//				  where a.ItemId == item
		//					  Quantity = a.Quantity,
		//					  Price = b.SellingPrice
		//	return new QuickSoft.Models.LegacyJsonResult { Data = new { items = bitems } };

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
