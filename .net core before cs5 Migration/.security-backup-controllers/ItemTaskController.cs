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
    public class ItemTaskController :BaseController
    {

        ApplicationDbContext db;
        Common com;
        public ItemTaskController()
        {
            db = new ApplicationDbContext();
            com = new Common();
        }
		// GET: ItemTask
		[RedirectingAction]
		[Authorize(Roles = "Dev,My ProTask")]
		[HttpGet]
		public ActionResult Create(long id)
		{


			
			ViewBag.Remind = id;


			ViewBag.Item = QkSelect.List(
						   new List<SelectListItem>
						   {
									new SelectListItem { Selected = false, Text = "", Value = "0"},
						   }, "Value", "Text", 1);

			
			var taskname = db.ProTasks
			 .Select(s => new
			 {
				 ID = s.ProTaskId,
				 Name = s.TaskName
			 }).Distinct()
			 .ToList().OrderBy(a => a.Name);
			

			var mcs = db.MCs.Select(s => new
			{
				McId = s.MCId,
				Name = s.MCName
			}).ToList();
			

			var UserId = User.Identity.GetUserId();
			
			
			
			ItemTaskViewModel vmodel = new ItemTaskViewModel();
			vmodel.TaskDate = (System.DateTime.Now).ToString("dd-MM-yyyy");
			vmodel.taskid = id;
			ViewBag.TaskName = QkSelect.List(taskname.Where(o=>o.ID==id).Take(1).ToList(), "ID", "Name");
			ViewBag.MaterialId = db.MCs.Where(o => o.AssignedUser == UserId).Select(o => o.MCId).FirstOrDefault(); ;
			ViewBag.MCbag = QkSelect.List(mcs.Where(o=>o.McId==ViewBag.MaterialId).Take(1).ToList(), "McId", "Name");
			return View(vmodel);
		}
		private long GetVTNo()
		{
			Int64 SENo = 0;
			Int32 number = db.CodePrefixs.Where(a => a.section == "StockTransfer").Select(a => a.number).FirstOrDefault();
			if ((db.StockTransfers.Select(p => p.STNo).AsEnumerable().DefaultIfEmpty(0).Max()) == 0)
			{
				if (number == 0)
				{
					SENo = 1;
				}
				else
				{
					SENo = number;
				}
			}
			else
			{
				SENo = db.StockTransfers.Max(p => p.STNo + 1);
			}
			return SENo;
		}














		// [QkAuthorize(Roles = "Dev,Delete Category")]
		public ActionResult Delete(long? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			itemtasklist itemtaskli = db.itemtasklist.Find(id);
			if (itemtaskli == null)
			{
				return NotFound();
			}
			return PartialView(itemtaskli);
		}

		public ActionResult Deletenew(long? id,long? protraskid)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			List<itemtasklist> itemtaskli = db.itemtasklist.Where(o => o.itemid == id&&o.protaskid==protraskid).ToList();
			for(int i= 0;i < itemtaskli.Count(); i++)
            {
				var userids = itemtaskli[i].userid;
				itemtaskli[i].userid = db.Employees.Where(o => o.UserId == userids).Select(o => o.FirstName + " " + o.LastName).FirstOrDefault();

			}
			if (itemtaskli.Count()<1)
			{
				return NotFound();
			}
			return PartialView(itemtaskli);
		}
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		//  [QkAuthorize(Roles = "Dev,Delete Category")]
		public JsonResult DeleteConfirmed(long id)
		{
			bool stat = false;
			string msg;

			stat = DeleteFn(id);
			if (stat == true)
			{
				msg = "Success .";
				Success(msg, true);
			}
			else
			{
				msg = "Item Already Invoiced.So delete from invoice";
				Danger(msg, true);
			}
			
			return new QuickSoft.Models.LegacyJsonResult { Data = new { status = stat, message = msg } };
		}
		public bool DeleteFn(long id)
		{

			var exists = db.itemtasklist.Find(id);
			if (exists!=null)
			{
				var UserId = User.Identity.GetUserId();
				itemtasklist a = db.itemtasklist.Find(id);
                var tempp = db.StockTransferItems.Where(b => b.Id == a.stocktransferitid).FirstOrDefault();

                long mcfrom = a.mcfrom;

				var today = Convert.ToDateTime(System.DateTime.Now);



				var f = db.ItemTask.Where(x => x.itemtasklistid == id).FirstOrDefault();
				if (f.invoiced == 2)
				{
					var salesitem=db.SEItemss.Where(o => o.Item == f.ItemId&&o.SalesEntry==f.seitemid).FirstOrDefault();
					if (salesitem != null)
					return false;
				}

				db.ItemTask.Remove(db.ItemTask.Find(f.TaskId));
				db.SaveChanges();
				db.itemtasklist.Remove(db.itemtasklist.Find(id));
				db.SaveChanges();
				db.StockTransferItems.RemoveRange(db.StockTransferItems.Where(o => o.Id == a.stocktransferitid));
				db.SaveChanges();
                com.addlog(LogTypes.Deleted, UserId, "ProTask", "ProTasks", findip(), tempp.StockTransferId, "Successfully Deleted Protask Item");

                return true;
			}
			else
            {
				return true;
            }

		}

		[HttpPost]
		[RedirectingAction]
		[Authorize(Roles = "Dev,My ProTask")]
		public ActionResult Create(ItemTaskViewModel vmodel,long id)
		{
			bool stat = false;
			string msg;

			long protaskid = id;
			var UserId = User.Identity.GetUserId();
			var today = Convert.ToDateTime(System.DateTime.Now);
			DateTime? Date = null;
			if (vmodel.TaskDate != null)
			{
				Date = DateTime.Parse(vmodel.TaskDate, new CultureInfo("en-GB"));
			}
			db.ItemTaskMasters.RemoveRange(db.ItemTaskMasters.Where(o => o.TaskId == protaskid));
			db.SaveChanges();
			ItemTaskMaster bom = new ItemTaskMaster
			{
				CreatedBy = UserId,
				TaskDate = today,
				TaskName = db.ProTasks.Where(p => p.ProTaskId == id).Select(p => p.TaskName).FirstOrDefault(),
				TaskId = protaskid,
				McId = (long)vmodel.MaterialId,
				

			};
			

			db.ItemTaskMasters.Add(bom);
			db.SaveChanges();


			Int64 bomId = bom.TaskMasterId;

			ItemTasks bomItem = new ItemTasks();
			StockTransfer sl = new StockTransfer();

			sl.STNo = GetVTNo();
			sl.Voucher = "Field-Task " + db.ProTasks.Where(p => p.ProTaskId == id).Select(p => p.TaskCode).FirstOrDefault();
			sl.Date = today;
			sl.MCFrom = (long)vmodel.MaterialId;
			sl.MCTo = db.MCs.Where(o=>o.MCName== "TASK CENTER").Select(o=>o.MCId).SingleOrDefault();
			sl.Remarks = "Item used in field service";
			sl.TotalAmount = 0;
			sl.CreatedDate = today;

			sl.CreatedBy = UserId;
			sl.Status = Status.active;
			sl.editable = choice.Yes;
			sl.Branch = 1;

			string str = "";
			StockType Stype = StockType.StockTransfer;
			sl.StockType = Stype;
			sl.Ref1 = "";
			sl.Ref2 = "";
			sl.Ref3 = "";
			sl.Ref4 = "";
			sl.Ref5 = "";

			db.StockTransfers.Add(sl);
			db.SaveChanges();

			Int64 STId = sl.Id;
			foreach (var arr in vmodel.bomitems)
			{
				if (arr.ItemId != 0&& arr.Quantity>0)
				{

					
					StockTransferItem mt = new StockTransferItem();
					decimal confactor = 1;
					var itt = db.Items.Where(o => o.ItemID == arr.ItemId).FirstOrDefault();
					if (itt.SubUnitId == arr.Unit && itt.ConFactor > 1)
						confactor = itt.ConFactor;

					mt.StockTransferId = STId;
					mt.Item = arr.ItemId;
					mt.Unit = arr.Unit;
					mt.Quantity = arr.Quantity;
					mt.Price = db.Items.Where(o=>o.ItemID==arr.ItemId).Select(o=>o.PurchasePrice).SingleOrDefault()/confactor;
					mt.Amount = 0;
					db.StockTransferItems.Add(mt);
					db.SaveChanges();
					itemtasklist imt = new itemtasklist
					{
						itemid = arr.ItemId,
						mcfrom = (long)vmodel.MaterialId,
						protaskid = protaskid,
						userid = UserId,
						qty = arr.Quantity,
						unit=arr.Unit,
						stocktransferitid=mt.Id  /*STId */

                    };
					db.itemtasklist.Add(imt);
					db.SaveChanges();
					bomItem.ItemId = arr.ItemId;
					bomItem.Quantity = arr.Quantity;
					bomItem.Unit = arr.Unit;
					bomItem.protaskid = protaskid;
					bomItem.itemtasklistid = imt.fieldmcid;
					db.ItemTask.Add(bomItem);
					db.SaveChanges();
				}
					


					
					
					
				
			}
			com.updateprotaskdate(protaskid);
			com.addlog(LogTypes.Created, UserId, "ProTask", "ProTasks", findip(), STId, "Successfully Submitted TaskItems");
		
			msg = "Successfully Added TaskItems .";
			stat = true;
			return RedirectToAction("MyTask", "ProTask");
			

		}




		[HttpPost]
		//   [QkAuthorize(Roles = "Dev,Category")]
		public ActionResult Gettaskitems(long protaskid)
		{

		
			var taskcode = db.ProTasks.Where(o => o.ProTaskId == protaskid).Select(o => o.TaskCode).FirstOrDefault();
			var voucherno = "Field-Task " + taskcode;
			long[] stocktrasferid = db.StockTransfers.Where(o => o.Voucher == voucherno).Select(o => o.Id).ToList().ToArray();

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
			var uEdit = User.IsInRole("Edit Category");
			var uDelete = User.IsInRole("Delete Category");

            // dc.ChangeTracker.LazyLoadingEnabled = false; // if your table is relational, contain foreign key
            var v = (from a in db.itemtasklist
                     join i in db.Items on a.itemid equals i.ItemID
                     join b in db.ItemUnits on i.ItemUnitID equals b.ItemUnitID into temp
                     from b in temp.DefaultIfEmpty()
                     join c in db.ItemUnits on i.SubUnitId equals c.ItemUnitID into temp2
                     from c in temp2.DefaultIfEmpty()
					 join d in db.ItemUnits on a.unit equals d.ItemUnitID into orgunit
					 from d in orgunit.DefaultIfEmpty()
                     where (a.protaskid == protaskid)
                     select new
                     {
                         a.qty,
                         ItemName = i.ItemCode + " " + i.ItemName,
                         UnitPrice = i.SellingPrice,
                         a.fieldmcid,
                         crntUnit = a.unit,
						 unit=d.ItemUnitName,
                         primaryUnit=i.ItemUnitID,
                         secondaryUnit=i.SubUnitId,
                         ConFactor=i.ConFactor,
                         PriUnitName=b.ItemUnitName,
                         SubUnitName=c.ItemUnitName
                     });

			var data = (from a in db.ItemTask
						join z in db.ItemTaskMasters on a.protaskid.ToString() equals z.TaskId.ToString()
						join b in db.Items on a.ItemId equals b.ItemID
						join c in db.ItemUnits on b.ItemUnitID equals c.ItemUnitID into primary
						from c in primary.DefaultIfEmpty()
						join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
						from d in second.DefaultIfEmpty()
						join e in db.ItemCategorys on b.ItemCategoryID equals e.ItemCategoryID into cat
						from e in cat.DefaultIfEmpty()
						join f in db.Taxs on b.TaxID equals f.TaxID into taxss
						from f in taxss.DefaultIfEmpty()
						where (a.protaskid == protaskid)

						select new
						{
							a.ItemId,
							
							a.Quantity,
							ItemName = b.ItemCode + " " + b.ItemName,
							UnitPrice = b.SellingPrice,
					
							crntUnit = a.Unit,
							unit = d.ItemUnitName,
							primaryUnit = b.ItemUnitID,
							secondaryUnit = b.SubUnitId,
							ConFactor = b.ConFactor,
							PriUnitName = d.ItemUnitName,
							SubUnitName = c.ItemUnitName

						}).AsEnumerable().Select(o => new
						{
							o.ItemId,
							o.Quantity,
							o.ItemName,
							o.UnitPrice,
						
							o.crntUnit,
							o.unit,
							o.primaryUnit,
							o.secondaryUnit,
							o.ConFactor,
							o.PriUnitName,
							o.SubUnitName,


						}).GroupBy(p => new { p.ItemId, p.crntUnit }, (key, g) => new
						{
							ItemId = key.ItemId,
							qty = g.Sum(o => o.Quantity),
							crntUnit=key.crntUnit,
							protaskid,
							ItemName =g.FirstOrDefault().ItemName,
							UnitPrice = (from gg in db.itemtasklist
										 
										 join h in db.StockTransferItems on gg.itemid equals h.Item into stkit
										 from h in stkit.DefaultIfEmpty()
										 join i in db.StockTransfers on h.StockTransferId equals i.Id into sttr
										 from i in sttr.DefaultIfEmpty()
										 join oo in db.Items on h.Item equals oo.ItemID into oitem
										 from oo in oitem.DefaultIfEmpty()
										 where gg.protaskid==protaskid &&  stocktrasferid.Contains(i.Id)
										 select new
										 {

											 // Price =(decimal)0,// (gg.unit == g.FirstOrDefault().secondaryUnit && h.Price < 5 && g.FirstOrDefault().ConFactor > 1) ? h.Price * g.FirstOrDefault().ConFactor :h.Price,
											 // h.Item

											 Price =h.Price,// (h.Unit == oo.SubUnitId && h.Price < 1.1 && oo.ConFactor > 1) ? h.Price * oo.ConFactor : h.Price,

											 h.Item,
											 h.Unit
										 }).Where(o => o.Item == key.ItemId).Select(o => o.Price).Average(),

						
							unit=g.FirstOrDefault().unit,
							primaryUnit=g.FirstOrDefault().primaryUnit,
							secondaryUnit= g.FirstOrDefault().secondaryUnit,
							ConFactor= g.FirstOrDefault().ConFactor,
						PriUnitName= g.FirstOrDefault().PriUnitName,
							SubUnitName= g.FirstOrDefault().SubUnitName,
							


						}).Select(o=>new
						{
							o.ItemId,
							o.qty,
							o.crntUnit,
							o.protaskid,
							o.ItemName,
							UnitPrice=o.UnitPrice,// (o.crntUnit ==o.secondaryUnit && o.UnitPrice < 5 && o.ConFactor > 1) ? o.UnitPrice : o.UnitPrice / o.ConFactor,

							o.unit,
							o.primaryUnit,
							o.secondaryUnit,
							o.ConFactor,
							o.PriUnitName,
							o.SubUnitName,



						});



		//	Price = (key.crntUnit == g.FirstOrDefault().secondaryUnit && h.Price < 5 && g.FirstOrDefault().ConFactor > 1) ? h.Price * g.FirstOrDefault().ConFactor : h.Price,
											


			//search
			if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
			{
				// Apply search   
			}

			//SORT
			if (sortColumn != "" && !(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
			{
				v = v.AsQueryable().OrderBy(sortColumn + " " + sortColumnDir);

			}

			recordsTotal = data.Count();
			var datas = data.ToList();
			return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = datas });

		}









		public ActionResult Index(long id)

		{
			ItemTaskViewModel vmodel = new ItemTaskViewModel();
			return View(vmodel);
		}
		[HttpPost]
		//  [QkAuthorize(Roles = "Dev,ItemTask List")]
		public JsonResult GetData()
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
			var uEdit = User.IsInRole("Edit ItemTask");

			var v = (from a in db.ItemTask
					 join b in db.Items on a.ItemId equals b.ItemID into usr

					 from b in usr.DefaultIfEmpty()
					 join d in db.ItemUnits on b.SubUnitId equals d.ItemUnitID into second
					 from d in second.DefaultIfEmpty()

					

					 select new
					 {
						 id = a.TaskId,
						 b.ItemName,
						 d.ItemUnitName,
						 a.Quantity,
						 


						 Dev = uDev,
						 Edit = uEdit,
					 });
			//search
			if (!string.IsNullOrEmpty(search) && !string.IsNullOrWhiteSpace(search))
			{
				// Apply search   
				v = v.Where(p => p.ItemName.ToString().ToLower().Contains(search.ToLower()) ||
								 p.Quantity.ToString().ToLower().Contains(search.ToLower()));
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


	}
}
